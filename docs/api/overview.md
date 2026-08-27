---
uid: api-overview
---

# API Overview

本页只提供能力和入口地图。参数、返回值、异常、所有权和生命周期以源码 XML 注释生成的 API Reference 为准；第一次使用请从[用户使用手册](../guides/user-guide.md)开始。

## 定义优化问题

```text
ContinuousProblem
├─ IObjectiveFunction       必需：评价候选位置
├─ IConstraint             可选：报告非负约束违背量
└─ ICandidateRepair        可选：修复候选位置，默认 Clamp [0, 10]
```

主要入口：

- [ContinuousProblem](xref:Anastasya.Metaheuristics.Core.Problems.ContinuousProblem)
- [IObjectiveFunction](xref:Anastasya.Metaheuristics.Core.Problems.IObjectiveFunction)
- [IConstraint](xref:Anastasya.Metaheuristics.Core.Problems.IConstraint)
- [CandidateRepairs](xref:Anastasya.Metaheuristics.Core.Problems.CandidateRepairs)

## 执行一次优化

```text
Problem + Optimizer + OptimizationRunOptions
    → OptimizationRunner.Execute
    → OptimizationRunSummary + Optimizer.BestPosition
```

主要入口：

- [OptimizationRunner](xref:Anastasya.Metaheuristics.Core.Execution.OptimizationRunner)
- [OptimizationRunOptions](xref:Anastasya.Metaheuristics.Core.Execution.OptimizationRunOptions)
- [StoppingConditions](xref:Anastasya.Metaheuristics.Core.Execution.StoppingConditions)
- [BatOptimizer](xref:Anastasya.Metaheuristics.Algorithms.Bat.BatOptimizer)
- [BatOptimizerOptions](xref:Anastasya.Metaheuristics.Algorithms.Bat.BatOptimizerOptions)

## 运行重复实验

```text
ExperimentCase + ExperimentDefinition + ExperimentExecutionOptions
    → ExperimentRunner.RunAsync
    → ExperimentResult
```

主要入口：

- [ExperimentCase&lt;TConfiguration&gt;](xref:Anastasya.Metaheuristics.Experiments.Configuration.ExperimentCase`1)
- [ExperimentDefinition](xref:Anastasya.Metaheuristics.Experiments.Configuration.ExperimentDefinition)
- [ExperimentExecutionOptions](xref:Anastasya.Metaheuristics.Experiments.Configuration.ExperimentExecutionOptions)
- [ExperimentRunner](xref:Anastasya.Metaheuristics.Experiments.Execution.ExperimentRunner)
- [ExperimentResult](xref:Anastasya.Metaheuristics.Experiments.Results.ExperimentResult)

## 实现自己的策略

| 目的 | 扩展接口 |
| --- | --- |
| 定义目标函数 | [IObjectiveFunction](xref:Anastasya.Metaheuristics.Core.Problems.IObjectiveFunction) |
| 定义约束 | [IConstraint](xref:Anastasya.Metaheuristics.Core.Problems.IConstraint) |
| 生成初始候选 | [ICandidateInitializer](xref:Anastasya.Metaheuristics.Core.Problems.ICandidateInitializer) |
| 修复候选位置 | [ICandidateRepair](xref:Anastasya.Metaheuristics.Core.Problems.ICandidateRepair) |
| 定义停止规则 | [IStoppingCondition](xref:Anastasya.Metaheuristics.Core.Execution.IStoppingCondition) |
| 实现完整算法 | [IOptimizer](xref:Anastasya.Metaheuristics.Core.Execution.IOptimizer) |

实现契约、状态所有权、随机性和验证要求见[开发者架构手册](../architecture/developer-guide.md)。
