using System.Numerics;
using System.Runtime.CompilerServices;

namespace Anastasya.Metaheuristics.Benchmarks.References;

/// <summary>
/// 基准参考 R：播种、状态隔离与排放规则和候选一致，但每个 lane 用标量状态推进。
/// </summary>
/// <remarks>
/// R 用来回答“同样的语义换成标量实现有多快”，因此不额外增加分配、虚调用或逐样本拷贝。
/// 该类型只属于基准程序集，不是运行时代码。
/// </remarks>
internal sealed class RandomSourceScalarReference
{
    private const ulong SplitMixIncrement = 0x9E3779B97F4A7C15UL;
    private const ulong SplitMixMultiplier1 = 0xBF58476D1CE4E5B9UL;
    private const ulong SplitMixMultiplier2 = 0x94D049BB133111EBUL;
    private const double InverseTwoToThePower53 = 1.0 / 9007199254740992.0;

    private readonly ulong[] _state0;
    private readonly ulong[] _state1;
    private readonly ulong[] _state2;
    private readonly ulong[] _state3;
    private readonly int _laneCount;

    internal RandomSourceScalarReference(ulong seed)
    {
        _laneCount = Vector<ulong>.Count;
        _state0 = new ulong[_laneCount];
        _state1 = new ulong[_laneCount];
        _state2 = new ulong[_laneCount];
        _state3 = new ulong[_laneCount];

        var state = seed;
        var state0 = NextSplitMix64(ref state);
        var state1 = NextSplitMix64(ref state);
        var state2 = NextSplitMix64(ref state);
        var state3 = NextSplitMix64(ref state);
        if ((state0 | state1 | state2 | state3) == 0)
        {
            state3 = 1;
        }

        for (var lane = 0; lane < _laneCount; lane++)
        {
            Jump(ref state0, ref state1, ref state2, ref state3);
            _state0[lane] = state0;
            _state1[lane] = state1;
            _state2[lane] = state2;
            _state3[lane] = state3;
        }
    }

    internal Vector<ulong> NextULongVector()
    {
        Span<ulong> round = stackalloc ulong[_laneCount];
        NextRound(round);
        return new Vector<ulong>(round);
    }

    internal Vector<double> NextDoubleVector()
    {
        Span<ulong> round = stackalloc ulong[_laneCount];
        NextRound(round);
        var raw = new Vector<ulong>(round);
        return Vector.ConvertToDouble(raw >> 11) * new Vector<double>(InverseTwoToThePower53);
    }

    internal void Fill(Span<ulong> destination)
    {
        if (destination.IsEmpty)
        {
            return;
        }

        Span<ulong> round = stackalloc ulong[_laneCount];
        var index = 0;
        while (index < destination.Length)
        {
            NextRound(round);
            for (var lane = 0; lane < _laneCount && index < destination.Length; lane++)
            {
                destination[index++] = round[lane];
            }
        }
    }

    internal void Fill(Span<double> destination)
    {
        if (destination.IsEmpty)
        {
            return;
        }

        Span<ulong> round = stackalloc ulong[_laneCount];
        var index = 0;
        while (index < destination.Length)
        {
            NextRound(round);
            for (var lane = 0; lane < _laneCount && index < destination.Length; lane++)
            {
                destination[index++] = (round[lane] >> 11) * InverseTwoToThePower53;
            }
        }
    }

    internal void Fill(Span<double> destination, double minimum, double maximum)
    {
        if (destination.IsEmpty)
        {
            return;
        }

        Span<ulong> round = stackalloc ulong[_laneCount];
        var index = 0;
        while (index < destination.Length)
        {
            NextRound(round);
            for (var lane = 0; lane < _laneCount && index < destination.Length; lane++)
            {
                destination[index++] = Scale((round[lane] >> 11) * InverseTwoToThePower53, minimum, maximum);
            }
        }
    }

