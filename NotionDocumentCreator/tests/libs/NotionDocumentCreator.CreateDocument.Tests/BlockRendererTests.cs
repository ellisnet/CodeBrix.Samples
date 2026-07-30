using CodeBrix.NotionApi;
using CodeBrix.PdfDocCreate.DocumentObjectModel;
using NotionDocumentCreator.CreateDocument.Internal;
using SilverAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using NText = CodeBrix.NotionApi.Text;

namespace NotionDocumentCreator.CreateDocument.Tests;

/// <summary>
/// One test per Notion block type from the print mapping — each builds a
/// hand-made block and asserts the composed MigraDoc document contains the
/// expected element. This is the file that proves "handles every block type".
/// </summary>
public class BlockRendererTests
{
    // ── Builders ─────────────────────────────────────────────────────────

    private static RichTextText T(string text, bool bold = false, bool italic = false) =>
        new()
        {
            PlainText = text,
            Annotations = new Annotations { IsBold = bold, IsItalic = italic },
            Text = new NText { Content = text }
        };

    private static List<RichTextBase> Runs(params RichTextText[] runs) =>
        runs.Cast<RichTextBase>().ToList();

    private static NotionBlockNode N(IBlock block, params NotionBlockNode[] children) =>
        new() { Block = block, Children = children };

    private static NotionBlockNode Para(string text) =>
        N(new ParagraphBlock { Paragraph = new ParagraphBlock.Info { RichText = Runs(T(text)) } });

    private static (Section Section, RenderContext Context) Render(
        Action<RenderContext> configure, params NotionBlockNode[] nodes)
    {
        var (_, section, context, renderer) = TestDom.CreateRenderer(configure);
        renderer.RenderPage(section, nodes);
        return (section, context);
    }

    private static (Section Section, RenderContext Context) Render(params NotionBlockNode[] nodes) =>
        Render(null, nodes);

    //A real (decodable) 1x1 transparent PNG — the imaging back-end decodes eagerly
    private static readonly byte[] OnePixelPng = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");

    private static PreparedMedia FakeImage() =>
        new() { Image = new ProcessedImage { Bytes = OnePixelPng, Width = 400, Height = 300 } };

    // ── Text blocks ──────────────────────────────────────────────────────

    [Fact]
    public void paragraph_block_renders_body_text()
    {
        //Act
        var (section, _) = Render(Para("Hello book."));

        //Assert
        var paragraphs = TestDom.AllParagraphs(section);
        paragraphs.Should().HaveCount(1);
        paragraphs[0].Style.Should().Be("BodyOpen");
        TestDom.TextOf(paragraphs[0]).Should().Be("Hello book.");
    }

    [Fact]
    public void consecutive_paragraphs_get_the_classic_book_indent()
    {
        //Act
        var (section, _) = Render(Para("First."), Para("Second."));

        //Assert
        var paragraphs = TestDom.AllParagraphs(section);
        paragraphs[0].Style.Should().Be("BodyOpen");
        paragraphs[1].Style.Should().Be("BodyIndented");
    }

    [Fact]
    public void empty_paragraph_contributes_nothing()
    {
        //Arrange
        var empty = N(new ParagraphBlock { Paragraph = new ParagraphBlock.Info { RichText = Runs() } });

        //Act
        var (section, _) = Render(empty);

        //Assert
        TestDom.AllParagraphs(section).Should().HaveCount(0);
    }

    [Fact]
    public void heading_one_block_uses_heading1_style()
    {
        //Arrange
        var block = N(new HeadingOneBlock
        {
            Heading_1 = new HeadingOneBlock.Info { RichText = Runs(T("History")) }
        });

        //Act
        var (section, _) = Render(block);

        //Assert
        var paragraph = TestDom.AllParagraphs(section).Single();
        paragraph.Style.Should().Be("Heading1");
        TestDom.TextOf(paragraph).Should().Be("History");
    }

