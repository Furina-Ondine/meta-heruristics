# SPEC-0005：算法私有 SIMD 演进

## 元数据

- 编号：`SPEC-0005`
- 状态：`Implemented`
- 创建日期：2026-08-29
- 批准人：项目作者
- 批准日期：2026-08-29
- 替代：[SPEC-0003](../SPEC-0003-simd-repairs/spec.md) 的 FR-001 中“只有 Core 直接引用 `System.Numerics.Tensors`”的项目引用限制；不替代其 Repair 行为或 Core 对内置 Repair 的实现所有权。
- 被替代：无
- 相关 ADR：[ADR-0009](../../decisions/0009-group-scoped-optimizer-execution.md)、[ADR-0010](../../decisions/0010-scalar-evaluation-baseline.md)、[ADR-0013](../../decisions/0013-tensor-shaped-repair-bounds.md)、[ADR-0014](../../decisions/0014-spec-driven-change-governance.md)、[ADR-0016](../../decisions/0016-algorithm-fixed-width-simd-cascade.md)

## 问题与动机

`BatOptimizer`、`PsoOptimizer`、`FireflyOptimizer` 和 `CuckooOptimizer` 都在各自私有工作区内执行逐维 `double` 算术。现有实现正确地保持 RunGroup 私有状态、独立随机流、逐候选 Repair 与标量单点评估，但尚未以算法端到端基准证明哪些更新循环值得使用运行时 SIMD。

四种算法的向量化难度并不相同。PSO 每个粒子在逐维更新前只消耗两个随机数，随后是规则的速度限幅和位置更新，因此可在不改变随机数消费顺序的前提下进行私有 SIMD 试点。萤火虫的 attractor 遍历、吸引度计算与每次移动后的 Repair 时机是算法语义的一部分；距离平方的实现则应优先审查 `TensorPrimitives` 直接归约或组合。蝙蝠和布谷鸟的主要循环分别受分支相关随机数以及高斯采样、幂与三角函数限制；没有基准证据时，不能假定其 SIMD 化有收益。

需要建立一个按算法、按阶段、以性能证据为准入条件的路径：先验证 PSO，再在满足同样的数值、确定性和端到端性能门槛时验证萤火虫；其余算法先保留标量实现并以诊断基准决定是否另行立项。

## 目标

- 在不改变公开 API、Core 的单点评估契约或 Repair 职责的前提下，为 PSO 建立以 `TensorPrimitives` 为首选的私有张量更新路径及权威标量回退。
- 在 PSO 通过预先定义的正确性和性能门槛后，为萤火虫建立以 `TensorPrimitives` 为首选、仅覆盖逐维移动算术的私有张量路径。
- 用 BenchmarkDotNet 的内核与端到端证据决定每一阶段是否保留，避免为了“全部 SIMD 化”合入较慢实现。
- 保持每个 Optimizer 的工作区、随机流、Repair/Evaluate 调用链和运行隔离完全属于既有责任层。
- 按固定优先级选择实现：先使用一个直接表达所需逐元素运算的 `TensorPrimitives` 方法；其次使用不产生调用级分配的 `TensorPrimitives` 组合；只有前两者不能同时满足公式、重叠规则、数值兼容和性能门槛时，才使用 Algorithms 包内私有 `VectorOps`。

## 非目标

