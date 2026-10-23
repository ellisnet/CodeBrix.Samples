using System;
using CodeBrix.Platform.UI.WebGpu;
using CodeBrix.WebGpu.Native;
using GpuBuffer = CodeBrix.WebGpu.Native.Buffer;
using Wgpu = CodeBrix.WebGpu.Native.WebGpu;

namespace PalmVisualizer.Rendering;

/// <summary>
/// Draws "Ripple - WebGPU": a shimmering, lightly iridescent pool of blue water. Each frame
/// is two passes recorded into the view's frame: a compute pass that advances a 2D wave
/// equation over a height field held in two storage buffers (ping-ponged, two steps a frame),
/// with the palms stirring it and <see cref="RippleRain"/> dropping on it; then a render pass
/// that shades the height field as water - slope normals; a pastel glass mosaic of an
/// eight-petalled rosette on the pool floor, refracted by the waves; caustics where the waves
/// focus the light, a Fresnel sky reflection, sun glints, and a thin-film rainbow sheen on the
/// slopes. Calm water gets only the light rain and a faint swell; open palms
/// rock the water and drag wakes behind them as they move, so the surface follows the hands.
/// </summary>
public sealed class RippleWebGpuRenderer : IWebGpuRenderer, IDisposable
{
    /// <summary>The wave simulation's grid, in cells across and down (the cells stretch to the view).</summary>
    public const int GridWidth = 448;

    /// <summary>The wave simulation's grid, in cells down.</summary>
    public const int GridHeight = 256;

    //Two simulation steps a frame - an even number, so the newest heights always end up back
    //  in the first buffer, which is the one the render pass reads
    private const int StepsPerFrame = 2;
    private const int WorkgroupSize = 16;
    private const float Damping = 0.9965f;

    //Uniforms: grid size, wave weights (x, y), damping, time, view aspect, drop count, then
    //  the palms (x, y, strength, speed) and the frame's drops (x, y, depth, radius)
    internal const string SimulationWgsl = """
        struct Simulation {
            size: vec2<f32>,
            weights: vec2<f32>,
            damping: f32,
            time: f32,
            aspect: f32,
            dropCount: f32,
            palms: array<vec4<f32>, 4>,
            drops: array<vec4<f32>, 4>,
        };
        @group(0) @binding(0) var<uniform> sim: Simulation;
        @group(0) @binding(1) var<storage, read> current: array<f32>;
        @group(0) @binding(2) var<storage, read_write> previous: array<f32>;

        // A round bump (in view space, whatever the view's shape) of radius r centred on c
        fn bump(uv: vec2<f32>, c: vec2<f32>, r: f32) -> f32 {
            let d = (uv - c) * vec2<f32>(sim.aspect, 1.0);
            return exp(-dot(d, d) / (r * r));
        }

        @compute @workgroup_size(16, 16)
        fn step(@builtin(global_invocation_id) id: vec3<u32>) {
            let w = u32(sim.size.x);
            let h = u32(sim.size.y);
            if (id.x >= w || id.y >= h) { return; }
            let i = id.y * w + id.x;

            // The pool's walls reflect: an edge cell stands in for its missing neighbour
            let c = current[i];
            let l = current[select(i - 1u, i, id.x == 0u)];
            let r = current[select(i + 1u, i, id.x == w - 1u)];
            let u = current[select(i - w, i, id.y == 0u)];
            let d = current[select(i + w, i, id.y == h - 1u)];

            // The wave equation, one leapfrog step: the previous heights become the next ones
            var next = 2.0 * c - previous[i] + sim.weights.x * (l + r - 2.0 * c) + sim.weights.y * (u + d - 2.0 * c);
            next = next * sim.damping;

            let uv = (vec2<f32>(id.xy) + 0.5) / sim.size;

            // An open palm rocks the water under it, and drags a trough behind it as it moves
            for (var k = 0u; k < 4u; k = k + 1u) {
                let palm = sim.palms[k];
                if (palm.z > 0.001) {
                    let rock = sin(sim.time * 6.5 + f32(k) * 1.7);
                    next = next + bump(uv, palm.xy, 0.045) * palm.z * (0.045 * rock - 0.07 * min(palm.w, 2.0));
                }
            }

            // Rain: each drop presses a small dimple into the surface
            for (var k = 0u; k < u32(sim.dropCount); k = k + 1u) {
                let drop = sim.drops[k];
                next = next - drop.z * bump(uv, drop.xy, drop.w);
            }

            previous[i] = next;
        }
        """;

