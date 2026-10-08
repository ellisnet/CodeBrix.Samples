using System;
using System.IO;
using System.Text;
using CodeBrix.Platform.GameEngine.Drawing;
using GoddessTempleDiscovery.Assets;
using SkiaSharp;

namespace GoddessTempleDiscovery.Game.Rendering;

/// <summary>Rasterizes SVG text (a composed face, or a picture of the Assets catalog) to PNG bytes for the XAML side.</summary>
public static class SvgRaster
{
    /// <summary>The width a face is rendered at for the inspector.</summary>
    public const int FacePngWidth = 500;

    /// <summary>The height a face is rendered at for the inspector.</summary>
    public const int FacePngHeight = 800;

    /// <summary>Rasterizes SVG text at an exact size.</summary>
    /// <param name="svg">The SVG text.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <returns>The bitmap; the caller disposes it.</returns>
    public static SKBitmap Rasterize(string svg, int width, int height)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(svg);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        using var resource = SvgResource.Load(stream);
        return resource.Rasterize(width, height).Copy();
    }

    /// <summary>Rasterizes SVG text to PNG bytes.</summary>
    /// <param name="svg">The SVG text.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <returns>The PNG bytes.</returns>
    public static byte[] RenderPng(string svg, int width, int height)
    {
        using var bitmap = Rasterize(svg, width, height);
        return Encode(bitmap);
    }

    /// <summary>Rasterizes a face to PNG at 500 x 800.</summary>
    /// <param name="svgFace">The composed face.</param>
    /// <returns>The PNG bytes.</returns>
    public static byte[] RenderFacePng(string svgFace) => RenderPng(svgFace, FacePngWidth, FacePngHeight);

    /// <summary>Rasterizes a picture of the Assets catalog, its longer side <paramref name="longSide"/> pixels.</summary>
    /// <param name="artKey">The art key (a missing key yields an empty array).</param>
    /// <param name="longSide">The longer side, in pixels.</param>
    /// <returns>The PNG bytes.</returns>
    public static byte[] RenderArtPng(string artKey, int longSide)
    {
        if (!ArtCatalog.Contains(artKey))
        {
            return Array.Empty<byte>();
        }

        using var stream = ArtCatalog.Open(artKey);
        using var resource = SvgResource.Load(stream);
        var size = resource.IntrinsicSize;
        var scale = longSide / Math.Max(1f, Math.Max(size.Width, size.Height));
        var width = Math.Max(1, (int)Math.Round(size.Width * scale));
        var height = Math.Max(1, (int)Math.Round(size.Height * scale));
        using var bitmap = resource.Rasterize(width, height).Copy();
        return Encode(bitmap);
    }

    /// <summary>Rasterizes the drawn part of a catalog picture (cropped to <see cref="ContentBounds"/>) to PNG.</summary>
    /// <param name="artKey">The art key (a missing key yields an empty array).</param>
    /// <param name="longSide">The longer side of the cropped picture, in pixels.</param>
    /// <returns>The PNG bytes.</returns>
    public static byte[] RenderArtPngCropped(string artKey, int longSide)
    {
        var bounds = ContentBounds(artKey);
        if (bounds == null)
        {
            return Array.Empty<byte>();
        }

        var crop = bounds.Value;
        var scale = longSide / Math.Max(1f, Math.Max(crop.Width, crop.Height));
        using var stream = ArtCatalog.Open(artKey);
        using var resource = SvgResource.Load(stream);
        var size = resource.IntrinsicSize;
        using var full = resource.Rasterize(Math.Max(1, (int)Math.Round(size.Width * scale)), Math.Max(1, (int)Math.Round(size.Height * scale))).Copy();
        var rect = SKRectI.Round(new SKRect(crop.Left * scale, crop.Top * scale, crop.Right * scale, crop.Bottom * scale));
        rect.Intersect(new SKRectI(0, 0, full.Width, full.Height));
        using var cropped = new SKBitmap(Math.Max(1, rect.Width), Math.Max(1, rect.Height));
        full.ExtractSubset(cropped, rect);
        return Encode(cropped);
    }

    /// <summary>The share of pixels that are neither transparent nor one flat colour (a blank image scores 0).</summary>
    /// <param name="bitmap">The bitmap.</param>
    /// <returns>0 to 1.</returns>
    public static double InkCoverage(SKBitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        var first = bitmap.GetPixel(0, 0);
        long differing = 0, total = 0;
        for (var y = 0; y < bitmap.Height; y += 4)
        {
            for (var x = 0; x < bitmap.Width; x += 4)
            {
                var pixel = bitmap.GetPixel(x, y);
                total++;
                if (pixel.Alpha > 0 && pixel != first)
                {
                    differing++;
                }
            }
        }

        return total == 0 ? 0 : (double)differing / total;
    }

    /// <summary>
    /// The part of a catalog picture that is drawn (its non-transparent pixels), in the picture's own units; the whole
    /// view box when nothing is drawn. Deco pieces sit in a square box, so pages crop to this.
    /// </summary>
    /// <param name="artKey">The art key.</param>
    /// <returns>The bounds (x, y, width, height), or null for a missing key.</returns>
    public static SKRect? ContentBounds(string artKey)
    {
        if (!ArtCatalog.Contains(artKey))
        {
            return null;
        }

        lock (BoundsCache)
        {
            if (BoundsCache.TryGetValue(artKey, out var cached))
            {
                return cached;
            }
        }

        using var stream = ArtCatalog.Open(artKey);
        using var resource = SvgResource.Load(stream);
        var size = resource.IntrinsicSize;
        const int raster = 400;
        var scale = raster / Math.Max(1f, Math.Max(size.Width, size.Height));
        using var bitmap = resource.Rasterize(Math.Max(1, (int)(size.Width * scale)), Math.Max(1, (int)(size.Height * scale))).Copy();
        int left = bitmap.Width, top = bitmap.Height, right = -1, bottom = -1;
        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
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

        var bounds = right < left
            ? SKRect.Create(0, 0, size.Width, size.Height)
            : new SKRect(left / scale, top / scale, (right + 1) / scale, (bottom + 1) / scale);
        lock (BoundsCache)
        {
            BoundsCache[artKey] = bounds;
        }

        return bounds;
    }

    private static readonly System.Collections.Generic.Dictionary<string, SKRect> BoundsCache = new(StringComparer.Ordinal);

    private static byte[] Encode(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
