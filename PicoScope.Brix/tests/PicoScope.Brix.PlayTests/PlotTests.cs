using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PicoScope.Brix.PlayTests;

public sealed partial class ApplicationTests
{
    private Locator Plotter => Page.GetByTestId("Plotter");
    private Task<double[]> TimeAxisAsync() => Page.EvaluateAsync(() =>
    {
        lock (Fixture.Model.Plot.Model.SyncRoot)
            return new[] { Fixture.Model.Plot.Model.Axes[0].ActualMinimum, Fixture.Model.Plot.Model.Axes[0].ActualMaximum };
    });
    private async Task<SKBitmap> PlotPixelsAsync()
    {
        var bounds = await Plotter.BoundingBoxAsync();
        using var screen = SKBitmap.Decode(await Page.ScreenshotAsync());
        using var plot = new SKBitmap();
        screen.ExtractSubset(plot, SKRectI.Create((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height)).Should().BeTrue();
        return plot.Copy();
    }
    // Channel A draws in yellow (230, 200, 40) and channel B in blue (70, 150, 235) on a near-black chart.
    private async Task<(int Yellow, int Blue)> TraceColorsAsync()
    {
        using var plot = await PlotPixelsAsync();
        int yellow = 0, blue = 0;
        for (var y = 0; y < plot.Height; y++)
            for (var x = 0; x < plot.Width; x++)
            {
                var pixel = plot.GetPixel(x, y);
                if (pixel.Red > 190 && pixel.Green > 160 && pixel.Blue < 100) yellow++;
                else if (pixel.Red < 120 && pixel.Green > 110 && pixel.Green < 190 && pixel.Blue > 190) blue++;
            }
        return (yellow, blue);
    }
    // Re-captures until the chart's next render shows the expected traces (each capture renders a frame).
    private async Task<(int Yellow, int Blue)> WaitForTraceColorsAsync(Func<(int Yellow, int Blue), bool> ready)
    {
        var elapsed = Stopwatch.StartNew();
        (int Yellow, int Blue) colors;
        do colors = await TraceColorsAsync();
        while (!ready(colors) && elapsed.Elapsed < TimeSpan.FromSeconds(10));
        return colors;
    }

    [Fact]
    public async Task Home_key_resets_zoom_after_wheel()
    {
        await StopAndCaptureAsync();
        var initial = await TimeAxisAsync();
        await Plotter.HoverAsync();
        await Page.Mouse.WheelAsync(0, -240);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Plot.Model.Axes[0].ActualMaximum - Fixture.Model.Plot.Model.Axes[0].ActualMinimum,
            span => Math.Abs(span - (initial[1] - initial[0])) > 1e-9, description: "a zoomed time axis");
        await Plotter.PressAsync("Home");
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Plot.Model.Axes[0].ActualMaximum - Fixture.Model.Plot.Model.Axes[0].ActualMinimum,
            span => Math.Abs(span - (initial[1] - initial[0])) < 1e-9, description: "the reset time axis");
        (await TimeAxisAsync()).Should().Equal(initial);
    }

    [Fact]
    public async Task Capture_trace_pixels_follow_channel_visibility()
    {
        await StopAndCaptureAsync();
        var both = await WaitForTraceColorsAsync(colors => colors.Yellow > 200 && colors.Blue > 200);
        both.Yellow.Should().BeGreaterThan(200);
        both.Blue.Should().BeGreaterThan(200);
        await Page.GetByRole(AriaRole.Checkbox, new() { Name = "A", Exact = true }).UncheckAsync();
        var onlyB = await WaitForTraceColorsAsync(colors => colors.Yellow == 0);
        onlyB.Yellow.Should().Be(0);
        onlyB.Blue.Should().BeGreaterThan(200);
        await Page.GetByRole(AriaRole.Checkbox, new() { Name = "A", Exact = true }).CheckAsync();
        (await WaitForTraceColorsAsync(colors => colors.Yellow > 200)).Yellow.Should().BeGreaterThan(200);
        await SnapshotAsync("PicoScope.Brix-block");
    }

    [Fact]
    public async Task Captured_block_screenshot_is_stable()
    {
        // The simulator is noise-free and every block starts at t=0, so a second capture redraws identically.
        await StopAndCaptureAsync();
        await WaitForTraceColorsAsync(colors => colors.Yellow > 200 && colors.Blue > 200);
        using var first = await PlotPixelsAsync();
        await Button("Flash LED").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Flashing the device LED.");
        await Button("Single capture").ClickAsync();
        await Expect(Status).ToHaveTextAsync(new Regex("^Captured 2000 samples"));
        using var second = await PlotPixelsAsync();
        second.Bytes.Should().Equal(first.Bytes);
    }
}
