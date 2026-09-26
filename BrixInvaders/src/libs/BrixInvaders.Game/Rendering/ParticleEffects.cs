using System;
using System.Collections.Generic;
using System.Drawing;
using BrixInvaders.GameLogic;
using CodeBrix.Platform.GameEngine.Drawing.Direct.Particles;
using SkiaSharp;

namespace BrixInvaders.Game.Rendering;

/// <summary>Turns game events into bursts on the engine's <see cref="ParticleSurface"/> (explosions, sparks, pickups).</summary>
public sealed class ParticleEffects
{
    private readonly ParticleSurface _surface;

    /// <summary>Creates the effects over a particle surface.</summary>
    /// <param name="surface">The surface (on the world layer).</param>
    public ParticleEffects(ParticleSurface surface)
    {
        _surface = surface ?? throw new ArgumentNullException(nameof(surface));
    }

    /// <summary>The colour of an enemy's explosion (its tier colour).</summary>
    /// <param name="colour">The enemy colour.</param>
    /// <returns>ARGB.</returns>
    public static uint ExplosionColour(EnemyColour colour) => colour switch
    {
        EnemyColour.Red => 0xFFFF6A4D,
        EnemyColour.Green => 0xFF8CFF6A,
        EnemyColour.Blue => 0xFF6AC8FF,
        _ => 0xFFC8C8D8,
    };

    /// <summary>Bursts for every event that explodes or sparkles.</summary>
    /// <param name="events">The events (playfield pixels, which are layer pixels at the pinned render resolution).</param>
    public void Emit(IReadOnlyList<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        foreach (var gameEvent in events)
        {
            var x = (float)gameEvent.X;
            var y = (float)gameEvent.Y;
            switch (gameEvent.Kind)
            {
                case GameEventKind.EnemyDestroyed:
                    Burst(x, y, 26, 220, (2, 5), (0.25f, 0.6f), ExplosionColour(gameEvent.Colour));
                    Burst(x, y, 10, 90, (4, 8), (0.2f, 0.4f), 0xFFFFE9A8);
                    break;
                case GameEventKind.UfoDestroyed:
                case GameEventKind.BossSectionDestroyed:
                    Burst(x, y, 60, 320, (3, 8), (0.4f, 0.9f), 0xFFFFB347);
                    Burst(x, y, 20, 140, (6, 12), (0.3f, 0.6f), 0xFFFFF2C0);
                    break;
                case GameEventKind.PlayerDestroyed:
                case GameEventKind.BossDefeated:
                    Burst(x, y, 140, 460, (4, 10), (0.6f, 1.3f), 0xFFFF6622);
                    Burst(x, y, 60, 260, (6, 14), (0.7f, 1.5f), 0xFFFFD670);
                    break;
                case GameEventKind.MeteorDestroyed:
                    Burst(x, y, gameEvent.Value == 1 ? 34 : 18, 200, (2, 6), (0.3f, 0.7f), 0xFFB08A64);
                    break;
                case GameEventKind.MissileDestroyed:
                case GameEventKind.EnemyShieldBroken:
                    Burst(x, y, 16, 180, (2, 4), (0.2f, 0.45f), 0xFF9BE7FF);
                    break;
                case GameEventKind.PowerUpCollected:
                    Burst(x, y, 24, 150, (2, 4), (0.3f, 0.6f), 0xFF7CFF8B);
                    break;
                case GameEventKind.PlayerHit:
                case GameEventKind.ShieldAbsorbed:
                    Burst(x, y, 18, 200, (2, 5), (0.2f, 0.5f), 0xFFFF5C5C);
                    break;
                case GameEventKind.BombDetonated:
                    for (var i = 0; i < 8; i++)
                    {
                        Burst(80 + (i * 160), 300, 30, 380, (3, 9), (0.4f, 0.9f), 0xFFFFFFFF);
                    }

                    break;
            }
        }
    }

    private void Burst(float x, float y, int count, float speed, (float Min, float Max) size, (float Min, float Max) life,
        uint colour)
    {
        _surface.Burst(new ParticleEmitter
        {
            Position = new PointF(x, y),
            EmitRate = 0f,
            LifeRange = life,
            VelocityRangeX = (-speed, speed),
            VelocityRangeY = (-speed, speed),
            SizeRange = size,
            Color = new SKColor(colour),
            GravityX = 0f,
            GravityY = 60f,
            JitterX = 6,
            JitterY = 6,
            SpawnDistribution = ParticleSpawnDistribution.Gaussian,
        }, count);
    }
}
