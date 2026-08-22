using System.Collections.ObjectModel;

namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>
/// 指定目标函数的优化方向。
/// </summary>
public enum OptimizationDirection
{
    /// <summary>
    /// 使目标值尽可能小。
    /// </summary>
    Minimize,

    /// <summary>
    /// 使目标值尽可能大。
    /// </summary>
    Maximize,
}

/// <summary>
/// 表示一个连续变量的可选下界和上界。
/// </summary>
public readonly record struct VariableBounds
{
    /// <summary>
    /// 使用给定的边界创建变量范围。
    /// </summary>
    /// <param name="lowerBound">可选的包含式下界；为 <see langword="null"/> 时表示无下界。</param>
    /// <param name="upperBound">可选的包含式上界；为 <see langword="null"/> 时表示无上界。</param>
    /// <exception cref="ArgumentOutOfRangeException">指定的边界不是有限值。</exception>
    /// <exception cref="ArgumentException">下界大于上界。</exception>
    public VariableBounds(double? lowerBound = null, double? upperBound = null)
    {
        if (lowerBound is not null && !double.IsFinite(lowerBound.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(lowerBound), "A specified lower bound must be finite.");
        }

        if (upperBound is not null && !double.IsFinite(upperBound.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(upperBound), "A specified upper bound must be finite.");
        }

        if (lowerBound > upperBound)
        {
            throw new ArgumentException("The lower bound cannot exceed the upper bound.", nameof(lowerBound));
        }

        LowerBound = lowerBound;
        UpperBound = upperBound;
    }

    /// <summary>
    /// 获取不限制变量取值的范围。
    /// </summary>
    public static VariableBounds Unbounded => new();

    /// <summary>
    /// 获取包含式下界；无下界时为 <see langword="null"/>。
    /// </summary>
    public double? LowerBound { get; }

    /// <summary>
    /// 获取包含式上界；无上界时为 <see langword="null"/>。
    /// </summary>
    public double? UpperBound { get; }

    /// <summary>
    /// 有效下界，用于快速比较
    /// </summary>
    public double EffectiveLowerBound =>
        LowerBound ?? double.NegativeInfinity;

    /// <summary>
    /// 有效上界，用于快速比较
    /// </summary>
    public double EffectiveUpperBound =>
        UpperBound ?? double.PositiveInfinity;

    /// <summary>
    /// 判断给定值是否为有限值且位于此范围内。
    /// </summary>
    /// <param name="value">要检查的变量值。</param>
    /// <returns>值满足边界且为有限值时返回 <see langword="true"/>，否则返回 <see langword="false"/>。</returns>
    public bool Contains(double value)
    {
        return double.IsFinite(value)
            && (LowerBound is null || value >= LowerBound.Value)
            && (UpperBound is null || value <= UpperBound.Value);
    }
}

/// <summary>
/// 表示一次候选解评估得到的约束违背汇总。
/// </summary>
public readonly record struct ConstraintEvaluation
{
    /// <summary>
    /// 使用违背量汇总创建约束评估结果。
    /// </summary>
    /// <param name="totalViolation">所有违背约束的归一化违背量之和。</param>
    /// <param name="maxViolation">单个约束的最大归一化违背量。</param>
    /// <param name="violatedCount">产生正违背量的约束数量。</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="totalViolation"/>、<paramref name="maxViolation"/> 不是有限非负值，或 <paramref name="violatedCount"/> 为负数。</exception>
    /// <exception cref="ArgumentException">违背量与违背约束数量之间的汇总关系不一致。</exception>
    public ConstraintEvaluation(double totalViolation, double maxViolation, int violatedCount)
    {
        if (!double.IsFinite(totalViolation) || totalViolation < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalViolation),
                "The total constraint violation must be finite and non-negative.");
        }

        if (!double.IsFinite(maxViolation) || maxViolation < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxViolation),
                "The maximum constraint violation must be finite and non-negative.");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(violatedCount);

        if (violatedCount == 0 && (totalViolation != 0 || maxViolation != 0))
        {
            throw new ArgumentException("A feasible evaluation cannot report a positive violation.");
        }

        if (violatedCount > 0 && (totalViolation <= 0 || maxViolation <= 0 || maxViolation > totalViolation))
        {
            throw new ArgumentException("An infeasible evaluation must report consistent positive violations.");
        }

        TotalViolation = totalViolation;
        MaxViolation = maxViolation;
        ViolatedCount = violatedCount;
    }

    /// <summary>
    /// 获取表示没有约束违背的结果。
    /// </summary>
    public static ConstraintEvaluation Feasible => default;

    /// <summary>
    /// 获取候选解是否满足所有约束。
    /// </summary>
    public bool IsFeasible => ViolatedCount == 0;

    /// <summary>
    /// 获取所有违背约束的归一化违背量之和。
    /// </summary>
    public double TotalViolation { get; }

    /// <summary>
    /// 获取单个约束的最大归一化违背量。
    /// </summary>
    public double MaxViolation { get; }

    /// <summary>
    /// 获取产生正违背量的约束数量。
    /// </summary>
    public int ViolatedCount { get; }
}

