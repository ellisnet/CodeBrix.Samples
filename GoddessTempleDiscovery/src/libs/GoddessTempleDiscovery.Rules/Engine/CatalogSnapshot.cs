using System;
using System.Collections.Generic;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Content;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>An immutable copy of a set of decks; the engine's default reads the static <see cref="Catalog"/>.</summary>
public sealed class CatalogSnapshot : ICardCatalog
{
    /// <summary>The id the static catalog's Mask of Warka is looked up by first.</summary>
    public const string MaskOfWarkaKey = "mask-of-warka";

    /// <summary>Creates a snapshot of the given decks (each list is copied; null means empty).</summary>
    /// <param name="discoveries">The Site deck.</param>
    /// <param name="tablets">The Tablet deck.</param>
    /// <param name="specialists">The Expedition deck.</param>
    /// <param name="seasons">The seasons in play order.</param>
    /// <param name="favors">The Favor deck.</param>
    /// <param name="teams">The team profiles.</param>
    /// <param name="finalSeasonDiscoveryId">The id of the discovery the FinalSeason effect brings into the row, or null.</param>
    public CatalogSnapshot(
        IEnumerable<DiscoveryCard> discoveries,
        IEnumerable<TabletCard> tablets,
        IEnumerable<SpecialistCard> specialists,
        IEnumerable<SeasonCard> seasons,
        IEnumerable<FavorCard> favors,
        IEnumerable<TeamProfile> teams,
        string finalSeasonDiscoveryId = null)
    {
        Discoveries = Copy(discoveries);
        Tablets = Copy(tablets);
        Specialists = Copy(specialists);
        Seasons = Copy(seasons);
        Favors = Copy(favors);
        Teams = Copy(teams);
        FinalSeasonDiscoveryId = finalSeasonDiscoveryId;
    }

    /// <inheritdoc />
    public IReadOnlyList<DiscoveryCard> Discoveries { get; }

    /// <inheritdoc />
    public IReadOnlyList<TabletCard> Tablets { get; }

    /// <inheritdoc />
    public IReadOnlyList<SpecialistCard> Specialists { get; }

    /// <inheritdoc />
    public IReadOnlyList<SeasonCard> Seasons { get; }

    /// <inheritdoc />
    public IReadOnlyList<FavorCard> Favors { get; }

    /// <inheritdoc />
    public IReadOnlyList<TeamProfile> Teams { get; }

    /// <inheritdoc />
    public string FinalSeasonDiscoveryId { get; }

    /// <summary>
    /// A snapshot of the game's own content in <see cref="Catalog"/>. Seasons are put in order of their
    /// <see cref="SeasonCard.Index"/>.
    /// </summary>
    /// <remarks>
    /// Decision: the Mask of Warka is found as the discovery whose id or art key is "mask-of-warka", else the first
    /// whose id contains "mask"; none found means the FinalSeason effect brings nothing into the row.
    /// </remarks>
    /// <returns>The snapshot.</returns>
    public static CatalogSnapshot FromCatalog()
    {
        var discoveries = Catalog.Discoveries ?? Array.Empty<DiscoveryCard>();
        var mask = discoveries.FirstOrDefault(d => d != null && (d.Id == MaskOfWarkaKey || d.ArtKey == MaskOfWarkaKey))
            ?? discoveries.FirstOrDefault(d => d != null && d.Id != null && d.Id.Contains("mask", StringComparison.Ordinal));
        var seasons = (Catalog.Seasons ?? Array.Empty<SeasonCard>()).Where(s => s != null).OrderBy(s => s.Index).ToArray();
        return new CatalogSnapshot(
            discoveries,
            Catalog.Tablets,
            Catalog.Specialists,
            seasons,
            Catalog.Favors,
            Catalog.Teams,
            mask?.Id);
    }

    private static IReadOnlyList<T> Copy<T>(IEnumerable<T> items) where T : class =>
        items == null ? Array.Empty<T>() : items.Where(i => i != null).ToArray();
}
