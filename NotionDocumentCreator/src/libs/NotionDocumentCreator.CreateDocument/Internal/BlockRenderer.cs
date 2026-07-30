using CodeBrix.NotionApi;
using CodeBrix.PdfDocCreate.DocumentObjectModel;
using CodeBrix.PdfDocCreate.DocumentObjectModel.Tables;
using CodeBrix.PdfDocuments.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// The heart of the library: renders every Notion block type into book-designed
/// MigraDoc content per the print mapping — body typography, headings, lists,
/// callout sidebars, code panels, figures with credits, booktabs tables, columns,
/// link cards and cross-references. Unknown block types render a visible marker
/// and a warning; nothing silently vanishes and nothing ever throws mid-book.
/// </summary>
internal sealed class BlockRenderer
{
    private readonly RenderContext _context;
    private readonly RichTextWriter _richText;
    private readonly BookTheme _theme;

    private IReadOnlyList<NotionBlockNode> _chapterBlocks = [];
    private int _figureNumber;
    private int _tableNumber;
    private bool _previousWasBodyParagraph;

    public BlockRenderer(RenderContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _theme = context.Theme ?? throw new ArgumentException("The context needs a theme.", nameof(context));
        _richText = new RichTextWriter(_theme, context.Warnings);

        //Registers the embedded fonts AND the imaging back-end — the PDF image
        //  pipeline needs ImageSource.ImageSourceImpl set before any image is placed
        BookFonts.EnsureRegistered();
    }

    /// <summary>Total characters dropped because no embedded font covers them.</summary>
    public int DroppedCharacterCount => _richText.DroppedCharacterCount;

    /// <summary>Number of figures (images and video posters) placed so far.</summary>
    public int FigureCount => _figureNumber;

    /// <summary>
    /// Renders one chapter's blocks into its section. The ancestor titles (from the
    /// book root down to the page itself) feed breadcrumb blocks.
    /// </summary>
    public void RenderPage(Section section, IReadOnlyList<NotionBlockNode> blocks,
        IReadOnlyList<string> ancestorTitles = null)
    {
        ArgumentNullException.ThrowIfNull(section);
        _chapterBlocks = blocks ?? [];
        AncestorTitles = ancestorTitles ?? [];
        _previousWasBodyParagraph = false;
        RenderBlocks(new SectionTarget(section), _chapterBlocks, indentLevel: 0);
    }

    private IReadOnlyList<string> AncestorTitles { get; set; } = [];

    // ── The block loop ───────────────────────────────────────────────────

    private void RenderBlocks(IBlockTarget target, IReadOnlyList<NotionBlockNode> blocks, int indentLevel)
    {
        var numberedCounter = 0;

        for (var i = 0; i < blocks.Count; i++)
        {
            var node = blocks[i];

            //Numbered-list numbering restarts per contiguous run at each depth
            numberedCounter = node.Block is NumberedListItemBlock ? numberedCounter + 1 : 0;

            switch (node.Block)
            {
                case ParagraphBlock:
                    RenderParagraph(target, node, indentLevel);
                    continue; //RenderParagraph manages the body-indent state itself

                case HeadingOneBlock h1:
                    RenderHeading(target, node, "Heading1", h1.Heading_1?.RichText);
                    break;

                case HeadingTwoBlock h2:
                    RenderHeading(target, node, "Heading2", h2.Heading_2?.RichText);
                    break;

                case HeadingThreeBlock h3:
                    RenderHeading(target, node, "Heading3", h3.Heading_3?.RichText);
                    break;

                case BulletedListItemBlock bulleted:
                    RenderListItem(target, node, bulleted.BulletedListItem?.RichText, indentLevel, marker: null);
                    break;

                case NumberedListItemBlock numbered:
                    RenderListItem(target, node, numbered.NumberedListItem?.RichText, indentLevel,
                        marker: $"{numberedCounter}.");
                    break;

                case ToDoBlock toDo:
                    RenderToDo(target, node, toDo, indentLevel);
                    break;

                case ToggleBlock toggle:
                    RenderToggle(target, node, toggle, indentLevel);
                    break;

                case ChildPageBlock childPage:
                    RenderChildPageReference(target, childPage);
                    break;

                case ChildDatabaseBlock childDatabase:
                    RenderChildDatabaseReference(target, childDatabase);
                    break;

                case CodeBlock code:
                    RenderCode(target, code);
                    break;

                case DividerBlock:
                    target.AddParagraph("", "SectionRule");
                    break;

                case CalloutBlock callout:
                    RenderCallout(target, node, callout);
                    break;

                case QuoteBlock quoteBlock:
                    RenderQuote(target, node, quoteBlock, indentLevel);
                    break;

                case ImageBlock image:
                    i = RenderImage(target, blocks, i, image);
                    break;

                case VideoBlock video:
                    i = RenderVideo(target, blocks, i, video);
                    break;

                case AudioBlock audio:
                    RenderMediaFileCard(target, "AUDIO", audio.Audio, node.Block.Id);
                    break;

                case FileBlock file:
                    RenderMediaFileCard(target, "FILE", file.File, node.Block.Id);
                    break;

                case PDFBlock pdf:
                    RenderMediaFileCard(target, "PDF", pdf.PDF, node.Block.Id);
                    break;

                case EmbedBlock embed:
                    RenderLinkCard(target, "EMBED", embed.Embed?.Url, embed.Embed?.Caption);
                    break;

                case BookmarkBlock bookmark:
                    RenderLinkCard(target, "BOOKMARK", bookmark.Bookmark?.Url, bookmark.Bookmark?.Caption);
                    break;

                case LinkPreviewBlock linkPreview:
                    RenderLinkCard(target, "LINK", linkPreview.LinkPreview?.Url, caption: null);
                    break;

                case EquationBlock equation:
                    RenderEquation(target, equation);
                    break;

                case TableBlock table:
                    RenderTable(target, node, table);
                    break;

                case ColumnListBlock:
                    RenderColumnList(target, node, indentLevel);
                    break;

                case ColumnBlock:
                    //A stray column outside a column_list: render its children in flow
                    RenderBlocks(target, node.Children, indentLevel);
                    break;

                case TableOfContentsBlock:
                    RenderTableOfContents(target);
                    break;

                case BreadcrumbBlock:
                    RenderBreadcrumb(target);
                    break;

                case LinkToPageBlock linkToPage:
                    RenderLinkToPage(target, linkToPage);
                    break;

                case SyncedBlockBlock:
                    //Transclusion: the reader already resolved the content (source's
                    //  children for the duplicate form, own children for the original)
                    RenderBlocks(target, node.Children, indentLevel);
                    break;

                case TemplateBlock:
                    //An authoring affordance with no print meaning
                    _context.Notes.Add("A template block was skipped (no print meaning).");
                    break;

                case TranscriptionBlock transcription:
                    RenderTranscription(target, node, transcription, indentLevel);
                    break;

                default:
                    RenderUnsupported(target, node.Block);
                    break;
            }

            _previousWasBodyParagraph = false;
        }
    }

