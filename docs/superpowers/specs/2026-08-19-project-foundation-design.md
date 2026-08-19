# Metaheuristics.NET 项目基础设计

## 1. 目的

Metaheuristics.NET 是面向 .NET 的通用、高性能元启发式优化算法库。项目同时服务科研实验和工程求解，但通过分层避免实验管理逻辑污染算法热路径。

本设计用于指导新仓库的项目结构、依赖边界和后续功能演进。旧仓库 `task-schedule` 保留为学位论文实验代码归档，新仓库不直接复制其既有架构。

## 2. 首个稳定版本的范围

首个稳定版本只原生支持单目标、有约束的连续优化：

- 候选位置固定为 `double` 连续向量；
- 目标值固定为 `double` 标量；
- 支持最小化和最大化；
- 支持逐维上下界、等式约束和不等式约束；
- 单点评估是基础契约，本机同步批量评估是可选快速路径；
- 用户能够通过公开强类型接口实现目标函数、算法、初始化器、约束策略和停止条件；
- 相同库版本、运行时和执行设置下，相同种子必须产生相同结果；
- 改变批次并发度不得改变每个独立 run 的随机序列。

首版明确不包含：

- 多目标优化；
- 二进制、整数或排列候选表示；
- 运行时程序集插件发现；
- 集群调度、远程执行和 GPU 后端；
- 只依靠 JSON 字符串类型名恢复任意用户组件。

## 3. 项目结构

技术基线为 .NET 10 SDK，所有项目目标框架统一为 `net10.0`，使用该 SDK 对应的默认稳定 C# 语言版本。运行时项目必须保持跨平台，不依赖 Windows 专属 API。程序集和根命名空间统一使用 `Metaheuristics.*`。

仓库采用以下目录和项目：

```text
Metaheuristics.NET.slnx
Directory.Build.props
Directory.Packages.props
global.json

src/
├── Metaheuristics.Core/
├── Metaheuristics.Algorithms/
└── Metaheuristics.Experiments/

examples/
└── Metaheuristics.Examples/

tests/
└── Metaheuristics.Tests/

benchmarks/
└── Metaheuristics.Benchmarks/

docs/
└── superpowers/
    ├── specs/
    └── plans/
```

职责如下：

- `Metaheuristics.Core`：问题模型、目标函数、边界、约束、候选比较、优化器公共接口、Session、运行上下文、随机流、停止条件、结果和单次执行器；
- `Metaheuristics.Algorithms`：PSO、连续遗传算法、布谷鸟搜索、蝙蝠算法和萤火虫算法；
- `Metaheuristics.Experiments`：多次独立运行、并发控制、种子派生、收敛历史汇总、统计报告和导出；
- `Metaheuristics.Examples`：最小可运行示例、自定义扩展示例和批量实验示例；
- `Metaheuristics.Tests`：契约、正确性、约束、可复现性和并发隔离测试；
- `Metaheuristics.Benchmarks`：分配、向量运算和算法吞吐量基准，不作为运行时包发布。

## 4. 依赖边界

项目引用必须保持单向：

```text
Algorithms   ──→ Core
Experiments  ──→ Core
Examples     ──→ Core + Algorithms + Experiments
Tests        ──→ Core + Algorithms + Experiments
Benchmarks   ──→ Core + Algorithms
```

`Core` 不引用任何其他项目。`Experiments` 只认识 Core 中的优化器契约，不引用具体算法。论文业务代码不进入新仓库的运行时项目。

## 5. 问题与评估模型

连续问题由以下概念组成：

- 维度；
- 每一维的上下界；
- 单目标函数；
- 零个或多个约束；
- 优化方向；
- 可选候选修复策略。

首版评估结果采用固定标量模型：

```csharp
public readonly record struct Evaluation(
    double Objective,
    ConstraintEvaluation Constraints);
```

目标函数首先提供单点同步契约。可选批量契约只用于本机 CPU 快速路径；算法在目标函数不支持批量评估时自动退回逐个评估。

Core 返回最佳连续位置，不在热路径中构造 `Schedule`、`Route` 等领域对象。调用者在运行结束后根据最佳位置解码领域解。

## 6. 约束模型

约束比较默认采用可行性优先规则：

1. 可行解优于不可行解；
2. 两个解都可行时比较目标值；
3. 两个解都不可行时比较加权、归一化的总违背量；
4. 总违背量相同时比较目标值。

