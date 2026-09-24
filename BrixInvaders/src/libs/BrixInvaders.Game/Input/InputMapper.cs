using System;
using System.Collections.Generic;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Input;

/// <summary>
/// Turns the stream of <see cref="RawInput"/> samples (keyboard and gamepad at the same time) into what the game
/// logic reads: a held <see cref="GameInput"/> for play and edge-triggered <see cref="MenuInput"/> for the screens.
/// </summary>
/// <remarks>
/// <para>
/// The host samples every engine cycle (thousands of times a second) but the game consumes input once per fixed
/// step, so menu presses are LATCHED here until <see cref="TakeMenuInput"/> collects them: a tap shorter than a
/// frame is never lost. Held state (movement, fire, bomb) is read from the latest sample.
/// </para>
/// <para>
/// Bindings follow DESIGN.md section 13. The left stick has a 0.15 dead zone for movement and acts as a digital
/// direction in menus once it passes 0.5 (released below 0.3); the D-pad is always digital. After a stick release
/// the same axis ignores the stick for <see cref="StickSettleSeconds"/>, because a released stick springs back PAST
/// centre and that overshoot would otherwise read as a push the other way.
/// </para>
/// <para>
/// Menu directions (arrows, D-pad, stick) repeat while held: the press acts at once, the first repeat follows
/// <see cref="RepeatDelay"/> seconds later and the next ones every <see cref="MenuRepeatInterval"/> seconds, at a
/// constant rate, until the direction is released. The clock is the elapsed time passed to
/// <see cref="Sample(RawInput, double)"/>. Everything else (confirm, back, pause, ...) is a single edge per press.
/// </para>
/// </remarks>
public sealed class InputMapper
{
    /// <summary>The left-stick dead zone for movement.</summary>
    public const double DeadZone = 0.15;

    /// <summary>How far the stick must be pushed to count as a menu direction.</summary>
    public const double StickMenuThreshold = 0.5;

    /// <summary>How far the stick must come back before a menu direction counts as released.</summary>
    public const double StickMenuRelease = 0.3;

    /// <summary>
    /// Seconds after a stick release during which that axis cannot fire a new menu direction, either way (the
    /// spring-back overshoot of a released stick passes the opposite threshold for a few milliseconds).
    /// </summary>
    public const double StickSettleSeconds = 0.08;

    /// <summary>Seconds from a menu-direction press to its first repeat while held.</summary>
    public const double RepeatDelay = 0.35;

    /// <summary>Seconds between menu-direction repeats while held (menus).</summary>
    public const double RepeatInterval = 0.1;

    /// <summary>Seconds between repeats while held on the high-score name entry (a little slower, so letters are readable).</summary>
    public const double NameEntryRepeatInterval = 0.12;

    private readonly List<string> _pressedNames = new List<string>();
    private RawInput _latest;
    private bool _hasSample;
    private bool _up;
    private bool _down;
    private bool _left;
    private bool _right;
    private bool _confirm;
    private bool _back;
    private bool _pause;
    private bool _start;
    private bool _kenneyLink;
    private bool _other;
    private readonly StickAxis _stickX = new StickAxis();
    private readonly StickAxis _stickY = new StickAxis();
    private readonly HeldRepeat _upRepeat = new HeldRepeat();
    private readonly HeldRepeat _downRepeat = new HeldRepeat();
    private readonly HeldRepeat _leftRepeat = new HeldRepeat();
    private readonly HeldRepeat _rightRepeat = new HeldRepeat();
    private bool _clickPending;
    private double _clickX;
    private double _clickY;

    /// <summary>The gamepad binding preset (fire and bomb buttons).</summary>
    public GamepadProfile Profile { get; set; } = GamepadProfile.Classic;

    /// <summary>
    /// Seconds between menu-direction repeats while held; <see cref="RepeatInterval"/> by default, the host sets
    /// <see cref="NameEntryRepeatInterval"/> on the name entry.
    /// </summary>
    public double MenuRepeatInterval { get; set; } = RepeatInterval;

    /// <summary>The device that produced the most recent press; on-screen prompts follow it.</summary>
    public InputDevice LastDevice { get; set; } = InputDevice.Keyboard;

