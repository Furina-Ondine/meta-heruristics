using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Anastasya.Metaheuristics.Core.Randomness;

/// <summary>
/// 提供由 Core 执行及其使用方在单次运行中拥有的均匀随机源。
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RandomSource"/> 由 Core 为单次优化运行创建，且不保证线程安全。
/// 实例同时维护两套互不推进的 <c>xoshiro256++</c> 状态：四个单值 <see cref="ulong"/> 状态字，
/// 以及四个运行时宽度（<see cref="Vector{T}.Count"/> 个 lane）的批量状态向量。
/// 两套状态都由同一个显式 64 位种子派生。
/// </para>
/// <para>
/// 单值入口（<see cref="NextULong"/>、<see cref="NextDouble()"/>、<see cref="NextDouble(double,double)"/>
/// 与 <see cref="NextInt"/>）只推进单值状态；<see cref="NextULongVector"/>、
/// <see cref="NextDoubleVector"/>、全部 <c>Fill</c> 重载与 <see cref="StandardNormal.Fill"/>
/// 只推进批量状态。
/// </para>
/// <para>
/// 批量调用按 lane 0 至 lane L-1 的顺序排放样本，每次调用末尾未写满一轮的 lane 会被丢弃，
/// 不在调用之间缓存。调用方不得在不同运行、Group 或并发操作之间共享实例。
/// </para>
/// </remarks>
public sealed class RandomSource
{
    private const ulong SplitMixIncrement = 0x9E3779B97F4A7C15UL;
    private const ulong SplitMixMultiplier1 = 0xBF58476D1CE4E5B9UL;
    private const ulong SplitMixMultiplier2 = 0x94D049BB133111EBUL;
    private const double InverseTwoToThePower53 = 1.0 / 9007199254740992.0;
    private const int WordBits = 64;
    private const int DoubleMantissaShift = 11;

    private const ulong JumpPolynomial0 = 0x180EC6D33CFD0ABAUL;
    private const ulong JumpPolynomial1 = 0xD5A61266F0C9392CUL;
    private const ulong JumpPolynomial2 = 0xA9582618E03FC9AAUL;
    private const ulong JumpPolynomial3 = 0x39ABDC4529B1661CUL;

    private ulong _state0;
    private ulong _state1;
    private ulong _state2;
    private ulong _state3;

    private Vector<ulong> _batchState0;
    private Vector<ulong> _batchState1;
    private Vector<ulong> _batchState2;
    private Vector<ulong> _batchState3;

    /// <summary>
    /// 从显式 64 位种子初始化由运行拥有的随机源。
    /// </summary>
    /// <param name="seed">Core 执行为当前运行选择的种子。</param>
    internal RandomSource(ulong seed)
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

        // 批量 lane i 保存 Jump^(i+1)(S)。Jump 只在局部副本上推进，单值状态保持不变。
        var batchState0 = _state0;
        var batchState1 = _state1;
        var batchState2 = _state2;
        var batchState3 = _state3;
        var laneCount = Vector<ulong>.Count;
        Span<ulong> lanes0 = stackalloc ulong[laneCount];
        Span<ulong> lanes1 = stackalloc ulong[laneCount];
        Span<ulong> lanes2 = stackalloc ulong[laneCount];
        Span<ulong> lanes3 = stackalloc ulong[laneCount];
        for (var lane = 0; lane < laneCount; lane++)
        {
            Jump(ref batchState0, ref batchState1, ref batchState2, ref batchState3);
            lanes0[lane] = batchState0;
            lanes1[lane] = batchState1;
            lanes2[lane] = batchState2;
            lanes3[lane] = batchState3;
        }

        _batchState0 = new Vector<ulong>(lanes0);
        _batchState1 = new Vector<ulong>(lanes1);
        _batchState2 = new Vector<ulong>(lanes2);
        _batchState3 = new Vector<ulong>(lanes3);
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
    /// 返回一轮批量状态的原始样本向量。
    /// </summary>
    /// <remarks>
    /// 每次调用推进全部批量 lane 一次，并返回 lane 0 至 lane L-1 的完整范围原始值。
    /// 返回值是样本值，不是状态视图；调用不分配托管内存，也不影响单值状态。
    /// </remarks>
    /// <returns>本轮批量 lane 的原始样本。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector<ulong> NextULongVector()
    {
        var state0 = _batchState0;
        var state1 = _batchState1;
        var state2 = _batchState2;
        var state3 = _batchState3;
        var result = NextRaw(ref state0, ref state1, ref state2, ref state3);
        _batchState0 = state0;
        _batchState1 = state1;
        _batchState2 = state2;
        _batchState3 = state3;
        return result;
    }

