using System;

namespace BrixInvaders.GameLogic;

/// <summary>
/// A small, seedable xorshift64* pseudo-random generator. The whole simulation draws from one instance so a seed
/// and an input script reproduce a game exactly on any machine (it does not depend on <see cref="Random"/>'s
/// implementation, which is allowed to change between runtime versions).
/// </summary>
public sealed class GameRandom
{
    private const ulong ZeroSeedReplacement = 0x9E3779B97F4A7C15UL;
    private ulong _state;

    /// <summary>Creates a generator from a seed. Seed 0 is replaced by a fixed non-zero constant.</summary>
    /// <param name="seed">Any seed.</param>
    public GameRandom(int seed)
    {
        Seed = seed;
        var mixed = SplitMix((ulong)(uint)seed);
        _state = mixed == 0 ? ZeroSeedReplacement : mixed;
    }

    /// <summary>The seed this generator was created with.</summary>
    public int Seed { get; }

    /// <summary>Returns the next raw 64-bit value.</summary>
    /// <returns>A pseudo-random 64-bit value.</returns>
    public ulong NextUInt64()
    {
        _state ^= _state >> 12;
        _state ^= _state << 25;
        _state ^= _state >> 27;
        return _state * 0x2545F4914F6CDD1DUL;
    }

    /// <summary>Returns a value in [0, 1).</summary>
    /// <returns>A double in [0, 1).</returns>
    public double NextDouble() => (NextUInt64() >> 11) * (1.0 / 9007199254740992.0);

    /// <summary>Returns a value in [<paramref name="min"/>, <paramref name="max"/>).</summary>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Exclusive upper bound.</param>
    /// <returns>A double in the range.</returns>
    public double NextDouble(double min, double max) => min + (NextDouble() * (max - min));

    /// <summary>Returns an integer in [0, <paramref name="maxExclusive"/>).</summary>
    /// <param name="maxExclusive">Exclusive upper bound; must be positive.</param>
    /// <returns>An integer in the range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxExclusive"/> is not positive.</exception>
    public int Next(int maxExclusive)
    {
        if (maxExclusive <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive, "Must be positive.");
        }

        return (int)(NextUInt64() % (ulong)maxExclusive);
    }

    /// <summary>Returns an integer in [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
    /// <param name="minInclusive">Inclusive lower bound.</param>
    /// <param name="maxExclusive">Exclusive upper bound; must be greater than the lower bound.</param>
    /// <returns>An integer in the range.</returns>
    public int Next(int minInclusive, int maxExclusive) => minInclusive + Next(maxExclusive - minInclusive);

    /// <summary>Returns true with the given percentage chance.</summary>
    /// <param name="percent">Chance in percent, 0..100.</param>
    /// <returns>True with the given chance.</returns>
    public bool Chance(double percent) => NextDouble() * 100.0 < percent;

    private static ulong SplitMix(ulong value)
    {
        value += 0x9E3779B97F4A7C15UL;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }
}
