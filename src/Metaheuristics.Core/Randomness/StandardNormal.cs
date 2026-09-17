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
    /// 每个块取两个单位样本向量：第一个提供半径输入 <c>u</c>，第二个提供角度输入 <c>v</c>，
    /// 同一 lane 的 <c>u</c>、<c>v</c> 配成一对，其中
    /// <c>r = sqrt(-2*log(1-u))</c>、<c>a = 2*pi*v</c>。每个 lane 都参与运算，因此不存在重复计算；
    /// 一个块产出 <c>2*L</c> 个样本并消耗两轮批量状态。
    /// </para>
    /// <para>
    /// 写出按向量粒度：每个块先写入 <c>L</c> 个 <c>r*cos(a)</c>，再写入 <c>L</c> 个 <c>r*sin(a)</c>，
    /// 不做逐值交错。
    /// </para>
    /// <para>
    /// 最后一个不足 <c>2*L</c> 的块仍取两个向量、执行完整向量数学，只写入剩余目标；
    /// 未使用的 lane 不跨调用保留。Sample 仍走标量成对路径，两者不保证产生相同序列。
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
        Span<double> tail = stackalloc double[2 * laneCount];
        var negativeTwo = new Vector<double>(-2);
        var twoPi = new Vector<double>(TwoPi);
        var one = Vector<double>.One;
        ref var tailStart = ref MemoryMarshal.GetReference(tail);
        ref var current = ref MemoryMarshal.GetReference(destination);
        var remaining = destination.Length;
        while (remaining > 0)
        {
            // 一个单位向量给半径输入、另一个给角度输入，每个 lane 都参与运算。
            var radiusInput = one - random.NextDoubleVector();
            var angleInput = random.NextDoubleVector();
            var radius = Vector.SquareRoot(negativeTwo * Vector.Log(radiusInput));
            var angle = twoPi * angleInput;
            var (sin, cos) = Vector.SinCos(angle);
            var cosine = radius * cos;
            var sine = radius * sin;
            if (remaining >= 2 * laneCount)
            {
                // 向量粒度写出：整条 cos 结果先写，整条 sin 结果随后，写回只有两条向量存储。
                cosine.StoreUnsafe(ref current);
                sine.StoreUnsafe(ref Unsafe.Add(ref current, laneCount));
                current = ref Unsafe.Add(ref current, 2 * laneCount);
                remaining -= 2 * laneCount;
                continue;
            }

            // 尾块仍执行完整向量数学，只写需要的 lane。
            cosine.StoreUnsafe(ref tailStart);
            sine.StoreUnsafe(ref Unsafe.Add(ref tailStart, laneCount));
            ref var tailSource = ref tailStart;
            for (var lane = 0; lane < remaining; lane++)
            {
                Unsafe.Add(ref current, lane) = Unsafe.Add(ref tailSource, lane);
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
