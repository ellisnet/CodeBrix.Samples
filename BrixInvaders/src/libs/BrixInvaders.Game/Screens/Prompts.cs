using BrixInvaders.Game.Input;
using CodeBrix.Platform.GameEngine.Input.Actions;

namespace BrixInvaders.Game.Screens;

/// <summary>The on-screen control prompts, in the glyph set of the last device used.</summary>
public static class Prompts
{
    /// <summary>The confirm control.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Confirm(InputDeviceKind device) => device == InputDeviceKind.Gamepad ? "[A]" : "[ENTER]";

    /// <summary>The back control.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Back(InputDeviceKind device) => device == InputDeviceKind.Gamepad ? "[B]" : "[ESC]";

    /// <summary>The pause control.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Pause(InputDeviceKind device) => device == InputDeviceKind.Gamepad ? "[START]" : "[ESC]";

    /// <summary>The Kenney link control.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Link(InputDeviceKind device) => device == InputDeviceKind.Gamepad ? "[Y]" : "[K]";

    /// <summary>The navigation controls.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Navigate(InputDeviceKind device) => device == InputDeviceKind.Gamepad ? "[D-PAD]" : "[ARROWS]";

    /// <summary>The move control.</summary>
    /// <param name="device">The device.</param>
    /// <returns>The glyph text.</returns>
    public static string Move(InputDeviceKind device) => device == InputDeviceKind.Gamepad ? "[STICK]" : "[A/D]";

    /// <summary>The fire control.</summary>
    /// <param name="device">The device.</param>
    /// <param name="profile">The gamepad profile.</param>
    /// <returns>The glyph text.</returns>
    public static string Fire(InputDeviceKind device, GamepadProfile profile) =>
        device != InputDeviceKind.Gamepad ? "[SPACE]" : profile == GamepadProfile.Shoulder ? "[RB]" : "[A]";

    /// <summary>The bomb control.</summary>
    /// <param name="device">The device.</param>
    /// <param name="profile">The gamepad profile.</param>
    /// <returns>The glyph text.</returns>
    public static string Bomb(InputDeviceKind device, GamepadProfile profile) =>
        device != InputDeviceKind.Gamepad ? "[SHIFT]" : profile == GamepadProfile.Shoulder ? "[LB]" : "[B]";
}
