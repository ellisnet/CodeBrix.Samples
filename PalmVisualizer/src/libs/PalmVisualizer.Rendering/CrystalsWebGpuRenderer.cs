using System;
using CodeBrix.Platform.UI.WebGpu;
using CodeBrix.WebGpu.Native;
using GpuBuffer = CodeBrix.WebGpu.Native.Buffer;
using Wgpu = CodeBrix.WebGpu.Native.WebGpu;

namespace PalmVisualizer.Rendering;

/// <summary>
/// Draws "Crystals - WebGPU": glinting, translucent quartz crystals growing over a dark indigo
/// backdrop, in vibrant indigo, purple, magenta, mauve and orange, every one gently pulsing.
/// One render pass: a full-screen triangle for the backdrop (with a soft light under each
/// palm), then every crystal of the <see cref="CrystalGarden"/> as an instanced quad, blended
/// over it. Each crystal's fragment shader draws a quartz point - a hexagonal prism with a
/// pointed termination: its three visible prism faces and three tip faces shaded apart, facet
/// edges catching the light, glass that is brighter where it is thin, faint phantom outlines
/// inside, a white glint sweeping up it, a sparkle at its tip now and then, and a soft glow
/// round it. Excited crystals - those near an open palm - are drawn more vibrant, pulse harder
/// and faster, and glint more often.
/// </summary>
public sealed class CrystalsWebGpuRenderer : IWebGpuRenderer, IDisposable
{
    //Both shaders read the same uniform block: view size, time, palm activity; crystal count
    //  and padding; the palms (x, y, strength, speed); then the crystals, three vec4s each
    private const string GardenStruct = """
        struct Garden {
            resolution: vec2<f32>,
            time: f32,
            activity: f32,
            count: f32,
            pad0: f32,
            pad1: f32,
            pad2: f32,
            palms: array<vec4<f32>, 4>,
            crystals: array<vec4<f32>, 288>,
        };
        @group(0) @binding(0) var<uniform> g: Garden;
        """;

    internal const string BackdropWgsl = GardenStruct + """

        @vertex fn vs(@builtin(vertex_index) index: u32) -> @builtin(position) vec4<f32> {
            // One triangle that covers the whole target
            var corners = array<vec2<f32>, 3>(vec2<f32>(-1.0, -1.0), vec2<f32>(3.0, -1.0), vec2<f32>(-1.0, 3.0));
            return vec4<f32>(corners[index], 0.0, 1.0);
        }

        @fragment fn fs(@builtin(position) fragCoord: vec4<f32>) -> @location(0) vec4<f32> {
            let uv = fragCoord.xy / g.resolution;
            let aspect = g.resolution.x / g.resolution.y;
            let q = vec2<f32>(uv.x * aspect, uv.y);
            let t = g.time;

            // Dark indigo, deepest at the edges, with a faint, slowly shifting haze
            let r = length(q - vec2<f32>(aspect * 0.5, 0.5)) / 0.9;
            var col = mix(vec3<f32>(0.07, 0.03, 0.16), vec3<f32>(0.008, 0.004, 0.025), smoothstep(0.0, 1.0, r));
            let haze = 0.5 + 0.5 * sin(q.x * 3.1 + t * 0.07) * sin(q.y * 2.3 - t * 0.05);
            col = col + vec3<f32>(0.07, 0.02, 0.10) * haze;

            // A soft magenta-and-orange light under each open palm
            for (var k = 0; k < 4; k = k + 1) {
                let palm = g.palms[k];
                if (palm.z > 0.001) {
                    let d = q - vec2<f32>(palm.x * aspect, palm.y);
                    let light = palm.z * exp(-dot(d, d) * 10.0);
                    col = col + vec3<f32>(0.35, 0.10, 0.32) * light * 0.6 + vec3<f32>(0.30, 0.12, 0.02) * light * light * 0.5;
                }
            }
            return vec4<f32>(col, 1.0);
        }
        """;

