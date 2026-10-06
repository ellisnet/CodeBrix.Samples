using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Audio;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.PlayTest;
using GameEngineMusicDemo.Game;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace GameEngineMusicDemo.PlayTests;

public sealed partial class ApplicationTests : PageTest, IClassFixture<AppFixture>, IAsyncLifetime
{
    private readonly AppFixture _fixture;
    private static MusicManager Music => MusicManager.Instance;

    public ApplicationTests(AppFixture fixture) : base(fixture.Application) => _fixture = fixture;
    public async ValueTask InitializeAsync()
    {
        var test = (Xunit.v3.IXunitTest)TestContext.Current.Test;
        test.Traits.TryGetValue(PlayTestOrientationAttribute.CaseTraitName, out var caseOrientations);
        await _fixture.ResetAsync(PlayTestOrientationAttribute.Resolve(test.TestMethod.Method, caseOrientations));
    }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private Locator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    // The control panel scrolls, so reveal each control before clicking it.
    private async Task ClickAsync(string name)
    {
        await Button(name).ScrollIntoViewIfNeededAsync();
        await Button(name).ClickAsync();
    }

    private async Task PressSliderAsync(string slider, string key)
    {
        await Page.GetByTestId(slider).ScrollIntoViewIfNeededAsync();
        await Page.GetByTestId(slider).PressAsync(key);
    }

    private Task<T> WaitAsync<T>(Func<T> probe, Func<T, bool> ready, string description, float? timeout = null) =>
        _fixture.Application.WaitForAsync(probe, ready, timeout, description);

    private async Task PlayAsync(string button, string key)
    {
        await ClickAsync(button);
        await WaitAsync(() => Music.NowPlaying?.Key, now => now == key, $"'{key}' playing");
        // The fade-in is over and the track's own clock is moving.
        var start = await WaitAsync(() => (Music.ActiveFadeCount, Music.NowPlaying.Volume, Music.NowPlaying.Position),
            state => state.ActiveFadeCount == 0 && state.Volume == 1f, $"'{key}' faded in", 5000);
        await WaitAsync(() => Music.NowPlaying.Position, position => position > start.Position, $"'{key}' advancing");
    }

    // A transition queued on the next bar is only observable while it waits, so queue it early in a bar,
    // never in the bar before the loop point. Returns the bar boundary it will wait for, in seconds.
    private async Task<double> WaitForEarlyInABarAsync()
    {
        var position = await WaitAsync(() => Music.NowPlaying.Position.TotalSeconds,
            seconds => seconds % 2.0 is > 0.1 and < 0.9 && seconds < 5.0, "early in a bar of Track A", 12000);
        return Math.Floor(position / 2.0) * 2.0 + 2.0;
    }

    [Fact]
    public async Task Startup_starts_the_engine_and_loads_every_track_without_problems()
    {
        await Expect(Page.GetByTestId("MasterSlider")).ToBeVisibleAsync();
        await Expect(Button("Play Track A (fade in)")).ToBeEnabledAsync();
        await Page.EvaluateAsync(() =>
        {
            Engine.Instance.IsRunning.Should().BeTrue();
            Engine.Instance.IsPaused.Should().BeFalse();
            _fixture.Demo.Stems.Stems.Select(stem => stem.Name).Should().Equal("pad", "bass", "lead");
            _fixture.Demo.SongStems.Stems.Select(stem => stem.Name).Should().Equal("Vocals", "Drums", "Bass");
            _fixture.Demo.MidiTrack.Problems.Should().BeEmpty();
            _fixture.Demo.SamplerTrack.Problems.Should().BeEmpty();
            _fixture.Demo.SongStems.Problems.Should().BeEmpty();
            Music.NowPlaying.Should().BeNull();
        });
    }

    [Fact]
    public void Startup_writes_the_generated_asset_set_beside_the_executable()
    {
        MusicAssetFactory.AssetDirectory.Should().Be(Path.Combine(AppContext.BaseDirectory, "GeneratedMusic"));
        var files = MusicAssetFactory.StemPaths.Concat(new[]
        {
            MusicAssetFactory.TrackAPath, MusicAssetFactory.TrackBPath, MusicAssetFactory.StingerPath,
            MusicAssetFactory.VoicePath, MusicAssetFactory.InstrumentPath, MusicAssetFactory.MidiPath,
            MusicAssetFactory.DecentSamplerPresetPath, MusicAssetFactory.TempoChangeMidiPath,
        });
        foreach (var file in files) new FileInfo(file).Length.Should().BeGreaterThan(0, file);
        Directory.EnumerateFiles(MusicAssetFactory.StemsExportFolder).Should().NotBeEmpty();
    }

