using Anastasya.Metaheuristics.Core.Comparison;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Algorithms.Bat;

/// <summary>
/// 实现面向连续单目标问题的蝙蝠优化算法。
/// </summary>
/// <remarks>
/// 实例拥有两组种群工作区，并在正常的顺序 run 之间复用全部主要数组。
/// 该类型不保证线程安全，也不能在一次运行异常后继续复用。
/// </remarks>
public sealed class BatOptimizer : IOptimizer
{
    private readonly ICandidateInitializer _initializer;
    private readonly BatOptimizerOptions _options;
    private BatState[]? _populationA;
    private BatState[]? _populationB;
    private double[]? _bestPosition;
    private Evaluation _bestEvaluation;
    private OptimizationRunContext? _context;
    private int _dimension;
    private int _iteration;
    private bool _populationAIsCurrent;
    private bool _runInitialized;

    /// <summary>
    /// 创建蝙蝠优化器。
    /// </summary>
    /// <param name="initializer">为每个候选 Position 写入初始值的必需初始化器；返回后会立即调用 Repair。</param>
    /// <param name="options">算法参数；为 <see langword="null"/> 时使用默认配置。</param>
    /// <exception cref="ArgumentOutOfRangeException">种群数量或任一数值参数不在允许范围内。</exception>
    /// <exception cref="ArgumentException">任一参数区间反向或宽度溢出。</exception>
    public BatOptimizer(ICandidateInitializer initializer, BatOptimizerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(initializer);
        _options = options is null ? new BatOptimizerOptions() : options with { };
        ValidateOptions(_options);
        _initializer = initializer;
    }

    /// <summary>
    /// 获取当前 run 已发现的历史最优位置。
    /// </summary>
    /// <exception cref="InvalidOperationException">尚未成功完成 <see cref="ResetForRun"/>。</exception>
    public ReadOnlySpan<double> BestPosition => _runInitialized
        ? _bestPosition
        : throw new InvalidOperationException("The optimizer has not been reset for a run.");

    /// <summary>
    /// 获取当前 run 已发现的历史最优评估结果。
    /// </summary>
    /// <exception cref="InvalidOperationException">尚未成功完成 <see cref="ResetForRun"/>。</exception>
    public Evaluation BestEvaluation => _runInitialized
        ? _bestEvaluation
        : throw new InvalidOperationException("The optimizer has not been reset for a run.");

    /// <summary>
    /// 复用或创建种群工作区，并使用当前 run 的随机流初始化和评估完整种群。
    /// </summary>
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
        _populationAIsCurrent = true;

        var population = _populationA!;
        var hasBest = false;
        foreach (var bat in population)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            InitializePosition(bat.Position, context);
            for (var dimensionIndex = 0; dimensionIndex < _dimension; dimensionIndex++)
            {
                bat.Velocity[dimensionIndex] = NextDouble(
                    context.Random,
                    _options.VelocityLowerBound,
                    _options.VelocityUpperBound);
                bat.Frequency[dimensionIndex] = NextDouble(
                    context.Random,
                    _options.FrequencyLowerBound,
                    _options.FrequencyUpperBound);
                bat.Loudness[dimensionIndex] = NextDouble(
                    context.Random,
                    _options.InitialLoudnessLowerBound,
                    _options.InitialLoudnessUpperBound);
                bat.PulseRate[dimensionIndex] = NextDouble(
                    context.Random,
                    _options.InitialPulseRateLowerBound,
                    _options.InitialPulseRateUpperBound);
                bat.InitialPulseRate[dimensionIndex] = bat.PulseRate[dimensionIndex];
            }

