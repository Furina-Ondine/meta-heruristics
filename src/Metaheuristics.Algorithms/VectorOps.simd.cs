using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

[assembly: SimdTemplate("double", SimdCapabilities.FloatingPoint)]

namespace Anastasya.Metaheuristics.Algorithms;

internal static partial class VectorOps
{
    /// <summary>使用已生成样本和标量 Math.Pow 分母完成 Cuckoo Lévy 候选位置更新。</summary>
    internal static void UpdateCuckooLevyCandidate(
        ReadOnlySpan<double> sourcePosition,
        ReadOnlySpan<double> bestPosition,
        ReadOnlySpan<double> numeratorSamples,
        ReadOnlySpan<double> denominators,
        ReadOnlySpan<double> unitSamples,
        double levySigma,
        double gaussianScale,
        double levyScale,
        Span<double> targetPosition)
    {
        var index = 0;
        ref var sourcePositionStart = ref MemoryMarshal.GetReference(sourcePosition);
        ref var bestPositionStart = ref MemoryMarshal.GetReference(bestPosition);
        ref var numeratorSamplesStart = ref MemoryMarshal.GetReference(numeratorSamples);
        ref var denominatorsStart = ref MemoryMarshal.GetReference(denominators);
        ref var unitSamplesStart = ref MemoryMarshal.GetReference(unitSamples);
        ref var targetPositionStart = ref MemoryMarshal.GetReference(targetPosition);

        __SimdExpandHardwareAcceleratedWidths(() =>
        {
            var levySigmaVector = __Vector.Create(levySigma);
            var gaussianScaleVector = __Vector.Create(gaussianScale);
            var levyScaleVector = __Vector.Create(levyScale);
            var levyWeight = __Vector.Create(0.8);
            var guidanceWeight = __Vector.Create(0.2);
            while (index <= sourcePosition.Length - __Vector<double>.Count)
            {
                var source = __Vector.LoadUnsafe(ref sourcePositionStart, (nuint)index);
                var numerator = __Vector.LoadUnsafe(ref numeratorSamplesStart, (nuint)index)
                    * levySigmaVector
                    * gaussianScaleVector;
                var levyStep = levyScaleVector * numerator
                    / __Vector.LoadUnsafe(ref denominatorsStart, (nuint)index);
                var guidance = (__Vector.LoadUnsafe(ref bestPositionStart, (nuint)index) - source)
                    * __Vector.LoadUnsafe(ref unitSamplesStart, (nuint)index);
                (source + (levyWeight * levyStep) + (guidanceWeight * guidance))
                    .StoreUnsafe(ref targetPositionStart, (nuint)index);
                index += __Vector<double>.Count;
            }
        });

        for (; index < sourcePosition.Length; index++)
        {
            var numerator = numeratorSamples[index] * levySigma * gaussianScale;
            var levyStep = levyScale * numerator / denominators[index];
            var guidance = (bestPosition[index] - sourcePosition[index]) * unitSamples[index];
            targetPosition[index] = sourcePosition[index]
                + (0.8 * levyStep)
                + (0.2 * guidance);
        }
    }

    /// <summary>完成 Cuckoo 遗弃阶段的差分、扰动与位置写回。</summary>
    internal static void UpdateCuckooAbandonmentCandidate(
        ReadOnlySpan<double> bestPosition,
        ReadOnlySpan<double> firstPosition,
        ReadOnlySpan<double> secondPosition,
        ReadOnlySpan<double> unitSamples,
        double differenceScale,
        double perturbationScale,
        Span<double> targetPosition)
    {
        var index = 0;
        ref var bestPositionStart = ref MemoryMarshal.GetReference(bestPosition);
        ref var firstPositionStart = ref MemoryMarshal.GetReference(firstPosition);
        ref var secondPositionStart = ref MemoryMarshal.GetReference(secondPosition);
        ref var unitSamplesStart = ref MemoryMarshal.GetReference(unitSamples);
        ref var targetPositionStart = ref MemoryMarshal.GetReference(targetPosition);

        __SimdExpandHardwareAcceleratedWidths(() =>
        {
            var differenceScaleVector = __Vector.Create(differenceScale);
            var perturbationScaleVector = __Vector.Create(perturbationScale);
            var half = __Vector.Create(0.5);
            while (index <= bestPosition.Length - __Vector<double>.Count)
            {
                var perturbation = (__Vector.LoadUnsafe(ref unitSamplesStart, (nuint)index) - half)
                    * perturbationScaleVector;
                (__Vector.LoadUnsafe(ref bestPositionStart, (nuint)index)
                    + (differenceScaleVector
                        * (__Vector.LoadUnsafe(ref firstPositionStart, (nuint)index)
                            - __Vector.LoadUnsafe(ref secondPositionStart, (nuint)index)))
                    + perturbation)
                    .StoreUnsafe(ref targetPositionStart, (nuint)index);
                index += __Vector<double>.Count;
            }
        });

        for (; index < bestPosition.Length; index++)
        {
            var perturbation = (unitSamples[index] - 0.5) * perturbationScale;
            targetPosition[index] = bestPosition[index]
                + (differenceScale * (firstPosition[index] - secondPosition[index]))
                + perturbation;
        }
    }

