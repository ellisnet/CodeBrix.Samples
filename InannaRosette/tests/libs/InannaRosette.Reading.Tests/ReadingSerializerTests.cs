// ReadingSerializerTests.cs - saving a reading to JSON and loading it back: the round trip, the
// camelCase document shape written to disk, and what happens to a document that is wrong.

using System;
using System.Linq;
using System.Text.Json;
using InannaRosette.Reading.Data;
using InannaRosette.Reading.Models;
using InannaRosette.Reading.Services;
using SilverAssertions;
using Xunit;

namespace InannaRosette.Reading.Tests;

public class ReadingSerializerTests
{
    private const string HandWritten = """
        {
          "version": 1,
          "created": "2026-09-13T10:30:00",
          "querent": "Enheduanna",
          "question": "What is being asked of me?",
          "placements": [
            { "position": 0, "cardId": 1, "reversed": false },
            { "position": 4, "cardId": 27, "reversed": true },
            { "position": 8, "cardId": 40, "reversed": false }
          ]
        }
        """;

    [Fact]
    public void ToJson_rejects_a_missing_reading()
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        Action act = () => serializer.ToJson(null);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void a_full_reading_survives_the_round_trip()
    {
        //Arrange
        var serializer = TestData.Serializer();
        var original = TestData.FullReading();

        //Act
        var loaded = serializer.FromJson(serializer.ToJson(original));

        //Assert
        loaded.Querent.Should().Be(original.Querent);
        loaded.Question.Should().Be(original.Question);
        loaded.Placements.Should().HaveCount(9);
        loaded.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void the_round_trip_preserves_every_station_card_and_orientation()
    {
        //Arrange
        var serializer = TestData.Serializer();
        var original = TestData.FullReading();

        //Act
        var loaded = serializer.FromJson(serializer.ToJson(original));

        //Assert
        foreach (var placed in original.Placements)
        {
            var back = loaded.At(placed.Position.Index);
            back.Should().NotBeNull();
            back.Card.Id.Should().Be(placed.Card.Id);
            back.IsReversed.Should().Be(placed.IsReversed);
            back.Position.Index.Should().Be(placed.Position.Index);
        }
    }

    [Fact]
    public void the_round_trip_preserves_the_timestamp()
    {
        //Arrange
        var serializer = TestData.Serializer();
        var original = TestData.FullReading();

        //Act
        var loaded = serializer.FromJson(serializer.ToJson(original));

        //Assert
        loaded.Created.Should().Be(original.Created);
    }

    [Fact]
    public void a_partial_reading_survives_the_round_trip()
    {
        //Arrange
        var serializer = TestData.Serializer();
        var original = TestData.PartialReading();

        //Act
        var loaded = serializer.FromJson(serializer.ToJson(original));

        //Assert
        loaded.Placements.Select(p => p.Position.Index).Should().Equal([0, 3, 7]);
        loaded.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void the_document_uses_the_documented_camel_case_keys()
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        var json = serializer.ToJson(TestData.FullReading());
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        //Assert
        root.TryGetProperty("version", out _).Should().BeTrue();
        root.TryGetProperty("created", out _).Should().BeTrue();
        root.TryGetProperty("querent", out _).Should().BeTrue();
        root.TryGetProperty("question", out _).Should().BeTrue();
        root.TryGetProperty("placements", out var placements).Should().BeTrue();

        var first = placements.EnumerateArray().First();
        first.TryGetProperty("position", out _).Should().BeTrue();
        first.TryGetProperty("cardId", out _).Should().BeTrue();
        first.TryGetProperty("reversed", out _).Should().BeTrue();
    }

    [Fact]
    public void the_document_version_is_one()
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        using var document = JsonDocument.Parse(serializer.ToJson(TestData.FullReading()));

        //Assert
        document.RootElement.GetProperty("version").GetInt32().Should().Be(1);
    }

