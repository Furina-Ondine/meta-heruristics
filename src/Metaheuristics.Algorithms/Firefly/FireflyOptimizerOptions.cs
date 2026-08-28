namespace Anastasya.Metaheuristics.Algorithms.Firefly;

/// <summary>配置连续萤火虫优化器的种群、吸引度和随机步长。</summary>
/// <remarks>
/// 候选位置范围由调用方组合的 <c>ICandidateRepair</c> 持有，不在算法配置中重复声明。
/// <see cref="FireflyOptimizer"/> 在构造时复制并一次性验证本配置；后续运行不会重新读取调用方记录。
/// </remarks>
public sealed record FireflyOptimizerOptions
{
    /// <summary>获取萤火虫数量；必须为正数。</summary>
    public int PopulationSize { get; init; } = 100;

    /// <summary>获取零距离时的基础吸引度；必须为非负有限值。</summary>
    public double BaseAttractiveness { get; init; } = 0.5;

    /// <summary>获取按距离平方衰减吸引度的系数；必须为非负有限值。</summary>
    public double DistanceAttenuation { get; init; } = 12;

    /// <summary>获取第零轮随机步长的坐标尺度；必须为非负有限值。</summary>
    public double InitialRandomStep { get; init; } = 0.2;

    /// <summary>获取每轮乘到随机步长上的有限衰减系数；必须位于 <c>(0, 1]</c>。</summary>
    public double RandomStepDecay { get; init; } = 0.97;
}