- 不新增 `IOptimizer`、`OptimizationRunContext`、`ContinuousProblem`、`IObjectiveFunction` 或批量评估 API；Core 继续只提供标量单点评估。
- 不改变 AoS 工作区布局，不引入 SoA、共享种群基类、公共 SIMD 开关、后端接口、数组池或跨 Optimizer 缓冲区。
- 不引入公开或 Core 级 `VectorOps`。Algorithms 包内的 `internal` `VectorOps` 只可封装经证明无法由直接 `TensorPrimitives` 或无分配组合表达的、被实际算法调用的纯 Span 算术；它不得包含边界、随机、评估、比较、缓存、配置或公开运行时分派职责。
- 任何实际算法调用的 Algorithms 私有 `VectorOps` 都可以在本 Spec 或后续对应 Spec 证明具体性能收益时，使用 `System.Runtime.Intrinsics` 中平台无关的 `Vector512<T>`、`Vector256<T>` 和 `Vector128<T>`，并按 512→256→128→标量尾部的顺序处理 Span。PSO 是当前首个落地点，Firefly 在本 Spec 内进行第二次候选验证。不得使用 `System.Runtime.Intrinsics.X86`、`.Arm` 或其他 ISA 专属 API；GPU、并行种群评估或算法布局迁移必须由后续 Spec 和必要 ADR 单独决定。
- 不在本 Spec 中迁移蝙蝠或布谷鸟的生产更新循环；它们只接受基准诊断，不以“覆盖全部算法”为完成条件。
- 不改变 Options、算法公式、默认值、比较、取消、Repair 时机、目标/约束数值域或随机数生成器。

## 架构契合

Algorithms 继续只引用 Core 作为项目依赖，并拥有各算法的私有状态与更新算术；Core 继续唯一拥有 Problem、Repair、评估、比较与运行生命周期。Algorithms 将通过直接包引用使用集中版本管理锁定的 `System.Numerics.Tensors`，与 Core 使用相同包版本。每个张量操作只读取当前 Optimizer 私有数组并写入其私有目标数组，不保存共享状态，不建立跨运行缓存，也不访问 Repair 的边界或内部实现。

实现选择必须按以下顺序记录在 Plan 与基准中：直接 `TensorPrimitives` 调用、无调用级分配的 `TensorPrimitives` 组合、最后才是 Algorithms 包内私有 `VectorOps`。后者是 `internal` 的纯 Span 算术实现细节，不是新的策略、公共后端或通用种群抽象；只有有实际调用点时才创建。任何采用 `VectorOps` 的算法都必须在当前剩余长度足够时依次选择 512、256、128 位向量后执行标量尾部。

该受限的直接包引用和内部帮助器不改变 [ADR-0010](../../decisions/0010-scalar-evaluation-baseline.md) 的单点评估决定。[ADR-0016](../../decisions/0016-algorithm-fixed-width-simd-cascade.md) 记录了项目作者将通用固定宽度向量作为 Algorithms 私有性能手段的决定；PSO 是首个落地点，Firefly 可在本 Spec 内按相同门槛验证。这不允许 ISA 专属 API，也不改变 [SPEC-0004](../SPEC-0004-masked-simd-reflect/spec.md) 中 CandidateRepairs 的职责。若需要 ISA 专属 API 或突破任一边界，必须先暂停实施并新增或替代 ADR。

## 信任与责任边界

| 数据或行为 | 责任方 | Core 是否验证 | 违反契约的结果 |
| --- | --- | --- | --- |
| 算法 Options 与私有工作区 | 对应 Optimizer | 否；构造时由 Optimizer 验证 | 保持既有参数异常或状态错误。 |
| 每 run 随机流、取消与评估计数 | `OptimizationRunContext` | 是 | 保持既有随机、取消与计数语义。 |
| 位置合法性、特殊位置值与边界 | 调用方选定的 `ICandidateRepair` | Core 通过 Context 委托 | 算法仅在既有时机调用 Repair，不检查或读取边界。 |
| 目标、约束与 Evaluation 数值域 | `ContinuousProblem` 与公开值对象 | 是 | 保持既有异常和有序扩展数值语义。 |
| 张量实现优先级与算法私有 `VectorOps` | Algorithms 维护者 | 否 | 未满足优先级、数值或性能门槛时保留标量实现。 |
| Tensor/SIMD 硬件可用性 | `TensorPrimitives`、通用固定宽度向量与 .NET 运行时 | 否 | PSO 按 512→256→128→标量尾部处理；功能语义由差分测试保护。 |

