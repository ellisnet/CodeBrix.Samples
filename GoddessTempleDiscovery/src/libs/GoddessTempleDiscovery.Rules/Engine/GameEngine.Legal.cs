using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Scoring;

namespace GoddessTempleDiscovery.Rules.Engine;

public sealed partial class GameEngine
{
    private static readonly DiceChoice[] AllChoices = { DiceChoice.DieA, DiceChoice.DieB, DiceChoice.Both };

    /// <summary>Every action the current team may take now, in a stable order.</summary>
    /// <remarks>
    /// Rolls in <see cref="GamePhase.SeasonStart"/> and <see cref="GamePhase.AwaitRoll"/>; one discard per card in
    /// <see cref="GamePhase.TurnEnd"/>; in <see cref="GamePhase.Spend"/>: re-rolls, every successful dig (each die
    /// choice, each Tablet or none, each Worker count), recruits, studies, surveys, every publishable subset of the
    /// hand, and ending the turn. Nothing once the game is over.
    /// </remarks>
    /// <returns>The legal actions.</returns>
    public IReadOnlyList<GameAction> LegalActions()
    {
        var list = new List<GameAction>();
        var s = _state;
        var team = s.CurrentTeam;
        switch (s.Phase)
        {
            case GamePhase.SeasonStart:
            case GamePhase.AwaitRoll:
                list.Add(new RollAction());
                break;
            case GamePhase.TurnEnd:
                list.AddRange(team.HandList.Select(c => new DiscardAction(c.Id)));
                list.AddRange(team.TabletList.Select(t => new DiscardAction(t.Id)));
                break;
            case GamePhase.Spend:
                AddSpendActions(list, team);
                break;
        }

        return list;
    }

    /// <summary>True when <paramref name="action"/> may be applied now.</summary>
    /// <param name="action">The action.</param>
    /// <returns>True when legal.</returns>
    public bool IsLegal(GameAction action) => Validate(action) == null;

    /// <summary>Why <paramref name="action"/> may not be applied now, or null when it may.</summary>
    /// <param name="action">The action.</param>
    /// <returns>A plain reason, or null.</returns>
    public string WhyIllegal(GameAction action) => Validate(action);

    /// <summary>What a dig by the current team would come to; <see cref="DigPreview.CanDig"/> says whether it is legal.</summary>
    /// <remarks>
    /// Decisions: the Foreman's +1 is added only when the dig would fall short without it, so it is never wasted, and
    /// it then counts as this turn's use. A pending Favor +2 is always used by the next dig. In the WorkersCapped
    /// season a team still needs the Workers it adds, but they are not spent.
    /// </remarks>
    /// <param name="slot">The Site Row slot.</param>
    /// <param name="dice">The die, or both dice.</param>
    /// <param name="workers">The Workers to add.</param>
    /// <param name="tabletId">A Tablet to spend, or null.</param>
    /// <returns>The preview.</returns>
    public DigPreview PreviewDig(int slot, DiceChoice dice, int workers, string tabletId) =>
        EvaluateDig(_state.CurrentTeam, slot, dice, workers, tabletId).Preview;

    /// <summary>What publishing these Discoveries from the current team's hand would score now.</summary>
    /// <param name="cardIds">The ids.</param>
    /// <returns>The preview; <see cref="ReportPreview.CanPublish"/> says whether it is legal.</returns>
    public ReportPreview PreviewReport(IReadOnlyList<string> cardIds)
    {
        var team = _state.CurrentTeam;
        if (cardIds == null)
        {
            return new ReportPreview(ReportKind.Plain, 0, false);
        }

        var cards = cardIds.Select(id => team.HandList.FirstOrDefault(c => c.Id == id)).ToArray();
        if (cards.Any(c => c == null))
        {
            return new ReportPreview(ReportKind.Plain, 0, false);
        }

        var (kind, points) = ReportScoring.Score(cards, team.HasRole(SpecialistRole.Photographer), Effect == SeasonEffect.PublishBonus);
        return new ReportPreview(kind, points, Validate(new PublishAction(cardIds)) == null);
    }

    /// <summary>The die value the current team needs to recruit the Specialist in an Expedition Row slot, or 0 for an empty slot.</summary>
    /// <param name="slot">The slot, 0 to 3.</param>
    /// <returns>The cost after the season and any Favor discount (never below 1).</returns>
    public int RecruitCost(int slot) =>
        slot >= 0 && slot < GameRules.ExpeditionRowSize && _state.ExpeditionRowArray[slot] != null
            ? RecruitCost(_state.CurrentTeam, _state.ExpeditionRowArray[slot])
            : 0;

