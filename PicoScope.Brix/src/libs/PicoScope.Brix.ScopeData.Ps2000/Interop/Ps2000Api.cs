using System.Runtime.InteropServices;
using System.Text;

namespace PicoScope.Brix.ScopeData.Ps2000.Interop;

/// <summary>
/// The complete ps2000 driver API, as declared in <c>ps2000.h</c>.
/// </summary>
/// <remarks>
/// <para>
/// Every entry point the driver exports is declared here, so this file can serve
/// as the reference for what the API offers. The names match the C names exactly
/// rather than being tidied up, so they can be looked up in the Programmer's
/// Guide without a translation step.
/// </para>
/// <para>
/// <b>Return-value convention.</b> Most of these return <c>int16_t</c>, where
/// <b>zero means failure</b> and any non-zero value means success. That is the
/// opposite of the <c>PICO_STATUS</c> convention every newer Pico driver uses,
/// where zero (<c>PICO_OK</c>) means success. Mixing the two up is the single
/// easiest mistake to make here. The exceptions are noted per function:
/// <see cref="ps2000_open_unit"/> returns a handle, the get-values functions
/// return sample counts, and <see cref="ps2000_set_ets"/> returns an interval.
/// </para>
/// <para>
/// <b>One declaration, every operating system.</b> The library is named by its
/// bare name, <c>ps2000</c>, so the runtime binds to <c>ps2000.dll</c> on
/// Windows and <c>libps2000.so</c> on Linux. The entry points, signatures and
/// behaviours are identical -- both are built from the same <c>ps2000.h</c>
/// -- so nothing in this file is platform-specific. Finding the library is
/// <see cref="Ps2000DriverLoader"/>'s job.
/// </para>
/// <para>
/// <b>Calling convention.</b> The header declares these <c>__stdcall</c> on
/// Win32, which matters only for 32-bit code -- x64 has a single calling
/// convention on each operating system, so the default marshalling is correct.
/// </para>
/// <para>
/// <b>Bitness.</b> These bind to a 64-bit driver; the host process must be
/// 64-bit.
/// </para>
/// </remarks>
public static class Ps2000Api
{
    private const string Driver = Ps2000DriverLoader.LibraryName;

    /// <summary>
    /// Prepares the driver for use. Call before the first P/Invoke.
    /// </summary>
    public static void Initialise() => Ps2000DriverLoader.EnsureRegistered();

    /// <summary>
    /// Receives batches of samples during fast streaming.
    /// </summary>
    /// <param name="overviewBuffers">
    /// An array of pointers to the driver's internal buffers, two per channel
    /// (maximum then minimum). Channel A's maximum buffer is index 0, its
    /// minimum index 1, channel B's maximum index 2, and so on.
    /// </param>
    /// <param name="overflow">Per-channel overflow flags for this batch.</param>
    /// <param name="triggeredAt">The index within the batch at which a trigger fired.</param>
    /// <param name="triggered">Non-zero when a trigger fired within this batch.</param>
    /// <param name="autoStop">Non-zero when the device has stopped by itself.</param>
    /// <param name="nValues">The number of samples in this batch.</param>
    /// <remarks>
    /// The driver invokes this synchronously from inside
    /// <see cref="ps2000_get_streaming_last_values"/> and the pointers are valid
    /// only for the duration of the call, so the data must be copied out before
    /// returning. The delegate must also be kept alive by managed code for as
    /// long as it might be called -- see the field
    /// <c>Ps2000ScopeDataDevice._streamingCallback</c>, which exists solely to hold a
    /// reference the garbage collector can see.
    /// </remarks>
    public unsafe delegate void GetOverviewBuffersMaxMin(
        short** overviewBuffers,
        short overflow,
        uint triggeredAt,
        short triggered,
        short autoStop,
        uint nValues);

    // ------------------------------------------------------------- lifecycle

    /// <summary>
    /// Opens the first available scope.
    /// </summary>
    /// <returns>
    /// A positive handle on success; 0 when no device was found; -1 when a
    /// device was found but could not be opened.
    /// </returns>
    /// <remarks>
    /// Unlike most of this API, the return value is a handle rather than a
    /// success flag. Only one process may hold a device open, so this fails
    /// while the PicoScope desktop application is running.
    /// </remarks>
    [DllImport(Driver, EntryPoint = "ps2000_open_unit")]
    public static extern short ps2000_open_unit();

