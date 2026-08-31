# SPEC-0007：Repair 边界形状专用化

## 元数据

- 编号：`SPEC-0007`
- 状态：`Approved`
- 创建日期：2026-08-31
- 批准人：项目作者
- 批准日期：2026-08-31
- 替代：无；实施后替代 [ADR-0013](../../decisions/0013-tensor-shaped-repair-bounds.md) 对内置 Repair 四种边界形状的决定。
- 被替代：无
- 相关 ADR：[ADR-0013](../../decisions/0013-tensor-shaped-repair-bounds.md)、[ADR-0014](../../decisions/0014-spec-driven-change-governance.md)、[ADR-0017](../../decisions/0017-repository-private-simd-source-generation.md)

## 问题与动机

`CandidateRepairs.Clamp`、`Reflect` 和 `RandomReset` 当前都公开标量/标量、向量/标量、标量/向量、向量/向量四种工厂。两种混合形状缺少明确的实际应用场景，却使私有 `Boundary` 判别式、每次 Repair 的端点访问与 Reflect 每个 SIMD 块的 lower/upper/width/period 加载都必须运行时选择形状。

这既增加维护面，也使已获批准的 Reflect 掩码 SIMD 内核包含广播与数组加载的条件选择、nullable 派生参数和重复分支。本规格将 API 收窄到实际需要的两种同形状边界，并在创建时分派到具体私有类型，从热路径删除边界形状探测。性能优化只在不改变数值、随机性和 Repair 职责的条件下接受。

## 目标

- `Clamp`、`Reflect` 和 `RandomReset` 只提供标量/标量与向量/向量边界。
- 删除混合工厂、私有通用边界判别模型、混合测试/基准参数和任何兼容壳。
- 由工厂在创建时选择标量或向量的密封私有 Repair 类型；热路径不得再按边界形状分派。
- 将 Reflect 的 scalar/scalar 与 vector/vector SIMD 内核拆为唯一的广播或数组加载路径，保留已批准的 512→256→128→scalar 回退和危险 lane 标量修补。
- 测量 Clamp、Reflect、RandomReset 的局部与代表性端到端成本；只保留无可确认回退、无新增分配且具有维护或性能收益的专用实现。

## 非目标

- 不新增公开 SIMD API、运行时后端、泛型元素类型、对齐策略、ISA 专属 API、批量 Repair API 或边界模型。
- 不改变 `ICandidateRepair`、`ContinuousProblem`、Optimizer、RunGroup、Repair/Evaluate 时点或算法随机流。
- 不将混合端点自动广播成向量，也不保留 Obsolete 重载、转发层或运行时兼容路径。
- 不承诺全部硬件、长度或输入都加速；未通过性能准入的候选不得合入。

## 架构契合

Core 继续唯一拥有 Repair、边界副本、派生 width/period 和私有 SIMD。两个保留工厂在创建时分别实例化专用类型：标量类型仅保存标量端点及其标量派生值，向量类型仅保存防御性复制的等长数组及派生数组。现有 `ICandidateRepair` 作为运行上下文策略边界不变；本变更不新增 delegate、接口层级、反射、共享状态或运行时后端。

SIMD 位宽与硬件门仍由 ADR-0017 的编译期生成器机械展开。生成器不决定 Repair 形状或数值规则；它只为各专用模板生成现有固定宽度级联。ADR-0013 当前授权四形状 API，实施前必须以新的 Accepted ADR 替代其边界形状决定。

## 信任与责任边界

| 数据或行为 | 责任方 | Core 是否验证 | 违反契约的结果 |
| --- | --- | --- | --- |
| 选择同形状工厂 | 调用方 | 由可用签名在编译期限定 | 混合调用不再编译，调用方必须显式提供同形状边界。 |
| 标量端点 NaN 和顺序 | 调用方 | 创建时一次 | 保持既有构造异常。 |
| 向量端点 NaN、等长和逐维顺序 | 调用方 | 创建时复制并验证 | 保持既有构造异常。 |
| Position 与向量边界长度 | 调用方 | 每次向量 Repair 一次 | 保持 `ArgumentException`。 |
| 特殊值、Reflect 数值和 RandomReset 采样 | 所选 Repair | 按既有参考语义 | 不得改变可观察结果或随机消费条件。 |
| SIMD 硬件与尾部 | 运行时/生成代码 | 逐级回退 | 无 SIMD 时使用对应标量参考。 |
| 性能准入 | 维护者 | JIT、分配和 BenchmarkDotNet | 有可确认回退或新增分配时不得替换。 |

