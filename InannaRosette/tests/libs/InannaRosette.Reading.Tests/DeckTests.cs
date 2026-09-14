// DeckTests.cs - the shuffled stack: what a fresh deck holds, what a seed guarantees, and what
// drawing, returning and resetting do to the stack.

using System;
using System.Collections.Generic;
using System.Linq;
using InannaRosette.Reading.Data;
using InannaRosette.Reading.Models;
using InannaRosette.Reading.Services;
using SilverAssertions;
using Xunit;

namespace InannaRosette.Reading.Tests;

public class DeckTests
{
    private static List<int> IdsOf(IEnumerable<Card> cards) => cards.Select(c => c.Id).OrderBy(i => i).ToList();

    private static readonly List<int> AllIds = Enumerable.Range(1, 40).ToList();

    [Fact]
    public void a_fresh_deck_holds_all_forty_cards()
    {
        //Act
        var deck = new Deck(seed: 1234);

        //Assert
        deck.Remaining.Should().Be(40);
        deck.Cards.Should().HaveCount(40);
    }

    [Fact]
    public void a_fresh_deck_holds_each_card_exactly_once()
    {
        //Act
        var deck = new Deck(seed: 99);

        //Assert
        IdsOf(deck.Cards).Should().Equal(AllIds);
    }

    [Fact]
    public void the_same_seed_always_produces_the_same_order()
    {
        //Act
        var first = new Deck(seed: 2026).Cards.Select(c => c.Id).ToList();
        var second = new Deck(seed: 2026).Cards.Select(c => c.Id).ToList();

        //Assert
        second.Should().Equal(first);
    }

    [Fact]
    public void different_seeds_produce_different_orders()
    {
        //Act
        var first = new Deck(seed: 1).Cards.Select(c => c.Id).ToList();
        var second = new Deck(seed: 2).Cards.Select(c => c.Id).ToList();

        //Assert
        second.Should().NotEqual(first);
    }

    [Fact]
    public void an_unseeded_deck_still_holds_the_whole_pack()
    {
        //Act
        var deck = new Deck();

        //Assert
        IdsOf(deck.Cards).Should().Equal(AllIds);
    }

    [Fact]
    public void shuffling_keeps_the_multiset_of_cards()
    {
        //Arrange
        var deck = new Deck(seed: 7);

        //Act
        deck.Shuffle();

        //Assert
        deck.Remaining.Should().Be(40);
        IdsOf(deck.Cards).Should().Equal(AllIds);
    }

    [Fact]
    public void shuffling_with_a_seed_resets_the_random_source()
    {
        //Arrange - two decks in the same order, then shuffled from the same seed
        var a = new Deck(seed: 11);
        var b = new Deck(seed: 11);

        //Act
        a.Shuffle(seed: 500);
        b.Shuffle(seed: 500);

        //Assert
        b.Cards.Select(c => c.Id).Should().Equal(a.Cards.Select(c => c.Id).ToList());
    }

    [Fact]
    public void shuffling_from_a_different_seed_gives_a_different_order()
    {
        //Arrange
        var a = new Deck(seed: 11);
        var b = new Deck(seed: 11);

        //Act
        a.Shuffle(seed: 500);
        b.Shuffle(seed: 501);

        //Assert
        b.Cards.Select(c => c.Id).Should().NotEqual(a.Cards.Select(c => c.Id).ToList());
    }

    [Fact]
    public void shuffling_without_a_seed_carries_on_from_the_source_the_deck_already_has()
    {
        //Arrange - the same deck, shuffled the same number of times from the same seed
        var a = new Deck(seed: 64);
        var b = new Deck(seed: 64);

        //Act
        a.Shuffle();
        a.Shuffle();
        b.Shuffle();
        b.Shuffle();

        //Assert
        b.Cards.Select(c => c.Id).Should().Equal(a.Cards.Select(c => c.Id).ToList());
    }

    [Fact]
    public void shuffling_only_reorders_what_is_left_in_the_stack()
    {
        //Arrange
        var deck = new Deck(seed: 5);
        var drawn = Enumerable.Range(0, 10).Select(_ => deck.Draw()).ToList();

        //Act
        deck.Shuffle();

        //Assert
        deck.Remaining.Should().Be(30);
        var left = IdsOf(deck.Cards);
        foreach (var card in drawn) left.Should().NotContain(card.Id);
    }

