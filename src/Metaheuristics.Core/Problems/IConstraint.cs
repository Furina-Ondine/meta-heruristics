namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>定义一个返回归一化约束违背量的约束。</summary>
/// <remarks>实现必须自行声明并满足其线程安全性；Core 不序列化对它的调用。</remarks>
public interface IConstraint
{
    /// <summary>计算候选位置对该约束的归一化违背量。</summary>
    /// <param name="position">待检查的候选位置。</param>
    /// <returns>非负且有限的违背量；零表示满足约束。</returns>
    /// <exception cref="ArgumentException">候选位置维度不正确。</exception>
    /// <exception cref="ArgumentOutOfRangeException">候选位置包含非有限值或越过边界。</exception>
    double EvaluateViolation(ReadOnlySpan<double> position);
}
