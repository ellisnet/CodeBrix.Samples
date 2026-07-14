using SilverAssertions;
using System.Linq;
using Xunit;

namespace WebcamPainter.Painting.Tests;

public class HighlighterPaletteTests
{
    [Fact]
    public void Colors_is_roygbiv_in_rainbow_order()
    {
        //Arrange
        var expected = new[] { "Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet" };

        //Assert
        HighlighterPalette.Colors.Select(c => c.Name).SequenceEqual(expected).Should().Be(true);
    }

    [Fact]
    public void Color_names_are_unique()
        => HighlighterPalette.Colors.Select(c => c.Name).Distinct().Count()
            .Should().Be(HighlighterPalette.Colors.Count);
}
