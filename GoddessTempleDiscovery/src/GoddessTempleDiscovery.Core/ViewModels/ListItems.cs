using System.Linq;
using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;

namespace GoddessTempleDiscovery.ViewModels;

/// <summary>A labelled line (a fact of the inspector, a cell of a list).</summary>
[Microsoft.UI.Xaml.Data.Bindable]
public class LabelValueItem
{
    /// <summary>Creates the line.</summary>
    /// <param name="label">The label.</param>
    /// <param name="value">The value.</param>
    public LabelValueItem(string label, string value)
    {
        Label = label ?? string.Empty;
        Value = value ?? string.Empty;
    }

    /// <summary>The label.</summary>
    public string Label { get; }

    /// <summary>The value.</summary>
    public string Value { get; }
}

/// <summary>A heading, a text and an optional picture (How to Play, History, Credits).</summary>
[Microsoft.UI.Xaml.Data.Bindable]
public class TextItem : SimpleViewModel
{
    /// <summary>Creates the item.</summary>
    /// <param name="heading">The heading.</param>
    /// <param name="subheading">A line under the heading (may be empty).</param>
    /// <param name="text">The text.</param>
    /// <param name="artKey">The art key of its picture, or empty.</param>
    public TextItem(string heading, string subheading, string text, string artKey)
    {
        Heading = heading ?? string.Empty;
        Subheading = subheading ?? string.Empty;
        Text = text ?? string.Empty;
        ArtKey = artKey ?? string.Empty;
    }

    /// <summary>The heading.</summary>
    public string Heading { get; }

    /// <summary>The line under the heading.</summary>
    public string Subheading { get; }

    /// <summary>The text.</summary>
    public string Text { get; }

    /// <summary>The art key of the picture.</summary>
    public string ArtKey { get; }

    /// <summary>The picture, once loaded.</summary>
    [AffectsProperties(nameof(ImageVisibility))]
    public BitmapImage Image
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>The picture's box shows once it has loaded.</summary>
    public Visibility ImageVisibility => GetVisibility(Image != null);

    /// <summary>The subheading shows when it has text.</summary>
    public Visibility SubheadingVisibility => GetVisibility(Subheading.Length > 0);
}

/// <summary>One line of the final results table.</summary>
[Microsoft.UI.Xaml.Data.Bindable]
public class ScoreRow
{
    /// <summary>The rank, such as "1".</summary>
    public string Rank { get; init; } = string.Empty;

    /// <summary>The team.</summary>
    public string Team { get; init; } = string.Empty;

    /// <summary>Published points.</summary>
    public string Published { get; init; } = string.Empty;

    /// <summary>Half the unpublished finds.</summary>
    public string Unpublished { get; init; } = string.Empty;

    /// <summary>Tablet points.</summary>
    public string Tablets { get; init; } = string.Empty;

    /// <summary>Tablet set bonus.</summary>
    public string Sets { get; init; } = string.Empty;

    /// <summary>Her star bonus.</summary>
    public string Star { get; init; } = string.Empty;

    /// <summary>The total.</summary>
    public string Total { get; init; } = string.Empty;
}

/// <summary>One entry of the Field Journal pane.</summary>
[Microsoft.UI.Xaml.Data.Bindable]
public class JournalItem : TextItem
{
    /// <summary>Creates the item.</summary>
    /// <param name="entry">The engine's entry.</param>
    /// <param name="seed">The game's seed, which chooses the headline edition whose byline the entry shows.</param>
    public JournalItem(Rules.Journal.JournalEntry entry, int seed = 0)
        : base(entry.Title, Line(entry), FirstParagraph(entry.Text), entry.ArtKey)
    {
        Kind = entry.Kind;
        Byline = BylineOf(entry, seed);
    }

    /// <summary>The byline of the edition this game printed for a discovery or a season (empty for other entries).</summary>
    public string Byline { get; }

    /// <summary>The byline shows when there is one.</summary>
    public Microsoft.UI.Xaml.Visibility BylineVisibility => GetVisibility(Byline.Length > 0);

    private static string BylineOf(Rules.Journal.JournalEntry entry, int seed)
    {
        switch (entry.Kind)
        {
            case Rules.Journal.JournalEntryKind.Discovery:
                var card = Rules.Content.Catalog.Discoveries.FirstOrDefault(d => d.Title == entry.Title);
                return card == null ? string.Empty : Game.Cards.Editions.For(seed, card).Byline;
            case Rules.Journal.JournalEntryKind.Season:
                var season = Rules.Content.Catalog.Seasons.FirstOrDefault(s => s.Year == entry.SeasonYear);
                return season == null ? string.Empty : Game.Cards.Editions.For(seed, season).Byline;
            default:
                return string.Empty;
        }
    }

    /// <summary>What the entry records.</summary>
    public Rules.Journal.JournalEntryKind Kind { get; }

    private static string Line(Rules.Journal.JournalEntry entry) =>
        string.Join(" · ", new[] { entry.SeasonYear, entry.Kind.ToString(), entry.TeamName }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

    private static string FirstParagraph(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var end = text.IndexOf("\n\n", System.StringComparison.Ordinal);
        return end < 0 ? text : text.Substring(0, end);
    }
}
