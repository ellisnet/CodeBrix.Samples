// SkiaCodecFormatTests.cs
//
// Round-trip coverage for the SkiaSharp-codec-backed formats: PNG must be
// lossless; JPEG is lossy so only dimensions and approximate pixel values are
// asserted, plus the compression-quality event plumbing.

using System;
using Pinta.Brix.Engine;
using SkiaSharp;

namespace Pinta.Brix.FileFormats.Tests;

public class SkiaCodecFormatTests
{
	[Fact]
	public void Export_then_Import_round_trips_png_exactly ()
	{
		//Arrange - opaque, transparent and valid-premultiplied pixels.
		ColorBgra[] pixels = [
			ColorBgra.FromBgra (b: 0, g: 0, r: 255, a: 255),
			ColorBgra.FromBgra (b: 10, g: 20, r: 30, a: 128),
			ColorBgra.Zero,
			ColorBgra.FromBgra (b: 255, g: 255, r: 255, a: 255),
			ColorBgra.FromBgra (b: 64, g: 32, r: 16, a: 64),
			ColorBgra.FromBgra (b: 100, g: 200, r: 50, a: 255),
		];
		Document original = TestDocuments.Create (3, 2);
		Layer layer = original.Layers.AddNewLayer ("Background");
		TestDocuments.SetPixels (layer, pixels);

		SkiaCodecFormat format = new ("png", SKEncodedImageFormat.Png);
		string file = TestDocuments.TempFile ("png");

		try {
			//Act
			format.Export (original, file);
			Document imported = format.Import (file);

			//Assert
			Assert.Equal (new Size (3, 2), imported.ImageSize);
			Assert.Equal ("png", imported.FileType);
			Assert.Equal (pixels, TestDocuments.GetPixels (imported.Layers[0]));
		} finally {
			TestDocuments.DeleteQuietly (file);
		}
	}

	[Fact]
	public void Import_throws_FormatException_for_non_image_file ()
	{
		//Arrange
		string file = TestDocuments.TempFile ("png");
		System.IO.File.WriteAllText (file, "not an image");
		SkiaCodecFormat format = new ("png", SKEncodedImageFormat.Png);

		try {
			//Act/Assert
			Assert.Throws<FormatException> (() => format.Import (file));
		} finally {
			TestDocuments.DeleteQuietly (file);
		}
	}

	[Fact]
	public void Export_throws_NotSupportedException_without_an_encode_format ()
	{
		//Arrange
		Document document = TestDocuments.Create (2, 2);
		Layer layer = document.Layers.AddNewLayer ("Background");
		TestDocuments.SetPixels (layer, TestDocuments.Quad2x2 ());
		SkiaCodecFormat importOnly = new ("bmp");
		string file = TestDocuments.TempFile ("bmp");

		try {
			//Act/Assert
			Assert.Throws<NotSupportedException> (() => importOnly.Export (document, file));
		} finally {
			TestDocuments.DeleteQuietly (file);
		}
	}
}

public class JpegFormatTests
{
	private const int WIDTH = 8;
	private const int HEIGHT = 8;

	private static Document CreateUniformDocument (ColorBgra color)
	{
		Document document = TestDocuments.Create (WIDTH, HEIGHT);
		Layer layer = document.Layers.AddNewLayer ("Background");
		ColorBgra[] pixels = new ColorBgra[WIDTH * HEIGHT];
		Array.Fill (pixels, color);
		TestDocuments.SetPixels (layer, pixels);
		return document;
	}

	[Fact]
	public void Export_then_Import_round_trips_jpeg_approximately ()
	{
		//Arrange
		ColorBgra color = ColorBgra.FromBgra (b: 50, g: 100, r: 200, a: 255);
		Document original = CreateUniformDocument (color);
		JpegFormat format = new ();
		string file = TestDocuments.TempFile ("jpg");

		try {
			//Act
			format.Export (original, file);
			Document imported = format.Import (file);

			//Assert - JPEG is lossy: dimensions must match and every pixel must
			//land close to the original color.
			Assert.Equal (new Size (WIDTH, HEIGHT), imported.ImageSize);
			Assert.Equal ("jpeg", imported.FileType);
			foreach (ColorBgra pixel in TestDocuments.GetPixels (imported.Layers[0])) {
				Assert.Equal (255, pixel.A);
				Assert.InRange (pixel.B, color.B - 8, color.B + 8);
				Assert.InRange (pixel.G, color.G - 8, color.G + 8);
				Assert.InRange (pixel.R, color.R - 8, color.R + 8);
			}
		} finally {
			TestDocuments.DeleteQuietly (file);
		}
	}

	[Fact]
	public void Export_raises_ModifyCompression_and_honors_cancel ()
	{
		//Arrange
		Document document = CreateUniformDocument (ColorBgra.FromBgra (10, 20, 30, 255));
		JpegFormat format = new ();
		string file = TestDocuments.TempFile ("jpg");
		int seenQuality = -1;

		void OnModifyCompression (object? sender, ModifyCompressionEventArgs args)
		{
			seenQuality = args.Quality;
			args.Cancel = true;
		}

		JpegFormat.ModifyCompression += OnModifyCompression;
		try {
			//Act/Assert - a canceling handler aborts the save, upstream-style.
			Assert.Throws<OperationCanceledException> (() => format.Export (document, file));
			Assert.InRange (seenQuality, 1, 100);
			Assert.False (System.IO.File.Exists (file));
		} finally {
			JpegFormat.ModifyCompression -= OnModifyCompression;
			TestDocuments.DeleteQuietly (file);
		}
	}
}