    /// <summary>
    /// 返回一轮批量状态的半开区间 <c>[0, 1)</c> 单位样本向量。
    /// </summary>
    /// <remarks>
    /// 每个 lane 对本轮原始值使用 <c>(raw &gt;&gt; 11) * 2^-53</c> 映射。lane 顺序与
    /// <see cref="NextULongVector"/> 相同，且不推进单值状态。
    /// </remarks>
    /// <returns>本轮批量 lane 的单位区间样本。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector<double> NextDoubleVector()
    {
        return ConvertToUnitInterval(NextULongVector());
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

        var laneCount = Vector<ulong>.Count;
        var roundCount = destination.Length / laneCount;
        var tailLength = destination.Length - (roundCount * laneCount);
        var state0 = _batchState0;
        var state1 = _batchState1;
        var state2 = _batchState2;
        var state3 = _batchState3;
        ref var current = ref MemoryMarshal.GetReference(destination);
        var round = 0;
        while (round + 4 <= roundCount)
        {
            var first = NextRaw(ref state0, ref state1, ref state2, ref state3);
            var second = NextRaw(ref state0, ref state1, ref state2, ref state3);
            var third = NextRaw(ref state0, ref state1, ref state2, ref state3);
            var fourth = NextRaw(ref state0, ref state1, ref state2, ref state3);
            first.StoreUnsafe(ref current);
            second.StoreUnsafe(ref current, (nuint)laneCount);
            third.StoreUnsafe(ref current, (nuint)(2 * laneCount));
            fourth.StoreUnsafe(ref current, (nuint)(3 * laneCount));
            current = ref Unsafe.Add(ref current, 4 * laneCount);
            round += 4;
        }

        while (round < roundCount)
        {
            NextRaw(ref state0, ref state1, ref state2, ref state3).StoreUnsafe(ref current);
            current = ref Unsafe.Add(ref current, laneCount);
            round++;
        }

        if (tailLength != 0)
        {
            WriteTail(
                ref current,
                NextRaw(ref state0, ref state1, ref state2, ref state3),
                tailLength);
        }

        _batchState0 = state0;
        _batchState1 = state1;
        _batchState2 = state2;
        _batchState3 = state3;
    }

