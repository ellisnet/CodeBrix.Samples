using CodeBrix.Platform.Simple;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WikipediaPublisher.Helpers;
using WikipediaPublisher.RenderArticle.Models;
using WikipediaPublisher.RenderArticle.Services;

// ReSharper disable once CheckNamespace
namespace WikipediaPublisher.ViewModels;

/// <summary>
/// Lets the hosting page hand the view model a way to drive the embedded
/// WebView browser (only wired up on heads that have one).
/// </summary>
public interface IWebViewBridge
{
    /// <summary>Navigates the embedded browser to the given URL (null when no WebView).</summary>
    Action<string> NavigateToUrl { get; set; }

    /// <summary>Called by the page whenever the embedded browser lands on a new URL.</summary>
    void SetCurrentBrowserUrl(string url);
}

#if HAS_CODEBRIX
[Microsoft.UI.Xaml.Data.Bindable]
#endif
public class MainViewModel : SimpleViewModel, IWebViewBridge
{
    public const string HomeUrl = "https://en.wikipedia.org/wiki/Main_Page";

    private const string WikiHost = "en.wikipedia.org";

    //Wikipedia namespace prefixes that are not printable articles
    private static readonly string[] NonArticlePrefixes =
    [
        "Special:", "File:", "Category:", "Help:", "Wikipedia:", "Talk:",
        "Template:", "Template_talk:", "Portal:", "Draft:", "Module:", "MediaWiki:",
        "Main_Page"
    ];

    private IArticleRenderService _renderSvc;
    private List<ArticleSearchResult> _searchResults = [];

    public MainViewModel()
    {
        if (!IsDesignMode(true))
        {
            Debug.WriteLine("Main view model startup.");

            _renderSvc = GetService<IArticleRenderService>();

            PageSizeNames.Clear();
            foreach (var info in PageSizeInfo.All)
            {
                PageSizeNames.Add(info.DisplayName);
            }
            _selectedPageSizeName = PageSizeInfo.All[0].DisplayName;
            base.NotifyPropertyChanged(nameof(PageSizeNames));
            base.NotifyPropertyChanged(nameof(SelectedPageSizeName));

            _outputFolder = GetDefaultOutputFolder();
            base.NotifyPropertyChanged(nameof(OutputFolder));

            StatusText = AppCapabilities.HasWebView
                ? "Search for an article, browse to it, then click Publish."
                : "Search for an article, pick it from the results, then click Publish.";
        }
    }

