using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>
/// 比较 RunGroupPlan 已经展开后，不同全局有界并发调度方式的成本。
/// </summary>
/// <remarks>
/// 基准体只包含同步 CPU 工作，用来模拟当前同步的优化运行。若改用 <see cref="Task.Delay(int)"/>，
/// 测得的主要会是计时器和异步恢复成本，不能代表优化算法占用工作线程时的行为。
/// </remarks>
[MemoryDiagnoser]
[ThreadingDiagnoser]
[BenchmarkCategory("Experiment", "Scheduling")]
public class RunGroupSchedulingBenchmarks
{
    private const int PlanCount = 128;
    private const int IterationsPerWorkUnit = 25_000;
    private static long s_sink;

    private int[] _planWorkUnits = null!;
    private ParallelOptions _parallelOptions = null!;

    /// <summary>
    /// 获取或设置全局并发工作槽数量。
    /// </summary>
    [Params(4)]
    public int WorkerCount { get; set; }

    /// <summary>
    /// 获取或设置 RunGroupPlan 的相对时长分布。
    /// </summary>
    [Params(
        WorkloadShape.UniformShort,
        WorkloadShape.UniformLong,
        WorkloadShape.AlternatingLongShort,
        WorkloadShape.SingleLongTail)]
    public WorkloadShape Shape { get; set; }

