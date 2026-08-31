using Anastasya.Metaheuristics.Core.Problems;

namespace Anastasya.Metaheuristics.Tests.Core;

/// <summary>验证内置 Repair 的 Tensor 实现与独立标量参考保持兼容。</summary>
public sealed class CandidateRepairsTensorTests
{
    private static readonly int[] TestedLengths = [2, 7, 8, 31, 32, 33, 127, 128, 129, 1024];

    /// <summary>验证 Clamp 在边界形状、特殊值和向量尾部下逐位匹配标量参考。</summary>
    [Xunit.Fact]
    public void ClampMatchesTheScalarReferenceForSameShapeBoundariesAndLengths()
    {
        foreach (var length in TestedLengths)
        {
            var (lower, upper) = CreateBounds(length);
            foreach (var shape in Enum.GetValues<BoundaryShape>())
            {
                var actual = CreateClampPosition(length);
                var expected = (double[])actual.Clone();
                ClampReference(expected, lower, upper, shape);

                CreateClamp(lower, upper, shape).Repair(actual, new Random(1));

                AssertBitwiseEqual(expected, actual);
            }
        }
    }

    /// <summary>验证有限 Reflect 张量路径在批准的一 ULP 数值兼容范围内。</summary>
    [Xunit.Fact]
    public void ReflectMatchesTheScalarReferenceWithinOneUlpForFiniteInputs()
    {
        foreach (var length in TestedLengths)
        {
            var (lower, upper) = CreateBounds(length);
            foreach (var shape in Enum.GetValues<BoundaryShape>())
            {
                var actual = CreateFiniteReflectPosition(length, lower, upper, shape);
                var expected = (double[])actual.Clone();
                ReflectReference(expected, lower, upper, shape);

                CreateReflect(lower, upper, shape).Repair(actual, new Random(1));

                AssertWithinOneUlp(expected, actual);
            }
        }
    }

    /// <summary>验证 Reflect 对特殊值、端点和溢出范围继续选择逐位兼容的标量回退。</summary>
    [Xunit.Fact]
    public void ReflectUsesTheScalarSemanticsForSpecialValuesEndpointsAndOverflowingRanges()
    {
        var lower = new[] { 0.0, 0.0, double.NegativeInfinity, -double.MaxValue, 0.0 };
        var upper = new[] { 10.0, 10.0, double.PositiveInfinity, double.MaxValue, 10.0 };
        var actual = new[] { double.NaN, 0.0, double.PositiveInfinity, 1.0, double.PositiveInfinity };
        var expected = (double[])actual.Clone();

        ReflectReference(expected, lower, upper, BoundaryShape.VectorVector);
        CreateReflect(lower, upper, BoundaryShape.VectorVector).Repair(actual, new Random(1));

        AssertBitwiseEqual(expected, actual);
    }

    /// <summary>验证异常 lane 不会改变同一位置数组中其它正常 lane 的数值契约。</summary>
    [Xunit.Fact]
    public void ReflectMatchesTheReferenceForMixedFiniteAndFallbackLanes()
    {
        foreach (var length in TestedLengths)
        {
            var (lower, upper) = CreateBounds(length);
            foreach (var shape in Enum.GetValues<BoundaryShape>())
            {
                var actual = CreateMixedReflectPosition(length, lower, upper, shape);
                var source = (double[])actual.Clone();
                var expected = (double[])actual.Clone();
                ReflectReference(expected, lower, upper, shape);

                CreateReflect(lower, upper, shape).Repair(actual, new Random(1));

                AssertReflectCompatible(expected, actual, source, lower, upper, shape);
            }
        }
    }

    /// <summary>验证大但有限的偏移不会因向量 remainder 的中间舍入偏离标量参考。</summary>
    [Xunit.Fact]
    public void ReflectMatchesTheReferenceWithinOneUlpForLargeFiniteOffsets()
    {
        foreach (var (minimum, maximum) in new[] { (-10.0, 10.0), (0.1, 0.3), (-123.456, 789.123) })
        {
            var width = maximum - minimum;
            var actual = new[]
            {
                -1e100,
                -1e50,
                minimum - (width * 4095.25),
                minimum - (width * 3.75),
                maximum + (width * 3.75),
                maximum + (width * 4095.25),
                1e50,
                1e100,
            };
            var expected = (double[])actual.Clone();

            ReflectReference(expected, [minimum], [maximum], BoundaryShape.ScalarScalar);
            CandidateRepairs.Reflect(minimum, maximum).Repair(actual, new Random(1));

            AssertWithinOneUlp(expected, actual);
        }
    }

    private static (double[] Lower, double[] Upper) CreateBounds(int length)
    {
        var lower = new double[length];
        var upper = new double[length];
        for (var index = 0; index < length; index++)
        {
            lower[index] = -10 - (index % 3);
            upper[index] = 10 + (index % 5);
        }

        return (lower, upper);
    }

    private static double[] CreateClampPosition(int length)
    {
        var values = new double[length];
        for (var index = 0; index < length; index++)
        {
            values[index] = index % 11 switch
            {
                0 => double.NaN,
                1 => double.NegativeInfinity,
                2 => double.PositiveInfinity,
                _ => (index * 3.75) - 22,
            };
        }

        return values;
    }

    private static double[] CreateFiniteReflectPosition(
        int length,
        double[] lower,
        double[] upper,
        BoundaryShape shape)
    {
        var values = new double[length];
        for (var index = 0; index < length; index++)
        {
            var (minimum, maximum) = GetBounds(lower, upper, shape, index);
            var width = maximum - minimum;
            values[index] = minimum + (width * ((index % 7) - 3.25));
        }

        return values;
    }

