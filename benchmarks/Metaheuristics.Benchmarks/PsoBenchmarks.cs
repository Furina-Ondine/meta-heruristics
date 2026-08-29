using System.Runtime.Intrinsics;
using System.Numerics.Tensors;
using Anastasya.Metaheuristics.Algorithms;
using Anastasya.Metaheuristics.Algorithms.Pso;
using Anastasya.Metaheuristics.Core.Comparison;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;
using BenchmarkDotNet.Attributes;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>比较 PSO 标量候选更新与计划采用的 TensorPrimitives 组合。</summary>
[MemoryDiagnoser]
public class PsoCandidateUpdateBenchmarks
{
    private double[] _sourcePosition = null!;
    private double[] _sourceVelocity = null!;
    private double[] _personalBestPosition = null!;
    private double[] _globalBestPosition = null!;
    private double[] _scalarPosition = null!;
    private double[] _scalarVelocity = null!;
    private double[] _tensorPosition = null!;
    private double[] _tensorVelocity = null!;
    private double[] _vectorOpsPosition = null!;
    private double[] _vectorOpsVelocity = null!;

    /// <summary>获取或设置候选位置的维度。</summary>
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

    /// <summary>为标量与 Tensor 路径创建相同的固定输入。</summary>
    [GlobalSetup]
    public void Setup()
    {
        _sourcePosition = new double[Dimension];
        _sourceVelocity = new double[Dimension];
        _personalBestPosition = new double[Dimension];
        _globalBestPosition = new double[Dimension];
        _scalarPosition = new double[Dimension];
        _scalarVelocity = new double[Dimension];
        _tensorPosition = new double[Dimension];
        _tensorVelocity = new double[Dimension];
        _vectorOpsPosition = new double[Dimension];
        _vectorOpsVelocity = new double[Dimension];

        for (var index = 0; index < Dimension; index++)
        {
            _sourcePosition[index] = (index % 11) - 5;
            _sourceVelocity[index] = ((index % 7) - 3) * 0.25;
            _personalBestPosition[index] = _sourcePosition[index] + ((index % 3) - 1);
            _globalBestPosition[index] = _sourcePosition[index] + ((index % 5) - 2);
        }
    }

    /// <summary>测量当前 PSO 标量逐维更新作为同机基线。</summary>
    [Benchmark(Baseline = true)]
    public void ScalarCandidateUpdate()
    {
        for (var index = 0; index < Dimension; index++)
        {
            var velocity = (0.79 * _sourceVelocity[index])
                + (0.75 * (_personalBestPosition[index] - _sourcePosition[index]))
                + (0.25 * (_globalBestPosition[index] - _sourcePosition[index]));
            _scalarVelocity[index] = Math.Clamp(velocity, -1, 1);
            _scalarPosition[index] = _sourcePosition[index] + _scalarVelocity[index];
        }
    }

    /// <summary>测量采用候选 Position/Velocity 作为复用工作区的 TensorPrimitives 组合。</summary>
    [Benchmark]
    public void TensorPrimitivesCandidateUpdate()
    {
        TensorPrimitives.Subtract(_personalBestPosition, _sourcePosition, _tensorPosition);
        TensorPrimitives.Multiply(_tensorPosition, 0.75, _tensorPosition);
        TensorPrimitives.Multiply(_sourceVelocity, 0.79, _tensorVelocity);
        TensorPrimitives.Add(_tensorVelocity, _tensorPosition, _tensorVelocity);
        TensorPrimitives.Subtract(_globalBestPosition, _sourcePosition, _tensorPosition);
        TensorPrimitives.Multiply(_tensorPosition, 0.25, _tensorPosition);
        TensorPrimitives.Add(_tensorVelocity, _tensorPosition, _tensorVelocity);
        TensorPrimitives.Clamp(_tensorVelocity, -1, 1, _tensorVelocity);
        TensorPrimitives.Add(_sourcePosition, _tensorVelocity, _tensorPosition);
    }

