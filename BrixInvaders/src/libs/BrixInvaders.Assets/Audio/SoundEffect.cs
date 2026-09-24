namespace BrixInvaders.Assets;

/// <summary>Every sound effect the game plays. <see cref="SoundEffects.KeyOf"/> gives its asset (and engine audio) key.</summary>
public enum SoundEffect
{
    /// <summary>The player fires.</summary>
    PlayerLaser = 0,

    /// <summary>An enemy fires a bolt.</summary>
    EnemyLaser = 1,

    /// <summary>A boss section fires.</summary>
    BossLaser = 2,

    /// <summary>A homing missile is launched.</summary>
    MissileLaunch = 3,

    /// <summary>A formation enemy, missile or small meteor is destroyed.</summary>
    ExplosionSmall = 4,

    /// <summary>A big meteor or the UFO is destroyed.</summary>
    ExplosionMedium = 5,

    /// <summary>The player's ship is destroyed.</summary>
    ExplosionLarge = 6,

    /// <summary>The UFO flies across the lane (loop it while it is on screen).</summary>
    Ufo = 7,

    /// <summary>A power-up is collected.</summary>
    PowerUpPickup = 8,

    /// <summary>A power-up drop falls off the screen or times out.</summary>
    PowerUpLost = 9,

    /// <summary>The player's shield gains strength.</summary>
    ShieldUp = 10,

    /// <summary>The player's shield absorbs a hit.</summary>
    ShieldHit = 11,

    /// <summary>The player's hull is hit.</summary>
    PlayerHit = 12,

    /// <summary>A shielded enemy loses its shield.</summary>
    EnemyShieldBroken = 13,

    /// <summary>A bolt hits boss armour.</summary>
    ArmourHit = 14,

    /// <summary>A boss section takes damage.</summary>
    BossHit = 15,

    /// <summary>A boss section is destroyed.</summary>
    BossSectionDestroyed = 16,

    /// <summary>The boss enters its next phase.</summary>
    BossPhase = 17,

    /// <summary>The boss is defeated.</summary>
    BossExplosion = 18,

    /// <summary>The boss warning stinger.</summary>
    BossWarning = 19,

    /// <summary>The player detonates a bomb.</summary>
    Bomb = 20,

    /// <summary>An extra life is awarded.</summary>
    ExtraLife = 21,

    /// <summary>The chain multiplier goes up.</summary>
    ChainUp = 22,

    /// <summary>The chain is broken.</summary>
    ChainBroken = 23,

    /// <summary>A wave starts.</summary>
    WaveStart = 24,

    /// <summary>A sector is cleared.</summary>
    SectorClear = 25,

    /// <summary>The formation reaches the player's line.</summary>
    FormationLanded = 26,

    /// <summary>Formation march note 0 (the classic four-note march).</summary>
    March0 = 27,

    /// <summary>Formation march note 1.</summary>
    March1 = 28,

    /// <summary>Formation march note 2.</summary>
    March2 = 29,

    /// <summary>Formation march note 3.</summary>
    March3 = 30,

    /// <summary>The menu cursor moves.</summary>
    MenuMove = 31,

    /// <summary>A menu item is chosen.</summary>
    MenuConfirm = 32,

    /// <summary>Back out of a menu.</summary>
    MenuBack = 33,

    /// <summary>Game over.</summary>
    GameOver = 34,

    /// <summary>A new high score is entered.</summary>
    HighScore = 35,
}