    //Uniforms: view size in pixels, grid size, time, palm activity, then the palms
    internal const string SurfaceWgsl = """
        struct Surface {
            resolution: vec2<f32>,
            grid: vec2<f32>,
            time: f32,
            activity: f32,
            pad0: f32,
            pad1: f32,
            palms: array<vec4<f32>, 4>,
        };
        @group(0) @binding(0) var<uniform> s: Surface;
        @group(0) @binding(1) var<storage, read> heights: array<f32>;

        @vertex fn vs(@builtin(vertex_index) index: u32) -> @builtin(position) vec4<f32> {
            // One triangle that covers the whole target
            var corners = array<vec2<f32>, 3>(vec2<f32>(-1.0, -1.0), vec2<f32>(3.0, -1.0), vec2<f32>(-1.0, 3.0));
            return vec4<f32>(corners[index], 0.0, 1.0);
        }

        fn cell(x: i32, y: i32) -> f32 {
            let w = i32(s.grid.x);
            let h = i32(s.grid.y);
            return heights[u32(clamp(y, 0, h - 1) * w + clamp(x, 0, w - 1))];
        }

        // The simulated surface height at a point of the view, bilinearly filtered
        fn heightAt(uv: vec2<f32>) -> f32 {
            let g = uv * s.grid - 0.5;
            let base = floor(g);
            let f = g - base;
            let x = i32(base.x);
            let y = i32(base.y);
            let top = mix(cell(x, y), cell(x + 1, y), f.x);
            let bottom = mix(cell(x, y + 1), cell(x + 1, y + 1), f.x);
            return mix(top, bottom, f.y);
        }

        // The slope of a faint swell of small travelling waves on top of the simulation
        fn swell(p: vec2<f32>, t: f32) -> vec2<f32> {
            var dirs = array<vec3<f32>, 4>(
                vec3<f32>(0.80, 0.60, 23.0), vec3<f32>(-0.53, 0.85, 31.0),
                vec3<f32>(0.95, -0.31, 41.0), vec3<f32>(-0.20, -0.98, 57.0));
            var slope = vec2<f32>(0.0);
            for (var k = 0; k < 4; k = k + 1) {
                let d = dirs[k];
                slope = slope + d.xy * cos(dot(p, d.xy) * d.z + t * (1.3 + 0.35 * f32(k)));
            }
            return slope * 0.25;
        }

        // A soft net of light, as the sun makes on a pool floor
        fn caustics(p: vec2<f32>, t: f32) -> f32 {
            var q = p * 6.0;
            for (var k = 0; k < 3; k = k + 1) {
                q = q + vec2<f32>(sin(q.y * 1.3 + t * 0.55), cos(q.x * 1.1 - t * 0.45)) * 0.6;
            }
            let net = abs(sin(q.x) * sin(q.y));
            return pow(1.0 - net, 7.0);
        }

        const PI = 3.14159265;

        fn hash2(p: vec2<f32>) -> vec2<f32> {
            let q = vec2<f32>(dot(p, vec2<f32>(127.1, 311.7)), dot(p, vec2<f32>(269.5, 183.3)));
            return fract(sin(q) * 43758.5453);
        }

        // Signed distance to one petal of the rosette: a pointed lens (two circular arcs) running
        // from near the heart, at 0.06, out to the rim, at 0.44, along angle a
        fn petal(q: vec2<f32>, a: f32) -> f32 {
            let along = vec2<f32>(cos(a), sin(a));
            let p = abs(vec2<f32>(dot(q, vec2<f32>(-along.y, along.x)), dot(q, along) - 0.25));
            // Half-length 0.19, half-width 0.10: arcs of radius 0.2305 centred 0.1305 to either side
            let r = 0.2305;
            let d = 0.1305;
            let b = 0.19;
            if ((p.y - b) * d > p.x * b) {
                return length(p - vec2<f32>(0.0, b));
            }
            return length(p - vec2<f32>(-d, 0.0)) - r;
        }

        // The rosette's design at a point (relative to its centre): eight pastel glass petals with
        // pale gold outlines, a cream heart, a gold ring holding it all, sea glass around it
        fn rosette(q: vec2<f32>) -> vec3<f32> {
            var palette = array<vec3<f32>, 4>(
                vec3<f32>(0.92, 0.50, 0.64), vec3<f32>(0.98, 0.64, 0.36),
                vec3<f32>(0.60, 0.48, 0.90), vec3<f32>(0.40, 0.80, 0.66));
            let gold = vec3<f32>(0.80, 0.58, 0.18);
            let r = length(q);

            var col = mix(vec3<f32>(0.45, 0.74, 0.84), vec3<f32>(0.62, 0.58, 0.88), 0.5 + 0.5 * sin(q.x * 4.0 + q.y * 3.0));
            if (r < 0.47) {
                col = mix(col, vec3<f32>(0.84, 0.88, 0.95), 0.15);
            }

            var fill = vec3<f32>(0.0);
            var count = 0.0;
            var edge = 1000.0;
            for (var k = 0; k < 8; k = k + 1) {
                let a = (f32(k) + 0.5) * PI / 4.0;
                let dk = petal(q, a);
                edge = min(edge, abs(dk));
                if (dk < 0.0) {
                    // Lighter toward the petal's tip
                    let tip = clamp((dot(q, vec2<f32>(cos(a), sin(a))) - 0.06) / 0.38, 0.0, 1.0);
                    let base = palette[k % 4];
                    fill = fill + mix(base * 0.92, mix(base, vec3<f32>(1.0), 0.15), tip);
                    count = count + 1.0;
                }
            }
            if (count > 0.0) {
                // Where two petals overlap, the glass layers into a lighter blend of both
                col = mix(fill / count, vec3<f32>(1.0), 0.06 * (count - 1.0));
            }
            if (edge < 0.0075 || abs(r - 0.47) < 0.00975) {
                col = gold;
            }

            // The heart: a solid cream highlight whose rim softens into the petals
            col = mix(col, vec3<f32>(0.98, 0.88, 0.66), 1.0 - smoothstep(0.045, 0.072, r));
            return col;
        }

        // The glass tesserae: (distance to the piece's edge, two random values for the piece)
        fn tesserae(p: vec2<f32>) -> vec3<f32> {
            let cells = p * 34.0;
            let i = floor(cells);
            let f = fract(cells);
            var nearest = 8.0;
            var second = 8.0;
            var id = vec2<f32>(0.0);
            for (var y = -1; y <= 1; y = y + 1) {
                for (var x = -1; x <= 1; x = x + 1) {
                    let g = vec2<f32>(f32(x), f32(y));
                    let offset = g + hash2(i + g) * 0.8 + 0.1 - f;
                    let dist = dot(offset, offset);
                    if (dist < nearest) {
                        second = nearest;
                        nearest = dist;
                        id = i + g;
                    } else if (dist < second) {
                        second = dist;
                    }
                }
            }
            return vec3<f32>((sqrt(second) - sqrt(nearest)) * 0.5 / 34.0, hash2(id + 7.3));
        }

        @fragment fn fs(@builtin(position) fragCoord: vec4<f32>) -> @location(0) vec4<f32> {
            let uv = fragCoord.xy / s.resolution;
            let aspect = s.resolution.x / s.resolution.y;
            let p = vec2<f32>(uv.x * aspect, uv.y);
            let t = s.time;

            // The surface: slopes of the simulated heights (in view heights), plus the swell
            let ex = 1.0 / s.grid.x;
            let ey = 1.0 / s.grid.y;
            let hC = heightAt(uv);
            let hL = heightAt(uv - vec2<f32>(ex, 0.0));
            let hR = heightAt(uv + vec2<f32>(ex, 0.0));
            let hU = heightAt(uv - vec2<f32>(0.0, ey));
            let hD = heightAt(uv + vec2<f32>(0.0, ey));
            var slope = vec2<f32>((hR - hL) / (2.0 * ex * aspect), (hD - hU) / (2.0 * ey)) * 0.010;
            slope = slope + swell(p, t) * (0.045 + 0.10 * s.activity);
            let n = normalize(vec3<f32>(-slope.x, -slope.y, 1.0));
            let steep = min(length(slope), 0.6);

            // The pool floor seen through the water: a pastel glass mosaic of an eight-petalled
            // rosette, refracted by the waves and lit by caustics
            let floorP = p + n.xy * 0.07;
            let pieces = tesserae(floorP);
            var glass = rosette(floorP - vec2<f32>(aspect * 0.5, 0.5));
            glass = glass * (0.94 + 0.10 * pieces.y) + (pieces.z - 0.5) * 0.03;
            let groutWidth = 0.0022;
            let grout = 1.0 - smoothstep(groutWidth * 0.5, groutWidth * 1.5, pieces.x);
            var floorColor = mix(glass, vec3<f32>(0.82, 0.82, 0.80), grout * 0.55);
            let focus = clamp(-(hL + hR + hU + hD - 4.0 * hC) * 1.5, -0.5, 1.0);
            let light = caustics(floorP, t) * (0.55 + 0.45 * focus) + max(focus, 0.0) * 0.35;
            floorColor = floorColor + vec3<f32>(1.0, 0.97, 0.90) * light * 0.18;

            // Through the water: it absorbs light - red and green a little more than blue - so the
            // pool reads deep and blue-ish while the mosaic keeps its own colours; a touch deeper
            // toward the edges
            let rim = smoothstep(0.0, 0.5, min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y)));
            let absorbed = floorColor * vec3<f32>(0.74, 0.81, 0.90) * (0.86 + 0.14 * rim);
            var col = mix(vec3<f32>(0.08, 0.26, 0.46), absorbed, 0.86 + 0.06 * rim);

            // The surface itself: a Fresnel reflection of a pale sky...
            let fresnel = clamp(0.05 + 1.6 * steep * steep, 0.0, 0.45);
            let sky = mix(vec3<f32>(0.82, 0.92, 1.0), vec3<f32>(0.45, 0.68, 0.95), uv.y);
            col = mix(col, sky, fresnel);

            // ...a thin-film rainbow sheen that drifts over the water and shimmers where it slopes...
            let film = 0.3 + hC * 0.6 + steep * 4.0 + t * 0.05 + 0.25 * sin(p.x * 2.2 + p.y * 1.7 + t * 0.3);
            let sheen = 0.5 + 0.5 * cos(6.28318 * (film + vec3<f32>(0.0, 0.33, 0.67)));
            col = col + (sheen - 0.45) * (0.10 + 0.9 * steep) * 0.5;

            // ...and glints of sun
            let toSun = normalize(vec3<f32>(-0.35, -0.55, 0.76));
            let glint = pow(max(dot(reflect(-toSun, n), vec3<f32>(0.0, 0.0, 1.0)), 0.0), 140.0);
            col = col + vec3<f32>(1.0, 0.98, 0.92) * glint * 1.4;

            return vec4<f32>(clamp(col, vec3<f32>(0.0), vec3<f32>(1.0)), 1.0);
        }
        """;

