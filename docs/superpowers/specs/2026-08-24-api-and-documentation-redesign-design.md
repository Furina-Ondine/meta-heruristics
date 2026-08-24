# API 可读性与文档重整设计

## 状态

待用户审阅。

## 给读者的摘要

本次重整面向仍处于早期演示阶段的 Metaheuristics.NET。它不改变算法的数值语义、随机性、并发调度或性能策略；它把目前聚集在少数源文件中的模型和执行细节按职责拆开，并把“如何使用”和“如何扩展”分别写给用户与开发者。

单次优化仍由 `OptimizationRunner.Execute` 驱动。它使用调用方拥有的 `ContinuousProblem`、`IOptimizer` 和 `OptimizationRunOptions`，在优化器中保留最佳位置。调用方若需要稳定快照，必须在下一次 `IOptimizer.ResetForRun` 前自行复制 `IOptimizer.BestPosition`。此前仅调用 `Execute` 再复制位置的单行 `OptimizationRunner.Run` 将删除。

批量实验仍由 `ExperimentRunner.RunAsync` 执行。一个 RunGroup 独占 Problem、Optimizer 和运行选项，并在组内顺序复用 Optimizer 的工作区；多个 Group 只能在全局并发上限内并发。异常后的 Optimizer 不再复用。

## 目标

- 让公共类型的职责、可变性、所有权和线程安全性可以从名称、源文件和 XML 文档中直接看出。
- 让用户从可运行的最小示例开始，而不是从 API 清单或内部实现开始。
- 让开发者能沿着明确的依赖方向理解并扩展 Core、Algorithms 和 Experiments。
- 在不新增热路径分配、不改变确定性或数值规则的前提下完成整理。

## 非目标

- 不增加新算法、新问题表示、新计算后端或远程执行能力。
- 不改变目标/约束比较、候选修复、停止检查、轨迹采样、seed 派生、取消、失败恢复或结果统计的可观察行为。
- 不为保持二进制或源兼容保留已删除的单行 `OptimizationRunner.Run`；项目处于早期 demo 阶段。
- 不新增 ADR：此次仅改变局部组织与可读性，不改变既定架构决策。

## 公开 API 设计

### 保留的名称和入口

- `Evaluation`、`ConstraintEvaluation` 和 `OptimizationRunner.Execute` 保持原名称。
- `OptimizationRunner.Run` 删除。它不包含独立执行语义，只是调用 `Execute` 后复制 Optimizer 的位置缓冲区。
- `ContinuousProblem`、`IOptimizer`、`OptimizationRunContext`、`OptimizationRunOptions`、`OptimizationRunSummary`、`ExperimentRunner`、`ExperimentCase`、`ExperimentDefinition` 及现有结果类型保留其概念和行为。

### 按职责拆分

Core 源码按以下边界整理；每个公共类型单独占用与其名称对应的文件，避免一个文件混合值对象、接口和服务。

| 区域 | 职责 | 代表类型 |
| --- | --- | --- |
| `Problems` | 描述连续问题的边界、目标、约束、初始化和修复。 | `ContinuousProblem`、`VariableBounds`、`IObjectiveFunction`、`IConstraint`、`ICandidateInitializer`、`ICandidateRepair` |
| `Evaluation` | 表示目标与约束汇总，并实施可行性优先的比较规则。 | `Evaluation`、`ConstraintEvaluation`、`EvaluationComparer` |
| `Execution` | 描述优化器契约、一次执行上下文、停止、轨迹和不可变汇总。 | `IOptimizer`、`OptimizationRunContext`、`IStoppingCondition`、`OptimizationRunOptions`、`OptimizationRunSummary`、`OptimizationRunner` |

现有命名空间可以保持不变，以避免为了目录外观引入没有语义价值的迁移；目录和文件名表达的是实现职责，公开命名空间仍遵循项目既有的 `Problems`、`Comparison` 和 `Execution` 分层。

`ExperimentRunner` 保留一个公开入口，内部拆为计划生成、固定 Worker 执行、Group 环境创建、seed 派生和 Case 结果累积等单职责协作对象。它们均为 internal/private 实现细节，不成为新的用户配置入口。

## 生命周期、状态与并发

### 单次执行

```text
ContinuousProblem + IOptimizer + OptimizationRunOptions
                  │
                  ▼
OptimizationRunner.Execute
  → 创建 OptimizationRunContext（Random(seed)、计数、取消令牌）
  → IOptimizer.ResetForRun
  → 停止检查与 IOptimizer.Advance 循环
  → OptimizationRunSummary
```

