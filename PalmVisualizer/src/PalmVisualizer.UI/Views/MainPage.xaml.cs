using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;
using PalmVisualizer.Camera;
using PalmVisualizer.Rendering;
using PalmVisualizer.ViewModels;

namespace PalmVisualizer.Views;

public sealed partial class MainPage : Page, IVisualizerCanvasHost
{
    //Where the preview renderer pulls frames from - the view model, seen as a frame source
    private IWebcamFrameSource _frameSource;

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
            _frameSource = DataContext as IWebcamFrameSource;

            //The panel the visualizer sessions show their views in
            (DataContext as IManageVisualizerCanvas)?.SetVisualizerCanvasHost(this);

            if (DataContext is ICanvasBridge canvasBridge)
            {
                //Frames arrive on the capture thread - marshal the repaints onto the UI thread
                canvasBridge.InvalidatePreviewCanvas =
                    () => DispatcherQueue?.TryEnqueue(() => PreviewCanvas?.Invalidate());
            }
        };

        this.InitializeComponent();

        PreviewCanvas.PaintSurface += (_, e) =>
            _previewRenderer.Render(e.Surface, e.Info, _frameSource, mirror: true);
        PreviewCanvas.SizeChanged += (_, _) => PreviewCanvas.Invalidate();
    }

    //The one thing a visualizer session needs from this page (see IVisualizerCanvasHost)
    Panel IVisualizerCanvasHost.Container => VisualizerCanvas;
}
