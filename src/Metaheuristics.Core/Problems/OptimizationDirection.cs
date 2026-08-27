namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>
/// 指定可行性和约束违背量相同时，目标值之间的优劣顺序。
/// </summary>
public enum OptimizationDirection
{
    /// <summary>使目标值尽可能小。</summary>
    Minimize,

    /// <summary>使目标值尽可能大。</summary>
    Maximize,
}