    // ── Text blocks ──────────────────────────────────────────────────────

    private void RenderParagraph(IBlockTarget target, NotionBlockNode node, int indentLevel)
    {
        var runs = ((ParagraphBlock)node.Block).Paragraph?.RichText?.ToList() ?? [];
        var hasText = runs.Any(r => !string.IsNullOrEmpty(r?.PlainText));

        if (hasText)
        {
            var paragraph = target.AddParagraph();
            paragraph.Style = _previousWasBodyParagraph && indentLevel == 0 ? "BodyIndented" : "BodyOpen";
            Indent(paragraph, indentLevel);
            _richText.Append(paragraph, runs);
            _previousWasBodyParagraph = indentLevel == 0;
        }
        //Empty paragraphs are Notion spacing; a printed book gets its rhythm from
        //  paragraph styles instead, so they contribute nothing

        if (node.Children.Count > 0)
        {
            RenderBlocks(target, node.Children, indentLevel + 1);
            _previousWasBodyParagraph = false;
        }
    }

    private void RenderHeading(IBlockTarget target, NotionBlockNode node, string style,
        IEnumerable<RichTextBase> runs)
    {
        var paragraph = target.AddParagraph();
        paragraph.Style = style;
        _richText.Append(paragraph, runs);

        //A toggleable heading carries children — print cannot collapse, so they follow
        if (node.Children.Count > 0)
        {
            RenderBlocks(target, node.Children, indentLevel: 0);
        }
    }

    private void RenderListItem(IBlockTarget target, NotionBlockNode node,
        IEnumerable<RichTextBase> runs, int indentLevel, string marker)
    {
        var paragraph = target.AddParagraph();
        paragraph.Style = "ListItem";
        paragraph.Format.LeftIndent =
            Unit.FromPoint(_theme.BodySize * 1.6 + indentLevel * _theme.BodySize * 1.5);

        var label = paragraph.AddFormattedText(marker is null
            ? (indentLevel == 0 ? "•  " : "–  ")
            : marker + "  ");
        label.Font.Color = BookTheme.Accent;

        _richText.Append(paragraph, runs);

        if (node.Children.Count > 0)
        {
            RenderBlocks(target, node.Children, indentLevel + 1);
        }
    }