    [Fact]
    public void Draw_takes_the_top_card_off_the_stack()
    {
        //Arrange
        var deck = new Deck(seed: 42);
        var top = deck.Peek();

        //Act
        var drawn = deck.Draw();

        //Assert
        drawn.Should().BeSameAs(top);
        deck.Remaining.Should().Be(39);
        deck.Cards.Should().NotContain(drawn);
    }

    [Fact]
    public void Draw_never_repeats_a_card()
    {
        //Arrange
        var deck = new Deck(seed: 314);
        var drawn = new List<int>();

        //Act
        while (deck.Draw() is Card card) drawn.Add(card.Id);

        //Assert
        drawn.Should().HaveCount(40);
        drawn.Should().OnlyHaveUniqueItems();
        drawn.OrderBy(i => i).Should().Equal(AllIds);
    }

    [Fact]
    public void Draw_returns_null_once_the_deck_is_empty()
    {
        //Arrange
        var deck = new Deck(seed: 8);
        for (var i = 0; i < 40; i++) deck.Draw();

        //Act
        var beyond = deck.Draw();

        //Assert
        beyond.Should().BeNull();
        deck.Remaining.Should().Be(0);
        deck.Draw().Should().BeNull();
    }

    [Fact]
    public void Peek_looks_at_the_top_card_without_taking_it()
    {
        //Arrange
        var deck = new Deck(seed: 17);

        //Act
        var peeked = deck.Peek();

        //Assert
        peeked.Should().NotBeNull();
        deck.Remaining.Should().Be(40);
        deck.Peek().Should().BeSameAs(peeked);
    }

    [Fact]
    public void Peek_returns_null_on_an_empty_deck()
    {
        //Arrange
        var deck = new Deck(seed: 3);
        while (deck.Draw() is not null) { }

        //Assert
        deck.Peek().Should().BeNull();
    }

    [Fact]
    public void ReturnToBottom_puts_the_card_under_the_stack()
    {
        //Arrange
        var deck = new Deck(seed: 64);
        var drawn = deck.Draw();

        //Act
        deck.ReturnToBottom(drawn);

        //Assert
        deck.Remaining.Should().Be(40);
        deck.Cards[^1].Should().BeSameAs(drawn);
        deck.Peek().Should().NotBeSameAs(drawn);
    }

