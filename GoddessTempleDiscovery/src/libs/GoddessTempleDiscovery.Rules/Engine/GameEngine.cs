using System;
using System.Collections.Generic;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Scoring;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>
/// The rules of Goddess Temple Discovery: deals the decks, runs seasons and turns, checks and applies every action,
/// raises events for the presentation, keeps the Field Journal, and scores. Deterministic for a seed and an action
/// sequence.
/// </summary>
public sealed partial class GameEngine
{
    private static readonly string[] FallbackColours = { "#B5482E", "#2E6FB5", "#3E8E4F", "#8E3EA0" };

    private readonly ICardCatalog _catalog;
    private readonly GameRandom _random;
    private readonly GameState _state;
    private readonly List<GameEvent> _applyEvents = new List<GameEvent>();
    private readonly HashSet<string> _journalKeys = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>Starts a game: shuffles the seat order and the decks, deals the rows, and flips the first Season card.</summary>
    /// <remarks>
    /// Decisions: with no seed the engine takes <see cref="Environment.TickCount"/> and reports it in
    /// <see cref="GameState.Seed"/>. A seat whose profile id is not in the catalog gets a plain profile of its own.
    /// An empty team name takes the profile's name; a name already taken gets " 2", " 3" appended. The starting
    /// Tablets are drawn before the first Season card flips, so their journal entries come first.
    /// </remarks>
    /// <param name="setup">The options of the game.</param>
    /// <param name="catalog">The decks to deal from; null reads the game's own content.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="setup"/> is null.</exception>
    /// <exception cref="ArgumentException">When the seats or the catalog are not playable.</exception>
    public GameEngine(GameSetup setup, ICardCatalog catalog = null)
    {
        ArgumentNullException.ThrowIfNull(setup);
        if (setup.Seats == null || setup.Seats.Count < GameRules.MinSeats || setup.Seats.Count > GameRules.MaxSeats)
        {
            throw new ArgumentException("A game has two to four seats.", nameof(setup));
        }

        if (setup.Seats.Any(s => s == null))
        {
            throw new ArgumentException("A seat is missing.", nameof(setup));
        }

        if (setup.TurnsPerSeason < GameRules.MinTurnsPerSeason || setup.TurnsPerSeason > GameRules.MaxTurnsPerSeason)
        {
            throw new ArgumentException("Turns per season run from 1 to 3.", nameof(setup));
        }

        if (!Enum.IsDefined(setup.Difficulty))
        {
            throw new ArgumentException("Unknown difficulty.", nameof(setup));
        }

        _catalog = catalog ?? CatalogSnapshot.FromCatalog();
        if (_catalog.Seasons == null || _catalog.Seasons.Count == 0)
        {
            throw new ArgumentException("The catalog holds no seasons.", nameof(catalog));
        }

        var seed = setup.Seed ?? Environment.TickCount;
        _random = new GameRandom(seed);
        _state = new GameState(setup, seed, _catalog.Seasons.Where(s => s != null).ToArray());
        SetUp();
    }

    /// <summary>The table, for the presentation to read.</summary>
    public GameState State => _state;

    /// <summary>Clears <see cref="GameState.Events"/> once the presentation has shown them.</summary>
    public void ClearEvents() => _state.EventList.Clear();

    /// <summary>The scores as they stand (final once the game is over); valid at any time.</summary>
    /// <returns>One score per team, in seat order.</returns>
    public FinalScore[] FinalScores() => FinalScoring.Compute(_state.Teams);

    private SeasonEffect Effect => _state.Season?.Effect ?? SeasonEffect.None;

    private void SetUp()
    {
        var seats = _state.Setup.Seats;
        var order = Enumerable.Range(0, seats.Count).ToList();
        _random.Shuffle(order);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < order.Count; i++)
        {
            var setupIndex = order[i];
            var seat = seats[setupIndex];
            var profile = seat.TeamProfileId == null
                ? null
                : _catalog.Teams.FirstOrDefault(t => string.Equals(t.Id, seat.TeamProfileId, StringComparison.Ordinal));
            var name = !string.IsNullOrWhiteSpace(seat.TeamName)
                ? seat.TeamName.Trim()
                : !string.IsNullOrWhiteSpace(profile?.Name) ? profile.Name : "Team " + (setupIndex + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
            var unique = name;
            for (var n = 2; !names.Add(unique); n++)
            {
                unique = name + " " + n.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            profile ??= new TeamProfile(
                seat.TeamProfileId ?? "team-" + (setupIndex + 1).ToString(System.Globalization.CultureInfo.InvariantCulture),
                unique,
                string.Empty,
                FallbackColours[setupIndex % FallbackColours.Length],
                string.Empty);
            var team = new TeamState(profile, unique, seat.Kind, seat.Temperament, i, setupIndex)
            {
                Workers = GameRules.StartingWorkers,
            };
            _state.TeamList.Add(team);
        }

        _state.SiteDeck.AddRange(SiteDeckBuilder.Build(_catalog.Discoveries, _random));
        AddShuffled(_state.TabletDeck, _catalog.Tablets);
        AddShuffled(_state.ExpeditionDeck, _catalog.Specialists);
        AddShuffled(_state.FavorDeck, _catalog.Favors);

        for (var slot = 0; slot < GameRules.SiteRowSize; slot++)
        {
            _state.SiteRowArray[slot] = DrawSite();
        }

        for (var slot = 0; slot < GameRules.ExpeditionRowSize; slot++)
        {
            _state.ExpeditionRowArray[slot] = DrawSpecialist();
        }

        foreach (var team in _state.TeamList)
        {
            for (var t = 0; t < GameRules.StartingTablets; t++)
            {
                GiveTablet(team, _state.Seasons[0].Year);
            }
        }

        StartSeason(0);
    }

    private void AddShuffled<T>(List<T> deck, IReadOnlyList<T> cards)
    {
        if (cards != null)
        {
            deck.AddRange(cards.Where(c => c != null));
        }

        _random.Shuffle(deck);
    }

    private void Raise(GameEvent gameEvent)
    {
        _applyEvents.Add(gameEvent);
        _state.EventList.Add(gameEvent);
    }

    private void Write(JournalEntry entry, string onceKey)
    {
        if (onceKey != null && !_journalKeys.Add(onceKey))
        {
            return;
        }

        _state.JournalList.Add(entry);
        Raise(new JournalEntryAdded(entry));
    }
}
