// OraFormatTests.cs
//
// Round-trip coverage for the OpenRaster importer/exporter: a multi-layer
// document with blend modes, opacities and hidden layers must survive
// save -> load with its layer pixel data and properties intact.

using System;
using Pinta.Brix.Engine;

namespace Pinta.Brix.FileFormats.Tests;

public class OraFormatTests
{
	private const int WIDTH = 4;
	private const int HEIGHT = 3;

	private static ColorBgra[] BackgroundPixels ()
	{
		ColorBgra[] pixels = new ColorBgra[WIDTH * HEIGHT];
		for (int i = 0; i < pixels.Length; i++)
			pixels[i] = ColorBgra.FromBgra (
				b: (byte) (i * 20),
				g: (byte) (255 - i * 20),
				r: (byte) (i * 10),
				a: 255);
		return pixels;
	}

	// Valid premultiplied semi-transparent pixels (every channel <= alpha):
	// such values survive PNG's premultiplied <-> straight conversions exactly.
	private static ColorBgra[] MiddlePixels ()
	{
		ColorBgra[] pixels = new ColorBgra[WIDTH * HEIGHT];
		for (int i = 0; i < pixels.Length; i++)
			pixels[i] = ColorBgra.FromBgra (
				b: (byte) (i * 10),
				g: (byte) (i * 5),
				r: (byte) (64 + i),
				a: (byte) (128 + i));
		return pixels;
	}

	private static ColorBgra[] TopPixels ()
	{
		ColorBgra[] pixels = new ColorBgra[WIDTH * HEIGHT];
		for (int i = 0; i < pixels.Length; i++)
			pixels[i] =
				(i % 2 == 0)
				? ColorBgra.Zero
				: ColorBgra.FromBgra (b: 255, g: (byte) (i * 15), r: 0, a: 255);
		return pixels;
	}

	private static Document CreateTestDocument ()
	{
		Document document = TestDocuments.Create (WIDTH, HEIGHT);

		UserLayer background = document.Layers.AddNewLayer ("Background");
		TestDocuments.SetPixels (background, BackgroundPixels ());

		UserLayer middle = document.Layers.AddNewLayer ("Middle");
		middle.Opacity = 0.5;
		middle.BlendMode = BlendMode.Multiply;
		TestDocuments.SetPixels (middle, MiddlePixels ());

		UserLayer top = document.Layers.AddNewLayer ("Top");
		top.Opacity = 0.75;
		top.BlendMode = BlendMode.Screen;
		top.Hidden = true;
		TestDocuments.SetPixels (top, TopPixels ());

		return document;
	}

	[Fact]
	public void Export_then_Import_round_trips_layer_pixels_and_properties ()
	{
		//Arrange
		Document original = CreateTestDocument ();
		OraFormat format = new ();
		string file = TestDocuments.TempFile ("ora");

		try {
			//Act
			format.Export (original, file);
			Document imported = format.Import (file);

			//Assert
			Assert.Equal ("ora", imported.FileType);
			Assert.Equal (new Size (WIDTH, HEIGHT), imported.ImageSize);
			Assert.Equal (3, imported.Layers.Count ());

			UserLayer background = imported.Layers[0];
			Assert.Equal ("Background", background.Name);
			Assert.Equal (1.0, background.Opacity);
			Assert.Equal (BlendMode.Normal, background.BlendMode);
			Assert.False (background.Hidden);
			Assert.Equal (BackgroundPixels (), TestDocuments.GetPixels (background));

			UserLayer middle = imported.Layers[1];
			Assert.Equal ("Middle", middle.Name);
			Assert.Equal (0.5, middle.Opacity);
			Assert.Equal (BlendMode.Multiply, middle.BlendMode);
			Assert.False (middle.Hidden);
			Assert.Equal (MiddlePixels (), TestDocuments.GetPixels (middle));

			UserLayer top = imported.Layers[2];
			Assert.Equal ("Top", top.Name);
			Assert.Equal (0.75, top.Opacity);
			Assert.Equal (BlendMode.Screen, top.BlendMode);
			Assert.True (top.Hidden);
			Assert.Equal (TopPixels (), TestDocuments.GetPixels (top));
		} finally {
			TestDocuments.DeleteQuietly (file);
		}
	}

	[Fact]
	public void Export_writes_uncompressed_mimetype_as_first_entry ()
	{
		//Arrange
		Document original = CreateTestDocument ();
		OraFormat format = new ();
		string file = TestDocuments.TempFile ("ora");

		try {
			//Act
			format.Export (original, file);

			//Assert - per the OpenRaster spec the archive starts with a
			//"mimetype" entry holding "image/openraster".
			using System.IO.Compression.ZipArchive archive = System.IO.Compression.ZipFile.OpenRead (file);
			System.IO.Compression.ZipArchiveEntry first = archive.Entries[0];
			Assert.Equal ("mimetype", first.Name);
			using System.IO.StreamReader reader = new (first.Open ());
			Assert.Equal ("image/openraster", reader.ReadToEnd ());
			Assert.NotNull (archive.GetEntry ("stack.xml"));
			Assert.NotNull (archive.GetEntry ("mergedimage.png"));
			Assert.NotNull (archive.GetEntry ("Thumbnails/thumbnail.png"));
			Assert.NotNull (archive.GetEntry ("data/layer0.png"));
			Assert.NotNull (archive.GetEntry ("data/layer1.png"));
			Assert.NotNull (archive.GetEntry ("data/layer2.png"));
		} finally {
			TestDocuments.DeleteQuietly (file);
		}
	}
}
