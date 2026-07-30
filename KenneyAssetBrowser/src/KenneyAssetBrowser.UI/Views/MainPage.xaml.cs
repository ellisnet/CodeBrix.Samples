using CodeBrix.Platform.Simple;
using CodeBrix.Platform.UI.FlexPanel;
using CodeBrix.Platform.WinUI.Graphics3DGL;
using KenneyAssetBrowser.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace KenneyAssetBrowser.Views;

public sealed partial class MainPage : Page
{
    private MainViewModel ViewModel => DataContext as MainViewModel;

    //Whether the "3D preview unavailable" dialog has been shown already (once per app run).
    private bool _renderingUnavailableReported;

    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            //(e.g. the bundle license dialog).
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

            if (DataContext is MainViewModel viewModel)
            {
                //Marshal 2D-canvas invalidations from the view model onto the UI thread
                viewModel.InvalidateImageCanvas = () => DispatcherQueue?.TryEnqueue(() => ImageCanvas?.Invalidate());

                //Audio bridge: the view model hands over decoded WAV streams and transport
                //calls; the AudioPlayer element does the playing (it takes stream ownership)
                viewModel.LoadAudioSource = stream => AudioElement?.SetSourceStream(stream);
                viewModel.PlayAudio = () => AudioElement?.Play();
                viewModel.PauseAudio = () => AudioElement?.Pause();
                viewModel.StopAudio = () => AudioElement?.Stop();
                viewModel.SetAudioLooping = looping =>
                {
                    if (AudioElement != null) { AudioElement.IsLooping = looping; }
                };

                viewModel.PropertyChanged += (_, args) =>
                {
                    //A new cell collection means the user switched bundle, searched or
                    //re-filtered: jump back to the top.
                    if (args.PropertyName == nameof(MainViewModel.Cells))
                    {
                        CatalogScroll.ChangeView(null, 0, null, disableAnimation: true);
                    }

                    //The user just opened the Viewer View: if the GL canvas already knows its
                    //OpenGL initialization failed, tell them why the preview pane is empty.
                    if (args.PropertyName == nameof(MainViewModel.IsViewerActive))
                    {
                        _ = MaybeReportRenderingUnavailableAsync();
                    }
                };
            }
        };

        InitializeComponent();

        //The canvas may only attempt its OpenGL initialization when it loads into the visual
        //tree, which can happen after IsViewerActive is set - so check at both moments.
        ModelCanvas.Loaded += (_, _) => _ = MaybeReportRenderingUnavailableAsync();

        //The 2D viewer: the view model's painter draws images and spritesheets (checkerboard,
        //zoom, sprite spotlight) onto this SkiaSharp surface.
        ImageCanvas.PaintSurface += (_, e) =>
            ViewModel?.ImagePainter.Paint(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        ImageCanvas.SizeChanged += (_, _) => ImageCanvas.Invalidate();
        ImageCanvas.PointerWheelChanged += (_, e) =>
        {
            var delta = e.GetCurrentPoint(ImageCanvas).Properties.MouseWheelDelta;
            ViewModel?.AdjustZoomFromWheel(delta);
            e.Handled = true;
        };

        //The Viewer View's content panes: side-by-side while the window is landscape. In
        //portrait the FlexPanel's main axis flips so the viewer drops below the facts pane,
        //which trades its fixed-width column for half the height as a flex basis.
        SizeChanged += (_, args) =>
        {
            var portrait = args.NewSize.Width < args.NewSize.Height;
            ViewerContentFlex.Direction = portrait ? FlexDirection.Column : FlexDirection.Row;
            ViewerInfoPane.Width = portrait ? double.NaN : 380;
            FlexPanel.SetBasis(ViewerInfoPane,
                portrait ? new FlexBasis(0.5f, isRelative: true) : FlexBasis.Auto);
            ViewerInfoPane.Margin = portrait ? new Thickness(0, 0, 0, 20) : new Thickness(0, 0, 20, 0);
        };

        //Lazy grid loading: as the grid scrolls within two screens of its bottom edge,
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
    }

    //When the Viewer View is active and the preview canvas reports failed OpenGL initialization,
    //surface the failure (status + reason) in a dialog instead of leaving a silently empty pane.
    private async Task MaybeReportRenderingUnavailableAsync()
    {
        if (_renderingUnavailableReported || ViewModel is not { IsViewerActive: true } viewModel)
        {
            return;
        }

        var state = ModelCanvas.GetGLInitializationState();
        if (state.Status == GLInitializationStatus.InitializationFailed)
        {
            _renderingUnavailableReported = true;
            await viewModel.ShowRenderingUnavailableAsync(state);
        }
    }
}
