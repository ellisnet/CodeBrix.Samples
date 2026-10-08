using System;
using System.Collections.Generic;
using GoddessTempleDiscovery.Game.Cards;
using GoddessTempleDiscovery.Game.Rendering;
using GoddessTempleDiscovery.Game.Session;
using GoddessTempleDiscovery.Rules.Cards;

namespace GoddessTempleDiscovery.Game.Bridges;

/// <summary>
/// Builds the <see cref="CardView"/> of every kind of card: its texts, its facts, its newspaper headline, and its
/// pictures rendered to PNG. Rendering takes a moment, so the host calls these on a worker thread.
/// </summary>
public static class CardViews
{
    /// <summary>The long side, in pixels, of the art rendered for the newspaper photograph.</summary>
    public const int PhotoLongSide = 520;

    /// <summary>The view of a discovery: an EXTRA! edition.</summary>
    /// <param name="card">The discovery.</param>
    /// <param name="faces">The composed faces.</param>
    /// <param name="seasonYear">The season it was excavated in (empty when it is not yet excavated).</param>
    /// <param name="teamName">The team that excavated it (null when none has).</param>
    /// <param name="whereNow">"In the hand of ..." or a similar line for the table, or null.</param>
    /// <param name="autoClose">Seconds before the inspector closes itself; 0 to stay.</param>
    /// <param name="seed">The game's seed, which chooses the headline's edition (see <see cref="Editions"/>).</param>
    /// <returns>The view.</returns>
    public static CardView For(DiscoveryCard card, CardFaceLibrary faces, string seasonYear, string teamName, string whereNow, double autoClose, int seed = 0)
    {
        ArgumentNullException.ThrowIfNull(card);
        var headline = Editions.For(seed, card);
        var facts = new List<FactLine>
        {
            new FactLine("Excavated by", card.ExcavatedBy),
            new FactLine("Season found", card.SeasonFound),
            new FactLine("Level", card.Level),
            new FactLine("Date", card.ApproximateDate),
            new FactLine("Dig Number", CardText.N(card.DigNumber)),
            new FactLine("Points", CardText.N(card.Points)),
            new FactLine("Layer", DepthTiers.Name(card.Tier)),
        };
        if (!string.IsNullOrWhiteSpace(teamName))
        {
            facts.Add(new FactLine("In this game", teamName + (string.IsNullOrWhiteSpace(seasonYear) ? string.Empty : ", winter " + seasonYear)));
        }

        if (!string.IsNullOrWhiteSpace(whereNow))
        {
            facts.Add(new FactLine("On the table", whereNow));
        }

        if (card.IsStarred)
        {
            facts.Add(new FactLine("Her star", "One of Holy Inanna's own finds"));
        }

        var svg = Face(faces, CardFaceComposer.FaceKey(card.Id));
        return new CardView
        {
            Id = card.Id,
            Title = card.Title,
            Kind = CardKind.Discovery,
            KindLabel = "Discovery · " + card.Kind,
            Period = CardText.PeriodName(card.Period) + ", " + card.ApproximateDate,
            ArtKey = card.ArtKey,
            Cuneiform = card.Cuneiform ?? string.Empty,
            CuneiformReading = card.CuneiformReading ?? string.Empty,
            CardText = card.CardText ?? string.Empty,
            LongText = card.LongText ?? string.Empty,
            Facts = facts,
            Sources = card.Sources ?? string.Empty,
            Headline = headline.Headline,
            SubHead = headline.SubHead,
            Dateline = CardText.Dateline(string.IsNullOrWhiteSpace(seasonYear) ? Year(card.SeasonFound) : seasonYear),
            Byline = headline.Byline,
            Caption = CardText.ShortTitle(card.Title) + ". " + card.Level + "; " + card.ApproximateDate + ".",
            WhereNow = card.WhereNow ?? string.Empty,
            IsStarred = card.IsStarred,
            SvgFace = svg,
            PngFace = Png(svg),
            ArtPng = SvgRaster.RenderArtPng(card.ArtKey, PhotoLongSide),
            AutoCloseSeconds = autoClose,
        };
    }

