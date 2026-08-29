# ADR-0016: 算法私有固定宽度 SIMD 级联

## 状态

Accepted

## 背景

PSO 的速度公式是无状态的逐元素算术，但没有单个 `TensorPrimitives` 操作能表达其三个速度项的融合公式。直接的 `TensorPrimitives` 组合因多次 Span 遍历未满足全部端到端门槛，因此需要一个 Algorithms 私有实现。

最初的 `Vector<T>` 实现只采用运行时的单一宽度并以标量处理尾部。在支持 512 位向量的机器上，长度 15 会变成 8 个向量元素加 7 个标量元素，不能像 `TensorPrimitives` 和 [SPEC-0004](../specs/SPEC-0004-masked-simd-reflect/spec.md) 的内核那样继续使用较窄向量处理剩余部分。

项目作者指定 PSO 首先使用 512→256→128→标量级联，并保持 `TensorPrimitives` 为优先选择。这扩大了 Algorithms 可使用的内部向量设施范围，必须明确其边界。PSO 是当前首个落地点，不构成其他算法使用该设施的排他授权。

## 决策

- `Metaheuristics.Algorithms` 中任何实际算法调用的 `internal VectorOps`，在该算法的 Spec/Verification 证明有具体性能收益且保持既有语义时，可使用 `System.Runtime.Intrinsics` 的通用 `Vector512<T>`、`Vector256<T>` 和 `Vector128<T>`。PSO 是首个已批准的调用者，Firefly 可按同一门槛进行候选实现。
- 每个算法的固定宽度内核都按当前剩余长度和 `IsHardwareAccelerated` 依次处理完整的 512、256、128 位向量块，最后只用标量处理不足 128 位的尾部。例如 512 位硬件上的 15 个 double 按 8 + 4 + 2 + 1 处理。
- 不使用 ISA 专属命名空间（包括 `System.Runtime.Intrinsics.X86` 与 `.Arm`），不引入公开 SIMD 后端、配置、SoA 布局、共享缓冲区或跨 Optimizer 状态。
- 各算法仍须先审查直接 `TensorPrimitives` 调用和无分配组合；固定宽度级联只覆盖已证明无法以更简单路径满足性能门槛的具体热路径。PSO 的速度限幅和位置更新继续由 `TensorPrimitives.Clamp` 和 `TensorPrimitives.Add` 负责。
- 每次 BenchmarkDotNet 测量前必须先展示待测代码与完整命令并获得项目作者反馈。性能是否保留仍由同机内核和端到端门槛决定。

## 替代方案

- 单一 `Vector<T>` 加标量尾部：不采用。它不能逐级利用较窄向量，已在 512 位机器的 15 元素场景暴露出不足。
- 基于 `2 * Vector<T>.Count` 的整段标量阈值：不采用。它用启发式回避问题，但没有实现项目作者指定的完整宽度级联。
- 仅用 `TensorPrimitives` 操作组合：保留为优先尝试，但不用于当前融合速度公式的生产实现，因为现有端到端测量未证明其在所有主要负载中更快。
- ISA 专属 API：不采用。它会扩大硬件和维护边界，且本决策只需要通用固定宽度向量。

## 后果

采用该决策的算法会增加三个窄小的固定宽度循环，并必须保证每个循环只在剩余 Span 足够时加载或存储。测试必须覆盖 2、7、8、15、16 及主要长度，基准报告必须记录三个宽度的硬件可用性。

该决策不改变公开 API、随机调用顺序、Repair/Evaluate 时点或跨平台数值承诺。不同运行时可以选择不同向量宽度；同一目标环境内仍由差分和固定 seed 测试保护语义。

## 重新评估条件

若 `TensorPrimitives` 提供可直接表达目标热路径的无分配操作且端到端测量通过，或若需要 ISA 专属 API、更多向量宽度、公共计算后端、共享状态或改变公开边界，则以新的 ADR 重新评估。其他算法在自己的 Spec/Verification 中证明收益不需要新的 ADR。
