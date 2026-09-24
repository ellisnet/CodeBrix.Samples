using System;
using BrixInvaders.Game.Input;
using BrixInvaders.GameLogic;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Input;

/// <summary>
/// Scenario fence for the high-score name entry driven through the real <see cref="InputMapper"/>: Up (arrow, D-pad
/// or stick pushed up) moves to the NEXT character (A -> B, the "^" arrow above the letter), Down to the previous
/// one, both wrap, and a held direction scrolls.
/// </summary>
public class NameEntryInputTests
{
    private const double Frame = 1.0 / 60;

    private static (InputMapper Mapper, NameEntry Entry) Started(string name = "AAA")
    {
        var mapper = new InputMapper { MenuRepeatInterval = InputMapper.NameEntryRepeatInterval };
        mapper.Sample(new RawInput());
        return (mapper, new NameEntry(name));
    }

    private static void Frames(InputMapper mapper, NameEntry entry, RawInput raw, int frames, double elapsed = Frame)
    {
        for (var i = 0; i < frames; i++)
        {
            mapper.Sample(raw, elapsed);
            entry.Handle(mapper.TakeMenuInput());
        }
    }

    public static TheoryData<RawInput> Ups => new TheoryData<RawInput>
    {
        new RawInput(InputKeys.Up),
        new RawInput(buttons: PadButtons.DPadUp),
        new RawInput(stickY: 1.0),
    };

    public static TheoryData<RawInput> Downs => new TheoryData<RawInput>
    {
        new RawInput(InputKeys.Down),
        new RawInput(buttons: PadButtons.DPadDown),
        new RawInput(stickY: -1.0),
    };

    [Theory]
    [MemberData(nameof(Ups))]
    public void a_tap_up_moves_to_the_next_letter(RawInput up)
    {
        //Arrange
        var (mapper, entry) = Started();

        //Act
        Frames(mapper, entry, up, 3);
        Frames(mapper, entry, new RawInput(), 3);

        //Assert
        entry.Name.Should().Be("BAA");
    }

    [Theory]
    [MemberData(nameof(Downs))]
    public void a_tap_down_moves_to_the_previous_letter_and_wraps(RawInput down)
    {
        //Arrange
        var (mapper, entry) = Started();

        //Act
        Frames(mapper, entry, down, 3);
        Frames(mapper, entry, new RawInput(), 3);

        //Assert
        entry.Name.Should().Be("9AA");
    }

    [Fact]
    public void a_stick_flicked_up_and_released_moves_up_even_when_it_springs_back_past_centre()
    {
        //Arrange
        var (mapper, entry) = Started("JAA");

        //Act - push up for a few frames, let go: the stick snaps back and overshoots downward for ~10 ms
        Frames(mapper, entry, new RawInput(stickY: 1.0), 4);
        Frames(mapper, entry, new RawInput(stickY: 0.1), 1, 0.003);
        Frames(mapper, entry, new RawInput(stickY: -0.8), 1, 0.003);
        Frames(mapper, entry, new RawInput(stickY: -0.6), 1, 0.003);
        Frames(mapper, entry, new RawInput(stickY: 0.2), 1, 0.003);
        Frames(mapper, entry, new RawInput(stickX: 0.08, stickY: -0.03), 10);

        //Assert
        entry.Name.Should().Be("KAA");
    }

    [Fact]
    public void repeated_stick_flicks_up_each_move_one_letter()
    {
        //Arrange
        var (mapper, entry) = Started();

        //Act - three flicks with spring-back overshoot, a normal thumb rhythm
        for (var i = 0; i < 3; i++)
        {
            Frames(mapper, entry, new RawInput(stickY: 1.0), 5);
            Frames(mapper, entry, new RawInput(stickY: -0.7), 1, 0.004);
            Frames(mapper, entry, new RawInput(), 8);
        }

        //Assert
        entry.Name.Should().Be("DAA");
    }

    [Theory]
    [MemberData(nameof(Ups))]
    public void holding_up_scrolls_at_a_constant_rate(RawInput up)
    {
        //Arrange
        var (mapper, entry) = Started();
        var frames = (int)Math.Round((InputMapper.RepeatDelay + (10 * InputMapper.NameEntryRepeatInterval) - (Frame / 2)) / Frame);

        //Act - the press, then ten repeats
        Frames(mapper, entry, up, frames);
        Frames(mapper, entry, new RawInput(), 3);

        //Assert
        entry.LetterAt(0).Should().BeOneOf('K', 'L');
    }

    [Fact]
    public void holding_down_from_A_scrolls_back_through_the_digits_to_Z()
    {
        //Arrange
        var (mapper, entry) = Started();
        var seconds = InputMapper.RepeatDelay + (10 * InputMapper.NameEntryRepeatInterval) - (Frame / 2);

        //Act - the press (A -> 9) then ten repeats (8 .. 0, Z)
        Frames(mapper, entry, new RawInput(buttons: PadButtons.DPadDown), (int)Math.Round(seconds / Frame));
        Frames(mapper, entry, new RawInput(), 3);

        //Assert
        entry.LetterAt(0).Should().Be('Z');
    }

    [Fact]
    public void left_and_right_move_between_letters_and_confirm_finishes_on_the_last()
    {
        //Arrange
        var (mapper, entry) = Started();

        //Act
        Frames(mapper, entry, new RawInput(stickX: 1.0), 2);
        Frames(mapper, entry, new RawInput(), 10);
        Frames(mapper, entry, new RawInput(stickY: 1.0), 2);
        Frames(mapper, entry, new RawInput(), 10);
        Frames(mapper, entry, new RawInput(buttons: PadButtons.DPadRight), 2);
        Frames(mapper, entry, new RawInput(), 2);
        Frames(mapper, entry, new RawInput(buttons: PadButtons.A), 2);

        //Assert
        entry.Name.Should().Be("ABA");
        entry.IsComplete.Should().BeTrue();
    }
}
