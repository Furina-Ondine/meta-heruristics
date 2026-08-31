# 功能规格

本目录保存 Metaheuristics.NET 新的权威功能规格。跨功能持续工程契约见 [`ENGINEERING.md`](../../ENGINEERING.md)，长期或跨功能决策理由见 [ADR](../decisions/README.md)，当前实现状态见[架构概览](../architecture/overview.md)。旧设计和实施过程保留在 [`docs/superpowers/`](../superpowers/README.md)，不迁入本目录，也不取得权威规格状态。

## Change package

每项完整 SDD 变更使用一个目录：

```text
SPEC-NNNN-kebab-case/
├─ spec.md
├─ plan.md
├─ tasks.md
└─ verification.md
```

- `spec.md` 定义 why 与 what：公共意图、行为、责任、非目标和成功标准。
- `plan.md` 定义 how：当前实现调查、方案、影响、删除范围和验证设计。
- `tasks.md` 把已批准方案拆为引用需求编号的执行步骤，不引入新设计。
- `verification.md` 记录需求到实现、测试、基准和文档的最终证据。

从 [`_templates/spec.md`](./_templates/spec.md) 开始创建新 package，并使用同目录的 Plan、Tasks 和 Verification 模板。编号在本目录内递增且不得复用；目录主题使用小写 kebab-case。

## 状态

Spec 只使用以下状态：

```text
Draft → Clarifying → Approved → Implementing
      → Verifying → Implemented → Superseded
```

- `Draft`：初稿，尚未进入系统性澄清。
- `Clarifying`：仍有问题需要用户决定。
- `Approved`：公共行为、边界和风险已经用户批准，可以制定或执行 Plan。
- `Implementing`：按 Approved Plan 和 Tasks 实施。
- `Verifying`：实现完成，正在收集追踪和工程证据。
- `Implemented`：需求、清理和验证证据全部完成。
- `Superseded`：已由新 Spec 替代，必须链接替代 package。

影响公共行为的未决问题存在时不得进入 `Approved`。`Approved` 后不得静默修改需求；发现规格错误时退回 `Clarifying`，修改并重新批准。

Plan 使用 `Draft`、`Approved`、`Superseded`；Tasks 使用 `Pending`、`InProgress`、`Completed`、`Blocked`。一个 package 同时只能有一个 `InProgress` Task。

## 风险分类

以下任一变化必须使用完整 SDD：公共 API 或行为、项目职责或依赖、策略与执行抽象、状态与并发、随机性与确定性、数值语义、热路径性能、兼容策略、跨两个以上运行时项目，或要求代理猜测会影响结果的行为。

只有不改变上述契约、已有权威资料明确预期且局限于一个模块的修改，才能采用经用户批准的轻量设计。发现隐藏复杂度时只能升级。

## 批准门

1. Spec 在 `Approved` 前必须解决公共行为问题，并记录批准人和日期。
2. Plan 必须基于 Approved Spec，完成当前实现调查、方案比较、影响矩阵和删除计划，再由用户批准。
3. Tasks 只能落实 Approved Plan。需要新行为或架构选择时停止并返回 Spec/Plan。
4. Spec 只有在 Verification 覆盖所有 `FR/NFR`、替代残留和工程验证后才能进入 `Implemented`。

## 权威与冲突

`ENGINEERING.md` 和 Accepted ADR 约束所有 Spec。Approved/Implemented Spec 是对应功能公共意图和行为的权威来源。实现与 Spec 冲突时不默认以代码为准；先报告冲突，由用户决定修正实现、修订 Spec 或新增替代 ADR。

## 当前 package

| 编号 | 主题 | 状态 |
| --- | --- | --- |
| [SPEC-0001](./SPEC-0001-evaluation-special-values/spec.md) | 评估结果的特殊数值语义 | `Implemented` |
| [SPEC-0002](./SPEC-0002-continuous-algorithm-migration/spec.md) | 连续集中式算法迁移 | `Implemented` |
| [SPEC-0003](./SPEC-0003-simd-repairs/spec.md) | SIMD 内置 Repair | `Implemented` |
| [SPEC-0004](./SPEC-0004-masked-simd-reflect/spec.md) | 掩码 SIMD Reflect Repair | `Implemented` |
| [SPEC-0005](./SPEC-0005-algorithm-private-simd/spec.md) | 算法私有 SIMD 演进 | `Implemented` |
| [SPEC-0006](./SPEC-0006-zero-overhead-simd-cascade/spec.md) | 零开销 SIMD 级联源码生成 | `Implemented` |
| [SPEC-0007](./SPEC-0007-repair-boundary-shape-specialization/spec.md) | Repair 边界形状专用化 | `Approved` |
