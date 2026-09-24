namespace BrixInvaders.Game.Input;

/// <summary>The kind of device that produced the player's most recent input; on-screen prompts follow it.</summary>
public enum InputDevice
{
    /// <summary>The keyboard (and mouse).</summary>
    Keyboard = 0,

    /// <summary>A game controller.</summary>
    Gamepad = 1,
}
