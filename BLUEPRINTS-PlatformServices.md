# CodeBrix.Samples Blueprints: Bridging platform services into the view model

These recipes cover the seam between a view model and the capabilities only a
hosting page or a head can provide: dialogs that need a XamlRoot, native file
open and save pickers, the clipboard, canvas repaints, a repeating timer,
the mouse cursor, an embedded browser, an audio transport and the signal that
says the page is finally on screen. The shape is nearly always the same one -
the view model declares a small interface holding a delegate, implements it,
and the page fills the delegate in when the data context arrives - so one piece
of shared code runs both on a head that supplies the capability and on a head
that does not. The same interfaces carry work in the other direction too: a page
that forwards its paint call, or reads a fact back, through the contract it
already implements never has to name the view model's type. Several recipes also
cover what to do with what comes back, such as normalizing the path a picker
returns, deciding which side of the seam a picker's policy belongs on, or keeping
a single replace-file confirmation instead of two. One goes the other way and
hands the dialog shapes themselves back to the page, so an application with a
strong look keeps it, with a written-down answer for every delegate a head
leaves null. Reach for this file when a command needs something the view model
cannot do for itself, when the same feature has to work across several UI
stacks, or when a head with no windowing system must still start and explain
what it cannot do.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Give the view model a XamlRoot so its dialogs can show](#give-the-view-model-a-xamlroot-so-its-dialogs-can-show)
- [Save a file through a native dialog from the view model](#save-a-file-through-a-native-dialog-from-the-view-model)
- [Pick a file to open through a native dialog from the view model](#pick-a-file-to-open-through-a-native-dialog-from-the-view-model)
- [Clean up the path a file picker returns](#clean-up-the-path-a-file-picker-returns)
- [Suppress a native save dialog overwrite prompt so the view model owns confirmation](#suppress-a-native-save-dialog-overwrite-prompt-so-the-view-model-owns-confirmation)
- [Let the page invalidate a canvas through a bridge interface](#let-the-page-invalidate-a-canvas-through-a-bridge-interface)
- [Copy text to the clipboard from a command through a bridge interface](#copy-text-to-the-clipboard-from-a-command-through-a-bridge-interface)
- [Open a URL in the default browser from a view model](#open-a-url-in-the-default-browser-from-a-view-model)
- [Put a platform service behind an interface with a no-op default](#put-a-platform-service-behind-an-interface-with-a-no-op-default)
- [Install UI dialogs into a headless model through handler delegates](#install-ui-dialogs-into-a-headless-model-through-handler-delegates)
- [Marshal a repeating timer into a headless model](#marshal-a-repeating-timer-into-a-headless-model)
- [Set the mouse cursor from a model owned interface](#set-the-mouse-cursor-from-a-model-owned-interface)
- [Veto a window close until unsaved work is handled](#veto-a-window-close-until-unsaved-work-is-handled)
- [Tell the user when graphics initialization failed](#tell-the-user-when-graphics-initialization-failed)
- [Show a WebView on every head and drive it from a command](#show-a-webview-on-every-head-and-drive-it-from-a-command)
- [Replay a finished audio clip with one button press](#replay-a-finished-audio-clip-with-one-button-press)
- [Keep an embedded interpreter on its own thread and post every call to it](#keep-an-embedded-interpreter-on-its-own-thread-and-post-every-call-to-it)
- [Inject the quit action so a guest exit cannot end the test host](#inject-the-quit-action-so-a-guest-exit-cannot-end-the-test-host)
- [Add your own commands to an embedded interpreter](#add-your-own-commands-to-an-embedded-interpreter)
- [Release an exclusive device handle from both the page unload and the window close](#release-an-exclusive-device-handle-from-both-the-page-unload-and-the-window-close)
- [Marshal a save dialog onto the UI thread from a command handler](#marshal-a-save-dialog-onto-the-ui-thread-from-a-command-handler)
- [Offer a typed path where a head has no folder dialog](#offer-a-typed-path-where-a-head-has-no-folder-dialog)
- [Assign every bridge through the interface that declares it](#assign-every-bridge-through-the-interface-that-declares-it)
- [Signal the view model when the page is on screen](#signal-the-view-model-when-the-page-is-on-screen)
- [Keep picker plumbing in the page and picker policy in the view model](#keep-picker-plumbing-in-the-page-and-picker-policy-in-the-view-model)
- [Build a head's native picker behind a registered service](#build-a-heads-native-picker-behind-a-registered-service)
- [Call the page's bridge from the setter that changed](#call-the-pages-bridge-from-the-setter-that-changed)
- [Send the paint call back to the view model through the canvas bridge](#send-the-paint-call-back-to-the-view-model-through-the-canvas-bridge)
- [Ask the page for dialogs so they keep the application's own styling](#ask-the-page-for-dialogs-so-they-keep-the-applications-own-styling)

## Related blueprints

- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - the view model side of these bridges: SimpleViewModel, SimpleCommand and the threading rules the delegates are invoked under
- [BLUEPRINTS-ViewsAndControls.md](BLUEPRINTS-ViewsAndControls.md) - the page code-behind that fills the delegates in, and the controls the bridges reach for
- [BLUEPRINTS-AppStructureAndStartup.md](BLUEPRINTS-AppStructureAndStartup.md) - head-level opt-ins these recipes point at, such as enabling a file picker on the framebuffer head or the entry-point requirements on Windows
- [BLUEPRINTS-GameEngine.md](BLUEPRINTS-GameEngine.md) - the same page-supplied bridge shape, used to hand an engine canvas to a view model

---

## Bridging platform services into the view model

### Give the view model a XamlRoot so its dialogs can show

**When you want this.** Your view model calls `ConfirmDialog`, `ShowInfo`,
`ShowError` or `CreateDialog`, and those need a `XamlRoot` that only the page has.

**The MVVM shape.** `SimpleViewModel` implements `IXamlRootGetter`. The page's one
job is to hand it a getter - not the value, a getter - as soon as the
`DataContext` is set. This is the smallest bridge in the family and every
application that shows a dialog needs it.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml.cs
using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;

namespace MediaPlayerDemo.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);
        };

        this.InitializeComponent(); //Leave this line last
    }
}
```

The same getter also serves platform services that need a root of their own:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
if (OffscreenGLContext.TryCreate(GetXamlRoot(), out var glContext))
{
    // ... render the product shots ...
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
_modelPainter = new ModelScenePainter(_engineSelector.Create(RenderEngineKind.OpenGL, GetXamlRoot));
```

A native head satisfies the same interface with whatever its own dialog API
anchors to:

```csharp
// From CodeBrix.Samples/JustBetweenUs/Mobile/Views/MainPage.xaml.cs
(BindingContext as IXamlRootGetter)?.SetXamlRootGetter(() => this);
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`
`JustBetweenUs/Mobile/Views/MainPage.xaml.cs`

**Also shown by.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs`,
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml.cs`,
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs` and
`PainDiagram/PainDiagram.WinUI/Views/MainPage.xaml.cs`,
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs`,
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs`,
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`,
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs`,
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs`
(`(DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot)` wired from
`DataContextChanged`, in an application that then routes its own dialogs through
a bridge so it can style them - the wiring is there so the `SimpleViewModel`
helpers would work)

**Sharp edges.**
- A lambda, not the value. The page's `XamlRoot` is null until the page is in the
  visual tree, so the view model has to re-read it at the moment it needs it.
- The wiring goes in `DataContextChanged`, subscribed before
  `InitializeComponent()`, because the XAML is what sets the `DataContext`. Most
  of these files carry the comment "Leave this line last" on
  `InitializeComponent()` for exactly that reason.
- The `as` cast plus `?.` is the graceful-degradation path: a page whose data
  context is something else, or a design-time data context, simply does nothing.
- A native WPF head skips this entirely - WPF has no `XamlRoot` - and its dialogs
  still work, so shared view-model code must not assume the getter was supplied.
- Wire it even in an application that has no dialogs yet. It costs one line, and
  CodeBrixVideoTool and MediaPlayerDemo both do it before they need it.

### Save a file through a native dialog from the view model

**When you want this.** A command needs a destination path from a "save as"
dialog, and the application must still work on a head that has none.

**The MVVM shape.** The view model declares a small interface holding one delegate
the page fills in, and implements it itself. The command supplies a suggested file
name, treats a null or blank result as a cancel, and handles two separate "no
dialog" signals: a null delegate (the head never wired one) and a
`NotSupportedException` (the head wired one but the platform refuses). The page
implements the picker in a few lines inside its `DataContextChanged` handler, and
hands back a normalized path and nothing else: what that path then means to the
application is the view model's business, which is why the empty-placeholder rule
below sits in the command rather than in the page (see
[Keep picker plumbing in the page and picker policy in the view model](BLUEPRINTS-PlatformServices.md#keep-picker-plumbing-in-the-page-and-picker-policy-in-the-view-model)).

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Lets the hosting page give the view model a native "Save PDF as…" file dialog. Each head
/// wires this up with the file dialog appropriate to its UI stack (the CodeBrix.Platform
/// <c>FileSavePicker</c> on the Skia heads).
/// </summary>
public interface IFileSaveBridge
{
    /// <summary>
    /// Shows a "save PDF" dialog seeded with suggestedFileName and returns the
    /// full path the user chose, or <c>null</c> if they cancelled. The head leaves this null when
    /// it has no file dialog, in which case the user types the path directly into the box.
    /// Signature: <c>Func&lt;suggestedFileName, Task&lt;chosenPathOrNull&gt;&gt;</c>.
    /// </summary>
    Func<string, Task<string>> PickSavePdfPathAsync { get; set; }
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
private async Task DoSelectOutputFile()
{
    if (!CanSelectOutputFile()) { return; }

    if (PickSavePdfPathAsync == null)
    {
        //No native file dialog on this head — the user types the destination
        //  path directly into the box instead.
        await ShowInfo(
            "This head has no file dialog. Type the full path (including the .pdf file name) " +
            "for the PDF into the “Save PDF to” box.");
        return;
    }

    try
    {
        var chosenPath = await PickSavePdfPathAsync(GetSuggestedFileName());
        if (!string.IsNullOrWhiteSpace(chosenPath))
        {
            var destination = chosenPath.Trim();

            //Deciding that a brand-new, still-empty file the picker created is a destination
            //  rather than a document is this application's policy, so it lives here and not
            //  in the page: the create-time "replace existing file?" prompt should fire only
            //  for a file that really has content in it.
            FileDialogHelper.RemoveEmptyPlaceholder(destination);

            OutputFilePath = destination;
            StatusText = $"Will save to: {OutputFilePath}";
        }
    }
    catch (NotSupportedException)
    {
        //Some heads register no picker — there is no window to host a dialog
        await ShowInfo(
            "File dialogs are not supported on this head. Type the full path (including the " +
            ".pdf file name) for the PDF into the “Save PDF to” box.");
    }
    catch (Exception e)
    {
        await ShowError($"Could not open the file dialog: {e.Message}");
    }
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml.cs
public MainPage()
{
    //Doing this before InitializeComponent() - in case InitializeComponent()
    //  is the thing that sets the data context.
    DataContextChanged += (_, _) =>
    {
        //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
        (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

        //Give the view model a native "Save PDF as…" file dialog (CodeBrix.Platform's
        //  FileSavePicker). Heads with no windowing system throw NotSupportedException
        //  from the picker; the view model handles that.
        if (DataContext is IFileSaveBridge fileSave)
        {
            fileSave.PickSavePdfPathAsync = PickSavePdfPathAsync;
        }
    };

    this.InitializeComponent(); //Leave this line last
}

private static async Task<string> PickSavePdfPathAsync(string suggestedFileName)
{
    var picker = new FileSavePicker
    {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        SuggestedFileName = suggestedFileName,
        DefaultFileExtension = ".pdf"
    };
    picker.FileTypeChoices.Add("PDF document", new List<string> { ".pdf" });

    var file = await picker.PickSaveFileAsync();
    if (file == null) { return null; }

    //Some heads percent-encode the path they return, which would save "My Book.pdf" as
    //  "My%20Book.pdf"; decode it before anything touches the disk. That is the only thing
    //  done to it here: what the chosen path then means is the view model's business.
    return FileDialogHelper.ToFileSystemPath(file.Path);
}
```

The view model computes the suggested name from its own state and sanitizes it:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
/// <summary>A sensible default PDF file name: the first checked page's title.</summary>
private string GetSuggestedFileName()
{
    var name = Flatten().FirstOrDefault(n => !n.IsPlaceholder && n.IsChecked)?.Title;
    if (string.IsNullOrWhiteSpace(name)) { name = "NotionBook"; }

    var invalid = Path.GetInvalidFileNameChars();
    var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
    return (cleaned.Length == 0 ? "NotionBook" : cleaned) + ".pdf";
}
```

**Variant: write somewhere sensible when there is no dialog at all.** Where an
application would rather write a file than refuse, the null-delegate branch picks
a path itself:

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
string outputPath;

if (PickSaveJpegPathAsync == null)
{
    //No native file dialog on this head (e.g. the Linux framebuffer head) -
    //  save to a default location instead
    outputPath = GetDefaultSavePath();
}
else
{
    outputPath = await PickSaveJpegPathAsync(GetSuggestedFileName());
    if (String.IsNullOrWhiteSpace(outputPath))
    {
        return; //the user cancelled the dialog
    }
    outputPath = outputPath.Trim();

    //Confirm before clobbering an existing file (the head's own overwrite
    //  prompt is suppressed so this is the single confirmation)
    if (File.Exists(outputPath))
    {
        var replace = await ConfirmDialog(
            $"A file already exists at:\n{outputPath}\n\nDo you want to replace it?",
            "Replace existing file?");
        if (!replace)
        {
            StatusText = "Save cancelled - the existing file was kept.";
            return;
        }
    }
}

IsBusy = true;

var jpeg = _paintSession.ExportJpeg();
await File.WriteAllBytesAsync(outputPath, jpeg);
```

**Variant: a bridge that also carries the extension, and writes beside the
source.** CodeBrixVideoTool's bridge takes both a suggested name and an
extension, and when it is absent the view model writes next to the input file:

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/IOutputPathBridge.cs
public interface IOutputPathBridge
{
    /// <summary>
    /// Shows a "save as" dialog seeded with a suggested file name and returns the full path the
    /// person chose, or null if they cancelled. The head leaves this null when it has no file
    /// dialog, in which case the result is written beside the source instead.
    /// </summary>
    Func<string, string, Task<string>> PickOutputPathAsync { get; set; }
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs
var destination = SelectedDestination.Kind;
var suggested = ConversionPlanner.SuggestOutputFileName(Source, destination);
var extension = MediaFormats.Extension(destination);

string outputPath;
if (PickOutputPathAsync is null)
{
    outputPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Source.Path)) ?? ".", suggested);
}
else
{
    outputPath = await PickOutputPathAsync(suggested, extension);
    if (string.IsNullOrWhiteSpace(outputPath))
    {
        StatusText = "Cancelled - no destination was chosen.";
        return;
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
and `NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml.cs`
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/IOutputPathBridge.cs`

**Also shown by.**
`PainDiagram/Shared/ViewModels/MainViewModel.cs` and the three page code-behinds
that satisfy it (Skia, WinUI 3, WPF),
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs` and its four heads,
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs` (the
picker call kept in the view model behind a private static method, with
`NotSupportedException` caught specifically),
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs`
(`PickSavePathAsync(suggestedFileName, typeName, extension)` as a static method
on the page, filled into `IReadingFileBridge`; the view model composes the report
bytes before it asks where to put them, so a report that fails to compose never
shows a dialog at all)

**Sharp edges.**
- The page code-behind needs `using System;` for the awaiter extension that makes
  the picker awaitable. Several of these files carry a comment saying so, because
  the using looks unused and is easy to remove by mistake.
- Sanitize the suggested file name against `Path.GetInvalidFileNameChars()` before
  handing it to the picker.
- Set the busy flag only after the dialog closes, so the busy state does not
  disable the UI while a modal picker is open.
- Null the delegate in `Dispose()`, or the page stays alive through the view
  model.
- A delegate bridge is also trivially substitutable in a scripted run: the video
  tool's smoke path assigns `(_, _) => Task.FromResult(outputPath)`.
- Where two formats share an extension, put the difference in the suggested name
  so the two are distinguishable on disk.

### Pick a file to open through a native dialog from the view model

**When you want this.** A command has to ask a person which file to work with, and
only a head knows how to show a dialog.

**The MVVM shape.** The same bridge shape as saving: a one-member interface whose
member is a delegate the page fills in. The command checks for null first and says
so in the status line when a head cannot supply one, so an application that runs
where there is no windowing system still starts and still explains itself.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Services/IMediaFileBridge.cs
/// <summary>
/// The one thing the main view model cannot do for itself: ask a person which file to open. Only a
/// head knows how to show a file dialog, so the page fills this in.
/// </summary>
public interface IMediaFileBridge
{
    Func<Task<string>> PickMediaFileAsync { get; set; }
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs
public SimpleCommand OpenCommand => field ??= new SimpleCommand(
    () => !IsBusy, (Func<object, Task>)(_ => DoOpenAsync()));

private async Task DoOpenAsync()
{
    if (PickMediaFileAsync is null)
    {
        StatusText = "This head has no file dialog, so a file cannot be chosen by hand.";
        return;
    }

    var path = await PickMediaFileAsync();
    if (string.IsNullOrWhiteSpace(path))
    {
        return;
    }

    await AddAsync(path, CancellationToken.None);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
private static async Task<string> PickMediaFileAsync()
{
    try
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.VideosLibrary
        };

        foreach (var extension in MediaFormats.ImportExtensions)
        {
            picker.FileTypeFilter.Add(extension);
        }

        picker.FileTypeFilter.Add(".mkv");
        picker.FileTypeFilter.Add(".webm");
        picker.FileTypeFilter.Add(".cbv");

        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }
    catch (NotSupportedException)
    {
        //A head with no windowing system registers no picker extensions.
        return null;
    }
}
```

**A second application, the same shape.** PdfSideBySide declares its bridge in the
Core library beside the other service contracts, and the command asks for a file
only when a head has supplied one. A head that cannot show a dialog is a case the
view model answers, not an exception it catches:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/Services/IPdfFileBridge.cs
/// <summary>
/// The one thing the main view model cannot do for itself: ask a person which PDF to open. Only a
/// head knows how to show a file dialog, so the page fills this in when it takes the view model as
/// its data context.
/// </summary>
public interface IPdfFileBridge
{
    /// <summary>
    /// Shows an "open file" dialog filtered to PDF documents and returns the full path the person
    /// chose, or <c>null</c> when they cancelled. A head with no file dialog leaves this null, and
    /// the view model says so instead of browsing.
    /// </summary>
    Func<Task<string>> PickPdfPathAsync { get; set; }
}
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
private async Task BrowseAsync(DocumentSide side)
{
    if (IsBusy) { return; }
    if (PickPdfPathAsync == null)
    {
        //This head has no file dialog, so the command line is the only way in
        await ShowInfo("This head cannot browse for files. Start it with the two PDF file " +
            "paths on the command line instead.");
        return;
    }

    IsBusy = true;
    try
    {
        var path = await PickPdfPathAsync();
        if (path == null) { return; }

        var document = await _comparison.OpenAsync(side, path);
        // ...
    }
    // ...
}
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs
DataContextChanged += (_, _) =>
{
    //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
    (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

    //Give the view model the file dialog only a head can show
    if (DataContext is IPdfFileBridge fileBridge)
    {
        fileBridge.PickPdfPathAsync = PickPdfPathAsync;
    }

    WireViewModel();
};
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Services/IMediaFileBridge.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs`
`PdfSideBySide/src/PdfSideBySide.Core/Services/IPdfFileBridge.cs` and
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs`
`PdfSideBySide/src/PdfSideBySide.LinuxFrameBuffer/Program.cs`

**Also shown by.**
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs`
(`PickReadingPathAsync`, a `FileOpenPicker` filtered to `.json`, reached through a
settable `Func<Task<string?>>` that a head with no dialog simply leaves null)

**Sharp edges.**
- Two degradation points, not one: the delegate may be null (no head wired it) and
  the delegate may return null (no dialog, or the person cancelled). Treat them
  differently - the first deserves an explanation, the second is silent.
- A head with no windowing system registers no picker and
  `PickSingleFileAsync()` throws `NotSupportedException`; catch it in the page and
  return null rather than letting it reach the view model.
- `FileTypeFilter` takes extensions with the leading dot, and a filter list is
  only a first pass - candidates should still be validated after they are chosen.
- The pickers live in `Windows.Storage.Pickers`, which the library that carries
  CodeBrix.Platform already provides; no extra package is needed.
- The LinuxFrameBuffer head has to opt into an open picker on its host builder
  (`EnableFileOpenPicker(...)`); see the framebuffer blueprint in the startup
  area.

### Clean up the path a file picker returns

**When you want this.** A picker on one head hands back a percent-encoded
URI-shaped path, or creates an empty placeholder file at the chosen location, and
your application then behaves differently per head.

**The MVVM shape.** Two small static helpers in the shared library, called by
whichever head-side picker code needs them, so every head hands the view model the
same kind of plain file-system path and the same truthful answer to
`File.Exists()`.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/Helpers/FileDialogHelper.cs
/// <summary>
/// Turns the path a picker hands back into a real file-system path. The Linux Skia heads
/// build theirs out of the desktop portal's <c>file://</c> URI and leave it
/// percent-encoded, so a name with a space in it arrives as <c>My%20Book.pdf</c> and
/// would be written to disk under that literal name; accented names fare worse still
/// (<c>Ölberg</c> arrives as <c>%C3%96lberg</c>). Nothing is decoded unless the text
/// really does carry escapes, so paths from heads that already return a plain one — the
/// Win32 and WPF save dialogs — pass through untouched.
/// </summary>
public static string ToFileSystemPath(string path)
{
    if (string.IsNullOrWhiteSpace(path)) { return path; }

    //A head that hands back the whole URI rather than just its path.
    if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
        && Uri.TryCreate(path, UriKind.Absolute, out var uri)
        && uri.IsFile)
    {
        return uri.LocalPath;
    }

    return HasPercentEscape(path) ? Uri.UnescapeDataString(path) : path;
}

//True when the text holds at least one "%" followed by two hex digits. A literal percent
//  sign that is not the start of an escape (say "100% done.pdf") leaves the path alone.
private static bool HasPercentEscape(string text)
{
    for (var i = 0; i + 2 < text.Length; i++)
    {
        if (text[i] == '%' && Uri.IsHexDigit(text[i + 1]) && Uri.IsHexDigit(text[i + 2]))
        {
            return true;
        }
    }

    return false;
}
```

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/Helpers/FileDialogHelper.cs
/// <summary>
/// The WinRT <c>FileSavePicker</c> (Skia heads and native WinUI) creates an empty
/// placeholder file at the chosen path for a brand-new name. Remove it — but only when it
/// is genuinely empty — so a chosen path behaves like a pure destination and the app's own
/// "replace existing file?" prompt fires only for a real, non-empty file. A file that has
/// content is never deleted, so no user data is lost before the save-time confirmation.
/// </summary>
public static void RemoveEmptyPlaceholder(string path)
{
    if (string.IsNullOrWhiteSpace(path)) { return; }

    try
    {
        var info = new FileInfo(path);
        if (info.Exists && info.Length == 0)
        {
            info.Delete();
        }
    }
    catch
    {
        //Leave the file in place if it cannot be removed; the save-time overwrite
        //  prompt will simply ask about it.
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/Helpers/FileDialogHelper.cs`
`PainDiagram/Shared/Helpers/FileDialogHelper.cs` and
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Services/WinRtFileSavePicker.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Helpers/FileDialogHelper.cs` (the
folder picker needs the same decoding: a folder called "My Models" would otherwise
send every download to a literally named `My%20Models`),
`WikipediaPublisher/Shared/Helpers/FileDialogHelper.cs` (linked into the shared
library and the WinUI head only - the WPF head does not link it, because a WPF
`SaveFileDialog` already returns a plain path),
`WebcamPainter/src/WebcamPainter.Core/Helpers/FileDialogHelper.cs`,
`InannaRosette/src/InannaRosette.Core/Helpers/FileDialogHelper.cs`
(`ToFileSystemPath` unescapes only when the text really carries a `%` followed by
two hex digits, so a name like "100% done.pdf" is left alone and a plain path
from a Win32 dialog passes through untouched, and `RemoveEmptyPlaceholder`
deletes the picker's placeholder only when it is genuinely zero length)

**Sharp edges.**
- Decoding unconditionally would corrupt a legitimate name containing a percent
  sign, which is why the helper looks for a real `%XX` escape first.
- The placeholder is deleted only when its length is zero, so a real file is never
  lost before the application's own overwrite confirmation.
- Failure to delete is deliberately swallowed: the worst case is one extra
  confirmation prompt, never lost data.
- The two helpers belong at different depths. Decoding is plumbing: the page (or
  the picker service it resolves) does it, so the view model only ever sees real
  paths. Removing the placeholder is a decision about what a chosen path means,
  so applications whose command owns that question call it from the view model -
  see
  [Keep picker plumbing in the page and picker policy in the view model](BLUEPRINTS-PlatformServices.md#keep-picker-plumbing-in-the-page-and-picker-policy-in-the-view-model).

### Suppress a native save dialog overwrite prompt so the view model owns confirmation

**When you want this.** The user is asked twice whether to replace a file - once
by the save dialog and once by your application.

**The MVVM shape.** The bridge delegate is the seam. Each head configures its own
dialog to stay silent, and the single point of confirmation is a `SimpleDialog`
call in the view model's command, so the behavior is identical on every head. The
view model is unchanged.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs
var dialog = new Microsoft.Win32.SaveFileDialog
{
    Title = "Save PNG as",
    Filter = "PNG image (*.png)|*.png|All files (*.*)|*.*",
    DefaultExt = ".png",
    AddExtension = true,
    FileName = suggestedFileName,
    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
    OverwritePrompt = false   //The app does its own replace prompt via SimpleDialog
};
```

The WinRT picker cannot be told to stay quiet, so the WinUI 3 head drops to the
Win32 common item dialog through COM interop and clears the option itself:

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.WinUI/Views/MainPage.xaml.cs
if (DataContext is IFileSaveBridge fileSave)
{
    fileSave.PickSavePngPathAsync = (fileName) =>
    {
        //The Win32 dialog (rather than the WinRT FileSavePicker) so the un-suppressible
        //  WinRT overwrite prompt does not double up with the app's own confirmation
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.CurrentWindow);
        var path = Win32SaveFileDialog.PickSavePath(hwnd, fileName, "Save PNG as");
        return Task.FromResult(path);
    };
}
```

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.WinUI/Views/Win32SaveFileDialog.cs
public static string PickSavePath(IntPtr ownerHwnd, string suggestedFileName, string title)
{
    var dialog = (IFileDialog)new FileSaveDialog();
    try
    {
        //Start from the file system's real paths, and don't nag about overwriting.
        dialog.GetOptions(out var options);
        options |= FOS.FORCEFILESYSTEM;
        options &= ~FOS.OVERWRITEPROMPT;
        dialog.SetOptions(options);
        // ... filters, title, suggested file name, default folder

        const int cancelledHr = unchecked((int)0x800704C7); //HRESULT_FROM_WIN32(ERROR_CANCELLED)
        var hr = dialog.Show(ownerHwnd);
        if (hr == cancelledHr) { return null; }
        if (hr < 0) { Marshal.ThrowExceptionForHR(hr); }

        dialog.GetResult(out var item);
        try
        {
            item.GetDisplayName(SIGDN.FILESYSPATH, out var pathPtr);
            try { return Marshal.PtrToStringUni(pathPtr); }
            finally { Marshal.FreeCoTaskMem(pathPtr); }
        }
        finally
        {
            Marshal.ReleaseComObject(item);
        }
    }
    finally
    {
        Marshal.ReleaseComObject(dialog);
    }
}
```

**Where to look.**
`PainDiagram/PainDiagram.WinUI/Views/Win32SaveFileDialog.cs`
`PainDiagram/PainDiagram.WinUI/Views/MainPage.xaml.cs`
`PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs`

**Also shown by.**
`WikipediaPublisher/WikipediaPublisher.Wpf/Views/MainWindow.xaml.cs` and
`WikipediaPublisher/WikipediaPublisher.WinUI/Views/Win32SaveFileDialog.cs`

**Sharp edges.**
- The class documentation records both reasons for dropping to the Win32 dialog:
  the WinRT picker always shows its own replace confirmation with no way to turn
  it off, and it also creates an empty placeholder file. The Win32 dialog does
  neither.
- The dialog needs a window handle, so `App` exposes its main window as a static
  property purely so the page can ask for it.
- COM objects are released in `finally` blocks and the display-name pointer is
  freed explicitly.
- If you keep the WinRT picker, as the Skia heads do, pair it with the
  empty-placeholder cleanup from the previous blueprint instead.

### Let the page invalidate a canvas through a bridge interface

**When you want this.** Background work changes what should be drawn, and the view
model has to trigger a repaint without owning a control reference.

**The MVVM shape.** The view model declares a one-property interface holding an
`Action` (or one per canvas) and implements it. The page assigns a closure over
its own canvas when the `DataContext` arrives, and is responsible for marshalling
to the UI thread. The view model calls `?.Invoke()` from whichever thread it is
on, which is also the graceful-degradation path when no page has wired one.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/Bridges/ICanvasBridge.cs
/// <summary>
/// The main and self-view canvas contract between the hosting page and the view model, in
/// both directions. The page hands the view model the invalidate (repaint) delegates for the
/// two Skia canvases; frames and tracking results arrive on capture/worker threads, so the
/// page's delegates are responsible for marshalling their invalidates onto the UI thread.
/// The other direction is <see cref="RenderMainCanvas"/>: the page's paint handler forwards
/// the surface, so what the main canvas shows stays the view model's decision.
/// </summary>
public interface ICanvasBridge
{
    /// <summary>Invalidates the main canvas (live preview in Capture Mode; the painting in Paint Mode).</summary>
    Action InvalidateMainCanvas { get; set; }

    /// <summary>Invalidates the small self-view canvas shown beside the painting in Paint Mode.</summary>
    Action InvalidateSelfView { get; set; }

    // ... RenderMainCanvas, the return leg, has its own recipe
}
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs
DataContextChanged += (_, _) =>
{
    (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

    if (DataContext is IFileSaveBridge fileSave)
    {
        fileSave.PickSaveJpegPathAsync = PickSaveJpegPathAsync;
    }

    if (DataContext is ICanvasBridge canvasBridge)
    {
        //Frames and tracking results arrive on capture/worker threads - marshal
        //  the repaints onto the UI thread
        canvasBridge.InvalidateMainCanvas = () => DispatcherQueue?.TryEnqueue(() => MainCanvas?.Invalidate());
        canvasBridge.InvalidateSelfView = () => DispatcherQueue?.TryEnqueue(() => SelfViewCanvas?.Invalidate());
    }
};

//Nothing else owns the view model - the XAML declares it - so the page is what runs its
//  teardown: the camera stopped, the tracking thread joined, the bridge delegates dropped
Unloaded += (_, _) => (DataContext as IDisposable)?.Dispose();

InitializeComponent();
```

The bridge interfaces live in a `Bridges/` folder of the Core library rather than
in the view model's own file, which is what lets a second view model - or a test -
satisfy the same contract.

Where a library raises its own "I changed, repaint me" event, the view model
subscribes once and forwards, with no timer and no per-frame polling anywhere:

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
public interface ICanvasInvalidator
{
    /// <summary>Invalidates the hosting page's drawing canvas (null before the page wires it up).</summary>
    Action InvalidateCanvas { get; set; }
}

// ... in the constructor:
_session.RedrawRequested += (_, _) => InvalidateCanvas?.Invoke();
_session.DrawingChanged += (_, _) => InvokeOnMainThread(() => HasDrawing = _session.HasStrokes);
```

A native WPF head has to marshal differently, which is exactly why the bridge is a
delegate rather than a method the view model calls:

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs
private void InvalidateDrawCanvas()
{
    if (DrawCanvas.Dispatcher.CheckAccess())
    {
        DrawCanvas.InvalidateVisual();
    }
    else
    {
        DrawCanvas.Dispatcher.BeginInvoke(DrawCanvas.InvalidateVisual);
    }
}
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/Bridges/ICanvasBridge.cs` and
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs`
`PainDiagram/Shared/ViewModels/MainViewModel.cs` and
`PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs`

**Also shown by.**
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs` and
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
and `PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`
(where the page assigns its own coalescing `RequestRender` method rather than a
raw invalidate)

**Sharp edges.**
- The null-conditional chain inside the delegate
  (`DispatcherQueue?.TryEnqueue(() => Canvas?.Invalidate())`) matters: these
  delegates can fire while the page is being torn down.
- Two kinds of event deserve two treatments. A cheap repaint request can invoke
  the delegate directly and let the delegate marshal; an event that writes a bound
  property goes through `InvokeOnMainThread` in the view model.
- The view model nulls every delegate in `Dispose()`, which is what breaks the
  page-to-view-model reference cycle.
- Call it from the `finally` of a load path too, so a failure still repaints.

### Copy text to the clipboard from a command through a bridge interface

**When you want this.** A capability only the head can provide, needed by a
command, on heads that do not all support it.

**The MVVM shape.** The view model declares a tiny interface with a settable
delegate and implements it. The command checks whether the delegate was supplied:
if it was, it invokes it on the main thread; if it was not, it tells the user the
feature is not available on this platform. Each head's page assigns the delegate
in one place, using its own clipboard API.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
public interface ICopyToClipboard { Action<string> CopyTextToClipboard { get; set; }}

// ...

public class MainViewModel : SimpleViewModel, ICopyToClipboard, IPageReadyNotifier
{
    // ...
    private async Task DoCopyToClipboard()
    {
        if (CanCopyToClipboard())
        {
            if (CopyTextToClipboard != null)
            {
                InvokeOnMainThread(() => CopyTextToClipboard(ProcessedText));
                if (!_copyMessageShown)
                {
                    _copyMessageShown = true;
                    await ShowInfo("The processed text has been copied to the system clipboard.");
                }
            }
            else
            {
                await ShowError(
                    "This platform implementation does not have the Copy-to-clipboard functionality enabled.");
            }
        }
    }

    public Action<string> CopyTextToClipboard { get; set; }
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml.cs
public MainPage()
{
    //Doing this before InitializeComponent() - in case InitializeComponent()
    //  is the thing that sets the data context.
    DataContextChanged += (sender, args) =>
    {
        (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

        if (DataContext is ICopyToClipboard copy)
        {
            copy.CopyTextToClipboard = (text) =>
            {
                if (!string.IsNullOrEmpty(text))
                {
                    var clipData = new DataPackage();
                    clipData.SetText(text);
                    Clipboard.SetContent(clipData);
                }
            };
        }
    };

    //The view model waits for this before it shows its startup dialog: a dialog needs a XamlRoot,
    //  and the page does not have one until it is on screen.
    Loaded += (sender, args) => (DataContext as IPageReadyNotifier)?.NotifyPageReady();

    InitializeComponent();
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Wpf/Views/MainWindow.xaml.cs
DataContextChanged += (sender, args) =>
{
    if (DataContext is ICopyToClipboard copy)
    {
        copy.CopyTextToClipboard = Clipboard.SetText;
    }
};
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Mobile/Views/MainPage.xaml.cs
BindingContextChanged += (sender, args) =>
{
    (BindingContext as IXamlRootGetter)?.SetXamlRootGetter(() => this);

    if (BindingContext is ICopyToClipboard copy)
    {
        copy.CopyTextToClipboard = (text) =>
        {
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.Default.SetTextAsync(text); //Not necessary to await this
            }
        };
    }
};
```

**Where to look.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml.cs`
`JustBetweenUs/JustBetweenUs.WinUI/Views/MainPage.xaml.cs`
`JustBetweenUs/JustBetweenUs.Wpf/Views/MainWindow.xaml.cs`
`JustBetweenUs/Mobile/Views/MainPage.xaml.cs`

**Sharp edges.**
- The interface is declared in the view model's own file, not in a head assembly.
  That is what lets four unrelated UI stacks satisfy it.
- The wiring is done in the data-context-changed handler and subscribed before
  `InitializeComponent()`, because on some heads `InitializeComponent()` is what
  sets the data context.
- The graceful-degradation branch is the whole point of the null check: a head
  that supplies nothing still runs and tells the user why the button did nothing.
  Nothing throws.
- Three implementations use three different clipboard APIs, which is why the
  bridge is a delegate rather than a method the view model could call directly.
- One page satisfies as many of these contracts as it has work for. This one also
  implements the page-ready signal in the same constructor; see
  [Signal the view model when the page is on screen](BLUEPRINTS-PlatformServices.md#signal-the-view-model-when-the-page-is-on-screen).

### Open a URL in the default browser from a view model

**When you want this.** A row, a link or a button should open a web page in whatever
browser the user has, and the view model is where the click lands.

**The MVVM shape.** `Windows.System.Launcher.LaunchUriAsync` is available to the view
model directly - no bridge interface and no page involvement - but it needs wrapping,
because it can both return false and throw. One private method in the whole application
does it, and every list item reaches that method through a `Func<string, Task>` it was
handed, so no row or group holds a reference to the view model that made it.

**Code.**

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
//One place in the whole application asks the host to open a URL. A refusal is a status line,
//never an exception that reaches the user.
private async Task OpenUrlAsync(string url)
{
    if (string.IsNullOrWhiteSpace(url)) { return; }

    try
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            SetStatus($"That link could not be read: {url}", SearchStatusKind.Failed);
            return;
        }

        var opened = await Windows.System.Launcher.LaunchUriAsync(uri);
        if (!opened)
        {
            SetStatus("No browser was available to open that page.", SearchStatusKind.Failed);
        }
    }
    catch (Exception failure)
    {
        SetStatus($"That page could not be opened: {failure.Message}", SearchStatusKind.Failed);
    }
}
```

The item view models take the opener as a delegate and expose an ordinary command, so the
data template binds to the item's own `OpenCommand`:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/IssueRowViewModel.cs
/// <summary>
/// Opens this row's page in the host's browser. Living on the row keeps the template's
/// binding a plain one, because a template binds to its own item.
/// </summary>
public SimpleCommand OpenCommand => _openCommand ??=
    new SimpleCommand((Func<object, Task>)(_ => OpenAsync()));

// ...

private Task OpenAsync() => _openUrlAsync == null ? Task.CompletedTask : _openUrlAsync(Url);
```

The owner passes the method group when it builds each item:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
rows.Add(new IssueRowViewModel(item, palette, _showAssignees, now, OpenUrlAsync));
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/IssueRowViewModel.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/RepositoryGroupViewModel.cs`

**Also shown by.**
`InannaRosette/src/InannaRosette.Core/ViewModels/MainViewModel.cs`
(`DoOpenSavedReport` handles both of `LaunchUriAsync`'s failure modes - a false
return and a throw - in the one place the application asks the host to open
anything, and the button that runs it lives on a page-built dialog)

**Sharp edges.**
- `LaunchUriAsync` reports two kinds of failure. A false return means nothing was willing
  to open the address; an exception means the attempt itself failed. Handle both, and
  turn both into whatever your application uses to tell the user - here, the status line.
- Parse the address before handing it over. A malformed string is the common case when the
  URL came from an API, and `Uri.TryCreate` says so without an exception.
- Hand list items a delegate rather than a reference to their owner. An item that holds its
  owner keeps the whole view model alive for as long as the list does, and makes the item
  untestable on its own.
- Verify it on each head you ship. It opens the host's default handler, which is a
  different mechanism on each desktop.

### Put a platform service behind an interface with a no-op default

**When you want this.** A headless model wants to cut, copy and paste - or use any
other platform capability - but must run in tests and must not break on a head
where the capability is partial.

**The MVVM shape.** The model declares the interface and holds a null-object
implementation from the start, so every call site can be unconditional. The UI
layer installs the real one at startup.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/IClipboardService.cs
public interface IClipboardService
{
	void SetText (string text);

	Task<string?> GetTextAsync ();

	void SetImage (ImageSurface surface);

	Task<ImageSurface?> GetImageAsync ();
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/PintaCore.cs
/// <summary>
/// Installs the UI-layer clipboard implementation. Call once at startup.
/// </summary>
/// <remarks>
/// Until this is called the clipboard is a no-op that reports nothing
/// available, so engine code can call it unconditionally.
/// </remarks>
public static void InitializeClipboard (IClipboardService clipboard)
{
	Clipboard = clipboard;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PlatformServices.cs
public void SetImage (ImageSurface surface)
{
    // Encode as PNG and hand the platform a stream reference.
    // (Image WRITE is not yet supported by the X11 clipboard backend;
    // this degrades gracefully there.)
    using SKImage image = SKImage.FromBitmap (surface.Bitmap);
    using SKData data = image.Encode (SKEncodedImageFormat.Png, 100);

    InMemoryRandomAccessStream stream = new ();
    using (Stream outStream = stream.AsStreamForWrite ()) {
        data.SaveTo (outStream);
        outStream.Flush ();
    }
    stream.Seek (0);

    DataPackage package = new ();
    package.SetBitmap (RandomAccessStreamReference.CreateFromStream (stream));
    Clipboard.SetContent (package);
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/IClipboardService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/NullClipboardService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PlatformServices.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Clipboard image writing is not supported by every backend; the code notes it and
  degrades rather than throwing.
- Image transfer goes through an in-memory random-access stream holding the
  encoded bytes, seeked back to zero before the package is set.
- The reads are asynchronous and the writes are not, which is why the interface is
  asymmetric; keep the asymmetry rather than forcing a shape the platform does not
  have.

### Install UI dialogs into a headless model through handler delegates

**When you want this.** A library that must stay UI-free still needs to ask the
user something: an error, a confirmation, a configuration panel.

**The MVVM shape.** The model exposes `Initialize*` methods taking delegates; the
page - or, in a cleaner shape, the view model - installs them once it has a
`XamlRoot`. The model calls them without knowing what a dialog is.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ChromeManager.cs
public delegate Task<ErrorDialogResponse> ErrorDialogHandler (string message, string body, string details);
public delegate Task MessageDialogHandler (string message, string body);
public delegate Task<bool> SimpleEffectDialogHandler (BaseEffect effect, IWorkspaceService workspace);

public interface IProgressDialog
{
	void Show ();
	void Hide ();
	string Title { get; set; }
	string Text { get; set; }
	double Progress { get; set; }
	event EventHandler Canceled;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
//Chrome wiring: dialogs need a XamlRoot, so this happens on Loaded
PintaCore.Chrome.InitializeErrorDialogHandler(ShowErrorDialogAsync);
PintaCore.Chrome.InitializeMessageDialog(ShowMessageDialogAsync);
PintaCore.Chrome.InitializeProgessDialog(new ContentProgressDialog(() => XamlRoot));
//Custom effect dialogs route by effect type; everything else gets the
//reflection-generated dialog. Upstream's effects each opened their own
//Gtk dialog directly; here the Effects library stays UI-free, so the
//routing lives at this seam instead.
PintaCore.Chrome.InitializeSimpleEffectDialog(
    (effect, _) => effect switch
    {
        Effects.AlignObjectEffect align => Dialogs.AlignmentDialog.ShowAsync(align, XamlRoot),
        Effects.CurvesEffect curves => Dialogs.CurvesDialog.ShowAsync(curves, XamlRoot),
        Effects.LevelsEffect levels => Dialogs.LevelsDialog.ShowAsync(levels, XamlRoot),
        Effects.PosterizeEffect posterize => Dialogs.PosterizeDialog.ShowAsync(posterize, XamlRoot),
        _ => EffectOptionsDialog.ShowAsync(effect, XamlRoot),
    });
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Effects/Adjustments/PosterizeEffect.cs
public override Task<bool> LaunchConfiguration ()
{
	// Pinta.Brix note: upstream constructed the custom PosterizeDialog
	// directly; this library stays UI-free, so the dialog request goes
	// through the chrome seam and the UI layer routes it to the ported
	// PosterizeDialog by effect type.
	return chrome.LaunchSimpleEffectDialog (this, workspace);
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ChromeManager.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Effects/Adjustments/PosterizeEffect.cs`

**Sharp edges.**
- The wiring must happen on `Loaded`, not in the constructor: dialogs need a
  `XamlRoot` and there is none before then.
- The progress dialog takes a `Func<XamlRoot?>` rather than a `XamlRoot`, because
  it is constructed before the page has a root.
- The type-switch router is the one place that knows which items have bespoke
  dialogs; adding another is a one-line change there.

### Marshal a repeating timer into a headless model

**When you want this.** A library needs a periodic tick on the UI thread - a poll,
a progress update - but must not reference the dispatcher.

**The MVVM shape.** The model declares a one-method interface returning an
`IDisposable` handle; the UI layer implements it over the dispatcher queue's
timer. Until it is installed, a proxy forwards to nothing.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ITimerService.cs
public interface ITimerService
{
	/// <summary>
	/// Starts a repeating timer on the UI thread. The callback returns true
	/// to keep ticking or false to stop; disposing the returned handle also
	/// stops the timer.
	/// </summary>
	IDisposable Start (uint intervalMilliseconds, Func<bool> callback);
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PlatformServices.cs
public IDisposable Start (uint intervalMilliseconds, Func<bool> callback)
{
    Handle handle = new ();
    DispatcherQueueTimer timer = dispatcher.CreateTimer ();
    handle.Timer = timer;
    timer.Interval = TimeSpan.FromMilliseconds (intervalMilliseconds);
    timer.Tick += (_, _) => {
        if (!callback ())
            handle.Dispose ();
    };
    timer.Start ();
    return handle;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/NullServices.cs
/// <summary>
/// Forwards to the timer service the UI layer installs; before that, started
/// timers never tick.
/// </summary>
public sealed class TimerServiceProxy : ITimerService
{
	public ITimerService? Inner { get; set; }

	public IDisposable Start (uint intervalMilliseconds, Func<bool> callback)
		=> Inner?.Start (intervalMilliseconds, callback) ?? new NullHandle ();
}
```

The application installs the real one with the window's dispatcher queue:
`PintaCore.InitializeTimer(new DispatcherTimerService(MainWindow.DispatcherQueue))`.

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ITimerService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/NullServices.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PlatformServices.cs`
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`

**Sharp edges.**
- A proxy, not a null object, for the timer: the real implementation arrives after
  the model has already handed the proxy to other services, so those references
  must stay valid.
- The callback's `bool` return is the stop signal, and disposing the handle stops
  it too. Both paths matter, because callers use `using`.

### Set the mouse cursor from a model owned interface

**When you want this.** Your model decides which cursor is right - a tool, a hover
state, a drag - and the view must not hold that decision.

**The MVVM shape.** The model exposes a framework-free cursor descriptor; the view
maps it to the platform cursor in one switch. Unsupported descriptors degrade to
the closest available shape rather than failing.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs
public ToolCursor? Cursor {
    get => tool_cursor;
    set {
        tool_cursor = value;
        ProtectedCursor = InputSystemCursor.Create (MapCursor (value));
    }
}

private static InputSystemCursorShape MapCursor (ToolCursor? cursor)
{
    if (cursor is null)
        return InputSystemCursorShape.Arrow;

    // Icon/image cursors are approximated with a crosshair until custom
    // bitmap cursors are supported platform-side; tools also draw brush
    // outlines as canvas overlays, which carries most of the meaning.
    if (cursor.IconName is not null || cursor.Image is not null)
        return InputSystemCursorShape.Cross;

    return cursor.Shape switch {
        StandardCursor.Crosshair => InputSystemCursorShape.Cross,
        StandardCursor.Hand => InputSystemCursorShape.Hand,
        StandardCursor.Move => InputSystemCursorShape.SizeAll,
        StandardCursor.IBeam => InputSystemCursorShape.IBeam,
        StandardCursor.NotAllowed => InputSystemCursorShape.UniversalNo,
        StandardCursor.SizeNWSE => InputSystemCursorShape.SizeNorthwestSoutheast,
        // ...
        _ => InputSystemCursorShape.Arrow,
    };
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Input/ToolCursor.cs`

**Sharp edges.**
- `ProtectedCursor` is the seam on a `UIElement`; it is protected, so this only
  works from a subclass.
- Custom bitmap cursors are not available, so image-based cursors degrade to a
  crosshair. Plan for the degradation rather than assuming a bitmap cursor.

### Veto a window close until unsaved work is handled

**When you want this.** Your application holds unsaved documents and the window's
own close button is a way out.

**The MVVM shape.** The window's `Closed` event is the platform seam. The handler
vetoes the close, runs the async save-prompt loop, and re-issues the close when
the answer comes back. `App` owns the window and the page owns the prompt loop,
and neither may hold a reference to the other, so a registered service holds the
loop: the page hands it to the view model through a bridge, the view model
installs it on the service, and the window-close handler resolves the service.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
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
```

The service is the whole of its own registration, and holds nothing but the
delegate, so it is safe to resolve before any window or page exists:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.Core/Services/ShellCloseService.cs
public sealed class ShellCloseService : IShellCloseService
{
    /// <inheritdoc />
    public Func<Task<bool>> ConfirmCloseApplicationAsync { get; set; }

    /// <inheritdoc />
    public async Task<bool> ConfirmCloseAsync()
    {
        //Read once: the shell can replace the loop while a close is in flight.
        Func<Task<bool>> prompt = ConfirmCloseApplicationAsync;

        if (prompt == null) { return true; }

        return await prompt();
    }
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.Core/ViewModels/MainViewModel.cs
//App's window-close handler resolves this service and asks it whether
//the close may proceed; the page installs the prompt loop on the
//bridge below, and this is what forwards it.
IShellCloseService closeService = GetService<IShellCloseService>();
if (closeService != null)
{
    closeService.ConfirmCloseApplicationAsync = ConfirmCloseAsync;
}
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`
`Pinta.Brix/src/Pinta.Brix.Core/Services/IShellCloseService.cs` and
`ShellCloseService.cs`
`Pinta.Brix/src/Pinta.Brix.Core/Bridges/IShellCloseBridge.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Dialogs.cs`

**Sharp edges.**
- A re-entrancy guard flag is mandatory: the confirmed `Close()` re-raises
  `Closed`, and without the flag the prompt loops forever.
- The prompt is asynchronous while the event is not, hence the veto-then-reissue
  shape rather than awaiting inside the veto decision.
- Wrap the whole body so a prompt failure leaves the veto standing rather than
  losing the user's work.
- Not every head has window chrome. An application whose only exit is the window
  button has no exit path at all on the framebuffer head.
- Reaching the page from `App` through a static `Current` property is the shortcut
  to avoid: it pins one page instance for the life of the process and makes the
  close path untestable. A registered service that holds only a delegate costs one
  registration line and inverts the dependency.
- Read the delegate into a local before awaiting it. The shell can replace or drop
  its prompt loop while a close is in flight.

### Tell the user when graphics initialization failed

**When you want this.** A GL-backed pane can be empty on a machine with no usable
driver, and an empty pane looks like a bug. This is the graceful-degradation path
for a hardware capability.

**The MVVM shape.** The page asks the canvas for its initialization state - a view
concern - and hands the state object to a view-model method that owns the message
and the dialog. Platform detection inside the message comes from `SimpleOsInfo`,
not from a compile-time switch.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Shows a dialog explaining why the 3D preview cannot render. Called from the view when
/// the Model View is active and the preview's GLCanvasElement reports that its OpenGL
/// initialization failed (e.g. on systems without OpenGL 3.0+ support, where the preview
/// would otherwise just be an empty pane).
/// </summary>
public async Task ShowRenderingUnavailableAsync(GLInitializationState state)
{
    var message =
        "The interactive 3D model preview is not available on this system, so the preview " +
        "pane will stay empty.\n\n";

    //On Windows, the usual cause is a missing OpenGL driver; Microsoft's free "OpenCL and
    //OpenGL Compatibility Pack" adds one. Only show this hint when actually on Windows.
    var osInfo = await SimpleOsInfo.GatherInfo(withConsoleOutput: false);
    if (osInfo.IsWindows)
    {
        message +=
            "On Windows, you may be able to fix this by installing the free Microsoft " +
            "\"OpenCL and OpenGL Compatibility Pack\". Download and install it from:\n" +
            "https://apps.microsoft.com/detail/9NQPSL29BFFF\n\n" +
            "After installing it, restart this app.\n\n";
    }

    message += $"Details:\nStatus: {state.Status}\n{state.FailedReason ?? "(none reported)"}";

    using var dialog = CreateDialog(message, "3D Preview Unavailable");
    _ = await dialog.ShowAsync();
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
//The user just opened the Model View: if the GL canvas already knows its
//OpenGL initialization failed, tell them why the preview pane is empty.
modelViewBridge.ModelViewOpened = () => _ = MaybeReportRenderingUnavailableAsync();

// ...

//The canvas may only attempt its OpenGL initialization when it loads into the visual
//tree, which can happen after IsModelViewActive is set - so check at both moments.
ModelCanvas.Loaded += (_, _) => _ = MaybeReportRenderingUnavailableAsync();

//When the Model View is active and the preview canvas reports failed OpenGL initialization,
//surface the failure (status + reason) in a dialog instead of leaving a silently empty pane.
private async Task MaybeReportRenderingUnavailableAsync()
{
    if (_renderingUnavailableReported || ViewModel is not { IsModelViewActive: true } viewModel)
    {
        return;
    }

    var state = ModelCanvas.GetGLInitializationState();
    if (state.Status == GLInitializationStatus.InitializationFailed)
    {
        _renderingUnavailableReported = true;
        await viewModel.ShowRenderingUnavailableAsync(state);
    }
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs` and
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Check at two moments - the canvas's `Loaded` and the view's activation - because
  a collapsed canvas may not attempt initialization until it enters the visual
  tree. The activation half arrives through a bridge the view model invokes from
  the setter that switched views, not through a `PropertyChanged` name check; see
  [Call the page's bridge from the setter that changed](BLUEPRINTS-PlatformServices.md#call-the-pages-bridge-from-the-setter-that-changed).
- A page-level flag reports the failure once per run; without it the dialog
  reappears on every item the user opens.
- Decide the operating-system-specific hint with `SimpleOsInfo` rather than
  compiling it in, so the same message code runs on every head.

### Show a WebView on every head and drive it from a command

**When you want this.** Your application needs an embedded browser the user
navigates freely, and a command that sends it somewhere.

**The MVVM shape.** The view model declares a bridge with an `Action<string>` the
page sets, a method the page calls once its browser exists, and a method the page
calls whenever the browser lands on a new URL. The command builds the URL and one
private helper marshals every navigation onto the UI thread; the page does nothing
but forward. Where the browser opens first is the view model's decision too, which
is what the ready signal is for - the page never sets a start page of its own. The
view model checks the delegate for null before using it and never names a WebView
type.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
public interface IWebViewBridge
{
    /// <summary>Navigates the embedded browser to the given URL (null when no WebView).</summary>
    Action<string> NavigateToUrl { get; set; }

    /// <summary>
    /// Called by the page once its embedded browser exists and <see cref="NavigateToUrl"/> has
    /// been set, so the view model decides which page the browser opens on.
    /// </summary>
    void NotifyBrowserReady();

    /// <summary>Called by the page whenever the embedded browser lands on a new URL.</summary>
    void SetCurrentBrowserUrl(string url);
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
private Task DoSearch()
{
    //Every head has an embedded WebView: browse the real Wikipedia search page; the user
    //  picks an article by navigating to it, and Publish uses whatever page is displayed.
    if (CanSearch() && NavigateToUrl != null)
    {
        var searchUrl =
            $"https://{WikiHost}/w/index.php?search={Uri.EscapeDataString(SearchTerms.Trim())}";
        Navigate(searchUrl);
        StatusText = "Browse to the article you want, then click Publish.";
    }

    return Task.CompletedTask;
}

private void Navigate(string url)
{
    if (NavigateToUrl != null && (!string.IsNullOrWhiteSpace(url)))
    {
        InvokeOnMainThread(() => NavigateToUrl(url));
    }
}

public void NotifyBrowserReady()
{
    //The view model, not the page, decides where the browser starts, so every head opens
    //  on the same page and all navigation flows one way.
    Navigate(HomeUrl);
}

public void SetCurrentBrowserUrl(string url)
{
    if (string.IsNullOrWhiteSpace(url)) { return; }

    InvokeOnMainThread(() =>
    {
        ArticleUrl = url;
        StatusText = IsPublishableArticleUrl(url)
            ? "Ready to publish this article."
            : "Browse to an article page to enable publishing.";
    });
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs
private void InitializeBrowser()
{
    //Reached through the bridge interface, not the concrete view model type: the page needs
    //  nothing from the view model beyond the three members the browser contract names.
    if (_browserInitialized || DataContext is not IWebViewBridge browser) { return; }
    _browserInitialized = true;

    //Use CoreWebView2.Source (the authoritative current URL after redirects / user
    //  navigation); the XAML Browser.Source property does not reliably reflect those.
    Browser.NavigationCompleted += (_, _) =>
        browser.SetCurrentBrowserUrl(Browser.CoreWebView2?.Source ?? Browser.Source?.AbsoluteUri);

    browser.NavigateToUrl = url =>
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            Browser.Source = new Uri(url);
        }
    };

    //The view model owns the start page too, so every navigation flows the same way.
    browser.NotifyBrowserReady();
}
```

Every Skia head has a `WebView2` control to host: the Windows, Skia-on-WPF and
macOS runtimes have one built in, and the Linux heads get one from the
CodeBrix.Platform WebView add-in. The page's XAML declares it and nothing else:

```xml
<!-- From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml -->
<!-- ... -->
<WebView2 Grid.Row="1" x:Name="Browser" />
```

**Using the CodeBrix.Platform WebView add-in.** The Linux Skia heads have no
built-in browser; the add-in supplies one, and it is referenced once in the
library that carries the application's packages so every head inherits it:

```xml
<!-- From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.Core/WikipediaPublisher.Core.csproj -->
<!-- WebView add-in: gives the Linux Skia heads an embedded WebView2 (WPE WebKit,
     offscreen). Referenced once here in Core; every Skia head inherits it transitively.
     The Windows, Skia-on-WPF and macOS runtimes already have WebView2 built in, so the
     add-in is inert there. The Linux heads need the system WPE WebKit engine at run time:
     sudo apt install libwpewebkit-2.0-1 libwpebackend-fdo-1.0-1 libwpe-1.0-1 -->
```

**Where to look.**
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs`
and `Views/MainPage.xaml`
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.Core/WikipediaPublisher.Core.csproj`

**Also shown by.**
`WikipediaPublisher/WikipediaPublisher.WinUI/Views/MainPage.xaml.cs`,
`WikipediaPublisher/WikipediaPublisher.Wpf/Views/MainWindow.xaml.cs` (a different
WebView control satisfying the same interface)

**Sharp edges.**
- Read the current URL from the core browser object, not from the XAML `Source`
  property; all three head implementations carry the same comment saying the XAML
  property does not reliably reflect redirects or user navigation.
- The Skia head wires the browser in a `Loaded` handler behind a guard flag,
  because `Loaded` can fire more than once.
- Wire the delegate before raising the ready signal. The view model answers the
  signal by navigating, and a navigation with no delegate in place is silently
  lost.
- The system WPE WebKit engine is a run-time dependency, not a build one: the
  build succeeds on a machine that cannot run the WebView.
- Referencing the add-in once, in the shared library, is deliberate. It is inert
  where a WebView already exists, so one reference covers every head.
- On Windows the browser also constrains the head's entry point: see the
  synchronous-STA blueprint in the startup area.

### Replay a finished audio clip with one button press

**When you want this.** Your transport has a single Play button and the clip is
short. Without this, a clip that has run to its end does nothing when Play is
pressed again.

**The MVVM shape.** The page's bridge implementation is the natural home for the
element's own transport calls, but the policy - Play means replay when the clip
has finished, resume when the user has scrubbed - is application behavior and
belongs on the view model. So the bridge carries the transport facts back as
read-only delegates, plus a seek, and the whole decision is one method on the view
model. The page's only part in it is one line that forwards the player's
playback-ended event.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/IAudioPlayerBridge.cs
public interface IAudioPlayerBridge
{
    // ... LoadAudioSource, PlayAudio, PauseAudio, StopAudio, SetAudioLooping ...

    /// <summary>Whether the player is currently advancing.</summary>
    Func<bool> IsAudioPlaying { get; set; }

    /// <summary>The player's position within the clip.</summary>
    Func<TimeSpan> AudioPosition { get; set; }

    /// <summary>The loaded clip's duration.</summary>
    Func<TimeSpan> AudioDuration { get; set; }

    /// <summary>Moves the player to a position within the clip.</summary>
    Action<TimeSpan> SeekAudio { get; set; }
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
//Whether the current clip has run to its end. A finished clip leaves the transport parked
//at the end, where Play() has nothing left to play, so the next Play rewinds first.
private bool _audioPlaybackEnded;

//How close to the duration still counts as "parked at the end". The player refreshes its
//position on an interval (150 ms by default), so the last value it reports before ending
//can sit just short of the duration.
private static readonly TimeSpan AudioEndTolerance = TimeSpan.FromMilliseconds(250);

// ...

/// <summary>Starts (or resumes) audio playback; a clip parked at its end replays instead.</summary>
public SimpleCommand PlayAudioCommand => field ??= new SimpleCommand(CanUseAudioTransport, DoPlayAudio);

// ...

//Starts (or resumes) the audio clip. A clip that has played through to its end leaves the
//transport parked at the end, where Play() alone has nothing left to play - so rewind first
//and let one click replay the clip. Two things deliberately do NOT rewind: a player that is
//still going (a looping clip reports playback ended on every pass), and a clip the user has
//scrubbed away from the end since it finished - there, the thumb is the intent, so resume
//from where they left it.
private void DoPlayAudio()
{
    if (!CanUseAudioTransport()) { return; }

    if (_audioPlaybackEnded
        && IsAudioPlaying?.Invoke() == false
        && AudioDuration?.Invoke() > TimeSpan.Zero
        && AudioPosition?.Invoke() >= AudioDuration.Invoke() - AudioEndTolerance)
    {
        SeekAudio?.Invoke(TimeSpan.Zero);
    }

    _audioPlaybackEnded = false;
    PlayAudio?.Invoke();
}

// ...

public void NotifyAudioPlaybackEnded() => _audioPlaybackEnded = true;
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
//Audio bridge: the view model hands over the clip's raw stream and transport
//calls; the AudioPlayer element does the decoding and playing (it takes
//stream ownership). The last four members are the transport facts the view
//model's replay policy reads back, plus the seek it uses to rewind.
// ...
audioBridge.IsAudioPlaying = () => AudioElement?.IsPlaying ?? false;
audioBridge.AudioPosition = () => AudioElement?.Position ?? TimeSpan.Zero;
audioBridge.AudioDuration = () => AudioElement?.Duration ?? TimeSpan.Zero;
audioBridge.SeekAudio = position => AudioElement?.Seek(position);

// ... after InitializeComponent():

//A clip that plays through to its end parks the transport at the end; the view model
//remembers that so the next Play can rewind instead of doing nothing.
AudioElement.PlaybackEnded += (_, _) => ViewModel?.NotifyAudioPlaybackEnded();
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/IAudioPlayerBridge.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- A looping clip raises its playback-ended event on every pass while still
  playing, so the flag alone is not enough; the "is it playing" check is what
  stops a loop being rewound mid-play.
- The player refreshes its reported position on an interval, so the last position
  before the end can sit slightly short of the duration. A tolerance window is
  what makes the end-of-clip test reliable.
- Loading a new source and stopping both clear the flag.
- Facts the policy reads come back as `Func<T>` delegates rather than as bound
  properties, because they are read at the moment of the decision and never
  displayed. A transport fact that the UI does show - the scrubber position -
  would be a bound property instead.

### Keep an embedded interpreter on its own thread and post every call to it

**When you want this.** You are hosting a synchronous guest - an interpreter, a
script engine, a legacy toolkit - that expects to own the thread it runs on, and
it has to share a process with an asynchronous UI thread that owns the visual
tree.

**The MVVM shape.** One rule, enforced in one class: the guest lives on its own
thread, and the toolkit's hosted bridge marshals every one of the guest's UI calls
back to the UI thread. Application code never touches the guest from the UI thread
except through the bridge's post method, and never touches the visual tree from
the guest's thread at all. What the UI thread does contribute - the host element's
window tree - is captured on the UI thread before the background work starts.

**Code.**

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs
/// <summary>
/// Starts DRAKON in HOSTED mode inside the given host view — the way the
/// application runs it. Call once, from the UI thread, after the host has
/// loaded (its tree and dispatcher exist).
/// </summary>
/// <param name="host">The loaded Tk host view.</param>
public void Start(TkHostView host)
{
    if (host == null) { throw new ArgumentNullException(nameof(host)); }
    if (_started) { return; }
    _started = true;

    WindowTree tree = host.Tree;
    string assets = Path.Combine(AppContext.BaseDirectory, "Assets");

    Task.Run(() =>
    {
        try
        {
            if (!Boot(tree, hosted: true, code => Environment.Exit(code), new TkHostFileDialogs(), assets))
            {
                return;
            }
            RunHostedDiagnostics();
        }
        catch (Exception ex)
        {
            Report("startup exception: " + ex);
        }
    });
}
```

Everything the application later wants to run inside the guest goes through the
same post method, so it arrives on the guest's thread rather than the caller's:

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs
    // A generic diagnostic: when DRAKONBRIX_EVAL is set, its content is
    // evaluated as a Tcl script once the editor is up, and the completion
    // code/result are reported — lets any DRAKON flow be driven and observed
    // from the log without mouse input.
    string evalScript = Environment.GetEnvironmentVariable("DRAKONBRIX_EVAL");
    if (!String.IsNullOrEmpty(evalScript))
    {
        _bridge.Post(evalInterp =>
        {
            Result evalResult = null;
            ReturnCode evalCode = evalInterp.EvaluateScript(evalScript, ref evalResult);
            Report("eval(" + evalCode + "): " + evalResult);
        });
    }
```

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs
/// <summary>Stops the Tcl thread and disposes the interpreter.</summary>
public void Dispose()
{
    if (_bridge != null) { _bridge.Dispose(); }
    if (_interpreter != null) { _interpreter.Dispose(); }
}
```

**Where to look.**
`DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs`
`DRAKON.Brix/src/DRAKON.Brix.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Capture what the UI thread owns - here the host element's window tree - on the
  UI thread, before the background task starts. Reading it from inside the task is
  the failure this shape exists to prevent.
- The bridge's post is the only way in. An interpreter call made directly from the
  UI thread runs on the wrong thread and corrupts engine state that has no
  contract for concurrent access.
- Disposal order is the bridge first, then the engine: the bridge is what stops
  the guest's thread, and disposing the engine out from under a running thread is
  a teardown that will not reproduce on demand.
- Give the bridge's background-error event a sink at boot. An error raised inside
  the guest's own event loop has no caller, and without the subscription it is
  simply lost.
- Anything that must run after the guest is up belongs in a posted callback, not
  in a wait on the UI thread.

### Inject the quit action so a guest exit cannot end the test host

**When you want this.** The guest program you are hosting was written for a
command-line launcher, so its way of ending is to call the launcher's exit. Inside
a hosted application that either does nothing visible or, in a test run, takes the
test host down with it.

**The MVVM shape.** The host re-implements the guest's notion of "the program ends
now" as a command of its own, and injects the behavior rather than hard-coding it.
The application passes the process-ending action; a headless caller passes a no-op.
One command class, two callers, no conditional compilation and no test-only branch
inside the command.

**Code.**

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/Commands/QuitCommand.cs
/// <summary>
/// The <c>__drakonbrix_quit ?code?</c> Tcl command that ends the application,
/// the way real Tk's <c>exit</c> does. The engine's own <c>exit</c> only marks
/// the interpreter as exited; in a hosted app the UI thread keeps the process
/// alive, so DRAKON's File &gt; Quit (a bare <c>exit</c>) would otherwise hang.
/// bootstrap.tcl shadows <c>exit</c> with a proc that routes here.
/// <para>The actual "end the app" behaviour is injected: the hosted
/// application supplies <c>Environment.Exit</c>, while non-hosted callers such
/// as tests supply a safe no-op so a stray <c>exit</c> cannot tear down the
/// test host.</para>
/// </summary>
internal sealed class QuitCommand : Default
{
    private readonly Action<int> _onQuit;

    internal QuitCommand(Action<int> onQuit)
        : base(new CommandData(
            "__drakonbrix_quit", null, null, null,
            typeof(QuitCommand).FullName, CommandFlags.None, null, 0))
    {
        _onQuit = onQuit;
    }

    public override ReturnCode Execute(
        Interpreter interpreter, IClientData clientData, ArgumentList arguments, ref Result result)
    {
        int code = 0;
        if (arguments != null && arguments.Count >= 2)
        {
            int parsed;
            if (Int32.TryParse(arguments[1], out parsed)) { code = parsed; }
        }

        // Route to the injected action. The hosted app passes
        // Environment.Exit(code) — the single reliable way to end a hosted UI
        // app from the Tcl thread, since the UI thread would otherwise keep the
        // process alive after the engine's own exit merely flags the interpreter.
        if (_onQuit != null) { _onQuit(code); }

        result = string.Empty;
        return ReturnCode.Ok;
    }
}
```

The action arrives as an argument to the shared boot method, so the two callers
choose it and nothing else changes:

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs
        //The real-quit command; bootstrap.tcl shadows exit onto it so
        //DRAKON's File > Quit ends the app. The exit action is injected so
        //non-hosted callers (tests) pass a safe no-op.
        var quit = new QuitCommand(onQuit);
        long quitToken = 0;
        Result quitError = null;
        interp.AddCommand(quit, null, ref quitToken, ref quitError);
```

The last half of the path is in the guest's own language: a shadowing procedure
that takes over the built-in name and forwards to the host's command.

```tcl
# From CodeBrix.Samples/DRAKON.Brix/src/DRAKON.Brix.Core/Assets/bootstrap.tcl
# Added for DRAKON.Brix - because stock DRAKON's File > Quit menu item (and its
#   file-error paths) run the bare [exit] command, which under tclsh terminates
#   the process. The managed engine's own [exit] only marks the interpreter as
#   exited; in this hosted UI app the interpreter runs on a dedicated Tcl thread
#   while the UI thread keeps the process alive, so a bare [exit] would hang.
#   This proc shadows [exit] onto the host's __drakonbrix_quit command (see
#   Drakon/QuitCommand.cs), which calls Environment.Exit -- matching tclsh's
#   process-terminating exit semantics.
#
# Stock DRAKON / Tcl:
#   exit                          ;# tclsh's built-in, terminates the process
proc exit { args } {
    __drakonbrix_quit {*}$args
}
```

**Where to look.**
`DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/Commands/QuitCommand.cs`
`DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs` and
`src/DRAKON.Brix.Core/Assets/bootstrap.tcl`

**Sharp edges.**
- Register the host command before any script that might call it, and shadow the
  built-in name in the glue script rather than editing the guest. A guest you have
  not modified is a guest you can upgrade.
- The no-op the tests pass is what makes a stray exit inside a guest script
  harmless instead of ending the run at whichever case happened to hit it.
- Parse the arguments defensively. A script can call the command with any arity,
  and the exit code is optional.
- Ending a hosted application from the guest's thread means ending the process;
  asking the UI thread to close its window is not equivalent, because the guest's
  caller expects never to return.

### Add your own commands to an embedded interpreter

**When you want this.** The guest program needs something only the host can do -
report into the application log, read a host setting, end the application - and
you want it available as an ordinary command in the guest's own language rather
than as a special case in the boot code.

**The MVVM shape.** Each command is a small internal class: it derives from the
engine's command base type, declares its name and metadata in a `CommandData`, and
implements one execute method that reads its arguments and writes a result. What
the command actually does is a delegate handed in through the constructor, so the
same class serves the application and a headless caller. Registration happens once,
inside the boot sequence, on the engine's own thread.

**Code.**

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/Commands/DiagnosticReportCommand.cs
/// <summary>
/// A tiny <c>__brixreport MESSAGE</c> Tcl command that routes text to the
/// runtime's diagnostic sink — the interpreter's own <c>puts</c> is not
/// wired to the process console, so this is how startup-time Tcl probes
/// surface in the log.
/// </summary>
internal sealed class DiagnosticReportCommand : Default
{
    private readonly Action<string> _report;

    internal DiagnosticReportCommand(Action<string> report)
        : base(new CommandData(
            "__brixreport", null, null, null,
            typeof(DiagnosticReportCommand).FullName, CommandFlags.None, null, 0))
    {
        _report = report;
    }

    public override ReturnCode Execute(
        Interpreter interpreter, IClientData clientData, ArgumentList arguments, ref Result result)
    {
        if (arguments != null && arguments.Count >= 2)
        {
            _report("PROBE " + arguments[1]);
        }
        result = string.Empty;
        return ReturnCode.Ok;
    }
}
```

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs
    dispatch(interp =>
    {
        //Diagnostic reporting command reachable from Tcl.
        var diagnostic = new DiagnosticReportCommand(Report);
        long token = 0;
        Result addError = null;
        interp.AddCommand(diagnostic, null, ref token, ref addError);
        // ...
    });
```

The sink the delegate points at is one private method, so every diagnostic - the
guest's, the bridge's background errors and the host's own - leaves by the same
door and can also be observed by a test through the event:

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs
private void Report(string message)
{
    Console.WriteLine("DRAKONBRIX: " + message);
    Action<string> handler = Diagnostic;
    if (handler != null) { handler(message); }
}
```

**Where to look.**
`DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/Commands/DiagnosticReportCommand.cs`
`DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/Commands/QuitCommand.cs` and
`src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs`

**Sharp edges.**
- Always set a result, even an empty one, and return the engine's success code
  rather than throwing. The caller is a script, and an exception crossing back into
  interpreted code is not something the guest can handle.
- Prefix host command names so they cannot collide with anything the guest or its
  libraries define.
- Keep the command classes internal. Nothing outside the library that owns the
  engine should be adding commands to it.
- Register on the engine's thread, through the same dispatch the rest of the boot
  uses, and before the scripts that call them are sourced.
- The guest's own console output is not necessarily wired to the process console;
  a report command is how script-side probes become log lines.

### Release an exclusive device handle from both the page unload and the window close

**When you want this.** The view model holds something only one process may
hold - a device handle, an exclusive file lock, a capture session - and leaving
it held is not a leak the operating system tidies up until the process exits.
[Veto a window close until unsaved work is handled](BLUEPRINTS-PlatformServices.md#veto-a-window-close-until-unsaved-work-is-handled)
is the case where the close has to be delayed; this is the case where it must
not be delayed at all, only never missed.

**The MVVM shape.** The view model exposes one idempotent shutdown method that
unsubscribes, stops and releases. The page calls it from its unloaded handler
and the application calls it from the window's closed event. Neither knows what
is being released, and either may be the one that runs.

**Code.**

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Stops acquisition and releases the scope. Call from the view's unload or
/// window-closed handler -- a handle left open keeps the device locked
/// against every other process until this one exits.
/// </summary>
public void Shutdown()
{
    if (_scope == null) { return; }

    _scope.SamplesAvailable -= OnSamplesAvailable;
    _scope.StopStreaming();
    _scope.StopSignalGenerator();
    ScopeDeviceFinder.Reset();
    _scope = null;
    IsReady = false;
    IsStreaming = false;
    _log.LogInformation("Scope released.");
}
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.UI/Views/MainPage.xaml.cs
/// <summary>The page's view model, for the window-closed handler in <see cref="App"/>.</summary>
internal MainViewModel ViewModel => DataContext as MainViewModel;

// ...

public MainPage()
{
    // ...
    Loaded += async (_, _) =>
    {
        if (ViewModel != null) { await ViewModel.InitializeAsync(); }
    };

    Unloaded += (_, _) =>
    {
        //Releasing the handle matters: a scope left open stays locked
        //  against every other process until this one exits.
        ViewModel?.Shutdown();
    };

    this.InitializeComponent(); //Leave this line last
}
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.UI/App.xaml.cs
//Closing the window must release the scope: a handle left open keeps the
//  device locked against every other process, and a running signal
//  generator keeps running, until this process exits.
MainWindow.Closed += (_, _) => (rootFrame.Content as Views.MainPage)?.ViewModel?.Shutdown();
```

**Where to look.**
`PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs`
`PicoScope.Brix/src/PicoScope.Brix.UI/Views/MainPage.xaml.cs` and
`App.xaml.cs`

**Sharp edges.**
- Make the method idempotent and safe in either order. Both seams can fire, one
  after the other, and the early return on the already-released field is the
  whole guard.
- Neither seam is enough on its own. A page that is navigated away from unloads
  without the window closing, and on some heads a window can close without the
  page ever unloading.
- Unsubscribe before stopping, so no batch arrives after the view model believes
  it has finished and mutates state nobody is watching any more.
- Stop the side effects too. Here the signal generator keeps driving its output
  after the window is gone unless it is stopped explicitly, which is the kind of
  thing a dispose-only path misses.
- Disposal is a safety net, not the plan: the interface implements it so that a
  crash still frees the device, but the explicit release is what makes the
  handle available to the next process promptly. For the rest of the teardown -
  commands, delegates, subscriptions - see
  [Dispose a view model its commands and its bridge delegates](BLUEPRINTS-MVVM.md#dispose-a-view-model-its-commands-and-its-bridge-delegates).

### Marshal a save dialog onto the UI thread from a command handler

**When you want this.** A command opens a native picker, and you cannot prove
which thread the command handler is running on. A dialog belongs to the window it
is shown over, so the hop to the UI thread has to be made rather than assumed -
and the awaiting command still has to get the chosen path back. This is the
threading half of the picker story;
[Save a file through a native dialog from the view model](BLUEPRINTS-PlatformServices.md#save-a-file-through-a-native-dialog-from-the-view-model)
covers the bridge and the no-dialog head, and
[Clean up the path a file picker returns](BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns)
covers the path the picker hands back.

**The MVVM shape.** The view model exposes a settable delegate taking a suggested
file name and returning the chosen path or null, and awaits it. The page fills the
delegate in with a method that creates a `TaskCompletionSource`, enqueues the
actual picker onto the dispatcher queue, and returns the task: the command awaits a
result that is produced on the UI thread whichever thread asked for it.

**Code.**

```csharp
// From CodeBrix.Samples/SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Set by the head: asks the person where to write a baked ".cube" file, and hands back the full path
/// they chose, or null when they cancelled.
/// </summary>
/// <remarks>
/// The save dialog belongs to the head because every platform's is its own - Win32 on WPF, the file
/// picker on WinUI and on the CodeBrix.Platform heads. What they all share is the rule: a bake goes
/// where the person says it goes, and nowhere otherwise.
/// </remarks>
public Func<string, Task<string>> PickSaveCubePathAsync { get; set; }
```

```csharp
// From CodeBrix.Samples/SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml.cs
/// <summary>Asks where to write a baked lookup table, and returns null when the person cancels.</summary>
/// <remarks>
/// No SuggestedStartLocation: the dialog opens where this person last was, and the application never
/// proposes a folder of its own. The frame-buffer head has no dialog to show, so a bake there simply
/// says so rather than writing somewhere nobody chose.
/// </remarks>
private Task<string> PickSaveCubePathAsync(string suggestedFileName)
{
    //A SimpleCommand does not promise to run its handler on the user-interface thread, and a picker
    //  belongs to the window it is shown over - so the thread is made certain rather than assumed.
    TaskCompletionSource<string> chosen =
        new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

    var enqueued = DispatcherQueue?.TryEnqueue(async () =>
    {
        try { chosen.TrySetResult(await ShowSavePickerAsync(suggestedFileName)); }
        catch (Exception exception) { chosen.TrySetException(exception); }
    });

    //No dispatcher means no window to show it over, which reads the same as declining to choose.
    if (enqueued != true) { chosen.TrySetResult(null); }

    return chosen.Task;
}
```

```csharp
// From CodeBrix.Samples/SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml.cs
DataContextChanged += (_, _) =>
{
    //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
    (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

    if (DataContext is ICanvasBridge canvasBridge)
    {
        //Raised on the decoding thread: hop to the user-interface thread and mark the canvas dirty
        canvasBridge.InvalidateVideoCanvas = () => DispatcherQueue?.TryEnqueue(InvalidateVideoCanvas);
    }

    if (DataContext is IFileSaveBridge fileSave)
    {
        //The bake's destination: this head has a picker, and a head without one wires nothing
        fileSave.PickSaveCubePathAsync = PickSaveCubePathAsync;
    }
};
```

The command on the other end treats a null as a decision rather than a failure,
and a head that wired no delegate at all as its own case with its own sentence:

```csharp
// From CodeBrix.Samples/SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.Core/ViewModels/MainViewModel.cs
if (PickSaveCubePathAsync == null)
{
    //A head that cannot ask has nowhere to put the file, and this application never picks a
    //  location on someone's behalf.
    ShowMessage("This head has no save dialog, so there is nowhere to bake to.");
    return;
}

var cubeFilePath = await PickSaveCubePathAsync(BakeLocations.CreateFileName(DateTime.Now));

//Cancelled. Nothing is written and nothing is said: deciding not to save is not a failure.
if (string.IsNullOrWhiteSpace(cubeFilePath)) { return; }
```

**Where to look.**
`SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml.cs`
`SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.Core/ViewModels/MainViewModel.cs` and
`src/libs/SimpleCbxVideoPlayer.SkiaVideo/Playback/BakeLocations.cs`

**Sharp edges.**
- Create the completion source with the run-continuations-asynchronously option.
  Without it the command's continuation runs inline on the UI thread inside the
  enqueued callback, which is exactly the reentrancy a dialog does not want.
- Enqueueing can fail - no dispatcher, no window - and the return value says so.
  Treat that as "no choice was made" and complete the task, or the command awaits
  forever.
- Catch inside the enqueued callback and set the exception on the task. An
  exception thrown on the dispatcher queue with nobody watching takes the process
  with it.
- Never propose a folder unless you know the person wants one. Supplying only a
  suggested file name lets the platform's dialog open where they last were.
- A stamped suggested name stops two saves in a row from quietly proposing the
  same file.
- Two seams, two interfaces, one handler. The repaint delegate and the save dialog
  are wired in the same `DataContextChanged` block but through separate `is`
  tests, so a head that can supply one and not the other still gets what it can
  give; see
  [Assign every bridge through the interface that declares it](BLUEPRINTS-PlatformServices.md#assign-every-bridge-through-the-interface-that-declares-it).

### Offer a typed path where a head has no folder dialog

**When you want this.** Some of your heads have a native folder dialog and some
do not, and you want the feature to keep working on the ones that do not rather
than merely explaining itself. The one-delegate bridge and its null check are
established by
[Pick a file to open through a native dialog from the view model](BLUEPRINTS-PlatformServices.md#pick-a-file-to-open-through-a-native-dialog-from-the-view-model);
what this recipe adds is what you put behind the failure - a bound, editable
destination that exists whether or not a dialog does, so the picker is a
convenience rather than the only way in.

**The MVVM shape.** The bridge is the same one-property interface holding a
delegate the page fills in from `DataContextChanged`. The command checks the
delegate, calls it inside a try, and on either failure writes a status line that
names the alternative. The destination itself is an ordinary two-way bound
property, so the picker's only job is to write into it.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Lets the hosting page give the view model a native "choose a folder…" dialog. The page
/// wires this up with the CodeBrix.Platform <c>FolderPicker</c>; a head with no folder dialog
/// leaves it null and the user types the path into the text box instead.
/// </summary>
public interface IFolderPickBridge
{
    /// <summary>Shows a folder picker and returns the chosen path, or null if cancelled.</summary>
    Func<Task<string>> PickFolderPathAsync { get; set; }
}
```

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs
private async Task DoBrowseFolder()
{
    if (PickFolderPathAsync == null)
    {
        StatusText = "No folder dialog on this head - type the folder path into the text box.";
        return;
    }
    try
    {
        var path = await PickFolderPathAsync();
        if (!String.IsNullOrWhiteSpace(path))
        {
            FolderPath = path.Trim();
            StatusText = $"Photos will be saved to: {FolderPath}";
        }
    }
    catch (Exception e)
    {
        StatusText = $"Folder dialog failed: {e.Message} - type the folder path instead.";
    }
}
```

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.UI/Views/MainPage.xaml.cs
using System; //Required: the IAsyncOperation GetAwaiter extension (awaiting the FolderPicker) lives here

// ...

        DataContextChanged += (_, _) =>
        {
            // ...
            if (DataContext is IFolderPickBridge folderPick)
            {
                folderPick.PickFolderPathAsync = PickFolderPathAsync;
            }
            // ...
        };

// ...

    private static async Task<string> PickFolderPathAsync()
    {
        var picker = new FolderPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary
        };
        picker.FileTypeFilter.Add("*");

        StorageFolder folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }
```

**Where to look.**
`WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs`
`WebcamViewer/src/WebcamViewer.UI/Views/MainPage.xaml.cs` and
`Views/MainPage.xaml`

**Sharp edges.**
- The page wires the delegate unconditionally, so in this application the null
  branch never runs and the catch is the branch that matters. A head that
  registers no picker fails inside the call, not before it, so both paths have to
  say the same thing about the text box.
- Three outcomes, not two: no delegate and a throwing delegate both deserve an
  explanation, while a cancelled picker is silent and leaves the previous
  destination alone.
- Adding a filter is required on a folder picker even though it filters nothing.
- The awaiter extension for the picker's return type lives in the `System`
  namespace, so the page needs that using directive even when nothing else in the
  file does. The file carries a comment saying so, which is worth copying.
- Trim what the picker returns before storing it, because the same property is
  typed into by hand and both routes land in the same validation.
- A head that can opt into a picker on its host builder is a third case again; see
  the framebuffer opt-in in the startup area.

### Assign every bridge through the interface that declares it

**When you want this.** A page has several delegates to fill in on its data
context, and the quickest way to reach them all is one cast to the view model's
type. That cast is a compile-time dependency on a class the page has no other
reason to know, and it silently makes every capability all-or-nothing.

**The MVVM shape.** Each capability is its own small interface that the view model
implements. The page tests the data context once per contract with `is`, or
narrows a single test into one interface-typed local per contract, and assigns
through that. The page then names contracts and not classes, a head that can
satisfy some of them and not others still gives what it can, and a test or a
second view model can stand in because nothing is tied to a type.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
private void WireViewModel()
{
    if (ViewModel is not { } viewModel)
    {
        return;
    }

    surface ??= new VideoPlayerSurface(Player);
    viewModel.Playback.AttachSurface(surface);

    //Each bridge is handed over through the interface that declares it rather than through the
    //view model's own type, so what the page has to supply is the contract and nothing more.
    if (DataContext is IMediaFileBridge mediaFile)
    {
        mediaFile.PickMediaFileAsync = PickMediaFileAsync;
    }

    if (viewModel.Conversion is IOutputPathBridge outputPath)
    {
        outputPath.PickOutputPathAsync = PickOutputPathAsync;
    }
}
```

A bridge need not be implemented by the top-level view model. The second one here
belongs to the child view model that owns the conversion, and the page reaches it
through the same kind of test. The page still holds a typed `ViewModel` property
for the things a page legitimately needs its own view model for - here, attaching
the player surface - and that is the point of the split: the type is used where a
type is meant, and the contracts are used for everything a head supplies.

The other form narrows one test into a local per contract, which reads better when
a page fills in a lot of delegates at once:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
if (DataContext is MainViewModel viewModel)
{
    //Each bridge is filled in through the interface the view model implements
    //rather than through the concrete type, so what the page owes it is explicit.
    IImageCanvasBridge canvasBridge = viewModel;
    ICatalogGridBridge catalogBridge = viewModel;
    IViewerPaneBridge viewerBridge = viewModel;
    IAudioPlayerBridge audioBridge = viewModel;

    //Marshal 2D-canvas invalidations from the view model onto the UI thread
    canvasBridge.InvalidateImageCanvas = () => DispatcherQueue?.TryEnqueue(() => ImageCanvas?.Invalidate());
    // ...
}
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`

**Also shown by.**
`SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.UI/Views/MainPage.xaml.cs` (two
`is` tests in one handler, one per seam),
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs`,
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs`,
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs`
(the browser wiring reaches the view model through `IWebViewBridge` and stops
there),
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs` and
`InannaRosette/src/InannaRosette.Core/Services/IReadingTableBridge.cs`
(one `DataContextChanged` handler that tests the data context against three
bridge interfaces and `IXamlRootGetter` in turn rather than casting once to the
view model, so each capability is independently absent-able and the page names no
view-model type to install any of them)

**Sharp edges.**
- Wire from `DataContextChanged`, before `InitializeComponent()`. On some heads
  `InitializeComponent()` is what sets the data context, so a handler subscribed
  afterwards never runs.
- One test per contract rather than one for all of them. Otherwise a view model
  that does not implement the last interface you added quietly gets none of its
  delegates.
- Reading the page's `is` tests is the fastest description of what this head
  promises the view model. That is worth as much as the decoupling.
- The view model nulls every delegate it was handed in `Dispose()`, which is what
  breaks the page-to-view-model reference the wiring creates.

### Signal the view model when the page is on screen

**When you want this.** Startup work in the view model wants to show a dialog, and
a dialog needs a UI anchor that does not exist while the page is still being
built. Sleeping for a moment and hoping is the thing to avoid.

**The MVVM shape.** A one-method interface the view model implements. Each head's
page calls it from its loaded event, and the view model's startup task awaits a
`TaskCompletionSource` that the call completes. A head that never calls it simply
never shows that dialog, and disposal cancels the wait so nothing is left pending.
The XamlRoot getter covered by
[Give the view model a XamlRoot so its dialogs can show](BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show)
says where a dialog attaches; this says when there is something to attach to.

**Code.**

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
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
private readonly TaskCompletionSource _pageReady = new(TaskCreationOptions.RunContinuationsAsynchronously);

// ...

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
    // ...
}

// ...

public void NotifyPageReady() => _pageReady.TrySetResult();
```

Every head satisfies it in one line, and the MAUI head differs only in the name of
the property that holds the view model:

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Wpf/Views/MainWindow.xaml.cs
//The view model waits for this before it shows its startup dialog
Loaded += (sender, args) => (DataContext as IPageReadyNotifier)?.NotifyPageReady();
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Mobile/Views/MainPage.xaml.cs
//The view model waits for this before it shows its startup dialog: a dialog needs a page to
//  attach to, and the page is not on screen until it has loaded.
Loaded += (sender, args) => (BindingContext as IPageReadyNotifier)?.NotifyPageReady();
```

**Where to look.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml.cs`,
`JustBetweenUs.Wpf/Views/MainWindow.xaml.cs`,
`JustBetweenUs.WinUI/Views/MainPage.xaml.cs` and
`Mobile/Views/MainPage.xaml.cs`

**Also shown by.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs` (`OnPageReady`,
an ordinary method guarded by a bool rather than a completion source, because
nothing is waiting on it - the page's `Loaded` handler is what starts the work) and
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Cancel the completion source in `Dispose()`. A view model torn down before its
  page loaded would otherwise leave a task waiting forever, and the awaiting code
  needs a `catch (OperationCanceledException)` that does nothing.
- Create it with the run-continuations-asynchronously option, or the startup task
  resumes inline on whichever thread raised the loaded event.
- `Loaded` can fire more than once. `TrySetResult` makes the second call harmless,
  which is why the signal is idempotent rather than an event.
- Do not replace this with a delay. The wait is for a specific fact, and on a slow
  head a guessed delay is either too short to be true or long enough to be felt.

### Keep picker plumbing in the page and picker policy in the view model

**When you want this.** A native picker hands back something that needs work
before it is useful - a percent-encoded path, an empty placeholder file it created
at the chosen name - and you have to decide where that work belongs.
[Clean up the path a file picker returns](BLUEPRINTS-PlatformServices.md#clean-up-the-path-a-file-picker-returns)
is the pair of helpers that do it; this is where each one is called from, and why
the answer is not the same for both.

**The MVVM shape.** Draw the line at "is this true of the platform, or true of
this application". Turning what a head returns into a real file-system path is
plumbing: it is a fact about that head's dialog, so the page does it and the view
model never sees anything else. What a chosen path then means - that an empty file
a save dialog just created is a destination rather than a document the user
already has - is application policy, identical on every head, so the command
applies it. The bridge delegate carries a plain path across the seam.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs
private static async Task<string> PickSavePdfPathAsync(string suggestedFileName)
{
    var picker = new FileSavePicker
    {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        SuggestedFileName = suggestedFileName,
        DefaultFileExtension = ".pdf"
    };
    picker.FileTypeChoices.Add("PDF document", new List<string> { ".pdf" });

    var file = await picker.PickSaveFileAsync();
    if (file == null) { return null; }

    //Some heads percent-encode the path they return, which would save "My Article.pdf" as
    //  "My%20Article.pdf"; decode it before anything touches the disk. Decoding is the only
    //  thing the page does to the path: what to make of an empty file already sitting there
    //  is application policy, and the view model applies it for every head.
    return FileDialogHelper.ToFileSystemPath(file.Path);
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
var chosenPath = await PickSavePdfPathAsync(GetSuggestedFileName());
if (!string.IsNullOrWhiteSpace(chosenPath))
{
    OutputFilePath = chosenPath.Trim();

    //Application policy, applied identically on every head: an empty file that a
    //  save dialog created as a placeholder for a brand-new name is not a file the
    //  user already has, so remove it and let the publish-time "replace existing
    //  file?" prompt speak only for a file with real content in it.
    FileDialogHelper.RemoveEmptyPlaceholder(OutputFilePath);

    StatusText = $"Will save to: {OutputFilePath}";
}
```

**Where to look.**
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs`
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`

**Also shown by.**
`NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml.cs` and
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
(the same split, with the policy comment in the command that applies it),
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Services/WinRtFileSavePicker.cs`
(the placeholder removal is inside the picker service there, because that
application's save command asks its own replace question later and the service is
head-specific anyway)

**Sharp edges.**
- The test is not "which file is it easier to write in". Anything a second head
  would have to repeat differently is plumbing; anything every head must do the
  same way is policy, and policy in a page is policy you will have to copy.
- Policy in the command is policy you can test, because the command runs without a
  window and the page's picker does not.
- Keep the delegate's contract boring: a path or null. A bridge that returns a
  storage object, or a tuple of path plus flags, drags the platform back across
  the seam.
- Trim what comes back before storing it, whichever side you are on, because the
  same property is usually typed into by hand as well.

### Build a head's native picker behind a registered service

**When you want this.** The page code-behind that hands a picker delegate to the
view model has grown a dialog of its own to build, and you would rather that
construction lived somewhere a second page, or a second head, can reuse.

**The MVVM shape.** The head's UI project declares an interface for the dialog it
can show and one implementation of it, registers it as a singleton in `App`'s
service callback, and the page resolves it and hands the view model a method group.
The view model is untouched: it still sees only its own file-save bridge, and a
head with a different dialog would register its own implementation of its own
interface.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Services/IFileSavePicker.cs
/// <summary>
/// The head's native "save PNG" dialog, behind an interface and registered with
/// <c>SimpleServiceResolver</c> at startup, so the page that hands the delegate to the view
/// model's <c>IFileSaveBridge</c> property never builds a picker itself.
/// </summary>
public interface IFileSavePicker
{
    /// <summary>
    /// Shows a "save PNG" dialog seeded with <paramref name="suggestedFileName"/> and returns
    /// the full path the user chose, or <c>null</c> if they cancelled.
    /// </summary>
    /// <param name="suggestedFileName">The file name the dialog opens with.</param>
    /// <returns>The chosen full path, or <c>null</c> when the dialog was cancelled.</returns>
    Task<string> PickSavePngPathAsync(string suggestedFileName);
}
```

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/App.xaml.cs
SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
{
    //The one thing the view model cannot do for itself: this head's native save
    //  dialog. The page resolves it and hands it to the view model's file-save
    //  bridge; everything else, the drawing session included, lives in the view model
    services.AddSingleton<IFileSavePicker, WinRtFileSavePicker>();
});
```

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs
if (DataContext is IFileSaveBridge fileSave)
{
    //The save dialog is a registered service, so the page hands the view model a
    //  delegate without knowing how the dialog is built
    fileSave.PickSavePngPathAsync =
        SimpleServiceResolver.Instance.GetService<IFileSavePicker>().PickSavePngPathAsync;
}
```

The implementation is the code that used to sit in the page, unchanged, including
the placeholder-file cleanup that this application's picker owes its view model:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Services/WinRtFileSavePicker.cs
public class WinRtFileSavePicker : IFileSavePicker
{
    /// <inheritdoc />
    public async Task<string> PickSavePngPathAsync(string suggestedFileName)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            SuggestedFileName = suggestedFileName,
            DefaultFileExtension = ".png"
        };
        picker.FileTypeChoices.Add("PNG image", new List<string> { ".png" });

        StorageFile file = await picker.PickSaveFileAsync();
        if (file == null) { return null; }

        FileDialogHelper.RemoveEmptyPlaceholder(file.Path);
        return file.Path;
    }
}
```

**Where to look.**
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Services/IFileSavePicker.cs` and
`WinRtFileSavePicker.cs`
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/App.xaml.cs`
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- The interface belongs to the head, not to the shared library. Its implementation
  names a WinRT dialog type, so it could not be shared with a native WPF head even
  if you wanted to - and in this application those native heads still build their
  own dialog in the page, which is fine while each is a handful of lines.
- Resolve in the page, not in the view model. A view model that resolved a picker
  service would be back to knowing that dialogs exist, which is the thing the
  bridge was for.
- A method group is the whole assignment. There is no wrapper lambda to forget to
  null out, and the delegate the view model holds points at the singleton rather
  than at the page.
- This is worth doing when the construction is more than a few lines or a second
  caller wants it. A three-line picker is fine where it is.

### Call the page's bridge from the setter that changed

**When you want this.** Something in the view model changed, and a page has to
react to it in a way no binding expresses: scroll a list back to its top, ask a
canvas whether it initialized. Subscribing to `PropertyChanged` in the page and
comparing property names is the usual reflex.

**The MVVM shape.** Turn it around. The view model declares a one-delegate bridge
for the thing only the page can do, and invokes it from the setter or the method
where the change actually happens. The page's part is one assignment. Nothing
matches on a string, the page has no subscription to unhook, and the moment the
call is made is exactly the moment the view model means, rather than whenever the
notification is dispatched.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/IModelViewBridge.cs
/// <summary>
/// The head-capability bridge for the Model View: the view model invokes this as the view
/// opens, and the page does what only it can - ask its 3D preview canvas whether OpenGL
/// initialization failed. The view model must behave sensibly when the delegate is
/// <c>null</c>.
/// </summary>
public interface IModelViewBridge { Action ModelViewOpened { get; set; } }
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>Whether the Model View is active (otherwise the Browsing View shows).</summary>
[AffectsCommands(nameof(DocumentCommand))]
[AffectsProperties(nameof(BrowsingViewVisibility), nameof(ModelViewVisibility))]
public bool IsModelViewActive
{
    get;
    private set
    {
        var wasActive = field;
        SetProperty(ref field, value);

        //Only the page can see whether the preview canvas managed to initialize OpenGL,
        //so it is told when there is something to look at.
        if (value && !wasActive) { ModelViewOpened?.Invoke(); }
    }
}
```

The same application's other bridge is invoked from a method rather than a setter,
which is the same idea one level up: the call goes where the decision is made.

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
//Re-applies search + sort and swaps in a fresh lazily-loading cell collection.
private void RebuildCells()
{
    // ...

    //The grid is showing a different set of models now, so start it at the first one.
    ScrollCatalogToTop?.Invoke();
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/IModelViewBridge.cs` and
`ICatalogGridBridge.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/ICatalogGridBridge.cs`
and `IViewerPaneBridge.cs` (the catalog scroll is invoked from the `Cells` setter,
the viewer hook from the `IsViewerActive` setter)

**Sharp edges.**
- Guard the edge, not just the value. The view-activation setter tests
  `value && !wasActive`, because a setter can be written with the value it already
  holds and the page's work is not idempotent. A call made from a method that only
  runs when something really changed needs no such guard.
- The delegate is null until a page wires it, and on a head that wires nothing it
  stays null, so `?.Invoke()` is the graceful path and not a defensive habit.
- Invoking from a setter means the call happens on whichever thread wrote the
  property. If the page's work must be on the UI thread, the page's delegate is
  what marshals it, exactly as with a repaint.
- Do not send state through the bridge that a binding already carries. These
  delegates say "this just happened", not "here is the new value".

### Send the paint call back to the view model through the canvas bridge

**When you want this.** What a canvas should draw depends on application state -
which mode the application is in, which of two things the canvas is showing - and
the paint handler in the page is where that decision keeps landing.
[Let the page invalidate a canvas through a bridge interface](BLUEPRINTS-PlatformServices.md#let-the-page-invalidate-a-canvas-through-a-bridge-interface)
carries repaint requests from the view model to the page; this is the same
interface used in the other direction.

**The MVVM shape.** The bridge gains a method - not a delegate, because the view
model is the one implementing it - that takes the surface and draws. The page's
`PaintSurface` handler forwards and does nothing else, so the mode decision, the
renderers and the overlay state all stay in the view model, and the page keeps no
opinion about what it is showing.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/Bridges/ICanvasBridge.cs
/// <summary>
/// Draws whatever the main canvas should be showing right now - the mirrored live preview
/// in Capture Mode, the painting and its crosshair in Paint Mode. Called from the page's
/// <c>PaintSurface</c> handler, so always on the UI thread.
/// </summary>
/// <param name="surface">The Skia surface to render onto.</param>
/// <param name="info">The image info describing the surface.</param>
void RenderMainCanvas(SKSurface surface, SKImageInfo info);
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Draws the main canvas: the mirrored live preview in Capture Mode, the painting and its
/// hand crosshair in Paint Mode. The mode decision lives here, so the page's paint handler
/// is a single forward (see <see cref="ICanvasBridge"/>) and stays out of application state.
/// </summary>
/// <param name="surface">The Skia surface to render onto.</param>
/// <param name="info">The image info describing the surface.</param>
public void RenderMainCanvas(SKSurface surface, SKImageInfo info)
{
    var session = _paintSession;
    if (IsPaintMode && session != null)
    {
        PaintCanvasHelper.Render(surface, info, session,
            CrosshairNormX, CrosshairNormY, IsBrushPainting);
    }
    else
    {
        _mainRenderer.Render(surface, info, _captureService, mirror: true);
    }
}
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs
//Which of the two things the main canvas shows is application state, so the handler
//  forwards the surface and lets the view model draw (see ICanvasBridge)
MainCanvas.PaintSurface += (_, e) =>
    (DataContext as ICanvasBridge)?.RenderMainCanvas(e.Surface, e.Info);
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/Bridges/ICanvasBridge.cs`
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs`

**Also shown by.**
`WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs` (`IVideoFrameSource`,
which the page's renderer pulls the newest frame through - the view model keeps the
frame, the renderer keeps the buffers, and neither names the other's type),
`PalmVisualizer/src/libs/PalmVisualizer.Camera/IWebcamCaptureService.cs`
(`IWebcamFrameSource`, the same contract, implemented explicitly by the view model
so it stays off its public surface),
`GameEngineMusicDemo/src/GameEngineMusicDemo.Core/ViewModels/MainViewModel.cs`
(`IManageGameCanvas.Demo`, a read-only property the page reads back through the
bridge it implements rather than through the view model type)

**Sharp edges.**
- A method, not a settable delegate. The direction decides the shape: the page
  fills in delegates because the page is the implementer, and the view model
  implements methods for the same reason.
- The handler is called on the UI thread by the canvas, so the view model's
  implementation must not block. Anything expensive belongs in the cached state it
  draws from, not in the draw.
- Cast the data context, do not hold it. `(DataContext as ICanvasBridge)?` in the
  handler costs nothing and survives the data context being replaced or cleared.
- Where two canvases show different things, only the one whose content is a
  decision needs this. WebcamPainter's self-view keeps its own renderer in the
  page, because it always shows exactly one thing.

### Ask the page for dialogs so they keep the application's own styling

**When you want this.** The application has a strong look, and a stock dialog in
the middle of it is a hole. You want the view model to go on asking for a
confirmation or a message in one line, but for the thing that appears to be built
from the application's own resources.

Two existing recipes cover the neighboring cases.
[Confirm and inform from the view model with SimpleViewModel dialogs](BLUEPRINTS-MVVM.md#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs)
is the right answer when the stock dialog is fine, and
[Install UI dialogs into a headless model through handler delegates](BLUEPRINTS-PlatformServices.md#install-ui-dialogs-into-a-headless-model-through-handler-delegates)
puts handler delegates on a library that must stay UI-free. This one is about
choosing to hand the shapes back to the page purely so the page can keep the look,
and about writing down what each delegate means when it is null.

**The MVVM shape.** The view model declares one interface per family of
capability, and this one holds the three dialog shapes the application actually
asks for by name - a confirmation, a message, and a task-specific panel. The page
implements all three from its own styles and resources and assigns them in its
data-context handler. The view model calls each one through a small private helper
that says, in code, what happens when the delegate is null, so a head that cannot
show a dialog is a documented case rather than a crash.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.Core/Services/IReadingDialogBridge.cs
/// <summary>
/// The dialogs this application asks for by name. <c>SimpleViewModel</c>'s own
/// <c>ConfirmDialog</c> and <c>ShowError</c> helpers would do the job, but the temple styling —
/// gold on lapis, the app's own button styles, a "Report saved" panel with an Open button — is
/// a page concern, so the page supplies the three shapes the view model needs and keeps the
/// look with them. A head that cannot show a dialog leaves them null; the view model then
/// treats a confirmation as granted and reports failures in the status line only.
/// </summary>
public interface IReadingDialogBridge
{
    /// <summary>
    /// Asks a yes/no question and returns true when the person pressed the primary button.
    /// Signature: <c>Func&lt;title, message, primaryButtonCaption, Task&lt;bool&gt;&gt;</c>.
    /// </summary>
    Func<string, string, string, Task<bool>>? ConfirmAsync { get; set; }

    /// <summary>Tells the person something and waits for them to close it.</summary>
    Func<string, string, Task>? ShowMessageAsync { get; set; }

    /// <summary>
    /// Shows the "Report saved" panel for a finished PDF, whose Open button runs
    /// <see cref="ViewModels.MainViewModel.OpenSavedReportCommand"/>.
    /// </summary>
    Func<string, Task>? ShowReportSavedAsync { get; set; }
}
```

The page's implementations are ordinary dialog code that reaches into
`Application.Current.Resources` for the application's own button styles, and turns
a failure into a documented answer rather than an exception crossing back into the
view model:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
private async Task<bool> ConfirmAsync(string title, string message, string primary)
{
    try
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = Text(message, 12.5, Ornament.Ivory, opacity: 0.9, wrap: true),
            PrimaryButtonText = primary,
            CloseButtonText = "Cancel",
            PrimaryButtonStyle = (Style)Application.Current.Resources["TemplePrimaryButtonStyle"],
            CloseButtonStyle = (Style)Application.Current.Resources["TempleButtonStyle"],
            XamlRoot = XamlRoot,
        };
        return await dialog.ShowAsync() == ContentDialogResult.Primary;
    }
    catch (Exception ex)
    {
        _log.LogWarning(ex, "Confirm dialog failed.");
        return true;
    }
}
```

The third shape is the one a general-purpose helper could not have produced: a
panel with content the application composed and a button of its own. Even there,
the page does not act - the button runs the view model's command, so deciding to
hand a file to the operating system stays a view-model decision.

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
private async Task ShowReportSavedAsync(string path)
{
    var stack = new StackPanel { Spacing = 12, Width = 430 };
    stack.Children.Add(Text("The report has been written to:", 12, Ornament.Ivory, opacity: 0.9, wrap: true));
    stack.Children.Add(Text(path, 11, Ornament.GoldPale, wrap: true));

    //Handing the file to the operating system is the view model's job, so the button runs
    //  its command rather than starting anything itself
    var open = new Button
    {
        Content = "Open",
        Style = (Style)Application.Current.Resources["TempleButtonStyle"],
        Command = ViewModel?.OpenSavedReportCommand,
    };
    // ... the button into a row, the row into the panel, the panel into a ContentDialog
}
```

Every call goes through a helper that states the null policy once:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.Core/ViewModels/MainViewModel.cs
//A head with no dialog cannot ask, so the answer is yes - exactly what the page's own
//  dialog did when ContentDialog.ShowAsync threw
private async Task<bool> Confirm(string title, string message, string primary)
{
    var confirm = ConfirmAsync;
    return confirm == null || await confirm(title, message, primary);
}

private async Task ShowMessage(string title, string message)
{
    var show = ShowMessageAsync;
    if (show == null)
    {
        InvokeOnMainThread(() => SetStatus($"{title}: {message}"));
        return;
    }

    await show(title, message);
}

private async Task<string?> PickSavePath(string suggestedFileName, string typeName, string extension)
{
    var picker = PickSavePathAsync;
    if (picker == null)
    {
        await ShowMessage("The file dialog is unavailable", "This head cannot save files.");
        return null;
    }

    return await picker(suggestedFileName, typeName, extension);
}
```

**Where to look.**
`InannaRosette/src/InannaRosette.Core/Services/IReadingDialogBridge.cs` and
`IReadingFileBridge.cs`
`InannaRosette/src/InannaRosette.Core/ViewModels/MainViewModel.cs` (the region
"Head-capability bridges")
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs` (`ConfirmAsync`,
`ShowMessageAsync`, `ShowReportSavedAsync`)

**Sharp edges.**
- Decide the null answer per delegate and write it down. A missing confirmation is
  granted, because refusing would make a head with no dialog unable to do
  anything; a missing message becomes status text, because the information is
  still worth having; a missing picker becomes a message saying this head cannot
  save. Silently doing nothing is the one answer that is always wrong.
- The page's catch has to agree with the null case. Here the dialog's failure path
  returns true for the same reason the null delegate does, so the behavior is the
  same whether the head has no dialog or the dialog throws.
- Capture the delegate into a local before testing and calling it. It is a mutable
  property, and the page nulls all of them on unload.
- The page still hands over a `XamlRoot` getter separately, so anything that does
  use the stock helpers has somewhere to attach - see
  [Give the view model a XamlRoot so its dialogs can show](BLUEPRINTS-PlatformServices.md#give-the-view-model-a-xamlroot-so-its-dialogs-can-show).
  Styling the shapes yourself is a choice, not a replacement for that wiring.