    /// <summary>Begins opening a scope without blocking.</summary>
    /// <returns>Non-zero when the open began; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_open_unit_async")]
    public static extern short ps2000_open_unit_async();

    /// <summary>Polls the progress of an asynchronous open.</summary>
    /// <param name="handle">Receives the handle once the open completes.</param>
    /// <param name="progressPercent">Receives progress from 0 to 100.</param>
    /// <returns>1 when complete, 0 while pending, -1 on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_open_unit_progress")]
    public static extern short ps2000_open_unit_progress(out short handle, out short progressPercent);

    /// <summary>Closes a scope and releases the device.</summary>
    /// <param name="handle">The device handle.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_close_unit")]
    public static extern short ps2000_close_unit(short handle);

    /// <summary>Reads one line of device information.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="text">Receives the text.</param>
    /// <param name="textLength">The capacity of <paramref name="text"/>.</param>
    /// <param name="line">Which line to read; see <c>PS2000_INFO</c>.</param>
    /// <returns>The number of characters written; zero on failure.</returns>
    /// <remarks>
    /// Line 6 is the device's error code, and is the only diagnostic this API
    /// offers when another call fails.
    /// </remarks>
    [DllImport(Driver, EntryPoint = "ps2000_get_unit_info")]
    public static extern short ps2000_get_unit_info(short handle, StringBuilder text, short textLength, short line);

    /// <summary>Checks that the device is still connected and responding.</summary>
    /// <param name="handle">The device handle.</param>
    /// <returns>Non-zero when the device answered.</returns>
    [DllImport(Driver, EntryPoint = "ps2000PingUnit")]
    public static extern short ps2000PingUnit(short handle);

    /// <summary>
    /// Flashes the device LED until the next call to any other driver function.
    /// </summary>
    /// <param name="handle">The device handle.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_flash_led")]
    public static extern short ps2000_flash_led(short handle);

    /// <summary>
    /// Sets the LED state. Not supported by a PicoScope 2204A, which returns zero.
    /// </summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="state">The state to set.</param>
    /// <returns>Non-zero on success; zero when unsupported.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_set_led")]
    public static extern short ps2000_set_led(short handle, short state);

    /// <summary>
    /// Sets the built-in light. Not supported by a PicoScope 2204A, which returns zero.
    /// </summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="state">The state to set.</param>
    /// <returns>Non-zero on success; zero when unsupported.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_set_light")]
    public static extern short ps2000_set_light(short handle, short state);

    /// <summary>
    /// Reads the front-panel button state on models that have one.
    /// </summary>
    /// <param name="handle">The device handle.</param>
    /// <returns>0 no press, 1 short press, 2 long press.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_last_button_press")]
    public static extern short ps2000_last_button_press(short handle);

    // ----------------------------------------------------------------- setup

    /// <summary>Configures an input channel.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="channel">The channel ordinal.</param>
    /// <param name="enabled">Non-zero to enable the channel.</param>
    /// <param name="dc">Non-zero for DC coupling, zero for AC.</param>
    /// <param name="range">The range ordinal.</param>
    /// <returns>Non-zero on success; zero when the channel or range is unsupported.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_set_channel")]
    public static extern short ps2000_set_channel(short handle, short channel, short enabled, short dc, short range);

    /// <summary>Queries what a timebase means for the current configuration.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="timebase">The timebase index.</param>
    /// <param name="noOfSamples">The intended sample count.</param>
    /// <param name="timeInterval">
    /// Receives the sampling interval <b>in nanoseconds</b>, regardless of what
    /// <paramref name="timeUnits"/> says.
    /// </param>
    /// <param name="timeUnits">
    /// Receives the units of the timestamp array that
    /// <see cref="ps2000_get_times_and_values"/> will fill -- <b>not</b> the
    /// units of <paramref name="timeInterval"/>.
    /// </param>
    /// <param name="oversample">The oversampling factor.</param>
    /// <param name="maxSamples">Receives the largest block size available.</param>
    /// <returns>
    /// <b>1 on success, 0 on failure</b> -- note this is the opposite sense to
    /// the <c>PICO_STATUS</c> convention of the newer drivers.
    /// </returns>
    /// <remarks>
    /// The split between <paramref name="timeInterval"/> and
    /// <paramref name="timeUnits"/> is the most misread part of this API.
    /// Measured on a 2204A, timebase 18 reports an interval of 2621440 with
    /// units of microseconds; the capture actually runs at 2621440 nanoseconds
    /// per sample. The driver chooses the finest timestamp unit in which the
    /// whole capture fits a 32-bit integer, which is why the reported unit
    /// changes with the timebase.
    /// </remarks>
    [DllImport(Driver, EntryPoint = "ps2000_get_timebase")]
    public static extern short ps2000_get_timebase(
        short handle, short timebase, int noOfSamples,
        out int timeInterval, out short timeUnits, short oversample, out int maxSamples);

