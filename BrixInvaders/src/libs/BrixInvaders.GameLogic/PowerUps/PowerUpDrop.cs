namespace BrixInvaders.GameLogic;

/// <summary>A power-up drifting down the playfield, waiting to be collected.</summary>
public sealed class PowerUpDrop
{
    /// <summary>Hit-box size (square).</summary>
    public const double Size = 30.0;

    internal PowerUpDrop(int id, PowerUpKind kind, double x, double y)
    {
        Id = id;
        Kind = kind;
        X = x;
        Y = y;
    }

    /// <summary>Unique id within the simulation.</summary>
    public int Id { get; }

    /// <summary>The power-up.</summary>
    public PowerUpKind Kind { get; }

    /// <summary>Centre x.</summary>
    public double X { get; internal set; }

    /// <summary>Centre y.</summary>
    public double Y { get; internal set; }

    /// <summary>Seconds since it dropped (blink it during the last 2 seconds of <see cref="PowerUpSystem.DropLifetime"/>).</summary>
    public double Age { get; internal set; }

    /// <summary>True until collected, timed out or fallen off the bottom.</summary>
    public bool IsAlive { get; internal set; } = true;

    /// <summary>The hit box.</summary>
    public Box Box => Box.FromCenter(X, Y, Size, Size);
}
