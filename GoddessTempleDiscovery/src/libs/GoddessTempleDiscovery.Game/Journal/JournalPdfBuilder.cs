using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CodeBrix.PdfDocuments.Drawing;
using CodeBrix.PdfDocuments.Pdf;
using GoddessTempleDiscovery.Assets;
using GoddessTempleDiscovery.Game.Cards;
using GoddessTempleDiscovery.Game.Credits;
using GoddessTempleDiscovery.Game.Journal.Pdf;
using GoddessTempleDiscovery.Game.Rendering;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Content;
using GoddessTempleDiscovery.Rules.Journal;
using GoddessTempleDiscovery.Rules.Scoring;

namespace GoddessTempleDiscovery.Game.Journal;

/// <summary>What the Field Journal is made from.</summary>
/// <param name="Entries">The engine's journal, in order.</param>
/// <param name="TeamNames">The teams, in seat order.</param>
/// <param name="Scores">The final scores (empty when the game is not over).</param>
/// <param name="Date">The date printed on the cover.</param>
/// <param name="Seed">The game's seed, which chooses each headline's edition (see <see cref="Editions"/>).</param>
public sealed record JournalPdfInput(IReadOnlyList<JournalEntry> Entries, IReadOnlyList<string> TeamNames, IReadOnlyList<FinalScore> Scores, DateTime Date, int Seed = 0);

/// <summary>A built Field Journal.</summary>
/// <param name="Bytes">The PDF.</param>
/// <param name="PageCount">Its pages.</param>
/// <param name="DrawnText">Every line of text drawn on its pages, in order (for tests and for search).</param>
public sealed record JournalPdf(byte[] Bytes, int PageCount, IReadOnlyList<string> DrawnText);

/// <summary>
/// The Field Journal as a PDF, hand-placed on <see cref="XGraphics"/> in the InannaRosette manner: the bound run of
/// the game's editions of the Warka Herald. A cover; then, in the order they were read, a front page for each
/// season and an EXTRA! page for each discovery (masthead, headline, columns, the art framed as a photograph, the
/// WHERE IT IS NOW and THE RECORD boxes), with the tablets, favors, specialists and reports set as the Learned
/// Society's papers and notices between them; the Final Edition with the results; the Note on Cultural Heritage and
/// History; and the sources. All art is vector (<see cref="SvgToPdf"/>).
/// </summary>
public sealed class JournalPdfBuilder
{
    private const double PageWidth = 595.28;
    private const double PageHeight = 841.89;
    private const double Margin = 42;
    private const double ColumnGap = 18;

    private static readonly XColor Ink = XColor.FromArgb(30, 26, 23);
    private static readonly XColor Paper = XColor.FromArgb(244, 239, 227);
    private static readonly XColor GoldDeep = XColor.FromArgb(168, 118, 31);
    private static readonly XColor Red = XColor.FromArgb(184, 50, 42);
    private static readonly XColor Night = XColor.FromArgb(22, 33, 58);

    private PdfDocument _document;
    private XGraphics _gfx;
    private List<string> _drawn;
    private double _y;
    private bool _flowPage;
    private int _seed;

    /// <summary>The file name a journal is suggested to be saved under.</summary>
    /// <param name="date">The date.</param>
    /// <returns>The name.</returns>
    public static string SuggestedFileName(DateTime date) =>
        "Goddess Temple Discovery - Field Journal " + date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".pdf";

    /// <summary>Builds the journal.</summary>
    /// <param name="input">The game's journal, teams and scores.</param>
    /// <returns>The PDF.</returns>
    public JournalPdf Build(JournalPdfInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        JournalFonts.EnsureRegistered();
        _drawn = new List<string>();
        _seed = input.Seed;
        using var document = new PdfDocument();
        _document = document;
        document.Info.Title = "Goddess Temple Discovery! — The Field Journal";
        document.Info.Author = "Goddess Temple Discovery";
        document.Info.Subject = "The bound run of the Warka Herald's editions of one game";
        document.Info.Creator = "Goddess Temple Discovery · JournalPdfBuilder";

        Cover(input);
        foreach (var entry in input.Entries ?? Array.Empty<JournalEntry>())
        {
            switch (entry.Kind)
            {
                case JournalEntryKind.Season:
                    SeasonPage(entry);
                    break;
                case JournalEntryKind.Discovery:
                    DiscoveryPage(entry);
                    break;
                default:
                    Notice(entry);
                    break;
            }
        }

        if (input.Scores != null && input.Scores.Count > 0)
        {
            FinalEdition(input.Scores);
        }

        Heritage();
        Sources(input.Entries ?? Array.Empty<JournalEntry>());
        _gfx?.Dispose();
        _gfx = null;

        var pages = document.PageCount;
        using var stream = new MemoryStream();
        document.Save(stream);
        return new JournalPdf(stream.ToArray(), pages, _drawn);
    }

