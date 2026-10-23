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
/// Lets the hosting page hand the view model the page that shows the visualizer, so the view
/// model can start a visualizer session in it without ever naming a view type.
/// </summary>
public interface IManageVisualizerCanvas
{
    /// <summary>Called once, on the UI thread, when the page and the view model are paired.</summary>
    /// <param name="host">The page whose panel the visualizers are shown in.</param>
    void SetVisualizerCanvasHost(IVisualizerCanvasHost host);
}

[Microsoft.UI.Xaml.Data.Bindable]
public class MainViewModel : SimpleViewModel, ICanvasBridge, IManageVisualizerCanvas, IWebcamFrameSource
{
    private const string OpenPalmPrompt = "Show the camera your open palm - the colors will gather toward it.";

    private readonly IVisualizerSessionFactory _sessionFactory;

    private IWebcamCaptureService _captureService;
    private IPalmTracker _tracker;
    private IVisualizerCanvasHost _visualizerHost;
    private IVisualizerSession _visualizerSession;

    private byte[] _visionFrame;
    private int _reportedOpenPalmCount;

    private int _cameraSwitchVersion;

    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Debug.WriteLine("PalmVisualizer view model startup.");

        //The three collaborators come from the container App registered them with, so this
        //  view model can also be built against stand-ins that need no camera and no GPU
        _captureService = GetService<IWebcamCaptureService>() ?? new WebcamCaptureService();
        _tracker = GetService<IPalmTracker>() ?? new PalmTracker();
        _sessionFactory = GetService<IVisualizerSessionFactory>() ?? new VisualizerSessionFactory();

        _captureService.FrameArrived += OnFrameArrived;
        _tracker.TrackingUpdated += OnTrackingUpdated;

        StatusText = "Discovering cameras…";
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            var cameras = await _captureService.DiscoverCamerasAsync();
            InvokeOnMainThread(() =>
            {
                Cameras.Clear();
                foreach (var camera in cameras)
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

    #region | Live frames and palm tracking |

    private void OnFrameArrived(object sender, EventArgs e)
    {
        //Capture-thread context: get out fast
        if (!HasFrame)
        {
            //The page may have been closed (and the service released) by the time this runs
            InvokeOnMainThread(() => HasFrame = _captureService?.HasFrame ?? false);
        }

        if (IsCameraMode)
        {
            InvalidatePreviewCanvas?.Invoke();
        }
        else
        {
            //Visualize Mode: the live feed drives the palm tracker
            var tracker = _tracker;
            if (tracker is { IsRunning: true }
                && _captureService.TryCopyLatestFrame(ref _visionFrame, out var width, out var height))
            {
                tracker.SubmitFrame(_visionFrame, width, height);
            }
        }
    }

    private void OnTrackingUpdated(object sender, PalmTrackingEventArgs e)
    {
        //Worker-thread context: the visualizer's attractor field is thread-safe, so the
        //  palms feed straight in - only the status line needs the UI thread
        var session = _visualizerSession;
        if (IsCameraMode || session == null) { return; }

        var attractors = new List<PalmAttractor>(e.Result.Palms.Count);
        foreach (var palm in e.Result.Palms)
        {
            //Only OPEN palms attract the colors - and the user watched a mirrored
            //  preview, so mirror the palm positions to match
            if (palm.IsOpenPalm)
            {
                attractors.Add(new PalmAttractor(palm.TrackId, 1f - palm.PalmCenterX, palm.PalmCenterY));
            }
        }
        session.UpdatePalms(attractors);

        var openCount = attractors.Count;
        if (openCount != _reportedOpenPalmCount)
        {
            _reportedOpenPalmCount = openCount;
            InvokeOnMainThread(() =>
            {
                //Back may have run since this frame was analyzed: its status line stands
                if (IsCameraMode) { return; }
                StatusText = openCount switch
                {
                    0 => OpenPalmPrompt,
                    1 => "The colors are chasing your open palm - close your hand to set them free.",
                    _ => $"The colors are chasing {openCount} open palms - close your hands to set them free.",
                };
            });
        }
    }

    #endregion

    #region | IWebcamFrameSource implementation |

    /// <summary>
    /// Hands the page's preview renderer the newest camera frame. Implemented explicitly:
    /// the page pulls frames through the narrow <see cref="IWebcamFrameSource"/> seam and
    /// never sees the capture service behind it.
    /// </summary>
    /// <param name="buffer">The renderer's frame buffer; replaced when the size does not match.</param>
    /// <param name="width">The frame's width in pixels.</param>
    /// <param name="height">The frame's height in pixels.</param>
    /// <returns><c>true</c> when a frame was copied.</returns>
    bool IWebcamFrameSource.TryCopyLatestFrame(ref byte[] buffer, out int width, out int height)
    {
        var service = _captureService;
        if (service == null)
        {
            width = 0;
            height = 0;
            return false;
        }
        return service.TryCopyLatestFrame(ref buffer, out width, out height);
    }

    #endregion

    #region | Bindable properties |

    /// <summary>The connected cameras shown in the dropdown.</summary>
    public ObservableCollection<CameraDevice> Cameras { get; } = new();

    public CameraDevice SelectedCamera
    {
        get;
        set
        {
            if (field != value)
            {
                SetProperty(ref field, value);
                SwitchCamera(value);
            }
        }
    }

    /// <summary>The visualizers the dropdown lists.</summary>
    public IReadOnlyList<VisualizerOption> Visualizers => VisualizerOption.All;

    /// <summary>
    /// The visualizer Visualize Mode shows. Choosing another while it is showing switches the
    /// visual at once; chosen in Camera Mode, it is the one the next "Visualize!" shows.
    /// </summary>
    public VisualizerOption SelectedVisualizer
    {
        get;
        set
        {
            if (value != null && field != value)
            {
                SetProperty(ref field, value);
                SwitchVisualizer();
            }
        }
    } = VisualizerOption.All[0];

    [AffectsCommands(nameof(VisualizeCommand), nameof(BackCommand))]
    public bool IsCameraMode
    {
        get;
        private set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(IsVisualizeMode));
        }
    } = true;

