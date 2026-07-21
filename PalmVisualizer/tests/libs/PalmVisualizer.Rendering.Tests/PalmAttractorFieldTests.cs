using SilverAssertions;
using System;
using Xunit;
using PalmVisualizer.Rendering;

namespace PalmVisualizer.Rendering.Tests;

public class PalmAttractorFieldTests
{
    private static float SlotX(float[] state, int slot) => state[slot * 3];
    private static float SlotY(float[] state, int slot) => state[(slot * 3) + 1];
    private static float SlotStrength(float[] state, int slot) => state[(slot * 3) + 2];

    private static float[] Snapshot(PalmAttractorField field)
    {
        var state = new float[PalmAttractorField.MaxAttractors * 3];
        field.CopyState(state);
        return state;
    }

    private static void StepSeconds(PalmAttractorField field, float seconds)
    {
        //Step in render-frame-sized increments, the way the backdrop drives the field
        var stepped = 0f;
        while (stepped < seconds)
        {
            field.Step(1f / 60f);
            stepped += 1f / 60f;
        }
    }

    [Fact]
    public void New_field_has_no_influence()
    {
        //Arrange
        var field = new PalmAttractorField();

        //Act
        float[] state = Snapshot(field);

        //Assert
        for (int slot = 0; slot < PalmAttractorField.MaxAttractors; slot++)
        {
            SlotStrength(state, slot).Should().Be(0f);
        }
    }

    [Fact]
    public void A_new_palm_fades_in_at_its_own_position()
    {
        //Arrange
        var field = new PalmAttractorField();

        //Act
        field.SetTargets(new[] { new PalmAttractor(1, 0.7f, 0.3f) });
        float[] justSet = Snapshot(field);
        StepSeconds(field, 0.1f);
        float[] shortlyAfter = Snapshot(field);
        StepSeconds(field, 3f);
        float[] settled = Snapshot(field);

        //Assert - the slot appears AT the palm (no sweep from stale state), swells smoothly,
        //  and settles at full strength
        SlotX(justSet, 0).Should().Be(0.7f);
        SlotY(justSet, 0).Should().Be(0.3f);
        SlotStrength(justSet, 0).Should().Be(0f);
        (SlotStrength(shortlyAfter, 0) > 0f).Should().Be(true);
        (SlotStrength(shortlyAfter, 0) < 1f).Should().Be(true);
        (SlotStrength(settled, 0) > 0.98f).Should().Be(true);
    }

    [Fact]
    public void A_released_palm_fades_out_and_frees_its_slot()
    {
        //Arrange - a fully swelled-in palm
        var field = new PalmAttractorField();
        field.SetTargets(new[] { new PalmAttractor(1, 0.5f, 0.5f) });
        StepSeconds(field, 3f);

        //Act - release it (an empty update = no open palms)
        field.SetTargets(Array.Empty<PalmAttractor>());
        StepSeconds(field, 0.1f);
        float[] fading = Snapshot(field);
        StepSeconds(field, 5f);
        float[] gone = Snapshot(field);

        //Assert - the influence melts away rather than snapping off, then the slot frees
        (SlotStrength(fading, 0) > 0f).Should().Be(true);
        (SlotStrength(fading, 0) < 1f).Should().Be(true);
        SlotStrength(gone, 0).Should().Be(0f);
    }

    [Fact]
    public void A_moving_palm_glides_toward_its_target_without_refading()
    {
        //Arrange - a settled palm at the left
        var field = new PalmAttractorField();
        field.SetTargets(new[] { new PalmAttractor(1, 0.2f, 0.5f) });
        StepSeconds(field, 3f);

        //Act - the same palm (same id) jumps to the right; step a partial glide
        field.SetTargets(new[] { new PalmAttractor(1, 0.8f, 0.5f) });
        StepSeconds(field, 0.1f);
        float[] gliding = Snapshot(field);

        //Assert - position is on its way (neither old nor new), strength never dipped
        (SlotX(gliding, 0) > 0.2f).Should().Be(true);
        (SlotX(gliding, 0) < 0.8f).Should().Be(true);
        (SlotStrength(gliding, 0) > 0.98f).Should().Be(true);
    }

    [Fact]
    public void A_reopened_palm_reattaches_to_its_fading_slot()
    {
        //Arrange - a settled palm that has just started fading out
        var field = new PalmAttractorField();
        field.SetTargets(new[] { new PalmAttractor(7, 0.4f, 0.6f) });
        StepSeconds(field, 3f);
        field.SetTargets(Array.Empty<PalmAttractor>());
        StepSeconds(field, 0.2f);
        float[] fading = Snapshot(field);

        //Act - the same id comes back (hand briefly closed, then reopened)
        field.SetTargets(new[] { new PalmAttractor(7, 0.4f, 0.6f) });
        StepSeconds(field, 0.2f);
        float[] recovered = Snapshot(field);

        //Assert - the influence swells back from where the fade left it (no restart at zero)
        (SlotStrength(fading, 0) > 0f).Should().Be(true);
        (SlotStrength(recovered, 0) > SlotStrength(fading, 0)).Should().Be(true);
    }

    [Fact]
    public void Palms_beyond_capacity_are_ignored()
    {
        //Arrange
        var field = new PalmAttractorField();
        var palms = new PalmAttractor[PalmAttractorField.MaxAttractors + 2];
        for (int i = 0; i < palms.Length; i++)
        {
            palms[i] = new PalmAttractor(i + 1, 0.1f * (i + 1), 0.5f);
        }

        //Act
        field.SetTargets(palms);
        StepSeconds(field, 3f);
        float[] state = Snapshot(field);

        //Assert - every slot is in use, and no more than every slot
        for (int slot = 0; slot < PalmAttractorField.MaxAttractors; slot++)
        {
            (SlotStrength(state, slot) > 0.9f).Should().Be(true);
        }
    }

    [Fact]
    public void Reset_releases_everything_instantly()
    {
        //Arrange
        var field = new PalmAttractorField();
        field.SetTargets(new[] { new PalmAttractor(1, 0.5f, 0.5f), new PalmAttractor(2, 0.7f, 0.2f) });
        StepSeconds(field, 3f);

        //Act
        field.Reset();
        float[] state = Snapshot(field);

        //Assert
        for (int slot = 0; slot < PalmAttractorField.MaxAttractors; slot++)
        {
            SlotStrength(state, slot).Should().Be(0f);
        }
    }

    [Fact]
    public void CopyState_rejects_a_short_buffer()
    {
        //Arrange
        var field = new PalmAttractorField();

        //Act / Assert
        Assert.Throws<ArgumentException>(() => field.CopyState(new float[3]));
        Assert.Throws<ArgumentException>(() => field.CopyState(null));
    }

    [Fact]
    public void Null_and_empty_targets_mean_the_same_thing()
    {
        //Arrange
        var field = new PalmAttractorField();
        field.SetTargets(new[] { new PalmAttractor(1, 0.5f, 0.5f) });
        StepSeconds(field, 3f);

        //Act
        field.SetTargets(null);
        StepSeconds(field, 5f);
        float[] state = Snapshot(field);

        //Assert
        SlotStrength(state, 0).Should().Be(0f);
    }
}
