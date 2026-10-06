using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using Microsoft.UI.Xaml;
using PicoScope.Brix.ScopeData;
using PicoScope.Brix.ScopeData.Model;
using PicoScope.Brix.ScopeData.Simulation;
using PicoScope.Brix.ViewModels;
using PicoScope.Brix.Views;

namespace PicoScope.Brix.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    private const string Streaming = "Streaming.";
    // The launch page asks ScopeDeviceFinder for a scope too. This absent "hardware" records that it has
    // asked, so the first reset cannot register a scope the launch page would then open and keep.
    private readonly FixtureScopeDevice _launchProbe = FixtureScopeDevice.Absent();
    private IScopeDataDevice _next;
    private bool _hasNext;
    private string _expectedStatus = Streaming;
    public IScopeDataDevice Device { get; private set; }
    public MainViewModel Model => (MainViewModel)View.DataContext;
    protected override Application CreateApplication() => new App();
    protected override void Prepare()
    {
        ScopeDeviceFinder.Reset();
        ScopeDeviceFinder.Register(_launchProbe);
    }

    // Each page releases the scope on Unloaded with ScopeDeviceFinder.Reset(), which disposes every
    // registered device, and a disposed simulator cannot reopen. So the old page is shut down here,
    // before the fresh scope is registered; its later Unloaded call then has nothing left to release.
    protected override async Task BeforeResetAsync()
    {
        if (View == null) await _launchProbe.OpenAttempted.Task.WaitAsync(TimeSpan.FromSeconds(30));
        else
        {
            (Device as FixtureScopeDevice)?.ReleaseCapture();
            await Application.EvaluateAsync(() => Model.Shutdown());
        }
        ScopeDeviceFinder.Reset();
        Device = _hasNext ? _next : new SimulatedScopeDataDevice { SimulatedNoiseFraction = 0 };
        if (Device != null) ScopeDeviceFinder.Register(Device);
    }
    protected override async Task AfterResetAsync()
    {
        var expected = _expectedStatus;
        _next = null;
        _hasNext = false;
        _expectedStatus = Streaming;
        await Application.WaitForAsync(() => Model.StatusText, status => status.StartsWith(expected, StringComparison.Ordinal),
            description: "startup status '" + expected + "'");
    }
    protected override void Cleanup() => ScopeDeviceFinder.Reset();

    // Replaces the page with one that starts against the given scope (null registers none) and waits for
    // the startup status to begin with expectedStatus.
    public Task RestartAsync(IScopeDataDevice device, string expectedStatus)
    {
        _next = device;
        _hasNext = true;
        _expectedStatus = expectedStatus;
        return ResetAsync();
    }
}

// A scope at the ScopeDeviceFinder boundary: a noise-free simulator underneath, presented as absent,
// failing or real hardware, with an optional gate that holds the startup capture.
public sealed class FixtureScopeDevice : IScopeDataDevice
{
    private readonly SimulatedScopeDataDevice _inner = new() { SimulatedNoiseFraction = 0 };
    private readonly bool _present;
    private readonly string _openFailure;
    private TaskCompletionSource _captureGate;

    private FixtureScopeDevice(string name, bool isSimulated, bool present, string openFailure)
    {
        Name = name;
        IsSimulated = isSimulated;
        _present = present;
        _openFailure = openFailure;
    }

    public static FixtureScopeDevice Absent() => new("Absent fixture scope", false, false, null);
    public static FixtureScopeDevice Failing(string message) => new("Failing fixture scope", true, true, message);
    public static FixtureScopeDevice Hardware(string name, bool holdCapture)
    {
        var device = new FixtureScopeDevice(name, false, true, null);
        if (holdCapture) device._captureGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        return device;
    }

    public TaskCompletionSource OpenAttempted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource CaptureStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public void ReleaseCapture() => _captureGate?.TrySetResult();

    public string Name { get; }
    public bool IsSimulated { get; }
    public bool IsOpen => _inner.IsOpen;
    public ScopeCapabilities Capabilities => _inner.Capabilities;
    public UnitInfo UnitInfo => _inner.UnitInfo;
    public IReadOnlyDictionary<ChannelId, ChannelSettings> Channels => _inner.Channels;
    public bool IsStreaming => _inner.IsStreaming;

    public event EventHandler<StreamingSamplesEventArgs> SamplesAvailable
    {
        add => _inner.SamplesAvailable += value;
        remove => _inner.SamplesAvailable -= value;
    }

    public bool OpenScope()
    {
        OpenAttempted.TrySetResult();
        if (_openFailure != null) throw new PicoScopeException(_openFailure);
        return _present && _inner.OpenScope();
    }

    public async Task<CaptureBlock> RunBlockAsync(int sampleCount, int timebase, int oversample = 1,
        bool includeTimestamps = false, CancellationToken cancellationToken = default)
    {
        CaptureStarted.TrySetResult();
        if (_captureGate != null) await _captureGate.Task.WaitAsync(cancellationToken);
        return await _inner.RunBlockAsync(sampleCount, timebase, oversample, includeTimestamps, cancellationToken);
    }

    public void CloseScope() => _inner.CloseScope();
    public bool Ping() => _inner.Ping();
    public void FlashLed() => _inner.FlashLed();
    public int GetLastButtonPress() => _inner.GetLastButtonPress();
    public string GetUnitInfoLine(UnitInfoLine line) => _inner.GetUnitInfoLine(line);
    public void SetChannel(ChannelId channel, ChannelSettings settings) => _inner.SetChannel(channel, settings);
    public IReadOnlyList<ChannelId> GetEnabledChannels() => _inner.GetEnabledChannels();
    public TimebaseInfo GetTimebase(int timebase, int sampleCount, int oversample = 1) =>
        _inner.GetTimebase(timebase, sampleCount, oversample);
    public TimebaseInfo FindTimebase(double desiredIntervalNanoseconds, int sampleCount, int oversample = 1) =>
        _inner.FindTimebase(desiredIntervalNanoseconds, sampleCount, oversample);
    public void SetTrigger(TriggerSettings trigger) => _inner.SetTrigger(trigger);
    public void DisableTrigger() => _inner.DisableTrigger();
    public void SetAdvancedTrigger(AdvancedTriggerSettings settings) => _inner.SetAdvancedTrigger(settings);
    public void SetPulseWidthQualifier(PulseWidthQualifier qualifier) => _inner.SetPulseWidthQualifier(qualifier);
    public EtsResult SetEts(EtsMode mode, int cycles, int interleave) => _inner.SetEts(mode, cycles, interleave);
    public void Stop() => _inner.Stop();
    public void StartStreaming(StreamingSettings settings) => _inner.StartStreaming(settings);
    public void StopStreaming() => _inner.StopStreaming();
    public void SetSignalGenerator(SignalGeneratorSettings settings) => _inner.SetSignalGenerator(settings);
    public void SetArbitraryWaveform(ArbitraryWaveformSettings settings) => _inner.SetArbitraryWaveform(settings);
    public void StopSignalGenerator() => _inner.StopSignalGenerator();

    public void Dispose()
    {
        ReleaseCapture();
        _inner.Dispose();
    }
}
