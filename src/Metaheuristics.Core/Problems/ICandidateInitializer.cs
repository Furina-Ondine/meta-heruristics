namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>定义如何生成一次执行的初始候选位置。</summary>
/// <remarks>实现使用当前执行专属的随机流；它不应保存执行级可变状态。</remarks>
public interface ICandidateInitializer
{
    /// <summary>将一个候选位置初始化为问题允许的起始状态。</summary>
    /// <param name="position">要写入的候选位置缓冲区。</param>
    /// <param name="problem">候选位置所属的连续优化问题。</param>
    /// <param name="random">当前执行独占的随机数生成器。</param>
    void Initialize(Span<double> position, ContinuousProblem problem, Random random);
}