    [Fact]
    public void the_document_records_the_querent_and_the_question_verbatim()
    {
        //Arrange
        var serializer = TestData.Serializer();
        var reading = TestData.FullReading("Enheduanna", "Should I finish the hymn?");

        //Act
        using var document = JsonDocument.Parse(serializer.ToJson(reading));

        //Assert
        document.RootElement.GetProperty("querent").GetString().Should().Be("Enheduanna");
        document.RootElement.GetProperty("question").GetString().Should().Be("Should I finish the hymn?");
    }

    [Fact]
    public void the_placements_are_written_in_station_order()
    {
        //Arrange
        var serializer = TestData.Serializer();
        var reading = TestData.EmptyReading();
        reading.Placements.Add(TestData.Place(7, 12));
        reading.Placements.Add(TestData.Place(0, 1));
        reading.Placements.Add(TestData.Place(3, 30));

        //Act
        using var document = JsonDocument.Parse(serializer.ToJson(reading));
        var positions = document.RootElement.GetProperty("placements")
            .EnumerateArray().Select(p => p.GetProperty("position").GetInt32()).ToList();

        //Assert
        positions.Should().Equal([0, 3, 7]);
    }

    [Fact]
    public void the_placements_carry_only_the_identifying_data()
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        using var document = JsonDocument.Parse(serializer.ToJson(TestData.FullReading()));
        var first = document.RootElement.GetProperty("placements").EnumerateArray().First();

