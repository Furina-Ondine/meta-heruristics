using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Core.Execution;

/// <summary>
/// 指定初始化基线之后，优化执行在何种进度事件上追加轨迹点。
/// </summary>
public enum OptimizationTraceMode
{
    /// <summary>
    /// 不记录轨迹点。
    /// </summary>
    None,

    /// <summary>
    /// 记录初始化基线，并在每次完成迭代时记录。
    /// </summary>
    EveryIteration,

    /// <summary>
    /// 记录初始化基线，并在达到后续评估次数间隔时记录。
    /// </summary>
    EvaluationInterval,

    /// <summary>
    /// 记录初始化基线，并按用户指定的最大迭代预算比例记录。
    /// </summary>
    IterationProgress,
}

/// <summary>
/// 配置优化轨迹的记录策略、评估间隔和可选进度总迭代数。
/// </summary>
public sealed class OptimizationTraceOptions
{
    private int? _progressTotalIterations;

    /// <summary>
    /// 创建轨迹配置。
    /// </summary>
    /// <param name="mode">轨迹记录模式。</param>
    /// <param name="evaluationInterval">评估间隔模式下的正整数间隔。</param>
    /// <param name="progressIntervalRatio">
    /// 按迭代进度记录时的比例间隔，取值范围为 <c>(0, 1]</c>；例如 <c>0.1</c> 表示每 10%。
    /// 其他模式下保持默认值零。
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="mode"/> 不是定义的枚举值、评估间隔不是正数，或进度模式的比例不在 <c>(0, 1]</c> 内。
    /// </exception>
    public OptimizationTraceOptions(
        OptimizationTraceMode mode,
        long evaluationInterval = 1,
        double progressIntervalRatio = 0)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(evaluationInterval);

        if (mode == OptimizationTraceMode.IterationProgress
            && (!double.IsFinite(progressIntervalRatio)
                || progressIntervalRatio <= 0
                || progressIntervalRatio > 1))
        {
            throw new ArgumentOutOfRangeException(nameof(progressIntervalRatio));
        }

        Mode = mode;
        EvaluationInterval = evaluationInterval;
        ProgressIntervalRatio = progressIntervalRatio;
    }

    /// <summary>
    /// 获取不记录轨迹的默认配置。
    /// </summary>
    public static OptimizationTraceOptions None { get; } = new(OptimizationTraceMode.None);

    /// <summary>
    /// 获取轨迹记录模式。
    /// </summary>
    public OptimizationTraceMode Mode { get; }

    /// <summary>
    /// 获取评估间隔模式使用的正整数间隔。
    /// </summary>
    public long EvaluationInterval { get; }

    /// <summary>
    /// 获取按迭代进度记录时使用的比例间隔；例如 <c>0.1</c> 表示每 10%。
    /// </summary>
    public double ProgressIntervalRatio { get; }

    /// <summary>
    /// 获取或设置按比例记录时使用的总迭代数。
    /// </summary>
    /// <remarks>
    /// 未指定时，Runner 会尝试从 <see cref="StoppingConditions.MaxIterations(int)"/>
    /// 或包含该条件的 <see cref="StoppingConditions.Any(ReadOnlySpan{IStoppingCondition})"/> 中读取。
    /// 显式值优先于停止条件中的最大迭代数。
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">设置值不是正数。</exception>
    public int? ProgressTotalIterations
    {
        get => _progressTotalIterations;
        init
        {
            if (value is not null)
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value.Value);
            }

            _progressTotalIterations = value;
        }
    }
}

/// <summary>
/// 表示某次运行中记录的最优状态快照。
/// </summary>
/// <param name="Iteration">记录时已完成的迭代次数。</param>
/// <param name="Evaluations">记录时已完成的目标评估次数。</param>
/// <param name="Elapsed">从运行开始到记录时经过的时间。</param>
/// <param name="BestEvaluation">记录时的当前最优评估结果。</param>
public readonly record struct OptimizationTracePoint(
    int Iteration,
    long Evaluations,
    TimeSpan Elapsed,
    Evaluation BestEvaluation);
