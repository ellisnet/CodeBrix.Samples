using System;
using System.Collections.Generic;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>
/// A small, seedable xorshift64* generator. The engine draws every shuffle and every die from one instance, so a seed
/// and an action sequence reproduce a game exactly on any machine and runtime.
/// </summary>
internal sealed class GameRandom
{
    private const ulong ZeroSeedReplacement = 0x9E3779B97F4A7C15UL;

    internal GameRandom(int seed)
    {
        var mixed = SplitMix((ulong)(uint)seed);
        State = mixed == 0 ? ZeroSeedReplacement : mixed;
    }

    internal ulong State { get; private set; }

    internal ulong NextUInt64()
    {
        var x = State;
        x ^= x >> 12;
        x ^= x << 25;
        x ^= x >> 27;
        State = x;
        return x * 0x2545F4914F6CDD1DUL;
    }

    internal int Next(int maxExclusive)
    {
        if (maxExclusive <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive, "Must be positive.");
        }

        return (int)(NextUInt64() % (ulong)maxExclusive);
    }

    internal int RollDie() => Next(6) + 1;

    internal void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static ulong SplitMix(ulong x)
    {
        x += 0x9E3779B97F4A7C15UL;
        x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
        x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
        return x ^ (x >> 31);
    }
}
