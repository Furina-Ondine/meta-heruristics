namespace Anastasya.Metaheuristics.Algorithms.Bat;

/// <summary>
/// 配置连续蝙蝠优化器的种群规模、速度、频率、响度和脉冲发射率。
/// </summary>
/// <remarks>
/// 候选位置范围由调用方组合的 <c>ICandidateRepair</c> 持有，不在算法配置中重复声明。
/// <see cref="BatOptimizer"/> 构造时复制并一次性验证配置；后续运行不会重新读取调用方记录。
/// <see cref="VelocityLowerBound"/>、<see cref="FrequencyLowerBound"/>、
/// <see cref="InitialLoudnessLowerBound"/> 和 <see cref="InitialPulseRateLowerBound"/>
/// 均不得超过各自对应的上界；速度、频率和响度区间的宽度还必须为有限值。
/// </remarks>
public sealed record BatOptimizerOptions
{
    /// <summary>
    /// 获取蝙蝠种群数量；必须为正数。
    /// </summary>
    public int PopulationSize { get; init; } = 100;

    /// <summary>
    /// 获取每个速度分量的有限包含式下界。
    /// </summary>
    public double VelocityLowerBound { get; init; } = -2;

    /// <summary>
    /// 获取每个速度分量的有限包含式上界。
    /// </summary>
    public double VelocityUpperBound { get; init; } = 2;

    /// <summary>
    /// 获取每个频率分量的非负有限包含式下界。
    /// </summary>
    public double FrequencyLowerBound { get; init; }

    /// <summary>
    /// 获取每个频率分量的有限包含式上界。
    /// </summary>
    public double FrequencyUpperBound { get; init; } = 2;

    /// <summary>
    /// 获取初始响度的非负有限包含式下界。
    /// </summary>
    public double InitialLoudnessLowerBound { get; init; } = 0.7;

    /// <summary>
    /// 获取初始响度的有限包含式上界。
    /// </summary>
    public double InitialLoudnessUpperBound { get; init; } = 1;

    /// <summary>
    /// 获取初始脉冲发射率的有限包含式下界；必须位于 <c>[0, 1]</c>。
    /// </summary>
    public double InitialPulseRateLowerBound { get; init; }

    /// <summary>
    /// 获取初始脉冲发射率的有限包含式上界；必须位于 <c>[0, 1]</c>。
    /// </summary>
    public double InitialPulseRateUpperBound { get; init; } = 0.4;

    /// <summary>
    /// 获取接受一次坐标更新后乘到响度上的有限衰减系数；必须位于 <c>(0, 1]</c>。
    /// </summary>
    public double LoudnessDecay { get; init; } = 0.98;

    /// <summary>
    /// 获取脉冲发射率趋近其初始值时使用的有限增长系数；必须大于零。
    /// </summary>
    public double PulseRateGrowth { get; init; } = 0.98;
}
