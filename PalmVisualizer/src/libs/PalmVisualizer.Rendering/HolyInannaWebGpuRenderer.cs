using System;
using CodeBrix.Platform.UI.WebGpu;
using CodeBrix.WebGpu.Native;
using GpuBuffer = CodeBrix.WebGpu.Native.Buffer;
using Wgpu = CodeBrix.WebGpu.Native.WebGpu;

namespace PalmVisualizer.Rendering;

/// <summary>
/// Draws "Holy Inanna - WebGPU": the eight-pointed Star of Inanna (Ishtar) as a golden 3D
/// object, slowly spinning and gently tilting in a soft halo, in a space nebula of indigo,
/// purple, magenta and mauve with glowing orange cores. One fragment shader does it all: the
/// nebula is layers of domain-warped noise with dark dust lanes and a twinkling star field,
/// drifting slowly and swirled and brightened around each open palm; the star is ray-marched
/// as a solid - the symbol's star shape extruded with faceted, ridged rays and a raised centre -
/// and shaded as polished gold carrying the symbol's indigo line work (its outline, two nested
/// star outlines, the double ring and the centre dot). <see cref="HolyInannaMotion"/> decides
/// where the star is, how large and how turned.
/// </summary>
public sealed class HolyInannaWebGpuRenderer : IWebGpuRenderer, IDisposable
{
    //Uniforms: view size, time, palm activity; the star's centre and radius in pixels and its
    //  glow; its spin; the rotation from view space into the star's own space (three rows);
    //  then the palms (x, y, strength, speed)
    internal const string SceneWgsl = """
        struct Scene {
            resolution: vec2<f32>,
            time: f32,
            activity: f32,
            star: vec4<f32>,
            extra: vec4<f32>,
            m0: vec4<f32>,
            m1: vec4<f32>,
            m2: vec4<f32>,
            palms: array<vec4<f32>, 4>,
        };
        @group(0) @binding(0) var<uniform> u: Scene;

        const PI = 3.14159265;

        // The star's inner corners, as a fraction of its points - measured from the symbol
        const INNER = 0.426;

        @vertex fn vs(@builtin(vertex_index) index: u32) -> @builtin(position) vec4<f32> {
            // One triangle that covers the whole target
            var corners = array<vec2<f32>, 3>(vec2<f32>(-1.0, -1.0), vec2<f32>(3.0, -1.0), vec2<f32>(-1.0, 3.0));
            return vec4<f32>(corners[index], 0.0, 1.0);
        }

        fn hash(p: vec2<f32>) -> f32 {
            return fract(sin(dot(p, vec2<f32>(127.1, 311.7))) * 43758.5453);
        }

        fn noise(p: vec2<f32>) -> f32 {
            let i = floor(p);
            let f = fract(p);
            let s = f * f * (3.0 - 2.0 * f);
            let a = hash(i);
            let b = hash(i + vec2<f32>(1.0, 0.0));
            let c = hash(i + vec2<f32>(0.0, 1.0));
            let d = hash(i + vec2<f32>(1.0, 1.0));
            return mix(mix(a, b, s.x), mix(c, d, s.x), s.y);
        }

        fn fbm(start: vec2<f32>) -> f32 {
            var p = start;
            var v = 0.0;
            var a = 0.5;
            let turn = mat2x2<f32>(0.8, 0.6, -0.6, 0.8);
            for (var i = 0; i < 5; i = i + 1) {
                v = v + a * noise(p);
                p = turn * p * 2.02 + vec2<f32>(1.7, 9.2);
                a = a * 0.5;
            }
            return v;
        }

        // A field of fine stars: in a few cells of a grid, a pin-point of light at a random place
        // that twinkles gently (scale = grid cells per view height)
        fn starField(q: vec2<f32>, scale: f32, t: f32) -> f32 {
            let g = q * scale;
            let cell = floor(g);
            let h = hash(cell);
            if (h < 0.965) { return 0.0; }
            let at = vec2<f32>(hash(cell + 17.1), hash(cell + 31.7)) * 0.8 + 0.1;
            let pixels = length(g - cell - at) * u.resolution.y / scale;
            let twinkle = 0.75 + 0.25 * sin(t * (1.5 + 3.0 * h) + h * 100.0);
            return exp(-pixels * pixels * 0.9) * (h - 0.965) / 0.035 * twinkle;
        }

        // The nebula's palette, for what the gold star reflects: deep indigo, through purple,
        // to orange and a golden heart
        fn nebulaTone(heat: f32) -> vec3<f32> {
            let x = clamp(heat, 0.0, 1.0);
            var c = mix(vec3<f32>(0.02, 0.01, 0.08), vec3<f32>(0.16, 0.05, 0.42), smoothstep(0.0, 0.3, x));
            c = mix(c, vec3<f32>(0.50, 0.10, 0.58), smoothstep(0.3, 0.55, x));
            c = mix(c, vec3<f32>(1.00, 0.45, 0.08), smoothstep(0.55, 0.8, x));
            c = mix(c, vec3<f32>(1.00, 0.80, 0.45), smoothstep(0.85, 1.0, x));
            return c;
        }

        // Signed distance to the eight-pointed star whose points reach 1 (negative inside)
        fn starDistance(p: vec2<f32>) -> f32 {
            let sector = PI / 4.0;
            let a = atan2(p.x, p.y);
            let b = a - sector * floor((a + sector * 0.5) / sector);
            let q = length(p) * vec2<f32>(abs(sin(b)), cos(b));
            let tip = vec2<f32>(0.0, 1.0);
            let e = INNER * vec2<f32>(sin(PI / 8.0), cos(PI / 8.0)) - tip;
            let w = q - tip;
            let along = clamp(dot(w, e) / dot(e, e), 0.0, 1.0);
            let dist = length(w - e * along);
            return select(dist, -dist, e.x * w.y - e.y * w.x < 0.0);
        }

        // The star as a solid: its shape extruded, each ray ridged along its middle, the centre raised
        fn solid(p: vec3<f32>) -> f32 {
            let d2 = starDistance(p.xy);
            let thickness = 0.035 + 0.55 * min(max(-d2, 0.0), 0.22) + 0.10 * (1.0 - smoothstep(0.0, 0.26, length(p.xy)));
            let dz = abs(p.z) - thickness;
            return (min(max(d2, dz), 0.0) + length(max(vec2<f32>(d2, dz), vec2<f32>(0.0)))) * 0.8;
        }

        fn toStar(v: vec3<f32>) -> vec3<f32> {
            return vec3<f32>(dot(u.m0.xyz, v), dot(u.m1.xyz, v), dot(u.m2.xyz, v));
        }

        fn toView(v: vec3<f32>) -> vec3<f32> {
            return v.x * u.m0.xyz + v.y * u.m1.xyz + v.z * u.m2.xyz;
        }

        // The symbol's indigo line work: a fine line just inside the edge, two nested star
        // outlines, the double ring and the centre dot (the side walls stay gold)
        fn engraving(p: vec2<f32>) -> f32 {
            let r = length(p);
            let w = 0.011;
            var m = 1.0 - smoothstep(w, w * 1.8, abs(starDistance(p) + 0.035));
            m = max(m, 1.0 - smoothstep(w, w * 1.8, abs(starDistance(p / 0.66) * 0.66)));
            m = max(m, 1.0 - smoothstep(w, w * 1.8, abs(starDistance(p / 0.40) * 0.40)));
            m = max(m, 1.0 - smoothstep(w, w * 1.8, abs(r - 0.235)));
            m = max(m, 1.0 - smoothstep(w, w * 1.8, abs(r - 0.147)));
            m = max(m, 1.0 - smoothstep(0.040, 0.050, r));
            return m;
        }

        fn shadeStar(p: vec3<f32>) -> vec3<f32> {
            let k = vec2<f32>(1.0, -1.0) * 0.0015;
            let local = normalize(k.xyy * solid(p + k.xyy) + k.yyx * solid(p + k.yyx) + k.yxy * solid(p + k.yxy) + k.xxx * solid(p + k.xxx));
            let n = normalize(toView(local));
            let toLight = normalize(vec3<f32>(-0.45, 0.55, 0.70));
            let diffuse = max(dot(n, toLight), 0.0);
            let shine = pow(max(dot(reflect(-toLight, n), vec3<f32>(0.0, 0.0, 1.0)), 0.0), 36.0);
            let mirror = reflect(vec3<f32>(0.0, 0.0, -1.0), n);
            let surroundings = nebulaTone(0.45 + 0.45 * mirror.y + 0.15 * u.activity);
            let gold = vec3<f32>(1.0, 0.80, 0.34) * (0.38 + 0.75 * diffuse) * (0.75 + 0.45 * surroundings)
                + vec3<f32>(1.0, 0.94, 0.75) * shine * 1.2;
            let indigo = vec3<f32>(0.07, 0.04, 0.56) * (0.35 + 0.75 * diffuse) + vec3<f32>(shine * 0.15);
            return mix(gold, indigo, engraving(p.xy));
        }

        @fragment fn fs(@builtin(position) fragCoord: vec4<f32>) -> @location(0) vec4<f32> {
            let uv = fragCoord.xy / u.resolution;
            let aspect = u.resolution.x / u.resolution.y;
            let t = u.time;

            // The nebula, turning slowly about the middle of the view; each open palm swirls it
            // round itself and glows orange - and a moving palm stirs a wider patch, swirls it
            // harder and churns it
            let middle = vec2<f32>(aspect * 0.5, 0.5);
            let drift = t * 0.012;
            let around = vec2<f32>(uv.x * aspect, uv.y) - middle;
            var f = middle + vec2<f32>(around.x * cos(drift) - around.y * sin(drift), around.x * sin(drift) + around.y * cos(drift));
            var glow = 0.0;
            for (var k = 0; k < 4; k = k + 1) {
                let palm = u.palms[k];
                if (palm.z > 0.001) {
                    let c = vec2<f32>(palm.x * aspect, palm.y);
                    let d = f - c;
                    let speed = min(palm.w, 1.5);
                    let g = palm.z * exp(-dot(d, d) * 7.0 / (1.0 + 1.2 * speed));
                    let turn = g * (1.4 + 3.2 * speed);
                    let cs = cos(turn);
                    let sn = sin(turn);
                    f = c + vec2<f32>(d.x * cs - d.y * sn, d.x * sn + d.y * cs) - d * g * 0.25;
                    f = f + (vec2<f32>(noise(f * 5.0 + t * 0.8), noise(f * 5.0 + vec2<f32>(4.1, 7.3) - t * 0.8)) - 0.5) * g * speed * 0.5;
                    glow = glow + palm.z * exp(-dot(d, d) * 9.0) * (0.55 + 0.45 * speed);
                }
            }
            let slow = t * 0.035;
            let p = f * 1.6;
            let w1 = vec2<f32>(fbm(p + vec2<f32>(slow, 0.0)), fbm(p + vec2<f32>(3.1, 1.7) - slow));
            let w2 = vec2<f32>(fbm(p + 2.2 * w1 + vec2<f32>(1.7, 9.2) + slow * 0.7), fbm(p + 2.2 * w1 + vec2<f32>(8.3, 2.8)));
            let cloud = fbm(p + 2.6 * w2);
            let wisps = fbm(p * 2.3 + 3.0 * w2 - vec2<f32>(slow * 1.3, 0.0));
            let dust = smoothstep(0.48, 0.78, fbm(p * 1.3 + w1 * 1.5 + vec2<f32>(5.0, 5.0)));

            // Deep space into indigo, purple clouds, magenta wisps, a mauve haze, orange cores
            var col = mix(vec3<f32>(0.015, 0.01, 0.05), vec3<f32>(0.10, 0.04, 0.30), smoothstep(0.2, 0.7, cloud));
            col = mix(col, vec3<f32>(0.34, 0.09, 0.48), smoothstep(0.35, 0.8, cloud * w1.x * 1.6));
            col = col + vec3<f32>(0.58, 0.12, 0.42) * pow(wisps, 3.0) * 0.7;
            col = col + vec3<f32>(0.62, 0.44, 0.64) * pow(w2.y, 4.0) * 0.45;
            let core = pow(clamp(cloud * w2.x * 2.0 - 0.15, 0.0, 1.0), 2.0);
            col = col + vec3<f32>(1.0, 0.48, 0.12) * core * 1.3;
            col = col + vec3<f32>(0.40, 0.14, 0.45) * 0.08 * u.activity;

            // Dark dust lanes; then a vivid orange glow round each palm, over the dust; then the
            // stars shining through
            col = col * (1.0 - dust * 0.55);
            col = mix(col, vec3<f32>(1.0, 0.52, 0.08), clamp(glow, 0.0, 0.8));
            col = col + vec3<f32>(1.0, 0.62, 0.20) * glow * glow * 0.35;
            let sky = fragCoord.xy / u.resolution.y;
            col = col + vec3<f32>(0.92, 0.90, 1.0) * (starField(sky, 90.0, t) + 0.6 * starField(sky + 7.3, 190.0, t));

            // The star: where this pixel lies in the star's frame (y up, radius 1)
            let radius = u.star.z;
            var s = (fragCoord.xy - u.star.xy) / radius;
            s.y = -s.y;
            let reach = length(s);

            // A soft golden halo, with eight faint rays that turn with the star
            let rays = pow(max(cos(8.0 * (atan2(s.x, s.y) - u.extra.x)), 0.0), 8.0);
            let halo = exp(-max(reach - 0.5, 0.0) * max(reach - 0.5, 0.0) * 3.0) * (0.45 + 0.25 * rays * exp(-reach));
            col = col + vec3<f32>(1.0, 0.80, 0.45) * halo * u.star.w * 1.2;

            if (reach < 1.08) {
                let origin = toStar(vec3<f32>(s, 2.0));
                let ray = toStar(vec3<f32>(0.0, 0.0, -1.0));
                var travelled = 0.0;
                var hit = false;
                var closest = 1000.0;
                var closestAt = 0.0;
                for (var i = 0; i < 80; i = i + 1) {
                    let d = solid(origin + ray * travelled);
                    if (d < closest) {
                        closest = d;
                        closestAt = travelled;
                    }
                    if (d < 0.0015) {
                        hit = true;
                        break;
                    }
                    travelled = travelled + max(d, 0.004);
                    if (travelled > 4.0) { break; }
                }

                // Antialiased edge: a near miss still covers part of the pixel
                let coverage = select(1.0 - smoothstep(0.0, 1.5 / radius, closest), 1.0, hit);
                if (coverage > 0.0) {
                    let at = select(closestAt, travelled, hit);
                    col = mix(col, shadeStar(origin + ray * at), coverage);
                }
            }

            return vec4<f32>(clamp(col, vec3<f32>(0.0), vec3<f32>(1.0)), 1.0);
        }
        """;

