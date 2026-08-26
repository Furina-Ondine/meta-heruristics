namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>定义单个候选位置的同步目标函数。</summary>
/// <remarks>实现必须自行声明并满足其线程安全性；Core 不序列化对它的调用。</remarks>
public interface IObjectiveFunction
{
    /// <summary>计算给定候选位置的目标值。</summary>
    /// <param name="position">由调用方的初始化器和 Repair 准备的候选位置。</param>
    /// <returns>候选位置对应的有限目标值。</returns>
    double Evaluate(ReadOnlySpan<double> position);
}
