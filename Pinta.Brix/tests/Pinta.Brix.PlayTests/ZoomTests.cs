using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using Xunit;

namespace Pinta.Brix.PlayTests;

public sealed class ZoomTests(AppFixture fixture) : PintaTest(fixture)
{
    private Locator ZoomBox => Page.GetByRole(AriaRole.Combobox, new() { Name = "Zoom", Exact = true });
    private Task<double> ScaleAsync() => EvaluateAsync(() => ActiveDocument.Workspace.Scale);

    [Fact]
    public async Task Zoom_buttons_step_through_the_presets()
    {
        (await ScaleAsync()).Should().Be(1.0);
        await Page.GetByRole(AriaRole.Button, new() { Name = "+", Exact = true }).ClickAsync();
        await WaitAsync(() => ActiveDocument.Workspace.Scale, scale => scale == 1.25, "one step in");
        var canvas = await Canvas.BoundingBoxAsync();
        canvas.Width.Should().Be(1000);
        await Page.GetByRole(AriaRole.Button, new() { Name = "−", Exact = true }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "−", Exact = true }).ClickAsync();
        await WaitAsync(() => ActiveDocument.Workspace.Scale, scale => scale == 0.66, "one step out of actual size");
        await MenuAsync("View", "Normal Size");
        await WaitAsync(() => ActiveDocument.Workspace.Scale, scale => scale == 1.0, "actual size");
    }

    [Fact]
    public async Task Zoom_preset_list_sets_the_zoom()
    {
        await ZoomBox.ClickAsync(new() { Position = new() { X = 80, Y = 12 } });
        // The list opens above the status bar, scrolled to its small end.
        var preset = Page.GetByRole(AriaRole.Option, new() { Name = "50%", Exact = true });
        await preset.ScrollIntoViewIfNeededAsync();
        await preset.ClickAsync();
        await WaitAsync(() => ActiveDocument.Workspace.Scale, scale => scale == 0.5, "50%");
        (await Canvas.BoundingBoxAsync()).Width.Should().Be(400);
    }
}
