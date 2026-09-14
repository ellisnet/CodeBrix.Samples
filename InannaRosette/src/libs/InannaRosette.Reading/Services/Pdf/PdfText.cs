using CodeBrix.PdfDocuments.Drawing;

namespace InannaRosette.Reading.Services.Pdf;

/// <summary>Horizontal placement for the tracked-capitals helpers.</summary>
public enum TrackedAlign
{
    /// <summary>Left edge of the box.</summary>
    Left,
    /// <summary>Centred in the box.</summary>
    Center,
    /// <summary>Right edge of the box.</summary>
    Right,
}

/// <summary>
/// Measuring and drawing primitives the report needs and that the PDF library does not provide:
/// letter-spaced ("tracked") capitals, greedy word wrapping, and justified body copy whose last
/// line stays ragged.
/// </summary>
public static class PdfText
{
    /// <summary>Width of <paramref name="text"/> when drawn with the given letter spacing.</summary>
    public static double MeasureTracked(XGraphics gfx, string text, XFont font, double tracking)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        double width = 0;
        foreach (var ch in text) width += gfx.MeasureString(ch.ToString(), font).Width + tracking;
        return width - tracking;
    }

    /// <summary>
    /// Draws letter-spaced text with its baseline at <paramref name="baselineY"/>.
    /// Returns the width consumed.
    /// </summary>
    public static double DrawTracked(
        XGraphics gfx, string text, XFont font, XBrush brush,
        double x, double baselineY, double tracking, TrackedAlign align = TrackedAlign.Left, double boxWidth = 0)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var width = MeasureTracked(gfx, text, font, tracking);
        var penX = align switch
        {
            TrackedAlign.Center => x + (boxWidth - width) / 2.0,
            TrackedAlign.Right => x + boxWidth - width,
            _ => x,
        };

        foreach (var ch in text)
        {
            var s = ch.ToString();
            gfx.DrawString(s, font, brush, new XPoint(penX, baselineY));
            penX += gfx.MeasureString(s, font).Width + tracking;
        }

        return width;
    }

    /// <summary>
    /// Draws tracked capitals, shrinking the tracking and then the point size until the string
    /// fits <paramref name="boxWidth"/>. Returns the width actually used.
    /// </summary>
    public static double DrawTrackedFitted(
        XGraphics gfx, string text, string fontFamily, double emSize, XFontStyle style, XBrush brush,
        double x, double baselineY, double boxWidth, double tracking, TrackedAlign align = TrackedAlign.Left)
    {
        var size = emSize;
        var track = tracking;
        var font = new XFont(fontFamily, size, style);

        for (var guard = 0; guard < 24; guard++)
        {
            var w = MeasureTracked(gfx, text, font, track);
            if (w <= boxWidth) break;
            if (track > 0.15) track = Math.Max(0, track - 0.25);
            else
            {
                size *= 0.94;
                font = new XFont(fontFamily, size, style);
            }
        }

        return DrawTracked(gfx, text, font, brush, x, baselineY, track, align, boxWidth);
    }

    /// <summary>Greedy word wrap. Never returns an empty list for non-empty input.</summary>
    public static List<string> WrapLines(XGraphics gfx, string text, XFont font, double width)
    {
        var lines = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return lines;

        foreach (var hardLine in text.Replace("\r\n", "\n").Split('\n'))
        {
            var words = hardLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                lines.Add(string.Empty);
                continue;
            }

            var current = new List<string>();
            foreach (var word in words)
            {
                var candidate = current.Count == 0 ? word : string.Join(' ', current) + " " + word;
                if (current.Count > 0 && gfx.MeasureString(candidate, font).Width > width)
                {
                    lines.Add(string.Join(' ', current));
                    current.Clear();
                    current.Add(word);
                }
                else
                {
                    current.Add(word);
                }
            }

            if (current.Count > 0) lines.Add(string.Join(' ', current));
        }

        return lines;
    }

    /// <summary>Height a justified or ragged paragraph will occupy, without drawing anything.</summary>
    public static double MeasureParagraph(XGraphics gfx, string text, XFont font, double width, double lineHeight)
        => WrapLines(gfx, text, font, width).Count * lineHeight;

    /// <summary>
    /// Draws a justified paragraph with the last line left aligned - which the built-in
    /// <c>XTextFormatter</c>'s Justify mode does not do. Returns the height consumed.
    /// </summary>
    public static double DrawJustified(
        XGraphics gfx, string text, XFont font, XBrush brush,
        double x, double top, double width, double lineHeight)
        => DrawJustifiedCore(gfx, text, font, brush, x, top, width, lineHeight, justifyLastLine: false);

    /// <summary>
    /// As <see cref="DrawJustified"/>, but justifies the final line too. Used when a paragraph is
    /// split across a page break and its last line on this page is not really the last line.
    /// </summary>
    public static double DrawJustifiedFull(
        XGraphics gfx, string text, XFont font, XBrush brush,
        double x, double top, double width, double lineHeight)
        => DrawJustifiedCore(gfx, text, font, brush, x, top, width, lineHeight, justifyLastLine: true);

    private static double DrawJustifiedCore(
        XGraphics gfx, string text, XFont font, XBrush brush,
        double x, double top, double width, double lineHeight, bool justifyLastLine)
    {
        var lines = WrapLines(gfx, text, font, width);
        if (lines.Count == 0) return 0;

        var ascent = font.GetHeight() * 0.78;
        var baseline = top + ascent;

        for (var i = 0; i < lines.Count; i++)
        {
            var words = lines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var isLast = !justifyLastLine && i == lines.Count - 1;

            if (isLast || words.Length < 2)
            {
                gfx.DrawString(lines[i], font, brush, new XPoint(x, baseline));
            }
            else
            {
                var wordsWidth = words.Sum(w => gfx.MeasureString(w, font).Width);
                var gap = (width - wordsWidth) / (words.Length - 1);

                // Guard against a pathologically short line being stretched into a ladder.
                var naturalGap = gfx.MeasureString(" ", font).Width;
                if (gap > naturalGap * 4.5)
                {
                    gfx.DrawString(lines[i], font, brush, new XPoint(x, baseline));
                }
                else
                {
                    var penX = x;
                    foreach (var w in words)
                    {
                        gfx.DrawString(w, font, brush, new XPoint(penX, baseline));
                        penX += gfx.MeasureString(w, font).Width + gap;
                    }
                }
            }

            baseline += lineHeight;
        }

        return lines.Count * lineHeight;
    }

    /// <summary>Draws a ragged-right (or centred) paragraph. Returns the height consumed.</summary>
    public static double DrawWrapped(
        XGraphics gfx, string text, XFont font, XBrush brush,
        double x, double top, double width, double lineHeight, TrackedAlign align = TrackedAlign.Left)
    {
        var lines = WrapLines(gfx, text, font, width);
        if (lines.Count == 0) return 0;

        var baseline = top + font.GetHeight() * 0.78;
        foreach (var line in lines)
        {
            var lineWidth = gfx.MeasureString(line, font).Width;
            var penX = align switch
            {
                TrackedAlign.Center => x + (width - lineWidth) / 2.0,
                TrackedAlign.Right => x + width - lineWidth,
                _ => x,
            };
            gfx.DrawString(line, font, brush, new XPoint(penX, baseline));
            baseline += lineHeight;
        }

        return lines.Count * lineHeight;
    }

    /// <summary>
    /// Draws a single line, shrinking the point size until it fits <paramref name="width"/>.
    /// Returns the font actually used.
    /// </summary>
    public static XFont DrawFitted(
        XGraphics gfx, string text, string family, double emSize, XFontStyle style, XBrush brush,
        double x, double baselineY, double width, TrackedAlign align = TrackedAlign.Left)
    {
        var size = emSize;
        var font = new XFont(family, size, style);
        for (var guard = 0; guard < 20 && gfx.MeasureString(text, font).Width > width; guard++)
        {
            size *= 0.94;
            font = new XFont(family, size, style);
        }

        var w = gfx.MeasureString(text, font).Width;
        var penX = align switch
        {
            TrackedAlign.Center => x + (width - w) / 2.0,
            TrackedAlign.Right => x + width - w,
            _ => x,
        };
        gfx.DrawString(text, font, brush, new XPoint(penX, baselineY));
        return font;
    }

    /// <summary>
    /// Wraps into at most <paramref name="maxLines"/> lines, shrinking the point size as needed and
    /// finally ellipsising. Used inside the very small mini-cards.
    /// </summary>
    public static (XFont Font, List<string> Lines) FitLines(
        XGraphics gfx, string text, string family, double emSize, XFontStyle style,
        double width, int maxLines, double minSize)
    {
        var size = emSize;
        var font = new XFont(family, size, style);
        var lines = WrapLines(gfx, text, font, width);

        while (lines.Count > maxLines && size > minSize)
        {
            size = Math.Max(minSize, size * 0.92);
            font = new XFont(family, size, style);
            lines = WrapLines(gfx, text, font, width);
        }

        if (lines.Count > maxLines)
        {
            lines = lines.Take(maxLines).ToList();
            var last = lines[^1];
            while (last.Length > 1 && gfx.MeasureString(last + "…", font).Width > width)
            {
                last = last[..^1];
            }

            lines[^1] = last.TrimEnd() + "…";
        }

        return (font, lines);
    }
}
