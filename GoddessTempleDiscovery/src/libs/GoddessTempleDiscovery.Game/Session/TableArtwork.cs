using System;
using System.Collections.Generic;
using CodeBrix.Platform.GameEngine.CardsAndDice.Table;
using GoddessTempleDiscovery.Assets;
using SkiaSharp;

namespace GoddessTempleDiscovery.Game.Session;

/// <summary>
/// The table and the artwork registered on it: the composed card faces (<c>face/...</c>) and the Assets pictures
/// the HUD draws (<c>art/&lt;key&gt;</c>). The add-on rasterizes a picture when it is registered and refuses a key
/// twice, so this keeps the set of keys already registered; one instance lives as long as its table.
/// </summary>
public sealed class TableArtwork
{
    /// <summary>The prefix of an Assets picture's table key.</summary>
    public const string ArtPrefix = "art/";

    private readonly HashSet<string> _registered = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Wraps a table.</summary>
    /// <param name="table">The table.</param>
    public TableArtwork(CardsAndDiceTable table)
    {
        Table = table ?? throw new ArgumentNullException(nameof(table));
    }

    /// <summary>The table.</summary>
    public CardsAndDiceTable Table { get; }

    /// <summary>How many pictures have been registered.</summary>
    public int RegisteredCount => _registered.Count;

    /// <summary>Whether a key has been registered.</summary>
    /// <param name="key">The table key.</param>
    /// <returns>True when registered.</returns>
    public bool IsRegistered(string key) => key != null && _registered.Contains(key);

    /// <summary>Registers SVG text under a key, once (later calls for the same key do nothing).</summary>
    /// <param name="key">The table key.</param>
    /// <param name="svg">The SVG text.</param>
    public void Register(string key, string svg)
    {
        if (_registered.Contains(key))
        {
            return;
        }

        Table.RegisterSvg(key, svg);
        _registered.Add(key);
    }

    /// <summary>The table key of an Assets picture.</summary>
    /// <param name="artKey">The Assets art key.</param>
    /// <returns>The table key.</returns>
    public static string ArtTableKey(string artKey) => ArtPrefix + artKey;

    /// <summary>
    /// The cached raster of an Assets picture (registered on first use); null when the catalog has no such picture.
    /// Table-owned: never dispose it.
    /// </summary>
    /// <param name="artKey">The Assets art key, such as <c>deco-button-plate</c>.</param>
    /// <returns>The image, or null.</returns>
    public SKImage Art(string artKey)
    {
        if (!ArtCatalog.Contains(artKey))
        {
            return null;
        }

        var key = ArtTableKey(artKey);
        Register(key, ArtCatalog.ReadSvg(artKey));
        return Table.Image(key);
    }
}
