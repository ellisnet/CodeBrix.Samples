using CodeBrix.PdfDocuments;
using CodeBrix.PdfDocuments.Drawing;
using CodeBrix.PdfDocuments.Pdf;

namespace InannaOctaquetra.Reading.Services.Pdf;

/// <summary>
/// A vertical cursor over a sequence of pages. The low-level PDF API has no concept of a margin
/// or of a page break, so the report measures every block before it commits to it and asks the
/// flow for a fresh page when one would not fit.
/// </summary>
internal sealed class PdfPageFlow : IDisposable
{
    private readonly PdfDocument _document;
    private readonly PageSize _pageSize;
    private readonly string _headerText;
    private XGraphics? _gfx;

    public PdfPageFlow(PdfDocument document, PageSize pageSize, string headerText)
    {
        _document = document;
        _pageSize = pageSize;
        _headerText = headerText;

        // Page metrics are fixed by the page size; take them from a throwaway probe so that
        // callers can plan a layout before the first page exists.
        var probe = new PdfPage { Size = pageSize, Orientation = PageOrientation.Portrait };
        PageWidth = probe.Width.Point;
        PageHeight = probe.Height.Point;
    }

    public double MarginLeft { get; init; } = 60;
    public double MarginRight { get; init; } = 60;
    public double MarginTop { get; init; } = 82;
    public double MarginBottom { get; init; } = 66;

    public double PageWidth { get; }
    public double PageHeight { get; }

    public double ContentLeft => MarginLeft;
    public double ContentRight => PageWidth - MarginRight;
    public double ContentWidth => PageWidth - MarginLeft - MarginRight;
    public double ContentTop => MarginTop;
    public double ContentBottom => PageHeight - MarginBottom;

    /// <summary>The vertical cursor, in points from the top of the page.</summary>
    public double Y { get; set; }

    /// <summary>Space left on the current page below the cursor.</summary>
    public double Remaining => ContentBottom - Y;

    /// <summary>The graphics context of the current page.</summary>
    public XGraphics Gfx => _gfx ?? throw new InvalidOperationException("No page has been started.");

    /// <summary>1-based number of the current page.</summary>
    public int PageNumber { get; private set; }

    /// <summary>
    /// Closes the current page and starts a new one. Decorated pages get the parchment ground,
    /// the running header and the centred page number; the cover does not.
    /// </summary>
    public XGraphics BeginPage(bool decorate = true)
    {
        _gfx?.Dispose();

        var page = _document.AddPage();
        page.Size = _pageSize;
        page.Orientation = PageOrientation.Portrait;
        PageNumber = _document.PageCount;

        _gfx = XGraphics.FromPdfPage(page);
        _gfx.DrawRectangle(new XSolidBrush(PdfPalette.Ivory), new XRect(0, 0, PageWidth, PageHeight));

        if (decorate)
        {
            DrawRunningHeader(_gfx);
            DrawFooter(_gfx);
        }

        Y = ContentTop;
        return _gfx;
    }

    /// <summary>Starts a new page if <paramref name="needed"/> points would not fit below the cursor.</summary>
    /// <returns><c>true</c> when a page break happened.</returns>
    public bool EnsureSpace(double needed)
    {
        if (_gfx is null)
        {
            BeginPage();
            return true;
        }

        if (Y + needed <= ContentBottom) return false;
        BeginPage();
        return true;
    }

    /// <summary>Moves the cursor down, never past the bottom of the text area.</summary>
    public void Advance(double amount) => Y = Math.Min(ContentBottom, Y + amount);

    private void DrawRunningHeader(XGraphics gfx)
    {
        // a band of royal purple velvet across the head of the page, edged in gold
        const double bandHeight = 58;
        PdfOrnaments.Velvet(gfx, new XRect(0, 0, PageWidth, bandHeight), 0.42, offsetY: PageNumber * 137);
        PdfOrnaments.StarScatter(gfx, new XRect(0, 0, PageWidth, bandHeight), PageNumber * 131 + 7, 26, 6, onVelvet: true,
            keepClear: [new XRect(ContentLeft - 6, 22, PageWidth - 2 * ContentLeft + 12, 22)]);
        gfx.DrawLine(new XPen(PdfPalette.Gold, 1.1), new XPoint(0, bandHeight), new XPoint(PageWidth, bandHeight));
        gfx.DrawLine(new XPen(PdfPalette.GoldDeep.Alpha(0.5), 0.4), new XPoint(0, bandHeight + 2.6), new XPoint(PageWidth, bandHeight + 2.6));

        var font = new XFont(PdfFonts.Serif, 6.9, XFontStyle.Bold);
        var baseline = 36;
        PdfText.DrawTracked(gfx, _headerText, font, new XSolidBrush(PdfPalette.GoldPale), ContentLeft, baseline, 1.75);

        // a small Octaquetra closes the header line on the outer edge
        PdfOrnaments.Octaquetra(gfx, new XPoint(ContentRight - 5, baseline - 2.4), 7.2, PdfPalette.Gold);

        // a few gold sparkles in the parchment margins, as if the velvet has shed its stars
        PdfOrnaments.StarScatter(gfx, new XRect(8, bandHeight + 10, MarginLeft - 20, PageHeight - bandHeight - 70),
            PageNumber * 977 + 3, 7, 2, onVelvet: false);
        PdfOrnaments.StarScatter(gfx, new XRect(PageWidth - MarginRight + 12, bandHeight + 10, MarginRight - 20, PageHeight - bandHeight - 70),
            PageNumber * 613 + 11, 7, 2, onVelvet: false);
    }

    private void DrawFooter(XGraphics gfx)
    {
        const double bandHeight = 34;
        var top = PageHeight - bandHeight;
        PdfOrnaments.Velvet(gfx, new XRect(0, top, PageWidth, bandHeight), 0.4, offsetY: PageNumber * 211 + 400);
        PdfOrnaments.StarScatter(gfx, new XRect(0, top, PageWidth, bandHeight), PageNumber * 211 + 5, 18, 4, onVelvet: true,
            keepClear: [new XRect(PageWidth / 2 - 40, top + 6, 80, 22)]);
        gfx.DrawLine(new XPen(PdfPalette.GoldDeep.Alpha(0.5), 0.4), new XPoint(0, top - 2.6), new XPoint(PageWidth, top - 2.6));
        gfx.DrawLine(new XPen(PdfPalette.Gold, 1.1), new XPoint(0, top), new XPoint(PageWidth, top));

        var font = new XFont(PdfFonts.SerifLight, 8.2);
        var baseline = top + 21;
        PdfText.DrawTracked(gfx, $"— {PageNumber} —", font, new XSolidBrush(PdfPalette.GoldPale),
            ContentLeft, baseline, 1.1, TrackedAlign.Center, ContentWidth);
        PdfOrnaments.Sparkle(gfx, new XPoint(PageWidth / 2 - 30, baseline - 3), 3.2, PdfPalette.Gold, 0.9);
        PdfOrnaments.Sparkle(gfx, new XPoint(PageWidth / 2 + 30, baseline - 3), 3.2, PdfPalette.Gold, 0.9);
    }

    public void Dispose()
    {
        _gfx?.Dispose();
        _gfx = null;
    }
}
