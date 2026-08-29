using System.Numerics.Tensors;
using Anastasya.Metaheuristics.Core.Comparison;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Algorithms.Pso;

/// <summary>实现面向连续单目标问题的粒子群优化算法。</summary>
/// <remarks>
/// 实例拥有两组粒子工作区，并在正常的顺序 run 之间复用全部主要数组。
/// 该类型不保证线程安全，也不能在一次运行异常后继续复用。
/// </remarks>
public sealed class PsoOptimizer : IOptimizer
{
    private readonly ICandidateInitializer _initializer;
    private readonly PsoOptimizerOptions _options;
    private PsoState[]? _populationA;
    private PsoState[]? _populationB;
    private double[]? _bestPosition;
    private Evaluation _bestEvaluation;
    private OptimizationRunContext? _context;
    private int _dimension;
    private int _iteration;
    private bool _populationAIsCurrent;
    private bool _runInitialized;

    /// <summary>创建粒子群优化器。</summary>
    /// <param name="initializer">为每个候选 Position 写入初始值的必需初始化器；返回后会立即调用 Repair。</param>
    /// <param name="options">算法参数；为 <see langword="null"/> 时使用默认配置。</param>
    /// <exception cref="ArgumentNullException"><paramref name="initializer"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException">种群数量或任一数值参数不在允许范围内。</exception>
    /// <exception cref="ArgumentException">速度区间反向、区间宽度溢出或惯性上下限关系无效。</exception>
    public PsoOptimizer(ICandidateInitializer initializer, PsoOptimizerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(initializer);
        _options = options is null ? new PsoOptimizerOptions() : options with { };
        ValidateOptions(_options);
        _initializer = initializer;
    }

    /// <inheritdoc cref="IOptimizer.BestPosition"/>
    /// <exception cref="InvalidOperationException">尚未成功完成 <see cref="ResetForRun"/>。</exception>
    public ReadOnlySpan<double> BestPosition => _runInitialized
        ? _bestPosition
        : throw new InvalidOperationException("The optimizer has not been reset for a run.");

    /// <inheritdoc cref="IOptimizer.BestEvaluation"/>
    /// <exception cref="InvalidOperationException">尚未成功完成 <see cref="ResetForRun"/>。</exception>
    public Evaluation BestEvaluation => _runInitialized
        ? _bestEvaluation
        : throw new InvalidOperationException("The optimizer has not been reset for a run.");

    /// <inheritdoc cref="IOptimizer.ResetForRun(OptimizationRunContext)"/>
    /// <exception cref="InvalidOperationException">当前实例被用于不同维度的问题。</exception>
    public void ResetForRun(OptimizationRunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _runInitialized = false;
        EnsureWorkspace(context.Problem.Dimension);
        _context = context;
        _iteration = 0;
        _populationAIsCurrent = true;

        var hasBest = false;
        foreach (var particle in _populationA!)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
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

    /// <inheritdoc cref="IOptimizer.Advance"/>
    /// <exception cref="InvalidOperationException">尚未成功完成 <see cref="ResetForRun"/>。</exception>
    public void Advance()
    {
        if (!_runInitialized || _context is null)
        {
            throw new InvalidOperationException("The optimizer has not been reset for a run.");
        }

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
            particle.Evaluation = _context.Evaluate(particle.Position);
        }

        foreach (var particle in targetPopulation)
        {
            if (EvaluationComparer.IsBetter(
                    particle.Evaluation,
                    particle.PersonalBestEvaluation,
                    _context.Problem.Direction))
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

    private static void ValidateOptions(PsoOptimizerOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.PopulationSize);
        ValidateRange(options.VelocityLowerBound, options.VelocityUpperBound, nameof(options));
        ValidateNonNegativeFinite(options.InitialInertia, nameof(options), "The initial inertia must be finite and non-negative.");
        ValidateNonNegativeFinite(options.MinimumInertia, nameof(options), "The minimum inertia must be finite and non-negative.");
        if (options.MinimumInertia > options.InitialInertia)
        {
            throw new ArgumentException("The minimum inertia cannot exceed the initial inertia.", nameof(options));
        }

        if (!double.IsFinite(options.InertiaDecay)
            || options.InertiaDecay <= 0
            || options.InertiaDecay > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The inertia decay must be finite and in the interval (0, 1].");
        }

        ValidateNonNegativeFinite(options.CognitiveCoefficient, nameof(options), "The cognitive coefficient must be finite and non-negative.");
        ValidateNonNegativeFinite(options.SocialCoefficient, nameof(options), "The social coefficient must be finite and non-negative.");
    }

    private static void ValidateRange(double lowerBound, double upperBound, string parameterName)
    {
        if (!double.IsFinite(lowerBound) || !double.IsFinite(upperBound))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Velocity bounds must be finite.");
        }

        if (lowerBound > upperBound)
        {
            throw new ArgumentException("The lower bound cannot exceed the upper bound.", parameterName);
        }

        if (!double.IsFinite(upperBound - lowerBound))
        {
            throw new ArgumentException("The numeric range width must be finite.", parameterName);
        }
    }

    private static void ValidateNonNegativeFinite(double value, string parameterName, string message)
    {
        if (!double.IsFinite(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, message);
        }
    }

    private void EnsureWorkspace(int dimension)
    {
        if (_populationA is not null)
        {
            if (_dimension != dimension)
            {
                throw new InvalidOperationException(
                    "A PsoOptimizer instance cannot be reused with a different problem dimension.");
            }

            return;
        }

        _dimension = dimension;
        _populationA = CreatePopulation(_options.PopulationSize, dimension);
        _populationB = CreatePopulation(_options.PopulationSize, dimension);
        _bestPosition = new double[dimension];
    }

    private static PsoState[] CreatePopulation(int populationSize, int dimension)
    {
        var population = new PsoState[populationSize];
        for (var particleIndex = 0; particleIndex < population.Length; particleIndex++)
        {
            population[particleIndex] = new PsoState(dimension);
        }

        return population;
    }

    private void GenerateCandidate(PsoState source, PsoState target, double inertia)
    {
        var context = _context!;
        var random = context.Random;
        var cognitiveRandom = random.NextDouble();
        var socialRandom = random.NextDouble();
        var cognitiveScale = _options.CognitiveCoefficient * cognitiveRandom;
        var socialScale = _options.SocialCoefficient * socialRandom;

        VectorOps.ComputePsoVelocity(
            source.Position,
            source.Velocity,
            source.PersonalBestPosition,
            _bestPosition!,
            inertia,
            cognitiveScale,
            socialScale,
            target.Velocity);
        TensorPrimitives.Clamp(
            target.Velocity,
            _options.VelocityLowerBound,
            _options.VelocityUpperBound,
            target.Velocity);
        TensorPrimitives.Add(source.Position, target.Velocity, target.Position);

        context.Repair(target.Position);
        source.PersonalBestPosition.CopyTo(target.PersonalBestPosition, 0);
        target.PersonalBestEvaluation = source.PersonalBestEvaluation;
    }

    private void CopyBest(PsoState source)
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
