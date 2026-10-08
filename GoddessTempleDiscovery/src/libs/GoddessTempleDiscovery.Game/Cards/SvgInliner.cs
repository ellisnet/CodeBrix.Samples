using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace GoddessTempleDiscovery.Game.Cards;

/// <summary>
/// Inlines one SVG picture into another: its root's children go under a <c>&lt;g transform="translate(..)
/// scale(..)"&gt;</c> that fits its view box into a target box, the header comment is dropped, the root's
/// presentation attributes move to the group, and every id (and every <c>url(#id)</c> or <c>#id</c> reference) gets
/// a prefix so two inlined pictures never collide.
/// </summary>
public static class SvgInliner
{
    /// <summary>The SVG namespace.</summary>
    public static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

    private static readonly XNamespace XLink = "http://www.w3.org/1999/xlink";

    private static readonly string[] RootOnlyAttributes = { "viewBox", "width", "height", "x", "y", "version", "preserveAspectRatio" };

    /// <summary>A parsed picture: its view box and its root element.</summary>
    /// <param name="Root">The root &lt;svg&gt; element.</param>
    /// <param name="MinX">The view box's left.</param>
    /// <param name="MinY">The view box's top.</param>
    /// <param name="Width">The view box's width.</param>
    /// <param name="Height">The view box's height.</param>
    public sealed record Picture(XElement Root, double MinX, double MinY, double Width, double Height);

    /// <summary>Parses SVG text (comments are dropped).</summary>
    /// <param name="svg">The SVG text.</param>
    /// <returns>The picture.</returns>
    /// <exception cref="FormatException">When the text is not an SVG document with a usable size.</exception>
    public static Picture Parse(string svg)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(svg);
        var document = XDocument.Parse(svg, LoadOptions.None);
        var root = document.Root;
        if (root == null || root.Name.LocalName != "svg")
        {
            throw new FormatException("Not an SVG document.");
        }

        foreach (var comment in document.DescendantNodes().OfType<XComment>().ToArray())
        {
            comment.Remove();
        }

        double minX = 0, minY = 0, width = 0, height = 0;
        var viewBox = (string)root.Attribute("viewBox");
        if (!string.IsNullOrWhiteSpace(viewBox))
        {
            var parts = viewBox.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => double.Parse(p, CultureInfo.InvariantCulture)).ToArray();
            if (parts.Length == 4)
            {
                (minX, minY, width, height) = (parts[0], parts[1], parts[2], parts[3]);
            }
        }

        if (width <= 0 || height <= 0)
        {
            width = Length((string)root.Attribute("width"));
            height = Length((string)root.Attribute("height"));
        }

        if (width <= 0 || height <= 0)
        {
            throw new FormatException("The SVG document has no usable size.");
        }

        return new Picture(root, minX, minY, width, height);
    }

    /// <summary>
    /// A group holding the picture's content, fitted (aspect kept, centred) into the box at
    /// (<paramref name="x"/>, <paramref name="y"/>) of the given size.
    /// </summary>
    /// <param name="picture">The picture.</param>
    /// <param name="idPrefix">The prefix for its ids, unique within the target document.</param>
    /// <param name="x">The box's left.</param>
    /// <param name="y">The box's top.</param>
    /// <param name="width">The box's width.</param>
    /// <param name="height">The box's height.</param>
    /// <param name="stretch">True to fill the box exactly (aspect not kept).</param>
    /// <param name="clip">True to clip the content to the picture's view box (art that bleeds past its edge).</param>
    /// <returns>The group.</returns>
    public static XElement Fit(Picture picture, string idPrefix, double x, double y, double width, double height, bool stretch = false,
        bool clip = false)
    {
        ArgumentNullException.ThrowIfNull(picture);
        double sx = width / picture.Width, sy = height / picture.Height;
        double tx, ty;
        if (stretch)
        {
            tx = x - (picture.MinX * sx);
            ty = y - (picture.MinY * sy);
        }
        else
        {
            sx = sy = Math.Min(sx, sy);
            tx = x + ((width - (picture.Width * sx)) / 2) - (picture.MinX * sx);
            ty = y + ((height - (picture.Height * sy)) / 2) - (picture.MinY * sy);
        }

        var group = new XElement(Svg + "g",
            new XAttribute("transform", string.Format(CultureInfo.InvariantCulture,
                "translate({0:0.###},{1:0.###}) scale({2:0.#####},{3:0.#####})", tx, ty, sx, sy)));
        foreach (var attribute in picture.Root.Attributes())
        {
            if (attribute.IsNamespaceDeclaration || RootOnlyAttributes.Contains(attribute.Name.LocalName))
            {
                continue;
            }

            group.Add(new XAttribute(attribute.Name, attribute.Value));
        }

        if (clip)
        {
            var clipId = (idPrefix ?? string.Empty) + "viewbox-clip";
            group.Add(new XElement(Svg + "clipPath", new XAttribute("id", clipId),
                new XElement(Svg + "rect",
                    new XAttribute("x", picture.MinX.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("y", picture.MinY.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("width", picture.Width.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("height", picture.Height.ToString(CultureInfo.InvariantCulture)))));
            group.Add(new XAttribute("clip-path", "url(#" + clipId + ")"));
        }

        foreach (var child in picture.Root.Elements())
        {
            group.Add(Prefix(new XElement(child), idPrefix));
        }

        return group;
    }

    private static XElement Prefix(XElement element, string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            return element;
        }

        foreach (var node in element.DescendantsAndSelf())
        {
            foreach (var attribute in node.Attributes().ToArray())
            {
                if (attribute.Name.LocalName == "id")
                {
                    attribute.Value = prefix + attribute.Value;
                }
                else if (attribute.Name.LocalName == "href" || attribute.Name == XLink + "href")
                {
                    if (attribute.Value.StartsWith('#'))
                    {
                        attribute.Value = "#" + prefix + attribute.Value.Substring(1);
                    }
                }
                else if (attribute.Value.Contains("url(#", StringComparison.Ordinal))
                {
                    attribute.Value = attribute.Value.Replace("url(#", "url(#" + prefix, StringComparison.Ordinal);
                }
            }
        }

        return element;
    }

    private static double Length(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        var digits = new string(value.TakeWhile(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
        return double.TryParse(digits, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ? number : 0;
    }
}
