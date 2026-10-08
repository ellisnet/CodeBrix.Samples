using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using GoddessTempleDiscovery.Assets;
using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Game.Cards;

/// <summary>
/// Builds the face SVG of every card from the Assets art: a 250 x 400 card whose frame is the Deco frame piece with
/// the kind's colour band, the title and every line of text set as vector paths (Merriweather), a cuneiform line as
/// vector paths (Noto Sans Cuneiform), the card's art inlined in the window, and the number badges and Her star.
/// Every face is self-contained: no font, no external reference.
/// </summary>
/// <remarks>Not thread-safe; use one composer per thread.</remarks>
public sealed class CardFaceComposer : IDisposable
{
    /// <summary>The face width.</summary>
    public const int Width = 250;

    /// <summary>The face height.</summary>
    public const int Height = 400;

    /// <summary>The largest a face may be, in bytes of SVG text.</summary>
    public const int MaxBytes = 200 * 1024;

    /// <summary>The art drawn when a card's art key is not in the catalog.</summary>
    public const string FallbackArtKey = "symbol-eight-pointed-star";

    /// <summary>The Deco frame of every card.</summary>
    public const string FrameKey = "deco-card-frame";

    /// <summary>The Deco frame of Her starred cards.</summary>
    public const string StarredFrameKey = "deco-card-frame-starred";

    //The window the art shows through, and the two limestone plates (deco-card-frame.svg)
    private const float WindowX = 18, WindowY = 76, WindowW = 214, WindowH = 198;
    private const float TitleTop = 24.5f, TitleBottom = 57.5f, TextTop = 290.5f, TextBottom = 375.5f;
    private const float PlateInnerWidth = 196;
    private const float BandY = 274, BandH = 13;

    private readonly VectorText _bold;
    private readonly VectorText _regular;
    private readonly VectorText _cuneiform;
    private readonly Dictionary<string, SvgInliner.Picture> _pictures = new Dictionary<string, SvgInliner.Picture>(StringComparer.Ordinal);

    /// <summary>Creates a composer over the embedded faces.</summary>
    public CardFaceComposer()
    {
        _bold = new VectorText(FontAssets.MerriweatherBold);
        _regular = new VectorText(FontAssets.MerriweatherRegular);
        _cuneiform = new VectorText(FontAssets.NotoSansCuneiform, _regular);
    }

    /// <summary>The table artwork key of a discovery's or tablet's face, "face/&lt;id&gt;".</summary>
    /// <param name="id">The card id.</param>
    /// <returns>The key.</returns>
    public static string FaceKey(string id) => "face/" + id;

    /// <summary>The face key of a specialist role (the copies share one face).</summary>
    /// <param name="role">The role.</param>
    /// <returns>The key.</returns>
    public static string FaceKey(SpecialistRole role) => "face/specialist-" + CardText.RoleKey(role);

    /// <summary>The face key of a favor.</summary>
    /// <param name="card">The favor.</param>
    /// <returns>The key.</returns>
    public static string FaceKey(FavorCard card) => "face/favor-" + card.Id;

    /// <summary>The face key of a season.</summary>
    /// <param name="card">The season.</param>
    /// <returns>The key.</returns>
    public static string FaceKey(SeasonCard card) => "face/season-" + card.Index.ToString(CultureInfo.InvariantCulture);

    /// <summary>The face of a discovery: CLAY band (GOLD for Her stars), Dig Number and points badges, Her star.</summary>
    /// <param name="card">The discovery.</param>
    /// <returns>The SVG text.</returns>
    public string Compose(DiscoveryCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        var band = CardPalette.Band(card);
        var face = Begin(card.ArtKey, card.IsStarred);
        Band(face, band, CardText.PeriodName(card.Period));
        Title(face, card.Title);
        Body(face, card.Cuneiform, card.CardText);
        Badge(face, 38, 96, CardText.N(card.DigNumber), "DIG", CardPalette.Night, CardPalette.Gold, CardPalette.Limestone);
        Badge(face, 212, 96, CardText.N(card.Points), "PTS", CardPalette.Gold, CardPalette.Ink, CardPalette.Ink);
        if (card.IsStarred)
        {
            Star(face, 125, 256, 12);
        }

        return End(face);
    }

