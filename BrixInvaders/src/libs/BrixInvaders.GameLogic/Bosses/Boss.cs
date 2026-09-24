using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>
/// A sector boss: a multi-part ship with destructible sections, 2 or 3 attack phases switched at total-health
/// thresholds, a weaving movement, and per-section attacks. Designs 1..5 repeat on every loop with more health.
/// </summary>
public sealed class Boss
{
    /// <summary>Centre y the boss settles at.</summary>
    public const double FightY = 170.0;

    /// <summary>Centre y the boss enters from.</summary>
    public const double EntryY = -160.0;

    /// <summary>Entry speed.</summary>
    public const double EntrySpeed = 110.0;

    /// <summary>Horizontal weave amplitude around the playfield centre.</summary>
    public const double WeaveAmplitude = 340.0;

    /// <summary>Seconds the defeated boss explodes before it is gone.</summary>
    public const double DyingDuration = 2.0;

    /// <summary>Damage a bomb does to every damageable section.</summary>
    public const int BombDamage = 10;

    private static readonly double[] WeaveSpeeds = { 0.5, 0.8, 1.1 };
    private static readonly double[] TurretIntervals = { 1.8, 1.3, 0.9 };
    private static readonly double[] CannonIntervals = { 3.0, 2.4, 1.8 };
    private static readonly double[] MissileIntervals = { 5.0, 4.0, 3.0 };
    private static readonly double[] CoreIntervals = { 2.0, 1.5, 1.1 };
    private static readonly double[] CannonFanDegrees = { -30, -15, 0, 15, 30 };
    private static readonly double[] CoreFanDegrees = { -10, 0, 10 };

    private readonly List<BossSection> _sections;
    private readonly double[] _thresholds;
    private readonly double _fireScale;
    private readonly double _boltSpeed;
    private readonly int _missileCap;
    private readonly double _missileSpeed;
    private double _weaveAngle;

    private Boss(int sector, List<BossSection> sections, double[] thresholds, DifficultySettings difficulty)
    {
        Sector = sector;
        Design = SectorRules.DesignOf(sector);
        _sections = sections;
        _thresholds = thresholds;
        _fireScale = difficulty.BossFireScale * SectorRules.IntervalScale(sector);
        _boltSpeed = difficulty.EnemyBoltSpeed * SectorRules.SpeedScale(sector);
        _missileCap = difficulty.MissileCap + SectorRules.LoopOf(sector);
        _missileSpeed = Projectiles.MissileSpeed * SectorRules.SpeedScale(sector);
        X = Playfield.CenterX;
        Y = EntryY;
        Phase = 1;
        State = BossState.Entering;
        foreach (var section in _sections)
        {
            TotalHealth += section.MaxHealth;
            section.FireTimer = IntervalOf(section.Kind) * (0.5 + (0.15 * section.Index));
        }
    }

    /// <summary>Sector number.</summary>
    public int Sector { get; }

    /// <summary>Design 1..5.</summary>
    public int Design { get; }

    /// <summary>Centre x.</summary>
    public double X { get; private set; }

    /// <summary>Centre y.</summary>
    public double Y { get; private set; }

    /// <summary>Life-cycle state.</summary>
    public BossState State { get; private set; }

    /// <summary>Current attack phase, 1..<see cref="PhaseCount"/>.</summary>
    public int Phase { get; private set; }

    /// <summary>Number of attack phases (2 for design 1, else 3).</summary>
    public int PhaseCount => _thresholds.Length + 1;

    /// <summary>The total-health fractions at which the phase advances (e.g. 0.66 and 0.33).</summary>
    public IReadOnlyList<double> PhaseThresholds => _thresholds;

    /// <summary>The sections; index 0 is the core.</summary>
    public IReadOnlyList<BossSection> Sections => _sections;

    /// <summary>Sum of all sections' full health.</summary>
    public int TotalHealth { get; }

    /// <summary>Sum of all sections' remaining health.</summary>
    public int RemainingHealth
    {
        get
        {
            var sum = 0;
            foreach (var section in _sections)
            {
                sum += Math.Max(0, section.Health);
            }

            return sum;
        }
    }

    /// <summary>Remaining / total health, 0..1 (for the health bar).</summary>
    public double HealthFraction => TotalHealth == 0 ? 0 : (double)RemainingHealth / TotalHealth;

