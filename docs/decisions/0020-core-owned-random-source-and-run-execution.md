# ADR-0020: Core 拥有的封闭随机源与 RunGroup 执行

## 状态

Accepted

替代 [ADR-0009](0009-group-scoped-optimizer-execution.md)。本 ADR 保留其 RunGroup、Optimizer、Context 和异常后复用决定，并以 [SPEC-0009](../specs/SPEC-0009-high-performance-random-sampling/spec.md) 批准的封闭 `RandomSource` 与 `ulong` seed 替代 `System.Random`/`int` seed。

## 背景

ADR-0009 建立了 Config、Factory、RunGroup 独占 `IOptimizer` 和每 run `OptimizationRunContext` 的执行模型，并由 Context 从 `int` seed 创建 `System.Random`。当前 Bat 与 Cuckoo 的显著热路径消耗在逐次随机采样；`System.Random` 不提供本库所需的强类型批量均匀/分布 API，也不能让后续算法先填充随机工作区、再处理规则 SIMD 算术。

随机源的主要消费虽发生在 Algorithms，但其创建、每 run 所有权、Initializer/Repair 传递和 seed 记录属于 Core 执行契约。Algorithms 与 Experiments 都已依赖 Core，且没有不应依赖 Core 的真实 Randomness 消费者。为一个 PRNG 和一个标准正态组件新增程序集只会增加必选依赖，不产生独立消费或发布边界。

## 决策

- `IOptimizationSession` 与独立 `IOptimizationWorker` 继续不存在。强类型 Config 保存可复用算法定义；用户 Factory 为每个 RunGroup 创建独占 `IOptimizer`。
- `IOptimizer` 继续是有状态、RunGroup 独占且不可并发使用的算法实例，直接拥有种群数组和临时工作区，并公开 `BestPosition`、`BestEvaluation`、`ResetForRun` 和 `Advance`。
- `ResetForRun` 可在首次 run 分配维度相关工作区；后续正常顺序 run 复用主要数组，但必须重置全部逻辑状态、重新初始化、Repair 并完成初始评估。
- `IOptimizer` 不继承 `IDisposable`。Runner 不释放调用方拥有的 Optimizer；若具体实现拥有必须确定释放的资源，由具体类型自行表达。
- `OptimizationRunContext` 继续每 run 创建，统一提供 Problem、seed、随机源、取消、评估计数和 Repair；Context 不得跨 run 复用或缓存。
- Randomness 作为 `Metaheuristics.Core` 内部边界清晰的子系统，不新增 `csproj`、运行时程序集或 NuGet 包。现有 Algorithms 和 Experiments 到 Core 的项目依赖保持不变。
- Core 公开 sealed `RandomSource`，但其构造函数为 internal。用户可在自定义 Optimizer、Initializer 和 Repair 中消费 Context 提供的实例，但不能创建、继承、实现或替换随机源。
- `RandomSource` 内部唯一 PRNG 为 `xoshiro256++` 1.0；四个 64 位状态字由 SplitMix64 从一个 `ulong` seed 展开。不预建 `IRandomSource`、第二 PRNG、engine 接口、factory、适配器、注册或实现切换层。
- Runner、Context、run summary、Experiment options/plan/result 和 Group context 统一使用 `ulong` seed，不保留 `int` 重载或兼容包装。`OptimizationRunOptions` 不保存 seed 或随机 factory；Runner 每次运行显式接收 seed，并构造对应 Context 和 `RandomSource`。
- Experiment 只拥有 seed 排程，不了解 PRNG 或内部播种。未提供显式 seed 列表时，第 `i` 个 repetition 使用 `unchecked(BaseSeed + (ulong)i)`；同一 repetition 在不同 Case 和 Group 拆分下继续共享同一 seed。
- 每个 run 独占一个 `RandomSource`；它不保证线程安全，不得并发或跨 run/Group 共享，不得使用全局随机流或隐式时间播种。
- run 抛出非取消异常后，当前 Optimizer 继续视为状态可能损坏，Experiment 丢弃它并通过 Group Factory 为剩余 repetition 重建实例。
- 每个 RunGroup 使用独立 Problem 和 Optimizer；不同 Group 只能共享调用方提供的不可变底层数据。Case、RunGroup、有界调度、取消、二维结果矩阵和聚合语义保持不变。

## 替代方案

- 新增 `Metaheuristics.Random` 程序集和包：不采用。当前没有脱离 Core 的真实消费者、可选重量依赖或独立发布节奏，拆分只会增加必选程序集。
- 公开可外部实现的 `IRandomSource`：不采用。本库不承诺用户随机后端，接口会引入热路径分派、factory 生命周期和第三方契约。
- 公开 abstract 随机源基类或封闭层次：不采用。当前只有一个实现，不应为未证实扩展点支付虚调用和维护成本。
- 将 Randomness 放入 Algorithms：不采用。Core 的 Context、Initializer 和 Repair 已拥有随机契约，该方向会产生 Core/Algorithms 循环依赖或重复随机概念。
- 保留 `System.Random` 并只在 Algorithms 增加批量 helper：不采用。它无法建立统一的强类型批量契约，也会使分布实现复制到算法。
- 复用 `Optimizer -> Worker` 或跨 run Context：不采用。现有 Config/Factory/RunGroup 已表达状态所有权，而复用 Context 会扩大随机流污染风险。

## 后果

Core 成为随机源契约、内置实现和每 run 状态的唯一所有者；Algorithms 只拥有抽样时机与算法专用公式；Experiments 只拥有 seed 计划。公共 API 从 `System.Random`/`int` seed 一次性破坏性迁移到封闭 `RandomSource`/`ulong` seed，无兼容壳或可插拔随机后端。

封闭具体类型使内置标量和 Fill 路径不依赖 JIT 接口去虚化，但同时意味着未来若要用户选择 PRNG，必须重新设计公共构造、工厂、状态和确定性契约。本项目不保证迁移前后的固定 seed 轨迹，也不保证跨包版本、平台或 Runtime 保持序列。

封闭构造也带来外部测试代价：引用 Core 的用户测试程序集不能仅凭 seed 创建 `RandomSource`，因此不能脱离运行生命周期直接对自定义 `ICandidateInitializer` 或 `ICandidateRepair` 做确定性单元测试。外部验证路径是通过公共 `OptimizationRunner.Execute` 传入包含这些策略的自定义 `IOptimizer`，在 `ResetForRun`/`Advance` 回调中消费 Context 提供的随机源，并以位置、调用记录和运行结果断言行为；仓库测试与基准的 `InternalsVisibleTo` 访问不构成公共测试契约。

## 重新评估条件

出现以下任一真实需求时，通过新 Spec 和 ADR 重新评估：

- 有不应依赖 Core 的消费者需要独立 Randomness 程序集或发布包；
- GPU、并行随机访问或子流要求 Philox 等计数器模型、Jump 或新的状态所有权；
- 用户需要选择、创建或注入随机源；
- Randomness 需要不同目标框架、重量依赖、独立版本或已测量的构建/部署隔离；
- 通用运行时资源开始需要确定释放，或 run 级 Context 分配经基准证明需要复用。
