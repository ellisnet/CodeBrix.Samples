using SilverAssertions;
using System;
using Xunit;
using PalmVisualizer.Rendering;

namespace PalmVisualizer.Rendering.Tests;

public class EtherealSceneTests
{
    private static float StarX(EtherealScene scene, int star) => scene.Stars[star * EtherealScene.StarStride];
    private static float StarY(EtherealScene scene, int star) => scene.Stars[(star * EtherealScene.StarStride) + 1];
    private static float StarRadius(EtherealScene scene, int star) => scene.Stars[(star * EtherealScene.StarStride) + 2];
    private static float StarAlpha(EtherealScene scene, int star) => scene.Stars[(star * EtherealScene.StarStride) + 3];

    //Advances in render-frame-sized steps, so the attractor smoothing settles the way it does at run time
    private static void AdvanceSeconds(EtherealScene scene, float from, float seconds, float width, float height)
    {
        for (float time = from; time <= from + seconds; time += 1f / 60f)
        {
            scene.Advance(time, width, height, 1f);
        }
    }

    [Fact]
    public void Undisturbed_stars_lie_inside_the_surface_with_their_base_brightness()
    {
        //Arrange
        var scene = new EtherealScene(new PalmAttractorField());

        //Act
        scene.Advance(12.5f, 640f, 360f, 1f);

        //Assert
        scene.Stars.Length.Should().Be(EtherealScene.StarCount * EtherealScene.StarStride);
        for (int i = 0; i < EtherealScene.StarCount; i++)
        {
            (StarX(scene, i) >= 0f && StarX(scene, i) < 640f).Should().Be(true);
            (StarY(scene, i) >= 0f && StarY(scene, i) < 360f).Should().Be(true);
            (StarRadius(scene, i) >= 0.75f && StarRadius(scene, i) <= 3f).Should().Be(true);
            (StarAlpha(scene, i) >= 90f / 255f && StarAlpha(scene, i) <= 1f).Should().Be(true);
        }
    }

    [Fact]
    public void The_undisturbed_frame_is_a_pure_function_of_time()
    {
        //Arrange - two scenes, one of which has been running a while
        var fresh = new EtherealScene(new PalmAttractorField());
        var running = new EtherealScene(new PalmAttractorField());
        AdvanceSeconds(running, 0f, 2f, 640f, 360f);

        //Act
        fresh.Advance(7f, 640f, 360f, 1f);
        running.Advance(7f, 640f, 360f, 1f);

        //Assert - with no palms the stars depend only on the time
        fresh.Stars.ToArray().Should().Equal(running.Stars.ToArray());
    }

    [Fact]
    public void Star_sizes_follow_the_display_scale()
    {
        //Arrange
        var standard = new EtherealScene(new PalmAttractorField());
        var doubled = new EtherealScene(new PalmAttractorField());

        //Act
        standard.Advance(4f, 640f, 360f, 1f);
        doubled.Advance(4f, 640f, 360f, 2f);

        //Assert
        for (int i = 0; i < EtherealScene.StarCount; i++)
        {
            StarRadius(doubled, i).Should().Be(StarRadius(standard, i) * 2f);
        }
    }

    [Fact]
    public void Palms_reach_the_shader_in_its_aspect_corrected_space()
    {
        //Arrange - one palm right of center, settled at full strength
        var field = new PalmAttractorField();
        field.SetTargets(new[] { new PalmAttractor(1, 0.75f, 0.25f) });
        var scene = new EtherealScene(field);

        //Act
        AdvanceSeconds(scene, 0f, 3f, 800f, 400f);

        //Assert - x is stretched by the 2:1 aspect, y keeps -1..1 (down is positive)
        (Math.Abs(scene.Palms[0] - 1.0f) < 0.001f).Should().Be(true);
        (Math.Abs(scene.Palms[1] - -0.5f) < 0.001f).Should().Be(true);
        (scene.Palms[2] > 0.98f).Should().Be(true);
        for (int slot = 1; slot < PalmAttractorField.MaxAttractors; slot++)
        {
            scene.Palms[(slot * EtherealScene.PalmStride) + 2].Should().Be(0f);
        }
    }

    [Fact]
    public void An_open_palm_draws_the_stars_toward_it_and_brightens_them()
    {
        //Arrange - the same frame with and without a palm in the middle
        var field = new PalmAttractorField();
        var withPalm = new EtherealScene(field);
        var without = new EtherealScene(new PalmAttractorField());
        field.SetTargets(new[] { new PalmAttractor(1, 0.5f, 0.5f) });
        AdvanceSeconds(withPalm, 0f, 3f, 640f, 360f);

        //Act
        withPalm.Advance(5f, 640f, 360f, 1f);
        without.Advance(5f, 640f, 360f, 1f);

        //Assert - on average the stars sit closer to the palm and shine brighter
        double pulledDistance = 0, freeDistance = 0, pulledAlpha = 0, freeAlpha = 0;
        for (int i = 0; i < EtherealScene.StarCount; i++)
        {
            pulledDistance += Math.Sqrt(Math.Pow(StarX(withPalm, i) - 320f, 2) + Math.Pow(StarY(withPalm, i) - 180f, 2));
            freeDistance += Math.Sqrt(Math.Pow(StarX(without, i) - 320f, 2) + Math.Pow(StarY(without, i) - 180f, 2));
            pulledAlpha += StarAlpha(withPalm, i);
            freeAlpha += StarAlpha(without, i);
        }
        (pulledDistance < freeDistance).Should().Be(true);
        (pulledAlpha > freeAlpha).Should().Be(true);
    }

    [Fact]
    public void A_surface_with_no_size_computes_nothing()
    {
        //Arrange
        var scene = new EtherealScene(new PalmAttractorField());

        //Act
        scene.Advance(1f, 0f, 0f, 1f);

        //Assert
        scene.Width.Should().Be(0f);
        StarRadius(scene, 0).Should().Be(0f);
    }

    [Fact]
    public void A_scene_needs_an_attractor_field()
    {
        //Act / Assert
        Assert.Throws<ArgumentNullException>(() => new EtherealScene(null));
    }
}