    [Fact]
    public void heading_two_block_uses_heading2_style()
    {
        //Arrange
        var block = N(new HeadingTwoBlock
        {
            Heading_2 = new HeadingTwoBlock.Info { RichText = Runs(T("Origins")) }
        });

        //Act
        var (section, _) = Render(block);

        //Assert
        TestDom.AllParagraphs(section).Single().Style.Should().Be("Heading2");
    }

    [Fact]
    public void heading_three_block_uses_heading3_style()
    {
        //Arrange
        var block = N(new HeadingThreeBlock
        {
            Heading_3 = new HeadingThreeBlock.Info { RichText = Runs(T("Details")) }
        });

        //Act
        var (section, _) = Render(block);

        //Assert
        TestDom.AllParagraphs(section).Single().Style.Should().Be("Heading3");
    }

    [Fact]
    public void bulleted_list_item_renders_with_bullet_and_indents_nested_items()
    {
        //Arrange
        var nested = N(new BulletedListItemBlock
        {
            BulletedListItem = new BulletedListItemBlock.Info { RichText = Runs(T("Inner")) }
        });
        var outer = N(new BulletedListItemBlock
        {
            BulletedListItem = new BulletedListItemBlock.Info { RichText = Runs(T("Outer")) }
        }, nested);

        //Act
        var (section, _) = Render(outer);

        //Assert
        var paragraphs = TestDom.AllParagraphs(section);
        paragraphs.Should().HaveCount(2);
        TestDom.TextOf(paragraphs[0]).Should().StartWith("•");
        TestDom.TextOf(paragraphs[1]).Should().StartWith("–");
        (paragraphs[1].Format.LeftIndent.Point > paragraphs[0].Format.LeftIndent.Point).Should().Be(true);
    }

    [Fact]
    public void numbered_list_item_numbering_restarts_per_contiguous_run()
    {
        //Arrange
        NotionBlockNode Item(string text) => N(new NumberedListItemBlock
        {
            NumberedListItem = new NumberedListItemBlock.Info { RichText = Runs(T(text)) }
        });

        //Act
        var (section, _) = Render(Item("one"), Item("two"), Para("Interruption."), Item("restart"));

        //Assert
        var texts = TestDom.AllTexts(section);
        texts[0].Should().StartWith("1.");
        texts[1].Should().StartWith("2.");
        texts[3].Should().StartWith("1.");
    }

    [Fact]
    public void to_do_block_renders_a_checkbox_marker()
    {
        //Arrange
        var unchecked_ = N(new ToDoBlock
        {
            ToDo = new ToDoBlock.Info { RichText = Runs(T("Buy ink")), IsChecked = false }
        });
        var checked_ = N(new ToDoBlock
        {
            ToDo = new ToDoBlock.Info { RichText = Runs(T("Buy paper")), IsChecked = true }
        });

        //Act
        var (section, _) = Render(unchecked_, checked_);

        //Assert - the ballot-box glyph when an embedded face has it, [ ]/[x] otherwise
        var texts = TestDom.AllTexts(section);
        (texts[0].Contains('☐') || texts[0].Contains("[ ]")).Should().Be(true);
        (texts[1].Contains('☑') || texts[1].Contains("[x]")).Should().Be(true);
    }

    [Fact]
    public void toggle_block_renders_bold_lead_in_then_children_beneath()
    {
        //Arrange
        var toggle = N(new ToggleBlock
        {
            Toggle = new ToggleBlock.Info { RichText = Runs(T("More detail")) }
        }, Para("Hidden body."));

        //Act
        var (section, _) = Render(toggle);

        //Assert
        var paragraphs = TestDom.AllParagraphs(section);
        paragraphs.Should().HaveCount(2);
        TestDom.FormattedRuns(paragraphs[0]).Any(r => r.Font.Bold).Should().Be(true);
        TestDom.TextOf(paragraphs[1]).Should().Be("Hidden body.");
    }

    // ── Child pages and databases ────────────────────────────────────────

