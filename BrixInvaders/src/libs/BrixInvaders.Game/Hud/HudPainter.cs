using System;
using System.Globalization;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;
using SkiaSharp;

namespace BrixInvaders.Game.Hud;

/// <summary>
/// The in-play heads-up display (overlay list): score, chain and multiplier, lives, sector and wave, bombs, active
/// power-up timers, the boss health bar, and the stage banners ("WAVE 3", "BOSS INCOMING", "SECTOR CLEAR").
/// </summary>
public static class HudPainter
{
    /// <summary>The HUD band height (DESIGN.md: nothing spawns in it).</summary>
    public const double BandHeight = Playfield.HudHeight;

    /// <summary>Formats a score with thousands separators.</summary>
    /// <param name="score">The score.</param>
    /// <returns>The text.</returns>
    public static string FormatScore(long score) => score.ToString("N0", CultureInfo.InvariantCulture);

    /// <summary>The chain readout ("CHAIN 23  x3").</summary>
    /// <param name="chain">The chain count.</param>
    /// <param name="multiplier">The multiplier.</param>
    /// <returns>The text.</returns>
    public static string FormatChain(int chain, int multiplier) => $"CHAIN {chain}  x{multiplier}";

    /// <summary>The short HUD name of a timed power-up.</summary>
    /// <param name="kind">The power-up.</param>
    /// <returns>The name.</returns>
    public static string PowerUpName(PowerUpKind kind) => kind switch
    {
        PowerUpKind.SpreadShot => "SPREAD",
        PowerUpKind.RapidFire => "RAPID",
        PowerUpKind.PiercingLaser => "PIERCE",
        PowerUpKind.ShieldBubble => "SHIELD",
        PowerUpKind.SpeedBoost => "SPEED",
        PowerUpKind.ExtraLife => "LIFE",
        _ => "BOMB",
    };

    /// <summary>The banner text for the current stage, or null for none.</summary>
    /// <param name="game">The game.</param>
    /// <returns>The banner, or null.</returns>
    public static string BannerFor(GameSimulation game)
    {
        ArgumentNullException.ThrowIfNull(game);
        return game.Phase switch
        {
            StagePhase.WaveIntro => $"WAVE {game.Wave}",
            StagePhase.WaveIntermission when game.Meteors.IsShowerActive => "METEOR SHOWER",
            StagePhase.BossWarning => "WARNING - BOSS INCOMING",
            StagePhase.SectorComplete => "SECTOR CLEAR",
            _ => null,
        };
    }

    /// <summary>Draws the HUD for a game.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="game">The game.</param>
    /// <param name="time">Seconds (for pulsing).</param>
    public static void Paint(FrameLists frame, GameSimulation game, double time)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(game);

        frame.Overlay.Rectangle(Playfield.Width / 2, BandHeight / 2, Playfield.Width, BandHeight, 0xA0040A18);
        frame.Overlay.Rectangle(Playfield.Width / 2, BandHeight, Playfield.Width, 2, 0xFF1E3A5F);

        //Score and chain, left
        frame.Overlay.Text("SCORE", 24, 16, frame.Font, 13, Palette.Dim, SKTextAlign.Left);
        frame.Overlay.Text(FormatScore(game.Score), 24, 34, frame.Font, 22, Palette.Gold, SKTextAlign.Left);
        var multiplier = game.Scoring.ChainMultiplier;
        frame.Overlay.Text(FormatChain(game.Scoring.ChainCount, multiplier), 250, 26, frame.Font, 16,
            multiplier > 1 ? Palette.Accent : Palette.Dim, SKTextAlign.Left);

        //Sector and wave, centre
        var stage = game.Phase is StagePhase.BossWarning or StagePhase.BossFight
            ? "BOSS"
            : $"WAVE {Math.Min(game.Wave, SectorRules.WavesPerSector)}/{SectorRules.WavesPerSector}";
        frame.Overlay.Text($"SECTOR {game.Sector} - {SectorRules.NameOf(game.Sector).ToUpperInvariant()}", Playfield.Width / 2,
            16, frame.ThinFont, 14, Palette.Text);
        frame.Overlay.Text(stage, Playfield.Width / 2, 34, frame.Font, 16, Palette.Accent);

