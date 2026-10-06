using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BrixInvaders.Game.Session;
using BrixInvaders.Game.Settings;
using BrixInvaders.GameLogic;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.PlayTest;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace BrixInvaders.PlayTests;

public sealed partial class ApplicationTests : PageTest, IClassFixture<AppFixture>, IAsyncLifetime
{
    private readonly AppFixture _fixture;

    public ApplicationTests(AppFixture fixture) : base(fixture.Application) => _fixture = fixture;
    public async ValueTask InitializeAsync()
    {
        var test = (Xunit.v3.IXunitTest)TestContext.Current.Test;
        test.Traits.TryGetValue(PlayTestOrientationAttribute.CaseTraitName, out var caseOrientations);
        await _fixture.ResetAsync(PlayTestOrientationAttribute.Resolve(test.TestMethod.Method, caseOrientations));
    }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private GameSession Session => _fixture.Session;

    private Task<T> WaitAsync<T>(Func<T> probe, Func<T, bool> ready, string description, float? timeout = null) =>
        _fixture.WaitAsync(probe, ready, description, timeout);

    // The session's clock only moves while the engine steps the game.
    private async Task WaitForTheGameToStepAsync()
    {
        var time = await _fixture.ReadAsync(() => Session.Time);
        await WaitAsync(() => Session.Time, now => now > time, "the game stepping");
    }

    [Fact]
    public async Task Launch_starts_the_engine_and_reaches_the_title_after_the_splash()
    {
        await Expect(_fixture.Canvas).ToBeVisibleAsync();
        (await Page.EvaluateAsync(() => Engine.Instance.IsRunning)).Should().BeTrue();
        (await Page.EvaluateAsync(() => ReferenceEquals(_fixture.Host, _fixture.FirstHost))).Should().BeTrue();
        (await Page.EvaluateAsync(() => Session.CurrentScreen)).Should().Be(GameScreen.Title);
        var first = _fixture.ScreenChanges.First();
        (first.From, first.To).Should().Be((GameScreen.Splash, GameScreen.Title));
        _fixture.Log.Should().Contain(line => line.Contains("splash: composed"));
        await WaitForTheGameToStepAsync();
        var canvas = await _fixture.Canvas.BoundingBoxAsync();
        canvas.Width.Should().Be(_fixture.Application.Width);
        canvas.Height.Should().Be(_fixture.Application.Height);
    }

    [Fact]
    public async Task Startup_loads_every_kenney_asset_and_runs_on_the_cpu_tier()
    {
        (await Page.EvaluateAsync(() => _fixture.Host.Provider.CheckKeys(Assets.AssetKeyCatalog.AllKeys).MissingKeys.Count)).Should().Be(0);
        (await Page.EvaluateAsync(() => _fixture.Host.PackCredits.Count)).Should().Be(5);
        _fixture.Log.Should().Contain(line => line.Contains("render tier requested: CPU"));
        // The head has no OpenGL; the game logs the tier it really runs on once the first frame rate is measured.
        await WaitAsync(() => _fixture.Log.Any(line => line.Contains("render tier: CPU")), logged => logged, "the render tier line");
    }

    [Fact]
    public async Task Startup_reads_the_seeded_settings_and_high_scores_from_the_throwaway_store()
    {
        SettingsService.DirectoryPath.Should().Be(_fixture.SettingsDirectory);
        Directory.EnumerateFiles(_fixture.SettingsDirectory, "*.sqlite").Should().NotBeEmpty();
        _fixture.Log.Should().Contain(line => line.EndsWith($"settings store: {_fixture.SettingsDirectory}"));
        (await Page.EvaluateAsync(() => (Session.Screens.Difficulty, Session.Screens.ShipShape, Session.Screens.ShipColour, Session.Screens.LastName)))
            .Should().Be((AppFixture.SeededDifficulty, AppFixture.SeededShipShape, AppFixture.SeededShipColour, AppFixture.SeededName));
        (await Page.EvaluateAsync(() => Session.SettingsMenu.ValueOf(SettingsMenu.MasterVolumeRow))).Should().Be("50%");
        var best = await Page.EvaluateAsync(() => Session.HighScores.EntriesFor(AppFixture.SeededDifficulty).First());
        (best.Name, best.Score, best.Sector).Should().Be((AppFixture.SeededName, AppFixture.SeededScore, AppFixture.SeededSector));
        (await Page.EvaluateAsync(() => Session.HighScores.EntriesFor(Difficulty.Pilot).Count)).Should().Be(0);
        // The stored levels reach the music director right after it starts.
        _fixture.Music.Calls.Should().Contain($"Volumes {AppFixture.SeededMasterVolume:0.0} " +
            $"{SettingsService.DefaultMusicVolume:0.0} {SettingsService.DefaultEffectsVolume:0.0}");
    }

