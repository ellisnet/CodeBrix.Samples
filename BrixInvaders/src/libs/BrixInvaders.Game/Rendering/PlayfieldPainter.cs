using System;
using System.Collections.Generic;
using BrixInvaders.Assets;
using BrixInvaders.Game.Hosting;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Rendering;

/// <summary>
/// Draws a running <see cref="GameSimulation"/> into the world list: space backdrop, planet, parallax stars, the
/// formation, bosses, the UFO, bolts, missiles, meteors, power-up drops and the player's ship with its thrust,
/// upgrade parts (<see cref="ShipLoadout"/>), damage overlays and shield bubble. Short-lived impact flashes come from the game events it is given.
/// </summary>
public sealed class PlayfieldPainter
{
    /// <summary>The player's ship draw box (its hit box is 64 x 48).</summary>
    public const double PlayerDrawWidth = 76;

    /// <summary>The player's ship draw box height.</summary>
    public const double PlayerDrawHeight = 58;

    /// <summary>A formation enemy's draw box (its hit box is 48 x 40).</summary>
    public const double EnemyDrawSize = 56;

    private const double FlashSeconds = 0.18;
    private const int MaxFlashes = 64;

    private readonly StarField _stars = new StarField();
    private readonly List<Flash> _flashes = new List<Flash>();
    private string _loadout;

    /// <summary>Seconds of star scrolling (advances with <see cref="Update"/>).</summary>
    public double ScrollTime { get; private set; }

    /// <summary>The star speed multiplier (the title drifts slowly; play scrolls at full speed).</summary>
    public double StarSpeed { get; set; } = 1.0;

    /// <summary>
    /// The last loadout description drawn (<see cref="ShipLoadout.Describe"/>), or null before the first ship; a change
    /// writes one <c>loadout:</c> log line.
    /// </summary>
    public string Loadout => _loadout;

    /// <summary>Advances the scrolling and the flashes.</summary>
    /// <param name="dt">Seconds.</param>
    public void Update(double dt)
    {
        ScrollTime += dt * StarSpeed;
        for (var i = _flashes.Count - 1; i >= 0; i--)
        {
            var flash = _flashes[i];
            flash.Age += dt;
            if (flash.Age >= FlashSeconds)
            {
                _flashes.RemoveAt(i);
            }
            else
            {
                _flashes[i] = flash;
            }
        }
    }

