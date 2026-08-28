# SPEC-0004 技术计划

## 元数据

- 状态：`Approved`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`a87df18`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`NFR-001`、`NFR-002`
- 批准人：项目作者
- 批准日期：2026-08-28

## 当前实现调查

- 当前相关类型和入口：`CandidateRepairs.ReflectCandidateRepair` 在 Core 中保存创建时计算的 scalar/vector width 与 period。`Repair` 先调用 `CanUseTensorPath` 以标量循环检查整段 Position；成功时用 `TensorPrimitives.Subtract`、`Remainder`、`Abs`、`Add` 管线原位计算，失败时整段调用私有标量 `Reflect`。
- 当前调用链：Optimizer 在每次改写位置后调用 `OptimizationRunContext.Repair`，后者把 run 独占的 `Random` 传给 `ICandidateRepair`。所有算法均只走该入口，且不读取边界。
- 当前职责所属层：Core 正确拥有边界、Repair、Tensor 包和数值语义；Algorithms、Experiments 与 Examples 均无需引用 SIMD 类型。
- 已存在的相似或重复概念：`BoundedCandidateRepair.Clamp` 与私有标量 `Reflect` 是既有语义参考。不得再创建公开或跨类的 SIMD 抽象。
- 当前测试、示例、文档和基准：`CandidateRepairsTensorTests` 已覆盖四种形状、长度 2/7/8/31/32/33/127/128/129/1024、有限样本和少量特殊值。`RepairBenchmarks` 当前对比标量参考与内置实现，但没有保留“当前整段 Tensor 分派”作为实现后仍可运行的同机基线。
- 与 Spec 或 ADR 的已知冲突：`SPEC-0003` 的整段分派被 `SPEC-0004` 显式替代；两者其余外部数值契约共同有效。ADR-0013 未锁定 Reflect 的 SIMD 后端，因此不存在 ADR 冲突。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A：将 `CanUseTensorPath` 改为 `TensorPrimitives.IsFiniteAll` | 预扫描本身可 SIMD。 | 一个异常 lane 仍会回退整段，不能解决核心问题。 | 低，但不满足 FR-001。 | 不采用。 |
| B：对每个硬件向量块使用 intrinsic 掩码、候选值与条件选择 | 正常与异常 lane 可在同一块内独立处理；无需调用级缓冲区；可按剩余长度缩窄向量宽度。 | 需要三种向量宽度的私有内核、手写 remainder 和严格数值测试。 | 末位差异与实现复杂度受 FR-002/NFR-002 约束。 | 采用。 |
| C：将原始 Position 复制到临时数组，再对整段保留 Tensor 管线并回填异常 lane | 可复用现有 remainder 管线。 | 需要调用级数组或池化所有权，增加带宽和状态风险。 | 违反 FR-003，且会引入无消费者的缓冲区概念。 | 不采用。 |

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| Reflect 安全判断 | Core 的 `CanUseTensorPath` 整段标量循环 | Core 的私有向量 lane 掩码 | 异常 lane 不应取消同块正常 lane 的 SIMD。 |
| Reflect 算术和结果写回 | Core 的全 Span Tensor 管线或整段标量循环 | Core 的私有 512/256/128 位块内核 | 原始值、Clamp 候选和 Reflect 候选需同时存在于寄存器中。 |
| 边界 width/period | Core 创建时私有缓存 | 不变，并可增加仅依赖边界的私有分类缓存 | 避免在热路径重复推导。 |
| 标量 Reflect | Core 私有参考 | 不变 | 无 SIMD 硬件、最终单元素尾部与不能安全向量表达的 lane 的权威结果。 |
| Clamp、RandomReset、算法调用链 | 现有 Core/Algorithms | 不变 | 不在本 Spec 范围内。 |

## 内核设计

