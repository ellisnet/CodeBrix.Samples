using SilverAssertions;
using SkiaSharp;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

/// <summary>
/// Exercises <see cref="ModelShotRenderer"/> end to end against a real (llvmpipe or GPU)
/// OpenGL ES context via the Mesa surfaceless platform. Skips when no GL stack is available.
/// </summary>
[Trait("Category", "RequiresGL")]
public class ModelShotRendererTests
{
    private static EglTestContext RequireGl()
    {
        var context = EglTestContext.TryCreate();
        Assert.SkipWhen(context is null, "No EGL/OpenGL stack available on this machine (install Mesa llvmpipe).");
        return context!;
    }

    [Fact]
    public void renders_a_staged_shot_to_a_decodable_png_of_the_requested_size()
    {
        //Arrange
        using var egl = RequireGl();
        var scene = ShotSceneBuilder.Build(TestAssets.BuildTriangleModel(), ShotStage.Light());
        using var renderer = new ModelShotRenderer(egl.Gl);

        //Act
        var png = renderer.RenderPng(scene, ShotStage.Light(), ShotAngle.Hero, 320, 280);

        //Assert
        using var bitmap = SKBitmap.Decode(png);
        bitmap.Should().NotBeNull();
        bitmap.Width.Should().Be(320);
        bitmap.Height.Should().Be(280);
    }

    [Fact]
    public void the_shot_shows_both_the_model_and_its_stage()
    {
        //Arrange - a red triangle on the dark stage
        using var egl = RequireGl();
        var stage = ShotStage.Dark();
        var scene = ShotSceneBuilder.Build(TestAssets.BuildTriangleModel(), stage);
        using var renderer = new ModelShotRenderer(egl.Gl);

        //Act
        var png = renderer.RenderPng(scene, stage, ShotAngle.Front, 160, 160);

        //Assert - some pixels are red-dominant (the model), others dark (the stage)
        using var bitmap = SKBitmap.Decode(png);
        var sawModel = false;
        var sawStage = false;
        for (var y = 0; y < bitmap.Height; y += 2)
        {
            for (var x = 0; x < bitmap.Width; x += 2)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel.Red > 120 && pixel.Green < 80 && pixel.Blue < 80) { sawModel = true; }
                if (pixel.Red < 90 && pixel.Green < 90 && pixel.Blue < 100) { sawStage = true; }
            }
        }

        sawModel.Should().BeTrue();
        sawStage.Should().BeTrue();
    }

    [Fact]
    public void every_named_angle_renders_without_error()
    {
        //Arrange
        using var egl = RequireGl();
        var stage = ShotStage.Tabletop();
        var scene = ShotSceneBuilder.Build(TestAssets.BuildTriangleModel(), stage);
        using var renderer = new ModelShotRenderer(egl.Gl);

        foreach (var angle in new[] { ShotAngle.Hero, ShotAngle.Front, ShotAngle.Side, ShotAngle.Back, ShotAngle.Top })
        {
            //Act
            var png = renderer.RenderPng(scene, stage, angle, 96, 96);

            //Assert
            png.Length.Should().BeGreaterThan(100);
        }
    }
}
