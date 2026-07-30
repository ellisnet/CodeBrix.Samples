using System.Collections.Generic;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>A page included in the book: its title and its section's bookmark name.</summary>
internal sealed class BookPageRef
{
    /// <summary>The page title (used for "See …" cross-references).</summary>
    public string Title { get; init; } = "";

    /// <summary>The MigraDoc bookmark name of the page's chapter opening.</summary>
    public string BookmarkName { get; init; } = "";
}

/// <summary>
/// Everything the block renderer needs beyond the blocks themselves: the theme,
/// pre-downloaded media, which pages are in the book (for cross-references),
/// rendering options, and the shared warning/note sinks.
/// </summary>
internal sealed class RenderContext
{
    /// <summary>The trim-size-derived theme.</summary>
    public BookTheme Theme { get; init; }

    /// <summary>Print-ready media keyed by block ID (populated by the download pass).</summary>
    public IDictionary<string, PreparedMedia> MediaByBlockId { get; init; } =
        new Dictionary<string, PreparedMedia>();

    /// <summary>The pages included in the book, keyed by page ID.</summary>
    public IDictionary<string, BookPageRef> PagesInBook { get; init; } =
        new Dictionary<string, BookPageRef>();

    /// <summary>Row counts for child databases, keyed by database ID (optional decoration).</summary>
    public IDictionary<string, int> DatabaseRowCounts { get; init; } =
        new Dictionary<string, int>();

    /// <summary>When false, image blocks are skipped entirely.</summary>
    public bool IncludeImages { get; set; } = true;

    /// <summary>When false, video/audio media is not downloaded (cards render instead).</summary>
    public bool IncludeMedia { get; set; } = true;

    /// <summary>Non-fatal problems worth showing the user (surfaced in the result dialog).</summary>
    public IList<string> Warnings { get; init; } = new List<string>();

    /// <summary>Informational notes (logged, not shown as warnings).</summary>
    public IList<string> Notes { get; init; } = new List<string>();
}
