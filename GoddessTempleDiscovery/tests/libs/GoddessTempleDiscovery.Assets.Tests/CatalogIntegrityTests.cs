using System;
using System.Collections.Generic;
using System.Linq;
using GoddessTempleDiscovery.Rules.Cards;
using GoddessTempleDiscovery.Rules.Content;
using SilverAssertions;
using Xunit;

namespace GoddessTempleDiscovery.Assets.Tests;

/// <summary>Every card the content pass wrote points at a picture that exists and carries the texts the game shows.</summary>
public class CatalogIntegrityTests
{
    private static IEnumerable<(string Id, string ArtKey, string Where)> EveryArtReference()
    {
        foreach (var card in Catalog.Discoveries) { yield return (card.Id, card.ArtKey, "discovery"); }
        foreach (var card in Catalog.Tablets) { yield return (card.Id, card.ArtKey, "tablet"); }
        foreach (var card in Catalog.Specialists) { yield return (card.Role.ToString(), card.ArtKey, "specialist"); }
        foreach (var card in Catalog.Seasons) { yield return (card.Year, card.ArtKey, "season"); }
        foreach (var card in Catalog.Favors) { yield return (card.Id, card.ArtKey, "favor"); }
        foreach (var card in Catalog.Teams) { yield return (card.Id, card.ArtKey, "team"); }
        foreach (var card in Catalog.People) { yield return (card.Id, card.ArtKey, "person"); }
        foreach (var card in Catalog.Periods) { yield return (card.Period.ToString(), card.ArtKey, "period"); }
    }

    [Fact]
    public void Every_art_key_on_a_card_is_an_embedded_picture()
    {
        var missing = EveryArtReference().Where(r => !ArtCatalog.Contains(r.ArtKey)).Select(r => $"{r.Where} {r.Id}: {r.ArtKey}").ToList();
        missing.Should().BeEmpty(string.Join("\n", missing));
    }

    [Fact]
    public void Decks_have_the_sizes_the_design_asks_for()
    {
        Catalog.Discoveries.Count.Should().BeGreaterThan(59);
        Catalog.Seasons.Count.Should().Be(12);
        Catalog.Favors.Count.Should().Be(16);
        Catalog.Specialists.Count.Should().Be(18);
        Catalog.Teams.Count.Should().Be(8);
        Catalog.Periods.Count.Should().Be(Enum.GetValues<Period>().Length);
        Catalog.Prologue.Count.Should().BeGreaterThan(3);
        Catalog.Epilogue.Count.Should().BeGreaterThan(3);
    }

    [Fact]
    public void Discovery_ids_are_unique_and_every_tier_has_four_cards()
    {
        var ids = Catalog.Discoveries.Select(c => c.Id).ToList();
        ids.Distinct(StringComparer.Ordinal).Count().Should().Be(ids.Count);
        for (var tier = DepthTiers.Shallowest; tier <= DepthTiers.Deepest; tier++)
        {
            Catalog.Discoveries.Count(c => c.Tier == tier).Should().BeGreaterThan(3, $"tier {tier}");
        }

        Catalog.Discoveries.Count(c => c.IsStarred).Should().BeGreaterThan(5);
    }

    [Fact]
    public void Every_discovery_carries_its_texts_and_sources()
    {
        foreach (var card in Catalog.Discoveries)
        {
            card.Title.Should().NotBeNullOrWhiteSpace(card.Id);
            card.CardText.Length.Should().BeGreaterThan(80, card.Id);
            card.LongText.Length.Should().BeGreaterThan(card.CardText.Length, card.Id);
            card.Sources.Should().NotBeNullOrWhiteSpace(card.Id);
            card.ExcavatedBy.Should().NotBeNullOrWhiteSpace(card.Id);
            card.SeasonFound.Should().NotBeNullOrWhiteSpace(card.Id);
            card.WhereNow.Should().NotBeNullOrWhiteSpace(card.Id);
            card.ApproximateDate.Should().NotBeNullOrWhiteSpace(card.Id);
            card.CuneiformReading.Length.Should().Be(card.Cuneiform.Length == 0 ? 0 : card.CuneiformReading.Length, card.Id);
            (card.Cuneiform.Length == 0 || card.CuneiformReading.Length > 0).Should().BeTrue(card.Id);
        }
    }

