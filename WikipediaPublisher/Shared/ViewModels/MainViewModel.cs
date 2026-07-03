using CodeBrix.Platform.Simple;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

/// <summary>
/// Lets the hosting page give the view model a native "Save PDF as…" file dialog. Each head
/// wires this up with the file dialog appropriate to its UI stack (the CodeBrix.Platform
/// <c>FileSavePicker</c> on the Skia heads and native WinUI, and <c>SaveFileDialog</c> on WPF).
/// </summary>
public interface IFileSaveBridge
{
    /// <summary>
    /// Shows a "save PDF" dialog seeded with <paramref name="suggestedFileName"/> and returns the
    /// full path the user chose, or <c>null</c> if they cancelled. The head leaves this null when
    /// it has no file dialog (e.g. the Linux framebuffer head), in which case the user types the
    /// path directly into the box.
    /// Signature: <c>Func&lt;suggestedFileName, Task&lt;chosenPathOrNull&gt;&gt;</c>.
    /// </summary>
    Func<string, Task<string>> PickSavePdfPathAsync { get; set; }
}

#if HAS_CODEBRIX
[Microsoft.UI.Xaml.Data.Bindable]
#endif
public class MainViewModel : SimpleViewModel, IWebViewBridge, IFileSaveBridge
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

            StatusText = "Search for an article, browse to it, choose where to save the PDF, then click Publish.";
        }
    }

    /// <summary>
    /// A sensible default PDF file name for the "Save PDF as…" dialog: the current article's
    /// title when one is selected, otherwise a generic name.
    /// </summary>
    private string GetSuggestedFileName()
    {
        var name = "WikipediaPublisher";

        if (Uri.TryCreate((ArticleUrl ?? "").Trim(), UriKind.Absolute, out var uri))
        {
            const string wikiPathPrefix = "/wiki/";
            if (uri.AbsolutePath.StartsWith(wikiPathPrefix, StringComparison.Ordinal))
            {
                var title = Uri.UnescapeDataString(uri.AbsolutePath[wikiPathPrefix.Length..])
                    .Replace('_', ' ')
                    .Trim();
                if (title.Length > 0)
                {
                    var invalid = Path.GetInvalidFileNameChars();
                    name = new string(title.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
                }
            }
        }

        return $"{name}.pdf";
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

    private string _outputFilePath = string.Empty;
    [AffectsCommands(nameof(PublishCommand))]
    public string OutputFilePath
    {
        get => _outputFilePath;
        set => SetProperty(ref _outputFilePath, value ?? string.Empty);
    }

    /// <summary>Set by the hosting head (see <see cref="IFileSaveBridge"/>); null on heads with no file dialog.</summary>
    public Func<string, Task<string>> PickSavePdfPathAsync { get; set; }

    public List<string> PageSizeNames { get; } = new();

    private string _selectedPageSizeName = string.Empty;
    public string SelectedPageSizeName
    {
        get => _selectedPageSizeName;
        set => SetProperty(ref _selectedPageSizeName, value ?? string.Empty);
    }

    private bool _isBusy;
    [AffectsCommands(nameof(SearchCommand), nameof(PublishCommand), nameof(SelectOutputFileCommand))]
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

    #endregion

    #region | Commands and their implementations |

    #region SearchCommand

    private SimpleCommand _searchCommand;
    public SimpleCommand SearchCommand =>
        (_searchCommand ??= new SimpleCommand(CanSearch, DoSearch));

    private bool CanSearch() => (!IsBusy) && (!string.IsNullOrWhiteSpace(SearchTerms));

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

    #endregion

    #region SelectOutputFileCommand

    private SimpleCommand _selectOutputFileCommand;
    public SimpleCommand SelectOutputFileCommand =>
        (_selectOutputFileCommand ??= new SimpleCommand(CanSelectOutputFile, DoSelectOutputFile));

    private bool CanSelectOutputFile() => !IsBusy;

    private async Task DoSelectOutputFile()
    {
        if (!CanSelectOutputFile()) { return; }

        if (PickSavePdfPathAsync == null)
        {
            //No native file dialog on this head (e.g. the Linux framebuffer head) — the user
            //  types the destination path directly into the box instead.
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
            //Some heads (Linux framebuffer) register no picker — there is no window to host a dialog
            await ShowInfo(
                "File dialogs are not supported on this head. Type the full path (including the " +
                ".pdf file name) for the PDF into the “Save PDF to” box.");
        }
        catch (Exception e)
        {
            await ShowError($"Could not open the file dialog: {e.Message}");
        }
    }

    #endregion

    #region PublishCommand

    private SimpleCommand _publishCommand;
    public SimpleCommand PublishCommand =>
        (_publishCommand ??= new SimpleCommand(CanPublish, DoPublish));

    private bool CanPublish() =>
        (!IsBusy)
        && (!string.IsNullOrWhiteSpace(OutputFilePath))
        && IsPublishableArticleUrl(ArticleUrl);

    private async Task DoPublish()
    {
        if (!CanPublish()) { return; }

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
                OutputFilePath = outputPath,
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
            await ShowError($"Error while publishing: {e.Message}\n\nArticle URL: {ArticleUrl}");
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
        _selectOutputFileCommand?.Dispose();
        _selectOutputFileCommand = null;
        _publishCommand?.Dispose();
        _publishCommand = null;
        NavigateToUrl = null;
        PickSavePdfPathAsync = null;
        base.Dispose();
    }

    #endregion
}
