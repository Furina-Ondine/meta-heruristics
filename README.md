# Metaheuristics.NET

面向 .NET 的通用、高性能元启发式优化算法库。

> [!IMPORTANT]
> 项目目前处于设计和早期开发阶段，公共 API 尚未稳定，不建议用于生产环境。

## 项目目标

Metaheuristics.NET 希望在清晰、可扩展的 API 之上，同时满足科研实验和工程求解的需要：

- 提供经过验证的经典元启发式算法实现；
- 支持单目标、有约束的连续优化；
- 保证独立运行之间的状态和随机流隔离；
- 在明确边界内提供可复现的实验结果；
- 支持本机 CPU 上的高性能单点评估和可选批量评估；
- 允许用户通过强类型接口实现目标函数、算法、约束策略和停止条件；
- 提供批量实验、统计报告、标准测试函数和性能基准。

## 首个稳定版本的范围

首个稳定版本聚焦于把连续单目标优化做好：

- 候选位置采用 `double` 连续向量；
- 目标值采用 `double` 标量，支持最小化和最大化；
- 支持变量上下界、等式约束和不等式约束；
- 默认使用可行性优先和归一化约束违背量比较候选解；
- 单点评估是基础契约，批量评估是可选的本机快速路径；
- 相同库版本、运行时和执行设置下，相同种子产生相同结果；
- 单次求解传入实例，批量实验通过强类型工厂创建相互隔离的运行实例。

首版不包含计算集群、远程执行、GPU 后端、运行时插件发现、多目标优化、二进制表示或排列表示。

## 架构

```text
Metaheuristics.Core
├── 连续优化问题、边界与约束
├── 目标函数与候选解评估
├── Optimizer、Session 与运行上下文
├── 随机流、停止条件与结果模型
└── 单次优化执行器

Metaheuristics.Algorithms
├── PSO
├── Genetic Algorithm
├── Cuckoo Search
├── Bat Algorithm
└── Firefly Algorithm

Metaheuristics.Experiments
├── 多次独立运行与并发控制
├── 确定性种子分配
├── 收敛历史采集
└── 汇总统计与报告

Metaheuristics.Examples
├── 无约束与约束优化示例
├── 自定义目标函数和算法
└── 可复现批量实验

Metaheuristics.Tests
└── 契约、正确性、可复现性和并发隔离测试

Metaheuristics.Benchmarks
└── 分配、向量运算和算法吞吐量基准
```

依赖关系保持单向：

```text
Algorithms   ──→ Core
Experiments  ──→ Core
Examples     ──→ Core + Algorithms + Experiments
Tests        ──→ Core + Algorithms + Experiments
Benchmarks   ──→ Core + Algorithms
```

## 执行模型

算法定义与单次运行状态相互分离：

```text
Optimizer（算法及不可变参数，可复用）
└── Session（种群、随机流和临时缓冲区，仅属于一次运行）
```

Core 负责运行循环、评估计数、停止条件、取消、约束比较和结果生成；用户负责选择并装配算法与问题；Experiments 负责重复创建独立运行并汇总结果。

## 路线图

### 基础版本

- **v0.1**：建立 Core、随机流、运行模型、结果模型和 PSO 参考实现；
- **v0.2**：完成约束体系、连续遗传算法和基础实验管理；
- **v0.3**：迁移并验证布谷鸟搜索；
- **v0.4**：迁移并验证蝙蝠算法、萤火虫算法，补齐 Examples 与 Benchmarks；
- **v1.0**：五种算法通过统一的正确性、可复现性、约束处理和性能质量门槛。

### 后续演进

1. 将单目标值泛型化，通过 Objective Policy 定义比较、改进度量和统计投影能力；
2. 引入多目标向量、Pareto 支配、非支配解集及相应算法；
3. 增加二进制和排列候选表示，以及对应的初始化器和搜索算子；
4. 根据实际需求评估更多算法和计算后端，避免提前引入分布式系统复杂度。

## 质量原则

算法进入稳定 API 前必须满足以下要求：

- 实现与可信文献或标准伪代码一致；
- 固定种子下结果可复现；
- 不使用全局随机数或跨运行共享可变状态；
- 不产生未处理的越界、`NaN` 或无穷值；
- 在标准测试函数上表现合理；
- 具有单元测试、统计验证和性能基准；
- 参数默认值和来源有明确文档。

## 开发

环境要求：.NET SDK 10.0.400 或满足 `global.json` 滚动策略的兼容补丁版本。

```powershell
dotnet restore Metaheuristics.NET.slnx
dotnet build Metaheuristics.NET.slnx --configuration Release --no-restore
dotnet test Metaheuristics.NET.slnx --configuration Release --no-build
dotnet run --project examples/Metaheuristics.Examples/Metaheuristics.Examples.csproj --configuration Release --no-build
```

性能基准将在首个算法实现进入仓库时加入；基准宿主项目已预先建立，以固定依赖方向和构建入口。

## 项目来源

本项目源自学位论文实验仓库 [task-schedule](https://github.com/Furina-Ondine/task-schedule)。旧仓库以完成论文实验为主要目标，保留为历史实现和实验归档；其中 [`fix` 分支](https://github.com/Furina-Ondine/task-schedule/tree/fix) 包含部分问题的快速修复。Metaheuristics.NET 将在相关实践经验基础上重新设计，而不是直接把研究原型包装成公共库。
