using CodeBrix.Platform.Simple;
using CodeBrix.VideoPlayback.Captions;
using CodeBrix.VideoPlayback.Chapters;
using CodeBrix.Platform.UI.VideoPlayer.Skia;
using CodeBrixVideoTool.Playback.Services;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace CodeBrixVideoTool.Views;

public sealed partial class MainPage : Page
{
    private VideoPlayerSurface surface;

    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);
            WireViewModel();
        };

        //Optional scripted run: import, play and report without anyone touching the window.
        if (SmokeOptions.FromEnvironment() is { } smoke)
        {
            Loaded += (_, _) => RunSmoke(smoke);
        }

        this.InitializeComponent(); //Leave this line last
    }

    private MainViewModel ViewModel => DataContext as MainViewModel;

    private void WireViewModel()
    {
        if (ViewModel is not { } viewModel)
        {
            return;
        }

        surface ??= new VideoPlayerSurface(Player);
        viewModel.Playback.AttachSurface(surface);
        viewModel.PickMediaFileAsync = PickMediaFileAsync;
        viewModel.Conversion.PickOutputPathAsync = PickOutputPathAsync;
    }

    #region | Player element events |

    private void Player_MediaOpened(object sender, EventArgs e) => surface?.RaiseMediaOpened();

    private void Player_PlaybackEnded(object sender, EventArgs e) => surface?.RaisePlaybackEnded();

    private void Player_MediaFailed(object sender, VideoPlayerFailedEventArgs e) => surface?.RaiseMediaFailed(e.Message);

    #endregion

    #region | Head-capability bridges |

    private static async Task<string> PickMediaFileAsync()
    {
        try
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.VideosLibrary
            };

            foreach (var extension in MediaFormats.ImportExtensions)
            {
                picker.FileTypeFilter.Add(extension);
            }

            picker.FileTypeFilter.Add(".mkv");
            picker.FileTypeFilter.Add(".webm");
            picker.FileTypeFilter.Add(".cbv");

            var file = await picker.PickSingleFileAsync();
            return file?.Path;
        }
        catch (NotSupportedException)
        {
            //A head with no windowing system registers no picker extensions.
            return null;
        }
    }

    private static async Task<string> PickOutputPathAsync(string suggestedFileName, string extension)
    {
        try
        {
            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.VideosLibrary,
                SuggestedFileName = suggestedFileName,
                DefaultFileExtension = extension
            };
            picker.FileTypeChoices.Add(DescribeExtension(extension), new List<string> { extension });

            var file = await picker.PickSaveFileAsync();
            return file?.Path;
        }
        catch (NotSupportedException)
        {
            //As above: no dialog here, so the caller writes beside the source instead.
            return null;
        }
    }

    private static string DescribeExtension(string extension) => extension switch
    {
        ".cbv" => "CodeBrix video",
        ".mkv" => "Matroska video",
        ".webm" => "WebM video",
        _ => "MP4 video",
    };

    #endregion

    #region | The player element behind the playback view model's interface |

    /// <summary>
    /// The player element, behind the small interface the playback view model drives it through.
    /// The element is a XAML control and can only live here; every decision about it lives in the
    /// view model.
    /// </summary>
    private sealed class VideoPlayerSurface : IVideoPlayerSurface
    {
        private readonly VideoPlayer player;

        internal VideoPlayerSurface(VideoPlayer player)
        {
            this.player = player;
            player.RegisterPropertyChangedCallback(
                VideoPlayer.IsPlayingProperty, (_, _) => PlayStateChanged?.Invoke(this, EventArgs.Empty));
            player.ChapterChanged += (_, _) => ChapterChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler MediaOpened;

        public event EventHandler PlaybackEnded;

        public event EventHandler<string> MediaFailed;

        public event EventHandler PlayStateChanged;

        public event EventHandler ChapterChanged;

        public TimeSpan Duration => player.Duration;

        public bool IsPlaying => player.IsPlaying;

        public IReadOnlyList<Chapter> Chapters => player.Chapters;

        public IReadOnlyList<CaptionTrack> CaptionTracks => player.CaptionTracks;

        public int CurrentChapterIndex => player.CurrentChapter?.Index ?? -1;

        public void Open(string path)
        {
            //The source has to be unloaded before anything read at open time is changed, and the
            //real path comes last.
            player.Source = "";
            player.AutoPlay = false;
            player.Source = path;
        }

        public void Close() => player.Source = "";

        public void Play() => player.Play();

        public void Pause() => player.Pause();

        public void Stop() => player.Stop();

        public void SeekToChapter(int index) => player.SeekToChapter(index);

        public void SelectCaptionTrack(CaptionTrack track) => player.SelectedCaptionTrack = track;

        internal void RaiseMediaOpened() => MediaOpened?.Invoke(this, EventArgs.Empty);

        internal void RaisePlaybackEnded() => PlaybackEnded?.Invoke(this, EventArgs.Empty);

        internal void RaiseMediaFailed(string message) => MediaFailed?.Invoke(this, message);
    }

    #endregion

    #region | Smoke mode |

    /// <summary>What a scripted run asked for. Null when no scripted run was asked for at all.</summary>
    private sealed record SmokeOptions(MediaFormatKind Destination, string WorkFolder, bool KeepFiles, int HoldSeconds)
    {
        public static SmokeOptions FromEnvironment()
        {
            var requested = Environment.GetEnvironmentVariable("CODEBRIXVIDEOTOOL_SMOKE");
            if (string.IsNullOrWhiteSpace(requested))
            {
                return null;
            }

            var destination = requested.Trim().ToLowerInvariant() switch
            {
                "mode1" => MediaFormatKind.CodeBrixMode1,
                "webm" => MediaFormatKind.WebM,
                "mkv" or "matroska" => MediaFormatKind.Matroska,
                _ => MediaFormatKind.CodeBrixMode2,
            };

            var folder = Environment.GetEnvironmentVariable("CODEBRIXVIDEOTOOL_SMOKE_OUT");
            if (string.IsNullOrWhiteSpace(folder))
            {
                folder = Path.Combine(Path.GetTempPath(), "CodeBrixVideoTool.Smoke", Guid.NewGuid().ToString("N"));
            }

            var keep = Environment.GetEnvironmentVariable("CODEBRIXVIDEOTOOL_SMOKE_KEEP");
            _ = int.TryParse(Environment.GetEnvironmentVariable("CODEBRIXVIDEOTOOL_SMOKE_HOLD"), out var hold);

            return new SmokeOptions(destination, folder, keep is "1" or "true", Math.Clamp(hold, 0, 300));
        }
    }

    private static void Fact(string name, object value) =>
        Console.WriteLine($"CBVT-SMOKE: {name}={value?.ToString() ?? "(null)"}");

    private static void Finish(int failures)
    {
        Console.WriteLine($"CBVT-SMOKE: RESULT {(failures == 0 ? "PASS" : $"FAIL ({failures})")}");
        Console.Out.Flush();
        Environment.Exit(failures == 0 ? 0 : 1);
    }

    private async void RunSmoke(SmokeOptions options)
    {
        var failures = 0;

        void Check(string step, bool ok, string detail)
        {
            Console.WriteLine($"CBVT-SMOKE: {(ok ? "PASS" : "FAIL")} {step} ({detail})");
            if (!ok)
            {
                failures++;
            }
        }

        try
        {
            var viewModel = ViewModel;
            Check("view-model-ready", viewModel is not null, viewModel is null ? "no data context" : "ready");
            if (failures > 0)
            {
                Finish(failures);
                return;
            }

            Directory.CreateDirectory(options.WorkFolder);
            Fact("destination", options.Destination);
            Fact("workFolder", options.WorkFolder);

            var sourcePath = await CodeBrixVideoTool.Processing.Samples.SampleClipFactory
                .WriteRichMp4Async(options.WorkFolder, TimeSpan.FromSeconds(3));
            Fact("source", sourcePath);

            var info = await viewModel.AddAsync(sourcePath, System.Threading.CancellationToken.None);
            Check("source-probed", info is not null, info?.Summary ?? viewModel.StatusText);
            if (failures > 0)
            {
                Finish(failures);
                return;
            }

            Fact("sourceFormat", info.Format);
            Fact("sourceCaptionTracks", info.CaptionTrackCount);
            Fact("sourceChapters", info.ChapterCount);
            Check("mp4-is-not-playable-here", !info.IsPlayable, "the player decodes AV1 only");

            var destination = viewModel.Conversion.Destinations.FirstOrDefault(d => d.Kind == options.Destination);
            Check("destination-offered", destination is not null, options.Destination.ToString());
            if (failures > 0)
            {
                Finish(failures);
                return;
            }

            viewModel.Conversion.SelectedDestination = destination;
            Fact("action", viewModel.Conversion.ActionLabel);
            Fact("resolutions", viewModel.Conversion.Resolutions.Count);

            var outputPath = Path.Combine(
                options.WorkFolder, "smoke" + MediaFormats.Extension(options.Destination));
            viewModel.Conversion.PickOutputPathAsync = (_, _) => Task.FromResult(outputPath);

            var finished = new TaskCompletionSource<Processing.Operations.ConversionOutcome>();
            void OnFinished(object _, Processing.Operations.ConversionOutcome result) => finished.TrySetResult(result);
            viewModel.Conversion.ConversionFinished += OnFinished;
            viewModel.Conversion.RunCommand.Execute(null);
            var outcome = await finished.Task;
            viewModel.Conversion.ConversionFinished -= OnFinished;

            Check("conversion-succeeded", outcome.Succeeded, outcome.Failure ?? outcome.ToString());
            if (failures > 0)
            {
                Finish(failures);
                return;
            }

            Fact("output", outcome.OutputPath);
            Fact("outputBytes", outcome.SizeInBytes);
            Fact("profileVerdict", outcome.ProfileVerdict ?? "(not checked)");
            Fact("elapsedSeconds", outcome.Elapsed.TotalSeconds.ToString("F1"));
            Check("streamable-profile-passes", outcome.PassesProfile, outcome.ProfileVerdict ?? "(not checked)");

            //The library add and the player open both happen off the conversion-finished event, so
            //give them a moment to land before asking the player anything.
            for (var attempt = 0; attempt < 100 && Player.DurationSeconds <= 0; attempt++)
            {
                await Task.Delay(100);
            }

            var produced = viewModel.SelectedItem;
            Check("output-added-to-the-list", produced is not null && produced.Path == outcome.OutputPath,
                produced?.FileName ?? "(nothing selected)");

            if (produced is not null)
            {
                Fact("outputFormat", produced.Format);
                Fact("outputAudioCodec", produced.AudioCodec);
                Fact("outputVideoCodec", produced.VideoCodec);

                if (options.Destination == MediaFormatKind.CodeBrixMode2)
                {
                    Check("mode2-audio-is-vorbis", produced.AudioCodec == "vorbis", produced.AudioCodec);
                }
            }

            Check("player-opened", Player.DurationSeconds > 0,
                $"DurationSeconds={Player.DurationSeconds:F2}");

            Fact("chapters", Player.Chapters.Count);
            Fact("captionTracks", Player.CaptionTracks.Count);
            Check("chapters-survived", Player.Chapters.Count == info.ChapterCount,
                $"{Player.Chapters.Count} of {info.ChapterCount}");
            Check("captions-survived", Player.CaptionTracks.Count == info.CaptionTrackCount,
                $"{Player.CaptionTracks.Count} of {info.CaptionTrackCount}");

            var startPosition = Player.PositionSeconds;
            viewModel.Playback.PlayCommand.Execute(null);
            await Task.Delay(3000);
            var endPosition = Player.PositionSeconds;

            Fact("positionAtStart", startPosition.ToString("F2"));
            Fact("positionAtEnd", endPosition.ToString("F2"));
            Check("position-advances", endPosition - startPosition > 1.0,
                $"advanced {endPosition - startPosition:F2} s in 3 s");

            var statistics = Player.FrameStatistics;
            Fact("framesPosted", statistics.Posted);
            Fact("framesPresented", statistics.Presented);
            Fact("framesDropped", statistics.Dropped);
            Check("frames-were-presented", statistics.Presented > 0, statistics.Presented.ToString());

            viewModel.Playback.StopCommand.Execute(null);

            //Leaves the window up with everything loaded, for a scripted run that wants to look at
            //it rather than only read what it printed.
            if (options.HoldSeconds > 0)
            {
                Fact("holdingSeconds", options.HoldSeconds);
                viewModel.Playback.PlayCommand.Execute(null);
                await Task.Delay(options.HoldSeconds * 1000);
            }

            if (!options.KeepFiles)
            {
                viewModel.Playback.Close();
                await Task.Delay(200);
                TryDeleteFolder(options.WorkFolder);
            }

            Finish(failures);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"CBVT-SMOKE: RESULT FAIL (exception: {exception.Message})");
            Console.Out.Flush();
            Environment.Exit(1);
        }
    }

    private static void TryDeleteFolder(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch (IOException)
        {
            //A temporary folder that will not delete is not worth failing a scripted run over.
        }
    }

    #endregion
}
