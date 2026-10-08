using System;
using System.IO;

namespace GoddessTempleDiscovery.Assets;

/// <summary>
/// The fonts the game draws its cards and its journal with, embedded so the pictures look the same on every
/// machine: Merriweather (Regular, Bold, Italic) for titles and text, and Noto Sans Cuneiform for the cuneiform
/// lines. Both are under the SIL Open Font License 1.1; the license texts travel beside them.
/// </summary>
public static class FontAssets
{
    private const string Prefix = "GoddessTempleDiscovery.Assets.Fonts.";

    /// <summary>The resource name of Merriweather Regular.</summary>
    public const string MerriweatherRegular = Prefix + "Merriweather-Regular.ttf";

    /// <summary>The resource name of Merriweather Bold.</summary>
    public const string MerriweatherBold = Prefix + "Merriweather-Bold.ttf";

    /// <summary>The resource name of Merriweather Italic.</summary>
    public const string MerriweatherItalic = Prefix + "Merriweather-Italic.ttf";

    /// <summary>The resource name of Noto Sans Cuneiform Regular.</summary>
    public const string NotoSansCuneiform = Prefix + "NotoSansCuneiform-Regular.ttf";

    /// <summary>The assembly the font resources live in (for the PDF font resolver).</summary>
    public static System.Reflection.Assembly Assembly => typeof(FontAssets).Assembly;

    /// <summary>Opens a font by its resource name as a caller-owned stream.</summary>
    /// <param name="resourceName">One of the resource-name constants of this class.</param>
    public static Stream Open(string resourceName) =>
        Assembly.GetManifestResourceStream(resourceName)
        ?? throw new InvalidOperationException($"The font resource '{resourceName}' is missing from the assembly.");

    /// <summary>Reads a font's bytes.</summary>
    /// <param name="resourceName">One of the resource-name constants of this class.</param>
    public static byte[] ReadBytes(string resourceName)
    {
        using var stream = Open(resourceName);
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    /// <summary>Reads the license text of a font family: "Merriweather" or "NotoSansCuneiform".</summary>
    /// <param name="family">The family name as it appears in the license file name.</param>
    public static string ReadLicense(string family)
    {
        using var stream = Assembly.GetManifestResourceStream(Prefix + "OFL-" + family + ".txt")
                           ?? throw new InvalidOperationException($"No license text is embedded for '{family}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