    [Fact]
    public void checked_child_page_renders_nothing_in_the_parent()
    {
        //Arrange - the page is in the book, so it becomes its own chapter
        var child = N(new ChildPageBlock
        {
            Id = "page-1",
            ChildPage = new ChildPageBlock.Info { Title = "Chapter A" }
        });

        //Act
        var (section, _) = Render(
            context => context.PagesInBook["page-1"] = new BookPageRef { Title = "Chapter A" },
            child);

        //Assert
        TestDom.AllParagraphs(section).Should().HaveCount(0);
    }

    [Fact]
    public void unchecked_child_page_renders_a_continues_in_line()
    {
        //Arrange
        var child = N(new ChildPageBlock
        {
            Id = "page-2",
            ChildPage = new ChildPageBlock.Info { Title = "Appendix" }
        });

        //Act
        var (section, _) = Render(child);

        //Assert
        var paragraph = TestDom.AllParagraphs(section).Single();
        paragraph.Style.Should().Be("RefLine");
        TestDom.TextOf(paragraph).Should().Be("Continues in: Appendix");
    }

    [Fact]
    public void child_database_renders_a_titled_reference_with_row_count()
    {
        //Arrange
        var child = N(new ChildDatabaseBlock
        {
            Id = "db-1",
            ChildDatabase = new ChildDatabaseBlock.Info { Title = "Sources" }
        });

        //Act
        var (section, _) = Render(
            context => context.DatabaseRowCounts["db-1"] = 3,
            child);

        //Assert
        TestDom.TextOf(TestDom.AllParagraphs(section).Single())
            .Should().Be("Database: Sources (3 entries)");
    }

    // ── Code, dividers, callouts, quotes ─────────────────────────────────

    [Fact]
    public void code_block_renders_mono_panel_with_language_label_and_kept_indentation()
    {
        //Arrange
        var code = N(new CodeBlock
        {
            Code = new CodeBlock.Info
            {
                RichText = Runs(T("def x():\n    pass")),
                Language = "python"
            }
        });

        //Act
        var (section, _) = Render(code);

        //Assert
        var paragraphs = TestDom.AllParagraphs(section);
        paragraphs.Any(p => p.Style == "CodeLabel").Should().Be(true);
        var codeParagraph = paragraphs.Single(p => p.Style == "CodeText");
        //Leading spaces survive MigraDoc's space collapsing as NBSPs
        TestDom.TextOf(codeParagraph).Should().Contain("\u00A0\u00A0\u00A0\u00A0pass");
        TestDom.AllTables(section).Should().HaveCount(1);
    }

    [Fact]
    public void divider_block_renders_a_section_rule()
    {
        //Act
        var (section, _) = Render(N(new DividerBlock()));

        //Assert
        TestDom.AllParagraphs(section).Single().Style.Should().Be("SectionRule");
    }

    [Fact]
    public void callout_block_renders_a_tinted_boxed_aside()
    {
        //Arrange
        var callout = N(new CalloutBlock
        {
            Callout = new CalloutBlock.Info { RichText = Runs(T("A wise aside.")) }
        });

        //Act
        var (section, _) = Render(callout);

        //Assert
        var table = TestDom.AllTables(section).Single();
        table.Shading.Color.Should().Be(BookTheme.PanelTint);
        TestDom.AllTexts(section).Any(t => t.Contains("A wise aside.")).Should().Be(true);
    }

    [Fact]
    public void callout_title_line_is_emboldened()
    {
        //Arrange - Jeremy's sidebar banners carry "Title\nBody" text
        var callout = N(new CalloutBlock
        {
            Callout = new CalloutBlock.Info { RichText = Runs(T("The Banner Title\nAnd the body text.")) }
        });

        //Act
        var (section, _) = Render(callout);

        //Assert
        var titleParagraph = TestDom.AllParagraphs(section)
            .Single(p => TestDom.TextOf(p).Contains("The Banner Title"));
        TestDom.FormattedRuns(titleParagraph).Any(r => r.Font.Bold).Should().Be(true);
        TestDom.AllTexts(section).Any(t => t.Contains("And the body text.")).Should().Be(true);
    }

