namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>
/// 表示一次候选解评估得到的约束违背汇总。
/// </summary>
/// <remarks>这是不可变值类型，可在线程之间安全共享。</remarks>
public readonly record struct ConstraintEvaluation
{
    /// <summary>使用违背量汇总创建约束评估结果。</summary>
    /// <param name="totalViolation">所有违背约束的归一化违背量之和；允许 <see cref="double.PositiveInfinity"/>。</param>
    /// <param name="maxViolation">单个约束的最大归一化违背量；允许 <see cref="double.PositiveInfinity"/>。</param>
    /// <param name="violatedCount">产生正违背量的约束数量。</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="totalViolation"/>、<paramref name="maxViolation"/> 为 <see cref="double.NaN"/> 或负值，或 <paramref name="violatedCount"/> 为负数。</exception>
    /// <exception cref="ArgumentException">违背量与违背约束数量之间的汇总关系不一致。</exception>
    public ConstraintEvaluation(double totalViolation, double maxViolation, int violatedCount)
    {
        if (double.IsNaN(totalViolation) || totalViolation < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalViolation), "The total constraint violation must be non-negative and cannot be NaN.");
        }

        if (double.IsNaN(maxViolation) || maxViolation < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxViolation), "The maximum constraint violation must be non-negative and cannot be NaN.");
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

    /// <summary>获取表示没有约束违背的结果。</summary>
    public static ConstraintEvaluation Feasible => default;

    /// <summary>获取候选解是否满足所有约束。</summary>
    public bool IsFeasible => ViolatedCount == 0;

    /// <summary>获取所有违背约束的归一化违背量之和。</summary>
    public double TotalViolation { get; }

    /// <summary>获取单个约束的最大归一化违背量。</summary>
    public double MaxViolation { get; }

    /// <summary>获取产生正违背量的约束数量。</summary>
    public int ViolatedCount { get; }
}
