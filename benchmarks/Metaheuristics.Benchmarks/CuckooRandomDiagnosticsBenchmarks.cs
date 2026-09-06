using Anastasya.Metaheuristics.Core.Randomness;
using BenchmarkDotNet.Attributes;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>
/// 测量 Cuckoo 当前生产循环实际使用的标量随机请求组合。
/// </summary>
[MemoryDiagnoser]
public class CuckooRandomDiagnosticsBenchmarks
{
    private const ulong Seed = 0x1357_9BDF_2468_ACE0UL;
    private const int PopulationSize = 64;

    private RandomSource _randomSource = null!;
    private System.Random _systemRandom = null!;
    private bool _randomSourceHasSpare;
    private bool _systemRandomHasSpare;
    private double _randomSourceSpare;
    private double _systemRandomSpare;

    [Params(32, 128)]
    public int Dimension { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _randomSource = new RandomSource(Seed);
        _systemRandom = new System.Random(unchecked((int)Seed));
        _randomSourceHasSpare = false;
        _systemRandomHasSpare = false;
        _randomSourceSpare = 0;
        _systemRandomSpare = 0;
    }

    /// <summary>
    /// 测量 Cuckoo Lévy 和引导路径使用的单位区间标量 double。
    /// </summary>
    [Benchmark]
    public double RandomSourceNextDouble()
    {
        var sum = 0.0;
        for (var index = 0; index < Dimension; index++)
        {
            sum += _randomSource.NextDouble();
        }

        return sum;
    }

    /// <summary>
    /// 测量 seeded System.Random 的单位区间标量 double 对照。
    /// </summary>
    [Benchmark]
    public double SystemRandomNextDouble()
    {
        var sum = 0.0;
        for (var index = 0; index < Dimension; index++)
        {
            sum += _systemRandom.NextDouble();
        }

        return sum;
    }

    /// <summary>
    /// 测量 Cuckoo 弃巢索引使用的半开 [0, 64) 整数。
    /// </summary>
    [Benchmark]
    public int RandomSourceNextPopulationIndex()
    {
        var sum = 0;
        for (var index = 0; index < Dimension; index++)
        {
            sum += _randomSource.NextInt(0, PopulationSize);
        }

        return sum;
    }

    /// <summary>
    /// 测量 seeded System.Random 的半开 [0, 64) 整数对照。
    /// </summary>
    [Benchmark]
    public int SystemRandomNextPopulationIndex()
    {
        var sum = 0;
        for (var index = 0; index < Dimension; index++)
        {
            sum += _systemRandom.Next(0, PopulationSize);
        }

        return sum;
    }

    /// <summary>
    /// 测量 Cuckoo 私有带 spare Box–Muller 的标量组合。
    /// </summary>
    [Benchmark]
    public double RandomSourceGaussianWithSpare()
    {
        var sum = 0.0;
        for (var index = 0; index < Dimension; index++)
        {
            sum += NextRandomSourceGaussianWithSpare();
        }

        return sum;
    }

    /// <summary>
    /// 测量旧 seeded System.Random 带 spare Box–Muller 的对照组合。
    /// </summary>
    [Benchmark]
    public double SystemRandomGaussianWithSpare()
    {
        var sum = 0.0;
        for (var index = 0; index < Dimension; index++)
        {
            sum += NextSystemRandomGaussianWithSpare();
        }

        return sum;
    }

    private double NextRandomSourceGaussianWithSpare()
    {
        if (_randomSourceHasSpare)
        {
            _randomSourceHasSpare = false;
            return _randomSourceSpare;
        }

        var first = Math.Max(_randomSource.NextDouble(), double.Epsilon);
        var second = _randomSource.NextDouble();
        var radius = Math.Sqrt(-2 * Math.Log(first));
        var angle = 2 * Math.PI * second;
        _randomSourceSpare = radius * Math.Sin(angle);
        _randomSourceHasSpare = true;
        return radius * Math.Cos(angle);
    }

    private double NextSystemRandomGaussianWithSpare()
    {
        if (_systemRandomHasSpare)
        {
            _systemRandomHasSpare = false;
            return _systemRandomSpare;
        }

        var first = Math.Max(_systemRandom.NextDouble(), double.Epsilon);
        var second = _systemRandom.NextDouble();
        var radius = Math.Sqrt(-2 * Math.Log(first));
        var angle = 2 * Math.PI * second;
        _systemRandomSpare = radius * Math.Sin(angle);
        _systemRandomHasSpare = true;
        return radius * Math.Cos(angle);
    }
}
