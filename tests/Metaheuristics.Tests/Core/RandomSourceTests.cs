using System.Numerics;
using System.Reflection;
using Anastasya.Metaheuristics.Core.Randomness;

namespace Anastasya.Metaheuristics.Tests.Core;

/// <summary>
/// 验证封闭随机源的标量、批量契约和状态安全。
/// </summary>
public sealed class RandomSourceTests
{
    /// <summary>
    /// 对照独立参考输出验证 SplitMix64 播种的 xoshiro256++ 序列。
    /// </summary>
    [Xunit.Fact]
    public void KnownSeedsMatchXoshiroReferenceOutputs()
    {
        var expected = new Dictionary<ulong, ulong[]>
        {
            [0] = [
                0x53175D61490B23DF,
                0x61DA6F3DC380D507,
                0x5C0FDF91EC9A7BFC,
                0x02EEBF8C3BBE5E1A,
                0x7ECA04EBAF4A5EEA,
            ],
            [1] = [
                0xCFC5D07F6F03C29B,
                0xBF424132963FE08D,
                0x19A37D5757AAF520,
                0xBF08119F05CD56D6,
                0x2F47184B86186FA4,
            ],
            [ulong.MaxValue] = [
                0x56CCF8CE948E27B2,
                0xE68588432E5A5B90,
                0xE3E9B5A48119CA8B,
                0x460F19495532AE73,
                0xA7D62040EA9263E1,
            ],
        };

        foreach (var (seed, outputs) in expected)
        {
            var source = new RandomSource(seed);
            foreach (var value in outputs)
            {
                Xunit.Assert.Equal(value, source.NextULong());
            }
        }
    }

    /// <summary>
    /// 对照独立参考状态机验证连续一万次输出。
    /// </summary>
    [Xunit.Fact]
    public void LongSequenceMatchesIndependentReference()
    {
        const ulong seed = 0x0123456789ABCDEF;
        var source = new RandomSource(seed);
        var state = CreateReferenceState(seed);

        for (var index = 0; index < 10_000; index++)
        {
            Xunit.Assert.Equal(ReferenceNext(ref state), source.NextULong());
        }
    }

    /// <summary>
    /// 对照独立乘高位拒绝映射验证有界整数结果。
    /// </summary>
    [Xunit.Fact]
    public void BoundedIntegersMatchIndependentMultiplyHighReference()
    {
        const ulong seed = 0xA5A5A5A5A5A5A5A5;
        var source = new RandomSource(seed);
        var state = CreateReferenceState(seed);

        for (var index = 0; index < 10_000; index++)
        {
            Xunit.Assert.Equal(
                ReferenceNextInt(ref state, -123456789, 1987654321),
                source.NextInt(-123456789, 1987654321));
        }
    }

    /// <summary>
    /// 验证完全相同的标量调用序列具有确定性。
    /// </summary>
    [Xunit.Fact]
    public void ExactCallSequenceRepeatsDeterministically()
    {
        var first = new RandomSource(0x123456789ABCDEF0);
        var second = new RandomSource(0x123456789ABCDEF0);

        for (var index = 0; index < 1000; index++)
        {
            Xunit.Assert.Equal(first.NextULong(), second.NextULong());
            Xunit.Assert.Equal(first.NextDouble(), second.NextDouble());
            Xunit.Assert.Equal(first.NextDouble(-1.5, 7.25), second.NextDouble(-1.5, 7.25));
            Xunit.Assert.Equal(first.NextInt(int.MinValue, int.MaxValue), second.NextInt(int.MinValue, int.MaxValue));
        }
    }

    /// <summary>
    /// 验证公共随机源密封且不暴露公共构造或后端抽象。
    /// </summary>
    [Xunit.Fact]
    public void PublicShapeIsClosed()
    {
        var type = typeof(RandomSource);
        Xunit.Assert.True(type.IsSealed);
        Xunit.Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Xunit.Assert.DoesNotContain(type.GetInterfaces(), static interfaceType => interfaceType.Name == "IRandomSource");
        Xunit.Assert.Equal(
            ["Fill", "Fill", "Fill", "NextDouble", "NextDouble", "NextDoubleVector", "NextInt", "NextULong", "NextULongVector"],
            type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(static method => !method.IsSpecialName)
                .Select(static method => method.Name)
                .OrderBy(static name => name)
                .ToArray());
    }

