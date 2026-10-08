using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Rules.Engine;

public sealed partial class GameEngine
{
    /// <summary>
    /// A stable 64-bit FNV-1a hash (16 hex digits) of the whole state: decks in order, rows, dice, every team, the
    /// journal and the generator. Identical for identical seeds and actions; it changes with every applied action.
    /// </summary>
    /// <returns>The hash.</returns>
    public string StateHash()
    {
        var hash = 14695981039346656037UL;
        foreach (var ch in CanonicalState())
        {
            hash ^= ch;
            hash *= 1099511628211UL;
        }

        return hash.ToString("x16", CultureInfo.InvariantCulture);
    }

    internal string CanonicalState()
    {
        var s = _state;
        var sb = new StringBuilder();
        Field(sb, "seed", s.Seed);
        sb.Append("rng=").Append(_random.State.ToString(CultureInfo.InvariantCulture)).Append('|');
        Field(sb, "phase", (int)s.Phase);
        Field(sb, "season", s.SeasonIndex);
        Field(sb, "round", s.TurnInSeason);
        Field(sb, "pos", s.TurnOrderPosition);
        Field(sb, "actions", s.ActionCount);
        List(sb, "dice", s.DiceList.Select(d => d.ToString(CultureInfo.InvariantCulture)));
        List(sb, "kept", s.ChosenList.Select(d => d.ToString(CultureInfo.InvariantCulture)));
        Field(sb, "used", (s.DieAUsed ? 1 : 0) + (s.DieBUsed ? 2 : 0) + (s.FavorDrawnThisTurn ? 4 : 0));
        foreach (var t in s.TeamList)
        {
            sb.Append("team:").Append(t.Name).Append('|');
            Field(sb, "w", t.Workers);
            Field(sb, "pp", t.PublishedPoints);
            Field(sb, "flags", (t.RerollAvailable ? 1 : 0) + (t.ExtraDieNextTurn ? 2 : 0) + (t.RecruitDiscountPending ? 4 : 0)
                + (t.NoHandLimitThisTurn ? 8 : 0) + (t.ForemanUsedThisTurn ? 16 : 0) + (t.SurveyorUsedThisTurn ? 32 : 0)
                + (t.SeasonFreeSurvey ? 64 : 0));
            Field(sb, "fs", t.FavorFreeSurveys);
            Field(sb, "p2", t.PlusTwoTurnsLeft);
            List(sb, "hand", t.HandList.Select(c => c.Id));
            List(sb, "tab", t.TabletList.Select(c => c.Id));
            List(sb, "spec", t.SpecialistList.Select(Key));
            foreach (var r in t.ReportList)
            {
                List(sb, "rep" + r.Number.ToString(CultureInfo.InvariantCulture) + ":" + r.Kind + ":" + r.Points.ToString(CultureInfo.InvariantCulture) + ":" + r.SeasonYear, r.Cards.Select(c => c.Id));
            }
        }

        List(sb, "row", s.SiteRowArray.Select(c => c?.Id ?? "-"));
        List(sb, "site", s.SiteDeck.Select(c => c.Id));
        List(sb, "exprow", s.ExpeditionRowArray.Select(c => c == null ? "-" : Key(c)));
        List(sb, "exp", s.ExpeditionDeck.Select(Key));
        List(sb, "tdeck", s.TabletDeck.Select(c => c.Id));
        List(sb, "tdisc", s.TabletDiscard.Select(c => c.Id));
        List(sb, "fdeck", s.FavorDeck.Select(c => c.Id));
        List(sb, "fdisc", s.FavorDiscard.Select(c => c.Id));
        List(sb, "gone", s.DiscardedDiscoveries.Select(c => c.Id));
        Field(sb, "journal", s.JournalList.Count);
        return sb.ToString();
    }

    private static string Key(SpecialistCard card) =>
        card.Role + ":" + card.Title + ":" + card.Cost.ToString(CultureInfo.InvariantCulture);

    private static void Field(StringBuilder sb, string name, int value) =>
        sb.Append(name).Append('=').Append(value.ToString(CultureInfo.InvariantCulture)).Append('|');

    private static void List(StringBuilder sb, string name, IEnumerable<string> values) =>
        sb.Append(name).Append('[').Append(string.Join(",", values)).Append("]|");
}