    // ------------------------------------------------------------------ pages

    private void Cover(JournalPdfInput input)
    {
        NewPage(false);
        Art("deco-masthead-warka-herald", new XRect(Margin, 36, PageWidth - (2 * Margin), 70));
        Rule(118, 1.6);
        Text("Goddess Temple Discovery!", Font(34, XFontStyle.Bold), Ink, PageWidth / 2, 160, XStringFormats.Center);
        Rule(186, 0.8);
        Text("WARKA, IRAQ — WINTER 1912/13 — TWO TO FOUR EXPEDITIONS", Font(9, XFontStyle.Bold), GoldDeep, PageWidth / 2, 200, XStringFormats.Center);
        Text("THE FIELD JOURNAL", Font(15, XFontStyle.Bold), Ink, PageWidth / 2, 236, XStringFormats.Center);
        Text("The bound run of the editions of one game", Font(11, XFontStyle.Italic), Ink, PageWidth / 2, 256, XStringFormats.Center);
        Art("title-ziggurat-at-dawn", new XRect(Margin + 60, 280, PageWidth - (2 * Margin) - 120, 330));
        var y = 640.0;
        Text("THE EXPEDITIONS", Font(10, XFontStyle.Bold), GoldDeep, PageWidth / 2, y, XStringFormats.Center);
        foreach (var team in input.TeamNames ?? Array.Empty<string>())
        {
            y += 18;
            Text(team, Font(12, XFontStyle.Regular), Ink, PageWidth / 2, y, XStringFormats.Center);
        }

        Text(input.Date.ToString("d MMMM yyyy", CultureInfo.InvariantCulture), Font(10, XFontStyle.Italic), Ink, PageWidth / 2, PageHeight - 50, XStringFormats.Center);
    }

    private void SeasonPage(JournalEntry entry)
    {
        var season = (Catalog.Seasons ?? Array.Empty<SeasonCard>()).FirstOrDefault(s => s.Year == entry.SeasonYear);
        NewPage(false);
        Masthead("FRONT PAGE · " + CardText.Dateline(entry.SeasonYear));
        var headline = season == null ? new Catalog.HeadlinePair(entry.Title.ToUpperInvariant(), string.Empty) : Editions.For(_seed, season);
        _y = Headline(headline.Headline, headline.SubHead, _y);
        Slug(entry.Title);
        Byline(headline.Byline);
        var photo = new XRect(Margin, _y, 250, 200);
        Photograph(entry.ArtKey, photo, season?.Title ?? entry.Title);
        var notice = new XRect(Margin + 270, _y, PageWidth - (2 * Margin) - 270, 200);
        Box(notice, "NOTICE TO ALL EXPEDITIONS");
        var inner = notice.Y + 30;
        inner = Paragraph(season == null ? string.Empty : CardText.EffectLine(season), Font(11, XFontStyle.Bold), notice.X + 10, inner, notice.Width - 20);
        if (season != null)
        {
            inner = Paragraph("Directed by " + season.Director + ".", Font(9.5, XFontStyle.Italic), notice.X + 10, inner + 6, notice.Width - 20);
        }

        _y = photo.Bottom + 24;
        Columns(new[] { season?.Story ?? entry.Text, season?.LongStory ?? string.Empty }, entry.Sources);
    }