    [Fact]
    public void ReturnToBottom_rejects_a_missing_card()
    {
        //Arrange
        var deck = new Deck(seed: 1);

        //Act
        Action act = () => deck.ReturnToBottom(null);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void cards_returned_to_the_bottom_are_the_last_ones_the_deck_reaches()
    {
        //Arrange - three cards drawn and then sent back under the stack, as Clear does
        var deck = new Deck(seed: 808);
        var cleared = Enumerable.Range(0, 3).Select(_ => deck.Draw()!).ToList();
        foreach (var card in cleared) deck.ReturnToBottom(card);

        //Act - every card but the three that went under
        var next = Enumerable.Range(0, 37).Select(_ => deck.Draw()!.Id).ToList();

        //Assert
        deck.Remaining.Should().Be(3);
        foreach (var card in cleared) next.Should().NotContain(card.Id);
        IdsOf(deck.Cards).Should().Equal(IdsOf(cleared));
    }

    [Fact]
    public void Remove_takes_a_named_card_out_of_the_stack()
    {
        //Arrange
        var deck = new Deck(seed: 123);
        var card = deck.Cards[17];

        //Act
        var removed = deck.Remove(card);

        //Assert
        removed.Should().BeTrue();
        deck.Remaining.Should().Be(39);
        deck.Cards.Should().NotContain(card);
    }

    [Fact]
    public void Remove_leaves_the_rest_of_the_stack_in_the_order_it_was_in()
    {
        //Arrange
        var deck = new Deck(seed: 321);
        var before = deck.Cards.Select(c => c.Id).ToList();
        var card = deck.Cards[6];

        //Act
        deck.Remove(card);

        //Assert
        before.Remove(card.Id);
        deck.Cards.Select(c => c.Id).Should().Equal(before);
    }

    [Fact]
    public void Remove_matches_a_card_by_its_id()
    {
        //Arrange - the same card as the deck holds, rebuilt rather than looked up
        var deck = new Deck(seed: 44);
        var copy = deck.Cards[3] with { Name = "A different name" };

        //Act
        var removed = deck.Remove(copy);

        //Assert
        removed.Should().BeTrue();
        deck.Remaining.Should().Be(39);
        deck.Cards.Select(c => c.Id).Should().NotContain(copy.Id);
    }

    [Fact]
    public void Remove_reports_false_when_the_stack_does_not_hold_the_card()
    {
        //Arrange
        var deck = new Deck(seed: 45);
        var card = deck.Draw()!;

        //Act
        var removed = deck.Remove(card);

        //Assert
        removed.Should().BeFalse();
        deck.Remaining.Should().Be(39);
    }

    [Fact]
    public void Remove_takes_only_the_card_it_was_given()
    {
        //Arrange - the nine a saved reading might have laid
        var deck = new Deck(seed: 46);
        var laid = deck.Cards.Take(9).ToList();

        //Act
        foreach (var card in laid) deck.Remove(card).Should().BeTrue();

        //Assert
        deck.Remaining.Should().Be(31);
        foreach (var card in laid) deck.Cards.Should().NotContain(card);
    }

    [Fact]
    public void Remove_rejects_a_missing_card()
    {
        //Arrange
        var deck = new Deck(seed: 47);

        //Act
        Action act = () => deck.Remove(null);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Reset_restores_all_forty_cards()
    {
        //Arrange
        var deck = new Deck(seed: 55);
        for (var i = 0; i < 25; i++) deck.Draw();

        //Act
        deck.Reset();

        //Assert
        deck.Remaining.Should().Be(40);
        IdsOf(deck.Cards).Should().Equal(AllIds);
    }

    [Fact]
    public void Reset_on_an_emptied_deck_makes_it_drawable_again()
    {
        //Arrange
        var deck = new Deck(seed: 91);
        while (deck.Draw() is not null) { }

        //Act
        deck.Reset();

        //Assert
        deck.Draw().Should().NotBeNull();
        deck.Remaining.Should().Be(39);
    }

    [Fact]
    public void DrawOriented_hands_back_a_card_and_an_orientation()
    {
        //Arrange
        var deck = new Deck(seed: 2);

        //Act
        var drawn = deck.DrawOriented();

        //Assert
        drawn.Should().NotBeNull();
        drawn.Value.Card.Should().NotBeNull();
        deck.Remaining.Should().Be(39);
    }

    [Fact]
    public void DrawOriented_returns_null_once_the_deck_is_empty()
    {
        //Arrange
        var deck = new Deck(seed: 4);
        while (deck.Draw() is not null) { }

        //Assert
        deck.DrawOriented().Should().BeNull();
    }

    [Fact]
    public void DrawOriented_lays_some_cards_reversed_and_some_upright()
    {
        //Arrange
        var deck = new Deck(seed: 777);
        var orientations = new List<bool>();

        //Act
        while (deck.DrawOriented() is { } drawn) orientations.Add(drawn.IsReversed);

        //Assert - forty draws at one-in-four is overwhelmingly unlikely to be all one way
        orientations.Should().HaveCount(40);
        orientations.Should().Contain(true);
        orientations.Should().Contain(false);
    }

    [Fact]
    public void the_reversed_chance_is_one_card_in_four()
    {
        //Assert
        Deck.ReversedChance.Should().Be(0.25);
    }

    [Fact]
    public void RollReversed_comes_up_roughly_one_time_in_four()
    {
        //Arrange
        var rng = new Random(20260913);

        //Act
        var reversed = Enumerable.Range(0, 20000).Count(_ => Deck.RollReversed(rng));

        //Assert - 25% of 20000 is 5000; a generous band still catches a broken threshold
        reversed.Should().BeInRange(4500, 5500);
    }

    [Fact]
    public void RollReversed_rejects_a_missing_random_source()
    {
        //Act
        Action act = () => Deck.RollReversed(null);

        //Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void the_deck_exposes_the_random_source_it_shuffles_with()
    {
        //Arrange
        var deck = new Deck(seed: 10);

        //Act
        var rng = deck.Rng;
        deck.Shuffle(seed: 10);

        //Assert - a seeded shuffle replaces the source, so the old one is no longer in use
        rng.Should().NotBeNull();
        deck.Rng.Should().NotBeSameAs(rng);
    }

    [Fact]
    public void the_deck_is_built_from_the_same_card_instances_as_the_deck_data()
    {
        //Arrange
        var deck = new Deck(seed: 6);

        //Act & Assert
        foreach (var card in deck.Cards)
        {
            DeckData.ById(card.Id).Should().BeSameAs(card);
        }
    }
}
