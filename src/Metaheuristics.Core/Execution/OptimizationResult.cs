using System.Collections.ObjectModel;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Core.Execution;

/// <summary>
/// 提供一次优化运行完成后的不可变结果快照。
/// </summary>
/// <remarks>
/// 结果会复制最优位置，并引用不可变的运行汇总，因此不会随优化器的后续运行继续变化。
/// </remarks>
public sealed class OptimizationResult
{
    private readonly ReadOnlyCollection<double> _bestPosition;
    private readonly OptimizationRunSummary _summary;

    internal OptimizationResult(
        ReadOnlySpan<double> bestPosition,
        OptimizationRunSummary summary)
    {
        _bestPosition = Array.AsReadOnly(bestPosition.ToArray());
        _summary = summary ?? throw new ArgumentNullException(nameof(summary));
    }

    /// <summary>
    /// 获取最终发现的最优候选位置。
    /// </summary>
    public IReadOnlyList<double> BestPosition => _bestPosition;

    /// <summary>
    /// 获取最优候选位置的评估结果。
    /// </summary>
    public Evaluation BestEvaluation => _summary.BestEvaluation;

    /// <summary>
    /// 获取运行终止原因。
    /// </summary>
    public TerminationReason TerminationReason => _summary.TerminationReason;

    /// <summary>
    /// 获取已完成的迭代次数。
    /// </summary>
    public int Iterations => _summary.Iterations;

    /// <summary>
    /// 获取已完成的目标评估次数。
    /// </summary>
    public long Evaluations => _summary.Evaluations;

    /// <summary>
    /// 获取运行持续时间。
    /// </summary>
    public TimeSpan Duration => _summary.Duration;

    /// <summary>
    /// 获取运行使用的随机种子。
    /// </summary>
    public int Seed => _summary.Seed;

    /// <summary>
    /// 获取按配置记录的不可变轨迹点集合。
    /// </summary>
    public IReadOnlyList<OptimizationTracePoint> Trace => _summary.Trace;
}
