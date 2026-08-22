using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Core.Execution;

/// <summary>
/// 创建常用的优化停止条件。
/// </summary>
public static class StoppingConditions
{
    /// <summary>
    /// 创建按已完成迭代次数停止的条件。
    /// </summary>
    /// <param name="maximumIterations">允许完成的最大迭代次数；为零表示初始化后立即停止。</param>
    /// <returns>达到迭代上限时返回 <see cref="TerminationReason.MaxIterations"/> 的停止条件。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maximumIterations"/> 为负数。</exception>
    public static IStoppingCondition MaxIterations(int maximumIterations)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximumIterations);

        return new MaximumIterationsCondition(maximumIterations);
    }

    /// <summary>
    /// 创建按目标评估次数停止的条件。
    /// </summary>
    /// <remarks>
    /// Runner 在初始化和完整迭代之间检查此阈值；一次迭代可能使最终评估计数越过阈值。
    /// </remarks>
    /// <param name="maximumEvaluations">触发停止的目标评估次数阈值，必须为正数。</param>
    /// <returns>达到评估上限时返回 <see cref="TerminationReason.MaxEvaluations"/> 的停止条件。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maximumEvaluations"/> 不是正数。</exception>
    public static IStoppingCondition MaxEvaluations(long maximumEvaluations)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumEvaluations);

        return new MaximumEvaluationsCondition(maximumEvaluations);
    }

    /// <summary>
    /// 创建按运行时长停止的条件。
    /// </summary>
    /// <param name="maximumDuration">允许的最大运行时长，必须大于零。</param>
    /// <returns>达到时长上限时返回 <see cref="TerminationReason.TimeLimit"/> 的停止条件。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maximumDuration"/> 不大于零。</exception>
    public static IStoppingCondition TimeLimit(TimeSpan maximumDuration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maximumDuration, TimeSpan.Zero);

        return new TimeLimitCondition(maximumDuration);
    }

    /// <summary>
    /// 创建按可行候选目标值停止的条件。
    /// </summary>
    /// <param name="target">目标阈值；最小化时要求目标值不大于该值，最大化时要求不小于该值。</param>
    /// <returns>达到目标阈值时返回 <see cref="TerminationReason.TargetReached"/> 的停止条件。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="target"/> 不是有限值。</exception>
    public static IStoppingCondition TargetObjective(double target)
    {
        if (!double.IsFinite(target))
        {
            throw new ArgumentOutOfRangeException(nameof(target));
        }

        return new TargetObjectiveCondition(target);
    }

    /// <summary>
    /// 创建在任一给定条件触发时停止的组合条件。
    /// </summary>
    /// <param name="conditions">至少一个非空停止条件；按传入顺序检查。</param>
    /// <returns>返回第一个触发条件的终止原因，否则返回 <see langword="null"/> 的组合条件。</returns>
    /// <exception cref="ArgumentException">没有提供条件，或条件集合包含 <see langword="null"/> 项。</exception>
    public static IStoppingCondition Any(params ReadOnlySpan<IStoppingCondition> conditions)
    {
        if (conditions.IsEmpty)
        {
            throw new ArgumentException("At least one stopping condition is required.", nameof(conditions));
        }

        var copy = conditions.ToArray();
        if (copy.Any(static condition => condition is null))
        {
            throw new ArgumentException("A stopping condition cannot be null.", nameof(conditions));
        }

        return new AnyCondition(copy);
    }

    /// <summary>
    /// 从标准停止条件或其 OR 组合中提取最早的最大迭代阈值，供比例轨迹计算进度。
    /// </summary>
    /// <param name="condition">要检查的停止条件。</param>
    /// <returns>找到的最小迭代阈值；无法推断时返回 <see langword="null"/>。</returns>
    internal static int? FindMaximumIterations(IStoppingCondition condition)
    {
        return condition switch
        {
            MaximumIterationsCondition maximum => maximum.MaximumIterations,
            AnyCondition any => FindSmallestMaximumIterations(any.Conditions),
            _ => null,
        };
    }

    private static int? FindSmallestMaximumIterations(IReadOnlyList<IStoppingCondition> conditions)
    {
        int? smallest = null;
        foreach (var condition in conditions)
        {
            var maximum = FindMaximumIterations(condition);
            if (maximum is not null && (smallest is null || maximum.Value < smallest.Value))
            {
                smallest = maximum;
            }
        }

        return smallest;
    }

    /// <summary>
    /// 在已完成迭代数达到阈值时终止运行。
    /// </summary>
    private sealed record MaximumIterationsCondition(int MaximumIterations) : IStoppingCondition
    {
        public TerminationReason? Evaluate(OptimizationState state)
        {
            return state.Iterations >= MaximumIterations ? TerminationReason.MaxIterations : null;
        }
    }

    /// <summary>
    /// 在已完成评估数达到阈值时终止运行。
    /// </summary>
    private sealed record MaximumEvaluationsCondition(long MaximumEvaluations) : IStoppingCondition
    {
        public TerminationReason? Evaluate(OptimizationState state)
        {
            return state.Evaluations >= MaximumEvaluations ? TerminationReason.MaxEvaluations : null;
        }
    }

    /// <summary>
    /// 在运行时间达到阈值时终止运行。
    /// </summary>
    private sealed record TimeLimitCondition(TimeSpan MaximumDuration) : IStoppingCondition
    {
        public TerminationReason? Evaluate(OptimizationState state)
        {
            return state.Elapsed >= MaximumDuration ? TerminationReason.TimeLimit : null;
        }
    }

    /// <summary>
    /// 在可行候选按当前优化方向达到目标阈值时终止运行。
    /// </summary>
    private sealed record TargetObjectiveCondition(double Target) : IStoppingCondition
    {
        public TerminationReason? Evaluate(OptimizationState state)
        {
            if (!state.BestEvaluation.Constraints.IsFeasible)
            {
                return null;
            }

            var reached = state.Direction == OptimizationDirection.Minimize
                ? state.BestEvaluation.Objective <= Target
                : state.BestEvaluation.Objective >= Target;
            return reached ? TerminationReason.TargetReached : null;
        }
    }

    /// <summary>
    /// 按声明顺序检查子条件，并返回第一个触发的终止原因。
    /// </summary>
    private sealed class AnyCondition(IReadOnlyList<IStoppingCondition> conditions) : IStoppingCondition
    {
        public IReadOnlyList<IStoppingCondition> Conditions => conditions;

        public TerminationReason? Evaluate(OptimizationState state)
        {
            foreach (var condition in conditions)
            {
                var reason = condition.Evaluate(state);
                if (reason is not null)
                {
                    return reason;
                }
            }

            return null;
        }
    }
}
