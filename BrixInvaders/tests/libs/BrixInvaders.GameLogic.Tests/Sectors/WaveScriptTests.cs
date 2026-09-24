using System;
using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class WaveScriptTests
{
    [Fact]
    public void For_sector_one_wave_one_is_eight_by_four_with_shooters_on_top()
    {
        //Act
        var layout = WaveScript.For(1, 1);

        //Assert
        layout.Columns.Should().Be(8);
        layout.Rows.Should().Be(4);
        layout.EnemyCount.Should().Be(32);
        layout.RowRoles.Should().Equal(EnemyRole.Shooter, EnemyRole.Grunt, EnemyRole.Grunt, EnemyRole.Grunt);
        layout.StartY.Should().Be(120);
        layout.IsSplit.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 8)]
    [InlineData(2, 9)]
    [InlineData(3, 10)]
    [InlineData(4, 11)]
    [InlineData(5, 11)]
    [InlineData(6, 11)]
    public void For_column_count_and_start_height_per_wave(int wave, int columns)
    {
        //Act
        var layout = WaveScript.For(2, wave);

        //Assert
        layout.Columns.Should().Be(columns);
        layout.StartY.Should().Be(120 + (12 * (wave - 1)));
        layout.Wave.Should().Be(wave);
        layout.Sector.Should().Be(2);
    }

    [Fact]
    public void every_wave_only_uses_roles_its_design_has_introduced()
    {
        //Arrange
        var allowed = new[] { "GS", "GSD", "GSDH", "GSDHM", "GSDHM" };

        //Act
        var violations = (from design in Enumerable.Range(1, 5)
                          from wave in Enumerable.Range(1, 6)
                          let code = WaveScript.CodeOf(design, wave)
                          where code.Any(c => !allowed[design - 1].Contains(c))
                          select code).ToList();

        //Assert
        violations.Should().BeEmpty();
    }

    [Theory]
    [InlineData(2, 'D')]
    [InlineData(3, 'H')]
    [InlineData(4, 'M')]
    public void every_wave_of_a_design_shows_its_new_role(int design, char letter)
        => Enumerable.Range(1, 6).Select(w => WaveScript.CodeOf(design, w)).Should().OnlyContain(code => code.Contains(letter));

    [Fact]
    public void only_design_five_splits_from_wave_two()
    {
        //Act
        var splits = (from sector in Enumerable.Range(1, 10)
                      from wave in Enumerable.Range(1, 6)
                      where WaveScript.For(sector, wave).IsSplit
                      select (sector, wave)).ToList();

        //Assert
        splits.Should().Equal((5, 2), (5, 3), (5, 4), (5, 5), (5, 6), (10, 2), (10, 3), (10, 4), (10, 5), (10, 6));
    }

    [Theory]
    [InlineData(0, EnemyColour.Red)]
    [InlineData(1, EnemyColour.Green)]
    [InlineData(2, EnemyColour.Blue)]
    [InlineData(3, EnemyColour.Black)]
    [InlineData(4, EnemyColour.Black)]
    public void ColourOfRow_top_rows_are_worth_more(int row, EnemyColour colour) => WaveScript.ColourOfRow(row).Should().Be(colour);

    [Fact]
    public void RoleOf_rejects_an_unknown_letter()
    {
        //Act
        Action act = () => WaveScript.RoleOf('X');

        //Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void For_rejects_wave_seven()
    {
        //Act
        Action act = () => WaveScript.For(1, 7);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void loops_repeat_the_designs() => WaveScript.For(7, 3).RowRoles.Should().Equal(WaveScript.For(2, 3).RowRoles);
}
