using CodeBrix.VideoProcessing;
using System;
using System.IO;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// Extracts a poster frame from a downloaded video via ffmpeg, and probes media
/// durations via ffprobe. Every path is wrapped so a missing ffmpeg, an
/// unreadable codec or a timeout produces a null result (the caller renders a
/// media card plus one warning) — never a failed document.
/// </summary>
internal static class VideoPosterExtractor
{
    /// <summary>Probes the duration of a media file; null when ffprobe cannot say.</summary>
    public static TimeSpan? TryProbeDuration(string mediaFilePath)
    {
        try
        {
            var analysis = FFProbe.Analyse(mediaFilePath);
            var duration = analysis?.Duration ?? TimeSpan.Zero;
            return duration > TimeSpan.Zero ? duration : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Grabs a poster frame — at 10% of the duration, or one second in for very
    /// short clips — and returns the PNG bytes, or null when extraction fails.
    /// </summary>
    public static byte[] TryExtractPoster(string videoFilePath, string workDirectory, out TimeSpan? duration)
    {
        duration = TryProbeDuration(videoFilePath);
        try
        {
            var captureAt = duration is { } known && known > TimeSpan.FromSeconds(10)
                ? TimeSpan.FromTicks(known.Ticks / 10)
                : TimeSpan.FromSeconds(1);
            if (duration is { } total && captureAt >= total)
            {
                captureAt = TimeSpan.FromTicks(total.Ticks / 2);
            }

            Directory.CreateDirectory(workDirectory);
            var posterPath = Path.Combine(workDirectory, $"poster-{Guid.NewGuid():N}.png");
            try
            {
                if (!FFMpeg.Snapshot(videoFilePath, posterPath, size: null, captureTime: captureAt))
                {
                    return null;
                }
                return File.Exists(posterPath) ? File.ReadAllBytes(posterPath) : null;
            }
            finally
            {
                if (File.Exists(posterPath)) { File.Delete(posterPath); }
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
}