    /// <summary>The held input for play, from the latest sample.</summary>
    public GameInput GameInput
    {
        get
        {
            if (!_hasSample)
            {
                return GameInput.None;
            }

            var keys = _latest.Keys;
            var buttons = _latest.Buttons;
            var axis = 0.0;
            if (Has(keys, InputKeys.Left) || Has(keys, InputKeys.A) || Has(buttons, PadButtons.DPadLeft))
            {
                axis -= 1;
            }

            if (Has(keys, InputKeys.Right) || Has(keys, InputKeys.D) || Has(buttons, PadButtons.DPadRight))
            {
                axis += 1;
            }

            axis += ApplyDeadZone(_latest.StickX);
            var fireButton = Profile == GamepadProfile.Classic ? PadButtons.A : PadButtons.RightShoulder;
            var bombButton = Profile == GamepadProfile.Classic ? PadButtons.B : PadButtons.LeftShoulder;
            var fire = Has(keys, InputKeys.Space) || Has(buttons, fireButton);
            var bomb = Has(keys, InputKeys.LeftShift) || Has(buttons, bombButton);

            return new GameInput(Math.Clamp(axis, -1.0, 1.0), fire, bomb);
        }
    }

    /// <summary>A stick value with the movement dead zone applied (0 inside it, unchanged outside).</summary>
    /// <param name="value">The raw stick axis.</param>
    /// <returns>The value the game uses.</returns>
    public static double ApplyDeadZone(double value) => Math.Abs(value) < DeadZone ? 0 : Math.Clamp(value, -1.0, 1.0);

    /// <summary>Takes one sample. Call once per engine cycle.</summary>
    /// <param name="raw">What every device reports now.</param>
    /// <param name="elapsedSeconds">Seconds since the previous sample (drives hold-to-repeat and the stick settle time).</param>
    public void Sample(RawInput raw, double elapsedSeconds = 0)
    {
        if (!_hasSample)
        {
            //Whatever is already held when sampling starts is not a press, and does not repeat until released
            _latest = raw;
            _hasSample = true;
            _stickX.Start(raw.StickX);
            _stickY.Start(raw.StickY);
            _upRepeat.Suppress(IsUpHeld(raw) || _stickY.Direction > 0);
            _downRepeat.Suppress(IsDownHeld(raw) || _stickY.Direction < 0);
            _leftRepeat.Suppress(IsLeftHeld(raw) || _stickX.Direction < 0);
            _rightRepeat.Suppress(IsRightHeld(raw) || _stickX.Direction > 0);
            return;
        }

        var pressedKeys = raw.Keys & ~_latest.Keys;
        var pressedButtons = raw.Buttons & ~_latest.Buttons;

        if (pressedKeys != InputKeys.None)
        {
            LastDevice = InputDevice.Keyboard;
            LatchKeys(pressedKeys);
        }

        if (pressedButtons != PadButtons.None)
        {
            LastDevice = InputDevice.Gamepad;
            LatchButtons(pressedButtons);
        }

        UpdateStick(raw, elapsedSeconds);
        UpdateDirections(raw, elapsedSeconds);
        _latest = raw;
    }

    /// <summary>Registers a primary-button click (world units); it counts as keyboard-and-mouse input.</summary>
    /// <param name="x">The click X.</param>
    /// <param name="y">The click Y.</param>
    public void RegisterClick(double x, double y)
    {
        LastDevice = InputDevice.Keyboard;
        _clickPending = true;
        _clickX = x;
        _clickY = y;
        _other = true;
    }

    /// <summary>
    /// Latches a pause request that did not come from a key or a button - the window being hidden, for
    /// example - so the next menu read sees it exactly as it would see Escape or Start.
    /// </summary>
    public void RequestPause()
    {
        _pause = true;
        _pressedNames.Add("pause request (window hidden)");
    }

    /// <summary>Returns every menu press (and hold repeat) latched since the last call, and clears the latch.</summary>
    /// <returns>The edge-triggered menu input.</returns>
    public MenuInput TakeMenuInput()
    {
        var input = new MenuInput(_up, _down, _left, _right, _confirm, _back, _pause, _start, _kenneyLink, _other);
        _up = _down = _left = _right = _confirm = _back = _pause = _start = _kenneyLink = _other = false;
        return input;
    }

    /// <summary>
    /// Returns the names of the keys and buttons pressed since the last call ("key Enter", "button A",
    /// "stick Up"), for the input log, and clears them.
    /// </summary>
    /// <returns>The names, oldest first.</returns>
    public IReadOnlyList<string> TakePressedNames()
    {
        var names = _pressedNames.ToArray();
        _pressedNames.Clear();
        return names;
    }

