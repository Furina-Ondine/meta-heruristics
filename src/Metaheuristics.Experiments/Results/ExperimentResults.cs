using System.Collections.ObjectModel;
using Anastasya.Metaheuristics.Core.Execution;

namespace Anastasya.Metaheuristics.Experiments.Results;

/// <summary>
/// 指定 run、Case 或最终 Experiment 的执行状态。
/// </summary>
public enum ExperimentExecutionStatus
{
    /// <summary>执行因取消而从未开始。</summary>
    NotStarted,

    /// <summary>执行正常完成。</summary>
    Succeeded,

    /// <summary>执行发生非取消异常。</summary>
    Failed,

    /// <summary>执行开始后观察到取消。</summary>
    Canceled,
}

/// <summary>
/// 表示一个 Repetition 的不可变执行记录。
/// </summary>
public sealed class ExperimentRunResult
{
    internal ExperimentRunResult(
        string caseId,
        int groupIndex,
        int repetitionIndex,
        int seed,
        ExperimentExecutionStatus status,
        OptimizationRunSummary? summary,
        Exception? exception)
    {
        CaseId = caseId;
        GroupIndex = groupIndex;
        RepetitionIndex = repetitionIndex;
        Seed = seed;
        Status = status;
        Summary = summary;
        Exception = exception;
    }

    /// <summary>获取所属 Case 的稳定标识。</summary>
    public string CaseId { get; }

    /// <summary>获取所属 RunGroup 在 Case 内的下标。</summary>
    public int GroupIndex { get; }

    /// <summary>获取当前 Repetition 下标。</summary>
    public int RepetitionIndex { get; }

    /// <summary>获取当前 run 实际使用的 seed。</summary>
    public int Seed { get; }

    /// <summary>获取当前 run 的最终状态。</summary>
    public ExperimentExecutionStatus Status { get; }

    /// <summary>获取成功 run 的 Core 汇总；其他状态为 <see langword="null"/>。</summary>
    public OptimizationRunSummary? Summary { get; }

    /// <summary>获取失败或取消异常；未开始和成功状态通常为 <see langword="null"/>。</summary>
    public Exception? Exception { get; }
}

/// <summary>
/// 表示一个 Case 的稳定顺序结果、最佳位置矩阵和统计。
/// </summary>
public sealed class ExperimentCaseResult
{
    private readonly ReadOnlyCollection<ExperimentRunResult> _runs;

    internal ExperimentCaseResult(
        string caseId,
        ExperimentExecutionStatus status,
        ExperimentRunResult[] runs,
        BestPositionMatrix? bestPositions,
        ExperimentStatistics statistics)
    {
        CaseId = caseId;
        Status = status;
        _runs = Array.AsReadOnly(runs);
        BestPositions = bestPositions;
        Statistics = statistics;
    }

    /// <summary>获取 Case 的稳定标识。</summary>
    public string CaseId { get; }

    /// <summary>获取 Case 的最终执行状态。</summary>
    public ExperimentExecutionStatus Status { get; }

    /// <summary>获取按 Repetition 下标排列的不可变运行记录。</summary>
    public IReadOnlyList<ExperimentRunResult> Runs => _runs;

    /// <summary>获取最佳位置矩阵；所有 Group 均未成功创建 Problem 时为 <see langword="null"/>。</summary>
    public BestPositionMatrix? BestPositions { get; }

    /// <summary>获取只统计成功 run 的指标及全部状态计数。</summary>
    public ExperimentStatistics Statistics { get; }
}

/// <summary>
/// 表示一次 Experiment 执行完成后的不可变部分或完整结果。
/// </summary>
/// <remarks>结果及其集合均为稳定快照，可在线程之间安全共享。</remarks>
public sealed class ExperimentResult
{
    private readonly ReadOnlyCollection<ExperimentCaseResult> _cases;
    private readonly ReadOnlyCollection<int> _seeds;

    internal ExperimentResult(
        ExperimentExecutionStatus status,
        ExperimentCaseResult[] cases,
        int[] seeds,
        ExperimentRunCounts counts,
        TimeSpan duration)
    {
        Status = status;
        _cases = Array.AsReadOnly(cases);
        _seeds = Array.AsReadOnly(seeds);
        Counts = counts;
        Duration = duration;
    }

    /// <summary>获取 Experiment 的最终状态。</summary>
    public ExperimentExecutionStatus Status { get; }

    /// <summary>获取按声明顺序排列的 Case 结果。</summary>
    public IReadOnlyList<ExperimentCaseResult> Cases => _cases;

    /// <summary>获取执行时快照的共享 seed 序列。</summary>
    public IReadOnlyList<int> Seeds => _seeds;

    /// <summary>获取所有 Case 的状态数量汇总。</summary>
    public ExperimentRunCounts Counts { get; }

    /// <summary>获取从调度开始到所有 Worker Task 结束的持续时间。</summary>
    public TimeSpan Duration { get; }
}
