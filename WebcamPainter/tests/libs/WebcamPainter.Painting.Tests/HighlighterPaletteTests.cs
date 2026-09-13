using CodeBrix.Imaging;
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

    [Fact]
    public void TextColor_is_white_on_the_dark_inks()
    {
        //Arrange
        var darkInkNames = new[] { "Red", "Blue", "Indigo", "Violet" };
        var white = Color.FromRgb(255, 255, 255);

        //Act
        var captionColors = HighlighterPalette.Colors
            .Where(c => darkInkNames.Contains(c.Name))
            .Select(c => c.TextColor)
            .ToList();

        //Assert
        captionColors.Count.Should().Be(darkInkNames.Length);
        captionColors.All(c => c == white).Should().Be(true);
    }

    [Fact]
    public void TextColor_is_black_on_the_light_inks()
    {
        //Arrange
        var lightInkNames = new[] { "Orange", "Yellow", "Green" };
        var black = Color.FromRgb(0, 0, 0);

        //Act
        var captionColors = HighlighterPalette.Colors
            .Where(c => lightInkNames.Contains(c.Name))
            .Select(c => c.TextColor)
            .ToList();

        //Assert
        captionColors.Count.Should().Be(lightInkNames.Length);
        captionColors.All(c => c == black).Should().Be(true);
    }

    [Fact]
    public void TextColor_follows_luminance_rather_than_the_name()
    {
        //Arrange
        var paleLemon = new HighlighterColor("Pale lemon", Color.FromRgb(250, 250, 180));
        var deepTeal = new HighlighterColor("Deep teal", Color.FromRgb(0, 70, 80));

        //Assert
        (paleLemon.TextColor == Color.FromRgb(0, 0, 0)).Should().Be(true);
        (deepTeal.TextColor == Color.FromRgb(255, 255, 255)).Should().Be(true);
    }
}
