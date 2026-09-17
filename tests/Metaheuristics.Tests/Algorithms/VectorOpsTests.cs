using Anastasya.Metaheuristics.Algorithms;

namespace Anastasya.Metaheuristics.Tests.Algorithms;

/// <summary>验证 Algorithms 私有向量操作与其标量公式的数值兼容性。</summary>
public sealed class VectorOpsTests
{
    [Xunit.Theory]
    [Xunit.InlineData(1)]
    [Xunit.InlineData(7)]
    [Xunit.InlineData(8)]
    [Xunit.InlineData(24)]
    [Xunit.InlineData(25)]
    [Xunit.InlineData(32)]
    [Xunit.InlineData(64)]
    [Xunit.InlineData(65)]
    [Xunit.InlineData(128)]
    [Xunit.InlineData(129)]
    [Xunit.InlineData(1024)]
    public void CuckooCandidateUpdatesMatchTheirScalarFormulas(int length)
    {
        var source = new double[length];
        var best = new double[length];
        var first = new double[length];
        var second = new double[length];
        var numerator = new double[length];
        var denominator = new double[length];
        var unit = new double[length];
        var levyActual = new double[length];
        var abandonmentActual = new double[length];
        for (var index = 0; index < length; index++)
        {
            source[index] = (index % 11) - 5;
            best[index] = source[index] + ((index % 5) - 2) * 0.25;
            first[index] = source[index] + 1;
            second[index] = source[index] - 0.5;
            numerator[index] = ((index % 7) - 3) * 0.3;
            denominator[index] = 0.5 + ((index % 9) * 0.1);
            unit[index] = (index % 17) / 17.0;
        }

        VectorOps.UpdateCuckooLevyCandidate(
            source,
            best,
            numerator,
            denominator,
            unit,
            levySigma: 0.7,
            gaussianScale: 0.9,
            levyScale: 0.2,
            levyActual);
        VectorOps.UpdateCuckooAbandonmentCandidate(
            best,
            first,
            second,
            unit,
            differenceScale: 0.4,
            perturbationScale: 0.1,
            abandonmentActual);

        for (var index = 0; index < length; index++)
        {
            var scaledNumerator = numerator[index] * 0.7 * 0.9;
            var levyStep = 0.2 * scaledNumerator / denominator[index];
            var guidance = (best[index] - source[index]) * unit[index];
            var expectedLevy = source[index] + (0.8 * levyStep) + (0.2 * guidance);
            var perturbation = (unit[index] - 0.5) * 0.1;
            var expectedAbandonment = best[index]
                + (0.4 * (first[index] - second[index]))
                + perturbation;
            AssertSameDouble(expectedLevy, levyActual[index]);
            AssertSameDouble(expectedAbandonment, abandonmentActual[index]);
        }
    }

    [Xunit.Fact]
    public void CuckooCandidateUpdatesPreserveSpecialValueClassificationAndSignedZero()
    {
        double[] source = [double.PositiveInfinity, double.NaN, -0.0, 2, -3];
        double[] best = [0, 1, 0.0, double.NegativeInfinity, -3];
        double[] first = [1, double.PositiveInfinity, -0.0, 4, double.NaN];
        double[] second = [0, 2, 0.0, double.NegativeInfinity, 1];
        double[] numerator = [0, 1, -0.0, double.PositiveInfinity, double.NaN];
        double[] denominator = [1, double.PositiveInfinity, 1, 2, 1];
        double[] unit = [0.5, 0, 0.5, 1, 0.25];
        var levyActual = new double[source.Length];
        var abandonmentActual = new double[source.Length];

        VectorOps.UpdateCuckooLevyCandidate(
            source, best, numerator, denominator, unit, 0.7, 0.9, 0.2, levyActual);
        VectorOps.UpdateCuckooAbandonmentCandidate(
            best, first, second, unit, 0.4, 0.1, abandonmentActual);

        for (var index = 0; index < source.Length; index++)
        {
            var scaledNumerator = numerator[index] * 0.7 * 0.9;
            var levyStep = 0.2 * scaledNumerator / denominator[index];
            var expectedLevy = source[index]
                + (0.8 * levyStep)
                + (0.2 * ((best[index] - source[index]) * unit[index]));
            var expectedAbandonment = best[index]
                + (0.4 * (first[index] - second[index]))
                + ((unit[index] - 0.5) * 0.1);
            AssertSameDouble(expectedLevy, levyActual[index]);
            AssertSameDouble(expectedAbandonment, abandonmentActual[index]);
        }
    }

