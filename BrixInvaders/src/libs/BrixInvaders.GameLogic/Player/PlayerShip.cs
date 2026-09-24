using System;

namespace BrixInvaders.GameLogic;

/// <summary>
/// The player's ship: movement clamped to the playfield, fire cooldown, hull with damage tiers, lives, respawn with
/// invulnerability, bombs and the shield bubble.
/// </summary>
public sealed class PlayerShip
{
    /// <summary>Hit-box width.</summary>
    public const double Width = 64.0;

    /// <summary>Hit-box height.</summary>
    public const double Height = 48.0;

    /// <summary>Centre y of the ship (fixed).</summary>
    public const double LineY = 660.0;

    /// <summary>Movement speed.</summary>
    public const double Speed = 420.0;

    /// <summary>Speed multiplier while the speed boost is active.</summary>
    public const double SpeedBoostFactor = 1.5;

    /// <summary>Seconds between shots.</summary>
    public const double FireInterval = 0.30;

    /// <summary>Seconds between shots with rapid fire.</summary>
    public const double RapidFireInterval = 0.15;

    /// <summary>Hull points of a fresh ship. Damage overlays: hull 2 = 2/3, hull 1 = 1/3.</summary>
    public const int MaxHull = 3;

    /// <summary>Seconds between destruction and respawn.</summary>
    public const double RespawnDelay = 1.5;

    /// <summary>Invulnerability after a respawn.</summary>
    public const double RespawnInvulnerability = 2.5;

    /// <summary>Invulnerability after a hull hit that did not destroy the ship.</summary>
    public const double HitInvulnerability = 1.0;

    /// <summary>Invulnerability after the shield absorbed a hit.</summary>
    public const double ShieldInvulnerability = 0.5;

    /// <summary>Most lives the player can hold.</summary>
    public const int MaxLives = 9;

    /// <summary>Most bombs the player can stock.</summary>
    public const int MaxBombs = 3;

    /// <summary>Highest shield bubble strength.</summary>
    public const int MaxShield = 3;

    /// <summary>Smallest centre x (the ship never crosses the side margin).</summary>
    public const double MinX = Playfield.SideMargin + (Width / 2.0);

    /// <summary>Largest centre x.</summary>
    public const double MaxX = Playfield.Width - Playfield.SideMargin - (Width / 2.0);

    /// <summary>Timers closer than this to zero count as expired (absorbs floating-point step residue).</summary>
    public const double TimerEpsilon = 1e-9;

    internal PlayerShip(int lives, int bombs)
    {
        Lives = lives;
        Bombs = Math.Clamp(bombs, 0, MaxBombs);
        Hull = MaxHull;
        X = Playfield.CenterX;
        IsPresent = true;
    }

    /// <summary>Centre x.</summary>
    public double X { get; internal set; }

    /// <summary>Centre y.</summary>
    public double Y => LineY;

    /// <summary>Lives left, including the ship on screen.</summary>
    public int Lives { get; internal set; }

    /// <summary>Hull points, 1..3 while present.</summary>
    public int Hull { get; internal set; }

    /// <summary>Damage overlay tier: 0 intact, 1 at 2/3 hull, 2 at 1/3 hull.</summary>
    public int DamageTier => MaxHull - Math.Clamp(Hull, 1, MaxHull);

    /// <summary>False between destruction and respawn (and after the last life).</summary>
    public bool IsPresent { get; internal set; }

    /// <summary>Seconds until respawn while not present.</summary>
    public double RespawnTimer { get; internal set; }

    /// <summary>Seconds of invulnerability left.</summary>
    public double InvulnerableTime { get; internal set; }

    /// <summary>True while hits are ignored (blink the ship).</summary>
    public bool IsInvulnerable => InvulnerableTime > 0;

    /// <summary>Bombs in stock, 0..3.</summary>
    public int Bombs { get; internal set; }

    /// <summary>Shield bubble strength, 0..3.</summary>
    public int ShieldStrength { get; internal set; }

    /// <summary>Seconds until the next shot is allowed.</summary>
    public double FireCooldown { get; internal set; }