    internal const string CrystalWgsl = GardenStruct + """

        struct CrystalVertex {
            @builtin(position) position: vec4<f32>,
            @location(0) local: vec2<f32>,
            @location(1) @interpolate(flat) index: u32,
        };

        // How far past its sides, in half-widths, a crystal's quad reaches - room for its glow
        const MARGIN = 2.2;

        @vertex fn vs(@builtin(vertex_index) vertex: u32, @builtin(instance_index) instance: u32) -> CrystalVertex {
            let a = g.crystals[instance * 3u];
            let b = g.crystals[instance * 3u + 1u];
            var corners = array<vec2<f32>, 6>(
                vec2<f32>(-1.0, 0.0), vec2<f32>(1.0, 0.0), vec2<f32>(-1.0, 1.0),
                vec2<f32>(-1.0, 1.0), vec2<f32>(1.0, 0.0), vec2<f32>(1.0, 1.0));
            let c = corners[vertex];
            let pad = b.x * MARGIN;

            // The crystal's own frame, in pixels: x across its axis, y along it from the base
            let local = vec2<f32>(c.x * (b.x + pad), mix(-pad, a.w + pad, c.y));
            let along = vec2<f32>(cos(a.z), sin(a.z));
            let across = vec2<f32>(-along.y, along.x);
            let pixel = a.xy + along * local.y + across * local.x;

            var result: CrystalVertex;
            result.position = vec4<f32>(pixel.x / g.resolution.x * 2.0 - 1.0, 1.0 - pixel.y / g.resolution.y * 2.0, 0.0, 1.0);
            result.local = local;
            result.index = instance;
            return result;
        }

        // The vibrant palette, around the wheel: indigo, purple, magenta, mauve, orange
        fn palette(h: f32) -> vec3<f32> {
            var colors = array<vec3<f32>, 5>(
                vec3<f32>(0.30, 0.22, 1.00), vec3<f32>(0.62, 0.22, 1.00), vec3<f32>(1.00, 0.22, 0.72),
                vec3<f32>(0.88, 0.56, 0.98), vec3<f32>(1.00, 0.50, 0.06));
            let x = fract(h) * 5.0;
            let i = u32(floor(x)) % 5u;
            return mix(colors[i], colors[(i + 1u) % 5u], smoothstep(0.0, 1.0, fract(x)));
        }

        // Distance (pixels) from a point to the crystal's outline - a prism of half-width `half`
        // ending in a point at `len` - negative inside
        fn outline(p: vec2<f32>, half: f32, len: f32) -> f32 {
            let tipLen = min(half * 1.7, len * 0.5);
            let q = vec2<f32>(abs(p.x), p.y);
            let slope = normalize(vec2<f32>(tipLen, half));
            return max(max(q.x - half, -q.y), dot(q - vec2<f32>(half, len - tipLen), slope));
        }

        fn segment(p: vec2<f32>, a: vec2<f32>, b: vec2<f32>) -> f32 {
            let pa = p - a;
            let ba = b - a;
            return length(pa - ba * clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0));
        }

        @fragment fn fs(input: CrystalVertex) -> @location(0) vec4<f32> {
            let a = g.crystals[input.index * 3u];
            let b = g.crystals[input.index * 3u + 1u];
            let c = g.crystals[input.index * 3u + 2u];
            let len = a.w;
            let half = b.x;
            let p = input.local;
            let t = g.time;
            let excite = c.w;

            let tipLen = min(half * 1.7, len * 0.5);
            let shoulder = len - tipLen;
            let d = outline(p, half, len);

            // Colour: pulsing in brightness and hue - harder, faster and more vibrant when excited
            let pulse = sin(t * (1.1 + 1.8 * excite) + c.x * 6.2832);
            var tint = palette(b.y + 0.025 * pulse * (0.3 + excite));
            tint = mix(vec3<f32>(dot(tint, vec3<f32>(0.3, 0.5, 0.2))), tint, b.z);
            tint = tint * (1.0 + (0.12 + 0.30 * excite) * pulse);

            // Facets: the prism's three visible faces, then the three faces of the point
            let x = p.x / half;
            var shade = 1.0;
            var edges = 0.0;
            if (p.y < shoulder) {
                shade = select(select(1.0, 0.80, x > 0.33), 0.62, x < -0.33);
                edges = 1.0 - smoothstep(0.0, 1.4, abs(abs(p.x) - half * 0.33));
            } else {
                let k = x / max(1.0 - (p.y - shoulder) / tipLen, 0.001);
                shade = select(select(1.12, 0.90, k > 0.33), 0.70, k < -0.33);
                let tip = vec2<f32>(0.0, len);
                edges = max(1.0 - smoothstep(0.0, 1.4, segment(p, tip, vec2<f32>(half * 0.33, shoulder))),
                            1.0 - smoothstep(0.0, 1.4, segment(p, tip, vec2<f32>(-half * 0.33, shoulder))));
            }
            edges = max(edges, (1.0 - smoothstep(0.0, 1.2, abs(p.y - shoulder))) * step(abs(p.x), half) * 0.6);

            // Translucent glass: bright where it is thin - at the edges and toward the point
            let rim = smoothstep(-half * 0.6, 0.0, d);
            let toward = clamp(p.y / len, 0.0, 1.0);
            var col = tint * shade * (0.55 + 0.35 * toward + 0.40 * rim);

            // Phantoms: faint ghosts of the crystal's outline inside it
            let ghosts = (1.0 - smoothstep(0.0, 1.3, abs(d + len * 0.16))) + (1.0 - smoothstep(0.0, 1.3, abs(d + len * 0.30)));
            col = col + tint * 0.30 * ghosts;

            // Glints: a white streak sweeping up the crystal, and its facet edges catching light
            let sweep = fract(t * (0.10 + 0.40 * excite) + c.y) * len * 1.8 - len * 0.4;
            let streak = exp(-pow((p.y - sweep + p.x * 0.9) / (half * 0.45), 2.0));
            col = col + vec3<f32>(1.0, 0.96, 1.0) * (streak * 0.85 * shade + edges * (0.30 + 0.45 * excite));

            // The glass's coverage: an antialiased outline, melting into its cluster at the base
            let inside = (1.0 - smoothstep(-0.7, 0.7, d)) * smoothstep(0.0, half * 0.9, p.y);
            let alpha = inside * (0.68 + 0.25 * rim) * b.w;

            // Light added round it: a soft glow, and now and then a four-rayed sparkle at its point
            let outside = max(d, 0.0);
            let glow = exp(-outside * outside / (half * half * 1.4)) * (0.16 + 0.10 * pulse + 0.45 * excite) * (1.0 - inside);
            let twinkle = pow(max(sin(t * (0.5 + 1.2 * excite) + c.z * 40.0), 0.0), 24.0);
            let toTip = p - vec2<f32>(0.0, len);
            let rays = exp(-abs(toTip.x) / 1.3) * exp(-abs(toTip.y) / (half * 1.6)) + exp(-abs(toTip.y) / 1.3) * exp(-abs(toTip.x) / (half * 1.6));
            let sparkle = twinkle * (rays + exp(-length(toTip) / (half * 0.25)));
            let light = (tint * glow * 0.6 + vec3<f32>(1.0, 0.95, 1.0) * sparkle) * b.w;

            // Premultiplied: the glass covers what is behind it; the light only adds
            return vec4<f32>(col * alpha + light, alpha);
        }
        """;

