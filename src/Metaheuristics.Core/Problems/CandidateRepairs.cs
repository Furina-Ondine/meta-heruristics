namespace Anastasya.Metaheuristics.Core.Problems;

/// <summary>创建由自身持有变量边界的内置候选 Repair。</summary>
public static class CandidateRepairs
{
    /// <summary>创建将有界维度截断到端点的 Repair。</summary>
    /// <param name="bounds">每个位置分量的边界；创建时复制。</param>
    /// <remarks><see cref="double.NaN"/> 保持不变；无界维度不处理。</remarks>
    public static ICandidateRepair Clamp(IReadOnlyList<VariableBounds> bounds) => new ClampCandidateRepair(bounds);

    /// <summary>创建对双侧有限边界执行镜像映射的 Repair。</summary>
    /// <param name="bounds">每个位置分量的边界；创建时复制。</param>
    /// <remarks>单侧边界和无穷值退化为截断；<see cref="double.NaN"/> 与无界维度保持不变。</remarks>
    public static ICandidateRepair Reflect(IReadOnlyList<VariableBounds> bounds) => new ReflectCandidateRepair(bounds);

    /// <summary>创建对双侧有限边界随机回退的 Repair。</summary>
    /// <param name="bounds">每个位置分量的边界；创建时复制。</param>
    /// <remarks>单侧边界退化为截断；<see cref="double.NaN"/> 与无界维度保持不变。</remarks>
    public static ICandidateRepair RandomReset(IReadOnlyList<VariableBounds> bounds) => new RandomResetCandidateRepair(bounds);

    /// <summary>获取完全不修改位置的 Repair。</summary>
    /// <remarks>除非调用方能自行保证位置正确性，否则不要使用；它不处理越界值、无穷值或 <see cref="double.NaN"/>。</remarks>
    public static ICandidateRepair DoNothing { get; } = new DoNothingCandidateRepair();

    private abstract class BoundedCandidateRepair(IReadOnlyList<VariableBounds> bounds) : ICandidateRepair
    {
        protected VariableBounds[] Bounds { get; } = CopyBounds(bounds);

        public abstract void Repair(Span<double> position, Random random);

        protected void ValidateDimension(Span<double> position)
        {
            if (position.Length != Bounds.Length)
            {
                throw new ArgumentException("The position length must match the repair boundary dimension.", nameof(position));
            }
        }

        protected static double Clamp(double value, VariableBounds bounds)
        {
            if (double.IsNaN(value))
            {
                return value;
            }

            if (bounds.LowerBound is { } lowerBound && value < lowerBound)
            {
                return lowerBound;
            }

            return bounds.UpperBound is { } upperBound && value > upperBound
                ? upperBound
                : value;
        }

        private static VariableBounds[] CopyBounds(IReadOnlyList<VariableBounds> bounds)
        {
            ArgumentNullException.ThrowIfNull(bounds);
            var copy = new VariableBounds[bounds.Count];
            for (var index = 0; index < copy.Length; index++)
            {
                copy[index] = bounds[index];
            }

            return copy;
        }
    }

    private sealed class ClampCandidateRepair(IReadOnlyList<VariableBounds> bounds) : BoundedCandidateRepair(bounds)
    {
        public override void Repair(Span<double> position, Random random)
        {
            ValidateDimension(position);
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = Clamp(position[index], Bounds[index]);
            }
        }
    }

    private sealed class ReflectCandidateRepair(IReadOnlyList<VariableBounds> bounds) : BoundedCandidateRepair(bounds)
    {
        public override void Repair(Span<double> position, Random random)
        {
            ValidateDimension(position);
            for (var index = 0; index < position.Length; index++)
            {
                position[index] = Reflect(position[index], Bounds[index]);
            }
        }

        private static double Reflect(double value, VariableBounds bounds)
        {
            if (double.IsNaN(value) || bounds.LowerBound is not { } lowerBound || bounds.UpperBound is not { } upperBound)
            {
                return Clamp(value, bounds);
            }

            if (double.IsFinite(value) && value > lowerBound && value < upperBound)
            {
                return value;
            }

            if (!double.IsFinite(value))
            {
                return Clamp(value, bounds);
            }

            var width = upperBound - lowerBound;
            var period = width * 2;
            if (width <= 0 || !double.IsFinite(width) || !double.IsFinite(period))
            {
                return Clamp(value, bounds);
            }

            var offset = value - lowerBound;
            if (!double.IsFinite(offset))
            {
                return Clamp(value, bounds);
            }

            var remainder = offset % period;
            if (remainder < 0)
            {
                remainder += period;
            }

            return remainder <= width
                ? lowerBound + remainder
                : upperBound - (remainder - width);
        }
    }

    private sealed class RandomResetCandidateRepair(IReadOnlyList<VariableBounds> bounds) : BoundedCandidateRepair(bounds)
    {
        public override void Repair(Span<double> position, Random random)
        {
            ArgumentNullException.ThrowIfNull(random);
            ValidateDimension(position);
            for (var index = 0; index < position.Length; index++)
            {
                var value = position[index];
                var bounds = Bounds[index];
                if (double.IsNaN(value) || bounds.LowerBound is not { } lowerBound || bounds.UpperBound is not { } upperBound)
                {
                    position[index] = Clamp(value, bounds);
                    continue;
                }

                if (value < lowerBound || value > upperBound)
                {
                    var width = upperBound - lowerBound;
                    position[index] = double.IsFinite(width)
                        ? lowerBound + (width * random.NextDouble())
                        : Clamp(value, bounds);
                }
            }
        }
    }

    private sealed class DoNothingCandidateRepair : ICandidateRepair
    {
        public void Repair(Span<double> position, Random random)
        {
        }
    }
}
