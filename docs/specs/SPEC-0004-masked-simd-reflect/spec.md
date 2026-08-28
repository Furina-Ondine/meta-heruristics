# SPEC-0004：掩码 SIMD Reflect Repair

## 元数据

- 编号：`SPEC-0004`
- 状态：`Approved`
- 创建日期：2026-08-28
- 批准人：项目作者
- 批准日期：2026-08-28
- 替代：`SPEC-0003` 的 FR-003 内部 Reflect 分派规则（整段安全预扫描与整段标量回退）
- 被替代：无
- 相关 ADR：[ADR-0010](../../decisions/0010-scalar-evaluation-baseline.md)、[ADR-0013](../../decisions/0013-tensor-shaped-repair-bounds.md)、[ADR-0014](../../decisions/0014-spec-driven-change-governance.md)

## 问题与动机

`SPEC-0003` 使 Reflect 的一整段有限、安全输入走 `TensorPrimitives` 管线，但先以标量循环验证每个分量。任一分量为 `NaN`、无穷、端点或具有危险中间值时，整段都会回退到标量参考实现。因此该预扫描本身是热路径的额外 O(n) 工作，且一个异常 lane 会取消其余正常 lane 的 SIMD 机会。

目标是在不改变 Repair API、边界所有权、随机性或既有数值语义的前提下，将条件检查和结果选择推进到 SIMD lane 内。正常 lane 使用 Reflect 计算；异常 lane 保留当前标量 `Reflect` 的可观察结果，而不再拖累整段。

## 目标

- 对支持硬件 SIMD 的运行时，让 Reflect 在单次调用中按 lane 选择 Reflect 或既有退化结果，不进行整段标量安全预扫描。
- 按 Position 的剩余长度自适应选择运行时可用的最大向量宽度，再逐级缩窄处理尾部；不能因长度小于可用 `Vector512<double>` 的 8 个 lane 而直接整段退回标量。
- 将可在创建时确定的边界派生数据继续前置；调用时不分配临时数组或保存跨调用缓冲区。
- 保持公开 API、四种边界形状、异常、特殊值、端点、确定性和 RandomReset 行为不变。
- 在定义的全有限主要负载上，以 BenchmarkDotNet 证明新内核快于当前整段分派实现；若无法证明，则不替换现有实现。

## 非目标

- 不修改 `ICandidateRepair`、`CandidateRepairs` 工厂、`ContinuousProblem`、算法公式、随机数消费或 Problem 的边界职责。
- 不向 RandomReset 或 DoNothing 引入 SIMD。
- 不新增公开 SIMD 开关、后端接口、`VectorOps` 帮助层、批量 Repair API 或共享临时缓冲区。
- 不承诺不支持硬件 SIMD 的运行时、极短尾部或所有边界形状均获得加速。

## 架构契合

Core 继续唯一拥有内置 Repair、边界副本、派生 width/period 和内部 SIMD 实现；Algorithms 继续只调用 `OptimizationRunContext.Repair`。`System.Runtime.Intrinsics` 是目标框架自带的内部实现设施，不新增包或项目依赖。`System.Numerics.Tensors` 继续仅用于 Clamp。

边界的有效性仍只在 Repair 创建时验证；每次调用仍只保留既有的位置长度验证。掩码是 Reflect 计算本身所需的内部数值分类，不是 Core 对 Problem 或算法的重复验证。内核只读调用方独占的 Position 并原位写回，不保存运行状态、随机状态或缓冲区，因而不改变 RunGroup 隔离和可复现性。

ADR-0013 没有规定 Reflect 的具体 SIMD 后端；该 ADR 已将 Repair 优化归属 Core。本规格不改变长期架构决策，因此不新增或替代 ADR。

## 信任与责任边界