    [Xunit.Theory]
    [Xunit.InlineData(1)]
    [Xunit.InlineData(2)]
    [Xunit.InlineData(7)]
    [Xunit.InlineData(8)]
    [Xunit.InlineData(15)]
    [Xunit.InlineData(16)]
    [Xunit.InlineData(24)]
    [Xunit.InlineData(25)]
    [Xunit.InlineData(31)]
    [Xunit.InlineData(32)]
    [Xunit.InlineData(33)]
    [Xunit.InlineData(64)]
    [Xunit.InlineData(65)]
    [Xunit.InlineData(127)]
    [Xunit.InlineData(128)]
    [Xunit.InlineData(129)]
    [Xunit.InlineData(1024)]
    public void BatCandidateUpdateMatchesTheCompleteScalarDataFlow(int length)
    {
        var sourcePosition = new double[length];
        var sourceVelocity = new double[length];
        var sourceLoudness = new double[length];
        var sourcePulseRate = new double[length];
        var sourceInitialPulseRate = new double[length];
        var bestPosition = new double[length];
        var frequency = new double[length];
        var pulseSamples = new double[length];
        var perturbationSamples = new double[length];
        var acceptanceSamples = new double[length];
        var actualPosition = new double[length];
        var actualVelocity = new double[length];
        var actualLoudness = new double[length];
        var actualPulseRate = new double[length];
        var actualInitialPulseRate = new double[length];

        for (var index = 0; index < length; index++)
        {
            sourcePosition[index] = (index % 13) - 6;
            sourceVelocity[index] = ((index % 9) - 4) * 0.25;
            sourceLoudness[index] = (index % 5) * 0.25;
            sourcePulseRate[index] = (index % 4) * 0.25;
            sourceInitialPulseRate[index] = 0.2 + ((index % 3) * 0.1);
            bestPosition[index] = sourcePosition[index] + ((index % 7) - 3);
            frequency[index] = (index % 6) * 0.2;
            pulseSamples[index] = (index % 8) * 0.125;
            perturbationSamples[index] = ((index % 11) - 5) * 0.1;
            acceptanceSamples[index] = ((index + 3) % 8) * 0.125;
        }

        VectorOps.UpdateBatCandidate(
            sourcePosition,
            sourceVelocity,
            sourceLoudness,
            sourcePulseRate,
            sourceInitialPulseRate,
            bestPosition,
            frequency,
            pulseSamples,
            perturbationSamples,
            acceptanceSamples,
            velocityLowerBound: -1,
            velocityUpperBound: 1,
            loudnessDecay: 0.9,
            pulseRateFactor: 0.4,
            actualPosition,
            actualVelocity,
            actualLoudness,
            actualPulseRate,
            actualInitialPulseRate);

        for (var index = 0; index < length; index++)
        {
            var velocity = Math.Clamp(
                sourceVelocity[index]
                    + ((bestPosition[index] - sourcePosition[index]) * frequency[index]),
                -1,
                1);
            var nextPosition = pulseSamples[index] > sourcePulseRate[index]
                ? bestPosition[index] + (perturbationSamples[index] * sourceLoudness[index])
                : sourcePosition[index] + velocity;
            var accepted = acceptanceSamples[index] < sourceLoudness[index];

            AssertSameDouble(accepted ? nextPosition : sourcePosition[index], actualPosition[index]);
            AssertSameDouble(velocity, actualVelocity[index]);
            AssertSameDouble(accepted ? 0.9 * sourceLoudness[index] : sourceLoudness[index], actualLoudness[index]);
            AssertSameDouble(
                accepted ? sourceInitialPulseRate[index] * 0.4 : sourcePulseRate[index],
                actualPulseRate[index]);
            AssertSameDouble(sourceInitialPulseRate[index], actualInitialPulseRate[index]);
        }
    }

