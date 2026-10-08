using System;
using System.IO;
using System.Linq;
using System.Text;
using CodeBrix.PdfDocuments.Pdf.IO;
using GoddessTempleDiscovery.Game.Credits;
using GoddessTempleDiscovery.Game.Journal;
using GoddessTempleDiscovery.Rules.Brains;
using GoddessTempleDiscovery.Rules.Engine;
using GoddessTempleDiscovery.Rules.Journal;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Game.Tests.Journal;

public class JournalPdfBuilderTests
{
    private static readonly Lazy<(GameEngine Engine, JournalPdf Pdf)> Short = new(BuildShort);

    //A short seeded game: two computer teams, one turn a season, three seasons
    private static (GameEngine, JournalPdf) BuildShort()
    {
        var engine = new GameEngine(new GameSetup(
            new[]
            {
                new SeatSetup("The Reed Marsh Expedition", null, SeatKind.Computer, Temperament.DeepDigger),
                new SeatSetup("The Lapis Road Society", null, SeatKind.Computer, Temperament.Scholar),
            },
            TurnsPerSeason: 1,
            Seed: 1913));
        var random = new Random(3);
        while (engine.State.SeasonIndex < 3 && !engine.State.IsGameOver)
        {
            engine.Apply(ComputerBrain.Choose(engine, engine.State.CurrentTeam.Temperament, random));
            engine.ClearEvents();
        }

        var pdf = new JournalPdfBuilder().Build(new JournalPdfInput(
            engine.State.Journal, engine.State.Teams.Select(t => t.Name).ToArray(), engine.FinalScores(), new DateTime(2026, 10, 8)));
        return (engine, pdf);
    }

    [Fact]
    public void a_short_game_builds_a_pdf()
    {
        //Arrange
        var bytes = Short.Value.Pdf.Bytes;

        //Assert
        Encoding.ASCII.GetString(bytes, 0, 5).Should().Be("%PDF-");
        Encoding.ASCII.GetString(bytes, bytes.Length - 32, 32).TrimEnd('\r', '\n', ' ').Should().EndWith("%%EOF");
    }

    [Fact]
    public void the_pdf_has_the_expected_pages()
    {
        //Arrange
        var (engine, pdf) = Short.Value;
        var seasons = engine.State.Journal.Count(e => e.Kind == JournalEntryKind.Season);
        var discoveries = engine.State.Journal.Count(e => e.Kind == JournalEntryKind.Discovery);

        //Act
        using var document = PdfReader.Open(new MemoryStream(pdf.Bytes), PdfDocumentOpenMode.Import);

        //Assert - the cover, a page per season and per discovery, the papers, the final edition, the note, the sources
        document.PageCount.Should().Be(pdf.PageCount);
        pdf.PageCount.Should().BeGreaterThanOrEqualTo(1 + seasons + discoveries + 3);
        seasons.Should().Be(4);
    }

    [Fact]
    public void the_pdf_carries_the_heritage_note_verbatim()
    {
        //Arrange
        var text = string.Join(" ", Short.Value.Pdf.DrawnText);

        //Assert
        text.Should().Contain(HeritageNote.Title);
        text.Should().Contain("I wish to acknowledge the deep and ongoing pain felt by the people of Iraq");
        text.Should().Contain("No part of this creative work is intended to celebrate individuals who espoused or were aligned with Nazi ideology");
        string.Join(" ", HeritageNote.Text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
            .Should().Be(string.Join(" ", HeritageNote.FirstParagraph.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)) + " "
                + string.Join(" ", HeritageNote.SecondParagraph.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)));
    }

    [Fact]
    public void the_pdf_names_every_entry_of_the_journal()
    {
        //Arrange
        var (engine, pdf) = Short.Value;
        var drawn = pdf.DrawnText.ToHashSet(StringComparer.Ordinal);

        //Assert
        engine.State.Journal.Should().NotBeEmpty();
        foreach (var entry in engine.State.Journal)
        {
            drawn.Any(line => line.Contains(entry.Title, StringComparison.Ordinal)).Should().BeTrue(entry.Title);
        }
    }

    [Fact]
    public void writes_the_journal_when_asked()
    {
        //Only with GODDESSTEMPLE_DUMP_JOURNAL=<file>: a look at the pages by eye
        var path = Environment.GetEnvironmentVariable("GODDESSTEMPLE_DUMP_JOURNAL");
        if (!string.IsNullOrWhiteSpace(path))
        {
            File.WriteAllBytes(path, Short.Value.Pdf.Bytes);
        }
    }

    [Fact]
    public void the_pdf_embeds_merriweather()
    {
        //Assert
        Encoding.Latin1.GetString(Short.Value.Pdf.Bytes).Should().Contain("Merriweather");
    }
}
