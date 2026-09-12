using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PicoScope.Brix.ScopeData;
using PicoScope.Brix.ScopeData.Model;
using PicoScope.Brix.ScopeData.Ps2000.Interop;

namespace PicoScope.Brix.ScopeData.Ps2000;

/// <summary>
/// An <see cref="IScopeDataDevice"/> backed by a real PicoScope 2000-series device,
/// through the ps2000 driver.
/// </summary>
/// <remarks>
/// <para>
/// This is the reference implementation: it shows how each part of the API is
/// meant to be driven, and it encodes the behaviours that are easy to get wrong.
/// The awkward ones, all verified against a physical PicoScope 2204A:
/// </para>
/// <list type="bullet">
/// <item><description>
/// <c>ps2000_get_timebase</c> returns <b>1 for success and 0 for failure</b>,
/// the opposite of every newer Pico driver's <c>PICO_STATUS</c>.
/// </description></item>
/// <item><description>
/// Its <c>time_interval</c> output is <b>always nanoseconds</b>; the
/// accompanying <c>time_units</c> describes the timestamp array instead.
/// </description></item>
/// <item><description>
/// Full scale is <b>32767</b>, not the ps2000a driver's 32512.
/// </description></item>
/// <item><description>
/// The streaming callback delegate must be rooted in managed code for as long
/// as streaming runs -- see <see cref="_streamingCallback"/>.
/// </description></item>
/// <item><description>
/// The driver has to be found at run time, and on Windows it usually lives
/// outside the SDK folder; see <see cref="Ps2000DriverLoader"/>. On Linux the
/// same code binds to <c>libps2000.so</c> with no change.
/// </description></item>
/// </list>
/// <para>
/// Only one process may hold the device open, so opening fails while the
/// PicoScope desktop application is running.
/// </para>
/// </remarks>
public sealed class Ps2000ScopeDataDevice : IScopeDataDevice
{
    private readonly Dictionary<ChannelId, ChannelSettings> _channels = new Dictionary<ChannelId, ChannelSettings>();
    private readonly object _syncRoot = new object();

    private short _handle;
    private bool _disposed;
    private CancellationTokenSource _streamCancellation;
    private Task _streamTask;
    private long _totalStreamedSamples;
    private double _streamIntervalNs;
    private int _streamChannelCount;

    //Remembered so an acquisition interrupted to reach the signal generator can
    //  be put back exactly as it was. See WithAcquisitionPaused.
    private StreamingSettings _activeStreamingSettings;

    //Rooted for the lifetime of a stream so the garbage collector cannot
    //  reclaim the native thunk between polls. Passing a method group directly
    //  to the P/Invoke would allocate a fresh delegate per call and leave its
    //  lifetime to chance.
    private Ps2000Api.GetOverviewBuffersMaxMin _streamingCallback;

    //Filled by the callback, drained by the polling loop.
    private readonly Dictionary<ChannelId, List<short>> _streamBuffers = new Dictionary<ChannelId, List<short>>();
    private short _streamOverflow;
    private bool _streamTriggered;
    private uint _streamTriggerIndex;
    private bool _streamAutoStopped;

    /// <summary>
    /// Creates a scope bound to the first device the driver finds.
    /// </summary>
    public Ps2000ScopeDataDevice()
    {
        Ps2000Api.Initialise();
    }

    /// <inheritdoc />
    public string Name => IsOpen && Capabilities != null
        ? $"PicoScope {Capabilities.Variant}"
        : "PicoScope (ps2000)";

    /// <inheritdoc />
    public bool IsSimulated => false;

    /// <inheritdoc />
    public bool IsOpen => _handle > 0;

    /// <inheritdoc />
    public ScopeCapabilities Capabilities { get; private set; }

    /// <inheritdoc />
    public UnitInfo UnitInfo { get; private set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<ChannelId, ChannelSettings> Channels => _channels;

    /// <inheritdoc />
    public bool IsStreaming => _streamTask != null && !_streamTask.IsCompleted;

    /// <inheritdoc />
    public event EventHandler<StreamingSamplesEventArgs> SamplesAvailable;

    // --------------------------------------------------------------- lifecycle

    /// <inheritdoc />
    public bool OpenScope()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (IsOpen) { return true; }

        short handle;
        try
        {
            handle = Ps2000Api.ps2000_open_unit();
        }
        catch (DllNotFoundException ex)
        {
            throw new PicoScopeException(
                $"Could not load {Ps2000DriverLoader.DriverFileName}. Searched: " +
                string.Join("; ", Ps2000DriverLoader.GetSearchPaths()) +
                ". Install the PicoScope application or the ps2000 driver package (PicoSDK " +
                "on Windows, libps2000 on Linux), and make sure this process is 64-bit.", ex);
        }
        catch (BadImageFormatException ex)
        {
            throw new PicoScopeException(
                $"{Ps2000DriverLoader.DriverFileName} could not be loaded into this process. " +
                "The installed driver is 64-bit, so the application must be 64-bit too.", ex);
        }

        //0 means no device found, -1 means found but not usable. Neither is an
        //  error worth throwing over -- "nothing plugged in" is ordinary.
        if (handle <= 0) { return false; }

        _handle = handle;

        try
        {
            UnitInfo = ReadUnitInfo();
            Capabilities = DiscoverCapabilities();
            ApplyDefaultChannelSettings();
        }
        catch
        {
            //Never leave a half-open handle behind; it would lock the device.
            Ps2000Api.ps2000_close_unit(_handle);
            _handle = 0;
            throw;
        }

        return true;
    }

