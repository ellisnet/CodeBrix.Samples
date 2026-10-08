using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CodeBrix.Platform.Simple;
using GoddessTempleDiscovery.Assets;
using GoddessTempleDiscovery.Game.Rendering;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace GoddessTempleDiscovery.ViewModels;

/// <summary>One thumbnail of the gallery.</summary>
[Microsoft.UI.Xaml.Data.Bindable]
public class GalleryItem : SimpleViewModel
{
    private readonly Action<GalleryItem> _open;

    /// <summary>Creates the thumbnail.</summary>
    /// <param name="info">The picture.</param>
    /// <param name="open">Opens it large.</param>
    public GalleryItem(ArtInfo info, Action<GalleryItem> open)
    {
        Info = info;
        _open = open;
    }

    /// <summary>The picture.</summary>
    public ArtInfo Info { get; }

    /// <summary>The art key.</summary>
    public string Key => Info.Key;

    /// <summary>The picture's title.</summary>
    public string Title => Info.Title;

    /// <summary>The gallery category.</summary>
    public string Category => Info.Category;

    /// <summary>The thumbnail, once rendered.</summary>
    public BitmapImage Image
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Opens the picture large.</summary>
    public SimpleCommand OpenCommand => field ??= new SimpleCommand(() => _open?.Invoke(this));
}

public partial class MainViewModel
{
    /// <summary>The thumbnails on one page of the gallery.</summary>
    public const int GalleryPageSize = 24;

    private const string AllCategories = "everything";
    private const int ThumbnailSize = 150;

    private readonly Dictionary<string, BitmapImage> _thumbnails = new Dictionary<string, BitmapImage>(StringComparer.Ordinal);
    private IReadOnlyList<ArtInfo> _art;
    private int _galleryPage;

    #region | The gallery of the game's art |

    /// <summary>True while the gallery is open.</summary>
    [AffectsProperties(nameof(GalleryVisibility))]
    public bool IsGalleryOpen
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>The gallery pane.</summary>
    public Visibility GalleryVisibility => GetVisibility(IsGalleryOpen);

    /// <summary>The category choices: everything, then the catalog's categories.</summary>
    public IReadOnlyList<string> GalleryCategories { get; } = new[] { AllCategories }.Concat(ArtInfo.Categories).ToArray();

    /// <summary>The chosen category.</summary>
    public string GalleryCategory
    {
        get;
        set
        {
            SetProperty(ref field, value ?? AllCategories);
            _galleryPage = 0;
            ShowGalleryPage();
        }
    } = AllCategories;

    /// <summary>The thumbnails of the current page.</summary>
    public ObservableCollection<GalleryItem> GalleryItems { get; } = new ObservableCollection<GalleryItem>();

    /// <summary>"Page 2 of 11 · 263 pictures".</summary>
    public string GalleryPageText
    {
        get;
        private set => SetProperty(ref field, value ?? string.Empty);
    } = string.Empty;

    /// <summary>Opens the gallery.</summary>
    public SimpleCommand GalleryCommand => field ??= new SimpleCommand(OpenGallery);

    /// <summary>Closes the gallery.</summary>
    public SimpleCommand CloseGalleryCommand => field ??= new SimpleCommand(() =>
    {
        IsGalleryLargeOpen = false;
        IsGalleryOpen = false;
    });

    /// <summary>The previous page.</summary>
    public SimpleCommand PreviousGalleryPageCommand => field ??= new SimpleCommand(() => PageGallery(-1));

    /// <summary>The next page.</summary>
    public SimpleCommand NextGalleryPageCommand => field ??= new SimpleCommand(() => PageGallery(1));

    /// <summary>True while one picture is shown large.</summary>
    [AffectsProperties(nameof(GalleryLargeVisibility))]
    public bool IsGalleryLargeOpen
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>The large view.</summary>
    public Visibility GalleryLargeVisibility => GetVisibility(IsGalleryLargeOpen);

    /// <summary>The picture shown large.</summary>
    public BitmapImage GalleryLargeImage { get => field; private set => SetProperty(ref field, value); }

