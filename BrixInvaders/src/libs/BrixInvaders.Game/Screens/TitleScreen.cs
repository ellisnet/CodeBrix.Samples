using System;
using System.Globalization;
using BrixInvaders.Assets;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Screens;

/// <summary>
/// The title: an invasion fleet drifting in formation over a planet, parallax stars, the title in the Kenney
/// future font and the menu (Play / High scores / Settings / Credits / Quit). The state machine starts attract
/// mode after 15 seconds without input.
/// </summary>
public sealed class TitleScreen : ScreenPainter
{
    /// <summary>The title text.</summary>
    public const string GameTitle = "BRIXINVADERS";

    /// <summary>The menu rows, in <see cref="TitleMenuItem"/> order.</summary>
    public static readonly string[] MenuItems = { "PLAY", "HIGH SCORES", "SETTINGS", "CREDITS", "QUIT" };

    private const int FleetColumns = 9;
    private const int FleetRows = 4;

    /// <summary>Draws the drifting title fleet (also behind the splash's composed card).</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="time">Seconds.</param>
    public static void PaintFleet(FrameBuilder frame, double time)
    {
        ArgumentNullException.ThrowIfNull(frame);
        var sway = Math.Sin(time * 0.45) * 150;
        var descent = (time * 9) % 260;
        for (var row = 0; row < FleetRows; row++)
        {
            for (var column = 0; column < FleetColumns; column++)
            {
                var x = Ui.CenterX + sway + ((column - ((FleetColumns - 1) / 2.0)) * 84);
                var y = 150 + descent + (row * 62) - 130;
                var alpha = Math.Clamp((y - 40) / 80, 0, 1) * Math.Clamp((560 - y) / 90, 0, 1);
                if (alpha <= 0)
                {
                    continue;
                }

                var role = (EnemyRole)((row + column) % 5);
                var colour = (EnemyColour)(3 - row);
                PlayfieldPainter.PaintEnemy(frame, role, colour, x, y + (Math.Sin((time * 2) + column) * 5), 48,
                    Math.Sin((time * 1.5) + row) * 6, alpha * 0.9);
            }
        }
    }

    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var frame = context.Frame;
        context.Playfield.StarSpeed = 0.35;
        context.Playfield.PaintBackdrop(frame, SpaceBackground.DarkPurple, SpriteCatalog.Planet(7), Ui.CenterX, 900, 1000, 0.95);
        PaintFleet(frame, context.Time);

        var pulse = 0.85 + (0.15 * Math.Sin(OpenTime * 2));
        frame.AddOverlay(DrawCommand.Label(GameTitle, Ui.CenterX + 4, 134, 84, 0xB0000000));
        frame.AddOverlay(DrawCommand.Label(GameTitle, Ui.CenterX, 130, 84, Palette.Accent, TextAnchor.Center, false, pulse));
        frame.AddOverlay(DrawCommand.Label("DEFEND THE SECTORS", Ui.CenterX, 190, 20, Palette.Text, TextAnchor.Center, thin: true));

        var screens = context.Session.Screens;
        var best = context.Session.HighScores.BestScore(screens.Difficulty);
        if (best > 0)
        {
            frame.AddOverlay(DrawCommand.Label($"BEST ON {screens.Difficulty.ToString().ToUpperInvariant()}: " +
                best.ToString("N0", CultureInfo.InvariantCulture), Ui.CenterX, 226, 16, Palette.Gold));
        }

        Ui.Panel(frame, Ui.CenterX, 430, 480, 330, 0.85);
        for (var i = 0; i < MenuItems.Length; i++)
        {
            Ui.MenuRow(frame, MenuItems[i], Ui.CenterX, 310 + (i * 60), (int)screens.TitleCursor == i, context.Time);
        }

        frame.AddOverlay(DrawCommand.Label(KenneyPacks.CreditLine, Ui.CenterX, 634, 14, Palette.Text, TextAnchor.Center, thin: true));
        Ui.Footer(frame, $"{Prompts.Navigate(context.Device)} Choose    {Prompts.Confirm(context.Device)} Select");
        Ui.Message(context);
    }
}
