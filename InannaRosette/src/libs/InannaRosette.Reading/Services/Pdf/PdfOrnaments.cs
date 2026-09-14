using CodeBrix.PdfDocuments.Drawing;
using InannaRosette.Reading.Data;

namespace InannaRosette.Reading.Services.Pdf;

/// <summary>
/// The ornamental vocabulary of the report: the Venus star, rosettes, corner flourishes,
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

    /// <summary>Draws the rosette motif so that its petals reach <paramref name="radius"/>.</summary>
    public static void Rosette(
        XGraphics gfx, XPoint center, double radius,
        XColor main, XColor? accent = null, XColor? stroke = null, double opacity = 1.0)
        => DrawLayers(gfx, EmblemArt.RosetteMotif, BoxFor(center, radius),
            main, accent ?? main, stroke ?? main, opacity);

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
    /// The ornamental rosette the spread is laid on: eight elongated petal outlines radiating from
    /// a centre ring, in thin gold strokes, under a faint outer circle.
    /// </summary>
    public static void SpreadRosette(XGraphics gfx, XPoint center, double radius, XColor gold, XColor glow)
    {
        // A soft halo: widening rings, each fainter than the last.
        for (var i = 4; i >= 1; i--)
        {
            var r = radius * (1.0 + i * 0.018);
            gfx.DrawEllipse(new XPen(glow.Alpha(0.05 * i), 1.6),
                new XRect(center.X - r, center.Y - r, r * 2, r * 2));
        }

        gfx.DrawEllipse(new XPen(gold.Alpha(0.42), 0.8),
            new XRect(center.X - radius, center.Y - radius, radius * 2, radius * 2));
        var inner = radius * 0.955;
        gfx.DrawEllipse(new XPen(gold.Alpha(0.22), 0.5),
            new XRect(center.X - inner, center.Y - inner, inner * 2, inner * 2));

        // Eight bezier petals, tips reaching almost to the outer circle.
        var petalPen = new XPen(gold.Alpha(0.5), 0.85);
        var petalFill = new XSolidBrush(gold.Alpha(0.05));
        var length = radius * 0.93;
        var halfWidth = radius * 0.235;

        for (var i = 0; i < 8; i++)
        {
            var state = gfx.Save();
            gfx.RotateAtTransform(i * 45.0, center);
            gfx.DrawPath(petalPen, petalFill, Petal(center, length, halfWidth));
            gfx.Restore(state);
        }

        // Hairline spokes on the half-angles, so the petals read as a flower and not a wheel.
        var spoke = new XPen(gold.Alpha(0.32), 0.5) { DashStyle = XDashStyle.Dot, LineCap = XLineCap.Round };
        for (var i = 0; i < 8; i++)
        {
            var a = Math.PI * 2 * i / 8 + Math.PI / 8;
            gfx.DrawLine(spoke,
                new XPoint(center.X + Math.Sin(a) * radius * 0.20, center.Y - Math.Cos(a) * radius * 0.20),
                new XPoint(center.X + Math.Sin(a) * radius * 0.99, center.Y - Math.Cos(a) * radius * 0.99));
        }

        // Centre rings.
        var hub = radius * 0.175;
        gfx.DrawEllipse(new XPen(gold.Alpha(0.55), 1.0),
            new XRect(center.X - hub, center.Y - hub, hub * 2, hub * 2));
        var hub2 = hub * 0.72;
        gfx.DrawEllipse(new XPen(gold.Alpha(0.3), 0.6),
            new XRect(center.X - hub2, center.Y - hub2, hub2 * 2, hub2 * 2));
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
