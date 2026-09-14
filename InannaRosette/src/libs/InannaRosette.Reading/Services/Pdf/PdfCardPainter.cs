using CodeBrix.PdfDocuments.Drawing;
using InannaRosette.Reading.Data;
using InannaRosette.Reading.Models;

namespace InannaRosette.Reading.Services.Pdf;

/// <summary>How much of the card face to render; the smaller sizes drop detail rather than shrink it.</summary>
public enum CardDetail
{
    /// <summary>Numeral band, medallion, name and foot ornament - the spread-diagram card.</summary>
    Full,
    /// <summary>Numeral band and medallion only - the margin thumbnail beside a station heading.</summary>
    Compact,
}

/// <summary>
/// Draws the deck's card design into a PDF. The UI and the PDF share one design: gold double
/// border, corner flourishes, a numeral band, a medallion holding the vector emblem, and the card
/// name beneath it. The card ratio is 5:8.
/// </summary>
public static class PdfCardPainter
{
    /// <summary>Width of a card of the given height, at the deck's 5:8 ratio.</summary>
    public const double AspectRatio = 5.0 / 8.0;

    /// <summary>Returns the card rectangle of the given height, centred on <paramref name="center"/>.</summary>
    public static XRect RectFor(XPoint center, double height)
    {
        var width = height * AspectRatio;
        return new XRect(center.X - width / 2.0, center.Y - height / 2.0, width, height);
    }

    /// <summary>
    /// Draws one card face inside <paramref name="rect"/>. A reversed card is rotated a half turn
    /// about its own centre, exactly as it lies on the table.
    /// </summary>
    public static void DrawCard(
        XGraphics gfx, XRect rect, Card card, bool isReversed,
        CardDetail detail = CardDetail.Full, bool showReversedMark = false)
    {
        var state = gfx.Save();
        if (isReversed)
        {
            gfx.RotateAtTransform(180, new XPoint(rect.X + rect.Width / 2.0, rect.Y + rect.Height / 2.0));
        }

        DrawFace(gfx, rect, card, detail);
        gfx.Restore(state);

        if (isReversed && showReversedMark) DrawReversedMark(gfx, rect);
    }

    /// <summary>The face-down card back: Venus star on night, gold frame, ring of eight rosettes.</summary>
    public static void DrawCardBack(XGraphics gfx, XRect rect)
    {
        var h = rect.Height;
        var radius = h * 0.035;

        gfx.DrawRoundedRectangle(new XSolidBrush(PdfPalette.Night), rect, new XSize(radius * 2, radius * 2));

        var frame = Inset(rect, h * 0.045);
        gfx.DrawRoundedRectangle(new XPen(PdfPalette.GoldDeep, h * 0.012), frame, new XSize(radius * 2, radius * 2));

        var center = new XPoint(rect.X + rect.Width / 2.0, rect.Y + h * 0.44);
        var discR = h * 0.20;
        gfx.DrawEllipse(new XSolidBrush(PdfPalette.Lapis), new XRect(center.X - discR, center.Y - discR, discR * 2, discR * 2));
        PdfOrnaments.VenusStar(gfx, center, h * 0.255, PdfPalette.Gold, PdfPalette.GoldPale, PdfPalette.GoldDeep);

        var ringR = h * 0.345;
        for (var i = 0; i < 8; i++)
        {
            var a = Math.PI * 2 * i / 8 + Math.PI / 8;
            var p = new XPoint(center.X + Math.Sin(a) * ringR, center.Y - Math.Cos(a) * ringR);
            PdfOrnaments.Rosette(gfx, p, h * 0.03, PdfPalette.GoldDeep, PdfPalette.Gold, PdfPalette.GoldDeep);
        }

        var tiny = new XFont(PdfFonts.Serif, Math.Max(3.4, h * 0.042), XFontStyle.Bold);
        PdfText.DrawTracked(gfx, "ROSETTE OF INANNA", tiny, new XSolidBrush(PdfPalette.Gold.Alpha(0.8)),
            rect.X, rect.Y + h * 0.895, h * 0.012, TrackedAlign.Center, rect.Width);
    }

