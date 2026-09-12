using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Anastasya.Metaheuristics.Core.Randomness;

/// <summary>
/// 从运行拥有的 <see cref="RandomSource"/> 生成相互独立的标准正态样本。
/// </summary>
/// <remarks>
/// 实现使用 Box–Muller 成对采样。调用之间不保留备用样本，因此采样器没有跨调用可变状态：
/// <see cref="Sample"/> 只推进单值状态，<see cref="Fill"/> 只推进批量状态并使用向量数学。
/// 传入随机源的生命周期由调用方负责。
/// </remarks>
public static class StandardNormal
{
    private const double TwoPi = 2 * Math.PI;

    /// <summary>
    /// 生成一个均值为零、标准差为一的标准正态样本。
    /// </summary>
    /// <param name="random">要消费的运行拥有随机源。</param>
    /// <returns>一个标准正态样本。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="random"/> 为 <see langword="null"/>。</exception>
    public static double Sample(RandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);
        NextPair(random, out var first, out _);
        return first;
    }

    /// <summary>
    /// 将标准正态样本写入调用方拥有的 span。
    /// </summary>
    /// <param name="random">要消费批量状态的运行拥有随机源；不推进其单值状态。</param>
    /// <param name="destination">要写入的 span；空 span 不消费状态。</param>
    /// <remarks>
    /// <para>
    /// 每轮取一个单位样本向量，并按相邻 lane 配对：lane 2j 作为半径输入 u、lane 2j+1 作为
    /// 角度输入 v，输出 <c>r*cos(a)</c>、<c>r*sin(a)</c>，其中
    /// <c>r = sqrt(-2*log(1-u))</c>、<c>a = 2*pi*v</c>。
    /// </para>
    /// <para>
    /// 长度不足一轮时仍执行完整一轮向量数学，只写入剩余目标；未使用的 lane 不跨调用保留。
    /// 不保证与实际向量宽度之外的切分或连续单值采样产生相同序列。
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="random"/> 为 <see langword="null"/>。</exception>
    public static void Fill(RandomSource random, Span<double> destination)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (destination.IsEmpty)
        {
            return;
        }

        var laneCount = Vector<double>.Count;
        Span<double> radiusInputs = stackalloc double[laneCount];
        Span<double> angleInputs = stackalloc double[laneCount];
        Span<long> cosineSelector = stackalloc long[laneCount];
        Span<double> tailLanes = stackalloc double[laneCount];
        for (var lane = 0; lane < laneCount; lane++)
        {
            cosineSelector[lane] = (lane & 1) == 0 ? -1L : 0L;
        }

        var cosineMask = new Vector<long>(cosineSelector);
        var negativeTwo = new Vector<double>(-2);
        var twoPi = new Vector<double>(TwoPi);
        var one = Vector<double>.One;
        ref var current = ref MemoryMarshal.GetReference(destination);
        var remaining = destination.Length;
        while (remaining > 0)
        {
            var uniform = random.NextDoubleVector();

            // Vector<T> 没有 lane 重排 API：只有输入重排走标量 lane，
            // 半径、角度与三角运算仍在整个向量上完成。
            for (var lane = 0; lane < laneCount; lane++)
            {
                radiusInputs[lane] = uniform[lane & ~1];
                angleInputs[lane] = uniform[lane | 1];
            }

            var radius = Vector.SquareRoot(
                negativeTwo * Vector.Log(one - new Vector<double>(radiusInputs)));
            var angle = twoPi * new Vector<double>(angleInputs);
            var (sin, cos) = Vector.SinCos(angle);
            var samples = radius * Vector.ConditionalSelect(cosineMask, cos, sin);
            if (remaining >= laneCount)
            {
                samples.StoreUnsafe(ref current);
                current = ref Unsafe.Add(ref current, laneCount);
                remaining -= laneCount;
                continue;
            }

            samples.CopyTo(tailLanes);
            ref var tail = ref MemoryMarshal.GetReference(tailLanes);
            for (var lane = 0; lane < remaining; lane++)
            {
                Unsafe.Add(ref current, lane) = Unsafe.Add(ref tail, lane);
            }

            remaining = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void NextPair(RandomSource random, out double first, out double second)
    {
        var radiusInput = 1 - random.NextDouble();
        var angleInput = random.NextDouble();
        var radius = Math.Sqrt(-2 * Math.Log(radiusInput));
        var angle = TwoPi * angleInput;
        first = radius * Math.Cos(angle);
        second = radius * Math.Sin(angle);
    }
}
