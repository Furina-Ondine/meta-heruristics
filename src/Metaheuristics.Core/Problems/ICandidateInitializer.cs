namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>定义如何生成一次执行的初始候选位置。</summary>
/// <remarks>实现只负责写入 Position，不感知问题边界；算法会在返回后立即调用 Repair。</remarks>
public interface ICandidateInitializer
{
    /// <summary>使用当前 run 的随机流写满一个候选位置的初始值。</summary>
    /// <remarks>返回后算法会立即调用 Repair；实现不应保存缓冲区或从其他随机源取值。</remarks>
    /// <param name="position">本次调用独占且必须完整写入的位置缓冲区。</param>
    /// <param name="random">当前执行独占、由显式 seed 创建的随机数生成器。</param>
    void Initialize(Span<double> position, Random random);
}
