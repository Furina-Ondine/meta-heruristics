using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Algorithms.Firefly;

/// <summary>保存一只萤火虫的算法专属可变状态。</summary>
/// <remarks>对象和 Position 数组在 Optimizer 的双缓冲工作区中复用。</remarks>
internal sealed class FireflyState
{
    public FireflyState(int dimension)
    {
        Position = new double[dimension];
    }

    public double[] Position { get; }

    public Evaluation Evaluation { get; set; }
}
