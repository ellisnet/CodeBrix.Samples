using System;
using CodeBrix.Platform.UI.WebGpu;
using CodeBrix.WebGpu.Native;
using Wgpu = CodeBrix.WebGpu.Native.WebGpu;

namespace PalmVisualizer.Rendering;

/// <summary>
/// Draws the Ethereal visual with WebGPU - the "Ethereal - WebGPU" renderer, given to a
/// <see cref="WebGpuRenderView"/> as its <see cref="IWebGpuRenderer"/>. Each frame is one
/// render pass: a full-screen triangle through a WGSL port of the Ethereal plasma shader,
/// then every star of the <see cref="EtherealScene"/> as an instanced, antialiased quad
/// blended over it. The scene supplies the same palms and stars
/// <see cref="EtherealSkiaRenderer"/> draws, so the two versions are the same visual.
/// </summary>
/// <remarks>
/// The plasma WGSL is a line-for-line port of the SkSL in <see cref="EtherealSkiaRenderer"/>;
/// a change to one belongs in the other. Pipelines are created in
/// <see cref="OnDeviceReady"/> and released in <see cref="OnDeviceLost"/>, as the view's
/// device lifetime requires; frames run on the UI thread (the view's Continuous driver).
/// </remarks>
public sealed class EtherealWebGpuRenderer : IWebGpuRenderer, IDisposable
{
    //Uniforms: time, (padding), resolution, then the four palms as vec4 (x, y, strength, unused)
    internal const string PlasmaWgsl = """
        struct Plasma {
            time: f32,
            pad: f32,
            resolution: vec2<f32>,
            palms: array<vec4<f32>, 4>,
        };
        @group(0) @binding(0) var<uniform> u: Plasma;

        @vertex fn vs(@builtin(vertex_index) index: u32) -> @builtin(position) vec4<f32> {
            // One triangle that covers the whole target
            var corners = array<vec2<f32>, 3>(vec2<f32>(-1.0, -1.0), vec2<f32>(3.0, -1.0), vec2<f32>(-1.0, 3.0));
            return vec4<f32>(corners[index], 0.0, 1.0);
        }

        fn pullAt(p: vec2<f32>, palm: vec4<f32>) -> f32 {
            let d = palm.xy - p;
            return palm.z * exp(-dot(d, d) * 1.4);
        }

        fn warpAt(p: vec2<f32>, palm: vec4<f32>) -> vec2<f32> {
            let d = palm.xy - p;
            let pull = palm.z * exp(-dot(d, d) * 1.4);
            let ang = pull * 2.4;
            let cs = cos(ang);
            let sn = sin(ang);
            let rel = vec2<f32>((-d.x * cs) - (-d.y * sn), (-d.x * sn) + (-d.y * cs));
            return palm.xy + rel + (d * pull * 0.55);
        }

        fn rippleAt(p: vec2<f32>, palm: vec4<f32>, t: f32) -> f32 {
            let dist = length(palm.xy - p);
            let pull = palm.z * exp(-dist * dist * 1.1);
            return pull * sin((dist * 9.0) + (t * 2.4)) * 1.1;
        }

        @fragment fn fs(@builtin(position) fragCoord: vec4<f32>) -> @location(0) vec4<f32> {
            let uv = fragCoord.xy / u.resolution;
            var p = uv * 2.0 - 1.0;
            p.x = p.x * (u.resolution.x / u.resolution.y);

            let t = u.time * 0.6;

            var q = warpAt(p, u.palms[0]);
            q = warpAt(q, u.palms[1]);
            q = warpAt(q, u.palms[2]);
            q = warpAt(q, u.palms[3]);

            let glow = pullAt(p, u.palms[0]) + pullAt(p, u.palms[1]) + pullAt(p, u.palms[2]) + pullAt(p, u.palms[3]);

            var v = sin(q.x * 4.0 + t);
            v = v + sin((q.y + t) * 3.0);
            v = v + sin((q.x + q.y + t) * 5.0);
            let r = length(q + vec2<f32>(sin(t * 0.7), cos(t * 0.9)) * 0.4);
            v = v + sin(r * 8.0 - t * 2.0);

            v = v + rippleAt(p, u.palms[0], u.time);
            v = v + rippleAt(p, u.palms[1], u.time);
            v = v + rippleAt(p, u.palms[2], u.time);
            v = v + rippleAt(p, u.palms[3], u.time);

            v = v * 0.25;

            var col = 0.5 + 0.5 * cos(6.28318 * (v + vec3<f32>(0.0, 0.33, 0.67)) + t * 0.3);

            // smoothstep(1.6, 0.2, r), written out: WGSL leaves smoothstep undefined when low > high
            let s = clamp((r - 1.6) / (0.2 - 1.6), 0.0, 1.0);
            col = col * (0.55 + 0.45 * (s * s * (3.0 - 2.0 * s)));

            col = col + col * glow * 0.9;
            col = min(col, vec3<f32>(1.0));

            return vec4<f32>(col, 1.0);
        }
        """;

