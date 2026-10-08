using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Rules.Journal;

/// <summary>Turns cards into Field Journal entries, carrying every text of the card and its sources.</summary>
public static class JournalWriter
{
    /// <summary>The entry for a Season card.</summary>
    /// <param name="season">The season.</param>
    /// <returns>The entry.</returns>
    public static JournalEntry ForSeason(SeasonCard season)
    {
        ArgumentNullException.ThrowIfNull(season);
        return new JournalEntry(
            JournalEntryKind.Season,
            Join(" - ", season.Year, season.Title),
            Paragraphs(
                season.Story,
                season.LongStory,
                Labelled("Director", season.Director),
                Labelled("Effect", season.EffectText)),
            season.ArtKey ?? string.Empty,
            season.Year ?? string.Empty,
            null,
            season.Sources ?? string.Empty);
    }

    /// <summary>The entry for an excavated Discovery.</summary>
    /// <param name="card">The discovery.</param>
    /// <param name="seasonYear">The season it was excavated in.</param>
    /// <param name="teamName">The team that excavated it.</param>
    /// <returns>The entry.</returns>
    public static JournalEntry ForDiscovery(DiscoveryCard card, string seasonYear, string teamName)
    {
        ArgumentNullException.ThrowIfNull(card);
        var depth = card.Tier >= DepthTiers.Shallowest && card.Tier <= DepthTiers.Deepest
            ? string.Format(CultureInfo.InvariantCulture, "Tier {0} ({1}), Dig Number {2}, {3} points{4}.",
                card.Tier, DepthTiers.Name(card.Tier), card.DigNumber, card.Points, card.IsStarred ? ", one of Her stars" : string.Empty)
            : string.Empty;
        return new JournalEntry(
            JournalEntryKind.Discovery,
            card.Title ?? card.Id,
            Paragraphs(
                card.CardText,
                card.LongText,
                Cuneiform(card.Cuneiform, card.CuneiformReading),
                Join(", ", card.Level, card.ApproximateDate),
                depth,
                Labelled("Excavated by", card.ExcavatedBy),
                Labelled("Season found", card.SeasonFound),
                Labelled("Where it is now", card.WhereNow)),
            card.ArtKey ?? string.Empty,
            seasonYear ?? string.Empty,
            teamName,
            card.Sources ?? string.Empty);
    }

    /// <summary>The entry for a Tablet drawn.</summary>
    /// <param name="card">The tablet.</param>
    /// <param name="seasonYear">The season it was drawn in.</param>
    /// <param name="teamName">The team that drew it.</param>
    /// <returns>The entry.</returns>
    public static JournalEntry ForTablet(TabletCard card, string seasonYear, string teamName)
    {
        ArgumentNullException.ThrowIfNull(card);
        var quote = string.IsNullOrWhiteSpace(card.Quote)
            ? string.Empty
            : string.IsNullOrWhiteSpace(card.QuoteAttribution) ? card.Quote : card.Quote + " - " + card.QuoteAttribution;
        return new JournalEntry(
            JournalEntryKind.Tablet,
            card.Title ?? card.Id,
            Paragraphs(
                Labelled("Kind", card.Kind.ToString()),
                card.CardText,
                card.LongText,
                Cuneiform(card.Cuneiform, card.CuneiformReading),
                quote,
                Labelled("Say it", card.Pronunciation)),
            card.ArtKey ?? string.Empty,
            seasonYear ?? string.Empty,
            teamName,
            card.Sources ?? string.Empty);
    }

    /// <summary>The entry for a Favor drawn.</summary>
    /// <param name="card">The favor.</param>
    /// <param name="seasonYear">The season it was drawn in.</param>
    /// <param name="teamName">The team that drew it.</param>
    /// <returns>The entry.</returns>
    public static JournalEntry ForFavor(FavorCard card, string seasonYear, string teamName)
    {
        ArgumentNullException.ThrowIfNull(card);
        return new JournalEntry(
            JournalEntryKind.Favor,
            card.Title ?? card.Id,
            Paragraphs(card.Fact, Labelled("The gift", card.EffectText)),
            card.ArtKey ?? string.Empty,
            seasonYear ?? string.Empty,
            teamName,
            card.Sources ?? string.Empty);
    }

    /// <summary>The entry for a Specialist recruited.</summary>
    /// <param name="card">The specialist.</param>
    /// <param name="seasonYear">The season it was recruited in.</param>
    /// <param name="teamName">The team that recruited it.</param>
    /// <returns>The entry.</returns>
    public static JournalEntry ForSpecialist(SpecialistCard card, string seasonYear, string teamName)
    {
        ArgumentNullException.ThrowIfNull(card);
        return new JournalEntry(
            JournalEntryKind.Specialist,
            card.Title ?? card.Role.ToString(),
            Paragraphs(card.CardText, card.HistoricalNote),
            card.ArtKey ?? string.Empty,
            seasonYear ?? string.Empty,
            teamName,
            card.Sources ?? string.Empty);
    }

    /// <summary>The entry for a published report.</summary>
    /// <param name="report">The report.</param>
    /// <param name="teamName">The team that published it.</param>
    /// <returns>The entry.</returns>
    public static JournalEntry ForReport(Report report, string teamName)
    {
        ArgumentNullException.ThrowIfNull(report);
        var kind = report.Kind switch
        {
            ReportKind.Stratigraphy => "A stratigraphy report",
            ReportKind.Sequence => "A sequence report",
            _ => "A report",
        };
        var summary = string.Format(
            CultureInfo.InvariantCulture,
            "{0} of {1} {2}: {3}. It scored {4} points.",
            kind,
            report.Cards.Count,
            report.Cards.Count == 1 ? "find" : "finds",
            string.Join(", ", report.Cards.Select(c => c.Title ?? c.Id)),
            report.Points);
        var sources = string.Join("; ", report.Cards
            .Select(c => c.Sources)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.Ordinal));
        return new JournalEntry(
            JournalEntryKind.Report,
            (teamName == null ? string.Empty : teamName + ": ") + report.Title,
            summary,
            string.Empty,
            report.SeasonYear ?? string.Empty,
            teamName,
            sources);
    }

    private static string Cuneiform(string signs, string reading) =>
        string.IsNullOrWhiteSpace(signs) ? string.Empty : Join(" - ", signs, reading);

    private static string Labelled(string label, string value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : label + ": " + value;

    private static string Join(string separator, params string[] parts) =>
        string.Join(separator, parts.Where(p => !string.IsNullOrWhiteSpace(p)));

    private static string Paragraphs(params string[] parts) => Join("\n\n", parts);
}