    // --------------------------------------------------------------- trigger

    /// <summary>Sets a simple trigger with an integer delay.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="source">The trigger source ordinal.</param>
    /// <param name="threshold">The trigger level as an ADC count.</param>
    /// <param name="direction">0 rising, 1 falling.</param>
    /// <param name="delay">The trigger position as a percentage of the block, -100 to 100.</param>
    /// <param name="autoTriggerMs">Auto-trigger timeout; zero waits forever.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_set_trigger")]
    public static extern short ps2000_set_trigger(
        short handle, short source, short threshold, short direction, short delay, short autoTriggerMs);

    /// <summary>
    /// Sets a simple trigger with a fractional delay. Prefer this over
    /// <see cref="ps2000_set_trigger"/>; it is the same call with finer
    /// positioning.
    /// </summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="source">The trigger source ordinal.</param>
    /// <param name="threshold">The trigger level as an ADC count.</param>
    /// <param name="direction">0 rising, 1 falling.</param>
    /// <param name="delay">The trigger position as a percentage of the block, -100 to 100.</param>
    /// <param name="autoTriggerMs">Auto-trigger timeout; zero waits forever.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_set_trigger2")]
    public static extern short ps2000_set_trigger2(
        short handle, short source, short threshold, short direction, float delay, short autoTriggerMs);

    /// <summary>Sets per-channel threshold properties for the advanced trigger.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="channelProperties">The per-channel properties.</param>
    /// <param name="nChannelProperties">How many entries the array holds.</param>
    /// <param name="autoTriggerMilliseconds">Auto-trigger timeout; zero waits forever.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000SetAdvTriggerChannelProperties")]
    public static extern short ps2000SetAdvTriggerChannelProperties(
        short handle, Ps2000TriggerChannelProperties[] channelProperties,
        short nChannelProperties, int autoTriggerMilliseconds);

    /// <summary>Sets the logical condition combining the advanced trigger channels.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="conditions">The conditions.</param>
    /// <param name="nConditions">How many entries the array holds.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000SetAdvTriggerChannelConditions")]
    public static extern short ps2000SetAdvTriggerChannelConditions(
        short handle, Ps2000TriggerConditions[] conditions, short nConditions);

    /// <summary>Sets the threshold direction each channel tests.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="channelA">Channel A's direction.</param>
    /// <param name="channelB">Channel B's direction.</param>
    /// <param name="channelC">Channel C's direction.</param>
    /// <param name="channelD">Channel D's direction.</param>
    /// <param name="ext">The external input's direction.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000SetAdvTriggerChannelDirections")]
    public static extern short ps2000SetAdvTriggerChannelDirections(
        short handle, int channelA, int channelB, int channelC, int channelD, int ext);

    /// <summary>Sets the advanced trigger's delay and pre-trigger fraction.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="delay">The post-condition delay in samples.</param>
    /// <param name="preTriggerDelay">The fraction of the block captured before the trigger.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000SetAdvTriggerDelay")]
    public static extern short ps2000SetAdvTriggerDelay(short handle, uint delay, float preTriggerDelay);

    /// <summary>Sets a pulse-width qualifier on the advanced trigger.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="conditions">The channel conditions being timed.</param>
    /// <param name="nConditions">How many entries the array holds.</param>
    /// <param name="direction">The threshold direction being timed.</param>
    /// <param name="lower">The lower width bound, in samples.</param>
    /// <param name="upper">The upper width bound, in samples.</param>
    /// <param name="type">The comparison to apply.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000SetPulseWidthQualifier")]
    public static extern short ps2000SetPulseWidthQualifier(
        short handle, Ps2000PwqConditions[] conditions, short nConditions,
        int direction, uint lower, uint upper, int type);

    // ------------------------------------------------------------------- ETS

