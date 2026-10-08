using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CodeBrix.PdfDocuments.Drawing;
using GoddessTempleDiscovery.Game.Cards;

namespace GoddessTempleDiscovery.Game.Journal.Pdf;

/// <summary>
/// Draws an SVG picture onto a PDF page as vector shapes: <c>&lt;path&gt;</c> (through <see cref="SvgPathToPdf"/>),
/// <c>&lt;rect&gt;</c>, <c>&lt;circle&gt;</c>, <c>&lt;ellipse&gt;</c>, <c>&lt;polygon&gt;</c>, <c>&lt;polyline&gt;</c>,
/// <c>&lt;line&gt;</c> and <c>&lt;g transform&gt;</c> (translate, scale, rotate, matrix), with fill, stroke,
/// stroke-width and opacity inherited down the groups. A gradient paints as its first stop's colour; clip paths and
/// anything else are skipped.
/// </summary>
public static class SvgToPdf
{
    /// <summary>The element kinds drawn.</summary>
    public static readonly IReadOnlyList<string> Supported = new[] { "path", "rect", "circle", "ellipse", "polygon", "polyline", "line", "g" };

    private sealed record Style(string Fill, string Stroke, double StrokeWidth, double Opacity, double FillOpacity, double StrokeOpacity, string FillRule);

    /// <summary>Draws SVG text fitted (aspect kept, centred) into a rectangle.</summary>
    /// <param name="gfx">The page graphics.</param>
    /// <param name="svg">The SVG text.</param>
    /// <param name="box">The rectangle.</param>
    /// <returns>The number of shapes drawn.</returns>
    public static int Draw(XGraphics gfx, string svg, XRect box) => Draw(gfx, svg, box, null);

    /// <summary>Draws the part <paramref name="source"/> of SVG text fitted (aspect kept, centred) into a rectangle.</summary>
    /// <param name="gfx">The page graphics.</param>
    /// <param name="svg">The SVG text.</param>
    /// <param name="box">The rectangle.</param>
    /// <param name="source">The part of the picture to show, in its own units; null for its whole view box.</param>
    /// <returns>The number of shapes drawn.</returns>
    public static int Draw(XGraphics gfx, string svg, XRect box, XRect? source)
    {
        ArgumentNullException.ThrowIfNull(gfx);
        var picture = SvgInliner.Parse(svg);
        var view = source ?? new XRect(picture.MinX, picture.MinY, picture.Width, picture.Height);
        var scale = Math.Min(box.Width / view.Width, box.Height / view.Height);
        var dx = box.X + ((box.Width - (view.Width * scale)) / 2) - (view.X * scale);
        var dy = box.Y + ((box.Height - (view.Height * scale)) / 2) - (view.Y * scale);
        var gradients = picture.Root.Descendants()
            .Where(e => e.Name.LocalName is "linearGradient" or "radialGradient" && e.Attribute("id") != null)
            .ToDictionary(e => (string)e.Attribute("id"), FirstStop, StringComparer.Ordinal);

        var state = gfx.Save();
        try
        {
            gfx.IntersectClip(new XRect(box.X + ((box.Width - (view.Width * scale)) / 2), box.Y + ((box.Height - (view.Height * scale)) / 2),
                view.Width * scale, view.Height * scale));
            gfx.TranslateTransform(dx, dy);
            gfx.ScaleTransform(scale, scale);
            var root = Inherit(new Style("#000000", "none", 1, 1, 1, 1, "nonzero"), picture.Root);
            return Children(gfx, picture.Root, root, gradients);
        }
        finally
        {
            gfx.Restore(state);
        }
    }

