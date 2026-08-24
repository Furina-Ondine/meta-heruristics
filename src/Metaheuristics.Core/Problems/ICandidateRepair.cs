namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>定义如何将候选位置修复到问题允许的范围内。</summary>
/// <remarks>修复策略是可选的；修复完成后 Core 会验证位置。它不改变约束比较语义。</remarks>
public interface ICandidateRepair
{
    /// <summary>就地修复候选位置。</summary>
    /// <param name="position">要就地修改的候选位置缓冲区。</param>
    /// <param name="problem">候选位置所属的连续优化问题。</param>
    /// <param name="random">当前执行独占的随机数生成器。</param>
    void Repair(Span<double> position, ContinuousProblem problem, Random random);
}