    /// <summary>The hit box.</summary>
    public Box Box => Box.FromCenter(X, LineY, Width, Height);

    /// <summary>Moves the ship horizontally and clamps it to the playfield.</summary>
    /// <param name="axis">-1 .. 1 (clamped).</param>
    /// <param name="dt">Step in seconds.</param>
    /// <param name="boosted">True while the speed boost is active.</param>
    internal void Move(double axis, double dt, bool boosted)
    {
        if (!IsPresent)
        {
            return;
        }

        var speed = boosted ? Speed * SpeedBoostFactor : Speed;
        X = Math.Clamp(X + (Math.Clamp(axis, -1.0, 1.0) * speed * dt), MinX, MaxX);
    }

    /// <summary>Counts down cooldown, invulnerability and the respawn timer; respawns when due.</summary>
    /// <param name="dt">Step in seconds.</param>
    /// <param name="events">Receives PlayerRespawned.</param>
    internal void Update(double dt, GameEvents events)
    {
        FireCooldown = CountDown(FireCooldown, dt);
        InvulnerableTime = CountDown(InvulnerableTime, dt);
        if (!IsPresent && Lives > 0)
        {
            RespawnTimer -= dt;
            if (RespawnTimer <= 0)
            {
                RespawnTimer = 0;
                IsPresent = true;
                Hull = MaxHull;
                X = Playfield.CenterX;
                InvulnerableTime = RespawnInvulnerability;
                events.Add(new GameEvent(GameEventKind.PlayerRespawned, X, LineY));
            }
        }
    }

    /// <summary>
    /// Counts a timer down, snapping values within <see cref="TimerEpsilon"/> of zero to zero so that an interval
    /// that is a whole number of fixed steps (0.15 s = 9 steps) really lasts that many steps.
    /// </summary>
    /// <param name="value">Timer value.</param>
    /// <param name="dt">Step in seconds.</param>
    /// <returns>The new value, never negative.</returns>
    internal static double CountDown(double value, double dt)
    {
        var next = value - dt;
        return next < TimerEpsilon ? 0 : next;
    }

    /// <summary>True when a shot may be fired now.</summary>
    /// <returns>True when present and the cooldown is over.</returns>
    internal bool CanFire() => IsPresent && FireCooldown <= 0;

    /// <summary>Applies one hit: shield first, then hull; destroys the ship at hull 0.</summary>
    /// <param name="events">Receives ShieldAbsorbed / PlayerHit / PlayerDestroyed.</param>
    /// <returns>What happened.</returns>
    internal PlayerHitResult ApplyHit(GameEvents events)
    {
        if (!IsPresent || IsInvulnerable)
        {
            return PlayerHitResult.Ignored;
        }

        if (ShieldStrength > 0)
        {
            ShieldStrength--;
            InvulnerableTime = ShieldInvulnerability;
            events.Add(new GameEvent(GameEventKind.ShieldAbsorbed, X, LineY, value: ShieldStrength));
            return PlayerHitResult.Absorbed;
        }

        Hull--;
        if (Hull > 0)
        {
            InvulnerableTime = HitInvulnerability;
            events.Add(new GameEvent(GameEventKind.PlayerHit, X, LineY, value: Hull));
            return PlayerHitResult.Damaged;
        }

        Destroy(events);
        return PlayerHitResult.Destroyed;
    }

    /// <summary>Destroys the ship regardless of shield and invulnerability (the formation landed).</summary>
    /// <param name="events">Receives PlayerDestroyed.</param>
    /// <returns>True when the ship was present and is now destroyed.</returns>
    internal bool Destroy(GameEvents events)
    {
        if (!IsPresent)
        {
            return false;
        }

        IsPresent = false;
        Hull = 0;
        ShieldStrength = 0;
        InvulnerableTime = 0;
        Lives = Math.Max(0, Lives - 1);
        RespawnTimer = RespawnDelay;
        events.Add(new GameEvent(GameEventKind.PlayerDestroyed, X, LineY, value: Lives));
        return true;
    }
}
