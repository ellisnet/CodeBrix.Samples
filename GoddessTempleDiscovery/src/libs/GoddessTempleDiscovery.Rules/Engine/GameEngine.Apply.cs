using System;
using System.Collections.Generic;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Scoring;

namespace GoddessTempleDiscovery.Rules.Engine;

public sealed partial class GameEngine
{
    /// <summary>Applies one action of the current team.</summary>
    /// <param name="action">The action; it must be legal now.</param>
    /// <returns>The action and the events it raised.</returns>
    /// <exception cref="InvalidOperationException">When the action is not legal now; the state is unchanged.</exception>
    public ApplyResult Apply(GameAction action)
    {
        var reason = Validate(action);
        if (reason != null)
        {
            throw new InvalidOperationException(reason);
        }

        _applyEvents.Clear();
        switch (action)
        {
            case RollAction:
                ApplyRoll();
                break;
            case RerollAction reroll:
                ApplyReroll(reroll);
                break;
            case DigAction dig:
                ApplyDig(dig);
                break;
            case RecruitAction recruit:
                ApplyRecruit(recruit);
                break;
            case StudyAction study:
                ApplyStudy(study);
                break;
            case SurveyAction survey:
                ApplySurvey(survey);
                break;
            case PublishAction publish:
                ApplyPublish(publish);
                break;
            case DiscardAction discard:
                ApplyDiscard(discard);
                break;
            case EndTurnAction:
                ApplyEndTurn();
                break;
        }

        _state.ActionCount++;
        return new ApplyResult(action, _applyEvents.ToArray());
    }

    /// <summary>
    /// Decision: with an extra die the three dice stay visible and the best two are kept automatically (on equal
    /// values the earlier die); Die A is the kept die with the lower index.
    /// </summary>
    private void ApplyRoll()
    {
        var s = _state;
        var team = s.CurrentTeam;
        s.DiceList.Clear();
        s.ChosenList.Clear();
        var count = team.ExtraDieNextTurn ? 3 : 2;
        team.ExtraDieNextTurn = false;
        for (var i = 0; i < count; i++)
        {
            s.DiceList.Add(_random.RollDie());
        }

        var kept = Enumerable.Range(0, count)
            .OrderByDescending(i => s.DiceList[i])
            .ThenBy(i => i)
            .Take(2)
            .OrderBy(i => i);
        s.ChosenList.AddRange(kept);
        s.DieAUsed = false;
        s.DieBUsed = false;
        s.FavorDrawnThisTurn = false;
        s.Phase = GamePhase.Spend;
        Raise(new DiceRolled(team.Name, s.DiceList.ToArray()));
    }

    /// <summary>Decision: a re-rolled die stays kept even when an unkept third die now shows more.</summary>
    private void ApplyReroll(RerollAction reroll)
    {
        var team = _state.CurrentTeam;
        var value = _random.RollDie();
        _state.DiceList[reroll.DieIndex] = value;
        team.RerollAvailable = false;
        Raise(new DieRerolled(team.Name, reroll.DieIndex, value));
    }

    private void ApplyDig(DigAction dig)
    {
        var s = _state;
        var team = s.CurrentTeam;
        var eval = EvaluateDig(team, dig.Slot, dig.Dice, dig.Workers, dig.TabletId);
        SpendDice(dig.Dice);
        if (dig.Workers > 0 && Effect != SeasonEffect.WorkersCapped)
        {
            team.Workers -= dig.Workers;
            Raise(new WorkersSpent(team.Name, dig.Workers));
        }

        if (eval.Tablet != null)
        {
            team.TabletList.Remove(eval.Tablet);
            s.TabletDiscard.Add(eval.Tablet);
            Raise(new TabletSpent(team.Name, eval.Tablet));
        }

        if (eval.PlusTwoUsed)
        {
            team.PlusTwoTurnsLeft = 0;
        }

        if (eval.ForemanUsed)
        {
            team.ForemanUsedThisTurn = true;
        }

        var card = s.SiteRowArray[dig.Slot];
        s.SiteRowArray[dig.Slot] = null;
        team.HandList.Add(card);
        Raise(new SiteExcavated(team.Name, dig.Slot, card, eval.Preview));
        Write(JournalWriter.ForDiscovery(card, s.Season.Year, team.Name), null);
        var next = DrawSite();
        s.SiteRowArray[dig.Slot] = next;
        Raise(new SiteRefilled(team.Name, dig.Slot, next));
        AfterDiceSpent(team);
    }

