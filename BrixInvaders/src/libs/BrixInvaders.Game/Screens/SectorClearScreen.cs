using System;
using BrixInvaders.Game.Hud;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Screens;

/// <summary>Sector clear: the bonus, the score so far and the Kenney card; confirm goes to the next briefing.</summary>
public sealed class SectorClearScreen : ScreenPainter
{
    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var frame = context.Frame;
        var game = context.Session.Game;
        var sector = context.Session.Screens.CurrentSector;
        if (game != null)
        {
            context.Playfield.StarSpeed = 1.6;
            context.Playfield.PaintGame(frame, game, context.Time);
        }

        Ui.Veil(frame, 0.55);
        Ui.Heading(frame, $"SECTOR {sector} CLEAR", 100, 52);
        frame.Overlay.Text($"SECTOR BONUS {HudPainter.FormatScore(Scoring.SectorClearBonus(sector))}", Ui.CenterX, 160,
            frame.Font, 22, Palette.Good);
        if (game != null)
        {
            frame.Overlay.Text($"SCORE {HudPainter.FormatScore(game.Score)}", Ui.CenterX, 200, frame.Font, 26, Palette.Gold);
            frame.Overlay.Text($"BEST CHAIN {game.Scoring.BestChain}", Ui.CenterX, 236, frame.ThinFont, 16, Palette.Text);
        }

        KenneyCard.Paint(context, Ui.CenterX, 430);
        var ready = OpenTime >= ScreenStateMachine.SectorClearMinSeconds;
        frame.Overlay.Text($"{Prompts.Confirm(context.Device)} NEXT SECTOR: {SectorRules.NameOf(sector + 1).ToUpperInvariant()}",
            Ui.CenterX, 610, frame.Font, 22, Palette.Accent, alpha: ready ? 0.6 + (0.4 * Math.Sin(OpenTime * 4)) : 0.3);
        Ui.Message(context);
    }
}
