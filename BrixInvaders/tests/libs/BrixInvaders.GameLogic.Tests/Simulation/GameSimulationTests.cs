using System;
using System.Linq;
using SilverAssertions;
using Xunit;

namespace BrixInvaders.GameLogic.Tests;

public class GameSimulationTests
{
    private static readonly GameInput Fire = new GameInput(0, true, false);
    private static readonly GameInput Bomb = new GameInput(0, false, true);

    [Fact]
    public void a_new_game_starts_with_the_wave_one_intro()
    {
        //Act
        var simulation = TestSupport.Simulation(Difficulty.Cadet, sector: 3);

        //Assert
        simulation.Sector.Should().Be(3);
        simulation.Wave.Should().Be(1);
        simulation.Phase.Should().Be(StagePhase.WaveIntro);
        simulation.PhaseTimeLeft.Should().Be(GameSimulation.WaveIntroDuration);
        simulation.Player.Lives.Should().Be(5);
        simulation.Player.Bombs.Should().Be(2);
        simulation.Enemies.Should().HaveCount(32);
        simulation.Events.Should().Contain(e => e.Kind == GameEventKind.WaveStarted && e.Value == 1);
        simulation.Features.Should().Be(SectorRules.FeaturesOf(3));
    }

    [Fact]
    public void constructor_rejects_null() => ((Action)(() => new GameSimulation(null))).Should().Throw<ArgumentNullException>();

    [Fact]
    public void the_formation_stays_put_during_the_intro_then_starts_stepping()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        var x = simulation.Enemies[0].X;

        //Act
        var introSteps = TestSupport.StepUntil(simulation, s => s.Phase == StagePhase.WaveActive);
        var xAfterIntro = simulation.Enemies[0].X;
        TestSupport.StepUntil(simulation, s => s.Enemies[0].X != x, 120);

