namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>定义如何就地修复候选位置。</summary>
/// <remarks>Repair 自己拥有边界或所需数据；算法和 Core 不向它暴露 Problem。它不改变约束比较语义。</remarks>
public interface ICandidateRepair
{
    /// <summary>就地修复候选位置。</summary>
    /// <param name="position">要就地修改的候选位置缓冲区。</param>
    /// <param name="random">当前执行独占的随机数生成器。</param>
    void Repair(Span<double> position, Random random);
}
