using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Pinta.Brix.Controls;
using Pinta.Brix.Settings;
using SilverAssertions;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class LayoutTests(AppFixture fixture) : PintaTest(fixture)
{
    private Task<double> PadsWidthAsync() => EvaluateAsync(() => ((Grid)Fixture.View.FindName("PadsColumn")).ActualWidth);

    [Fact]
    public async Task Dragging_the_pads_splitter_resizes_the_pads_and_remembers_the_width()
    {
        (await PadsWidthAsync()).Should().Be(230);
        // Two splitters: the tall bar beside the pads and the short one between them.
        var splitters = Page.GetByType<ThumbSplitter>();
        var index = (await EvaluateAsync(() => AppFixture.Descendants(Fixture.View).OfType<ThumbSplitter>()
            .Select(splitter => splitter.Orientation).ToArray())).ToList().IndexOf(Orientation.Vertical);
        var box = await splitters.Nth(index).BoundingBoxAsync();
        await splitters.Nth(index).DragByAsync(-100, 0, new() { Position = new() { X = box.Width / 2, Y = box.Height / 2 } });
        await WaitAsync(() => ((Grid)Fixture.View.FindName("PadsColumn")).ActualWidth, width => width == 330, "the wider pads");
        (await EvaluateAsync(() => SettingsService.Get("pads-width", 0))).Should().Be(330);
        // The pads never get narrower than their minimum.
        box = await splitters.Nth(index).BoundingBoxAsync();
        await splitters.Nth(index).DragByAsync(300, 0, new() { Position = new() { X = box.Width / 2, Y = box.Height / 2 } });
        await WaitAsync(() => ((Grid)Fixture.View.FindName("PadsColumn")).ActualWidth, width => width == 200, "the narrowest pads");
        (await EvaluateAsync(() => SettingsService.Get("pads-width", 0))).Should().Be(200);
    }
}
