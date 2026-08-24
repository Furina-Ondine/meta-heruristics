using System.Globalization;
using Anastasya.Metaheuristics.Algorithms.Bat;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;
using Anastasya.Metaheuristics.Examples;
using Anastasya.Metaheuristics.Experiments.Configuration;
using Anastasya.Metaheuristics.Experiments.Execution;

var problem = CreateSphereProblem(dimension: 2);
var optimizer = new BatOptimizer(new BatOptimizerOptions { PopulationSize = 40 });
var options = new OptimizationRunOptions(StoppingConditions.MaxIterations(100))
{
    Trace = new OptimizationTraceOptions(
        OptimizationTraceMode.IterationProgress,
        progressIntervalRatio: 0.1),
};

var result = OptimizationRunner.Execute(problem, optimizer, options, seed: 20260820);
var bestPosition = optimizer.BestPosition.ToArray();

Console.WriteLine(FormattableString.Invariant($"Best objective: {result.BestEvaluation.Objective:F6}"));
Console.WriteLine(
    $"Best position: [{string.Join(", ", bestPosition.Select(
        value => value.ToString("F4", CultureInfo.InvariantCulture)))}]");
Console.WriteLine($"Iterations: {result.Iterations}; evaluations: {result.Evaluations}");

var experiment = new ExperimentDefinition(
[
    new ExperimentCase<BatCaseConfiguration>(
        id: "sphere-small",
        configuration: new BatCaseConfiguration(2, 32, 50),
        repetitions: 4,
        createGroup: static (configuration, _) => CreateGroup(configuration),
        runGroupCount: 1),
    new ExperimentCase<BatCaseConfiguration>(
        id: "sphere-large",
        configuration: new BatCaseConfiguration(8, 40, 100),
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
    Console.WriteLine(FormattableString.Invariant(
        $"{caseResult.CaseId}: {caseResult.Statistics.Counts.Succeeded} succeeded; mean objective = {caseResult.Statistics.BestObjective?.Mean:F6}"));
}

static ContinuousProblem CreateSphereProblem(int dimension)
{
    var bounds = Enumerable
        .Repeat(new VariableBounds(-5, 5), dimension)
        .ToArray();
    return new ContinuousProblem(bounds, new SphereObjective());
}

static ExperimentGroupSetup CreateGroup(BatCaseConfiguration configuration)
{
    return new ExperimentGroupSetup(
        CreateSphereProblem(configuration.Dimension),
        new BatOptimizer(new BatOptimizerOptions
        {
            PopulationSize = configuration.PopulationSize,
        }),
        new OptimizationRunOptions(StoppingConditions.MaxIterations(configuration.Iterations)));
}

namespace Anastasya.Metaheuristics.Examples
{
    /// <summary>
    /// 保存蝙蝠算法实验 Case 的不可变配置。
    /// </summary>
    /// <param name="Dimension">连续问题的维度。</param>
    /// <param name="PopulationSize">每个 RunGroup 持有的蝙蝠数量。</param>
    /// <param name="Iterations">每次 run 的最大迭代次数。</param>
    file sealed record BatCaseConfiguration(
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
}
