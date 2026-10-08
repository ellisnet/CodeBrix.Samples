using System;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Engine;

public class GameEngineSetupTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void Constructor_rejects_a_seat_count_outside_two_to_four(int seats)
    {
        //Arrange
        var setup = new GameSetup(Enumerable.Range(0, seats).Select(i => new SeatSetup("T" + i, null, SeatKind.Human, Temperament.Scholar)).ToArray());

        //Act
        Action act = () => new GameEngine(setup, Fixtures.Catalog());

        //Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public void Constructor_rejects_turns_per_season_outside_one_to_three(int turns)
    {
        //Act
        Action act = () => new GameEngine(Fixtures.Setup(turnsPerSeason: turns), Fixtures.Catalog());

        //Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_rejects_a_catalog_without_seasons()
    {
        //Arrange
        var catalog = new CatalogSnapshot(Fixtures.Discoveries(), Fixtures.Tablets(), Fixtures.Specialists(), null, null, null);

        //Act
        Action act = () => new GameEngine(Fixtures.Setup(), catalog);

        //Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Setup_deals_five_sites_and_four_specialists()
    {
        //Act
        var engine = Fixtures.Engine();

        //Assert
        engine.State.SiteRow.Count.Should().Be(5);
        engine.State.SiteRow.All(c => c != null).Should().BeTrue();
        engine.State.ExpeditionRow.Count.Should().Be(4);
        engine.State.ExpeditionRow.All(c => c != null).Should().BeTrue();
        engine.State.SiteDeckCount.Should().Be(36 - 5);
        engine.State.ExpeditionDeckCount.Should().Be(6 - 4);
    }

    [Fact]
    public void The_row_starts_shallow()
    {
        //Act
        var engine = Fixtures.Engine(seed: 99);

        //Assert
        engine.State.SiteRow.Max(c => c.Tier).Should().BeLessThanOrEqualTo(4);
    }

    [Fact]
    public void Every_team_starts_with_two_workers_and_one_tablet()
    {
        //Act
        var engine = Fixtures.Engine(seats: 4);

        //Assert
        engine.State.Teams.Count.Should().Be(4);
        engine.State.Teams.All(t => t.Workers == 2 && t.Tablets.Count == 1 && t.Hand.Count == 0 && t.Specialists.Count == 0).Should().BeTrue();
        engine.State.TabletDeckCount.Should().Be(12 - 4);
    }

    [Fact]
    public void Seat_order_is_shuffled_and_keeps_every_seat()
    {
        //Act
        var orders = Enumerable.Range(1, 12)
            .Select(seed => Fixtures.Engine(seats: 4, seed: seed).State.Teams.Select(t => t.SetupIndex).ToArray())
            .ToArray();

        //Assert
        orders.All(o => o.OrderBy(i => i).SequenceEqual(new[] { 0, 1, 2, 3 })).Should().BeTrue();
        orders.Select(o => string.Join(",", o)).Distinct().Count().Should().BeGreaterThan(1);
    }

    [Fact]
    public void Team_takes_its_profile_and_an_empty_name_takes_the_profile_name()
    {
        //Act
        var engine = Fixtures.Engine();

        //Assert
        foreach (var team in engine.State.Teams)
        {
            team.Name.Should().Be(team.Profile.Name);
            team.Index.Should().Be(engine.State.Teams.ToList().IndexOf(team));
        }
    }

    [Fact]
    public void Unknown_profiles_get_a_plain_profile_and_repeated_names_are_made_unique()
    {
        //Arrange
        var setup = new GameSetup(new[]
        {
            new SeatSetup("Uruk Team", "no-such-profile", SeatKind.Human, Temperament.Surveyor),
            new SeatSetup("Uruk Team", null, SeatKind.Computer, Temperament.Scholar),
        }, Seed: 3);

        //Act
        var engine = new GameEngine(setup, Fixtures.Catalog());

        //Assert
        engine.State.Teams.Select(t => t.Name).OrderBy(n => n).Should().Equal("Uruk Team", "Uruk Team 2");
        engine.State.Teams.All(t => t.Profile != null && !string.IsNullOrEmpty(t.Profile.Colour)).Should().BeTrue();
    }

    [Fact]
    public void Game_opens_on_the_first_season_awaiting_the_first_roll()
    {
        //Act
        var engine = Fixtures.Engine();

        //Assert
        engine.State.Phase.Should().Be(GamePhase.SeasonStart);
        engine.State.SeasonIndex.Should().Be(0);
        engine.State.Season.Year.Should().Be("1912/13");
        engine.State.CurrentTeam.Should().BeSameAs(engine.State.Teams[0]);
        engine.State.Dice.Should().BeEmpty();
        engine.State.Events.OfType<SeasonStarted>().Count().Should().Be(1);
        engine.State.Events.OfType<TabletDrawn>().Count().Should().Be(2);
    }

    [Fact]
    public void Without_a_seed_the_engine_picks_one_and_reports_it()
    {
        //Arrange
        var setup = Fixtures.Setup() with { Seed = null };

        //Act
        var engine = new GameEngine(setup, Fixtures.Catalog());
        var replay = new GameEngine(setup with { Seed = engine.State.Seed }, Fixtures.Catalog());

        //Assert
        replay.StateHash().Should().Be(engine.StateHash());
    }
}
