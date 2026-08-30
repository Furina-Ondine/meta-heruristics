using System.Numerics.Tensors;

namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>创建在算法每次写入位置后就地恢复候选分量的内置策略。</summary>
/// <remarks>
/// 下界和上界可分别为标量或逐维向量；向量在创建时复制，并在 Repair 时要求长度与位置一致。
/// 端点不能为 <see cref="double.NaN"/>，每一维下界不能大于上界；
/// <see cref="double.NegativeInfinity"/> 和 <see cref="double.PositiveInfinity"/> 分别表示无下界和无上界。
/// 所有策略保留位置中的 <see cref="double.NaN"/>，且不承担约束判定职责。
/// </remarks>
public static partial class CandidateRepairs
{
    /// <summary>创建把每个越界分量截到最近标量端点的 Repair。</summary>
    /// <param name="lower">应用于所有分量的包含式下界。</param>
    /// <param name="upper">应用于所有分量的包含式上界。</param>
    /// <returns>不持有 Problem 引用、可重复调用的就地 Repair。</returns>
    /// <exception cref="ArgumentOutOfRangeException">任一端点为 <see cref="double.NaN"/>。</exception>
    /// <exception cref="ArgumentException"><paramref name="lower"/> 大于 <paramref name="upper"/>。</exception>
    public static ICandidateRepair Clamp(double lower, double upper) => new ClampCandidateRepair(lower, upper);
    /// <inheritdoc cref="Clamp(double, double)"/>
    /// <remarks>逐维下界在创建时复制；Repair 时其长度必须与位置一致。</remarks>
    public static ICandidateRepair Clamp(ReadOnlySpan<double> lower, double upper) => new ClampCandidateRepair(lower, upper);
    /// <inheritdoc cref="Clamp(double, double)"/>
    /// <remarks>逐维上界在创建时复制；Repair 时其长度必须与位置一致。</remarks>
    public static ICandidateRepair Clamp(double lower, ReadOnlySpan<double> upper) => new ClampCandidateRepair(lower, upper);
    /// <inheritdoc cref="Clamp(double, double)"/>
    /// <remarks>两个端点向量在创建时复制且长度必须相等；Repair 时该长度必须与位置一致。</remarks>
    public static ICandidateRepair Clamp(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper) => new ClampCandidateRepair(lower, upper);

    /// <summary>创建把有限区间外的有限分量镜像回区间内的 Repair。</summary>
    /// <remarks>单侧无界、双侧无界或位置为无穷时退化为 Clamp；位置中的 <see cref="double.NaN"/> 保持不变。</remarks>
    /// <param name="lower">应用于所有分量的包含式下界。</param>
    /// <param name="upper">应用于所有分量的包含式上界。</param>
    /// <returns>不持有 Problem 引用、可重复调用的就地 Repair。</returns>
    /// <exception cref="ArgumentOutOfRangeException">任一端点为 <see cref="double.NaN"/>。</exception>
    /// <exception cref="ArgumentException"><paramref name="lower"/> 大于 <paramref name="upper"/>。</exception>
    public static ICandidateRepair Reflect(double lower, double upper) => new ReflectCandidateRepair(lower, upper);
    /// <inheritdoc cref="Reflect(double, double)"/>
    /// <remarks>逐维下界在创建时复制；Repair 时其长度必须与位置一致。</remarks>
    public static ICandidateRepair Reflect(ReadOnlySpan<double> lower, double upper) => new ReflectCandidateRepair(lower, upper);
    /// <inheritdoc cref="Reflect(double, double)"/>
    /// <remarks>逐维上界在创建时复制；Repair 时其长度必须与位置一致。</remarks>
    public static ICandidateRepair Reflect(double lower, ReadOnlySpan<double> upper) => new ReflectCandidateRepair(lower, upper);
    /// <inheritdoc cref="Reflect(double, double)"/>
    /// <remarks>两个端点向量在创建时复制且长度必须相等；Repair 时该长度必须与位置一致。</remarks>
    public static ICandidateRepair Reflect(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper) => new ReflectCandidateRepair(lower, upper);

