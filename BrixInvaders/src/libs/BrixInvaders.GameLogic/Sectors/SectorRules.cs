using System;

namespace BrixInvaders.GameLogic;

/// <summary>
/// What each sector is. Sectors are numbered from 1 without limit; sectors 1..5 are the five designs and after
/// sector 5 the designs repeat ("loop" 1, 2, ...) with tighter numbers while the sector counter keeps climbing.
/// </summary>
public static class SectorRules
{
    /// <summary>Number of distinct sector designs before the loop restarts.</summary>
    public const int SectorsPerLoop = 5;

    /// <summary>Waves per sector (followed by the boss).</summary>
    public const int WavesPerSector = 6;

    private static readonly string[] Names =
    {
        "Outer Picket",
        "Raider Lanes",
        "Aegis Belt",
        "Missile Reach",
        "Mothership Gate",
    };

    private static readonly string[] Briefings =
    {
        "The invasion fleet's picket line. Watch the top of the screen for the bonus UFO.",
        "Raiders break away from the formation, swoop at you and return to their slots.",
        "Shielded ships take two hits. Meteor showers sweep the sector between waves.",
        "Missile carriers launch homing missiles. Shoot them down or out-turn them.",
        "Everything at once - and the formation splits into two groups.",
    };

    /// <summary>The design (1..5) a sector uses.</summary>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <returns>The design, 1..5.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="sector"/> is below 1.</exception>
    public static int DesignOf(int sector)
    {
        CheckSector(sector);
        return ((sector - 1) % SectorsPerLoop) + 1;
    }

    /// <summary>The loop a sector belongs to: 0 for sectors 1..5, 1 for 6..10, and so on.</summary>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <returns>The loop index.</returns>
    public static int LoopOf(int sector)
    {
        CheckSector(sector);
        return (sector - 1) / SectorsPerLoop;
    }

    /// <summary>The features a design introduces (exactly one flag, except design 3 which introduces two).</summary>
    /// <param name="design">Design 1..5.</param>
    /// <returns>The newly introduced features.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the design is not 1..5.</exception>
    public static SectorFeatures IntroducedBy(int design) => design switch
    {
        1 => SectorFeatures.Ufo,
        2 => SectorFeatures.Divers,
        3 => SectorFeatures.Shielded | SectorFeatures.MeteorShowers,
        4 => SectorFeatures.HomingMissiles,
        5 => SectorFeatures.SplitFormation,
        _ => throw new ArgumentOutOfRangeException(nameof(design), design, "Design must be 1..5."),
    };

    /// <summary>All features active in a sector (cumulative over the designs up to its own).</summary>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <returns>The active features.</returns>
    public static SectorFeatures FeaturesOf(int sector)
    {
        var design = DesignOf(sector);
        var features = SectorFeatures.None;
        for (var d = 1; d <= design; d++)
        {
            features |= IntroducedBy(d);
        }

        return features;
    }

    /// <summary>The sector's display name, e.g. "Raider Lanes" (loops add " II", " III", ...).</summary>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <returns>The name.</returns>
    public static string NameOf(int sector)
    {
        var name = Names[DesignOf(sector) - 1];
        var loop = LoopOf(sector);
        return loop == 0 ? name : name + " " + RomanNumeral(loop + 1);
    }

    /// <summary>The one-line briefing text describing the sector's new behaviour.</summary>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <returns>The briefing text.</returns>
    public static string BriefingOf(int sector)
    {
        var text = Briefings[DesignOf(sector) - 1];
        return LoopOf(sector) == 0 ? text : text + " The fleet is faster and fires tighter now.";
    }

    /// <summary>Multiplier for every enemy interval (fire, formation step, dives, missiles): 0.85 per loop.</summary>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <returns>The scale, 1 in the first loop.</returns>
    public static double IntervalScale(int sector) => Math.Pow(0.85, LoopOf(sector));

    /// <summary>Multiplier for enemy bolt and missile speeds: +10% per loop.</summary>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <returns>The scale, 1 in the first loop.</returns>
    public static double SpeedScale(int sector) => 1.0 + (0.1 * LoopOf(sector));

    /// <summary>Multiplier for boss health: +50% per loop.</summary>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <returns>The scale, 1 in the first loop.</returns>
    public static double BossHealthScale(int sector) => 1.0 + (0.5 * LoopOf(sector));

    /// <summary>Extra enemy bolts allowed on screen: +2 per loop.</summary>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <returns>The extra bolt allowance.</returns>
    public static int ExtraEnemyBolts(int sector) => 2 * LoopOf(sector);

    internal static string RomanNumeral(int value)
    {
        string[] numerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        var result = string.Empty;
        for (var i = 0; i < values.Length; i++)
        {
            while (value >= values[i])
            {
                result += numerals[i];
                value -= values[i];
            }
        }

        return result;
    }

    private static void CheckSector(int sector)
    {
        if (sector < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sector), sector, "Sectors start at 1.");
        }
    }
}
