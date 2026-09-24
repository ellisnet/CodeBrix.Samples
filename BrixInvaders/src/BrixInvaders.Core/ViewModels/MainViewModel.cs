using System;
using System.Diagnostics;
using BrixInvaders.Game.Audio;
using BrixInvaders.Game.Credits;
using BrixInvaders.Game.Hosting;
using BrixInvaders.Game.Links;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using CodeBrix.Platform.Simple;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace BrixInvaders.ViewModels;

/// <summary>
/// Lets the page hand the view model the game canvas once it has a size, so the page never names the view model's
/// concrete type.
/// </summary>
public interface IManageGameCanvas
{
    /// <summary>The running game host, or null before the canvas has started.</summary>
    BrixInvadersGameHost Host { get; }

    /// <summary>Called once, when the canvas has started for the first time.</summary>
    /// <param name="canvas">The started canvas.</param>
    void CanvasFirstStart(GameSurfaceCanvas canvas);

    /// <summary>Called when the window is hidden (minimized) or shown again.</summary>
    /// <param name="visible">Whether the window is now visible.</param>
    void WindowVisibilityChanged(bool visible);
}

/// <summary>
/// The main page's view model. It owns the <see cref="BrixInvadersGameHost"/>: it chooses the render tier and the
/// render resolution before the canvas builds its scene pipeline, then creates and starts the host.
/// </summary>
[Microsoft.UI.Xaml.Data.Bindable]
public class MainViewModel : SimpleViewModel, IManageGameCanvas
{
    /// <summary>Creates the view model.</summary>
    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Debug.WriteLine("BrixInvaders view model startup.");
    }

    #region | Bindable properties |

    //The game draws everything itself (HUD and menus included), so the page has no bindable state.

    #endregion

    #region | Commands and their implementations |

    //No commands: every control is a key, a gamepad button or a click on the game surface.

    #endregion

    #region | IManageGameCanvas implementation |

    /// <inheritdoc />
    public BrixInvadersGameHost Host { get; private set; }

    /// <inheritdoc />
    public void CanvasFirstStart(GameSurfaceCanvas canvas)
    {
        //The GPU tier (unless BRIXINVADERS_USE_CPU=1) and the pinned 1280 x 720 render resolution MUST be set before
        //  the first access to canvas.Host - the render tier cannot change once the scene pipeline exists.
        BrixInvadersGameHost.PrepareCanvas(canvas);

        var dispatcher = DispatcherQueue.GetForCurrentThread();

        //The three seams the game library leaves to the app: generated music (the engine's UseGeneratedMusic), links
        //  opened by the platform launcher on this (UI) thread, and credits that read the host's Kenney pack titles
        //  and the music card lazily - both only when the credits screen opens, long after the host has loaded them.
        var music = new GeneratedMusicDirector(new EngineMusicEngine());
        var links = new LauncherLinkOpener(action => dispatcher != null && dispatcher.TryEnqueue(() => action()));
        BrixInvadersGameHost host = null;
        var credits = new KenneyCreditsContent(() => host?.PackCredits ?? Array.Empty<string>(), music.CreditLines);
        host = new BrixInvadersGameHost(canvas, music, links, credits);
        Host = host;
        Host.QuitRequested += () => dispatcher?.TryEnqueue(() => Application.Current.Exit());

        //Information keeps the game's own [BrixInvaders] lines on the console along with the engine's milestones
        Host.Initialize(logLevel: Microsoft.Extensions.Logging.LogLevel.Information);
    }

    /// <inheritdoc/>
    public void WindowVisibilityChanged(bool visible)
    {
        if (Host == null) { return; }
        if (visible) { Host.OnWindowShown(); } else { Host.OnWindowHidden(); }
    }

    #endregion
}
