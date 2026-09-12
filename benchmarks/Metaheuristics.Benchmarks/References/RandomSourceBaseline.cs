// 基准基线 A：SPEC-0009 的单状态实现（提交 56c1f57）的逐字副本，仅供基准对照。

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Anastasya.Metaheuristics.Benchmarks.References;

/// <summary>
/// 提供由 Core 执行及其使用方在单次运行中拥有的均匀随机源。
/// </summary>
/// <remarks>
/// <see cref="RandomSourceBaseline"/> 由 Core 为单次优化运行创建，且不保证线程安全。
/// 其状态由一个显式 64 位种子初始化的内部 <c>xoshiro256++</c> 生成器维护。
/// 调用方不得在不同运行、Group 或并发操作之间共享实例。
/// </remarks>
public sealed class RandomSourceBaseline
{
    private const ulong SplitMixIncrement = 0x9E3779B97F4A7C15UL;
    private const ulong SplitMixMultiplier1 = 0xBF58476D1CE4E5B9UL;
    private const ulong SplitMixMultiplier2 = 0x94D049BB133111EBUL;
    private const double InverseTwoToThePower53 = 1.0 / 9007199254740992.0;

    private ulong _state0;
    private ulong _state1;
    private ulong _state2;
    private ulong _state3;

    /// <summary>
    /// 从显式 64 位种子初始化由运行拥有的随机源。
    /// </summary>
    /// <param name="seed">Core 执行为当前运行选择的种子。</param>
    internal RandomSourceBaseline(ulong seed)
    {
        var state = seed;
        _state0 = NextSplitMix64(ref state);
        _state1 = NextSplitMix64(ref state);
        _state2 = NextSplitMix64(ref state);
        _state3 = NextSplitMix64(ref state);

        if ((_state0 | _state1 | _state2 | _state3) == 0)
        {
            _state3 = 1;
        }
    }

    /// <summary>
    /// 返回完整 <see cref="ulong"/> 范围内的下一个均匀 64 位值。
    /// </summary>
    /// <returns>下一个原始生成器输出。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong NextULong()
    {
        return NextRawFromFields();
    }

    /// <summary>
    /// 将完整范围内均匀分布的 64 位值写入调用方拥有的 span。
    /// </summary>
    /// <param name="destination">要写入的 span；空 span 不执行操作且不消费状态。</param>
    public void Fill(Span<ulong> destination)
    {
        if (destination.IsEmpty)
        {
            return;
        }

        var state0 = _state0;
        var state1 = _state1;
        var state2 = _state2;
        var state3 = _state3;
        for (var index = 0; index < destination.Length; index++)
        {
            destination[index] = NextRaw(ref state0, ref state1, ref state2, ref state3);
        }

        _state0 = state0;
        _state1 = state1;
        _state2 = state2;
        _state3 = state3;
    }

    /// <summary>
    /// 返回半开区间 <c>[0, 1)</c> 内均匀分布的 double。
    /// </summary>
    /// <returns>大于等于零且小于一的值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double NextDouble()
    {
        return (NextRawFromFields() >> 11) * InverseTwoToThePower53;
    }

    /// <summary>
    /// 返回半开区间 <c>[minimum, maximum)</c> 内均匀分布的 double。
    /// </summary>
    /// <param name="minimum">有限的包含下端点。</param>
    /// <param name="maximum">有限的排除上端点。</param>
    /// <returns>请求半开区间内的值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">任一端点不是有限值。</exception>
    /// <exception cref="ArgumentException"><paramref name="minimum"/> 不小于 <paramref name="maximum"/>。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double NextDouble(double minimum, double maximum)
    {
        ValidateDoubleRange(minimum, maximum);
        return ScaleUnitInterval(NextDouble(), minimum, maximum);
    }

    /// <summary>
    /// 将半开区间 <c>[0, 1)</c> 内的 double 写入调用方拥有的 span。
    /// </summary>
    /// <param name="destination">要写入的 span；空 span 不消费状态。</param>
    public void Fill(Span<double> destination)
    {
        if (destination.IsEmpty)
        {
            return;
        }

        var state0 = _state0;
        var state1 = _state1;
        var state2 = _state2;
        var state3 = _state3;
        for (var index = 0; index < destination.Length; index++)
        {
            destination[index] = (NextRaw(ref state0, ref state1, ref state2, ref state3) >> 11)
                * InverseTwoToThePower53;
        }

        _state0 = state0;
        _state1 = state1;
        _state2 = state2;
        _state3 = state3;
    }

    /// <summary>
    /// 将半开区间 <c>[minimum, maximum)</c> 内的 double 写入调用方拥有的 span。
    /// </summary>
    /// <param name="destination">要写入的 span；无效端点会在状态或目标发生变化前被拒绝。</param>
    /// <param name="minimum">有限的包含下端点。</param>
    /// <param name="maximum">有限的排除上端点。</param>
    /// <exception cref="ArgumentOutOfRangeException">任一端点不是有限值。</exception>
    /// <exception cref="ArgumentException"><paramref name="minimum"/> 不小于 <paramref name="maximum"/>。</exception>
    public void Fill(Span<double> destination, double minimum, double maximum)
    {
        ValidateDoubleRange(minimum, maximum);
        if (destination.IsEmpty)
        {
            return;
        }

        var state0 = _state0;
        var state1 = _state1;
        var state2 = _state2;
        var state3 = _state3;
        for (var index = 0; index < destination.Length; index++)
        {
            var unit = (NextRaw(ref state0, ref state1, ref state2, ref state3) >> 11)
                * InverseTwoToThePower53;
            destination[index] = ScaleUnitInterval(unit, minimum, maximum);
        }

        _state0 = state0;
        _state1 = state1;
        _state2 = state2;
        _state3 = state3;
    }

