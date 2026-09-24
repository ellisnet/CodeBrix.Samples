using System;
using System.Collections.Generic;
using BrixInvaders.Game.Hosting;
using CodeBrix.Platform.GameEngine.Drawing.Tilesheets;
using SkiaSharp;

namespace BrixInvaders.Game.Rendering;

/// <summary>
/// Every picture the game draws, resolved ONCE at load time from the tilesheets the Kenney provider materialised:
/// image key (see <see cref="SpriteCatalog"/>) to the frame's <see cref="SKImage"/>. Read-only afterwards, so the
/// render thread can look pictures up while the engine thread builds frames.
/// </summary>
public sealed class ImageLibrary
{
    private readonly Dictionary<string, SKImage> _images = new Dictionary<string, SKImage>(StringComparer.Ordinal);
    private readonly List<string> _missing = new List<string>();

    /// <summary>The number of pictures resolved.</summary>
    public int Count => _images.Count;

    /// <summary>The image keys that could not be resolved.</summary>
    public IReadOnlyList<string> Missing => _missing;

    /// <summary>Resolves every frame of an atlas the catalog lists under that atlas key.</summary>
    /// <param name="atlasKey">The atlas asset key.</param>
    /// <param name="atlas">The materialised atlas.</param>
    /// <param name="frameImageKeys">The image keys (atlas key + frame name) to resolve.</param>
    public void AddAtlasFrames(string atlasKey, Tilesheet atlas, IEnumerable<string> frameImageKeys)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(frameImageKeys);
        foreach (var key in frameImageKeys)
        {
            if (!SpriteCatalog.TrySplit(key, out var keyAtlas, out var frameName) || keyAtlas != atlasKey)
            {
                continue;
            }

            Add(key, atlas.GetImage(frameName, 0, 0));
        }
    }

    /// <summary>Resolves a loose picture (a sheet whose default region's single tile is the whole picture).</summary>
    /// <param name="key">The image key.</param>
    /// <param name="sheet">The sheet.</param>
    public void AddPicture(string key, Tilesheet sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        Add(key, sheet.GetImage(TilesheetRegion.DefaultRegionName, 0, 0));
    }

    /// <summary>Looks a picture up.</summary>
    /// <param name="key">The image key.</param>
    /// <returns>The picture, or null when it was never resolved.</returns>
    public SKImage Get(string key) => key != null && _images.TryGetValue(key, out var image) ? image : null;

    /// <summary>The native size of a picture.</summary>
    /// <param name="key">The image key.</param>
    /// <returns>Width and height, or (0, 0) when unknown.</returns>
    public (int Width, int Height) SizeOf(string key)
    {
        var image = Get(key);
        return image == null ? (0, 0) : (image.Width, image.Height);
    }

    private void Add(string key, SKImage image)
    {
        if (image == null)
        {
            _missing.Add(key);
            GameLog.Write($"WARNING: picture not found: {key}");
            return;
        }

        _images[key] = image;
    }
}
