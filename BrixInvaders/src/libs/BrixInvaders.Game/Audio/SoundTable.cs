using System.Collections.Generic;
using BrixInvaders.Assets;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Audio;

/// <summary>
/// THE table from game events (and menu actions) to sound effects - the one place that decides what the game
/// sounds like. Every <see cref="GameEventKind"/> is either mapped here or listed in <see cref="SilentKinds"/>.
/// </summary>
public static class SoundTable
{
    /// <summary>
    /// The events that make no sound here (they are shown, not heard, another event sounds, or the music director
    /// plays them as stingers).
    /// </summary>
    public static readonly IReadOnlyCollection<GameEventKind> SilentKinds = new HashSet<GameEventKind>
    {
        GameEventKind.WaveCleared,
        GameEventKind.BonusAwarded,
        GameEventKind.DiverLaunched,
        GameEventKind.FormationDropped,
        GameEventKind.PlayerRespawned,
        GameEventKind.PowerUpDropped,
        GameEventKind.PowerUpExpired,
        GameEventKind.UfoEscaped,
        GameEventKind.MissileExpired,
        GameEventKind.MeteorShowerStarted,
        GameEventKind.MeteorShowerEnded,
        GameEventKind.BossAppeared,

        //The music director plays these two as stingers on the effects bus, with the music ducked under them
        GameEventKind.BossIncoming,
        GameEventKind.GameOver,
    };

    /// <summary>The cue a menu cursor move plays.</summary>
    public static SoundCue MenuMove => new SoundCue(SoundEffect.MenuMove, 0.6f, 5);

    /// <summary>The cue a menu confirm plays.</summary>
    public static SoundCue MenuConfirm => new SoundCue(SoundEffect.MenuConfirm, 0.7f, 5);

    /// <summary>The cue a menu back plays.</summary>
    public static SoundCue MenuBack => new SoundCue(SoundEffect.MenuBack, 0.7f, 5);

    /// <summary>The cue a new high score plays.</summary>
    public static SoundCue HighScore => new SoundCue(SoundEffect.HighScore, 0.9f, 9);

    /// <summary>Finds the sound an event makes.</summary>
    /// <param name="gameEvent">The event.</param>
    /// <param name="cue">The cue, when the event sounds.</param>
    /// <returns>Whether the event makes a sound.</returns>
    public static bool TryGetCue(GameEvent gameEvent, out SoundCue cue)
    {
        cue = default;
        switch (gameEvent.Kind)
        {
            case GameEventKind.WaveStarted:
                cue = new SoundCue(SoundEffect.WaveStart, 0.7f, 6);
                return true;
            case GameEventKind.SectorCleared:
                cue = new SoundCue(SoundEffect.SectorClear, 0.9f, 9);
                return true;
            case GameEventKind.PlayerFired:
                cue = new SoundCue(SoundEffect.PlayerLaser, 0.45f, 2);
                return true;
            case GameEventKind.EnemyFired:
                cue = gameEvent.Value > 1
                    ? new SoundCue(SoundEffect.BossLaser, 0.55f, 3)
                    : new SoundCue(SoundEffect.EnemyLaser, 0.35f, 1);
                return true;
            case GameEventKind.MissileLaunched:
                cue = new SoundCue(SoundEffect.MissileLaunch, 0.5f, 3);
                return true;
            case GameEventKind.BombDetonated:
                cue = new SoundCue(SoundEffect.Bomb, 1f, 8);
                return true;
            case GameEventKind.EnemyShieldBroken:
                cue = new SoundCue(SoundEffect.EnemyShieldBroken, 0.6f, 3);
                return true;
            case GameEventKind.EnemyDestroyed:
                cue = new SoundCue(SoundEffect.ExplosionSmall, 0.55f, 3);
                return true;
            case GameEventKind.FormationStepped:
                cue = new SoundCue(MarchNote(gameEvent.Value), 0.5f, 1);
                return true;
            case GameEventKind.FormationLanded:
                cue = new SoundCue(SoundEffect.FormationLanded, 1f, 9);
                return true;
            case GameEventKind.PlayerHit:
                cue = new SoundCue(SoundEffect.PlayerHit, 0.9f, 8);
                return true;
            case GameEventKind.ShieldAbsorbed:
                cue = new SoundCue(SoundEffect.ShieldHit, 0.8f, 7);
                return true;
            case GameEventKind.PlayerDestroyed:
                cue = new SoundCue(SoundEffect.ExplosionLarge, 1f, 9);
                return true;
            case GameEventKind.PowerUpCollected:
                cue = gameEvent.PowerUp switch
                {
                    PowerUpKind.ShieldBubble => new SoundCue(SoundEffect.ShieldUp, 0.8f, 6),
                    PowerUpKind.ExtraLife => new SoundCue(SoundEffect.ExtraLife, 0.9f, 7),
                    _ => new SoundCue(SoundEffect.PowerUpPickup, 0.8f, 6),
                };
                return true;
            case GameEventKind.PowerUpLost:
                cue = new SoundCue(SoundEffect.PowerUpLost, 0.4f, 2);
                return true;
            case GameEventKind.UfoAppeared:
                cue = new SoundCue(SoundEffect.Ufo, 0.5f, 4);
                return true;
            case GameEventKind.UfoDestroyed:
                cue = new SoundCue(SoundEffect.ExplosionMedium, 0.8f, 6);
                return true;
            case GameEventKind.MissileDestroyed:
                cue = new SoundCue(SoundEffect.ExplosionSmall, 0.45f, 3);
                return true;
            case GameEventKind.MeteorHit:
                cue = new SoundCue(SoundEffect.ArmourHit, 0.4f, 2);
                return true;
            case GameEventKind.MeteorDestroyed:
                cue = new SoundCue(SoundEffect.ExplosionSmall, 0.5f, 3);
                return true;
            case GameEventKind.ChainBroken:
                cue = new SoundCue(SoundEffect.ChainBroken, 0.5f, 4);
                return gameEvent.Value >= Scoring.ChainStep;
            case GameEventKind.ChainMultiplierChanged:
                //Fires on every change; only a rise past x1 is worth a sound (a break is ChainBroken's job)
                cue = new SoundCue(SoundEffect.ChainUp, 0.7f, 5);
                return gameEvent.Value >= 2;
            case GameEventKind.BossHit:
                cue = gameEvent.Value == 1
                    ? new SoundCue(SoundEffect.ArmourHit, 0.45f, 3)
                    : new SoundCue(SoundEffect.BossHit, 0.5f, 3);
                return true;
            case GameEventKind.BossPhaseChanged:
                cue = new SoundCue(SoundEffect.BossPhase, 0.9f, 8);
                return true;
            case GameEventKind.BossSectionDestroyed:
                cue = new SoundCue(SoundEffect.BossSectionDestroyed, 0.9f, 8);
                return true;
            case GameEventKind.BossDefeated:
                cue = new SoundCue(SoundEffect.BossExplosion, 1f, 10);
                return true;
            default:
                return false;
        }
    }

    /// <summary>The note of the classic four-note march for a <see cref="GameEventKind.FormationStepped"/> value.</summary>
    /// <param name="note">The march note, 0..3 (wraps).</param>
    /// <returns>The effect.</returns>
    public static SoundEffect MarchNote(int note) => (((note % 4) + 4) % 4) switch
    {
        0 => SoundEffect.March0,
        1 => SoundEffect.March1,
        2 => SoundEffect.March2,
        _ => SoundEffect.March3,
    };
}
