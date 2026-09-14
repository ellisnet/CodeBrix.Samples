// DeckDataTests.cs - the forty cards themselves: the suit counts, the identifiers, the text every
// card must carry, and the procedural emblem art each card names.

using System;
using System.Collections.Generic;
using System.Linq;
using InannaRosette.Reading.Data;
using InannaRosette.Reading.Models;
using InannaRosette.Reading.Services.Pdf;
using SilverAssertions;
using Xunit;

namespace InannaRosette.Reading.Tests;

public class DeckDataTests
{
    private static IReadOnlyList<Card> Cards => DeckData.Cards;

    [Fact]
    public void the_deck_holds_forty_cards()
    {
        //Act
        var cards = Cards;

        //Assert
        cards.Should().HaveCount(40);
    }

    [Fact]
    public void the_deck_holds_twenty_four_great_goddesses()
    {
        //Assert
        Cards.Count(c => c.Suit == CardSuit.Goddess).Should().Be(24);
    }

    [Fact]
    public void the_deck_holds_eight_gates()
    {
        //Assert
        Cards.Count(c => c.Suit == CardSuit.Gate).Should().Be(8);
    }

    [Fact]
    public void the_deck_holds_eight_sacred_emblems()
    {
        //Assert
        Cards.Count(c => c.Suit == CardSuit.Emblem).Should().Be(8);
    }

    [Fact]
    public void card_ids_run_from_one_to_forty_without_gaps()
    {
        //Act
        var ids = Cards.Select(c => c.Id).ToList();

        //Assert
        ids.Should().Equal(Enumerable.Range(1, 40).ToList());
    }

    [Fact]
    public void card_names_are_unique()
    {
        //Act
        var names = Cards.Select(c => c.Name).ToList();

        //Assert
        names.Should().OnlyHaveUniqueItems();
        names.Should().HaveCount(40);
    }

    [Fact]
    public void every_suit_numbers_its_cards_from_one_upwards()
    {
        //Act
        foreach (var suit in Enum.GetValues<CardSuit>())
        {
            var numbers = Cards.Where(c => c.Suit == suit).Select(c => c.Number).ToList();

            //Assert
            numbers.Should().Equal(Enumerable.Range(1, numbers.Count).ToList());
        }
    }

    [Fact]
    public void the_cards_are_ordered_goddesses_then_gates_then_emblems()
    {
        //Act
        var suits = Cards.Select(c => c.Suit).ToList();

        //Assert
        suits.Take(24).Should().AllBeEquivalentTo(CardSuit.Goddess);
        suits.Skip(24).Take(8).Should().AllBeEquivalentTo(CardSuit.Gate);
        suits.Skip(32).Should().AllBeEquivalentTo(CardSuit.Emblem);
    }

