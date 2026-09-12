using Anastasya.Metaheuristics.Benchmarks.References;
using Anastasya.Metaheuristics.Core.Randomness;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace Anastasya.Metaheuristics.Benchmarks;

/// <summary>
/// 随机源自身吞吐对比的共用设置：基线 A、候选 B 与标量参考 R 使用相同种子。
/// </summary>
/// <remarks>
/// 每个场景单独一个基准类，BenchmarkDotNet 的 Ratio 列因此始终是
/// 「候选相对本法基线的倍数」：大于 1 表示候选更快。所有方法都返回最后写入的样本，
/// 避免写入被优化掉。
/// </remarks>
public abstract class RandomSourceComparisonBenchmarksBase
{
    internal const ulong Seed = 0x0123_4567_89AB_CDEFUL;
    internal const int BoundedIntegerMinimum = -5;
    internal const int BoundedIntegerMaximum = 5;
    internal const double BoundedDoubleMinimum = -5;
    internal const double BoundedDoubleMaximum = 5;

    private protected RandomSourceBaseline Baseline = null!;
    private protected RandomSource Candidate = null!;
    private protected RandomSourceScalarReference Reference = null!;
    private protected ulong[] RawValues = null!;
    private protected double[] DoubleValues = null!;
    private protected int[] IntValues = null!;

    [Params(32, 128)]
    public int Length { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        Baseline = new RandomSourceBaseline(Seed);
        Candidate = new RandomSource(Seed);
        Reference = new RandomSourceScalarReference(Seed);
        RawValues = new ulong[Length];
        DoubleValues = new double[Length];
        IntValues = new int[Length];
    }

    private protected static double Last(ulong[] values) => values.Length == 0 ? 0 : values[^1];

    private protected static double Last(double[] values) => values.Length == 0 ? 0 : values[^1];

    private protected static double Last(int[] values) => values.Length == 0 ? 0 : values[^1];
}

/// <summary>
/// 记录热路径的实际机器码，作为批量状态算术确实以向量指令执行的证据。
/// </summary>
/// <remarks>
/// 该基准不用于吞吐结论，只用于查看候选实现编译后的状态算术、转换和正态数学路径。
/// </remarks>
[DisassemblyDiagnoser(maxDepth: 2)]
public class RsJitDiagnosticsBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark]
    public double RawFill()
    {
        Candidate.Fill(RawValues);
        return Last(RawValues);
    }

    [Benchmark]
    public double UnitFill()
    {
        Candidate.Fill(DoubleValues);
        return Last(DoubleValues);
    }

    [Benchmark]
    public double BoundedIntFill()
    {
        Candidate.Fill(IntValues, BoundedIntegerMinimum, BoundedIntegerMaximum);
        return Last(IntValues);
    }

    [Benchmark]
    public double NormalFill()
    {
        StandardNormal.Fill(Candidate, DoubleValues);
        return Last(DoubleValues);
    }

    [Benchmark]
    public ulong VectorApi() => Candidate.NextULongVector()[0];
}

/// <summary>
/// 原始 ulong 批量填充：候选相对旧单流基线与标量参考。
/// </summary>
[MemoryDiagnoser]
public class RsRawFillBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark(Baseline = true)]
    public double BaselineFill()
    {
        Baseline.Fill(RawValues);
        return Last(RawValues);
    }

    [Benchmark]
    public double CandidateFill()
    {
        Candidate.Fill(RawValues);
        return Last(RawValues);
    }

    [Benchmark]
    public double ScalarReferenceFill()
    {
        Reference.Fill(RawValues);
        return Last(RawValues);
    }
}

/// <summary>
/// 单位 double 批量填充：候选相对旧单流基线与标量参考。
/// </summary>
[MemoryDiagnoser]
public class RsUnitFillBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark(Baseline = true)]
    public double BaselineFill()
    {
        Baseline.Fill(DoubleValues);
        return Last(DoubleValues);
    }

    [Benchmark]
    public double CandidateFill()
    {
        Candidate.Fill(DoubleValues);
        return Last(DoubleValues);
    }

    [Benchmark]
    public double ScalarReferenceFill()
    {
        Reference.Fill(DoubleValues);
        return Last(DoubleValues);
    }
}

/// <summary>
/// 有界 double 批量填充：候选相对旧单流基线。
/// </summary>
[MemoryDiagnoser]
public class RsBoundedDoubleFillBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark(Baseline = true)]
    public double BaselineFill()
    {
        Baseline.Fill(DoubleValues, BoundedDoubleMinimum, BoundedDoubleMaximum);
        return Last(DoubleValues);
    }

    [Benchmark]
    public double CandidateFill()
    {
        Candidate.Fill(DoubleValues, BoundedDoubleMinimum, BoundedDoubleMaximum);
        return Last(DoubleValues);
    }
}

