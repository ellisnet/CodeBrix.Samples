// CommandAcceleratorTableTests.cs
//
// The table is what actually fires every keyboard shortcut in the application,
// because XAML KeyboardAccelerators do not invoke on the Skia heads.

using Pinta.Brix.Controls;
using Pinta.Brix.Engine;
using SilverAssertions;
using Windows.System;
using Xunit;

namespace Pinta.Brix.Controls.Tests;

public class CommandAcceleratorTableTests
{
	private static Command CommandWith (params string[] shortcuts)
		=> new ("test", "Test", null, null, shortcuts);

	[Fact]
	public void TryInvoke_activates_the_command_bound_to_the_key ()
	{
		//Arrange
		CommandAcceleratorTable table = new ();
		Command command = CommandWith ("<Primary>Z");
		bool activated = false;
		command.Activated += (_, _) => activated = true;
		table.Register (command);
		table.TrackModifier (VirtualKey.Control, down: true);

		//Act
		bool invoked = table.TryInvoke (VirtualKey.Z);

		//Assert
		invoked.Should ().BeTrue ();
		activated.Should ().BeTrue ();
	}

	[Fact]
	public void TryInvoke_requires_the_modifiers_to_match ()
	{
		//Arrange
		CommandAcceleratorTable table = new ();
		Command command = CommandWith ("<Primary>Z");
		bool activated = false;
		command.Activated += (_, _) => activated = true;
		table.Register (command);

		//Act
		//No Control held, so Z alone must not fire Ctrl+Z.
		bool invoked = table.TryInvoke (VirtualKey.Z);

		//Assert
		invoked.Should ().BeFalse ();
		activated.Should ().BeFalse ();
	}

	[Fact]
	public void TryInvoke_ignores_an_insensitive_command ()
	{
		//Arrange
		CommandAcceleratorTable table = new ();
		Command command = CommandWith ("<Primary>Z");
		command.Sensitive = false;
		bool activated = false;
		command.Activated += (_, _) => activated = true;
		table.Register (command);
		table.TrackModifier (VirtualKey.Control, down: true);

		//Act
		bool invoked = table.TryInvoke (VirtualKey.Z);

		//Assert
		//A disabled command must swallow nothing - the key should behave as if
		//the shortcut were not bound at all.
		invoked.Should ().BeFalse ();
		activated.Should ().BeFalse ();
	}

	[Fact]
	public void Register_binds_every_alias_a_command_declares ()
	{
		//Arrange
		CommandAcceleratorTable table = new ();

		//Act
		table.Register (CommandWith ("<Primary><Shift>Z", "<Ctrl>Y"));

		//Assert
		table.Count.Should ().Be (2);
	}

	[Fact]
	public void Register_keeps_the_first_command_when_two_claim_one_shortcut ()
	{
		//Arrange
		//Upstream has real collisions: View's ActualSize and Edit's Deselect
		//both claim <Primary><Shift>A.
		CommandAcceleratorTable table = new ();
		Command first = CommandWith ("<Primary><Shift>A");
		Command second = CommandWith ("<Primary><Shift>A");
		bool firstFired = false;
		bool secondFired = false;
		first.Activated += (_, _) => firstFired = true;
		second.Activated += (_, _) => secondFired = true;

		table.Register (first);
		table.Register (second);
		table.TrackModifier (VirtualKey.Control, down: true);
		table.TrackModifier (VirtualKey.Shift, down: true);

		//Act
		table.TryInvoke (VirtualKey.A);

		//Assert
		firstFired.Should ().BeTrue ();
		secondFired.Should ().BeFalse ();
	}

	[Theory]
	[InlineData (VirtualKey.Control)]
	[InlineData (VirtualKey.LeftControl)]
	[InlineData (VirtualKey.RightControl)]
	[InlineData (VirtualKey.Shift)]
	[InlineData (VirtualKey.LeftShift)]
	[InlineData (VirtualKey.Menu)]
	public void TrackModifier_reports_modifier_keys (VirtualKey key)
	{
		//Arrange
		CommandAcceleratorTable table = new ();

		//Act
		bool isModifier = table.TrackModifier (key, down: true);

		//Assert
		isModifier.Should ().BeTrue ();
	}

	[Fact]
	public void TrackModifier_does_not_claim_an_ordinary_key ()
	{
		//Arrange
		CommandAcceleratorTable table = new ();

		//Act
		bool isModifier = table.TrackModifier (VirtualKey.Z, down: true);

		//Assert
		isModifier.Should ().BeFalse ();
	}

	[Fact]
	public void ResetModifiers_releases_a_modifier_held_when_focus_was_lost ()
	{
		//Arrange
		CommandAcceleratorTable table = new ();
		table.TrackModifier (VirtualKey.Control, down: true);

		//Act
		table.ResetModifiers ();

		//Assert
		table.CurrentModifiers.Should ().Be (VirtualKeyModifiers.None);
	}

	[Fact]
	public void CurrentModifiers_combines_the_keys_held ()
	{
		//Arrange
		CommandAcceleratorTable table = new ();

		//Act
		table.TrackModifier (VirtualKey.Control, down: true);
		table.TrackModifier (VirtualKey.Shift, down: true);

		//Assert
		table.CurrentModifiers.Should ().Be (VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift);
	}
}
