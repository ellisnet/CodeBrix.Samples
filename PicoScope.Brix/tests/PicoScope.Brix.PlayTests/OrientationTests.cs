using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_layout_keeps_controls_and_plot_on_screen()
    {
        Fixture.Application.Orientation.Should().Be(ScreenOrientation.Portrait);
        var width = Fixture.Application.Width;
        foreach (var control in new[]
        {
            Button("Single capture"), Button("Start streaming"), Button("Stop"), Page.GetByTestId("SelectedRangeOption"),
            Page.GetByRole(AriaRole.Checkbox, new() { Name = "B", Exact = true }), Page.GetByTestId("GeneratorFrequencyText"),
            Button("Toggle"), Button("Flash LED"), Page.GetByTestId("StatusText"), Page.GetByTestId("SimulatedBadge"),
        })
        {
            await Expect(control).ToBeVisibleAsync();
            var box = await control.BoundingBoxAsync();
            (box.X + box.Width).Should().BeLessThanOrEqualTo(width);
        }
        var plot = await Page.GetByTestId("Plotter").BoundingBoxAsync();
        (plot.X + plot.Width).Should().BeLessThanOrEqualTo(width);
        plot.Height.Should().BeGreaterThan(plot.Width, "the chart takes the spare height of a portrait screen");
        (await Page.GetByTestId("StatusText").BoundingBoxAsync()).Y.Should().BeGreaterThan(plot.Y + plot.Height);
        await Button("Stop").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Stopped.");
        await SnapshotAsync("PicoScope.Brix-portrait");
    }
}
