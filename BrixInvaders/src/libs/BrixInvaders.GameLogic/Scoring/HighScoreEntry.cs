namespace BrixInvaders.GameLogic;

/// <summary>One high-score table row.</summary>
public sealed class HighScoreEntry
{
    /// <summary>Creates an entry.</summary>
    /// <param name="name">Three-letter name (normalised by <see cref="NameEntry.Normalize"/>).</param>
    /// <param name="score">Score.</param>
    /// <param name="sector">Sector reached.</param>
    public HighScoreEntry(string name, long score, int sector)
    {
        Name = NameEntry.Normalize(name);
        Score = score;
        Sector = sector;
    }

    /// <summary>Three-letter name.</summary>
    public string Name { get; }

    /// <summary>Score.</summary>
    public long Score { get; }

    /// <summary>Sector reached.</summary>
    public int Sector { get; }

    /// <inheritdoc />
    public override string ToString() => $"{Name} {Score} (sector {Sector})";
}
