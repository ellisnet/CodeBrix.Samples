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

	[Theory]
	[InlineData (VirtualKey.P, KeyConstants.KEY_P)]
	[InlineData (VirtualKey.S, KeyConstants.KEY_S)]
	[InlineData (VirtualKey.Z, KeyConstants.KEY_Z)]
	public void TryGetToolShortcut_maps_an_unmodified_letter_onto_the_tools_shortcut_key (VirtualKey pressed, uint toolKey)
	{
		//Act
		bool isShortcut = InputMapper.TryGetToolShortcut (
			pressed, VirtualKeyModifiers.None, handled: false, typing: false, out Key shortcut);

		//Assert - the tool manager compares case-insensitively, so the test does too
		isShortcut.Should ().BeTrue ();
		shortcut.ToUpper ().Should ().Be (new Key (toolKey).ToUpper ());
	}

	[Theory]
	[InlineData (VirtualKeyModifiers.Control)]
	[InlineData (VirtualKeyModifiers.Shift)]
	[InlineData (VirtualKeyModifiers.Menu)]
	[InlineData (VirtualKeyModifiers.Windows)]
	public void TryGetToolShortcut_ignores_a_letter_pressed_with_a_modifier (VirtualKeyModifiers modifiers)
	{
		//Act
		bool isShortcut = InputMapper.TryGetToolShortcut (
			VirtualKey.P, modifiers, handled: false, typing: false, out Key shortcut);

		//Assert
		isShortcut.Should ().BeFalse ();
		shortcut.Should ().Be (Key.Invalid);
	}

	[Fact]
	public void TryGetToolShortcut_ignores_a_key_the_active_tool_already_consumed ()
	{
		//Act
		bool isShortcut = InputMapper.TryGetToolShortcut (
			VirtualKey.P, VirtualKeyModifiers.None, handled: true, typing: false, out _);

		//Assert
		isShortcut.Should ().BeFalse ();
	}

	[Fact]
	public void TryGetToolShortcut_ignores_a_key_typed_into_a_text_entry ()
	{
		//Act
		bool isShortcut = InputMapper.TryGetToolShortcut (
			VirtualKey.P, VirtualKeyModifiers.None, handled: false, typing: true, out _);

		//Assert
		isShortcut.Should ().BeFalse ();
	}

	//A key with no keysym maps onto the same value the tools with no shortcut
	//report, so it must never reach the tool manager as a shortcut.
	[Theory]
	[InlineData (VirtualKey.Number1)]
	[InlineData (VirtualKey.Escape)]
	[InlineData (VirtualKey.Space)]
	[InlineData (VirtualKey.F5)]
	public void TryGetToolShortcut_ignores_a_key_that_is_not_a_letter (VirtualKey pressed)
	{
		//Act
		bool isShortcut = InputMapper.TryGetToolShortcut (
			pressed, VirtualKeyModifiers.None, handled: false, typing: false, out Key shortcut);

		//Assert
		isShortcut.Should ().BeFalse ();
		shortcut.Should ().Be (Key.Invalid);
	}
}
