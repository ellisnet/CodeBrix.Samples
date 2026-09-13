using Microsoft.Extensions.DependencyInjection;
using PdfSideBySide.PdfRender.Rendering;
using SilverAssertions;
using System;
using Xunit;

namespace PdfSideBySide.PdfRender.Tests;

public class RegisterServicesTests
{
    [Fact]
    public void AddPdfRender_registers_a_comparison_factory_and_a_page_renderer()
    {
        //Arrange
        var services = new ServiceCollection();

        //Act
        services.AddPdfRender();
        using var provider = services.BuildServiceProvider();

        //Assert
        provider.GetService<IPdfComparisonFactory>().Should().NotBeNull();
        provider.GetService<IPageRenderer>().Should().NotBeNull();
    }

    [Fact]
    public void AddPdfRender_makes_a_comparison_with_both_sides_empty()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddPdfRender();
        using var provider = services.BuildServiceProvider();

        //Act
        var comparison = provider.GetService<IPdfComparisonFactory>().Create();

        //Assert
        comparison.Left.Should().BeNull();
        comparison.Right.Should().BeNull();
        comparison.IsReady.Should().BeFalse();
        comparison.View.Zoom.Percent.Should().Be(100);
    }

    [Fact]
    public void AddPdfRender_gives_every_holder_its_own_renderer()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddPdfRender();
        using var provider = services.BuildServiceProvider();

        //Act
        var first = provider.GetService<IPageRenderer>();
        var second = provider.GetService<IPageRenderer>();

        //Assert - each one owns a rasterizer and a cache that its holder disposes
        first.Should().NotBeSameAs(second);
        first.Dpi.Should().Be(PageRenderer.DefaultDpi);
        first.CacheCapacity.Should().Be(PageRenderer.DefaultCacheCapacity);
    }

    [Fact]
    public void AddPdfRender_leaves_a_registration_the_application_made_itself()
    {
        //Arrange
        var services = new ServiceCollection();
        var ownFactory = new PdfComparisonFactory();
        services.AddSingleton<IPdfComparisonFactory>(ownFactory);

        //Act
        services.AddPdfRender();
        using var provider = services.BuildServiceProvider();

        //Assert
        provider.GetService<IPdfComparisonFactory>().Should().BeSameAs(ownFactory);
    }

    [Fact]
    public void AddPdfRender_rejects_a_missing_service_collection()
    {
        //Arrange
        IServiceCollection services = null;

        //Act
        Action act = () => services.AddPdfRender();

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
