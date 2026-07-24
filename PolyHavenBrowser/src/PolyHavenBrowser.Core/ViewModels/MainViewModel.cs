using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeBrix.Platform.Simple;
using CodeBrix.Platform.WinUI.Graphics3DGL;
using Microsoft.UI.Xaml;
using Windows.Storage.Pickers;
using PolyHavenBrowser.PolyHavenApiClient;
using PolyHavenBrowser.Rendering;
using PolyHavenBrowser.Services;

// ReSharper disable once CheckNamespace
namespace PolyHavenBrowser.ViewModels;

/// <summary>One label/value row of the Model View's facts panel.</summary>
#if HAS_CODEBRIX
[Microsoft.UI.Xaml.Data.Bindable]
#endif
public sealed class ModelFact
{
    /// <summary>Creates a fact row.</summary>
    public ModelFact(string label, string value)
    {
        Label = label;
        Value = value;
    }

    /// <summary>The fact's label, e.g. <c>Triangles</c>.</summary>
    public string Label { get; }

    /// <summary>The fact's display value, e.g. <c>12,204</c>.</summary>
    public string Value { get; }
}

/// <summary>
/// Drives the whole PolyHavenBrowser main page. The page has two modes, toggled by
/// visibility: the <b>Browsing View</b> (a lazily-loading catalog grid of every Poly Haven
/// 3D model, with search, sorting and a download-folder picker) and the <b>Model View</b>
/// (everything the API knows about one downloaded model, beside an interactive OpenGL
/// preview the user can rotate and zoom).
/// </summary>
#if HAS_CODEBRIX
[Microsoft.UI.Xaml.Data.Bindable]
#endif
public class MainViewModel : SimpleViewModel
{
    private const string SortMostPopular = "Most popular";
    private const string SortNewest = "Newest";
    private const string SortNameAscending = "Name A–Z";

    private readonly ModelCatalogService _catalog;
    private readonly ModelDownloadService _downloads;

    private IReadOnlyList<PolyHavenAsset> _allModels = [];
    private ModelCellCollection _cells;
    private string _searchText = string.Empty;
    private string _selectedSortOption = SortMostPopular;
    private CancellationTokenSource _searchDebounce;
    private bool _isCatalogLoading = true;
    private string _catalogStatusText = "Loading the Poly Haven model catalog…";
    private string _resultCountText = string.Empty;

    private string _downloadFolder;
    private bool _isDownloading;
    private double _downloadProgress;
    private string _downloadStatusText = string.Empty;

    private bool _isModelViewActive;
    private LoadedModel _currentModel;
    private string _modelTitle = string.Empty;
    private string _modelAuthorLine = string.Empty;
    private string _modelDescription = string.Empty;
    private string _modelTagsText = string.Empty;