    /// <inheritdoc />
    public void CloseScope()
    {
        if (!IsOpen) { return; }

        try
        {
            StopStreaming();
            StopSignalGenerator();
            Ps2000Api.ps2000_stop(_handle);
        }
        catch (Exception)
        {
            //Closing must succeed even if the device has already gone away.
        }
        finally
        {
            Ps2000Api.ps2000_close_unit(_handle);
            _handle = 0;
        }
    }

    /// <inheritdoc />
    public bool Ping() => IsOpen && Ps2000Api.ps2000PingUnit(_handle) != 0;

    /// <inheritdoc />
    public void FlashLed()
    {
        EnsureOpen("flash the LED");
        Ps2000Api.ps2000_flash_led(_handle);
    }

    /// <inheritdoc />
    public int GetLastButtonPress()
    {
        EnsureOpen("read the button");
        return Ps2000Api.ps2000_last_button_press(_handle);
    }

    /// <inheritdoc />
    public string GetUnitInfoLine(UnitInfoLine line)
    {
        EnsureOpen("read unit info");
        return ReadInfoLine((short)line);
    }

    private string ReadInfoLine(short line)
    {
        var text = new StringBuilder(256);
        short written = Ps2000Api.ps2000_get_unit_info(_handle, text, (short)text.Capacity, line);
        return written > 0 ? text.ToString() : string.Empty;
    }

    private UnitInfo ReadUnitInfo() => new UnitInfo(
        Variant: ReadInfoLine((short)UnitInfoLine.VariantInfo),
        BatchAndSerial: ReadInfoLine((short)UnitInfoLine.BatchAndSerial),
        CalibrationDate: ReadInfoLine((short)UnitInfoLine.CalibrationDate),
        DriverVersion: ReadInfoLine((short)UnitInfoLine.DriverVersion),
        HardwareVersion: ReadInfoLine((short)UnitInfoLine.HardwareVersion),
        UsbVersion: ReadInfoLine((short)UnitInfoLine.UsbVersion),
        KernelDriverVersion: ReadInfoLine((short)UnitInfoLine.KernelDriverVersion),
        DriverPath: ReadInfoLine((short)UnitInfoLine.DriverPath),
        ErrorCode: ReadInfoLine((short)UnitInfoLine.ErrorCode));

    // ------------------------------------------------------------ capabilities

