using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;
using PalmVisualizer.Camera;
using PalmVisualizer.ViewModels;

namespace PalmVisualizer.Views;

public sealed partial class MainPage : Page
{
    private MainViewModel ViewModel => DataContext as MainViewModel;

    private IManageGameCanvas _gameCanvasManager;

    //One frame renderer for the live-preview canvas (it caches its own buffers)
    private readonly WebcamFrameRenderer _previewRenderer = new WebcamFrameRenderer();

    public MainPage()
    {
        //Doing this before InitializeComponent() - in case InitializeComponent()
        //  is the thing that sets the data context.
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);
            _gameCanvasManager = DataContext as IManageGameCanvas;

            if (DataContext is ICanvasBridge canvasBridge)
            {
                //Frames arrive on the capture thread - marshal the repaints onto the UI thread
                canvasBridge.InvalidatePreviewCanvas =
                    () => DispatcherQueue?.TryEnqueue(() => PreviewCanvas?.Invalidate());
            }
        };

        this.InitializeComponent();

        PreviewCanvas.PaintSurface += (_, e) =>
            _previewRenderer.Render(e.Surface, e.Info, ViewModel?.CaptureService, mirror: true);
        PreviewCanvas.SizeChanged += (_, _) => PreviewCanvas.Invalidate();

        //Fires once, at the canvas's first non-zero layout size - i.e. the first time
        //  Visualize Mode is shown - which is when the engine can start
        VisualizerCanvas.FirstStarted += (_, _) => _gameCanvasManager?.CanvasFirstStart(VisualizerCanvas);
    }
}
