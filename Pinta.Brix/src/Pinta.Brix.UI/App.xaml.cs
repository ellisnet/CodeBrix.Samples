using CodeBrix.Platform.Simple;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Pinta.Brix.Helpers;
using Pinta.Brix.Services;
using System;
using System.Linq;

namespace Pinta.Brix;

public partial class App : Application
{
    public App() : this(null) { }

    // Alternate hosts (the PlayTests) keep the settings store out of the user's own folder.
    public App(string settingsDirectory)
    {
        //Set Open Sans as the default font for all text in the application
        global::CodeBrix.Platform.UI.FeatureConfiguration.Font.DefaultTextFontFamily =
            "ms-appx:///CodeBrix.Platform.Fonts.OpenSans/Fonts/OpenSans.ttf";

        SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
        {
            //Register the app's services here
            services.AddPintaBrix();
        });
        SimpleViewModel.SetIsDesignMode(false);

        //Open (or silently create) the single portable settings.sqlite store -
        //including its startup auto-backup and pruning - before anything reads
        //a setting. PintaCore's static constructor builds the palette manager,
        //which reads settings, so this must come first.
        if (settingsDirectory is null)
        {
            Pinta.Brix.Settings.SettingsService.Initialize();
        }
        else
        {
            Pinta.Brix.Settings.SettingsService.Initialize(settingsDirectory);
        }

        //Restore the persisted window size BEFORE any window exists - the
        //Skia heads consult ApplicationView.PreferredLaunchViewSize when they
        //create the native window, and that is the only public seam for the
        //initial size. Setting names and the 1100x750 defaults match
        //upstream. The maximized flag is not restored: the platform exposes
        //no public presenter state on the Skia heads.
        //The launch size is the client area in effective pixels, which is
        //what X11, Wayland and macOS open; Win32Skia and WPF open it as the
        //framed window instead, a frame too small. RestoreFramedWindowSize
        //puts that right once the page has loaded.
        int windowWidth = Pinta.Brix.Settings.SettingsService.Get("window-size-width", 1100);
        int windowHeight = Pinta.Brix.Settings.SettingsService.Get("window-size-height", 750);
        Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize =
            new Windows.Foundation.Size(windowWidth, windowHeight);

        //The framed size the window had when it last changed. It is read now,
        //before the window exists, because the window's own first layout
        //writes over it.
        savedFramedWidth = Pinta.Brix.Settings.SettingsService.Get("window-framed-width", 0);
        savedFramedHeight = Pinta.Brix.Settings.SettingsService.Get("window-framed-height", 0);