    /// <summary>测量融合速度公式的私有 VectorOps 加直接 Clamp/Add 的生产组合。</summary>
    [Benchmark]
    public void VectorOpsCandidateUpdate()
    {
        VectorOps.ComputePsoVelocity(
            _sourcePosition,
            _sourceVelocity,
            _personalBestPosition,
            _globalBestPosition,
            inertia: 0.79,
            cognitiveScale: 0.75,
            socialScale: 0.25,
            destination: _vectorOpsVelocity);
        TensorPrimitives.Clamp(_vectorOpsVelocity, -1, 1, _vectorOpsVelocity);
        TensorPrimitives.Add(_sourcePosition, _vectorOpsVelocity, _vectorOpsPosition);
    }
}

/// <summary>记录完整 PSO `Advance` 生命周期的同机基线或迁移后结果。</summary>
[MemoryDiagnoser]
public class PsoAdvanceBenchmarks
{
    private ContinuousProblem _problem = null!;
    private ScalarPsoBenchmarkOptimizer _scalarOptimizer = null!;
    private PsoOptimizer _vectorOpsOptimizer = null!;
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

    /// <summary>创建可在各次测量间复用工作区的正常 PSO 运行。</summary>
    [GlobalSetup]
    public void Setup()
    {
        _problem = new ContinuousProblem(Dimension, new SphereObjective(), CandidateRepairs.Clamp(-5, 5));
        var options = new PsoOptimizerOptions { PopulationSize = 64 };
        _scalarOptimizer = new ScalarPsoBenchmarkOptimizer(new RandomPositionInitializer(), options);
        _vectorOpsOptimizer = new PsoOptimizer(new RandomPositionInitializer(), options);
        _runOptions = new OptimizationRunOptions(StoppingConditions.MaxIterations(10));
        OptimizationRunner.Execute(_problem, _scalarOptimizer, _runOptions, seed: -1);
        OptimizationRunner.Execute(_problem, _vectorOpsOptimizer, _runOptions, seed: -1);
    }

    /// <summary>测量原标量候选更新、Repair 和目标求值的完整生命周期。</summary>
    [Benchmark(Baseline = true)]
    public double ScalarAdvanceLifecycle()
    {
        return OptimizationRunner.Execute(_problem, _scalarOptimizer, _runOptions, seed: 1).BestEvaluation.Objective;
    }

