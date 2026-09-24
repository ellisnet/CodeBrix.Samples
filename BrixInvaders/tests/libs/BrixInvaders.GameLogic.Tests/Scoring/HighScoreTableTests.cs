using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class HighScoreTableTests
{
    private static HighScoreTable FullTable(Difficulty level = Difficulty.Pilot)
    {
        var table = new HighScoreTable();
        for (var i = 10; i >= 1; i--)
        {
            table.Insert(level, "A" + (char)('A' + i), i * 100, 1);
        }

        return table;
    }

    [Fact]
    public void an_empty_table_takes_any_positive_score_at_rank_zero()
    {
        //Arrange
        var table = new HighScoreTable();

        //Act
        var rank = table.RankFor(Difficulty.Ace, 1);

        //Assert
        rank.Should().Be(0);
        table.Qualifies(Difficulty.Ace, 0).Should().BeFalse();
        table.BestScore(Difficulty.Ace).Should().Be(0);
    }

    [Fact]
    public void entries_are_kept_highest_first()
    {
        //Arrange
        var table = new HighScoreTable();

        //Act
        table.Insert(Difficulty.Cadet, "BBB", 200, 1);
        table.Insert(Difficulty.Cadet, "CCC", 300, 2);
        table.Insert(Difficulty.Cadet, "AAA", 100, 1);

        //Assert
        table.EntriesFor(Difficulty.Cadet).Select(e => e.Score).Should().Equal(300, 200, 100);
        table.BestScore(Difficulty.Cadet).Should().Be(300);
    }

    [Theory]
    [InlineData(1050, 0)]
    [InlineData(950, 1)]
    [InlineData(850, 2)]
    [InlineData(750, 3)]
    [InlineData(650, 4)]
    [InlineData(550, 5)]
    [InlineData(450, 6)]
    [InlineData(350, 7)]
    [InlineData(250, 8)]
    [InlineData(150, 9)]
    public void Insert_at_every_rank_drops_the_old_tenth(long score, int expectedRank)
    {
        //Arrange
        var table = FullTable();

        //Act
        var rank = table.Insert(Difficulty.Pilot, "NEW", score, 3);

        //Assert
        var entries = table.EntriesFor(Difficulty.Pilot);
        rank.Should().Be(expectedRank);
        entries.Should().HaveCount(10);
        entries[rank].Name.Should().Be("NEW");
        entries.Select(e => e.Score).Should().BeInDescendingOrder();
        entries.Select(e => e.Score).Should().NotContain(100);
    }

    [Fact]
    public void Insert_rejects_a_score_below_the_tenth()
    {
        //Arrange
        var table = FullTable();

        //Act
        var rank = table.Insert(Difficulty.Pilot, "LOW", 50, 1);

        //Assert
        rank.Should().Be(-1);
        table.EntriesFor(Difficulty.Pilot).Should().HaveCount(10);
    }

    [Fact]
    public void a_tie_goes_below_the_existing_score()
    {
        //Arrange
        var table = FullTable();

        //Act
        var rank = table.Insert(Difficulty.Pilot, "TIE", 500, 1);

        //Assert
        rank.Should().Be(6);
        table.EntriesFor(Difficulty.Pilot)[5].Name.Should().NotBe("TIE");
        table.RankFor(Difficulty.Pilot, 100).Should().Be(-1);
    }

    [Fact]
    public void difficulties_have_separate_tables()
    {
        //Arrange
        var table = FullTable(Difficulty.Legend);

        //Act
        var pilotEntries = table.EntriesFor(Difficulty.Pilot);

        //Assert
        pilotEntries.Should().BeEmpty();
        table.EntriesFor(Difficulty.Legend).Should().HaveCount(10);
    }

    [Fact]
    public void ToLines_and_FromLines_round_trip()
    {
        //Arrange
        var table = FullTable(Difficulty.Ace);
        table.Insert(Difficulty.Cadet, "ZED", 123456789012, 17);

        //Act
        var copy = HighScoreTable.FromLines(table.ToLines());

        //Assert
        copy.ToLines().Should().Equal(table.ToLines());
        copy.EntriesFor(Difficulty.Cadet)[0].Score.Should().Be(123456789012);
        copy.EntriesFor(Difficulty.Cadet)[0].Sector.Should().Be(17);
        table.ToLines()[0].Should().Be("Cadet|ZED|123456789012|17");
    }

    [Fact]
    public void FromLines_skips_malformed_lines_and_normalises_names()
    {
        //Arrange
        var lines = new[] { "garbage", "Pilot|ab|500|2", "Nope|AAA|1|1", "Ace|XYZ|notanumber|1", "", "Legend|x!yq|10|0", "Pilot|CCC|700|x" };

        //Act
        var table = HighScoreTable.FromLines(lines);

        //Assert
        table.EntriesFor(Difficulty.Pilot).Should().ContainSingle(e => e.Name == "ABA" && e.Score == 500);
        table.EntriesFor(Difficulty.Legend).Should().ContainSingle(e => e.Name == "XAY" && e.Sector == 1);
        table.EntriesFor(Difficulty.Ace).Should().BeEmpty();
    }

    [Fact]
    public void FromLines_null_is_an_empty_table() => HighScoreTable.FromLines(null).ToLines().Should().BeEmpty();

    [Fact]
    public void Clear_and_ClearAll_remove_entries()
    {
        //Arrange
        var table = FullTable(Difficulty.Cadet);
        table.Insert(Difficulty.Ace, "ACE", 10, 1);

        //Act
        table.Clear(Difficulty.Cadet);
        var aceAfterClear = table.EntriesFor(Difficulty.Ace).Count;
        table.ClearAll();

        //Assert
        aceAfterClear.Should().Be(1);
        table.ToLines().Should().BeEmpty();
    }
}
