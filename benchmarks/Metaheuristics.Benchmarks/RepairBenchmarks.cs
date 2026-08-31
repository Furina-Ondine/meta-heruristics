using System.Numerics.Tensors;
using Anastasya.Metaheuristics.Algorithms.Bat;
using Anastasya.Metaheuristics.Core.Execution;
using Anastasya.Metaheuristics.Core.Problems;
using BenchmarkDotNet.Attributes;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>测量标量参考与内置 Tensor Repair 在不同边界形状和位置长度下的成本。</summary>
[MemoryDiagnoser]
public class RepairBenchmarks
{
    private double[] _lower = null!;
    private double[] _upper = null!;
    private double[] _clampSeed = null!;
    private double[] _reflectSeed = null!;
    private double[] _randomResetSeed = null!;
    private double[] _clampPosition = null!;
    private double[] _reflectPosition = null!;
    private double[] _randomResetPosition = null!;
    private ICandidateRepair _clamp = null!;
    private ICandidateRepair _reflect = null!;
    private ICandidateRepair _randomReset = null!;
    private Random _random = null!;
    private double _legacyScalarWidth;
    private double _legacyScalarPeriod;
    private double[]? _legacyWidths;
    private double[]? _legacyPeriods;

    /// <summary>获取或设置 Position 的维度，包含对齐与非对齐的尾部案例。</summary>
    [Params(2, 7, 8, 31, 32, 33, 127, 128, 129, 1024)]
    public int Dimension { get; set; }

    /// <summary>获取或设置 Clamp/Reflect 的标量与向量端点形状。</summary>
    [Params(
        RepairBoundaryShape.ScalarScalar,
        RepairBoundaryShape.VectorVector)]
    public RepairBoundaryShape BoundaryShape { get; set; }

    /// <summary>创建对应形状的边界、参考位置及实际 Repair。</summary>
    [GlobalSetup]
    public void Setup()
    {
        _lower = new double[Dimension];
        _upper = new double[Dimension];
        _clampSeed = new double[Dimension];
        _reflectSeed = new double[Dimension];
        _randomResetSeed = new double[Dimension];
        _clampPosition = new double[Dimension];
        _reflectPosition = new double[Dimension];
        _randomResetPosition = new double[Dimension];

        for (var index = 0; index < Dimension; index++)
        {
            _lower[index] = -10 - (index % 3);
            _upper[index] = 10 + (index % 5);
            var (lower, upper) = GetBounds(index);
            var width = upper - lower;
            _clampSeed[index] = lower + (width * ((index % 7) - 3.25));
            _reflectSeed[index] = lower + (width * ((index % 7) - 3.25));
            _randomResetSeed[index] = lower + (width * ((index % 7) - 3.25));
        }

        _clamp = CreateClamp();
        _reflect = CreateReflect();
        _randomReset = CreateRandomReset();
        CreateLegacyReflectionParameters();
        _random = new Random(1);
    }

    /// <summary>在每次测量前恢复未修复的位置，排除复制以外的运行间状态。</summary>
    [IterationSetup]
    public void ResetPositions()
    {
        _clampSeed.CopyTo(_clampPosition, 0);
        _reflectSeed.CopyTo(_reflectPosition, 0);
        _randomResetSeed.CopyTo(_randomResetPosition, 0);
        _random = new Random(1);
    }

    /// <summary>测量与既有实现相同的 Clamp 标量参考。</summary>
    [Benchmark]
    public void ScalarClamp()
    {
        for (var index = 0; index < _clampPosition.Length; index++)
        {
            var (lower, upper) = GetBounds(index);
            _clampPosition[index] = Clamp(_clampPosition[index], lower, upper);
        }
    }

    /// <summary>测量实际内置 Clamp Repair。</summary>
    [Benchmark]
    public void TensorClamp() => _clamp.Repair(_clampPosition, _random);

