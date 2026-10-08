using System.Collections.Generic;
using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>The decks a game is dealt from. <see cref="CatalogSnapshot.FromCatalog"/> reads the game's own content.</summary>
public interface ICardCatalog
{
    /// <summary>The Site deck's cards.</summary>
    IReadOnlyList<DiscoveryCard> Discoveries { get; }

    /// <summary>The Tablet deck's cards.</summary>
    IReadOnlyList<TabletCard> Tablets { get; }

    /// <summary>The Expedition deck's cards (copies included).</summary>
    IReadOnlyList<SpecialistCard> Specialists { get; }

    /// <summary>The seasons, in play order.</summary>
    IReadOnlyList<SeasonCard> Seasons { get; }

    /// <summary>The Favor deck's cards.</summary>
    IReadOnlyList<FavorCard> Favors { get; }

    /// <summary>The team profiles seats may choose.</summary>
    IReadOnlyList<TeamProfile> Teams { get; }

    /// <summary>
    /// The id of the discovery the FinalSeason effect brings into the Site Row if it has not appeared (the Mask of
    /// Warka), or null for none.
    /// </summary>
    string FinalSeasonDiscoveryId { get; }
}