    /// <summary>Returns the latest primary-button click (world units) since the last call, if there was one.</summary>
    /// <param name="x">The click X.</param>
    /// <param name="y">The click Y.</param>
    /// <returns>Whether a click was pending.</returns>
    public bool TryTakeClick(out double x, out double y)
    {
        x = _clickX;
        y = _clickY;
        var pending = _clickPending;
        _clickPending = false;
        return pending;
    }

    /// <summary>Drops every latched press (used when a screen must not see input meant for the previous one).</summary>
    public void ClearLatched()
    {
        TakeMenuInput();
        _pressedNames.Clear();
        _clickPending = false;
    }

    private static bool Has(InputKeys keys, InputKeys key) => (keys & key) != 0;

    private static bool Has(PadButtons buttons, PadButtons button) => (buttons & button) != 0;

    private void LatchKeys(InputKeys pressed)
    {
        foreach (InputKeys key in Enum.GetValues<InputKeys>())
        {
            if (key == InputKeys.None || !Has(pressed, key))
            {
                continue;
            }

            _pressedNames.Add($"key {key}");
            switch (key)
            {
                case InputKeys.Up:
                case InputKeys.Down:
                case InputKeys.Left:
                case InputKeys.Right:
                    //Menu directions act through the hold-to-repeat clocks (UpdateDirections)
                    break;
                case InputKeys.Enter:
                    _confirm = true;
                    break;
                case InputKeys.Escape:
                    _back = true;
                    _pause = true;
                    break;
                case InputKeys.K:
                    _kenneyLink = true;
                    break;
                default:
                    _other = true;
                    break;
            }
        }
    }

    private void LatchButtons(PadButtons pressed)
    {
        foreach (PadButtons button in Enum.GetValues<PadButtons>())
        {
            if (button == PadButtons.None || !Has(pressed, button))
            {
                continue;
            }

            _pressedNames.Add($"button {button}");
            switch (button)
            {
                case PadButtons.DPadUp:
                case PadButtons.DPadDown:
                case PadButtons.DPadLeft:
                case PadButtons.DPadRight:
                    //Menu directions act through the hold-to-repeat clocks (UpdateDirections)
                    break;
                case PadButtons.A:
                    _confirm = true;
                    break;
                case PadButtons.B:
                    _back = true;
                    break;
                case PadButtons.Start:
                    _start = true;
                    break;
                case PadButtons.Y:
                    _kenneyLink = true;
                    break;
                default:
                    _other = true;
                    break;
            }
        }
    }

    private static bool IsUpHeld(RawInput raw) => Has(raw.Keys, InputKeys.Up) || Has(raw.Buttons, PadButtons.DPadUp);

    private static bool IsDownHeld(RawInput raw) => Has(raw.Keys, InputKeys.Down) || Has(raw.Buttons, PadButtons.DPadDown);

    private static bool IsLeftHeld(RawInput raw) => Has(raw.Keys, InputKeys.Left) || Has(raw.Buttons, PadButtons.DPadLeft);

    private static bool IsRightHeld(RawInput raw) => Has(raw.Keys, InputKeys.Right) || Has(raw.Buttons, PadButtons.DPadRight);

    private void UpdateStick(RawInput raw, double elapsed)
    {
        if (_stickY.Update(raw.StickY, elapsed))
        {
            StickPressed(_stickY.Direction > 0 ? "Up" : "Down");
        }

        if (_stickX.Update(raw.StickX, elapsed))
        {
            StickPressed(_stickX.Direction > 0 ? "Right" : "Left");
        }
    }

    private void UpdateDirections(RawInput raw, double elapsed)
    {
        var interval = MenuRepeatInterval > 0 ? MenuRepeatInterval : RepeatInterval;
        _up |= _upRepeat.Advance(IsUpHeld(raw) || _stickY.Direction > 0, elapsed, RepeatDelay, interval);
        _down |= _downRepeat.Advance(IsDownHeld(raw) || _stickY.Direction < 0, elapsed, RepeatDelay, interval);
        _left |= _leftRepeat.Advance(IsLeftHeld(raw) || _stickX.Direction < 0, elapsed, RepeatDelay, interval);
        _right |= _rightRepeat.Advance(IsRightHeld(raw) || _stickX.Direction > 0, elapsed, RepeatDelay, interval);
    }

    private void StickPressed(string direction)
    {
        LastDevice = InputDevice.Gamepad;
        _pressedNames.Add($"stick {direction}");
    }
}
