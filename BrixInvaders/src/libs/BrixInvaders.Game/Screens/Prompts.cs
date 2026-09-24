using BrixInvaders.Game.Input;

namespace BrixInvaders.Game.Screens;

/// <summary>The on-screen control prompts, in the glyph set of the last device used.</summary>
public static class Prompts
{
    /// <summary>The confirm control.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Confirm(InputDevice device) => device == InputDevice.Gamepad ? "[A]" : "[ENTER]";

    /// <summary>The back control.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Back(InputDevice device) => device == InputDevice.Gamepad ? "[B]" : "[ESC]";

    /// <summary>The pause control.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Pause(InputDevice device) => device == InputDevice.Gamepad ? "[START]" : "[ESC]";

    /// <summary>The Kenney link control.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Link(InputDevice device) => device == InputDevice.Gamepad ? "[Y]" : "[K]";

    /// <summary>The navigation controls.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Navigate(InputDevice device) => device == InputDevice.Gamepad ? "[D-PAD]" : "[ARROWS]";

    /// <summary>The move control.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Move(InputDevice device) => device == InputDevice.Gamepad ? "[STICK]" : "[A/D]";

    /// <summary>The fire control.</summary>
    /// <param name="device">The device.</param>
    /// <param name="profile">The gamepad profile.</param>
    /// <returns>The glyph text.</returns>
    public static string Fire(InputDevice device, GamepadProfile profile) =>
        device != InputDevice.Gamepad ? "[SPACE]" : profile == GamepadProfile.Shoulder ? "[RB]" : "[A]";

    /// <summary>The bomb control.</summary>
    /// <param name="device">The device.</param>
    /// <param name="profile">The gamepad profile.</param>
    /// <returns>The glyph text.</returns>
    public static string Bomb(InputDevice device, GamepadProfile profile) =>
        device != InputDevice.Gamepad ? "[SHIFT]" : profile == GamepadProfile.Shoulder ? "[LB]" : "[B]";
}
