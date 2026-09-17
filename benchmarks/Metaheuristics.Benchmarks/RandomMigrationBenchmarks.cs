using System;
using Anastasya.Metaheuristics.Algorithms.Bat;
using Anastasya.Metaheuristics.Algorithms.Cuckoo;
using Anastasya.Metaheuristics.Algorithms.Firefly;
using Anastasya.Metaheuristics.Algorithms.Pso;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;
using Anastasya.Metaheuristics.Core.Randomness;
using BenchmarkDotNet.Attributes;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>
/// 测量 SPEC-0009 使用的 Bat 端到端执行。
/// </summary>
[MemoryDiagnoser]
public class BatRandomMigrationBenchmarks
{
    private readonly ICandidateInitializer _initializer = new RandomPositionInitializer();
    private ContinuousProblem _problem = null!;
    private BatOptimizerOptions _options = null!;
    private BatOptimizer _reusedOptimizer = null!;
    private OptimizationRunOptions _runOptions = null!;

    [Params(32, 128)]
    public int Dimension { get; set; }

    [Params(10, 100)]
    public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _problem = new ContinuousProblem(Dimension, new SphereObjective(), CandidateRepairs.Clamp(-5, 5));
        _options = new BatOptimizerOptions { PopulationSize = 64 };
        _runOptions = new OptimizationRunOptions(StoppingConditions.MaxIterations(Iterations));
        _reusedOptimizer = new BatOptimizer(_initializer, _options);
        OptimizationRunner.Execute(_problem, _reusedOptimizer, _runOptions, seed: ulong.MaxValue);
    }

    [Benchmark]
    public double FirstRun()
    {
        var optimizer = new BatOptimizer(_initializer, _options);
        var summary = OptimizationRunner.Execute(_problem, optimizer, _runOptions, seed: 20260905);
        return summary.BestEvaluation.Objective + (summary.Evaluations * double.Epsilon);
    }

    [Benchmark]
    public double ReusedRun()
    {
        var summary = OptimizationRunner.Execute(
            _problem,
            _reusedOptimizer,
            _runOptions,
            seed: 20260905);
        return summary.BestEvaluation.Objective + (summary.Evaluations * double.Epsilon);
    }

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
        public void Initialize(Span<double> position, RandomSource random)
        {
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = (random.NextDouble() * 20) - 10;
            }
        }
    }
}

/// <summary>
/// 测量 SPEC-0009 使用的 Cuckoo 端到端执行。
/// </summary>
[MemoryDiagnoser]
public class CuckooRandomMigrationBenchmarks
{
    private readonly ICandidateInitializer _initializer = new RandomPositionInitializer();
    private ContinuousProblem _problem = null!;
    private CuckooOptimizerOptions _options = null!;
    private CuckooOptimizer _reusedOptimizer = null!;
    private OptimizationRunOptions _runOptions = null!;

    [Params(32, 128)]
    public int Dimension { get; set; }

    [Params(10, 100)]
    public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _problem = new ContinuousProblem(Dimension, new SphereObjective(), CandidateRepairs.Clamp(-5, 5));
        _options = new CuckooOptimizerOptions { PopulationSize = 64 };
        _runOptions = new OptimizationRunOptions(StoppingConditions.MaxIterations(Iterations));
        _reusedOptimizer = new CuckooOptimizer(_initializer, _options);
        OptimizationRunner.Execute(_problem, _reusedOptimizer, _runOptions, seed: ulong.MaxValue);
    }

    [Benchmark]
    public double FirstRun()
    {
        var optimizer = new CuckooOptimizer(_initializer, _options);
        var summary = OptimizationRunner.Execute(_problem, optimizer, _runOptions, seed: 20260905);
        return summary.BestEvaluation.Objective + (summary.Evaluations * double.Epsilon);
    }

    [Benchmark]
    public double ReusedRun()
    {
        var summary = OptimizationRunner.Execute(
            _problem,
            _reusedOptimizer,
            _runOptions,
            seed: 20260905);
        return summary.BestEvaluation.Objective + (summary.Evaluations * double.Epsilon);
    }

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
        public void Initialize(Span<double> position, RandomSource random)
        {
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = (random.NextDouble() * 20) - 10;
            }
        }
    }
}

