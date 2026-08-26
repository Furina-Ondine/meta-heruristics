# ContinuousProblem 默认 Clamp Repair 设计

## 目标

移除 `ContinuousProblem` 对逐维 `VariableBounds` 的构造依赖，保留其维度信息，并在未显式提供 Repair 时为每个维度应用 `[0, 10]` Clamp。允许调用方通过 Repair 工厂继续表达逐维或统一标量边界。

## 公共契约

`ContinuousProblem` 使用以下构造函数：

```csharp
ContinuousProblem(
    int dimension,
    IObjectiveFunction objective,
    ICandidateRepair? repair = null,
    OptimizationDirection direction = OptimizationDirection.Minimize,
    IReadOnlyList<IConstraint>? constraints = null)
```

- `dimension <= 0` 抛出 `ArgumentOutOfRangeException`。
- `objective` 为 `null` 抛出 `ArgumentNullException`。
- `repair` 为 `null` 时，问题创建与 `CandidateRepairs.Clamp(dimension, 0, 10)` 等价的 Repair。
- 显式 Repair 不会被 Problem 包装、复制或根据 `dimension` 预验证；Repair 在被算法调用时按自己的契约检查 Position 长度。
- 目标方向和约束的验证与防御性复制行为不变。

## Repair 工厂

保留现有逐维边界重载：

```csharp
CandidateRepairs.Clamp(IReadOnlyList<VariableBounds> bounds)
CandidateRepairs.Reflect(IReadOnlyList<VariableBounds> bounds)
CandidateRepairs.RandomReset(IReadOnlyList<VariableBounds> bounds)
```

为三个有界 Repair 增加统一标量边界重载：

```csharp
CandidateRepairs.Clamp(int dimension, double? lowerBound = null, double? upperBound = null)
CandidateRepairs.Reflect(int dimension, double? lowerBound = null, double? upperBound = null)
CandidateRepairs.RandomReset(int dimension, double? lowerBound = null, double? upperBound = null)
```

这些重载验证 `dimension > 0`，构造一次 `VariableBounds(lowerBound, upperBound)`，再为每个维度复制该不可变值。它们覆盖双侧有界、单侧下界、单侧上界和无界。无界配置仍按每种现有 Repair 的文档化退化语义处理。

## 调用点迁移

- 将所有 `new ContinuousProblem(bounds, objective, ...)` 改为 `new ContinuousProblem(bounds.Count, objective, repair: CandidateRepairs.Clamp(bounds), ...)`，但只有原调用依赖非 `[0, 10]` 边界时才传入自定义 Clamp。
- 原本使用无界边界且不希望默认 Clamp 的调用点改为显式 `CandidateRepairs.DoNothing`。
- 仅需默认 `[0, 10]` Clamp 的调用点只传入维度。
- 示例和基准保留其展示的边界范围，显式构造相应 Repair，避免无意改变算法行为。

## 测试

- 覆盖维度无效、目标为 `null`、默认 Repair 为每维 `[0, 10]` Clamp、显式 Repair 被保留，以及约束复制与评估语义不变。
- 对每个新标量 Repair 重载覆盖四种边界组合、非法维度和与逐维重载等价的修复结果。
- 调整算法、运行器、实验与基准相关测试的构造方式；固定种子行为保持可复现。
- 在 Release 配置执行 restore、build 和完整 test。

## 文档

新增 ADR-0013 替代 ADR-0012 的默认 Repair 决策；更新工程规范、架构概览、Core/Algorithms API 文档、用户与开发指南，以及示例注释，删除“由问题构造参数中的边界创建默认 Clamp”的说明。
