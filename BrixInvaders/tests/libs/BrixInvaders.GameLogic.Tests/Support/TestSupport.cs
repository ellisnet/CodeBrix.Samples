using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic.Tests;

internal static class TestSupport
{
    public const double Dt = Playfield.FixedStep;

    public static Func<int> Ids()
    {
        var id = 0;
        return () => ++id;
    }

    public static WaveLayout Layout(string code, int columns, int sector = 1, int wave = 1, bool split = false, int startY = 120)
    {
        var roles = new List<EnemyRole>();
        foreach (var letter in code)
        {
            roles.Add(WaveScript.RoleOf(letter));
        }

        return new WaveLayout(sector, wave, columns, roles, startY, split);
    }

    public static FormationController Formation(WaveLayout layout, Difficulty difficulty = Difficulty.Pilot)
        => new FormationController(layout, DifficultyTable.For(difficulty), Ids());

    public static GameSimulation Simulation(Difficulty difficulty = Difficulty.Pilot, int sector = 1, int seed = 7)
        => new GameSimulation(new GameSetup(difficulty, sector, seed));

    public static int StepUntil(GameSimulation simulation, Func<GameSimulation, bool> condition, int maxSteps = 200000)
        => StepUntil(simulation, condition, GameInput.None, maxSteps);

    public static int StepUntil(GameSimulation simulation, Func<GameSimulation, bool> condition, GameInput input, int maxSteps = 200000)
    {
        for (var i = 0; i < maxSteps; i++)
        {
            if (condition(simulation))
            {
                return i;
            }

            simulation.Step(Dt, input);
        }

        throw new InvalidOperationException("Condition not reached within " + maxSteps + " steps.");
    }

    public static bool StepUntilEvent(GameSimulation simulation, GameEventKind kind, GameInput input, int maxSteps)
    {
        for (var i = 0; i < maxSteps; i++)
        {
            simulation.Step(Dt, input);
            if (simulation.Events.Contains(kind))
            {
                return true;
            }
        }

        return false;
    }

    public static void MakeSafe(GameSimulation simulation)
    {
        simulation.Player.Lives = 9;
        simulation.Player.InvulnerableTime = 1000;
    }

    public static void AdvanceToWaveActive(GameSimulation simulation)
        => StepUntil(simulation, s => s.Phase == StagePhase.WaveActive);

    public static void ClearWavesUntilBoss(GameSimulation simulation)
    {
        MakeSafe(simulation);
        StepUntil(simulation, s =>
        {
            if (s.Phase == StagePhase.WaveActive || s.Phase == StagePhase.WaveIntro)
            {
                s.ForceWaveCleared();
            }

            return s.Phase == StagePhase.BossFight;
        });
    }

    public static void BringBossToFight(Boss boss, PlayerShip player, Projectiles projectiles, GameEvents events)
    {
        var ids = Ids();
        for (var i = 0; i < 10000 && boss.State == BossState.Entering; i++)
        {
            boss.Update(Dt, player, projectiles, ids, events);
        }
    }

    public static List<GameEvent> Collect(GameEvents events, GameEventKind kind)
    {
        var list = new List<GameEvent>();
        foreach (var item in events)
        {
            if (item.Kind == kind)
            {
                list.Add(item);
            }
        }

        return list;
    }
}
