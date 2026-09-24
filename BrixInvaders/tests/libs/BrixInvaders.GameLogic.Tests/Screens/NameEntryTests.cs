using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class NameEntryTests
{
    [Fact]
    public void a_new_entry_is_AAA_with_the_cursor_on_the_first_letter()
    {
        //Act
        var entry = new NameEntry();

        //Assert
        entry.Name.Should().Be("AAA");
        entry.Cursor.Should().Be(0);
        entry.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void Up_cycles_forward_and_wraps_from_nine_to_A()
    {
        //Arrange
        var entry = new NameEntry("9AA");

        //Act
        entry.Up();

        //Assert
        entry.Name.Should().Be("AAA");
    }

    [Fact]
    public void Up_from_Z_goes_to_zero()
    {
        //Arrange
        var entry = new NameEntry("ZAA");

        //Act
        entry.Up();

        //Assert
        entry.LetterAt(0).Should().Be('0');
    }

    [Fact]
    public void Down_cycles_backward_and_wraps_from_A_to_nine()
    {
        //Arrange
        var entry = new NameEntry();

        //Act
        entry.Down();

        //Assert
        entry.Name.Should().Be("9AA");
    }

    [Fact]
    public void a_full_cycle_in_either_direction_returns_to_the_start()
    {
        //Arrange
        var up = new NameEntry("KAA");
        var down = new NameEntry("KAA");

        //Act
        for (var i = 0; i < NameEntry.Alphabet.Length; i++)
        {
            up.Up();
            down.Down();
        }

        //Assert
        up.Name.Should().Be("KAA");
        down.Name.Should().Be("KAA");
    }

    [Fact]
    public void Left_and_Right_stop_at_the_ends()
    {
        //Arrange
        var entry = new NameEntry();

        //Act
        entry.Left();
        var afterLeft = entry.Cursor;
        entry.Right();
        entry.Right();
        entry.Right();

        //Assert
        afterLeft.Should().Be(0);
        entry.Cursor.Should().Be(2);
    }

    [Fact]
    public void letters_are_edited_under_the_cursor()
    {
        //Arrange
        var entry = new NameEntry();

        //Act
        entry.Right();
        entry.Up();
        entry.Right();
        entry.Down();

        //Assert
        entry.Name.Should().Be("AB9");
    }

    [Fact]
    public void Confirm_moves_right_then_completes_and_freezes_the_entry()
    {
        //Arrange
        var entry = new NameEntry("JER");

        //Act
        entry.Confirm();
        entry.Confirm();
        var beforeLast = entry.IsComplete;
        entry.Confirm();
        entry.Up();
        entry.Left();

        //Assert
        beforeLast.Should().BeFalse();
        entry.IsComplete.Should().BeTrue();
        entry.Name.Should().Be("JER");
        entry.Cursor.Should().Be(2);
    }

    [Fact]
    public void Handle_maps_menu_input_and_reports_completion()
    {
        //Arrange
        var entry = new NameEntry();

        //Act
        entry.Handle(new MenuInput(up: true));
        entry.Handle(new MenuInput(confirm: true));
        entry.Handle(new MenuInput(back: true));
        entry.Handle(new MenuInput(up: true));
        entry.Handle(new MenuInput(right: true));
        entry.Handle(new MenuInput(start: true));
        var completed = entry.Handle(new MenuInput(confirm: true));

        //Assert
        completed.Should().BeTrue();
        entry.Name.Should().Be("CAA");
        entry.Handle(new MenuInput(confirm: true)).Should().BeFalse();
    }

    [Theory]
    [InlineData("abc", "ABC")]
    [InlineData("a", "AAA")]
    [InlineData("TOOLONG", "TOO")]
    [InlineData("a-b", "AAB")]
    [InlineData("", "AAA")]
    [InlineData(null, "AAA")]
    [InlineData("r2d", "R2D")]
    public void Normalize_makes_any_text_a_valid_name(string raw, string expected) => NameEntry.Normalize(raw).Should().Be(expected);
}
