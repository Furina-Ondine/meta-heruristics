# SPEC-0003 技术计划

## 元数据

- 状态：`Draft`
- 对应 Spec：[`spec.md`](./spec.md)
- Spec 基线提交：`b2faf3d`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`FR-004`、`FR-005`、`NFR-001`、`NFR-002`
- 批准人：—
- 批准日期：—

## 当前实现调查

- 当前相关类型和入口：`CandidateRepairs` 在 Core 中以私有 `BoundedCandidateRepair` 和 `Boundary` 保存标量或复制后的 `double[]` 边界；Clamp、Reflect、RandomReset 均通过 `GetLower`/`GetUpper` 在标量循环中处理。`ICandidateRepair.Repair(Span<double>, Random)` 是唯一调用契约。
- 当前调用链：Optimizer 初始化或改写 Position 后调用 `OptimizationRunContext.Repair`，Context 委托 `ContinuousProblem.Repair`；算法项目不读取边界。Bat、PSO、Firefly 和 Cuckoo 均使用该路径。
- 当前职责所属层：Core 正确拥有内置 Repair、边界验证和特殊值语义；Algorithms 只生成位置并委托 Context。
- 已存在的相似或重复概念：无 Tensor 包引用、SIMD 封装或 Repair 基准。现有 `BoundedCandidateRepair.Clamp` 是 Clamp、Reflect 回退与 RandomReset 共享的标量权威语义，应保留而非复制公共工具。
- 当前测试、示例、文档和基准：`ContinuousProblemTests` 覆盖四种端点形状、定义/长度验证以及一次 Reflect 特殊值示例。基准项目只有 Bat 工作区复用与 RunGroup 调度基准；没有 Repair 性能或等价性基准。`BenchmarkSwitcher` 自动发现基准类型。
- 与 Spec 或 ADR 的已知冲突：ADR-0013 已声明 `System.Numerics.Tensors` 是 Core 实现依赖，但当前 `Directory.Packages.props` 和 Core 项目文件未包含该引用；这正是 FR-001 要补齐的既有实现缺口。

## 方案选择

| 方案 | 优点 | 成本 | 架构风险 | 是否采用 |
| --- | --- | --- | --- | --- |
| A：仅以 TensorPrimitives 替换 Clamp，Reflect 保留标量 | 最小且严格对齐 ADR-0013。 | 未满足本规格的 Reflect SIMD 目标。 | 低，但范围不足。 | 不采用。 |
| B：Clamp 使用 `TensorPrimitives.Clamp`；Reflect 用 TensorPrimitives 管线、创建时 width/period 缓存和整次标量回退 | 不添加公共 API、运行时缓冲或手写 intrinsic；覆盖两个无随机内置 Repair。 | Reflect 需要安全预扫描、四种形状分派及数值兼容测试。 | 受限的末位差异；由 Approved Spec 的 1 ULP 契约控制。 | 采用。 |
| C：Reflect 使用 `Vector256/512` 掩码与条件选择 | 可较贴近当前标量的分支序列。 | 引入低层硬件实现、尾部控制和与 TensorPrimitives 并行的第二套 SIMD 模型。 | 高；偏离用户选定的 TensorPrimitives 管线。 | 不采用。 |

## 目标职责模型

| 概念或行为 | 变更前所属 | 变更后所属 | 原因 |
| --- | --- | --- | --- |
| Tensor 包版本 | 无 | 集中包版本管理 | 固定 Core 的实现依赖版本。 |
| Tensor 包引用 | 无 | Core 项目 | ADR-0013 已规定 Core 拥有此实现依赖。 |
| Clamp 逐元素执行 | Core 标量循环 | Core 的 TensorPrimitives 分派 | API 与边界归属不变，只替换内部热路径。 |
| Reflect 安全有限输入 | Core 标量循环 | Core 的 TensorPrimitives 计算链 | 用户已选择该计算后端与 1 ULP 兼容范围。 |
| Reflect 特殊值/危险数值 | Core 标量循环 | Core 保留的标量参考路径 | 保持无界、NaN、Infinity、溢出与端点语义。 |
| RandomReset、DoNothing、Optimizer 更新 | Core/Algorithms 现有实现 | 不变 | 不在 Approved Spec 范围内。 |