    [Fact]
    public void quote_block_renders_quote_style()
    {
        //Arrange
        var quote = N(new QuoteBlock
        {
            Quote = new QuoteBlock.Info { RichText = Runs(T("To be printed.")) }
        });

        //Act
        var (section, _) = Render(quote);

        //Assert
        TestDom.AllParagraphs(section).Single().Style.Should().Be("Quote");
    }

    // ── Figures and media ────────────────────────────────────────────────

    [Fact]
    public void image_block_renders_a_numbered_figure_with_caption()
    {
        //Arrange
        var image = N(new ImageBlock
        {
            Id = "img-1",
            Image = new UploadedFile { Caption = Runs(T("The first computer.")) }
        });

        //Act
        var (section, _) = Render(
            context => context.MediaByBlockId["img-1"] = FakeImage(),
            image);

        //Assert
        var paragraphs = TestDom.AllParagraphs(section);
        paragraphs.Any(p => p.Style == "Figure").Should().Be(true);
        var caption = paragraphs.Single(p => p.Style == "Caption");
        TestDom.TextOf(caption).Should().Contain("FIG. 1");
        TestDom.TextOf(caption).Should().Contain("The first computer.");
    }

    [Fact]
    public void image_credit_paragraph_is_consumed_and_rendered_with_the_figure()
    {
        //Arrange
        var image = N(new ImageBlock { Id = "img-1", Image = new UploadedFile() });
        var credit = Para("© 1975 Somebody, public archive");

        //Act
        var (section, _) = Render(
            context => context.MediaByBlockId["img-1"] = FakeImage(),
            image, credit);

        //Assert - the credit renders in Credit style, not as body text
        var creditParagraph = TestDom.AllParagraphs(section).Single(p => p.Style == "Credit");
        TestDom.TextOf(creditParagraph).Should().Contain("© 1975 Somebody");
        TestDom.AllParagraphs(section).Any(p => p.Style is "BodyOpen" or "BodyIndented").Should().Be(false);
    }

    [Fact]
    public void image_without_media_renders_a_card_and_a_warning()
    {
        //Arrange
        var image = N(new ImageBlock
        {
            Id = "img-missing",
            Image = new UploadedFile { Name = "lost.png" }
        });

        //Act
        var (section, context) = Render(image);

        //Assert
        context.Warnings.Should().HaveCount(1);
        TestDom.AllParagraphs(section).Any(p => p.Style == "CardLabel").Should().Be(true);
    }

    [Fact]
    public void images_are_skipped_entirely_when_include_images_is_false()
    {
        //Arrange
        var image = N(new ImageBlock { Id = "img-1", Image = new UploadedFile() });

        //Act
        var (section, context) = Render(
            context => context.IncludeImages = false,
            image);

        //Assert - text-only rendering: no figure, no card, no warning
        TestDom.AllParagraphs(section).Should().HaveCount(0);
        context.Warnings.Should().HaveCount(0);
    }

    [Fact]
    public void video_block_with_poster_renders_a_video_labelled_figure()
    {
        //Arrange
        var video = N(new VideoBlock
        {
            Id = "vid-1",
            Video = new ExternalFile
            {
                External = new ExternalFile.Info { Url = "https://example.com/clip.mp4" },
                Caption = Runs(T("Launch footage."))
            }
        });
        var media = new PreparedMedia
        {
            Image = FakeImage().Image,
            Duration = TimeSpan.FromSeconds(83)
        };

        //Act
        var (section, _) = Render(
            context => context.MediaByBlockId["vid-1"] = media,
            video);

        //Assert
        var texts = TestDom.AllTexts(section);
        texts.Any(t => t.Contains("VIDEO")).Should().Be(true);
        texts.Any(t => t.Contains("Launch footage.")).Should().Be(true);
        texts.Any(t => t.Contains("example.com/clip.mp4")).Should().Be(true);
    }

