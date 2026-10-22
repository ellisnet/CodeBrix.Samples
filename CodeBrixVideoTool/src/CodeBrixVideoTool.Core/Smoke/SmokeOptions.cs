using System;
using System.IO;
using CodeBrixVideoTool.Processing.Formats;

namespace CodeBrixVideoTool.Smoke;

/// <summary>What a scripted run asked for. Null when no scripted run was asked for at all.</summary>
/// <param name="Destination">The format the run converts its generated source clip to.</param>
/// <param name="WorkFolder">The folder the run writes its source clip and its outputs into.</param>
/// <param name="KeepFiles">Whether the work folder is left behind when the run finishes.</param>
/// <param name="HoldSeconds">How long the window is left up at the end, from 0 to 300 seconds.</param>
/// <param name="ThenDestination">
/// A second format to transcode the run's own output to at the end, or null for none - the way to drive a
/// conversion FROM one of the five formats, a Mode 2 or Mode 3 file above all, through the whole pipeline.
/// </param>
public sealed record SmokeOptions(
    MediaFormatKind Destination, string WorkFolder, bool KeepFiles, int HoldSeconds, MediaFormatKind? ThenDestination = null)
{
    /// <summary>
    /// Reads what the environment asked for. <c>CODEBRIXVIDEOTOOL_SMOKE</c> names the destination and
    /// is what turns a scripted run on at all; <c>CODEBRIXVIDEOTOOL_SMOKE_OUT</c>,
    /// <c>CODEBRIXVIDEOTOOL_SMOKE_KEEP</c>, <c>CODEBRIXVIDEOTOOL_SMOKE_HOLD</c> and
    /// <c>CODEBRIXVIDEOTOOL_SMOKE_THEN</c> tune it.
    /// </summary>
    /// <returns>What the run was asked for, or null when no scripted run was asked for.</returns>
    public static SmokeOptions FromEnvironment()
    {
        var requested = Environment.GetEnvironmentVariable("CODEBRIXVIDEOTOOL_SMOKE");
        if (string.IsNullOrWhiteSpace(requested))
        {
            return null;
        }

        var folder = Environment.GetEnvironmentVariable("CODEBRIXVIDEOTOOL_SMOKE_OUT");
        if (string.IsNullOrWhiteSpace(folder))
        {
            folder = Path.Combine(Path.GetTempPath(), "CodeBrixVideoTool.Smoke", Guid.NewGuid().ToString("N"));
        }

        var keep = Environment.GetEnvironmentVariable("CODEBRIXVIDEOTOOL_SMOKE_KEEP");
        _ = int.TryParse(Environment.GetEnvironmentVariable("CODEBRIXVIDEOTOOL_SMOKE_HOLD"), out var hold);

        var then = Environment.GetEnvironmentVariable("CODEBRIXVIDEOTOOL_SMOKE_THEN");
        MediaFormatKind? thenDestination = string.IsNullOrWhiteSpace(then) ? null : ParseDestination(then);

        return new SmokeOptions(
            ParseDestination(requested), folder, keep is "1" or "true", Math.Clamp(hold, 0, 300), thenDestination);
    }

    private static MediaFormatKind ParseDestination(string requested) => requested.Trim().ToLowerInvariant() switch
    {
        "mode1" => MediaFormatKind.CodeBrixMode1,
        "webm" => MediaFormatKind.WebM,
        "mkv" or "matroska" => MediaFormatKind.Matroska,
        "mode3" or "master" => MediaFormatKind.CodeBrixMaster,
        _ => MediaFormatKind.CodeBrixMode2,
    };
}
