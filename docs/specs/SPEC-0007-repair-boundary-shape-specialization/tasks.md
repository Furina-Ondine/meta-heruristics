# SPEC-0007 实施任务

## 执行规则

- 只实施已批准的 [`spec.md`](./spec.md) 与 [`plan.md`](./plan.md) 中已有的行为。
- 一个时刻只能有一个 `InProgress` 任务；发现未批准的行为或设计判断时停止并回到 Spec/Plan。

## T001：替代边界形状 ADR

- 状态：`Completed`
- 覆盖需求：`FR-001`、`NFR-001`、`NFR-004`
- 依赖：Approved Plan
- 工作：新增 ADR-0018，收窄内置 Repair 到 scalar/scalar 与 vector/vector；将 ADR-0013 标记为被替代，并更新 ADR 索引。性能回退决定新增 ADR-0019，替代 ADR-0017/0018 的当前契约。
- 验证结果：ADR-0013、ADR-0017、ADR-0018 与 ADR 索引均已更新。

## T002：迁移公开工厂与同形状存储

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-002`、`FR-003`、`NFR-001`、`NFR-002`、`NFR-003`、`NFR-004`
- 依赖：T001
- 工作：删除混合 factories、`Boundary` 与通用有界 Repair；实现 Clamp、Reflect、RandomReset 的 scalar/scalar 与 vector/vector 密封私有类型，并保留已批准的创建时验证、复制、Position 长度、特殊值和随机消费语义。
- 验证结果：已完成实现与生产代码残留搜索；Release build 与完整自动化测试通过。

## T003：删除同形状 Reflect SIMD 模板

- 状态：`Completed`
- 覆盖需求：`FR-002`、`FR-003`、`FR-004`、`NFR-002`、`NFR-003`、`NFR-004`
- 依赖：T002
- 工作：删除 Reflect 生成模板、Core generator analyzer/AdditionalFiles 接入及 SIMD 数值辅助状态；两种同形状 Repair 回到直接标量循环。
- 验证结果：Core 与 `eng` 的残留搜索确认不存在 Reflect SIMD 关联；Release build 与完整自动化测试通过。

## T004：迁移测试、基准和文档

- 状态：`Completed`
- 覆盖需求：`FR-001`、`FR-003`、`FR-004`、`FR-005`、`NFR-001`、`NFR-002`、`NFR-003`、`NFR-004`
- 依赖：T002、T003
- 工作：删除混合形状的测试和 benchmark enum/分支；补齐保留形状的差分、特殊值、长度、复制和随机测试，更新 Repair 局部与 Bat 端到端基准、架构概览与 API 文档表述。
- 验证结果：混合形状测试/benchmark 参数已删除；架构概览、用户指南、ADR 与 Spec 同步为同形状标量 Reflect。

## T005：性能准入与最终验证

- 状态：`Completed`
- 覆盖需求：`FR-005`、`NFR-001`、`NFR-002`、`NFR-003`、`NFR-004`
- 依赖：T004
- 工作：记录拒绝 SIMD 的 Repair 对照，执行残留搜索、生成器测试及完整 Release build/test；将证据写入 verification 并把 Spec 标记为 Implemented。
- 验证结果：M4 Pro 长测显示 Reflect SIMD 候选无稳定收益，故已删除；生成器也已移除无硬件门的 `__SimdExpandWidths` 及关联能力；Release build 0 warning/0 error，166 项测试通过。
