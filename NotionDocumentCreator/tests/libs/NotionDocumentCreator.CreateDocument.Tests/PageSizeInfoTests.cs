using NotionDocumentCreator.CreateDocument.Models;
using SilverAssertions;
using System;
using Xunit;

namespace NotionDocumentCreator.CreateDocument.Tests;

public class PageSizeInfoTests
{
    [Fact]
    public void all_four_trims_are_present()
    {
        //Assert
        PageSizeInfo.All.Should().HaveCount(4);
    }

    [Fact]
    public void the_default_trim_is_eight_by_ten()
    {
        //Assert
        PageSizeInfo.All[0].Option.Should().Be(PageSizeOption.EightByTen);
        PageSizeInfo.All[0].WidthPoints.Should().Be(576);
        PageSizeInfo.All[0].HeightPoints.Should().Be(720);
    }

    [Fact]
    public void for_round_trips_every_option()
    {
        //Act + Assert
        foreach (var option in Enum.GetValues<PageSizeOption>())
        {
            PageSizeInfo.For(option).Option.Should().Be(option);
        }
    }

    [Fact]
    public void every_trim_has_a_display_name_and_positive_dimensions()
    {
        //Assert
        foreach (var info in PageSizeInfo.All)
        {
            info.DisplayName.Should().NotBeNullOrEmpty();
            (info.WidthPoints > 0).Should().Be(true);
            (info.HeightPoints > info.WidthPoints).Should().Be(true); //All four trims are portrait
        }
    }
}
