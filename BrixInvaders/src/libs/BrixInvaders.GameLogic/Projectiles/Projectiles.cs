using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>All projectiles in flight: player bolts, enemy bolts and homing missiles; movement, homing and culling.</summary>
public sealed class Projectiles
{
    /// <summary>Player bolt speed (straight up for a single shot).</summary>
    public const double PlayerBoltSpeed = 900.0;

    /// <summary>Homing missile speed before the loop speed scale.</summary>
    public const double MissileSpeed = 220.0;

    /// <summary>Homing missile turn rate in radians per second (90 degrees per second).</summary>
    public const double MissileTurnRate = Math.PI / 2.0;

    /// <summary>Seconds a homing missile flies before it expires.</summary>
    public const double MissileLifetime = 6.0;

    /// <summary>Distance beyond the playfield edge at which projectiles are culled.</summary>
    public const double CullMargin = 32.0;

    private readonly List<Projectile> _playerBolts = new List<Projectile>();
    private readonly List<Projectile> _enemyBolts = new List<Projectile>();
    private readonly List<Projectile> _missiles = new List<Projectile>();

    /// <summary>Player bolts in flight.</summary>
    public IReadOnlyList<Projectile> PlayerBolts => _playerBolts;

    /// <summary>Enemy bolts in flight.</summary>
    public IReadOnlyList<Projectile> EnemyBolts => _enemyBolts;

    /// <summary>Homing missiles in flight.</summary>
    public IReadOnlyList<Projectile> Missiles => _missiles;

    internal Projectile AddPlayerBolt(int id, double x, double y, double vx, double vy, bool piercing)
    {
        var bolt = new Projectile(id, ProjectileKind.PlayerBolt, x, y, vx, vy, piercing);
        _playerBolts.Add(bolt);
        return bolt;
    }

    internal Projectile AddEnemyBolt(int id, double x, double y, double vx, double vy)
    {
        var bolt = new Projectile(id, ProjectileKind.EnemyBolt, x, y, vx, vy, false);
        _enemyBolts.Add(bolt);
        return bolt;
    }

    internal Projectile AddMissile(int id, double x, double y, double speed)
    {
        var missile = new Projectile(id, ProjectileKind.Missile, x, y, 0, speed, false);
        _missiles.Add(missile);
        return missile;
    }

    /// <summary>
    /// Moves every projectile, steers missiles toward the target, expires old missiles and culls projectiles
    /// that left the playfield.
    /// </summary>
    /// <param name="dt">Step in seconds.</param>
    /// <param name="targetX">Homing target x (the player).</param>
    /// <param name="targetY">Homing target y (the player).</param>
    /// <param name="events">Receives MissileExpired events.</param>
    /// <returns>The number of player bolts that left the top without hitting anything (misses).</returns>
    internal int Update(double dt, double targetX, double targetY, GameEvents events)
    {
        var misses = 0;
        foreach (var bolt in _playerBolts)
        {
            Advance(bolt, dt);
            if (bolt.Box.Bottom < 0 || IsOutside(bolt))
            {
                bolt.IsAlive = false;
                if (!bolt.HasHit)
                {
                    misses++;
                }
            }
        }

        foreach (var bolt in _enemyBolts)
        {
            Advance(bolt, dt);
            if (IsOutside(bolt))
            {
                bolt.IsAlive = false;
            }
        }

        foreach (var missile in _missiles)
        {
            Steer(missile, dt, targetX, targetY);
            Advance(missile, dt);
            if (missile.Age >= MissileLifetime)
            {
                missile.IsAlive = false;
                events.Add(new GameEvent(GameEventKind.MissileExpired, missile.X, missile.Y));
            }
            else if (IsOutside(missile))
            {
                missile.IsAlive = false;
            }
        }

        RemoveDead();
        return misses;
    }

    /// <summary>Turns a missile toward the target by at most <see cref="MissileTurnRate"/> * dt, keeping its speed.</summary>
    /// <param name="missile">The missile.</param>
    /// <param name="dt">Step in seconds.</param>
    /// <param name="targetX">Target x.</param>
    /// <param name="targetY">Target y.</param>
    internal static void Steer(Projectile missile, double dt, double targetX, double targetY)
    {
        var speed = Math.Sqrt((missile.Vx * missile.Vx) + (missile.Vy * missile.Vy));
        var heading = Math.Atan2(missile.Vy, missile.Vx);
        var desired = Math.Atan2(targetY - missile.Y, targetX - missile.X);
        var difference = NormalizeAngle(desired - heading);
        var maxTurn = MissileTurnRate * dt;
        heading += Math.Clamp(difference, -maxTurn, maxTurn);
        missile.Vx = Math.Cos(heading) * speed;
        missile.Vy = Math.Sin(heading) * speed;
    }

    /// <summary>Normalises an angle to (-pi, pi].</summary>
    /// <param name="angle">Angle in radians.</param>
    /// <returns>The normalised angle.</returns>
    internal static double NormalizeAngle(double angle)
    {
        while (angle > Math.PI)
        {
            angle -= 2.0 * Math.PI;
        }

        while (angle <= -Math.PI)
        {
            angle += 2.0 * Math.PI;
        }

        return angle;
    }

    /// <summary>Removes every enemy bolt and missile (the bomb). Returns how many were removed.</summary>
    /// <returns>The number removed.</returns>
    internal int ClearEnemyFire()
    {
        var removed = _enemyBolts.Count + _missiles.Count;
        _enemyBolts.Clear();
        _missiles.Clear();
        return removed;
    }

    /// <summary>Removes everything (used between sectors).</summary>
    internal void Clear()
    {
        _playerBolts.Clear();
        _enemyBolts.Clear();
        _missiles.Clear();
    }

    /// <summary>Drops projectiles flagged dead by movement or collisions.</summary>
    internal void RemoveDead()
    {
        _playerBolts.RemoveAll(p => !p.IsAlive);
        _enemyBolts.RemoveAll(p => !p.IsAlive);
        _missiles.RemoveAll(p => !p.IsAlive);
    }

    private static void Advance(Projectile projectile, double dt)
    {
        projectile.X += projectile.Vx * dt;
        projectile.Y += projectile.Vy * dt;
        projectile.Age += dt;
    }

    private static bool IsOutside(Projectile projectile)
    {
        var box = projectile.Box;
        return box.Bottom < -CullMargin || box.Top > Playfield.Height + CullMargin
            || box.Right < -CullMargin || box.Left > Playfield.Width + CullMargin;
    }
}
