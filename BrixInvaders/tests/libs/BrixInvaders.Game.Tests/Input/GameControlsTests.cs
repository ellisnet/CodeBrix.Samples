using System;
using BrixInvaders.Game.Input;
using BrixInvaders.Game.Tests.Support;
using BrixInvaders.GameLogic;
using CodeBrix.Platform.GameEngine.Input.Actions;
using CodeBrix.Platform.GameEngine.Sdl2.Gamepad;
using SilverAssertions;
using Windows.System;
using Xunit;

namespace BrixInvaders.Game.Tests.Input;

/// <summary>
/// Fences the game's bindings and numbers (DESIGN.md section 13) as read through the engine input-action map. The
/// map's own mechanics (latching, repeat, stick hysteresis) are the engine's and tested there.
/// </summary>
public class GameControlsTests
{
    private const double Frame = 1.0 / 60;

    private static (FakeDevices Devices, InputActionMap Map) Started(GamepadProfile profile = GamepadProfile.Classic)
    {
        var devices = new FakeDevices();
        var map = devices.CreateMap(profile);
        map.Update(Frame);
        return (devices, map);
    }

    private static int Hold(InputActionMap map, double seconds, Func<MenuInput, bool> direction)
    {
        var count = 0;
        for (var t = 0.0; t < seconds; t += Frame)
        {
            map.Update(Frame);
            count += direction(GameControls.ReadMenu(map)) ? 1 : 0;
        }

        return count;
    }

    [Theory]
    [InlineData(VirtualKey.Left, -1.0)]
    [InlineData(VirtualKey.A, -1.0)]
    [InlineData(VirtualKey.Right, 1.0)]
    [InlineData(VirtualKey.D, 1.0)]
    public void the_keyboard_moves_the_ship(VirtualKey key, double expected)
    {
        //Arrange
        var (devices, map) = Started();

        //Act
        devices.Key(key);
        map.Update(Frame);

        //Assert
        GameControls.ReadPlay(map).MoveAxis.Should().Be(expected);
    }

    [Theory]
    [InlineData(0.1, 0.0)]
    [InlineData(0.5, 0.5)]
    [InlineData(-1.0, -1.0)]
    public void the_stick_moves_the_ship_outside_the_dead_zone(double stickX, double expected)
    {
        //Arrange
        var (devices, map) = Started();

        //Act
        devices.Set(stickX: stickX);
        map.Update(Frame);

        //Assert
        GameControls.ReadPlay(map).MoveAxis.Should().BeApproximately(expected, 1e-6);
    }

    [Theory]
    [InlineData(GamepadProfile.Classic, SdlGamepadButtons.A, SdlGamepadButtons.B)]
    [InlineData(GamepadProfile.Shoulder, SdlGamepadButtons.RightShoulder, SdlGamepadButtons.LeftShoulder)]
    public void each_profile_fires_and_bombs_with_its_own_buttons(GamepadProfile profile, string fire, string bomb)
    {
        //Arrange
        var (devices, map) = Started(profile);

        //Act
        devices.Set(buttons: new[] { fire, bomb });
        map.Update(Frame);
        var play = GameControls.ReadPlay(map);

        //Assert
        play.Fire.Should().BeTrue();
        play.Bomb.Should().BeTrue();
    }

    [Fact]
    public void the_shoulder_profile_leaves_A_and_B_to_confirm_and_back()
    {
        //Arrange
        var (devices, map) = Started(GamepadProfile.Shoulder);

        //Act
        devices.Set(buttons: new[] { SdlGamepadButtons.A, SdlGamepadButtons.B });
        map.Update(Frame);
        var menu = GameControls.ReadMenu(map);

        //Assert
        GameControls.ReadPlay(map).HasAnyInput.Should().BeFalse();
        menu.Confirm.Should().BeTrue();
        menu.Back.Should().BeTrue();
    }

    [Theory]
    [InlineData(VirtualKey.Space, true, false)]
    [InlineData(VirtualKey.LeftShift, false, true)]
    [InlineData(VirtualKey.Shift, false, true)]
    public void the_keyboard_fires_and_bombs(VirtualKey key, bool fire, bool bomb)
    {
        //Arrange
        var (devices, map) = Started();

        //Act
        devices.Key(key);
        map.Update(Frame);

        //Assert
        GameControls.ReadPlay(map).Fire.Should().Be(fire);
        GameControls.ReadPlay(map).Bomb.Should().Be(bomb);
    }

