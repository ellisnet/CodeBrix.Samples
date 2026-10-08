namespace GoddessTempleDiscovery.Rules.Cards;

/// <summary>A short quotation for a loading line or a card flavor line.</summary>
/// <param name="Text">The words, at most about thirty-five.</param>
/// <param name="Attribution">Who, in what work, when.</param>
/// <param name="IsPrimary">True for an ancient author's or an excavator's own words; false for a modern scholar.</param>
/// <param name="Sources">The library file or publication.</param>
public sealed record Quotation(string Text, string Attribution, bool IsPrimary, string Sources);
