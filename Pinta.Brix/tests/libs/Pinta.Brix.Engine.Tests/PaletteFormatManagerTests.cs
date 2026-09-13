// PaletteFormatManagerTests.cs
//
// The palette registry is the other half of the same story: it decides what
// the load and save palette dialogs offer, and which format a chosen file name
// resolves to.

using System.Collections.Generic;
using System.Linq;
using Pinta.Brix.Engine;
using SilverAssertions;

namespace Pinta.Brix.Engine.Tests;

public class PaletteFormatManagerTests
{
	[Fact]
	public void GetLoadFilters_offers_one_entry_per_readable_format ()
	{
		//Arrange
		PaletteFormatManager formats = new ();

		//Act
		IReadOnlyList<FileDialogFilter> filters = formats.GetLoadFilters ();

		//Assert
		filters.Count.Should ().Be (formats.Formats.Count (f => f.Loader is not null));
	}

	[Fact]
	public void GetSaveFilters_offers_one_entry_per_writable_format ()
	{
		//Arrange
		PaletteFormatManager formats = new ();

		//Act
		IReadOnlyList<FileDialogFilter> filters = formats.GetSaveFilters ();

		//Assert
		filters.Count.Should ().Be (formats.Formats.Count (f => f.Saver is not null));
	}

	[Fact]
	public void GetLoadExtensions_carries_both_spellings_of_each_extension ()
	{
		//Arrange
		PaletteFormatManager formats = new ();

		//Act
		IReadOnlyList<string> extensions = formats.GetLoadExtensions ();

		//Assert
		extensions.Should ().Contain (".gpl");
		extensions.Should ().Contain (".GPL");
	}

	[Fact]
	public void GetLoadExtensions_lists_each_extension_once ()
	{
		//Arrange
		PaletteFormatManager formats = new ();

		//Act
		IReadOnlyList<string> extensions = formats.GetLoadExtensions ();

		//Assert
		extensions.Distinct ().Count ().Should ().Be (extensions.Count);
	}

	[Theory]
	[InlineData ("colors.gpl", "gpl")]
	[InlineData ("COLORS.GPL", "gpl")]
	[InlineData ("colors.txt", "txt")]
	[InlineData ("colors.pal", "pal")]
	public void GetFormatByFilename_matches_either_case (string fileName, string expectedExtension)
	{
		//Arrange
		PaletteFormatManager formats = new ();

		//Act
		PaletteDescriptor? descriptor = formats.GetFormatByFilename (fileName);

		//Assert
		descriptor.Should ().NotBeNull ();
		descriptor!.Extensions.Should ().Contain (expectedExtension);
	}

	[Fact]
	public void GetFormatByFilename_returns_null_for_an_unknown_extension ()
	{
		//Arrange
		PaletteFormatManager formats = new ();

		//Act
		PaletteDescriptor? descriptor = formats.GetFormatByFilename ("colors.nope");

		//Assert
		descriptor.Should ().BeNull ();
	}
}