    /// <summary>
    /// Works out what this particular unit supports by asking it, rather than by
    /// matching on the model name.
    /// </summary>
    /// <remarks>
    /// Every probe here is a call the device either accepts or rejects, so the
    /// result stays correct if a different 2000-series scope is attached. This
    /// runs once, at open, before the caller has configured anything -- it
    /// leaves the channels in a known state afterwards.
    /// </remarks>
    private ScopeCapabilities DiscoverCapabilities()
    {
        string variant = UnitInfo.Variant;

        //Channels: enable each in turn and see which the device accepts.
        var channels = new List<ChannelId>();
        foreach (ChannelId channel in new[]
                 { ChannelId.ChannelA, ChannelId.ChannelB, ChannelId.ChannelC, ChannelId.ChannelD })
        {
            if (Ps2000Api.ps2000_set_channel(_handle, (short)channel, 1, 1, (short)VoltageRange.Range5V) != 0)
            {
                channels.Add(channel);
            }
        }
        if (channels.Count == 0) { channels.Add(ChannelId.ChannelA); }

        //Ranges: probe on the first channel we know exists.
        var ranges = new List<VoltageRange>();
        short probeChannel = (short)channels[0];
        foreach (VoltageRange range in Enum.GetValues<VoltageRange>())
        {
            if (Ps2000Api.ps2000_set_channel(_handle, probeChannel, 1, 1, (short)range) != 0)
            {
                ranges.Add(range);
            }
        }
        //Leave the probe channel on a sane range rather than the last one tried.
        VoltageRange settled = ranges.Contains(VoltageRange.Range5V) ? VoltageRange.Range5V : ranges[^1];
        Ps2000Api.ps2000_set_channel(_handle, probeChannel, 1, 1, (short)settled);

        //Trigger sources.
        var triggerSources = new List<ChannelId>();
        foreach (ChannelId source in new[]
                 { ChannelId.ChannelA, ChannelId.ChannelB, ChannelId.ChannelC,
                   ChannelId.ChannelD, ChannelId.External, ChannelId.None })
        {
            if (Ps2000Api.ps2000_set_trigger2(_handle, (short)source, 0, 0, 0f, 1000) != 0)
            {
                triggerSources.Add(source);
            }
        }
        Ps2000Api.ps2000_set_trigger2(_handle, (short)ChannelId.None, 0, 0, 0f, 0);

        //Timebases, with every channel enabled (the multi-channel limit).
        foreach (ChannelId channel in channels)
        {
            Ps2000Api.ps2000_set_channel(_handle, (short)channel, 1, 1, (short)settled);
        }
        int minMulti = ProbeMinTimebase(out int maxSamplesMulti);
        int maxTimebase = ProbeMaxTimebase();

        //Now with one channel only, which unlocks the fastest timebase.
        for (int i = 1; i < channels.Count; i++)
        {
            Ps2000Api.ps2000_set_channel(_handle, (short)channels[i], 0, 1, (short)settled);
        }
        int minSingle = ProbeMinTimebase(out int maxSamplesSingle);
        int maxOversample = ProbeMaxOversample(minSingle);

        //ETS.
        int etsInterval = Ps2000Api.ps2000_set_ets(_handle, (short)EtsMode.Fast, 250, 40);
        Ps2000Api.ps2000_set_ets(_handle, (short)EtsMode.Off, 0, 0);

        //Streaming. Start each mode briefly and stop it again.
        bool compatibleStreaming = Ps2000Api.ps2000_run_streaming(_handle, 1, 60000, 0) != 0;
        Ps2000Api.ps2000_stop(_handle);
        bool fastStreaming = Ps2000Api.ps2000_run_streaming_ns(
            _handle, 1, (int)TimeUnits.Microseconds, 100000, 1, 1, 15000) != 0;
        Ps2000Api.ps2000_stop(_handle);

        //Signal generator: which waveforms, and what is the frequency ceiling.
        var waveTypes = new List<WaveType>();
        foreach (WaveType wave in Enum.GetValues<WaveType>())
        {
            if (Ps2000Api.ps2000_set_sig_gen_built_in(
                    _handle, 0, 1_000_000, (int)wave, 1000f, 1000f, 0f, 0f, 0, 0) != 0)
            {
                waveTypes.Add(wave);
            }
        }
        double maxSigGenHz = ProbeMaxSignalGeneratorFrequency(waveTypes.Count > 0);
        uint maxSigGenAmplitude = ProbeMaxSignalGeneratorAmplitude(waveTypes.Count > 0);
        int maxAwgSize = ProbeMaxArbitraryWaveformSize();
        //Silence the generator after probing it.
        Ps2000Api.ps2000_set_sig_gen_built_in(_handle, 0, 0, (int)WaveType.DcVoltage, 0f, 0f, 0f, 0f, 0, 0);

        bool advancedTrigger = Ps2000Api.ps2000SetAdvTriggerChannelDirections(
            _handle, (int)ThresholdDirection.None, (int)ThresholdDirection.None,
            (int)ThresholdDirection.None, (int)ThresholdDirection.None,
            (int)ThresholdDirection.None) != 0;

        bool flashLed = Ps2000Api.ps2000_flash_led(_handle) != 0;
        //flash_led leaves the LED blinking until the next driver call; make one.
        Ps2000Api.ps2000PingUnit(_handle);

        return new ScopeCapabilities
        {
            Variant = variant,
            SupportedChannels = channels,
            SupportedRanges = ranges,
            SupportedTriggerSources = triggerSources,
            SupportedWaveTypes = waveTypes,
            MinTimebaseSingleChannel = minSingle,
            MinTimebaseMultiChannel = minMulti,
            MaxTimebase = maxTimebase,
            MaxSamplesSingleChannel = maxSamplesSingle,
            MaxSamplesMultiChannel = maxSamplesMulti,
            MaxOversample = maxOversample,
            SupportsEts = etsInterval > 0,
            EtsEffectiveIntervalPicoseconds = etsInterval,
            MaxEtsCycles = 250,
            MaxEtsInterleave = 40,
            SupportsCompatibleStreaming = compatibleStreaming,
            SupportsFastStreaming = fastStreaming,
            SupportsSignalGenerator = waveTypes.Count > 0,
            SupportsArbitraryWaveform = maxAwgSize > 0,
            MaxSignalGeneratorFrequencyHz = maxSigGenHz,
            MaxSignalGeneratorAmplitudeMicrovolts = maxSigGenAmplitude,
            MaxArbitraryWaveformSize = maxAwgSize,
            SupportsAdvancedTrigger = advancedTrigger,
            SupportsFlashLed = flashLed,
            SupportsButton = false
        };
    }

    private int ProbeMinTimebase(out int maxSamples)
    {
        for (short tb = 0; tb <= 30; tb++)
        {
            if (Ps2000Api.ps2000_get_timebase(_handle, tb, 1024, out _, out _, 1, out int max) != 0)
            {
                maxSamples = max;
                return tb;
            }
        }
        maxSamples = 0;
        return 0;
    }

    private int ProbeMaxTimebase()
    {
        int highest = 0;
        for (short tb = 0; tb <= 40; tb++)
        {
            if (Ps2000Api.ps2000_get_timebase(_handle, tb, 1024, out _, out _, 1, out _) != 0)
            {
                highest = tb;
            }
        }
        return highest;
    }

