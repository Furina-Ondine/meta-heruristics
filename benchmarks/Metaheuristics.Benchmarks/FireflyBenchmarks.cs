using System.Numerics.Tensors;
using System.Runtime.Intrinsics;
using Anastasya.Metaheuristics.Algorithms;
using Anastasya.Metaheuristics.Algorithms.Firefly;
using Anastasya.Metaheuristics.Core.Comparison;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;
using BenchmarkDotNet.Attributes;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>比较萤火虫单次吸引移动的标量和两个仅供测量的 SIMD 候选。</summary>
/// <remarks>
/// 内核和完整生命周期仅用于同机候选比较；生产路径由 SPEC-0005 的性能门槛决定。
/// </remarks>
[MemoryDiagnoser]
public class FireflyMoveBenchmarks
{
    private double[] _attractorPosition = null!;
    private double[] _randomWalk = null!;
    private double[] _scalarPosition = null!;
    private double[] _tensorPosition = null!;
    private double[] _vectorOpsPosition = null!;
    private double[] _difference = null!;

    /// <summary>获取或设置萤火虫位置维度。</summary>
    [Params(2, 7, 8, 15, 16, 31, 32, 33, 127, 128, 129)]
    public int Dimension { get; set; }

    /// <summary>获取当前运行时固定宽度向量的可用性和宽度，以写入基准报告。</summary>
    [ParamsSource(nameof(SimdConfigurations))]
    public string SimdConfiguration { get; set; } = null!;

    /// <summary>为基准报告提供当前运行时唯一的固定宽度 SIMD 配置。</summary>
    public static IEnumerable<string> SimdConfigurations =>
    [
        $"V512={Vector512.IsHardwareAccelerated}/{Vector512<double>.Count}; "
        + $"V256={Vector256.IsHardwareAccelerated}/{Vector256<double>.Count}; "
        + $"V128={Vector128.IsHardwareAccelerated}/{Vector128<double>.Count}",
    ];

    /// <summary>创建相同初始位置、吸引者和预先生成随机步长的候选路径。</summary>
    [GlobalSetup]
    public void Setup()
    {
        _attractorPosition = new double[Dimension];
        _randomWalk = new double[Dimension];
        _scalarPosition = new double[Dimension];
        _tensorPosition = new double[Dimension];
        _vectorOpsPosition = new double[Dimension];
        _difference = new double[Dimension];

        for (var index = 0; index < Dimension; index++)
        {
            var position = (index % 11) - 5;
            _scalarPosition[index] = position;
            _tensorPosition[index] = position;
            _vectorOpsPosition[index] = position;
            _attractorPosition[index] = position + ((index % 5) - 2);
            _randomWalk[index] = ((index % 7) - 3) * 0.01;
        }
    }

    /// <summary>测量标量平方距离、吸引度和逐维位置更新作为基线。</summary>
    [Benchmark(Baseline = true)]
    public void ScalarMove()
    {
        var distanceSquared = 0.0;
        for (var index = 0; index < Dimension; index++)
        {
            var delta = _scalarPosition[index] - _attractorPosition[index];
            distanceSquared += delta * delta;
        }

        var attractiveness = 0.7 * Math.Exp(-0.2 * distanceSquared);
        for (var index = 0; index < Dimension; index++)
        {
            _scalarPosition[index] += (attractiveness
                * (_attractorPosition[index] - _scalarPosition[index])) + _randomWalk[index];
        }
    }

    /// <summary>测量只由 TensorPrimitives 组合构成的距离与位置更新候选。</summary>
    [Benchmark]
    public void TensorPrimitivesMove()
    {
        TensorPrimitives.Subtract(_tensorPosition, _attractorPosition, _difference);
        var distanceSquared = TensorPrimitives.Dot(_difference, _difference);
        var attractiveness = 0.7 * Math.Exp(-0.2 * distanceSquared);

        TensorPrimitives.Subtract(_attractorPosition, _tensorPosition, _difference);
        TensorPrimitives.MultiplyAdd(_difference, attractiveness, _randomWalk, _difference);
        TensorPrimitives.Add(_tensorPosition, _difference, _tensorPosition);
    }

    /// <summary>测量固定宽度 512→256→128→标量级联的 VectorOps 候选。</summary>
    [Benchmark]
    public void VectorOpsMove()
    {
        var distanceSquared = VectorOps.DistanceSquared(
            _vectorOpsPosition,
            _attractorPosition);
        var attractiveness = 0.7 * Math.Exp(-0.2 * distanceSquared);
        VectorOps.UpdateFireflyPosition(
            _vectorOpsPosition,
            _attractorPosition,
            _randomWalk,
            attractiveness,
            _vectorOpsPosition);
    }
}