    /// <summary>按 Bat 的完整候选数据流更新目标状态。</summary>
    internal static void UpdateBatCandidate(
        ReadOnlySpan<double> sourcePosition,
        ReadOnlySpan<double> sourceVelocity,
        ReadOnlySpan<double> sourceLoudness,
        ReadOnlySpan<double> sourcePulseRate,
        ReadOnlySpan<double> sourceInitialPulseRate,
        ReadOnlySpan<double> bestPosition,
        ReadOnlySpan<double> frequency,
        ReadOnlySpan<double> pulseSamples,
        ReadOnlySpan<double> perturbationSamples,
        ReadOnlySpan<double> acceptanceSamples,
        double velocityLowerBound,
        double velocityUpperBound,
        double loudnessDecay,
        double pulseRateFactor,
        Span<double> targetPosition,
        Span<double> targetVelocity,
        Span<double> targetLoudness,
        Span<double> targetPulseRate,
        Span<double> targetInitialPulseRate)
    {
        var index = 0;
        ref var sourcePositionStart = ref MemoryMarshal.GetReference(sourcePosition);
        ref var sourceVelocityStart = ref MemoryMarshal.GetReference(sourceVelocity);
        ref var sourceLoudnessStart = ref MemoryMarshal.GetReference(sourceLoudness);
        ref var sourcePulseRateStart = ref MemoryMarshal.GetReference(sourcePulseRate);
        ref var sourceInitialPulseRateStart = ref MemoryMarshal.GetReference(sourceInitialPulseRate);
        ref var bestPositionStart = ref MemoryMarshal.GetReference(bestPosition);
        ref var frequencyStart = ref MemoryMarshal.GetReference(frequency);
        ref var pulseSamplesStart = ref MemoryMarshal.GetReference(pulseSamples);
        ref var perturbationSamplesStart = ref MemoryMarshal.GetReference(perturbationSamples);
        ref var acceptanceSamplesStart = ref MemoryMarshal.GetReference(acceptanceSamples);
        ref var targetPositionStart = ref MemoryMarshal.GetReference(targetPosition);
        ref var targetVelocityStart = ref MemoryMarshal.GetReference(targetVelocity);
        ref var targetLoudnessStart = ref MemoryMarshal.GetReference(targetLoudness);
        ref var targetPulseRateStart = ref MemoryMarshal.GetReference(targetPulseRate);
        ref var targetInitialPulseRateStart = ref MemoryMarshal.GetReference(targetInitialPulseRate);

        __SimdExpandHardwareAcceleratedWidths(() =>
        {
            var velocityLowerBoundVector = __Vector.Create(velocityLowerBound);
            var velocityUpperBoundVector = __Vector.Create(velocityUpperBound);
            var loudnessDecayVector = __Vector.Create(loudnessDecay);
            var pulseRateFactorVector = __Vector.Create(pulseRateFactor);
            while (index <= sourcePosition.Length - __Vector<double>.Count)
            {
                var sourcePositionVector = __Vector.LoadUnsafe(ref sourcePositionStart, (nuint)index);
                var sourceVelocityVector = __Vector.LoadUnsafe(ref sourceVelocityStart, (nuint)index);
                var sourceLoudnessVector = __Vector.LoadUnsafe(ref sourceLoudnessStart, (nuint)index);
                var sourcePulseRateVector = __Vector.LoadUnsafe(ref sourcePulseRateStart, (nuint)index);
                var sourceInitialPulseRateVector = __Vector.LoadUnsafe(
                    ref sourceInitialPulseRateStart,
                    (nuint)index);
                var bestPositionVector = __Vector.LoadUnsafe(ref bestPositionStart, (nuint)index);
                var frequencyVector = __Vector.LoadUnsafe(ref frequencyStart, (nuint)index);

                var velocity = sourceVelocityVector
                    + ((bestPositionVector - sourcePositionVector) * frequencyVector);
                velocity = __Vector.ConditionalSelect(
                    __Vector.LessThan(velocity, velocityLowerBoundVector),
                    velocityLowerBoundVector,
                    velocity);
                velocity = __Vector.ConditionalSelect(
                    __Vector.GreaterThan(velocity, velocityUpperBoundVector),
                    velocityUpperBoundVector,
                    velocity);

                var perturbedPosition = bestPositionVector
                    + (__Vector.LoadUnsafe(ref perturbationSamplesStart, (nuint)index)
                        * sourceLoudnessVector);
                var velocityPosition = sourcePositionVector + velocity;
                var nextPosition = __Vector.ConditionalSelect(
                    __Vector.GreaterThan(
                        __Vector.LoadUnsafe(ref pulseSamplesStart, (nuint)index),
                        sourcePulseRateVector),
                    perturbedPosition,
                    velocityPosition);
                var accepted = __Vector.LessThan(
                    __Vector.LoadUnsafe(ref acceptanceSamplesStart, (nuint)index),
                    sourceLoudnessVector);

                __Vector.ConditionalSelect(accepted, nextPosition, sourcePositionVector)
                    .StoreUnsafe(ref targetPositionStart, (nuint)index);
                velocity.StoreUnsafe(ref targetVelocityStart, (nuint)index);
                __Vector.ConditionalSelect(
                    accepted,
                    loudnessDecayVector * sourceLoudnessVector,
                    sourceLoudnessVector)
                    .StoreUnsafe(ref targetLoudnessStart, (nuint)index);
                __Vector.ConditionalSelect(
                    accepted,
                    sourceInitialPulseRateVector * pulseRateFactorVector,
                    sourcePulseRateVector)
                    .StoreUnsafe(ref targetPulseRateStart, (nuint)index);
                sourceInitialPulseRateVector.StoreUnsafe(ref targetInitialPulseRateStart, (nuint)index);
                index += __Vector<double>.Count;
            }
        });

        for (; index < sourcePosition.Length; index++)
        {
            var velocity = sourceVelocity[index]
                + ((bestPosition[index] - sourcePosition[index]) * frequency[index]);
            targetVelocity[index] = Math.Clamp(velocity, velocityLowerBound, velocityUpperBound);

            var nextPosition = pulseSamples[index] > sourcePulseRate[index]
                ? bestPosition[index] + (perturbationSamples[index] * sourceLoudness[index])
                : sourcePosition[index] + targetVelocity[index];

            targetInitialPulseRate[index] = sourceInitialPulseRate[index];
            if (acceptanceSamples[index] < sourceLoudness[index])
            {
                targetPosition[index] = nextPosition;
                targetLoudness[index] = loudnessDecay * sourceLoudness[index];
                targetPulseRate[index] = sourceInitialPulseRate[index] * pulseRateFactor;
            }
            else
            {
                targetPosition[index] = sourcePosition[index];
                targetLoudness[index] = sourceLoudness[index];
                targetPulseRate[index] = sourcePulseRate[index];
            }
        }
    }

