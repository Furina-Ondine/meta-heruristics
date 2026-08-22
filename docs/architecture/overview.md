# 当前架构概览

本文只报告实现现状，不记录选择理由。工程契约见
[`ENGINEERING.md`](../../ENGINEERING.md)，决策理由和替代关系见
[`docs/decisions/README.md`](../decisions/README.md)。

## 当前状态

仓库已建立六个项目及其引用关系。第一波 Core 公共 API、单次运行生命周期和 Experiment 执行已经落地；正式算法实现尚未落地。

| 项目 | 实现现状 |
| --- | --- |
| `Metaheuristics.Core` | 已提供连续问题、标量评估与比较、有状态 Optimizer、run Context、停止、轨迹、结果和单次 Runner API。 |
| `Metaheuristics.Algorithms` | 项目骨架；尚无算法实现。 |
| `Metaheuristics.Experiments` | 已提供强类型 Case、RunGroup 规划、有界并发、共享 seed、部分失败/取消结果和基本统计。 |
| `Metaheuristics.Examples` | 已提供单次运行和双 Case Experiment 的随机搜索示例；尚无正式算法示例。 |
| `Metaheuristics.Tests` | 已包含目标运行时检查、Core 契约以及 Experiment 调度与结果行为测试。 |
| `Metaheuristics.Benchmarks` | 占位基准宿主，尚无基准用例。 |

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

## 首版排除项

- 远程执行、集群调度和 GPU 计算后端。
- 多目标、二进制和排列表示；也不为尚未进入路线图的表示类型、目标类型或后端预建复杂抽象。
- 全局字符串注册中心、服务定位器，以及按字符串类型名恢复组件的机制。

## API 文档

公共契约见 [Core API](../api/core.md) 和 [Experiments API](../api/experiments.md)，可运行示例见 [`examples/Metaheuristics.Examples/Program.cs`](../../examples/Metaheuristics.Examples/Program.cs)。