## 功能需求

### FR-001：收窄公开工厂

- 前置条件：调用方创建内置 Clamp、Reflect 或 RandomReset。
- 触发行为：调用 `CandidateRepairs` 工厂。
- 预期结果：每种 Repair 只保留 `double lower, double upper` 和 `ReadOnlySpan<double> lower, ReadOnlySpan<double> upper`；`DoNothing` 不变。
- 边界情况：不得保留 `ReadOnlySpan<double>, double` 或 `double, ReadOnlySpan<double>` 的 public/internal 重载、自动广播或兼容转发。
- 验收标准：API/残留审查与仓库编译证明三种混合入口、调用点、示例和基准形状彻底删除。

### FR-002：形状专用私有类型

- 前置条件：调用方使用任一保留工厂。
- 触发行为：创建 Repair 后调用 `Repair(Span<double>, Random)`。
- 预期结果：标量类型只直接使用标量端点；向量类型只直接索引或加载端点数组。删除 `Boundary` 判别结构、`LowerIsVector`/`UpperIsVector`、`GetLower`/`GetUpper` 与每次调用的形状选择。
- 边界情况：向量 Repair 继续仅在执行时检查 Position 长度；空向量与空 Position 保持兼容。
- 验收标准：实现与 Release JIT 审查确认每种保留形状有唯一端点访问路径，无旧四形状判断、nullable 派生字段或兼容壳。

### FR-003：边界、特殊值和随机兼容

- 前置条件：有效同形状边界与任意 Position/Random。
- 触发行为：创建或执行 Repair。
- 预期结果：NaN、逐维顺序、向量等长、Position 长度与防御性复制的验证阶段和异常类型保持不变。Clamp 保持 Position `NaN`；Reflect 保持特殊值、端点、大 offset 标量修补以及普通有限 lane 最多 1 ULP 的既有许可。
- 随机规则：RandomReset 在且仅在有限、越界且有限宽度的区间消费一次 `Random.NextDouble()`；其余情况按 Clamp 退化且不消费随机数。
- 验收标准：同形状差分、特殊值、边界、长度、复制、固定 seed 与随机次数测试通过；旧混合测试删除而非变为私有兼容测试。

### FR-004：简化同形状 Reflect SIMD

- 前置条件：执行标量或向量 Reflect，且运行时支持至少一种 128-bit SIMD 宽度。
- 触发行为：处理完整向量块。
- 预期结果：标量模板只广播 lower/upper/width/period；向量模板只加载数组。块内不得判断形状、选择广播/加载或检查 nullable 派生数组。
- 边界情况：长度 2、7、8、31、32、33、127、128、129、1024 继续采用 512→256→128→单元素尾部；危险大 offset 只修补对应 scalar lane。
- 验收标准：归一化反汇编无旧形状控制流；同形状差分覆盖正常、特殊、端点、溢出和大 offset lane。

### FR-005：性能优化准入

- 前置条件：先记录当前同形状 Release 基线。
- 触发行为：对 Clamp、Reflect、RandomReset 局部 Repair 与有实际 Repair 调用的代表性优化运行执行 BenchmarkDotNet。
- 预期结果：记录机器、Runtime、JIT、硬件宽度、job、维度、输入分布、分配及 `基线耗时 / 修改后耗时`。Reflect 的 scalar/scalar 与 vector/vector 的 32、128 维是主要比较；Clamp/RandomReset 至少覆盖两种保留形状和 32、128 维。
- 边界情况：基准不得伪造已删除混合形状的广播成本；随机基准必须可重复且把 Position 重置与 Repair 成本分开。
- 验收标准：无新增托管分配或运行时抽象；任一主要比较有统计上可确认回退时，删除对应激进优化，但不得恢复旧通用 Boundary 模型。Verification 记录局部与端到端比率或无有意义端到端路径的原因。

