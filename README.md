# Metaheuristics.NET

面向 .NET 的通用、高性能元启发式优化算法库。

> [!IMPORTANT]
> 项目处于早期开发阶段，公共 API 尚未稳定，不建议用于生产环境。

## 首版范围

首版聚焦连续单目标优化：候选位置为 `double` 向量，目标值为标量 `double`，支持最小化、最大化、变量边界及约束。目标评估采用单点同步接口；运行之间保持随机状态隔离和可复现性。

首版不包含远程、集群、GPU、多目标、二进制或排列表示，也不提供运行时插件发现。

## 项目结构

`Core` 提供稳定契约，`Algorithms` 提供算法，`Experiments` 编排批量实验；`Examples`、`Tests` 和 `Benchmarks` 分别提供示例、测试和性能测量。职责与依赖见[架构概览](docs/architecture/overview.md)。

## 路线图

- `v0.1`：Core、运行模型、结果模型、实验管理和蝙蝠算法；
- `v0.2`：约束体系、PSO 和连续遗传算法；
- `v0.3`：布谷鸟搜索；
- `v0.4`：萤火虫算法、扩展示例和基准；
- `v1.0`：统一质量门槛。

后续按“泛型单目标值 → 多目标 → 二进制/排列表示”演进，详见[ADR 索引](docs/decisions/README.md)。

## 开发

环境要求：与 `global.json` 兼容的 .NET SDK 10.0.400。

```powershell
dotnet restore Metaheuristics.NET.slnx
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
dotnet run --project examples/Metaheuristics.Examples/Metaheuristics.Examples.csproj --configuration Release --no-build
```

## 文档

- [工程规范](ENGINEERING.md)：长期有效的代码、架构、测试和发布规则；
- [架构概览](docs/architecture/overview.md)：当前项目职责、依赖和运行流程；
- [Core API](docs/api/core.md)：第一波连续问题、运行模型和扩展契约；
- [Algorithms API](docs/api/algorithms.md)：蝙蝠算法配置、生命周期、迁移来源和边界；
- [Experiments API](docs/api/experiments.md)：Case、RunGroup、并发调度、失败与结果统计；
- [RunGroup 调度基准](docs/benchmarks/run-group-scheduling.md)：固定 Worker、Parallel API 与信号量方案在长短计划下的对照；
- [Superpowers 设计与实施档案](docs/superpowers/README.md)：按变更保存设计规格和实施计划，不作为长期工程规范；
- [ADR 索引](docs/decisions/README.md)：决策背景、替代方案和重新评估条件。

## 项目来源

本项目源自学位论文实验仓库 [task-schedule](https://github.com/Furina-Ondine/task-schedule)。旧仓库保留为历史实现和实验归档；其 [`fix` 分支](https://github.com/Furina-Ondine/task-schedule/tree/fix)包含部分问题的快速修复。
