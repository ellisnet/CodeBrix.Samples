using System;
using System.IO;
using System.Text;
using Pinta.Brix.Engine.Drawing;
using SkiaSharp;

namespace Pinta.Brix.Engine.Tests;

internal static class Utilities
{
	//was previously: Gio/GdkPixbuf/Cairo/Gdk module initialization in a static
	//constructor; the Pinta.Brix engine is engine-pure and needs none of it.

	/// <returns>
	/// <see langword="true"/> if the files with these file names
	/// are byte-for-byte the same, <see langword="false"/> if not
	/// </returns>
	internal static bool AreFilesEqual (string fileName1, string fileName2)
	{
		//was previously: chunked Gio.DataInputStream comparison
		ReadOnlySpan<byte> bytes1 = File.ReadAllBytes (fileName1);
		ReadOnlySpan<byte> bytes2 = File.ReadAllBytes (fileName2);
		return bytes1.SequenceEqual (bytes2);
	}

	internal static string GetAssetPath (string fileName)
	{
		const string ASSETS_FOLDER = "Assets";
		string assemblyPath = System.IO.Path.GetDirectoryName (typeof (Utilities).Assembly.Location)!;
		return System.IO.Path.Combine (assemblyPath, ASSETS_FOLDER, fileName);
	}

	public static ImageSurface LoadImage (string imageFilePath)
	{
		//was previously: GdkPixbuf decode drawn onto a Cairo surface; now a
		//SkiaSharp decode converted straight into the surface's premultiplied
		//BGRA32 pixel memory.
		using SKBitmap decoded = SKBitmap.Decode (imageFilePath)
			?? throw new InvalidOperationException ($"Could not decode image file: {imageFilePath}");
		ImageSurface surf = CairoExtensions.CreateImageSurface (Format.Argb32, decoded.Width, decoded.Height); // Not disposing because it will be returned
		SKBitmap destination = surf.Bitmap;
		using SKPixmap decodedPixels = decoded.PeekPixels ();
		if (!decodedPixels.ReadPixels (destination.Info, destination.GetPixels (), destination.RowBytes))
			throw new InvalidOperationException ($"Could not convert pixels of image file: {imageFilePath}");
		surf.MarkDirty ();
		return surf;
	}

	public static void CompareImages (
		ImageSurface result,
		ImageSurface expected,
		int tolerance = 1)
	{
		Assert.Equal (expected.GetSize (), result.GetSize ());

		ReadOnlySpan<ColorBgra> result_pixels = result.GetReadOnlyPixelData ();
		ReadOnlySpan<ColorBgra> expected_pixels = expected.GetReadOnlyPixelData ();

		int diffs = 0;
		StringBuilder details = new ();
		for (int i = 0; i < result_pixels.Length; ++i) {

			if (ColorBgra.ColorsWithinTolerance (result_pixels[i], expected_pixels[i], tolerance))
				continue;

			++diffs;

			// Display info about the first few failures.
			if (diffs <= 10)
				details.AppendLine ($"Difference at pixel {i}, got {result_pixels[i]} vs {expected_pixels[i]}, diff. of {ColorBgra.ColorDifference (result_pixels[i], expected_pixels[i])}");
		}

		Assert.True (diffs == 0, $"{diffs} pixel(s) differed by more than the tolerance of {tolerance}:{Environment.NewLine}{details}");
	}

	public static void TestBlendOp (
		UserBlendOp blendOp,
		string sourceExpected,
		string? saveImageName = null,
		string sourceA = "visual_a.png",
		string sourceB = "visual_b.png")
	{
		string pathA = Utilities.GetAssetPath (sourceA);
		string pathB = Utilities.GetAssetPath (sourceB);
		string pathExpected = Utilities.GetAssetPath (sourceExpected);
		using ImageSurface loadedA = Utilities.LoadImage (pathA);
		using ImageSurface loadedB = Utilities.LoadImage (pathB);
		using ImageSurface expectedOutput = Utilities.LoadImage (pathExpected);
		using ImageSurface result = CairoExtensions.CreateImageSurface (Format.Argb32, loadedA.Width, loadedB.Height);
		blendOp.Apply (result, loadedB, loadedA);

		// For debugging, optionally save out the result to a file.
		if (saveImageName != null)
			SaveImage (result, saveImageName);

		Utilities.CompareImages (expectedOutput, result);
	}

	//was previously: result.ToPixbuf ().Savev (...); now a SkiaSharp PNG encode
	private static void SaveImage (ImageSurface surface, string filePath)
	{
		using SKImage snapshot = SKImage.FromBitmap (surface.Bitmap);
		using SKData encoded = snapshot.Encode (SKEncodedImageFormat.Png, 100);
		using FileStream stream = File.Create (filePath);
		encoded.SaveTo (stream);
	}
}
