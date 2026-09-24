using System;

namespace BrixInvaders.GameLogic;

/// <summary>
/// The attract-mode AI. Deterministic: it dodges any enemy bolt, missile, meteor or diver predicted to reach the ship within 0.9 s, otherwise
/// lines up under the nearest target (lowest enemy of the nearest column, a boss section, the UFO) and fires.
/// </summary>
public sealed class AttractPilot
{
    /// <summary>A hazard predicted to reach the ship's line within this many seconds is a threat.</summary>
    public const double ThreatSeconds = 0.9;

    /// <summary>Horizontal distance within which a hazard is a threat.</summary>
    public const double ThreatHalfWidth = 44.0;

    /// <summary>The pilot fires when the target is within this horizontal distance.</summary>
    public const double FireAlignment = 16.0;

    /// <summary>Decides the input for the next step.</summary>
    /// <param name="simulation">The simulation being demoed.</param>
    /// <returns>The input.</returns>
    /// <exception cref="ArgumentNullException">When <paramref name="simulation"/> is null.</exception>
    public GameInput Decide(GameSimulation simulation)
    {
        if (simulation == null)
        {
            throw new ArgumentNullException(nameof(simulation));
        }

        var player = simulation.Player;
        if (!player.IsPresent)
        {
            return GameInput.None;
        }

        if (TryFindThreat(simulation, out var threatX))
        {
            var direction = threatX > player.X ? -1 : threatX < player.X ? 1 : (player.X < Playfield.CenterX ? 1 : -1);
            if ((direction < 0 && player.X <= PlayerShip.MinX + 4) || (direction > 0 && player.X >= PlayerShip.MaxX - 4))
            {
                direction = -direction;
            }

            return new GameInput(direction, true, false);
        }

        if (!TryFindTarget(simulation, out var targetX))
        {
            var home = Playfield.CenterX - player.X;
            return new GameInput(Math.Abs(home) < 4 ? 0 : Math.Clamp(home / 40.0, -1, 1), false, false);
        }

        var dx = targetX - player.X;
        var axis = Math.Abs(dx) < 4 ? 0 : Math.Clamp(dx / 40.0, -1, 1);
        return new GameInput(axis, Math.Abs(dx) < FireAlignment, false);
    }

    internal static bool TryFindThreat(GameSimulation simulation, out double threatX)
    {
        var player = simulation.Player;
        var soonest = double.MaxValue;
        var foundX = 0.0;

        void Consider(double x, double y, double vx, double vy, double halfWidth)
        {
            var below = player.Y - y;
            if (below < -PlayerShip.Height || vy <= 0)
            {
                return;
            }

            var seconds = Math.Max(0, below) / vy;
            var predictedX = x + (vx * seconds);
            if (seconds <= ThreatSeconds && Math.Abs(predictedX - player.X) <= ThreatHalfWidth + halfWidth && seconds < soonest)
            {
                soonest = seconds;
                foundX = predictedX;
            }
        }

        foreach (var bolt in simulation.Projectiles.EnemyBolts)
        {
            Consider(bolt.X, bolt.Y, bolt.Vx, bolt.Vy, Projectile.EnemyBoltWidth / 2.0);
        }

        foreach (var missile in simulation.Projectiles.Missiles)
        {
            Consider(missile.X, missile.Y, 0, Math.Max(missile.Vy, 1), Projectile.MissileSize / 2.0);
        }

        foreach (var meteor in simulation.Meteors.Meteors)
        {
            Consider(meteor.X, meteor.Y, meteor.Vx, meteor.Vy, meteor.Box.HalfWidth);
        }

        foreach (var enemy in simulation.Enemies)
        {
            if (enemy.IsAlive && enemy.State != EnemyState.InFormation)
            {
                Consider(enemy.X, enemy.Y, 0, 200, Enemy.Width / 2.0);
            }
        }

        threatX = foundX;
        return soonest < double.MaxValue;
    }

    internal static bool TryFindTarget(GameSimulation simulation, out double targetX)
    {
        var player = simulation.Player;
        targetX = 0;
        var bestDistance = double.MaxValue;
        foreach (var enemy in simulation.Enemies)
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            var distance = Math.Abs(enemy.X - player.X);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                targetX = enemy.X;
            }
        }

        var boss = simulation.Boss;
        if (boss != null && (boss.State == BossState.Fighting || boss.State == BossState.Entering))
        {
            foreach (var section in boss.Sections)
            {
                if (section.IsDestroyed || (section.Kind == BossSectionKind.Core && boss.IsCoreArmoured))
                {
                    continue;
                }

                var x = boss.BoxOf(section).CenterX;
                var distance = Math.Abs(x - player.X);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    targetX = x;
                }
            }
        }

        if (bestDistance == double.MaxValue && simulation.Ufo != null)
        {
            targetX = simulation.Ufo.X;
            bestDistance = Math.Abs(targetX - player.X);
        }

        return bestDistance < double.MaxValue;
    }
}
