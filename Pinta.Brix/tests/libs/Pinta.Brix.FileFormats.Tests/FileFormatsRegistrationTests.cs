// FileFormatsRegistrationTests.cs
//
// Coverage for the format registration entry point: every advertised format
// resolves by extension with the expected import/export capabilities.

using Pinta.Brix.Engine;

namespace Pinta.Brix.FileFormats.Tests;

public class FileFormatsRegistrationTests
{
	private static ImageConverterManager CreateRegisteredManager ()
	{
		ImageConverterManager formats = new (PintaCore.Settings);
		FileFormatsRegistration.RegisterAll (formats);
		return formats;
	}

	[Theory]
	[InlineData ("png")]
	[InlineData ("jpg")]
	[InlineData ("jpeg")]
	[InlineData ("webp")]
	[InlineData ("bmp")]
	[InlineData ("gif")]
	[InlineData ("tif")]
	[InlineData ("tiff")]
	[InlineData ("ora")]
	[InlineData ("ppm")]
	public void RegisterAll_registers_import_and_export_for_extension (string extension)
	{
		//Arrange
		ImageConverterManager formats = CreateRegisteredManager ();

		//Act
		FormatDescriptor? descriptor = formats.GetFormatByExtension (extension);

		//Assert
		Assert.NotNull (descriptor);
		Assert.True (descriptor.IsImportAvailable ());
		Assert.True (descriptor.IsExportAvailable ());
	}

	[Fact]
	public void RegisterAll_registers_tga_as_export_only ()
	{
		//Arrange
		ImageConverterManager formats = CreateRegisteredManager ();

		//Act
		FormatDescriptor? descriptor = formats.GetFormatByExtension ("tga");

		//Assert
		Assert.NotNull (descriptor);
		Assert.False (descriptor.IsImportAvailable ());
		Assert.True (descriptor.IsExportAvailable ());
	}

	[Fact]
	public void RegisterAll_marks_only_ora_as_supporting_layers ()
	{
		//Arrange
		ImageConverterManager formats = CreateRegisteredManager ();

		//Assert
		foreach (FormatDescriptor descriptor in formats.Formats)
			Assert.Equal (descriptor.Extensions.Contains ("ora"), descriptor.SupportsLayers);
	}

	[Fact]
	public void GetFormatByExtension_is_case_insensitive_for_jpeg_variants ()
	{
		//Arrange
		ImageConverterManager formats = CreateRegisteredManager ();

		//Act
		FormatDescriptor? jpg = formats.GetFormatByExtension ("JPG");
		FormatDescriptor? jpeg = formats.GetFormatByExtension (".jpeg");

		//Assert - the four jpg/jpeg spellings share one descriptor.
		Assert.NotNull (jpg);
		Assert.Same (jpg, jpeg);
		Assert.IsType<JpegFormat> (jpg.Exporter);
	}

	[Fact]
	public void RegisterAll_pairs_skia_import_with_imaging_export_for_bmp_and_gif ()
	{
		//Arrange
		ImageConverterManager formats = CreateRegisteredManager ();

		//Assert - Skia decodes BMP/GIF but cannot encode them.
		foreach (string extension in new[] { "bmp", "gif" }) {
			FormatDescriptor? descriptor = formats.GetFormatByExtension (extension);
			Assert.NotNull (descriptor);
			Assert.IsType<SkiaCodecFormat> (descriptor.Importer);
			Assert.IsType<CodeBrixImagingFormat> (descriptor.Exporter);
		}
	}
}
