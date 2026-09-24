using System;
using System.Linq;
using BrixInvaders.Game.Input;
using BrixInvaders.GameLogic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Input;

public class InputMapperTests
{
    private const double Frame = 1.0 / 60;

    /// <summary>Holds one sample for a while, one sample and one menu read per frame; counts the frames that saw the direction.</summary>
    private static int Hold(InputMapper mapper, RawInput raw, double seconds, Func<MenuInput, bool> direction)
    {
        var count = 0;
        var frames = (int)Math.Round(seconds / Frame);
        for (var i = 0; i < frames; i++)
        {
            mapper.Sample(raw, Frame);
            if (direction(mapper.TakeMenuInput()))
            {
                count++;
            }
        }

        return count;
    }

    private static InputMapper Started(RawInput first = default)
    {
        var mapper = new InputMapper();
        mapper.Sample(first);
        return mapper;
    }

    [Fact]
    public void GameInput_is_none_before_any_sample() => new InputMapper().GameInput.HasAnyInput.Should().BeFalse();

    [Theory]
    [InlineData(InputKeys.Left, -1.0)]
    [InlineData(InputKeys.A, -1.0)]
    [InlineData(InputKeys.Right, 1.0)]
    [InlineData(InputKeys.D, 1.0)]
    [InlineData(InputKeys.Left | InputKeys.Right, 0.0)]
    public void GameInput_moves_with_the_keyboard(InputKeys keys, double expected)
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(keys));

        //Assert
        mapper.GameInput.MoveAxis.Should().Be(expected);
    }

    [Fact]
    public void GameInput_fires_and_bombs_from_the_keyboard()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(InputKeys.Space | InputKeys.LeftShift));

        //Assert
        mapper.GameInput.Fire.Should().BeTrue();
        mapper.GameInput.Bomb.Should().BeTrue();
    }

    [Fact]
    public void GameInput_uses_A_and_B_on_the_classic_profile()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(buttons: PadButtons.A | PadButtons.B));

        //Assert
        mapper.GameInput.Fire.Should().BeTrue();
        mapper.GameInput.Bomb.Should().BeTrue();
    }

    [Fact]
    public void GameInput_uses_the_shoulders_on_the_shoulder_profile()
    {
        //Arrange
        var mapper = Started();
        mapper.Profile = GamepadProfile.Shoulder;

        //Act
        mapper.Sample(new RawInput(buttons: PadButtons.A | PadButtons.B));
        var faceButtons = mapper.GameInput;
        mapper.Sample(new RawInput(buttons: PadButtons.RightShoulder | PadButtons.LeftShoulder));

        //Assert
        faceButtons.Fire.Should().BeFalse();
        faceButtons.Bomb.Should().BeFalse();
        mapper.GameInput.Fire.Should().BeTrue();
        mapper.GameInput.Bomb.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.1, 0.0)]
    [InlineData(-0.149, 0.0)]
    [InlineData(0.15, 0.15)]
    [InlineData(-0.6, -0.6)]
    [InlineData(1.4, 1.0)]
    public void ApplyDeadZone_zeroes_a_resting_stick_only(double raw, double expected) =>
        InputMapper.ApplyDeadZone(raw).Should().BeApproximately(expected, 1e-9);

    [Fact]
    public void GameInput_moves_with_the_stick_and_the_dpad()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(stickX: 0.5));
        var stick = mapper.GameInput.MoveAxis;
        mapper.Sample(new RawInput(buttons: PadButtons.DPadLeft));

        //Assert
        stick.Should().BeApproximately(0.5, 1e-9);
        mapper.GameInput.MoveAxis.Should().Be(-1.0);
    }

    [Fact]
    public void keyboard_and_gamepad_work_at_the_same_time()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(InputKeys.Right, PadButtons.A));

        //Assert
        mapper.GameInput.MoveAxis.Should().Be(1.0);
        mapper.GameInput.Fire.Should().BeTrue();
    }

    [Fact]
    public void TakeMenuInput_latches_a_tap_until_it_is_taken()
    {
        //Arrange
        var mapper = Started();
        mapper.Sample(new RawInput(InputKeys.Enter));
        mapper.Sample(new RawInput());

        //Act
        var first = mapper.TakeMenuInput();
        var second = mapper.TakeMenuInput();

        //Assert
        first.Confirm.Should().BeTrue();
        second.HasAnyInput.Should().BeFalse();
    }

    [Fact]
    public void TakeMenuInput_does_not_repeat_a_held_key_before_the_repeat_delay()
    {
        //Arrange
        var mapper = Started();
        mapper.Sample(new RawInput(InputKeys.Down));
        mapper.TakeMenuInput();

        //Act
        mapper.Sample(new RawInput(InputKeys.Down));
        var held = mapper.TakeMenuInput();

        //Assert
        held.Down.Should().BeFalse();
    }

    [Fact]
    public void a_key_already_held_when_sampling_starts_is_not_a_press()
    {
        //Arrange
        var mapper = Started(new RawInput(InputKeys.Enter));

        //Act
        mapper.Sample(new RawInput(InputKeys.Enter));

        //Assert
        mapper.TakeMenuInput().Confirm.Should().BeFalse();
    }

    [Fact]
    public void RequestPause_latches_a_pause_without_any_key_or_button()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.RequestPause();
        var menu = mapper.TakeMenuInput();
        var names = mapper.TakePressedNames();

        //Assert
        menu.Pause.Should().BeTrue();
        menu.Back.Should().BeFalse();
        names.Should().Contain("pause request (window hidden)");
    }

    [Fact]
    public void Escape_is_back_and_pause()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(InputKeys.Escape));
        var menu = mapper.TakeMenuInput();

        //Assert
        menu.Back.Should().BeTrue();
        menu.Pause.Should().BeTrue();
        menu.Start.Should().BeFalse();
    }

    [Theory]
    [InlineData(PadButtons.A, true, false, false, false)]
    [InlineData(PadButtons.B, false, true, false, false)]
    [InlineData(PadButtons.Start, false, false, true, false)]
    [InlineData(PadButtons.Y, false, false, false, true)]
    public void gamepad_buttons_map_to_menu_actions(PadButtons button, bool confirm, bool back, bool start, bool link)
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(buttons: button));
        var menu = mapper.TakeMenuInput();

        //Assert
        menu.Confirm.Should().Be(confirm);
        menu.Back.Should().Be(back);
        menu.Start.Should().Be(start);
        menu.KenneyLink.Should().Be(link);
    }

    [Fact]
    public void K_opens_the_Kenney_link()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(InputKeys.K));

        //Assert
        mapper.TakeMenuInput().KenneyLink.Should().BeTrue();
    }

    [Fact]
    public void the_dpad_navigates_menus()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(buttons: PadButtons.DPadUp | PadButtons.DPadRight));
        var menu = mapper.TakeMenuInput();

        //Assert
        menu.Up.Should().BeTrue();
        menu.Right.Should().BeTrue();
    }

    [Fact]
    public void the_stick_navigates_once_per_push_and_rearms_when_released()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(stickY: 0.9));
        var pushed = mapper.TakeMenuInput();
        mapper.Sample(new RawInput(stickY: 0.95), Frame);
        var held = mapper.TakeMenuInput();
        mapper.Sample(new RawInput(stickY: 0.1), Frame);
        mapper.Sample(new RawInput(stickY: 0.1), 0.2);
        mapper.Sample(new RawInput(stickY: 0.8), Frame);
        var again = mapper.TakeMenuInput();

        //Assert
        pushed.Up.Should().BeTrue();
        held.Up.Should().BeFalse();
        again.Up.Should().BeTrue();
    }

    [Fact]
    public void the_stick_down_and_sideways_are_menu_directions()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(stickX: -0.7, stickY: -0.7));
        var menu = mapper.TakeMenuInput();

        //Assert
        menu.Down.Should().BeTrue();
        menu.Left.Should().BeTrue();
    }

    [Fact]
    public void LastDevice_follows_the_device_that_pressed_last()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(buttons: PadButtons.A));
        var afterPad = mapper.LastDevice;
        mapper.Sample(new RawInput(InputKeys.Space));
        var afterKey = mapper.LastDevice;
        mapper.Sample(new RawInput(InputKeys.Space, stickX: 0.9));

        //Assert
        afterPad.Should().Be(InputDevice.Gamepad);
        afterKey.Should().Be(InputDevice.Keyboard);
        mapper.LastDevice.Should().Be(InputDevice.Gamepad);
    }

    [Fact]
    public void TakePressedNames_names_every_press_once()
    {
        //Arrange
        var mapper = Started();
        mapper.Sample(new RawInput(InputKeys.Enter, PadButtons.Start));

        //Act
        var names = mapper.TakePressedNames();
        var again = mapper.TakePressedNames();

        //Assert
        names.Should().Contain("key Enter");
        names.Should().Contain("button Start");
        again.Should().BeEmpty();
    }

    [Fact]
    public void other_keys_count_as_input_without_a_menu_action()
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(InputKeys.Space));
        var menu = mapper.TakeMenuInput();

        //Assert
        menu.Other.Should().BeTrue();
        menu.HasAnyInput.Should().BeTrue();
        menu.Confirm.Should().BeFalse();
    }

    [Fact]
    public void RegisterClick_is_taken_once_and_counts_as_keyboard_and_mouse()
    {
        //Arrange
        var mapper = Started();
        mapper.LastDevice = InputDevice.Gamepad;

        //Act
        mapper.RegisterClick(100, 200);
        var taken = mapper.TryTakeClick(out var x, out var y);
        var again = mapper.TryTakeClick(out _, out _);

        //Assert
        taken.Should().BeTrue();
        x.Should().Be(100);
        y.Should().Be(200);
        again.Should().BeFalse();
        mapper.LastDevice.Should().Be(InputDevice.Keyboard);
    }

    [Fact]
    public void ClearLatched_drops_pending_presses()
    {
        //Arrange
        var mapper = Started();
        mapper.Sample(new RawInput(InputKeys.Enter));
        mapper.RegisterClick(1, 1);

        //Act
        mapper.ClearLatched();

        //Assert
        mapper.TakeMenuInput().HasAnyInput.Should().BeFalse();
        mapper.TakePressedNames().Any().Should().BeFalse();
        mapper.TryTakeClick(out _, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData(1.0, true)]
    [InlineData(0.55, true)]
    [InlineData(-1.0, false)]
    [InlineData(-0.55, false)]
    public void the_stick_Y_polarity_is_up_positive(double stickY, bool up)
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(stickY: stickY), Frame);
        var menu = mapper.TakeMenuInput();

        //Assert
        menu.Up.Should().Be(up);
        menu.Down.Should().Be(!up);
        mapper.TakePressedNames().Should().Contain(up ? "stick Up" : "stick Down");
    }

    [Theory]
    [InlineData(1.0, false)]
    [InlineData(-1.0, true)]
    public void the_stick_X_polarity_is_right_positive(double stickX, bool left)
    {
        //Arrange
        var mapper = Started();

        //Act
        mapper.Sample(new RawInput(stickX: stickX), Frame);
        var menu = mapper.TakeMenuInput();

        //Assert
        menu.Left.Should().Be(left);
        menu.Right.Should().Be(!left);
    }

    [Theory]
    [InlineData(0.9, -0.7)]
    [InlineData(-0.9, 0.7)]
    public void a_released_stick_springing_back_past_centre_is_not_a_push_the_other_way(double pushed, double overshoot)
    {
        //Arrange
        var mapper = Started();
        mapper.Sample(new RawInput(stickY: pushed), Frame);
        mapper.TakeMenuInput();

        //Act - released: the stick snaps back through centre and overshoots for a few milliseconds
        mapper.Sample(new RawInput(stickY: 0.0), 0.004);
        mapper.Sample(new RawInput(stickY: overshoot), 0.004);
        mapper.Sample(new RawInput(stickY: overshoot / 3), 0.004);
        mapper.Sample(new RawInput(stickY: -0.03), 0.004);
        var menu = mapper.TakeMenuInput();

        //Assert
        menu.Up.Should().BeFalse();
        menu.Down.Should().BeFalse();
    }

    [Fact]
    public void a_deliberate_push_the_other_way_still_acts_once_the_stick_has_settled()
    {
        //Arrange
        var mapper = Started();
        mapper.Sample(new RawInput(stickY: 0.9), Frame);
        mapper.Sample(new RawInput(stickY: 0.0), Frame);
        mapper.TakeMenuInput();

        //Act
        mapper.Sample(new RawInput(stickY: -0.9), 0.01);
        var early = mapper.TakeMenuInput();
        mapper.Sample(new RawInput(stickY: -0.9), InputMapper.StickSettleSeconds);
        var settled = mapper.TakeMenuInput();

        //Assert
        early.Down.Should().BeFalse();
        settled.Down.Should().BeTrue();
    }

    [Fact]
    public void a_stick_already_pushed_when_sampling_starts_neither_presses_nor_repeats()
    {
        //Arrange
        var mapper = Started(new RawInput(stickY: 1.0));

        //Act
        var held = Hold(mapper, new RawInput(stickY: 1.0), 1.0, m => m.Up);
        mapper.Sample(new RawInput(), Frame);
        mapper.Sample(new RawInput(), 0.2);
        var afterRelease = Hold(mapper, new RawInput(stickY: 1.0), Frame, m => m.Up);

        //Assert
        held.Should().Be(0);
        afterRelease.Should().Be(1);
    }

    public static TheoryData<RawInput> HeldUps => new TheoryData<RawInput>
    {
        new RawInput(InputKeys.Up),
        new RawInput(buttons: PadButtons.DPadUp),
        new RawInput(stickY: 1.0),
    };

    [Theory]
    [MemberData(nameof(HeldUps))]
    public void a_hold_shorter_than_the_repeat_delay_acts_once(RawInput held) =>
        Hold(Started(), held, InputMapper.RepeatDelay - (2 * Frame), m => m.Up).Should().Be(1);

    [Theory]
    [MemberData(nameof(HeldUps))]
    public void a_hold_past_the_delay_repeats_at_the_interval(RawInput held)
    {
        //Arrange
        var mapper = Started();

        //Act - the press, then 1 s past the delay at 0.1 s = 10 repeats
        var count = Hold(mapper, held, InputMapper.RepeatDelay + 1.0 - (Frame / 2), m => m.Up);

        //Assert
        count.Should().BeInRange(10, 12);
    }

    [Fact]
    public void the_repeat_interval_follows_MenuRepeatInterval()
    {
        //Arrange
        var menus = Started();
        var nameEntry = Started();
        nameEntry.MenuRepeatInterval = InputMapper.NameEntryRepeatInterval;

        //Act
        var menuCount = Hold(menus, new RawInput(InputKeys.Down), InputMapper.RepeatDelay + 1.2, m => m.Down);
        var nameCount = Hold(nameEntry, new RawInput(InputKeys.Down), InputMapper.RepeatDelay + 1.2, m => m.Down);

        //Assert
        menuCount.Should().BeInRange(12, 14);
        nameCount.Should().BeInRange(10, 12);
        nameCount.Should().BeLessThan(menuCount);
    }

    [Fact]
    public void releasing_resets_the_repeat_clock()
    {
        //Arrange
        var mapper = Started();
        Hold(mapper, new RawInput(buttons: PadButtons.DPadRight), InputMapper.RepeatDelay + 0.5, m => m.Right);

        //Act
        var released = Hold(mapper, new RawInput(), 0.2, m => m.Right);
        var again = Hold(mapper, new RawInput(buttons: PadButtons.DPadRight), InputMapper.RepeatDelay - (2 * Frame), m => m.Right);

        //Assert
        released.Should().Be(0);
        again.Should().Be(1);
    }

    [Fact]
    public void holding_one_direction_on_two_devices_acts_once()
    {
        //Arrange
        var mapper = Started();
        mapper.Sample(new RawInput(InputKeys.Left), Frame);
        mapper.TakeMenuInput();

        //Act
        mapper.Sample(new RawInput(InputKeys.Left, PadButtons.DPadLeft), Frame);
        var menu = mapper.TakeMenuInput();

        //Assert
        menu.Left.Should().BeFalse();
        mapper.LastDevice.Should().Be(InputDevice.Gamepad);
    }

    [Fact]
    public void repeats_do_not_add_names_to_the_input_log()
    {
        //Arrange
        var mapper = Started();

        //Act
        Hold(mapper, new RawInput(InputKeys.Up), InputMapper.RepeatDelay + 0.5, m => m.Up);
        var names = mapper.TakePressedNames();

        //Assert
        names.Should().ContainSingle().Which.Should().Be("key Up");
    }
}
