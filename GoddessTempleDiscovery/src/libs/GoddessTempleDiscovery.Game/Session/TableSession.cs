using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CodeBrix.Platform.GameEngine.CardsAndDice.Cards;
using CodeBrix.Platform.GameEngine.CardsAndDice.Dice;
using CodeBrix.Platform.GameEngine.CardsAndDice.Layout;
using CodeBrix.Platform.GameEngine.CardsAndDice.Table;
using GoddessTempleDiscovery.Game.Cards;
using GoddessTempleDiscovery.Game.Hosting;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Game.Session;

/// <summary>
/// One game on the table: owns the <see cref="GameEngine"/> and lays its state out on the
/// <see cref="CardsAndDiceTable"/>. Every Rules card is one add-on <see cref="Card"/> of a <see cref="Deck"/> per
/// Rules deck; every engine event becomes a queued table animation (a deal, a flip, a roll, a gather), drained one
/// step at a time by <see cref="Update"/> on the engine thread. The session never decides a rule: actions go through
/// <see cref="Apply"/> to the engine, and only the events that come back move cards.
/// </summary>
/// <remarks>
/// <para>
/// Dice: the engine rolls (<see cref="RollAction"/> carries no value). The table's three traditional dice then tumble
/// through <see cref="CardsAndDiceTable.Roll"/>, and each die's logical face is set to the engine's value by
/// rolling it once more through <c>Die.Roll</c> with a <see cref="FixedFaceRandom"/> that can only answer that face.
/// The add-on keeps no state of a roll besides the face index, so the tumble ends on the engine's value.
/// </para>
/// <para>
/// Hands: only the current team's hand and specialist shelf are on the table (a Fan and a Row); the other teams'
/// cards wait in piles with no area, and change places when a turn starts. Cards the engine moves while their team
/// is not shown (the starting Tablets, the last season's reports) move without animation.
/// </para>
/// </remarks>
public sealed class TableSession
{
    private const string CelestialBack = "backs/celestial.svg";

    private readonly CardsAndDiceTable _table;
    private readonly PresentationQueue _queue;
    private readonly Dictionary<string, Card> _cardsById = new Dictionary<string, Card>(StringComparer.Ordinal);
    private readonly Dictionary<string, CardPile> _handStash = new Dictionary<string, CardPile>(StringComparer.Ordinal);
    private readonly Dictionary<string, CardPile> _shelfStash = new Dictionary<string, CardPile>(StringComparer.Ordinal);
    private readonly Dictionary<string, TableArea> _reports = new Dictionary<string, TableArea>(StringComparer.Ordinal);
    private readonly HashSet<string> _translated = new HashSet<string>(StringComparer.Ordinal);
    private readonly CardPile _out = new CardPile("Out of the game");
    private readonly TableArea[] _siteSlots = new TableArea[GameRules.SiteRowSize];
    private readonly TableArea[] _expeditionSlots = new TableArea[GameRules.ExpeditionRowSize];
    private readonly TableDie[] _dice = new TableDie[3];
    private readonly Deck _siteDeck;
    private readonly Deck _tabletDeck;
    private readonly Deck _expeditionDeck;
    private readonly Deck _favorDeck;
    private readonly ICardCatalog _catalog;
    private TableArea _tell;
    private TableArea _tabletArea;
    private TableArea _expeditionDeckArea;
    private TableArea _favorDeckArea;
    private TableArea _favorShow;
    private TableArea _favorDiscard;
    private TableArea _hand;
    private TableArea _shelf;
    private bool _started;