## 信任和验证设计

| 输入或结果 | 验证位置 | 验证次数 | 是否在热路径 | 保护的不变量与失败语义 |
| --- | --- | --- | --- | --- |
| 边界 `NaN`、顺序和向量长度 | 现有 `Boundary` 创建与 `ValidateBounds` | 创建时一次 | 否 | 保持当前异常类型和消息语义；不在 Repair 重复验证。 |
| Position 与向量边界长度 | `ValidatePositionLength` | 每次 Repair 一次 | 是，既有检查 | 长度不匹配仍抛出 `ArgumentException`。 |
| Clamp 输入/输出重叠 | TensorPrimitives 调用 | 每次 Repair | 是 | 输入与 destination 均为同一 Position 起始 Span，符合 API 重叠规则。 |
| Reflect SIMD 安全条件 | Reflect 私有预扫描 | 每次 Repair 一次 | 是 | 仅所有 lane 的 value、端点、width、period、offset 和中间计算均有限、width 正且不触发端点精确性回退时进入管线；否则整次调用标量参考。 |
| Reflect width/period | 构造 Reflect 时 | 创建时一次 | 否 | 向量形状缓存私有数组；标量形状保存标量值；不创建调用级临时数组。 |
| Reflect 输出 | 测试中的独立标量参考 | 自动化测试 | 否 | 有限安全路径最多 1 ULP；特殊值、端点、回退逐位一致。 |
| 随机流 | RandomReset 回归测试与代码审查 | 每次测试 | 不适用 | 本计划不编辑 RandomReset；不新增或移动 `NextDouble`。 |

## API 与行为变化

- 新增：Core 对 `System.Numerics.Tensors` 的私有实现依赖、内部 width/period 缓存、私有标量参考帮助方法及基准类型。
- 修改：Clamp 使用 TensorPrimitives；Reflect 对符合安全条件的有限输入使用 TensorPrimitives 计算链。
- 删除：Clamp 的独立热路径标量 `for` 循环；在安全 Reflect 调用中被取代的标量循环。
- 破坏性变化：无。Reflect 正常有限输出允许最多 1 ULP 差异，是 Approved Spec 明确记录的可观察数值兼容范围。
- 调用方迁移方式：无；现有 `CandidateRepairs` 工厂和 `ICandidateRepair` 调用保持不变。
- 明确保持不变的行为：边界所有权与防御性复制、参数和长度异常、NaN/Infinity/无界语义、RandomReset 随机数消费、DoNothing、Optimizer 算法和单点评估。

## 替代与清理计划

- 被取代的类型、接口和入口：无公开类型或入口；只取代 Clamp 热循环和安全 Reflect 热循环。
- 必须删除的转发层、兼容壳和重复抽象：不得创建公开的 `SimdRepair`、`VectorOps`、后端策略、配置开关或 Algorithms Tensor 帮助层。
- 必须删除或改写的旧测试：不删除现有契约测试；将其扩展为形状、长度、特殊值与数值误差覆盖。
- 必须更新的示例和文档：更新 `CandidateRepairs` 的 XML remarks（仅说明语义与内部优化不改变 API）；架构概览仅在实现落地后报告 Core 使用 Tensor 实现依赖。无需示例迁移。
- 全仓库残留搜索方式：使用 `rg -n "System\.Numerics\.Tensors|TensorPrimitives|VectorOps|SimdRepair" .` 确认 Tensor 只在 Core 和测试/基准预期位置出现，且未产生禁止的公共抽象。
- 保留旧结构时的真实消费者、期限和删除条件：Reflect 标量参考路径是特殊值和安全回退的唯一实现，长期保留；RandomReset 和 DoNothing 服务不同策略语义，长期保留。

## 连带影响矩阵

