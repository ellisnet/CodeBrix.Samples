using CodeBrix.PdfDocuments.Drawing;
using InannaOctaquetra.Reading.Data;

namespace InannaOctaquetra.Reading.Services.Pdf;

/// <summary>
/// The ornamental vocabulary of the report: the Venus star, Octaquetras, corner flourishes,
/// double rules and cuneiform-like wedges. All vector, all drawn from the same art the UI uses.
/// </summary>
public static class PdfOrnaments
{
    /// <summary>
    /// Fraction of the 0..100 design box that the artwork actually spans (the emblems are drawn
    /// inside a circle of radius ~38 about the centre). Used to turn a wanted visual radius into
    /// the box the paths must be mapped onto.
    /// </summary>
    private const double ArtSpan = 0.38;

    /// <summary>
    /// Draws a stack of <see cref="EmblemLayer"/>s into <paramref name="box"/>.
    /// Layers marked as accents take <paramref name="accent"/>; other filled layers take
    /// <paramref name="main"/>; other stroked layers take <paramref name="stroke"/>.
    /// </summary>
    public static void DrawLayers(
        XGraphics gfx,
        IReadOnlyList<EmblemLayer> layers,
        XRect box,
        XColor main,
        XColor accent,
        XColor stroke,
        double opacity = 1.0,
        double designBox = SvgPathToPdf.DefaultDesignBox)
    {
        if (layers.Count == 0) return;
        var scale = Math.Min(box.Width, box.Height) / designBox;

        foreach (var layer in layers)
        {
            if (string.IsNullOrWhiteSpace(layer.PathData)) continue;

            XGraphicsPath path;
            try
            {
                path = SvgPathToPdf.Build(layer.PathData, box, designBox);
            }
            catch (FormatException)
            {
                continue;   // one malformed glyph must never take the whole report down
            }

            var layerOpacity = Math.Clamp(layer.Opacity, 0, 1) * Math.Clamp(opacity, 0, 1);
            if (layerOpacity <= 0) continue;

            if (layer.Filled)
            {
                var colour = layer.Accent ? accent : main;
                gfx.DrawPath(new XSolidBrush(colour.Alpha(layerOpacity)), path);
            }
            else
            {
                var colour = layer.Accent ? accent : stroke;
                var width = Math.Max(0.22, layer.StrokeWidth * scale);
                gfx.DrawPath(new XPen(colour.Alpha(layerOpacity), width)
                {
                    LineCap = XLineCap.Round,
                    LineJoin = XLineJoin.Round,
                }, path);
            }
        }
    }

    private static XRect BoxFor(XPoint center, double visualRadius)
    {
        var side = visualRadius / ArtSpan;
        return new XRect(center.X - side / 2.0, center.Y - side / 2.0, side, side);
    }

    /// <summary>
    /// Draws the eight-pointed Venus star so that its points reach <paramref name="radius"/>
    /// from <paramref name="center"/>.
    /// </summary>
    public static void VenusStar(
        XGraphics gfx, XPoint center, double radius,
        XColor main, XColor? accent = null, XColor? stroke = null, double opacity = 1.0)
        => DrawLayers(gfx, EmblemArt.VenusStar, BoxFor(center, radius),
            main, accent ?? main, stroke ?? main, opacity);

    /// <summary>The gold leaf of the Octaquetra of Inanna, top to bottom, as in the reference artwork.</summary>
    private static readonly (double Offset, XColor Colour)[] GoldLeafStops =
    [
        (0.00, PdfPalette.Rgb(0xF7, 0xE2, 0x7E)),
        (0.40, PdfPalette.Rgb(0xE8, 0xC2, 0x4E)),
        (0.75, PdfPalette.Rgb(0xC1, 0x8F, 0x2A)),
        (1.00, PdfPalette.Rgb(0x7E, 0x4E, 0x10)),
    ];

