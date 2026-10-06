using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace CodeBrixVideoTool.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_layout_keeps_operation_panel_usable()
    {
        Fixture.Application.Orientation.Should().Be(ScreenOrientation.Portrait);
        await OpenClipAsync(Fixture.PlainClip);
        await ChooseAsync("Destination", "WebM .webm (AV1 + Opus)");
        await ChooseAsync("Quality", "Better");
        foreach (var id in new[] { "Destination", "Resolution", "Quality", "ActionButton", "StatusText" })
        {
            var box = await Page.GetByTestId(id).BoundingBoxAsync();
            (box.X + box.Width).Should().BeLessThanOrEqualTo(Fixture.Application.Width);
            (box.Y + box.Height).Should().BeLessThanOrEqualTo(Fixture.Application.Height);
        }
        await RunToAsync("plain.webm");
        await Expect(Page.GetByTestId("StatusText")).ToHaveTextAsync(new Regex("^Wrote "));
        await Expect(Row("plain.webm")).ToBeVisibleAsync();
        await SnapshotAsync("CodeBrixVideoTool-portrait");
    }
}