    /// <summary>
    /// 返回半开区间 <c>[0, 1)</c> 内均匀分布的 double。
    /// </summary>
    /// <returns>大于等于零且小于一的值。</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double NextDouble()
    {
        return (NextRawFromFields() >> DoubleMantissaShift) * InverseTwoToThePower53;
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

        var laneCount = Vector<ulong>.Count;
        var roundCount = destination.Length / laneCount;
        var tailLength = destination.Length - (roundCount * laneCount);
        var state0 = _batchState0;
        var state1 = _batchState1;
        var state2 = _batchState2;
        var state3 = _batchState3;
        ref var current = ref MemoryMarshal.GetReference(destination);
        var round = 0;
        while (round + 4 <= roundCount)
        {
            var first = ConvertToUnitInterval(NextRaw(ref state0, ref state1, ref state2, ref state3));
            var second = ConvertToUnitInterval(NextRaw(ref state0, ref state1, ref state2, ref state3));
            var third = ConvertToUnitInterval(NextRaw(ref state0, ref state1, ref state2, ref state3));
            var fourth = ConvertToUnitInterval(NextRaw(ref state0, ref state1, ref state2, ref state3));
            first.StoreUnsafe(ref current);
            second.StoreUnsafe(ref current, (nuint)laneCount);
            third.StoreUnsafe(ref current, (nuint)(2 * laneCount));
            fourth.StoreUnsafe(ref current, (nuint)(3 * laneCount));
            current = ref Unsafe.Add(ref current, 4 * laneCount);
            round += 4;
        }

        while (round < roundCount)
        {
            ConvertToUnitInterval(NextRaw(ref state0, ref state1, ref state2, ref state3))
                .StoreUnsafe(ref current);
            current = ref Unsafe.Add(ref current, laneCount);
            round++;
        }

        if (tailLength != 0)
        {
            WriteTail(
                ref current,
                ConvertToUnitInterval(NextRaw(ref state0, ref state1, ref state2, ref state3)),
                tailLength);
        }

        _batchState0 = state0;
        _batchState1 = state1;
        _batchState2 = state2;
        _batchState3 = state3;
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

        var laneCount = Vector<ulong>.Count;
        var roundCount = destination.Length / laneCount;
        var tailLength = destination.Length - (roundCount * laneCount);
        var state0 = _batchState0;
        var state1 = _batchState1;
        var state2 = _batchState2;
        var state3 = _batchState3;

        // 区间参数每次调用只换算一次，循环内只做向量仿射变换与上界修正。
        var minimumVector = new Vector<double>(minimum);
        var maximumVector = new Vector<double>(maximum);
        var upperBoundVector = new Vector<double>(Math.BitDecrement(maximum));
        var one = Vector<double>.One;
        var width = maximum - minimum;
        var widthIsFinite = double.IsFinite(width);
        var widthVector = new Vector<double>(width);
        ref var current = ref MemoryMarshal.GetReference(destination);
        var round = 0;
        while (round < roundCount)
        {
            ScaleUnitInterval(
                ConvertToUnitInterval(NextRaw(ref state0, ref state1, ref state2, ref state3)),
                minimumVector,
                maximumVector,
                upperBoundVector,
                widthVector,
                widthIsFinite,
                one)
                .StoreUnsafe(ref current);
            current = ref Unsafe.Add(ref current, laneCount);
            round++;
        }

        if (tailLength != 0)
        {
            WriteTail(
                ref current,
                ScaleUnitInterval(
                    ConvertToUnitInterval(NextRaw(ref state0, ref state1, ref state2, ref state3)),
                    minimumVector,
                    maximumVector,
                    upperBoundVector,
                    widthVector,
                    widthIsFinite,
                    one),
                tailLength);
        }

        _batchState0 = state0;
        _batchState1 = state1;
        _batchState2 = state2;
        _batchState3 = state3;
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

        var laneCount = Vector<ulong>.Count;
        var range = (ulong)((long)maximum - minimum);
        var state0 = _batchState0;
        var state1 = _batchState1;
        var state2 = _batchState2;
        var state3 = _batchState3;
        var threshold = BoundedThreshold(range);
        Span<ulong> roundValues = stackalloc ulong[laneCount];
        ref var roundSource = ref MemoryMarshal.GetReference(roundValues);
        var index = 0;
        while (index < destination.Length)
        {
            // 一轮的原始字整轮只落栈一次，再按 lane 顺序检查：被拒绝的 lane 由后续轮次补足，
            // 轮尾未用样本丢弃。映射参数（拒绝阈值）在进入循环前算好，循环内不再做除法。
            NextRaw(ref state0, ref state1, ref state2, ref state3).StoreUnsafe(ref roundSource);
            for (var lane = 0; lane < laneCount && index < destination.Length; lane++)
            {
                if (TryMapBounded(Unsafe.Add(ref roundSource, lane), range, threshold, out var offset))
                {
                    destination[index++] = (int)((long)minimum + (long)offset);
                }
            }
        }

        _batchState0 = state0;
        _batchState1 = state1;
        _batchState2 = state2;
        _batchState3 = state3;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<double> ScaleUnitInterval(
        Vector<double> unit,
        Vector<double> minimum,
        Vector<double> maximum,
        Vector<double> upperBound,
        Vector<double> width,
        bool widthIsFinite,
        Vector<double> one)
    {
        var value = widthIsFinite
            ? minimum + (width * unit)
            : (minimum * (one - unit)) + (maximum * unit);
        return Vector.Max(minimum, Vector.Min(value, upperBound));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<double> ConvertToUnitInterval(Vector<ulong> raw)
    {
        return Vector.ConvertToDouble(raw >> DoubleMantissaShift) * new Vector<double>(InverseTwoToThePower53);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteTail(ref ulong destination, Vector<ulong> values, int length)
    {
        Span<ulong> lanes = stackalloc ulong[Vector<ulong>.Count];
        ref var source = ref MemoryMarshal.GetReference(lanes);
        values.StoreUnsafe(ref source);
        for (var index = 0; index < length; index++)
        {
            Unsafe.Add(ref destination, index) = Unsafe.Add(ref source, index);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteTail(ref double destination, Vector<double> values, int length)
    {
        Span<double> lanes = stackalloc double[Vector<double>.Count];
        ref var source = ref MemoryMarshal.GetReference(lanes);
        values.StoreUnsafe(ref source);
        for (var index = 0; index < length; index++)
        {
            Unsafe.Add(ref destination, index) = Unsafe.Add(ref source, index);
        }
    }

    // 单值路径先局部化四个状态字，再与批量路径共用同一个纯转换体。
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
        var threshold = BoundedThreshold(range);
        while (true)
        {
            var raw = NextRaw(ref state0, ref state1, ref state2, ref state3);
            if (TryMapBounded(raw, range, threshold, out var result))
            {
                return result;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong BoundedThreshold(ulong range)
    {
        // 拒绝阈值只在每次调用开始时计算一次；宽度 1 与 2 的幂不需要拒绝。
        return range <= 1 || (range & (range - 1)) == 0
            ? 0UL
            : unchecked((0UL - range) % range);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryMapBounded(ulong raw, ulong range, ulong threshold, out ulong result)
    {
        // 2 的幂宽度拒绝阈值为零；乘高位在此时等于按对应位数右移，
        // 因而常见种群索引路径不需要软件宽乘法。
        if (range == 1)
        {
            result = 0;
            return true;
        }

        if ((range & (range - 1)) == 0)
        {
            result = raw >> (WordBits - BitOperations.TrailingZeroCount(range));
            return true;
        }

        var productHigh = Math.BigMul(raw, range, out var productLow);
        result = productHigh;
        return productLow >= threshold;
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

    // 批量转换体把四个状态向量保持在局部变量里：字段只在进入和退出时各触碰一次。
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<ulong> NextRaw(
        ref Vector<ulong> state0,
        ref Vector<ulong> state1,
        ref Vector<ulong> state2,
        ref Vector<ulong> state3)
    {
        var local0 = state0;
        var local1 = state1;
        var local2 = state2;
        var local3 = state3;

        var sum = local0 + local3;
        var result = RotateLeft(sum, 23) + local0;
        var temporary = local1 << 17;

        local2 ^= local0;
        local3 ^= local1;
        local1 ^= local2;
        local0 ^= local3;
        local2 ^= temporary;
        local3 = RotateLeft(local3, 45);

        state0 = local0;
        state1 = local1;
        state2 = local2;
        state3 = local3;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<ulong> RotateLeft(Vector<ulong> value, [ConstantExpected(Min = 1, Max = 63)] byte offset)
    {
        // 宽度匹配的硬件旋转指令优先：AVX-512F 的 vprolq（512 位），以及 AVX-512VL /
        // AVX10.1 的 256 位变体。Vector<T>.Count 与 IsSupported 都是 JIT 常量，未选中的
        // 分支会被完全消除，因此这里没有运行时分派。128 位按项目作者要求暂不接入；
        // ARM64（NEON/SVE/SVE2）没有 64 位 lane 的通用旋转指令，落到下面的“两次移位 + 或”回退。
        if (Avx512F.IsSupported && Vector<ulong>.Count == Vector512<ulong>.Count)
        {
            return Avx512F.RotateLeft(value.AsVector512(), offset).AsVector();
        }

        if (Avx512F.VL.IsSupported && Vector<ulong>.Count == Vector256<ulong>.Count)
        {
            return Avx512F.VL.RotateLeft(value.AsVector256(), offset).AsVector();
        }

        if (Avx10v1.IsSupported && Vector<ulong>.Count == Vector256<ulong>.Count)
        {
            return Avx10v1.RotateLeft(value.AsVector256(), offset).AsVector();
        }

        return (value << offset) | (value >>> (WordBits - offset));
    }

    private static void Jump(
        ref ulong state0,
        ref ulong state1,
        ref ulong state2,
        ref ulong state3)
    {
        var jumpedState0 = 0UL;
        var jumpedState1 = 0UL;
        var jumpedState2 = 0UL;
        var jumpedState3 = 0UL;
        ApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumpedState0, ref jumpedState1, ref jumpedState2, ref jumpedState3,
            JumpPolynomial0);
        ApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumpedState0, ref jumpedState1, ref jumpedState2, ref jumpedState3,
            JumpPolynomial1);
        ApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumpedState0, ref jumpedState1, ref jumpedState2, ref jumpedState3,
            JumpPolynomial2);
        ApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumpedState0, ref jumpedState1, ref jumpedState2, ref jumpedState3,
            JumpPolynomial3);

        state0 = jumpedState0;
        state1 = jumpedState1;
        state2 = jumpedState2;
        state3 = jumpedState3;
    }

    private static void ApplyJump(
        ref ulong state0,
        ref ulong state1,
        ref ulong state2,
        ref ulong state3,
        ref ulong jumpedState0,
        ref ulong jumpedState1,
        ref ulong jumpedState2,
        ref ulong jumpedState3,
        ulong jumpPolynomial)
    {
        for (var bit = 0; bit < WordBits; bit++)
        {
            if ((jumpPolynomial & (1UL << bit)) != 0)
            {
                jumpedState0 ^= state0;
                jumpedState1 ^= state1;
                jumpedState2 ^= state2;
                jumpedState3 ^= state3;
            }

            NextRaw(ref state0, ref state1, ref state2, ref state3);
        }
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