    private static XColor GoldLeafAt(double t)
    {
        t = Math.Clamp(t, 0, 1);
        for (var i = 1; i < GoldLeafStops.Length; i++)
        {
            var (o1, c1) = GoldLeafStops[i - 1];
            var (o2, c2) = GoldLeafStops[i];
            if (t <= o2)
            {
                var k = (t - o1) / (o2 - o1);
                return c2.Over(c1, k);
            }
        }
        return GoldLeafStops[^1].Colour;
    }

    /// <summary>
    /// Draws the Octaquetra of Inanna, the authored artwork filled, with its outer edge reaching
    /// <paramref name="radius"/>. In gold leaf (pale gold above, bronze below) unless a
    /// <paramref name="solid"/> colour is given. PDF brushes here take one colour per fill, so the
    /// gradient is laid down as horizontal bands, each clipped and filled in its own shade. Over a
    /// known <paramref name="background"/>, translucency is pre-blended so bands overlap without seams.
    /// <paramref name="outline"/> traces the edge once more to thicken very small Octaquetras.
    /// </summary>
    public static void InannaOctaquetra(
        XGraphics gfx, XPoint center, double radius, double outline = 0,
        double opacity = 1.0, XColor? solid = null, XColor? background = null)
    {
        if (radius <= 0) return;
        XGraphicsPath path;
        try
        {
            path = SvgPathToPdf.Build(EmblemArt.InannaOctaquetraPath(center.X, center.Y, radius),
                new XRect(0, 0, 100, 100), 100);
            path.FillMode = XFillMode.Alternate;
        }
        catch (FormatException)
        {
            return;
        }

        var alpha = Math.Clamp(opacity, 0, 1);
        XColor Shade(XColor colour) => background is XColor ground ? colour.Over(ground, alpha) : colour.Alpha(alpha);
        void Paint(XColor colour)
        {
            var shade = Shade(colour);
            if (outline > 0)
            {
                gfx.DrawPath(new XPen(shade, outline) { LineJoin = XLineJoin.Round }, new XSolidBrush(shade), path);
            }
            else
            {
                gfx.DrawPath(new XSolidBrush(shade), path);
            }
        }

        var opaque = background is not null || alpha >= 0.999;
        var bands = solid is null && radius >= 10 ? Math.Clamp((int)(radius / 1.2), 10, 90) : 1;
        if (bands == 1)
        {
            Paint(solid ?? GoldLeafAt(0.5));
            return;
        }

        var reach = radius + outline;
        var top = center.Y - reach;
        var height = reach * 2 / bands;
        for (var b = 0; b < bands; b++)
        {
            var state = gfx.Save();
            var overlap = opaque && b < bands - 1 ? 0.8 : 0;
            gfx.IntersectClip(new XRect(center.X - reach, top + b * height, reach * 2, height + overlap));
            Paint(GoldLeafAt((b + 0.5) / bands));
            gfx.Restore(state);
        }
    }

    /// <summary>
    /// Draws the Octaquetra of Inanna so that its outer edge reaches <paramref name="radius"/>, tracing
    /// the outline a little when it is tiny so it never dissolves into a smudge.
    /// Gold leaf by default; pass <paramref name="goldLeaf"/> false to draw it in <paramref name="main"/>.
    /// </summary>
    public static void Octaquetra(
        XGraphics gfx, XPoint center, double radius,
        XColor main, XColor? accent = null, XColor? stroke = null, double opacity = 1.0, bool goldLeaf = true)
    {
        var outline = radius < 12 ? radius * 0.07 : 0;
        InannaOctaquetra(gfx, center, radius - outline / 2, outline, opacity, goldLeaf ? null : main);
    }

    // ==================================================================== velvet and sparkle

    [ThreadStatic] private static XImage? _velvet;
    [ThreadStatic] private static string? _velvetOwner;

