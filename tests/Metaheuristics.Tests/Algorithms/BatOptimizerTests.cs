using System.Reflection;
using Anastasya.Metaheuristics.Algorithms.Bat;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Tests.Algorithms;

/// <summary>
/// 验证蝙蝠算法的配置、初始化、状态所有权、确定性和 Core 集成行为。
/// </summary>
public sealed class BatOptimizerTests
{
    /// <summary>
    /// 验证 Reset 会评估完整初始种群，并从真实评估结果建立历史最优。
    /// </summary>
    [Xunit.Fact]
    public void RunEvaluatesTheInitialPopulationAndCapturesItsBestMember()
    {
        var objective = new FirstCoordinateObjective();
        var optimizer = new BatOptimizer(
            new BatOptimizerOptions { PopulationSize = 4 },
            new SequenceInitializer(4, 3, 2, 1));

        var result = OptimizationRunner.Run(
            CreateProblem(1, objective),
            optimizer,
            StopAfterIterations(0),
            seed: 11,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(4, result.Evaluations);
        Xunit.Assert.Equal(4, objective.EvaluationCount);
        Xunit.Assert.Equal(1, result.BestEvaluation.Objective);
        Xunit.Assert.Equal(1, result.BestPosition[0]);
    }

    /// <summary>
    /// 验证算法使用 Core 比较器支持最大化方向。
    /// </summary>
    [Xunit.Fact]
    public void RunSupportsMaximization()
    {
        var optimizer = new BatOptimizer(
            new BatOptimizerOptions { PopulationSize = 4 },
            new SequenceInitializer(1, 2, 3, 4));
        var problem = new ContinuousProblem(
            [new VariableBounds(-10, 10)],
            new FirstCoordinateObjective(),
            OptimizationDirection.Maximize);

        var result = OptimizationRunner.Run(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 12,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(4, result.BestEvaluation.Objective);
        Xunit.Assert.Equal(4, result.BestPosition[0]);
    }

    /// <summary>
    /// 验证可行解优先于目标值更好但违反约束的候选。
    /// </summary>
    [Xunit.Fact]
    public void RunUsesTheCoreConstraintOrdering()
    {
        var optimizer = new BatOptimizer(
            new BatOptimizerOptions { PopulationSize = 2 },
            new SequenceInitializer(-1, 1));
        var problem = new ContinuousProblem(
            [new VariableBounds(-2, 2)],
            new FirstCoordinateObjective(),
            constraints: [new NegativeValueConstraint()]);

        var result = OptimizationRunner.Run(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 13,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.True(result.BestEvaluation.Constraints.IsFeasible);
        Xunit.Assert.Equal(1, result.BestPosition[0]);
    }

    /// <summary>
    /// 验证相同配置和 seed 在独立实例上产生完全相同的运行结果。
    /// </summary>
    [Xunit.Fact]
    public void RunIsReproducibleForTheSameSeed()
    {
        var options = new BatOptimizerOptions { PopulationSize = 20 };
        var first = OptimizationRunner.Run(
            CreateProblem(4, new SphereObjective()),
            new BatOptimizer(options),
            StopAfterIterations(30),
            seed: 20260822,
            Xunit.TestContext.Current.CancellationToken);
        var second = OptimizationRunner.Run(
            CreateProblem(4, new SphereObjective()),
            new BatOptimizer(options),
            StopAfterIterations(30),
            seed: 20260822,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Equal(first.BestEvaluation, second.BestEvaluation);
        Xunit.Assert.Equal(first.BestPosition, second.BestPosition);
        Xunit.Assert.Equal(first.Evaluations, second.Evaluations);
    }

    /// <summary>
    /// 验证独立优化器实例并发运行时不会共享种群或随机状态。
    /// </summary>
    [Xunit.Fact]
    public async Task IndependentInstancesRemainIsolatedDuringConcurrentRuns()
    {
        var cancellationToken = Xunit.TestContext.Current.CancellationToken;
        var tasks = Enumerable.Range(0, 4)
            .Select(_ => Task.Run(
                () => OptimizationRunner.Run(
                    CreateProblem(3, new SphereObjective()),
                    new BatOptimizer(new BatOptimizerOptions { PopulationSize = 16 }),
                    StopAfterIterations(20),
                    seed: 12345,
                    cancellationToken),
                cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        var expected = results[0];
        Xunit.Assert.All(
            results,
            result =>
            {
                Xunit.Assert.Equal(expected.BestEvaluation, result.BestEvaluation);
                Xunit.Assert.Equal(expected.BestPosition, result.BestPosition);
            });
    }

    /// <summary>
    /// 验证正常的顺序 Reset 会复用双缓冲种群及其主要数组。
    /// </summary>
    [Xunit.Fact]
    public void ResetForRunReusesTheAllocatedWorkspace()
    {
        var optimizer = new BatOptimizer(new BatOptimizerOptions { PopulationSize = 3 });
        var problem = CreateProblem(2, new SphereObjective());

        OptimizationRunner.Run(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 1,
            Xunit.TestContext.Current.CancellationToken);
        var firstPopulationA = GetPopulation(optimizer, "_populationA");
        var firstPopulationB = GetPopulation(optimizer, "_populationB");
        var firstPositions = GetStateVectors(firstPopulationA, "Position");
        var firstVelocities = GetStateVectors(firstPopulationA, "Velocity");

        OptimizationRunner.Run(
            problem,
            optimizer,
            StopAfterIterations(0),
            seed: 2,
            Xunit.TestContext.Current.CancellationToken);
        var secondPopulationA = GetPopulation(optimizer, "_populationA");
        var secondPopulationB = GetPopulation(optimizer, "_populationB");

        Xunit.Assert.Same(firstPopulationA, secondPopulationA);
        Xunit.Assert.Same(firstPopulationB, secondPopulationB);
        Xunit.Assert.Equal(
            firstPositions,
            GetStateVectors(secondPopulationA, "Position"),
            ReferenceEqualityComparer.Instance);
        Xunit.Assert.Equal(
            firstVelocities,
            GetStateVectors(secondPopulationA, "Velocity"),
            ReferenceEqualityComparer.Instance);
    }

    /// <summary>
    /// 验证候选使用本轮新频率，且被拒绝时不会污染源蝙蝠的速度状态。
    /// </summary>
    [Xunit.Fact]
    public void RejectedCandidateUsesNewFrequencyWithoutMutatingSourceVelocity()
    {
        var options = new BatOptimizerOptions
        {
            PopulationSize = 2,
            FrequencyLowerBound = 1,
            FrequencyUpperBound = 1,
            InitialLoudnessLowerBound = 0,
            InitialLoudnessUpperBound = 0,
        };
        var baseline = new BatOptimizer(options, new SequenceInitializer(1, 2));
        OptimizationRunner.Run(
            CreateProblem(1, new IncreasingObjective()),
            baseline,
            StopAfterIterations(0),
            seed: 99,
            Xunit.TestContext.Current.CancellationToken);
        var initialVelocities = GetStateVectors(GetPopulation(baseline, "_populationA"), "Velocity")
            .Select(static velocity => velocity.ToArray())
            .ToArray();

        var exercised = new BatOptimizer(options, new SequenceInitializer(1, 2));
        var result = OptimizationRunner.Run(
            CreateProblem(1, new IncreasingObjective()),
            exercised,
            StopAfterIterations(1),
            seed: 99,
            Xunit.TestContext.Current.CancellationToken);

        // 候选评估值 3、4 都劣于初始值 1、2，因此下一代应仍引用未受污染的源状态。
        var selectedVelocities = GetStateVectors(GetPopulation(exercised, "_populationB"), "Velocity");
        Xunit.Assert.Equal(initialVelocities[0], selectedVelocities[0]);
        Xunit.Assert.Equal(initialVelocities[1], selectedVelocities[1]);

        var rejectedCandidates = GetPopulation(exercised, "_populationA");
        var candidateFrequencies = GetStateVectors(rejectedCandidates, "Frequency");
        Xunit.Assert.All(candidateFrequencies, static frequency => Xunit.Assert.Equal(1, frequency[0]));
        Xunit.Assert.Contains(
            GetStateVectors(rejectedCandidates, "Velocity").Select(static velocity => velocity[0]),
            candidateVelocity => !initialVelocities.Any(initial => initial[0] == candidateVelocity));
        Xunit.Assert.Equal(1, result.BestEvaluation.Objective);
        Xunit.Assert.Equal(1, result.BestPosition[0]);
    }

    /// <summary>
    /// 验证固定 seed 下算法可以改善标准 Sphere 函数，并始终返回有限结果。
    /// </summary>
    [Xunit.Fact]
    public void RunImprovesTheSphereBenchmark()
    {
        var problem = CreateProblem(5, new SphereObjective());
        var initial = OptimizationRunner.Run(
            problem,
            new BatOptimizer(new BatOptimizerOptions { PopulationSize = 40 }),
            StopAfterIterations(0),
            seed: 314159,
            Xunit.TestContext.Current.CancellationToken);
        var optimized = OptimizationRunner.Run(
            problem,
            new BatOptimizer(new BatOptimizerOptions { PopulationSize = 40 }),
            StopAfterIterations(200),
            seed: 314159,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.True(double.IsFinite(optimized.BestEvaluation.Objective));
        Xunit.Assert.True(optimized.BestEvaluation.Objective < initial.BestEvaluation.Objective);
        Xunit.Assert.True(optimized.BestEvaluation.Objective < 0.1);
    }

    /// <summary>
    /// 验证默认均匀初始化拒绝无界问题，而显式初始化器可以为其提供有限起点。
    /// </summary>
    [Xunit.Fact]
    public void DefaultInitializerRequiresFiniteProblemBounds()
    {
        var problem = new ContinuousProblem([VariableBounds.Unbounded], new SphereObjective());

        Xunit.Assert.Throws<InvalidOperationException>(
            () => OptimizationRunner.Run(
                problem,
                new BatOptimizer(new BatOptimizerOptions { PopulationSize = 2 }),
                StopAfterIterations(0),
                seed: 1,
                Xunit.TestContext.Current.CancellationToken));

        var result = OptimizationRunner.Run(
            problem,
            new BatOptimizer(
                new BatOptimizerOptions { PopulationSize = 2 },
                new ConstantInitializer(0.5)),
            StopAfterIterations(0),
            seed: 1,
            Xunit.TestContext.Current.CancellationToken);
        Xunit.Assert.Equal(0.25, result.BestEvaluation.Objective);
    }

    /// <summary>
    /// 验证一个优化器实例不能在已经分配工作区后切换问题维度。
    /// </summary>
    [Xunit.Fact]
    public void ResetForRunRejectsADifferentProblemDimension()
    {
        var optimizer = new BatOptimizer(new BatOptimizerOptions { PopulationSize = 2 });
        OptimizationRunner.Run(
            CreateProblem(1, new SphereObjective()),
            optimizer,
            StopAfterIterations(0),
            seed: 1,
            Xunit.TestContext.Current.CancellationToken);

        Xunit.Assert.Throws<InvalidOperationException>(
            () => OptimizationRunner.Run(
                CreateProblem(2, new SphereObjective()),
                optimizer,
                StopAfterIterations(0),
                seed: 1,
                Xunit.TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// 验证构造函数会拒绝无法安全生成随机状态的配置。
    /// </summary>
    [Xunit.Fact]
    public void ConstructorRejectsInvalidOptions()
    {
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => new BatOptimizer(new BatOptimizerOptions { PopulationSize = 0 }));
        Xunit.Assert.Throws<ArgumentException>(
            () => new BatOptimizer(new BatOptimizerOptions
            {
                VelocityLowerBound = 2,
                VelocityUpperBound = -2,
            }));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => new BatOptimizer(new BatOptimizerOptions { FrequencyLowerBound = -1 }));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => new BatOptimizer(new BatOptimizerOptions { InitialPulseRateUpperBound = 1.1 }));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => new BatOptimizer(new BatOptimizerOptions { LoudnessDecay = 0 }));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(
            () => new BatOptimizer(new BatOptimizerOptions { PulseRateGrowth = double.NaN }));
    }

    private static ContinuousProblem CreateProblem(int dimension, IObjectiveFunction objective)
    {
        return new ContinuousProblem(
            Enumerable.Repeat(new VariableBounds(-5, 5), dimension).ToArray(),
            objective);
    }

    private static OptimizationRunOptions StopAfterIterations(int iterations)
    {
        return new OptimizationRunOptions(StoppingConditions.MaxIterations(iterations));
    }

    private static Array GetPopulation(BatOptimizer optimizer, string fieldName)
    {
        return (Array)(typeof(BatOptimizer)
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(optimizer)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not initialized."));
    }

    private static double[][] GetStateVectors(Array population, string propertyName)
    {
        var result = new double[population.Length][];
        for (var index = 0; index < population.Length; index++)
        {
            var state = population.GetValue(index)
                ?? throw new InvalidOperationException("The bat state was null.");
            result[index] = (double[])(state.GetType()
                .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
                ?.GetValue(state)
                ?? throw new InvalidOperationException($"Property '{propertyName}' was not available."));
        }

        return result;
    }

    /// <summary>
    /// 按调用顺序把预设值写入位置的首个分量。
    /// </summary>
    private sealed class SequenceInitializer(params double[] values) : ICandidateInitializer
    {
        private int _next;

        public void Initialize(Span<double> position, ContinuousProblem problem, Random random)
        {
            position.Clear();
            position[0] = values[_next++ % values.Length];
        }
    }

    /// <summary>
    /// 用固定有限值初始化全部位置分量。
    /// </summary>
    private sealed class ConstantInitializer(double value) : ICandidateInitializer
    {
        public void Initialize(Span<double> position, ContinuousProblem problem, Random random)
        {
            position.Fill(value);
        }
    }

    /// <summary>
    /// 返回首个位置分量并记录调用次数。
    /// </summary>
    private sealed class FirstCoordinateObjective : IObjectiveFunction
    {
        public int EvaluationCount { get; private set; }

        public double Evaluate(ReadOnlySpan<double> position)
        {
            EvaluationCount++;
            return position[0];
        }
    }

    /// <summary>
    /// 每次调用返回递增值，使所有后续候选都劣于初始种群。
    /// </summary>
    private sealed class IncreasingObjective : IObjectiveFunction
    {
        private int _evaluationCount;

        public double Evaluate(ReadOnlySpan<double> position)
        {
            return Interlocked.Increment(ref _evaluationCount);
        }
    }

    /// <summary>
    /// 计算标准 Sphere 函数。
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

    /// <summary>
    /// 把负位置标记为不可行。
    /// </summary>
    private sealed class NegativeValueConstraint : IConstraint
    {
        public double EvaluateViolation(ReadOnlySpan<double> position)
        {
            return position[0] < 0 ? -position[0] : 0;
        }
    }
}
