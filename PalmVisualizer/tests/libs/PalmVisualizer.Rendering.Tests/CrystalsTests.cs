using SilverAssertions;
using System;
using Xunit;
using PalmVisualizer.Rendering;

namespace PalmVisualizer.Rendering.Tests;

public class CrystalGardenTests
{
    //Advances palms and garden together in frame-sized steps; returns the time reached
    private static float AdvanceSeconds(PalmMotion palms, CrystalGarden garden, float from, float seconds)
    {
        float time = from;
        for (; time <= from + seconds; time += 1f / 60f)
        {
            palms.Advance(time);
            garden.Advance(palms, 16f / 9f);
        }
        return time;
    }

    [Fact]
    public void A_new_garden_already_has_young_crystals()
    {
        var garden = new CrystalGarden();

        (garden.Count >= 12).Should().Be(true);
        for (int i = 0; i < garden.Count; i++)
        {
            (garden[i].Growth < 1f).Should().Be(true);
            (garden[i].X >= 0f && garden[i].X <= 1f && garden[i].Y >= 0f && garden[i].Y <= 1f).Should().Be(true);
        }
    }

    [Fact]
    public void Crystals_keep_sprouting_and_growing_but_never_overfill_the_garden()
    {
        //Arrange
        var palms = new PalmMotion(new PalmAttractorField());
        var garden = new CrystalGarden();
        int start = garden.Count;

        //Act / Assert - over a long calm run the garden fills, turns over, and stays within its cap
        float time = 0f;
        for (int minute = 0; minute < 4; minute++)
        {
            time = AdvanceSeconds(palms, garden, time, 60f);
            (garden.Count <= CrystalGarden.MaxCrystals).Should().Be(true);
        }
        (garden.Count > start).Should().Be(true);

        var grown = 0;
        for (int i = 0; i < garden.Count; i++)
        {
            if (garden[i].Growth >= 1f) { grown++; }
        }
        (grown > 0).Should().Be(true);
    }

    [Fact]
    public void Old_crystals_fade_away_to_make_room()
    {
        //Arrange
        var palms = new PalmMotion(new PalmAttractorField());
        var garden = new CrystalGarden();
        var first = garden[0];

        //Act - long past every first crystal's life
        AdvanceSeconds(palms, garden, 0f, 120f);

        //Assert - none of the garden's first crystals is still the oldest one standing
        (garden[0].X != first.X || garden[0].Y != first.Y).Should().Be(true);
    }

    [Fact]
    public void A_palm_excites_the_crystals_near_it_and_they_grow_faster()
    {
        //Arrange - two gardens grown identically, one with a palm held over its oldest crystal
        var calmPalms = new PalmMotion(new PalmAttractorField());
        var calm = new CrystalGarden();
        var field = new PalmAttractorField();
        var excitedPalms = new PalmMotion(field);
        var excited = new CrystalGarden();
        field.SetTargets(new[] { new PalmAttractor(1, excited[0].X, excited[0].Y) });

        //Act
        AdvanceSeconds(calmPalms, calm, 0f, 2f);
        AdvanceSeconds(excitedPalms, excited, 0f, 2f);

        //Assert
        calm[0].Excitement.Should().Be(0f);
        (excited[0].Excitement > 0.5f).Should().Be(true);
        (excited[0].Growth > calm[0].Growth).Should().Be(true);
    }

    [Fact]
    public void Crystals_sprout_where_the_palms_are()
    {
        //Arrange - a palm in a corner
        var field = new PalmAttractorField();
        var palms = new PalmMotion(field);
        var garden = new CrystalGarden();
        int start = garden.Count;
        field.SetTargets(new[] { new PalmAttractor(1, 0.9f, 0.1f) });

        //Act
        AdvanceSeconds(palms, garden, 0f, 6f);

        //Assert - some of the new crystals stand near the palm
        var nearby = 0;
        for (int i = start; i < garden.Count; i++)
        {
            float dx = garden[i].X - 0.9f;
            float dy = garden[i].Y - 0.1f;
            if ((dx * dx) + (dy * dy) < 0.1f * 0.1f) { nearby++; }
        }
        (nearby >= 3).Should().Be(true);
    }

