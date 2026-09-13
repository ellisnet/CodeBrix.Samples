using System;
using System.Linq;
using SilverAssertions;
using WikipediaPublisher.RenderArticle.Models;
using Xunit;

namespace WikipediaPublisher.RenderArticle.Tests.Models;

public class PageSizeInfoTests
{
    [Fact]
    public void All_starts_with_the_default_trim_size()
    {
        //Arrange - the picker binds this list and selects its first entry

        //Act
        var first = PageSizeInfo.All[0];

        //Assert
        first.Option.Should().Be(PageSizeOption.EightByTen);
    }

    [Fact]
    public void All_covers_every_page_size_option_once()
    {
        //Arrange
        var options = Enum.GetValues<PageSizeOption>();

        //Act
        var offered = PageSizeInfo.All.Select(info => info.Option).ToList();

        //Assert
        offered.Should().Equal(options);
    }

    [Fact]
    public void All_display_names_are_distinct_and_not_blank()
    {
        //Arrange - the picker shows DisplayName, so two identical names would be unreadable

        //Act
        var names = PageSizeInfo.All.Select(info => info.DisplayName).ToList();

        //Assert
        names.Should().OnlyHaveUniqueItems();
        names.Should().AllSatisfy(name => name.Should().NotBeNullOrWhiteSpace());
    }

    [Theory]
    [InlineData(PageSizeOption.EightByTen)]
    [InlineData(PageSizeOption.SixByNine)]
    [InlineData(PageSizeOption.Letter)]
    [InlineData(PageSizeOption.A4)]
    public void For_returns_the_matching_info_with_positive_dimensions(PageSizeOption option)
    {
        //Arrange

        //Act
        var info = PageSizeInfo.For(option);

        //Assert
        info.Option.Should().Be(option);
        info.WidthPoints.Should().BeGreaterThan(0);
        info.HeightPoints.Should().BeGreaterThan(0);
    }

    [Fact]
    public void For_rejects_an_option_that_is_not_in_the_list()
    {
        //Arrange
        var unknown = (PageSizeOption)999;

        //Act
        var act = () => PageSizeInfo.For(unknown);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
