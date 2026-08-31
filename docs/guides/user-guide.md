---
uid: user-guide
---

# 用户使用手册

本手册面向第一次使用 Metaheuristics.NET 解决优化问题的人。读完后，你应该知道库能解决什么、单次求解从哪里进入、怎样读取结果，以及什么时候需要转向批量实验。

## 这个库解决什么问题

当你可以用一个函数评价“某个 `double` 向量有多好”，但无法直接写出最优向量时，可以使用元启发式算法搜索近似最优解。

以 Sphere 函数为例：输入是位置 `[x₁, x₂, …]`，目标值是各分量平方和。最小值出现在零向量。你负责提供评价函数；Optimizer 负责生成和改进候选；Runner 负责停止、计数、取消和结果汇总。

当前版本适合连续、单目标、同步评价问题。它不承诺找到数学上的全局最优值，也不支持多目标、二进制、排列、远程或 GPU 计算。

## 两条使用路径

### 求解一次

```text
ContinuousProblem + IOptimizer + OptimizationRunOptions
    → OptimizationRunner.Execute
    → OptimizationRunSummary + optimizer.BestPosition
```

适合求解一个问题、调试 Objective 或查看一次收敛过程。

### 重复实验

```text
ExperimentCase + ExperimentDefinition + ExperimentExecutionOptions
    → ExperimentRunner.RunAsync
    → ExperimentResult
```

适合用多个 seed 重复运行、比较多个配置、限制总并发并汇总统计。

## 第一次运行需要掌握的六个概念

| 概念 | 你需要决定什么 |
| --- | --- |
| Problem | 候选向量有多少维，以及如何组合 Objective 和可选的 Repair、Constraint。 |
| Objective | 给定一个位置，返回怎样的目标值。 |
| Initializer | 如何使用当前 run 的随机流写入每个初始候选位置。 |
| Optimizer | 使用哪种搜索算法；当前内置实现包括 `BatOptimizer`、`PsoOptimizer`、`FireflyOptimizer` 和 `CuckooOptimizer`。 |
| Stopping Condition | 达到多少迭代、评估、时间或目标值时停止。 |
| Result | 从 Summary 读取目标、迭代和停止原因；从 Optimizer 读取最佳位置。 |

Initializer 是所有内置 Optimizer 的必需依赖。当前库中没有可直接实例化的通用随机 Initializer；调用方需要实现 `ICandidateInitializer`，明确初值的分布。Repair 和 Constraint 是按问题需要组合的可选策略。

## 第一个完整例子

下面是一份可作为顶层 `Program.cs` 编译的单次求解示例：

```csharp
using Anastasya.Metaheuristics.Algorithms.Bat;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;

var problem = new ContinuousProblem(
    dimension: 2,
    objective: new SphereObjective(),
    repair: CandidateRepairs.Clamp(-5, 5));

var optimizer = new BatOptimizer(
    new UniformInitializer(-5, 5),
    new BatOptimizerOptions { PopulationSize = 40 });
var options = new OptimizationRunOptions(
    StoppingConditions.MaxIterations(100));

var summary = OptimizationRunner.Execute(
    problem, optimizer, options, seed: 20260820);
var bestPosition = optimizer.BestPosition.ToArray();

Console.WriteLine($"Best objective: {summary.BestEvaluation.Objective}");
Console.WriteLine($"Best position: [{string.Join(", ", bestPosition)}]");

file sealed class SphereObjective : IObjectiveFunction
{
    public double Evaluate(ReadOnlySpan<double> position)
    {
        var sum = 0.0;
        foreach (var value in position)
        {
            sum += value * value;
        }

        return sum;
    }
}

file sealed class UniformInitializer : ICandidateInitializer
{
    private readonly double _lowerBound;
    private readonly double _width;

    public UniformInitializer(double lowerBound, double upperBound)
    {
        var width = upperBound - lowerBound;
        if (!double.IsFinite(lowerBound)
            || !double.IsFinite(upperBound)
            || lowerBound > upperBound
            || !double.IsFinite(width))
        {
            throw new ArgumentException("Initializer bounds must define a finite ordered interval.");
        }

        _lowerBound = lowerBound;
        _width = width;
    }

    public void Initialize(Span<double> position, Random random)
    {
        for (var index = 0; index < position.Length; index++)
        {
            position[index] = _lowerBound + (_width * random.NextDouble());
        }
    }
}
```

