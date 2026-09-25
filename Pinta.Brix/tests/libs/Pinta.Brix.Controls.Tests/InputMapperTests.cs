// InputMapperTests.cs
//
// The engine keeps upstream Pinta's GDK convention: its "Ctrl"
// (IsControlPressed) is the Command key on macOS, carried as MetaMask.
// CodeBrix.Platform reports Command as the Windows modifier, so the mapper has
// to land it on MetaMask there - and only there. Both platform flavours are
// exercised explicitly, so every mapping is checked on every OS the suite runs on.

using System;
using Pinta.Brix.Controls;
using Pinta.Brix.Engine;
using SilverAssertions;
using Windows.System;
using Xunit;

namespace Pinta.Brix.Controls.Tests;

public class InputMapperTests
{
	[Theory]
	[InlineData (true, ModifierType.MetaMask)]
	[InlineData (false, ModifierType.SuperMask)]
	public void ToModifierType_maps_the_windows_modifier_per_platform (bool isMacOS, ModifierType expected)
	{
		//Act
		ModifierType state = InputMapper.ToModifierType (VirtualKeyModifiers.Windows, null, isMacOS);

		//Assert
		state.Should ().Be (expected);
	}

	[Theory]
	[InlineData (true)]
	[InlineData (false)]
	public void ToModifierType_maps_control_shift_and_alt_the_same_on_every_platform (bool isMacOS)
	{
		//Act
		ModifierType state = InputMapper.ToModifierType (
			VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift | VirtualKeyModifiers.Menu, null, isMacOS);

		//Assert
		state.Should ().Be (ModifierType.ControlMask | ModifierType.ShiftMask | ModifierType.AltMask);
	}

	[Theory]
	[InlineData (VirtualKey.LeftWindows, true, ModifierType.MetaMask)]
	[InlineData (VirtualKey.RightWindows, true, ModifierType.MetaMask)]
	[InlineData (VirtualKey.LeftWindows, false, ModifierType.SuperMask)]
	[InlineData (VirtualKey.RightWindows, false, ModifierType.SuperMask)]
	[InlineData (VirtualKey.LeftControl, true, ModifierType.ControlMask)]
	[InlineData (VirtualKey.Control, false, ModifierType.ControlMask)]
	[InlineData (VirtualKey.Shift, true, ModifierType.ShiftMask)]
	[InlineData (VirtualKey.Menu, false, ModifierType.AltMask)]
	[InlineData (VirtualKey.A, true, ModifierType.None)]
	public void ModifierMaskForKey_tracks_the_modifier_keys (VirtualKey key, bool isMacOS, ModifierType expected)
	{
		//Act
		ModifierType mask = InputMapper.ModifierMaskForKey (key, isMacOS);

		//Assert
		mask.Should ().Be (expected);
	}

	/// <summary>
	/// End to end on the OS running the suite: the platform's primary modifier
	/// (Command on macOS, Ctrl elsewhere) must read as the engine's Ctrl, and
	/// the other one must not.
	/// </summary>
	[Fact]
	public void The_primary_modifier_is_the_engines_control_on_this_platform ()
	{
		//Arrange
		VirtualKeyModifiers primary = OperatingSystem.IsMacOS () ? VirtualKeyModifiers.Windows : VirtualKeyModifiers.Control;
		VirtualKeyModifiers other = OperatingSystem.IsMacOS () ? VirtualKeyModifiers.Control : VirtualKeyModifiers.Windows;

		//Act
		bool primaryIsControl = InputMapper.ToModifierType (primary).IsControlPressed ();
		bool otherIsControl = InputMapper.ToModifierType (other).IsControlPressed ();

		//Assert
		primaryIsControl.Should ().BeTrue ();
		otherIsControl.Should ().BeFalse ();
	}
}
