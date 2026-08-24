using System.Collections.ObjectModel;

namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>描述连续单目标优化问题及其评估规则。</summary>
/// <remarks>
/// 实例在构造后只保存问题定义；评估不会修改候选位置。候选位置必须先通过变量边界检查，
/// 然后目标值和各约束违背量才会被汇总为 <see cref="Evaluation"/>。
/// </remarks>
public sealed class ContinuousProblem
{
    private readonly VariableBounds[] _bounds;
    private readonly IConstraint[] _constraints;
    private readonly ReadOnlyCollection<IConstraint> _readOnlyConstraints;

    /// <summary>创建一个连续优化问题。</summary>
    /// <param name="bounds">每个维度的变量范围；至少包含一个维度。</param>
    /// <param name="objective">用于计算目标值的目标函数。</param>
    /// <param name="direction">目标优化方向。</param>
    /// <param name="constraints">可选约束集合；集合中的每一项都不能为 <see langword="null"/>。</param>
    /// <param name="repair">可选的候选修复策略。</param>
    /// <exception cref="ArgumentNullException"><paramref name="bounds"/> 或 <paramref name="objective"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">没有维度，或约束集合包含 <see langword="null"/> 项。</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> 不是定义的枚举值。</exception>
    public ContinuousProblem(IReadOnlyList<VariableBounds> bounds, IObjectiveFunction objective, OptimizationDirection direction = OptimizationDirection.Minimize, IReadOnlyList<IConstraint>? constraints = null, ICandidateRepair? repair = null)
    {
        ArgumentNullException.ThrowIfNull(bounds);
        ArgumentNullException.ThrowIfNull(objective);
        if (bounds.Count == 0)
        {
            throw new ArgumentException("A problem must contain at least one dimension.", nameof(bounds));
        }

        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(nameof(direction));
        }

        _bounds = new VariableBounds[bounds.Count];
        for (var i = 0; i < bounds.Count; i++)
        {
            _bounds[i] = bounds[i];
        }

        _constraints = constraints is null ? [] : new IConstraint[constraints.Count];
        for (var i = 0; i < _constraints.Length; i++)
        {
            _constraints[i] = constraints![i] ?? throw new ArgumentException("A constraint cannot be null.", nameof(constraints));
        }

        _readOnlyConstraints = Array.AsReadOnly(_constraints);
        Objective = objective;
        Direction = direction;
        Repair = repair;
    }

    /// <summary>获取候选位置的维度。</summary>
    public int Dimension => _bounds.Length;

    /// <summary>获取各维度的变量范围，顺序与候选位置一致。</summary>
    public ReadOnlySpan<VariableBounds> Bounds => _bounds;

    /// <summary>获取目标函数。</summary>
    public IObjectiveFunction Objective { get; }

    /// <summary>获取目标优化方向。</summary>
    public OptimizationDirection Direction { get; }

    /// <summary>获取问题约束的只读视图。</summary>
    public IReadOnlyList<IConstraint> Constraints => _readOnlyConstraints;

    /// <summary>获取可选的候选修复策略。</summary>
    public ICandidateRepair? Repair { get; }

    /// <summary>评估一个候选位置。</summary>
    /// <param name="position">长度必须等于 <see cref="Dimension"/> 且满足变量边界的候选位置。</param>
    /// <returns>包含目标值和约束违背汇总的评估结果。</returns>
    /// <exception cref="ArgumentException">候选位置维度不正确。</exception>
    /// <exception cref="ArgumentOutOfRangeException">候选位置包含非有限值或越过变量边界。</exception>
    /// <exception cref="InvalidOperationException">目标函数或约束返回了无效数值。</exception>
    public Evaluation Evaluate(ReadOnlySpan<double> position)
    {
        ValidatePosition(position);
        return CreateEvaluation(Objective.Evaluate(position), position);
    }

    /// <summary>验证候选位置的维度、有限性和变量边界。</summary>
    /// <param name="position">要验证的候选位置。</param>
    /// <exception cref="ArgumentException">候选位置维度不正确。</exception>
    /// <exception cref="ArgumentOutOfRangeException">候选位置包含非有限值或越过变量边界。</exception>
    public void ValidatePosition(ReadOnlySpan<double> position)
    {
        if (position.Length != Dimension)
        {
            throw new ArgumentException("The candidate dimension does not match the problem dimension.", nameof(position));
        }

        for (var i = 0; i < position.Length; i++)
        {
            if (!_bounds[i].Contains(position[i]))
            {
                throw new ArgumentOutOfRangeException(nameof(position), $"Candidate value at dimension {i} is non-finite or outside its bounds.");
            }
        }
    }

    private Evaluation CreateEvaluation(double objective, ReadOnlySpan<double> position)
    {
        if (!double.IsFinite(objective))
        {
            throw new InvalidOperationException("The objective function returned a non-finite value.");
        }

        // 在 Core 统一汇总违背量，保持约束判定与候选比较、修复策略相互独立。
        var totalViolation = 0.0;
        var maxViolation = 0.0;
        var violatedCount = 0;
        foreach (var constraint in _constraints)
        {
            var violation = constraint.EvaluateViolation(position);
            if (!double.IsFinite(violation) || violation < 0)
            {
                throw new InvalidOperationException("A constraint returned a non-finite or negative normalized violation.");
            }

            if (violation == 0)
            {
                continue;
            }

            totalViolation += violation;
            if (!double.IsFinite(totalViolation))
            {
                throw new InvalidOperationException("The total constraint violation overflowed.");
            }

            maxViolation = Math.Max(maxViolation, violation);
            violatedCount++;
        }

        return new Evaluation(objective, violatedCount == 0 ? ConstraintEvaluation.Feasible : new ConstraintEvaluation(totalViolation, maxViolation, violatedCount));
    }
}
