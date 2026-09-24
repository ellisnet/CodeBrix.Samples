using System;
using BrixInvaders.Game.Rendering;
using BrixInvaders.GameLogic;

namespace BrixInvaders.Game.Screens;

/// <summary>Shared overlay furniture for the screens: panels, headings, menu rows, the prompt footer.</summary>
public static class Ui
{
    /// <summary>The centre X of the playfield.</summary>
    public const double CenterX = Playfield.Width / 2;

    /// <summary>Draws a panel.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="x">Centre X.</param>
    /// <param name="y">Centre Y.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    /// <param name="alpha">Opacity.</param>
    public static void Panel(FrameBuilder frame, double x, double y, double width, double height, double alpha = 1)
    {
        ArgumentNullException.ThrowIfNull(frame);
        frame.AddOverlay(DrawCommand.Rect(x, y, width, height, Palette.Panel, Palette.PanelEdge, 2, 12, alpha));
    }

    /// <summary>Dims the whole playfield.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="alpha">Opacity of the veil.</param>
    public static void Veil(FrameBuilder frame, double alpha = 1)
    {
        ArgumentNullException.ThrowIfNull(frame);
        frame.AddOverlay(DrawCommand.Rect(CenterX, Playfield.Height / 2, Playfield.Width, Playfield.Height, Palette.Veil, 0, 0, 0, alpha));
    }

    /// <summary>Draws a screen heading.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="text">The heading.</param>
    /// <param name="y">Centre Y.</param>
    /// <param name="size">Font size.</param>
    public static void Heading(FrameBuilder frame, string text, double y = 90, double size = 44)
    {
        ArgumentNullException.ThrowIfNull(frame);
        frame.AddOverlay(DrawCommand.Label(text, CenterX + 2, y + 3, size, 0xC0000000));
        frame.AddOverlay(DrawCommand.Label(text, CenterX, y, size, Palette.Accent));
    }

    /// <summary>Draws one menu row, highlighted when selected.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="text">The row text.</param>
    /// <param name="x">Centre X.</param>
    /// <param name="y">Centre Y.</param>
    /// <param name="selected">Whether the cursor is on it.</param>
    /// <param name="time">Seconds (cursor pulse).</param>
    /// <param name="size">Font size.</param>
    public static void MenuRow(FrameBuilder frame, string text, double x, double y, bool selected, double time, double size = 28)
    {
        ArgumentNullException.ThrowIfNull(frame);
        if (selected)
        {
            frame.AddOverlay(DrawCommand.Rect(x, y, 420, size + 20, 0x402E7DD6, 0xFF5CE1FF, 1.5, 8));
            var bob = Math.Sin(time * 8) * 4;
            frame.AddOverlay(DrawCommand.Sprite(SpriteCatalog.Cursor, x - 190 + bob, y, 26, 26));
        }

        frame.AddOverlay(DrawCommand.Label(text, x, y, size, selected ? Palette.Text : Palette.Dim));
    }

    /// <summary>Draws the control prompts along the bottom.</summary>
    /// <param name="frame">The frame.</param>
    /// <param name="text">The prompts.</param>
    public static void Footer(FrameBuilder frame, string text)
    {
        ArgumentNullException.ThrowIfNull(frame);
        frame.AddOverlay(DrawCommand.Label(text, CenterX, Playfield.Height - 24, 15, Palette.Dim, TextAnchor.Center, thin: true));
    }

    /// <summary>Draws the transient message (a link that could not open), when there is one.</summary>
    /// <param name="context">The context.</param>
    public static void Message(PaintContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var message = context.Session.Message;
        if (message == null)
        {
            return;
        }

        var alpha = Math.Min(1, context.Session.MessageTimeLeft);
        context.Frame.AddOverlay(DrawCommand.Rect(CenterX, 640, 520, 44, 0xE0400A0A, Palette.Danger, 2, 8, alpha));
        context.Frame.AddOverlay(DrawCommand.Label(message, CenterX, 640, 18, Palette.Text, TextAnchor.Center, false, alpha));
    }
}
