# ADR-0009: RunGroup 独占的有状态 Optimizer

## 状态

状态：Accepted

替代 [ADR-0008](0008-experiment-run-groups-and-reusable-workers.md)。

## 背景

ADR-0008 用 `Optimizer` 表示不可变算法定义，并增加 `Worker` 保存一个 RunGroup 可复用的数组和运行状态。继续推演公共 API 后发现，用户已经通过强类型 Config 和 Factory 分离了算法定义与实例构造：Config 是可复用定义，Factory 为每个 RunGroup 创建独占的算法实例。此时再让 Optimizer 创建 Worker，会重复表达相同的状态所有权。

现有 `IOptimizationSession` 也以一次 run 为生命周期，无法直接承载一个 RunGroup 内多次顺序 run 的工作区复用。当前尚无正式算法或 Experiment 实现，撤销这一层不会产生实际兼容负担。

## 决策

- `IOptimizationSession` 从公共执行模型中撤销，不再引入独立的 `IOptimizationWorker`。
- 强类型 Config 保存算法参数；用户提供的 Factory 为每个 RunGroup 创建一个 `IOptimizer` 实例。
- `IOptimizer` 是有状态、RunGroup 独占且不可并发使用的算法实例，直接拥有种群数组和算法临时缓冲区。
- `IOptimizer` 暴露当前最佳位置、当前最佳评估、`ResetForRun` 和 `Advance`。
- `ResetForRun` 在首次运行时可以完成依赖问题维度的物理分配；后续 run 复用这些数组，并重置全部逻辑状态、重新填写种群、应用当前 seed 的随机流并完成初始评估。
- `ResetForRun` 返回前必须提供合法的最佳位置和最佳评估；`Advance` 完成一次原子算法迭代。
- `IOptimizer` 不继承 `IDisposable`。通用契约只要求托管资源；Group 完成或 run 异常后停止引用和复用该实例，由 GC 回收。
- 若具体算法实际拥有需要确定释放的资源，可以在具体类型上自行实现 `IDisposable`，但这不属于通用优化器接口的第一波承诺。
- `OptimizationContext` 保留并重命名为 `OptimizationRunContext`，每个 run 创建一个实例，统一提供 Problem、seed、`Random`、取消、评估计数和修复。
- `OptimizationRunOptions` 不再包含 seed；Runner 每次运行显式接收 seed，并创建对应的 `OptimizationRunContext`。
- Runner 不释放调用方传入的 `IOptimizer`。单次调用使用一个 Optimizer；Experiment RunGroup 对同一个 Optimizer 顺序调用多次 `ResetForRun`。
- run 抛出非取消异常后，该 Optimizer 视为状态可能损坏，不再复用。Experiment 丢弃引用并通过 Factory 为剩余 run 创建新实例。
- 每个 RunGroup 使用独立 Problem 和 Optimizer；不同 Group 只能共享调用方提供的不可变底层数据。
- Case、RunGroup、有界调度、共享 seed 序列、取消、状态、二维结果矩阵和聚合语义继续采用 ADR-0008 中的决定。

概念接口为：

```csharp
public interface IOptimizer
{
    ReadOnlySpan<double> BestPosition { get; }

    Evaluation BestEvaluation { get; }

    void ResetForRun(OptimizationRunContext context);

    void Advance();
}
```

## 替代方案

- 保留 `Optimizer -> Worker`：所有权清楚，但 Config 和 Factory 已经承担算法定义与实例构造，Worker 会形成无实际收益的额外层次。
- 将 Session 改为跨 run 可复用：可以减少类型数量，但 Session 这一名称仍暗示一次运行，且与用户希望直接实现通用 `IOptimizer` 接口不符。
- 每个 run 创建独立 Optimizer：隔离最简单，但会重复分配大型种群和临时缓冲区。
- 让 `IOptimizer` 继承 `IDisposable`：便于未来资源释放，但当前通用实现只持有托管资源，会给全部算法和调用方增加无必要的释放契约。
- 复用 `OptimizationRunContext`：可以再减少一个小对象分配，但需要可靠重置 Random、评估计数和取消状态，增加跨 run 污染风险。

## 后果

公共执行层次缩短为 Config、Factory、IOptimizer 和每 run Context。算法作者直接在 IOptimizer 中管理可复用数组，Experiment 通过 RunGroup 控制实例数量和复用粒度。

IOptimizer 不再是可安全并发复用的不可变定义。文档和命名必须明确：Config 可复用，IOptimizer 是可变执行实例。调用方不得同时驱动同一个 IOptimizer，也不得在异常后继续调用 `ResetForRun`。

通用接口没有确定释放钩子。未来若池化数组、非托管内存、文件或设备资源成为通用算法需求，需要通过新 ADR 重新评估资源所有权，而不是假定丢弃托管引用足以释放所有资源。

## 重新评估条件

出现以下需求时通过新 ADR 重新评估：

- 通用算法稳定使用池化数组、非托管内存或其他必须确定释放的资源；
- 同一个算法定义需要直接并发创建大量执行实例，且 Config/Factory 无法清晰表达；
- 异步初始化、异步释放或跨进程执行进入路线图；
- 基准证明 run 级 Context 分配也需要复用。
