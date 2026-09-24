using System;
using System.Collections.Generic;

namespace BrixInvaders.GameLogic;

/// <summary>
/// Power-up drops (chance per destroyed enemy, weighted kind, drift down, time out) and the active timed
/// power-ups. Stacking rules: collecting a timed power-up that is already active REFRESHES its timer to the full
/// duration (it does not add); different timed power-ups run at the same time; shield strength adds up to 3;
/// extra lives up to 9; bombs up to 3; a surplus life / bomb / shield pays a bonus instead.
/// </summary>
public sealed class PowerUpSystem
{
    /// <summary>Drift speed of a drop.</summary>
    public const double DriftSpeed = 120.0;

    /// <summary>Seconds a drop stays before it times out.</summary>
    public const double DropLifetime = 8.0;

    /// <summary>Bonus base points for a surplus extra life.</summary>
    public const int SurplusLifeBonus = 1000;

    /// <summary>Bonus base points for a surplus bomb.</summary>
    public const int SurplusBombBonus = 500;

    /// <summary>Bonus base points for a surplus shield.</summary>
    public const int SurplusShieldBonus = 250;

    private static readonly PowerUpKind[] Kinds =
    {
        PowerUpKind.SpreadShot, PowerUpKind.RapidFire, PowerUpKind.PiercingLaser, PowerUpKind.ShieldBubble,
        PowerUpKind.SpeedBoost, PowerUpKind.ExtraLife, PowerUpKind.Bomb,
    };

    private static readonly int[] Weights = { 20, 20, 15, 15, 15, 5, 10 };

    private readonly List<PowerUpDrop> _drops = new List<PowerUpDrop>();
    private readonly double[] _timers = new double[Kinds.Length];

    /// <summary>Drops on the playfield.</summary>
    public IReadOnlyList<PowerUpDrop> Drops => _drops;

    /// <summary>The kind weights (out of 100) used to pick a dropped power-up.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>Its weight.</returns>
    public static int WeightOf(PowerUpKind kind) => Weights[(int)kind];

    /// <summary>Duration of a timed power-up in seconds, or 0 for the instant ones.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>Seconds, or 0.</returns>
    public static double DurationOf(PowerUpKind kind) => kind switch
    {
        PowerUpKind.SpreadShot => 12.0,
        PowerUpKind.RapidFire => 12.0,
        PowerUpKind.PiercingLaser => 10.0,
        PowerUpKind.SpeedBoost => 10.0,
        _ => 0.0,
    };

    /// <summary>True for the power-ups that run on a timer.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>True when timed.</returns>
    public static bool IsTimed(PowerUpKind kind) => DurationOf(kind) > 0;

    /// <summary>True while a timed power-up is active.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>True when active.</returns>
    public bool IsActive(PowerUpKind kind) => _timers[(int)kind] > 0;

    /// <summary>Seconds left on a timed power-up (0 when inactive).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>Seconds left.</returns>
    public double TimeLeft(PowerUpKind kind) => _timers[(int)kind];

    /// <summary>The active timed power-ups with their time left, in enum order (for the HUD).</summary>
    /// <returns>The active timers.</returns>
    public IReadOnlyList<KeyValuePair<PowerUpKind, double>> ActiveTimers()
    {
        var result = new List<KeyValuePair<PowerUpKind, double>>();
        foreach (var kind in Kinds)
        {
            if (IsActive(kind))
            {
                result.Add(new KeyValuePair<PowerUpKind, double>(kind, TimeLeft(kind)));
            }
        }

        return result;
    }

    /// <summary>Picks a power-up kind by weight.</summary>
    /// <param name="random">The simulation RNG.</param>
    /// <returns>The kind.</returns>
    internal static PowerUpKind ChooseKind(GameRandom random)
    {
        var total = 0;
        foreach (var weight in Weights)
        {
            total += weight;
        }

        var roll = random.Next(total);
        for (var i = 0; i < Weights.Length; i++)
        {
            if (roll < Weights[i])
            {
                return Kinds[i];
            }

            roll -= Weights[i];
        }

        return Kinds[Kinds.Length - 1];
    }