    /// <summary>Configures equivalent-time sampling.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="mode">0 off, 1 fast, 2 slow.</param>
    /// <param name="etsCycles">How many captures to interleave.</param>
    /// <param name="etsInterleave">How many sub-sample offsets to step through.</param>
    /// <returns>
    /// The effective sampling interval <b>in picoseconds</b>, or zero when the
    /// device does not support ETS. Note this is a value, not a success flag.
    /// </returns>
    [DllImport(Driver, EntryPoint = "ps2000_set_ets")]
    public static extern int ps2000_set_ets(short handle, short mode, short etsCycles, short etsInterleave);

    // ------------------------------------------------------------ block mode

    /// <summary>Starts a block capture.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="noOfValues">How many samples to capture.</param>
    /// <param name="timebase">The timebase index.</param>
    /// <param name="oversample">The oversampling factor.</param>
    /// <param name="timeIndisposedMs">Receives how long the capture will take, in milliseconds.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_run_block")]
    public static extern short ps2000_run_block(
        short handle, int noOfValues, short timebase, short oversample, out int timeIndisposedMs);

    /// <summary>Polls whether a block capture has finished.</summary>
    /// <param name="handle">The device handle.</param>
    /// <returns>Non-zero when data is ready; zero while still capturing.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_ready")]
    public static extern short ps2000_ready(short handle);

    /// <summary>Stops any capture in progress.</summary>
    /// <param name="handle">The device handle.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_stop")]
    public static extern short ps2000_stop(short handle);

    /// <summary>Retrieves the samples from a completed block capture.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="bufferA">Channel A's buffer, or null when not wanted.</param>
    /// <param name="bufferB">Channel B's buffer, or null when not wanted.</param>
    /// <param name="bufferC">Channel C's buffer, or null.</param>
    /// <param name="bufferD">Channel D's buffer, or null.</param>
    /// <param name="overflow">Receives per-channel overflow flags.</param>
    /// <param name="noOfValues">How many samples to retrieve.</param>
    /// <returns>The number of samples actually written, or zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_get_values")]
    public static extern int ps2000_get_values(
        short handle, short[] bufferA, short[] bufferB, short[] bufferC, short[] bufferD,
        out short overflow, int noOfValues);

    /// <summary>Retrieves samples and their timestamps from a completed block capture.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="times">Receives per-sample timestamps in <paramref name="timeUnits"/>.</param>
    /// <param name="bufferA">Channel A's buffer, or null.</param>
    /// <param name="bufferB">Channel B's buffer, or null.</param>
    /// <param name="bufferC">Channel C's buffer, or null.</param>
    /// <param name="bufferD">Channel D's buffer, or null.</param>
    /// <param name="overflow">Receives per-channel overflow flags.</param>
    /// <param name="timeUnits">The units for the timestamps, from <see cref="ps2000_get_timebase"/>.</param>
    /// <param name="noOfValues">How many samples to retrieve.</param>
    /// <returns>The number of samples actually written, or zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_get_times_and_values")]
    public static extern int ps2000_get_times_and_values(
        short handle, int[] times, short[] bufferA, short[] bufferB, short[] bufferC, short[] bufferD,
        out short overflow, short timeUnits, int noOfValues);

    // ------------------------------------------------------------- streaming

    /// <summary>Starts the original polled streaming mode.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="sampleIntervalMs">The interval between samples, in whole milliseconds.</param>
    /// <param name="maxSamples">The buffer size, capped at 60000.</param>
    /// <param name="windowed">Non-zero for a sliding window, zero for a one-shot fill.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_run_streaming")]
    public static extern short ps2000_run_streaming(
        short handle, short sampleIntervalMs, int maxSamples, short windowed);

    /// <summary>Starts fast streaming, which pushes data through a callback.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="sampleInterval">The interval between samples, in <paramref name="timeUnits"/>.</param>
    /// <param name="timeUnits">The units of <paramref name="sampleInterval"/>.</param>
    /// <param name="maxSamples">How many samples to collect before auto-stopping.</param>
    /// <param name="autoStop">Non-zero to stop automatically at <paramref name="maxSamples"/>.</param>
    /// <param name="noOfSamplesPerAggregate">How many raw samples to combine into each reported value.</param>
    /// <param name="overviewBufferSize">The driver's internal buffer size, in samples.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_run_streaming_ns")]
    public static extern short ps2000_run_streaming_ns(
        short handle, uint sampleInterval, int timeUnits, uint maxSamples,
        short autoStop, uint noOfSamplesPerAggregate, uint overviewBufferSize);

