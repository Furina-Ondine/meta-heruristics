using System.Diagnostics;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Experiments.Configuration;
using Anastasya.Metaheuristics.Experiments.Results;

namespace Anastasya.Metaheuristics.Experiments.Execution;

/// <summary>
/// 规划并有界并发执行多个实验 Case。
/// </summary>
public static class ExperimentRunner
{
    /// <summary>
    /// 执行 Experiment，并在取消时返回已经完成的部分结果。
    /// </summary>
    /// <param name="experiment">包含至少一个 Case 的实验定义。</param>
    /// <param name="options">可选全局并发度和共享 seed 配置。</param>
    /// <param name="cancellationToken">用于停止投放新 Group 并协作取消运行中 Group 的令牌。</param>
    /// <returns>完整结果，或状态为取消且包含已完成 run 的部分结果。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="experiment"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException">全局并发度不是正数。</exception>
    /// <exception cref="ArgumentException">显式 seed 数量不足。</exception>
    public static async Task<ExperimentResult> RunAsync(
        ExperimentDefinition experiment,
        ExperimentExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(experiment);
        options ??= new ExperimentExecutionOptions();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.GlobalMaxConcurrency);

        var maximumRepetitions = experiment.Cases.Max(static experimentCase => experimentCase.Repetitions);
        var seeds = ResolveSeeds(options, maximumRepetitions);
        var builders = experiment.Cases
            .Select(experimentCase => new CaseResultBuilder(experimentCase, seeds))
            .ToArray();
        var executionState = new ExperimentExecutionState();
        var stopwatch = Stopwatch.StartNew();

        if (!cancellationToken.IsCancellationRequested)
        {
            var groupCount = experiment.Cases.Sum(static experimentCase => experimentCase.RunGroupCount);
            var workerCount = Math.Min(options.GlobalMaxConcurrency, groupCount);
            using var plans = CreatePlans(experiment.Cases, seeds).GetEnumerator();
            var planGate = new object();
            var workers = new Task[workerCount];
            for (var workerIndex = 0; workerIndex < workers.Length; workerIndex++)
            {
                workers[workerIndex] = Task.Run(
                    () => RunWorker(plans, planGate, builders, executionState, cancellationToken),
                    CancellationToken.None);
            }

            await Task.WhenAll(workers).ConfigureAwait(false);
        }

