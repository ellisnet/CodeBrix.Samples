using System;
using BrixInvaders.GameLogic;
using CodeBrix.Platform.GameEngine.Input.Actions;
using CodeBrix.Platform.GameEngine.Input.Gamepad;
using CodeBrix.Platform.GameEngine.Sdl2.Gamepad;
using Windows.System;

namespace BrixInvaders.Game.Input;

/// <summary>
/// The game's controls over the engine's <see cref="InputActionMap"/>: its action names, the two binding profiles and
/// the numbers of its feel (DESIGN.md section 13), and the reads that turn one step of the map into the game logic's
/// <see cref="GameInput"/> and <see cref="MenuInput"/>. The map itself latches short taps, repeats held menu
/// directions, reads the stick as a direction, remembers the last device and claims the keys.
/// </summary>
public static class GameControls
{
    /// <summary>Move left in play (A, Left, D-pad left; the left stick is added by <see cref="ReadPlay"/>).</summary>
    public const string MoveLeft = "MoveLeft";

    /// <summary>Move right in play (D, Right, D-pad right).</summary>
    public const string MoveRight = "MoveRight";

    /// <summary>Fire (Space; A, or the right shoulder in the Shoulder profile).</summary>
    public const string Fire = "Fire";

    /// <summary>Drop a bomb (Left Shift; B, or the left shoulder in the Shoulder profile).</summary>
    public const string Bomb = "Bomb";

    /// <summary>Menu up (Up, D-pad up, left stick up); repeats while held.</summary>
    public const string MenuUp = "MenuUp";

    /// <summary>Menu down; repeats while held.</summary>
    public const string MenuDown = "MenuDown";

    /// <summary>Menu left; repeats while held.</summary>
    public const string MenuLeft = "MenuLeft";

    /// <summary>Menu right; repeats while held.</summary>
    public const string MenuRight = "MenuRight";

    /// <summary>Menu confirm (Enter, A).</summary>
    public const string Confirm = "Confirm";

    /// <summary>Menu back (Escape, B).</summary>
    public const string Back = "Back";

    /// <summary>Pause (Escape; the window being hidden presses it too).</summary>
    public const string Pause = "Pause";

    /// <summary>The gamepad Start button.</summary>
    public const string Start = "Start";

    /// <summary>Open the Kenney link on the cards (K, Y).</summary>
    public const string KenneyLink = "KenneyLink";

    /// <summary>Every other watched key and button: it counts as input (it wakes the title from attract mode).</summary>
    public const string Other = "Other";

    /// <summary>Seconds from a menu-direction press to its first repeat while held.</summary>
    public const double RepeatDelay = 0.35;

    /// <summary>Seconds between menu-direction repeats while held (menus).</summary>
    public const double RepeatInterval = 0.1;

    /// <summary>Seconds between repeats while held on the high-score name entry (a little slower, so letters are readable).</summary>
    public const double NameEntryRepeatInterval = 0.12;

    /// <summary>The left-stick dead zone for movement.</summary>
    public const double DeadZone = 0.15;

    /// <summary>How far the stick must be pushed to count as a menu direction.</summary>
    public const double StickMenuThreshold = 0.5;

    /// <summary>How far the stick must come back before a menu direction counts as released.</summary>
    public const double StickMenuRelease = 0.3;

    /// <summary>Seconds after a stick release during which that axis cannot act again, either way (spring-back overshoot).</summary>
    public const double StickSettleSeconds = 0.08;

    private static readonly string[] MenuDirections = { MenuUp, MenuDown, MenuLeft, MenuRight };

