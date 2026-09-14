using System.Numerics;
using Anastasya.Metaheuristics.Core.Randomness;

namespace Anastasya.Metaheuristics.Tests.Core;

/// <summary>
/// 验证标准正态采样、批量行为、统计性质和无分配执行路径。
/// </summary>
public sealed class StandardNormalTests
{
    /// <summary>
    /// 验证空引用校验和空批量调用保持随机源状态。
    /// </summary>
    [Xunit.Fact]
    public void NullAndEmptyInputsFollowTheContract()
    {
        Xunit.Assert.Throws<ArgumentNullException>(() => StandardNormal.Sample(null!));
        Xunit.Assert.Throws<ArgumentNullException>(() => StandardNormal.Fill(null!, Span<double>.Empty));

        var source = new RandomSource(123);
        var expected = new RandomSource(123);
        StandardNormal.Fill(source, Span<double>.Empty);
        Xunit.Assert.Equal(expected.NextULong(), source.NextULong());
    }

    /// <summary>
    /// 验证批量正态的双向量配对、尾部处理、向量数学结果与块消费量。
    /// </summary>
    [Xunit.Fact]
    public void VectorFillMatchesScalarBoxMullerReferenceAndConsumesWholeRounds()
    {
        const ulong seed = 0x0BADF00D_12345678;
        var laneCount = Vector<double>.Count;
        foreach (var length in new[] { 1, 2, laneCount - 1, laneCount, laneCount + 1, (3 * laneCount) + 2 })
        {
            var source = new RandomSource(seed);
            var units = new RandomSource(seed);
            var untouched = new RandomSource(seed);
            var values = new double[length];

            StandardNormal.Fill(source, values);

            var expected = new List<double>(length);
            while (expected.Count < length)
            {
                var radiusInput = units.NextDoubleVector();
                var angleInput = units.NextDoubleVector();
                var cosines = new List<double>(laneCount);
                var sines = new List<double>(laneCount);
                for (var lane = 0; lane < laneCount; lane++)
                {
                    var radius = Math.Sqrt(-2 * Math.Log(1 - radiusInput[lane]));
                    var angle = 2 * Math.PI * angleInput[lane];
                    cosines.Add(radius * Math.Cos(angle));
                    sines.Add(radius * Math.Sin(angle));
                }

                // 写出是向量粒度：整条 cos 结果在前，整条 sin 结果在后。
                foreach (var value in cosines.Concat(sines))
                {
                    if (expected.Count == length)
                    {
                        break;
                    }

                    expected.Add(value);
                }
            }

            for (var index = 0; index < length; index++)
            {
                var reference = expected[index];
                Xunit.Assert.True(double.IsFinite(values[index]));
                Xunit.Assert.True(
                    Math.Abs(values[index] - reference) <= 1e-12 + (1e-12 * Math.Abs(reference)),
                    $"index {index}: {values[index]} vs {reference}");
            }

            // 每个块消耗两轮批量状态（2*L 个输出），尾部不足一块时仍消耗完整一块；
            // 整个调用完全不推进单值状态。
            var rounds = 2 * (((length + (2 * laneCount)) - 1) / (2 * laneCount));
            var advanced = new RandomSource(seed);
            for (var round = 0; round < rounds; round++)
            {
                _ = advanced.NextDoubleVector();
            }

            var expectedNext = advanced.NextDoubleVector();
            var actualNext = source.NextDoubleVector();
            for (var lane = 0; lane < laneCount; lane++)
            {
                Xunit.Assert.Equal(expectedNext[lane], actualNext[lane]);
            }

            Xunit.Assert.Equal(untouched.NextULong(), source.NextULong());
        }
    }

    /// <summary>
    /// 验证相同标量与批量调用序列可重复且结果有限。
    /// </summary>
    [Xunit.Fact]
    public void ExactCallSequencesRepeatWithoutCrossCallSpareState()
    {
        var first = new RandomSource(456);
        var second = new RandomSource(456);

        for (var index = 0; index < 100; index++)
        {
            var firstBatch = new double[7];
            var secondBatch = new double[7];
            StandardNormal.Fill(first, firstBatch);
            StandardNormal.Fill(second, secondBatch);
            Xunit.Assert.Equal(firstBatch, secondBatch);
            Xunit.Assert.All(firstBatch, static value => Xunit.Assert.True(double.IsFinite(value)));
            Xunit.Assert.Equal(StandardNormal.Sample(first), StandardNormal.Sample(second));
        }
    }

    /// <summary>
    /// 验证预热后的标量与批量采样不在调用线程分配内存。
    /// </summary>
    [Xunit.Fact]
    public void WarmedSamplingDoesNotAllocate()
    {
        var source = new RandomSource(789);
        var destination = new double[32];
        for (var index = 0; index < 10; index++)
        {
            _ = StandardNormal.Sample(source);
            StandardNormal.Fill(source, destination);
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 100; index++)
        {
            _ = StandardNormal.Sample(source);
            StandardNormal.Fill(source, destination);
        }

        Xunit.Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }

    /// <summary>
    /// 验证一百万个固定种子样本满足已批准的统计阈值。
    /// </summary>
    [Xunit.Fact]
    public void FixedSeedMillionSampleStatisticsMeetTheApprovedThresholds()
    {
        const int sampleCount = 1_000_000;
        var samples = new double[sampleCount];
        StandardNormal.Fill(new RandomSource(0x0123456789ABCDEF), samples);

        var sum = 0.0;
        var sumSquares = 0.0;
        var tailCount = 0;
        foreach (var sample in samples)
        {
            Xunit.Assert.True(double.IsFinite(sample));
            sum += sample;
            sumSquares += sample * sample;
            if (Math.Abs(sample) > 3)
            {
                tailCount++;
            }
        }

        Array.Sort(samples);
        var mean = sum / sampleCount;
        var variance = (sumSquares / sampleCount) - (mean * mean);
        Xunit.Assert.InRange(mean, -0.005, 0.005);
        Xunit.Assert.InRange(variance, 0.99, 1.01);
        Xunit.Assert.InRange(samples[sampleCount / 2], -0.01, 0.01);
        Xunit.Assert.InRange(samples[sampleCount / 100], -2.40, -2.25);
        Xunit.Assert.InRange(samples[(sampleCount * 99) / 100], 2.25, 2.40);
        Xunit.Assert.InRange(tailCount, 2400, 3000);
    }
}
