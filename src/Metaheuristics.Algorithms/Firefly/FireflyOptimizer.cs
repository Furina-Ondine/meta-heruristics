using Anastasya.Metaheuristics.Core.Comparison;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Algorithms.Firefly;

/// <summary>实现面向连续单目标问题的萤火虫优化算法。</summary>
/// <remarks>
/// 每轮读取稳定的当前代，并在独立的下一代工作区中顺序应用严格更优萤火虫的吸引。
/// 实例不保证线程安全，也不能在一次运行异常后继续复用。
/// </remarks>
public sealed class FireflyOptimizer : IOptimizer
{
    private readonly ICandidateInitializer _initializer;
    private readonly FireflyOptimizerOptions _options;
    private FireflyState[]? _populationA;
    private FireflyState[]? _populationB;
    private double[]? _bestPosition;
    private Evaluation _bestEvaluation;
    private OptimizationRunContext? _context;
    private int _dimension;
    private int _iteration;
    private bool _populationAIsCurrent;
    private bool _runInitialized;

    /// <summary>创建萤火虫优化器。</summary>
    /// <param name="initializer">为每个候选 Position 写入初始值的必需初始化器；返回后会立即调用 Repair。</param>
    /// <param name="options">算法参数；为 <see langword="null"/> 时使用默认配置。</param>
    /// <exception cref="ArgumentNullException"><paramref name="initializer"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException">种群数量或任一数值参数不在允许范围内。</exception>
    public FireflyOptimizer(ICandidateInitializer initializer, FireflyOptimizerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(initializer);
        _options = options is null ? new FireflyOptimizerOptions() : options with { };
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
        foreach (var firefly in _populationA!)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
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
        var randomStep = _options.InitialRandomStep * Math.Pow(_options.RandomStepDecay, _iteration);

        for (var fireflyIndex = 0; fireflyIndex < sourcePopulation.Length; fireflyIndex++)
        {
            GenerateCandidate(sourcePopulation[fireflyIndex], sourcePopulation, targetPopulation[fireflyIndex], randomStep);
        }

        FireflyState? generationBest = null;
        foreach (var firefly in targetPopulation)
        {
            firefly.Evaluation = _context.Evaluate(firefly.Position);
            if (generationBest is null || EvaluationComparer.IsBetter(
                    firefly.Evaluation,
                    generationBest.Evaluation,
                    _context.Problem.Direction))
            {
                generationBest = firefly;
            }
        }

        if (EvaluationComparer.IsBetter(
                generationBest!.Evaluation,
                _bestEvaluation,
                _context.Problem.Direction))
        {
            CopyBest(generationBest);
        }

        _populationAIsCurrent = !_populationAIsCurrent;
        _iteration = checked(_iteration + 1);
    }

    private static void ValidateOptions(FireflyOptimizerOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.PopulationSize);
        ValidateNonNegativeFinite(options.BaseAttractiveness, nameof(options), "The base attractiveness must be finite and non-negative.");
        ValidateNonNegativeFinite(options.DistanceAttenuation, nameof(options), "The distance attenuation must be finite and non-negative.");
        ValidateNonNegativeFinite(options.InitialRandomStep, nameof(options), "The initial random step must be finite and non-negative.");
        if (!double.IsFinite(options.RandomStepDecay)
            || options.RandomStepDecay <= 0
            || options.RandomStepDecay > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The random-step decay must be finite and in the interval (0, 1].");
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
                    "A FireflyOptimizer instance cannot be reused with a different problem dimension.");
            }

            return;
        }

        _dimension = dimension;
        _populationA = CreatePopulation(_options.PopulationSize, dimension);
        _populationB = CreatePopulation(_options.PopulationSize, dimension);
        _bestPosition = new double[dimension];
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

    private void GenerateCandidate(
        FireflyState source,
        IReadOnlyList<FireflyState> sourcePopulation,
        FireflyState target,
        double randomStep)
    {
        source.Position.CopyTo(target.Position, 0);
        foreach (var attractor in sourcePopulation)
        {
            if (!EvaluationComparer.IsBetter(
                    attractor.Evaluation,
                    source.Evaluation,
                    _context!.Problem.Direction))
            {
                continue;
            }

            var distanceSquared = DistanceSquared(target.Position, attractor.Position);
            var attractiveness = _options.BaseAttractiveness
                * Math.Exp(-_options.DistanceAttenuation * distanceSquared);
            for (var dimensionIndex = 0; dimensionIndex < _dimension; dimensionIndex++)
            {
                var randomWalk = randomStep * (_context.Random.NextDouble() - 0.5);
                target.Position[dimensionIndex] += (attractiveness
                    * (attractor.Position[dimensionIndex] - target.Position[dimensionIndex])) + randomWalk;
            }

            _context.Repair(target.Position);
        }
    }

    private static double DistanceSquared(ReadOnlySpan<double> first, ReadOnlySpan<double> second)
    {
        var distance = 0.0;
        for (var index = 0; index < first.Length; index++)
        {
            var delta = first[index] - second[index];
            distance += delta * delta;
        }

        return distance;
    }

    private void CopyBest(FireflyState source)
    {
        source.Position.CopyTo(_bestPosition!, 0);
        _bestEvaluation = source.Evaluation;
    }
}
