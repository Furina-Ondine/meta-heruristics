namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>创建由自身持有标量或逐维边界的内置候选 Repair。</summary>
public static class CandidateRepairs
{
    /// <summary>创建使用标量下界和上界的 Clamp Repair。</summary>
    public static ICandidateRepair Clamp(double lower, double upper) => new ClampCandidateRepair(lower, upper);
    /// <summary>创建使用逐维下界和标量上界的 Clamp Repair。</summary>
    public static ICandidateRepair Clamp(ReadOnlySpan<double> lower, double upper) => new ClampCandidateRepair(lower, upper);
    /// <summary>创建使用标量下界和逐维上界的 Clamp Repair。</summary>
    public static ICandidateRepair Clamp(double lower, ReadOnlySpan<double> upper) => new ClampCandidateRepair(lower, upper);
    /// <summary>创建使用逐维下界和上界的 Clamp Repair。</summary>
    public static ICandidateRepair Clamp(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper) => new ClampCandidateRepair(lower, upper);

    /// <summary>创建使用标量下界和上界的 Reflect Repair。</summary>
    public static ICandidateRepair Reflect(double lower, double upper) => new ReflectCandidateRepair(lower, upper);
    /// <summary>创建使用逐维下界和标量上界的 Reflect Repair。</summary>
    public static ICandidateRepair Reflect(ReadOnlySpan<double> lower, double upper) => new ReflectCandidateRepair(lower, upper);
    /// <summary>创建使用标量下界和逐维上界的 Reflect Repair。</summary>
    public static ICandidateRepair Reflect(double lower, ReadOnlySpan<double> upper) => new ReflectCandidateRepair(lower, upper);
    /// <summary>创建使用逐维下界和上界的 Reflect Repair。</summary>
    public static ICandidateRepair Reflect(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper) => new ReflectCandidateRepair(lower, upper);

    /// <summary>创建使用标量下界和上界的 RandomReset Repair。</summary>
    public static ICandidateRepair RandomReset(double lower, double upper) => new RandomResetCandidateRepair(lower, upper);
    /// <summary>创建使用逐维下界和标量上界的 RandomReset Repair。</summary>
    public static ICandidateRepair RandomReset(ReadOnlySpan<double> lower, double upper) => new RandomResetCandidateRepair(lower, upper);
    /// <summary>创建使用标量下界和逐维上界的 RandomReset Repair。</summary>
    public static ICandidateRepair RandomReset(double lower, ReadOnlySpan<double> upper) => new RandomResetCandidateRepair(lower, upper);
    /// <summary>创建使用逐维下界和上界的 RandomReset Repair。</summary>
    public static ICandidateRepair RandomReset(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper) => new RandomResetCandidateRepair(lower, upper);

    /// <summary>获取完全不修改位置的 Repair。</summary>
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
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = Clamp(position[index], GetLower(index), GetUpper(index));
            }
        }
    }

    private sealed class ReflectCandidateRepair : BoundedCandidateRepair
    {
        public ReflectCandidateRepair(double lower, double upper) : base(lower, upper) { }
        public ReflectCandidateRepair(ReadOnlySpan<double> lower, double upper) : base(lower, upper) { }
        public ReflectCandidateRepair(double lower, ReadOnlySpan<double> upper) : base(lower, upper) { }
        public ReflectCandidateRepair(ReadOnlySpan<double> lower, ReadOnlySpan<double> upper) : base(lower, upper) { }

        public override void Repair(Span<double> position, Random random)
        {
            ValidatePositionLength(position);
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = Reflect(position[index], GetLower(index), GetUpper(index));
            }
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
