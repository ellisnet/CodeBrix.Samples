using BrixInvaders.ViewModels;
using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BrixInvaders.Views;

/// <summary>The one page: a full-window game surface. The view model owns the game host.</summary>
public sealed partial class MainPage : Page
{
    private IManageGameCanvas _gameCanvasManager;

    /// <summary>Creates the page.</summary>
    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

            //Through the interface the view model implements, never through its concrete type
            _gameCanvasManager = DataContext as IManageGameCanvas;
        };

        this.InitializeComponent(); //Leave this line last (before the canvas wiring below)

        GameCanvas.FirstStarted += (_, _) =>
        {
            _gameCanvasManager?.CanvasFirstStart(GameCanvas);

            //The window was activated before the game existed, so hand the canvas keyboard focus once here (later
            //  activations are the engine's GameWindowLifecycle). Deferred so it lands after start-up finishes.
            DispatcherQueue.TryEnqueue(() => GameCanvas.Focus(FocusState.Programmatic));
        };
    }
}
