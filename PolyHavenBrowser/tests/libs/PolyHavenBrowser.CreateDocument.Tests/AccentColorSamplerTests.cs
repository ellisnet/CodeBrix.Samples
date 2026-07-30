using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats.Png;
using CodeBrix.Imaging.PixelFormats;
using PolyHavenBrowser.CreateDocument.Internal;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.CreateDocument.Tests;

public class AccentColorSamplerTests
{
    private static byte[] SolidPng(byte r, byte g, byte b, int size = 32)
    {
        using var image = new Image<Rgba32>(size, size);
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                image[x, y] = new Rgba32(r, g, b);
            }
        }

        using var stream = new MemoryStream();
        image.Save(stream, new PngEncoder());
        return stream.ToArray();
    }

    [Fact]
    public void a_red_thumbnail_yields_a_red_family_accent()
    {
        //Arrange
        var thumbnail = SolidPng(200, 40, 40);

        //Act
        var accent = AccentColorSampler.Sample(thumbnail);

        //Assert - hue survives, even though the tone is deepened for paper
        ((int)accent.R).Should().BeGreaterThan(accent.G);
        ((int)accent.R).Should().BeGreaterThan(accent.B);
    }

    [Fact]
    public void a_blue_thumbnail_yields_a_blue_family_accent()
    {
        //Arrange
        var thumbnail = SolidPng(30, 60, 190);

        //Act
        var accent = AccentColorSampler.Sample(thumbnail);

        //Assert
        ((int)accent.B).Should().BeGreaterThan(accent.R);
        ((int)accent.B).Should().BeGreaterThan(accent.G);
    }

    [Fact]
    public void a_grayscale_thumbnail_falls_back_to_the_fixed_accent()
    {
        //Arrange
        var thumbnail = SolidPng(128, 128, 128);

        //Act
        var accent = AccentColorSampler.Sample(thumbnail);

        //Assert
        accent.Should().Be(AccentColorSampler.Fallback);
    }

    [Fact]
    public void missing_or_broken_thumbnails_fall_back_instead_of_throwing()
    {
        //Act
        var missing = AccentColorSampler.Sample(null);
        var empty = AccentColorSampler.Sample([]);
        var garbage = AccentColorSampler.Sample([1, 2, 3, 4, 5]);

        //Assert
        missing.Should().Be(AccentColorSampler.Fallback);
        empty.Should().Be(AccentColorSampler.Fallback);
        garbage.Should().Be(AccentColorSampler.Fallback);
    }

    [Fact]
    public void the_accent_is_deep_enough_to_read_as_text_on_white_paper()
    {
        //Arrange - a blown-out, nearly-white yellow
        var thumbnail = SolidPng(250, 245, 130);

        //Act
        var accent = AccentColorSampler.Sample(thumbnail);

        //Assert - value is clamped down so the accent can carry kickers and rules
        var maxChannel = Math.Max(accent.R, Math.Max(accent.G, accent.B));
        ((int)maxChannel).Should().BeLessThan(170);
    }

    [Fact]
    public void sampling_is_deterministic()
    {
        //Arrange
        var thumbnail = SolidPng(90, 160, 70);

        //Act
        var first = AccentColorSampler.Sample(thumbnail);
        var second = AccentColorSampler.Sample(thumbnail);

        //Assert
        first.Should().Be(second);
    }
}
