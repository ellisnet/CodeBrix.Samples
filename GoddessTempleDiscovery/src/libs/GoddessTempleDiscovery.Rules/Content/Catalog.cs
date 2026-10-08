using System.Collections.Generic;
using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Content;

/// <summary>
/// Every card and every text in the game, as data. The partial files beside this one each hold one deck; this
/// file holds nothing but the doorway. The content is documented: every entry carries its sources.
/// </summary>
public static partial class Catalog
{
    /// <summary>The Site deck: one card per real discovery, from both content passes.</summary>
    public static IReadOnlyList<DiscoveryCard> Discoveries => AllDiscoveries.Value;

    private static readonly System.Lazy<IReadOnlyList<DiscoveryCard>> AllDiscoveries =
        new(() => System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(DiscoveryCards, MoreDiscoveryCards)));

    /// <summary>The Tablet deck.</summary>
    public static IReadOnlyList<TabletCard> Tablets => TabletCards;

    /// <summary>The Expedition deck: the six roles, each repeated as the design says.</summary>
    public static IReadOnlyList<SpecialistCard> Specialists => SpecialistCards;

    /// <summary>The twelve seasons, in order.</summary>
    public static IReadOnlyList<SeasonCard> Seasons => SeasonCards;

    /// <summary>The Favor deck.</summary>
    public static IReadOnlyList<FavorCard> Favors => FavorCards;

    /// <summary>The fictional teams.</summary>
    public static IReadOnlyList<TeamProfile> Teams => TeamProfiles;

    /// <summary>The real people of the excavations.</summary>
    public static IReadOnlyList<PersonCard> People => PersonCards;

    /// <summary>The periods of the timeline, in order.</summary>
    public static IReadOnlyList<PeriodInfo> Periods => PeriodInfos;

    /// <summary>The glossary.</summary>
    public static IReadOnlyList<GlossaryEntry> Glossary => GlossaryEntries;

    /// <summary>Quotations for loading lines.</summary>
    public static IReadOnlyList<Quotation> Quotations => QuotationList;

    /// <summary>The prologue shown before the first season (title, then paragraphs).</summary>
    public static IReadOnlyList<string> Prologue => PrologueParagraphs;

    /// <summary>The epilogue shown after the last season (title, then paragraphs).</summary>
    public static IReadOnlyList<string> Epilogue => EpilogueParagraphs;
}
