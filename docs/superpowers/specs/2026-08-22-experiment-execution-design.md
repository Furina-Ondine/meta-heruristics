# Experiment 执行架构与接口设计

## 状态与文档边界

状态：第一版已实现。

本文固定 `Metaheuristics.Experiments` 第一波实现的详细设计，包括 Case、RunGroup、调度、内存复用、种子、失败、取消、结果和聚合语义。持续有效的工程契约见 [`ENGINEERING.md`](../../../ENGINEERING.md)，最新执行与评估决策见 [ADR-0009](../../decisions/0009-group-scoped-optimizer-execution.md) 和 [ADR-0010](../../decisions/0010-scalar-evaluation-baseline.md)。

本文保存设计阶段确认的职责和数据流；最终公共类型、签名与用法以 [API Overview](../../api/overview.md) 和生成式 API Reference 为准。

## 目标

- 一个 Experiment 包含多个 Case，不同 Case 可以使用不同 Problem、Optimizer 和配置。
- 每个 Case 包含 N 次相互独立、可复现的优化运行。
- 用户通过 `RunGroupCount` 控制一个 Case 的最大并行度和同时存在的 Optimizer 数量。
- 一个 RunGroup 只进行一次主要内存分配，并在单线程内顺序执行 M 次逻辑 run。
- 调度器只认识已规划的 RunGroup，不认识 Case，也不维护 Case 间并发层。
- 每个 run 的 seed、结果编号和随机序列不受 Group 拆分、调度顺序或全局并发度影响。
- 单个 run 失败后继续实验；取消返回已完成的部分结果。
- 第一波提供原始结果和基本统计，不提供文件导出。

## 非目标

- 远程执行、集群调度、GPU 后端和跨进程强制终止。
- 显式 Case 优先级、权重或进度通知。
- CSV、JSON 或数据库持久化。
- 为所有算法设计统一数组池或通用 Workspace 布局。
- Core 级批量目标评估和统一种群数据布局。
- 自定义可重置 PRNG；每个 run 继续创建独立的 `Random(seed)`。
- 收敛曲线对齐、显著性检验、ANOVA 或可视化报告。

## 术语与总体流程

| 术语 | 含义 |
| --- | --- |
| Experiment | 一组需要统一调度并汇总的 Case。 |
| Case | 一个强类型配置及其 Group 工厂、重复次数和拆分规则。 |
| Repetition | Case 中编号稳定的一次独立优化运行。 |
| RunGroup | 单个调度工作单元；拥有独立 Problem 和有状态 Optimizer，并顺序执行若干 Repetition。 |
| Optimizer | Factory 创建的 Group 独占算法实例；持有种群数组、临时缓冲区和当前 run 状态。 |
| Run context | 单次 Repetition 的 seed、随机数、取消、评估计数和逻辑状态。 |

总体流程为：

```text
ExperimentCase[]
    → Planner 均衡拆分并交错 RunGroup
    → 有界全局 Scheduler
    → Group Factory 创建 Problem + 可复用 Optimizer
    → Optimizer 顺序 Reset/Run M 次
    → 原始结果矩阵与统计聚合
```

## Case 与强类型工厂

Experiment 不通过字符串、反射、服务定位器或约定构造函数解释用户配置。用户提供强类型配置和每个 Group 调用一次的工厂。

概念 API 如下：

```csharp
public interface IExperimentCase
{
    string Id { get; }

    int Repetitions { get; }

    int RunGroupCount { get; }
}

public sealed class ExperimentCase<TConfiguration> : IExperimentCase
{
    public required string Id { get; init; }

    public required TConfiguration Configuration { get; init; }

    public required int Repetitions { get; init; }

    public int RunGroupCount { get; init; } = 1;

    public required ExperimentGroupFactory<TConfiguration> CreateGroup { get; init; }
}

public delegate ExperimentGroupSetup ExperimentGroupFactory<TConfiguration>(
    TConfiguration configuration,
    ExperimentGroupContext context);

public sealed record ExperimentGroupSetup(
    ContinuousProblem Problem,
    IOptimizer Optimizer,
    OptimizationRunOptions RunOptions);
```

`ExperimentGroupContext` 至少提供：

