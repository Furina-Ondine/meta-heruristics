# 架构决策记录

本目录记录重要选择的背景、理由和长期影响。架构概览只描述当前状态；需要了解持续有效的工程契约时，先阅读
[`ENGINEERING.md`](../../ENGINEERING.md)。

## 规则

- 不改变含义的错字、链接和表达可以原地修正；不改变决策边界的解释或证据可以带日期补充。改变允许/禁止行为、责任、约束或选择时新增 ADR，保留原决策正文。判定依据是语义，不是修改字数。
- 每份新 ADR 尽量只处理一个可以独立变化的决策；操作安排放在 ENGINEERING 或 Plan，不与长期架构选择捆绑。仅因局部性能实现需要完整 SDD，不自动要求新增 ADR。
- 只使用三种状态：`Accepted`（当前采用）、`Superseded`（已由后续 ADR 替代）和 `Rejected`（未采用但保留记录）。
- `Superseded` 表示全文不再作为现行决策入口，必须链接接替记录；新记录列出撤销范围、保留内容和其当前权威位置，不能只写“其余沿用旧 ADR”。部分替代的旧记录保持 Accepted，并在状态区显著标明失效条款及接替位置。Rejected 记录保留否决原因。
- 状态为 `Accepted` 的 ADR 解释当前工程契约和架构选择的理由。实现、规范与 ADR 冲突时，先报告冲突，再通过新 ADR 解决。
- 日常任务先按下面的主题入口读取当前资料，再读相关 Accepted 决策；历史记录只在追溯原因时加载。Accepted 不代表已经实现，也不能跳过其适用范围说明。见 [ADR-0026](0026-current-document-applicability.md)。
- 历史替代链不设长度上限；当前主题入口直接定位有效条款，不要求读者从最早 ADR 逐篇合并决策。多次部分替代使入口难以理解时，只整理适用范围与直接链接；新的决策仍按语义变化记录，不能为缩短链而抹除历史。
- 背景与摘要遵循 ENGINEERING 的稳定表述原则，省去冗余数量；决策明确限定的集合、边界和当时取舍仍须精确，不以开放措辞隐含批准未来扩展。

## 当前主题入口

| 主题 | 当前契约或规格 | 当前决策理由 |
| --- | --- | --- |
| 项目定位、依赖和版本 | [ENGINEERING](../../ENGINEERING.md)、[系统概览](../architecture/overview.md) | ADR-0001/0002/0003/0007 |
| run/Group、状态所有权和随机性 | [ENGINEERING](../../ENGINEERING.md)、[SPEC-0010](../specs/SPEC-0010-simd-random-sampling/spec.md) | [ADR-0024](0024-adaptive-vector-random-api.md)；旧 RunGroup 决策仅供追溯 |
| 单点评估和特殊数值 | [SPEC-0001](../specs/SPEC-0001-evaluation-special-values/spec.md)、[开发者手册](../architecture/developer-guide.md) | ADR-0010 的适用范围、ADR-0015 |
| Repair 边界和标量 Reflect | [SPEC-0007](../specs/SPEC-0007-repair-boundary-shape-specialization/spec.md) | ADR-0019 |
| 算法 SIMD | [SIMD 联合规格](../specs/simd-review.md) | ADR-0025 |
| 变更流程和文档有效性 | [ENGINEERING](../../ENGINEERING.md)、[规格规则](../specs/README.md) | ADR-0014 的保留部分、ADR-0026 |

## 当前 ADR

| 编号 | 主题 | 状态 |
| --- | --- | --- |
| [0001](0001-platform-and-toolchain.md) | 平台与工具链 | `Accepted` |
| [0002](0002-library-scope-and-evolution.md) | 库范围与演进顺序 | `Accepted` |
| [0003](0003-project-and-package-boundaries.md) | 项目与包边界 | `Accepted` |
| [0007](0007-versioning-and-release.md) | 版本与发布 | `Accepted` |
| [0010](0010-scalar-evaluation-baseline.md) | 单点评估基础契约 | `Accepted` |
| [0011](0011-bat-first-algorithm-migration.md) | 首个算法迁移选择蝙蝠算法 | `Accepted` |
| [0014](0014-spec-driven-change-governance.md) | Spec-Driven 变更治理 | `Accepted` |
| [0015](0015-ordered-extended-evaluation-values.md) | 评估结果使用有序扩展数值域 | `Accepted` |
| [0019](0019-scalar-reflect-and-algorithm-only-simd-generation.md) | 标量 Reflect 与仅 Algorithms 使用 SIMD 生成 | `Accepted` |
| [0024](0024-adaptive-vector-random-api.md) | 自适应 Vector 随机状态与向量采样 API | `Accepted` |
| [0025](0025-direct-tensor-primitives-and-benchmark-execution.md) | 直接 TensorPrimitives 优先与算法基准执行 | `Accepted` |
| [0026](0026-current-document-applicability.md) | 文档执行历史与当前适用性分离 | `Accepted` |

## 历史 ADR

以下记录用于追溯，不作为当前任务的默认规则入口。

| 编号 | 主题 | 状态 |
| --- | --- | --- |
| [0004](0004-composition-and-execution-model.md) | 组件构造与运行模型 | `Superseded` |
| [0005](0005-candidate-objective-and-constraints.md) | 候选、目标值与约束 | `Superseded` |
| [0006](0006-evaluation-performance-and-reproducibility.md) | 评估、性能与可复现性 | `Superseded` |
| [0008](0008-experiment-run-groups-and-reusable-workers.md) | Experiment RunGroup 与可复用 Worker | `Superseded` |
| [0009](0009-group-scoped-optimizer-execution.md) | RunGroup 独占的有状态 Optimizer | `Superseded` |
| [0012](0012-repair-owned-candidate-boundaries.md) | Repair 拥有候选位置边界 | `Superseded` |
| [0013](0013-tensor-shaped-repair-bounds.md) | Tensor 形状的 Repair 边界 | `Superseded` |
| [0016](0016-algorithm-fixed-width-simd-cascade.md) | 算法私有固定宽度 SIMD 级联 | `Superseded` |
| [0017](0017-repository-private-simd-source-generation.md) | 仓库私有 SIMD 增量源码生成 | `Superseded` |
| [0018](0018-repair-boundary-shape-specialization.md) | Repair 边界形状专用化 | `Superseded` |
| [0020](0020-core-owned-random-source-and-run-execution.md) | Core 拥有的封闭随机源与 RunGroup 执行 | `Superseded` |
| [0021](0021-run-private-simd-random-lanes.md) | run 私有 SIMD 随机 lane 与封闭执行契约 | `Superseded` |
| [0022](0022-batched-algorithm-simd-fusion.md) | 批量采样下的算法私有 SIMD 与融合 | `Superseded` |
| [0023](0023-separate-scalar-and-batch-random-state.md) | 分离单值与批量随机状态 | `Superseded` |

## ADR 模板

每份 ADR 使用以下固定章节；标题应包含编号和主题，状态变更时保留原决策内容：

```markdown
# ADR-NNNN: 主题

## 状态

Accepted

批准日期与依据。发生部分替代时，显著列出被替代条款、接替位置及仍有效范围；此处不以历史实施进度代替当前状态。

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
