# 当前架构概览

本文只描述当前架构、职责和运行流程，不记录选择理由。工程契约见
[`ENGINEERING.md`](../../ENGINEERING.md)，决策理由和替代关系见
[`docs/decisions/README.md`](../decisions/README.md)。

## 项目职责

| 项目 | 当前职责 |
| --- | --- |
| `Metaheuristics.Core` | 定义稳定的运行时契约，以及候选、目标、约束和执行所需的公共抽象。 |
| `Metaheuristics.Algorithms` | 提供具体优化算法和 `Optimizer` 实现；只依赖 `Core`。 |
| `Metaheuristics.Experiments` | 定义批量实验的强类型工厂、运行编排和结果聚合；只依赖 `Core`，不依赖具体算法项目。 |
| `Metaheuristics.Examples` | 展示从问题和算法装配到运行、结果处理的使用方式。 |
| `Metaheuristics.Tests` | 验证 `Core`、`Algorithms` 和 `Experiments` 的契约与行为。 |
| `Metaheuristics.Benchmarks` | 对核心和算法热路径进行基准测量，为性能改动提供数据依据。 |

## 项目依赖

依赖保持单向，批准的项目依赖图如下：

```text
Algorithms   ──→ Core
Experiments  ──→ Core
Examples     ──→ Core + Algorithms + Experiments
Tests        ──→ Core + Algorithms + Experiments
Benchmarks   ──→ Core + Algorithms
```

## 单次运行

单次运行接收调用方装配的 `Problem` 和 `Optimizer`，由库管理运行生命周期：

```text
Problem + Optimizer  →  Runner  →  Session  →  Result
```

`Optimizer` 保存可复用的算法定义和不可变参数；`Runner` 负责启动和协调运行；`Session` 独占本次运行的种群、随机流和临时缓冲区；`Result` 表示运行输出。

## 批量实验

批量实验由实验层使用强类型工厂创建隔离实例，并在全部运行完成后聚合结果：

```text
Experiments  →  typed factories  →  isolated runs  →  aggregate
```

每个独立 run 拥有自己的运行状态和随机流，聚合阶段只处理各 run 的结果。

## 当前扩展点

- 以强类型接口、显式依赖和组件组合扩展 `Problem`、`Optimizer` 及策略组件。
- 单次运行传入已装配的实例；批量实验通过强类型 factory 创建隔离实例。
- 算法实现位于 `Algorithms`，稳定契约位于 `Core`；扩展不得反向污染核心抽象。

## 首版排除项

- 远程执行、集群调度和 GPU 计算后端。
- 多目标、二进制和排列表示；也不为尚未进入路线图的表示类型、目标类型或后端预建复杂抽象。
- 全局字符串注册中心、服务定位器，以及按字符串类型名恢复组件的机制。