- `CaseId`；
- `GroupIndex`；
- 当前 Group 的 Repetition 下标；
- 与这些下标对应的 seed；
- 实验取消令牌。

每次工厂调用必须返回当前 Group 独占的 Problem 和 Optimizer 实例。多个 Group 不共享这些实例；它们可以引用用户提前加载的同一份不可变底层数据。

## Case 规划与 RunGroup 拆分

每个 Case 使用：

```csharp
RunGroupCount = P;
```

默认 `P = 1`。验证规则为：

- `Repetitions >= 1`；
- `1 <= RunGroupCount <= Repetitions`；
- Experiment 至少包含一个 Case；
- Case ID 在同一 Experiment 内唯一。

Planner 将 N 次 Repetition 确定性地均衡分配给 P 个 Group。若 `N = 10`、`P = 3`，三个 Group 分别包含 4、3、3 次运行。每个 Repetition 的编号和 seed 在拆分前确定，因此改变 P 不会改变单次实验身份。

不同 Case 产生的 Group 按轮转顺序交错。例如：

```text
Case A / Group 0
Case B / Group 0
Case C / Group 0
Case A / Group 1
Case B / Group 1
...
```

Planner 可以惰性产生 Group 计划。Scheduler 只消费计划，不读取 Case 配置，也不施加 Case 间并发限制。

## 全局调度

第一波只提供一个全局并发参数：

```csharp
public sealed record ExperimentExecutionOptions
{
    public int GlobalMaxConcurrency { get; init; } = Environment.ProcessorCount;
}
```

调度规则为：

- `GlobalMaxConcurrency >= 1`；
- 同时执行的 RunGroup 不超过该值；
- 不为所有 Group 或 Repetition 预先创建 Task；
- 只有获得执行槽后才调用 Group Factory，并分配 Problem、Optimizer 和种群；
- 一个 Group 固定由一个执行线程顺序驱动，不并发访问其 Problem 或 Optimizer；
- Group 的完成顺序不影响结果顺序，结果始终按 Case 和 Repetition 下标读取。

`RunGroupCount` 是单个 Case 的理论最大并行度，也是该 Case 同时存在的 Optimizer 数量上限，但不保证这些 Group 同时执行。

## Core Optimizer 生命周期与内存复用

现有“一次 run 创建和释放一个 Session”的模型将被撤销。强类型 Config 保存可复用算法参数，Factory 为每个 RunGroup 创建一个有状态 Optimizer。概念契约为：

```csharp
public interface IOptimizer
{
    ReadOnlySpan<double> BestPosition { get; }

    Evaluation BestEvaluation { get; }

    void ResetForRun(OptimizationRunContext context);

    void Advance();
}
```

Optimizer 直接拥有物理工作区，包括：

- 分配种群位置、速度、适应度和索引数组；
- 分配算法专用临时缓冲区；
- 建立仅由当前 Group 使用的可变工作状态。

依赖 Problem 维度的物理分配可以在 Factory 构造阶段或第一次 `ResetForRun` 中完成，但正常的后续 Reset 不得重新分配主要工作区。每个 Repetition 开始前，Runner 创建新的 run 级 `OptimizationRunContext` 和 `Random(seed)`，随后调用 `ResetForRun`。Reset 必须：

- 重置迭代、评估和最佳状态；
- 使用当前 seed 的随机流重新填写初始种群；
- 覆盖所有可能在本 run 中被读取的旧数据；
- 清除轨迹及其他 run 级逻辑状态；
- 不要求清零必然会在读取前完全覆盖的缓冲区。

Optimizer 不保证线程安全，不得跨 Group 共享。具体算法自行管理并复用其数组；Core 第一波不提供统一 Workspace 或数组租赁抽象。

`IOptimizer` 不继承 `IDisposable`。通用实现只持有托管对象和数组；Group 完成或异常后停止引用该实例，由 GC 回收。拥有非托管资源的具体实现可以自行实现 `IDisposable`，但这不是通用接口契约。

单次优化和 Experiment 使用同一个 Runner 核心。Runner 不释放调用方传入的 Optimizer；单次调用使用一个实例，Experiment RunGroup 对同一实例顺序执行多次。运行异常后该实例不再复用，Group 通过 Factory 创建新 Optimizer 继续剩余 run。

