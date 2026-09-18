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
- `Implemented`：该版本的需求、清理和验证证据全部完成；不单独表示所有条款今天仍然有效。
- `Superseded`：已由新 Spec 替代，必须链接替代 package。

影响公共行为的未决问题存在时不得进入 `Approved`。`Approved` 后不得静默修改需求；发现规格错误时退回 `Clarifying`，修改并重新批准。

Plan 使用 `Draft`、`Approved`、`Superseded`；Tasks 使用 `Pending`、`InProgress`、`Completed`、`Blocked`。一个 package 同时只能有一个 `InProgress` Task。

## 当前适用性

每份 Spec 另记 `当前适用性`，与执行状态独立：

- `Current`：尚无已批准替代；Draft 的 Current 不表示已批准或可实施。
- `Partial`：部分条款已被替代；在“当前适用范围”逐项列出旧 FR/NFR、发生变化的行为、接替 Spec 条款和仍有效内容。
- `Superseded`：所有规范性内容已有明确接替位置；该文只供历史追溯。执行状态可以保留 Implemented，保留曾完成验收的事实。

现有执行状态 `Superseded` 保持可读，但必须同时标记适用性 Superseded。不要通过回退历史执行状态或改写旧 Verification 表达适用性变化。修改适用性说明不修改原批准的需求正文；新增行为仍须重新审批。

新替代必须同步双方元数据及当前索引，不依赖“编号较新者优先”。一个需求中只有某项约束变化时，写明该约束，不能连带撤销其他数值或生命周期规则。当前任务优先读取 Current 及 Partial 的适用范围；历史实现细节按需追溯。

后续接替再次变化时，同步更新当前入口到有效条款的直接链接，原始替代关系留作历史。背景、导航与摘要避免冗余数量；FR/NFR 的批准对象、非目标、边界与验收数字仍须明确。不能将“一次批准指定算法的变更”改成“所有内置算法”来省略后续审批。

## 风险分类

以下任一变化必须使用完整 SDD：公共 API 或行为、项目职责或依赖、策略与执行抽象、状态与并发、随机性与确定性、数值语义、热路径性能、兼容策略、跨两个以上运行时项目，或要求代理猜测会影响结果的行为。

只有不改变上述契约、已有权威资料明确预期且局限于一个模块的修改，才能采用经用户批准的轻量设计。发现隐藏复杂度时只能升级。

## 批准门

1. Spec 在 `Approved` 前必须解决公共行为问题，并记录批准人和日期。
2. Plan 必须基于 Approved Spec，完成当前实现调查、方案比较、影响矩阵和删除计划，再由用户批准。
3. Tasks 只能落实 Approved Plan。需要新行为或架构选择时停止并返回 Spec/Plan。
4. Spec 只有在 Verification 覆盖所有 `FR/NFR`、替代残留和工程验证后才能进入 `Implemented`。

### 完成前的现状文档核对

按 ENGINEERING 的“现状文档维护”落实 Plan 影响调查、任务内同步及最终核对。Verification 使用模板中的“现状文档核对”表，至少明确核对架构概览，其余受影响入口按需追加；每行链接本地文档，并记录“已更新”及更新章节/变化，或“无需更新”及具体理由。依据实际交付重新判断，不能将预测或空泛结论当作验收证据。

验证器在 Spec 为 Implemented 或 Verification 为 Passed 时检查此表，不要求尚未完成的报告填写最终结论。2026-09-18 引入规则前已经完成的包，按校验脚本中固定的包名与当时 Plan 所记 Spec 基线组合豁免缺失此表；该清单不随新完成的包增长。旧包更换批准基线后重新验收须遵循新规则；自愿补记核对时标明实际核对时间并满足表格要求，不改写历史验收事实。格式校验通过不代表影响判断已经通过语义审查。

## 权威与冲突

`ENGINEERING.md` 和 Accepted ADR 的当前适用条款约束所有 Spec。经批准且当前适用的 Spec 条款是对应功能公共意图和行为的权威来源。实现与 Spec 冲突时不默认以代码为准；先报告冲突，由用户决定修正实现、修订 Spec 或新增替代 ADR。执行完成、当前适用与实测平台范围是三种不同信息，不能互相替代。见 [ADR-0026](../decisions/0026-current-document-applicability.md)。

## 跨 Spec 共同方案

多个 Spec 存在共同目标、依赖或验证协议时，可以按需维护共同方案或附件。沿用现有 Spec 编号、四件套和两次批准门，不要求新增上层编号、固定目录、模板或独立审批阶段。是否需要共同文档取决于协调复杂度，不以 Spec 数量判定。