    private static string GetDefaultOutputFolder()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrWhiteSpace(documents))
        {
            documents = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        return Path.Combine(documents, "WikipediaPublisher");
    }

    #region | Bindable properties |

    private string _searchTerms = string.Empty;
    [AffectsCommands(nameof(SearchCommand))]
    public string SearchTerms
    {
        get => _searchTerms;
        set => SetProperty(ref _searchTerms, value ?? string.Empty);
    }

    private string _articleUrl = string.Empty;
    [AffectsCommands(nameof(PublishCommand))]
    public string ArticleUrl
    {
        get => _articleUrl;
        set => SetProperty(ref _articleUrl, value ?? string.Empty);
    }

    private string _outputFolder = string.Empty;
    [AffectsCommands(nameof(PublishCommand))]
    public string OutputFolder
    {
        get => _outputFolder;
        set => SetProperty(ref _outputFolder, value ?? string.Empty);
    }

    public List<string> PageSizeNames { get; } = new();

    private string _selectedPageSizeName = string.Empty;
    public string SelectedPageSizeName
    {
        get => _selectedPageSizeName;
        set => SetProperty(ref _selectedPageSizeName, value ?? string.Empty);
    }

    //ObservableCollection so the results ListView refreshes as results arrive
    public System.Collections.ObjectModel.ObservableCollection<string> SearchResultItems { get; } = new();

    private string _selectedSearchResultItem = string.Empty;
    public string SelectedSearchResultItem
    {
        get => _selectedSearchResultItem;
        set
        {
            SetProperty(ref _selectedSearchResultItem, value ?? string.Empty);

            var index = SearchResultItems.IndexOf(_selectedSearchResultItem);
            if (index >= 0 && index < _searchResults.Count)
            {
                ArticleUrl = _searchResults[index].ArticleUrl;
                StatusText = $"Selected: {_searchResults[index].Title}";
            }
        }
    }

    private bool _isBusy;
    [AffectsCommands(nameof(SearchCommand), nameof(PublishCommand))]
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value ?? string.Empty);
    }

    private int _progressValue;
    public int ProgressValue
    {
        get => _progressValue;
        set => SetProperty(ref _progressValue, value);
    }

    public bool HasWebView => AppCapabilities.HasWebView;

    #endregion

    #region | Commands and their implementations |

    #region SearchCommand

    private SimpleCommand _searchCommand;
    public SimpleCommand SearchCommand =>
        (_searchCommand ??= new SimpleCommand(CanSearch, DoSearch));

    private bool CanSearch() => (!IsBusy) && (!string.IsNullOrWhiteSpace(SearchTerms));

    private async Task DoSearch()
    {
        if (!CanSearch()) { return; }

        if (AppCapabilities.HasWebView && NavigateToUrl != null)
        {
            //Browse the real Wikipedia search page; the user picks an article by
            //  navigating to it, and Publish uses whatever page is displayed
            var searchUrl =
                $"https://{WikiHost}/w/index.php?search={Uri.EscapeDataString(SearchTerms.Trim())}";
            InvokeOnMainThread(() => NavigateToUrl(searchUrl));
            StatusText = "Browse to the article you want, then click Publish.";
            return;
        }

        //No WebView on this head: use the MediaWiki search API and a results list
        try
        {
            IsBusy = true;
            StatusText = $"Searching for “{SearchTerms.Trim()}”…";

            var results = await _renderSvc.SearchArticlesAsync(SearchTerms.Trim());

            InvokeOnMainThread(() =>
            {
                _searchResults = results.ToList();
                SearchResultItems.Clear();
                foreach (var result in _searchResults)
                {
                    var snippet = string.IsNullOrWhiteSpace(result.Snippet)
                        ? ""
                        : $" — {result.Snippet}";
                    SearchResultItems.Add($"{result.Title}{snippet}");
                }
                StatusText = _searchResults.Count == 0
                    ? "No articles found — try different search terms."
                    : $"Found {_searchResults.Count} articles — select one, then click Publish.";
            });
        }
        catch (Exception e)
        {
            await ShowError($"Error while searching: {e.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #endregion

    #region PublishCommand

    private SimpleCommand _publishCommand;
    public SimpleCommand PublishCommand =>
        (_publishCommand ??= new SimpleCommand(CanPublish, DoPublish));

    private bool CanPublish() =>
        (!IsBusy)
        && (!string.IsNullOrWhiteSpace(OutputFolder))
        && IsPublishableArticleUrl(ArticleUrl);

    private async Task DoPublish()
    {
        if (!CanPublish()) { return; }

        try
        {
            IsBusy = true;
            ProgressValue = 0;

            var pageSize = PageSizeInfo.All
                .FirstOrDefault(p => p.DisplayName == SelectedPageSizeName)?.Option
                ?? PageSizeOption.EightByTen;

            var request = new RenderRequest
            {
                ArticleUrl = ArticleUrl.Trim(),
                OutputDirectory = OutputFolder.Trim(),
                PageSize = pageSize
            };

            var progress = new Progress<RenderProgress>(p => InvokeOnMainThread(() =>
            {
                StatusText = p.Message;
                ProgressValue = p.PercentComplete;
            }));

            var result = await _renderSvc.RenderArticleAsync(request, progress);

            StatusText = $"Saved: {result.OutputFilePath}";
            await ShowInfo(
                $"Published “{result.Title}”\n\n" +
                $"{result.PageCount} pages · {result.ImageCount} images · " +
                $"{result.Elapsed.TotalSeconds:F0} seconds\n\n" +
                $"Saved to: {result.OutputFilePath}");
        }
        catch (Exception e)
        {
            StatusText = "Publishing failed.";
            await ShowError($"Error while publishing: {e.Message}");
        }
        finally
        {
            ProgressValue = 0;
            IsBusy = false;
        }
    }

    /// <summary>True when the URL points at a printable Wikipedia article page.</summary>
    public static bool IsPublishableArticleUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) { return false; }
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)) { return false; }

        const string wikiPathPrefix = "/wiki/";
        if (!uri.AbsolutePath.StartsWith(wikiPathPrefix, StringComparison.Ordinal)) { return false; }

        var title = uri.AbsolutePath[wikiPathPrefix.Length..];
        if (title.Length == 0) { return false; }

        return !NonArticlePrefixes.Any(prefix =>
            title.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #endregion

    #region | IWebViewBridge implementation |

    public Action<string> NavigateToUrl { get; set; }

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

    #endregion

    #region | IDisposable implementation |

    public override void Dispose()
    {
        _renderSvc = null;
        _searchCommand?.Dispose();
        _searchCommand = null;
        _publishCommand?.Dispose();
        _publishCommand = null;
        NavigateToUrl = null;
        base.Dispose();
    }

    #endregion
}
