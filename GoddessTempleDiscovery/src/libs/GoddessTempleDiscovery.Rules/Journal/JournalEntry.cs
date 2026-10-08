namespace GoddessTempleDiscovery.Rules.Journal;

/// <summary>One entry of the Field Journal: everything read this game, in order, printable at the end.</summary>
/// <param name="Kind">What the entry records.</param>
/// <param name="Title">The heading.</param>
/// <param name="Text">The full text, paragraphs separated by blank lines.</param>
/// <param name="ArtKey">The key of the card's art (may be empty).</param>
/// <param name="SeasonYear">The season it was read in, such as "1930/31".</param>
/// <param name="TeamName">The team that turned it up, or null for the whole table.</param>
/// <param name="Sources">The sources the texts rest on (may be empty).</param>
public sealed record JournalEntry(
    JournalEntryKind Kind,
    string Title,
    string Text,
    string ArtKey,
    string SeasonYear,
    string TeamName,
    string Sources);
