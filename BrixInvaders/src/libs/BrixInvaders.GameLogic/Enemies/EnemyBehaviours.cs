using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>
/// Enemy behaviours during a wave: the column-fire rule (the lowest enemy of a random column fires straight down),
/// shooter cadence (aimed bolts), missile carriers (homing missiles, capped on screen), and divers (leave the
/// formation, fly a cubic Bezier swoop toward the player, then return to their moving slot).
/// </summary>
public sealed class EnemyBehaviours
{
    /// <summary>Seconds a dive curve takes.</summary>
    public const double DiveDuration = 1.8;

    /// <summary>Speed of a diver flying back to its slot.</summary>
    public const double ReturnSpeed = 360.0;

    /// <summary>Largest horizontal speed of a shooter's aimed bolt.</summary>
    public const double MaxAimedBoltVx = 120.0;

    /// <summary>A diver fires once when it passes this fraction of its dive.</summary>
    public const double DiveFireAt = 0.5;

    private readonly DifficultySettings _difficulty;
    private readonly GameRandom _random;
    private readonly double _intervalScale;
    private readonly double _speedScale;
    private readonly int _maxBolts;
    private double _columnFireTimer;
    private double _diveTimer;

    /// <summary>Creates the behaviours for a sector.</summary>
    /// <param name="difficulty">Difficulty settings.</param>
    /// <param name="sector">Sector number (for loop scaling).</param>
    /// <param name="random">The simulation RNG.</param>
    public EnemyBehaviours(DifficultySettings difficulty, int sector, GameRandom random)
    {
        _difficulty = difficulty;
        _random = random;
        _intervalScale = SectorRules.IntervalScale(sector);
        _speedScale = SectorRules.SpeedScale(sector);
        _maxBolts = difficulty.MaxEnemyBolts + SectorRules.ExtraEnemyBolts(sector);
        MaxDivers = difficulty.MaxDivers + SectorRules.LoopOf(sector);
        MissileCap = difficulty.MissileCap + SectorRules.LoopOf(sector);
    }

    /// <summary>Seconds between column-fire shots.</summary>
    public double ColumnFireInterval => _difficulty.ColumnFireInterval * _intervalScale;

    /// <summary>Seconds between each shooter's aimed shots.</summary>
    public double ShooterInterval => _difficulty.ShooterInterval * _intervalScale;

    /// <summary>Seconds between each missile carrier's launches.</summary>
    public double MissileInterval => _difficulty.MissileInterval * _intervalScale;

    /// <summary>Seconds between dive launches.</summary>
    public double DiveInterval => _difficulty.DiveInterval * _intervalScale;

    /// <summary>Enemy bolt speed.</summary>
    public double BoltSpeed => _difficulty.EnemyBoltSpeed * _speedScale;

    /// <summary>Most enemy bolts on screen.</summary>
    public int MaxBolts => _maxBolts;

    /// <summary>Most divers out of formation.</summary>
    public int MaxDivers { get; }

    /// <summary>Most homing missiles on screen.</summary>
    public int MissileCap { get; }

    /// <summary>
    /// A point on the dive curve: P0 = start, P1 = (start.x + side x 180, start.y - 60), P2 = (target.x, 900),
    /// P3 = (target.x + side x 160, 560); x clamped to the playfield margins, y to at most 696.
    /// </summary>
    /// <param name="t">Curve parameter 0..1.</param>
    /// <param name="startX">Start x.</param>
    /// <param name="startY">Start y.</param>
    /// <param name="targetX">The player's x when the dive started.</param>
    /// <param name="side">+1 or -1: which way the dive first loops.</param>
    /// <returns>The point.</returns>
    public static (double X, double Y) DivePoint(double t, double startX, double startY, double targetX, int side)
    {
        t = Math.Clamp(t, 0.0, 1.0);
        var u = 1.0 - t;
        var b0 = u * u * u;
        var b1 = 3.0 * u * u * t;
        var b2 = 3.0 * u * t * t;
        var b3 = t * t * t;
        var x = (b0 * startX) + (b1 * (startX + (side * 180.0))) + (b2 * targetX) + (b3 * (targetX + (side * 160.0)));
        var y = (b0 * startY) + (b1 * (startY - 60.0)) + (b2 * 900.0) + (b3 * 560.0);
        var halfWidth = Enemy.Width / 2.0;
        x = Math.Clamp(x, Playfield.SideMargin + halfWidth, Playfield.Width - Playfield.SideMargin - halfWidth);
        y = Math.Min(y, Playfield.Height - Playfield.SideMargin);
        return (x, y);
    }

