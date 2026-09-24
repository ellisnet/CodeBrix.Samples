using BrixInvaders.Game.Hud;

namespace BrixInvaders.Game.Screens;

/// <summary>Play: the playfield and the HUD.</summary>
public sealed class PlayingScreen : ScreenPainter
{
    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var game = context.Session.Game;
        if (game == null)
        {
            return;
        }

        context.Playfield.StarSpeed = 1.0;
        context.Playfield.PaintGame(context.Frame, game, context.Time);
        HudPainter.Paint(context.Frame, game, context.Time);
        if (OpenTime < 4 && game.Wave <= 1)
        {
            Ui.Footer(context.Frame, $"{Prompts.Move(context.Device)} Move    {Prompts.Fire(context.Device, context.Profile)} Fire    " +
                                     $"{Prompts.Bomb(context.Device, context.Profile)} Bomb    {Prompts.Pause(context.Device)} Pause");
        }
    }
}
