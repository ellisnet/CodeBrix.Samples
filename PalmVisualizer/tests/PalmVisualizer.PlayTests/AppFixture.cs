using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using CodeBrix.Webcam.Devices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PalmVisualizer.Camera;
using PalmVisualizer.Rendering;
using PalmVisualizer.ViewModels;
using PalmVisualizer.Views;
using PalmVisualizer.Vision;

namespace PalmVisualizer.PlayTests;

// All three collaborators are fakes, registered before the launch page is built: no camera is enumerated or
// opened, no model is loaded and the game engine is never started.
public sealed class AppFixture : SampleFixture<MainPage>
{
    private bool _keepSetup;

    public CaptureFixture Capture { get; } = new();
    public TrackerFixture Tracker { get; } = new();
    public SessionFactoryFixture Sessions { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    protected override Application CreateApplication() => new App(services => services
        .AddSingleton<IWebcamCaptureService>(Capture)
        .AddSingleton<IPalmTracker>(Tracker)
        .AddSingleton<IVisualizerSessionFactory>(Sessions));

    // Builds a fresh page against the camera setup the test has just scripted (startup discovery runs in the
    // page's constructor, before a test body starts).
    public async Task ReloadAsync()
    {
        _keepSetup = true;
        try { await ResetAsync(); }
        finally { _keepSetup = false; }
    }

    protected override async Task BeforeResetAsync()
    {
        // Let the outgoing page finish its discovery and camera switches, so none of its calls lands in the next
        // test's records. The launch page always starts the first default camera.
        Capture.ReleaseAll();
        if (View == null)
        {
            await Application.WaitForAsync(() => Capture.Started.Count > 0 && Capture.InFlight == 0, settled => settled,
                description: "the launch page's camera start");
        }
        else
        {
            await Application.WaitForAsync(() => Capture.InFlight == 0 && IsSettled(Model.StatusText), settled => settled,
                description: "the outgoing page's camera discovery and switch");
        }
        Capture.Reset(_keepSetup);
        Tracker.Reset();
        Sessions.Reset();
    }

    private static bool IsSettled(string status) =>
        !status.StartsWith("Discovering cameras", StringComparison.Ordinal)
        && !status.StartsWith("Found ", StringComparison.Ordinal);
}

public sealed class CaptureFixture : IWebcamCaptureService
{
    public static readonly string[] DefaultCameras = { "Fake Cam A", "Fake Cam B" };

    private readonly object _gate = new();
    private readonly Dictionary<string, TaskCompletionSource> _holds = new();
    private readonly List<string> _started = new();
    private byte[] _frame;
    private int _width;
    private int _height;
    private int _inFlight;

    public event EventHandler FrameArrived;

    // Scripted per page: read when the page discovers and starts cameras.
    public IReadOnlyList<string> Cameras { get; set; } = DefaultCameras;
    public Exception DiscoveryError { get; set; }
    public HashSet<string> FailingCameras { get; } = new();

    public bool IsRunning { get; private set; }
    public bool HasFrame { get { lock (_gate) return _frame != null; } }
    public int InFlight => Volatile.Read(ref _inFlight);
    public int StopCount { get; private set; }
    public int DisposeCount { get; private set; }
    public IReadOnlyList<string> Started { get { lock (_gate) return _started.ToArray(); } }

    // Holds the next Start on this camera until the returned gate is released.
    public TaskCompletionSource HoldStart(string friendlyName)
    {
        var hold = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_gate) _holds[friendlyName] = hold;
        return hold;
    }

    public void ReleaseAll()
    {
        lock (_gate)
        {
            foreach (var hold in _holds.Values) hold.TrySetResult();
            _holds.Clear();
        }
    }

    public void Reset(bool keepSetup)
    {
        ReleaseAll();
        lock (_gate)
        {
            _started.Clear();
            _frame = null;
        }
        if (!keepSetup)
        {
            Cameras = DefaultCameras;
            DiscoveryError = null;
            FailingCameras.Clear();
        }
        StopCount = DisposeCount = 0;
    }

    // Stands in for the capture thread: stores the frame, then raises FrameArrived on the caller's thread.
    public void PushFrame(byte[] bgra, int width, int height)
    {
        lock (_gate)
        {
            _frame = bgra;
            _width = width;
            _height = height;
        }
        FrameArrived?.Invoke(this, EventArgs.Empty);
    }

    public Task<IReadOnlyList<CameraDevice>> DiscoverCamerasAsync()
    {
        if (DiscoveryError != null) return Task.FromException<IReadOnlyList<CameraDevice>>(DiscoveryError);
        return Task.FromResult<IReadOnlyList<CameraDevice>>(
            Cameras.Select(name => new CameraDevice(new FakeImagingDevice(name))).ToArray());
    }

    public void Start(CameraDevice camera)
    {
        Interlocked.Increment(ref _inFlight);
        try
        {
            TaskCompletionSource hold;
            lock (_gate)
            {
                _started.Add(camera.FriendlyName);
                _frame = null;
                _holds.Remove(camera.FriendlyName, out hold);
            }
            hold?.Task.Wait();
            if (FailingCameras.Contains(camera.FriendlyName)) throw new InvalidOperationException("Fixture camera unavailable");
            IsRunning = true;
        }
        finally { Interlocked.Decrement(ref _inFlight); }
    }

