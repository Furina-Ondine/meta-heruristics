using System.Globalization;
using Anastasya.Metaheuristics.Core.Comparison;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;
using Anastasya.Metaheuristics.Examples;
using Anastasya.Metaheuristics.Experiments.Configuration;
using Anastasya.Metaheuristics.Experiments.Execution;

var problem = new ContinuousProblem(
    [new VariableBounds(-5, 5), new VariableBounds(-5, 5)],
    new SphereObjective());
var optimizer = new RandomSearchOptimizer(samplesPerIteration: 32);
var options = new OptimizationRunOptions(StoppingConditions.MaxIterations(100))
{
    Trace = new OptimizationTraceOptions(
        OptimizationTraceMode.IterationProgress,
        progressIntervalRatio: 0.1),
};

var result = OptimizationRunner.Run(problem, optimizer, options, seed: 20260820);

Console.WriteLine(FormattableString.Invariant($"Best objective: {result.BestEvaluation.Objective:F6}"));
Console.WriteLine(
    $"Best position: [{string.Join(", ", result.BestPosition.Select(
        x => x.ToString("F4", CultureInfo.InvariantCulture)))}]");
Console.WriteLine($"Iterations: {result.Iterations}; evaluations: {result.Evaluations}");

var experiment = new ExperimentDefinition(
[
    new ExperimentCase<RandomSearchCaseConfiguration>(
        id: "sphere-small",
        configuration: new RandomSearchCaseConfiguration(2, 16, 50),
        repetitions: 4,
        createGroup: static (configuration, _) => CreateGroup(configuration),
        runGroupCount: 1),
    new ExperimentCase<RandomSearchCaseConfiguration>(
        id: "sphere-large",
        configuration: new RandomSearchCaseConfiguration(8, 32, 100),
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

static ExperimentGroupSetup CreateGroup(RandomSearchCaseConfiguration configuration)
{
    var bounds = Enumerable
        .Repeat(new VariableBounds(-5, 5), configuration.Dimension)
        .ToArray();
    return new ExperimentGroupSetup(
        new ContinuousProblem(bounds, new SphereObjective()),
        new RandomSearchOptimizer(configuration.SamplesPerIteration),
        new OptimizationRunOptions(StoppingConditions.MaxIterations(configuration.Iterations)));
}

namespace Anastasya.Metaheuristics.Examples
{
    /// <summary>
    /// 保存随机搜索实验 Case 的不可变配置。
    /// </summary>
    /// <param name="Dimension">连续问题的维度。</param>
    /// <param name="SamplesPerIteration">每次迭代评估的随机候选数量。</param>
    /// <param name="Iterations">每次 run 的最大迭代次数。</param>
    file sealed record RandomSearchCaseConfiguration(
        int Dimension,
        int SamplesPerIteration,
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

    /// <summary>
    /// 在顺序运行之间复用候选数组的有状态随机搜索优化器。
    /// </summary>
    file sealed class RandomSearchOptimizer : IOptimizer
    {
        private readonly int _samplesPerIteration;
        private double[]? _bestPosition;
        private double[]? _candidate;
        private OptimizationRunContext? _context;

        /// <summary>
        /// 创建随机搜索优化器。
        /// </summary>
        /// <param name="samplesPerIteration">每次迭代生成并评估的候选数量。</param>
        /// <exception cref="ArgumentOutOfRangeException">样本数量不是正数。</exception>
        public RandomSearchOptimizer(int samplesPerIteration)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(samplesPerIteration);
            _samplesPerIteration = samplesPerIteration;
        }

        /// <summary>
        /// 获取当前运行找到的最优位置。
        /// </summary>
        public ReadOnlySpan<double> BestPosition => _bestPosition
            ?? throw new InvalidOperationException("The optimizer has not been reset for a run.");

        /// <summary>
        /// 获取当前最优位置的评估结果。
        /// </summary>
        public Evaluation BestEvaluation { get; private set; }

        /// <summary>
        /// 复用或首次创建候选数组，并使用当前 run 的随机流重新初始化搜索。
        /// </summary>
        /// <param name="context">当前 run 独占的问题、随机数和评估上下文。</param>
        public void ResetForRun(OptimizationRunContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (_bestPosition is null)
            {
                _bestPosition = new double[context.Problem.Dimension];
                _candidate = new double[context.Problem.Dimension];
            }
            else if (_bestPosition.Length != context.Problem.Dimension)
            {
                throw new InvalidOperationException("An optimizer instance cannot be reused with a different dimension.");
            }

            _context = context;
            FillCandidate(_bestPosition);
            BestEvaluation = context.Evaluate(_bestPosition);
        }

        /// <summary>
        /// 生成本轮候选并保留其中更优的结果。
        /// </summary>
        public void Advance()
        {
            var context = _context
                ?? throw new InvalidOperationException("The optimizer has not been reset for a run.");
            var candidate = _candidate!;
            var bestPosition = _bestPosition!;

            // 所有评估都经由 Context 完成，以统一处理计数、取消和数值校验。
            for (var sample = 0; sample < _samplesPerIteration; sample++)
            {
                FillCandidate(candidate);
                var evaluation = context.Evaluate(candidate);
                if (!EvaluationComparer.IsBetter(
                        evaluation,
                        BestEvaluation,
                        context.Problem.Direction))
                {
                    continue;
                }

                candidate.CopyTo(bestPosition, 0);
                BestEvaluation = evaluation;
            }
        }

        /// <summary>
        /// 使用运行专属随机流在每一维的有限边界内均匀生成候选位置。
        /// </summary>
        /// <param name="candidate">要就地填充的位置缓冲区。</param>
        private void FillCandidate(Span<double> candidate)
        {
            for (var i = 0; i < candidate.Length; i++)
            {
                var bounds = _context!.Problem.Bounds[i];
                var lower = bounds.LowerBound
                            ?? throw new InvalidOperationException("This example requires finite lower bounds.");
                var upper = bounds.UpperBound
                            ?? throw new InvalidOperationException("This example requires finite upper bounds.");
                candidate[i] = lower + ((upper - lower) * _context.Random.NextDouble());
            }
        }
    }
}
