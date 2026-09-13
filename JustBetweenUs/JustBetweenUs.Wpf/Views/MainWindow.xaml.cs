using JustBetweenUs.ViewModels;
using System.Windows;

namespace JustBetweenUs.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        DataContextChanged += (sender, args) =>
        {
            if (DataContext is ICopyToClipboard copy)
            {
                copy.CopyTextToClipboard = Clipboard.SetText;
            }
        };

        //The view model waits for this before it shows its startup dialog
        Loaded += (sender, args) => (DataContext as IPageReadyNotifier)?.NotifyPageReady();

        InitializeComponent();
    }
}
