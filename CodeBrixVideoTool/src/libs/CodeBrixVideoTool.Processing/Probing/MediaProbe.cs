using CodeBrix.VideoPlayback.Containers;
using CodeBrix.VideoProcessing;
using CodeBrix.VideoProcessing.Exceptions;
using CodeBrixVideoTool.Processing.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodeBrixVideoTool.Processing.Probing;

/// <summary>
/// The application's one way of finding out what is in a media file.
/// </summary>
/// <remarks>
/// A <c>.cbv</c> or <c>.cbvmaster</c> file is opened with the playback core's container readers, because
/// ffprobe cannot read the bespoke CBVF container at all and would only see a Mode 1 file as an
/// ordinary WebM. Everything else goes to ffprobe, run through CodeBrix.VideoProcessing - the only
/// external process this application ever starts.
/// </remarks>
public sealed class MediaProbe : IMediaProbe
{
    /// <inheritdoc />
    public Task<SourceMediaInfo> ProbeAsync(string path, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new VideoToolProcessingException("No file was named.");
        }

        if (!File.Exists(path))
        {
            throw new VideoToolProcessingException($"There is no file at '{path}'.");
        }

        var format = MediaFormats.Detect(path);
        if (format == MediaFormatKind.Unknown)
        {
            throw new VideoToolProcessingException(
                $"'{Path.GetFileName(path)}' is not a container this application recognises.");
        }

        return MediaFormats.IsCodeBrixContainer(format)
            ? Task.FromResult(ProbeCodeBrixContainer(path, format))
            : ProbeWithFfProbeAsync(path, format, cancellationToken);
    }

    private static SourceMediaInfo ProbeCodeBrixContainer(string path, MediaFormatKind format)
    {
        try
        {
            using var reader = MediaContainers.Open(path);

            //A master file's alpha plane is a second video track - monochrome, the picture's own size and
            //in lock step with it. It is not the picture, so it is counted rather than described.
            var video = reader.Tracks.FirstOrDefault(t => t.Kind == MediaTrackKind.Video && !t.IsAlphaPlane);
            var audio = reader.Tracks.FirstOrDefault(t => t.Kind == MediaTrackKind.Audio);

            var info = new SourceMediaInfo
            {
                Path = path,
                FileName = Path.GetFileName(path),
                Format = format,
                Duration = reader.Duration,
                Width = video?.Width ?? 0,
                Height = video?.Height ?? 0,
                FrameRate = FrameRateFrom(video?.DefaultDuration ?? TimeSpan.Zero),
                VideoCodec = video?.CodecId ?? string.Empty,
                AudioCodec = audio?.CodecId ?? string.Empty,
                AudioChannels = audio?.Channels ?? 0,
                AudioSampleRateHz = audio?.SampleRate ?? 0,
                HasAlpha = reader.Tracks.Any(t => t.IsAlphaPlane),
                CaptionTrackCount = reader.CaptionTracks.Count,
                ChapterCount = reader.Chapters.Count,
                SizeInBytes = new FileInfo(path).Length,
                Notices = reader.Notices.ToArray(),
            };

            RequireVideo(info);
            return info;
        }
        catch (VideoToolProcessingException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new VideoToolProcessingException(
                $"'{Path.GetFileName(path)}' could not be read as a CodeBrix video file: {exception.Message}",
                exception);
        }
    }

    private static async Task<SourceMediaInfo> ProbeWithFfProbeAsync(
        string path, MediaFormatKind format, CancellationToken cancellationToken)
    {
        IMediaAnalysis analysis;
        try
        {
            analysis = await FFProbe.AnalyseAsync(path, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (exception is FFMpegException or FFProbeException or IOException)
        {
            throw new VideoToolProcessingException(
                $"'{Path.GetFileName(path)}' could not be probed: {exception.Message}", exception);
        }

        var video = analysis.PrimaryVideoStream;
        var audio = analysis.PrimaryAudioStream;

        var info = new SourceMediaInfo
        {
            Path = path,
            FileName = Path.GetFileName(path),
            Format = format,
            Duration = analysis.Duration,
            Width = video?.Width ?? 0,
            Height = video?.Height ?? 0,
            FrameRate = video?.FrameRate ?? 0d,
            VideoCodec = video?.CodecName ?? string.Empty,
            AudioCodec = audio?.CodecName ?? string.Empty,
            AudioChannels = audio?.Channels ?? 0,
            AudioSampleRateHz = audio?.SampleRateHz ?? 0,
            HasAlpha = HasAlphaChannel(video?.PixelFormat),
            CaptionTrackCount = analysis.SubtitleStreams?.Count ?? 0,
            ChapterCount = analysis.Chapters?.Count ?? 0,
            SizeInBytes = new FileInfo(path).Length,
            Notices = NoticesFrom(analysis),
        };

        RequireVideo(info);
        return info;
    }

    private static IReadOnlyList<string> NoticesFrom(IMediaAnalysis analysis)
    {
        var errors = analysis.ErrorData;
        return errors is { Count: > 0 } ? errors.ToArray() : [];
    }

    /// <summary>
    /// Whether an FFmpeg pixel format carries an alpha channel: the yuva, ya and gbrap families and the
    /// four packed RGB orders with an alpha byte. This is the same reading the authoring library's
    /// FFmpeg engine makes when a master is written with its alpha left to Auto, so what the list says
    /// about a source is what an import to Mode 3 will find.
    /// </summary>
    /// <param name="pixelFormat">The pixel format ffprobe reported, or null.</param>
    /// <returns>True when the format has an alpha channel.</returns>
    /// <remarks>
    /// It is a description, not a promise: a VP8 or VP9 WebM keeps its alpha in a side stream that
    /// ffprobe does not report in the pixel format, and a source whose alpha is everywhere opaque still
    /// "has" one.
    /// </remarks>
    public static bool HasAlphaChannel(string pixelFormat)
    {
        if (string.IsNullOrWhiteSpace(pixelFormat))
        {
            return false;
        }

        var format = pixelFormat.Trim().ToLowerInvariant();
        return format.StartsWith("yuva", StringComparison.Ordinal)
            || format.StartsWith("gbrap", StringComparison.Ordinal)
            || format.StartsWith("ya", StringComparison.Ordinal)
            || format.StartsWith("rgba", StringComparison.Ordinal)
            || format.StartsWith("bgra", StringComparison.Ordinal)
            || format.StartsWith("argb", StringComparison.Ordinal)
            || format.StartsWith("abgr", StringComparison.Ordinal);
    }

    private static double FrameRateFrom(TimeSpan frameDuration) =>
        frameDuration > TimeSpan.Zero ? 1d / frameDuration.TotalSeconds : 0d;

    private static void RequireVideo(SourceMediaInfo info)
    {
        if (!info.HasVideo)
        {
            throw new VideoToolProcessingException(
                $"'{info.FileName}' carries no video track, so there is nothing for this application to convert or play.");
        }

        if (info.Duration <= TimeSpan.Zero)
        {
            throw new VideoToolProcessingException(
                $"'{info.FileName}' does not state a duration, so its progress could not be reported and it is refused.");
        }
    }
}
