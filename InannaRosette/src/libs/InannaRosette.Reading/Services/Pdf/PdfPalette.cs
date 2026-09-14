using CodeBrix.PdfDocuments.Drawing;

namespace InannaRosette.Reading.Services.Pdf;

/// <summary>
/// The "temple at dusk" palette of the Rosette of Inanna, as PDF colours.
/// These are the exact tokens from the shared design brief; the XAML resources in the UI
/// project carry the same values.
/// </summary>
internal static class PdfPalette
{
    // Darks
    public static readonly XColor Night = Rgb(0x0F, 0x0A, 0x1E);
    public static readonly XColor Night2 = Rgb(0x1A, 0x10, 0x30);
    public static readonly XColor Kohl = Rgb(0x07, 0x05, 0x0D);

    // Lapis
    public static readonly XColor Lapis = Rgb(0x1F, 0x3A, 0x93);
    public static readonly XColor LapisLight = Rgb(0x2B, 0x4C, 0xB8);
    public static readonly XColor LapisGlow = Rgb(0x4C, 0x6F, 0xE0);

    // Gold
    public static readonly XColor Gold = Rgb(0xE6, 0xC4, 0x76);
    public static readonly XColor GoldDeep = Rgb(0xC9, 0xA1, 0x4A);
    public static readonly XColor GoldPale = Rgb(0xF3, 0xDF, 0xA2);
    public static readonly XColor GoldShadow = Rgb(0x8A, 0x6A, 0x1F);

    // Carnelian
    public static readonly XColor Carnelian = Rgb(0xB6, 0x40, 0x2E);
    public static readonly XColor CarnelianLight = Rgb(0xD9, 0x63, 0x4E);

    // Lights and ink
    public static readonly XColor Ivory = Rgb(0xF4, 0xEB, 0xD9);
    public static readonly XColor Parchment = Rgb(0xEF, 0xE3, 0xC8);
    public static readonly XColor Ink = Rgb(0x2A, 0x1E, 0x12);

    public static readonly XColor Malachite = Rgb(0x2E, 0x8B, 0x6B);

    public static XColor Rgb(int r, int g, int b) => XColor.FromArgb(r, g, b);

    /// <summary>Same colour with an explicit alpha (0..1). Safe for solid brushes and pens.</summary>
    public static XColor Alpha(this XColor color, double alpha)
    {
        var a = (int)Math.Round(Math.Clamp(alpha, 0, 1) * 255);
        return XColor.FromArgb(a, (int)color.R, (int)color.G, (int)color.B);
    }

    /// <summary>
    /// Opaque blend of <paramref name="fore"/> over <paramref name="back"/>.
    /// Used for tinted text and for gradient stops, whose alpha channel the renderer ignores.
    /// </summary>
    public static XColor Over(this XColor fore, XColor back, double amount)
    {
        var t = Math.Clamp(amount, 0, 1);
        return XColor.FromArgb(
            (int)Math.Round(back.R + (fore.R - back.R) * t),
            (int)Math.Round(back.G + (fore.G - back.G) * t),
            (int)Math.Round(back.B + (fore.B - back.B) * t));
    }

    /// <summary>Parses "#RRGGBB", "#AARRGGBB", "RRGGBB" or a bare name; falls back to <paramref name="fallback"/>.</summary>
    public static XColor Parse(string? hex, XColor fallback)
    {
        if (string.IsNullOrWhiteSpace(hex)) return fallback;
        var s = hex.Trim().TrimStart('#');
        if (s.Length == 3)
        {
            s = string.Concat(s[0], s[0], s[1], s[1], s[2], s[2]);
        }

        if (s.Length == 6 &&
            int.TryParse(s.AsSpan(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var r6) &&
            int.TryParse(s.AsSpan(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var g6) &&
            int.TryParse(s.AsSpan(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var b6))
        {
            return XColor.FromArgb(r6, g6, b6);
        }

        if (s.Length == 8 &&
            int.TryParse(s.AsSpan(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var a8) &&
            int.TryParse(s.AsSpan(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var r8) &&
            int.TryParse(s.AsSpan(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var g8) &&
            int.TryParse(s.AsSpan(6, 2), System.Globalization.NumberStyles.HexNumber, null, out var b8))
        {
            return XColor.FromArgb(a8, r8, g8, b8);
        }

        return fallback;
    }

    /// <summary>Relative luminance, 0 (black) .. 1 (white).</summary>
    public static double Luminance(this XColor c) =>
        (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B) / 255.0;

    /// <summary>
    /// Lifts a colour until it reads clearly against the deep card body, so that a card whose
    /// accent is very dark still shows its emblem.
    /// </summary>
    public static XColor EnsureReadableOnDark(this XColor c, double minLuminance = 0.28)
    {
        var result = c;
        var guard = 0;
        while (result.Luminance() < minLuminance && guard++ < 12)
        {
            result = Rgb(255, 255, 255).Over(result, 0.16);
        }

        return result;
    }
}
