# SPEC-0002 技术计划

## 元数据

- 状态：`Approved`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`5caccf473e2bd96e18209a0518a5c6476744ed60`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`FR-006`、`FR-007`、`NFR-001`、`NFR-002`
- 批准人：项目作者
- 批准日期：2026-08-27

## 当前实现调查

- 当前相关类型和入口：Algorithms 仅有 `BatOptimizer`、`BatOptimizerOptions` 和私有 `BatState`；所有具体算法通过 Core 的 `IOptimizer`、`OptimizationRunContext`、`EvaluationComparer`、`ICandidateInitializer` 与 `ContinuousProblem` 组合。Examples、API Overview、User Guide、README 和架构概览均以 Bat 作为唯一内置算法入口或现状。
- 当前调用链：调用方创建 Optimizer；`OptimizationRunner` 创建带独立 `Random(seed)` 的 Context；Optimizer 的 `ResetForRun` 初始化、Repair、评估并建立最佳快照；Runner 随后驱动 `Advance`。Experiment 只通过 Group Factory 创建 `IOptimizer`，不认识具体算法。
- 当前职责所属层：Core 正确拥有连续问题、Repair、评估、比较、取消和运行生命周期；Algorithms 正确拥有 Bat 的 Options、工作区和状态；Experiments 正确只依赖 Core。新增算法不需要移动这些职责。
- 已存在的相似或重复概念：Bat 的私有状态类、双缓冲与独立最佳位置是对其自身布局的必要表达，不能抽成通用“种群算法”层。旧仓库的 `MetaheuristicBase`、泛型 Fitness/Solution、`VectorOps`、注册表和批量实验入口与当前边界重复且必须不迁入。
- 当前测试、示例、文档和基准：`BatOptimizerTests` 已证明固定 seed、方向/约束、工作区复用、Repair、最佳快照、参数验证和 Sphere 改善；Examples 只演示 Bat；Benchmark 只测 Bat 工作区复用及 Experiment 调度；Algorithms 项目已生成 XML 文档。
- 历史参考调查：`D:\task-schedule` 的 `master` 工作树含一个与本变更无关的已修改 `experimentManager/BatchRunner.cs`，不得触碰。`fix` 分支的 `d79b1fdd4b4653a09181b52c78cffe7af323f1c8` 是参考基线；它修复了三种算法的初始评估、最佳快照和布谷鸟候选/遗弃所有权问题，但仍保留 `Random.Shared`、算法内位置边界、泛型评估模型及部分为旧边界设计的缓存。
- 与 Spec 或 ADR 的已知冲突：当前实现尚未提供三种公开算法，现有文档把 Bat 描述为唯一内置算法；这与 Approved Spec 冲突。没有现有 ADR 冲突：ADR-0011 已允许后续波次由用户决定。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A. 直接搬运旧 `fix` 分支类型，并将最小接口改为 `IOptimizer` | 初期代码量少，保留更多旧细节。 | 会保留位置边界、`Random.Shared`、泛型 Fitness/Solution、共享工具和引用式状态，随后仍要逐项拆除。 | 高；违反 Repair、确定性和单一职责边界。 | 不采用。 |
| B. 为每种算法建立独立 Options、私有状态和直接 `IOptimizer` 实现；以 `fix` 的更新关系与已知修复为参考 | 与 Bat 和当前生命周期一致；工作区、随机性、比较和 Repair 的责任清晰；每种变体可独立测试。 | 需要重写数据布局和测试夹具，历史结果不逐调用复现。 | 低；没有新增跨项目抽象。 | 采用。 |
| C. 先建立公共 Population/VectorOps/OptimizerBase 框架，再在其上实现三种算法 | 可能减少表面重复。 | 必须为尚未稳定的 AoS、双缓冲、候选缓冲、Lévy 采样和最佳状态预设共同布局。 | 高；Core 或 Algorithms 会出现未经实际验证的抽象，且削弱算法专属所有权。 | 不采用。 |
| D. 让 `CuckooOptimizer` 从 Runner 停止条件读取总迭代数，重用旧 `MaxIteration` 语义 | 用户少传一个参数。 | 使算法行为依赖可组合且可能非迭代的停止条件。 | 高；违反 Options 自包含与层次边界。 | 不采用。 |

