using System.Collections.Generic;
using BrixInvaders.Game.Credits;
using BrixInvaders.Game.Rendering;

namespace BrixInvaders.Game.Screens;

/// <summary>
/// Credits: renders whatever <see cref="ICreditsContent"/> returns; link lines are clickable and open through the
/// link seam, and K / gamepad Y opens the Kenney bundle link.
/// </summary>
public sealed class CreditsScreen : ScreenPainter
{
    private IReadOnlyList<CreditsLine> _lines = new List<CreditsLine>();

    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var frame = context.Frame;
        context.Playfield.StarSpeed = 0.5;
        context.Playfield.PaintBackdrop(frame, Assets.SpaceBackground.DarkPurple, SpriteCatalog.Planet(0), Ui.CenterX, 820, 900, 0.8);
        TitleScreen.PaintFleet(frame, context.Time);
        Ui.Panel(frame, Ui.CenterX, 350, 1000, 600, 0.9);

        var y = 80.0;
        foreach (var line in _lines)
        {
            switch (line.Style)
            {
                case CreditsLineStyle.Heading:
                    frame.AddOverlay(DrawCommand.Label(line.Text, Ui.CenterX, y, 24, Palette.Accent));
                    y += 34;
                    break;
                case CreditsLineStyle.Link:
                    frame.AddOverlay(DrawCommand.Label(line.Text, Ui.CenterX, y, 17, Palette.Gold));
                    frame.AddHotspot(new Hotspot(Ui.CenterX, y, 820, 26, line.Url));
                    y += 28;
                    break;
                case CreditsLineStyle.Spacer:
                    y += 14;
                    break;
                default:
                    frame.AddOverlay(DrawCommand.Label(line.Text, Ui.CenterX, y, 15, Palette.Text, TextAnchor.Center, thin: true));
                    y += 24;
                    break;
            }
        }

        Ui.Footer(frame, $"Click a link    {Prompts.Link(context.Device)} Kenney bundle    {Prompts.Back(context.Device)} Back");
        Ui.Message(context);
    }

    /// <inheritdoc />
    protected override void OnOpen(PaintContext context) => _lines = context.Credits.GetLines();
}
