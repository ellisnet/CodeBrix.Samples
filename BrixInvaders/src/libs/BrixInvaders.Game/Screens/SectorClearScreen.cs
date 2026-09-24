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
        frame.AddOverlay(DrawCommand.Label($"SECTOR BONUS {HudPainter.FormatScore(Scoring.SectorClearBonus(sector))}", Ui.CenterX, 160,
            22, Palette.Good));
        if (game != null)
        {
            frame.AddOverlay(DrawCommand.Label($"SCORE {HudPainter.FormatScore(game.Score)}", Ui.CenterX, 200, 26, Palette.Gold));
            frame.AddOverlay(DrawCommand.Label($"BEST CHAIN {game.Scoring.BestChain}", Ui.CenterX, 236, 16, Palette.Text,
                TextAnchor.Center, thin: true));
        }

        KenneyCard.Paint(context, Ui.CenterX, 430);
        var ready = OpenTime >= ScreenStateMachine.SectorClearMinSeconds;
        frame.AddOverlay(DrawCommand.Label($"{Prompts.Confirm(context.Device)} NEXT SECTOR: {SectorRules.NameOf(sector + 1).ToUpperInvariant()}",
            Ui.CenterX, 610, 22, Palette.Accent, TextAnchor.Center, false, ready ? 0.6 + (0.4 * Math.Sin(OpenTime * 4)) : 0.3));
        Ui.Message(context);
    }
}
