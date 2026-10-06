using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeBrix.Platform.PlayTest;
using CodeBrix.Samples.PlayTests;
using PicoScope.Brix.ScopeData.Model;
using PicoScope.Brix.Views;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.PlayTests;

public sealed partial class ApplicationTests(AppFixture fixture) : SampleTest<AppFixture, MainPage>(fixture)
{
    private static readonly string[] Commands = { "Single capture", "Start streaming", "Stop", "Toggle", "Flash LED" };
    private Locator Status => Page.GetByTestId("StatusText");
    private Task<string> SeriesTitlesAsync() =>
        Page.EvaluateAsync(() => string.Join("|", Fixture.Model.Plot.Model.Series.Select(series => series.Title)));
    // Pixel and plot checks need a still chart: stop the live stream, then draw one noise-free block.
    private async Task StopAndCaptureAsync()
    {
        await Button("Stop").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Stopped.");
        await Button("Single capture").ClickAsync();
        await Expect(Status).ToHaveTextAsync(new Regex("^Captured 2000 samples"));
    }
    private async Task SelectAsync(string picker, string option)
    {
        await Page.GetByTestId(picker).ClickAsync();
        await Page.GetByRole(AriaRole.Option, new() { Name = option, Exact = true }).ClickAsync();
    }

    [Fact]
    public async Task Startup_finds_simulator_and_streams()
    {
        await Expect(Status).ToHaveTextAsync("Streaming.");
        await Expect(Page.GetByTestId("SimulatedBadge")).ToHaveTextAsync("SIMULATED");
        await Expect(Page.GetByTestId("DeviceText")).ToContainTextAsync("PicoScope 2204A (simulated) (serial SIM/0001");
        await Expect(Page.GetByText("2 channels | 9 ranges (+/-50 mV to +/-20 V) | timebase 0-23 | fast streaming | ETS 500 ps | siggen to 100 kHz",
            new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("SelectedRangeOption")).ToContainTextAsync("+/-5 V");
        await Expect(Page.GetByTestId("SelectedWaveType")).ToContainTextAsync("Sine");
        await Expect(Page.GetByTestId("GeneratorFrequencyText")).ToHaveValueAsync("5000");
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Plot.Model.Subtitle ?? "", subtitle => subtitle.StartsWith("Streaming at 10 kS/s"));
        (await SeriesTitlesAsync()).Should().Be("Channel A (+/-5 V)|Channel B (+/-5 V)");
        await SnapshotAsync("PicoScope.Brix-streaming");
    }

    [Fact]
    public async Task Streaming_disables_capture_and_start_enables_stop()
    {
        await Expect(Button("Single capture")).ToBeDisabledAsync();
        await Expect(Button("Start streaming")).ToBeDisabledAsync();
        await Expect(Button("Stop")).ToBeEnabledAsync();
        await Expect(Button("Toggle")).ToBeEnabledAsync();
        await Expect(Button("Flash LED")).ToBeEnabledAsync();
    }

    [Fact]
    public async Task Stop_then_single_capture_shows_block()
    {
        await Button("Stop").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Stopped.");
        await Expect(Button("Stop")).ToBeDisabledAsync();
        await Expect(Button("Single capture")).ToBeEnabledAsync();
        await Expect(Button("Start streaming")).ToBeEnabledAsync();
        await Button("Single capture").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Captured 2000 samples at 781.3 kS/s.");
        (await Page.EvaluateAsync(() => Fixture.Model.Plot.Model.Subtitle)).Should().Be("Block: 2000 samples at 781.25 kS/s (1280 ns/sample)");
        (await Page.EvaluateAsync(() => Fixture.Model.IsStreaming)).Should().BeFalse();
    }

