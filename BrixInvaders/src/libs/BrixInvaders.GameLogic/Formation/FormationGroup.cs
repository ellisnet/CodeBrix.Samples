namespace BrixInvaders.GameLogic;

/// <summary>
/// One independently stepping block of formation columns. A normal wave has one group spanning the playfield; a
/// split wave (sector design 5) has two, each confined to its half of the playfield. Positions are integers.
/// </summary>
public sealed class FormationGroup
{
    internal FormationGroup(int firstColumn, int lastColumn, int originX, int originY, int direction, int minX, int maxX)
    {
        FirstColumn = firstColumn;
        LastColumn = lastColumn;
        OriginX = originX;
        OriginY = originY;
        Direction = direction;
        MinX = minX;
        MaxX = maxX;
    }

    /// <summary>First formation column in this group.</summary>
    public int FirstColumn { get; }

    /// <summary>Last formation column in this group.</summary>
    public int LastColumn { get; }

    /// <summary>Centre x of the group's first column.</summary>
    public int OriginX { get; internal set; }

    /// <summary>Centre y of the top row.</summary>
    public int OriginY { get; internal set; }

    /// <summary>+1 stepping right, -1 stepping left.</summary>
    public int Direction { get; internal set; }

    /// <summary>Leftmost x any enemy edge of this group may reach.</summary>
    public int MinX { get; }

    /// <summary>Rightmost x any enemy edge of this group may reach.</summary>
    public int MaxX { get; }

    /// <summary>True when the column belongs to this group.</summary>
    /// <param name="column">The column.</param>
    /// <returns>True when it belongs here.</returns>
    public bool Contains(int column) => column >= FirstColumn && column <= LastColumn;
}
