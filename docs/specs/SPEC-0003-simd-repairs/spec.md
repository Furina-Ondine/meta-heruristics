# SPEC-0003：SIMD 内置 Repair

## 元数据

- 编号：`SPEC-0003`
- 状态：`Implemented`
- 创建日期：2026-08-28
- 批准人：项目作者
- 批准日期：2026-08-28
- 替代：无
- 被替代：[SPEC-0004](../SPEC-0004-masked-simd-reflect/spec.md) 的 FR-003 内部 Reflect 分派规则（整段安全预扫描与任一 lane 不安全即整段标量回退）
- 相关 ADR：[ADR-0001](../../decisions/0001-platform-and-toolchain.md)、[ADR-0010](../../decisions/0010-scalar-evaluation-baseline.md)、[ADR-0013](../../decisions/0013-tensor-shaped-repair-bounds.md)、[ADR-0014](../../decisions/0014-spec-driven-change-governance.md)

## 问题与动机

内置 `ClampCandidateRepair` 与 `ReflectCandidateRepair` 都在每次算法写入候选位置后逐元素运行。当前实现采用标量循环；当位置维度较大时，它们会重复执行可并行的数据运算。ADR-0013 已规定 Clamp 的结果与 `TensorPrimitives.Clamp` 对齐，并明确将 `System.Numerics.Tensors` 作为 Core 的实现依赖，但当前 Core 尚未引用该包，也未接入 SIMD 实现或相应性能证据。

需要在不改变公开 Repair API、边界所有权、异常、随机性和特殊值契约的前提下，使用 `System.Numerics.Tensors.TensorPrimitives` 加速 Clamp 与 Reflect，并用可重复的正确性测试和 BenchmarkDotNet 记录其实际影响。

## 目标

- 让内置 Clamp 的四种标量/向量边界形状使用与其参数形状对应的 `TensorPrimitives.Clamp` 原位实现。
- 让内置 Reflect 对适合向量处理的完整位置使用由 `TensorPrimitives` 组成的逐元素计算管线，并对不安全输入使用既有标量语义。
- 保留独立、可读的标量参考路径，用于异常/特殊值回退和验证基线。
- 建立 Repair 微基准与一个 Bat 端到端基准，记录不同维度、边界形状和向量尾部下的时间及分配数据。

## 非目标

- 不修改 `ICandidateRepair`、`CandidateRepairs` 工厂、`ContinuousProblem` 或任何公开 API。
- 不修改 Bat、PSO、Firefly 或 Cuckoo 的状态布局、更新公式、随机数消费或评估路径。
- 不向 RandomReset 引入向量化；其随机数消费顺序保持原样。
- 不引入批量目标评估、并行 Repair、池化、GPU 后端、公共 SIMD 抽象或性能开关。
- 不承诺所有硬件、所有维度、所有 Repair 形状或所有 Optimizer 均获得加速。

## 架构契合

本变更只在 `Metaheuristics.Core` 增加其已由 ADR-0013 指定的实现依赖，并只优化 Core 已拥有的内置 Repair 实现。Algorithms 继续只通过 `OptimizationRunContext.Repair` 使用调用方选择的策略，绝不读取边界或依赖 Tensor 包。Repair 仍在创建时复制和验证边界，并在运行时仅验证向量边界与位置长度关系。

`System.Numerics.Tensors` 的固定版本由集中包版本管理定义，Core 通过直接包引用使用它。没有新的运行状态、共享缓冲区或随机流：向量边界的 width 与 period 仅在 Repair 创建时计算并由该不可变 Repair 独占；每次 Repair 不分配临时数组。该实现不改变单点评估契约，也不改变 RunGroup 隔离、线程安全或确定性承诺。

ADR-0013 已明确此依赖归属及未来 SIMD 空间；本变更不改变长期架构决策，不需要新增或替代 ADR。

## 信任与责任边界

