namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>
/// 定义如何生成一次运行的初始候选位置。
/// </summary>
public interface ICandidateInitializer
{
    /// <summary>
    /// 将一个候选位置初始化为问题允许的起始状态。
    /// </summary>
    /// <param name="position">要写入的候选位置缓冲区。</param>
    /// <param name="problem">候选位置所属的连续优化问题。</param>
    /// <param name="random">当前运行独占的随机数生成器。</param>
    /// <exception cref="ArgumentException">候选位置维度与问题维度不匹配。</exception>
    /// <exception cref="ArgumentOutOfRangeException">初始化后的位置包含非有限值或越过变量边界。</exception>
    void Initialize(Span<double> position, ContinuousProblem problem, Random random);
}

/// <summary>
/// 定义如何将候选位置修复到问题允许的范围内。
/// </summary>
/// <remarks>
/// 修复策略是可选的；修复完成后 Core 会再次验证位置。修复不改变约束比较语义。
/// </remarks>
public interface ICandidateRepair
{
    /// <summary>
    /// 就地修复候选位置。
    /// </summary>
    /// <param name="position">要就地修改的候选位置缓冲区。</param>
    /// <param name="problem">候选位置所属的连续优化问题。</param>
    /// <param name="random">当前运行独占的随机数生成器。</param>
    /// <exception cref="ArgumentException">候选位置维度与问题维度不匹配。</exception>
    /// <exception cref="ArgumentOutOfRangeException">修复后的位置包含非有限值或越过变量边界。</exception>
    void Repair(Span<double> position, ContinuousProblem problem, Random random);
}
