using Anastasya.Metaheuristics.Core.Comparison;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Tests.Core;

/// <summary>
/// 验证可行性优先、违背量和目标方向的评估比较规则。
/// </summary>
public sealed class EvaluationComparerTests
{
    /// <summary>
    /// 验证可行候选始终优于不可行候选，即使其目标值较差。
    /// </summary>
    [Xunit.Fact]
    public void FeasibleCandidateBeatsInfeasibleCandidateRegardlessOfObjective()
    {
        var feasible = new Evaluation(100, ConstraintEvaluation.Feasible);
        var infeasible = new Evaluation(0, new ConstraintEvaluation(0.1, 0.1, 1));

        Xunit.Assert.True(EvaluationComparer.IsBetter(feasible, infeasible, OptimizationDirection.Minimize));
    }

    /// <summary>
    /// 验证两个候选都不可行时，违背量较小者优先于目标值较优者。
    /// </summary>
    [Xunit.Fact]
    public void SmallerViolationBeatsBetterObjectiveWhenBothCandidatesAreInfeasible()
    {
        var smallerViolation = new Evaluation(100, new ConstraintEvaluation(0.1, 0.1, 1));
        var betterObjective = new Evaluation(0, new ConstraintEvaluation(0.2, 0.2, 1));

        Xunit.Assert.True(
            EvaluationComparer.IsBetter(smallerViolation, betterObjective, OptimizationDirection.Minimize));
    }

    /// <summary>
    /// 验证最小化和最大化方向会改变可行候选的目标值排序。
    /// </summary>
    /// <param name="direction">要验证的优化方向。</param>
    /// <param name="preferred">按该方向应优先的目标值。</param>
    /// <param name="other">按该方向应较差的目标值。</param>
    [Xunit.Theory]
    [Xunit.InlineData(OptimizationDirection.Minimize, 1, 2)]
    [Xunit.InlineData(OptimizationDirection.Maximize, 2, 1)]
    public void DirectionControlsObjectiveOrdering(OptimizationDirection direction, double preferred, double other)
    {
        var candidate = new Evaluation(preferred, ConstraintEvaluation.Feasible);
        var incumbent = new Evaluation(other, ConstraintEvaluation.Feasible);

        Xunit.Assert.True(EvaluationComparer.IsBetter(candidate, incumbent, direction));
    }
}
