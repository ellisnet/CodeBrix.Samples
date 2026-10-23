using SilverAssertions;
using System;
using Xunit;
using PalmVisualizer.Rendering;

namespace PalmVisualizer.Rendering.Tests;

public class HolyInannaMotionTests
{
    //Advances palms and star together in frame-sized steps; returns the time reached
    private static float AdvanceSeconds(PalmMotion palms, HolyInannaMotion star, float from, float seconds)
    {
        float time = from;
        for (; time <= from + seconds; time += 1f / 60f)
        {
            palms.Advance(time);
            star.Advance(palms);
        }
        return time;
    }

    [Fact]
    public void Without_palms_the_star_keeps_to_its_own_path_and_recedes()
    {
        //Arrange
        var palms = new PalmMotion(new PalmAttractorField());
        var star = new HolyInannaMotion();

        //Act
        AdvanceSeconds(palms, star, 0f, 20f);

        //Assert - it follows its path closely (trailing it only by the slow easing)
        (Math.Abs(star.CenterX - star.PathPosition.X) < 0.06f).Should().Be(true);
        (Math.Abs(star.CenterY - star.PathPosition.Y) < 0.06f).Should().Be(true);
        (Math.Abs(star.Scale - HolyInannaMotion.CalmScale) < 0.001f).Should().Be(true);
    }

    [Fact]
    public void Palms_can_never_draw_the_star_far_from_its_path()
    {
        //Arrange - four palms in a far corner, waving hard, for a long time
        var field = new PalmAttractorField();
        var palms = new PalmMotion(field);
        var star = new HolyInannaMotion();
        float time = 0f;

        //Act / Assert - at every moment the star stays within the pull limit of its own path
        for (int wave = 0; wave < 60; wave++)
        {
            float x = wave % 2 == 0 ? 0.02f : 0.12f;
            field.SetTargets(new[]
            {
                new PalmAttractor(1, x, 0.02f), new PalmAttractor(2, x, 0.05f),
                new PalmAttractor(3, x + 0.03f, 0.02f), new PalmAttractor(4, x + 0.03f, 0.05f),
            });
            time = AdvanceSeconds(palms, star, time, 0.25f);
            (Math.Abs(star.CenterX - star.PathPosition.X) <= HolyInannaMotion.MaxPalmPull + 0.02f).Should().Be(true);
            (Math.Abs(star.CenterY - star.PathPosition.Y) <= HolyInannaMotion.MaxPalmPull + 0.02f).Should().Be(true);
        }
    }

    [Fact]
    public void An_open_palm_draws_the_star_gently_toward_it()
    {
        //Arrange
        var field = new PalmAttractorField();
        var palms = new PalmMotion(field);
        var star = new HolyInannaMotion();
        float time = AdvanceSeconds(palms, star, 0f, 1f);

        //Act - a palm appears to the right of wherever the star is heading
        field.SetTargets(new[] { new PalmAttractor(1, 0.98f, star.PathPosition.Y) });
        float startX = star.CenterX;
        palms.Advance(time + (1f / 60f));
        star.Advance(palms);
        float firstStep = star.CenterX - startX;
        AdvanceSeconds(palms, star, time + (2f / 60f), 6f);

        //Assert - it barely moves in one frame, but in time it has come right of its own path
        (Math.Abs(firstStep) < 0.01f).Should().Be(true);
        (star.CenterX > star.PathPosition.X + 0.05f).Should().Be(true);
    }

    [Fact]
    public void The_star_comes_closer_with_activity_and_spins_on()
    {
        //Arrange
        var field = new PalmAttractorField();
        var palms = new PalmMotion(field);
        var star = new HolyInannaMotion();
        field.SetTargets(new[] { new PalmAttractor(1, 0.4f, 0.5f), new PalmAttractor(2, 0.6f, 0.5f) });

        //Act
        AdvanceSeconds(palms, star, 0f, 10f);

        //Assert
        (star.Scale > HolyInannaMotion.CalmScale + 0.3f).Should().Be(true);
        (star.Scale <= HolyInannaMotion.ActiveScale).Should().Be(true);
        (star.Spin > 0f && star.Spin < 2f * MathF.PI).Should().Be(true);
        (Math.Abs(star.TiltX) <= 0.36f && Math.Abs(star.TiltY) <= 0.30f).Should().Be(true);
    }

    [Fact]
    public void Advance_needs_the_palms()
    {
        Assert.Throws<ArgumentNullException>(() => new HolyInannaMotion().Advance(null));
    }
}

public class HolyInannaWebGpuRendererTests
{
    [Fact]
    public void The_view_to_star_rotation_is_a_rotation()
    {
        //Arrange
        var rows = new float[12];

        //Act
        HolyInannaWebGpuRenderer.WriteViewToStar(rows, 0.3f, -0.25f, 1.1f);

        //Assert - unit rows, perpendicular to each other, no fourth component
        for (int a = 0; a < 3; a++)
        {
            rows[(a * 4) + 3].Should().Be(0f);
            for (int b = 0; b < 3; b++)
            {
                float dot = (rows[a * 4] * rows[b * 4]) + (rows[(a * 4) + 1] * rows[(b * 4) + 1]) + (rows[(a * 4) + 2] * rows[(b * 4) + 2]);
                (Math.Abs(dot - (a == b ? 1f : 0f)) < 1e-5f).Should().Be(true);
            }
        }
    }

    [Fact]
    public void An_untilted_star_only_spins_about_the_line_of_sight()
    {
        //Arrange
        var rows = new float[12];

        //Act - a quarter turn, no tilt
        HolyInannaWebGpuRenderer.WriteViewToStar(rows, 0f, 0f, MathF.PI / 2f);

        //Assert - the line of sight is the star's own axis, and view x lands on star -y
        (Math.Abs(rows[10] - 1f) < 1e-5f).Should().Be(true);
        (Math.Abs(rows[0]) < 1e-5f).Should().Be(true);
        (Math.Abs(rows[4] + 1f) < 1e-5f).Should().Be(true);
    }

    [Fact]
    public void The_scene_holds_every_palm_and_has_the_entry_points_the_pipeline_names()
    {
        HolyInannaWebGpuRenderer.SceneWgsl.Should().Contain($"palms: array<vec4<f32>, {PalmAttractorField.MaxAttractors}>");
        HolyInannaWebGpuRenderer.SceneWgsl.Should().Contain("@vertex fn vs(");
        HolyInannaWebGpuRenderer.SceneWgsl.Should().Contain("@fragment fn fs(");
        HolyInannaWebGpuRenderer.SceneWgsl.Should().Contain("const INNER = 0.426;");
    }
}
