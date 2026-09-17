using Anastasya.Metaheuristics.Algorithms;
using BenchmarkDotNet.Attributes;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>比较 Cuckoo 同样本标量公式与保留 Math.Pow 的融合算术。</summary>
[MemoryDiagnoser]
public class CuckooCandidateArithmeticBenchmarks
{
    private double[] _source = null!;
    private double[] _best = null!;
    private double[] _first = null!;
    private double[] _second = null!;
    private double[] _numerator = null!;
    private double[] _rawDenominator = null!;
    private double[] _denominator = null!;
    private double[] _unit = null!;
    private double[] _scalarLevy = null!;
    private double[] _fusedLevy = null!;
    private double[] _scalarAbandonment = null!;
    private double[] _fusedAbandonment = null!;

    /// <summary>获取或设置候选维度。</summary>
    [ParamsSource(nameof(Dimensions))]
    public int Dimension { get; set; }

    /// <summary>提供完整诊断维度，或按环境开关只提供主要验收维度。</summary>
    public static IEnumerable<int> Dimensions =>
        Environment.GetEnvironmentVariable("METAHEURISTICS_BENCHMARK_PRIMARY_ONLY") == "1"
            ? [32, 128]
            : [24, 25, 32, 64, 65, 128];

    /// <summary>建立两条路径共享的确定性样本。</summary>
    [GlobalSetup]
    public void Setup()
    {
        _source = new double[Dimension];
        _best = new double[Dimension];
        _first = new double[Dimension];
        _second = new double[Dimension];
        _numerator = new double[Dimension];
        _rawDenominator = new double[Dimension];
        _denominator = new double[Dimension];
        _unit = new double[Dimension];
        _scalarLevy = new double[Dimension];
        _fusedLevy = new double[Dimension];
        _scalarAbandonment = new double[Dimension];
        _fusedAbandonment = new double[Dimension];

        for (var index = 0; index < Dimension; index++)
        {
            _source[index] = (index % 11) - 5;
            _best[index] = _source[index] + ((index % 5) - 2) * 0.25;
            _first[index] = _source[index] + 1;
            _second[index] = _source[index] - 0.5;
            _numerator[index] = ((index % 7) - 3) * 0.3;
            _rawDenominator[index] = ((index % 9) - 4) * 0.2;
            _unit[index] = (index % 17) / 17.0;
        }
    }

    /// <summary>测量保留原运算顺序的 Math.Pow 与逐维 Lévy 位置公式。</summary>
    [Benchmark(Baseline = true)]
    public void ScalarLevyCandidate()
    {
        _rawDenominator.AsSpan().CopyTo(_denominator);
        for (var index = 0; index < Dimension; index++)
        {
            _denominator[index] = Math.Pow(Math.Abs(_denominator[index]) + 1e-10, 1 / 1.5);
        }

        for (var index = 0; index < Dimension; index++)
        {
            var numerator = _numerator[index] * 0.7 * 0.9;
            var levyStep = 0.2 * numerator / _denominator[index];
            var guidance = (_best[index] - _source[index]) * _unit[index];
            _scalarLevy[index] = _source[index] + (0.8 * levyStep) + (0.2 * guidance);
        }
    }

    /// <summary>测量相同 Math.Pow 阶段加融合 Lévy 位置写回。</summary>
    [Benchmark]
    public void FusedLevyCandidate()
    {
        _rawDenominator.AsSpan().CopyTo(_denominator);
        for (var index = 0; index < Dimension; index++)
        {
            _denominator[index] = Math.Pow(Math.Abs(_denominator[index]) + 1e-10, 1 / 1.5);
        }

        VectorOps.UpdateCuckooLevyCandidate(
            _source, _best, _numerator, _denominator, _unit,
            levySigma: 0.7, gaussianScale: 0.9, levyScale: 0.2, _fusedLevy);
    }

    /// <summary>测量逐维遗弃位置公式。</summary>
    [Benchmark]
    public void ScalarAbandonmentCandidate()
    {
        for (var index = 0; index < Dimension; index++)
        {
            var perturbation = (_unit[index] - 0.5) * 0.1;
            _scalarAbandonment[index] = _best[index]
                + (0.4 * (_first[index] - _second[index]))
                + perturbation;
        }
    }

    /// <summary>测量融合遗弃位置写回。</summary>
    [Benchmark]
    public void FusedAbandonmentCandidate()
    {
        VectorOps.UpdateCuckooAbandonmentCandidate(
            _best, _first, _second, _unit,
            differenceScale: 0.4, perturbationScale: 0.1, _fusedAbandonment);
    }
}