    [Fact]
    public void video_without_poster_renders_a_media_card_with_warning()
    {
        //Arrange
        var video = N(new VideoBlock
        {
            Id = "vid-2",
            Video = new UploadedFile { Name = "too-big.mov" }
        });

        //Act
        var (section, context) = Render(video);

        //Assert
        TestDom.AllParagraphs(section).Any(p => p.Style == "CardLabel").Should().Be(true);
        context.Warnings.Should().HaveCount(1);
    }

    [Fact]
    public void audio_block_renders_a_media_card_with_duration()
    {
        //Arrange
        var audio = N(new AudioBlock
        {
            Id = "aud-1",
            Audio = new UploadedFile { Name = "hymn.mp3" }
        });

        //Act
        var (section, _) = Render(
            context => context.MediaByBlockId["aud-1"] =
                new PreparedMedia { Duration = TimeSpan.FromSeconds(205) },
            audio);

        //Assert
        var texts = TestDom.AllTexts(section);
        texts.Any(t => t.Contains("hymn.mp3")).Should().Be(true);
        texts.Any(t => t.Contains("3:25")).Should().Be(true);
    }

    [Fact]
    public void file_block_renders_a_file_card_with_size()
    {
        //Arrange
        var file = N(new FileBlock
        {
            Id = "file-1",
            File = new UploadedFile { Name = "notes.zip" }
        });

        //Act
        var (section, _) = Render(
            context => context.MediaByBlockId["file-1"] =
                new PreparedMedia { SourceLength = 45 * 1024 },
            file);

        //Assert
        var texts = TestDom.AllTexts(section);
        texts.Any(t => t.Contains("notes.zip")).Should().Be(true);
        texts.Any(t => t.Contains("45 KB")).Should().Be(true);
    }

    [Fact]
    public void pdf_block_renders_a_pdf_card()
    {
        //Arrange
        var pdf = N(new PDFBlock
        {
            Id = "pdf-1",
            PDF = new ExternalFile
            {
                External = new ExternalFile.Info { Url = "https://example.com/paper.pdf" },
                Name = "paper.pdf"
            }
        });

        //Act
        var (section, _) = Render(pdf);

        //Assert
        var texts = TestDom.AllTexts(section);
        texts.Any(t => t.Contains("paper.pdf")).Should().Be(true);
        texts.Any(t => t.Contains("example.com/paper.pdf")).Should().Be(true);
    }

    // ── Link cards ───────────────────────────────────────────────────────

    [Fact]
    public void embed_block_renders_a_link_card_with_domain()
    {
        //Arrange
        var embed = N(new EmbedBlock
        {
            Embed = new EmbedBlock.Info { Url = "https://maps.example.org/uruk" }
        });

        //Act
        var (section, _) = Render(embed);

        //Assert
        var texts = TestDom.AllTexts(section);
        texts.Any(t => t.Contains("maps.example.org")).Should().Be(true);
    }

    [Fact]
    public void bookmark_block_renders_a_link_card()
    {
        //Arrange
        var bookmark = N(new BookmarkBlock
        {
            Bookmark = new BookmarkBlock.Info
            {
                Url = "https://drakon.example.com",
                Caption = Runs(T("The DRAKON site."))
            }
        });

        //Act
        var (section, _) = Render(bookmark);

        //Assert
        var texts = TestDom.AllTexts(section);
        texts.Any(t => t.Contains("drakon.example.com")).Should().Be(true);
        texts.Any(t => t.Contains("The DRAKON site.")).Should().Be(true);
    }

    [Fact]
    public void link_preview_block_renders_a_link_card()
    {
        //Arrange
        var preview = N(new LinkPreviewBlock
        {
            LinkPreview = new LinkPreviewBlock.Data { Url = "https://github.com/example/repo" }
        });

        //Act
        var (section, _) = Render(preview);

        //Assert
        TestDom.AllTexts(section).Any(t => t.Contains("github.com")).Should().Be(true);
    }

