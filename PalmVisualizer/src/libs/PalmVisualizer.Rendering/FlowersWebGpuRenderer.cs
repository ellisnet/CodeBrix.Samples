using System;
using CodeBrix.Platform.UI.WebGpu;
using CodeBrix.WebGpu.Native;
using GpuBuffer = CodeBrix.WebGpu.Native.Buffer;
using Wgpu = CodeBrix.WebGpu.Native.WebGpu;

namespace PalmVisualizer.Rendering;

/// <summary>
/// Draws "Flowers - WebGPU": vivid eight-petalled flowers growing out of dark pool-table green
/// felt that glows bright green under the palms. One render pass: a full-screen triangle for
/// the felt - lit by the same palm-light formula <see cref="FlowerMeadow.LightAt"/> uses - then
/// every flower of the <see cref="FlowerMeadow"/> as an instanced quad, blended over it. The
/// light's reach (<see cref="FlowerMeadow.LightReach"/>) travels in the uniform block. Each
/// flower's fragment shader draws a tapered stem that grows up first, with two leaves; then a
/// green bud that swells and unfolds into exactly eight identical petals, 45 degrees apart,
/// alternately in front and behind so the four-way symmetry holds even where they overlap -
/// shaded as cupped 3D petals with a vein, a base-to-tip gradient and a soft shadow on the felt
/// - round a domed centre dotted with stamens. Light makes the colours more vivid; withering
/// browns, shrinks and curls the flower until it fades away.
/// </summary>
public sealed class FlowersWebGpuRenderer : IWebGpuRenderer, IDisposable
{
    //Both shaders read the same uniform block: view size, time, palm activity; flower count,
    //  the palm light's reach across and down, padding; the palms (x, y, strength, speed); then
    //  the flowers, six vec4s each
    private const string MeadowStruct = """
        struct Meadow {
            resolution: vec2<f32>,
            time: f32,
            activity: f32,
            count: f32,
            reachAcross: f32,
            reachDown: f32,
            pad0: f32,
            palms: array<vec4<f32>, 4>,
            flowers: array<vec4<f32>, 480>,
        };
        @group(0) @binding(0) var<uniform> m: Meadow;

        const PI = 3.14159265;
        """;

    internal const string FeltWgsl = MeadowStruct + """

        @vertex fn vs(@builtin(vertex_index) index: u32) -> @builtin(position) vec4<f32> {
            // One triangle that covers the whole target
            var corners = array<vec2<f32>, 3>(vec2<f32>(-1.0, -1.0), vec2<f32>(3.0, -1.0), vec2<f32>(-1.0, 3.0));
            return vec4<f32>(corners[index], 0.0, 1.0);
        }

        fn grain(p: vec2<f32>) -> f32 {
            return fract(sin(dot(floor(p), vec2<f32>(127.1, 311.7))) * 43758.5453);
        }

        @fragment fn fs(@builtin(position) fragCoord: vec4<f32>) -> @location(0) vec4<f32> {
            let uv = fragCoord.xy / m.resolution;
            let aspect = m.resolution.x / m.resolution.y;

            // The palm light: a soft oval round each open palm, reaching as far as FlowerMeadow.LightReach says
            var light = 0.0;
            for (var k = 0; k < 4; k = k + 1) {
                let palm = m.palms[k];
                if (palm.z > 0.001) {
                    let d = vec2<f32>((uv.x - palm.x) * aspect / m.reachAcross, (uv.y - palm.y) / m.reachDown);
                    light = light + palm.z * exp(-dot(d, d));
                }
            }
            light = min(light, 1.0);

            // Dark pool-table green felt, darkest at the edges, with a fine grain - glowing a
            // vibrant green where the palms light it
            let edge = length((uv - 0.5) * vec2<f32>(aspect, 1.0)) / (0.6 * aspect);
            var felt = mix(vec3<f32>(0.012, 0.075, 0.035), vec3<f32>(0.005, 0.035, 0.018), smoothstep(0.2, 1.0, edge));
            felt = mix(felt, vec3<f32>(0.10, 0.60, 0.22) * (1.0 - 0.25 * smoothstep(0.4, 1.0, edge)), light);
            felt = felt * (0.94 + 0.12 * grain(fragCoord.xy * 0.7));
            return vec4<f32>(felt, 1.0);
        }
        """;

