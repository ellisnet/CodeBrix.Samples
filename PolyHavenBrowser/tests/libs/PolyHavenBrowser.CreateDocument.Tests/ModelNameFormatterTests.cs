using PolyHavenBrowser.CreateDocument.Internal;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.CreateDocument.Tests;

public class ModelNameFormatterTests
{
    [Theory]
    [InlineData("Marble Bust 1", "Marble Bust")]
    [InlineData("Camera 01", "Camera")]
    [InlineData("Chess Set 02", "Chess Set")]
    [InlineData("Wooden Table", "Wooden Table")]
    [InlineData("Model 01 2", "Model")]
    public void strips_trailing_number_tokens(string name, string expected)
    {
        //Act
        var stripped = ModelNameFormatter.StripTrailingNumbers(name);

        //Assert
        stripped.Should().Be(expected);
    }

    [Fact]
    public void a_purely_numeric_name_is_kept_rather_than_stripped_to_nothing()
    {
        //Act
        var stripped = ModelNameFormatter.StripTrailingNumbers("01");

        //Assert
        stripped.Should().Be("01");
    }

    [Fact]
    public void numbers_inside_the_name_are_untouched()
    {
        //Act
        var stripped = ModelNameFormatter.StripTrailingNumbers("Type 59 Tank");

        //Assert
        stripped.Should().Be("Type 59 Tank");
    }

    [Fact]
    public void surrounding_whitespace_is_trimmed()
    {
        //Act
        var stripped = ModelNameFormatter.StripTrailingNumbers("  Marble Bust 1  ");

        //Assert
        stripped.Should().Be("Marble Bust");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void blank_names_become_empty(string? name)
    {
        //Act
        var stripped = ModelNameFormatter.StripTrailingNumbers(name);

        //Assert
        stripped.Should().Be(string.Empty);
    }

    [Fact]
    public void mixed_alphanumeric_final_tokens_are_kept()
    {
        //Act - "01b" is not an all-digit token, so nothing strips
        var stripped = ModelNameFormatter.StripTrailingNumbers("Rock 01b");

        //Assert
        stripped.Should().Be("Rock 01b");
    }
}
