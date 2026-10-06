using System.IO;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace WikipediaPublisher.PlayTests;

public sealed partial class ApplicationTests
{
    [Theory]
    [InlineData(ScreenOrientation.Landscape, Traits = new[] { "PlayTestOrientation", "Landscape" })]
    [InlineData(ScreenOrientation.Portrait, Traits = new[] { "PlayTestOrientation", "Portrait" })]
    public async Task Long_output_path_keeps_select_button_on_screen(ScreenOrientation orientation)
    {
        Fixture.Application.Orientation.Should().Be(orientation);
        var path = Path.Combine(Fixture.DataDirectory, new string('a', 160), new string('b', 160), "A very long article title.pdf");
        await Page.GetByTestId("OutputFilePath").FillAsync(path);
        var box = await Page.GetByTestId("OutputFilePath").BoundingBoxAsync();
        var select = await Button("Select…").BoundingBoxAsync();
        // The page has a 16px margin; the path box gives way so Select stays inside it.
        (select.X + select.Width).Should().BeLessThanOrEqualTo(Fixture.Application.Width - 16);
        (box.X + box.Width).Should().BeLessThanOrEqualTo(select.X);
        Fixture.Application.FilePickers.EnqueueSaveFile(null);
        await Button("Select…").ClickAsync();
        Fixture.Application.FilePickers.SaveFileRequestCount.Should().Be(1);
        await SnapshotAsync("WikipediaPublisher-long-path-" + orientation);
    }
}
