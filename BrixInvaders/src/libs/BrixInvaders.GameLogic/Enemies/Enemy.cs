namespace BrixInvaders.GameLogic;

/// <summary>A formation enemy. Read-only for the renderer; the simulation changes it.</summary>
public sealed class Enemy
{
    /// <summary>Hit-box width in world units.</summary>
    public const double Width = 48.0;

    /// <summary>Hit-box height in world units.</summary>
    public const double Height = 40.0;

    internal Enemy(int id, EnemyRole role, EnemyColour colour, int column, int row)
    {
        Id = id;
        Role = role;
        Colour = colour;
        Column = column;
        Row = row;
        MaxHealth = role == EnemyRole.Shielded ? 2 : 1;
        Health = MaxHealth;
        State = EnemyState.InFormation;
    }

    /// <summary>Unique id within the simulation.</summary>
    public int Id { get; }

    /// <summary>The role (ship shape).</summary>
    public EnemyRole Role { get; }

    /// <summary>The colour (point tier).</summary>
    public EnemyColour Colour { get; }

    /// <summary>Formation column, 0 = leftmost.</summary>
    public int Column { get; }

    /// <summary>Formation row, 0 = top.</summary>
    public int Row { get; }

    /// <summary>Centre x.</summary>
    public double X { get; internal set; }

    /// <summary>Centre y.</summary>
    public double Y { get; internal set; }

    /// <summary>Formation / dive state.</summary>
    public EnemyState State { get; internal set; }

    /// <summary>Remaining hits.</summary>
    public int Health { get; internal set; }

    /// <summary>Hits needed from full health (2 for shielded enemies, 1 otherwise).</summary>
    public int MaxHealth { get; }

    /// <summary>True while the enemy has not been destroyed.</summary>
    public bool IsAlive => Health > 0;

    /// <summary>True while a shielded enemy's shield is up (draw the shield sprite).</summary>
    public bool HasShield => Role == EnemyRole.Shielded && Health >= 2;

    /// <summary>The hit box.</summary>
    public Box Box => Box.FromCenter(X, Y, Width, Height);

    internal double FireTimer { get; set; }

    internal double DiveTime { get; set; }

    internal double DiveStartX { get; set; }

    internal double DiveStartY { get; set; }

    internal double DiveTargetX { get; set; }

    internal int DiveSide { get; set; }

    internal bool HasFiredThisDive { get; set; }
}