    /// <summary>The face of a tablet: its band coloured by kind.</summary>
    /// <param name="card">The tablet.</param>
    /// <returns>The SVG text.</returns>
    public string Compose(TabletCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        var face = Begin(card.ArtKey, card.Kind == TabletKind.Goddess);
        Band(face, CardPalette.Band(card.Kind), "TABLET · " + CardText.TabletKindName(card.Kind));
        Title(face, card.Title);
        Body(face, card.Cuneiform, card.CardText);
        Badge(face, 38, 96, "+" + CardText.N(TabletCard.DigBonus), "DIG", CardPalette.Night, CardPalette.Gold, CardPalette.Limestone);
        Badge(face, 212, 96, CardText.N(TabletCard.PointValue), "PTS", CardPalette.Gold, CardPalette.Ink, CardPalette.Ink);
        return End(face);
    }

    /// <summary>The face of a specialist: LAPIS band and the recruit cost.</summary>
    /// <param name="card">The specialist.</param>
    /// <returns>The SVG text.</returns>
    public string Compose(SpecialistCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        var face = Begin(card.ArtKey, false);
        Band(face, CardPalette.Lapis, "SPECIALIST");
        Title(face, card.Title);
        Body(face, string.Empty, card.CardText);
        Badge(face, 38, 96, CardText.N(card.Cost), "COST", CardPalette.Night, CardPalette.Gold, CardPalette.Limestone);
        return End(face);
    }

    /// <summary>The face of a favor: GOLD band and Her star.</summary>
    /// <param name="card">The favor.</param>
    /// <returns>The SVG text.</returns>
    public string Compose(FavorCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        var face = Begin(card.ArtKey, true);
        Band(face, CardPalette.Gold, "FAVOR OF THE GODDESS");
        Title(face, card.Title);
        Body(face, string.Empty, card.EffectText + " " + card.Fact);
        Star(face, 125, 256, 12);
        return End(face);
    }

    /// <summary>The face of a season: MOSAIC_RED band and the year.</summary>
    /// <param name="card">The season.</param>
    /// <returns>The SVG text.</returns>
    public string Compose(SeasonCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        var face = Begin(card.ArtKey, false);
        Band(face, CardPalette.MosaicRed, "SEASON " + card.Year);
        Title(face, card.Title);
        Body(face, string.Empty, CardText.EffectLine(card) + " " + card.Story);
        return End(face);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _bold.Dispose();
        _regular.Dispose();
        _cuneiform.Dispose();
    }

    private sealed class Face
    {
        public XElement Root;
        public int Pieces;
    }

    private SvgInliner.Picture Picture(string key)
    {
        if (!ArtCatalog.Contains(key))
        {
            key = FallbackArtKey;
        }

        if (!_pictures.TryGetValue(key, out var picture))
        {
            picture = SvgInliner.Parse(ArtCatalog.ReadSvg(key));
            _pictures[key] = picture;
        }

        return picture;
    }

    private Face Begin(string artKey, bool starredFrame)
    {
        var ns = SvgInliner.Svg;
        var root = new XElement(ns + "svg",
            new XAttribute("viewBox", "0 0 250 400"),
            new XAttribute("width", Width),
            new XAttribute("height", Height));
        var face = new Face { Root = root };

        //The window ground, then the art in it, then the frame over both (the frame's ground has the window cut out)
        root.Add(Rect(WindowX, WindowY, WindowW, WindowH, CardPalette.Sky));
        root.Add(SvgInliner.Fit(Picture(artKey), Next(face), WindowX + 4, WindowY + 4, WindowW - 8, WindowH - 8, clip: true));
        root.Add(SvgInliner.Fit(Picture(starredFrame ? StarredFrameKey : FrameKey), Next(face), 0, 0, Width, Height, stretch: true));
        return face;
    }

    private static string Next(Face face) => "p" + (++face.Pieces).ToString(CultureInfo.InvariantCulture) + "-";

    private static string End(Face face) => face.Root.ToString(SaveOptions.DisableFormatting);

    private void Band(Face face, string colour, string label)
    {
        face.Root.Add(Rect(WindowX, BandY, WindowW, BandH, colour));
        var text = (label ?? string.Empty).ToUpperInvariant();
        var size = 7.2f;
        while (size > 4.5f && _bold.Measure(text, size, 120) > PlateInnerWidth)
        {
            size -= 0.3f;
        }

        face.Root.Add(PathElement(_bold.CentredPathData(text, size, Width / 2f, BandY + 9.6f, 120), CardPalette.OnBand(colour), "band"));
    }

