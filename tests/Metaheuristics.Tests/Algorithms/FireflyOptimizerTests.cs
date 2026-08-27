using System.Reflection;
using Anastasya.Metaheuristics.Algorithms.Firefly;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Tests.Algorithms;

/// <summary>验证萤火虫算法的配置、顺序移动、状态所有权和 Core 集成行为。</summary>
public sealed class FireflyOptimizerTests
{
    [Xunit.Fact]
    public void RunEvaluatesInitialPopulationAndUsesCoreOrdering()
    {
        var objective = new FirstCoordinateObjective();
        var optimizer = new FireflyOptimizer(
            new SequenceInitializer(-1, 1, 2),
            new FireflyOptimizerOptions { PopulationSize = 3 });
        var problem = new ContinuousProblem(
            1,
            objective,
            CandidateRepairs.Clamp(-5, 5),
            constraints: [new NegativeValueConstraint()]);

        var result = ExecuteWithSnapshot(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 1,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(3, result.Evaluations);
        Xunit.Assert.Equal(3, objective.EvaluationCount);
        Xunit.Assert.True(result.BestEvaluation.Constraints.IsFeasible);
        Xunit.Assert.Equal(1, result.BestPosition[0]);
    }

    [Xunit.Fact]
    public void RunSupportsMaximization()
    {
        var optimizer = new FireflyOptimizer(
            new SequenceInitializer(1, 2, 3),
            new FireflyOptimizerOptions { PopulationSize = 3 });
        var problem = new ContinuousProblem(
            1,
            new FirstCoordinateObjective(),
            CandidateRepairs.Clamp(-5, 5),
            OptimizationDirection.Maximize);

        var result = ExecuteWithSnapshot(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 2,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(3, result.BestEvaluation.Objective);
        Xunit.Assert.Equal(3, result.BestPosition[0]);
    }

    [Xunit.Fact]
    public void AdvanceMovesOnlyTowardStrictlyBetterMembersAndRepairsEachMove()
    {
        var repair = new RecordingRepair();
        var optimizer = new FireflyOptimizer(
            new SequenceInitializer(2, 1, 0),
            new FireflyOptimizerOptions
            {
                PopulationSize = 3,
                BaseAttractiveness = 0,
                InitialRandomStep = 0,
            });
        var problem = new ContinuousProblem(1, new FirstCoordinateObjective(), repair);

        var result = ExecuteWithSnapshot(
            problem,
            optimizer,
            StopAfterIterations(1),
            seed: 3,
            Xunit.TestContext.Current.CancellationToken);

        // 初始位置各修复一次；值为 2、1 的萤火虫分别遇到两个、一个严格更优的 attractor。
        Xunit.Assert.Equal(6, repair.CallCount);
        Xunit.Assert.Equal(0, result.BestEvaluation.Objective);
        Xunit.Assert.Equal(0, result.BestPosition[0]);
    }

    [Xunit.Fact]
    public async Task RunIsReproducibleAndIndependentInstancesRemainIsolated()
    {
        var options = new FireflyOptimizerOptions
        {
            PopulationSize = 16,
            DistanceAttenuation = 0.4,
        };
        var first = ExecuteWithSnapshot(
            CreateProblem(3, new SphereObjective()),
            CreateOptimizer(options),
            StopAfterIterations(25),
            seed: 20260827,
            Xunit.TestContext.Current.CancellationToken);
        var second = ExecuteWithSnapshot(
            CreateProblem(3, new SphereObjective()),
            CreateOptimizer(options),
            StopAfterIterations(25),
            seed: 20260827,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(first.BestEvaluation, second.BestEvaluation);
        Xunit.Assert.Equal(first.BestPosition, second.BestPosition);

        var concurrent = Enumerable.Range(0, 3)
            .Select(_ => Task.Run(
                () => ExecuteWithSnapshot(
                    CreateProblem(3, new SphereObjective()),
                    CreateOptimizer(options),
                    StopAfterIterations(25),
                    seed: 20260827,
                    Xunit.TestContext.Current.CancellationToken),
                Xunit.TestContext.Current.CancellationToken))
            .ToArray();
        var concurrentResults = await Task.WhenAll(concurrent);
        Xunit.Assert.All(
            concurrentResults,
            result =>
            {
                Xunit.Assert.Equal(first.BestEvaluation, result.BestEvaluation);
                Xunit.Assert.Equal(first.BestPosition, result.BestPosition);
            });
    }

    [Xunit.Fact]
    public void ResetForRunReusesWorkspaceAndRejectsDifferentDimensions()
    {
        var optimizer = CreateOptimizer(new FireflyOptimizerOptions { PopulationSize = 3 });
        var problem = CreateProblem(2, new SphereObjective());
        ExecuteWithSnapshot(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 4,
            Xunit.TestContext.Current.CancellationToken);
        var firstPopulationA = GetPopulation(optimizer, "_populationA");
        var firstPopulationB = GetPopulation(optimizer, "_populationB");
        var firstPositions = GetVectors(firstPopulationA);

        ExecuteWithSnapshot(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 5,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Same(firstPopulationA, GetPopulation(optimizer, "_populationA"));
        Xunit.Assert.Same(firstPopulationB, GetPopulation(optimizer, "_populationB"));
        Xunit.Assert.Equal(firstPositions, GetVectors(firstPopulationA), ReferenceEqualityComparer.Instance);
        Xunit.Assert.Throws<InvalidOperationException>(
            () => ExecuteWithSnapshot(
                CreateProblem(3, new SphereObjective()),
                optimizer,
                StopAfterIterations(0),
                seed: 6,
                Xunit.TestContext.Current.CancellationToken));
    }

    [Xunit.Fact]
    public void RunImprovesSphereFixture()
    {
        var options = new FireflyOptimizerOptions
        {
            PopulationSize = 24,
            BaseAttractiveness = 0.7,
            DistanceAttenuation = 0.2,
            InitialRandomStep = 0.05,
        };
        var initial = ExecuteWithSnapshot(
            CreateProblem(3, new SphereObjective()),
            CreateOptimizer(options),
            StopAfterIterations(0),
            seed: 314159,
            Xunit.TestContext.Current.CancellationToken);
        var optimized = ExecuteWithSnapshot(
            CreateProblem(3, new SphereObjective()),
            CreateOptimizer(options),
            StopAfterIterations(40),
            seed: 314159,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.True(double.IsFinite(optimized.BestEvaluation.Objective));
        Xunit.Assert.True(optimized.BestEvaluation.Objective < initial.BestEvaluation.Objective);
    }

    [Xunit.Fact]
    public void ConstructorRejectsInvalidOptions()
    {
        Xunit.Assert.Throws<ArgumentNullException>(() => new FireflyOptimizer(null!));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateOptimizer(new FireflyOptimizerOptions { PopulationSize = 0 }));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateOptimizer(new FireflyOptimizerOptions { BaseAttractiveness = double.NaN }));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateOptimizer(new FireflyOptimizerOptions { DistanceAttenuation = -1 }));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateOptimizer(new FireflyOptimizerOptions { RandomStepDecay = 0 }));
    }

    private static FireflyOptimizer CreateOptimizer(FireflyOptimizerOptions? options = null)
    {
        return new FireflyOptimizer(new RandomPositionInitializer(), options);
    }

    private static ContinuousProblem CreateProblem(int dimension, IObjectiveFunction objective)
    {
        return new ContinuousProblem(dimension, objective, CandidateRepairs.Clamp(-5, 5));
    }

    private static OptimizationRunOptions StopAfterIterations(int iterations)
    {
        return new OptimizationRunOptions(StoppingConditions.MaxIterations(iterations));
    }

    private static ExecutionSnapshot ExecuteWithSnapshot(
        ContinuousProblem problem,
        FireflyOptimizer optimizer,
        OptimizationRunOptions options,
        int seed,
        CancellationToken cancellationToken)
    {
        var summary = OptimizationRunner.Execute(problem, optimizer, options, seed, cancellationToken);
        return new ExecutionSnapshot(summary, optimizer.BestPosition.ToArray());
    }

    private static Array GetPopulation(FireflyOptimizer optimizer, string fieldName)
    {
        return (Array)(typeof(FireflyOptimizer)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(optimizer)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not initialized."));
    }

    private static double[][] GetVectors(Array population)
    {
        var result = new double[population.Length][];
        for (var index = 0; index < population.Length; index++)
        {
            var state = population.GetValue(index)
                ?? throw new InvalidOperationException("The firefly state was null.");
            result[index] = (double[])(state.GetType()
                .GetProperty("Position", BindingFlags.Instance | BindingFlags.Public)
                ?.GetValue(state)
                ?? throw new InvalidOperationException("The Position property was not available."));
        }

        return result;
    }

    private sealed class ExecutionSnapshot(OptimizationRunSummary summary, double[] bestPosition)
    {
        public Evaluation BestEvaluation => summary.BestEvaluation;

        public double[] BestPosition => bestPosition;

        public long Evaluations => summary.Evaluations;
    }

    private sealed class SequenceInitializer(params double[] values) : ICandidateInitializer
    {
        private int _next;

        public void Initialize(Span<double> position, Random random)
        {
            position.Clear();
            position[0] = values[_next++ % values.Length];
        }
    }

    private sealed class RandomPositionInitializer : ICandidateInitializer
    {
        public void Initialize(Span<double> position, Random random)
        {
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = (random.NextDouble() * 10) - 5;
            }
        }
    }

    private sealed class RecordingRepair : ICandidateRepair
    {
        public int CallCount { get; private set; }

        public void Repair(Span<double> position, Random random)
        {
            CallCount++;
        }
    }

    private sealed class FirstCoordinateObjective : IObjectiveFunction
    {
        public int EvaluationCount { get; private set; }

        public double Evaluate(ReadOnlySpan<double> position)
        {
            EvaluationCount++;
            return position[0];
        }
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

    private sealed class NegativeValueConstraint : IConstraint
    {
        public double EvaluateViolation(ReadOnlySpan<double> position)
        {
            return position[0] < 0 ? -position[0] : 0;
        }
    }
}
