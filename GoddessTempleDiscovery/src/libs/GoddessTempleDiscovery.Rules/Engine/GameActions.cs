using System;
using System.Collections.Generic;
using System.Linq;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>Something the current team does. <see cref="GameEngine.LegalActions"/> lists the ones open now.</summary>
public abstract record GameAction;

/// <summary>Roll the dice (two, or three when a Favor gave an extra die; the best two are kept).</summary>
public sealed record RollAction : GameAction;

/// <summary>Re-roll one unspent kept die (the RerollOnce season, once per team per season).</summary>
/// <param name="DieIndex">The index of the die in <see cref="GameState.Dice"/>.</param>
public sealed record RerollAction(int DieIndex) : GameAction;

/// <summary>Excavate a site of the Site Row.</summary>
/// <param name="Slot">The Site Row slot, 0 to 4.</param>
/// <param name="Dice">The die, or both dice, spent.</param>
/// <param name="Workers">The Workers added (+1 each).</param>
/// <param name="TabletId">The id of a Tablet in the hand spent for +2, or null for none.</param>
public sealed record DigAction(int Slot, DiceChoice Dice, int Workers, string TabletId) : GameAction;

/// <summary>Recruit a Specialist from the Expedition Row.</summary>
/// <param name="Slot">The Expedition Row slot, 0 to 3.</param>
/// <param name="Dice">The die, or both dice, spent.</param>
public sealed record RecruitAction(int Slot, DiceChoice Dice) : GameAction;

/// <summary>Spend a die to draw Tablets.</summary>
/// <param name="Dice">The die, or both dice, spent.</param>
public sealed record StudyAction(DiceChoice Dice) : GameAction;

/// <summary>Send a Site Row card to the bottom of the Site deck and deal a new one.</summary>
/// <param name="Slot">The Site Row slot, 0 to 4.</param>
/// <param name="Dice">The die spent, or null to use a free survey.</param>
public sealed record SurveyAction(int Slot, DiceChoice? Dice) : GameAction;

/// <summary>Lay Discoveries from the hand down as a Preliminary Report.</summary>
/// <param name="CardIds">The ids of the Discoveries, three or more.</param>
public sealed record PublishAction(IReadOnlyList<string> CardIds) : GameAction
{
    /// <summary>Two publish actions are equal when they name the same ids in the same order.</summary>
    /// <param name="other">The other action.</param>
    /// <returns>True when equal.</returns>
    public bool Equals(PublishAction other) =>
        other is not null
        && (ReferenceEquals(CardIds, other.CardIds)
            || (CardIds != null && other.CardIds != null && CardIds.SequenceEqual(other.CardIds, StringComparer.Ordinal)));

    /// <summary>A hash over the ids.</summary>
    /// <returns>The hash.</returns>
    public override int GetHashCode()
    {
        var hash = 17;
        if (CardIds != null)
        {
            foreach (var id in CardIds)
            {
                hash = (hash * 31) + (id == null ? 0 : StringComparer.Ordinal.GetHashCode(id));
            }
        }

        return hash;
    }
}

/// <summary>Discard one card (a Discovery or a Tablet) to get down to the hand limit.</summary>
/// <param name="CardId">The id of the card.</param>
public sealed record DiscardAction(string CardId) : GameAction;

/// <summary>End the turn (unspent dice are lost).</summary>
public sealed record EndTurnAction : GameAction;
