using CodeBrix.PdfDocCreate.DocumentObjectModel;
using NotionDocumentCreator.CreateDocument.Models;
using System.Linq;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// All layout metrics and shared colors for one book rendering, derived from the
/// selected trim size. Every dimension is in typographic points.
/// </summary>
internal sealed class BookTheme
{
    // ── Palette (warm ink on paper with an oxblood accent) ──────────────
    public static readonly Color Ink = new(31, 30, 28);
    public static readonly Color Accent = new(122, 44, 38);
    public static readonly Color Muted = new(112, 108, 102);
    public static readonly Color Hairline = new(203, 197, 189);

    /// <summary>Soft warm paper tint behind callout sidebars.</summary>
    public static readonly Color PanelTint = new(245, 241, 234);

    /// <summary>Cool neutral tint behind code panels.</summary>
    public static readonly Color CodeTint = new(243, 243, 240);

    public PageSizeInfo Page { get; private init; }

    public double PageWidth { get; private init; }
    public double PageHeight { get; private init; }
    public double InnerMargin { get; private init; }
    public double OuterMargin { get; private init; }
    public double TopMargin { get; private init; }
    public double BottomMargin { get; private init; }

    /// <summary>The width of the text block (page width minus side margins).</summary>
    public double TextWidth => PageWidth - InnerMargin - OuterMargin;

    /// <summary>The height of the text block (page height minus top/bottom margins).</summary>
    public double TextHeight => PageHeight - TopMargin - BottomMargin;

    /// <summary>Body text size in points; every other size in the scale derives from this.</summary>
    public double BodySize { get; private init; }

    /// <summary>Body leading (line spacing) in points.</summary>
    public double Leading => BodySize * 1.47;

    /// <summary>Display size for the cover title.</summary>
    public double CoverTitleSize { get; private init; }

    public double H1Size => BodySize * 1.62;
    public double H2Size => BodySize * 1.27;
    public double H3Size => BodySize * 1.08;
    public double QuoteSize => BodySize * 1.02;
    public double CaptionSize => BodySize * 0.81;
    public double LabelSize => BodySize * 0.70;
    public double TableSize => BodySize * 0.83;
    public double FolioSize => BodySize * 0.88;
    public double RaisedCapSize => BodySize * 2.35;

    public static BookTheme For(PageSizeOption option)
    {
        var page = PageSizeInfo.For(option);

        //Every metric derives from the trim size, so each of the four trims (and any
        //  added later) gets a proportionate book layout instead of one tuned at a
        //  single size: margins as fractions of the page, the inner (binding) margin
        //  a little larger than the outer, and type scaled to the resulting measure
        var innerMargin = page.WidthPoints * 0.125;
        var outerMargin = page.WidthPoints * 0.106;
        var topMargin = page.HeightPoints * 0.09;
        var bottomMargin = page.HeightPoints * 0.113;
        var textWidth = page.WidthPoints - innerMargin - outerMargin;

        //Cap the measure at the classic book line length (~65-75 characters) — on
        //  wide trims (US Letter, A4) the excess goes into the side margins instead
        const double maxMeasure = 435;
        if (textWidth > maxMeasure)
        {
            var extra = (textWidth - maxMeasure) / 2;
            innerMargin += extra;
            outerMargin += extra;
            textWidth = maxMeasure;
        }

        var bodySize = System.Math.Clamp(8.0 + textWidth * 0.0058, 9.0, 11.5);

        return new BookTheme
        {
            Page = page,
            PageWidth = page.WidthPoints,
            PageHeight = page.HeightPoints,
            InnerMargin = innerMargin,
            OuterMargin = outerMargin,
            TopMargin = topMargin,
            BottomMargin = bottomMargin,
            BodySize = bodySize,
            CoverTitleSize = textWidth * 0.09
        };
    }

    /// <summary>
    /// Spaces out the characters of a label ("HISTORY" → "H I S T O R Y") for a
    /// small-caps kicker effect, preserving word boundaries with wider gaps.
    /// Non-breaking spaces are used because MigraDoc collapses runs of ordinary
    /// blanks into a single space when laying out text.
    /// </summary>
    public static string Letterspace(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) { return ""; }

        const char letterGap = '\u00A0';
        const string wordGap = "\u00A0\u00A0\u00A0";

        var words = text.ToUpperInvariant()
            .Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
            .Select(word => string.Join(letterGap, word.ToCharArray()));
        return string.Join(wordGap, words);
    }

    /// <summary>
    /// Letterspaces like <see cref="Letterspace"/> but keeps the word gaps
    /// breakable (NBSP–space–NBSP), so a long display line — a letterspaced cover
    /// title — can wrap between words instead of overflowing the measure.
    /// </summary>
    public static string LetterspaceBreakable(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) { return ""; }

        const char letterGap = '\u00A0';
        const string wordGap = "\u00A0 \u00A0";

        var words = text.ToUpperInvariant()
            .Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
            .Select(word => string.Join(letterGap, word.ToCharArray()));
        return string.Join(wordGap, words);
    }
}
