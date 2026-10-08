using System;
using System.Collections.Generic;
using System.Linq;
using GoddessTempleDiscovery.Rules.Brains;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Engine;

public class GameEngineDeterminismTests
{
    private static readonly Temperament[] Mixed = { Temperament.Surveyor, Temperament.DeepDigger, Temperament.Scholar, Temperament.DeepDigger };

    private static GameEngine Golden(int seed, out int actions)
    {
        var engine = new GameEngine(Fixtures.Setup(seats: 4, seed: seed, temperaments: Mixed), Fixtures.Catalog());
        actions = Harness.PlayOut(engine, new Random(seed));
        return engine;
    }

    private static string Scores(GameEngine engine) =>
        string.Join(";", engine.FinalScores().Select(s => s.TeamName + "=" + s.Total + "#" + s.Rank));

    [Fact]
    public void Golden_seed_2718_full_game()
    {
        //Act
        var engine = Golden(2718, out var actions);

        //Assert
        engine.State.IsGameOver.Should().BeTrue();
        //One assertion over all three pinned values, so a rule change shows every new value at once
        $"{actions}|{Scores(engine)}|{engine.StateHash()}".Should().Be($"{GoldenValues.Actions2718}|{GoldenValues.Scores2718}|{GoldenValues.Hash2718}");
    }

    [Fact]
    public void Golden_seed_1913_full_game()
    {
        //Act
        var engine = Golden(1913, out var actions);

        //Assert
        engine.State.IsGameOver.Should().BeTrue();
        //One assertion over all three pinned values, so a rule change shows every new value at once
        $"{actions}|{Scores(engine)}|{engine.StateHash()}".Should().Be($"{GoldenValues.Actions1913}|{GoldenValues.Scores1913}|{GoldenValues.Hash1913}");
    }

    [Fact]
    public void Two_engines_with_one_seed_and_one_action_sequence_stay_identical()
    {
        //Arrange
        var a = new GameEngine(Fixtures.Setup(seats: 3, seed: 555), Fixtures.Catalog());
        var b = new GameEngine(Fixtures.Setup(seats: 3, seed: 555), Fixtures.Catalog());
        var random = new Random(9);

        //Act
        while (!a.State.IsGameOver)
        {
            var action = ComputerBrain.Choose(a, random);
            a.Apply(action);
            b.Apply(action);
            b.StateHash().Should().Be(a.StateHash());
        }

        //Assert
        b.State.IsGameOver.Should().BeTrue();
        b.State.Journal.Count.Should().Be(a.State.Journal.Count);
    }

    [Fact]
    public void Different_seeds_deal_different_games()
    {
        //Assert
        Fixtures.Engine(seed: 1).StateHash().Should().NotBe(Fixtures.Engine(seed: 2).StateHash());
    }

    [Fact]
    public void The_hash_changes_with_every_action()
    {
        //Arrange
        var engine = new GameEngine(Fixtures.Setup(seats: 2, seed: 31), Fixtures.Catalog());
        var random = new Random(31);
        var seen = new HashSet<string> { engine.StateHash() };

        //Act
        while (!engine.State.IsGameOver)
        {
            engine.Apply(ComputerBrain.Choose(engine, random));
            seen.Add(engine.StateHash()).Should().BeTrue();
        }

        //Assert
        seen.Count.Should().Be(engine.State.ActionCount + 1);
    }

    [Fact]
    public void The_journal_records_every_season_discovery_tablet_favor_specialist_and_report_in_order()
    {
        //Arrange
        var engine = new GameEngine(Fixtures.Setup(seats: 4, seed: 2718, temperaments: Mixed), Fixtures.Catalog());
        var random = new Random(2718);
        var expected = new List<JournalEntry>();
        var seenOnce = new HashSet<string>();

        //Act
        expected.AddRange(engine.State.Events.OfType<JournalEntryAdded>().Select(e => e.Entry));
        foreach (var t in engine.State.Events.OfType<TabletDrawn>())
        {
            seenOnce.Add("t:" + t.Card.Id);
        }

        engine.ClearEvents();
        var excavated = 0;
        var reports = 0;
        while (!engine.State.IsGameOver)
        {
            var result = engine.Apply(ComputerBrain.Choose(engine, random));
            excavated += result.Events.OfType<SiteExcavated>().Count();
            reports += result.Events.OfType<ReportPublished>().Count();
            foreach (var e in result.Events)
            {
                var key = e switch
                {
                    TabletDrawn t => "t:" + t.Card.Id,
                    FavorDrawn f => "f:" + f.Card.Id,
                    SpecialistRecruited s => "s:" + s.Card.Role,
                    _ => null,
                };
                if (key != null)
                {
                    seenOnce.Add(key);
                }
            }

            expected.AddRange(result.Events.OfType<JournalEntryAdded>().Select(e => e.Entry));
        }

        //Assert
        var journal = engine.State.Journal;
        journal.Should().Equal(expected);
        journal.Count(j => j.Kind == JournalEntryKind.Season).Should().Be(12);
        journal.Count(j => j.Kind == JournalEntryKind.Discovery).Should().Be(excavated);
        journal.Count(j => j.Kind == JournalEntryKind.Report).Should().Be(reports);
        var unique = journal.Count(j => j.Kind == JournalEntryKind.Tablet || j.Kind == JournalEntryKind.Favor || j.Kind == JournalEntryKind.Specialist);
        unique.Should().Be(seenOnce.Count);
        journal.Where(j => j.Kind != JournalEntryKind.Report).All(j => !string.IsNullOrEmpty(j.Sources) && !string.IsNullOrEmpty(j.Text)).Should().BeTrue();
        journal.Select(j => j.SeasonYear).Distinct().Should().Equal(Fixtures.Years);
        excavated.Should().BeGreaterThan(0);
        reports.Should().BeGreaterThan(0);
    }
}
