# CodeBrix.Samples Blueprints: View models, commands and threading

These recipes cover the view model layer of a CodeBrix.Platform application:
SimpleViewModel properties that raise their own change notification,
SimpleCommand actions whose enabled state refreshes itself, computed
properties that keep value converters out of the XAML, and the design-mode
guard every constructor opens with. They go on to what a command actually
does - long jobs with progress, cancellation and a busy flag; results
marshalled back from capture and worker threads with InvokeOnMainThread;
stale answers dropped when the selection has already moved on; confirmations,
informational dialogs and error reporting raised from the view model; parent
and child view models sharing one page; and orderly disposal of commands,
event subscriptions and the delegates a page handed over. A further group is
about what the user picks and how much of it you load: enum-backed pickers,
drop-downs whose choices depend on the current selection, alerting and
reverting when the platform cannot honor a choice, and grids, trees and
search boxes that fill lazily rather than all at once. Reach for this file
when you are deciding what belongs on a view model rather than in a page,
or when bound state has to survive background work, a slow service, or a
user clicking faster than the application can answer.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Write bound properties and commands the family way](#write-bound-properties-and-commands-the-family-way)
- [Refresh CanExecute when the gating state is not a bound property](#refresh-canexecute-when-the-gating-state-is-not-a-bound-property)
- [Refresh command enablement in one pass from a headless command model](#refresh-command-enablement-in-one-pass-from-a-headless-command-model)
- [Give each grid cell its own command and lazily loaded thumbnail](#give-each-grid-cell-its-own-command-and-lazily-loaded-thumbnail)
- [Guard a view model constructor for the XAML designer](#guard-a-view-model-constructor-for-the-xaml-designer)
- [Kick off async startup loading from the view model constructor](#kick-off-async-startup-loading-from-the-view-model-constructor)
- [Load documents named on the command line during startup](#load-documents-named-on-the-command-line-during-startup)
- [Set bound properties from a background thread with InvokeOnMainThread](#set-bound-properties-from-a-background-thread-with-invokeonmainthread)
- [Hand results from a capture thread through a worker to the UI thread](#hand-results-from-a-capture-thread-through-a-worker-to-the-ui-thread)
- [Run a long job from a command with progress cancellation and a busy flag](#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag)
- [Report progress across stages when only some of them know a percentage](#report-progress-across-stages-when-only-some-of-them-know-a-percentage)
- [Snapshot view model state before a long running command](#snapshot-view-model-state-before-a-long-running-command)
- [Dispose a view model its commands and its bridge delegates](#dispose-a-view-model-its-commands-and-its-bridge-delegates)
- [Run one render per pane with latest request wins cancellation](#run-one-render-per-pane-with-latest-request-wins-cancellation)
- [Ignore a stale async result when the selection moved on](#ignore-a-stale-async-result-when-the-selection-moved-on)
- [Debounce a search box before rebuilding a filtered list](#debounce-a-search-box-before-rebuilding-a-filtered-list)
- [Fill a grid lazily as it scrolls](#fill-a-grid-lazily-as-it-scrolls)
- [Show and hide panes with computed Visibility properties](#show-and-hide-panes-with-computed-visibility-properties)
- [Load a tree lazily as the user expands it](#load-a-tree-lazily-as-the-user-expands-it)
- [Confirm and inform from the view model with SimpleViewModel dialogs](#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs)
- [Prompt before discarding unsaved work](#prompt-before-discarding-unsaved-work)
- [Gate an action behind a chosen folder and explain the gate with a dialog](#gate-an-action-behind-a-chosen-folder-and-explain-the-gate-with-a-dialog)
- [Report a failure as status text instead of throwing](#report-a-failure-as-status-text-instead-of-throwing)
- [Report a domain rule violation as a typed exception the view model can catch](#report-a-domain-rule-violation-as-a-typed-exception-the-view-model-can-catch)
- [Compose a page from a parent view model and child view models](#compose-a-page-from-a-parent-view-model-and-child-view-models)
- [Notify a value typed bindable property by hand](#notify-a-value-typed-bindable-property-by-hand)
- [Bind a picker to enum values with or without friendly labels](#bind-a-picker-to-enum-values-with-or-without-friendly-labels)
- [Stop a two way bound selection from commanding the control back](#stop-a-two-way-bound-selection-from-commanding-the-control-back)
- [Alert and revert when the user picks an unsupported option](#alert-and-revert-when-the-user-picks-an-unsupported-option)
- [Offer only the choices that make sense for the current selection](#offer-only-the-choices-that-make-sense-for-the-current-selection)
- [Settle an operation in a plan before running any of it](#settle-an-operation-in-a-plan-before-running-any-of-it)
- [Report the host operating system from the view model](#report-the-host-operating-system-from-the-view-model)
- [Cache rendered results with a bounded most recently used cache](#cache-rendered-results-with-a-bounded-most-recently-used-cache)
- [Signal a non property model change to the view with a version counter](#signal-a-non-property-model-change-to-the-view-with-a-version-counter)
- [Do blocking work in a service behind Task Run](#do-blocking-work-in-a-service-behind-task-run)
- [Load an asset off the UI thread and resolve its side files from the same container](#load-an-asset-off-the-ui-thread-and-resolve-its-side-files-from-the-same-container)
- [Pre warm a rendering backend off the UI thread](#pre-warm-a-rendering-backend-off-the-ui-thread)
- [Coalesce repaints and drop backlogged pointer frames](#coalesce-repaints-and-drop-backlogged-pointer-frames)
- [Run a sensor pipeline on a worker thread with latest frame wins](#run-a-sensor-pipeline-on-a-worker-thread-with-latest-frame-wins)
- [Survive a native runtime tearing down while a frame is in flight](#survive-a-native-runtime-tearing-down-while-a-frame-is-in-flight)
- [Publish a small immutable result type from a background pipeline](#publish-a-small-immutable-result-type-from-a-background-pipeline)
- [Capture a still and start a second pipeline from a command](#capture-a-still-and-start-a-second-pipeline-from-a-command)
- [Run an effect on worker threads with a live preview](#run-an-effect-on-worker-threads-with-a-live-preview)
- [Drive an undo history from a list and travel to a clicked point](#drive-an-undo-history-from-a-list-and-travel-to-a-clicked-point)
- [Bind a tab per open document and keep both directions in sync](#bind-a-tab-per-open-document-and-keep-both-directions-in-sync)
- [Show selection state in button captions from computed properties](#show-selection-state-in-button-captions-from-computed-properties)

## Related blueprints

- [BLUEPRINTS-PlatformServices.md](BLUEPRINTS-PlatformServices.md) - the recipes here resolve services and pickers through SimpleServiceResolver, and that file shows how those services reach the view model
- [BLUEPRINTS-ViewsAndControls.md](BLUEPRINTS-ViewsAndControls.md) - the page side of these bindings: XAML data contexts, data templates, and the bridge delegates a page hands its view model
- [BLUEPRINTS-AppStructureAndStartup.md](BLUEPRINTS-AppStructureAndStartup.md) - where the design-mode flag is cleared and the services these view models resolve are registered
- [BLUEPRINTS-Testing.md](BLUEPRINTS-Testing.md) - how these view models and their commands are exercised without a window

---

## View models, commands and threading

### Write bound properties and commands the family way

**When you want this.** You are writing your first `SimpleViewModel` and want the
exact shape the whole repository uses: bound properties, lazily created commands,
and buttons that enable themselves.

**The MVVM shape.** State is `field`-keyword auto-properties whose setters call
`SetProperty(ref field, value)`. Behavior is a `SimpleCommand` per action, created
lazily from a `CanXxx()` predicate and a `DoXxx()` handler. Anything a predicate
reads carries `[AffectsCommands(...)]` naming the commands it gates, so
`CanExecute` refreshes itself with no `RaiseCanExecuteChanged()` anywhere;
`[AffectsProperties(...)]` does the same for computed properties, and
`[AffectsAllCommands]` covers a flag that gates everything.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
[AffectsCommands(nameof(TakePhotoCommand))]
public bool HasFrame
{
    get;
    private set => SetProperty(ref field, value);
}

public CameraDevice SelectedCamera
{
    get;
    set
    {
        if (field != value)
        {
            SetProperty(ref field, value);
            SwitchCamera(value);
        }
    }
}

public string StatusText
{
    get;
    set => SetProperty(ref field, value ?? string.Empty);
} = string.Empty;
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
private SimpleCommand _takePhotoCommand;
public SimpleCommand TakePhotoCommand =>
    (_takePhotoCommand ??= new SimpleCommand(CanTakePhoto, DoTakePhoto));

private bool CanTakePhoto() => (!IsBusy) && IsCaptureMode && HasFrame;

private async Task DoTakePhoto()
{
    if (!CanTakePhoto()) { return; }
    // ...
}

private SimpleCommand _selectColorCommand;
public SimpleCommand SelectColorCommand =>
    (_selectColorCommand ??= new SimpleCommand(CanSelectColor, (Action<object>)DoSelectColor));

private void DoSelectColor(object parameter)
{
    var session = _paintSession;
    if (session != null && parameter is string colorName && session.SelectColor(colorName))
    {
        ActiveColorText = $"Painting with: {session.ActiveColorName}";
    }
}
```

A property can gate a command and a computed property at once:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
[AffectsCommands(nameof(CreateCommand), nameof(LoadWholeTreeCommand))]
[AffectsProperties(nameof(TreePlaceholderVisibility), nameof(TreeVisibility))]
public bool IsConnected
{
    get;
    private set => SetProperty(ref field, value);
}

// ...

private SimpleCommand _createCommand;
public SimpleCommand CreateCommand =>
    (_createCommand ??= new SimpleCommand(CanCreate, DoCreate));

private bool CanCreate() =>
    (!IsBusy)
    && IsConnected
    && (!string.IsNullOrWhiteSpace(OutputFilePath))
    && CheckedCount > 0;
```

The page's side of the contract is a plain binding, with
`UpdateSourceTrigger=PropertyChanged` where a button should follow typing:

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml -->
<TextBox Grid.Column="0" Height="40"
         VerticalAlignment="Center" VerticalContentAlignment="Center"
         Text="{d:Binding MediaAddress, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
<Button Grid.Column="1" Margin="8,0,0,0" Height="40"
        VerticalAlignment="Center" Content="Load"
        Command="{d:Binding LoadCommand}" />
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`,
`PainDiagram/Shared/ViewModels/MainViewModel.cs`,
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`,
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`,
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs`

**Sharp edges.**
- An asynchronous command body needs an explicit cast so the right
  `SimpleCommand` overload is chosen: `(Func<object, Task>)(_ => RunAsync())` or
  `(Func<Task>)(() => StepAsync(...))`. Without it a `Task`-returning lambda binds
  to the synchronous `Action` overload and the command completes immediately while
  the work runs unobserved. A parameterized synchronous command needs
  `(Action<object>)` for the same reason.
- Every `DoXxx()` re-checks its own `CanXxx()` on the first line. `CanExecute` is a
  UI hint, not a guarantee, because a command can also be invoked
  programmatically or while the UI has not refreshed yet.
- `[AffectsCommands]` takes command property names, so renaming a command without
  updating the attribute silently stops refreshing the button. `nameof` keeps that
  honest.
- Commands are kept in explicit backing fields precisely so `Dispose()` can reach
  them. `field ??=` on an expression-bodied command property works too and creates
  the command once; a plain `=> new SimpleCommand(...)` would hand a fresh
  instance to every binding, and `RaiseCanExecuteChanged()` would then update a
  command nothing is bound to.
- A computed companion property (`IsVisualizeMode => !IsCameraMode`) needs an
  explicit `NotifyPropertyChanged` from the setter it depends on, unless the
  source property lists it in `[AffectsProperties]`.
- Setters normalize `null` to `string.Empty`, so predicates never have to
  null-check separately.
- One `CanExecute` in JustBetweenUs deliberately leaves out a validity check: the
  commented-out `IsBase64Text(EnteredText)` in `CanDecrypt` records that including
  it made the Decrypt button flash on and off as the user typed. The check moved
  into the command body, which shows an informational message instead.

### Refresh CanExecute when the gating state is not a bound property

**When you want this.** Your buttons are enabled by facts that live in a model
object, not by properties on the view model, so `[AffectsCommands]` has nothing to
hang on.

**The MVVM shape.** Use `[AffectsAllCommands]` for the one real bound property
that gates everything, and call `RaiseCanExecuteChanged()` explicitly from the
single method that already runs whenever the model moved. Both the predicate and
the body read the model directly, so the view model never mirrors model state into
properties of its own.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    /// <summary>Whether a file picker or document open is in progress (blocks the navigation buttons).</summary>
    [AffectsAllCommands]
    public bool IsBusy
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Main Down: both documents to their next page.</summary>
    public SimpleCommand NextPageCommand => field ??=
        new SimpleCommand(() => !IsBusy && _comparison.CanMoveBothNext,
            (Func<Task>)(() => StepAsync(_comparison.MoveBothNext, renderLeft: true)));

    //Tell the page the view (zoom/pan/page) moved and refresh every button that depends on it
    private void ViewChanged()
    {
        ViewVersion++;
        NotifyPropertyChanged(nameof(ZoomLabel));
        RaiseNavigationCanExecute();
    }

    private void RaiseNavigationCanExecute()
    {
        PreviousPageCommand.RaiseCanExecuteChanged();
        NextPageCommand.RaiseCanExecuteChanged();
        AdjustPreviousCommand.RaiseCanExecuteChanged();
        AdjustNextCommand.RaiseCanExecuteChanged();
        ZoomInCommand.RaiseCanExecuteChanged();
        ZoomOutCommand.RaiseCanExecuteChanged();
        ZoomResetCommand.RaiseCanExecuteChanged();
        PanCommand.RaiseCanExecuteChanged();
    }
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs` (a
private `_isCreatingDocument` field with `DocumentCommand.RaiseCanExecuteChanged()`
at both ends of the run)

**Sharp edges.**
- Funnel every model change through one method. Adding a new kind of change then
  means calling that one method rather than remembering three separate things.
- `[AffectsAllCommands]` handles the busy flag; everything else still needs the
  explicit raise.

### Refresh command enablement in one pass from a headless command model

**When you want this.** Dozens of commands whose enabled state depends on the same
few facts, declared in a headless library rather than as `SimpleCommand`
properties on a view model.

**The MVVM shape.** One method recomputes every command's enabled state from
current state, called from a single "something about the document changed" funnel
that every model event routes through. This is the manual version of what
`[AffectsCommands]` automates.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Actions.cs
/// <summary>
/// Enables and disables commands to match the current document, selection
/// and history state. Upstream drove this from a scattering of event
/// handlers; doing it in one pass makes the rules visible in one place.
/// </summary>
private void UpdateActionSensitivity()
{
    ActionManager actions = PintaCore.Actions;

    bool hasDocument = PintaCore.Workspace.HasOpenDocuments;
    // ...
    foreach (Command command in actions.View.Commands())
    {
        //The visibility toggles stay usable with no document open; only the
        //zoom commands need one.
        if (command is not ToggleCommand)
            command.Sensitive = hasDocument;
    }
    // ...
    Document document = PintaCore.Workspace.ActiveDocument;
    DocumentHistory history = document.History;

    actions.Edit.Undo.Sensitive = history.CanUndo;
    actions.Edit.Redo.Sensitive = history.CanRedo;

    bool hasSelection = document.Selection.Visible;
    actions.Edit.Deselect.Sensitive = hasSelection;
    actions.Image.CropToSelection.Sensitive = hasSelection;
    actions.View.ZoomToSelection.Sensitive = hasSelection;

    int layerCount = document.Layers.Count();
    int currentIndex = document.Layers.CurrentUserLayerIndex;

    actions.Layers.DeleteLayer.Sensitive = layerCount > 1;
    actions.Layers.MergeLayerDown.Sensitive = currentIndex > 0;
    actions.Layers.MoveLayerUp.Sensitive = currentIndex < layerCount - 1;
    actions.Image.Flatten.Sensitive = layerCount > 1;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
/// <summary>
/// One place for "something about the document changed" - the pads and the
/// command enablement both follow from it.
/// </summary>
private void OnDocumentStateChanged()
{
    RefreshLayersPad();
    RefreshHistoryPad();
    UpdateActionSensitivity();
    UpdateSelectionSizeText();
}
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Actions.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- The single funnel is what keeps six different model events from each having to
  know which commands they affect.
- The no-document branch resets the state-dependent commands explicitly, so a
  stale enabled state never survives a document close.
- Prefer `SimpleCommand` with `[AffectsCommands]` when the commands can live on a
  view model; reach for this shape only when the command model is owned by a
  headless library.

### Give each grid cell its own command and lazily loaded thumbnail

**When you want this.** A data-templated list or grid whose template should bind
to its own item, where each item lazily fetches an image and its button may also
depend on application-wide state.

**The MVVM shape.** A cell view model per item, holding display text plus
delegates the owner supplies: what opening the cell does, how its thumbnail bytes
are fetched, and (where needed) whether the action is currently allowed. The
template then binds a plain `{Binding OpenCommand}` and `{Binding Thumbnail}` with
no `ElementName` or ancestor lookups, and the cell type stays independently
testable because it holds delegates rather than a reference to its owner.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AssetCellViewModel.cs
/// <summary>
/// Creates a cell for one asset. The owning view model supplies what opening the asset
/// does (openAsync) and how the thumbnail's bytes are fetched (thumbnailBytesAsync,
/// <c>null</c> for kinds with no thumbnail).
/// </summary>
public AssetCellViewModel(string title, AssetCellKind kind, string kindLabel, string glyph,
    string subtitle, string detailText, object payload,
    Func<AssetCellViewModel, Task> openAsync, Func<Task<byte[]>> thumbnailBytesAsync)
{ /* ... */ }

/// <summary>
/// Opens this cell's asset in the viewer. Living on the cell itself keeps the cell
/// template's binding a plain <c>{Binding OpenCommand}</c> - a template binds to its own item.
/// </summary>
public SimpleCommand OpenCommand => field ??=
    new SimpleCommand((Func<object, Task>)(_ => _openAsync(this)));

/// <summary>The placeholder glyph's visibility (shown until a thumbnail arrives, or always for kinds without one).</summary>
public Visibility PlaceholderVisibility => _thumbnail == null ? Visibility.Visible : Visibility.Collapsed;

public async Task LoadThumbnailAsync()
{
    if (_thumbnail != null || _thumbnailFailed || _thumbnailBytesAsync == null) { return; }

    try
    {
        var bytes = await _thumbnailBytesAsync();
        if (bytes == null) { _thumbnailFailed = true; return; }

        //Back on the UI thread here (the awaiter restores the dispatcher context), which
        //is where BitmapImage wants to be touched.
        var image = new BitmapImage();
        using (var stream = new MemoryStream(bytes))
        {
            await image.SetSourceAsync(stream.AsRandomAccessStream());
        }
        Thumbnail = image;
    }
    catch (Exception)
    {
        //A missing thumbnail is cosmetic; the cell simply keeps its placeholder.
        _thumbnailFailed = true;
    }
}
```

The owner wires the delegates when it builds the list:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
var thumbnailLoader = kind switch
{
    AssetCellKind.Image => (Func<Task<byte[]>>)(() => ReadArchiveBytesAsync(entry.EntryPath)),
    AssetCellKind.Vector => () => ReadSvgThumbnailAsync(entry.EntryPath),
    _ => null,
};

return new AssetCellViewModel(
    entry.Name, kind, kindLabel, glyph, subtitle, sizeText, entry, OpenAssetAsync, thumbnailLoader);
```

**Variant: an application-wide gate on a per-cell command.** PolyHavenBrowser
injects the gate as a delegate too, and pokes every materialized cell when it
changes:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/ModelCellViewModel.cs
public SimpleCommand DownloadCommand => _downloadCommand ??=
    new SimpleCommand(() => _canDownload(), _ => _downloadAsync(this));

/// <summary>
/// Lets the owning view model tell this cell's Download button to re-query its enabled
/// state (called on every cell when a download starts or finishes).
/// </summary>
public void NotifyCanDownloadChanged() => _downloadCommand?.RaiseCanExecuteChanged();
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
public bool IsDownloading
{
    get;
    private set
    {
        SetProperty(ref field, value);
        NotifyPropertyChanged(nameof(DownloadBarVisibility));

        //The download gate lives on each cell's own command; tell every materialized
        //cell to re-query it. (Cells materialized later evaluate the gate fresh anyway.)
        if (Cells is { } cells)
        {
            foreach (var cell in cells) { cell.NotifyCanDownloadChanged(); }
        }
    }
}
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AssetCellViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/BundleCellViewModel.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/ModelCellViewModel.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Converters/NullToVisibilityConverter.cs`
(the same "placeholder until the image arrives" idea done with a converter rather
than a computed `Visibility` property)

**Sharp edges.**
- `BitmapImage` wants to be created and filled on the UI thread. Awaiting the fetch
  restores the dispatcher context, so the construction after the `await` is
  already in the right place; the code says so in a comment.
- Guard on both "already have one" and "already failed", because a lazily filling
  collection may ask a cell to load more than once, and a failed fetch should
  never be retried on every rescroll.
- Cells whose kind has no thumbnail get a `null` loader and return immediately.
- Making the whole card a `Button` bound to the cell's command gives keyboard and
  hover behavior for free.
- The lazy command creation plus `?.` on the refresh means a cell whose button was
  never realized costs nothing.

### Guard a view model constructor for the XAML designer

**When you want this.** The page declares its view model in XAML, so the designer
constructs it too, and the constructor does real work: opening cameras, starting
threads, hitting the network.

**The MVVM shape.** The first line of the constructor is
`if (IsDesignMode(true)) { return; }`. At run time `SetIsDesignMode(false)` has
already been called during application startup, so the guard falls through. In the
designer it returns immediately and only the property initializers run, which is
where design-time values come from.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs
[Microsoft.UI.Xaml.Data.Bindable]
public class MainViewModel : SimpleViewModel
{
    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Debug.WriteLine("Main view model startup.");

        //Load (and, because the player has AutoPlay enabled, start) the default media on startup
        LoadMedia();
    }
    // ...
}
```

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml -->
<Page.DataContext>
    <vm:MainViewModel />
</Page.DataContext>
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs`,
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`,
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs` and
`.../ViewModels/DocumentPaneViewModel.cs`,
`Pinta.Brix/src/Pinta.Brix.Core/ViewModels/MainViewModel.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`,
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs` and
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs` (both wrap the whole body
in `if (!IsDesignMode(true)) { ... }` instead of returning early)

**Sharp edges.**
- The comment is part of the pattern: the guard must be the first line, before any
  field is assigned or any service resolved.
- The pairing is easy to get half right. Without `SetIsDesignMode(false)` in `App`,
  the run-time constructor also returns early and the application silently does
  nothing at all.
- A child view model needs the guard too, and because it returns early in design
  mode its constructor-assigned members stay null then.
- `[Microsoft.UI.Xaml.Data.Bindable]` on the class is what makes the type usable
  as a binding source. Applications that also compile the view model into a native
  head put it behind `#if HAS_CODEBRIX`.

### Kick off async startup loading from the view model constructor

**When you want this.** The page must show something immediately while its data
arrives, and must show a readable message when the load fails.

**The MVVM shape.** The constructor sets up synchronous state and starts one async
method without awaiting it. That method sets bound state, flips a loading flag,
and turns a failure into text on screen rather than an exception.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
public bool IsCatalogLoading
{
    get;
    private set
    {
        SetProperty(ref field, value);
        NotifyPropertyChanged(nameof(CatalogLoadingVisibility));
    }
} = true;

public Visibility CatalogLoadingVisibility => IsCatalogLoading ? Visibility.Visible : Visibility.Collapsed;

public string CatalogStatusText
{
    get;
    private set => SetProperty(ref field, value);
} = "Loading the Poly Haven model catalog…";

private async Task LoadCatalogAsync()
{
    try
    {
        _allModels = await _catalog.GetModelsAsync(CancellationToken.None);
        IsCatalogLoading = false;
        RebuildCells();
    }
    catch (Exception ex)
    {
        CatalogStatusText = $"Could not load the Poly Haven catalog: {ex.Message}";
    }
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
public MainViewModel()
{
    if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

    _catalogService = GetService<AssetCatalogService>();

    _assetsFolder = SettingsService.Get<string>(AssetsFolderKey);
    if (HasAssetsFolder)
    {
        _ = ReloadCatalogAsync();
    }
}
```

**Variant: name the task so a page or a test can await it.**

```csharp
// Adapted from CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
// The sample starts a fire-and-forget Task in the constructor and pads it with a
// fixed Task.Delay before showing its first dialog; this version keeps the same
// steps but names the initialization so a page or a test can await it.
public MainViewModel()
{
    if (!IsDesignMode(true))
    {
        _encryptSvc = GetService<IEncryptionService>();
        // ... fill the picker list and select the first entry ...
        Initialization = InitializeAsync();
    }
}

public Task Initialization { get; private set; } = Task.CompletedTask;

private async Task InitializeAsync()
{
    var defaultKey = await _encryptSvc.GetDefaultKey();
    InvokeOnMainThread(() => EncryptionKey = defaultKey);
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`

**Also shown by.**
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`
(`_ = InitializeAsync();` after setting a "Discovering cameras…" status),
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The discard (`_ = ...`) is deliberate, and every exception is caught inside, so
  nothing is left unobserved. A constructor that awaits would block page
  construction.
- On failure PolyHavenBrowser deliberately leaves the loading indicator visible
  with the error text under it, rather than leaving the user staring at an empty
  grid.
- Work started this early can complete before the page has handed the view model a
  `XamlRoot`, and a dialog raised then has nowhere to attach. JustBetweenUs pads
  its first dialog with a fixed delay and its own comment records that the real
  fix is awaiting a page-readiness signal; prefer a readiness signal in your own
  code.

### Load documents named on the command line during startup

**When you want this.** Repeating the same task, or launching from a script,
without clicking through file pickers first.

**The MVVM shape.** A fire-and-forget async method started from the view-model
constructor, guarded by the busy flag, with every failure funnelled into the
standard error dialog. The head's `Main` does nothing special; the view model
reads the process arguments itself.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    /// <summary>
    /// Convenience for repeated comparisons: launching a head as
    /// PdfSideBySide.LinuxX11 left.pdf right.pdf pre-loads the two documents, so the
    /// user need not browse for them. Anything that goes wrong is reported in the status line.
    /// </summary>
    private async Task OpenStartupDocumentsAsync()
    {
        var arguments = Environment.GetCommandLineArgs();
        if (arguments.Length < 3) { return; }

        IsBusy = true;
        try
        {
            LeftPane.ShowDocument(await _comparison.OpenAsync(DocumentSide.Left, arguments[1]));
            RightPane.ShowDocument(await _comparison.OpenAsync(DocumentSide.Right, arguments[2]));
            UpdateStatus();
            ViewChanged();
            await Task.WhenAll(RenderSideAsync(DocumentSide.Left), RenderSideAsync(DocumentSide.Right));
        }
        catch (Exception e)
        {
            await ShowError(e, "Could not open the documents given on the command line.");
        }
        finally
        {
            IsBusy = false;
        }
    }
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- `Environment.GetCommandLineArgs()` includes the executable at index 0, so the
  two paths are at indices 1 and 2 and the guard is `arguments.Length < 3`. The
  `string[] args` passed to a head's `Main` is never forwarded anywhere.
- Starting it from the constructor means it can finish before the page has handed
  the view model a `XamlRoot`, so an error dialog raised this early has nowhere to
  attach. Deferring the load until the page signals it is ready is safer.

### Set bound properties from a background thread with InvokeOnMainThread

**When you want this.** Work finished off the UI thread and you need to push the
result into a bound property, or call a head-supplied delegate.

**The MVVM shape.** The view model owns the marshalling. Wrap the assignment in
`InvokeOnMainThread`, an inherited `SimpleViewModel` member, so the same code is
correct on every head. Everything that does not touch bound state stays on the
raising thread.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
var defaultKey = await _encryptSvc.GetDefaultKey();
//We can't set a value to EncryptionKey except on the main (UI) thread, because this causes problems on Linux and macOS
InvokeOnMainThread(() => EncryptionKey = defaultKey);
```

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
_session.DrawingChanged += (_, _) => InvokeOnMainThread(() => HasDrawing = _session.HasStrokes);
```

**Where to look.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`
`PainDiagram/Shared/ViewModels/MainViewModel.cs`

**Also shown by.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`,
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`,
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`,
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The comment in JustBetweenUs is the rule worth remembering: assigning a bound
  property off the UI thread appears to work on Windows and fails on Linux and
  macOS. Test the marshalling on the strictest head, not the most forgiving one.
- The same wrapper is used for calling head-supplied bridge delegates, not only
  for property assignment, because a clipboard or canvas API is usually
  main-thread only as well.
- An assignment that also drives `[AffectsCommands]` must be marshalled for the
  same reason: refreshing a command's `CanExecute` touches the UI.
- A `Progress<T>` constructed on the UI thread already marshals its callbacks; one
  handed to a service from a worker thread does not. Check which case you are in
  before adding a second layer of marshalling.

### Hand results from a capture thread through a worker to the UI thread

**When you want this.** Three threads are involved - a sensor callback, a
processing worker, and the UI - and only the view model should decide what the UI
sees.

**The MVVM shape.** The capture-thread handler does the minimum and forwards
pixels to the worker. The worker-thread handler feeds anything thread-safe
straight in, and wraps only what touches bound state in `InvokeOnMainThread` -
and only when it actually changed.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
private void OnFrameArrived(object sender, EventArgs e)
{
    //Capture-thread context: get out fast
    if (!HasFrame)
    {
        InvokeOnMainThread(() => HasFrame = _captureService.HasFrame);
    }

    if (IsCaptureMode)
    {
        InvalidateMainCanvas?.Invoke();
    }
    else
    {
        //Paint Mode: the live feed drives the hand tracker and the little self-view
        var tracker = _tracker;
        if (tracker is { IsRunning: true }
            && _captureService.TryCopyLatestFrame(ref _visionFrame, out var width, out var height))
        {
            tracker.SubmitFrame(_visionFrame, width, height);
        }
        InvalidateSelfView?.Invoke();
    }
}

private void OnTrackingUpdated(object sender, HandTrackingEventArgs e)
{
    //Worker-thread context: marshal all painting decisions onto the UI thread
    var result = e.Result;
    InvokeOnMainThread(() =>
    {
        var session = _paintSession;
        if (IsCaptureMode || session == null) { return; }
        // ... update crosshair, begin/continue/end the stroke ...
        InvalidateMainCanvas?.Invoke();
    });
}
```

PalmVisualizer shows the other half of the trade: when the consumer is itself
thread-safe, only the status line needs the dispatcher, and only when it changes.

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
private void OnTrackingUpdated(object sender, PalmTrackingEventArgs e)
{
    //Worker-thread context: the visualizer's attractor field is thread-safe, so the
    //  palms feed straight in - only the status line needs the UI thread
    var session = _visualizerSession;
    if (IsCameraMode || session == null) { return; }

    var attractors = new List<PalmAttractor>(e.Result.Palms.Count);
    foreach (var palm in e.Result.Palms)
    {
        //Only OPEN palms attract the colors - and the user watched a mirrored
        //  preview, so mirror the palm positions to match
        if (palm.IsOpenPalm)
        {
            attractors.Add(new PalmAttractor(palm.TrackId, 1f - palm.PalmCenterX, palm.PalmCenterY));
        }
    }
    session.UpdatePalms(attractors);

    var openCount = attractors.Count;
    if (openCount != _reportedOpenPalmCount)
    {
        _reportedOpenPalmCount = openCount;
        InvokeOnMainThread(() => StatusText = openCount switch
        {
            0 => "Show the camera your open palm - the colors will gather toward it.",
            1 => "The colors are chasing your open palm - close your hand to set them free.",
            _ => $"The colors are chasing {openCount} open palms - close your hands to set them free.",
        });
    }
}
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Read a field another thread can null into a local first
  (`var tracker = _tracker;`, `var session = _paintSession;`) and then use the
  local, so a concurrent `Dispose()` cannot turn the null check into a race.
- Dispatch only when something changed. Without the `if (openCount != ...)` and
  `if (!HasFrame)` guards, the UI thread takes a dispatch on every processed
  frame.
- Re-check the mode inside the marshalled callback: by the time it runs the user
  may already have pressed Back.
- The frame handler is the one place that decides where a frame goes - repaint in
  one mode, inference in the other - so a single camera feed serves two consumers
  with no duplicated capture.
- Coordinate conventions are reconciled in exactly one place. The tracker reports
  positions in unmirrored camera space and the preview is mirrored by a canvas
  transform, so the view model applies `1f - x` once, where both conventions meet.

### Run a long job from a command with progress cancellation and a busy flag

**When you want this.** The canonical long-running-operation shape: a Run command,
a Cancel command that stays live, a progress bar, a status line, and everything
else disabled.

**The MVVM shape.** `IsRunning` (or `IsBusy`) and `IsCancelling` are
`[AffectsCommands]` properties, so pressing Run disables Run and enables Cancel
with no manual refresh. The service takes an `IProgress<T>` and a
`CancellationToken` and knows nothing about the UI. The `CancellationTokenSource`
is a field, disposed and nulled in a `finally` that also clears the flags.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs
public SimpleCommand RunCommand => field ??= new SimpleCommand(
    () => !IsRunning && Source is not null && SelectedDestination is not null,
    (Func<object, Task>)(_ => RunAsync()));

public SimpleCommand CancelCommand => field ??= new SimpleCommand(
    () => IsRunning && !IsCancelling, _ => DoCancel());

private async Task RunAsync()
{
    // ... choose the output path, build the plan ...

    //The notes on screen belong to the run named in the status bar, so they go the moment a new
    //run takes that line over.
    SetLastRunNotes([]);

    IsRunning = true;
    IsCancelling = false;
    ProgressPercent = 0d;
    IsProgressIndeterminate = true;
    ProgressText = "Starting...";
    StatusText = plan.ToString();

    cancellation = new CancellationTokenSource();
    var progress = new Progress<ConversionProgress>(report =>
    {
        ProgressPercent = report.OverallPercent;
        IsProgressIndeterminate = report.IsIndeterminate;
        ProgressText = report.ToString();
    });

    ConversionOutcome outcome;
    try
    {
        outcome = await runner.RunAsync(plan, progress, cancellation.Token);
    }
    finally
    {
        cancellation.Dispose();
        cancellation = null;
        IsRunning = false;
        IsCancelling = false;
    }

    ProgressPercent = outcome.Succeeded ? 100d : 0d;
    IsProgressIndeterminate = false;
    ProgressText = string.Empty;
    StatusText = outcome.ToString();
    SetLastRunNotes(DescribeOutcome(outcome, destination));

    ConversionFinished?.Invoke(this, outcome);
}

private void DoCancel()
{
    if (cancellation is null)
    {
        return;
    }

    IsCancelling = true;
    ProgressText = "Stopping...";
    cancellation.Cancel();
}
```

Where the service reports from an arbitrary thread, the progress callback does the
marshalling itself:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
private async Task DoCreate()
{
    if (!CanCreate()) { return; }
    // ...
    try
    {
        IsBusy = true;
        ProgressValue = 0;
        // ...
        var progress = new Progress<CreateProgress>(p => InvokeOnMainThread(() =>
        {
            StatusText = p.Message;
            ProgressValue = p.PercentComplete;
        }));

        var result = await _documentSvc.CreateDocumentAsync(request, progress);

        StatusText = $"Saved: {result.OutputFilePath}";
        await ShowInfo(BuildResultMessage(result));
    }
    catch (Exception e)
    {
        StatusText = "Creation failed.";
        await ShowError($"Error while creating the document: {e.Message}");
    }
    finally
    {
        ProgressValue = 0;
        IsBusy = false;
    }
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Models/RenderModels.cs
/// <summary>
/// The stages a render moves through, in order (useful for progress display).
/// </summary>
public enum RenderStage
{
    FetchingArticle = 0,
    ParsingArticle,
    DownloadingImages,
    ComposingBook,
    SavingPdf,
    Done
}

/// <summary>
/// A progress report raised while rendering.
/// </summary>
public sealed record RenderProgress(RenderStage Stage, string Message, int PercentComplete);
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`

**Also shown by.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
(one `IsBusy` naming three commands, an indeterminate bar bound to
`BusyVisibility`, and `IProgress<string>` straight into `StatusText`),
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- A `Progress<T>` created on the UI thread already posts its callbacks back there;
  one whose callback runs wherever the service happens to be needs an explicit
  `InvokeOnMainThread`. Both forms appear in this repository, and each is correct
  for its own service.
- Reset the progress value in `finally`, so a failed run does not leave the bar
  part-filled, and clear the busy flag there too.
- Cancellation should not travel as an exception out of the command. The video
  tool's service catches `OperationCanceledException` itself and returns a
  cancelled outcome, so the view model has one exit path; it also deletes the
  part-written output on cancel and on failure.
- Ask for confirmation before setting the busy flag, so a cancelled overwrite
  prompt never leaves the UI in a busy state.
- A progress record that carries a stage enum as well as a message and a
  percentage lets a UI render a stage list rather than only a bar, and computing
  the percentage as a band per stage stops the bar going backwards between stages.
- Make `IProgress<T>` optional on the service; the offline tests rely on passing
  `null`.

### Report progress across stages when only some of them know a percentage

**When you want this.** An operation has a preparation stage whose length cannot
be known and a working stage that can report a real percentage, and you want one
honest bar.

**The MVVM shape.** A small immutable report type carrying the stage name, its
number, the stage count and a nullable percentage, with `IsIndeterminate` and
`OverallPercent` derived on it. The view model copies three values out of each
report; the progress bar binds `Value` and `IsIndeterminate`.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionProgress.cs
/// <remarks>
/// Every stage that runs FFmpeg reports a real percentage, because FFmpeg says where in the media it
/// has reached and the source's duration is known. A stage that does not run FFmpeg - reading a
/// bespoke container, muxing an intermediate - reports no percentage at all, and the progress bar
/// shows that it is working rather than inventing a number.
/// </remarks>
public sealed class ConversionProgress
{
    // ...
    public bool IsIndeterminate => StagePercent is null;

    /// <remarks>
    /// A stage with no percentage of its own counts as half-done, so the bar still moves forward
    /// when one finishes rather than sitting still until the last stage starts.
    /// </remarks>
    public double OverallPercent
    {
        get
        {
            var within = Math.Clamp(StagePercent ?? 50d, 0d, 100d);
            var completed = Math.Max(0, StageNumber - 1);
            return Math.Clamp(((completed * 100d) + within) / StageCount, 0d, 100d);
        }
    }

    public override string ToString() => StagePercent is null
        ? $"{Stage} ({StageNumber} of {StageCount})"
        : $"{Stage} ({StageNumber} of {StageCount}) - {StagePercent:F0}%";
}
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<ProgressBar Grid.Column="0"
             Height="6"
             Minimum="0"
             Maximum="100"
             Value="{d:Binding Conversion.ProgressPercent}"
             IsIndeterminate="{d:Binding Conversion.IsProgressIndeterminate}"
             VerticalAlignment="Center"
             Margin="0,0,14,0" />
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionProgress.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs`

**Sharp edges.**
- The stage count is fixed for every run, so the bar never rescales mid-operation.
- Where an underlying library reports per-pass, the runner folds pass number and
  pass count into one within-stage percentage before reporting.
- A stage with no percentage counts as half-done, so the bar advances when it ends
  instead of sitting at zero.

### Snapshot view model state before a long running command

**When you want this.** A command takes many seconds and the user is free to
navigate away or change the selection while it runs.

**The MVVM shape.** Copy everything the run needs into locals at the top, so the
run depends on nothing that can change underneath it. Guard re-entry with a flag,
refresh the command at both ends, and announce completion after the `finally` so
the button is live again by the time the user dismisses the dialog.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
private bool CanCreateDocument() => IsModelViewActive && !_isCreatingDocument;

private async Task CreateDocumentAsync()
{
    if (!CanCreateDocument()) { return; }

    //Snapshot everything the document needs: the user can navigate Back (or open a
    //  different model) while it builds, and the run continues from this snapshot.
    var asset = _currentAsset;
    var stats = _currentStats;
    var model = _currentModel;
    if (asset == null || stats == null || model == null) { return; }

    var title = ModelTitle;
    // ... authorLine, description, facts, downloadFolder ...

    // ... pick the output path ...

    _isCreatingDocument = true;
    DocumentCommand.RaiseCanExecuteChanged();
    var saved = false;
    try
    {
        // ... stages 1-4, then: ...
        await Task.Run(() => new MarketingSheetCreator().CreateToFile(request, outputPath));
        DocumentStatusText = $"Saved: {outputPath}";
        saved = true;
    }
    catch (Exception e)
    {
        DocumentStatusText = string.Empty;
        await ShowError(e, $"Could not create the marketing one-sheet for “{title}”.");
    }
    finally
    {
        _isCreatingDocument = false;
        DocumentCommand.RaiseCanExecuteChanged();
    }

    if (saved)
    {
        //Say so plainly: creating the sheet takes a while, and the footer status line is
        //  easy to miss. Announced after the finally block so the Document button is live
        //  again by the time the user dismisses this.
        using var alert = CreateDialog(
            $"The marketing one-sheet for “{title}” has been created.\n\n" +
            $"It was saved to:\n{outputPath}",
            "Document Created");
        _ = await alert.ShowAsync();
    }
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Setting the status line per stage is what makes a multi-second command
  tolerable.
- Announcing success after the `finally` block, rather than inside the `try`, is
  what leaves the button live while the dialog is up.

### Dispose a view model its commands and its bridge delegates

**When you want this.** A view model that holds commands, service references,
delegates the page handed it, and possibly threads and native handles.

**The MVVM shape.** Override `Dispose()`. Dispose and null each command, null
every bridge delegate (each one captures the page and would keep it alive),
unsubscribe every event before disposing its source, release service references
without disposing container singletons, and call `base.Dispose()` last.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
public override void Dispose()
{
    _takePhotoCommand?.Dispose();
    _takePhotoCommand = null;
    // ... the other four commands ...

    PickSaveJpegPathAsync = null;
    InvalidateMainCanvas = null;
    InvalidateSelfView = null;

    if (_tracker != null)
    {
        _tracker.TrackingUpdated -= OnTrackingUpdated;
        _tracker.Dispose();
        _tracker = null;
    }

    var session = _paintSession;
    _paintSession = null;
    session?.Dispose();

    if (_captureService != null)
    {
        _captureService.FrameArrived -= OnFrameArrived;
        _captureService.Dispose();
        _captureService = null;
    }

    base.Dispose();
}
```

The minimal version, for a view model that owns one command and nothing else:

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs
#region | IDisposable implementation |

public override void Dispose()
{
    _loadCommand?.Dispose();
    _loadCommand = null;
    base.Dispose();
}

#endregion
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`,
`PainDiagram/Shared/ViewModels/MainViewModel.cs`,
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Null the field before disposing the object
  (`var session = _paintSession; _paintSession = null; session?.Dispose();`), so a
  callback arriving mid-teardown sees null instead of a disposed object. The same
  applies to an engine session: null it, then `Stop()` it.
- Unsubscribe in both directions. The view model unsubscribes from each source,
  and the library classes null their own event before stopping
  (`TrackingUpdated = null; Stop();`), which guarantees no handler runs during
  teardown.
- Nulling the bridge delegates is what actually releases the page; a delegate
  captured in the page's constructor holds the page alive through the view model
  until it is cleared.
- A container singleton is released, not disposed: the view model drops its
  reference and leaves the lifetime to the container.
- Disposable library objects the view model created itself do get disposed, and a
  field rather than a get-only property is what makes that possible - the public
  property stays `=> _field`, so every consumer's null-conditional access keeps
  working after disposal.
- A command created through the `field` keyword cannot be reached from `Dispose()`,
  so use an explicit field for any command that owns resources.

### Run one render per pane with latest request wins cancellation

**When you want this.** The user is clicking faster than results render, and you
want the newest request to win without older ones painting stale output.

**The MVVM shape.** One `CancellationTokenSource` per independent region. Starting
work cancels the previous one for that region, sets its busy flag, awaits the
service, and only pushes the result if its own token was not cancelled.
`OperationCanceledException` is swallowed silently: it is the expected outcome,
not a fault.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    //One in-flight render per side; a newer page request cancels the older one
    private CancellationTokenSource _leftRender;
    private CancellationTokenSource _rightRender;
    // ...
    private async Task RenderSideAsync(DocumentSide side)
    {
        var document = _comparison.GetDocument(side);
        if (document == null) { return; }

        //Supersede whatever render was in flight for this side
        var previous = side == DocumentSide.Left ? _leftRender : _rightRender;
        previous?.Cancel();
        var cts = new CancellationTokenSource();
        if (side == DocumentSide.Left) { _leftRender = cts; } else { _rightRender = cts; }

        var pane = PaneFor(side);
        pane.SetRendering(true);
        try
        {
            var dpi = View.Zoom.GetRenderDpi(_renderer.Dpi);
            var page = await _renderer.RenderCurrentPageAsync(document, dpi, cts.Token);
            if (!cts.IsCancellationRequested)
            {
                await pane.ShowPageAsync(page);
            }
        }
        catch (OperationCanceledException)
        {
            //A newer page request won; nothing to show for this one
        }
        catch (Exception e)
        {
            await ShowError(e, $"Could not render page {document.CurrentPage} of “{document.FileName}”.");
        }
        finally
        {
            if (!cts.IsCancellationRequested) { pane.SetRendering(false); }
            previous?.Dispose();
        }
    }
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
        //The right document moves on every step; the left only on a "both" step
        var renders = renderLeft
            ? Task.WhenAll(RenderSideAsync(DocumentSide.Left), RenderSideAsync(DocumentSide.Right))
            : RenderSideAsync(DocumentSide.Right);
        await renders;
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Clear the busy flag in `finally` only when this render was not superseded. A
  cancelled render must not turn off a busy indicator that the newer render turned
  on.
- Check the token a second time before showing the result: a service can return a
  cached answer without ever observing cancellation.
- `previous?.Dispose()` disposes the older source, not this one, so the current
  source stays usable. The trade is that the last source per region is never
  disposed; a view model that lived and died repeatedly would want an
  `IDisposable` implementation that cancels and disposes both.
- Running two regions concurrently is safe only because the service locks its
  cache and does its heavy work inside `Task.Run`.

### Ignore a stale async result when the selection moved on

**When you want this.** A selection change starts a fetch, and the user can change
the selection again before it returns.

**The MVVM shape.** Capture the item the request was for; on completion, compare
it against the current selection inside the marshalled callback and drop the
result if it no longer matches. This is the comparison-based counterpart to
cancellation, and it needs no token.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
private async Task LoadPreviewForNodeAsync(NotionPageNodeViewModel node)
{
    try
    {
        var preview = await _documentSvc.LoadPreviewAsync(node.Id);
        InvokeOnMainThread(() =>
        {
            if (SelectedNode != node) { return; } //A newer selection superseded this preview

            PreviewTitle = preview.Title;
            PreviewMeta = string.Join("  ·  ",
                preview.ChildPageCount == 1 ? "1 child page" : $"{preview.ChildPageCount} child pages",
                $"edited {preview.LastEditedTime.ToLocalTime():yyyy-MM-dd}");
            PreviewSnippets = string.Join("\n\n", preview.TextSnippets);

            PreviewCoverSource = null;
            var imageUrl = preview.CoverUrl.Length > 0 ? preview.CoverUrl : preview.IconUrl;
            if (imageUrl.Length > 0)
            {
                try { PreviewCoverSource = new BitmapImage(new Uri(imageUrl)); }
                catch (Exception) { } //A malformed URL just leaves the pane imageless
            }
        });
    }
    catch (Exception e)
    {
        InvokeOnMainThread(() => StatusText = $"Preview failed: {e.Message}");
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- `SelectedNode` is set synchronously before the async work starts, which is what
  makes the comparison meaningful.
- A malformed image URL is swallowed on purpose so the pane still shows its text.

### Debounce a search box before rebuilding a filtered list

**When you want this.** A search field bound with
`UpdateSourceTrigger=PropertyChanged` where rebuilding on every keystroke would
make typing feel heavy.

**The MVVM shape.** The property setter starts a cancellable delay; the next
keystroke cancels the previous one. All of it lives on the view model, and the
page's `TextBox` stays a plain two-way binding.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>The search text; matching cells re-populate shortly after each keystroke.</summary>
public string SearchText
{
    get;
    set
    {
        var newValue = value ?? string.Empty;
        if (newValue == field) { return; }

        SetProperty(ref field, newValue);
        DebounceRebuild();
    }
} = string.Empty;

//Waits a beat after the last keystroke before rebuilding, so typing stays smooth.
private async void DebounceRebuild()
{
    _searchDebounce?.Cancel();
    var debounce = new CancellationTokenSource();
    _searchDebounce = debounce;
    try
    {
        await Task.Delay(300, debounce.Token);
        RebuildCells();
    }
    catch (OperationCanceledException)
    {
        //Superseded by more typing.
    }
}
```

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml -->
<TextBox Width="300" VerticalAlignment="Center"
         PlaceholderText="Search models…"
         Text="{d:Binding SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
         CornerRadius="8" />
```

A neighboring filter uses a suppression flag instead, so repopulating its list for
a new selection does not trigger a rebuild:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
_suppressCategoryRebuild = true;
Categories = categories;
_selectedCategory = AllCategories;
NotifyPropertyChanged(nameof(SelectedCategory));
_suppressCategoryRebuild = false;
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- `async void` is correct here - it is a fire-and-forget UI reaction - and the
  cancellation is caught rather than allowed to escape.
- The setter compares before assigning, so re-setting the same text does not
  restart the timer.
- A discrete choice such as a sort selector rebuilds immediately; only free text
  needs debouncing.
- The suppression flag is needed because assigning the list and resetting the
  selection each raise a change notification that would otherwise rebuild twice.

### Fill a grid lazily as it scrolls

**When you want this.** A collection large enough that materializing every item,
and its thumbnail, up front would stall the window.

**The MVVM shape.** A collection type that owns the full filtered list but adds
only a batch at a time, exposing `HasMoreItems` and a `RequestMore` method. The
page watches the `ScrollViewer` and calls `RequestMore` as the bottom approaches.
Each item starts its own thumbnail fetch when it appears.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AssetCellCollection.cs
[Microsoft.UI.Xaml.Data.Bindable]
public class AssetCellCollection : ObservableCollection<AssetCellViewModel>
{
    //Enough cells to overfill the first screen even on a wide monitor.
    private const int InitialBatch = 36;

    private readonly IReadOnlyList<AssetCellViewModel> _source;

    public AssetCellCollection(IReadOnlyList<AssetCellViewModel> source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));

        RequestMore(InitialBatch);
    }

    public int TotalCount => _source.Count;
    public bool HasMoreItems => Count < _source.Count;

    public void RequestMore(int count)
    {
        var toLoad = Math.Min(count, _source.Count - Count);

        for (var i = 0; i < toLoad; i++)
        {
            var cell = _source[Count];
            Add(cell);

            //Fire-and-forget: the cell fetches its thumbnail in the background and raises
            //a property change when the image arrives.
            _ = cell.LoadThumbnailAsync();
        }
    }
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
//Lazy grid loading: as the grid scrolls within two screens of its bottom edge,
//ask the cell collection to materialize the next batch.
CatalogScroll.ViewChanged += (_, _) =>
{
    var cells = ViewModel?.Cells;
    if (cells == null || !cells.HasMoreItems) { return; }

    var remaining = CatalogScroll.ExtentHeight - CatalogScroll.VerticalOffset - CatalogScroll.ViewportHeight;
    if (remaining < CatalogScroll.ViewportHeight * 2)
    {
        cells.RequestMore(24);
    }
};

//A new cell collection means the user switched bundle, searched or
//re-filtered: jump back to the top.
if (args.PropertyName == nameof(MainViewModel.Cells))
{
    CatalogScroll.ChangeView(null, 0, null, disableAnimation: true);
}
```

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
<!-- Fixed-size cells; the per-row count follows the window width. Cells
     materialize lazily: the page asks for more as this view scrolls
     toward its bottom edge. -->
<ScrollViewer x:Name="CatalogScroll" Grid.Row="1"
              Padding="24,4,24,8"
              VerticalScrollBarVisibility="Auto">
    <StackPanel HorizontalAlignment="Center">
        <ItemsRepeater ItemsSource="{d:Binding Cells}"
                       ItemTemplate="{StaticResource AssetCellTemplate}">
            <ItemsRepeater.Layout>
                <UniformGridLayout Orientation="Horizontal"
                                   MinItemWidth="230" MinItemHeight="248"
                                   MinColumnSpacing="14" MinRowSpacing="14"
                                   ItemsStretch="None" />
            </ItemsRepeater.Layout>
        </ItemsRepeater>
    </StackPanel>
</ScrollViewer>
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AssetCellCollection.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml` and
`Views/MainPage.xaml.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/ModelCellCollection.cs`
and `PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml` (the same
batching collection behind a card grid, built through a factory delegate so the
collection does not know how a cell is constructed)

**Sharp edges.**
- Filtering swaps in a whole new collection instance rather than mutating the
  existing one, which is what makes "scroll back to the top" a single property
  change to watch.
- A threshold of two viewports means a batch is already in place before the user
  reaches the end.
- `RequestMore` is safe to call repeatedly and no-ops once everything is
  materialized.
- The collection type is marked `[Microsoft.UI.Xaml.Data.Bindable]`, as is every
  other bound type in these applications, including plain record types.

### Show and hide panes with computed Visibility properties

**When you want this.** Placeholder text before data arrives and real content
afterwards, or one region of a page showing different content depending on what is
selected, with no value converters in the XAML.

**The MVVM shape.** The view model exposes `Visibility` properties computed from
its own state - `SimpleViewModel` supplies a `GetVisibility(bool)` helper - and
the source property either lists them in `[AffectsProperties]` or notifies them
from its setter. The XAML stacks the panes in the same grid cell and binds each
one's `Visibility`.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
public Visibility PreviewContentVisibility => GetVisibility(SelectedNode is not null);
public Visibility PreviewPlaceholderVisibility => GetVisibility(SelectedNode is null);
public Visibility PreviewCoverVisibility => GetVisibility(PreviewCoverSource is not null);
public Visibility TreePlaceholderVisibility => GetVisibility(!IsConnected);
public Visibility TreeVisibility => GetVisibility(IsConnected);
```

```xml
<!-- From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml -->
<!-- Before Connect: a quiet hint instead of a blank panel -->
<StackPanel Grid.Row="1" HorizontalAlignment="Center" VerticalAlignment="Center"
            Spacing="14" MaxWidth="420" Margin="20"
            Visibility="{d:Binding TreePlaceholderVisibility}">
    <FontIcon Glyph="&#xE8F1;" FontSize="40"
              Foreground="{StaticResource AccentDimBrush}"
              HorizontalAlignment="Center" />
    <TextBlock Text="Connect to see your pages"
               FontSize="15.5" FontWeight="SemiBold" TextAlignment="Center"
               Foreground="{StaticResource TextPrimaryBrush}" />
</StackPanel>

<TreeView Grid.Row="1" Padding="10,0,10,12"
          SelectionMode="None"
          Visibility="{d:Binding TreeVisibility}"
          ItemsSource="{d:Binding RootNodes}"
          ItemTemplate="{StaticResource PageNodeTemplate}" />
```

Where there are several exclusive panes, a private mode enum and one method that
sets it keeps every notification in one place:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
private enum ViewerMode { None, Image, Model, Text, Audio }

public Visibility ImageViewerVisibility => _viewerMode == ViewerMode.Image ? Visibility.Visible : Visibility.Collapsed;
public Visibility ModelViewerVisibility => _viewerMode == ViewerMode.Model ? Visibility.Visible : Visibility.Collapsed;
public Visibility TextViewerVisibility => _viewerMode == ViewerMode.Text ? Visibility.Visible : Visibility.Collapsed;
public Visibility NoPreviewVisibility => _viewerMode == ViewerMode.None ? Visibility.Visible : Visibility.Collapsed;
public Visibility AudioViewerVisibility => _viewerMode == ViewerMode.Audio ? Visibility.Visible : Visibility.Collapsed;
public Visibility ZoomBarVisibility => ImageViewerVisibility;

private void SetViewerMode(ViewerMode mode, string hint, bool activateViewer = true)
{
    _viewerMode = mode;
    ViewerHint = hint;
    if (mode == ViewerMode.Image)
    {
        ImagePainter.ZoomFactor = 1f;
        ImagePainter.HighlightRegion = null;
        NotifyPropertyChanged(nameof(ZoomText));
    }

    NotifyPropertyChanged(nameof(ImageViewerVisibility));
    NotifyPropertyChanged(nameof(ModelViewerVisibility));
    NotifyPropertyChanged(nameof(TextViewerVisibility));
    NotifyPropertyChanged(nameof(NoPreviewVisibility));
    NotifyPropertyChanged(nameof(AudioViewerVisibility));
    NotifyPropertyChanged(nameof(ZoomBarVisibility));
    NotifyPropertyChanged(nameof(RegionListVisibility));
    NotifyPropertyChanged(nameof(AnimationBarVisibility));

    if (activateViewer)
    {
        IsViewerActive = true;
        InvalidateImageCanvas?.Invoke();
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/NotionPageNodeViewModel.cs`

**Also shown by.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/DocumentPaneViewModel.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The placeholder and the real content are siblings in the same grid cell, each
  with its own visibility, rather than one element being swapped in and out.
- Route every path that changes mode through one method, so the notifications live
  in one place instead of being scattered through eight of them.
- KenneyAssetBrowser's two top-level views are also just two grids in the same
  cell with bound visibility, so there is no navigation and no page state to
  restore.
- Unsupported items still open, into a "nothing to preview" mode with an
  explanatory caption, so nothing in the grid is a dead card.

### Load a tree lazily as the user expands it

**When you want this.** A hierarchy that is expensive to enumerate - one API call
per level - and should be fetched only where the user looks.

**The MVVM shape.** Each row is its own small `SimpleViewModel`. A synthetic
placeholder child keeps the expand chevron visible before the real children exist.
Setting `IsExpanded` triggers a one-shot load; the parent view model does the call
and marshals the result back.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/NotionPageNodeViewModel.cs
public NotionPageNodeViewModel(NotionPageNode node, MainViewModel owner)
{
    Node = node;
    _owner = owner;

    if (node?.HasChildren == true)
    {
        //A placeholder child keeps the expand chevron visible until the real
        //  children arrive on first expand
        Children.Add(new NotionPageNodeViewModel());
    }
    // ...
}

private NotionPageNodeViewModel()
{
    IsPlaceholder = true;
}

public bool IsExpanded
{
    get;
    set
    {
        SetProperty(ref field, value);
        if (value) { _ = EnsureChildrenLoadedAsync(); }
    }
}

/// <summary>Loads the real children on first expand (no-op afterwards).</summary>
internal async System.Threading.Tasks.Task EnsureChildrenLoadedAsync()
{
    if (IsPlaceholder || _loadRequested || Node?.HasChildren != true || _owner is null) { return; }
    _loadRequested = true;
    await _owner.LoadChildrenForNodeAsync(this);
}

/// <summary>Replaces the placeholder with the loaded children.</summary>
internal void SetChildren(IEnumerable<NotionPageNodeViewModel> children)
{
    Children.Clear();
    foreach (var child in children) { Children.Add(child); }
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
/// <summary>Loads a node's children from Notion (called on first expand).</summary>
internal async Task LoadChildrenForNodeAsync(NotionPageNodeViewModel node)
{
    try
    {
        var children = await _documentSvc.LoadChildrenAsync(node.Id);
        InvokeOnMainThread(() =>
            node.SetChildren(children.Select(c => new NotionPageNodeViewModel(c, this))));
    }
    catch (Exception e)
    {
        InvokeOnMainThread(() => StatusText = $"Could not load child pages: {e.Message}");
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/NotionPageNodeViewModel.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- `_loadRequested` is a separate flag from the child count, so a page that turns
  out to have no children is not re-fetched on every expand.
- A "load everything" command walks the same `EnsureChildrenLoadedAsync()` path
  recursively, so there is one loading code path rather than two.
- A failed child load writes to the status line and leaves the row usable; it
  never throws into the expand gesture.

### Confirm and inform from the view model with SimpleViewModel dialogs

**When you want this.** A command needs a yes/no answer, or has something to tell
the user, and you do not want a dialog type in your view model.

**The MVVM shape.** `SimpleViewModel` supplies awaitable `ConfirmDialog`,
`ShowInfo` and `ShowError` helpers, so the command asks and reacts inline. The
page's only contribution is handing the view model a way to reach the XAML root
(see the bridge area). Confirmation is conditional: trivial cases are not
interrupted.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
private async Task DoClear()
{
    if (!CanClear()) { return; }

    var doClear = true;
    if (_paintSession.StrokeCount > 2)
    {
        doClear = await ConfirmDialog(
            "Are you sure you want to clear your painting and start over?",
            "Confirm");
    }

    if (doClear)
    {
        _paintSession.Clear();
        StatusText = "Cleared - paint something new.";
    }
}

private async Task DoGoBack()
{
    if (!CanGoBack()) { return; }

    if (HasDrawing)
    {
        var discard = await ConfirmDialog(
            "Going back to the camera will discard your painting. Are you sure?",
            "Discard painting?");
        if (!discard) { return; }
    }

    LeavePaintMode();
    // ...
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
var outputPath = OutputFilePath.Trim();

//Confirm before clobbering an existing file (requirement: prompt via SimpleDialog)
if (File.Exists(outputPath))
{
    var replace = await ConfirmDialog(
        $"A file already exists at:\n{outputPath}\n\nDo you want to replace it?",
        "Replace existing file?");
    if (!replace)
    {
        StatusText = "Publishing cancelled - the existing file was kept.";
        return;
    }
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
private async Task DoDecrypt()
{
    if (CanDecrypt())
    {
        if (!_encryptSvc.IsBase64Text(EnteredText))
        {
            await ShowInfo("The specified text does not look like it is encrypted.");
        }
        else
        {
            try
            {
                // ... call the service, assign ProcessedText ...
            }
            catch (Exception e)
            {
                await ShowError($"Error while decrypting: {e.Message}");
            }
        }
    }
}
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`PainDiagram/Shared/ViewModels/MainViewModel.cs`
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`

**Also shown by.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
(a result dialog that caps how many warnings it lists and says how many more there
were, so a page full of unsupported content cannot produce an unreadable dialog),
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
(`ShowError(ex, message)` as the single error surface for a whole open path)

**Sharp edges.**
- `ShowError` has two shapes: `ShowError(string)` for a message that is already
  user-ready, and `ShowError(Exception, string)` for "here is what went wrong plus
  context".
- The confirmation happens before the busy flag is set, so a cancelled overwrite
  never leaves the UI busy.
- Confirm at the moment of writing, not at the moment of picking, so a path the
  user typed by hand is covered too. The heads' own pickers have their overwrite
  prompts suppressed precisely so this is the single confirmation the user sees.
- A threshold rather than a blanket prompt keeps a destructive-action confirmation
  from becoming noise: PainDiagram and WebcamPainter both skip it for two strokes
  or fewer.
- A repeated informational message can be shown only the first time, behind a
  private flag, so an action the user repeats does not nag.
- Long multi-line dialog bodies are not portable; JustBetweenUs records that on
  one mobile platform the text is truncated to a maximum number of lines.

### Prompt before discarding unsaved work

**When you want this.** An application with dirty documents and more than one way
to close one.

**The MVVM shape.** One async method returning a three-way result (save, discard,
cancel), one close method that consumes it, and one close-all loop over the first.
Every close path in the application funnels through the same method.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Dialogs.cs
private enum SaveConfirmation
{
    Save,
    Discard,
    Cancel,
}

/// <summary>
/// Closes a document, prompting first when it has unsaved changes.
/// </summary>
/// <returns>False when the user cancelled.</returns>
private async Task<bool> CloseDocumentAsync(Document document)
{
    if (document is null) { return true; }

    if (document.IsDirty)
    {
        switch (await ConfirmDiscardAsync(document))
        {
            case SaveConfirmation.Cancel:
                return false;

            case SaveConfirmation.Save:
                //A failed or cancelled save must not lose the document.
                if (!await document.Save(saveAs: false)) { return false; }
                break;
        }
    }

    PintaCore.Workspace.CloseDocument(document);
    return true;
}

private async Task<bool> CloseAllAsync()
{
    foreach (Document document in PintaCore.Workspace.OpenDocuments.ToList())
    {
        if (!await CloseDocumentAsync(document)) { return false; }
    }

    return true;
}
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Dialogs.cs`
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`

**Sharp edges.**
- The three-button dialog maps its primary, secondary and close results to save,
  discard and cancel; the dismiss case must fall into cancel, not discard.
- The close-all loop iterates a snapshot, because closing mutates the collection.
- Both the tab close button and the window close funnel here, so there is one
  place the behavior can be wrong.

### Gate an action behind a chosen folder and explain the gate with a dialog

**When you want this.** An action cannot run until the user has supplied
something, and you want them told why rather than shown a dead button.

**The MVVM shape.** The view model owns the gate, the picker command and the
explanation. The gated command still executes; it just explains itself and
returns.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
public bool HasDownloadFolder => !string.IsNullOrWhiteSpace(_downloadFolder);

/// <summary>The folder-picker button's caption: an invitation, or the chosen path.</summary>
public string DownloadFolderLabel => HasDownloadFolder ? _downloadFolder : "Choose download folder…";

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

    //Same encoding trap as the save picker: a folder called "My Models" would otherwise
    //  come back as "My%20Models" and every download would go to the wrong place.
    _downloadFolder = FileDialogHelper.ToFileSystemPath(folder.Path);
    NotifyPropertyChanged(nameof(HasDownloadFolder));
    NotifyPropertyChanged(nameof(DownloadFolderLabel));
}

private async Task DownloadAsync(ModelCellViewModel cell)
{
    if (cell == null || IsDownloading) { return; }

    if (!HasDownloadFolder)
    {
        using (var alert = CreateDialog(
            "Downloading is disabled until you choose a download folder.\n\n" +
            "Use the folder button at the top of the window to pick where models should be saved.",
            "Choose a Download Folder"))
        {
            _ = await alert.ShowAsync();
        }
        return;
    }
    // ... download, then open the Model View ...
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Helpers/FileDialogHelper.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
(the same folder gate, with the chosen folder remembered between runs)

**Sharp edges.**
- Dispose the dialog after showing it; `using` is enough.
- `FileTypeFilter.Add("*")` is required on the folder picker even though it
  filters nothing.
- The picker's returned path needs decoding before anything touches the disk; see
  the bridge area.
- The button's caption doubles as the state display: an invitation before, the
  chosen path after.

### Report a failure as status text instead of throwing

**When you want this.** A user-entered value can be invalid and you want the
application to say so rather than crash or open a dialog.

**The MVVM shape.** The operation is wrapped in try/catch inside the view model.
On success it sets both the result property and a status string; on failure it
sets only the status string, leaving the previous good state in place. A
`TextBlock` bound to the status property is the whole UI for it.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs
private void LoadMedia()
{
    try
    {
        var uri = new Uri(MediaAddress);
        PlayerSource = MediaSource.CreateFromUri(uri);
        StatusText = $"Loaded: {uri}";
    }
    catch (Exception ex)
    {
        StatusText = $"Cannot load '{MediaAddress}': {ex.Message}";
    }
}

public string StatusText
{
    get;
    private set => SetProperty(ref field, value ?? string.Empty);
} = "Ready";
```

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml -->
<TextBlock Grid.Row="2" Text="{d:Binding StatusText}" />
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
(`StatusText = $"Could not load the ... sample: {ex.Message}"`)

**Sharp edges.**
- The status property has a public getter and a private setter, so only the view
  model writes it.
- On failure the previous good state stays. Whether that is what you want is an
  application decision; if not, clear it in the catch.
- Be honest about what the status covers. MediaPlayerDemo's covers only URI
  construction and source creation, and says nothing about whether the media
  actually plays.

### Report a domain rule violation as a typed exception the view model can catch

**When you want this.** A model-level rule needs a user-facing message, and the
view model needs to tell that case apart from a real failure.

**The MVVM shape.** The library declares its own exception type and throws it
wherever the application can say something better than the underlying library
can, with the message already phrased for a human. The view model catches that
type first and shows the message; anything else falls into a generic handler with
its own context sentence.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/DuplicateDocumentException.cs
public sealed class DuplicateDocumentException : InvalidOperationException
{
    public DuplicateDocumentException(string filePath, DocumentSide alreadyOpenSide)
        : base($"“{Path.GetFileName(filePath)}” is already selected as " +
               $"{DescribeSide(alreadyOpenSide)}; choose a different PDF for " +
               $"{DescribeSide(alreadyOpenSide == DocumentSide.Left ? DocumentSide.Right : DocumentSide.Left)}.")
    {
        FilePath = filePath;
        AlreadyOpenSide = alreadyOpenSide;
    }
    // ...
}
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
        catch (DuplicateDocumentException e)
        {
            //The same file cannot be compared with itself; the pane keeps what it had
            await ShowError(e.Message);
        }
        catch (Exception e)
        {
            await ShowError(e, "Could not open the PDF document.");
        }
```

One exception type can serve a whole service layer:

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/VideoToolProcessingException.cs
/// <summary>
/// Thrown when a file cannot be probed, a conversion cannot be planned, or a conversion fails in a
/// way this application can explain in a sentence.
/// </summary>
public class VideoToolProcessingException : Exception
{
    public VideoToolProcessingException(string message) : base(message) { }

    public VideoToolProcessingException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs
catch (OperationCanceledException)
{
    DeletePartialOutput(plan.OutputPath);
    return ConversionOutcome.Cancelled(stopwatch.Elapsed, notes);
}
catch (VideoToolProcessingException exception)
{
    DeletePartialOutput(plan.OutputPath);
    return ConversionOutcome.Failed(exception.Message, stopwatch.Elapsed, notes);
}
catch (Exception exception)
{
    DeletePartialOutput(plan.OutputPath);
    return ConversionOutcome.Failed(exception.Message, stopwatch.Elapsed, notes);
}
finally
{
    DeleteFolder(workingFolder);
}
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/DuplicateDocumentException.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/VideoToolProcessingException.cs`

**Sharp edges.**
- Carry the facts as properties, not only as a message, so a different UI could
  phrase the same failure differently.
- Throw before the side effect, so the failed operation leaves the previous state
  untouched - the pane keeps whatever it had.
- `OperationCanceledException` is always caught before the general handlers, so a
  cancel is never reported as a failure.
- A service can also refuse to let any exception out at all, turning each case
  into an outcome value so its caller has a single exit path.
- Every message names the thing that failed and says what to do about it, not
  only what went wrong.

### Compose a page from a parent view model and child view models

**When you want this.** A window has two or more regions that each own real state,
and you want them separate without giving up one data context.

**The MVVM shape.** The parent exposes each child as a get-only property, creates
them in its constructor, and owns the one thing they share. The children hold
bindable state and the commands that belong to them; the parent passes a
command's body in as a delegate and pushes state through `internal` methods.
Children talk upward through an event rather than a back-reference. XAML binds
through the parent with dotted paths, or scopes a region with its own
`DataContext`.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs
public MainViewModel()
{
    if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

    probe = GetService<IMediaProbe>() ?? new MediaProbe();

    Playback = new PlaybackViewModel();
    Conversion = new ConversionViewModel();
    Conversion.ConversionFinished += OnConversionFinished;
}

/// <summary>The player half: what is open, the transport, the chapters and the captions.</summary>
public PlaybackViewModel Playback { get; }

/// <summary>The conversion half: the destination, the size, the action and the progress.</summary>
public ConversionViewModel Conversion { get; }

/// <summary>The file the player is showing and the conversion panel is set up for.</summary>
[AffectsCommands(nameof(RemoveCommand))]
public SourceMediaInfo SelectedItem
{
    get;
    set
    {
        SetProperty(ref field, value);
        Conversion.Source = value;
        Playback.Open(value);
        NotifyPropertyChanged(nameof(EmptyLibraryVisibility));
    }
}
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<Page.DataContext>
    <vm:MainViewModel />
</Page.DataContext>
<!-- ... -->
<Button Content="Play"
        Style="{StaticResource TransportButton}"
        Command="{d:Binding Playback.PlayCommand}" />
<!-- ... -->
<ComboBox HorizontalAlignment="Stretch"
          PlaceholderText="Choose a format"
          ItemsSource="{d:Binding Conversion.Destinations}"
          SelectedItem="{d:Binding Conversion.SelectedDestination, Mode=TwoWay}"
          ItemTemplate="{StaticResource LabelTemplate}" />
```

Two identical regions are the same idea with a scoped `DataContext`:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Debug.WriteLine("Main view model startup.");

        LeftPane = new DocumentPaneViewModel("Document 1", () => BrowseAsync(DocumentSide.Left));
        RightPane = new DocumentPaneViewModel("Document 2", () => BrowseAsync(DocumentSide.Right));
        _ = OpenStartupDocumentsAsync();
    }

    /// <summary>The left pane - Document 1.</summary>
    public DocumentPaneViewModel LeftPane { get; }

    /// <summary>The right pane - Document 2.</summary>
    public DocumentPaneViewModel RightPane { get; }
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/DocumentPaneViewModel.cs
    public DocumentPaneViewModel(string title, Func<Task> browse)
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Title = title;
        BrowseCommand = new SimpleCommand(browse);
    }
    // ...
    /// <summary>Shows document (or clears the pane when it is null).</summary>
    internal void ShowDocument(PdfPageDocument document)
    {
        FilePath = document?.FilePath;
        PagePixelWidth = 0;
        PagePixelHeight = 0;
        PageImage = null;
        UpdatePageLabel(document);
    }
```

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml -->
        <Grid Grid.Column="0" DataContext="{d:Binding LeftPane}" RowSpacing="6">
            <!-- ... -->
                <Button Content="{d:Binding BrowseLabel}" Command="{d:Binding BrowseCommand}" FontWeight="SemiBold"
                        Height="24" MinHeight="0" Padding="8,0" />
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs` and
`.../ViewModels/DocumentPaneViewModel.cs`

**Sharp edges.**
- The design-mode guard belongs in the child constructor too. Because the child
  returns early in design mode, members its constructor would assign stay null
  then.
- Child state-changing methods are `internal`, not `public`, so only the parent
  and the test assembly can push into them; bindings only read.
- The children in CodeBrixVideoTool live in different assemblies from the parent
  and from each other, which is what makes them testable in isolation.
- A child talks upward by raising an event the parent subscribes to, never by
  holding a reference to the parent.
- Get-only child properties that are never reassigned keep the XAML's scoped
  `DataContext` bindings valid for the life of the page.

### Notify a value typed bindable property by hand

**When you want this.** A bindable property is a `double`, an `enum` or another
value type and `SetProperty` will not take it.

**The MVVM shape.** Compare, assign, notify - in the setter, with a comment saying
why. Everything else about the property stays the same.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs
public QualityLevel SelectedQuality
{
    get;
    set
    {
        //SetProperty takes reference types only; compare-and-notify by hand, as ProgressPercent does.
        if (field == value) { return; }
        field = value;
        NotifyPropertyChanged(nameof(SelectedQuality));
    }
} = QualityLevel.Good;

public double ProgressPercent
{
    get;
    private set
    {
        //No SetProperty overload takes a double; compare-and-notify by hand.
        if (field.Equals(value)) { return; }
        field = value;
        NotifyPropertyChanged(nameof(ProgressPercent));
    }
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs`

**Sharp edges.**
- `bool` properties in the same file do use `SetProperty(ref field, value)`, so
  the restriction is not simply "value types". Check for an overload before
  assuming.
- An enum-valued property has a dedicated helper, `SetEnumProperty()`; see the
  picker blueprint.
- The `field` keyword is used throughout, with the property's initializer after
  the closing brace.

### Bind a picker to enum values with or without friendly labels

**When you want this.** A pick-one-of-several control whose choices are the
members of an enum.

**The MVVM shape.** When the member names are already the text you want, expose a
read-only list of the offered values and a two-way selected-value property set
through `SetEnumProperty()`; the page binds `ItemsSource` and `SelectedItem` with
no template, label list or converter. When you need friendlier text, derive a
small class from `SimpleEnumInfo<TEnum>` that ties each member to a description.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs
//The stretch modes offered by the ComboBox. The Stretch enum's member names ("Uniform",
//  "UniformToFill", "Fill", "None") are exactly the text we want shown, so the ComboBox can
//  bind straight to the enum values with no separate label list.
public IReadOnlyList<Stretch> StretchOptions { get; } =
[
    Stretch.Uniform,
    Stretch.UniformToFill,
    Stretch.Fill,
    Stretch.None
];

//The player's stretch mode, two-way bound to the ComboBox's SelectedItem.
public Stretch SelectedStretch
{
    get;
    set => SetEnumProperty(ref field, value);
} = Stretch.Uniform;
```

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml -->
<ComboBox Grid.Column="2" Margin="8,0,0,0" Height="40"
          VerticalAlignment="Center"
          ItemsSource="{d:Binding StretchOptions}"
          SelectedItem="{d:Binding SelectedStretch, Mode=TwoWay}" />
```

**Variant: labeled members with SimpleEnumInfo.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/EncryptionMode.cs
public class EncryptionMode : SimpleEnumInfo<EncryptionMode.CryptAlgorithm>
{
    public enum CryptAlgorithm
    {
        [SimpleEnum<EncryptionMode>(nameof(EncryptionMode.Aes))]
        Aes = 0,

        [SimpleEnum<EncryptionMode>(nameof(EncryptionMode.TripleDes))]
        TripleDes,

        [SimpleEnum<EncryptionMode>(nameof(EncryptionMode.Twofish))]
        Twofish,
    }

    public static EncryptionMode Aes => new(CryptAlgorithm.Aes,
        "AES Standard Encryption (Secure)");

    public static EncryptionMode TripleDes => new(CryptAlgorithm.TripleDes,
        "Triple DES (Obsolete, insecure)");

    public static EncryptionMode Twofish => new(CryptAlgorithm.Twofish,
        "Twofish Encryption (Very secure)");

    public EncryptionMode(CryptAlgorithm algorithm, string description)
        : base(algorithm) =>
        Description = description?.Trim();

    public static Dictionary<CryptAlgorithm, EncryptionMode> GetDictionary() =>
        GetDictionary<EncryptionMode>();
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
private readonly Dictionary<EncryptionMode.CryptAlgorithm, EncryptionMode> _encryptionModeDictionary =
    EncryptionMode.GetDictionary();

public List<string> EncryptionModes { get; } = new();

public string SelectedEncryptionModeText
{
    get => _selectedEncryptionModeText;
    set
    {
        SetProperty(ref _selectedEncryptionModeText, value);
        _selectedEncryptionMode = _encryptionModeDictionary
            .Single(s => s.Value.Description == value)
            .Key;
    }
}
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`
`JustBetweenUs/Shared/ViewModels/EncryptionMode.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml`

**Also shown by.**
`JustBetweenUs/Mobile/Views/MainPage.xaml` (a MAUI `Picker` binds identically,
with no view-model change),
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs` (a typed option list
projected to display names, so no application type leaks into XAML)

**Sharp edges.**
- Enum-valued bound properties use `SetEnumProperty()`, not `SetProperty()`.
- A curated list written out by hand keeps unwanted members out of the picker and
  fixes the display order; `Enum.GetValues()` gives you neither.
- Binding the description string rather than the object means the setter has to
  map text back to the enum with a `Single()` lookup, which throws if two members
  ever share a description. Binding the object and using `DisplayMemberPath`
  avoids that.

### Stop a two way bound selection from commanding the control back

**When you want this.** A drop-down both drives a control and follows it, so
setting the selection from the control's own event must not turn around and
command the control.

**The MVVM shape.** One suppression field on the view model. The selection setter
acts on the surface only when the flag is false; every place the view model sets
the selection itself - following a change, refreshing the list, clearing on close -
sets the flag inside a `try`/`finally`.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/ViewModels/PlaybackViewModel.cs
public ChapterEntry SelectedChapter
{
    get;
    set
    {
        SetProperty(ref field, value);
        if (!suppressSelectionChanges && value is not null)
        {
            surface?.SeekToChapter(value.Index);
        }
    }
}

private void OnChapterChanged(object sender, EventArgs e)
{
    var index = surface?.CurrentChapterIndex ?? -1;
    if (index < 0 || index >= Chapters.Count)
    {
        return;
    }

    //The drop-down follows playback; setting it here must not seek back to where it already is.
    suppressSelectionChanges = true;
    try
    {
        SelectedChapter = Chapters[index];
    }
    finally
    {
        suppressSelectionChanges = false;
    }
}
```

The same problem in a tabbed shell is solved by comparing before pushing, so the
model event and the control event cannot ping-pong:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
private void DocumentTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (DocumentTabs.SelectedItem is not TabViewItem tab) { return; }
    Document document = documentTabs.FirstOrDefault(kv => kv.Value == tab).Key;
    if (document is null) { return; }
    int index = PintaCore.Workspace.OpenDocuments.IndexOf(document);
    if (index >= 0 && index != PintaCore.Workspace.ActiveDocumentIndex)
    {
        PintaCore.Workspace.SetActiveDocument(index);
    }
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/ViewModels/PlaybackViewModel.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
(a `_suppressCategoryRebuild` flag around repopulating a filter list),
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs` (a guard flag around
programmatic history-list selection changes)

**Sharp edges.**
- Every write path needs the flag, including teardown: clearing a collection and
  then nulling the selection would otherwise command the control on the way down.
- `try`/`finally` around each block, so an exception cannot leave the flag set.

### Alert and revert when the user picks an unsupported option

**When you want this.** A picker offers something the running platform cannot do,
and you want the user to learn why it is unavailable rather than silently not see
the option.

**The MVVM shape.** The dropdown lists every choice. The bound setter is
optimistic: it shows the new selection at once and raises the change, then an
async method validates. On failure it shows a dialog and writes the previous value
back through the backing field plus a manual notification, which snaps the control
back.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
public string SelectedRenderEngineName
{
    get => _selectedRenderEngineName;
    set
    {
        if (string.IsNullOrEmpty(value) || value == _selectedRenderEngineName) { return; }

        //Optimistic: show the new selection at once; SwitchEngineAsync reverts it if the
        //engine is unsupported or fails to initialize.
        _selectedRenderEngineName = value;
        NotifyPropertyChanged(nameof(SelectedRenderEngineName));
        _ = SwitchEngineAsync(value);
    }
}

private async Task SwitchEngineAsync(string engineName)
{
    if (!Enum.TryParse<RenderEngineKind>(engineName, out var kind) || kind == _currentEngineKind) { return; }

    if (IsBusy)
    {
        //The dropdown is disabled while busy; this is just a belt-and-braces revert.
        RevertEngineSelection();
        return;
    }

    if (!_engineSelector.IsSupported(kind))
    {
        //The unsupported engine differs by platform: Vulkan is excluded on macOS, Metal is
        //excluded everywhere except macOS - so name whichever one was picked.
        using (var alert = CreateDialog(
            $"{kind} rendering is not available on this platform.", $"{kind} Rendering"))
        {
            _ = await alert.ShowAsync();
        }
        RevertEngineSelection();
        return;
    }
    // ...
}

private void RevertEngineSelection()
{
    _selectedRenderEngineName = _currentEngineKind.ToString();
    NotifyPropertyChanged(nameof(SelectedRenderEngineName));
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The revert writes the backing field directly and raises the notification by
  hand; going through the public setter would re-enter the validation.
- The revert target is the currently active choice, not a hard-coded default, so a
  second failed switch returns to whatever is really running.
- The control is also disabled while busy, and the method still re-checks it.

### Offer only the choices that make sense for the current selection

**When you want this.** Two drop-downs whose contents depend on what is selected,
rebuilt whenever the selection changes.

**The MVVM shape.** One private refresh method, called from the source property's
setter. It clears and refills both collections from static rules that live in
plain classes, selects the first row of each, and notifies the derived text
properties. The rules are static methods, so tests can prove them without a view
model.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs
private void RefreshForSource()
{
    Destinations.Clear();
    Resolutions.Clear();

    if (Source is null)
    {
        SelectedDestination = null;
        SelectedResolution = null;
        NotifyPropertyChanged(nameof(PanelVisibility));
        NotifyPropertyChanged(nameof(RouteText));
        return;
    }

    foreach (var destination in MediaFormats.DestinationsFor(Source.Format))
    {
        Destinations.Add(new DestinationOption(destination));
    }

    foreach (var rung in ResolutionLadder.Build(Source.Width, Source.Height))
    {
        Resolutions.Add(rung);
    }

    SelectedDestination = Destinations.Count > 0 ? Destinations[0] : null;
    SelectedResolution = Resolutions.Count > 0 ? Resolutions[0] : null;

    NotifyPropertyChanged(nameof(PanelVisibility));
    NotifyPropertyChanged(nameof(RouteText));
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs
public static IReadOnlyList<MediaFormatKind> DestinationsFor(MediaFormatKind source)
{
    if (source == MediaFormatKind.Unknown)
    {
        return [];
    }

    var destinations = new List<MediaFormatKind>();
    foreach (var candidate in SupportedFormats)
    {
        if (candidate != source)
        {
            destinations.Add(candidate);
        }
    }

    if (IsSupportedFormat(source))
    {
        destinations.Add(MediaFormatKind.Mp4);
    }

    return destinations;
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/DestinationOption.cs`

**Sharp edges.**
- Each drop-down row type carries a `Label` and overrides `ToString()` to return
  it, so a `ComboBox` shows something sensible with or without an item template.
- The action button's caption is derived rather than stored: it asks the rules
  what the operation is called and falls back to a neutral word when the pair is
  one the application does not offer.

### Settle an operation in a plan before running any of it

**When you want this.** You want the "can this be done, and what exactly will
happen" question answered in one testable place, separately from the doing.

**The MVVM shape.** A static `Create()` that validates and returns an immutable
plan carrying every derived answer, plus a human-readable list of steps. The view
model catches one exception type from it and puts the message in the status bar;
the runner reads the plan and branches on nothing else.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Planning/ConversionPlanner.cs
public static ConversionPlan Create(
    SourceMediaInfo source,
    MediaFormatKind destination,
    string outputPath,
    ResolutionOption resolution,
    QualityLevel quality = QualityLevel.Good)
{
    ArgumentNullException.ThrowIfNull(source);

    if (string.IsNullOrWhiteSpace(outputPath))
    {
        throw new VideoToolProcessingException("A conversion needs somewhere to put its result.");
    }

    if (source.Format == destination)
    {
        throw new VideoToolProcessingException(
            $"'{source.FileName}' is already {MediaFormats.DisplayName(destination)}, so there is nothing to convert.");
    }

    ConversionOperationKind operation;
    try
    {
        operation = MediaFormats.OperationFor(source.Format, destination);
    }
    catch (ArgumentException exception)
    {
        throw new VideoToolProcessingException(exception.Message, exception);
    }

    if (PathsMatch(source.Path, outputPath))
    {
        throw new VideoToolProcessingException("A conversion cannot write over the file it is reading.");
    }

    var chosen = resolution ?? ResolutionOption.Original(
        ResolutionLadder.MakeEven(source.Width), ResolutionLadder.MakeEven(source.Height));

    return new ConversionPlan(source, destination, outputPath, chosen, quality, operation,
        DescribeSteps(source, destination, operation, chosen, quality));
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Planning/ConversionPlan.cs
public TargetAudioCodec AudioCodec => MediaFormats.AudioCodecFor(Destination);

public int AudioChannels => MediaFormats.AudioChannelsFor(Destination, Source.AudioChannels);

public bool DownmixesAudio => Source.HasAudio && AudioChannels < Source.AudioChannels;

public TargetVideoCodec VideoCodec => MediaFormats.VideoCodecFor(Destination);

/// <summary>
/// True when the source is a Mode 2 file, which FFmpeg cannot open and which therefore has to be
/// demultiplexed and re-wrapped before anything else can happen.
/// </summary>
public bool RequiresMode2Extraction => Source.Format == MediaFormatKind.CodeBrixMode2;

public bool IsResized => Resolution is { IsOriginal: false };
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Planning/ConversionPlanner.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Planning/ConversionPlan.cs`

**Sharp edges.**
- Everything the runner branches on is a property of the plan, so the runner reads
  as a straight line and the branching is testable without doing any work.
- The step descriptions the plan carries are the same sentences the status line
  and the run notes show, so the explanation and the behavior come from one place.
- Policy limits belong to the destination rather than to the underlying codec, so
  that adding a second destination using the same codec does not inherit the first
  one's limit by accident.

### Report the host operating system from the view model

**When you want this.** A diagnostics or About screen that proves which operating
system and runtime the user is on.

**The MVVM shape.** `SimpleOsInfo.GatherInfo()` is awaited once, cached in a
field, and formatted into a string the view model shows through its own dialog
helper. No head-specific code at all.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
public SimpleCommand ShowOsInfoCommand =>
    (field ??= new SimpleCommand(DoShowOsInfo));

private async Task DoShowOsInfo()
{
    _osInfo ??= await SimpleOsInfo.GatherInfo(withConsoleOutput: false);
    var sb = new StringBuilder();
    sb.AppendLine($"Currently running on: {_osInfo.PlatformOsName}");
    sb.AppendLine($"Operating system description: {_osInfo.OsDescription}");
    sb.AppendLine($"Operating system version: {_osInfo.OsVersion}");
    sb.AppendLine($"Product name: {_osInfo.ProductName}");
    sb.AppendLine($"Product name (for display): {_osInfo.ProductNameDisplay}");

    sb.AppendLine($"Running as user: {_osInfo.RunningAsUser}{((_osInfo.IsAdminUser is true) ? " (local admin)" : "")}");
    sb.AppendLine($"DotNet version: {_osInfo.DotNetVersion}");
    sb.AppendLine($"Platform architecture: {_osInfo.PlatformArchitecture}");

    await ShowInfo(sb.ToString());
}
```

**Where to look.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`

**Sharp edges.**
- `GatherInfo` takes a `withConsoleOutput` flag; pass false unless you want the
  same report on the console.
- This command uses the `field` keyword for its lazy backing store because it has
  nothing to dispose; the commands beside it use explicit fields so `Dispose()`
  can reach them.
- To learn which head is running rather than which operating system, see the
  head-detection blueprint in the startup area.

### Cache rendered results with a bounded most recently used cache

**When you want this.** Stepping back and forth between neighboring items should
not re-render anything, but you do not want an unbounded pile of decoded bitmaps
either.

**The MVVM shape.** The cache is a private detail of the service, not of the view
model. It is keyed by everything that affects the output, guarded by a lock
because work runs on worker threads, and exposes only a count and a `ClearCache()`
for tests and for the resolution setter.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs
    private readonly Dictionary<string, RenderedPage> _cache = new();
    private readonly LinkedList<string> _cacheOrder = new(); //Most recently used at the front
    private readonly Lock _cacheLock = new();
    // ...
    private static string CacheKey(PdfPageDocument document, int pageNumber, int dpi) =>
        $"{document.FilePath}|{pageNumber}|{dpi}";

    private bool TryGetCached(string key, out RenderedPage rendered)
    {
        lock (_cacheLock)
        {
            if (!_cache.TryGetValue(key, out rendered)) { return false; }
            _cacheOrder.Remove(key);
            _cacheOrder.AddFirst(key);
            return true;
        }
    }

    private void AddToCache(string key, RenderedPage rendered)
    {
        if (CacheCapacity < 1) { return; }
        lock (_cacheLock)
        {
            if (_cache.ContainsKey(key)) { _cacheOrder.Remove(key); }
            _cache[key] = rendered;
            _cacheOrder.AddFirst(key);
            while (_cache.Count > CacheCapacity)
            {
                var oldest = _cacheOrder.Last.Value;
                _cacheOrder.RemoveLast();
                _cache.Remove(oldest);
            }
        }
    }
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs
    public int Dpi
    {
        get;
        set
        {
            var dpi = value < 1 ? DefaultDpi : value;
            if (field == dpi) { return; }
            field = dpi;
            ClearCache();
        }
    } = DefaultDpi;
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/PageRendererTests.cs`

**Sharp edges.**
- The resolution is part of the key and changing the default also clears the
  cache. Both are needed: the key stops a low-resolution result being served for a
  high-resolution request, the clear stops stale entries accumulating.
- A capacity below one disables caching entirely rather than throwing; the
  constructor clamps.
- `System.Threading.Lock` is used rather than locking on an arbitrary object.
- A cache hit returns the same instance, so a returned record must never be
  mutated.

### Signal a non property model change to the view with a version counter

**When you want this.** The thing that changed is an object graph, and you do not
want the page subscribing to a dozen properties.

**The MVVM shape.** The view model exposes one `int` that it increments whenever
anything about the view moved. The page watches that single property name and
re-applies everything.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    /// <summary>The shared zoom level and the two pan positions; the page lays the images out from it.</summary>
    public ComparisonView View => _comparison.View;

    /// <summary>
    /// Bumped whenever the zoom, a pan position, or a page changes, so the page can re-apply
    /// the view to its image controls (one property to watch instead of many).
    /// </summary>
    public int ViewVersion
    {
        get;
        private set => SetProperty(ref field, value);
    }
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs
                viewModel.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.ViewVersion)) { ApplyViews(); }
                };
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Every call site that changes the model goes through one method, which bumps the
  counter, re-notifies the derived labels and refreshes the commands.
- `nameof(MainViewModel.ViewVersion)` keeps the page's filter refactor-safe.
- A counter, not a `bool` or an event: any increment is a change, and it survives
  being read late.

### Do blocking work in a service behind Task Run

**When you want this.** Startup or a command has to read a directory, parse a
file, or decode an image, and the window must stay responsive with a visible
loading state.

**The MVVM shape.** A registered service exposes only `Task`-returning methods and
does the blocking work inside `Task.Run`. The view model awaits them, owns the
loading flag and the visibility that follows it, and disposes whatever it opened.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/Services/AssetCatalogService.cs
public class AssetCatalogService
{
    public Task<AssetFolderCatalog> LoadCatalogAsync(string folderPath) =>
        Task.Run(() => AssetFolderCatalog.LoadFrom(folderPath));

    public Task<BundleArchive> OpenArchiveAsync(AssetBundle bundle) =>
        Task.Run(() => new BundleArchive(bundle.ZipPath));

    public Task<byte[]> ReadEntryBytesAsync(AssetBundle bundle, string entryPath) =>
        Task.Run(() =>
        {
            using var archive = new BundleArchive(bundle.ZipPath);
            return archive.ReadEntryBytes(entryPath);
        });
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
private async Task ReloadCatalogAsync()
{
    IsCatalogLoading = true;
    CloseViewer();
    DisposeArchive();
    _selectedBundle = null;
    BundleCells.Clear();
    Cells = new AssetCellCollection([]);
    ResultCountText = string.Empty;

    _catalog = await _catalogService.LoadCatalogAsync(_assetsFolder);
    // ... build the sidebar cards ...
    IsCatalogLoading = false;

    //Restore the bundle the user browsed last time, or start with the first one
    if (BundleCells.Count > 0)
    {
        var lastBundleFile = SettingsService.Get<string>(LastBundleKey);
        var restored = BundleCells.FirstOrDefault(c =>
            c.Bundle.FileName.Equals(lastBundleFile ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        await SelectBundleAsync(restored ?? BundleCells[0]);
    }
}
```

**Variant: await the network, then build off the UI thread.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
private async Task SelectAsync(SampleAssetKind kind)
{
    if (IsBusy) { return; }

    _selectedKind = kind;
    RaiseSelectionChanged();
    IsBusy = true;

    try
    {
        var progress = new Progress<string>(message => StatusText = message);
        var asset = await _assets.EnsureSampleAsync(kind, progress, CancellationToken.None);

        //Decode/build off the UI thread; the painters upload to GL lazily during Paint.
        var painter = await Task.Run(() => BuildPainter(kind, asset));
        _currentPainter = painter;
        StatusText = $"{Label(kind)}: {asset.Name}    ·    {Hint(kind)}";
    }
    catch (Exception ex)
    {
        StatusText = $"Could not load the {kind.ToString().ToLowerInvariant()} sample: {ex.Message}";
    }
    finally
    {
        IsBusy = false;
        InvalidateCanvas?.Invoke();
    }
}
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/Services/AssetCatalogService.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- One-off reads open and dispose their own handle rather than sharing a long-lived
  one, so a background fetch cannot outlive the selection that started it.
- Swap a long-lived handle under a helper that nulls the field before disposing,
  so a read racing the swap sees null rather than a disposed object.
- Guard re-entry with the busy flag at the top of the method, and always clear the
  flag - and invalidate whatever needs repainting - in the `finally`.
- Keep GPU work off the worker thread. The renderers here take a lock, stash the
  new data as pending, and upload it on the next render, on the render thread.
- Dispose the previous result only after the new one is built and assigned, so a
  failed build leaves the previous view intact.

### Load an asset off the UI thread and resolve its side files from the same container

**When you want this.** Opening a document or model means a parse that must not
block the window, and the file references sibling files that live in the same
archive rather than on disk.

**The MVVM shape.** The parse runs in `Task.Run` behind a loader interface; the
awaited result is assigned and published with a change notification, which the
bound control picks up. External references are resolved by a closure over the
open container, so the loader stays ignorant of where the bytes come from.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
//Parse the GLB off the UI thread; the GPU upload happens lazily at first paint.
//Kenney GLBs reference their colormap texture beside themselves rather than embedding
//it, so external references resolve back into the bundle archive.
var archive = _archive;
var animated = await Task.Run(() =>
{
    using var stream = new MemoryStream(bytes, writable: false);
    return new GltfModelLoader().LoadAnimated(stream,
        name => archive?.ReadDependencyBytes(variant.EntryPath, name));
});
var loaded = animated.Model;

_animatedModel = animated;
_currentModel = loaded;
NotifyPropertyChanged(nameof(CurrentModel));
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Models/GltfModelLoader.cs
private static ModelRoot ReadRoot(Stream stream, Func<string, byte[]?>? resolveDependency)
{
    try
    {
        if (resolveDependency == null)
        {
            return ModelRoot.ReadGLB(stream);
        }

        var context = ReadContext.Create(assetName =>
        {
            var bytes = resolveDependency(Uri.UnescapeDataString(assetName));
            return bytes == null
                ? throw new FileNotFoundException($"The model references '{assetName}', which was not found.")
                : new ArraySegment<byte>(bytes);
        });
        return context.ReadBinarySchema2(stream);
    }
    catch (Exception ex) when (ex is not InvalidDataException)
    {
        throw new InvalidDataException("The stream does not contain a loadable glTF binary (.glb) model.", ex);
    }
}
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Models/GltfModelLoader.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Models/IModelLoader.cs`

**Sharp edges.**
- A "self-contained" binary file may still reference a sibling. Passing a resolver
  that reads back into the same container is what makes such files load; passing
  `null` refuses all external references, which is the safe default for untrusted
  input.
- Referenced names arrive URI-escaped; unescape before looking them up.
- Capture the container field into a local before the `Task.Run`, so a selection
  change during the parse cannot null it out mid-flight.
- The loader interface exists so the loading technology can be swapped or mocked
  without touching the renderer, which takes the loaded model type and never a
  format-specific one.

### Pre warm a rendering backend off the UI thread

**When you want this.** You are about to hand a new GPU backend to a paint
callback, and a supported platform can still have a missing or broken driver. You
want a status message, not an exception inside the paint handler.

**The MVVM shape.** The view model creates the backend, renders one throwaway tiny
frame on a worker thread, and only then swaps painters. A failure is caught,
written to bound status text, and the selection reverted.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
IsBusy = true;
try
{
    var engine = _engineSelector.Create(kind, GetXamlRoot);
    if (kind is RenderEngineKind.Vulkan or RenderEngineKind.Metal)
    {
        //Fail fast off the UI thread (a supported platform can still lack a working
        //driver) so a failure never surfaces inside the Skia paint callback. Safe for the
        //own-stack engines (Vulkan, Metal): they have no thread-affinity, unlike the
        //OpenGL engine's native GL context, which must be created on the render thread at
        //first paint.
        await Task.Run(() => engine.RenderFrame(1, 1, (0f, 0f, 0f, 1f)));
    }

    var oldPainter = _modelPainter;
    _modelPainter = new ModelScenePainter(engine);
    _currentEngineKind = kind;
    if (ReferenceEquals(_currentPainter, oldPainter))
    {
        _currentPainter = null;
    }
    oldPainter?.Dispose();
}
catch (Exception ex)
{
    StatusText = $"Could not switch to {engineName} rendering: {ex.Message}";
    RevertEngineSelection();
    return;
}
finally
{
    IsBusy = false;
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The pre-warm is only safe for backends with no thread affinity. The OpenGL
  engine is deliberately excluded, because its native context must be created on
  the render thread at first paint.
- The current painter is cleared before the old one is disposed, so the page's
  paint handler cannot call into a disposed painter between the two statements.
- After a successful switch the current asset is re-displayed from the local
  cache, so switching backends never touches the network.

### Coalesce repaints and drop backlogged pointer frames

**When you want this.** Each repaint is expensive, and a fast mouse can queue more
pointer events than you can draw.

**The MVVM shape.** Two independent mechanisms. Paint coalescing keeps at most one
pending invalidate. Backlog detection compares the pointer event's own timestamp
against a stopwatch and, when the input stream has fallen behind, advances the
painter's drag anchor without rendering, so the camera stays in sync with the
cursor while frames are skipped.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
//A pointer frame that is running more than this far behind real time is a backlog
//frame: keep the cursor anchor in sync but skip rendering it, catching up to the latest.
private const double StaleFrameMicroseconds = 1_000_000; // 1 second

//Coalescing: never queue more than one paint. While one is pending, pointer moves only
//update the camera; the next paint draws the latest state.
private bool _renderPending;

private void RequestRender()
{
    if (_renderPending) { return; }
    _renderPending = true;
    DisplayCanvas?.Invalidate();
}

private bool IsBacklogFrame(ulong timestamp)
{
    if (!_gestureClock.IsRunning) { return false; }
    var inputElapsed = timestamp - _gestureStartTimestamp;
    var lag = _gestureClock.Elapsed.TotalMicroseconds - inputElapsed;
    return lag > StaleFrameMicroseconds;
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IScenePainter.cs
/// <summary>
/// Advances the drag anchor to the given position without moving the camera, used to
/// discard a stale (backlogged) pointer frame while staying in sync with the cursor.
/// </summary>
void PointerSkip(double x, double y);
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IScenePainter.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/ModelScenePainter.cs`

**Sharp edges.**
- The pending flag is cleared at the top of the paint handler, so a request made
  during paint still queues the next frame.
- `PointerSkip` exists precisely so that dropping a frame does not make the scene
  jump: it moves the anchor without applying the delta to the camera.
- On pointer release the page requests one more render at full, non-drag
  resolution, which is what makes a two-tier resolution scheme work.

### Run a sensor pipeline on a worker thread with latest frame wins

**When you want this.** A sensor produces frames faster than your processing can
consume them and you must never block the producer.

**The MVVM shape.** The whole thing lives in a library class with a
`SubmitFrame()` method and an event; the view model owns the instance, subscribes,
and does nothing else. The class documents that its event is raised on the worker
thread, so consumers know they must marshal.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs
public void SubmitFrame(byte[] bgraPixels, int width, int height)
{
    if (!_running || bgraPixels == null || width < 1 || height < 1) { return; }

    int needed = width * height * 4;
    if (bgraPixels.Length < needed) { return; }

    lock (_pendingLock)
    {
        if (_pendingFrame == null || _pendingFrame.Length != needed)
        {
            _pendingFrame = new byte[needed];
        }
        Array.Copy(bgraPixels, _pendingFrame, needed);
        _pendingWidth = width;
        _pendingHeight = height;
        _hasPending = true;
    }
    _frameSignal.Set();
}

private void WorkerLoop()
{
    PalmDetector detector = null;
    HandLandmarker landmarker = null;
    try
    {
        detector = new PalmDetector(LoadEmbeddedModel(DetectorResourceName));
        landmarker = new HandLandmarker(LoadEmbeddedModel(LandmarkerResourceName));

        while (_running)
        {
            _frameSignal.WaitOne();
            if (!_running) { break; }

            int width;
            int height;
            lock (_pendingLock)
            {
                if (!_hasPending) { continue; }

                //Swap the pending buffer out under the lock; copy-free hand-off
                (_workingFrame, _pendingFrame) = (_pendingFrame, _workingFrame);
                width = _pendingWidth;
                height = _pendingHeight;
                _hasPending = false;
            }
            // ... process _workingFrame and raise TrackingUpdated ...
        }
    }
    finally
    {
        detector?.Dispose();
        landmarker?.Dispose();
        // ... dispose the cached Mats ...
    }
}
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs`

**Also shown by.**
`PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs` (the same worker
shape, extended with multi-hand tracking across frames)

**Sharp edges.**
- Submitting faster than the worker can process silently replaces the pending
  frame. That is the point: stale frames are dropped and the producer never waits.
- `SubmitFrame` copies before returning so the caller may reuse its buffer
  immediately; the worker then swaps the two buffers under the lock, so steady
  state costs one copy per processed frame and no allocations.
- Expensive resources are created inside the worker, so constructing the tracker
  is cheap and the loading cost lands on the background thread.
- The thread is named and marked background; `Stop()` clears the flag, signals the
  wait handle, and joins, which makes disposal genuinely synchronous.
- `Start()` and `Stop()` are idempotent, and there is a test for that.

### Survive a native runtime tearing down while a frame is in flight

**When you want this.** A worker thread calls into a native library that may be
unloaded at process exit, and you do not want that to become a fatal unhandled
exception.

**The MVVM shape.** Two `catch` clauses on the per-frame work: an exception filter
that recognizes shutdown and exits the loop quietly, and a general one that drops
a single bad frame and keeps going.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs
try
{
    HandTrackingResult result = ProcessFrame(detector, landmarker, _workingFrame, width, height);
    TrackingUpdated?.Invoke(this, new HandTrackingEventArgs(result));
}
catch (Exception ex) when (!_running)
{
    //Shutting down: a frame was in flight when the tracker - or the native
    //  OpenCV runtime at process exit - began tearing down (e.g. "terminated
    //  TLS container"). The app is going away; exit the loop quietly rather
    //  than surfacing this as a fatal unhandled exception on the worker thread.
    Debug.WriteLine($"HandTracker worker stopping during shutdown: {ex.Message}");
    break;
}
catch (Exception ex)
{
    //A single frame failed to process - drop it and keep tracking rather than
    //  taking down the whole application over one bad frame.
    Debug.WriteLine($"HandTracker skipped a frame: {ex.Message}");
}
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs`

**Sharp edges.**
- The `when (!_running)` filter is what separates "we are shutting down" from "a
  frame was bad". Without it the shutdown race is indistinguishable from a real
  failure.
- The running flag is `volatile` precisely so the filter sees it the moment
  `Stop()` clears it.
- The `finally` block disposes the native handles on the worker thread that
  created them.

### Publish a small immutable result type from a background pipeline

**When you want this.** A worker raises events at frame rate and you want no risk
of a consumer mutating shared state.

**The MVVM shape.** An immutable result class with an `internal` constructor, a
cached "nothing found" singleton, and an `EventArgs` wrapper. The view model reads
the result once into a local and closes over it.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/HandTrackingResult.cs
internal static HandTrackingResult NoHand { get; } =
    new HandTrackingResult(false, false, 0f, 0f, 0f, 0f);

/// <summary>Indicates whether a hand was found in the frame.</summary>
public bool HandDetected { get; }

/// <summary>
/// Indicates whether the hand is showing the open-palm ("spatula") gesture - the
/// gesture that paints.
/// </summary>
public bool IsOpenPalm { get; }

/// <summary>
/// The palm center's horizontal position, normalized 0..1 across the UNMIRRORED camera
/// frame (smoothed across recent frames).
/// </summary>
public float PalmCenterX { get; }
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Vision/HandTrackingResult.cs`
`WebcamPainter/src/libs/WebcamPainter.Webcam/CapturedPhoto.cs`

**Sharp edges.**
- The event fires on "nothing found" frames too, and the documentation says why:
  subscribers need it to end an in-progress gesture.
- `internal` constructors mean only the library can create results; consumers can
  only read them.
- The XML documentation carries the coordinate contract - unmirrored, normalized,
  smoothed - which is where a consumer learns it must mirror.

### Capture a still and start a second pipeline from a command

**When you want this.** One command has to grab data, build a heavier model off
the UI thread, subscribe to it, and flip the whole UI into another mode.

**The MVVM shape.** An async command that captures, offloads construction with
`Task.Run`, wires the new object's events (marshalling the ones that touch bound
state), stores it, lazily creates the long-lived worker, and flips the mode flag
last.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
IsBusy = true;
try
{
    var photo = _captureService.CapturePhoto();

    //The preview the user was watching is mirrored, so mirror the still to match
    var session = await Task.Run(() =>
        PaintingSession.Create(photo.PixelsBgra32, photo.Width, photo.Height, mirrorHorizontally: true));

    session.Session.RedrawRequested += (_, _) => InvalidateMainCanvas?.Invoke();
    session.Session.DrawingChanged += (_, _) =>
        InvokeOnMainThread(() => HasDrawing = _paintSession?.HasStrokes ?? false);

    _paintSession = session;
    HasDrawing = false;
    ActiveColorText = $"Painting with: {session.ActiveColorName}";

    if (_tracker == null)
    {
        _tracker = new HandTracker();
        _tracker.TrackingUpdated += OnTrackingUpdated;
    }
    _tracker.Start();

    IsCaptureMode = false;
    NotifyPropertyChanged(nameof(PaintSession));
    InvalidateMainCanvas?.Invoke();
    StatusText = "Show the camera your open palm to spread paint on the photo - " +
                 "close your hand (or hide it) to stop painting.";
}
catch (Exception e)
{
    StatusText = $"Photo failed: {e.Message}";
}
finally
{
    IsBusy = false;
}
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- A plain expression-bodied property over a field needs an explicit
  `NotifyPropertyChanged` when the field is replaced; only `SetProperty`
  properties notify themselves.
- The worker is created once and reused across mode changes; only `Start()` and
  `Stop()` cycle, and its event is subscribed exactly once.
- Events that fire off the UI thread marshal in their handler; a handler that only
  calls a bridge delegate can rely on the delegate to marshal itself.
- The mode flag flips only after everything is in place, so a frame arriving
  mid-setup does not find a half-built mode.

### Run an effect on worker threads with a live preview

**When you want this.** An expensive transform must render off the UI thread, show
partial results as it goes, stay cancellable, and end up in the undo history.

**The MVVM shape.** A manager owns the preview surface and the render handle; the
renderer is a static that splits the region across threads. The UI thread only
polls for finished tiles through a timer service, and the configuration dialog is
awaited concurrently with the render.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/LivePreviewManager.cs
const uint UPDATE_MILLISECONDS = 100;

AsyncEffectRenderer.Settings settings = new (
    threadCount: system.RenderThreads,
    renderBounds: RenderBounds,
    effectIsTileable: effect.IsTileable);
// ...
renderHandle = AsyncEffectRenderer.Start (
    settings,
    effect,
    layer.Surface,
    LivePreviewSurface);

using IDisposable _ = timer.Start (
    UPDATE_MILLISECONDS,
    () => {
        if (!renderAlive) return false;
        PollForUpdate (renderHandle);
        return true; // Keep ticking as long as the effect is active.
    }
);

bool userConfirmed = !effect.IsConfigurable || await effect.LaunchConfiguration ();

chrome.MainWindowBusy = true;

if (!userConfirmed) {
    renderHandle.Cancel ();
    await renderHandle.Task;
    return;
}

dialog.Show ();

var result = await renderHandle.Task;

// Final poll after the renderer finishes to ensure the last-rendered tiles are displayed.
PollForUpdate (renderHandle);
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Effects/BaseEffect.cs
/// <summary>
/// Specifies whether Render() can be called separately (and possibly in parallel) for different sub-regions of the image.
/// If false, Render () will be called once with the entire region the effect is applied to.
/// This is required for effects which cannot be applied independently to each pixel, e.g. if the effect accumulates information from previously processed pixels.
/// </summary>
public abstract bool IsTileable { get; }
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/LivePreviewManager.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/AsyncEffectRenderer.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Effects/BaseEffect.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/CanvasRenderer.cs`

**Sharp edges.**
- The tileable flag is the correctness gate: an effect that accumulates state
  across pixels must declare itself untileable or parallel tiles produce wrong
  output.
- One final poll after the render task completes, or the last tiles never reach
  the screen.
- The renderer's own comment says its methods are to be called from a single
  thread, the UI thread, only.
- Thread count comes from a system service, which the tests replace with a mock.
- The canvas renderer substitutes the preview surface for the active layer while
  the preview is enabled, so no extra compositing path is needed.

### Drive an undo history from a list and travel to a clicked point

**When you want this.** Undo and redo, a visible history, and the ability to jump
several steps at once.

**The MVVM shape.** The document owns a history of items with a pointer; the view
binds a list to the items, dims the ones past the pointer, and travels one step at
a time so each item's own undo or redo runs. Command enablement follows the
history's own `CanUndo` and `CanRedo`.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (updatingHistorySelection || !PintaCore.Workspace.HasOpenDocuments) { return; }

    DocumentHistory history = PintaCore.Workspace.ActiveWorkspace.History;
    int target = HistoryList.SelectedIndex;

    if (target < 0 || target == history.Pointer) { return; }

    //Travel to the clicked point, one step at a time so every history item's
    //own Undo/Redo runs.
    while (history.Pointer > target && history.CanUndo) { history.Undo(); }
    while (history.Pointer < target && history.CanRedo) { history.Redo(); }
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Pads/HistoryRowFactory.cs
StackPanel row = new () {
    Orientation = Orientation.Horizontal,
    Spacing = 6,
    // Dimming is what tells a user the entry is "ahead" of where the
    // document currently is.
    Opacity = undone ? 0.45 : 1.0,
};
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Tools/Tools/PencilTool.cs
protected override void OnMouseUp (Document document, ToolMouseEventArgs e)
{
	if (undo_surface != null && surface_modified)
		document.History.PushNewItem (new SimpleHistoryItem (Icon, Name, undo_surface, document.Layers.CurrentUserLayerIndex));

	surface_modified = false;
	undo_surface = null;
	mouse_button = MouseButton.None;
}
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/DocumentHistory.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Pads/HistoryRowFactory.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Tools/Tools/PencilTool.cs`

**Sharp edges.**
- A guard flag around programmatic selection changes is mandatory, or the refresh
  that follows an undo triggers another travel.
- Travel one step at a time; moving the pointer directly would skip each item's
  own undo work.
- The undo snapshot is taken on the gesture's start and pushed only if the surface
  was actually modified.

### Bind a tab per open document and keep both directions in sync

**When you want this.** A tabbed multi-document interface where the model, not the
tab control, owns which document is active.

**The MVVM shape.** A dictionary maps documents to tab items; model events add and
remove tabs, and the tab's own selection change pushes the choice back into the
model. Comparison before pushing stops the echo.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
private void AddDocumentTab(Document document)
{
    PintaCanvasView view = new() { Document = document };
    TabViewItem tab = new()
    {
        Header = document.DisplayName,
        Content = view,
    };
    documentTabs[document] = tab;
    DocumentTabs.TabItems.Add(tab);
    DocumentTabs.SelectedItem = tab;

    document.Renamed += (_, _) => { tab.Header = document.DisplayName; RebuildWindowMenu(); };
    document.IsDirtyChanged += (_, _) =>
    {
        tab.Header = document.IsDirty ? $"{document.DisplayName}*" : document.DisplayName;
        RebuildWindowMenu();
    };

    //History changes drive Undo/Redo enablement and the history pad.
    document.History.HistoryItemAdded += (_, _) => OnDocumentStateChanged();
    document.History.ActionUndone += (_, _) => OnDocumentStateChanged();
    document.History.ActionRedone += (_, _) => OnDocumentStateChanged();
    // ...
}
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml`

**Sharp edges.**
- The index check before pushing the active document is what stops the model event
  and the tab event from ping-ponging.
- A tab close request must run the save prompt rather than closing the tab
  directly; the tab close button is the most likely way to lose a document.
- Subscriptions that are re-established on every activation are removed before
  being added, so switching tabs repeatedly does not stack handlers.

### Show selection state in button captions from computed properties

**When you want this.** The UI must show which of several modes is active, without
a converter or code-behind.

**The MVVM shape.** One private-set property holds the active name; computed
properties derive the button captions from it; the setter raises change
notifications for all of them. The XAML binds `Content` to the computed
properties.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
public string ActiveLayerName
{
    get;
    private set
    {
        SetProperty(ref field, value);
        NotifyPropertyChanged(nameof(PainButtonText));
        NotifyPropertyChanged(nameof(NumbnessButtonText));
        NotifyPropertyChanged(nameof(TinglingButtonText));
    }
} = PainLayerName;

public string PainButtonText => ActiveLayerName == PainLayerName ? "✓ Pain" : "Pain";
public string NumbnessButtonText => ActiveLayerName == NumbnessLayerName ? "✓ Numbness" : "Numbness";
public string TinglingButtonText => ActiveLayerName == TinglingLayerName ? "✓ Tingling" : "Tingling";
```

```xml
<!-- From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml -->
<StackPanel Grid.Row="1" Orientation="Horizontal" Spacing="8" Margin="0,0,0,8">
    <Button Content="{d:Binding PainButtonText}" Command="{d:Binding SelectPainCommand}" MinWidth="110"
            Background="#66FF1EE6" />
    <Button Content="{d:Binding NumbnessButtonText}" Command="{d:Binding SelectNumbnessCommand}" MinWidth="110"
            Background="#661E80CC" />
    <Button Content="{d:Binding TinglingButtonText}" Command="{d:Binding SelectTinglingCommand}" MinWidth="110"
            Background="#66CCAA0A" />
</StackPanel>
```

**Where to look.**
`PainDiagram/Shared/ViewModels/MainViewModel.cs`
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml`

**Sharp edges.**
- The property initializer sets the initial caption without running the setter
  body, so the computed captions are correct before the first notification.
- Commands with no meaningful `CanExecute` - here the three selection commands -
  are constructed from the handler alone, and a synchronous handler in an
  async-shaped signature ends with `return Task.CompletedTask;`.

