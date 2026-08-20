# 当前架构概览

本文只报告实现现状，不记录选择理由。工程契约见
[`ENGINEERING.md`](../../ENGINEERING.md)，决策理由和替代关系见
[`docs/decisions/README.md`](../decisions/README.md)。

## 当前状态

仓库已建立六个项目及其引用关系，但仍处于脚手架阶段：运行时公共 API、算法实现和实验执行尚未落地。

| 项目 | 实现现状 |
| --- | --- |
| `Metaheuristics.Core` | 项目骨架；尚无运行时公共类型。 |
| `Metaheuristics.Algorithms` | 项目骨架；尚无算法实现。 |
| `Metaheuristics.Experiments` | 项目骨架；尚无实验执行能力。 |
| `Metaheuristics.Examples` | 占位控制台入口。 |
| `Metaheuristics.Tests` | 仅包含目标运行时检查。 |
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

## 已接受、尚未落地的设计

### 单次运行模型（v0.1）

按照 [ADR-0004](../decisions/0004-composition-and-execution-model.md)，v0.1 将采用以下单次运行模型：

```text
Problem + Optimizer  →  Runner  →  Session  →  Result
```

### 批量实验流程（v0.2）

批量实验的目标流程为：

```text
Experiments  →  typed factories  →  isolated runs  →  aggregate
```

这些名称和流程是已接受的设计，不是当前公共 API 或扩展点。

## 首版排除项

- 远程执行、集群调度和 GPU 计算后端。
- 多目标、二进制和排列表示；也不为尚未进入路线图的表示类型、目标类型或后端预建复杂抽象。
- 全局字符串注册中心、服务定位器，以及按字符串类型名恢复组件的机制。

## API 文档

当前没有 API 文档，因为运行时公共 API 尚未实现。首批公共 API 加入时同步增加 API 文档和可运行示例。