    private void RenderToDo(IBlockTarget target, NotionBlockNode node, ToDoBlock toDo, int indentLevel)
    {
        var isChecked = toDo.ToDo?.IsChecked == true;
        var glyph = isChecked ? "☑" : "☐";
        var codepoint = isChecked ? 0x2611 : 0x2610;

        var paragraph = target.AddParagraph();
        paragraph.Style = "ListItem";
        paragraph.Format.LeftIndent =
            Unit.FromPoint(_theme.BodySize * 1.6 + indentLevel * _theme.BodySize * 1.5);

        //Use the first embedded face that has the ballot-box glyph; plain [x] otherwise
        string glyphFamily = null;
        if (FontCoverage.Covers(FontCoverage.SerifRegular, codepoint)) { glyphFamily = BookFonts.SerifFamily; }
        else if (FontCoverage.Covers(FontCoverage.SansRegular, codepoint)) { glyphFamily = BookFonts.SansFamily; }
        else if (FontCoverage.Covers(FontCoverage.EmojiRegular, codepoint)) { glyphFamily = BookFonts.EmojiFamily; }

        var label = paragraph.AddFormattedText(
            glyphFamily is null ? (isChecked ? "[x]  " : "[ ]  ") : glyph + "  ");
        if (glyphFamily is not null) { label.Font.Name = glyphFamily; }
        label.Font.Color = BookTheme.Accent;

        _richText.Append(paragraph, toDo.ToDo?.RichText);

        if (node.Children.Count > 0)
        {
            RenderBlocks(target, node.Children, indentLevel + 1);
        }
    }

    private void RenderToggle(IBlockTarget target, NotionBlockNode node, ToggleBlock toggle, int indentLevel)
    {
        //Print cannot collapse: the toggle text becomes a bold lead-in line
        var paragraph = target.AddParagraph();
        paragraph.Style = "BodyOpen";
        Indent(paragraph, indentLevel);
        _richText.Append(paragraph, Embolden(toggle.Toggle?.RichText));

        if (node.Children.Count > 0)
        {
            RenderBlocks(target, node.Children, indentLevel + 1);
        }
    }

    private void RenderQuote(IBlockTarget target, NotionBlockNode node, QuoteBlock quoteBlock, int indentLevel)
    {
        var paragraph = target.AddParagraph();
        paragraph.Style = "Quote";
        _richText.Append(paragraph, quoteBlock.Quote?.RichText);

        if (node.Children.Count > 0)
        {
            RenderBlocks(target, node.Children, indentLevel + 1);
        }
    }

    // ── Cross-references ─────────────────────────────────────────────────

    private void RenderChildPageReference(IBlockTarget target, ChildPageBlock childPage)
    {
        //A checked child page becomes its own chapter — nothing renders here.
        //  An unchecked one leaves a quiet italic pointer so the gap is explained.
        if (_context.PagesInBook.ContainsKey(childPage.Id)) { return; }

        var title = string.IsNullOrWhiteSpace(childPage.ChildPage?.Title)
            ? "an untitled page" : childPage.ChildPage.Title;
        target.AddParagraph($"Continues in: {title}", "RefLine");
    }

    private void RenderChildDatabaseReference(IBlockTarget target, ChildDatabaseBlock childDatabase)
    {
        var title = string.IsNullOrWhiteSpace(childDatabase.ChildDatabase?.Title)
            ? "Untitled database" : childDatabase.ChildDatabase.Title;
        var text = _context.DatabaseRowCounts.TryGetValue(childDatabase.Id, out var rows)
            ? $"Database: {title} ({rows} {(rows == 1 ? "entry" : "entries")})"
            : $"Database: {title}";
        target.AddParagraph(text, "RefLine");
    }

    private void RenderLinkToPage(IBlockTarget target, LinkToPageBlock linkToPage)
    {
        var pageId = (linkToPage.LinkToPage as LinkPageToPage)?.PageId;

        if (pageId is not null
            && _context.PagesInBook.TryGetValue(NotionConvert.NormalizeId(pageId), out var bookPage))
        {
            //The target is in the book: give the reader the printed folio
            var paragraph = target.AddParagraph();
            paragraph.Style = "RefLine";
            paragraph.AddText($"See {bookPage.Title}, page ");
            paragraph.AddPageRefField(bookPage.BookmarkName);
            paragraph.AddText(".");
            return;
        }

        target.AddParagraph("See the linked page in Notion.", "RefLine");
        _context.Notes.Add("A link_to_page block points at a page outside this book.");
    }

    // ── Code, callouts, equations ────────────────────────────────────────

    private void RenderCode(IBlockTarget target, CodeBlock code)
    {
        var language = (code.Code?.Language ?? "").Trim();
        if (language.Length > 0 && !"plain text".Equals(language, StringComparison.OrdinalIgnoreCase))
        {
            target.AddParagraph(BookTheme.Letterspace(language), "CodeLabel");
        }

        var text = NotionConvert.PlainText(code.Code?.RichText).TrimEnd();
        var lines = text.Split('\n');

        if (target.SupportsTables)
        {
            var table = target.AddTable();
            table.Borders.Visible = false;
            table.Borders.Left.Width = 1.4;
            table.Borders.Left.Color = BookTheme.Accent;
            table.Shading.Color = BookTheme.CodeTint;
            table.TopPadding = Unit.FromPoint(6);
            table.BottomPadding = Unit.FromPoint(6);
            table.LeftPadding = Unit.FromPoint(9);
            table.RightPadding = Unit.FromPoint(6);
            table.AddColumn(Unit.FromPoint(_theme.TextWidth));
            var row = table.AddRow();
            AddCodeLines(row.Cells[0].AddParagraph(), lines);
        }
        else
        {
            var paragraph = target.AddParagraph();
            paragraph.Style = "CodeText";
            paragraph.Format.Shading.Color = BookTheme.CodeTint;
            AddCodeLines(paragraph, lines);
        }

        if (code.Code?.Caption?.Any() == true)
        {
            var caption = target.AddParagraph();
            caption.Style = "Caption";
            _richText.Append(caption, code.Code.Caption);
        }
    }

