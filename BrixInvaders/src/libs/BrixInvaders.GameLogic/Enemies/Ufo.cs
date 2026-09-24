namespace BrixInvaders.GameLogic;

/// <summary>The bonus UFO crossing the top of the playfield.</summary>
public sealed class Ufo
{
    /// <summary>Hit-box width.</summary>
    public const double Width = 64.0;

    /// <summary>Hit-box height.</summary>
    public const double Height = 28.0;

    /// <summary>Flight lane centre y.</summary>
    public const double LaneY = 76.0;

    /// <summary>Horizontal speed.</summary>
    public const double Speed = 180.0;

    internal Ufo(int id, int direction, int points)
    {
        Id = id;
        Direction = direction;
        Points = points;
        Y = LaneY;
        X = direction > 0 ? -Width : Playfield.Width + Width;
    }

    /// <summary>Unique id within the simulation.</summary>
    public int Id { get; }

    /// <summary>Centre x.</summary>
    public double X { get; internal set; }

    /// <summary>Centre y.</summary>
    public double Y { get; }

    /// <summary>+1 flying right, -1 flying left.</summary>
    public int Direction { get; }

    /// <summary>Base points (before chain and difficulty multipliers).</summary>
    public int Points { get; }

    /// <summary>The hit box.</summary>
    public Box Box => Box.FromCenter(X, Y, Width, Height);

    /// <summary>True once the UFO has crossed the whole playfield.</summary>
    public bool HasEscaped => Direction > 0 ? X > Playfield.Width + Width : X < -Width;
}