        stopwatch.Stop();
        var caseResults = builders.Select(static builder => builder.Build()).ToArray();
        var counts = ExperimentStatisticsCalculator.CreateCounts(
            caseResults.SelectMany(static caseResult => caseResult.Runs));
        var status = ResolveExperimentStatus(counts, executionState.StartedGroupCount);
        return new ExperimentResult(status, caseResults, seeds, counts, stopwatch.Elapsed);
    }

    private static void RunWorker(
        IEnumerator<RunGroupPlan> plans,
        object planGate,
        IReadOnlyList<CaseResultBuilder> builders,
        ExperimentExecutionState executionState,
        CancellationToken cancellationToken)
    {
        while (TryTakePlan(plans, planGate, cancellationToken, out var plan))
        {
            ExecutePlan(plan!, builders[plan!.CaseIndex], executionState, cancellationToken);
        }
    }

    private static bool TryTakePlan(
        IEnumerator<RunGroupPlan> plans,
        object planGate,
        CancellationToken cancellationToken,
        out RunGroupPlan? plan)
    {
        lock (planGate)
        {
            if (cancellationToken.IsCancellationRequested || !plans.MoveNext())
            {
                plan = null;
                return false;
            }

            plan = plans.Current;
            return true;
        }
    }

    private static void ExecutePlan(
        RunGroupPlan plan,
        CaseResultBuilder builder,
        ExperimentExecutionState executionState,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        executionState.MarkGroupStarted();
        builder.MarkGroupStarted();
        var nextRunOffset = 0;
        ExperimentGroupSetup setup;
        try
        {
            setup = CreateSetup(plan, builder, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception exception)
        {
            builder.SetFailed(plan.RepetitionIndices, exception);
            return;
        }

        while (nextRunOffset < plan.RepetitionIndices.Length)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var repetitionIndex = plan.RepetitionIndices[nextRunOffset];
            var seed = plan.Seeds[nextRunOffset];
            try
            {
                var summary = OptimizationRunner.Execute(
                    setup.Problem,
                    setup.Optimizer,
                    setup.RunOptions,
                    seed,
                    cancellationToken);
                builder.SetSucceeded(repetitionIndex, summary, setup.Optimizer.BestPosition);
                nextRunOffset++;
            }
            catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
            {
                builder.SetCanceled(repetitionIndex, exception);
                return;
            }
            catch (Exception exception)
            {
                builder.SetFailed(repetitionIndex, exception);
                nextRunOffset++;
                if (nextRunOffset >= plan.RepetitionIndices.Length
                    || cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {
                    // 异常可能留下不完整状态；丢弃旧引用并为剩余 run 创建全新 Group 环境。
                    setup = CreateSetup(plan, builder, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception rebuildException)
                {
                    builder.SetFailed(plan.RepetitionIndices.AsSpan(nextRunOffset), rebuildException);
                    return;
                }
            }
        }
    }

    private static ExperimentGroupSetup CreateSetup(
        RunGroupPlan plan,
        CaseResultBuilder builder,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new ExperimentGroupContext(
            plan.Case.Id,
            plan.GroupIndex,
            plan.RepetitionIndices,
            plan.Seeds,
            cancellationToken);
        var setup = plan.Case.CreateGroup(context);
        ArgumentNullException.ThrowIfNull(setup.RunOptions.Trace);
        builder.EnsureDimension(setup.Problem.Dimension);
        return setup;
    }

    private static IEnumerable<RunGroupPlan> CreatePlans(IReadOnlyList<ExperimentCase> cases, int[] seeds)
    {
        var maximumGroupCount = cases.Max(static experimentCase => experimentCase.RunGroupCount);
        for (var groupIndex = 0; groupIndex < maximumGroupCount; groupIndex++)
        {
            for (var caseIndex = 0; caseIndex < cases.Count; caseIndex++)
            {
                var experimentCase = cases[caseIndex];
                if (groupIndex >= experimentCase.RunGroupCount)
                {
                    continue;
                }

                var baseSize = experimentCase.Repetitions / experimentCase.RunGroupCount;
                var remainder = experimentCase.Repetitions % experimentCase.RunGroupCount;
                var size = baseSize + (groupIndex < remainder ? 1 : 0);
                var start = (groupIndex * baseSize) + Math.Min(groupIndex, remainder);
                var repetitionIndices = new int[size];
                var groupSeeds = new int[size];
                for (var offset = 0; offset < size; offset++)
                {
                    var repetitionIndex = start + offset;
                    repetitionIndices[offset] = repetitionIndex;
                    groupSeeds[offset] = seeds[repetitionIndex];
                }

                yield return new RunGroupPlan(caseIndex, experimentCase, groupIndex, repetitionIndices, groupSeeds);
            }
        }
    }

    private static int[] ResolveSeeds(ExperimentExecutionOptions options, int requiredCount)
    {
        if (options.Seeds is not null)
        {
            if (options.Seeds.Count < requiredCount)
            {
                throw new ArgumentException(
                    "The explicit seed list must cover the largest case repetition count.",
                    nameof(options));
            }

            return options.Seeds.Take(requiredCount).ToArray();
        }

        var seeds = new int[requiredCount];
        for (var repetitionIndex = 0; repetitionIndex < seeds.Length; repetitionIndex++)
        {
            seeds[repetitionIndex] = DeriveSeed(options.BaseSeed, repetitionIndex);
        }

        return seeds;
    }

    private static int DeriveSeed(int baseSeed, int repetitionIndex)
    {
        // 这些可逆的 32 位变换构成一个置换，因此不同下标不会因混合而产生 seed 碰撞。
        var value = unchecked((uint)baseSeed + (uint)repetitionIndex);
        value ^= value >> 16;
        value *= 0x7FEB352Du;
        value ^= value >> 15;
        value *= 0x846CA68Bu;
        value ^= value >> 16;
        return unchecked((int)value);
    }

    private static ExperimentExecutionStatus ResolveExperimentStatus(ExperimentRunCounts counts, int startedGroupCount)
    {
        if (startedGroupCount == 0)
        {
            return ExperimentExecutionStatus.NotStarted;
        }

        if (counts.Canceled > 0 || counts.NotStarted > 0)
        {
            return ExperimentExecutionStatus.Canceled;
        }

        return counts.Failed > 0
            ? ExperimentExecutionStatus.Failed
            : ExperimentExecutionStatus.Succeeded;
    }

    private sealed class CaseResultBuilder
    {
        private readonly ExperimentCase _case;
        private readonly int[] _groupIndices;
        private readonly object _matrixGate = new();
        private readonly ExperimentRunResult?[] _runs;
        private readonly IReadOnlyList<int> _seeds;
        private BestPositionMatrix? _bestPositions;
        private int _startedGroupCount;

        public CaseResultBuilder(ExperimentCase experimentCase, IReadOnlyList<int> seeds)
        {
            _case = experimentCase;
            _seeds = seeds;
            _runs = new ExperimentRunResult?[experimentCase.Repetitions];
            _groupIndices = CreateGroupIndices(experimentCase);
        }

        public void MarkGroupStarted()
        {
            Interlocked.Increment(ref _startedGroupCount);
        }

        public void EnsureDimension(int dimension)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimension);
            lock (_matrixGate)
            {
                if (_bestPositions is null)
                {
                    _bestPositions = new BestPositionMatrix(_case.Repetitions, dimension);
                    return;
                }

                if (_bestPositions.Dimension != dimension)
                {
                    throw new InvalidOperationException(
                        "All run groups in an experiment case must use the same problem dimension.");
                }
            }
        }

        public void SetSucceeded(
            int repetitionIndex,
            OptimizationRunSummary summary,
            ReadOnlySpan<double> bestPosition)
        {
            var matrix = _bestPositions
                ?? throw new InvalidOperationException("The case position matrix has not been initialized.");
            matrix.SetPosition(repetitionIndex, bestPosition);
            _runs[repetitionIndex] = CreateRunResult(
                repetitionIndex,
                ExperimentExecutionStatus.Succeeded,
                summary,
                null);
        }

        public void SetFailed(int repetitionIndex, Exception exception)
        {
            _runs[repetitionIndex] = CreateRunResult(
                repetitionIndex,
                ExperimentExecutionStatus.Failed,
                null,
                exception);
        }

        public void SetFailed(ReadOnlySpan<int> repetitionIndices, Exception exception)
        {
            foreach (var repetitionIndex in repetitionIndices)
            {
                SetFailed(repetitionIndex, exception);
            }
        }

        public void SetCanceled(int repetitionIndex, OperationCanceledException exception)
        {
            _runs[repetitionIndex] = CreateRunResult(
                repetitionIndex,
                ExperimentExecutionStatus.Canceled,
                null,
                exception);
        }

        public ExperimentCaseResult Build()
        {
            var runs = new ExperimentRunResult[_runs.Length];
            for (var repetitionIndex = 0; repetitionIndex < runs.Length; repetitionIndex++)
            {
                runs[repetitionIndex] = _runs[repetitionIndex]
                    ?? CreateRunResult(repetitionIndex, ExperimentExecutionStatus.NotStarted, null, null);
            }

            var statistics = ExperimentStatisticsCalculator.Create(runs);
            var status = ResolveCaseStatus(statistics.Counts, Volatile.Read(ref _startedGroupCount));
            return new ExperimentCaseResult(_case.Id, status, runs, _bestPositions, statistics);
        }

        private ExperimentRunResult CreateRunResult(
            int repetitionIndex,
            ExperimentExecutionStatus status,
            OptimizationRunSummary? summary,
            Exception? exception)
        {
            return new ExperimentRunResult(
                _case.Id,
                _groupIndices[repetitionIndex],
                repetitionIndex,
                _seeds[repetitionIndex],
                status,
                summary,
                exception);
        }

        private static ExperimentExecutionStatus ResolveCaseStatus(ExperimentRunCounts counts, int startedGroupCount)
        {
            if (startedGroupCount == 0)
            {
                return ExperimentExecutionStatus.NotStarted;
            }

            if (counts.Canceled > 0 || counts.NotStarted > 0)
            {
                return ExperimentExecutionStatus.Canceled;
            }

            return counts.Failed > 0
                ? ExperimentExecutionStatus.Failed
                : ExperimentExecutionStatus.Succeeded;
        }

        private static int[] CreateGroupIndices(ExperimentCase experimentCase)
        {
            var result = new int[experimentCase.Repetitions];
            var baseSize = experimentCase.Repetitions / experimentCase.RunGroupCount;
            var remainder = experimentCase.Repetitions % experimentCase.RunGroupCount;
            var start = 0;
            for (var groupIndex = 0; groupIndex < experimentCase.RunGroupCount; groupIndex++)
            {
                var size = baseSize + (groupIndex < remainder ? 1 : 0);
                Array.Fill(result, groupIndex, start, size);
                start += size;
            }

            return result;
        }
    }
}