    /// <summary>Turns hit events into impact flashes.</summary>
    /// <param name="events">The events of the last steps.</param>
    public void AddFlashes(IReadOnlyList<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);
        foreach (var gameEvent in events)
        {
            string image = gameEvent.Kind switch
            {
                GameEventKind.EnemyShieldBroken or GameEventKind.BossHit or GameEventKind.MeteorHit => SpriteCatalog.PlayerImpact,
                GameEventKind.PlayerHit or GameEventKind.ShieldAbsorbed => SpriteCatalog.EnemyImpact,
                _ => null,
            };

            if (image != null && _flashes.Count < MaxFlashes)
            {
                _flashes.Add(new Flash(image, gameEvent.X, gameEvent.Y, 0));
            }
        }
    }

    /// <summary>Forgets every flash (a new game, a new screen).</summary>
    public void ClearFlashes() => _flashes.Clear();

    /// <summary>Draws the space backdrop: a tiled Kenney background, a planet and the parallax stars.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="background">Which tiling background.</param>
    /// <param name="planet">The planet image key, or null for none.</param>
    /// <param name="planetX">Planet centre X.</param>
    /// <param name="planetY">Planet centre Y.</param>
    /// <param name="planetSize">Planet diameter.</param>
    /// <param name="planetAlpha">Planet opacity.</param>
    public void PaintBackdrop(FrameBuilder frame, SpaceBackground background, string planet, double planetX, double planetY,
        double planetSize, double planetAlpha = 1)
    {
        ArgumentNullException.ThrowIfNull(frame);
        const double tile = 256;
        var offset = (ScrollTime * 6) % tile;
        var image = SpriteCatalog.Background(background);
        for (var y = -tile + offset; y < Playfield.Height + tile; y += tile)
        {
            for (double x = 0; x < Playfield.Width; x += tile)
            {
                frame.AddWorld(DrawCommand.Sprite(image, x + (tile / 2), y + (tile / 2), tile + 1, tile + 1));
            }
        }

        if (planet != null)
        {
            frame.AddWorld(DrawCommand.Sprite(planet, planetX, planetY, planetSize, planetSize, ScrollTime * 0.6, planetAlpha));
        }

        _stars.Paint(frame, ScrollTime);
    }

    /// <summary>Draws a whole game: backdrop, every object and the flashes.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="game">The game.</param>
    /// <param name="time">Seconds (for animation).</param>
    public void PaintGame(FrameBuilder frame, GameSimulation game, double time)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(game);

        PaintBackdrop(frame, SpriteCatalog.SectorBackground(game.Sector), SpriteCatalog.SectorPlanet(game.Sector),
            1060, 250, 420, 0.55);
        PaintDrops(frame, game, time);
        PaintMeteors(frame, game);
        PaintBoss(frame, game, time);
        PaintEnemies(frame, game, time);
        PaintUfo(frame, game);
        PaintProjectiles(frame, game);
        PaintPlayer(frame, game, time);
        foreach (var flash in _flashes)
        {
            var size = 34 + (flash.Age / FlashSeconds * 24);
            frame.AddWorld(DrawCommand.Sprite(flash.Image, flash.X, flash.Y, size, size, flash.Age * 400, 1 - (flash.Age / FlashSeconds)));
        }
    }

    /// <summary>Draws a formation enemy (used by the title fleet too).</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="role">The role.</param>
    /// <param name="colour">The colour.</param>
    /// <param name="x">Centre X.</param>
    /// <param name="y">Centre Y.</param>
    /// <param name="size">Draw size.</param>
    /// <param name="rotation">Clockwise degrees.</param>
    /// <param name="alpha">Opacity.</param>
    public static void PaintEnemy(FrameBuilder frame, EnemyRole role, EnemyColour colour, double x, double y, double size,
        double rotation = 0, double alpha = 1)
    {
        ArgumentNullException.ThrowIfNull(frame);
        frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.Enemy(role, colour), x, y, size, size, rotation, alpha));
    }

    /// <summary>Draws a player ship with its thrust flame (used by ship select and the title too).</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="shape">Ship shape.</param>
    /// <param name="colour">Ship colour.</param>
    /// <param name="x">Centre X.</param>
    /// <param name="y">Centre Y.</param>
    /// <param name="scale">Size multiplier on the normal draw box.</param>
    /// <param name="time">Seconds (flame flicker).</param>
    /// <param name="overlay">Whether to draw in the overlay list instead of the world list.</param>
    public static void PaintShip(FrameBuilder frame, int shape, int colour, double x, double y, double scale, double time,
        bool overlay = false)
    {
        ArgumentNullException.ThrowIfNull(frame);
        var flicker = (int)(time * 20);
        var flame = DrawCommand.Sprite(SpriteCatalog.Thrust(flicker), x, y + (PlayerDrawHeight * 0.55 * scale),
            14 * scale, 32 * scale, 0, 0.9);
        var ship = DrawCommand.Sprite(SpriteCatalog.PlayerShip(shape, colour), x, y, PlayerDrawWidth * scale, PlayerDrawHeight * scale);
        if (overlay)
        {
            frame.AddOverlay(flame);
            frame.AddOverlay(ship);
        }
        else
        {
            frame.AddWorld(flame);
            frame.AddWorld(ship);
        }
    }

    private static void PaintDrops(FrameBuilder frame, GameSimulation game, double time)
    {
        foreach (var drop in game.PowerUps.Drops)
        {
            if (!drop.IsAlive)
            {
                continue;
            }

            var blinking = drop.Age > PowerUpSystem.DropLifetime - 2.0 && ((int)(time * 8) % 2 == 0);
            var bob = Math.Sin((time * 4) + drop.Id) * 3;
            frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.PowerUp(drop.Kind), drop.X, drop.Y + bob, 34, 34, 0, blinking ? 0.35 : 1));
        }
    }

    private static void PaintMeteors(FrameBuilder frame, GameSimulation game)
    {
        foreach (var meteor in game.Meteors.Meteors)
        {
            if (!meteor.IsAlive)
            {
                continue;
            }

            var size = meteor.Size == MeteorSize.Big ? 74 : 38;
            frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.Meteor(meteor.Id, meteor.Size), meteor.X, meteor.Y, size, size,
                meteor.Rotation * 180 / Math.PI));
        }
    }

    private static void PaintBoss(FrameBuilder frame, GameSimulation game, double time)
    {
        var boss = game.Boss;
        if (boss == null || boss.State == BossState.Gone)
        {
            return;
        }

        var dying = boss.State == BossState.Dying;
        var shake = dying ? Math.Sin(time * 60) * 4 : 0;
        foreach (var section in boss.Sections)
        {
            var box = boss.BoxOf(section);
            var x = box.CenterX + shake;
            var y = box.CenterY;
            if (section.IsDestroyed)
            {
                frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.Puff(section.Id + (int)(time * 6)), x, y, box.Width, box.Height, time * 40, 0.55));
                continue;
            }

            var flicker = dying && ((int)(time * 16) % 2 == 0) ? 0.5 : 1.0;
            switch (section.Kind)
            {
                case BossSectionKind.Core:
                    frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.BossSection(section.Kind, boss.Design), x, y,
                        box.Width * 1.35, box.Height * 1.6, SpriteCatalog.BossShipFacing, flicker));
                    if (boss.IsCoreArmoured && !dying)
                    {
                        frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.EnemyShield, x, y, box.Width * 1.25, box.Height * 1.5, 0,
                            0.35 + (0.1 * Math.Sin(time * 5))));
                    }

                    break;
                case BossSectionKind.Turret:
                    frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.BossSection(section.Kind, boss.Design), x, y, box.Width, box.Height, 0, flicker));
                    frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.BossTurretGun, x, y + 14, box.Width * 0.45, box.Height * 0.8,
                        AimDegrees(x, y, game.Player.X, game.Player.Y), flicker));
                    break;
                default:
                    frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.BossSection(section.Kind, boss.Design), x, y,
                        box.Width * 1.2, box.Height * 1.2, SpriteCatalog.BossShipFacing, flicker));
                    break;
            }
        }
    }

    private static void PaintEnemies(FrameBuilder frame, GameSimulation game, double time)
    {
        foreach (var enemy in game.Enemies)
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            var tilt = enemy.State == EnemyState.InFormation ? Math.Sin((time * 3) + enemy.Column) * 4 : Math.Sin(time * 10) * 12;
            PaintEnemy(frame, enemy.Role, enemy.Colour, enemy.X, enemy.Y, EnemyDrawSize, tilt);
            if (enemy.HasShield)
            {
                frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.EnemyShield, enemy.X, enemy.Y, 74, 74, time * 30,
                    0.55 + (0.15 * Math.Sin((time * 6) + enemy.Id))));
            }
        }
    }

    private static void PaintUfo(FrameBuilder frame, GameSimulation game)
    {
        var ufo = game.Ufo;
        if (ufo == null)
        {
            return;
        }

        frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.Ufo(ufo.Id), ufo.X, ufo.Y, 60, 60, ufo.X * 0.8));
    }

    private static void PaintProjectiles(FrameBuilder frame, GameSimulation game)
    {
        foreach (var bolt in game.Projectiles.PlayerBolts)
        {
            if (bolt.IsAlive)
            {
                frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.PlayerBolt(bolt.IsPiercing), bolt.X, bolt.Y, 9, 30, HeadingDegrees(bolt) + 90));
            }
        }

        foreach (var bolt in game.Projectiles.EnemyBolts)
        {
            if (bolt.IsAlive)
            {
                frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.EnemyBolt, bolt.X, bolt.Y, 8, 24, HeadingDegrees(bolt) + 90));
            }
        }

        foreach (var missile in game.Projectiles.Missiles)
        {
            if (missile.IsAlive)
            {
                frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.Missile, missile.X, missile.Y, 18, 36,
                    HeadingDegrees(missile) + SpriteCatalog.MissileArtOffset));
            }
        }
    }

    private void PaintPlayer(FrameBuilder frame, GameSimulation game, double time)
    {
        var player = game.Player;
        if (!player.IsPresent)
        {
            return;
        }

        var shape = game.Setup.ShipShape;
        var colour = game.Setup.ShipColour;
        var alpha = player.IsInvulnerable && ((int)(time * 12) % 2 == 0) ? 0.35 : 1.0;
        if (game.PowerUps.IsActive(PowerUpKind.SpeedBoost))
        {
            frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.SpeedStreak, player.X - 30, player.Y + 26, 10, 40, 0, 0.7 * alpha));
            frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.SpeedStreak, player.X + 30, player.Y + 26, 10, 40, 0, 0.7 * alpha));
        }

        var parts = ShipLoadout.PartsFor(shape, ShipLoadout.ActiveKinds(game.PowerUps));
        NoteLoadout(shape, parts);
        PaintParts(frame, game, parts, ShipPartLayer.UnderHull, shape, alpha, time);

        var flicker = (int)(time * 20);
        frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.Thrust(flicker), player.X, player.Y + 36, 16, 34, 0, 0.9 * alpha));
        frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.PlayerShip(shape, colour), player.X, player.Y, PlayerDrawWidth,
            PlayerDrawHeight, 0, alpha));
        var damage = SpriteCatalog.DamageOverlay(shape, player.DamageTier);
        if (damage != null)
        {
            frame.AddWorld(DrawCommand.Sprite(damage, player.X, player.Y, PlayerDrawWidth, PlayerDrawHeight, 0, alpha));
        }

        PaintParts(frame, game, parts, ShipPartLayer.OverHull, shape, alpha, time);

        if (player.ShieldStrength > 0)
        {
            frame.AddWorld(DrawCommand.Sprite(SpriteCatalog.PlayerShield(player.ShieldStrength), player.X, player.Y - 4, 118, 104, 0,
                0.75 + (0.15 * Math.Sin(time * 6))));
        }
    }

    private static void PaintParts(FrameBuilder frame, GameSimulation game, IReadOnlyList<ShipPart> parts, ShipPartLayer layer,
        int shape, double alpha, double time)
    {
        var scale = ShipLoadout.HullScale(shape);
        var player = game.Player;
        var blinkOff = (int)(time * 8) % 2 == 0;
        for (var i = 0; i < parts.Count; i++)
        {
            var part = parts[i];
            if (part.Layer != layer)
            {
                continue;
            }

            var partAlpha = alpha;
            if (blinkOff && game.PowerUps.TimeLeft(part.PowerUp) < ShipLoadout.ExpiryBlinkSeconds)
            {
                partAlpha *= 0.35;
            }

            var image = part.Frames[0];
            switch (part.Effect)
            {
                case ShipPartEffect.Flicker:
                    image = part.Frames[((int)(time * 20) + i) % part.Frames.Count];
                    break;
                case ShipPartEffect.Pulse:
                    partAlpha *= 0.7 + (0.3 * Math.Sin(time * 10));
                    break;
            }

            frame.AddWorld(DrawCommand.Sprite(image, player.X + (part.X * scale), player.Y + (part.Y * scale), part.Width * scale,
                part.Height * scale, part.Rotation, partAlpha));
        }
    }

    private void NoteLoadout(int shape, IReadOnlyList<ShipPart> parts)
    {
        var description = ShipLoadout.Describe(shape, parts);
        if (description != _loadout)
        {
            _loadout = description;
            GameLog.Write($"loadout: {description}");
        }
    }

    private static double HeadingDegrees(Projectile projectile) => projectile.Heading * 180 / Math.PI;

    private static double AimDegrees(double fromX, double fromY, double toX, double toY) =>
        (Math.Atan2(toY - fromY, toX - fromX) * 180 / Math.PI) + 90;

    private struct Flash
    {
        public Flash(string image, double x, double y, double age)
        {
            Image = image;
            X = x;
            Y = y;
            Age = age;
        }

        public string Image { get; }

        public double X { get; }

        public double Y { get; }

        public double Age { get; set; }
    }
}
