# RunGroup 调度基准

本文记录 RunGroupPlan 已经完成拆分和交错后，不同全局有界并发实现的对照结果。它只评估调度器，不改变 Experiment 的公共行为。

## 要回答的问题

当前 `ExperimentRunner` 创建固定数量的长期 Worker Task。每个 Worker 从一个加锁的惰性枚举器中取得下一个 RunGroupPlan，并同步执行该计划。

这与 `Parallel.ForEachAsync` 的 worker-loop 结构接近，但仍需与以下替代方案对照：

- 固定 Worker + 共享枚举器，即当前生产实现；
- [`Parallel.ForEachAsync`](https://learn.microsoft.com/dotnet/api/system.threading.tasks.parallel.foreachasync)；
- 在线程池 Task 中运行同步 `Parallel.ForEach`；
- 每个计划创建一个 Task，所有 Task 等待容量为 `workerCount` 的 `SemaphoreSlim`。
- 调度主流程先等待 `SemaphoreSlim`，获得槽后才创建当前计划的 Task，并由 Task 释放槽位。

.NET 的 `Parallel.ForEachAsync` 实现同样按并发度逐步创建 Worker，每个 Worker 循环取得并执行多个元素，而不是为每个元素创建 Task。具体机制可见 [.NET Runtime 源码](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Threading.Tasks.Parallel/src/System/Threading/Tasks/Parallel.ForEachAsync.cs)。

## 方法

基准代码位于 [`RunGroupSchedulingBenchmarks.cs`](../../benchmarks/Metaheuristics.Benchmarks/RunGroupSchedulingBenchmarks.cs)，使用 BenchmarkDotNet 0.15.8 的内存和线程诊断器。

测试环境：

- Windows 11 25H2；
- AMD Ryzen 7 9800X3D，8 个物理核心、16 个逻辑处理器；
- .NET 10.0.11，x64 RyuJIT；
- `workerCount = 4`，每次操作包含 128 个计划；
- 3 个独立 launch；每个 launch 进行 5 次预热和 10 次正式测量。

计划执行体是同步 CPU 整数运算，不使用 `Task.Delay`、`Sleep` 或显式让出线程。四种时长分布为：

| 分布 | 含义 |
| --- | --- |
| `UniformShort` | 128 个等长短计划，最容易放大调度成本。 |
| `UniformLong` | 128 个等长计划，每个工作量是短计划的 16 倍。 |
| `AlternatingLongShort` | 长短计划按输入顺序交替。 |
| `SingleLongTail` | 前 127 个计划很短，最后一个计划是短计划的 128 倍。 |

运行命令：

```powershell
dotnet run --project benchmarks/Metaheuristics.Benchmarks/Metaheuristics.Benchmarks.csproj `
  -c Release -- --filter "*RunGroupSchedulingBenchmarks*" `
  --launchCount 3 --warmupCount 5 --iterationCount 10
```

## 结果

| 调度方式 | 全短 | 全长 | 长短交替 | 单一长尾 | Work item/次 | 分配/次 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 固定 Worker | 1.402 ms | 22.252 ms | 11.759 ms | 6.825 ms | 4.0 | 728–744 B |
| `Parallel.ForEachAsync` | 1.412 ms | 22.233 ms | 11.780 ms | 6.804 ms | 约 4.0–4.4 | 419–497 B |
| `Task.Run` + `Parallel.ForEach` | 1.426 ms | 22.216 ms | 11.801 ms | 6.728 ms | 4.0 | 2,356–2,408 B |
| 每计划 Task + 信号量 | 1.428 ms | 21.892 ms | 11.768 ms | 6.851 ms | 约 251.8–252.0 | 约 56,900 B |
| 生产者先等信号量，再创建 Task | 1.419 ms | 21.852 ms | 11.893 ms | 6.892 ms | 约 232.9–251.0 | 约 31.1–32.7 KB |

30 次正式测量覆盖三个独立进程，各方案的耗时差距仍在约 2% 内，而且没有一种方案在四种时长分布中持续领先，不足以证明稳定的吞吐排序。可以稳定观察到的是任务和内存形态：两种每计划 Task 的信号量实现都没有获得相应的吞吐收益，却把累计 work item 和 Task 分配从 `O(workerCount)` 提升到 `O(planCount)`。

生产者先等待信号量的版本不会提前创建尚未获得槽位的 Task，也不会让全部计划同时成为信号量 waiter，因此比“Task 内等待”少约一半分配。它仍需为每个计划创建 Task，并在每次 `Release` 后恢复生产者以投放下一项；这解释了其 work item 数仍远高于长期 Worker。

### 同步 Runner 补测

上述五种方法保留了当前 `RunAsync` 的异步外形。为评估同步 `ExperimentRunner.Run`，使用相同环境和轮次另测三种同步入口：调用线程同步等待固定 Worker、调用线程直接进入 `Parallel.ForEach`，以及调用线程同步等待信号量后创建工作 Task。

| 调度方式 | 全短 | 全长 | 长短交替 | 单一长尾 | Work item/次 | 分配/次 |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| 同步等待固定 Worker | 1.390 ms | 22.053 ms | 11.781 ms | 6.835 ms | 4.0 | 624 B |
| 直接同步 `Parallel.ForEach` | 1.396 ms | 22.071 ms | 11.769 ms | 6.801 ms | 3.0 | 2,152–2,201 B |
| 主线程 `Wait()`，Task `Release()` | 1.471 ms | 22.321 ms | 12.055 ms | 6.923 ms | 128.0 | 22,784 B |

同步固定 Worker 与直接 `Parallel.ForEach` 在四种分布下的均值差距不足 0.5%，没有稳定赢家。直接 `Parallel.ForEach` 让调用线程参与计算，因此只需约三个额外线程池 work item；固定 Worker 分配更少，但调用线程只负责等待四个线程池 Worker。同步信号量方案避免了异步生产者 continuation，却仍为 128 个计划各创建一个 Task，四种分布下均慢于另外两种同步方案。

`Completed Work Items` 是线程池处理的 work item 数，不是操作系统上下文切换次数。它能反映任务排队和恢复边界，并说明信号量方案提供了更多发生上下文切换的机会；若要报告精确的 OS context-switch 次数，需要另用 ETW/WPA 对较长的独立运行采样，不能从该列直接换算。

## 结论

- 当前固定 Worker 实现确实可以看作一个面向同步 RunGroupPlan 的简化 `Parallel.ForEachAsync`。两者都是少量长期 Worker 动态领取下一项。
- 不采用“先为全部计划创建 Task、Task 内等待共享信号量”作为生产实现。它会提前物化全部计划对应的 Task 和 continuation，与“不为所有 Group 预建 Task”的现有契约冲突，而且本次基准没有显示吞吐收益。
- “生产者先等待信号量，获得槽后再创建 Task”满足按槽位惰性投放，也比前一种信号量方案节省约一半分配；但仍产生每计划 Task 和生产者恢复，当前没有相对于固定 Worker 的收益。
- 若保留异步 API，`Parallel.ForEachAsync` 可以作为减少自定义调度代码的候选，但本次数据没有给出性能迁移动机。
- 若将 Experiment 改成同步 `Run`，直接 `Parallel.ForEach` 与同步等待固定 Worker 的吞吐相当。前者避免占用一个只等待的调用线程并减少自定义调度代码，是更自然的候选；替换前仍须保持部分取消、继续失败、停止生成新计划和不向调用方抛出取消异常等语义。
- 无论同步还是异步，都不采用每计划 Task 的信号量方案。生产者侧先获得槽位可以减少 waiter 和分配，但不能消除每计划 Task 的累计调度成本。
- 单一长尾下四种方案耗时几乎相同。调度器不能拆分已经开始执行的 RunGroupPlan；长尾利用率仍主要由用户选择 `RunGroupCount`、每组包含的 repetition 数量以及计划顺序决定。