- **职责与范围**：共同文档说明其用途、关联 Spec、共同目标及必要的依赖；共同验证附件由相关 Plan 明确引用。具体公共行为和职责契约归对应 Spec，长期架构选择归 ADR，持续工程规则归 ENGINEERING；共同文档通过引用组织这些内容，不复制或隐含覆盖权威条款。
- **批准对象**：认可共同方向不自动批准各 Spec 或实现。允许合并审阅和批准，但记录须明确所包含的 Spec、Plan、共同附件及对应内容版本（提交或内容标识；未提交修订须保留可核对的差异）。后续尚未形成的规格不在此次批准范围内。历史记录缺少精确版本时如实说明，不能根据最终实现提交反推当时批准内容。
- **变更影响**：共同内容发生实质修改时，识别受影响的 Spec、Plan 和既有验收，按原有流程修订并重新批准相关部分；受影响工作在相应批准完成前不得按新内容继续，无关工作不必暂停。已完成部分说明哪些证据仍有效、哪些需要补验，不因更新共享文件而静默改变旧验收依据。编辑性修正不触发重新批准。
- **目标与现状**：已批准但未实现的目标、实际完成状态和历史阶段记录分别标明，架构概览随实现与验收更新。涉及现有工程规则或决策的改变时，明确被替代条款、保留范围和生效条件，按既有批准流程处理；共同方案不能自行豁免约束。
- **整体验收**：存在跨 Spec 的集成或整体目标时，指定验收责任归属、标准和证据记录位置，可放在共同文档或指定 Spec 中并相互链接。各 Spec 完成不自动证明整体目标完成；共同入口汇总结论和缺口，不复制各包任务清单或全部验证报告。

## SIMD 演进

[SPEC-0010 至 SPEC-0014 联合规格](simd-review.md)已全部实施。SPEC-0011 至 SPEC-0014 于 2026-09-17 完成批量采样、私有 SIMD、候选清理以及局部、Vector128、H/A/B/C 完整 run 验收；入口见[联合审查](simd-review.md)，共同验收设计见[验证计划附件](simd-plan.md)，详细数据见各 Verification。

## 当前 package

Current 先读当前条款；Partial 先读包内适用范围，再定位仍有效需求。执行状态只报告该版本的实施进度。

| 编号 | 主题 | 当前适用性 | 状态 |
| --- | --- | --- | --- |
| [SPEC-0001](./SPEC-0001-evaluation-special-values/spec.md) | 评估结果的特殊数值语义 | `Current` | `Implemented` |
| [SPEC-0002](./SPEC-0002-continuous-algorithm-migration/spec.md) | 连续集中式算法迁移 | `Partial` | `Implemented` |
| [SPEC-0003](./SPEC-0003-simd-repairs/spec.md) | SIMD 内置 Repair | `Partial` | `Implemented` |
| [SPEC-0005](./SPEC-0005-algorithm-private-simd/spec.md) | 算法私有 SIMD 演进 | `Partial` | `Implemented` |
| [SPEC-0006](./SPEC-0006-zero-overhead-simd-cascade/spec.md) | 零开销 SIMD 级联源码生成 | `Partial` | `Implemented` |
| [SPEC-0007](./SPEC-0007-repair-boundary-shape-specialization/spec.md) | Repair 边界形状专用化 | `Current` | `Implemented` |
| [SPEC-0009](./SPEC-0009-high-performance-random-sampling/spec.md) | 高性能随机源与批量分布采样 | `Partial` | `Implemented` |
| [SPEC-0010](./SPEC-0010-simd-random-sampling/spec.md) | 随机采样 SIMD 化 | `Current` | `Implemented` |
| [SPEC-0011](./SPEC-0011-bat-batched-simd/spec.md) | Bat 批量采样与 SIMD | `Current` | `Implemented` |
| [SPEC-0012](./SPEC-0012-cuckoo-batched-simd/spec.md) | Cuckoo 批量采样与 SIMD | `Current` | `Implemented` |
| [SPEC-0013](./SPEC-0013-pso-simd-refinement/spec.md) | PSO SIMD 增量优化 | `Current` | `Implemented` |
| [SPEC-0014](./SPEC-0014-firefly-simd-refinement/spec.md) | Firefly SIMD 增量优化 | `Current` | `Implemented` |

## 历史 package

以下包保留历史验收事实，不作为现行实现要求。

| 编号 | 主题 | 当前适用性 | 状态 |
| --- | --- | --- | --- |
| [SPEC-0004](./SPEC-0004-masked-simd-reflect/spec.md) | 掩码 SIMD Reflect Repair | `Superseded` | `Implemented` |