        //Lives and bombs, right
        var lifeIcon = SpriteCatalog.LifeIcon(game.Setup.ShipShape, game.Setup.ShipColour);
        var lives = Math.Max(0, game.Player.Lives);
        var shown = Math.Min(lives, 5);
        for (var i = 0; i < shown; i++)
        {
            frame.Overlay.Image(lifeIcon, null, Playfield.Width - 36 - (i * 30), 16, 26, 20);
        }

        if (lives > shown)
        {
            frame.Overlay.Text($"x{lives}", Playfield.Width - 36 - (shown * 30), 17, frame.Font, 14, Palette.Text,
                SKTextAlign.Right);
        }

        for (var i = 0; i < game.Player.Bombs; i++)
        {
            frame.Overlay.Image(SpriteCatalog.BombIcon, null, Playfield.Width - 36 - (i * 28), 36, 20, 20);
        }

        frame.Overlay.Text("BOMBS", Playfield.Width - 36 - (Math.Max(game.Player.Bombs, 1) * 28), 37, frame.Font, 11,
            Palette.Dim, SKTextAlign.Right);

        PaintPowerUps(frame, game);
        PaintBossBar(frame, game, time);
        PaintBanner(frame, game, time);
    }

    private static void PaintPowerUps(FrameLists frame, GameSimulation game)
    {
        var x = 24.0;
        const double y = 700;
        foreach (var timer in game.PowerUps.ActiveTimers())
        {
            frame.Overlay.Image(SpriteCatalog.PowerUp(timer.Key), null, x + 12, y - 2, 22, 22);
            frame.Overlay.Text($"{PowerUpName(timer.Key)} {Math.Ceiling(timer.Value):0}", x + 28, y, frame.Font, 13,
                timer.Value < 3 ? Palette.Danger : Palette.Text, SKTextAlign.Left);
            x += 128;
        }

        if (game.Player.ShieldStrength > 0)
        {
            frame.Overlay.Image(SpriteCatalog.PowerUp(PowerUpKind.ShieldBubble), null, x + 12, y - 2, 22, 22);
            frame.Overlay.Text($"SHIELD {game.Player.ShieldStrength}", x + 28, y, frame.Font, 13, Palette.Accent,
                SKTextAlign.Left);
        }
    }

    private static void PaintBossBar(FrameLists frame, GameSimulation game, double time)
    {
        var boss = game.Boss;
        if (boss == null || boss.State is BossState.Gone or BossState.Dying)
        {
            return;
        }

        const double width = 520;
        const double y = 64;
        var fraction = Math.Clamp(boss.HealthFraction, 0, 1);
        var fill = fraction > 0.5 ? 0xFF46D27A : fraction > 0.25 ? 0xFFF0BE3C : 0xFFEB463C;
        frame.Overlay.Rectangle(Playfield.Width / 2, y, width + 6, 16, 0xDC141820, 0xFFEBF1F7, 1, 3);
        frame.Overlay.Rectangle((Playfield.Width / 2) - (width / 2) + (width * fraction / 2), y, width * fraction, 10, fill, 0,
            0, 2, boss.IsCoreArmoured ? 0.75 + (0.25 * Math.Sin(time * 6)) : 1);
        frame.Overlay.Text($"BOSS - PHASE {boss.Phase}/{boss.PhaseCount}", Playfield.Width / 2, y + 18, frame.Font, 11,
            Palette.Dim);
    }

    private static void PaintBanner(FrameLists frame, GameSimulation game, double time)
    {
        var banner = BannerFor(game);
        if (banner == null)
        {
            return;
        }

        var warning = game.Phase == StagePhase.BossWarning;
        var alpha = warning ? 0.55 + (0.45 * Math.Abs(Math.Sin(time * 5))) : 1;
        frame.Overlay.Text(banner, Playfield.Width / 2, 330, frame.Font, 46, warning ? Palette.Danger : Palette.Accent,
            alpha: alpha);
        if (game.Phase == StagePhase.WaveIntro && game.Wave == 1)
        {
            frame.Overlay.Text(SectorRules.NameOf(game.Sector).ToUpperInvariant(), Playfield.Width / 2, 380, frame.ThinFont, 20,
                Palette.Text);
        }
    }
}