    /// <summary>Creates the session: builds the decks, the areas and the dice. Call <see cref="Start"/> next.</summary>
    /// <param name="engine">The game, freshly created.</param>
    /// <param name="artwork">The table and its registered faces (every face of the catalog must be registered).</param>
    /// <param name="catalog">The decks the engine was dealt from; null for the game's own content (the engine's default).</param>
    /// <param name="width">The table width (1280 or wider).</param>
    public TableSession(GameEngine engine, TableArtwork artwork, ICardCatalog catalog = null, float width = TableLayout.BaseWidth)
    {
        Engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _catalog = catalog ?? CatalogSnapshot.FromCatalog();
        Artwork = artwork ?? throw new ArgumentNullException(nameof(artwork));
        _table = artwork.Table;
        _queue = new PresentationQueue(() => _table.IsAnimating);
        _queue.Flushed += (seconds, steps) =>
        {
            if (seconds > 2)
            {
                GameLog.Write($"animation queue flushed after {seconds:0.0} s ({steps} steps)");
            }
        };
        Layout = new TableLayout(width);

        var state = engine.State;
        _siteDeck = BuildDeck("Site deck", AllDiscoveries(state), d => new CardDefinition(d.Id, d.Title, CardFaceComposer.FaceKey(d.Id), CelestialBack) { Data = d });
        _tabletDeck = BuildDeck("Tablet deck", AllTablets(state), t => new CardDefinition(t.Id, t.Title, CardFaceComposer.FaceKey(t.Id), CelestialBack) { Data = t });
        _expeditionDeck = BuildDeck("Expedition deck", AllSpecialists(state),
            s => new CardDefinition("specialist-" + CardText.RoleKey(s.Role), s.Title, CardFaceComposer.FaceKey(s.Role), CelestialBack) { Data = s });
        _favorDeck = BuildDeck("Favor deck", AllFavors(state), f => new CardDefinition(f.Id, f.Title, CardFaceComposer.FaceKey(f), CelestialBack) { Data = f });
        foreach (var card in _siteDeck.Cards.Concat(_tabletDeck.Cards))
        {
            _cardsById[card.Definition.Key] = card;
        }

        BuildTable();
    }

    /// <summary>The game.</summary>
    public GameEngine Engine { get; }

    /// <summary>The table and its artwork.</summary>
    public TableArtwork Artwork { get; }

    /// <summary>The table.</summary>
    public CardsAndDiceTable Table => _table;

    /// <summary>Where everything sits.</summary>
    public TableLayout Layout { get; private set; }

    /// <summary>The season shown on the banner (it changes when the SeasonStarted step plays).</summary>
    public SeasonCard BannerSeason { get; private set; }

    /// <summary>The team whose hand and shelf are on the table.</summary>
    public string DisplayedTeam { get; private set; }

    /// <summary>How many dice are on the table (0 before a roll, 2, or 3 with a Favor's extra die).</summary>
    public int DiceInPlay { get; private set; }

    /// <summary>True while presentation steps are waiting or the table is animating.</summary>
    public bool IsBusy => !_queue.IsIdle || _table.IsAnimating;

    /// <summary>The pending presentation steps.</summary>
    public PresentationQueue Queue => _queue;

    /// <summary>The names of every engine event kind this session has translated into table steps.</summary>
    public IReadOnlyCollection<string> TranslatedEventKinds => _translated;

    /// <summary>The Site Row slot areas.</summary>
    public IReadOnlyList<TableArea> SiteSlots => _siteSlots;

    /// <summary>The Expedition Row slot areas.</summary>
    public IReadOnlyList<TableArea> ExpeditionSlots => _expeditionSlots;

    /// <summary>The current team's hand (a Fan).</summary>
    public TableArea HandArea => _hand;

    /// <summary>The current team's specialist shelf (a Row).</summary>
    public TableArea ShelfArea => _shelf;

    /// <summary>The Tell: the Site deck, face-down.</summary>
    public TableArea TellArea => _tell;

    /// <summary>The Tablet deck.</summary>
    public TableArea TabletArea => _tabletArea;

    /// <summary>The table's three dice (the third is parked off the table unless a Favor gave an extra die).</summary>
    public IReadOnlyList<TableDie> Dice => _dice;

