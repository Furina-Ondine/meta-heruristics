using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Algorithms.Cuckoo;

/// <summary>保存一个巢的算法专属可变状态。</summary>
/// <remarks>对象和 Position 数组在 Optimizer 的种群与候选工作区中复用。</remarks>
internal sealed class CuckooState
{
    public CuckooState(int dimension)
    {
        Position = new double[dimension];
    }

    public double[] Position { get; }

    public Evaluation Evaluation { get; set; }
}
