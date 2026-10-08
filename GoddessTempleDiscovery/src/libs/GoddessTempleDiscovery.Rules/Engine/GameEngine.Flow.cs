using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Journal;

namespace GoddessTempleDiscovery.Rules.Engine;

public sealed partial class GameEngine
{
    /// <summary>
    /// Flips a Season card and applies its effect. Decisions: FreeSurvey gives every team one free survey that lasts
    /// the season; RerollOnce gives every team one re-roll that lasts the season; GainWorker is paid at once.
    /// </summary>
    private void StartSeason(int index)
    {
        var s = _state;
        s.SeasonIndex = index;
        s.TurnInSeason = 0;
        s.TurnOrderPosition = 0;
        var season = s.Season;
        Raise(new SeasonStarted(season));
        Write(JournalWriter.ForSeason(season), "season:" + index.ToString(System.Globalization.CultureInfo.InvariantCulture));
        switch (season.Effect)
        {
            case SeasonEffect.FreeSurvey:
                foreach (var team in s.TeamList)
                {
                    team.SeasonFreeSurvey = true;
                }

                break;
            case SeasonEffect.GainWorker:
                foreach (var team in s.TeamList)
                {
                    team.Workers++;
                    Raise(new WorkerGained(team.Name, 1));
                }

                break;
            case SeasonEffect.RerollOnce:
                foreach (var team in s.TeamList)
                {
                    team.RerollAvailable = true;
                }

                break;
            case SeasonEffect.FinalSeason:
                BringFinalSeasonDiscovery();
                break;
        }

        BeginTurn(GamePhase.SeasonStart);
    }

    private void BeginTurn(GamePhase phase)
    {
        _state.Phase = phase;
        Raise(new TurnStarted(_state.CurrentTeam.Name));
    }

    /// <summary>Decision: the PlusTwoNextDig Favor lasts the rest of this turn and the team's next turn, then lapses.</summary>
    private void FinishTurn()
    {
        var s = _state;
        var team = s.CurrentTeam;
        team.ForemanUsedThisTurn = false;
        team.SurveyorUsedThisTurn = false;
        team.NoHandLimitThisTurn = false;
        if (team.PlusTwoTurnsLeft > 0)
        {
            team.PlusTwoTurnsLeft--;
        }

        s.DiceList.Clear();
        s.ChosenList.Clear();
        s.DieAUsed = false;
        s.DieBUsed = false;
        s.FavorDrawnThisTurn = false;
        Raise(new TurnEnded(team.Name));

        s.TurnOrderPosition++;
        if (s.TurnOrderPosition < s.TeamList.Count)
        {
            BeginTurn(GamePhase.AwaitRoll);
            return;
        }

        s.TurnOrderPosition = 0;
        s.TurnInSeason++;
        if (s.TurnInSeason < s.Setup.TurnsPerSeason)
        {
            BeginTurn(GamePhase.AwaitRoll);
            return;
        }

        EndSeason();
    }

    /// <summary>
    /// Ends the season: every team gains its Worker and the season's grants lapse. Nothing is published for a team
    /// automatically, not even in the final season: finds left in a hand at the end score half (DESIGN section 7),
    /// which is what keeps publishing worth doing.
    /// </summary>
    private void EndSeason()
    {
        var s = _state;
        foreach (var team in s.TeamList)
        {
            team.Workers += GameRules.SeasonEndWorkers;
            Raise(new WorkerGained(team.Name, GameRules.SeasonEndWorkers));
            team.RerollAvailable = false;
            team.SeasonFreeSurvey = false;
        }

        if (s.SeasonIndex >= s.SeasonCount - 1)
        {
            s.Phase = GamePhase.GameOver;
            Raise(new GameEnded(FinalScores()));
            return;
        }

        StartSeason(s.SeasonIndex + 1);
    }

