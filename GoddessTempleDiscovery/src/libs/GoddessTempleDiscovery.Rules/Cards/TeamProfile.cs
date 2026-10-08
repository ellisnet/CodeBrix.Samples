namespace GoddessTempleDiscovery.Rules.Cards;

/// <summary>A fictional expedition team a player may choose (or a computer takes).</summary>
/// <param name="Id">A stable kebab-case identifier.</param>
/// <param name="Name">The team's name.</param>
/// <param name="ArtKey">The key of the team's emblem.</param>
/// <param name="Colour">The team's colour as a CSS hex value.</param>
/// <param name="Motto">A short motto.</param>
public sealed record TeamProfile(string Id, string Name, string ArtKey, string Colour, string Motto);
