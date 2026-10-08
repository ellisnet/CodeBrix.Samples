using System.Collections.Generic;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Scoring;

namespace GoddessTempleDiscovery.Game.Bridges;

/// <summary>
/// What the host tells the page's chrome about the game (the view model implements it). The host calls these on the
/// UI thread through the dispatcher it was given.
/// </summary>
public interface ISessionBridge
{
    /// <summary>A season began.</summary>
    /// <param name="season">The season card.</param>
    /// <param name="index">Its index, from 0.</param>
    /// <param name="count">The number of seasons.</param>
    void SeasonChanged(SeasonCard season, int index, int count);

    /// <summary>The teams' standing changed.</summary>
    /// <param name="teams">The teams, in seat order.</param>
    void TeamsChanged(IReadOnlyList<TeamView> teams);

    /// <summary>The prompt or the wire-service ticker line changed.</summary>
    /// <param name="prompt">The line.</param>
    void PromptChanged(string prompt);

    /// <summary>The game ended.</summary>
    /// <param name="scores">The final scores, in seat order.</param>
    void GameEnded(FinalScore[] scores);

    /// <summary>The Field Journal grew.</summary>
    /// <param name="entries">Every entry so far, in order.</param>
    void JournalChanged(IReadOnlyList<JournalEntry> entries);

    /// <summary>A HUD button asked for a XAML pane: "journal" or "settings".</summary>
    /// <param name="pane">The pane's name.</param>
    void PaneRequested(string pane);
}
