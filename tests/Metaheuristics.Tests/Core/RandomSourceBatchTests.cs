using System.Numerics;
using Anastasya.Metaheuristics.Core.Randomness;

namespace Anastasya.Metaheuristics.Tests.Core;

/// <summary>
/// 验证批量状态的播种、排放顺序、映射、隔离与分配；参考实现独立于生产代码。
/// </summary>
public sealed class RandomSourceBatchTests
{
    private static readonly ulong[] Seeds = [0, 1, 20260908, 0x0123456789ABCDEF, ulong.MaxValue];

    private static int LaneCount => Vector<ulong>.Count;

    /// <summary>
    /// 验证批量 lane 由 Jump^(i+1)(S) 播种，且向量入口不推进单值状态。
    /// </summary>
    [Xunit.Fact]
    public void VectorSamplesMatchIndependentJumpSeededLanes()
    {
        foreach (var seed in Seeds)
        {
            var source = new RandomSource(seed);
            var lanes = CreateReferenceLanes(seed);
            var scalarState = CreateReferenceState(seed);

            for (var round = 0; round < 512; round++)
            {
                AssertLanesEqual(ReferenceNextRound(lanes), source.NextULongVector());
            }

            Xunit.Assert.Equal(ReferenceNext(ref scalarState), source.NextULong());
            Xunit.Assert.All(lanes, static lane => Xunit.Assert.NotEqual(0UL, lane[0] | lane[1] | lane[2] | lane[3]));
            Xunit.Assert.Equal(lanes.Length, lanes.Select(static lane => string.Join(',', lane)).Distinct().Count());
        }
    }

    /// <summary>
    /// 验证单位样本向量就是同一轮原始样本的 53 位映射。
    /// </summary>
    [Xunit.Fact]
    public void UnitVectorMatchesRawVectorMapping()
    {
        var rawSource = new RandomSource(0x0F0F0F0F0F0F0F0F);
        var unitSource = new RandomSource(0x0F0F0F0F0F0F0F0F);
        for (var round = 0; round < 256; round++)
        {
            var raw = rawSource.NextULongVector();
            var unit = unitSource.NextDoubleVector();
            for (var lane = 0; lane < LaneCount; lane++)
            {
                Xunit.Assert.Equal((raw[lane] >> 11) * (1.0 / 9007199254740992.0), unit[lane]);
            }
        }
    }

    /// <summary>
    /// 验证原始批量填充的长度矩阵、lane 顺序与后续批量状态。
    /// </summary>
    [Xunit.Fact]
    public void RawFillMatchesReferenceEmissionForAllShapeBoundaries()
    {
        var lengths = new List<int>
        {
            0, 1, 2, 3, 31, 32, 33, 127, 128, 129, 1024,
            LaneCount - 1, LaneCount, LaneCount + 1, (2 * LaneCount) - 1, 2 * LaneCount, (2 * LaneCount) + 1,
        };
        foreach (var length in lengths.Distinct().Where(static value => value >= 0))
        {
            var source = new RandomSource(0x0123456789ABCDEF);
            var lanes = CreateReferenceLanes(0x0123456789ABCDEF);
            var scalarState = CreateReferenceState(0x0123456789ABCDEF);
            var values = new ulong[length];

            source.Fill(values);

            Xunit.Assert.Equal(ReferenceFill(lanes, length), values);
            AssertLanesEqual(ReferenceNextRound(lanes), source.NextULongVector());
            Xunit.Assert.Equal(ReferenceNext(ref scalarState), source.NextULong());
        }
    }

    /// <summary>
    /// 验证单位批量填充与参考映射逐位一致，并保持后续批量状态。
    /// </summary>
    [Xunit.Fact]
    public void UnitFillMatchesReferenceMapping()
    {
        foreach (var length in new[] { 1, LaneCount, LaneCount + 1, 127, 129 })
        {
            var source = new RandomSource(20260908);
            var lanes = CreateReferenceLanes(20260908);
            var values = new double[length];

            source.Fill(values);

            var raw = ReferenceFill(lanes, length);
            for (var index = 0; index < length; index++)
            {
                Xunit.Assert.Equal((raw[index] >> 11) * (1.0 / 9007199254740992.0), values[index]);
            }

            AssertLanesEqual(ReferenceNextRound(lanes), source.NextULongVector());
        }
    }

