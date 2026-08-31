using System.Numerics.Tensors;
using System.Runtime.Intrinsics;

namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>创建在算法每次写入位置后就地恢复候选分量的内置策略。</summary>
/// <remarks>
/// 两端必须同为标量或同为逐维向量；向量在创建时复制，并在 Repair 时要求长度与位置一致。
/// 端点不能为 <see cref="double.NaN"/>，每一维下界不能大于上界；
/// <see cref="double.NegativeInfinity"/> 和 <see cref="double.PositiveInfinity"/> 分别表示无下界和无上界。
/// 所有策略保留位置中的 <see cref="double.NaN"/>，且不承担约束判定职责。
/// </remarks>
public static class CandidateRepairs
{
    /// <summary>创建把每个越界分量截到最近标量端点的 Repair。</summary>
    public static ICandidateRepair Clamp(double lower, double upper)
    {
        ValidateScalarBounds(lower, upper);
        return new ScalarClampCandidateRepair(lower, upper);
    }

    /// <summary>创建把每个越界分量截到最近逐维端点的 Repair。</summary>
    /// <remarks>两个端点向量在创建时复制且长度必须相等；Repair 时该长度必须与位置一致。</remarks>
    public static ICandidateRepair Clamp(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper)
        => new VectorClampCandidateRepair(CopyAndValidateBounds(lower, upper));

    /// <summary>创建把有限区间外的有限分量镜像回标量区间内的 Repair。</summary>
    /// <remarks>单侧无界、双侧无界或位置为无穷时退化为 Clamp；位置中的 <see cref="double.NaN"/> 保持不变。</remarks>
    public static ICandidateRepair Reflect(double lower, double upper)
    {
        ValidateScalarBounds(lower, upper);
        return new ScalarReflectCandidateRepair(lower, upper);
    }

    /// <summary>创建把有限区间外的有限分量镜像回逐维区间内的 Repair。</summary>
    /// <remarks>两个端点向量在创建时复制且长度必须相等；Repair 时该长度必须与位置一致。</remarks>
    public static ICandidateRepair Reflect(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper)
        => new VectorReflectCandidateRepair(CopyAndValidateBounds(lower, upper));

    /// <summary>创建把标量有限区间外的分量重新均匀采样到区间内的 Repair。</summary>
    /// <remarks>只有双侧有限区间使用随机流；无界端点退化为 Clamp，位置中的 <see cref="double.NaN"/> 保持不变。</remarks>
    public static ICandidateRepair RandomReset(double lower, double upper)
    {
        ValidateScalarBounds(lower, upper);
        return new ScalarRandomResetCandidateRepair(lower, upper);
    }

    /// <summary>创建把逐维有限区间外的分量重新均匀采样到区间内的 Repair。</summary>
    /// <remarks>两个端点向量在创建时复制且长度必须相等；Repair 时该长度必须与位置一致。</remarks>
    public static ICandidateRepair RandomReset(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper)
        => new VectorRandomResetCandidateRepair(CopyAndValidateBounds(lower, upper));

    /// <summary>获取完全不修改位置的 Repair。</summary>
    /// <remarks>这是显式风险选择：Core 不会替它验证位置，调用方必须自行承担越界和非有限位置的后果。</remarks>
    public static ICandidateRepair DoNothing { get; } = new DoNothingCandidateRepair();

    private static void ValidateScalarBounds(double lower, double upper)
    {
        if (double.IsNaN(lower))
        {
            throw new ArgumentOutOfRangeException(nameof(lower), "A boundary cannot be NaN.");
        }

        if (double.IsNaN(upper))
        {
            throw new ArgumentOutOfRangeException(nameof(upper), "A boundary cannot be NaN.");
        }

        if (lower > upper)
        {
            throw new ArgumentException("A lower boundary cannot exceed its corresponding upper boundary.", nameof(lower));
        }
    }

    private static (double[] Lower, double[] Upper) CopyAndValidateBounds(
        ReadOnlySpan<double> lower,
        ReadOnlySpan<double> upper)
    {
        var lowerCopy = lower.ToArray();
        var upperCopy = upper.ToArray();
        ValidateBoundaryValues(lowerCopy, nameof(lower));
        ValidateBoundaryValues(upperCopy, nameof(upper));
        if (lowerCopy.Length != upperCopy.Length)
        {
            throw new ArgumentException("The lower and upper boundary vectors must have the same length.", nameof(upper));
        }

        for (var index = 0; index < lowerCopy.Length; index++)
        {
            if (lowerCopy[index] > upperCopy[index])
            {
                throw new ArgumentException("A lower boundary cannot exceed its corresponding upper boundary.", nameof(lower));
            }
        }

        return (lowerCopy, upperCopy);
    }

