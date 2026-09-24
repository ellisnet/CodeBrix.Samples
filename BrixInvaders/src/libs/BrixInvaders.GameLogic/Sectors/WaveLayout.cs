using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>The formation of one wave: columns, the role of each row (top row first), start height, split flag.</summary>
public sealed class WaveLayout
{
    internal WaveLayout(int sector, int wave, int columns, IReadOnlyList<EnemyRole> rowRoles, int startY, bool split)
    {
        Sector = sector;
        Wave = wave;
        Columns = columns;
        RowRoles = rowRoles;
        StartY = startY;
        IsSplit = split;
    }

    /// <summary>Sector number.</summary>
    public int Sector { get; }

    /// <summary>Wave number, 1..6.</summary>
    public int Wave { get; }

    /// <summary>Number of columns.</summary>
    public int Columns { get; }

    /// <summary>Number of rows.</summary>
    public int Rows => RowRoles.Count;

    /// <summary>The enemy role of every row, top row first.</summary>
    public IReadOnlyList<EnemyRole> RowRoles { get; }

    /// <summary>Centre y of the top row when the wave starts.</summary>
    public int StartY { get; }

    /// <summary>True when the formation is two independently stepping groups.</summary>
    public bool IsSplit { get; }

    /// <summary>Total number of enemies in the wave.</summary>
    public int EnemyCount => Columns * Rows;
}
