using System.Collections.Generic;
using BrixInvaders.Game.Input;
using CodeBrix.Platform.GameEngine.Sdl2.Gamepad;
using Windows.System;

namespace BrixInvaders.Game.Hosting;

/// <summary>The physical keys and gamepad buttons the host samples, and the game input flag each one sets.</summary>
public static class KeyBindings
{
    /// <summary>Every watched key and its flag (Shift and Left Shift both count as Left Shift).</summary>
    public static readonly IReadOnlyList<(VirtualKey Key, InputKeys Flag)> Keys = new[]
    {
        (VirtualKey.Left, InputKeys.Left),
        (VirtualKey.Right, InputKeys.Right),
        (VirtualKey.Up, InputKeys.Up),
        (VirtualKey.Down, InputKeys.Down),
        (VirtualKey.A, InputKeys.A),
        (VirtualKey.D, InputKeys.D),
        (VirtualKey.Space, InputKeys.Space),
        (VirtualKey.Enter, InputKeys.Enter),
        (VirtualKey.Escape, InputKeys.Escape),
        (VirtualKey.LeftShift, InputKeys.LeftShift),
        (VirtualKey.Shift, InputKeys.LeftShift),
        (VirtualKey.K, InputKeys.K),
        (VirtualKey.W, InputKeys.Other),
        (VirtualKey.S, InputKeys.Other),
        (VirtualKey.Z, InputKeys.Other),
        (VirtualKey.X, InputKeys.Other),
        (VirtualKey.P, InputKeys.Other),
        (VirtualKey.RightShift, InputKeys.Other),
    };

    /// <summary>Every watched gamepad button (SDL2 standard names) and its flag.</summary>
    public static readonly IReadOnlyList<(string Button, PadButtons Flag)> Buttons = new[]
    {
        (SdlGamepadButtons.A, PadButtons.A),
        (SdlGamepadButtons.B, PadButtons.B),
        (SdlGamepadButtons.X, PadButtons.X),
        (SdlGamepadButtons.Y, PadButtons.Y),
        (SdlGamepadButtons.Start, PadButtons.Start),
        (SdlGamepadButtons.Back, PadButtons.Back),
        (SdlGamepadButtons.LeftShoulder, PadButtons.LeftShoulder),
        (SdlGamepadButtons.RightShoulder, PadButtons.RightShoulder),
        (SdlGamepadButtons.DPadUp, PadButtons.DPadUp),
        (SdlGamepadButtons.DPadDown, PadButtons.DPadDown),
        (SdlGamepadButtons.DPadLeft, PadButtons.DPadLeft),
        (SdlGamepadButtons.DPadRight, PadButtons.DPadRight),
    };
}
