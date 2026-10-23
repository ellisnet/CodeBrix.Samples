using System;
using SkiaSharp;

namespace SimpleCbxVideoPlayer.SkiaVideo.Playback;

/// <summary>
/// The checkerboard drawn behind a transparent picture, so that its transparency can be SEEN.
/// </summary>
/// <remarks>
/// <para>
/// A frame from a master (<c>.cbvmaster</c>, CodeBrix Video Mode3) file that carries an alpha plane is composed
/// as premultiplied colour with real opacity, and the presenter blends it over whatever the canvas already
/// holds. This application clears its canvas to black for the letterbox bars, and a transparent pixel over
/// black looks exactly like a black one - so without something else behind the picture, a master's
/// transparency would be indistinguishable from a dark picture.
/// </para>
/// <para>
/// The checkerboard is the convention every image and video editor uses for "nothing here": two mid greys,
/// neither of which a real picture is likely to be confused with. It is drawn only inside the picture's own
/// rectangle, never under the letterbox bars, so the bars stay black and the edge of the picture stays
/// where it is.
/// </para>
/// </remarks>
public static class TransparencyBackdrop
{
    /// <summary>The side of one square of the checkerboard, in canvas pixels.</summary>
    public const float CellSize = 16f;

    /// <summary>The lighter of the two squares.</summary>
    public static readonly SKColor LightColor = new SKColor(0x9A, 0x9A, 0x9A);

    /// <summary>The darker of the two squares.</summary>
    public static readonly SKColor DarkColor = new SKColor(0x66, 0x66, 0x66);

    /// <summary>Fills a rectangle of the canvas with the checkerboard.</summary>
    /// <param name="canvas">The canvas to draw into.</param>
    /// <param name="area">The rectangle to fill - the picture's rectangle, not the whole view.</param>
    /// <remarks>
    /// The squares are anchored at the rectangle's own top-left corner, so the pattern sits still while the
    /// picture does, and a square at the edge is clipped rather than squeezed.
    /// </remarks>
    public static void Draw(SKCanvas canvas, SKRect area)
    {
        if (canvas == null || area.Width <= 0f || area.Height <= 0f) { return; }

        using SKPaint light = new SKPaint { Color = LightColor, IsAntialias = false };
        using SKPaint dark = new SKPaint { Color = DarkColor, IsAntialias = false };

        canvas.Save();
        canvas.ClipRect(area);
        canvas.DrawRect(area, light);

        var columns = (int)Math.Ceiling(area.Width / CellSize);
        var rows = (int)Math.Ceiling(area.Height / CellSize);

        for (var row = 0; row < rows; row++)
        {
            for (var column = (row % 2 == 0) ? 1 : 0; column < columns; column += 2)
            {
                var left = area.Left + (column * CellSize);
                var top = area.Top + (row * CellSize);
                canvas.DrawRect(SKRect.Create(left, top, CellSize, CellSize), dark);
            }
        }

        canvas.Restore();
    }
}