    /// <summary>连续完成 PSO 速度公式、限幅与位置双输出写回。</summary>
    internal static void UpdatePsoCandidate(
        ReadOnlySpan<double> sourcePosition,
        ReadOnlySpan<double> sourceVelocity,
        ReadOnlySpan<double> personalBestPosition,
        ReadOnlySpan<double> globalBestPosition,
        double inertia,
        double cognitiveScale,
        double socialScale,
        double velocityLowerBound,
        double velocityUpperBound,
        Span<double> targetVelocity,
        Span<double> targetPosition)
    {
        var index = 0;
        ref var sourcePositionStart = ref MemoryMarshal.GetReference(sourcePosition);
        ref var sourceVelocityStart = ref MemoryMarshal.GetReference(sourceVelocity);
        ref var personalBestStart = ref MemoryMarshal.GetReference(personalBestPosition);
        ref var globalBestStart = ref MemoryMarshal.GetReference(globalBestPosition);
        ref var targetVelocityStart = ref MemoryMarshal.GetReference(targetVelocity);
        ref var targetPositionStart = ref MemoryMarshal.GetReference(targetPosition);

        __SimdExpandHardwareAcceleratedWidths(() =>
        {
            var inertiaVector = __Vector.Create(inertia);
            var cognitiveScaleVector = __Vector.Create(cognitiveScale);
            var socialScaleVector = __Vector.Create(socialScale);
            var velocityLowerBoundVector = __Vector.Create(velocityLowerBound);
            var velocityUpperBoundVector = __Vector.Create(velocityUpperBound);
            while (index <= sourcePosition.Length - __Vector<double>.Count)
            {
                var position = __Vector.LoadUnsafe(ref sourcePositionStart, (nuint)index);
                var velocity = (__Vector.LoadUnsafe(ref sourceVelocityStart, (nuint)index) * inertiaVector)
                    + ((__Vector.LoadUnsafe(ref personalBestStart, (nuint)index) - position)
                        * cognitiveScaleVector)
                    + ((__Vector.LoadUnsafe(ref globalBestStart, (nuint)index) - position)
                        * socialScaleVector);
                velocity = __Vector.ConditionalSelect(
                    __Vector.LessThan(velocity, velocityLowerBoundVector),
                    velocityLowerBoundVector,
                    velocity);
                velocity = __Vector.ConditionalSelect(
                    __Vector.GreaterThan(velocity, velocityUpperBoundVector),
                    velocityUpperBoundVector,
                    velocity);
                velocity.StoreUnsafe(ref targetVelocityStart, (nuint)index);
                (position + velocity).StoreUnsafe(ref targetPositionStart, (nuint)index);
                index += __Vector<double>.Count;
            }
        });

        for (; index < sourcePosition.Length; index++)
        {
            var velocity = (inertia * sourceVelocity[index])
                + (cognitiveScale * (personalBestPosition[index] - sourcePosition[index]))
                + (socialScale * (globalBestPosition[index] - sourcePosition[index]));
            velocity = Math.Clamp(velocity, velocityLowerBound, velocityUpperBound);
            targetVelocity[index] = velocity;
            targetPosition[index] = sourcePosition[index] + velocity;
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

        __SimdExpandHardwareAcceleratedWidths(() =>
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
        });

        for (; index < currentPosition.Length; index++)
        {
            var difference = currentPosition[index] - attractorPosition[index];
            distance += difference * difference;
        }

        return distance;
    }

