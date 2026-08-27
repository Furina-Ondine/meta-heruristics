# SPEC-0001：评估结果的特殊数值语义

## 元数据

- 编号：`SPEC-0001`
- 状态：`Clarifying`
- 创建日期：2026-08-27
- 批准人：—
- 批准日期：—
- 替代：无
- 被替代：无
- 相关 ADR：[ADR-0012](../../decisions/0012-repair-owned-candidate-boundaries.md)（其“目标值和约束违背量必须有限”规则需要被新 ADR 替代）

## 问题与动机

当前 `ContinuousProblem`、`Evaluation` 和 `ConstraintEvaluation` 把所有非有限评估结果视为无效。这把 `NaN` 与有顺序意义的正负无穷混为一谈：`NaN` 无法形成可靠顺序，应该立即失败；正负无穷则可表达无界目标或不可逾越的约束违背，并能由 `double.CompareTo` 和优化方向确定顺序。

如果只放宽构造器校验，影响会传播到目标停止条件、Experiment 的均值/中位数/标准差以及多约束违背量聚合。尤其 `+Infinity - +Infinity` 和 `-Infinity + +Infinity` 会产生 `NaN`。因此本变更必须先定义整条评估消费链的语义，不能只改一个 `IsFinite`。

## 目标

- 对目标值和约束违背量分别定义 `NaN`、`-Infinity`、有限值和 `+Infinity` 的公共行为。
- 保持可行性优先、违背量优先和优化方向比较规则在特殊值下仍有确定结果。
- 让停止条件、Experiment 统计和 XML/API 文档与新的评估域一致。

## 非目标

- 不验证或修复候选位置中的 `NaN`/Infinity；位置仍由 Initializer、Repair 和调用方负责。
- 不引入泛型目标值、多目标优化或新的约束权重模型。
- 不改变内置 Repair 对特殊位置值的现有语义。
- 本 Clarifying 版本不授权任何运行时代码修改。

## 架构契合

该规则只验证 Core 自身必须依赖的评估结果不变量：拒绝不能可靠比较的 `NaN`，接受有明确顺序的 Infinity。验证发生在评估边界，不扫描候选位置，也不在算法中重复。由于它替代 ADR-0012 的持续规则，实施前必须新增替代 ADR。

## 信任与责任边界

| 数据或行为 | 责任方 | Core 是否验证 | 违反契约的结果 |
| --- | --- | --- | --- |
| 候选位置分量 | Initializer、Repair、调用方 | 否 | 结果由所选策略及目标函数负责 |
| 目标函数结果 | Objective | 是，仅拒绝 `NaN` | `InvalidOperationException` |
| 直接构造的 `Evaluation.Objective` | 调用方 | 是，仅拒绝 `NaN` | `ArgumentOutOfRangeException` |
| 单项约束违背量 | Constraint | 是，拒绝 `NaN`、负有限值和 `-Infinity` | `InvalidOperationException` |
| 直接构造的约束汇总 | 调用方 | 是，验证数值域和汇总关系 | `ArgumentOutOfRangeException` 或 `ArgumentException` |
| Infinity 的下游统计和停止语义 | Core / Experiments | 是 | 尚待下文问题决定 |

## 功能需求

### FR-001: 目标值拒绝 NaN

- 前置条件：Objective 返回 `NaN`，或调用方直接构造包含 `NaN` 的 `Evaluation`。
- 触发行为：问题评估或值对象构造。
- 预期结果：立即抛出当前边界对应的明确异常；`NaN` 不进入比较、停止、轨迹或统计。
- 边界情况：候选位置本身包含 `NaN` 不触发 Core 的位置验证。
- 验收标准：问题评估与直接构造均有自动化测试，异常文档与实现一致。

### FR-002: 目标 Infinity 参与确定性排序

- 前置条件：Objective 或直接构造的 `Evaluation` 给出 `-Infinity` 或 `+Infinity`。
- 触发行为：评估并比较两个候选。
- 预期结果：接受该值；可行性与违背量相同时，最小化按 `-Infinity < finite < +Infinity`，最大化顺序相反。
- 边界情况：两个相同 Infinity 比较为等价；可行候选仍优于任何不可行候选。
- 验收标准：两个方向、可行/不可行和相同 Infinity 均有比较测试。

