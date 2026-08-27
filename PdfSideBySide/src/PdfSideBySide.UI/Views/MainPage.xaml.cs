using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;
using PdfSideBySide.PdfRender;
using PdfSideBySide.ViewModels;
using System;

namespace PdfSideBySide.Views;

public sealed partial class MainPage : Page
{
    private bool _isWiredToViewModel;

    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

            //The view (zoom + pan) and each pane's image are laid out here, because only the
            //  page knows how big the viewers are
            if (DataContext is MainViewModel viewModel && !_isWiredToViewModel)
            {
                _isWiredToViewModel = true;
                viewModel.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.ViewVersion)) { ApplyViews(); }
                };
                viewModel.LeftPane.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(DocumentPaneViewModel.PageImage)) { ApplyView(DocumentSide.Left); }
                };
                viewModel.RightPane.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(DocumentPaneViewModel.PageImage)) { ApplyView(DocumentSide.Right); }
                };
            }
        };

        this.InitializeComponent(); //Leave this line last

        LeftScroller.SizeChanged += (_, _) => ApplyView(DocumentSide.Left);
        RightScroller.SizeChanged += (_, _) => ApplyView(DocumentSide.Right);
    }

    private MainViewModel ViewModel => DataContext as MainViewModel;

    private void ApplyViews()
    {
        ApplyView(DocumentSide.Left);
        ApplyView(DocumentSide.Right);
    }

    /// <summary>
    /// Sizes side's image to zoom x fit-the-page (so 100% shows the whole page, centred, and
    /// every level above it overflows the viewer) and scrolls the viewer to the pane's pan position.
    /// </summary>
    private void ApplyView(DocumentSide side)
    {
        var viewModel = ViewModel;
        if (viewModel == null) { return; }

        var pane = side == DocumentSide.Left ? viewModel.LeftPane : viewModel.RightPane;
        var scroller = side == DocumentSide.Left ? LeftScroller : RightScroller;
        var image = side == DocumentSide.Left ? LeftImage : RightImage;

        var viewportWidth = scroller.ActualWidth;
        var viewportHeight = scroller.ActualHeight;
        if (pane.PagePixelWidth <= 0 || pane.PagePixelHeight <= 0 || viewportWidth <= 0 || viewportHeight <= 0)
        {
            image.Width = double.NaN;
            image.Height = double.NaN;
            return;
        }

        var fit = Math.Min(viewportWidth / pane.PagePixelWidth, viewportHeight / pane.PagePixelHeight);
        var factor = viewModel.View.Zoom.Factor;
        image.Width = Math.Floor(pane.PagePixelWidth * fit * factor);
        image.Height = Math.Floor(pane.PagePixelHeight * fit * factor);

        //Let the viewer measure the new extent before positioning it
        scroller.UpdateLayout();
        var pan = viewModel.View.PanOf(side);
        scroller.ChangeView(
            pan.Horizontal * Math.Max(0, scroller.ScrollableWidth),
            pan.Vertical * Math.Max(0, scroller.ScrollableHeight),
            null, disableAnimation: true);
    }
}