    /// <summary>Arms the timers for a new wave (random initial offsets so enemies do not fire in unison).</summary>
    /// <param name="enemies">The wave's enemies.</param>
    internal void StartWave(IReadOnlyList<Enemy> enemies)
    {
        _columnFireTimer = ColumnFireInterval;
        _diveTimer = DiveInterval;
        foreach (var enemy in enemies)
        {
            if (enemy.Role == EnemyRole.Shooter)
            {
                enemy.FireTimer = ShooterInterval * _random.NextDouble(0.5, 1.0);
            }
            else if (enemy.Role == EnemyRole.MissileCarrier)
            {
                enemy.FireTimer = MissileInterval * _random.NextDouble(0.5, 1.0);
            }
        }
    }

    /// <summary>Runs every behaviour for one step.</summary>
    internal void Update(double dt, FormationController formation, PlayerShip player, Projectiles projectiles,
        Func<int> nextId, GameEvents events)
    {
        ColumnFire(dt, formation, projectiles, nextId, events);
        foreach (var enemy in formation.Enemies)
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            if (enemy.Role == EnemyRole.Shooter && enemy.State == EnemyState.InFormation)
            {
                enemy.FireTimer -= dt;
                if (enemy.FireTimer <= 0)
                {
                    enemy.FireTimer += ShooterInterval;
                    if (projectiles.EnemyBolts.Count < _maxBolts)
                    {
                        var vx = Math.Clamp((player.X - enemy.X) * 0.5, -MaxAimedBoltVx, MaxAimedBoltVx);
                        FireBolt(enemy, vx, projectiles, nextId, events);
                    }
                }
            }
            else if (enemy.Role == EnemyRole.MissileCarrier && enemy.State == EnemyState.InFormation)
            {
                enemy.FireTimer -= dt;
                if (enemy.FireTimer <= 0)
                {
                    enemy.FireTimer += MissileInterval;
                    if (projectiles.Missiles.Count < MissileCap)
                    {
                        var y = enemy.Y + (Enemy.Height / 2.0) + (Projectile.MissileSize / 2.0);
                        projectiles.AddMissile(nextId(), enemy.X, y, Projectiles.MissileSpeed * _speedScale);
                        events.Add(new GameEvent(GameEventKind.MissileLaunched, enemy.X, y));
                    }
                }
            }
        }

