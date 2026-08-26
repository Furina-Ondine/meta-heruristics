using System.Collections.ObjectModel;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Experiments.Configuration;

/// <summary>
/// 表示可由 Experiment 规划为一个或多个 RunGroup 的实验案例。
/// </summary>
public abstract class ExperimentCase
{
    /// <summary>
    /// 初始化实验案例的稳定标识、重复次数和 Group 数量。
    /// </summary>
    /// <param name="id">在所属 Experiment 内唯一的非空标识。</param>
    /// <param name="repetitions">独立运行次数，必须为正数。</param>
    /// <param name="runGroupCount">均衡拆分出的 RunGroup 数量，必须位于<c>1</c>到<paramref name="repetitions"/>之间。</param>
    /// <exception cref="ArgumentException"><paramref name="id"/> 为空或只包含空白。</exception>
    /// <exception cref="ArgumentOutOfRangeException">重复次数或 Group 数量无效。</exception>
    protected ExperimentCase(string id, int repetitions, int runGroupCount)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("A case identifier cannot be empty or whitespace.", nameof(id));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(repetitions);
        if (runGroupCount <= 0 || runGroupCount > repetitions)
        {
            throw new ArgumentOutOfRangeException(
                nameof(runGroupCount),
                "The run group count must be between one and the repetition count.");
        }

        Id = id;
        Repetitions = repetitions;
        RunGroupCount = runGroupCount;
    }

    /// <summary>
    /// 获取在所属 Experiment 内唯一的案例标识。
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// 获取案例包含的独立运行次数。
    /// </summary>
    public int Repetitions { get; }

    /// <summary>
    /// 获取案例均衡拆分出的 RunGroup 数量。
    /// </summary>
    public int RunGroupCount { get; }

    internal abstract ExperimentGroupSetup CreateGroup(ExperimentGroupContext context);
}

/// <summary>
/// 表示携带强类型配置和 Group 工厂的实验案例。
/// </summary>
/// <typeparam name="TConfiguration">用户定义的不可变案例配置类型。</typeparam>
public sealed class ExperimentCase<TConfiguration> : ExperimentCase
{
    private readonly ExperimentGroupFactory<TConfiguration> _createGroup;

    /// <summary>
    /// 创建强类型实验案例。
    /// </summary>
    /// <param name="id">在所属 Experiment 内唯一的案例标识。</param>
    /// <param name="configuration">传给每次 Group 工厂调用的强类型配置。</param>
    /// <param name="repetitions">独立运行次数。</param>
    /// <param name="createGroup">为每个 RunGroup 创建独立 Problem、Optimizer 和运行选项的工厂。</param>
    /// <param name="runGroupCount">RunGroup 数量；默认值一表示案例内顺序运行。</param>
    /// <exception cref="ArgumentNullException">配置或工厂为 <see langword="null"/>。</exception>
    public ExperimentCase(
        string id,
        TConfiguration configuration,
        int repetitions,
        ExperimentGroupFactory<TConfiguration> createGroup,
        int runGroupCount = 1)
        : base(id, repetitions, runGroupCount)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        Configuration = configuration;
        _createGroup = createGroup ?? throw new ArgumentNullException(nameof(createGroup));
    }

    /// <summary>
    /// 获取传给 Group 工厂的强类型配置。
    /// </summary>
    public TConfiguration Configuration { get; }

    internal override ExperimentGroupSetup CreateGroup(ExperimentGroupContext context)
    {
        return _createGroup(Configuration, context)
            ?? throw new InvalidOperationException("The experiment group factory returned null.");
    }
}

/// <summary>
/// 为一个 RunGroup 创建独立 Problem、Optimizer 和运行选项。
/// </summary>
/// <typeparam name="TConfiguration">强类型案例配置。</typeparam>
/// <param name="configuration">案例配置。</param>
/// <param name="context">当前 Group 的稳定编号、Repetition 和 seed。</param>
/// <returns>当前 Group 独占的执行组件。</returns>
public delegate ExperimentGroupSetup ExperimentGroupFactory<TConfiguration>(
    TConfiguration configuration,
    ExperimentGroupContext context);

/// <summary>
/// 提供 Group 工厂创建隔离组件时所需的规划信息。
/// </summary>
/// <remarks>这是不可变的计划快照；取消令牌除外，它反映整个 Experiment 的实时取消状态。</remarks>
public sealed class ExperimentGroupContext
{
    private readonly ReadOnlyCollection<int> _repetitionIndices;
    private readonly ReadOnlyCollection<int> _seeds;

    internal ExperimentGroupContext(
        string caseId,
        int groupIndex,
        int[] repetitionIndices,
        int[] seeds,
        CancellationToken cancellationToken)
    {
        CaseId = caseId;
        GroupIndex = groupIndex;
        _repetitionIndices = Array.AsReadOnly(repetitionIndices);
        _seeds = Array.AsReadOnly(seeds);
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// 获取所属 Case 的稳定标识。
    /// </summary>
    public string CaseId { get; }

    /// <summary>
    /// 获取当前 Group 在 Case 内从零开始的编号。
    /// </summary>
    public int GroupIndex { get; }

    /// <summary>
    /// 获取当前 Group 顺序执行的 Repetition 下标。
    /// </summary>
    public IReadOnlyList<int> RepetitionIndices => _repetitionIndices;

    /// <summary>
    /// 获取与 <see cref="RepetitionIndices"/> 一一对应的随机种子。
    /// </summary>
    public IReadOnlyList<int> Seeds => _seeds;

    /// <summary>
    /// 获取整个 Experiment 的取消令牌。
    /// </summary>
    public CancellationToken CancellationToken { get; }
}

/// <summary>
/// 保存一个 RunGroup 独占的 Problem、Optimizer 和可复用运行选项。
/// </summary>
/// <remarks>组件只能在所属 Group 内顺序使用。Optimizer 在任一执行异常后必须丢弃，不能用于后续 repetition。</remarks>
public sealed class ExperimentGroupSetup
{
    /// <summary>
    /// 创建 RunGroup 执行组件集合。
    /// </summary>
    /// <param name="problem">当前 Group 独占的问题实例。</param>
    /// <param name="optimizer">当前 Group 独占的有状态优化器实例。</param>
    /// <param name="runOptions">Group 内各 run 复用的停止和轨迹配置。</param>
    /// <exception cref="ArgumentNullException">任一组件为 <see langword="null"/>。</exception>
    public ExperimentGroupSetup(ContinuousProblem problem, IOptimizer optimizer, OptimizationRunOptions runOptions)
    {
        Problem = problem ?? throw new ArgumentNullException(nameof(problem));
        Optimizer = optimizer ?? throw new ArgumentNullException(nameof(optimizer));
        RunOptions = runOptions ?? throw new ArgumentNullException(nameof(runOptions));
    }

    /// <summary>
    /// 获取当前 Group 独占的问题实例。
    /// </summary>
    public ContinuousProblem Problem { get; }

    /// <summary>
    /// 获取当前 Group 独占并在顺序 run 之间复用的优化器。
    /// </summary>
    public IOptimizer Optimizer { get; }

    /// <summary>
    /// 获取 Group 内各 run 复用的停止和轨迹配置。
    /// </summary>
    public OptimizationRunOptions RunOptions { get; }
}
