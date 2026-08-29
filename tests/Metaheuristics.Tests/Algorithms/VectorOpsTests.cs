using Anastasya.Metaheuristics.Algorithms;

namespace Anastasya.Metaheuristics.Tests.Algorithms;

/// <summary>验证 Algorithms 私有向量操作与其标量公式的数值兼容性。</summary>
public sealed class VectorOpsTests
{
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
    public void CascadingFixedWidthVectorsMatchTheScalarFormulaAtDiagnosticLengths(int length)
    {
        var sourcePosition = new double[length];
        var sourceVelocity = new double[length];
        var personalBest = new double[length];
        var globalBest = new double[length];
        var actual = new double[length];

        for (var index = 0; index < length; index++)
        {
            sourcePosition[index] = index - 4.0;
            sourceVelocity[index] = (index - 2.0) * 0.25;
            personalBest[index] = sourcePosition[index] + 0.5;
            globalBest[index] = sourcePosition[index] - 0.25;
        }

        VectorOps.ComputePsoVelocity(
            sourcePosition,
            sourceVelocity,
            personalBest,
            globalBest,
            inertia: 0.5,
            cognitiveScale: 0.75,
            socialScale: 0.25,
            destination: actual);

        for (var index = 0; index < length; index++)
        {
            var expected = (0.5 * sourceVelocity[index])
                + (0.75 * (personalBest[index] - sourcePosition[index]))
                + (0.25 * (globalBest[index] - sourcePosition[index]));
            Xunit.Assert.Equal(
                BitConverter.DoubleToInt64Bits(expected),
                BitConverter.DoubleToInt64Bits(actual[index]));
        }
    }

    [Xunit.Fact]
    public void ComputePsoVelocityMatchesTheScalarFormulaForSpecialValuesAndTailElements()
    {
        double[] sourcePosition = [2, double.PositiveInfinity, double.NegativeInfinity, double.NaN, -0.0];
        double[] sourceVelocity = [1, 2, -3, 4, -5];
        double[] personalBest = [4, double.PositiveInfinity, 1, 2, 0.0];
        double[] globalBest = [-2, 3, double.NegativeInfinity, 5, -0.0];
        var actual = new double[sourcePosition.Length];
        var expected = new double[sourcePosition.Length];

        VectorOps.ComputePsoVelocity(
            sourcePosition,
            sourceVelocity,
            personalBest,
            globalBest,
            inertia: 0.5,
            cognitiveScale: 0.75,
            socialScale: 0.25,
            destination: actual);

        for (var index = 0; index < expected.Length; index++)
        {
            expected[index] = (0.5 * sourceVelocity[index])
                + (0.75 * (personalBest[index] - sourcePosition[index]))
                + (0.25 * (globalBest[index] - sourcePosition[index]));
        }

        for (var index = 0; index < expected.Length; index++)
        {
            if (double.IsNaN(expected[index]))
            {
                Xunit.Assert.True(double.IsNaN(actual[index]));
            }
            else
            {
                Xunit.Assert.Equal(
                    BitConverter.DoubleToInt64Bits(expected[index]),
                    BitConverter.DoubleToInt64Bits(actual[index]));
            }
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
    public void FireflyPositionUpdateMatchesTheScalarFormulaAtDiagnosticLengths(int length)
    {
        var current = new double[length];
        var attractor = new double[length];
        var randomWalk = new double[length];
        var actual = new double[length];
        const double attractiveness = 0.37;

        for (var index = 0; index < length; index++)
        {
            current[index] = (index % 11) - 5;
            attractor[index] = current[index] + ((index % 5) - 2) * 0.25;
            randomWalk[index] = ((index % 7) - 3) * 0.01;
            actual[index] = current[index];
        }

        VectorOps.UpdateFireflyPosition(current, attractor, randomWalk, attractiveness, actual);

        for (var index = 0; index < length; index++)
        {
            var expected = current[index]
                + ((attractiveness * (attractor[index] - current[index])) + randomWalk[index]);
            Xunit.Assert.Equal(
                BitConverter.DoubleToInt64Bits(expected),
                BitConverter.DoubleToInt64Bits(actual[index]));
        }
    }

    [Xunit.Fact]
    public void FireflyVectorOpsPreserveSpecialValueClassification()
    {
        double[] current = [double.PositiveInfinity, double.NaN, -0.0, 2];
        double[] attractor = [0, 1, 0.0, double.NegativeInfinity];
        double[] randomWalk = [0, 0, 0, 0];
        var actual = new double[current.Length];

        var distance = VectorOps.DistanceSquared(current, attractor);
        VectorOps.UpdateFireflyPosition(current, attractor, randomWalk, 0.5, actual);

        Xunit.Assert.True(double.IsNaN(distance));
        Xunit.Assert.True(double.IsNaN(actual[0]));
        Xunit.Assert.True(double.IsNaN(actual[1]));
        Xunit.Assert.Equal(BitConverter.DoubleToInt64Bits(0.0), BitConverter.DoubleToInt64Bits(actual[2]));
        Xunit.Assert.Equal(double.NegativeInfinity, actual[3]);
    }
}
