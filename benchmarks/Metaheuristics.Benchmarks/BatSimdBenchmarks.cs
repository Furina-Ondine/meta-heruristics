using System;
using Anastasya.Metaheuristics.Algorithms;
using BenchmarkDotNet.Attributes;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>比较 Bat 完整候选算术数据流的标量分支与私有掩码 SIMD。</summary>
[MemoryDiagnoser]
public class BatCandidateArithmeticBenchmarks
{
    private double[] _acceptanceSamples = null!;
    private double[] _actualInitialPulseRate = null!;
    private double[] _actualLoudness = null!;
    private double[] _actualPosition = null!;
    private double[] _actualPulseRate = null!;
    private double[] _actualVelocity = null!;
    private double[] _bestPosition = null!;
    private double[] _frequency = null!;
    private double[] _perturbationSamples = null!;
    private double[] _pulseSamples = null!;
    private double[] _sourceInitialPulseRate = null!;
    private double[] _sourceLoudness = null!;
    private double[] _sourcePosition = null!;
    private double[] _sourcePulseRate = null!;
    private double[] _sourceVelocity = null!;

    /// <summary>获取或设置一次完整 Bat 候选操作的维度。</summary>
    [ParamsSource(nameof(Dimensions))]
    public int Dimension { get; set; }

    /// <summary>提供完整诊断维度，或按环境开关只提供主要验收维度。</summary>
    public static IEnumerable<int> Dimensions =>
        Environment.GetEnvironmentVariable("METAHEURISTICS_BENCHMARK_PRIMARY_ONLY") == "1"
            ? [32, 128]
            : [24, 25, 32, 64, 65, 128];

    /// <summary>创建覆盖接受/拒绝和两条位置路径的稳定输入。</summary>
    [GlobalSetup]
    public void Setup()
    {
        _sourcePosition = new double[Dimension];
        _sourceVelocity = new double[Dimension];
        _sourceLoudness = new double[Dimension];
        _sourcePulseRate = new double[Dimension];
        _sourceInitialPulseRate = new double[Dimension];
        _bestPosition = new double[Dimension];
        _frequency = new double[Dimension];
        _pulseSamples = new double[Dimension];
        _perturbationSamples = new double[Dimension];
        _acceptanceSamples = new double[Dimension];
        _actualPosition = new double[Dimension];
        _actualVelocity = new double[Dimension];
        _actualLoudness = new double[Dimension];
        _actualPulseRate = new double[Dimension];
        _actualInitialPulseRate = new double[Dimension];

        for (var index = 0; index < Dimension; index++)
        {
            _sourcePosition[index] = (index % 13) - 6;
            _sourceVelocity[index] = ((index % 9) - 4) * 0.25;
            _sourceLoudness[index] = (index % 5) * 0.25;
            _sourcePulseRate[index] = (index % 4) * 0.25;
            _sourceInitialPulseRate[index] = 0.2 + ((index % 3) * 0.1);
            _bestPosition[index] = _sourcePosition[index] + ((index % 7) - 3);
            _frequency[index] = (index % 6) * 0.2;
            _pulseSamples[index] = (index % 8) * 0.125;
            _perturbationSamples[index] = ((index % 11) - 5) * 0.1;
            _acceptanceSamples[index] = ((index + 3) % 8) * 0.125;
        }
    }

    /// <summary>测量完整标量数据流。</summary>
    [Benchmark(Baseline = true)]
    public double Scalar()
    {
        for (var index = 0; index < Dimension; index++)
        {
            var velocity = _sourceVelocity[index]
                + ((_bestPosition[index] - _sourcePosition[index]) * _frequency[index]);
            _actualVelocity[index] = Math.Clamp(velocity, -1, 1);
            var nextPosition = _pulseSamples[index] > _sourcePulseRate[index]
                ? _bestPosition[index] + (_perturbationSamples[index] * _sourceLoudness[index])
                : _sourcePosition[index] + _actualVelocity[index];
            _actualInitialPulseRate[index] = _sourceInitialPulseRate[index];
            if (_acceptanceSamples[index] < _sourceLoudness[index])
            {
                _actualPosition[index] = nextPosition;
                _actualLoudness[index] = 0.9 * _sourceLoudness[index];
                _actualPulseRate[index] = _sourceInitialPulseRate[index] * 0.4;
            }
            else
            {
                _actualPosition[index] = _sourcePosition[index];
                _actualLoudness[index] = _sourceLoudness[index];
                _actualPulseRate[index] = _sourcePulseRate[index];
            }
        }

        return _actualPosition[0] + _actualVelocity[^1];
    }

    /// <summary>测量一次完整的私有掩码 SIMD 数据流。</summary>
    [Benchmark]
    public double VectorOpsCandidate()
    {
        VectorOps.UpdateBatCandidate(
            _sourcePosition,
            _sourceVelocity,
            _sourceLoudness,
            _sourcePulseRate,
            _sourceInitialPulseRate,
            _bestPosition,
            _frequency,
            _pulseSamples,
            _perturbationSamples,
            _acceptanceSamples,
            velocityLowerBound: -1,
            velocityUpperBound: 1,
            loudnessDecay: 0.9,
            pulseRateFactor: 0.4,
            _actualPosition,
            _actualVelocity,
            _actualLoudness,
            _actualPulseRate,
            _actualInitialPulseRate);
        return _actualPosition[0] + _actualVelocity[^1];
    }
}
