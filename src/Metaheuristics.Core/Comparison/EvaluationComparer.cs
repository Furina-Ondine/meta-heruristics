using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Core.Comparison;

/// <summary>
/// 按可行性优先规则比较候选解评估结果。
/// </summary>
/// <remarks>
/// 比较顺序为：可行解优先；两个不可行解比较总违背量；最后按目标优化方向比较目标值。
/// </remarks>
public static class EvaluationComparer
{
    /// <summary>
    /// 判断候选评估是否严格优于当前评估。
    /// </summary>
    /// <param name="candidate">待比较的候选评估。</param>
    /// <param name="incumbent">当前最优评估。</param>
    /// <param name="direction">目标优化方向。</param>
    /// <returns>候选评估严格更优时返回 <see langword="true"/>，相等或更差时返回 <see langword="false"/>。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> 不是定义的枚举值。</exception>
    public static bool IsBetter(Evaluation candidate, Evaluation incumbent, OptimizationDirection direction)
    {
        return Compare(candidate, incumbent, direction) < 0;
    }

    /// <summary>
    /// 比较两个评估结果的优先顺序。
    /// </summary>
    /// <param name="first">第一个评估结果。</param>
    /// <param name="second">第二个评估结果。</param>
    /// <param name="direction">目标优化方向。</param>
    /// <returns>第一个结果更优时返回负数，相等时返回零，更差时返回正数。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> 不是定义的枚举值。</exception>
    public static int Compare(Evaluation first, Evaluation second, OptimizationDirection direction)
    {
        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        if (first.Constraints.IsFeasible != second.Constraints.IsFeasible)
        {
            return first.Constraints.IsFeasible ? -1 : 1;
        }

        if (!first.Constraints.IsFeasible)
        {
            var violationComparison = first.Constraints.TotalViolation.CompareTo(second.Constraints.TotalViolation);
            if (violationComparison != 0)
            {
                return violationComparison;
            }
        }

        var objectiveComparison = first.Objective.CompareTo(second.Objective);
        return direction == OptimizationDirection.Minimize
            ? objectiveComparison
            : -objectiveComparison;
    }
}
