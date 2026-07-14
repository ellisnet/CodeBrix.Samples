using CodeBrix.Platform.Simple;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WebcamPainter.Painting;
using WebcamPainter.Vision;
using WebcamPainter.Webcam;

namespace WebcamPainter.ViewModels;

/// <summary>
/// Lets the hosting page give the view model a native "Save JPEG as…" file dialog. The
/// Skia heads wire this up with the CodeBrix.Platform <c>FileSavePicker</c>; heads with no
/// dialog (the Linux framebuffer head) leave it null and the image saves to a default
/// Pictures-folder path instead.
/// </summary>
public interface IFileSaveBridge
{
    /// <summary>
    /// Shows a "save JPEG" dialog seeded with the suggested file name and returns the full
    /// path the user chose, or <c>null</c> if they cancelled.
    /// Signature: <c>Func&lt;suggestedFileName, Task&lt;chosenPathOrNull&gt;&gt;</c>.
    /// </summary>
    Func<string, Task<string>> PickSaveJpegPathAsync { get; set; }
}

/// <summary>
/// Lets the hosting page hand the view model the invalidate (repaint) delegates for the two
/// Skia canvases. Frames and tracking results arrive on capture/worker threads; the page's
/// delegates are responsible for marshalling their invalidates onto the UI thread.
/// </summary>
public interface ICanvasBridge
{
    /// <summary>Invalidates the main canvas (live preview in Capture Mode; the painting in Paint Mode).</summary>
    Action InvalidateMainCanvas { get; set; }

    /// <summary>Invalidates the small self-view canvas shown beside the painting in Paint Mode.</summary>
    Action InvalidateSelfView { get; set; }
}

#if HAS_CODEBRIX
[Microsoft.UI.Xaml.Data.Bindable]
#endif
public class MainViewModel : SimpleViewModel, IFileSaveBridge, ICanvasBridge
{
    private WebcamCaptureService _captureService;
    private HandTracker _tracker;
    private PaintingSession _paintSession;

    private byte[] _visionFrame;

    public MainViewModel()
    {
        if (!IsDesignMode(true))
        {
            Debug.WriteLine("WebcamPainter view model startup.");

            _captureService = new WebcamCaptureService();
            _captureService.FrameArrived += OnFrameArrived;

            StatusText = "Discovering cameras…";
            _ = InitializeAsync();
        }
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

    /// <summary>The capture service - the page's canvases pull live frames from it.</summary>
    public WebcamCaptureService CaptureService => _captureService;

    /// <summary>The Paint Mode session; null while in Capture Mode.</summary>
    public PaintingSession PaintSession => _paintSession;

    #region | Live frames and hand tracking |

    private void OnFrameArrived(object sender, EventArgs e)
    {
        //Capture-thread context: get out fast
        if (!HasFrame)
        {
            InvokeOnMainThread(() => HasFrame = _captureService.HasFrame);
        }

        if (IsCaptureMode)
        {
            InvalidateMainCanvas?.Invoke();
        }
        else
        {
            //Paint Mode: the live feed drives the hand tracker and the little self-view
            HandTracker tracker = _tracker;
            if (tracker != null && tracker.IsRunning
                && _captureService.TryCopyLatestFrame(ref _visionFrame, out int width, out int height))
            {
                tracker.SubmitFrame(_visionFrame, width, height);
            }
            InvalidateSelfView?.Invoke();
        }
    }

    private void OnTrackingUpdated(object sender, HandTrackingEventArgs e)
    {
        //Worker-thread context: marshal all painting decisions onto the UI thread
        HandTrackingResult result = e.Result;
        InvokeOnMainThread(() =>
        {
            PaintingSession session = _paintSession;
            if (IsCaptureMode || session == null) { return; }

            if (result.HandDetected)
            {
                //The preview and the captured still are mirrored, so mirror the hand too
                CrosshairNormX = 1f - result.PalmCenterX;
                CrosshairNormY = result.PalmCenterY;
            }
            else
            {
                CrosshairNormX = null;
                CrosshairNormY = null;
            }

            bool paintNow = result.HandDetected && result.IsOpenPalm;
            IsBrushPainting = paintNow;

            //Strokes are driven in normalized still-image coordinates, so no canvas size is
            //  needed - the drawing space is calibrated from the captured photo.
            if (paintNow)
            {
                if (session.IsStrokeActive)
                {
                    session.ContinueStroke(CrosshairNormX.Value, CrosshairNormY.Value);
                }
                else
                {
                    session.BeginStroke(CrosshairNormX.Value, CrosshairNormY.Value);
                }
            }
            else if (session.IsStrokeActive)
            {
                session.EndStroke();
            }

            InvalidateMainCanvas?.Invoke();
        });
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

    private bool _isCaptureMode = true;
    [AffectsCommands(nameof(TakePhotoCommand), nameof(BackCommand), nameof(ClearCommand),
        nameof(SaveCommand), nameof(SelectColorCommand))]
    public bool IsCaptureMode
    {
        get => _isCaptureMode;
        private set
        {
            SetProperty(ref _isCaptureMode, value);
            NotifyPropertyChanged(nameof(IsPaintMode));
        }
    }

    /// <summary>Paint Mode is simply not-Capture Mode.</summary>
    public bool IsPaintMode => !IsCaptureMode;

    private bool _hasFrame;
    [AffectsCommands(nameof(TakePhotoCommand))]
    public bool HasFrame
    {
        get => _hasFrame;
        private set => SetProperty(ref _hasFrame, value);
    }

    private bool _hasDrawing;
    [AffectsCommands(nameof(SaveCommand), nameof(ClearCommand))]
    public bool HasDrawing
    {
        get => _hasDrawing;
        private set => SetProperty(ref _hasDrawing, value);
    }

    private bool _isBusy;
    [AffectsCommands(nameof(TakePhotoCommand), nameof(BackCommand), nameof(ClearCommand),
        nameof(SaveCommand), nameof(SelectColorCommand))]
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _activeColorText = string.Empty;
    public string ActiveColorText
    {
        get => _activeColorText;
        private set => SetProperty(ref _activeColorText, value ?? string.Empty);
    }

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value ?? string.Empty);
    }