- `ContinuousProblem` 在构造后只保存问题定义。它不会修改候选位置；其可并发使用前提是用户提供的目标函数、约束和修复策略也满足各自声明的并发约束。
- `OptimizationRunContext` 是一次执行专属对象。它拥有随机数流和评估计数，不能跨执行或跨线程共享。
- `IOptimizer` 拥有可变种群和临时工作区，不保证线程安全。它只属于一个 RunGroup；同一 Group 内正常完成的多次执行可以顺序复用它的物理存储。任何执行异常后，调用方必须丢弃该实例。
- `OptimizationRunSummary` 是不包含位置数组的不可变汇总。最佳位置缓冲区始终归 Optimizer 所有；调用方必须在下一次 reset 前完成读取或复制。
- `IStoppingCondition` 必须可重入且不得保存执行级可变状态，因此可被多个执行并发调用。

### 批量实验

```text
ExperimentDefinition
  → 规划稳定 RunGroup
  → 固定数量的 Worker 领取计划
  → Group factory 创建独占 Problem / Optimizer / Options
  → Group 内顺序 Execute；跨 Group 受 GlobalMaxConcurrency 限制
  → 不可变 ExperimentResult
```

每个 repetition 的 seed 只由实验计划和 repetition 下标决定，不依赖 Group 拆分、Worker 获取顺序或并发度。失败的 repetition 记录异常；为同一 Group 的后续 repetition 重建全新的 Group 环境。取消停止投放新 Group，并返回已完成部分的稳定结果。

## 文档设计

### 用户手册

新增 `docs/guides/user-guide.md`，以人类阅读路径组织：

1. 本库解决什么问题，以及当前版本包含和不包含什么。
2. 最小可运行的单次执行：定义目标函数与边界、配置 Bat、调用 `Execute`、复制最佳位置。
3. 停止条件、轨迹、约束、初始化和修复的常用组合。
4. 实验：Case、RunGroup、并发、seed、取消和部分结果。
5. 线程安全、所有权、可复现性及常见错误。

示例直接使用仓库中可构建的 API，避免伪代码与实际代码漂移。

### 开发者架构手册

新增 `docs/architecture/developer-guide.md`，以“先理解，再扩展”的顺序说明：

1. 项目边界、依赖图和核心术语。
2. Core、Algorithms、Experiments、Examples、Tests、Benchmarks 各自的职责。
3. 组件组合、单次执行和 RunGroup 生命周期。
4. 状态所有权、线程安全、取消、异常恢复和确定性边界。
5. 新算法的实现契约、测试要求和性能验证条件。

`README.md` 调整为简短入口页，按“快速开始、用户手册、架构说明、API 参考、ADR”顺序链接。既有 `docs/api/*.md` 保留为参考页，并连接到更适合第一次阅读的两份手册。

### XML 文档与内部注释

所有 public 类型和成员使用简洁中文 XML 文档。类型和成员的首句说明职责或行为；只有以下公共契约需要在 remarks/异常说明中展开：

- 可变状态归属、快照是否复制、读取有效期和释放责任；
- 是否线程安全、允许的并发共享方式；
- seed、取消、失败和异常后的复用限制；
- 位置、目标值、约束违背和数值有效性的前置/后置条件。

内部注释仅保留不从代码本身显而易见的原因：数值容差、溢出保护、确定性 seed 混合、锁粒度和工作区复用。删除逐行翻译代码的注释。

## 实施与验证

1. 先提交用户已有的 RunGroup 调度基准和关联文档修改。
2. 拆分 Core、Algorithms 和 Experiments 的实现文件，并删除 `OptimizationRunner.Run`；同步迁移调用点、示例、测试和 API 参考。
3. 补齐两份手册、README 导航和 XML 文档；将可视化头脑风暴临时目录加入 `.gitignore`。
4. 执行 `dotnet restore`、Release `dotnet build`、Release `dotnet test`；变更前后保留现有测试对固定 seed、并发隔离、调度与数值安全的覆盖。
5. 不执行新的性能优化；保留基准项目作为对工作区复用与调度策略的既有证据。

## 风险与缓解

删除 `OptimizationRunner.Run` 是刻意的早期 API 破坏。用户手册、示例和编译器错误会明确引导调用者改为 `Execute` 后在有效窗口内复制 `IOptimizer.BestPosition`。内部文件拆分不改变访问修饰符、循环或数组布局；测试将防止重构意外改变确定性、取消或失败恢复行为。