    /// <summary>Raised when the presentation step of an engine event starts (on the engine thread).</summary>
    public event Action<GameEvent> EventPresented;

    /// <summary>The report stack area of a team.</summary>
    /// <param name="teamName">The team.</param>
    /// <returns>The area, or null.</returns>
    public TableArea ReportArea(string teamName) => teamName != null && _reports.TryGetValue(teamName, out var area) ? area : null;

    /// <summary>The label under each Site Row slot, from the engine's Site Row (null for an empty slot).</summary>
    /// <returns>Five labels.</returns>
    public IReadOnlyList<SlotLabel> SlotLabels()
    {
        var labels = new SlotLabel[GameRules.SiteRowSize];
        for (var slot = 0; slot < labels.Length; slot++)
        {
            var card = Engine.State.SiteRow[slot];
            labels[slot] = card == null ? null : CardText.SlotLabelFor(card, CardText.SlotDigNumber(Engine, slot));
        }

        return labels;
    }

    /// <summary>The Rules card behind a table card (a DiscoveryCard, TabletCard, SpecialistCard or FavorCard).</summary>
    /// <param name="card">The table card.</param>
    /// <returns>The Rules card, or null.</returns>
    public static object RulesCard(Card card) => card?.Definition.Data;

    /// <summary>The Site Row slot a table card lies in, or -1.</summary>
    /// <param name="card">The card.</param>
    /// <returns>The slot.</returns>
    public int SiteSlotOf(Card card) => card == null ? -1 : Array.FindIndex(_siteSlots, a => ReferenceEquals(a.Pile, card.Pile));

    /// <summary>The Expedition Row slot a table card lies in, or -1.</summary>
    /// <param name="card">The card.</param>
    /// <returns>The slot.</returns>
    public int ExpeditionSlotOf(Card card) => card == null ? -1 : Array.FindIndex(_expeditionSlots, a => ReferenceEquals(a.Pile, card.Pile));

    /// <summary>True when a table card is in the shown hand.</summary>
    /// <param name="card">The card.</param>
    /// <returns>True when in the hand.</returns>
    public bool IsInHand(Card card) => card != null && ReferenceEquals(card.Pile, _hand.Pile);

    /// <summary>The table card of a discovery or tablet id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The card, or null.</returns>
    public Card CardOf(string id) => id != null && _cardsById.TryGetValue(id, out var card) ? card : null;

    /// <summary>The index of a table die in the engine's <see cref="GameState.Dice"/>, or -1.</summary>
    /// <param name="die">The die.</param>
    /// <returns>The index.</returns>
    public int DieIndexOf(Die die)
    {
        var index = Array.FindIndex(_dice, d => ReferenceEquals(d.Die, die));
        return index < DiceInPlay ? index : -1;
    }

    /// <summary>
    /// Lays the start of the game out: the Site Row dealt face-down from the Tell, the Expedition Row dealt face-up,
    /// the starting Tablets into the teams' hands, then the events the engine raised while it set up (the first
    /// Season card, the first turn).
    /// </summary>
    public void Start()
    {
        if (_started)
        {
            throw new InvalidOperationException("The session has already started.");
        }

        _started = true;
        var state = Engine.State;
        foreach (var team in state.Teams)
        {
            foreach (var tablet in team.Tablets)
            {
                var card = Find(_tabletDeck, tablet);
                card.IsFaceUp = true;
                Stash(team.Name).Add(card, onTop: false);
            }
        }

        for (var slot = 0; slot < GameRules.SiteRowSize; slot++)
        {
            var card = state.SiteRow[slot];
            if (card != null)
            {
                var index = slot;
                _queue.Enqueue("deal site " + index, () => DealFrom(_siteDeck, _tell, Find(_siteDeck, card), _siteSlots[index], faceUp: false));
            }
        }

        for (var slot = 0; slot < GameRules.ExpeditionRowSize; slot++)
        {
            var card = state.ExpeditionRow[slot];
            if (card != null)
            {
                var index = slot;
                _queue.Enqueue("deal specialist " + index, () => DealFrom(_expeditionDeck, _expeditionDeckArea, FindAvailable(_expeditionDeck.DrawPile, card), _expeditionSlots[index], faceUp: true));
            }
        }

        var initial = state.Events.ToArray();
        Engine.ClearEvents();
        Translate(initial.Where(e => e is not TabletDrawn));
    }