    //Uniforms: resolution (xy; zw unused), then each star as vec4 (center x, center y, radius, alpha)
    internal const string StarsWgsl = """
        struct Stars {
            resolution: vec4<f32>,
            items: array<vec4<f32>, 220>,
        };
        @group(0) @binding(0) var<uniform> s: Stars;

        struct StarVertex {
            @builtin(position) position: vec4<f32>,
            @location(0) offset: vec2<f32>,
            @location(1) radius: f32,
            @location(2) alpha: f32,
        };

        @vertex fn vs(@builtin(vertex_index) vertex: u32, @builtin(instance_index) instance: u32) -> StarVertex {
            var corners = array<vec2<f32>, 6>(
                vec2<f32>(-1.0, -1.0), vec2<f32>(1.0, -1.0), vec2<f32>(-1.0, 1.0),
                vec2<f32>(-1.0, 1.0), vec2<f32>(1.0, -1.0), vec2<f32>(1.0, 1.0));
            let star = s.items[instance];

            // A quad one pixel larger than the disc, so its antialiased edge fits
            let offset = corners[vertex] * (star.z + 1.0);
            let pixel = star.xy + offset;

            var result: StarVertex;
            result.position = vec4<f32>(pixel.x / s.resolution.x * 2.0 - 1.0, 1.0 - pixel.y / s.resolution.y * 2.0, 0.0, 1.0);
            result.offset = offset;
            result.radius = star.z;
            result.alpha = star.w;
            return result;
        }

        @fragment fn fs(input: StarVertex) -> @location(0) vec4<f32> {
            // Coverage of a disc with a one-pixel antialiased edge, as premultiplied white
            let coverage = clamp(input.radius + 0.5 - length(input.offset), 0.0, 1.0);
            let a = coverage * input.alpha;
            return vec4<f32>(a, a, a, a);
        }
        """;

    private const int PlasmaFloats = 4 + (PalmAttractorField.MaxAttractors * 4);
    private const int StarsFloats = 4 + (EtherealScene.StarCount * 4);

    private readonly EtherealScene _scene;
    private readonly Func<float> _clock;
    private readonly Func<double> _pixelScale;
    private readonly TextureFormat _format;

    //Reused per-frame uniform blocks - OnRender allocates nothing
    private readonly float[] _plasmaUniforms = new float[PlasmaFloats];
    private readonly float[] _starsUniforms = new float[StarsFloats];

    private WgslPass _plasma;
    private WgslPass _stars;

    /// <summary>
    /// Initializes a new instance of the <see cref="EtherealWebGpuRenderer"/> class.
    /// </summary>
    /// <param name="scene">The scene to draw; advanced by this renderer once per frame.</param>
    /// <param name="clock">The visual's clock, in seconds (it stops while the visual is paused).</param>
    /// <param name="pixelScale">The view's pixels per layout unit (its <see cref="WebGpuRenderView.PixelScale"/>).</param>
    /// <param name="format">The view's colour format (its <see cref="WebGpuRenderView.PreferredFormat"/>).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="scene"/>,
    /// <paramref name="clock"/> or <paramref name="pixelScale"/> is null.</exception>
    public EtherealWebGpuRenderer(EtherealScene scene, Func<float> clock, Func<double> pixelScale, TextureFormat format)
    {
        _scene = scene ?? throw new ArgumentNullException(nameof(scene));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _pixelScale = pixelScale ?? throw new ArgumentNullException(nameof(pixelScale));
        _format = format;
    }

    /// <inheritdoc />
    public void OnDeviceReady(IWebGpuDeviceSource device)
    {
        ReleasePasses();
        _plasma = new WgslPass(device.Device, device.Queue, PlasmaWgsl, _format, PlasmaFloats * sizeof(float), premultipliedBlend: false);
        _stars = new WgslPass(device.Device, device.Queue, StarsWgsl, _format, StarsFloats * sizeof(float), premultipliedBlend: true);
    }

    /// <inheritdoc />
    public unsafe void OnRender(WebGpuFrame frame)
    {
        if (_plasma == null || _stars == null) { return; }

        float width = frame.Width;
        float height = frame.Height;
        _scene.Advance(_clock(), width, height, (float)_pixelScale());

        // The plasma's block: time, padding, resolution, then each palm as (x, y, strength, 0)
        _plasmaUniforms[0] = _scene.Time;
        _plasmaUniforms[1] = 0f;
        _plasmaUniforms[2] = width;
        _plasmaUniforms[3] = height;
        ReadOnlySpan<float> palms = _scene.Palms;
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            int to = 4 + (k * 4);
            palms.Slice(k * EtherealScene.PalmStride, EtherealScene.PalmStride).CopyTo(_plasmaUniforms.AsSpan(to));
            _plasmaUniforms[to + 3] = 0f;
        }

        // The stars' block: resolution, then the stars exactly as the scene lays them out
        _starsUniforms[0] = width;
        _starsUniforms[1] = height;
        _scene.Stars.CopyTo(_starsUniforms.AsSpan(4));

        _plasma.WriteUniforms(_plasmaUniforms);
        _stars.WriteUniforms(_starsUniforms);

        // One pass: the plasma covers every pixel, then the stars blend over it
        var attachment = new RenderPassColorAttachment
        {
            View = frame.TextureView,
            DepthSlice = Constants.DepthSliceUndefined,
            LoadOp = LoadOp.Load,
            StoreOp = StoreOp.Store,
        };
        var descriptor = new RenderPassDescriptor { ColorAttachmentCount = 1, ColorAttachments = &attachment };
        RenderPassEncoder pass = Wgpu.CommandEncoderBeginRenderPass(frame.Encoder, &descriptor);
        _plasma.Record(pass, 3, 1);
        _stars.Record(pass, 6, EtherealScene.StarCount);
        Wgpu.RenderPassEncoderEnd(pass);
        Wgpu.RenderPassEncoderRelease(pass);
    }

    /// <inheritdoc />
    public void OnDeviceLost(string reason) => ReleasePasses();

    /// <summary>Releases the pipelines (the view releases its device itself).</summary>
    public void Dispose() => ReleasePasses();

    private void ReleasePasses()
    {
        _plasma?.Dispose();
        _plasma = null;
        _stars?.Dispose();
        _stars = null;
    }
}
