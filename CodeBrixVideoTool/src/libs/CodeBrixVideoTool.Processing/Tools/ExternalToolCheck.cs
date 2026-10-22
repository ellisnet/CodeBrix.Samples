using CodeBrix.VideoProcessing.Core.Tools;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodeBrixVideoTool.Processing.Tools;

/// <summary>
/// Checks that FFmpeg, FFprobe and FFmpeg's SVT-AV1 encoder are there, and says which is missing - on an
/// engine that runs FFmpeg at all.
/// </summary>
/// <remarks>
/// <para>
/// All three questions go through the registered engine's external tools, exactly as the real work does:
/// it starts <c>ffmpeg -version</c> and <c>ffprobe -version</c> from wherever the engine looks for them
/// (the FFmpeg engine's configured folder, then the PATH), and reads FFmpeg's own encoder list. Nothing
/// here is specific to one operating system, and the answer is the one the conversions themselves would
/// get.
/// </para>
/// <para>
/// FFprobe is checked because this application uses it: on the FFmpeg engine, <see cref="Probing.MediaProbe" />
/// describes every non-<c>.cbv</c> file through it, and the sidecar extractor reads a source's chapters and
/// captions through it. SVT-AV1 is checked because every MKV, WebM and <c>.cbv</c> destination is authored
/// with it there.
/// </para>
/// <para>
/// An engine without external tools - Android's - needs none of the three: it describes, decodes and
/// writes video through the platform's own media codecs, and the authoring library encodes AV1 in process
/// on it. There the check has nothing to find and says so at once.
/// </para>
/// </remarks>
public sealed class ExternalToolCheck : IExternalToolCheck
{
    internal const string SvtAv1EncoderName = "libsvtav1";

    internal const string FFmpegMissingMessage =
        "FFmpeg is required for this application, but cannot be found on this machine. Converting video, and "
        + "opening any file that is not a .cbv, run through the 'ffmpeg' and 'ffprobe' commands. Install FFmpeg "
        + "so that 'ffmpeg' runs from a terminal - this application looks for it on its PATH - and then restart "
        + "the application.";

    internal const string FFprobeMissingMessage =
        "FFprobe is required for this application, but cannot be found on this machine. FFmpeg was found, but "
        + "'ffprobe' - which this application uses to look inside every file it opens that is not a .cbv - was "
        + "not. FFprobe normally comes with FFmpeg: install a complete FFmpeg so that 'ffprobe' also runs from a "
        + "terminal, and then restart the application.";

    internal const string SvtAv1MissingMessage =
        "The SVT-AV1 encoder is required for this application's conversions, but the FFmpeg on this machine does "
        + "not include it (its encoder list has no 'libsvtav1'). Files can still be opened and played, and "
        + "exporting to .mp4 still works, but importing, transcoding or exporting to MKV, WebM or CodeBrix .cbv "
        + "will fail. Install an FFmpeg build that includes SVT-AV1, and then restart the application.";

    private readonly Func<bool> usesExternalTools;
    private readonly Func<bool> ffmpegRuns;
    private readonly Func<bool> ffprobeRuns;
    private readonly Func<string, bool?> hasEncoder;

    /// <summary>Creates a check against the tools the registered engine offers.</summary>
    public ExternalToolCheck()
        : this(() => ProcessingEngine.HasExternalTools, () => ToolRuns("ffmpeg"), () => ToolRuns("ffprobe"), HasEncoder)
    {
    }

    /// <summary>Creates a check that asks its three questions of the functions given, for tests.</summary>
    /// <param name="ffmpegRuns">Whether ffmpeg runs.</param>
    /// <param name="ffprobeRuns">Whether ffprobe runs.</param>
    /// <param name="hasEncoder">Whether ffmpeg has the named encoder, or null when that could not be found out.</param>
    internal ExternalToolCheck(Func<bool> ffmpegRuns, Func<bool> ffprobeRuns, Func<string, bool?> hasEncoder)
        : this(() => true, ffmpegRuns, ffprobeRuns, hasEncoder)
    {
    }

    /// <summary>Creates a check that first asks whether the engine runs external tools at all, for tests.</summary>
    /// <param name="usesExternalTools">Whether the registered engine runs FFmpeg; when it does not, nothing else is asked.</param>
    /// <param name="ffmpegRuns">Whether ffmpeg runs.</param>
    /// <param name="ffprobeRuns">Whether ffprobe runs.</param>
    /// <param name="hasEncoder">Whether ffmpeg has the named encoder, or null when that could not be found out.</param>
    internal ExternalToolCheck(
        Func<bool> usesExternalTools, Func<bool> ffmpegRuns, Func<bool> ffprobeRuns, Func<string, bool?> hasEncoder)
    {
        this.usesExternalTools = usesExternalTools;
        this.ffmpegRuns = ffmpegRuns;
        this.ffprobeRuns = ffprobeRuns;
        this.hasEncoder = hasEncoder;
    }

    /// <inheritdoc />
    public Task<string> FindProblemAsync(CancellationToken cancellationToken)
    {
        //Each step starts a child process and waits for it, so none of it belongs on the UI thread.
        return Task.Run(() => FindProblem(cancellationToken), cancellationToken);
    }

    internal string FindProblem(CancellationToken cancellationToken)
    {
        if (!usesExternalTools())
        {
            return null;
        }

        if (!ffmpegRuns())
        {
            return FFmpegMissingMessage;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (!ffprobeRuns())
        {
            return FFprobeMissingMessage;
        }

        cancellationToken.ThrowIfCancellationRequested();

        //Only a definite "no" is reported. An encoder list that could not be read says nothing either way, and a
        //warning that might be wrong is worse than none: the conversion itself will say what went wrong.
        return hasEncoder(SvtAv1EncoderName) == false ? SvtAv1MissingMessage : null;
    }

    private static bool ToolRuns(string toolName)
    {
        try
        {
            return ProcessingEngine.TryGetExternalTools(out var tools)
                && tools.Run(new ExternalToolCommand(toolName, new ExternalToolArguments().Add("-version"))).Succeeded;
        }
        catch (Exception)
        {
            //Not found, not runnable, or not allowed to start: all of them mean the application cannot use it.
            return false;
        }
    }

    private static bool? HasEncoder(string encoderName)
    {
        try
        {
            if (!ProcessingEngine.TryGetExternalTools(out var tools))
            {
                return null;
            }

            var result = tools.Run(new ExternalToolCommand(
                "ffmpeg", new ExternalToolArguments().Add("-hide_banner").Add("-encoders")));
            if (!result.Succeeded)
            {
                return null;
            }

            //One encoder per line after the legend: six flag characters, a space, the name, the description.
            return result.StandardOutput
                .Split('\n')
                .Select(line => line.Trim().Split(' ', 3, StringSplitOptions.RemoveEmptyEntries))
                .Any(fields => fields.Length >= 2 && fields[0].Length == 6 && fields[0][0] == 'V' &&
                               string.Equals(fields[1], encoderName, StringComparison.Ordinal));
        }
        catch (Exception)
        {
            return null;
        }
    }
}