    /// <summary>
    /// Lays royal purple crushed velvet into <paramref name="rect"/>, cover-fitted and clipped, then
    /// deepens it by <paramref name="shade"/> (0 = as woven, 1 = black). Falls back to a solid
    /// night purple if the embedded texture cannot be read.
    /// </summary>
    public static void Velvet(XGraphics gfx, XRect rect, double shade = 0.25, double offsetY = 0)
    {
        XImage? image = null;
        try
        {
            if (ArtAssets.VelvetPrintJpeg is { Length: > 0 } bytes)
            {
                // one XImage per document, so the texture is embedded once and referenced by every page
                var owner = gfx.PdfPage?.Owner?.GetHashCode().ToString() ?? "";
                if (_velvet is null || _velvetOwner != owner)
                {
                    _velvet = XImage.FromStream(() => new MemoryStream(bytes, writable: false));
                    _velvetOwner = owner;
                }
                image = _velvet;
            }
        }
        catch (Exception)
        {
            image = null;
        }

        var state = gfx.Save();
        gfx.IntersectClip(rect);
        if (image is not null && image.PixelWidth > 0)
        {
            var aspect = (double)image.PixelHeight / image.PixelWidth;
            var width = Math.Max(rect.Width, rect.Height / aspect);
            var height = width * aspect;
            // offsetY slides a different stretch of the fabric into view, always covering the rect
            var slide = height > rect.Height ? ((offsetY % (height - rect.Height)) + (height - rect.Height)) % (height - rect.Height) : 0;
            gfx.DrawImage(image, rect.X + (rect.Width - width) / 2, rect.Y - slide, width, height);
        }
        else
        {
            gfx.DrawRectangle(new XSolidBrush(PdfPalette.Night2), rect);
        }
        if (shade > 0)
        {
            gfx.DrawRectangle(new XSolidBrush(PdfPalette.Kohl.Alpha(Math.Clamp(shade, 0, 1))), rect);
        }
        gfx.Restore(state);
    }

    /// <summary>
    /// A four-pointed sparkle, the same star the app's shuffle and starfield scatter, with a soft
    /// halo. <paramref name="radius"/> is the reach of its points.
    /// </summary>
    public static void Sparkle(XGraphics gfx, XPoint center, double radius, XColor colour, double opacity = 1.0, double rotation = 0)
    {
        if (radius <= 0) return;
        var alpha = Math.Clamp(opacity, 0, 1);
        for (var i = 3; i >= 1; i--)
        {
            var r = radius * 0.28 * i;
            gfx.DrawEllipse(new XSolidBrush(colour.Alpha(alpha * 0.07)),
                new XRect(center.X - r, center.Y - r, r * 2, r * 2));
        }

        var state = gfx.Save();
        gfx.TranslateTransform(center.X, center.Y);
        if (rotation != 0) gfx.RotateTransform(rotation);
        var k = radius / 11.0;
        XPoint P(double x, double y) => new(x * k, y * k);
        var path = new XGraphicsPath();
        path.AddBezier(P(0, -11), P(1.1, -2.4), P(2.4, -1.1), P(11, 0));
        path.AddBezier(P(11, 0), P(2.4, 1.1), P(1.1, 2.4), P(0, 11));
        path.AddBezier(P(0, 11), P(-1.1, 2.4), P(-2.4, 1.1), P(-11, 0));
        path.AddBezier(P(-11, 0), P(-2.4, -1.1), P(-1.1, -2.4), P(0, -11));
        path.CloseFigure();
        gfx.DrawPath(new XSolidBrush(colour.Alpha(alpha)), path);
        gfx.Restore(state);
    }

