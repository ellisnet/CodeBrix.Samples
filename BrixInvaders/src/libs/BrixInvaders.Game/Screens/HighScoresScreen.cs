using System.Globalization;
using BrixInvaders.Game.Hud;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;
using SkiaSharp;

namespace BrixInvaders.Game.Screens;

/// <summary>The high-score tables, one tab per difficulty.</summary>
public sealed class HighScoresScreen : ScreenPainter
{
    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var frame = context.Frame;
        var session = context.Session;
        var shown = session.Screens.HighScoreViewDifficulty;
        context.Playfield.StarSpeed = 0.5;
        context.Playfield.PaintBackdrop(frame, Assets.SpaceBackground.Blue, SpriteCatalog.Planet(5), 150, 150, 360, 0.7);
        Ui.Heading(frame, "HIGH SCORES", 70);

        var tabX = Ui.CenterX - 330;
        foreach (var level in DifficultyTable.Levels)
        {
            var selected = level == shown;
            frame.Overlay.Rectangle(tabX, 130, 200, 40, selected ? 0x602E7DD6 : Palette.Panel,
                selected ? Palette.Accent : Palette.PanelEdge, selected ? 2 : 1, 8);
            frame.Overlay.Text(level.ToString().ToUpperInvariant(), tabX, 131, frame.Font, 18,
                selected ? Palette.Text : Palette.Dim);
            tabX += 220;
        }

        Ui.Panel(frame, Ui.CenterX, 400, 760, 470, 0.85);
        var entries = session.HighScores.EntriesFor(shown);
        if (entries.Count == 0)
        {
            frame.Overlay.Text("NO SCORES YET - GO AND SET ONE", Ui.CenterX, 400, frame.Font, 20, Palette.Dim);
        }

        var highlight = shown == session.Screens.Difficulty ? session.LastHighScoreRank : -1;
        for (var i = 0; i < entries.Count; i++)
        {
            var y = 190 + (i * 42);
            var color = i == highlight ? Palette.Good : i == 0 ? Palette.Gold : Palette.Text;
            frame.Overlay.Text($"{i + 1}.", Ui.CenterX - 300, y, frame.Font, 22, color, SKTextAlign.Right);
            frame.Overlay.Text(entries[i].Name, Ui.CenterX - 250, y, frame.Font, 22, color, SKTextAlign.Left);
            frame.Overlay.Text(HudPainter.FormatScore(entries[i].Score), Ui.CenterX + 150, y, frame.Font, 22, color,
                SKTextAlign.Right);
            frame.Overlay.Text($"SECTOR {entries[i].Sector.ToString(CultureInfo.InvariantCulture)}", Ui.CenterX + 330, y,
                frame.ThinFont, 16, Palette.Dim, SKTextAlign.Right);
        }

        Ui.Footer(frame, $"{Prompts.Navigate(context.Device)} Difficulty    {Prompts.Back(context.Device)} Back");
    }
}