        //Assert - card text is rehydrated from DeckData, never stored
        first.EnumerateObject().Select(p => p.Name).Should().Equal(["position", "cardId", "reversed"]);
    }

    [Fact]
    public void an_empty_reading_writes_an_empty_placements_array()
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        using var document = JsonDocument.Parse(serializer.ToJson(TestData.EmptyReading()));

        //Assert
        document.RootElement.GetProperty("placements").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public void the_json_is_written_indented()
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        var json = serializer.ToJson(TestData.FullReading());

        //Assert
        json.Should().Contain("\n");
        json.Should().Contain("  \"version\"");
    }

    [Fact]
    public void a_hand_written_document_loads()
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        var reading = serializer.FromJson(HandWritten);

        //Assert
        reading.Querent.Should().Be("Enheduanna");
        reading.Question.Should().Be("What is being asked of me?");
        reading.Placements.Should().HaveCount(3);
        reading.At(0).Card.Id.Should().Be(1);
        reading.At(4).Card.Id.Should().Be(27);
        reading.At(4).IsReversed.Should().BeTrue();
        reading.At(8).Card.Id.Should().Be(40);
        reading.Created.Should().Be(new DateTime(2026, 9, 13, 10, 30, 0));
    }

    [Fact]
    public void a_loaded_reading_carries_the_full_card_and_station_text()
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        var reading = serializer.FromJson(HandWritten);
        var heart = reading.At(0);

        //Assert - rehydrated from DeckData and RosetteSpread, not from the document
        heart.Card.Should().BeSameAs(DeckData.ById(1));
        heart.Card.Lore.Should().NotBeNullOrWhiteSpace();
        heart.Position.Should().BeSameAs(RosetteSpread.At(0));
        heart.Position.Description.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void a_document_with_no_timestamp_is_stamped_on_load()
    {
        //Arrange
        var serializer = TestData.Serializer();
        var before = DateTime.Now.AddSeconds(-5);

        //Act
        var reading = serializer.FromJson("""{ "version": 1, "placements": [] }""");

        //Assert
        (reading.Created >= before).Should().BeTrue();
        reading.Created.Should().NotBe(default(DateTime));
    }

    [Fact]
    public void a_document_with_no_placements_loads_as_an_empty_rosette()
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        var reading = serializer.FromJson("""{ "version": 1, "querent": "Jeremy" }""");

        //Assert
        reading.Placements.Should().BeEmpty();
        reading.Querent.Should().Be("Jeremy");
        reading.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void an_unknown_card_id_is_skipped_as_the_serializer_documents()
    {
        //Arrange - the contract is "unknown card ids or station indexes are skipped"
        var serializer = TestData.Serializer();
        var json = """
            { "version": 1, "placements": [
              { "position": 0, "cardId": 1, "reversed": false },
              { "position": 1, "cardId": 999, "reversed": false } ] }
            """;

        //Act
        var reading = serializer.FromJson(json);

        //Assert
        reading.Placements.Should().HaveCount(1);
        reading.At(0).Card.Id.Should().Be(1);
        reading.At(1).Should().BeNull();
    }

    [Fact]
    public void an_unknown_station_index_is_skipped_as_the_serializer_documents()
    {
        //Arrange
        var serializer = TestData.Serializer();
        var json = """
            { "version": 1, "placements": [
              { "position": 0, "cardId": 1, "reversed": false },
              { "position": 99, "cardId": 2, "reversed": false } ] }
            """;

        //Act
        var reading = serializer.FromJson(json);

        //Assert
        reading.Placements.Should().HaveCount(1);
        reading.Placements[0].Position.Index.Should().Be(0);
    }

    [Fact]
    public void a_station_named_twice_keeps_the_first_card()
    {
        //Arrange
        var serializer = TestData.Serializer();
        var json = """
            { "version": 1, "placements": [
              { "position": 2, "cardId": 5, "reversed": false },
              { "position": 2, "cardId": 6, "reversed": true } ] }
            """;

        //Act
        var reading = serializer.FromJson(json);

        //Assert
        reading.Placements.Should().HaveCount(1);
        reading.At(2).Card.Id.Should().Be(5);
        reading.At(2).IsReversed.Should().BeFalse();
    }

    [Theory]
    [InlineData("this is not json")]
    [InlineData("{ \"version\": 1, ")]
    [InlineData("{ \"placements\": \"not an array\" }")]
    [InlineData("[1, 2, 3]")]
    [InlineData("{ \"placements\": [ { \"position\": \"middle\" } ] }")]
    public void malformed_json_is_rejected_with_a_format_exception(string json)
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        Action act = () => serializer.FromJson(json);

        //Assert
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void the_format_exception_says_what_is_wrong()
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        var thrown = Record.Exception(() => serializer.FromJson("this is not json"));

        //Assert
        thrown.Should().BeOfType<FormatException>();
        thrown.Message.Should().Contain("not a valid reading document");
    }

    [Fact]
    public void a_json_null_document_is_rejected()
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        Action act = () => serializer.FromJson("null");

        //Assert
        act.Should().Throw<FormatException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void an_empty_document_is_rejected(string json)
    {
        //Arrange
        var serializer = TestData.Serializer();

        //Act
        Action act = () => serializer.FromJson(json);

        //Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void the_stored_document_can_be_built_by_hand_and_written_back()
    {
        //Arrange - the document type is public so a caller may inspect or build one
        var document = new ReadingSerializer.ReadingDocument
        {
            Created = TestData.Created,
            Querent = "Jeremy",
            Question = "What now?",
            Placements = [new ReadingSerializer.PlacementDocument { Position = 0, CardId = 3, Reversed = true }],
        };

        //Act
        var json = JsonSerializer.Serialize(document, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
        var reading = TestData.Serializer().FromJson(json);

        //Assert
        reading.Querent.Should().Be("Jeremy");
        reading.At(0).Card.Id.Should().Be(3);
        reading.At(0).IsReversed.Should().BeTrue();
    }

    [Fact]
    public void a_reading_with_no_querent_or_question_round_trips_as_empty_strings()
    {
        //Arrange
        var serializer = TestData.Serializer();
        var reading = TestData.FullReading(querent: "", question: "");

        //Act
        var loaded = serializer.FromJson(serializer.ToJson(reading));

        //Assert
        loaded.Querent.Should().Be("");
        loaded.Question.Should().Be("");
    }

    [Fact]
    public void a_round_tripped_reading_interprets_to_the_same_text()
    {
        //Arrange
        var serializer = TestData.Serializer();
        var original = TestData.FullReading();

        //Act
        var loaded = serializer.FromJson(serializer.ToJson(original));

        //Assert
        TestData.AllText(TestData.Interpret(loaded)).Should().Be(TestData.AllText(TestData.Interpret(original)));
    }
}
