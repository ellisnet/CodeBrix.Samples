using System;
using System.Collections.Generic;

namespace BrixInvaders.Assets;

/// <summary>Maps each <see cref="SoundEffect"/> to its <see cref="AssetKeys.Sfx"/> key.</summary>
public static class SoundEffects
{
    private static readonly Dictionary<SoundEffect, string> _keys = new()
    {
        [SoundEffect.PlayerLaser] = AssetKeys.Sfx.PlayerLaser,
        [SoundEffect.EnemyLaser] = AssetKeys.Sfx.EnemyLaser,
        [SoundEffect.BossLaser] = AssetKeys.Sfx.BossLaser,
        [SoundEffect.MissileLaunch] = AssetKeys.Sfx.MissileLaunch,
        [SoundEffect.ExplosionSmall] = AssetKeys.Sfx.ExplosionSmall,
        [SoundEffect.ExplosionMedium] = AssetKeys.Sfx.ExplosionMedium,
        [SoundEffect.ExplosionLarge] = AssetKeys.Sfx.ExplosionLarge,
        [SoundEffect.Ufo] = AssetKeys.Sfx.Ufo,
        [SoundEffect.PowerUpPickup] = AssetKeys.Sfx.PowerUpPickup,
        [SoundEffect.PowerUpLost] = AssetKeys.Sfx.PowerUpLost,
        [SoundEffect.ShieldUp] = AssetKeys.Sfx.ShieldUp,
        [SoundEffect.ShieldHit] = AssetKeys.Sfx.ShieldHit,
        [SoundEffect.PlayerHit] = AssetKeys.Sfx.PlayerHit,
        [SoundEffect.EnemyShieldBroken] = AssetKeys.Sfx.EnemyShieldBroken,
        [SoundEffect.ArmourHit] = AssetKeys.Sfx.ArmourHit,
        [SoundEffect.BossHit] = AssetKeys.Sfx.BossHit,
        [SoundEffect.BossSectionDestroyed] = AssetKeys.Sfx.BossSectionDestroyed,
        [SoundEffect.BossPhase] = AssetKeys.Sfx.BossPhase,
        [SoundEffect.BossExplosion] = AssetKeys.Sfx.BossExplosion,
        [SoundEffect.BossWarning] = AssetKeys.Sfx.BossWarning,
        [SoundEffect.Bomb] = AssetKeys.Sfx.Bomb,
        [SoundEffect.ExtraLife] = AssetKeys.Sfx.ExtraLife,
        [SoundEffect.ChainUp] = AssetKeys.Sfx.ChainUp,
        [SoundEffect.ChainBroken] = AssetKeys.Sfx.ChainBroken,
        [SoundEffect.WaveStart] = AssetKeys.Sfx.WaveStart,
        [SoundEffect.SectorClear] = AssetKeys.Sfx.SectorClear,
        [SoundEffect.FormationLanded] = AssetKeys.Sfx.FormationLanded,
        [SoundEffect.March0] = AssetKeys.Sfx.March0,
        [SoundEffect.March1] = AssetKeys.Sfx.March1,
        [SoundEffect.March2] = AssetKeys.Sfx.March2,
        [SoundEffect.March3] = AssetKeys.Sfx.March3,
        [SoundEffect.MenuMove] = AssetKeys.Sfx.MenuMove,
        [SoundEffect.MenuConfirm] = AssetKeys.Sfx.MenuConfirm,
        [SoundEffect.MenuBack] = AssetKeys.Sfx.MenuBack,
        [SoundEffect.GameOver] = AssetKeys.Sfx.GameOver,
        [SoundEffect.HighScore] = AssetKeys.Sfx.HighScore,
    };

    /// <summary>Gets every sound effect with its key.</summary>
    public static IReadOnlyDictionary<SoundEffect, string> Keys => _keys;

    /// <summary>
    /// Gets the key of a sound effect. It is both the Kenney asset key and the engine audio key the sound is
    /// registered under (<c>AudioResourceManager</c>) once <see cref="BrixInvadersAssets.LoadSounds"/> has run.
    /// </summary>
    /// <param name="effect">The sound effect.</param>
    /// <returns>The key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for a value that is not a defined sound effect.</exception>
    public static string KeyOf(SoundEffect effect) =>
        _keys.TryGetValue(effect, out string key)
            ? key
            : throw new ArgumentOutOfRangeException(nameof(effect), effect, "Not a defined sound effect.");
}
