using System;
using System.Collections.Generic;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Rules.Brains;

/// <summary>
/// The computer teams' decision procedure (DESIGN.md section 9): one procedure, three weightings. Each call returns
/// one action taken from <see cref="GameEngine.LegalActions"/>, so it is always legal.
/// </summary>
public static class ComputerBrain
{
    /// <summary>The largest random jitter added to a candidate's score so games differ.</summary>
    public const double Jitter = 0.25;

    private const double SingleDieCost = 0.15;
    private const double BothDiceCost = 0.8;

    /// <summary>Chooses the current team's next action.</summary>
    /// <remarks>
    /// Rolls when the dice wait; at the hand limit discards the card it values least; in the Spend phase scores every
    /// legal action (points now plus the temperament's weights, minus what it spends, plus up to <see cref="Jitter"/>)
    /// and ends the turn when no candidate scores above zero.
    /// </remarks>
    /// <param name="engine">The game.</param>
    /// <param name="temperament">The weighting to use.</param>
    /// <param name="random">The source of the jitter.</param>
    /// <returns>A legal action.</returns>
    /// <exception cref="InvalidOperationException">When the game is over and nothing is legal.</exception>
    public static GameAction Choose(GameEngine engine, Temperament temperament, Random random)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(random);
        var legal = engine.LegalActions();
        if (legal.Count == 0)
        {
            throw new InvalidOperationException("No action is legal: the game is over.");
        }

        var state = engine.State;
        var weights = TemperamentWeights.For(temperament);
        switch (state.Phase)
        {
            case GamePhase.SeasonStart:
            case GamePhase.AwaitRoll:
                return legal.FirstOrDefault(a => a is RollAction) ?? legal[0];
            case GamePhase.TurnEnd:
                return legal
                    .Select(a => (Action: a, Score: KeepValue(state.CurrentTeam, weights, ((DiscardAction)a).CardId) + (random.NextDouble() * 0.1)))
                    .OrderBy(x => x.Score)
                    .First()
                    .Action;
        }

        GameAction best = null;
        var bestScore = 0.0;
        foreach (var action in legal)
        {
            if (action is EndTurnAction)
            {
                continue;
            }

            var score = Score(engine, weights, action) + (random.NextDouble() * Jitter);
            if (score > bestScore)
            {
                best = action;
                bestScore = score;
            }
        }

