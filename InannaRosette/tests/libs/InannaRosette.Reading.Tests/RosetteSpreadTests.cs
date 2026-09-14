// RosetteSpreadTests.cs - the nine stations of the spread: the Heart at the centre, the eight
// petals clockwise from the top, and the four axes that face each other across the flower.

using System.Linq;
using InannaRosette.Reading.Data;
using SilverAssertions;
using Xunit;

namespace InannaRosette.Reading.Tests;

public class RosetteSpreadTests
{
    private static readonly string[] PetalTitlesClockwise =
    [
        "Heaven",
        "The Morning Star",
        "The Storehouse",
        "The Descent",
        "The Great Below",
        "The Return",
        "The Evening Star",
        "The Gift",
    ];

    [Fact]
    public void the_spread_has_nine_stations()
    {
        //Assert
        RosetteSpread.Positions.Should().HaveCount(9);
    }

    [Fact]
    public void the_first_station_is_the_heart_at_the_centre()
    {
        //Act
        var heart = RosetteSpread.Positions[0];

        //Assert
        heart.Index.Should().Be(0);
        heart.Title.Should().Be("The Heart");
        heart.IsCenter.Should().BeTrue();
        RosetteSpread.Center.Should().BeSameAs(heart);
    }

    [Fact]
    public void the_station_indexes_run_zero_to_eight_and_are_unique()
    {
        //Act
        var indexes = RosetteSpread.Positions.Select(p => p.Index).ToList();

        //Assert
        indexes.Should().Equal([0, 1, 2, 3, 4, 5, 6, 7, 8]);
        indexes.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void the_eight_petals_carry_the_documented_names_in_clockwise_order()
    {
        //Act
        var titles = RosetteSpread.Petals.Select(p => p.Title).ToList();

        //Assert
        titles.Should().Equal(PetalTitlesClockwise);
    }

    [Fact]
    public void Petals_are_the_eight_stations_that_are_not_the_centre()
    {
        //Act
        var petals = RosetteSpread.Petals;

        //Assert
        petals.Should().HaveCount(8);
        petals.Should().AllSatisfy(p => p.IsCenter.Should().BeFalse());
        petals.Select(p => p.Index).Should().Equal([1, 2, 3, 4, 5, 6, 7, 8]);
    }

    [Fact]
    public void the_petals_stand_forty_five_degrees_apart_starting_at_the_top()
    {
        //Act & Assert
        foreach (var petal in RosetteSpread.Petals)
        {
            petal.AngleDegrees.Should().Be((petal.Index - 1) * 45.0);
        }
    }

    [Fact]
    public void only_the_heart_reports_itself_as_the_centre()
    {
        //Assert
        RosetteSpread.Positions.Count(p => p.IsCenter).Should().Be(1);
    }

    [Fact]
    public void every_station_carries_its_text()
    {
        //Act & Assert
        foreach (var position in RosetteSpread.Positions)
        {
            position.Title.Should().NotBeNullOrWhiteSpace();
            position.Subtitle.Should().NotBeNullOrWhiteSpace();
            position.Question.Should().NotBeNullOrWhiteSpace();
            position.Description.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void the_station_titles_are_unique()
    {
        //Assert
        RosetteSpread.Positions.Select(p => p.Title).Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 6)]
    [InlineData(3, 7)]
    [InlineData(4, 8)]
    [InlineData(5, 1)]
    [InlineData(6, 2)]
    [InlineData(7, 3)]
    [InlineData(8, 4)]
    public void each_petal_faces_the_petal_across_the_flower(int index, int expectedOpposite)
    {
        //Act
        var position = RosetteSpread.At(index);

        //Assert
        position.Should().NotBeNull();
        position.OppositeIndex.Should().Be(expectedOpposite);
        RosetteSpread.Opposite(position).Index.Should().Be(expectedOpposite);
    }

    [Fact]
    public void the_heart_faces_nothing()
    {
        //Assert
        RosetteSpread.Center.OppositeIndex.Should().BeNull();
        RosetteSpread.Opposite(RosetteSpread.Center).Should().BeNull();
    }

    [Fact]
    public void At_returns_the_station_at_each_index()
    {
        //Act & Assert
        for (var i = 0; i < 9; i++)
        {
            RosetteSpread.At(i).Should().BeSameAs(RosetteSpread.Positions[i]);
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(9)]
    [InlineData(int.MaxValue)]
    public void At_returns_null_outside_the_spread(int index)
    {
        //Assert
        RosetteSpread.At(index).Should().BeNull();
    }

    [Fact]
    public void the_four_axes_are_the_four_opposite_pairs()
    {
        //Act
        var axes = RosetteSpread.Axes;

        //Assert
        axes.Should().HaveCount(4);
        axes.Select(a => (a.A.Index, a.B.Index)).Should().Equal([(1, 5), (2, 6), (3, 7), (4, 8)]);
    }

    [Fact]
    public void every_axis_joins_two_petals_that_face_one_another()
    {
        //Act & Assert
        foreach (var (a, b) in RosetteSpread.Axes)
        {
            a.OppositeIndex.Should().Be(b.Index);
            b.OppositeIndex.Should().Be(a.Index);
        }
    }
}