    /// <summary>Creates the view model and begins loading the model catalog.</summary>
    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; }

        _catalog = GetService<ModelCatalogService>();
        _downloads = GetService<ModelDownloadService>();

        _ = LoadCatalogAsync();
    }

    #region | Browsing View: catalog, search, sort |

    /// <summary>The lazily-loading catalog cells the grid displays.</summary>
    public ModelCellCollection Cells
    {
        get => _cells;
        private set => SetProperty(ref _cells, value);
    }

    /// <summary>Whether the initial catalog fetch is still in flight.</summary>
    public bool IsCatalogLoading
    {
        get => _isCatalogLoading;
        private set
        {
            SetProperty(ref _isCatalogLoading, value);
            NotifyPropertyChanged(nameof(CatalogLoadingVisibility));
        }
    }

    /// <summary>The visibility of the initial catalog-loading indicator.</summary>
    public Visibility CatalogLoadingVisibility => IsCatalogLoading ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>The status line shown while the catalog loads (or when it fails).</summary>
    public string CatalogStatusText
    {
        get => _catalogStatusText;
        private set => SetProperty(ref _catalogStatusText, value);
    }

    /// <summary>The result-count caption, e.g. <c>312 models</c>.</summary>
    public string ResultCountText
    {
        get => _resultCountText;
        private set => SetProperty(ref _resultCountText, value);
    }

    /// <summary>The search text; matching cells re-populate shortly after each keystroke.</summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            var newValue = value ?? string.Empty;
            if (newValue == _searchText) { return; }

            SetProperty(ref _searchText, newValue);
            DebounceRebuild();
        }
    }

    /// <summary>The sort options shown in the sort selector.</summary>
    public List<string> SortOptions { get; } = [SortMostPopular, SortNewest, SortNameAscending];

    /// <summary>The selected sort option; changing it re-populates the grid.</summary>
    public string SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (string.IsNullOrEmpty(value) || value == _selectedSortOption) { return; }

            SetProperty(ref _selectedSortOption, value);
            RebuildCells();
        }
    }

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

    //Re-applies search + sort and swaps in a fresh lazily-loading cell collection.
    private void RebuildCells()
    {
        if (_allModels.Count == 0 && IsCatalogLoading) { return; }

        var matching = ModelCatalogService.SortAndFilter(_allModels, SelectedSortOrder, SearchText);
        Cells = new ModelCellCollection(matching,
            asset => new ModelCellViewModel(asset, _catalog, DownloadAsync, () => !IsDownloading));
        ResultCountText = matching.Count == 1 ? "1 model" : $"{matching.Count:N0} models";
    }

    private CatalogSortOrder SelectedSortOrder => _selectedSortOption switch
    {
        SortNewest => CatalogSortOrder.Newest,
        SortNameAscending => CatalogSortOrder.NameAscending,
        _ => CatalogSortOrder.MostPopular,
    };

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

    #endregion

    #region | Download folder |

    /// <summary>Whether the user has chosen a download folder yet.</summary>
    public bool HasDownloadFolder => !string.IsNullOrWhiteSpace(_downloadFolder);

    /// <summary>The folder-picker button's caption: an invitation, or the chosen path.</summary>
    public string DownloadFolderLabel => HasDownloadFolder ? _downloadFolder : "Choose download folder…";

    private SimpleCommand _pickFolderCommand;

    /// <summary>Opens the folder picker to choose where models download to.</summary>
    public SimpleCommand PickFolderCommand => _pickFolderCommand ??=
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

        _downloadFolder = folder.Path;
        NotifyPropertyChanged(nameof(HasDownloadFolder));
        NotifyPropertyChanged(nameof(DownloadFolderLabel));
    }

    #endregion

    #region | Downloading |

    /// <summary>Whether a model download is in flight (drives the bottom progress bar).</summary>
    public bool IsDownloading
    {
        get => _isDownloading;
        private set
        {
            SetProperty(ref _isDownloading, value);
            NotifyPropertyChanged(nameof(DownloadBarVisibility));

            //The download gate lives on each cell's own command; tell every materialized
            //cell to re-query it. (Cells materialized later evaluate the gate fresh anyway.)
            if (Cells is { } cells)
            {
                foreach (var cell in cells) { cell.NotifyCanDownloadChanged(); }
            }
        }
    }

    /// <summary>The bottom progress bar's visibility.</summary>
    public Visibility DownloadBarVisibility => IsDownloading ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>The download progress in [0, 100].</summary>
    public double DownloadProgress
    {
        get => _downloadProgress;
        private set
        {
            //No SetProperty overload takes a double; compare-and-notify by hand.
            if (_downloadProgress.Equals(value)) { return; }
            _downloadProgress = value;
            NotifyPropertyChanged(nameof(DownloadProgress));
        }
    }

    /// <summary>The caption beside the bottom progress bar, e.g. the downloading model's name.</summary>
    public string DownloadStatusText
    {
        get => _downloadStatusText;
        private set => SetProperty(ref _downloadStatusText, value);
    }

    //Each cell owns its Download command; this is the shared implementation the cells'
    //commands delegate to (see the cell factory in RebuildCells). With no download folder
    //chosen yet, explains itself with a dialog instead.
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

        IsDownloading = true;
        DownloadProgress = 0;
        DownloadStatusText = $"Downloading “{cell.Title}”…";
        try
        {
            var progress = new Progress<double>(fraction => DownloadProgress = fraction * 100d);
            var downloaded = await _downloads.EnsureDownloadedAsync(
                cell.Asset, _downloadFolder, progress, CancellationToken.None);

            await OpenModelViewAsync(cell.Asset, downloaded);
        }
        catch (Exception ex)
        {
            await ShowError(ex, $"Could not download “{cell.Title}”.");
        }
        finally
        {
            IsDownloading = false;
            DownloadStatusText = string.Empty;
        }
    }

    #endregion

    #region | Model View |

    /// <summary>Whether the Model View is active (otherwise the Browsing View shows).</summary>
    public bool IsModelViewActive
    {
        get => _isModelViewActive;
        private set
        {
            SetProperty(ref _isModelViewActive, value);
            NotifyPropertyChanged(nameof(BrowsingViewVisibility));
            NotifyPropertyChanged(nameof(ModelViewVisibility));
        }
    }

    /// <summary>The Browsing View's visibility.</summary>
    public Visibility BrowsingViewVisibility => IsModelViewActive ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>The Model View's visibility.</summary>
    public Visibility ModelViewVisibility => IsModelViewActive ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>The model shown in the 3D preview (null while browsing); the preview control binds to this.</summary>
    public LoadedModel CurrentModel => _currentModel;

    /// <summary>The Model View's title (the model's name).</summary>
    public string ModelTitle
    {
        get => _modelTitle;
        private set => SetProperty(ref _modelTitle, value);
    }

    /// <summary>The creator credit line under the title.</summary>
    public string ModelAuthorLine
    {
        get => _modelAuthorLine;
        private set => SetProperty(ref _modelAuthorLine, value);
    }

    /// <summary>The full (synthesized) description paragraph.</summary>
    public string ModelDescription
    {
        get => _modelDescription;
        private set => SetProperty(ref _modelDescription, value);
    }

    /// <summary>The model's tags as one flowing line.</summary>
    public string ModelTagsText
    {
        get => _modelTagsText;
        private set => SetProperty(ref _modelTagsText, value);
    }

    /// <summary>The label/value fact rows shown beside the 3D preview.</summary>
    public ObservableCollection<ModelFact> ModelFacts { get; } = new();

    /// <summary>How to drive the 3D preview, shown under the canvas.</summary>
    public string ViewerHint => "drag to rotate · scroll to zoom";

    private SimpleCommand _backCommand;

    /// <summary>Returns from the Model View to the Browsing View.</summary>
    public SimpleCommand BackCommand => _backCommand ??=
        new SimpleCommand((Func<object, Task>)(_ => { CloseModelView(); return Task.CompletedTask; }));

    private SimpleCommand _documentCommand;

    /// <summary>
    /// Reserved for a future release: creating a PDF document about the model. Inactive in 1.0
    /// (its can-execute is always false, so the button stays disabled).
    /// </summary>
    public SimpleCommand DocumentCommand => _documentCommand ??=
        new SimpleCommand(() => false, (Func<object, Task>)(_ => Task.CompletedTask));

    private async Task OpenModelViewAsync(PolyHavenAsset asset, DownloadedModel downloaded)
    {
        DownloadStatusText = $"Loading “{asset.Name ?? downloaded.Slug}”…";

        //Parse the glTF and gather its stats off the UI thread; GPU upload happens lazily
        //at first paint.
        var (model, stats) = await Task.Run(() =>
        {
            var loaded = new GltfModelLoader().LoadFile(downloaded.GltfPath);
            return (loaded, ModelFileStats.FromLoadedModel(loaded, downloaded.ModelFolder));
        });

        // Hand the model to the preview control via its bound CurrentModel; the control frames
        // the camera and repaints itself. The GPU upload happens lazily at its first render.
        _currentModel = model;

        ModelTitle = string.IsNullOrWhiteSpace(asset.Name) ? downloaded.Slug : asset.Name;
        ModelAuthorLine = BuildAuthorLine(asset);
        ModelDescription = ModelDescriptionBuilder.BuildFullDescription(asset, stats);
        ModelTagsText = string.Join("   ", (asset.Tags ?? []).Select(t => $"#{t}"));
        PopulateFacts(asset, stats);

        NotifyPropertyChanged(nameof(CurrentModel));
        IsModelViewActive = true;
    }

    private void CloseModelView()
    {
        IsModelViewActive = false;

        _currentModel = null;
        NotifyPropertyChanged(nameof(CurrentModel));
    }

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

    private void PopulateFacts(PolyHavenAsset asset, ModelFileStats stats)
    {
        ModelFacts.Clear();

        if (asset.Categories is { Length: > 0 })
        {
            ModelFacts.Add(new ModelFact("Categories", string.Join(", ", asset.Categories)));
        }
        ModelFacts.Add(new ModelFact("Published", asset.DatePublishedUtc.ToString("MMMM d, yyyy")));
        ModelFacts.Add(new ModelFact("Downloads", $"{asset.DownloadCount:N0}"));
        if (asset.MaxResolution is { Length: > 0 })
        {
            ModelFacts.Add(new ModelFact("Max texture size", string.Join(" × ", asset.MaxResolution)));
        }
        ModelFacts.Add(new ModelFact("Triangles", $"{stats.Triangles:N0}"));
        ModelFacts.Add(new ModelFact("Vertices", $"{stats.Vertices:N0}"));
        ModelFacts.Add(new ModelFact("Materials",
            stats.TexturedMaterials > 0 ? $"{stats.Materials:N0} ({stats.TexturedMaterials:N0} textured)" : $"{stats.Materials:N0}"));
        ModelFacts.Add(new ModelFact("Size on disk", ModelDescriptionBuilder.FormatBytes(stats.DiskBytes)));
        ModelFacts.Add(new ModelFact("License", "CC0 (public domain)"));
    }

    private static string BuildAuthorLine(PolyHavenAsset asset)
    {
        var authors = asset.Authors?.Keys.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray() ?? [];
        return authors.Length == 0 ? "from Poly Haven" : $"by {string.Join(", ", authors)}   ·   Poly Haven";
    }

    #endregion
}
