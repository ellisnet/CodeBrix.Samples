using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats.Png;
using CodeBrix.Imaging.PixelFormats;
using CodeBrix.Imaging.Processing;
using System;
using System.IO;

namespace InannaOctaquetra.Helpers;

/// <summary>
/// Writes a captured frame to a PNG file for the INANNA_SNAP snapshot hook (see the README).
/// The page captures its pixels with RenderTargetBitmap; this turns them into a file with
/// CodeBrix.Imaging, so the hook works the same on every head - desktop and Android alike.
/// </summary>
public static class SnapshotPng
{
    /// <summary>
    /// Saves BGRA8 pixels (the layout RenderTargetBitmap.GetPixelsAsync hands back) as a PNG.
    /// When <paramref name="logicalWidth"/> and <paramref name="logicalHeight"/> are given and
    /// differ from the captured size - a 2x Retina capture, say - the frame is resampled down to
    /// them with the Mitchell filter so the file shows the page at its design size.
    /// </summary>
    /// <param name="bgra">The pixel bytes, four per pixel, rows top to bottom.</param>
    /// <param name="width">The captured width in pixels.</param>
    /// <param name="height">The captured height in pixels.</param>
    /// <param name="path">Where to write the PNG; the folder is created if needed.</param>
    /// <param name="logicalWidth">Optional width to resample to; 0 keeps the captured size.</param>
    /// <param name="logicalHeight">Optional height to resample to; 0 keeps the captured size.</param>
    public static void Save(byte[] bgra, int width, int height, string path,
        int logicalWidth = 0, int logicalHeight = 0)
    {
        ArgumentNullException.ThrowIfNull(bgra);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (width <= 0 || height <= 0) { throw new ArgumentOutOfRangeException(nameof(width), "The frame is empty."); }

        var folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder)) { Directory.CreateDirectory(folder); }

        using var image = Image.LoadPixelData<Bgra32>(bgra, width, height, PngFormat.Instance);
        if (logicalWidth > 0 && logicalHeight > 0 && (logicalWidth != width || logicalHeight != height))
        {
            image.Mutate(x => x.Resize(logicalWidth, logicalHeight, KnownResamplers.MitchellNetravali));
        }

        image.SaveAsPng(path);
    }
}
