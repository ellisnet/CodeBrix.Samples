using BrixInvaders.Assets;
using BrixInvaders.Game.Rendering;

namespace BrixInvaders.Game.Screens;

/// <summary>The splash: the engine's splash overlay shows the composed title card over a quiet starfield.</summary>
public sealed class SplashScreen : ScreenPainter
{
    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        context.Playfield.StarSpeed = 0.4;
        context.Playfield.PaintBackdrop(context.Frame, SpaceBackground.Black, null, 0, 0, 0);
        Ui.Footer(context.Frame, $"{Prompts.Confirm(context.Device)} Skip");
    }
}