    /// <summary>创建把有限区间外的分量重新均匀采样到区间内的 Repair。</summary>
    /// <remarks>只有双侧有限区间使用随机流；无界端点退化为 Clamp，位置中的 <see cref="double.NaN"/> 保持不变。</remarks>
    /// <param name="lower">应用于所有分量的包含式下界。</param>
    /// <param name="upper">应用于所有分量的包含式上界。</param>
    /// <returns>使用调用方随机流、可由固定 seed 重现的就地 Repair。</returns>
    /// <exception cref="ArgumentOutOfRangeException">任一端点为 <see cref="double.NaN"/>。</exception>
    /// <exception cref="ArgumentException"><paramref name="lower"/> 大于 <paramref name="upper"/>。</exception>
    public static ICandidateRepair RandomReset(double lower, double upper) => new RandomResetCandidateRepair(lower, upper);
    /// <inheritdoc cref="RandomReset(double, double)"/>
    /// <remarks>逐维下界在创建时复制；Repair 时其长度必须与位置一致。</remarks>
    public static ICandidateRepair RandomReset(ReadOnlySpan<double> lower, double upper) => new RandomResetCandidateRepair(lower, upper);
    /// <inheritdoc cref="RandomReset(double, double)"/>
    /// <remarks>逐维上界在创建时复制；Repair 时其长度必须与位置一致。</remarks>
    public static ICandidateRepair RandomReset(double lower, ReadOnlySpan<double> upper) => new RandomResetCandidateRepair(lower, upper);
    /// <inheritdoc cref="RandomReset(double, double)"/>
    /// <remarks>两个端点向量在创建时复制且长度必须相等；Repair 时该长度必须与位置一致。</remarks>
    public static ICandidateRepair RandomReset(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper) => new RandomResetCandidateRepair(lower, upper);

    /// <summary>获取完全不修改位置的 Repair。</summary>
    /// <remarks>这是显式风险选择：Core 不会替它验证位置，调用方必须自行承担越界和非有限位置的后果。</remarks>
    public static ICandidateRepair DoNothing { get; } = new DoNothingCandidateRepair();

    private abstract class BoundedCandidateRepair : ICandidateRepair
    {
        private readonly Boundary _lower;
        private readonly Boundary _upper;

        protected BoundedCandidateRepair(double lower, double upper) : this(Boundary.Create(lower), Boundary.Create(upper)) { }
        protected BoundedCandidateRepair(ReadOnlySpan<double> lower, double upper) : this(Boundary.Create(lower), Boundary.Create(upper)) { }
        protected BoundedCandidateRepair(double lower, ReadOnlySpan<double> upper) : this(Boundary.Create(lower), Boundary.Create(upper)) { }
        protected BoundedCandidateRepair(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper) : this(Boundary.Create(lower), Boundary.Create(upper)) { }

        public abstract void Repair(Span<double> position, Random random);

        protected void ValidatePositionLength(Span<double> position)
        {
            if ((_lower.IsVector && position.Length != _lower.Length)
                || (_upper.IsVector && position.Length != _upper.Length))
            {
                throw new ArgumentException("The position length must match every vector boundary length.", nameof(position));
            }
        }

        protected double GetLower(int index) => _lower.GetValue(index);
        protected double GetUpper(int index) => _upper.GetValue(index);
        protected bool LowerIsVector => _lower.IsVector;
        protected bool UpperIsVector => _upper.IsVector;
        protected int VectorBoundaryLength => _lower.IsVector ? _lower.Length : _upper.Length;
        protected double LowerScalar => _lower.Scalar;
        protected double UpperScalar => _upper.Scalar;
        protected ReadOnlySpan<double> LowerValues => _lower.Values;
        protected ReadOnlySpan<double> UpperValues => _upper.Values;

        protected static double Clamp(double value, double lower, double upper)
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

        private BoundedCandidateRepair(Boundary lower, Boundary upper)
        {
            ValidateBounds(lower, upper);
            _lower = lower;
            _upper = upper;
        }

        private static void ValidateBounds(Boundary lower, Boundary upper)
        {
            if (lower.IsVector && upper.IsVector && lower.Length != upper.Length)
            {
                throw new ArgumentException("The lower and upper boundary vectors must have the same length.", nameof(upper));
            }

            var length = lower.IsVector ? lower.Length : upper.IsVector ? upper.Length : 1;
            for (var index = 0; index < length; index++)
            {
                if (lower.GetValue(index) > upper.GetValue(index))
                {
                    throw new ArgumentException("A lower boundary cannot exceed its corresponding upper boundary.", nameof(lower));
                }
            }
        }