/// <summary>
/// 测量 PSO 的实际生产端到端执行，用于跨源提交比较 H/A/C。
/// </summary>
[MemoryDiagnoser]
public class PsoRandomMigrationBenchmarks
{
    private readonly ICandidateInitializer _initializer = new RandomPositionInitializer();
    private ContinuousProblem _problem = null!;
    private PsoOptimizerOptions _options = null!;
    private PsoOptimizer _reusedOptimizer = null!;
    private OptimizationRunOptions _runOptions = null!;

    [Params(32, 128)]
    public int Dimension { get; set; }

    [Params(10, 100)]
    public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _problem = new ContinuousProblem(Dimension, new SphereObjective(), CandidateRepairs.Clamp(-5, 5));
        _options = new PsoOptimizerOptions { PopulationSize = 64 };
        _runOptions = new OptimizationRunOptions(StoppingConditions.MaxIterations(Iterations));
        _reusedOptimizer = new PsoOptimizer(_initializer, _options);
        OptimizationRunner.Execute(_problem, _reusedOptimizer, _runOptions, seed: ulong.MaxValue);
    }

    [Benchmark]
    public double FirstRun()
    {
        var optimizer = new PsoOptimizer(_initializer, _options);
        var summary = OptimizationRunner.Execute(_problem, optimizer, _runOptions, seed: 20260905);
        return summary.BestEvaluation.Objective + (summary.Evaluations * double.Epsilon);
    }

    [Benchmark]
    public double ReusedRun()
    {
        var summary = OptimizationRunner.Execute(
            _problem,
            _reusedOptimizer,
            _runOptions,
            seed: 20260905);
        return summary.BestEvaluation.Objective + (summary.Evaluations * double.Epsilon);
    }

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
        public void Initialize(Span<double> position, RandomSource random)
        {
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = (random.NextDouble() * 20) - 10;
            }
        }
    }
}

/// <summary>
/// 测量 Firefly 的实际生产端到端执行，用于跨源提交比较 H/A/C。
/// </summary>
[MemoryDiagnoser]
public class FireflyRandomMigrationBenchmarks
{
    private readonly ICandidateInitializer _initializer = new RandomPositionInitializer();
    private ContinuousProblem _problem = null!;
    private FireflyOptimizerOptions _options = null!;
    private FireflyOptimizer _reusedOptimizer = null!;
    private OptimizationRunOptions _runOptions = null!;

    [Params(32, 128)]
    public int Dimension { get; set; }

    [Params(10, 100)]
    public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _problem = new ContinuousProblem(Dimension, new SphereObjective(), CandidateRepairs.Clamp(-5, 5));
        _options = new FireflyOptimizerOptions { PopulationSize = 64 };
        _runOptions = new OptimizationRunOptions(StoppingConditions.MaxIterations(Iterations));
        _reusedOptimizer = new FireflyOptimizer(_initializer, _options);
        OptimizationRunner.Execute(_problem, _reusedOptimizer, _runOptions, seed: ulong.MaxValue);
    }

    [Benchmark]
    public double FirstRun()
    {
        var optimizer = new FireflyOptimizer(_initializer, _options);
        var summary = OptimizationRunner.Execute(_problem, optimizer, _runOptions, seed: 20260905);
        return summary.BestEvaluation.Objective + (summary.Evaluations * double.Epsilon);
    }

    [Benchmark]
    public double ReusedRun()
    {
        var summary = OptimizationRunner.Execute(
            _problem,
            _reusedOptimizer,
            _runOptions,
            seed: 20260905);
        return summary.BestEvaluation.Objective + (summary.Evaluations * double.Epsilon);
    }

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
        public void Initialize(Span<double> position, RandomSource random)
        {
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = (random.NextDouble() * 20) - 10;
            }
        }
    }
}
