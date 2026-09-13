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
event subscriptions, lifetime tokens and the delegates a page handed over -
including which of them the view model really owns, and who calls Dispose at
all. A group of their own covers work that has to start where nothing can
await it: a guarded async void, a task discarded through a helper that
observes it, and a first load that waits until the page says it is on screen.
A further group is about what the user picks and how much of it you load:
enum-backed pickers, drop-downs whose choices depend on the current selection,
alerting and reverting when the platform cannot honor a choice, and grids,
trees and search boxes that fill lazily rather than all at once. Reach for this file
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
- [Stream an IAsyncEnumerable of pages into a bound collection](#stream-an-iasyncenumerable-of-pages-into-a-bound-collection)
- [Make a rate limit wait visible through the progress channel](#make-a-rate-limit-wait-visible-through-the-progress-channel)
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
- [Mutate a plot model under its own sync root so streamed batches need no dispatcher hop](#mutate-a-plot-model-under-its-own-sync-root-so-streamed-batches-need-no-dispatcher-hop)
- [Root a native callback delegate for the life of a streaming session](#root-a-native-callback-delegate-for-the-life-of-a-streaming-session)
- [Refresh every section from one shared snapshot and one pausable timer](#refresh-every-section-from-one-shared-snapshot-and-one-pausable-timer)
- [Let section view models ask the shell for the few things they cannot do](#let-section-view-models-ask-the-shell-for-the-few-things-they-cannot-do)
- [Cache the newest frame in the view model and let the renderer pull it](#cache-the-newest-frame-in-the-view-model-and-let-the-renderer-pull-it)
- [Push a bound toggle into a live native session and re-apply it to the next one](#push-a-bound-toggle-into-a-live-native-session-and-re-apply-it-to-the-next-one)
- [Validate a typed folder path inside CanExecute](#validate-a-typed-folder-path-inside-canexecute)
- [Guard an async void handler the platform calls](#guard-an-async-void-handler-the-platform-calls)
- [Start work you cannot await through a helper that observes it](#start-work-you-cannot-await-through-a-helper-that-observes-it)
- [Start the first load when the page says it is ready](#start-the-first-load-when-the-page-says-it-is-ready)
- [Cancel one lifetime token from Dispose so in-flight work stops](#cancel-one-lifetime-token-from-dispose-so-in-flight-work-stops)
- [Dispose only the service the view model built itself](#dispose-only-the-service-the-view-model-built-itself)
- [Dispose a view model the XAML declared from the page Unloaded](#dispose-a-view-model-the-xaml-declared-from-the-page-unloaded)
- [Fail a bound setter into the status line when a device refuses it](#fail-a-bound-setter-into-the-status-line-when-a-device-refuses-it)
- [Switch devices off the UI thread and let the newest switch report](#switch-devices-off-the-ui-thread-and-let-the-newest-switch-report)

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
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/ModelCellViewModel.cs`
(a per-cell command whose gate is a delegate into the owner, so no attribute can
see it; the owner calls the cell's `NotifyCanDownloadChanged()` instead)

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
    actions.Layers.MoveLayerDown.Sensitive = currentIndex > 0;
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
}
```

The view model keeps a funnel of its own for the state it does own, so the page's
funnel is only about the headless commands and the pads:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.Core/ViewModels/MainViewModel.cs
    private void RefreshDocumentState()
    {
        HasOpenDocuments = PintaCore.Workspace.HasOpenDocuments;
        UpdateSelectionSizeText();
    }
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Actions.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`
`Pinta.Brix/src/Pinta.Brix.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The single funnel is what keeps six different model events from each having to
  know which commands they affect.
- The no-document branch resets the state-dependent commands explicitly, so a
  stale enabled state never survives a document close.
- Prefer `SimpleCommand` with `[AffectsCommands]` when the commands can live on a
  view model; reach for this shape only when the command model is owned by a
  headless library. Anything that is really bound state - here the status bar's
  three readouts and the zoom control - belongs on the view model with its own
  funnel, which is why the page's funnel no longer touches them.

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
/// <summary>Whether a model download is in flight (drives the bottom progress bar).</summary>
[AffectsProperties(nameof(DownloadBarVisibility))]
public bool IsDownloading
{
    get;
    private set
    {
        SetProperty(ref field, value);

        //The download gate lives on each cell's own command; tell every materialized
        //cell to re-query it. (Cells materialized later evaluate the gate fresh anyway.)
        if (Cells is { } cells)
        {
            foreach (var cell in cells) { cell.NotifyCanDownloadChanged(); }
        }
    }
}
```

The attribute covers the computed visibility, so the setter body is only the part
no attribute can express: poking every materialized cell's own command.

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
    //The application's configured logger factory, which the heads set up in App.InitializeLogging().
    //  NullLogger covers the design-mode path, where the constructor returns before this is assigned.
    private ILogger _log = NullLogger.Instance;

    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        _log = LogExtensionPoint.AmbientLoggerFactory.CreateLogger<MainViewModel>();
        _log.LogInformation("Main view model startup.");

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
- Anything the constructor assigns after the guard is unassigned in the designer,
  so give such a field a harmless initial value at its declaration. The logger
  here starts as the null logger for exactly that reason, and nothing has to
  null-check it.
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
/// <summary>Whether the initial catalog fetch is still in flight.</summary>
[AffectsProperties(nameof(CatalogLoadingVisibility))]
public bool IsCatalogLoading
{
    get;
    private set => SetProperty(ref field, value);
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
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
public MainViewModel()
{
    if (!IsDesignMode(true))
    {
        Debug.WriteLine("Main view model startup.");

        _encryptSvc = GetService<IEncryptionService>();
        // ... fill the picker list and select the first entry ...
        Initialization = InitializeAsync();
    }
}

/// <summary>
/// The startup work that the constructor begins: reading the default encryption key and showing
/// the application's first informational dialog. A page or a test can await this to find out when
/// that work has finished.
/// </summary>
public Task Initialization { get; private set; } = Task.CompletedTask;

private async Task InitializeAsync()
{
    try
    {
        var defaultKey = await _encryptSvc.GetDefaultKey();
        //We can't set a value to EncryptionKey except on the main (UI) thread, because this causes problems on Linux and macOS
        InvokeOnMainThread(() => EncryptionKey = defaultKey);

        //A dialog needs a UI anchor that does not exist until the page has been laid out, so wait
        //  for the page to say that it is ready instead of guessing how long that takes.
        await _pageReady.Task;

        await ShowInfo("This application is adapted from a sample provided by Paul Ainsworth.");
    }
    catch (OperationCanceledException)
    {
        //The view model was disposed before the page became ready - there is nothing left to show
    }
    catch (Exception e)
    {
        //Startup work must never be able to bring the application down
        Debug.WriteLine($"Main view model startup failed: {e.Message}");
    }
}
```

A named task costs one property and buys two things: a page or a test can find
out when startup finished, and every failure has somewhere to land instead of
being an unobserved exception on a discarded task.

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
  `XamlRoot`, and a dialog raised then has nowhere to attach. JustBetweenUs waits
  on a page-readiness signal - a `TaskCompletionSource` each head's page completes
  from its loaded event - rather than guessing how long the page takes; see
  [Start the first load when the page says it is ready](#start-the-first-load-when-the-page-says-it-is-ready).
- The named task is cancelled, not abandoned, when the view model goes away: the
  readiness source is cancelled in `Dispose()`, and the startup method catches
  `OperationCanceledException` and says nothing.

### Load documents named on the command line during startup

**When you want this.** Repeating the same task, or launching from a script,
without clicking through file pickers first.

**The MVVM shape.** A fire-and-forget async method started once the page says it
is on screen, guarded by the busy flag, with every failure funnelled into the
standard error dialog. The head's `Main` does nothing special; the view model
reads the process arguments itself.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    /// <summary>
    /// Convenience for repeated comparisons: launching a head as
    /// <c>PdfSideBySide.LinuxX11 left.pdf right.pdf</c> pre-loads the two documents, so the
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

It is started from the page-ready signal rather than from the constructor,
because its failure path raises a dialog:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    public void OnPageReady()
    {
        if (_isPageReady) { return; }
        _isPageReady = true;

        //Discarded deliberately: every failure is caught and reported inside
        _ = OpenStartupDocumentsAsync();
    }
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- `Environment.GetCommandLineArgs()` includes the executable at index 0, so the
  two paths are at indices 1 and 2 and the guard is `arguments.Length < 3`. The
  `string[] args` passed to a head's `Main` is never forwarded anywhere.
- Started from the constructor this could finish before the page had handed the
  view model a `XamlRoot`, so the error dialog would have had nowhere to attach.
  The page-ready signal is what makes the dialog safe; see
  [Start the first load when the page says it is ready](#start-the-first-load-when-the-page-says-it-is-ready).
- The guard flag makes the signal idempotent, so a page that is loaded more than
  once does not re-open the documents.

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

### Stream an IAsyncEnumerable of pages into a bound collection

**When you want this.** A service reads a paged remote source that takes minutes to
walk, and the user should see each page as it lands rather than a spinner until the
end.

**The MVVM shape.** The service returns `IAsyncEnumerable<TPage>` and takes an
`IProgress<T>` and a `CancellationToken`. The view model enumerates it with
`await foreach` under `ConfigureAwait(false)`, and hands each page to
`InvokeOnMainThread` to be folded into the bound collections. The
`CancellationTokenSource` is a field so a Cancel command can reach it, and every
callback the run posts checks that it is still the current run before it touches
anything.

**Code.**

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
private async Task DoSearch()
{
    if (!CanSearch()) { return; }

    //Everything the run needs is copied out now, because the user is free to keep typing.
    var owner = (Owner ?? string.Empty).Trim();
    var assignee = (Assignee ?? string.Empty).Trim();
    var includeClosed = IncludeClosed;

    // ... write all three through the settings facade ...

    //Starting a search supersedes whatever was already running. The new token source is
    //published first, so the older run sees that it is no longer the current one and stays
    //quiet on its way out.
    var previous = _searchCts;
    var cancellation = new CancellationTokenSource();
    _searchCts = cancellation;
    previous?.Cancel();

    // ... remember what this run is searching for, and clear the last run's results ...

    IsSearching = true;
    IsCancelling = false;
    IsProgressIndeterminate = true;
    ProgressValue = 0d;
    SetStatus("Contacting GitHub...", SearchStatusKind.Working);

    //Created on the UI thread, so its callbacks already arrive there and must not be
    //marshalled a second time.
    var progress = new Progress<SearchProgress>(report =>
    {
        if (!ReferenceEquals(cancellation, _searchCts)) { return; }
        OnProgress(report);
    });

    // ... build the request from the three locals ...

    try
    {
        await foreach (var page in _searchService
            .SearchAsync(request, progress, cancellation.Token)
            .ConfigureAwait(false))
        {
            var arrived = page;
            InvokeOnMainThread(() =>
            {
                if (!ReferenceEquals(cancellation, _searchCts)) { return; }
                FoldPage(arrived);
            });
        }

        InvokeOnMainThread(() =>
        {
            if (!ReferenceEquals(cancellation, _searchCts)) { return; }
            CompleteSearch();
        });
    }
    catch (OperationCanceledException)
    {
        InvokeOnMainThread(() =>
        {
            if (!ReferenceEquals(cancellation, _searchCts)) { return; }
            SetStatus($"Cancelled after {CountPhrase()}.", SearchStatusKind.Cancelled);
        });
    }
    // ... one catch turning a typed API failure into status text, and one for anything else ...
    finally
    {
        //Everything this run still has to say is already queued on the UI thread, so the
        //tidying up is queued behind it: clearing the field here would make the last page and
        //the closing sentence look as though a newer run had superseded them. Only the run
        //that is still the current one may turn the busy indicators off, and the token source
        //is disposed on the UI thread, after the field is cleared, so a cancel arriving in
        //the meantime can never reach a disposed one.
        InvokeOnMainThread(() =>
        {
            if (ReferenceEquals(cancellation, _searchCts))
            {
                _searchCts = null;
                IsSearching = false;
                IsCancelling = false;
            }

            cancellation.Dispose();
        });
    }
}
```

A page is turned into rows and grouped in full before any of it reaches a bound
collection:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
//The whole page is turned into rows and gathered by repository BEFORE anything reaches the
//bound collections. A repository new to this search then arrives on screen with its rows
//already in it: a group inserted empty and filled a moment later can be measured while it
//is still empty and draws as a bare header until something else forces a fresh layout.
foreach (var repository in order)
{
    var rows = rowsByRepository[repository];

    if (_groupsByRepository.TryGetValue(repository, out var existing))
    {
        foreach (var row in rows) { existing.Add(row); }
        continue;
    }

    var group = new RepositoryGroupViewModel(repository, urlByRepository[repository], OpenUrlAsync);
    foreach (var row in rows) { group.Add(row); }
    _groupsByRepository[repository] = group;
    InsertAlphabetically(group);
}
```

The service side is an ordinary iterator, with one deliberate exception: the public
method is not the iterator, so argument validation happens on the call rather than on
the first `MoveNextAsync`.

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/GitHubIssueSearchService.cs
//Deliberately not an iterator: an iterator would defer the argument checks until the
//first MoveNextAsync, which hides a bad call from anything that only starts the search.
public IAsyncEnumerable<IssueSearchPage> SearchAsync(IssueSearchRequest request,
    IProgress<SearchProgress> progress = null, CancellationToken cancellationToken = default)
{
    if (request == null) { throw new ArgumentNullException(nameof(request)); }
    ObjectDisposedException.ThrowIf(IsDisposed, this);

    //Copied so a caller that edits its request while the pages are still arriving
    //cannot change the query half way through.
    var snapshot = new IssueSearchRequest
    {
        Owner = request.Owner,
        Assignee = request.Assignee,
        IncludeClosed = request.IncludeClosed,
    };

    //Checks the owner and throws now rather than on the first page.
    IssueSearchQueryBuilder.BuildQuery(snapshot);

    return SearchInternalAsync(snapshot, progress, cancellationToken);
}
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs`
`GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/GitHubIssueSearchService.cs`
`GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/IGitHubIssueSearchService.cs`

**Sharp edges.**
- Publish the new `CancellationTokenSource` to the field *before* cancelling the old
  one. Do it the other way round and the old run wakes up while it is still the
  current one, and writes its cancellation over the new run's status line.
- Do the tidying up inside `InvokeOnMainThread` rather than on the background thread.
  It then runs behind everything the run has already posted; clearing the field
  eagerly makes the last page and the closing message look superseded, and the search
  appears to stop one page short.
- A `Progress<T>` created on the UI thread already marshals its callbacks. Wrapping
  them in `InvokeOnMainThread` as well is a second hop for nothing.
- `ConfigureAwait(false)` on the `await foreach` keeps the enumeration off the UI
  thread; the only things that go back to it are the explicit
  `InvokeOnMainThread` calls.

### Make a rate limit wait visible through the progress channel

**When you want this.** Your service sometimes has to sit and wait - for a rate limit
to reset, for a lock, for a retry delay - and a progress bar that simply stops looks
like a hang.

**The MVVM shape.** The wait reports itself on the same `IProgress<T>` channel the
work reports on, once a second, as a phase of its own carrying how much of the wait is
left. The view model switches on the phase to pick the status text, its color and its
glyph. Nothing new is built for the wait: it is another kind of progress.

**Code.**

The progress record carries the phase and, for a wait, how long is left; its
`ToString` writes the sentence for every phase, so the view model does not compose
strings:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Models/SearchProgress.cs
public sealed record SearchProgress(
    SearchPhase Phase,
    int Fetched,
    int? Total,
    int PagesFetched,
    TimeSpan? WaitRemaining,
    DateTimeOffset? WaitUntil,
    RateLimitSnapshot Search,
    RateLimitSnapshot Core)
{
    public override string ToString()
    {
        switch (Phase)
        {
            case SearchPhase.Starting:
                return "Contacting GitHub...";

            case SearchPhase.ListingRepositories:
                return $"Listing repositories ({PagesFetched} {(PagesFetched == 1 ? "page" : "pages")} so far)";

            case SearchPhase.WaitingForQuota:
                return $"Fetched {CountText()} · {WaitText()}";

            // ... one case per remaining phase ...
        }
    }
}
```

The waiting loop announces itself once a second and measures on the injected clock, so
a test can watch a whole hour of reports go by in a millisecond:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/RateLimitThrottle.cs
//Waits until the given moment, announcing what is left once a second. Returns at once
//when the moment has already passed.
internal async Task DelayUntilAsync(DateTimeOffset until, Action<TimeSpan, DateTimeOffset> reportWait,
    CancellationToken cancellationToken)
{
    var now = _timeProvider.GetUtcNow();
    while (now < until)
    {
        var remaining = until - now;
        if (reportWait != null) { reportWait(remaining, until); }

        var slice = remaining > ReportInterval ? ReportInterval : remaining;
        await Task.Delay(slice, _timeProvider, cancellationToken).ConfigureAwait(false);
        now = _timeProvider.GetUtcNow();
    }
}
```

The service turns that callback into a report on the caller's channel:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/GitHubIssueSearchService.cs
private Action<TimeSpan, DateTimeOffset> BuildWaitReporter(IProgress<SearchProgress> progress,
    SearchState state)
{
    if (progress == null) { return null; }

    return (remaining, until) => progress.Report(new SearchProgress(SearchPhase.WaitingForQuota,
        state.Fetched, state.Total, state.PagesFetched, remaining, until,
        _searchThrottle.Snapshot, _coreThrottle.Snapshot));
}
```

The view model reads the phase, not the text, to decide how the line should look:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
switch (report.Phase)
{
    case SearchPhase.WaitingForQuota:
        SetStatus(report.ToString(), SearchStatusKind.Waiting);
        break;

    case SearchPhase.Failed:
    case SearchPhase.Cancelled:
    case SearchPhase.Completed:
        //The view model writes its own sentence for these three, because it knows the
        //repository count and how long the run took.
        break;

    default:
        SetStatus(report.ToString(), SearchStatusKind.Working);
        break;
}
```

The status kind then picks the color role and the glyph, and re-tints the brushes the
view model owns rather than the resource dictionary:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
private void RepaintOwnBrushes()
{
    var palette = CurrentPalette;
    if (palette == null) { return; }

    PaletteBrushes.Repoint(StatusBrush as SolidColorBrush, palette[StatusRole()]);

    var waiting = StatusKind == SearchStatusKind.Waiting;
    PaletteBrushes.Repoint(SearchQuotaBackground as SolidColorBrush,
        waiting ? palette.AttentionSubtle : palette.CanvasInset);
    PaletteBrushes.Repoint(SearchQuotaBorderBrush as SolidColorBrush,
        waiting ? palette.Attention : palette.Hairline);
    PaletteBrushes.Repoint(SearchQuotaForeground as SolidColorBrush,
        waiting ? palette.Attention : palette.TextSecondary);
}
```

**Where to look.**
`GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Models/SearchProgress.cs`
`GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Search/RateLimitThrottle.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Report once a second, not once a frame. The reports go through the UI thread, and a
  countdown that ticks faster than a second is noise the user cannot read anyway.
- Let the library report only what it knows. Cancellation and failure are the view
  model's phases: it is the thing that caught the exception, and it knows what to say
  about the run as a whole.
- A display fed only by progress reports freezes on its last value when the work
  stops. Where a number keeps changing after the run - a quota that refills, a
  countdown to a retry - refresh it from the service on a timer until it is back to
  rest, and stop the timer there.

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
run depends on nothing that can change underneath it. Guard re-entry with a bound
flag that names the command in `[AffectsCommands]`, so setting it at both ends of
the run is the whole of the enablement work, and announce completion after the
`finally` so the button is live again by the time the user dismisses the dialog.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>Whether a marketing one-sheet is being built right now (re-entry gate).</summary>
[AffectsCommands(nameof(DocumentCommand))]
public bool IsCreatingDocument
{
    get;
    private set => SetProperty(ref field, value);
}

private bool CanCreateDocument() => IsModelViewActive && !IsCreatingDocument;

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

    IsCreatingDocument = true;
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
        IsCreatingDocument = false;
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
- Make the re-entry gate a bound property rather than a private field. The
  attribute then refreshes the button for free, and the flag is available to
  anything else on the page that should dim while the run is going.

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

The minimal version, for a view model that owns one command and one disposable it
built itself:

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs
#region | IDisposable implementation |

public override void Dispose()
{
    (PlayerSource as IDisposable)?.Dispose();
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
        _ = DebounceRebuildAsync();
    }
} = string.Empty;

//Waits a beat after the last keystroke before rebuilding, so typing stays smooth. It
//returns a Task rather than being `async void`, so a failure is captured in the task
//instead of escaping onto the UI thread from a property setter.
private async Task DebounceRebuildAsync()
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
- The debounce method returns a `Task` that the setter discards, rather than being
  `async void`. Both are fire-and-forget from the setter's point of view, but a
  failure inside an `async void` is thrown onto the UI thread, while a discarded
  task keeps it. The cancellation is caught either way, because being superseded
  by more typing is the expected outcome and not a fault.
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
only a batch at a time, exposing `HasMoreItems` and a `RequestMore` method. How
big a batch is, and how near the bottom counts as near, are the collection's
policy and are named constants on it; the page contributes only the scroll
geometry, which is the one thing a view model cannot measure. Each item starts
its own thumbnail fetch when it appears.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AssetCellCollection.cs
[Microsoft.UI.Xaml.Data.Bindable]
public class AssetCellCollection : ObservableCollection<AssetCellViewModel>
{
    //Enough cells to overfill the first screen even on a wide monitor.
    private const int InitialBatch = 36;

    /// <summary>
    /// How many further cells to materialize each time the grid asks for more. How near the
    /// bottom edge counts as "near" is a measurement only the view can make, but how much to
    /// add when it does is this collection's own policy, beside its initial batch.
    /// </summary>
    public const int ScrollBatch = 24;

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
//ask the cell collection to materialize the next batch (how big a batch is the
//collection's own policy).
CatalogScroll.ViewChanged += (_, _) =>
{
    var cells = ViewModel?.Cells;
    if (cells == null || !cells.HasMoreItems) { return; }

    var remaining = CatalogScroll.ExtentHeight - CatalogScroll.VerticalOffset - CatalogScroll.ViewportHeight;
    if (remaining < CatalogScroll.ViewportHeight * 2)
    {
        cells.RequestMore(AssetCellCollection.ScrollBatch);
    }
};
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

**Variant: forward the geometry and let the collection decide.** PolyHavenBrowser
moves the last piece of arithmetic off the page too. The page reports three
numbers; the view model forwards them; the collection answers the whole question.

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/ModelCellCollection.cs
    //One scroll-triggered batch, and how much unscrolled extent may remain before it is
    //  asked for: both are this collection's policy, not the page's.
    private const int ScrollBatch = 24;
    private const double LookAheadViewports = 2d;
    // ...
    public void RequestMoreIfNearEnd(double extentHeight, double verticalOffset, double viewportHeight)
    {
        if (!HasMoreItems) { return; }

        var remaining = extentHeight - verticalOffset - viewportHeight;
        if (remaining < viewportHeight * LookAheadViewports)
        {
            RequestMore(ScrollBatch);
        }
    }
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
    public void NotifyCatalogScrolled(double extentHeight, double verticalOffset, double viewportHeight) =>
        Cells?.RequestMoreIfNearEnd(extentHeight, verticalOffset, viewportHeight);
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AssetCellCollection.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml` and
`Views/MainPage.xaml.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/ModelCellCollection.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs` and
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs` (the page's
whole contribution: one `ViewChanged` handler forwarding three numbers)

**Sharp edges.**
- Filtering swaps in a whole new collection instance rather than mutating the
  existing one, which gives the view model one place - the collection property's
  own setter - to say "and scroll the grid back to the top", through a bridge
  delegate the page supplies.
- A threshold of two viewports means a batch is already in place before the user
  reaches the end.
- `RequestMore` is safe to call repeatedly and no-ops once everything is
  materialized.
- The collection type is marked `[Microsoft.UI.Xaml.Data.Bindable]`, as is every
  other bound type in these applications, including plain record types.
- Split the question at the one line a view model cannot answer. "How far down
  has the grid scrolled" needs the control; "is that near enough, and how many
  more" does not. The batch size is a public constant the page names rather than
  a number the page invents.

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
        //A setter cannot await, so the one-shot child load is started through a helper
        //  that observes it rather than through a bare fire-and-forget discard.
        if (value) { BackgroundWork.StartAndObserve(EnsureChildrenLoadedAsync, ReportChildLoadFailure); }
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
  never throws into the expand gesture. The expand setter cannot await, so the
  load goes through a start-and-observe helper with the row's own failure
  reporter; see
  [Start work you cannot await through a helper that observes it](#start-work-you-cannot-await-through-a-helper-that-observes-it).

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
        StatusText = "Publishing cancelled — the existing file was kept.";
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
        SetPlayerSource(MediaSource.CreateFromUri(uri));
        StatusText = $"Loaded: {uri}";
    }
    catch (Exception ex)
    {
        _log.LogWarning(ex, "Cannot load '{MediaAddress}'.", MediaAddress);
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
- Status text is for the user; a log line is for whoever has to work out why. The
  catch does both, and the logged form keeps the exception itself, which the
  sentence on screen throws away.

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
        // ...
        LeftPane = new DocumentPaneViewModel("Document 1", () => BrowseAsync(DocumentSide.Left));
        RightPane = new DocumentPaneViewModel("Document 2", () => BrowseAsync(DocumentSide.Right));

        //A newly rendered page is one more thing that moves the view, so each pane's image change
        //  is folded into the one signal the page watches
        LeftPane.PropertyChanged += OnPanePropertyChanged;
        RightPane.PropertyChanged += OnPanePropertyChanged;
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
    /// <summary>Shows document (or clears the pane when it is <c>null</c>).</summary>
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
  holding a reference to the parent. PdfSideBySide's parent subscribes to each
  pane's `PropertyChanged` and folds it into the one signal the page watches, so
  the page still has a single subscription however many children there are - and
  the parent unsubscribes from both panes in its own `Dispose()`.
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

The view model offers the info objects themselves, and the control is told which
of their properties to show:

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
    private readonly Dictionary<EncryptionMode.CryptAlgorithm, EncryptionMode> _encryptionModeDictionary =
        EncryptionMode.GetDictionary();

    /// <summary>The algorithms the picker offers, in enum order; the picker displays their Description.</summary>
    public IReadOnlyList<EncryptionMode> EncryptionModes
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>The algorithm that the picker currently has selected.</summary>
    public EncryptionMode SelectedEncryptionMode
    {
        get;
        set => SetProperty(ref field, value);
    }
```

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml -->
            <ComboBox HorizontalAlignment="Center"
                VerticalAlignment="Center"
                Width="300"
                ItemsSource="{d:Binding EncryptionModes}"
                DisplayMemberPath="Description"
                SelectedItem="{d:Binding SelectedEncryptionMode, Mode=TwoWay}" />
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`
`JustBetweenUs/Shared/ViewModels/EncryptionMode.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml`

**Also shown by.**
`JustBetweenUs/Mobile/Views/MainPage.xaml` (the same two properties behind a MAUI
`Picker`, whose `ItemDisplayBinding` plays the part `DisplayMemberPath` plays on
the other heads - one view model, two dialects of the same binding),
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs` and
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
(a page-size option type bound the same way)

**Sharp edges.**
- Enum-valued bound properties use `SetEnumProperty()`, not `SetProperty()`.
- A curated list written out by hand keeps unwanted members out of the picker and
  fixes the display order; `Enum.GetValues()` gives you neither.
- Bind the object and name its display property; do not bind the description
  string. Binding the text forces the setter to map it back to a member with a
  `Single()` lookup, which throws the moment two members share a description, and
  it puts a lookup in the selection path of every pick.
- The list property is set once, from the constructor, through `SetProperty`, and
  the picker's initial selection is set in the same place. A selected item that is
  not an element of the offered list shows as nothing selected.

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
public RenderEngineKind SelectedRenderEngine
{
    get => _selectedRenderEngine;
    set
    {
        if (value == _selectedRenderEngine) { return; }

        //Optimistic: show the new selection at once; SwitchEngineAsync reverts it if the
        //engine is unsupported or fails to initialize.
        SetEnumProperty(ref _selectedRenderEngine, value);
        _ = RunSwitchEngineAsync(value);
    }
}

//Switches the 3D engine behind the model painter: alert + snap back when the engine is
//not okayed for this platform, otherwise swap painters and re-display the current sample.
private async Task SwitchEngineAsync(RenderEngineKind kind)
{
    if (kind == _currentEngineKind) { return; }

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
    _selectedRenderEngine = _currentEngineKind;
    NotifyPropertyChanged(nameof(SelectedRenderEngine));
}
```

The setter discards the task it starts, so a wrapper stands between the two and
turns anything the switch throws into the same status line and the same revert:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
    //Starts an engine switch from the bound setter. The task is discarded, so this wrapper is
    //what guarantees a failure becomes status text rather than an unobserved exception.
    private async Task RunSwitchEngineAsync(RenderEngineKind kind)
    {
        try
        {
            await SwitchEngineAsync(kind);
        }
        catch (Exception ex)
        {
            StatusText = $"Could not switch to {kind} rendering: {ex.Message}";
            RevertEngineSelection();
        }
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
- The offered values are the enum members themselves, not their names, so the
  validation is a comparison rather than a parse and an unknown string is not a
  case that can arise. The setter uses `SetEnumProperty`, which is the overload
  an enum-valued bound property needs.

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
private SimpleCommand _showOsInfoCommand;
public SimpleCommand ShowOsInfoCommand =>
    (_showOsInfoCommand ??= new SimpleCommand(DoShowOsInfo));

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
- The command is held in an explicit field, like every other command in the file,
  so `Dispose()` can reach it. A `field ??=` command property creates the command
  once too, but nothing can dispose what it holds.
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

The page subscribes once, from a method both its data-context change and its
loaded event call, and drops the subscription when it unloads:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs
    //One property to watch: the view model folds everything that moves the view - a zoom, a pan, a
    //  page change, a newly rendered image - into ViewVersion
    private void WireViewModel()
    {
        var viewModel = ViewModel;
        if (ReferenceEquals(viewModel, _wiredViewModel)) { return; }

        UnwireViewModel();
        if (viewModel == null) { return; }

        _wiredViewModel = viewModel;
        _wiredViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void UnwireViewModel()
    {
        if (_wiredViewModel == null) { return; }

        _wiredViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _wiredViewModel = null;
    }

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(MainViewModel.ViewVersion)) { ApplyViews(); }
    }
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Every call site that changes the model goes through one method, which bumps the
  counter, re-notifies the derived labels and refreshes the commands. The panes'
  own change notifications are folded into it by the parent view model, so the
  page still watches exactly one name.
- `nameof(MainViewModel.ViewVersion)` keeps the page's filter refactor-safe.
- A counter, not a `bool` or an event: any increment is a change, and it survives
  being read late.
- Subscribe through a method that first unsubscribes, and call it from both the
  data-context change and the loaded event. A lambda subscribed inline cannot be
  removed, and a page whose data context is set more than once then holds a
  handler per assignment.

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
private async Task ReloadCatalogCoreAsync()
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

**Variant: await the network, decode off the UI thread, apply on it.** The work
splits in two at the line where the result stops being data and starts being
something the UI owns.

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
        var asset = await _assets.EnsureSampleAsync(kind, progress, _lifetime.Token);

        //Decode off the UI thread; the painters upload to GL lazily during Paint.
        var decoded = await Task.Run(() => DecodeSample(kind, asset), _lifetime.Token);

        //Hand the decoded content to a painter back on the UI thread: the painters, their
        //cameras and the bound status line are only ever touched there.
        InvokeOnMainThread(() =>
        {
            CurrentPainter = ApplyDecodedSample(kind, decoded);
            StatusText = $"{Label(kind)}: {asset.Name}    ·    {Hint(kind)}";
        });
    }
    catch (OperationCanceledException)
    {
        //The view model is shutting down; leave the status line as it is.
    }
    catch (Exception ex)
    {
        StatusText = $"Could not load the {kind.ToString().ToLowerInvariant()} sample: {ex.Message}";
    }
    finally
    {
        IsBusy = false;
        RequestRender();
    }
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
    //Runs on a worker thread: decodes the downloaded file and builds the mesh. It touches no
    //painter, no camera and no bound state, so nothing here needs the UI thread.
    private static DecodedSample DecodeSample(SampleAssetKind kind, SampleAsset asset)
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
- Split the method where the answer stops being data: the decode half is `static`
  and takes only what it needs, which is how you can tell by looking that it
  touches no bound state; the apply half runs inside `InvokeOnMainThread`. A
  `static` worker method is the cheapest proof a reviewer can have.
- Pass a token that is cancelled when the view model is disposed, and catch
  `OperationCanceledException` separately from a real failure, so a window closing
  mid-load does not write an error onto a status line nobody will read.
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
    if (ReferenceEquals(CurrentPainter, oldPainter))
    {
        CurrentPainter = null;
    }
    oldPainter?.Dispose();
}
catch (Exception ex)
{
    StatusText = $"Could not switch to {kind} rendering: {ex.Message}";
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

**The MVVM shape.** Two independent mechanisms, both on the view model. Paint
coalescing keeps at most one pending invalidate, raised through the page's
invalidate delegate. Backlog detection compares the pointer event's own timestamp
against a stopwatch and, when the input stream has fallen behind, advances the
painter's drag anchor without rendering, so the camera stays in sync with the
cursor while frames are skipped. The page contributes the two things only it can:
the pixel coordinates of the event, and the pointer capture.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
    //A pointer frame that is running more than this far behind real time is a backlog
    //frame: keep the cursor anchor in sync but skip rendering it, catching up to the latest.
    private const double StaleFrameMicroseconds = 1_000_000; // 1 second

    //Tracks how far behind real time the pointer stream is, to detect a backlog.
    private readonly Stopwatch _gestureClock = new();
    private double _gestureStartTimestamp;

    //Coalescing: never queue more than one paint. While one is pending, pointer moves only
    //update the camera; the next paint draws the latest state.
    private bool _renderPending;
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
    public void PaintCanvas(SKSurface surface, SKImageInfo info)
    {
        _renderPending = false;
        _currentPainter?.Paint(surface, info);
    }

    /// <summary>
    /// Requests a repaint, keeping at most one queued: while a paint is pending, pointer moves
    /// only update the camera and the next paint draws the latest state.
    /// </summary>
    public void RequestRender()
    {
        if (_renderPending) { return; }
        _renderPending = true;
        InvalidateCanvas?.Invoke();
    }
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
    public bool PointerMoved(double x, double y, ulong timestamp)
    {
        var painter = _currentPainter;
        if (painter == null) { return false; }

        if (IsBacklogFrame(timestamp))
        {
            //Discard this stale frame: stay aligned with the cursor but don't render it.
            painter.PointerSkip(x, y);
        }
        else
        {
            painter.PointerDrag(x, y);
            RequestRender();
        }

        return true;
    }

    //True when this pointer frame is running far enough behind real time to be a backlog
    //frame that should be dropped rather than rendered.
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
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IScenePainter.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/ModelScenePainter.cs`

**Sharp edges.**
- The pending flag is cleared at the top of the paint method, so a request made
  during paint still queues the next frame.
- `PointerSkip` exists precisely so that dropping a frame does not make the scene
  jump: it moves the anchor without applying the delta to the camera.
- On pointer release the view model requests one more render at full, non-drag
  resolution, which is what makes a two-tier resolution scheme work.
- None of this is view code, even though all of it is about painting: the policy
  reads a timestamp the event carries and a flag of its own. What the page keeps
  is converting the event's position into canvas pixels, and capturing and
  releasing the pointer.
- The pointer methods return a `bool` so the page knows whether to take the
  capture. A `void` signature would leave the page guessing whether anything took
  the press.

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
- The new session is a private field, not a bound property. It reaches the screen
  through the view model's own renderer and the invalidate delegate, so there is
  nothing to notify - and nothing that could hand a page a Skia object. A public
  property here would need an explicit `NotifyPropertyChanged` on every
  replacement, because a plain expression-bodied property over a field does not
  notify itself.
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

The command model that starts a preview raises a synchronous event, so the
manager offers a `void` entry point over an awaitable method, and observes the
task it discards:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/LivePreviewManager.cs
	public void Start (BaseEffect effect)
	{
		_ = RunAndObserve (StartAsync (effect), effect.Name);
	}

	private static async Task RunAndObserve (Task run, string effectName)
	{
		try {
			await run;
		} catch (Exception ex) {
			Debug.WriteLine ($"Live preview of '{effectName}' failed: {ex}");
		}
	}
```

The run itself is one method: the preview surface, the worker-thread render, the
configuration dialog and the history item the confirmed result becomes.

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
- Keep the real work awaitable and put the `void` entry point beside it, rather
  than writing the run itself as `async void`. The entry point is then the only
  place that has to observe the task, and a caller that can wait - a test, or
  another `async` method - gets the failure instead of losing it; see
  [Start work you cannot await through a helper that observes it](#start-work-you-cannot-await-through-a-helper-that-observes-it).

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

### Mutate a plot model under its own sync root so streamed batches need no dispatcher hop

**When you want this.** A device, a decoder or a sensor delivers batches on its
own thread, far faster than a repaint, and every batch must reach the chart
without being dropped or queued. This is the case
[Set bound properties from a background thread with InvokeOnMainThread](BLUEPRINTS-MVVM.md#set-bound-properties-from-a-background-thread-with-invokeonmainthread)
and
[Hand results from a capture thread through a worker to the UI thread](BLUEPRINTS-MVVM.md#hand-results-from-a-capture-thread-through-a-worker-to-the-ui-thread)
do not cover: the data never crosses to the UI thread at all, because the render
target and the producer share one lock. Only the notifications cross.

**The MVVM shape.** The view model subscribes to the device's event and forwards
the batch straight into its chart object on the thread it arrived on. The chart
object holds the render target's own sync root while it mutates, and asks for a
repaint after releasing it. Bound properties keep the ordinary rule: every
setter marshals its change notification.

**Code.**

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs
private void OnSamplesAvailable(object sender, StreamingSamplesEventArgs e)
{
    //Arrives on the polling thread. ScopePlot locks the model while it
    //  mutates and the plot view renders under the same lock, so the batch
    //  is applied right here.
    Plot.AppendStreaming(e);

    if (!_loggedFirstBatch)
    {
        _loggedFirstBatch = true;
        _log.LogInformation("Streaming: first batch of {Count} samples at {Interval} ns/sample.",
            e.SampleCount, e.IntervalNanoseconds);
    }
}
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/Charting/ScopePlot.cs
public void AppendStreaming(StreamingSamplesEventArgs batch)
{
    if (batch == null) { throw new ArgumentNullException(nameof(batch)); }

    lock (Model.SyncRoot)
    {
        double intervalSeconds = batch.IntervalNanoseconds / 1e9;
        double widestVolts = 0;

        // ... decimate, append and trim each channel's points ...

        _streamElapsedSeconds += batch.SampleCount * intervalSeconds;

        double windowEnd = _streamElapsedSeconds;
        _timeAxis.Minimum = Math.Max(0, windowEnd - StreamWindowSeconds);
        _timeAxis.Maximum = windowEnd > 0 ? windowEnd : double.NaN;
        ApplyVoltageRange(widestVolts);

        Model.Subtitle = FormatStreamSubtitle(batch);
    }

    Model.InvalidatePlot(true);
}
```

The bound half of the view model is unchanged by any of this: it is still the
UI thread's, and every setter says so.

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs
/// <summary>A one-line status message for the view.</summary>
public string StatusText
{
    get => _statusText;
    private set => SetProperty(ref _statusText, value ?? string.Empty, notifyOnMainThread: true);
}
```

**Where to look.**
`PicoScope.Brix/src/PicoScope.Brix.Core/Charting/ScopePlot.cs`
`PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs` (the class
remark on threading, and every bindable property)

**Sharp edges.**
- This works only because the control documents that it renders under the
  model's sync root and accepts an invalidate from any thread. Check that a
  render target makes that promise before copying the pattern; without it, the
  batch has to be marshalled like anything else.
- Ask for the repaint after the lock is released, not inside it.
- Bound properties still belong to the UI thread. A change notification raised
  from the polling thread breaks binding, which is why every setter here passes
  the notify-on-main-thread flag.
- The lock is held for the whole batch, so keep the work inside it bounded -
  here the decimation is what keeps it short, and it happens inside the lock on
  purpose so nothing else can observe a half-updated series.
- The chart object is the only thing that touches the model. That is what makes
  a single-lock argument reviewable at all.

### Root a native callback delegate for the life of a streaming session

**When you want this.** A native library calls back into managed code for as
long as a session runs, and the session is started and stopped by commands on a
view model. It is the lifetime sibling of
[Survive a native runtime tearing down while a frame is in flight](BLUEPRINTS-MVVM.md#survive-a-native-runtime-tearing-down-while-a-frame-is-in-flight):
that one is about a call that outlives the runtime, this one about a delegate
that must not be collected before the session ends.

**The MVVM shape.** The view model's start and stop commands call a library
session object and know nothing about delegates. The library holds the delegate
in a field for exactly as long as the session runs, copies the data out inside
the callback, and clears the field only after the polling loop has stopped.

**Code.**

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/Ps2000ScopeDataDevice.cs
//Rooted for the lifetime of a stream so the garbage collector cannot
//  reclaim the native thunk between polls. Passing a method group directly
//  to the P/Invoke would allocate a fresh delegate per call and leave its
//  lifetime to chance.
private Ps2000Api.GetOverviewBuffersMaxMin _streamingCallback;
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/Ps2000ScopeDataDevice.cs
//Root the delegate for the lifetime of the stream. Binding a method
//  group to a delegate whose signature contains pointers needs an
//  unsafe context, even though no pointer is dereferenced here.
unsafe { _streamingCallback = StreamingCallback; }

_streamCancellation = new CancellationTokenSource();
CancellationToken token = _streamCancellation.Token;
_streamTask = Task.Run(() => StreamLoop(settings, token), token);
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/Ps2000ScopeDataDevice.cs
try
{
    cts.Cancel();
    task?.Wait(TimeSpan.FromSeconds(2));
}
catch (AggregateException)
{
    //Cancellation surfaces as an exception on the task; expected.
}
finally
{
    cts.Dispose();
    _streamingCallback = null;
    if (IsOpen) { Ps2000Api.ps2000_stop(_handle); }
}
```

The callback itself runs inside the polling call, so everything it is handed has
to be copied before it returns:

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/Ps2000ScopeDataDevice.cs
lock (_syncRoot)
{
    int channelIndex = 0;
    foreach (ChannelId channel in _streamBuffers.Keys.ToArray())
    {
        //Two buffers per channel: maxima then minima. With no
        //  aggregation the two are identical, so the maximum is taken.
        short* source = overviewBuffers[channelIndex * 2];
        if (source == null) { channelIndex++; continue; }

        List<short> target = _streamBuffers[channel];
        for (uint i = 0; i < nValues; i++) { target.Add(source[i]); }
        channelIndex++;
    }
}
```

**Where to look.**
`PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/Ps2000ScopeDataDevice.cs`
`PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/Interop/Ps2000Api.cs`
(the delegate declaration and what it says about lifetime)

**Sharp edges.**
- One delegate in a field, created once when the session starts. Passing a
  method group at every call site allocates a fresh delegate whose lifetime
  nobody owns, and the symptom - a callback that simply stops firing, usually
  under load - looks nothing like a collected delegate.
- Binding a method group to a delegate whose signature contains pointers needs
  an unsafe context even though no pointer is dereferenced at that line, so the
  interop project has to allow unsafe blocks. That requirement belongs to the
  interop library only; nothing else in the application needs it.
- The data the callback is handed is valid only for the duration of the call.
  Copy it out; do not keep the pointers.
- Clear the field in a `finally`, after the polling task has been cancelled and
  waited for, so the delegate outlives the last call rather than the other way
  round.
- Expect noise on the first status check of a freshly started session; treating
  the first overrun report as real produces a warning on every start.

### Refresh every section from one shared snapshot and one pausable timer

**When you want this.** Several sections of one window all read the same remote
state - a daemon, a device, a service - and each of them polling for itself would
mean several sets of calls racing each other for the same answers, several
different ideas of what is current, and no way to hold the polling still while a
long operation runs. The timer itself is a familiar piece; what is different here
is that one timer and one snapshot serve every section, where
[Marshal a repeating timer into a headless model](BLUEPRINTS-PlatformServices.md#marshal-a-repeating-timer-into-a-headless-model)
is about getting a tick into a library that may not touch the dispatcher at all.

**The MVVM shape.** A plain service class owns the snapshot: get-only properties
for everything the screen needs, one `RefreshAsync` that fills all of them behind
a semaphore, and one event raised afterwards whether the refresh succeeded or
not. A second small class owns the single dispatcher timer and can suppress its
ticks without stopping it. The shell view model wires the two together, and then
pushes the snapshot into its own bound properties and into each section's
`ApplySnapshot`. No section ever calls the service the snapshot came from.

**Code.**

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/Services/AppState.cs
/// <summary>
/// The shared, observable snapshot every section reads. One refresh asks the daemon for
/// everything the shell and the eight sections need, so the sections never race each other
/// for the same daemon call. <see cref="Changed"/> is raised on whatever thread the refresh
/// finished on; subscribers marshal for themselves.
/// </summary>
public sealed class AppState
{
    private readonly IDockerManager _docker;
    private readonly IRedisTopologyService _topologies;
    private readonly SemaphoreSlim _gate = new(1, 1);

    // ... the constructor, and a get-only property per thing the snapshot holds ...

    /// <summary>Raised after every refresh, successful or not.</summary>
    public event Action Changed;

    /// <summary>Whether the last refresh reached the daemon.</summary>
    public bool IsDaemonReachable { get; private set; }

    /// <summary>The message from the last failed refresh, or null when the last one succeeded.</summary>
    public string LastError { get; private set; }

    // ...

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var daemon = await _docker.GetDaemonInfoAsync(cancellationToken).ConfigureAwait(false);
            Daemon = daemon;
            IsDaemonReachable = daemon is not null && daemon.IsReachable;

            if (!IsDaemonReachable)
            {
                LastError = "The Docker daemon did not answer at " + _docker.Endpoint + ".";
                LastRefreshed = DateTimeOffset.Now;
                return;
            }

            Containers = await _docker.ListContainersAsync(true, cancellationToken)
                .ConfigureAwait(false);
            // ... images, networks, volumes, disk usage, discovered instances, advisor findings ...

            LastError = null;
            LastRefreshed = DateTimeOffset.Now;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            IsDaemonReachable = false;
            LastError = exception.Message;
            LastRefreshed = DateTimeOffset.Now;
        }
        finally
        {
            _gate.Release();
            Changed?.Invoke();
        }
    }
}
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/Services/RefreshCoordinator.cs
/// <summary>
/// One <see cref="DispatcherTimer"/> driving the app's periodic refresh. It exists so that
/// exactly one timer ticks for the whole application instead of one per section, and so the
/// tick can be paused while a long operation (creating an instance, tearing one down) is in
/// flight. Construct it on the UI thread.
/// </summary>
public sealed class RefreshCoordinator
{
    private readonly DispatcherTimer _timer = new();
    private bool _isPaused;

    // ... the constructor sets the interval and hooks Tick ...

    /// <summary>Raised on the UI thread on every unpaused tick.</summary>
    public event Action Tick;

    /// <summary>Starts ticking.</summary>
    public void Start() => _timer.Start();

    /// <summary>Stops ticking altogether.</summary>
    public void Stop() => _timer.Stop();

    /// <summary>Suppresses ticks without stopping the timer, for the length of a long operation.</summary>
    public void Pause() => _isPaused = true;

    /// <summary>Lets ticks through again.</summary>
    public void Resume() => _isPaused = false;
}
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/ViewModels/MainViewModel.cs
_refresh = new RefreshCoordinator();
_refresh.Tick += () => _ = RefreshAsync();
_refresh.Start();

// ...

public async Task RefreshAsync()
{
    if (_isRefreshing) { return; }

    _isRefreshing = true;
    IsBusy = true;
    try
    {
        await _state.RefreshAsync().ConfigureAwait(true);
        ApplyState();
    }
    finally
    {
        IsBusy = false;
        _isRefreshing = false;
    }
}

// ...

private void ApplyState()
{
    IsDaemonReachable = _state.IsDaemonReachable;
    var daemon = _state.Daemon;
    DaemonPillText = _state.IsDaemonReachable && daemon is not null
        ? "Docker " + daemon.ServerVersion + " · API " + daemon.ApiVersion
        : "daemon unreachable";
    DaemonPillBrush = _state.IsDaemonReachable ? Palette.Good : Palette.Bad;
    // ... the footer captions and the rail badges ...

    Dashboard.ApplySnapshot();
    Instances.ApplySnapshot();
    CreateInstance.ApplySnapshot();
    Containers.ApplySnapshot();
    Consoles.ApplySnapshot();
    Images.ApplySnapshot();
    NetworksVolumes.ApplySnapshot();
    System.ApplySnapshot();
}
```

A long operation pauses the tick rather than stopping the timer, and resumes it
in a `finally` so a failure cannot leave the application frozen:

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/ViewModels/CreateInstanceViewModel.cs
IsCreating = true;
SetError(null);
ProgressLines.Clear();
ProgressHeadline = "Starting…";
Shell?.PauseAutoRefresh();
_createCancellation = new CancellationTokenSource();

// ...

finally
{
    _createCancellation?.Dispose();
    _createCancellation = null;
    IsCreating = false;
    Shell?.ResumeAutoRefresh();
}
```

**Where to look.**
`RedisSetupTool/src/RedisSetupTool.Core/Services/AppState.cs`
`RedisSetupTool/src/RedisSetupTool.Core/Services/RefreshCoordinator.cs` and
`ViewModels/MainViewModel.cs`, `ViewModels/SectionViewModel.cs`

**Sharp edges.**
- The refresh never throws. A failure leaves the reachability flag false and the
  message beside it, and the change event fires from the `finally` either way, so
  a section that lost the daemon redraws as empty rather than as stale.
- The event is raised on whatever thread the refresh finished on. Say so on the
  event itself, as this one does, because subscribers have to marshal for
  themselves.
- The dispatcher timer has to be constructed on the UI thread. Building the
  coordinator in the shell's constructor is what guarantees that.
- Two guards are doing different jobs: the snapshot's semaphore serializes
  refreshes from any caller, while the shell's own re-entry flag stops a tick
  queueing behind the refresh already running. And pause, do not stop - a stopped
  timer has to be restarted by someone, and the someone is easy to forget on the
  failure path.

### Let section view models ask the shell for the few things they cannot do

**When you want this.** A shell holds several child view models that occasionally
need something only the shell can do - show a different section, raise a dialog,
reach the clipboard, hold the polling still - and you do not want to hand each
child a reference to the whole shell. This is the narrow-interface half of
[Compose a page from a parent view model and child view models](#compose-a-page-from-a-parent-view-model-and-child-view-models):
that recipe is about the parent owning the children and pushing state down, this
one is about what the children are allowed to ask for on the way back up, written
as one interface the parent implements.

**The MVVM shape.** The shell implements a context interface and passes `this` to
every child's constructor. The interface is deliberately short: it is the complete
list of cross-section moves the application allows, and adding to it is a
deliberate act. An abstract section base class stores it, exposes the shared
snapshot through it, and wraps the one thing every section does the same way -
running a remote operation with a busy flag and turning a failure into a readable
line.

**Code.**

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/Services/IShellContext.cs
/// <summary>
/// What a section view model may ask of the shell. <c>MainViewModel</c> implements it and
/// hands itself to every child at construction, which keeps the children free of a back
/// reference to the whole shell and makes the small set of cross-section moves explicit.
/// </summary>
public interface IShellContext
{
    /// <summary>The shared daemon snapshot every section reads.</summary>
    AppState State { get; }

    /// <summary>Shows the given section.</summary>
    void Navigate(SectionKey section);

    /// <summary>Puts text on the clipboard, doing nothing on a head with no clipboard.</summary>
    void CopyToClipboard(string text);

    /// <summary>Re-reads everything from the daemon and pushes it into every section.</summary>
    Task RefreshAsync();

    /// <summary>Opens a console tab on a container and shows the Consoles section.</summary>
    void OpenConsole(string containerId, string containerName);

    /// <summary>Shows the Containers section with the given container selected.</summary>
    void ShowContainer(string containerId);

    /// <summary>Suppresses the periodic refresh while a long operation runs.</summary>
    void PauseAutoRefresh();

    /// <summary>Lets the periodic refresh resume.</summary>
    void ResumeAutoRefresh();

    /// <summary>
    /// Asks the user to confirm something. Dialogs go through the shell because only the view
    /// model the page set as its DataContext has been given a <c>XamlRoot</c> to attach one to.
    /// </summary>
    Task<bool> ConfirmAsync(string message, string title);

    // ... ShowErrorAsync, ShowInfoAsync and the automation log line ...
}
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/ViewModels/MainViewModel.cs
public MainViewModel()
{
    if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

    _state = GetService<AppState>();
    _docker = GetService<IDockerManager>();
    _automation = new StartupAutomation(this);

    Dashboard = new DashboardViewModel(this);
    Instances = new InstancesViewModel(this);
    CreateInstance = new CreateInstanceViewModel(this);
    // ... the other five sections, each handed the same `this` ...
}

// ...

/// <inheritdoc />
public void Navigate(SectionKey section)
{
    if (_currentSection == section) { return; }

    //Leaving a section cancels whatever live feed it was running: the container stats
    //  stream and the log poll are the two that would otherwise keep asking the daemon.
    if (_currentSection == SectionKey.Containers) { Containers.Suspend(); }

    // ... flip the selected rail row and notify the eight Visibility properties ...
}

/// <inheritdoc />
public void CopyToClipboard(string text)
{
    //A head with no clipboard leaves the delegate null; copying is then simply a no-op.
    CopyTextToClipboard?.Invoke(text);
}

/// <inheritdoc />
public Task<bool> ConfirmAsync(string message, string title) => ConfirmDialog(message, title);
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/ViewModels/SectionViewModel.cs
protected SectionViewModel(IShellContext shell)
{
    Shell = shell;
}

/// <summary>The shell that owns this section. Null in design mode.</summary>
protected IShellContext Shell { get; }

/// <summary>The shared daemon snapshot.</summary>
protected AppState State => Shell?.State;

// ...

protected async Task<bool> RunAsync(Func<Task> work, bool refreshAfter = true)
{
    if (work is null) { return false; }

    IsBusy = true;
    NotifyPropertyChanged(nameof(IsNotBusy));
    SetError(null);
    try
    {
        await work().ConfigureAwait(true);
        if (refreshAfter && Shell is not null)
        {
            await Shell.RefreshAsync().ConfigureAwait(true);
        }
        return true;
    }
    catch (OperationCanceledException)
    {
        //A cancelled operation is the user changing their mind, not a failure.
        return false;
    }
    catch (Exception exception)
    {
        SetError(exception.Message);
        return false;
    }
    finally
    {
        IsBusy = false;
        NotifyPropertyChanged(nameof(IsNotBusy));
    }
}
```

**Where to look.**
`RedisSetupTool/src/RedisSetupTool.Core/Services/IShellContext.cs`
`RedisSetupTool/src/RedisSetupTool.Core/ViewModels/MainViewModel.cs` and
`ViewModels/SectionViewModel.cs`, `ViewModels/ConsolesViewModel.cs`

**Sharp edges.**
- The design-mode guard returns before the children are built, so the shell
  reference a section holds is null in the designer. Every use of it in a section
  is null-conditional, and the base class's snapshot property is too.
- Dialogs belong on the interface, not in the child, because only the view model
  the page set as its data context was handed a root to attach one to. A child
  that raises its own dialog silently does nothing.
- Leaving a section is the natural place to stop whatever live feed it was
  running. Put that in the navigate method rather than in each section, or one day
  a section will keep polling from behind another one.
- Keep the interface honestly small. Every method added to it is a new thing every
  section is permitted to do, and the list is the only documentation of that
  permission.

### Cache the newest frame in the view model and let the renderer pull it

**When you want this.** A device raises frames faster than the screen can show
them, and you want whatever arrived most recently on screen with no queue, no
backlog bookkeeping and no worker thread in between. This is the pull half of a
producer and consumer pair; the push half, where frames are handed to a
processing worker that swaps buffers under its own lock, is
[Run a sensor pipeline on a worker thread with latest frame wins](BLUEPRINTS-MVVM.md#run-a-sensor-pipeline-on-a-worker-thread-with-latest-frame-wins).
The difference is who asks: there the producer submits and a worker consumes,
here the producer only refreshes a cache and the UI thread's paint handler takes
what it finds.

**The MVVM shape.** The view model owns the buffer, the lock and the two
dimensions, and is the only thing that knows the frames exist. The capture
callback writes into that cache and leaves. A `TryGet...` method reads out into a
buffer the caller owns, and is the whole surface the renderer is given - declared
on a one-member interface the view model implements, so the renderer takes the
source rather than the view model. No control, no Skia type and no dispatcher
appears on either side of it.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs
private readonly object _frameLock = new object();
private byte[] _latestFrame;
private int _frameWidth;
private int _frameHeight;

// ...

private void OnFrameReceived(object sender, WebcamFrameEventArgs frame)
{
    // Capture-thread context: copy the pixels and get out fast.
    lock (_frameLock)
    {
        var needed = (int)(frame.Width * frame.Height * 4);
        if (_latestFrame == null || _latestFrame.Length != needed)
        {
            _latestFrame = new byte[needed];
        }
        frame.CopyTo(_latestFrame);
        _frameWidth = (int)frame.Width;
        _frameHeight = (int)frame.Height;
    }

    if (!HasFrame)
    {
        InvokeOnMainThread(() => HasFrame = true);
    }
    InvalidateCanvas?.Invoke();
}
```

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Copies the most recent video frame (tightly packed BGRA) into <paramref name="buffer"/>
/// (which is (re)allocated as needed). Returns false when no frame has arrived yet.
/// Called by the canvas renderer on the UI thread.
/// </summary>
public bool TryGetLatestFrame(ref byte[] buffer, out int width, out int height)
{
    lock (_frameLock)
    {
        if (_latestFrame == null)
        {
            width = 0;
            height = 0;
            return false;
        }
        if (buffer == null || buffer.Length != _latestFrame.Length)
        {
            buffer = new byte[_latestFrame.Length];
        }
        Array.Copy(_latestFrame, buffer, _latestFrame.Length);
        width = _frameWidth;
        height = _frameHeight;
        return true;
    }
}
```

The read is declared on an interface the view model implements, so the renderer
never names the view model; and because the consumer is a paint handler, "nothing
yet" has to be a normal answer rather than an error:

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Lets the canvas renderer pull the most recent webcam frame without knowing which
/// concrete view model produced it. The hosting page resolves its data context through this
/// interface and hands it to the renderer from the paint handler.
/// </summary>
public interface IVideoFrameSource
{
    // ...
}
```

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/Video/VideoCanvas.cs
    public static void RenderFrame(SKSurface surface, SKImageInfo info, IVideoFrameSource frameSource)
    {
        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);

        if (frameSource == null
            || !frameSource.TryGetLatestFrame(ref _frameBuffer, out int width, out int height)
            || width <= 0 || height <= 0)
        {
            return;
        }
        // ...
    }
```

**Where to look.**
`WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs`
`WebcamViewer/src/WebcamViewer.Core/Video/VideoCanvas.cs` and
`src/WebcamViewer.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- One lock guards three fields together - the pixels, the width and the height -
  so a reader can never pair new pixels with the previous frame's dimensions.
- Both sides reallocate only when the length changes, because a camera can change
  resolution mid-session. Steady state is two copies per displayed frame and no
  allocation at all, and that is the deliberate trade for never letting the two
  threads touch the same array.
- Nothing but the copy happens inside the lock. The bound flag's dispatch and the
  repaint request are both raised after it is released.
- The "a frame has arrived" flag is raised on the transition only, and through
  `InvokeOnMainThread` because it is bound and gates a command. Raising it per
  frame would churn that command many times a second for no change.
- Nothing is queued, so a slow UI misses frames instead of building a backlog.
  That is the intended behavior for a live view and the wrong behavior for a
  recorder.
- The renderer takes the interface, and the page hands it its data context cast to
  that interface. Naming the view model type in the render path would tie a static
  helper to one application's view model for no gain; the pull half of the pair is
  one method wide, so the interface is one method wide too.

### Push a bound toggle into a live native session and re-apply it to the next one

**When you want this.** A checkbox controls something inside a running native
session - audio monitoring, a torch, a mute - and the user expects it to take
effect the moment it is clicked, to stay grayed out on a device that cannot do
it, and to survive switching to another device.

**The MVVM shape.** Three members and no plumbing. A read-only capability flag
the session reports after it has started drives `IsEnabled`. A two-way bound
setting drives `IsChecked`, and its setter both stores the value and pushes it
into the live session. One line in the device-switch path copies the stored value
into each new session before it is started.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs
private bool _isMicAvailable;
public bool IsMicAvailable
{
    get => _isMicAvailable;
    private set => SetProperty(ref _isMicAvailable, value);
}

private bool _isAudioMonitorOn;
public bool IsAudioMonitorOn
{
    get => _isAudioMonitorOn;
    set
    {
        SetProperty(ref _isAudioMonitorOn, value);
        var session = _session;
        if (session != null)
        {
            session.MonitorAudio = value;
        }
    }
}
```

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs
_session = new WebcamSession(camera.Device);
_session.FrameReceived += OnFrameReceived;
_session.MonitorAudio = IsAudioMonitorOn;
_session.Start();
IsMicAvailable = _session.IsAudioCaptureActive;
```

```xml
<!-- From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.UI/Views/MainPage.xaml -->
<CheckBox Content="Monitor audio" VerticalAlignment="Center"
          IsChecked="{d:Binding IsAudioMonitorOn, Mode=TwoWay}"
          IsEnabled="{d:Binding IsMicAvailable}" />
```

**Where to look.**
`WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs`
`WebcamViewer/src/WebcamViewer.UI/Views/MainPage.xaml`

**Sharp edges.**
- The ordering around `Start()` is not interchangeable: the setting is applied
  before the session starts, and the capability is read after, because a session
  cannot say whether it has audio until it has tried to open it.
- Read the session field into a local before using it. A teardown on another code
  path can null it between the null check and the assignment.
- Two properties, two jobs. The capability is private-set and drives enablement;
  the preference is public-set and drives the tick. Keeping them apart is what
  lets a device with no microphone gray the box without discarding the user's
  preference.
- `Mode=TwoWay` is not the default for `IsChecked`; without it the box moves on
  screen and the view model never hears about it.
- Off at startup is the right default for anything that feeds a camera's
  microphone back through the same room's speakers.
- Nothing here persists the preference. Adding a settings store is the natural
  next step; see the settings area.

### Validate a typed folder path inside CanExecute

**When you want this.** The destination of an action can be typed as well as
picked, and the button that uses it should be dead until the destination is real
- with no converter, no validation framework and no error decoration on the box.
Where the gate is explained to the user with a dialog and the location can only
come from a picker, see
[Gate an action behind a chosen folder and explain the gate with a dialog](BLUEPRINTS-MVVM.md#gate-an-action-behind-a-chosen-folder-and-explain-the-gate-with-a-dialog);
this is the quieter arrangement, where the check is a predicate the command
already calls and a dead button is the whole message.

**The MVVM shape.** A private static predicate over the string sits beside the
command and is called from its `CanExecute`, alongside the other gates. Every
property the predicate reads carries `[AffectsCommands]` naming that command, so
nothing anywhere calls `RaiseCanExecuteChanged()`.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs
private static bool IsValidFolder(string path)
    => !String.IsNullOrWhiteSpace(path) && Directory.Exists(path.Trim());
```

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs
private bool _hasFrame;
[AffectsCommands(nameof(PhotoCommand))]
public bool HasFrame
{
    get => _hasFrame;
    private set => SetProperty(ref _hasFrame, value);
}

// ...

private string _folderPath = string.Empty;
[AffectsCommands(nameof(PhotoCommand))]
public string FolderPath
{
    get => _folderPath;
    set => SetProperty(ref _folderPath, value ?? string.Empty);
}

private bool _isBusy;
[AffectsCommands(nameof(PhotoCommand), nameof(BrowseFolderCommand))]
public bool IsBusy
{
    get => _isBusy;
    set => SetProperty(ref _isBusy, value);
}
```

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs
private SimpleCommand _photoCommand;
public SimpleCommand PhotoCommand =>
    (_photoCommand ??= new SimpleCommand(CanTakePhoto, DoTakePhoto));

private bool CanTakePhoto() => (!IsBusy) && HasFrame && IsValidFolder(FolderPath);
```

```xml
<!-- From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.UI/Views/MainPage.xaml -->
<TextBox Grid.Column="1" Text="{d:Binding FolderPath, Mode=TwoWay}"
         PlaceholderText="Where frame-photos get saved" VerticalAlignment="Center" />
<Button Grid.Column="2" Content="Browse…" Command="{d:Binding BrowseFolderCommand}"
        Margin="8,0,0,0" MinWidth="100" />
<Button Grid.Column="3" Content="Photo" Command="{d:Binding PhotoCommand}"
        Margin="8,0,0,0" MinWidth="100" Style="{ThemeResource AccentButtonStyle}" />
```

**Where to look.**
`WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs`
`WebcamViewer/src/WebcamViewer.UI/Views/MainPage.xaml`

**Sharp edges.**
- Three unrelated properties gate one command, and each names it in its own
  attribute. That, plus the predicate, is the entire enablement mechanism; there
  is not a line of code about it in the page.
- The attribute refreshes the command when the property changes, and a text box
  changes its bound property when it pushes its value to the source. Add
  `UpdateSourceTrigger=PropertyChanged` to the binding if the button should follow
  the typing rather than the commit.
- Touching the disk from a `CanExecute` predicate runs it every time enablement is
  re-evaluated. It is cheap enough here and honest about a folder that has
  disappeared; cache the answer if your check is expensive.
- The check proves the folder existed when the button was evaluated, not when the
  file is written. The write still needs its own try and catch.
- Trim in both places. The predicate trims before testing and the command trims
  before combining the path, because the same property can be typed into by hand.
- The busy flag gates both commands, so the browse button is dead while a file is
  being written.

### Guard an async void handler the platform calls

**When you want this.** A handler whose signature you do not choose - a page's
`Loaded`, a headless command model's `Activated`, a `FrameArrived`, a finished
event on a child view model - has to await something. `async void` is the only
shape that fits, and a failure inside one does not land in a task anybody holds:
it reaches the dispatcher as an unhandled exception and ends the application.

**The MVVM shape.** Every `async void` in these applications has the same shape: a
`try` around everything it does, and a `catch` that turns the failure into
whatever this application already uses to say something went wrong - a status
line, a log line, a report method. Where there is real work, it stays in an
awaitable method beside the handler, so the handler is a wrapper and nothing
else, and a test can await the work and see what the handler would swallow.

**Code.**

An event handler on a view model says in its own summary why the whole body is
wrapped:

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs
    /// <summary>
    /// Adds a finished conversion's output to the list. This is an event handler, so it is
    /// <c>async void</c> and has no caller to hand a failure back to: the whole body is wrapped so
    /// that nothing can escape into the dispatcher, and what <see cref="AddAsync" /> does not already
    /// turn into a sentence ends up in the status bar as one.
    /// </summary>
    private async void OnConversionFinished(object sender, ConversionOutcome outcome)
    {
        try
        {
            // ...
            await AddAsync(outcome.OutputPath, CancellationToken.None);
            StatusText = outcome.ToString();
        }
        catch (Exception exception)
        {
            StatusText = $"The conversion finished, but its result could not be listed: {exception.Message}";
        }
    }
```

A page's `Loaded` is the other common one. Here the failure is a graphics API that
would not start, and the handler deliberately falls through to the settling step
that copes with exactly that:

```csharp
// From CodeBrix.Samples/SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml.cs
    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        if (gpuCanvas != null) { return; }

        try
        {
            // ... build the GPU canvas and give it a moment to report ...
        }
        catch (Exception exception)
        {
            //Nothing may escape an async void handler. A graphics API that refused to start is not a
            //  crash here either: the settle below collapses the canvas that did not start, and the
            //  view model is told it is on the processor.
            Debug.WriteLine($"SimpleCbxVideoPlayer: the GPU canvas could not be started - {exception.Message}");
        }

        DispatcherQueue?.TryEnqueue(SettleVideoSurface);
    }
```

The guard does not have to live in the page. Where the page's `async void` handler
only awaits one view-model method, the view model is the better place for it,
because the view model is what knows how this application reports a failure:

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs
    public async Task InitializeAsync()
    {
        try
        {
            await StartUpAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Scope startup failed.");
            SetStatus("Could not start: " + ex.Message);
        }
    }
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs` and
`src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs` (`StartSmokeRun`)
`SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml.cs`
`PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Actions.cs` (one `OnActivated`
helper wrapping every asynchronous command handler, so each subscription in the
wiring block stays one line),
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
(a debounce whose own failure becomes the grid's caption)

**Sharp edges.**
- Catch `Exception`, not a shortlist. The point of the wrapper is that nothing
  escapes, and a handler that catches three types still ends the application on
  the fourth.
- Keep the real work in an awaitable method beside the handler. That is what lets
  a test await it, and it is what stops the wrapper growing a body of its own.
- A report can fail too - showing a dialog is one more UI operation - so guard the
  reporting, as Pinta.Brix's does. A failed report must not be worse than the
  failure it was reporting.
- `OperationCanceledException` usually deserves its own empty `catch` above the
  general one, so a superseded run is not reported as a fault.
- Where the handler's whole body is one call into a view model, put the guard in
  the view model instead. The page then has nothing to decide, and every head gets
  the same behavior without repeating it.

### Start work you cannot await through a helper that observes it

**When you want this.** The place that has to start asynchronous work cannot await
it: a bound property's setter, a synchronous model event, a constructor. A bare
`_ = SomethingAsync();` starts the work, but its failure goes nowhere at all - not
to a caller, not to the status line, not even to the debugger until the finalizer
notices.

**The MVVM shape.** Start the work through something whose only job is to observe
how it ends: either a small static helper the whole application shares, or a
private wrapper method beside the work it starts. The work itself stays an
ordinary `Task`-returning method, so a command or a test can still await it, and
the failure handler belongs to the caller, which is what knows where a message
about this particular work should go. This is the same instinct as
[Guard an async void handler the platform calls](#guard-an-async-void-handler-the-platform-calls);
the difference is that there the signature forced `async void` on you, while here
you are choosing to discard a task and have to answer for it.

**Code.**

NotionDocumentCreator makes the helper explicit, and documents when not to reach
for it:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/Helpers/BackgroundWork.cs
/// <summary>
/// Starts asynchronous work from a place that cannot await it - a bound property's setter, a
/// selection change - and observes the result, so the gesture that started the work never sees
/// an exception and nothing is left as an unobserved task. Prefer awaiting the work from a
/// command; reach for this only where the caller really has no way to await.
/// </summary>
public static class BackgroundWork
{
    public static void StartAndObserve(Func<Task> work, Action<Exception> onError = null)
    {
        if (work is null) { return; }
        _ = ObserveAsync(work, onError);
    }

    private static async Task ObserveAsync(Func<Task> work, Action<Exception> onError)
    {
        try
        {
            await work();
        }
        catch (Exception e)
        {
            //Reporting the failure is the caller's business; observing it is this method's.
            try { onError?.Invoke(e); }
            catch (Exception) { } //A failing error handler must not replace one unobserved fault with another
        }
    }
}
```

Each call site supplies the sentence that belongs to it:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
    /// <summary>Shows the preview pane for a tapped row.</summary>
    internal void ShowPreview(NotionPageNodeViewModel node)
    {
        if (node is null || node.IsPlaceholder) { return; }
        SelectedNode = node;

        //The selection has to take effect now, and the load cannot be awaited from here, so it
        //  is started through a helper that observes it instead of a bare discard.
        BackgroundWork.StartAndObserve(() => LoadPreviewForNodeAsync(node),
            e => InvokeOnMainThread(() => StatusText = $"Preview failed: {e.Message}"));
    }
```

Where there is only one such call, a private wrapper says the same thing without a
helper class. The constructor discards this one; the wrapper is what makes that
safe:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
    //The constructor starts this without awaiting it, so a failure has nowhere to surface:
    //it becomes the sidebar's caption rather than an unobserved task.
    private async Task ReloadCatalogAsync()
    {
        try
        {
            await ReloadCatalogCoreAsync();
        }
        catch (Exception ex)
        {
            IsCatalogLoading = false;
            BundleCountText = $"Could not read this folder: {ex.Message}";
        }
    }
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/Helpers/BackgroundWork.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/NotionPageNodeViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/LivePreviewManager.cs` (a `void`
entry point over `StartAsync`, with a `RunAndObserve` wrapper, because the command
model's event is synchronous),
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
(`RunSwitchEngineAsync`, the wrapper a bound picker's setter starts)

**Sharp edges.**
- Keep the started method awaitable. The wrapper is for the one caller that cannot
  wait; a command, a page-ready signal or a test should still be able to await the
  real thing and see what happened.
- The failure handler belongs to the caller, not to the helper. Only the caller
  knows whether this failure is a status line, a grid caption or a log entry.
- Guard the failure handler too. An error path that throws replaces one unobserved
  fault with another, and the second one is much harder to find.
- A helper that swallows silently when no handler is supplied is a deliberate
  choice, and it is the reason the summary tells you to prefer awaiting from a
  command. Reach for the helper where there is really no way to await.

### Start the first load when the page says it is ready

**When you want this.** The first thing a view model does after startup can raise
a dialog - an error report, a welcome message, a prompt. Started from the
constructor it can run before the page exists, and a dialog raised then has no
root to attach to and simply does not appear.

**The MVVM shape.** The page tells the view model when it is on screen, and the
view model does its first load then. Two shapes appear here, and both are
one-directional: the page calls, the view model decides. A view model with one
such load exposes an idempotent `OnPageReady()` the page calls from `Loaded`; a
view model whose startup task is already running exposes a readiness signal the
task awaits partway through, so the work before the dialog still starts
immediately.

**Code.**

The call form. The page's loaded handler is one line, and the method it calls
guards itself so a page that loads twice does not load twice:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs
        //Anything the view model opens can fail, and an error dialog needs the XamlRoot that only
        //  a loaded page has, so the startup documents are opened from here
        Loaded += (_, _) =>
        {
            WireViewModel();
            ViewModel?.OnPageReady();
        };
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    /// <summary>
    /// Called by the page once it is loaded, its <c>XamlRoot</c> getter is in place and its file
    /// bridge is wired: opens the two documents named on the command line, if there are any. Later
    /// calls do nothing, so the page can call it from every load.
    /// </summary>
    public void OnPageReady()
    {
        if (_isPageReady) { return; }
        _isPageReady = true;

        //Discarded deliberately: every failure is caught and reported inside
        _ = OpenStartupDocumentsAsync();
    }
```

The signal form. A `TaskCompletionSource` declared beside the startup task lets the
work that needs no page run at once and the dialog wait:

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
/// <summary>
/// Lets the hosting page tell the view model that its UI is on screen and can host a dialog. Each
/// head calls <see cref="NotifyPageReady"/> from its page's loaded event, and that is what releases
/// the startup dialog; a head that never calls it simply never shows that dialog.
/// </summary>
public interface IPageReadyNotifier
{
    /// <summary>Tells the view model that the page is loaded and can host a dialog.</summary>
    void NotifyPageReady();
}

// ...

    private readonly TaskCompletionSource _pageReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
    // ...
        //A dialog needs a UI anchor that does not exist until the page has been laid out, so wait
        //  for the page to say that it is ready instead of guessing how long that takes.
        await _pageReady.Task;
    // ...
    public void NotifyPageReady() => _pageReady.TrySetResult();
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml.cs
        //The view model waits for this before it shows its startup dialog: a dialog needs a XamlRoot,
        //  and the page does not have one until it is on screen.
        Loaded += (sender, args) => (DataContext as IPageReadyNotifier)?.NotifyPageReady();
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs` and
`src/PdfSideBySide.UI/Views/MainPage.xaml.cs`
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs` and
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- A fixed delay is not a readiness signal. It is either too short on a slow start
  or an idle wait on a fast one, and it is wrong on whichever head you did not
  test.
- Make the entry point idempotent. `Loaded` can be raised more than once, and a
  page that is navigated away from and back must not reload everything.
- Release the wait when the view model goes away, or a `Dispose()` before the page
  ever loads leaves the startup task parked forever. JustBetweenUs cancels the
  source in `Dispose()` and catches the cancellation where it awaits.
- The page reaches the view model through the readiness interface, not through the
  view model's own type. Every head then has the same one-line loaded handler,
  including heads that never call it - in which case the dialog simply never
  shows, which is a documented outcome rather than a hang.
- Work that needs no page should still start in the constructor. Waiting for
  readiness is for the part that touches a dialog, not for the whole startup.

### Cancel one lifetime token from Dispose so in-flight work stops

**When you want this.** The window is closing while a download, a decode or a
timer is still running, and you do not want shutdown to wait for the network, nor
a completion callback to arrive and write into a view model that is already torn
down.

**The MVVM shape.** One `CancellationTokenSource` per view model, created with the
view model and cancelled first thing in `Dispose()`. Every long call it makes takes
that token; every `catch` treats `OperationCanceledException` as an ordinary
shutdown rather than a failure. This is not the per-operation source that
[Run one render per pane with latest request wins cancellation](#run-one-render-per-pane-with-latest-request-wins-cancellation)
uses to supersede an older request: this one is never renewed, and its only
cancel is the last one.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
    //Cancels an in-flight download when the view model is disposed, so shutdown does not
    //wait for the network. Nothing else cancels: the sample buttons are disabled while busy.
    private readonly CancellationTokenSource _lifetime = new();
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
    public override void Dispose()
    {
        //Stop an in-flight download so shutdown does not wait for the network.
        _lifetime.Cancel();
        // ... dispose the commands, clear the invalidate delegate, release the painters ...
        _lifetime.Dispose();

        base.Dispose();
    }
```

Where the sources are per-operation rather than per-view-model, `Dispose()` still
has to reach them, and cancelling one that a later run already disposed is a case
worth handling rather than avoiding:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    private static void CancelAndDispose(ref CancellationTokenSource source)
    {
        var cancellation = source;
        source = null;
        if (cancellation == null) { return; }

        try
        {
            cancellation.Cancel();
        }
        catch (ObjectDisposedException)
        {
            //Already disposed as the "previous" of a later render
        }
        cancellation.Dispose();
    }
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs` (`_pageReady.TrySetCanceled()`
releases a startup task still waiting for the page),
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs` (the
search debounce's source, nulled and then disposed)

**Sharp edges.**
- Cancel before anything else in `Dispose()`, and dispose the source after the
  rest of the teardown. In between, work that was already inside a call can wake
  up, see the cancellation and leave without touching a field that is now null.
- Null the field before disposing whatever it holds, so a callback arriving
  mid-teardown finds null rather than a disposed object.
- Catch `OperationCanceledException` on its own and say nothing. A window closing
  should not leave an error on a status line, and it certainly should not raise a
  dialog on a page that is going away.
- One token for the view model's own lifetime, separate sources for
  latest-request-wins. Mixing the two gives you a token that is cancelled for two
  different reasons and a shutdown that cannot tell them apart.

### Dispose only the service the view model built itself

**When you want this.** A view model resolves a service if one is registered and
constructs its own otherwise, and that service holds something real - a connection
pool, a handle, a cache. Disposing it unconditionally breaks every other consumer
of a registered singleton; never disposing it leaks whatever the fallback opened.

**The MVVM shape.** Record the ownership at the moment of the decision, in a
`readonly bool` beside the service field, and let `Dispose()` read it. The rule
the whole repository follows - a container singleton is released, not disposed - is
then written into the code rather than remembered.

**Code.**

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
    private readonly IGitHubIssueSearchService _searchService;
    private readonly bool _ownsSearchService;
```

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
        _searchService = GetService<IGitHubIssueSearchService>();
        if (_searchService == null)
        {
            //Nothing registered the service, so the view model builds its own and is then the
            //thing that has to close its connection pool when it goes.
            _searchService = new GitHubIssueSearchService(new GitHubSearchOptions());
            _ownsSearchService = true;
        }
```

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
            //Only the instance this view model built itself; a registered one belongs to the
            //container that handed it over and is shared with whatever else resolved it.
            if (_ownsSearchService && _searchService is IDisposable ownService)
            {
                ownService.Dispose();
            }
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs` (the same
question about an object rather than a service: the view model creates every
playback source it hands to the element, so it publishes the new one, disposes the
one it replaced, and releases the last in `Dispose()`),
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs` (a renderer
resolved with a `?? new` fallback and disposed because it owns a page cache)

**Sharp edges.**
- Decide ownership where the object is obtained, not where it is disposed. A
  `Dispose()` that tries to work out what it owns is guessing, and it will guess
  differently after the next refactor.
- `?? new` is convenient and it changes who owns the result. Every use of that
  pattern needs an answer to "and who disposes it".
- Publish the replacement before disposing the old one when a control is bound to
  it. The binding then moves off the old object before it becomes invalid.
- A registered singleton may be shared with a section of the application you are
  not looking at. Releasing the reference is the whole of the view model's duty
  there; the container owns the lifetime.

### Dispose a view model the XAML declared from the page Unloaded

**When you want this.** The page declares its view model in `<Page.DataContext>`,
so nothing else has a reference to it - and the view model holds a camera, a
tracking thread, GPU painters or the page's own bridge delegates. Nobody calls
`Dispose()` unless the page does.

**The MVVM shape.** The page's constructor subscribes `Unloaded` and disposes its
data context through `IDisposable`, never through the view model's own type, so
the line is the same on every head and says nothing about what is being released.
The view model's `Dispose()` is the one place that knows.
[Dispose a view model its commands and its bridge delegates](#dispose-a-view-model-its-commands-and-its-bridge-delegates)
is about what that method does; this is about who calls it.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs
        //Nothing else owns the view model - the XAML declares it - so the page is what runs its
        //  teardown: the camera stopped, the tracking thread joined, the bridge delegates dropped
        Unloaded += (_, _) => (DataContext as IDisposable)?.Dispose();
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
        //The XAML creates the view model and nothing else owns it, so the page is what
        //  releases its painters, rendering engines and commands.
        Unloaded += (_, _) => (DataContext as IDisposable)?.Dispose();
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- This is the answer for a single-page application, where unloading the page means
  the application is going. A page that is navigated away from and back would get
  a disposed view model on the way back, so an application with navigation needs
  the view model's lifetime to belong to whatever owns the navigation instead.
- Cast to `IDisposable`, not to the view model type. The page then does not name
  the view model in that line at all, which is the same discipline the bridge
  assignments in the handler above it follow.
- Where what has to be released is exclusive - a device handle, a lock - one
  unloaded handler is not enough, because a window can close without unloading its
  page on every head. See
  [Release an exclusive device handle from both the page unload and the window close](BLUEPRINTS-PlatformServices.md#release-an-exclusive-device-handle-from-both-the-page-unload-and-the-window-close).
- Make `Dispose()` safe to call twice. A page can be unloaded more than once, and
  a second teardown must be a no-op rather than a second `Stop()` on a native
  session.

### Fail a bound setter into the status line when a device refuses it

**When you want this.** A picker or a check box pushes its new value straight into
a device, a driver or a native session, and that push can be refused - a voltage
range this model does not have, a mode the hardware will not enter. An exception
thrown out of a property setter has nowhere to go: it leaves through the binding
engine, and the control has already moved.

**The MVVM shape.** The setter stores the value and calls one private method that
does the talking. That method catches the library's own exception type, writes the
refusal into the status line the application already uses, and re-reads from the
device whatever the failed attempt may have changed, rather than assuming the
state it was heading for. Where the refusal means the choice itself is impossible,
see
[Alert and revert when the user picks an unsupported option](#alert-and-revert-when-the-user-picks-an-unsupported-option);
this is the quieter case, where the value stands and the device simply says no.

**Code.**

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs
    /// <summary>The combo-box item for the input range applied to both channels.</summary>
    public VoltageRangeOption SelectedRangeOption
    {
        get => _selectedRangeOption;
        set
        {
            //A combo box clears its selection while its items are replaced;
            //  a null here is that transient, not a choice.
            if (value == null || ReferenceEquals(_selectedRangeOption, value)) { return; }
            SetProperty(ref _selectedRangeOption, value, notifyOnMainThread: true);
            ApplyChannelSettings();
        }
    }
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs
    private void ApplyChannelSettings()
    {
        if (_scope == null || !_scope.IsOpen) { return; }

        try
        {
            bool wasStreaming = _scope.IsStreaming;
            if (wasStreaming) { _scope.StopStreaming(); }

            foreach (ChannelId channel in _scope.Capabilities.SupportedChannels)
            {
                _scope.SetChannel(channel, new ChannelSettings(true, Coupling.Dc, SelectedRange));
            }

            if (wasStreaming) { DoStartStreaming(); }
        }
        catch (PicoScopeException ex)
        {
            //One of the two callers is the range picker's setter, so a range the
            //  device refuses must not throw out of a bound property. The stream
            //  may have been stopped before the refusal, so the flag the buttons
            //  read is re-read from the device rather than assumed.
            IsStreaming = _scope.IsStreaming;
            SetStatus("Could not apply the channel settings: " + ex.Message);
        }
    }
```

**Where to look.**
`PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Catch the library's own exception type, not everything. A device refusal is a
  sentence for the user; a null reference in your own code is a defect, and
  swallowing it into a status line hides it.
- Re-read the state the failure may have changed. Here the streaming flag drives
  the transport buttons, and the stream was stopped before the refusal, so
  assuming it is still running leaves two buttons lying.
- Guard the transient null. A combo box clears its selection while its item source
  is replaced, and treating that as a choice pushes a meaningless value at the
  device on every refresh.
- The same method is called from startup and from the setter. Writing it so it is
  safe with no device open (`_scope == null`) is what lets both callers share it.

### Switch devices off the UI thread and let the newest switch report

**When you want this.** A dropdown chooses which camera, scope or capture device
is live, and opening one takes long enough to freeze the window. The user can also
choose a third device while the second is still opening, and the one that finishes
last must not be the one that gets to write the status line.

**The MVVM shape.** The setter does two things only: clear the flags that describe
the old device, and start the switch with a version number it increments. The
switch runs the blocking open inside `Task.Run`, and every line after the await -
the status text, the repaint, even the error message - is guarded by comparing its
version against the current one. Where the open is quick enough to stay on the UI
thread, the simpler arrangement in
[Own one capture session in the view model and switch it from the selection setter](BLUEPRINTS-MediaAndVision.md#own-one-capture-session-in-the-view-model-and-switch-it-from-the-selection-setter)
is the one to copy; this recipe is what that shape turns into once opening a
device is slow enough to be seen.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
    private void SwitchCamera(CameraDevice camera)
    {
        //The setter only kicks the switch off: opening a device can take long enough to
        //  stall the UI thread, so the switch itself runs on a worker and the status line
        //  carries the result
        HasFrame = false;
        _ = SwitchCameraAsync(camera, ++_cameraSwitchVersion);
    }
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
    /// <summary>
    /// Starts (or stops) live capture off the UI thread - the capture service serializes its
    /// own start and stop, so two devices are never opened at once - and lets only the newest
    /// switch report: a camera that finishes opening late must not overwrite the status of
    /// the one the user has since chosen.
    /// </summary>
    private async Task SwitchCameraAsync(CameraDevice camera, int version)
    {
        try
        {
            var service = _captureService;
            if (service == null) { return; }

            await Task.Run(() =>
            {
                if (camera == null)
                {
                    service.Stop();
                }
                else
                {
                    service.Start(camera);
                }
            }).ConfigureAwait(false);

            if (version != _cameraSwitchVersion) { return; } //a newer switch took over

            if (camera == null)
            {
                //The page's delegate marshals the repaint itself
                InvalidatePreviewCanvas?.Invoke();
                return;
            }
            InvokeOnMainThread(() => StatusText = $"Live: {camera.FriendlyName}");
        }
        catch (Exception e)
        {
            if (version == _cameraSwitchVersion)
            {
                InvokeOnMainThread(() =>
                    StatusText = $"Could not start '{camera?.FriendlyName}': {e.Message}");
            }
        }
    }
```

**Where to look.**
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`
`PalmVisualizer/src/libs/PalmVisualizer.Camera/WebcamCaptureService.cs`

**Sharp edges.**
- The version is incremented in the argument itself, so the new value is both
  stored and carried by the switch it started. Two statements would leave a window
  where a switch carries a version that is no longer current.
- The `catch` checks the version too. A device that fails to open after the user
  has already chosen another one has nothing worth saying.
- Serializing the device calls is the service's job, not the view model's. The
  view model may well have two switches in flight; what must never happen is two
  devices being opened at once, and a lock inside the service is where that is
  guaranteed for every caller.
- Read the service field into a local before using it, so a teardown on another
  code path cannot null it between the check and the call.
- The setter clears the "we have a frame" flag before the switch starts, so a
  command gated on it cannot run against the device that is on its way out.
