using System.Collections.Generic;

namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>The options of one game (DESIGN.md section 3).</summary>
/// <param name="Seats">Two to four seats, in the order the setup screen lists them; the engine shuffles the seat order.</param>
/// <param name="TurnsPerSeason">Turns per team per season, 1 to 3.</param>
/// <param name="Difficulty">Shifts every Dig Number by -1, 0 or +1.</param>
/// <param name="Seed">A seed for a repeatable game, or null for a fresh one.</param>
public sealed record GameSetup(
    IReadOnlyList<SeatSetup> Seats,
    int TurnsPerSeason = 2,
    Difficulty Difficulty = Difficulty.Standard,
    int? Seed = null);
