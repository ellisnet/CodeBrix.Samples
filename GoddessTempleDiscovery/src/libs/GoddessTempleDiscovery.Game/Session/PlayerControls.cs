using System;
using System.Collections.Generic;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Engine;

namespace GoddessTempleDiscovery.Game.Session;

/// <summary>What the human player is choosing on the table this turn.</summary>
public enum ControlMode
{
    /// <summary>Spending dice: click a die, then a site or a specialist.</summary>
    Normal,
    /// <summary>Survey: the next click on a site surveys it.</summary>
    Survey,
    /// <summary>Publish: clicks in the hand pick the Discoveries of a report.</summary>
    Publish,
}

/// <summary>
/// The human player's choices in progress: the dice selected, the Workers and the Tablet to add to a dig, the
/// survey and publish modes. It turns a choice into an engine action and asks the engine whether it is legal; it
/// never decides a rule itself.
/// </summary>
public sealed class PlayerControls
{
    private readonly HashSet<int> _selectedDice = new HashSet<int>();
    private readonly List<string> _publish = new List<string>();

    /// <summary>The mode.</summary>
    public ControlMode Mode { get; set; }

    /// <summary>The engine indices (into <see cref="GameState.Dice"/>) of the selected dice.</summary>
    public IReadOnlyCollection<int> SelectedDice => _selectedDice;

    /// <summary>The Workers to add to the next dig.</summary>
    public int Workers { get; set; }

    /// <summary>The id of the Tablet to spend on the next dig, or null.</summary>
    public string TabletId { get; set; }

    /// <summary>The ids of the Discoveries picked for a report, in the order picked.</summary>
    public IReadOnlyList<string> PublishSelection => _publish;

    /// <summary>The Site Row slot under the pointer, or -1.</summary>
    public int HoverSlot { get; set; } = -1;

    /// <summary>Clears every choice (a new turn).</summary>
    public void Reset()
    {
        _selectedDice.Clear();
        _publish.Clear();
        Workers = 0;
        TabletId = null;
        Mode = ControlMode.Normal;
        HoverSlot = -1;
    }

    /// <summary>Selects or deselects a kept, unspent die.</summary>
    /// <param name="engine">The game.</param>
    /// <param name="dieIndex">The die's index in <see cref="GameState.Dice"/>.</param>
    /// <returns>True when the selection changed.</returns>
    public bool ToggleDie(GameEngine engine, int dieIndex)
    {
        ArgumentNullException.ThrowIfNull(engine);
        if (ChoiceOf(engine, dieIndex) == null)
        {
            return false;
        }

        if (!_selectedDice.Remove(dieIndex))
        {
            _selectedDice.Add(dieIndex);
        }

        return true;
    }

    /// <summary>Drops dice from the selection that the engine has spent.</summary>
    /// <param name="engine">The game.</param>
    public void Prune(GameEngine engine)
    {
        _selectedDice.RemoveWhere(i => ChoiceOf(engine, i) == null);
        var team = engine.State.CurrentTeam;
        if (TabletId != null && (team == null || team.Tablets.All(t => t.Id != TabletId)))
        {
            TabletId = null;
        }

        if (team != null)
        {
            Workers = Math.Clamp(Workers, 0, team.Workers);
            _publish.RemoveAll(id => team.Hand.All(c => c.Id != id));
        }
    }