## Problem 生命周期

每个 RunGroup 通过 Group Factory 创建一个独立 Problem，并在 Group 内顺序复用：

```text
Case
├─ Group 0 → Problem 0 + Optimizer 0
├─ Group 1 → Problem 1 + Optimizer 1
└─ Group 2 → Problem 2 + Optimizer 2
```

同一 Case 的各 Group 必须具有相同的维度、优化方向和问题语义。不同 Problem 可以共享不可变输入数据，但不能默认共享可变缓存或其他运行状态。

## 停止条件与运行选项

`IStoppingCondition` 保持现有接口，不增加状态 Session。其公共契约补充为：

- `Evaluate` 可重入；
- 实例不保存 run 级可变状态；
- 同一实例允许被多个 Group 并发调用；
- 组合条件必须保持相同性质。

`OptimizationRunOptions` 可在 Group 内复用。Seed 从 Options 移出，由每次 Runner 调用显式传入，以避免 Group 配置与 Repetition 计划产生两个 seed 来源。

未来若实现需要内部历史的停止规则，例如无显著改进窗口，应先重新评估停止状态模型，不能将可变计数直接加入共享条件实例。

## 单点评估边界

第一波撤销 `IBatchObjectiveFunction`、`ContinuousProblem.EvaluateBatch` 和 Context 上的 `EvaluateBatch`，只保留：

```csharp
Evaluation OptimizationRunContext.Evaluate(ReadOnlySpan<double> position);
```

Optimizer 自行决定种群使用 AoS、SoA、矩形数组、交错数组或其他算法专用布局。Core 不要求算法把候选位置整理成统一的连续批量缓冲区。

未来若实际算法和端到端基准证明批量评估有稳定收益，再设计由算法显式选择的可选能力；基准必须包含布局转换、候选验证、约束处理和临时分配，而不只测目标函数调用。

## Seed 与可复现性

Experiment 使用一个共享的实验级 seed 序列。不同 Case 的相同 Repetition 默认使用相同 seed：

```text
Case A / repetition 0 → seed[0]
Case B / repetition 0 → seed[0]
Case A / repetition 1 → seed[1]
Case B / repetition 1 → seed[1]
```

用户可以显式传入 seed 列表；列表必须覆盖 Experiment 中最大的 Repetition 数。未显式传入时，由 `BaseSeed` 和稳定派生函数生成所需数量的互异 seed，不使用当前时间。

以下变化不得改变任一 Repetition 的 seed：

- `RunGroupCount`；
- `GlobalMaxConcurrency`；
- Case 或 Group 的调度和完成顺序。

每个 run 重新创建 `Random(seed)`。第一波不为避免这一小型对象分配而维护自定义 PRNG。

## 失败、取消与状态

run 和最终 Experiment 结果使用以下状态：

```csharp
public enum ExperimentExecutionStatus
{
    NotStarted,
    Succeeded,
    Failed,
    Canceled,
}
```

单个 run 的语义为：

- `Succeeded`：优化正常完成并产生有效结果；
- `Failed`：Group 初始化、Optimizer 创建、Reset 或运行抛出非取消异常；
- `Canceled`：已经开始的 run 观察到实验取消；
- `NotStarted`：取消发生时尚未开始执行。

Experiment 最终状态为：

- `NotStarted`：在第一个 Group 开始初始化前观察到取消；
- `Succeeded`：所有计划 run 成功；
- `Failed`：未取消，但至少一个 run 或 Group 初始化失败；
- `Canceled`：至少一个 Group 已经开始后观察到取消。

状态优先级为：开始后的取消高于失败，失败高于成功。第一波不暴露实时执行状态，因此不定义 `Running`；未来加入执行句柄或进度报告时再扩充实时状态。

失败处理遵循：

- 单个 run 失败时记录异常并继续；
- run 中途异常后丢弃当前 Problem 和 Optimizer 的引用；
- 为 Group 剩余 run 重建 Group 环境和 Optimizer；
- Group 首次初始化或重建失败时，该 Group 尚未执行的 run 全部标记为 `Failed`；
- 用户取消后不重建 Optimizer，停止投放新 Group，并等待已启动 Group 协作退出；
- 第一波结果保存原始 `Exception`，序列化错误模型在导出功能进入范围时另行设计。