/// <summary>比较萤火虫标量、TensorPrimitives 和 VectorOps 的完整 Advance 生命周期。</summary>
[MemoryDiagnoser]
public class FireflyAdvanceBenchmarks
{
    private ContinuousProblem _problem = null!;
    private FireflyBenchmarkOptimizer _scalarOptimizer = null!;
    private FireflyBenchmarkOptimizer _tensorPrimitivesOptimizer = null!;
    private FireflyBenchmarkOptimizer _vectorOpsOptimizer = null!;
    private OptimizationRunOptions _runOptions = null!;

    /// <summary>获取或设置问题维度。</summary>
    [Params(32, 128)]
    public int Dimension { get; set; }

    /// <summary>获取当前运行时固定宽度向量的可用性和宽度，以写入基准报告。</summary>
    [ParamsSource(nameof(SimdConfigurations))]
    public string SimdConfiguration { get; set; } = null!;

    /// <summary>为基准报告提供当前运行时唯一的固定宽度 SIMD 配置。</summary>
    public static IEnumerable<string> SimdConfigurations =>
    [
        $"V512={Vector512.IsHardwareAccelerated}/{Vector512<double>.Count}; "
        + $"V256={Vector256.IsHardwareAccelerated}/{Vector256<double>.Count}; "
        + $"V128={Vector128.IsHardwareAccelerated}/{Vector128<double>.Count}",
    ];

    /// <summary>创建三个使用相同问题、配置和随机种子的可复用完整生命周期。</summary>
    [GlobalSetup]
    public void Setup()
    {
        _problem = new ContinuousProblem(Dimension, new SphereObjective(), CandidateRepairs.Clamp(-5, 5));
        var options = new FireflyOptimizerOptions { PopulationSize = 64 };
        _scalarOptimizer = new FireflyBenchmarkOptimizer(
            new RandomPositionInitializer(),
            options,
            FireflyUpdatePath.Scalar);
        _tensorPrimitivesOptimizer = new FireflyBenchmarkOptimizer(
            new RandomPositionInitializer(),
            options,
            FireflyUpdatePath.TensorPrimitives);
        _vectorOpsOptimizer = new FireflyBenchmarkOptimizer(
            new RandomPositionInitializer(),
            options,
            FireflyUpdatePath.VectorOps);
        _runOptions = new OptimizationRunOptions(StoppingConditions.MaxIterations(10));

        OptimizationRunner.Execute(_problem, _scalarOptimizer, _runOptions, seed: -1);
        OptimizationRunner.Execute(_problem, _tensorPrimitivesOptimizer, _runOptions, seed: -1);
        OptimizationRunner.Execute(_problem, _vectorOpsOptimizer, _runOptions, seed: -1);
    }

    /// <summary>测量原标量距离、移动、Repair 和目标求值的完整生命周期。</summary>
    [Benchmark(Baseline = true)]
    public double ScalarAdvanceLifecycle()
    {
        return OptimizationRunner.Execute(_problem, _scalarOptimizer, _runOptions, seed: 1)
            .BestEvaluation.Objective;
    }

    /// <summary>测量 TensorPrimitives 距离和移动、Repair 及目标求值的完整生命周期。</summary>
    [Benchmark]
    public double TensorPrimitivesAdvanceLifecycle()
    {
        return OptimizationRunner.Execute(_problem, _tensorPrimitivesOptimizer, _runOptions, seed: 1)
            .BestEvaluation.Objective;
    }

    /// <summary>测量 VectorOps 级联距离和移动、Repair 及目标求值的完整生命周期。</summary>
    [Benchmark]
    public double VectorOpsAdvanceLifecycle()
    {
        return OptimizationRunner.Execute(_problem, _vectorOpsOptimizer, _runOptions, seed: 1)
            .BestEvaluation.Objective;
    }

    private enum FireflyUpdatePath
    {
        Scalar,
        TensorPrimitives,
        VectorOps,
    }

    private sealed class FireflyBenchmarkOptimizer : IOptimizer
    {
        private readonly ICandidateInitializer _initializer;
        private readonly FireflyOptimizerOptions _options;
        private readonly FireflyUpdatePath _updatePath;
        private FireflyState[]? _populationA;
        private FireflyState[]? _populationB;
        private double[]? _bestPosition;
        private double[]? _difference;
        private double[]? _randomWalk;
        private Evaluation _bestEvaluation;
        private OptimizationRunContext? _context;
        private int _dimension;
        private int _iteration;
        private bool _populationAIsCurrent;
        private bool _runInitialized;

