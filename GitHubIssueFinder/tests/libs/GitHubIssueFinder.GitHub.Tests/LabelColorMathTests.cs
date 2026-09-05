using GitHubIssueFinder.GitHub;
using SilverAssertions;
using Xunit;
using System;

namespace GitHubIssueFinder.GitHub.Tests;

public class LabelColorMathTests
{
    //GitHub's own "bug" label colour, and the two page grounds the schemes use.
    private const string BugLabelHex = "d73a4a";
    private const uint BugLabel = 0xFFD73A4Au;
    private const uint LightCanvas = 0xFFFFFFFFu;
    private const uint DarkCanvas = 0xFF0D1117u;

    [Fact]
    public void try_parse_hex_reads_six_plain_digits()
    {
        //Act
        var parsed = LabelColorMath.TryParseHex(BugLabelHex, out var argb);

        //Assert
        parsed.Should().BeTrue();
        argb.Should().Be(BugLabel);
    }

    [Fact]
    public void try_parse_hex_reads_a_leading_hash()
    {
        //Act
        var parsed = LabelColorMath.TryParseHex("#d73a4a", out var argb);

        //Assert
        parsed.Should().BeTrue();
        argb.Should().Be(BugLabel);
    }

    [Fact]
    public void try_parse_hex_reads_upper_case_and_surrounding_space()
    {
        //Act
        var upper = LabelColorMath.TryParseHex("D73A4A", out var upperArgb);
        var spaced = LabelColorMath.TryParseHex("  #D73A4A  ", out var spacedArgb);

        //Assert
        upper.Should().BeTrue();
        upperArgb.Should().Be(BugLabel);
        spaced.Should().BeTrue();
        spacedArgb.Should().Be(BugLabel);
    }

    [Fact]
    public void try_parse_hex_rejects_text_that_is_not_a_colour()
    {
        //Act
        var nothing = LabelColorMath.TryParseHex(null, out var nothingArgb);
        var empty = LabelColorMath.TryParseHex("   ", out _);
        var tooShort = LabelColorMath.TryParseHex("abc", out _);
        var tooLong = LabelColorMath.TryParseHex("d73a4a1", out _);
        var notHex = LabelColorMath.TryParseHex("gggggg", out _);

        //Assert
        nothing.Should().BeFalse();
        nothingArgb.Should().Be(0u);
        empty.Should().BeFalse();
        tooShort.Should().BeFalse();
        tooLong.Should().BeFalse();
        notHex.Should().BeFalse();
    }

    [Fact]
    public void pill_background_lays_the_label_faintly_over_a_light_canvas()
    {
        //Act
        var colors = LabelColorMath.PillColors(BugLabel, LightCanvas, darkBase: false);

        //Assert - 18 per cent of the label over white
        colors.Background.Should().Be(0xFFF8DCDEu);
    }

    [Fact]
    public void pill_border_lays_the_label_more_strongly_over_a_light_canvas()
    {
        //Act
        var colors = LabelColorMath.PillColors(BugLabel, LightCanvas, darkBase: false);

        //Assert - 45 per cent of the label over white
        colors.Border.Should().Be(0xFFEDA6AEu);
    }

    [Fact]
    public void pill_colours_blend_toward_a_dark_canvas()
    {
        //Act
        var colors = LabelColorMath.PillColors(BugLabel, DarkCanvas, darkBase: true);

        //Assert
        colors.Background.Should().Be(0xFF311820u);
        colors.Border.Should().Be(0xFF68232Eu);
    }

    [Fact]
    public void pill_text_darkens_a_label_on_a_light_page()
    {
        //Act
        var colors = LabelColorMath.PillColors(BugLabel, LightCanvas, darkBase: false);

        //Assert - the same red, taken down to the light-page lightness ceiling
        colors.Text.Should().Be(0xFFB22433u);
    }

