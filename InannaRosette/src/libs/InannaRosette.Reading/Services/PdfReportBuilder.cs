using System.Globalization;
using System.Text;
using CodeBrix.PdfDocuments;
using CodeBrix.PdfDocuments.Drawing;
using CodeBrix.PdfDocuments.Pdf;
using InannaRosette.Reading.Data;
using InannaRosette.Reading.Models;
using InannaRosette.Reading.Services.Pdf;

namespace InannaRosette.Reading.Services;

/// <summary>Options for <see cref="PdfReportBuilder"/>.</summary>
public sealed class PdfReportOptions
{
    /// <summary>US Letter instead of the default A4.</summary>
    public bool UseLetter { get; set; } = false;

    /// <summary>The value written to the PDF's Author field.</summary>
    public string Author { get; set; } = "Rosette of Inanna";
}

/// <summary>
/// Renders a <see cref="ReadingInterpretation"/> as a printable report: cover, the spread drawn as
/// vector art with a legend, the opening and insights, one section per station, the counsel and
/// closing invocation, the closing blessing, and an appendix of the cards' lore.
/// </summary>
/// <remarks>
/// Every mark is placed by hand. Text blocks are measured before they are drawn and a page break
/// is taken when one would not fit, so nothing runs off the page and no heading is left stranded
/// at the foot of a page without its first paragraph.
/// </remarks>
public sealed class PdfReportBuilder : IPdfReportBuilder
{
    private readonly PdfReportOptions _options;

    // --- type scale -------------------------------------------------------------------------
    private const double BodySize = 10.2;
    private const double BodyLeading = 15.4;
    private const double KickerSize = 7.4;
    private const double KickerTracking = 1.9;

    // --- station layout ---------------------------------------------------------------------
    private const double ThumbWidth = 44;
    private const double ThumbHeight = ThumbWidth / PdfCardPainter.AspectRatio;   // 5:8
    private const double Gutter = 66;

    // --- spread diagram and legend -----------------------------------------------------------
    private const double LegendHeaderHeight = 16;
    private const double LegendRowHeight = 15.4;
    private const double LegendLead = 20;      // the "LEGEND" kicker and the gap above the table
    private const double StationLabelHeight = 13;
    private const double PetalRadiusRatio = 1.786;

    /// <summary>Creates a builder; <paramref name="options"/> defaults to A4.</summary>
    public PdfReportBuilder(PdfReportOptions? options = null)
    {
        _options = options ?? new PdfReportOptions();
        PdfFonts.EnsureRegistered();
    }

    /// <summary>Builds the whole report and returns the PDF bytes.</summary>
    public byte[] Build(ReadingInterpretation interpretation)
    {
        ArgumentNullException.ThrowIfNull(interpretation);
        PdfFonts.EnsureRegistered();

        var reading = interpretation.Reading;
        var querent = Display(reading.Querent, "the Querent");
        var date = reading.Created.ToString("d MMMM yyyy", CultureInfo.InvariantCulture);

        using var document = new PdfDocument();
        document.Info.Title = interpretation.Title;
        document.Info.Author = _options.Author;
        document.Info.Subject = $"A Rosette of Inanna reading for {querent}, {date}";
        document.Info.Keywords = string.Join(", ", new[]
        {
            "Rosette of Inanna", "oracle", "Mesopotamia", "Sumer", "Inanna", "reading",
            querent, date,
        }.Where(s => !string.IsNullOrWhiteSpace(s)));
        document.Info.Creator = "Rosette of Inanna · PdfReportBuilder";
        document.Info.CreationDate = reading.Created;

        var header = $"ROSETTE OF INANNA  ·  {querent.ToUpperInvariant()}  ·  {date.ToUpperInvariant()}";

        using var flow = new PdfPageFlow(document, _options.UseLetter ? PageSize.Letter : PageSize.A4, header)
        {
            MarginLeft = 62,
            MarginRight = 62,
            MarginTop = 84,
            MarginBottom = 66,
        };

        DrawCover(flow, interpretation, querent, date);
        DrawSpreadPage(flow, interpretation);
        DrawOpeningAndInsights(flow, interpretation);
        DrawStations(flow, interpretation);
        DrawCounsel(flow, interpretation);
        DrawClosingBlessing(flow, interpretation);
        DrawAppendix(flow, interpretation);

        flow.Dispose();

        using var stream = new MemoryStream();
        document.Save(stream);
        return stream.ToArray();
    }

    /// <summary>A safe, descriptive file name such as <c>Rosette-Reading-Jeremy-2026-09-13.pdf</c>.</summary>
    public string SuggestedFileName(ReadingInterpretation interpretation)
    {
        ArgumentNullException.ThrowIfNull(interpretation);

        var reading = interpretation.Reading;
        var name = Sanitize(reading.Querent);
        var stamp = reading.Created.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return string.IsNullOrEmpty(name)
            ? $"Rosette-Reading-{stamp}.pdf"
            : $"Rosette-Reading-{name}-{stamp}.pdf";
    }

    // =============================================================================================
    // 1. Cover
    // =============================================================================================