    /// <summary>How many Tablets a Study by the current team would draw with the given die value (before the deck runs out).</summary>
    /// <param name="value">The die value or sum.</param>
    /// <returns>1 or 2.</returns>
    public int StudyDraws(int value) => StudyDraws(_state.CurrentTeam, value);

    /// <summary>True when the current team has a free survey to use (the Surveyor's, the season's, or a Favor's).</summary>
    public bool FreeSurveyAvailable => _state.CurrentTeam != null && HasFreeSurvey(_state.CurrentTeam);

    private void AddSpendActions(List<GameAction> list, TeamState team)
    {
        var s = _state;
        if (team.RerollAvailable)
        {
            if (!s.DieAUsed)
            {
                list.Add(new RerollAction(s.ChosenList[0]));
            }

            if (!s.DieBUsed)
            {
                list.Add(new RerollAction(s.ChosenList[1]));
            }
        }

        var choices = AllChoices.Where(DiceAvailable).ToArray();
        var maxWorkers = Effect == SeasonEffect.WorkersCapped ? Math.Min(team.Workers, GameRules.CappedWorkersPerDig) : team.Workers;
        var tablets = new List<string> { null };
        tablets.AddRange(team.TabletList.Select(t => t.Id));
        for (var slot = 0; slot < GameRules.SiteRowSize; slot++)
        {
            if (s.SiteRowArray[slot] == null)
            {
                continue;
            }

            foreach (var choice in choices)
            {
                foreach (var tablet in tablets)
                {
                    for (var w = 0; w <= maxWorkers; w++)
                    {
                        if (EvaluateDig(team, slot, choice, w, tablet).Preview.CanDig)
                        {
                            list.Add(new DigAction(slot, choice, w, tablet));
                        }
                    }
                }
            }
        }

        for (var slot = 0; slot < GameRules.ExpeditionRowSize; slot++)
        {
            foreach (var choice in choices)
            {
                var recruit = new RecruitAction(slot, choice);
                if (Validate(recruit) == null)
                {
                    list.Add(recruit);
                }
            }
        }

        if (TabletsLeft > 0)
        {
            list.AddRange(choices.Select(c => new StudyAction(c)));
        }

        if (s.SiteDeck.Count > 0)
        {
            var free = HasFreeSurvey(team);
            for (var slot = 0; slot < GameRules.SiteRowSize; slot++)
            {
                if (s.SiteRowArray[slot] == null)
                {
                    continue;
                }

                if (free)
                {
                    list.Add(new SurveyAction(slot, null));
                }

                list.AddRange(choices.Select(c => new SurveyAction(slot, c)));
            }
        }

        var hand = team.HandList;
        if (hand.Count >= GameRules.MinReportCards && hand.Count < 31)
        {
            var subsets = new List<int>();
            for (var mask = 1; mask < (1 << hand.Count); mask++)
            {
                if (System.Numerics.BitOperations.PopCount((uint)mask) >= GameRules.MinReportCards)
                {
                    subsets.Add(mask);
                }
            }

            foreach (var mask in subsets.OrderBy(m => System.Numerics.BitOperations.PopCount((uint)m)).ThenBy(m => m))
            {
                var ids = new List<string>();
                for (var i = 0; i < hand.Count; i++)
                {
                    if ((mask & (1 << i)) != 0)
                    {
                        ids.Add(hand[i].Id);
                    }
                }

                if (ids.Distinct(StringComparer.Ordinal).Count() == ids.Count)
                {
                    list.Add(new PublishAction(ids.ToArray()));
                }
            }
        }

        list.Add(new EndTurnAction());
    }

    private int TabletsLeft => _state.TabletDeck.Count + _state.TabletDiscard.Count;