    /// <summary>Its title.</summary>
    public string GalleryLargeTitle { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>Its key and category.</summary>
    public string GalleryLargeKey { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>Its subject line.</summary>
    public string GalleryLargeSubject { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>Its sources line.</summary>
    public string GalleryLargeSources { get => field; private set => SetProperty(ref field, value ?? string.Empty); } = string.Empty;

    /// <summary>The art's license line.</summary>
    public string GalleryLicense => ArtCatalog.LicenseText;

    /// <summary>Closes the large view.</summary>
    public SimpleCommand CloseGalleryLargeCommand => field ??= new SimpleCommand(() => IsGalleryLargeOpen = false);

    /// <inheritdoc />
    public bool HandleKey(Windows.System.VirtualKey key)
    {
        switch (key)
        {
            case Windows.System.VirtualKey.Escape:
                return CloseTopmost();
            case Windows.System.VirtualKey.Left or Windows.System.VirtualKey.PageUp when IsGalleryOpen && !IsGalleryLargeOpen:
                PageGallery(-1);
                return true;
            case Windows.System.VirtualKey.Right or Windows.System.VirtualKey.PageDown when IsGalleryOpen && !IsGalleryLargeOpen:
                PageGallery(1);
                return true;
            default:
                return false;
        }
    }

    //Escape closes what is on top: the gallery's large view, the gallery, the newspaper, the journal, the settings
    private bool CloseTopmost()
    {
        if (IsGalleryLargeOpen)
        {
            IsGalleryLargeOpen = false;
        }
        else if (IsGalleryOpen)
        {
            IsGalleryOpen = false;
        }
        else if (IsInspectorOpen)
        {
            CloseInspector();
        }
        else if (IsJournalOpen)
        {
            IsJournalOpen = false;
        }
        else if (IsSettingsOpen)
        {
            IsSettingsOpen = false;
        }
        else
        {
            Host?.NotifyInspectorClosed();
            return false;
        }

        return true;
    }

    private void OpenGallery()
    {
        _art ??= ArtInfo.All();
        IsGalleryLargeOpen = false;
        IsGalleryOpen = true;
        ShowGalleryPage();
    }

    private IReadOnlyList<ArtInfo> GalleryFiltered() =>
        (_art ??= ArtInfo.All()).Where(a => GalleryCategory == AllCategories || a.Category == GalleryCategory).ToArray();

    private void PageGallery(int delta)
    {
        var pages = Math.Max(1, (GalleryFiltered().Count + GalleryPageSize - 1) / GalleryPageSize);
        _galleryPage = ((_galleryPage + delta) % pages + pages) % pages;
        ShowGalleryPage();
    }

    private void ShowGalleryPage()
    {
        if (!IsGalleryOpen)
        {
            return;
        }

        var filtered = GalleryFiltered();
        var pages = Math.Max(1, (filtered.Count + GalleryPageSize - 1) / GalleryPageSize);
        _galleryPage = Math.Clamp(_galleryPage, 0, pages - 1);
        GalleryItems.Clear();
        foreach (var info in filtered.Skip(_galleryPage * GalleryPageSize).Take(GalleryPageSize))
        {
            var item = new GalleryItem(info, ShowLarge);
            GalleryItems.Add(item);
            _ = LoadThumbnailAsync(item);
        }

        GalleryPageText = string.Format(CultureInfo.InvariantCulture, "Page {0} of {1} · {2} pictures", _galleryPage + 1, pages, filtered.Count);
    }

    //Rendered the first time a thumbnail is shown, then kept for the rest of the run
    private async Task LoadThumbnailAsync(GalleryItem item)
    {
        if (_thumbnails.TryGetValue(item.Key, out var cached))
        {
            item.Image = cached;
            return;
        }

        try
        {
            var bytes = await Task.Run(() => SvgRaster.RenderArtPng(item.Key, ThumbnailSize));
            var image = await ImageFromAsync(bytes);
            _thumbnails[item.Key] = image;
            item.Image = image;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Goddess Temple Discovery: the thumbnail {item.Key} could not be rendered: {ex.Message}");
        }
    }

    private void ShowLarge(GalleryItem item)
    {
        if (item == null)
        {
            return;
        }

        GalleryLargeTitle = item.Info.Title;
        GalleryLargeKey = item.Key + " · " + item.Category;
        GalleryLargeSubject = item.Info.Subject;
        GalleryLargeSources = item.Info.Sources;
        GalleryLargeImage = item.Image;
        IsGalleryLargeOpen = true;
        _ = LoadLargeAsync(item.Key);
    }

    private async Task LoadLargeAsync(string key)
    {
        try
        {
            var bytes = await Task.Run(() => SvgRaster.RenderArtPng(key, 700));
            var image = await ImageFromAsync(bytes);
            if (IsGalleryLargeOpen && GalleryLargeKey.StartsWith(key + " ", StringComparison.Ordinal))
            {
                GalleryLargeImage = image;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Goddess Temple Discovery: the picture {key} could not be rendered: {ex.Message}");
        }
    }

    #endregion
}