    /// <summary>
    /// 验证有界 double 批量填充与标量映射公式逐位一致。
    /// </summary>
    [Xunit.Fact]
    public void BoundedDoubleFillMatchesScalarMapping()
    {
        var ranges = new (double Minimum, double Maximum)[]
        {
            (-5, 5),
            (0, 1),
            (1, Math.BitIncrement(1.0)),
            (-double.MaxValue, double.MaxValue),
        };
        foreach (var (minimum, maximum) in ranges)
        {
            foreach (var length in new[] { 1, LaneCount, LaneCount + 3, 128 })
            {
                var source = new RandomSource(0x5EED_0001);
                var lanes = CreateReferenceLanes(0x5EED_0001);
                var values = new double[length];

                source.Fill(values, minimum, maximum);

                var raw = ReferenceFill(lanes, length);
                for (var index = 0; index < length; index++)
                {
                    var unit = (raw[index] >> 11) * (1.0 / 9007199254740992.0);
                    Xunit.Assert.Equal(ReferenceScale(unit, minimum, maximum), values[index]);
                }

                AssertLanesEqual(ReferenceNextRound(lanes), source.NextULongVector());
            }
        }
    }

    /// <summary>
    /// 验证单值入口与批量入口互不推进，且各自与独立参考一致。
    /// </summary>
    [Xunit.Fact]
    public void ScalarAndBatchRoutesStayIndependent()
    {
        const ulong seed = 0xABCDEF0123456789;
        var source = new RandomSource(seed);
        var scalarState = CreateReferenceState(seed);
        var lanes = CreateReferenceLanes(seed);
        var rawBuffer = new ulong[LaneCount + 3];
        var unitBuffer = new double[LaneCount + 3];

        for (var iteration = 0; iteration < 64; iteration++)
        {
            Xunit.Assert.Equal(ReferenceNext(ref scalarState), source.NextULong());
            AssertLanesEqual(ReferenceNextRound(lanes), source.NextULongVector());
            Xunit.Assert.Equal(
                ReferenceNextBoundedScalar(ref scalarState, -7, 11),
                source.NextInt(-7, 11));
            source.Fill(rawBuffer);
            Xunit.Assert.Equal(ReferenceFill(lanes, rawBuffer.Length), rawBuffer);
            AssertLanesEqual(ReferenceNextRound(lanes), source.NextULongVector());
            source.Fill(unitBuffer);
            var unitReference = ReferenceFill(lanes, unitBuffer.Length);
            for (var index = 0; index < unitBuffer.Length; index++)
            {
                Xunit.Assert.Equal((unitReference[index] >> 11) * (1.0 / 9007199254740992.0), unitBuffer[index]);
            }
        }
    }

