namespace GoddessTempleDiscovery.Game.Settings;

/// <summary>One seat of the last setup, as it is stored (JSON) under <see cref="SettingsService.LastSeatsKey"/>.</summary>
public sealed class SeatRecord
{
    /// <summary>The team name typed or chosen.</summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>The id of the chosen team profile, or empty.</summary>
    public string TeamProfileId { get; set; } = string.Empty;

    /// <summary>True for a computer seat.</summary>
    public bool IsComputer { get; set; }

    /// <summary>The computer temperament's name (Surveyor, DeepDigger or Scholar).</summary>
    public string Temperament { get; set; } = "Surveyor";
}
