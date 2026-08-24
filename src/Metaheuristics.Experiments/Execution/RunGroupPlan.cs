using Anastasya.Metaheuristics.Experiments.Configuration;

namespace Anastasya.Metaheuristics.Experiments.Execution;

/// <summary>表示调度器分配给一个 Worker 的稳定 Group 计划。</summary>
/// <remarks>计划在调度前完全确定，不依赖 Worker 的实际领取顺序。</remarks>
internal sealed record RunGroupPlan(
    int CaseIndex,
    ExperimentCase Case,
    int GroupIndex,
    int[] RepetitionIndices,
    int[] Seeds);
