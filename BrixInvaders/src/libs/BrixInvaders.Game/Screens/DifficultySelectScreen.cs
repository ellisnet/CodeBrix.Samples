using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Screens;

/// <summary>Difficulty select: Cadet, Pilot, Ace or Legend, and the start sector (up to the one unlocked).</summary>
public sealed class DifficultySelectScreen : ScreenPainter
{
    /// <summary>A one-line description of a difficulty level.</summary>
    /// <param name="level">The level.</param>
    /// <returns>The description.</returns>
    public static string Describe(Difficulty level)
    {
        var settings = DifficultyTable.For(level);
        return $"{settings.Lives} lives - {settings.StartingBombs} bombs - score {DifficultyTable.MultiplierText(level)}";
    }

    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var frame = context.Frame;
        var screens = context.Session.Screens;
        context.Playfield.StarSpeed = 0.6;
        context.Playfield.PaintBackdrop(frame, Assets.SpaceBackground.Purple, SpriteCatalog.Planet(4), 180, 600, 460, 0.8);
        Ui.Heading(frame, "CHOOSE DIFFICULTY");

        Ui.Panel(frame, Ui.CenterX, 330, 620, 330, 0.85);
        var y = 210.0;
        foreach (var level in DifficultyTable.Levels)
        {
            var selected = level == screens.Difficulty;
            Ui.MenuRow(frame, level.ToString().ToUpperInvariant(), Ui.CenterX, y, selected, context.Time);
            frame.Overlay.Text(Describe(level), Ui.CenterX, y + 28, frame.ThinFont, 13, selected ? Palette.Text : Palette.Dim);
            y += 78;
        }

        var unlocked = screens.UnlockedSector(screens.Difficulty);
        var sectorText = unlocked > 1
            ? $"START AT SECTOR  < {screens.StartSector} >   ({unlocked} unlocked)"
            : "START AT SECTOR 1   (clear sectors to unlock more)";
        frame.Overlay.Text(sectorText, Ui.CenterX, 540, frame.Font, 18, Palette.Gold);
        frame.Overlay.Text(SectorRules.NameOf(screens.StartSector).ToUpperInvariant(), Ui.CenterX, 572, frame.ThinFont, 16,
            Palette.Text);
        Ui.Footer(frame, $"{Prompts.Navigate(context.Device)} Up/Down level, Left/Right sector    " +
                         $"{Prompts.Confirm(context.Device)} Launch    {Prompts.Back(context.Device)} Back");
    }
}
