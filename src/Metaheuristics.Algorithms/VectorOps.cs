using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Anastasya.Metaheuristics.Algorithms;

/// <summary>提供 Algorithms 程序集内经测量需要的无分配向量操作。</summary>
internal static class VectorOps
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

        if (Vector512.IsHardwareAccelerated)
        {
            var inertiaVector = Vector512.Create(inertia);
            var cognitiveScaleVector = Vector512.Create(cognitiveScale);
            var socialScaleVector = Vector512.Create(socialScale);
            while (index <= sourcePosition.Length - Vector512<double>.Count)
            {
                var position = Vector512.LoadUnsafe(ref sourcePositionStart, (nuint)index);
                var velocity = (Vector512.LoadUnsafe(ref sourceVelocityStart, (nuint)index) * inertiaVector)
                    + ((Vector512.LoadUnsafe(ref personalBestStart, (nuint)index) - position) * cognitiveScaleVector)
                    + ((Vector512.LoadUnsafe(ref globalBestStart, (nuint)index) - position) * socialScaleVector);
                velocity.StoreUnsafe(ref destinationStart, (nuint)index);
                index += Vector512<double>.Count;
            }
        }

        if (Vector256.IsHardwareAccelerated)
        {
            var inertiaVector = Vector256.Create(inertia);
            var cognitiveScaleVector = Vector256.Create(cognitiveScale);
            var socialScaleVector = Vector256.Create(socialScale);
            while (index <= sourcePosition.Length - Vector256<double>.Count)
            {
                var position = Vector256.LoadUnsafe(ref sourcePositionStart, (nuint)index);
                var velocity = (Vector256.LoadUnsafe(ref sourceVelocityStart, (nuint)index) * inertiaVector)
                    + ((Vector256.LoadUnsafe(ref personalBestStart, (nuint)index) - position) * cognitiveScaleVector)
                    + ((Vector256.LoadUnsafe(ref globalBestStart, (nuint)index) - position) * socialScaleVector);
                velocity.StoreUnsafe(ref destinationStart, (nuint)index);
                index += Vector256<double>.Count;
            }
        }

        if (Vector128.IsHardwareAccelerated)
        {
            var inertiaVector = Vector128.Create(inertia);
            var cognitiveScaleVector = Vector128.Create(cognitiveScale);
            var socialScaleVector = Vector128.Create(socialScale);
            while (index <= sourcePosition.Length - Vector128<double>.Count)
            {
                var position = Vector128.LoadUnsafe(ref sourcePositionStart, (nuint)index);
                var velocity = (Vector128.LoadUnsafe(ref sourceVelocityStart, (nuint)index) * inertiaVector)
                    + ((Vector128.LoadUnsafe(ref personalBestStart, (nuint)index) - position) * cognitiveScaleVector)
                    + ((Vector128.LoadUnsafe(ref globalBestStart, (nuint)index) - position) * socialScaleVector);
                velocity.StoreUnsafe(ref destinationStart, (nuint)index);
                index += Vector128<double>.Count;
            }
        }

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

        if (Vector512.IsHardwareAccelerated)
        {
            var sum = Vector512<double>.Zero;
            while (index <= currentPosition.Length - Vector512<double>.Count)
            {
                var difference = Vector512.LoadUnsafe(ref currentPositionStart, (nuint)index)
                    - Vector512.LoadUnsafe(ref attractorPositionStart, (nuint)index);
                sum += difference * difference;
                index += Vector512<double>.Count;
            }

            distance += Vector512.Sum(sum);
        }

        if (Vector256.IsHardwareAccelerated)
        {
            var sum = Vector256<double>.Zero;
            while (index <= currentPosition.Length - Vector256<double>.Count)
            {
                var difference = Vector256.LoadUnsafe(ref currentPositionStart, (nuint)index)
                    - Vector256.LoadUnsafe(ref attractorPositionStart, (nuint)index);
                sum += difference * difference;
                index += Vector256<double>.Count;
            }

            distance += Vector256.Sum(sum);
        }

        if (Vector128.IsHardwareAccelerated)
        {
            var sum = Vector128<double>.Zero;
            while (index <= currentPosition.Length - Vector128<double>.Count)
            {
                var difference = Vector128.LoadUnsafe(ref currentPositionStart, (nuint)index)
                    - Vector128.LoadUnsafe(ref attractorPositionStart, (nuint)index);
                sum += difference * difference;
                index += Vector128<double>.Count;
            }

            distance += Vector128.Sum(sum);
        }

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

        if (Vector512.IsHardwareAccelerated)
        {
            var attractivenessVector = Vector512.Create(attractiveness);
            while (index <= currentPosition.Length - Vector512<double>.Count)
            {
                var current = Vector512.LoadUnsafe(ref currentPositionStart, (nuint)index);
                var movement = ((Vector512.LoadUnsafe(ref attractorPositionStart, (nuint)index) - current)
                    * attractivenessVector)
                    + Vector512.LoadUnsafe(ref randomWalkStart, (nuint)index);
                (current + movement).StoreUnsafe(ref destinationStart, (nuint)index);
                index += Vector512<double>.Count;
            }
        }

        if (Vector256.IsHardwareAccelerated)
        {
            var attractivenessVector = Vector256.Create(attractiveness);
            while (index <= currentPosition.Length - Vector256<double>.Count)
            {
                var current = Vector256.LoadUnsafe(ref currentPositionStart, (nuint)index);
                var movement = ((Vector256.LoadUnsafe(ref attractorPositionStart, (nuint)index) - current)
                    * attractivenessVector)
                    + Vector256.LoadUnsafe(ref randomWalkStart, (nuint)index);
                (current + movement).StoreUnsafe(ref destinationStart, (nuint)index);
                index += Vector256<double>.Count;
            }
        }

        if (Vector128.IsHardwareAccelerated)
        {
            var attractivenessVector = Vector128.Create(attractiveness);
            while (index <= currentPosition.Length - Vector128<double>.Count)
            {
                var current = Vector128.LoadUnsafe(ref currentPositionStart, (nuint)index);
                var movement = ((Vector128.LoadUnsafe(ref attractorPositionStart, (nuint)index) - current)
                    * attractivenessVector)
                    + Vector128.LoadUnsafe(ref randomWalkStart, (nuint)index);
                (current + movement).StoreUnsafe(ref destinationStart, (nuint)index);
                index += Vector128<double>.Count;
            }
        }

        for (; index < currentPosition.Length; index++)
        {
            destination[index] = currentPosition[index]
                + ((attractiveness * (attractorPosition[index] - currentPosition[index])) + randomWalk[index]);
        }
    }
}
