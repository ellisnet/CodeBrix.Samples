using CodeBrix.Platform.Simple;
using JustBetweenUs.ViewModels;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;

// ReSharper disable once CheckNamespace
namespace JustBetweenUs.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        //Doing this before InitializeComponent() - in case InitializeComponent()
        //  is the thing that sets the binding context.
        BindingContextChanged += (sender, args) =>
        {
            (BindingContext as IXamlRootGetter)?.SetXamlRootGetter(() => this);

            if (BindingContext is ICopyToClipboard copy)
            {
                copy.CopyTextToClipboard = (text) =>
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        Clipboard.Default.SetTextAsync(text); //Not necessary to await this
                    }
                };
            }
        };

        //The view model waits for this before it shows its startup dialog: a dialog needs a page to
        //  attach to, and the page is not on screen until it has loaded.
        Loaded += (sender, args) => (BindingContext as IPageReadyNotifier)?.NotifyPageReady();

        InitializeComponent();
    }
}
