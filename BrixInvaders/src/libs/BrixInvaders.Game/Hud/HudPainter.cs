using System;
using System.Globalization;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;

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
    public static void Paint(FrameBuilder frame, GameSimulation game, double time)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(game);

        frame.AddOverlay(DrawCommand.Rect(Playfield.Width / 2, BandHeight / 2, Playfield.Width, BandHeight, 0xA0040A18));
        frame.AddOverlay(DrawCommand.Rect(Playfield.Width / 2, BandHeight, Playfield.Width, 2, 0xFF1E3A5F));

        //Score and chain, left
        frame.AddOverlay(DrawCommand.Label("SCORE", 24, 16, 13, Palette.Dim, TextAnchor.Left));
        frame.AddOverlay(DrawCommand.Label(FormatScore(game.Score), 24, 34, 22, Palette.Gold, TextAnchor.Left));
        var multiplier = game.Scoring.ChainMultiplier;
        frame.AddOverlay(DrawCommand.Label(FormatChain(game.Scoring.ChainCount, multiplier), 250, 26, 16,
            multiplier > 1 ? Palette.Accent : Palette.Dim, TextAnchor.Left));

        //Sector and wave, centre
        var stage = game.Phase is StagePhase.BossWarning or StagePhase.BossFight
            ? "BOSS"
            : $"WAVE {Math.Min(game.Wave, SectorRules.WavesPerSector)}/{SectorRules.WavesPerSector}";
        frame.AddOverlay(DrawCommand.Label($"SECTOR {game.Sector} - {SectorRules.NameOf(game.Sector).ToUpperInvariant()}",
            Playfield.Width / 2, 16, 14, Palette.Text, TextAnchor.Center, thin: true));
        frame.AddOverlay(DrawCommand.Label(stage, Playfield.Width / 2, 34, 16, Palette.Accent));

        //Lives and bombs, right
        var lifeIcon = SpriteCatalog.LifeIcon(game.Setup.ShipShape, game.Setup.ShipColour);
        var lives = Math.Max(0, game.Player.Lives);
        var shown = Math.Min(lives, 5);
        for (var i = 0; i < shown; i++)
        {
            frame.AddOverlay(DrawCommand.Sprite(lifeIcon, Playfield.Width - 36 - (i * 30), 16, 26, 20));
        }

        if (lives > shown)
        {
            frame.AddOverlay(DrawCommand.Label($"x{lives}", Playfield.Width - 36 - (shown * 30), 17, 14, Palette.Text, TextAnchor.Right));
        }

        for (var i = 0; i < game.Player.Bombs; i++)
        {
            frame.AddOverlay(DrawCommand.Sprite(SpriteCatalog.BombIcon, Playfield.Width - 36 - (i * 28), 36, 20, 20));
        }

        frame.AddOverlay(DrawCommand.Label("BOMBS", Playfield.Width - 36 - (Math.Max(game.Player.Bombs, 1) * 28), 37, 11,
            Palette.Dim, TextAnchor.Right));

        PaintPowerUps(frame, game);
        PaintBossBar(frame, game, time);
        PaintBanner(frame, game, time);
    }

    private static void PaintPowerUps(FrameBuilder frame, GameSimulation game)
    {
        var x = 24.0;
        const double y = 700;
        foreach (var timer in game.PowerUps.ActiveTimers())
        {
            frame.AddOverlay(DrawCommand.Sprite(SpriteCatalog.PowerUp(timer.Key), x + 12, y - 2, 22, 22));
            frame.AddOverlay(DrawCommand.Label($"{PowerUpName(timer.Key)} {Math.Ceiling(timer.Value):0}", x + 28, y, 13,
                timer.Value < 3 ? Palette.Danger : Palette.Text, TextAnchor.Left));
            x += 128;
        }

        if (game.Player.ShieldStrength > 0)
        {
            frame.AddOverlay(DrawCommand.Sprite(SpriteCatalog.PowerUp(PowerUpKind.ShieldBubble), x + 12, y - 2, 22, 22));
            frame.AddOverlay(DrawCommand.Label($"SHIELD {game.Player.ShieldStrength}", x + 28, y, 13, Palette.Accent, TextAnchor.Left));
        }
    }

    private static void PaintBossBar(FrameBuilder frame, GameSimulation game, double time)
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
        frame.AddOverlay(DrawCommand.Rect(Playfield.Width / 2, y, width + 6, 16, 0xDC141820, 0xFFEBF1F7, 1, 3));
        frame.AddOverlay(DrawCommand.Rect((Playfield.Width / 2) - (width / 2) + (width * fraction / 2), y, width * fraction, 10, fill, 0, 0, 2,
            boss.IsCoreArmoured ? 0.75 + (0.25 * Math.Sin(time * 6)) : 1));
        frame.AddOverlay(DrawCommand.Label($"BOSS - PHASE {boss.Phase}/{boss.PhaseCount}", Playfield.Width / 2, y + 18, 11, Palette.Dim));
    }

    private static void PaintBanner(FrameBuilder frame, GameSimulation game, double time)
    {
        var banner = BannerFor(game);
        if (banner == null)
        {
            return;
        }

        var warning = game.Phase == StagePhase.BossWarning;
        var alpha = warning ? 0.55 + (0.45 * Math.Abs(Math.Sin(time * 5))) : 1;
        frame.AddOverlay(DrawCommand.Label(banner, Playfield.Width / 2, 330, 46, warning ? Palette.Danger : Palette.Accent,
            TextAnchor.Center, false, alpha));
        if (game.Phase == StagePhase.WaveIntro && game.Wave == 1)
        {
            frame.AddOverlay(DrawCommand.Label(SectorRules.NameOf(game.Sector).ToUpperInvariant(), Playfield.Width / 2, 380, 20,
                Palette.Text, TextAnchor.Center, thin: true));
        }
    }
}
