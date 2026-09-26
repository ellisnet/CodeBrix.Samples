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
/// Scenario fence for the high-score name entry driven through the game's controls on the engine <see cref="InputActionMap"/>: Up (arrow, D-pad
/// or stick pushed up) moves to the NEXT character (A -> B, the "^" arrow above the letter), Down to the previous
/// one, both wrap, and a held direction scrolls.
/// </summary>
public class NameEntryInputTests
{
    private const double Frame = 1.0 / 60;

    private readonly FakeDevices _devices = new FakeDevices();

    private (InputActionMap Map, NameEntry Entry) Started(string name = "AAA")
    {
        var map = _devices.CreateMap();
        GameControls.SetMenuRepeat(map, GameControls.NameEntryRepeatInterval);
        map.Update(Frame);
        return (map, new NameEntry(name));
    }

    //held is the devices after the Set that the call site makes; it is named for the reader only
    private static void Frames(InputActionMap map, NameEntry entry, FakeDevices held, int frames, double elapsed = Frame)
    {
        for (var i = 0; i < frames; i++)
        {
            map.Update(elapsed);
            entry.Handle(GameControls.ReadMenu(map));
        }
    }

    private FakeDevices Up(string device) => device switch
    {
        "key" => _devices.Key(VirtualKey.Up),
        "dpad" => _devices.Button(SdlGamepadButtons.DPadUp),
        _ => _devices.Set(stickY: 1.0),
    };

    private FakeDevices Down(string device) => device switch
    {
        "key" => _devices.Key(VirtualKey.Down),
        "dpad" => _devices.Button(SdlGamepadButtons.DPadDown),
        _ => _devices.Set(stickY: -1.0),
    };

    [Theory]
    [InlineData("key")]
    [InlineData("dpad")]
    [InlineData("stick")]
    public void a_tap_up_moves_to_the_next_letter(string device)
    {
        //Arrange
        var (map, entry) = Started();

        //Act
        Frames(map, entry, Up(device), 3);
        Frames(map, entry, _devices.None(), 3);

        //Assert
        entry.Name.Should().Be("BAA");
    }

    [Theory]
    [InlineData("key")]
    [InlineData("dpad")]
    [InlineData("stick")]
    public void a_tap_down_moves_to_the_previous_letter_and_wraps(string device)
    {
        //Arrange
        var (map, entry) = Started();

        //Act
        Frames(map, entry, Down(device), 3);
        Frames(map, entry, _devices.None(), 3);

        //Assert
        entry.Name.Should().Be("9AA");
    }

    [Fact]
    public void a_stick_flicked_up_and_released_moves_up_even_when_it_springs_back_past_centre()
    {
        //Arrange
        var (map, entry) = Started("JAA");

        //Act - push up for a few frames, let go: the stick snaps back and overshoots downward for ~10 ms
        Frames(map, entry, _devices.Set(stickY: 1.0), 4);
        Frames(map, entry, _devices.Set(stickY: 0.1), 1, 0.003);
        Frames(map, entry, _devices.Set(stickY: -0.8), 1, 0.003);
        Frames(map, entry, _devices.Set(stickY: -0.6), 1, 0.003);
        Frames(map, entry, _devices.Set(stickY: 0.2), 1, 0.003);
        Frames(map, entry, _devices.Set(stickX: 0.08, stickY: -0.03), 10);

        //Assert
        entry.Name.Should().Be("KAA");
    }

    [Fact]
    public void repeated_stick_flicks_up_each_move_one_letter()
    {
        //Arrange
        var (map, entry) = Started();

        //Act - three flicks with spring-back overshoot, a normal thumb rhythm
        for (var i = 0; i < 3; i++)
        {
            Frames(map, entry, _devices.Set(stickY: 1.0), 5);
            Frames(map, entry, _devices.Set(stickY: -0.7), 1, 0.004);
            Frames(map, entry, _devices.None(), 8);
        }

        //Assert
        entry.Name.Should().Be("DAA");
    }

    [Theory]
    [InlineData("key")]
    [InlineData("dpad")]
    [InlineData("stick")]
    public void holding_up_scrolls_at_a_constant_rate(string device)
    {
        //Arrange
        var (map, entry) = Started();
        var frames = (int)Math.Round((GameControls.RepeatDelay + (10 * GameControls.NameEntryRepeatInterval) - (Frame / 2)) / Frame);

        //Act - the press, then ten repeats
        Frames(map, entry, Up(device), frames);
        Frames(map, entry, _devices.None(), 3);

        //Assert
        entry.LetterAt(0).Should().BeOneOf('K', 'L');
    }

    [Fact]
    public void holding_down_from_A_scrolls_back_through_the_digits_to_Z()
    {
        //Arrange
        var (map, entry) = Started();
        var seconds = GameControls.RepeatDelay + (10 * GameControls.NameEntryRepeatInterval) - (Frame / 2);

        //Act - the press (A -> 9) then ten repeats (8 .. 0, Z)
        Frames(map, entry, _devices.Button(SdlGamepadButtons.DPadDown), (int)Math.Round(seconds / Frame));
        Frames(map, entry, _devices.None(), 3);

        //Assert
        entry.LetterAt(0).Should().Be('Z');
    }

    [Fact]
    public void left_and_right_move_between_letters_and_confirm_finishes_on_the_last()
    {
        //Arrange
        var (map, entry) = Started();

        //Act
        Frames(map, entry, _devices.Set(stickX: 1.0), 2);
        Frames(map, entry, _devices.None(), 10);
        Frames(map, entry, _devices.Set(stickY: 1.0), 2);
        Frames(map, entry, _devices.None(), 10);
        Frames(map, entry, _devices.Button(SdlGamepadButtons.DPadRight), 2);
        Frames(map, entry, _devices.None(), 2);
        Frames(map, entry, _devices.Button(SdlGamepadButtons.A), 2);

        //Assert
        entry.Name.Should().Be("ABA");
        entry.IsComplete.Should().BeTrue();
    }
}