    private int ProbeMaxOversample(int timebase)
    {
        int best = 1;
        for (short os = 1; os <= 256; os *= 2)
        {
            if (Ps2000Api.ps2000_get_timebase(_handle, (short)timebase, 1024, out _, out _, os, out _) != 0)
            {
                best = os;
            }
        }
        return best;
    }

    private double ProbeMaxSignalGeneratorFrequency(bool hasGenerator)
    {
        if (!hasGenerator) { return 0; }

        double best = 0;
        foreach (double hz in new[] { 1.0, 100.0, 1000.0, 10000.0, 100000.0, 200000.0, 1000000.0 })
        {
            if (Ps2000Api.ps2000_set_sig_gen_built_in(
                    _handle, 0, 1_000_000, (int)WaveType.Sine, (float)hz, (float)hz, 0f, 0f, 0, 0) != 0)
            {
                best = hz;
            }
        }
        return best;
    }

    private uint ProbeMaxSignalGeneratorAmplitude(bool hasGenerator)
    {
        if (!hasGenerator) { return 0; }

        uint best = 0;
        foreach (uint microvolts in new uint[]
                 { 500_000, 1_000_000, 2_000_000, 4_000_000, 8_000_000, 20_000_000 })
        {
            if (Ps2000Api.ps2000_set_sig_gen_built_in(
                    _handle, 0, microvolts, (int)WaveType.Sine, 1000f, 1000f, 0f, 0f, 0, 0) != 0)
            {
                best = microvolts;
            }
        }
        return best;
    }

    private int ProbeMaxArbitraryWaveformSize()
    {
        int best = 0;
        foreach (int size in new[] { 1024, 2048, 4096, 8192 })
        {
            var wave = new byte[size];
            for (int i = 0; i < size; i++) { wave[i] = (byte)(i % 256); }

            uint delta = ArbitraryWaveformSettings.FrequencyToDeltaPhase(1000.0, size);
            if (Ps2000Api.ps2000_set_sig_gen_arbitrary(
                    _handle, 0, 1_000_000, delta, delta, 0, 0, wave, size, 0, 0) != 0)
            {
                best = size;
            }
        }
        return best;
    }

    private void ApplyDefaultChannelSettings()
    {
        _channels.Clear();
        VoltageRange range = Capabilities.Supports(VoltageRange.Range5V)
            ? VoltageRange.Range5V
            : Capabilities.SupportedRanges[^1];

        foreach (ChannelId channel in Capabilities.SupportedChannels)
        {
            var settings = new ChannelSettings(true, Coupling.Dc, range);
            _channels[channel] = settings;
            Ps2000Api.ps2000_set_channel(_handle, (short)channel, 1, 1, (short)range);
        }
    }

    // ---------------------------------------------------------------- channels

    /// <inheritdoc />
    public void SetChannel(ChannelId channel, ChannelSettings settings)
    {
        EnsureOpen("set a channel");

        if (!Capabilities.Supports(channel))
        {
            throw new ScopeCapabilityException(
                $"Channel {channel} is not present on a {Capabilities.Variant}.");
        }
        if (!Capabilities.Supports(settings.Range))
        {
            throw new ScopeCapabilityException(
                $"Range {settings.Range.ToDisplayString()} is not supported by a {Capabilities.Variant}.");
        }

        short rc = Ps2000Api.ps2000_set_channel(
            _handle,
            (short)channel,
            (short)(settings.Enabled ? 1 : 0),
            (short)(settings.Coupling == Coupling.Dc ? 1 : 0),
            (short)settings.Range);

        if (rc == 0) { throw Failure($"configure channel {channel}", "ps2000_set_channel"); }

        _channels[channel] = settings;
    }

    /// <inheritdoc />
    public IReadOnlyList<ChannelId> GetEnabledChannels()
        => _channels.Where(c => c.Value.Enabled).Select(c => c.Key).OrderBy(c => (int)c).ToArray();

    // ---------------------------------------------------------------- timebase

    /// <inheritdoc />
    public TimebaseInfo GetTimebase(int timebase, int sampleCount, int oversample = 1)
    {
        EnsureOpen("query a timebase");

        //Success here is 1, not 0. This is the inverse of the PICO_STATUS
        //  convention used by every newer Pico driver.
        short rc = Ps2000Api.ps2000_get_timebase(
            _handle, (short)timebase, sampleCount,
            out int intervalNs, out short units, (short)oversample, out int maxSamples);

        if (rc == 0) { return TimebaseInfo.Invalid(timebase, oversample); }

        //intervalNs really is nanoseconds, whatever `units` says -- `units`
        //  describes the timestamp array, not this value.
        return new TimebaseInfo(timebase, true, intervalNs, (TimeUnits)units, maxSamples, oversample);
    }

    /// <inheritdoc />
    public TimebaseInfo FindTimebase(double desiredIntervalNanoseconds, int sampleCount, int oversample = 1)
    {
        EnsureOpen("find a timebase");

        int enabled = GetEnabledChannels().Count;
        for (int tb = Capabilities.MinTimebaseFor(enabled); tb <= Capabilities.MaxTimebase; tb++)
        {
            TimebaseInfo info = GetTimebase(tb, sampleCount, oversample);
            if (info.IsValid && info.IntervalNanoseconds >= desiredIntervalNanoseconds)
            {
                return info;
            }
        }
        return TimebaseInfo.Invalid(Capabilities.MaxTimebase, oversample);
    }