| 数据或行为 | 责任方 | Core 是否验证 | 违反契约的结果 |
| --- | --- | --- | --- |
| 边界 NaN、顺序与向量长度 | 调用方通过 Repair 工厂提供 | 创建时一次 | 保持既有构造异常 |
| Position 与向量边界长度 | 调用方 | 每次 Repair 一次 | 保持既有 `ArgumentException` |
| Position 的特殊值 | 调用方与所选 Repair | Reflect 按本规格逐 lane 处理 | 保留当前 Reflect/Clamp 语义 |
| SIMD 硬件可用性 | 运行时 | 内核选择可用宽度，否则标量参考 | 功能语义不降级 |
| 性能是否足以替换旧路径 | Core 维护者 | BenchmarkDotNet | 未达到门槛时不合入替换 |

## 功能需求

### FR-001：按 lane 的 Reflect 分派

- 前置条件：调用方通过任一 `CandidateRepairs.Reflect` 工厂创建 Repair，且 Position 长度有效。
- 触发行为：调用 `Repair(Span<double>, Random)`。
- 预期结果：在可用 SIMD 宽度的每个向量块中，内核从原始 lane 值计算 Clamp 候选、Reflect 候选和安全掩码，并通过向量条件选择写回结果；不得先扫描整段 Position 再决定全段路径。
- 安全掩码：必须表达当前标量 Reflect 的分支条件，包括位置和端点的有限性、正且有限的 width/period、有限 offset，以及严格位于端点之外的数值条件。端点精确性必须单独保护。
- 边界情况：一个异常 lane 只影响自身的输出选择，不得导致同一向量块或整段其它正常 lane 进入标量循环。
- 验收标准：实现审查确认没有 `CanUseTensorPath` 式整段标量预扫描；差分测试证明混合正常/异常 Position 中的正常 lane 仍走 SIMD 内核。

### FR-002：数值与特殊值兼容

- 前置条件：任意有效边界形状和任意 Position 分量。
- 触发行为：执行掩码 SIMD Reflect。
- 预期结果：`NaN` 保持 `NaN`；无穷 Position、单侧或双侧无界端点、零宽或溢出的 width/period、非有限 offset 等输入保留当前标量 `Reflect` 的结果。严格位于区间内的有限值与精确等于上下端点的值保持当前结果。
- 数值兼容性：普通有限、可反射 lane 相对独立标量参考允许最多 1 ULP 差异。所有特殊值类别、端点、异常、长度检查及任何不能安全以向量表达的 lane 都必须与参考逐位一致；后者可以逐 lane 使用私有标量参考，不能触发整段回退。
- 验收标准：四种边界形状与长度 2、7、8、31、32、33、127、128、129、1024 的差分测试覆盖正常、混合、特殊值、端点和溢出输入。

### FR-003：创建时派生与无分配

- 前置条件：构造任一内置 Reflect Repair。
- 触发行为：Repair 创建与后续调用。
- 预期结果：width、period 及任何只依赖边界的安全分类只在创建时计算并私有保存；每次 Repair 不分配托管数组，也不缓存调用者 Position 或 run 级状态。
- 边界情况：标量和三个向量边界形状均保持创建时复制、验证与调用时长度验证的既有规则。
- 验收标准：MemoryDiagnoser 与实现审查确认正常调用没有 Repair 级托管分配；不存在共享/池化的可变缓冲区。

### FR-004：硬件与尾部回退

- 前置条件：运行时硬件 SIMD 宽度不同，或 Position 长度不是任一向量宽度的整数倍。
- 触发行为：执行 Reflect。
- 预期结果：内核从运行时可用的最大宽度开始处理完整块，并对剩余元素依次尝试 `Vector256<double>` 和 `Vector128<double>`；只有不足两个 `double` 的最终尾部可使用私有标量参考。硬件不支持任何 SIMD 宽度时整段使用该参考。
- 边界情况：在支持 `Vector512<double>` 的机器上，长度 7 必须依次以 4-lane、2-lane 和 1-lane 尾部处理，不能因不足 8 lane 直接进入 7 次标量循环；长度 2 至 7 也必须在可用的最窄 SIMD 宽度上处理完整子块。尾部结果逐位符合参考。
- 验收标准：测试与基准包含长度 2、7、8 及相邻长度；实现审查确认宽度选择为 `512 → 256 → 128 → 最终单元素` 的分层策略，而不是“低于最大宽度即整段标量”。