    /// <summary>The view of a season: the front page.</summary>
    /// <param name="card">The season.</param>
    /// <param name="faces">The composed faces.</param>
    /// <param name="autoClose">Seconds before the inspector closes itself; 0 to stay.</param>
    /// <param name="seed">The game's seed, which chooses the headline's edition.</param>
    /// <returns>The view.</returns>
    public static CardView For(SeasonCard card, CardFaceLibrary faces, double autoClose, int seed = 0)
    {
        ArgumentNullException.ThrowIfNull(card);
        var headline = Editions.For(seed, card);
        var svg = Face(faces, CardFaceComposer.FaceKey(card));
        return new CardView
        {
            Id = card.Year,
            Title = card.Title,
            Kind = CardKind.Season,
            KindLabel = "Season " + card.Year,
            Period = "Winter " + card.Year,
            ArtKey = card.ArtKey,
            CardText = card.Story ?? string.Empty,
            LongText = card.LongStory ?? string.Empty,
            Facts = new[]
            {
                new FactLine("Director", card.Director),
                new FactLine("Season", card.Year),
                new FactLine("Effect", CardText.EffectLine(card)),
            },
            Sources = card.Sources ?? string.Empty,
            Headline = headline.Headline,
            SubHead = headline.SubHead,
            Dateline = CardText.Dateline(card.Year),
            Byline = headline.Byline,
            Caption = card.Title + ", winter " + card.Year + ".",
            WhereNow = CardText.EffectLine(card),
            SvgFace = svg,
            PngFace = Png(svg),
            ArtPng = SvgRaster.RenderArtPng(card.ArtKey, PhotoLongSide),
            AutoCloseSeconds = autoClose,
        };
    }

    /// <summary>The view of a tablet: a column of the Learned Society.</summary>
    /// <param name="card">The tablet.</param>
    /// <param name="faces">The composed faces.</param>
    /// <param name="seasonYear">The season it was read in, or empty.</param>
    /// <param name="autoClose">Seconds before the inspector closes itself; 0 to stay.</param>
    /// <returns>The view.</returns>
    public static CardView For(TabletCard card, CardFaceLibrary faces, string seasonYear, double autoClose)
    {
        ArgumentNullException.ThrowIfNull(card);
        var svg = Face(faces, CardFaceComposer.FaceKey(card.Id));
        var facts = new List<FactLine>
        {
            new FactLine("Kind", CardText.TabletKindName(card.Kind)),
            new FactLine("Worth", CardText.N(TabletCard.PointValue) + " points at the end; +" + CardText.N(TabletCard.DigBonus) + " on one dig when spent"),
        };
        if (!string.IsNullOrWhiteSpace(card.Pronunciation))
        {
            facts.Add(new FactLine("Say it", card.Pronunciation));
        }

        return new CardView
        {
            Id = card.Id,
            Title = card.Title,
            Kind = CardKind.Tablet,
            KindLabel = "Tablet · " + CardText.TabletKindName(card.Kind),
            ArtKey = card.ArtKey,
            Cuneiform = card.Cuneiform ?? string.Empty,
            CuneiformReading = card.CuneiformReading ?? string.Empty,
            CardText = card.CardText ?? string.Empty,
            LongText = card.LongText ?? string.Empty,
            Facts = facts,
            Quote = card.Quote ?? string.Empty,
            QuoteAttribution = card.QuoteAttribution ?? string.Empty,
            Sources = card.Sources ?? string.Empty,
            Headline = card.Title.ToUpperInvariant(),
            SubHead = CardText.FirstSentence(card.CardText),
            Dateline = CardText.Dateline(seasonYear),
            Byline = "A paper read before the Learned Society",
            Caption = card.Title + ".",
            SvgFace = svg,
            PngFace = Png(svg),
            ArtPng = SvgRaster.RenderArtPng(card.ArtKey, PhotoLongSide),
            AutoCloseSeconds = autoClose,
        };
    }

