using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;
using System; //Required: the IAsyncOperation GetAwaiter extension (awaiting the FileSavePicker) lives here
using System.Collections.Generic;
using System.Threading.Tasks;
using WebcamPainter.Helpers;
using WebcamPainter.Painting;
using WebcamPainter.ViewModels;
using WebcamPainter.Webcam;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace WebcamPainter.Views;

public sealed partial class MainPage : Page
{
    private MainViewModel ViewModel => DataContext as MainViewModel;

    //One frame renderer per canvas that shows live video (each caches its own buffers)
    private readonly WebcamFrameRenderer _mainRenderer = new WebcamFrameRenderer();
    private readonly WebcamFrameRenderer _selfViewRenderer = new WebcamFrameRenderer();

    public MainPage()
    {
        //Doing this before InitializeComponent() - in case InitializeComponent()
        //  is the thing that sets the data context.
        DataContextChanged += (_, _) =>
        {
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

            if (DataContext is IFileSaveBridge fileSave)
            {
                fileSave.PickSaveJpegPathAsync = PickSaveJpegPathAsync;
            }

            if (DataContext is ICanvasBridge canvasBridge)
            {
                //Frames and tracking results arrive on capture/worker threads - marshal
                //  the repaints onto the UI thread
                canvasBridge.InvalidateMainCanvas = () => DispatcherQueue?.TryEnqueue(() => MainCanvas?.Invalidate());
                canvasBridge.InvalidateSelfView = () => DispatcherQueue?.TryEnqueue(() => SelfViewCanvas?.Invalidate());
                canvasBridge.GetMainCanvasSize = () => ((float)MainCanvas.ActualWidth, (float)MainCanvas.ActualHeight);
            }
        };

        InitializeComponent();

        MainCanvas.PaintSurface += (_, e) =>
        {
            var viewModel = ViewModel;
            if (viewModel == null) { return; }

            if (viewModel.IsPaintMode && viewModel.PaintSession != null)
            {
                PaintCanvasHelper.Render(e.Surface, e.Info, viewModel.PaintSession,
                    viewModel.CrosshairNormX, viewModel.CrosshairNormY, viewModel.IsBrushPainting);
            }
            else
            {
                _mainRenderer.Render(e.Surface, e.Info, viewModel.CaptureService, mirror: true);
            }
        };

        SelfViewCanvas.PaintSurface += (_, e) =>
            _selfViewRenderer.Render(e.Surface, e.Info, ViewModel?.CaptureService, mirror: true);

        MainCanvas.SizeChanged += (_, _) => MainCanvas.Invalidate();
        SelfViewCanvas.SizeChanged += (_, _) => SelfViewCanvas.Invalidate();
    }

    private static async Task<string> PickSaveJpegPathAsync(string suggestedFileName)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            SuggestedFileName = suggestedFileName,
            DefaultFileExtension = ".jpg"
        };
        picker.FileTypeChoices.Add("JPEG image", new List<string> { ".jpg" });

        StorageFile file = await picker.PickSaveFileAsync();
        if (file == null) { return null; }

        FileDialogHelper.RemoveEmptyPlaceholder(file.Path);
        return file.Path;
    }
}
