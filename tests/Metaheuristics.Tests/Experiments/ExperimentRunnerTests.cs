using System.Collections.Concurrent;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;
using Anastasya.Metaheuristics.Experiments.Configuration;
using Anastasya.Metaheuristics.Experiments.Execution;
using Anastasya.Metaheuristics.Experiments.Results;

namespace Anastasya.Metaheuristics.Tests.Experiments;

/// <summary>
/// 验证 Experiment 的规划、并发、复用、容错、取消、结果和统计契约。
/// </summary>
public sealed class ExperimentRunnerTests
{
    /// <summary>
    /// 验证 Case 会均衡拆分、不同 Case 的 Group 会交错，并共享相同 Repetition seed。
    /// </summary>
    [Xunit.Fact]
    public async Task RunAsyncPlansGroupsRoundRobinAndReusesEachOptimizer()
    {
        var creationOrder = new ConcurrentQueue<string>();
        var optimizers = new ConcurrentBag<SeedValueOptimizer>();
        var firstCase = CreateRecordingCase("first", 5, 2, creationOrder, optimizers);
        var secondCase = CreateRecordingCase("second", 5, 2, creationOrder, optimizers);
        var experiment = new ExperimentDefinition([firstCase, secondCase]);

        var result = await ExperimentRunner.RunAsync(
            experiment,
            new ExperimentExecutionOptions
            {
                GlobalMaxConcurrency = 1,
                Seeds = [11, 22, 33, 44, 55],
            },
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(ExperimentExecutionStatus.Succeeded, result.Status);
        Xunit.Assert.Equal(["first:0:0,1,2", "second:0:0,1,2", "first:1:3,4", "second:1:3,4"], creationOrder);
        Xunit.Assert.Equal([2, 2, 3, 3], optimizers.Select(static optimizer => optimizer.ResetCount).Order());
        Xunit.Assert.Equal(result.Cases[0].Runs.Select(static run => run.Seed), result.Cases[1].Runs.Select(static run => run.Seed));
        Xunit.Assert.Equal(11, result.Cases[0].BestPositions![0, 0]);
        Xunit.Assert.Equal(55, result.Cases[1].BestPositions![4, 0]);
    }

    /// <summary>
    /// 验证固定数量的 Worker Task 会执行真实并发，且不会超过全局上限。
    /// </summary>
    [Xunit.Fact]
    public async Task RunAsyncHonorsTheGlobalConcurrencyLimit()
    {
        using var probe = new ConcurrencyProbe(requiredConcurrency: 2);
        var experimentCase = new ExperimentCase<ConcurrencyProbe>(
            "parallel",
            probe,
            repetitions: 4,
            static (configuration, _) => new ExperimentGroupSetup(
                CreateProblem(),
                new ProbedOptimizer(configuration),
                StopImmediately()),
            runGroupCount: 4);

        var result = await ExperimentRunner.RunAsync(
            new ExperimentDefinition([experimentCase]),
            new ExperimentExecutionOptions { GlobalMaxConcurrency = 2 },
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(ExperimentExecutionStatus.Succeeded, result.Status);
        Xunit.Assert.Equal(2, probe.MaximumConcurrency);
    }

    /// <summary>
    /// 验证 run 异常后当前 Optimizer 不再复用，剩余 run 使用 Factory 重建的实例继续。
    /// </summary>
    [Xunit.Fact]
    public async Task RunAsyncRebuildsTheGroupAfterARunFailure()
    {
        var factoryCalls = 0;
        var experimentCase = new ExperimentCase<object>(
            "rebuild",
            new object(),
            repetitions: 3,
            (_, _) => new ExperimentGroupSetup(
                CreateProblem(),
                new AdvanceFailureOptimizer(Interlocked.Increment(ref factoryCalls) == 1),
                new OptimizationRunOptions(StoppingConditions.MaxIterations(1))));

        var result = await ExperimentRunner.RunAsync(
            new ExperimentDefinition([experimentCase]),
            new ExperimentExecutionOptions { GlobalMaxConcurrency = 1, Seeds = [1, 2, 3] },
            Xunit.TestContext.Current.CancellationToken);

        var caseResult = Xunit.Assert.Single(result.Cases);
        Xunit.Assert.Equal(ExperimentExecutionStatus.Failed, result.Status);
        Xunit.Assert.Equal(2, factoryCalls);
        Xunit.Assert.Equal(
            [ExperimentExecutionStatus.Failed, ExperimentExecutionStatus.Succeeded, ExperimentExecutionStatus.Succeeded],
            caseResult.Runs.Select(static run => run.Status));
        Xunit.Assert.False(caseResult.BestPositions!.HasPosition(0));
        Xunit.Assert.True(caseResult.BestPositions.HasPosition(1));
        Xunit.Assert.Equal(2, caseResult.Statistics.Counts.Succeeded);
        Xunit.Assert.Equal(1, caseResult.Statistics.Counts.Failed);
    }

    /// <summary>
    /// 验证 Group Factory 失败会把该 Group 的全部运行标记为失败，并继续其他 Group。
    /// </summary>
    [Xunit.Fact]
    public async Task RunAsyncMarksAllRunsFailedWhenGroupCreationFails()
    {
        var factoryCalls = 0;
        var experimentCase = new ExperimentCase<object>(
            "factory-failure",
            new object(),
            repetitions: 4,
            (_, _) =>
            {
                Interlocked.Increment(ref factoryCalls);
                throw new InvalidOperationException("factory failed");
            },
            runGroupCount: 2);

        var result = await ExperimentRunner.RunAsync(
            new ExperimentDefinition([experimentCase]),
            new ExperimentExecutionOptions { GlobalMaxConcurrency = 2 },
            Xunit.TestContext.Current.CancellationToken);

        var caseResult = Xunit.Assert.Single(result.Cases);
        Xunit.Assert.Equal(ExperimentExecutionStatus.Failed, result.Status);
        Xunit.Assert.Equal(2, factoryCalls);
        Xunit.Assert.All(caseResult.Runs, static run => Xunit.Assert.Equal(ExperimentExecutionStatus.Failed, run.Status));
        Xunit.Assert.Null(caseResult.BestPositions);
        Xunit.Assert.Null(caseResult.Statistics.BestObjective);
    }

    /// <summary>
    /// 验证取消会保留已成功结果，区分运行中取消与尚未开始的 Repetition。
    /// </summary>
    [Xunit.Fact]
    public async Task RunAsyncReturnsPartialResultsForCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var experimentCase = new ExperimentCase<CancellationTokenSource>(
            "cancel",
            cancellation,
            repetitions: 3,
            static (source, _) => new ExperimentGroupSetup(
                CreateProblem(),
                new CancelOnSecondResetOptimizer(source),
                StopImmediately()));

        var result = await ExperimentRunner.RunAsync(
            new ExperimentDefinition([experimentCase]),
            new ExperimentExecutionOptions { GlobalMaxConcurrency = 1 },
            cancellation.Token);

        var caseResult = Xunit.Assert.Single(result.Cases);
        Xunit.Assert.Equal(ExperimentExecutionStatus.Canceled, result.Status);
        Xunit.Assert.Equal(
            [ExperimentExecutionStatus.Succeeded, ExperimentExecutionStatus.Canceled, ExperimentExecutionStatus.NotStarted],
            caseResult.Runs.Select(static run => run.Status));
        Xunit.Assert.Equal(1, result.Counts.Succeeded);
        Xunit.Assert.Equal(1, result.Counts.Canceled);
        Xunit.Assert.Equal(1, result.Counts.NotStarted);
    }

    /// <summary>
    /// 验证执行开始前已经取消时，Experiment 和所有 run 都保持 NotStarted。
    /// </summary>
    [Xunit.Fact]
    public async Task RunAsyncReturnsNotStartedWhenAlreadyCanceled()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var experimentCase = CreateRecordingCase(
            "not-started",
            2,
            1,
            new ConcurrentQueue<string>(),
            new ConcurrentBag<SeedValueOptimizer>());

        var result = await ExperimentRunner.RunAsync(
            new ExperimentDefinition([experimentCase]),
            cancellationToken: cancellation.Token);

        Xunit.Assert.Equal(ExperimentExecutionStatus.NotStarted, result.Status);
        Xunit.Assert.All(
            result.Cases[0].Runs,
            static run => Xunit.Assert.Equal(ExperimentExecutionStatus.NotStarted, run.Status));
    }

