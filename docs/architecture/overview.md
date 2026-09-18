# 当前系统与架构概览

本文是理解当前 Metaheuristics.NET 的入口，报告已实现能力和现有边界。持续规则以 [ENGINEERING](../../ENGINEERING.md) 为准，成员级契约以源码 XML/API Reference 为准；功能行为见[当前规格索引](../specs/README.md)，选择原因见[当前 ADR 索引](../decisions/README.md)。本文不覆盖这些权威来源，也不把已批准但未实现的设计描述为现状。

## 产品定位与范围

面向 .NET 调用方的连续单目标元启发式优化库：输入是 `double` 位置向量，目标值是标量 `double`，支持最小化、最大化和约束。调用方提供业务评价、初始化、Repair 和停止条件，库负责运行生命周期与重复实验。内置优化器及入口见 [API Overview](../api/overview.md)。

当前支持进程内 CPU 执行、单次优化、多 Case 重复实验、显式 seed 和有界并发。API 仍处早期阶段；当前不支持多目标、二进制/排列表示、远程/集群/GPU 执行。论文业务模型和领域解码留在调用方，不进入运行时包。入门与示例见[用户手册](../guides/user-guide.md)。

## 设计取向

- 强类型、显式组装与组件组合：调用方选择策略，库不扫描程序集或解释字符串注册表。
- 最小 Core：协议、比较与运行生命周期归 Core，算法公式和种群布局归 Optimizer，实验编排归 Experiments。
- 验证跟随责任边界：构造配置尽量一次验证，内部代码依赖已建立的前置条件；性能不是删除必要协议或数值检查的理由。
- 状态所有权明确：每个 run 独立随机状态，每个 RunGroup 独占 Optimizer；顺序 run 复用存储但重置逻辑状态。
- 性能结论依赖实测：保留可验证的基础路径，不将特定硬件或能力路径的收益外推到未测平台。

以上是 [ENGINEERING](../../ENGINEERING.md) 的导航摘要；修改取舍时先按其风险分类和批准流程处理。

## 项目与职责

| 项目 | 当前职责与能力 |
| --- | --- |
| Core | 连续 Problem、策略协议、评估与比较、运行 Context、封闭随机源、停止、轨迹、汇总及单次 Runner；只提供单点评估契约 |
| Algorithms | 内置连续优化器、强类型参数、Optimizer 私有种群/缓冲及私有 SIMD 算术 |
| Experiments | typed Case、RunGroup 计划、seed 排程、有界 Worker、部分失败/取消、结果存储与统计 |
| Examples | 单次运行、内置算法及可替换 Optimizer 的实验组装示例 |
| Tests | Core、算法、Experiment 的契约、数值、隔离与生命周期测试 |
| Benchmarks | 局部内核、随机采样、完整 run、工作区复用及 RunGroup 调度对照 |

```text
Algorithms   ──→ Core
Experiments  ──→ Core
Examples     ──→ Core + Algorithms + Experiments
Tests        ──→ Core + Algorithms + Experiments
Benchmarks   ──→ Core + Algorithms
```

Core 不引用其他仓库项目，Algorithms 与 Experiments 彼此不依赖。策略扩展方式见[开发者架构手册](developer-guide.md)，公共类型入口见 [API Overview](../api/overview.md)。

## 责任与信任边界

| 数据或操作 | 责任方与当前处理 | 下游可以依赖什么 |
| --- | --- | --- |
| 维度、算法 Options、Repair 端点 | 各自构造入口验证；向量边界由 Repair 持有防御性副本 | 已建立的配置不在逐元素热路径重复验证 |
| 候选位置 | Initializer 初始化，Optimizer 在初始化和每次修改后调用 Repair | Core 不扫描位置是否合法，也不替策略静默修复 |
| 目标与约束结果 | 算法经 Context.Evaluate 进入 Problem 的评估边界，由 Problem 验证结果数值域 | 比较器不重复验证目标 NaN 或负约束违背量 |
| 计数、取消与停止 | Context 管理评估计数及取消观察；Runner 在初始化和完整迭代之间检查停止 | 评估预算可在一次迭代内被跨过，不是逐评估硬上限 |
| 私有算术缓冲 | Optimizer 建立长度、别名与有效区间前置条件，读取前写满有效区间 | 内核不增加逐块重复防御验证；前置条件由调用链与测试保证 |
| 共享数据与策略 | Group 拥有独立 Problem/Optimizer；跨 Group 共享底层数据须显式且不可变 | 不能从“接口是只读的”推断其实现没有可变状态 |

位置责任与数值规则分别见 [ENGINEERING](../../ENGINEERING.md)、[SPEC-0007](../specs/SPEC-0007-repair-boundary-shape-specialization/spec.md) 和 [SPEC-0001](../specs/SPEC-0001-evaluation-special-values/spec.md)。评估入口见 [ContinuousProblem](../../src/Metaheuristics.Core/Problems/ContinuousProblem.cs)，运行接口见 [OptimizationContracts](../../src/Metaheuristics.Core/Execution/OptimizationContracts.cs)。

## 数据与控制流

### 单次运行

```text
调用方组装 Problem + Optimizer + RunOptions，提供 ulong seed
  → OptimizationRunner.Execute
  → 新建 Context / RandomSource
  → ResetForRun：初始化位置 → Repair → Evaluate → 建立最佳状态
  → 检查停止条件
  → Advance：算法更新 → Repair → Evaluate → 选择/最佳状态更新
  → 在完整迭代边界再次检查停止，直到返回 Summary
```

