using CodeBrix.Platform.Simple;
using PdfSideBySide.PdfRender;
using PdfSideBySide.PdfRender.Documents;
using PdfSideBySide.PdfRender.Rendering;
using PdfSideBySide.PdfRender.Viewing;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace PdfSideBySide.ViewModels;

[Microsoft.UI.Xaml.Data.Bindable]
public class MainViewModel : SimpleViewModel
{

    private readonly PdfComparison _comparison = new();
    private readonly PageRenderer _renderer = new();

    //One in-flight render per side; a newer page request cancels the older one
    private CancellationTokenSource _leftRender;
    private CancellationTokenSource _rightRender;

    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Debug.WriteLine("Main view model startup.");

        LeftPane = new DocumentPaneViewModel("Document 1", () => BrowseAsync(DocumentSide.Left));
        RightPane = new DocumentPaneViewModel("Document 2", () => BrowseAsync(DocumentSide.Right));
        _ = OpenStartupDocumentsAsync();
    }

    #region | Bindable properties |

    /// <summary>The left pane - Document 1.</summary>
    public DocumentPaneViewModel LeftPane { get; }

    /// <summary>The right pane - Document 2.</summary>
    public DocumentPaneViewModel RightPane { get; }

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

    /// <summary>The zoom level for display - "100%".</summary>
    public string ZoomLabel => $"{View.Zoom.Percent}%";

    /// <summary>"Comparing 39:35" - the two current page numbers, shown between the page labels; empty until both documents are open.</summary>
    public string StatusText
    {
        get;
        private set => SetProperty(ref field, value ?? string.Empty);
    } = string.Empty;

    /// <summary>Whether a file picker or document open is in progress (blocks the navigation buttons).</summary>
    [AffectsAllCommands]
    public bool IsBusy
    {
        get;
        private set => SetProperty(ref field, value);
    }

    #endregion

    #region | Commands and their implementations |

    /// <summary>Main Up: both documents to their previous page.</summary>
    public SimpleCommand PreviousPageCommand => field ??=
        new SimpleCommand(() => !IsBusy && _comparison.CanMoveBothPrevious,
            (Func<Task>)(() => StepAsync(_comparison.MoveBothPrevious, renderLeft: true)));

    /// <summary>Main Down: both documents to their next page.</summary>
    public SimpleCommand NextPageCommand => field ??=
        new SimpleCommand(() => !IsBusy && _comparison.CanMoveBothNext,
            (Func<Task>)(() => StepAsync(_comparison.MoveBothNext, renderLeft: true)));

    /// <summary>Adjustment Up: only the right document to its previous page.</summary>
    public SimpleCommand AdjustPreviousCommand => field ??=
        new SimpleCommand(() => !IsBusy && _comparison.CanAdjustRightPrevious,
            (Func<Task>)(() => StepAsync(_comparison.AdjustRightPrevious, renderLeft: false)));

    /// <summary>Adjustment Down: only the right document to its next page.</summary>
    public SimpleCommand AdjustNextCommand => field ??=
        new SimpleCommand(() => !IsBusy && _comparison.CanAdjustRightNext,
            (Func<Task>)(() => StepAsync(_comparison.AdjustRightNext, renderLeft: false)));

    /// <summary>Zoom in: both panes one level closer, each keeping its position.</summary>
    public SimpleCommand ZoomInCommand => field ??=
        new SimpleCommand(() => !IsBusy && HasAnyDocument && View.Zoom.CanZoomIn,
            (Func<Task>)(() => ChangeZoomAsync(View.ZoomIn)));

    /// <summary>Zoom out: both panes one level further away, never past fit-the-page.</summary>
    public SimpleCommand ZoomOutCommand => field ??=
        new SimpleCommand(() => !IsBusy && HasAnyDocument && View.Zoom.CanZoomOut,
            (Func<Task>)(() => ChangeZoomAsync(View.ZoomOut)));

    /// <summary>Back to fit-the-page, both panes centred.</summary>
    public SimpleCommand ZoomResetCommand => field ??=
        new SimpleCommand(() => !IsBusy && HasAnyDocument && View.Zoom.IsZoomedIn,
            (Func<Task>)(() => ChangeZoomAsync(View.Reset)));

    /// <summary>
    /// Pans one pane one step; the parameter names the pane and direction as
    /// <c>"Left:Up"</c>, <c>"Right:Down"</c>, etc. Only enabled when zoomed in.
    /// </summary>
    public SimpleCommand PanCommand => field ??=
        new SimpleCommand(parameter => CanPan(parameter), parameter => DoPan(parameter));

    private bool CanPan(object parameter)
    {
        if (IsBusy || !TryParsePan(parameter, out var side, out var direction)) { return false; }
        return _comparison.GetDocument(side) != null && View.CanPan(side, direction);
    }

    private void DoPan(object parameter)
    {
        if (!TryParsePan(parameter, out var side, out var direction)) { return; }
        if (!View.Pan(side, direction)) { return; }
        ViewChanged();
    }

    private static bool TryParsePan(object parameter, out DocumentSide side, out PanDirection direction)
    {
        side = default;
        direction = default;
        if (parameter is not string text) { return false; }
        var parts = text.Split(':');
        return parts.Length == 2
            && Enum.TryParse(parts[0], ignoreCase: true, out side)
            && Enum.TryParse(parts[1], ignoreCase: true, out direction);
    }

    private async Task ChangeZoomAsync(Func<bool> change)
    {
        if (!change()) { return; }
        ViewChanged();

        //Re-render both pages at the resolution the new zoom wants; until each arrives the
        //  page scales the image it already has
        await Task.WhenAll(RenderSideAsync(DocumentSide.Left), RenderSideAsync(DocumentSide.Right));
    }

    private async Task StepAsync(Func<bool> move, bool renderLeft)
    {
        if (!move()) { return; }

        LeftPane.UpdatePageLabel(_comparison.Left);
        RightPane.UpdatePageLabel(_comparison.Right);
        UpdateStatus();
        ViewChanged(); //A page change comes back at fit-the-page (the library reset the view)

        //The right document moves on every step; the left only on a "both" step
        var renders = renderLeft
            ? Task.WhenAll(RenderSideAsync(DocumentSide.Left), RenderSideAsync(DocumentSide.Right))
            : RenderSideAsync(DocumentSide.Right);
        await renders;
    }

    private async Task BrowseAsync(DocumentSide side)
    {
        if (IsBusy) { return; }
        IsBusy = true;
        try
        {
            var path = await PickPdfPathAsync();
            if (path == null) { return; }

            var document = await _comparison.OpenAsync(side, path);
            PaneFor(side).ShowDocument(document);
            UpdateStatus();
            ViewChanged(); //Opening a document resets the view to fit-the-page
            await Task.WhenAll(RenderSideAsync(DocumentSide.Left), RenderSideAsync(DocumentSide.Right));
        }
        catch (DuplicateDocumentException e)
        {
            //The same file cannot be compared with itself; the pane keeps what it had
            await ShowError(e.Message);
        }
        catch (Exception e)
        {
            await ShowError(e, "Could not open the PDF document.");
        }
        finally
        {
            IsBusy = false;
        }
    }

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

    #endregion

    #region | Rendering |

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

    private DocumentPaneViewModel PaneFor(DocumentSide side) =>
        side == DocumentSide.Left ? LeftPane : RightPane;

    private void UpdateStatus()
    {
        StatusText = _comparison.IsReady
            ? $"Comparing {_comparison.Left.CurrentPage}:{_comparison.Right.CurrentPage}"
            : string.Empty;
    }

    private bool HasAnyDocument => _comparison.Left != null || _comparison.Right != null;

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

    #endregion
}