    /// <summary>Applies one action to the engine and queues the table animation of every event it raised.</summary>
    /// <param name="action">A legal action.</param>
    /// <returns>The engine's result.</returns>
    public ApplyResult Apply(GameAction action)
    {
        var result = Engine.Apply(action);
        Engine.ClearEvents();
        Translate(result.Events);
        return result;
    }

    /// <summary>Advances the table's animations and the presentation queue (engine thread, or a headless test).</summary>
    /// <param name="seconds">The seconds since the last update.</param>
    public void Update(double seconds)
    {
        _table.Update(Math.Max(0, seconds));
        _queue.Update(Math.Max(0, seconds));
    }

    /// <summary>Plays every queued step to the end at once (tests, and reduced motion on a new game).</summary>
    /// <param name="maxSeconds">A guard against a step that never settles.</param>
    public void Settle(double maxSeconds = 600)
    {
        for (double t = 0; t < maxSeconds && IsBusy; t += 0.05)
        {
            _table.CompleteAnimations();
            Update(0.05);
        }
    }

    /// <summary>Moves every area to a new table width.</summary>
    /// <param name="width">The table width.</param>
    public void Relayout(float width)
    {
        Layout = new TableLayout(width);
        PlaceAreas();
        PlaceDice(DiceInPlay);
    }

    private static Deck BuildDeck<T>(string name, IEnumerable<T> cards, Func<T, CardDefinition> define) =>
        new Deck(cards.Select(define).ToArray(), name);

    //The engine's decks are internal, so the session builds its decks from the catalog the engine was dealt from
    //  (the cards already on the table are included in case a caller passed a narrower catalog)
    private IEnumerable<DiscoveryCard> AllDiscoveries(GameState state) =>
        state.SiteRow.Where(c => c != null).Concat(state.Teams.SelectMany(t => t.Hand)).Concat(_catalog.Discoveries)
            .Distinct(ReferenceEqualityComparer.Instance).Cast<DiscoveryCard>();

    private IEnumerable<TabletCard> AllTablets(GameState state) =>
        state.Teams.SelectMany(t => t.Tablets).Concat(_catalog.Tablets)
            .Distinct(ReferenceEqualityComparer.Instance).Cast<TabletCard>();

    //Copies of a role may be equal records: every instance is kept, by reference
    private IEnumerable<SpecialistCard> AllSpecialists(GameState state) =>
        state.ExpeditionRow.Where(c => c != null).Concat(_catalog.Specialists)
            .Distinct(ReferenceEqualityComparer.Instance).Cast<SpecialistCard>();

    private IEnumerable<FavorCard> AllFavors(GameState state) =>
        _catalog.Favors.Distinct(ReferenceEqualityComparer.Instance).Cast<FavorCard>();

