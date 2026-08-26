using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Tests.Core;

/// <summary>
/// 验证单次优化运行的生命周期、随机性、取消和轨迹记录契约。
/// </summary>
public sealed class OptimizationRunnerTests
{
    /// <summary>
    /// 验证按比例记录模式必须显式提供比例参数。
    /// </summary>
    [Xunit.Fact]
    public void IterationProgressTraceRequiresAUserSuppliedRatio()
    {
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => new OptimizationTraceOptions(OptimizationTraceMode.IterationProgress));
    }

    /// <summary>
    /// 验证 Runner 重置调用方拥有的优化器，且调用方可在重置前显式复制最佳位置。
    /// </summary>
    [Xunit.Fact]
    public void ExecuteLeavesTheBestPositionOwnedByTheOptimizer()
    {
        var objective = new RecordingObjective();
        var problem = new ContinuousProblem(1, objective);
        var optimizer = new CountdownOptimizer();
        var options = new OptimizationRunOptions(StoppingConditions.MaxIterations(2))
        {
            Trace = new OptimizationTraceOptions(OptimizationTraceMode.EveryIteration),
        };

        var result = OptimizationRunner.Execute(
            problem,
            optimizer,
            options,
            seed: 42,
            cancellationToken: Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(TerminationReason.MaxIterations, result.TerminationReason);
        Xunit.Assert.Equal(2, result.Iterations);
        Xunit.Assert.Equal(3, result.Evaluations);
        Xunit.Assert.Equal(42, result.Seed);
        var bestPosition = optimizer.BestPosition.ToArray();
        Xunit.Assert.Equal(1, bestPosition[0]);
        Xunit.Assert.Equal(1, result.BestEvaluation.Objective);
        Xunit.Assert.Equal(3, result.Trace.Count);
        Xunit.Assert.Equal(1, optimizer.BestPositionAccessCount);
        Xunit.Assert.Equal(1, optimizer.ResetCount);

        optimizer.Position[0] = 9;
        Xunit.Assert.Equal(1, bestPosition[0]);
    }

    /// <summary>
    /// 验证相同种子会为可复用优化器创建相同的独立随机序列。
    /// </summary>
    [Xunit.Fact]
    public void RunUsesAnIndependentSeededRandomStream()
    {
        var problem = new ContinuousProblem(1, new RecordingObjective(), CandidateRepairs.Clamp(0, 1));
        var optimizer = new RandomValueOptimizer();
        var options = new OptimizationRunOptions(StoppingConditions.MaxIterations(0));
        var first = OptimizationRunner.Execute(
            problem,
            optimizer,
            options,
            seed: 1234,
            cancellationToken: Xunit.TestContext.Current.CancellationToken);
        var firstPosition = optimizer.BestPosition.ToArray();
        var second = OptimizationRunner.Execute(
            problem,
            optimizer,
            options,
            seed: 1234,
            cancellationToken: Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(firstPosition[0], optimizer.BestPosition[0]);
        Xunit.Assert.Equal(2, optimizer.ResetCount);
    }

    /// <summary>
    /// 验证收到取消请求时运行抛出取消异常，并且不会继续推进当前优化器。
    /// </summary>
    [Xunit.Fact]
    public void RunThrowsForCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var problem = new ContinuousProblem(1, new CancellingObjective(cancellation));
        var optimizer = new CountdownOptimizer();

        Xunit.Assert.Throws<OperationCanceledException>(
            () => OptimizationRunner.Execute(
                problem,
                optimizer,
                new OptimizationRunOptions(StoppingConditions.MaxIterations(2)),
                cancellationToken: cancellation.Token));
        Xunit.Assert.Equal(1, optimizer.ResetCount);
    }

    /// <summary>
    /// 验证轨迹按用户指定的迭代进度比例记录检查点。
    /// </summary>
    [Xunit.Fact]
    public void RunRecordsAtTheRequestedProgressRatio()
    {
        var problem = new ContinuousProblem(1, new RecordingObjective(), CandidateRepairs.Clamp(0, 1));
        var optimizer = new RandomValueOptimizer();
        var options = new OptimizationRunOptions(
            StoppingConditions.Any(StoppingConditions.MaxEvaluations(1_000), StoppingConditions.MaxIterations(20)))
        {
            Trace = new OptimizationTraceOptions(OptimizationTraceMode.IterationProgress, progressIntervalRatio: 0.25),
        };

        var result = OptimizationRunner.Execute(
            problem,
            optimizer,
            options,
            cancellationToken: Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(
            [0, 5, 10, 15, 20],
            result.Trace.Select(static point => point.Iteration));
    }

    /// <summary>
    /// 验证显式总迭代数优先于停止条件中的最大迭代数。
    /// </summary>
    [Xunit.Fact]
    public void ProportionalTraceAcceptsAnExplicitIterationTotal()
    {
        var problem = new ContinuousProblem(1, new RecordingObjective(), CandidateRepairs.Clamp(0, 1));
        var optimizer = new RandomValueOptimizer();
        var options = new OptimizationRunOptions(StoppingConditions.MaxIterations(10))
        {
            Trace = new OptimizationTraceOptions(OptimizationTraceMode.IterationProgress, progressIntervalRatio: 0.25)
            {
                ProgressTotalIterations = 20,
            },
        };

        var result = OptimizationRunner.Execute(
            problem,
            optimizer,
            options,
            cancellationToken: Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(
            [0, 5, 10],
            result.Trace.Select(static point => point.Iteration));
    }

    /// <summary>
    /// 验证非整数检查点会向上取到已完成的迭代次数。
    /// </summary>
    [Xunit.Fact]
    public void ProportionalTraceRoundsEachCheckpointUpToACompletedIteration()
    {
        var problem = new ContinuousProblem(1, new RecordingObjective(), CandidateRepairs.Clamp(0, 1));
        var optimizer = new RandomValueOptimizer();
        var options = new OptimizationRunOptions(StoppingConditions.MaxIterations(23))
        {
            Trace = new OptimizationTraceOptions(OptimizationTraceMode.IterationProgress, progressIntervalRatio: 0.1),
        };

        var result = OptimizationRunner.Execute(
            problem,
            optimizer,
            options,
            cancellationToken: Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(
            [0, 3, 5, 7, 10, 12, 14, 17, 19, 21, 23],
            result.Trace.Select(static point => point.Iteration));
    }

    /// <summary>
    /// 验证按比例记录即使比例不能整除预算也始终记录最终状态。
    /// </summary>
    [Xunit.Fact]
    public void ProportionalTraceAlwaysRecordsTheFinalState()
    {
        var problem = new ContinuousProblem(1, new RecordingObjective(), CandidateRepairs.Clamp(0, 1));
        var optimizer = new RandomValueOptimizer();
        var options = new OptimizationRunOptions(StoppingConditions.MaxIterations(10))
        {
            Trace = new OptimizationTraceOptions(OptimizationTraceMode.IterationProgress, progressIntervalRatio: 0.3),
        };

        var result = OptimizationRunner.Execute(
            problem,
            optimizer,
            options,
            cancellationToken: Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(
            [0, 3, 6, 9, 10],
            result.Trace.Select(static point => point.Iteration));
    }

    /// <summary>
    /// 将第一维直接作为目标值，便于断言 Runner 记录的状态。
    /// </summary>
    private sealed class RecordingObjective : IObjectiveFunction
    {
        /// <summary>
        /// 返回候选位置的第一个维度，用于稳定记录评估结果。
        /// </summary>
        /// <param name="position">包含至少一个维度的候选位置。</param>
        /// <returns>候选位置的第一个值。</returns>
        public double Evaluate(ReadOnlySpan<double> position) => position[0];
    }

    /// <summary>
    /// 在评估过程中触发取消，用于验证 Runner 的释放路径。
    /// </summary>
    private sealed class CancellingObjective(CancellationTokenSource cancellation) : IObjectiveFunction
    {
        /// <summary>
        /// 请求取消后返回候选值，以覆盖评估期间的取消路径。
        /// </summary>
        /// <param name="position">包含至少一个维度的候选位置。</param>
        /// <returns>候选位置的第一个值。</returns>
        public double Evaluate(ReadOnlySpan<double> position)
        {
            cancellation.Cancel();
            return position[0];
        }
    }

    /// <summary>
    /// 使用一组可复用位置缓冲区实现倒计时优化器。
    /// </summary>
    private sealed class CountdownOptimizer : IOptimizer
    {
        private OptimizationRunContext? _context;

        /// <summary>
        /// 获取测试用的可变候选位置。
        /// </summary>
        public double[] Position { get; } = [3];

        /// <summary>
        /// 获取 Runner 读取最佳位置的次数，用于防止每轮重复复制位置缓冲区。
        /// </summary>
        public int BestPositionAccessCount { get; private set; }

        /// <summary>
        /// 获取运行重置次数，用于验证同一实例可执行多个顺序 run。
        /// </summary>
        public int ResetCount { get; private set; }

        /// <summary>
        /// 返回倒计时优化器当前的候选位置。
        /// </summary>
        public ReadOnlySpan<double> BestPosition
        {
            get
            {
                BestPositionAccessCount++;
                return Position;
            }
        }

        /// <summary>
        /// 获取倒计时优化器当前的评估结果。
        /// </summary>
        public Evaluation BestEvaluation { get; private set; }

        /// <summary>
        /// 重置位置并评估当前 run 的初始候选。
        /// </summary>
        /// <param name="context">当前 run 独占的执行上下文。</param>
        public void ResetForRun(OptimizationRunContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Position[0] = 3;
            BestEvaluation = _context.Evaluate(Position);
            ResetCount++;
        }

        /// <summary>
        /// 将位置减一并重新评估，模拟持续改善的迭代。
        /// </summary>
        public void Advance()
        {
            Position[0]--;
            BestEvaluation = _context!.Evaluate(Position);
        }
    }

    /// <summary>
    /// 使用运行独立随机流并复用位置数组的最小优化器。
    /// </summary>
    private sealed class RandomValueOptimizer : IOptimizer
    {
        private readonly double[] _position = new double[1];

        /// <summary>
        /// 获取运行重置次数。
        /// </summary>
        public int ResetCount { get; private set; }

        /// <summary>
        /// 返回随机值优化器当前的位置。
        /// </summary>
        public ReadOnlySpan<double> BestPosition => _position;

        /// <summary>
        /// 获取随机值优化器当前的评估结果。
        /// </summary>
        public Evaluation BestEvaluation { get; private set; }

        /// <summary>
        /// 从运行专属随机流生成并评估初始位置，同时复用既有位置数组。
        /// </summary>
        /// <param name="context">当前 run 独占的执行上下文。</param>
        public void ResetForRun(OptimizationRunContext context)
        {
            _position[0] = context.Random.NextDouble();
            BestEvaluation = context.Evaluate(_position);
            ResetCount++;
        }

        /// <summary>
        /// 随机值测试优化器不需要推进迭代。
        /// </summary>
        public void Advance()
        {
        }

    }
}
