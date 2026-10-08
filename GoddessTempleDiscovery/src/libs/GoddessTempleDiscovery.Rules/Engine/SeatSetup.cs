namespace GoddessTempleDiscovery.Rules.Engine;

/// <summary>One seat at the table, as the setup screen fills it.</summary>
/// <param name="TeamName">The name typed or chosen; when empty the team profile's name is used.</param>
/// <param name="TeamProfileId">The id of a <see cref="GoddessTempleDiscovery.Rules.Cards.TeamProfile"/> in the catalog, or null.</param>
/// <param name="Kind">Human or computer.</param>
/// <param name="Temperament">The computer temperament (kept but unused for a human seat).</param>
public sealed record SeatSetup(string TeamName, string TeamProfileId, SeatKind Kind, Temperament Temperament);
