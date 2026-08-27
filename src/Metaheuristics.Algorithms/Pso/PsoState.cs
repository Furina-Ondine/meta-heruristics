using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Algorithms.Pso;

/// <summary>保存一个粒子的算法专属可变状态。</summary>
/// <remarks>对象和数组在 Optimizer 的双缓冲工作区中复用，不会逐代分配。</remarks>
internal sealed class PsoState
{
    public PsoState(int dimension)
    {
        Position = new double[dimension];
        Velocity = new double[dimension];
        PersonalBestPosition = new double[dimension];
    }

    public double[] Position { get; }

    public double[] Velocity { get; }

    public double[] PersonalBestPosition { get; }

    public Evaluation Evaluation { get; set; }

    public Evaluation PersonalBestEvaluation { get; set; }
}
