using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>创建在算法每次写入位置后就地恢复候选分量的内置策略。</summary>
/// <remarks>
/// 下界和上界可分别为标量或逐维向量；向量在创建时复制，并在 Repair 时要求长度与位置一致。
/// 端点不能为 <see cref="double.NaN"/>，每一维下界不能大于上界；
/// <see cref="double.NegativeInfinity"/> 和 <see cref="double.PositiveInfinity"/> 分别表示无下界和无上界。
/// 所有策略保留位置中的 <see cref="double.NaN"/>，且不承担约束判定职责。
/// </remarks>
public static class CandidateRepairs
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

    private sealed class ReflectCandidateRepair : BoundedCandidateRepair
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

        public override void Repair(Span<double> position, Random random)
        {
            ValidatePositionLength(position);
            var index = 0;
            if (Vector512.IsHardwareAccelerated)
            {
                while (index <= position.Length - Vector512<double>.Count)
                {
                    RepairVector512(position, index);
                    index += Vector512<double>.Count;
                }
            }

            if (Vector256.IsHardwareAccelerated)
            {
                while (index <= position.Length - Vector256<double>.Count)
                {
                    RepairVector256(position, index);
                    index += Vector256<double>.Count;
                }
            }

            if (Vector128.IsHardwareAccelerated)
            {
                while (index <= position.Length - Vector128<double>.Count)
                {
                    RepairVector128(position, index);
                    index += Vector128<double>.Count;
                }
            }

            for (; index < position.Length; index++)
            {
                position[index] = Reflect(position[index], GetLower(index), GetUpper(index));
            }
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

        private double GetWidth(int index) => _vectorWidths is null ? _scalarWidth : _vectorWidths[index];

        private double GetPeriod(int index) => _vectorPeriods is null ? _scalarPeriod : _vectorPeriods[index];

        private void RepairVector512(Span<double> position, int index)
        {
            ref var positionStart = ref MemoryMarshal.GetReference(position);
            var value = Vector512.LoadUnsafe(ref positionStart, (nuint)index);
            var lower = LoadLower512(index);
            var upper = LoadUpper512(index);
            var result = ReflectVector512(value, lower, upper, LoadWidth512(index), LoadPeriod512(index), out var scalarRepairMask);
            result.StoreUnsafe(ref positionStart, (nuint)index);
            RepairLargeOffsetLanes(position, index, value, lower, upper, scalarRepairMask);
        }

        private void RepairVector256(Span<double> position, int index)
        {
            ref var positionStart = ref MemoryMarshal.GetReference(position);
            var value = Vector256.LoadUnsafe(ref positionStart, (nuint)index);
            var lower = LoadLower256(index);
            var upper = LoadUpper256(index);
            var result = ReflectVector256(value, lower, upper, LoadWidth256(index), LoadPeriod256(index), out var scalarRepairMask);
            result.StoreUnsafe(ref positionStart, (nuint)index);
            RepairLargeOffsetLanes(position, index, value, lower, upper, scalarRepairMask);
        }

        private void RepairVector128(Span<double> position, int index)
        {
            ref var positionStart = ref MemoryMarshal.GetReference(position);
            var value = Vector128.LoadUnsafe(ref positionStart, (nuint)index);
            var lower = LoadLower128(index);
            var upper = LoadUpper128(index);
            var result = ReflectVector128(value, lower, upper, LoadWidth128(index), LoadPeriod128(index), out var scalarRepairMask);
            result.StoreUnsafe(ref positionStart, (nuint)index);
            RepairLargeOffsetLanes(position, index, value, lower, upper, scalarRepairMask);
        }

        private static void RepairLargeOffsetLanes(
            Span<double> position,
            int index,
            Vector512<double> value,
            Vector512<double> lower,
            Vector512<double> upper,
            ulong scalarRepairMask)
        {
            for (var lane = 0; lane < Vector512<double>.Count; lane++)
            {
                if ((scalarRepairMask & (1UL << lane)) != 0)
                {
                    position[index + lane] = Reflect(value.GetElement(lane), lower.GetElement(lane), upper.GetElement(lane));
                }
            }
        }

        private static void RepairLargeOffsetLanes(
            Span<double> position,
            int index,
            Vector256<double> value,
            Vector256<double> lower,
            Vector256<double> upper,
            ulong scalarRepairMask)
        {
            for (var lane = 0; lane < Vector256<double>.Count; lane++)
            {
                if ((scalarRepairMask & (1UL << lane)) != 0)
                {
                    position[index + lane] = Reflect(value.GetElement(lane), lower.GetElement(lane), upper.GetElement(lane));
                }
            }
        }

        private static void RepairLargeOffsetLanes(
            Span<double> position,
            int index,
            Vector128<double> value,
            Vector128<double> lower,
            Vector128<double> upper,
            ulong scalarRepairMask)
        {
            for (var lane = 0; lane < Vector128<double>.Count; lane++)
            {
                if ((scalarRepairMask & (1UL << lane)) != 0)
                {
                    position[index + lane] = Reflect(value.GetElement(lane), lower.GetElement(lane), upper.GetElement(lane));
                }
            }
        }

        private Vector512<double> LoadLower512(int index) => LowerIsVector
            ? Vector512.LoadUnsafe(ref MemoryMarshal.GetReference(LowerValues), (nuint)index)
            : Vector512.Create(LowerScalar);

        private Vector512<double> LoadUpper512(int index) => UpperIsVector
            ? Vector512.LoadUnsafe(ref MemoryMarshal.GetReference(UpperValues), (nuint)index)
            : Vector512.Create(UpperScalar);

        private Vector512<double> LoadWidth512(int index) => _vectorWidths is null
            ? Vector512.Create(_scalarWidth)
            : Vector512.LoadUnsafe(ref MemoryMarshal.GetArrayDataReference(_vectorWidths), (nuint)index);

        private Vector512<double> LoadPeriod512(int index) => _vectorPeriods is null
            ? Vector512.Create(_scalarPeriod)
            : Vector512.LoadUnsafe(ref MemoryMarshal.GetArrayDataReference(_vectorPeriods), (nuint)index);

        private Vector256<double> LoadLower256(int index) => LowerIsVector
            ? Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(LowerValues), (nuint)index)
            : Vector256.Create(LowerScalar);

        private Vector256<double> LoadUpper256(int index) => UpperIsVector
            ? Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(UpperValues), (nuint)index)
            : Vector256.Create(UpperScalar);

        private Vector256<double> LoadWidth256(int index) => _vectorWidths is null
            ? Vector256.Create(_scalarWidth)
            : Vector256.LoadUnsafe(ref MemoryMarshal.GetArrayDataReference(_vectorWidths), (nuint)index);

        private Vector256<double> LoadPeriod256(int index) => _vectorPeriods is null
            ? Vector256.Create(_scalarPeriod)
            : Vector256.LoadUnsafe(ref MemoryMarshal.GetArrayDataReference(_vectorPeriods), (nuint)index);

        private Vector128<double> LoadLower128(int index) => LowerIsVector
            ? Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(LowerValues), (nuint)index)
            : Vector128.Create(LowerScalar);

        private Vector128<double> LoadUpper128(int index) => UpperIsVector
            ? Vector128.LoadUnsafe(ref MemoryMarshal.GetReference(UpperValues), (nuint)index)
            : Vector128.Create(UpperScalar);

        private Vector128<double> LoadWidth128(int index) => _vectorWidths is null
            ? Vector128.Create(_scalarWidth)
            : Vector128.LoadUnsafe(ref MemoryMarshal.GetArrayDataReference(_vectorWidths), (nuint)index);

        private Vector128<double> LoadPeriod128(int index) => _vectorPeriods is null
            ? Vector128.Create(_scalarPeriod)
            : Vector128.LoadUnsafe(ref MemoryMarshal.GetArrayDataReference(_vectorPeriods), (nuint)index);

        private static Vector512<double> ReflectVector512(
            Vector512<double> value,
            Vector512<double> lower,
            Vector512<double> upper,
            Vector512<double> width,
            Vector512<double> period,
            out ulong scalarRepairMask)
        {
            var zero = Vector512<double>.Zero;
            var clamped = Vector512.ConditionalSelect(
                Vector512.LessThan(value, lower),
                lower,
                Vector512.ConditionalSelect(Vector512.GreaterThan(value, upper), upper, value));
            var offset = value - lower;
            var finite = Vector512.BitwiseAnd(Vector512.IsFinite(value), Vector512.IsFinite(lower));
            finite = Vector512.BitwiseAnd(finite, Vector512.IsFinite(upper));
            finite = Vector512.BitwiseAnd(finite, Vector512.IsFinite(width));
            finite = Vector512.BitwiseAnd(finite, Vector512.IsFinite(period));
            finite = Vector512.BitwiseAnd(finite, Vector512.IsFinite(offset));
            var canReflect = Vector512.BitwiseAnd(finite, Vector512.GreaterThan(width, zero));
            var requiresScalarRepair = Vector512.BitwiseAnd(
                canReflect,
                Vector512.GreaterThan(Vector512.Abs(offset), period * Vector512.Create(MaxVectorizedRemainderQuotient)));
            scalarRepairMask = Vector512.ExtractMostSignificantBits(requiresScalarRepair);
            var quotient = Vector512.Truncate(Vector512.Divide(offset, period));
            var remainder = offset - (period * quotient);
            remainder = Vector512.ConditionalSelect(Vector512.LessThan(remainder, zero), remainder + period, remainder);
            var reflected = Vector512.ConditionalSelect(
                Vector512.LessThanOrEqual(remainder, width),
                lower + remainder,
                upper - (remainder - width));
            var result = Vector512.ConditionalSelect(canReflect, reflected, clamped);
            var keepOriginal = Vector512.BitwiseOr(
                Vector512.BitwiseAnd(Vector512.GreaterThan(value, lower), Vector512.LessThan(value, upper)),
                Vector512.BitwiseOr(Vector512.Equals(value, lower), Vector512.Equals(value, upper)));
            return Vector512.ConditionalSelect(keepOriginal, value, result);
        }

        private static Vector256<double> ReflectVector256(
            Vector256<double> value,
            Vector256<double> lower,
            Vector256<double> upper,
            Vector256<double> width,
            Vector256<double> period,
            out ulong scalarRepairMask)
        {
            var zero = Vector256<double>.Zero;
            var clamped = Vector256.ConditionalSelect(
                Vector256.LessThan(value, lower),
                lower,
                Vector256.ConditionalSelect(Vector256.GreaterThan(value, upper), upper, value));
            var offset = value - lower;
            var finite = Vector256.BitwiseAnd(Vector256.IsFinite(value), Vector256.IsFinite(lower));
            finite = Vector256.BitwiseAnd(finite, Vector256.IsFinite(upper));
            finite = Vector256.BitwiseAnd(finite, Vector256.IsFinite(width));
            finite = Vector256.BitwiseAnd(finite, Vector256.IsFinite(period));
            finite = Vector256.BitwiseAnd(finite, Vector256.IsFinite(offset));
            var canReflect = Vector256.BitwiseAnd(finite, Vector256.GreaterThan(width, zero));
            var requiresScalarRepair = Vector256.BitwiseAnd(
                canReflect,
                Vector256.GreaterThan(Vector256.Abs(offset), period * Vector256.Create(MaxVectorizedRemainderQuotient)));
            scalarRepairMask = Vector256.ExtractMostSignificantBits(requiresScalarRepair);
            var quotient = Vector256.Truncate(Vector256.Divide(offset, period));
            var remainder = offset - (period * quotient);
            remainder = Vector256.ConditionalSelect(Vector256.LessThan(remainder, zero), remainder + period, remainder);
            var reflected = Vector256.ConditionalSelect(
                Vector256.LessThanOrEqual(remainder, width),
                lower + remainder,
                upper - (remainder - width));
            var result = Vector256.ConditionalSelect(canReflect, reflected, clamped);
            var keepOriginal = Vector256.BitwiseOr(
                Vector256.BitwiseAnd(Vector256.GreaterThan(value, lower), Vector256.LessThan(value, upper)),
                Vector256.BitwiseOr(Vector256.Equals(value, lower), Vector256.Equals(value, upper)));
            return Vector256.ConditionalSelect(keepOriginal, value, result);
        }

        private static Vector128<double> ReflectVector128(
            Vector128<double> value,
            Vector128<double> lower,
            Vector128<double> upper,
            Vector128<double> width,
            Vector128<double> period,
            out ulong scalarRepairMask)
        {
            var zero = Vector128<double>.Zero;
            var clamped = Vector128.ConditionalSelect(
                Vector128.LessThan(value, lower),
                lower,
                Vector128.ConditionalSelect(Vector128.GreaterThan(value, upper), upper, value));
            var offset = value - lower;
            var finite = Vector128.BitwiseAnd(Vector128.IsFinite(value), Vector128.IsFinite(lower));
            finite = Vector128.BitwiseAnd(finite, Vector128.IsFinite(upper));
            finite = Vector128.BitwiseAnd(finite, Vector128.IsFinite(width));
            finite = Vector128.BitwiseAnd(finite, Vector128.IsFinite(period));
            finite = Vector128.BitwiseAnd(finite, Vector128.IsFinite(offset));
            var canReflect = Vector128.BitwiseAnd(finite, Vector128.GreaterThan(width, zero));
            var requiresScalarRepair = Vector128.BitwiseAnd(
                canReflect,
                Vector128.GreaterThan(Vector128.Abs(offset), period * Vector128.Create(MaxVectorizedRemainderQuotient)));
            scalarRepairMask = Vector128.ExtractMostSignificantBits(requiresScalarRepair);
            var quotient = Vector128.Truncate(Vector128.Divide(offset, period));
            var remainder = offset - (period * quotient);
            remainder = Vector128.ConditionalSelect(Vector128.LessThan(remainder, zero), remainder + period, remainder);
            var reflected = Vector128.ConditionalSelect(
                Vector128.LessThanOrEqual(remainder, width),
                lower + remainder,
                upper - (remainder - width));
            var result = Vector128.ConditionalSelect(canReflect, reflected, clamped);
            var keepOriginal = Vector128.BitwiseOr(
                Vector128.BitwiseAnd(Vector128.GreaterThan(value, lower), Vector128.LessThan(value, upper)),
                Vector128.BitwiseOr(Vector128.Equals(value, lower), Vector128.Equals(value, upper)));
            return Vector128.ConditionalSelect(keepOriginal, value, result);
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
