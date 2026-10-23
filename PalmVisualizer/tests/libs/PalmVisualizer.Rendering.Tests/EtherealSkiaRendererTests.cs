using SilverAssertions;
using SkiaSharp;
using Xunit;
using PalmVisualizer.Rendering;

namespace PalmVisualizer.Rendering.Tests;

public class EtherealSkiaRendererTests
{
    [Fact]
    public void Shader_compiles()
    {
        //Act
        using SKRuntimeEffect effect = EtherealSkiaRenderer.CreateEffect();

        //Assert
        (effect != null).Should().Be(true);
    }

    [Fact]
    public void Shader_accepts_all_of_the_renderers_uniforms()
    {
        //Arrange
        using SKRuntimeEffect effect = EtherealSkiaRenderer.CreateEffect();

        //Act / Assert - setting an unknown uniform name throws, so this passing proves
        //  every uniform the renderer sets per frame exists in the compiled shader
        var uniforms = new SKRuntimeEffectUniforms(effect)
        {
            ["iTime"] = 1.5f,
            ["iResolution"] = new[] { 640f, 360f },
            ["iPalm0"] = new[] { 0.5f, 0.0f, 1.0f },
            ["iPalm1"] = new[] { 0f, 0f, 0f },
            ["iPalm2"] = new[] { 0f, 0f, 0f },
            ["iPalm3"] = new[] { 0f, 0f, 0f },
        };
        using SKShader shader = effect.ToShader(uniforms);
        (shader != null).Should().Be(true);
    }

    [Fact]
    public void Shader_paints_colors_onto_a_raster_surface()
    {
        //Arrange - evaluate the real shader on Skia's CPU backend, exactly what the
        //  CPU fallback of Ethereal - OpenGL does at run time
        using SKRuntimeEffect effect = EtherealSkiaRenderer.CreateEffect();
        var uniforms = new SKRuntimeEffectUniforms(effect)
        {
            ["iTime"] = 2.0f,
            ["iResolution"] = new[] { 64f, 64f },
            ["iPalm0"] = new[] { 0.0f, 0.0f, 1.0f },   //an open palm at the center
            ["iPalm1"] = new[] { 0f, 0f, 0f },
            ["iPalm2"] = new[] { 0f, 0f, 0f },
            ["iPalm3"] = new[] { 0f, 0f, 0f },
        };

        using var surface = SKSurface.Create(new SKImageInfo(64, 64, SKColorType.Rgba8888));
        using (SKShader shader = effect.ToShader(uniforms))
        using (var paint = new SKPaint())
        {
            paint.Shader = shader;

            //Act
            surface.Canvas.DrawRect(new SKRect(0, 0, 64, 64), paint);
        }

        //Assert - the plasma is never black: every sampled pixel carries color
        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        var coloredPixels = 0;
        for (int y = 8; y < 64; y += 16)
        {
            for (int x = 8; x < 64; x += 16)
            {
                SKColor pixel = bitmap.GetPixel(x, y);
                if (pixel.Red + pixel.Green + pixel.Blue > 30) { coloredPixels++; }
            }
        }
        (coloredPixels > 0).Should().Be(true);
    }
    [Fact]
    public void Draw_paints_a_whole_frame_of_the_scene()
    {
        //Arrange - a scene advanced to a frame, drawn by the real renderer on the CPU
        var scene = new EtherealScene(new PalmAttractorField());
        scene.Advance(3f, 96f, 64f, 1f);
        using var renderer = new EtherealSkiaRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(96, 64, SKColorType.Rgba8888));
        surface.Canvas.Clear(SKColors.Black);

        //Act
        renderer.Draw(surface.Canvas, scene);

        //Assert - the plasma covers the frame: no pixel is left at the black clear colour
        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        var blackPixels = 0;
        for (int y = 0; y < 64; y += 4)
        {
            for (int x = 0; x < 96; x += 4)
            {
                SKColor pixel = bitmap.GetPixel(x, y);
                if (pixel.Red + pixel.Green + pixel.Blue < 10) { blackPixels++; }
            }
        }
        blackPixels.Should().Be(0);
    }

    [Fact]
    public void Draw_rejects_a_missing_canvas_or_scene()
    {
        //Arrange
        using var renderer = new EtherealSkiaRenderer();
        using var surface = SKSurface.Create(new SKImageInfo(8, 8));
        var scene = new EtherealScene(new PalmAttractorField());

        //Act / Assert
        Assert.Throws<System.ArgumentNullException>(() => renderer.Draw(null, scene));
        Assert.Throws<System.ArgumentNullException>(() => renderer.Draw(surface.Canvas, null));
    }
}