    private void DrawCover(PdfPageFlow flow, ReadingInterpretation interpretation, string querent, string date)
    {
        var gfx = flow.BeginPage(decorate: false);
        var pw = flow.PageWidth;
        var ph = flow.PageHeight;
        var cx = pw / 2.0;
        var bandHeight = Math.Round(ph * 0.45);

        // --- the night band, full bleed ---------------------------------------------------------
        gfx.DrawRectangle(new XSolidBrush(PdfPalette.Night), new XRect(0, 0, pw, bandHeight));

        // The band is typeset from its foot upwards - date, question, dedication, rule, title -
        // so that the Venus star can take whatever height is left without ever colliding.
        var question = interpretation.Reading.Question?.Trim();
        if (!string.IsNullOrWhiteSpace(question) && Contains(interpretation.Title, question))
        {
            // The reading title already quotes the question; do not print it twice.
            question = null;
        }

        var questionFont = new XFont(PdfFonts.SerifLight, 10.4, XFontStyle.Italic);
        var questionWidth = flow.ContentWidth - 56;
        var questionLines = string.IsNullOrWhiteSpace(question)
            ? []
            : PdfText.WrapLines(gfx, $"“{question}”", questionFont, questionWidth);

        const double questionLeading = 15.2;
        var dateBaseline = bandHeight - 26;
        var questionTop = dateBaseline - 22 - questionLines.Count * questionLeading;
        var dedicationBaseline = questionTop - (questionLines.Count > 0 ? 13 : 0);
        var ruleY = dedicationBaseline - 27;
        var titleBaseline = ruleY - 17;

        var starCenter = new XPoint(cx, Math.Max(56, (titleBaseline - 34) / 2.0 + 6));
        var starRadius = Math.Min(Math.Min(pw, bandHeight) * 0.185, (titleBaseline - 34 - 30) / 2.0);

        // A radial dusk behind the star. Gradient stops ignore alpha, so every stop is opaque.
        var dusk = new XShadingBrush(starCenter, 0, starCenter, bandHeight * 0.95,
        [
            new XGradientStop(0.00, PdfPalette.Lapis.Over(PdfPalette.Night2, 0.55)),
            new XGradientStop(0.38, PdfPalette.Lapis.Over(PdfPalette.Night, 0.28)),
            new XGradientStop(0.78, PdfPalette.Night),
            new XGradientStop(1.00, PdfPalette.Kohl),
        ]);
        gfx.DrawRectangle(dusk, new XRect(0, 0, pw, bandHeight));

        // A scatter of faint stars, kept clear of the emblem.
        var sky = new Random(20260913);
        for (var i = 0; i < 110; i++)
        {
            var x = sky.NextDouble() * pw;
            var y = sky.NextDouble() * bandHeight;
            var d = Math.Sqrt((x - starCenter.X) * (x - starCenter.X) + (y - starCenter.Y) * (y - starCenter.Y));
            if (d < starRadius * 1.75) continue;
            var r = 0.3 + sky.NextDouble() * 0.75;
            gfx.DrawEllipse(new XSolidBrush(PdfPalette.GoldPale.Alpha(0.22 + sky.NextDouble() * 0.38)),
                new XRect(x - r, y - r, r * 2, r * 2));
        }

        // --- the Venus star ---------------------------------------------------------------------
        for (var i = 5; i >= 1; i--)
        {
            var r = starRadius * (0.62 + i * 0.11);
            gfx.DrawEllipse(new XSolidBrush(PdfPalette.LapisGlow.Over(PdfPalette.Night, 0.055)),
                new XRect(starCenter.X - r, starCenter.Y - r, r * 2, r * 2));
        }

        var disc = starRadius * 0.60;
        gfx.DrawEllipse(new XSolidBrush(PdfPalette.Lapis.Over(PdfPalette.Night, 0.85)),
            new XRect(starCenter.X - disc, starCenter.Y - disc, disc * 2, disc * 2));
        gfx.DrawEllipse(new XPen(PdfPalette.GoldDeep.Alpha(0.6), 0.8),
            new XRect(starCenter.X - disc, starCenter.Y - disc, disc * 2, disc * 2));

        PdfOrnaments.VenusStar(gfx, starCenter, starRadius,
            PdfPalette.Gold, PdfPalette.GoldPale, PdfPalette.GoldDeep);

        // A ring of eight tiny rosettes around the star.
        var ringR = starRadius * 1.44;
        for (var i = 0; i < 8; i++)
        {
            var a = Math.PI * 2 * i / 8 + Math.PI / 8;
            PdfOrnaments.Rosette(gfx,
                new XPoint(starCenter.X + Math.Sin(a) * ringR, starCenter.Y - Math.Cos(a) * ringR),
                4.6, PdfPalette.GoldDeep, PdfPalette.Gold, PdfPalette.GoldDeep, 0.8);
        }

        // --- titling inside the band ------------------------------------------------------------
        PdfText.DrawTrackedFitted(gfx, "ROSETTE OF INANNA", PdfFonts.Serif, 19, XFontStyle.Bold,
            new XSolidBrush(PdfPalette.Gold), flow.ContentLeft, titleBaseline, flow.ContentWidth, 7.2,
            TrackedAlign.Center);

        PdfOrnaments.DoubleRule(gfx, cx - 118, cx + 118, ruleY, PdfPalette.GoldDeep.Alpha(0.75), 0.9, 0.4, 2.4);

        PdfText.DrawFitted(gfx, $"A Reading for {querent}", PdfFonts.SerifLight, 13.5, XFontStyle.Regular,
            new XSolidBrush(PdfPalette.Ivory.Alpha(0.93)),
            flow.ContentLeft, dedicationBaseline, flow.ContentWidth, TrackedAlign.Center);

        if (questionLines.Count > 0)
        {
            var baselineQ = questionTop + questionFont.GetHeight() * 0.78;
            foreach (var line in questionLines)
            {
                var lw = gfx.MeasureString(line, questionFont).Width;
                gfx.DrawString(line, questionFont, new XSolidBrush(PdfPalette.Ivory.Alpha(0.70)),
                    new XPoint(cx - lw / 2.0, baselineQ));
                baselineQ += questionLeading;
            }
        }

        PdfText.DrawTracked(gfx, date.ToUpperInvariant(), new XFont(PdfFonts.Serif, 7.4, XFontStyle.Bold),
            new XSolidBrush(PdfPalette.GoldPale.Alpha(0.8)),
            flow.ContentLeft, dateBaseline, 2.6, TrackedAlign.Center, flow.ContentWidth);

        // --- the gold seam and the parchment half ------------------------------------------------
        gfx.DrawRectangle(new XSolidBrush(PdfPalette.GoldDeep), new XRect(0, bandHeight, pw, 3.2));
        gfx.DrawRectangle(new XSolidBrush(PdfPalette.Gold.Alpha(0.55)), new XRect(0, bandHeight + 3.2, pw, 0.9));

        // Reading title.
        var lowerTop = bandHeight + 70;
        var (titleFont, titleLines) = PdfText.FitLines(
            gfx, interpretation.Title, PdfFonts.Serif, 25, XFontStyle.Bold,
            flow.ContentWidth - 24, 3, 15);
        var leading = titleFont.GetHeight() * 1.12;
        var baseline = lowerTop + titleFont.GetHeight() * 0.78;
        foreach (var line in titleLines)
        {
            var w = gfx.MeasureString(line, titleFont).Width;
            gfx.DrawString(line, titleFont, new XSolidBrush(PdfPalette.Ink), new XPoint(cx - w / 2.0, baseline));
            baseline += leading;
        }

        var afterTitle = lowerTop + titleLines.Count * leading;

        PdfOrnaments.Divider(gfx, cx, afterTitle + 20, 190, PdfPalette.GoldDeep);

        // The roll-call, so that the cover already says how much of the flower was laid.
        var laid = interpretation.Reading.Placements.Count;
        PdfText.DrawTracked(gfx,
            laid == 9 ? "THE FULL ROSETTE · NINE STATIONS" : $"A PARTIAL ROSETTE · {laid} OF NINE STATIONS",
            new XFont(PdfFonts.Serif, 7.6, XFontStyle.Bold), new XSolidBrush(PdfPalette.GoldShadow),
            flow.ContentLeft, afterTitle + 50, 2.4, TrackedAlign.Center, flow.ContentWidth);

        // A large, quiet rosette centred in whatever space is left above the footer.
        var ornamentTop = afterTitle + 68;
        var ornamentBottom = ph - 82;
        if (ornamentBottom - ornamentTop > 70)
        {
            var ornamentCenter = new XPoint(cx, (ornamentTop + ornamentBottom) / 2.0);
            var ornamentRadius = Math.Min(80, (ornamentBottom - ornamentTop) / 2.0);
            PdfOrnaments.SpreadRosette(gfx, ornamentCenter, ornamentRadius,
                PdfPalette.GoldDeep.Over(PdfPalette.Ivory, 0.75), PdfPalette.GoldDeep.Over(PdfPalette.Ivory, 0.4));
            PdfOrnaments.Rosette(gfx, ornamentCenter, ornamentRadius * 0.33,
                PdfPalette.GoldDeep, PdfPalette.Gold, PdfPalette.GoldDeep, 0.55);
        }

        // Corner flourishes on the parchment half.
        PdfOrnaments.CornerFlourishes(gfx,
            new XRect(34, bandHeight + 24, pw - 68, ph - bandHeight - 24 - 46), 26,
            PdfPalette.GoldDeep, 0.6);

        // Footer line.
        var footFont = new XFont(PdfFonts.Serif, 6.8, XFontStyle.Bold);
        var footY = ph - 44;
        gfx.DrawLine(new XPen(PdfPalette.GoldDeep.Alpha(0.5), 0.6),
            new XPoint(flow.ContentLeft, footY - 12), new XPoint(flow.ContentRight, footY - 12));
        PdfText.DrawTracked(gfx, "AN ORACLE OF THE DIVINE FEMININE OF SUMER AND AKKAD", footFont,
            new XSolidBrush(PdfPalette.GoldShadow.Over(PdfPalette.Ivory, 0.9)),
            flow.ContentLeft, footY, 2.2, TrackedAlign.Center, flow.ContentWidth);
    }

