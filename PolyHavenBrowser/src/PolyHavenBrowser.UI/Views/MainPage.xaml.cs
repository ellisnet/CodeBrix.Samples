using System.Diagnostics;
using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;
using PolyHavenBrowser.ViewModels;
using Windows.Foundation;

namespace PolyHavenBrowser.Views;

public sealed partial class MainPage : Page
{
    //A pointer frame that is running more than this far behind real time is a backlog
    //frame: keep the cursor anchor in sync but skip rendering it, catching up to the latest.
    private const double StaleFrameMicroseconds = 1_000_000; // 1 second

    private MainViewModel ViewModel => DataContext as MainViewModel;

    //Coalescing: never queue more than one paint. While one is pending, pointer moves only
    //update the camera; the next paint draws the latest state.
    private bool _renderPending;

    //Tracks how far behind real time the pointer stream is, to detect a backlog.
    private readonly Stopwatch _gestureClock = new();
    private double _gestureStartTimestamp;

    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            if (DataContext is ICanvasInvalidator invalidator)
            {
                invalidator.InvalidateCanvas = RequestRender;
            }

            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            //(e.g. the "choose a download folder first" alert).
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

            //A new cell collection means the user re-searched or re-sorted: jump back to the top.
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.Cells))
                    {
                        CatalogScroll.ChangeView(null, 0, null, disableAnimation: true);
                    }
                };
            }
        };

        InitializeComponent();

        //Lazy catalog loading: as the grid scrolls within two screens of its bottom edge,
        //ask the cell collection to materialize the next batch.
        CatalogScroll.ViewChanged += (_, _) =>
        {
            var cells = ViewModel?.Cells;
            if (cells == null || !cells.HasMoreItems) { return; }

            var remaining = CatalogScroll.ExtentHeight - CatalogScroll.VerticalOffset - CatalogScroll.ViewportHeight;
            if (remaining < CatalogScroll.ViewportHeight * 2)
            {
                cells.RequestMore(24);
            }
        };

        #region | Wire the Model View's canvas to the scene painter |

        ModelCanvas.PaintSurface += (_, e) =>
        {
            _renderPending = false;
            ViewModel?.CurrentPainter?.Paint(e.Surface, e.Info);
        };

        ModelCanvas.PointerPressed += (_, e) =>
        {
            var painter = ViewModel?.CurrentPainter;
            if (painter == null) { return; }

            var point = e.GetCurrentPoint(ModelCanvas);
            if (!point.Properties.IsLeftButtonPressed) { return; }

            var (x, y) = ToCanvasPixels(point.Position);
            painter.PointerDown(x, y);
            _gestureStartTimestamp = point.Timestamp;
            _gestureClock.Restart();
            ModelCanvas.CapturePointer(e.Pointer);
            RequestRender();
            e.Handled = true;
        };

        ModelCanvas.PointerMoved += (_, e) =>
        {
            var painter = ViewModel?.CurrentPainter;
            if (painter == null) { return; }

            var point = e.GetCurrentPoint(ModelCanvas);
            var (x, y) = ToCanvasPixels(point.Position);

            if (IsBacklogFrame(point.Timestamp))
            {
                //Discard this stale frame: stay aligned with the cursor but don't render it.
                painter.PointerSkip(x, y);
            }
            else
            {
                painter.PointerDrag(x, y);
                RequestRender();
            }

            //Handle the move so it doesn't bubble to the window manager (which would
            //  otherwise drag/manipulate the window instead of orbiting the model).
            e.Handled = true;
        };

        ModelCanvas.PointerReleased += (_, e) =>
        {
            ViewModel?.CurrentPainter?.PointerUp();
            _gestureClock.Reset();
            ModelCanvas.ReleasePointerCapture(e.Pointer);
            RequestRender();   // redraw once at full (non-drag) resolution
            e.Handled = true;
        };

        ModelCanvas.PointerCaptureLost += (_, _) =>
        {
            ViewModel?.CurrentPainter?.PointerUp();
            _gestureClock.Reset();
            RequestRender();
        };

        ModelCanvas.PointerWheelChanged += (_, e) =>
        {
            var painter = ViewModel?.CurrentPainter;
            if (painter == null) { return; }

            var delta = e.GetCurrentPoint(ModelCanvas).Properties.MouseWheelDelta;
            painter.Zoom(delta);
            RequestRender();
            e.Handled = true;
        };

        ModelCanvas.SizeChanged += (_, _) => RequestRender();

        #endregion
    }

    //Coalesced repaint: request at most one pending paint at a time.
    private void RequestRender()
    {
        if (_renderPending) { return; }
        _renderPending = true;
        ModelCanvas?.Invalidate();
    }

    //True when this pointer frame is running far enough behind real time to be a backlog
    //frame that should be dropped rather than rendered.
    private bool IsBacklogFrame(ulong timestamp)
    {
        if (!_gestureClock.IsRunning) { return false; }
        var inputElapsed = timestamp - _gestureStartTimestamp;
        var lag = _gestureClock.Elapsed.TotalMicroseconds - inputElapsed;
        return lag > StaleFrameMicroseconds;
    }

    // Maps a pointer position (in view/DIP units) to the canvas's pixel space, so pointer
    // input stays aligned with the rendered pixels at any DPI and after any window resize.
    private (double X, double Y) ToCanvasPixels(Point position)
    {
        var canvasSize = ModelCanvas.CanvasSize;
        var scaleX = ModelCanvas.ActualWidth > 0 && canvasSize.Width > 0
            ? canvasSize.Width / ModelCanvas.ActualWidth : 1.0;
        var scaleY = ModelCanvas.ActualHeight > 0 && canvasSize.Height > 0
            ? canvasSize.Height / ModelCanvas.ActualHeight : 1.0;
        return (position.X * scaleX, position.Y * scaleY);
    }
}