方案 B 的实现不复制旧仓库的向量缓存。每个 Optimizer 在自身私有循环中完成所需基础算术；只有经后续端到端基准证明后，才可评估专用 SIMD 或共享帮助类。

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| 连续运行、Repair、评估、比较、取消与随机流 | Core | Core，不变 | 三种算法应复用已验证的稳定协议。 |
| PSO Options、粒子位置/速度/个体最佳和全局最佳快照 | 不存在 | Algorithms / `PsoOptimizer` 私有状态 | PSO 专属布局与更新。 |
| 萤火虫 Options、当前/下一代与全局最佳快照 | 不存在 | Algorithms / `FireflyOptimizer` 私有状态 | 当前代稳定读取和顺序移动是该变体专属。 |
| 布谷鸟 Options、种群、候选缓冲、正态采样状态与全局最佳快照 | 不存在 | Algorithms / `CuckooOptimizer` 私有状态 | Lévy、遗弃和随机尺度不应污染 Core 或其他算法。 |
| 用户选择算法 | Bat 单一入口 | Examples、API Overview 与用户指南列出四种可选 Optimizer | 文档仅提供入口地图和任务教学。 |
| 历史框架与共享工具 | 仅历史仓库存在 | 不迁入 | 没有新仓库消费者，且与当前职责重复。 |

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| 各算法 Options 的标量范围和相互关系 | Optimizer 构造器 | 每个实例一次 | 否 | 用 `ArgumentOutOfRangeException` 或 `ArgumentException` 拒绝 Spec 禁止的数量、范围、`NaN`/Infinity 和关系。 |
| Initializer 依赖 | Optimizer 构造器 | 每个实例一次 | 否 | `ArgumentNullException`；不验证初始化器生成的位置。 |
| Problem 维度与工作区 | 首次 `ResetForRun` 及后续 Reset | 每次 Reset 的常数检查 | 否 | 首次分配；不同维度抛 `InvalidOperationException`，不重新分配或混用状态。 |
| 初始/更新的位置 | `context.Repair` | 每个初始化与每次位置向量变更后 | 是 | 位置范围、特殊值和随机修复归调用方策略；算法不读取边界。 |
| 目标和约束结果、取消、评估计数 | `context.Evaluate` | 每次候选评估 | 是 | 复用 Core 异常、计数和取消语义；算法不重复检查数值。 |
| 候选比较 | `EvaluationComparer` | 接受、个体最佳和全局最佳更新时 | 是 | 使用可行性优先、方向和 Infinity 语义；等价不视为改进。 |
| 高斯采样与临时缓冲 | `CuckooOptimizer` 私有工作区 | 每个 run 完整重置 | 是 | 仅使用 Context.Random；不使用静态可变缓存或全局随机流。 |

## API 与行为变化

- 新增：
  - `Anastasya.Metaheuristics.Algorithms.Pso.PsoOptimizer` 与 `PsoOptimizerOptions`。
  - `Anastasya.Metaheuristics.Algorithms.Firefly.FireflyOptimizer` 与 `FireflyOptimizerOptions`。
  - `Anastasya.Metaheuristics.Algorithms.Cuckoo.CuckooOptimizer` 与 `CuckooOptimizerOptions`。
  - 三个 Optimizer 的构造形状统一为 `Optimizer(ICandidateInitializer initializer, OptimizerOptions? options = null)`；私有状态类型不进入公共 API。
- 选项默认值：PSO 为 Population `100`、速度 `[-1, 1]`、初始/最小惯性 `0.79`/`0.4`、惯性衰减 `0.975`、两个系数 `2.44`；萤火虫为 Population `100`、基础吸引度 `0.5`、距离衰减 `12`、初始随机步长 `0.2`、随机步长衰减 `0.97`；布谷鸟为 Population `100`、遗弃率 `0.25`、Lévy 指数 `1.5`、正态尺度 `1`、基础 Lévy 尺度 `10`、遗弃扰动尺度 `0.5`、Lévy 候选数 `2`、衰减迭代数 `100`。
- 修改：README、API Overview、User Guide、Examples 和架构概览从“Bat 是唯一内置算法”改为列出 Bat、PSO、萤火虫和布谷鸟；Tests 新增各算法的独立行为文件。
- 删除：无新仓库公共 API 删除；不添加历史 API 的兼容入口。
- 破坏性变化：无。新增 Algorithms 公共类型是向后兼容功能；实际发布时三个运行时包按既定锁步规则进行对应 MINOR 发布准备。
- 调用方迁移方式：现有 Bat 调用不变；新调用方选择一个 Optimizer 和对应 Options，并继续显式提供 Initializer、Problem Repair、停止条件及 seed。
- 明确保持不变的行为：Core API、Experiment 调度、Batch/RunGroup 工厂形状、Repair 所有权、随机种子来源、标量评估和 Bat 数值行为均不改变。

## 算法实现设计

### PSO

- 使用两组私有粒子状态，分别保存 Position、Velocity、PersonalBestPosition、当前 Evaluation 与 PersonalBestEvaluation；另用独立 `double[]` 和 `Evaluation` 保存全局最佳。
- Reset 重新填充当前位置、立即 Repair、生成受速度区间限制的速度、评估并初始化个人/全局最佳；两代数组和最佳数组在同维度 run 间复用。
- 每轮从当前代读取，以一对每粒子随机系数和当前全局最佳生成下一代速度/位置；位置 Repair 后评估；按 `EvaluationComparer` 更新个人与独立全局最佳。当前代绝不被候选生成写入。

