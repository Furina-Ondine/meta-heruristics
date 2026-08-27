namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>定义如何就地修复候选位置。</summary>
/// <remarks>Repair 自己拥有边界或所需数据；算法和 Core 不向它暴露 Problem。它不改变约束比较语义。</remarks>
public interface ICandidateRepair
{
    /// <summary>在候选被评价前按策略约束就地恢复位置。</summary>
    /// <remarks>实现不应保存缓冲区；不需要随机性的策略必须保持随机流不变，以免改变算法确定性。</remarks>
    /// <param name="position">本次调用独占、可以原位修改的位置缓冲区。</param>
    /// <param name="random">当前执行独占的随机数生成器，仅供随机恢复策略使用。</param>
    void Repair(Span<double> position, Random random);
}
