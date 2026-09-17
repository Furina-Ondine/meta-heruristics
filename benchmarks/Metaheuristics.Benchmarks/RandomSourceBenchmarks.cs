using Anastasya.Metaheuristics.Core.Randomness;
using BenchmarkDotNet.Attributes;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>
/// 比较运行时随机源的标量循环、Fill 路径和带固定种子的 System.Random 循环。
/// </summary>
[MemoryDiagnoser]
public class RandomSourceBenchmarks
{
    private const ulong Seed = 0x0123_4567_89AB_CDEFUL;
    private const int BoundedIntegerMinimum = -17;
    private const int BoundedIntegerMaximum = 29;
    private const double BoundedDoubleMinimum = -5;
    private const double BoundedDoubleMaximum = 5;

    private ulong[] _ulongValues = null!;
    private double[] _doubleValues = null!;
    private int[] _integerValues = null!;
    private RandomSource _source = null!;
    private System.Random _systemRandom = null!;

    [Params(0, 1, 2, 7, 8, 31, 32, 33, 127, 128, 129)]
    public int Length { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _ulongValues = new ulong[Length];
        _doubleValues = new double[Length];
        _integerValues = new int[Length];
        _source = new RandomSource(Seed);
        _systemRandom = new System.Random(unchecked((int)Seed));
    }

    [Benchmark]
    public double RandomSourceFillULong()
    {
        _source.Fill(_ulongValues);
        return Last(_ulongValues);
    }

    [Benchmark]
    public double RandomSourceScalarULong()
    {
        for (var index = 0; index < _ulongValues.Length; index++)
        {
            _ulongValues[index] = _source.NextULong();
        }

        return Last(_ulongValues);
    }

    [Benchmark]
    public double SystemRandomScalarULong()
    {
        for (var index = 0; index < _ulongValues.Length; index++)
        {
            var high = (ulong)_systemRandom.NextInt64(1L << 32);
            var low = (ulong)_systemRandom.NextInt64(1L << 32);
            _ulongValues[index] = (high << 32) | low;
        }

        return Last(_ulongValues);
    }

    [Benchmark]
    public double RandomSourceFillUnitDouble()
    {
        _source.Fill(_doubleValues);
        return Last(_doubleValues);
    }

    [Benchmark]
    public double RandomSourceScalarUnitDouble()
    {
        for (var index = 0; index < _doubleValues.Length; index++)
        {
            _doubleValues[index] = _source.NextDouble();
        }

        return Last(_doubleValues);
    }

    [Benchmark]
    public double SystemRandomScalarUnitDouble()
    {
        for (var index = 0; index < _doubleValues.Length; index++)
        {
            _doubleValues[index] = _systemRandom.NextDouble();
        }

        return Last(_doubleValues);
    }

    [Benchmark]
    public double RandomSourceFillBoundedDouble()
    {
        _source.Fill(_doubleValues, BoundedDoubleMinimum, BoundedDoubleMaximum);
        return Last(_doubleValues);
    }

    [Benchmark]
    public double RandomSourceScalarBoundedDouble()
    {
        for (var index = 0; index < _doubleValues.Length; index++)
        {
            _doubleValues[index] = _source.NextDouble(BoundedDoubleMinimum, BoundedDoubleMaximum);
        }

        return Last(_doubleValues);
    }

    [Benchmark]
    public double SystemRandomScalarBoundedDouble()
    {
        const double width = BoundedDoubleMaximum - BoundedDoubleMinimum;
        for (var index = 0; index < _doubleValues.Length; index++)
        {
            _doubleValues[index] = BoundedDoubleMinimum + (width * _systemRandom.NextDouble());
        }

        return Last(_doubleValues);
    }

    [Benchmark]
    public double RandomSourceScalarBoundedInt()
    {
        for (var index = 0; index < _integerValues.Length; index++)
        {
            _integerValues[index] = _source.NextInt(BoundedIntegerMinimum, BoundedIntegerMaximum);
        }

        return Last(_integerValues);
    }

    [Benchmark]
    public double SystemRandomScalarBoundedInt()
    {
        for (var index = 0; index < _integerValues.Length; index++)
        {
            _integerValues[index] = _systemRandom.Next(BoundedIntegerMinimum, BoundedIntegerMaximum);
        }

        return Last(_integerValues);
    }

    private static double Last(ulong[] values) => values.Length == 0 ? 0 : values[^1];

    private static double Last(double[] values) => values.Length == 0 ? 0 : values[^1];

    private static double Last(int[] values) => values.Length == 0 ? 0 : values[^1];
}
