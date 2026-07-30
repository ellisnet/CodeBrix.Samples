using CodeBrix.NotionApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NotionDocumentCreator.CreateDocument.Internal;

/// <summary>
/// Small conversions shared by the tree and page readers: ID normalisation,
/// timestamp handling, and mapping Notion API objects to display values.
/// </summary>
internal static class NotionConvert
{
    /// <summary>
    /// Normalises user input into a canonical hyphenated Notion ID. Accepts a bare
    /// 32-hex ID, a hyphenated ID, or a full Notion URL (the query string is dropped
    /// first, because a URL's ?v=…/?p=… parameters carry other object IDs). Returns
    /// the trimmed input unchanged when no ID can be found, letting the API reject it.
    /// </summary>
    public static string NormalizeId(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) { return ""; }

        var value = input.Trim();
        var queryStart = value.IndexOf('?');
        if (queryStart >= 0) { value = value[..queryStart]; }

        var compact = value.Replace("-", "");
        var matches = Regex.Matches(compact, "[0-9a-fA-F]{32}");
        if (matches.Count == 0) { return input.Trim(); }

        var hex = matches[^1].Value.ToLowerInvariant();
        return $"{hex[..8]}-{hex[8..12]}-{hex[12..16]}-{hex[16..20]}-{hex[20..]}";
    }

    /// <summary>
    /// Interprets a Notion API timestamp as UTC (the API always returns UTC, but the
    /// deserialised <see cref="DateTime"/> kind cannot be relied on).
    /// </summary>
    public static DateTimeOffset AsUtc(DateTime value) =>
        new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    /// <summary>Concatenates a rich-text run list into its plain text.</summary>
    public static string PlainText(IEnumerable<RichTextBase> richText) =>
        richText is null ? "" : string.Concat(richText.Select(rt => rt?.PlainText ?? ""));

    /// <summary>
    /// Extracts a page's title. The title property's key varies (it is the database's
    /// title column name for database rows), so the lookup matches on property type.
    /// </summary>
    public static string TitleOf(Page page)
    {
        var values = page?.Properties?.Values;
        if (values is null) { return "Untitled"; }

        var title = PlainText(values.OfType<TitlePropertyValue>()
            .SelectMany(t => t.Title ?? [])).Trim();
        return title.Length == 0 ? "Untitled" : title;
    }

    /// <summary>Extracts a database's title.</summary>
    public static string TitleOf(Database database)
    {
        var title = PlainText(database?.Title).Trim();
        return title.Length == 0 ? "Untitled database" : title;
    }

    /// <summary>
    /// Maps a page icon to its display form: an emoji character, or an image URL.
    /// At most one of the two is non-empty.
    /// </summary>
    public static (string Emoji, string Url) IconOf(IPageIcon icon) => icon switch
    {
        EmojiPageIcon emoji => (emoji.Emoji ?? "", ""),
        CustomEmojiPageIcon custom => ("", custom.CustomEmoji?.Url ?? ""),
        FilePageIcon file => ("", file.File?.Url ?? ""),
        ExternalPageIcon external => ("", external.External?.Url ?? ""),
        _ => ("", "")
    };

    /// <summary>Maps a page cover to its image URL (empty when there is no cover).</summary>
    public static string CoverUrlOf(IPageCover cover) => cover switch
    {
        FilePageCover file => file.File?.Url ?? "",
        ExternalPageCover external => external.External?.Url ?? "",
        _ => ""
    };

    /// <summary>
    /// The plain text a block contributes to a page preview; empty for blocks with
    /// no directly displayable text (images, dividers, child pages, …).
    /// </summary>
    public static string PlainTextOf(IBlock block) => block switch
    {
        ParagraphBlock paragraph => PlainText(paragraph.Paragraph?.RichText),
        HeadingOneBlock heading1 => PlainText(heading1.Heading_1?.RichText),
        HeadingTwoBlock heading2 => PlainText(heading2.Heading_2?.RichText),
        HeadingThreeBlock heading3 => PlainText(heading3.Heading_3?.RichText),
        QuoteBlock quote => PlainText(quote.Quote?.RichText),
        CalloutBlock callout => PlainText(callout.Callout?.RichText),
        BulletedListItemBlock bulleted => PlainText(bulleted.BulletedListItem?.RichText),
        NumberedListItemBlock numbered => PlainText(numbered.NumberedListItem?.RichText),
        ToDoBlock toDo => PlainText(toDo.ToDo?.RichText),
        ToggleBlock toggle => PlainText(toggle.Toggle?.RichText),
        _ => ""
    };
}