    // ----------------------------------------------------------------- trigger

    /// <inheritdoc />
    public void SetTrigger(TriggerSettings trigger)
    {
        EnsureOpen("set the trigger");

        if (!Capabilities.SupportedTriggerSources.Contains(trigger.Source))
        {
            throw new ScopeCapabilityException(
                $"{trigger.Source} cannot be used as a trigger source on a {Capabilities.Variant}.");
        }

        short rc = Ps2000Api.ps2000_set_trigger2(
            _handle, (short)trigger.Source, trigger.ThresholdAdc, (short)trigger.Direction,
            trigger.DelaySamples, trigger.AutoTriggerMilliseconds);

        if (rc == 0) { throw Failure("set the trigger", "ps2000_set_trigger2"); }
    }

    /// <inheritdoc />
    public void DisableTrigger()
    {
        EnsureOpen("disable the trigger");
        Ps2000Api.ps2000_set_trigger2(_handle, (short)ChannelId.None, 0, 0, 0f, 0);
    }

    /// <inheritdoc />
    public void SetAdvancedTrigger(AdvancedTriggerSettings settings)
    {
        EnsureOpen("set an advanced trigger");
        if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
        if (!Capabilities.SupportsAdvancedTrigger)
        {
            throw new ScopeCapabilityException(
                $"A {Capabilities.Variant} does not support advanced triggering.");
        }

        if (settings.ChannelProperties.Length > 0)
        {
            Ps2000TriggerChannelProperties[] native = settings.ChannelProperties
                .Select(p => new Ps2000TriggerChannelProperties
                {
                    ThresholdMajor = p.ThresholdMajor,
                    ThresholdMinor = p.ThresholdMinor,
                    Hysteresis = p.Hysteresis,
                    Channel = (short)p.Channel,
                    ThresholdMode = (int)p.Mode
                })
                .ToArray();

            if (Ps2000Api.ps2000SetAdvTriggerChannelProperties(
                    _handle, native, (short)native.Length, settings.AutoTriggerMilliseconds) == 0)
            {
                throw Failure("set advanced trigger properties", "ps2000SetAdvTriggerChannelProperties");
            }
        }

        var conditions = new[]
        {
            new Ps2000TriggerConditions
            {
                ChannelA = (int)settings.ChannelAState,
                ChannelB = (int)settings.ChannelBState,
                ChannelC = (int)TriggerState.DontCare,
                ChannelD = (int)TriggerState.DontCare,
                External = (int)TriggerState.DontCare,
                PulseWidthQualifier = (int)settings.PulseWidthQualifierState
            }
        };

        if (Ps2000Api.ps2000SetAdvTriggerChannelConditions(_handle, conditions, 1) == 0)
        {
            throw Failure("set advanced trigger conditions", "ps2000SetAdvTriggerChannelConditions");
        }

        Ps2000Api.ps2000SetAdvTriggerChannelDirections(
            _handle,
            (int)settings.ChannelADirection,
            (int)settings.ChannelBDirection,
            (int)ThresholdDirection.None,
            (int)ThresholdDirection.None,
            (int)ThresholdDirection.None);

        Ps2000Api.ps2000SetAdvTriggerDelay(_handle, settings.Delay, settings.PreTriggerDelayPercent);
    }

    /// <inheritdoc />
    public void SetPulseWidthQualifier(PulseWidthQualifier qualifier)
    {
        EnsureOpen("set a pulse-width qualifier");

        var conditions = new[]
        {
            new Ps2000PwqConditions
            {
                ChannelA = (int)qualifier.ChannelAState,
                ChannelB = (int)qualifier.ChannelBState,
                ChannelC = (int)TriggerState.DontCare,
                ChannelD = (int)TriggerState.DontCare,
                External = (int)TriggerState.DontCare
            }
        };

        if (Ps2000Api.ps2000SetPulseWidthQualifier(
                _handle, conditions, 1, (int)qualifier.Direction,
                qualifier.LowerSamples, qualifier.UpperSamples, (int)qualifier.Type) == 0)
        {
            throw Failure("set the pulse-width qualifier", "ps2000SetPulseWidthQualifier");
        }
    }

    // --------------------------------------------------------------------- ETS

    /// <inheritdoc />
    public EtsResult SetEts(EtsMode mode, int cycles, int interleave)
    {
        EnsureOpen("configure ETS");

        int intervalPs = Ps2000Api.ps2000_set_ets(_handle, (short)mode, (short)cycles, (short)interleave);
        return new EtsResult(mode, cycles, interleave, intervalPs);
    }

    // -------------------------------------------------------------- block mode