        private readonly struct Boundary
        {
            private readonly double _scalar;
            private readonly double[]? _values;

            private Boundary(double scalar)
            {
                if (double.IsNaN(scalar))
                {
                    throw new ArgumentOutOfRangeException(nameof(scalar), "A boundary cannot be NaN.");
                }

                _scalar = scalar;
            }

            private Boundary(ReadOnlySpan<double> values)
            {
                _values = values.ToArray();
                for (var index = 0; index < _values.Length; index++)
                {
                    if (double.IsNaN(_values[index]))
                    {
                        throw new ArgumentOutOfRangeException(nameof(values), "A boundary cannot contain NaN.");
                    }
                }
            }

            public bool IsVector => _values is not null;
            public int Length => _values?.Length ?? 0;
            public double Scalar => _scalar;
            public ReadOnlySpan<double> Values => _values is null ? [] : _values;
            public double GetValue(int index) => _values is null ? _scalar : _values[index];
            public static Boundary Create(double scalar) => new(scalar);
            public static Boundary Create(ReadOnlySpan<double> values) => new(values);
        }
    }

    private sealed class ClampCandidateRepair : BoundedCandidateRepair
    {
        public ClampCandidateRepair(double lower, double upper) : base(lower, upper) { }
        public ClampCandidateRepair(ReadOnlySpan<double> lower, double upper) : base(lower, upper) { }
        public ClampCandidateRepair(double lower, ReadOnlySpan<double> upper) : base(lower, upper) { }
        public ClampCandidateRepair(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper) : base(lower, upper) { }

        public override void Repair(Span<double> position, Random random)
        {
            ValidatePositionLength(position);
            if (LowerIsVector)
            {
                if (UpperIsVector)
                {
                    TensorPrimitives.Clamp(position, LowerValues, UpperValues, position);
                }
                else
                {
                    TensorPrimitives.Clamp(position, LowerValues, UpperScalar, position);
                }
            }
            else if (UpperIsVector)
            {
                TensorPrimitives.Clamp(position, LowerScalar, UpperValues, position);
            }
            else
            {
                TensorPrimitives.Clamp(position, LowerScalar, UpperScalar, position);
            }
        }
    }

    private sealed partial class ReflectCandidateRepair : BoundedCandidateRepair
    {
        private const double MaxVectorizedRemainderQuotient = 2;
        private readonly double _scalarWidth;
        private readonly double _scalarPeriod;
        private readonly double[]? _vectorWidths;
        private readonly double[]? _vectorPeriods;

        public ReflectCandidateRepair(double lower, double upper) : base(lower, upper)
        {
            _scalarWidth = upper - lower;
            _scalarPeriod = _scalarWidth * 2;
        }

        public ReflectCandidateRepair(ReadOnlySpan<double> lower, double upper) : base(lower, upper)
        {
            (_vectorWidths, _vectorPeriods) = CreateReflectionParameters();
        }

        public ReflectCandidateRepair(double lower, ReadOnlySpan<double> upper) : base(lower, upper)
        {
            (_vectorWidths, _vectorPeriods) = CreateReflectionParameters();
        }

        public ReflectCandidateRepair(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper) : base(lower, upper)
        {
            (_vectorWidths, _vectorPeriods) = CreateReflectionParameters();
        }

        private (double[] Widths, double[] Periods) CreateReflectionParameters()
        {
            var widths = new double[VectorBoundaryLength];
            var periods = new double[widths.Length];
            for (var index = 0; index < widths.Length; index++)
            {
                widths[index] = GetUpper(index) - GetLower(index);
                periods[index] = widths[index] * 2;
            }

            return (widths, periods);
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
    }

    private sealed class RandomResetCandidateRepair : BoundedCandidateRepair
    {
        public RandomResetCandidateRepair(double lower, double upper) : base(lower, upper) { }
        public RandomResetCandidateRepair(ReadOnlySpan<double> lower, double upper) : base(lower, upper) { }
        public RandomResetCandidateRepair(double lower, ReadOnlySpan<double> upper) : base(lower, upper) { }
        public RandomResetCandidateRepair(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper) : base(lower, upper) { }

        public override void Repair(Span<double> position, Random random)
        {
            ArgumentNullException.ThrowIfNull(random);
            ValidatePositionLength(position);
            for (var index = 0; index < position.Length; index++)
            {
                var value = position[index];
                var lower = GetLower(index);
                var upper = GetUpper(index);
                if (double.IsNaN(value) || !double.IsFinite(lower) || !double.IsFinite(upper))
                {
                    position[index] = Clamp(value, lower, upper);
                    continue;
                }

                if (value < lower || value > upper)
                {
                    var width = upper - lower;
                    position[index] = double.IsFinite(width)
                        ? lower + (width * random.NextDouble())
                        : Clamp(value, lower, upper);
                }
            }
        }
    }

    private sealed class DoNothingCandidateRepair : ICandidateRepair
    {
        public void Repair(Span<double> position, Random random) { }
    }
}
