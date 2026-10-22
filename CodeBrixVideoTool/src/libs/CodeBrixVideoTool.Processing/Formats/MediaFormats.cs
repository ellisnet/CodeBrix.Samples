using CodeBrix.VideoPlayback.Containers.Cbv;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeBrixVideoTool.Processing.Formats;

/// <summary>
/// Everything the application knows about the six container shapes: which of them it writes, what
/// each is called, which codecs each is written with, and which of them the in-application player
/// can open.
/// </summary>
public static class MediaFormats
{
    /// <summary>The file-name extensions an import will accept, lower-case and dotted.</summary>
    /// <remarks>
    /// Every one of these is a container FFmpeg reads. The list is a first filter for the file
    /// picker only - a candidate is still probed before it is accepted.
    /// </remarks>
    public static IReadOnlyList<string> ImportExtensions { get; } =
    [
        ".mp4",
        ".m4v",
        ".mov",
        ".avi",
        ".mpg",
        ".mpeg",
        ".ts",
        ".m2ts",
        ".wmv",
        ".flv",
        ".ogv",
        ".3gp",
    ];

    /// <summary>
    /// The file-name extensions of the formats the in-application player opens, lower-case and dotted:
    /// the rest of the open dialog's filter, after <see cref="ImportExtensions" />.
    /// </summary>
    public static IReadOnlyList<string> PlayableExtensions { get; } =
    [
        ".mkv",
        ".webm",
        CbvFormat.FileExtension,
        CbvFormat.MasterFileExtension,
    ];

    /// <summary>The five formats this application writes, plays and transcodes between.</summary>
    public static IReadOnlyList<MediaFormatKind> SupportedFormats { get; } =
    [
        MediaFormatKind.Matroska,
        MediaFormatKind.WebM,
        MediaFormatKind.CodeBrixMode1,
        MediaFormatKind.CodeBrixMode2,
        MediaFormatKind.CodeBrixMaster,
    ];

    /// <summary>The name to show a person for one format.</summary>
    /// <param name="kind">The format.</param>
    /// <returns>A short display name.</returns>
    public static string DisplayName(MediaFormatKind kind) => kind switch
    {
        MediaFormatKind.Mp4 => "MP4 (H.264 + AAC)",
        MediaFormatKind.Matroska => "Matroska .mkv (AV1 + Opus)",
        MediaFormatKind.WebM => "WebM .webm (AV1 + Opus)",
        MediaFormatKind.CodeBrixMode1 => "CodeBrix Mode 1 .cbv (AV1 + Opus)",
        MediaFormatKind.CodeBrixMode2 => "CodeBrix Mode 2 .cbv (AV1 + Vorbis)",
        MediaFormatKind.CodeBrixMaster => "CodeBrix Mode 3 .cbvmaster (AV1 + alpha + FLAC)",
        _ => "Unrecognised",
    };

    /// <summary>A very short name for one format, for a badge in a list.</summary>
    /// <param name="kind">The format.</param>
    /// <returns>A two- or six-character name.</returns>
    public static string ShortName(MediaFormatKind kind) => kind switch
    {
        MediaFormatKind.Mp4 => "MP4",
        MediaFormatKind.Matroska => "MKV",
        MediaFormatKind.WebM => "WebM",
        MediaFormatKind.CodeBrixMode1 => "Mode 1",
        MediaFormatKind.CodeBrixMode2 => "Mode 2",
        MediaFormatKind.CodeBrixMaster => "Mode 3",
        _ => "?",
    };

    /// <summary>
    /// The label a "save as" dialog puts on the file-type filter for one extension. This is the same
    /// policy <see cref="DisplayName" /> states, said the short way a dialog has room for, and keyed
    /// by the extension because that is all a save dialog is given.
    /// </summary>
    /// <param name="extension">A lower-case, dotted extension.</param>
    /// <returns>What to call a file with that extension in a dialog.</returns>
    /// <remarks>
    /// An extension this application does not write is described as an MP4, because the <c>.mp4</c>
    /// export is the one destination that is not one of the five supported formats.
    /// </remarks>
    public static string DescribeExtension(string extension) => extension switch
    {
        ".cbv" => "CodeBrix video",
        ".cbvmaster" => "CodeBrix master video",
        ".mkv" => "Matroska video",
        ".webm" => "WebM video",
        _ => "MP4 video",
    };

