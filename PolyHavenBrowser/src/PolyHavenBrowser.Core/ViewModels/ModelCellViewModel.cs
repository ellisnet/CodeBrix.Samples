using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using PolyHavenBrowser.PolyHavenApiClient;
using PolyHavenBrowser.Services;

// ReSharper disable once CheckNamespace
namespace PolyHavenBrowser.ViewModels;

/// <summary>
/// One catalog cell of the Browsing View: a single Poly Haven model's hero thumbnail,
/// title, creator credit, short description and download stats. Cells are materialized
/// lazily as the user scrolls, and each cell fetches its own thumbnail when created.
/// </summary>
#if HAS_CODEBRIX
[Microsoft.UI.Xaml.Data.Bindable]
#endif
public class ModelCellViewModel : SimpleViewModel
{
    private readonly ModelCatalogService _catalog;
    private ImageSource _thumbnail;
    private bool _thumbnailFailed;

    /// <summary>Creates a cell for one model of the catalog.</summary>
    public ModelCellViewModel(PolyHavenAsset asset, ModelCatalogService catalog)
    {
        Asset = asset ?? throw new ArgumentNullException(nameof(asset));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));

        Title = string.IsNullOrWhiteSpace(asset.Name) ? asset.Id : asset.Name;
        AuthorCredit = BuildAuthorCredit(asset);
        Blurb = ModelDescriptionBuilder.BuildCellBlurb(asset);
        DownloadsText = ModelDescriptionBuilder.FormatCompactCount(asset.DownloadCount);
        CategoriesText = BuildCategoriesText(asset);
    }

    /// <summary>The Poly Haven asset this cell represents.</summary>
    public PolyHavenAsset Asset { get; }

    /// <summary>The model's display name.</summary>
    public string Title { get; }

    /// <summary>The creator credit line, e.g. <c>by Rico Cilliers</c>.</summary>
    public string AuthorCredit { get; }

    /// <summary>The short description shown under the title.</summary>
    public string Blurb { get; }

    /// <summary>The compact download count, e.g. <c>44.1k</c>.</summary>
    public string DownloadsText { get; }

    /// <summary>The model's categories, dot-separated, e.g. <c>furniture · props</c>.</summary>
    public string CategoriesText { get; }

    /// <summary>The hero thumbnail, populated asynchronously after the cell appears.</summary>
    public ImageSource Thumbnail
    {
        get => _thumbnail;
        private set => SetProperty(ref _thumbnail, value);
    }

    /// <summary>
    /// Fetches the cell's thumbnail (through the catalog service's cache) and hands it to the
    /// Image control. Failures leave the placeholder showing; scrolling back re-tries once more.
    /// </summary>
    public async Task LoadThumbnailAsync()
    {
        if (_thumbnail != null || _thumbnailFailed) { return; }

        try
        {
            var bytes = await _catalog.GetThumbnailAsync(Asset, System.Threading.CancellationToken.None);

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

    private static string BuildAuthorCredit(PolyHavenAsset asset)
    {
        var authors = asset.Authors?.Keys.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray() ?? [];
        return authors.Length == 0 ? "Poly Haven" : string.Join(", ", authors);
    }

    private static string BuildCategoriesText(PolyHavenAsset asset)
    {
        var categories = (asset.Categories ?? [])
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Take(2)
            .ToArray();
        return string.Join(" · ", categories);
    }
}
