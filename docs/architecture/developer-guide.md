# 开发者架构手册

本手册说明当前实现如何组合。工程约束以 [工程规范](../../ENGINEERING.md) 为准，决策理由以 [ADR](../decisions/README.md) 为准；本文解释开发时应从哪里进入与如何安全扩展。

## 项目职责

```text
Algorithms ──→ Core
Experiments ─→ Core
Examples ───→ Core + Algorithms + Experiments
Tests ──────→ Core + Algorithms + Experiments
Benchmarks ─→ Core + Algorithms
```

- `Core` 定义连续问题、评估/比较、优化器执行契约、停止、轨迹与汇总。
- `Algorithms` 实现 `IOptimizer`；当前为使用双缓冲种群工作区的 `BatOptimizer`。
- `Experiments` 用强类型 Case 和 Group factory 规划、调度和汇总多次执行。
- `Examples` 展示用户组合；`Tests` 锁定可观察行为；`Benchmarks` 为工作区复用与调度策略提供性能证据。

## 组件组合与生命周期

单次执行由用户组装：

```text
ContinuousProblem + IOptimizer + OptimizationRunOptions
  → OptimizationRunner.Execute
  → OptimizationRunContext
  → ResetForRun → Advance* → OptimizationRunSummary
```

`OptimizationRunContext` 为每次执行新建，拥有 `Random(seed)`、评估计数与取消令牌。所有算法评估必须调用 `context.Evaluate`，以保持计数、取消与目标/约束结果验证一致；所有位置初始化与修改后必须调用 `context.Repair`。停止检查发生在初始化完成后和每次完整 `Advance` 后；因此评估阈值是迭代边界预算。

实验路径为：

```text
ExperimentDefinition → RunGroup plans → fixed workers
  → group factory → sequential Execute within a Group → ExperimentResult
```

计划按 Case 和 Group 下标稳定生成；seed 仅取决于 repetition 下标，因而不受 Group 数量、Worker 领取顺序或并发度影响。

## 所有权与线程安全

`ContinuousProblem` 防御性复制约束集合，并在未提供 Repair 时使用标量 `[0, 10]` Clamp。自定义 Repair 自己拥有边界；向量边界会在 Repair 创建时防御性复制，Problem 不公开它们。其用户提供的目标、约束、初始化器和修复策略是否可并发调用由用户决定。`IOptimizer` 拥有种群、临时数组与 `BestPosition`，不保证线程安全；一个实例只能属于一个 Group。正常的顺序执行可以复用数组，异常后必须丢弃实例。

`OptimizationRunSummary`、`Evaluation`、`ConstraintEvaluation` 和最终 Experiment 结果均为不可变快照。`BestPosition` 不是快照；它在下一次 reset 前才有效。`IStoppingCondition` 必须可重入，因此可从多个 Group 并发调用。

Experiment 的每个 Group 都创建独占的 Problem、Optimizer 和运行选项。跨 Group 只能共享调用方明确提供的不可变数据。`BestPositionMatrix` 的构造过程受 Case 内部锁保护，完成后只读。

## 错误、取消与确定性

目标值、约束违背和关键配置都会验证有限性与范围。候选位置不由 Core 验证：`ICandidateInitializer` 写入初值，算法每次修改后通过 `context.Repair` 委托 `ICandidateRepair`。内置 Clamp 是默认策略；DoNothing 是调用方明确承担后果的风险选择。非取消异常使当前 repetition 失败；Experiment 为同 Group 的后续 repetition 新建环境。取消停止投放新的 Group，已经开始的执行通过 Context 协作取消，最后返回部分结果。

禁止全局随机流、时间播种和跨执行共享逻辑状态。不要为优化方便改变随机调用顺序、数组布局、候选比较或 seed 派生；这些都是确定性契约的一部分。

## 新增算法

实现 `IOptimizer` 时，`ResetForRun` 必须完整初始化逻辑状态、在每个初始位置写入后调用 `context.Repair`、完成初始评估并建立最佳状态；`Advance` 必须完成一个完整迭代，并在每条修改位置的路径结束后调用 `context.Repair`。首次 reset 可按维度分配工作区；后续正常执行应复用它。算法不应读取变量上下界，不应把论文专用模型带入 Core，也不应直接依赖 Experiments。

至少补充固定 seed、最小化/最大化、约束比较、取消、并发隔离、异常后不复用及工作区复用测试。若声称性能收益，使用 BenchmarkDotNet 测量实际算法、约束处理和布局转换的端到端路径。
