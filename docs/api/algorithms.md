# Algorithms API（第一波）

首次选择算法请先阅读[用户使用手册](../guides/user-guide.md)。`Metaheuristics.Algorithms` 当前提供连续蝙蝠算法 `BatOptimizer`。它实现 Core 的有状态 `IOptimizer`，可以用于单次 `OptimizationRunner`，也可以由 Experiment 的 RunGroup 工厂创建。

## 最小用法

```csharp
var problem = new ContinuousProblem(
    Enumerable.Repeat(new VariableBounds(-5, 5), 10).ToArray(),
    new SphereObjective());

var optimizer = new BatOptimizer(
    new RandomPositionInitializer(),
    new BatOptimizerOptions
{
    PopulationSize = 40,
});

var summary = OptimizationRunner.Execute(
    problem,
    optimizer,
    new OptimizationRunOptions(StoppingConditions.MaxIterations(200)),
    seed: 42,
    cancellationToken);
var bestPosition = optimizer.BestPosition.ToArray();
```

`RandomPositionInitializer` 是一个实现 `ICandidateInitializer` 的应用类型，例如用户手册中的[位置初始化器示例](../guides/user-guide.md#从单次执行开始)。它只写 Position；不要让它持有或修改 `BatOptimizer` 的速度、频率、响度或脉冲率状态。

可运行的单次和双 Case Experiment 示例见 [`examples/Metaheuristics.Examples/Program.cs`](../../examples/Metaheuristics.Examples/Program.cs)。

## 配置

| 属性 | 默认值 | 约束与含义 |
| --- | ---: | --- |
| `PopulationSize` | `100` | 正整数；每个 Optimizer 实例持有的蝙蝠数量。 |
| `VelocityLowerBound` / `VelocityUpperBound` | `-2` / `2` | 有限、正向且宽度有限的逐分量速度区间。 |
| `FrequencyLowerBound` / `FrequencyUpperBound` | `0` / `2` | 有限、非负、正向且宽度有限的逐分量频率区间。 |
| `InitialLoudnessLowerBound` / `InitialLoudnessUpperBound` | `0.7` / `1` | 有限、非负且正向的初始响度区间。 |
| `InitialPulseRateLowerBound` / `InitialPulseRateUpperBound` | `0` / `0.4` | 位于 `[0, 1]` 的初始脉冲发射率区间。 |
| `LoudnessDecay` | `0.98` | 位于 `(0, 1]`，接受坐标更新时衰减响度。 |
| `PulseRateGrowth` | `0.98` | 有限正数，控制脉冲发射率向初始值增长。 |

`BatOptimizer` 必须接收 `ICandidateInitializer`；它只写 Position，不能初始化速度、频率、响度或脉冲率等蝙蝠专属状态。算法独立初始化这些状态，并在每个 Position 初始化后及每次生成候选后调用 `OptimizationRunContext.Repair`。

算法不读取 `ContinuousProblem` 的边界，也不自行截断位置。未提供自定义 Repair 时，Problem 使用 Clamp Repair；自定义 Repair 必须自行保存所需边界。无界问题同样需要调用方提供能生成适用位置的初始化器。除非调用方能承担越界、无穷或 `NaN` 的后果，否则不要使用 `CandidateRepairs.DoNothing`。

## 生命周期和状态

- 第一次 `ResetForRun` 根据问题维度创建两组蝙蝠工作区，每只蝙蝠拥有位置、速度、频率、响度、当前脉冲发射率和初始脉冲发射率数组。
- 后续相同维度的正常顺序 run 复用这些对象和数组，只重新填充状态并评估完整初始种群。
- 同一实例不能切换维度，不能并发驱动，也不能在异常后继续复用。
- `BestPosition` 保存在独立数组中，只在严格改善时更新，不引用会被下一代覆写的种群对象。
- 每次 `Advance` 只把候选写入另一组缓冲。候选被拒绝时，通过交换状态对象引用保留 incumbent，不复制整条数组，也不修改 incumbent 速度。
- 所有评估都经过 `OptimizationRunContext.Evaluate`；选择使用 `EvaluationComparer`，因此支持最小化、最大化和可行性优先约束语义。

## 迁移来源与兼容边界

实现参考了原论文 [A New Metaheuristic Bat-Inspired Algorithm](https://arxiv.org/abs/1004.4170) 以及旧论文仓库 [`fix` 分支](https://github.com/Furina-Ondine/task-schedule/tree/fix)。迁移明确保留旧实现的逐维频率、响度和脉冲率变体，并带入 `fix` 分支已经确认的三项修复：

- 初始化后立即评估完整种群；
- 历史最优使用独立快照；
- 候选使用本轮新频率，并且只写目标速度缓冲。

新实现不承诺与旧仓库逐位复现。旧实现使用 `Random.Shared`、只按较小适应度选择，并在算法配置中重复声明位置边界；这些行为已经由当前 Core 的显式 seed、优化方向、约束比较和 Repair-owned 边界替代。

## 工作区复用基准

`BatWorkspaceReuseBenchmarks` 对比每个 run 新建 Optimizer 与一个 RunGroup 内顺序复用同一实例。它同时启用 `MemoryDiagnoser`，测量完整 Reset、迭代和 Core 评估路径，而不只测数组构造。

```powershell
dotnet run --project benchmarks/Metaheuristics.Benchmarks/Metaheuristics.Benchmarks.csproj `
  --configuration Release -- `
  --filter "*BatWorkspaceReuseBenchmarks*"
```

正式性能结论应使用 BenchmarkDotNet 的完整默认作业；`Dry` 作业只适合验证基准能否构建和执行。
