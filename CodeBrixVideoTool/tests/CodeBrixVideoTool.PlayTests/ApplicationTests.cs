using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Platform.UI.VideoPlayer.Skia;
using CodeBrix.Samples.PlayTests;
using CodeBrixVideoTool.Processing.Formats;
using CodeBrixVideoTool.Processing.Operations;
using CodeBrixVideoTool.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SilverAssertions;
using Xunit;

namespace CodeBrixVideoTool.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private const string Opening = "00:00  Opening";
    private const string Closing = "00:10  Closing";
    private const string English = "English (en)";

    private Locator Dialog => Page.GetByRole(AriaRole.Dialog);
    private Locator Row(string fileName) => Page.GetByTestId("LibraryList").GetByText(fileName, new() { Exact = true });

    private async Task OpenAsync(string path)
    {
        Fixture.Application.FilePickers.EnqueueOpenFile(path);
        await Button("Open file...").ClickAsync();
        await Expect(Page.GetByTestId("StatusText"))
            .ToHaveTextAsync(new Regex("^Opened " + Regex.Escape(Path.GetFileName(path)) + " - "));
    }
    // Opens a clip and waits until the player has read it and the transport is up.
    private async Task OpenClipAsync(string path)
    {
        await OpenAsync(path);
        await Expect(Page.GetByTestId("PlaybackStatus"))
            .ToHaveTextAsync(new Regex("^" + Regex.Escape(Path.GetFileName(path)) + " - 00:00:20"));
        await Expect(Button("Play")).ToBeEnabledAsync();
    }
    private async Task ChooseAsync(string testId, string option)
    {
        await Page.GetByTestId(testId).ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = option, Exact = true }).ClickAsync();
        await Expect(Page.GetByTestId(testId)).ToHaveValueAsync(option);
    }
    private async Task<string> RunToAsync(string fileName)
    {
        var output = Path.Combine(Fixture.DataDirectory, Guid.NewGuid().ToString("N")[..8], fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        Fixture.Application.FilePickers.EnqueueSaveFile(output);
        await Page.GetByTestId("ActionButton").ClickAsync();
        return output;
    }
    private Task<T> PlayerAsync<T>(Func<VideoPlayer, T> read) => Page.EvaluateAsync(() => read(Fixture.Player));
    private Task<T> WaitForPlayerAsync<T>(Func<VideoPlayer, T> probe, Func<T, bool> predicate, string description) =>
        Fixture.Application.WaitForAsync(() => probe(Fixture.Player), predicate, description: description);

    [Fact]
    public async Task Empty_library_shows_hint_and_disables_remove()
    {
        await Expect(Page.GetByTestId("EmptyLibraryHint")).ToBeVisibleAsync();
        await Expect(Button("Remove")).ToBeDisabledAsync();
        await Expect(Button("Open file...")).ToBeEnabledAsync();
        await Expect(Page.GetByText("No video open", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Button("Play")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("ActionButton")).ToHaveTextAsync("Convert");
        await Expect(Page.GetByTestId("ActionButton")).ToBeDisabledAsync();
        await Expect(Page.GetByTestId("RouteText")).ToHaveTextAsync("Select a file to convert.");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Open a video file to begin.");
        await Expect(Page.GetByTestId("PlaybackStatus")).ToHaveTextAsync("Open a file to play it.");
        await SnapshotAsync("CodeBrixVideoTool-empty");
    }

    [Fact]
    public async Task First_open_with_missing_ffmpeg_shows_dialog_then_continues()
    {
        Fixture.Tools.Problem = "FFmpeg was not found on this machine.";
        Fixture.Application.FilePickers.EnqueueOpenFile(Fixture.PlainClip);
        await Button("Open file...").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("FFmpeg was not found on this machine.");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("FFmpeg was not found on this machine.");
        Fixture.Application.FilePickers.OpenFileRequestCount.Should().Be(0);
        await Dialog.GetByRole(AriaRole.Button).First.ClickAsync();
        await Expect(Dialog).ToHaveCountAsync(0);
        await Expect(Row("plain.cbv")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex("^Opened plain.cbv - "));
        // The check is made once per session: a second Open neither checks nor warns.
        await OpenAsync(Fixture.RichClip);
        await Expect(Dialog).ToHaveCountAsync(0);
        Fixture.Tools.CheckCount.Should().Be(1);
    }

    [Fact]
    public async Task Cancelled_open_picker_leaves_library_unchanged()
    {
        Fixture.Application.FilePickers.EnqueueOpenFile(null);
        await Button("Open file...").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Application.FilePickers.OpenFileRequestCount,
            count => count == 1, description: "open picker request");
        await Expect(Button("Open file...")).ToBeEnabledAsync();
        await Expect(Page.GetByTestId("EmptyLibraryHint")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Open a video file to begin.");
        (await Page.EvaluateAsync(() => Fixture.Model.Library.Count)).Should().Be(0);
        Fixture.Probe.Probed.Should().BeEmpty();
    }

    [Fact]
    public async Task Opening_cbv_lists_it_shows_transport_and_presents_frames()
    {
        await OpenClipAsync(Fixture.PlainClip);
        await Expect(Row("plain.cbv")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("LibraryList").GetByText("Mode 2", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("EmptyLibraryHint")).ToBeHiddenAsync();
        await Expect(Page.GetByText("No video open", new() { Exact = true })).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("PlaybackStatus")).ToHaveTextAsync("plain.cbv - 00:00:20.");
        await Expect(Page.GetByTestId("DurationText")).ToHaveTextAsync("0:20");
        await Expect(Page.GetByTestId("PositionText")).ToHaveTextAsync("0:00");
        await Expect(Button("Pause")).ToBeDisabledAsync();
        await Expect(Button("Stop")).ToBeEnabledAsync();
        await Expect(Button("Remove")).ToBeEnabledAsync();
        await Expect(Page.GetByTestId("Chapter")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("Captions")).ToBeHiddenAsync();
        Fixture.Probe.Probed.Should().Equal(Fixture.PlainClip);
        await Button("Play").ClickAsync();
        await WaitForPlayerAsync(player => player.FrameStatistics.Presented, presented => presented > 2, "presented frames");
        await SnapshotAsync("CodeBrixVideoTool-playing");
    }

    [Fact]
    public async Task Play_advances_position_and_pause_holds_it()
    {
        await OpenClipAsync(Fixture.PlainClip);
        await Button("Play").ClickAsync();
        await Expect(Button("Pause")).ToBeEnabledAsync();
        await Expect(Button("Play")).ToBeDisabledAsync();
        await WaitForPlayerAsync(player => player.PositionSeconds, seconds => seconds >= 1, "position past one second");
        await Expect(Page.GetByTestId("PositionText")).Not.ToHaveTextAsync("0:00");
        await Button("Pause").ClickAsync();
        await Expect(Button("Play")).ToBeEnabledAsync();
        (await PlayerAsync(player => player.IsPlaying)).Should().BeFalse();
        var held = await PlayerAsync(player => player.PositionSeconds);
        held.Should().BeGreaterThanOrEqualTo(1);
        // Playing again resumes from where it was held rather than from the start.
        await Button("Play").ClickAsync();
        await WaitForPlayerAsync(player => player.PositionSeconds, seconds => seconds > held + 0.4, "resumed position");
        await Button("Pause").ClickAsync();
    }

    [Fact]
    public async Task Stop_returns_to_start_and_reports_stopped()
    {
        await OpenClipAsync(Fixture.PlainClip);
        await Button("Play").ClickAsync();
        await WaitForPlayerAsync(player => player.PositionSeconds, seconds => seconds >= 0.6, "position moving");
        await Button("Stop").ClickAsync();
        await Expect(Page.GetByTestId("PlaybackStatus")).ToHaveTextAsync("Stopped.");
        await WaitForPlayerAsync(player => player.PositionSeconds, seconds => seconds < 0.1, "rewound position");
        await Expect(Page.GetByTestId("PositionText")).ToHaveTextAsync("0:00");
        await Expect(Button("Play")).ToBeEnabledAsync();
        await Expect(Button("Pause")).ToBeDisabledAsync();
        (await PlayerAsync(player => player.IsPlaying)).Should().BeFalse();
    }

    [Fact]
    public async Task Chapter_and_caption_dropdowns_appear_only_when_file_has_them()
    {
        await OpenClipAsync(Fixture.PlainClip);
        await Expect(Page.GetByTestId("Chapter")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("Captions")).ToBeHiddenAsync();
        await OpenClipAsync(Fixture.RichClip);
        await Expect(Page.GetByTestId("PlaybackStatus"))
            .ToHaveTextAsync("rich.cbv - 00:00:20, 2 chapters, 1 caption track(s).");
        await Expect(Page.GetByTestId("Chapter")).ToHaveValueAsync(Opening);
        await Expect(Page.GetByTestId("Captions")).ToHaveValueAsync("Captions off");
        await ChooseAsync("Chapter", Closing);
        await WaitForPlayerAsync(player => player.PositionSeconds, seconds => seconds >= 10 && seconds < 11, "chapter seek");
        await ChooseAsync("Captions", English);
        (await PlayerAsync(player => player.SelectedCaptionTrack?.Name)).Should().Be("English");
        await ChooseAsync("Captions", "Captions off");
        (await PlayerAsync(player => player.SelectedCaptionTrack)).Should().BeNull();
        // Back to the plain clip: both selectors go away again.
        await Row("plain.cbv").ClickAsync();
        await Expect(Page.GetByTestId("PlaybackStatus")).ToHaveTextAsync("plain.cbv - 00:00:20.");
        await Expect(Page.GetByTestId("Chapter")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("Captions")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task Mp4_row_is_dimmed_and_player_shows_unplayable_notice()
    {
        var mp4 = Path.Combine(Fixture.DataDirectory, "phone.mp4");
        await File.WriteAllTextAsync(mp4, "fixture", TestContext.Current.CancellationToken);
        await OpenClipAsync(Fixture.PlainClip);
        await OpenAsync(mp4);
        await Expect(Page.GetByTestId("LibraryList").GetByText("MP4", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("This file is not played here", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("PlaybackStatus")).ToHaveTextAsync(
            "MP4 (H.264 + AAC) is not played in this application - import it to one of the four CodeBrix formats first.");
        await Expect(Button("Play")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("ActionButton")).ToHaveTextAsync("Import");
        (await Page.EvaluateAsync(() => RowOpacity("phone.mp4"))).Should().Be(0.45);
        (await Page.EvaluateAsync(() => RowOpacity("plain.cbv"))).Should().Be(1.0);
        await SnapshotAsync("CodeBrixVideoTool-mp4");
    }

    [Fact]
    public async Task Remove_selects_previous_file_and_reports_removal()
    {
        await OpenClipAsync(Fixture.PlainClip);
        await OpenClipAsync(Fixture.RichClip);
        await Button("Remove").ClickAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Removed rich.cbv from the list.");
        await Expect(Row("rich.cbv")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("PlaybackStatus")).ToHaveTextAsync("plain.cbv - 00:00:20.");
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedItem.FileName)).Should().Be("plain.cbv");
        await Button("Remove").ClickAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Removed plain.cbv from the list.");
        await Expect(Page.GetByTestId("EmptyLibraryHint")).ToBeVisibleAsync();
        await Expect(Button("Remove")).ToBeDisabledAsync();
        await Expect(Page.GetByText("No video open", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("PlaybackStatus")).ToHaveTextAsync("Open a file to play it.");
        File.Exists(Fixture.RichClip).Should().BeTrue();
    }

    [Theory]
    [InlineData("Matroska .mkv (AV1 + Opus)", "Transcode", "Av1", "Opus")]
    [InlineData("WebM .webm (AV1 + Opus)", "Transcode", "Av1", "Opus")]
    [InlineData("CodeBrix Mode 1 .cbv (AV1 + Opus)", "Transcode", "Av1", "Opus")]
    [InlineData("MP4 (H.264 + AAC)", "Export", "H264", "Aac")]
    public async Task Destination_choice_updates_action_label_codecs_and_route(
        string destination, string action, string video, string audio)
    {
        await OpenClipAsync(Fixture.PlainClip);
        await Expect(Page.GetByTestId("Destination")).ToHaveValueAsync("Matroska .mkv (AV1 + Opus)");
        await Page.GetByTestId("Destination").ClickAsync();
        // A Mode 2 source is never offered Mode 2 again.
        await Expect(Page.GetByRole(AriaRole.Option, new() { Name = "MP4 (H.264 + AAC)", Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Option, new() { Name = "CodeBrix Mode 2 .cbv (AV1 + Vorbis)", Exact = true }))
            .ToHaveCountAsync(0);
        (await Page.EvaluateAsync(() => Fixture.Model.Conversion.Destinations.Count)).Should().Be(4);
        await Page.GetByRole(AriaRole.Option, new() { Name = destination, Exact = true }).ClickAsync();
        await Expect(Page.GetByTestId("Destination")).ToHaveValueAsync(destination);
        await Expect(Page.GetByTestId("ActionButton")).ToHaveTextAsync(action);
        await Expect(Page.GetByTestId("VideoCodecText")).ToHaveTextAsync("Video: " + video);
        await Expect(Page.GetByTestId("AudioCodecText")).ToHaveTextAsync("Audio: " + audio);
        await Expect(Page.GetByTestId("RouteText")).ToHaveTextAsync(
            $"{action} plain.cbv to {destination} at its own size. The bespoke container is demultiplexed first, without re-encoding.");
        await Expect(Page.GetByTestId("Resolution")).ToHaveValueAsync(new Regex("64"));
    }

    [Fact]
    public async Task Quality_dropdown_offers_four_stops_defaulting_to_good()
    {
        await OpenClipAsync(Fixture.PlainClip);
        await Expect(Page.GetByTestId("Quality")).ToHaveValueAsync("Good");
        await Page.GetByTestId("Quality").ClickAsync();
        foreach (var stop in new[] { "Fair", "Good", "Better", "Best" })
            await Expect(Page.GetByRole(AriaRole.Option, new() { Name = stop, Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Conversion.QualityLevels.Count)).Should().Be(4);
        await Page.GetByRole(AriaRole.Option, new() { Name = "Best", Exact = true }).ClickAsync();
        await Expect(Page.GetByTestId("Quality")).ToHaveValueAsync("Best");
        (await Page.EvaluateAsync(() => Fixture.Model.Conversion.SelectedQuality)).Should().Be(QualityLevel.Best);
        await RunToAsync("plain.mkv");
        await Fixture.Application.WaitForAsync(() => Fixture.Runner.LastPlan, plan => plan != null, description: "conversion plan");
        Fixture.Runner.LastPlan.Quality.Should().Be(QualityLevel.Best);
    }

    [Fact]
    public async Task Run_uses_suggested_save_name_and_finished_output_joins_list()
    {
        await OpenClipAsync(Fixture.PlainClip);
        await ChooseAsync("Destination", "WebM .webm (AV1 + Opus)");
        var output = await RunToAsync("plain.webm");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(
            new Regex("^Wrote " + Regex.Escape(output) + @" \(0\.0 MB\) in 00:00\.$"));
        Fixture.Application.FilePickers.LastSuggestedFileName.Should().Be("plain.webm");
        Fixture.Runner.LastPlan.Destination.Should().Be(MediaFormatKind.WebM);
        Fixture.Runner.LastPlan.OutputPath.Should().Be(output);
        Fixture.Runner.LastPlan.Source.FileName.Should().Be("plain.cbv");
        await Expect(Row("plain.webm")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedItem.Path)).Should().Be(output);
        await Expect(Page.GetByTestId("ConversionProgress")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("ActionButton")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Cancelled_save_picker_reports_no_destination()
    {
        await OpenClipAsync(Fixture.PlainClip);
        Fixture.Application.FilePickers.EnqueueSaveFile(null);
        await Page.GetByTestId("ActionButton").ClickAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Cancelled - no destination was chosen.");
        Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(1);
        Fixture.Runner.RunCount.Should().Be(0);
        await Expect(Page.GetByTestId("ConversionProgress")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("ActionButton")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Library.Count)).Should().Be(1);
    }

    [Fact]
    public async Task Cancel_during_long_job_stops_it_and_reenables_controls()
    {
        Fixture.Runner.Reports = new[] { new ConversionProgress("Encoding", 1, 2, 40) };
        Fixture.Runner.Hold();
        await OpenClipAsync(Fixture.PlainClip);
        await RunToAsync("plain.mkv");
        await Expect(Button("Cancel")).ToBeEnabledAsync();
        await Expect(Page.GetByTestId("ProgressText")).ToHaveTextAsync("Encoding (1 of 2) - 40%");
        await Expect(Page.GetByTestId("ActionButton")).ToBeDisabledAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex("^Transcode plain.cbv"));
        await Button("Cancel").ClickAsync();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Cancelled.");
        await Expect(Button("Cancel")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("ActionButton")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Library.Count)).Should().Be(1);
        (await Page.EvaluateAsync(() => Fixture.Model.Conversion.ProgressPercent)).Should().Be(0);
    }

    [Theory]
    [InlineData("Encoding", 1, 40d, "Encoding (1 of 2) - 40%", 20d, false)]
    [InlineData("Muxing", 2, null, "Muxing (2 of 2)", 75d, true)]
    public async Task Progress_bar_tracks_reported_percent_then_hides(
        string stage, int number, double? percent, string text, double overall, bool indeterminate)
    {
        Fixture.Runner.Reports = new[] { new ConversionProgress(stage, number, 2, percent) };
        var hold = Fixture.Runner.Hold();
        await OpenClipAsync(Fixture.PlainClip);
        await RunToAsync("plain.mkv");
        await Expect(Page.GetByTestId("ConversionProgress")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("ProgressText")).ToHaveTextAsync(text);
        (await Page.EvaluateAsync(() => Fixture.Model.Conversion.ProgressPercent)).Should().Be(overall);
        (await Page.EvaluateAsync(() => Fixture.Model.Conversion.IsProgressIndeterminate)).Should().Be(indeterminate);
        hold.TrySetResult();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex("^Wrote "));
        await Expect(Page.GetByTestId("ConversionProgress")).ToBeHiddenAsync();
        await Expect(Button("Cancel")).ToBeHiddenAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Conversion.ProgressPercent)).Should().Be(100);
        (await Page.EvaluateAsync(() => Fixture.Model.Conversion.ProgressText)).Should().BeEmpty();
    }

    [Fact]
    public async Task Notes_panel_shows_profile_verdict_then_notes_and_clears_on_next_run()
    {
        const string Verdict = "Streamable profile: FAIL - the seek index is at the end (expected for a standard MKV)";
        Fixture.Runner.ProfileVerdict = "the seek index is at the end";
        Fixture.Runner.Notes = new[] { "Captions were carried across." };
        await OpenClipAsync(Fixture.PlainClip);
        await Expect(Page.GetByTestId("LastRunNotes")).ToBeHiddenAsync();
        await RunToAsync("plain.mkv");
        await Expect(Page.GetByTestId("LastRunNotes").GetByText(Verdict, new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("LastRunNotes").GetByText("Captions were carried across.", new() { Exact = true }))
            .ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Conversion.LastRunNotes.ToArray()))
            .Should().Equal(Verdict, "Captions were carried across.");
        await SnapshotAsync("CodeBrixVideoTool-notes");
        // The next run empties the panel the moment it starts; a run with nothing to say leaves it empty.
        Fixture.Runner.ProfileVerdict = null;
        Fixture.Runner.Notes = null;
        var hold = Fixture.Runner.Hold();
        await RunToAsync("again.webm");
        await Expect(Button("Cancel")).ToBeEnabledAsync();
        await Expect(Page.GetByTestId("LastRunNotes")).ToBeHiddenAsync();
        hold.TrySetResult();
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex("^Wrote "));
        await Expect(Page.GetByTestId("LastRunNotes")).ToBeHiddenAsync();
        Fixture.Runner.RunCount.Should().Be(2);
    }

    [Fact]
    public async Task Failed_conversion_puts_outcome_in_status_bar()
    {
        Fixture.Runner.Failure = "FFmpeg exited with code 1.";
        Fixture.Runner.Notes = new[] { "The partial output was deleted." };
        await OpenClipAsync(Fixture.PlainClip);
        await RunToAsync("plain.mkv");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync("Failed: FFmpeg exited with code 1.");
        await Expect(Page.GetByTestId("LastRunNotes").GetByText("The partial output was deleted.", new() { Exact = true }))
            .ToBeVisibleAsync();
        await Expect(Page.GetByTestId("ConversionProgress")).ToBeHiddenAsync();
        await Expect(Page.GetByTestId("ActionButton")).ToBeEnabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Library.Count)).Should().Be(1);
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedItem.FileName)).Should().Be("plain.cbv");
    }

    [Fact]
    public async Task Dragging_scrubber_seeks_to_where_it_is_released()
    {
        await OpenClipAsync(Fixture.PlainClip);
        var presented = await PlayerAsync(player => player.FrameStatistics.Presented);
        var scrubber = Page.GetByTestId("Scrubber");
        var box = await scrubber.BoundingBoxAsync();
        await scrubber.DragByAsync(box.Width / 2, 0, new() { Position = new() { X = 10, Y = box.Height / 2 } });
        // Positions tick every 150 ms and the drag lands wherever the thumb was released: a range, not a value.
        var landed = await WaitForPlayerAsync(player => player.PositionSeconds, seconds => seconds > 6 && seconds < 14,
            "dragged position");
        (await PlayerAsync(player => player.IsPlaying)).Should().BeFalse();
        // The seek reaches the screen while paused: a new frame is presented for the new position.
        await WaitForPlayerAsync(player => player.FrameStatistics.Presented, count => count > presented, "seek frame");
        await Expect(Page.GetByTestId("PositionText")).ToHaveTextAsync(new Regex(@"^0:(0[6-9]|1[0-3])$"));
        // Playing carries on from there rather than from the start.
        await Button("Play").ClickAsync();
        await WaitForPlayerAsync(player => player.PositionSeconds, seconds => seconds > landed + 0.4, "resumed after seek");
        await Button("Pause").ClickAsync();
    }

    [Fact]
    public async Task Mute_checkbox_and_volume_slider_reach_player()
    {
        await OpenClipAsync(Fixture.PlainClip);
        (await PlayerAsync(player => player.IsMuted)).Should().BeFalse();
        await Page.GetByTestId("Mute").CheckAsync();
        await WaitForPlayerAsync(player => player.IsMuted, muted => muted, "muted player");
        await Page.GetByTestId("Volume").PressAsync("Home");
        await WaitForPlayerAsync(player => player.Volume, volume => volume == 0, "volume at minimum");
        var box = await Page.GetByTestId("Volume").BoundingBoxAsync();
        await Page.GetByTestId("Volume").ClickAsync(new() { Position = new() { X = box.Width / 2, Y = box.Height / 2 } });
        await WaitForPlayerAsync(player => player.Volume, volume => volume > 0.35 && volume < 0.65, "volume near half");
        await Page.GetByTestId("Mute").UncheckAsync();
        await WaitForPlayerAsync(player => player.IsMuted, muted => !muted, "unmuted player");
    }

    [Fact]
    public async Task Controls_disabled_while_busy()
    {
        await OpenClipAsync(Fixture.PlainClip);
        var hold = Fixture.Probe.Hold();
        Fixture.Application.FilePickers.EnqueueOpenFile(Fixture.RichClip);
        await Button("Open file...").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Probe.Probed.Count, count => count == 2, description: "second probe");
        await Expect(Button("Open file...")).ToBeDisabledAsync();
        await Expect(Button("Remove")).ToBeDisabledAsync();
        hold.TrySetResult();
        await Expect(Row("rich.cbv")).ToBeVisibleAsync();
        await Expect(Button("Open file...")).ToBeEnabledAsync();
        await Expect(Button("Remove")).ToBeEnabledAsync();
        // A running conversion disables its own action until it finishes.
        hold = Fixture.Runner.Hold();
        await RunToAsync("rich.mkv");
        await Expect(Page.GetByTestId("ActionButton")).ToBeDisabledAsync();
        hold.TrySetResult();
        await Expect(Page.GetByTestId("ActionButton")).ToBeEnabledAsync();
    }

    // The opacity a file's row is really drawn at: the template's outermost element, named LibraryRow.
    private double? RowOpacity(string fileName)
    {
        var list = (ListView)Fixture.View.FindName("LibraryList");
        var item = Fixture.Model.Library.First(info => info.FileName == fileName);
        return list.ContainerFromItem(item) is DependencyObject container ? FindRow(container)?.Opacity : null;
    }

    private static FrameworkElement FindRow(DependencyObject node)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(node); index++)
        {
            var child = VisualTreeHelper.GetChild(node, index);
            if (child is FrameworkElement { Name: "LibraryRow" } row) return row;
            if (FindRow(child) is { } found) return found;
        }
        return null;
    }
}