    internal const string FlowerWgsl = MeadowStruct + """

        // Every flower, whatever its size and shape, has exactly this many even petals
        const PETALS = 8.0;

        struct FlowerVertex {
            @builtin(position) position: vec4<f32>,
            @location(0) local: vec2<f32>,
            @location(1) @interpolate(flat) index: u32,
        };

        @vertex fn vs(@builtin(vertex_index) vertex: u32, @builtin(instance_index) instance: u32) -> FlowerVertex {
            let a = m.flowers[instance * 6u];
            let b = m.flowers[instance * 6u + 1u];
            var corners = array<vec2<f32>, 6>(
                vec2<f32>(-1.0, 0.0), vec2<f32>(1.0, 0.0), vec2<f32>(-1.0, 1.0),
                vec2<f32>(-1.0, 1.0), vec2<f32>(1.0, 0.0), vec2<f32>(1.0, 1.0));
            let c = corners[vertex];

            // The flower's own frame, in pixels: the head's centre at the origin (y down), the
            // stem below it reaching the ground at y = stem length
            let reach = a.z * 1.25;
            let below = max(reach, b.w + a.z * 0.6);
            let local = vec2<f32>(c.x * max(reach, a.z * 1.0 + b.w * 0.3), mix(-reach, below, c.y));

            var result: FlowerVertex;
            let pixel = a.xy + local;
            result.position = vec4<f32>(pixel.x / m.resolution.x * 2.0 - 1.0, 1.0 - pixel.y / m.resolution.y * 2.0, 0.0, 1.0);
            result.local = local;
            result.index = instance;
            return result;
        }

        fn hash(p: vec2<f32>) -> f32 {
            return fract(sin(dot(p, vec2<f32>(127.1, 311.7))) * 43758.5453);
        }

        // One petal along angle `axis`: (distance to its edge in pixels, how far along it 0..1,
        // how far across it -1..1). Its outline runs from a rounded to a pointed tip with `tip`,
        // and is widest near its base (`widest` 0, a lance) or near its tip (`widest` 1, a spoon)
        fn petal(p: vec2<f32>, axis: f32, len: f32, width: f32, tip: f32, widest: f32) -> vec3<f32> {
            let dir = vec2<f32>(cos(axis), sin(axis));
            let along = dot(p, dir);
            let across = dot(p, vec2<f32>(-dir.y, dir.x));
            let u = along / max(len, 0.001);
            if (u <= 0.0 || u >= 1.0) {
                return vec3<f32>(1000.0, u, 0.0);
            }
            let w = pow(u, mix(0.55, 1.8, widest));
            let rounded = sqrt(clamp(4.0 * w * (1.0 - w), 0.0, 1.0)) * smoothstep(0.0, 0.25, u);
            let pointed = sin(PI * w);
            let half = len * width * mix(rounded, pointed, tip);
            return vec3<f32>(abs(across) - half, u, across / max(half, 0.001));
        }

        @fragment fn fs(input: FlowerVertex) -> @location(0) vec4<f32> {
            let f0 = m.flowers[input.index * 6u];
            let f1 = m.flowers[input.index * 6u + 1u];
            let f2 = m.flowers[input.index * 6u + 2u];
            let f3 = m.flowers[input.index * 6u + 3u];
            let f4 = m.flowers[input.index * 6u + 4u];
            let f5 = m.flowers[input.index * 6u + 5u];
            let radius = f0.z;
            let stemLen = f1.w;
            let bloom = f2.x;
            let stemGrowth = f2.y;
            let wither = f2.z;
            let light = f4.z;
            let p = input.local;
            let toLight = normalize(vec3<f32>(-0.4, -0.5, 0.75));

            // Colours: more vivid in the palms' light, browning as the flower withers
            let brown = vec3<f32>(0.34, 0.21, 0.09);
            let grey = vec3<f32>(dot(f1.rgb, vec3<f32>(0.3, 0.5, 0.2)));
            var petalColor = mix(grey, f1.rgb, clamp(0.82 + 0.3 * light, 0.0, 1.15)) * (0.85 + 0.3 * light);
            petalColor = mix(petalColor, brown, smoothstep(0.0, 0.8, wither));
            var leafColor = mix(vec3<f32>(0.08, 0.30, 0.10), vec3<f32>(0.18, 0.78, 0.24), light);
            leafColor = mix(leafColor, brown * 0.85, smoothstep(0.15, 0.9, wither));

            var col = vec3<f32>(0.0);
            var cover = 0.0;

            // 1) The stem, growing up from the ground toward the head, bending a little, with two leaves
            let top = stemLen * (1.0 - stemGrowth);
            let s = clamp(p.y / max(stemLen, 1.0), 0.0, 1.0);
            let stemX = f4.y * radius * 0.6 * s * s;
            let stemHalf = radius * (0.045 + 0.035 * s);
            let across = (p.x - stemX) / stemHalf;
            let stem = (1.0 - smoothstep(-0.7, 0.7, abs(p.x - stemX) - stemHalf)) * smoothstep(top - 1.0, top + 1.0, p.y) * step(p.y, stemLen);
            col = leafColor * (0.6 + 0.45 * sqrt(max(1.0 - across * across, 0.0)));
            cover = stem;

            for (var k = 0; k < 2; k = k + 1) {
                let at = stemLen * (0.55 + 0.2 * f32(k));
                let grown = clamp((at - top) / (radius * 0.8), 0.0, 1.0);
                if (grown > 0.0) {
                    let side = select(-1.0, 1.0, k == 0) * select(1.0, -1.0, f4.w > 0.5);
                    let dir = normalize(vec2<f32>(side * 0.8, -0.6));
                    let q = p - vec2<f32>(f4.y * radius * 0.6 * (at / stemLen) * (at / stemLen), at);
                    let len = radius * 0.85 * grown * (1.0 - 0.3 * wither);
                    let along = dot(q, dir) / max(len, 0.001);
                    let off = dot(q, vec2<f32>(-dir.y, dir.x));
                    let half = len * 0.24 * sin(PI * clamp(along, 0.0, 1.0));
                    let leaf = (1.0 - smoothstep(-0.7, 0.7, abs(off) - half)) * step(0.0, along) * step(along, 1.0);
                    let vein = 1.0 - 0.25 * exp(-off * off / max(half * half * 0.02, 0.01));
                    col = mix(col, leafColor * (0.75 + 0.3 * along) * vein, leaf);
                    cover = max(cover, leaf);
                }
            }

            // 2) The head, once the stem has reached it: a soft shadow on the felt, then the petals
            var shadow = 0.0;
            if (stemGrowth >= 1.0) {
                let open = smoothstep(0.0, 1.0, bloom);
                let len = radius * f5.y * mix(0.30, 1.0, open) * (1.0 - 0.35 * wither);
                let width = f3.x * mix(0.55, 1.0, open) * (1.0 - 0.45 * wither);
                let budTint = 1.0 - smoothstep(0.15, 0.55, bloom);

                shadow = 0.32 * (1.0 - smoothstep(len * 0.65, len * 1.05, length(p - vec2<f32>(radius * 0.06, radius * 0.10))));

                // Exactly PETALS even petals: the nearest one and its two neighbours, the
                // even-numbered four in front of the odd four
                let sector = 2.0 * PI / PETALS;
                let angle = atan2(p.y, p.x) - f0.w;
                let nearest = floor(angle / sector + 0.5);
                var best = vec3<f32>(1000.0, 0.0, 0.0);
                var bestAxis = 0.0;
                var bestFront = false;
                for (var n = -1; n <= 1; n = n + 1) {
                    let index = nearest + f32(n);
                    let axis = index * sector + f0.w;
                    let pet = petal(p, axis, len, width, f3.y, f5.x);
                    let front = (i32(index) % 2 + 2) % 2 == 0;
                    let covers = pet.x < 0.7;
                    let takes = covers && (best.x >= 0.7 || (front && !bestFront) || (front == bestFront && pet.x < best.x));
                    if (takes) {
                        best = pet;
                        bestAxis = axis;
                        bestFront = front;
                    }
                }

                if (best.x < 0.7) {
                    let u = best.y;
                    let v = best.z;

                    // A cupped petal: its surface curls up toward the edges and the base
                    let cup = f3.z + 0.3 * wither;
                    let slopeAcross = v * (0.5 + 0.9 * cup);
                    let slopeAlong = (0.55 - u) * (0.4 + 0.8 * cup);
                    let dir = vec2<f32>(cos(bestAxis), sin(bestAxis));
                    let side = vec2<f32>(-dir.y, dir.x);
                    let n = normalize(vec3<f32>(-(dir * slopeAlong + side * slopeAcross), 1.0));
                    let shade = 0.55 + 0.6 * max(dot(n, toLight), 0.0);

                    var c = mix(petalColor * 0.62, mix(petalColor, vec3<f32>(1.0), 0.22), smoothstep(0.0, 1.0, u));
                    c = c * (1.0 - 0.2 * exp(-v * v * 40.0));
                    c = c + petalColor * 0.18 * smoothstep(0.75, 1.0, abs(v));
                    c = mix(c, leafColor * 1.1, budTint);
                    if (!bestFront) {
                        c = c * 0.86;
                    }
                    let petalCover = 1.0 - smoothstep(-0.7, 0.7, best.x);
                    col = mix(col, c * shade, petalCover);
                    cover = max(cover, petalCover);
                }

                // The domed centre, dotted with stamens - hidden in the bud
                let r = length(p);
                let centre = radius * f3.w * mix(0.5, 1.0, open) * (1.0 - 0.3 * wither);
                if (bloom > 0.2 && r < centre + 1.0) {
                    var cc = vec3<f32>(1.0, 0.78, 0.15);
                    if (f4.x > 1.5) {
                        cc = vec3<f32>(1.0, 0.94, 0.68);
                    } else if (f4.x > 0.5) {
                        cc = vec3<f32>(0.28, 0.13, 0.05);
                    }
                    cc = mix(cc, brown * 0.7, smoothstep(0.0, 0.8, wither));
                    let dome = sqrt(max(1.0 - (r * r) / (centre * centre), 0.0));
                    // Stamens: a round dot in about half the cells of a fine grid
                    let grid = p / max(centre * 0.24, 1.5);
                    let cell = floor(grid);
                    let spot = length(fract(grid) - 0.5);
                    let dots = step(0.5, hash(cell + f4.w * 17.0)) * (1.0 - smoothstep(0.22, 0.34, spot)) * 0.3;
                    let centreCover = (1.0 - smoothstep(centre - 0.7, centre + 0.7, r)) * smoothstep(0.2, 0.4, bloom);
                    col = mix(col, cc * (0.55 + 0.5 * dome) * (1.0 - dots) + vec3<f32>(0.15) * pow(dome, 8.0), centreCover);
                    cover = max(cover, centreCover);
                }
            }

            // Premultiplied: the flower over its own shadow, all fading out as it withers away
            let fade = f2.w;
            let alpha = (cover + shadow * (1.0 - cover)) * fade;
            return vec4<f32>(col * cover * fade, alpha);
        }
        """;

