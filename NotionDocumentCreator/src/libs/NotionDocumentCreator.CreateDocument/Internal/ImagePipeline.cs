using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats;
using CodeBrix.Imaging.Formats.Gif;
using CodeBrix.Imaging.Formats.Jpeg;
using CodeBrix.Imaging.Formats.Png;
using CodeBrix.Imaging.Formats.Webp;
using CodeBrix.Imaging.Processing;
using System;
using System.IO;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// Normalises a downloaded image for PDF embedding: capped pixel width, JPEG for
/// photographs, PNG for graphics with transparency (which also converts formats
/// the PDF embedder cannot take, such as WebP and GIF).
/// </summary>
internal static class ImagePipeline
{
    private const int MaxPixelWidth = 1800;
    private const int JpegQuality = 87;

    /// <summary>
    /// Decodes and normalises image bytes. Throws on undecodable data — callers
    /// turn that into a warning plus a media card, never a failed document.
    /// </summary>
    public static ProcessedImage ProcessForPrint(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        using var image = Image.Load(bytes, out IImageFormat format);

        var keepsTransparency = format is PngFormat or WebpFormat or GifFormat;
        var needsResize = image.Width > MaxPixelWidth;

        //Untouched JPEG/PNG bytes embed best — only re-encode when we must
        if (!needsResize && (format is JpegFormat || format is PngFormat))
        {
            return new ProcessedImage { Bytes = bytes, Width = image.Width, Height = image.Height };
        }

        if (needsResize)
        {
            image.Mutate(x => x.Resize(MaxPixelWidth, 0));
        }

        using var output = new MemoryStream();
        if (keepsTransparency)
        {
            image.Save(output, new PngEncoder());
        }
        else
        {
            image.Save(output, new JpegEncoder { Quality = JpegQuality });
        }

        return new ProcessedImage { Bytes = output.ToArray(), Width = image.Width, Height = image.Height };
    }
}

/// <summary>An image normalised for PDF embedding.</summary>
internal sealed class ProcessedImage
{
    /// <summary>The encoded (JPEG or PNG) image bytes.</summary>
    public byte[] Bytes { get; init; }

    /// <summary>Pixel width after processing.</summary>
    public int Width { get; init; }

    /// <summary>Pixel height after processing.</summary>
    public int Height { get; init; }
}
