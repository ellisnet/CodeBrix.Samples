using System;
using System.Collections.Generic;
using System.Drawing;
using CodeBrix.Platform.GameEngine.Drawing.Direct;
using CodeBrix.Platform.GameEngine.Rendering;
using CodeBrix.Platform.GameEngine.Rendering.Backbuffers;
using CodeBrix.Platform.GameEngine.Rendering.Views;
using CodeBrix.Platform.GameEngine.Scenes;
using SkiaSharp;

namespace BrixInvaders.Game.Rendering;

/// <summary>
/// An engine direct drawing that draws the latest published <see cref="RenderFrame"/>: either its world list (as a
/// scene-layer drawing, under the particles and the boss health bar) or its overlay list (as a view drawing, over
/// everything). The world-to-screen transform is applied here, once, from <see cref="PlayfieldTransform"/>.
/// </summary>
/// <remarks>
/// On the GPU tier <see cref="OnDraw"/> runs on the UI thread with the GPU context current while the engine thread
/// builds the next frame; it only reads the immutable frame it was handed and the read-only
/// <see cref="ImageLibrary"/>, and owns its paints and fonts.
/// </remarks>
public sealed class CommandCanvas : DirectDrawingBase
{
    private readonly Func<RenderFrame> _frames;
    private readonly bool _overlay;
    private readonly ImageLibrary _images;
    private readonly SKTypeface _typeface;
    private readonly SKTypeface _thinTypeface;
    private readonly Dictionary<(bool Thin, float Size), SKFont> _fonts = new Dictionary<(bool Thin, float Size), SKFont>();
    private readonly SKPaint _imagePaint = new SKPaint { IsAntialias = true };
    private readonly SKPaint _fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
    private readonly SKPaint _strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };
    private readonly SKPaint _textPaint = new SKPaint { IsAntialias = true };
    //Linear filtering without mipmaps: mipmapping a raster picture is rebuilt on every draw on the CPU tier, which
    //  costs far more than it improves the look of sprites drawn near their native size
    private readonly SKSamplingOptions _sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);

    /// <summary>Creates the world canvas on a scene layer.</summary>
    /// <param name="host">The render surface host.</param>
    /// <param name="layer">The scene layer (camera at the origin, so world pixels are screen pixels).</param>
    /// <param name="bounds">The layer area to draw in (the whole backbuffer).</param>
    /// <param name="images">The pictures.</param>
    /// <param name="typeface">The Kenney future typeface.</param>
    /// <param name="thinTypeface">The thin face.</param>
    /// <param name="frames">Returns the latest published frame.</param>
    public CommandCanvas(RenderSurfaceHostBase host, SceneLayer layer, Rectangle bounds, ImageLibrary images, SKTypeface typeface,
        SKTypeface thinTypeface, Func<RenderFrame> frames)
        : base(host, DirectDrawingMode.SceneLayer, layer, null, null, bounds, "brixinvaders-world")
    {
        _overlay = false;
        _images = images ?? throw new ArgumentNullException(nameof(images));
        _typeface = typeface;
        _thinTypeface = thinTypeface ?? typeface;
        _frames = frames ?? throw new ArgumentNullException(nameof(frames));
    }

    /// <summary>Creates the overlay canvas on a view.</summary>
    /// <param name="host">The render surface host.</param>
    /// <param name="view">The view.</param>
    /// <param name="bounds">The screen area to draw in (the whole backbuffer).</param>
    /// <param name="images">The pictures.</param>
    /// <param name="typeface">The Kenney future typeface.</param>
    /// <param name="thinTypeface">The thin face.</param>
    /// <param name="frames">Returns the latest published frame.</param>
    public CommandCanvas(RenderSurfaceHostBase host, View view, Rectangle bounds, ImageLibrary images, SKTypeface typeface,
        SKTypeface thinTypeface, Func<RenderFrame> frames)
        : base(host, DirectDrawingMode.View, null, view, bounds, null, "brixinvaders-overlay")
    {
        _overlay = true;
        _images = images ?? throw new ArgumentNullException(nameof(images));
        _typeface = typeface;
        _thinTypeface = thinTypeface ?? typeface;
        _frames = frames ?? throw new ArgumentNullException(nameof(frames));
    }

    /// <summary>Marks the canvas dirty every engine frame, so the CPU tier's dirty-rectangle path keeps redrawing it.</summary>
    /// <param name="tick">The engine tick.</param>
    public override void Update(long tick)
    {
        base.Update(tick);
        ForceRefresh();
    }

    /// <inheritdoc />
    protected override void OnDraw(BackbufferBase backbuffer, RectangleF destRectScreen)
    {
        var canvas = backbuffer.Canvas;
        if (canvas == null || destRectScreen.Width <= 0 || destRectScreen.Height <= 0)
        {
            return;
        }

        var frame = _frames() ?? RenderFrame.Empty;
        var commands = _overlay ? frame.Overlay : frame.World;
        var transform = PlayfieldTransform.Fit(destRectScreen.Width, destRectScreen.Height);

        canvas.Save();
        canvas.ClipRect(new SKRect(destRectScreen.Left, destRectScreen.Top, destRectScreen.Right, destRectScreen.Bottom));
        canvas.Translate(destRectScreen.Left + (float)transform.OffsetX, destRectScreen.Top + (float)transform.OffsetY);
        canvas.Scale((float)transform.Scale);
        for (var i = 0; i < commands.Count; i++)
        {
            var command = commands[i];
            switch (command.Kind)
            {
                case DrawKind.Image:
                    DrawImage(canvas, command);
                    break;
                case DrawKind.Rect:
                    DrawRect(canvas, command);
                    break;
                case DrawKind.Text:
                    DrawText(canvas, command);
                    break;
                case DrawKind.Circle:
                    _fillPaint.Color = WithAlpha(command.Color, command.Alpha);
                    canvas.DrawCircle(command.X, command.Y, command.Width, _fillPaint);
                    break;
            }
        }

        canvas.Restore();
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _imagePaint.Dispose();
            _fillPaint.Dispose();
            _strokePaint.Dispose();
            _textPaint.Dispose();
            foreach (var font in _fonts.Values)
            {
                font.Dispose();
            }

            _fonts.Clear();
        }

        base.Dispose(disposing);
    }

    private static SKColor WithAlpha(uint argb, float alpha)
    {
        var color = new SKColor(argb);
        return color.WithAlpha((byte)Math.Clamp(color.Alpha * alpha, 0, 255));
    }

    private void DrawImage(SKCanvas canvas, DrawCommand command)
    {
        var image = _images.Get(command.Image);
        if (image == null || command.Alpha <= 0)
        {
            return;
        }

        var scale = Math.Min(command.Width / image.Width, command.Height / image.Height);
        var halfWidth = image.Width * scale / 2f;
        var halfHeight = image.Height * scale / 2f;
        _imagePaint.Color = SKColors.White.WithAlpha((byte)(255 * command.Alpha));
        canvas.Save();
        canvas.Translate(command.X, command.Y);
        if (command.Rotation != 0)
        {
            canvas.RotateDegrees(command.Rotation);
        }

        canvas.DrawImage(image, new SKRect(-halfWidth, -halfHeight, halfWidth, halfHeight), _sampling, _imagePaint);
        canvas.Restore();
    }

    private void DrawRect(SKCanvas canvas, DrawCommand command)
    {
        var rect = new SKRect(command.X - (command.Width / 2), command.Y - (command.Height / 2), command.X + (command.Width / 2),
            command.Y + (command.Height / 2));
        if ((command.Color >> 24) != 0)
        {
            _fillPaint.Color = WithAlpha(command.Color, command.Alpha);
            canvas.DrawRoundRect(rect, command.CornerRadius, command.CornerRadius, _fillPaint);
        }

        if ((command.StrokeColor >> 24) != 0 && command.StrokeWidth > 0)
        {
            _strokePaint.Color = WithAlpha(command.StrokeColor, command.Alpha);
            _strokePaint.StrokeWidth = command.StrokeWidth;
            canvas.DrawRoundRect(rect, command.CornerRadius, command.CornerRadius, _strokePaint);
        }
    }

    private void DrawText(SKCanvas canvas, DrawCommand command)
    {
        if (string.IsNullOrEmpty(command.Text) || _typeface == null)
        {
            return;
        }

        var font = FontFor(command.Thin, command.FontSize);
        var metrics = font.Metrics;
        var baseline = command.Y - ((metrics.Ascent + metrics.Descent) / 2f);
        var align = command.Anchor switch
        {
            TextAnchor.Left => SKTextAlign.Left,
            TextAnchor.Right => SKTextAlign.Right,
            _ => SKTextAlign.Center,
        };

        _textPaint.Color = WithAlpha(command.Color, command.Alpha);
        canvas.DrawText(command.Text, command.X, baseline, align, font, _textPaint);
    }

    private SKFont FontFor(bool thin, float size)
    {
        var key = (thin, size);
        if (!_fonts.TryGetValue(key, out var font))
        {
            font = new SKFont(thin ? _thinTypeface : _typeface, size) { Subpixel = true, Edging = SKFontEdging.Antialias };
            _fonts[key] = font;
        }

        return font;
    }
}