    [Fact]
    public async Task Midi_files_report_their_own_grid_markers_and_tempo_change()
    {
        await Page.EvaluateAsync(() =>
        {
            var midi = _fixture.Demo.MidiTrack.Timeline;
            midi.BeatsPerMinute.Should().Be(MusicAssetFactory.BeatsPerMinute);
            midi.BeatsPerBar.Should().Be(MusicAssetFactory.BeatsPerBar);
            midi.HasTempoChanges.Should().BeFalse();
            midi.Markers.Select(marker => marker.Name).Should().Equal("verse", "chorus");
            midi.TryGetMarker("chorus", out var chorus).Should().BeTrue();
            chorus.Should().Be(TimeSpan.FromSeconds(4));

            var sampler = _fixture.Demo.SamplerTrack.Timeline;
            sampler.BeatsPerMinute.Should().Be(MusicAssetFactory.BeatsPerMinute);
            sampler.HasTempoChanges.Should().BeTrue();
            sampler.Markers.Select(marker => marker.Name).Should().Equal("fast", "slow");

            var song = _fixture.Demo.SongStems.Timeline;
            song.BeatsPerMinute.Should().Be(MusicAssetFactory.StemsBeatsPerMinute);
            song.HasTempoChanges.Should().BeTrue();
        });
    }

    [Theory]
    [InlineData("Play Track A (fade in)", "Track A")]
    [InlineData("Play adaptive stems", "Adaptive Stems")]
    [InlineData("Play MIDI theme (SFZ)", "MIDI Theme")]
    [InlineData("Play MIDI theme (Decent Sampler)", "Decent Sampler Theme")]
    [InlineData("Play the stems export", "Song Stems")]
    [InlineData("Play playlist (A then B)", "Track A")]
    public async Task Transport_buttons_play_their_track(string button, string key)
    {
        await PlayAsync(button, key);
        (await Page.EvaluateAsync(() => Music.IsPlaying && Music.NowPlaying.IsPlaying)).Should().BeTrue();
    }

    [Fact]
    public async Task Stop_fades_out_to_nothing_playing()
    {
        await PlayAsync("Play Track A (fade in)", "Track A");
        var track = await Page.EvaluateAsync(() => Music.NowPlaying);
        await ClickAsync("Stop (fade out)");
        (await Page.EvaluateAsync(() => Music.NowPlaying)).Should().BeNull();
        await WaitAsync(() => track.IsPlaying, playing => !playing, "Track A stopped after its fade out", 5000);
        (await Page.EvaluateAsync(() => Music.ActiveFadeCount)).Should().Be(0);
    }

    [Fact]
    public async Task Immediate_crossfade_switches_to_track_b()
    {
        await PlayAsync("Play Track A (fade in)", "Track A");
        var trackA = await Page.EvaluateAsync(() => Music.NowPlaying);
        await ClickAsync("Crossfade to B - immediately");
        await Page.EvaluateAsync(() =>
        {
            Music.NowPlaying.Key.Should().Be("Track B");
            Music.HasPendingTransition.Should().BeFalse();
            Music.ActiveFadeCount.Should().BeGreaterThan(0);
        });
        await WaitAsync(() => trackA.IsPlaying, playing => !playing, "Track A faded out", 5000);
        (await Page.EvaluateAsync(() => Music.NowPlaying.Volume)).Should().Be(1f);
    }

    [Fact]
    public async Task Bar_crossfade_is_queued_and_can_be_cancelled()
    {
        await PlayAsync("Play Track A (fade in)", "Track A");
        var boundary = await WaitForEarlyInABarAsync();
        await ClickAsync("Crossfade to B - on the next bar");
        (await Page.EvaluateAsync(() => (Music.HasPendingTransition, Music.NowPlaying.Key))).Should().Be((true, "Track A"));
        await ClickAsync("Cancel the queued transition");
        (await Page.EvaluateAsync(() => Music.HasPendingTransition)).Should().BeFalse();
        // Past the bar the transition would have waited for, Track A is still the one playing.
        await WaitAsync(() => Music.NowPlaying.Position.TotalSeconds, seconds => seconds > boundary + 0.25,
            "Track A past the next bar", 5000);
        (await Page.EvaluateAsync(() => Music.NowPlaying.Key)).Should().Be("Track A");
    }