    /// <summary>The file-name extension a destination format is written with.</summary>
    /// <param name="kind">The format.</param>
    /// <returns>A lower-case, dotted extension.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The format is not one this application writes.</exception>
    public static string Extension(MediaFormatKind kind) => kind switch
    {
        MediaFormatKind.Mp4 => ".mp4",
        MediaFormatKind.Matroska => ".mkv",
        MediaFormatKind.WebM => ".webm",
        MediaFormatKind.CodeBrixMode1 => ".cbv",
        MediaFormatKind.CodeBrixMode2 => ".cbv",
        MediaFormatKind.CodeBrixMaster => ".cbvmaster",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There is no extension for an unrecognised format."),
    };

    /// <summary>
    /// The audio codec a destination format is written with. This is the whole of the application's
    /// audio-codec policy: Mode 2 is Vorbis, Mode 3 is FLAC, the other three are Opus, and an export
    /// is AAC.
    /// </summary>
    /// <param name="kind">The destination format.</param>
    /// <returns>The codec that destination is written with.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The format is not one this application writes.</exception>
    public static TargetAudioCodec AudioCodecFor(MediaFormatKind kind) => kind switch
    {
        MediaFormatKind.Mp4 => TargetAudioCodec.Aac,
        MediaFormatKind.Matroska => TargetAudioCodec.Opus,
        MediaFormatKind.WebM => TargetAudioCodec.Opus,
        MediaFormatKind.CodeBrixMode1 => TargetAudioCodec.Opus,

        //The hard invariant: a bespoke CBVF file this application writes carries Vorbis, never Opus.
        MediaFormatKind.CodeBrixMode2 => TargetAudioCodec.Vorbis,

        //Its master form is lossless: FLAC is the only codec a Mode 3 file takes.
        MediaFormatKind.CodeBrixMaster => TargetAudioCodec.Flac,

        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There is no audio codec for an unrecognised format."),
    };