### FR-005：性能替换门槛

- 前置条件：Release 构建可在目标机器运行 BenchmarkDotNet。
- 触发行为：比较当前整段 Tensor 分派实现与候选掩码 SIMD 内核。
- 预期结果：在全有限、可反射 Position 的标量/标量和向量/向量边界形状、长度 32 与 128 上，候选内核的平均执行时间均低于当前实现；所有四种边界形状及长度 2、7、8、31、32、33、127、128、129、1024 均须记录为诊断数据，以确认自适应宽度没有把小长度无谓降级为整段标量。
- 边界情况：短长度、尾部和无 SIMD 硬件可以无收益；不得把目标机器的结果推广为通用性能承诺。
- 验收标准：若任一四个主要比较未改善，停止实现并保留当前整段分派；若均改善，记录完整结果、分配与运行环境到 Verification。

## 非功能需求

### NFR-001：无公开契约漂移

- 测量方式：完整 Core/Algorithms 测试、差分测试、API 审查及随机流审查。
- 可接受阈值：不改变任何公开签名、算法调用链、边界所有权、RandomReset 的随机数调用顺序或 DoNothing 行为；仅 FR-002 明确允许的有限 Reflect 末位差异例外。
- 证据类型：自动化测试、`git diff` 审查与 Release build。

### NFR-002：受限的实现复杂度

- 测量方式：实现和残留搜索审查。
- 可接受阈值：仅在 `CandidateRepairs` 的私有 Reflect 实现中使用 intrinsic；不得新增公开类型、跨项目依赖、运行时配置或第二个边界模型。
- 证据类型：代码审查、`rg` 残留搜索和项目引用审查。

## 职责与替代关系

- 新增的概念：Core 私有的逐 lane intrinsic 内核与边界派生安全数据。
- 被替代的概念：Reflect 的整段标量安全预扫描和“任一 lane 不安全即整段标量回退”的分派。
- 必须删除的旧行为或公共入口：删除上述内部整段分派；不保留开关或兼容壳。
- 明确保留的旧概念及独立理由：私有标量 `Reflect` 是无 SIMD 硬件、标量尾部及不可安全向量表达 lane 的权威参考；Clamp、RandomReset 与 DoNothing 各承担独立语义。
- 完成后每个概念的唯一所属层：Core 拥有 intrinsic 内核、参考实现和边界数据；Algorithms 仅通过 Context 调用；Tests 定义兼容性；Benchmarks 记录性能证据。

## 成功标准

- 一个异常分量不再取消同段其它正常分量的 SIMD Reflect。
- 所有既有调用方无需迁移，特殊值和端点仍符合现有可观察规则。
- 主要全有限 Reflect 负载的四个预先定义比较均快于当前实现，且无 Repair 级分配。
- 若性能门槛未达成，仓库保留当前已验证的实现而不是为了“全 SIMD”引入更慢的代码。

## 假设与待澄清问题

- 已确认：目标是消除整段预扫描和整段回退，而不是要求不足一个硬件向量宽度的尾部也使用 SIMD。
- 已确认：长度适配采用可用向量宽度的逐级缩窄；例如 512 位机器上的长度 7 必须使用 4-lane 与 2-lane SIMD 子块，只允许最后 1 个元素标量处理。
- 已确认：可以使用 `System.Runtime.Intrinsics` 的私有掩码内核；Clamp 保持 `TensorPrimitives` 实现。
- 已确认：候选实现必须在定义的主要有限负载上快于当前实现，否则不替换。
- 已确认：本规格只替代 `SPEC-0003` 的内部 Reflect 分派细节；Clamp、公开 API、特殊值和数值兼容边界仍以 `SPEC-0003` 为共同约束。

## 批准记录

- 规格批准：项目作者
- 批准日期：2026-08-28
- 批准时明确接受的风险：掩码内核可能在有限可反射 lane 上带来最多 1 ULP 的末位差异；只有目标机器基准能证明性能改善，不能外推为所有硬件或输入均加速。
