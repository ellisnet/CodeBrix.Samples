using System.Collections.Generic;
using System.Collections.ObjectModel;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Journal;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>Everything on the table: a read-only view for the presentation, changed only by the <see cref="GameEngine"/>.</summary>
public sealed class GameState
{
    internal GameState(GameSetup setup, int seed, IReadOnlyList<SeasonCard> seasons)
    {
        Setup = setup;
        Seed = seed;
        Seasons = seasons;
        Teams = new ReadOnlyCollection<TeamState>(TeamList);
        SiteRow = new ReadOnlyCollection<DiscoveryCard>(SiteRowArray);
        ExpeditionRow = new ReadOnlyCollection<SpecialistCard>(ExpeditionRowArray);
        Dice = new ReadOnlyCollection<int>(DiceList);
        ChosenDice = new ReadOnlyCollection<int>(ChosenList);
        Journal = new ReadOnlyCollection<JournalEntry>(JournalList);
        Events = new ReadOnlyCollection<GameEvent>(EventList);
    }

    /// <summary>The setup the game was started from.</summary>
    public GameSetup Setup { get; }

    /// <summary>The seed in use (the setup's, or the one chosen when it gave none).</summary>
    public int Seed { get; }

    /// <summary>The seasons of the game, in order.</summary>
    public IReadOnlyList<SeasonCard> Seasons { get; }

    /// <summary>The number of seasons.</summary>
    public int SeasonCount => Seasons.Count;

    /// <summary>The current season.</summary>
    public SeasonCard Season => SeasonIndex >= 0 && SeasonIndex < Seasons.Count ? Seasons[SeasonIndex] : null;

    /// <summary>The current season's index, from 0.</summary>
    public int SeasonIndex { get; internal set; }

    /// <summary>The round within the season, from 0 to <see cref="GameSetup.TurnsPerSeason"/> - 1.</summary>
    public int TurnInSeason { get; internal set; }

    /// <summary>How many teams have already played in this round (0 for the round's first team).</summary>
    public int TurnOrderPosition { get; internal set; }

    /// <summary>The team that plays first this season (the first team rotates each season).</summary>
    public int FirstTeamIndex => Teams.Count == 0 ? 0 : SeasonIndex % Teams.Count;

    /// <summary>The team whose turn it is.</summary>
    public TeamState CurrentTeam => Teams.Count == 0 ? null : Teams[(FirstTeamIndex + TurnOrderPosition) % Teams.Count];

    /// <summary>The teams, in seat order.</summary>
    public IReadOnlyList<TeamState> Teams { get; }

    /// <summary>The five Site Row slots; a slot is null only when the Site deck is empty.</summary>
    public IReadOnlyList<DiscoveryCard> SiteRow { get; }

    /// <summary>The four Expedition Row slots; a slot is null only when the Expedition deck is empty.</summary>
    public IReadOnlyList<SpecialistCard> ExpeditionRow { get; }

    /// <summary>Cards left in the Site deck.</summary>
    public int SiteDeckCount => SiteDeck.Count;

    /// <summary>Cards left in the Tablet deck (the spent Tablets are reshuffled into it when it runs out).</summary>
    public int TabletDeckCount => TabletDeck.Count;

    /// <summary>Spent and discarded Tablets waiting to be reshuffled.</summary>
    public int TabletDiscardCount => TabletDiscard.Count;

    /// <summary>Cards left in the Expedition deck.</summary>
    public int ExpeditionDeckCount => ExpeditionDeck.Count;

    /// <summary>Cards left in the Favor deck (drawn Favors are reshuffled into it when it runs out).</summary>
    public int FavorDeckCount => FavorDeck.Count;

    /// <summary>The dice values: empty before the roll, two values, or three when an extra die was rolled.</summary>
    public IReadOnlyList<int> Dice { get; }

    /// <summary>The indices in <see cref="Dice"/> of the two kept dice: Die A first, then Die B (empty before the roll).</summary>
    public IReadOnlyList<int> ChosenDice { get; }

    /// <summary>The value of Die A, or 0 before the roll.</summary>
    public int DieA => ChosenList.Count == 2 ? DiceList[ChosenList[0]] : 0;

    /// <summary>The value of Die B, or 0 before the roll.</summary>
    public int DieB => ChosenList.Count == 2 ? DiceList[ChosenList[1]] : 0;

    /// <summary>True once Die A has been spent this turn.</summary>
    public bool DieAUsed { get; internal set; }

    /// <summary>True once Die B has been spent this turn.</summary>
    public bool DieBUsed { get; internal set; }

    /// <summary>True when the two kept dice show the same value.</summary>
    public bool IsDoubles => ChosenList.Count == 2 && DieA == DieB;

    /// <summary>True once this turn's doubles have drawn their Favor.</summary>
    public bool FavorDrawnThisTurn { get; internal set; }

    /// <summary>The phase of the current turn.</summary>
    public GamePhase Phase { get; internal set; }

    /// <summary>True when the game has ended.</summary>
    public bool IsGameOver => Phase == GamePhase.GameOver;

    /// <summary>The Field Journal, in the order things were read.</summary>
    public IReadOnlyList<JournalEntry> Journal { get; }

    /// <summary>The events raised since the presentation last called <see cref="GameEngine.ClearEvents"/>.</summary>
    public IReadOnlyList<GameEvent> Events { get; }

    /// <summary>The number of actions applied so far.</summary>
    public int ActionCount { get; internal set; }

    internal List<TeamState> TeamList { get; } = new List<TeamState>();

    internal DiscoveryCard[] SiteRowArray { get; } = new DiscoveryCard[GameRules.SiteRowSize];

    internal SpecialistCard[] ExpeditionRowArray { get; } = new SpecialistCard[GameRules.ExpeditionRowSize];

    internal List<DiscoveryCard> SiteDeck { get; } = new List<DiscoveryCard>();

    internal List<TabletCard> TabletDeck { get; } = new List<TabletCard>();

    internal List<TabletCard> TabletDiscard { get; } = new List<TabletCard>();

    internal List<SpecialistCard> ExpeditionDeck { get; } = new List<SpecialistCard>();

    internal List<FavorCard> FavorDeck { get; } = new List<FavorCard>();

    internal List<FavorCard> FavorDiscard { get; } = new List<FavorCard>();

    internal List<DiscoveryCard> DiscardedDiscoveries { get; } = new List<DiscoveryCard>();

    internal List<int> DiceList { get; } = new List<int>();

    internal List<int> ChosenList { get; } = new List<int>();

    internal List<JournalEntry> JournalList { get; } = new List<JournalEntry>();

    internal List<GameEvent> EventList { get; } = new List<GameEvent>();
}
