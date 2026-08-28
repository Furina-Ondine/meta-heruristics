using System.Diagnostics;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Core.Execution;

/// <summary>
/// 驱动单次优化运行的生命周期。
/// </summary>
/// <remarks>
/// Runner 负责创建 run 级上下文、检查取消和停止条件、构造标量汇总以及收集轨迹；
/// Optimizer 由调用方创建并拥有，可以在多次正常的顺序运行之间复用工作区。
/// </remarks>
public static class OptimizationRunner
{
    /// <summary>
    /// 执行一次优化，并返回不复制最佳位置的不可变汇总。
    /// </summary>
    /// <remarks>
    /// 返回后 <paramref name="optimizer"/> 仍拥有最终最佳位置。调用方如需稳定快照，必须在下一次
    /// <see cref="IOptimizer.ResetForRun"/> 前复制 <see cref="IOptimizer.BestPosition"/>；Runner 不释放 Optimizer。
    /// </remarks>
    /// <param name="problem">待解决的连续优化问题。</param>
    /// <param name="optimizer">当前调用方拥有的有状态优化器实例。</param>
    /// <param name="options">运行停止和轨迹配置。</param>
    /// <param name="seed">当前运行使用的随机种子。</param>
    /// <param name="cancellationToken">用于取消运行的令牌。</param>
    /// <returns>不包含最佳位置副本的不可变运行汇总。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="problem"/>、<paramref name="optimizer"/>、<paramref name="options"/> 或轨迹配置为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">
    /// 轨迹模式为 <see cref="OptimizationTraceMode.IterationProgress"/>，但既未显式提供正的总迭代数，
    /// 也无法从停止条件中找到最大迭代数。
    /// </exception>
    /// <exception cref="OperationCanceledException">运行开始前、重置后或迭代后收到取消请求。</exception>
    /// <exception cref="InvalidOperationException">停止条件返回未定义的终止原因。</exception>
    public static OptimizationRunSummary Execute(
        ContinuousProblem problem,
        IOptimizer optimizer,
        OptimizationRunOptions options,
        int seed = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(optimizer);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Trace);
        cancellationToken.ThrowIfCancellationRequested();

        var progressTotalIterations = ResolveProgressTotalIterations(options);

        var context = new OptimizationRunContext(problem, seed, cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        // Reset 同时负责当前 run 的逻辑初始化、位置 Repair 和初始评估；正常返回后才能读取最佳状态。
        optimizer.ResetForRun(context);
        cancellationToken.ThrowIfCancellationRequested();

        var iterations = 0;
        var bestEvaluation = optimizer.BestEvaluation;
        var trace = new TraceCollector(options.Trace, progressTotalIterations);
        trace.RecordInitial(iterations, context.Evaluations, stopwatch.Elapsed, bestEvaluation);

        while (true)
        {
            // 停止条件只在初始化和完整 Advance 之间检查；若一次迭代跨过评估阈值，
            // 当前迭代仍会完整结束，然后在下一个检查点停止。
            var state = new OptimizationState(
                iterations,
                context.Evaluations,
                stopwatch.Elapsed,
                bestEvaluation,
                problem.Direction);
            var reason = options.StoppingCondition.Evaluate(state);
            if (reason is not null)
            {
                if (!Enum.IsDefined(reason.Value))
                {
                    throw new InvalidOperationException("The stopping condition returned an invalid termination reason.");
                }

                stopwatch.Stop();
                return new OptimizationRunSummary(
                    optimizer.BestEvaluation,
                    reason.Value,
                    iterations,
                    context.Evaluations,
                    stopwatch.Elapsed,
                    seed,
                    trace.Points);
            }

            optimizer.Advance();
            cancellationToken.ThrowIfCancellationRequested();
            iterations = checked(iterations + 1);
            bestEvaluation = optimizer.BestEvaluation;
            trace.RecordStep(iterations, context.Evaluations, stopwatch.Elapsed, bestEvaluation);
        }
    }