1. `ContinuousProblem` 组合维度、Sphere Objective 和位置 Repair。
2. `UniformInitializer` 是调用方在示例中实现的策略，只用当前 run 传入的 `Random` 产生 `[-5, 5)` 初值；它不是库的内置契约。
3. `BatOptimizer` 使用 Initializer 创建种群并执行搜索；也可换成 `PsoOptimizer`、`FireflyOptimizer` 或 `CuckooOptimizer`。
4. `OptimizationRunOptions` 组合 Stopping Condition，决定何时停止。
5. `OptimizationRunner.Execute` 用显式 seed 执行一次完整生命周期，并返回 Summary。
6. Summary 保存最佳评估、迭代数和停止原因；最佳位置则从 Optimizer 读取。

`summary.BestEvaluation.Objective` 是最佳目标值。最佳位置仍存放在 Optimizer 的可复用工作区，因此示例在 `Execute` 返回后立即调用 `ToArray()` 保存副本；精确生命周期见生成式 API Reference 中的 `IOptimizer.BestPosition`。包含四种算法和 Experiment 组装的项目级示例见 [`Program.cs`](../../examples/Metaheuristics.Examples/Program.cs)。

运行完整示例：

```powershell
dotnet run --project examples/Metaheuristics.Examples/Metaheuristics.Examples.csproj --configuration Release
```

## 常见变化

### 最大化

创建 Problem 时选择 `OptimizationDirection.Maximize`。Objective 仍只负责计算数值，比较方向由 Problem 决定。

### 改变位置范围或恢复策略

通过 `CandidateRepairs` 选择 Clamp、Reflect 或 RandomReset，并把策略传给 `ContinuousProblem`。两端边界必须同为标量，或同为与 Position 等长的逐维向量；混合标量/向量端点由调用方按领域语义显式转换。算法不读取上下界；Initializer 写入初值后以及算法修改位置后，都会调用 Repair。

未提供 Repair 时使用 `[0, 10]` Clamp。`DoNothing` 表示调用方自己承担位置合法性和数值后果，不是推荐的默认配置。

### 增加约束

实现 `IConstraint` 返回非负违背量：零表示满足，大于零表示违反。候选比较先考虑可行性，再比较违背量，最后才比较目标值。

### 理解 NaN 与 Infinity

Objective 可以返回有限值或正负 Infinity；Infinity 会按最小化/最大化方向正常排序。Objective 返回 `NaN` 会立即失败，因为它不能形成可靠顺序。

Constraint 可以返回非负有限值或 `+Infinity`；后者表示无界的不可行程度。负值、`-Infinity` 和 `NaN` 都是无效结果。这里的规则只约束评价结果，不会让 Core 扫描或修复候选位置。

Experiment 的目标统计可能无法定义：样本同时含正负 Infinity 时 Mean 为 `null`，偶数样本的两个中间值为相反 Infinity 时 Median 为 `null`，多样本含任意 Infinity 时 StandardDeviation 为 `null`。Minimum 和 Maximum 始终保留。输出统计时应显式处理 `null`，不要把它显示成零或 `NaN`。

### 改变停止方式

`StoppingConditions` 可以按最大迭代、最大评估、时限或目标值停止，也可以用 `Any` 组合多个条件。停止检查发生在初始化完成后和每次完整算法迭代后。

目标阈值接受正负 Infinity、拒绝 `NaN`，比较方式与 Problem 的优化方向一致。

### 保持可复现

每次运行都传入显式 seed。不要在 Objective、Constraint、Initializer 或 Repair 中使用 `Random.Shared`、当前时间或跨运行共享的随机状态。

## 什么时候使用 Experiment

当问题从“求解一次”变成“用多个 seed 重复运行”“比较多组配置”或“限制整个批次的并发”时，使用 Experiments：

- `ExperimentCase<TConfiguration>` 保存一组强类型配置、重复次数和 Group 工厂；
- `ExperimentDefinition` 收集一个或多个 Case；
- `ExperimentExecutionOptions` 设置共享 seed 序列和全局并发上限；
- `ExperimentRunner.RunAsync` 执行并返回按 Case 和 repetition 稳定排列的结果。

`RunGroupCount` 决定一个 Case 拆成多少个独占 Optimizer 的 Group。同一 Group 内顺序复用工作区；不同 Group 不共享可变 Optimizer、Problem 或随机状态。取消会返回已完成的部分结果，而不是丢弃整个 Experiment。

完整实验组装见同一份 [`Program.cs`](../../examples/Metaheuristics.Examples/Program.cs)。

## 替换策略

不需要修改 Core 就能实现自己的：

- `IObjectiveFunction` 和 `IConstraint`；
- `ICandidateInitializer` 和 `ICandidateRepair`；
- `IStoppingCondition`；
- `IOptimizer`。

具体入口关系见 [API Overview](../api/overview.md)。实现策略或新算法前阅读[开发者架构手册](../architecture/developer-guide.md)；参数、异常、所有权和成员生命周期以按 API Overview 中的生成说明构建的 API Reference 为准。
