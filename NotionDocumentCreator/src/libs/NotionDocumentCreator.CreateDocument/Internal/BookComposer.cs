using CodeBrix.PdfDocCreate.DocumentObjectModel;
using CodeBrix.PdfDocuments.Drawing;
using System;
using System.Collections.Generic;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// Everything one selected Notion page contributes to the book: its identity,
/// its fetched block tree, and its tree path (for breadcrumbs).
/// </summary>
internal sealed class ChapterContent
{
    /// <summary>The Notion page ID.</summary>
    public string PageId { get; init; } = "";

    /// <summary>The page title (the chapter title; the first chapter's is the book title).</summary>
    public string Title { get; init; } = "";

    /// <summary>The page icon when it is an emoji; empty otherwise.</summary>
    public string IconEmoji { get; init; } = "";

    /// <summary>The page's cover image URL; empty when the page has no cover.</summary>
    public string CoverUrl { get; init; } = "";

    /// <summary>Titles along the tree path from the root down to this page.</summary>
    public IReadOnlyList<string> AncestorTitles { get; init; } = [];

    /// <summary>The page's full block tree.</summary>
    public IReadOnlyList<NotionBlockNode> Blocks { get; init; } = [];
}

/// <summary>
/// Composes the selected chapters into a book-designed MigraDoc document: an
/// unnumbered cover section built from the first page, then one section per
/// page — each starting on a fresh page with running heads and continuous
/// folios that begin at 1 after the cover and never restart.
/// </summary>
internal sealed class BookComposer
{
    private readonly IReadOnlyList<ChapterContent> _chapters;
    private readonly RenderContext _context;
    private readonly BookTheme _theme;
    private BlockRenderer _renderer;

    public BookComposer(IReadOnlyList<ChapterContent> chapters, RenderContext context)
    {
        _chapters = chapters ?? throw new ArgumentNullException(nameof(chapters));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _theme = context.Theme ?? throw new ArgumentException("The context needs a theme.", nameof(context));
    }

    /// <summary>Total images placed (cover image plus in-chapter figures).</summary>
    public int PlacedImageCount { get; private set; }

    /// <summary>Total characters dropped because no embedded font covers them.</summary>
    public int DroppedCharacterCount => _renderer?.DroppedCharacterCount ?? 0;

    /// <summary>Builds the complete document.</summary>
    public Document Compose()
    {
        if (_chapters.Count == 0)
        {
            throw new InvalidOperationException("At least one page must be selected for the book.");
        }

        BookFonts.EnsureRegistered();

        var document = new Document();
        document.Info.Title = _chapters[0].Title;
        document.Info.Subject = "A book created from Notion pages";

        BookStyles.Define(document, _theme);
        _renderer = new BlockRenderer(_context);

        ComposeCover(document, _chapters[0]);
        for (var i = 1; i < _chapters.Count; i++)
        {
            ComposeChapter(document, _chapters[i], isFirstContentChapter: i == 1);
        }

        PlacedImageCount += _renderer.FigureCount;
        return document;
    }

    // ── Cover (the first selected page; unnumbered, no running head) ─────