    // =============================================================================================
    // 2. The spread
    // =============================================================================================

    private void DrawSpreadPage(PdfPageFlow flow, ReadingInterpretation interpretation)
    {
        var gfx = flow.BeginPage();
        var reading = interpretation.Reading;
        var cx = flow.PageWidth / 2.0;

        flow.Y = flow.ContentTop;
        DrawPageTitle(flow, "THE ROSETTE", "The nine stations as they were laid");

        // --- geometry ---------------------------------------------------------------------------
        // The petal radius is 1.786 x the card height, which keeps neighbouring cards clear: the
        // tightest pair (the 45 and 90 degree petals) is separated by R cos(45 deg) = 0.71 R
        // vertically, against a block of one card plus its label. The card is then sized so that
        // the whole flower and its legend fit the page, whichever page size was asked for.
        var stations = RosetteSpread.Positions.Count;
        var legendBlock = LegendLead + LegendHeaderHeight + stations * LegendRowHeight + 18;
        var available = flow.ContentBottom - flow.Y - 8 - legendBlock;
        var cardHeight = Math.Clamp(
            (available - 2 * StationLabelHeight) / (2 * PetalRadiusRatio + 1), 62, 84);
        var radius = cardHeight * PetalRadiusRatio;
        var labelHeight = StationLabelHeight;

        var diagramTop = flow.Y + 8;
        var centerY = diagramTop + radius + cardHeight / 2.0 + labelHeight;
        var center = new XPoint(cx, centerY);

        PdfOrnaments.SpreadRosette(gfx, center, radius + cardHeight * 0.74,
            PdfPalette.GoldDeep.Over(PdfPalette.Ivory, 0.85), PdfPalette.GoldDeep.Over(PdfPalette.Ivory, 0.45));

        // The centre station sits on the ornament; ring it so that it reads as the heart.
        var heartRing = cardHeight * 0.82;
        gfx.DrawEllipse(new XPen(PdfPalette.GoldDeep.Alpha(0.3), 0.7),
            new XRect(center.X - heartRing, center.Y - heartRing, heartRing * 2, heartRing * 2));

        foreach (var position in RosetteSpread.Positions.OrderBy(p => p.Index))
        {
            var stationCenter = StationCenter(center, position, radius);
            var rect = PdfCardPainter.RectFor(stationCenter, cardHeight);
            var placed = reading.At(position.Index);

            // The label sits on the outward side of its card, so it always reads away from the
            // centre of the flower: beneath the three southern petals, above everything else.
            var label = position.IsCenter
                ? position.Title.ToUpperInvariant()
                : $"{Card.ToRoman(position.Index)} · {position.Title.ToUpperInvariant()}";
            var below = position.Index is 4 or 5 or 6;
            var labelBaseline = below ? rect.Y + rect.Height + 11.5 : rect.Y - 5.5;

            PdfText.DrawTrackedFitted(gfx, label, PdfFonts.Serif, 6.3, XFontStyle.Bold,
                new XSolidBrush(PdfPalette.GoldShadow.Over(PdfPalette.Ivory, 0.95)),
                rect.X - 26, labelBaseline, rect.Width + 52, 1.25, TrackedAlign.Center);

            if (placed is not null)
            {
                // A soft drop shadow lifts the card off the ornament.
                gfx.DrawRoundedRectangle(
                    new XSolidBrush(PdfPalette.Ink.Alpha(0.10)),
                    new XRect(rect.X + 1.5, rect.Y + 2.0, rect.Width, rect.Height),
                    new XSize(cardHeight * 0.07, cardHeight * 0.07));
                PdfCardPainter.DrawCard(gfx, rect, placed.Card, placed.IsReversed);
            }
            else
            {
                PdfCardPainter.DrawEmptySlot(gfx, rect);
            }
        }

        flow.Y = centerY + radius + cardHeight / 2.0 + labelHeight + 16;

        // --- the legend -------------------------------------------------------------------------
        DrawLegend(flow, reading);
    }

    private static XPoint StationCenter(XPoint center, SpreadPosition position, double radius)
    {
        if (position.IsCenter) return center;

        // Petal i sits at (i-1) * 45 degrees, measured clockwise from straight up.
        var angle = (position.Index - 1) * 45.0 * Math.PI / 180.0;
        return new XPoint(center.X + Math.Sin(angle) * radius, center.Y - Math.Cos(angle) * radius);
    }