    private void Title(Face face, string title)
    {
        var text = title ?? string.Empty;
        var size = 15f;
        IReadOnlyList<string> lines = new[] { text };
        while (size > 8f)
        {
            lines = _bold.Wrap(text, size, PlateInnerWidth);
            var height = lines.Count * size * 1.12f;
            if (lines.Count <= 2 && height <= TitleBottom - TitleTop - 2 && lines.All(l => _bold.Measure(l, size) <= PlateInnerWidth))
            {
                break;
            }

            size -= 0.5f;
        }

        if (lines.Count > 2)
        {
            lines = lines.Take(2).ToArray();
        }

        var lineHeight = size * 1.12f;
        var top = ((TitleTop + TitleBottom) / 2f) - (lines.Count * lineHeight / 2f);
        var data = string.Concat(lines.Select((line, i) =>
            _bold.CentredPathData(line, size, Width / 2f, top + (i * lineHeight) + (size * 0.86f))));
        face.Root.Add(PathElement(data, CardPalette.Ink, "title"));
    }

    private void Body(Face face, string cuneiform, string text)
    {
        var y = TextTop + 3;
        if (!string.IsNullOrWhiteSpace(cuneiform))
        {
            var size = 12f;
            while (size > 6f && _cuneiform.Measure(cuneiform, size) > PlateInnerWidth)
            {
                size -= 0.5f;
            }

            y += size;
            face.Root.Add(PathElement(_cuneiform.CentredPathData(cuneiform.Trim(), size, Width / 2f, y), CardPalette.GoldDeep, "cuneiform"));
            y += 3;
        }

        //Two or three lines of the card text; the inspector and the journal carry the rest
        const float textSize = 8.2f;
        const float lineHeight = 11.6f;
        const int maxLines = 3;
        var lines = _regular.Wrap(text ?? string.Empty, textSize, PlateInnerWidth - 4).ToList();
        var room = Math.Min(maxLines, (int)Math.Floor((TextBottom - 2 - y) / lineHeight));
        if (lines.Count > room && room > 0)
        {
            lines = lines.Take(room).ToList();
            var last = lines[room - 1];
            while (last.Length > 0 && _regular.Measure(last + "…", textSize) > PlateInnerWidth - 4)
            {
                last = last.Substring(0, last.Length - 1);
            }

            lines[room - 1] = last.TrimEnd() + "…";
        }

        var data = string.Concat(lines.Take(Math.Max(0, room)).Select((line, i) =>
            _regular.CentredPathData(line, textSize, Width / 2f, y + ((i + 1) * lineHeight) - 1)));
        face.Root.Add(PathElement(data, CardPalette.Ink, "text"));
    }

    private void Badge(Face face, float cx, float cy, string value, string caption, string fill, string stroke, string ink)
    {
        var ns = SvgInliner.Svg;
        face.Root.Add(new XElement(ns + "circle",
            new XAttribute("cx", F(cx)), new XAttribute("cy", F(cy)), new XAttribute("r", "15"),
            new XAttribute("fill", fill), new XAttribute("stroke", stroke), new XAttribute("stroke-width", "2")));
        face.Root.Add(new XElement(ns + "circle",
            new XAttribute("cx", F(cx)), new XAttribute("cy", F(cy)), new XAttribute("r", "12"),
            new XAttribute("fill", "none"), new XAttribute("stroke", stroke), new XAttribute("stroke-width", "0.8")));
        face.Root.Add(PathElement(_bold.CentredPathData(value, 11.5f, cx, cy + 3.2f), ink, "badge-value"));
        face.Root.Add(PathElement(_bold.CentredPathData(caption, 4.6f, cx, cy + 9.6f, 100), ink, "badge-caption"));
    }

    private void Star(Face face, float cx, float cy, float radius)
    {
        var ns = SvgInliner.Svg;
        face.Root.Add(new XElement(ns + "circle",
            new XAttribute("cx", F(cx)), new XAttribute("cy", F(cy)), new XAttribute("r", F(radius + 3)),
            new XAttribute("fill", CardPalette.Lapis), new XAttribute("stroke", CardPalette.Gold), new XAttribute("stroke-width", "1.5")));
        face.Root.Add(SvgInliner.Fit(Picture("symbol-eight-pointed-star"), Next(face), cx - radius, cy - radius, radius * 2, radius * 2));
    }

    private static XElement Rect(float x, float y, float w, float h, string fill) =>
        new XElement(SvgInliner.Svg + "rect",
            new XAttribute("x", F(x)), new XAttribute("y", F(y)), new XAttribute("width", F(w)), new XAttribute("height", F(h)),
            new XAttribute("fill", fill));

    private static XElement PathElement(string data, string fill, string role) =>
        new XElement(SvgInliner.Svg + "path",
            new XAttribute("class", role),
            new XAttribute("d", string.IsNullOrEmpty(data) ? "M0,0" : data),
            new XAttribute("fill", fill));

    private static string F(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
