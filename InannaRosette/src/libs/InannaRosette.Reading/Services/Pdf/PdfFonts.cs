using CodeBrix.PdfDocuments.Drawing;
using CodeBrix.PdfDocuments.Fonts;

namespace InannaRosette.Reading.Services.Pdf;

/// <summary>
/// Registers this library's embedded OFL-licensed Merriweather faces with the CodeBrix.PdfDocuments
/// font system, under two stable family aliases.
/// </summary>
/// <remarks>
/// The faces travel inside the assembly as embedded resources, so the resolver never touches the
/// file system and the report looks identical on every platform regardless of what is installed on
/// the OS. Registering per FACE name before the first <see cref="XFont"/> is constructed makes the
/// outcome deterministic; whatever is resolved is embedded in the PDF as a glyph subset
/// automatically.
/// </remarks>
public static class PdfFonts
{
    /// <summary>Family alias for the text faces (Regular / Bold / Italic / BoldItalic).</summary>
    public const string Serif = "InannaSerif";

    /// <summary>Family alias for the quieter faces used for captions and invocations.</summary>
    public const string SerifLight = "InannaSerifLight";

    private const string ResourcePrefix = "InannaRosette.Reading.Fonts.";

    private static readonly object Gate = new();
    private static bool _registered;

    /// <summary>Registers the font resolvers exactly once per process. Safe to call from anywhere.</summary>
    public static void EnsureRegistered()
    {
        if (_registered) { return; }
        lock (Gate)
        {
            if (_registered) { return; }

            var assembly = typeof(PdfFonts).Assembly;

            var serifFaces = new[]
            {
                $"{Serif}-Regular", $"{Serif}-Bold", $"{Serif}-Italic", $"{Serif}-BoldItalic"
            };
            var serifResolver = new EmbeddedFontResolver(
                fontFamilyName: Serif,
                fontFaceResources:
                [
                    new EmbeddedResourceFontFace(FaceName: $"{Serif}-Regular", EmbeddedResourceName: $"{ResourcePrefix}Merriweather-Regular.ttf"),
                    new EmbeddedResourceFontFace(FaceName: $"{Serif}-Bold", EmbeddedResourceName: $"{ResourcePrefix}Merriweather-Bold.ttf"),
                    new EmbeddedResourceFontFace(FaceName: $"{Serif}-Italic", EmbeddedResourceName: $"{ResourcePrefix}Merriweather-Italic.ttf"),
                    new EmbeddedResourceFontFace(FaceName: $"{Serif}-BoldItalic", EmbeddedResourceName: $"{ResourcePrefix}Merriweather-BoldItalic.ttf")
                ],
                fontEmbeddedResourceAssembly: assembly);

            //The light family deliberately maps its "Bold" face to Merriweather Medium: the
            //  resolver matches faces by looking for "bold"/"italic" in the face NAME, and a
            //  bolder-but-still-quiet weight is what the captions and invocations want.
            var lightFaces = new[]
            {
                $"{SerifLight}-Regular", $"{SerifLight}-Bold", $"{SerifLight}-Italic", $"{SerifLight}-BoldItalic"
            };
            var lightResolver = new EmbeddedFontResolver(
                fontFamilyName: SerifLight,
                fontFaceResources:
                [
                    new EmbeddedResourceFontFace(FaceName: $"{SerifLight}-Regular", EmbeddedResourceName: $"{ResourcePrefix}Merriweather-Light.ttf"),
                    new EmbeddedResourceFontFace(FaceName: $"{SerifLight}-Bold", EmbeddedResourceName: $"{ResourcePrefix}Merriweather-Medium.ttf"),
                    new EmbeddedResourceFontFace(FaceName: $"{SerifLight}-Italic", EmbeddedResourceName: $"{ResourcePrefix}Merriweather-LightItalic.ttf"),
                    new EmbeddedResourceFontFace(FaceName: $"{SerifLight}-BoldItalic", EmbeddedResourceName: $"{ResourcePrefix}Merriweather-MediumItalic.ttf")
                ],
                fontEmbeddedResourceAssembly: assembly);

            //MetaFontResolver routes family-name lookups (ResolveTypeface) via any registered
            //  resolver whose DefaultFontName matches, but face-name lookups (GetFont) require
            //  a registration per face name.
            foreach (var face in serifFaces)
            {
                MetaFontResolver.Instance.RegisterFontResolver(face, serifResolver);
            }
            foreach (var face in lightFaces)
            {
                MetaFontResolver.Instance.RegisterFontResolver(face, lightResolver);
            }

            _registered = true;
        }
    }
}
