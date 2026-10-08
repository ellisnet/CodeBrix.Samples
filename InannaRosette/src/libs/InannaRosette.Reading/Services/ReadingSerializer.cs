using System.Text.Json;
using System.Text.Json.Serialization;
using InannaRosette.Reading.Data;
using InannaRosette.Reading.Models;

namespace InannaRosette.Reading.Services;

/// <summary>
/// Saves and loads a <see cref="RosetteReading"/> as JSON. Only the identifying data is stored
/// (card ids, station indexes, orientation, querent, question, timestamp); the card text
/// and the spread text are rehydrated from <see cref="DeckData"/> and <see cref="RosetteSpread"/>.
/// </summary>
public sealed class ReadingSerializer : IReadingSerializer
{
    /// <summary>
    /// The document-format version this serializer writes and the only one it reads. Version 1
    /// is the first and only format there has ever been, so there is no older format to migrate
    /// from: a document carrying any other version is refused.
    /// </summary>
    public const int CurrentVersion = 1;

    /// <summary>The document actually written to disk. Public so callers may inspect or build one.</summary>
    public sealed record ReadingDocument
    {
        /// <summary>The document-format version; see <see cref="CurrentVersion"/>.</summary>
        public int Version { get; init; } = CurrentVersion;

        /// <summary>When the reading was laid.</summary>
        public DateTime Created { get; init; }

        /// <summary>Who the reading is for; may be empty.</summary>
        public string Querent { get; init; } = "";

        /// <summary>The question put to the rosette; may be empty.</summary>
        public string Question { get; init; } = "";

        /// <summary>The cards on the rosette, ordered by station.</summary>
        public List<PlacementDocument> Placements { get; init; } = [];
    }

    /// <summary>One card at one station, in storage form.</summary>
    public sealed record PlacementDocument
    {
        /// <summary>The station the card sits on: 0 is the Heart, 1-8 the petals.</summary>
        public int Position { get; init; }

        /// <summary>The card's identifier in the deck.</summary>
        public int CardId { get; init; }

        /// <summary>Whether the card was laid reversed.</summary>
        public bool Reversed { get; init; }
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>Writes a reading as indented JSON.</summary>
    public string ToJson(RosetteReading reading)
    {
        ArgumentNullException.ThrowIfNull(reading);
        var doc = new ReadingDocument
        {
            Created = reading.Created,
            Querent = reading.Querent,
            Question = reading.Question,
            Placements = reading.Placements
                .OrderBy(p => p.Position.Index)
                .Select(p => new PlacementDocument
                {
                    Position = p.Position.Index,
                    CardId = p.Card.Id,
                    Reversed = p.IsReversed,
                })
                .ToList(),
        };
        return JsonSerializer.Serialize(doc, Options);
    }

    /// <summary>
    /// Reads a reading back from JSON. The document must carry <see cref="CurrentVersion"/>;
    /// unknown card ids or station indexes are skipped.
    /// </summary>
    /// <exception cref="FormatException">The text is not a reading document, carries no
    /// version, or carries a version other than <see cref="CurrentVersion"/>.</exception>
    public RosetteReading FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        ReadingDocument? doc;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            var root = parsed.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new FormatException("The text is not a valid reading document.");
            }

            CheckVersion(root);
            doc = root.Deserialize<ReadingDocument>(Options);
        }
        catch (JsonException ex)
        {
            throw new FormatException("The text is not a valid reading document.", ex);
        }

        if (doc is null) throw new FormatException("The text is not a valid reading document.");

        var reading = new RosetteReading
        {
            Created = doc.Created == default ? DateTime.Now : doc.Created,
            Querent = doc.Querent,
            Question = doc.Question,
        };

        foreach (var p in doc.Placements.OrderBy(p => p.Position))
        {
            var position = RosetteSpread.At(p.Position);
            var card = DeckData.ById(p.CardId);
            if (position is null || card is null) continue;
            if (reading.At(p.Position) is not null) continue;
            reading.Placements.Add(new PlacedCard(position, card, p.Reversed));
        }

        return reading;
    }

    //Every document this application has ever written carries a version, so one without it was
    //  not saved by InannaRosette. Version 1 is the only format there has been: an older number
    //  names no real format, and a newer one was written by a later InannaRosette whose changes
    //  this one cannot know, so both are refused rather than half-read.
    private static void CheckVersion(JsonElement root)
    {
        if (!root.TryGetProperty("version", out var element))
        {
            throw new FormatException(
                "The text is not a valid reading document: it has no format version.");
        }

        if (element.ValueKind != JsonValueKind.Number || !element.TryGetInt32(out var version))
        {
            throw new FormatException(
                "The text is not a valid reading document: its format version is not a whole number.");
        }

        if (version > CurrentVersion)
        {
            throw new FormatException(
                $"This reading was saved in format version {version} by a newer InannaRosette; "
                + $"this one reads format version {CurrentVersion}.");
        }

        if (version < CurrentVersion)
        {
            throw new FormatException(
                $"The text is not a valid reading document: format version {version} does not exist; "
                + $"the first format version is {CurrentVersion}.");
        }
    }
}
