using SilverAssertions;
using System;
using Xunit;
using PalmVisualizer.Rendering;

namespace PalmVisualizer.Rendering.Tests;

public class PalmMotionTests
{
    private static float Strength(PalmMotion motion, int slot) => motion.Palms[(slot * PalmMotion.Stride) + 2];
    private static float Speed(PalmMotion motion, int slot) => motion.Palms[(slot * PalmMotion.Stride) + 3];

    //Advances in render-frame-sized steps from the given time; returns the time reached
    private static float AdvanceSeconds(PalmMotion motion, float from, float seconds)
    {
        float time = from;
        for (; time <= from + seconds; time += 1f / 60f)
        {
            motion.Advance(time);
        }
        return time;
    }

    [Fact]
    public void No_palms_means_no_activity()
    {
        //Arrange
        var motion = new PalmMotion(new PalmAttractorField());

        //Act
        AdvanceSeconds(motion, 0f, 2f);

        //Assert
        motion.Activity.Should().Be(0f);
        for (int slot = 0; slot < PalmAttractorField.MaxAttractors; slot++)
        {
            Strength(motion, slot).Should().Be(0f);
            Speed(motion, slot).Should().Be(0f);
        }
    }

    [Fact]
    public void An_open_palm_raises_the_activity_and_a_moving_one_raises_it_further()
    {
        //Arrange - one palm held still
        var field = new PalmAttractorField();
        var motion = new PalmMotion(field);
        field.SetTargets(new[] { new PalmAttractor(1, 0.3f, 0.5f) });
        float time = AdvanceSeconds(motion, 0f, 4f);
        float still = motion.Activity;

        //Act - the same palm sweeps back and forth across the view
        for (int sweep = 0; sweep < 8; sweep++)
        {
            field.SetTargets(new[] { new PalmAttractor(1, sweep % 2 == 0 ? 0.7f : 0.3f, 0.5f) });
            time = AdvanceSeconds(motion, time, 0.25f);
        }

        //Assert
        (still > 0.5f).Should().Be(true);
        (Speed(motion, 0) > 0.2f).Should().Be(true);
        (motion.Activity > still).Should().Be(true);
        (motion.Activity <= 1f).Should().Be(true);
    }

    [Fact]
    public void Activity_falls_slowly_after_the_palms_close()
    {
        //Arrange - a lively scene
        var field = new PalmAttractorField();
        var motion = new PalmMotion(field);
        field.SetTargets(new[] { new PalmAttractor(1, 0.5f, 0.5f), new PalmAttractor(2, 0.2f, 0.4f) });
        float time = AdvanceSeconds(motion, 0f, 4f);
        float lively = motion.Activity;

        //Act - the palms close
        field.SetTargets(null);
        time = AdvanceSeconds(motion, time, 0.5f);
        float soonAfter = motion.Activity;
        AdvanceSeconds(motion, time, 15f);

        //Assert - it eases down rather than snapping, and settles near calm
        (soonAfter < lively).Should().Be(true);
        (soonAfter > lively * 0.5f).Should().Be(true);
        (motion.Activity < 0.02f).Should().Be(true);
    }

    [Fact]
    public void The_first_frame_has_no_delta_and_a_stall_is_clamped()
    {
        //Arrange
        var motion = new PalmMotion(new PalmAttractorField());

        //Act / Assert
        motion.Advance(5f);
        motion.DeltaTime.Should().Be(0f);
        motion.Advance(9f);
        motion.DeltaTime.Should().Be(0.1f);
        motion.Time.Should().Be(9f);
    }

    [Fact]
    public void Motion_needs_an_attractor_field()
    {
        Assert.Throws<ArgumentNullException>(() => new PalmMotion(null));
    }
}
