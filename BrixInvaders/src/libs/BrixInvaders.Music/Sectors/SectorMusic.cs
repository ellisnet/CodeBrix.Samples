using System;
using System.Collections.Generic;

namespace BrixInvaders.Music;

/// <summary>
/// The music table: which preset each generator plays in each sector design, at what tempo, and what the boss
/// follow-up switches to. Sector 6 plays design 1 again, sector 7 design 2, and so on for endless play; sector 0 is
/// the title screen. DESIGN.md section 15 documents the table and the reasons for each choice.
/// </summary>
public static class SectorMusic
{
    /// <summary>The sector number that means the title screen.</summary>
    public const int TitleSector = 0;

    /// <summary>How many sector designs there are before the table repeats.</summary>
    public const int DesignCount = 5;

    /// <summary>The slowest tempo the table uses or accepts.</summary>
    public const double MinimumBeatsPerMinute = 60;

    /// <summary>The fastest tempo the table uses or accepts.</summary>
    public const double MaximumBeatsPerMinute = 180;

    /// <summary>The title screen's music. It has no boss of its own; its boss presets repeat its own presets.</summary>
    public static SectorMusicEntry Title { get; } = new(
        TitleSector, "Title", "calm and spacious - waiting in orbit before the invasion",
        "AmbientElectronica", "WaltzDuetInAMinor", 108,
        "AmbientElectronica", "WaltzDuetInAMinor", 108);

    private static readonly SectorMusicEntry[] _designs =
    [
        new(1, "Outer Picket", "steady and bright - the first patrol along the picket line",
            "FourOnTheFloor", "HornpipeInG", 120,
            "ClubArrangement", "ReelInGMinor", 128),
        new(2, "Raider Lanes", "restless and swooping - raiders break from the formation",
            "ClubArrangement", "JigInD", 126,
            "FourOnTheFloor", "ReelInGMinor", 134),
        new(3, "Aegis Belt", "wide and eerie - drifting rock and shielded ships",
            "AmbientElectronica", "AirInDMixolydian", 104,
            "ClubArrangement", "ReelInGMinor", 112),
        new(4, "Missile Reach", "tense and minor - hunted by homing missiles",
            "ClubArrangement", "WaltzInAMinor", 132,
            "FourOnTheFloor", "ReelInGMinor", 140),
        new(5, "Mothership Gate", "grand and driving - everything at once at the gate",
            "FourOnTheFloor", "DuetInC", 138,
            "ClubArrangement", "ReelInGMinor", 146),
    ];

    /// <summary>The five sector designs, design 1 first.</summary>
    public static IReadOnlyList<SectorMusicEntry> Designs { get; } = Array.AsReadOnly(_designs);

    /// <summary>
    /// The sector design a sector number plays: 0 for the title, 1..5 for sectors 1..5, then wrapping (6 -> 1,
    /// 10 -> 5, 11 -> 1, ...).
    /// </summary>
    /// <param name="sector">The sector number (0 = the title screen).</param>
    /// <returns>The design, 0..5.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The sector is negative.</exception>
    public static int DesignFor(int sector)
    {
        if (sector < 0) { throw new ArgumentOutOfRangeException(nameof(sector), sector, "A sector is 0 (the title screen) or more."); }
        return sector == TitleSector ? TitleSector : (sector - 1) % DesignCount + 1;
    }

    /// <summary>The music entry for a sector number (0 = the title screen; 6 and on wrap to designs 1..5).</summary>
    /// <param name="sector">The sector number.</param>
    /// <returns>The entry.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The sector is negative.</exception>
    public static SectorMusicEntry For(int sector)
    {
        var design = DesignFor(sector);
        return design == TitleSector ? Title : _designs[design - 1];
    }
}
