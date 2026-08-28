namespace Anastasya.Metaheuristics.Algorithms.Cuckoo;

/// <summary>配置连续布谷鸟优化器的 Lévy、遗弃和坐标尺度参数。</summary>
/// <remarks>
/// <see cref="BaseLevyScale"/> 与 <see cref="AbandonmentPerturbationScale"/> 直接以候选位置的坐标单位表示；
/// 它们不读取调用方 Repair 私有的边界。<see cref="CuckooOptimizer"/> 在构造时复制并一次性验证本配置。
/// <see cref="LevyCandidateCount"/> 不得超过 <see cref="PopulationSize"/>。
/// </remarks>
public sealed record CuckooOptimizerOptions
{
    /// <summary>获取巢的数量；必须为正数。</summary>
    public int PopulationSize { get; init; } = 100;

    /// <summary>获取每轮遗弃最差巢的有限比例；必须位于 <c>[0, 1]</c>。</summary>
    public double AbandonmentRate { get; init; } = 0.25;

    /// <summary>获取 Mantegna Lévy 分布的有限稳定指数；必须位于 <c>(0, 2)</c>。</summary>
    public double LevyExponent { get; init; } = 1.5;

    /// <summary>获取 Mantegna 分子正态样本的尺度；必须为正有限值。</summary>
    public double GaussianScale { get; init; } = 1;

    /// <summary>获取第零轮 Lévy 步长的坐标尺度；必须为正有限值。</summary>
    public double BaseLevyScale { get; init; } = 10;

    /// <summary>获取遗弃阶段随机扰动的坐标尺度；必须为非负有限值，零表示不添加独立扰动。</summary>
    public double AbandonmentPerturbationScale { get; init; } = 0.5;

    /// <summary>获取每轮生成 Lévy 候选的巢数量；必须至少为一。</summary>
    public int LevyCandidateCount { get; init; } = 2;

    /// <summary>获取 Lévy 步长从初始尺度线性衰减到 10% 所需的迭代数；必须为正数。</summary>
    public int StepDecayIterations { get; init; } = 100;
}
