using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using WikipediaPublisher.ViewModels;

namespace WikipediaPublisher.WinUI.Views;

/// <summary>
/// The main (and only) page of the WinUI head.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainPage()
    {
        //Doing this before InitializeComponent() - in case InitializeComponent()
        //  is the thing that sets the data context.
        DataContextChanged += (sender, args) =>
        {
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

            if (DataContext is IWebViewBridge bridge)
            {
                bridge.NavigateToUrl = url =>
                {
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        Browser.Source = new Uri(url);
                    }
                };
            }

            //Native WinUI "Save PDF as…" dialog, consistent with the Skia heads
            if (DataContext is IFileSaveBridge fileSave)
            {
                fileSave.PickSavePdfPathAsync = PickSavePdfPathAsync;
            }
        };

        InitializeComponent();

        //Use CoreWebView2.Source (the authoritative current URL after redirects / user
        //  navigation); the XAML Browser.Source property does not reliably reflect those.
        Browser.NavigationCompleted += (sender, args) =>
            (DataContext as IWebViewBridge)?.SetCurrentBrowserUrl(
                sender.CoreWebView2?.Source ?? Browser.Source?.AbsoluteUri);
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

    private static Task<string> PickSavePdfPathAsync(string suggestedFileName)
    {
        //Use the Win32 "Save As" dialog with its overwrite prompt turned off (see
        //  Win32SaveFileDialog) so choosing an existing file is silent — the app's own
        //  publish-time "replace existing file?" prompt is the single point of confirmation,
        //  matching the WPF head. The WinRT FileSavePicker can't suppress its own prompt.
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.CurrentWindow);
        var path = Win32SaveFileDialog.PickSavePath(hwnd, suggestedFileName, "Save PDF as");
        return Task.FromResult(path);
    }
}
