using System.Collections.Generic;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Scoring;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>Something that happened on the table, for the presentation to animate.</summary>
/// <param name="TeamName">The team it happened to, or null for the whole table.</param>
public abstract record GameEvent(string TeamName);

/// <summary>A Season card flipped.</summary>
/// <param name="Season">The season.</param>
public sealed record SeasonStarted(SeasonCard Season) : GameEvent((string)null);

/// <summary>A team's turn began.</summary>
/// <param name="TeamName">The team.</param>
public sealed record TurnStarted(string TeamName) : GameEvent(TeamName);

/// <summary>The dice were rolled.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="Values">The values, two or three.</param>
public sealed record DiceRolled(string TeamName, IReadOnlyList<int> Values) : GameEvent(TeamName);

/// <summary>One die was re-rolled.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="Index">The die's index in <see cref="GameState.Dice"/>.</param>
/// <param name="Value">The new value.</param>
public sealed record DieRerolled(string TeamName, int Index, int Value) : GameEvent(TeamName);

/// <summary>A site was excavated: the card flips and goes to the team's hand.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="Slot">The Site Row slot.</param>
/// <param name="Card">The discovery.</param>
/// <param name="Preview">The arithmetic of the dig.</param>
public sealed record SiteExcavated(string TeamName, int Slot, DiscoveryCard Card, DigPreview Preview) : GameEvent(TeamName);

/// <summary>A Site Row slot was dealt a new face-down card.</summary>
/// <param name="TeamName">The team whose action caused it, or null.</param>
/// <param name="Slot">The slot.</param>
/// <param name="Card">The new card, or null when the Site deck is empty.</param>
public sealed record SiteRefilled(string TeamName, int Slot, DiscoveryCard Card) : GameEvent(TeamName);

/// <summary>A Specialist joined a team.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="Slot">The Expedition Row slot.</param>
/// <param name="Card">The Specialist.</param>
public sealed record SpecialistRecruited(string TeamName, int Slot, SpecialistCard Card) : GameEvent(TeamName);

/// <summary>An Expedition Row slot was refilled.</summary>
/// <param name="TeamName">The team whose action caused it.</param>
/// <param name="Slot">The slot.</param>
/// <param name="Card">The new Specialist, or null when the Expedition deck is empty.</param>
public sealed record ExpeditionRefilled(string TeamName, int Slot, SpecialistCard Card) : GameEvent(TeamName);

/// <summary>A team drew a Tablet.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="Card">The Tablet.</param>
public sealed record TabletDrawn(string TeamName, TabletCard Card) : GameEvent(TeamName);

/// <summary>A team spent a Tablet for +2 on a dig.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="Card">The Tablet.</param>
public sealed record TabletSpent(string TeamName, TabletCard Card) : GameEvent(TeamName);

/// <summary>A Site Row card went to the bottom of the deck and a new one was dealt.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="Slot">The slot.</param>
/// <param name="Removed">The card sent back.</param>
/// <param name="Added">The card dealt.</param>
public sealed record Surveyed(string TeamName, int Slot, DiscoveryCard Removed, DiscoveryCard Added) : GameEvent(TeamName);

/// <summary>A report was published.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="Report">The report.</param>
public sealed record ReportPublished(string TeamName, Report Report) : GameEvent(TeamName);

/// <summary>A team drew a Favor card on doubles.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="Card">The Favor.</param>
public sealed record FavorDrawn(string TeamName, FavorCard Card) : GameEvent(TeamName);

/// <summary>A team gained Workers.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="Count">How many.</param>
public sealed record WorkerGained(string TeamName, int Count) : GameEvent(TeamName);

/// <summary>A team spent Workers on a dig.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="Count">How many.</param>
public sealed record WorkersSpent(string TeamName, int Count) : GameEvent(TeamName);

/// <summary>A team discarded a card at the hand limit.</summary>
/// <param name="TeamName">The team.</param>
/// <param name="CardId">The card's id.</param>
public sealed record HandLimitDiscard(string TeamName, string CardId) : GameEvent(TeamName);

/// <summary>A team's turn ended.</summary>
/// <param name="TeamName">The team.</param>
public sealed record TurnEnded(string TeamName) : GameEvent(TeamName);

/// <summary>The last season ended: the final scores.</summary>
/// <param name="Scores">The scores, in seat order.</param>
public sealed record GameEnded(FinalScore[] Scores) : GameEvent((string)null);

/// <summary>An entry was written to the Field Journal.</summary>
/// <param name="Entry">The entry.</param>
public sealed record JournalEntryAdded(JournalEntry Entry) : GameEvent(Entry?.TeamName);
