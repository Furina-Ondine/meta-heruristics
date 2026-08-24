using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Algorithms.Bat;

/// <summary>
/// 保存一只蝙蝠的算法专属可变状态。
/// </summary>
/// <remarks>对象在双缓冲种群之间交换；所有数组只在工作区创建时分配，不会逐代分配。</remarks>
internal sealed class BatState
{
    public BatState(int dimension)
    {
        Position = new double[dimension];
        Velocity = new double[dimension];
        Frequency = new double[dimension];
        Loudness = new double[dimension];
        PulseRate = new double[dimension];
        InitialPulseRate = new double[dimension];
    }

    public double[] Position { get; }

    public double[] Velocity { get; }

    public double[] Frequency { get; }

    public double[] Loudness { get; }

    public double[] PulseRate { get; }

    public double[] InitialPulseRate { get; }

    public Evaluation Evaluation { get; set; }
}
