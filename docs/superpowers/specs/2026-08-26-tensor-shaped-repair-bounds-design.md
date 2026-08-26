# Tensor 形状 Repair 边界设计

## 目标

移除 `VariableBounds`，以与 `TensorPrimitives.Clamp` 对齐的端点形状配置内置 Repair。下界和上界可独立为单个 `double` 或逐维 `ReadOnlySpan<double>`；Repair 在创建后自包含，并能在未来替换为经验证的 SIMD 实现。

## ContinuousProblem 契约

```csharp
ContinuousProblem(
    int dimension,
    IObjectiveFunction objective,
    ICandidateRepair? repair = null,
    OptimizationDirection direction = OptimizationDirection.Minimize,
    IReadOnlyList<IConstraint>? constraints = null)
```

- `dimension` 必须为正数；`objective` 不可为 `null`。
- `repair` 为 `null` 时使用 `CandidateRepairs.Clamp(0, 10)`。
- 自定义 Repair 由调用方完全拥有其端点配置。Problem 不读取或预验证 Repair 内部的向量长度。
- 方向与约束的验证、约束集合防御性复制和评估语义不变。

## Repair 工厂

`Clamp`、`Reflect`、`RandomReset` 为同一组边界形状各提供以下重载：

```csharp
Repair(double lower, double upper)
Repair(ReadOnlySpan<double> lower, double upper)
Repair(double lower, ReadOnlySpan<double> upper)
Repair(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper)
```

这里的 `Repair` 表示对应的 `CandidateRepairs` 工厂名。Span 参数只用于构造，工厂会复制为私有数组；随后对调用方数组的修改不会影响 Repair。

端点规则：

- `-Infinity` 表示无下界，`+Infinity` 表示无上界；它们合法。
- 端点 `NaN` 不合法。
- 每一维必须满足 `lower <= upper`；标量与向量组合在创建时逐维验证。
- 两个向量端点的长度必须在创建时相等。任一向量端点在 Repair 时必须与 Position 等长；违反时抛出 `ArgumentException`。

## Repair 行为

- Clamp 按 `TensorPrimitives.Clamp` 的逐元素语义原位写回 Position。`NaN` Position 保持不变；无界端点不会限制该侧。
- Reflect 只对双侧有限区间的有限越界值执行镜像映射；任何无界端点或非有限 Position 退化为 Clamp。
- RandomReset 只对双侧有限区间的非 `NaN` 越界值或无穷值进行均匀采样；其他情形退化为 Clamp。
- DoNothing 继续不修改 Position，不持有边界。

## 实现边界

- 删除 `VariableBounds.cs`，以私有的标量/向量端点存储取代它。
- 内置 Repair 使用共享的创建时端点验证和运行时长度验证；不在每次 Repair 时重复检查边界值。
- 将 `System.Numerics.Tensors` 包引用迁移至 `Metaheuristics.Core`。首版可以采用清晰的标量循环；只有在 BenchmarkDotNet 证明价值后才直接调用 TensorPrimitives 并记录性能结论。
- 移除 Algorithms 项目内为 Repair 试验加入的 Tensor 引用及无参调用，避免错误的依赖方向。

## 迁移与测试

- 调用方把 `VariableBounds[]` 改为标量端点或 `double[]` 端点；`ContinuousProblem` 的第一个参数改为维度。
- 示例、Benchmark 和测试保留原有范围，通过相应 Repair 工厂显式表达。
- 覆盖四种端点形状、全部无界/单侧无界/双侧有限、边界 `NaN`、反向范围、向量长度不一致、Position 长度不一致、源数组防御性复制、`NaN` Position、Reflect/RandomReset 退化语义和固定种子可复现性。
- 在 Release 配置执行 restore、build 和完整 test；SIMD 路径只有在添加基准与验证后才可启用。
