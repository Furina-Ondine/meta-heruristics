# SPEC-0002：连续集中式算法迁移

## 元数据

- 编号：`SPEC-0002`
- 状态：`Draft`
- 创建日期：2026-08-27
- 批准人：—
- 批准日期：—
- 替代：无
- 被替代：无
- 相关 ADR：[ADR-0009](../../decisions/0009-group-scoped-optimizer-execution.md)、[ADR-0010](../../decisions/0010-scalar-evaluation-baseline.md)、[ADR-0011](../../decisions/0011-bat-first-algorithm-migration.md)、[ADR-0013](../../decisions/0013-tensor-shaped-repair-bounds.md)、[ADR-0014](../../decisions/0014-spec-driven-change-governance.md)、[ADR-0015](../../decisions/0015-ordered-extended-evaluation-values.md)

## 问题与动机

当前 `Metaheuristics.Algorithms` 仅提供连续蝙蝠算法。历史论文仓库 `task-schedule` 还保存了连续 PSO、萤火虫和布谷鸟搜索，但其 `master` 分支是研究原型：使用全局随机流、泛型 Fitness/Solution 和算法内位置边界；其初始评估、最佳状态与复用工作区还出现过已由 `fix` 分支修复的问题。直接复制这些类型会破坏当前的 RunGroup 隔离、Repair 职责和确定性契约。

需要将三种连续算法作为当前库的独立优化器实现，使用户能够在同一 `ContinuousProblem`、停止条件和 Experiment 运行模型中选择它们，同时保留已用于研究的核心更新变体。遗传算法使用二进制表示，不在本规格范围内；现有 Core 也没有二进制候选契约。

## 目标

- 在 `Metaheuristics.Algorithms` 提供连续 `PsoOptimizer`、`FireflyOptimizer` 和 `CuckooOptimizer`，每种类型都可直接作为 `IOptimizer` 交给现有 Runner 或 Experiment。
- 用强类型、复制后验证的 Options 公开算法专属参数；位置初始化和位置修复仍由调用方组合的策略负责。
- 以 `task-schedule` 的 `fix` 分支为行为参考，保留三个连续算法的研究变体，同时用当前 Core 的评估、比较、取消、随机性和状态生命周期替代旧框架。
- 为每种算法建立可观察的自动化契约，使后续变体调整可以与迁移正确性区分开来。

## 非目标

- 不迁移二进制遗传算法、二进制候选表示、泛型目标值、多目标优化或排列表示；这些能力必须由独立 Spec 和 ADR 推进。
- 不复制旧仓库的 `MetaheuristicBase`、`IMetaheuristic`、Fitness/Solution 模型、初始化器工厂、字符串注册表、实验管理、批量评估或 `VectorOps` 公共层。
- 不为三个算法建立共享的种群、工作区、数组池、抽象基类或 Core 扩展点。
- 不承诺与历史仓库逐随机数调用或逐结果复现；新库只承诺在相同版本、运行时、配置和 seed 下自身结果确定。
- 不以本规格引入 SIMD、并行种群评估、远程执行、GPU 后端或性能倍数承诺。

## 架构契合

三个优化器均位于 Algorithms 项目，只引用 Core，并实现既有 `IOptimizer`；不改变 Core、Experiments 或依赖方向。Options 是算法定义的不可变记录，Optimizer 是调用方或 RunGroup Factory 创建的有状态执行实例。每个实例独占其种群、最佳快照和临时数组，可在正常顺序 run 间复用物理工作区，但不得跨 Group 共享、并发驱动或在异常后重用。

算法不读取问题位置边界，也不声明位置上下界参数。初始化器写入位置后、每一次位置向量变更后，算法均通过 `OptimizationRunContext.Repair` 就地修复；每一次评价均通过 `OptimizationRunContext.Evaluate` 完成。算法不重新验证 Objective 或 Constraint 的返回值，使用 Core 的 `EvaluationComparer` 处理方向、可行性、违背量和有序扩展数值。

本次仅向既有 Algorithms 层增加具体实现，并未改变项目依赖、Core 执行模型、候选/评估语义或既定路线图顺序，因此不需要新增 ADR。ADR-0011 已明确 PSO、萤火虫和布谷鸟的后续迁移波次由未来用户决策确定；本规格即该决定的功能级落点。

