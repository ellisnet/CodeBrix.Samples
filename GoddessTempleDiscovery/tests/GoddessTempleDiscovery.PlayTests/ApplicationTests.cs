using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using GoddessTempleDiscovery.Game.Bridges;
using GoddessTempleDiscovery.Game.Session;
using GoddessTempleDiscovery.Game.Settings;
using GoddessTempleDiscovery.Rules.Content;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.ViewModels;
using GoddessTempleDiscovery.Views;
using SilverAssertions;
using Xunit;
using Microsoft.UI.Xaml.Controls;
using GameLoop = CodeBrix.Platform.GameEngine.Engine;

namespace GoddessTempleDiscovery.PlayTests;

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

    private const int DefaultSeed = 7;

    private MainViewModel Model => _fixture.Model;
    private MainPage View => _fixture.View;
    private TableSession Session => _fixture.Session;
    private GameState State => _fixture.Session.Engine.State;

    private Locator Id(string automationId) => Page.GetByTestId(automationId);

    private Task<T> ReadAsync<T>(Func<T> probe) => _fixture.ReadAsync(probe);

    private Task<T> OnEngineAsync<T>(Func<T> probe) => _fixture.OnEngineAsync(probe);

    private Task<T> WaitAsync<T>(Func<T> probe, Func<T, bool> ready, string description, float? timeout = null) =>
        _fixture.WaitAsync(probe, ready, description, timeout);

    private Task<T> WaitOnEngineAsync<T>(Func<T> probe, Func<T, bool> ready, string description, float timeout = 20000) =>
        _fixture.WaitOnEngineAsync(probe, ready, description, timeout);

    private Task SnapshotAsync(string name) => Page.ScreenshotAsync(new()
    {
        Path = System.IO.Path.Combine("TestResults", "PlayTest", name + ".png"),
    });

    // ------------------------------------------------------------------ the setup pane

    private Task<int> SeatCountAsync() => ReadAsync(() => Model.Seats.Count);

    private async Task OpenSetupAsync()
    {
        await Id("NewGame").ClickAsync();
        await Expect(Id("SetupPane")).ToBeVisibleAsync();
    }

    private async Task SetSeatCountAsync(int seats)
    {
        while (await SeatCountAsync() < seats)
        {
            var before = await SeatCountAsync();
            await Id("AddSeat").ClickAsync();
            await WaitAsync(() => Model.Seats.Count, count => count == before + 1, "a seat added");
        }

        while (await SeatCountAsync() > seats)
        {
            var before = await SeatCountAsync();
            await Id("SeatRemove").Last.ClickAsync();
            await WaitAsync(() => Model.Seats.Count, count => count == before - 1, "a seat removed");
        }
    }

    // A game set up and begun through the page: seat 1 is the human, the others computers.
    private async Task StartGameAsync(int seed, int seats = 4, int humans = 1, string turns = null)
    {
        var previous = await ReadAsync(() => Session);
        await OpenSetupAsync();
        await SetSeatCountAsync(seats);
        for (var seat = 0; seat < seats; seat++)
        {
            await Id("SeatComputer").Nth(seat).SetCheckedAsync(seat >= humans);
        }

        if (turns != null)
        {
            await Id("TurnsPerSeason").SelectOptionAsync(turns);
        }

        await Id("Seed").FillAsync(seed.ToString(System.Globalization.CultureInfo.InvariantCulture));
        await Expect(Id("Start")).ToBeEnabledAsync();
        await Id("Start").ClickAsync();
        await Expect(Id("ProloguePane")).ToBeVisibleAsync();
        while (_fixture.Events.TryDequeue(out _)) { }
        await Id("Begin").ClickAsync();
        await Expect(Id("PlayMenu")).ToBeVisibleAsync();

        // The card faces are composed once per run and the new game's cards prepared before the first deal.
        var session = await WaitAsync(() => Session, s => s != null && !ReferenceEquals(s, previous), "the new game on the table", 120000);
        await OnEngineAsync(() =>
        {
            session.EventPresented += _fixture.Events.Enqueue;
            return true;
        });
        await WaitOnEngineAsync(() => session.Engine.State.Seed, s => s == seed, "the seeded game");
        // The first season's front page opens by itself; close it so the table takes the clicks.
        await WaitAsync(() => Model.IsInspectorOpen && Model.InspectorKind == CardKind.Season, open => open, "the first season's front page", 120000);
        await CloseInspectorAsync();
        if (humans > 0)
        {
            await WaitForHumanTurnAsync();
        }
    }

    private async Task CloseInspectorAsync()
    {
        await Id("CloseInspector").ClickAsync();
        await Expect(Id("InspectorPane")).ToBeHiddenAsync();
    }

    // The human's turn, with the table still: the computers' turns and the season front pages pass by themselves
    // (the front pages are closed as they open, as a player would).
    private async Task WaitForHumanTurnAsync(float timeout = 180000)
    {
        var started = DateTime.UtcNow;
        while ((DateTime.UtcNow - started).TotalMilliseconds < timeout)
        {
            if (await ReadAsync(() => Model.IsInspectorOpen))
            {
                try { await CloseInspectorAsync(); }
                catch (PlayTestException) { }
                continue;
            }

            if (await OnEngineAsync(() => _fixture.Host.Frame.IsHumanTurn && !Session.IsBusy)) return;
            await Task.Delay(50);
        }

        throw new TimeoutException("Timed out waiting for the human team's turn.");
    }

    // ------------------------------------------------------------------ the table

    private async Task RollAsync()
    {
        (await OnEngineAsync(() => State.Phase)).Should().BeOneOf(GamePhase.AwaitRoll, GamePhase.SeasonStart);
        await _fixture.ClickHudButtonAsync(0);
        await WaitOnEngineAsync(() => State.Phase == GamePhase.Spend && !Session.IsBusy, ready => ready, "the dice rolled and settled");
    }

    // Clicks the die (or both dice) a dig spends, then the trench.
    private async Task DigAsync(DigAction dig)
    {
        var chosen = await OnEngineAsync(() => State.ChosenDice.ToArray());
        var dice = dig.Dice switch
        {
            DiceChoice.DieA => new[] { chosen[0] },
            DiceChoice.DieB => new[] { chosen[1] },
            _ => new[] { chosen[0], chosen[1] },
        };
        foreach (var die in dice)
        {
            var centre = await OnEngineAsync(() => Session.Dice[die].Center);
            await _fixture.ClickTableAsync(centre);
            await WaitOnEngineAsync(() => _fixture.Host.Controls.SelectedDice.Contains(die), selected => selected, "the die chosen");
        }

        var slot = await OnEngineAsync(() => Session.Layout.SiteSlotCentre(dig.Slot));
        await _fixture.ClickTableAsync(slot);
    }

    // A dig the current roll allows with one die (or the sum), no Workers and no Tablet.
    private Task<DigAction> PlainDigAsync() => OnEngineAsync(() => Session.Engine.LegalActions()
        .OfType<DigAction>()
        .Where(d => d.Workers == 0 && d.TabletId == null)
        .OrderBy(d => d.Dice == DiceChoice.Both ? 1 : 0)
        .FirstOrDefault());

    private TeamState Human => State.Teams.First(t => t.Kind == SeatKind.Human);

    private Task<int> HandDiscoveriesAsync() => OnEngineAsync(() => Human.Hand.Count);

    // A game on the table and the table showing: the one in progress, or a new seeded one.
    private async Task ShowTableAsync()
    {
        if (await ReadAsync(() => Model.GameInProgress))
        {
            await Id("Continue").ClickAsync();
            await Expect(Id("PlayMenu")).ToBeVisibleAsync();
            return;
        }

        await StartGameAsync(DefaultSeed);
    }

    // ------------------------------------------------------------------ launch

    [Fact]
    public async Task Launch_starts_the_engine_from_the_canvas_first_layout()
    {
        await Expect(_fixture.Canvas).ToBeVisibleAsync();
        (await ReadAsync(() => GameLoop.Instance.IsRunning)).Should().BeTrue();
        // One host for the whole run: re-laying out the page never starts a second one.
        (await ReadAsync(() => ReferenceEquals(_fixture.Host, _fixture.FirstHost))).Should().BeTrue();
        _fixture.Log.Should().Contain(line => line.Contains("render tier requested: CPU"));
        _fixture.Log.Should().Contain(line => line.EndsWith("starting"));
        var steps = await ReadAsync(() => _fixture.Host.StepCount);
        await WaitAsync(() => _fixture.Host.StepCount, now => now > steps, "the engine stepping");
        var canvas = await _fixture.Canvas.BoundingBoxAsync();
        canvas.Width.Should().Be(_fixture.Application.Width);
        canvas.Height.Should().Be(_fixture.Application.Height);
    }

    [Fact]
    public async Task Title_pane_shows_the_wordmark_art_and_the_Jordan_epigraph()
    {
        await Expect(Id("TitlePane")).ToBeVisibleAsync();
        await Expect(Id("TitleWordmark")).ToHaveTextAsync("Goddess Temple Discovery!");
        await Expect(Id("TitleWordmarkArt")).ToBeVisibleAsync();
        await WaitAsync(() => Model.WordmarkImage != null && Model.TitleArtImage != null, loaded => loaded, "the wordmark and title art");
        await WaitAsync(() => ReferenceEquals(_fixture.Find<Image>("TitleWordmarkArt").Source, Model.WordmarkImage), shown => shown, "the wordmark art on the title");
        var epigraph = Catalog.Quotations.First();
        await Expect(Id("TitleEpigraph")).ToHaveTextAsync(epigraph.Text);
        await Expect(Id("TitleEpigraphAttribution")).ToContainTextAsync("Julius Jordan");
        await Expect(Id("TitleEpigraph")).ToContainTextAsync("descent of their personifications from heaven to earth");
        foreach (var button in new[] { "NewGame", "Continue", "Settings", "HowToPlay", "History", "Credits" })
        {
            await Expect(Id(button)).ToBeVisibleAsync();
        }

        await SnapshotAsync("GoddessTempleDiscovery-title");
    }

    [Fact]
    public async Task Window_title_is_the_game_name()
    {
        (await ReadAsync(() => _fixture.Application.Window.Title)).Should().Be("Goddess Temple Discovery!");
    }

    [Fact]
    public async Task Startup_reads_the_seeded_preferences_from_the_throwaway_store()
    {
        SettingsService.DirectoryPath.Should().Be(_fixture.SettingsDirectory);
        _fixture.Log.Should().Contain(line => line.EndsWith($"settings store: {_fixture.SettingsDirectory}"));
        (await ReadAsync(() => (Model.SoundEnabled, Model.ReducedMotion, Model.RevealComputerDiscoveries, Model.SelectedSpeed)))
            .Should().Be((AppFixture.SeededSound, AppFixture.SeededReducedMotion, AppFixture.SeededReveal, "2×"));
    }

    // The engine reads its keys once a step: the key stays down until the game has seen it. A click on the canvas
    // leaves it without keyboard focus; activating the window gives it back, as the fixture does here.
    private async Task HoldKeyAsync(string key, Func<bool> seen)
    {
        await ReadAsync(() => _fixture.GameCanvas.Focus(Microsoft.UI.Xaml.FocusState.Programmatic));
        await Page.Keyboard.DownAsync(key);
        try { await WaitAsync(seen, done => done, $"the game seeing {key}"); }
        finally { await Page.Keyboard.UpAsync(key); }
        await _fixture.WaitStepsAsync(2);
    }

    // The HUD's JOURNAL and SETTINGS buttons (top right of the table's header).
    private async Task<Vector2> HudPaneButtonAsync(bool settings)
    {
        var rect = await OnEngineAsync(() => Session.Layout.HeaderButton(settings ? 0 : 1));
        return new Vector2(rect.MidX, rect.MidY);
    }
}