    //The star's radius at scale 1, as a fraction of the view's shorter side
    private const float RadiusFraction = 0.36f;
    private const int SceneFloats = 24 + (PalmAttractorField.MaxAttractors * 4);

    private readonly PalmMotion _palms;
    private readonly HolyInannaMotion _star = new();
    private readonly Func<float> _clock;
    private readonly TextureFormat _format;

    //The reused per-frame uniform block - OnRender allocates nothing
    private readonly float[] _sceneUniforms = new float[SceneFloats];

    private Queue _queue;
    private ShaderModule _module;
    private RenderPipeline _pipeline;
    private GpuBuffer _sceneBuffer;
    private BindGroup _group;

    /// <summary>Initializes a new instance of the <see cref="HolyInannaWebGpuRenderer"/> class.</summary>
    /// <param name="palms">The palms that move the nebula and the star; advanced by this renderer once per frame.</param>
    /// <param name="clock">The visual's clock, in seconds (it stops while the visual is paused).</param>
    /// <param name="format">The view's colour format (its <see cref="WebGpuRenderView.PreferredFormat"/>).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="palms"/> or
    /// <paramref name="clock"/> is null.</exception>
    public HolyInannaWebGpuRenderer(PalmMotion palms, Func<float> clock, TextureFormat format)
    {
        _palms = palms ?? throw new ArgumentNullException(nameof(palms));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _format = format;
    }