    private const int HeaderFloats = 8 + (PalmAttractorField.MaxAttractors * 4);
    private const int GardenFloats = HeaderFloats + (CrystalGarden.MaxCrystals * CrystalGarden.FloatsPerCrystal);

    private readonly PalmMotion _palms;
    private readonly CrystalGarden _garden = new();
    private readonly Func<float> _clock;
    private readonly TextureFormat _format;

    //The reused per-frame uniform block - OnRender allocates nothing
    private readonly float[] _gardenUniforms = new float[GardenFloats];

    private Queue _queue;
    private ShaderModule _backdropModule;
    private ShaderModule _crystalModule;
    private RenderPipeline _backdrop;
    private RenderPipeline _crystals;
    private GpuBuffer _gardenBuffer;
    private BindGroup _backdropGroup;
    private BindGroup _crystalGroup;

    /// <summary>Initializes a new instance of the <see cref="CrystalsWebGpuRenderer"/> class.</summary>
    /// <param name="palms">The palms that excite the crystals; advanced by this renderer once per frame.</param>
    /// <param name="clock">The visual's clock, in seconds (it stops while the visual is paused).</param>
    /// <param name="format">The view's colour format (its <see cref="WebGpuRenderView.PreferredFormat"/>).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="palms"/> or
    /// <paramref name="clock"/> is null.</exception>
    public CrystalsWebGpuRenderer(PalmMotion palms, Func<float> clock, TextureFormat format)
    {
        _palms = palms ?? throw new ArgumentNullException(nameof(palms));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _format = format;
    }

