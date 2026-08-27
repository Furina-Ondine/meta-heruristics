using Anastasya.Metaheuristics.Core.Comparison;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Tests.Core;

/// <summary>
/// 验证评估结果、比较、停止和约束聚合共享同一特殊值契约。
/// </summary>
public sealed class EvaluationSpecialValueTests
{
    [Xunit.Fact]
    public void EvaluationRejectsNaNAndAcceptsInfinities()
    {
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => new Evaluation(double.NaN, ConstraintEvaluation.Feasible));

        Xunit.Assert.Equal(
            double.NegativeInfinity,
            new Evaluation(double.NegativeInfinity, ConstraintEvaluation.Feasible).Objective);
        Xunit.Assert.Equal(
            double.PositiveInfinity,
            new Evaluation(double.PositiveInfinity, ConstraintEvaluation.Feasible).Objective);
    }

    [Xunit.Fact]
    public void ProblemAcceptsInfiniteObjectivesButRejectsNaN()
    {
        var negativeInfinity = CreateProblem(new FixedObjective(double.NegativeInfinity));
        var positiveInfinity = CreateProblem(new FixedObjective(double.PositiveInfinity));
        var nan = CreateProblem(new FixedObjective(double.NaN));

        Xunit.Assert.Equal(double.NegativeInfinity, negativeInfinity.Evaluate([0]).Objective);
        Xunit.Assert.Equal(double.PositiveInfinity, positiveInfinity.Evaluate([0]).Objective);
        Xunit.Assert.Throws<InvalidOperationException>(() => nan.Evaluate([0]));
    }

    [Xunit.Theory]
    [Xunit.InlineData(double.NaN)]
    [Xunit.InlineData(-1.0)]
    [Xunit.InlineData(double.NegativeInfinity)]
    public void ProblemRejectsInvalidConstraintValues(double violation)
    {
        var problem = CreateProblem(new FixedObjective(0), [new FixedConstraint(violation)]);

        Xunit.Assert.Throws<InvalidOperationException>(() => problem.Evaluate([0]));
    }

    [Xunit.Fact]
    public void ProblemAggregatesUnboundedAndOverflowedViolations()
    {
        var unbounded = CreateProblem(
            new FixedObjective(0),
            [new FixedConstraint(2), new FixedConstraint(double.PositiveInfinity)]);
        var overflowed = CreateProblem(
            new FixedObjective(0),
            [new FixedConstraint(double.MaxValue), new FixedConstraint(double.MaxValue)]);

        var unboundedResult = unbounded.Evaluate([0]).Constraints;
        Xunit.Assert.Equal(double.PositiveInfinity, unboundedResult.TotalViolation);
        Xunit.Assert.Equal(double.PositiveInfinity, unboundedResult.MaxViolation);
        Xunit.Assert.Equal(2, unboundedResult.ViolatedCount);

        var overflowedResult = overflowed.Evaluate([0]).Constraints;
        Xunit.Assert.Equal(double.PositiveInfinity, overflowedResult.TotalViolation);
        Xunit.Assert.Equal(double.MaxValue, overflowedResult.MaxViolation);
        Xunit.Assert.Equal(2, overflowedResult.ViolatedCount);
    }

    [Xunit.Fact]
    public void ConstraintEvaluationAcceptsConsistentPositiveInfinity()
    {
        var directInfinity = new ConstraintEvaluation(
            double.PositiveInfinity,
            double.PositiveInfinity,
            violatedCount: 1);
        var overflowedTotal = new ConstraintEvaluation(
            double.PositiveInfinity,
            double.MaxValue,
            violatedCount: 2);

        Xunit.Assert.False(directInfinity.IsFeasible);
        Xunit.Assert.Equal(double.PositiveInfinity, overflowedTotal.TotalViolation);
    }

    [Xunit.Theory]
    [Xunit.InlineData(double.NaN, 1.0)]
    [Xunit.InlineData(1.0, double.NaN)]
    [Xunit.InlineData(double.NegativeInfinity, 1.0)]
    [Xunit.InlineData(1.0, double.NegativeInfinity)]
    [Xunit.InlineData(-1.0, 1.0)]
    [Xunit.InlineData(1.0, -1.0)]
    public void ConstraintEvaluationRejectsUnorderedOrNegativeValues(double total, double maximum)
    {
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => new ConstraintEvaluation(total, maximum, violatedCount: 1));
    }

    [Xunit.Theory]
    [Xunit.InlineData(OptimizationDirection.Minimize, double.NegativeInfinity, 0.0)]
    [Xunit.InlineData(OptimizationDirection.Minimize, 0.0, double.PositiveInfinity)]
    [Xunit.InlineData(OptimizationDirection.Maximize, double.PositiveInfinity, 0.0)]
    [Xunit.InlineData(OptimizationDirection.Maximize, 0.0, double.NegativeInfinity)]
    public void InfinityOrderingFollowsOptimizationDirection(
        OptimizationDirection direction,
        double preferred,
        double other)
    {
        var candidate = new Evaluation(preferred, ConstraintEvaluation.Feasible);
        var incumbent = new Evaluation(other, ConstraintEvaluation.Feasible);

        Xunit.Assert.True(EvaluationComparer.IsBetter(candidate, incumbent, direction));
        Xunit.Assert.False(EvaluationComparer.IsBetter(incumbent, candidate, direction));
    }

    [Xunit.Theory]
    [Xunit.InlineData(OptimizationDirection.Minimize, double.NegativeInfinity)]
    [Xunit.InlineData(OptimizationDirection.Minimize, double.PositiveInfinity)]
    [Xunit.InlineData(OptimizationDirection.Maximize, double.NegativeInfinity)]
    [Xunit.InlineData(OptimizationDirection.Maximize, double.PositiveInfinity)]
    public void EqualInfinitiesCompareAsEquivalent(OptimizationDirection direction, double objective)
    {
        var evaluation = new Evaluation(objective, ConstraintEvaluation.Feasible);

        Xunit.Assert.Equal(0, EvaluationComparer.Compare(evaluation, evaluation, direction));
    }

    [Xunit.Fact]
    public void TargetObjectiveRejectsNaN()
    {
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(() => StoppingConditions.TargetObjective(double.NaN));
    }

    [Xunit.Theory]
    [Xunit.InlineData(OptimizationDirection.Minimize, double.PositiveInfinity, 0.0, true)]
    [Xunit.InlineData(OptimizationDirection.Minimize, double.NegativeInfinity, 0.0, false)]
    [Xunit.InlineData(OptimizationDirection.Minimize, double.NegativeInfinity, double.NegativeInfinity, true)]
    [Xunit.InlineData(OptimizationDirection.Maximize, double.NegativeInfinity, 0.0, true)]
    [Xunit.InlineData(OptimizationDirection.Maximize, double.PositiveInfinity, 0.0, false)]
    [Xunit.InlineData(OptimizationDirection.Maximize, double.PositiveInfinity, double.PositiveInfinity, true)]
    public void TargetObjectiveAcceptsInfinityAndUsesDirection(
        OptimizationDirection direction,
        double target,
        double objective,
        bool expectedReached)
    {
        var condition = StoppingConditions.TargetObjective(target);
        var state = CreateState(new Evaluation(objective, ConstraintEvaluation.Feasible), direction);

        var reason = condition.Evaluate(state);

        Xunit.Assert.Equal(
            expectedReached ? TerminationReason.TargetReached : null,
            reason);
    }

    [Xunit.Fact]
    public void TargetObjectiveDoesNotAcceptAnInfeasibleCandidate()
    {
        var condition = StoppingConditions.TargetObjective(double.PositiveInfinity);
        var state = CreateState(
            new Evaluation(
                double.NegativeInfinity,
                new ConstraintEvaluation(double.PositiveInfinity, double.PositiveInfinity, 1)),
            OptimizationDirection.Minimize);

        Xunit.Assert.Null(condition.Evaluate(state));
    }

    private static ContinuousProblem CreateProblem(
        IObjectiveFunction objective,
        IReadOnlyList<IConstraint>? constraints = null) =>
        new(1, objective, CandidateRepairs.DoNothing, constraints: constraints);

    private static OptimizationState CreateState(Evaluation evaluation, OptimizationDirection direction) =>
        new(0, 1, TimeSpan.Zero, evaluation, direction);

    private sealed class FixedObjective(double value) : IObjectiveFunction
    {
        public double Evaluate(ReadOnlySpan<double> position) => value;
    }

    private sealed class FixedConstraint(double violation) : IConstraint
    {
        public double EvaluateViolation(ReadOnlySpan<double> position) => violation;
    }
}
