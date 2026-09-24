using BrixInvaders.Game.Hud;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Screens;

/// <summary>
/// The pause overlay over the frozen game: Resume or Quit to title. This is a GAME pause (the simulation stops
/// stepping and the music director hears <c>OnPause</c>), not the engine's global pause, because the engine's input
/// pollers stop during an engine pause and the menu must still be driven.
/// </summary>
public sealed class PausedScreen : ScreenPainter
{
    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var game = context.Session.Game;
        var frame = context.Frame;
        if (game != null)
        {
            context.Playfield.StarSpeed = 0;
            context.Playfield.PaintGame(frame, game, context.Time);
            HudPainter.Paint(frame, game, context.Time);
        }

        Ui.Veil(frame, 0.8);
        Ui.Heading(frame, "PAUSED", 250, 56);
        var cursor = context.Session.Screens.PauseCursor;
        Ui.MenuRow(frame, "RESUME", Ui.CenterX, 350, cursor == PauseMenuItem.Resume, context.Time);
        Ui.MenuRow(frame, "QUIT TO TITLE", Ui.CenterX, 420, cursor == PauseMenuItem.QuitToTitle, context.Time);
        Ui.Footer(frame, $"{Prompts.Navigate(context.Device)} Choose    {Prompts.Confirm(context.Device)} Select    " +
                         $"{Prompts.Pause(context.Device)} Resume");
    }
}