    // ── Equations, tables, columns ───────────────────────────────────────

    [Fact]
    public void equation_block_renders_the_expression_with_a_warning()
    {
        //Arrange
        var equation = N(new EquationBlock
        {
            Equation = new EquationBlock.Info { Expression = "e = mc^2" }
        });

        //Act
        var (section, context) = Render(equation);

        //Assert
        var paragraph = TestDom.AllParagraphs(section).Single();
        paragraph.Style.Should().Be("EquationDisplay");
        TestDom.TextOf(paragraph).Should().Be("e = mc^2");
        context.Warnings.Should().HaveCount(1);
    }

    [Fact]
    public void table_block_renders_a_booktabs_table_with_bold_header()
    {
        //Arrange
        RichTextText[][] Cells(params string[] texts) => texts.Select(t => new[] { T(t) }).ToArray();
        var table = N(
            new TableBlock
            {
                Table = new TableBlock.Info { TableWidth = 2, HasColumnHeader = true }
            },
            N(new TableRowBlock { TableRow = new TableRowBlock.Info { Cells = Cells("Year", "Event") } }),
            N(new TableRowBlock { TableRow = new TableRowBlock.Info { Cells = Cells("1986", "Buran begins") } }));

        //Act
        var (section, _) = Render(table);

        //Assert
        TestDom.AllTables(section).Should().HaveCount(1);
        var texts = TestDom.AllTexts(section);
        texts.Any(t => t.Contains("TABLE 1")).Should().Be(true);
        var header = TestDom.AllParagraphs(section).Single(p => TestDom.TextOf(p) == "Year");
        header.Format.Font.Bold.Should().Be(true);
    }

    [Fact]
    public void column_list_renders_columns_side_by_side()
    {
        //Arrange
        var columns = N(new ColumnListBlock(),
            N(new ColumnBlock(), Para("Left column.")),
            N(new ColumnBlock(), Para("Right column.")));

        //Act
        var (section, _) = Render(columns);

        //Assert
        var table = TestDom.AllTables(section).Single();
        (table.Columns.Count == 2).Should().Be(true);
        var texts = TestDom.AllTexts(section);
        texts.Any(t => t.Contains("Left column.")).Should().Be(true);
        texts.Any(t => t.Contains("Right column.")).Should().Be(true);
    }

    // ── Page furniture ───────────────────────────────────────────────────

    [Fact]
    public void table_of_contents_block_lists_the_pages_headings()
    {
        //Arrange
        var h1 = N(new HeadingOneBlock { Heading_1 = new HeadingOneBlock.Info { RichText = Runs(T("Alpha")) } });
        var h2 = N(new HeadingTwoBlock { Heading_2 = new HeadingTwoBlock.Info { RichText = Runs(T("Beta")) } });
        var toc = N(new TableOfContentsBlock());

        //Act
        var (section, _) = Render(toc, h1, h2);

        //Assert
        var tocLines = TestDom.AllParagraphs(section).Where(p => p.Style == "TocLine").ToList();
        tocLines.Should().HaveCount(2);
        TestDom.TextOf(tocLines[0]).Should().Be("Alpha");
        TestDom.TextOf(tocLines[1]).Should().Be("Beta");
        (tocLines[1].Format.LeftIndent.Point > tocLines[0].Format.LeftIndent.Point).Should().Be(true);
    }

    [Fact]
    public void breadcrumb_block_renders_the_ancestor_path()
    {
        //Arrange
        var (_, section, context, renderer) = TestDom.CreateRenderer();

        //Act
        renderer.RenderPage(section, [N(new BreadcrumbBlock())],
            ancestorTitles: ["The Father of DRAKON", "Chapter 3"]);

        //Assert
        var paragraph = TestDom.AllParagraphs(section).Single();
        paragraph.Style.Should().Be("BreadcrumbLine");
        TestDom.TextOf(paragraph).Should().Contain(BookTheme.Letterspace("Chapter 3"));
        _ = context;
    }