    /// <summary>
    /// Collects whatever fast-streaming data has arrived, invoking the callback
    /// synchronously before returning.
    /// </summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="callback">The delegate to receive the data.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    /// <remarks>
    /// Because the callback runs before this returns, the delegate only has to
    /// survive the call -- but it must still be rooted in managed code, or the
    /// collector may reclaim the thunk between polls.
    /// </remarks>
    [DllImport(Driver, EntryPoint = "ps2000_get_streaming_last_values")]
    public static extern short ps2000_get_streaming_last_values(
        short handle, GetOverviewBuffersMaxMin callback);

    /// <summary>Reports whether the driver's overview buffer overran.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="previousBufferOverrun">Non-zero when samples were lost since the last check.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_overview_buffer_status")]
    public static extern short ps2000_overview_buffer_status(short handle, out short previousBufferOverrun);

    /// <summary>Retrieves aggregated streaming values, each with a minimum and maximum.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="startTime">Receives the time of the first sample.</param>
    /// <param name="bufferAMax">Channel A maxima, or null.</param>
    /// <param name="bufferAMin">Channel A minima, or null.</param>
    /// <param name="bufferBMax">Channel B maxima, or null.</param>
    /// <param name="bufferBMin">Channel B minima, or null.</param>
    /// <param name="bufferCMax">Channel C maxima, or null.</param>
    /// <param name="bufferCMin">Channel C minima, or null.</param>
    /// <param name="bufferDMax">Channel D maxima, or null.</param>
    /// <param name="bufferDMin">Channel D minima, or null.</param>
    /// <param name="overflow">Receives per-channel overflow flags.</param>
    /// <param name="triggerAt">Receives the trigger index.</param>
    /// <param name="triggered">Receives non-zero when a trigger fired.</param>
    /// <param name="noOfValues">How many values to retrieve.</param>
    /// <param name="noOfSamplesPerAggregate">How many raw samples each value represents.</param>
    /// <returns>The number of values written.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_get_streaming_values")]
    public static extern uint ps2000_get_streaming_values(
        short handle, out double startTime,
        short[] bufferAMax, short[] bufferAMin, short[] bufferBMax, short[] bufferBMin,
        short[] bufferCMax, short[] bufferCMin, short[] bufferDMax, short[] bufferDMin,
        out short overflow, out uint triggerAt, out short triggered,
        uint noOfValues, uint noOfSamplesPerAggregate);

    /// <summary>Retrieves streaming values without aggregation.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="startTime">Receives the time of the first sample.</param>
    /// <param name="bufferA">Channel A's buffer, or null.</param>
    /// <param name="bufferB">Channel B's buffer, or null.</param>
    /// <param name="bufferC">Channel C's buffer, or null.</param>
    /// <param name="bufferD">Channel D's buffer, or null.</param>
    /// <param name="overflow">Receives per-channel overflow flags.</param>
    /// <param name="triggerAt">Receives the trigger index.</param>
    /// <param name="triggered">Receives non-zero when a trigger fired.</param>
    /// <param name="noOfValues">How many values to retrieve.</param>
    /// <returns>The number of values written.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_get_streaming_values_no_aggregation")]
    public static extern uint ps2000_get_streaming_values_no_aggregation(
        short handle, out double startTime,
        short[] bufferA, short[] bufferB, short[] bufferC, short[] bufferD,
        out short overflow, out uint triggerAt, out short triggered, uint noOfValues);

    // ------------------------------------------------------ signal generator

    /// <summary>Configures and starts the built-in signal generator.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="offsetVoltage">The DC offset, in microvolts.</param>
    /// <param name="pkToPk">The peak-to-peak amplitude, in microvolts.</param>
    /// <param name="waveType">The waveform ordinal.</param>
    /// <param name="startFrequency">The start frequency, in hertz. A 2204A tops out at 100 kHz.</param>
    /// <param name="stopFrequency">The stop frequency for a sweep, in hertz.</param>
    /// <param name="increment">The frequency step per sweep interval.</param>
    /// <param name="dwellTime">How long to hold each sweep step, in seconds.</param>
    /// <param name="sweepType">The sweep shape ordinal.</param>
    /// <param name="sweeps">How many sweeps to run; zero means continuous.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    /// <remarks>
    /// <b>Returns zero while any acquisition is running.</b> Verified on a 2204A:
    /// this fails during fast streaming and during an in-flight block capture,
    /// and succeeds immediately after <see cref="ps2000_stop"/>. Unit-info line 6
    /// still reads "0", so there is no diagnostic to distinguish this from a bad
    /// argument. Stop, configure, then restart. A 2204A also caps
    /// <paramref name="pkToPk"/> at 4 V (4000000 uV).
    /// </remarks>
    [DllImport(Driver, EntryPoint = "ps2000_set_sig_gen_built_in")]
    public static extern short ps2000_set_sig_gen_built_in(
        short handle, int offsetVoltage, uint pkToPk, int waveType,
        float startFrequency, float stopFrequency, float increment, float dwellTime,
        int sweepType, uint sweeps);