    /// <summary>测量与既有实现相同的 Reflect 标量参考。</summary>
    [Benchmark]
    public void ScalarReflect()
    {
        for (var index = 0; index < _reflectPosition.Length; index++)
        {
            var (lower, upper) = GetBounds(index);
            _reflectPosition[index] = Reflect(_reflectPosition[index], lower, upper);
        }
    }

    /// <summary>测量提交 21804bd 中的整段 Tensor Reflect 分派，作为候选内核的同机基线。</summary>
    [Benchmark(Baseline = true)]
    public void LegacyTensorReflect() => RepairWithLegacyTensorReflect(_reflectPosition);

    /// <summary>测量实际内置 Reflect Repair。</summary>
    [Benchmark]
    public void Reflect() => _reflect.Repair(_reflectPosition, _random);

    /// <summary>测量实际内置 RandomReset Repair，位置和随机流在迭代准备阶段重置。</summary>
    [Benchmark]
    public void RandomReset() => _randomReset.Repair(_randomResetPosition, _random);

    private ICandidateRepair CreateClamp() => BoundaryShape switch
    {
        RepairBoundaryShape.ScalarScalar => CandidateRepairs.Clamp(_lower[0], _upper[0]),
        RepairBoundaryShape.VectorVector => CandidateRepairs.Clamp(_lower, _upper),
        _ => throw new InvalidOperationException("The configured boundary shape is unsupported."),
    };

    private ICandidateRepair CreateReflect() => BoundaryShape switch
    {
        RepairBoundaryShape.ScalarScalar => CandidateRepairs.Reflect(_lower[0], _upper[0]),
        RepairBoundaryShape.VectorVector => CandidateRepairs.Reflect(_lower, _upper),
        _ => throw new InvalidOperationException("The configured boundary shape is unsupported."),
    };

    private ICandidateRepair CreateRandomReset() => BoundaryShape switch
    {
        RepairBoundaryShape.ScalarScalar => CandidateRepairs.RandomReset(_lower[0], _upper[0]),
        RepairBoundaryShape.VectorVector => CandidateRepairs.RandomReset(_lower, _upper),
        _ => throw new InvalidOperationException("The configured boundary shape is unsupported."),
    };

    private (double Lower, double Upper) GetBounds(int index) => BoundaryShape switch
    {
        RepairBoundaryShape.ScalarScalar => (_lower[0], _upper[0]),
        RepairBoundaryShape.VectorVector => (_lower[index], _upper[index]),
        _ => throw new InvalidOperationException("The configured boundary shape is unsupported."),
    };

    private void CreateLegacyReflectionParameters()
    {
        if (BoundaryShape == RepairBoundaryShape.ScalarScalar)
        {
            _legacyScalarWidth = _upper[0] - _lower[0];
            _legacyScalarPeriod = _legacyScalarWidth * 2;
            return;
        }

        _legacyWidths = new double[Dimension];
        _legacyPeriods = new double[Dimension];
        for (var index = 0; index < Dimension; index++)
        {
            var (lower, upper) = GetBounds(index);
            _legacyWidths[index] = upper - lower;
            _legacyPeriods[index] = _legacyWidths[index] * 2;
        }
    }

    private void RepairWithLegacyTensorReflect(Span<double> position)
    {
        if (CanUseLegacyTensorPath(position))
        {
            ApplyLegacyTensorReflection(position);
            return;
        }

        for (var index = 0; index < position.Length; index++)
        {
            var (lower, upper) = GetBounds(index);
            position[index] = Reflect(position[index], lower, upper);
        }
    }

    private bool CanUseLegacyTensorPath(ReadOnlySpan<double> position)
    {
        for (var index = 0; index < position.Length; index++)
        {
            var value = position[index];
            var (lower, upper) = GetBounds(index);
            var width = GetLegacyWidth(index);
            var period = GetLegacyPeriod(index);
            var offset = value - lower;
            if (!double.IsFinite(value)
                || !double.IsFinite(lower)
                || !double.IsFinite(upper)
                || !double.IsFinite(width)
                || !double.IsFinite(period)
                || !double.IsFinite(offset)
                || !double.IsFinite(lower + width)
                || width <= 0
                || value == lower
                || value == upper)
            {
                return false;
            }
        }

        return true;
    }