    /// <summary>
    /// Decision: if the Mask (the catalog's FinalSeasonDiscoveryId) is still in the Site deck, it takes the place of
    /// the shallowest card in the row (the leftmost on a tie), which goes to the bottom of the deck.
    /// </summary>
    private void BringFinalSeasonDiscovery()
    {
        var s = _state;
        var id = _catalog.FinalSeasonDiscoveryId;
        if (id == null)
        {
            return;
        }

        var index = s.SiteDeck.FindIndex(c => c.Id == id);
        if (index < 0)
        {
            return;
        }

        var mask = s.SiteDeck[index];
        s.SiteDeck.RemoveAt(index);
        var slot = -1;
        for (var i = 0; i < GameRules.SiteRowSize; i++)
        {
            if (s.SiteRowArray[i] == null)
            {
                slot = i;
                break;
            }

            if (slot < 0 || s.SiteRowArray[i].Tier < s.SiteRowArray[slot].Tier)
            {
                slot = i;
            }
        }

        var old = s.SiteRowArray[slot];
        if (old != null)
        {
            s.SiteDeck.Add(old);
        }

        s.SiteRowArray[slot] = mask;
        Raise(new SiteRefilled(null, slot, mask));
    }

    /// <summary>
    /// Decision: the Favor deck is reshuffled from the drawn Favors when it runs out; an empty Favor deck draws nothing.
    /// </summary>
    private void DrawFavor(TeamState team)
    {
        var s = _state;
        s.FavorDrawnThisTurn = true;
        if (s.FavorDeck.Count == 0 && s.FavorDiscard.Count > 0)
        {
            s.FavorDeck.AddRange(s.FavorDiscard);
            s.FavorDiscard.Clear();
            _random.Shuffle(s.FavorDeck);
        }

        if (s.FavorDeck.Count == 0)
        {
            return;
        }

        var card = s.FavorDeck[0];
        s.FavorDeck.RemoveAt(0);
        s.FavorDiscard.Add(card);
        Raise(new FavorDrawn(team.Name, card));
        Write(JournalWriter.ForFavor(card, s.Season.Year, team.Name), "favor:" + card.Id);
        ApplyFavor(team, card.Effect);
    }

    /// <summary>
    /// Decisions: RecruitDiscount makes the team's next recruit (this turn or later) one cheaper, since the dice that
    /// drew it are spent; OnePoint is added to the published points; a Favor FreeSurvey keeps until used.
    /// </summary>
    private void ApplyFavor(TeamState team, FavorEffect effect)
    {
        switch (effect)
        {
            case FavorEffect.ExtraDieNextTurn:
                team.ExtraDieNextTurn = true;
                break;
            case FavorEffect.FreeTablet:
                GiveTablet(team, _state.Season.Year);
                break;
            case FavorEffect.FreeWorker:
                team.Workers++;
                Raise(new WorkerGained(team.Name, 1));
                break;
            case FavorEffect.FreeSurvey:
                team.FavorFreeSurveys++;
                break;
            case FavorEffect.PlusTwoNextDig:
                team.PlusTwoTurnsLeft = 2;
                break;
            case FavorEffect.RecruitDiscount:
                team.RecruitDiscountPending = true;
                break;
            case FavorEffect.OnePoint:
                team.PublishedPoints++;
                break;
            case FavorEffect.NoHandLimit:
                team.NoHandLimitThisTurn = true;
                break;
        }
    }

    private DiscoveryCard DrawSite()
    {
        var deck = _state.SiteDeck;
        if (deck.Count == 0)
        {
            return null;
        }

        var card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    private SpecialistCard DrawSpecialist()
    {
        var deck = _state.ExpeditionDeck;
        if (deck.Count == 0)
        {
            return null;
        }

        var card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    /// <summary>
    /// Decisions: spent and discarded Tablets are reshuffled into a new Tablet deck when it runs out; a Tablet, Favor or
    /// Specialist is written to the Journal the first time it is read, so the printed journal has no repeats.
    /// </summary>
    private bool GiveTablet(TeamState team, string seasonYear)
    {
        var s = _state;
        if (s.TabletDeck.Count == 0 && s.TabletDiscard.Count > 0)
        {
            s.TabletDeck.AddRange(s.TabletDiscard);
            s.TabletDiscard.Clear();
            _random.Shuffle(s.TabletDeck);
        }

        if (s.TabletDeck.Count == 0)
        {
            return false;
        }

        var card = s.TabletDeck[0];
        s.TabletDeck.RemoveAt(0);
        team.TabletList.Add(card);
        Raise(new TabletDrawn(team.Name, card));
        Write(JournalWriter.ForTablet(card, seasonYear, team.Name), "tablet:" + card.Id);
        return true;
    }
}