## 功能需求

### FR-001：PSO 私有张量候选生成

- 前置条件：`PsoOptimizer` 已完成 `ResetForRun`，并持有当前/目标双缓冲粒子数组与有效的 `OptimizationRunContext`。
- 触发行为：`Advance` 为每个粒子生成下一代候选。
- 预期结果：每粒子仍恰好先从当前 run 的 `Random` 取得一对认知/社会系数；随后按原有公式计算速度、按原有速度边界限幅、计算位置，并且只在完整 Position 写入后调用一次既有 `context.Repair`。实现必须先尝试直接 `TensorPrimitives` 方法；无单一方法覆盖完整公式时，才以 `TensorPrimitives` 原位组合和 Optimizer 已拥有的目标数组完成计算；只有该组合不满足约束时才能调用 Algorithms 私有 `VectorOps`。该帮助器按当前剩余长度依次使用可硬件加速的 `Vector512<double>`、`Vector256<double>`、`Vector128<double>`，最后逐元素标量处理，不按 `Vector<T>.Count` 选取单一宽度。
- 边界情况：零宽速度范围、`NaN`、正负 Infinity、负零、重叠输入/目标 Span 规则及 Repair 保留的特殊 Position 都必须产生与权威标量路径一致的逐维结果。若 `TensorPrimitives.Clamp` 或其他张量操作不能表达某个元素的标量语义，该元素必须退回标量计算，不能改变为隐式边界值。
- 验收标准：同一目标执行环境中，固定 seed 的重复运行产生逐位一致的候选 Position、Velocity、个人/全局最佳和 Evaluation；逐元素非归约算术与标量参考差分一致，随机数消费、Repair/Evaluate 调用次数、取消传播、RunGroup 隔离和工作区复用均与既有契约测试一致。不同平台之间不要求数值或轨迹逐位一致。

### FR-002：萤火虫私有张量移动

- 前置条件：PSO 已满足本 Spec 的 NFR-002 性能门槛，且 `FireflyOptimizer` 已完成 `ResetForRun`。
- 触发行为：`Advance` 将一个候选按当前代中严格更优的 attractor 顺序移动。
- 预期结果：严格比较和 attractor 遍历顺序保持不变。距离平方先按优先级审查直接 `TensorPrimitives` 归约；若没有直接等价操作，再以无分配的 `Subtract` 加 `Dot` 等张量组合计算 `sum((x - y)^2)`；最后才使用 Algorithms 私有 `VectorOps`。在已经确定的吸引度下，逐维位置更新也遵循相同优先级。每一个 attractor 仍按原维度顺序消耗一个随机步长样本；如需暂存样本，缓冲区必须为该 Optimizer 的私有、首次工作区分配时创建并在正常 run 间复用。每次移动后仍立即调用一次 `context.Repair`。
- 边界情况：没有严格更优 attractor、零随机步长、任意 Repair 特殊值、短维度、重叠 Span 规则不得改变原有状态或随机数消费。张量归约可使用与标量不同的求和次序；不同平台间不要求最后位或搜索轨迹一致。
- 验收标准：同一目标执行环境中固定 seed 的重复运行逐位一致；既有“只向严格更优成员移动且每次移动后 Repair”的测试继续通过，并新增张量归约、重叠 Span、特殊 Position 和重复运行测试。不同平台之间不使用固定结果快照作为验收条件。

### FR-003：基准驱动的阶段准入