    private double GetLegacyWidth(int index) => _legacyWidths is null ? _legacyScalarWidth : _legacyWidths[index];

    private double GetLegacyPeriod(int index) => _legacyPeriods is null ? _legacyScalarPeriod : _legacyPeriods[index];

    private void ApplyLegacyTensorReflection(Span<double> position)
    {
        if (BoundaryShape == RepairBoundaryShape.VectorVector)
        {
            TensorPrimitives.Subtract(position, _lower, position);
        }
        else
        {
            TensorPrimitives.Subtract(position, _lower[0], position);
        }

        if (_legacyPeriods is null)
        {
            TensorPrimitives.Remainder(position, _legacyScalarPeriod, position);
        }
        else
        {
            TensorPrimitives.Remainder(position, _legacyPeriods, position);
        }

        TensorPrimitives.Abs(position, position);
        if (_legacyWidths is null)
        {
            TensorPrimitives.Subtract(position, _legacyScalarWidth, position);
        }
        else
        {
            TensorPrimitives.Subtract(position, _legacyWidths, position);
        }

        TensorPrimitives.Abs(position, position);
        if (_legacyWidths is null)
        {
            TensorPrimitives.Subtract(_legacyScalarWidth, position, position);
        }
        else
        {
            TensorPrimitives.Subtract(_legacyWidths, position, position);
        }

        if (BoundaryShape == RepairBoundaryShape.VectorVector)
        {
            TensorPrimitives.Add(position, _lower, position);
        }
        else
        {
            TensorPrimitives.Add(position, _lower[0], position);
        }
    }

    private static double Clamp(double value, double lower, double upper)
    {
        if (double.IsNaN(value))
        {
            return value;
        }

        if (value < lower)
        {
            return lower;
        }

        return value > upper ? upper : value;
    }

    private static double Reflect(double value, double lower, double upper)
    {
        if (double.IsNaN(value) || !double.IsFinite(lower) || !double.IsFinite(upper) || !double.IsFinite(value))
        {
            return Clamp(value, lower, upper);
        }

        if (value > lower && value < upper)
        {
            return value;
        }

        var width = upper - lower;
        var period = width * 2;
        if (width <= 0 || !double.IsFinite(width) || !double.IsFinite(period))
        {
            return Clamp(value, lower, upper);
        }

        var offset = value - lower;
        if (!double.IsFinite(offset))
        {
            return Clamp(value, lower, upper);
        }

        var remainder = offset % period;
        if (remainder < 0)
        {
            remainder += period;
        }

        return remainder <= width ? lower + remainder : upper - (remainder - width);
    }
}

/// <summary>表示 Repair 的两端点是标量或逐维向量的组合。</summary>
public enum RepairBoundaryShape
{
    /// <summary>两个端点均为标量。</summary>
    ScalarScalar,

    /// <summary>两个端点均为向量。</summary>
    VectorVector,
}

/// <summary>测量 Bat 在标量参考与内置 Repair 之间的端到端成本。</summary>
[MemoryDiagnoser]
public class BatRepairBenchmarks
{
    private readonly ICandidateInitializer _initializer = new RandomPositionInitializer();
    private BatOptimizerOptions _optimizerOptions = null!;
    private OptimizationRunOptions _runOptions = null!;
    private ContinuousProblem _scalarProblem = null!;
    private ContinuousProblem _tensorProblem = null!;
    private BatOptimizer _scalarOptimizer = null!;
    private BatOptimizer _tensorOptimizer = null!;

    /// <summary>获取或设置候选位置维度。</summary>
    [Params(32, 128)]
    public int Dimension { get; set; }

    /// <summary>获取或设置要测量的内置 Repair 行为。</summary>
    [Params(BatRepairKind.Clamp, BatRepairKind.Reflect)]
    public BatRepairKind RepairKind { get; set; }