    /// <summary>
    /// 验证基本统计只使用成功 run，并采用样本标准差。
    /// </summary>
    [Xunit.Fact]
    public async Task RunAsyncCalculatesBasicSampleStatistics()
    {
        var experimentCase = CreateRecordingCase(
            "statistics",
            3,
            1,
            new ConcurrentQueue<string>(),
            new ConcurrentBag<SeedValueOptimizer>());

        var result = await ExperimentRunner.RunAsync(
            new ExperimentDefinition([experimentCase]),
            new ExperimentExecutionOptions { Seeds = [1, 2, 3] },
            Xunit.TestContext.Current.CancellationToken);

        var statistics = result.Cases[0].Statistics;
        Xunit.Assert.Equal(2, statistics.BestObjective!.Mean);
        Xunit.Assert.Equal(2, statistics.BestObjective.Median);
        Xunit.Assert.Equal(1, statistics.BestObjective.Minimum);
        Xunit.Assert.Equal(3, statistics.BestObjective.Maximum);
        Xunit.Assert.Equal(1, statistics.BestObjective.StandardDeviation);
        Xunit.Assert.Equal(0, statistics.Iterations!.StandardDeviation);
        Xunit.Assert.Equal(0, statistics.Evaluations!.StandardDeviation);
    }

    /// <summary>
    /// 验证单个成功样本的标准差为零，且复制出的最佳位置不能修改内部二维矩阵。
    /// </summary>
    [Xunit.Fact]
    public async Task RunAsyncKeepsSingleResultStorageReadOnly()
    {
        var experimentCase = CreateRecordingCase(
            "single-result",
            1,
            1,
            new ConcurrentQueue<string>(),
            new ConcurrentBag<SeedValueOptimizer>());

        var result = await ExperimentRunner.RunAsync(
            new ExperimentDefinition([experimentCase]),
            new ExperimentExecutionOptions { Seeds = [17] },
            Xunit.TestContext.Current.CancellationToken);

        var caseResult = Xunit.Assert.Single(result.Cases);
        var positions = Xunit.Assert.IsType<BestPositionMatrix>(caseResult.BestPositions);
        Span<double> copy = stackalloc double[1];
        positions.CopyPositionTo(0, copy);
        copy[0] = -1;

        Xunit.Assert.Equal(17, positions[0, 0]);
        Xunit.Assert.Equal(0, caseResult.Statistics.BestObjective!.StandardDeviation);
    }

