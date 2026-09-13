using SilverAssertions;
using Xunit;

namespace RedisSetupTool.TerminalView.Tests;

/// <summary>Covers the lines the application writes into a terminal itself.</summary>
public class TerminalTextTests
{
    /// <summary>An error is a line of its own, in red, and is reset afterwards.</summary>
    [Fact]
    public void Error_WhenThereIsAMessage_WrapsItInItsOwnRedLine()
    {
        //Act
        var line = TerminalText.Error("No usable shell in this image.");

        //Assert
        line.Should().Be("\r\n\x1b[31mNo usable shell in this image.\x1b[0m\r\n");
    }

    /// <summary>Nothing to say means nothing to feed, rather than a stray blank line.</summary>
    [Fact]
    public void Error_WhenThereIsNoMessage_ProducesNothing()
    {
        //Act
        var fromNull = TerminalText.Error(null);
        var fromEmpty = TerminalText.Error(string.Empty);

        //Assert
        fromNull.Should().BeEmpty();
        fromEmpty.Should().BeEmpty();
    }
}