    /// <summary>Visualize Mode is simply not-Camera Mode.</summary>
    public bool IsVisualizeMode => !IsCameraMode;

    [AffectsCommands(nameof(VisualizeCommand))]
    public bool HasFrame
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public string StatusText
    {
        get;
        set => SetProperty(ref field, value ?? string.Empty);
    } = string.Empty;

    /// <summary>Set by the hosting page (see <see cref="ICanvasBridge"/>).</summary>
    public Action InvalidatePreviewCanvas { get; set; }

    #endregion

    private void SwitchCamera(CameraDevice camera)
    {
        //The setter only kicks the switch off: opening a device can take long enough to
        //  stall the UI thread, so the switch itself runs on a worker and the status line
        //  carries the result
        HasFrame = false;
        _ = SwitchCameraAsync(camera, ++_cameraSwitchVersion);
    }

    /// <summary>
    /// Starts (or stops) live capture off the UI thread - the capture service serializes its
    /// own start and stop, so two devices are never opened at once - and lets only the newest
    /// switch report: a camera that finishes opening late must not overwrite the status of
    /// the one the user has since chosen.
    /// </summary>
    private async Task SwitchCameraAsync(CameraDevice camera, int version)
    {
        try
        {
            var service = _captureService;
            if (service == null) { return; }

            await Task.Run(() =>
            {
                if (camera == null)
                {
                    service.Stop();
                }
                else
                {
                    service.Start(camera);
                }
            }).ConfigureAwait(false);

            if (version != _cameraSwitchVersion) { return; } //a newer switch took over

            if (camera == null)
            {
                //The page's delegate marshals the repaint itself
                InvalidatePreviewCanvas?.Invoke();
                return;
            }
            InvokeOnMainThread(() => StatusText = $"Live: {camera.FriendlyName}");
        }
        catch (Exception e)
        {
            if (version == _cameraSwitchVersion)
            {
                InvokeOnMainThread(() =>
                    StatusText = $"Could not start '{camera?.FriendlyName}': {e.Message}");
            }
        }
    }

    #region | IManageVisualizerCanvas implementation |

    public void SetVisualizerCanvasHost(IVisualizerCanvasHost host) => _visualizerHost = host;

    #endregion

    #region | Visualizer sessions |

    /// <summary>
    /// Creates and starts a session for the selected visualizer (UI thread). Later mode
    /// switches pause and resume it; choosing another visualizer stops it.
    /// </summary>
    private void StartVisualizer()
    {
        var host = _visualizerHost;
        var option = SelectedVisualizer;
        if (host == null || option == null) { return; }

        var session = _sessionFactory.CreateSession(option.Kind, host);
        session.Problem += OnVisualizerProblem;
        _visualizerSession = session;
        session.Start();
    }

    /// <summary>
    /// Stops the current session, if any (UI thread). The field is read into a local and
    /// nulled first, so a tracking result arriving on the worker thread sees no session
    /// rather than one that is stopping.
    /// </summary>
    private void StopVisualizer()
    {
        var session = _visualizerSession;
        _visualizerSession = null;
        if (session == null) { return; }

        session.Problem -= OnVisualizerProblem;
        session.Stop();
    }

    private void SwitchVisualizer()
    {
        //A session draws one visualizer for its whole life: switching replaces it. In Camera
        //  Mode that is all - the next "Visualize!" starts the new choice
        StopVisualizer();
        if (IsVisualizeMode)
        {
            _reportedOpenPalmCount = 0;
            StartVisualizer();
            StatusText = OpenPalmPrompt;
        }
    }

    private void OnVisualizerProblem(object sender, VisualizerProblemEventArgs e)
    {
        InvokeOnMainThread(() =>
        {
            //Only the visualizer the user is looking at reports
            if (IsVisualizeMode && ReferenceEquals(sender, _visualizerSession))
            {
                StatusText = e.Message;
            }
        });
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

        //Starting the tracker is what loads the models, on its own worker thread
        _tracker.Start();
        _reportedOpenPalmCount = 0;

        //The first time through, start a session for the selected visualizer; on later
        //  entries it is merely paused from Camera Mode, so wake it back up
        IsCameraMode = false;
        if (_visualizerSession == null)
        {
            StartVisualizer();
        }
        else
        {
            _visualizerSession.Resume();
        }

        StatusText = OpenPalmPrompt;
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

        StopVisualizer();

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