真正不响应取消或停止检查的算法无法在进程内安全强制终止。第一波只提供协作式取消和 Core 停止条件。

## 结果存储

每个成功 run 保留完整最佳位置。Case 为结果一次性分配矩形数组：

```csharp
double[repetitionCount, dimension]
```

第一维是 Repetition 下标，第二维是候选位置维度。不同 run 写入不同的行，不需要加锁。失败、取消和未开始的 run 没有有效位置。

底层数组不直接公开，避免调用方修改结果。概念只读 API 为：

```csharp
public sealed class BestPositionMatrix
{
    public int RunCount { get; }

    public int Dimension { get; }

    public double this[int repetitionIndex, int dimensionIndex] { get; }

    public void CopyPositionTo(int repetitionIndex, Span<double> destination);
}
```

每个 run 的记录至少包含：

- Case ID、Group 下标和 Repetition 下标；
- 实际 seed；
- 状态和可选异常；
- 最佳评估结果；
- 终止原因；
- 迭代数、评估数和耗时；
- 可选轨迹。

结果按 Case 声明顺序和 Repetition 下标提供稳定读取顺序，不使用任务完成顺序。

## 聚合统计

第一波对以下指标分别提供 `Mean`、`Median`、`Min`、`Max` 和样本标准差：

- 最佳目标值；
- 迭代数；
- 评估数；
- 耗时。

统计规则为：

- 仅 `Succeeded` run 参与数值统计；
- 样本标准差使用 `n - 1` 作为分母；
- `n = 1` 时标准差为 `0`；
- `n = 0` 时所有统计值为 nullable，不使用 `NaN`；
- 单独报告成功、失败、取消和未开始数量；
- `Min` 和 `Max` 是数值统计，不根据优化方向改名为最好或最差。

第一波不提供 CSV、JSON、ANOVA、显著性检验或收敛曲线聚合。

## 启动前验证

以下问题在调度开始前抛出参数或配置异常，不生成失败结果：

- Experiment 没有 Case；
- Case ID 为空或重复；
- Repetition 数小于 1；
- `RunGroupCount` 不在 `[1, Repetitions]`；
- `GlobalMaxConcurrency` 小于 1；
- 显式 seed 数量不足；
- 必需的配置、工厂或运行选项为空。

只有在 Group Factory、Optimizer 创建、Reset 或实际运行期间出现的异常才进入实验失败结果。

## 测试与性能验证

实现必须至少覆盖：

- N 不能整除 P 时的均衡、无遗漏、无重复拆分；
- 不同 Case Group 的轮转交错顺序；
- 全局并发度上限且不提前创建全部任务；
- Optimizer 在正常 run 间复用，物理数组不重复分配；
- 每次 Reset 完全隔离种群、最佳状态、计数和随机流；
- run 异常后 Optimizer 不再复用并重新创建；
- Group 初始化失败时剩余 run 的状态；
- 取消后的 `Canceled` 与 `NotStarted` 区分及部分结果；
- seed 不受并发度和 Group 拆分影响；
- 二维最佳位置存储不会暴露可变数组；
- 统计的正常、单样本、无成功样本和失败排除行为；
- Problem 和 Optimizer 在正常、异常和取消路径均停止使用并释放强引用。

内存复用属于热路径优化。实现应通过分配分析或 BenchmarkDotNet 比较“一次 run 一次工作区分配”和“Group 内 Optimizer 复用”，记录种群规模、维度、重复次数、分配字节和运行时间；在获得数据前不作具体性能倍数承诺。

## 配套文档

第一版实现已同步：

- API Overview 与生成式 Reference：撤销 Session 和批量评估，并记录有状态 Optimizer 的重用契约；
- `docs/api/overview.md` 与生成式 API Reference：记录最终公共入口、类型和成员契约；
- `docs/architecture/overview.md`：把 Experiment 从“已决策、尚未实现”更新为实际状态；
- 示例和测试文档：提供多个 Case、共享 seed、不同 Group 数量和部分失败的用法。
