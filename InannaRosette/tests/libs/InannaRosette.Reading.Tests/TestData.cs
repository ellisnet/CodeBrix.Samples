// TestData.cs - shared builders for the reading fixtures every other file in this project uses.
// Readings are assembled from fixed card ids and fixed station indexes so that the interpreter's
// deterministic text and the serializer's round trips can be asserted exactly.

using System;
using System.Collections.Generic;
using System.Linq;
using InannaRosette.Reading.Data;
using InannaRosette.Reading.Models;
using InannaRosette.Reading.Services;

namespace InannaRosette.Reading.Tests;

internal static class TestData
{
    /// <summary>A fixed timestamp, so file names and serialized documents are reproducible.</summary>
    public static readonly DateTime Created = new(2026, 9, 13, 10, 30, 0, DateTimeKind.Unspecified);

    /// <summary>The nine card ids used for the "full rosette" fixture, station 0 first.</summary>
    public static readonly int[] FullRosetteCardIds = [1, 2, 7, 12, 25, 30, 33, 18, 40];

    /// <summary>The stations that are laid reversed in the "full rosette" fixture.</summary>
    public static readonly int[] FullRosetteReversedStations = [2, 5, 8];

    /// <summary>Builds one placement from a station index, a card id and an orientation.</summary>
    public static PlacedCard Place(int positionIndex, int cardId, bool reversed = false)
    {
        var position = RosetteSpread.At(positionIndex)
            ?? throw new ArgumentOutOfRangeException(nameof(positionIndex));
        var card = DeckData.ById(cardId)
            ?? throw new ArgumentOutOfRangeException(nameof(cardId));
        return new PlacedCard(position, card, reversed);
    }

    /// <summary>An empty reading with a querent and a question but no cards.</summary>
    public static RosetteReading EmptyReading(string querent = "Jeremy", string question = "What should I attend to?") =>
        new() { Created = Created, Querent = querent, Question = question };

    /// <summary>All nine stations filled, three of them reversed. Always the same cards.</summary>
    public static RosetteReading FullReading(
        string querent = "Jeremy",
        string question = "What should I attend to this season?")
    {
        var reading = EmptyReading(querent, question);
        for (var i = 0; i < 9; i++)
        {
            reading.Placements.Add(Place(i, FullRosetteCardIds[i], FullRosetteReversedStations.Contains(i)));
        }
        return reading;
    }

    /// <summary>A second full reading with different cards, for "two rosettes differ" assertions.</summary>
    public static RosetteReading OtherFullReading(
        string querent = "Ninshubur",
        string question = "Who will stand at the gate?")
    {
        var reading = EmptyReading(querent, question);
        int[] ids = [3, 9, 14, 21, 26, 31, 35, 20, 39];
        for (var i = 0; i < 9; i++) reading.Placements.Add(Place(i, ids[i], i % 2 == 1));
        return reading;
    }

    /// <summary>Only some stations filled, so the "partial rosette" paths are exercised.</summary>
    public static RosetteReading PartialReading(
        string querent = "Enheduanna",
        string question = "What is being asked of me?")
    {
        var reading = EmptyReading(querent, question);
        reading.Placements.Add(Place(0, 1));
        reading.Placements.Add(Place(3, 26, reversed: true));
        reading.Placements.Add(Place(7, 33));
        return reading;
    }

    /// <summary>A reading with the Heart deliberately left empty.</summary>
    public static RosetteReading HeartlessReading()
    {
        var reading = EmptyReading("Dumuzi", "Where should I not be sitting?");
        reading.Placements.Add(Place(1, 5));
        reading.Placements.Add(Place(5, 11, reversed: true));
        return reading;
    }

    /// <summary>A full reading whose querent and question are as long as the UI could ever make them.</summary>
    public static RosetteReading ExtremeReading()
    {
        var querent = string.Join(" ", Enumerable.Repeat("Enheduanna-of-Ur", 40));
        var question = string.Join(" ",
            Enumerable.Repeat("What am I being asked to surrender at the seventh gate of the great below", 40)) + "?";
        var reading = new RosetteReading { Created = Created, Querent = querent, Question = question };
        for (var i = 0; i < 9; i++) reading.Placements.Add(Place(i, FullRosetteCardIds[i], i % 3 == 0));
        return reading;
    }

    /// <summary>A reading with one card only, at the Heart.</summary>
    public static RosetteReading SingleCardReading(int cardId = 1, bool reversed = false)
    {
        var reading = EmptyReading("Inanna", "");
        reading.Placements.Add(Place(0, cardId, reversed));
        return reading;
    }

    /// <summary>The interpreter used by every interpretation test.</summary>
    public static IReadingInterpreter Interpreter() => new ReadingInterpreter();

    /// <summary>The serializer used by every serialization test.</summary>
    public static IReadingSerializer Serializer() => new ReadingSerializer();

    /// <summary>The PDF builder used by every report test.</summary>
    public static IPdfReportBuilder Builder() => new PdfReportBuilder();

    /// <summary>Interprets a reading with the default interpreter.</summary>
    public static ReadingInterpretation Interpret(RosetteReading reading) => Interpreter().Interpret(reading);

    /// <summary>Every paragraph of an interpretation, flattened, so text can be searched as a whole.</summary>
    public static string AllText(ReadingInterpretation interpretation) => string.Join("\n", new[]
        {
            interpretation.Title,
            interpretation.Opening,
            interpretation.Counsel,
            interpretation.ClosingInvocation,
        }
        .Concat(interpretation.Insights.SelectMany(i => new[] { i.Title, i.Text }))
        .Concat(interpretation.Positions.SelectMany(p =>
            new[] { p.Heading }.Concat(p.Keywords).Concat(p.Paragraphs))));

    /// <summary>True when the byte sequence contains the ASCII text, anywhere.</summary>
    public static bool ContainsAscii(byte[] bytes, string text)
    {
        var needle = System.Text.Encoding.ASCII.GetBytes(text);
        for (var i = 0; i + needle.Length <= bytes.Length; i++)
        {
            var match = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (bytes[i + j] != needle[j]) { match = false; break; }
            }
            if (match) return true;
        }
        return false;
    }

    /// <summary>How many times the ASCII text occurs in the byte sequence.</summary>
    public static int CountAscii(byte[] bytes, string text)
    {
        var needle = System.Text.Encoding.ASCII.GetBytes(text);
        var found = 0;
        for (var i = 0; i + needle.Length <= bytes.Length; i++)
        {
            var match = true;
            for (var j = 0; j < needle.Length; j++)
            {
                if (bytes[i + j] != needle[j]) { match = false; break; }
            }
            if (match) found++;
        }
        return found;
    }

    /// <summary>The characters no common desktop file system will accept in a file name.</summary>
    public static IReadOnlyList<char> InvalidFileNameCharacters { get; } =
        [.. new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' }
            .Concat(System.IO.Path.GetInvalidFileNameChars())
            .Distinct()];
}
