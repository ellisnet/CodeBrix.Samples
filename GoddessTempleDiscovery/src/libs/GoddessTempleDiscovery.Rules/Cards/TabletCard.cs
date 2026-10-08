namespace GoddessTempleDiscovery.Rules.Cards;

/// <summary>The four kinds of knowledge a Tablet card teaches.</summary>
public enum TabletKind
{
    /// <summary>Holy Inanna Herself.</summary>
    Goddess,
    /// <summary>The Sumerian pantheon and the heroes of Uruk.</summary>
    Pantheon,
    /// <summary>Sumerian culture, craft and daily life.</summary>
    Culture,
    /// <summary>The timeline of the city and the texts of Sumer.</summary>
    Timeline,
}

/// <summary>
/// One Tablet card: a piece of knowledge worth one point, collectable in sets of the four kinds, or spent once for
/// +2 on a dig. Everything on it is documented; <see cref="Sources"/> says where.
/// </summary>
/// <param name="Id">A stable kebab-case identifier, unique across the deck.</param>
/// <param name="Kind">The kind of knowledge.</param>
/// <param name="Title">The title.</param>
/// <param name="ArtKey">The key of the vector art drawn on the card face.</param>
/// <param name="Cuneiform">A short cuneiform line shown on the face (may be empty).</param>
/// <param name="CuneiformReading">The transliteration and meaning of <paramref name="Cuneiform"/> (may be empty).</param>
/// <param name="CardText">Two to four plain sentences read on the card.</param>
/// <param name="LongText">The fuller account shown in the inspector and the Field Journal.</param>
/// <param name="Quote">A short quotation from an ancient text or an excavator (may be empty).</param>
/// <param name="QuoteAttribution">Who said or wrote <paramref name="Quote"/>, and where (may be empty).</param>
/// <param name="Pronunciation">How to say the names on the card (may be empty).</param>
/// <param name="Sources">The sources the texts rest on.</param>
public sealed record TabletCard(
    string Id,
    TabletKind Kind,
    string Title,
    string ArtKey,
    string Cuneiform,
    string CuneiformReading,
    string CardText,
    string LongText,
    string Quote,
    string QuoteAttribution,
    string Pronunciation,
    string Sources)
{
    /// <summary>The points a Tablet is worth at the end of the game.</summary>
    public const int PointValue = 2;

    /// <summary>The bonus a Tablet adds to one dig when it is spent.</summary>
    public const int DigBonus = 2;
}