    /// <summary>The hand's horizontal position over the still, 0..1; null when no hand is tracked.
    /// Read by the main canvas's paint handler (not a XAML binding).</summary>
    public float? CrosshairNormX { get; private set; }

    /// <summary>The hand's vertical position over the still, 0..1; null when no hand is tracked.</summary>
    public float? CrosshairNormY { get; private set; }

    /// <summary>Indicates whether the open palm is actively painting right now.</summary>
    public bool IsBrushPainting { get; private set; }

    /// <summary>Set by the hosting head (see <see cref="IFileSaveBridge"/>); null on heads with no file dialog.</summary>
    public Func<string, Task<string>> PickSaveJpegPathAsync { get; set; }

    /// <summary>Set by the hosting page (see <see cref="ICanvasBridge"/>).</summary>
    public Action InvalidateMainCanvas { get; set; }

    /// <summary>Set by the hosting page (see <see cref="ICanvasBridge"/>).</summary>
    public Action InvalidateSelfView { get; set; }

    #endregion

    private void SwitchCamera(CameraDevice camera)
    {
        try
        {
            HasFrame = false;
            if (camera == null)
            {
                _captureService.Stop();
                InvalidateMainCanvas?.Invoke();
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

    #region | Commands and their implementations |

    #region TakePhotoCommand

    private SimpleCommand _takePhotoCommand;
    public SimpleCommand TakePhotoCommand =>
        (_takePhotoCommand ??= new SimpleCommand(CanTakePhoto, DoTakePhoto));

    private bool CanTakePhoto() => (!IsBusy) && IsCaptureMode && HasFrame;

    private async Task DoTakePhoto()
    {
        if (!CanTakePhoto()) { return; }

        IsBusy = true;
        try
        {
            CapturedPhoto photo = _captureService.CapturePhoto();

            //The preview the user was watching is mirrored, so mirror the still to match
            PaintingSession session = await Task.Run(() =>
                PaintingSession.Create(photo.PixelsBgra32, photo.Width, photo.Height, mirrorHorizontally: true));

            session.Session.RedrawRequested += (_, _) => InvalidateMainCanvas?.Invoke();
            session.Session.DrawingChanged += (_, _) =>
                InvokeOnMainThread(() => HasDrawing = _paintSession?.HasStrokes ?? false);

            _paintSession = session;
            HasDrawing = false;
            ActiveColorText = $"Painting with: {session.ActiveColorName}";

            if (_tracker == null)
            {
                _tracker = new HandTracker();
                _tracker.TrackingUpdated += OnTrackingUpdated;
            }
            _tracker.Start();

            IsCaptureMode = false;
            NotifyPropertyChanged(nameof(PaintSession));
            InvalidateMainCanvas?.Invoke();
            StatusText = "Show the camera your open palm to spread paint on the photo - " +
                         "close your hand (or hide it) to stop painting.";
        }
        catch (Exception e)
        {
            StatusText = $"Photo failed: {e.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region BackCommand

    private SimpleCommand _backCommand;
    public SimpleCommand BackCommand =>
        (_backCommand ??= new SimpleCommand(CanGoBack, DoGoBack));

    private bool CanGoBack() => (!IsBusy) && IsPaintMode;

    private async Task DoGoBack()
    {
        if (!CanGoBack()) { return; }

        if (HasDrawing)
        {
            bool discard = await ConfirmDialog(
                "Going back to the camera will discard your painting. Are you sure?",
                "Discard painting?");
            if (!discard) { return; }
        }

        LeavePaintMode();
        StatusText = SelectedCamera != null
            ? $"Live: {SelectedCamera.FriendlyName}"
            : "Select a camera.";
    }

    private void LeavePaintMode()
    {
        _tracker?.Stop();

        PaintingSession session = _paintSession;
        _paintSession = null;
        session?.Dispose();

        HasDrawing = false;
        CrosshairNormX = null;
        CrosshairNormY = null;
        IsBrushPainting = false;
        ActiveColorText = string.Empty;

        IsCaptureMode = true;
        NotifyPropertyChanged(nameof(PaintSession));
        InvalidateMainCanvas?.Invoke();
    }

    #endregion

    #region ClearCommand

    private SimpleCommand _clearCommand;
    public SimpleCommand ClearCommand =>
        (_clearCommand ??= new SimpleCommand(CanClear, DoClear));

    private bool CanClear() => (!IsBusy) && IsPaintMode && HasDrawing;

    private async Task DoClear()
    {
        if (!CanClear()) { return; }

        var doClear = true;
        if (_paintSession.StrokeCount > 2)
        {
            doClear = await ConfirmDialog(
                "Are you sure you want to clear your painting and start over?",
                "Confirm");
        }

        if (doClear)
        {
            _paintSession.Clear();
            StatusText = "Cleared - paint something new.";
        }
    }

    #endregion

    #region SaveCommand

    private SimpleCommand _saveCommand;
    public SimpleCommand SaveCommand =>
        (_saveCommand ??= new SimpleCommand(CanSave, DoSave));

    private bool CanSave() => (!IsBusy) && IsPaintMode && HasDrawing;

    private static string GetSuggestedFileName() => "webcam_painting.jpg";

    private static string GetDefaultSavePath()
    {
        string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        if (String.IsNullOrWhiteSpace(folder))
        {
            folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        return Path.Combine(folder, $"webcam_painting_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
    }

    private async Task DoSave()
    {
        if (!CanSave()) { return; }

        try
        {
            string outputPath;

            if (PickSaveJpegPathAsync == null)
            {
                //No native file dialog on this head (e.g. the Linux framebuffer head) -
                //  save to a default location instead
                outputPath = GetDefaultSavePath();
            }
            else
            {
                outputPath = await PickSaveJpegPathAsync(GetSuggestedFileName());
                if (String.IsNullOrWhiteSpace(outputPath))
                {
                    return; //the user cancelled the dialog
                }
                outputPath = outputPath.Trim();

                //Confirm before clobbering an existing file (the head's own overwrite
                //  prompt is suppressed so this is the single confirmation)
                if (File.Exists(outputPath))
                {
                    bool replace = await ConfirmDialog(
                        $"A file already exists at:\n{outputPath}\n\nDo you want to replace it?",
                        "Replace existing file?");
                    if (!replace)
                    {
                        StatusText = "Save cancelled - the existing file was kept.";
                        return;
                    }
                }
            }

            IsBusy = true;

            byte[] jpeg = _paintSession.ExportJpeg();
            await File.WriteAllBytesAsync(outputPath, jpeg);

            StatusText = $"Saved: {outputPath}";

            bool clearDrawing = await ConfirmDialog(
                $"The painted photo was saved to:\n{outputPath}\n\nDo you want to clear the painting?",
                "Image saved");
            if (clearDrawing)
            {
                _paintSession.Clear();
            }
        }
        catch (NotSupportedException)
        {
            await ShowError("File dialogs are not supported on this head.");
        }
        catch (Exception e)
        {
            StatusText = "Saving failed.";
            await ShowError($"Error while saving the painted photo: {e.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region SelectColorCommand

    private SimpleCommand _selectColorCommand;
    public SimpleCommand SelectColorCommand =>
        (_selectColorCommand ??= new SimpleCommand(CanSelectColor, (Action<object>)DoSelectColor));

    private bool CanSelectColor() => (!IsBusy) && IsPaintMode;

    private void DoSelectColor(object parameter)
    {
        PaintingSession session = _paintSession;
        if (session != null && parameter is string colorName && session.SelectColor(colorName))
        {
            ActiveColorText = $"Painting with: {session.ActiveColorName}";
        }
    }

    #endregion

    #endregion

    #region | IDisposable implementation |

    public override void Dispose()
    {
        _takePhotoCommand?.Dispose();
        _takePhotoCommand = null;
        _backCommand?.Dispose();
        _backCommand = null;
        _clearCommand?.Dispose();
        _clearCommand = null;
        _saveCommand?.Dispose();
        _saveCommand = null;
        _selectColorCommand?.Dispose();
        _selectColorCommand = null;

        PickSaveJpegPathAsync = null;
        InvalidateMainCanvas = null;
        InvalidateSelfView = null;

        if (_tracker != null)
        {
            _tracker.TrackingUpdated -= OnTrackingUpdated;
            _tracker.Dispose();
            _tracker = null;
        }

        PaintingSession session = _paintSession;
        _paintSession = null;
        session?.Dispose();

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
