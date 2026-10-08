using System;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Tests.Support;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Rules.Tests.Engine;

public class GameEngineDigTests
{
    private static GameEngine Ready(int a, int b, DiscoveryCard site, Difficulty difficulty = Difficulty.Standard, SeasonEffect effect = SeasonEffect.None)
    {
        var engine = Fixtures.Engine(Fixtures.Effects(effect), difficulty: difficulty);
        Harness.Roll(engine, a, b);
        Harness.PutSite(engine, 0, site);
        return engine;
    }

    [Fact]
    public void A_single_die_digs_when_it_reaches_the_dig_number()
    {
        //Arrange
        var engine = Ready(3, 1, Fixtures.Discovery("s", 1));

        //Act
        var preview = engine.PreviewDig(0, DiceChoice.DieA, 0, null);

        //Assert
        preview.Needed.Should().Be(3);
        preview.Total.Should().Be(3);
        preview.CanDig.Should().BeTrue();
        preview.Modifiers.Should().BeEmpty();
    }

    [Fact]
    public void A_short_die_cannot_dig_and_the_apply_throws()
    {
        //Arrange
        var engine = Ready(2, 1, Fixtures.Discovery("s", 1));

        //Act
        Action act = () => engine.Apply(new DigAction(0, DiceChoice.DieA, 0, null));

        //Assert
        engine.PreviewDig(0, DiceChoice.DieA, 0, null).CanDig.Should().BeFalse();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Both_dice_dig_as_one_sum()
    {
        //Arrange
        var engine = Ready(3, 4, Fixtures.Discovery("s", 5));

        //Act
        engine.Apply(new DigAction(0, DiceChoice.Both, 0, null));

        //Assert
        engine.State.DieAUsed.Should().BeTrue();
        engine.State.DieBUsed.Should().BeTrue();
        engine.State.CurrentTeam.Hand.Single().Id.Should().Be("s");
    }

    [Fact]
    public void Workers_add_one_each_and_are_spent()
    {
        //Arrange
        var engine = Ready(4, 1, Fixtures.Discovery("s", 4));

        //Act
        var preview = engine.PreviewDig(0, DiceChoice.DieA, 2, null);
        var result = engine.Apply(new DigAction(0, DiceChoice.DieA, 2, null));

        //Assert
        preview.Total.Should().Be(6);
        preview.Modifiers.Should().Contain("2 Workers +2");
        engine.State.CurrentTeam.Workers.Should().Be(0);
        result.Events.OfType<WorkersSpent>().Single().Count.Should().Be(2);
    }

    [Fact]
    public void A_team_cannot_add_more_workers_than_it_has()
    {
        //Arrange
        var engine = Ready(6, 1, Fixtures.Discovery("s", 9));

        //Assert
        engine.PreviewDig(0, DiceChoice.DieA, 3, null).CanDig.Should().BeFalse();
        engine.PreviewDig(0, DiceChoice.DieA, -1, null).CanDig.Should().BeFalse();
    }

    [Fact]
    public void A_tablet_adds_two_and_is_discarded()
    {
        //Arrange
        var engine = Ready(4, 1, Fixtures.Discovery("s", 4));
        var tablet = engine.State.CurrentTeam.Tablets[0];

        //Act
        var result = engine.Apply(new DigAction(0, DiceChoice.DieA, 0, tablet.Id));

        //Assert
        result.Events.OfType<SiteExcavated>().Single().Preview.Modifiers.Should().Contain("Tablet +2");
        engine.State.CurrentTeam.Tablets.Should().BeEmpty();
        engine.State.TabletDiscardCount.Should().Be(1);
        result.Events.OfType<TabletSpent>().Single().Card.Should().Be(tablet);
    }

    [Fact]
    public void A_tablet_not_in_the_hand_cannot_be_spent()
    {
        //Arrange
        var engine = Ready(6, 1, Fixtures.Discovery("s", 1));

        //Assert
        engine.PreviewDig(0, DiceChoice.DieA, 0, "no-such-tablet").CanDig.Should().BeFalse();
    }

    [Theory]
    [InlineData(SpecialistRole.Architect, DiscoveryKind.Building, 1)]
    [InlineData(SpecialistRole.Architect, DiscoveryKind.Object, 0)]
    [InlineData(SpecialistRole.Epigrapher, DiscoveryKind.Inscription, 1)]
    [InlineData(SpecialistRole.Epigrapher, DiscoveryKind.Building, 0)]
    [InlineData(SpecialistRole.SmallFindsKeeper, DiscoveryKind.Object, 1)]
    [InlineData(SpecialistRole.SmallFindsKeeper, DiscoveryKind.Deposit, 1)]
    [InlineData(SpecialistRole.SmallFindsKeeper, DiscoveryKind.Stratum, 0)]
    [InlineData(SpecialistRole.Photographer, DiscoveryKind.Building, 0)]
    [InlineData(SpecialistRole.Surveyor, DiscoveryKind.Stratum, 0)]
    public void Specialists_add_one_on_their_own_kind_of_site(SpecialistRole role, DiscoveryKind kind, int bonus)
    {
        //Arrange
        var engine = Ready(5, 1, Fixtures.Discovery("s", 4, kind));
        Harness.Give(engine, role);

        //Act
        var preview = engine.PreviewDig(0, DiceChoice.DieA, 0, null);

        //Assert
        preview.Total.Should().Be(5 + bonus);
    }

    [Fact]
    public void The_foreman_adds_one_once_per_turn_and_only_when_needed()
    {
        //Arrange
        var engine = Ready(5, 5, Fixtures.Discovery("s", 4));
        Harness.PutSite(engine, 1, Fixtures.Discovery("t", 4));
        Harness.PutSite(engine, 2, Fixtures.Discovery("u", 3));
        Harness.Give(engine, SpecialistRole.Foreman);

        //Act
        var reachable = engine.PreviewDig(2, DiceChoice.DieA, 0, null);
        engine.Apply(new DigAction(0, DiceChoice.DieA, 0, null));
        var second = engine.PreviewDig(1, DiceChoice.DieB, 0, null);

        //Assert
        reachable.Modifiers.Should().NotContain("Foreman +1");
        engine.State.CurrentTeam.ForemanUsedThisTurn.Should().BeTrue();
        second.Total.Should().Be(5);
        second.CanDig.Should().BeFalse();
    }

    [Fact]
    public void A_pending_favor_adds_two_and_is_used_up()
    {
        //Arrange
        var engine = Ready(4, 4, Fixtures.Discovery("s", 4));
        Harness.PutSite(engine, 1, Fixtures.Discovery("t", 4));
        engine.State.CurrentTeam.PlusTwoTurnsLeft = 2;

        //Act
        var preview = engine.PreviewDig(0, DiceChoice.DieA, 0, null);
        engine.Apply(new DigAction(0, DiceChoice.DieA, 0, null));

        //Assert
        preview.Total.Should().Be(6);
        preview.Modifiers.Should().Contain("Favor +2");
        engine.State.CurrentTeam.PlusTwoPending.Should().BeFalse();
        engine.PreviewDig(1, DiceChoice.DieB, 0, null).Total.Should().Be(4);
    }

    [Theory]
    [InlineData(Difficulty.Easy, 2)]
    [InlineData(Difficulty.Standard, 3)]
    [InlineData(Difficulty.Hard, 4)]
    public void Difficulty_shifts_the_dig_number(Difficulty difficulty, int needed)
    {
        //Arrange
        var engine = Ready(1, 1, Fixtures.Discovery("s", 1), difficulty);

        //Act
        var preview = engine.PreviewDig(0, DiceChoice.DieA, 0, null);

        //Assert
        preview.Needed.Should().Be(needed);
    }

    [Fact]
    public void Every_modifier_adds_up_together()
    {
        //Arrange
        var engine = Ready(6, 5, Fixtures.Discovery("s", 9, DiscoveryKind.Building), Difficulty.Hard);
        engine.State.CurrentTeam.Workers = 3;
        Harness.Give(engine, SpecialistRole.Architect);
        Harness.Give(engine, SpecialistRole.Foreman);
        engine.State.CurrentTeam.PlusTwoTurnsLeft = 1;
        var tablet = engine.State.CurrentTeam.Tablets[0];

        //Act
        var exact = engine.PreviewDig(0, DiceChoice.Both, 0, null);
        var all = engine.PreviewDig(0, DiceChoice.Both, 3, tablet.Id);

        //Assert
        exact.Needed.Should().Be(12);
        exact.Total.Should().Be(11 + 1 + 2);
        all.Total.Should().Be(11 + 3 + 2 + 1 + 2);
        all.Modifiers.Should().Equal("3 Workers +3", "Tablet +2", "Dig Number +1 (Hard)", "Architect +1", "Favor +2");
    }

    [Fact]
    public void A_dig_takes_the_card_refills_the_slot_and_writes_the_journal()
    {
        //Arrange
        var engine = Ready(3, 1, Fixtures.Discovery("s", 1));
        var nextInDeck = engine.State.SiteDeck[0];

        //Act
        var result = engine.Apply(new DigAction(0, DiceChoice.DieA, 0, null));

        //Assert
        engine.State.CurrentTeam.Hand.Single().Id.Should().Be("s");
        engine.State.SiteRow[0].Should().BeSameAs(nextInDeck);
        result.Events.OfType<SiteExcavated>().Single().Card.Id.Should().Be("s");
        result.Events.OfType<SiteRefilled>().Single().Card.Should().BeSameAs(nextInDeck);
        var entry = engine.State.Journal.Last();
        entry.Kind.Should().Be(JournalEntryKind.Discovery);
        entry.TeamName.Should().Be(engine.State.CurrentTeam.Name);
        entry.Sources.Should().Be("Source of s");
    }

    [Fact]
    public void An_empty_site_deck_leaves_the_slot_empty()
    {
        //Arrange
        var engine = Ready(3, 1, Fixtures.Discovery("s", 1));
        engine.State.SiteDeck.Clear();

        //Act
        var result = engine.Apply(new DigAction(0, DiceChoice.DieA, 0, null));

        //Assert
        engine.State.SiteRow[0].Should().BeNull();
        result.Events.OfType<SiteRefilled>().Single().Card.Should().BeNull();
        engine.PreviewDig(0, DiceChoice.DieB, 0, null).CanDig.Should().BeFalse();
        engine.LegalActions().OfType<DigAction>().Any(d => d.Slot == 0).Should().BeFalse();
    }
}
