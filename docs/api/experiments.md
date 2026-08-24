# Experiments API（第一版）

`Metaheuristics.Experiments` 用于把多个实验 Case 拆成可独立调度的 RunGroup，在受控并发度下重复执行优化，并汇总原始结果和基本统计。

## 最小用法

```csharp
var experiment = new ExperimentDefinition(
[
    new ExperimentCase<MyConfiguration>(
        id: "pso-sphere",
        configuration: configuration,
        repetitions: 30,
        createGroup: static (config, context) => new ExperimentGroupSetup(
            CreateProblem(config),
            CreateOptimizer(config),
            new OptimizationRunOptions(StoppingConditions.MaxIterations(1_000))),
        runGroupCount: 4),
]);

var result = await ExperimentRunner.RunAsync(
    experiment,
    new ExperimentExecutionOptions
    {
        GlobalMaxConcurrency = Environment.ProcessorCount,
        BaseSeed = 20260822,
    },
    cancellationToken);
```

完整的双 Case 可运行示例见 [`examples/Metaheuristics.Examples/Program.cs`](../../examples/Metaheuristics.Examples/Program.cs)。

## Case、Group 与并发

- `ExperimentDefinition` 至少包含一个 `ExperimentCase<TConfiguration>`，Case ID 必须唯一。
- 每个 Case 有 `Repetitions = N` 和 `RunGroupCount = P`；必须满足 `1 <= P <= N`，`P` 默认是 1。
- N 次运行被确定性地均衡分配给 P 个 Group。例如 N=10、P=3 时，各 Group 分别执行 4、3、3 次。
- 不同 Case 的 Group 按 Group 下标轮转投放，调度器只处理 Group，不另设 Case 并发层。
- `GlobalMaxConcurrency` 是所有 Case 共享的 Group 并发上限。实现只创建固定数量的 Worker Task，不为每个 Group 或 run 预建 Task。
- Group 工厂只在获得执行槽后调用。它创建该 Group 独占的 `ContinuousProblem`、`IOptimizer` 和可复用 `OptimizationRunOptions`。
- 同一 Group 内各 run 单线程顺序执行，并复用同一 Optimizer 的主要工作区；不同 Group 不得共享有状态 Problem 或 Optimizer。

调度实现与 `Parallel.ForEachAsync`、同步 `Parallel.ForEach` 及两种“每计划 Task + 信号量”方案的长短任务对照见 [RunGroup 调度基准](../benchmarks/run-group-scheduling.md)。现有固定 Worker 与 `Parallel.ForEachAsync` 都只维护约 `GlobalMaxConcurrency` 个长期工作项。生产者先等待信号量的版本虽能惰性投放计划，仍会为每个计划创建 Task；当前基准未显示替换收益。

`ExperimentGroupContext` 向工厂提供 Case ID、Group 下标、本 Group 的 repetition 下标及 seed，以及实验取消令牌。工厂可以用这些稳定信息创建隔离资源，但不能依赖调度或完成顺序。

## Seed

所有 Case 共享同一条实验级 seed 序列：相同 repetition 下标默认使用相同 seed。改变 `RunGroupCount`、全局并发度或实际调度顺序不会改变 seed。

- 设置 `BaseSeed` 时，Runner 使用稳定算法派生足够多的 seed，不读取当前时间。
- 设置 `Seeds` 时，Runner 在启动前复制该列表；它必须覆盖所有 Case 中最大的 repetition 数。
- 每个 run 都重新创建 `Random(seed)`，不会在 run 间复用随机数对象。

## 失败和取消

run、Case 和 Experiment 使用 `NotStarted`、`Succeeded`、`Failed`、`Canceled` 四种状态。

- 单个 run 抛出非取消异常时记录为 `Failed`，其异常保存在 `ExperimentRunResult.Exception` 中；实验继续。
- run 失败后，该 Group 不再复用当前 Optimizer，而是重新调用工厂，再执行剩余 run。
- Group 首次初始化或重建失败时，该 Group 剩余 run 全部标记为 `Failed`。
- 取消是协作式的。已经开始且观察到取消的 run 标记为 `Canceled`，尚未开始的 run 保持 `NotStarted`，`RunAsync` 返回部分结果而不向调用方重新抛出取消异常。
- 无法响应 Context 取消检查的算法不能在进程内被安全强制终止。

## 结果与统计

`ExperimentResult.Cases` 按 Case 声明顺序排列，每个 `ExperimentCaseResult.Runs` 按 repetition 下标排列，不受任务完成顺序影响。

- 成功 run 的标量信息位于 `ExperimentRunResult.Summary`。
- `BestPositionMatrix` 用私有 `double[repetition, dimension]` 保存成功 run 的最佳位置。通过索引器或 `CopyPositionTo` 只读访问；失败、取消和未开始行可用 `HasPosition` 判断。
- `ExperimentStatistics` 只统计成功 run，提供最佳目标值、迭代数、评估数和持续时间的均值、中位数、最小值、最大值及样本标准差。
- 没有成功 run 时，相应统计为 `null`；单样本标准差为零。四种状态的数量始终单独报告。

第一版不提供优先级、实时进度、CSV/JSON 导出、显著性检验或收敛曲线聚合。
