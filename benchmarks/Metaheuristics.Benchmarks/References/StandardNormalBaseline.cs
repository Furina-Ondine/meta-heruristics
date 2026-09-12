using System.Runtime.CompilerServices;

namespace Anastasya.Metaheuristics.Benchmarks.References;

/// <summary>
/// 基准基线 A 的标准正态采样：SPEC-0009 的标量 Box–Muller 成对路径。
/// </summary>
internal static class StandardNormalBaseline
{
    private const double TwoPi = 2 * Math.PI;

    internal static double Sample(RandomSourceBaseline random)
    {
        ArgumentNullException.ThrowIfNull(random);
        NextPair(random, out var first, out _);
        return first;
    }

    internal static void Fill(RandomSourceBaseline random, Span<double> destination)
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
    private static void NextPair(RandomSourceBaseline random, out double first, out double second)
    {
        var radiusInput = 1 - random.NextDouble();
        var angleInput = random.NextDouble();
        var radius = Math.Sqrt(-2 * Math.Log(radiusInput));
        var angle = TwoPi * angleInput;
        first = radius * Math.Cos(angle);
        second = radius * Math.Sin(angle);
    }
}
