using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PicoScope.Brix.ScopeData.Model;
using PicoScope.Brix.ScopeData.Simulation;
using SilverAssertions;
using Xunit;

namespace PicoScope.Brix.ScopeData.Tests;

public class SimulatedScopeDataDeviceTests
{
    private static SimulatedScopeDataDevice Open()
    {
        var scope = new SimulatedScopeDataDevice();
        scope.OpenScope().Should().BeTrue();
        return scope;
    }

    // ---------------------------------------------------------------- lifecycle

    [Fact]
    public void IsSimulated_is_true()
    {
        using var scope = new SimulatedScopeDataDevice();
        scope.IsSimulated.Should().BeTrue();
    }

    [Fact]
    public void OpenScope_populates_capabilities_and_unit_info()
    {
        //Arrange
        using var scope = new SimulatedScopeDataDevice();
        scope.Capabilities.Should().BeNull();

        //Act
        bool opened = scope.OpenScope();

        //Assert
        opened.Should().BeTrue();
        scope.IsOpen.Should().BeTrue();
        scope.Capabilities.Variant.Should().Be("2204A");
        scope.UnitInfo.Variant.Should().Contain("2204A");
        scope.UnitInfo.ToString().Should().Contain("simulated");
    }

    [Fact]
    public void OpenScope_is_idempotent()
    {
        //Arrange
        using var scope = Open();

        //Act + Assert
        scope.OpenScope().Should().BeTrue();
        scope.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Ping_reports_the_open_state()
    {
        using var scope = new SimulatedScopeDataDevice();
        scope.Ping().Should().BeFalse();
        scope.OpenScope();
        scope.Ping().Should().BeTrue();
        scope.CloseScope();
        scope.Ping().Should().BeFalse();
    }

    [Fact]
    public void device_calls_throw_ScopeNotOpenException_before_OpenScope()
    {
        //Arrange
        using var scope = new SimulatedScopeDataDevice();

        //Act + Assert
        Assert.Throws<ScopeNotOpenException>(() => scope.SetChannel(ChannelId.ChannelA, ChannelSettings.Default));
        Assert.Throws<ScopeNotOpenException>(() => scope.GetTimebase(1, 100));
        Assert.Throws<ScopeNotOpenException>(() => scope.FlashLed());
        Assert.Throws<ScopeNotOpenException>(() => scope.StartStreaming(new StreamingSettings()));
        Assert.Throws<ScopeNotOpenException>(() => scope.SetSignalGenerator(SignalGeneratorSettings.Off));
    }

    [Fact]
    public void Dispose_closes_the_scope_and_blocks_further_use()
    {
        //Arrange
        var scope = Open();

        //Act
        scope.Dispose();

        //Assert
        scope.IsOpen.Should().BeFalse();
        Assert.Throws<ObjectDisposedException>(() => scope.OpenScope());
        Assert.Throws<ObjectDisposedException>(() => scope.SetChannel(ChannelId.ChannelA, ChannelSettings.Default));
    }

    [Fact]
    public void GetUnitInfoLine_matches_the_unit_info()
    {
        //Arrange
        using var scope = Open();

        //Act + Assert
        scope.GetUnitInfoLine(UnitInfoLine.VariantInfo).Should().Be(scope.UnitInfo.Variant);
        scope.GetUnitInfoLine(UnitInfoLine.BatchAndSerial).Should().Be(scope.UnitInfo.BatchAndSerial);
        scope.GetUnitInfoLine(UnitInfoLine.DriverPath).Should().Be(scope.UnitInfo.DriverPath);
    }

    [Fact]
    public void GetLastButtonPress_reports_no_press_like_a_2204A()
    {
        using var scope = Open();
        scope.GetLastButtonPress().Should().Be(0);
    }

    // ----------------------------------------------------------------- channels

    [Fact]
    public void channels_start_enabled_at_the_default_settings()
    {
        //Arrange
        using var scope = Open();

        //Assert
        scope.Channels.Count.Should().Be(2);
        scope.Channels[ChannelId.ChannelA].Should().Be(ChannelSettings.Default);
        scope.GetEnabledChannels().Should().Equal(new[] { ChannelId.ChannelA, ChannelId.ChannelB });
    }

    [Fact]
    public void SetChannel_applies_the_settings()
    {
        //Arrange
        using var scope = Open();
        var settings = new ChannelSettings(true, Coupling.Ac, VoltageRange.Range200mV);

        //Act
        scope.SetChannel(ChannelId.ChannelB, settings);

        //Assert
        scope.Channels[ChannelId.ChannelB].Should().Be(settings);
    }

    [Fact]
    public void SetChannel_rejects_a_range_the_device_does_not_have()
    {
        using var scope = Open();
        Assert.Throws<ScopeCapabilityException>(
            () => scope.SetChannel(ChannelId.ChannelA, new ChannelSettings(true, Coupling.Dc, VoltageRange.Range10mV)));
        Assert.Throws<ScopeCapabilityException>(
            () => scope.SetChannel(ChannelId.ChannelA, new ChannelSettings(true, Coupling.Dc, VoltageRange.Range50V)));
    }

    [Fact]
    public void SetChannel_rejects_a_channel_the_device_does_not_have()
    {
        using var scope = Open();
        Assert.Throws<ScopeCapabilityException>(() => scope.SetChannel(ChannelId.ChannelC, ChannelSettings.Default));
    }

    [Fact]
    public void GetEnabledChannels_omits_a_disabled_channel()
    {
        //Arrange
        using var scope = Open();

        //Act
        scope.SetChannel(ChannelId.ChannelA, ChannelSettings.Default.AsDisabled());

        //Assert
        scope.GetEnabledChannels().Should().Equal(new[] { ChannelId.ChannelB });
    }

    // ----------------------------------------------------------------- timebase

    [Fact]
    public void GetTimebase_follows_the_ten_times_two_to_the_n_rule()
    {
        //Arrange
        using var scope = Open();

        //Act
        TimebaseInfo tb5 = scope.GetTimebase(5, 1000);
        TimebaseInfo tb5x2 = scope.GetTimebase(5, 1000, oversample: 2);

        //Assert
        tb5.IsValid.Should().BeTrue();
        tb5.IntervalNanoseconds.Should().Be(320);
        tb5.MaxSamples.Should().Be(3968);
        tb5x2.IntervalNanoseconds.Should().Be(640);
        tb5x2.MaxSamples.Should().Be(1984);
    }

    [Fact]
    public void GetTimebase_is_invalid_outside_the_device_limits()
    {
        //Arrange
        using var scope = Open();

        //Act + Assert
        scope.GetTimebase(0, 100).IsValid.Should().BeFalse();       //two channels need timebase 1 or slower
        scope.GetTimebase(24, 100).IsValid.Should().BeFalse();      //past MaxTimebase
        scope.GetTimebase(5, 100, oversample: 5).IsValid.Should().BeFalse();
        scope.GetTimebase(5, 100, oversample: 0).IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetTimebase_allows_timebase_zero_with_one_channel()
    {
        //Arrange
        using var scope = Open();
        scope.SetChannel(ChannelId.ChannelB, ChannelSettings.Default.AsDisabled());

        //Act
        TimebaseInfo tb0 = scope.GetTimebase(0, 100);

        //Assert
        tb0.IsValid.Should().BeTrue();
        tb0.IntervalNanoseconds.Should().Be(10);
        tb0.MaxSamples.Should().Be(8064);
        tb0.SampleRateHz.Should().BeApproximately(100_000_000, 1e-3);
    }

    [Fact]
    public void FindTimebase_returns_the_first_interval_at_least_the_one_requested()
    {
        //Arrange
        using var scope = Open();

        //Act
        TimebaseInfo found = scope.FindTimebase(1000, 2000);

        //Assert
        found.IsValid.Should().BeTrue();
        found.Timebase.Should().Be(7);
        found.IntervalNanoseconds.Should().Be(1280);
    }

    [Fact]
    public void FindTimebase_reports_invalid_when_nothing_is_slow_enough()
    {
        using var scope = Open();
        scope.FindTimebase(1e12, 100).IsValid.Should().BeFalse();
    }

    // ------------------------------------------------------------------ trigger

    [Fact]
    public void SetTrigger_accepts_a_channel_source_and_rejects_the_external_input()
    {
        //Arrange
        using var scope = Open();

        //Act + Assert
        scope.SetTrigger(TriggerSettings.RisingEdge(ChannelId.ChannelA, VoltageRange.Range5V, 1000));
        Assert.Throws<ScopeCapabilityException>(
            () => scope.SetTrigger(TriggerSettings.Disabled with { Source = ChannelId.External }));
        scope.DisableTrigger();
    }

    [Fact]
    public void SetAdvancedTrigger_rejects_null()
    {
        using var scope = Open();
        Assert.Throws<ArgumentNullException>(() => scope.SetAdvancedTrigger(null));
    }

    // ---------------------------------------------------------------------- ETS

    [Fact]
    public void SetEts_clamps_to_the_device_limits()
    {
        //Arrange
        using var scope = Open();

        //Act
        EtsResult result = scope.SetEts(EtsMode.Fast, 1000, 100);

        //Assert
        result.IsSupported.Should().BeTrue();
        result.Mode.Should().Be(EtsMode.Fast);
        result.Cycles.Should().Be(250);
        result.Interleave.Should().Be(40);
        result.EffectiveIntervalPicoseconds.Should().Be(500);
    }

    [Fact]
    public void SetEts_off_reports_no_effective_interval()
    {
        using var scope = Open();
        scope.SetEts(EtsMode.Off, 10, 10).IsSupported.Should().BeFalse();
    }

    // --------------------------------------------------------------- block mode

    [Fact]
    public async Task RunBlockAsync_returns_the_requested_samples_within_the_channel_range()
    {
        //Arrange
        using var scope = Open();

        //Act
        CaptureBlock block = await scope.RunBlockAsync(2000, 7, cancellationToken: TestContext.Current.CancellationToken);

        //Assert
        block.SampleCount.Should().Be(2000);
        block.IntervalNanoseconds.Should().Be(1280);
        block.Channels.Count.Should().Be(2);
        foreach (ChannelSamples samples in block.Channels.Values)
        {
            samples.Count.Should().Be(2000);
            samples.Range.Should().Be(VoltageRange.Range5V);
            for (int i = 0; i < samples.Count; i++)
            {
                Math.Abs(samples.VoltsAt(i)).Should().BeLessThanOrEqualTo(5.0);
            }
        }
    }

    [Fact]
    public async Task RunBlockAsync_only_returns_enabled_channels()
    {
        //Arrange
        using var scope = Open();
        scope.SetChannel(ChannelId.ChannelB, ChannelSettings.Default.AsDisabled());

        //Act
        CaptureBlock block = await scope.RunBlockAsync(100, 3, cancellationToken: TestContext.Current.CancellationToken);

        //Assert
        block.Channels.Keys.Should().Equal(new[] { ChannelId.ChannelA });
    }

    [Fact]
    public async Task RunBlockAsync_rejects_more_samples_than_the_buffer_holds()
    {
        using var scope = Open();
        await Assert.ThrowsAsync<PicoScopeException>(() => scope.RunBlockAsync(4000, 7, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RunBlockAsync_rejects_an_invalid_timebase()
    {
        using var scope = Open();
        await Assert.ThrowsAsync<PicoScopeException>(() => scope.RunBlockAsync(100, 24, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RunBlockAsync_includes_timestamps_when_asked()
    {
        //Arrange
        using var scope = Open();

        //Act
        CaptureBlock without = await scope.RunBlockAsync(100, 7, cancellationToken: TestContext.Current.CancellationToken);
        CaptureBlock with = await scope.RunBlockAsync(100, 7, includeTimestamps: true, cancellationToken: TestContext.Current.CancellationToken);

        //Assert
        without.Timestamps.Should().BeNull();
        with.Timestamps.Length.Should().Be(100);
        with.TimestampUnits.Should().Be(TimeUnits.Nanoseconds);
        with.Timestamps[0].Should().Be(0);
        with.Timestamps[1].Should().Be(1280);
    }

    [Fact]
    public async Task RunBlockAsync_honours_cancellation()
    {
        //Arrange
        using var scope = Open();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.Cancel();

        //Act + Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => scope.RunBlockAsync(2000, 20, cancellationToken: cts.Token));
    }

    // ---------------------------------------------------------------- streaming

    [Fact]
    public void StartStreaming_delivers_batches_until_stopped()
    {
        //Arrange
        using var scope = Open();
        using var received = new ManualResetEventSlim(false);
        StreamingSamplesEventArgs first = null;
        scope.SamplesAvailable += (_, e) =>
        {
            first ??= e;
            received.Set();
        };

        //Act
        scope.StartStreaming(new StreamingSettings { PollIntervalMilliseconds = 5 });
        bool arrived = received.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        scope.IsStreaming.Should().BeTrue();
        scope.StopStreaming();

        //Assert
        arrived.Should().BeTrue();
        scope.IsStreaming.Should().BeFalse();
        first.Channels.Count.Should().Be(2);
        first.SampleCount.Should().BeGreaterThanOrEqualTo(1);
        first.TotalSampleCount.Should().Be(first.SampleCount);
        first.IntervalNanoseconds.Should().Be(100_000);
        first.BufferOverrun.Should().BeFalse();
    }

    [Fact]
    public void StartStreaming_rejects_null_settings()
    {
        using var scope = Open();
        Assert.Throws<ArgumentNullException>(() => scope.StartStreaming(null));
    }

    [Fact]
    public void StopStreaming_is_safe_when_not_streaming()
    {
        using var scope = Open();
        scope.StopStreaming();
        scope.Stop();
        scope.IsStreaming.Should().BeFalse();
    }

    [Fact]
    public void CloseScope_stops_a_running_stream()
    {
        //Arrange
        using var scope = Open();
        scope.StartStreaming(new StreamingSettings { PollIntervalMilliseconds = 5 });

        //Act
        scope.CloseScope();

        //Assert
        scope.IsStreaming.Should().BeFalse();
        scope.IsOpen.Should().BeFalse();
    }

    // --------------------------------------------------------- signal generator

    [Fact]
    public void SetSignalGenerator_rejects_a_frequency_above_the_limit()
    {
        using var scope = Open();
        Assert.Throws<ScopeCapabilityException>(
            () => scope.SetSignalGenerator(SignalGeneratorSettings.Tone(WaveType.Sine, 200_000, 1.0)));
    }

    [Fact]
    public void SetSignalGenerator_rejects_an_amplitude_above_the_limit()
    {
        using var scope = Open();
        Assert.Throws<ScopeCapabilityException>(
            () => scope.SetSignalGenerator(SignalGeneratorSettings.Tone(WaveType.Sine, 1000, 5.0)));
    }

    [Fact]
    public async Task SetSignalGenerator_makes_channel_A_follow_the_generator()
    {
        //Arrange
        using var scope = Open();
        scope.SimulatedNoiseFraction = 0;
        scope.SetSignalGenerator(SignalGeneratorSettings.Tone(WaveType.Sine, 1000, 2.0));

        //Act
        CaptureBlock block = await scope.RunBlockAsync(2000, 7, cancellationToken: TestContext.Current.CancellationToken);   //2.56 ms: a few cycles of 1 kHz

        //Assert
        ChannelSamples a = block.Channels[ChannelId.ChannelA];
        double max = Enumerable.Range(0, a.Count).Max(i => a.VoltsAt(i));
        double min = Enumerable.Range(0, a.Count).Min(i => a.VoltsAt(i));
        max.Should().BeApproximately(1.0, 0.01);
        min.Should().BeApproximately(-1.0, 0.01);
    }

    [Fact]
    public async Task StopSignalGenerator_returns_channel_A_to_the_synthesised_signal()
    {
        //Arrange
        using var scope = Open();
        scope.SimulatedNoiseFraction = 0;
        scope.SetSignalGenerator(SignalGeneratorSettings.Tone(WaveType.DcVoltage, 0, 0));

        //Act
        scope.StopSignalGenerator();
        CaptureBlock block = await scope.RunBlockAsync(2000, 7, cancellationToken: TestContext.Current.CancellationToken);

        //Assert: the built-in 2 V fundamental with third harmonic swings well past 1 V
        ChannelSamples a = block.Channels[ChannelId.ChannelA];
        double max = Enumerable.Range(0, a.Count).Max(i => a.VoltsAt(i));
        max.Should().BeGreaterThanOrEqualTo(1.5);
    }

    [Fact]
    public void SetArbitraryWaveform_enforces_the_buffer_size()
    {
        //Arrange
        using var scope = Open();

        //Act + Assert
        scope.SetArbitraryWaveform(ArbitraryWaveformSettings.Fixed(new byte[4096], 1000, 1.0));
        Assert.Throws<ScopeCapabilityException>(
            () => scope.SetArbitraryWaveform(ArbitraryWaveformSettings.Fixed(new byte[4097], 1000, 1.0)));
        Assert.Throws<ArgumentException>(
            () => scope.SetArbitraryWaveform(
                new ArbitraryWaveformSettings(Array.Empty<byte>(), 0, 1_000_000, 1, 1, 0, 0, SweepType.Up, 0)));
    }
}
