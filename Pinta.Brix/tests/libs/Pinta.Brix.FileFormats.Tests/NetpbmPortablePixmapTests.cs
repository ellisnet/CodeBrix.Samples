// NetpbmPortablePixmapTests.cs
//
// Golden-bytes coverage for the text-based "P3" portable pixmap exporter,
// plus a save -> load round trip.

using System.IO;
using System.Text;
using Pinta.Brix.Engine;

namespace Pinta.Brix.FileFormats.Tests;

public class NetpbmPortablePixmapTests
{
	private static Document CreateQuadDocument ()
	{
		Document document = TestDocuments.Create (2, 2);
		Layer layer = document.Layers.AddNewLayer ("Background");
		TestDocuments.SetPixels (layer, TestDocuments.Quad2x2 ());
		return document;
	}

	[Fact]
	public void Export_writes_expected_golden_bytes ()
	{
		//Arrange
		Document document = CreateQuadDocument ();
		NetpbmPortablePixmap format = new ();
		string file = TestDocuments.TempFile ("ppm");

		const string EXPECTED =
			"P3\n" +
			"2 2\n" +
			"255\n" +
			"255   0   0     0 255   0\n" +
			"  0   0 255   255 255 255\n";

		try {
			//Act
			format.Export (document, file);

			//Assert - byte-for-byte: ASCII, "\n" newlines on every platform.
			Assert.Equal (Encoding.ASCII.GetBytes (EXPECTED), File.ReadAllBytes (file));
		} finally {
			TestDocuments.DeleteQuietly (file);
		}
	}

	[Fact]
	public void Export_then_Import_round_trips_opaque_pixels ()
	{
		//Arrange
		Document original = CreateQuadDocument ();
		NetpbmPortablePixmap format = new ();
		string file = TestDocuments.TempFile ("ppm");

		try {
			//Act
			format.Export (original, file);
			Document imported = format.Import (file);

			//Assert
			Assert.Equal (new Size (2, 2), imported.ImageSize);
			Assert.Equal ("ppm", imported.FileType);
			Assert.Equal (TestDocuments.Quad2x2 (), TestDocuments.GetPixels (imported.Layers[0]));
		} finally {
			TestDocuments.DeleteQuietly (file);
		}
	}

	[Fact]
	public void Import_throws_FormatException_for_wrong_magic ()
	{
		//Arrange
		string file = TestDocuments.TempFile ("ppm");
		File.WriteAllText (file, "P6\n2 2\n255\n");
		NetpbmPortablePixmap format = new ();

		try {
			//Act/Assert
			Assert.Throws<System.FormatException> (() => format.Import (file));
		} finally {
			TestDocuments.DeleteQuietly (file);
		}
	}
}
