using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WikipediaPublisher.ViewModels;

namespace WikipediaPublisher.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContextChanged += (sender, args) =>
        {
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

            //Native WPF "Save PDF as…" dialog, consistent with the Skia and WinUI heads
            if (DataContext is IFileSaveBridge fileSave)
            {
                fileSave.PickSavePdfPathAsync = PickSavePdfPathAsync;
            }
        };

        InitializeComponent();

        //Use CoreWebView2.Source (the authoritative current URL after redirects / user
        //  navigation); the WebView2.Source property does not reliably reflect those.
        Browser.NavigationCompleted += (sender, args) =>
            (DataContext as IWebViewBridge)?.SetCurrentBrowserUrl(
                Browser.CoreWebView2?.Source ?? Browser.Source?.AbsoluteUri);
    }

    private Task<string> PickSavePdfPathAsync(string suggestedFileName)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Save PDF as",
            Filter = "PDF document (*.pdf)|*.pdf|All files (*.*)|*.*",
            DefaultExt = ".pdf",
            AddExtension = true,
            FileName = suggestedFileName,
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            //The app does its own "replace existing file?" prompt (via SimpleDialog) at publish
            //  time, so suppress the dialog's built-in overwrite prompt to avoid a double prompt.
            OverwritePrompt = false
        };

        var chosen = dialog.ShowDialog(this) == true ? dialog.FileName : null;
        return Task.FromResult(chosen);
    }
}