    /// <summary>Parses an SVG transform list into a matrix.</summary>
    /// <param name="transform">The attribute value, such as "translate(10,20) rotate(30 5 5)".</param>
    /// <returns>The matrix (identity for null or empty).</returns>
    public static XMatrix ParseTransform(string transform)
    {
        var matrix = XMatrix.Identity;
        if (string.IsNullOrWhiteSpace(transform))
        {
            return matrix;
        }

        var text = transform;
        var i = 0;
        while (i < text.Length)
        {
            var open = text.IndexOf('(', i);
            if (open < 0)
            {
                break;
            }

            var close = text.IndexOf(')', open);
            if (close < 0)
            {
                break;
            }

            var name = text.Substring(i, open - i).Trim().Trim(',').Trim();
            var args = text.Substring(open + 1, close - open - 1)
                .Split(new[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => double.Parse(a, CultureInfo.InvariantCulture)).ToArray();
            XMatrix step;
            switch (name)
            {
                case "translate":
                    step = new XMatrix(1, 0, 0, 1, args.ElementAtOrDefault(0), args.ElementAtOrDefault(1));
                    break;
                case "scale":
                    var sx = args.ElementAtOrDefault(0);
                    step = new XMatrix(sx, 0, 0, args.Length > 1 ? args[1] : sx, 0, 0);
                    break;
                case "rotate":
                    var radians = args.ElementAtOrDefault(0) * Math.PI / 180;
                    var rotation = new XMatrix(Math.Cos(radians), Math.Sin(radians), -Math.Sin(radians), Math.Cos(radians), 0, 0);
                    if (args.Length >= 3)
                    {
                        //rotate(a cx cy) is translate(cx cy) rotate(a) translate(-cx -cy)
                        step = new XMatrix(1, 0, 0, 1, -args[1], -args[2]);
                        step.Append(rotation);
                        step.Append(new XMatrix(1, 0, 0, 1, args[1], args[2]));
                    }
                    else
                    {
                        step = rotation;
                    }

                    break;
                case "matrix" when args.Length == 6:
                    step = new XMatrix(args[0], args[1], args[2], args[3], args[4], args[5]);
                    break;
                default:
                    step = XMatrix.Identity;
                    break;
            }

            //SVG applies the list left to right to the coordinate system: the leftmost transform is outermost
            matrix.Prepend(step);
            i = close + 1;
        }

        return matrix;
    }

    private static int Children(XGraphics gfx, XElement parent, Style style, IReadOnlyDictionary<string, string> gradients)
    {
        var drawn = 0;
        foreach (var element in parent.Elements())
        {
            drawn += Element(gfx, element, style, gradients);
        }

        return drawn;
    }

    private static int Element(XGraphics gfx, XElement element, Style inherited, IReadOnlyDictionary<string, string> gradients)
    {
        var name = element.Name.LocalName;
        if (!Supported.Contains(name))
        {
            return 0;
        }

        var style = Inherit(inherited, element);
        var transform = (string)element.Attribute("transform");
        var saved = transform == null ? null : gfx.Save();
        try
        {
            if (transform != null)
            {
                gfx.MultiplyTransform(ParseTransform(transform));
            }

            if (name == "g")
            {
                return Children(gfx, element, style, gradients);
            }

            var path = Shape(element);
            if (path == null)
            {
                return 0;
            }

            var brush = Brush(style, gradients);
            var pen = Pen(style, gradients);
            if (name == "line")
            {
                brush = null;
            }

            path.FillMode = style.FillRule == "evenodd" ? XFillMode.Alternate : XFillMode.Winding;
            if (brush == null && pen == null)
            {
                return 0;
            }

            if (brush != null && pen != null)
            {
                gfx.DrawPath(pen, brush, path);
            }
            else if (brush != null)
            {
                gfx.DrawPath(brush, path);
            }
            else
            {
                gfx.DrawPath(pen, path);
            }

            return 1;
        }
        finally
        {
            if (saved != null)
            {
                gfx.Restore(saved);
            }
        }
    }

    private static XGraphicsPath Shape(XElement element)
    {
        double A(string attribute) => double.TryParse((string)element.Attribute(attribute), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0;
        switch (element.Name.LocalName)
        {
            case "path":
                var d = (string)element.Attribute("d");
                return string.IsNullOrWhiteSpace(d) ? null : SvgPathToPdf.Build(d, 1, 1, 0, 0);
            case "rect":
            {
                var path = new XGraphicsPath();
                var rx = A("rx");
                if (rx > 0)
                {
                    path.AddRoundedRectangle(A("x"), A("y"), A("width"), A("height"), rx * 2, (A("ry") > 0 ? A("ry") : rx) * 2);
                }
                else
                {
                    path.AddRectangle(new XRect(A("x"), A("y"), A("width"), A("height")));
                }

                return path;
            }

            case "circle":
            {
                var path = new XGraphicsPath();
                var r = A("r");
                path.AddEllipse(new XRect(A("cx") - r, A("cy") - r, r * 2, r * 2));
                return path;
            }

            case "ellipse":
            {
                var path = new XGraphicsPath();
                path.AddEllipse(new XRect(A("cx") - A("rx"), A("cy") - A("ry"), A("rx") * 2, A("ry") * 2));
                return path;
            }

            case "line":
            {
                var path = new XGraphicsPath();
                path.AddLine(A("x1"), A("y1"), A("x2"), A("y2"));
                return path;
            }

            case "polygon":
            case "polyline":
            {
                var numbers = ((string)element.Attribute("points") ?? string.Empty)
                    .Split(new[] { ' ', ',', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => double.Parse(n, CultureInfo.InvariantCulture)).ToArray();
                if (numbers.Length < 4)
                {
                    return null;
                }

                var points = new XPoint[numbers.Length / 2];
                for (var i = 0; i < points.Length; i++)
                {
                    points[i] = new XPoint(numbers[i * 2], numbers[(i * 2) + 1]);
                }

                var path = new XGraphicsPath();
                if (element.Name.LocalName == "polygon")
                {
                    path.AddPolygon(points);
                }
                else
                {
                    path.AddLines(points);
                }

                return path;
            }

            default:
                return null;
        }
    }

    private static Style Inherit(Style parent, XElement element)
    {
        string Get(string name)
        {
            var value = (string)element.Attribute(name);
            var css = (string)element.Attribute("style");
            if (css != null)
            {
                foreach (var part in css.Split(';'))
                {
                    var colon = part.IndexOf(':');
                    if (colon > 0 && part.Substring(0, colon).Trim() == name)
                    {
                        value = part.Substring(colon + 1).Trim();
                    }
                }
            }

            return value;
        }

        double Number(string name, double fallback) =>
            double.TryParse(Get(name), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : fallback;

        return new Style(
            Get("fill") ?? parent.Fill,
            Get("stroke") ?? parent.Stroke,
            Number("stroke-width", parent.StrokeWidth),
            parent.Opacity * Number("opacity", 1),
            Number("fill-opacity", parent.FillOpacity),
            Number("stroke-opacity", parent.StrokeOpacity),
            Get("fill-rule") ?? parent.FillRule);
    }

    private static XBrush Brush(Style style, IReadOnlyDictionary<string, string> gradients)
    {
        var colour = Colour(style.Fill, style.Opacity * style.FillOpacity, gradients);
        return colour == null ? null : new XSolidBrush(colour.Value);
    }

    private static XPen Pen(Style style, IReadOnlyDictionary<string, string> gradients)
    {
        var colour = Colour(style.Stroke, style.Opacity * style.StrokeOpacity, gradients);
        if (colour == null || style.StrokeWidth <= 0)
        {
            return null;
        }

        return new XPen(colour.Value, style.StrokeWidth) { LineJoin = XLineJoin.Round, LineCap = XLineCap.Round };
    }

    private static XColor? Colour(string value, double opacity, IReadOnlyDictionary<string, string> gradients)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "none" || value == "transparent")
        {
            return null;
        }

        var text = value.Trim();
        if (text.StartsWith("url(#", StringComparison.Ordinal))
        {
            var id = text.Substring(5).TrimEnd(')');
            if (!gradients.TryGetValue(id, out text) || text == null)
            {
                return null;
            }
        }

        var alpha = (int)Math.Round(Math.Clamp(opacity, 0, 1) * 255);
        if (text.StartsWith('#'))
        {
            var hex = text.Substring(1);
            if (hex.Length == 3)
            {
                hex = string.Concat(hex.Select(c => new string(c, 2)));
            }

            if (hex.Length == 6 && int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
            {
                return XColor.FromArgb(alpha, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
            }

            return null;
        }

        return text switch
        {
            "white" => XColor.FromArgb(alpha, 255, 255, 255),
            "black" => XColor.FromArgb(alpha, 0, 0, 0),
            _ => null,
        };
    }

    private static string FirstStop(XElement gradient)
    {
        var stop = gradient.Elements().FirstOrDefault(e => e.Name.LocalName == "stop");
        if (stop == null)
        {
            return null;
        }

        var colour = (string)stop.Attribute("stop-color");
        var css = (string)stop.Attribute("style");
        if (colour == null && css != null)
        {
            var part = css.Split(';').FirstOrDefault(p => p.Trim().StartsWith("stop-color", StringComparison.Ordinal));
            colour = part?.Substring(part.IndexOf(':') + 1).Trim();
        }

        return colour;
    }
}
