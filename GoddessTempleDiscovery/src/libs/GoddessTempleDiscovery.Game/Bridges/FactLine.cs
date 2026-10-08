namespace GoddessTempleDiscovery.Game.Bridges;

/// <summary>One labelled fact of a card, such as "Dig Number" and "9".</summary>
/// <param name="Label">The label.</param>
/// <param name="Value">The value.</param>
public sealed record FactLine(string Label, string Value);