            bat.Evaluation = context.Evaluate(bat.Position);
            if (!hasBest || EvaluationComparer.IsBetter(bat.Evaluation, _bestEvaluation, context.Problem.Direction))
            {
                CopyBest(bat);
                hasBest = true;
            }
        }

        _runInitialized = true;
    }

    /// <summary>
    /// 生成、评估并选择一代候选蝙蝠，同时保留严格历史最优快照。
    /// </summary>
    /// <exception cref="InvalidOperationException">尚未成功完成 <see cref="ResetForRun"/>。</exception>
    public void Advance()
    {
        if (!_runInitialized || _context is null)
        {
            throw new InvalidOperationException("The optimizer has not been reset for a run.");
        }

        var sourcePopulation = _populationAIsCurrent ? _populationA! : _populationB!;
        var targetPopulation = _populationAIsCurrent ? _populationB! : _populationA!;

        // 候选只写入目标缓冲；即使随后被拒绝，也不会污染 incumbent 的速度和其他状态。
        for (var batIndex = 0; batIndex < sourcePopulation.Length; batIndex++)
        {
            GenerateCandidate(sourcePopulation[batIndex], targetPopulation[batIndex]);
        }

        foreach (var candidate in targetPopulation)
        {
            candidate.Evaluation = _context.Evaluate(candidate.Position);
        }

        BatState? generationBest = null;
        for (var batIndex = 0; batIndex < sourcePopulation.Length; batIndex++)
        {
            if (!EvaluationComparer.IsBetter(
                    targetPopulation[batIndex].Evaluation,
                    sourcePopulation[batIndex].Evaluation,
                    _context.Problem.Direction))
            {
                // 通过交换对象引用保留 incumbent，避免复制整条位置和状态数组。
                (targetPopulation[batIndex], sourcePopulation[batIndex]) =
                    (sourcePopulation[batIndex], targetPopulation[batIndex]);
            }

            var selected = targetPopulation[batIndex];
            if (generationBest is null || EvaluationComparer.IsBetter(
                    selected.Evaluation,
                    generationBest.Evaluation,
                    _context.Problem.Direction))
            {
                generationBest = selected;
            }
        }

        if (EvaluationComparer.IsBetter(generationBest!.Evaluation, _bestEvaluation, _context.Problem.Direction))
        {
            CopyBest(generationBest);
        }

        _populationAIsCurrent = !_populationAIsCurrent;
        _iteration = checked(_iteration + 1);
    }

    private static void ValidateOptions(BatOptimizerOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.PopulationSize);
        ValidateRange(options.VelocityLowerBound, options.VelocityUpperBound, nameof(options));
        ValidateRange(options.FrequencyLowerBound, options.FrequencyUpperBound, nameof(options));
        if (options.FrequencyLowerBound < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Frequency bounds must be non-negative.");
        }

        ValidateRange(options.InitialLoudnessLowerBound, options.InitialLoudnessUpperBound, nameof(options));
        if (options.InitialLoudnessLowerBound < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Initial loudness must be non-negative.");
        }

        ValidateRange(options.InitialPulseRateLowerBound, options.InitialPulseRateUpperBound, nameof(options));
        if (options.InitialPulseRateLowerBound < 0 || options.InitialPulseRateUpperBound > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Initial pulse rates must be between zero and one.");
        }

        if (!double.IsFinite(options.LoudnessDecay)
            || options.LoudnessDecay <= 0
            || options.LoudnessDecay > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The loudness decay must be finite and in the interval (0, 1].");
        }

        if (!double.IsFinite(options.PulseRateGrowth) || options.PulseRateGrowth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "The pulse-rate growth factor must be finite and positive.");
        }
    }

    private static void ValidateRange(double lowerBound, double upperBound, string parameterName)
    {
        if (!double.IsFinite(lowerBound))
        {
            throw new ArgumentOutOfRangeException(parameterName, "The lower bound must be finite.");
        }

        if (!double.IsFinite(upperBound))
        {
            throw new ArgumentOutOfRangeException(parameterName, "The upper bound must be finite.");
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

    private void EnsureWorkspace(int dimension)
    {
        if (_populationA is not null)
        {
            if (_dimension != dimension)
            {
                throw new InvalidOperationException(
                    "A BatOptimizer instance cannot be reused with a different problem dimension.");
            }

            return;
        }

        _dimension = dimension;
        _populationA = CreatePopulation(_options.PopulationSize, dimension);
        _populationB = CreatePopulation(_options.PopulationSize, dimension);
        _bestPosition = new double[dimension];
    }

    private static BatState[] CreatePopulation(int populationSize, int dimension)
    {
        var population = new BatState[populationSize];
        for (var batIndex = 0; batIndex < population.Length; batIndex++)
        {
            population[batIndex] = new BatState(dimension);
        }

        return population;
    }

    private void InitializePosition(Span<double> position, OptimizationRunContext context)
    {
        _initializer.Initialize(position, context.Random);
        context.Repair(position);
    }

    private void GenerateCandidate(BatState source, BatState target)
    {
        var context = _context!;
        var random = context.Random;
        for (var dimensionIndex = 0; dimensionIndex < _dimension; dimensionIndex++)
        {
            target.Frequency[dimensionIndex] = NextDouble(
                random,
                _options.FrequencyLowerBound,
                _options.FrequencyUpperBound);

            // fix 分支的关键修复：使用本轮新频率并只写目标速度，拒绝候选时源状态保持不变。
            var velocity = source.Velocity[dimensionIndex]
                + ((_bestPosition![dimensionIndex] - source.Position[dimensionIndex])
                    * target.Frequency[dimensionIndex]);
            target.Velocity[dimensionIndex] = Math.Clamp(
                velocity,
                _options.VelocityLowerBound,
                _options.VelocityUpperBound);

            var nextPosition = random.NextDouble() > source.PulseRate[dimensionIndex]
                ? _bestPosition[dimensionIndex]
                    + (NextDouble(random, -1, 1) * source.Loudness[dimensionIndex])
                : source.Position[dimensionIndex] + target.Velocity[dimensionIndex];

            target.InitialPulseRate[dimensionIndex] = source.InitialPulseRate[dimensionIndex];
            if (random.NextDouble() < source.Loudness[dimensionIndex])
            {
                target.Position[dimensionIndex] = nextPosition;
                target.Loudness[dimensionIndex] =
                    _options.LoudnessDecay * source.Loudness[dimensionIndex];
                target.PulseRate[dimensionIndex] = source.InitialPulseRate[dimensionIndex]
                    * (1 - Math.Exp(-_options.PulseRateGrowth * _iteration));
            }
            else
            {
                target.Position[dimensionIndex] = source.Position[dimensionIndex];
                target.Loudness[dimensionIndex] = source.Loudness[dimensionIndex];
                target.PulseRate[dimensionIndex] = source.PulseRate[dimensionIndex];
            }
        }

        RepairPosition(target.Position, context);
    }

    private static void RepairPosition(Span<double> position, OptimizationRunContext context)
    {
        context.Repair(position);
    }

    private void CopyBest(BatState source)
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