    /// <inheritdoc />
    public void OnDeviceReady(IWebGpuDeviceSource device)
    {
        Release();
        _queue = device.Queue;
        _module = WgslGpu.CreateShaderModule(device.Device, SceneWgsl);
        _pipeline = WgslGpu.CreateRenderPipeline(device.Device, _module, _format);
        _sceneBuffer = WgslGpu.CreateBuffer(device.Device, BufferUsage.Uniform | BufferUsage.CopyDst, SceneFloats * sizeof(float));

        BindGroupLayout layout = Wgpu.RenderPipelineGetBindGroupLayout(_pipeline, 0);
        _group = WgslGpu.CreateBindGroup(device.Device, layout, new[] { (_sceneBuffer, SceneFloats * sizeof(float)) });
        Wgpu.BindGroupLayoutRelease(layout);
    }

    /// <inheritdoc />
    public unsafe void OnRender(WebGpuFrame frame)
    {
        if (_group.IsNull) { return; }

        float width = frame.Width;
        float height = frame.Height;
        _palms.Advance(_clock());
        _star.Advance(_palms);

        // The star's place in pixels, kept whole inside the view
        float radius = _star.Scale * RadiusFraction * Math.Min(width, height);
        float margin = radius * 1.05f;
        float centerX = _star.CenterX * width;
        float centerY = _star.CenterY * height;
        if (width > 2f * margin) { centerX = Math.Clamp(centerX, margin, width - margin); }
        if (height > 2f * margin) { centerY = Math.Clamp(centerY, margin, height - margin); }

        _sceneUniforms[0] = width;
        _sceneUniforms[1] = height;
        _sceneUniforms[2] = _palms.Time;
        _sceneUniforms[3] = _palms.Activity;
        _sceneUniforms[4] = centerX;
        _sceneUniforms[5] = centerY;
        _sceneUniforms[6] = radius;
        _sceneUniforms[7] = 0.35f + (0.35f * _palms.Activity);
        _sceneUniforms[8] = _star.Spin;
        _sceneUniforms[9] = 0f;
        _sceneUniforms[10] = 0f;
        _sceneUniforms[11] = 0f;
        WriteViewToStar(_sceneUniforms.AsSpan(12, 12), _star.TiltX, _star.TiltY, _star.Spin);
        _palms.Palms.CopyTo(_sceneUniforms.AsSpan(24));
        WgslGpu.WriteBuffer(_queue, _sceneBuffer, _sceneUniforms);

        var attachment = new RenderPassColorAttachment
        {
            View = frame.TextureView,
            DepthSlice = Constants.DepthSliceUndefined,
            LoadOp = LoadOp.Load,
            StoreOp = StoreOp.Store,
        };
        var descriptor = new RenderPassDescriptor { ColorAttachmentCount = 1, ColorAttachments = &attachment };
        RenderPassEncoder pass = Wgpu.CommandEncoderBeginRenderPass(frame.Encoder, &descriptor);
        Wgpu.RenderPassEncoderSetPipeline(pass, _pipeline);
        Wgpu.RenderPassEncoderSetBindGroup(pass, 0, _group, 0, null);
        Wgpu.RenderPassEncoderDraw(pass, 3, 1, 0, 0);
        Wgpu.RenderPassEncoderEnd(pass);
        Wgpu.RenderPassEncoderRelease(pass);
    }