        InitializeComponent();
    }

    protected Window MainWindow { get; private set; }

    private bool windowCloseConfirmed;

    private readonly int savedFramedWidth;

    private readonly int savedFramedHeight;

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window
        {
            Title = Pinta.Brix.Engine.PintaCore.ApplicationName
        };

        //Engine bootstrap: install the UI-layer services and register the
        //file formats and core effects/adjustments with the engine
        Pinta.Brix.Engine.PintaCore.InitializeResources(new Pinta.Brix.Controls.SkiaResourceService());
        Pinta.Brix.Engine.PintaCore.InitializeTimer(
            new Pinta.Brix.Controls.DispatcherTimerService(MainWindow.DispatcherQueue));
        Pinta.Brix.FileFormats.FileFormatsRegistration.RegisterAll(Pinta.Brix.Engine.PintaCore.ImageFormats);
        Pinta.Brix.Effects.CoreEffects.Register(Pinta.Brix.Engine.PintaCore.Services);
        Pinta.Brix.Tools.CoreTools.Register(Pinta.Brix.Engine.PintaCore.Services);

        //Window title tracks the active document
        Pinta.Brix.Engine.PintaCore.Chrome.MainWindowTitleChanged += (_, _) =>
            MainWindow.Title = Pinta.Brix.Engine.PintaCore.Chrome.MainWindowTitle;

        //Window-close save prompt. Closed is the platform's cancellable-close
        //event: setting Handled vetoes the close, and the X11 head reports
        //SupportsClosingCancellation. The save-prompt loop is async, so when
        //dirty documents exist the close is vetoed first and re-issued once
        //the user has decided. Mirrors upstream's exit-path prompt loop; it
        //is triggered by window close because there is no File > Quit here.
        MainWindow.Closed += async (_, e) =>
        {
            if (windowCloseConfirmed) { return; }

            if (!Pinta.Brix.Engine.PintaCore.Workspace.OpenDocuments.Any(d => d.IsDirty)) { return; }

            e.Handled = true;

            try
            {
                //The shell installs its save-prompt loop on this service, so the
                //window close reaches it without knowing which page is showing.
                IShellCloseService closeService =
                    SimpleServiceResolver.Instance?.GetService<IShellCloseService>();

                if (closeService is not null && await closeService.ConfirmCloseAsync())
                {
                    windowCloseConfirmed = true;
                    MainWindow.Close();
                }
            }
            catch (Exception)
            {
                //A failed prompt must never take the window down with unsaved
                //work - the veto above stands and the application stays open.
            }
        };

        //Write-through persistence of the window size; the store ignores
        //writes when the value is unchanged. args.Size is the client area in
        //effective pixels - the same unit PreferredLaunchViewSize takes.
        MainWindow.SizeChanged += (_, args) =>
        {
            //Sizes reported before the content is in place are not the
            //window the user sees.
            if (MainWindow.Content?.XamlRoot is null) { return; }

            Pinta.Brix.Settings.SettingsService.Set("window-size-width", (int)Math.Round(args.Size.Width));
            Pinta.Brix.Settings.SettingsService.Set("window-size-height", (int)Math.Round(args.Size.Height));
        };

        //The framed size is stored next to it: AppWindow.Size, the window plus
        //whatever frame the windowing system draws around it, in physical
        //pixels. AppWindow.Resize takes that same quantity, so the pair
        //round-trips exactly. AppWindow.Changed rather than SizeChanged,
        //because some heads update AppWindow.Size after they raise SizeChanged.
        MainWindow.AppWindow.Changed += (sender, args) =>
        {
            if (!args.DidSizeChange) { return; }

            Pinta.Brix.Settings.SettingsService.Set("window-framed-width", sender.Size.Width);
            Pinta.Brix.Settings.SettingsService.Set("window-framed-height", sender.Size.Height);
        };

        if (MainWindow.Content is not Frame rootFrame)
        {
            rootFrame = new Frame();
            MainWindow.Content = rootFrame;
            rootFrame.NavigationFailed += OnNavigationFailed;
            rootFrame.Loaded += RestoreFramedWindowSize;
        }

        if (rootFrame.Content == null)
        {
            rootFrame.Navigate(typeof(Views.MainPage), args.Arguments);
        }

        MainWindow.Activate();
    }

    //Once the page is in place the window has its first real size. On the
    //heads that open the launch size as the framed window (Win32Skia and WPF)
    //it is a frame smaller than the window the user left, so the stored framed
    //size is put back through AppWindow.Resize. Where the launch size already
    //restored the window (X11, Wayland, macOS) the framed size matches and
    //nothing is called, which also keeps Wayland, where a client cannot resize
    //its own window, from logging a warning. A pixel or two either way is
    //rounding between effective and physical pixels, not a frame.
    private void RestoreFramedWindowSize(object sender, RoutedEventArgs e)
    {
        ((FrameworkElement)sender).Loaded -= RestoreFramedWindowSize;

        if (savedFramedWidth <= 0 || savedFramedHeight <= 0) { return; }

        Windows.Graphics.SizeInt32 current = MainWindow.AppWindow.Size;
        if (Math.Abs(current.Width - savedFramedWidth) <= 2
            && Math.Abs(current.Height - savedFramedHeight) <= 2) { return; }

        MainWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32
        {
            Width = savedFramedWidth,
            Height = savedFramedHeight
        });
    }

    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
    }

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
