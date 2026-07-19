// TgaExporterTests.cs
//
// Golden-bytes coverage for the uncompressed 32-bit TGA exporter.

using System.Collections.Generic;
using System.IO;
using System.Text;
using Pinta.Brix.Engine;

namespace Pinta.Brix.FileFormats.Tests;

public class TgaExporterTests
{
	[Fact]
	public void Export_writes_expected_golden_bytes ()
	{
		//Arrange - 2x2: red, green (top row), blue, white (bottom row).
		Document document = TestDocuments.Create (2, 2);
		Layer layer = document.Layers.AddNewLayer ("Background");
		TestDocuments.SetPixels (layer, TestDocuments.Quad2x2 ());

		TgaExporter exporter = new ();
		string file = TestDocuments.TempFile ("tga");

		List<byte> expected = [
			// --- 18-byte TGA header (little endian)
			17,     // idLength: "Created by Pinta" + BinaryWriter length prefix
			0,      // cmapType: no color map
			2,      // imageType: uncompressed RGB
			0, 0,   // cmapIndex
			0, 0,   // cmapLength
			0,      // cmapEntrySize
			0, 0,   // xOrigin
			0, 0,   // yOrigin
			2, 0,   // imageWidth
			2, 0,   // imageHeight
			32,     // pixelDepth
			8,      // imageDesc: 32-bit, lower-left origin
			// --- image ID field (BinaryWriter string: 7-bit length prefix + bytes)
			16,
			.. Encoding.ASCII.GetBytes ("Created by Pinta"),
			// --- pixel data, bottom row first, BGRA little endian
			255, 0, 0, 255,       // blue
			255, 255, 255, 255,   // white
			0, 0, 255, 255,       // red
			0, 255, 0, 255,       // green
		];

		try {
			//Act
			exporter.Export (document, file);

			//Assert
			Assert.Equal (expected.ToArray (), File.ReadAllBytes (file));
		} finally {
			TestDocuments.DeleteQuietly (file);
		}
	}
}