    /// <summary>The default profile: A fires, B drops a bomb.</summary>
    public static readonly InputBindingProfile Classic = new InputBindingProfile(nameof(GamepadProfile.Classic))
        .Bind(MoveLeft, Key(VirtualKey.Left), Key(VirtualKey.A), InputBinding.DPad(StickDirection.Left))
        .Bind(MoveRight, Key(VirtualKey.Right), Key(VirtualKey.D), InputBinding.DPad(StickDirection.Right))
        .Bind(Fire, Key(VirtualKey.Space), Button(SdlGamepadButtons.A))
        .Bind(Bomb, Key(VirtualKey.LeftShift), Key(VirtualKey.Shift), Button(SdlGamepadButtons.B))
        .Bind(MenuUp, Key(VirtualKey.Up), InputBinding.DPad(StickDirection.Up), Stick(StickDirection.Up))
        .Bind(MenuDown, Key(VirtualKey.Down), InputBinding.DPad(StickDirection.Down), Stick(StickDirection.Down))
        .Bind(MenuLeft, Key(VirtualKey.Left), InputBinding.DPad(StickDirection.Left), Stick(StickDirection.Left))
        .Bind(MenuRight, Key(VirtualKey.Right), InputBinding.DPad(StickDirection.Right), Stick(StickDirection.Right))
        .Bind(Confirm, Key(VirtualKey.Enter), Button(SdlGamepadButtons.A))
        .Bind(Back, Key(VirtualKey.Escape), Button(SdlGamepadButtons.B))
        .Bind(Pause, Key(VirtualKey.Escape))
        .Bind(Start, Button(SdlGamepadButtons.Start))
        .Bind(KenneyLink, Key(VirtualKey.K), Button(SdlGamepadButtons.Y))
        .Bind(Other, Key(VirtualKey.W), Key(VirtualKey.S), Key(VirtualKey.Z), Key(VirtualKey.X), Key(VirtualKey.P),
            Key(VirtualKey.RightShift), Button(SdlGamepadButtons.X), Button(SdlGamepadButtons.Back),
            Button(SdlGamepadButtons.LeftShoulder), Button(SdlGamepadButtons.RightShoulder));

    /// <summary>The right shoulder fires and the left shoulder drops a bomb; A and B stay confirm and back.</summary>
    public static readonly InputBindingProfile Shoulder = Classic.Copy(nameof(GamepadProfile.Shoulder))
        .Rebind(Fire, Key(VirtualKey.Space), Button(SdlGamepadButtons.RightShoulder))
        .Rebind(Bomb, Key(VirtualKey.LeftShift), Key(VirtualKey.Shift), Button(SdlGamepadButtons.LeftShoulder));

    /// <summary>The binding profile of a settings choice.</summary>
    /// <param name="profile">The choice.</param>
    /// <returns><see cref="Shoulder"/> or <see cref="Classic"/>.</returns>
    public static InputBindingProfile ProfileFor(GamepadProfile profile) => profile == GamepadProfile.Shoulder ? Shoulder : Classic;

    /// <summary>Gives a map the game's stick numbers and menu repeat timing.</summary>
    /// <param name="map">The map.</param>
    /// <returns>The same map.</returns>
    public static InputActionMap Configure(InputActionMap map)
    {
        ArgumentNullException.ThrowIfNull(map);
        map.StickDeadZone = DeadZone;
        map.StickPressThreshold = StickMenuThreshold;
        map.StickReleaseThreshold = StickMenuRelease;
        map.StickSettleSeconds = StickSettleSeconds;
        SetMenuRepeat(map, RepeatInterval);
        return map;
    }

    /// <summary>Sets the repeat interval of the four menu directions (<see cref="NameEntryRepeatInterval"/> on the name entry).</summary>
    /// <param name="map">The map.</param>
    /// <param name="interval">Seconds between repeats.</param>
    public static void SetMenuRepeat(InputActionMap map, double interval)
    {
        foreach (var direction in MenuDirections)
        {
            map.SetRepeat(direction, new InputRepeat(RepeatDelay, interval));
        }
    }

    /// <summary>The held input for play in the current step (the left stick added to the digital movement).</summary>
    /// <param name="map">The map, after this step's <see cref="InputActionMap.Update"/>.</param>
    /// <returns>The input.</returns>
    public static GameInput ReadPlay(InputActionMap map) =>
        new GameInput(map.GetAxis(MoveLeft, MoveRight, GamepadStick.Left), map.IsHeld(Fire), map.IsHeld(Bomb));

    /// <summary>The menu presses (and held-direction repeats) of the current step.</summary>
    /// <param name="map">The map, after this step's <see cref="InputActionMap.Update"/>.</param>
    /// <param name="clicked">Whether the mouse was clicked this step (it counts as input).</param>
    /// <returns>The input.</returns>
    public static MenuInput ReadMenu(InputActionMap map, bool clicked = false) => new MenuInput(
        map.IsTriggered(MenuUp), map.IsTriggered(MenuDown), map.IsTriggered(MenuLeft), map.IsTriggered(MenuRight),
        map.WasPressed(Confirm), map.WasPressed(Back), map.WasPressed(Pause), map.WasPressed(Start),
        map.WasPressed(KenneyLink), map.AnyPressed || clicked);

    private static InputBinding Key(VirtualKey key) => InputBinding.Key((int)key, key.ToString());

    private static InputBinding Button(string button) => InputBinding.GamepadButton(button);

    private static InputBinding Stick(StickDirection direction) => InputBinding.StickPush(GamepadStick.Left, direction);
}
