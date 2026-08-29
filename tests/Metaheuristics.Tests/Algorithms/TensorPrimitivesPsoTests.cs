using System.Numerics.Tensors;

namespace Anastasya.Metaheuristics.Tests.Algorithms;

/// <summary>验证 PSO 候选更新将使用的 TensorPrimitives 调用形态。</summary>
public sealed class TensorPrimitivesPsoTests
{
    [Xunit.Fact]
    public void CandidateUpdateSequenceSupportsDistinctSourcesAndExactDestinationAliasing()
    {
        double[] sourcePosition = [2, -3, 4];
        double[] sourceVelocity = [3, -2, 1];
        double[] personalBest = [4, -7, 4];
        double[] globalBest = [-2, -1, 8];
        double[] candidatePosition = new double[sourcePosition.Length];
        double[] candidateVelocity = new double[sourcePosition.Length];
        const double inertia = 0.5;
        const double cognitiveScale = 0.75;
        const double socialScale = 0.25;

        TensorPrimitives.Subtract(personalBest, sourcePosition, candidatePosition);
        TensorPrimitives.Multiply(candidatePosition, cognitiveScale, candidatePosition);
        TensorPrimitives.Multiply(sourceVelocity, inertia, candidateVelocity);
        TensorPrimitives.Add(candidateVelocity, candidatePosition, candidateVelocity);
        TensorPrimitives.Subtract(globalBest, sourcePosition, candidatePosition);
        TensorPrimitives.Multiply(candidatePosition, socialScale, candidatePosition);
        TensorPrimitives.Add(candidateVelocity, candidatePosition, candidateVelocity);
        TensorPrimitives.Clamp(candidateVelocity, -1.5, 1.5, candidateVelocity);
        TensorPrimitives.Add(sourcePosition, candidateVelocity, candidatePosition);

        Xunit.Assert.Equal(new[] { 1.5, -1.5, 1.5 }, candidateVelocity);
        Xunit.Assert.Equal(new[] { 3.5, -4.5, 5.5 }, candidatePosition);
    }

    [Xunit.Fact]
    public void ClampSupportsInPlaceSpecialValueHandlingRequiredByPso()
    {
        double[] values = [double.NegativeInfinity, -2, -0.0, 0.0, 2, double.PositiveInfinity, double.NaN];

        TensorPrimitives.Clamp(values, -1.0, 1.0, values);

        Xunit.Assert.Equal(-1.0, values[0]);
        Xunit.Assert.Equal(-1.0, values[1]);
        Xunit.Assert.Equal(BitConverter.DoubleToInt64Bits(-0.0), BitConverter.DoubleToInt64Bits(values[2]));
        Xunit.Assert.Equal(0.0, values[3]);
        Xunit.Assert.Equal(1.0, values[4]);
        Xunit.Assert.Equal(1.0, values[5]);
        Xunit.Assert.True(double.IsNaN(values[6]));
    }
}
