using System;
using System.Text;
using Pinta.Brix.Engine;
using Pinta.Brix.Engine.Drawing;
using SkiaSharp;

//was previously: namespace Pinta.Effects.Tests;
namespace Pinta.Brix.Effects.Tests;

internal static class Utilities
{
	// Pinta.Brix note: upstream initialized the Gio/GdkPixbuf/Cairo/Gdk native
	// modules here and loaded/saved images through GdkPixbuf. The port's
	// drawing layer is SkiaSharp-backed, so images are decoded and encoded
	// with SkiaSharp directly and no native toolkit setup is needed.

	public static IServiceProvider CreateMockServices ()
	{
		Size imageSize = new (250, 250);

		ServiceManager manager = new ();
		manager.AddService<IPaletteService> (new MockPalette ());
		manager.AddService<IChromeService> (new MockChromeManager ());
		manager.AddService<IWorkspaceService> (new MockWorkspaceService (imageSize));
		manager.AddService<ISystemService> (new MockSystemService ());
		manager.AddService<ILivePreview> (new MockLivePreview (new RectangleI (0, 0, imageSize.Width, imageSize.Height)));
		return manager;
	}

	public static ImageSurface LoadImage (string image_name)
	{
		string assembly_path = System.IO.Path.GetDirectoryName (typeof (Utilities).Assembly.Location)!;
		string file_path = System.IO.Path.Combine (assembly_path, "Assets", image_name);

		// Decode straight into the surface's pixel format: premultiplied BGRA32.
		using SKCodec codec = SKCodec.Create (file_path) ?? throw new InvalidOperationException ($"Could not open image file {file_path}");
		SKImageInfo target = new (codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
		using SKBitmap decoded = SKBitmap.Decode (codec, target) ?? throw new InvalidOperationException ($"Could not decode image file {file_path}");

		ImageSurface surf = CairoExtensions.CreateImageSurface (Format.Argb32, decoded.Width, decoded.Height); // Not disposing because it will be returned

		ReadOnlySpan<byte> src = decoded.GetPixelSpan ();
		Span<byte> dst = surf.GetData ();
		int row_length = decoded.Width * 4;
		for (int y = 0; y < decoded.Height; ++y)
			src.Slice (y * decoded.RowBytes, row_length).CopyTo (dst.Slice (y * row_length, row_length));

		surf.MarkDirty ();
		return surf;
	}

	public static void SaveImage (ImageSurface surface, string file_path)
	{
		using SKImage image = SKImage.FromBitmap (surface.Bitmap) ?? throw new InvalidOperationException ("Could not snapshot surface");
		using SKData data = image.Encode (SKEncodedImageFormat.Png, 100);
		using System.IO.FileStream fs = System.IO.File.Create (file_path);
		data.SaveTo (fs);
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

		if (diffs != 0)
			Assert.Fail ($"{diffs} pixel(s) differ beyond tolerance {tolerance}:{Environment.NewLine}{details}");
	}

	public static void TestEffect (
		BaseEffect effect,
		string result_image_name,
		string? save_image_name = null,
		string source_image_name = "input.png")
	{
		using ImageSurface source = Utilities.LoadImage (source_image_name);
		using ImageSurface result = CairoExtensions.CreateImageSurface (Format.Argb32, source.Width, source.Height);
		using ImageSurface expected = LoadImage (result_image_name);

		effect.Render (source, result, [source.GetBounds ()]);

		// For debugging, optionally save out the result to a file.
		if (save_image_name != null)
			SaveImage (result, save_image_name);

		CompareImages (result, expected);
	}
}
