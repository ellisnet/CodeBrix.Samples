using System.Threading;
using CodeBrix.PdfDocuments.Fonts;
using GoddessTempleDiscovery.Assets;

namespace GoddessTempleDiscovery.Game.Journal.Pdf;

/// <summary>
/// Registers the Assets library's embedded faces with the CodeBrix.PdfDocuments font system, the InannaRosette way:
/// Merriweather under one family alias (its Bold-Italic face maps to Bold, which the set does not carry) and Noto
/// Sans Cuneiform under another. Whatever a page uses is embedded in the PDF as a subset.
/// </summary>
public static class JournalFonts
{
    /// <summary>The Merriweather family alias.</summary>
    public const string Serif = "GoddessTempleSerif";

    /// <summary>The Noto Sans Cuneiform family alias.</summary>
    public const string Cuneiform = "GoddessTempleCuneiform";

    private static readonly Lock Gate = new Lock();
    private static bool _registered;

    /// <summary>Registers the resolvers once per process. Safe to call from anywhere.</summary>
    public static void EnsureRegistered()
    {
        if (_registered)
        {
            return;
        }

        lock (Gate)
        {
            if (_registered)
            {
                return;
            }

            var assembly = FontAssets.Assembly;
            var serifFaces = new[] { Serif + "-Regular", Serif + "-Bold", Serif + "-Italic", Serif + "-BoldItalic" };
            var serif = new EmbeddedFontResolver(
                fontFamilyName: Serif,
                fontFaceResources:
                [
                    new EmbeddedResourceFontFace(FaceName: Serif + "-Regular", EmbeddedResourceName: FontAssets.MerriweatherRegular),
                    new EmbeddedResourceFontFace(FaceName: Serif + "-Bold", EmbeddedResourceName: FontAssets.MerriweatherBold),
                    new EmbeddedResourceFontFace(FaceName: Serif + "-Italic", EmbeddedResourceName: FontAssets.MerriweatherItalic),
                    new EmbeddedResourceFontFace(FaceName: Serif + "-BoldItalic", EmbeddedResourceName: FontAssets.MerriweatherBold),
                ],
                fontEmbeddedResourceAssembly: assembly);
            var cuneiformFaces = new[] { Cuneiform + "-Regular", Cuneiform + "-Bold", Cuneiform + "-Italic", Cuneiform + "-BoldItalic" };
            var cuneiform = new EmbeddedFontResolver(
                fontFamilyName: Cuneiform,
                fontFaceResources:
                [
                    new EmbeddedResourceFontFace(FaceName: Cuneiform + "-Regular", EmbeddedResourceName: FontAssets.NotoSansCuneiform),
                    new EmbeddedResourceFontFace(FaceName: Cuneiform + "-Bold", EmbeddedResourceName: FontAssets.NotoSansCuneiform),
                    new EmbeddedResourceFontFace(FaceName: Cuneiform + "-Italic", EmbeddedResourceName: FontAssets.NotoSansCuneiform),
                    new EmbeddedResourceFontFace(FaceName: Cuneiform + "-BoldItalic", EmbeddedResourceName: FontAssets.NotoSansCuneiform),
                ],
                fontEmbeddedResourceAssembly: assembly);

            foreach (var face in serifFaces)
            {
                MetaFontResolver.Instance.RegisterFontResolver(face, serif);
            }

            foreach (var face in cuneiformFaces)
            {
                MetaFontResolver.Instance.RegisterFontResolver(face, cuneiform);
            }

            _registered = true;
        }
    }
}