    /// <summary>Loads an arbitrary waveform into the generator and starts it.</summary>
    /// <param name="handle">The device handle.</param>
    /// <param name="offsetVoltage">The DC offset, in microvolts.</param>
    /// <param name="pkToPk">The peak-to-peak amplitude, in microvolts.</param>
    /// <param name="startDeltaPhase">The DDS phase increment at the start of a sweep.</param>
    /// <param name="stopDeltaPhase">The DDS phase increment at the end of a sweep.</param>
    /// <param name="deltaPhaseIncrement">How much the increment changes per sweep step.</param>
    /// <param name="dwellCount">How many DDS ticks to hold each sweep step.</param>
    /// <param name="arbitraryWaveform">The waveform samples, one byte each.</param>
    /// <param name="arbitraryWaveformSize">
    /// The number of samples. A 2204A accepts at most 4096 and rejects more.
    /// </param>
    /// <param name="sweepType">The sweep shape ordinal.</param>
    /// <param name="sweeps">How many sweeps to run; zero means continuous.</param>
    /// <returns>Non-zero on success; zero on failure.</returns>
    [DllImport(Driver, EntryPoint = "ps2000_set_sig_gen_arbitrary")]
    public static extern short ps2000_set_sig_gen_arbitrary(
        short handle, int offsetVoltage, uint pkToPk,
        uint startDeltaPhase, uint stopDeltaPhase, uint deltaPhaseIncrement, uint dwellCount,
        byte[] arbitraryWaveform, int arbitraryWaveformSize, int sweepType, uint sweeps);
}

/// <summary>
/// Per-channel threshold properties for the advanced trigger, matching
/// <c>PS2000_TRIGGER_CHANNEL_PROPERTIES</c>.
/// </summary>
/// <remarks>
/// The header wraps this in <c>#pragma pack(1)</c>, so the managed declaration
/// must specify <c>Pack = 1</c> to match. Leaving it at the default packing
/// silently corrupts every field after the first.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Ps2000TriggerChannelProperties
{
    /// <summary>The primary threshold, as an ADC count.</summary>
    public short ThresholdMajor;

    /// <summary>The secondary threshold, used as the far edge of a window.</summary>
    public short ThresholdMinor;

    /// <summary>The noise-rejection band around the threshold, as an ADC count.</summary>
    public ushort Hysteresis;

    /// <summary>The channel ordinal these properties apply to.</summary>
    public short Channel;

    /// <summary>0 for level comparison, 1 for window comparison.</summary>
    public int ThresholdMode;
}

/// <summary>
/// The logical condition combining channels in an advanced trigger, matching
/// <c>PS2000_TRIGGER_CONDITIONS</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Ps2000TriggerConditions
{
    /// <summary>The state channel A must be in.</summary>
    public int ChannelA;

    /// <summary>The state channel B must be in.</summary>
    public int ChannelB;

    /// <summary>The state channel C must be in.</summary>
    public int ChannelC;

    /// <summary>The state channel D must be in.</summary>
    public int ChannelD;

    /// <summary>The state the external input must be in.</summary>
    public int External;

    /// <summary>The state the pulse-width qualifier must be in.</summary>
    public int PulseWidthQualifier;
}

/// <summary>
/// The channel conditions a pulse-width qualifier applies to, matching
/// <c>PS2000_PWQ_CONDITIONS</c>. Note it has no pulse-width member of its own,
/// unlike <see cref="Ps2000TriggerConditions"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Ps2000PwqConditions
{
    /// <summary>The state channel A must be in.</summary>
    public int ChannelA;

    /// <summary>The state channel B must be in.</summary>
    public int ChannelB;

    /// <summary>The state channel C must be in.</summary>
    public int ChannelC;

    /// <summary>The state channel D must be in.</summary>
    public int ChannelD;

    /// <summary>The state the external input must be in.</summary>
    public int External;
}
