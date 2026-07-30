using CodeBrix.NotionApi;
using CodeBrix.PdfDocCreate.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// Writes Notion rich-text runs into a MigraDoc paragraph: bold, italic,
/// strikethrough, underline, inline code (Source Code Pro), links, inline
/// equations and mentions. Characters the embedded fonts cannot render are
/// routed to the Noto Emoji face when it covers them, and dropped otherwise —
/// never printed as tofu.
/// </summary>
internal sealed class RichTextWriter
{
    private readonly BookTheme _theme;
    private readonly IList<string> _warnings;

    public RichTextWriter(BookTheme theme, IList<string> warnings)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _warnings = warnings ?? [];
    }

    /// <summary>Total characters dropped because no embedded font covers them.</summary>
    public int DroppedCharacterCount { get; private set; }

    /// <summary>Appends the rich-text runs to the paragraph.</summary>
    public void Append(Paragraph paragraph, IEnumerable<RichTextBase> richText)
    {
        ArgumentNullException.ThrowIfNull(paragraph);
        if (richText is null) { return; }

        foreach (var run in richText)
        {
            if (run is null) { continue; }

            if (run is RichTextEquation equation)
            {
                AppendInlineEquation(paragraph, equation);
                continue;
            }

            //Text runs, mentions (which read as their plain text) and unknown runs
            var text = run.PlainText ?? "";
            if (text.Length == 0) { continue; }

            var url = run.Href;
            if (string.IsNullOrWhiteSpace(url) && run is RichTextText richTextText)
            {
                url = richTextText.Text?.Link?.Url;
            }

            if (!string.IsNullOrWhiteSpace(url))
            {
                var hyperlink = paragraph.AddHyperlink(url, HyperlinkType.Web);
                AppendAnnotated(new HyperlinkTarget(hyperlink), text, run.Annotations, isLink: true);
            }
            else
            {
                AppendAnnotated(new ParagraphTarget(paragraph), text, run.Annotations, isLink: false);
            }
        }
    }

    private void AppendInlineEquation(Paragraph paragraph, RichTextEquation equation)
    {
        var expression = equation.Equation?.Expression ?? equation.PlainText ?? "";
        if (expression.Length == 0) { return; }

        var formatted = paragraph.AddFormattedText(Sanitize(expression, FontCoverage.SerifRegular));
        formatted.Font.Italic = true;
        _warnings.Add($"Equation rendered as its LaTeX source (no math typesetting): {Shorten(expression)}");
    }

    private void AppendAnnotated(IRunTarget target, string text, Annotations annotations, bool isLink)
    {
        //Split the text into segments the body font covers and segments only the
        //  emoji face covers; anything neither can render is dropped (no tofu)
        foreach (var (segment, isEmoji) in Segment(text))
        {
            for (var lineStart = 0; lineStart < segment.Length;)
            {
                var newline = segment.IndexOf('\n', lineStart);
                var line = newline < 0 ? segment[lineStart..] : segment[lineStart..newline];

                if (line.Length > 0)
                {
                    var formatted = target.AddFormattedText(line);
                    ApplyAnnotations(formatted, annotations, isLink, isEmoji);
                }
                if (newline < 0) { break; }
                target.AddLineBreak();
                lineStart = newline + 1;
            }
        }
    }

    private void ApplyAnnotations(FormattedText formatted, Annotations annotations, bool isLink, bool isEmoji)
    {
        if (isEmoji)
        {
            formatted.Font.Name = BookFonts.EmojiFamily;
        }

        if (isLink)
        {
            formatted.Font.Color = BookTheme.Accent;
        }

        if (annotations is null) { return; }

        if (annotations.IsBold) { formatted.Font.Bold = true; }
        if (annotations.IsItalic) { formatted.Font.Italic = true; }
        if (annotations.IsUnderline) { formatted.Font.Underline = Underline.Single; }
        if (annotations.IsStrikeThrough) { formatted.Font.Strikethrough = Strikethrough.Single; }
        if (annotations.IsCode && !isEmoji)
        {
            formatted.Font.Name = BookFonts.MonoFamily;
            formatted.Font.Size = _theme.BodySize * 0.88;
        }
    }

    /// <summary>
    /// Splits text into runs by font coverage. Variation selectors and joiners are
    /// removed silently; genuinely unrenderable characters are counted and dropped.
    /// </summary>
    private IEnumerable<(string Text, bool IsEmoji)> Segment(string text)
    {
        var current = new StringBuilder();
        var currentIsEmoji = false;
        var segments = new List<(string, bool)>();

        void Flush()
        {
            if (current.Length > 0)
            {
                segments.Add((current.ToString(), currentIsEmoji));
                current.Clear();
            }
        }

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            int codepoint = c;
            var charCount = 1;
            if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                codepoint = char.ConvertToUtf32(c, text[i + 1]);
                charCount = 2;
            }

            //Variation selectors and the zero-width joiner shape emoji sequences the
            //  monochrome face cannot compose anyway — drop them without counting
            if (codepoint is 0xFE0E or 0xFE0F or 0x200D)
            {
                i += charCount - 1;
                continue;
            }

            bool isEmoji;
            if (codepoint is '\n' or '\r' or '\t' || FontCoverage.Covers(FontCoverage.SerifRegular, codepoint))
            {
                isEmoji = false;
                if (codepoint == '\r') { i += charCount - 1; continue; } //Normalise CRLF to the \n that follows
            }
            else if (FontCoverage.EmojiPrintable(codepoint))
            {
                isEmoji = true;
            }
            else
            {
                DroppedCharacterCount++;
                i += charCount - 1;
                continue;
            }

            if (isEmoji != currentIsEmoji) { Flush(); currentIsEmoji = isEmoji; }
            current.Append(text, i, charCount);
            i += charCount - 1;
        }

        Flush();
        return segments;
    }

    /// <summary>Drops characters the given font cannot render (no tidy-up pass).</summary>
    private string Sanitize(string text, string fontFileName)
    {
        var builder = new StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            int codepoint = c;
            var charCount = 1;
            if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                codepoint = char.ConvertToUtf32(c, text[i + 1]);
                charCount = 2;
            }

            if (codepoint is '\n' or '\t' || FontCoverage.Covers(fontFileName, codepoint))
            {
                builder.Append(text, i, charCount);
            }
            else
            {
                DroppedCharacterCount++;
            }
            i += charCount - 1;
        }
        return builder.ToString();
    }

    private static string Shorten(string text) =>
        text.Length <= 60 ? text : text[..60] + "…";

    //Paragraph and Hyperlink share no add-text base type, so a tiny adapter pair
    //  lets annotated runs target either
    private interface IRunTarget
    {
        FormattedText AddFormattedText(string text);
        void AddLineBreak();
    }

    private sealed class ParagraphTarget : IRunTarget
    {
        private readonly Paragraph _paragraph;
        public ParagraphTarget(Paragraph paragraph) { _paragraph = paragraph; }
        public FormattedText AddFormattedText(string text) => _paragraph.AddFormattedText(text);
        public void AddLineBreak() => _paragraph.AddLineBreak();
    }

    private sealed class HyperlinkTarget : IRunTarget
    {
        private readonly Hyperlink _hyperlink;
        public HyperlinkTarget(Hyperlink hyperlink) { _hyperlink = hyperlink; }
        public FormattedText AddFormattedText(string text) => _hyperlink.AddFormattedText(text);

        //Hyperlinks cannot hold line breaks — a newline inside link text becomes a space
        public void AddLineBreak() => _hyperlink.AddText(" ");
    }
}
