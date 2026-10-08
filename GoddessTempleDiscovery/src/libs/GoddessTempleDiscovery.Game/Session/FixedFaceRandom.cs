using System;

namespace GoddessTempleDiscovery.Game.Session;

/// <summary>
/// A <see cref="Random"/> that always answers one face index. The add-on's <c>Die.Roll(Random)</c> takes its result
/// from <c>random.Next(faceCount)</c>, so rolling a die with this instance sets its logical face to the engine's value
/// while the table's tumbling animation, started by <c>CardsAndDiceTable.Roll()</c>, runs on unchanged.
/// </summary>
public sealed class FixedFaceRandom : Random
{
    private readonly int _index;

    /// <summary>Creates the source.</summary>
    /// <param name="index">The face index every draw returns.</param>
    public FixedFaceRandom(int index)
    {
        _index = Math.Max(0, index);
    }

    /// <inheritdoc />
    public override int Next(int maxValue) => maxValue <= 0 ? 0 : Math.Min(_index, maxValue - 1);

    /// <inheritdoc />
    public override int Next(int minValue, int maxValue) => maxValue <= minValue ? minValue : Math.Clamp(minValue + _index, minValue, maxValue - 1);

    /// <inheritdoc />
    public override int Next() => _index;
}
