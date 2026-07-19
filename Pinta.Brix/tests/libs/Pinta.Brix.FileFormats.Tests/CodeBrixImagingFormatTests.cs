// CodeBrixImagingFormatTests.cs
//
// Round-trip coverage for the CodeBrix.Imaging-backed exporters. BMP and GIF
// are read back through the SkiaSharp importer (the same pairing the format
// registration uses); TIFF - which SkiaSharp cannot decode - is read back
// through CodeBrix.Imaging itself.

using Pinta.Brix.Engine;

namespace Pinta.Brix.FileFormats.Tests;

public class CodeBrixImagingFormatTests
{
	private static Document CreateQuadDocument ()
	{
		Document document = TestDocuments.Create (2, 2);
		Layer layer = document.Layers.AddNewLayer ("Background");
		TestDocuments.SetPixels (layer, TestDocuments.Quad2x2 ());
		return document;
	}

	private static void AssertQuadRoundTrip (IImageExporter exporter, IImageImporter importer, string extension, string expectedFileType)
	{
		//Arrange
		Document original = CreateQuadDocument ();
		string file = TestDocuments.TempFile (extension);

		try {
			//Act
			exporter.Export (original, file);
			Document imported = importer.Import (file);

			//Assert
			Assert.Equal (new Size (2, 2), imported.ImageSize);
			Assert.Equal (expectedFileType, imported.FileType);
			Assert.Equal (TestDocuments.Quad2x2 (), TestDocuments.GetPixels (imported.Layers[0]));
		} finally {
			TestDocuments.DeleteQuietly (file);
		}
	}

	[Fact]
	public void Bmp_export_round_trips_through_skia_import ()
		=> AssertQuadRoundTrip (
			CodeBrixImagingFormat.CreateBmp (),
			new SkiaCodecFormat ("bmp"),
			"bmp",
			"bmp");

	[Fact]
	public void Gif_export_round_trips_through_skia_import ()
		=> AssertQuadRoundTrip (
			CodeBrixImagingFormat.CreateGif (),
			new SkiaCodecFormat ("gif"),
			"gif",
			"gif");

	[Fact]
	public void Tiff_export_round_trips_through_imaging_import ()
	{
		CodeBrixImagingFormat tiff = CodeBrixImagingFormat.CreateTiff ();
		AssertQuadRoundTrip (tiff, tiff, "tif", "tiff");
	}
}