    /// <summary>
    /// A deterministic scatter of pinpoint stars and sparkles over <paramref name="area"/>, skipping
    /// anything inside <paramref name="keepClear"/>. Gold on parchment, pale gold and lavender on velvet.
    /// </summary>
    public static void StarScatter(XGraphics gfx, XRect area, int seed, int stars, int sparkles,
                                   bool onVelvet, IReadOnlyList<XRect>? keepClear = null)
    {
        var rng = new Random(seed);
        bool Clear(double x, double y, double pad)
        {
            if (keepClear is null) return true;
            foreach (var r in keepClear)
            {
                if (x > r.X - pad && x < r.X + r.Width + pad && y > r.Y - pad && y < r.Y + r.Height + pad) return false;
            }
            return true;
        }

        var dotColours = onVelvet
            ? new[] { PdfPalette.GoldPale, PdfPalette.Rgb(0xE7, 0xD2, 0xFF), PdfPalette.Rgb(0xFF, 0xF8, 0xE6) }
            : new[] { PdfPalette.GoldDeep, PdfPalette.Gold, PdfPalette.Rgb(0x9B, 0x6B, 0xC9) };
        for (var i = 0; i < stars; i++)
        {
            var x = area.X + rng.NextDouble() * area.Width;
            var y = area.Y + rng.NextDouble() * area.Height;
            var r = 0.35 + Math.Pow(rng.NextDouble(), 2) * 0.9;
            var alpha = (onVelvet ? 0.35 : 0.22) + rng.NextDouble() * 0.45;
            if (!Clear(x, y, 3)) continue;
            gfx.DrawEllipse(new XSolidBrush(dotColours[rng.Next(dotColours.Length)].Alpha(alpha)),
                new XRect(x - r, y - r, r * 2, r * 2));
        }

        var sparkleColours = onVelvet
            ? new[] { PdfPalette.GoldPale, PdfPalette.Rgb(0xFF, 0xF3, 0xC4), PdfPalette.Rgb(0xE7, 0xD2, 0xFF) }
            : new[] { PdfPalette.GoldDeep, PdfPalette.Gold, PdfPalette.Rgb(0x8E, 0x5B, 0xC4) };
        for (var i = 0; i < sparkles; i++)
        {
            var x = area.X + rng.NextDouble() * area.Width;
            var y = area.Y + rng.NextDouble() * area.Height;
            var radius = 2.2 + Math.Pow(rng.NextDouble(), 2) * 5.5;
            if (!Clear(x, y, radius + 3)) continue;
            Sparkle(gfx, new XPoint(x, y), radius, sparkleColours[rng.Next(sparkleColours.Length)],
                (onVelvet ? 0.7 : 0.55) + rng.NextDouble() * 0.3, rng.NextDouble() < 0.3 ? 45 : 0);
        }
    }

    /// <summary>
    /// Draws the corner flourish (authored in a 0..30 box) at all four corners of
    /// <paramref name="frame"/>, mirrored so that each one turns inwards.
    /// </summary>
    public static void CornerFlourishes(XGraphics gfx, XRect frame, double size, XColor colour, double opacity = 1.0)
    {
        const double designBox = 30.0;

        (double X, double Y, double Sx, double Sy)[] corners =
        [
            (frame.X, frame.Y, 1, 1),
            (frame.X + frame.Width, frame.Y, -1, 1),
            (frame.X, frame.Y + frame.Height, 1, -1),
            (frame.X + frame.Width, frame.Y + frame.Height, -1, -1),
        ];

        foreach (var (x, y, sx, sy) in corners)
        {
            var state = gfx.Save();
            gfx.TranslateTransform(x, y);
            gfx.ScaleTransform(sx, sy);
            DrawLayers(gfx, EmblemArt.CornerFlourish, new XRect(0, 0, size, size),
                colour, colour, colour, opacity, designBox);
            gfx.Restore(state);
        }
    }

    /// <summary>A fine double rule: a thicker line with a hairline a short distance below it.</summary>
    public static void DoubleRule(
        XGraphics gfx, double x1, double x2, double y, XColor color,
        double thick = 1.1, double thin = 0.4, double gap = 2.6)
    {
        gfx.DrawLine(new XPen(color, thick), new XPoint(x1, y), new XPoint(x2, y));
        gfx.DrawLine(new XPen(color.Alpha(0.55), thin), new XPoint(x1, y + gap), new XPoint(x2, y + gap));
    }

    /// <summary>A short centred divider: hairline rules either side of a small dot cluster.</summary>
    public static void Divider(XGraphics gfx, double centerX, double y, double width, XColor color)
    {
        var half = width / 2.0;
        var pen = new XPen(color.Alpha(0.6), 0.6);
        gfx.DrawLine(pen, new XPoint(centerX - half, y), new XPoint(centerX - 11, y));
        gfx.DrawLine(pen, new XPoint(centerX + 11, y), new XPoint(centerX + half, y));

        var brush = new XSolidBrush(color);
        foreach (var dx in new[] { -5.5, 0.0, 5.5 })
        {
            var r = dx == 0 ? 1.9 : 1.2;
            gfx.DrawEllipse(brush, new XRect(centerX + dx - r, y - r, r * 2, r * 2));
        }
    }

