using SilverAssertions;
using Xunit;
using PalmVisualizer.Rendering;

namespace PalmVisualizer.Rendering.Tests;

// The WGSL itself compiles only on a WebGPU device, at run time; these tests pin what the C# side
// writes into the shaders' uniform blocks to what the shaders declare, so the two cannot drift apart.
public class EtherealWebGpuRendererTests
{
    [Fact]
    public void The_stars_shader_holds_every_star_of_the_scene()
    {
        EtherealWebGpuRenderer.StarsWgsl.Should().Contain($"items: array<vec4<f32>, {EtherealScene.StarCount}>");
        EtherealScene.StarStride.Should().Be(4);
    }

    [Fact]
    public void The_plasma_shader_holds_every_palm_the_field_can_track()
    {
        EtherealWebGpuRenderer.PlasmaWgsl.Should().Contain($"palms: array<vec4<f32>, {PalmAttractorField.MaxAttractors}>");
        //Every palm warps the field, glows and ripples - the first warp starts from p, the rest chain on q
        EtherealWebGpuRenderer.PlasmaWgsl.Should().Contain("warpAt(p, u.palms[0])");
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            if (k > 0) { EtherealWebGpuRenderer.PlasmaWgsl.Should().Contain($"warpAt(q, u.palms[{k}])"); }
            EtherealWebGpuRenderer.PlasmaWgsl.Should().Contain($"pullAt(p, u.palms[{k}])");
            EtherealWebGpuRenderer.PlasmaWgsl.Should().Contain($"rippleAt(p, u.palms[{k}], u.time)");
        }
    }

    [Fact]
    public void Both_shaders_have_the_entry_points_the_pipelines_name()
    {
        foreach (var wgsl in new[] { EtherealWebGpuRenderer.PlasmaWgsl, EtherealWebGpuRenderer.StarsWgsl })
        {
            wgsl.Should().Contain("@vertex fn vs(");
            wgsl.Should().Contain("@fragment fn fs(");
            wgsl.Should().Contain("@group(0) @binding(0) var<uniform>");
        }
    }
}
