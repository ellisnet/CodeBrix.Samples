using Microsoft.UI.Xaml.Controls;
using PolyHavenBrowser.ViewModels;

namespace PolyHavenBrowser.Views;

public sealed partial class MainPage : Page
{
    private MainViewModel ViewModel => DataContext as MainViewModel;

    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            if (DataContext is ICanvasInvalidator invalidator)
            {
                invalidator.InvalidateCanvas = () => DisplayCanvas?.Invalidate();
            }
        };

        InitializeComponent();

        #region | Wire the display canvas to the current scene painter |

        DisplayCanvas.PaintSurface += (_, e) => ViewModel?.CurrentPainter?.Paint(e.Surface, e.Info);

        DisplayCanvas.PointerPressed += (_, e) =>
        {
            var painter = ViewModel?.CurrentPainter;
            if (painter == null) { return; }

            var point = e.GetCurrentPoint(DisplayCanvas);
            if (!point.Properties.IsLeftButtonPressed) { return; }

            painter.PointerDown(point.Position.X, point.Position.Y);
            DisplayCanvas.CapturePointer(e.Pointer);
            e.Handled = true;
        };

        DisplayCanvas.PointerMoved += (_, e) =>
        {
            var painter = ViewModel?.CurrentPainter;
            if (painter == null) { return; }

            var point = e.GetCurrentPoint(DisplayCanvas);
            painter.PointerDrag(point.Position.X, point.Position.Y);
            DisplayCanvas.Invalidate();
        };

        DisplayCanvas.PointerReleased += (_, e) =>
        {
            ViewModel?.CurrentPainter?.PointerUp();
            DisplayCanvas.ReleasePointerCapture(e.Pointer);
        };

        DisplayCanvas.PointerCaptureLost += (_, _) => ViewModel?.CurrentPainter?.PointerUp();

        DisplayCanvas.PointerWheelChanged += (_, e) =>
        {
            var painter = ViewModel?.CurrentPainter;
            if (painter == null) { return; }

            var delta = e.GetCurrentPoint(DisplayCanvas).Properties.MouseWheelDelta;
            painter.Zoom(delta);
            DisplayCanvas.Invalidate();
            e.Handled = true;
        };

        DisplayCanvas.SizeChanged += (_, _) => DisplayCanvas.Invalidate();

        #endregion
    }
}
