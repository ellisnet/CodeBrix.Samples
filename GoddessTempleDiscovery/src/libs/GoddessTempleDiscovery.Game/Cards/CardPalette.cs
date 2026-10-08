using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Game.Cards;

/// <summary>The art bible's palette, and the frame-band colour of every kind of card.</summary>
public static class CardPalette
{
    /// <summary>INK: every outline and the text on light plates.</summary>
    public const string Ink = "#1E1A17";

    /// <summary>CLAY: mudbrick, the band of a discovery.</summary>
    public const string Clay = "#C8955A";

    /// <summary>CLAY_SHADE.</summary>
    public const string ClayShade = "#9C6B3C";

    /// <summary>LIMESTONE: the plates and the newspaper paper.</summary>
    public const string Limestone = "#EDE6D6";

    /// <summary>MOSAIC_RED: the band of a season.</summary>
    public const string MosaicRed = "#B8322A";

    /// <summary>LAPIS: the band of a specialist.</summary>
    public const string Lapis = "#2A4B8D";

    /// <summary>LAPIS_LIGHT.</summary>
    public const string LapisLight = "#5B7BC4";

    /// <summary>GOLD: the band of a favor and of a starred discovery.</summary>
    public const string Gold = "#D9A441";

    /// <summary>GOLD_DEEP.</summary>
    public const string GoldDeep = "#A8761F";

    /// <summary>REED.</summary>
    public const string Reed = "#7A8A3A";

    /// <summary>SKY: the ground of the art window.</summary>
    public const string Sky = "#EFE3C8";

    /// <summary>NIGHT: the Deco grounds.</summary>
    public const string Night = "#16213A";

    /// <summary>The band colour of a discovery: GOLD for Her stars, CLAY otherwise.</summary>
    /// <param name="card">The discovery.</param>
    /// <returns>A CSS hex colour.</returns>
    public static string Band(DiscoveryCard card) => card != null && card.IsStarred ? Gold : Clay;

    /// <summary>The band colour of a tablet kind.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>A CSS hex colour.</returns>
    public static string Band(TabletKind kind) => kind switch
    {
        TabletKind.Goddess => LapisLight,
        TabletKind.Pantheon => GoldDeep,
        TabletKind.Culture => Reed,
        _ => ClayShade,
    };

    /// <summary>The text colour that reads on a band: INK on light bands, LIMESTONE on dark ones.</summary>
    /// <param name="band">The band colour, "#RRGGBB".</param>
    /// <returns>A CSS hex colour.</returns>
    public static string OnBand(string band)
    {
        if (band == null || band.Length != 7)
        {
            return Ink;
        }

        var r = System.Convert.ToInt32(band.Substring(1, 2), 16);
        var g = System.Convert.ToInt32(band.Substring(3, 2), 16);
        var b = System.Convert.ToInt32(band.Substring(5, 2), 16);
        var luma = (0.299 * r) + (0.587 * g) + (0.114 * b);
        return luma > 150 ? Ink : Limestone;
    }
}