这是公共阶段示意，候选选择及 best 更新的精确顺序仍由各算法规定。Optimizer 不从 Repair 提取变量边界；Cuckoo 的尺度由显式 Options 表达。单次 Runner 返回的 Summary 不复制最佳位置；调用方要保留位置快照时，应在下一次 ResetForRun 前复制 Optimizer.BestPosition。实现入口见 [OptimizationRunner](../../src/Metaheuristics.Core/Execution/OptimizationRunner.cs)。

### 重复实验

```text
Cases + repetition/seed 计划
  → 稳定的 RunGroup plans
  → 有界长期 Worker 领取计划
  → Group factory 创建独占 Problem / Optimizer / RunOptions
  → Group 内顺序运行，保存每次 Summary 与最佳位置副本
  → 按 Case/repetition 位置组织结果并统计
```

RunGroupCount 控制每个 Case 的拆分，所有 Group 受统一并发上限约束。seed 与结果定位不依赖 Worker 完成顺序。一次非取消运行异常会记录失败并为剩余 repetition 重建 Group 环境；取消返回已完成的部分实验结果。单次 Runner 的取消则抛出异常，不能混同两种入口的结果语义。实现入口见 [ExperimentRunner](../../src/Metaheuristics.Experiments/Execution/ExperimentRunner.cs)，调度证据见 [RunGroup 基准](../benchmarks/run-group-scheduling.md)。

## 所有权与生命周期

| 对象或数据 | 生命周期与所有者 | 复用、共享和失效边界 |
| --- | --- | --- |
| 强类型配置与不可变底层数据 | 调用方 | 可显式共享；不携带 run 级可变状态 |
| Problem / Optimizer / RunOptions | 单次调用方，或 Experiment 的 Group setup | Group 间独占；正常 run 顺序复用；异常后不继续复用旧 Optimizer |
| Context / RandomSource | Runner 为每个 run 新建 | 不带出 run、不跨 Group 共享，不是可替换随机后端 |
| 种群和样本缓冲 | Optimizer | 跨正常顺序 run 复用物理存储，Reset 完整重置逻辑状态；只读本次已写入区间 |
| Stopping Condition | 调用方配置 | 可重入，不存 run 级状态，允许并发调用 |
| BestPosition | 借用 Optimizer 工作区 | 非独立快照；需要持久化时复制，Experiment 在后续 run 前保存副本 |
| Summary / 实验结果 | 返回给调用方 | Summary 保存标量结果和轨迹；实验结果另存各 run 的位置与执行状态 |

IOptimizer 不保证线程安全，也不要求 IDisposable。具体借用、异常和成员生命周期以 [OptimizationContracts](../../src/Metaheuristics.Core/Execution/OptimizationContracts.cs) 的 XML 与生成式 API Reference 为准。

## 数值、随机与 SIMD 现状

目标值允许正负 Infinity、拒绝 NaN；约束违背量允许非负有限值及正 Infinity。统计中的未定义结果通过 nullable 表达，不以零或 NaN 代替。默认 Repair 为 Clamp `[0,10]`；同形状标量或逐维向量端点在创建时分派，Reflect 当前使用标量循环。特殊值与比较规则见 [SPEC-0001](../specs/SPEC-0001-evaluation-special-values/spec.md) 和 [SPEC-0007](../specs/SPEC-0007-repair-boundary-shape-specialization/spec.md)。

RandomSource 由 Core 所有：单值与批量入口分别推进独立状态；批量宽度由 Vector<T>.Count 决定，不保证跨宽度批量序列一致。相同版本、运行时与执行设置下复现；Group 拆分和调度不改变 seed 排程。当前行为见 [SPEC-0010](../specs/SPEC-0010-simd-random-sampling/spec.md)，决策及旋转指令的局部例外见 [ADR-0024](../decisions/0024-adaptive-vector-random-api.md)。

Algorithms 使用私有 512/256/128 完整块级联与标量尾部，源码生成器只服务 Algorithms，不存在公共 SIMD 后端。Bat 掩码候选更新、Cuckoo Lévy/遗弃更新、PSO 双输出融合、Firefly 移动融合均已接入。当前入口和证据见 [SIMD 联合规格](../specs/simd-review.md) 与 [ADR-0025](../decisions/0025-direct-tensor-primitives-and-benchmark-execution.md)。

## 已知限制与证据范围

- 当前随机源报告有本机 128/256/512 位矩阵；[SPEC-0011 至 SPEC-0014 的算法报告](../specs/simd-review.md)有普通硬件环境的完整 run 和受限 Vector128 局部测量。该证据只覆盖报告中的算法与版本；位宽矩阵不能自动解释为仅 AVX2 或 ARM64 实测。
- 这些算法报告尚未提供仅 AVX2 和 ARM64 的完整性能覆盖；部分历史报告缺少稳定的被测源码身份或原始报告。详见各 Verification 的证据限制，不据此宣称所有平台性能通过。
- 固定迭代完整 run 数据衡量执行吞吐，不表示达到给定精度的求解时间。源码或运行时变更后，旧性能证据不能自动代表新版本。
- 路线图、历史方案与已实现能力分开阅读。继续任务时先查[当前规格索引](../specs/README.md)的适用范围；需要了解取舍时再查相关 ADR，无需通读所有历史记录。