### FR-003: 约束违背量只接受非负有序值

- 前置条件：Constraint 返回或调用方构造 `NaN`、`-Infinity`、负有限值、非负有限值或 `+Infinity`。
- 触发行为：问题评估或 `ConstraintEvaluation` 构造。
- 预期结果：拒绝 `NaN`、`-Infinity` 和负有限值；接受零、正有限值和 `+Infinity`。`+Infinity` 表示无界的不可行程度。
- 边界情况：零仍表示该约束没有违背；`+Infinity` 必须计入 `ViolatedCount`。
- 验收标准：每种数值类别均有问题评估和适用的值对象测试。

### FR-004: 约束汇总保持确定性

- 前置条件：一个或多个约束产生正有限值或 `+Infinity`。
- 触发行为：计算 `TotalViolation`、`MaxViolation` 和 `ViolatedCount`。
- 预期结果：有限和不溢出时保持现有结果；含 `+Infinity` 时汇总能够表达无界违背且不产生 `NaN`。
- 边界情况：有限正值求和溢出到 `+Infinity` 时是否视同合法无界违背，尚待批准。
- 验收标准：混合有限值与 `+Infinity`、多个 `+Infinity` 及有限和溢出均有明确测试。

### FR-005: 所有评估消费者定义 Infinity 行为

- 前置条件：最佳目标值为 Infinity，或同一 Experiment 的成功 run 包含 Infinity。
- 触发行为：目标停止判断、轨迹/汇总生成和 Experiment 统计。
- 预期结果：任何路径都不得意外生成或静默传播未定义的 `NaN`；具体统计表示和 Infinity 目标阈值规则尚待批准。
- 边界情况：全为同号 Infinity、有限值与单侧 Infinity、同时含正负 Infinity、偶数样本中位数跨越正负 Infinity。
- 验收标准：每个已批准组合都有自动化测试和 API 文档。

## 非功能需求

### NFR-001: 热路径验证成本不增加扫描

- 测量方式：代码审查确认每个 Objective/Constraint 结果只做常数次标量分类，且不恢复候选位置扫描；如实现引入额外集合遍历则补充 BenchmarkDotNet。
- 可接受阈值：评估边界不新增候选维度相关扫描，常见有限值路径不分配托管对象。
- 证据类型：实现审查、测试；触发额外遍历时使用 BenchmarkDotNet 或分配分析。

## 职责与替代关系

- 新增的概念：评估结果的“有序扩展实数”域；约束域只取其中非负部分。
- 被替代的概念：ADR-0012 和现有 XML 文档中的“目标值与约束违背量必须有限”。
- 必须删除的旧行为或公共入口：所有无差别拒绝 Infinity 的评估边界校验；不新增兼容开关。
- 明确保留的旧概念及独立理由：候选位置的特殊值仍归策略负责；Repair 规则与本 Spec 无关。
- 完成后每个概念的唯一所属层：Core 定义评估域与比较/停止语义；Experiments 只负责已定义评估域上的跨 run 统计。

## 成功标准

- 用户能从一个规则表预测任意目标值或约束违背量的接受、异常和排序结果。
- Core、Experiments、测试、XML Reference 与手写指南不再同时存在“必须有限”和“允许 Infinity”的冲突。
- 特殊值不会因算术组合在下游意外变成未处理的 `NaN`。

## 假设与待澄清问题

1. Experiment 统计遇到 Infinity 时如何表示均值、中位数和标准差？需要在“扩展现有字段语义”“仅统计有限样本并另报 Infinity 数量”“调整 API 表示未定义统计”等方案中选择。
2. `StoppingConditions.TargetObjective` 是否应像目标值一样只拒绝 `NaN`、接受正负 Infinity？接受时，比较结果将直接遵循优化方向和 IEEE 顺序。
3. 多个有限正违背量求和溢出为 `+Infinity` 时，是否应接受为合法的无界总违背，而不是抛出溢出异常？

上述问题影响公共行为，解决前状态保持 `Clarifying`。

## 批准记录

- 规格批准：—
- 批准日期：—
- 批准时明确接受的风险：—
