namespace PolyHavenBrowser.Rendering;

/// <summary>
/// The WGSL source for the two WebGPU renderers. WebGPU compiles WGSL from source when the shader
/// module is created, so - like the Metal backend and unlike the Vulkan one - there is no
/// pre-compiled shader artifact and building or running the app needs no shader toolchain.
/// </summary>
internal static class WebGpuShaders
{
    /// <summary>
    /// The model shader for <see cref="WebGpuSceneRenderer"/>. The shading mirrors
    /// <see cref="GlModelSceneRenderer"/>, <c>VulkanShaders</c> and <see cref="MetalShaders"/>:
    /// 0.25 ambient + 0.75 diffuse (N·L, double-sided via <c>abs</c> when flagged) modulated by the
    /// material base color and an optional base-color texture. The per-draw values arrive in one
    /// 112-byte uniform block (group 0); the base-color texture and its sampler are group 1. WGSL
    /// reads <c>mat4x4</c> column-major, which applies the same implicit transpose the other
    /// backends rely on when they hand over System.Numerics' row-major matrix unchanged.
    /// </summary>
    internal const string Model = """
        struct Uniforms {
            mvp: mat4x4<f32>,
            baseColorFactor: vec4<f32>,
            lightDirection: vec4<f32>,
            flags: vec4<i32>, // x = hasTexture, y = doubleSided
        };

        @group(0) @binding(0) var<uniform> u: Uniforms;
        @group(1) @binding(0) var baseColorTexture: texture_2d<f32>;
        @group(1) @binding(1) var baseColorSampler: sampler;

        struct VertexIn {
            @location(0) position: vec3<f32>,
            @location(1) normal: vec3<f32>,
            @location(2) texCoord: vec2<f32>,
        };

        struct VertexOut {
            @builtin(position) position: vec4<f32>,
            @location(0) normal: vec3<f32>,
            @location(1) texCoord: vec2<f32>,
        };

        @vertex
        fn vs_main(input: VertexIn) -> VertexOut {
            var out: VertexOut;
            out.position = u.mvp * vec4<f32>(input.position, 1.0);
            out.normal = input.normal;
            out.texCoord = input.texCoord;
            return out;
        }

        @fragment
        fn fs_main(input: VertexOut) -> @location(0) vec4<f32> {
            let normal = normalize(input.normal);
            let nDotL = dot(normal, normalize(u.lightDirection.xyz));
            // Double-sided (abs) lights back faces too, which reads correctly for a headlit
            // model or double-sided foliage; single-sided gives a solid shape real form.
            let diffuse = select(max(nDotL, 0.0), abs(nDotL), u.flags.y == 1);
            let light = 0.25 + 0.75 * diffuse;
            // Sampled unconditionally (WGSL wants textureSample in uniform control flow); an
            // untextured material is bound to a 1x1 white texture and ignores the result.
            let texel = textureSample(baseColorTexture, baseColorSampler, input.texCoord);
            let baseColor = select(u.baseColorFactor, u.baseColorFactor * texel, u.flags.x == 1);
            return vec4<f32>(baseColor.rgb * light, baseColor.a);
        }
        """;

    /// <summary>
    /// The HDRI panorama shader for <see cref="WebGpuPanoramaRenderer"/>: one full-screen triangle
    /// whose fragment shader does on the GPU exactly what <see cref="EquirectPanoramaRenderer"/>
    /// does per pixel on the CPU - cast the camera ray, map it to equirectangular (u, v), sample the
    /// HDR panorama bilinearly (u wraps, v clamps - the sampler's address modes), then exposure →
    /// tone-map operator → sRGB. The view block carries the camera basis pre-scaled by the field of
    /// view and aspect ratio, so a pixel's ray is just <c>forward + right·x + up·y</c>.
    /// </summary>
    internal const string Panorama = """
        struct View {
            forward: vec4<f32>,
            right: vec4<f32>,  // camera right * tan(fov / 2) * aspect
            up: vec4<f32>,     // camera up * tan(fov / 2)
            params: vec4<f32>, // x = exposure, y = operator (0 = ACES filmic, 1 = Reinhard, 2 = clamp)
        };

        @group(0) @binding(0) var<uniform> view: View;
        @group(0) @binding(1) var panorama: texture_2d<f32>;
        @group(0) @binding(2) var panoramaSampler: sampler;

        struct VertexOut {
            @builtin(position) position: vec4<f32>,
            @location(0) ndc: vec2<f32>,
        };

        const PI: f32 = 3.14159265358979;

        @vertex
        fn vs_main(@builtin(vertex_index) index: u32) -> VertexOut {
            var corners = array<vec2<f32>, 3>(vec2<f32>(-1.0, -1.0), vec2<f32>(3.0, -1.0), vec2<f32>(-1.0, 3.0));
            var out: VertexOut;
            out.position = vec4<f32>(corners[index], 0.0, 1.0);
            out.ndc = corners[index]; // y = +1 at the top of the frame, as on the CPU
            return out;
        }

        fn tone_map(value: f32, op: f32) -> f32 {
            if (value <= 0.0) { return 0.0; }
            if (op < 0.5) {
                // Krzysztof Narkowicz's ACES filmic approximation.
                return clamp(value * (2.51 * value + 0.03) / (value * (2.43 * value + 0.59) + 0.14), 0.0, 1.0);
            }
            if (op < 1.5) { return value / (1.0 + value); }
            return min(value, 1.0);
        }

        fn to_srgb(value: f32) -> f32 {
            if (value <= 0.0031308) { return 12.92 * value; }
            return 1.055 * pow(value, 1.0 / 2.4) - 0.055;
        }

        @fragment
        fn fs_main(input: VertexOut) -> @location(0) vec4<f32> {
            let d = normalize(view.forward.xyz + view.right.xyz * input.ndc.x + view.up.xyz * input.ndc.y);
            // Equirect mapping: u from the heading (0.5 at -Z), v from the elevation (0 at top).
            let u = 0.5 + atan2(d.x, -d.z) / (2.0 * PI);
            let v = 0.5 - asin(clamp(d.y, -1.0, 1.0)) / PI;
            let hdr = textureSampleLevel(panorama, panoramaSampler, vec2<f32>(u, v), 0.0).rgb * view.params.x;
            let op = view.params.y;
            return vec4<f32>(
                to_srgb(tone_map(hdr.r, op)),
                to_srgb(tone_map(hdr.g, op)),
                to_srgb(tone_map(hdr.b, op)),
                1.0);
        }
        """;
}