    private void ApplyRecruit(RecruitAction recruit)
    {
        var s = _state;
        var team = s.CurrentTeam;
        SpendDice(recruit.Dice);
        var card = s.ExpeditionRowArray[recruit.Slot];
        s.ExpeditionRowArray[recruit.Slot] = null;
        team.SpecialistList.Add(card);
        team.RecruitDiscountPending = false;
        Raise(new SpecialistRecruited(team.Name, recruit.Slot, card));
        Write(JournalWriter.ForSpecialist(card, s.Season.Year, team.Name), "specialist:" + card.Role);
        var next = DrawSpecialist();
        s.ExpeditionRowArray[recruit.Slot] = next;
        Raise(new ExpeditionRefilled(team.Name, recruit.Slot, next));
        AfterDiceSpent(team);
    }

    private void ApplyStudy(StudyAction study)
    {
        var team = _state.CurrentTeam;
        var draws = StudyDraws(team, DiceValue(study.Dice));
        SpendDice(study.Dice);
        for (var i = 0; i < draws; i++)
        {
            GiveTablet(team, _state.Season.Year);
        }

        AfterDiceSpent(team);
    }

    /// <summary>
    /// Decision: a free survey uses the grant that expires soonest: the Surveyor's once-per-turn use, then the
    /// season's, then a Favor's (Favor surveys keep until used).
    /// </summary>
    private void ApplySurvey(SurveyAction survey)
    {
        var s = _state;
        var team = s.CurrentTeam;
        if (survey.Dice == null)
        {
            if (team.HasRole(SpecialistRole.Surveyor) && !team.SurveyorUsedThisTurn)
            {
                team.SurveyorUsedThisTurn = true;
            }
            else if (team.SeasonFreeSurvey)
            {
                team.SeasonFreeSurvey = false;
            }
            else
            {
                team.FavorFreeSurveys--;
            }
        }
        else
        {
            SpendDice(survey.Dice.Value);
        }

        var removed = s.SiteRowArray[survey.Slot];
        s.SiteDeck.Add(removed);
        var added = s.SiteDeck[0];
        s.SiteDeck.RemoveAt(0);
        s.SiteRowArray[survey.Slot] = added;
        Raise(new Surveyed(team.Name, survey.Slot, removed, added));
        if (survey.Dice != null)
        {
            AfterDiceSpent(team);
        }
    }

    private void ApplyPublish(PublishAction publish)
    {
        var team = _state.CurrentTeam;
        var cards = publish.CardIds.Select(id => team.HandList.First(c => c.Id == id)).ToList();
        PublishCards(team, cards);
    }

    /// <summary>Decision: discarded Discoveries leave the game; discarded Tablets go to the Tablet discard pile.</summary>
    private void ApplyDiscard(DiscardAction discard)
    {
        var s = _state;
        var team = s.CurrentTeam;
        var card = team.HandList.FirstOrDefault(c => c.Id == discard.CardId);
        if (card != null)
        {
            team.HandList.Remove(card);
            s.DiscardedDiscoveries.Add(card);
        }
        else
        {
            var tablet = team.TabletList.First(t => t.Id == discard.CardId);
            team.TabletList.Remove(tablet);
            s.TabletDiscard.Add(tablet);
        }

        Raise(new HandLimitDiscard(team.Name, discard.CardId));
        if (team.HandCount <= GameRules.HandLimit)
        {
            FinishTurn();
        }
    }

    /// <summary>
    /// Decision: doubles left unspent still draw their Favor when the turn ends (before the hand limit is checked, so a
    /// NoHandLimit Favor counts at once).
    /// </summary>
    private void ApplyEndTurn()
    {
        var s = _state;
        var team = s.CurrentTeam;
        if (s.IsDoubles && !s.FavorDrawnThisTurn)
        {
            DrawFavor(team);
        }

        if (team.HandCount > GameRules.HandLimit && !team.NoHandLimitThisTurn)
        {
            s.Phase = GamePhase.TurnEnd;
            return;
        }

        FinishTurn();
    }

    private void SpendDice(DiceChoice choice)
    {
        if (choice != DiceChoice.DieB)
        {
            _state.DieAUsed = true;
        }

        if (choice != DiceChoice.DieA)
        {
            _state.DieBUsed = true;
        }
    }

    private void AfterDiceSpent(TeamState team)
    {
        var s = _state;
        if (s.DieAUsed && s.DieBUsed && s.IsDoubles && !s.FavorDrawnThisTurn)
        {
            DrawFavor(team);
        }
    }

    private void PublishCards(TeamState team, IReadOnlyList<DiscoveryCard> cards)
    {
        var (kind, points) = ReportScoring.Score(cards, team.HasRole(SpecialistRole.Photographer), Effect == SeasonEffect.PublishBonus);
        foreach (var card in cards)
        {
            team.HandList.Remove(card);
        }

        var report = new Report(team.ReportList.Count + 1, cards.ToArray(), kind, points, _state.Season.Year);
        team.ReportList.Add(report);
        team.PublishedPoints += points;
        Raise(new ReportPublished(team.Name, report));
        Write(JournalWriter.ForReport(report, team.Name), null);
    }
}