    /// <summary>
    /// 返回半开区间 <c>[minimum, maximum)</c> 内无偏的整数。
    /// </summary>
    /// <param name="minimum">包含下端点。</param>
    /// <param name="maximum">排除上端点。</param>
    /// <returns>请求半开区间内的整数。</returns>
    /// <exception cref="ArgumentException"><paramref name="minimum"/> 不小于 <paramref name="maximum"/>。</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int NextInt(int minimum, int maximum)
    {
        ValidateIntegerRange(minimum, maximum);
        var range = (ulong)((long)maximum - minimum);
        var offset = NextBoundedFromFields(range);
        return (int)((long)minimum + (long)offset);
    }

    /// <summary>
    /// 将半开区间 <c>[minimum, maximum)</c> 内无偏的整数写入调用方拥有的 span。
    /// </summary>
    /// <param name="destination">要写入的 span；无效端点会在状态或目标发生变化前被拒绝。</param>
    /// <param name="minimum">包含下端点。</param>
    /// <param name="maximum">排除上端点。</param>
    /// <exception cref="ArgumentException"><paramref name="minimum"/> 不小于 <paramref name="maximum"/>。</exception>
    public void Fill(Span<int> destination, int minimum, int maximum)
    {
        ValidateIntegerRange(minimum, maximum);
        if (destination.IsEmpty)
        {
            return;
        }

        var range = (ulong)((long)maximum - minimum);
        var state0 = _state0;
        var state1 = _state1;
        var state2 = _state2;
        var state3 = _state3;
        for (var index = 0; index < destination.Length; index++)
        {
            var offset = NextBounded(range, ref state0, ref state1, ref state2, ref state3);
            destination[index] = (int)((long)minimum + (long)offset);
        }

        _state0 = state0;
        _state1 = state1;
        _state2 = state2;
        _state3 = state3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateDoubleRange(double minimum, double maximum)
    {
        if (!double.IsFinite(minimum))
        {
            throw new ArgumentOutOfRangeException(nameof(minimum), "The minimum endpoint must be finite.");
        }

        if (!double.IsFinite(maximum))
        {
            throw new ArgumentOutOfRangeException(nameof(maximum), "The maximum endpoint must be finite.");
        }

        if (minimum >= maximum)
        {
            throw new ArgumentException("The minimum endpoint must be less than the maximum endpoint.", nameof(maximum));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ValidateIntegerRange(int minimum, int maximum)
    {
        if (minimum >= maximum)
        {
            throw new ArgumentException("The minimum endpoint must be less than the maximum endpoint.", nameof(maximum));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double ScaleUnitInterval(double unit, double minimum, double maximum)
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

    // 标量路径先局部化四个状态字，再与 Fill 共用同一个状态转换和有界映射，避免重复算法体。
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextBoundedFromFields(ulong range)
    {
        var state0 = _state0;
        var state1 = _state1;
        var state2 = _state2;
        var state3 = _state3;
        var result = NextBounded(range, ref state0, ref state1, ref state2, ref state3);
        _state0 = state0;
        _state1 = state1;
        _state2 = state2;
        _state3 = state3;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextRawFromFields()
    {
        var state0 = _state0;
        var state1 = _state1;
        var state2 = _state2;
        var state3 = _state3;
        var result = NextRaw(ref state0, ref state1, ref state2, ref state3);
        _state0 = state0;
        _state1 = state1;
        _state2 = state2;
        _state3 = state3;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong NextBounded(
        ulong range,
        ref ulong state0,
        ref ulong state1,
        ref ulong state2,
        ref ulong state3)
    {
        // 2 的幂宽度拒绝阈值为零。乘高位等于原始字按对应位数右移，
        // 因而能保留 64 位映射，同时避免常见种群索引路径的软件宽乘法。
        var raw = NextRaw(ref state0, ref state1, ref state2, ref state3);
        if (range == 1)
        {
            return 0;
        }

        if ((range & (range - 1)) == 0)
        {
            return raw >> (64 - BitOperations.TrailingZeroCount(range));
        }

        var threshold = unchecked((0UL - range) % range);
        while (true)
        {
            var productHigh = Math.BigMul(
                raw,
                range,
                out var productLow);
            if (productLow >= threshold)
            {
                return productHigh;
            }

            raw = NextRaw(ref state0, ref state1, ref state2, ref state3);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong NextRaw(
        ref ulong state0,
        ref ulong state1,
        ref ulong state2,
        ref ulong state3)
    {
        var result = BitOperations.RotateLeft(state0 + state3, 23) + state0;
        var temporary = state1 << 17;

        state2 ^= state0;
        state3 ^= state1;
        state1 ^= state2;
        state0 ^= state3;
        state2 ^= temporary;
        state3 = BitOperations.RotateLeft(state3, 45);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong NextSplitMix64(ref ulong state)
    {
        state = unchecked(state + SplitMixIncrement);
        var value = state;
        value = unchecked((value ^ (value >> 30)) * SplitMixMultiplier1);
        value = unchecked((value ^ (value >> 27)) * SplitMixMultiplier2);
        return value ^ (value >> 31);
    }
}
