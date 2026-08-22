# Experiment 第一版实施计划

## 目标

实现 [Experiment 执行架构与接口设计](../specs/2026-08-22-experiment-execution-design.md)，并落实 [ADR-0009](../../decisions/0009-group-scoped-optimizer-execution.md) 与 [ADR-0010](../../decisions/0010-scalar-evaluation-baseline.md)。

## 任务 1：重构 Core 执行契约

- 撤销 `IOptimizationSession`，让 `IOptimizer` 直接暴露 `ResetForRun`、`Advance` 和最佳状态。
- 将 `OptimizationContext` 重命名为 `OptimizationRunContext`，保持每 run 独立的 Problem、seed、Random、取消和评估计数。
- 从 `OptimizationRunOptions` 移除 seed，改为 Runner 显式参数。
- 增加不复制最佳位置的运行汇总路径，供 Experiment 在下一次 Reset 前直接写入结果矩阵。
- 保留单次运行的不可变 `OptimizationResult` 便利 API。

## 任务 2：撤销批量评估

- 删除 `IBatchObjectiveFunction`。
- 删除 `ContinuousProblem.EvaluateBatch` 和 `OptimizationRunContext.EvaluateBatch`。
- 更新 Core 测试、示例和 API 文档，只保留单点评估契约。

## 任务 3：实现 Experiment 定义与规划

- 增加异构 Case 基类和强类型 `ExperimentCase<TConfiguration>`。
- 增加每 Group 调用一次的强类型 Factory、Group Setup 和 Group Context。
- 验证 Case ID、Repetition、`RunGroupCount`、并发度和 seed。
- 按确定性均衡规则拆分 Repetition，并按 Case 轮转产生 RunGroup。

## 任务 4：实现有界调度与容错

- 使用固定数量的 CPU Worker Task 惰性消费 RunGroup，不为每个 run 创建 Task。
- 每个 Group 顺序复用同一个 IOptimizer。
- run 失败后记录异常、丢弃 Optimizer，并为剩余 run 重建 Group 环境。
- 取消后停止投放新 Group，区分 `Canceled` 和 `NotStarted`，返回部分结果。
- 保证结果顺序和 seed 不受并发度与 Group 拆分影响。

## 任务 5：实现结果与统计

- 使用 Case 级 `double[,]` 保存成功 run 的最佳位置，并提供只读包装。
- 增加 run、Case 和 Experiment 的四状态结果。
- 对成功 run 计算目标值、迭代数、评估数和耗时的均值、中位数、最小值、最大值和样本标准差。
- 提供成功、失败、取消和未开始数量。

## 任务 6：测试、示例和文档

- 覆盖 Core Optimizer 复用、状态重置、seed、取消和零复制结果路径。
- 覆盖规划均衡、全局并发上限、失败重建、初始化失败、部分取消、二维结果矩阵和统计边界。
- 更新示例以实现新的 IOptimizer。
- 新增 Experiment API 文档，并在实现完成后同步架构概览。

## 验证

```powershell
dotnet restore Metaheuristics.NET.slnx
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
dotnet format Metaheuristics.NET.slnx --verify-no-changes
git diff --check
```

内存复用不承诺具体倍数；以行为测试证明同一 Optimizer 在 Group 内复用，并在正式算法落地后补充代表性分配基准。