    private const int SimulationFloats = 8 + (PalmAttractorField.MaxAttractors * 4) + (RippleRain.MaxDropsPerFrame * 4);
    private const int SurfaceFloats = 8 + (PalmAttractorField.MaxAttractors * 4);
    private const int HeightBytes = GridWidth * GridHeight * sizeof(float);

    private readonly PalmMotion _palms;
    private readonly RippleRain _rain = new();
    private readonly Func<float> _clock;
    private readonly TextureFormat _format;

    //Reused per-frame uniform blocks - OnRender allocates nothing
    private readonly float[] _simulationUniforms = new float[SimulationFloats];
    private readonly float[] _surfaceUniforms = new float[SurfaceFloats];

    private Queue _queue;
    private ShaderModule _simulationModule;
    private ShaderModule _surfaceModule;
    private ComputePipeline _simulation;
    private RenderPipeline _surface;
    private GpuBuffer _simulationBuffer;
    private GpuBuffer _surfaceBuffer;
    private GpuBuffer _heightsA;
    private GpuBuffer _heightsB;
    private BindGroup _stepFromA;
    private BindGroup _stepFromB;
    private BindGroup _surfaceGroup;

    /// <summary>Initializes a new instance of the <see cref="RippleWebGpuRenderer"/> class.</summary>
    /// <param name="palms">The palms that stir the water; advanced by this renderer once per frame.</param>
    /// <param name="clock">The visual's clock, in seconds (it stops while the visual is paused).</param>
    /// <param name="format">The view's colour format (its <see cref="WebGpuRenderView.PreferredFormat"/>).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="palms"/> or
    /// <paramref name="clock"/> is null.</exception>
    public RippleWebGpuRenderer(PalmMotion palms, Func<float> clock, TextureFormat format)
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

