# CodeBrix.Samples Blueprints: Settings and persistence

These recipes cover how an application keeps state between runs through
the AppSettings add-in: wrapping the store in one application-named facade,
opening it early enough that a static initializer can read from it, remembering
a user's folder choice and the last window size, persisting small pieces of
state such as a palette or a recent list through the same store, and flushing
deferred writes at natural points rather than only at quit. Another recipe
argues for the opposite arrangement - no store at all - for an application
whose state lives on the resources it creates somewhere else and can be rebuilt
from the labels they carry. The last two come from a game: a settings screen
whose menu model changes a stored value and reports what kind of change it was,
so the code that applies it lives somewhere else, and a facade that gives every
stored value a typed, validated property - high-score tables included - behind
an interface the tests implement in memory. Reach for this file when a value
has to survive a restart, when the order in which the store opens relative to
the rest of startup matters, or when you are deciding whether you need a store
in the first place.

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
- [Put identity in labels on the resource instead of a state file beside it](#put-identity-in-labels-on-the-resource-instead-of-a-state-file-beside-it)
- [Let a menu model change stored settings and report what changed](#let-a-menu-model-change-stored-settings-and-report-what-changed)
- [Type and validate every stored value behind an interface the game can fake](#type-and-validate-every-stored-value-behind-an-interface-the-game-can-fake)

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

    /// <summary>
    /// Closes the store and permits a later <see cref="Initialize()"/> (test hosts).
    /// </summary>
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

**Also shown by.**
`BrixInvaders/src/libs/BrixInvaders.Game/Settings/SettingsService.cs` (the
facade in the game library rather than a library of its own, with a typed,
validated property for every value - see
[Type and validate every stored value behind an interface the game can fake](BLUEPRINTS-SettingsAndPersistence.md#type-and-validate-every-stored-value-behind-an-interface-the-game-can-fake))

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
`BrixInvaders/src/BrixInvaders.UI/App.xaml.cs` and
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/GameStartup.cs` (the first
statement of the `App` constructor, before the fonts and the container; the call
is idempotent, and it is where an unattended autopilot run is given a scratch
store - see
[Let an autopilot play through the player's input path and keep its saves apart](BLUEPRINTS-Testing.md#let-an-autopilot-play-through-the-players-input-path-and-keep-its-saves-apart))

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

### Put identity in labels on the resource instead of a state file beside it

**When you want this.** Your application creates resources somewhere else - on a
container daemon, in a cloud account, on a device - and has to find them again on
the next run, know which ones are its own, and be able to remove exactly those and
nothing else. The rest of this file is about remembering state in a store on the
user's machine; this recipe is the case for having no store at all, when the thing
you create can carry its own identity.

**The MVVM shape.** Not a view-model concern. A single static class holds the
label schema and the filters built from it; the creation path stamps the whole set
on every resource it makes; discovery lists by the schema's presence filter,
groups by the identity label and rebuilds the application's model of the world
from nothing else. The view models only ever see the rebuilt model.

**Code.**

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/libs/RedisSetupTool.DockerManagement/Instances/InstanceLabels.cs
/// <summary>
/// The label schema, in one place. Labels are the database: every container, volume and network this
/// tool creates carries them, and discovery rebuilds an instance from nothing else.
/// </summary>
public static class InstanceLabels
{
    /// <summary>The prefix every label shares.</summary>
    public const string Prefix = "codebrix.redissetup.";

    /// <summary>The instance id - the primary key.</summary>
    public const string Instance = Prefix + "instance";

    /// <summary>The topology code, for example <c>D2</c>.</summary>
    public const string Topology = Prefix + "topology";

    // ... the node role and index, the friendly name, the creation time, the published ports,
    // ... the image, the shared secret and the resource kind ...

    /// <summary>
    /// Gets a filter matching every resource this tool created, whatever its instance. A null value
    /// makes the query builder emit a presence match rather than an equality match.
    /// </summary>
    public static IDictionary<string, string> PresenceFilter =>
        new Dictionary<string, string>(StringComparer.Ordinal) { [Instance] = null };

    /// <summary>Gets a filter matching one instance's resources.</summary>
    public static IDictionary<string, string> InstanceFilter(string instanceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId);
        return new Dictionary<string, string>(StringComparer.Ordinal) { [Instance] = instanceId };
    }
}
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/libs/RedisSetupTool.DockerManagement/Topologies/Builders/TopologyBuildContext.cs
/// <summary>Builds the label set every resource of the instance carries.</summary>
/// <param name="resourceKind">One of <c>network</c>, <c>volume</c> or <c>container</c>.</param>
internal Dictionary<string, string> BaseLabels(string resourceKind)
{
    var labels = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [InstanceLabels.Instance] = InstanceId,
        [InstanceLabels.Topology] = Descriptor.Code,
        [InstanceLabels.Name] = InstanceName,
        [InstanceLabels.Created] = CreatedAt.ToString("O", CultureInfo.InvariantCulture),
        [InstanceLabels.Image] = Descriptor.Image,
        [InstanceLabels.Resource] = resourceKind,
    };

    // ... the optional ones: the announced gateway, the shared password, the declared users
    // ... and the service name, each added only when the topology has one ...

    return labels;
}
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/libs/RedisSetupTool.DockerManagement/DockerManager.cs
/// <inheritdoc />
public Task<IReadOnlyList<ContainerInfo>> ListManagedContainersAsync(
    CancellationToken cancellationToken = default) =>
    ListContainersAsync(InstanceLabels.PresenceFilter, includeStopped: true, cancellationToken);

/// <inheritdoc />
public Task<IReadOnlyList<ContainerInfo>> ListInstanceContainersAsync(string instanceId,
    CancellationToken cancellationToken = default) =>
    ListContainersAsync(InstanceLabels.InstanceFilter(instanceId), includeStopped: true,
        cancellationToken);
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/libs/RedisSetupTool.DockerManagement/Topologies/RedisTopologyService.cs
public async Task<IReadOnlyList<TopologyInstance>> DiscoverAsync(
    CancellationToken cancellationToken = default)
{
    var containers = await _docker.ListManagedContainersAsync(cancellationToken)
        .ConfigureAwait(false);
    var volumes = await _docker.ListVolumesAsync(cancellationToken).ConfigureAwait(false);

    var grouped = new Dictionary<string, List<ContainerInfo>>(StringComparer.Ordinal);
    foreach (var container in containers)
    {
        if (string.IsNullOrEmpty(container.InstanceId))
        {
            continue;
        }

        // ... add it to its instance's list ...
    }

    // ... rebuild one instance per group, newest first ...
}

// ...

private async Task<TopologyInstance> RebuildAsync(string instanceId,
    List<ContainerInfo> containers, IReadOnlyList<VolumeInfo> volumes,
    CancellationToken cancellationToken)
{
    // ...
    var code = LabelOf(containers, InstanceLabels.Topology);
    if (!TopologyCatalog.TryParseCode(code, out var topologyId)
        && !InstanceId.TryParseTopology(instanceId, out topologyId))
    {
        return null;
    }
    // ... every other field of the instance comes out of the labels the same way ...
}
```

Teardown is the same query in reverse, and it is written to be idempotent: a
resource that is already gone is not a failure, which is also what lets a
half-finished creation roll itself back down the same path.

**Where to look.**
`RedisSetupTool/src/libs/RedisSetupTool.DockerManagement/Instances/InstanceLabels.cs`
`RedisSetupTool/src/libs/RedisSetupTool.DockerManagement/Instances/InstanceId.cs` and
`Topologies/RedisTopologyService.cs`,
`Topologies/Builders/TopologyBuildContext.cs`

**Sharp edges.**
- Stamp every kind of resource, not just the primary one. A volume or a network
  created without the label set is invisible to discovery and survives teardown as
  litter nobody can attribute. Discovery also has to survive a partly stamped
  resource - one created by an older version, or by a run that failed halfway -
  and falling back to parsing the identity out of the resource name is the cheap
  version of that.
- A presence match and an equality match are different queries. Letting a null
  value mean "the label exists, whatever its value" is what makes one filter find
  everything the application ever created.
- The schema is a published contract with something outside your process.
  Renaming a key orphans everything an earlier run created, so treat the constants
  as public API and add rather than rename.
- Anything in a label is readable by anyone who can inspect the resource. This
  application puts a development password in one and says so on the card; a real
  secret belongs somewhere else.

### Let a menu model change stored settings and report what changed

**When you want this.** A settings screen is a list of rows - volumes, a choice
between named options, a toggle, the defaults for the next game, a destructive
reset - and each change has to reach a different part of the application: the
mixer, the music, the next game's setup. The screen may not even be XAML; here
the game engine draws it. You want the meaning of every row in one testable
place, and the code that reacts to a change in another.

**The MVVM shape.** Three pieces, none of them a view. The screen state machine
in the rules library only moves the cursor and emits "adjust this row by this
much" and "activate this row" commands. A menu model in the game library owns
what each row means: it reads and writes the value through the settings
interface and returns an enum saying what changed. The game session routes that
enum to the mixer, the music director or a deferred rebuild, and the painter asks
the menu model for each row's label and value text. In an application with a
XAML settings page, the view model would take the session's routing role and the
menu model would stay as it is.

**Code.**

The state machine turns menu presses on the settings screen into commands that
carry the row and the direction, and knows nothing about what the rows are:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.GameLogic/Screens/ScreenStateMachine.cs
if (input.Left)
{
    _commands.Add(new ScreenCommand(ScreenCommandKind.AdjustSetting, SettingsCursor, -1));
}

if (input.Right)
{
    _commands.Add(new ScreenCommand(ScreenCommandKind.AdjustSetting, SettingsCursor, 1));
}

if (input.Confirm || input.Start)
{
    _commands.Add(new ScreenCommand(ScreenCommandKind.ActivateSetting, SettingsCursor));
}
```

The menu model changes the stored value and says what kind of change it was. The
reset row needs two confirms, and anything else disarms it:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Settings/SettingsMenu.cs
public SettingsChange Adjust(int row, int delta)
{
    var step = Math.Sign(delta);
    if (step == 0)
    {
        return SettingsChange.None;
    }

    ResetArmed = false;
    switch (row)
    {
        case MasterVolumeRow:
            _settings.MasterVolume = StepVolume(_settings.MasterVolume, step);
            return SettingsChange.Volumes;
        // ...
        case MusicModelRow:
            _settings.MusicGenerator = MusicChoices.NextGenerator(_settings.MusicGenerator, step);
            return SettingsChange.MusicChoice;
        // ...
        case DefaultDifficultyRow:
            _settings.Difficulty = (Difficulty)((((int)_settings.Difficulty + step) % 4 + 4) % 4);
            return SettingsChange.Defaults;
        default:
            return SettingsChange.None;
    }
}

/// <summary>Confirm on a row. Only "Reset high scores" reacts: the first confirm arms it, the second clears.</summary>
/// <param name="row">The row.</param>
/// <returns>What changed.</returns>
public SettingsChange Activate(int row)
{
    if (row != ResetHighScoresRow)
    {
        ResetArmed = false;
        return SettingsChange.None;
    }

    if (!ResetArmed)
    {
        ResetArmed = true;
        ResetDone = false;
        return SettingsChange.ResetArmed;
    }

    ResetArmed = false;
    ResetDone = true;
    _settings.ResetHighScores();
    return SettingsChange.HighScoresReset;
}
```

The session is the only place that knows who has to hear about each kind of
change:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Session/GameSession.cs
private void Apply(SettingsChange change)
{
    switch (change)
    {
        case SettingsChange.Volumes:
            _sound.SetLevels(Settings.MasterVolume, Settings.EffectsVolume);
            _music.SetVolumes(Settings.MasterVolume, Settings.MusicVolume, Settings.EffectsVolume);
            break;
        case SettingsChange.MusicChoice:
            GameLog.Write($"settings: music {Settings.MusicGenerator} through {Settings.InstrumentLibrary}");
            _music.ApplySettings(CreateMusicSettings(), Music is MusicMoment.Sector or MusicMoment.Boss ? MusicSector : 0,
                Music == MusicMoment.Boss);
            break;
        case SettingsChange.GamepadProfile:
            GameLog.Write($"settings: gamepad profile {Settings.GamepadProfile}");
            break;
        case SettingsChange.Defaults:
            _rebuildMachine = true;
            break;
        case SettingsChange.HighScoresReset:
            HighScores.ClearAll();
            GameLog.Write("settings: every high-score table cleared");
            break;
    }
}
```

And the painter draws every row from the menu model, including the armed state
of the reset:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Screens/SettingsScreen.cs
for (var row = 0; row < SettingsMenu.RowCount; row++)
{
    var y = 130 + (row * 56);
    var selected = row == cursor;
    if (selected)
    {
        frame.Overlay.Rectangle(Ui.CenterX, y, 860, 46, 0x402E7DD6, Palette.Accent, 1.5, 8);
    }

    frame.Overlay.Text(SettingsMenu.LabelOf(row).ToUpperInvariant(), Ui.CenterX - 410, y, frame.Font, 20,
        selected ? Palette.Text : Palette.Dim, SKTextAlign.Left);
    var value = session.SettingsMenu.ValueOf(row);
    var armed = row == SettingsMenu.ResetHighScoresRow && session.SettingsMenu.ResetArmed;
    frame.Overlay.Text(selected && row != SettingsMenu.ResetHighScoresRow ? $"<  {value}  >" : value, Ui.CenterX + 410,
        y, frame.Font, 18, armed ? Palette.Danger : selected ? Palette.Accent : Palette.Text, SKTextAlign.Right);
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Settings/SettingsMenu.cs` and
`Settings/SettingsChange.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Session/GameSession.cs` (`Handle`,
`Apply`, `ProcessCommands`)
`BrixInvaders/src/libs/BrixInvaders.GameLogic/Screens/ScreenStateMachine.cs` (`UpdateSettings`)
`BrixInvaders/src/libs/BrixInvaders.Game/Screens/SettingsScreen.cs`
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Settings/SettingsMenuTests.cs`

**Sharp edges.**
- Return what changed instead of applying it. The menu model holds no mixer and
  no music reference, so every row is tested against an in-memory settings
  object with nothing else built.
- A destructive row needs an armed state that everything else clears: a second
  confirm acts, but an adjust, a confirm on another row, or reopening the screen
  disarms it. Show the armed state in the row's value text, or the player cannot
  tell the next press will erase something.
- Writing a new default to the store does not change an object that was built
  from the old one. The screen state machine holds the ship and difficulty it
  offers first, so a changed default only sets a flag, and the session builds a
  fresh machine from the stored settings once the player is back on the title.
- Round a stepped value when you store it. Taking a tenth off 0.8 in floating
  point does not give exactly 0.7, and the test that asserts the new level would
  fail on the difference.
- A change of music model or instruments goes to the music director along with
  where the game is, so the director can decide whether it is a fresh session -
  see [Start endless generated music with one call](BLUEPRINTS-GameEngine.md#start-endless-generated-music-with-one-call).

### Type and validate every stored value behind an interface the game can fake

**When you want this.** The application reads a dozen stored values - numbers
with a range, enum choices, names from a fixed list, a table of records - and
the code that uses them has to run in unit tests with no store open. This goes
further than
[Wrap the AppSettings add-in in one application named facade](BLUEPRINTS-SettingsAndPersistence.md#wrap-the-appsettings-add-in-in-one-application-named-facade),
whose facade forwards generic reads and writes by key: here the facade gives
every value its own typed, validated property, stores a table of records as JSON
under one key per category, and the game reaches all of it through an interface
that tests implement in memory.

**The MVVM shape.** The static facade in the game library is the only type that
calls the AppSettings add-in. An interface names exactly what the game session
reads and writes; one small class implements it by forwarding to the facade, and
the host constructs that class once the store is open. The session and the menu
model take the interface, and the tests hand them an in-memory copy.

**Code.**

Each value is a typed property the add-in creates when the store opens; the
getter validates what it reads, and the setter validates what it writes:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Settings/SettingsService.cs
/// <summary>The chosen ship shape (0..2; default 0).</summary>
public static int ShipShape
{
    get => Math.Clamp(Require(_shipShape).Value, 0, GameSetup.ShipShapeCount - 1);
    set => Require(_shipShape).Set(Math.Clamp(value, 0, GameSetup.ShipShapeCount - 1));
}
// ...
/// <summary>The default difficulty (default Pilot).</summary>
public static Difficulty Difficulty
{
    get => Enum.TryParse(Require(_difficulty).Value, true, out Difficulty level) && Enum.IsDefined(level)
        ? level
        : Difficulty.Pilot;
    set => Require(_difficulty).Set(value.ToString());
}
// ...
/// <summary>The music generator name (default SkyTNT); an unknown stored name reads as the default.</summary>
public static string MusicGenerator
{
    get => MusicChoices.IsGenerator(Require(_musicGenerator).Value)
        ? MusicChoices.ResolveGenerator(_musicGenerator.Value)
        : MusicChoices.DefaultGenerator;
    set => Require(_musicGenerator).Set(MusicChoices.ResolveGenerator(value));
}
// ...
private static void CreateProperties()
{
    _shipShape = AppSettingsService.Wrap(ShipShapeKey, 0);
    _shipColour = AppSettingsService.Wrap(ShipColourKey, 0);
    _difficulty = AppSettingsService.Wrap(DifficultyKey, nameof(Difficulty.Pilot));
    // ...
}

private static T Require<T>(T property) where T : class
{
    RequireStore();
    return property ?? throw new InvalidOperationException("SettingsService.Initialize has not run.");
}
```

The high-score tables go into the same store as one JSON array per difficulty,
through a record type that is the stored shape and nothing else. Loading goes
back through the domain table's own insert, and anything malformed reads as no
records:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Settings/SettingsService.cs
public static HighScoreTable LoadHighScores()
{
    RequireStore();
    var table = new HighScoreTable();
    foreach (var difficulty in DifficultyTable.Levels)
    {
        foreach (var record in FromJson(AppSettingsService.Get(HighScoresKeyPrefix + difficulty, "[]")))
        {
            table.Insert(difficulty, record.Name, record.Score, record.Sector);
        }
    }

    return table;
}
// ...
public static IReadOnlyList<HighScoreRecord> FromJson(string json)
{
    if (string.IsNullOrWhiteSpace(json))
    {
        return Array.Empty<HighScoreRecord>();
    }

    try
    {
        var records = JsonSerializer.Deserialize<HighScoreRecord[]>(json);
        return records == null
            ? Array.Empty<HighScoreRecord>()
            : records.Where(record => record != null && record.Score > 0).ToArray();
    }
    catch (JsonException)
    {
        return Array.Empty<HighScoreRecord>();
    }
}
```

The game sees an interface, and the class it runs with only forwards:

```csharp
// From CodeBrix.Samples/BrixInvaders/src/libs/BrixInvaders.Game/Settings/IGameSettings.cs
/// <summary>
/// The persisted preferences and records the game session reads and writes. <see cref="StoredGameSettings"/> keeps
/// them in the AppSettings store through <see cref="SettingsService"/>; tests use an in-memory implementation.
/// </summary>
public interface IGameSettings
{
    /// <summary>The chosen ship shape, 0..2.</summary>
    int ShipShape { get; set; }
    // ...
    /// <summary>Loads every high-score table.</summary>
    /// <returns>The tables.</returns>
    HighScoreTable LoadHighScores();

    /// <summary>Saves one difficulty's table.</summary>
    /// <param name="table">The tables.</param>
    /// <param name="difficulty">The difficulty to save.</param>
    void SaveHighScores(HighScoreTable table, Difficulty difficulty);
    // ...
}
```

```csharp
// From CodeBrix.Samples/BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Support/MemoryGameSettings.cs
/// <summary>An in-memory <see cref="IGameSettings"/> with the documented defaults, recording saves.</summary>
internal sealed class MemoryGameSettings : IGameSettings
{
    private readonly Dictionary<Difficulty, int> _cleared = new Dictionary<Difficulty, int>();
    private HighScoreTable _stored = new HighScoreTable();
    // ...
    public double MasterVolume { get; set; } = SettingsService.DefaultMasterVolume;
    // ...
    public HighScoreTable LoadHighScores() => HighScoreTable.FromLines(_stored.ToLines());

    public void SaveHighScores(HighScoreTable table, Difficulty difficulty)
    {
        SaveCount++;
        _stored = HighScoreTable.FromLines(table.ToLines());
    }
    // ...
}
```

**Where to look.**
`BrixInvaders/src/libs/BrixInvaders.Game/Settings/SettingsService.cs`,
`IGameSettings.cs`, `StoredGameSettings.cs` and `HighScoreRecord.cs`
`BrixInvaders/src/libs/BrixInvaders.Game/Hosting/BrixInvadersGameHost.cs` (`OnInitializing`)
`BrixInvaders/src/libs/BrixInvaders.Game/Session/GameSession.cs` (`SubmitHighScore`)
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Settings/SettingsServiceTests.cs`
`BrixInvaders/tests/libs/BrixInvaders.Game.Tests/Support/MemoryGameSettings.cs`
and `Support/TempSettingsStore.cs`
`BrixInvaders/DESIGN.md` (the settings keys and their defaults)

**Sharp edges.**
- The typed properties exist only after the store opens. A read before that
  would be a null reference deep inside a getter; checking first turns it into
  an exception that says the store was never opened. Clear them again on
  shutdown, so a test can close the store and open a fresh one.
- Store an enum by name, parse it case-insensitively and check that the parsed
  value is defined. A renamed member or a hand-edited store then costs one value
  its default, not a crash at start-up.
- Validate on read as well as on write. The store can hold a value an older
  build wrote, or one outside today's range; clamping in the getter keeps it out
  of the game.
- Keep the stored record type apart from the domain type. The rules library's
  high-score entry references nothing, and the record with settable properties is
  the JSON shape a test pins exactly. Loading through the domain's insert re-ranks
  and trims the table, so a stored table in the wrong order or too long still
  loads correctly.
- Save a record the moment it is made. The session writes the table when the
  player enters a name, so a crash later in the run cannot lose it.