    /// <inheritdoc />
    public async Task<CaptureBlock> RunBlockAsync(
        int sampleCount,
        int timebase,
        int oversample = 1,
        bool includeTimestamps = false,
        CancellationToken cancellationToken = default)
    {
        EnsureOpen("run a block capture");

        TimebaseInfo info = GetTimebase(timebase, sampleCount, oversample);
        if (!info.IsValid)
        {
            throw new PicoScopeException(
                $"Timebase {timebase} is not valid with {GetEnabledChannels().Count} channel(s) " +
                $"enabled and oversample {oversample}.");
        }
        if (sampleCount > info.MaxSamples)
        {
            throw new PicoScopeException(
                $"{sampleCount} samples exceeds the {info.MaxSamples}-sample buffer at timebase {timebase}.");
        }

        if (Ps2000Api.ps2000_run_block(
                _handle, sampleCount, (short)timebase, (short)oversample, out int indisposedMs) == 0)
        {
            throw Failure("start the block capture", "ps2000_run_block");
        }

        //Poll for completion. The driver has no completion event for block mode,
        //  so polling is the documented approach; sleep between polls rather
        //  than spinning.
        try
        {
            while (Ps2000Api.ps2000_ready(_handle) == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Ps2000Api.ps2000_stop(_handle);
            throw;
        }

        IReadOnlyList<ChannelId> enabled = GetEnabledChannels();
        short[] bufferA = enabled.Contains(ChannelId.ChannelA) ? new short[sampleCount] : null;
        short[] bufferB = enabled.Contains(ChannelId.ChannelB) ? new short[sampleCount] : null;
        short[] bufferC = enabled.Contains(ChannelId.ChannelC) ? new short[sampleCount] : null;
        short[] bufferD = enabled.Contains(ChannelId.ChannelD) ? new short[sampleCount] : null;

        int[] timestamps = includeTimestamps ? new int[sampleCount] : null;
        short overflow;
        int collected;

        if (includeTimestamps)
        {
            collected = Ps2000Api.ps2000_get_times_and_values(
                _handle, timestamps, bufferA, bufferB, bufferC, bufferD,
                out overflow, (short)info.TimestampUnits, sampleCount);
        }
        else
        {
            collected = Ps2000Api.ps2000_get_values(
                _handle, bufferA, bufferB, bufferC, bufferD, out overflow, sampleCount);
        }

        Ps2000Api.ps2000_stop(_handle);

        if (collected == 0) { throw Failure("read the captured block", "ps2000_get_values"); }

        var channels = new Dictionary<ChannelId, ChannelSamples>();
        AddChannel(channels, ChannelId.ChannelA, bufferA, collected);
        AddChannel(channels, ChannelId.ChannelB, bufferB, collected);
        AddChannel(channels, ChannelId.ChannelC, bufferC, collected);
        AddChannel(channels, ChannelId.ChannelD, bufferD, collected);

        return new CaptureBlock
        {
            Channels = channels,
            IntervalNanoseconds = info.IntervalNanoseconds,
            SampleCount = collected,
            Timestamps = timestamps,
            TimestampUnits = info.TimestampUnits,
            OverflowFlags = overflow,
            CapturedAtUtc = DateTime.UtcNow
        };
    }

    private void AddChannel(
        IDictionary<ChannelId, ChannelSamples> target, ChannelId channel, short[] buffer, int count)
    {
        if (buffer == null) { return; }

        short[] trimmed = buffer.Length == count ? buffer : buffer[..count];
        target[channel] = new ChannelSamples(channel, _channels[channel].Range, trimmed);
    }

    /// <inheritdoc />
    public void Stop()
    {
        StopStreaming();
        if (IsOpen) { Ps2000Api.ps2000_stop(_handle); }
    }

    // --------------------------------------------------------------- streaming

    /// <inheritdoc />
    public void StartStreaming(StreamingSettings settings)
    {
        EnsureOpen("start streaming");
        if (settings == null) { throw new ArgumentNullException(nameof(settings)); }

        if (settings.Mode == StreamingMode.Fast && !Capabilities.SupportsFastStreaming)
        {
            throw new ScopeCapabilityException($"A {Capabilities.Variant} does not support fast streaming.");
        }
        if (settings.Mode == StreamingMode.Compatible && !Capabilities.SupportsCompatibleStreaming)
        {
            throw new ScopeCapabilityException($"A {Capabilities.Variant} does not support compatible streaming.");
        }

        StopStreaming();

        IReadOnlyList<ChannelId> enabled = GetEnabledChannels();
        _streamChannelCount = enabled.Count;
        _streamIntervalNs = settings.IntervalInNanoseconds();
        _totalStreamedSamples = 0;
        _streamAutoStopped = false;

        lock (_syncRoot)
        {
            _streamBuffers.Clear();
            foreach (ChannelId channel in enabled) { _streamBuffers[channel] = new List<short>(); }
        }

        short started = settings.Mode == StreamingMode.Fast
            ? Ps2000Api.ps2000_run_streaming_ns(
                _handle, settings.SampleInterval, (int)settings.IntervalUnits,
                settings.MaxSamples, (short)(settings.AutoStop ? 1 : 0),
                settings.SamplesPerAggregate, settings.OverviewBufferSize)
            : Ps2000Api.ps2000_run_streaming(
                _handle, (short)Math.Max(1, settings.SampleInterval),
                (int)Math.Min(settings.MaxSamples, 60000), 0);

        if (started == 0) { throw Failure("start streaming", "ps2000_run_streaming_ns"); }

        _activeStreamingSettings = settings;

        //Root the delegate for the lifetime of the stream. Binding a method
        //  group to a delegate whose signature contains pointers needs an
        //  unsafe context, even though no pointer is dereferenced here.
        unsafe { _streamingCallback = StreamingCallback; }

        _streamCancellation = new CancellationTokenSource();
        CancellationToken token = _streamCancellation.Token;
        _streamTask = Task.Run(() => StreamLoop(settings, token), token);
    }

    /// <inheritdoc />
    public void StopStreaming()
    {
        CancellationTokenSource cts;
        Task task;
        lock (_syncRoot)
        {
            cts = _streamCancellation;
            task = _streamTask;
            _streamCancellation = null;
            _streamTask = null;
        }

        if (cts == null) { return; }

        try
        {
            cts.Cancel();
            task?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            //Cancellation surfaces as an exception on the task; expected.
        }
        finally
        {
            cts.Dispose();
            _streamingCallback = null;
            if (IsOpen) { Ps2000Api.ps2000_stop(_handle); }
        }
    }

    /// <summary>
    /// Receives a batch from the driver. Runs on the polling thread, inside the
    /// <c>ps2000_get_streaming_last_values</c> call.
    /// </summary>
    private unsafe void StreamingCallback(
        short** overviewBuffers, short overflow, uint triggeredAt,
        short triggered, short autoStop, uint nValues)
    {
        if (nValues == 0) { return; }

        _streamOverflow = overflow;
        _streamTriggered = triggered != 0;
        _streamTriggerIndex = triggeredAt;
        if (autoStop != 0) { _streamAutoStopped = true; }

        lock (_syncRoot)
        {
            int channelIndex = 0;
            foreach (ChannelId channel in _streamBuffers.Keys.ToArray())
            {
                //Two buffers per channel: maxima then minima. With no
                //  aggregation the two are identical, so the maximum is taken.
                short* source = overviewBuffers[channelIndex * 2];
                if (source == null) { channelIndex++; continue; }

                List<short> target = _streamBuffers[channel];
                for (uint i = 0; i < nValues; i++) { target.Add(source[i]); }
                channelIndex++;
            }
        }
    }

    private void StreamLoop(StreamingSettings settings, CancellationToken token)
    {
        int pollMs = Math.Max(1, settings.PollIntervalMilliseconds);

        while (!token.IsCancellationRequested && !_streamAutoStopped)
        {
            Ps2000Api.ps2000_get_streaming_last_values(_handle, _streamingCallback);

            short overrunFlag = 0;
            Ps2000Api.ps2000_overview_buffer_status(_handle, out overrunFlag);

            Dictionary<ChannelId, ChannelSamples> batch = null;
            int batchSize = 0;

            lock (_syncRoot)
            {
                batchSize = _streamBuffers.Count > 0 ? _streamBuffers.Values.Max(v => v.Count) : 0;
                if (batchSize > 0)
                {
                    batch = new Dictionary<ChannelId, ChannelSamples>();
                    foreach (KeyValuePair<ChannelId, List<short>> entry in _streamBuffers)
                    {
                        batch[entry.Key] = new ChannelSamples(
                            entry.Key, _channels[entry.Key].Range, entry.Value.ToArray());
                        entry.Value.Clear();
                    }
                }
            }

            if (batch != null)
            {
                _totalStreamedSamples += batchSize;
                SamplesAvailable?.Invoke(this, new StreamingSamplesEventArgs
                {
                    Channels = batch,
                    SampleCount = batchSize,
                    TotalSampleCount = _totalStreamedSamples,
                    IntervalNanoseconds = _streamIntervalNs,
                    Triggered = _streamTriggered,
                    TriggerIndex = _streamTriggerIndex,
                    OverflowFlags = _streamOverflow,
                    BufferOverrun = overrunFlag != 0
                });
                _streamTriggered = false;
            }

            try
            {
                Task.Delay(pollMs, token).Wait(token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    // -------------------------------------------------------- signal generator

    /// <summary>
    /// Runs <paramref name="action"/> with any acquisition stopped, then puts the
    /// acquisition back as it was.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The device refuses every signal-generator call while an acquisition is in
    /// progress. Verified on a 2204A: <c>ps2000_set_sig_gen_built_in</c> returns
    /// 0 during both fast streaming and an in-flight block capture, and succeeds
    /// the moment <c>ps2000_stop</c> has been called. The driver reports no
    /// reason -- unit-info line 6 still reads "0" -- so the failure is silent
    /// and looks like a bad argument.
    /// </para>
    /// <para>
    /// Every caller would otherwise have to perform this dance itself, and get
    /// the restart right, so it is done here. The running sample total is carried
    /// across the restart, since from the caller's point of view the stream did
    /// not end.
    /// </para>
    /// </remarks>
    private void WithAcquisitionPaused(Action action)
    {
        StreamingSettings resume = IsStreaming ? _activeStreamingSettings : null;
        long carriedTotal = _totalStreamedSamples;

        if (resume != null) { StopStreaming(); }
        else { Ps2000Api.ps2000_stop(_handle); }

        try
        {
            action();
        }
        finally
        {
            if (resume != null)
            {
                StartStreaming(resume);
                _totalStreamedSamples = carriedTotal;
            }
        }
    }

    /// <inheritdoc />
    public void SetSignalGenerator(SignalGeneratorSettings settings)
    {
        EnsureOpen("set the signal generator");

        if (!Capabilities.SupportsSignalGenerator)
        {
            throw new ScopeCapabilityException($"A {Capabilities.Variant} has no signal generator.");
        }
        if (settings.StartFrequencyHz > Capabilities.MaxSignalGeneratorFrequencyHz
            || settings.StopFrequencyHz > Capabilities.MaxSignalGeneratorFrequencyHz)
        {
            throw new ScopeCapabilityException(
                $"The signal generator on a {Capabilities.Variant} tops out at " +
                $"{Capabilities.MaxSignalGeneratorFrequencyHz} Hz.");
        }
        if (settings.PeakToPeakMicrovolts > Capabilities.MaxSignalGeneratorAmplitudeMicrovolts)
        {
            throw new ScopeCapabilityException(
                $"The signal generator on a {Capabilities.Variant} tops out at " +
                $"{Capabilities.MaxSignalGeneratorAmplitudeMicrovolts / 1_000_000.0:0.#} V peak-to-peak.");
        }

        WithAcquisitionPaused(() =>
        {
            short rc = Ps2000Api.ps2000_set_sig_gen_built_in(
                _handle,
                settings.OffsetMicrovolts,
                settings.PeakToPeakMicrovolts,
                (int)settings.WaveType,
                (float)settings.StartFrequencyHz,
                (float)settings.StopFrequencyHz,
                (float)settings.IncrementHz,
                (float)settings.DwellTimeSeconds,
                (int)settings.SweepType,
                settings.SweepCount);

            if (rc == 0) { throw Failure("configure the signal generator", "ps2000_set_sig_gen_built_in"); }
        });
    }

    /// <inheritdoc />
    public void SetArbitraryWaveform(ArbitraryWaveformSettings settings)
    {
        EnsureOpen("set an arbitrary waveform");

        if (!Capabilities.SupportsArbitraryWaveform)
        {
            throw new ScopeCapabilityException($"A {Capabilities.Variant} has no arbitrary waveform generator.");
        }
        if (settings.Waveform == null || settings.Waveform.Length == 0)
        {
            throw new ArgumentException("The waveform must contain at least one sample.", nameof(settings));
        }
        if (settings.Waveform.Length > Capabilities.MaxArbitraryWaveformSize)
        {
            throw new ScopeCapabilityException(
                $"The AWG buffer on a {Capabilities.Variant} holds at most " +
                $"{Capabilities.MaxArbitraryWaveformSize} samples.");
        }
        if (settings.PeakToPeakMicrovolts > Capabilities.MaxSignalGeneratorAmplitudeMicrovolts)
        {
            throw new ScopeCapabilityException(
                $"The signal generator on a {Capabilities.Variant} tops out at " +
                $"{Capabilities.MaxSignalGeneratorAmplitudeMicrovolts / 1_000_000.0:0.#} V peak-to-peak.");
        }

        WithAcquisitionPaused(() =>
        {
            short rc = Ps2000Api.ps2000_set_sig_gen_arbitrary(
                _handle,
                settings.OffsetMicrovolts,
                settings.PeakToPeakMicrovolts,
                settings.StartDeltaPhase,
                settings.StopDeltaPhase,
                settings.DeltaPhaseIncrement,
                settings.DwellCount,
                settings.Waveform,
                settings.Waveform.Length,
                (int)settings.SweepType,
                settings.SweepCount);

            if (rc == 0) { throw Failure("load the arbitrary waveform", "ps2000_set_sig_gen_arbitrary"); }
        });
    }

    /// <inheritdoc />
    public void StopSignalGenerator()
    {
        if (!IsOpen) { return; }

        //Silencing the generator is a generator call like any other, so it is
        //  refused mid-acquisition too -- but this one runs on the shutdown path
        //  where throwing would be unhelpful, so a failure is tolerated.
        WithAcquisitionPaused(() =>
            Ps2000Api.ps2000_set_sig_gen_built_in(
                _handle, 0, 0, (int)WaveType.DcVoltage, 0f, 0f, 0f, 0f, 0, 0));
    }

    // ----------------------------------------------------------------- helpers

    /// <summary>
    /// Builds an exception for a failed driver call, enriching it with the
    /// device error code -- the only diagnostic this API offers.
    /// </summary>
    private PicoScopeException Failure(string what, string driverFunction)
    {
        string deviceError = string.Empty;
        try
        {
            deviceError = ReadInfoLine((short)UnitInfoLine.ErrorCode);
        }
        catch (Exception)
        {
            //If even the error code cannot be read, report without it.
        }
        return new PicoScopeException($"Failed to {what}.", driverFunction, deviceError);
    }

    private void EnsureOpen(string operation)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!IsOpen) { throw new ScopeNotOpenException(operation); }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;
        CloseScope();
    }
}