        LaunchDives(dt, formation, player, events);
        MoveDivers(dt, formation, projectiles, nextId, events);
    }

    internal int DivingCount(FormationController formation)
    {
        var count = 0;
        foreach (var enemy in formation.Enemies)
        {
            if (enemy.IsAlive && enemy.State != EnemyState.InFormation)
            {
                count++;
            }
        }

        return count;
    }

    private void ColumnFire(double dt, FormationController formation, Projectiles projectiles, Func<int> nextId, GameEvents events)
    {
        _columnFireTimer -= dt;
        if (_columnFireTimer > 0)
        {
            return;
        }

        _columnFireTimer += ColumnFireInterval;
        if (projectiles.EnemyBolts.Count >= _maxBolts)
        {
            return;
        }

        var lowestByColumn = new SortedDictionary<int, Enemy>();
        foreach (var enemy in formation.Enemies)
        {
            if (!enemy.IsAlive || enemy.State != EnemyState.InFormation)
            {
                continue;
            }

            if (!lowestByColumn.TryGetValue(enemy.Column, out var current) || enemy.Row > current.Row)
            {
                lowestByColumn[enemy.Column] = enemy;
            }
        }

        if (lowestByColumn.Count == 0)
        {
            return;
        }

        var pick = _random.Next(lowestByColumn.Count);
        foreach (var shooter in lowestByColumn.Values)
        {
            if (pick-- == 0)
            {
                FireBolt(shooter, 0, projectiles, nextId, events);
                return;
            }
        }
    }

    private void FireBolt(Enemy enemy, double vx, Projectiles projectiles, Func<int> nextId, GameEvents events)
    {
        var y = enemy.Y + (Enemy.Height / 2.0) + (Projectile.EnemyBoltHeight / 2.0);
        projectiles.AddEnemyBolt(nextId(), enemy.X, y, vx, BoltSpeed);
        events.Add(new GameEvent(GameEventKind.EnemyFired, enemy.X, y));
    }

    private void LaunchDives(double dt, FormationController formation, PlayerShip player, GameEvents events)
    {
        _diveTimer -= dt;
        if (_diveTimer > 0)
        {
            return;
        }

        _diveTimer += DiveInterval;
        if (DivingCount(formation) >= MaxDivers)
        {
            return;
        }

        var candidates = new List<Enemy>();
        foreach (var enemy in formation.Enemies)
        {
            if (enemy.IsAlive && enemy.Role == EnemyRole.Diver && enemy.State == EnemyState.InFormation)
            {
                candidates.Add(enemy);
            }
        }

        if (candidates.Count == 0)
        {
            return;
        }

        var diver = candidates[_random.Next(candidates.Count)];
        diver.State = EnemyState.Diving;
        diver.DiveTime = 0;
        diver.DiveStartX = diver.X;
        diver.DiveStartY = diver.Y;
        diver.DiveTargetX = player.X;
        diver.DiveSide = diver.X < Playfield.CenterX ? -1 : 1;
        diver.HasFiredThisDive = false;
        events.Add(new GameEvent(GameEventKind.DiverLaunched, diver.X, diver.Y, role: diver.Role, colour: diver.Colour));
    }

    private void MoveDivers(double dt, FormationController formation, Projectiles projectiles, Func<int> nextId, GameEvents events)
    {
        foreach (var enemy in formation.Enemies)
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            if (enemy.State == EnemyState.Diving)
            {
                enemy.DiveTime += dt;
                var t = enemy.DiveTime / DiveDuration;
                var point = DivePoint(t, enemy.DiveStartX, enemy.DiveStartY, enemy.DiveTargetX, enemy.DiveSide);
                enemy.X = point.X;
                enemy.Y = point.Y;
                if (!enemy.HasFiredThisDive && t >= DiveFireAt)
                {
                    enemy.HasFiredThisDive = true;
                    if (projectiles.EnemyBolts.Count < _maxBolts)
                    {
                        FireBolt(enemy, 0, projectiles, nextId, events);
                    }
                }

                if (t >= 1.0)
                {
                    enemy.State = EnemyState.Returning;
                }
            }
            else if (enemy.State == EnemyState.Returning)
            {
                var homeX = formation.HomeX(enemy);
                var homeY = formation.HomeY(enemy);
                var dx = homeX - enemy.X;
                var dy = homeY - enemy.Y;
                var distance = Math.Sqrt((dx * dx) + (dy * dy));
                var travel = ReturnSpeed * dt;
                if (distance <= travel)
                {
                    enemy.State = EnemyState.InFormation;
                    enemy.X = homeX;
                    enemy.Y = homeY;
                }
                else
                {
                    enemy.X += dx / distance * travel;
                    enemy.Y += dy / distance * travel;
                }
            }
        }
    }
}
