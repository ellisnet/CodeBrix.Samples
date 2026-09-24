namespace BrixInvaders.GameLogic;

/// <summary>Everything the simulation reports to the Game library (sprites, sounds, particles, HUD flashes).</summary>
public enum GameEventKind
{
    /// <summary>A wave's formation appeared. Value = wave number.</summary>
    WaveStarted,

    /// <summary>The last enemy of a wave died. Value = wave number.</summary>
    WaveCleared,

    /// <summary>The sector's boss is gone and the sector bonus is paid. Value = sector number.</summary>
    SectorCleared,

    /// <summary>Bonus points were awarded outside a kill (sector bonus, surplus power-ups). Points = amount.</summary>
    BonusAwarded,

    /// <summary>The player fired. Value = number of bolts in the volley. X/Y = ship position.</summary>
    PlayerFired,

    /// <summary>An enemy (or boss part) fired a bolt. X/Y = muzzle.</summary>
    EnemyFired,

    /// <summary>A homing missile was launched. X/Y = launch point.</summary>
    MissileLaunched,

    /// <summary>The player used a bomb. X/Y = ship position.</summary>
    BombDetonated,

    /// <summary>A shielded enemy lost its shield. X/Y, Role, Colour describe it.</summary>
    EnemyShieldBroken,

    /// <summary>An enemy was destroyed. X/Y, Role, Colour, Points (awarded, all multipliers applied).</summary>
    EnemyDestroyed,

    /// <summary>A diver left the formation. X/Y = start of the dive.</summary>
    DiverLaunched,

    /// <summary>The formation took a sideways step. Value = march note 0..3 (cycles).</summary>
    FormationStepped,

    /// <summary>The formation hit an edge and dropped a row.</summary>
    FormationDropped,

    /// <summary>The formation reached the player's line: a life is lost and the formation is pushed back up.</summary>
    FormationLanded,

    /// <summary>The player's hull took a hit. Value = hull remaining (1 or 2).</summary>
    PlayerHit,

    /// <summary>The shield bubble absorbed a hit. Value = shield strength remaining.</summary>
    ShieldAbsorbed,

    /// <summary>The player's ship was destroyed. Value = lives remaining. X/Y = ship position.</summary>
    PlayerDestroyed,

    /// <summary>The player's ship respawned (invulnerable for a while). X/Y = ship position.</summary>
    PlayerRespawned,

    /// <summary>No lives left. Points = final score.</summary>
    GameOver,

    /// <summary>A power-up dropped. PowerUp, X/Y.</summary>
    PowerUpDropped,

    /// <summary>The player collected a power-up. PowerUp, X/Y.</summary>
    PowerUpCollected,

    /// <summary>An active timed power-up ran out. PowerUp.</summary>
    PowerUpExpired,

    /// <summary>A dropped power-up timed out or fell off the bottom uncollected. PowerUp, X/Y.</summary>
    PowerUpLost,

    /// <summary>The UFO appeared. Value = direction (+1 right, -1 left).</summary>
    UfoAppeared,

    /// <summary>The UFO was shot. X/Y, Points.</summary>
    UfoDestroyed,

    /// <summary>The UFO left the playfield.</summary>
    UfoEscaped,

    /// <summary>A homing missile was shot down. X/Y, Points.</summary>
    MissileDestroyed,

    /// <summary>A homing missile ran out of fuel. X/Y.</summary>
    MissileExpired,

    /// <summary>A meteor shower began between waves.</summary>
    MeteorShowerStarted,

    /// <summary>The meteor shower ended.</summary>
    MeteorShowerEnded,

    /// <summary>A big meteor was hit but not destroyed. X/Y.</summary>
    MeteorHit,

    /// <summary>A meteor was destroyed. X/Y, Points, Value = 1 big / 0 small.</summary>
    MeteorDestroyed,

    /// <summary>A player shot missed and the chain reset. Value = the chain length that was lost.</summary>
    ChainBroken,

    /// <summary>The chain multiplier changed. Value = new multiplier (1..5).</summary>
    ChainMultiplierChanged,

    /// <summary>The boss warning starts. Value = sector number.</summary>
    BossIncoming,

    /// <summary>The boss entered the playfield. X/Y.</summary>
    BossAppeared,

    /// <summary>A boss part was hit. X/Y = the part. Value = 1 when the hit was absorbed by armour, else 0.</summary>
    BossHit,

    /// <summary>The boss switched attack phase. Value = new phase (2 or 3).</summary>
    BossPhaseChanged,

    /// <summary>A boss section was destroyed. Section, Value = section index, X/Y, Points.</summary>
    BossSectionDestroyed,

    /// <summary>The boss was defeated. X/Y, Points.</summary>
    BossDefeated,
}