    /// <summary>
    /// The most audio channels a destination may carry. The five formats this application writes are
    /// capped at stereo whichever codec they carry, because this application writes mono or stereo
    /// audio only; an <c>.mp4</c> export is not capped and keeps the source's own layout, up to the
    /// authoring ceiling of eight.
    /// </summary>
    /// <param name="destination">The destination format.</param>
    /// <returns>The channel ceiling for that destination.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The format is not one this application writes.</exception>
    /// <remarks>
    /// The cap is an application policy rather than a codec limit: nothing in Opus, Vorbis, FLAC or the
    /// containers themselves stops a surround track being written. It is stated per DESTINATION and
    /// not per codec so that the one uncapped destination - the <c>.mp4</c> export, which is AAC -
    /// stays uncapped no matter what else is written with the same codec later.
    /// </remarks>
    public static int MaxAudioChannels(MediaFormatKind destination) => destination switch
    {
        MediaFormatKind.Mp4 => 8,
        MediaFormatKind.Matroska => 2,
        MediaFormatKind.WebM => 2,
        MediaFormatKind.CodeBrixMode1 => 2,
        MediaFormatKind.CodeBrixMode2 => 2,
        MediaFormatKind.CodeBrixMaster => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(destination), destination, "There is no channel ceiling for an unrecognised format."),
    };

    /// <summary>
    /// The channel count a destination is written with: the source's own, clamped to at least one and
    /// at most <see cref="MaxAudioChannels" /> for that destination. This is the whole of the
    /// application's channel policy; nothing is ever upmixed.
    /// </summary>
    /// <param name="destination">The destination format.</param>
    /// <param name="sourceChannels">How many channels the source carries.</param>
    /// <returns>The channel count to encode.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The format is not one this application writes.</exception>
    public static int AudioChannelsFor(MediaFormatKind destination, int sourceChannels) =>
        Math.Clamp(sourceChannels, 1, MaxAudioChannels(destination));

    /// <summary>The four quality stops a person may pick between, from smallest file to best picture.</summary>
    /// <remarks>
    /// The order is the order a drop-down lists them in. The knob moves the encoder's rate factor and
    /// nothing else: the speed preset stays pinned so an encode takes about as long whichever stop is
    /// chosen, and sound is never touched by it.
    /// </remarks>
    public static IReadOnlyList<QualityLevel> QualityLevels { get; } =
    [
        QualityLevel.Fair,
        QualityLevel.Good,
        QualityLevel.Better,
        QualityLevel.Best,
    ];

    /// <summary>The video codec a destination format is written with.</summary>
    /// <param name="kind">The destination format.</param>
    /// <returns>The codec that destination is written with.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The format is not one this application writes.</exception>
    public static TargetVideoCodec VideoCodecFor(MediaFormatKind kind) => kind switch
    {
        MediaFormatKind.Mp4 => TargetVideoCodec.H264,
        MediaFormatKind.Matroska => TargetVideoCodec.Av1,
        MediaFormatKind.WebM => TargetVideoCodec.Av1,
        MediaFormatKind.CodeBrixMode1 => TargetVideoCodec.Av1,
        MediaFormatKind.CodeBrixMode2 => TargetVideoCodec.Av1,
        MediaFormatKind.CodeBrixMaster => TargetVideoCodec.Av1,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There is no video codec for an unrecognised format."),
    };

    /// <summary>Whether a format is one of the five this application writes and plays.</summary>
    /// <param name="kind">The format.</param>
    /// <returns>True for MKV, WebM, Mode 1, Mode 2 and Mode 3.</returns>
    public static bool IsSupportedFormat(MediaFormatKind kind) => SupportedFormats.Contains(kind);

    /// <summary>
    /// Whether the in-application player can open a format. The player decodes AV1, so it opens all
    /// five supported formats and no <c>.mp4</c> ever. A Mode 3 file needs nothing more: its alpha
    /// plane is an ordinary AV1 stream, and its FLAC sound is decoded by the playback core itself.
    /// </summary>
    /// <param name="kind">The format.</param>
    /// <returns>True when the file can be played inside this application.</returns>
    public static bool IsPlayable(MediaFormatKind kind) => IsSupportedFormat(kind);

    /// <summary>Whether a format is one of the CodeBrix video formats, carried in a <c>.cbv</c> or <c>.cbvmaster</c> file.</summary>
    /// <param name="kind">The format.</param>
    /// <returns>True for Mode 1, Mode 2 and Mode 3.</returns>
    public static bool IsCodeBrixContainer(MediaFormatKind kind) =>
        kind is MediaFormatKind.CodeBrixMode1 or MediaFormatKind.CodeBrixMode2 or MediaFormatKind.CodeBrixMaster;

    /// <summary>
    /// Whether a format is the bespoke CBVF container, which FFmpeg cannot open at all: Mode 2, or its
    /// master form, Mode 3.
    /// </summary>
    /// <param name="kind">The format.</param>
    /// <returns>True for Mode 2 and Mode 3.</returns>
    public static bool IsBespokeContainer(MediaFormatKind kind) =>
        kind is MediaFormatKind.CodeBrixMode2 or MediaFormatKind.CodeBrixMaster;

    /// <summary>
    /// Whether a format carries an alpha channel - real transparency - when this application writes
    /// it. Only the master form does; every other destination is opaque.
    /// </summary>
    /// <param name="kind">The format.</param>
    /// <returns>True for Mode 3.</returns>
    public static bool CarriesAlpha(MediaFormatKind kind) => kind == MediaFormatKind.CodeBrixMaster;

    /// <summary>
    /// Whether a conversion between two formats may change the picture's size, which decides whether
    /// the resolution drop-down offers anything below the source's own size.
    /// </summary>
    /// <param name="source">Where the media is coming from.</param>
    /// <param name="destination">Where it is going.</param>
    /// <returns>False only for a Mode 3 file exported to <c>.mp4</c>.</returns>
    /// <remarks>
    /// A master goes out to <c>.mp4</c> through the authoring library's exporter, because that is what
    /// composites its alpha over a backdrop, and the exporter always writes the source's own size.
    /// Scaling the result afterwards would encode the picture twice, so a smaller size is not offered.
    /// </remarks>
    public static bool CanResize(MediaFormatKind source, MediaFormatKind destination) =>
        !(source == MediaFormatKind.CodeBrixMaster && destination == MediaFormatKind.Mp4);

    /// <summary>What a conversion between two formats is called.</summary>
    /// <param name="source">Where the media is coming from.</param>
    /// <param name="destination">Where it is going.</param>
    /// <returns>Import, Transcode or Export.</returns>
    /// <exception cref="ArgumentException">The pair is not a conversion this application offers.</exception>
    public static ConversionOperationKind OperationFor(MediaFormatKind source, MediaFormatKind destination)
    {
        if (source == MediaFormatKind.Mp4 && IsSupportedFormat(destination))
        {
            return ConversionOperationKind.Import;
        }

        if (IsSupportedFormat(source) && destination == MediaFormatKind.Mp4)
        {
            return ConversionOperationKind.Export;
        }

        if (IsSupportedFormat(source) && IsSupportedFormat(destination))
        {
            return ConversionOperationKind.Transcode;
        }

        throw new ArgumentException(
            $"Converting {DisplayName(source)} to {DisplayName(destination)} is not something this application does.",
            nameof(destination));
    }

    /// <summary>The verb to put on the action button for one conversion.</summary>
    /// <param name="operation">The kind of conversion.</param>
    /// <returns>"Import", "Transcode" or "Export".</returns>
    public static string ActionVerb(ConversionOperationKind operation) => operation switch
    {
        ConversionOperationKind.Import => "Import",
        ConversionOperationKind.Export => "Export",
        _ => "Transcode",
    };

    /// <summary>The word for a conversion that is under way.</summary>
    /// <param name="operation">The kind of conversion.</param>
    /// <returns>"Importing", "Transcoding" or "Exporting".</returns>
    public static string ActionGerund(ConversionOperationKind operation) => operation switch
    {
        ConversionOperationKind.Import => "Importing",
        ConversionOperationKind.Export => "Exporting",
        _ => "Transcoding",
    };

    /// <summary>
    /// The destination formats offered for one source: the five supported formats other than the
    /// source's own, plus <c>.mp4</c> when the source is one of the five.
    /// </summary>
    /// <param name="source">The format the media is in now.</param>
    /// <returns>The destinations that make sense, in the order they should be listed.</returns>
    public static IReadOnlyList<MediaFormatKind> DestinationsFor(MediaFormatKind source)
    {
        if (source == MediaFormatKind.Unknown)
        {
            return [];
        }

        var destinations = new List<MediaFormatKind>();
        foreach (var candidate in SupportedFormats)
        {
            if (candidate != source)
            {
                destinations.Add(candidate);
            }
        }

        if (IsSupportedFormat(source))
        {
            destinations.Add(MediaFormatKind.Mp4);
        }

        return destinations;
    }

    /// <summary>
    /// Works out what a file is from its first four bytes, falling back to its extension when the
    /// file cannot be read.
    /// </summary>
    /// <param name="path">The file to look at.</param>
    /// <returns>The format, or Unknown.</returns>
    /// <remarks>
    /// A <c>.cbv</c> or <c>.cbvmaster</c> file is Mode 1 when it starts with the EBML magic, and the
    /// bespoke container when it starts with the ASCII bytes "CBVF": Mode 2 at format version 0 and
    /// Mode 3, the master form, at version 1. Nothing else about the file is consulted, which is exactly
    /// how the playback core picks its reader. The extension is only ever a hint, so a master file named
    /// <c>.cbv</c> is still a master.
    /// </remarks>
    public static MediaFormatKind Detect(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return MediaFormatKind.Unknown;
        }

        var extension = Path.GetExtension(path).ToLowerInvariant();
        var sniffed = SniffSignature(path);

        if (extension is ".cbv" or ".cbvmaster")
        {
            return sniffed == MediaFormatKind.Unknown ? MediaFormatKind.Unknown : sniffed;
        }

        if (extension == ".mkv")
        {
            return MediaFormatKind.Matroska;
        }

        if (extension == ".webm")
        {
            return MediaFormatKind.WebM;
        }

        return ImportExtensions.Contains(extension) ? MediaFormatKind.Mp4 : MediaFormatKind.Unknown;
    }

    private static MediaFormatKind SniffSignature(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            //The four-byte magic, then - in the bespoke container - its u16 little-endian format version.
            Span<byte> first = stackalloc byte[6];
            var read = stream.ReadAtLeast(first, first.Length, false);
            if (read < 4)
            {
                return MediaFormatKind.Unknown;
            }

            if (CbvReader.IsCbv(first))
            {
                if (read < first.Length)
                {
                    return MediaFormatKind.Unknown;
                }

                return BinaryPrimitives.ReadUInt16LittleEndian(first[4..]) >= CbvFormat.MasterVersion
                    ? MediaFormatKind.CodeBrixMaster
                    : MediaFormatKind.CodeBrixMode2;
            }

            return first[..4].SequenceEqual(CbvFormat.EbmlMagic) ? MediaFormatKind.CodeBrixMode1 : MediaFormatKind.Unknown;
        }
        catch (IOException)
        {
            return MediaFormatKind.Unknown;
        }
        catch (UnauthorizedAccessException)
        {
            return MediaFormatKind.Unknown;
        }
    }
}
