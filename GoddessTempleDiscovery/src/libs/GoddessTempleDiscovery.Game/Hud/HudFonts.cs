using System;
using GoddessTempleDiscovery.Assets;
using SkiaSharp;

namespace GoddessTempleDiscovery.Game.Hud;

/// <summary>The embedded Merriweather faces the HUD draws its text with.</summary>
public sealed class HudFonts : IDisposable
{
    /// <summary>Loads the faces from the Assets library.</summary>
    public HudFonts()
    {
        Regular = Load(FontAssets.MerriweatherRegular);
        Bold = Load(FontAssets.MerriweatherBold);
        Italic = Load(FontAssets.MerriweatherItalic);
    }

    /// <summary>Merriweather Regular.</summary>
    public SKTypeface Regular { get; }

    /// <summary>Merriweather Bold.</summary>
    public SKTypeface Bold { get; }

    /// <summary>Merriweather Italic.</summary>
    public SKTypeface Italic { get; }

    /// <summary>The advance width of a line, plus <paramref name="trackingEm"/> of an em after every character.</summary>
    /// <param name="text">The text.</param>
    /// <param name="typeface">The face.</param>
    /// <param name="size">The size.</param>
    /// <param name="trackingEm">Extra space per character, in ems.</param>
    /// <returns>The width.</returns>
    public static float Measure(string text, SKTypeface typeface, float size, float trackingEm = 0)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        using var font = new SKFont(typeface, size) { Subpixel = true };
        var width = font.MeasureText(text);
        return trackingEm == 0 ? width : width + (trackingEm * size * Math.Max(0, text.Length - 1));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Regular.Dispose();
        Bold.Dispose();
        Italic.Dispose();
    }

    private static SKTypeface Load(string resource)
    {
        using var stream = FontAssets.Open(resource);
        return SKTypeface.FromStream(stream) ?? SKTypeface.Default;
    }
}
