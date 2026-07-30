using CodeBrix.PdfDocCreate.DocumentObjectModel;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// Defines every named paragraph style the book uses, derived from the trim-size
/// theme. Shared by the block renderer (chapter content) and the book composer
/// (cover, folios, running heads) so the whole volume reads as one design.
/// </summary>
internal static class BookStyles
{
    /// <summary>Adds all book styles to the document, deriving metrics from the theme.</summary>
    public static void Define(Document document, BookTheme theme)
    {
        var t = theme;

        var normal = document.Styles["Normal"];
        normal.Font.Name = BookFonts.SerifFamily;
        normal.Font.Size = t.BodySize;
        normal.Font.Color = BookTheme.Ink;
        normal.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
        normal.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
        normal.ParagraphFormat.LineSpacing = t.Leading;
        normal.ParagraphFormat.SpaceAfter = 0;
        normal.ParagraphFormat.WidowControl = true;

        //Body paragraph that opens a section (no indent), and continuation paragraphs
        //  (classic book first-line indent, no inter-paragraph space)
        var bodyOpen = document.AddStyle("BodyOpen", "Normal");
        bodyOpen.ParagraphFormat.FirstLineIndent = 0;

        var bodyIndented = document.AddStyle("BodyIndented", "Normal");
        bodyIndented.ParagraphFormat.FirstLineIndent = Unit.FromPoint(t.BodySize * 1.55);

        var sectionRule = document.AddStyle("SectionRule", "Normal");
        sectionRule.Font.Size = 2;
        sectionRule.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        sectionRule.ParagraphFormat.Borders.Top.Width = 1.1;
        sectionRule.ParagraphFormat.Borders.Top.Color = BookTheme.Accent;
        sectionRule.ParagraphFormat.LeftIndent = Unit.FromPoint((t.TextWidth - 54) / 2);
        sectionRule.ParagraphFormat.RightIndent = Unit.FromPoint((t.TextWidth - 54) / 2);
        sectionRule.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 2.7);
        sectionRule.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 1.0);
        sectionRule.ParagraphFormat.KeepWithNext = true;

        var heading1 = document.Styles["Heading1"];
        heading1.Font.Name = BookFonts.SerifFamily;
        heading1.Font.Size = t.H1Size;
        heading1.Font.Bold = false;
        heading1.Font.Color = BookTheme.Ink;
        heading1.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        heading1.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        heading1.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 2.0);
        heading1.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 1.25);
        heading1.ParagraphFormat.KeepWithNext = true;
        heading1.ParagraphFormat.OutlineLevel = OutlineLevel.Level2;

        var heading2 = document.Styles["Heading2"];
        heading2.Font.Name = BookFonts.SerifFamily;
        heading2.Font.Size = t.H2Size;
        heading2.Font.Bold = true;
        heading2.Font.Color = BookTheme.Ink;
        heading2.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        heading2.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        heading2.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 1.7);
        heading2.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 0.5);
        heading2.ParagraphFormat.KeepWithNext = true;
        heading2.ParagraphFormat.OutlineLevel = OutlineLevel.Level3;

        var heading3 = document.Styles["Heading3"];
        heading3.Font.Name = BookFonts.SerifFamily;
        heading3.Font.Size = t.H3Size;
        heading3.Font.Bold = true;
        heading3.Font.Italic = true;
        heading3.Font.Color = BookTheme.Ink;
        heading3.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        heading3.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        heading3.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 1.3);
        heading3.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 0.35);
        heading3.ParagraphFormat.KeepWithNext = true;
        heading3.ParagraphFormat.OutlineLevel = OutlineLevel.Level4;

        var listItem = document.AddStyle("ListItem", "Normal");
        listItem.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        listItem.ParagraphFormat.LeftIndent = Unit.FromPoint(t.BodySize * 1.6);
        listItem.ParagraphFormat.FirstLineIndent = Unit.FromPoint(-t.BodySize * 1.0);
        listItem.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 0.18);
        listItem.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 0.18);

        //Block quote: a left accent rule with slightly smaller italic serif
        var quote = document.AddStyle("Quote", "Normal");
        quote.Font.Italic = true;
        quote.Font.Size = t.QuoteSize;
        quote.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        quote.ParagraphFormat.LeftIndent = Unit.FromPoint(t.BodySize * 1.8);
        quote.ParagraphFormat.RightIndent = Unit.FromPoint(t.TextWidth * 0.07);
        quote.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 1.0);
        quote.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 1.0);
        quote.ParagraphFormat.Borders.Left.Width = 1.4;
        quote.ParagraphFormat.Borders.Left.Color = BookTheme.Accent;
        quote.ParagraphFormat.Borders.DistanceFromLeft = 8;

        var figure = document.AddStyle("Figure", "Normal");
        figure.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        figure.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        figure.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 1.25);
        figure.ParagraphFormat.SpaceAfter = 0;
        figure.ParagraphFormat.KeepWithNext = true;

        var caption = document.AddStyle("Caption", "Normal");
        caption.Font.Name = BookFonts.SansFamily;
        caption.Font.Size = t.CaptionSize;
        caption.Font.Color = BookTheme.Muted;
        caption.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        caption.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        caption.ParagraphFormat.LineSpacing = Unit.FromPoint(t.CaptionSize * 1.35);
        caption.ParagraphFormat.LeftIndent = Unit.FromPoint(t.TextWidth * 0.08);
        caption.ParagraphFormat.RightIndent = Unit.FromPoint(t.TextWidth * 0.08);
        caption.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.CaptionSize * 0.7);
        caption.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 1.35);

        //Image credit: a whisper-small line hugging the image, sitting directly beneath it and
        //  above the figure caption. Exactly (not AtLeast) line spacing keeps the tiny line box
        //  from reserving a full text line's worth of empty space above the credit.
        var creditSize = t.LabelSize * 0.8;
        var credit = document.AddStyle("Credit", "Caption");
        credit.Font.Size = creditSize;
        credit.Font.Italic = true;
        credit.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
        credit.ParagraphFormat.LineSpacing = Unit.FromPoint(creditSize * 1.12);
        credit.ParagraphFormat.SpaceBefore = Unit.FromPoint(creditSize * 0.15);
        credit.ParagraphFormat.SpaceAfter = 0;
        credit.ParagraphFormat.KeepWithNext = true;

        var tableCaption = document.AddStyle("TableCaption", "Caption");
        tableCaption.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 1.25);
        tableCaption.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.CaptionSize * 0.8);
        tableCaption.ParagraphFormat.KeepWithNext = true;

        var tableText = document.AddStyle("TableText", "Normal");
        tableText.Font.Name = BookFonts.SansFamily;
        tableText.Font.Size = t.TableSize;
        tableText.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        tableText.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        tableText.ParagraphFormat.LineSpacing = Unit.FromPoint(t.TableSize * 1.3);

        var runningHead = document.AddStyle("RunningHead", "Normal");
        runningHead.Font.Name = BookFonts.SansFamily;
        runningHead.Font.Size = t.LabelSize;
        runningHead.Font.Color = BookTheme.Muted;
        runningHead.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        runningHead.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        runningHead.ParagraphFormat.Borders.Bottom.Width = 0.4;
        runningHead.ParagraphFormat.Borders.Bottom.Color = BookTheme.Hairline;
        runningHead.ParagraphFormat.Borders.DistanceFromBottom = 3;

        var folio = document.AddStyle("Folio", "Normal");
        folio.Font.Size = t.FolioSize;
        folio.Font.Color = BookTheme.Muted;
        folio.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        folio.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;

        //Chapter opener: a letterspaced kicker line, then the page title as a display heading
        var chapterKicker = document.AddStyle("ChapterKicker", "Normal");
        chapterKicker.Font.Name = BookFonts.SansFamily;
        chapterKicker.Font.Size = t.BodySize * 0.78;
        chapterKicker.Font.Color = BookTheme.Muted;
        chapterKicker.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        chapterKicker.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        chapterKicker.ParagraphFormat.KeepWithNext = true;

        var chapterTitle = document.AddStyle("ChapterTitle", "Normal");
        chapterTitle.Font.Size = t.H1Size * 1.35;
        chapterTitle.Font.Color = BookTheme.Ink;
        chapterTitle.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        chapterTitle.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        chapterTitle.ParagraphFormat.LineSpacing = Unit.FromPoint(t.H1Size * 1.45);
        chapterTitle.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 1.2);
        chapterTitle.ParagraphFormat.KeepWithNext = true;
        chapterTitle.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;

        //Code panel: language label above, monospace lines on a tinted panel
        var codeLabel = document.AddStyle("CodeLabel", "Normal");
        codeLabel.Font.Name = BookFonts.SansFamily;
        codeLabel.Font.Size = t.LabelSize;
        codeLabel.Font.Color = BookTheme.Muted;
        codeLabel.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        codeLabel.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        codeLabel.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 1.1);
        codeLabel.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.LabelSize * 0.5);
        codeLabel.ParagraphFormat.KeepWithNext = true;

        var codeText = document.AddStyle("CodeText", "Normal");
        codeText.Font.Name = BookFonts.MonoFamily;
        codeText.Font.Size = t.BodySize * 0.82;
        codeText.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        codeText.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        codeText.ParagraphFormat.LineSpacing = Unit.FromPoint(t.BodySize * 0.82 * 1.35);

        //Callout sidebar body (sans, a touch smaller than body serif)
        var calloutText = document.AddStyle("CalloutText", "Normal");
        calloutText.Font.Name = BookFonts.SansFamily;
        calloutText.Font.Size = t.BodySize * 0.92;
        calloutText.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        calloutText.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        calloutText.ParagraphFormat.LineSpacing = Unit.FromPoint(t.BodySize * 0.92 * 1.42);
        calloutText.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 0.45);

        //Media/link cards
        var cardLabel = document.AddStyle("CardLabel", "Normal");
        cardLabel.Font.Name = BookFonts.SansFamily;
        cardLabel.Font.Size = t.LabelSize;
        cardLabel.Font.Bold = true;
        cardLabel.Font.Color = BookTheme.Accent;
        cardLabel.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        cardLabel.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        cardLabel.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.LabelSize * 0.4);

        var cardTitle = document.AddStyle("CardTitle", "Normal");
        cardTitle.Font.Name = BookFonts.SansFamily;
        cardTitle.Font.Size = t.BodySize * 0.95;
        cardTitle.Font.Bold = true;
        cardTitle.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        cardTitle.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        cardTitle.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 0.25);

        var cardMeta = document.AddStyle("CardMeta", "Normal");
        cardMeta.Font.Name = BookFonts.SansFamily;
        cardMeta.Font.Size = t.CaptionSize;
        cardMeta.Font.Color = BookTheme.Muted;
        cardMeta.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        cardMeta.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        cardMeta.ParagraphFormat.LineSpacing = Unit.FromPoint(t.CaptionSize * 1.3);

        var cardUrl = document.AddStyle("CardUrl", "Normal");
        cardUrl.Font.Name = BookFonts.MonoFamily;
        cardUrl.Font.Size = t.LabelSize * 0.9;
        cardUrl.Font.Color = BookTheme.Muted;
        cardUrl.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        cardUrl.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        cardUrl.ParagraphFormat.LineSpacing = Unit.FromPoint(t.LabelSize * 1.2);
        cardUrl.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.LabelSize * 0.4);

        //Display equation: centred italic serif rendering of the LaTeX source
        var equationDisplay = document.AddStyle("EquationDisplay", "Normal");
        equationDisplay.Font.Italic = true;
        equationDisplay.Font.Size = t.QuoteSize;
        equationDisplay.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        equationDisplay.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        equationDisplay.ParagraphFormat.LineSpacing = Unit.FromPoint(t.QuoteSize * 1.4);
        equationDisplay.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 0.9);
        equationDisplay.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 0.9);

        //Transcript: an indented block in slightly smaller serif
        var transcriptText = document.AddStyle("TranscriptText", "Normal");
        transcriptText.Font.Size = t.BodySize * 0.9;
        transcriptText.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        transcriptText.ParagraphFormat.LeftIndent = Unit.FromPoint(t.BodySize * 2.0);
        transcriptText.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        transcriptText.ParagraphFormat.LineSpacing = Unit.FromPoint(t.BodySize * 0.9 * 1.42);

        //Breadcrumb: a small-caps ancestor path line
        var breadcrumbLine = document.AddStyle("BreadcrumbLine", "Normal");
        breadcrumbLine.Font.Name = BookFonts.SansFamily;
        breadcrumbLine.Font.Size = t.LabelSize;
        breadcrumbLine.Font.Color = BookTheme.Muted;
        breadcrumbLine.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        breadcrumbLine.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        breadcrumbLine.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 0.8);

        //Cross-reference lines ("Continues in: …", "See …")
        var refLine = document.AddStyle("RefLine", "Normal");
        refLine.Font.Italic = true;
        refLine.Font.Size = t.BodySize * 0.9;
        refLine.Font.Color = BookTheme.Muted;
        refLine.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        refLine.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        refLine.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 0.4);
        refLine.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 0.4);

        //In-page table_of_contents block: one line per heading, indented by level
        var tocLine = document.AddStyle("TocLine", "Normal");
        tocLine.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        tocLine.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        tocLine.ParagraphFormat.LineSpacing = Unit.FromPoint(t.Leading * 1.05);

        //Visible marker for unsupported blocks
        var unsupportedMarker = document.AddStyle("UnsupportedMarker", "Normal");
        unsupportedMarker.Font.Name = BookFonts.SansFamily;
        unsupportedMarker.Font.Size = t.LabelSize;
        unsupportedMarker.Font.Color = BookTheme.Muted;
        unsupportedMarker.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        unsupportedMarker.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
        unsupportedMarker.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 0.5);
        unsupportedMarker.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 0.5);

        //Cover styles (used by the composer)
        var coverKicker = document.AddStyle("CoverKicker", "Normal");
        coverKicker.Font.Name = BookFonts.SansFamily;
        coverKicker.Font.Size = t.BodySize * 0.78;
        coverKicker.Font.Color = BookTheme.Muted;
        coverKicker.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        coverKicker.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;

        var coverTitle = document.AddStyle("CoverTitle", "Normal");
        coverTitle.Font.Size = t.CoverTitleSize;
        coverTitle.Font.Color = BookTheme.Ink;
        coverTitle.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        coverTitle.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        coverTitle.ParagraphFormat.LineSpacing = Unit.FromPoint(t.CoverTitleSize * 1.08);

        var coverSubtitle = document.AddStyle("CoverSubtitle", "Normal");
        coverSubtitle.Font.Italic = true;
        coverSubtitle.Font.Size = t.BodySize * 1.33;
        coverSubtitle.Font.Color = BookTheme.Muted;
        coverSubtitle.ParagraphFormat.Alignment = ParagraphAlignment.Center;
        coverSubtitle.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
        coverSubtitle.ParagraphFormat.LineSpacing = Unit.FromPoint(t.BodySize * 1.7);

        var coverRule = document.AddStyle("CoverRule", "SectionRule");
        coverRule.ParagraphFormat.SpaceBefore = Unit.FromPoint(t.BodySize * 1.5);
        coverRule.ParagraphFormat.SpaceAfter = Unit.FromPoint(t.BodySize * 1.5);
        coverRule.ParagraphFormat.KeepWithNext = false;
    }
}