    private string Validate(GameAction action)
    {
        if (action == null)
        {
            return "No action was given.";
        }

        var s = _state;
        if (s.IsGameOver)
        {
            return "The game is over.";
        }

        var team = s.CurrentTeam;
        switch (action)
        {
            case RollAction:
                return s.Phase == GamePhase.SeasonStart || s.Phase == GamePhase.AwaitRoll ? null : "The dice are already rolled this turn.";
            case DiscardAction discard:
                if (s.Phase != GamePhase.TurnEnd)
                {
                    return "Cards are discarded only at the hand limit, at the end of a turn.";
                }

                return team.HandList.Any(c => c.Id == discard.CardId) || team.TabletList.Any(t => t.Id == discard.CardId)
                    ? null
                    : "That card is not in the hand.";
        }

        if (s.Phase != GamePhase.Spend)
        {
            return s.Phase == GamePhase.TurnEnd
                ? "The team must discard down to the hand limit."
                : "Roll the dice first.";
        }

        switch (action)
        {
            case EndTurnAction:
                return null;
            case RerollAction reroll:
                if (!team.RerollAvailable)
                {
                    return "No re-roll is available.";
                }

                if (reroll.DieIndex == s.ChosenList[0])
                {
                    return s.DieAUsed ? "That die is already spent." : null;
                }

                if (reroll.DieIndex == s.ChosenList[1])
                {
                    return s.DieBUsed ? "That die is already spent." : null;
                }

                return "Only a kept, unspent die may be re-rolled.";
            case DigAction dig:
                {
                    var eval = EvaluateDig(team, dig.Slot, dig.Dice, dig.Workers, dig.TabletId);
                    return eval.Preview.CanDig ? null : eval.Reason;
                }

            case RecruitAction recruit:
                {
                    if (!DiceAvailable(recruit.Dice))
                    {
                        return "That die is already spent.";
                    }

                    if (recruit.Slot < 0 || recruit.Slot >= GameRules.ExpeditionRowSize || s.ExpeditionRowArray[recruit.Slot] == null)
                    {
                        return "There is no Specialist in that slot.";
                    }

                    var card = s.ExpeditionRowArray[recruit.Slot];
                    if (team.SpecialistList.Count >= GameRules.MaxSpecialists)
                    {
                        return "A team holds at most four Specialists.";
                    }

                    if (team.HasRole(card.Role))
                    {
                        return "The team already has that Specialist.";
                    }

                    return DiceValue(recruit.Dice) >= RecruitCost(team, card) ? null : "The die is lower than the Specialist's cost.";
                }

            case StudyAction study:
                if (!DiceAvailable(study.Dice))
                {
                    return "That die is already spent.";
                }

                return TabletsLeft > 0 ? null : "No Tablets are left to draw.";
            case SurveyAction survey:
                if (survey.Slot < 0 || survey.Slot >= GameRules.SiteRowSize || s.SiteRowArray[survey.Slot] == null)
                {
                    return "There is no site in that slot.";
                }

                if (s.SiteDeck.Count == 0)
                {
                    return "The Site deck is empty, so there is nothing to survey into the row.";
                }

                if (survey.Dice == null)
                {
                    return HasFreeSurvey(team) ? null : "No free survey is available.";
                }

                return DiceAvailable(survey.Dice.Value) ? null : "That die is already spent.";
            case PublishAction publish:
                {
                    if (publish.CardIds == null || publish.CardIds.Count < GameRules.MinReportCards)
                    {
                        return "A report needs three or more Discoveries.";
                    }

                    if (publish.CardIds.Distinct(StringComparer.Ordinal).Count() != publish.CardIds.Count)
                    {
                        return "A card is named twice.";
                    }

                    return publish.CardIds.All(id => team.HandList.Any(c => c.Id == id)) ? null : "A card is not an unpublished Discovery in the hand.";
                }

            default:
                return "Unknown action.";
        }
    }

    private bool DiceAvailable(DiceChoice choice)
    {
        var s = _state;
        if (s.Phase != GamePhase.Spend || s.ChosenList.Count != 2)
        {
            return false;
        }

        return choice switch
        {
            DiceChoice.DieA => !s.DieAUsed,
            DiceChoice.DieB => !s.DieBUsed,
            DiceChoice.Both => !s.DieAUsed && !s.DieBUsed,
            _ => false,
        };
    }

    private int DiceValue(DiceChoice choice) => choice switch
    {
        DiceChoice.DieA => _state.DieA,
        DiceChoice.DieB => _state.DieB,
        DiceChoice.Both => _state.DieA + _state.DieB,
        _ => 0,
    };

    /// <summary>Decision: the RecruitCheaper season and a RecruitDiscount Favor each take one off, never below 1.</summary>
    private int RecruitCost(TeamState team, SpecialistCard card)
    {
        var cost = card.Cost;
        if (Effect == SeasonEffect.RecruitCheaper)
        {
            cost--;
        }

        if (team.RecruitDiscountPending)
        {
            cost--;
        }

        return Math.Max(1, cost);
    }

