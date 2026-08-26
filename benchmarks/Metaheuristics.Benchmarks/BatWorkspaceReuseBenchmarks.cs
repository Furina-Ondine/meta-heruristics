using System;
using System.Linq;
using Anastasya.Metaheuristics.Algorithms.Bat;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;
using BenchmarkDotNet.Attributes;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>
/// 比较每个 run 新建蝙蝠优化器与 RunGroup 内复用工作区的分配和执行成本。
/// </summary>
[MemoryDiagnoser]
public class BatWorkspaceReuseBenchmarks
{
    private BatOptimizer _groupOptimizer = null!;
    private readonly ICandidateInitializer _initializer = new RandomPositionInitializer();
    private BatOptimizerOptions _optimizerOptions = null!;
    private OptimizationRunOptions _runOptions = null!;
    private ContinuousProblem _problem = null!;

    /// <summary>
    /// 获取或设置标准测试问题的维度。
    /// </summary>
    [Params(32)]
    public int Dimension { get; set; }

    /// <summary>
    /// 获取或设置每个优化器的蝙蝠数量。
    /// </summary>
    [Params(64)]
    public int PopulationSize { get; set; }

    /// <summary>
    /// 获取或设置一次基准操作包含的顺序 run 数量。
    /// </summary>
    [Params(8)]
    public int Repetitions { get; set; }

    /// <summary>
    /// 创建共享问题、运行选项和已经分配工作区的 Group Optimizer。
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _problem = new ContinuousProblem(Dimension, new SphereObjective(), CandidateRepairs.Clamp(-5, 5));
        _optimizerOptions = new BatOptimizerOptions { PopulationSize = PopulationSize };
        _runOptions = new OptimizationRunOptions(StoppingConditions.MaxIterations(5));
        _groupOptimizer = new BatOptimizer(_initializer, _optimizerOptions);

        // 预热一次以把工作区分配排除在复用路径的测量之外。
        OptimizationRunner.Execute(_problem, _groupOptimizer, _runOptions, seed: -1);
    }

    /// <summary>
    /// 测量每次运行创建和初始化全新 Optimizer 的基线路径。
    /// </summary>
    /// <returns>用于阻止运行结果被消除的目标值校验和。</returns>
    [Benchmark(Baseline = true)]
    public double NewOptimizerPerRun()
    {
        var checksum = 0.0;
        for (var repetition = 0; repetition < Repetitions; repetition++)
        {
            var optimizer = new BatOptimizer(_initializer, _optimizerOptions);
            checksum += OptimizationRunner.Execute(
                _problem,
                optimizer,
                _runOptions,
                repetition).BestEvaluation.Objective;
        }

        return checksum;
    }

    /// <summary>
    /// 测量一个 RunGroup 内顺序复用同一 Optimizer 工作区的路径。
    /// </summary>
    /// <returns>用于阻止运行结果被消除的目标值校验和。</returns>
    [Benchmark]
    public double ReuseOptimizerWithinGroup()
    {
        var checksum = 0.0;
        for (var repetition = 0; repetition < Repetitions; repetition++)
        {
            checksum += OptimizationRunner.Execute(
                _problem,
                _groupOptimizer,
                _runOptions,
                repetition).BestEvaluation.Objective;
        }

        return checksum;
    }

    /// <summary>
    /// 计算标准 Sphere 目标函数。
    /// </summary>
    private sealed class SphereObjective : IObjectiveFunction
    {
        public double Evaluate(ReadOnlySpan<double> position)
        {
            var result = 0.0;
            foreach (var value in position)
            {
                result += value * value;
            }

            return result;
        }
    }

    private sealed class RandomPositionInitializer : ICandidateInitializer
    {
        public void Initialize(Span<double> position, Random random)
        {
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = (random.NextDouble() * 20) - 10;
            }
        }
    }
}
