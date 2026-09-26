using System.Collections.Generic;
using System.Linq;
using BrixInvaders.Game.Input;
using CodeBrix.Platform.GameEngine.Input.Actions;
using CodeBrix.Platform.GameEngine.Input.Gamepad;
using CodeBrix.Platform.GameEngine.Input.Keyboard;
using Windows.System;

namespace BrixInvaders.Game.Tests.Support;

/// <summary>
/// A keyboard and one gamepad in a single fake, for driving the game's <see cref="InputActionMap"/> without an
/// engine: <see cref="Set"/> replaces what is held, and each <see cref="InputActionMap.Update"/> reads it.
/// </summary>
internal sealed class FakeDevices : IKeyboardAdapter, IGamepadAdapter
{
    private readonly HashSet<int> _keys = new HashSet<int>();
    private readonly HashSet<string> _buttons = new HashSet<string>();
    private float _stickX;
    private float _stickY;

    public KeyboardModifierState CurrentKeyboardModifiers => default;

    public string GamepadId => "fake";

    public IReadOnlyCollection<string> PressedButtons => _buttons.ToArray();

    public GamepadStickState? LeftStick => new GamepadStickState(_stickX, _stickY);

    public GamepadStickState? RightStick => null;

    public float LeftTrigger => 0;

    public float RightTrigger => 0;

    public bool IsDown(int keyCode) => _keys.Contains(keyCode);

    /// <summary>Creates a map over these devices with the game's bindings and numbers.</summary>
    public InputActionMap CreateMap(GamepadProfile profile = GamepadProfile.Classic) =>
        GameControls.Configure(new InputActionMap(GameControls.ProfileFor(profile), () => this, () => new IGamepadAdapter[] { this }));

    /// <summary>Replaces everything held: keys, buttons (SDL names) and the left stick (up is positive).</summary>
    public FakeDevices Set(VirtualKey[] keys = null, string[] buttons = null, double stickX = 0, double stickY = 0)
    {
        _keys.Clear();
        _keys.UnionWith((keys ?? new VirtualKey[0]).Select(key => (int)key));
        _buttons.Clear();
        _buttons.UnionWith(buttons ?? new string[0]);
        _stickX = (float)stickX;
        _stickY = (float)stickY;
        return this;
    }

    /// <summary>Holds one key and nothing else.</summary>
    public FakeDevices Key(VirtualKey key) => Set(new[] { key });

    /// <summary>Holds one gamepad button and nothing else.</summary>
    public FakeDevices Button(string button) => Set(buttons: new[] { button });

    /// <summary>Releases everything.</summary>
    public FakeDevices None() => Set();
}