    private void DrawLegend(PdfPageFlow flow, RosetteReading reading)
    {
        var positions = RosetteSpread.Positions.OrderBy(p => p.Index).ToList();
        const double headerHeight = LegendHeaderHeight;
        const double rowHeight = LegendRowHeight;
        var needed = LegendLead + headerHeight + positions.Count * rowHeight;

        if (flow.Remaining < needed)
        {
            flow.BeginPage();
            DrawPageTitle(flow, "THE ROSETTE", "The legend of the nine stations");
        }

        var gfx = flow.Gfx;
        var x = flow.ContentLeft;
        var width = flow.ContentWidth;

        PdfText.DrawTracked(gfx, "LEGEND", new XFont(PdfFonts.Serif, KickerSize, XFontStyle.Bold),
            new XSolidBrush(PdfPalette.GoldShadow), x, flow.Y + 7, KickerTracking);
        flow.Advance(13);

        var col1 = width * 0.335;
        var col2 = width * 0.445;
        var col3 = width - col1 - col2;
        var top = flow.Y;

        // Header row.
        gfx.DrawRectangle(new XSolidBrush(PdfPalette.Night2), new XRect(x, top, width, headerHeight));
        var headFont = new XFont(PdfFonts.Serif, 6.8, XFontStyle.Bold);
        var headBrush = new XSolidBrush(PdfPalette.GoldPale);
        PdfText.DrawTracked(gfx, "STATION", headFont, headBrush, x + 9, top + 10.6, 1.6);
        PdfText.DrawTracked(gfx, "CARD", headFont, headBrush, x + col1 + 9, top + 10.6, 1.6);
        PdfText.DrawTracked(gfx, "ORIENTATION", headFont, headBrush, x + col1 + col2 + 9, top + 10.6, 1.6);

        var y = top + headerHeight;
        var cellFont = new XFont(PdfFonts.Serif, 8.4);
        var cellBold = new XFont(PdfFonts.Serif, 8.4, XFontStyle.Bold);
        var cellLight = new XFont(PdfFonts.SerifLight, 8.4, XFontStyle.Italic);

        for (var i = 0; i < positions.Count; i++)
        {
            var position = positions[i];
            var placed = reading.At(position.Index);
            var row = new XRect(x, y, width, rowHeight);
            if (i % 2 == 1) gfx.DrawRectangle(new XSolidBrush(PdfPalette.Parchment.Over(PdfPalette.Ivory, 0.75)), row);

            // The centre is marked with the Venus star itself; Merriweather has no such glyph.
            if (position.IsCenter)
            {
                PdfOrnaments.VenusStar(gfx, new XPoint(x + 15, y + rowHeight / 2.0), 4.4,
                    PdfPalette.GoldShadow, PdfPalette.GoldDeep, PdfPalette.GoldShadow);
            }
            else
            {
                gfx.DrawString(Card.ToRoman(position.Index), cellBold, new XSolidBrush(PdfPalette.GoldShadow),
                    new XRect(x + 9, y, 24, rowHeight), XStringFormats.CenterLeft);
            }
            gfx.DrawString(position.Title, cellFont, new XSolidBrush(PdfPalette.Ink),
                new XRect(x + 30, y, col1 - 36, rowHeight), XStringFormats.CenterLeft);

            if (placed is null)
            {
                var muted = new XSolidBrush(PdfPalette.Ink.Over(PdfPalette.Ivory, 0.45));
                gfx.DrawString("— not laid —", cellLight, muted,
                    new XRect(x + col1 + 9, y, col2 - 14, rowHeight), XStringFormats.CenterLeft);
                gfx.DrawString("—", cellLight, muted,
                    new XRect(x + col1 + col2 + 9, y, col3 - 14, rowHeight), XStringFormats.CenterLeft);
            }
            else
            {
                gfx.DrawString(placed.Card.Name, cellBold, new XSolidBrush(PdfPalette.Ink),
                    new XRect(x + col1 + 9, y, col2 - 14, rowHeight), XStringFormats.CenterLeft);

                var tag = placed.IsReversed ? "Reversed" : "Upright";
                var tagBrush = new XSolidBrush(placed.IsReversed ? PdfPalette.Carnelian : PdfPalette.Malachite);
                gfx.DrawEllipse(tagBrush, new XRect(x + col1 + col2 + 9, y + rowHeight / 2 - 2, 4, 4));
                gfx.DrawString(tag, cellFont, new XSolidBrush(PdfPalette.Ink),
                    new XRect(x + col1 + col2 + 19, y, col3 - 24, rowHeight), XStringFormats.CenterLeft);
            }

            gfx.DrawLine(new XPen(PdfPalette.GoldDeep.Alpha(0.28), 0.5),
                new XPoint(x, y + rowHeight), new XPoint(x + width, y + rowHeight));
            y += rowHeight;
        }

        gfx.DrawRectangle(new XPen(PdfPalette.GoldDeep.Alpha(0.6), 0.7), new XRect(x, top, width, y - top));
        gfx.DrawLine(new XPen(PdfPalette.GoldDeep.Alpha(0.28), 0.5),
            new XPoint(x + col1, top + headerHeight), new XPoint(x + col1, y));
        gfx.DrawLine(new XPen(PdfPalette.GoldDeep.Alpha(0.28), 0.5),
            new XPoint(x + col1 + col2, top + headerHeight), new XPoint(x + col1 + col2, y));

        flow.Y = y;
    }

    // =============================================================================================
    // 3. Opening and insights
    // =============================================================================================

