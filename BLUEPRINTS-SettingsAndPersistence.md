# CodeBrix.Samples Blueprints: Settings and persistence

These recipes cover how an application keeps state between runs through
the AppSettings add-in: wrapping the store in one application-named facade,
opening it early enough that a static initializer can read from it, remembering
a user's folder choice and the last window size, persisting small pieces of
state such as a palette or a recent list through the same store, and flushing
deferred writes at natural points rather than only at quit. Reach for this
file when a value has to survive a restart, or when the order in which the
store opens relative to the rest of startup matters.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Wrap the AppSettings add-in in one application named facade](#wrap-the-appsettings-add-in-in-one-application-named-facade)
- [Open the settings store before any other startup work](#open-the-settings-store-before-any-other-startup-work)
- [Choose a folder with the picker and remember it across runs](#choose-a-folder-with-the-picker-and-remember-it-across-runs)
- [Restore a remembered window size before any window exists](#restore-a-remembered-window-size-before-any-window-exists)
- [Persist small pieces of application state through the same store](#persist-small-pieces-of-application-state-through-the-same-store)
- [Flush deferred settings at natural points instead of at quit](#flush-deferred-settings-at-natural-points-instead-of-at-quit)

## Related blueprints

- [BLUEPRINTS-AppStructureAndStartup.md](BLUEPRINTS-AppStructureAndStartup.md) - the App constructor ordering these recipes slot into, before InitializeComponent
- [BLUEPRINTS-PlatformServices.md](BLUEPRINTS-PlatformServices.md) - the folder picker bridge that the remember-a-folder recipe calls through
- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - the async commands and change notifications that surround a settings read or write
- [BLUEPRINTS-ThemingAndStyling.md](BLUEPRINTS-ThemingAndStyling.md) - the color scheme this store remembers, and why it has to be read before the first page is built
- [BLUEPRINTS-NotYetCovered.md](BLUEPRINTS-NotYetCovered.md) - the topics no application here demonstrates yet, if this store is not the persistence you need

---

## Settings and persistence

### Wrap the AppSettings add-in in one application named facade

**When you want this.** Any application with settings. The facade gives you one
application-named type to call, one place to change the backend, and a store that
survives corruption.

**The MVVM shape.** A static facade in its own small library forwards every call
to the add-in. View models call the facade by key; nothing else in the application
talks to the add-in. Keys are constants on the type that owns them.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Settings/SettingsService.cs
public static class SettingsService
{
    /// <summary>The application name the settings store is registered under.</summary>
    public const string AppName = "KenneyAssetBrowser";

    public static bool IsInitialized => AppSettingsService.IsInitialized;
    public static AppSettingsStore Store => AppSettingsService.Store;
    public static string DefaultDirectory => AppSettingsService.GetDefaultDirectory(AppName);

    /// <summary>
    /// Opens the settings store in the default folder, running the startup
    /// auto-backup and pruning sequence. Call once, before any UI renders.
    /// </summary>
    public static void Initialize() => AppSettingsService.Initialize(AppName);

    public static void Initialize(string directoryPath) =>
        AppSettingsService.Initialize(AppName, directoryPath);

    /// <summary>Closes the store and permits a later Initialize() (test hosts).</summary>
    public static void Shutdown() => AppSettingsService.Shutdown();

    public static AppSettingProperty<T> Wrap<T>(string property, T defaultValue) =>
        AppSettingsService.Wrap(property, defaultValue);

    public static T Get<T>(string property) => AppSettingsService.Get<T>(property);
    public static void Set(string key, object val) => AppSettingsService.Set(key, val);

    public static void AddPropertyHandler(string propertyName, EventHandler<AppSettingChangedEventArgs> handler) =>
        AppSettingsService.AddSettingHandler(propertyName, handler);
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>The settings.sqlite key holding the user's chosen assets folder.</summary>
public const string AssetsFolderKey = "KenneyAssetBrowser.Settings.AssetsFolder";

/// <summary>The settings.sqlite key holding the file name of the last-browsed bundle.</summary>
public const string LastBundleKey = "KenneyAssetBrowser.Settings.LastBundleFile";
```

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Settings/Pinta.Brix.Settings.csproj -->
<!-- The settings machinery (store, typed properties, change events, backup/
     import/export) is provided by the CodeBrix.Platform.AppSettings add-in;
     this library is the thin Pinta.Brix-named facade over it. -->
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Settings/SettingsService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Settings/SettingsService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Settings/Pinta.Brix.Settings.csproj`

**Sharp edges.**
- The add-in supplies the whole store - typed properties, change events, startup
  auto-backup and pruning, corruption recovery, import and export. Do not
  re-implement any of it; the facade exists only to name it after your
  application.
- Initialization runs the startup backup and prune, so it belongs before any UI
  renders and before anything reads a setting.
- The store is process-global, so a test host needs the shutdown call, or a
  throwaway directory, to re-initialize between cases.
- A companion logging facade forwards to the add-in's logging service, so the
  settings backend's diagnostics reach the same sinks as the rest of the
  application.
- Keep the layering rule in a project-file comment: every persisted value goes
  through the settings library, and it is the only project that takes the storage
  dependency.

### Open the settings store before any other startup work

**When you want this.** A static type in one of your libraries reads a setting
from its own static constructor, so ordering is not optional.

**The MVVM shape.** The `App` constructor opens the store as its first real step,
before `InitializeComponent()`; the ordering comment travels with the call.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Open (or silently create) the single portable settings.sqlite store -
//including its startup auto-backup and pruning - before anything reads
//a setting. PintaCore's static constructor builds the palette manager,
//which reads settings, so this must come first.
Pinta.Brix.Settings.SettingsService.Initialize();
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/App.xaml.cs` (opened after the
container and before `InitializeComponent()`, because the page's view model reads
a setting in its own constructor)
`GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs` (opened for the same
reason, and then read immediately, because the remembered color scheme decides the
application theme and that may be set only before initialization completes - see
[Remember the chosen scheme and read it back before the first page](BLUEPRINTS-ThemingAndStyling.md#remember-the-chosen-scheme-and-read-it-back-before-the-first-page))

**Sharp edges.**
- The failure is quiet and order-dependent: a static constructor that runs before
  the store exists gets defaults instead of the user's values.
- Store creation is silent on first run: no dialog, no error.

### Choose a folder with the picker and remember it across runs

**When you want this.** The application needs a user-chosen location, and the
choice should be the last thing the user ever has to do about it.

**The MVVM shape.** An async command on the view model opens the picker, writes
the result through the settings facade, and raises change notifications for every
derived property - including the visibility properties that swap a first-launch
prompt for the real content.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
public bool HasAssetsFolder => !string.IsNullOrWhiteSpace(_assetsFolder);

public string AssetsFolderLabel => HasAssetsFolder ? _assetsFolder : "Choose assets folder…";

public Visibility FolderPromptVisibility => HasAssetsFolder ? Visibility.Collapsed : Visibility.Visible;

public Visibility CatalogAreaVisibility => HasAssetsFolder ? Visibility.Visible : Visibility.Collapsed;

public SimpleCommand PickFolderCommand => field ??=
    new SimpleCommand((Func<object, Task>)(_ => PickFolderAsync()));

private async Task PickFolderAsync()
{
    var picker = new FolderPicker
    {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
    };
    picker.FileTypeFilter.Add("*");

    var folder = await picker.PickSingleFolderAsync();
    if (folder == null) { return; }

    _assetsFolder = folder.Path;
    SettingsService.Set(AssetsFolderKey, _assetsFolder);
    NotifyPropertyChanged(nameof(HasAssetsFolder));
    NotifyPropertyChanged(nameof(AssetsFolderLabel));
    NotifyPropertyChanged(nameof(FolderPromptVisibility));
    NotifyPropertyChanged(nameof(CatalogAreaVisibility));

    await ReloadCatalogAsync();
}
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml`

**Sharp edges.**
- The filter call is required even for a folder picker.
- A cancelled picker returns null; the command returns without touching state.
- Bind the same command from both the first-launch prompt and the header button,
  so there is one code path either way.
- On the LinuxFrameBuffer head the picker exists only because that head opted into
  it; see the startup area.
- A path a picker returns may need decoding before it is stored; see the bridge
  area.

### Restore a remembered window size before any window exists

**When you want this.** You want the application to reopen at the size the user
left it, and the head creates the native window before your page loads.

**The MVVM shape.** A settings read in the `App` constructor feeding the
platform's preferred launch size, plus a write-through handler on the window's
size-changed event. The scale conversion is the part that is easy to get wrong.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Restore the persisted window size BEFORE any window exists - the
//Skia heads consult ApplicationView.PreferredLaunchViewSize when they
//create the native window, and that is the only public seam for the
//initial size. Setting names and the 1100x750 defaults match
//upstream. The maximized flag is not restored: the platform exposes
//no public presenter state on the Skia heads.
int windowWidth = Pinta.Brix.Settings.SettingsService.Get("window-size-width", 1100);
int windowHeight = Pinta.Brix.Settings.SettingsService.Get("window-size-height", 750);
Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize =
    new Windows.Foundation.Size(windowWidth, windowHeight);
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Write-through persistence of the window size; the store ignores
//writes when the value is unchanged. args.Size is in logical units
//but the X11 head consumes PreferredLaunchViewSize as NATIVE pixels,
//so the stored value must be native pixels or every restart would
//rescale the window by the display-scale factor.
MainWindow.SizeChanged += (_, args) =>
{
    if (MainWindow.Content?.XamlRoot is not { } root) { return; }

    double scale = root.RasterizationScale;
    Pinta.Brix.Settings.SettingsService.Set("window-size-width", (int)Math.Round(args.Size.Width * scale));
    Pinta.Brix.Settings.SettingsService.Set("window-size-height", (int)Math.Round(args.Size.Height * scale));
};
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`

**Sharp edges.**
- The size-changed event reports logical units while the preferred launch size is
  consumed as native pixels on the X11 head. Multiply by the root's rasterization
  scale on the way in, or the window shrinks or grows at every restart on a scaled
  display.
- Pinta.Brix restores the size and not a maximized flag, and the comment above says
  why it was written that way. The presenter itself is reachable from application
  code: `MainWindow.AppWindow.Presenter` is an `OverlappedPresenter` as soon as the
  `Window` is constructed, and that is where a minimum or maximum size goes. See
  [Keep the window from shrinking below a minimum](BLUEPRINTS-AppStructureAndStartup.md#keep-the-window-from-shrinking-below-a-minimum).
- Setting the launch size from a settings read is one use of the same seam. For the
  plain form, where the size is a constant in the `App` class rather than a stored
  value, and for what each head does with the numbers, see
  [Set the window's launch size](BLUEPRINTS-AppStructureAndStartup.md#set-the-windows-launch-size).
- Write-through on every resize is cheap because the store skips unchanged values.

### Persist small pieces of application state through the same store

**When you want this.** A palette, a recent list, a last-used value - state that
should survive a restart without inventing a file format.

**The MVVM shape.** The owning manager reads its state from the settings service
on construction and writes it back on change; the values are serialized through
the same store as everything else, and the keys live in one constants class.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/PaletteManager.cs
// Pinta.Brix note: upstream kept the working palette in a palette.txt file
// beside settings.xml. Everything persisted now lives in settings.sqlite, so
// the palette is a setting like any other - stored as its list of colours.
// (Edit > Palette > Save As still writes a real file, but only where the
// user asks for one: that is an export, not application state.)
private const string CURRENT_PALETTE_KEY = "current-palette";
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/PaletteManager.cs
private void SaveColors ()
{
	// Primary / Secondary colors
	settings.PutSetting (SettingNames.PRIMARY_COLOR, PrimaryColor.ToHex ());
	settings.PutSetting (SettingNames.SECONDARY_COLOR, SecondaryColor.ToHex ());

	// Recently used palette
	settings.PutSetting (SettingNames.RECENT_COLORS, recently_used.Select (c => c.ToHex ()).ToArray ());
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/SettingNames.cs
internal static class SettingNames
{
	internal const string DEFAULT_IMAGE_TYPE = "default-image-type";
	internal const string JPG_QUALITY = "jpg-quality";
	// ...
	internal static string ToolAntialias (BaseTool tool)
		=> $"{tool.GetType ().Name.ToLowerInvariant ()}-antialias";
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/PaletteManager.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/SettingNames.cs`

**Sharp edges.**
- Setting keys live in one constants class, including a convention for per-item
  keys derived from a type name, so a key is never spelled twice.
- The store serializes values as JSON, so an array round trips directly and no
  packing convention is needed.
- Reads use a default that is also the application's default, so a missing key and
  a fresh install behave identically.

### Flush deferred settings at natural points instead of at quit

**When you want this.** Components push their state on a "save before quit" event,
in an application that has no quit path.

**The MVVM shape.** Keep the event, but raise it at points where the state has
naturally settled - a tool change, a document close - rather than only at exit.
Every write goes straight through to the store.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/SettingsManager.cs
// Pinta.Brix note: upstream kept its settings in an in-memory dictionary that
// was serialised to settings.xml ONCE, on quit. This port stores everything in
// the single portable settings.sqlite instead (see Pinta.Brix.Settings), and
// every PutSetting WRITES THROUGH IMMEDIATELY - so nothing is lost when the
// application is closed from the window's own chrome, which is the only way it
// can be closed here (there is deliberately no File > Quit).
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/SettingsManager.cs
/// <remarks>
/// Safe and cheap to call often: each PutSetting is a single upsert, and the
/// store does nothing at all when the value has not changed.
/// </remarks>
public void DoSaveSettingsBeforeQuit ()
{
	try {
		SaveSettingsBeforeQuit?.Invoke (this, EventArgs.Empty);
	} catch (Exception ex) {
		// Flushing settings must never take the application down.
		LoggingService.LogError ("Settings could not be saved", ex);
	}
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ToolManager.cs
// Pinta.Brix note: the ported tools push their option values from
// inside SaveSettingsBeforeQuit rather than as they change, and this
// application has no quit path - the window's own chrome closes it.
// Flushing on every tool change means a tool's options reach
// settings.sqlite while the user is still working.
PintaCore.Settings.DoSaveSettingsBeforeQuit ();
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/SettingsManager.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ToolManager.cs`

**Sharp edges.**
- A quit-only flush loses everything on a head with no quit command; find the
  natural settle points instead.
- The flush is wrapped so a failing subscriber cannot take the application down.
- Frequent flushing is only cheap because the store skips unchanged values.

