using BrixInvaders.Game.Input;
using BrixInvaders.Game.Screens;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.Game.Tests.Screens;

public class HighScoreEntryScreenTests
{
    [Fact]
    public void FooterText_uses_gamepad_glyphs_and_mentions_the_stick_and_holding()
    {
        //Act
        var text = HighScoreEntryScreen.FooterText(InputDevice.Gamepad, 0);

        //Assert
        text.Should().Contain("[D-PAD] [STICK]");
        text.Should().Contain("hold to scroll");
        text.Should().Contain("[A] Next");
        text.Should().Contain("[B] Previous");
    }

    [Fact]
    public void FooterText_uses_keyboard_glyphs_and_says_done_on_the_last_letter()
    {
        //Act
        var text = HighScoreEntryScreen.FooterText(InputDevice.Keyboard, 2);

        //Assert
        text.Should().StartWith("[ARROWS]");
        text.Should().Contain("[ENTER] Done");
        text.Should().Contain("[ESC] Previous");
    }
}