    private void DiscoveryPage(JournalEntry entry)
    {
        var card = (Catalog.Discoveries ?? Array.Empty<DiscoveryCard>()).FirstOrDefault(d => d.Title == entry.Title);
        NewPage(false);
        Masthead("EXTRA! · " + CardText.Dateline(entry.SeasonYear));
        if (card != null && card.IsStarred)
        {
            Art("deco-extra-badge", new XRect(PageWidth - Margin - 56, 88, 56, 56));
        }

        var headline = card == null ? new Catalog.HeadlinePair(entry.Title.ToUpperInvariant() + " FOUND AT WARKA", string.Empty) : Editions.For(_seed, card);
        _y = Headline(headline.Headline, headline.SubHead, _y);
        Slug(entry.Title);
        Byline(headline.Byline + (string.IsNullOrWhiteSpace(entry.TeamName) ? string.Empty : " · with " + entry.TeamName));

        var photo = new XRect(Margin, _y, 260, 230);
        Photograph(entry.ArtKey, photo, card == null ? entry.Title : CardText.ShortTitle(card.Title) + ". " + card.Level + "; " + card.ApproximateDate + ".");
        var side = new XRect(Margin + 278, _y, PageWidth - (2 * Margin) - 278, 230);
        Box(new XRect(side.X, side.Y, side.Width, 84), "WHERE IT IS NOW");
        Paragraph(card?.WhereNow ?? string.Empty, Font(9, XFontStyle.Regular), side.X + 8, side.Y + 26, side.Width - 16, side.Y + 82);
        var record = new XRect(side.X, side.Y + 94, side.Width, 136);
        Box(record, "THE RECORD");
        var ry = record.Y + 26;
        if (card != null)
        {
            ry = Paragraph("Excavated by: " + card.ExcavatedBy, Font(8.5, XFontStyle.Regular), record.X + 8, ry, record.Width - 16, record.Bottom - 4);
            ry = Paragraph("Season: " + card.SeasonFound, Font(8.5, XFontStyle.Regular), record.X + 8, ry, record.Width - 16, record.Bottom - 4);
            ry = Paragraph(string.Format(CultureInfo.InvariantCulture, "Dig Number {0} · {1} points{2}", card.DigNumber, card.Points, card.IsStarred ? " · one of Her stars" : string.Empty),
                Font(8.5, XFontStyle.Bold), record.X + 8, ry, record.Width - 16, record.Bottom - 4);
            if (!string.IsNullOrWhiteSpace(card.Cuneiform))
            {
                ry = Cuneiform(card.Cuneiform, record.X + 8, ry + 2, 14);
                Paragraph(card.CuneiformReading, Font(7.5, XFontStyle.Italic), record.X + 8, ry, record.Width - 16, record.Bottom - 4);
            }
        }

        _y = photo.Bottom + 22;
        Columns(card == null ? new[] { entry.Text } : new[] { card.CardText, card.LongText }, entry.Sources);
    }

    private void Notice(JournalEntry entry)
    {
        var heading = entry.Kind switch
        {
            JournalEntryKind.Tablet => "A PAPER READ BEFORE THE SOCIETY",
            JournalEntryKind.Favor => "A NOTICE: THE GODDESS FAVORS AN EXPEDITION",
            JournalEntryKind.Specialist => "THE EXPEDITION STAFF",
            JournalEntryKind.Report => "PUBLICATIONS RECEIVED",
            JournalEntryKind.Person => "FROM THE ARCHIVE",
            _ => "NOTES",
        };
        var font = Font(9.5, XFontStyle.Regular);
        var paragraphs = (entry.Text ?? string.Empty).Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        var hasArt = ArtCatalog.Contains(entry.ArtKey);
        var textLeft = Margin + (hasArt ? 78 : 0);
        var width = PageWidth - Margin - textLeft;
        var needed = 40 + paragraphs.Sum(p => (Lines(p, font, width).Count * 12.5) + 4);
        if (!_flowPage || _y + Math.Min(needed, 300) > PageHeight - Margin)
        {
            NewPage(true);
            Art("deco-wordmark-learned-society", new XRect(Margin + 100, 30, PageWidth - (2 * Margin) - 200, 64));
            Rule(100, 1.2);
            _y = 116;
        }

        var top = _y;
        Text(heading, Font(8, XFontStyle.Bold), GoldDeep, Margin, _y, XStringFormats.TopLeft);
        _y += 13;
        Text(entry.Title + (string.IsNullOrWhiteSpace(entry.TeamName) ? string.Empty : " — " + entry.TeamName), Font(12.5, XFontStyle.Bold), Ink, Margin, _y, XStringFormats.TopLeft);
        _y += 20;
        if (hasArt)
        {
            Art(entry.ArtKey, new XRect(Margin, _y, 66, 66));
        }

        foreach (var paragraph in paragraphs)
        {
            _y = Paragraph(paragraph, font, textLeft, _y, width, PageHeight - Margin, continueOnNewPage: true) + 4;
        }

        if (!string.IsNullOrWhiteSpace(entry.Sources))
        {
            _y = Paragraph("Sources: " + entry.Sources, Font(7.5, XFontStyle.Italic), textLeft, _y, width, PageHeight - Margin, continueOnNewPage: true);
        }

        _y = Math.Max(_y, hasArt && _y - top < 110 ? top + 110 : _y) + 10;
        Rule(_y, 0.5);
        _y += 12;
    }