    [Fact]
    public void WriteInstances_lays_out_each_crystal_for_the_shader()
    {
        //Arrange
        var garden = new CrystalGarden();
        var data = new float[CrystalGarden.MaxCrystals * CrystalGarden.FloatsPerCrystal];

        //Act
        int written = garden.WriteInstances(data, 1280f, 720f);

        //Assert
        written.Should().Be(garden.Count);
        for (int i = 0; i < written; i++)
        {
            int at = i * CrystalGarden.FloatsPerCrystal;
            (data[at] >= 0f && data[at] <= 1280f).Should().Be(true);
            (data[at + 1] >= 0f && data[at + 1] <= 720f).Should().Be(true);
            (data[at + 3] >= 1f && data[at + 3] <= 720f * 0.25f).Should().Be(true);
            (data[at + 4] >= 1f && data[at + 4] < data[at + 3]).Should().Be(true);
            (data[at + 6] >= 0.80f && data[at + 6] <= 1f).Should().Be(true);
            (data[at + 7] >= 0f && data[at + 7] <= 1f).Should().Be(true);
        }
    }

    [Fact]
    public void Every_crystal_grows_upward_within_forty_degrees_of_vertical()
    {
        //Arrange - a long run, with a palm sprouting crystals of its own
        var field = new PalmAttractorField();
        var palms = new PalmMotion(field);
        var garden = new CrystalGarden();
        field.SetTargets(new[] { new PalmAttractor(1, 0.3f, 0.6f) });
        AdvanceSeconds(palms, garden, 0f, 30f);
        var data = new float[CrystalGarden.MaxCrystals * CrystalGarden.FloatsPerCrystal];

        //Act
        int written = garden.WriteInstances(data, 1280f, 720f);

        //Assert - in pixel space y grows downward, so "up" is an angle of -90 degrees
        for (int i = 0; i < written; i++)
        {
            float offVertical = MathF.Abs(data[(i * CrystalGarden.FloatsPerCrystal) + 2] + (MathF.PI / 2f));
            (offVertical <= 0.70f).Should().Be(true);
        }
    }

    [Fact]
    public void Calm_crystals_stay_close_to_their_palette_colours()
    {
        //Arrange
        var garden = new CrystalGarden();
        var data = new float[CrystalGarden.MaxCrystals * CrystalGarden.FloatsPerCrystal];

        //Act
        int written = garden.WriteInstances(data, 1280f, 720f);

        //Assert - each hue lies within a small drift of one of the five palette colours (i / 5)
        for (int i = 0; i < written; i++)
        {
            float hue = data[(i * CrystalGarden.FloatsPerCrystal) + 5];
            float nearest = MathF.Round(hue * 5f) / 5f;
            (MathF.Abs(hue - nearest) <= 0.03f).Should().Be(true);
        }
    }

    [Fact]
    public void Advance_needs_the_palms()
    {
        Assert.Throws<ArgumentNullException>(() => new CrystalGarden().Advance(null, 1f));
    }
}

// The WGSL compiles only on a WebGPU device, at run time; these pin the shaders' uniform block
// to what the C# side writes.
public class CrystalsWebGpuRendererTests
{
    [Fact]
    public void Both_shaders_hold_every_palm_and_every_crystal()
    {
        int vectors = CrystalGarden.MaxCrystals * CrystalGarden.FloatsPerCrystal / 4;
        foreach (var wgsl in new[] { CrystalsWebGpuRenderer.BackdropWgsl, CrystalsWebGpuRenderer.CrystalWgsl })
        {
            wgsl.Should().Contain($"palms: array<vec4<f32>, {PalmAttractorField.MaxAttractors}>");
            wgsl.Should().Contain($"crystals: array<vec4<f32>, {vectors}>");
            wgsl.Should().Contain("@vertex fn vs(");
            wgsl.Should().Contain("@fragment fn fs(");
        }
        (CrystalGarden.FloatsPerCrystal % 4).Should().Be(0);
    }

    [Fact]
    public void Each_crystal_is_an_instanced_quad_read_as_three_vectors()
    {
        CrystalsWebGpuRenderer.CrystalWgsl.Should().Contain("@builtin(instance_index) instance: u32");
        CrystalsWebGpuRenderer.CrystalWgsl.Should().Contain("g.crystals[instance * 3u]");
        CrystalsWebGpuRenderer.CrystalWgsl.Should().Contain("g.crystals[input.index * 3u + 2u]");
        (CrystalGarden.FloatsPerCrystal / 4).Should().Be(3);
    }
}