- 前置条件：候选 SIMD 路径和相应标量参考均可在 Release 配置运行。
- 触发行为：在同一机器使用 BenchmarkDotNet 比较标量与候选路径。
- 预期结果：每个算法阶段均同时测量：一是以无额外 Repair 成本与廉价目标函数突出更新开销的 `Advance` 负载；二是带既有 Clamp Repair 和 Sphere 目标函数的端到端负载。每个负载须比较标量基线、直接 `TensorPrimitives`、可行的张量组合及实际使用私有 `VectorOps` 时的候选路径。主要维度为 32 和 128，主要种群大小为 64；长度 2、7、8、31、33、127、129 为诊断输入。
- 边界情况：不支持硬件 SIMD 的运行时、诊断短长度和目标/Repair 主导的工作负载可以无收益；不得从某一台机器推广为所有硬件、目标函数或维度的性能承诺。
- 验收标准：在两项主要维度和两类主要负载上，候选路径的平均耗时均低于对应标量基线，且 `Advance` 不产生新增托管分配。任何主要比较不达标时，删除该阶段的候选 SIMD 生产路径并保留标量实现及基准证据。

### FR-004：受限范围与后续算法诊断

- 前置条件：PSO 或萤火虫阶段的基准运行完毕。
- 触发行为：审查蝙蝠和布谷鸟的热点。
- 预期结果：可新增仅用于诊断的 BenchmarkDotNet 负载，量化蝙蝠的分支相关随机数成本、布谷鸟 Lévy 的随机采样/超越函数成本和遗弃候选的线性算术成本；不修改它们的生产实现。
- 边界情况：诊断发现最终线性算术比例很低、需要改变随机数消费顺序、需要向量化 `Pow`/三角函数或需要新布局时，均不进入本 Spec 的实施。
- 验收标准：任何 Bat/Cuckoo 生产 SIMD 提议都必须附带独立 Spec，其中明确随机数、数值与性能门槛；本 Spec 完成时这两个 Optimizer 仍保持原实现。

## 非功能需求

### NFR-001：目标环境内的确定性与数值语义

- 测量方式：在同一 OS、硬件架构、.NET Runtime、JIT 设置和库版本下，执行固定 seed 的重复运行；同时以标量参考差分逐元素非归约算术，覆盖两种优化方向、可行/不可行 Evaluation、特殊 Position、连续 run 复用和并发 RunGroup。
- 可接受阈值：同一目标执行环境的重复运行结果与随机数消费逐位一致；特殊值、Repair/Evaluate 时机、比较、取消和状态隔离保持既有语义；没有新增每轮随机数、验证、共享状态或可避免的工作区分配。跨 OS、硬件架构、Runtime 或 JIT 设置不保证数值、归约结果、搜索轨迹或最佳解逐位一致。
- 证据类型：自动化测试、实现审查和分配分析。

### NFR-002：可证实的性能替换门槛

- 测量方式：BenchmarkDotNet 使用同一 Runtime、硬件、配置、种子、维度和种群大小，报告平均时间、误差、分配、基线与加速比。
- 可接受阈值：FR-003 的四个主要比较均快于标量基线；`Advance` 每次调用的新增托管分配为零。若不满足，候选生产路径不得合入。
- 证据类型：BenchmarkDotNet、MemoryDiagnoser、Release 构建与端到端运行证据。

### NFR-003：实现边界可审计

- 测量方式：项目引用、公开 API 与残留搜索审查。
- 可接受阈值：Algorithms 仅新增对集中锁定 `System.Numerics.Tensors` 的直接引用；不新增公开 SIMD 类型、Core 批量评估、布局转换或跨 Optimizer 缓冲区。实际算法调用的 `internal VectorOps` 可使用通用 `Vector512<T>`、`Vector256<T>`、`Vector128<T>`；不得使用 ISA 专属命名空间。代码审查必须证明每项逐元素实现依次审查过直接 `TensorPrimitives`、无分配组合和 Algorithms 私有 `VectorOps`；后者仅为无状态、无验证、无随机性的纯 Span 算术。
- 证据类型：代码审查、`rg` 残留搜索、API/项目引用审查。

## 职责与替代关系