    [Xunit.Fact]
    public async Task RunAsyncUsesNullForStatisticsUndefinedByOppositeInfinities()
    {
        var statistics = await RunValueStatisticsAsync([double.NegativeInfinity, double.PositiveInfinity]);

        Xunit.Assert.Null(statistics.Mean);
        Xunit.Assert.Null(statistics.Median);
        Xunit.Assert.Equal(double.NegativeInfinity, statistics.Minimum);
        Xunit.Assert.Equal(double.PositiveInfinity, statistics.Maximum);
        Xunit.Assert.Null(statistics.StandardDeviation);
    }

    [Xunit.Theory]
    [Xunit.InlineData(1.0, double.PositiveInfinity, double.PositiveInfinity)]
    [Xunit.InlineData(double.NegativeInfinity, 1.0, double.NegativeInfinity)]
    public async Task RunAsyncPreservesOneSidedInfinityInMeanAndMedian(
        double first,
        double second,
        double expected)
    {
        var statistics = await RunValueStatisticsAsync([first, second]);

        Xunit.Assert.Equal(expected, statistics.Mean);
        Xunit.Assert.Equal(expected, statistics.Median);
        Xunit.Assert.Null(statistics.StandardDeviation);
    }

    [Xunit.Fact]
    public async Task RunAsyncKeepsSingleInfiniteSampleDefined()
    {
        var statistics = await RunValueStatisticsAsync([double.PositiveInfinity]);

        Xunit.Assert.Equal(double.PositiveInfinity, statistics.Mean);
        Xunit.Assert.Equal(double.PositiveInfinity, statistics.Median);
        Xunit.Assert.Equal(0, statistics.StandardDeviation);
    }

    [Xunit.Fact]
    public async Task RunAsyncAvoidsIntermediateOverflowForFiniteSamples()
    {
        var sameExtreme = await RunValueStatisticsAsync([double.MaxValue, double.MaxValue]);
        var oppositeExtremes = await RunValueStatisticsAsync([-double.MaxValue, double.MaxValue]);

        Xunit.Assert.Equal(double.MaxValue, sameExtreme.Mean);
        Xunit.Assert.Equal(double.MaxValue, sameExtreme.Median);
        Xunit.Assert.Equal(0, sameExtreme.StandardDeviation);

        Xunit.Assert.Equal(0, oppositeExtremes.Mean);
        Xunit.Assert.Equal(0, oppositeExtremes.Median);
        Xunit.Assert.Equal(double.PositiveInfinity, oppositeExtremes.StandardDeviation);
    }

