using System.Collections.Generic;
using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class BossTests
{
    private sealed class Rig
    {
        public Rig(int sector, Difficulty level = Difficulty.Pilot)
        {
            Boss = Boss.Create(sector, DifficultyTable.For(level), Ids);
        }

        public System.Func<int> Ids { get; } = TestSupport.Ids();

        public Boss Boss { get; }

        public PlayerShip Player { get; } = new PlayerShip(3, 0);

        public Projectiles Projectiles { get; } = new Projectiles();

        public GameEvents Events { get; } = new GameEvents();

        public void Step()
        {
            Events.Clear();
            Boss.Update(TestSupport.Dt, Player, Projectiles, Ids, Events);
        }

        public void Fight() => TestSupport.BringBossToFight(Boss, Player, Projectiles, Events);

        public void Destroy(BossSection section)
        {
            while (!section.IsDestroyed)
            {
                Boss.ApplyDamage(section, 1, Events);
            }
        }
    }

    [Theory]
    [InlineData(1, 3, 2)]
    [InlineData(2, 4, 3)]
    [InlineData(3, 4, 3)]
    [InlineData(4, 5, 3)]
    [InlineData(5, 6, 3)]
    public void Create_builds_each_designs_sections_and_phases(int sector, int sections, int phases)
    {
        //Act
        var boss = new Rig(sector).Boss;

        //Assert
        boss.Sections.Should().HaveCount(sections);
        boss.Sections[0].Kind.Should().Be(BossSectionKind.Core);
        boss.PhaseCount.Should().Be(phases);
        boss.Phase.Should().Be(1);
        boss.State.Should().Be(BossState.Entering);
        boss.TotalHealth.Should().Be(boss.Sections.Sum(s => s.MaxHealth));
        boss.HealthFraction.Should().Be(1.0);
    }

    [Fact]
    public void Create_design_five_has_every_section_kind()
        => new Rig(5).Boss.Sections.Select(s => s.Kind).Distinct().Should().BeEquivalentTo(new[]
            { BossSectionKind.Core, BossSectionKind.Turret, BossSectionKind.Cannon, BossSectionKind.MissileBay });

    [Theory]
    [InlineData(Difficulty.Cadet, 1, 45)]
    [InlineData(Difficulty.Pilot, 1, 60)]
    [InlineData(Difficulty.Ace, 1, 78)]
    [InlineData(Difficulty.Legend, 1, 102)]
    [InlineData(Difficulty.Pilot, 6, 90)]
    public void Create_scales_core_health_by_difficulty_and_loop(Difficulty level, int sector, int coreHealth)
        => new Rig(sector, level).Boss.Sections[0].MaxHealth.Should().Be(coreHealth);

    [Theory]
    [InlineData(1.0, 1)]
    [InlineData(0.67, 1)]
    [InlineData(0.66, 2)]
    [InlineData(0.34, 2)]
    [InlineData(0.33, 3)]
    [InlineData(0.0, 3)]
    public void PhaseFor_switches_at_the_thresholds(double fraction, int phase) => Boss.PhaseFor(fraction, new[] { 0.66, 0.33 }).Should().Be(phase);

    [Fact]
    public void the_boss_enters_armoured_then_fights_at_its_fight_height()
    {
        //Arrange
        var rig = new Rig(1);
        var turret = rig.Boss.Sections[1];

        //Act
        var whileEntering = rig.Boss.ApplyDamage(turret, 1, rig.Events);
        rig.Fight();

        //Assert
        whileEntering.Should().Be(BossDamageResult.Armoured);
        turret.Health.Should().Be(turret.MaxHealth);
        rig.Boss.State.Should().Be(BossState.Fighting);
        rig.Boss.Y.Should().Be(Boss.FightY);
    }

    [Fact]
    public void the_core_is_armoured_until_every_other_section_is_destroyed()
    {
        //Arrange
        var rig = new Rig(2);
        rig.Fight();
        var core = rig.Boss.Sections[0];

        //Act
        var early = rig.Boss.ApplyDamage(core, 1, rig.Events);
        foreach (var section in rig.Boss.Sections.Skip(1))
        {
            rig.Destroy(section);
        }

        var late = rig.Boss.ApplyDamage(core, 1, rig.Events);

        //Assert
        early.Should().Be(BossDamageResult.Armoured);
        rig.Boss.IsCoreArmoured.Should().BeFalse();
        late.Should().Be(BossDamageResult.Damaged);
        core.Health.Should().Be(core.MaxHealth - 1);
    }

    [Fact]
    public void destroying_a_section_reports_it_and_further_hits_are_ignored()
    {
        //Arrange
        var rig = new Rig(1);
        rig.Fight();
        var turret = rig.Boss.Sections[1];
        var results = new List<BossDamageResult>();

        //Act
        for (var i = 0; i < turret.MaxHealth + 1; i++)
        {
            results.Add(rig.Boss.ApplyDamage(turret, 1, rig.Events));
        }

        //Assert
        results.Take(turret.MaxHealth - 1).Should().OnlyContain(r => r == BossDamageResult.Damaged);
        results[turret.MaxHealth - 1].Should().Be(BossDamageResult.SectionDestroyed);
        results[^1].Should().Be(BossDamageResult.Ignored);
    }

    [Fact]
    public void phases_advance_at_the_thresholds_and_never_go_back()
    {
        //Arrange
        var rig = new Rig(3);
        rig.Fight();
        var phases = new List<(double Fraction, int Phase)>();

        //Act
        foreach (var section in rig.Boss.Sections.Skip(1).Concat(new[] { rig.Boss.Sections[0] }))
        {
            while (!section.IsDestroyed)
            {
                rig.Events.Clear();
                rig.Boss.ApplyDamage(section, 1, rig.Events);
                phases.Add((rig.Boss.HealthFraction, rig.Boss.Phase));
            }
        }

        //Assert
        phases.Where(p => p.Fraction > 0).Should().OnlyContain(p => p.Phase == Boss.PhaseFor(p.Fraction, rig.Boss.PhaseThresholds));
        phases.Zip(phases.Skip(1), (a, b) => b.Phase >= a.Phase).Should().OnlyContain(ok => ok);
        phases[^1].Phase.Should().Be(3);
    }

    [Fact]
    public void BossPhaseChanged_is_raised_once_per_phase()
    {
        //Arrange
        var rig = new Rig(4);
        rig.Fight();
        var changes = new List<int>();

        //Act
        foreach (var section in rig.Boss.Sections.Skip(1).Concat(new[] { rig.Boss.Sections[0] }))
        {
            while (!section.IsDestroyed)
            {
                rig.Events.Clear();
                rig.Boss.ApplyDamage(section, 1, rig.Events);
                changes.AddRange(TestSupport.Collect(rig.Events, GameEventKind.BossPhaseChanged).Select(e => e.Value));
            }
        }

        //Assert
        changes.Should().Equal(2, 3);
    }

    [Fact]
    public void destroying_the_core_defeats_the_boss_which_is_gone_after_the_dying_time()
    {
        //Arrange
        var rig = new Rig(1);
        rig.Fight();
        foreach (var section in rig.Boss.Sections.Skip(1))
        {
            rig.Destroy(section);
        }

        var core = rig.Boss.Sections[0];
        for (var i = 1; i < core.MaxHealth; i++)
        {
            rig.Boss.ApplyDamage(core, 1, rig.Events);
        }

        //Act
        var result = rig.Boss.ApplyDamage(core, 1, rig.Events);
        var steps = 0;
        while (rig.Boss.State == BossState.Dying)
        {
            rig.Step();
            steps++;
        }

        //Assert
        result.Should().Be(BossDamageResult.Defeated);
        rig.Boss.State.Should().Be(BossState.Gone);
        (steps * TestSupport.Dt).Should().BeApproximately(Boss.DyingDuration, TestSupport.Dt * 1.5);
        rig.Boss.ApplyDamage(core, 1, rig.Events).Should().Be(BossDamageResult.Ignored);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(5)]
    public void the_weaving_boss_stays_inside_the_playfield(int sector)
    {
        //Arrange
        var rig = new Rig(sector);
        rig.Fight();
        var boxes = new List<Box>();

        //Act
        for (var step = 0; step < 60 * 60; step++)
        {
            rig.Step();
            rig.Projectiles.Clear();
            boxes.AddRange(rig.Boss.Sections.Select(rig.Boss.BoxOf));
        }

        //Assert
        boxes.Should().OnlyContain(b => b.Left >= Playfield.SideMargin && b.Right <= Playfield.Width - Playfield.SideMargin
            && b.Top >= Playfield.HudHeight);
    }

    [Fact]
    public void turrets_aim_at_the_player()
    {
        //Arrange
        var rig = new Rig(1);
        rig.Fight();
        rig.Player.X = PlayerShip.MinX;

        //Act
        for (var step = 0; step < 60 * 5 && rig.Projectiles.EnemyBolts.Count == 0; step++)
        {
            rig.Step();
        }

        //Assert
        var bolt = rig.Projectiles.EnemyBolts[0];
        var toPlayerX = rig.Player.X - bolt.X;
        var toPlayerY = rig.Player.Y - bolt.Y;
        (bolt.Vx * toPlayerY - bolt.Vy * toPlayerX).Should().BeApproximately(0, 1e-6 * System.Math.Abs(bolt.Vy * toPlayerX) + 1e-6);
        bolt.Vy.Should().BeGreaterThan(0);
    }

    [Fact]
    public void the_exposed_core_fires_a_three_bolt_fan()
    {
        //Arrange
        var rig = new Rig(1);
        rig.Fight();
        foreach (var section in rig.Boss.Sections.Skip(1))
        {
            rig.Destroy(section);
        }

        //Act
        for (var step = 0; step < 60 * 5 && rig.Projectiles.EnemyBolts.Count == 0; step++)
        {
            rig.Step();
        }

        //Assert
        rig.Projectiles.EnemyBolts.Should().HaveCount(3);
    }

    [Fact]
    public void missile_bays_respect_the_missile_cap()
    {
        //Arrange
        var rig = new Rig(4, Difficulty.Cadet);
        rig.Fight();
        var most = 0;

        //Act
        for (var step = 0; step < 60 * 40; step++)
        {
            rig.Step();
            rig.Projectiles.Update(TestSupport.Dt, rig.Player.X, rig.Player.Y, rig.Events);
            most = System.Math.Max(most, rig.Projectiles.Missiles.Count);
        }

        //Assert
        most.Should().Be(DifficultyTable.For(Difficulty.Cadet).MissileCap);
    }

    [Fact]
    public void BaseHealths_pin_every_design()
    {
        //Act
        var healths = Enumerable.Range(1, 5).Select(Boss.BaseHealths).ToList();

        //Assert
        healths[0].Should().Equal(60, 20, 0, 0);
        healths[1].Should().Equal(80, 25, 35, 0);
        healths[2].Should().Equal(100, 30, 40, 0);
        healths[3].Should().Equal(120, 30, 0, 35);
        healths[4].Should().Equal(150, 35, 45, 40);
    }
}
