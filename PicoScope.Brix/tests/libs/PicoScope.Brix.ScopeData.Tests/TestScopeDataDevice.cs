using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PicoScope.Brix.ScopeData.Model;

namespace PicoScope.Brix.ScopeData.Tests;

/// <summary>
/// A scripted <see cref="IScopeDataDevice"/> for tests: it plays back exactly
/// the samples it is given, records what was asked of it, and can be told
/// whether to open. Tests can therefore assert precise values and precise calls
/// rather than tolerate the simulator's synthesised signal.
/// </summary>
public sealed class TestScopeDataDevice : IScopeDataDevice
{
    private readonly Dictionary<ChannelId, ChannelSettings> _channels = new Dictionary<ChannelId, ChannelSettings>();
    private readonly Dictionary<ChannelId, short[]> _scripted = new Dictionary<ChannelId, short[]>();
    private long _totalStreamed;

    public TestScopeDataDevice(string name = "Test scope", bool isSimulated = false)
    {
        Name = name;
        IsSimulated = isSimulated;
        foreach (ChannelId channel in Capabilities.SupportedChannels)
        {
            _channels[channel] = ChannelSettings.Default;
        }
    }

    // ------------------------------------------------------- scripting knobs

    /// <summary>Whether <see cref="OpenScope"/> succeeds. False models "nothing plugged in".</summary>
    public bool CanOpen { get; set; } = true;

    /// <summary>When set, <see cref="OpenScope"/> throws this instead of opening.</summary>
    public Exception OpenException { get; set; }

    public int OpenCount { get; private set; }
    public int CloseCount { get; private set; }
    public int FlashLedCount { get; private set; }
    public int StopCount { get; private set; }
    public bool IsDisposed { get; private set; }
    public List<(ChannelId Channel, ChannelSettings Settings)> ChannelCalls { get; } = new List<(ChannelId, ChannelSettings)>();
    public TriggerSettings? LastTrigger { get; private set; }
    public StreamingSettings LastStreamingSettings { get; private set; }
    public SignalGeneratorSettings? LastSignalGenerator { get; private set; }
    public bool SignalGeneratorRunning { get; private set; }

    /// <summary>
    /// Sets the samples a channel plays back. A capture or batch longer than the
    /// script repeats it; an unscripted channel plays zeros.
    /// </summary>
    public void Script(ChannelId channel, params short[] samples) => _scripted[channel] = samples;

    /// <summary>
    /// Raises <see cref="SamplesAvailable"/> synchronously with scripted samples,
    /// as a running stream would from its polling thread.
    /// </summary>
    public StreamingSamplesEventArgs EmitBatch(int sampleCount)
    {
        if (!IsStreaming) { throw new InvalidOperationException("Not streaming."); }

        var channels = new Dictionary<ChannelId, ChannelSamples>();
        foreach (ChannelId channel in GetEnabledChannels())
        {
            channels[channel] = new ChannelSamples(channel, _channels[channel].Range, Playback(channel, sampleCount));
        }

        _totalStreamed += sampleCount;
        var args = new StreamingSamplesEventArgs
        {
            Channels = channels,
            SampleCount = sampleCount,
            TotalSampleCount = _totalStreamed,
            IntervalNanoseconds = LastStreamingSettings?.IntervalInNanoseconds() ?? 0
        };
        SamplesAvailable?.Invoke(this, args);
        return args;
    }

    // -------------------------------------------------------- IScopeDataDevice

    public string Name { get; }
    public bool IsSimulated { get; }
    public bool IsOpen { get; private set; }
    public ScopeCapabilities Capabilities { get; set; } = ScopeCapabilities.PicoScope2204A;
    public UnitInfo UnitInfo { get; set; } =
        new UnitInfo("TEST", "TEST/0001", "01Jan26", "test", "test", "n/a", "n/a", "(none)", "0");
    public IReadOnlyDictionary<ChannelId, ChannelSettings> Channels => _channels;
    public bool IsStreaming { get; private set; }
    public event EventHandler<StreamingSamplesEventArgs> SamplesAvailable;

