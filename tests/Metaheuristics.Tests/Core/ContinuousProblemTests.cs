using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Tests.Core;

/// <summary>
/// 验证连续优化问题的边界、评估和约束聚合契约。
/// </summary>
public sealed class ContinuousProblemTests
{
    /// <summary>
    /// 验证问题会汇总所有归一化约束违背量及其最大值和数量。
    /// </summary>
    [Xunit.Fact]
    public void EvaluateAggregatesNormalizedConstraintViolations()
    {
        var problem = new ContinuousProblem(
            [new VariableBounds(0, 10), new VariableBounds(-5, 5)],
            new SumObjective(),
            constraints: [new FixedConstraint(0), new FixedConstraint(1.5), new FixedConstraint(0.5)]);

        var evaluation = problem.Evaluate([2, 3]);

        Xunit.Assert.Equal(5, evaluation.Objective);
        Xunit.Assert.False(evaluation.Constraints.IsFeasible);
        Xunit.Assert.Equal(2, evaluation.Constraints.TotalViolation);
        Xunit.Assert.Equal(1.5, evaluation.Constraints.MaxViolation);
        Xunit.Assert.Equal(2, evaluation.Constraints.ViolatedCount);
    }

    /// <summary>
    /// 验证默认 Repair 复制边界，避免调用方后续修改影响修复语义。
    /// </summary>
    [Xunit.Fact]
    public void ConstructorCopiesDefaultRepairBoundsAndConstraints()
    {
        var bounds = new[] { new VariableBounds(0, 1) };
        var constraints = new IConstraint[] { new FixedConstraint(0) };
        var problem = new ContinuousProblem(bounds, new SumObjective(), constraints: constraints);

        bounds[0] = new VariableBounds(2, 3);
        constraints[0] = new FixedConstraint(1);

        var position = new[] { 2.0 };
        problem.Repair.Repair(position, new Random(1));
        Xunit.Assert.Equal(1, position[0]);
        Xunit.Assert.True(problem.Evaluate([0.5]).Constraints.IsFeasible);
    }

    /// <summary>
    /// 验证评估不再验证候选位置；位置责任由初始化器和 Repair 承担。
    /// </summary>
    [Xunit.Fact]
    public void EvaluateDefersPositionValidityToTheCaller()
    {
        var problem = new ContinuousProblem([new VariableBounds(0, 1)], new FixedObjective(2));

        Xunit.Assert.Equal(2, problem.Evaluate([double.NaN]).Objective);
        Xunit.Assert.Equal(2, problem.Evaluate([1.1]).Objective);
    }

    /// <summary>
    /// 验证默认 Clamp Repair 截断有界分量，并保留 NaN 和无界分量。
    /// </summary>
    [Xunit.Fact]
    public void DefaultClampRepairHandlesBoundsAndSpecialValues()
    {
        var problem = new ContinuousProblem(
            [new VariableBounds(0, 1), new VariableBounds(null, 2), new VariableBounds(-2, null), VariableBounds.Unbounded, new VariableBounds(0, 1)],
            new SumObjective());
        var position = new[] { -1.0, double.PositiveInfinity, double.NegativeInfinity, double.PositiveInfinity, double.NaN };

        problem.Repair.Repair(position, new Random(1));

        Xunit.Assert.Equal([0, 2, -2, double.PositiveInfinity], position[..4]);
        Xunit.Assert.True(double.IsNaN(position[4]));
    }

    /// <summary>
    /// 验证镜像、随机回退和 DoNothing Repair 的公开语义。
    /// </summary>
    [Xunit.Fact]
    public void BuiltInRepairsFollowTheirDocumentedSemantics()
    {
        var bounds = new[] { new VariableBounds(0, 10), VariableBounds.Unbounded };
        var reflected = new[] { 12.0, double.PositiveInfinity };
        CandidateRepairs.Reflect(bounds).Repair(reflected, new Random(1));
        Xunit.Assert.Equal([8, double.PositiveInfinity], reflected);

        var randomFirst = new[] { -1.0, double.NaN };
        var randomSecond = new[] { -1.0, double.NaN };
        CandidateRepairs.RandomReset(bounds).Repair(randomFirst, new Random(42));
        CandidateRepairs.RandomReset(bounds).Repair(randomSecond, new Random(42));
        Xunit.Assert.InRange(randomFirst[0], 0, 10);
        Xunit.Assert.Equal(randomFirst[0], randomSecond[0]);
        Xunit.Assert.True(double.IsNaN(randomFirst[1]));

        var unchanged = new[] { -1.0, double.PositiveInfinity };
        CandidateRepairs.DoNothing.Repair(unchanged, new Random(1));
        Xunit.Assert.Equal([-1, double.PositiveInfinity], unchanged);
    }

    /// <summary>
    /// 验证目标函数返回非有限值时评估会失败。
    /// </summary>
    [Xunit.Fact]
    public void EvaluateRejectsInvalidObjectiveResult()
    {
        var problem = new ContinuousProblem([VariableBounds.Unbounded], new NonFiniteObjective());

        Xunit.Assert.Throws<InvalidOperationException>(() => problem.Evaluate([0]));
    }

    /// <summary>
    /// 用于验证目标评估的确定性求和函数。
    /// </summary>
    private sealed class SumObjective : IObjectiveFunction
    {
        /// <summary>
        /// 返回候选位置各维度的和。
        /// </summary>
        /// <param name="position">待计算的候选位置。</param>
        /// <returns>候选位置的总和。</returns>
        public double Evaluate(ReadOnlySpan<double> position)
        {
            var sum = 0.0;
            foreach (var value in position)
            {
                sum += value;
            }

            return sum;
        }
    }

    /// <summary>
    /// 用于触发非有限目标值校验的测试替身。
    /// </summary>
    private sealed class NonFiniteObjective : IObjectiveFunction
    {
        /// <summary>
        /// 返回非有限值，用于验证问题拒绝无效目标结果。
        /// </summary>
        /// <param name="position">未使用的候选位置。</param>
        /// <returns>始终为 <see cref="double.NaN"/>。</returns>
        public double Evaluate(ReadOnlySpan<double> position) => double.NaN;
    }

    private sealed class FixedObjective(double value) : IObjectiveFunction
    {
        public double Evaluate(ReadOnlySpan<double> position) => value;
    }

    /// <summary>
    /// 用固定违背量隔离并验证约束聚合逻辑。
    /// </summary>
    private sealed class FixedConstraint(double violation) : IConstraint
    {
        /// <summary>
        /// 返回构造时指定的固定违背量。
        /// </summary>
        /// <param name="position">未使用的候选位置。</param>
        /// <returns>固定的归一化违背量。</returns>
        public double EvaluateViolation(ReadOnlySpan<double> position) => violation;
    }
}
