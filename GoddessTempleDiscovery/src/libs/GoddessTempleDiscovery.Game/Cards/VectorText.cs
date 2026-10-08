using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GoddessTempleDiscovery.Assets;
using SkiaSharp;

namespace GoddessTempleDiscovery.Game.Cards;

/// <summary>
/// Text set as vector outlines: the glyphs of an embedded face turned into SVG path data with SkiaSharp
/// (<see cref="SKFont.GetGlyphPath"/>, <see cref="SKPath.ToSvgPathData"/>), so a card face carries its lettering
/// without a font. A fallback face covers characters the main face lacks (the modifier letters in "ᵈ𒈹").
/// </summary>
public sealed class VectorText : IDisposable
{
    private static readonly Regex Decimals = new Regex(@"(\d+\.\d)\d+", RegexOptions.Compiled);
    private static readonly Regex Zeros = new Regex(@"(\d)\.0(?!\d)", RegexOptions.Compiled);

    private readonly SKTypeface _typeface;
    private readonly SKFont _probe;
    private readonly VectorText _fallback;
    private readonly bool _ownsFallback;

    /// <summary>Creates vector text over one embedded face.</summary>
    /// <param name="fontResource">A resource name from <see cref="FontAssets"/>.</param>
    /// <param name="fallback">A face for characters this one lacks, or null.</param>
    /// <param name="ownsFallback">True when disposing this also disposes <paramref name="fallback"/>.</param>
    public VectorText(string fontResource, VectorText fallback = null, bool ownsFallback = false)
    {
        using var stream = FontAssets.Open(fontResource);
        _typeface = SKTypeface.FromStream(stream)
                    ?? throw new InvalidOperationException($"The font '{fontResource}' could not be read.");
        _probe = new SKFont(_typeface, 12);
        _fallback = fallback;
        _ownsFallback = ownsFallback;
    }

    /// <summary>The typeface (for measuring or for the HUD).</summary>
    public SKTypeface Typeface => _typeface;

    /// <summary>The advance width of a line of text.</summary>
    /// <param name="text">The text.</param>
    /// <param name="size">The font size.</param>
    /// <param name="tracking">Extra space after every character, in font units of 1/1000 em.</param>
    /// <returns>The width.</returns>
    public float Measure(string text, float size, float tracking = 0)
    {
        float width = 0;
        foreach (var (run, face) in Runs(text, tracking != 0))
        {
            using var font = face.Font(size);
            width += font.MeasureText(run) + (tracking == 0 ? 0 : tracking * size / 1000f);
        }

        return width;
    }

    /// <summary>The SVG path data of one line of text.</summary>
    /// <param name="text">The text.</param>
    /// <param name="size">The font size.</param>
    /// <param name="x">The left edge of the line.</param>
    /// <param name="baseline">The baseline.</param>
    /// <param name="tracking">Extra space after every character, in 1/1000 em.</param>
    /// <returns>The path data (empty for empty text).</returns>
    public string PathData(string text, float size, float x, float baseline, float tracking = 0)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var data = new StringBuilder();
        var pen = x;
        foreach (var (run, face) in Runs(text, tracking != 0))
        {
            using var font = face.Font(size);
            using var path = font.GetTextPath(run, new SKPoint(pen, baseline));
            if (path != null && !path.IsEmpty)
            {
                data.Append(path.ToSvgPathData());
            }

            pen += font.MeasureText(run) + (tracking == 0 ? 0 : tracking * size / 1000f);
        }

        return Zeros.Replace(Decimals.Replace(data.ToString(), "$1"), "$1");
    }

    /// <summary>The path data of a line centred on a point.</summary>
    /// <param name="text">The text.</param>
    /// <param name="size">The font size.</param>
    /// <param name="centreX">The centre of the line.</param>
    /// <param name="baseline">The baseline.</param>
    /// <param name="tracking">Extra space after every character, in 1/1000 em.</param>
    /// <returns>The path data.</returns>
    public string CentredPathData(string text, float size, float centreX, float baseline, float tracking = 0) =>
        PathData(text, size, centreX - (Measure(text, size, tracking) / 2f), baseline, tracking);

    /// <summary>Breaks text into lines no wider than <paramref name="width"/>, word by word.</summary>
    /// <param name="text">The text.</param>
    /// <param name="size">The font size.</param>
    /// <param name="width">The line width.</param>
    /// <returns>The lines.</returns>
    public IReadOnlyList<string> Wrap(string text, float size, float width)
    {
        var lines = new List<string>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return lines;
        }

        var line = new StringBuilder();
        foreach (var word in text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = line.Length == 0 ? word : line + " " + word;
            if (line.Length > 0 && Measure(candidate, size) > width)
            {
                lines.Add(line.ToString());
                line.Clear();
                line.Append(word);
            }
            else
            {
                line.Clear();
                line.Append(candidate);
            }
        }

        if (line.Length > 0)
        {
            lines.Add(line.ToString());
        }

        return lines;
    }

    /// <summary>Whether this face (not counting the fallback) has a glyph for every character of the text.</summary>
    /// <param name="text">The text.</param>
    /// <returns>True when covered.</returns>
    public bool Covers(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            var element = enumerator.GetTextElement();
            if (!char.IsWhiteSpace(element, 0) && !HasGlyph(element))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _probe.Dispose();
        _typeface.Dispose();
        if (_ownsFallback)
        {
            _fallback?.Dispose();
        }
    }

    private bool HasGlyph(string element)
    {
        var codepoint = char.ConvertToUtf32(element, 0);
        return _probe.GetGlyph(codepoint) != 0;
    }

    private SKFont Font(float size) =>
        new SKFont(_typeface, size) { Subpixel = true, LinearMetrics = true, Hinting = SKFontHinting.None };

    //Runs of text set in one face (the fallback takes characters this face lacks); one text element (a character
    //  or a surrogate pair) per run when the text is tracked, so the extra space falls between every pair
    private IEnumerable<(string Run, VectorText Face)> Runs(string text, bool perElement)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield break;
        }

        var run = new StringBuilder();
        VectorText runFace = null;
        var enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            var element = enumerator.GetTextElement();
            var face = char.IsWhiteSpace(element, 0) || HasGlyph(element) || _fallback == null ? this : _fallback;
            if (run.Length > 0 && (perElement || !ReferenceEquals(face, runFace)))
            {
                yield return (run.ToString(), runFace);
                run.Clear();
            }

            runFace = face;
            run.Append(element);
        }

        if (run.Length > 0)
        {
            yield return (run.ToString(), runFace);
        }
    }
}