    private static int? ResolveProgressTotalIterations(OptimizationRunOptions options)
    {
        if (options.Trace.Mode != OptimizationTraceMode.IterationProgress)
        {
            return null;
        }

        var totalIterations = options.Trace.ProgressTotalIterations
            ?? StoppingConditions.FindMaximumIterations(options.StoppingCondition);
        if (totalIterations is null or <= 0)
        {
            throw new ArgumentException(
                "IterationProgress tracing requires a positive explicit progress total or a MaxIterations stopping condition.",
                nameof(options));
        }

        return totalIterations;
    }

    /// <summary>
    /// 根据配置筛选轨迹检查点；轨迹只保存评估结果，不读取或复制候选位置。
    /// </summary>
    private sealed class TraceCollector
    {
        private readonly OptimizationTraceOptions _options;
        private readonly int? _progressTotalIterations;
        private readonly List<OptimizationTracePoint> _points = [];
        private long _nextEvaluationThreshold;
        private double _lastProgressBucket;
        private bool _progressCompleted;

        public TraceCollector(OptimizationTraceOptions options, int? progressTotalIterations)
        {
            _options = options;
            _progressTotalIterations = progressTotalIterations;
        }

        public IReadOnlyList<OptimizationTracePoint> Points => _points;

        public void RecordInitial(int iteration, long evaluations, TimeSpan elapsed, Evaluation bestEvaluation)
        {
            if (_options.Mode == OptimizationTraceMode.None)
            {
                return;
            }

            Add(iteration, evaluations, elapsed, bestEvaluation);
            if (_options.Mode == OptimizationTraceMode.EvaluationInterval)
            {
                _nextEvaluationThreshold = NextThreshold(evaluations, _options.EvaluationInterval);
            }
        }

        public void RecordStep(int iteration, long evaluations, TimeSpan elapsed, Evaluation currentBest)
        {
            var shouldRecord = _options.Mode switch
            {
                OptimizationTraceMode.None => false,
                OptimizationTraceMode.EveryIteration => true,
                OptimizationTraceMode.EvaluationInterval => evaluations >= _nextEvaluationThreshold,
                OptimizationTraceMode.IterationProgress => HasReachedNextProgressInterval(iteration),
                _ => throw new InvalidOperationException("Unsupported trace mode."),
            };

            if (!shouldRecord)
            {
                return;
            }

            Add(iteration, evaluations, elapsed, currentBest);
            if (_options.Mode == OptimizationTraceMode.EvaluationInterval)
            {
                _nextEvaluationThreshold = NextThreshold(evaluations, _options.EvaluationInterval);
            }
            else if (_options.Mode == OptimizationTraceMode.IterationProgress)
            {
                _lastProgressBucket = GetProgressBucket(iteration);
                _progressCompleted = iteration >= _progressTotalIterations!.Value;
            }
        }

        private bool HasReachedNextProgressInterval(int iteration)
        {
            // 无论比例能否整除 100%，最终完成最大迭代预算时都保留一个结果点。
            return !_progressCompleted
                && (iteration >= _progressTotalIterations!.Value
                    || GetProgressBucket(iteration) > _lastProgressBucket);
        }

        private double GetProgressBucket(int iteration)
        {
            var progress = (double)iteration / _progressTotalIterations!.Value;
            // 微小容差避免 0.3 / 0.1 等二进制浮点表示使整数比例点被延迟一轮。
            return Math.Floor((progress / _options.ProgressIntervalRatio) + 1e-12);
        }

        private static long NextThreshold(long evaluations, long interval)
        {
            var intervals = evaluations / interval;
            return intervals >= (long.MaxValue / interval)
                ? long.MaxValue
                : (intervals + 1) * interval;
        }

        private void Add(int iteration, long evaluations, TimeSpan elapsed, Evaluation bestEvaluation)
        {
            _points.Add(new OptimizationTracePoint(iteration, evaluations, elapsed, bestEvaluation));
        }
    }
}
