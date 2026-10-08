using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GoddessTempleDiscovery.Game.Cards;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Game.Session;

/// <summary>
/// The composed face of every card a game can deal, by table key. Composing is pure work (no table, no engine), so
/// the host runs it on a worker and registers the result on the engine thread a few faces per frame.
/// </summary>
public sealed class CardFaceLibrary
{
    private static readonly Lock CacheGate = new Lock();
    private static CardFaceLibrary _catalogLibrary;

    private CardFaceLibrary(IReadOnlyDictionary<string, string> faces)
    {
        Faces = faces;
    }

    /// <summary>Every face: table key to SVG text.</summary>
    public IReadOnlyDictionary<string, string> Faces { get; }

    /// <summary>Composes the faces of every card of a set of decks.</summary>
    /// <param name="catalog">The decks.</param>
    /// <returns>The library.</returns>
    public static CardFaceLibrary Compose(ICardCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var faces = new Dictionary<string, string>(StringComparer.Ordinal);
        using var composer = new CardFaceComposer();
        foreach (var card in catalog.Discoveries)
        {
            faces[CardFaceComposer.FaceKey(card.Id)] = composer.Compose(card);
        }

        foreach (var card in catalog.Tablets)
        {
            faces[CardFaceComposer.FaceKey(card.Id)] = composer.Compose(card);
        }

        foreach (var card in catalog.Specialists.GroupBy(s => s.Role).Select(g => g.First()))
        {
            faces[CardFaceComposer.FaceKey(card.Role)] = composer.Compose(card);
        }

        foreach (var card in catalog.Favors)
        {
            faces[CardFaceComposer.FaceKey(card)] = composer.Compose(card);
        }

        foreach (var card in catalog.Seasons)
        {
            faces[CardFaceComposer.FaceKey(card)] = composer.Compose(card);
        }

        return new CardFaceLibrary(faces);
    }

    /// <summary>The faces of the game's own content, composed once per process.</summary>
    /// <returns>The library.</returns>
    public static CardFaceLibrary ForCatalog()
    {
        lock (CacheGate)
        {
            return _catalogLibrary ??= Compose(CatalogSnapshot.FromCatalog());
        }
    }

    /// <summary>The keys not yet registered on a table.</summary>
    /// <param name="artwork">The table's artwork.</param>
    /// <returns>The keys.</returns>
    public IReadOnlyList<string> Pending(TableArtwork artwork) =>
        Faces.Keys.Where(k => !artwork.IsRegistered(k)).OrderBy(k => k, StringComparer.Ordinal).ToArray();

    /// <summary>Registers every face on a table at once (tests; the host spreads it over frames).</summary>
    /// <param name="artwork">The table's artwork.</param>
    public void RegisterAll(TableArtwork artwork)
    {
        ArgumentNullException.ThrowIfNull(artwork);
        foreach (var key in Pending(artwork))
        {
            artwork.Register(key, Faces[key]);
        }
    }
}