    public bool OpenScope()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        OpenCount++;
        if (OpenException != null) { throw OpenException; }
        if (!CanOpen) { return false; }
        IsOpen = true;
        return true;
    }

    public void CloseScope()
    {
        if (!IsOpen) { return; }
        CloseCount++;
        StopStreaming();
        IsOpen = false;
    }

    public bool Ping() => IsOpen;

    public void FlashLed()
    {
        EnsureOpen();
        FlashLedCount++;
    }

    public int GetLastButtonPress()
    {
        EnsureOpen();
        return 0;
    }

    public string GetUnitInfoLine(UnitInfoLine line)
    {
        EnsureOpen();
        return line == UnitInfoLine.VariantInfo ? UnitInfo.Variant : string.Empty;
    }

    public void SetChannel(ChannelId channel, ChannelSettings settings)
    {
        EnsureOpen();
        ChannelCalls.Add((channel, settings));
        _channels[channel] = settings;
    }

    public IReadOnlyList<ChannelId> GetEnabledChannels()
        => _channels.Where(c => c.Value.Enabled).Select(c => c.Key).OrderBy(c => (int)c).ToArray();

    public TimebaseInfo GetTimebase(int timebase, int sampleCount, int oversample = 1)
    {
        EnsureOpen();
        if (timebase < 0 || timebase > Capabilities.MaxTimebase || oversample < 1)
        {
            return TimebaseInfo.Invalid(timebase, oversample);
        }

        long intervalNs = 10L * (1L << timebase) * oversample;
        int maxSamples = Capabilities.MaxSamplesFor(GetEnabledChannels().Count);
        return new TimebaseInfo(timebase, true, intervalNs, TimeUnits.Nanoseconds, maxSamples, oversample);
    }

    public TimebaseInfo FindTimebase(double desiredIntervalNanoseconds, int sampleCount, int oversample = 1)
    {
        EnsureOpen();
        for (int tb = 0; tb <= Capabilities.MaxTimebase; tb++)
        {
            TimebaseInfo info = GetTimebase(tb, sampleCount, oversample);
            if (info.IsValid && info.IntervalNanoseconds >= desiredIntervalNanoseconds) { return info; }
        }

        return TimebaseInfo.Invalid(Capabilities.MaxTimebase, oversample);
    }

    public void SetTrigger(TriggerSettings trigger)
    {
        EnsureOpen();
        LastTrigger = trigger;
    }

    public void DisableTrigger()
    {
        EnsureOpen();
        LastTrigger = TriggerSettings.Disabled;
    }

    public void SetAdvancedTrigger(AdvancedTriggerSettings settings) => EnsureOpen();

    public void SetPulseWidthQualifier(PulseWidthQualifier qualifier) => EnsureOpen();

    public EtsResult SetEts(EtsMode mode, int cycles, int interleave)
    {
        EnsureOpen();
        return new EtsResult(mode, cycles, interleave,
            mode == EtsMode.Off ? 0 : Capabilities.EtsEffectiveIntervalPicoseconds);
    }

    public Task<CaptureBlock> RunBlockAsync(
        int sampleCount,
        int timebase,
        int oversample = 1,
        bool includeTimestamps = false,
        CancellationToken cancellationToken = default)
    {
        EnsureOpen();
        TimebaseInfo info = GetTimebase(timebase, sampleCount, oversample);
        if (!info.IsValid) { throw new PicoScopeException($"Timebase {timebase} is not valid."); }

        var channels = new Dictionary<ChannelId, ChannelSamples>();
        foreach (ChannelId channel in GetEnabledChannels())
        {
            channels[channel] = new ChannelSamples(channel, _channels[channel].Range, Playback(channel, sampleCount));
        }

        int[] timestamps = null;
        if (includeTimestamps)
        {
            timestamps = new int[sampleCount];
            for (int i = 0; i < sampleCount; i++) { timestamps[i] = (int)(i * info.IntervalNanoseconds); }
        }

        return Task.FromResult(new CaptureBlock
        {
            Channels = channels,
            IntervalNanoseconds = info.IntervalNanoseconds,
            SampleCount = sampleCount,
            Timestamps = timestamps,
            TimestampUnits = TimeUnits.Nanoseconds
        });
    }

    public void Stop()
    {
        StopCount++;
        StopStreaming();
    }

    public void StartStreaming(StreamingSettings settings)
    {
        EnsureOpen();
        if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
        LastStreamingSettings = settings;
        _totalStreamed = 0;
        IsStreaming = true;
    }

    public void StopStreaming() => IsStreaming = false;

    public void SetSignalGenerator(SignalGeneratorSettings settings)
    {
        EnsureOpen();
        LastSignalGenerator = settings;
        SignalGeneratorRunning = true;
    }

    public void SetArbitraryWaveform(ArbitraryWaveformSettings settings)
    {
        EnsureOpen();
        SignalGeneratorRunning = true;
    }

    public void StopSignalGenerator() => SignalGeneratorRunning = false;

    public void Dispose()
    {
        if (IsDisposed) { return; }
        IsDisposed = true;
        CloseScope();
    }

    private short[] Playback(ChannelId channel, int sampleCount)
    {
        short[] result = new short[sampleCount];
        if (_scripted.TryGetValue(channel, out short[] script) && script.Length > 0)
        {
            for (int i = 0; i < sampleCount; i++) { result[i] = script[i % script.Length]; }
        }

        return result;
    }

    private void EnsureOpen()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (!IsOpen) { throw new ScopeNotOpenException("use the test scope"); }
    }
}