    [Fact]
    public async Task Bar_crossfade_lands_on_track_b_within_one_bar()
    {
        await PlayAsync("Play Track A (fade in)", "Track A");
        var trackA = await Page.EvaluateAsync(() => Music.NowPlaying);
        var boundary = await WaitForEarlyInABarAsync();
        await ClickAsync("Crossfade to B - on the next bar");
        (await Page.EvaluateAsync(() => Music.HasPendingTransition)).Should().BeTrue();
        var landed = await WaitAsync(() => (Music.NowPlaying.Key, trackA.Position.TotalSeconds),
            state => state.Key == "Track B", "Track B after the next bar", 5000);
        landed.TotalSeconds.Should().BeInRange(boundary - 0.05, boundary + 0.5);
        (await Page.EvaluateAsync(() => Music.HasPendingTransition)).Should().BeFalse();
    }

    [Fact]
    public async Task Pause_freezes_the_engine_and_keeps_the_queued_transition()
    {
        await PlayAsync("Play Track A (fade in)", "Track A");
        await WaitForEarlyInABarAsync();
        await ClickAsync("Crossfade to B - on the next bar");
        await ClickAsync("Pause / Resume the engine");
        await Page.EvaluateAsync(() =>
        {
            Engine.Instance.IsPaused.Should().BeTrue();
            _fixture.Demo.IsPaused.Should().BeTrue();
            Music.HasPendingTransition.Should().BeTrue();
            Music.NowPlaying.Key.Should().Be("Track A");
            // The engine renders one forced frame on its way into the pause; the banner is written onto it.
            Engine.Instance.LastFrameBeforePause.Should().NotBeNull();
        });
        await ClickAsync("Pause / Resume the engine");
        (await Page.EvaluateAsync(() => Engine.Instance.IsPaused)).Should().BeFalse();
        await WaitAsync(() => Music.NowPlaying.Key, key => key == "Track B", "the queued transition after resuming", 5000);
    }

    [Theory]
    [InlineData("MasterSlider")]
    [InlineData("MusicSlider")]
    [InlineData("SfxSlider")]
    public async Task Master_music_and_sfx_sliders_drive_the_buses(string slider)
    {
        Func<float> bus = slider switch
        {
            "MasterSlider" => () => AudioMixer.MasterVolume,
            "MusicSlider" => () => AudioMixer.MusicVolume,
            _ => () => AudioMixer.SfxVolume,
        };
        await PressSliderAsync(slider, "Home");
        await WaitAsync(bus, volume => volume == 0f, slider + " bus at zero");
        (await Page.EvaluateAsync(() => _fixture.Slider(slider).Value)).Should().Be(0);
        await PressSliderAsync(slider, "ArrowRight");
        await WaitAsync(bus, volume => volume == 0.01f, slider + " bus one step up");
        await PressSliderAsync(slider, "End");
        await WaitAsync(bus, volume => volume == 1f, slider + " bus at full");
        (await Page.EvaluateAsync(() => _fixture.Slider(slider).Value)).Should().Be(100);
    }

    [Fact]
    public async Task Hold_and_release_duck_moves_the_duck_multiplier()
    {
        await PlayAsync("Play Track A (fade in)", "Track A");
        await ClickAsync("Hold a duck open");
        await WaitAsync(() => AudioMixer.MusicDuckMultiplier, duck => Math.Abs(duck - 0.2f) < 0.001f, "the held duck", 5000);
        await ClickAsync("Hold a duck open");
        await ClickAsync("Release the held duck");
        // Holding twice keeps one duck, so one release restores the music.
        await WaitAsync(() => AudioMixer.MusicDuckMultiplier, duck => duck == 1f, "the released duck", 5000);
    }

