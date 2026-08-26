# 用户使用手册

Metaheuristics.NET 是一个仍在演进中的连续单目标优化 demo。你负责描述问题、选择算法与停止条件；库负责一次执行的生命周期，或把多次执行编排成可复现实验。

## 从单次执行开始

下面是完整的最小流程：定义有界问题、创建一个优化器、执行、然后在下一次执行前复制最佳位置。

```csharp
var problem = new ContinuousProblem(
    [new VariableBounds(-5, 5), new VariableBounds(-5, 5)],
    new SphereObjective());
var optimizer = new BatOptimizer(
    new RandomPositionInitializer(),
    new BatOptimizerOptions { PopulationSize = 40 });
var options = new OptimizationRunOptions(StoppingConditions.MaxIterations(200));

var summary = OptimizationRunner.Execute(problem, optimizer, options, seed: 42);
var bestPosition = optimizer.BestPosition.ToArray();

Console.WriteLine(summary.BestEvaluation.Objective);
```

其中初始化器只负责写入 Position，并使用执行提供的随机流；默认 Clamp Repair 会在它返回后立即运行：

```csharp
sealed class RandomPositionInitializer : ICandidateInitializer
{
    public void Initialize(Span<double> position, Random random)
    {
        for (var index = 0; index < position.Length; index++)
        {
            position[index] = (random.NextDouble() * 10) - 5;
        }
    }
}
```

`OptimizationRunSummary` 是不可变的，包含最佳评估、停止原因、迭代数、评估数、耗时、seed 和轨迹。`BestPosition` 由 `IOptimizer` 的可复用工作区拥有：在下一次 `ResetForRun` 或任何异常之后都不得继续引用它；需要长期保存时立即复制。

## 描述问题

`ContinuousProblem` 需要每一维的 `VariableBounds` 与一个 `IObjectiveFunction`。边界用于确定维度，并在没有显式 Repair 时创建默认的截断 Repair；算法实现不会读取它们。目标函数必须返回有限 `double`。默认方向为最小化；传入 `OptimizationDirection.Maximize` 可最大化。

可选 `IConstraint` 返回已归一化的非负违背量，零表示满足。比较时始终先选可行解，再比较不可行解的总违背量，最后才按目标方向比较。位置本身由你的 `ICandidateInitializer` 和 `ICandidateRepair` 负责：算法在初始 Position 写入后、以及每次修改 Position 后都会调用 Repair；Core 不检查位置是否越界、非有限或包含 `NaN`。

默认 Repair 是 `CandidateRepairs.Clamp`：有界维度上的有限越界值与正负无穷会被截断到端点，`NaN` 保持不变，无界维度不处理。也可传入 `CandidateRepairs.Reflect(bounds)` 做双侧镜像，或 `CandidateRepairs.RandomReset(bounds)` 在双侧有限边界内随机回退。`CandidateRepairs.DoNothing` 完全跳过修复；除非你能自行保证初始化、每条位置更新路径及其数值后果，否则不要使用它。

## 控制执行

使用 `StoppingConditions` 组合最大迭代、最大评估、时限或目标阈值。`OptimizationTraceOptions` 可关闭轨迹、逐迭代记录、按评估间隔记录，或按显式进度比例记录。取消使用标准 `CancellationToken`；单次 `Execute` 遇到取消会抛出 `OperationCanceledException`，不会返回部分汇总。

每次执行都以显式 seed 创建独立 `Random`。相同库版本、运行时和执行设置下，相同 seed 会生成相同结果；不要在目标函数、约束、初始化器或修复器中使用 `Random.Shared` 或当前时间。

## 批量实验

`ExperimentRunner.RunAsync` 接受 `ExperimentDefinition`。每个 `ExperimentCase<TConfiguration>` 用强类型配置和 Group 工厂创建隔离的 Problem、Optimizer 与运行选项。

`RunGroupCount` 把一个 Case 的 repetitions 均衡拆为若干 Group；同一 Group 内顺序复用 Optimizer 的工作区，多个 Group 受 `GlobalMaxConcurrency` 限制并发。不同 Group 不得共享可变 Problem、Optimizer、随机数或工作区。

实验结果始终按 Case 声明顺序和 repetition 下标排列。取消返回部分 `ExperimentResult`：已执行项为 `Succeeded`、`Failed` 或 `Canceled`，未投放项为 `NotStarted`。失败后库会丢弃该 Group 的 Optimizer，并为余下 repetitions 重新创建 Group。

## 自定义扩展时的安全规则

- `IOptimizer` 不线程安全；只能由一个 Group 顺序驱动，异常后必须丢弃。
- `IStoppingCondition` 必须可重入，不能保存执行级可变状态。
- `IObjectiveFunction`、`IConstraint`、`ICandidateInitializer` 和 `ICandidateRepair` 的线程安全由实现者负责；若多个 Group 共享它们依赖的底层数据，该数据必须不可变或自行同步。
- `BatOptimizer` 必须传入 `ICandidateInitializer`。它只初始化 Position；速度、频率、响度和脉冲率仍由算法自己初始化。初始化器应使用传入的 `Random`，并让随后的 Repair 处理位置恢复。

可运行的单次与实验示例见 [`Program.cs`](../../examples/Metaheuristics.Examples/Program.cs)。
