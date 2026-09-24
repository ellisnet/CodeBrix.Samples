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
            FocusGameCanvas();
        };
    }

    /// <summary>
    /// Called by the app when the window is activated. The canvas takes keyboard focus on first start and after
    /// a click, but nothing restored it after the window was deactivated and activated again (alt-tab, raising
    /// it from another application): keys then went to the focused element's ancestors and the keyboard was
    /// silently dead. A gamepad needs no focus, which made the failure look stranger than it was.
    /// </summary>
    internal void OnWindowActivated() => FocusGameCanvas();

    /// <summary>Called by the app when the window is hidden or shown.</summary>
    /// <param name="visible">Whether the window is now visible.</param>
    internal void OnWindowVisibilityChanged(bool visible) => _gameCanvasManager?.WindowVisibilityChanged(visible);

    //Defer to the dispatcher so focus lands after whatever took it finishes processing.
    private void FocusGameCanvas() =>
        DispatcherQueue.TryEnqueue(() => GameCanvas.Focus(FocusState.Programmatic));
}