    private void BuildTable()
    {
        _table.Clear();
        _table.InputEnabled = true;
        _table.DragEnabled = false;
        _table.SelectionMode = CardSelectionMode.None;

        _tell = Add(_siteDeck.DrawPile, CardLayout.Stack, TableLayout.SiteCard);
        _tabletArea = Add(_tabletDeck.DrawPile, CardLayout.Stack, new SkiaSharp.SKSize(90, 144));
        _expeditionDeckArea = Add(_expeditionDeck.DrawPile, CardLayout.Stack, TableLayout.ExpeditionCard);
        _favorDeckArea = Add(_favorDeck.DrawPile, CardLayout.Stack, TableLayout.SmallCard);
        _favorShow = Add(new CardPile("Favor shown"), CardLayout.Stack, TableLayout.ShowcaseCard);
        _favorDiscard = Add(_favorDeck.DiscardPile, CardLayout.Stack, TableLayout.SmallCard);
        for (var slot = 0; slot < _siteSlots.Length; slot++)
        {
            _siteSlots[slot] = Add(new CardPile("Site " + (slot + 1)), CardLayout.Stack, TableLayout.SiteCard);
            _siteSlots[slot].SingleCard = true;
        }

        for (var slot = 0; slot < _expeditionSlots.Length; slot++)
        {
            _expeditionSlots[slot] = Add(new CardPile("Expedition " + (slot + 1)), CardLayout.Stack, TableLayout.ExpeditionCard);
            _expeditionSlots[slot].SingleCard = true;
        }

        _hand = Add(new CardPile("Hand"), CardLayout.Fan, TableLayout.HandCard);
        _shelf = Add(new CardPile("Specialists"), CardLayout.Row, TableLayout.SmallCard);
        foreach (var team in Engine.State.Teams)
        {
            _reports[team.Name] = Add(new CardPile(team.Name + " reports"), CardLayout.Stack, TableLayout.SmallCard);
            _handStash[team.Name] = new CardPile(team.Name + " hand");
            _shelfStash[team.Name] = new CardPile(team.Name + " specialists");
        }

        foreach (var area in _table.Areas)
        {
            area.AcceptsDrops = false;
        }

        for (var i = 0; i < _dice.Length; i++)
        {
            _dice[i] = _table.AddDie(Die.Traditional(), TableLayout.DieParked, TableLayout.DieSize);
        }

        PlaceAreas();
    }

    private TableArea Add(CardPile pile, CardLayout layout, SkiaSharp.SKSize cardSize)
    {
        var area = _table.AddArea(pile, TableLayout.Point(Vector2.Zero), layout);
        area.CardWidth = cardSize.Width;
        area.CardHeight = cardSize.Height;
        return area;
    }

    private void PlaceAreas()
    {
        var layout = Layout;
        _tell.Bounds = TableLayout.Point(layout.TellCentre);
        _tabletArea.Bounds = TableLayout.Point(layout.TabletDeckCentre);
        _expeditionDeckArea.Bounds = TableLayout.Point(new Vector2(layout.Width + 160, layout.ExpeditionCentre(3).Y));
        _favorDeckArea.Bounds = TableLayout.Point(layout.FavorDeckCentre);
        _favorDiscard.Bounds = TableLayout.Point(layout.FavorDiscardCentre);
        _favorShow.Bounds = TableLayout.Point(layout.ShowcaseCentre);
        for (var slot = 0; slot < _siteSlots.Length; slot++)
        {
            _siteSlots[slot].Bounds = TableLayout.Point(layout.SiteSlotCentre(slot));
        }

        for (var slot = 0; slot < _expeditionSlots.Length; slot++)
        {
            _expeditionSlots[slot].Bounds = TableLayout.Point(layout.ExpeditionCentre(slot));
        }

        _hand.Bounds = layout.HandBounds;
        _shelf.Bounds = layout.ShelfBounds;
        var teams = Engine.State.Teams;
        for (var i = 0; i < teams.Count; i++)
        {
            _reports[teams[i].Name].Bounds = TableLayout.Point(layout.ReportCentre(i, teams.Count));
        }
    }

    private void PlaceDice(int count)
    {
        for (var i = 0; i < _dice.Length; i++)
        {
            _dice[i].Center = i < count ? Layout.DieCentre(i, count) : TableLayout.DieParked;
        }
    }

    private CardPile Stash(string teamName)
    {
        if (!_handStash.TryGetValue(teamName, out var pile))
        {
            pile = new CardPile(teamName + " hand");
            _handStash[teamName] = pile;
        }

        return pile;
    }

