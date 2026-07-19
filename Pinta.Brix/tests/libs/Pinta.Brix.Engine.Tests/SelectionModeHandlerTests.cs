using System;
using System.Collections.Generic;

namespace Pinta.Brix.Engine.Tests;

public sealed class SelectionModeHandlerTests
{
	/// <summary>
	/// Settings stub that always hands back the caller's default, matching a
	/// first run with no stored selection-mode preference.
	/// </summary>
	private sealed class DefaultsOnlySettings : ISettingsService
	{
		public T GetSetting<T> (string key, T defaultValue) => defaultValue;
		public string GetUserSettingsDirectory () => string.Empty;
		public void PutSetting (string key, object value) { }
		public event EventHandler? SaveSettingsBeforeQuit { add { } remove { } }
	}

	private static SelectionModeHandler BuiltHandler ()
	{
		SelectionModeHandler handler = new (new SystemManager ());
		handler.BuildToolbar (new ToolBar (), new DefaultsOnlySettings ());
		return handler;
	}

	private static ToolMouseEventArgs Click (MouseButton button, ModifierType modifiers)
		=> new () { MouseButton = button, State = modifiers };

	/// <summary>
	/// Regression guard: the combine mode must start at Replace. The combo box
	/// is constructed already sitting on its initial index, so assigning
	/// SelectedIndex raises no change event and the handler's backing field
	/// would otherwise keep default(CombineMode) -- which is Union, because
	/// Union is the first member of the enum. That made every new selection
	/// union with the previous one (starting from the full-canvas default),
	/// so selections silently covered the whole image.
	/// </summary>
	[Fact]
	public void DetermineCombineMode_defaults_to_Replace ()
	{
		//Arrange
		SelectionModeHandler handler = BuiltHandler ();

		//Act
		CombineMode mode = handler.DetermineCombineMode (Click (MouseButton.Left, ModifierType.Button1Mask));

		//Assert
		Assert.Equal (CombineMode.Replace, mode);
	}

	/// <summary>
	/// The modifier overrides documented on the tool bar still apply on top of
	/// the default mode.
	/// </summary>
	[Theory]
	[InlineData (MouseButton.Left, ModifierType.ControlMask, CombineMode.Union)]
	[InlineData (MouseButton.Left, ModifierType.AltMask, CombineMode.Intersect)]
	[InlineData (MouseButton.Right, ModifierType.None, CombineMode.Exclude)]
	[InlineData (MouseButton.Right, ModifierType.ControlMask, CombineMode.Xor)]
	public void DetermineCombineMode_honours_modifier_overrides (
		MouseButton button,
		ModifierType modifiers,
		CombineMode expected)
	{
		//Arrange
		SelectionModeHandler handler = BuiltHandler ();

		//Act
		CombineMode mode = handler.DetermineCombineMode (Click (button, modifiers));

		//Assert
		Assert.Equal (expected, mode);
	}

	/// <summary>
	/// Pins the hazard the regression above came from, so that reordering the
	/// enum (or relying on its default) is a deliberate act.
	/// </summary>
	[Fact]
	public void Default_CombineMode_value_is_Union_not_Replace ()
	{
		//Arrange, Act, Assert
		Assert.Equal (CombineMode.Union, default (CombineMode));
		Assert.NotEqual (CombineMode.Replace, default (CombineMode));
	}
}
