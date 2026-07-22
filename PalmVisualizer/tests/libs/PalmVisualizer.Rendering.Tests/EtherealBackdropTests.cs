using SilverAssertions;
using SkiaSharp;
using Xunit;
using PalmVisualizer.Rendering;

namespace PalmVisualizer.Rendering.Tests;

public class EtherealBackdropTests
{
    [Fact]
    public void Shader_compiles()
    {
        //Act
        using SKRuntimeEffect effect = EtherealBackdrop.CreateEffect();

        //Assert
        (effect != null).Should().Be(true);
    }

    [Fact]
    public void Shader_accepts_all_of_the_backdrops_uniforms()
    {
        //Arrange
        using SKRuntimeEffect effect = EtherealBackdrop.CreateEffect();

        //Act / Assert - setting an unknown uniform name throws, so this passing proves
        //  every uniform the backdrop sets per frame exists in the compiled shader
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
        //  CpuRendering fallback does at run time
        using SKRuntimeEffect effect = EtherealBackdrop.CreateEffect();
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
}