    [Fact]
    public async Task Playing_stems_syncs_the_stem_sliders()
    {
        // Another part of a game moves the layers; playing the set brings the sliders up to date.
        await Page.EvaluateAsync(() =>
        {
            _fixture.Demo.Stems[0].Gain = 0.25f;
            _fixture.Demo.Stems[1].Gain = 0.5f;
            _fixture.Demo.Stems[2].Gain = 0.75f;
        });
        await PlayAsync("Play adaptive stems", "Adaptive Stems");
        await Page.EvaluateAsync(() =>
        {
            _fixture.Slider("PadSlider").Value.Should().Be(25);
            _fixture.Slider("BassSlider").Value.Should().Be(50);
            _fixture.Slider("LeadSlider").Value.Should().Be(75);
            _fixture.Demo.Stems.Stems.Select(stem => stem.Gain).Should().Equal(0.25f, 0.5f, 0.75f);
        });
        await PressSliderAsync("BassSlider", "End");
        await WaitAsync(() => _fixture.Demo.Stems[1].Gain, gain => gain == 1f, "the bass layer at full");
        await PressSliderAsync("PadSlider", "Home");
        await WaitAsync(() => _fixture.Demo.Stems[0].Gain, gain => gain == 0f, "the pad layer out");
    }

    [Fact]
    public async Task Fade_lead_buttons_move_slider_and_layer_gain()
    {
        await PlayAsync("Play adaptive stems", "Adaptive Stems");
        await ClickAsync("Fade the lead in over 2s");
        // The slider jumps to the destination at once; the layer itself fades there over two seconds.
        await Page.EvaluateAsync(() =>
        {
            _fixture.Slider("LeadSlider").Value.Should().Be(100);
            _fixture.Demo.Stems[2].Gain.Should().BeLessThan(1f);
            Music.ActiveFadeCount.Should().BeGreaterThan(0);
        });
        await WaitAsync(() => _fixture.Demo.Stems[2].Gain, gain => gain == 1f, "the lead faded in", 5000);
        await ClickAsync("Fade the lead out over 2s");
        await Page.EvaluateAsync(() =>
        {
            _fixture.Slider("LeadSlider").Value.Should().Be(0);
            _fixture.Demo.Stems[2].Gain.Should().BeGreaterThan(0f);
        });
        await WaitAsync(() => _fixture.Demo.Stems[2].Gain, gain => gain == 0f, "the lead faded out", 5000);
    }

    [Theory]
    [InlineData("Drums")]
    [InlineData("Bass")]
    public async Task Song_stem_fades_work_by_name(string stem)
    {
        await PlayAsync("Play the stems export", "Song Stems");
        (await Page.EvaluateAsync(() => _fixture.Demo.SongStems[stem].Gain)).Should().Be(0f);
        await ClickAsync($"Fade the {stem} layer in");
        await WaitAsync(() => _fixture.Demo.SongStems[stem].Gain, gain => gain == 1f, stem + " faded in", 5000);
        await ClickAsync($"Fade the {stem} layer out");
        await WaitAsync(() => _fixture.Demo.SongStems[stem].Gain, gain => gain == 0f, stem + " faded out", 5000);
        (await Page.EvaluateAsync(() => _fixture.Demo.SongStems["Vocals"].Gain)).Should().Be(1f);
    }

    [Fact]
    public async Task Midi_harmony_layer_fades_out_and_in()
    {
        await PlayAsync("Play MIDI theme (SFZ)", "MIDI Theme");
        await ClickAsync("Fade the harmony channel out");
        await WaitAsync(() => _fixture.Demo.MidiTrack.GetLayerVolume(2), volume => volume == 0f, "the harmony faded out", 5000);
        (await Page.EvaluateAsync(() => _fixture.Demo.MidiTrack.GetLayerVolume(1))).Should().Be(1f);
        await ClickAsync("Fade the harmony channel in");
        await WaitAsync(() => _fixture.Demo.MidiTrack.GetLayerVolume(2), volume => volume == 1f, "the harmony faded in", 5000);
    }

    [Fact]
    public async Task Tempo_slider_sets_midi_speed()
    {
        await PlayAsync("Play MIDI theme (SFZ)", "MIDI Theme");
        await PressSliderAsync("SpeedSlider", "End");
        await WaitAsync(() => _fixture.Demo.MidiTrack.Speed, speed => speed == 2f, "double speed");
        await PressSliderAsync("SpeedSlider", "Home");
        await WaitAsync(() => _fixture.Demo.MidiTrack.Speed, speed => speed == 0.25f, "quarter speed");
        (await Page.EvaluateAsync(() => Music.NowPlaying.Key)).Should().Be("MIDI Theme");
    }