    [Xunit.Fact]
    public void BatCandidateUpdatePreservesScalarSpecialValueSemantics()
    {
        double[] sourcePosition = [double.PositiveInfinity, double.NaN, -0.0, 2, -3];
        double[] sourceVelocity = [1, 2, -0.0, double.NegativeInfinity, 0];
        double[] sourceLoudness = [1, double.NaN, 0, 0.5, double.PositiveInfinity];
        double[] sourcePulseRate = [0.5, 0.5, 0.5, double.NaN, 0];
        double[] sourceInitialPulseRate = [0.2, 0.3, -0.0, 0.4, double.NaN];
        double[] bestPosition = [0, 1, 0.0, double.PositiveInfinity, -3];
        double[] frequency = [0, 1, 0, 1, double.NaN];
        double[] pulseSamples = [0, 1, 0, 1, 1];
        double[] perturbationSamples = [0, 1, -0.0, double.NegativeInfinity, 0];
        double[] acceptanceSamples = [0, 0, 1, 0, 0];
        var position = new double[sourcePosition.Length];
        var velocity = new double[sourcePosition.Length];
        var loudness = new double[sourcePosition.Length];
        var pulseRate = new double[sourcePosition.Length];
        var initialPulseRate = new double[sourcePosition.Length];

        VectorOps.UpdateBatCandidate(
            sourcePosition, sourceVelocity, sourceLoudness, sourcePulseRate,
            sourceInitialPulseRate, bestPosition, frequency, pulseSamples,
            perturbationSamples, acceptanceSamples, -1, 1, 0.9, 0.4,
            position, velocity, loudness, pulseRate, initialPulseRate);

        for (var index = 0; index < sourcePosition.Length; index++)
        {
            var expectedVelocity = Math.Clamp(
                sourceVelocity[index]
                    + ((bestPosition[index] - sourcePosition[index]) * frequency[index]),
                -1,
                1);
            var nextPosition = pulseSamples[index] > sourcePulseRate[index]
                ? bestPosition[index] + (perturbationSamples[index] * sourceLoudness[index])
                : sourcePosition[index] + expectedVelocity;
            var accepted = acceptanceSamples[index] < sourceLoudness[index];
            AssertSameDouble(accepted ? nextPosition : sourcePosition[index], position[index]);
            AssertSameDouble(expectedVelocity, velocity[index]);
            AssertSameDouble(accepted ? 0.9 * sourceLoudness[index] : sourceLoudness[index], loudness[index]);
            AssertSameDouble(
                accepted ? sourceInitialPulseRate[index] * 0.4 : sourcePulseRate[index],
                pulseRate[index]);
            AssertSameDouble(sourceInitialPulseRate[index], initialPulseRate[index]);
        }
    }

    [Xunit.Theory]
    [Xunit.InlineData(1)]
    [Xunit.InlineData(7)]
    [Xunit.InlineData(8)]
    [Xunit.InlineData(24)]
    [Xunit.InlineData(25)]
    [Xunit.InlineData(32)]
    [Xunit.InlineData(64)]
    [Xunit.InlineData(65)]
    [Xunit.InlineData(128)]
    [Xunit.InlineData(129)]
    [Xunit.InlineData(1024)]
    public void PsoCandidateUpdateMatchesTheCompleteScalarDataFlow(int length)
    {
        var sourcePosition = new double[length];
        var sourceVelocity = new double[length];
        var personalBest = new double[length];
        var globalBest = new double[length];
        var actualVelocity = new double[length];
        var actualPosition = new double[length];
        for (var index = 0; index < length; index++)
        {
            sourcePosition[index] = (index % 11) - 5;
            sourceVelocity[index] = ((index % 7) - 3) * 0.25;
            personalBest[index] = sourcePosition[index] + ((index % 3) - 1);
            globalBest[index] = sourcePosition[index] + ((index % 5) - 2);
        }

        VectorOps.UpdatePsoCandidate(
            sourcePosition,
            sourceVelocity,
            personalBest,
            globalBest,
            inertia: 0.79,
            cognitiveScale: 0.75,
            socialScale: 0.25,
            velocityLowerBound: -1,
            velocityUpperBound: 1,
            actualVelocity,
            actualPosition);

        for (var index = 0; index < length; index++)
        {
            var velocity = Math.Clamp(
                (0.79 * sourceVelocity[index])
                    + (0.75 * (personalBest[index] - sourcePosition[index]))
                    + (0.25 * (globalBest[index] - sourcePosition[index])),
                -1,
                1);
            AssertSameDouble(velocity, actualVelocity[index]);
            AssertSameDouble(sourcePosition[index] + velocity, actualPosition[index]);
        }
    }