## 信任与责任边界

| 数据或行为 | 责任方 | Core 是否验证 | 违反契约的结果 |
| --- | --- | --- | --- |
| 初始候选位置 | 调用方提供的 `ICandidateInitializer` | 否；算法随后请求 Repair | 由 Repair 与目标函数语义决定 |
| 候选位置边界与特殊位置值 | 调用方提供的 `ICandidateRepair` | 否；Context 委托 Repair | 由所选 Repair 的已定义行为决定 |
| 算法 Options | 调用方 | 否；具体 Optimizer 构造时验证 | `ArgumentOutOfRangeException` 或 `ArgumentException` |
| run 随机流、取消和评估计数 | `OptimizationRunContext` | 是 | Context 在取消时抛出；每个 run 使用独立 `Random(seed)` |
| 目标、约束和评价值数值域 | `ContinuousProblem` 与公开值对象 | 是 | 已定义的 `InvalidOperationException` 或参数异常 |
| 候选比较与最佳快照更新 | Optimizer | 通过 Core 比较器 | 违反属于算法实现缺陷，由契约测试发现 |
| 工作区独占与 Factory 组装 | 调用方和 Experiments | Experiment 按既有 RunGroup 规则创建 | 同一 Optimizer 并发使用不受支持 |

## 功能需求

### FR-001：三种连续优化器的公开入口

- 前置条件：调用方引用 Algorithms，并提供 `ICandidateInitializer` 与可选的对应 Options。
- 触发行为：构造 `PsoOptimizer`、`FireflyOptimizer` 或 `CuckooOptimizer`，然后将其交给 `OptimizationRunner` 或 `ExperimentCase` 的 Group Factory。
- 预期结果：每个公开 Optimizer 实现 `IOptimizer`，每种 Options 都是独立的强类型公开记录；Options 在构造时防御性复制，之后调用方对原记录的替换不影响实例。
- 边界情况：首次 `ResetForRun` 可按问题维度分配工作区；后续使用不同维度的 Problem 必须明确失败，不能静默重建或混用数组。
- 验收标准：API Reference、单次运行示例和 Experiment Factory 示例均能组装三种算法；不同维度复用、空初始化器和无效 Options 都有自动化测试。

### FR-002：共用运行生命周期与状态所有权

- 前置条件：一个 RunGroup 独占一个 Optimizer，Runner 为每次 run 创建 `OptimizationRunContext`。
- 触发行为：调用 `ResetForRun`，再调用零次或多次 `Advance`。
- 预期结果：Reset 完成全部逻辑重置、位置初始化、Repair、初始评估和全局最佳快照建立；`Advance` 完成一次原子算法迭代。`BestPosition` 始终是 Optimizer 自有、不会被后续种群或临时候选覆写的最佳位置；`BestEvaluation` 与之对应。
- 边界情况：初始种群即使停止条件在零次迭代时终止，也必须给出合法最佳值。发生非取消异常后，既有 Experiment 规则丢弃该实例；算法不提供异常后恢复保证。
- 验收标准：每种算法的零迭代运行、连续 run 复用、工作区标识复用、最佳快照稳定性、固定 seed 复现和 RunGroup 并发隔离均由测试覆盖；每种算法还必须在一个记录了配置与 seed 的 Sphere fixture 中产生严格优于其初始最佳值的最终最佳值。

### FR-003：PSO 研究变体

- 前置条件：`PsoOptimizerOptions` 指定 `PopulationSize`、速度分量闭区间、初始惯性、最小惯性、惯性衰减、认知系数和社会系数。
- 触发行为：初始化并执行一次 PSO 迭代。
- 预期结果：每个粒子保存位置、速度与个体历史最佳位置；初始速度在配置的速度区间中由本 run 随机流生成。每轮对每个粒子生成一对随机系数，按 `max(MinimumInertia, InitialInertia × InertiaDecay^iteration)` 更新速度，以当前全局最佳和个体最佳更新位置，Repair 后评估，并用 Core 比较器更新个体及全局最佳快照。
- 边界情况：速度边界只约束算法内部速度，不代表候选位置边界；位置修复可能改变更新后的位置但不改写速度。相同评估按 Core 比较器等价处理，不以引用或原始 `double` 小于号决定最优。
- 验收标准：速度边界、惯性下限、Repair 调用、最小化/最大化、可行性优先、历史最佳跨双缓冲保存和固定 seed 调用序列均有针对性测试。

