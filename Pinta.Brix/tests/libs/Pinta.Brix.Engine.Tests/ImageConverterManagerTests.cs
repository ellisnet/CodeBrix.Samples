// ImageConverterManagerTests.cs
//
// The registry decides what a file dialog offers: which formats appear, under
// what name, and with which extensions. Every extension is registered in both
// cases so that matching a file name is case-insensitive, and a dialog wants
// only one spelling of each - a rule that is invisible until a picker shows
// ".PNG" beside ".png".

using System.Collections.Generic;
using System.Linq;
using Pinta.Brix.Engine;
using SilverAssertions;

namespace Pinta.Brix.Engine.Tests;

public class ImageConverterManagerTests
{
	private sealed class StubImporter : IImageImporter
	{
		public Document Import (string file) => throw new System.NotSupportedException ();
	}

	private sealed class StubExporter : IImageExporter
	{
		public void Export (Document document, string file) => throw new System.NotSupportedException ();
	}

	private static ImageConverterManager CreateManager ()
	{
		ImageConverterManager formats = new (PintaCore.Settings);

		formats.RegisterFormat (new FormatDescriptor (
			"Readable", ["rdo", "RDO"], ["image/rdo"], new StubImporter (), null));
		formats.RegisterFormat (new FormatDescriptor (
			"Writable", ["wro", "WRO"], ["image/wro"], null, new StubExporter ()));
		formats.RegisterFormat (new FormatDescriptor (
			"Both", ["bth", "BTH", "bo"], ["image/bth"], new StubImporter (), new StubExporter ()));

		return formats;
	}

	[Fact]
	public void GetImportFilters_offers_only_importable_formats ()
	{
		//Arrange
		ImageConverterManager formats = CreateManager ();

		//Act
		IReadOnlyList<FileDialogFilter> filters = formats.GetImportFilters ();

		//Assert
		filters.Count.Should ().Be (2);
		filters.Any (f => f.Extensions.Contains (".wro")).Should ().BeFalse ();
	}

	[Fact]
	public void GetExportFilters_offers_only_exportable_formats ()
	{
		//Arrange
		ImageConverterManager formats = CreateManager ();

		//Act
		IReadOnlyList<FileDialogFilter> filters = formats.GetExportFilters ();

		//Assert
		filters.Count.Should ().Be (2);
		filters.Any (f => f.Extensions.Contains (".rdo")).Should ().BeFalse ();
	}

	[Fact]
	public void GetExportFilters_drops_the_upper_case_spellings ()
	{
		//Arrange
		ImageConverterManager formats = CreateManager ();

		//Act
		FileDialogFilter filter = formats.GetExportFilters ().First (f => f.Extensions.Contains (".bth"));

		//Assert
		filter.Extensions.Should ().BeEquivalentTo ([".bth", ".bo"]);
	}

	[Fact]
	public void GetExportFilters_carries_the_format_display_name ()
	{
		//Arrange
		ImageConverterManager formats = CreateManager ();
		FormatDescriptor descriptor = formats.GetFormatByExtension ("wro")!;

		//Act
		FileDialogFilter filter = formats.GetExportFilters ().First (f => f.Extensions.Contains (".wro"));

		//Assert
		filter.Name.Should ().Be (descriptor.FilterName);
	}

	[Fact]
	public void GetImportExtensions_lists_every_readable_extension_once ()
	{
		//Arrange
		ImageConverterManager formats = CreateManager ();

		//Act
		IReadOnlyList<string> extensions = formats.GetImportExtensions ();

		//Assert
		extensions.Should ().BeEquivalentTo ([".rdo", ".bth", ".bo"]);
	}

	[Fact]
	public void GetExportFilters_is_empty_before_anything_is_registered ()
	{
		//Arrange
		ImageConverterManager formats = new (PintaCore.Settings);

		//Act
		IReadOnlyList<FileDialogFilter> filters = formats.GetExportFilters ();

		//Assert
		filters.Count.Should ().Be (0);
	}
}