    private void DrawOpeningAndInsights(PdfPageFlow flow, ReadingInterpretation interpretation)
    {
        flow.BeginPage();
        DrawPageTitle(flow, "THE OPENING", interpretation.Title);

        var gfx = flow.Gfx;
        var x = flow.ContentLeft;
        var width = flow.ContentWidth;

        if (!string.IsNullOrWhiteSpace(interpretation.Opening))
        {
            // The opening is set a little larger, as a standfirst.
            var leadFont = new XFont(PdfFonts.SerifLight, 11.4);
            foreach (var paragraph in SplitParagraphs(interpretation.Opening))
            {
                DrawFlowedParagraph(flow, paragraph, leadFont, new XSolidBrush(PdfPalette.Ink),
                    x, width, 17.2);
                flow.Advance(9);
            }

            flow.Advance(10);
        }

        if (interpretation.Insights.Count == 0) return;

        flow.EnsureSpace(70);
        gfx = flow.Gfx;
        PdfOrnaments.Divider(gfx, x + width / 2.0, flow.Y + 6, width * 0.55, PdfPalette.GoldDeep);
        flow.Advance(24);

        DrawSectionHeading(flow, "WHAT THE ROSETTE SHOWS", "Insights");

        var titleFont = new XFont(PdfFonts.Serif, 11, XFontStyle.Bold);
        var bodyFont = new XFont(PdfFonts.Serif, BodySize);

        foreach (var insight in interpretation.Insights)
        {
            const double indent = 22;
            var textWidth = width - indent;
            var bodyHeight = PdfText.MeasureParagraph(gfx, insight.Text, bodyFont, textWidth, BodyLeading);

            // Keep the insight title with at least its first two lines.
            flow.EnsureSpace(18 + Math.Min(bodyHeight, BodyLeading * 2));
            gfx = flow.Gfx;

            PdfOrnaments.Wedge(gfx, new XPoint(x + 9, flow.Y + 7.5), 8, 3.2, 0,
                new XSolidBrush(PdfPalette.GoldDeep));

            gfx.DrawString(insight.Title, titleFont, new XSolidBrush(PdfPalette.Ink),
                new XPoint(x + indent, flow.Y + 9));
            flow.Advance(16);

            DrawFlowedParagraph(flow, insight.Text, bodyFont, new XSolidBrush(PdfPalette.Ink),
                x + indent, textWidth, BodyLeading);
            flow.Advance(13);
        }
    }

    // =============================================================================================
    // 4. One section per station
    // =============================================================================================

    private void DrawStations(PdfPageFlow flow, ReadingInterpretation interpretation)
    {
        if (interpretation.Positions.Count == 0) return;

        flow.BeginPage();
        DrawPageTitle(flow, "THE STATIONS", "Petal by petal, as the rosette was read");

        var bodyFont = new XFont(PdfFonts.Serif, BodySize);
        var invocationFont = new XFont(PdfFonts.SerifLight, 10.0, XFontStyle.Italic);

        var textX = flow.ContentLeft + Gutter;
        var textWidth = flow.ContentWidth - Gutter;

        var first = true;
        foreach (var entry in interpretation.Positions.OrderBy(p => p.Placed.Position.Index))
        {
            var placed = entry.Placed;
            var position = placed.Position;
            var invocation = placed.Card.Invocation?.Trim();

            // The interpreter often signs a station off with the card's invocation. Set that as
            // the invocation block rather than printing the same sentence twice.
            var paragraphs = entry.Paragraphs
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
            if (!string.IsNullOrWhiteSpace(invocation))
            {
                paragraphs.RemoveAll(p => Contains(p, invocation) && p.Length <= invocation.Length + 24);
            }

            var firstParagraph = paragraphs.Count > 0 ? paragraphs[0] : string.Empty;

            // Keep the kicker, heading, keyword line and the first two lines of prose together.
            var headBlock = 14 + 22 + 14 + Math.Min(
                PdfText.MeasureParagraph(flow.Gfx, firstParagraph, bodyFont, textWidth, BodyLeading),
                BodyLeading * 3);

            if (!first)
            {
                if (flow.Remaining < headBlock + 30) flow.BeginPage();
                else
                {
                    flow.Advance(12);
                    PdfOrnaments.Divider(flow.Gfx, flow.ContentLeft + flow.ContentWidth / 2.0,
                        flow.Y, flow.ContentWidth * 0.45, PdfPalette.GoldDeep.Alpha(0.8));
                    flow.Advance(22);
                }
            }

            first = false;
            flow.EnsureSpace(headBlock);
            var gfx = flow.Gfx;

            var blockTop = flow.Y;

            // --- the card thumbnail in the margin -------------------------------------------------
            var thumbRect = new XRect(flow.ContentLeft, blockTop + 2, ThumbWidth, ThumbHeight);
            gfx.DrawRoundedRectangle(new XSolidBrush(PdfPalette.Ink.Alpha(0.10)),
                new XRect(thumbRect.X + 1.4, thumbRect.Y + 1.9, thumbRect.Width, thumbRect.Height),
                new XSize(ThumbHeight * 0.07, ThumbHeight * 0.07));
            PdfCardPainter.DrawCard(gfx, thumbRect, placed.Card, placed.IsReversed, CardDetail.Compact, showReversedMark: true);

            // --- kicker, heading, keywords ---------------------------------------------------------
            var kicker = position.IsCenter
                ? $"THE HEART · {position.Subtitle.ToUpperInvariant()}"
                : $"PETAL {Card.ToRoman(position.Index)} · {position.Title.ToUpperInvariant()} · {position.Subtitle.ToUpperInvariant()}";

            PdfText.DrawTrackedFitted(gfx, kicker, PdfFonts.Serif, KickerSize, XFontStyle.Bold,
                new XSolidBrush(PdfPalette.GoldShadow), textX, blockTop + 8, textWidth, KickerTracking);

            var headingY = blockTop + 30;
            var heading = StripOrientation(
                string.IsNullOrWhiteSpace(entry.Heading) ? placed.Card.Name : entry.Heading);
            var (hFont, hLines) = PdfText.FitLines(gfx, heading, PdfFonts.Serif, 15.5, XFontStyle.Bold,
                textWidth, 2, 11.5);
            var hLead = hFont.GetHeight() * 1.02;
            var hBaseline = headingY;
            foreach (var line in hLines)
            {
                gfx.DrawString(line, hFont, new XSolidBrush(PdfPalette.Ink), new XPoint(textX, hBaseline));
                hBaseline += hLead;
            }

            var afterHeading = hBaseline - hLead + 6;

            // Orientation tag.
            var tagText = placed.IsReversed ? "REVERSED" : "UPRIGHT";
            var tagColour = placed.IsReversed ? PdfPalette.Carnelian : PdfPalette.Malachite;
            var tagFont = new XFont(PdfFonts.Serif, 6.6, XFontStyle.Bold);
            var tagWidth = PdfText.MeasureTracked(gfx, tagText, tagFont, 1.5) + 14;
            gfx.DrawRoundedRectangle(new XSolidBrush(tagColour.Over(PdfPalette.Ivory, 0.14)),
                new XRect(textX, afterHeading + 3, tagWidth, 12.4), new XSize(6, 6));
            gfx.DrawRoundedRectangle(new XPen(tagColour.Alpha(0.55), 0.5),
                new XRect(textX, afterHeading + 3, tagWidth, 12.4), new XSize(6, 6));
            PdfText.DrawTracked(gfx, tagText, tagFont, new XSolidBrush(tagColour),
                textX + 7, afterHeading + 11.8, 1.5);

            // Keyword line in small tracked caps, beside the tag when it fits.
            var keywords = entry.Keywords.Count > 0
                ? string.Join("  ·  ", entry.Keywords.Select(k => k.ToUpperInvariant()))
                : string.Empty;

            var keywordX = textX + tagWidth + 12;
            var keywordRoom = textWidth - tagWidth - 12;
            if (!string.IsNullOrEmpty(keywords))
            {
                PdfText.DrawTrackedFitted(gfx, keywords, PdfFonts.Serif, 7.3, XFontStyle.Bold,
                    new XSolidBrush(PdfPalette.Lapis), keywordX, afterHeading + 11.8, keywordRoom, 1.3);
            }

            flow.Y = afterHeading + 26;

            // --- the prose --------------------------------------------------------------------------
            foreach (var paragraph in paragraphs)
            {
                DrawFlowedParagraph(flow, paragraph, bodyFont, new XSolidBrush(PdfPalette.Ink),
                    textX, textWidth, BodyLeading);
                flow.Advance(8);
            }

            // --- the invocation ---------------------------------------------------------------------
            if (!string.IsNullOrWhiteSpace(invocation))
            {
                flow.Advance(6);
                DrawInvocation(flow, invocation, invocationFont, textX, textWidth);
            }

            flow.Advance(12);
        }
    }

