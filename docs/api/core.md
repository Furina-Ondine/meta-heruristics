# Core API 参考

首次使用请先阅读[用户使用手册](../guides/user-guide.md)。本页按类型汇总 `Metaheuristics.Core` 的连续、标量单目标契约；不承诺多目标、二进制、排列、远程或 GPU 执行。

## API 分组

| 分组 | 公共类型 | 职责 |
| --- | --- | --- |
| 问题 | `ContinuousProblem`、`VariableBounds`、`IObjectiveFunction`、`IConstraint` | 定义维度、标量目标函数和归一化约束违背量；边界仅用于构造默认 Repair。 |
| 数值结果 | `Evaluation`、`ConstraintEvaluation`、`OptimizationDirection` | 固定首版的 `double` 目标值、可行性与约束统计。 |
| 比较 | `EvaluationComparer` | 实现可行性优先、总违背量优先，再按最小化或最大化方向比较目标值。 |
| 策略 | `ICandidateInitializer`、`ICandidateRepair`、`CandidateRepairs` | 将位置初始化和修复与约束判定分离；Repair 自己持有边界或其他恢复数据。 |
| 算法扩展 | `IOptimizer`、`OptimizationRunContext` | 有状态 Optimizer 持有可复用工作区；Context 提供当前 run 的问题、随机流、取消和统一评估计数。 |
| 执行 | `OptimizationRunner`、`OptimizationRunOptions`、`OptimizationRunSummary` | 管理重置、运行循环、停止和可选轨迹。 |
| 停止与轨迹 | `IStoppingCondition`、`StoppingConditions`、`OptimizationTraceOptions` | 支持自定义停止条件以及最大迭代、最大评估、时限、目标值和 OR 组合；轨迹可关闭或按迭代、评估间隔、用户指定的迭代进度比例记录。 |

## 最小用法

```csharp
var problem = new ContinuousProblem(
    [new VariableBounds(-5, 5), new VariableBounds(-5, 5)],
    new SphereObjective(),
    OptimizationDirection.Minimize);

var options = new OptimizationRunOptions(
    StoppingConditions.Any(
        StoppingConditions.MaxIterations(500),
        StoppingConditions.MaxEvaluations(20_000)));

var summary = OptimizationRunner.Execute(
    problem,
    optimizer,
    options,
    seed: 42,
    cancellationToken);
var bestPosition = optimizer.BestPosition.ToArray();
```

`optimizer` 是调用方创建并拥有的 `IOptimizer` 实例。当前正式实现及其配置见 [Algorithms API](algorithms.md)，可运行示例见 [`examples/Metaheuristics.Examples/Program.cs`](../../examples/Metaheuristics.Examples/Program.cs)。

## 生命周期契约

一次运行按以下顺序执行：

```text
创建 run Context → Optimizer.ResetForRun
  → 检查停止 → Advance → 检查停止 → …
  → 返回 Summary；调用方按需复制最佳位置
```

- `IOptimizer` 是有状态实例，可以保存种群、临时数组和当前运行状态；它不保证线程安全，不能被并发调用。
- `ResetForRun` 必须覆盖上一 run 的逻辑状态，并在返回前产生可评估的 `BestPosition` 和 `BestEvaluation`。算法应在初始化每个位置后调用 `context.Repair`；首次调用可以分配依赖维度的主要工作区，后续正常运行应复用它们。
- 每次 `Advance()` 完成一个原子算法迭代。算法只通过 `OptimizationRunContext.Evaluate` 评估候选，以统一处理计数、取消和数值校验。
- 每个 run 都有显式 seed 和新的 `Random(seed)`；不得使用 `Random.Shared` 或按当前时间自行播种。
- Runner 在重置后及每个完整迭代之后检查停止条件。因此最大评估数是迭代边界预算；单次 `Advance()` 可以让最终计数超过阈值。
- `IStoppingCondition.Evaluate` 必须可重入，不保存 run 级可变状态；同一配置可能被多个实验 Group 并发读取。
- 取消遵循 .NET 约定并抛出 `OperationCanceledException`。单次 `Execute` 不返回部分汇总。
- `OptimizationRunner.Execute` 返回不含位置副本的 `OptimizationRunSummary`。调用方或实验调度器必须在下一次重置前，从 `IOptimizer.BestPosition` 复制所需位置。
- Runner 不释放 Optimizer；通用 `IOptimizer` 也不继承 `IDisposable`。一次运行异常后，不应继续复用该实例。
- 启用轨迹时，第一个点是 `ResetForRun` 完成后的迭代 0 基线。轨迹点只保存评估结果，不复制候选位置。
- `IterationProgress` 按 `ProgressIntervalRatio` 记录，例如 `0.1` 表示每 10%、`0.25` 表示每 25%。比例点不是整数迭代时，在首次达到或超过它的完整迭代记录，并始终保留最终状态。

## 数值契约

- `VariableBounds` 用 `null` 表示缺省的无界端点；显式边界必须有限且不能形成反向区间。它们只用于构造或实现 `ICandidateRepair`，不由算法读取。
- `ContinuousProblem` 和 Runner 不验证候选位置的维度、范围或有限性。初始化器和 Repair 共同对此负责；算法必须在每次初始化或修改位置后调用 `context.Repair`。
- 未显式提供 Repair 时，`ContinuousProblem` 从构造参数创建 `CandidateRepairs.Clamp`。内置 Clamp 将有界维度的有限越界值和无穷值截断到端点，保留 `NaN`；无界维度不处理。
- `CandidateRepairs.DoNothing` 完全跳过修复。除非调用方能自行保证每条位置更新路径与其数值后果，否则不要使用它。
- 目标函数只能返回有限 `double`。
- `IConstraint.EvaluateViolation` 返回已经加权、归一化的非负有限违背量；`0` 表示约束满足。
- 两个不可行候选先比较 `TotalViolation`；相同后才比较目标值。候选修复不会改变这套比较语义。
- Core 第一波只提供标量单点评估，不规定种群采用 AoS、SoA 或其他内存布局。

## 当前波次边界

本波固定算法与 Experiment 共用的 Core 表面。标准约束类型和无显著改进停止条件尚未进入本次交付；第一种正式算法实现见 [Algorithms API](algorithms.md)。