    private void ComposeCover(Document document, ChapterContent chapter)
    {
        var t = _theme;

        var cover = document.AddSection();
        cover.PageSetup.PageWidth = Unit.FromPoint(t.PageWidth);
        cover.PageSetup.PageHeight = Unit.FromPoint(t.PageHeight);
        //A deep top margin sinks the title block toward the optical centre — the
        //  classic title-page position (most Notion root pages carry little body)
        cover.PageSetup.TopMargin = Unit.FromPoint(t.PageHeight * 0.24);
        cover.PageSetup.BottomMargin = Unit.FromPoint(t.BottomMargin);
        cover.PageSetup.LeftMargin = Unit.FromPoint(t.OuterMargin + 6);
        cover.PageSetup.RightMargin = Unit.FromPoint(t.OuterMargin + 6);

        //The page's icon emoji as a quiet mark above the title, when printable
        if (chapter.IconEmoji.Length > 0)
        {
            var codepoint = char.ConvertToUtf32(chapter.IconEmoji, 0);
            if (FontCoverage.EmojiPrintable(codepoint))
            {
                var mark = cover.AddParagraph();
                mark.Style = "CoverKicker";
                var glyph = mark.AddFormattedText(char.ConvertFromUtf32(codepoint));
                glyph.Font.Name = BookFonts.EmojiFamily;
                glyph.Font.Size = t.BodySize * 1.7;
            }
        }

        cover.AddParagraph("", "CoverRule");
        var title = cover.AddParagraph();
        title.Style = "CoverTitle";
        title.AddBookmark($"page.{chapter.PageId}");
        title.AddText(BookTheme.LetterspaceBreakable(chapter.Title));
        cover.AddParagraph("", "CoverRule");

        //The page's own Notion cover image becomes the cover plate
        if (_context.MediaByBlockId.TryGetValue("cover:" + chapter.PageId, out var media)
            && media.HasImage)
        {
            var coverTextWidth = t.PageWidth - 2 * (t.OuterMargin + 6);
            var width = coverTextWidth * 0.74;
            var aspect = (double)media.Image.Width / media.Image.Height;
            var maxHeight = t.PageHeight * 0.38;
            if (width / aspect > maxHeight) { width = maxHeight * aspect; }

            var plate = cover.AddParagraph("", "Figure");
            plate.Format.SpaceBefore = Unit.FromPoint(t.BodySize * 1.4);
            plate.Format.KeepWithNext = false;

            var bytes = media.Image.Bytes;
            var image = plate.AddImage(ImageSource.FromBinary(
                $"cover-{chapter.PageId}", () => bytes, quality: 90));
            image.LockAspectRatio = true;
            image.Width = Unit.FromPoint(width);
            image.LineFormat.Width = 0.4;
            image.LineFormat.Color = BookTheme.Hairline;
            PlacedImageCount++;
        }

        //The rest of the first page's content flows beneath the title block
        _renderer.RenderPage(cover, chapter.Blocks, chapter.AncestorTitles);
    }

    // ── Chapters (one section per selected page) ─────────────────────────

    private void ComposeChapter(Document document, ChapterContent chapter, bool isFirstContentChapter)
    {
        var t = _theme;

        var section = document.AddSection();
        var setup = section.PageSetup;
        setup.PageWidth = Unit.FromPoint(t.PageWidth);
        setup.PageHeight = Unit.FromPoint(t.PageHeight);
        setup.TopMargin = Unit.FromPoint(t.TopMargin);
        setup.BottomMargin = Unit.FromPoint(t.BottomMargin);
        setup.LeftMargin = Unit.FromPoint(t.InnerMargin);
        setup.RightMargin = Unit.FromPoint(t.OuterMargin);
        setup.MirrorMargins = true;
        setup.OddAndEvenPagesHeaderFooter = true;
        setup.DifferentFirstPageHeaderFooter = true;
        setup.HeaderDistance = Unit.FromPoint(t.TopMargin * 0.48);
        setup.FooterDistance = Unit.FromPoint(t.BottomMargin * 0.42);

        //The unnumbered cover is "page 0": printed numbering starts at 1 on the
        //  first content chapter, and later sections must NOT set StartingNumber
        //  again — that would restart every chapter at 1
        if (isFirstContentChapter)
        {
            setup.StartingNumber = 1;
        }

        //Running heads — chapter title on recto, book title on verso, none on
        //  the chapter opener page
        var recto = section.Headers.Primary.AddParagraph(BookTheme.Letterspace(chapter.Title));
        recto.Style = "RunningHead";
        var verso = section.Headers.EvenPage.AddParagraph(BookTheme.Letterspace(_chapters[0].Title));
        verso.Style = "RunningHead";

        AddFolio(section.Footers.Primary);
        AddFolio(section.Footers.EvenPage);
        AddFolio(section.Footers.FirstPage);

        //Chapter opener: ornament rule, then the page title as display type — the
        //  block sinks below the top margin, the classic chapter-opening drop
        var ornament = section.AddParagraph("", "SectionRule");
        ornament.Format.SpaceBefore = Unit.FromPoint(t.PageHeight * 0.055);

        var title = section.AddParagraph();
        title.Style = "ChapterTitle";
        title.AddBookmark($"page.{chapter.PageId}");
        title.AddText(chapter.Title);

        _renderer.RenderPage(section, chapter.Blocks, chapter.AncestorTitles);
    }

    private static void AddFolio(HeaderFooter footer)
    {
        var folio = footer.AddParagraph();
        folio.Style = "Folio";
        folio.AddPageField();
    }
}