    private const int HeaderFloats = 8 + (PalmAttractorField.MaxAttractors * 4);
    private const int MeadowFloats = HeaderFloats + (FlowerMeadow.MaxFlowers * FlowerMeadow.FloatsPerFlower);

    private readonly PalmMotion _palms;
    private readonly FlowerMeadow _meadow = new();
    private readonly Func<float> _clock;
    private readonly TextureFormat _format;

    //The reused per-frame uniform block - OnRender allocates nothing
    private readonly float[] _meadowUniforms = new float[MeadowFloats];

    private Queue _queue;
    private ShaderModule _feltModule;
    private ShaderModule _flowerModule;
    private RenderPipeline _felt;
    private RenderPipeline _flowers;
    private GpuBuffer _meadowBuffer;
    private BindGroup _feltGroup;
    private BindGroup _flowerGroup;

    /// <summary>Initializes a new instance of the <see cref="FlowersWebGpuRenderer"/> class.</summary>
    /// <param name="palms">The palms that light the meadow; advanced by this renderer once per frame.</param>
    /// <param name="clock">The visual's clock, in seconds (it stops while the visual is paused).</param>
    /// <param name="format">The view's colour format (its <see cref="WebGpuRenderView.PreferredFormat"/>).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="palms"/> or
    /// <paramref name="clock"/> is null.</exception>
    public FlowersWebGpuRenderer(PalmMotion palms, Func<float> clock, TextureFormat format)
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

