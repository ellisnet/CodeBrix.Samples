using System;
using System.Collections.Generic;
using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class FormationControllerTests
{
    public static IEnumerable<object[]> AllWavesAndLevels()
    {
        foreach (var level in DifficultyTable.Levels)
        {
            for (var sector = 1; sector <= 6; sector++)
            {
                for (var wave = 1; wave <= 6; wave++)
                {
                    yield return new object[] { level, sector, wave };
                }
            }
        }
    }

    [Fact]
    public void constructor_centres_a_normal_formation()
    {
        //Act
        var formation = TestSupport.Formation(WaveScript.For(1, 4));

        //Assert
        formation.Groups.Should().HaveCount(1);
        formation.Groups[0].OriginX.Should().Be(640 - (5 * 72));
        formation.Groups[0].OriginY.Should().Be(156);
        formation.Groups[0].Direction.Should().Be(1);
        formation.TotalCount.Should().Be(55);
        formation.AliveCount.Should().Be(55);
        formation.Enemies[0].X.Should().Be(280);
        formation.Enemies[0].Y.Should().Be(156);
        formation.Enemies[54].X.Should().Be(1000);
        formation.Enemies[54].Y.Should().Be(156 + (4 * 56));
    }

    [Fact]
    public void constructor_assigns_roles_and_colours_by_row()
    {
        //Act
        var formation = TestSupport.Formation(WaveScript.For(2, 2));

        //Assert
        formation.Enemies.Where(e => e.Row == 0).Should().OnlyContain(e => e.Role == EnemyRole.Shooter && e.Colour == EnemyColour.Red);
        formation.Enemies.Where(e => e.Row == 1).Should().OnlyContain(e => e.Role == EnemyRole.Diver && e.Colour == EnemyColour.Green);
        formation.Enemies.Where(e => e.Row == 4).Should().OnlyContain(e => e.Role == EnemyRole.Grunt && e.Colour == EnemyColour.Black);
        formation.Enemies.Select(e => e.Id).Distinct().Count().Should().Be(formation.TotalCount);
    }

    [Fact]
    public void constructor_splits_design_five_into_two_mirrored_groups()
    {
        //Act
        var formation = TestSupport.Formation(WaveScript.For(5, 6));

        //Assert
        formation.Groups.Should().HaveCount(2);
        formation.Groups[0].FirstColumn.Should().Be(0);
        formation.Groups[0].LastColumn.Should().Be(5);
        formation.Groups[1].FirstColumn.Should().Be(6);
        formation.Groups[1].LastColumn.Should().Be(10);
        formation.Groups[0].Direction.Should().Be(-1);
        formation.Groups[1].Direction.Should().Be(1);
        formation.Groups[0].MaxX.Should().BeLessThan(formation.Groups[1].MinX);
        formation.Groups[0].OriginX.Should().Be(146);
        formation.Groups[1].OriginX.Should().Be(810);
    }

    [Theory]
    [MemberData(nameof(AllWavesAndLevels))]
    public void formation_never_leaves_its_bounds_and_each_edge_hit_drops_exactly_one_row(Difficulty level, int sector, int wave)
    {
        //Arrange
        var formation = TestSupport.Formation(WaveScript.For(sector, wave), level);
        var events = new GameEvents();
        var drop = DifficultyTable.For(level).DropHeight;
        var violations = new List<string>();

        //Act
        for (var step = 0; step < 600; step++)
        {
            var before = formation.Groups.Select(g => (g.OriginX, g.OriginY, g.Direction)).ToList();
            events.Clear();
            formation.Step(events);
            for (var i = 0; i < formation.Groups.Count; i++)
            {
                var group = formation.Groups[i];
                if (!formation.TryGetExtents(group, out var left, out var right))
                {
                    continue;
                }

                if (left < group.MinX || right > group.MaxX || left < Playfield.SideMargin || right > Playfield.Width - Playfield.SideMargin)
                {
                    violations.Add($"step {step} group {i} out of bounds: {left}..{right}");
                }

                var dy = group.OriginY - before[i].OriginY;
                var dx = group.OriginX - before[i].OriginX;
                var dropped = dy != 0;
                if (dropped && (dy != drop || dx != 0 || group.Direction != -before[i].Direction))
                {
                    violations.Add($"step {step} group {i} bad drop dy={dy} dx={dx}");
                }

                if (!dropped && (Math.Abs(dx) != FormationController.StepX || group.Direction != before[i].Direction))
                {
                    violations.Add($"step {step} group {i} bad sideways step dx={dx}");
                }
            }

            var drops = formation.Groups.Select((g, i) => g.OriginY != before[i].OriginY).Count(d => d);
            if (events.CountOf(GameEventKind.FormationDropped) != drops)
            {
                violations.Add($"step {step} drop events {events.CountOf(GameEventKind.FormationDropped)} != {drops}");
            }
        }

        //Assert
        violations.Should().BeEmpty();
        formation.DropCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void a_drop_is_followed_by_a_sideways_step_in_the_new_direction()
    {
        //Arrange
        var formation = TestSupport.Formation(WaveScript.For(1, 6));
        var events = new GameEvents();
        StepUntilFirstDrop(formation, events);
        var group = formation.Groups[0];
        var x = group.OriginX;

        //Act
        formation.Step(events);

        //Assert
        group.OriginX.Should().Be(x + (group.Direction * FormationController.StepX));
        formation.DropCount.Should().Be(1);
    }

    [Fact]
    public void dead_edge_columns_let_the_formation_travel_further()
    {
        //Arrange
        var full = TestSupport.Formation(WaveScript.For(1, 6));
        var thinned = TestSupport.Formation(WaveScript.For(1, 6));
        foreach (var enemy in thinned.Enemies.Where(e => e.Column == 10))
        {
            enemy.Health = 0;
        }

        //Act
        var fullSteps = StepUntilFirstDrop(full, new GameEvents());
        var thinnedSteps = StepUntilFirstDrop(thinned, new GameEvents());

        //Assert
        thinnedSteps.Should().BeGreaterThan(fullSteps);
    }

    [Fact]
    public void IntervalFor_never_increases_as_enemies_die()
    {
        //Arrange
        var intervals = new List<double>();

        //Act
        for (var alive = 55; alive >= 1; alive--)
        {
            intervals.Add(FormationController.IntervalFor(alive, 55, 0.5));
        }

        //Assert
        intervals[0].Should().Be(0.5);
        intervals[^1].Should().Be(FormationController.MinStepInterval);
        intervals.Zip(intervals.Skip(1), (a, b) => b < a).Should().OnlyContain(strictlyFaster => strictlyFaster);
    }

    [Fact]
    public void CurrentInterval_speeds_up_as_the_formation_thins()
    {
        //Arrange
        var formation = TestSupport.Formation(WaveScript.For(1, 1));
        var readings = new List<double> { formation.CurrentInterval };

        //Act
        foreach (var enemy in formation.Enemies)
        {
            enemy.Health = 0;
            if (formation.AliveCount > 0)
            {
                readings.Add(formation.CurrentInterval);
            }
        }

        //Assert
        readings.Zip(readings.Skip(1), (a, b) => b < a).Should().OnlyContain(faster => faster);
    }

    [Fact]
    public void BaseInterval_is_faster_on_later_waves_and_loops()
    {
        //Act
        var wave1 = TestSupport.Formation(WaveScript.For(1, 1)).BaseInterval;
        var wave6 = TestSupport.Formation(WaveScript.For(1, 6)).BaseInterval;
        var loop = TestSupport.Formation(WaveScript.For(6, 1)).BaseInterval;

        //Assert
        wave1.Should().BeApproximately(0.5, 1e-9);
        wave6.Should().BeApproximately(0.5 * 0.85, 1e-9);
        loop.Should().BeApproximately(0.5 * 0.85, 1e-9);
    }

    [Fact]
    public void Update_takes_one_step_per_elapsed_interval()
    {
        //Arrange
        var formation = TestSupport.Formation(WaveScript.For(1, 1));
        var events = new GameEvents();
        var interval = formation.CurrentInterval;

        //Act
        formation.Update(interval * 3.5, events);

        //Assert
        (formation.StepCount + formation.DropCount).Should().Be(3);
        events.CountOf(GameEventKind.FormationStepped).Should().Be(formation.StepCount);
    }

    [Fact]
    public void FormationStepped_cycles_four_march_notes()
    {
        //Arrange
        var formation = TestSupport.Formation(WaveScript.For(1, 1));
        var events = new GameEvents();
        var notes = new List<int>();

        //Act
        for (var i = 0; i < 6; i++)
        {
            events.Clear();
            formation.Step(events);
            notes.AddRange(TestSupport.Collect(events, GameEventKind.FormationStepped).Select(e => e.Value));
        }

        //Assert
        notes.Should().Equal(0, 1, 2, 3, 0, 1);
    }

    [Fact]
    public void HasLanded_when_the_lowest_row_reaches_the_landing_line_and_ResetHeight_undoes_it()
    {
        //Arrange
        var formation = TestSupport.Formation(WaveScript.For(1, 1));
        formation.Groups[0].OriginY = (int)(FormationController.LandingLine - (3 * 56) - 20);
        formation.PlaceInFormation();

        //Act
        var landed = formation.HasLanded();
        formation.ResetHeight();

        //Assert
        landed.Should().BeTrue();
        formation.HasLanded().Should().BeFalse();
        formation.Groups[0].OriginY.Should().Be(formation.StartY);
        formation.Enemies[0].Y.Should().Be(formation.StartY);
    }

    [Fact]
    public void HomeX_and_HomeY_follow_the_group_origin()
    {
        //Arrange
        var formation = TestSupport.Formation(WaveScript.For(1, 1));
        var enemy = formation.Enemies.First(e => e.Column == 3 && e.Row == 2);

        //Act
        formation.Groups[0].OriginX += 12;
        formation.Groups[0].OriginY += 20;

        //Assert
        formation.HomeX(enemy).Should().Be(formation.Groups[0].OriginX + (3 * 72));
        formation.HomeY(enemy).Should().Be(formation.Groups[0].OriginY + (2 * 56));
    }

    private static int StepUntilFirstDrop(FormationController formation, GameEvents events)
    {
        var steps = 0;
        while (formation.DropCount == 0)
        {
            formation.Step(events);
            steps++;
            if (steps > 10000)
            {
                throw new InvalidOperationException("No drop.");
            }
        }

        return steps;
    }
}
