using System;
using System.Globalization;
using System.Linq;
using GoddessTempleDiscovery.Game.Cards;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Game.Session;

/// <summary>
/// Words every applied action as a wire-service line for the HUD ticker and the log:
/// "WARKA — The Lapis Road Society digs the Kassite layer with a 6 and two Workers".
/// </summary>
public static class Narrator
{
    /// <summary>The ticker's dateline prefix.</summary>
    public const string Dateline = "WARKA — ";

    private static readonly string[] Numbers = { "no", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };

    /// <summary>Describes an action that has just been applied.</summary>
    /// <param name="engine">The game, after the action.</param>
    /// <param name="teamName">The team that acted.</param>
    /// <param name="result">What the engine returned.</param>
    /// <returns>The line, without the dateline.</returns>
    public static string Describe(GameEngine engine, string teamName, ApplyResult result)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(result);
        var who = teamName ?? "A team";
        var state = engine.State;
        switch (result.Action)
        {
            case RollAction:
                var rolled = result.Events.OfType<DiceRolled>().FirstOrDefault();
                return rolled == null
                    ? who + " rolls"
                    : who + " rolls " + string.Join(" and ", rolled.Values.Select(v => v.ToString(CultureInfo.InvariantCulture)))
                      + (rolled.Values.Count > 2 ? " (the Goddess's extra die; the best two are kept)" : string.Empty);
            case RerollAction:
                var rerolled = result.Events.OfType<DieRerolled>().FirstOrDefault();
                return who + " re-rolls a die" + (rerolled == null ? string.Empty : " and gets a " + N(rerolled.Value));
            case DigAction dig:
                var excavated = result.Events.OfType<SiteExcavated>().FirstOrDefault();
                var layer = excavated == null ? "a site" : "the " + CardText.PeriodName(excavated.Card.Period) + " layer";
                var line = who + " digs " + layer + " with " + DiceWords(state, dig.Dice);
                if (dig.Workers > 0)
                {
                    line += " and " + Count(dig.Workers, "Worker");
                }

                if (dig.TabletId != null)
                {
                    line += " and a Tablet";
                }

                if (excavated != null)
                {
                    line += ": " + CardText.ShortTitle(excavated.Card.Title) + (excavated.Card.IsStarred ? ", one of Her stars" : string.Empty);
                }

                return line + Favor(result);
            case RecruitAction:
                var recruited = result.Events.OfType<SpecialistRecruited>().FirstOrDefault();
                return who + " recruits " + (recruited == null ? "a Specialist" : "the " + recruited.Card.Title) + Favor(result);
            case StudyAction:
                var tablets = result.Events.OfType<TabletDrawn>().Count(t => t.TeamName == teamName);
                return who + " studies and draws " + Count(tablets, "Tablet") + Favor(result);
            case SurveyAction survey:
                return who + " surveys trench " + N(survey.Slot + 1) + (survey.Dice == null ? ", free" : string.Empty) + Favor(result);
            case PublishAction:
                var report = result.Events.OfType<ReportPublished>().FirstOrDefault()?.Report;
                return report == null
                    ? who + " publishes"
                    : who + " publishes its " + report.Title + " (" + report.Kind + ", " + N(report.Points) + " points)";
            case DiscardAction:
                return who + " sets a card aside at the hand limit";
            case EndTurnAction:
                return who + " ends its turn" + Favor(result);
            default:
                return who + " acts";
        }
    }

    private static string Favor(ApplyResult result)
    {
        var favor = result.Events.OfType<FavorDrawn>().FirstOrDefault();
        return favor == null ? string.Empty : "; the Goddess favors it: " + favor.Card.Title;
    }

    private static string DiceWords(GameState state, DiceChoice choice) => choice switch
    {
        DiceChoice.DieA => "a " + N(state.DieA),
        DiceChoice.DieB => "a " + N(state.DieB),
        _ => "a " + N(state.DieA) + " and a " + N(state.DieB),
    };

    private static string Count(int count, string noun) =>
        (count >= 0 && count < Numbers.Length ? Numbers[count] : N(count)) + " " + noun + (count == 1 ? string.Empty : "s");

    private static string N(int value) => value.ToString(CultureInfo.InvariantCulture);
}