    [Xunit.Fact]
    public void PsoCandidateUpdatePreservesScalarSpecialValueSemantics()
    {
        double[] sourcePosition = [double.PositiveInfinity, double.NaN, -0.0, 2, -3];
        double[] sourceVelocity = [1, 2, -0.0, double.NegativeInfinity, 0];
        double[] personalBest = [0, 1, 0.0, 4, double.NaN];
        double[] globalBest = [double.PositiveInfinity, 3, -0.0, double.NegativeInfinity, -3];
        var velocity = new double[sourcePosition.Length];
        var position = new double[sourcePosition.Length];

        VectorOps.UpdatePsoCandidate(
            sourcePosition, sourceVelocity, personalBest, globalBest,
            0.79, 0.75, 0.25, -1, 1, velocity, position);

        for (var index = 0; index < sourcePosition.Length; index++)
        {
            var expectedVelocity = Math.Clamp(
                (0.79 * sourceVelocity[index])
                    + (0.75 * (personalBest[index] - sourcePosition[index]))
                    + (0.25 * (globalBest[index] - sourcePosition[index])),
                -1,
                1);
            AssertSameDouble(expectedVelocity, velocity[index]);
            AssertSameDouble(sourcePosition[index] + expectedVelocity, position[index]);
        }
    }

    [Xunit.Theory]
    [Xunit.InlineData(2)]
    [Xunit.InlineData(7)]
    [Xunit.InlineData(8)]
    [Xunit.InlineData(15)]
    [Xunit.InlineData(16)]
    [Xunit.InlineData(31)]
    [Xunit.InlineData(32)]
    [Xunit.InlineData(33)]
    [Xunit.InlineData(127)]
    [Xunit.InlineData(128)]
    [Xunit.InlineData(129)]
    public void FireflyDistanceUsesTheFixedWidthCascadeAtDiagnosticLengths(int length)
    {
        var current = new double[length];
        var attractor = new double[length];
        var expected = 0.0;
        for (var index = 0; index < length; index++)
        {
            current[index] = index % 5;
            attractor[index] = (index % 5) - 2;
            var difference = current[index] - attractor[index];
            expected += difference * difference;
        }

        var actual = VectorOps.DistanceSquared(current, attractor);

        Xunit.Assert.Equal(expected, actual);
    }

    [Xunit.Theory]
    [Xunit.InlineData(1)]
    [Xunit.InlineData(7)]
    [Xunit.InlineData(8)]
    [Xunit.InlineData(24)]
    [Xunit.InlineData(25)]
    [Xunit.InlineData(32)]
    [Xunit.InlineData(64)]
    [Xunit.InlineData(65)]
    [Xunit.InlineData(128)]
    [Xunit.InlineData(129)]
    [Xunit.InlineData(1024)]
    public void FireflyUnitSampleFusionMatchesTheScalarFormula(int length)
    {
        var current = new double[length];
        var attractor = new double[length];
        var unitRandom = new double[length];
        var actual = new double[length];
        for (var index = 0; index < length; index++)
        {
            current[index] = (index % 11) - 5;
            attractor[index] = current[index] + ((index % 5) - 2) * 0.25;
            unitRandom[index] = (index % 17) / 17.0;
        }

        VectorOps.UpdateFireflyPositionFromUnitSamples(
            current,
            attractor,
            unitRandom,
            randomStep: 0.1,
            attractiveness: 0.37,
            actual);

        for (var index = 0; index < length; index++)
        {
            var randomWalk = 0.1 * (unitRandom[index] - 0.5);
            var expected = current[index]
                + ((0.37 * (attractor[index] - current[index])) + randomWalk);
            AssertSameDouble(expected, actual[index]);
        }
    }

    [Xunit.Fact]
    public void FireflyUnitSampleFusionPreservesSpecialValueClassificationAndSignedZero()
    {
        double[] current = [double.PositiveInfinity, double.NaN, -0.0, 2, -3];
        double[] attractor = [0, 1, 0.0, double.NegativeInfinity, -3];
        double[] unit = [0.5, 0, 0.5, 1, double.NaN];
        var actual = new double[current.Length];

        VectorOps.UpdateFireflyPositionFromUnitSamples(current, attractor, unit, 0.1, 0.5, actual);

        for (var index = 0; index < current.Length; index++)
        {
            var randomWalk = 0.1 * (unit[index] - 0.5);
            var expected = current[index]
                + ((0.5 * (attractor[index] - current[index])) + randomWalk);
            AssertSameDouble(expected, actual[index]);
        }
    }

    private static void AssertSameDouble(double expected, double actual)
    {
        if (double.IsNaN(expected))
        {
            Xunit.Assert.True(double.IsNaN(actual));
            return;
        }

        Xunit.Assert.Equal(
            BitConverter.DoubleToInt64Bits(expected),
            BitConverter.DoubleToInt64Bits(actual));
    }

}
