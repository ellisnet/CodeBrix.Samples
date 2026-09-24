namespace BrixInvaders.Game.Input;

/// <summary>
/// One sample of every input device the game reads: the keys held, the gamepad buttons held (all connected pads
/// merged) and the left stick. The host fills one per engine cycle; <see cref="InputMapper"/> turns the
/// stream of samples into the game's input.
/// </summary>
public readonly struct RawInput
{
    /// <summary>Creates a sample.</summary>
    /// <param name="keys">The keys held.</param>
    /// <param name="buttons">The gamepad buttons held.</param>
    /// <param name="stickX">Left stick X, -1 (left) .. +1 (right).</param>
    /// <param name="stickY">Left stick Y, -1 (down) .. +1 (up).</param>
    public RawInput(InputKeys keys = InputKeys.None, PadButtons buttons = PadButtons.None, double stickX = 0, double stickY = 0)
    {
        Keys = keys;
        Buttons = buttons;
        StickX = double.IsNaN(stickX) ? 0 : stickX;
        StickY = double.IsNaN(stickY) ? 0 : stickY;
    }

    /// <summary>The keys held.</summary>
    public InputKeys Keys { get; }

    /// <summary>The gamepad buttons held.</summary>
    public PadButtons Buttons { get; }

    /// <summary>Left stick X, -1 (left) .. +1 (right).</summary>
    public double StickX { get; }

    /// <summary>Left stick Y, -1 (down) .. +1 (up).</summary>
    public double StickY { get; }
}
