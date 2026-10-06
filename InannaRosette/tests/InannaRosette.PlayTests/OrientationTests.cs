using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace InannaRosette.PlayTests;

public sealed partial class ApplicationTests
{
    [Fact]
    [PlayTestOrientation(ScreenOrientation.Portrait)]
    public async Task Portrait_layout_keeps_header_and_rail_on_screen()
    {
        // Portrait is narrower than the window size the application asks for on a desktop.
        Fixture.Application.Width.Should().Be(1080);
        var onScreen = new List<Locator>
        {
            Page.GetByTestId("Querent"), Page.GetByTestId("Question"), Page.GetByTestId("DeckStack"),
            Page.GetByTestId("StationsCount"), Button("Shuffle"), Button("Draw"), Button("Auto-lay"), Button("Clear"),
            Button("Interpret"), Button("Create PDF"), Button("Save reading…"), Button("Open reading…"),
        };
        for (var station = 0; station < 9; station++) onScreen.Add(Page.GetByTestId("Station" + station));
        foreach (var locator in onScreen)
        {
            await Expect(locator).ToBeVisibleAsync();
            var box = await locator.BoundingBoxAsync();
            box.X.Should().BeGreaterThanOrEqualTo(0);
            box.Y.Should().BeGreaterThanOrEqualTo(0);
            (box.X + box.Width).Should().BeLessThanOrEqualTo(Fixture.Application.Width);
            (box.Y + box.Height).Should().BeLessThanOrEqualTo(Fixture.Application.Height);
        }
        await AutoLayAsync();
        await SnapshotAsync("InannaRosette-portrait");
    }
}
