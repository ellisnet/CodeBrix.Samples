using BrixInvaders.Game.Rendering;
using BrixInvaders.Game.Settings;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Screens;

/// <summary>Ship select: three hull shapes in four colours (cosmetic).</summary>
public sealed class ShipSelectScreen : ScreenPainter
{
    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var frame = context.Frame;
        var screens = context.Session.Screens;
        context.Playfield.StarSpeed = 0.6;
        context.Playfield.PaintBackdrop(frame, Assets.SpaceBackground.Blue, SpriteCatalog.Planet(2), 1100, 620, 520, 0.8);
        Ui.Heading(frame, "CHOOSE YOUR SHIP");

        for (var shape = 0; shape < GameSetup.ShipShapeCount; shape++)
        {
            var x = Ui.CenterX + ((shape - 1) * 300);
            var selected = shape == screens.ShipShape;
            Ui.Panel(frame, x, 330, 240, 250, selected ? 1 : 0.55);
            var scale = selected ? 2.0 + (0.08 * System.Math.Sin(OpenTime * 3)) : 1.4;
            PlayfieldPainter.PaintShip(frame, shape, screens.ShipColour, x, 320, scale, context.Time, overlay: true);
            frame.Overlay.Text($"SHIP {shape + 1}", x, 430, frame.Font, 20, selected ? Palette.Accent : Palette.Dim);
        }

        for (var colour = 0; colour < GameSetup.ShipColourCount; colour++)
        {
            var x = Ui.CenterX + ((colour - 1.5) * 110);
            var selected = colour == screens.ShipColour;
            frame.Overlay.Image(SpriteCatalog.LifeIcon(screens.ShipShape, colour), null, x, 530, selected ? 52 : 36,
                selected ? 40 : 28, 0, selected ? 1 : 0.5);
        }

        frame.Overlay.Text(SettingsMenu.ShipName(screens.ShipShape, screens.ShipColour).ToUpperInvariant(), Ui.CenterX, 590,
            frame.Font, 22, Palette.Text);
        Ui.Footer(frame, $"{Prompts.Navigate(context.Device)} Left/Right hull, Up/Down colour    " +
                         $"{Prompts.Confirm(context.Device)} Next    {Prompts.Back(context.Device)} Back");
    }
}