    /// <inheritdoc />
    public void OnDeviceLost(string reason) => Release();

    /// <summary>Releases the GPU objects (the view releases its device itself).</summary>
    public void Dispose() => Release();

    /// <summary>
    /// Writes the rotation from view space into the star's own space as three rows of four
    /// floats (the fourth unused). The star is turned by its spin about its own axis, then
    /// tilted about the view's vertical and horizontal axes: view = Rx(tiltX) Ry(tiltY) Rz(spin)
    /// star, so view-to-star is that matrix transposed.
    /// </summary>
    internal static void WriteViewToStar(Span<float> rows, float tiltX, float tiltY, float spin)
    {
        float cx = MathF.Cos(tiltX), sx = MathF.Sin(tiltX);
        float cy = MathF.Cos(tiltY), sy = MathF.Sin(tiltY);
        float cz = MathF.Cos(spin), sz = MathF.Sin(spin);

        // Ry(tiltY) Rz(spin)
        float a00 = cy * cz, a01 = -cy * sz, a02 = sy;
        float a10 = sz, a11 = cz, a12 = 0f;
        float a20 = -sy * cz, a21 = sy * sz, a22 = cy;

        // R = Rx(tiltX) (Ry Rz)
        float r00 = a00, r01 = a01, r02 = a02;
        float r10 = (cx * a10) - (sx * a20), r11 = (cx * a11) - (sx * a21), r12 = (cx * a12) - (sx * a22);
        float r20 = (sx * a10) + (cx * a20), r21 = (sx * a11) + (cx * a21), r22 = (sx * a12) + (cx * a22);

        // Rows of R transposed: the columns of R
        rows[0] = r00; rows[1] = r10; rows[2] = r20; rows[3] = 0f;
        rows[4] = r01; rows[5] = r11; rows[6] = r21; rows[7] = 0f;
        rows[8] = r02; rows[9] = r12; rows[10] = r22; rows[11] = 0f;
    }

    private void Release()
    {
        if (!_group.IsNull) { Wgpu.BindGroupRelease(_group); _group = default; }
        if (!_sceneBuffer.IsNull) { Wgpu.BufferRelease(_sceneBuffer); _sceneBuffer = default; }
        if (!_pipeline.IsNull) { Wgpu.RenderPipelineRelease(_pipeline); _pipeline = default; }
        if (!_module.IsNull) { Wgpu.ShaderModuleRelease(_module); _module = default; }
        _queue = default;
    }
}
