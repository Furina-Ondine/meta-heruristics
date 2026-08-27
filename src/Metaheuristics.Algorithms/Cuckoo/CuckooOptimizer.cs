using Anastasya.Metaheuristics.Core.Comparison;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Algorithms.Cuckoo;

/// <summary>实现面向连续单目标问题的布谷鸟优化算法。</summary>
/// <remarks>
/// 实例拥有种群、候选和正态采样工作区；所有可变状态在正常顺序 run 之间复用，且不与其他实例共享。
/// 该类型不保证线程安全，也不能在一次运行异常后继续复用。
/// </remarks>
public sealed class CuckooOptimizer : IOptimizer
{
    private readonly ICandidateInitializer _initializer;
    private readonly CuckooOptimizerOptions _options;
    private readonly double _levySigma;
    private CuckooState[]? _population;
    private CuckooState[]? _candidates;
    private int[]? _sortedIndices;
    private double[]? _bestPosition;
    private Evaluation _bestEvaluation;
    private OptimizationRunContext? _context;
    private int _dimension;
    private int _iteration;
    private bool _hasSpareGaussian;
    private double _spareGaussian;
    private bool _runInitialized;

    /// <summary>创建布谷鸟优化器。</summary>
    /// <param name="initializer">为每个候选 Position 写入初始值的必需初始化器；返回后会立即调用 Repair。</param>
    /// <param name="options">算法参数；为 <see langword="null"/> 时使用默认配置。</param>
    /// <exception cref="ArgumentOutOfRangeException">种群数量或任一数值参数不在允许范围内。</exception>
    /// <exception cref="ArgumentException">Lévy 候选数与种群数量不相容。</exception>
    public CuckooOptimizer(ICandidateInitializer initializer, CuckooOptimizerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(initializer);
        _options = options is null ? new CuckooOptimizerOptions() : options with { };
        ValidateOptions(_options);
        _levySigma = ComputeLevySigma(_options.LevyExponent);
        _initializer = initializer;
    }

    /// <summary>获取当前 run 发现的历史最佳位置。</summary>
    /// <exception cref="InvalidOperationException">尚未成功完成 <see cref="ResetForRun"/>。</exception>
    public ReadOnlySpan<double> BestPosition => _runInitialized
        ? _bestPosition
        : throw new InvalidOperationException("The optimizer has not been reset for a run.");

    /// <summary>获取当前 run 发现的历史最佳评估结果。</summary>
    /// <exception cref="InvalidOperationException">尚未成功完成 <see cref="ResetForRun"/>。</exception>
    public Evaluation BestEvaluation => _runInitialized
        ? _bestEvaluation
        : throw new InvalidOperationException("The optimizer has not been reset for a run.");

    /// <summary>复用或创建工作区，并初始化、修复和评估完整初始种群。</summary>
    /// <param name="context">当前 run 独占的问题、随机数、取消和评估上下文。</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="InvalidOperationException">当前实例被用于不同维度的问题。</exception>
    public void ResetForRun(OptimizationRunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _runInitialized = false;
        EnsureWorkspace(context.Problem.Dimension);
        _context = context;
        _iteration = 0;
        _hasSpareGaussian = false;

        var hasBest = false;
        foreach (var cuckoo in _population!)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            _initializer.Initialize(cuckoo.Position, context.Random);
            context.Repair(cuckoo.Position);
            cuckoo.Evaluation = context.Evaluate(cuckoo.Position);
            if (!hasBest || EvaluationComparer.IsBetter(
                    cuckoo.Evaluation,
                    _bestEvaluation,
                    context.Problem.Direction))
            {
                CopyBest(cuckoo);
                hasBest = true;
            }
        }

