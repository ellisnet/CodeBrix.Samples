// AcceleratorParserTests.cs
//
// The Command objects carry upstream's GTK accelerator strings verbatim, and
// every keyboard shortcut in the application is dispatched by parsing them.
// A silent mis-parse costs a shortcut with no other symptom, so the parser is
// the piece of the port most worth pinning down with tests.

using Pinta.Brix.Controls;
using SilverAssertions;
using Windows.System;
using Xunit;

namespace Pinta.Brix.Controls.Tests;

public class AcceleratorParserTests
{
	[Theory]
	[InlineData ("<Primary>Z", VirtualKey.Z, VirtualKeyModifiers.Control)]
	[InlineData ("<Ctrl>Y", VirtualKey.Y, VirtualKeyModifiers.Control)]
	[InlineData ("<Control>S", VirtualKey.S, VirtualKeyModifiers.Control)]
	[InlineData ("<Primary><Shift>Z", VirtualKey.Z, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift)]
	[InlineData ("<Ctrl><Alt>X", VirtualKey.X, VirtualKeyModifiers.Control | VirtualKeyModifiers.Menu)]
	public void TryParse_reads_modifiers (string accelerator, VirtualKey key, VirtualKeyModifiers modifiers)
	{
		//Act
		bool parsed = AcceleratorParser.TryParse (accelerator, out ParsedAccelerator result);

		//Assert
		parsed.Should ().BeTrue ();
		result.Key.Should ().Be (key);
		result.Modifiers.Should ().Be (modifiers);
	}

	[Theory]
	[InlineData ("Delete", VirtualKey.Delete)]
	[InlineData ("BackSpace", VirtualKey.Back)]
	[InlineData ("KP_Add", VirtualKey.Add)]
	[InlineData ("KP_Subtract", VirtualKey.Subtract)]
	[InlineData ("F1", VirtualKey.F1)]
	[InlineData ("F11", VirtualKey.F11)]
	[InlineData ("F12", VirtualKey.F12)]
	public void TryParse_reads_named_keys (string accelerator, VirtualKey key)
	{
		//Act
		bool parsed = AcceleratorParser.TryParse (accelerator, out ParsedAccelerator result);

		//Assert
		parsed.Should ().BeTrue ();
		result.Key.Should ().Be (key);
		result.Modifiers.Should ().Be (VirtualKeyModifiers.None);
	}

	[Theory]
	[InlineData ("<Primary>plus")]
	[InlineData ("<Primary>equal")]
	public void TryParse_maps_both_spellings_of_the_plus_key_to_one_key (string accelerator)
	{
		//Arrange
		//GTK matched on the produced CHARACTER, so upstream lists "plus" and
		//"equal" as separate shortcuts for the same physical key.
		AcceleratorParser.TryParse ("<Primary>plus", out ParsedAccelerator plus);

		//Act
		bool parsed = AcceleratorParser.TryParse (accelerator, out ParsedAccelerator result);

		//Assert
		parsed.Should ().BeTrue ();
		result.Key.Should ().Be (plus.Key);
	}

	[Theory]
	[InlineData ("<Primary>minus")]
	[InlineData ("<Primary>underscore")]
	public void TryParse_maps_both_spellings_of_the_minus_key_to_one_key (string accelerator)
	{
		//Arrange
		AcceleratorParser.TryParse ("<Primary>minus", out ParsedAccelerator minus);

		//Act
		bool parsed = AcceleratorParser.TryParse (accelerator, out ParsedAccelerator result);

		//Assert
		parsed.Should ().BeTrue ();
		result.Key.Should ().Be (minus.Key);
	}

	[Theory]
	[InlineData (null)]
	[InlineData ("")]
	[InlineData ("   ")]
	[InlineData ("<Primary")]          // unterminated modifier
	[InlineData ("<Primary>")]         // modifier with no key
	[InlineData ("<Nonsense>A")]       // unknown modifier
	[InlineData ("NotAKeyName")]
	[InlineData ("F99")]
	public void TryParse_rejects_what_it_cannot_dispatch (string? accelerator)
	{
		//Act
		bool parsed = AcceleratorParser.TryParse (accelerator, out ParsedAccelerator result);

		//Assert
		//Rejecting is deliberate: a guessed binding is worse than no binding,
		//because it silently steals a keystroke from something else.
		parsed.Should ().BeFalse ();
		result.Should ().Be (default (ParsedAccelerator));
	}

	[Fact]
	public void ParseAll_drops_the_accelerators_it_cannot_read ()
	{
		//Arrange
		string[] accelerators = ["<Primary>Z", "garbage", "<Ctrl>Y"];

		//Act
		var parsed = System.Linq.Enumerable.ToList (AcceleratorParser.ParseAll (accelerators));

		//Assert
		parsed.Count.Should ().Be (2);
	}

	[Fact]
	public void ParseAll_handles_a_null_list ()
	{
		//Act
		var parsed = System.Linq.Enumerable.ToList (AcceleratorParser.ParseAll (null));

		//Assert
		parsed.Should ().BeEmpty ();
	}

	[Theory]
	[InlineData ("<Primary><Shift>X", "Ctrl+Shift+X")]
	[InlineData ("<Primary>Z", "Ctrl+Z")]
	[InlineData ("F11", "F11")]
	[InlineData ("Delete", "Delete")]
	[InlineData ("BackSpace", "Backspace")]
	[InlineData ("<Primary>plus", "Ctrl++")]
	[InlineData ("<Primary>KP_Add", "Ctrl+Num +")]
	[InlineData ("<Ctrl><Alt>X", "Ctrl+Alt+X")]
	public void FormatAccelerator_renders_what_a_menu_should_show (string accelerator, string expected)
	{
		//Act
		string text = CommandMenuBuilder.FormatAccelerator (accelerator);

		//Assert
		text.Should ().Be (expected);
	}
}
