using System;
using BrixInvaders.Game.Hud;
using BrixInvaders.Game.Rendering;

namespace BrixInvaders.Game.Screens;

/// <summary>Attract mode: the demo pilot plays a wave on its own; any input returns to the title.</summary>
public sealed class AttractScreen : ScreenPainter
{
    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var attract = context.Session.Attract;
        if (attract == null)
        {
            return;
        }

        context.Playfield.StarSpeed = 1.0;
        context.Playfield.PaintGame(context.Frame, attract, context.Time);
        HudPainter.Paint(context.Frame, attract, context.Time);
        var alpha = 0.6 + (0.4 * Math.Sin(OpenTime * 3));
        context.Frame.Overlay.Text("DEMO", Ui.CenterX, 150, context.Frame.Font, 48, Palette.Accent, alpha: alpha);
        context.Frame.Overlay.Text("PRESS ANY KEY OR BUTTON", Ui.CenterX, 196, context.Frame.Font, 18, Palette.Text);
    }
}