    [Fact]
    public void link_to_page_in_the_book_adds_a_page_reference()
    {
        //Arrange
        var link = N(new LinkToPageBlock
        {
            LinkToPage = new LinkPageToPage { PageId = "11111111-2222-3333-4444-555555555555" }
        });

        //Act
        var (section, _) = Render(
            context => context.PagesInBook["11111111-2222-3333-4444-555555555555"] =
                new BookPageRef { Title = "Chapter 7", BookmarkName = "ch.7" },
            link);

        //Assert
        var paragraph = TestDom.AllParagraphs(section).Single();
        paragraph.Style.Should().Be("RefLine");
        TestDom.TextOf(paragraph).Should().Contain("See Chapter 7, page");
    }

    [Fact]
    public void link_to_page_outside_the_book_renders_a_generic_line_and_note()
    {
        //Arrange
        var link = N(new LinkToPageBlock
        {
            LinkToPage = new LinkPageToPage { PageId = "99999999-9999-9999-9999-999999999999" }
        });

        //Act
        var (section, context) = Render(link);

        //Assert
        TestDom.TextOf(TestDom.AllParagraphs(section).Single())
            .Should().Be("See the linked page in Notion.");
        context.Notes.Should().HaveCount(1);
    }

    [Fact]
    public void synced_block_renders_its_children_inline()
    {
        //Arrange - the reader resolves synced content into the node's children
        var synced = N(new SyncedBlockBlock(), Para("Synced content."));

        //Act
        var (section, _) = Render(synced);

        //Assert
        TestDom.TextOf(TestDom.AllParagraphs(section).Single()).Should().Be("Synced content.");
    }

    [Fact]
    public void template_block_renders_nothing_and_logs_a_note()
    {
        //Act
        var (section, context) = Render(N(new TemplateBlock()));

        //Assert
        TestDom.AllParagraphs(section).Should().HaveCount(0);
        context.Notes.Should().HaveCount(1);
        context.Warnings.Should().HaveCount(0);
    }

    [Fact]
    public void transcription_block_renders_transcript_style()
    {
        //Arrange
        var transcription = N(new TranscriptionBlock
        {
            Transcription = new TranscriptionBlockResponse { Title = Runs(T("Meeting notes")) }
        });

        //Act
        var (section, _) = Render(transcription);

        //Assert
        var paragraph = TestDom.AllParagraphs(section).Single();
        paragraph.Style.Should().Be("TranscriptText");
        TestDom.TextOf(paragraph).Should().Be("Meeting notes");
    }

    [Fact]
    public void unsupported_block_renders_a_visible_marker_and_warning()
    {
        //Act
        var (section, context) = Render(N(new UnsupportedBlock()));

        //Assert
        TestDom.TextOf(TestDom.AllParagraphs(section).Single())
            .Should().Be("[unsupported Notion block]");
        context.Warnings.Should().HaveCount(1);
    }

    [Fact]
    public void unrecognised_block_type_renders_like_unsupported()
    {
        //Act - BlockType is extensible, so future types deserialise instead of throwing
        var (section, context) = Render(N(new FakeFutureBlock()));

        //Assert
        TestDom.TextOf(TestDom.AllParagraphs(section).Single())
            .Should().Be("[unsupported Notion block]");
        context.Warnings.Should().HaveCount(1);
    }

    private sealed class FakeFutureBlock : IBlock
    {
        public BlockType Type { get; set; } = new("holo_deck");
        public bool HasChildren { get; set; }
        public bool InTrash { get; set; }
        public IParentOfBlock Parent { get; set; }
        public string Id { get; set; } = "future-1";
        public ObjectType Object => ObjectType.Block;
        public DateTime CreatedTime { get; set; }
        public DateTime LastEditedTime { get; set; }
        public PartialUser CreatedBy { get; set; }
        public PartialUser LastEditedBy { get; set; }
    }
}
