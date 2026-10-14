using System.Linq;
using System.Threading.Tasks;
using GoddessTempleDiscovery.Game.Settings;
using GoddessTempleDiscovery.Rules.Content;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.ViewModels;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    public async Task New_game_opens_the_setup_with_the_last_setup_from_the_store()
    {
        var stored = SettingsService.LoadLastSeats();
        await OpenSetupAsync();
        var seats = await ReadAsync(() => Model.Seats.Select(s => s.ToRecord()).ToArray());
        seats.Length.Should().Be(stored.Count);
        for (var i = 0; i < seats.Length; i++)
        {
            (seats[i].TeamProfileId, seats[i].TeamName, seats[i].IsComputer, seats[i].Temperament)
                .Should().Be((stored[i].TeamProfileId, stored[i].TeamName, stored[i].IsComputer, stored[i].Temperament));
        }

        (await ReadAsync(() => Model.SelectedDifficulty)).Should().Be(SettingsService.Difficulty.ToString());
        await Expect(Id("Seed")).ToHaveValueAsync(string.Empty);
        (await ReadAsync(() => Model.SetupProblem)).Should().BeEmpty();
        await Expect(Id("Start")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Seats_go_from_two_to_four_and_no_further()
    {
        await OpenSetupAsync();
        await SetSeatCountAsync(4);
        await Expect(Id("SeatTeam")).ToHaveCountAsync(4);
        await Expect(Id("AddSeat")).ToBeDisabledAsync();
        await SetSeatCountAsync(2);
        await Expect(Id("SeatTeam")).ToHaveCountAsync(2);
        await Expect(Id("AddSeat")).ToBeEnabledAsync();
        // A game has at least two seats: removing one of the last two does nothing.
        await Id("SeatRemove").Last.ClickAsync();
        await Expect(Id("SeatTeam")).ToHaveCountAsync(2);
        (await ReadAsync(() => Model.Seats.Select(s => s.SeatLabel).ToArray())).Should().Equal("SEAT 1", "SEAT 2");
        await Expect(Id("Start")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task A_seat_toggles_between_human_and_computer_and_only_computers_choose_a_temperament()
    {
        await OpenSetupAsync();
        await SetSeatCountAsync(3);
        await Id("SeatComputer").Nth(0).UncheckAsync();
        await Id("SeatComputer").Nth(1).CheckAsync();
        await Id("SeatComputer").Nth(2).UncheckAsync();
        (await ReadAsync(() => Model.Seats.Select(s => s.KindLabel).ToArray())).Should().Equal("HUMAN", "COMPUTER", "HUMAN");
        await Expect(Id("SeatTemperament")).ToHaveCountAsync(1);
        await Id("SeatComputer").Nth(2).CheckAsync();
        await Expect(Id("SeatTemperament")).ToHaveCountAsync(2);
        (await ReadAsync(() => Model.Seats[2].IsComputer)).Should().BeTrue();
    }

    [Fact]
    public async Task A_seat_takes_a_team_card_a_name_of_its_own_and_a_temperament()
    {
        await OpenSetupAsync();
        await SetSeatCountAsync(2);
        await Id("SeatComputer").Nth(1).CheckAsync();
        // The last team card, unless the other seat holds it (a card belongs to one seat): then the first free one.
        var team = await ReadAsync(() => Model.Seats[0].TeamOptions.Contains(Catalog.Teams.Last().Name)
            ? Catalog.Teams.Last().Name
            : Model.Seats[0].TeamOptions.First(o => o != SeatViewModel.OwnName));
        await Id("SeatTeam").Nth(0).SelectOptionAsync(team);
        var teamId = Catalog.Teams.Single(t => t.Name == team).Id;
        await Id("SeatTeam").Nth(1).SelectOptionAsync(SeatViewModel.OwnName);
        // Only a seat with a name of its own shows its name box, so the second seat's is the only one.
        await Expect(Id("SeatName")).ToHaveCountAsync(1);
        await Id("SeatName").First.FillAsync("The Babylon Rivals");
        await Id("SeatTemperament").First.SelectOptionAsync(nameof(Temperament.DeepDigger));
        var setups = await ReadAsync(() => Model.Seats.Select(s => s.ToSetup()).ToArray());
        setups[0].TeamProfileId.Should().Be(teamId);
        // A name of its own carries an emblem: the team card's it had, or the first one no seat holds.
        var emblem = await ReadAsync(() => Model.Seats[1].EmblemProfileId);
        emblem.Should().NotBeEmpty();
        emblem.Should().NotBe(teamId);
        (setups[1].TeamName, setups[1].TeamProfileId, setups[1].Kind, setups[1].Temperament)
            .Should().Be(("The Babylon Rivals", emblem, SeatKind.Computer, Temperament.DeepDigger));
    }

    [Fact]
    public async Task A_team_of_its_own_name_chooses_an_emblem_no_other_seat_holds()
    {
        await OpenSetupAsync();
        await SetSeatCountAsync(3);
        // The first seat takes a team card it may have (one no other seat holds).
        var firstName = await ReadAsync(() => Model.Seats[0].TeamOptions.First(o => o != SeatViewModel.OwnName));
        await Id("SeatTeam").Nth(0).SelectOptionAsync(firstName);
        var first = Catalog.Teams.Single(t => t.Name == firstName);
        await Id("SeatTeam").Nth(1).SelectOptionAsync(SeatViewModel.OwnName);
        await Expect(Id("SeatName").First).ToBeVisibleAsync();
        await Expect(Id("SeatEmblems").Nth(0)).ToBeVisibleAsync();

        // The first seat's card is veiled for the second seat and missing from the others' team lists.
        (await ReadAsync(() => Model.Seats[1].Emblems.Single(e => e.ProfileId == first.Id).IsAvailable)).Should().BeFalse();
        (await ReadAsync(() => Model.Seats[2].TeamOptions.Contains(first.Name))).Should().BeFalse();
        (await ReadAsync(() => Model.Seats[0].TeamOptions.Contains(first.Name))).Should().BeTrue();

        // Choosing a free emblem takes it from every other seat; the one it left is free again.
        var before = await ReadAsync(() => Model.Seats[1].EmblemProfileId);
        var free = await ReadAsync(() => Model.Seats[1].Emblems.First(e => e.IsAvailable && !e.IsChosen).Profile);
        await ReadAsync(() =>
        {
            Model.Seats[1].Emblems.Single(e => e.ProfileId == free.Id).ChooseCommand.Execute(null);
            return true;
        });
        (await ReadAsync(() => Model.Seats[1].EmblemProfileId)).Should().Be(free.Id);
        (await ReadAsync(() => Model.Seats[2].TeamOptions.Contains(free.Name))).Should().BeFalse();
        (await ReadAsync(() => Model.Seats[2].TeamOptions.Contains(Catalog.Teams.Single(t => t.Id == before).Name))).Should().BeTrue();

        // No two seats ever hold the same card, and the stored setup keeps the emblem.
        var claims = await ReadAsync(() => Model.Seats.Select(s => s.ClaimedProfileId).ToArray());
        claims.Should().OnlyHaveUniqueItems();
        claims.Should().NotContainNulls();
        (await ReadAsync(() => Model.Seats[1].ToRecord().EmblemProfileId)).Should().Be(free.Id);
    }

    [Fact]
    public async Task The_seat_count_suggests_the_turns_a_season_and_any_of_one_to_three_may_be_chosen()
    {
        await OpenSetupAsync();
        //The suggested value is the rules' own default for the seat count, so the test follows a re-balancing
        for (var seats = 2; seats <= 4; seats++)
        {
            await SetSeatCountAsync(seats);
            var suggested = GameRules.DefaultTurnsPerSeason(seats);
            (await ReadAsync(() => Model.SelectedTurns)).Should().Be(suggested.ToString(System.Globalization.CultureInfo.InvariantCulture));
            (await ReadAsync(() => Model.TurnsHint)).Should().StartWith($"With {seats} seats the game suggests {suggested} turn");
        }

        await Id("TurnsPerSeason").SelectOptionAsync("3");
        (await ReadAsync(() => Model.SelectedTurns)).Should().Be("3");
    }

    [Fact]
    public async Task Difficulty_and_seed_are_chosen_and_start_is_enabled_only_when_the_setup_is_valid()
    {
        await OpenSetupAsync();
        await Id("Difficulty").SelectOptionAsync(nameof(Difficulty.Hard));
        (await ReadAsync(() => Model.SelectedDifficulty)).Should().Be(nameof(Difficulty.Hard));
        await Id("Seed").FillAsync("twelve");
        await Expect(Id("Start")).ToBeDisabledAsync();
        await Expect(Id("SetupProblem")).ToHaveTextAsync("The seed is a whole number, or empty for a fresh game.");
        await Id("Seed").FillAsync("1912");
        await Expect(Id("Start")).ToBeEnabledAsync();
        (await ReadAsync(() => Model.SetupProblem)).Should().BeEmpty();
        await Id("Seed").FillAsync(string.Empty);
        await Expect(Id("Start")).ToBeEnabledAsync();
        await Id("SetupBack").ClickAsync();
        await Expect(Id("TitlePane")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task The_last_setup_persists_through_the_store_and_comes_back_on_the_next_new_game()
    {
        await OpenSetupAsync();
        await SetSeatCountAsync(3);
        await Id("SeatComputer").Nth(0).UncheckAsync();
        await Id("SeatComputer").Nth(1).CheckAsync();
        await Id("SeatComputer").Nth(2).CheckAsync();
        await Id("SeatTeam").Nth(1).SelectOptionAsync(Catalog.Teams[4].Name);
        await Id("SeatTemperament").Nth(1).SelectOptionAsync(nameof(Temperament.Scholar));
        await Id("TurnsPerSeason").SelectOptionAsync("1");
        await Id("Difficulty").SelectOptionAsync(nameof(Difficulty.Hard));
        var chosen = await ReadAsync(() => Model.Seats.Select(s => s.ToRecord()).ToArray());
        await Id("Start").ClickAsync();
        await Expect(Id("ProloguePane")).ToBeVisibleAsync();

        var stored = SettingsService.LoadLastSeats();
        stored.Select(s => (s.TeamProfileId, s.IsComputer, s.Temperament))
            .Should().Equal(chosen.Select(s => (s.TeamProfileId, s.IsComputer, s.Temperament)));
        SettingsService.TurnsPerSeason.Should().Be(1);
        SettingsService.Difficulty.Should().Be(Difficulty.Hard);

        // Through the table and back to the title: the next setup is the stored one. The first season's front page
        // opens over the table and stays a while by default, so it is closed first, as a person would.
        await Id("Begin").ClickAsync();
        await Expect(Id("InspectorPane")).ToBeVisibleAsync();
        await Id("CloseInspector").ClickAsync();
        await Id("PlayMenu").ClickAsync();
        await OpenSetupAsync();
        (await ReadAsync(() => Model.Seats.Select(s => (s.ToRecord().TeamProfileId, s.IsComputer, s.Temperament)).ToArray()))
            .Should().Equal(chosen.Select(s => (s.TeamProfileId, s.IsComputer, s.Temperament)));
        (await ReadAsync(() => Model.SelectedDifficulty)).Should().Be(nameof(Difficulty.Hard));
        SettingsService.Difficulty = AppFixture.SeededDifficulty;
    }

    [Fact]
    public async Task Prologue_shows_its_paragraphs_and_begin_reaches_the_table_with_the_first_season_banner()
    {
        await OpenSetupAsync();
        await SetSeatCountAsync(2);
        await Id("SeatComputer").Nth(0).UncheckAsync();
        await Id("SeatComputer").Nth(1).CheckAsync();
        await Id("Seed").FillAsync("1913");
        await Id("Start").ClickAsync();
        await Expect(Id("PrologueTitle")).ToHaveTextAsync(Catalog.Prologue.First());
        var paragraphs = Id("PrologueParagraphs").GetByType<Microsoft.UI.Xaml.Controls.TextBlock>();
        await Expect(paragraphs).ToHaveCountAsync(Catalog.Prologue.Count - 1);
        await Expect(paragraphs.First).ToHaveTextAsync(Catalog.Prologue[1]);

        var previous = await ReadAsync(() => Session);
        await Id("Begin").ClickAsync();
        await Expect(Id("PlayMenu")).ToBeVisibleAsync();
        await Expect(Id("ProloguePane")).ToBeHiddenAsync();
        await WaitAsync(() => Session, s => s != null && !ReferenceEquals(s, previous), "the new game", 120000);
        // The first season's front page opens, and the banner names the first of the twelve seasons.
        await WaitAsync(() => Model.IsInspectorOpen, open => open, "the first season's front page", 120000);
        await Expect(Id("InspectorPane")).ToBeVisibleAsync();
        await Expect(Id("InspectorEdition")).ToHaveTextAsync("FRONT PAGE");
        var first = await OnEngineAsync(() => State.Seasons[0]);
        var count = await OnEngineAsync(() => State.SeasonCount);
        await WaitAsync(() => Model.SeasonTitle, title => title.EndsWith($"SEASON 1 OF {count}"), "the first season on the banner");
        (await OnEngineAsync(() => Session.BannerSeason)).Should().BeSameAs(first);
        _fixture.Log.Should().Contain(line => line.Contains($"season {first.Year}: {first.Title}"));
        await CloseInspectorAsync();
    }
}
