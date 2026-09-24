using System;
using System.IO;
using BrixInvaders.Assets;
using BrixInvaders.GameLogic;
using SkiaSharp;

namespace BrixInvaders.Game.Rendering;

/// <summary>
/// Composes the splash picture at start-up from the Kenney content (Spot.Brix's pattern): a planet rising from the
/// bottom, the invasion fleet in formation as a silhouette against it, and the title in the Kenney future font.
/// </summary>
public static class SplashComposer
{
    /// <summary>The composed picture's width.</summary>
    public const int Width = 1280;

    /// <summary>The composed picture's height.</summary>
    public const int Height = 720;

    /// <summary>Composes the splash as a PNG stream.</summary>
    /// <param name="images">The pictures.</param>
    /// <param name="typeface">The Kenney future typeface.</param>
    /// <param name="thinTypeface">The thin face.</param>
    /// <returns>A PNG stream positioned at its start.</returns>
    public static Stream Compose(ImageLibrary images, SKTypeface typeface, SKTypeface thinTypeface)
    {
        ArgumentNullException.ThrowIfNull(images);
        using var bitmap = new SKBitmap(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using (var canvas = new SKCanvas(bitmap))
        {
            DrawSky(canvas);
            DrawPlanet(canvas, images);
            DrawFleet(canvas, images);
            DrawTitle(canvas, typeface, thinTypeface);
            canvas.Flush();
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return new MemoryStream(data.ToArray());
    }

    private static void DrawSky(SKCanvas canvas)
    {
        using var sky = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, Height),
                new[] { new SKColor(4, 6, 20), new SKColor(18, 10, 44), new SKColor(40, 16, 60) }, null, SKShaderTileMode.Clamp),
        };
        canvas.DrawRect(0, 0, Width, Height, sky);

        var random = new Random(4242);
        using var star = new SKPaint { IsAntialias = true };
        for (var i = 0; i < 260; i++)
        {
            star.Color = new SKColor(220, 230, 255, (byte)random.Next(60, 230));
            canvas.DrawCircle((float)(random.NextDouble() * Width), (float)(random.NextDouble() * Height), (float)(0.5 + (random.NextDouble() * 1.4)), star);
        }
    }

    private static void DrawPlanet(SKCanvas canvas, ImageLibrary images)
    {
        var planet = images.Get(SpriteCatalog.Planet(7));
        if (planet == null)
        {
            return;
        }

        using var glow = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateRadialGradient(new SKPoint(Width / 2f, 1150), 760,
                new[] { new SKColor(120, 180, 255, 90), new SKColor(120, 180, 255, 0) }, null, SKShaderTileMode.Clamp),
        };
        canvas.DrawCircle(Width / 2f, 1150, 760, glow);
        using var paint = new SKPaint { IsAntialias = true };
        canvas.DrawImage(planet, new SKRect((Width / 2f) - 620, 540, (Width / 2f) + 620, 1780),
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear), paint);
    }

    private static void DrawFleet(SKCanvas canvas, ImageLibrary images)
    {
        using var silhouette = new SKPaint
        {
            IsAntialias = true,
            ColorFilter = SKColorFilter.CreateBlendMode(new SKColor(10, 8, 24, 235), SKBlendMode.SrcIn),
        };
        using var rim = new SKPaint
        {
            IsAntialias = true,
            ColorFilter = SKColorFilter.CreateBlendMode(new SKColor(92, 225, 255, 110), SKBlendMode.SrcIn),
        };
        var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
        for (var row = 0; row < 4; row++)
        {
            var columns = 11 - row;
            var size = 50f + (row * 6);
            for (var column = 0; column < columns; column++)
            {
                var image = images.Get(SpriteCatalog.Enemy((EnemyRole)((row + column) % 5), (EnemyColour)(row % 4)));
                if (image == null)
                {
                    continue;
                }

                var x = (Width / 2f) + ((column - ((columns - 1) / 2f)) * (size + 26));
                var y = 330f + (row * (size + 12));
                var scale = size / Math.Max(image.Width, image.Height);
                var rect = new SKRect(x - (image.Width * scale / 2), y - (image.Height * scale / 2), x + (image.Width * scale / 2),
                    y + (image.Height * scale / 2));
                var rimRect = rect;
                rimRect.Offset(0, -2);
                canvas.DrawImage(image, rimRect, sampling, rim);
                canvas.DrawImage(image, rect, sampling, silhouette);
            }
        }
    }

    private static void DrawTitle(SKCanvas canvas, SKTypeface typeface, SKTypeface thinTypeface)
    {
        if (typeface == null)
        {
            return;
        }

        using var titleFont = new SKFont(typeface, 112) { Edging = SKFontEdging.Antialias };
        using var subFont = new SKFont(thinTypeface ?? typeface, 22) { Edging = SKFontEdging.Antialias };
        using var shadow = new SKPaint { IsAntialias = true, Color = new SKColor(0, 0, 0, 170) };
        using var glow = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(92, 225, 255, 120),
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 14),
        };
        using var fill = new SKPaint { IsAntialias = true, Color = new SKColor(236, 246, 255) };
        using var sub = new SKPaint { IsAntialias = true, Color = new SKColor(180, 200, 230) };

        const string title = "BRIXINVADERS";
        canvas.DrawText(title, (Width / 2f) + 5, 215, SKTextAlign.Center, titleFont, shadow);
        canvas.DrawText(title, Width / 2f, 210, SKTextAlign.Center, titleFont, glow);
        canvas.DrawText(title, Width / 2f, 210, SKTextAlign.Center, titleFont, fill);
        canvas.DrawText(KenneyPacks.CreditLine, Width / 2f, 262, SKTextAlign.Center, subFont, sub);
    }
}