    /// <summary>The dice choice of the current selection: Die A, Die B, both, or null for none.</summary>
    /// <param name="engine">The game.</param>
    /// <returns>The choice.</returns>
    public DiceChoice? Choice(GameEngine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);
        var choices = _selectedDice.Select(i => ChoiceOf(engine, i)).Where(c => c != null).Select(c => c.Value).Distinct().ToArray();
        return choices.Length switch
        {
            1 => choices[0],
            2 => DiceChoice.Both,
            _ => null,
        };
    }

    /// <summary>The value the selection spends (a die, or the sum), or 0.</summary>
    /// <param name="engine">The game.</param>
    /// <returns>The value.</returns>
    public int ChoiceValue(GameEngine engine)
    {
        var state = engine.State;
        return Choice(engine) switch
        {
            DiceChoice.DieA => state.DieA,
            DiceChoice.DieB => state.DieB,
            DiceChoice.Both => state.DieA + state.DieB,
            _ => 0,
        };
    }

    /// <summary>The kept, unspent die an engine die index stands for, or null.</summary>
    /// <param name="engine">The game.</param>
    /// <param name="dieIndex">The index in <see cref="GameState.Dice"/>.</param>
    /// <returns>Die A, Die B, or null.</returns>
    public static DiceChoice? ChoiceOf(GameEngine engine, int dieIndex)
    {
        var state = engine.State;
        if (state.Phase != GamePhase.Spend || state.ChosenDice.Count != 2)
        {
            return null;
        }

        if (state.ChosenDice[0] == dieIndex && !state.DieAUsed)
        {
            return DiceChoice.DieA;
        }

        if (state.ChosenDice[1] == dieIndex && !state.DieBUsed)
        {
            return DiceChoice.DieB;
        }

        return null;
    }

    /// <summary>Toggles a Discovery in the report being picked.</summary>
    /// <param name="id">The discovery's id.</param>
    public void TogglePublish(string id)
    {
        if (!_publish.Remove(id))
        {
            _publish.Add(id);
        }
    }

    /// <summary>
    /// The dig of a slot: the player's own choice (dice, Workers, Tablet) when it is legal, otherwise the legal dig of
    /// that slot that spends least (the selected dice when some are selected; no Tablet before a Tablet; fewer Workers;
    /// one die before both; the lower die).
    /// </summary>
    /// <param name="engine">The game.</param>
    /// <param name="slot">The Site Row slot.</param>
    /// <returns>A legal dig, or null when none reaches the site.</returns>
    public DigAction BestDig(GameEngine engine, int slot)
    {
        ArgumentNullException.ThrowIfNull(engine);
        var choice = Choice(engine);
        if (choice != null)
        {
            var mine = new DigAction(slot, choice.Value, Workers, TabletId);
            if (engine.IsLegal(mine))
            {
                return mine;
            }
        }

        var state = engine.State;
        return engine.LegalActions()
            .OfType<DigAction>()
            .Where(d => d.Slot == slot && (choice == null || d.Dice == choice))
            .OrderBy(d => d.TabletId == null ? 0 : 1)
            .ThenBy(d => d.Workers)
            .ThenBy(d => d.Dice == DiceChoice.Both ? 1 : 0)
            .ThenBy(d => d.Dice == DiceChoice.DieA ? state.DieA : d.Dice == DiceChoice.DieB ? state.DieB : state.DieA + state.DieB)
            .FirstOrDefault();
    }

    /// <summary>The recruit of a slot with the selected dice, or the cheapest legal one.</summary>
    /// <param name="engine">The game.</param>
    /// <param name="slot">The Expedition Row slot.</param>
    /// <returns>A legal recruit, or null.</returns>
    public RecruitAction BestRecruit(GameEngine engine, int slot)
    {
        var choice = Choice(engine);
        var state = engine.State;
        return engine.LegalActions()
            .OfType<RecruitAction>()
            .Where(r => r.Slot == slot && (choice == null || r.Dice == choice))
            .OrderBy(r => r.Dice == DiceChoice.Both ? 1 : 0)
            .ThenBy(r => r.Dice == DiceChoice.DieA ? state.DieA : state.DieB)
            .FirstOrDefault();
    }

    /// <summary>A study with the selected die, or with the die that draws most for the least.</summary>
    /// <param name="engine">The game.</param>
    /// <returns>A legal study, or null.</returns>
    public StudyAction BestStudy(GameEngine engine)
    {
        var choice = Choice(engine);
        var state = engine.State;
        return engine.LegalActions()
            .OfType<StudyAction>()
            .Where(s => choice == null ? s.Dice != DiceChoice.Both : s.Dice == choice)
            .OrderBy(s => s.Dice == DiceChoice.DieA ? state.DieA : state.DieB)
            .FirstOrDefault();
    }

    /// <summary>A survey of a slot: free when a free survey is open, otherwise with the selected or the lower die.</summary>
    /// <param name="engine">The game.</param>
    /// <param name="slot">The Site Row slot.</param>
    /// <returns>A legal survey, or null.</returns>
    public SurveyAction BestSurvey(GameEngine engine, int slot)
    {
        var choice = Choice(engine);
        var state = engine.State;
        var surveys = engine.LegalActions().OfType<SurveyAction>().Where(s => s.Slot == slot).ToArray();
        return surveys.FirstOrDefault(s => s.Dice == null)
               ?? surveys.Where(s => choice == null ? s.Dice != DiceChoice.Both : s.Dice == choice)
                   .OrderBy(s => s.Dice == DiceChoice.DieA ? state.DieA : state.DieB)
                   .FirstOrDefault();
    }

    /// <summary>A re-roll of the selected die, or of the lower unspent kept die.</summary>
    /// <param name="engine">The game.</param>
    /// <returns>A legal re-roll, or null.</returns>
    public RerollAction BestReroll(GameEngine engine)
    {
        var state = engine.State;
        var rerolls = engine.LegalActions().OfType<RerollAction>().ToArray();
        return rerolls.FirstOrDefault(r => _selectedDice.Contains(r.DieIndex))
               ?? rerolls.OrderBy(r => state.Dice[r.DieIndex]).FirstOrDefault();
    }
}