    [Fact]
    public void Cuneiform_fields_hold_only_cuneiform_signs()
    {
        var fields = Catalog.Discoveries.Select(c => (c.Id, c.Cuneiform))
            .Concat(Catalog.Tablets.Select(c => (c.Id, c.Cuneiform)))
            .Concat(Catalog.Glossary.Select(g => (g.Word, g.Cuneiform)));
        foreach (var (id, cuneiform) in fields)
        {
            foreach (var rune in cuneiform.EnumerateRunes())
            {
                var inBlock = (rune.Value >= 0x12000 && rune.Value <= 0x1254F) || rune.Value == ' ' || rune.Value == 0x1D48 /* modifier d */;
                inBlock.Should().BeTrue($"{id}: U+{rune.Value:X}");
            }
        }
    }

    [Fact]
    public void Seasons_run_in_order_from_1912_to_1939()
    {
        Catalog.Seasons.Select(s => s.Index).Should().BeEquivalentTo(Enumerable.Range(0, 12));
        Catalog.Seasons[0].Year.Should().Be("1912/13");
        Catalog.Seasons[1].Year.Should().Be("1928/29");
        Catalog.Seasons[11].Year.Should().Be("1938/39");
        Catalog.Seasons[11].Effect.Should().Be(SeasonEffect.FinalSeason);
        Catalog.Seasons.Select(s => s.ArtKey).Distinct().Count().Should().Be(12);
        foreach (var season in Catalog.Seasons)
        {
            season.Story.Length.Should().BeGreaterThan(60, season.Year);
            season.Director.Should().NotBeNullOrWhiteSpace(season.Year);
            season.Sources.Should().NotBeNullOrWhiteSpace(season.Year);
        }
    }

    [Fact]
    public void Favors_cycle_every_effect_twice_with_a_fact_each()
    {
        foreach (var effect in Enum.GetValues<FavorEffect>())
        {
            Catalog.Favors.Count(f => f.Effect == effect).Should().Be(2, effect.ToString());
        }

        Catalog.Favors.Select(f => f.Fact).Distinct().Count().Should().Be(16);
    }

    [Fact]
    public void Specialists_cover_every_role_three_times_with_distinct_notes()
    {
        foreach (var role in Enum.GetValues<SpecialistRole>())
        {
            var cards = Catalog.Specialists.Where(s => s.Role == role).ToList();
            cards.Count.Should().Be(3, role.ToString());
            cards.Select(c => c.HistoricalNote).Distinct().Count().Should().Be(3, role.ToString());
            cards.Select(c => c.Cost).Distinct().Count().Should().Be(1, role.ToString());
        }
    }

    [Fact]
    public void People_carry_ethics_notes_only_where_documented()
    {
        var noted = Catalog.People.Where(p => p.EthicsNote.Length > 0).Select(p => p.Id).ToList();
        noted.Should().Contain(id => id.Contains("jordan"));
        noted.Should().Contain(id => id.Contains("falkenstein"));
        noted.Count.Should().BeLessThan(4);
    }

    [Fact]
    public void Headlines_fall_back_to_the_title_in_capitals()
    {
        var pair = Catalog.HeadlineFor("no-such-id", "The Limestone Temple");
        pair.Headline.Should().Be("THE LIMESTONE TEMPLE FOUND AT WARKA");
        Catalog.HeadlineFor(null, "").Headline.Should().Be("A DISCOVERY AT WARKA");
    }

    [Fact]
    public void Quotations_include_jordans_descent_passage()
    {
        Catalog.Quotations.Should().Contain(q => q.Text.Contains("could scarcely have found a more dignified or more beautiful"));
        Catalog.Quotations.Count(q => q.IsPrimary).Should().BeGreaterThan(20);
    }

    [Fact]
    public void Every_discovery_and_season_has_three_distinct_headline_editions_and_the_paper_has_bylines()
    {
        var ids = Catalog.Discoveries.Select(c => (c.Id, c.Title)).Concat(Catalog.Seasons.Select(s => (s.Year, s.Title))).ToList();
        var banners = new List<string>();
        foreach (var (id, title) in ids)
        {
            Catalog.HeadlineVariantCount(id).Should().BeGreaterThan(2, id);
            for (var edition = 0; edition < Catalog.HeadlineVariantCount(id); edition++)
            {
                var pair = Catalog.HeadlineFor(id, title, edition);
                pair.Headline.Should().NotBeNullOrWhiteSpace(id);
                pair.Headline.Should().Be(pair.Headline.ToUpperInvariant(), id);
                pair.SubHead.Should().NotBeNullOrWhiteSpace(id);
                banners.Add(pair.Headline);
            }

            //The edition index wraps, so any non-negative edition is a valid pick
            Catalog.HeadlineFor(id, title, Catalog.HeadlineVariantCount(id)).Should().Be(Catalog.HeadlineFor(id, title, 0));
        }

        banners.Distinct(StringComparer.Ordinal).Count().Should().Be(banners.Count);
        Catalog.Bylines.Count.Should().BeGreaterThan(7);
    }
}
