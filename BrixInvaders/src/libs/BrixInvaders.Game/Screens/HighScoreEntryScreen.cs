using System;
using BrixInvaders.Game.Hud;
using BrixInvaders.Game.Input;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Screens;

/// <summary>High-score name entry: three characters, keyboard or gamepad.</summary>
public sealed class HighScoreEntryScreen : ScreenPainter
{
    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var frame = context.Frame;
        var screens = context.Session.Screens;
        var entry = screens.NameEntry;
        context.Playfield.StarSpeed = 0.5;
        context.Playfield.PaintBackdrop(frame, Assets.SpaceBackground.DarkPurple, SpriteCatalog.Planet(9), 1080, 560, 480, 0.8);
        Ui.Heading(frame, "NEW HIGH SCORE", 120, 52);
        frame.AddOverlay(DrawCommand.Label(HudPainter.FormatScore(screens.FinalScore), Ui.CenterX, 190, 34, Palette.Gold));
        frame.AddOverlay(DrawCommand.Label($"ON {screens.Difficulty.ToString().ToUpperInvariant()}", Ui.CenterX, 230, 16, Palette.Text,
            TextAnchor.Center, thin: true));

        for (var i = 0; i < NameEntry.Length; i++)
        {
            var x = Ui.CenterX + ((i - 1) * 110);
            var selected = i == entry.Cursor && !entry.IsComplete;
            frame.AddOverlay(DrawCommand.Rect(x, 360, 90, 110, selected ? 0x602E7DD6 : Palette.Panel, selected ? Palette.Accent : Palette.PanelEdge,
                selected ? 3 : 2, 10));
            frame.AddOverlay(DrawCommand.Label(entry.LetterAt(i).ToString(), x, 362, 64, selected ? Palette.Text : Palette.Dim));
            if (selected)
            {
                var bob = Math.Sin(OpenTime * 6) * 3;
                frame.AddOverlay(DrawCommand.Label("^", x, 296 - bob, 22, Palette.Accent));
                frame.AddOverlay(DrawCommand.Label("v", x, 428 + bob, 22, Palette.Accent));
            }
        }

        Ui.Footer(frame, FooterText(context.Device, entry.Cursor));
    }

    /// <summary>
    /// The control hints for the name entry, in the glyph set of the last device used. Up moves to the next
    /// character (A -> B ... Z -> 0 ... 9 -> A), Down to the previous one; both repeat while held.
    /// </summary>
    /// <param name="device">The device the prompts follow.</param>
    /// <param name="cursor">The letter under the cursor, 0..2 (Confirm finishes on the last one).</param>
    /// <returns>The footer text.</returns>
    public static string FooterText(InputDevice device, int cursor)
    {
        var navigate = device == InputDevice.Gamepad ? $"{Prompts.Navigate(device)} {Prompts.Move(device)}" : Prompts.Navigate(device);
        var confirm = cursor >= NameEntry.Length - 1 ? "Done" : "Next";
        return $"{navigate} Up/Down letter (hold to scroll), Left/Right move    " +
               $"{Prompts.Confirm(device)} {confirm}    {Prompts.Back(device)} Previous";
    }
}
