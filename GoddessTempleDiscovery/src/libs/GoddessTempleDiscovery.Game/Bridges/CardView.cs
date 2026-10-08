using System;
using System.Collections.Generic;

namespace GoddessTempleDiscovery.Game.Bridges;

/// <summary>
/// A card as the XAML side shows it: every text of the card, its facts, its newspaper headline and its pictures
/// (the composed SVG face, and PNG renderings of the face and of the art for the image controls).
/// </summary>
public sealed class CardView
{
    /// <summary>The card's id (a discovery or tablet id, a favor id, a specialist role, or a season year).</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>The title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>What kind of card it is.</summary>
    public CardKind Kind { get; init; }

    /// <summary>The kind as the inspector labels it, such as "Discovery" or "Tablet - Goddess".</summary>
    public string KindLabel { get; init; } = string.Empty;

    /// <summary>The period line, such as "Late Uruk period, about 3500 to 3300 BCE" (may be empty).</summary>
    public string Period { get; init; } = string.Empty;

    /// <summary>The art key of the card's picture.</summary>
    public string ArtKey { get; init; } = string.Empty;

    /// <summary>The cuneiform line (may be empty).</summary>
    public string Cuneiform { get; init; } = string.Empty;

    /// <summary>The reading of the cuneiform line (may be empty).</summary>
    public string CuneiformReading { get; init; } = string.Empty;

    /// <summary>The short text printed on the card.</summary>
    public string CardText { get; init; } = string.Empty;

    /// <summary>The fuller account.</summary>
    public string LongText { get; init; } = string.Empty;

    /// <summary>The labelled facts, such as Excavated by, Season, Where now, Dig Number and Points.</summary>
    public IReadOnlyList<FactLine> Facts { get; init; } = Array.Empty<FactLine>();

    /// <summary>A quotation (may be empty).</summary>
    public string Quote { get; init; } = string.Empty;

    /// <summary>Who said or wrote the quotation (may be empty).</summary>
    public string QuoteAttribution { get; init; } = string.Empty;

    /// <summary>The sources the texts rest on.</summary>
    public string Sources { get; init; } = string.Empty;

    /// <summary>The newspaper banner headline, in capitals.</summary>
    public string Headline { get; init; } = string.Empty;

    /// <summary>The newspaper sub-head.</summary>
    public string SubHead { get; init; } = string.Empty;

    /// <summary>The date line, such as "WARKA, IRAQ — WINTER 1930/31".</summary>
    public string Dateline { get; init; } = string.Empty;

    /// <summary>The byline, such as "By our correspondent with the Lapis Road Society".</summary>
    public string Byline { get; init; } = string.Empty;

    /// <summary>The caption under the framed photograph.</summary>
    public string Caption { get; init; } = string.Empty;

    /// <summary>Where the find is now (the WHERE IT IS NOW sidebar; may be empty).</summary>
    public string WhereNow { get; init; } = string.Empty;

    /// <summary>True for one of Her star-marked finds (the EXTRA! badge).</summary>
    public bool IsStarred { get; init; }

    /// <summary>The composed SVG face (250 x 400).</summary>
    public string SvgFace { get; init; } = string.Empty;

    /// <summary>The face rendered to PNG at 500 x 800, for the XAML image.</summary>
    public byte[] PngFace { get; init; } = Array.Empty<byte>();

    /// <summary>The card's art rendered to PNG (the newspaper's framed photograph).</summary>
    public byte[] ArtPng { get; init; } = Array.Empty<byte>();

    /// <summary>Seconds after which the inspector closes itself unless the pointer is over it; 0 to stay open.</summary>
    public double AutoCloseSeconds { get; init; }
}
