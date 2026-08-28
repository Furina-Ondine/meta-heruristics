# 当前架构概览

本文只报告实现现状，不记录选择理由。工程契约见
[`ENGINEERING.md`](../../ENGINEERING.md)，决策理由和替代关系见
[`docs/decisions/README.md`](../decisions/README.md)。

## 当前状态

仓库已建立六个项目及其引用关系。第一波 Core 公共 API、单次运行生命周期、Experiment 执行以及四种连续算法已经落地。

| 项目 | 实现现状 |
| --- | --- |
| `Metaheuristics.Core` | 已提供连续问题、有序扩展实数评估与比较、有状态 Optimizer、run Context、停止、轨迹、结果和单次 Runner API；内置 Clamp 与安全有限 Reflect Repair 使用 `System.Numerics.Tensors` 的逐元素实现，并保留特殊值标量回退。 |
| `Metaheuristics.Algorithms` | 已提供连续蝙蝠、PSO、萤火虫和布谷鸟算法；每种算法都有强类型配置、RunGroup 私有工作区与顺序 run 复用。 |
| `Metaheuristics.Experiments` | 已提供强类型 Case、RunGroup 规划、有界并发、共享 seed、部分失败/取消结果，以及可显式表达 Infinity 未定义项的基本统计。 |
| `Metaheuristics.Examples` | 已提供四种内置算法的单次运行和可替换 Optimizer 的 Experiment 示例。 |
| `Metaheuristics.Tests` | 已包含目标运行时、Core、Experiment 以及四种内置算法的契约与行为测试。 |
| `Metaheuristics.Benchmarks` | 已提供蝙蝠算法工作区复用基准，以及固定 Worker、Parallel API 和信号量 RunGroup 调度基准。 |

## 项目依赖

依赖保持单向，批准的项目依赖图如下：

```text
Algorithms   ──→ Core
Experiments  ──→ Core
Examples     ──→ Core + Algorithms + Experiments
Tests        ──→ Core + Algorithms + Experiments
Benchmarks   ──→ Core + Algorithms
```

## 运行与实验模型

### 单次运行模型

```text
Problem + stateful Optimizer  →  Runner
    →  new run Context  →  ResetForRun  →  Advance...  →  Result/Summary
```

由 RunGroup 独占的有状态 Optimizer 直接持有并复用工作区。每次 run 创建新的 Context 和 `Random(seed)`，重新初始化逻辑状态。Core 只提供标量单点评估，不规定算法的种群内存布局。决策见 [ADR-0009](../decisions/0009-group-scoped-optimizer-execution.md) 和 [ADR-0010](../decisions/0010-scalar-evaluation-baseline.md)。

### 批量实验流程

```text
Cases  →  plan RunGroups  →  bounded scheduler
       →  group factories  →  reusable Optimizers  →  aggregate
```

Case 内用 `RunGroupCount` 表达用户掌握的并发拆分；所有 Group 再由单一全局并发上限调度。详细设计见 [Experiment 执行架构与接口设计](../superpowers/specs/2026-08-22-experiment-execution-design.md)。

当前调度器使用固定数量的长期 Worker 动态领取 RunGroupPlan，不为每个计划创建 Task。它与 `Parallel.ForEachAsync` 的 worker-loop 结构接近；不同计划时长下的实现对照和线程池指标见 [RunGroup 调度基准](../benchmarks/run-group-scheduling.md)。

## 首版排除项

- 远程执行、集群调度和 GPU 计算后端。
- 多目标、二进制和排列表示；也不为尚未进入路线图的表示类型、目标类型或后端预建复杂抽象。
- 全局字符串注册中心、服务定位器，以及按字符串类型名恢复组件的机制。

## 当前算法

内置连续算法为蝙蝠、PSO、萤火虫和布谷鸟。它们都不读取 Problem 的逐维位置边界：调用方提供位置初始化器，算法在初始化和每次位置更新后通过 Context 调用 Problem 配置的 Repair。默认 Repair 为标量 `[0, 10]` Clamp；每个 RunGroup 独占的 Optimizer 保存并复用自身种群及临时状态。布谷鸟的 Lévy 与遗弃尺度由显式 Options 表达，不从 Repair 提取边界。迁移顺序和旧仓库修复来源见 [ADR-0011](../decisions/0011-bat-first-algorithm-migration.md)，候选边界职责见 [ADR-0013](../decisions/0013-tensor-shaped-repair-bounds.md)。

## API 文档

按任务寻找公共入口时使用 [API Overview](../api/overview.md)，参数、异常、所有权和成员生命周期以按 API Overview 中的生成说明构建的 XML 注释 API Reference 为准。可运行示例见 [`examples/Metaheuristics.Examples/Program.cs`](../../examples/Metaheuristics.Examples/Program.cs)。