    private CardPile ShelfStash(string teamName)
    {
        if (!_shelfStash.TryGetValue(teamName, out var pile))
        {
            pile = new CardPile(teamName + " specialists");
            _shelfStash[teamName] = pile;
        }

        return pile;
    }

    //Where a team's hand cards are right now: on the table when it is the shown team, else in its stash
    private CardPile HandPileOf(string teamName) => teamName == DisplayedTeam ? _hand.Pile : Stash(teamName);

    private CardPile ShelfPileOf(string teamName) => teamName == DisplayedTeam ? _shelf.Pile : ShelfStash(teamName);

    private static Card Find(Deck deck, object rulesCard) =>
        deck.Cards.FirstOrDefault(c => ReferenceEquals(c.Definition.Data, rulesCard))
        ?? deck.Cards.FirstOrDefault(c => Equals(c.Definition.Data, rulesCard))
        ?? throw new InvalidOperationException($"No table card for {rulesCard}.");

    //A copy of a Rules card that is in a given pile (equal Specialist copies are interchangeable)
    private static Card FindAvailable(CardPile pile, object rulesCard) =>
        pile.Cards.FirstOrDefault(c => ReferenceEquals(c.Definition.Data, rulesCard))
        ?? pile.Cards.FirstOrDefault(c => Equals(c.Definition.Data, rulesCard));

    private Card FindSpecialist(object rulesCard, params CardPile[] piles) =>
        piles.Select(p => FindAvailable(p, rulesCard)).FirstOrDefault(c => c != null) ?? Find(_expeditionDeck, rulesCard);

    //Deals one particular card: it is brought to the top of its deck's draw pile first, because the add-on deals the top
    private void DealFrom(Deck deck, TableArea source, Card card, TableArea destination, bool faceUp, DealAnimation animation = null)
    {
        if (card == null)
        {
            return;
        }

        if (!ReferenceEquals(card.Pile, deck.DrawPile))
        {
            card.IsFaceUp = false;
        }

        deck.DrawPile.Add(card, onTop: true);
        _table.DealTo(deck, destination, null, faceUp, animation);
    }

    private double Hold(double seconds) => _table.ReducedMotion ? 0 : seconds / Math.Max(0.25, _table.AnimationSpeed);