### 萤火虫

- 使用两组私有萤火虫状态，各自保存 Position 和 Evaluation；另用独立 `double[]` 和 `Evaluation` 保存跨代最佳。
- 每轮先把当前萤火虫的位置复制到下一代；只枚举当前代的严格更优萤火虫。目标位置按已累积的目标位置计算距离并依次移动，每次移动后立即 Repair；然后统一评估下一代。
- 只在下一代中存在严格更优评价时更新独立全局最佳。该策略保留研究变体的顺序吸引，同时避免当前代被写入或最佳引用被覆盖。

### 布谷鸟

- 使用一组私有巢状态和一组等长私有候选缓冲；每个状态保存 Position 与 Evaluation。另用独立 `double[]` 和 `Evaluation` 保存全局最佳；Box–Muller 的备用正态样本（若使用）也只属于该实例并在 Reset 清除。
- 构造时用私有、稳定的 log-Gamma/Lanczos 计算 Mantegna 所需常量；不采用旧仓库只对指数 `1.5` 正确的静态幂缓存。采样与幂运算使用每个 run 的 Context.Random。
- 每轮依序处理前 `LevyCandidateCount` 个巢：用 `BaseLevyScale × decayFactor` 生成候选，Repair、评估、严格择优替换；然后从已更新种群选出 `floor(AbandonmentRate × PopulationSize)` 个最差巢，用两个随机巢的差和 `AbandonmentPerturbationScale` 生成候选，Repair、评估、严格择优替换。
- `decayFactor` 为 `1 - 0.9 × min(iteration / StepDecayIterations, 1)`。一巢情形允许两个随机索引相等，避免重试循环。最佳快照在每个成功替换后独立复制。

## 替代与清理计划

- 被取代的类型、接口和入口：无新仓库类型被取代；旧仓库相关类型只作为外部历史参考，不参与删除。
- 必须删除的转发层、兼容壳和重复抽象：不得创建 `MetaheuristicBase`、共享 `VectorOps`、旧 Config 名称、泛型 Fitness/Solution 适配器、字符串工厂或从 Repair/Problem 提取位置边界的帮助层。
- 必须删除或改写的旧测试：无；新增测试不能复制 Bat 私有实现细节作为唯一证据，必须断言 Spec 中的可观察行为。
- 必须更新的示例和文档：更新 README、API Overview、User Guide、架构概览与 `examples/Metaheuristics.Examples/Program.cs`；更新 XML 注释并用 DocFX 生成验证。Benchmarks 保持 Bat 的既有测量范围，不宣称三种新增算法的性能。
- 全仓库残留搜索方式：`rg -n "唯一内置|当前内置实现是|仅提供连续蝙蝠|Bat 是唯一|PositionLowerBound|PositionUpperBound|Random.Shared|MetaheuristicBase|IMetaheuristic|VectorOps" src tests examples docs README.md ENGINEERING.md`；逐项确认旧边界/全局随机表述未进入新增算法且 Bat 专属说明未被误删。
- 保留旧结构时的真实消费者、期限和删除条件：现有 Bat 状态、测试和基准仍服务 Bat；它们不因新增算法失去价值。无计划保留历史仓库代码、兼容层或转发类型。

## 连带影响矩阵

| 区域 | 是否受影响 | 具体影响或无影响理由 | 验证证据 |
| --- | --- | --- | --- |
| Core | 否 | 复用现有生命周期、Repair、标量评估、比较和特殊值契约。 | Release build、现有 Core 测试仍通过。 |
| Algorithms | 是 | 新增三组公开 Options/Optimizer 与私有工作区。 | 算法单元/集成测试、XML 编译。 |
| Experiments | 否（集成受益） | Factory 已只依赖 `IOptimizer`，无需认识新类型。 | 新算法至少一个 Group Factory 端到端测试；既有 Experiment 测试。 |
| Examples | 是 | 展示三种新增 Optimizer 的单次运行与可替换实验 Factory。 | Example build 与确定性运行烟雾检查。 |
| Tests | 是 | 新增每种算法的配置、生命周期、方向/约束、确定性与 Sphere 测试。 | `dotnet test -c Release`。 |
| Benchmarks | 否 | Spec 没有性能主张，不新增未经设计的基准。 | Verification 审查；如实现引入性能路径则退回 Plan。 |
| XML 文档 | 是 | 新公共类型必须说明所有权、边界、随机性和数值参数。 | DocFX warnings-as-errors。 |
| 用户/API 文档 | 是 | 从唯一 Bat 入口扩展为四种算法地图，不复制成员细节。 | 文档审查与 DocFX。 |
| ENGINEERING | 否 | 新实现遵守既有约束，不改变持续工程契约。 | 设计/代码审查。 |
| ADR | 否 | 不改变项目依赖、执行模型、责任、数值域或路线图决策。 | ADR-0011 与 Spec 链接审查。 |