    /// <summary>
    /// 验证批量流在固定 seed 下的位、字节与相邻相关性统计不超出 7 sigma 预算。
    /// </summary>
    /// <remarks>
    /// 统计只用于排查明显问题：每 lane 采样 2^20 轮，阈值取 7 倍标准差，失败不重试、不换种子。
    /// </remarks>
    [Xunit.Fact]
    public void BatchStreamsPassFixedSeedStatisticalChecks()
    {
        const int rounds = 1 << 20;
        var laneCount = LaneCount;
        var source = new RandomSource(20260908);
        var bitCounts = new long[laneCount * 64];
        var byteCounts = new long[laneCount * 8 * 256];
        var previous = new double[laneCount];
        var sumX = new double[laneCount];
        var sumXx = new double[laneCount];
        var sumXy = new double[laneCount];

        for (var round = 0; round < rounds; round++)
        {
            var raw = source.NextULongVector();
            for (var lane = 0; lane < laneCount; lane++)
            {
                var value = raw[lane];
                var bitBase = lane * 64;
                for (var bit = 0; bit < 64; bit++)
                {
                    bitCounts[bitBase + bit] += (long)(value >> bit) & 1L;
                }

                var byteBase = lane * 8 * 256;
                for (var position = 0; position < 8; position++)
                {
                    byteCounts[byteBase + (position * 256) + (int)((value >> (position * 8)) & 0xFF)]++;
                }

                var unit = (value >> 11) * (1.0 / 9007199254740992.0);
                Xunit.Assert.InRange(unit, 0.0, Math.BitDecrement(1.0));
                if (round != 0)
                {
                    sumX[lane] += unit;
                    sumXx[lane] += unit * unit;
                    sumXy[lane] += unit * previous[lane];
                }

                previous[lane] = unit;
            }
        }

        var bitDeviation = 7 * Math.Sqrt(rounds * 0.25);
        for (var index = 0; index < bitCounts.Length; index++)
        {
            Xunit.Assert.InRange(bitCounts[index], (rounds * 0.5) - bitDeviation, (rounds * 0.5) + bitDeviation);
        }

        var byteDeviation = 7 * Math.Sqrt(rounds * (1.0 / 256) * (255.0 / 256));
        for (var index = 0; index < byteCounts.Length; index++)
        {
            Xunit.Assert.InRange(byteCounts[index], (rounds / 256.0) - byteDeviation, (rounds / 256.0) + byteDeviation);
        }

        var correlationLimit = 7 / Math.Sqrt((rounds - 1) * laneCount);
        for (var lane = 0; lane < laneCount; lane++)
        {
            const int count = rounds - 1;
            var meanX = sumX[lane] / count;
            var covariance = (sumXy[lane] / count) - (meanX * meanX);
            var variance = (sumXx[lane] / count) - (meanX * meanX);
            Xunit.Assert.True(variance > 0);
            Xunit.Assert.InRange(covariance / variance, -correlationLimit, correlationLimit);
        }
    }

    /// <summary>
    /// 验证预热后的批量入口在调用线程不分配托管内存。
    /// </summary>
    [Xunit.Fact]
    public void BatchEntriesDoNotAllocateAfterWarmup()
    {
        var source = new RandomSource(0x1234_5678);
        var rawBuffer = new ulong[33];
        var unitBuffer = new double[33];
        for (var iteration = 0; iteration < 16; iteration++)
        {
            _ = source.NextULongVector();
            _ = source.NextDoubleVector();
            source.Fill(rawBuffer);
            source.Fill(unitBuffer);
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var iteration = 0; iteration < 256; iteration++)
        {
            _ = source.NextULongVector();
            _ = source.NextDoubleVector();
            source.Fill(rawBuffer);
            source.Fill(unitBuffer);
        }

        Xunit.Assert.Equal(before, GC.GetAllocatedBytesForCurrentThread());
    }

    private static void AssertLanesEqual(ulong[] expected, Vector<ulong> actual)
    {
        Xunit.Assert.Equal(LaneCount, expected.Length);
        for (var lane = 0; lane < LaneCount; lane++)
        {
            Xunit.Assert.Equal(expected[lane], actual[lane]);
        }
    }

    private static ulong[] CreateReferenceState(ulong seed)
    {
        var state = seed;
        return [
            ReferenceSplitMix(ref state),
            ReferenceSplitMix(ref state),
            ReferenceSplitMix(ref state),
            ReferenceSplitMix(ref state),
        ];
    }

    private static ulong[][] CreateReferenceLanes(ulong seed)
    {
        var words = CreateReferenceState(seed);
        if ((words[0] | words[1] | words[2] | words[3]) == 0)
        {
            words[3] = 1;
        }

        var lanes = new ulong[LaneCount][];
        for (var lane = 0; lane < LaneCount; lane++)
        {
            ReferenceJump(ref words[0], ref words[1], ref words[2], ref words[3]);
            lanes[lane] = [words[0], words[1], words[2], words[3]];
        }

        return lanes;
    }