    private void AddCodeLines(Paragraph paragraph, string[] lines)
    {
        paragraph.Style = "CodeText";
        for (var i = 0; i < lines.Length; i++)
        {
            if (i > 0) { paragraph.AddLineBreak(); }

            //MigraDoc collapses runs of ordinary spaces — indentation survives as NBSPs
            var line = lines[i].TrimEnd('\r').Replace("\t", "\u00A0\u00A0\u00A0\u00A0");
            var leadingSpaces = line.Length - line.TrimStart(' ').Length;
            line = new string('\u00A0', leadingSpaces) + line[leadingSpaces..];

            if (line.Length > 0) { paragraph.AddText(line); }
        }
    }

    private void RenderCallout(IBlockTarget target, NotionBlockNode node, CalloutBlock callout)
    {
        var (titleRuns, bodyRuns) = SplitCalloutTitle(callout.Callout?.RichText);

        //The icon leads the first line when the monochrome emoji face can print it
        string iconGlyph = null;
        if (callout.Callout?.Icon is EmojiPageIcon { Emoji.Length: > 0 } emojiIcon)
        {
            var codepoint = char.ConvertToUtf32(emojiIcon.Emoji, 0);
            if (FontCoverage.EmojiPrintable(codepoint))
            {
                iconGlyph = char.ConvertFromUtf32(codepoint);
            }
        }

        if (target.SupportsTables)
        {
            var table = target.AddTable();
            table.Borders.Visible = false;
            table.Borders.Left.Width = 1.4;
            table.Borders.Left.Color = BookTheme.Accent;
            table.Shading.Color = BookTheme.PanelTint;
            table.TopPadding = Unit.FromPoint(8);
            table.BottomPadding = Unit.FromPoint(8);
            table.LeftPadding = Unit.FromPoint(10);
            table.RightPadding = Unit.FromPoint(10);
            table.AddColumn(Unit.FromPoint(_theme.TextWidth));
            var cell = table.AddRow().Cells[0];
            FillCallout(new CellTarget(cell), node, titleRuns, bodyRuns, iconGlyph);
            AddPanelSpacer(target);
        }
        else
        {
            //Inside a table cell (a column): tinted paragraphs, no nested table
            FillCallout(target, node, titleRuns, bodyRuns, iconGlyph, tintParagraphs: true);
        }
    }

    private void FillCallout(IBlockTarget target, NotionBlockNode node,
        List<RichTextBase> titleRuns, List<RichTextBase> bodyRuns, string iconGlyph,
        bool tintParagraphs = false)
    {
        var first = target.AddParagraph();
        first.Style = "CalloutText";
        if (tintParagraphs) { first.Format.Shading.Color = BookTheme.PanelTint; }

        if (iconGlyph is not null)
        {
            var icon = first.AddFormattedText(iconGlyph + "  ");
            icon.Font.Name = BookFonts.EmojiFamily;
        }

        if (titleRuns is not null)
        {
            _richText.Append(first, Embolden(titleRuns));
            if (bodyRuns.Count > 0)
            {
                var body = target.AddParagraph();
                body.Style = "CalloutText";
                if (tintParagraphs) { body.Format.Shading.Color = BookTheme.PanelTint; }
                _richText.Append(body, bodyRuns);
            }
        }
        else
        {
            _richText.Append(first, bodyRuns);
        }

        if (node.Children.Count > 0)
        {
            RenderBlocks(target, node.Children, indentLevel: 0);
        }
    }

    private void RenderEquation(IBlockTarget target, EquationBlock equation)
    {
        var expression = equation.Equation?.Expression ?? "";
        if (expression.Length == 0) { return; }

        var paragraph = target.AddParagraph();
        paragraph.Style = "EquationDisplay";
        paragraph.AddText(expression);
        _context.Warnings.Add(
            $"Equation rendered as its LaTeX source (no math typesetting): {Shorten(expression)}");
    }

    private void RenderTranscription(IBlockTarget target, NotionBlockNode node,
        TranscriptionBlock transcription, int indentLevel)
    {
        var paragraph = target.AddParagraph();
        paragraph.Style = "TranscriptText";
        _richText.Append(paragraph, transcription.Transcription?.Title);

        if (node.Children.Count > 0)
        {
            RenderBlocks(target, node.Children, indentLevel + 1);
        }
    }