1. `ReflectCandidateRepair.Repair` 保留 `ValidatePositionLength`，删除整段 `CanUseTensorPath`。它以索引遍历 Position：在 `Vector512.IsHardwareAccelerated` 时优先处理所有完整 8-double 块；随后在 `Vector256`、`Vector128` 可用时分别处理完整 4-double、2-double 块；最后仅处理不足 2 个 double 的标量尾部。若没有任一硬件向量宽度，则使用原始标量循环。
2. 每种宽度使用同一私有算法、独立的 `Vector512<double>`、`Vector256<double>`、`Vector128<double>` 方法，避免公共泛型/后端层。通过 `MemoryMarshal.GetReference`、`LoadUnsafe` 与 `StoreUnsafe` 加载 Position 和向量边界；标量边界、width 与 period 用对应宽度的 `Create` 广播。不得开启 `unsafe` 编译选项。
3. 每个块先把原始 Position 保留为向量寄存器，并从该原值并行得到 Clamp 候选：`value < lower ? lower : value > upper ? upper : value`。这保留 NaN 的原始 lane，并对无穷位置与无界端点保持既有 Clamp 行为。
4. 同一块为标量 `Reflect` 的语义分支构造掩码：输入和端点有限、width/period 为有限且 width 正、offset 有限。严格位于 `(lower, upper)` 的 lane 与精确端点 lane 均以原值为结果；不满足可反射条件的 lane 选择 Clamp 候选。任何计算中出现的 NaN/无穷候选都不得未经掩码写回。
5. 其余可反射 lane 计算 `offset = value - lower`、`remainder = offset - (period * Truncate(offset / period))`、负 remainder 修正、三角反射结果。该表达与 `%` 的标量定义对齐；精确端点不选择该计算结果，避免重新舍入。向量计算即使在未选择的 lane 上得到 NaN 也不抛出异常，最终由条件选择屏蔽。
6. 以条件选择合成优先级为“Clamp 退化 → 原值（严格内部或端点）→ 可反射结果”，然后只写一次 Position 块。若差分测试发现任何按此掩码仍无法达到 FR-002 的 lane，调用私有标量 `Reflect` 仅修补该 lane；不得恢复整段或整个向量块的回退。
7. 创建时继续保存 width/period。只依赖边界的有限性、正宽度及 period 有限性可缓存，调用时仍对 Position、offset 和端点关系生成 lane 掩码。缓存只读，不保存 Position、Random 或 run 状态。

## API 与行为变化

- 新增：仅 `CandidateRepairs` 私有的 intrinsic 块方法、只读边界派生缓存，以及 benchmark 中私有的旧分派基线。
- 修改：Reflect 的内部执行从全 Span Tensor/整段标量分派改为按 lane 掩码与逐级宽度处理。
- 删除：`CanUseTensorPath` 及其“任一 lane 不安全即整段标量”的调用点；全 Span Tensor Reflect 管线不再作为正常路径保留。
- 破坏性变化：无。公开 API、异常、特殊值、端点和随机性不变；普通有限可反射 lane 继续适用已批准的最多 1 ULP 误差边界。
- 调用方迁移方式：无。
- 明确保持不变的行为：边界复制/验证、调用时长度验证、Clamp 的 TensorPrimitives 实现、RandomReset 的随机数消费、DoNothing、Optimizer 状态布局和单点评估。

## 替代与清理计划

- 被取代的类型、接口和入口：无公开符号；仅替代现有 Reflect 整段检查和全 Span Tensor Reflect 管线。
- 必须删除的转发层、兼容壳和重复抽象：不新增/保留 `SimdRepair`、`VectorOps`、后端开关或独立的边界类型；删除 `CanUseTensorPath`。
- 必须删除或改写的旧测试：保留全部既有差分测试；扩展特殊值测试为同一硬件块中的混合 lane。不得删除标量参考。
- 必须更新的示例和文档：实现完成后更新 architecture overview 的 Reflect 实现状态；不改变公开 XML、示例或 API Overview，因为调用方行为不变。
- 全仓库残留搜索方式：`rg -n "CanUseTensorPath|SimdRepair|VectorOps|System\.Runtime\.Intrinsics" src tests benchmarks docs`；`CanUseTensorPath` 必须为零，Intrinsic 引用仅在 Core 私有实现及测试/基准说明的预期位置。
- 保留旧结构时的真实消费者、期限和删除条件：标量 `Reflect` 是无 SIMD 硬件、最终尾部与精确修补的语义参考，长期保留；benchmark 中的旧分派基线仅服务 FR-005，随本 Spec 的验证一起保留。