    private void DrawInvocation(PdfPageFlow flow, string text, XFont font, double x, double width)
    {
        const double indent = 20;
        var innerWidth = width - indent - 10;
        var lines = PdfText.WrapLines(flow.Gfx, text, font, innerWidth);
        var height = lines.Count * 15.0 + 16;

        // Do not strand an invocation; take it whole to the next page if it does not fit.
        flow.EnsureSpace(height);
        var gfx = flow.Gfx;
        var top = flow.Y;

        gfx.DrawRectangle(new XSolidBrush(PdfPalette.Parchment.Over(PdfPalette.Ivory, 0.62)),
            new XRect(x + indent - 10, top, width - indent + 10, height));
        gfx.DrawRectangle(new XSolidBrush(PdfPalette.GoldDeep), new XRect(x + indent - 10, top, 1.8, height));

        var baseline = top + 8 + font.GetHeight() * 0.78;
        foreach (var line in lines)
        {
            gfx.DrawString(line, font, new XSolidBrush(PdfPalette.Ink.Over(PdfPalette.Ivory, 0.88)),
                new XPoint(x + indent + 4, baseline));
            baseline += 15.0;
        }

        flow.Y = top + height;
    }

    // =============================================================================================
    // 5. Counsel and closing
    // =============================================================================================

    private void DrawCounsel(PdfPageFlow flow, ReadingInterpretation interpretation)
    {
        if (string.IsNullOrWhiteSpace(interpretation.Counsel) &&
            string.IsNullOrWhiteSpace(interpretation.ClosingInvocation))
        {
            return;
        }

        flow.BeginPage();
        DrawPageTitle(flow, "THE COUNSEL", "What the rosette asks of you");

        var bodyFont = new XFont(PdfFonts.Serif, 10.8);
        var x = flow.ContentLeft;
        var width = flow.ContentWidth;

        foreach (var paragraph in SplitParagraphs(interpretation.Counsel))
        {
            DrawFlowedParagraph(flow, paragraph, bodyFont, new XSolidBrush(PdfPalette.Ink),
                x, width, 16.4);
            flow.Advance(10);
        }

        if (string.IsNullOrWhiteSpace(interpretation.ClosingInvocation)) return;

        // Set the verse so that its own line breaks survive: shrink until the longest authored
        // line fits the measure, rather than re-wrapping it into orphans.
        var verseWidth = width - 56;
        var closingSize = 11.6;
        var closingFont = new XFont(PdfFonts.SerifLight, closingSize, XFontStyle.Italic);
        var authored = interpretation.ClosingInvocation.Replace("\r\n", "\n").Split('\n');
        for (var guard = 0; guard < 14 && closingSize > 8.6; guard++)
        {
            if (authored.All(l => flow.Gfx.MeasureString(l.Trim(), closingFont).Width <= verseWidth)) break;
            closingSize *= 0.95;
            closingFont = new XFont(PdfFonts.SerifLight, closingSize, XFontStyle.Italic);
        }

        var lines = PdfText.WrapLines(flow.Gfx, interpretation.ClosingInvocation, closingFont, verseWidth);
        var lineStep = closingFont.GetHeight() * 1.38;
        var panelHeight = lines.Count * lineStep + 74;

        flow.EnsureSpace(panelHeight + 40);
        flow.Advance(Math.Clamp((flow.Remaining - panelHeight - 56) * 0.36, 26, 130));

        var gfx = flow.Gfx;
        var top = flow.Y;
        var panel = new XRect(x, top, width, panelHeight);

        gfx.DrawRectangle(new XSolidBrush(PdfPalette.Night), panel);
        var wash = new XShadingBrush(new XPoint(panel.X, panel.Y), new XPoint(panel.X + panel.Width, panel.Y + panel.Height),
        [
            new XGradientStop(0.0, PdfPalette.Lapis.Over(PdfPalette.Night, 0.34)),
            new XGradientStop(0.6, PdfPalette.Night),
            new XGradientStop(1.0, PdfPalette.Kohl),
        ]);
        gfx.DrawRectangle(wash, panel);
        gfx.DrawRectangle(new XPen(PdfPalette.GoldDeep.Alpha(0.8), 1.0), panel);
        PdfOrnaments.CornerFlourishes(gfx, new XRect(panel.X + 7, panel.Y + 7, panel.Width - 14, panel.Height - 14),
            18, PdfPalette.Gold, 0.6);

        PdfText.DrawTracked(gfx, "THE CLOSING INVOCATION", new XFont(PdfFonts.Serif, 7.2, XFontStyle.Bold),
            new XSolidBrush(PdfPalette.GoldPale.Alpha(0.85)), panel.X, top + 28, 2.6, TrackedAlign.Center, panel.Width);

        PdfOrnaments.DoubleRule(gfx, panel.X + panel.Width / 2 - 46, panel.X + panel.Width / 2 + 46,
            top + 38, PdfPalette.GoldDeep.Alpha(0.7), 0.8, 0.35, 2.0);

        var baseline = top + 62 + closingFont.GetHeight() * 0.78;
        foreach (var line in lines)
        {
            var w = gfx.MeasureString(line, closingFont).Width;
            gfx.DrawString(line, closingFont, new XSolidBrush(PdfPalette.Ivory.Alpha(0.95)),
                new XPoint(panel.X + (panel.Width - w) / 2.0, baseline));
            baseline += lineStep;
        }

        flow.Y = top + panelHeight;

        // A terminal mark: the reading proper ends here, the appendix is reference matter.
        flow.Advance(34);
        if (flow.Remaining >= 40)
        {
            var mark = flow.Gfx;
            PdfOrnaments.VenusStar(mark, new XPoint(x + width / 2.0, flow.Y + 7), 6.5,
                PdfPalette.GoldDeep, PdfPalette.Gold, PdfPalette.GoldDeep, 0.9);
            PdfText.DrawTracked(mark, "SO ENDS THE ROSETTE",
                new XFont(PdfFonts.Serif, 6.8, XFontStyle.Bold),
                new XSolidBrush(PdfPalette.GoldShadow.Over(PdfPalette.Ivory, 0.9)),
                x, flow.Y + 28, 2.4, TrackedAlign.Center, width);
            flow.Advance(34);
        }
    }

