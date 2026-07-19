namespace Pinta.Brix.Engine.Tests;

public sealed class FileFormatTests
{
	[Theory]
	[InlineData ("sixcolorsinput.gif", "sixcolors_standard_lf.ppm")]
	[InlineData ("sixcolorsinput.gif", "sixcolors_chaotic.ppm")]
	public void Files_NotEqual (string file1, string file2)
	{
		string path1 = Utilities.GetAssetPath (file1);
		string path2 = Utilities.GetAssetPath (file2);
		Assert.False (Utilities.AreFilesEqual (path1, path2));
	}

	// Upstream's Export_NetpbmPixmap_TextBased test is not ported: the
	// Pinta.Brix engine does not include the Netpbm portable-pixmap codec
	// (image codecs are not part of the engine; ImageConverterManager starts
	// empty), so there is no exporter to exercise. The sixcolors_*.ppm assets
	// are retained for the byte-comparison test above.
}