### FR-004：萤火虫研究变体

- 前置条件：`FireflyOptimizerOptions` 指定 `PopulationSize`、基础吸引度、距离衰减、初始随机步长和随机步长衰减。
- 触发行为：初始化并执行一次萤火虫迭代。
- 预期结果：每轮从当前代的稳定快照生成下一代；每个萤火虫依次向当前代中严格更优的萤火虫移动。每次移动的吸引度按距离平方衰减，并加入以当前轮 `InitialRandomStep × RandomStepDecay^iteration` 缩放的随机扰动；每一次移动后即 Repair。完成全部位置更新后评估下一代，并只在严格更优时更新跨代最佳快照。
- 边界情况：没有严格更优对象的萤火虫保留当前位置；同等评价不触发相互移动。单次迭代的中间位置是该候选私有的连续状态，不修改当前代任何位置。
- 验收标准：严格比较、顺序多次移动、每次变更后的 Repair、随机步长衰减、跨代最佳单调性、方向/约束语义和固定 seed 复现均有测试。

### FR-005：布谷鸟研究变体与显式时间尺度

- 前置条件：`CuckooOptimizerOptions` 指定 `PopulationSize`、遗弃率、Lévy 指数、正态分布尺度、基础 Lévy 尺度、`LevyCandidateCount` 和 `StepDecayIterations`。
- 触发行为：初始化并执行一次布谷鸟迭代。
- 预期结果：每轮先对 `LevyCandidateCount` 个当前巢生成 Mantegna Lévy 候选，以当前最佳位置提供引导；候选经 Repair 和评估后仅在严格更优时替换对应巢。随后按 `floor(AbandonmentRate × PopulationSize)` 选择当前最差巢，以随机巢差和随机扰动生成替代候选，经 Repair、评估和择优替换。全局最佳始终为独立快照。
- 时间尺度：本轮的 Lévy 尺度从基础尺度按已完成迭代数，在 `StepDecayIterations` 内线性衰减至其 10%；超过该数后维持 10%。该时间尺度只由 Options 定义，不读取或推断 Runner 的停止条件。
- 边界情况：`LevyCandidateCount` 由调用方选择且不得超过种群数；遗弃率为零时不生成遗弃候选；种群为一时随机巢对允许指向同一巢，不得发生重试死循环。随机数、高斯样本和临时数组均为该 Optimizer 实例所有，不使用共享可变缓存。
- 验收标准：用户传递的 Lévy 候选数、衰减边界、最差巢选择、遗弃率零、单巢、不同 Lévy 指数、最佳快照、方向/约束语义和固定 seed 复现均有测试。

### FR-006：参数验证和取消语义

- 前置条件：调用方构造任一 Options 或在运行前/运行中请求取消。
- 触发行为：Optimizer 构造、Reset 或 Advance。
- 预期结果：种群数量必须为正；所有算法系数和内部速度边界必须为有限值；速度下界不得大于上界。PSO 的惯性和两个学习系数不得为负，最小惯性不得大于初始惯性，惯性衰减范围为 `(0, 1]`。萤火虫的吸引度、距离衰减和初始随机步长不得为负，随机步长衰减范围为 `(0, 1]`。布谷鸟的遗弃率范围为 `[0, 1]`，Lévy 指数范围为 `(0, 2)`，正态分布尺度和基础 Lévy 尺度必须为正，`LevyCandidateCount` 范围为 `[1, PopulationSize]`，`StepDecayIterations` 必须为正。
- 边界情况：零速度范围、零吸引度、零随机步长、零学习系数、零遗弃率和达到衰减下限均有效；`NaN`、Infinity、倒置范围和未定义的 Lévy 参数必须在构造时失败。
- 验收标准：每类 Options 的默认值可用；每个不等式边界与特殊浮点类别均有构造测试。算法通过 Context 的 Repair/Evaluate 传播取消，不自行吞没、转换或延迟 `OperationCanceledException`。

### FR-007：文档、示例与现状报告

