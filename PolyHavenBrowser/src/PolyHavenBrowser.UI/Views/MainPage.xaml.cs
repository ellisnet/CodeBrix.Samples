using System.Threading.Tasks;
using CodeBrix.Platform.Simple;
using CodeBrix.Platform.WinUI.Graphics3DGL;
using Microsoft.UI.Xaml.Controls;
using PolyHavenBrowser.ViewModels;

namespace PolyHavenBrowser.Views;

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

                    //The user just opened the Model View: if the GL canvas already knows its
                    //OpenGL initialization failed, tell them why the preview pane is empty.
                    if (args.PropertyName == nameof(MainViewModel.IsModelViewActive))
                    {
                        _ = MaybeReportRenderingUnavailableAsync();
                    }
                };
            }
        };

        InitializeComponent();

        //The canvas may only attempt its OpenGL initialization when it loads into the visual
        //tree, which can happen after IsModelViewActive is set - so check at both moments.
        ModelCanvas.Loaded += (_, _) => _ = MaybeReportRenderingUnavailableAsync();

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
    }

    //When the Model View is active and the preview canvas reports failed OpenGL initialization,
    //surface the failure (status + reason) in a dialog instead of leaving a silently empty pane.
    private async Task MaybeReportRenderingUnavailableAsync()
    {
        if (_renderingUnavailableReported || ViewModel is not { IsModelViewActive: true } viewModel)
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