    private void Translate(IEnumerable<GameEvent> events)
    {
        foreach (var gameEvent in events)
        {
            _translated.Add(gameEvent.GetType().Name);
            var captured = gameEvent;
            switch (gameEvent)
            {
                case SeasonStarted season:
                    _queue.Enqueue("season " + season.Season.Year, () =>
                    {
                        BannerSeason = season.Season;
                        Presented(captured);
                    }, Hold(0.6));
                    break;
                case TurnStarted turn:
                    _queue.Enqueue("turn " + turn.TeamName, () =>
                    {
                        ShowTeam(turn.TeamName);
                        DiceInPlay = 0;
                        PlaceDice(0);
                        Presented(captured);
                    });
                    break;
                case DiceRolled rolled:
                    _queue.Enqueue("roll", () =>
                    {
                        RollTo(rolled.Values, null);
                        Presented(captured);
                    }, Hold(0.2));
                    break;
                case DieRerolled rerolled:
                    _queue.Enqueue("reroll", () =>
                    {
                        var values = Engine.State.Dice.ToArray();
                        if (rerolled.Index >= 0 && rerolled.Index < values.Length)
                        {
                            values[rerolled.Index] = rerolled.Value;
                        }

                        RollTo(values, rerolled.Index);
                        Presented(captured);
                    }, Hold(0.2));
                    break;
                case SiteExcavated excavated:
                    _queue.Enqueue("flip " + excavated.Card.Id, () =>
                    {
                        var card = Find(_siteDeck, excavated.Card);
                        if (!ReferenceEquals(card.Pile, _siteSlots[excavated.Slot].Pile))
                        {
                            _siteSlots[excavated.Slot].Pile.Add(card, onTop: true);
                        }

                        _table.Flip(card, true);
                        Presented(captured);
                    }, Hold(0.5));
                    _queue.Enqueue("to hand " + excavated.Card.Id, () =>
                    {
                        HandPileOf(excavated.TeamName).Add(Find(_siteDeck, excavated.Card));
                        SortHand();
                    });
                    break;
                case SiteRefilled refilled:
                    _queue.Enqueue("refill site " + refilled.Slot, () =>
                    {
                        var slot = _siteSlots[refilled.Slot];
                        foreach (var occupant in slot.Pile.Cards.ToArray())
                        {
                            //The last season's Mask takes the place of a card that goes to the bottom of the Tell
                            occupant.IsFaceUp = false;
                            _tell.Pile.Add(occupant);
                        }

                        if (refilled.Card != null)
                        {
                            DealFrom(_siteDeck, _tell, Find(_siteDeck, refilled.Card), slot, faceUp: false);
                        }

                        Presented(captured);
                    });
                    break;
                case SpecialistRecruited recruited:
                    _queue.Enqueue("recruit " + recruited.Card.Title, () =>
                    {
                        var card = FindSpecialist(recruited.Card, _expeditionSlots[recruited.Slot].Pile, _expeditionDeck.DrawPile);
                        ShelfPileOf(recruited.TeamName).Add(card);
                        Presented(captured);
                    });
                    break;
                case ExpeditionRefilled expedition:
                    _queue.Enqueue("refill expedition " + expedition.Slot, () =>
                    {
                        if (expedition.Card != null)
                        {
                            var card = FindAvailable(_expeditionDeck.DrawPile, expedition.Card) ?? Find(_expeditionDeck, expedition.Card);
                            DealFrom(_expeditionDeck, _expeditionDeckArea, card, _expeditionSlots[expedition.Slot], faceUp: true);
                        }

                        Presented(captured);
                    });
                    break;
                case TabletDrawn drawn:
                    _queue.Enqueue("tablet " + drawn.Card.Id, () =>
                    {
                        var card = Find(_tabletDeck, drawn.Card);
                        if (drawn.TeamName == DisplayedTeam)
                        {
                            DealFrom(_tabletDeck, _tabletArea, card, _hand, faceUp: true);
                        }
                        else
                        {
                            card.IsFaceUp = true;
                            Stash(drawn.TeamName).Add(card);
                        }

                        Presented(captured);
                    });
                    _queue.Enqueue("sort hand", SortHand);
                    break;
                case TabletSpent spent:
                    _queue.Enqueue("spend tablet " + spent.Card.Id, () =>
                    {
                        var card = Find(_tabletDeck, spent.Card);
                        _table.Flip(card, false);
                        _tabletArea.Pile.Add(card);
                        Presented(captured);
                    });
                    break;
                case Surveyed surveyed:
                    _queue.Enqueue("survey " + surveyed.Slot, () =>
                    {
                        var removed = Find(_siteDeck, surveyed.Removed);
                        removed.IsFaceUp = false;
                        _tell.Pile.Add(removed);
                        Presented(captured);
                    }, Hold(0.2));
                    _queue.Enqueue("survey deal " + surveyed.Slot, () =>
                        DealFrom(_siteDeck, _tell, Find(_siteDeck, surveyed.Added), _siteSlots[surveyed.Slot], faceUp: false));
                    break;
                case ReportPublished published:
                    _queue.Enqueue("report " + published.Report.Title, () =>
                    {
                        var stack = ReportArea(published.TeamName)?.Pile ?? _out;
                        foreach (var discovery in published.Report.Cards)
                        {
                            var card = Find(_siteDeck, discovery);
                            card.IsFaceUp = true;
                            stack.Add(card, onTop: true);
                        }

                        SortHand();
                        Presented(captured);
                    }, Hold(0.4));
                    break;
                case FavorDrawn favor:
                    _queue.Enqueue("favor " + favor.Card.Id, () =>
                    {
                        DealFrom(_favorDeck, _favorDeckArea, Find(_favorDeck, favor.Card), _favorShow, faceUp: true,
                            new DealAnimation { Style = DealStyle.Toss, Duration = 0.6 });
                        Presented(captured);
                    }, Hold(1.4));
                    _queue.Enqueue("favor away " + favor.Card.Id, () => _favorDeck.DiscardPile.Add(Find(_favorDeck, favor.Card), onTop: true));
                    break;
                case HandLimitDiscard discard:
                    _queue.Enqueue("discard " + discard.CardId, () =>
                    {
                        var card = CardOf(discard.CardId);
                        if (card != null && card.Definition.Data is TabletCard)
                        {
                            _table.Flip(card, false);
                            _tabletArea.Pile.Add(card);
                        }
                        else if (card != null)
                        {
                            _out.Add(card);
                        }

                        SortHand();
                        Presented(captured);
                    });
                    break;
                case TurnEnded:
                    _queue.Enqueue("turn ended", () =>
                    {
                        foreach (var card in _hand.Pile.Cards)
                        {
                            card.IsSelected = false;
                        }

                        Presented(captured);
                    });
                    break;
                default:
                    //WorkerGained, WorkersSpent, JournalEntryAdded and GameEnded move no card: the HUD and the
                    //  chrome show them when their step comes up
                    _queue.Enqueue(gameEvent.GetType().Name, () => Presented(captured));
                    break;
            }
        }
    }

