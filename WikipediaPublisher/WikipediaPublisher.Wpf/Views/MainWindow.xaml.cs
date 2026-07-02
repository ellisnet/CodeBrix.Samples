using System;
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
        };

        InitializeComponent();

        Browser.NavigationCompleted += (sender, args) =>
            (DataContext as IWebViewBridge)?.SetCurrentBrowserUrl(Browser.Source?.AbsoluteUri);
    }
}