    [Fact]
    public void Startup_uses_the_registered_music_director_and_link_opener()
    {
        // The App(Action<IServiceCollection>) overload put both fakes in front of the game: no model, no browser.
        _fixture.Music.Starts.Should().Be(1);
        _fixture.Links.Opened.Should().BeEmpty();
    }

    [Fact]
    public async Task Key_presses_reach_the_focused_game_canvas()
    {
        var keys = 0;
        var canvas = await Page.EvaluateAsync(() => _fixture.GameCanvas);
        var handler = new KeyEventHandler((_, _) => keys++);
        await Page.EvaluateAsync(() => canvas.AddHandler(UIElement.KeyDownEvent, handler, true));
        try
        {
            (await Page.EvaluateAsync(() => FocusManager.GetFocusedElement(canvas.XamlRoot))).Should().BeSameAs(canvas);
            // W is one of the game's "any key" bindings: on the title it only resets the idle clock.
            await Page.Keyboard.PressAsync("W");
            (await Page.EvaluateAsync(() => keys)).Should().Be(1);
        }
        finally
        {
            await Page.EvaluateAsync(() => canvas.RemoveHandler(UIElement.KeyDownEvent, handler));
        }
    }

    [Fact]
    public async Task Mouse_click_on_the_title_counts_as_input()
    {
        await WaitAsync(() => Session.Screens.IdleTime, idle => idle > 0.5, "the title idling");
        await _fixture.HoldClickAsync(() => Session.Screens.IdleTime, idle => idle < 0.5, "the idle clock reset by the click");
        (await Page.EvaluateAsync(() => Session.CurrentScreen)).Should().Be(GameScreen.Title);
        (await Page.EvaluateAsync(() => Session.Screens.TitleCursor)).Should().Be(TitleMenuItem.Play);
        // The title has no links: a click there opens nothing.
        _fixture.Links.Opened.Should().BeEmpty();
    }

    [Fact]
    public async Task Title_idle_enters_attract_and_a_click_leaves()
    {
        await WaitAsync(() => Session.CurrentScreen, screen => screen == GameScreen.Attract, "attract mode after the idle time",
            (float)(ScreenStateMachine.AttractIdleSeconds + 10) * 1000);
        await WaitAsync(() => Session.Attract?.StepCount ?? 0, steps => steps > 0, "the demo game stepping");
        await _fixture.HoldClickAsync(() => Session.CurrentScreen, screen => screen == GameScreen.Title, "the title after a click");
        (await Page.EvaluateAsync(() => Session.Attract)).Should().BeNull();
    }

    [Fact]
    public async Task Rendered_frame_is_not_blank()
    {
        var box = await _fixture.Canvas.BoundingBoxAsync();
        var lit = await WaitForLitShareAsync(box.X, box.Y, box.Width, box.Height, share => share > 0.02);
        lit.Should().BeGreaterThan(0.02);
    }

    // The share of sampled pixels in a band that are clearly brighter than the near-black space colour.
    private static double LitShare(SKBitmap bitmap, double left, double top, double width, double height)
    {
        var lit = 0;
        var total = 0;
        for (var y = (int)top; y < (int)(top + height); y += 4)
            for (var x = (int)left; x < (int)(left + width); x += 4)
            {
                var color = bitmap.GetPixel(x, y);
                total++;
                if (color.Red > 40 || color.Green > 40 || color.Blue > 40) lit++;
            }
        return total == 0 ? 0 : (double)lit / total;
    }

    // The engine presents its frames on its own clock: take screenshots until one shows the band as expected.
    private async Task<double> WaitForLitShareAsync(double left, double top, double width, double height, Func<double, bool> ready)
    {
        var share = 0.0;
        for (var attempt = 0; attempt < 50; attempt++)
        {
            using var bitmap = SKBitmap.Decode(await Page.ScreenshotAsync());
            share = LitShare(bitmap, left, top, width, height);
            if (ready(share)) return share;
        }
        return share;
    }
}