    private void RenderUnsupported(IBlockTarget target, IBlock block)
    {
        target.AddParagraph("[unsupported Notion block]", "UnsupportedMarker");
        var typeName = (block as UnsupportedBlock)?.Unsupported?.BlockType;
        _context.Warnings.Add(string.IsNullOrWhiteSpace(typeName)
            ? "A block of an unsupported type could not be rendered; a marker was printed instead."
            : $"A block of unsupported type \"{typeName}\" could not be rendered; a marker was printed instead.");
    }

    // ── Figures and media ────────────────────────────────────────────────

    private int RenderImage(IBlockTarget target, IReadOnlyList<NotionBlockNode> blocks, int index,
        ImageBlock image)
    {
        if (!_context.IncludeImages) { return index; }

        var creditIndex = FindCreditParagraph(blocks, index);
        var creditRuns = creditIndex > index
            ? ((ParagraphBlock)blocks[creditIndex].Block).Paragraph?.RichText
            : null;

        if (_context.MediaByBlockId.TryGetValue(image.Id, out var media) && media.HasImage)
        {
            PlaceFigure(target, media.Image, "FIG.", image.Image?.Caption, creditRuns, urlLine: null);
        }
        else
        {
            _context.Warnings.Add(BuildMediaWarning("Image", image.Image, media));
            RenderMediaFileCard(target, "IMAGE", image.Image, image.Id);
            RenderConsumedCredit(target, creditRuns);
        }
        return creditIndex;
    }

    private int RenderVideo(IBlockTarget target, IReadOnlyList<NotionBlockNode> blocks, int index,
        VideoBlock video)
    {
        var creditIndex = FindCreditParagraph(blocks, index);
        var creditRuns = creditIndex > index
            ? ((ParagraphBlock)blocks[creditIndex].Block).Paragraph?.RichText
            : null;

        if (_context.IncludeMedia
            && _context.MediaByBlockId.TryGetValue(video.Id, out var media) && media.HasImage)
        {
            var urlLine = (video.Video as ExternalFile)?.External?.Url;
            PlaceFigure(target, media.Image, "VIDEO", video.Video?.Caption, creditRuns, urlLine,
                metaSuffix: media.Duration is { } d ? $" · {FormatDuration(d)}" : "");
        }
        else
        {
            if (_context.IncludeMedia)
            {
                _context.Warnings.Add(BuildMediaWarning("Video", video.Video,
                    _context.MediaByBlockId.TryGetValue(video.Id, out var failed) ? failed : null));
            }
            RenderMediaFileCard(target, "VIDEO", video.Video, video.Id);
            RenderConsumedCredit(target, creditRuns);
        }
        return creditIndex;
    }

    private void PlaceFigure(IBlockTarget target, ProcessedImage processed, string label,
        IEnumerable<RichTextBase> caption, IEnumerable<RichTextBase> creditRuns, string urlLine,
        string metaSuffix = "")
    {
        var t = _theme;
        _figureNumber++;

        var aspect = (double)processed.Width / processed.Height;
        var width = aspect switch
        {
            >= 1.25 => t.TextWidth,
            >= 0.85 => t.TextWidth * 0.70,
            _ => t.TextWidth * 0.52
        };
        var maxHeight = t.TextHeight * 0.58;
        if (width / aspect > maxHeight) { width = maxHeight * aspect; }

        var figure = target.AddParagraph();
        figure.Style = "Figure";
        var bytes = processed.Bytes;
        var image = figure.AddImage(ImageSource.FromBinary(
            $"img-{_figureNumber}-{Guid.NewGuid():N}", () => bytes, quality: 90));
        image.LockAspectRatio = true;
        image.Width = Unit.FromPoint(width);

        //A hairline keyline flatters photographs; transparent graphics read better without
        if (IsJpeg(bytes))
        {
            image.LineFormat.Width = 0.4;
            image.LineFormat.Color = BookTheme.Hairline;
        }

        //Credit sits directly under the plate, whisper-small, kept with it
        if (creditRuns is not null)
        {
            var credit = target.AddParagraph();
            credit.Style = "Credit";
            _richText.Append(credit, creditRuns);
        }

        var captionParagraph = target.AddParagraph();
        captionParagraph.Style = "Caption";
        var figureLabel = captionParagraph.AddFormattedText(
            label == "FIG." ? $"FIG. {_figureNumber}" : label + metaSuffix);
        figureLabel.Font.Bold = true;
        figureLabel.Font.Size = t.LabelSize;
        figureLabel.Font.Color = BookTheme.Accent;

        if (caption?.Any(r => !string.IsNullOrEmpty(r?.PlainText)) == true)
        {
            captionParagraph.AddText("   ");
            _richText.Append(captionParagraph, caption);
        }

        if (!string.IsNullOrWhiteSpace(urlLine))
        {
            var url = target.AddParagraph();
            url.Style = "Credit";
            url.Format.KeepWithNext = false;
            url.AddText(Shorten(urlLine));
        }
    }

    private void RenderConsumedCredit(IBlockTarget target, IEnumerable<RichTextBase> creditRuns)
    {
        if (creditRuns is null) { return; }
        var credit = target.AddParagraph();
        credit.Style = "Credit";
        credit.Format.KeepWithNext = false;
        _richText.Append(credit, creditRuns);
    }

