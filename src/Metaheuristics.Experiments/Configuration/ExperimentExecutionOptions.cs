namespace Anastasya.Metaheuristics.Experiments.Configuration;

/// <summary>
/// 配置 Experiment 的全局并发度和共享 seed 序列来源。
/// </summary>
public sealed class ExperimentExecutionOptions
{
    /// <summary>
    /// 获取或设置同时执行的 RunGroup 上限；默认使用逻辑处理器数量。
    /// </summary>
    public int GlobalMaxConcurrency { get; init; } = Environment.ProcessorCount;

    /// <summary>
    /// 获取或设置未显式提供 seed 列表时使用的基础种子。
    /// </summary>
    public int BaseSeed { get; init; }

    /// <summary>
    /// 获取或设置可选的显式共享 seed 列表。
    /// </summary>
    /// <remarks>
    /// Runner 在启动时复制列表。列表必须覆盖所有 Case 中最大的 Repetition 数。
    /// 不同 Case 的相同 Repetition 默认读取同一个 seed。
    /// </remarks>
    public IReadOnlyList<int>? Seeds { get; init; }
}