    [Fact]
    public async Task Jump_points_move_to_verse_and_chorus()
    {
        await PlayAsync("Play MIDI theme (SFZ)", "MIDI Theme");
        await WaitAsync(() => Music.NowPlaying.Position.TotalSeconds, seconds => seconds < 3.0, "before the chorus", 12000);
        await ClickAsync("Jump to 'chorus'");
        await WaitAsync(() => Music.NowPlaying.Position.TotalSeconds, seconds => seconds is >= 4.0 and < 6.0, "at the chorus");
        await ClickAsync("Jump to 'verse'");
        await WaitAsync(() => Music.NowPlaying.Position.TotalSeconds, seconds => seconds < 2.0, "at the verse");
    }

    [Fact]
    public async Task Stinger_and_dialogue_duck_then_recover()
    {
        await PlayAsync("Play Track A (fade in)", "Track A");
        await ClickAsync("Stinger (ducks the music)");
        await WaitAsync(() => AudioMixer.MusicDuckMultiplier, duck => duck < 1f, "the stinger's duck", 5000);
        await WaitAsync(() => AudioMixer.MusicDuckMultiplier, duck => duck == 1f, "recovery after the stinger", 10000);
        await ClickAsync("Dialogue line (ducks for its length)");
        await WaitAsync(() => AudioMixer.MusicDuckMultiplier, duck => duck < 1f, "the dialogue's duck", 5000);
        await WaitAsync(() => AudioMixer.MusicDuckMultiplier, duck => duck == 1f, "recovery after the line", 10000);
        (await Page.EvaluateAsync(() => Music.NowPlaying.Key)).Should().Be("Track A");
    }

    [Fact]
    public async Task Next_in_playlist_switches_track()
    {
        await PlayAsync("Play playlist (A then B)", "Track A");
        (await Page.EvaluateAsync(() => Music.Playlist.Tracks.Select(track => track.Key).ToArray())).Should().Equal("Track A", "Track B");
        await ClickAsync("Next in playlist");
        await WaitAsync(() => Music.NowPlaying.Key, key => key == "Track B", "Track B from the playlist");
        await WaitAsync(() => Music.ActiveFadeCount, fades => fades == 0, "the playlist crossfade", 5000);
        await ClickAsync("Next in playlist");
        await WaitAsync(() => Music.NowPlaying.Key, key => key == "Track A", "the playlist repeating");
    }

    [Fact]
    public async Task Engine_canvas_renders_a_non_blank_readout()
    {
        var canvas = Page.GetByType<GameSurfaceCanvas>();
        await Expect(canvas).ToBeVisibleAsync();
        var box = await canvas.BoundingBoxAsync();
        box.Width.Should().Be(_fixture.Application.Width - 360);
        box.Height.Should().Be(_fixture.Application.Height);
        // The engine paints a dark-blue readout panel with light text onto the canvas's black surface.
        var idle = await WaitForCanvasAsync(box, pixels => pixels.Panel > 10_000 && pixels.Text > 500, "the readout panel and its text");
        await ClickAsync("Play Track A (fade in)");
        // The readout is redrawn from the engine loop, so what it says changes once something plays.
        await WaitForCanvasAsync(box, pixels => pixels.Text != idle.Text, "the readout redrawn for Track A");
    }

    private async Task<(int Panel, int Text)> WaitForCanvasAsync(LocatorBoundingBoxResult box,
        Func<(int Panel, int Text), bool> ready, string description)
    {
        var elapsed = Stopwatch.StartNew();
        (int Panel, int Text) pixels;
        do
        {
            pixels = CountCanvasPixels(await Page.ScreenshotAsync(), box);
            if (ready(pixels)) return pixels;
        } while (elapsed.Elapsed < TimeSpan.FromSeconds(10));
        throw new PlayTestException($"Timed out waiting for {description}. Last observed pixels: {pixels}");
    }

    private static (int Panel, int Text) CountCanvasPixels(byte[] png, LocatorBoundingBoxResult box)
    {
        using var bitmap = SKBitmap.Decode(png);
        var panel = 0;
        var text = 0;
        for (var y = (int)box.Y; y < (int)(box.Y + box.Height); y += 2)
            for (var x = (int)box.X; x < (int)(box.X + box.Width); x += 2)
            {
                var color = bitmap.GetPixel(x, y);
                if (color.Red < 40 && color.Green < 40 && color.Blue is > 25 and < 70 && color.Blue > color.Red + 8) panel++;
                else if (color.Red > 150 && color.Green > 150 && color.Blue > 150) text++;
            }
        return (panel, text);
    }
}
