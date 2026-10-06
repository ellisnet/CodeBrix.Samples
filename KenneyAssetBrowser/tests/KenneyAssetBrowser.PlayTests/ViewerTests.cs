using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using Xunit;

namespace KenneyAssetBrowser.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task Spritesheet_lists_regions_and_selecting_one_spotlights_it()
    {
        await OpenBundleFolderAsync(Fixture.PuzzleDirectory);
        await SearchAsync("spritesheet_default", 1);
        await OpenCellAsync("spritesheet_default");
        await Expect(Page.GetByText("SPRITES", new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.ViewerFacts.Single(f => f.Label == "Sprites").Value == Fixture.Model.AtlasRegions.Count.ToString("N0")))
            .Should().BeTrue();
        var row = Page.GetByRole(AriaRole.Button).Filter(new() { HasText = "22×22 at 27,338" });
        await row.ScrollIntoViewIfNeededAsync();
        await row.ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ImagePainter.HighlightRegion, region => region != null, description: "the spotlight");
        (await Page.EvaluateAsync(() => Fixture.Model.ImagePainter.HighlightRegion)).Should().Be(new SKRectI(27, 338, 49, 360));
        await row.ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.ImagePainter.HighlightRegion, region => region == null, description: "the cleared spotlight");
        await SnapshotAsync("KenneyAssetBrowser-spritesheet");
    }

    [Fact]
    public async Task Vector_asset_renders_rasterized_with_rendered_at_fact()
    {
        await OpenBundleFolderAsync(Fixture.PuzzleDirectory);
        await SearchAsync("puzzleAssets_vector.svg", 1);
        await OpenCellAsync("puzzleAssets_vector");
        await Expect(Page.GetByText(new Regex(@"^\d+ × \d+ px \(vector\)$"))).ToBeVisibleAsync();
        await Expect(Page.GetByText("vector art (rasterized) · scroll to zoom", new() { Exact = true })).ToBeVisibleAsync();
        var size = await Page.EvaluateAsync(() => Math.Max(Fixture.Model.ImagePainter.Bitmap.Width, Fixture.Model.ImagePainter.Bitmap.Height));
        size.Should().BeInRange(1, 1600);
    }

    [Fact]
    public async Task Flash_file_shows_no_preview_caption()
    {
        await OpenBundleFolderAsync(Fixture.PuzzleDirectory);
        await SearchAsync("puzzleAssets_vector.swf", 1);
        await OpenCellAsync("puzzleAssets_vector");
        await Expect(Page.GetByText("No preview is available for .swf files — the file is listed for reference.", new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.ViewerFacts.Single(f => f.Label == "File").Value)).Should().Be("puzzleAssets_vector.swf");
        await Expect(Button("Fit")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task License_button_shows_bundle_license_dialog_and_closes()
    {
        await OpenCatalogAsync();
        await Page.GetByTestId("ShowLicense").ClickAsync();
        await Expect(Dialog).ToContainTextAsync("CC0 test fixtures; generated locally.");
        await Expect(Dialog.GetByText("PlayTest synthetic assets — License", new() { Exact = true })).ToBeVisibleAsync();
        await CloseDialogAsync();
        await Expect(Page.GetByText("blue_tile", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Model_asset_reports_3d_preview_unavailable_and_lists_triangles()
    {
        await OpenBundleFolderAsync(Fixture.CharactersDirectory);
        await SearchAsync("character-a", 1);
        await OpenCellAsync("character-a");
        // The PlayTest head has no OpenGL; the operating-system probe behind the dialog can be slow.
        await Expect(Dialog).ToContainTextAsync("Status: InitializationFailed", new() { Timeout = 60000 });
        await Expect(Dialog.GetByText("3D Preview Unavailable", new() { Exact = true })).ToBeVisibleAsync();
        await CloseDialogAsync();
        await Expect(Page.GetByText("Triangles", new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.CurrentModel.TriangleCount)).Should().BeGreaterThan(0);
        (await Page.EvaluateAsync(() => Fixture.Model.ViewerFacts.Single(f => f.Label == "Triangles").Value == Fixture.Model.CurrentModel.TriangleCount.ToString("N0")))
            .Should().BeTrue();
    }

    [Fact]
    public async Task Animated_model_shows_animation_bar_and_play_toggle_changes_label()
    {
        await OpenBundleFolderAsync(Fixture.CharactersDirectory);
        await SearchAsync("character-b", 1);
        await OpenCellAsync("character-b");
        await Expect(Dialog).ToContainTextAsync("Status: InitializationFailed", new() { Timeout = 60000 });
        await CloseDialogAsync();
        await Expect(Page.GetByTestId("SelectedAnimation")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.SelectedAnimation)).Should().Be("idle");
        await Fixture.Application.WaitForAsync(() => Fixture.Model.CurrentAnimationClip?.Name, name => name == "idle", description: "the baked idle clip");
        await Button("Pause").ClickAsync();
        await Expect(Button("Play")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.IsAnimationPlaying)).Should().BeFalse();
        await Page.GetByTestId("SelectedAnimation").ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = "walk", Exact = true }).ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.CurrentAnimationClip?.Name, name => name == "walk", description: "the baked walk clip");
        await Button("Play").ClickAsync();
        await Expect(Button("Pause")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Audio_clip_enables_transport_and_loop_toggles_label()
    {
        var audio = await AttachFakeAudioAsync();
        await OpenBundleFolderAsync(Fixture.SoundsDirectory);
        await SearchAsync("laserSmall_000", 1);
        await OpenCellAsync("laserSmall_000");
        await Expect(Button("Play")).ToBeEnabledAsync();
        await Expect(Button("Stop")).ToBeEnabledAsync();
        await Fixture.Application.WaitForAsync(() => audio.LoadedLength, length => length == 7286, description: "the clip's bytes at the player");
        await Button("Loop: Off").ClickAsync();
        await Expect(Button("Loop: On")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => audio.Looping)).Should().BeTrue();
        await Button("Loop: On").ClickAsync();
        await Expect(Button("Loop: Off")).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => audio.Looping)).Should().BeFalse();
    }

    [Fact]
    public async Task Audio_play_after_end_rewinds_via_bridge()
    {
        var audio = await AttachFakeAudioAsync();
        await OpenBundleFolderAsync(Fixture.SoundsDirectory);
        await SearchAsync("laserSmall_000", 1);
        await OpenCellAsync("laserSmall_000");
        await Page.EvaluateAsync(() => audio.Calls.Clear());
        await Button("Play").ClickAsync();
        await Fixture.Application.WaitForAsync(() => string.Join(",", audio.Calls), calls => calls == "Play", description: "a plain play");
        // The clip runs to its end: the player parks at the duration and reports the end.
        await Page.EvaluateAsync(() =>
        {
            audio.Playing = false;
            audio.Position = audio.Duration;
            Fixture.Model.NotifyAudioPlaybackEnded();
        });
        await Button("Play").ClickAsync();
        await Fixture.Application.WaitForAsync(() => string.Join(",", audio.Calls), calls => calls == "Play,Seek 0,Play", description: "a rewind before the replay");
        // A paused clip resumes where it is.
        await Button("Pause").ClickAsync();
        await Button("Play").ClickAsync();
        await Fixture.Application.WaitForAsync(() => string.Join(",", audio.Calls), calls => calls == "Play,Seek 0,Play,Pause,Play", description: "a resume without a rewind");
    }

    [Fact]
    public async Task Back_from_audio_stops_playback_and_clears_clip()
    {
        var audio = await AttachFakeAudioAsync();
        await OpenBundleFolderAsync(Fixture.SoundsDirectory);
        await SearchAsync("laserSmall_000", 1);
        await OpenCellAsync("laserSmall_000");
        await Button("Play").ClickAsync();
        await Fixture.Application.WaitForAsync(() => audio.Playing, playing => playing, description: "playback");
        await Button("←").ClickAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.IsViewerActive, active => !active, description: "the browsing view");
        (await Page.EvaluateAsync(() => audio.Calls.Last())).Should().Be("Stop");
        (await Page.EvaluateAsync(() => audio.Playing)).Should().BeFalse();
        (await Page.EvaluateAsync(() => Fixture.Model.HasAudioClip)).Should().BeFalse();
        (await Page.EvaluateAsync(() => Fixture.Model.Cells.Count)).Should().Be(1);
    }

    [Fact]
    public async Task Zoom_out_and_wheel_change_scale_within_clamp()
    {
        await OpenCatalogAsync();
        await OpenCellAsync("blue_tile");
        for (var i = 0; i < 8; i++) await Button("−").ClickAsync();
        await Expect(Page.GetByText("25%", new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.ImagePainter.ZoomFactor)).Should().Be(0.25f);
        await Button("Fit").ClickAsync();
        await Expect(Page.GetByText("100%", new() { Exact = true })).ToBeVisibleAsync();
        for (var i = 0; i < 14; i++) await Button("+").ClickAsync();
        await Expect(Page.GetByText("1600%", new() { Exact = true })).ToBeVisibleAsync();
        await Button("Fit").ClickAsync();
        var canvas = await Page.GetByType<SKXamlCanvas>().BoundingBoxAsync();
        await Page.Mouse.MoveAsync(canvas.X + canvas.Width / 2, canvas.Y + canvas.Height / 2);
        await Page.Mouse.WheelAsync(0, -120);
        await Expect(Page.GetByText("125%", new() { Exact = true })).ToBeVisibleAsync();
        await Page.Mouse.WheelAsync(0, 120);
        await Page.Mouse.WheelAsync(0, 120);
        await Expect(Page.GetByText("80%", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Corrupt_image_entry_shows_could_not_open_dialog()
    {
        await OpenBundleFolderAsync(Fixture.EdgeCasesDirectory);
        await SearchAsync("broken", 1);
        await (await CellAsync("broken")).ClickAsync();
        await Expect(Dialog).ToContainTextAsync("Could not open “broken”.");
        await Expect(Dialog).ToContainTextAsync("The data is not a decodable image.");
        await CloseDialogAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.IsViewerActive)).Should().BeFalse();
    }

    [Fact]
    public async Task Font_asset_renders_specimen()
    {
        await OpenBundleFolderAsync(Fixture.EdgeCasesDirectory);
        await SearchAsync("Merriweather", 1);
        await OpenCellAsync("Merriweather");
        await Expect(Page.GetByText("font specimen · scroll to zoom", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Merriweather.ttf", new() { Exact = true })).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => (Fixture.Model.ImagePainter.Bitmap.Width, Fixture.Model.ImagePainter.Bitmap.Height))).Should().Be((1100, 760));
        await SnapshotAsync("KenneyAssetBrowser-font");
    }

    [Fact]
    public async Task Tiled_map_renders_composited()
    {
        await OpenBundleFolderAsync(Fixture.EdgeCasesDirectory);
        await SearchAsync("level", 1);
        await OpenCellAsync("level");
        await Expect(Page.GetByText("4 × 3 tiles (64 × 48 px)", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("16 × 16 px", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Tiled map (composited) · scroll to zoom", new() { Exact = true })).ToBeVisibleAsync();
        // The second tile (stone) sits at map cell (1, 1); the first (grass) fills the border.
        var stone = await Page.EvaluateAsync(() => Fixture.Model.ImagePainter.Bitmap.GetPixel(24, 24));
        var grass = await Page.EvaluateAsync(() => Fixture.Model.ImagePainter.Bitmap.GetPixel(8, 8));
        stone.Should().Be(SKColors.SlateGray);
        grass.Should().Be(SKColors.ForestGreen);
        await SnapshotAsync("KenneyAssetBrowser-tiled-map");
    }

    private async Task<FakeAudioBridge> AttachFakeAudioAsync()
    {
        var audio = new FakeAudioBridge();
        await Page.EvaluateAsync(() => audio.Attach(Fixture.Model));
        return audio;
    }
}
