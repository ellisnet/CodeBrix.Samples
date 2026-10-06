using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Platform.UI.VideoPlayer.Skia;
using CodeBrix.Samples.PlayTests;
using CodeBrix.VideoPlayback.Captions;
using CodeBrix.VideoPlayback.Chapters;
using CodeBrix.VideoPlayback.Codecs;
using CodeBrix.VideoPlayback.Containers.Cbv;
using CodeBrix.VideoPlayback.Decoding;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Operations;
using CodeBrixVideoTool.Processing.Planning;
using CodeBrixVideoTool.Processing.Probing;
using CodeBrixVideoTool.Processing.Tools;
using CodeBrixVideoTool.ViewModels;
using CodeBrixVideoTool.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace CodeBrixVideoTool.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    public ProbeFixture Probe { get; } = new();
    public RunnerFixture Runner { get; } = new();
    public ToolCheckFixture Tools { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    // Read on the UI thread only (inside EvaluateAsync / WaitForAsync).
    public VideoPlayer Player => (VideoPlayer)View.FindName("Player");
    // Twenty seconds of uncompressed video, no soundtrack: no codec, no FFmpeg and no audio device.
    public string PlainClip { get; private set; }
    // The same picture with two chapters and an English caption track.
    public string RichClip { get; private set; }

    protected override void Prepare()
    {
        // A scripted smoke run ends the process with Environment.Exit; it must never start under test.
        Environment.SetEnvironmentVariable("CODEBRIXVIDEOTOOL_SMOKE", null);
        PlainClip = ClipWriter.Write(Path.Combine(DataDirectory, "plain.cbv"), false);
        RichClip = ClipWriter.Write(Path.Combine(DataDirectory, "rich.cbv"), true);
    }

    protected override Application CreateApplication() => new App(services => services
        .AddSingleton<IMediaProbe>(Probe)
        .AddSingleton<IConversionRunner>(Runner)
        .AddSingleton<IExternalToolCheck>(Tools));

    protected override async Task BeforeResetAsync()
    {
        Probe.Reset();
        Runner.Reset();
        Tools.Reset();
        // The old page's player owns decode threads; release them before the next page arrives.
        if (View != null) await Application.EvaluateAsync(() => Player?.Close());
    }
}

// .cbv files go to the real managed probe (no ffprobe); every other file gets a fixed answer, as
// ffprobe would give for a 1080p clip with stereo sound.
public sealed class ProbeFixture : IMediaProbe
{
    private readonly MediaProbe _real = new();
    private TaskCompletionSource _hold;

    public List<string> Probed { get; } = new();

    public TaskCompletionSource Hold() => _hold = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Reset()
    {
        _hold?.TrySetResult();
        _hold = null;
        Probed.Clear();
    }

    public async Task<SourceMediaInfo> ProbeAsync(string path, CancellationToken cancellationToken)
    {
        Probed.Add(path);
        var hold = _hold;
        await Task.Yield();
        if (hold != null) await hold.Task.WaitAsync(cancellationToken);
        var format = MediaFormats.Detect(path);
        if (MediaFormats.IsCodeBrixContainer(format)) return await _real.ProbeAsync(path, cancellationToken);
        return new SourceMediaInfo
        {
            Path = path, FileName = Path.GetFileName(path), Format = format,
            Duration = TimeSpan.FromSeconds(20), Width = 1920, Height = 1080, FrameRate = 30,
            VideoCodec = format == MediaFormatKind.Mp4 ? "h264" : "av1",
            AudioCodec = format == MediaFormatKind.Mp4 ? "aac" : "opus",
            AudioChannels = 2, AudioSampleRateHz = 48000, SizeInBytes = new FileInfo(path).Length,
        };
    }
}

// Records the plan, reports scripted progress, optionally holds until released or cancelled, and
// writes a small marker file where a real conversion would have written its result.
public sealed class RunnerFixture : IConversionRunner
{
    private TaskCompletionSource _hold;

    public ConversionPlan LastPlan { get; set; }
    public int RunCount { get; set; }
    public IReadOnlyList<ConversionProgress> Reports { get; set; }
    // Reported as a failed outcome in place of success; null keeps the default successful outcome.
    public string Failure { get; set; }
    public string ProfileVerdict { get; set; }
    public bool PassesProfile { get; set; }
    public IReadOnlyList<string> Notes { get; set; }