    /// <summary>把单位随机样本的缩放融合进萤火虫位置更新。</summary>
    internal static void UpdateFireflyPositionFromUnitSamples(
        ReadOnlySpan<double> currentPosition,
        ReadOnlySpan<double> attractorPosition,
        ReadOnlySpan<double> unitRandom,
        double randomStep,
        double attractiveness,
        Span<double> destination)
    {
        var index = 0;
        ref var currentPositionStart = ref MemoryMarshal.GetReference(currentPosition);
        ref var attractorPositionStart = ref MemoryMarshal.GetReference(attractorPosition);
        ref var unitRandomStart = ref MemoryMarshal.GetReference(unitRandom);
        ref var destinationStart = ref MemoryMarshal.GetReference(destination);

        __SimdExpandHardwareAcceleratedWidths(() =>
        {
            var attractivenessVector = __Vector.Create(attractiveness);
            var randomStepVector = __Vector.Create(randomStep);
            var half = __Vector.Create(0.5);
            while (index <= currentPosition.Length - __Vector<double>.Count)
            {
                var current = __Vector.LoadUnsafe(ref currentPositionStart, (nuint)index);
                var randomWalk = randomStepVector
                    * (__Vector.LoadUnsafe(ref unitRandomStart, (nuint)index) - half);
                var movement = ((__Vector.LoadUnsafe(ref attractorPositionStart, (nuint)index) - current)
                    * attractivenessVector)
                    + randomWalk;
                (current + movement).StoreUnsafe(ref destinationStart, (nuint)index);
                index += __Vector<double>.Count;
            }
        });

        for (; index < currentPosition.Length; index++)
        {
            var randomWalk = randomStep * (unitRandom[index] - 0.5);
            destination[index] = currentPosition[index]
                + ((attractiveness * (attractorPosition[index] - currentPosition[index])) + randomWalk);
        }
    }
}