## 需求—验证设计

| 需求 | 自动化测试或基准 | 测试层级 | 预期证据 |
| --- | --- | --- | --- |
| FR-001 | 各 Options 默认构造、三种 Optimizer 的 Runner/Experiment Factory 组装、空初始化器与不同维度复用 | Algorithms 单元 + Experiment 集成 | 公开入口可组装；构造/维度异常一致。 |
| FR-002 | 零迭代、Reset 后种群/最佳完全重置、固定 seed 重复、RunGroup 并发隔离、最佳位置在后续迭代后不变、工作区引用复用 | Algorithms 集成 | 初始最佳有效，借用位置与评价一致，正常复用无泄漏。 |
| FR-003 | PSO 速度范围、惯性下限、个人/全局最佳、Repair、最小化/最大化与约束 fixture | Algorithms 单元/集成 | 更新关系仅写候选代，比较完全委托 Core。 |
| FR-004 | 萤火虫严格优于判定、顺序多次移动、每次移动 Repair、随机步长衰减、跨代最佳 | Algorithms 单元/集成 | 同值不移动；当前代不变；最佳单调。 |
| FR-005 | 用户传递的 Lévy 候选数、衰减起点/下限、遗弃最差巢、零遗弃率、单巢、不同指数、两个显式尺度与最佳快照 | Algorithms 单元/集成 | 无读取边界、无重试死循环；Mantegna 参数和替换语义符合 Spec。 |
| FR-006 | 每种 Options 的每个不等式端点、`NaN`/Infinity、取消于 Reset/Advance 中传播 | Algorithms 单元 | 仅构造验证；Context 异常不被吞没。 |
| FR-007 | Example 编译/运行、DocFX、文档残留搜索 | 文档/示例集成 | 入口文档准确且 API Reference 无警告。 |
| NFR-001 | 三种固定 seed、并发拆分与连续复用测试 | Algorithms + Experiment 集成 | 结果及最佳位置可复现且 Group 隔离。 |
| NFR-002 | 工作区复用检查与实现审查 | Algorithms 单元 + 审查 | 主要状态只在首个维度分配；无公共批量/池化/并行路径。 |

每种算法的 Sphere fixture 使用明确记录的 Options、Repair、停止迭代数和 seed；最终最佳必须严格优于同一配置的零迭代初始最佳。此类 fixture 在实施前先以测试表达，若任一算法无法稳定满足，停止并返回 Spec/Plan，而非偷偷调低断言。

## 风险和回退

- 最大实现风险：将旧算法内位置范围缩放错误地带入 Cuckoo，或把双缓冲/候选缓冲的引用泄漏为全局最佳；其次是 Mantegna 参数计算在接近合法边界时数值不稳定。
- 可能产生的行为漂移：旧 `Random.Shared` 与当前每 run `Random(seed)` 的随机调用序列不同；去除旧边界 Clamp 后，自定义 Repair 下的 Cuckoo 尺度完全由新增 Options 决定。这些是 Approved Spec 明确接受的迁移差异。
- 如何及早发现：以初始评估、Repair 次数、最佳快照、固定 seed、单巢/零遗弃、方向/约束和尺度边界测试先行；审查每个算法只在 Context 上调用随机、Repair 和 Evaluate；以残留搜索阻止旧框架回流。
- 退回方式：在尚未发布的实现提交中删除新增 Algorithms/Tests/Examples/文档改动；不留下空壳或兼容开关。若已批准的算法公式、默认值、比较、Repair 调用时机、尺度或公开 Options 需要改变，先将 Spec 或 Plan 退回 Clarifying。
- 退回 Spec 澄清的条件：需要新增通用种群/批量 API、需要让算法读取 Repair 边界、Cuckoo 默认尺度不足以表达已批准规则、Mantegna 分布需要扩大/收紧指数域，或 Sphere fixture 无法在不改变已批准变体的情况下稳定通过。

## ADR 判断

- 是否触发 ADR：否。
- 判断依据：本变更只在 Algorithms 层新增具体、可组合的连续实现，复用已 Accepted 的 Core 生命周期、Repair、评估、比较、确定性和依赖规则；不更改任何持续架构决策。
- 新 ADR 或被替代 ADR：无。ADR-0011 继续有效，并由本 Plan 落实其允许的后续迁移波次。

## 批准记录

- 计划批准：项目作者
- 批准日期：2026-08-27
