using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>A bolt or missile in flight.</summary>
public sealed class Projectile
{
    /// <summary>Player bolt hit-box width.</summary>
    public const double PlayerBoltWidth = 6.0;

    /// <summary>Player bolt hit-box height.</summary>
    public const double PlayerBoltHeight = 24.0;

    /// <summary>Enemy bolt hit-box width.</summary>
    public const double EnemyBoltWidth = 6.0;

    /// <summary>Enemy bolt hit-box height.</summary>
    public const double EnemyBoltHeight = 18.0;

    /// <summary>Missile hit-box size (square, so the box does not depend on the heading).</summary>
    public const double MissileSize = 14.0;

    private readonly HashSet<int> _hitIds = new HashSet<int>();

    internal Projectile(int id, ProjectileKind kind, double x, double y, double vx, double vy, bool piercing)
    {
        Id = id;
        Kind = kind;
        X = x;
        Y = y;
        Vx = vx;
        Vy = vy;
        IsPiercing = piercing;
        IsAlive = true;
    }

    /// <summary>Unique id within the simulation.</summary>
    public int Id { get; }

    /// <summary>The kind.</summary>
    public ProjectileKind Kind { get; }

    /// <summary>Centre x.</summary>
    public double X { get; internal set; }

    /// <summary>Centre y.</summary>
    public double Y { get; internal set; }

    /// <summary>Velocity x in world units per second.</summary>
    public double Vx { get; internal set; }

    /// <summary>Velocity y in world units per second.</summary>
    public double Vy { get; internal set; }

    /// <summary>Direction of travel in radians (0 = right, pi/2 = down); use it to rotate the sprite.</summary>
    public double Heading => Math.Atan2(Vy, Vx);

    /// <summary>True for a piercing player bolt: it passes through targets, hitting each one once.</summary>
    public bool IsPiercing { get; }

    /// <summary>Seconds since launch.</summary>
    public double Age { get; internal set; }

    /// <summary>False once the projectile has hit, left the playfield or expired.</summary>
    public bool IsAlive { get; internal set; }

    /// <summary>True once a player bolt has hit anything (so leaving the screen is not a miss).</summary>
    public bool HasHit => _hitIds.Count > 0;

    /// <summary>The hit box.</summary>
    public Box Box => Kind switch
    {
        ProjectileKind.PlayerBolt => Box.FromCenter(X, Y, PlayerBoltWidth, PlayerBoltHeight),
        ProjectileKind.EnemyBolt => Box.FromCenter(X, Y, EnemyBoltWidth, EnemyBoltHeight),
        _ => Box.FromCenter(X, Y, MissileSize, MissileSize),
    };

    internal bool HasHitTarget(int targetId) => _hitIds.Contains(targetId);

    internal void RecordHit(int targetId) => _hitIds.Add(targetId);
}
