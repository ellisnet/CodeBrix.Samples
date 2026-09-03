using CodeBrixVideoTool.Processing.Resolution;
using SilverAssertions;
using System.Linq;
using Xunit;

namespace CodeBrixVideoTool.Processing.Tests;

public class ResolutionLadderTests
{
    [Fact]
    public void the_first_rung_is_always_the_source_size()
    {
        //Act
        var rungs = ResolutionLadder.Build(1920, 1080);

        //Assert
        rungs[0].IsOriginal.Should().BeTrue();
        rungs[0].Width.Should().Be(1920);
        rungs[0].Height.Should().Be(1080);
    }

    [Fact]
    public void only_heights_strictly_below_the_source_are_offered()
    {
        //Act
        var rungs = ResolutionLadder.Build(1920, 1080);

        //Assert
        rungs.Skip(1).Select(r => r.Height).Should().BeEquivalentTo(new[] { 720, 480 });
    }

    [Fact]
    public void a_source_shorter_than_every_rung_is_offered_only_its_own_size()
    {
        //Act
        var rungs = ResolutionLadder.Build(640, 480);

        //Assert
        rungs.Should().HaveCount(1);
        rungs[0].IsOriginal.Should().BeTrue();
    }

    [Fact]
    public void a_four_k_source_is_offered_every_rung()
    {
        //Act
        var rungs = ResolutionLadder.Build(3840, 2160);

        //Assert
        rungs.Skip(1).Select(r => r.Height).Should().BeEquivalentTo(new[] { 1440, 1080, 720, 480 });
    }

    [Fact]
    public void widths_are_scaled_proportionally()
    {
        //Act
        var rungs = ResolutionLadder.Build(3840, 2160);

        //Assert
        rungs.Single(r => r.Height == 1080).Width.Should().Be(1920);
        rungs.Single(r => r.Height == 720).Width.Should().Be(1280);
    }

    [Fact]
    public void every_dimension_on_every_rung_is_even()
    {
        //Arrange
        var awkward = ResolutionLadder.Build(1919, 1081);

        //Act
        var oddCount = awkward.Count(r => r.Width % 2 != 0 || r.Height % 2 != 0);

        //Assert
        oddCount.Should().Be(0);
    }

    [Fact]
    public void a_portrait_source_keeps_its_shape()
    {
        //Arrange
        //A rung names the SHORT side, so a portrait source is measured across its width: the rungs
        //below are the ones strictly below 720, and each stays taller than it is wide.
        var rungs = ResolutionLadder.Build(720, 1280);

        //Act
        var shapes = rungs.Skip(1).Select(r => (r.Width, r.Height)).ToList();

        //Assert
        shapes.Should().HaveCount(1);
        shapes[0].Width.Should().Be(480);
        shapes[0].Height.Should().Be(854);
    }

    [Fact]
    public void a_portrait_source_is_keyed_on_its_short_side()
    {
        //Act
        var rungs = ResolutionLadder.Build(1080, 1920);

        //Assert
        rungs.Should().HaveCount(3);
        rungs[0].Label.Should().Be("Original (1080 x 1920)");
        rungs[1].Label.Should().Be("720p (720 x 1280)");
        rungs[2].Label.Should().Be("480p (480 x 854)");
    }

    [Fact]
    public void a_square_source_is_keyed_on_the_one_side_it_has()
    {
        //Act
        var rungs = ResolutionLadder.Build(1080, 1080);

        //Assert
        rungs.Should().HaveCount(3);
        rungs[0].Label.Should().Be("Original (1080 x 1080)");
        rungs[1].Label.Should().Be("720p (720 x 720)");
        rungs[2].Label.Should().Be("480p (480 x 480)");
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 2)]
    [InlineData(3, 4)]
    [InlineData(1919, 1920)]
    [InlineData(1920, 1920)]
    public void dimensions_round_up_to_even(int value, int expected)
    {
        //Act
        var even = ResolutionLadder.MakeEven(value);

        //Assert
        even.Should().Be(expected);
    }

    [Fact]
    public void the_long_side_follows_the_short_side_proportionally()
    {
        //Act
        var landscape = ResolutionLadder.ProportionalOtherSide(2160, 3840, 1080);
        var portrait = ResolutionLadder.ProportionalOtherSide(1080, 1920, 480);

        //Assert
        landscape.Should().Be(1920);
        portrait.Should().Be(854);
    }

    [Fact]
    public void the_labels_name_the_rung_and_its_size()
    {
        //Act
        var rungs = ResolutionLadder.Build(3840, 2160);

        //Assert
        rungs[0].Label.Should().Be("Original (3840 x 2160)");
        rungs.Single(r => r.Height == 1080).Label.Should().Be("1080p (1920 x 1080)");
    }
}
