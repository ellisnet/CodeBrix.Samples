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
            //(e.g. the "Vulkan rendering is not available on this platform." alert).
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);
        };

        InitializeComponent();

        #region | Wire the display canvas to the current scene painter |

        DisplayCanvas.PaintSurface += (_, e) =>
        {
            _renderPending = false;
            ViewModel?.CurrentPainter?.Paint(e.Surface, e.Info);
        };

        DisplayCanvas.PointerPressed += (_, e) =>
        {
            var painter = ViewModel?.CurrentPainter;
            if (painter == null) { return; }

            var point = e.GetCurrentPoint(DisplayCanvas);
            if (!point.Properties.IsLeftButtonPressed) { return; }

            var (x, y) = ToCanvasPixels(point.Position);
            painter.PointerDown(x, y);
            _gestureStartTimestamp = point.Timestamp;
            _gestureClock.Restart();
            DisplayCanvas.CapturePointer(e.Pointer);
            RequestRender();
            e.Handled = true;
        };

        DisplayCanvas.PointerMoved += (_, e) =>
        {
            var painter = ViewModel?.CurrentPainter;
            if (painter == null) { return; }

            var point = e.GetCurrentPoint(DisplayCanvas);
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
            //  otherwise drag/manipulate the window instead of orbiting the scene).
            e.Handled = true;
        };

        DisplayCanvas.PointerReleased += (_, e) =>
        {
            ViewModel?.CurrentPainter?.PointerUp();
            _gestureClock.Reset();
            DisplayCanvas.ReleasePointerCapture(e.Pointer);
            RequestRender();   // redraw once at full (non-drag) resolution
            e.Handled = true;
        };

        DisplayCanvas.PointerCaptureLost += (_, _) =>
        {
            ViewModel?.CurrentPainter?.PointerUp();
            _gestureClock.Reset();
            RequestRender();
        };

        DisplayCanvas.PointerWheelChanged += (_, e) =>
        {
            var painter = ViewModel?.CurrentPainter;
            if (painter == null) { return; }

            var delta = e.GetCurrentPoint(DisplayCanvas).Properties.MouseWheelDelta;
            painter.Zoom(delta);
            RequestRender();
            e.Handled = true;
        };

        DisplayCanvas.SizeChanged += (_, _) => RequestRender();

        #endregion
    }

    //Coalesced repaint: request at most one pending paint at a time.
    private void RequestRender()
    {
        if (_renderPending) { return; }
        _renderPending = true;
        DisplayCanvas?.Invalidate();
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
    // input stays aligned with the rendered pixels at any DPI and after any window resize -
    // the coordinate robustness the PainDiagram sample demonstrates.
    private (double X, double Y) ToCanvasPixels(Point position)
    {
        var canvasSize = DisplayCanvas.CanvasSize;
        var scaleX = DisplayCanvas.ActualWidth > 0 && canvasSize.Width > 0
            ? canvasSize.Width / DisplayCanvas.ActualWidth : 1.0;
        var scaleY = DisplayCanvas.ActualHeight > 0 && canvasSize.Height > 0
            ? canvasSize.Height / DisplayCanvas.ActualHeight : 1.0;
        return (position.X * scaleX, position.Y * scaleY);
    }
}