    /// <summary>Three tiny dots, as used along the foot of a card.</summary>
    public static void DotCluster(XGraphics gfx, double centerX, double y, double unit, XBrush brush)
    {
        foreach (var dx in new[] { -1.0, 0.0, 1.0 })
        {
            var r = dx == 0 ? unit * 0.62 : unit * 0.42;
            gfx.DrawEllipse(brush, new XRect(centerX + dx * unit * 2.1 - r, y - r, r * 2, r * 2));
        }
    }

    /// <summary>A small cuneiform-like wedge whose tip points along <paramref name="directionDegrees"/>.</summary>
    public static void Wedge(XGraphics gfx, XPoint tip, double length, double halfWidth, double directionDegrees, XBrush brush)
    {
        var state = gfx.Save();
        gfx.RotateAtTransform(directionDegrees, tip);
        var path = new XGraphicsPath();
        path.AddPolygon(
        [
            tip,
            new XPoint(tip.X - length, tip.Y - halfWidth),
            new XPoint(tip.X - length, tip.Y + halfWidth),
        ]);
        path.CloseFigure();
        gfx.DrawPath(brush, path);
        gfx.Restore(state);
    }

    /// <summary>
    /// Share of the Octaquetra's outer radius at which the petal stations sit (just outside each lobe's
    /// centre, so the cards rest inside the lobes). Matches the app's spread.
    /// </summary>
    public const double StationShare = 0.62;

    /// <summary>
    /// The Octaquetra of Inanna the spread is laid on, in gold leaf under a faint outer circle.
    /// When <paramref name="stationRadius"/> is given, the Octaquetra is sized so the petal stations sit
    /// inside its lobes, but its edge never passes <paramref name="radius"/>.
    /// </summary>
    public static void SpreadOctaquetra(XGraphics gfx, XPoint center, double radius, XColor gold, XColor glow,
                                     double? stationRadius = null, double opacity = 0.55)
    {
        var OctaquetraRadius = stationRadius is double stations
            ? Math.Min(stations / StationShare, radius)
            : radius * 0.94;
        var outer = Math.Max(radius, OctaquetraRadius + 3);

        // A soft halo: widening rings, each fainter than the last.
        for (var i = 4; i >= 1; i--)
        {
            var r = outer * (1.0 + i * 0.018);
            gfx.DrawEllipse(new XPen(glow.Alpha(0.05 * i), 1.6),
                new XRect(center.X - r, center.Y - r, r * 2, r * 2));
        }

        gfx.DrawEllipse(new XPen(gold.Alpha(0.42), 0.8),
            new XRect(center.X - outer, center.Y - outer, outer * 2, outer * 2));

        // the gold leaf itself, exactly as authored
        InannaOctaquetra(gfx, center, OctaquetraRadius, 0, opacity, background: PdfPalette.Ivory);
    }

    /// <summary>One petal pointing straight up from <paramref name="center"/>, as two cubic beziers.</summary>
    private static XGraphicsPath Petal(XPoint center, double length, double halfWidth)
    {
        double cx = center.X, cy = center.Y;
        var tip = new XPoint(cx, cy - length);
        var path = new XGraphicsPath();
        path.AddBezier(
            new XPoint(cx, cy),
            new XPoint(cx - halfWidth, cy - length * 0.32),
            new XPoint(cx - halfWidth * 0.78, cy - length * 0.86),
            tip);
        path.AddBezier(
            tip,
            new XPoint(cx + halfWidth * 0.78, cy - length * 0.86),
            new XPoint(cx + halfWidth, cy - length * 0.32),
            new XPoint(cx, cy));
        path.CloseFigure();
        return path;
    }
}
