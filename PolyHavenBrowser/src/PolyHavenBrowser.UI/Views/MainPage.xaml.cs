using CodeBrix.Platform.Simple;
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
    }
}
