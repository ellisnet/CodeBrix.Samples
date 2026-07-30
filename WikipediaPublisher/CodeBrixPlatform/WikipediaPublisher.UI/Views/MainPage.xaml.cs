using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WikipediaPublisher.Helpers;
using WikipediaPublisher.ViewModels;
using Windows.Storage.Pickers;

namespace WikipediaPublisher.Views;

public sealed partial class MainPage : Page
{
    private bool _browserInitialized;

    public MainPage()
    {
        //Doing this before InitializeComponent() - in case InitializeComponent()
        //  is the thing that sets the data context.
        DataContextChanged += (_, _) =>
        {
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

            //Give the view model a native "Save PDF as…" file dialog (CodeBrix.Platform's
            //  FileSavePicker). Heads with no windowing system (Linux framebuffer) throw
            //  NotSupportedException from the picker; the view model handles that.
            if (DataContext is IFileSaveBridge fileSave)
            {
                fileSave.PickSavePdfPathAsync = PickSavePdfPathAsync;
            }
        };

        InitializeComponent();

        Loaded += (_, _) => InitializeBrowser();
    }

    private void InitializeBrowser()
    {
        if (_browserInitialized || DataContext is not MainViewModel viewModel) { return; }
        _browserInitialized = true;

        //Use CoreWebView2.Source (the authoritative current URL after redirects / user
        //  navigation); the XAML Browser.Source property does not reliably reflect those.
        Browser.NavigationCompleted += (_, _) =>
            viewModel.SetCurrentBrowserUrl(Browser.CoreWebView2?.Source ?? Browser.Source?.AbsoluteUri);

        viewModel.NavigateToUrl = url =>
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                Browser.Source = new Uri(url);
            }
        };

        Browser.Source = new Uri(MainViewModel.HomeUrl);
    }

    //Pressing Enter in the search box runs Search, just like clicking the button.
    private void SearchBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter
            && DataContext is MainViewModel { SearchCommand: var search }
            && search.CanExecute(null))
        {
            search.Execute(null);
            e.Handled = true;
        }
    }

    private static async Task<string> PickSavePdfPathAsync(string suggestedFileName)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = suggestedFileName,
            DefaultFileExtension = ".pdf"
        };
        picker.FileTypeChoices.Add("PDF document", new List<string> { ".pdf" });

        var file = await picker.PickSaveFileAsync();
        if (file == null) { return null; }

        //Some heads percent-encode the path they return, which would save "My Article.pdf" as
        //  "My%20Article.pdf"; decode it before anything touches the disk.
        var path = FileDialogHelper.ToFileSystemPath(file.Path);

        FileDialogHelper.RemoveEmptyPlaceholder(path);
        return path;
    }
}