    public TaskCompletionSource Hold() => _hold = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Reset()
    {
        _hold?.TrySetResult();
        _hold = null;
        LastPlan = null;
        RunCount = 0;
        Reports = null;
        Failure = ProfileVerdict = null;
        PassesProfile = false;
        Notes = null;
    }

    public async Task<ConversionOutcome> RunAsync(
        ConversionPlan plan, IProgress<ConversionProgress> progress, CancellationToken cancellationToken)
    {
        LastPlan = plan;
        RunCount++;
        var hold = _hold;
        await Task.Yield();
        foreach (var report in Reports ?? Array.Empty<ConversionProgress>()) progress?.Report(report);
        if (hold != null)
        {
            try { await hold.Task.WaitAsync(cancellationToken); }
            catch (OperationCanceledException) { return ConversionOutcome.Cancelled(TimeSpan.Zero, []); }
        }
        if (Failure != null) return ConversionOutcome.Failed(Failure, TimeSpan.Zero, Notes ?? []);
        File.WriteAllText(plan.OutputPath, "fixture output");
        return ConversionOutcome.Success(plan.OutputPath, new FileInfo(plan.OutputPath).Length, TimeSpan.Zero,
            ProfileVerdict, PassesProfile, Notes ?? [], ["ffmpeg (fixture)"]);
    }
}

public sealed class ToolCheckFixture : IExternalToolCheck
{
    // The sentence the check reports; null means every tool was found.
    public string Problem { get; set; }
    public int CheckCount { get; set; }

    public void Reset()
    {
        Problem = null;
        CheckCount = 0;
    }

    public Task<string> FindProblemAsync(CancellationToken cancellationToken)
    {
        CheckCount++;
        return Task.FromResult(Problem);
    }
}

// Writes a bespoke (Mode 2) .cbv with the playback core's managed muxer: uncompressed 4:2:0 frames,
// which the core decodes itself, so nothing here needs FFmpeg, a codec package or a binary fixture.
public static class ClipWriter
{
    public const int Width = 64;
    public const int Height = 36;
    public const int FramesPerSecond = 5;
    public const int Seconds = 20;

    public static string Write(string path, bool chaptersAndCaptions)
    {
        var descriptor = new RawVideoDescriptor(Width, Height, 8, VideoPixelLayout.I420, VideoColorInfo.Unspecified);
        var frameDuration = TimeSpan.FromSeconds(1d / FramesPerSecond);
        using var muxer = CbvMuxer.Create(path);
        var track = muxer.AddVideoTrack(VideoCodecIds.Raw, RawVideoFormat.CreateDescriptor(in descriptor),
            Width, Height, 0, 0, 8, VideoPixelLayout.I420, VideoColorInfo.Unspecified, null, frameDuration,
            "", "", CbvTrackFlags.Default);
        if (chaptersAndCaptions)
        {
            muxer.AddChapters(new[]
            {
                new Chapter(0, TimeSpan.Zero, TimeSpan.FromSeconds(10), false, new Dictionary<string, string> { [""] = "Opening" }),
                new Chapter(1, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(Seconds), false, new Dictionary<string, string> { [""] = "Closing" }),
            });
            var vtt = new StringBuilder("WEBVTT\n\n");
            vtt.Append("00:00:00.000 --> 00:00:09.000\nFirst caption\n\n");
            vtt.Append("00:00:10.000 --> 00:00:19.000\nSecond caption\n\n");
            muxer.AddCaptionTrack(CaptionFiles.ParseWebVtt(vtt.ToString(), 1, "en", "English", CaptionTrackFlags.None));
        }
        var frame = new byte[RawVideoFormat.GetFrameByteCount(in descriptor)];
        var luma = Width * Height;
        for (var index = 0; index < FramesPerSecond * Seconds; index++)
        {
            // A grey ramp over time with neutral chroma: every frame differs from the one before.
            Array.Fill(frame, (byte)(40 + index % 180), 0, luma);
            Array.Fill(frame, (byte)128, luma, frame.Length - luma);
            muxer.WriteChunk(track, frame, frameDuration * index, frameDuration, true);
        }
        muxer.Complete();
        return path;
    }
}