    /// <summary>
    /// The credit look-ahead: when the sibling immediately after an image/video is a
    /// paragraph shaped like a rights line, it is consumed and rendered with the
    /// figure. Returns that sibling's index, or the block's own index when the next
    /// paragraph is ordinary body text.
    /// </summary>
    internal static int FindCreditParagraph(IReadOnlyList<NotionBlockNode> blocks, int index)
    {
        if (index + 1 >= blocks.Count) { return index; }
        if (blocks[index + 1].Block is not ParagraphBlock paragraph) { return index; }

        var text = NotionConvert.PlainText(paragraph.Paragraph?.RichText).TrimStart();
        string[] creditShapes = ["Credit", "Image credit", "Photo", "Source", "©", "Public domain", "CC "];
        return creditShapes.Any(shape => text.StartsWith(shape, StringComparison.Ordinal))
            ? index + 1
            : index;
    }

    private void RenderMediaFileCard(IBlockTarget target, string label, FileObject file, string blockId)
    {
        var meta = new List<string>();
        if (_context.MediaByBlockId.TryGetValue(blockId ?? "", out var media))
        {
            if (media.Duration is { } duration) { meta.Add(FormatDuration(duration)); }
            if (media.SourceLength > 0) { meta.Add(FormatSize(media.SourceLength)); }
        }

        //An uploaded file's pre-signed URL expires within the hour — printing it
        //  would be useless, so only external URLs go on the card
        var url = (file as ExternalFile)?.External?.Url;
        var title = string.IsNullOrWhiteSpace(file?.Name)
            ? (file is UploadedFile ? "Uploaded file" : "Linked file")
            : file.Name;

        RenderCard(target, label, title, meta, file?.Caption, url);
    }

    private void RenderLinkCard(IBlockTarget target, string label, string url,
        IEnumerable<RichTextBase> caption)
    {
        var domain = "";
        if (Uri.TryCreate(url ?? "", UriKind.Absolute, out var uri)) { domain = uri.Host; }
        RenderCard(target, label, domain.Length > 0 ? domain : "Link", [], caption, url);
    }

    private void RenderCard(IBlockTarget target, string label, string title,
        IEnumerable<string> metaLines, IEnumerable<RichTextBase> caption, string url)
    {
        var cardTarget = target;
        Table table = null;
        if (target.SupportsTables)
        {
            table = target.AddTable();
            table.Borders.Width = 0.5;
            table.Borders.Color = BookTheme.Hairline;
            table.Shading.Color = BookTheme.CodeTint;
            table.TopPadding = Unit.FromPoint(7);
            table.BottomPadding = Unit.FromPoint(7);
            table.LeftPadding = Unit.FromPoint(10);
            table.RightPadding = Unit.FromPoint(10);
            table.AddColumn(Unit.FromPoint(_theme.TextWidth));
            cardTarget = new CellTarget(table.AddRow().Cells[0]);
        }

        cardTarget.AddParagraph(BookTheme.Letterspace(label), "CardLabel");
        cardTarget.AddParagraph(title, "CardTitle");

        foreach (var meta in metaLines)
        {
            cardTarget.AddParagraph(meta, "CardMeta");
        }

        if (caption?.Any(r => !string.IsNullOrEmpty(r?.PlainText)) == true)
        {
            var captionParagraph = cardTarget.AddParagraph();
            captionParagraph.Style = "CardMeta";
            _richText.Append(captionParagraph, caption);
        }

        if (!string.IsNullOrWhiteSpace(url))
        {
            cardTarget.AddParagraph(Shorten(url), "CardUrl");
        }

        if (table is not null) { AddPanelSpacer(target); }
    }

    private string BuildMediaWarning(string kind, FileObject file, PreparedMedia media)
    {
        var name = string.IsNullOrWhiteSpace(file?.Name) ? "(unnamed)" : file.Name;
        var reason = string.IsNullOrWhiteSpace(media?.FailureReason)
            ? "it was not downloaded" : media.FailureReason;
        return $"{kind} \"{name}\" rendered as a card: {reason}";
    }

    // ── Tables and columns ───────────────────────────────────────────────

