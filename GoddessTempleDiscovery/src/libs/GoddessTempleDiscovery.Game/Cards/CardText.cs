using System;
using System.Globalization;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Content;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Game.Cards;

/// <summary>The short labels the faces, the HUD and the inspector share: period names, kind names, tier lines.</summary>
public static class CardText
{
    /// <summary>The display name of a period (from the catalog's timeline, else the enum name spaced out).</summary>
    /// <param name="period">The period.</param>
    /// <returns>The name.</returns>
    public static string PeriodName(Period period)
    {
        var info = (Catalog.Periods ?? Array.Empty<PeriodInfo>()).FirstOrDefault(p => p != null && p.Period == period);
        if (info != null && !string.IsNullOrWhiteSpace(info.Name))
        {
            return info.Name;
        }

        var name = period.ToString();
        var spaced = new System.Text.StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
            {
                spaced.Append(' ');
            }

            spaced.Append(name[i]);
        }

        return spaced.ToString();
    }

    /// <summary>
    /// A short display name of a period for narrow labels (the Site Row slots), such as "Kassite" or
    /// "Isin II and Assyria"; <see cref="PeriodName"/> stays the full name.
    /// </summary>
    /// <param name="period">The period.</param>
    /// <returns>The short name.</returns>
    public static string PeriodShortName(Period period) => period switch
    {
        Period.Ubaid => "Ubaid",
        Period.UbaidTransition => "Ubaid transition",
        Period.EarlyUruk => "Early Uruk",
        Period.MiddleUruk => "Middle Uruk",
        Period.LateUruk => "Late Uruk",
        Period.JemdetNasr => "Jemdet Nasr",
        Period.EarlyDynastic => "Early Dynastic",
        Period.Akkadian => "Akkadian",
        Period.UrIII => "Ur III",
        Period.OldBabylonian => "Old Babylonian",
        Period.Kassite => "Kassite",
        Period.IsinIIAndAssyria => "Isin II and Assyria",
        Period.NeoBabylonian => "Neo-Babylonian",
        Period.Achaemenid => "Achaemenid",
        Period.Seleucid => "Seleucid",
        Period.Parthian => "Parthian",
        Period.Sassanian => "Sassanian",
        Period.ArabConquest => "Arab conquest",
        Period.EuphratesShifts => "The river moves west",
        _ => PeriodName(period),
    };

    /// <summary>The kind of a tablet as the band prints it, such as "Goddess".</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The label.</returns>
    public static string TabletKindName(TabletKind kind) => kind switch
    {
        TabletKind.Goddess => "Goddess",
        TabletKind.Pantheon => "Pantheon",
        TabletKind.Culture => "Culture",
        _ => "Timeline",
    };

    /// <summary>The title of a specialist role, such as "Small-Finds Keeper".</summary>
    /// <param name="role">The role.</param>
    /// <returns>The label.</returns>
    public static string RoleName(SpecialistRole role) => role switch
    {
        SpecialistRole.SmallFindsKeeper => "Small-Finds Keeper",
        _ => role.ToString(),
    };

    /// <summary>The kebab-case form of a role, for keys.</summary>
    /// <param name="role">The role.</param>
    /// <returns>The key part, such as "small-finds-keeper".</returns>
    public static string RoleKey(SpecialistRole role) => RoleName(role).ToLowerInvariant().Replace(' ', '-');

    /// <summary>The first sentence of a text (the newspaper sub-head when the content gives none).</summary>
    /// <param name="text">The text.</param>
    /// <returns>The sentence, or empty.</returns>
    public static string FirstSentence(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var trimmed = text.Trim();
        for (var i = 0; i < trimmed.Length; i++)
        {
            if ((trimmed[i] == '.' || trimmed[i] == '!' || trimmed[i] == '?')
                && (i == trimmed.Length - 1 || char.IsWhiteSpace(trimmed[i + 1])))
            {
                return trimmed.Substring(0, i + 1);
            }
        }

        return trimmed;
    }

    /// <summary>The text after the first sentence (may be empty).</summary>
    /// <param name="text">The text.</param>
    /// <returns>The rest.</returns>
    public static string AfterFirstSentence(string text)
    {
        var first = FirstSentence(text);
        return string.IsNullOrEmpty(first) ? string.Empty : text.Trim().Substring(first.Length).Trim();
    }

    /// <summary>The newspaper headline of a discovery: the content's own, else the catalog's, else one from the title.</summary>
    /// <param name="card">The discovery.</param>
    /// <returns>The headline and sub-head.</returns>
    public static Catalog.HeadlinePair Headline(DiscoveryCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        if (!string.IsNullOrWhiteSpace(card.Headline))
        {
            return new Catalog.HeadlinePair(card.Headline, string.IsNullOrWhiteSpace(card.SubHead) ? FirstSentence(card.CardText) : card.SubHead);
        }

        var pair = Catalog.HeadlineFor(card.Id, ShortTitle(card.Title));
        return new Catalog.HeadlinePair(pair.Headline, string.IsNullOrWhiteSpace(pair.SubHead) ? FirstSentence(card.CardText) : pair.SubHead);
    }

    /// <summary>The front-page headline of a season.</summary>
    /// <param name="season">The season.</param>
    /// <returns>The headline and sub-head.</returns>
    public static Catalog.HeadlinePair Headline(SeasonCard season)
    {
        ArgumentNullException.ThrowIfNull(season);
        if (!string.IsNullOrWhiteSpace(season.Headline))
        {
            return new Catalog.HeadlinePair(season.Headline, season.SubHead ?? string.Empty);
        }

        var pair = Catalog.HeadlineFor(season.Year, season.Title);
        return new Catalog.HeadlinePair(pair.Headline, string.IsNullOrWhiteSpace(pair.SubHead) ? FirstSentence(season.Story) : pair.SubHead);
    }

    /// <summary>The season banner line in headline voice, such as "1930/31: THE DEEP TRENCH GOES DOWN".</summary>
    /// <param name="season">The season.</param>
    /// <returns>The line.</returns>
    public static string BannerLine(SeasonCard season) =>
        season == null ? string.Empty : season.Year + ": " + Headline(season).Headline;

    /// <summary>The season banner line of the edition a game prints (see <see cref="Editions"/>).</summary>
    /// <param name="season">The season.</param>
    /// <param name="seed">The game's seed.</param>
    /// <returns>The line.</returns>
    public static string BannerLine(SeasonCard season, int seed) =>
        season == null ? string.Empty : season.Year + ": " + Editions.For(seed, season).Headline;

    /// <summary>The final season's effect in the players' words, as the rules now play it (no automatic publishing).</summary>
    public const string FinalSeasonEffectText =
        "The Mask of Warka site enters the Site Row if it has not appeared yet. This is the final season: publish what you can; what stays in the crates scores half.";

    /// <summary>
    /// A season's effect as the HUD, the faces and the inspector print it. The final season's text comes from
    /// <see cref="FinalSeasonEffectText"/>: the engine no longer publishes the crates at the end, and the
    /// presentation says so whatever the content's line still reads.
    /// </summary>
    /// <param name="season">The season.</param>
    /// <returns>The effect line.</returns>
    public static string EffectLine(SeasonCard season) =>
        season == null ? string.Empty : season.Effect == SeasonEffect.FinalSeason ? FinalSeasonEffectText : season.EffectText ?? string.Empty;

    /// <summary>The date line of a season, such as "WARKA, IRAQ — WINTER 1930/31".</summary>
    /// <param name="seasonYear">The season year.</param>
    /// <returns>The line.</returns>
    public static string Dateline(string seasonYear) =>
        "WARKA, IRAQ — WINTER " + (string.IsNullOrWhiteSpace(seasonYear) ? "1912/13" : seasonYear);

    /// <summary>A title without the German name in parentheses (for headlines made from titles).</summary>
    /// <param name="title">The title.</param>
    /// <returns>The short title.</returns>
    public static string ShortTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        var open = title.IndexOf(" (", StringComparison.Ordinal);
        return open > 0 ? title.Substring(0, open) : title;
    }

    /// <summary>The slot label lines under a Site Row card: layer, period, Dig Number, points, Her star.</summary>
    /// <param name="card">The discovery in the slot.</param>
    /// <param name="digNumber">The Dig Number after difficulty and the season.</param>
    /// <returns>The label.</returns>
    public static SlotLabel SlotLabelFor(DiscoveryCard card, int digNumber)
    {
        ArgumentNullException.ThrowIfNull(card);
        return new SlotLabel(
            DepthTiers.Name(card.Tier),
            PeriodName(card.Period),
            digNumber,
            card.Points,
            card.IsStarred)
        {
            ShortLayer = DepthTiers.ShortName(card.Tier),
            ShortPeriod = PeriodShortName(card.Period),
        };
    }

    /// <summary>A number as invariant text.</summary>
    /// <param name="value">The number.</param>
    /// <returns>The text.</returns>
    public static string N(int value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// The Dig Number of a Site Row slot for the current team, difficulty and season, as the engine's
    /// <see cref="GameEngine.PreviewDig"/> reports it (the presentation never works a rule out itself).
    /// </summary>
    /// <param name="engine">The game.</param>
    /// <param name="slot">The Site Row slot.</param>
    /// <returns>The number to reach, or 0 for an empty slot.</returns>
    public static int SlotDigNumber(GameEngine engine, int slot)
    {
        ArgumentNullException.ThrowIfNull(engine);
        return engine.PreviewDig(slot, DiceChoice.DieA, 0, null).Needed;
    }
}

/// <summary>The label drawn under one Site Row slot.</summary>
/// <param name="Layer">The layer name of the tier.</param>
/// <param name="Period">The period name.</param>
/// <param name="DigNumber">The Dig Number to reach.</param>
/// <param name="Points">The points it is worth.</param>
/// <param name="IsStarred">True for one of Her stars.</param>
public sealed record SlotLabel(string Layer, string Period, int DigNumber, int Points, bool IsStarred)
{
    /// <summary>The tier's short name for the slot (<see cref="DepthTiers.ShortName"/>), such as "KASSITE / OLD BAB.".</summary>
    public string ShortLayer { get; init; } = string.Empty;

    /// <summary>The period's short name for the slot (<see cref="CardText.PeriodShortName"/>), such as "Kassite".</summary>
    public string ShortPeriod { get; init; } = string.Empty;
}
