using SilverAssertions;
using System;
using Xunit;
using PalmVisualizer.Rendering;

namespace PalmVisualizer.Rendering.Tests;

public class FlowerMeadowTests
{
    private const float Aspect = 16f / 9f;

    //Advances palms and meadow together in frame-sized steps, checking every frame; returns the time reached
    private static float AdvanceSeconds(PalmMotion palms, FlowerMeadow meadow, float from, float seconds, Action<FlowerMeadow> eachFrame = null)
    {
        float time = from;
        for (; time <= from + seconds; time += 1f / 30f)
        {
            palms.Advance(time);
            meadow.Advance(palms, Aspect);
            eachFrame?.Invoke(meadow);
        }
        return time;
    }

    private static int Living(FlowerMeadow meadow)
    {
        var living = 0;
        for (int i = 0; i < meadow.Count; i++)
        {
            if (!meadow[i].Withering) { living++; }
        }
        return living;
    }

    [Fact]
    public void Calm_flowers_sprout_one_at_a_time()
    {
        //Arrange
        var palms = new PalmMotion(new PalmAttractorField());
        var meadow = new FlowerMeadow();

        //Act / Assert - the first flower, then the next only once the one before has bloomed
        float time = AdvanceSeconds(palms, meadow, 0f, 1f);
        meadow.Count.Should().Be(1);
        time = AdvanceSeconds(palms, meadow, time, FlowerMeadow.CalmBloomSeconds * 0.8f);
        meadow.Count.Should().Be(1);
        time = AdvanceSeconds(palms, meadow, time, FlowerMeadow.CalmBloomSeconds * 0.4f);
        meadow.Count.Should().Be(2);
        meadow[0].Growth.Should().Be(1f);
        AdvanceSeconds(palms, meadow, time, FlowerMeadow.CalmBloomSeconds * 2f);
        meadow.Count.Should().Be(4);
    }

    [Fact]
    public void Once_four_have_bloomed_each_new_flower_withers_the_oldest()
    {
        //Arrange
        var palms = new PalmMotion(new PalmAttractorField());
        var meadow = new FlowerMeadow();

        //Act / Assert - over a long calm run: never more than four living flowers, never more
        //  than one withering beside them, and always four once the meadow has filled
        float time = AdvanceSeconds(palms, meadow, 0f, FlowerMeadow.CalmBloomSeconds * 4f - 1f);
        AdvanceSeconds(palms, meadow, time, 240f, m =>
        {
            (Living(m) <= FlowerMeadow.CalmFlowers).Should().Be(true);
            (m.Count <= FlowerMeadow.CalmFlowers + 1).Should().Be(true);
            (m.Count >= FlowerMeadow.CalmFlowers).Should().Be(true);
        });
    }

    [Fact]
    public void Calm_flowers_are_spread_evenly_over_the_view()
    {
        //Arrange
        var palms = new PalmMotion(new PalmAttractorField());
        var meadow = new FlowerMeadow();

        //Act - long enough for several turnovers
        AdvanceSeconds(palms, meadow, 0f, 200f);

        //Assert - no two living flowers crowd each other
        for (int a = 0; a < meadow.Count; a++)
        {
            for (int b = a + 1; b < meadow.Count; b++)
            {
                if (meadow[a].Withering || meadow[b].Withering) { continue; }
                float dx = (meadow[a].X - meadow[b].X) * Aspect;
                float dy = meadow[a].Y - meadow[b].Y;
                (MathF.Sqrt((dx * dx) + (dy * dy)) > 0.25f).Should().Be(true);
            }
        }
    }

    [Fact]
    public void Every_flower_is_a_vivid_colour_of_the_rainbow_but_never_green()
    {
        //Arrange - a lush meadow, for many flowers
        var field = new PalmAttractorField();
        var palms = new PalmMotion(field);
        var meadow = new FlowerMeadow();
        field.SetTargets(new[] { new PalmAttractor(1, 0.25f, 0.5f), new PalmAttractor(2, 0.75f, 0.5f) });

        //Act
        AdvanceSeconds(palms, meadow, 0f, 20f);

        //Assert
        (meadow.Count > 20).Should().Be(true);
        for (int i = 0; i < meadow.Count; i++)
        {
            var f = meadow[i];
            (f.G <= MathF.Max(f.R, f.B)).Should().Be(true);
            (MathF.Max(f.R, MathF.Max(f.G, f.B)) >= 0.85f).Should().Be(true);
        }
    }

