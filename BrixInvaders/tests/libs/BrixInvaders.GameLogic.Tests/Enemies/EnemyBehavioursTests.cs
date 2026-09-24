using System.Collections.Generic;
using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class EnemyBehavioursTests
{
    private sealed class Rig
    {
        public Rig(string code, int columns, int sector = 1, Difficulty level = Difficulty.Pilot, int seed = 3)
        {
            Formation = TestSupport.Formation(TestSupport.Layout(code, columns, sector), level);
            Behaviours = new EnemyBehaviours(DifficultyTable.For(level), sector, new GameRandom(seed));
            Behaviours.StartWave(Formation.Enemies);
        }

        public FormationController Formation { get; }

        public EnemyBehaviours Behaviours { get; }

        public PlayerShip Player { get; } = new PlayerShip(3, 0);

        public Projectiles Projectiles { get; } = new Projectiles();

        public GameEvents Events { get; } = new GameEvents();

        public System.Func<int> Ids { get; } = TestSupport.Ids();

        public void Step()
        {
            Events.Clear();
            Behaviours.Update(TestSupport.Dt, Formation, Player, Projectiles, Ids, Events);
        }
    }

    [Fact]
    public void DivePoint_starts_at_the_slot_and_ends_beside_the_target()
    {
        //Act
        var start = EnemyBehaviours.DivePoint(0, 400, 200, 640, 1);
        var end = EnemyBehaviours.DivePoint(1, 400, 200, 640, 1);

        //Assert
        start.X.Should().BeApproximately(400, 1e-9);
        start.Y.Should().BeApproximately(200, 1e-9);
        end.X.Should().BeApproximately(800, 1e-9);
        end.Y.Should().BeApproximately(560, 1e-9);
    }

    [Fact]
    public void DivePoint_never_leaves_the_playfield()
    {
        //Arrange
        var points = new List<(double X, double Y)>();

        //Act
        foreach (var side in new[] { -1, 1 })
        {
            foreach (var target in new[] { PlayerShip.MinX, 640.0, PlayerShip.MaxX })
            {
                foreach (var startX in new[] { 60.0, 640.0, 1220.0 })
                {
                    for (var t = 0.0; t <= 1.0; t += 0.01)
                    {
                        points.Add(EnemyBehaviours.DivePoint(t, startX, 150, target, side));
                    }
                }
            }
        }

        //Assert
        points.Should().OnlyContain(p => p.X - (Enemy.Width / 2) >= Playfield.SideMargin
            && p.X + (Enemy.Width / 2) <= Playfield.Width - Playfield.SideMargin
            && p.Y <= Playfield.Height - Playfield.SideMargin);
    }

    [Fact]
    public void DivePoint_swoops_low_enough_to_threaten_the_player()
    {
        //Act
        var lowest = Enumerable.Range(0, 101).Select(i => EnemyBehaviours.DivePoint(i / 100.0, 640, 150, 640, 1).Y).Max();

        //Assert
        (lowest + (Enemy.Height / 2)).Should().BeGreaterThan(PlayerShip.LineY - (PlayerShip.Height / 2));
    }

    [Fact]
    public void shooters_fire_aimed_bolts_exactly_once_per_shooter_interval()
    {
        //Arrange
        var rig = new Rig("S", 1);
        rig.Player.X = PlayerShip.MinX;
        var fireTimes = new List<double>();

        //Act
        for (var step = 0; step < 60 * 20; step++)
        {
            rig.Step();
            if (rig.Projectiles.EnemyBolts.Any(b => b.Vx != 0))
            {
                fireTimes.Add(step * TestSupport.Dt);
            }

            rig.Projectiles.Clear();
        }

        //Assert
        fireTimes.Count.Should().BeGreaterThan(4);
        fireTimes.Zip(fireTimes.Skip(1), (a, b) => b - a)
            .Should().OnlyContain(gap => System.Math.Abs(gap - rig.Behaviours.ShooterInterval) <= TestSupport.Dt + 1e-9);
    }

    [Fact]
    public void shooter_bolts_lean_toward_the_player_with_a_capped_speed()
    {
        //Arrange
        var rig = new Rig("S", 1);
        rig.Player.X = PlayerShip.MaxX;

        //Act
        for (var step = 0; step < 60 * 5 && !rig.Projectiles.EnemyBolts.Any(b => b.Vx != 0); step++)
        {
            rig.Step();
        }

        //Assert
        rig.Projectiles.EnemyBolts.First(b => b.Vx != 0).Vx.Should().Be(EnemyBehaviours.MaxAimedBoltVx);
    }

    [Fact]
    public void column_fire_comes_only_from_the_lowest_enemy_of_a_column()
    {
        //Arrange
        var rig = new Rig("GGG", 4);
        foreach (var enemy in rig.Formation.Enemies.Where(e => e.Column == 1 && e.Row == 2))
        {
            enemy.Health = 0;
        }

        var shots = new List<(double X, double Y)>();
        var bottom = rig.Formation.Enemies.First(e => e.Row == 2).Y + (Enemy.Height / 2) + (Projectile.EnemyBoltHeight / 2);
        var middle = rig.Formation.Enemies.First(e => e.Row == 1).Y + (Enemy.Height / 2) + (Projectile.EnemyBoltHeight / 2);

        //Act
        for (var step = 0; step < 60 * 60; step++)
        {
            rig.Step();
            shots.AddRange(rig.Projectiles.EnemyBolts.Select(b => (b.X, b.Y)));
            rig.Projectiles.Clear();
        }

        //Assert
        var column1X = rig.Formation.HomeX(rig.Formation.Enemies.First(e => e.Column == 1));
        shots.Should().NotBeEmpty();
        shots.Where(s => s.X == column1X).Should().OnlyContain(s => s.Y == middle);
        shots.Where(s => s.X != column1X).Should().OnlyContain(s => s.Y == bottom);
        shots.Select(s => s.X).Distinct().Count().Should().Be(4);
    }

    [Fact]
    public void enemy_bolts_never_exceed_the_on_screen_cap()
    {
        //Arrange
        var rig = new Rig("SSSSS", 11, level: Difficulty.Legend);
        var most = 0;

        //Act
        for (var step = 0; step < 60 * 30; step++)
        {
            rig.Step();
            most = System.Math.Max(most, rig.Projectiles.EnemyBolts.Count);
        }

        //Assert
        most.Should().Be(rig.Behaviours.MaxBolts);
    }

    [Fact]
    public void homing_missiles_never_exceed_the_cap()
    {
        //Arrange
        var rig = new Rig("MMM", 11, sector: 4);
        var most = 0;
        var launches = 0;

        //Act
        for (var step = 0; step < 60 * 30; step++)
        {
            rig.Step();
            rig.Projectiles.Update(TestSupport.Dt, rig.Player.X, rig.Player.Y, rig.Events);
            launches += rig.Events.CountOf(GameEventKind.MissileLaunched);
            most = System.Math.Max(most, rig.Projectiles.Missiles.Count);
        }

        //Assert
        launches.Should().BeGreaterThan(rig.Behaviours.MissileCap);
        most.Should().Be(rig.Behaviours.MissileCap);
    }

    [Fact]
    public void divers_leave_swoop_and_return_to_their_slot()
    {
        //Arrange
        var rig = new Rig("D", 3);
        Enemy diver = null;

        //Act
        for (var step = 0; step < 60 * 10 && diver == null; step++)
        {
            rig.Step();
            if (rig.Events.Contains(GameEventKind.DiverLaunched))
            {
                diver = rig.Formation.Enemies.First(e => e.State == EnemyState.Diving);
            }
        }

        var states = new List<EnemyState>();
        for (var step = 0; step < 60 * 10 && (states.Count == 0 || diver.State != EnemyState.InFormation); step++)
        {
            rig.Step();
            states.Add(diver.State);
        }

        //Assert
        diver.Should().NotBeNull();
        states.Should().Contain(EnemyState.Diving);
        states.Should().Contain(EnemyState.Returning);
        diver.State.Should().Be(EnemyState.InFormation);
        diver.X.Should().Be(rig.Formation.HomeX(diver));
        diver.Y.Should().Be(rig.Formation.HomeY(diver));
    }

    [Fact]
    public void a_diver_fires_once_per_dive()
    {
        //Arrange
        var rig = new Rig("D", 1, level: Difficulty.Cadet);
        var diver = rig.Formation.Enemies[0];
        var diveFire = 0;
        var dived = false;

        //Act
        for (var step = 0; step < 60 * 10 && !(dived && diver.State == EnemyState.InFormation); step++)
        {
            rig.Step();
            if (diver.State != EnemyState.InFormation)
            {
                dived = true;
                diveFire += rig.Events.CountOf(GameEventKind.EnemyFired);
            }

            rig.Projectiles.Clear();
        }

        //Assert
        dived.Should().BeTrue();
        diveFire.Should().Be(1);
    }

    [Fact]
    public void no_more_divers_than_the_cap_are_out_at_once()
    {
        //Arrange
        var rig = new Rig("DDD", 11, level: Difficulty.Cadet);
        var most = 0;

        //Act
        for (var step = 0; step < 60 * 60; step++)
        {
            rig.Step();
            most = System.Math.Max(most, rig.Behaviours.DivingCount(rig.Formation));
        }

        //Assert
        most.Should().Be(rig.Behaviours.MaxDivers);
        rig.Behaviours.MaxDivers.Should().Be(1);
    }

    [Fact]
    public void loop_sectors_scale_intervals_speed_and_caps()
    {
        //Arrange
        var first = new EnemyBehaviours(DifficultyTable.For(Difficulty.Pilot), 1, new GameRandom(1));

        //Act
        var looped = new EnemyBehaviours(DifficultyTable.For(Difficulty.Pilot), 6, new GameRandom(1));

        //Assert
        looped.ColumnFireInterval.Should().BeApproximately(first.ColumnFireInterval * 0.85, 1e-9);
        looped.BoltSpeed.Should().BeApproximately(first.BoltSpeed * 1.1, 1e-9);
        looped.MaxBolts.Should().Be(first.MaxBolts + 2);
        looped.MaxDivers.Should().Be(first.MaxDivers + 1);
        looped.MissileCap.Should().Be(first.MissileCap + 1);
    }
}