    internal void Fill(Span<int> destination, int minimum, int maximum)
    {
        if (destination.IsEmpty)
        {
            return;
        }

        var range = (ulong)((long)maximum - minimum);
        var threshold = unchecked((0UL - range) % range);
        Span<ulong> round = stackalloc ulong[_laneCount];
        var index = 0;
        while (index < destination.Length)
        {
            NextRound(round);
            for (var lane = 0; lane < _laneCount && index < destination.Length; lane++)
            {
                var raw = round[lane];
                ulong offset;
                if (range == 1)
                {
                    offset = 0;
                }
                else if ((range & (range - 1)) == 0)
                {
                    offset = raw >> (64 - BitOperations.TrailingZeroCount(range));
                }
                else
                {
                    var productHigh = Math.BigMul(raw, range, out var productLow);
                    if (productLow < threshold)
                    {
                        continue;
                    }

                    offset = productHigh;
                }

                destination[index++] = (int)((long)minimum + (long)offset);
            }
        }
    }

    private void NextRound(Span<ulong> round)
    {
        for (var lane = 0; lane < _laneCount; lane++)
        {
            round[lane] = NextRaw(
                ref _state0[lane],
                ref _state1[lane],
                ref _state2[lane],
                ref _state3[lane]);
        }
    }

    private static double Scale(double unit, double minimum, double maximum)
    {
        var width = maximum - minimum;
        var value = double.IsFinite(width)
            ? minimum + (width * unit)
            : (minimum * (1 - unit)) + (maximum * unit);
        if (value < minimum)
        {
            return minimum;
        }

        return value >= maximum ? Math.BitDecrement(maximum) : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong NextRaw(
        ref ulong state0,
        ref ulong state1,
        ref ulong state2,
        ref ulong state3)
    {
        var result = BitOperations.RotateLeft(state0 + state3, 23) + state0;
        var temporary = state1 << 17;

        state2 ^= state0;
        state3 ^= state1;
        state1 ^= state2;
        state0 ^= state3;
        state2 ^= temporary;
        state3 = BitOperations.RotateLeft(state3, 45);

        return result;
    }

    private static void Jump(ref ulong state0, ref ulong state1, ref ulong state2, ref ulong state3)
    {
        var jumped0 = 0UL;
        var jumped1 = 0UL;
        var jumped2 = 0UL;
        var jumped3 = 0UL;
        ApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumped0, ref jumped1, ref jumped2, ref jumped3, 0x180EC6D33CFD0ABAUL);
        ApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumped0, ref jumped1, ref jumped2, ref jumped3, 0xD5A61266F0C9392CUL);
        ApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumped0, ref jumped1, ref jumped2, ref jumped3, 0xA9582618E03FC9AAUL);
        ApplyJump(ref state0, ref state1, ref state2, ref state3,
            ref jumped0, ref jumped1, ref jumped2, ref jumped3, 0x39ABDC4529B1661CUL);

        state0 = jumped0;
        state1 = jumped1;
        state2 = jumped2;
        state3 = jumped3;
    }

    private static void ApplyJump(
        ref ulong state0,
        ref ulong state1,
        ref ulong state2,
        ref ulong state3,
        ref ulong jumped0,
        ref ulong jumped1,
        ref ulong jumped2,
        ref ulong jumped3,
        ulong polynomial)
    {
        for (var bit = 0; bit < 64; bit++)
        {
            if ((polynomial & (1UL << bit)) != 0)
            {
                jumped0 ^= state0;
                jumped1 ^= state1;
                jumped2 ^= state2;
                jumped3 ^= state3;
            }

            NextRaw(ref state0, ref state1, ref state2, ref state3);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong NextSplitMix64(ref ulong state)
    {
        state = unchecked(state + SplitMixIncrement);
        var value = state;
        value = unchecked((value ^ (value >> 30)) * SplitMixMultiplier1);
        value = unchecked((value ^ (value >> 27)) * SplitMixMultiplier2);
        return value ^ (value >> 31);
    }
}
