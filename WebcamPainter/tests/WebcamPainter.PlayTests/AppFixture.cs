using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Samples.PlayTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SkiaSharp;
using WebcamPainter.Painting;
using WebcamPainter.ViewModels;
using WebcamPainter.Views;
using WebcamPainter.Vision;
using WebcamPainter.Webcam;

namespace WebcamPainter.PlayTests;

public sealed class AppFixture : SampleFixture<MainPage>
{
    private bool _keepSetup;
    public CameraFixture Camera { get; } = new();
    public TrackerFixture Tracker { get; } = new();
    public MainViewModel Model => (MainViewModel)View.DataContext;
    public PaintingSession Session => Model.PaintSession;
    // The fakes are registered before the launch page exists, so no page ever sees a real camera.
    protected override Application CreateApplication() => new App(
        services => services.AddSingleton<ICameraSource>(Camera).AddSingleton<IHandTracker>(Tracker));

    // A fresh page that keeps the test's camera setup, for setups the page reads at construction.
    public async Task ReloadAsync()
    {
        _keepSetup = true;
        try { await ResetAsync(); }
        finally { _keepSetup = false; }
    }

    protected override Task BeforeResetAsync()
    {
        if (!_keepSetup) Camera.Reset();
        Camera.ClearHistory();
        Tracker.Reset();
        return Task.CompletedTask;
    }
}

public sealed class CameraFixture : ICameraSource
{
    public const string FirstCamera = "Fixture Cam A";
    public const string SecondCamera = "Fixture Cam B";
    public const int DefaultWidth = 320;
    public const int DefaultHeight = 240;
    private readonly object _lock = new();
    private byte[] _pattern;
    private int _patternWidth, _patternHeight;
    private byte[] _latest;
    private int _latestWidth, _latestHeight;
    private volatile bool _hasFrame;
    private volatile CameraDevice _current;

    public CameraFixture() => Reset();

    public List<CameraDevice> Cameras { get; } = new();
    // Start throws for the camera with this name, as a camera in use by another program does.
    public string FailingCamera { get; set; }
    public List<string> Started { get; } = new();
    public CameraDevice Current => _current;
    public bool IsRunning => _current != null;
    public bool HasFrame => _hasFrame;
    public event EventHandler FrameArrived;

    public void Reset()
    {
        Stop();
        Cameras.Clear();
        Cameras.Add(new CameraDevice("fixture-a", FirstCamera));
        Cameras.Add(new CameraDevice("fixture-b", SecondCamera));
        FailingCamera = null;
        SetFrame(DefaultWidth, DefaultHeight, SKColors.Red, SKColors.Blue);
    }

    public void ClearHistory()
    {
        lock (_lock) Started.Clear();
    }

    // The frames EmitFrame delivers from now on: the left half of the (unmirrored) frame in one colour, the right in another.
    public void SetFrame(int width, int height, SKColor left, SKColor right)
    {
        var pixels = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = x < width / 2 ? left : right;
                var index = (y * width + x) * 4;
                pixels[index] = color.Blue;
                pixels[index + 1] = color.Green;
                pixels[index + 2] = color.Red;
                pixels[index + 3] = 255;
            }
        }
        lock (_lock) (_pattern, _patternWidth, _patternHeight) = (pixels, width, height);
    }

    // Delivers one frame on the calling thread, as the capture thread delivers a real one.
    public void EmitFrame()
    {
        lock (_lock)
        {
            if (_current == null) throw new InvalidOperationException("No fixture camera is running.");
            (_latest, _latestWidth, _latestHeight) = ((byte[])_pattern.Clone(), _patternWidth, _patternHeight);
            _hasFrame = true;
        }
        FrameArrived?.Invoke(this, EventArgs.Empty);
    }

    public Task<IReadOnlyList<CameraDevice>> GetCamerasAsync() =>
        Task.FromResult<IReadOnlyList<CameraDevice>>(Cameras.ToArray());

    public void Start(CameraDevice camera)
    {
        ArgumentNullException.ThrowIfNull(camera);
        Stop();
        if (camera.FriendlyName == FailingCamera) throw new InvalidOperationException("The fixture camera is busy.");
        lock (_lock) Started.Add(camera.FriendlyName);
        _current = camera;
    }

    public void Stop()
    {
        lock (_lock)
        {
            _current = null;
            _hasFrame = false;
            _latest = null;
        }
    }

    public bool TryCopyLatestFrame(ref byte[] buffer, out int width, out int height)
    {
        lock (_lock)
        {
            if (_latest == null)
            {
                width = height = 0;
                return false;
            }
            if (buffer == null || buffer.Length != _latest.Length) buffer = new byte[_latest.Length];
            Array.Copy(_latest, buffer, _latest.Length);
            (width, height) = (_latestWidth, _latestHeight);
            return true;
        }
    }

    public CapturedPhoto CapturePhoto()
    {
        lock (_lock)
        {
            if (_current == null) throw new InvalidOperationException("Start a camera before capturing a photo.");
            if (_latest == null) throw new InvalidOperationException("No frame has arrived yet.");
            return new CapturedPhoto((byte[])_latest.Clone(), _latestWidth, _latestHeight);
        }
    }

    // The application disposes its camera with each page; the fixture outlives the pages.
    public void Dispose() => Stop();
}

public sealed class TrackerFixture : IHandTracker
{
    private volatile bool _running;
    private int _startCount;
    private int _submittedFrames;

    public bool IsRunning => _running;
    public int StartCount => Volatile.Read(ref _startCount);
    public int SubmittedFrames => Volatile.Read(ref _submittedFrames);
    public event EventHandler<HandTrackingEventArgs> TrackingUpdated;

    public void Reset()
    {
        _running = false;
        Volatile.Write(ref _startCount, 0);
        Volatile.Write(ref _submittedFrames, 0);
    }

    public void Start()
    {
        if (_running) return;
        _running = true;
        Interlocked.Increment(ref _startCount);
    }

    public void Stop() => _running = false;

    public void SubmitFrame(byte[] bgraPixels, int width, int height)
    {
        if (_running && bgraPixels != null && width > 0 && height > 0) Interlocked.Increment(ref _submittedFrames);
    }

    // A hand at palm position (x, y), normalized across the UNMIRRORED camera frame, raised on
    // the calling thread as the tracker's worker raises a real result.
    public void Report(bool openPalm, float x, float y) =>
        TrackingUpdated?.Invoke(this, new HandTrackingEventArgs(new HandTrackingResult(true, openPalm, x, y, 1f, 1f)));

    public void LoseHand() => TrackingUpdated?.Invoke(this, new HandTrackingEventArgs(HandTrackingResult.NoHand));

    public void Dispose() => Stop();
}