        //Assert
        xAfterIntro.Should().Be(x);
        (introSteps * TestSupport.Dt).Should().BeApproximately(GameSimulation.WaveIntroDuration, TestSupport.Dt * 1.5);
    }

    [Fact]
    public void Step_ignores_a_non_positive_step()
    {
        //Arrange
        var simulation = TestSupport.Simulation();

        //Act
        simulation.Step(0, Fire);
        simulation.Step(-1, Fire);

        //Assert
        simulation.StepCount.Should().Be(0);
        simulation.Projectiles.PlayerBolts.Should().BeEmpty();
    }

    [Fact]
    public void holding_fire_shoots_at_the_fire_interval()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        var shots = 0;

        //Act
        for (var i = 0; i < 60; i++)
        {
            simulation.Step(TestSupport.Dt, Fire);
            shots += simulation.Events.CountOf(GameEventKind.PlayerFired);
        }

        //Assert
        shots.Should().Be(4);
    }

    [Fact]
    public void rapid_fire_doubles_the_rate()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        simulation.PowerUps.Apply(PowerUpKind.RapidFire, simulation.Player, simulation.Scoring, simulation.Events);
        var shots = 0;

        //Act
        for (var i = 0; i < 60; i++)
        {
            simulation.Step(TestSupport.Dt, Fire);
            shots += simulation.Events.CountOf(GameEventKind.PlayerFired);
        }

        //Assert
        shots.Should().Be(7);
    }

    [Fact]
    public void spread_shot_fires_a_three_bolt_fan_and_piercing_marks_the_bolts()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        simulation.PowerUps.Apply(PowerUpKind.SpreadShot, simulation.Player, simulation.Scoring, simulation.Events);
        simulation.PowerUps.Apply(PowerUpKind.PiercingLaser, simulation.Player, simulation.Scoring, simulation.Events);

        //Act
        simulation.Step(TestSupport.Dt, Fire);

        //Assert
        var bolts = simulation.Projectiles.PlayerBolts;
        bolts.Should().HaveCount(3);
        bolts.Should().OnlyContain(b => b.IsPiercing && b.Vy < 0);
        bolts.Select(b => Math.Sign(b.Vx)).Should().Equal(-1, 0, 1);
    }

    [Fact]
    public void a_bolt_destroys_an_enemy_scores_it_and_grows_the_chain()
    {
        //Arrange
        var simulation = TestSupport.Simulation(Difficulty.Pilot);
        TestSupport.AdvanceToWaveActive(simulation);
        var target = simulation.Enemies.First(e => e.Row == 3);
        simulation.Projectiles.AddPlayerBolt(simulation.NextId(), target.X, target.Y + 30, 0, -900, false);

        //Act
        simulation.Step(TestSupport.Dt, GameInput.None);

        //Assert
        target.IsAlive.Should().BeFalse();
        var destroyed = simulation.Events.Single(e => e.Kind == GameEventKind.EnemyDestroyed);
        destroyed.Points.Should().Be(10 * 150 / 100);
        destroyed.Role.Should().Be(EnemyRole.Grunt);
        simulation.Scoring.ChainCount.Should().Be(1);
        simulation.Score.Should().Be(15);
    }

    [Fact]
    public void a_piercing_bolt_passes_through_a_whole_column()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        TestSupport.AdvanceToWaveActive(simulation);
        var column = simulation.Enemies.Where(e => e.Column == 2).ToList();
        simulation.Projectiles.AddPlayerBolt(simulation.NextId(), column[0].X, column.Max(e => e.Y) + 30, 0, -900, true);

        //Act
        for (var i = 0; i < 30; i++)
        {
            simulation.Step(TestSupport.Dt, GameInput.None);
        }

        //Assert
        column.Should().OnlyContain(e => !e.IsAlive);
        simulation.Scoring.ChainCount.Should().Be(column.Count);
    }

    [Fact]
    public void a_missed_shot_breaks_the_chain()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        var events = new GameEvents();
        for (var i = 0; i < 5; i++)
        {
            simulation.Scoring.RegisterHit(events);
        }

        simulation.Projectiles.AddPlayerBolt(simulation.NextId(), 20, 30, 0, -900, false);

        //Act
        var broken = TestSupport.StepUntilEvent(simulation, GameEventKind.ChainBroken, GameInput.None, 10);

        //Assert
        broken.Should().BeTrue();
        simulation.Scoring.ChainCount.Should().Be(0);
    }

    [Fact]
    public void a_shielded_enemy_needs_two_hits()
    {
        //Arrange
        var simulation = TestSupport.Simulation(sector: 3);
        TestSupport.AdvanceToWaveActive(simulation);
        var shielded = simulation.Enemies.First(e => e.Role == EnemyRole.Shielded);

        //Act
        simulation.Projectiles.AddPlayerBolt(simulation.NextId(), shielded.X, shielded.Y + 22, 0, -900, false);
        simulation.Step(TestSupport.Dt, GameInput.None);
        var brokenFirst = simulation.Events.Contains(GameEventKind.EnemyShieldBroken);
        var aliveAfterFirst = shielded.IsAlive;
        simulation.Projectiles.AddPlayerBolt(simulation.NextId(), shielded.X, shielded.Y + 22, 0, -900, false);
        simulation.Step(TestSupport.Dt, GameInput.None);

        //Assert
        brokenFirst.Should().BeTrue();
        aliveAfterFirst.Should().BeTrue();
        shielded.IsAlive.Should().BeFalse();
    }

    [Fact]
    public void an_enemy_bolt_damages_the_player()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        simulation.Projectiles.AddEnemyBolt(simulation.NextId(), simulation.Player.X, simulation.Player.Y - 30, 0, 300);

        //Act
        simulation.Step(TestSupport.Dt, GameInput.None);

        //Assert
        simulation.Player.Hull.Should().Be(2);
        simulation.Projectiles.EnemyBolts.Should().BeEmpty();
        simulation.Events.Contains(GameEventKind.PlayerHit).Should().BeTrue();
    }

    [Fact]
    public void an_invulnerable_player_lets_bolts_pass()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        simulation.Player.InvulnerableTime = 1;
        simulation.Projectiles.AddEnemyBolt(simulation.NextId(), simulation.Player.X, simulation.Player.Y - 30, 0, 300);

        //Act
        simulation.Step(TestSupport.Dt, GameInput.None);

        //Assert
        simulation.Player.Hull.Should().Be(PlayerShip.MaxHull);
        simulation.Projectiles.EnemyBolts.Should().HaveCount(1);
    }

    [Fact]
    public void a_bomb_clears_enemy_fire_and_hits_every_enemy_once()
    {
        //Arrange
        var simulation = TestSupport.Simulation(Difficulty.Cadet, sector: 3);
        TestSupport.AdvanceToWaveActive(simulation);
        simulation.Projectiles.AddEnemyBolt(simulation.NextId(), 100, 300, 0, 1);
        simulation.Projectiles.AddMissile(simulation.NextId(), 200, 300, 1);
        var shielded = simulation.Enemies.Where(e => e.Role == EnemyRole.Shielded).ToList();
        var others = simulation.Enemies.Where(e => e.Role != EnemyRole.Shielded).ToList();
        var chain = simulation.Scoring.ChainCount;

        //Act
        simulation.Step(TestSupport.Dt, Bomb);

        //Assert
        simulation.Player.Bombs.Should().Be(1);
        simulation.Projectiles.EnemyBolts.Should().BeEmpty();
        simulation.Projectiles.Missiles.Should().BeEmpty();
        others.Should().OnlyContain(e => !e.IsAlive);
        shielded.Should().OnlyContain(e => e.IsAlive && !e.HasShield);
        simulation.Events.Contains(GameEventKind.BombDetonated).Should().BeTrue();
        simulation.Scoring.ChainCount.Should().Be(chain);
    }

    [Fact]
    public void holding_bomb_uses_only_one()
    {
        //Arrange
        var simulation = TestSupport.Simulation(Difficulty.Cadet);

        //Act
        for (var i = 0; i < 30; i++)
        {
            simulation.Step(TestSupport.Dt, Bomb);
        }

        //Assert
        simulation.Player.Bombs.Should().Be(1);
    }

    [Fact]
    public void no_bombs_means_no_bomb()
    {
        //Arrange
        var simulation = TestSupport.Simulation(Difficulty.Legend);

        //Act
        simulation.Step(TestSupport.Dt, Bomb);

        //Assert
        simulation.Events.Contains(GameEventKind.BombDetonated).Should().BeFalse();
        simulation.Enemies.Should().OnlyContain(e => e.IsAlive);
    }

    [Fact]
    public void a_bomb_damages_the_fighting_boss()
    {
        //Arrange
        var simulation = TestSupport.Simulation(Difficulty.Cadet);
        TestSupport.ClearWavesUntilBoss(simulation);
        TestSupport.StepUntil(simulation, s => s.Boss.State == BossState.Fighting);
        var turret = simulation.Boss.Sections[1];

        //Act
        simulation.Step(TestSupport.Dt, Bomb);

        //Assert
        turret.Health.Should().Be(turret.MaxHealth - Boss.BombDamage);
        simulation.Boss.Sections[0].Health.Should().Be(simulation.Boss.Sections[0].MaxHealth);
    }

    [Fact]
    public void clearing_a_wave_leads_through_the_intermission_to_the_next_wave()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        TestSupport.AdvanceToWaveActive(simulation);
        simulation.ForceWaveCleared();

        //Act
        simulation.Step(TestSupport.Dt, GameInput.None);
        var cleared = simulation.Events.Contains(GameEventKind.WaveCleared);
        var phase = simulation.Phase;
        var waited = TestSupport.StepUntil(simulation, s => s.Wave == 2);

        //Assert
        cleared.Should().BeTrue();
        phase.Should().Be(StagePhase.WaveIntermission);
        (waited * TestSupport.Dt).Should().BeApproximately(GameSimulation.IntermissionDuration, TestSupport.Dt * 1.5);
        simulation.Phase.Should().Be(StagePhase.WaveIntro);
        simulation.Enemies.Should().HaveCount(WaveScript.For(1, 2).EnemyCount);
    }

    [Fact]
    public void sector_three_intermissions_are_meteor_showers()
    {
        //Arrange
        var simulation = TestSupport.Simulation(sector: 3);
        TestSupport.AdvanceToWaveActive(simulation);
        simulation.ForceWaveCleared();

        //Act
        simulation.Step(TestSupport.Dt, GameInput.None);

        //Assert
        simulation.Events.Contains(GameEventKind.MeteorShowerStarted).Should().BeTrue();
        simulation.PhaseTimeLeft.Should().Be(MeteorField.ShowerDuration);
        simulation.Meteors.IsShowerActive.Should().BeTrue();
    }

    [Fact]
    public void after_wave_six_comes_the_boss_warning_then_the_boss()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        var incoming = false;

        //Act
        TestSupport.MakeSafe(simulation);
        TestSupport.StepUntil(simulation, s =>
        {
            incoming |= s.Events.Contains(GameEventKind.BossIncoming);
            if (s.Phase == StagePhase.WaveActive)
            {
                s.ForceWaveCleared();
            }

            return s.Phase == StagePhase.BossFight;
        });

        //Assert
        incoming.Should().BeTrue();
        simulation.Wave.Should().Be(6);
        simulation.Boss.Should().NotBeNull();
        simulation.Events.Contains(GameEventKind.BossAppeared).Should().BeTrue();
        simulation.Formation.Should().BeNull();
        simulation.Enemies.Should().BeEmpty();
    }

    [Fact]
    public void defeating_the_boss_completes_the_sector_with_a_bonus()
    {
        //Arrange
        var simulation = TestSupport.Simulation(Difficulty.Ace);
        TestSupport.ClearWavesUntilBoss(simulation);
        TestSupport.StepUntil(simulation, s => s.Boss.State == BossState.Fighting);
        foreach (var section in simulation.Boss.Sections.Skip(1).Concat(new[] { simulation.Boss.Sections[0] }))
        {
            while (!section.IsDestroyed)
            {
                simulation.Boss.ApplyDamage(section, 1, simulation.Events);
            }
        }

        var score = simulation.Score;

        //Act
        TestSupport.StepUntil(simulation, s => s.Phase == StagePhase.SectorComplete);

        //Assert
        simulation.Boss.Should().BeNull();
        simulation.Events.Contains(GameEventKind.SectorCleared).Should().BeTrue();
        simulation.Score.Should().Be(score + (Scoring.SectorClearBonus(1) * 2));
    }

    [Fact]
    public void shooting_the_boss_core_awards_the_defeat_points()
    {
        //Arrange
        var simulation = TestSupport.Simulation(Difficulty.Cadet);
        TestSupport.ClearWavesUntilBoss(simulation);
        TestSupport.StepUntil(simulation, s => s.Boss.State == BossState.Fighting);
        foreach (var section in simulation.Boss.Sections.Skip(1))
        {
            while (!section.IsDestroyed)
            {
                simulation.Boss.ApplyDamage(section, 1, simulation.Events);
            }
        }

        var core = simulation.Boss.Sections[0];
        core.Health = 1;
        var box = simulation.Boss.BoxOf(core);

        //Act
        simulation.Projectiles.AddPlayerBolt(simulation.NextId(), box.CenterX, box.Bottom + 5, 0, -900, false);
        var defeated = TestSupport.StepUntilEvent(simulation, GameEventKind.BossDefeated, GameInput.None, 5);

        //Assert
        defeated.Should().BeTrue();
        simulation.Events.Single(e => e.Kind == GameEventKind.BossDefeated).Points.Should().Be(Scoring.BossDefeatPoints(1));
    }

    [Fact]
    public void BeginNextSector_moves_on_and_resets_the_waves()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        TestSupport.ClearWavesUntilBoss(simulation);
        foreach (var section in simulation.Boss.Sections)
        {
            section.Health = 0;
        }

        simulation.Boss.Sections[0].Health = 1;
        TestSupport.StepUntil(simulation, s => s.Boss.State == BossState.Fighting);
        simulation.Boss.ApplyDamage(simulation.Boss.Sections[0], 1, simulation.Events);
        TestSupport.StepUntil(simulation, s => s.Phase == StagePhase.SectorComplete);

        //Act
        simulation.BeginNextSector();

        //Assert
        simulation.Sector.Should().Be(2);
        simulation.Wave.Should().Be(1);
        simulation.Phase.Should().Be(StagePhase.WaveIntro);
    }

    [Fact]
    public void BeginNextSector_before_the_sector_is_complete_throws()
    {
        //Arrange
        var simulation = TestSupport.Simulation();

        //Act
        Action act = simulation.BeginNextSector;

        //Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void the_ufo_appears_during_a_wave_and_escapes()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        TestSupport.MakeSafe(simulation);

        //Act
        var appeared = TestSupport.StepUntilEvent(simulation, GameEventKind.UfoAppeared, GameInput.None, 60 * 30);
        var escaped = TestSupport.StepUntilEvent(simulation, GameEventKind.UfoEscaped, GameInput.None, 60 * 10);

        //Assert
        appeared.Should().BeTrue();
        escaped.Should().BeTrue();
        simulation.Ufo.Should().BeNull();
    }

    [Fact]
    public void shooting_the_ufo_scores_and_always_drops_a_power_up()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        TestSupport.MakeSafe(simulation);
        TestSupport.StepUntilEvent(simulation, GameEventKind.UfoAppeared, GameInput.None, 60 * 30);
        TestSupport.StepUntil(simulation, s => s.Ufo.X > 100 && s.Ufo.X < 1180);
        var ufo = simulation.Ufo;

        //Act
        simulation.Projectiles.AddPlayerBolt(simulation.NextId(), ufo.X + (ufo.Direction * 3), ufo.Y + 20, 0, -900, false);
        simulation.Step(TestSupport.Dt, GameInput.None);

        //Assert
        simulation.Ufo.Should().BeNull();
        simulation.Events.Contains(GameEventKind.UfoDestroyed).Should().BeTrue();
        simulation.Events.Contains(GameEventKind.PowerUpDropped).Should().BeTrue();
    }

    [Fact]
    public void the_formation_landing_costs_a_life_and_pushes_the_formation_back_up()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        TestSupport.AdvanceToWaveActive(simulation);
        simulation.Player.InvulnerableTime = 100;
        simulation.Formation.Groups[0].OriginY = 500;

        //Act
        simulation.Step(TestSupport.Dt, GameInput.None);

        //Assert
        simulation.Events.Contains(GameEventKind.FormationLanded).Should().BeTrue();
        simulation.Player.Lives.Should().Be(2);
        simulation.Formation.Groups[0].OriginY.Should().Be(simulation.Formation.StartY);
    }

    [Fact]
    public void losing_the_last_life_ends_the_game_and_freezes_the_simulation()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        simulation.Player.Lives = 1;
        simulation.Player.Hull = 1;
        simulation.Projectiles.AddEnemyBolt(simulation.NextId(), simulation.Player.X, simulation.Player.Y - 30, 0, 300);

        //Act
        simulation.Step(TestSupport.Dt, GameInput.None);
        var gameOverEvent = simulation.Events.Single(e => e.Kind == GameEventKind.GameOver);
        var steps = simulation.StepCount;
        simulation.Step(TestSupport.Dt, Fire);

        //Assert
        simulation.IsGameOver.Should().BeTrue();
        gameOverEvent.Points.Should().Be(simulation.Score);
        simulation.StepCount.Should().Be(steps);
        simulation.Events.Should().BeEmpty();
    }

    [Fact]
    public void power_ups_are_collected_by_flying_into_them()
    {
        //Arrange
        var simulation = TestSupport.Simulation();
        simulation.PowerUps.Spawn(PowerUpKind.SpeedBoost, simulation.Player.X, simulation.Player.Y - 20, simulation.NextId(), simulation.Events);

        //Act
        simulation.Step(TestSupport.Dt, GameInput.None);

        //Assert
        simulation.PowerUps.IsActive(PowerUpKind.SpeedBoost).Should().BeTrue();
        simulation.Events.Contains(GameEventKind.PowerUpCollected).Should().BeTrue();
    }

    [Fact]
    public void a_diving_enemy_that_rams_the_player_is_destroyed_without_points()
    {
        //Arrange
        var simulation = TestSupport.Simulation(sector: 2);
        TestSupport.AdvanceToWaveActive(simulation);
        var diver = simulation.Enemies.First(e => e.Role == EnemyRole.Diver);
        diver.State = EnemyState.Returning;
        diver.X = simulation.Player.X;
        diver.Y = simulation.Player.Y;
        var score = simulation.Score;

        //Act
        simulation.Step(TestSupport.Dt, GameInput.None);

        //Assert
        diver.IsAlive.Should().BeFalse();
        simulation.Player.Hull.Should().Be(2);
        simulation.Score.Should().Be(score);
    }

    [Fact]
    public void the_same_seed_and_inputs_give_the_same_game()
    {
        //Arrange
        var a = TestSupport.Simulation(Difficulty.Ace, seed: 123);
        var b = TestSupport.Simulation(Difficulty.Ace, seed: 123);
        var pilotA = new AttractPilot();
        var pilotB = new AttractPilot();
        var mismatches = 0;

        //Act
        for (var i = 0; i < 3000; i++)
        {
            a.Step(TestSupport.Dt, pilotA.Decide(a));
            b.Step(TestSupport.Dt, pilotB.Decide(b));
            if (a.ComputeStateHash() != b.ComputeStateHash())
            {
                mismatches++;
            }
        }

        //Assert
        mismatches.Should().Be(0);
    }

    [Fact]
    public void different_seeds_give_different_games()
    {
        //Arrange
        var a = TestSupport.Simulation(seed: 1);
        var b = TestSupport.Simulation(seed: 2);
        var pilot = new AttractPilot();

        //Act
        for (var i = 0; i < 1200; i++)
        {
            a.Step(TestSupport.Dt, pilot.Decide(a));
            b.Step(TestSupport.Dt, pilot.Decide(b));
        }

        //Assert
        a.ComputeStateHash().Should().NotBe(b.ComputeStateHash());
    }
}