        _simulationModule = WgslGpu.CreateShaderModule(gpu, SimulationWgsl);
        _surfaceModule = WgslGpu.CreateShaderModule(gpu, SurfaceWgsl);
        _simulation = WgslGpu.CreateComputePipeline(gpu, _simulationModule);
        _surface = WgslGpu.CreateRenderPipeline(gpu, _surfaceModule, _format);

        _simulationBuffer = WgslGpu.CreateBuffer(gpu, BufferUsage.Uniform | BufferUsage.CopyDst, SimulationFloats * sizeof(float));
        _surfaceBuffer = WgslGpu.CreateBuffer(gpu, BufferUsage.Uniform | BufferUsage.CopyDst, SurfaceFloats * sizeof(float));
        _heightsA = WgslGpu.CreateBuffer(gpu, BufferUsage.Storage, HeightBytes);
        _heightsB = WgslGpu.CreateBuffer(gpu, BufferUsage.Storage, HeightBytes);

        //A step reads one buffer as the current heights and overwrites the other (the previous
        //  heights) with the next ones; the two groups alternate the roles
        BindGroupLayout stepLayout = Wgpu.ComputePipelineGetBindGroupLayout(_simulation, 0);
        int simulationBytes = SimulationFloats * sizeof(float);
        _stepFromA = WgslGpu.CreateBindGroup(gpu, stepLayout, new[] { (_simulationBuffer, simulationBytes), (_heightsA, HeightBytes), (_heightsB, HeightBytes) });
        _stepFromB = WgslGpu.CreateBindGroup(gpu, stepLayout, new[] { (_simulationBuffer, simulationBytes), (_heightsB, HeightBytes), (_heightsA, HeightBytes) });
        Wgpu.BindGroupLayoutRelease(stepLayout);

