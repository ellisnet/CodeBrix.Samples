using System;
using System.Linq;
using GoddessTempleDiscovery.Rules.Brains;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;
using SilverAssertions;

namespace GoddessTempleDiscovery.Rules.Tests.Support;

/// <summary>Puts an engine into the exact position a test needs (through the internals the tests may see).</summary>
internal static class Harness
{
    /// <summary>Rolls, then sets the two kept dice to the given values.</summary>
    internal static void Roll(GameEngine engine, int a, int b)
    {
        engine.Apply(new RollAction());
        var s = engine.State;
        s.DiceList[s.ChosenList[0]] = a;
        s.DiceList[s.ChosenList[1]] = b;
    }

    internal static void PutSite(GameEngine engine, int slot, DiscoveryCard card) => engine.State.SiteRowArray[slot] = card;

    internal static void Give(GameEngine engine, SpecialistRole role, int cost = 3) =>
        engine.State.CurrentTeam.SpecialistList.Add(Fixtures.Specialist(role, cost));

    internal static TabletCard GiveTablet(GameEngine engine, string id, TabletKind kind = TabletKind.Goddess)
    {
        var tablet = Fixtures.Tablet(id, kind);
        engine.State.CurrentTeam.TabletList.Add(tablet);
        return tablet;
    }

    internal static void GiveDiscovery(GameEngine engine, DiscoveryCard card) => engine.State.CurrentTeam.HandList.Add(card);

    /// <summary>Rolls (if needed) and ends the turn, discarding the first cards at the hand limit.</summary>
    internal static void PassTurn(GameEngine engine)
    {
        var s = engine.State;
        if (s.Phase == GamePhase.SeasonStart || s.Phase == GamePhase.AwaitRoll)
        {
            engine.Apply(new RollAction());
        }

        engine.Apply(new EndTurnAction());
        while (s.Phase == GamePhase.TurnEnd)
        {
            engine.Apply(engine.LegalActions()[0]);
        }
    }

    /// <summary>Passes turns until the season index changes or the game ends.</summary>
    internal static void PassSeason(GameEngine engine)
    {
        var season = engine.State.SeasonIndex;
        while (!engine.State.IsGameOver && engine.State.SeasonIndex == season)
        {
            PassTurn(engine);
        }
    }

    /// <summary>Plays the game out with the computer brain, checking every choice is legal; returns the action count.</summary>
    internal static int PlayOut(GameEngine engine, Random random, int maxActions = 20000)
    {
        var count = 0;
        while (!engine.State.IsGameOver && count < maxActions)
        {
            var action = ComputerBrain.Choose(engine, random);
            engine.WhyIllegal(action).Should().BeNull();
            engine.Apply(action);
            engine.ClearEvents();
            count++;
        }

        return count;
    }
}
