# CodeBrix.Samples Blueprints: Bridging platform services into the view model

These recipes cover the seam between a view model and the capabilities only a
hosting page or a head can provide: dialogs that need a XamlRoot, native file
open and save pickers, the clipboard, canvas repaints, a repeating timer,
the mouse cursor, an embedded browser and an audio transport. The shape
is nearly always the same one - the view model declares a small interface
holding a delegate, implements it, and the page fills the delegate in when
the data context arrives - so one piece of shared code runs both on a head
that supplies the capability and on a head that does not. Several recipes also
cover what to do with what comes back, such as normalizing the path a picker
returns or keeping a single replace-file confirmation instead of two. Reach
for this file when a command needs something the view model cannot do for
itself, when the same feature has to work across several UI stacks, or when
a head with no windowing system must still start and explain what it cannot do.

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
- [Put a platform service behind an interface with a no-op default](#put-a-platform-service-behind-an-interface-with-a-no-op-default)
- [Install UI dialogs into a headless model through handler delegates](#install-ui-dialogs-into-a-headless-model-through-handler-delegates)
- [Marshal a repeating timer into a headless model](#marshal-a-repeating-timer-into-a-headless-model)
- [Set the mouse cursor from a model owned interface](#set-the-mouse-cursor-from-a-model-owned-interface)
- [Veto a window close until unsaved work is handled](#veto-a-window-close-until-unsaved-work-is-handled)
- [Tell the user when graphics initialization failed](#tell-the-user-when-graphics-initialization-failed)
- [Show a WebView on every head and drive it from a command](#show-a-webview-on-every-head-and-drive-it-from-a-command)
- [Replay a finished audio clip with one button press](#replay-a-finished-audio-clip-with-one-button-press)

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
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs`

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
implements the picker in a few lines inside its `DataContextChanged` handler.

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
            OutputFilePath = chosenPath.Trim();
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
    //  "My%20Book.pdf"; decode it before anything touches the disk.
    var path = FileDialogHelper.ToFileSystemPath(file.Path);

    FileDialogHelper.RemoveEmptyPlaceholder(path);
    return path;
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
`NotSupportedException` caught specifically)

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

**Adapted: put the picker behind an interface rather than calling it inline.**
PdfSideBySide calls the picker directly from its view model; the shape to prefer
keeps the picker configuration verbatim but moves the call behind a bridge, so a
head with no picker is a case the view model handles rather than an exception it
catches:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    private static async Task<string> PickPdfPathAsync()
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };
        picker.FileTypeFilter.Add(".pdf");

        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }
```

```csharp
// Adapted from CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
// The picker call is moved behind an interface the page implements, so a head that cannot
// show one is a case the view model handles instead of an exception it catches.
public interface IPdfFileBridge
{
    Task<string> PickPdfPathAsync();
}

// In the view model:
private IPdfFileBridge _fileBridge;

public void SetFileBridge(IPdfFileBridge bridge) => _fileBridge = bridge;

private async Task BrowseAsync(DocumentSide side)
{
    if (IsBusy) { return; }
    if (_fileBridge == null)
    {
        await ShowError("This head cannot browse for files; pass the two PDF paths on the command line.");
        return;
    }

    IsBusy = true;
    try
    {
        var path = await _fileBridge.PickPdfPathAsync();
        if (path == null) { return; }
        // ... unchanged from the sample
    }
    finally { IsBusy = false; }
}
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Services/IMediaFileBridge.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`
`PdfSideBySide/src/PdfSideBySide.LinuxFrameBuffer/Program.cs`

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
/// placeholder file at the chosen path for a brand-new name. Remove it - but only when it
/// is genuinely empty - so a chosen path behaves like a pure destination and the app's own
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
`PainDiagram/Shared/Helpers/FileDialogHelper.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Helpers/FileDialogHelper.cs` (the
folder picker needs the same decoding: a folder called "My Models" would otherwise
send every download to a literally named `My%20Models`),
`WikipediaPublisher/Shared/Helpers/FileDialogHelper.cs` (linked into the shared
library and the WinUI head only - the WPF head does not link it, because a WPF
`SaveFileDialog` already returns a plain path),
`WebcamPainter/src/WebcamPainter.Core/Helpers/FileDialogHelper.cs`

**Sharp edges.**
- Decoding unconditionally would corrupt a legitimate name containing a percent
  sign, which is why the helper looks for a real `%XX` escape first.
- The placeholder is deleted only when its length is zero, so a real file is never
  lost before the application's own overwrite confirmation.
- Failure to delete is deliberately swallowed: the worst case is one extra
  confirmation prompt, never lost data.
- Call both helpers in the page, before the path reaches the view model, so the
  view model only ever sees real paths.

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
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Lets the hosting page hand the view model the invalidate (repaint) delegates for the two
/// Skia canvases. Frames and tracking results arrive on capture/worker threads; the page's
/// delegates are responsible for marshalling their invalidates onto the UI thread.
/// </summary>
public interface ICanvasBridge
{
    Action InvalidateMainCanvas { get; set; }
    Action InvalidateSelfView { get; set; }
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

InitializeComponent();
```

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
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs` and
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

public class MainViewModel : SimpleViewModel, ICopyToClipboard
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
the answer comes back.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Window-close save prompt. Closed is the platform's cancellable-close
//event: setting Handled vetoes the close, and the X11 head reports
//SupportsClosingCancellation. The save-prompt loop is async, so when
//dirty documents exist the close is vetoed first and re-issued once
//the user has decided.
MainWindow.Closed += async (_, e) =>
{
    if (windowCloseConfirmed) { return; }

    if (!Pinta.Brix.Engine.PintaCore.Workspace.OpenDocuments.Any(d => d.IsDirty)) { return; }

    e.Handled = true;

    try
    {
        if (Views.MainPage.Current is { } page && await page.ConfirmCloseApplicationAsync())
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

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`
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
        message += "On Windows, you may be able to fix this by installing the free Microsoft " +
            "\"OpenCL and OpenGL Compatibility Pack\"...\n\n";
    }

    message += $"Details:\nStatus: {state.Status}\n{state.FailedReason ?? "(none reported)"}";

    using var dialog = CreateDialog(message, "3D Preview Unavailable");
    _ = await dialog.ShowAsync();
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
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
  tree.
- A page-level flag reports the failure once per run; without it the dialog
  reappears on every item the user opens.
- Decide the operating-system-specific hint with `SimpleOsInfo` rather than
  compiling it in, so the same message code runs on every head.

### Show a WebView on every head and drive it from a command

**When you want this.** Your application needs an embedded browser the user
navigates freely, and a command that sends it somewhere.

**The MVVM shape.** The view model declares a bridge with an `Action<string>` the
page sets, plus a method the page calls whenever the browser lands on a new URL.
The command builds the URL and marshals the navigation onto the UI thread; the
page does nothing but forward. The view model checks the delegate for null before
using it and never names a WebView type.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
public interface IWebViewBridge
{
    /// <summary>Navigates the embedded browser to the given URL (null when no WebView).</summary>
    Action<string> NavigateToUrl { get; set; }

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
        InvokeOnMainThread(() => NavigateToUrl(searchUrl));
        StatusText = "Browse to the article you want, then click Publish.";
    }

    return Task.CompletedTask;
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
    if (_browserInitialized || DataContext is not MainViewModel viewModel) { return; }
    _browserInitialized = true;

    //Use CoreWebView2.Source (the authoritative current URL after redirects / user
    //  navigation); the XAML Browser.Source property does not reliably reflect those.
    Browser.NavigationCompleted += (_, _) =>
        viewModel.SetCurrentBrowserUrl(Browser.CoreWebView2?.Source ?? Browser.Source?.AbsoluteUri);

    viewModel.NavigateToUrl = url =>
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            Browser.Source = new Uri(url);
        }
    };

    Browser.Source = new Uri(MainViewModel.HomeUrl);
}
```

```xml
<!-- From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml -->
<!-- Center: embedded browser. Every Skia head now has a WebView2 - the Windows,
     Skia-on-WPF and macOS runtimes have it built in, and the Linux heads get it
     from the CodeBrix.Platform.WebView add-in (WPE WebKit). -->
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
element's own transport quirk, but the policy - Play means replay when the clip
has finished, resume when the user has scrubbed - is application behavior and
belongs on the view model, with the bridge exposing read-only transport facts and
a seek. The block below is adapted to that shape; the sample keeps the same logic
in the page.

**Code.**

```csharp
// Adapted from CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
// The sample implements this in the page; the logic is unchanged, but here the state and
// the decision live on the view model, and the bridge grows read-only transport facts.
public interface IAudioPlayerBridge
{
    // ... LoadAudioSource, PlayAudio, PauseAudio, StopAudio, SetAudioLooping ...

    /// <summary>Whether the player is currently advancing.</summary>
    Func<bool> IsAudioPlaying { get; set; }

    /// <summary>The player's position and the clip's duration.</summary>
    Func<TimeSpan> AudioPosition { get; set; }
    Func<TimeSpan> AudioDuration { get; set; }

    /// <summary>Moves the player to a position.</summary>
    Action<TimeSpan> SeekAudio { get; set; }
}

//How close to the duration still counts as "parked at the end". The player refreshes its
//position on an interval, so the last value it reports before ending can sit just short
//of the duration.
private static readonly TimeSpan AudioEndTolerance = TimeSpan.FromMilliseconds(250);
private bool _audioPlaybackEnded;

public SimpleCommand PlayAudioCommand => field ??= new SimpleCommand(() =>
{
    //A clip that has played through to its end leaves the transport parked at the end,
    //where Play alone has nothing left to play - so rewind first and let one click replay
    //the clip. Two things deliberately do NOT rewind: a player that is still going (a
    //looping clip raises PlaybackEnded on every pass), and a clip the user has scrubbed
    //away from the end since it finished - there, the thumb is the intent.
    if (_audioPlaybackEnded
        && IsAudioPlaying?.Invoke() == false
        && AudioDuration?.Invoke() > TimeSpan.Zero
        && AudioPosition?.Invoke() >= AudioDuration.Invoke() - AudioEndTolerance)
    {
        SeekAudio?.Invoke(TimeSpan.Zero);
    }

    _audioPlaybackEnded = false;
    PlayAudio?.Invoke();
});
```

```csharp
// Adapted from CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
AudioElement.PlaybackEnded += (_, _) => ViewModel?.NotifyAudioPlaybackEnded();
viewModel.IsAudioPlaying = () => AudioElement?.IsPlaying ?? false;
viewModel.AudioPosition = () => AudioElement?.Position ?? TimeSpan.Zero;
viewModel.AudioDuration = () => AudioElement?.Duration ?? TimeSpan.Zero;
viewModel.SeekAudio = position => AudioElement?.Seek(position);
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- A looping clip raises its playback-ended event on every pass while still
  playing, so the flag alone is not enough; the "is it playing" check is what
  stops a loop being rewound mid-play.
- The player refreshes its reported position on an interval, so the last position
  before the end can sit slightly short of the duration. A tolerance window is
  what makes the end-of-clip test reliable.
- Loading a new source and stopping both clear the flag.