        public FireflyBenchmarkOptimizer(
            ICandidateInitializer initializer,
            FireflyOptimizerOptions options,
            FireflyUpdatePath updatePath)
        {
            _initializer = initializer;
            _options = options with { };
            _updatePath = updatePath;
        }

        public ReadOnlySpan<double> BestPosition => _runInitialized
            ? _bestPosition
            : throw new InvalidOperationException("The optimizer has not been reset for a run.");

        public Evaluation BestEvaluation => _runInitialized
            ? _bestEvaluation
            : throw new InvalidOperationException("The optimizer has not been reset for a run.");

        public void ResetForRun(OptimizationRunContext context)
        {
            _runInitialized = false;
            EnsureWorkspace(context.Problem.Dimension);
            _context = context;
            _iteration = 0;
            _populationAIsCurrent = true;

            var hasBest = false;
            foreach (var firefly in _populationA!)
            {
                _initializer.Initialize(firefly.Position, context.Random);
                context.Repair(firefly.Position);
                firefly.Evaluation = context.Evaluate(firefly.Position);
                if (!hasBest || EvaluationComparer.IsBetter(
                        firefly.Evaluation,
                        _bestEvaluation,
                        context.Problem.Direction))
                {
                    CopyBest(firefly);
                    hasBest = true;
                }
            }

            _runInitialized = true;
        }

        public void Advance()
        {
            var sourcePopulation = _populationAIsCurrent ? _populationA! : _populationB!;
            var targetPopulation = _populationAIsCurrent ? _populationB! : _populationA!;
            var randomStep = _options.InitialRandomStep * Math.Pow(_options.RandomStepDecay, _iteration);

            switch (_updatePath)
            {
                case FireflyUpdatePath.Scalar:
                    AdvanceScalar(sourcePopulation, targetPopulation, randomStep);
                    break;
                case FireflyUpdatePath.TensorPrimitives:
                    AdvanceTensorPrimitives(sourcePopulation, targetPopulation, randomStep);
                    break;
                case FireflyUpdatePath.VectorOps:
                    AdvanceVectorOps(sourcePopulation, targetPopulation, randomStep);
                    break;
                default:
                    throw new InvalidOperationException("Unknown Firefly benchmark update path.");
            }

            FinishAdvance(targetPopulation);
        }

        private void AdvanceScalar(
            FireflyState[] sourcePopulation,
            FireflyState[] targetPopulation,
            double randomStep)
        {
            for (var fireflyIndex = 0; fireflyIndex < sourcePopulation.Length; fireflyIndex++)
            {
                GenerateCandidateScalar(
                    sourcePopulation[fireflyIndex],
                    sourcePopulation,
                    targetPopulation[fireflyIndex],
                    randomStep);
            }
        }

        private void AdvanceTensorPrimitives(
            FireflyState[] sourcePopulation,
            FireflyState[] targetPopulation,
            double randomStep)
        {
            for (var fireflyIndex = 0; fireflyIndex < sourcePopulation.Length; fireflyIndex++)
            {
                GenerateCandidateTensorPrimitives(
                    sourcePopulation[fireflyIndex],
                    sourcePopulation,
                    targetPopulation[fireflyIndex],
                    randomStep);
            }
        }

        private void AdvanceVectorOps(
            FireflyState[] sourcePopulation,
            FireflyState[] targetPopulation,
            double randomStep)
        {
            for (var fireflyIndex = 0; fireflyIndex < sourcePopulation.Length; fireflyIndex++)
            {
                GenerateCandidateVectorOps(
                    sourcePopulation[fireflyIndex],
                    sourcePopulation,
                    targetPopulation[fireflyIndex],
                    randomStep);
            }
        }

        private void FinishAdvance(FireflyState[] targetPopulation)
        {
            foreach (var firefly in targetPopulation)
            {
                firefly.Evaluation = _context!.Evaluate(firefly.Position);
            }

            FireflyState? generationBest = null;
            foreach (var firefly in targetPopulation)
            {
                if (generationBest is null || EvaluationComparer.IsBetter(
                        firefly.Evaluation,
                        generationBest.Evaluation,
                        _context!.Problem.Direction))
                {
                    generationBest = firefly;
                }
            }

            if (EvaluationComparer.IsBetter(
                    generationBest!.Evaluation,
                    _bestEvaluation,
                    _context!.Problem.Direction))
            {
                CopyBest(generationBest);
            }

            _populationAIsCurrent = !_populationAIsCurrent;
            _iteration = checked(_iteration + 1);
        }

