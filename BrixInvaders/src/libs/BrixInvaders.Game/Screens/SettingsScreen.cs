using BrixInvaders.Game.Rendering;
using BrixInvaders.Game.Settings;

namespace BrixInvaders.Game.Screens;

/// <summary>
/// Settings: master/music/effects volume, music model, instrument library, gamepad profile, default ship and
/// difficulty, reset high scores. The rows' meaning lives in <see cref="SettingsMenu"/>; changes reach the mixer
/// and the music director through the session.
/// </summary>
public sealed class SettingsScreen : ScreenPainter
{
    /// <summary>The gamepad status line shown under the rows (set by the host).</summary>
    public string GamepadStatus { get; set; } = string.Empty;

    /// <inheritdoc />
    public override void Paint(PaintContext context)
    {
        var frame = context.Frame;
        var session = context.Session;
        context.Playfield.StarSpeed = 0.5;
        context.Playfield.PaintBackdrop(frame, Assets.SpaceBackground.Black, SpriteCatalog.Planet(3), 1150, 150, 380, 0.7);
        Ui.Heading(frame, "SETTINGS", 70);
        Ui.Panel(frame, Ui.CenterX, 370, 900, 560, 0.9);

        var cursor = session.Screens.SettingsCursor;
        for (var row = 0; row < SettingsMenu.RowCount; row++)
        {
            var y = 130 + (row * 56);
            var selected = row == cursor;
            if (selected)
            {
                frame.AddOverlay(DrawCommand.Rect(Ui.CenterX, y, 860, 46, 0x402E7DD6, Palette.Accent, 1.5, 8));
            }

            frame.AddOverlay(DrawCommand.Label(SettingsMenu.LabelOf(row).ToUpperInvariant(), Ui.CenterX - 410, y, 20,
                selected ? Palette.Text : Palette.Dim, TextAnchor.Left));
            var value = session.SettingsMenu.ValueOf(row);
            var armed = row == SettingsMenu.ResetHighScoresRow && session.SettingsMenu.ResetArmed;
            frame.AddOverlay(DrawCommand.Label(selected && row != SettingsMenu.ResetHighScoresRow ? $"<  {value}  >" : value,
                Ui.CenterX + 410, y, 18, armed ? Palette.Danger : selected ? Palette.Accent : Palette.Text, TextAnchor.Right));
        }

        if (!string.IsNullOrEmpty(GamepadStatus))
        {
            frame.AddOverlay(DrawCommand.Label(GamepadStatus, Ui.CenterX, 672, 13, Palette.Dim, TextAnchor.Center, thin: true));
        }

        Ui.Footer(frame, $"{Prompts.Navigate(context.Device)} Up/Down row, Left/Right change    " +
                         $"{Prompts.Confirm(context.Device)} Activate    {Prompts.Back(context.Device)} Back");
    }
}