## 连带影响矩阵

| 区域 | 是否受影响 | 具体影响或无影响理由 | 验证证据 |
| --- | --- | --- | --- |
| Core | 是 | Reflect 私有算法改为 intrinsic 块内核。 | Release build、差分测试、审查。 |
| Algorithms | 否 | 仍只经 Context 调用 Repair。 | 完整测试、引用搜索。 |
| Experiments | 否 | 无 RunGroup、Random 或统计变更。 | 完整测试。 |
| Examples | 否 | 无 API 或调用迁移。 | Release build。 |
| Tests | 是 | 增加混合 lane、宽度/尾部和逐位断言。 | Core 测试。 |
| Benchmarks | 是 | 新增旧分派同机基线和长度自适应报告。 | BenchmarkDotNet。 |
| XML 文档 | 否 | 公开语义不变。 | DocFX。 |
| 用户/API 文档 | 否 | 不宣传未经证实的普遍倍数。 | 文档审查。 |
| ENGINEERING | 否 | 继续遵守性能、数值和状态规则。 | 审查。 |
| ADR | 否 | ADR-0013 已授权 Core 内部 SIMD；无长期模型变化。 | ADR 判断。 |

## 需求—验证设计

| 需求 | 自动化测试或基准 | 测试层级 | 预期证据 |
| --- | --- | --- |
| FR-001 | 四种形状、每个指定长度的正常/混合 lane 差分测试；`CanUseTensorPath` 残留搜索 | Core 单元/审查 | 异常 lane 不触发整段回退。 |
| FR-002 | 独立标量参考；NaN、正负无穷、单/双侧无界、端点、零宽、width/period/offset 溢出及混合块 | Core 单元 | 特殊/端点逐位相同，正常有限最多 1 ULP。 |
| FR-003 | MemoryDiagnoser、构造/长度既有测试和实现审查 | 单元/基准 | 无 Repair 级分配，边界规则不变。 |
| FR-004 | 长度 2、7、8、31、32、33、127、128、129、1024 的差分测试和 BenchmarkDotNet | 单元/基准 | 512→256→128→尾部控制及正确尾部结果。 |
| FR-005 | `LegacyTensorReflect` 与候选内核同进程比较：标量/标量、向量/向量 × 32/128；全形状全长度诊断矩阵 | BenchmarkDotNet | 四个主要平均值均低于基线，且分配记录完整。 |
| NFR-001 | Release restore/build/test、固定 seed 回归、API diff 审查 | 工程/单元 | 无公开、随机或数值契约漂移。 |
| NFR-002 | 引用与残留搜索、项目引用审查 | 审查 | Intrinsic 仅在 Core 私有 Reflect 内核。 |

## 风险和回退

- 最大实现风险：以除法/截断表达 remainder 的有限结果可能超出 1 ULP，或掩码优先级与标量参考的某个特殊值分支不一致。
- 可能产生的性能风险：块内同时计算 Clamp 与 Reflect 候选的指令数，可能超过删除预扫描带来的收益，尤其是短长度。
- 如何及早发现：先扩展独立标量差分测试，再实现最窄宽度并依次增加 256/512；在替换 Core 正常路径前，将旧整段分派复制为仅限 benchmark 的私有基线并运行同机 BenchmarkDotNet。
- 回退条件：任一特殊/端点差分失败、普通有限 lane 超过 1 ULP、出现 Repair 级分配，或 FR-005 的任一主要比较没有改善时，停止并保留提交 `21804bd` 中的现有实现；不得以放宽数值契约或保留性能开关绕过门槛。
- 退回 Spec 澄清的条件：若满足数值语义需要新的公开行为、共享缓冲区、Algorithms 协作，或必须改变已定义的性能比较集合。

## ADR 判断

- 是否触发 ADR：否。
- 判断依据：实现仍位于 ADR-0013 已指定的 Core Repair 层，不改变项目依赖、公开执行模型、边界职责、随机性或可复现性；Intrinsic 是目标框架设施而非新的计算后端或公共策略。
- 新 ADR 或被替代 ADR：无。

## 批准记录

- 计划批准：项目作者
- 批准日期：2026-08-28