    private void FinalEdition(IReadOnlyList<FinalScore> scores)
    {
        NewPage(false);
        Masthead("FINAL EDITION · WARKA, IRAQ — SPRING 1939");
        Art("deco-wordmark-final-edition", new XRect(Margin + 80, _y, PageWidth - (2 * Margin) - 160, 50));
        _y += 60;
        _y = Headline("Goddess Temple Discovery! — Final Edition".ToUpperInvariant(), "The war closes the dig; the reports are tallied.", _y);
        var columns = new[] { "TEAM", "PUBLISHED", "HALF", "TABLETS", "SETS", "STAR", "TOTAL", "RANK" };
        var widths = new[] { 175.0, 60, 40, 50, 40, 40, 50, 40 };
        var x = Margin;
        for (var i = 0; i < columns.Length; i++)
        {
            Text(columns[i], Font(8, XFontStyle.Bold), GoldDeep, x, _y, XStringFormats.TopLeft);
            x += widths[i];
        }

        _y += 16;
        Rule(_y, 0.8);
        _y += 6;
        foreach (var score in scores.OrderBy(s => s.Rank).ThenBy(s => s.TeamIndex))
        {
            var cells = new[]
            {
                score.TeamName, N(score.Published), N(score.UnpublishedHalf), N(score.Tablets), N(score.SetBonus), N(score.StarBonus),
                N(score.Total), N(score.Rank),
            };
            x = Margin;
            for (var i = 0; i < cells.Length; i++)
            {
                Text(cells[i], Font(10, i == 0 || i == 6 ? XFontStyle.Bold : XFontStyle.Regular), Ink, x, _y, XStringFormats.TopLeft);
                x += widths[i];
            }

            _y += 20;
        }

        Rule(_y, 0.8);
        var winner = scores.Where(s => s.Rank == 1).Select(s => s.TeamName).ToArray();
        if (winner.Length > 0)
        {
            Paragraph((winner.Length == 1 ? winner[0] + " has made the greatest discoveries." : string.Join(" and ", winner) + " share the honours."),
                Font(12, XFontStyle.Italic), Margin, _y + 14, PageWidth - (2 * Margin));
        }
    }

    private void Heritage()
    {
        NewPage(false);
        Rule(60, 1.2);
        Text(HeritageNote.Title, Font(16, XFontStyle.Bold), Ink, PageWidth / 2, 84, XStringFormats.Center);
        Rule(104, 1.2);
        var y = 126.0;
        y = Paragraph(HeritageNote.FirstParagraph, Font(10.5, XFontStyle.Regular), Margin, y, PageWidth - (2 * Margin), lineHeight: 15.5);
        Paragraph(HeritageNote.SecondParagraph, Font(10.5, XFontStyle.Regular), Margin, y + 10, PageWidth - (2 * Margin), lineHeight: 15.5);
    }