    /// <summary>
    /// 为每个参数组合创建稳定的计划时长序列。
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _planWorkUnits = CreateWorkload(Shape);
        _parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = WorkerCount,
        };
    }

    /// <summary>
    /// 测量生产实现采用的固定数量长期 Worker 和共享惰性枚举器。
    /// </summary>
    /// <returns>表示全部计划完成的任务。</returns>
    [Benchmark(Baseline = true)]
    public Task FixedWorkerLoop()
    {
        return RunFixedWorkersAsync();
    }

    /// <summary>
    /// 测量同步 Runner 等待固定数量长期 Worker 完成的方案。
    /// </summary>
    [Benchmark]
    public void SynchronousFixedWorkerLoop()
    {
        using var plans = ((IEnumerable<int>)_planWorkUnits).GetEnumerator();
        var planGate = new object();
        var workerCount = Math.Min(WorkerCount, _planWorkUnits.Length);
        var workers = new Task[workerCount];
        for (var workerIndex = 0; workerIndex < workers.Length; workerIndex++)
        {
            workers[workerIndex] = Task.Run(() => RunWorker(plans, planGate));
        }

        Task.WaitAll(workers);
    }

    /// <summary>
    /// 测量运行时提供的异步并行枚举 API。
    /// </summary>
    /// <returns>表示全部计划完成的任务。</returns>
    [Benchmark]
    public Task ParallelForEachAsync()
    {
        return Parallel.ForEachAsync(
            _planWorkUnits,
            _parallelOptions,
            static (workUnits, _) =>
            {
                ExecutePlan(workUnits);
                return ValueTask.CompletedTask;
            });
    }

    /// <summary>
    /// 测量在线程池上运行同步  <see cref="Parallel.ForEach{TSource}(IEnumerable{TSource}, ParallelOptions, Action{TSource})"/> 的方案。
    /// </summary>
    /// <returns>表示全部计划完成的任务。</returns>
    [Benchmark]
    public Task ParallelForEach()
    {
        return Task.Run(
            () => Parallel.ForEach(
                _planWorkUnits,
                _parallelOptions,
                static workUnits => ExecutePlan(workUnits)));
    }

    /// <summary>
    /// 测量同步 Runner 直接调用 <see cref="Parallel.ForEach{TSource}(IEnumerable{TSource}, ParallelOptions, Action{TSource})"/> 的方案。
    /// </summary>
    /// <returns>并行循环的完成状态。</returns>
    [Benchmark]
    public ParallelLoopResult SynchronousParallelForEach()
    {
        return Parallel.ForEach(
            _planWorkUnits,
            _parallelOptions,
            static workUnits => ExecutePlan(workUnits));
    }

    /// <summary>
    /// 测量每个计划都创建 Task，再共同等待一个计数信号量的方案。
    /// </summary>
    /// <returns>表示全部计划完成的任务。</returns>
    [Benchmark]
    public Task TaskPerPlanWithSemaphore()
    {
        return RunSemaphoreTasksAsync();
    }

    /// <summary>
    /// 测量调度主流程先等待执行槽，获得槽后才为计划创建 Task 的方案。
    /// </summary>
    /// <returns>表示全部计划完成的任务。</returns>
    [Benchmark]
    public Task ProducerWaitThenTask()
    {
        return RunProducerGatedTasksAsync();
    }

    /// <summary>
    /// 测量同步调度线程先等待执行槽，再创建由工作 Task 释放槽位的方案。
    /// </summary>
    [Benchmark]
    public void SynchronousProducerWaitThenTask()
    {
        using var semaphore = new SemaphoreSlim(WorkerCount, WorkerCount);
        var tasks = new Task[_planWorkUnits.Length];
        for (var planIndex = 0; planIndex < tasks.Length; planIndex++)
        {
            // 同步 Runner 在生产者线程等待容量；没有槽位时不会创建下一计划的 Task。
            semaphore.Wait();
            var workUnits = _planWorkUnits[planIndex];
            tasks[planIndex] = Task.Run(
                () =>
                {
                    try
                    {
                        ExecutePlan(workUnits);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
        }

        Task.WaitAll(tasks);
    }

    private async Task RunFixedWorkersAsync()
    {
        using var plans = ((IEnumerable<int>)_planWorkUnits).GetEnumerator();
        var planGate = new object();
        var workerCount = Math.Min(WorkerCount, _planWorkUnits.Length);
        var workers = new Task[workerCount];
        for (var workerIndex = 0; workerIndex < workers.Length; workerIndex++)
        {
            workers[workerIndex] = Task.Run(() => RunWorker(plans, planGate));
        }

        await Task.WhenAll(workers).ConfigureAwait(false);
    }

    private async Task RunSemaphoreTasksAsync()
    {
        using var semaphore = new SemaphoreSlim(WorkerCount, WorkerCount);
        var tasks = new Task[_planWorkUnits.Length];
        for (var planIndex = 0; planIndex < tasks.Length; planIndex++)
        {
            var workUnits = _planWorkUnits[planIndex];
            tasks[planIndex] = Task.Run(
                async () =>
                {
                    await semaphore.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        ExecutePlan(workUnits);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task RunProducerGatedTasksAsync()
    {
        using var semaphore = new SemaphoreSlim(WorkerCount, WorkerCount);
        var tasks = new Task[_planWorkUnits.Length];
        for (var planIndex = 0; planIndex < tasks.Length; planIndex++)
        {
            // 先在生产者侧获得容量，避免未获得槽位的计划提前创建 Task 或进入工厂。
            await semaphore.WaitAsync().ConfigureAwait(false);
            var workUnits = _planWorkUnits[planIndex];
            tasks[planIndex] = Task.Run(
                () =>
                {
                    try
                    {
                        ExecutePlan(workUnits);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static void RunWorker(IEnumerator<int> plans, object planGate)
    {
        while (TryTakePlan(plans, planGate, out var workUnits))
        {
            ExecutePlan(workUnits);
        }
    }

    private static bool TryTakePlan(IEnumerator<int> plans, object planGate, out int workUnits)
    {
        lock (planGate)
        {
            if (!plans.MoveNext())
            {
                workUnits = 0;
                return false;
            }

            workUnits = plans.Current;
            return true;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void ExecutePlan(int workUnits)
    {
        var value = 0x9E3779B97F4A7C15UL ^ (uint)workUnits;
        var iterationCount = checked(workUnits * IterationsPerWorkUnit);
        for (var iteration = 0; iteration < iterationCount; iteration++)
        {
            // 只使用整数运算，避免休眠或显式让出线程掩盖调度本身的成本。
            value ^= value >> 12;
            value ^= value << 25;
            value ^= value >> 27;
            value *= 0x2545F4914F6CDD1DUL;
        }

        // 保留可观察副作用，防止 JIT 删除整个模拟工作负载。
        Volatile.Write(ref s_sink, unchecked((long)value));
    }

    private static int[] CreateWorkload(WorkloadShape shape)
    {
        var result = new int[PlanCount];
        switch (shape)
        {
            case WorkloadShape.UniformShort:
                Array.Fill(result, 1);
                break;
            case WorkloadShape.UniformLong:
                Array.Fill(result, 16);
                break;
            case WorkloadShape.AlternatingLongShort:
                for (var planIndex = 0; planIndex < result.Length; planIndex++)
                {
                    result[planIndex] = planIndex % 2 == 0 ? 1 : 16;
                }

                break;
            case WorkloadShape.SingleLongTail:
                Array.Fill(result, 1);
                result[^1] = 128;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(shape), shape, "Unknown workload shape.");
        }

        return result;
    }
}

/// <summary>
/// 定义 RunGroupPlan 的相对执行时长分布。
/// </summary>
public enum WorkloadShape
{
    /// <summary>
    /// 全部计划都很短，调度和上下文切换成本最容易显现。
    /// </summary>
    UniformShort,

    /// <summary>
    /// 全部计划都较长，用于观察调度成本被实际计算摊薄后的结果。
    /// </summary>
    UniformLong,

    /// <summary>
    /// 长短计划在输入序列中交替出现。
    /// </summary>
    AlternatingLongShort,

    /// <summary>
    /// 最后一个计划远长于其余计划，用于暴露无法通过调度器消除的尾部空闲。
    /// </summary>
    SingleLongTail,
}