    public void Stop()
    {
        StopCount++;
        IsRunning = false;
        lock (_gate) _frame = null;
    }

    public bool TryCopyLatestFrame(ref byte[] buffer, out int width, out int height)
    {
        lock (_gate)
        {
            width = _width;
            height = _height;
            if (_frame == null) return false;
            if (buffer == null || buffer.Length != _frame.Length) buffer = new byte[_frame.Length];
            Buffer.BlockCopy(_frame, 0, buffer, 0, _frame.Length);
            return true;
        }
    }

    // Each page's view model disposes the shared service; the fixture keeps it usable.
    public void Dispose() => DisposeCount++;

    // A solid-colour BGRA frame whose left half is one colour and right half another.
    public static byte[] SplitFrame(int width, int height, (byte R, byte G, byte B) left, (byte R, byte G, byte B) right)
    {
        var pixels = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = x < width / 2 ? left : right;
                var index = (y * width + x) * 4;
                pixels[index] = color.B;
                pixels[index + 1] = color.G;
                pixels[index + 2] = color.R;
                pixels[index + 3] = 255;
            }
        }
        return pixels;
    }
}

public sealed class FakeImagingDevice(string name) : IImagingMediaDevice
{
    public string Id => "fixture:" + name;
    public string FriendlyName => name;
    public ImagingDeviceHardwareInfo Hardware => null;
    public IReadOnlyList<ImagingMediaCapability> Capabilities => Array.Empty<ImagingMediaCapability>();
    public IReadOnlyList<IImagingDeviceControl> Controls => Array.Empty<IImagingDeviceControl>();
    public ImagingAudioPairing PairedMicrophone => null;
}

public sealed class TrackerFixture : IPalmTracker
{
    private int _submitted;

    public event EventHandler<PalmTrackingEventArgs> TrackingUpdated;

    public bool IsRunning { get; private set; }
    public int StartCount { get; private set; }
    public int StopCount { get; private set; }
    public int Submitted => Volatile.Read(ref _submitted);
    public (int Width, int Height) LastFrameSize { get; private set; }

    public void Reset()
    {
        IsRunning = false;
        StartCount = StopCount = 0;
        Volatile.Write(ref _submitted, 0);
        LastFrameSize = default;
    }

    public void Start()
    {
        StartCount++;
        IsRunning = true;
    }

    public void Stop()
    {
        StopCount++;
        IsRunning = false;
    }

    public void SubmitFrame(byte[] bgraPixels, int width, int height)
    {
        LastFrameSize = (width, height);
        Interlocked.Increment(ref _submitted);
    }

    // Stands in for the inference worker: raises TrackingUpdated on the caller's thread.
    public void Raise(params TrackedPalm[] palms) =>
        TrackingUpdated?.Invoke(this, new PalmTrackingEventArgs(new PalmTrackingResult(palms)));

    public static TrackedPalm Palm(int trackId, bool open, float x, float y) => new(trackId, open, x, y, 0.9f, 0.9f);

    // Each page's view model disposes the shared tracker; the fixture keeps it usable.
    public void Dispose() { }
}

public sealed class SessionFactoryFixture : IVisualizerSessionFactory
{
    private readonly List<SessionFixture> _created = new();

    public IReadOnlyList<SessionFixture> Created { get { lock (_created) return _created.ToArray(); } }
    public SessionFixture Last { get { lock (_created) return _created.LastOrDefault(); } }

    public void Reset()
    {
        lock (_created) _created.Clear();
    }

    public IVisualizerSession CreateSession(IGameCanvasHost host)
    {
        var session = new SessionFixture(host);
        lock (_created) _created.Add(session);
        return session;
    }
}

// Records the lifecycle calls and the palms; never starts the engine.
public sealed class SessionFixture(IGameCanvasHost host) : IVisualizerSession
{
    private readonly object _gate = new();
    private IReadOnlyList<PalmAttractor> _palms;
    private int _updates;

    public IGameCanvasHost Host { get; } = host;
    public bool IsStarted { get; private set; }
    public int StartCount { get; private set; }
    public int PauseCount { get; private set; }
    public int ResumeCount { get; private set; }
    public int StopCount { get; private set; }
    public int UpdateCount { get { lock (_gate) return _updates; } }
    public IReadOnlyList<PalmAttractor> Palms { get { lock (_gate) return _palms; } }

    public void Start()
    {
        StartCount++;
        IsStarted = true;
    }

    public void Pause() => PauseCount++;
    public void Resume() => ResumeCount++;
    public void Stop() => StopCount++;

    public void UpdatePalms(IReadOnlyList<PalmAttractor> palms)
    {
        lock (_gate)
        {
            _palms = palms?.ToArray() ?? Array.Empty<PalmAttractor>();
            _updates++;
        }
    }
}