    [Theory]
    [InlineData(VirtualKey.Enter, true, false, false, false)]
    [InlineData(VirtualKey.Escape, false, true, true, false)]
    [InlineData(VirtualKey.K, false, false, false, true)]
    public void keys_map_to_menu_actions(VirtualKey key, bool confirm, bool back, bool pause, bool link)
    {
        //Arrange
        var (devices, map) = Started();

        //Act
        devices.Key(key);
        map.Update(Frame);
        var menu = GameControls.ReadMenu(map);

        //Assert
        (menu.Confirm, menu.Back, menu.Pause, menu.KenneyLink).Should().Be((confirm, back, pause, link));
    }

    [Theory]
    [InlineData(SdlGamepadButtons.A, true, false, false, false)]
    [InlineData(SdlGamepadButtons.B, false, true, false, false)]
    [InlineData(SdlGamepadButtons.Start, false, false, true, false)]
    [InlineData(SdlGamepadButtons.Y, false, false, false, true)]
    public void gamepad_buttons_map_to_menu_actions(string button, bool confirm, bool back, bool start, bool link)
    {
        //Arrange
        var (devices, map) = Started();

        //Act
        devices.Button(button);
        map.Update(Frame);
        var menu = GameControls.ReadMenu(map);

        //Assert
        (menu.Confirm, menu.Back, menu.Start, menu.KenneyLink).Should().Be((confirm, back, start, link));
        menu.Pause.Should().BeFalse();
    }

    [Fact]
    public void other_watched_keys_and_a_click_count_as_input_without_a_menu_action()
    {
        //Arrange
        var (devices, map) = Started();

        //Act
        devices.Key(VirtualKey.W);
        map.Update(Frame);
        var key = GameControls.ReadMenu(map);
        devices.None();
        map.Update(Frame);
        var click = GameControls.ReadMenu(map, clicked: true);

        //Assert
        key.HasAnyInput.Should().BeTrue();
        key.Confirm.Should().BeFalse();
        click.HasAnyInput.Should().BeTrue();
    }

    [Fact]
    public void a_tap_shorter_than_a_step_still_fires_and_confirms()
    {
        //Arrange
        var (devices, map) = Started();

        //Act - down and up again between two steps
        devices.Set(new[] { VirtualKey.Space, VirtualKey.Enter });
        map.Poll(0.004);
        devices.None();
        map.Poll(0.004);
        map.Update(Frame);

        //Assert
        GameControls.ReadPlay(map).Fire.Should().BeTrue();
        GameControls.ReadMenu(map).Confirm.Should().BeTrue();
    }

    [Fact]
    public void menu_directions_repeat_after_the_delay_slower_on_the_name_entry()
    {
        //Arrange
        var (menuDevices, menus) = Started();
        var (nameDevices, nameEntry) = Started();
        GameControls.SetMenuRepeat(nameEntry, GameControls.NameEntryRepeatInterval);

        //Act - hold Down for the delay plus 1.2 s
        menuDevices.Key(VirtualKey.Down);
        nameDevices.Key(VirtualKey.Down);
        var menuCount = Hold(menus, GameControls.RepeatDelay + 1.2, m => m.Down);
        var nameCount = Hold(nameEntry, GameControls.RepeatDelay + 1.2, m => m.Down);

        //Assert - the press plus about 12 repeats at 0.1 s, about 10 at 0.12 s
        menuCount.Should().BeInRange(12, 14);
        nameCount.Should().BeInRange(10, 12);
    }

    [Fact]
    public void LastDevice_follows_the_device_that_pressed_last()
    {
        //Arrange
        var (devices, map) = Started();

        //Act
        devices.Button(SdlGamepadButtons.A);
        map.Update(Frame);
        var afterPad = map.LastDevice;
        devices.Key(VirtualKey.Space);
        map.Update(Frame);

        //Assert
        afterPad.Should().Be(InputDeviceKind.Gamepad);
        map.LastDevice.Should().Be(InputDeviceKind.KeyboardMouse);
    }
}