    private static void ValidateBoundaryValues(ReadOnlySpan<double> values, string parameterName)
    {
        foreach (var value in values)
        {
            if (double.IsNaN(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "A boundary cannot contain NaN.");
            }
        }
    }

    private static void ValidatePositionLength(Span<double> position, int boundaryLength)
    {
        if (position.Length != boundaryLength)
        {
            throw new ArgumentException("The position length must match every vector boundary length.", nameof(position));
        }
    }

    private static double ClampValue(double value, double lower, double upper)
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

    private static double ReflectValue(double value, double lower, double upper)
    {
        if (double.IsNaN(value) || !double.IsFinite(lower) || !double.IsFinite(upper) || !double.IsFinite(value))
        {
            return ClampValue(value, lower, upper);
        }

        if (value > lower && value < upper)
        {
            return value;
        }

        var width = upper - lower;
        var period = width * 2;
        if (width <= 0 || !double.IsFinite(width) || !double.IsFinite(period))
        {
            return ClampValue(value, lower, upper);
        }

        var offset = value - lower;
        if (!double.IsFinite(offset))
        {
            return ClampValue(value, lower, upper);
        }

        var remainder = offset % period;
        if (remainder < 0)
        {
            remainder += period;
        }

        return remainder <= width ? lower + remainder : upper - (remainder - width);
    }

    private sealed class ScalarClampCandidateRepair(double lower, double upper) : ICandidateRepair
    {
        public void Repair(Span<double> position, Random random)
            => TensorPrimitives.Clamp(position, lower, upper, position);
    }

    private sealed class VectorClampCandidateRepair((double[] Lower, double[] Upper) bounds) : ICandidateRepair
    {
        private readonly double[] _lower = bounds.Lower;
        private readonly double[] _upper = bounds.Upper;

        public void Repair(Span<double> position, Random random)
            => TensorPrimitives.Clamp(position, _lower, _upper, position);
    }

    private sealed class ScalarReflectCandidateRepair(double lower, double upper) : ICandidateRepair
    {
        public void Repair(Span<double> position, Random random)
        {
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = ReflectValue(position[index], lower, upper);
            }
        }
    }

    private sealed class VectorReflectCandidateRepair((double[] Lower, double[] Upper) bounds) : ICandidateRepair
    {
        private readonly double[] _lower = bounds.Lower;
        private readonly double[] _upper = bounds.Upper;

        public void Repair(Span<double> position, Random random)
        {
            ValidatePositionLength(position, _lower.Length);
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = ReflectValue(position[index], _lower[index], _upper[index]);
            }
        }
    }

    private sealed class ScalarRandomResetCandidateRepair(double lower, double upper) : ICandidateRepair
    {
        public void Repair(Span<double> position, Random random)
        {
            ArgumentNullException.ThrowIfNull(random);
            for (var index = 0; index < position.Length; index++)
            {
                var value = position[index];
                if (double.IsNaN(value) || !double.IsFinite(lower) || !double.IsFinite(upper))
                {
                    position[index] = ClampValue(value, lower, upper);
                }
                else if (value < lower || value > upper)
                {
                    var width = upper - lower;
                    position[index] = double.IsFinite(width) ? lower + (width * random.NextDouble()) : ClampValue(value, lower, upper);
                }
            }
        }
    }

    private sealed class VectorRandomResetCandidateRepair((double[] Lower, double[] Upper) bounds) : ICandidateRepair
    {
        private readonly double[] _lower = bounds.Lower;
        private readonly double[] _upper = bounds.Upper;

        public void Repair(Span<double> position, Random random)
        {
            ArgumentNullException.ThrowIfNull(random);
            ValidatePositionLength(position, _lower.Length);
            for (var index = 0; index < position.Length; index++)
            {
                var value = position[index];
                var lower = _lower[index];
                var upper = _upper[index];
                if (double.IsNaN(value) || !double.IsFinite(lower) || !double.IsFinite(upper))
                {
                    position[index] = ClampValue(value, lower, upper);
                }
                else if (value < lower || value > upper)
                {
                    var width = upper - lower;
                    position[index] = double.IsFinite(width) ? lower + (width * random.NextDouble()) : ClampValue(value, lower, upper);
                }
            }
        }
    }

    private sealed class DoNothingCandidateRepair : ICandidateRepair
    {
        public void Repair(Span<double> position, Random random) { }
    }
}