    [Fact]
    public void ById_finds_every_card_in_the_deck()
    {
        //Act & Assert
        foreach (var card in Cards)
        {
            DeckData.ById(card.Id).Should().BeSameAs(card);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(41)]
    [InlineData(int.MaxValue)]
    public void ById_returns_null_outside_the_deck(int id)
    {
        //Assert
        DeckData.ById(id).Should().BeNull();
    }

    [Fact]
    public void every_card_carries_its_lore_and_both_meanings()
    {
        //Act & Assert
        foreach (var card in Cards)
        {
            card.Name.Should().NotBeNullOrWhiteSpace();
            card.Epithet.Should().NotBeNullOrWhiteSpace();
            card.Transliteration.Should().NotBeNullOrWhiteSpace();
            card.Lore.Should().NotBeNullOrWhiteSpace();
            card.UprightMeaning.Should().NotBeNullOrWhiteSpace();
            card.ReversedMeaning.Should().NotBeNullOrWhiteSpace();
            card.Invocation.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void the_upright_and_reversed_meanings_of_a_card_are_never_the_same_text()
    {
        //Act & Assert
        foreach (var card in Cards)
        {
            card.ReversedMeaning.Should().NotBe(card.UprightMeaning);
        }
    }

    [Fact]
    public void every_card_carries_keywords_for_both_orientations_and_at_least_one_domain()
    {
        //Act & Assert
        foreach (var card in Cards)
        {
            card.Domains.Should().NotBeEmpty();
            card.UprightKeywords.Should().NotBeEmpty();
            card.ReversedKeywords.Should().NotBeEmpty();
            card.Domains.Should().AllSatisfy(d => d.Should().NotBeNullOrWhiteSpace());
            card.UprightKeywords.Should().AllSatisfy(k => k.Should().NotBeNullOrWhiteSpace());
            card.ReversedKeywords.Should().AllSatisfy(k => k.Should().NotBeNullOrWhiteSpace());
        }
    }

    [Fact]
    public void every_card_carries_two_hex_colours()
    {
        //Act & Assert
        foreach (var card in Cards)
        {
            card.AccentColor.Should().HaveLength(7);
            card.AccentColor.StartsWith('#').Should().BeTrue();
            card.SecondaryColor.Should().HaveLength(7);
            card.SecondaryColor.StartsWith('#').Should().BeTrue();
            card.AccentColor[1..].All(Uri.IsHexDigit).Should().BeTrue();
            card.SecondaryColor[1..].All(Uri.IsHexDigit).Should().BeTrue();
        }
    }

    [Fact]
    public void BySuit_returns_one_suit_in_number_order()
    {
        //Act
        var gates = DeckData.BySuit(CardSuit.Gate);

        //Assert
        gates.Should().HaveCount(8);
        gates.Select(g => g.Number).Should().Equal(Enumerable.Range(1, 8).ToList());
        gates.Should().AllSatisfy(g => g.Suit.Should().Be(CardSuit.Gate));
    }

    [Theory]
    [InlineData(CardSuit.Goddess, "The Great Goddesses")]
    [InlineData(CardSuit.Gate, "The Eight Gates")]
    [InlineData(CardSuit.Emblem, "The Sacred Emblems")]
    public void SuitName_prints_the_full_name_of_the_suit(CardSuit suit, string expected)
    {
        //Act
        var card = Cards.First(c => c.Suit == suit);

        //Assert
        card.SuitName.Should().Be(expected);
    }

    [Fact]
    public void NumeralLabel_prefixes_the_gates_and_the_emblems_but_not_the_goddesses()
    {
        //Assert
        Cards.First(c => c.Suit == CardSuit.Goddess && c.Number == 4).NumeralLabel.Should().Be("IV");
        Cards.First(c => c.Suit == CardSuit.Gate && c.Number == 7).NumeralLabel.Should().Be("Gate VII");
        Cards.First(c => c.Suit == CardSuit.Emblem && c.Number == 3).NumeralLabel.Should().Be("Emblem III");
    }

    [Theory]
    [InlineData(1, "I")]
    [InlineData(4, "IV")]
    [InlineData(9, "IX")]
    [InlineData(14, "XIV")]
    [InlineData(24, "XXIV")]
    [InlineData(40, "XL")]
    [InlineData(1987, "MCMLXXXVII")]
    [InlineData(3999, "MMMCMXCIX")]
    public void ToRoman_writes_a_roman_numeral(int value, string expected)
    {
        //Assert
        Card.ToRoman(value).Should().Be(expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(4000)]
    public void ToRoman_returns_plain_digits_outside_the_roman_range(int value)
    {
        //Assert
        Card.ToRoman(value).Should().Be(value.ToString());
    }

    // ---------------------------------------------------------------------------------------
    // The emblem art every card names
    // ---------------------------------------------------------------------------------------

    [Fact]
    public void every_emblem_enum_value_has_drawable_layers()
    {
        //Act & Assert
        foreach (var emblem in Enum.GetValues<Emblem>())
        {
            var layers = EmblemArt.Layers(emblem);
            layers.Should().NotBeEmpty();
            layers.Should().AllSatisfy(l => l.PathData.Should().NotBeNullOrWhiteSpace());
        }
    }

    [Fact]
    public void no_emblem_enum_value_falls_through_to_the_default_art()
    {
        //Arrange - an emblem value that does not exist takes the switch's fallback arm
        var fallback = EmblemArt.Layers((Emblem)(-1));

        //Act & Assert
        foreach (var emblem in Enum.GetValues<Emblem>())
        {
            EmblemArt.Layers(emblem).Should().NotEqual(fallback);
        }
    }

    [Fact]
    public void stroked_layers_carry_a_stroke_width_and_filled_layers_do_not_need_one()
    {
        //Act & Assert
        foreach (var emblem in Enum.GetValues<Emblem>())
        {
            foreach (var layer in EmblemArt.Layers(emblem))
            {
                if (!layer.Filled) layer.StrokeWidth.Should().BeGreaterThan(0);
                layer.Opacity.Should().BeInRange(0, 1);
            }
        }
    }

    [Fact]
    public void every_emblem_a_card_names_has_its_own_art()
    {
        //Act
        var used = Cards.Select(c => c.Emblem).Distinct().ToList();

        //Assert
        used.Should().NotBeEmpty();
        used.Should().AllSatisfy(e => EmblemArt.Layers(e).Should().NotBeEmpty());
    }

    [Fact]
    public void the_shared_ornaments_carry_path_data()
    {
        //Assert
        EmblemArt.VenusStar.Should().NotBeEmpty();
        EmblemArt.RosetteMotif.Should().NotBeEmpty();
        EmblemArt.CornerFlourish.Should().NotBeEmpty();
        EmblemArt.VenusStar.Should().AllSatisfy(l => l.PathData.Should().NotBeNullOrWhiteSpace());
        EmblemArt.RosetteMotif.Should().AllSatisfy(l => l.PathData.Should().NotBeNullOrWhiteSpace());
        EmblemArt.CornerFlourish.Should().AllSatisfy(l => l.PathData.Should().NotBeNullOrWhiteSpace());
    }

    [Fact]
    public void no_two_cards_share_an_emblem()
    {
        //Act - every card is meant to carry its own central image
        var emblems = Cards.Select(c => c.Emblem).ToList();

        //Assert
        emblems.Should().OnlyHaveUniqueItems();
        emblems.Should().HaveCount(40);
    }

    [Fact]
    public void every_emblem_a_card_names_parses_through_the_pdf_path_parser()
    {
        //Act & Assert - the report draws the same path data the UI does
        foreach (var card in Cards)
        {
            foreach (var layer in EmblemArt.Layers(card.Emblem))
            {
                SvgPathToPdf.CountSegments(layer.PathData).Should().BeGreaterThan(0);
            }
        }
    }

    [Fact]
    public void the_emblem_enum_has_one_value_for_every_piece_of_art()
    {
        //Arrange - forty motifs, one per card of the deck, plus the four unassigned spares
        var values = Enum.GetValues<Emblem>();

        //Assert
        values.Should().HaveCount(44);
        values.Select(v => (int)v).Should().OnlyHaveUniqueItems();
    }
}
