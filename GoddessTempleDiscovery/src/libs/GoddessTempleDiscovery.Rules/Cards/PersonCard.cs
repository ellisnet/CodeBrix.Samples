namespace GoddessTempleDiscovery.Rules.Cards;

/// <summary>A real person of the excavations, for the history screens and the Field Journal. The players never play these people.</summary>
/// <param name="Id">A stable kebab-case identifier.</param>
/// <param name="Name">The name, with diacritics.</param>
/// <param name="Dates">Birth and death years where known.</param>
/// <param name="Role">The role at Uruk and the seasons present.</param>
/// <param name="CardText">Two to four plain sentences.</param>
/// <param name="LongText">The fuller account.</param>
/// <param name="EthicsNote">What the record documents about Nazi-era ties, plainly and briefly, or empty.</param>
/// <param name="ArtKey">The key of the vector art (a scene, never a portrait).</param>
/// <param name="Sources">The sources the texts rest on.</param>
public sealed record PersonCard(
    string Id,
    string Name,
    string Dates,
    string Role,
    string CardText,
    string LongText,
    string EthicsNote,
    string ArtKey,
    string Sources);
