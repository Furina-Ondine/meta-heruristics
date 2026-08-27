---
uid: developer-guide
---

# 开发者架构手册

本手册面向实现新策略、算法或修改现有职责的人。持续有效的规则以 [`ENGINEERING.md`](../../ENGINEERING.md) 为准，选择理由以 [ADR](../decisions/README.md) 为准，具体功能行为以 [Approved/Implemented Spec](../specs/README.md) 为准。

## 先选择扩展点

```text
只改变评价             → IObjectiveFunction / IConstraint
只改变初始位置         → ICandidateInitializer
只改变位置恢复         → ICandidateRepair
只改变停止规则         → IStoppingCondition
改变完整搜索过程       → IOptimizer
组织多配置和重复运行   → Experiments 的 typed Case + Group factory
```

Core 定义最小协议和运行生命周期；调用方显式组装策略。不要为扩展建立字符串注册中心、服务定位器或运行时程序集扫描。

## 实现 Objective 或 Constraint

Objective 只根据位置计算目标值，不负责停止、比较、Repair 或随机数。它可以返回有限值或正负 Infinity，但不得返回 `NaN`。Constraint 返回归一化的非负违背量：零表示满足，`+Infinity` 表示无界违背；负值、`-Infinity` 和 `NaN` 违反契约。

所有算法评价候选都必须通过 `OptimizationRunContext.Evaluate`，让执行上下文统一处理计数、取消以及当前公共数值契约。不要直接调用 Problem 或 Objective 绕过 Context。

Core 只在每个策略结果的标量边界验证该数值域，不检查候选位置，也不要求算法或比较器重复验证。Infinity 的排序、停止和跨 run 统计规则见 [ADR-0015](../decisions/0015-ordered-extended-evaluation-values.md)；成员异常和 nullable 统计条件见生成式 API Reference。

Objective 和 Constraint 是否能被多个 RunGroup 并发调用由实现者负责。如果共享底层数据，该数据必须不可变或自行同步。

## 实现 Initializer 或 Repair

Initializer 只写入传入的位置并使用运行提供的 `Random`。算法在 Initializer 返回后立即调用 `context.Repair`。

Repair 拥有自己的边界或其他恢复数据。算法和 Problem 不读取这些数据；算法每次修改候选位置后都必须调用 `context.Repair`。向量端点等构造输入应在策略创建时复制和验证，避免在热路径重复处理配置。

策略失职造成的位置后果由策略和调用方负责；Core 不重新实现位置合法性判断或静默修复。

## 实现 Stopping Condition

`IStoppingCondition` 接收不可变的 `OptimizationState` 并返回可选停止原因。实现必须可重入，不能保存 run 级可变状态，因为同一实例可能被不同 Group 并发调用。

停止条件观察迭代边界状态。需要更细粒度预算时，应先形成新的公共行为 Spec，而不是从算法内部绕开 Runner。

## 实现 Optimizer

一次单次运行的调用顺序为：

```text
OptimizationRunner.Execute
  → new OptimizationRunContext(seed, cancellation)
  → optimizer.ResetForRun(context)
  → check stop
  → optimizer.Advance()
  → check stop
  → ...
  → OptimizationRunSummary
```

实现必须满足：

- `ResetForRun` 完整覆盖上一 run 的逻辑状态，并产生有效的最佳状态；
- 初次 reset 可以按问题维度分配主要工作区，正常的后续 run 应复用它；
- 每个初始位置以及每次位置修改后调用 `context.Repair`；
- 每次评价只调用 `context.Evaluate`；
- `Advance` 完成一个不可拆分的算法迭代；
- 不使用全局随机流、当前时间播种或跨 run 逻辑状态；
- 不读取 Repair 的边界，不把算法专属表示放进 Core。

`IOptimizer` 拥有种群、临时缓冲区和 `BestPosition`，不保证线程安全。一个实例只能由一个 RunGroup 顺序驱动；执行异常后不得复用。返回的最佳位置是借用工作区而不是快照，精确成员契约见生成式 API Reference。

至少覆盖固定 seed、最小化/最大化、约束比较、取消、并发隔离、异常后不复用以及工作区复用测试。性能主张必须有端到端 BenchmarkDotNet 或分配证据。

## 接入 Experiments

```text
ExperimentDefinition
  → stable RunGroup plans
  → bounded fixed workers
  → typed group factory
  → sequential runs in each Group
  → ExperimentResult
```

每个 Group 创建独占 Problem、Optimizer 和 RunOptions。同一 Group 的正常 run 可以复用物理工作区；异常后 Runner 丢弃环境并为后续 repetition 重建。不同 Group 只能共享调用方明确提供的不可变底层数据。

seed 只取决于 Experiment 计划和 repetition 下标，不依赖 Group 拆分、Worker 领取顺序或并发度。结果读取顺序同样不依赖任务完成顺序。

Experiments 只依赖 Core，不能引用具体算法。具体算法由调用方在 typed Group factory 中组装。

目标值统计覆盖所有成功 run，不过滤 Infinity。`NumericStatistics.Mean`、`Median` 和 `StandardDeviation` 可能为 `null`；Experiment 展示或导出层必须显式表达 undefined，不能用零或 `NaN` 代替。

## 修改现有职责

公共 API、行为、项目职责、策略或执行抽象、状态、随机性、数值语义、性能或跨项目变化必须先建立完整 [change package](../specs/README.md)。Plan 在提出方案前必须调查：

- 当前类型、入口和调用链；
- 其他层是否已有相似概念；
- 新设计替代哪些旧类型、测试和文档；
- Core、Algorithms、Experiments、Examples、Tests、Benchmarks、XML 和用户文档的连带影响；
- 哪些检查位于热路径，以及它们保护哪个库不变量。

职责迁移默认删除失去独立价值的旧入口、转发层和兼容壳。需要保留时必须记录真实消费者、期限和删除条件。

## 项目边界与验证

```text
Algorithms   ──→ Core
Experiments  ──→ Core
Examples     ──→ Core + Algorithms + Experiments
Tests        ──→ Core + Algorithms + Experiments
Benchmarks   ──→ Core + Algorithms
```

完成变更前至少执行：

```powershell
dotnet tool restore
dotnet restore Metaheuristics.NET.slnx --property:NuGetAudit=false
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
pwsh ./eng/verify-documentation.ps1
dotnet docfx docfx.json --warningsAsErrors
dotnet format Metaheuristics.NET.slnx --verify-no-changes --no-restore
```

当前项目结构和运行流程见[架构概览](overview.md)，具体类型入口见 [API Overview](../api/overview.md)。
