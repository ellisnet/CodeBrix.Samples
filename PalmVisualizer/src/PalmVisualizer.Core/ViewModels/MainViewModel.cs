using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.Simple;
using PalmVisualizer.Camera;
using PalmVisualizer.Rendering;
using PalmVisualizer.Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PalmVisualizer.ViewModels;

/// <summary>
/// Lets the hosting page hand the view model the invalidate (repaint) delegate for the
/// live-preview canvas. Frames arrive on the capture thread; the page's delegate is
/// responsible for marshalling its invalidate onto the UI thread.
/// </summary>
public interface ICanvasBridge
{
    /// <summary>Invalidates the live-preview canvas shown in Camera Mode.</summary>
    Action InvalidatePreviewCanvas { get; set; }
}

/// <summary>
/// Lets the hosting page tell the view model when the visualizer's game canvas has its
/// first real layout size - the engine can only start against a non-zero surface, which
/// happens the first time Visualize Mode is shown.
/// </summary>
public interface IManageGameCanvas
{
    /// <summary>Called once, on the UI thread, at the canvas's FirstStarted event.</summary>
    /// <param name="canvas">The game canvas the visualizer renders into.</param>
    void CanvasFirstStart(GameSurfaceCanvas canvas);
}

[Microsoft.UI.Xaml.Data.Bindable]
public class MainViewModel : SimpleViewModel, ICanvasBridge, IManageGameCanvas
{
    private WebcamCaptureService _captureService;
    private PalmTracker _tracker;
    private VisualizerSession _visualizerSession;

    private byte[] _visionFrame;
    private int _reportedOpenPalmCount;

    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Debug.WriteLine("PalmVisualizer view model startup.");

        _captureService = new WebcamCaptureService();
        _captureService.FrameArrived += OnFrameArrived;