    /// <summary>测量生产 VectorOps 加 TensorPrimitives 候选更新、Repair 和目标求值的完整生命周期。</summary>
    [Benchmark]
    public double VectorOpsAdvanceLifecycle()
    {
        return OptimizationRunner.Execute(_problem, _vectorOpsOptimizer, _runOptions, seed: 1).BestEvaluation.Objective;
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

    private sealed class ScalarPsoBenchmarkOptimizer : IOptimizer
    {
        private readonly ICandidateInitializer _initializer;
        private readonly PsoOptimizerOptions _options;
        private ScalarPsoBenchmarkState[]? _populationA;
        private ScalarPsoBenchmarkState[]? _populationB;
        private double[]? _bestPosition;
        private Evaluation _bestEvaluation;
        private OptimizationRunContext? _context;
        private int _dimension;
        private int _iteration;
        private bool _populationAIsCurrent;
        private bool _runInitialized;

        public ScalarPsoBenchmarkOptimizer(ICandidateInitializer initializer, PsoOptimizerOptions options)
        {
            _initializer = initializer;
            _options = options with { };
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
            foreach (var particle in _populationA!)
            {
                _initializer.Initialize(particle.Position, context.Random);
                context.Repair(particle.Position);
                for (var dimensionIndex = 0; dimensionIndex < _dimension; dimensionIndex++)
                {
                    particle.Velocity[dimensionIndex] = NextDouble(
                        context.Random,
                        _options.VelocityLowerBound,
                        _options.VelocityUpperBound);
                }

                particle.Evaluation = context.Evaluate(particle.Position);
                particle.Position.CopyTo(particle.PersonalBestPosition, 0);
                particle.PersonalBestEvaluation = particle.Evaluation;
                if (!hasBest || EvaluationComparer.IsBetter(
                        particle.Evaluation,
                        _bestEvaluation,
                        context.Problem.Direction))
                {
                    CopyBest(particle);
                    hasBest = true;
                }
            }

            _runInitialized = true;
        }

        public void Advance()
        {
            var sourcePopulation = _populationAIsCurrent ? _populationA! : _populationB!;
            var targetPopulation = _populationAIsCurrent ? _populationB! : _populationA!;
            var inertia = Math.Max(
                _options.MinimumInertia,
                _options.InitialInertia * Math.Pow(_options.InertiaDecay, _iteration));

            for (var particleIndex = 0; particleIndex < sourcePopulation.Length; particleIndex++)
            {
                GenerateCandidate(sourcePopulation[particleIndex], targetPopulation[particleIndex], inertia);
            }

            foreach (var particle in targetPopulation)
            {
                particle.Evaluation = _context!.Evaluate(particle.Position);
            }

            foreach (var particle in targetPopulation)
            {
                if (EvaluationComparer.IsBetter(
                        particle.Evaluation,
                        particle.PersonalBestEvaluation,
                        _context!.Problem.Direction))
                {
                    particle.Position.CopyTo(particle.PersonalBestPosition, 0);
                    particle.PersonalBestEvaluation = particle.Evaluation;
                }

                if (EvaluationComparer.IsBetter(
                        particle.Evaluation,
                        _bestEvaluation,
                        _context.Problem.Direction))
                {
                    CopyBest(particle);
                }
            }

            _populationAIsCurrent = !_populationAIsCurrent;
            _iteration = checked(_iteration + 1);
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
        }

        private static ScalarPsoBenchmarkState[] CreatePopulation(int populationSize, int dimension)
        {
            var population = new ScalarPsoBenchmarkState[populationSize];
            for (var particleIndex = 0; particleIndex < population.Length; particleIndex++)
            {
                population[particleIndex] = new ScalarPsoBenchmarkState(dimension);
            }

            return population;
        }

        private void GenerateCandidate(ScalarPsoBenchmarkState source, ScalarPsoBenchmarkState target, double inertia)
        {
            var context = _context!;
            var cognitiveRandom = context.Random.NextDouble();
            var socialRandom = context.Random.NextDouble();
            for (var dimensionIndex = 0; dimensionIndex < _dimension; dimensionIndex++)
            {
                var velocity = (inertia * source.Velocity[dimensionIndex])
                    + ((_options.CognitiveCoefficient * cognitiveRandom)
                        * (source.PersonalBestPosition[dimensionIndex] - source.Position[dimensionIndex]))
                    + ((_options.SocialCoefficient * socialRandom)
                        * (_bestPosition![dimensionIndex] - source.Position[dimensionIndex]));
                target.Velocity[dimensionIndex] = Math.Clamp(
                    velocity,
                    _options.VelocityLowerBound,
                    _options.VelocityUpperBound);
                target.Position[dimensionIndex] = source.Position[dimensionIndex] + target.Velocity[dimensionIndex];
            }

            context.Repair(target.Position);
            source.PersonalBestPosition.CopyTo(target.PersonalBestPosition, 0);
            target.PersonalBestEvaluation = source.PersonalBestEvaluation;
        }

        private void CopyBest(ScalarPsoBenchmarkState source)
        {
            source.Position.CopyTo(_bestPosition!, 0);
            _bestEvaluation = source.Evaluation;
        }

        private static double NextDouble(Random random, double lowerBound, double upperBound)
        {
            return lowerBound == upperBound
                ? lowerBound
                : lowerBound + ((upperBound - lowerBound) * random.NextDouble());
        }
    }

    private sealed class ScalarPsoBenchmarkState
    {
        public ScalarPsoBenchmarkState(int dimension)
        {
            Position = new double[dimension];
            Velocity = new double[dimension];
            PersonalBestPosition = new double[dimension];
        }

        public double[] Position { get; }

        public double[] Velocity { get; }

        public double[] PersonalBestPosition { get; }

        public Evaluation Evaluation { get; set; }

        public Evaluation PersonalBestEvaluation { get; set; }
    }
}
