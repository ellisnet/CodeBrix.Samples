using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CodeBrix.Platform.Simple;
using GoddessTempleDiscovery.Helpers;
using GoddessTempleDiscovery.Services;
using GoddessTempleDiscovery.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Storage.Pickers;

namespace GoddessTempleDiscovery.Views;

/// <summary>
/// The one page: the game table on a full-window surface, with the Art Deco panes and the newspaper inspector layered
/// over it. The code-behind keeps to the canvas wiring, the save picker, the inspector's slide and pointer, and the
/// XamlRoot getter.
/// </summary>
public sealed partial class MainPage : Page
{
    private IManageGameCanvas _gameCanvasManager;
    private INotifyPropertyChanged _notifier;

    /// <summary>Creates the page.</summary>
    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

            //Through the interfaces the view model implements, never through its concrete type
            _gameCanvasManager = DataContext as IManageGameCanvas;
            if (DataContext is IJournalFileBridge files)
            {
                files.PickSavePathAsync = PickSavePathAsync;
            }

            if (_notifier != null)
            {
                _notifier.PropertyChanged -= OnViewModelPropertyChanged;
            }

            _notifier = DataContext as INotifyPropertyChanged;
            if (_notifier != null)
            {
                _notifier.PropertyChanged += OnViewModelPropertyChanged;
            }
        };

        this.InitializeComponent(); //Leave this line last (before the canvas wiring below)

        //Keys the game surface did not take (it has no focus while a pane is open): the gallery's Escape and arrows
        KeyDown += (_, e) =>
        {
            if (!e.Handled && _gameCanvasManager != null && _gameCanvasManager.HandleKey(e.Key))
            {
                e.Handled = true;
            }
        };

        GameCanvas.FirstStarted += (_, _) =>
        {
            _gameCanvasManager?.CanvasFirstStart(GameCanvas);

            //The window was activated before the game existed, so hand the canvas keyboard focus once here (later
            //  activations are the engine's GameWindowLifecycle). Deferred so it lands after start-up finishes.
            DispatcherQueue.TryEnqueue(() => GameCanvas.Focus(FocusState.Programmatic));
        };
    }

    //The newspaper slides in from the top each time it opens
    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "IsInspectorOpen" || Newspaper.Visibility != Visibility.Visible)
        {
            return;
        }

        var slide = new DoubleAnimation
        {
            From = -760,
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(380)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(slide, NewspaperSlide);
        Storyboard.SetTargetProperty(slide, "Y");
        var storyboard = new Storyboard();
        storyboard.Children.Add(slide);
        storyboard.Begin();
    }

    private void OnNewspaperPointerEntered(object sender, PointerRoutedEventArgs e) => _gameCanvasManager?.InspectorPointerOver(true);

    private void OnNewspaperPointerExited(object sender, PointerRoutedEventArgs e) => _gameCanvasManager?.InspectorPointerOver(false);

    private static async Task<string> PickSavePathAsync(string suggestedFileName, string typeName, string extension)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = suggestedFileName,
            DefaultFileExtension = extension,
        };
        picker.FileTypeChoices.Add(typeName, new System.Collections.Generic.List<string> { extension });

        var file = await picker.PickSaveFileAsync();
        return file == null ? null : FileDialogHelper.ToFileSystemPath(file.Path);
    }
}