- 前置条件：三种算法已实现并通过验收测试。
- 触发行为：维护面向用户和维护者的文档。
- 预期结果：XML 注释定义 Options、构造参数、最佳位置借用、状态所有权、线程安全、位置 Repair 和算法特有数值约束；API Overview、用户指南、README、Examples 与架构概览列出三种新增算法，并仍将成员级细节唯一指向生成式 API Reference。
- 边界情况：文档不复制整段工程契约，不宣称历史仓库逐位复现，也不把遗传算法表述为已支持。
- 验收标准：DocFX 在警告视为错误下生成成功；Examples 能以每个新增算法完成确定性单次运行，至少一个 Experiment Factory 展示可替换的算法组装。

## 非功能需求

### NFR-001：确定性、隔离和生命周期安全

- 测量方式：固定 seed 的重复运行、不同 RunGroup 并发/串行拆分以及同一 Optimizer 顺序复用的自动化测试。
- 可接受阈值：相同库版本、运行时、Problem、Options、seed 和执行设置产生相同结果；不同 Group 的随机序列和工作区互不影响；正常复用不保留上一次 run 的逻辑状态。
- 证据类型：自动化测试、实现审查。

### NFR-002：热路径不引入未经测量的通用抽象

- 测量方式：实现审查和现有/新增工作区复用测试；如 Plan 提出 SIMD、池化、批量评估或并行化，再补充端到端 BenchmarkDotNet。
- 可接受阈值：算法只保留自身所需的数组和临时缓冲区，不在 Core 增加布局或批量评估 API；正常迭代不为可避免的种群或最佳快照分配新数组；本规格不作吞吐量或内存数值承诺。
- 证据类型：实现审查、自动化测试；触发性能设计时使用 BenchmarkDotNet 或分配分析。

## 职责与替代关系

- 新增的概念：Algorithms 层的 `PsoOptimizer`/`PsoOptimizerOptions`、`FireflyOptimizer`/`FireflyOptimizerOptions`、`CuckooOptimizer`/`CuckooOptimizerOptions`，以及各自私有状态与工作区。
- 被替代的概念：历史仓库中面向上述算法的泛型 Fitness/Solution、`Random.Shared`、算法内位置边界、共享向量工具和全局/引用式最佳状态。
- 必须删除的旧行为或公共入口：新仓库不引入旧仓库同名基础类、配置、工厂、注册表或兼容转发层；三个 Options 不得包含候选位置上下界。
- 明确保留的旧概念及独立理由：PSO 的惯性与速度限制、萤火虫的顺序严格吸引变体、布谷鸟的 Lévy 与遗弃两阶段及其 10% 衰减下限，均是研究算法行为而非旧框架耦合。
- 完成后每个概念的唯一所属层：Core 拥有问题、Repair、评估和运行生命周期；Algorithms 拥有三种算法的配置、状态和更新；Experiments 仅拥有 Factory 组装、RunGroup 调度与结果聚合；Examples 与文档只教学和报告入口。

## 成功标准

- 用户能以与 Bat 相同的组装方式选择 PSO、萤火虫或布谷鸟，且无需暴露位置边界或使用全局随机流。
- 每种算法的初始评估、候选更新、最佳快照、工作区复用、方向/约束处理、取消与配置边界都有可追踪自动化证据。
- 代码、测试、示例、API 文档和架构概览不再把连续 Bat 描述为唯一内置算法；遗传算法仍明确不在支持范围内。
- 实现不新增 Core 抽象、跨项目依赖、字符串组装机制、共享可变随机/工作区或未经测量的性能承诺。

## 假设与已澄清决定

- 本次范围固定为连续 PSO、萤火虫和布谷鸟；不考虑遗传算法。
- 采用一个完整 SDD package；后续 Plan/Tasks 按 PSO、萤火虫、布谷鸟分阶段实施和验证。
- 旧仓库 `fix` 分支是算法行为与已知缺陷修复的参考，绝不作为可复制的架构或 API。
- 布谷鸟的步长衰减由 `StepDecayIterations` 显式配置；它不与 Runner 的停止条件耦合。
- 布谷鸟每轮产生的 Lévy 候选数由调用方传入 `LevyCandidateCount` 决定。
- 没有未解决的公共行为问题。

## 批准记录

- 规格批准：—
- 批准日期：—
- 批准时明确接受的风险：—