    /// <summary>
    /// 验证自动派生的共享 seed 不受不同 Case 的 Group 拆分方式影响。
    /// </summary>
    [Xunit.Fact]
    public async Task RunAsyncKeepsDerivedSeedsStableAcrossGroupLayouts()
    {
        var firstCase = CreateRecordingCase(
            "sequential",
            4,
            1,
            new ConcurrentQueue<string>(),
            new ConcurrentBag<SeedValueOptimizer>());
        var secondCase = CreateRecordingCase(
            "parallel",
            4,
            4,
            new ConcurrentQueue<string>(),
            new ConcurrentBag<SeedValueOptimizer>());

        var result = await ExperimentRunner.RunAsync(
            new ExperimentDefinition([firstCase, secondCase]),
            new ExperimentExecutionOptions { BaseSeed = 1234, GlobalMaxConcurrency = 2 },
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(4, result.Seeds.Distinct().Count());
        Xunit.Assert.Equal(
            result.Cases[0].Runs.Select(static run => run.Seed),
            result.Cases[1].Runs.Select(static run => run.Seed));
    }

    /// <summary>
    /// 验证显式 seed 列表必须覆盖最大的 Case 重复次数。
    /// </summary>
    [Xunit.Fact]
    public async Task RunAsyncRejectsAnInsufficientSeedList()
    {
        var experimentCase = CreateRecordingCase(
            "seeds",
            3,
            1,
            new ConcurrentQueue<string>(),
            new ConcurrentBag<SeedValueOptimizer>());

        await Xunit.Assert.ThrowsAsync<ArgumentException>(
            () => ExperimentRunner.RunAsync(
                new ExperimentDefinition([experimentCase]),
                new ExperimentExecutionOptions { Seeds = [1, 2] },
                Xunit.TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// 验证 RunGroup 数量不能超过 Repetition 数量。
    /// </summary>
    [Xunit.Fact]
    public void CaseRejectsTooManyRunGroups()
    {
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => new ExperimentCase<object>(
                "invalid",
                new object(),
                repetitions: 1,
                static (_, _) => new ExperimentGroupSetup(CreateProblem(), new SeedValueOptimizer(), StopImmediately()),
                runGroupCount: 2));
    }

    private static ExperimentCase<RecordingCaseConfiguration> CreateRecordingCase(
        string id,
        int repetitions,
        int runGroupCount,
        ConcurrentQueue<string> creationOrder,
        ConcurrentBag<SeedValueOptimizer> optimizers)
    {
        var configuration = new RecordingCaseConfiguration(creationOrder, optimizers);
        return new ExperimentCase<RecordingCaseConfiguration>(
            id,
            configuration,
            repetitions,
            static (config, context) =>
            {
                config.CreationOrder.Enqueue(
                    $"{context.CaseId}:{context.GroupIndex}:{string.Join(',', context.RepetitionIndices)}");
                var optimizer = new SeedValueOptimizer();
                config.Optimizers.Add(optimizer);
                return new ExperimentGroupSetup(CreateProblem(), optimizer, StopImmediately());
            },
            runGroupCount);
    }

    private static ContinuousProblem CreateProblem()
    {
        return new ContinuousProblem(1, new FirstCoordinateObjective(), CandidateRepairs.DoNothing);
    }

    private static async Task<NumericStatistics> RunValueStatisticsAsync(double[] values)
    {
        var experimentCase = new ExperimentCase<double[]>(
            "special-values",
            values,
            values.Length,
            static (configuration, _) => new ExperimentGroupSetup(
                CreateProblem(),
                new SequenceValueOptimizer(configuration),
                StopImmediately()),
            runGroupCount: 1);
        var result = await ExperimentRunner.RunAsync(
            new ExperimentDefinition([experimentCase]),
            new ExperimentExecutionOptions { Seeds = Enumerable.Range(1, values.Length).ToArray() },
            Xunit.TestContext.Current.CancellationToken);

        return result.Cases[0].Statistics.BestObjective!;
    }

    private static OptimizationRunOptions StopImmediately()
    {
        return new OptimizationRunOptions(StoppingConditions.MaxIterations(0));
    }

    private sealed record RecordingCaseConfiguration(
        ConcurrentQueue<string> CreationOrder,
        ConcurrentBag<SeedValueOptimizer> Optimizers);

    /// <summary>
    /// 返回第一个位置分量，便于按 seed 构造可验证结果。
    /// </summary>
    private sealed class FirstCoordinateObjective : IObjectiveFunction
    {
        public double Evaluate(ReadOnlySpan<double> position) => position[0];
    }

    /// <summary>
    /// 每次 Reset 将 seed 写入同一位置数组，用于验证复用和 seed 对齐。
    /// </summary>
    private sealed class SeedValueOptimizer : IOptimizer
    {
        private readonly double[] _position = new double[1];

        public ReadOnlySpan<double> BestPosition => _position;

        public Evaluation BestEvaluation { get; private set; }

        public int ResetCount { get; private set; }

        public void ResetForRun(OptimizationRunContext context)
        {
            _position[0] = context.Seed;
            BestEvaluation = context.Evaluate(_position);
            ResetCount++;
        }

        public void Advance()
        {
        }
    }

    private sealed class SequenceValueOptimizer(double[] values) : IOptimizer
    {
        private readonly double[] _position = new double[1];
        private int _nextValue;

        public ReadOnlySpan<double> BestPosition => _position;

        public Evaluation BestEvaluation { get; private set; }

        public void ResetForRun(OptimizationRunContext context)
        {
            _position[0] = values[_nextValue++];
            BestEvaluation = context.Evaluate(_position);
        }

        public void Advance()
        {
        }
    }

    /// <summary>
    /// 在 Reset 临界区记录同时运行的 Group 数量。
    /// </summary>
    private sealed class ProbedOptimizer(ConcurrencyProbe probe) : IOptimizer
    {
        private readonly double[] _position = new double[1];

        public ReadOnlySpan<double> BestPosition => _position;

        public Evaluation BestEvaluation { get; private set; }

        public void ResetForRun(OptimizationRunContext context)
        {
            probe.Enter();
            try
            {
                BestEvaluation = context.Evaluate(_position);
            }
            finally
            {
                probe.Exit();
            }
        }

        public void Advance()
        {
        }
    }

    /// <summary>
    /// 让首次创建的 Optimizer 在 Advance 中失败，重建实例正常完成。
    /// </summary>
    private sealed class AdvanceFailureOptimizer(bool shouldFail) : IOptimizer
    {
        private readonly double[] _position = new double[1];
        private OptimizationRunContext? _context;

        public ReadOnlySpan<double> BestPosition => _position;

        public Evaluation BestEvaluation { get; private set; }

        public void ResetForRun(OptimizationRunContext context)
        {
            _context = context;
            _position[0] = context.Seed;
            BestEvaluation = context.Evaluate(_position);
        }

        public void Advance()
        {
            if (shouldFail)
            {
                throw new InvalidOperationException("run failed");
            }

            BestEvaluation = _context!.Evaluate(_position);
        }
    }

    /// <summary>
    /// 在第二次 Reset 时请求取消，用于构造部分结果。
    /// </summary>
    private sealed class CancelOnSecondResetOptimizer(CancellationTokenSource cancellation) : IOptimizer
    {
        private readonly double[] _position = new double[1];
        private int _resetCount;

        public ReadOnlySpan<double> BestPosition => _position;

        public Evaluation BestEvaluation { get; private set; }

        public void ResetForRun(OptimizationRunContext context)
        {
            _resetCount++;
            if (_resetCount == 2)
            {
                cancellation.Cancel();
            }

            _position[0] = context.Seed;
            BestEvaluation = context.Evaluate(_position);
        }

        public void Advance()
        {
        }
    }

    /// <summary>
    /// 使用短暂屏障验证两个 Group 能够同时进入算法代码。
    /// </summary>
    private sealed class ConcurrencyProbe(int requiredConcurrency) : IDisposable
    {
        private readonly ManualResetEventSlim _requiredConcurrencyReached = new(false);
        private int _active;
        private int _maximumConcurrency;

        public int MaximumConcurrency => Volatile.Read(ref _maximumConcurrency);

        public void Enter()
        {
            var active = Interlocked.Increment(ref _active);
            UpdateMaximum(active);
            if (active >= requiredConcurrency)
            {
                _requiredConcurrencyReached.Set();
            }

            if (!_requiredConcurrencyReached.Wait(TimeSpan.FromSeconds(2)))
            {
                throw new TimeoutException("The expected experiment concurrency was not reached.");
            }
        }

        public void Exit()
        {
            Interlocked.Decrement(ref _active);
        }

        public void Dispose()
        {
            _requiredConcurrencyReached.Dispose();
        }

        private void UpdateMaximum(int value)
        {
            var observed = Volatile.Read(ref _maximumConcurrency);
            while (value > observed)
            {
                var original = Interlocked.CompareExchange(ref _maximumConcurrency, value, observed);
                if (original == observed)
                {
                    return;
                }

                observed = original;
            }
        }
    }
}
