using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Core.Execution;

/// <summary>
/// 表示一个 RunGroup 独占、可在多次顺序运行之间复用工作区的优化器实例。
/// </summary>
/// <remarks>
/// 实例可以保存种群数组和算法临时缓冲区，但不保证线程安全，也不能在一次运行异常后继续复用。
/// <see cref="BestPosition"/> 的存储归 Optimizer 所有；调用方必须在下一次 <see cref="ResetForRun"/> 前完成读取或复制。
/// </remarks>
public interface IOptimizer
{
    /// <summary>
    /// 获取当前运行发现的最优候选位置。
    /// </summary>
    ReadOnlySpan<double> BestPosition { get; }

    /// <summary>
    /// 获取当前最优候选位置的评估结果。
    /// </summary>
    Evaluation BestEvaluation { get; }

    /// <summary>
    /// 为一次新运行重置全部逻辑状态、初始化候选并完成所需的初始评估。
    /// </summary>
    /// <param name="context">当前运行独占的问题、随机数、评估计数和取消上下文。</param>
    /// <remarks>
    /// 首次调用可以分配依赖问题维度的工作区；后续调用应复用主要数组。
    /// 返回前必须产生合法的 <see cref="BestPosition"/> 和 <see cref="BestEvaluation"/>。
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="OperationCanceledException">运行取消令牌已请求取消。</exception>
    void ResetForRun(OptimizationRunContext context);

    /// <summary>
    /// 执行一次优化迭代并更新当前运行状态。
    /// </summary>
    /// <exception cref="OperationCanceledException">运行取消令牌已请求取消。</exception>
    void Advance();
}

/// <summary>
/// 提供一次运行所需的问题、随机数、取消令牌和评估计数。
/// </summary>
/// <remarks>
/// 每个 run 获得独立的上下文和随机流；目标评估应通过此类型的方法完成，以保持计数和取消语义一致。
/// Context 仅属于创建它的一次执行，不能缓存或跨线程共享。
/// </remarks>
public sealed class OptimizationRunContext
{
    internal OptimizationRunContext(ContinuousProblem problem, int seed, CancellationToken cancellationToken)
    {
        Problem = problem;
        Seed = seed;
        Random = new Random(seed);
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// 获取当前运行所解决的问题。
    /// </summary>
    public ContinuousProblem Problem { get; }

    /// <summary>
    /// 获取当前运行使用的随机种子。
    /// </summary>
    public int Seed { get; }

    /// <summary>
    /// 获取当前运行独占的随机数生成器。
    /// </summary>
    public Random Random { get; }

    /// <summary>
    /// 获取当前运行的取消令牌。
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// 获取当前运行已完成的目标评估次数。
    /// </summary>
    public long Evaluations { get; private set; }

    /// <summary>
    /// 评估一个候选位置，并递增评估计数。
    /// </summary>
    /// <param name="position">待评估的候选位置。</param>
    /// <returns>目标值和约束违背汇总。</returns>
    /// <exception cref="OperationCanceledException">取消令牌在评估前已请求取消。</exception>
    /// <exception cref="ArgumentException">候选位置维度不正确。</exception>
    /// <exception cref="ArgumentOutOfRangeException">候选位置越界或包含非有限值。</exception>
    /// <exception cref="InvalidOperationException">目标函数或约束返回了无效数值。</exception>
    public Evaluation Evaluate(ReadOnlySpan<double> position)
    {
        CancellationToken.ThrowIfCancellationRequested();
        var evaluation = Problem.Evaluate(position);
        Evaluations = checked(Evaluations + 1);
        return evaluation;
    }

    /// <summary>
    /// 使用问题配置的修复策略就地修复候选位置，并在修复后验证边界。
    /// </summary>
    /// <param name="position">要就地修复的候选位置。</param>
    /// <exception cref="OperationCanceledException">取消令牌在修复前已请求取消。</exception>
    /// <exception cref="ArgumentException">候选位置维度不正确。</exception>
    /// <exception cref="ArgumentOutOfRangeException">修复后的候选位置越界或包含非有限值。</exception>
    public void Repair(Span<double> position)
    {
        CancellationToken.ThrowIfCancellationRequested();
        Problem.Repair?.Repair(position, Problem, Random);
        Problem.ValidatePosition(position);
    }
}

/// <summary>
/// 提供停止条件在某个检查点看到的运行状态快照。
/// </summary>
/// <param name="Iterations">已完成的迭代次数。</param>
/// <param name="Evaluations">已完成的目标评估次数。</param>
/// <param name="Elapsed">从运行开始到检查点经过的时间。</param>
/// <param name="BestEvaluation">检查点的当前最优评估结果。</param>
/// <param name="Direction">当前问题的优化方向。</param>
public readonly record struct OptimizationState(
    int Iterations,
    long Evaluations,
    TimeSpan Elapsed,
    Evaluation BestEvaluation,
    OptimizationDirection Direction);

/// <summary>
/// 定义根据运行状态决定是否终止优化的策略。
/// </summary>
/// <remarks>实现必须可重入，且不得保存执行级可变状态；同一实例可能被多个 Group 并发调用。</remarks>
public interface IStoppingCondition
{
    /// <summary>
    /// 检查运行状态并返回终止原因。
    /// </summary>
    /// <param name="state">当前运行状态快照。</param>
    /// <returns>应终止时返回原因，否则返回 <see langword="null"/>。</returns>
    TerminationReason? Evaluate(OptimizationState state);
}

/// <summary>
/// 标识优化运行结束的原因。
/// </summary>
public enum TerminationReason
{
    /// <summary>
    /// 达到最大迭代次数。
    /// </summary>
    MaxIterations,

    /// <summary>
    /// 达到最大目标评估次数。
    /// </summary>
    MaxEvaluations,

    /// <summary>
    /// 达到最大运行时长。
    /// </summary>
    TimeLimit,

    /// <summary>
    /// 达到目标值阈值。
    /// </summary>
    TargetReached,

    /// <summary>
    /// 在预定窗口内没有继续改善。
    /// </summary>
    NoImprovement,

    /// <summary>
    /// 调用方定义的条件请求终止。
    /// </summary>
    UserCondition,
}

/// <summary>
/// 配置一次优化运行的停止条件和轨迹选项。
/// </summary>
public sealed class OptimizationRunOptions
{
    /// <summary>
    /// 使用必需的停止条件创建运行配置。
    /// </summary>
    /// <param name="stoppingCondition">用于检查运行是否应终止的条件。</param>
    /// <exception cref="ArgumentNullException"><paramref name="stoppingCondition"/> 为 <see langword="null"/>。</exception>
    public OptimizationRunOptions(IStoppingCondition stoppingCondition)
    {
        StoppingCondition = stoppingCondition
            ?? throw new ArgumentNullException(nameof(stoppingCondition));
    }

    /// <summary>
    /// 获取运行停止条件。
    /// </summary>
    public IStoppingCondition StoppingCondition { get; }

    /// <summary>
    /// 获取或设置轨迹记录配置；默认为 <see cref="OptimizationTraceOptions.None"/>。
    /// </summary>
    public OptimizationTraceOptions Trace { get; init; } = OptimizationTraceOptions.None;
}
