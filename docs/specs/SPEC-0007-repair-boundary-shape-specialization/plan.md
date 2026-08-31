# SPEC-0007 技术计划

## 元数据

- 状态：`Approved`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`e41d33f08a53ad3ecbfcfaa5982419e017ac5e8c`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`NFR-001`、`NFR-002`、`NFR-003`、`NFR-004`
- 批准人：项目作者
- 批准日期：2026-08-31

## 当前实现调查

- 入口：`CandidateRepairs` 的 Clamp、Reflect、RandomReset 各公开四个 factory。`BoundedCandidateRepair` 保存两个私有 `Boundary`；每个 `Boundary` 以 scalar 或 copied `double[]` 表示端点。
- 调用链：算法只经 `OptimizationRunContext.Repair` 调用 `ICandidateRepair.Repair`。产品调用者使用 scalar/scalar；`ContinuousProblemTests`、`CandidateRepairsTensorTests`、`RepairBenchmarks` 仍覆盖四形状，`BatRepairBenchmarks` 提供 Clamp/Reflect 端到端路径。
- 热路径：Clamp 在 `Repair` 内按形状选择 `TensorPrimitives.Clamp` 重载；RandomReset 直接使用专用端点；Reflect 直接逐元素调用同一标量数值参考，不含 SIMD 模板或生成器接入。
- 既有保证：SPEC-0004 定义特殊 lane 与危险大 offset 的标量修补、2/7/8 等尾部和有限 lane 1 ULP 许可；SPEC-0006 定义模板/硬件门的零开销生成。ADR-0013 明确支持四形状，故与本 Spec 冲突。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A：仅删 public mixed overload，保留 `Boundary` | 改动小 | flag、nullable 派生值和块内分派仍存在 | 形成无消费者的四形状模型 | 否 |
| B：每个 Repair 一个类，内部 bool 分派两种保留形状 | 类型少 | 每次调用仍分派；Reflect 仍选择广播/加载 | 未兑现创建时分派与简化目标 | 否 |
| C：每种 Repair 建立 scalar/scalar 与 vector/vector 密封私有类型；Reflect 保持标量循环 | 删除全部形状判断，保留既有策略边界 | 六个私有类型、迁移测试/基准 | 必须维持一个标量参考权威 | 是 |
| D：公开泛型边界/后端策略 | 可扩展 | 增加 API、运行时抽象和配置 | 超出范围且可能增加分派 | 否 |

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| 混合端点工厂 | `CandidateRepairs` public API | 删除 | 无实际场景且造成维护面。 |
| 形状辨别与端点访问 | `BoundedCandidateRepair.Boundary` | 删除 | factory 已静态决定形状。 |
| scalar/scalar 三种 Repair | 通用派生类 + flag | 各自密封 scalar 类 | 仅保存/使用 scalar 数据。 |
| vector/vector 三种 Repair | 通用派生类 + flag | 各自密封 vector 类 | 仅保存 copied arrays，直接索引/加载。 |
| Reflect 标量参考 | `ReflectCandidateRepair` 私有成员 | `CandidateRepairs` 的单一私有 reference 函数 | 两种专用类型和危险 lane 共同依赖，避免数值规则复制。 |
| Reflect | 一个含形状选择的 AdditionalFile 模板 | 两个同形状标量循环 | 同时删除 SIMD 与形状选择。 |
| `ICandidateRepair` | Core public 策略边界 | 不变 | 保持 Context/Algorithms 运行模型。 |

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| scalar NaN 与端点顺序 | scalar factory | 创建时一次 | 否 | 保持构造异常。 |
| vector NaN、等长、逐维顺序和复制 | vector factory | 创建时一次 | 否 | 防御性复制，保持构造异常。 |
| Position/vector 长度 | vector `Repair` | 每调用一次 | 是 | 保持 `ArgumentException`。 |
| 特殊值、端点、溢出、大 offset | scalar reference | 每元素 | 是 | 保持既有数值规则。 |
| RandomReset 采样条件 | 专用 RandomReset | 每 element | 是 | 仅在既有条件下消费一次 random。 |

## API 与行为变化

- 新增：无 public API；只有 Core 私有具体类型。
- 修改：factory XML 只描述“两端均为标量”或“两端均为逐维向量”；架构概览记录同形状专用现状。
- 删除：三种 `ReadOnlySpan<double>, double` 与三种 `double, ReadOnlySpan<double>` public factory；`BoundedCandidateRepair`/`Boundary` 及其成员。
- 破坏性迁移：调用方必须将 scalar 显式扩展成 Position 维度数组，或将同一向量端点改为统一 scalar；选择由调用方领域语义决定，Core 不自动广播。
- 不变：两个保留签名、默认 `Clamp(0,10)`、向量复制/验证、Position 长度、Clamp/Reflect 特殊值、RandomReset 随机消费和算法调用时机。