    [Fact]
    public void Two_palms_in_the_two_halves_light_the_whole_view()
    {
        //Arrange - two settled palms, as PalmMotion lays them out
        var palms = new float[PalmAttractorField.MaxAttractors * PalmMotion.Stride];
        palms[0] = 0.25f; palms[1] = 0.5f; palms[2] = 1f;
        palms[4] = 0.75f; palms[5] = 0.5f; palms[6] = 1f;

        //Act / Assert - every corner and edge of the view is above the withering threshold
        for (float y = 0f; y <= 1f; y += 0.1f)
        {
            for (float x = 0f; x <= 1f; x += 0.1f)
            {
                (FlowerMeadow.LightAt(x, y, palms, Aspect) > FlowerMeadow.DarkThreshold).Should().Be(true);
            }
        }
        (FlowerMeadow.LightAt(0.25f, 0.5f, palms, Aspect) > 0.95f).Should().Be(true);
    }

    [Fact]
    public void One_palm_alone_lights_about_a_quarter_of_the_view()
    {
        //Arrange - one settled palm in the middle
        var palms = new float[PalmAttractorField.MaxAttractors * PalmMotion.Stride];
        palms[0] = 0.5f; palms[1] = 0.5f; palms[2] = 1f;

        //Act - sample the view on a fine grid
        int lit = 0, all = 0;
        for (float y = 0.0125f; y < 1f; y += 0.025f)
        {
            for (float x = 0.0125f; x < 1f; x += 0.025f)
            {
                all++;
                if (FlowerMeadow.LightAt(x, y, palms, Aspect) > FlowerMeadow.DarkThreshold) { lit++; }
            }
        }

        //Assert
        float share = (float)lit / all;
        (share > 0.18f && share < 0.32f).Should().Be(true);
    }

    [Fact]
    public void The_light_reaches_further_as_a_second_palm_comes_into_view()
    {
        //Arrange
        var one = new float[PalmAttractorField.MaxAttractors * PalmMotion.Stride];
        one[2] = 1f;
        var half = (float[])one.Clone();
        half[6] = 0.5f;
        var two = (float[])one.Clone();
        two[6] = 1f;

        //Act
        var single = FlowerMeadow.LightReach(one);
        var between = FlowerMeadow.LightReach(half);
        var pair = FlowerMeadow.LightReach(two);

        //Assert - smoothly from a palm's own oval to the pair's much larger one
        (single.Across < between.Across && between.Across < pair.Across).Should().Be(true);
        (single.Down < between.Down && between.Down < pair.Down).Should().Be(true);
        (pair.Across * pair.Down > 4f * single.Across * single.Down).Should().Be(true);
        FlowerMeadow.LightReach(new float[one.Length]).Should().Be(single);
    }

    [Fact]
    public void One_palm_lights_its_side_and_leaves_the_far_side_dark()
    {
        //Arrange
        var palms = new float[PalmAttractorField.MaxAttractors * PalmMotion.Stride];
        palms[0] = 0.15f; palms[1] = 0.5f; palms[2] = 1f;

        //Act / Assert
        (FlowerMeadow.LightAt(0.17f, 0.5f, palms, Aspect) > 0.9f).Should().Be(true);
        (FlowerMeadow.LightAt(0.95f, 0.5f, palms, Aspect) < FlowerMeadow.DarkThreshold).Should().Be(true);
        (FlowerMeadow.LightAt(0.5f, 0.5f, new float[palms.Length], Aspect)).Should().Be(0f);
    }