    /// <summary>The view of a favor: a small boxed notice.</summary>
    /// <param name="card">The favor.</param>
    /// <param name="faces">The composed faces.</param>
    /// <param name="seasonYear">The season it was drawn in.</param>
    /// <param name="teamName">The team it favored.</param>
    /// <param name="autoClose">Seconds before the inspector closes itself; 0 to stay.</param>
    /// <returns>The view.</returns>
    public static CardView For(FavorCard card, CardFaceLibrary faces, string seasonYear, string teamName, double autoClose)
    {
        ArgumentNullException.ThrowIfNull(card);
        var svg = Face(faces, CardFaceComposer.FaceKey(card));
        return new CardView
        {
            Id = card.Id,
            Title = card.Title,
            Kind = CardKind.Favor,
            KindLabel = "Favor of the Goddess",
            ArtKey = card.ArtKey,
            CardText = card.EffectText ?? string.Empty,
            LongText = card.Fact ?? string.Empty,
            Facts = new[] { new FactLine("The gift", card.EffectText), new FactLine("Favored", teamName ?? string.Empty) },
            Sources = card.Sources ?? string.Empty,
            Headline = "THE GODDESS FAVORS " + (teamName ?? "AN EXPEDITION").ToUpperInvariant(),
            SubHead = card.Title,
            Dateline = CardText.Dateline(seasonYear),
            Byline = "A notice to all expeditions",
            Caption = card.Title + ".",
            SvgFace = svg,
            PngFace = Png(svg),
            ArtPng = SvgRaster.RenderArtPng(card.ArtKey, PhotoLongSide),
            AutoCloseSeconds = autoClose,
        };
    }

    /// <summary>The view of a specialist.</summary>
    /// <param name="card">The specialist.</param>
    /// <param name="faces">The composed faces.</param>
    /// <returns>The view.</returns>
    public static CardView For(SpecialistCard card, CardFaceLibrary faces)
    {
        ArgumentNullException.ThrowIfNull(card);
        var svg = Face(faces, CardFaceComposer.FaceKey(card.Role));
        return new CardView
        {
            Id = CardText.RoleKey(card.Role),
            Title = card.Title,
            Kind = CardKind.Specialist,
            KindLabel = "Specialist · " + CardText.RoleName(card.Role),
            ArtKey = card.ArtKey,
            CardText = card.CardText ?? string.Empty,
            LongText = card.HistoricalNote ?? string.Empty,
            Facts = new[] { new FactLine("Cost", CardText.N(card.Cost)), new FactLine("Role", CardText.RoleName(card.Role)) },
            Sources = card.Sources ?? string.Empty,
            Headline = card.Title.ToUpperInvariant() + " JOINS THE DIG",
            SubHead = card.CardText ?? string.Empty,
            Dateline = CardText.Dateline(string.Empty),
            Byline = "From the expedition's staff list",
            Caption = card.Title + ".",
            SvgFace = svg,
            PngFace = Png(svg),
            ArtPng = SvgRaster.RenderArtPng(card.ArtKey, PhotoLongSide),
        };
    }

    /// <summary>The view of a real person of the excavations (no face; the art is a scene).</summary>
    /// <param name="card">The person.</param>
    /// <returns>The view.</returns>
    public static CardView For(PersonCard card)
    {
        ArgumentNullException.ThrowIfNull(card);
        var facts = new List<FactLine> { new FactLine("Dates", card.Dates), new FactLine("Role", card.Role) };
        if (!string.IsNullOrWhiteSpace(card.EthicsNote))
        {
            facts.Add(new FactLine("The record", card.EthicsNote));
        }

        return new CardView
        {
            Id = card.Id,
            Title = card.Name,
            Kind = CardKind.Person,
            KindLabel = "Person of the excavations",
            ArtKey = card.ArtKey,
            CardText = card.CardText ?? string.Empty,
            LongText = card.LongText ?? string.Empty,
            Facts = facts,
            Sources = card.Sources ?? string.Empty,
            Headline = card.Name.ToUpperInvariant(),
            SubHead = card.Role ?? string.Empty,
            Dateline = CardText.Dateline(string.Empty),
            Byline = "From the Warka Herald's archive",
            Caption = card.Name + ".",
            ArtPng = SvgRaster.RenderArtPng(card.ArtKey, PhotoLongSide),
        };
    }

    private static string Year(string seasonFound)
    {
        if (string.IsNullOrWhiteSpace(seasonFound))
        {
            return string.Empty;
        }

        foreach (var word in seasonFound.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (word.Length >= 7 && char.IsDigit(word[0]) && word.Contains('/', StringComparison.Ordinal))
            {
                return word.TrimEnd(',', '.', ';');
            }
        }

        return string.Empty;
    }

    private static string Face(CardFaceLibrary faces, string key) =>
        faces != null && faces.Faces.TryGetValue(key, out var svg) ? svg : string.Empty;

    private static byte[] Png(string svg) => string.IsNullOrEmpty(svg) ? Array.Empty<byte>() : SvgRaster.RenderFacePng(svg);
}