- 新增的概念：Algorithms 对 `System.Numerics.Tensors` 的直接包引用；以 `TensorPrimitives` 为首选的算法张量算术；仅在真实调用需要时的 Algorithms `internal` `VectorOps`；标量参考/回退；以及仅在需要时由 Firefly 独占的随机步长工作缓冲区。
- 被替代的概念：仅在通过性能门槛的 PSO/Firefly 逐元素算术中，原有标量循环由获胜的张量实现替代。
- 必须删除的旧行为或公共入口：不新增公共入口；性能门槛通过后删除被替代的整段生产标量循环，但保留具有独立职责的标量参考。未达门槛的张量候选路径必须删除，不保留开关或兼容壳。
- 明确保留的旧概念及独立理由：`OptimizationRunContext` 的随机、Repair、Evaluate 与取消；`EvaluationComparer`；RunGroup 私有状态；标量单点评估；算法特有工作区；以及 Core 的 Repair SIMD，均继续各自承担已有职责。
- 完成后每个概念的唯一所属层：Core 拥有运行协议、Repair 与评估；Algorithms 拥有 `TensorPrimitives` 的算法算术、必要时的 `internal VectorOps` 与标量回退；Benchmarks 记录证据；Tests 定义数值/确定性兼容；Spec 记录行为边界。

## 成功标准

- 用户继续用现有构造、Runner 和 Experiment 组装方式运行 PSO 与萤火虫，无需选择 SIMD 后端或调整 Problem/Repair。
- 通过门槛的阶段在定义的主要负载上更快且不新增 `Advance` 分配；未通过的阶段不会留下候选生产实现。
- 固定 seed 在同一目标执行环境中保持确定；Repair/Evaluate 调用时机、取消、工作区复用和 RunGroup 隔离保持既有可观察契约；跨平台数值稳定性不是承诺。
- 蝙蝠和布谷鸟不会因“迁移完整性”而引入未经测量的向量化或随机数/数值行为变化。

## 假设与已澄清决定

- 已确认：本变更编号为 `SPEC-0005`，此前同号的验证边界方向已放弃，不构成本 Spec 的范围或前置条件。
- 已确认：实现优先级为直接 `TensorPrimitives`、无分配 `TensorPrimitives` 组合、Algorithms 包内私有 `VectorOps`；Algorithms 可以直接引用集中锁定的 `System.Numerics.Tensors` 包版本。
- 已确认：不保证跨 OS、硬件架构、Runtime 或 JIT 设置的数值稳定性；只要求同一目标执行环境中的固定 seed 重复运行确定。
- 已确认：PSO 与萤火虫同属本 Spec，但萤火虫只有在 PSO 性能门槛通过后才进入实施。
- 已确认：主要性能门槛为两个维度（32、128）乘两类负载均优于标量基线，并要求零新增 `Advance` 分配。
- 已确认：PSO 私有向量公式参考 `TensorPrimitives` 的分级处理方式，在有足够剩余元素且硬件支持时依次使用 `Vector512<double>`、`Vector256<double>`、`Vector128<double>`，再处理标量尾部；不使用 ISA 专属 API。
- 已确认：ADR-0016 的固定宽度级联是 Algorithms 私有性能手段而非 PSO 独占授权；其他算法可在自身已批准范围内以同样的语义和 BenchmarkDotNet 门槛采用，Firefly 本次重新验证两条候选路径。

## 批准记录

- 规格批准：项目作者
- 批准日期：2026-08-29
- 批准时明确接受的风险：TensorPrimitives 的归约可因目标执行环境不同而改变末位、搜索轨迹或最佳解；跨平台数值稳定性不构成承诺。每个候选路径必须在定义的主要比较中快于标量基线，否则删除。
- 实施补充批准：项目作者
- 实施补充日期：2026-08-29
- 实施补充内容：以通用固定宽度向量的 512→256→128→标量级联替代基于单一 `Vector<T>.Count` 的长度阈值；每次 BenchmarkDotNet 测量前必须等待项目作者审阅代码和命令。
