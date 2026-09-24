namespace BrixInvaders.GameLogic;

/// <summary>
/// One thing that happened during a simulation step. Which fields are meaningful depends on <see cref="Kind"/>
/// (see the <see cref="GameEventKind"/> member documentation).
/// </summary>
public readonly struct GameEvent
{
    /// <summary>Creates an event.</summary>
    /// <param name="kind">What happened.</param>
    /// <param name="x">World x, when meaningful.</param>
    /// <param name="y">World y, when meaningful.</param>
    /// <param name="points">Points awarded, when meaningful.</param>
    /// <param name="value">Kind-specific integer (wave, phase, lives, ...).</param>
    /// <param name="role">Enemy role, when meaningful.</param>
    /// <param name="colour">Enemy colour, when meaningful.</param>
    /// <param name="powerUp">Power-up kind, when meaningful.</param>
    /// <param name="section">Boss section kind, when meaningful.</param>
    public GameEvent(GameEventKind kind, double x = 0, double y = 0, long points = 0, int value = 0,
        EnemyRole role = EnemyRole.Grunt, EnemyColour colour = EnemyColour.Black,
        PowerUpKind powerUp = PowerUpKind.SpreadShot, BossSectionKind section = BossSectionKind.Core)
    {
        Kind = kind;
        X = x;
        Y = y;
        Points = points;
        Value = value;
        Role = role;
        Colour = colour;
        PowerUp = powerUp;
        Section = section;
    }

    /// <summary>What happened.</summary>
    public GameEventKind Kind { get; }

    /// <summary>World x.</summary>
    public double X { get; }

    /// <summary>World y.</summary>
    public double Y { get; }

    /// <summary>Points awarded (all multipliers applied).</summary>
    public long Points { get; }

    /// <summary>Kind-specific integer.</summary>
    public int Value { get; }

    /// <summary>Enemy role.</summary>
    public EnemyRole Role { get; }

    /// <summary>Enemy colour.</summary>
    public EnemyColour Colour { get; }

    /// <summary>Power-up kind.</summary>
    public PowerUpKind PowerUp { get; }

    /// <summary>Boss section kind.</summary>
    public BossSectionKind Section { get; }

    /// <inheritdoc />
    public override string ToString() => $"{Kind}(x={X:0.#}, y={Y:0.#}, points={Points}, value={Value})";
}
