using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

[assembly: SimdTemplate("double", SimdCapabilities.FloatingPoint)]

namespace Anastasya.Metaheuristics.Core.Problems;

public static partial class CandidateRepairs
{
    private sealed partial class ReflectCandidateRepair
    {
        public override void Repair(Span<double> position, Random random)
        {
            ValidatePositionLength(position);
            var index = 0;
            __SimdExpandHardwareAcceleratedWidths(() =>
            {
                while (index <= position.Length - __Vector<double>.Count)
                {
                    RepairVector__Width(position, index);
                    index += __Vector<double>.Count;
                }
            });

            for (; index < position.Length; index++)
            {
                position[index] = Reflect(position[index], GetLower(index), GetUpper(index));
            }
        }

        private void RepairVector__Width(Span<double> position, int index)
        {
            __SimdExpandWidths(() =>
            {
                ref var positionStart = ref MemoryMarshal.GetReference(position);
                var value = __Vector.LoadUnsafe(ref positionStart, (nuint)index);
                var lower = LoadLower__Width(index);
                var upper = LoadUpper__Width(index);
                var result = ReflectVector__Width(
                    value,
                    lower,
                    upper,
                    LoadWidth__Width(index),
                    LoadPeriod__Width(index),
                    out var scalarRepairMask);
                result.StoreUnsafe(ref positionStart, (nuint)index);
                RepairLargeOffsetLanes(position, index, value, lower, upper, scalarRepairMask);
            });
        }

        private static void RepairLargeOffsetLanes(
            Span<double> position,
            int index,
            __Vector<double> value,
            __Vector<double> lower,
            __Vector<double> upper,
            ulong scalarRepairMask)
        {
            __SimdExpandWidths(() =>
            {
                for (var lane = 0; lane < __Vector<double>.Count; lane++)
                {
                    if ((scalarRepairMask & (1UL << lane)) != 0)
                    {
                        position[index + lane] = Reflect(
                            value.GetElement(lane),
                            lower.GetElement(lane),
                            upper.GetElement(lane));
                    }
                }
            });
        }

        private __Vector<double> LoadLower__Width(int index)
        {
            __SimdExpandWidths(() =>
            {
                return LowerIsVector
                    ? __Vector.LoadUnsafe(ref MemoryMarshal.GetReference(LowerValues), (nuint)index)
                    : __Vector.Create(LowerScalar);
            });
        }

        private __Vector<double> LoadUpper__Width(int index)
        {
            __SimdExpandWidths(() =>
            {
                return UpperIsVector
                    ? __Vector.LoadUnsafe(ref MemoryMarshal.GetReference(UpperValues), (nuint)index)
                    : __Vector.Create(UpperScalar);
            });
        }

        private __Vector<double> LoadWidth__Width(int index)
        {
            __SimdExpandWidths(() =>
            {
                return _vectorWidths is null
                    ? __Vector.Create(_scalarWidth)
                    : __Vector.LoadUnsafe(ref MemoryMarshal.GetArrayDataReference(_vectorWidths), (nuint)index);
            });
        }

        private __Vector<double> LoadPeriod__Width(int index)
        {
            __SimdExpandWidths(() =>
            {
                return _vectorPeriods is null
                    ? __Vector.Create(_scalarPeriod)
                    : __Vector.LoadUnsafe(ref MemoryMarshal.GetArrayDataReference(_vectorPeriods), (nuint)index);
            });
        }

        private static __Vector<double> ReflectVector__Width(
            __Vector<double> value,
            __Vector<double> lower,
            __Vector<double> upper,
            __Vector<double> width,
            __Vector<double> period,
            out ulong scalarRepairMask)
        {
            __SimdExpandWidths(() =>
            {
                var zero = __Vector<double>.Zero;
                var clamped = __Vector.ConditionalSelect(
                    __Vector.LessThan(value, lower),
                    lower,
                    __Vector.ConditionalSelect(__Vector.GreaterThan(value, upper), upper, value));
                var offset = value - lower;
                var finite = __Vector.BitwiseAnd(__Vector.IsFinite(value), __Vector.IsFinite(lower));
                finite = __Vector.BitwiseAnd(finite, __Vector.IsFinite(upper));
                finite = __Vector.BitwiseAnd(finite, __Vector.IsFinite(width));
                finite = __Vector.BitwiseAnd(finite, __Vector.IsFinite(period));
                finite = __Vector.BitwiseAnd(finite, __Vector.IsFinite(offset));
                var canReflect = __Vector.BitwiseAnd(finite, __Vector.GreaterThan(width, zero));
                var requiresScalarRepair = __Vector.BitwiseAnd(
                    canReflect,
                    __Vector.GreaterThan(
                        __Vector.Abs(offset),
                        period * __Vector.Create(MaxVectorizedRemainderQuotient)));
                scalarRepairMask = __Vector.ExtractMostSignificantBits(requiresScalarRepair);
                var quotient = __Vector.Truncate(__Vector.Divide(offset, period));
                var remainder = offset - (period * quotient);
                remainder = __Vector.ConditionalSelect(
                    __Vector.LessThan(remainder, zero),
                    remainder + period,
                    remainder);
                var reflected = __Vector.ConditionalSelect(
                    __Vector.LessThanOrEqual(remainder, width),
                    lower + remainder,
                    upper - (remainder - width));
                var result = __Vector.ConditionalSelect(canReflect, reflected, clamped);
                var keepOriginal = __Vector.BitwiseOr(
                    __Vector.BitwiseAnd(__Vector.GreaterThan(value, lower), __Vector.LessThan(value, upper)),
                    __Vector.BitwiseOr(__Vector.Equals(value, lower), __Vector.Equals(value, upper)));
                return __Vector.ConditionalSelect(keepOriginal, value, result);
            });
        }
    }
}
