namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>定义如何生成一次执行的初始候选位置。</summary>
/// <remarks>实现只负责写入 Position，不感知问题边界；算法会在返回后立即调用 Repair。</remarks>
public interface ICandidateInitializer
{
    /// <summary>将一个候选位置初始化为起始状态。</summary>
    /// <param name="position">要写入的候选位置缓冲区。</param>
    /// <param name="random">当前执行独占的随机数生成器。</param>
    void Initialize(Span<double> position, Random random);
}