    /// <summary>An empty station: a dashed gold slot outline with a faint rosette watermark.</summary>
    public static void DrawEmptySlot(XGraphics gfx, XRect rect)
    {
        var radius = rect.Height * 0.035;
        gfx.DrawRoundedRectangle(
            new XSolidBrush(PdfPalette.Parchment.Over(PdfPalette.Ivory, 0.55)),
            rect, new XSize(radius * 2, radius * 2));

        var pen = new XPen(PdfPalette.GoldDeep.Alpha(0.55), 0.9)
        {
            DashStyle = XDashStyle.Custom,
            DashPattern = [3.2, 2.6],
            LineCap = XLineCap.Round,
        };
        gfx.DrawRoundedRectangle(pen, rect, new XSize(radius * 2, radius * 2));

        var center = new XPoint(rect.X + rect.Width / 2.0, rect.Y + rect.Height * 0.44);
        PdfOrnaments.Rosette(gfx, center, rect.Height * 0.17,
            PdfPalette.GoldShadow, PdfPalette.GoldDeep, PdfPalette.GoldShadow, 0.26);

        var font = new XFont(PdfFonts.SerifLight, Math.Max(4.0, rect.Height * 0.055));
        PdfText.DrawTracked(gfx, "NOT LAID", font, new XSolidBrush(PdfPalette.GoldShadow.Alpha(0.6)),
            rect.X, rect.Y + rect.Height * 0.74, rect.Height * 0.016, TrackedAlign.Center, rect.Width);
    }

    // -----------------------------------------------------------------------------------------

    private static void DrawFace(XGraphics gfx, XRect rect, Card card, CardDetail detail)
    {
        var h = rect.Height;
        var w = rect.Width;
        var cx = rect.X + w / 2.0;
        var corner = h * 0.035;
        var ellipse = new XSize(corner * 2, corner * 2);

        var accent = PdfPalette.Parse(card.AccentColor, PdfPalette.Gold).EnsureReadableOnDark();
        var secondary = PdfPalette.Parse(card.SecondaryColor, PdfPalette.Lapis);

        // Body: Night2 with a soft lapis glow falling from the top edge.
        // DrawRoundedRectangle does not honour a shading brush, so lay a solid ground first and
        // paint the gradient through a rounded-rectangle clip, which DrawRectangle does honour.
        gfx.DrawRoundedRectangle(new XSolidBrush(PdfPalette.Night2), rect, ellipse);

        var glowCenter = new XPoint(cx, rect.Y + h * 0.06);
        var body = new XShadingBrush(glowCenter, 0, glowCenter, h * 0.98,
        [
            new XGradientStop(0.0, PdfPalette.LapisLight.Over(PdfPalette.Night2, 0.52)),
            new XGradientStop(0.40, PdfPalette.Lapis.Over(PdfPalette.Night2, 0.26)),
            new XGradientStop(1.0, PdfPalette.Night),
        ]);

        var clipState = gfx.Save();
        var clip = new XGraphicsPath();
        clip.AddRoundedRectangle(rect.X, rect.Y, rect.Width, rect.Height, corner * 2, corner * 2);
        gfx.IntersectClip(clip);
        gfx.DrawRectangle(body, rect);
        gfx.Restore(clipState);

        // Outer gold double border: a thick outer rule and a hairline inner one.
        gfx.DrawRoundedRectangle(new XPen(PdfPalette.GoldDeep, h * 0.0155), Inset(rect, h * 0.011), ellipse);
        gfx.DrawRoundedRectangle(new XPen(PdfPalette.Gold.Alpha(0.72), h * 0.005), Inset(rect, h * 0.052),
            new XSize(corner * 1.4, corner * 1.4));

        // Corner flourishes inside the hairline.
        PdfOrnaments.CornerFlourishes(gfx, Inset(rect, h * 0.068), h * 0.135, PdfPalette.Gold, 0.5);

        var inner = Inset(rect, h * 0.085);

        // Numeral band.
        PdfText.DrawTrackedFitted(gfx, card.NumeralLabel.ToUpperInvariant(), PdfFonts.Serif,
            Math.Max(3.6, h * 0.052), XFontStyle.Bold, new XSolidBrush(PdfPalette.GoldPale),
            inner.X, rect.Y + h * 0.148, inner.Width, h * 0.016, TrackedAlign.Center);

        gfx.DrawLine(new XPen(PdfPalette.Gold.Alpha(0.42), h * 0.004),
            new XPoint(inner.X + inner.Width * 0.18, rect.Y + h * 0.185),
            new XPoint(inner.X + inner.Width * 0.82, rect.Y + h * 0.185));

        // Medallion: radial gradient from the card's secondary colour to Night, inside a gold ring.
        var medCenter = new XPoint(cx, rect.Y + h * (detail == CardDetail.Full ? 0.435 : 0.52));
        var medRadius = h * (detail == CardDetail.Full ? 0.185 : 0.225);
        var medRect = new XRect(medCenter.X - medRadius, medCenter.Y - medRadius, medRadius * 2, medRadius * 2);

        var medallion = new XShadingBrush(medCenter, 0, medCenter, medRadius,
        [
            new XGradientStop(0.0, secondary.Over(PdfPalette.Night, 0.92)),
            new XGradientStop(0.55, secondary.Over(PdfPalette.Night, 0.55)),
            new XGradientStop(1.0, PdfPalette.Night),
        ]);
        gfx.DrawEllipse(medallion, medRect);
        gfx.DrawEllipse(new XPen(PdfPalette.Gold.Alpha(0.85), h * 0.008), medRect);
        var halo = medRadius * 1.1;
        gfx.DrawEllipse(new XPen(PdfPalette.GoldDeep.Alpha(0.3), h * 0.0045),
            new XRect(medCenter.X - halo, medCenter.Y - halo, halo * 2, halo * 2));

        // The emblem itself, inside the medallion.
        DrawEmblem(gfx, card.Emblem, new XRect(
                medCenter.X - medRadius * 0.76, medCenter.Y - medRadius * 0.76,
                medRadius * 1.52, medRadius * 1.52),
            accent, secondary);

        if (detail != CardDetail.Full) return;

        // Card name in gold, up to two lines, then the epithet if there is room.
        var nameTop = rect.Y + h * 0.655;
        var (nameFont, nameLines) = PdfText.FitLines(
            gfx, card.Name, PdfFonts.Serif, h * 0.077, XFontStyle.Bold,
            inner.Width, 2, h * 0.052);

        var lineHeight = nameFont.GetHeight() * 0.95;
        var baseline = nameTop + nameFont.GetHeight() * 0.78;
        foreach (var line in nameLines)
        {
            var lw = gfx.MeasureString(line, nameFont).Width;
            gfx.DrawString(line, nameFont, new XSolidBrush(PdfPalette.Gold),
                new XPoint(inner.X + (inner.Width - lw) / 2.0, baseline));
            baseline += lineHeight;
        }

        var afterName = nameTop + nameLines.Count * lineHeight;
        var epithetRoom = rect.Y + h * 0.90 - afterName;
        var epithetSize = h * 0.055;
        if (h >= 108 && epithetRoom >= epithetSize * 1.15 && !string.IsNullOrWhiteSpace(card.Epithet))
        {
            var (epFont, epLines) = PdfText.FitLines(
                gfx, card.Epithet, PdfFonts.SerifLight, epithetSize, XFontStyle.Italic,
                inner.Width, 1, h * 0.04);
            var lw = gfx.MeasureString(epLines[0], epFont).Width;
            gfx.DrawString(epLines[0], epFont, new XSolidBrush(PdfPalette.Ivory.Alpha(0.62)),
                new XPoint(inner.X + (inner.Width - lw) / 2.0, afterName + epFont.GetHeight() * 0.8));
        }

        PdfOrnaments.DotCluster(gfx, cx, rect.Y + h * 0.945, h * 0.016,
            new XSolidBrush(PdfPalette.GoldDeep.Alpha(0.85)));
    }