## 非功能需求

### NFR-001：明确的破坏性迁移

- 测量方式：API 审查、仓库全量编译、文档与残留搜索。
- 可接受阈值：不提供混合形状二进制、源码或运行时兼容层；除明确删除外无其他公开 API 漂移。
- 证据类型：替代 ADR、Release build、完整测试与文档更新。

### NFR-002：零新增运行时结构成本

- 测量方式：Release IL/JIT、MemoryDiagnoser 和实现审查。
- 可接受阈值：不得新增对象、托管分配、delegate、接口/虚调用、反射、共享状态或每块形状分派；既有 `ICandidateRepair` 调用边界不属于新增成本。
- 证据类型：反汇编、分配分析和残留搜索。

### NFR-003：数值、状态和确定性兼容

- 测量方式：差分、特殊值、固定 seed、随机次数、重复 run 和并发隔离测试。
- 可接受阈值：除 FR-001 明确删除的混合重载外，不引入新的数值容差、异常、状态、随机序列或调用时机差异；Reflect 仅保留 SPEC-0004 的 1 ULP 许可。
- 证据类型：自动化测试、实现审查、完整 Release 测试。

### NFR-004：有限维护复杂度

- 测量方式：类型、字段、模板、项目引用和生成输入审查。
- 可接受阈值：每种保留形状在每个 Repair 中只有一个权威实现；不扩大生成器/模板语言，不保留无消费者混合代码、benchmark enum、测试夹具或文档。
- 证据类型：残留搜索、生成器测试、Release build 和文档门禁。

## 职责与替代关系

- 新增概念：Core 私有 scalar/scalar、vector/vector Repair 专用类型及同形状 Reflect 模板。
- 被替代概念：三种混合工厂、通用 `Boundary` 判别存储和每次 Repair/SIMD 块的形状分派。
- 必须删除：混合工厂/构造器、形状标志、nullable 派生字段、混合 benchmark enum 值/测试分支及所有转发兼容层。
- 保留概念：`ICandidateRepair` 仍是稳定策略边界；同形状 Repair、标量 Reflect、危险 lane 修补、Tensor Clamp 与 RandomReset 仍有独立语义职责。
- 最终所属：Core 拥有工厂/边界/专用实现；生成器只拥有位宽和硬件门展开；Tests 定义兼容性；Benchmarks/Verification 记录性能证据。

## 成功标准

- 公开工厂只表达同形状边界，混合调用无兼容壳。
- Reflect 标量和向量路径不再运行时判断边界形状，SIMD 块只执行唯一广播或加载形态。
- 保留形状的数值、状态、随机性、尾部和分配保持兼容。
- 性能证据证明没有可确认回退，并报告专用化的局部和端到端影响。
- ADR、架构概览、API 文档、示例、测试、基准和 Verification 与最终实现一致。

## 已澄清决定与待批准项

- 已确认：三种内置 Repair 同时删除 vector/scalar 与 scalar/vector；不保留 Obsolete、自动广播或兼容层。
- 已确认：保留 scalar/scalar 与 vector/vector；向量边界继续创建时复制和验证、调用时匹配 Position 长度。
- 已确认：以工厂创建的不同私有类型消除形状判断，不新增运行时 backend 或公共类型。
- 待批准：后续 Plan 必须以新 ADR 替代 ADR-0013 的四形状决定；若更激进优化不达性能门槛，可保留更简单同形状专用实现，但不得恢复通用 Boundary 模型。

## 批准记录

- 规格批准：项目作者
- 批准日期：2026-08-31
- 批准时需明确接受的风险：这是破坏性 API 变更，混合端点调用方必须迁移；专用化的性能收益依赖机器、Runtime、JIT 与输入，只有同机证据通过后才可合入性能候选。