/// <summary>
/// 有界整数批量填充：候选相对旧单流基线。
/// </summary>
[MemoryDiagnoser]
public class RsBoundedIntFillBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark(Baseline = true)]
    public double BaselineFill()
    {
        Baseline.Fill(IntValues, BoundedIntegerMinimum, BoundedIntegerMaximum);
        return Last(IntValues);
    }

    [Benchmark]
    public double CandidateFill()
    {
        Candidate.Fill(IntValues, BoundedIntegerMinimum, BoundedIntegerMaximum);
        return Last(IntValues);
    }
}

/// <summary>
/// 标准正态批量填充：候选相对旧单流基线。
/// </summary>
[MemoryDiagnoser]
public class RsNormalFillBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark(Baseline = true)]
    public double BaselineFill()
    {
        StandardNormalBaseline.Fill(Baseline, DoubleValues);
        return Last(DoubleValues);
    }

    [Benchmark]
    public double CandidateFill()
    {
        StandardNormal.Fill(Candidate, DoubleValues);
        return Last(DoubleValues);
    }
}

/// <summary>
/// 单值原始 ulong：候选相对旧单流基线。
/// </summary>
[MemoryDiagnoser]
public class RsScalarNextULongBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark(Baseline = true)]
    public ulong BaselineNext() => Baseline.NextULong();

    [Benchmark]
    public ulong CandidateNext() => Candidate.NextULong();
}

/// <summary>
/// 单值单位 double：候选相对旧单流基线。
/// </summary>
[MemoryDiagnoser]
public class RsScalarNextDoubleBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark(Baseline = true)]
    public double BaselineNext() => Baseline.NextDouble();

    [Benchmark]
    public double CandidateNext() => Candidate.NextDouble();
}

/// <summary>
/// 单值有界整数：候选相对旧单流基线。
/// </summary>
[MemoryDiagnoser]
public class RsScalarNextIntBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark(Baseline = true)]
    public int BaselineNext() => Baseline.NextInt(BoundedIntegerMinimum, BoundedIntegerMaximum);

    [Benchmark]
    public int CandidateNext() => Candidate.NextInt(BoundedIntegerMinimum, BoundedIntegerMaximum);
}

/// <summary>
/// 单值标准正态：候选相对旧单流基线。
/// </summary>
[MemoryDiagnoser]
public class RsNormalSampleBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark(Baseline = true)]
    public double BaselineSample() => StandardNormalBaseline.Sample(Baseline);

    [Benchmark]
    public double CandidateSample() => StandardNormal.Sample(Candidate);
}

/// <summary>
/// 向量原始样本 API：候选相对标量参考 R，R 每次同样生成一轮 lane 的样本。
/// </summary>
[MemoryDiagnoser]
public class RsVectorApiRawBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark(Baseline = true)]
    public ulong ScalarReferenceNext() => Reference.NextULongVector()[0];

    [Benchmark]
    public ulong CandidateNext() => Candidate.NextULongVector()[0];
}

/// <summary>
/// 向量单位样本 API：候选相对标量参考 R。
/// </summary>
[MemoryDiagnoser]
public class RsVectorApiUnitBenchmarks : RandomSourceComparisonBenchmarksBase
{
    [Benchmark(Baseline = true)]
    public double ScalarReferenceNext() => Reference.NextDoubleVector()[0];

    [Benchmark]
    public double CandidateNext() => Candidate.NextDoubleVector()[0];
}

/// <summary>
/// 实例构造加一次单值采样：暴露车道初始化的固定成本。
/// </summary>
[MemoryDiagnoser]
public class RsStartupBenchmarks
{
    private readonly ulong _seed = RandomSourceComparisonBenchmarksBase.Seed;

    [Benchmark(Baseline = true)]
    public int BaselineConstruct() => new RandomSourceBaseline(_seed).NextInt(0, 2);

    [Benchmark]
    public int CandidateConstruct() => new RandomSource(_seed).NextInt(0, 2);
}

/// <summary>
/// 构造后立即执行一次 32 长度批量填充：暴露初始化在短调用中的占比。
/// </summary>
[MemoryDiagnoser]
public class RsStartupWithFillBenchmarks
{
    private double[] _values = null!;

    [GlobalSetup]
    public void Setup() => _values = new double[32];

    [Benchmark(Baseline = true)]
    public double BaselineConstructAndFill()
    {
        var source = new RandomSourceBaseline(RandomSourceComparisonBenchmarksBase.Seed);
        source.Fill(_values);
        return Last(_values);
    }

    [Benchmark]
    public double CandidateConstructAndFill()
    {
        var source = new RandomSource(RandomSourceComparisonBenchmarksBase.Seed);
        source.Fill(_values);
        return Last(_values);
    }

    private static double Last(double[] values) => values.Length == 0 ? 0 : values[^1];
}