        BindGroupLayout surfaceLayout = Wgpu.RenderPipelineGetBindGroupLayout(_surface, 0);
        _surfaceGroup = WgslGpu.CreateBindGroup(gpu, surfaceLayout, new[] { (_surfaceBuffer, SurfaceFloats * sizeof(float)), (_heightsA, HeightBytes) });
        Wgpu.BindGroupLayoutRelease(surfaceLayout);
    }

    /// <inheritdoc />
    public unsafe void OnRender(WebGpuFrame frame)
    {
        if (_surfaceGroup.IsNull) { return; }

        float width = frame.Width;
        float height = frame.Height;
        float aspect = height > 0f ? width / height : 1f;
        _palms.Advance(_clock());
        _rain.Advance(_palms.DeltaTime, _palms.Activity);

        // The wave weights: half the stability limit, split by the cells' shape in the view so
        //  the waves travel equally fast across and down
        float a = (GridWidth * GridWidth) / (aspect * aspect);
        float b = GridHeight * GridHeight;

        _simulationUniforms[0] = GridWidth;
        _simulationUniforms[1] = GridHeight;
        _simulationUniforms[2] = 0.5f * a / (a + b);
        _simulationUniforms[3] = 0.5f * b / (a + b);
        _simulationUniforms[4] = Damping;
        _simulationUniforms[5] = _palms.Time;
        _simulationUniforms[6] = aspect;
        _simulationUniforms[7] = _rain.Count;
        _palms.Palms.CopyTo(_simulationUniforms.AsSpan(8));
        int dropsAt = 8 + (PalmAttractorField.MaxAttractors * 4);
        _simulationUniforms.AsSpan(dropsAt).Clear();
        _rain.Drops.Slice(0, _rain.Count * RippleRain.Stride).CopyTo(_simulationUniforms.AsSpan(dropsAt));

        _surfaceUniforms[0] = width;
        _surfaceUniforms[1] = height;
        _surfaceUniforms[2] = GridWidth;
        _surfaceUniforms[3] = GridHeight;
        _surfaceUniforms[4] = _palms.Time;
        _surfaceUniforms[5] = _palms.Activity;
        _palms.Palms.CopyTo(_surfaceUniforms.AsSpan(8));

        WgslGpu.WriteBuffer(_queue, _simulationBuffer, _simulationUniforms);
        WgslGpu.WriteBuffer(_queue, _surfaceBuffer, _surfaceUniforms);

        // 1) Advance the waves
        var computeDescriptor = new ComputePassDescriptor();
        ComputePassEncoder compute = Wgpu.CommandEncoderBeginComputePass(frame.Encoder, &computeDescriptor);
        Wgpu.ComputePassEncoderSetPipeline(compute, _simulation);
        for (int step = 0; step < StepsPerFrame; step++)
        {
            Wgpu.ComputePassEncoderSetBindGroup(compute, 0, step % 2 == 0 ? _stepFromA : _stepFromB, 0, null);
            Wgpu.ComputePassEncoderDispatchWorkgroups(compute,
                (uint)((GridWidth + WorkgroupSize - 1) / WorkgroupSize), (uint)((GridHeight + WorkgroupSize - 1) / WorkgroupSize), 1);
        }
        Wgpu.ComputePassEncoderEnd(compute);
        Wgpu.ComputePassEncoderRelease(compute);

        // 2) Shade the water
        var attachment = new RenderPassColorAttachment
        {
            View = frame.TextureView,
            DepthSlice = Constants.DepthSliceUndefined,
            LoadOp = LoadOp.Load,
            StoreOp = StoreOp.Store,
        };
        var renderDescriptor = new RenderPassDescriptor { ColorAttachmentCount = 1, ColorAttachments = &attachment };
        RenderPassEncoder pass = Wgpu.CommandEncoderBeginRenderPass(frame.Encoder, &renderDescriptor);
        Wgpu.RenderPassEncoderSetPipeline(pass, _surface);
        Wgpu.RenderPassEncoderSetBindGroup(pass, 0, _surfaceGroup, 0, null);
        Wgpu.RenderPassEncoderDraw(pass, 3, 1, 0, 0);
        Wgpu.RenderPassEncoderEnd(pass);
        Wgpu.RenderPassEncoderRelease(pass);
    }

    /// <inheritdoc />
    public void OnDeviceLost(string reason) => Release();

    /// <summary>Releases the GPU objects (the view releases its device itself).</summary>
    public void Dispose() => Release();

    private void Release()
    {
        if (!_surfaceGroup.IsNull) { Wgpu.BindGroupRelease(_surfaceGroup); _surfaceGroup = default; }
        if (!_stepFromB.IsNull) { Wgpu.BindGroupRelease(_stepFromB); _stepFromB = default; }
        if (!_stepFromA.IsNull) { Wgpu.BindGroupRelease(_stepFromA); _stepFromA = default; }
        if (!_heightsB.IsNull) { Wgpu.BufferRelease(_heightsB); _heightsB = default; }
        if (!_heightsA.IsNull) { Wgpu.BufferRelease(_heightsA); _heightsA = default; }
        if (!_surfaceBuffer.IsNull) { Wgpu.BufferRelease(_surfaceBuffer); _surfaceBuffer = default; }
        if (!_simulationBuffer.IsNull) { Wgpu.BufferRelease(_simulationBuffer); _simulationBuffer = default; }
        if (!_surface.IsNull) { Wgpu.RenderPipelineRelease(_surface); _surface = default; }
        if (!_simulation.IsNull) { Wgpu.ComputePipelineRelease(_simulation); _simulation = default; }
        if (!_surfaceModule.IsNull) { Wgpu.ShaderModuleRelease(_surfaceModule); _surfaceModule = default; }
        if (!_simulationModule.IsNull) { Wgpu.ShaderModuleRelease(_simulationModule); _simulationModule = default; }
        _queue = default;
    }
}
