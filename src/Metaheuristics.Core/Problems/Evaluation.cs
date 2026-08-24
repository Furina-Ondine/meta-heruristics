namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>
/// 表示一个候选位置对应的目标值和约束评估结果。
/// </summary>
/// <remarks>这是不可变值类型，可在线程之间安全共享。</remarks>
public readonly record struct Evaluation
{
    /// <summary>创建一次候选解评估结果。</summary>
    /// <param name="objective">目标函数返回的有限目标值。</param>
    /// <param name="constraints">候选位置的约束评估结果。</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="objective"/> 不是有限值。</exception>
    public Evaluation(double objective, ConstraintEvaluation constraints)
    {
        if (!double.IsFinite(objective))
        {
            throw new ArgumentOutOfRangeException(nameof(objective), "The objective value must be finite.");
        }

        Objective = objective;
        Constraints = constraints;
    }

    /// <summary>获取目标函数值。</summary>
    public double Objective { get; }

    /// <summary>获取约束违背汇总。</summary>
    public ConstraintEvaluation Constraints { get; }
}