        _feltModule = WgslGpu.CreateShaderModule(gpu, FeltWgsl);
        _flowerModule = WgslGpu.CreateShaderModule(gpu, FlowerWgsl);
        _felt = WgslGpu.CreateRenderPipeline(gpu, _feltModule, _format);
        _flowers = WgslGpu.CreateRenderPipeline(gpu, _flowerModule, _format, premultipliedBlend: true);
        _meadowBuffer = WgslGpu.CreateBuffer(gpu, BufferUsage.Uniform | BufferUsage.CopyDst, MeadowFloats * sizeof(float));

        // Each pipeline's layout is derived from its own shader, so each gets its own bind group -
        //  both over the one uniform buffer
        BindGroupLayout feltLayout = Wgpu.RenderPipelineGetBindGroupLayout(_felt, 0);
        _feltGroup = WgslGpu.CreateBindGroup(gpu, feltLayout, new[] { (_meadowBuffer, MeadowFloats * sizeof(float)) });
        Wgpu.BindGroupLayoutRelease(feltLayout);

        BindGroupLayout flowerLayout = Wgpu.RenderPipelineGetBindGroupLayout(_flowers, 0);
        _flowerGroup = WgslGpu.CreateBindGroup(gpu, flowerLayout, new[] { (_meadowBuffer, MeadowFloats * sizeof(float)) });
        Wgpu.BindGroupLayoutRelease(flowerLayout);
    }

    /// <inheritdoc />
    public unsafe void OnRender(WebGpuFrame frame)
    {
        if (_flowerGroup.IsNull) { return; }

        float width = frame.Width;
        float height = frame.Height;
        _palms.Advance(_clock());
        _meadow.Advance(_palms, height > 0f ? width / height : 1f);

        int count = _meadow.WriteInstances(_meadowUniforms.AsSpan(HeaderFloats), width, height);
        _meadowUniforms[0] = width;
        _meadowUniforms[1] = height;
        _meadowUniforms[2] = _palms.Time;
        _meadowUniforms[3] = _palms.Activity;
        var (reachAcross, reachDown) = FlowerMeadow.LightReach(_palms.Palms);
        _meadowUniforms[4] = count;
        _meadowUniforms[5] = reachAcross;
        _meadowUniforms[6] = reachDown;
        _palms.Palms.CopyTo(_meadowUniforms.AsSpan(8));
        WgslGpu.WriteBuffer(_queue, _meadowBuffer, _meadowUniforms);

        var attachment = new RenderPassColorAttachment
        {
            View = frame.TextureView,
            DepthSlice = Constants.DepthSliceUndefined,
            LoadOp = LoadOp.Load,
            StoreOp = StoreOp.Store,
        };
        var descriptor = new RenderPassDescriptor { ColorAttachmentCount = 1, ColorAttachments = &attachment };
        RenderPassEncoder pass = Wgpu.CommandEncoderBeginRenderPass(frame.Encoder, &descriptor);
        Wgpu.RenderPassEncoderSetPipeline(pass, _felt);
        Wgpu.RenderPassEncoderSetBindGroup(pass, 0, _feltGroup, 0, null);
        Wgpu.RenderPassEncoderDraw(pass, 3, 1, 0, 0);
        if (count > 0)
        {
            Wgpu.RenderPassEncoderSetPipeline(pass, _flowers);
            Wgpu.RenderPassEncoderSetBindGroup(pass, 0, _flowerGroup, 0, null);
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
        if (!_flowerGroup.IsNull) { Wgpu.BindGroupRelease(_flowerGroup); _flowerGroup = default; }
        if (!_feltGroup.IsNull) { Wgpu.BindGroupRelease(_feltGroup); _feltGroup = default; }
        if (!_meadowBuffer.IsNull) { Wgpu.BufferRelease(_meadowBuffer); _meadowBuffer = default; }
        if (!_flowers.IsNull) { Wgpu.RenderPipelineRelease(_flowers); _flowers = default; }
        if (!_felt.IsNull) { Wgpu.RenderPipelineRelease(_felt); _felt = default; }
        if (!_flowerModule.IsNull) { Wgpu.ShaderModuleRelease(_flowerModule); _flowerModule = default; }
        if (!_feltModule.IsNull) { Wgpu.ShaderModuleRelease(_feltModule); _feltModule = default; }
        _queue = default;
    }
}