    private static double[] CreateMixedReflectPosition(
        int length,
        double[] lower,
        double[] upper,
        BoundaryShape shape)
    {
        var values = new double[length];
        for (var index = 0; index < values.Length; index++)
        {
            var (minimum, maximum) = GetBounds(lower, upper, shape, index);
            var width = maximum - minimum;
            values[index] = index % 9 switch
            {
                0 => double.NaN,
                1 => double.NegativeInfinity,
                2 => double.PositiveInfinity,
                3 => minimum,
                4 => maximum,
                5 => minimum - (width * 0.75),
                6 => maximum + (width * 0.75),
                7 => minimum + (width * 0.25),
                _ => minimum - (width * 2.25),
            };
        }

        return values;
    }

    private static ICandidateRepair CreateClamp(double[] lower, double[] upper, BoundaryShape shape) => shape switch
    {
        BoundaryShape.ScalarScalar => CandidateRepairs.Clamp(lower[0], upper[0]),
        BoundaryShape.VectorVector => CandidateRepairs.Clamp(lower, upper),
        _ => throw new ArgumentOutOfRangeException(nameof(shape)),
    };

    private static ICandidateRepair CreateReflect(double[] lower, double[] upper, BoundaryShape shape) => shape switch
    {
        BoundaryShape.ScalarScalar => CandidateRepairs.Reflect(lower[0], upper[0]),
        BoundaryShape.VectorVector => CandidateRepairs.Reflect(lower, upper),
        _ => throw new ArgumentOutOfRangeException(nameof(shape)),
    };

    private static void ClampReference(double[] position, double[] lower, double[] upper, BoundaryShape shape)
    {
        for (var index = 0; index < position.Length; index++)
        {
            var (minimum, maximum) = GetBounds(lower, upper, shape, index);
            position[index] = Clamp(position[index], minimum, maximum);
        }
    }

    private static void ReflectReference(double[] position, double[] lower, double[] upper, BoundaryShape shape)
    {
        for (var index = 0; index < position.Length; index++)
        {
            var (minimum, maximum) = GetBounds(lower, upper, shape, index);
            position[index] = Reflect(position[index], minimum, maximum);
        }
    }

    private static (double Lower, double Upper) GetBounds(
        double[] lower,
        double[] upper,
        BoundaryShape shape,
        int index) => shape switch
        {
            BoundaryShape.ScalarScalar => (lower[0], upper[0]),
            BoundaryShape.VectorVector => (lower[index], upper[index]),
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };

    private static double Clamp(double value, double lower, double upper)
    {
        if (double.IsNaN(value))
        {
            return value;
        }

        if (value < lower)
        {
            return lower;
        }

        return value > upper ? upper : value;
    }

    private static double Reflect(double value, double lower, double upper)
    {
        if (double.IsNaN(value) || !double.IsFinite(lower) || !double.IsFinite(upper) || !double.IsFinite(value))
        {
            return Clamp(value, lower, upper);
        }

        if (value > lower && value < upper)
        {
            return value;
        }

        var width = upper - lower;
        var period = width * 2;
        if (width <= 0 || !double.IsFinite(width) || !double.IsFinite(period))
        {
            return Clamp(value, lower, upper);
        }

        var offset = value - lower;
        if (!double.IsFinite(offset))
        {
            return Clamp(value, lower, upper);
        }

        var remainder = offset % period;
        if (remainder < 0)
        {
            remainder += period;
        }

        return remainder <= width ? lower + remainder : upper - (remainder - width);
    }

    private static void AssertBitwiseEqual(double[] expected, double[] actual)
    {
        Xunit.Assert.Equal(expected.Length, actual.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            Xunit.Assert.Equal(
                BitConverter.DoubleToInt64Bits(expected[index]),
                BitConverter.DoubleToInt64Bits(actual[index]));
        }
    }

    private static void AssertWithinOneUlp(double[] expected, double[] actual)
    {
        Xunit.Assert.Equal(expected.Length, actual.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            var expectedValue = expected[index];
            var actualValue = actual[index];
            Xunit.Assert.True(
                actualValue == expectedValue
                || actualValue == double.BitIncrement(expectedValue)
                || actualValue == double.BitDecrement(expectedValue),
                $"Expected {expectedValue:R} and actual {actualValue:R} to be within one ULP at index {index}.");
        }
    }

    private static void AssertReflectCompatible(
        double[] expected,
        double[] actual,
        double[] source,
        double[] lower,
        double[] upper,
        BoundaryShape shape)
    {
        Xunit.Assert.Equal(expected.Length, actual.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            var (minimum, maximum) = GetBounds(lower, upper, shape, index);
            var width = maximum - minimum;
            var period = width * 2;
            var offset = source[index] - minimum;
            var requiresBitwiseResult = !double.IsFinite(source[index])
                || !double.IsFinite(minimum)
                || !double.IsFinite(maximum)
                || !double.IsFinite(width)
                || !double.IsFinite(period)
                || !double.IsFinite(offset)
                || !double.IsFinite(minimum + width)
                || width <= 0
                || source[index] == minimum
                || source[index] == maximum;

            if (requiresBitwiseResult)
            {
                Xunit.Assert.Equal(
                    BitConverter.DoubleToInt64Bits(expected[index]),
                    BitConverter.DoubleToInt64Bits(actual[index]));
                continue;
            }

            Xunit.Assert.True(
                actual[index] == expected[index]
                || actual[index] == double.BitIncrement(expected[index])
                || actual[index] == double.BitDecrement(expected[index]),
                $"Expected {expected[index]:R} and actual {actual[index]:R} to be within one ULP at index {index}.");
        }
    }

    private enum BoundaryShape
    {
        ScalarScalar,
        VectorVector,
    }
}
