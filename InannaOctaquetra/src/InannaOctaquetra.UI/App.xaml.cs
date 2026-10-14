using CodeBrix.Platform.Simple;
using InannaOctaquetra.Helpers;
using InannaOctaquetra.Reading;
using InannaOctaquetra.Reading.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;

namespace InannaOctaquetra;

public partial class App : Application
{
    //The size the window opens at, and the smallest size it may be dragged to. The launch
    //size gives the altar an Octaquetra at its full card size with the 360-wide deck rail beside
    //it and the header's two rows of controls on one line each; the minimum is the point
    //below which the header buttons start to wrap and the Octaquetra's station labels collide
    //with the cards on the diagonal petals.
    private const int LaunchWidth = 1440;
    private const int LaunchHeight = 900;
    private const int MinimumWidth = 1180;
    private const int MinimumHeight = 760;

    /// <summary>The launch size actually requested: the preferred size, clamped to the main display on macOS.</summary>
    public static int WindowWidth { get; private set; } = LaunchWidth;

    /// <summary>The launch height actually requested; see <see cref="WindowWidth"/>.</summary>
    public static int WindowHeight { get; private set; } = LaunchHeight;

    [System.Runtime.InteropServices.DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern uint CGMainDisplayID();

    [System.Runtime.InteropServices.DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern CGRect CGDisplayBounds(uint display);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct CGRect { public double X, Y, Width, Height; }

    /// <summary>
    /// macOS: fit the launch size to the main display's logical bounds, leaving room for the menu
    /// bar, the Dock and the window frame, so the window opens whole on a small Mac. Anything that
    /// fails - and every other platform - leaves the preferred size in place.
    /// </summary>
    private static void ClampToDisplay()
    {
        try
        {
            if (!OperatingSystem.IsMacOS()) { return; }
            var bounds = CGDisplayBounds(CGMainDisplayID());
            if (bounds.Width < 400 || bounds.Height < 300) { return; }
            WindowWidth = (int)Math.Min(LaunchWidth, bounds.Width - 24);
            WindowHeight = (int)Math.Min(LaunchHeight, bounds.Height - 150);
            Console.WriteLine($"App: display {bounds.Width}x{bounds.Height}, window {WindowWidth}x{WindowHeight}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"App: display bounds unavailable ({ex.Message}).");
        }
    }

    public App() : this(null) { }

    // Alternate hosts may replace services before any view models are constructed.
    public App(Action<IServiceCollection> configureServices)
    {
        //Merriweather is the app's voice: an old-style serif for a temple oracle
        global::CodeBrix.Platform.UI.FeatureConfiguration.Font.DefaultTextFontFamily =
            "ms-appx:///CodeBrix.Platform.Fonts.Merriweather/Fonts/Merriweather.ttf";

        SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
        {
            //An alternate host's registrations go first: AddReading() only adds what is not
            //  already registered, so a replacement made here is the one that is used
            configureServices?.Invoke(services);

            //Register the app's services here
            services.AddReading();
        });
        SimpleViewModel.SetIsDesignMode(false);

        //The size the first window opens at. ApplicationView.PreferredLaunchViewSize is the
        //only public seam an application has for its own launch size: every desktop head
        //reads it while it is creating the native window and falls back to the platform's
        //own default when it is empty, so it has to be set before any window exists. The
        //value is set on every launch, unconditionally, because the platform remembers it in
        //its own settings file; setting it every time keeps that file in step with this
        //source file instead of letting an old value linger. On macOS the size is first fitted
        //to the display, so a small Mac still opens the whole window.
        ClampToDisplay();
        try
        {
            Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize =
                new Windows.Foundation.Size(WindowWidth, WindowHeight);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"App: PreferredLaunchViewSize unavailable ({ex.Message}).");
        }

        RequestedTheme = ApplicationTheme.Dark;
        InitializeComponent();
    }

    protected Window MainWindow { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        if (Environment.GetEnvironmentVariable("INANNA_DIAG") is { Length: > 0 })
        {
            _ = DiagnoseContentAsync();
        }

        MainWindow = new Window { Title = "Octaquetra of Inanna" };

        //The smallest size the user may drag the window to. The presenter is already in
        //place here: constructing a Window builds its native window straight away once the
        //application has finished initializing, and the window's default presenter is an
        //OverlappedPresenter. Setting the minimum now, before Activate(), means the window
        //manager has the constraint before the window is ever shown. No maximum is set, so
        //the window can still be resized up and maximized.
        if (MainWindow.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = Math.Min(MinimumWidth, WindowWidth);
            presenter.PreferredMinimumHeight = Math.Min(MinimumHeight, WindowHeight);
        }

        //macOS: the head's Resize units are native pixels on a Retina display, so the window is
        //nudged until the page is laid out into the intended logical size
        if (OperatingSystem.IsMacOS()) { SizeTheWindow(MainWindow); }

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

        MainWindow.Activate();
    }

    /// <summary>
    /// macOS: resize the window so the page is laid out into the launch size in *logical* units.
    /// AppWindow.Resize works in the head's own units (native pixels on a 2x display), so the size
    /// is corrected from Window.Bounds, which is the client area in effective pixels. Every step
    /// degrades gracefully: a throw anywhere leaves the platform default in place.
    /// </summary>
    private static void SizeTheWindow(Window window)
    {
        try
        {
            var appWindow = window.AppWindow;
            if (appWindow == null) { return; }
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = WindowWidth, Height = WindowHeight });
            BeginSizeCorrection(window);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"App: window sizing unavailable ({ex.Message}).");
        }
    }

    /// <summary>
    /// Nudge the window until its client area measures the launch size in effective pixels. A few
    /// short passes converge whatever the head's Resize units are and whatever the frame adds; the
    /// timer stops as soon as it is close enough, or after a handful of tries.
    /// </summary>
    private static void BeginSizeCorrection(Window window)
    {
        var queue = window.DispatcherQueue;
        if (queue == null) { return; }

        Microsoft.UI.Dispatching.DispatcherQueueTimer timer;
        try { timer = queue.CreateTimer(); }
        catch (Exception ex) { Console.WriteLine($"App: no dispatcher timer ({ex.Message})."); return; }
        if (timer == null) { return; }

        var passes = 0;
        timer.Interval = TimeSpan.FromMilliseconds(140);
        timer.Tick += (t, _) =>
        {
            passes++;
            try
            {
                var bounds = window.Bounds;
                if (bounds.Width >= 50 && bounds.Height >= 50)
                {
                    var offBy = Math.Abs(WindowWidth - bounds.Width) + Math.Abs(WindowHeight - bounds.Height);
                    if (offBy < 6) { t.Stop(); return; }

                    var current = window.AppWindow.Size;
                    window.AppWindow.Resize(new Windows.Graphics.SizeInt32
                    {
                        Width = (int)Math.Round(current.Width * (WindowWidth / bounds.Width)),
                        Height = (int)Math.Round(current.Height * (WindowHeight / bounds.Height)),
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"App: size correction stopped ({ex.Message}).");
                t.Stop();
                return;
            }

            if (passes >= 6) { t.Stop(); }
        };
        timer.Start();
    }

    /// <summary>INANNA_DIAG=1: print where app content is resolved from (packaging diagnostics).</summary>
    private static async Task DiagnoseContentAsync()
    {
        Console.WriteLine($"DIAG: BaseDirectory={AppContext.BaseDirectory}");
        try { Console.WriteLine($"DIAG: InstalledLocation={Windows.ApplicationModel.Package.Current.InstalledLocation.Path}"); }
        catch (Exception ex) { Console.WriteLine($"DIAG: InstalledLocation failed: {ex.Message}"); }
        try
        {
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(
                new Uri("ms-appx:///CodeBrix.Platform.Fonts.Merriweather/Fonts/Merriweather.ttf"));
            Console.WriteLine($"DIAG: font file={file.Path}");
        }
        catch (Exception ex) { Console.WriteLine($"DIAG: font file failed: {ex.GetType().Name}: {ex.Message}"); }
        Console.WriteLine($"DIAG: embedded art velvet={ArtAssets.VelvetJpeg.Length} print={ArtAssets.VelvetPrintJpeg.Length} band={ArtAssets.InscribedOctaquetraPng.Length} bytes");
        Console.Out.Flush();
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
