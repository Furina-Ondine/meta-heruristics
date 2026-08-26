# ADR-0013: Tensor 形状的 Repair 边界

## 状态

Accepted

替代 [ADR-0012](0012-repair-owned-candidate-boundaries.md) 中 `VariableBounds` 表示与由其配置内置 Repair 的规定。

## 背景

Repair 已经拥有候选位置边界，但现有 `VariableBounds` 将每个维度的两个端点绑定为对象数组。该表示无法自然表达「统一标量端点」与「逐维端点」的独立组合，也不与 `System.Numerics.Tensors.TensorPrimitives.Clamp` 的标量/向量重载形状对齐。

需要移除 `VariableBounds`，让调用方能够为下界和上界分别选择标量或向量，同时保留 Repair 的独立所有权、确定性与未来 SIMD 实现空间。

## 决策

- 删除 `VariableBounds` 及所有基于 `IReadOnlyList<VariableBounds>` 的公共 API。
- `ContinuousProblem` 的构造契约为 `ContinuousProblem(int dimension, IObjectiveFunction objective, ICandidateRepair? repair = null, OptimizationDirection direction = OptimizationDirection.Minimize, IReadOnlyList<IConstraint>? constraints = null)`。未提供 Repair 或传入 `null` 时，使用标量边界 `[0, 10]` 的 Clamp Repair。
- `CandidateRepairs.Clamp`、`Reflect` 和 `RandomReset` 各提供四种工厂重载：标量/标量、向量/标量、标量/向量和向量/向量。向量参数使用 `ReadOnlySpan<double>`，并在创建 Repair 时防御性复制。
- `double.NegativeInfinity` 与 `double.PositiveInfinity` 分别表示无下界与无上界；它们是有效端点。`NaN` 端点、任一维下界大于上界，以及双向量边界的长度不一致均在创建 Repair 时失败。
- Repair 在执行时检查任何向量边界的长度与 Position 长度一致。标量边界不约束 Position 维度。
- Clamp 的逐元素结果与 `TensorPrimitives.Clamp` 的标量/向量边界组合一致，Position 可原位修复。实现保留明确的基础路径，并为将来经基准验证的 SIMD 调用保留结构空间。
- Reflect 与 RandomReset 仅在某维上下界均有限时分别进行镜像或均匀回退；其余维度退化为 Clamp。因此单侧无界仍对有限端点截断，双侧无界不修改值。
- `System.Numerics.Tensors` 是 Core 的实现依赖；算法项目不负责 Repair 的 SIMD 实现。

## 替代方案

- 保留 `VariableBounds`：表达清晰但不能以 API 形状表示端点的独立标量/向量组合，并阻碍与 Tensor API 的直接映射。
- 每次 Repair 调用都传入边界 Span：避免复制，但扩张算法与运行上下文的契约，且 Repair 不再是自包含策略。
- 只提供全标量或全向量重载：API 较少，但不能覆盖 Tensor Clamp 支持的混合端点场景。

## 后果

这是破坏性公共 API 变更。调用方必须改用标量端点或边界数组；`ContinuousProblem` 只接收维度，不再从边界列表推导它。Repair 在创建时更早发现边界定义错误，在执行时仅检查位置与保存向量的长度关系。

现有 Clamp 的数值语义与 Tensor Clamp 对齐；Reflect 和 RandomReset 保持其已定义的有限区间行为。公共 API 文档、示例、测试和基准必须同步删除 `VariableBounds` 用法。任何 SIMD 性能声明必须先有 BenchmarkDotNet 数据。

## 重新评估条件

当 Core 支持不同数值类型、非连续候选表示、设备端执行，或实测基准证明另一种边界存储更合适时，通过新的 ADR 重新评估。
