namespace BrixInvaders.Game.Settings;

/// <summary>One stored high-score row, the JSON shape of the per-difficulty tables (<c>{ "Name", "Score", "Sector" }</c>).</summary>
public sealed class HighScoreRecord
{
    /// <summary>The three-character name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The final score.</summary>
    public long Score { get; set; }

    /// <summary>The sector the game ended in.</summary>
    public int Sector { get; set; }
}
