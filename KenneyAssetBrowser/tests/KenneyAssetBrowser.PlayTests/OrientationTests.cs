using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using SkiaSharp.Views.Windows;
using Xunit;

namespace KenneyAssetBrowser.PlayTests;

public sealed partial class ApplicationTests
{
    [Theory]
    [InlineData(ScreenOrientation.Landscape, Traits = new[] { "PlayTestOrientation", "Landscape" })]
    [InlineData(ScreenOrientation.Portrait, Traits = new[] { "PlayTestOrientation", "Portrait" })]
    public async Task Viewer_stacks_facts_above_preview_in_portrait(ScreenOrientation orientation)
    {
        Fixture.Application.Orientation.Should().Be(orientation);
        await OpenCatalogAsync();
        await OpenCellAsync("blue_tile");
        await Expect(Page.GetByText("96 × 96 px", new() { Exact = true })).ToBeVisibleAsync();
        var facts = await Page.GetByText("DETAILS", new() { Exact = true }).BoundingBoxAsync();
        var preview = await Page.GetByType<SKXamlCanvas>().BoundingBoxAsync();
        if (orientation == ScreenOrientation.Portrait)
        {
            preview.Y.Should().BeGreaterThan(facts.Y + facts.Height);
            preview.Width.Should().BeGreaterThan(Fixture.Application.Width / 2f);
        }
        else
        {
            preview.X.Should().BeGreaterThan(facts.X + facts.Width);
            preview.Y.Should().BeLessThan(facts.Y + facts.Height);
        }
        await SnapshotAsync("KenneyAssetBrowser-viewer-" + orientation.ToString().ToLowerInvariant());
    }
}
