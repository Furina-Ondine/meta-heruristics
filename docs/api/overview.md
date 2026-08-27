---
uid: api-overview
---

# API Overview

本页只提供能力和入口地图。生成网页后，下面的类型名称通过 `xref:` 链接跳转到成员级 API Reference；Markdown 源文件无法直接解析这些链接。请先按[下方生成说明](#生成-api-reference)构建并打开网页。第一次使用请从[用户使用手册](../guides/user-guide.md)开始。

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

## 生成 API Reference

API Reference 不是仓库中的手写页面，而是由 DocFX 根据运行时程序集签名和源码 XML 注释生成的本地产物。Markdown 源文件中的 `xref:` 链接只有在生成的网站中才能解析；请从仓库根目录执行：

```powershell
dotnet tool restore
dotnet docfx docfx.json --warningsAsErrors
```

构建成功后，在浏览器中打开 `docs/reference/_site/docs/api/overview.html`。其中的类型链接会跳转到生成的成员级 API Reference。

`docs/reference/api`（DocFX 元数据）和 `docs/reference/_site`（静态网站）均为本地生成目录，不提交到 Git；删除后可按上述步骤重新生成。
