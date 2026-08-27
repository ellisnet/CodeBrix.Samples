using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using PdfSideBySide.PdfRender.Documents;
using PdfSideBySide.PdfRender.Rendering;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PdfSideBySide.ViewModels;

/// <summary>
/// One of the two document panes: which PDF it shows, the page its cursor is on, and the
/// rendered image of that page. The <see cref="MainViewModel"/> owns the comparison and pushes
/// state in here; the pane only exposes it for binding.
/// </summary>
[Microsoft.UI.Xaml.Data.Bindable]
public class DocumentPaneViewModel : SimpleViewModel
{
    private const string NoDocumentText = "No document selected";

    public DocumentPaneViewModel(string title, Func<Task> browse)
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Title = title;
        BrowseCommand = new SimpleCommand(browse);
    }

    #region | Bindable properties |

    /// <summary>The pane's caption - "Document 1" or "Document 2".</summary>
    public string Title { get; }

    /// <summary>The browse button doubles as the pane's label: "Document 1…".</summary>
    public string BrowseLabel => $"{Title}…";

    /// <summary>Full path of the PDF shown in this pane, or empty when none is selected.</summary>
    public string FilePath
    {
        get;
        private set
        {
            SetProperty(ref field, value ?? string.Empty);
            NotifyPropertyChanged(nameof(FileName));
            NotifyPropertyChanged(nameof(HasDocument));
            NotifyPropertyChanged(nameof(PlaceholderVisibility));
        }
    } = string.Empty;

    /// <summary>The file name of the selected PDF, or a "none selected" hint.</summary>
    public string FileName => HasDocument ? Path.GetFileName(FilePath) : NoDocumentText;

    /// <summary>Whether a PDF is selected for this pane.</summary>
    public bool HasDocument => FilePath.Length > 0;

    /// <summary>Shows the empty-pane placeholder until a document is selected.</summary>
    public Visibility PlaceholderVisibility => HasDocument ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>"Page n of N" for the page the cursor is on; empty when no document is selected.</summary>
    public string PageLabel
    {
        get;
        private set => SetProperty(ref field, value ?? string.Empty);
    } = string.Empty;

    /// <summary>The rendered current page, or <c>null</c> while nothing has been rendered.</summary>
    public BitmapImage PageImage
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Pixel width of <see cref="PageImage"/> (0 when there is none); the page uses the aspect to size the view.</summary>
    public int PagePixelWidth
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Pixel height of <see cref="PageImage"/> (0 when there is none).</summary>
    public int PagePixelHeight
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Whether a page render is in flight for this pane (drives the busy bar).</summary>
    public bool IsRendering
    {
        get;
        private set
        {
            SetProperty(ref field, value);
            NotifyPropertyChanged(nameof(RenderingVisibility));
        }
    }

    /// <summary>Shows the busy bar while <see cref="IsRendering"/>.</summary>
    public Visibility RenderingVisibility => IsRendering ? Visibility.Visible : Visibility.Collapsed;

    #endregion

    #region | Commands and their implementations |

    /// <summary>Opens the file picker for this pane; the main view model supplies the work.</summary>
    public SimpleCommand BrowseCommand { get; }

    #endregion

    #region | Updates pushed by the main view model |

    /// <summary>Shows document (or clears the pane when it is <c>null</c>).</summary>
    internal void ShowDocument(PdfPageDocument document)
    {
        FilePath = document?.FilePath;
        PagePixelWidth = 0;
        PagePixelHeight = 0;
        PageImage = null;
        UpdatePageLabel(document);
    }

    /// <summary>Refreshes the page label after document's cursor moved.</summary>
    internal void UpdatePageLabel(PdfPageDocument document)
    {
        PageLabel = document == null ? string.Empty : $"Page {document.CurrentPage} of {document.PageCount}";
    }

    /// <summary>Flags whether a render is in flight.</summary>
    internal void SetRendering(bool isRendering) => IsRendering = isRendering;

    /// <summary>Decodes page's PNG into the pane's image. Must be called on the UI thread.</summary>
    internal async Task ShowPageAsync(RenderedPage page)
    {
        var image = new BitmapImage();
        using (var stream = new MemoryStream(page.PngBytes))
        {
            await image.SetSourceAsync(stream.AsRandomAccessStream());
        }
        PagePixelWidth = page.PixelWidth;
        PagePixelHeight = page.PixelHeight;
        PageImage = image; //Last, so a listener sees the size when the image changes
    }

    #endregion
}
