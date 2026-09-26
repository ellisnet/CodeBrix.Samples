using BrixInvaders.Game.Hud;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Screens;

/// <summary>Game over: the final score over the frozen playfield; on to name entry when it is a high score.</summary>
public sealed class GameOverScreen : ScreenPainter
{
    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var frame = context.Frame;
        var screens = context.Session.Screens;
        var game = context.Session.Game;
        if (game != null)
        {
            context.Playfield.StarSpeed = 0.3;
            context.Playfield.PaintGame(frame, game, context.Time);
        }

        Ui.Veil(frame, System.Math.Min(1, OpenTime / 1.5));
        Ui.Heading(frame, "GAME OVER", 250, 72);
        frame.Overlay.Text($"FINAL SCORE {HudPainter.FormatScore(screens.FinalScore)}", Ui.CenterX, 340, frame.Font, 30,
            Palette.Gold);
        if (game != null)
        {
            frame.Overlay.Text($"SECTOR {game.Sector} - {game.Setup.Difficulty.ToString().ToUpperInvariant()}", Ui.CenterX, 384,
                frame.ThinFont, 18, Palette.Text);
        }

        if (screens.PendingHighScore)
        {
            frame.Overlay.Text("A NEW HIGH SCORE!", Ui.CenterX, 440, frame.Font, 28, Palette.Good);
        }

        if (OpenTime >= ScreenStateMachine.GameOverMinSeconds)
        {
            Ui.Footer(frame, $"{Prompts.Confirm(context.Device)} Continue");
        }
    }
}
