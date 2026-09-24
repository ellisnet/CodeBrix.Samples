using System;

namespace BrixInvaders.GameLogic;

/// <summary>Everything needed to start a game.</summary>
public sealed class GameSetup
{
    /// <summary>Number of selectable ship shapes.</summary>
    public const int ShipShapeCount = 3;

    /// <summary>Number of selectable ship colours.</summary>
    public const int ShipColourCount = 4;

    /// <summary>Creates a setup.</summary>
    /// <param name="difficulty">Difficulty level.</param>
    /// <param name="startSector">First sector, 1 or more (a sector unlock lets the player start later).</param>
    /// <param name="seed">RNG seed; the same seed and inputs reproduce the same game.</param>
    /// <param name="shipShape">Cosmetic ship shape, 0..2.</param>
    /// <param name="shipColour">Cosmetic ship colour, 0..3.</param>
    /// <exception cref="ArgumentOutOfRangeException">When a value is out of range.</exception>
    public GameSetup(Difficulty difficulty, int startSector = 1, int seed = 1, int shipShape = 0, int shipColour = 0)
    {
        if (startSector < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(startSector), startSector, "Sectors start at 1.");
        }

        if (shipShape < 0 || shipShape >= ShipShapeCount)
        {
            throw new ArgumentOutOfRangeException(nameof(shipShape), shipShape, "Ship shape must be 0..2.");
        }

        if (shipColour < 0 || shipColour >= ShipColourCount)
        {
            throw new ArgumentOutOfRangeException(nameof(shipColour), shipColour, "Ship colour must be 0..3.");
        }

        Difficulty = difficulty;
        StartSector = startSector;
        Seed = seed;
        ShipShape = shipShape;
        ShipColour = shipColour;
    }

    /// <summary>Difficulty level.</summary>
    public Difficulty Difficulty { get; }

    /// <summary>First sector.</summary>
    public int StartSector { get; }

    /// <summary>RNG seed.</summary>
    public int Seed { get; }

    /// <summary>Cosmetic ship shape, 0..2.</summary>
    public int ShipShape { get; }

    /// <summary>Cosmetic ship colour, 0..3.</summary>
    public int ShipColour { get; }
}
