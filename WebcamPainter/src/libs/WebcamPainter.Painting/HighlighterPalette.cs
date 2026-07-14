using CodeBrix.Imaging;
using System.Collections.Generic;

namespace WebcamPainter.Painting;

/// <summary>One selectable highlighter color: a layer name and its ink color.</summary>
public sealed class HighlighterColor
{
    /// <summary>Creates a highlighter color entry.</summary>
    public HighlighterColor(string name, Color color)
    {
        Name = name;
        Color = color;
    }

    /// <summary>The color's display name; also the name of its drawing layer.</summary>
    public string Name { get; }

    /// <summary>The ink color.</summary>
    public Color Color { get; }
}

/// <summary>
/// The set of highlighter colors a <see cref="PaintingSession"/> offers - one drawing layer
/// per color. Tweak the color values here (and keep the hard-coded button backgrounds in
/// MainPage.xaml in sync when you do).
/// </summary>
public static class HighlighterPalette
{
    /// <summary>The ROYGBIV highlighter colors, in rainbow order.</summary>
    public static IReadOnlyList<HighlighterColor> Colors { get; } = new[]
    {
        new HighlighterColor("Red", Color.FromRgb(230, 30, 30)),
        new HighlighterColor("Orange", Color.FromRgb(255, 140, 20)),
        new HighlighterColor("Yellow", Color.FromRgb(240, 220, 20)),
        new HighlighterColor("Green", Color.FromRgb(40, 200, 60)),
        new HighlighterColor("Blue", Color.FromRgb(40, 90, 235)),
        new HighlighterColor("Indigo", Color.FromRgb(85, 45, 180)),
        new HighlighterColor("Violet", Color.FromRgb(190, 60, 220)),
    };
}
