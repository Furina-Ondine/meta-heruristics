using System.Runtime.CompilerServices;

namespace Anastasya.Metaheuristics.Core.Randomness;

/// <summary>
/// 从运行拥有的 <see cref="RandomSource"/> 生成相互独立的标准正态样本。
/// </summary>
/// <remarks>
/// 实现使用 Box–Muller 成对采样。调用之间不保留备用样本，因此采样器没有跨调用可变状态，
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
    /// <param name="random">要消费的运行拥有随机源。</param>
    /// <param name="destination">要写入的 span；空 span 不消费状态。</param>
    /// <exception cref="ArgumentNullException"><paramref name="random"/> 为 <see langword="null"/>。</exception>
    public static void Fill(RandomSource random, Span<double> destination)
    {
        ArgumentNullException.ThrowIfNull(random);
        var index = 0;
        for (; index + 1 < destination.Length; index += 2)
        {
            NextPair(random, out destination[index], out destination[index + 1]);
        }

        if (index < destination.Length)
        {
            NextPair(random, out destination[index], out _);
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
