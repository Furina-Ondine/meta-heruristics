using Anastasya.Metaheuristics.Core.Randomness;
using BenchmarkDotNet.Attributes;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>
/// 比较生产无跨调用 spare 的 Box–Muller 实现和独立参考实现。
/// </summary>
[MemoryDiagnoser]
public class StandardNormalBenchmarks
{
    private const ulong Seed = 0x0BAD_F00D_CAFE_BEEFUL;

    private double[] _values = null!;
    private RandomSource _productionSource = null!;
    private RandomSource _referenceSource = null!;
    private System.Random _systemRandom = null!;
    private bool _hasSpare;
    private double _spare;

    [Params(0, 1, 2, 7, 8, 31, 32, 33, 127, 128, 129)]
    public int Length { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _values = new double[Length];
        _productionSource = new RandomSource(Seed);
        _referenceSource = new RandomSource(Seed);
        _systemRandom = new System.Random(unchecked((int)Seed));
        _hasSpare = false;
        _spare = 0;
    }

    [Benchmark]
    public double StandardNormalSample() => StandardNormal.Sample(_productionSource);

    [Benchmark]
    public double ReferenceSourceBoxMullerSample() => NextWithoutSpare(_referenceSource, out _);

    [Benchmark]
    public double CuckooReferenceSample() => NextSystemRandomWithSpare();

    [Benchmark]
    public double StandardNormalFill()
    {
        StandardNormal.Fill(_productionSource, _values);
        return Last(_values);
    }

    [Benchmark]
    public double ReferenceSourceBoxMullerFill()
    {
        FillWithoutSpare(_referenceSource, _values);
        return Last(_values);
    }

    [Benchmark]
    public double CuckooReferenceFill()
    {
        for (var index = 0; index < _values.Length; index++)
        {
            _values[index] = NextSystemRandomWithSpare();
        }

        return Last(_values);
    }

    private static void FillWithoutSpare(RandomSource random, Span<double> destination)
    {
        var index = 0;
        for (; index + 1 < destination.Length; index += 2)
        {
            var first = NextWithoutSpare(random, out var second);
            destination[index] = first;
            destination[index + 1] = second;
        }

        if (index < destination.Length)
        {
            destination[index] = NextWithoutSpare(random, out _);
        }
    }

    private static double NextWithoutSpare(RandomSource random, out double discarded)
    {
        var first = Math.Max(random.NextDouble(), double.Epsilon);
        var second = random.NextDouble();
        var radius = Math.Sqrt(-2 * Math.Log(first));
        var angle = 2 * Math.PI * second;
        discarded = radius * Math.Sin(angle);
        return radius * Math.Cos(angle);
    }

    private double NextSystemRandomWithSpare()
    {
        if (_hasSpare)
        {
            _hasSpare = false;
            return _spare;
        }

        var first = Math.Max(_systemRandom.NextDouble(), double.Epsilon);
        var second = _systemRandom.NextDouble();
        var radius = Math.Sqrt(-2 * Math.Log(first));
        var angle = 2 * Math.PI * second;
        _spare = radius * Math.Sin(angle);
        _hasSpare = true;
        return radius * Math.Cos(angle);
    }

    private static double Last(double[] values) => values.Length == 0 ? 0 : values[^1];
}
