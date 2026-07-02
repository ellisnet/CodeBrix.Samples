using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using WikipediaPublisher.Helpers;
using WikipediaPublisher.ViewModels;

namespace WikipediaPublisher.Views;

public sealed partial class MainPage : Page
{
    private bool _browserInitialized;

    public MainPage()
    {
        //Doing this before InitializeComponent() - in case InitializeComponent()
        //  is the thing that sets the data context.
        DataContextChanged += (sender, args) =>
        {
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);
        };

        InitializeComponent();

        Loaded += (_, _) => InitializeBrowserArea();
    }

    private void InitializeBrowserArea()
    {
        if (_browserInitialized || DataContext is not MainViewModel viewModel) { return; }
        _browserInitialized = true;

        if (AppCapabilities.HasWebView)
        {
            //Create the WebView2 in code (rather than XAML) so that heads without
            //  WebView support never instantiate the control
            var webView = new WebView2();
            BrowserHost.Children.Add(webView);

            webView.NavigationCompleted += (sender, args) =>
                viewModel.SetCurrentBrowserUrl(webView.Source?.AbsoluteUri);

            viewModel.NavigateToUrl = url =>
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    webView.Source = new Uri(url);
                }
            };

            webView.Source = new Uri(MainViewModel.HomeUrl);
            FallbackPane.Visibility = Visibility.Collapsed;
        }
        else
        {
            FallbackPane.Visibility = Visibility.Visible;
        }
    }
}