    [Fact]
    public void Under_two_palms_the_meadow_grows_lush_and_nothing_withers()
    {
        //Arrange - a calm meadow of four
        var field = new PalmAttractorField();
        var palms = new PalmMotion(field);
        var meadow = new FlowerMeadow();
        float time = AdvanceSeconds(palms, meadow, 0f, 90f);
        int calm = meadow.Count;

        //Act - two palms light the whole view
        field.SetTargets(new[] { new PalmAttractor(1, 0.25f, 0.5f), new PalmAttractor(2, 0.75f, 0.5f) });
        time = AdvanceSeconds(palms, meadow, time, 2f);
        AdvanceSeconds(palms, meadow, time, 15f, m =>
        {
            for (int i = 0; i < m.Count; i++)
            {
                (m[i].Withering && m[i].Light > 0.5f && m[i].Wither < 0.85f).Should().Be(false);
            }
        });

        //Assert - many more flowers, and none withering
        (meadow.Count > calm + 15).Should().Be(true);
        Living(meadow).Should().Be(meadow.Count);
    }

    [Fact]
    public void Under_one_palm_the_far_side_withers()
    {
        //Arrange - a calm meadow of four, spread over the view
        var field = new PalmAttractorField();
        var palms = new PalmMotion(field);
        var meadow = new FlowerMeadow();
        float time = AdvanceSeconds(palms, meadow, 0f, 90f);

        //Act - one palm at the far left
        field.SetTargets(new[] { new PalmAttractor(1, 0.05f, 0.5f) });
        AdvanceSeconds(palms, meadow, time, 3f);

        //Assert - flowers in the dark right half wither; those in the palm's light do not
        var farWithering = 0;
        for (int i = 0; i < meadow.Count; i++)
        {
            var f = meadow[i];
            if (f.X > 0.75f && f.Wither > 0f) { farWithering++; }
            if (f.X < 0.25f && f.Light > 0.5f) { f.Withering.Should().Be(false); }
        }
        (farWithering > 0 || !HasFlowerBeyond(meadow, 0.75f)).Should().Be(true);
    }

    private static bool HasFlowerBeyond(FlowerMeadow meadow, float x)
    {
        for (int i = 0; i < meadow.Count; i++)
        {
            if (meadow[i].X > x) { return true; }
        }
        return false;
    }

    [Fact]
    public void When_the_palms_go_the_meadow_returns_to_four()
    {
        //Arrange - a lush meadow under two palms
        var field = new PalmAttractorField();
        var palms = new PalmMotion(field);
        var meadow = new FlowerMeadow();
        field.SetTargets(new[] { new PalmAttractor(1, 0.25f, 0.5f), new PalmAttractor(2, 0.75f, 0.5f) });
        float time = AdvanceSeconds(palms, meadow, 0f, 15f);
        (meadow.Count > 10).Should().Be(true);

        //Act - the palms go
        field.SetTargets(null);
        time = AdvanceSeconds(palms, meadow, time, 3f);

        //Assert - at most four keep living at once; the rest wither away in time
        (Living(meadow) <= FlowerMeadow.CalmFlowers).Should().Be(true);
        AdvanceSeconds(palms, meadow, time, FlowerMeadow.WitherSeconds * 2f);
        (meadow.Count <= FlowerMeadow.CalmFlowers + 1).Should().Be(true);
    }

    [Fact]
    public void WriteInstances_lays_out_the_flowers_back_to_front()
    {
        //Arrange
        var field = new PalmAttractorField();
        var palms = new PalmMotion(field);
        var meadow = new FlowerMeadow();
        field.SetTargets(new[] { new PalmAttractor(1, 0.5f, 0.5f) });
        AdvanceSeconds(palms, meadow, 0f, 6f);
        var data = new float[FlowerMeadow.MaxFlowers * FlowerMeadow.FloatsPerFlower];

        //Act
        int written = meadow.WriteInstances(data, 1280f, 720f);

        //Assert - higher in the view first, so lower flowers stand in front; sensible values
        written.Should().Be(meadow.Count);
        for (int i = 0; i < written; i++)
        {
            int at = i * FlowerMeadow.FloatsPerFlower;
            if (i > 0) { (data[at + 1] >= data[at + 1 - FlowerMeadow.FloatsPerFlower]).Should().Be(true); }
            (data[at + 2] > 0f && data[at + 2] < 720f * 0.12f).Should().Be(true);
            (data[at + 7] > data[at + 2]).Should().Be(true);
            for (int j = 8; j <= 11; j++) { (data[at + j] >= 0f && data[at + j] <= 1f).Should().Be(true); }
            (data[at + 12] >= 0.10f && data[at + 12] <= 0.45f).Should().Be(true);
            (data[at + 20] >= 0f && data[at + 20] <= 1f).Should().Be(true);
            (data[at + 21] >= 0.72f && data[at + 21] <= 1f).Should().Be(true);
        }
    }

