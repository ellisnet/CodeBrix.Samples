// CommandAcceleratorTable.cs
//
// Pinta.Brix note: a KeyboardAccelerator on a menu item fires application-wide
// on the Skia heads, but this port does not use them. The shortcut data lives
// on the engine's Command objects as upstream's GTK accelerator strings, in a
// library with no XAML, and this table turns those strings into one dispatch
// path from a single KeyDown handler on the page. That one place also holds
// the rules XAML accelerators would not give: first registration wins on
// upstream's genuine collisions, and a disabled command swallows nothing. The
// same handler carries upstream's unmodified tool and palette keys, which are
// not commands at all. The table predates app-wide menu accelerators; the menu
// items show the shortcut text from the same strings, and adding real
// accelerators as well would fire every shortcut twice.
//
// This is close to what upstream did anyway - Pinta's MainWindow carried a
// HandleGlobalKeyPress for exactly the keys GTK would not route - and it keeps
// the shortcut data on the Command objects where the port put it.

using System;
using System.Collections.Generic;
using Windows.System;

namespace Pinta.Brix.Controls;

/// <summary>
/// Maps parsed accelerators onto commands, and tracks the modifier keys so a
/// plain key event can be matched against them.
/// </summary>
public sealed class CommandAcceleratorTable
{
	private readonly Dictionary<(VirtualKey Key, VirtualKeyModifiers Modifiers), Engine.Command> map = [];

	private bool control_down;
	private bool shift_down;
	private bool alt_down;
	private bool windows_down;

	/// <summary>The modifiers currently held.</summary>
	public VirtualKeyModifiers CurrentModifiers =>
		(control_down ? VirtualKeyModifiers.Control : VirtualKeyModifiers.None)
		| (shift_down ? VirtualKeyModifiers.Shift : VirtualKeyModifiers.None)
		| (alt_down ? VirtualKeyModifiers.Menu : VirtualKeyModifiers.None)
		| (windows_down ? VirtualKeyModifiers.Windows : VirtualKeyModifiers.None);

	/// <summary>
	/// Registers every accelerator a command declares.
	/// </summary>
	/// <param name="command">The command to register.</param>
	/// <exception cref="ArgumentNullException"><paramref name="command"/> is null.</exception>
	public void Register (Engine.Command command)
	{
		ArgumentNullException.ThrowIfNull (command);

		foreach (ParsedAccelerator parsed in AcceleratorParser.ParseAll (command.Shortcuts)) {

			// First registration wins. Upstream has genuine collisions - View's
			// ActualSize and Edit's Deselect both claim <Primary><Shift>A - and
			// silently keeping the first match is what GTK did too.
			map.TryAdd ((parsed.Key, parsed.Modifiers), command);
		}
	}

	/// <summary>
	/// Updates the tracked modifier state, and reports whether the key was
	/// itself a modifier.
	/// </summary>
	/// <param name="key">The key that changed.</param>
	/// <param name="down">True on key down, false on key up.</param>
	/// <returns>True when the key was a modifier and nothing else should happen.</returns>
	public bool TrackModifier (VirtualKey key, bool down)
	{
		switch (key) {
			case VirtualKey.Control:
			case VirtualKey.LeftControl:
			case VirtualKey.RightControl:
				control_down = down;
				return true;
			case VirtualKey.Shift:
			case VirtualKey.LeftShift:
			case VirtualKey.RightShift:
				shift_down = down;
				return true;
			case VirtualKey.Menu:
			case VirtualKey.LeftMenu:
			case VirtualKey.RightMenu:
				alt_down = down;
				return true;
			case VirtualKey.LeftWindows:
			case VirtualKey.RightWindows:
				windows_down = down;
				return true;
			default:
				return false;
		}
	}

	/// <summary>
	/// Clears the tracked modifiers. Call when the window loses focus, so a
	/// modifier released elsewhere does not stay stuck down.
	/// </summary>
	public void ResetModifiers ()
	{
		control_down = false;
		shift_down = false;
		alt_down = false;
		windows_down = false;
	}

	/// <summary>
	/// Finds and activates the command bound to a key plus the modifiers
	/// currently held.
	/// </summary>
	/// <param name="key">The key that was pressed.</param>
	/// <returns>True when a sensitive command was activated.</returns>
	public bool TryInvoke (VirtualKey key)
	{
		if (!map.TryGetValue ((key, CurrentModifiers), out Engine.Command? command))
			return false;

		// A disabled command must swallow nothing: the key should behave as if
		// the shortcut were not bound at all.
		if (!command.Sensitive)
			return false;

		command.Activate ();
		return true;
	}

	/// <summary>The number of registered accelerators.</summary>
	public int Count => map.Count;
}
