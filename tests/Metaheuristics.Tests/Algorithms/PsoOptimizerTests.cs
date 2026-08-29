using System.Reflection;
using Anastasya.Metaheuristics.Algorithms.Pso;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Tests.Algorithms;

/// <summary>验证 PSO 的配置、状态所有权、确定性和 Core 集成行为。</summary>
public sealed class PsoOptimizerTests
{
    [Xunit.Fact]
    public void RunEvaluatesInitialPopulationAndUsesCoreOrdering()
    {
        var objective = new FirstCoordinateObjective();
        var optimizer = new PsoOptimizer(
            new SequenceInitializer(-1, 1, 2),
            new PsoOptimizerOptions { PopulationSize = 3 });
        var problem = new ContinuousProblem(
            1,
            objective,
            CandidateRepairs.Clamp(-5, 5),
            constraints: [new NegativeValueConstraint()]);

        var result = ExecuteWithSnapshot(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 10,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(3, result.Evaluations);
        Xunit.Assert.Equal(3, objective.EvaluationCount);
        Xunit.Assert.True(result.BestEvaluation.Constraints.IsFeasible);
        Xunit.Assert.Equal(1, result.BestPosition[0]);
    }

    [Xunit.Fact]
    public void RunSupportsMaximization()
    {
        var optimizer = new PsoOptimizer(
            new SequenceInitializer(1, 2, 3),
            new PsoOptimizerOptions { PopulationSize = 3 });
        var problem = new ContinuousProblem(
            1,
            new FirstCoordinateObjective(),
            CandidateRepairs.Clamp(-5, 5),
            OptimizationDirection.Maximize);

        var result = ExecuteWithSnapshot(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 11,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(3, result.BestEvaluation.Objective);
        Xunit.Assert.Equal(3, result.BestPosition[0]);
    }

    [Xunit.Fact]
    public async Task RunIsReproducibleAndIndependentInstancesRemainIsolated()
    {
        var options = new PsoOptimizerOptions { PopulationSize = 20 };
        var first = ExecuteWithSnapshot(
            CreateProblem(3, new SphereObjective()),
            CreateOptimizer(options),
            StopAfterIterations(30),
            seed: 20260827,
            Xunit.TestContext.Current.CancellationToken);
        var second = ExecuteWithSnapshot(
            CreateProblem(3, new SphereObjective()),
            CreateOptimizer(options),
            StopAfterIterations(30),
            seed: 20260827,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(first.BestEvaluation, second.BestEvaluation);
        Xunit.Assert.Equal(first.BestPosition, second.BestPosition);

        var concurrent = Enumerable.Range(0, 3)
            .Select(_ => Task.Run(
                () => ExecuteWithSnapshot(
                    CreateProblem(3, new SphereObjective()),
                    CreateOptimizer(options),
                    StopAfterIterations(30),
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
        var optimizer = CreateOptimizer(new PsoOptimizerOptions { PopulationSize = 3 });
        var problem = CreateProblem(2, new SphereObjective());
        ExecuteWithSnapshot(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 1,
            Xunit.TestContext.Current.CancellationToken);
        var firstPopulationA = GetPopulation(optimizer, "_populationA");
        var firstPopulationB = GetPopulation(optimizer, "_populationB");
        var firstPositions = GetVectors(firstPopulationA, "Position");
        var firstVelocities = GetVectors(firstPopulationA, "Velocity");

        ExecuteWithSnapshot(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 2,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Same(firstPopulationA, GetPopulation(optimizer, "_populationA"));
        Xunit.Assert.Same(firstPopulationB, GetPopulation(optimizer, "_populationB"));
        Xunit.Assert.Equal(firstPositions, GetVectors(firstPopulationA, "Position"), ReferenceEqualityComparer.Instance);
        Xunit.Assert.Equal(firstVelocities, GetVectors(firstPopulationA, "Velocity"), ReferenceEqualityComparer.Instance);
        Xunit.Assert.Throws<InvalidOperationException>(
            () => ExecuteWithSnapshot(
                CreateProblem(3, new SphereObjective()),
                optimizer,
                StopAfterIterations(0),
                seed: 3,
                Xunit.TestContext.Current.CancellationToken));
    }

    [Xunit.Fact]
    public void AdvanceRepairsEveryUpdatedPositionAndPreservesAnIndependentBestSnapshot()
    {
        var repair = new RecordingRepair();
        var optimizer = new PsoOptimizer(
            new SequenceInitializer(1, 2),
            new PsoOptimizerOptions
            {
                PopulationSize = 2,
                VelocityLowerBound = 1,
                VelocityUpperBound = 1,
                InitialInertia = 0,
                MinimumInertia = 0,
                CognitiveCoefficient = 0,
                SocialCoefficient = 0,
            });
        var problem = new ContinuousProblem(1, new IncreasingObjective(), repair);

        var result = ExecuteWithSnapshot(
            problem,
            optimizer,
            StopAfterIterations(1),
            seed: 20,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(4, repair.CallCount);
        Xunit.Assert.Equal(1, result.BestEvaluation.Objective);
        Xunit.Assert.Equal(1, result.BestPosition[0]);
    }

    [Xunit.Fact]
    public void AdvancePreservesRandomDrawOrderAndComputesTheExpectedVectorUpdate()
    {
        const int seed = 271828;
        var optimizer = new PsoOptimizer(
            new VectorSequenceInitializer([4, -4, 0], [1, -1, 1]),
            new PsoOptimizerOptions
            {
                PopulationSize = 2,
                VelocityLowerBound = -10,
                VelocityUpperBound = 10,
                InitialInertia = 0,
                MinimumInertia = 0,
                CognitiveCoefficient = 1,
                SocialCoefficient = 1.5,
            });
        var problem = new ContinuousProblem(3, new SphereObjective(), CandidateRepairs.Clamp(-10, 10));
        var random = new Random(seed);
        for (var draw = 0; draw < 7; draw++)
        {
            _ = random.NextDouble();
        }

        var socialRandom = random.NextDouble();

        ExecuteWithSnapshot(
            problem,
            optimizer,
            StopAfterIterations(1),
            seed,
            Xunit.TestContext.Current.CancellationToken);

        var expectedScale = 1.5 * socialRandom;
        var expected = new[]
        {
            4 + (expectedScale * (1 - 4)),
            -4 + (expectedScale * (-1 - -4)),
            expectedScale,
        };
        var targetPopulation = GetPopulation(optimizer, "_populationB");

        Xunit.Assert.Equal(expected, GetVectors(targetPopulation, "Position")[0]);
    }

    [Xunit.Fact]
    public void RunImprovesSphereFixture()
    {
        var options = new PsoOptimizerOptions { PopulationSize = 30 };
        var initial = ExecuteWithSnapshot(
            CreateProblem(4, new SphereObjective()),
            CreateOptimizer(options),
            StopAfterIterations(0),
            seed: 314159,
            Xunit.TestContext.Current.CancellationToken);
        var optimized = ExecuteWithSnapshot(
            CreateProblem(4, new SphereObjective()),
            CreateOptimizer(options),
            StopAfterIterations(80),
            seed: 314159,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.True(double.IsFinite(optimized.BestEvaluation.Objective));
        Xunit.Assert.True(optimized.BestEvaluation.Objective < initial.BestEvaluation.Objective);
    }

    [Xunit.Fact]
    public void ConstructorRejectsInvalidOptions()
    {
        Xunit.Assert.Throws<ArgumentNullException>(() => new PsoOptimizer(null!));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateOptimizer(new PsoOptimizerOptions { PopulationSize = 0 }));
        Xunit.Assert.Throws<ArgumentException>(
            () => CreateOptimizer(new PsoOptimizerOptions { VelocityLowerBound = 1, VelocityUpperBound = -1 }));
        Xunit.Assert.Throws<ArgumentException>(
            () => CreateOptimizer(new PsoOptimizerOptions { InitialInertia = 0.2, MinimumInertia = 0.3 }));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateOptimizer(new PsoOptimizerOptions { InertiaDecay = 0 }));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateOptimizer(new PsoOptimizerOptions { CognitiveCoefficient = double.NaN }));
    }

    private static PsoOptimizer CreateOptimizer(PsoOptimizerOptions? options = null)
    {
        return new PsoOptimizer(new RandomPositionInitializer(), options);
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
        PsoOptimizer optimizer,
        OptimizationRunOptions options,
        int seed,
        CancellationToken cancellationToken)
    {
        var summary = OptimizationRunner.Execute(problem, optimizer, options, seed, cancellationToken);
        return new ExecutionSnapshot(summary, optimizer.BestPosition.ToArray());
    }

    private static Array GetPopulation(PsoOptimizer optimizer, string fieldName)
    {
        return (Array)(typeof(PsoOptimizer)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(optimizer)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not initialized."));
    }

    private static double[][] GetVectors(Array population, string propertyName)
    {
        var result = new double[population.Length][];
        for (var index = 0; index < population.Length; index++)
        {
            var state = population.GetValue(index)
                ?? throw new InvalidOperationException("The PSO state was null.");
            result[index] = (double[])(state.GetType()
                .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
                ?.GetValue(state)
                ?? throw new InvalidOperationException($"Property '{propertyName}' was not available."));
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

    private sealed class VectorSequenceInitializer(params double[][] values) : ICandidateInitializer
    {
        private int _next;

        public void Initialize(Span<double> position, Random random)
        {
            values[_next++ % values.Length].AsSpan().CopyTo(position);
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

    private sealed class IncreasingObjective : IObjectiveFunction
    {
        private int _evaluationCount;

        public double Evaluate(ReadOnlySpan<double> position)
        {
            return Interlocked.Increment(ref _evaluationCount);
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
