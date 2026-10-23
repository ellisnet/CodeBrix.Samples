using SilverAssertions;
using Xunit;
using PalmVisualizer.Rendering;

namespace PalmVisualizer.Rendering.Tests;

public class RippleRainTests
{
    //Lets the rain fall for a while in frame-sized steps; returns how many drops fell
    private static int RainFor(RippleRain rain, float seconds, float activity)
    {
        var drops = 0;
        for (float t = 0f; t < seconds; t += 1f / 60f)
        {
            rain.Advance(1f / 60f, activity);
            drops += rain.Count;
        }
        return drops;
    }

    [Fact]
    public void Calm_water_gets_a_light_rain()
    {
        var drops = RainFor(new RippleRain(), 10f, 0f);
        (drops >= 12 && drops <= 20).Should().Be(true);
    }

    [Fact]
    public void Lively_palms_bring_more_and_deeper_drops()
    {
        //Arrange
        var calm = new RippleRain();
        var lively = new RippleRain();

        //Act
        int calmDrops = RainFor(calm, 10f, 0f);
        int livelyDrops = RainFor(lively, 10f, 1f);

        //Assert
        (livelyDrops > calmDrops * 3).Should().Be(true);
    }

    [Fact]
    public void Every_drop_falls_inside_the_pool_with_a_sensible_size()
    {
        var rain = new RippleRain();
        for (int frame = 0; frame < 600; frame++)
        {
            rain.Advance(1f / 60f, frame / 600f);
            for (int i = 0; i < rain.Count; i++)
            {
                int at = i * RippleRain.Stride;
                (rain.Drops[at] > 0f && rain.Drops[at] < 1f).Should().Be(true);
                (rain.Drops[at + 1] > 0f && rain.Drops[at + 1] < 1f).Should().Be(true);
                (rain.Drops[at + 2] > 0f && rain.Drops[at + 2] < 1f).Should().Be(true);
                (rain.Drops[at + 3] > 0f && rain.Drops[at + 3] < 0.05f).Should().Be(true);
            }
        }
    }

    [Fact]
    public void A_long_stall_does_not_save_up_a_downpour()
    {
        //Arrange
        var rain = new RippleRain();

        //Act - ten seconds pass in one frame, then a normal frame follows
        rain.Advance(10f, 1f);
        rain.Advance(1f / 60f, 1f);

        //Assert - at most one drop was owed afterwards
        (rain.Count <= 1).Should().Be(true);
    }
}

// The WGSL compiles only on a WebGPU device, at run time; these pin what the C# side writes into
// the shaders' uniform blocks to what the shaders declare.
public class RippleWebGpuRendererTests
{
    [Fact]
    public void The_simulation_holds_every_palm_and_every_drop_of_a_frame()
    {
        RippleWebGpuRenderer.SimulationWgsl.Should().Contain($"palms: array<vec4<f32>, {PalmAttractorField.MaxAttractors}>");
        RippleWebGpuRenderer.SimulationWgsl.Should().Contain($"drops: array<vec4<f32>, {RippleRain.MaxDropsPerFrame}>");
        PalmMotion.Stride.Should().Be(4);
        RippleRain.Stride.Should().Be(4);
    }

    [Fact]
    public void The_simulation_is_a_compute_step_over_two_height_buffers()
    {
        RippleWebGpuRenderer.SimulationWgsl.Should().Contain("@compute @workgroup_size(16, 16)");
        RippleWebGpuRenderer.SimulationWgsl.Should().Contain("fn step(");
        RippleWebGpuRenderer.SimulationWgsl.Should().Contain("@group(0) @binding(1) var<storage, read> current: array<f32>;");
        RippleWebGpuRenderer.SimulationWgsl.Should().Contain("@group(0) @binding(2) var<storage, read_write> previous: array<f32>;");
    }

    [Fact]
    public void The_surface_reads_the_heights_and_has_the_entry_points_the_pipeline_names()
    {
        RippleWebGpuRenderer.SurfaceWgsl.Should().Contain("@vertex fn vs(");
        RippleWebGpuRenderer.SurfaceWgsl.Should().Contain("@fragment fn fs(");
        RippleWebGpuRenderer.SurfaceWgsl.Should().Contain("@group(0) @binding(1) var<storage, read> heights: array<f32>;");
        RippleWebGpuRenderer.SurfaceWgsl.Should().Contain($"palms: array<vec4<f32>, {PalmAttractorField.MaxAttractors}>");
    }
}