    [Fact]
    public void Petals_range_from_narrow_to_broad()
    {
        //Arrange - a lush meadow, for many flowers
        var field = new PalmAttractorField();
        var palms = new PalmMotion(field);
        var meadow = new FlowerMeadow();
        field.SetTargets(new[] { new PalmAttractor(1, 0.25f, 0.5f), new PalmAttractor(2, 0.75f, 0.5f) });
        AdvanceSeconds(palms, meadow, 0f, 20f);
        var data = new float[FlowerMeadow.MaxFlowers * FlowerMeadow.FloatsPerFlower];

        //Act
        int written = meadow.WriteInstances(data, 1280f, 720f);

        //Assert - both narrow-petalled and broad-petalled flowers grow
        int narrow = 0, broad = 0;
        for (int i = 0; i < written; i++)
        {
            float width = data[(i * FlowerMeadow.FloatsPerFlower) + 12];
            if (width < 0.2f) { narrow++; }
            if (width > 0.35f) { broad++; }
        }
        (narrow > 0 && broad > 0).Should().Be(true);
    }

    [Fact]
    public void Advance_needs_the_palms()
    {
        Assert.Throws<ArgumentNullException>(() => new FlowerMeadow().Advance(null, 1f));
    }
}

// The WGSL compiles only on a WebGPU device, at run time; these pin what must never drift: the
// eight petals, the uniform layout, and the palm light the felt shows being the light the meadow uses.
public class FlowersWebGpuRendererTests
{
    [Fact]
    public void Every_flower_has_exactly_eight_even_petals()
    {
        FlowersWebGpuRenderer.FlowerWgsl.Should().Contain("const PETALS = 8.0;");
        FlowersWebGpuRenderer.FlowerWgsl.Should().Contain("let sector = 2.0 * PI / PETALS;");
    }

    [Fact]
    public void Both_shaders_hold_every_palm_and_every_flower()
    {
        int vectors = FlowerMeadow.MaxFlowers * FlowerMeadow.FloatsPerFlower / 4;
        foreach (var wgsl in new[] { FlowersWebGpuRenderer.FeltWgsl, FlowersWebGpuRenderer.FlowerWgsl })
        {
            wgsl.Should().Contain($"palms: array<vec4<f32>, {PalmAttractorField.MaxAttractors}>");
            wgsl.Should().Contain($"flowers: array<vec4<f32>, {vectors}>");
            wgsl.Should().Contain("@vertex fn vs(");
            wgsl.Should().Contain("@fragment fn fs(");
        }
        (FlowerMeadow.FloatsPerFlower % 4).Should().Be(0);
        FlowersWebGpuRenderer.FlowerWgsl.Should().Contain($"m.flowers[input.index * {FlowerMeadow.FloatsPerFlower / 4}u + 4u]");
    }

    [Fact]
    public void The_felt_is_lit_with_the_meadows_own_palm_light()
    {
        //The same oval as FlowerMeadow.LightAt, its reach handed over in the uniform block
        FlowersWebGpuRenderer.FeltWgsl.Should().Contain("reachAcross: f32,");
        FlowersWebGpuRenderer.FeltWgsl.Should().Contain("reachDown: f32,");
        FlowersWebGpuRenderer.FeltWgsl.Should().Contain(
            "let d = vec2<f32>((uv.x - palm.x) * aspect / m.reachAcross, (uv.y - palm.y) / m.reachDown);");
        FlowersWebGpuRenderer.FeltWgsl.Should().Contain("light = light + palm.z * exp(-dot(d, d));");
    }
}