    /// <summary>
    /// The one line every reading ends with, set on its own immediately before the appendix: the
    /// last words of the interpretation proper, so that whatever came before it — a counsel page,
    /// a closing invocation panel, or nothing at all when neither was written — the report closes
    /// the same way. A small gold rule separates it from the section above, as elsewhere.
    /// </summary>
    private static void DrawClosingBlessing(PdfPageFlow flow, ReadingInterpretation interpretation)
    {
        var blessing = interpretation.Closing?.Trim();
        if (string.IsNullOrWhiteSpace(blessing)) return;

        // A page has always been begun by now; this only matters if the sections above it all
        // declined to draw, and it is what gives the measuring below a graphics context.
        flow.EnsureSpace(1);

        const double leading = 17.0;
        const double inset = 36;
        var font = new XFont(PdfFonts.SerifLight, 11.0, XFontStyle.Italic);
        var width = flow.ContentWidth;
        var textWidth = width - inset * 2;
        var lines = PdfText.WrapLines(flow.Gfx, blessing, font, textWidth);

        // The rule, the air above and below it, and the line itself, taken whole to a fresh page
        // rather than split from the ornament that introduces it.
        const double ruleBlock = 26;
        flow.EnsureSpace(ruleBlock + lines.Count * leading + 10);

        var gfx = flow.Gfx;
        var x = flow.ContentLeft;

        flow.Advance(10);
        PdfOrnaments.Divider(gfx, x + width / 2.0, flow.Y + 6, width * 0.34,
            PdfPalette.GoldDeep.Alpha(0.85));
        flow.Advance(ruleBlock);

        PdfText.DrawWrapped(gfx, blessing, font, new XSolidBrush(PdfPalette.GoldShadow),
            x + inset, flow.Y, textWidth, leading, TrackedAlign.Center);
        flow.Advance(lines.Count * leading + 10);
    }

    // =============================================================================================
    // 6. Appendix
    // =============================================================================================

    private void DrawAppendix(PdfPageFlow flow, ReadingInterpretation interpretation)
    {
        var cards = interpretation.Reading.Placements
            .OrderBy(p => p.Position.Index)
            .Select(p => p.Card)
            .DistinctBy(c => c.Id)
            .ToList();

        if (cards.Count == 0) return;

        flow.BeginPage();
        DrawPageTitle(flow, "APPENDIX", "The cards in this reading");

        var bodyFont = new XFont(PdfFonts.Serif, 9.8);
        var nameFont = new XFont(PdfFonts.Serif, 12.2, XFontStyle.Bold);


        const double discColumn = 46;
        var textX = flow.ContentLeft + discColumn;
        var textWidth = flow.ContentWidth - discColumn;

        foreach (var card in cards)
        {
            var loreHeight = PdfText.MeasureParagraph(flow.Gfx, card.Lore, bodyFont, textWidth, 14.6);
            flow.EnsureSpace(54 + Math.Min(loreHeight, 14.6 * 5));

            var gfx = flow.Gfx;
            var top = flow.Y;

            // A small medallion of the card's emblem in the gutter.
            var accent = PdfPalette.Parse(card.AccentColor, PdfPalette.Gold);
            var secondary = PdfPalette.Parse(card.SecondaryColor, PdfPalette.Lapis);
            var discCenter = new XPoint(flow.ContentLeft + 16, top + 15);
            const double discR = 15;
            var disc = new XShadingBrush(discCenter, 0, discCenter, discR,
            [
                new XGradientStop(0.0, secondary.Over(PdfPalette.Night, 0.9)),
                new XGradientStop(0.6, secondary.Over(PdfPalette.Night, 0.5)),
                new XGradientStop(1.0, PdfPalette.Night),
            ]);
            var discRect = new XRect(discCenter.X - discR, discCenter.Y - discR, discR * 2, discR * 2);
            gfx.DrawEllipse(disc, discRect);
            gfx.DrawEllipse(new XPen(PdfPalette.GoldDeep.Alpha(0.85), 0.8), discRect);
            PdfCardPainter.DrawEmblem(gfx, card.Emblem,
                new XRect(discCenter.X - discR * 0.72, discCenter.Y - discR * 0.72, discR * 1.44, discR * 1.44),
                accent, secondary);

            // Name and epithet.
            gfx.DrawString(card.Name, nameFont, new XSolidBrush(PdfPalette.Ink), new XPoint(textX, top + 12));
            var nameWidth = gfx.MeasureString(card.Name, nameFont).Width;
            if (!string.IsNullOrWhiteSpace(card.Epithet) && nameWidth + 10 < textWidth)
            {
                PdfText.DrawFitted(gfx, $"·  {card.Epithet}", PdfFonts.SerifLight, 9.4, XFontStyle.Italic,
                    new XSolidBrush(PdfPalette.Ink.Over(PdfPalette.Ivory, 0.55)),
                    textX + nameWidth + 8, top + 12, textWidth - nameWidth - 8);
            }

            // Numeral and suit on one tracked-caps line, the domains on the next, so that neither
            // has to be squeezed down to an unreadable size.
            PdfText.DrawTrackedFitted(gfx,
                $"{card.NumeralLabel.ToUpperInvariant()}  ·  {card.SuitName.ToUpperInvariant()}",
                PdfFonts.Serif, 6.9, XFontStyle.Bold,
                new XSolidBrush(PdfPalette.GoldShadow), textX, top + 26, textWidth, 1.6);

            var afterMeta = top + 26;
            if (card.Domains.Count > 0)
            {
                PdfText.DrawTrackedFitted(gfx,
                    string.Join("  ·  ", card.Domains.Select(d => d.ToUpperInvariant())),
                    PdfFonts.Serif, 6.5, XFontStyle.Bold,
                    new XSolidBrush(PdfPalette.Lapis.Over(PdfPalette.Ivory, 0.85)),
                    textX, top + 37, textWidth, 1.4);
                afterMeta = top + 37;
            }

            gfx.DrawLine(new XPen(PdfPalette.GoldDeep.Alpha(0.4), 0.5),
                new XPoint(textX, afterMeta + 6), new XPoint(flow.ContentRight, afterMeta + 6));

            flow.Y = afterMeta + 14;
            DrawFlowedParagraph(flow, card.Lore, bodyFont,
                new XSolidBrush(PdfPalette.Ink.Over(PdfPalette.Ivory, 0.93)), textX, textWidth, 14.6);
            flow.Advance(18);
        }
    }

