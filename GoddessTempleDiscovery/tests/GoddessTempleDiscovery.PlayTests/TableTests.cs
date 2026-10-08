using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GoddessTempleDiscovery.Game.Bridges;
using GoddessTempleDiscovery.Game.Session;
using GoddessTempleDiscovery.Rules.Engine;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task The_site_row_shows_five_face_down_trenches_with_their_labels()
    {
        await StartGameAsync(DefaultSeed);
        var slots = await OnEngineAsync(() => Session.SiteSlots.Select(a => a.Pile.Count).ToArray());
        slots.Should().Equal(1, 1, 1, 1, 1);
        (await OnEngineAsync(() => Session.SiteSlots.All(a => a.Pile.Cards.All(c => !c.IsFaceUp)))).Should().BeTrue();
        var labels = await OnEngineAsync(() => Session.SlotLabels().ToArray());
        labels.Should().HaveCount(5);
        labels.Should().NotContainNulls();
        var row = await OnEngineAsync(() => State.SiteRow.ToArray());
        row.Should().NotContainNulls();
        // The table's cards are the engine's Site Row, slot for slot.
        (await OnEngineAsync(() => Enumerable.Range(0, 5).All(slot =>
            ReferenceEquals(TableSession.RulesCard(Session.SiteSlots[slot].Pile.Cards[0]), State.SiteRow[slot])))).Should().BeTrue();
        await SnapshotAsync("GoddessTempleDiscovery-table");
    }

    [Fact]
    public async Task Roll_puts_two_dice_values_on_the_table_and_in_the_engine()
    {
        await StartGameAsync(DefaultSeed);
        await RollAsync();
        var dice = await OnEngineAsync(() => State.Dice.ToArray());
        dice.Should().HaveCount(2);
        dice.Should().OnlyContain(value => value >= 1 && value <= 6);
        (await OnEngineAsync(() => Session.DiceInPlay)).Should().Be(2);
        // The table's dice show the engine's values.
        (await OnEngineAsync(() => Session.Dice.Take(2).Select(d => (d.Die.ResultIndex ?? -1) + 1).ToArray())).Should().Equal(dice);
        (await OnEngineAsync(() => Session.Dice.Take(2).All(d => d.Center.X > 0))).Should().BeTrue();
        _fixture.Events.OfType<DiceRolled>().Should().ContainSingle(e => e.Values.SequenceEqual(dice));
        (await ReadAsync(() => Model.TickerText)).Should().Contain($"rolls {dice[0]} and {dice[1]}");
        (await OnEngineAsync(() => _fixture.Host.Frame.Ticker)).Should().Contain($"rolls {dice[0]} and {dice[1]}");
    }

    [Fact]
    public async Task Digging_a_trench_opens_the_newspaper_with_its_headline_masthead_columns_and_sidebars()
    {
        await StartGameAsync(DefaultSeed);
        await RollAsync();
        var dig = await PlainDigAsync();
        dig.Should().NotBeNull();
        var before = await HandDiscoveriesAsync();
        await DigAsync(dig);

        var excavated = await WaitAsync(() => _fixture.Events.OfType<SiteExcavated>().FirstOrDefault(), e => e != null, "the SiteExcavated event");
        excavated.Slot.Should().Be(dig.Slot);
        (await HandDiscoveriesAsync()).Should().Be(before + 1);
        (await OnEngineAsync(() => Human.Hand.Contains(excavated.Card))).Should().BeTrue();

        await WaitAsync(() => Model.IsInspectorOpen && Model.InspectorKind == CardKind.Discovery, open => open, "the inspector on the find");
        await Expect(Id("InspectorPane")).ToBeVisibleAsync();
        var headline = await ReadAsync(() => Model.InspectorHeadline);
        headline.Should().NotBeNullOrWhiteSpace();
        await Expect(Id("InspectorHeadline")).ToHaveTextAsync(headline);
        await Expect(Id("InspectorTitle")).ToHaveTextAsync(excavated.Card.Title);
        await Expect(Id("InspectorEdition")).ToHaveTextAsync("EXTRA! · SPECIAL EDITION");
        await Expect(Id("InspectorMasthead")).ToBeVisibleAsync();
        await WaitAsync(() => _fixture.Find<Image>("InspectorMasthead").Source != null, loaded => loaded, "the masthead art");
        (await ReadAsync(() => _fixture.Find<Image>("InspectorMasthead").Source)).Should().BeSameAs(await ReadAsync(() => Model.MastheadImage));
        await Expect(Id("InspectorColumnOne")).ToHaveTextAsync(excavated.Card.CardText);
        (await ReadAsync(() => Model.InspectorColumnTwo)).Should().NotBeNullOrWhiteSpace();
        await Expect(Id("InspectorWhereNow")).ToBeVisibleAsync();
        (await ReadAsync(() => Model.InspectorFacts.Count)).Should().BeGreaterThan(0);
        await WaitAsync(() => Model.InspectorPhoto != null, loaded => loaded, "the photograph");
        (await ReadAsync(() => Model.ExtraVisibility)).Should().Be(excavated.Card.IsStarred ? Visibility.Visible : Visibility.Collapsed);
        await SnapshotAsync("GoddessTempleDiscovery-newspaper");

        // A person's own find stays open until it is closed.
        await Task.Delay(1000, TestContext.Current.CancellationToken);
        await Expect(Id("InspectorPane")).ToBeVisibleAsync();
        await CloseInspectorAsync();
        (await ReadAsync(() => Model.IsInspectorOpen)).Should().BeFalse();
    }

    [Fact]
    public async Task Escape_closes_the_newspaper()
    {
        await StartGameAsync(DefaultSeed);
        await RollAsync();
        await DigAsync(await PlainDigAsync());
        await WaitAsync(() => Model.IsInspectorOpen && Model.InspectorKind == CardKind.Discovery, open => open, "the inspector on the find");
        // A click on the canvas leaves it without keyboard focus; activating the window gives it back.
        await ReadAsync(() => _fixture.GameCanvas.Focus(FocusState.Programmatic));
        // The engine reads its keys once a step: hold Escape until the game has seen it.
        await Page.Keyboard.DownAsync("Escape");
        try { await WaitAsync(() => Model.IsInspectorOpen, open => !open, "the inspector closed by Escape"); }
        finally { await Page.Keyboard.UpAsync("Escape"); }
        await Expect(Id("InspectorPane")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task Study_draws_a_tablet_into_the_hand()
    {
        await StartGameAsync(DefaultSeed);
        await RollAsync();
        var tablets = await OnEngineAsync(() => Human.Tablets.Count);
        var handCards = await OnEngineAsync(() => Session.HandArea.Pile.Count);
        await _fixture.ClickHudButtonAsync(2);
        await WaitOnEngineAsync(() => Human.Tablets.Count, count => count > tablets, "a tablet drawn");
        await WaitOnEngineAsync(() => !Session.IsBusy && Session.HandArea.Pile.Count > handCards, ready => ready, "the tablet dealt into the hand");
        _fixture.Events.OfType<TabletDrawn>().Should().NotBeEmpty();
        (await OnEngineAsync(() => State.DieAUsed || State.DieBUsed)).Should().BeTrue();
    }

    [Fact]
    public async Task Survey_sends_a_trench_to_the_bottom_of_the_tell_and_deals_a_new_one()
    {
        await StartGameAsync(DefaultSeed);
        await RollAsync();
        var before = await OnEngineAsync(() => State.SiteRow[2]);
        await _fixture.ClickHudButtonAsync(3);
        await WaitOnEngineAsync(() => _fixture.Host.Controls.Mode, mode => mode == ControlMode.Survey, "the survey mode");
        (await OnEngineAsync(() => _fixture.Host.Frame.Prompt)).Should().Be("Click a trench of the Site Row to survey it.");
        await _fixture.ClickTableAsync(await OnEngineAsync(() => Session.Layout.SiteSlotCentre(2)));
        var surveyed = await WaitAsync(() => _fixture.Events.OfType<Surveyed>().FirstOrDefault(), e => e != null, "the Surveyed event");
        (surveyed.Slot, surveyed.Removed).Should().Be((2, before));
        var after = await OnEngineAsync(() => State.SiteRow[2]);
        after.Should().NotBeSameAs(before);
        after.Should().BeSameAs(surveyed.Added);
        await WaitOnEngineAsync(() => !Session.IsBusy && ReferenceEquals(TableSession.RulesCard(Session.SiteSlots[2].Pile.Cards.Single()), after),
            ready => ready, "the new site dealt into the trench");
        (await OnEngineAsync(() => _fixture.Host.Controls.Mode)).Should().Be(ControlMode.Normal);
    }

    [Fact]
    public async Task End_turn_passes_to_the_computers_whose_turns_play_out_until_the_human_turn()
    {
        await StartGameAsync(DefaultSeed);
        var human = await OnEngineAsync(() => Human);
        await RollAsync();
        var ticker = await ReadAsync(() => Model.TickerText);
        await _fixture.ClickHudButtonAsync(1);
        await WaitOnEngineAsync(() => State.CurrentTeam, team => team != human, "a computer team's turn");
        var computer = await OnEngineAsync(() => State.CurrentTeam);
        computer.Kind.Should().Be(SeatKind.Computer);
        // No click from here on: the computers' pacing and the season front page's own clock carry the game on.
        await WaitOnEngineAsync(() => State.CurrentTeam == human && _fixture.Host.Frame.IsHumanTurn, back => back, "the human team's next turn", 180000);
        // The table presents the human's turn once the computers' last moves have played out.
        await WaitAsync(() => _fixture.Events.OfType<TurnStarted>().LastOrDefault()?.TeamName, name => name == human.Name, "the human's turn on the table");
        var turns = _fixture.Events.OfType<TurnStarted>().Select(e => e.TeamName).ToArray();
        turns.Count(name => name != human.Name).Should().BeGreaterThanOrEqualTo(3);
        turns.Last().Should().Be(human.Name);
        _fixture.Log.Should().Contain(line => line.Contains($"action: {computer.Name}"));
        (await ReadAsync(() => Model.TickerText)).Should().NotBe(ticker);
        (await ReadAsync(() => Model.Standings.Count)).Should().Be(4);
        (await ReadAsync(() => Model.Standings.Count(s => s.Label.StartsWith("▸ ")))).Should().Be(1);
        (await ReadAsync(() => Model.Standings.Single(s => s.Label.StartsWith("▸ ")).Label)).Should().Be("▸ " + human.Name);
    }

    [Fact]
    public async Task Menu_returns_to_the_title_and_continue_returns_to_the_game()
    {
        await ShowTableAsync();
        var session = await ReadAsync(() => Session);
        await Id("PlayMenu").ClickAsync();
        await Expect(Id("TitlePane")).ToBeVisibleAsync();
        await Expect(Id("Continue")).ToBeEnabledAsync();
        await Id("Continue").ClickAsync();
        await Expect(Id("PlayMenu")).ToBeVisibleAsync();
        await Expect(Id("TitlePane")).ToBeHiddenAsync();
        (await ReadAsync(() => Session)).Should().BeSameAs(session);
    }

    [Fact]
    public async Task The_hud_journal_button_opens_the_field_journal()
    {
        await ShowTableAsync();
        if (await ReadAsync(() => Model.IsInspectorOpen)) await CloseInspectorAsync();
        await _fixture.ClickTableAsync(await HudPaneButtonAsync(settings: false));
        await Expect(Id("JournalPane")).ToBeVisibleAsync();
        await Id("CloseJournal").ClickAsync();
        await Expect(Id("JournalPane")).ToBeHiddenAsync();
    }

    [Fact]
    public async Task The_hud_settings_button_opens_the_settings()
    {
        await ShowTableAsync();
        if (await ReadAsync(() => Model.IsInspectorOpen)) await CloseInspectorAsync();
        await _fixture.ClickTableAsync(await HudPaneButtonAsync(settings: true));
        await Expect(Id("SettingsPane")).ToBeVisibleAsync();
        await Id("CloseSettings").ClickAsync();
        await Expect(Id("SettingsPane")).ToBeHiddenAsync();
    }

    // A HUD button that opened its pane on the press left the canvas without its release, and the next click on the
    // table was lost. HUD buttons now act on the release (see README.md): the next click on the table still counts.
    [Fact]
    public async Task After_a_hud_button_opens_a_pane_the_next_click_on_the_table_still_counts()
    {
        await ShowTableAsync();
        if (await ReadAsync(() => Model.IsInspectorOpen)) await CloseInspectorAsync();
        await _fixture.ClickTableAsync(await HudPaneButtonAsync(settings: false));
        await Expect(Id("JournalPane")).ToBeVisibleAsync();
        await Id("CloseJournal").ClickAsync();
        await Expect(Id("JournalPane")).ToBeHiddenAsync();
        await _fixture.ClickTableAsync(await HudPaneButtonAsync(settings: true));
        await Expect(Id("SettingsPane")).ToBeVisibleAsync();
        await Id("CloseSettings").ClickAsync();
    }
}