        StatusText = "Discovering cameras…";
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            var cameras = await WebcamCaptureService.GetCamerasAsync();
            InvokeOnMainThread(() =>
            {
                Cameras.Clear();
                foreach (CameraDevice camera in cameras)
                {
                    Cameras.Add(camera);
                }
                if (Cameras.Count == 0)
                {
                    StatusText = "No cameras were found on this machine.";
                }
                else
                {
                    StatusText = $"Found {Cameras.Count} camera(s).";
                    SelectedCamera = Cameras[0]; //auto-start on the first camera
                }
            });
        }
        catch (Exception e)
        {
            InvokeOnMainThread(() => StatusText = $"Camera discovery failed: {e.Message}");
        }
    }

    /// <summary>The capture service - the page's preview canvas pulls live frames from it.</summary>
    public WebcamCaptureService CaptureService => _captureService;

    #region | Live frames and palm tracking |

    private void OnFrameArrived(object sender, EventArgs e)
    {
        //Capture-thread context: get out fast
        if (!HasFrame)
        {
            InvokeOnMainThread(() => HasFrame = _captureService.HasFrame);
        }

        if (IsCameraMode)
        {
            InvalidatePreviewCanvas?.Invoke();
        }
        else
        {
            //Visualize Mode: the live feed drives the palm tracker
            PalmTracker tracker = _tracker;
            if (tracker != null && tracker.IsRunning
                && _captureService.TryCopyLatestFrame(ref _visionFrame, out int width, out int height))
            {
                tracker.SubmitFrame(_visionFrame, width, height);
            }
        }
    }

    private void OnTrackingUpdated(object sender, PalmTrackingEventArgs e)
    {
        //Worker-thread context: the visualizer's attractor field is thread-safe, so the
        //  palms feed straight in - only the status line needs the UI thread
        VisualizerSession session = _visualizerSession;
        if (IsCameraMode || session == null) { return; }

        var attractors = new List<PalmAttractor>(e.Result.Palms.Count);
        foreach (TrackedPalm palm in e.Result.Palms)
        {
            //Only OPEN palms attract the colors - and the user watched a mirrored
            //  preview, so mirror the palm positions to match
            if (palm.IsOpenPalm)
            {
                attractors.Add(new PalmAttractor(palm.TrackId, 1f - palm.PalmCenterX, palm.PalmCenterY));
            }
        }
        session.UpdatePalms(attractors);

        int openCount = attractors.Count;
        if (openCount != _reportedOpenPalmCount)
        {
            _reportedOpenPalmCount = openCount;
            InvokeOnMainThread(() => StatusText = openCount switch
            {
                0 => "Show the camera your open palm - the colors will gather toward it.",
                1 => "The colors are chasing your open palm - close your hand to set them free.",
                _ => $"The colors are chasing {openCount} open palms - close your hands to set them free.",
            });
        }
    }

    #endregion

    #region | Bindable properties |

    /// <summary>The connected cameras shown in the dropdown.</summary>
    public ObservableCollection<CameraDevice> Cameras { get; } = new();

    private CameraDevice _selectedCamera;
    public CameraDevice SelectedCamera
    {
        get => _selectedCamera;
        set
        {
            if (_selectedCamera != value)
            {
                SetProperty(ref _selectedCamera, value);
                SwitchCamera(value);
            }
        }
    }

    private bool _isCameraMode = true;
    [AffectsCommands(nameof(VisualizeCommand), nameof(BackCommand))]
    public bool IsCameraMode
    {
        get => _isCameraMode;
        private set
        {
            SetProperty(ref _isCameraMode, value);
            NotifyPropertyChanged(nameof(IsVisualizeMode));
        }
    }

    /// <summary>Visualize Mode is simply not-Camera Mode.</summary>
    public bool IsVisualizeMode => !IsCameraMode;

    private bool _hasFrame;
    [AffectsCommands(nameof(VisualizeCommand))]
    public bool HasFrame
    {
        get => _hasFrame;
        private set => SetProperty(ref _hasFrame, value);
    }

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value ?? string.Empty);
    }

    /// <summary>Set by the hosting page (see <see cref="ICanvasBridge"/>).</summary>
    public Action InvalidatePreviewCanvas { get; set; }

    #endregion

    private void SwitchCamera(CameraDevice camera)
    {
        try
        {
            HasFrame = false;
            if (camera == null)
            {
                _captureService.Stop();
                InvalidatePreviewCanvas?.Invoke();
                return;
            }

            _captureService.Start(camera);
            StatusText = $"Live: {camera.FriendlyName}";
        }
        catch (Exception e)
        {
            StatusText = $"Could not start '{camera?.FriendlyName}': {e.Message}";
        }
    }

    #region | IManageGameCanvas implementation |

    public void CanvasFirstStart(GameSurfaceCanvas canvas)
    {
        //UI thread, the first time Visualize Mode is shown with a real size: build the
        //  shader scene and start the engine. Later mode switches pause and resume it.
        _visualizerSession = new VisualizerSession(canvas);
        _visualizerSession.Start();
    }

    #endregion

    #region | Commands and their implementations |

    #region VisualizeCommand

    private SimpleCommand _visualizeCommand;
    public SimpleCommand VisualizeCommand =>
        (_visualizeCommand ??= new SimpleCommand(CanVisualize, DoVisualize));

    private bool CanVisualize() => IsCameraMode && HasFrame;

    private Task DoVisualize()
    {
        if (!CanVisualize()) { return Task.CompletedTask; }

        if (_tracker == null)
        {
            _tracker = new PalmTracker();
            _tracker.TrackingUpdated += OnTrackingUpdated;
        }
        _tracker.Start();
        _reportedOpenPalmCount = 0;

        //Showing the game canvas gives it its first real layout size, which raises its
        //  FirstStarted -> CanvasFirstStart the first time through; on later entries the
        //  engine is merely paused from Camera Mode, so wake it back up
        IsCameraMode = false;
        _visualizerSession?.Resume();

        StatusText = "Show the camera your open palm - the colors will gather toward it.";
        return Task.CompletedTask;
    }

    #endregion

    #region BackCommand

    private SimpleCommand _backCommand;
    public SimpleCommand BackCommand =>
        (_backCommand ??= new SimpleCommand(CanGoBack, DoGoBack));

    private bool CanGoBack() => IsVisualizeMode;

    private Task DoGoBack()
    {
        if (!CanGoBack()) { return Task.CompletedTask; }

        _tracker?.Stop();
        _visualizerSession?.Pause();

        IsCameraMode = true;
        InvalidatePreviewCanvas?.Invoke();
        StatusText = SelectedCamera != null
            ? $"Live: {SelectedCamera.FriendlyName}"
            : "Select a camera.";
        return Task.CompletedTask;
    }

    #endregion

    #endregion

    #region | IDisposable implementation |

    public override void Dispose()
    {
        _visualizeCommand?.Dispose();
        _visualizeCommand = null;
        _backCommand?.Dispose();
        _backCommand = null;

        InvalidatePreviewCanvas = null;

        if (_tracker != null)
        {
            _tracker.TrackingUpdated -= OnTrackingUpdated;
            _tracker.Dispose();
            _tracker = null;
        }

        VisualizerSession session = _visualizerSession;
        _visualizerSession = null;
        session?.Stop();

        if (_captureService != null)
        {
            _captureService.FrameArrived -= OnFrameArrived;
            _captureService.Dispose();
            _captureService = null;
        }

        base.Dispose();
    }

    #endregion
}
