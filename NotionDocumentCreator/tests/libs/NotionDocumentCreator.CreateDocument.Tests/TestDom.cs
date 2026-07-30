using CodeBrix.PdfDocCreate.DocumentObjectModel;
using CodeBrix.PdfDocCreate.DocumentObjectModel.Tables;
using NotionDocumentCreator.CreateDocument.Internal;
using NotionDocumentCreator.CreateDocument.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MdText = CodeBrix.PdfDocCreate.DocumentObjectModel.Text;

namespace NotionDocumentCreator.CreateDocument.Tests;

/// <summary>
/// Helpers for composing a themed test document and inspecting the MigraDoc DOM
/// the block renderer produced.
/// </summary>
internal static class TestDom
{
    /// <summary>Creates a styled document, one section, and a renderer over a fresh context.</summary>
    public static (Document Document, Section Section, RenderContext Context, BlockRenderer Renderer)
        CreateRenderer(Action<RenderContext> configure = null)
    {
        var theme = BookTheme.For(PageSizeOption.EightByTen);
        var document = new Document();
        BookStyles.Define(document, theme);
        var section = document.AddSection();
        var context = new RenderContext { Theme = theme };
        configure?.Invoke(context);
        return (document, section, context, new BlockRenderer(context));
    }

    /// <summary>Every paragraph in the section, including paragraphs inside table cells.</summary>
    public static List<Paragraph> AllParagraphs(Section section) =>
        CollectParagraphs(section.Elements).ToList();

    /// <summary>Every table in the section (top level only).</summary>
    public static List<Table> AllTables(Section section) =>
        section.Elements.Cast<object>().OfType<Table>().ToList();

    /// <summary>The concatenated plain text of one paragraph (line breaks become \n).</summary>
    public static string TextOf(Paragraph paragraph) => TextOfElements(paragraph.Elements);

    /// <summary>All paragraph texts in DOM order.</summary>
    public static List<string> AllTexts(Section section) =>
        AllParagraphs(section).Select(TextOf).ToList();

    /// <summary>All formatted-text runs of a paragraph, including runs inside hyperlinks.</summary>
    public static List<FormattedText> FormattedRuns(Paragraph paragraph) =>
        CollectRuns(paragraph.Elements).ToList();

    /// <summary>All hyperlinks of a paragraph.</summary>
    public static List<Hyperlink> Hyperlinks(Paragraph paragraph) =>
        paragraph.Elements.Cast<object>().OfType<Hyperlink>().ToList();

    private static IEnumerable<Paragraph> CollectParagraphs(IEnumerable elements)
    {
        foreach (var element in elements)
        {
            switch (element)
            {
                case Paragraph paragraph:
                    yield return paragraph;
                    break;
                case Table table:
                    foreach (var row in table.Rows.Cast<Row>())
                    {
                        foreach (var cell in row.Cells.Cast<Cell>())
                        {
                            foreach (var nested in CollectParagraphs(cell.Elements))
                            {
                                yield return nested;
                            }
                        }
                    }
                    break;
            }
        }
    }

    private static IEnumerable<FormattedText> CollectRuns(IEnumerable elements)
    {
        foreach (var element in elements)
        {
            switch (element)
            {
                case FormattedText formatted:
                    yield return formatted;
                    foreach (var nested in CollectRuns(formatted.Elements)) { yield return nested; }
                    break;
                case Hyperlink hyperlink:
                    foreach (var nested in CollectRuns(hyperlink.Elements)) { yield return nested; }
                    break;
            }
        }
    }

    private static string TextOfElements(IEnumerable elements)
    {
        var parts = new List<string>();
        foreach (var element in elements)
        {
            switch (element)
            {
                case MdText text:
                    parts.Add(text.Content);
                    break;
                case FormattedText formatted:
                    parts.Add(TextOfElements(formatted.Elements));
                    break;
                case Hyperlink hyperlink:
                    parts.Add(TextOfElements(hyperlink.Elements));
                    break;
                case Character:
                    parts.Add("\n");
                    break;
            }
        }
        return string.Concat(parts);
    }
}
