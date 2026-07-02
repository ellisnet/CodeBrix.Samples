using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;
using System;
using WikipediaPublisher.ViewModels;

namespace WikipediaPublisher.WinUi.Views;

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
        };

        InitializeComponent();

        Browser.NavigationCompleted += (sender, args) =>
            (DataContext as IWebViewBridge)?.SetCurrentBrowserUrl(Browser.Source?.AbsoluteUri);
    }
}
