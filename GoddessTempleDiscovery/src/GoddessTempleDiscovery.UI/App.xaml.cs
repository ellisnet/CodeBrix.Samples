using CodeBrix.Platform.GameEngine.Host.Hosting;
using CodeBrix.Platform.Simple;
using GoddessTempleDiscovery.Game.Hosting;
using GoddessTempleDiscovery.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace GoddessTempleDiscovery;

public partial class App : Application
{
    //The size the window opens at, and the smallest size it may be dragged to: the table is laid out for
    //1280 x 800 and widens with the window; below the minimum the Site Row and the hand start to overlap.
    private const int LaunchWidth = 1440;
    private const int LaunchHeight = 900;
    private const int MinimumWidth = 1180;
    private const int MinimumHeight = 740;

    public App() : this(null) { }

    // Alternate hosts (the PlayTests) may replace services before any view models are constructed.
    public App(Action<IServiceCollection> configureServices)
    {
        //Open (or silently create) the game's settings store before anything reads it
        GameStartup.OpenSettingsStore();

        //Merriweather is the application's voice: an old-style serif for a field journal
        global::CodeBrix.Platform.UI.FeatureConfiguration.Font.DefaultTextFontFamily =
            "ms-appx:///CodeBrix.Platform.Fonts.Merriweather/Fonts/Merriweather.ttf";

        SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
        {
            //Register the app's services here
            configureServices?.Invoke(services);
        });
        SimpleViewModel.SetIsDesignMode(false);

        //The size the first window opens at; set on every launch so the platform's remembered value follows this file
        Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize =
            new Windows.Foundation.Size(LaunchWidth, LaunchHeight);

        RequestedTheme = ApplicationTheme.Dark;
        InitializeComponent();
    }

    protected Window MainWindow { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window { Title = "Goddess Temple Discovery!" };

        if (MainWindow.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = MinimumWidth;
            presenter.PreferredMinimumHeight = MinimumHeight;
        }

        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
            rootFrame.NavigationFailed += OnNavigationFailed;
        }

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(Views.MainPage), args.Arguments);
        }

        //Minimizing the window pauses the engine (loop and audio); activating hands keyboard focus back to the canvas
        GameWindowLifecycle.Attach(MainWindow);

        MainWindow.Activate();
    }

    void OnNavigationFailed(object sender, NavigationFailedEventArgs e) =>
        throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");

    // Called from each head's Program.Main BEFORE building the host.
    public static void InitializeLogging()
    {
#if DEBUG
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddFilter("CodeBrix.Platform", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);
        });

        global::CodeBrix.Platform.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;
        global::CodeBrix.Platform.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
    }
}