    // =============================================================================================
    // Shared pieces
    // =============================================================================================

    private void DrawPageTitle(PdfPageFlow flow, string kicker, string subtitle)
    {
        var gfx = flow.Gfx;
        var x = flow.ContentLeft;
        var width = flow.ContentWidth;

        PdfText.DrawTracked(gfx, kicker, new XFont(PdfFonts.Serif, 13.5, XFontStyle.Bold),
            new XSolidBrush(PdfPalette.Ink), x, flow.Y + 12, 4.6);

        var used = 24.0;
        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            var font = new XFont(PdfFonts.SerifLight, 9.6, XFontStyle.Italic);
            used += PdfText.DrawWrapped(gfx, subtitle, font,
                new XSolidBrush(PdfPalette.Ink.Over(PdfPalette.Ivory, 0.58)),
                x, flow.Y + 20, width * 0.8, 13.6);
            used += 4;
        }

        PdfOrnaments.DoubleRule(gfx, x, flow.ContentRight, flow.Y + used + 6, PdfPalette.GoldDeep.Alpha(0.85));
        flow.Advance(used + 22);
    }

    private void DrawSectionHeading(PdfPageFlow flow, string kicker, string title)
    {
        flow.EnsureSpace(46);
        var gfx = flow.Gfx;
        var x = flow.ContentLeft;

        PdfText.DrawTracked(gfx, kicker, new XFont(PdfFonts.Serif, KickerSize, XFontStyle.Bold),
            new XSolidBrush(PdfPalette.GoldShadow), x, flow.Y + 8, KickerTracking);

        gfx.DrawString(title, new XFont(PdfFonts.Serif, 14.5, XFontStyle.Bold),
            new XSolidBrush(PdfPalette.Ink), new XPoint(x, flow.Y + 27));

        gfx.DrawRectangle(new XSolidBrush(PdfPalette.GoldDeep), new XRect(x, flow.Y + 34, 34, 1.6));
        flow.Advance(48);
    }

    /// <summary>
    /// Draws a justified paragraph starting at the flow's cursor, breaking it across pages line by
    /// line so that no text is ever clipped at the foot of a page, and leaves the cursor directly
    /// beneath the last line drawn.
    /// </summary>
    private static void DrawFlowedParagraph(
        PdfPageFlow flow, string text, XFont font, XBrush brush, double x, double width, double lineHeight)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var remainder = text;
        var consumedOnCurrentPage = 0.0;

        while (true)
        {
            var gfx = flow.Gfx;
            var available = flow.ContentBottom - (flow.Y + consumedOnCurrentPage);

            // Never leave a single widow line behind; move the whole tail instead.
            if (available < lineHeight * 2)
            {
                flow.Y += consumedOnCurrentPage;
                flow.BeginPage();
                consumedOnCurrentPage = 0;
                continue;
            }

            var maxLines = (int)Math.Floor(available / lineHeight);
            var lines = PdfText.WrapLines(gfx, remainder, font, width);

            if (lines.Count <= maxLines)
            {
                var drawn = PdfText.DrawJustified(gfx, remainder, font, brush,
                    x, flow.Y + consumedOnCurrentPage, width, lineHeight);
                flow.Y += consumedOnCurrentPage + drawn;
                return;
            }

            // Split: draw what fits (minus an orphan guard), carry the rest to the next page.
            // Leave at least two lines on this page and carry at least three to the next, so a
            // paragraph never ends a page - or opens one - with a single stranded line.
            var minTail = lines.Count >= 5 ? 3 : 2;
            var take = Math.Max(2, maxLines - (lines.Count - maxLines == 1 ? 1 : 0));
            take = Math.Min(take, lines.Count - minTail);
            if (take < 2)
            {
                flow.Y += consumedOnCurrentPage;
                flow.BeginPage();
                consumedOnCurrentPage = 0;
                continue;
            }

            var head = string.Join(' ', lines.Take(take));
            var tail = string.Join(' ', lines.Skip(take));

            PdfText.DrawJustifiedFull(gfx, head, font, brush, x, flow.Y + consumedOnCurrentPage, width, lineHeight);

            flow.Y = flow.ContentBottom;
            flow.BeginPage();
            consumedOnCurrentPage = 0;
            remainder = tail;
        }
    }

    private static IEnumerable<string> SplitParagraphs(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) yield break;
        var blocks = text.Replace("\r\n", "\n").Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        foreach (var block in blocks)
        {
            var cleaned = block.Replace('\n', ' ').Trim();
            if (cleaned.Length > 0) yield return cleaned;
        }
    }

    /// <summary>Loose containment test, ignoring case, punctuation and spacing.</summary>
    private static bool Contains(string? haystack, string? needle)
    {
        var h = Fold(haystack);
        var n = Fold(needle);
        return n.Length > 0 && h.Contains(n, StringComparison.Ordinal);
    }

    private static string Fold(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
        }

        return sb.ToString();
    }

    /// <summary>Removes a trailing "(Upright)" or "(Reversed)"; the orientation chip says it already.</summary>
    private static string StripOrientation(string heading)
    {
        var trimmed = heading.TrimEnd();
        foreach (var tail in new[] { "(Upright)", "(Reversed)", "(upright)", "(reversed)" })
        {
            if (trimmed.EndsWith(tail, StringComparison.Ordinal))
            {
                return trimmed[..^tail.Length].TrimEnd(' ', '\u2014', '-', ',');
            }
        }

        return trimmed;
    }

    private static string Display(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    /// <summary>Reduces a name to the safe, readable core of a file name.</summary>
    private static string Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var normalized = value.Trim().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        var lastWasSeparator = false;

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark) continue;

            if (char.IsLetterOrDigit(ch) && ch < 128)
            {
                sb.Append(ch);
                lastWasSeparator = false;
            }
            else if (!lastWasSeparator && sb.Length > 0)
            {
                sb.Append('-');
                lastWasSeparator = true;
            }
        }

        var result = sb.ToString().Trim('-');
        return result.Length > 48 ? result[..48].TrimEnd('-') : result;
    }
}