/// <summary>
/// 表示一个候选位置对应的目标值和约束评估结果。
/// </summary>
public readonly record struct Evaluation
{
    /// <summary>
    /// 创建一次候选解评估结果。
    /// </summary>
    /// <param name="objective">目标函数返回的有限目标值。</param>
    /// <param name="constraints">候选位置的约束评估结果。</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="objective"/> 不是有限值。</exception>
    public Evaluation(double objective, ConstraintEvaluation constraints)
    {
        if (!double.IsFinite(objective))
        {
            throw new ArgumentOutOfRangeException(nameof(objective), "The objective value must be finite.");
        }

        Objective = objective;
        Constraints = constraints;
    }

    /// <summary>
    /// 获取目标函数值。
    /// </summary>
    public double Objective { get; }

    /// <summary>
    /// 获取约束违背汇总。
    /// </summary>
    public ConstraintEvaluation Constraints { get; }
}

/// <summary>
/// 定义单个候选位置的同步目标函数。
/// </summary>
public interface IObjectiveFunction
{
    /// <summary>
    /// 计算给定候选位置的目标值。
    /// </summary>
    /// <param name="position">长度必须等于问题维度的候选位置。</param>
    /// <returns>候选位置对应的有限目标值。</returns>
    /// <exception cref="ArgumentException">候选位置维度不正确。</exception>
    /// <exception cref="ArgumentOutOfRangeException">候选位置包含非有限值或越过边界。</exception>
    double Evaluate(ReadOnlySpan<double> position);
}

/// <summary>
/// 定义一个返回归一化约束违背量的约束。
/// </summary>
public interface IConstraint
{
    /// <summary>
    /// 计算候选位置对该约束的归一化违背量。
    /// </summary>
    /// <param name="position">待检查的候选位置。</param>
    /// <returns>非负且有限的违背量；零表示满足约束。</returns>
    /// <exception cref="ArgumentException">候选位置维度不正确。</exception>
    /// <exception cref="ArgumentOutOfRangeException">候选位置包含非有限值或越过边界。</exception>
    double EvaluateViolation(ReadOnlySpan<double> position);
}

/// <summary>
/// 描述连续单目标优化问题及其评估规则。
/// </summary>
/// <remarks>
/// 实例在构造后只保存问题定义；评估不会修改候选位置。候选位置必须先通过变量边界检查，
/// 然后目标值和各约束违背量才会被汇总为 <see cref="Evaluation"/>。
/// </remarks>
public sealed class ContinuousProblem
{
    private readonly VariableBounds[] _bounds;
    private readonly IConstraint[] _constraints;
    private readonly ReadOnlyCollection<IConstraint> _readOnlyConstraints;

