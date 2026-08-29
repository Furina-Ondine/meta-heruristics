# 架构决策记录

本目录记录重要选择的背景、理由和长期影响。架构概览只描述当前状态；需要了解持续有效的工程契约时，先阅读
[`ENGINEERING.md`](../../ENGINEERING.md)。

## 规则

- ADR 不原地重写历史决策。变更时新增 ADR；旧记录保留原文，并在被替代时标记替代关系。
- 只使用三种状态：`Accepted`（当前采用）、`Superseded`（已由后续 ADR 替代）和 `Rejected`（未采用但保留记录）。
- `Superseded` 的记录必须链接替代它的新 ADR；新 ADR 应说明被替代的记录。`Rejected` 记录保留否决原因。
- 状态为 `Accepted` 的 ADR 解释当前工程契约和架构选择的理由。实现、规范与 ADR 冲突时，先报告冲突，再通过新 ADR 解决。

## ADR 索引

| 编号 | 主题 | 状态 |
| --- | --- | --- |
| [0001](0001-platform-and-toolchain.md) | 平台与工具链 | `Accepted` |
| [0002](0002-library-scope-and-evolution.md) | 库范围与演进顺序 | `Accepted` |
| [0003](0003-project-and-package-boundaries.md) | 项目与包边界 | `Accepted` |
| [0004](0004-composition-and-execution-model.md) | 组件构造与运行模型 | `Superseded` |
| [0005](0005-candidate-objective-and-constraints.md) | 候选、目标值与约束 | `Superseded` |
| [0006](0006-evaluation-performance-and-reproducibility.md) | 评估、性能与可复现性 | `Superseded` |
| [0007](0007-versioning-and-release.md) | 版本与发布 | `Accepted` |
| [0008](0008-experiment-run-groups-and-reusable-workers.md) | Experiment RunGroup 与可复用 Worker | `Superseded` |
| [0009](0009-group-scoped-optimizer-execution.md) | RunGroup 独占的有状态 Optimizer | `Accepted` |
| [0010](0010-scalar-evaluation-baseline.md) | 单点评估基础契约 | `Accepted` |
| [0011](0011-bat-first-algorithm-migration.md) | 首个算法迁移选择蝙蝠算法 | `Accepted` |
| [0012](0012-repair-owned-candidate-boundaries.md) | Repair 拥有候选位置边界 | `Superseded` |
| [0013](0013-tensor-shaped-repair-bounds.md) | Tensor 形状的 Repair 边界 | `Accepted` |
| [0014](0014-spec-driven-change-governance.md) | Spec-Driven 变更治理 | `Accepted` |
| [0015](0015-ordered-extended-evaluation-values.md) | 评估结果使用有序扩展数值域 | `Accepted` |
| [0016](0016-algorithm-fixed-width-simd-cascade.md) | 算法私有固定宽度 SIMD 级联 | `Accepted` |

## ADR 模板

每份 ADR 使用以下固定章节；标题应包含编号和主题，状态变更时保留原决策内容：

```markdown
# ADR-NNNN: 主题

## 状态

Accepted

## 背景

说明需要作出决策的问题和约束。

## 决策

说明采用的方案及其边界。

## 替代方案

列出考虑过但未采用的方案及原因。

## 后果

说明带来的收益、成本和约束。

## 重新评估条件

说明什么变化会触发重新评估，以及替代 ADR 的链接。
```
