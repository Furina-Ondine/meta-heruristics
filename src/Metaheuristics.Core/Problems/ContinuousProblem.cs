using System.Collections.ObjectModel;

namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>描述连续单目标优化问题及其评估规则。</summary>
/// <remarks>
/// 实例保存问题定义和维度。变量边界只由 Repair 感知；评估不验证或修改候选位置，
/// 但会验证目标值和约束违背量。调用方负责组合正确的初始化器与 Repair。
/// </remarks>
public sealed class ContinuousProblem
{
    private readonly IConstraint[] _constraints;
    private readonly ReadOnlyCollection<IConstraint> _readOnlyConstraints;

    /// <summary>创建一个连续优化问题。</summary>
    /// <param name="dimension">候选位置的维度；必须大于零。</param>
    /// <param name="objective">用于计算目标值的目标函数。</param>
    /// <param name="repair">候选修复策略；省略时使用边界为 [0, 10] 的 Clamp Repair。</param>
    /// <param name="direction">目标优化方向。</param>
    /// <param name="constraints">可选约束集合；集合中的每一项都不能为 <see langword="null"/>。</param>
    /// <exception cref="ArgumentNullException"><paramref name="objective"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">约束集合包含 <see langword="null"/> 项。</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dimension"/> 不是正数，或 <paramref name="direction"/> 不是定义的枚举值。</exception>
    public ContinuousProblem(
        int dimension,
        IObjectiveFunction objective,
        ICandidateRepair? repair = null,
        OptimizationDirection direction = OptimizationDirection.Minimize,
        IReadOnlyList<IConstraint>? constraints = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimension);
        ArgumentNullException.ThrowIfNull(objective);

        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        _constraints = constraints is null ? [] : new IConstraint[constraints.Count];
        for (var index = 0; index < _constraints.Length; index++)
        {
            _constraints[index] = constraints![index]
                ?? throw new ArgumentException("A constraint cannot be null.", nameof(constraints));
        }

        _readOnlyConstraints = Array.AsReadOnly(_constraints);
        Dimension = dimension;
        Objective = objective;
        Direction = direction;
        Repair = repair ?? CandidateRepairs.Clamp(0, 10);
    }

    /// <summary>获取候选位置的维度。</summary>
    public int Dimension { get; }

    /// <summary>获取目标函数。</summary>
    public IObjectiveFunction Objective { get; }

    /// <summary>获取目标优化方向。</summary>
    public OptimizationDirection Direction { get; }

    /// <summary>获取问题约束的只读视图。</summary>
    public IReadOnlyList<IConstraint> Constraints => _readOnlyConstraints;

    /// <summary>获取当前问题使用的候选 Repair。</summary>
    public ICandidateRepair Repair { get; }

    /// <summary>评估一个由调用方初始化器和 Repair 准备的候选位置。</summary>
    /// <param name="position">待评估的候选位置。</param>
    /// <returns>包含目标值和约束违背汇总的评估结果。</returns>
    /// <exception cref="InvalidOperationException">目标函数或约束返回了无效数值。</exception>
    public Evaluation Evaluate(ReadOnlySpan<double> position) =>
        CreateEvaluation(Objective.Evaluate(position), position);

    private Evaluation CreateEvaluation(double objective, ReadOnlySpan<double> position)
    {
        if (double.IsNaN(objective))
        {
            throw new InvalidOperationException("The objective function returned NaN.");
        }

        var totalViolation = 0.0;
        var maxViolation = 0.0;
        var violatedCount = 0;
        foreach (var constraint in _constraints)
        {
            var violation = constraint.EvaluateViolation(position);
            if (double.IsNaN(violation) || violation < 0)
            {
                throw new InvalidOperationException("A constraint returned NaN or a negative normalized violation.");
            }

            if (violation == 0)
            {
                continue;
            }

            totalViolation += violation;
            maxViolation = Math.Max(maxViolation, violation);
            violatedCount++;
        }

        return new Evaluation(
            objective,
            violatedCount == 0
                ? ConstraintEvaluation.Feasible
                : new ConstraintEvaluation(totalViolation, maxViolation, violatedCount));
    }
}