    /// <summary>True while the core is armoured (some other section still stands).</summary>
    public bool IsCoreArmoured
    {
        get
        {
            for (var i = 1; i < _sections.Count; i++)
            {
                if (!_sections[i].IsDestroyed)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>Seconds spent in the <see cref="BossState.Dying"/> state.</summary>
    public double DyingTime { get; private set; }

    /// <summary>Creates the boss of a sector.</summary>
    /// <param name="sector">Sector number, 1 or more.</param>
    /// <param name="difficulty">Difficulty settings (health, fire scale, missile cap).</param>
    /// <param name="nextId">Id source for the sections.</param>
    /// <returns>The boss, entering.</returns>
    public static Boss Create(int sector, DifficultySettings difficulty, Func<int> nextId)
    {
        var design = SectorRules.DesignOf(sector);
        var healthScale = difficulty.BossHealthPercent / 100.0 * SectorRules.BossHealthScale(sector);
        var sections = new List<BossSection>();

        void Add(BossSectionKind kind, double ox, double oy, double w, double h, int baseHealth)
            => sections.Add(new BossSection(nextId(), sections.Count, kind, ox, oy, w, h,
                Math.Max(1, (int)Math.Round(baseHealth * healthScale))));

        var healths = BaseHealths(design);
        Add(BossSectionKind.Core, 0, 0, 160, 96, healths[0]);
        Add(BossSectionKind.Turret, -150, 20, 56, 56, healths[1]);
        Add(BossSectionKind.Turret, 150, 20, 56, 56, healths[1]);
        if (design == 2 || design == 3 || design == 5)
        {
            Add(BossSectionKind.Cannon, 0, 70, 64, 40, healths[2]);
        }

        if (design >= 4)
        {
            Add(BossSectionKind.MissileBay, -230, -10, 56, 48, healths[3]);
            Add(BossSectionKind.MissileBay, 230, -10, 56, 48, healths[3]);
        }

        var thresholds = design == 1 ? new[] { 0.5 } : new[] { 0.66, 0.33 };
        return new Boss(sector, sections, thresholds, difficulty);
    }

    /// <summary>
    /// Base (Pilot, first loop) section health of a design: core, each turret, cannon, each missile bay
    /// (0 where the design has no such section).
    /// </summary>
    /// <param name="design">Design 1..5.</param>
    /// <returns>Four health values.</returns>
    public static int[] BaseHealths(int design) => design switch
    {
        1 => new[] { 60, 20, 0, 0 },
        2 => new[] { 80, 25, 35, 0 },
        3 => new[] { 100, 30, 40, 0 },
        4 => new[] { 120, 30, 0, 35 },
        5 => new[] { 150, 35, 45, 40 },
        _ => throw new ArgumentOutOfRangeException(nameof(design), design, "Design must be 1..5."),
    };

    /// <summary>The phase for a health fraction: 1 + the number of thresholds at or above the fraction.</summary>
    /// <param name="fraction">Remaining / total health.</param>
    /// <param name="thresholds">Phase thresholds, descending.</param>
    /// <returns>The phase.</returns>
    public static int PhaseFor(double fraction, IReadOnlyList<double> thresholds)
    {
        var phase = 1;
        foreach (var threshold in thresholds)
        {
            if (fraction <= threshold)
            {
                phase++;
            }
        }

        return phase;
    }

    /// <summary>The hit box of a section at the boss's current position.</summary>
    /// <param name="section">The section.</param>
    /// <returns>Its box.</returns>
    public Box BoxOf(BossSection section) => section.BoxAt(X, Y);

    /// <summary>Advances movement and attacks.</summary>
    internal void Update(double dt, PlayerShip player, Projectiles projectiles, Func<int> nextId, GameEvents events)
    {
        switch (State)
        {
            case BossState.Entering:
                Y = Math.Min(FightY, Y + (EntrySpeed * dt));
                if (Y >= FightY)
                {
                    State = BossState.Fighting;
                }

                break;
            case BossState.Fighting:
                _weaveAngle += WeaveSpeeds[Phase - 1] * dt;
                X = Playfield.CenterX + (WeaveAmplitude * Math.Sin(_weaveAngle));
                Attack(dt, player, projectiles, nextId, events);
                break;
            case BossState.Dying:
                DyingTime += dt;
                if (DyingTime >= DyingDuration)
                {
                    State = BossState.Gone;
                }

                break;
        }
    }

    /// <summary>
    /// Applies damage to a section and handles phase changes and defeat. Emits BossHit and BossPhaseChanged; the
    /// simulation emits BossSectionDestroyed / BossDefeated with the awarded points.
    /// </summary>
    internal BossDamageResult ApplyDamage(BossSection section, int damage, GameEvents events)
    {
        if (State == BossState.Dying || State == BossState.Gone || section.IsDestroyed)
        {
            return BossDamageResult.Ignored;
        }

        var box = BoxOf(section);
        if (State == BossState.Entering || (section.Kind == BossSectionKind.Core && IsCoreArmoured))
        {
            events.Add(new GameEvent(GameEventKind.BossHit, box.CenterX, box.CenterY, value: 1, section: section.Kind));
            return BossDamageResult.Armoured;
        }

        section.Health = Math.Max(0, section.Health - damage);
        events.Add(new GameEvent(GameEventKind.BossHit, box.CenterX, box.CenterY, value: 0, section: section.Kind));
        var newPhase = Math.Max(Phase, PhaseFor(HealthFraction, _thresholds));
        if (!section.IsDestroyed)
        {
            ChangePhase(newPhase, events);
            return BossDamageResult.Damaged;
        }

        if (section.Kind == BossSectionKind.Core)
        {
            State = BossState.Dying;
            DyingTime = 0;
            return BossDamageResult.Defeated;
        }

        ChangePhase(newPhase, events);
        return BossDamageResult.SectionDestroyed;
    }

    private void ChangePhase(int newPhase, GameEvents events)
    {
        if (newPhase > Phase)
        {
            Phase = Math.Min(newPhase, PhaseCount);
            events.Add(new GameEvent(GameEventKind.BossPhaseChanged, X, Y, value: Phase));
        }
    }

    private double IntervalOf(BossSectionKind kind)
    {
        var index = Math.Clamp(Phase - 1, 0, 2);
        var seconds = kind switch
        {
            BossSectionKind.Turret => TurretIntervals[index],
            BossSectionKind.Cannon => CannonIntervals[index],
            BossSectionKind.MissileBay => MissileIntervals[index],
            _ => CoreIntervals[index],
        };
        return seconds * _fireScale;
    }

    private void Attack(double dt, PlayerShip player, Projectiles projectiles, Func<int> nextId, GameEvents events)
    {
        foreach (var section in _sections)
        {
            if (section.IsDestroyed || (section.Kind == BossSectionKind.Core && IsCoreArmoured))
            {
                continue;
            }

            section.FireTimer -= dt;
            if (section.FireTimer > 0)
            {
                continue;
            }

            section.FireTimer += IntervalOf(section.Kind);
            var box = BoxOf(section);
            var muzzleX = box.CenterX;
            var muzzleY = box.Bottom;
            switch (section.Kind)
            {
                case BossSectionKind.Turret:
                    var dx = player.X - muzzleX;
                    var dy = player.Y - muzzleY;
                    var length = Math.Sqrt((dx * dx) + (dy * dy));
                    if (length < 1)
                    {
                        dx = 0;
                        dy = 1;
                        length = 1;
                    }

                    projectiles.AddEnemyBolt(nextId(), muzzleX, muzzleY, dx / length * _boltSpeed, dy / length * _boltSpeed);
                    events.Add(new GameEvent(GameEventKind.EnemyFired, muzzleX, muzzleY));
                    break;
                case BossSectionKind.Cannon:
                    Fan(CannonFanDegrees, muzzleX, muzzleY, projectiles, nextId, events);
                    break;
                case BossSectionKind.MissileBay:
                    if (projectiles.Missiles.Count < _missileCap)
                    {
                        projectiles.AddMissile(nextId(), muzzleX, muzzleY, _missileSpeed);
                        events.Add(new GameEvent(GameEventKind.MissileLaunched, muzzleX, muzzleY));
                    }

                    break;
                default:
                    Fan(CoreFanDegrees, muzzleX, muzzleY, projectiles, nextId, events);
                    break;
            }
        }
    }

    private void Fan(double[] degrees, double x, double y, Projectiles projectiles, Func<int> nextId, GameEvents events)
    {
        foreach (var degree in degrees)
        {
            var radians = degree * Math.PI / 180.0;
            projectiles.AddEnemyBolt(nextId(), x, y, Math.Sin(radians) * _boltSpeed, Math.Cos(radians) * _boltSpeed);
        }

        events.Add(new GameEvent(GameEventKind.EnemyFired, x, y, value: degrees.Length));
    }
}