        return best ?? legal.FirstOrDefault(a => a is EndTurnAction) ?? legal[0];
    }

    /// <summary>Chooses the current team's next action with the team's own temperament.</summary>
    /// <param name="engine">The game.</param>
    /// <param name="random">The source of the jitter.</param>
    /// <returns>A legal action.</returns>
    public static GameAction Choose(GameEngine engine, Random random)
    {
        ArgumentNullException.ThrowIfNull(engine);
        return Choose(engine, engine.State.CurrentTeam.Temperament, random);
    }

    /// <summary>The score of one Spend-phase action without jitter (0 is the score of ending the turn).</summary>
    /// <param name="engine">The game.</param>
    /// <param name="temperament">The weighting.</param>
    /// <param name="action">A legal action.</param>
    /// <returns>The score.</returns>
    public static double Score(GameEngine engine, Temperament temperament, GameAction action)
    {
        ArgumentNullException.ThrowIfNull(engine);
        return Score(engine, TemperamentWeights.For(temperament), action);
    }

    private static double Score(GameEngine engine, TemperamentWeights w, GameAction action)
    {
        var state = engine.State;
        return action switch
        {
            RerollAction r => state.Dice[r.DieIndex] <= 2 ? (3.5 - state.Dice[r.DieIndex]) * 0.5 : -1.0,
            DigAction d => ScoreDig(engine, w, d),
            RecruitAction r => ScoreRecruit(engine, w, r),
            StudyAction s => ScoreStudy(engine, w, s),
            SurveyAction s => ScoreSurvey(engine, w, s),
            PublishAction p => ScorePublish(engine, w, p),
            _ => -1.0,
        };
    }

    private static double ScoreDig(GameEngine engine, TemperamentWeights w, DigAction dig)
    {
        var state = engine.State;
        var team = state.CurrentTeam;
        var card = state.SiteRow[dig.Slot];
        var preview = engine.PreviewDig(dig.Slot, dig.Dice, dig.Workers, dig.TabletId);
        var deep = card.Tier >= 7;
        var workerCost = state.Season.Effect == SeasonEffect.WorkersCapped ? 0.05 : deep ? w.WorkerDeep : w.WorkerShallow;

        // Workers score nothing at the end, so they grow cheaper as the game runs on and as they pile up.
        workerCost *= Math.Max(0.1, 1.0 - Progress(state)) * 4.0 / (4.0 + team.Workers);
        var cost = (dig.Workers * workerCost)
            + (dig.TabletId != null ? w.TabletCost : 0.0)
            + DiceCost(dig.Dice)
            + (0.12 * Math.Max(0, preview.Total - preview.Needed));
        var pressure = team.HandCount >= GameRules.HandLimit ? 0.4 : 0.0;
        return Desirability(team, w, card) - cost - pressure;
    }

    private static double ScoreRecruit(GameEngine engine, TemperamentWeights w, RecruitAction recruit)
    {
        var state = engine.State;
        var card = state.ExpeditionRow[recruit.Slot];
        var value = w.Role(card.Role) * (1.0 - (0.8 * Progress(state)));
        var waste = DieValue(state, recruit.Dice) - engine.RecruitCost(recruit.Slot);
        return value - DiceCost(recruit.Dice) - (0.08 * waste);
    }

    private static double ScoreStudy(GameEngine engine, TemperamentWeights w, StudyAction study)
    {
        var state = engine.State;
        var team = state.CurrentTeam;
        var value = DieValue(state, study.Dice);
        var draws = Math.Min(engine.StudyDraws(value), state.TabletDeckCount + state.TabletDiscardCount);
        var pressure = team.HandCount + draws > GameRules.HandLimit ? 0.6 : 0.0;
        // A drawn Tablet is valued below its two end-game points: it may be discarded to the hand limit or spent, and its set
        //  may never complete. The temperament's taste for study is added on top.
        return (draws * (1.1 + w.Study)) - DiceCost(study.Dice) - (0.1 * value) - pressure;
    }

    private static double ScoreSurvey(GameEngine engine, TemperamentWeights w, SurveyAction survey)
    {
        var state = engine.State;
        var team = state.CurrentTeam;
        var card = state.SiteRow[survey.Slot];
        var reach = 12 + team.Workers + (2 * team.Tablets.Count) + 2;
        var unreachable = card.DigNumber + GameRules.DifficultyShift(state.Setup.Difficulty) > reach;
        var desirability = Desirability(team, w, card);
        if (survey.Dice == null)
        {
            return (unreachable ? 0.5 : 0.25) - (0.1 * desirability);
        }

        return 0.15 + (unreachable ? 0.3 : 0.0) - DiceCost(survey.Dice.Value) - (0.05 * DieValue(state, survey.Dice.Value)) - (0.1 * desirability);
    }

    private static double ScorePublish(GameEngine engine, TemperamentWeights w, PublishAction publish)
    {
        var state = engine.State;
        var team = state.CurrentTeam;
        var preview = engine.PreviewReport(publish.CardIds);
        var printed = publish.CardIds.Sum(id => team.Hand.First(c => c.Id == id).Points);
        var gain = preview.Points - (printed / 2.0);
        var lastChance = state.SeasonIndex == state.SeasonCount - 1 && state.TurnInSeason == state.Setup.TurnsPerSeason - 1;
        var wait = lastChance ? 0.0 : preview.Kind == ReportKind.Plain ? 2.0 : 0.6;
        var kind = preview.Kind switch
        {
            ReportKind.Stratigraphy => w.PublishStratigraphy,
            ReportKind.Sequence => w.PublishSequence,
            _ => 0.0,
        };
        var pressure = Math.Max(0, team.HandCount - GameRules.HandLimit) * 0.8;
        return (gain * 0.5) + kind + pressure - wait + (0.05 * publish.CardIds.Count);
    }

    private static double Desirability(TeamState team, TemperamentWeights w, DiscoveryCard card)
    {
        var value = (double)card.Points;
        if (card.IsStarred)
        {
            value += w.Star;
        }

        if (card.Tier >= 7)
        {
            value += w.DeepTier;
        }

        if (card.Tier <= 4)
        {
            value += w.ShallowTier;
        }

        if (team.Hand.Any(c => c.Period == card.Period))
        {
            value += w.SamePeriod;
        }

        if (team.Hand.Any(c => Math.Abs((int)c.Period - (int)card.Period) == 1))
        {
            value += w.AdjacentPeriod;
        }

        return value;
    }

    private static double KeepValue(TeamState team, TemperamentWeights w, string cardId)
    {
        var tablet = team.Tablets.FirstOrDefault(t => t.Id == cardId);
        if (tablet != null)
        {
            var onlyOfKind = team.Tablets.Count(t => t.Kind == tablet.Kind) == 1;
            return 1.0 + (onlyOfKind ? 0.8 : 0.0) + (w.TabletCost * 0.2);
        }

        var card = team.Hand.First(c => c.Id == cardId);
        var samePeriod = team.Hand.Count(c => c.Period == card.Period) > 1;
        return (card.Points * 0.75) + (card.IsStarred ? w.Star : 0.0) + (samePeriod ? w.SamePeriod : 0.0);
    }

    private static double Progress(GameState state) =>
        state.SeasonCount == 0 ? 1.0 : (state.SeasonIndex + ((double)state.TurnInSeason / state.Setup.TurnsPerSeason)) / state.SeasonCount;

    private static double DiceCost(DiceChoice choice) => choice == DiceChoice.Both ? BothDiceCost : SingleDieCost;

    private static int DieValue(GameState state, DiceChoice choice) => choice switch
    {
        DiceChoice.DieA => state.DieA,
        DiceChoice.DieB => state.DieB,
        _ => state.DieA + state.DieB,
    };
}
