using CodeBrix.Webcam;
using CodeBrix.Webcam.Capture;
using CodeBrix.Webcam.Devices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebcamPainter.Webcam;

/// <summary>
/// The webcam capture model for WebcamPainter: discovers connected cameras, runs a live
/// capture session on the selected one, keeps the most recent BGRA frame available for
/// rendering (and for the hand-tracking pipeline), and takes in-memory still photos.
/// Audio is never captured - this application only paints.
/// </summary>
public sealed class WebcamCaptureService : IDisposable
{
    private readonly object _frameLock = new object();
    private byte[] _latestFrame;
    private int _frameWidth;
    private int _frameHeight;

    private WebcamSession _session;

    /// <summary>
    /// Discovers the cameras connected to this computer.
    /// </summary>
    /// <returns>The connected cameras; empty when none were found.</returns>
    public static async Task<IReadOnlyList<CameraDevice>> GetCamerasAsync()
    {
        IReadOnlyList<IImagingMediaDevice> devices = await WebcamDevices.GetImagingMediaDeviceListAsync();
        var cameras = new List<CameraDevice>();
        foreach (IImagingMediaDevice device in devices)
        {
            cameras.Add(new CameraDevice(device));
        }
        return cameras;
    }

    /// <summary>Indicates whether a capture session is currently running.</summary>
    public bool IsRunning => _session != null;

    /// <summary>Indicates whether at least one frame has arrived since the session started.</summary>
    public bool HasFrame
    {
        get
        {
            lock (_frameLock) { return _latestFrame != null; }
        }
    }

    /// <summary>
    /// Raised after each new frame has been copied and is available via
    /// <see cref="TryCopyLatestFrame"/>. Raised on the CAPTURE thread - handlers must get
    /// out fast and must marshal any UI work themselves.
    /// </summary>
    public event EventHandler FrameArrived;

    /// <summary>
    /// Starts (or switches) live capture on the given camera. Any prior session is stopped
    /// first and its last frame discarded.
    /// </summary>
    /// <param name="camera">The camera to capture from.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="camera"/> is null.</exception>
    public void Start(CameraDevice camera)
    {
        if (camera == null) { throw new ArgumentNullException(nameof(camera)); }

        Stop();

        _session = new WebcamSession(camera.Device);
        _session.FrameReceived += OnFrameReceived;
        _session.Start();
    }

    /// <summary>Stops the running capture session, when there is one.</summary>
    public void Stop()
    {
        if (_session != null)
        {
            _session.FrameReceived -= OnFrameReceived;
            _session.Dispose();
            _session = null;
        }
        lock (_frameLock)
        {
            _latestFrame = null;
            _frameWidth = 0;
            _frameHeight = 0;
        }
    }

    private void OnFrameReceived(object sender, WebcamFrameEventArgs frame)
    {
        //Capture-thread context: copy the pixels and get out fast
        lock (_frameLock)
        {
            var needed = (int)(frame.Width * frame.Height * 4);
            if (_latestFrame == null || _latestFrame.Length != needed)
            {
                _latestFrame = new byte[needed];
            }
            frame.CopyTo(_latestFrame);
            _frameWidth = (int)frame.Width;
            _frameHeight = (int)frame.Height;
        }

        FrameArrived?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Copies the most recent frame (tightly packed BGRA) into <paramref name="buffer"/>,
    /// which is (re)allocated as needed. Returns <c>false</c> when no frame has arrived yet.
    /// Safe to call from any thread.
    /// </summary>
    /// <param name="buffer">The caller's frame buffer; replaced when the size does not match.</param>
    /// <param name="width">The frame's width in pixels.</param>
    /// <param name="height">The frame's height in pixels.</param>
    /// <returns><c>true</c> when a frame was copied.</returns>
    public bool TryCopyLatestFrame(ref byte[] buffer, out int width, out int height)
    {
        lock (_frameLock)
        {
            if (_latestFrame == null)
            {
                width = 0;
                height = 0;
                return false;
            }
            if (buffer == null || buffer.Length != _latestFrame.Length)
            {
                buffer = new byte[_latestFrame.Length];
            }
            Array.Copy(_latestFrame, buffer, _latestFrame.Length);
            width = _frameWidth;
            height = _frameHeight;
            return true;
        }
    }

    /// <summary>
    /// Captures a still photo from the live session, in memory (nothing is written to disk).
    /// </summary>
    /// <returns>The captured photo.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no capture session is running.</exception>
    public CapturedPhoto CapturePhoto()
    {
        WebcamSession session = _session;
        if (session == null)
        {
            throw new InvalidOperationException("Start a camera before capturing a photo.");
        }

        WebcamPhoto photo = session.CapturePhoto();
        return new CapturedPhoto(photo.PixelsBgra32, photo.Width, photo.Height);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        FrameArrived = null;
        Stop();
    }
}