    /// <summary>创建使用等价标量参考与内置 Repair 的两个问题。</summary>
    [GlobalSetup]
    public void Setup()
    {
        ICandidateRepair scalarRepair = RepairKind == BatRepairKind.Clamp
            ? new ScalarClampRepair(-5, 5)
            : new ScalarReflectRepair(-5, 5);
        var tensorRepair = RepairKind == BatRepairKind.Clamp
            ? CandidateRepairs.Clamp(-5, 5)
            : CandidateRepairs.Reflect(-5, 5);
        _scalarProblem = new ContinuousProblem(Dimension, new SphereObjective(), scalarRepair);
        _tensorProblem = new ContinuousProblem(Dimension, new SphereObjective(), tensorRepair);
        _optimizerOptions = new BatOptimizerOptions { PopulationSize = 64 };
        _runOptions = new OptimizationRunOptions(StoppingConditions.MaxIterations(5));
        _scalarOptimizer = new BatOptimizer(_initializer, _optimizerOptions);
        _tensorOptimizer = new BatOptimizer(_initializer, _optimizerOptions);
        OptimizationRunner.Execute(_scalarProblem, _scalarOptimizer, _runOptions, seed: -1);
        OptimizationRunner.Execute(_tensorProblem, _tensorOptimizer, _runOptions, seed: -1);
    }

    /// <summary>测量标量参考 Repair 下的完整 Bat run。</summary>
    [Benchmark(Baseline = true)]
    public double ScalarRepair()
    {
        return OptimizationRunner.Execute(_scalarProblem, _scalarOptimizer, _runOptions, seed: 1).BestEvaluation.Objective;
    }

    /// <summary>测量内置 Tensor Repair 下的完整 Bat run。</summary>
    [Benchmark]
    public double TensorRepair()
    {
        return OptimizationRunner.Execute(_tensorProblem, _tensorOptimizer, _runOptions, seed: 1).BestEvaluation.Objective;
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

    private sealed class RandomPositionInitializer : ICandidateInitializer
    {
        public void Initialize(Span<double> position, Random random)
        {
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = (random.NextDouble() * 20) - 10;
            }
        }
    }

    private sealed class ScalarClampRepair(double lower, double upper) : ICandidateRepair
    {
        public void Repair(Span<double> position, Random random)
        {
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = Clamp(position[index], lower, upper);
            }
        }
    }

    private sealed class ScalarReflectRepair(double lower, double upper) : ICandidateRepair
    {
        public void Repair(Span<double> position, Random random)
        {
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = Reflect(position[index], lower, upper);
            }
        }
    }

    private static double Clamp(double value, double lower, double upper)
    {
        if (double.IsNaN(value))
        {
            return value;
        }

        if (value < lower)
        {
            return lower;
        }

        return value > upper ? upper : value;
    }

    private static double Reflect(double value, double lower, double upper)
    {
        if (double.IsNaN(value) || !double.IsFinite(lower) || !double.IsFinite(upper) || !double.IsFinite(value))
        {
            return Clamp(value, lower, upper);
        }

        if (value > lower && value < upper)
        {
            return value;
        }

        var width = upper - lower;
        var period = width * 2;
        if (width <= 0 || !double.IsFinite(width) || !double.IsFinite(period))
        {
            return Clamp(value, lower, upper);
        }

        var offset = value - lower;
        if (!double.IsFinite(offset))
        {
            return Clamp(value, lower, upper);
        }

        var remainder = offset % period;
        if (remainder < 0)
        {
            remainder += period;
        }

        return remainder <= width ? lower + remainder : upper - (remainder - width);
    }
}

/// <summary>选择 Bat 端到端基准中的 Repair 策略。</summary>
public enum BatRepairKind
{
    /// <summary>包含式 Clamp。</summary>
    Clamp,

    /// <summary>有限区间 Reflect。</summary>
    Reflect,
}