    private void Sources(IEnumerable<JournalEntry> entries)
    {
        NewPage(false);
        Text("SOURCES", Font(16, XFontStyle.Bold), Ink, PageWidth / 2, 60, XStringFormats.Center);
        Rule(78, 1.0);
        _y = 92;
        var all = entries
            .SelectMany(e => (e.Sources ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Concat(new[] { "Art: original vector art of Goddess Temple Discovery, Apache License 2.0.", "Fonts: Merriweather and Noto Sans Cuneiform, SIL Open Font License 1.1." })
            .Distinct(StringComparer.Ordinal)
            .OrderBy(s => s, StringComparer.Ordinal);
        foreach (var source in all)
        {
            _y = Paragraph(source, Font(8.5, XFontStyle.Regular), Margin, _y, PageWidth - (2 * Margin), PageHeight - Margin, continueOnNewPage: true) + 2;
        }
    }

    // ------------------------------------------------------------------ pieces

    private void NewPage(bool flow)
    {
        _gfx?.Dispose();
        var page = _document.AddPage();
        page.Width = PageWidth;
        page.Height = PageHeight;
        _gfx = XGraphics.FromPdfPage(page);
        _gfx.DrawRectangle(new XSolidBrush(Paper), 0, 0, PageWidth, PageHeight);
        _gfx.DrawRectangle(new XPen(GoldDeep, 0.6), 18, 18, PageWidth - 36, PageHeight - 36);
        _gfx.DrawRectangle(new XPen(GoldDeep, 0.4), 22, 22, PageWidth - 44, PageHeight - 44);
        _flowPage = flow;
        _y = Margin;
    }

    private void Masthead(string editionLine)
    {
        Art("deco-masthead-warka-herald", new XRect(Margin + 40, 30, PageWidth - (2 * Margin) - 80, 64));
        Rule(100, 1.4);
        Text(editionLine, Font(8.5, XFontStyle.Bold), GoldDeep, PageWidth / 2, 110, XStringFormats.Center);
        Rule(120, 0.6);
        _y = 132;
    }

    private double Headline(string headline, string subHead, double y)
    {
        var size = 24.0;
        IReadOnlyList<string> lines;
        do
        {
            lines = Lines(headline, Font(size, XFontStyle.Bold), PageWidth - (2 * Margin));
            size -= 1;
        }
        while (lines.Count > 2 && size > 14);

        var font = Font(size + 1, XFontStyle.Bold);
        foreach (var line in lines)
        {
            Text(line, font, Ink, PageWidth / 2, y, XStringFormats.TopCenter);
            y += (size + 1) * 1.18;
        }

        if (!string.IsNullOrWhiteSpace(subHead))
        {
            y = Paragraph(subHead, Font(11.5, XFontStyle.Italic), Margin + 30, y + 4, PageWidth - (2 * Margin) - 60, centred: true);
        }

        Rule(y + 4, 0.6);
        return y + 14;
    }

    //The card's own title, set small above the story (the headline is the paper's, the slug is the card's)
    private void Slug(string title)
    {
        Text(title, Font(10, XFontStyle.Bold), GoldDeep, Margin, _y, XStringFormats.TopLeft);
        _y += 16;
    }

    private void Byline(string byline)
    {
        if (string.IsNullOrWhiteSpace(byline))
        {
            return;
        }

        Text(byline, Font(9.5, XFontStyle.Italic), Ink, Margin, _y, XStringFormats.TopLeft);
        _y += 18;
    }

    private void Photograph(string artKey, XRect box, string caption)
    {
        _gfx.DrawRectangle(new XPen(Ink, 2.2), new XSolidBrush(XColor.FromArgb(239, 227, 200)), box.X, box.Y, box.Width, box.Height - 30);
        _gfx.DrawRectangle(new XPen(Ink, 0.5), box.X + 5, box.Y + 5, box.Width - 10, box.Height - 40);
        Art(artKey, new XRect(box.X + 8, box.Y + 8, box.Width - 16, box.Height - 46));
        Paragraph(caption, Font(8, XFontStyle.Italic), box.X, box.Bottom - 26, box.Width, box.Bottom, centred: true);
    }

    private void Box(XRect box, string heading)
    {
        _gfx.DrawRectangle(new XPen(Ink, 1.2), box.X, box.Y, box.Width, box.Height);
        _gfx.DrawRectangle(new XSolidBrush(Night), box.X, box.Y, box.Width, 18);
        Text(heading, Font(8, XFontStyle.Bold), XColor.FromArgb(237, 230, 214), box.X + (box.Width / 2), box.Y + 9, XStringFormats.Center);
    }

    private void Columns(IEnumerable<string> texts, string sources)
    {
        var width = (PageWidth - (2 * Margin) - ColumnGap) / 2;
        var font = Font(9.5, XFontStyle.Regular);
        var lines = new List<string>();
        foreach (var text in texts.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            foreach (var paragraph in text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
            {
                lines.AddRange(Lines(paragraph, font, width));
                lines.Add(string.Empty);
            }
        }

        if (!string.IsNullOrWhiteSpace(sources))
        {
            lines.Add("\u0001Sources: " + sources);
        }

        const double lineHeight = 12.6;
        var column = 0;
        var top = _y;
        var y = top;

        //Balance the two columns when the story fits on this page; otherwise fill the first to the foot of the page
        var total = lines.Sum(l => l.Length == 0 ? lineHeight / 2 : l.StartsWith('\u0001') ? lineHeight * 2 : lineHeight);
        var bottom = PageHeight - Margin;
        if (total <= 2 * (bottom - top))
        {
            bottom = Math.Min(bottom, top + (total / 2) + (2 * lineHeight));
        }
        foreach (var raw in lines)
        {
            var italic = raw.StartsWith('\u0001');
            var parts = italic ? Lines(raw.Substring(1), Font(7.5, XFontStyle.Italic), width) : new[] { raw };
            foreach (var line in parts)
            {
                if (y + lineHeight > bottom)
                {
                    if (column == 0)
                    {
                        column = 1;
                    }
                    else
                    {
                        NewPage(false);
                        Text("(continued)", Font(8, XFontStyle.Italic), GoldDeep, PageWidth / 2, Margin, XStringFormats.TopCenter);
                        top = Margin + 18;
                        bottom = PageHeight - Margin;
                        column = 0;
                    }

                    y = top;
                }

                var x = Margin + (column * (width + ColumnGap));
                if (line.Length > 0)
                {
                    Text(line, italic ? Font(7.5, XFontStyle.Italic) : font, Ink, x, y, XStringFormats.TopLeft);
                }

                y += line.Length == 0 ? lineHeight / 2 : lineHeight;
            }
        }

        _gfx.DrawLine(new XPen(GoldDeep, 0.5), PageWidth / 2, top, PageWidth / 2, Math.Max(top + 20, column == 1 ? bottom : y));
        _y = column == 1 ? bottom : y;
    }

    private double Paragraph(string text, XFont font, double x, double y, double width, double bottom = double.MaxValue,
        bool centred = false, bool continueOnNewPage = false, double lineHeight = 0)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return y;
        }

        var height = lineHeight > 0 ? lineHeight : font.Size * 1.32;
        foreach (var line in Lines(text, font, width))
        {
            if (y + height > bottom)
            {
                if (!continueOnNewPage)
                {
                    break;
                }

                NewPage(_flowPage);
                y = Margin;
            }

            if (centred)
            {
                Text(line, font, Ink, x + (width / 2), y, XStringFormats.TopCenter);
            }
            else
            {
                Text(line, font, Ink, x, y, XStringFormats.TopLeft);
            }

            y += height;
        }

        return y;
    }

    private double Cuneiform(string signs, double x, double y, double size)
    {
        //The Noto face draws the signs as text; should the PDF font system refuse a sign outside the Basic
        //  Multilingual Plane, the same face is set as vector outlines instead
        try
        {
            var font = new XFont(JournalFonts.Cuneiform, size, XFontStyle.Regular);
            _gfx.DrawString(signs, font, new XSolidBrush(GoldDeep), x, y, XStringFormats.TopLeft);
            _drawn.Add(signs);
        }
        catch (Exception)
        {
            using var vector = new VectorText(FontAssets.NotoSansCuneiform);
            var data = vector.PathData(signs, (float)size, (float)x, (float)(y + size));
            if (!string.IsNullOrEmpty(data))
            {
                _gfx.DrawPath(new XSolidBrush(GoldDeep), SvgPathToPdf.Build(data, 1, 1, 0, 0));
                _drawn.Add(signs);
            }
        }

        return y + (size * 1.5);
    }

    private void Art(string artKey, XRect box)
    {
        if (!ArtCatalog.Contains(artKey))
        {
            return;
        }

        var bounds = SvgRaster.ContentBounds(artKey);
        XRect? source = bounds == null ? null : new XRect(bounds.Value.Left, bounds.Value.Top, bounds.Value.Width, bounds.Value.Height);
        SvgToPdf.Draw(_gfx, ArtCatalog.ReadSvg(artKey), box, source);
    }

    private void Rule(double y, double weight) =>
        _gfx.DrawLine(new XPen(Ink, weight), Margin, y, PageWidth - Margin, y);

    private void Text(string text, XFont font, XColor colour, double x, double y, XStringFormat format)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        _gfx.DrawString(text, font, new XSolidBrush(colour), x, y, format);
        _drawn.Add(text);
    }

    private IReadOnlyList<string> Lines(string text, XFont font, double width)
    {
        var lines = new List<string>();
        var line = string.Empty;
        foreach (var word in (text ?? string.Empty).Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
        {
            var candidate = line.Length == 0 ? word : line + " " + word;
            if (line.Length > 0 && Measure(candidate, font) > width)
            {
                lines.Add(line);
                line = word;
            }
            else
            {
                line = candidate;
            }
        }

        if (line.Length > 0)
        {
            lines.Add(line);
        }

        return lines;
    }

    private double Measure(string text, XFont font) => _gfx.MeasureString(text, font).Width;

    private static XFont Font(double size, XFontStyle style) => new XFont(JournalFonts.Serif, size, style);

    private static string N(int value) => value.ToString(CultureInfo.InvariantCulture);
}