    /// <inheritdoc />
    public void OnDeviceReady(IWebGpuDeviceSource device)
    {
        Release();
        Device gpu = device.Device;
        _queue = device.Queue;

        _backdropModule = WgslGpu.CreateShaderModule(gpu, BackdropWgsl);
        _crystalModule = WgslGpu.CreateShaderModule(gpu, CrystalWgsl);
        _backdrop = WgslGpu.CreateRenderPipeline(gpu, _backdropModule, _format);
        _crystals = WgslGpu.CreateRenderPipeline(gpu, _crystalModule, _format, premultipliedBlend: true);
        _gardenBuffer = WgslGpu.CreateBuffer(gpu, BufferUsage.Uniform | BufferUsage.CopyDst, GardenFloats * sizeof(float));

        // The two pipelines' layouts are each derived from their own shader, so each gets its
        //  own bind group - both over the one uniform buffer
        BindGroupLayout backdropLayout = Wgpu.RenderPipelineGetBindGroupLayout(_backdrop, 0);
        _backdropGroup = WgslGpu.CreateBindGroup(gpu, backdropLayout, new[] { (_gardenBuffer, GardenFloats * sizeof(float)) });
        Wgpu.BindGroupLayoutRelease(backdropLayout);

        BindGroupLayout crystalLayout = Wgpu.RenderPipelineGetBindGroupLayout(_crystals, 0);
        _crystalGroup = WgslGpu.CreateBindGroup(gpu, crystalLayout, new[] { (_gardenBuffer, GardenFloats * sizeof(float)) });
        Wgpu.BindGroupLayoutRelease(crystalLayout);
    }

    /// <inheritdoc />
    public unsafe void OnRender(WebGpuFrame frame)
    {
        if (_crystalGroup.IsNull) { return; }

        float width = frame.Width;
        float height = frame.Height;
        _palms.Advance(_clock());
        _garden.Advance(_palms, height > 0f ? width / height : 1f);

        int count = _garden.WriteInstances(_gardenUniforms.AsSpan(HeaderFloats), width, height);
        _gardenUniforms[0] = width;
        _gardenUniforms[1] = height;
        _gardenUniforms[2] = _palms.Time;
        _gardenUniforms[3] = _palms.Activity;
        _gardenUniforms[4] = count;
        _palms.Palms.CopyTo(_gardenUniforms.AsSpan(8));
        WgslGpu.WriteBuffer(_queue, _gardenBuffer, _gardenUniforms);

        var attachment = new RenderPassColorAttachment
        {
            View = frame.TextureView,
            DepthSlice = Constants.DepthSliceUndefined,
            LoadOp = LoadOp.Load,
            StoreOp = StoreOp.Store,
        };
        var descriptor = new RenderPassDescriptor { ColorAttachmentCount = 1, ColorAttachments = &attachment };
        RenderPassEncoder pass = Wgpu.CommandEncoderBeginRenderPass(frame.Encoder, &descriptor);
        Wgpu.RenderPassEncoderSetPipeline(pass, _backdrop);
        Wgpu.RenderPassEncoderSetBindGroup(pass, 0, _backdropGroup, 0, null);
        Wgpu.RenderPassEncoderDraw(pass, 3, 1, 0, 0);
        if (count > 0)
        {
            Wgpu.RenderPassEncoderSetPipeline(pass, _crystals);
            Wgpu.RenderPassEncoderSetBindGroup(pass, 0, _crystalGroup, 0, null);
            Wgpu.RenderPassEncoderDraw(pass, 6, (uint)count, 0, 0);
        }
        Wgpu.RenderPassEncoderEnd(pass);
        Wgpu.RenderPassEncoderRelease(pass);
    }

    /// <inheritdoc />
    public void OnDeviceLost(string reason) => Release();

    /// <summary>Releases the GPU objects (the view releases its device itself).</summary>
    public void Dispose() => Release();

    private void Release()
    {
        if (!_crystalGroup.IsNull) { Wgpu.BindGroupRelease(_crystalGroup); _crystalGroup = default; }
        if (!_backdropGroup.IsNull) { Wgpu.BindGroupRelease(_backdropGroup); _backdropGroup = default; }
        if (!_gardenBuffer.IsNull) { Wgpu.BufferRelease(_gardenBuffer); _gardenBuffer = default; }
        if (!_crystals.IsNull) { Wgpu.RenderPipelineRelease(_crystals); _crystals = default; }
        if (!_backdrop.IsNull) { Wgpu.RenderPipelineRelease(_backdrop); _backdrop = default; }
        if (!_crystalModule.IsNull) { Wgpu.ShaderModuleRelease(_crystalModule); _crystalModule = default; }
        if (!_backdropModule.IsNull) { Wgpu.ShaderModuleRelease(_backdropModule); _backdropModule = default; }
        _queue = default;
    }
}