| 数据或行为 | 责任方 | Core 是否验证 | 违反契约的结果 |
| --- | --- | --- | --- |
| Clamp/Reflect 端点与向量长度 | 调用方通过 Repair 工厂提供 | 创建时验证端点，调用时验证位置长度 | 保持既有 `ArgumentOutOfRangeException` 或 `ArgumentException` |
| Position 中的有限值、NaN 和 Infinity | 调用方与所选 Repair | Repair 按定义处理，不作 Problem 级位置验证 | 由内置 Repair 的已定义语义决定 |
| Tensor 包版本与实现使用 | Core | 集中版本管理与构建验证 | 构建失败，不能静默降级到未记录的依赖 |
| SIMD 可用性及其性能 | 运行时与硬件 | TensorPrimitives 负责运行时选择；仓库以基准记录 | 不承诺统一加速，功能语义仍成立 |
| RandomReset 的随机流 | 调用方传入的 `Random` 与 RandomReset | 不改变当前实现 | 相同版本、运行时、设置和 seed 下继续可复现 |

## 功能需求

### FR-001：Core Tensor 依赖

- 前置条件：仓库继续以 `net10.0` 为目标框架并使用集中包版本管理。
- 触发行为：构建 `Metaheuristics.Core`。
- 预期结果：集中版本管理声明稳定的 `System.Numerics.Tensors` 10.0.11，且只有 Core 直接引用该包；Algorithms 不新增该依赖。
- 边界情况：包无法还原或类型不可用时，构建必须失败，不能以隐式、未锁定的包版本继续。
- 验收标准：Release restore/build 显示 Core 可使用 `TensorPrimitives`；项目依赖图不变。

### FR-002：SIMD Clamp

- 前置条件：调用方由任一 `CandidateRepairs.Clamp` 工厂创建 Repair，并传入长度有效的 Position。
- 触发行为：调用 `Repair(Span<double>, Random)`。
- 预期结果：实现对标量/标量、向量/标量、标量/向量、向量/向量四种端点形状使用相应 `TensorPrimitives.Clamp` 重载，Position 可原位写回。
- 边界情况：Position 的 `NaN` 保持 `NaN`；`-Infinity`/`+Infinity` 端点继续表示无下界/无上界；有限和无限 Position 按包含式端点截断；向量长度不匹配与构造参数异常完全保持既有语义。
- 验收标准：现有 Clamp 契约测试继续通过；新增所有边界形状、特殊值、长度 2、7、8、31、32、33、127、128、129、1024 的测试均与独立标量参考逐位一致。

### FR-003：SIMD Reflect 与标量回退

- 前置条件：调用方由任一 `CandidateRepairs.Reflect` 工厂创建 Repair，并传入长度有效的 Position。
- 触发行为：调用 `Repair(Span<double>, Random)`。
- 预期结果：当本次 Position 的每个分量、两端点、width、period 与 offset 均有限且 `width > 0` 时，Reflect 使用由 `TensorPrimitives` 的减法、remainder、加法、绝对值与相关逐元素操作组成的原位计算管线。向量端点的 width 与 period 在创建时预计算并私有保存；Repair 调用不分配临时数组。
- 回退：只要任一分量不符合前述条件，整次调用使用保留的标量参考路径。该路径继续处理 `NaN`、无穷 Position、单侧/双侧无界端点、零或溢出的 width/period，以及非有限 offset。
- 数值兼容性：安全 SIMD 路径的有限结果相对独立标量参考允许最多 1 ULP 差异；`NaN` 类别、无穷符号、端点命中、异常和回退路径结果必须与参考逐位一致。此有限值末位风险由项目作者在 2026-08-28 明确接受。
- 验收标准：四种端点形状和上述所有长度覆盖安全、回退与混合输入；随机有限样本、边界命中、极值以及 width/period/offset 溢出均有自动化测试，且满足数值兼容性规则。

### FR-004：RandomReset 与其他 Repair 保持不变

- 前置条件：调用方选择 RandomReset 或 DoNothing。
- 触发行为：调用相应 Repair。
- 预期结果：代码路径、随机数调用顺序、公开 API 和既有语义均不因本规格改变。
- 边界情况：RandomReset 对无界端点的 Clamp 退化及固定 seed 可复现性保持不变。
- 验收标准：现有 RandomReset/DoNothing 测试通过；新增更改不触及其实现逻辑。