        _runInitialized = true;
    }

    /// <summary>执行 Lévy 候选与遗弃最差巢两个阶段，并保留独立的历史最佳快照。</summary>
    /// <exception cref="InvalidOperationException">尚未成功完成 <see cref="ResetForRun"/>。</exception>
    public void Advance()
    {
        if (!_runInitialized || _context is null)
        {
            throw new InvalidOperationException("The optimizer has not been reset for a run.");
        }

        var decayFactor = 1 - (0.9 * Math.Min(
            (double)_iteration / _options.StepDecayIterations,
            1));
        var levyScale = _options.BaseLevyScale * decayFactor;
        for (var candidateIndex = 0; candidateIndex < _options.LevyCandidateCount; candidateIndex++)
        {
            var candidate = _candidates![candidateIndex];
            GenerateLevyCandidate(_population![candidateIndex], candidate, levyScale);
            candidate.Evaluation = _context.Evaluate(candidate.Position);
            if (EvaluationComparer.IsBetter(
                    candidate.Evaluation,
                    _population[candidateIndex].Evaluation,
                    _context.Problem.Direction))
            {
                CopyState(candidate, _population[candidateIndex]);
                if (EvaluationComparer.IsBetter(
                        candidate.Evaluation,
                        _bestEvaluation,
                        _context.Problem.Direction))
                {
                    CopyBest(candidate);
                }
            }
        }

        ReplaceAbandonedNests(decayFactor);
        _iteration = checked(_iteration + 1);
    }

    private static void ValidateOptions(CuckooOptimizerOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.PopulationSize);
        if (!double.IsFinite(options.AbandonmentRate)
            || options.AbandonmentRate < 0
            || options.AbandonmentRate > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The abandonment rate must be finite and in the interval [0, 1].");
        }

        if (!double.IsFinite(options.LevyExponent)
            || options.LevyExponent <= 0
            || options.LevyExponent >= 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The Levy exponent must be finite and in the interval (0, 2).");
        }

        ValidatePositiveFinite(options.GaussianScale, nameof(options), "The Gaussian scale must be finite and positive.");
        ValidatePositiveFinite(options.BaseLevyScale, nameof(options), "The base Levy scale must be finite and positive.");
        if (!double.IsFinite(options.AbandonmentPerturbationScale)
            || options.AbandonmentPerturbationScale < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The abandonment perturbation scale must be finite and non-negative.");
        }

        if (options.LevyCandidateCount < 1 || options.LevyCandidateCount > options.PopulationSize)
        {
            throw new ArgumentException(
                "The Levy candidate count must be between one and the population size.",
                nameof(options));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.StepDecayIterations);
    }

    private static void ValidatePositiveFinite(double value, string parameterName, string message)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, message);
        }
    }

    private void EnsureWorkspace(int dimension)
    {
        if (_population is not null)
        {
            if (_dimension != dimension)
            {
                throw new InvalidOperationException(
                    "A CuckooOptimizer instance cannot be reused with a different problem dimension.");
            }

            return;
        }

        _dimension = dimension;
        _population = CreateStates(_options.PopulationSize, dimension);
        _candidates = CreateStates(_options.PopulationSize, dimension);
        _sortedIndices = new int[_options.PopulationSize];
        _bestPosition = new double[dimension];
    }

    private static CuckooState[] CreateStates(int populationSize, int dimension)
    {
        var states = new CuckooState[populationSize];
        for (var stateIndex = 0; stateIndex < states.Length; stateIndex++)
        {
            states[stateIndex] = new CuckooState(dimension);
        }

        return states;
    }

    private void GenerateLevyCandidate(CuckooState source, CuckooState target, double levyScale)
    {
        for (var dimensionIndex = 0; dimensionIndex < _dimension; dimensionIndex++)
        {
            var numerator = NextGaussian() * _levySigma * _options.GaussianScale;
            var denominator = Math.Pow(Math.Abs(NextGaussian()) + 1e-10, 1 / _options.LevyExponent);
            var levyStep = levyScale * numerator / denominator;
            var guidance = (_bestPosition![dimensionIndex] - source.Position[dimensionIndex])
                * _context!.Random.NextDouble();
            target.Position[dimensionIndex] = source.Position[dimensionIndex]
                + (0.8 * levyStep)
                + (0.2 * guidance);
        }

        _context!.Repair(target.Position);
    }

    private void ReplaceAbandonedNests(double decayFactor)
    {
        var abandonmentCount = (int)(_options.AbandonmentRate * _population!.Length);
        if (abandonmentCount == 0)
        {
            return;
        }

        FindWorstIndices(abandonmentCount);
        for (var candidateIndex = 0; candidateIndex < abandonmentCount; candidateIndex++)
        {
            var candidate = _candidates![candidateIndex];
            GenerateAbandonmentCandidate(candidate, decayFactor);
            candidate.Evaluation = _context!.Evaluate(candidate.Position);
            var targetIndex = _sortedIndices![candidateIndex];
            if (EvaluationComparer.IsBetter(
                    candidate.Evaluation,
                    _population[targetIndex].Evaluation,
                    _context.Problem.Direction))
            {
                CopyState(candidate, _population[targetIndex]);
                if (EvaluationComparer.IsBetter(
                        candidate.Evaluation,
                        _bestEvaluation,
                        _context.Problem.Direction))
                {
                    CopyBest(candidate);
                }
            }
        }
    }

    private void FindWorstIndices(int count)
    {
        for (var index = 0; index < _sortedIndices!.Length; index++)
        {
            _sortedIndices[index] = index;
        }

        for (var index = 0; index < count; index++)
        {
            var worstIndex = index;
            for (var candidateIndex = index + 1; candidateIndex < _sortedIndices.Length; candidateIndex++)
            {
                if (EvaluationComparer.IsBetter(
                        _population![_sortedIndices[worstIndex]].Evaluation,
                        _population[_sortedIndices[candidateIndex]].Evaluation,
                        _context!.Problem.Direction))
                {
                    worstIndex = candidateIndex;
                }
            }

            (_sortedIndices[index], _sortedIndices[worstIndex]) =
                (_sortedIndices[worstIndex], _sortedIndices[index]);
        }
    }

    private void GenerateAbandonmentCandidate(CuckooState target, double decayFactor)
    {
        var random = _context!.Random;
        var firstIndex = random.Next(_population!.Length);
        var secondIndex = random.Next(_population.Length);
        while (_population.Length > 1 && secondIndex == firstIndex)
        {
            secondIndex = random.Next(_population.Length);
        }

        var first = _population[firstIndex];
        var second = _population[secondIndex];
        var differenceScale = 0.5 * decayFactor;
        for (var dimensionIndex = 0; dimensionIndex < _dimension; dimensionIndex++)
        {
            var perturbation = (random.NextDouble() - 0.5) * _options.AbandonmentPerturbationScale;
            target.Position[dimensionIndex] = _bestPosition![dimensionIndex]
                + (differenceScale * (first.Position[dimensionIndex] - second.Position[dimensionIndex]))
                + perturbation;
        }

        _context.Repair(target.Position);
    }

    private double NextGaussian()
    {
        if (_hasSpareGaussian)
        {
            _hasSpareGaussian = false;
            return _spareGaussian;
        }

        var random = _context!.Random;
        var first = Math.Max(random.NextDouble(), double.Epsilon);
        var second = random.NextDouble();
        var radius = Math.Sqrt(-2 * Math.Log(first));
        var angle = 2 * Math.PI * second;
        _spareGaussian = radius * Math.Sin(angle);
        _hasSpareGaussian = true;
        return radius * Math.Cos(angle);
    }

    private void CopyBest(CuckooState source)
    {
        source.Position.CopyTo(_bestPosition!, 0);
        _bestEvaluation = source.Evaluation;
    }

    private static void CopyState(CuckooState source, CuckooState target)
    {
        source.Position.CopyTo(target.Position, 0);
        target.Evaluation = source.Evaluation;
    }

    private static double ComputeLevySigma(double exponent)
    {
        var logSigma = (
            LogGamma(1 + exponent)
            + Math.Log(Math.Sin(Math.PI * exponent / 2))
            - Math.Log(exponent)
            - LogGamma((1 + exponent) / 2)
            - (((exponent - 1) / 2) * Math.Log(2))) / exponent;
        return Math.Exp(logSigma);
    }

    private static double LogGamma(double value)
    {
        ReadOnlySpan<double> coefficients =
        [
            676.5203681218851,
            -1259.1392167224028,
            771.32342877765313,
            -176.61502916214059,
            12.507343278686905,
            -0.13857109526572012,
            9.9843695780195716e-6,
            1.5056327351493116e-7,
        ];

        var adjusted = value - 1;
        var series = 0.99999999999980993;
        for (var coefficientIndex = 0; coefficientIndex < coefficients.Length; coefficientIndex++)
        {
            series += coefficients[coefficientIndex] / (adjusted + coefficientIndex + 1);
        }

        var shifted = adjusted + coefficients.Length - 0.5;
        return (0.5 * Math.Log(2 * Math.PI))
            + ((adjusted + 0.5) * Math.Log(shifted))
            - shifted
            + Math.Log(series);
    }
}