## 替代与清理计划

- 新增 ADR-0019，并将 ADR-0017、ADR-0018 标记为 `Superseded`；ADR-0019 保留同形状边界决定并记录 Reflect 的标量回退。
- 删除 mixed factory/constructors、形状 flag、nullable width/period、Reflect SIMD 模板、Core 的 generator analyzer/AdditionalFiles 引用、混合 benchmark enum 值、测试分支与全部 Obsolete/转发层。
- 更新 XML、ADR index、architecture overview、SPEC-0007 Verification；若用户/API 文档有四形状说明则更新。产品示例当前仅 scalar/scalar，复核后应无需调用迁移。
- 用 `rg` 审查 mixed signatures、`VectorScalar`、`ScalarVector`、`Boundary`、`LowerIsVector`、`UpperIsVector`、`GetLower`、`GetUpper`、旧 nullable derived fields 与旧模板 helpers；不允许保留任何生产兼容路径。

## 连带影响矩阵

| 区域 | 是否受影响 | 具体影响或无影响理由 | 验证证据 |
| --- | --- | --- | --- |
| Core | 是 | factories、存储、三种 Repair、移除 Reflect SIMD templates | 定向/完整测试、残留搜索、分配。 |
| Algorithms | 否 | 只依赖 `ICandidateRepair`，不读边界 | build、算法测试、搜索。 |
| Experiments | 否 | 无 factory 调用 | build、搜索。 |
| Examples | 可能 | 当前只用 scalar/scalar，复核即可 | build、审查。 |
| Tests | 是 | 删除混合夹具，补同形状/随机/API 测试 | 定向/完整测试。 |
| Benchmarks | 是 | 删除 mixed enum，测三种 Repair 的保留路径 | BDN、MemoryDiagnoser。 |
| XML/API 文档 | 是 | 删除四形状表述 | DocFX、API 审查。 |
| ENGINEERING | 否 | 现有 SDD/性能规则已适用 | 审查。 |
| ADR | 是 | ADR-0018 替代 ADR-0013 | index/链接检查。 |

## 需求—验证设计

| 需求 | 自动化测试或基准 | 测试层级 | 预期证据 |
| --- | --- | --- | --- |
| FR-001 | API 编译、残留搜索、全仓库 build | API/集成 | 仅两种 factory、无调用残留。 |
| FR-002 | 三种 Repair 的 scalar/vector 行为与实现残留测试 | 单元/审查 | 无 `Boundary`/flag，唯一端点路径。 |
| FR-003 | 特殊值、边界、长度、复制、fixed seed/随机次数 | 单元 | 数值、异常、随机兼容。 |
| FR-004 | 指定长度/特殊值差分、残留搜索 | 单元/审查 | 无形状控制流、SIMD 或生成器接入。 |
| FR-005 | 拒绝 SIMD 的 Repair 对照与 allocation | 局部/审查 | 比率、分配、命令、环境。 |
| NFR-001 | API diff、build、文档/ADR 审查 | 工程 | 只有已批准的破坏性删除。 |
| NFR-002 | MemoryDiagnoser、残留搜索 | 性能/审查 | 无新增 runtime structure/allocation。 |
| NFR-003 | 全部 Release 测试、fixed seed/隔离 | 集成 | 无未批准语义漂移。 |
| NFR-004 | generator tests、Core 引用/模板审查 | 构建 | 生成器不再服务 Core，无死代码。 |

## 风险和回退

- 最大风险：拆分 Reflect 时复制或遗漏 remainder、端点、特殊 lane、危险 offset、随机消费或尾部规则。
- 早期发现：先以 `7380b9c` 同形状生产路径记录 baseline；每类完成即运行差分/seed 测试，查看生成源/Dry JIT，再在作者确认后跑 BenchmarkDotNet。
- 回退：性能候选失败时删除候选内核，保留正确的同形状专用实现；语义/JIT/分配失败时不提交，恢复已提交基线。不得保留新旧运行时开关或四形状模型。
- 退回 Spec 条件：需要保留混合调用、自动广播、公开边界类型、不同数值容差、RandomReset 规则变化、新宽度或生成器 DSL 扩展。

## ADR 判断

- 是否触发 ADR：是。
- 判断依据：ADR-0013 的 Accepted 决定明确授权四种 factory；本计划要进行破坏性 API 删除并以专用类型替代通用表示。
- 新 ADR 或被替代 ADR：新增 `ADR-0018`（Accepted）记录收窄到 scalar/scalar 与 vector/vector；ADR-0013 标为 `Superseded` 并链接 ADR-0018。

## 批准记录

- 计划批准：项目作者
- 批准日期：2026-08-31
