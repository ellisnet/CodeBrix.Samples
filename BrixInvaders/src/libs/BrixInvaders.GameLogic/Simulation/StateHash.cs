using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>
/// A 64-bit FNV-1a hash over the public simulation state (positions rounded to whole world units). Two runs with
/// the same setup and inputs produce the same hash after every step; the golden-seed tests pin it.
/// </summary>
public static class StateHash
{
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    /// <summary>Hashes the simulation's public state.</summary>
    /// <param name="simulation">The simulation.</param>
    /// <returns>The hash.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="simulation"/> is null.</exception>
    public static ulong Compute(GameSimulation simulation)
    {
        if (simulation == null)
        {
            throw new ArgumentNullException(nameof(simulation));
        }

        var hash = OffsetBasis;
        Mix(ref hash, simulation.StepCount);
        Mix(ref hash, simulation.Sector);
        Mix(ref hash, simulation.Wave);
        Mix(ref hash, (int)simulation.Phase);
        Mix(ref hash, simulation.Score);
        Mix(ref hash, simulation.Scoring.ChainCount);

        var player = simulation.Player;
        Mix(ref hash, player.Lives);
        Mix(ref hash, player.Hull);
        Mix(ref hash, player.Bombs);
        Mix(ref hash, player.ShieldStrength);
        Mix(ref hash, player.IsPresent ? 1 : 0);
        Mix(ref hash, player.X);

        foreach (var enemy in simulation.Enemies)
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            Mix(ref hash, enemy.Id);
            Mix(ref hash, enemy.Health);
            Mix(ref hash, (int)enemy.State);
            Mix(ref hash, enemy.X);
            Mix(ref hash, enemy.Y);
        }

        MixProjectiles(ref hash, simulation.Projectiles.PlayerBolts);
        MixProjectiles(ref hash, simulation.Projectiles.EnemyBolts);
        MixProjectiles(ref hash, simulation.Projectiles.Missiles);

        foreach (var meteor in simulation.Meteors.Meteors)
        {
            Mix(ref hash, meteor.Id);
            Mix(ref hash, meteor.Health);
            Mix(ref hash, meteor.X);
            Mix(ref hash, meteor.Y);
        }

        foreach (var drop in simulation.PowerUps.Drops)
        {
            Mix(ref hash, drop.Id);
            Mix(ref hash, (int)drop.Kind);
            Mix(ref hash, drop.Y);
        }

        foreach (var timer in simulation.PowerUps.ActiveTimers())
        {
            Mix(ref hash, (int)timer.Key);
            Mix(ref hash, timer.Value * 1000.0);
        }

        if (simulation.Ufo != null)
        {
            Mix(ref hash, simulation.Ufo.Id);
            Mix(ref hash, simulation.Ufo.X);
        }

        if (simulation.Boss != null)
        {
            var boss = simulation.Boss;
            Mix(ref hash, (int)boss.State);
            Mix(ref hash, boss.Phase);
            Mix(ref hash, boss.X);
            Mix(ref hash, boss.Y);
            foreach (var section in boss.Sections)
            {
                Mix(ref hash, section.Health);
            }
        }

        return hash;
    }

    private static void MixProjectiles(ref ulong hash, IReadOnlyList<Projectile> projectiles)
    {
        foreach (var projectile in projectiles)
        {
            Mix(ref hash, projectile.Id);
            Mix(ref hash, projectile.X);
            Mix(ref hash, projectile.Y);
        }
    }

    private static void Mix(ref ulong hash, double value) => Mix(ref hash, (long)Math.Round(value));

    private static void Mix(ref ulong hash, int value) => Mix(ref hash, (long)value);

    private static void Mix(ref ulong hash, long value)
    {
        var bits = unchecked((ulong)value);
        for (var i = 0; i < 8; i++)
        {
            hash ^= (bits >> (i * 8)) & 0xFF;
            hash = unchecked(hash * Prime);
        }
    }
}