    /// <summary>
    /// Renders every layer of an emblem: filled layers in the card's accent colour, layers marked
    /// as accents in its secondary colour, stroked layers in gold, each at the layer's own opacity.
    /// </summary>
    public static void DrawEmblem(XGraphics gfx, Emblem emblem, XRect box, XColor accent, XColor secondary)
    {
        PdfOrnaments.DrawLayers(
            gfx,
            EmblemArt.Layers(emblem),
            box,
            main: accent.EnsureReadableOnDark(),
            accent: secondary.EnsureReadableOnDark(0.42),
            stroke: PdfPalette.Gold);
    }

    /// <summary>A small inverted triangle beside the card, marking it as reversed.</summary>
    private static void DrawReversedMark(XGraphics gfx, XRect rect)
    {
        var size = Math.Max(3.0, rect.Height * 0.048);
        var cx = rect.X + rect.Width - size * 0.9;
        var cy = rect.Y + rect.Height + size * 1.15;

        var path = new XGraphicsPath();
        path.AddPolygon(
        [
            new XPoint(cx - size, cy - size * 0.82),
            new XPoint(cx + size, cy - size * 0.82),
            new XPoint(cx, cy + size * 0.82),
        ]);
        path.CloseFigure();
        gfx.DrawPath(new XPen(PdfPalette.GoldShadow.Alpha(0.8), 0.5), new XSolidBrush(PdfPalette.Carnelian), path);
    }

    private static XRect Inset(XRect r, double amount) =>
        new(r.X + amount, r.Y + amount, Math.Max(0.1, r.Width - amount * 2), Math.Max(0.1, r.Height - amount * 2));
}