对于不等式 `g(x) <= 0`：

```text
violation = max(0, g(x)) / scale * weight
```

对于等式 `h(x) = target`：

```text
tolerance = absoluteTolerance
          + relativeTolerance * max(abs(h(x)), abs(target))

violation = max(0, abs(h(x) - target) - tolerance)
          / scale * weight
```

等式约束必须具备显式容差。可以通过重新参数化消除的等式应优先消除；候选修复或投影作为独立扩展点，不与约束判定混为一体。

约束评估至少暴露：

- `IsFeasible`；
- `TotalViolation`；
- `MaxViolation`；
- `ViolatedCount`。

## 7. 构造职责与执行模型

职责划分为：用户负责装配，库负责生命周期。

- 单次求解允许用户直接传入问题和优化器实例；
- 批量实验由用户提供强类型问题工厂和优化器工厂；
- 库决定何时创建、运行和释放每个 run 的对象；
- Core 不使用全局字符串注册中心或 `object` 配置恢复用户组件。

算法定义和单次运行状态严格分离：

```text
Optimizer
├── 算法类型和不可变参数
├── 可以安全复用
└── 创建 Session

Session
├── 种群和个体状态
├── 当前迭代与当前最优
├── 独立随机流
├── 临时缓冲区
└── 仅属于一次运行，不保证线程安全
```

Core Runner 统一执行：

1. 根据 run seed 创建独立随机流；
2. 创建运行上下文和 Session；
3. 初始化 Session；
4. 循环执行 Step；
5. 统一执行评估计数、约束比较、停止和取消检查；
6. 在释放 Session 前复制不可变结果；
7. 释放 Session 及其池化缓冲区。

算法不得使用 `Random.Shared` 或按当前时间自行播种。

## 8. 停止、结果与追踪

Core 支持组合停止条件：

- 最大迭代次数；
- 最大评估次数；
- 最大运行时间；
- 连续若干次无显著改进；
- 达到目标值；
- 外部取消。

最大评估次数是一等预算，因为不同算法每次迭代消耗的评估次数不同。

单次结果至少包含：

- 最佳位置及其评估；
- 终止原因；
- 迭代次数和评估次数；
- 运行时长；
- run seed；
- 可选收敛轨迹。

轨迹采集支持关闭、仅记录改进、逐迭代记录和按评估间隔记录。默认不保存每代完整种群，避免不必要的内存和 I/O 成本。

## 9. 算法迁移顺序

算法逐个达到统一门槛，而不是一次性迁移：

- `v0.1`：Core 和 PSO 参考实现；
- `v0.2`：约束体系、连续遗传算法和基础实验管理；
- `v0.3`：布谷鸟搜索；
- `v0.4`：蝙蝠算法、萤火虫算法、Examples 和 Benchmarks；
- `v1.0`：五种算法全部通过统一质量门槛。

每个稳定算法必须满足：实现依据明确、固定种子可复现、无跨运行状态泄漏、边界和无效浮点值得到处理、标准测试函数表现合理，并具备单元测试、统计验证和性能基准。

## 10. 后续演进

首个稳定版本之后按以下顺序扩展：

1. 将单目标值泛型化，由 Objective Policy 定义比较、改进度量和统计投影能力；
2. 引入多目标向量、Pareto 支配、非支配解集以及多目标算法接口；
3. 引入二进制和排列候选表示及其专用初始化器、变异、交叉、邻域和修复算子。

多目标不是把单目标泛型参数替换为数组。它需要偏序比较、档案管理、不同的选择机制和一组最终解，因此作为独立设计阶段处理。

## 11. 初始脚手架交付范围

第一次实施只搭建可编译、可测试的仓库骨架：

- 创建解决方案和六个项目；
- 配置统一构建属性和集中包版本管理；
- 建立并验证全部项目引用方向；
- 启用 nullable、隐式 using、确定性构建和将警告视为错误；
- 添加一个证明项目引用可用的最小测试；
- 添加一个可运行但不承诺算法行为的 Examples 入口；
- 添加 CI 所需的标准 `dotnet restore`、`dotnet build` 和 `dotnet test` 命令说明；
- 不在脚手架任务中提前设计或实现 PSO、约束 API 或实验执行器。

此交付范围把项目边界先固定下来，后续每项行为通过独立规格、测试优先计划实现。