    [Fact]
    public void pill_text_lightens_a_label_on_a_dark_page()
    {
        //Act
        var colors = LabelColorMath.PillColors(BugLabel, DarkCanvas, darkBase: true);

        //Assert - the same red, taken up to the dark-page lightness floor
        colors.Text.Should().Be(0xFFE78892u);
    }

    [Fact]
    public void pill_text_leaves_a_label_that_is_already_dark_enough_alone()
    {
        //Arrange - GitHub's "enhancement" green sits below the light-page ceiling already
        const uint green = 0xFF0E8A16u;

        //Act
        var colors = LabelColorMath.PillColors(green, LightCanvas, darkBase: false);

        //Assert
        colors.Text.Should().Be(green);
    }

    [Fact]
    public void pill_text_keeps_a_grey_label_grey()
    {
        //Arrange - no hue and no saturation to preserve, only lightness moves
        const uint grey = 0xFF808080u;

        //Act
        var onLight = LabelColorMath.PillColors(grey, LightCanvas, darkBase: false);
        var onDark = LabelColorMath.PillColors(grey, DarkCanvas, darkBase: true);

        //Assert
        onLight.Text.Should().Be(0xFF6B6B6Bu);
        onDark.Text.Should().Be(0xFFB8B8B8u);
    }

    [Fact]
    public void a_label_the_colour_of_the_page_still_gets_a_visible_border()
    {
        //Arrange - black on the dark page and white on the light page are the two worst cases:
        //blending either one over its own ground lands within a percent or two of the ground.
        var blackOnDark = LabelColorMath.PillColors(0xFF000000u, DarkCanvas, darkBase: true);
        var whiteOnLight = LabelColorMath.PillColors(0xFFFFFFFFu, LightCanvas, darkBase: false);

        //Act
        var darkSeparation = Lightness(blackOnDark.Border) - Lightness(DarkCanvas);
        var lightSeparation = Lightness(LightCanvas) - Lightness(whiteOnLight.Border);

        //Assert - the border stands off the ground rather than sinking into it
        darkSeparation.Should().BeApproximately(LabelColorMath.MinimumBorderSeparation, 0.01d);
        lightSeparation.Should().BeApproximately(LabelColorMath.MinimumBorderSeparation, 0.01d);
    }

    [Fact]
    public void a_label_that_already_stands_out_keeps_the_border_the_blend_gave_it()
    {
        //Act
        var light = LabelColorMath.PillColors(BugLabel, LightCanvas, darkBase: false);
        var dark = LabelColorMath.PillColors(BugLabel, DarkCanvas, darkBase: true);

        //Assert - the separation rule only steps in when the blend is too close to the ground
        light.Border.Should().Be(0xFFEDA6AEu);
        dark.Border.Should().Be(0xFF68232Eu);
    }

    [Fact]
    public void pill_colours_are_always_fully_opaque()
    {
        //Act
        var light = LabelColorMath.PillColors(BugLabel, LightCanvas, darkBase: false);
        var dark = LabelColorMath.PillColors(0xFF000000u, DarkCanvas, darkBase: true);
        var white = LabelColorMath.PillColors(0xFFFFFFFFu, LightCanvas, darkBase: false);

        //Assert
        (light.Background >> 24).Should().Be(0xFFu);
        (light.Border >> 24).Should().Be(0xFFu);
        (light.Text >> 24).Should().Be(0xFFu);
        (dark.Background >> 24).Should().Be(0xFFu);
        (dark.Border >> 24).Should().Be(0xFFu);
        (dark.Text >> 24).Should().Be(0xFFu);
        (white.Text >> 24).Should().Be(0xFFu);
    }

    //The same lightness the helper works in: the midpoint of the brightest and dimmest channel.
    private static double Lightness(uint argb)
    {
        var red = ((argb >> 16) & 0xFFu) / 255d;
        var green = ((argb >> 8) & 0xFFu) / 255d;
        var blue = (argb & 0xFFu) / 255d;
        return (Math.Max(red, Math.Max(green, blue))
            + Math.Min(red, Math.Min(green, blue))) / 2d;
    }
}
