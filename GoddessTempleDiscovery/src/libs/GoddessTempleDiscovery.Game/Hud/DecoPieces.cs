using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CodeBrix.Platform.GameEngine.Drawing.Direct.DrawLists;
using GoddessTempleDiscovery.Assets;
using GoddessTempleDiscovery.Game.Session;
using SkiaSharp;

namespace GoddessTempleDiscovery.Game.Hud;

/// <summary>
/// Draws the Assets' Art Deco pieces on the HUD as table rasters (<see cref="TableArtwork.Art"/>). Every piece is
/// drawn in a square 400 box with its shape somewhere inside; the drawn part's bounds are measured once from the
/// raster, so a piece can be stretched exactly onto a rectangle of the table.
/// </summary>
public sealed class DecoPieces : IDisposable
{
    /// <summary>
    /// The trench slot plate without its limestone label plate (the art's label plate is too small at table scale to
    /// hold the slot's two lines, so the labels are set in gold under the slot instead).
    /// </summary>
    public const string SlotPlateWithoutLabel = "deco-slot-plate~no-label";

    private readonly TableArtwork _artwork;
    private readonly Dictionary<string, SKImage> _cropped = new Dictionary<string, SKImage>(StringComparer.Ordinal);

    /// <summary>Creates the painter's piece box.</summary>
    /// <param name="artwork">The table's artwork.</param>
    public DecoPieces(TableArtwork artwork)
    {
        _artwork = artwork ?? throw new ArgumentNullException(nameof(artwork));
    }

    /// <summary>The raster of a piece cropped to its drawn part (null when the catalog lacks it). Owned here.</summary>
    /// <param name="key">The art key.</param>
    /// <returns>The image.</returns>
    public SKImage Image(string key)
    {
        if (_cropped.TryGetValue(key, out var cropped))
        {
            return cropped;
        }

        var image = key == SlotPlateWithoutLabel ? SlotPlateArt() : _artwork.Art(key);
        if (image == null)
        {
            return null;
        }

        var content = Content(image);
        cropped = image.Subset(content) ?? image;
        _cropped[key] = cropped;
        return cropped;
    }

    private SKImage SlotPlateArt()
    {
        if (!ArtCatalog.Contains("deco-slot-plate"))
        {
            return null;
        }

        var svg = Regex.Replace(ArtCatalog.ReadSvg("deco-slot-plate"), "<g id=\"label-plate\">.*?</g>", string.Empty, RegexOptions.Singleline);
        var key = TableArtwork.ArtTableKey(SlotPlateWithoutLabel);
        _artwork.Register(key, svg);
        return _artwork.Table.Image(key);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var image in _cropped.Values)
        {
            image.Dispose();
        }

        _cropped.Clear();
    }

    /// <summary>Draws a piece so that its drawn part fills <paramref name="target"/> exactly.</summary>
    /// <param name="list">The draw list.</param>
    /// <param name="key">The art key.</param>
    /// <param name="target">The rectangle.</param>
    /// <param name="alpha">Opacity.</param>
    /// <param name="rotation">Clockwise degrees about the target's centre.</param>
    public void Fill(DrawList list, string key, SKRect target, double alpha = 1, double rotation = 0)
    {
        var image = Image(key);
        if (image == null)
        {
            return;
        }

        //A quarter turn swaps the box's sides, so the picture is drawn into the turned box
        var quarter = Math.Abs(Math.Round(rotation / 90.0)) % 2 == 1;
        var w = quarter ? target.Height : target.Width;
        var h = quarter ? target.Width : target.Height;
        list.Image(image, target.MidX, target.MidY, w, h, rotation, alpha, DrawImageFit.Stretch);
    }

    /// <summary>Draws a piece fitted (aspect kept) and centred in a box.</summary>
    /// <param name="list">The draw list.</param>
    /// <param name="key">The art key.</param>
    /// <param name="centreX">The box centre.</param>
    /// <param name="centreY">The box centre.</param>
    /// <param name="width">The box width.</param>
    /// <param name="height">The box height.</param>
    /// <param name="alpha">Opacity.</param>
    /// <param name="rotation">Clockwise degrees.</param>
    public void Fit(DrawList list, string key, float centreX, float centreY, float width, float height, double alpha = 1, double rotation = 0)
    {
        var image = Image(key);
        if (image == null)
        {
            return;
        }

        var scale = Math.Min(width / image.Width, height / image.Height);
        Fill(list, key, SKRect.Create(centreX - (image.Width * scale / 2), centreY - (image.Height * scale / 2),
            image.Width * scale, image.Height * scale), alpha, rotation);
    }

    /// <summary>Draws the four corner brackets of a panel (the piece is the top-left one; the others are rotations).</summary>
    /// <param name="list">The draw list.</param>
    /// <param name="panel">The panel.</param>
    /// <param name="size">The bracket size.</param>
    public void Corners(DrawList list, SKRect panel, float size)
    {
        Fill(list, "deco-corner-bracket", SKRect.Create(panel.Left, panel.Top, size, size));
        Fill(list, "deco-corner-bracket", SKRect.Create(panel.Right - size, panel.Top, size, size), rotation: 90);
        Fill(list, "deco-corner-bracket", SKRect.Create(panel.Right - size, panel.Bottom - size, size, size), rotation: 180);
        Fill(list, "deco-corner-bracket", SKRect.Create(panel.Left, panel.Bottom - size, size, size), rotation: 270);
    }

    //The bounds of the drawn (non-transparent) pixels of a raster
    private static SKRectI Content(SKImage image)
    {
        var rect = SKRectI.Create(0, 0, image.Width, image.Height);
        using (var bitmap = SKBitmap.FromImage(image))
        {
            if (bitmap != null)
            {
                int left = bitmap.Width, top = bitmap.Height, right = -1, bottom = -1;
                for (var y = 0; y < bitmap.Height; y += 2)
                {
                    for (var x = 0; x < bitmap.Width; x += 2)
                    {
                        if (bitmap.GetPixel(x, y).Alpha > 8)
                        {
                            left = Math.Min(left, x);
                            right = Math.Max(right, x);
                            top = Math.Min(top, y);
                            bottom = Math.Max(bottom, y);
                        }
                    }
                }

                if (right > left && bottom > top)
                {
                    rect = new SKRectI(left, top, Math.Min(bitmap.Width, right + 2), Math.Min(bitmap.Height, bottom + 2));
                }
            }
        }

        return rect;
    }
}
