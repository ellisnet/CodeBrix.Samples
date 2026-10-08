namespace GoddessTempleDiscovery.Rules.Cards;

/// <summary>What kind of thing a discovery is.</summary>
public enum DiscoveryKind
{
    /// <summary>A building or part of one.</summary>
    Building,
    /// <summary>A deposit: a buried group of objects, a foundation deposit, a sealed room.</summary>
    Deposit,
    /// <summary>A single object.</summary>
    Object,
    /// <summary>A stratum: a layer, a floor, a level of the deep sounding.</summary>
    Stratum,
    /// <summary>An inscription: a stamped brick, a tablet, a sealing.</summary>
    Inscription,
}

/// <summary>
/// One real discovery of the Uruk excavations: a card of the Site deck. Face-down it is a trench; face-up it is
/// the find. Every text on it is documented; <see cref="Sources"/> says where.
/// </summary>
/// <param name="Id">A stable kebab-case identifier, unique across the deck.</param>
/// <param name="Title">The English name, with the German name in parentheses on first use where one exists.</param>
/// <param name="Kind">What kind of thing it is.</param>
/// <param name="Period">The period it belongs to.</param>
/// <param name="Level">The Eanna level or horizon, such as "Uruk IVa" or "Neo-Babylonian".</param>
/// <param name="ApproximateDate">A plain date range, such as "about 3500 to 3300 BCE".</param>
/// <param name="Tier">The depth tier, 1 (surface) to 9 (the deep sounding); see <see cref="DepthTiers"/>.</param>
/// <param name="IsStarred">True when the discovery is one of Holy Inanna's own: Her temples, Her vase, Her mask.</param>
/// <param name="ArtKey">The key of the vector art drawn on the card face.</param>
/// <param name="Cuneiform">A short cuneiform line shown on the face (may be empty).</param>
/// <param name="CuneiformReading">The transliteration and meaning of <paramref name="Cuneiform"/> (may be empty).</param>
/// <param name="CardText">Two to four plain sentences read on the card.</param>
/// <param name="LongText">The fuller account shown in the inspector and the Field Journal.</param>
/// <param name="ExcavatedBy">Who brought it to light.</param>
/// <param name="SeasonFound">The campaign season, such as "winter 1930/31", or "season not recorded".</param>
/// <param name="WhereNow">Where the find is today, with any caveat.</param>
/// <param name="Sources">The sources the texts rest on.</param>
public sealed record DiscoveryCard(
    string Id,
    string Title,
    DiscoveryKind Kind,
    Period Period,
    string Level,
    string ApproximateDate,
    int Tier,
    bool IsStarred,
    string ArtKey,
    string Cuneiform,
    string CuneiformReading,
    string CardText,
    string LongText,
    string ExcavatedBy,
    string SeasonFound,
    string WhereNow,
    string Sources)
{
    /// <summary>The Dig Number of the card's tier.</summary>
    public int DigNumber => DepthTiers.DigNumber(Tier);

    /// <summary>The points the card scores when published (half, rounded down, when left unpublished).</summary>
    public int Points => DepthTiers.Points(Tier);

    /// <summary>
    /// The newspaper banner headline the discovery is announced with, in capitals, such as
    /// "LIMESTONE TEMPLE RISES FROM THE MUD"; empty until the headline pass fills it (see
    /// <c>Catalog.HeadlineFor</c>, which falls back to one made from the title).
    /// </summary>
    public string Headline { get; init; } = string.Empty;

    /// <summary>The sub-head under the banner, one sentence; empty until the headline pass fills it.</summary>
    public string SubHead { get; init; } = string.Empty;
}
