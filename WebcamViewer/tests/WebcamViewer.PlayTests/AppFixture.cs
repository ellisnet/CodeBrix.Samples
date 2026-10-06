using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using CodeBrix.Webcam.Devices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using WebcamViewer.Cameras;
using WebcamViewer.ViewModels;
using WebcamViewer.Views;

namespace WebcamViewer.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    // Registered before App builds its first page, so no page ever reaches a real camera.
    public CameraFixture Cameras { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    protected override Application CreateApplication() => new App(
        services => services.AddSingleton<ICameraService>(Cameras));

    protected override Task BeforeResetAsync()
    {
        Cameras.Reset();
        return Task.CompletedTask;
    }
}

public sealed class CameraFixture : ICameraService
{
    private readonly object _gate = new();
    private readonly List<CameraSessionFixture> _sessions = new();
    private TaskCompletionSource<IReadOnlyList<IImagingMediaDevice>> _discovery = NewDiscovery();

    // Thrown by CapturePhoto when set.
    public Exception PhotoError { get; set; }

    // Every session the page opened since the last reset, oldest first.
    public IReadOnlyList<CameraSessionFixture> Sessions
    {
        get { lock (_gate) return _sessions.ToArray(); }
    }

    // Discovery holds until the test calls Discover or FailDiscovery.
    public void Discover(params CameraDevice[] cameras) => _discovery.TrySetResult(cameras);

    public void FailDiscovery(string message) => _discovery.TrySetException(new InvalidOperationException(message));

    public void Reset()
    {
        // A page still waiting on the previous discovery sees no cameras.
        _discovery.TrySetResult(Array.Empty<IImagingMediaDevice>());
        _discovery = NewDiscovery();
        PhotoError = null;
        lock (_gate) _sessions.Clear();
    }

    public Task<IReadOnlyList<IImagingMediaDevice>> GetCamerasAsync() => _discovery.Task;

    public ICameraSession OpenSession(IImagingMediaDevice device)
    {
        var session = new CameraSessionFixture(this, (CameraDevice)device);
        lock (_gate) _sessions.Add(session);
        return session;
    }

    public static CameraDevice Camera(string name, bool microphone = false, string startError = null) =>
        new(name, microphone, startError);

    private static TaskCompletionSource<IReadOnlyList<IImagingMediaDevice>> NewDiscovery() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);
}

public sealed class CameraDevice : IImagingMediaDevice
{
    public CameraDevice(string name, bool microphone, string startError)
    {
        Id = "fixture:" + name;
        FriendlyName = name;
        StartError = startError;
        PairedMicrophone = microphone ? new ImagingAudioPairing("fixture-mic:" + name, name + " Microphone") : null;
    }

    public string Id { get; }
    public string FriendlyName { get; }
    // Start throws with this message when set.
    public string StartError { get; }
    public ImagingDeviceHardwareInfo Hardware { get; } = new(0, 0, null, null, null);
    public IReadOnlyList<ImagingMediaCapability> Capabilities { get; } = Array.Empty<ImagingMediaCapability>();
    public IReadOnlyList<IImagingDeviceControl> Controls { get; } = Array.Empty<IImagingDeviceControl>();
    public ImagingAudioPairing PairedMicrophone { get; }
}

public sealed class CameraSessionFixture : ICameraSession
{
    private readonly CameraFixture _owner;
    private readonly object _gate = new();
    private byte[] _lastFrame;
    private int _lastWidth;
    private int _lastHeight;

    public CameraSessionFixture(CameraFixture owner, CameraDevice device)
    {
        _owner = owner;
        Device = device;
    }

    public event EventHandler<CameraFrameEventArgs> FrameReceived;

    public CameraDevice Device { get; }
    public bool IsStarted { get; private set; }
    public bool IsDisposed { get; private set; }
    public bool IsAudioCaptureActive => Device.PairedMicrophone != null;
    public bool MonitorAudio { get; set; }

    public void Start()
    {
        if (Device.StartError != null) throw new InvalidOperationException(Device.StartError);
        IsStarted = true;
    }

    // Delivers one solid-colour BGRA frame from a worker thread, as the real capture thread does.
    public Task SendFrameAsync(int width, int height, byte red, byte green, byte blue) => Task.Run(() =>
    {
        var pixels = new byte[width * height * 4];
        for (var index = 0; index < pixels.Length; index += 4)
        {
            pixels[index] = blue;
            pixels[index + 1] = green;
            pixels[index + 2] = red;
            pixels[index + 3] = 255;
        }
        lock (_gate)
        {
            _lastFrame = pixels;
            _lastWidth = width;
            _lastHeight = height;
        }
        FrameReceived?.Invoke(this, new CameraFrameEventArgs(width, height,
            destination => Buffer.BlockCopy(pixels, 0, destination, 0, pixels.Length)));
    });

    public byte[] CapturePhoto(out int width, out int height)
    {
        if (_owner.PhotoError != null) throw _owner.PhotoError;
        lock (_gate)
        {
            if (_lastFrame == null) throw new InvalidOperationException("No frame has arrived.");
            width = _lastWidth;
            height = _lastHeight;
            return (byte[])_lastFrame.Clone();
        }
    }

    public void Dispose()
    {
        IsStarted = false;
        IsDisposed = true;
    }
}