    /// <summary>Rolls the drop chance; on success spawns a weighted random power-up.</summary>
    /// <returns>The drop, or null.</returns>
    internal PowerUpDrop TryDrop(double x, double y, double chancePercent, GameRandom random, int id, GameEvents events)
        => random.Chance(chancePercent) ? Spawn(ChooseKind(random), x, y, id, events) : null;

    /// <summary>Spawns a specific drop.</summary>
    /// <returns>The drop.</returns>
    internal PowerUpDrop Spawn(PowerUpKind kind, double x, double y, int id, GameEvents events)
    {
        var drop = new PowerUpDrop(id, kind, x, y);
        _drops.Add(drop);
        events.Add(new GameEvent(GameEventKind.PowerUpDropped, x, y, powerUp: kind));
        return drop;
    }

    /// <summary>Moves drops, times them out and counts down the active timers.</summary>
    internal void Update(double dt, GameEvents events)
    {
        foreach (var drop in _drops)
        {
            drop.Y += DriftSpeed * dt;
            drop.Age += dt;
            if (drop.Age >= DropLifetime || drop.Box.Top > Playfield.Height)
            {
                drop.IsAlive = false;
                events.Add(new GameEvent(GameEventKind.PowerUpLost, drop.X, drop.Y, powerUp: drop.Kind));
            }
        }

        _drops.RemoveAll(d => !d.IsAlive);
        for (var i = 0; i < _timers.Length; i++)
        {
            if (_timers[i] > 0)
            {
                _timers[i] = Math.Max(0, _timers[i] - dt);
                if (_timers[i] == 0)
                {
                    events.Add(new GameEvent(GameEventKind.PowerUpExpired, powerUp: Kinds[i]));
                }
            }
        }
    }

    /// <summary>Collects every drop that touches the player's ship.</summary>
    internal void CollectTouching(PlayerShip player, Scoring scoring, GameEvents events)
    {
        if (!player.IsPresent)
        {
            return;
        }

        var shipBox = player.Box;
        foreach (var drop in _drops)
        {
            if (CollisionResolver.Overlaps(shipBox, drop.Box))
            {
                drop.IsAlive = false;
                Apply(drop.Kind, player, scoring, events);
                events.Add(new GameEvent(GameEventKind.PowerUpCollected, drop.X, drop.Y, powerUp: drop.Kind));
            }
        }

        _drops.RemoveAll(d => !d.IsAlive);
    }

    /// <summary>Applies a power-up to the player (stacking and surplus rules).</summary>
    internal void Apply(PowerUpKind kind, PlayerShip player, Scoring scoring, GameEvents events)
    {
        switch (kind)
        {
            case PowerUpKind.ShieldBubble:
                if (player.ShieldStrength < PlayerShip.MaxShield)
                {
                    player.ShieldStrength++;
                }
                else
                {
                    Bonus(SurplusShieldBonus, scoring, events);
                }

                break;
            case PowerUpKind.ExtraLife:
                if (player.Lives < PlayerShip.MaxLives)
                {
                    player.Lives++;
                }
                else
                {
                    Bonus(SurplusLifeBonus, scoring, events);
                }

                break;
            case PowerUpKind.Bomb:
                if (player.Bombs < PlayerShip.MaxBombs)
                {
                    player.Bombs++;
                }
                else
                {
                    Bonus(SurplusBombBonus, scoring, events);
                }

                break;
            default:
                _timers[(int)kind] = DurationOf(kind);
                break;
        }
    }

    /// <summary>Removes all drops (between sectors).</summary>
    internal void ClearDrops() => _drops.Clear();

    private static void Bonus(int points, Scoring scoring, GameEvents events)
    {
        var awarded = scoring.AwardBonus(points);
        events.Add(new GameEvent(GameEventKind.BonusAwarded, points: awarded));
    }
}
