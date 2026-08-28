namespace Anastasya.Metaheuristics.Algorithms.Pso;

/// <summary>
/// 配置连续粒子群优化器的种群、速度和惯性/学习系数。
/// </summary>
/// <remarks>
/// 候选位置范围由调用方组合的 <c>ICandidateRepair</c> 持有，不在算法配置中重复声明。
/// <see cref="PsoOptimizer"/> 在构造时复制并一次性验证本配置；后续运行不会重新读取调用方记录。
/// <see cref="VelocityLowerBound"/> 不得超过 <see cref="VelocityUpperBound"/>，且区间宽度必须为有限值；
/// <see cref="MinimumInertia"/> 不得超过 <see cref="InitialInertia"/>。
/// </remarks>
public sealed record PsoOptimizerOptions
{
    /// <summary>获取粒子数量；必须为正数。</summary>
    public int PopulationSize { get; init; } = 100;

    /// <summary>获取每个速度分量的有限包含式下界。</summary>
    public double VelocityLowerBound { get; init; } = -1;

    /// <summary>获取每个速度分量的有限包含式上界。</summary>
    public double VelocityUpperBound { get; init; } = 1;

    /// <summary>获取第零轮使用的惯性权重；必须为非负有限值。</summary>
    public double InitialInertia { get; init; } = 0.79;

    /// <summary>获取惯性权重可衰减到的下限；必须为非负有限值。</summary>
    public double MinimumInertia { get; init; } = 0.4;

    /// <summary>获取每轮乘到惯性权重上的有限衰减系数；必须位于 <c>(0, 1]</c>。</summary>
    public double InertiaDecay { get; init; } = 0.975;

    /// <summary>获取个体历史最佳位置的学习系数；必须为非负有限值。</summary>
    public double CognitiveCoefficient { get; init; } = 2.44;

    /// <summary>获取全局历史最佳位置的学习系数；必须为非负有限值。</summary>
    public double SocialCoefficient { get; init; } = 2.44;
}
