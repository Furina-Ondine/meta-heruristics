using System.Globalization;
using Anastasya.Metaheuristics.Algorithms.Bat;
using Anastasya.Metaheuristics.Algorithms.Cuckoo;
using Anastasya.Metaheuristics.Algorithms.Firefly;
using Anastasya.Metaheuristics.Algorithms.Pso;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;
using Anastasya.Metaheuristics.Examples;
using Anastasya.Metaheuristics.Experiments.Configuration;
using Anastasya.Metaheuristics.Experiments.Execution;

var options = new OptimizationRunOptions(StoppingConditions.MaxIterations(100))
{
    Trace = new OptimizationTraceOptions(OptimizationTraceMode.IterationProgress, progressIntervalRatio: 0.1),
};

foreach (var (name, optimizer) in CreateSingleRunOptimizers(populationSize: 40))
{
    var result = OptimizationRunner.Execute(CreateSphereProblem(dimension: 2), optimizer, options, seed: 20260820);
    var bestPosition = optimizer.BestPosition.ToArray();
    Console.WriteLine(FormattableString.Invariant($"{name}: best objective = {result.BestEvaluation.Objective:F6}"));
    Console.WriteLine(
        $"  Best position: [{string.Join(", ", bestPosition.Select(
            value => value.ToString("F4", CultureInfo.InvariantCulture)))}]");
    Console.WriteLine($"  Iterations: {result.Iterations}; evaluations: {result.Evaluations}");
}

var experiment = new ExperimentDefinition(
[
    new ExperimentCase<AlgorithmCaseConfiguration>(
        id: "sphere-bat",
        configuration: new AlgorithmCaseConfiguration(OptimizerKind.Bat, 2, 32, 50),
        repetitions: 4,
        createGroup: static (configuration, _) => CreateGroup(configuration),
        runGroupCount: 1),
    new ExperimentCase<AlgorithmCaseConfiguration>(
        id: "sphere-pso",
        configuration: new AlgorithmCaseConfiguration(OptimizerKind.Pso, 2, 32, 50),
        repetitions: 4,
        createGroup: static (configuration, _) => CreateGroup(configuration),
        runGroupCount: 1),
    new ExperimentCase<AlgorithmCaseConfiguration>(
        id: "sphere-firefly",
        configuration: new AlgorithmCaseConfiguration(OptimizerKind.Firefly, 2, 32, 50),
        repetitions: 4,
        createGroup: static (configuration, _) => CreateGroup(configuration),
        runGroupCount: 1),
    new ExperimentCase<AlgorithmCaseConfiguration>(
        id: "sphere-cuckoo",
        configuration: new AlgorithmCaseConfiguration(OptimizerKind.Cuckoo, 2, 32, 50),
        repetitions: 4,
        createGroup: static (configuration, _) => CreateGroup(configuration),
        runGroupCount: 2),
]);

var experimentResult = await ExperimentRunner.RunAsync(
    experiment,
    new ExperimentExecutionOptions
    {
        BaseSeed = 20260820,
        GlobalMaxConcurrency = 2,
    });

Console.WriteLine($"Experiment status: {experimentResult.Status}");
foreach (var caseResult in experimentResult.Cases)
{
    // 不同 Case 的相同 Repetition 使用 experimentResult.Seeds 中的同一个 seed。
    var mean = caseResult.Statistics.BestObjective?.Mean;
    var meanText = mean is double value
        ? value.ToString("F6", CultureInfo.InvariantCulture)
        : "undefined";
    Console.WriteLine(
        $"{caseResult.CaseId}: {caseResult.Statistics.Counts.Succeeded} succeeded; mean objective = {meanText}");
}

static ContinuousProblem CreateSphereProblem(int dimension)
{
    return new ContinuousProblem(dimension, new SphereObjective(), CandidateRepairs.Clamp(-5, 5));
}

static IReadOnlyList<(string Name, IOptimizer Optimizer)> CreateSingleRunOptimizers(int populationSize)
{
    return
    [
        ("Bat", CreateOptimizer(OptimizerKind.Bat, populationSize)),
        ("PSO", CreateOptimizer(OptimizerKind.Pso, populationSize)),
        ("Firefly", CreateOptimizer(OptimizerKind.Firefly, populationSize)),
        ("Cuckoo", CreateOptimizer(OptimizerKind.Cuckoo, populationSize)),
    ];
}

static ExperimentGroupSetup CreateGroup(AlgorithmCaseConfiguration configuration)
{
    return new ExperimentGroupSetup(
        CreateSphereProblem(configuration.Dimension),
        CreateOptimizer(configuration.OptimizerKind, configuration.PopulationSize),
        new OptimizationRunOptions(StoppingConditions.MaxIterations(configuration.Iterations)));
}

static IOptimizer CreateOptimizer(OptimizerKind optimizerKind, int populationSize)
{
    return optimizerKind switch
    {
        OptimizerKind.Bat => new BatOptimizer(
            new RandomPositionInitializer(),
            new BatOptimizerOptions { PopulationSize = populationSize }),
        OptimizerKind.Pso => new PsoOptimizer(
            new RandomPositionInitializer(),
            new PsoOptimizerOptions { PopulationSize = populationSize }),
        OptimizerKind.Firefly => new FireflyOptimizer(
            new RandomPositionInitializer(),
            new FireflyOptimizerOptions { PopulationSize = populationSize }),
        OptimizerKind.Cuckoo => new CuckooOptimizer(
            new RandomPositionInitializer(),
            new CuckooOptimizerOptions { PopulationSize = populationSize }),
        _ => throw new ArgumentOutOfRangeException(nameof(optimizerKind)),
    };
}

namespace Anastasya.Metaheuristics.Examples
{
    /// <summary>
    /// 标识 Example 中显式组装的内置连续优化器。
    /// </summary>
    file enum OptimizerKind
    {
        Bat,
        Pso,
        Firefly,
        Cuckoo,
    }

    /// <summary>保存可替换优化器实验 Case 的不可变配置。</summary>
    /// <param name="OptimizerKind">显式选择的内置连续优化器。</param>
    /// <param name="Dimension">连续问题的维度。</param>
    /// <param name="PopulationSize">每个 RunGroup 持有的候选数量。</param>
    /// <param name="Iterations">每次 run 的最大迭代次数。</param>
    file sealed record AlgorithmCaseConfiguration(
        OptimizerKind OptimizerKind,
        int Dimension,
        int PopulationSize,
        int Iterations);

    /// <summary>
    /// 计算连续位置平方和的最小化目标函数。
    /// </summary>
    file sealed class SphereObjective : IObjectiveFunction
    {
        /// <summary>
        /// 返回候选位置各维度平方和。
        /// </summary>
        /// <param name="position">待计算的候选位置。</param>
        /// <returns>候选位置的平方和。</returns>
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

    /// <summary>生成待默认 Clamp Repair 处理的随机初始位置。</summary>
    file sealed class RandomPositionInitializer : ICandidateInitializer
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
