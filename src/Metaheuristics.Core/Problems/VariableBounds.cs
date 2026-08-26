namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>
/// 表示一个连续变量的可选包含式下界和上界。
/// </summary>
/// <remarks>这是不可变值类型，可在线程之间安全共享。</remarks>
public readonly record struct VariableBounds
{
    /// <summary>使用给定的边界创建变量范围。</summary>
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

    /// <summary>获取不限制变量取值的范围。</summary>
    public static VariableBounds Unbounded => new();

    /// <summary>获取包含式下界；无下界时为 <see langword="null"/>。</summary>
    public double? LowerBound { get; }

    /// <summary>获取包含式上界；无上界时为 <see langword="null"/>。</summary>
    public double? UpperBound { get; }

    /// <summary>获取供 Repair 实现使用的有效下界；无下界时为负无穷。</summary>
    public double EffectiveLowerBound => LowerBound ?? double.NegativeInfinity;

    /// <summary>获取供 Repair 实现使用的有效上界；无上界时为正无穷。</summary>
    public double EffectiveUpperBound => UpperBound ?? double.PositiveInfinity;

    /// <summary>判断给定值是否为有限值且位于此范围内；Core 不会自动调用此方法验证候选位置。</summary>
    /// <param name="value">要检查的变量值。</param>
    /// <returns>值满足边界且为有限值时返回 <see langword="true"/>。</returns>
    public bool Contains(double value) =>
        double.IsFinite(value)
        && (LowerBound is null || value >= LowerBound.Value)
        && (UpperBound is null || value <= UpperBound.Value);
}