    private static ulong[] ReferenceNextRound(ulong[][] lanes)
    {
        var outputs = new ulong[lanes.Length];
        for (var lane = 0; lane < lanes.Length; lane++)
        {
            outputs[lane] = ReferenceNext(ref lanes[lane]);
        }

        return outputs;
    }

    private static ulong[] ReferenceFill(ulong[][] lanes, int length)
    {
        var values = new List<ulong>(length);
        while (values.Count < length)
        {
            foreach (var value in ReferenceNextRound(lanes))
            {
                if (values.Count == length)
                {
                    break;
                }

                values.Add(value);
            }
        }

        return [.. values];
    }

    private static int ReferenceNextBoundedScalar(ref ulong[] state, int minimum, int maximum)
    {
        var range = (ulong)((long)maximum - minimum);
        var threshold = unchecked((0UL - range) % range);
        while (true)
        {
            var product = (UInt128)ReferenceNext(ref state) * range;
            if ((ulong)product >= threshold)
            {
                return (int)((long)minimum + (long)(product >> 64));
            }
        }
    }

    private static double ReferenceScale(double unit, double minimum, double maximum)
    {
        var width = maximum - minimum;
        var value = double.IsFinite(width)
            ? minimum + (width * unit)
            : (minimum * (1 - unit)) + (maximum * unit);
        if (value < minimum)
        {
            return minimum;
        }

        return value >= maximum ? Math.BitDecrement(maximum) : value;
    }

    private static ulong ReferenceNext(ref ulong[] state)
    {
        var result = RotateLeft(state[0] + state[3], 23) + state[0];
        var temporary = state[1] << 17;
        state[2] ^= state[0];
        state[3] ^= state[1];
        state[1] ^= state[2];
        state[0] ^= state[3];
        state[2] ^= temporary;
        state[3] = RotateLeft(state[3], 45);
        return result;
    }

    private static void ReferenceJump(ref ulong state0, ref ulong state1, ref ulong state2, ref ulong state3)
    {
        var jumped0 = 0UL;
        var jumped1 = 0UL;
        var jumped2 = 0UL;
        var jumped3 = 0UL;
        ReferenceApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumped0, ref jumped1, ref jumped2, ref jumped3, 0x180EC6D33CFD0ABAUL);
        ReferenceApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumped0, ref jumped1, ref jumped2, ref jumped3, 0xD5A61266F0C9392CUL);
        ReferenceApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumped0, ref jumped1, ref jumped2, ref jumped3, 0xA9582618E03FC9AAUL);
        ReferenceApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumped0, ref jumped1, ref jumped2, ref jumped3, 0x39ABDC4529B1661CUL);

        state0 = jumped0;
        state1 = jumped1;
        state2 = jumped2;
        state3 = jumped3;
    }

    private static void ReferenceApplyJump(
        ref ulong state0,
        ref ulong state1,
        ref ulong state2,
        ref ulong state3,
        ref ulong jumped0,
        ref ulong jumped1,
        ref ulong jumped2,
        ref ulong jumped3,
        ulong polynomial)
    {
        var words = new[] { state0, state1, state2, state3 };
        for (var bit = 0; bit < 64; bit++)
        {
            if ((polynomial & (1UL << bit)) != 0)
            {
                jumped0 ^= words[0];
                jumped1 ^= words[1];
                jumped2 ^= words[2];
                jumped3 ^= words[3];
            }

            ReferenceNext(ref words);
        }

        state0 = words[0];
        state1 = words[1];
        state2 = words[2];
        state3 = words[3];
    }

    private static ulong ReferenceSplitMix(ref ulong state)
    {
        state = unchecked(state + 0x9E3779B97F4A7C15UL);
        var value = state;
        value = unchecked((value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL);
        value = unchecked((value ^ (value >> 27)) * 0x94D049BB133111EBUL);
        return value ^ (value >> 31);
    }

    private static ulong RotateLeft(ulong value, int offset)
    {
        return (value << offset) | (value >> (64 - offset));
    }
}
