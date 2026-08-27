using PdfSideBySide.PdfRender.Documents;
using PdfSideBySide.PdfRender.Rendering;
using PdfSideBySide.PdfRender.Tests.Helpers;
using SilverAssertions;
using Xunit;
using System;
using System.Threading.Tasks;

namespace PdfSideBySide.PdfRender.Tests;

public class PageRendererTests
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    //Inanna.pdf pages are 576 x 720 points (8 x 10 inches)
    private const double InannaWidthInches = 8.0;
    private const double InannaHeightInches = 10.0;

    private static Task<PdfPageDocument> OpenInanna() =>
        PdfPageDocument.OpenAsync(TestPdfs.InannaPath, TestContext.Current.CancellationToken);

    [Fact]
    public void a_new_renderer_uses_the_default_dpi_and_capacity()
    {
        //Act
        using var renderer = new PageRenderer();

        //Assert
        renderer.Dpi.Should().Be(PageRenderer.DefaultDpi);
        renderer.CacheCapacity.Should().Be(PageRenderer.DefaultCacheCapacity);
        renderer.CachedPageCount.Should().Be(0);
    }

    [Fact]
    public void setting_dpi_below_one_restores_the_default()
    {
        //Arrange
        using var renderer = new PageRenderer { Dpi = 96 };

        //Act
        renderer.Dpi = 0;

        //Assert
        renderer.Dpi.Should().Be(PageRenderer.DefaultDpi);
    }

    [Fact]
    public async Task render_page_returns_a_png_of_the_page_at_the_requested_dpi()
    {
        //Arrange
        using var renderer = new PageRenderer { Dpi = 72 };
        var document = await OpenInanna();

        //Act
        var rendered = await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);

        //Assert
        rendered.FilePath.Should().Be(document.FilePath);
        rendered.PageNumber.Should().Be(1);
        rendered.PixelWidth.Should().Be((int)(InannaWidthInches * 72));
        rendered.PixelHeight.Should().Be((int)(InannaHeightInches * 72));
        rendered.PngBytes.Should().NotBeEmpty();
        rendered.PngBytes[..PngSignature.Length].Should().Equal(PngSignature);
    }

    [Fact]
    public async Task render_page_scales_with_dpi()
    {
        //Arrange
        using var renderer = new PageRenderer { Dpi = 150 };
        var document = await OpenInanna();

        //Act
        var rendered = await renderer.RenderPageAsync(document, 3, TestContext.Current.CancellationToken);

        //Assert
        rendered.PixelWidth.Should().Be((int)(InannaWidthInches * 150));
        rendered.PixelHeight.Should().Be((int)(InannaHeightInches * 150));
    }

    [Fact]
    public async Task render_page_accepts_a_dpi_for_one_call_without_changing_the_default()
    {
        //Arrange
        using var renderer = new PageRenderer { Dpi = 72 };
        var document = await OpenInanna();

        //Act
        var rendered = await renderer.RenderPageAsync(document, 1, 144, TestContext.Current.CancellationToken);

        //Assert
        rendered.PixelWidth.Should().Be((int)(InannaWidthInches * 144));
        renderer.Dpi.Should().Be(72);
    }

    [Fact]
    public async Task the_same_page_at_two_resolutions_is_cached_twice()
    {
        //Arrange
        using var renderer = new PageRenderer { Dpi = 72 };
        var document = await OpenInanna();

        //Act
        var low = await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);
        var high = await renderer.RenderPageAsync(document, 1, 144, TestContext.Current.CancellationToken);
        var lowAgain = await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);

        //Assert
        renderer.CachedPageCount.Should().Be(2);
        lowAgain.Should().BeSameAs(low);
        high.PixelWidth.Should().Be(low.PixelWidth * 2);
    }

    [Fact]
    public async Task render_page_rejects_a_dpi_below_one()
    {
        //Arrange
        using var renderer = new PageRenderer();
        var document = await OpenInanna();

        //Act
        Func<Task> act = () => renderer.RenderPageAsync(document, 1, 0, TestContext.Current.CancellationToken);

        //Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task render_current_page_at_a_dpi_follows_the_cursor()
    {
        //Arrange
        using var renderer = new PageRenderer { Dpi = 72 };
        var document = await OpenInanna();
        document.GoToPage(7);

        //Act
        var rendered = await renderer.RenderCurrentPageAsync(document, 96, TestContext.Current.CancellationToken);

        //Assert
        rendered.PageNumber.Should().Be(7);
        rendered.PixelWidth.Should().Be((int)(InannaWidthInches * 96));
    }

    [Fact]
    public async Task render_current_page_follows_the_documents_cursor()
    {
        //Arrange
        using var renderer = new PageRenderer { Dpi = 72 };
        var document = await OpenInanna();
        document.GoToPage(5);

        //Act
        var rendered = await renderer.RenderCurrentPageAsync(document, TestContext.Current.CancellationToken);

        //Assert
        rendered.PageNumber.Should().Be(5);
    }

    [Fact]
    public async Task different_pages_render_to_different_images()
    {
        //Arrange
        using var renderer = new PageRenderer { Dpi = 72 };
        var document = await OpenInanna();

        //Act
        var first = await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);
        var second = await renderer.RenderPageAsync(document, 2, TestContext.Current.CancellationToken);

        //Assert
        first.PngBytes.Should().NotEqual(second.PngBytes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(TestPdfs.InannaPageCount + 1)]
    public async Task render_page_rejects_pages_outside_the_document(int pageNumber)
    {
        //Arrange
        using var renderer = new PageRenderer();
        var document = await OpenInanna();

        //Act
        Func<Task> act = () => renderer.RenderPageAsync(document, pageNumber, TestContext.Current.CancellationToken);

        //Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task rendering_a_page_again_comes_from_the_cache()
    {
        //Arrange
        using var renderer = new PageRenderer { Dpi = 72 };
        var document = await OpenInanna();

        //Act
        var first = await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);
        var again = await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);

        //Assert
        again.Should().BeSameAs(first);
        renderer.CachedPageCount.Should().Be(1);
    }

    [Fact]
    public async Task the_cache_evicts_the_least_recently_used_page_past_its_capacity()
    {
        //Arrange
        using var renderer = new PageRenderer(cacheCapacity: 2) { Dpi = 72 };
        var document = await OpenInanna();
        var pageOne = await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);
        var pageTwo = await renderer.RenderPageAsync(document, 2, TestContext.Current.CancellationToken);

        //Act - touch page 1 so page 2 becomes the least recently used, then add a third
        await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);
        await renderer.RenderPageAsync(document, 3, TestContext.Current.CancellationToken);
        var pageOneAgain = await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);
        var pageTwoAgain = await renderer.RenderPageAsync(document, 2, TestContext.Current.CancellationToken);

        //Assert
        renderer.CachedPageCount.Should().Be(2);
        pageOneAgain.Should().BeSameAs(pageOne);    //Survived
        pageTwoAgain.Should().NotBeSameAs(pageTwo); //Evicted and re-rendered
    }

    [Fact]
    public async Task a_capacity_of_zero_disables_the_cache()
    {
        //Arrange
        using var renderer = new PageRenderer(cacheCapacity: 0) { Dpi = 72 };
        var document = await OpenInanna();

        //Act
        var first = await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);
        var again = await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);

        //Assert
        renderer.CachedPageCount.Should().Be(0);
        again.Should().NotBeSameAs(first);
        again.PngBytes.Should().Equal(first.PngBytes);
    }

    [Fact]
    public async Task changing_dpi_clears_the_cache()
    {
        //Arrange
        using var renderer = new PageRenderer { Dpi = 72 };
        var document = await OpenInanna();
        await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);

        //Act
        renderer.Dpi = 96;

        //Assert
        renderer.CachedPageCount.Should().Be(0);
    }

    [Fact]
    public async Task clear_cache_forgets_every_page()
    {
        //Arrange
        using var renderer = new PageRenderer { Dpi = 72 };
        var document = await OpenInanna();
        await renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);
        await renderer.RenderPageAsync(document, 2, TestContext.Current.CancellationToken);

        //Act
        renderer.ClearCache();

        //Assert
        renderer.CachedPageCount.Should().Be(0);
    }

    [Fact]
    public async Task a_disposed_renderer_refuses_to_render()
    {
        //Arrange
        var renderer = new PageRenderer();
        var document = await OpenInanna();
        renderer.Dispose();

        //Act
        Func<Task> act = () => renderer.RenderPageAsync(document, 1, TestContext.Current.CancellationToken);

        //Assert
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }
}
