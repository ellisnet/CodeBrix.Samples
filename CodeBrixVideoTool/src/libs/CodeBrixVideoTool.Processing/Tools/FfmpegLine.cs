using CodeBrix.VideoProcessing.Core.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace CodeBrixVideoTool.Processing.Tools;

/// <summary>
/// Runs one <c>ffmpeg</c> command line through the registered engine's external tools, with progress,
/// cancellation and the end of what FFmpeg said when it failed.
/// </summary>
/// <remarks>
/// <para>
/// The line is this library's own and the engine only finds the tool and starts it - the contract's
/// external-tools facet exists for exactly that, so no FFmpeg package is referenced here. The caller owns
/// the vocabulary: every flag, every filter and every <c>-map</c> is spelled out where the line is built.
/// </para>
/// <para>
/// Progress comes from <c>-progress pipe:1</c>, which FFmpeg writes as one <c>key=value</c> per line on
/// standard output, rather than from the statistics line on standard error: that one is redrawn in place
/// with carriage returns and arrives in lumps. <c>-nostats</c> turns it off, so standard error carries only
/// what FFmpeg has to say, which is what a failure reports.
/// </para>
/// </remarks>
internal static class FfmpegLine
{
    private const string ToolName = "ffmpeg";
    private const int ErrorLinesKept = 5;

    /// <summary>Starts a line with the options every run here shares.</summary>
    /// <returns>The arguments, ready for inputs.</returns>
    internal static ExternalToolArguments Start() => new ExternalToolArguments()
        .Add("-hide_banner")
        .Add("-nostats")
        .Add("-progress pipe:1")
        .Add("-y");

    /// <summary>Runs the line.</summary>
    /// <param name="tools">The engine's external tools.</param>
    /// <param name="arguments">Everything after the tool's name.</param>
    /// <param name="duration">How long the media is, for the percentage; zero for no percentage.</param>
    /// <param name="percent">Receives 0-100 as FFmpeg gets through the media, on a background thread; or null.</param>
    /// <param name="cancellationToken">Kills FFmpeg and throws <see cref="OperationCanceledException" />.</param>
    /// <returns>Whether it exited cleanly, the line as a person would type it, and the end of its error stream.</returns>
    /// <exception cref="OperationCanceledException">The run was cancelled.</exception>
    /// <exception cref="CodeBrix.VideoProcessing.Core.VideoProcessingException">FFmpeg could not be found or started.</exception>
    internal static async Task<FfmpegRun> RunAsync(
        IExternalTools tools,
        ExternalToolArguments arguments,
        TimeSpan duration,
        Action<double> percent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tools);
        ArgumentNullException.ThrowIfNull(arguments);

        var errors = new Queue<string>();
        var command = new ExternalToolCommand(ToolName, arguments)
        {
            OnStandardOutputLine = line => ReportProgress(line, duration, percent),
            OnStandardErrorLine = line =>
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                lock (errors)
                {
                    errors.Enqueue(line.Trim());
                    while (errors.Count > ErrorLinesKept)
                    {
                        errors.Dequeue();
                    }
                }
            },
        };

        //The runner is synchronous and waits for the process, so it runs on a worker thread. Cancelling
        //kills FFmpeg and its children and surfaces as OperationCanceledException.
        var result = await Task.Run(() => tools.Run(command, cancellationToken), CancellationToken.None)
            .ConfigureAwait(false);

        string[] lastErrors;
        lock (errors)
        {
            lastErrors = errors.ToArray();
        }

        return new FfmpegRun(result.Succeeded, Describe(arguments), lastErrors);
    }

    /// <summary>The line as a person would type it: the tool's name and then its arguments.</summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>The line.</returns>
    internal static string Describe(ExternalToolArguments arguments) => ToolName + " " + arguments.Render();

    private static void ReportProgress(string line, TimeSpan duration, Action<double> percent)
    {
        if (percent is null || duration <= TimeSpan.Zero || string.IsNullOrEmpty(line))
        {
            return;
        }

        //out_time_ms is in MICROseconds despite its name, exactly like out_time_us beside it; an older
        //FFmpeg writes only the first. "N/A" before the first frame is skipped.
        var separator = line.IndexOf('=');
        if (separator <= 0)
        {
            return;
        }

        var key = line.AsSpan(0, separator);
        if (!key.SequenceEqual("out_time_us") && !key.SequenceEqual("out_time_ms"))
        {
            return;
        }

        if (long.TryParse(line.AsSpan(separator + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out var microseconds)
            && microseconds >= 0)
        {
            var reached = TimeSpan.FromTicks(microseconds * 10);
            percent(Math.Clamp(reached.TotalMilliseconds / duration.TotalMilliseconds * 100d, 0d, 100d));
        }
    }
}

/// <summary>What one FFmpeg run came to.</summary>
/// <param name="Succeeded">True when FFmpeg exited with 0.</param>
/// <param name="Command">The line as a person would type it.</param>
/// <param name="LastErrors">The last few lines FFmpeg wrote to its error stream.</param>
internal sealed record FfmpegRun(bool Succeeded, string Command, IReadOnlyList<string> LastErrors)
{
    /// <summary>The error lines as one sentence-sized string, for a failure message.</summary>
    public string ErrorText => string.Join(" ", LastErrors);
}
