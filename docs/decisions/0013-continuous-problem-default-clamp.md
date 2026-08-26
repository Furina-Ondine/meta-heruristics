# ADR-0013: ContinuousProblem 默认标量 Clamp Repair

## 状态

Accepted

替代 [ADR-0012](0012-repair-owned-candidate-boundaries.md) 中“由 `ContinuousProblem` 构造参数的逐维边界创建默认 Clamp Repair”的规定。

## 背景

ADR-0012 将候选位置边界责任交给 Repair，但 `ContinuousProblem` 仍要求调用方传入逐维 `VariableBounds`，仅为从其推导维度并创建默认 Clamp Repair。这让默认用法把边界配置混入问题定义，并在无需逐维差异边界的常见场景中增加了样板代码。

需要保留安全的默认候选修复，同时使调用方可以仅声明问题维度，或显式组合持有自身边界的 Repair。

## 决策

- `ContinuousProblem` 的构造契约为 `ContinuousProblem(int dimension, IObjectiveFunction objective, ICandidateRepair? repair = null, OptimizationDirection direction = OptimizationDirection.Minimize, IReadOnlyList<IConstraint>? constraints = null)`。
- `dimension` 必须大于零。`ContinuousProblem` 不接受或公开逐维 `VariableBounds`。
- 未提供 Repair 或传入 `null` 时，`ContinuousProblem` 根据 `dimension` 创建所有维度均为 `[0, 10]` 的 `CandidateRepairs.Clamp`。
- 显式传入的 Repair 自己拥有任何边界或恢复数据；`ContinuousProblem` 不检查它的内部维度配置。
- `CandidateRepairs.Clamp`、`Reflect` 和 `RandomReset` 保留逐维 `IReadOnlyList<VariableBounds>` 工厂重载，并增加 `int dimension, double? lowerBound = null, double? upperBound = null` 重载。标量重载将同一 `VariableBounds` 复制到所有维度，支持双侧有界、仅下界、仅上界和无界四种组合。
- 算法仍在每次初始化和修改位置后调用 Repair。Repair 的特殊数值语义、随机性要求及 `DoNothing` 的显式风险语义保持不变。

## 替代方案

- 强制调用方总是传入 Repair：边界责任最显式，但会将最常用的 `[0, 10]` Clamp 配置重复到每个简单问题定义中。
- 保留 `bounds` 构造参数：能支持逐维默认 Clamp，但继续让问题定义承担 Repair 的实现配置。
- 默认使用 `DoNothing`：减少构造开销，但会把未经修复的候选位置风险静默引入现有默认路径。

## 后果

默认问题构造更简洁，仍提供确定且安全的有界位置恢复。需要非默认边界或特殊恢复策略的调用方必须显式提供 Repair。逐维 Repair 继续在 Repair 工厂处配置，而不泄漏到 `ContinuousProblem`。

这是破坏性公共 API 变更：所有现有调用点必须将边界数组改为维度，并在依赖非默认范围时显式构造 Repair。相关 XML 文档、API 文档、用户指南、架构概览、工程规范、示例、基准和契约测试必须同步更新。

## 重新评估条件

当默认边界不再适合连续问题的主要入门场景、引入其他候选表示，或 Core 需要重新验证候选位置安全性时，通过新的 ADR 重新评估本决策。
