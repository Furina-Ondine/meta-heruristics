# Repair 拥有候选位置边界设计

## 状态

待用户审阅。

## 目标

把候选位置的初始化与边界修复统一为“初始化器写入位置，Repair 立即处理；算法每次更新位置后再处理”。算法不读取边界，Core 不再逐点评估位置合法性。

## API 变更

- 删除 `ContinuousProblem.Bounds` 和 `ContinuousProblem.ValidatePosition`。
- `ICandidateInitializer.Initialize` 改为 `Initialize(Span<double> position, Random random)`；`BatOptimizer` 的初始化器成为必需参数。
- `ICandidateRepair.Repair` 改为 `Repair(Span<double> position, Random random)`；`OptimizationRunContext.Repair` 始终调用它。
- 新增 `CandidateRepairs.Clamp`、`Reflect`、`RandomReset` 和 `DoNothing`。`ContinuousProblem` 缺省使用 Clamp。

`ContinuousProblem` 继续接受边界列表，以确定维度并创建默认 Repair；自定义 Repair 在创建时自行接收并复制边界。Problem 不向算法公开这些边界。

## 语义

所有算法在 Position 初始化后和位置修改后调用 Repair。速度、频率、响度等算法内部状态仍由算法实现初始化，不交给通用初始化器。

内置 Repair 的非 `NaN` 有界行为由 ADR-0012 定义。`NaN` 与无界维度保持不变。目标值和约束违背量仍须是有限、有效数值；位置没有 Core 的防御性验证，错误 Repair 或初始化器的后果由调用方承担。

## 实施范围

1. 实现内置 Repair，并让 `ContinuousProblem` 在构造时创建默认 Clamp。
2. 更新 Core 契约和执行路径；移除所有 `ValidatePosition` 和算法边界读取。
3. 将 Bat 改为必需位置初始化器，在初始化及位置更新后调用 Repair。
4. 迁移示例、实验工厂、测试和 XML 文档；覆盖默认与自定义 Repair、无界、无穷、`NaN`、Repair 调用时机与确定性。
5. 更新工程规范、架构概览、用户/开发者/API 文档；Release 构建、测试和格式验证。