    private void RenderTable(IBlockTarget target, NotionBlockNode node, TableBlock tableBlock)
    {
        var rows = node.Children
            .Select(child => child.Block)
            .OfType<TableRowBlock>()
            .Select(rowBlock => (rowBlock.TableRow?.Cells ?? [])
                .Select(cell => (cell ?? []).Cast<RichTextBase>().ToList())
                .ToList())
            .ToList();
        var columnCount = Math.Max(tableBlock.Table?.TableWidth ?? 0, rows.Count > 0 ? rows.Max(r => r.Count) : 0);
        if (rows.Count == 0 || columnCount == 0) { return; }

        if (!target.SupportsTables)
        {
            //MigraDoc cannot nest tables — degrade to one line per row
            _context.Warnings.Add("A table inside a column was flattened to text lines (tables cannot nest).");
            foreach (var row in rows)
            {
                var line = target.AddParagraph();
                line.Style = "TableText";
                for (var c = 0; c < row.Count; c++)
                {
                    if (c > 0) { line.AddText("  ·  "); }
                    _richText.Append(line, row[c]);
                }
            }
            return;
        }

        var t = _theme;
        _tableNumber++;
        var hasColumnHeader = tableBlock.Table?.HasColumnHeader == true;
        var hasRowHeader = tableBlock.Table?.HasRowHeader == true;

        var captionParagraph = target.AddParagraph();
        captionParagraph.Style = "TableCaption";
        var label = captionParagraph.AddFormattedText($"TABLE {_tableNumber}");
        label.Font.Bold = true;
        label.Font.Size = t.LabelSize;
        label.Font.Color = BookTheme.Accent;

        var table = target.AddTable();
        table.Borders.Visible = false;
        table.TopPadding = Unit.FromPoint(3);
        table.BottomPadding = Unit.FromPoint(3);
        table.LeftPadding = Unit.FromPoint(2);
        table.RightPadding = Unit.FromPoint(4);

        var columnWidth = t.TextWidth / columnCount;
        for (var c = 0; c < columnCount; c++)
        {
            table.AddColumn(Unit.FromPoint(columnWidth));
        }

        for (var r = 0; r < rows.Count; r++)
        {
            var row = table.AddRow();

            //Booktabs styling: strong top and bottom rules, a light rule under the
            //  header, and no vertical rules at all
            if (r == 0)
            {
                row.Borders.Top.Width = 1.0;
                row.Borders.Top.Color = BookTheme.Ink;
                if (hasColumnHeader)
                {
                    row.Borders.Bottom.Width = 0.5;
                    row.Borders.Bottom.Color = BookTheme.Ink;
                    row.HeadingFormat = true;
                }
            }
            if (r == rows.Count - 1)
            {
                row.Borders.Bottom.Width = 1.0;
                row.Borders.Bottom.Color = BookTheme.Ink;
            }

            for (var c = 0; c < columnCount && c < rows[r].Count; c++)
            {
                var paragraph = row.Cells[c].AddParagraph();
                paragraph.Style = "TableText";
                if ((hasColumnHeader && r == 0) || (hasRowHeader && c == 0))
                {
                    paragraph.Format.Font.Bold = true;
                }
                _richText.Append(paragraph, rows[r][c]);
            }
        }

        AddPanelSpacer(target);
    }

    private void RenderColumnList(IBlockTarget target, NotionBlockNode node, int indentLevel)
    {
        var columns = node.Children.Where(child => child.Block is ColumnBlock).ToList();
        if (columns.Count == 0) { return; }

        if (!target.SupportsTables || columns.Count == 1)
        {
            //Nested column lists flatten in reading order
            foreach (var column in columns)
            {
                RenderBlocks(target, column.Children, indentLevel);
            }
            return;
        }

        var table = target.AddTable();
        table.Borders.Visible = false;
        table.TopPadding = 0;
        table.BottomPadding = 0;
        table.LeftPadding = 0;
        table.RightPadding = Unit.FromPoint(10);

        //The block model carries no width_ratio — columns share the width equally
        var columnWidth = _theme.TextWidth / columns.Count;
        for (var c = 0; c < columns.Count; c++)
        {
            table.AddColumn(Unit.FromPoint(columnWidth));
        }

        var row = table.AddRow();
        for (var c = 0; c < columns.Count; c++)
        {
            RenderBlocks(new CellTarget(row.Cells[c]), columns[c].Children, indentLevel: 0);
        }

        AddPanelSpacer(target);
    }

    // ── Page furniture ───────────────────────────────────────────────────

    private void RenderTableOfContents(IBlockTarget target)
    {
        //The block means "list this page's headings" — an in-page list, not a book TOC
        var found = false;
        foreach (var node in _chapterBlocks)
        {
            (string text, int level) = node.Block switch
            {
                HeadingOneBlock h1 => (NotionConvert.PlainText(h1.Heading_1?.RichText), 0),
                HeadingTwoBlock h2 => (NotionConvert.PlainText(h2.Heading_2?.RichText), 1),
                HeadingThreeBlock h3 => (NotionConvert.PlainText(h3.Heading_3?.RichText), 2),
                _ => (null, 0)
            };
            if (string.IsNullOrWhiteSpace(text)) { continue; }

            found = true;
            var line = target.AddParagraph();
            line.Style = "TocLine";
            line.Format.LeftIndent = Unit.FromPoint(_theme.BodySize * 1.9 * level);
            line.AddText(text);
        }

        if (!found)
        {
            _context.Notes.Add("A table_of_contents block found no headings on its page.");
        }
    }

    private void RenderBreadcrumb(IBlockTarget target)
    {
        if (AncestorTitles.Count == 0) { return; }
        var path = string.Join("  ·  ", AncestorTitles.Select(BookTheme.Letterspace));
        target.AddParagraph(path, "BreadcrumbLine");
    }