    /// <summary>
    /// 创建一个连续优化问题。
    /// </summary>
    /// <param name="bounds">每个维度的变量范围；至少包含一个维度。</param>
    /// <param name="objective">用于计算目标值的目标函数。</param>
    /// <param name="direction">目标优化方向。</param>
    /// <param name="constraints">可选约束集合；集合中的每一项都不能为 <see langword="null"/>。</param>
    /// <param name="repair">可选的候选修复策略。</param>
    /// <exception cref="ArgumentNullException"><paramref name="bounds"/> 或 <paramref name="objective"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">没有维度，或约束集合包含 <see langword="null"/> 项。</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/> 不是定义的枚举值。</exception>
    public ContinuousProblem(
        IReadOnlyList<VariableBounds> bounds,
        IObjectiveFunction objective,
        OptimizationDirection direction = OptimizationDirection.Minimize,
        IReadOnlyList<IConstraint>? constraints = null,
        ICandidateRepair? repair = null)
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
            _constraints[i] = constraints![i]
                ?? throw new ArgumentException("A constraint cannot be null.", nameof(constraints));
        }

        _readOnlyConstraints = Array.AsReadOnly(_constraints);
        Objective = objective;
        Direction = direction;
        Repair = repair;
    }

    /// <summary>
    /// 获取候选位置的维度。
    /// </summary>
    public int Dimension => _bounds.Length;

    /// <summary>
    /// 获取各维度的变量范围，顺序与候选位置一致。
    /// </summary>
    public ReadOnlySpan<VariableBounds> Bounds => _bounds;

    /// <summary>
    /// 获取目标函数。
    /// </summary>
    public IObjectiveFunction Objective { get; }

    /// <summary>
    /// 获取目标优化方向。
    /// </summary>
    public OptimizationDirection Direction { get; }

    /// <summary>
    /// 获取问题约束的只读视图。
    /// </summary>
    public IReadOnlyList<IConstraint> Constraints => _readOnlyConstraints;

    /// <summary>
    /// 获取可选的候选修复策略。
    /// </summary>
    public ICandidateRepair? Repair { get; }

    /// <summary>
    /// 评估一个候选位置。
    /// </summary>
    /// <param name="position">长度必须等于 <see cref="Dimension"/> 且满足变量边界的候选位置。</param>
    /// <returns>包含目标值和约束违背汇总的评估结果。</returns>
    /// <exception cref="ArgumentException">候选位置维度不正确。</exception>
    /// <exception cref="ArgumentOutOfRangeException">候选位置包含非有限值或越过变量边界。</exception>
    /// <exception cref="InvalidOperationException">目标函数或约束返回了无效数值。</exception>
    public Evaluation Evaluate(ReadOnlySpan<double> position)
    {
        ValidatePosition(position);
        var objective = Objective.Evaluate(position);
        return CreateEvaluation(objective, position);
    }

    /// <summary>
    /// 验证候选位置的维度、有限性和变量边界。
    /// </summary>
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
                throw new ArgumentOutOfRangeException(
                    nameof(position),
                    $"Candidate value at dimension {i} is non-finite or outside its bounds.");
            }
        }
    }

    /// <summary>
    /// 验证目标值并汇总候选位置的全部约束违背量。
    /// </summary>
    /// <param name="objective">目标函数返回的候选位置目标值。</param>
    /// <param name="position">已经通过维度和边界验证的候选位置。</param>
    /// <returns>目标值与约束违背量组成的评估结果。</returns>
    /// <exception cref="InvalidOperationException">目标值或约束违背量不是有效有限数值，或总违背量溢出。</exception>
    private Evaluation CreateEvaluation(double objective, ReadOnlySpan<double> position)
    {
        if (!double.IsFinite(objective))
        {
            throw new InvalidOperationException("The objective function returned a non-finite value.");
        }

        // 违背量先在 Core 中统一汇总，保持约束判定与候选比较、修复策略相互独立。
        var totalViolation = 0.0;
        var maxViolation = 0.0;
        var violatedCount = 0;

        foreach (var constraint in _constraints)
        {
            var violation = constraint.EvaluateViolation(position);
            if (!double.IsFinite(violation) || violation < 0)
            {
                throw new InvalidOperationException(
                    "A constraint returned a non-finite or negative normalized violation.");
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

        return new Evaluation(
            objective,
            violatedCount == 0
                ? ConstraintEvaluation.Feasible
                : new ConstraintEvaluation(totalViolation, maxViolation, violatedCount));
    }
}