### FR-005：性能证据

- 前置条件：Release 构建可执行 BenchmarkDotNet。
- 触发行为：运行 Repair 基准和 Bat 端到端基准。
- 预期结果：Repair 基准分别测量标量参考与实际 Clamp/Reflect，覆盖四种边界形状及长度 2、7、8、31、32、33、127、128、129、1024；Bat 基准明确使用内置 Clamp 或 Reflect，只报告该算法/配置组合。
- 边界情况：小维度、非对齐长度或不支持 SIMD 的硬件可能没有收益；基准不得把任何单一结果泛化为所有 Repair、维度或 Optimizer 的性能承诺。
- 验收标准：BenchmarkDotNet 输出记录在 Verification；实现审查确认正常 Repair 调用没有临时数组分配；对外文档不作未经数据支撑的倍数声明。

## 非功能需求

### NFR-001：契约保持与数值安全

- 测量方式：单元测试把实际 Clamp/Reflect 与独立标量参考逐元素比较，并覆盖工厂验证、运行时长度验证、特殊值和溢出回退。
- 可接受阈值：Clamp 和 Reflect 回退路径逐位相同；Reflect 安全 SIMD 路径仅允许有限正常值最多 1 ULP 差异；无新增异常、分配或随机数调用。
- 证据类型：自动化测试、代码审查。

### NFR-002：性能证据而非普遍性能承诺

- 测量方式：BenchmarkDotNet 在 Release 下报告微基准及 Bat 端到端结果，并由 MemoryDiagnoser 记录分配。
- 可接受阈值：每个正常 Repair 调用不分配临时数组；结果必须覆盖已定义的边界形状与对齐/尾部长度。结果可因运行时和硬件而不同，不设跨环境统一加速倍数。
- 证据类型：BenchmarkDotNet 输出、分配分析、实现审查。

## 职责与替代关系

- 新增的概念：无公开概念；仅新增 Core 内部的 Tensor 驱动实现、标量参考路径及创建时 width/period 缓存。
- 被替代的概念：Clamp 的热路径标量循环，以及符合安全条件的 Reflect 热路径标量循环。
- 必须删除的旧行为或公共入口：不保留与实际 SIMD 路径并存的旧 Clamp 热路径；不得新增公共 “Scalar/SIMD” 开关或 `VectorOps` 帮助层。
- 明确保留的旧概念及独立理由：Reflect 标量参考路径负责不安全分量的既有语义；RandomReset 与 DoNothing 因职责和随机性要求保持原样。
- 完成后每个概念的唯一所属层：Core 拥有 Tensor 包引用、内置 Repair、边界缓存和 SIMD/标量选择；Algorithms 仅调用 Context Repair；Benchmarks 仅测量；Tests 仅定义可观察契约。

## 成功标准

- 调用方不改代码即可获得具有原有 API、边界验证与特殊值语义的 Clamp 和 Reflect。
- Clamp 的全部形状逐位兼容；Reflect 对安全有限输入最多 1 ULP 偏差，其余行为逐位兼容。
- Repair 的向量化边界、尾部处理、标量回退和零临时分配都有可追踪测试及基准证据。
- 实现不把 Tensor、候选边界或性能策略泄漏到 Algorithms、Problems 的公共契约或批量评估模型。

## 假设与已澄清决定

- 范围只包含 Clamp 与 Reflect；不改变任一 Optimizer。
- Reflect 选择 TensorPrimitives 管线（方案 B），不使用手写向量 intrinsic 路径。
- 用户已接受安全有限 Reflect 输入最多 1 ULP 的末位差异；特殊值与回退结果仍要求逐位兼容。
- 非对齐长度 7 是必须覆盖的显式案例，并与前后相邻长度一起测试与基准。
- 没有未解决的公共行为问题。

## 批准记录

- 规格批准：项目作者
- 批准日期：2026-08-28
- 批准时明确接受的风险：Reflect 的 TensorPrimitives 计算链会改变部分有限值的最后一位；性能收益只由目标机器上的基准证明，不外推为通用承诺。