    /// <summary>
    /// Decision: Study draws two when the value spent is 6 or more (a single 6, or a sum of 6 or more), with the
    /// Epigrapher, or in the StudyDrawsTwo season; never more than two.
    /// </summary>
    private int StudyDraws(TeamState team, int value) =>
        value >= GameRules.StudyDrawsTwoAt || team.HasRole(SpecialistRole.Epigrapher) || Effect == SeasonEffect.StudyDrawsTwo
            ? GameRules.MaxStudyDraw
            : 1;

    private static bool HasFreeSurvey(TeamState team) =>
        (team.HasRole(SpecialistRole.Surveyor) && !team.SurveyorUsedThisTurn) || team.SeasonFreeSurvey || team.FavorFreeSurveys > 0;

    private DigEvaluation EvaluateDig(TeamState team, int slot, DiceChoice dice, int workers, string tabletId)
    {
        var s = _state;
        var modifiers = new List<string>();
        string reason = null;
        var card = slot >= 0 && slot < GameRules.SiteRowSize ? s.SiteRowArray[slot] : null;
        if (card == null)
        {
            reason = "There is no site in that slot.";
        }

        var total = 0;
        if (!Enum.IsDefined(dice))
        {
            reason ??= "Unknown dice choice.";
        }
        else if (!DiceAvailable(dice))
        {
            reason ??= s.Phase == GamePhase.Spend ? "That die is already spent." : "Roll the dice first.";
        }
        else
        {
            total = DiceValue(dice);
        }

        var capped = Effect == SeasonEffect.WorkersCapped;
        if (workers < 0)
        {
            reason ??= "Workers cannot be negative.";
        }
        else if (workers > team.Workers)
        {
            reason ??= "The team does not have that many Workers.";
        }
        else if (capped && workers > GameRules.CappedWorkersPerDig)
        {
            reason ??= "At most two Workers may be used on one dig this season.";
        }
        else if (workers > 0)
        {
            total += workers;
            modifiers.Add(string.Format(CultureInfo.InvariantCulture, "{0} Worker{1} +{0}{2}", workers, workers == 1 ? string.Empty : "s", capped ? " (free this season)" : string.Empty));
        }

        TabletCard tablet = null;
        if (tabletId != null)
        {
            tablet = team.TabletList.FirstOrDefault(t => t.Id == tabletId);
            if (tablet == null)
            {
                reason ??= "That Tablet is not in the hand.";
            }
            else
            {
                total += TabletCard.DigBonus;
                modifiers.Add("Tablet +2");
            }
        }

        if (card == null)
        {
            return new DigEvaluation(new DigPreview(0, total, false, modifiers), reason, null, false, false);
        }

        var needed = card.DigNumber;
        var shift = GameRules.DifficultyShift(s.Setup.Difficulty);
        if (shift != 0)
        {
            needed += shift;
            modifiers.Add(string.Format(CultureInfo.InvariantCulture, "Dig Number {0:+0;-0} ({1})", shift, s.Setup.Difficulty));
        }

        if ((Effect == SeasonEffect.KassiteCheaper && card.Tier == 3) || (Effect == SeasonEffect.DeepCheaper && card.Tier == DepthTiers.Deepest))
        {
            needed -= 1;
            modifiers.Add("Dig Number -1 (season)");
        }

        if (team.HasRole(SpecialistRole.Architect) && card.Kind == DiscoveryKind.Building)
        {
            total += 1;
            modifiers.Add("Architect +1");
        }

        if (team.HasRole(SpecialistRole.Epigrapher) && card.Kind == DiscoveryKind.Inscription)
        {
            total += 1;
            modifiers.Add("Epigrapher +1");
        }

        if (team.HasRole(SpecialistRole.SmallFindsKeeper) && (card.Kind == DiscoveryKind.Object || card.Kind == DiscoveryKind.Deposit))
        {
            total += 1;
            modifiers.Add("Small-Finds Keeper +1");
        }

        var plusTwo = team.PlusTwoPending;
        if (plusTwo)
        {
            total += GameRules.FavorDigBonus;
            modifiers.Add("Favor +2");
        }

        var foreman = false;
        if (total < needed && team.HasRole(SpecialistRole.Foreman) && !team.ForemanUsedThisTurn)
        {
            total += 1;
            foreman = true;
            modifiers.Add("Foreman +1");
        }

        var canDig = reason == null && total >= needed;
        if (reason == null && !canDig)
        {
            reason = "The total does not reach the Dig Number.";
        }

        return new DigEvaluation(new DigPreview(needed, total, canDig, modifiers), reason, tablet, foreman, plusTwo);
    }

    private sealed record DigEvaluation(DigPreview Preview, string Reason, TabletCard Tablet, bool ForemanUsed, bool PlusTwoUsed);
}