    private void Presented(GameEvent gameEvent) => EventPresented?.Invoke(gameEvent);

    private void ShowTeam(string teamName)
    {
        if (teamName == DisplayedTeam)
        {
            return;
        }

        if (DisplayedTeam != null)
        {
            Stash(DisplayedTeam).Collect(_hand.Pile);
            ShelfStash(DisplayedTeam).Collect(_shelf.Pile);
        }

        DisplayedTeam = teamName;
        _hand.Pile.Collect(Stash(teamName));
        _shelf.Pile.Collect(ShelfStash(teamName));
        SortHand();
    }

    //Discoveries first, in the order they were excavated, then Tablets; cards the engine no longer has in the hand
    //  are left where they are (a step on its way will move them)
    private void SortHand()
    {
        var team = Engine.State.Teams.FirstOrDefault(t => t.Name == DisplayedTeam);
        if (team == null)
        {
            return;
        }

        var order = team.Hand.Select(d => (object)d).Concat(team.Tablets).ToList();
        var cards = _hand.Pile.Cards
            .OrderBy(c =>
            {
                var index = order.FindIndex(o => ReferenceEquals(o, c.Definition.Data));
                return index < 0 ? int.MaxValue : index;
            })
            .ToArray();
        foreach (var card in cards)
        {
            _hand.Pile.Add(card);
        }
    }

    private void RollTo(IReadOnlyList<int> values, int? onlyIndex)
    {
        var count = Math.Min(values.Count, _dice.Length);
        DiceInPlay = count;
        PlaceDice(count);
        for (var i = 0; i < _dice.Length; i++)
        {
            _dice[i].Die.IsHeld = i >= count || (onlyIndex != null && onlyIndex != i);
        }

        _table.Roll();
        for (var i = 0; i < count; i++)
        {
            if (onlyIndex == null || onlyIndex == i)
            {
                ForceFace(_dice[i].Die, values[i]);
            }
        }

        foreach (var die in _dice)
        {
            die.Die.IsHeld = false;
        }
    }

    private static void ForceFace(Die die, int value)
    {
        var index = -1;
        for (var f = 0; f < die.Faces.Count; f++)
        {
            if (die.Faces[f].Value == value)
            {
                index = f;
                break;
            }
        }

        if (index >= 0)
        {
            die.Roll(new FixedFaceRandom(index));
        }
    }
}
