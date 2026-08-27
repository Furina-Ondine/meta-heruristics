---
uid: api-overview
---

# API Overview

本页只提供能力和入口地图。下面的类型名称直接链接到源码声明；参数、返回值、异常、所有权和生命周期以源码 XML 注释生成的 [API Reference](../reference/index.md) 为准。第一次使用请从[用户使用手册](../guides/user-guide.md)开始。

## 定义优化问题

```text
ContinuousProblem
├─ IObjectiveFunction       必需：评价候选位置
├─ IConstraint             可选：报告非负约束违背量
└─ ICandidateRepair        可选：修复候选位置，默认 Clamp [0, 10]
```

主要入口：

- [ContinuousProblem](../../src/Metaheuristics.Core/Problems/ContinuousProblem.cs)
- [IObjectiveFunction](../../src/Metaheuristics.Core/Problems/IObjectiveFunction.cs)
- [IConstraint](../../src/Metaheuristics.Core/Problems/IConstraint.cs)
- [CandidateRepairs](../../src/Metaheuristics.Core/Problems/CandidateRepairs.cs)

## 执行一次优化

```text
Problem + Optimizer + OptimizationRunOptions
    → OptimizationRunner.Execute
    → OptimizationRunSummary + Optimizer.BestPosition
```

主要入口：

- [OptimizationRunner](../../src/Metaheuristics.Core/Execution/OptimizationRunner.cs)
- [OptimizationRunOptions](../../src/Metaheuristics.Core/Execution/OptimizationContracts.cs)
- [StoppingConditions](../../src/Metaheuristics.Core/Execution/StoppingConditions.cs)
- [BatOptimizer](../../src/Metaheuristics.Algorithms/Bat/BatOptimizer.cs)
- [BatOptimizerOptions](../../src/Metaheuristics.Algorithms/Bat/BatOptimizerOptions.cs)

## 运行重复实验

```text
ExperimentCase + ExperimentDefinition + ExperimentExecutionOptions
    → ExperimentRunner.RunAsync
    → ExperimentResult
```

主要入口：

- [ExperimentCase&lt;TConfiguration&gt;](../../src/Metaheuristics.Experiments/Configuration/ExperimentCase.cs)
- [ExperimentDefinition](../../src/Metaheuristics.Experiments/Configuration/ExperimentDefinition.cs)
- [ExperimentExecutionOptions](../../src/Metaheuristics.Experiments/Configuration/ExperimentExecutionOptions.cs)
- [ExperimentRunner](../../src/Metaheuristics.Experiments/Execution/ExperimentRunner.cs)
- [ExperimentResult](../../src/Metaheuristics.Experiments/Results/ExperimentResults.cs)

## 实现自己的策略

| 目的 | 扩展接口 |
| --- | --- |
| 定义目标函数 | [IObjectiveFunction](../../src/Metaheuristics.Core/Problems/IObjectiveFunction.cs) |
| 定义约束 | [IConstraint](../../src/Metaheuristics.Core/Problems/IConstraint.cs) |
| 生成初始候选 | [ICandidateInitializer](../../src/Metaheuristics.Core/Problems/ICandidateInitializer.cs) |
| 修复候选位置 | [ICandidateRepair](../../src/Metaheuristics.Core/Problems/ICandidateRepair.cs) |
| 定义停止规则 | [IStoppingCondition](../../src/Metaheuristics.Core/Execution/OptimizationContracts.cs) |
| 实现完整算法 | [IOptimizer](../../src/Metaheuristics.Core/Execution/OptimizationContracts.cs) |

实现契约、状态所有权、随机性和验证要求见[开发者架构手册](../architecture/developer-guide.md)。
