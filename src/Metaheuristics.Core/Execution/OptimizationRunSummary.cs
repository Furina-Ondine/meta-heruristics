using System.Collections.ObjectModel;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Core.Execution;

/// <summary>
/// 提供一次优化运行的不可变标量结果和轨迹，但不复制最佳位置。
/// </summary>
/// <remarks>
/// 该类型用于需要自行保存最佳位置的调用方。<see cref="OptimizationRunner.Execute"/> 返回后，
/// 调用方必须在下一次 <see cref="IOptimizer.ResetForRun"/> 前读取优化器的最佳位置。
/// </remarks>
public sealed class OptimizationRunSummary
{
    private readonly ReadOnlyCollection<OptimizationTracePoint> _trace;

    internal OptimizationRunSummary(
        Evaluation bestEvaluation,
        TerminationReason terminationReason,
        int iterations,
        long evaluations,
        TimeSpan duration,
        int seed,
        IReadOnlyList<OptimizationTracePoint> trace)
    {
        BestEvaluation = bestEvaluation;
        TerminationReason = terminationReason;
        Iterations = iterations;
        Evaluations = evaluations;
        Duration = duration;
        Seed = seed;
        _trace = Array.AsReadOnly(trace.ToArray());
    }

    /// <summary>
    /// 获取运行终止时的最优评估结果。
    /// </summary>
    public Evaluation BestEvaluation { get; }

    /// <summary>
    /// 获取运行终止原因。
    /// </summary>
    public TerminationReason TerminationReason { get; }

    /// <summary>
    /// 获取已完成的迭代次数。
    /// </summary>
    public int Iterations { get; }

    /// <summary>
    /// 获取已完成的目标评估次数。
    /// </summary>
    public long Evaluations { get; }

    /// <summary>
    /// 获取运行持续时间。
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// 获取运行使用的随机种子。
    /// </summary>
    public int Seed { get; }

    /// <summary>
    /// 获取按配置记录的不可变轨迹点集合。
    /// </summary>
    public IReadOnlyList<OptimizationTracePoint> Trace => _trace;
}
