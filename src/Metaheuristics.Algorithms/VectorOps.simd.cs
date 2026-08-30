using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

[assembly: SimdTemplate("double", SimdCapabilities.FloatingPoint)]

namespace Anastasya.Metaheuristics.Algorithms;

internal static partial class VectorOps
{
    /// <summary>计算尚未限制到速度边界的 PSO 速度。</summary>
    internal static void ComputePsoVelocity(
        ReadOnlySpan<double> sourcePosition,
        ReadOnlySpan<double> sourceVelocity,
        ReadOnlySpan<double> personalBestPosition,
        ReadOnlySpan<double> globalBestPosition,
        double inertia,
        double cognitiveScale,
        double socialScale,
        Span<double> destination)
    {
        var index = 0;
        ref var sourcePositionStart = ref MemoryMarshal.GetReference(sourcePosition);
        ref var sourceVelocityStart = ref MemoryMarshal.GetReference(sourceVelocity);
        ref var personalBestStart = ref MemoryMarshal.GetReference(personalBestPosition);
        ref var globalBestStart = ref MemoryMarshal.GetReference(globalBestPosition);
        ref var destinationStart = ref MemoryMarshal.GetReference(destination);

        __SimdExpandWidths(() =>
        {
            if (__Vector.IsHardwareAccelerated)
            {
                var inertiaVector = __Vector.Create(inertia);
                var cognitiveScaleVector = __Vector.Create(cognitiveScale);
                var socialScaleVector = __Vector.Create(socialScale);
                while (index <= sourcePosition.Length - __Vector<double>.Count)
                {
                    var position = __Vector.LoadUnsafe(ref sourcePositionStart, (nuint)index);
                    var velocity = (__Vector.LoadUnsafe(ref sourceVelocityStart, (nuint)index) * inertiaVector)
                        + ((__Vector.LoadUnsafe(ref personalBestStart, (nuint)index) - position) * cognitiveScaleVector)
                        + ((__Vector.LoadUnsafe(ref globalBestStart, (nuint)index) - position) * socialScaleVector);
                    velocity.StoreUnsafe(ref destinationStart, (nuint)index);
                    index += __Vector<double>.Count;
                }
            }
        });

        for (; index < sourcePosition.Length; index++)
        {
            destination[index] = (inertia * sourceVelocity[index])
                + (cognitiveScale * (personalBestPosition[index] - sourcePosition[index]))
                + (socialScale * (globalBestPosition[index] - sourcePosition[index]));
        }
    }

    /// <summary>平方距离。</summary>
    internal static double DistanceSquared(
        ReadOnlySpan<double> currentPosition,
        ReadOnlySpan<double> attractorPosition)
    {
        var index = 0;
        var distance = 0.0;
        ref var currentPositionStart = ref MemoryMarshal.GetReference(currentPosition);
        ref var attractorPositionStart = ref MemoryMarshal.GetReference(attractorPosition);

        __SimdExpandWidths(() =>
        {
            if (__Vector.IsHardwareAccelerated)
            {
                var sum = __Vector<double>.Zero;
                while (index <= currentPosition.Length - __Vector<double>.Count)
                {
                    var difference = __Vector.LoadUnsafe(ref currentPositionStart, (nuint)index)
                        - __Vector.LoadUnsafe(ref attractorPositionStart, (nuint)index);
                    sum += difference * difference;
                    index += __Vector<double>.Count;
                }

                distance += __Vector.Sum(sum);
            }
        });

        for (; index < currentPosition.Length; index++)
        {
            var difference = currentPosition[index] - attractorPosition[index];
            distance += difference * difference;
        }

        return distance;
    }

    /// <summary>按萤火虫公式完成一次无分配的逐维位置更新。</summary>
    internal static void UpdateFireflyPosition(
        ReadOnlySpan<double> currentPosition,
        ReadOnlySpan<double> attractorPosition,
        ReadOnlySpan<double> randomWalk,
        double attractiveness,
        Span<double> destination)
    {
        var index = 0;
        ref var currentPositionStart = ref MemoryMarshal.GetReference(currentPosition);
        ref var attractorPositionStart = ref MemoryMarshal.GetReference(attractorPosition);
        ref var randomWalkStart = ref MemoryMarshal.GetReference(randomWalk);
        ref var destinationStart = ref MemoryMarshal.GetReference(destination);

        __SimdExpandWidths(() =>
        {
            if (__Vector.IsHardwareAccelerated)
            {
                var attractivenessVector = __Vector.Create(attractiveness);
                while (index <= currentPosition.Length - __Vector<double>.Count)
                {
                    var current = __Vector.LoadUnsafe(ref currentPositionStart, (nuint)index);
                    var movement = ((__Vector.LoadUnsafe(ref attractorPositionStart, (nuint)index) - current)
                        * attractivenessVector)
                        + __Vector.LoadUnsafe(ref randomWalkStart, (nuint)index);
                    (current + movement).StoreUnsafe(ref destinationStart, (nuint)index);
                    index += __Vector<double>.Count;
                }
            }
        });

        for (; index < currentPosition.Length; index++)
        {
            destination[index] = currentPosition[index]
                + ((attractiveness * (attractorPosition[index] - currentPosition[index])) + randomWalk[index]);
        }
    }
}
