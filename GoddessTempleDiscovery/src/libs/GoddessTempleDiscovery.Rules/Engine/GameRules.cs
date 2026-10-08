using System;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>The fixed numbers of the rules (DESIGN.md sections 3 to 7).</summary>
public static class GameRules
{
    /// <summary>The fewest seats.</summary>
    public const int MinSeats = 2;

    /// <summary>The most seats.</summary>
    public const int MaxSeats = 4;

    /// <summary>The fewest turns per team per season.</summary>
    public const int MinTurnsPerSeason = 1;

    /// <summary>The most turns per team per season.</summary>
    public const int MaxTurnsPerSeason = 3;

    /// <summary>The slots of the Site Row.</summary>
    public const int SiteRowSize = 5;

    /// <summary>The slots of the Expedition Row.</summary>
    public const int ExpeditionRowSize = 4;

    /// <summary>The Workers each team starts with.</summary>
    public const int StartingWorkers = 2;

    /// <summary>The Tablets each team starts with.</summary>
    public const int StartingTablets = 1;

    /// <summary>The most Specialists a team may hold.</summary>
    public const int MaxSpecialists = 4;

    /// <summary>The hand limit (Discoveries and Tablets together) checked at the end of a turn.</summary>
    public const int HandLimit = 7;

    /// <summary>The fewest Discoveries in a report published during play.</summary>
    public const int MinReportCards = 3;

    /// <summary>The fewest consecutive periods in a sequence report.</summary>
    public const int MinSequencePeriods = 3;

    /// <summary>The bonus per card of a stratigraphy report.</summary>
    public const int StratigraphyBonusPerCard = 1;

    /// <summary>The bonus per card of a sequence report.</summary>
    public const int SequenceBonusPerCard = 2;

    /// <summary>The Photographer's bonus on every report.</summary>
    public const int PhotographerBonus = 1;

    /// <summary>The PublishBonus season's bonus on every report.</summary>
    public const int SeasonPublishBonus = 1;

    /// <summary>The most Tablets a single Study draws.</summary>
    public const int MaxStudyDraw = 2;

    /// <summary>The die value (or sum) at which Study draws two.</summary>
    public const int StudyDrawsTwoAt = 6;

    /// <summary>The most Workers a dig may use in the WorkersCapped season.</summary>
    public const int CappedWorkersPerDig = 2;

    /// <summary>The bonus of a pending PlusTwoNextDig Favor.</summary>
    public const int FavorDigBonus = 2;

    /// <summary>The bonus for each complete set of the four Tablet kinds.</summary>
    public const int TabletSetBonus = 3;

    /// <summary>The Star of Holy Inanna bonus for the most star-marked Discoveries.</summary>
    public const int StarBonus = 5;

    /// <summary>The Worker each team gains at the end of every season.</summary>
    public const int SeasonEndWorkers = 1;

    private static readonly string[] Ordinals =
    {
        "First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Ninth", "Tenth",
        "Eleventh", "Twelfth", "Thirteenth", "Fourteenth", "Fifteenth", "Sixteenth", "Seventeenth", "Eighteenth",
        "Nineteenth", "Twentieth",
    };

    /// <summary>The Dig Number shift of a difficulty: -1, 0 or +1.</summary>
    /// <param name="difficulty">The difficulty.</param>
    public static int DifficultyShift(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => -1,
        Difficulty.Hard => 1,
        _ => 0,
    };

    /// <summary>The title of a report number, such as "Third Preliminary Report" (numerals past the twentieth).</summary>
    /// <param name="number">The report number, 1 or more.</param>
    public static string ReportTitle(int number)
    {
        if (number < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(number), number, "Reports are numbered from 1.");
        }

        return number <= Ordinals.Length
            ? Ordinals[number - 1] + " Preliminary Report"
            : "Preliminary Report " + number.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// The turns per season the setup pane preselects for a seat count, so the Site deck lasts the twelve seasons and a
    /// game runs about an hour: two seats 3, three or four seats 2 (from simulated games on the full deck of more than
    /// a hundred discoveries; DESIGN decision 9).
    /// </summary>
    /// <param name="seats">The number of seats, 2 to 4.</param>
    public static int DefaultTurnsPerSeason(int seats) => seats <= 2 ? 3 : 2;
}