    // ── Shared helpers ───────────────────────────────────────────────────

    private void Indent(Paragraph paragraph, int indentLevel)
    {
        if (indentLevel > 0)
        {
            paragraph.Format.LeftIndent = Unit.FromPoint(indentLevel * _theme.BodySize * 1.5);
        }
    }

    private void AddPanelSpacer(IBlockTarget target)
    {
        //A hair-high paragraph so consecutive panels/tables keep their breathing room
        var spacer = target.AddParagraph();
        spacer.Format.SpaceAfter = Unit.FromPoint(_theme.BodySize * 0.9);
        spacer.Format.LineSpacingRule = LineSpacingRule.Exactly;
        spacer.Format.LineSpacing = Unit.FromPoint(1);
    }

    /// <summary>
    /// Splits callout text at its first newline: the first line becomes the bold
    /// title, the rest the body. No newline → no title (runs render as authored).
    /// </summary>
    internal static (List<RichTextBase> Title, List<RichTextBase> Body) SplitCalloutTitle(
        IEnumerable<RichTextBase> runs)
    {
        var all = runs?.Where(r => r is not null).ToList() ?? [];
        var flat = string.Concat(all.Select(r => r.PlainText ?? ""));
        var newline = flat.IndexOf('\n');
        if (newline < 0) { return (null, all); }

        var title = new List<RichTextBase>();
        var body = new List<RichTextBase>();
        var seen = 0;
        foreach (var run in all)
        {
            var text = run.PlainText ?? "";
            if (seen + text.Length <= newline)
            {
                title.Add(run);
            }
            else if (seen > newline)
            {
                body.Add(run);
            }
            else
            {
                //The boundary run: split it into synthetic title/body halves
                var split = newline - seen;
                title.Add(CopyRun(run, text[..split]));
                var rest = text[(split + 1)..];
                if (rest.Length > 0) { body.Add(CopyRun(run, rest)); }
            }
            seen += text.Length;
        }
        return (title, body);
    }

    private static List<RichTextBase> Embolden(IEnumerable<RichTextBase> runs) =>
        (runs ?? []).Where(r => r is not null)
            .Select(r => r.Annotations?.IsBold == true ? r : CopyRun(r, r.PlainText ?? "", forceBold: true))
            .ToList();

    private static RichTextBase CopyRun(RichTextBase run, string text, bool forceBold = false) =>
        new()
        {
            PlainText = text,
            Href = run.Href,
            Annotations = new Annotations
            {
                IsBold = forceBold || run.Annotations?.IsBold == true,
                IsItalic = run.Annotations?.IsItalic == true,
                IsStrikeThrough = run.Annotations?.IsStrikeThrough == true,
                IsUnderline = run.Annotations?.IsUnderline == true,
                IsCode = run.Annotations?.IsCode == true,
                Color = run.Annotations?.Color
            }
        };

    private static string FormatDuration(TimeSpan duration) =>
        duration.TotalHours >= 1
            ? $"{(int)duration.TotalHours}:{duration.Minutes:D2}:{duration.Seconds:D2}"
            : $"{duration.Minutes}:{duration.Seconds:D2}";

    private static string FormatSize(long bytes) => bytes switch
    {
        >= 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F1} MB",
        >= 1024 => $"{bytes / 1024.0:F0} KB",
        _ => $"{bytes} bytes"
    };

    private static string Shorten(string text) =>
        text.Length <= 90 ? text : text[..90] + "…";

    private static bool IsJpeg(byte[] bytes) =>
        bytes is { Length: > 2 } && bytes[0] == 0xFF && bytes[1] == 0xD8;

    //Section and Cell share no content-adding base type, so the renderer targets
    //  either through a tiny adapter. Tables cannot nest inside cells in MigraDoc,
    //  which is what SupportsTables guards.
    private interface IBlockTarget
    {
        bool SupportsTables { get; }
        Paragraph AddParagraph();
        Paragraph AddParagraph(string text, string style);
        Table AddTable();
    }

    private sealed class SectionTarget : IBlockTarget
    {
        private readonly Section _section;
        public SectionTarget(Section section) { _section = section; }
        public bool SupportsTables => true;
        public Paragraph AddParagraph() => _section.AddParagraph();
        public Paragraph AddParagraph(string text, string style) => _section.AddParagraph(text, style);
        public Table AddTable() => _section.AddTable();
    }

    private sealed class CellTarget : IBlockTarget
    {
        private readonly Cell _cell;
        public CellTarget(Cell cell) { _cell = cell; }
        public bool SupportsTables => false;
        public Paragraph AddParagraph() => _cell.AddParagraph();
        public Paragraph AddParagraph(string text, string style)
        {
            var paragraph = _cell.AddParagraph(text);
            paragraph.Style = style;
            return paragraph;
        }
        public Table AddTable() =>
            throw new InvalidOperationException("MigraDoc cannot nest a table inside a table cell.");
    }
}