    [Fact]
    public async Task Start_streaming_after_stop_resumes_stream()
    {
        await Button("Stop").ClickAsync();
        await Expect(Button("Start streaming")).ToBeEnabledAsync();
        await Button("Start streaming").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Streaming.");
        await Expect(Button("Start streaming")).ToBeDisabledAsync();
        await Expect(Button("Single capture")).ToBeDisabledAsync();
        await Expect(Button("Stop")).ToBeEnabledAsync();
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Plot.Model.Subtitle ?? "", subtitle => subtitle.StartsWith("Streaming at"));
    }

    [Fact]
    public async Task Range_change_retitles_series_and_rescales_axis()
    {
        await SelectAsync("SelectedRangeOption", "+/-2 V");
        await Expect(Page.GetByTestId("SelectedRangeOption")).ToContainTextAsync("+/-2 V");
        await Fixture.Application.WaitForAsync(() => string.Join("|", Fixture.Model.Plot.Model.Series.Select(series => series.Title)),
            titles => titles == "Channel A (+/-2 V)|Channel B (+/-2 V)");
        (await Page.EvaluateAsync(() => Fixture.Model.Plot.Model.Axes[1].Minimum)).Should().Be(-2);
        (await Page.EvaluateAsync(() => Fixture.Model.Plot.Model.Axes[1].Maximum)).Should().Be(2);
        await Expect(Status).ToHaveTextAsync("Streaming.");
        Fixture.Device.Channels.Values.Select(settings => settings.Range).Distinct().Should().Equal(VoltageRange.Range2V);
    }

    [Fact]
    public async Task Range_change_while_stopped_applies_on_next_capture()
    {
        await StopAndCaptureAsync();
        await SelectAsync("SelectedRangeOption", "+/-2 V");
        await Expect(Page.GetByTestId("SelectedRangeOption")).ToContainTextAsync("+/-2 V");
        (await SeriesTitlesAsync()).Should().Be("Channel A (+/-5 V)|Channel B (+/-5 V)");
        (await Page.EvaluateAsync(() => Fixture.Model.IsStreaming)).Should().BeFalse();
        await Button("Single capture").ClickAsync();
        await Fixture.Application.WaitForAsync(() => string.Join("|", Fixture.Model.Plot.Model.Series.Select(series => series.Title)),
            titles => titles == "Channel A (+/-2 V)|Channel B (+/-2 V)");
        (await Page.EvaluateAsync(() => Fixture.Model.Plot.Model.Axes[1].Maximum)).Should().Be(2);
        await Expect(Status).ToHaveTextAsync(new Regex("^Captured 2000 samples"));
    }

    [Fact]
    public async Task Hiding_channel_a_hides_only_its_series()
    {
        await Page.GetByRole(AriaRole.Checkbox, new() { Name = "A", Exact = true }).UncheckAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Plot.Model.Series[0].IsVisible)).Should().BeFalse();
        (await Page.EvaluateAsync(() => Fixture.Model.Plot.Model.Series[1].IsVisible)).Should().BeTrue();
        await Expect(Page.GetByRole(AriaRole.Checkbox, new() { Name = "B", Exact = true })).ToBeCheckedAsync();
        // Hiding is chart-only: both channels keep acquiring.
        var subtitle = await Page.EvaluateAsync(() => Fixture.Model.Plot.Model.Subtitle);
        await Fixture.Application.WaitForAsync(() => Fixture.Model.Plot.Model.Subtitle, value => value != subtitle);
        await Expect(Status).ToHaveTextAsync("Streaming.");
        await Page.GetByRole(AriaRole.Checkbox, new() { Name = "A", Exact = true }).CheckAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.Plot.Model.Series[0].IsVisible)).Should().BeTrue();
    }

    [Fact]
    public async Task Generator_toggle_reports_wave_and_frequency()
    {
        await SelectAsync("SelectedWaveType", "Square");
        await Page.GetByTestId("GeneratorFrequencyText").FillAsync("250");
        await Button("Toggle").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Signal generator: Square at 250 Hz, 2 Vpp. Channel A follows it.");
        (await Page.EvaluateAsync(() => Fixture.Model.GeneratorRunning)).Should().BeTrue();
        await Button("Toggle").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Signal generator off.");
        (await Page.EvaluateAsync(() => Fixture.Model.GeneratorRunning)).Should().BeFalse();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-5")]
    public async Task Invalid_generator_frequency_is_rejected(string frequency)
    {
        await Page.GetByTestId("GeneratorFrequencyText").FillAsync(frequency);
        await Button("Toggle").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Enter a generator frequency in hertz.");
        (await Page.EvaluateAsync(() => Fixture.Model.GeneratorRunning)).Should().BeFalse();
    }

    [Fact]
    public async Task Generator_frequency_above_limit_reports_error()
    {
        await Page.GetByTestId("GeneratorFrequencyText").FillAsync("200000");
        await Button("Toggle").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Signal generator error: The signal generator on a 2204A tops out at 100000 Hz.");
        (await Page.EvaluateAsync(() => Fixture.Model.GeneratorRunning)).Should().BeFalse();
    }

    [Fact]
    public async Task Flash_led_reports_status()
    {
        await Button("Flash LED").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Flashing the device LED.");
    }

    [Fact]
    public async Task No_registered_scope_disables_every_command()
    {
        await Fixture.RestartAsync(null, "No scope available");
        await Expect(Status).ToHaveTextAsync("No scope available -- nothing registered with ScopeDeviceFinder.");
        foreach (var command in Commands) await Expect(Button(command)).ToBeDisabledAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.DeviceText)).Should().BeEmpty();
    }

    [Fact]
    public async Task Open_failure_is_reported_as_status()
    {
        await Fixture.RestartAsync(FixtureScopeDevice.Failing("Fixture scope is unavailable."), "Could not start");
        await Expect(Status).ToHaveTextAsync("Could not start: Fixture scope is unavailable.");
        foreach (var command in Commands) await Expect(Button(command)).ToBeDisabledAsync();
    }

    [Fact]
    public async Task Real_device_hides_simulated_badge()
    {
        var device = FixtureScopeDevice.Hardware("Fixture PicoScope 2204A", holdCapture: true);
        await Fixture.RestartAsync(device, "Connected to");
        await Expect(Status).ToHaveTextAsync("Connected to Fixture PicoScope 2204A.");
        await Expect(Page.GetByText("SIMULATED", new() { Exact = true })).ToBeHiddenAsync();
        (await Page.EvaluateAsync(() => Fixture.Model.SimulatedBadge)).Should().BeEmpty();
        device.ReleaseCapture();
        await Expect(Status).ToHaveTextAsync("Streaming.");
        await Button("Toggle").ClickAsync();
        await Expect(Status).ToHaveTextAsync("Signal generator: Sine at 5,000 Hz, 2 Vpp. Loop the generator output into Channel A to see it.");
    }
}