    /// <summary>
    /// 验证空目标不消费状态，无效范围不改变状态或目标。
    /// </summary>
    [Xunit.Fact]
    public void EmptyAndInvalidOperationsPreserveStateAndTargets()
    {
        var source = new RandomSource(987654321);
        var scalarExpected = new RandomSource(987654321);
        var batchExpected = new RandomSource(987654321);
        source.Fill(Span<ulong>.Empty);
        source.Fill(Span<double>.Empty);
        source.Fill(Span<double>.Empty, -1.0, 1.0);

        // 空目标既不写目标，也不消费两套状态中的任何一套。
        Xunit.Assert.True(batchExpected.NextULongVector() == source.NextULongVector());
        Xunit.Assert.Equal(scalarExpected.NextULong(), source.NextULong());

        var target = new[] { 3.0, 4.0 };
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(() => source.Fill(target, 0, double.PositiveInfinity));
        Xunit.Assert.Equal([3.0, 4.0], target);

        Xunit.Assert.Throws<ArgumentException>(() => source.Fill(target, 1.0, 1.0));
        Xunit.Assert.Equal([3.0, 4.0], target);

        // 失败之后两套状态都没有推进。
        Xunit.Assert.Equal(scalarExpected.NextULong(), source.NextULong());
        Xunit.Assert.True(batchExpected.NextULongVector() == source.NextULongVector());
    }

    /// <summary>
    /// 验证浮点标量和批量采样遵循半开区间契约，包括宽度溢出的范围。
    /// </summary>
    [Xunit.Fact]
    public void DoubleSamplesStayWithinHalfOpenRanges()
    {
        var source = new RandomSource(42);
        for (var index = 0; index < 20_000; index++)
        {
            var unit = source.NextDouble();
            Xunit.Assert.InRange(unit, 0.0, Math.BitDecrement(1.0));

            var bounded = source.NextDouble(-double.MaxValue, double.MaxValue);
            Xunit.Assert.True(bounded >= -double.MaxValue && bounded < double.MaxValue);
        }

        var adjacent = new double[128];
        source.Fill(adjacent, 1.0, Math.BitIncrement(1.0));
        Xunit.Assert.All(adjacent, static value => Xunit.Assert.Equal(1.0, value));
    }

    /// <summary>
    /// 验证整数端点和无偏映射的半开区间结果。
    /// </summary>
    [Xunit.Fact]
    public void IntegerSamplesSupportTheFullIntRange()
    {
        var powerOfTwoSource = new RandomSource(456);
        var powerOfTwoState = CreateReferenceState(456);
        for (var index = 0; index < 10_000; index++)
        {
            Xunit.Assert.Equal(
                ReferenceNextInt(ref powerOfTwoState, -19, 45),
                powerOfTwoSource.NextInt(-19, 45));
        }

        var singletonSource = new RandomSource(789);
        var singletonState = CreateReferenceState(789);
        for (var index = 0; index < 256; index++)
        {
            Xunit.Assert.Equal(
                ReferenceNextInt(ref singletonState, 17, 18),
                singletonSource.NextInt(17, 18));
        }
    }

    /// <summary>
    /// 验证规定的参数校验和异常类别。
    /// </summary>
    [Xunit.Fact]
    public void RangesRejectInvalidEndpoints()
    {
        var source = new RandomSource(1);
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(() => source.NextDouble(double.NaN, 1));
        Xunit.Assert.Throws<ArgumentOutOfRangeException>(() => source.NextDouble(0, double.PositiveInfinity));
        Xunit.Assert.Throws<ArgumentException>(() => source.NextDouble(1, 1));
        Xunit.Assert.Throws<ArgumentException>(() => source.NextDouble(2, 1));
        Xunit.Assert.Throws<ArgumentException>(() => source.NextInt(1, 1));
        Xunit.Assert.Throws<ArgumentException>(() => source.NextInt(2, 1));
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

    private static int ReferenceNextInt(ref ulong[] state, int minimum, int maximum)
    {
        var range = (ulong)((long)maximum - minimum);
        var threshold = unchecked((0UL - range) % range);
        while (true)
        {
            var product = (UInt128)ReferenceNext(ref state) * range;
            var low = (ulong)product;
            if (low >= threshold)
            {
                return (int)((long)minimum + (long)(product >> 64));
            }
        }
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