| 区域 | 是否受影响 | 具体影响或无影响理由 | 验证证据 |
| --- | --- | --- | --- |
| Core | 是 | 新包引用、边界访问/缓存和 Clamp/Reflect 内部实现。 | Release build、Core 测试。 |
| Algorithms | 否 | 只经既有 Context 调用 Repair，不改公式、状态或依赖。 | 残留搜索、完整算法测试。 |
| Experiments | 否 | 不接触 RunGroup、Random 或统计。 | 完整测试。 |
| Examples | 否 | API 与调用方式不变。 | Build。 |
| Tests | 是 | 增加 Repair 参考、形状/长度、特殊值及数值兼容测试。 | 测试结果。 |
| Benchmarks | 是 | 新增 Repair 微基准与 Bat Repair 端到端组合基准。 | BenchmarkDotNet 输出。 |
| XML 文档 | 是 | CandidateRepairs 注释说明实现不改变已有行为。 | XML 文档 build。 |
| 用户/API 文档 | 否 | 无新增 API 或迁移；不增加性能倍数宣传。 | 文档审查。 |
| ENGINEERING | 否 | 实现遵循现有性能、依赖与数值规则。 | 审查。 |
| ADR | 否 | ADR-0013 已授权 Core Tensor 依赖；不改变架构决策。 | ADR 判断。 |

## 需求—验证设计

| 需求 | 自动化测试或基准 | 测试层级 | 预期证据 |
| --- | --- | --- | --- |
| FR-001 | Core/solution Release restore 与 build；项目引用审查 | 工程 | 包版本集中且仅 Core 直接引用。 |
| FR-002 | 四种 Clamp 形状 × 2/7/8/31/32/33/127/128/129/1024，有限/NaN/Infinity 标量参考逐位比较 | Core 单元 | 保持所有 Clamp 契约和尾部正确性。 |
| FR-003 | 四种 Reflect 形状 × 所有长度；安全有限随机样本 1 ULP 比较；特殊值、端点、无界和溢出逐位比较 | Core 单元 | 安全分派与标量回退符合 Spec。 |
| FR-004 | 现有 RandomReset/DoNothing 与固定 seed 测试；差异审查 | Core 单元 | 无随机流或无关 Repair 漂移。 |
| FR-005 | Repair 微基准与 Bat Clamp/Reflect 端到端基准，含 MemoryDiagnoser | BenchmarkDotNet | 时间、分配及适用边界记录在 Verification。 |
| NFR-001 | 全部 Core/Algorithms 测试和数值参考辅助测试 | 单元/集成 | 无新增异常、随机或特殊值回归。 |
| NFR-002 | Release BenchmarkDotNet 输出与实现审查 | 性能 | 正常 Repair 无调用级临时数组；无普遍性能承诺。 |

## 风险和回退

- 最大实现风险：TensorPrimitives 的多步 Reflect 运算可能在有限输入的尾数、端点命中或中间 overflow 上超出已批准的 1 ULP 边界。
- 可能产生的行为漂移：仅批准的安全有限 Reflect lane 允许末位差异；Clamp、特殊值、端点、异常、无界和 RandomReset 行为都不允许漂移。
- 如何及早发现：先编写独立标量参考与全长度/形状差分测试；再实现分派；将端点或不安全 lane 整次回退；最后在 Release 下运行全量测试与基准。
- 退回 Spec 澄清的条件：若 Tensor 管线无法同时满足端点逐位语义和有限输入 1 ULP 上限，若创建时缓存需要公开状态/并发契约，若必须改 Algorithms 才能显示收益，或若必须使用手写 intrinsic 才能实现用户指定行为，则停止并退回 Spec。

## ADR 判断

- 是否触发 ADR：否。
- 判断依据：ADR-0013 已明确 `System.Numerics.Tensors` 归 Core、Clamp 与 Tensor 语义对齐、并要求以基准验证 SIMD；本计划没有改变依赖方向、公开模型、边界职责、随机性或计算后端。
- 新 ADR 或被替代 ADR：无。

## 批准记录

- 计划批准：—
- 批准日期：—