        private void GenerateCandidateScalar(
            FireflyState source,
            IReadOnlyList<FireflyState> sourcePopulation,
            FireflyState target,
            double randomStep)
        {
            source.Position.CopyTo(target.Position, 0);
            foreach (var attractor in sourcePopulation)
            {
                if (!IsBetterAttractor(attractor, source))
                {
                    continue;
                }

                var distanceSquared = DistanceSquared(target.Position, attractor.Position);
                var attractiveness = GetAttractiveness(distanceSquared);
                for (var dimensionIndex = 0; dimensionIndex < _dimension; dimensionIndex++)
                {
                    var randomWalk = randomStep * (_context!.Random.NextDouble() - 0.5);
                    target.Position[dimensionIndex] += (attractiveness
                        * (attractor.Position[dimensionIndex] - target.Position[dimensionIndex])) + randomWalk;
                }

                _context!.Repair(target.Position);
            }
        }

        private void GenerateCandidateTensorPrimitives(
            FireflyState source,
            IReadOnlyList<FireflyState> sourcePopulation,
            FireflyState target,
            double randomStep)
        {
            source.Position.CopyTo(target.Position, 0);
            foreach (var attractor in sourcePopulation)
            {
                if (!IsBetterAttractor(attractor, source))
                {
                    continue;
                }

                TensorPrimitives.Subtract(target.Position, attractor.Position, _difference!);
                var distanceSquared = TensorPrimitives.Dot(_difference!, _difference!);
                var attractiveness = GetAttractiveness(distanceSquared);
                FillRandomWalk(randomStep);
                TensorPrimitives.Subtract(attractor.Position, target.Position, _difference!);
                TensorPrimitives.MultiplyAdd(_difference!, attractiveness, _randomWalk!, _difference!);
                TensorPrimitives.Add(target.Position, _difference!, target.Position);
                _context!.Repair(target.Position);
            }
        }

        private void GenerateCandidateVectorOps(
            FireflyState source,
            IReadOnlyList<FireflyState> sourcePopulation,
            FireflyState target,
            double randomStep)
        {
            source.Position.CopyTo(target.Position, 0);
            foreach (var attractor in sourcePopulation)
            {
                if (!IsBetterAttractor(attractor, source))
                {
                    continue;
                }

                var distanceSquared = VectorOps.DistanceSquared(
                    target.Position,
                    attractor.Position);
                var attractiveness = GetAttractiveness(distanceSquared);
                FillRandomWalk(randomStep);
                VectorOps.UpdateFireflyPosition(
                    target.Position,
                    attractor.Position,
                    _randomWalk!,
                    attractiveness,
                    target.Position);
                _context!.Repair(target.Position);
            }
        }

        private bool IsBetterAttractor(FireflyState attractor, FireflyState source)
        {
            return EvaluationComparer.IsBetter(
                attractor.Evaluation,
                source.Evaluation,
                _context!.Problem.Direction);
        }

        private double GetAttractiveness(double distanceSquared)
        {
            return _options.BaseAttractiveness
                * Math.Exp(-_options.DistanceAttenuation * distanceSquared);
        }

        private void FillRandomWalk(double randomStep)
        {
            for (var dimensionIndex = 0; dimensionIndex < _dimension; dimensionIndex++)
            {
                _randomWalk![dimensionIndex] = randomStep * (_context!.Random.NextDouble() - 0.5);
            }
        }

        private void EnsureWorkspace(int dimension)
        {
            if (_populationA is not null)
            {
                return;
            }

            _dimension = dimension;
            _populationA = CreatePopulation(_options.PopulationSize, dimension);
            _populationB = CreatePopulation(_options.PopulationSize, dimension);
            _bestPosition = new double[dimension];
            _difference = new double[dimension];
            _randomWalk = new double[dimension];
        }

        private static FireflyState[] CreatePopulation(int populationSize, int dimension)
        {
            var population = new FireflyState[populationSize];
            for (var fireflyIndex = 0; fireflyIndex < population.Length; fireflyIndex++)
            {
                population[fireflyIndex] = new FireflyState(dimension);
            }

            return population;
        }

        private void CopyBest(FireflyState source)
        {
            source.Position.CopyTo(_bestPosition!, 0);
            _bestEvaluation = source.Evaluation;
        }

        private static double DistanceSquared(ReadOnlySpan<double> first, ReadOnlySpan<double> second)
        {
            var distance = 0.0;
            for (var index = 0; index < first.Length; index++)
            {
                var difference = first[index] - second[index];
                distance += difference * difference;
            }

            return distance;
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

    private sealed class SphereObjective : IObjectiveFunction
    {
        public double Evaluate(ReadOnlySpan<double> position)
        {
            var sum = 0.0;
            foreach (var value in position)
            {
                sum += value * value;
            }

            return sum;
        }
    }
}
