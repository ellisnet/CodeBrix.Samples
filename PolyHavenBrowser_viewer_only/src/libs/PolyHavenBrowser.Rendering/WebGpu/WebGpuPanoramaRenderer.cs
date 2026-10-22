using System.Numerics;
using CodeBrix.WebGpu.Native;
using CodeBrix.WebGpu.Skia;
using SkiaSharp;
using GpuBuffer = CodeBrix.WebGpu.Native.Buffer;

namespace PolyHavenBrowser.Rendering;

/// <summary>
/// A <b>WebGPU</b> renderer for interactive drag-to-look viewing of equirectangular HDR panoramas -
/// the GPU counterpart of <see cref="EquirectPanoramaRenderer"/>, with the same camera, exposure and
/// tone-map controls and the same per-pixel math (see <see cref="WebGpuShaders.Panorama"/>). The
/// panorama is uploaded once as a half-float texture; each frame is a single full-screen triangle
/// into a headless CodeBrix.WebGpu.Skia target, read back into the caller's bitmap.
/// <para>
/// Unlike the model renderers this one sets itself up <b>eagerly</b>, in the constructor: device,
/// pipeline and texture upload all happen there, so a caller can build it off the UI thread and a
/// machine without a working WebGPU driver fails right away - the cue to fall back to the CPU
/// renderer. After construction it may be used from one thread at a time.
/// </para>
/// </summary>
public sealed unsafe class WebGpuPanoramaRenderer : IDisposable
{
    private const TextureFormat PanoramaFormat = TextureFormat.RGBA16Float;
    private const TextureFormat TargetFormat = TextureFormat.RGBA8Unorm;

    // The 64-byte view block; layout matches the WGSL `View` struct.
    private struct ViewUniforms
    {
        public Vector4 Forward;
        public Vector4 Right;
        public Vector4 Up;
        public Vector4 Params;
    }

    private readonly WebGpuSkiaDevice _gpu;
    private ShaderModule _module;
    private BindGroupLayout _bindGroupLayout;
    private PipelineLayout _pipelineLayout;
    private RenderPipeline _pipeline;
    private GpuBuffer _viewBuffer;
    private Texture _panoramaTexture;
    private TextureView _panoramaView;
    private Sampler _sampler;
    private BindGroup _bindGroup;

    private WebGpuSkiaTarget? _target;
    private bool _disposed;

    /// <summary>
    /// Creates the renderer and uploads the panorama to the GPU. Throws when no WebGPU device is
    /// available or WebGPU rejects the setup.
    /// </summary>
    public WebGpuPanoramaRenderer(FloatImage panorama)
    {
        ArgumentNullException.ThrowIfNull(panorama);

        _gpu = WebGpuSupport.CreateDevice();
        try
        {
            CreatePipeline();
            UploadPanorama(FitToDevice(panorama, _gpu.Limits.MaxTextureDimension2D));
            CreateBindGroup();
            WebGpuSupport.ThrowIfErrors(_gpu, "panorama setup");
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    /// <summary>The view state (yaw/pitch/fov) the next render uses.</summary>
    public PanoramaCamera Camera { get; } = new();

    /// <summary>The linear exposure multiplier applied before tone mapping. Defaults to 1.</summary>
    public float Exposure { get; set; } = 1f;

    /// <summary>The tone-map operator. Defaults to <see cref="ToneMapOperator.AcesFilmic"/>.</summary>
    public ToneMapOperator Operator { get; set; } = ToneMapOperator.AcesFilmic;

    /// <summary>The GPU API the device runs on (Direct3D 12, Vulkan or Metal), for status display.</summary>
    public string BackendName => _gpu.Backend.ToString();

    /// <summary>Renders the current view into a new bitmap of the given size.</summary>
    public SKBitmap Render(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Opaque));
        RenderTo(bitmap);
        return bitmap;
    }

    /// <summary>
    /// Renders the current view into an existing RGBA bitmap (reuse the bitmap across frames while
    /// dragging to avoid per-frame allocations).
    /// </summary>
    public void RenderTo(SKBitmap target)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(target);
        if (target.ColorType != SKColorType.Rgba8888)
        {
            throw new ArgumentException($"Target bitmap must be {SKColorType.Rgba8888}.", nameof(target));
        }

        var renderTarget = EnsureTarget(target.Width, target.Height);

        // The view block goes in through a queue write, which runs at the start of the submit
        // EndWebGpuPass makes - before the pass below reads it.
        var view = BuildViewUniforms(target.Width, target.Height);
        WebGpu.QueueWriteBuffer(_gpu.Queue, _viewBuffer, 0, &view, (nuint)sizeof(ViewUniforms));

        var encoder = renderTarget.BeginWebGpuPass();
        var color = new RenderPassColorAttachment
        {
            View = renderTarget.TextureView,
            DepthSlice = Constants.DepthSliceUndefined,
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            ClearValue = new Color { R = 0, G = 0, B = 0, A = 1 },
        };
        var passDescriptor = new RenderPassDescriptor { ColorAttachmentCount = 1, ColorAttachments = &color };
        var pass = WebGpu.CommandEncoderBeginRenderPass(encoder, &passDescriptor);
        WebGpu.RenderPassEncoderSetPipeline(pass, _pipeline);
        WebGpu.RenderPassEncoderSetBindGroup(pass, 0, _bindGroup, 0, null);
        WebGpu.RenderPassEncoderDraw(pass, 3, 1, 0, 0);
        WebGpu.RenderPassEncoderEnd(pass);
        WebGpu.RenderPassEncoderRelease(pass);
        renderTarget.EndWebGpuPass();

        var bytes = new Span<byte>((void*)target.GetPixels(), target.RowBytes * target.Height);
        if (!renderTarget.ReadPixels(bytes, target.RowBytes))
        {
            throw new InvalidOperationException("WebGPU panorama readback failed: " + renderTarget.LastError);
        }
    }

    // The camera basis, built exactly as EquirectPanoramaRenderer builds it (yaw 0 / pitch 0 looks
    // down -Z, the horizontal center of the panorama), pre-scaled by the field of view and aspect.
    private ViewUniforms BuildViewUniforms(int width, int height)
    {
        var yaw = Camera.YawDegrees * MathF.PI / 180f;
        var pitch = Camera.PitchDegrees * MathF.PI / 180f;
        var forward = new Vector3(
            MathF.Cos(pitch) * MathF.Sin(yaw),
            MathF.Sin(pitch),
            -MathF.Cos(pitch) * MathF.Cos(yaw));
        var right = new Vector3(MathF.Cos(yaw), 0f, MathF.Sin(yaw));
        var up = Vector3.Cross(right, forward);

        var tanHalfFov = MathF.Tan(Camera.FovDegrees * MathF.PI / 360f);
        var aspect = (float)width / height;

        return new ViewUniforms
        {
            Forward = new Vector4(forward, 0f),
            Right = new Vector4(right * (tanHalfFov * aspect), 0f),
            Up = new Vector4(up * tanHalfFov, 0f),
            Params = new Vector4(Exposure, (float)Operator, 0f, 0f),
        };
    }

    // The headless target the frames render into and are read back from; re-created on a size change.
    private WebGpuSkiaTarget EnsureTarget(int width, int height)
    {
        if (_target is not null && _target.Width == width && _target.Height == height)
        {
            return _target;
        }

        _target?.Dispose();
        _target = WebGpuSkiaTarget.CreateHeadless(_gpu, width, height, new WebGpuSkiaTargetOptions { Format = TargetFormat });
        if (!_target.IsAvailable)
        {
            throw new InvalidOperationException("WebGPU render target unavailable: " + _target.TierReason);
        }
        return _target;
    }

    #region | One-time setup: pipeline, panorama texture, bind group |

    private void CreatePipeline()
    {
        var device = _gpu.Device;
        _module = WebGpuSupport.CreateShaderModule(device, WebGpuShaders.Panorama);

        var entries = stackalloc BindGroupLayoutEntry[3];
        entries[0] = new BindGroupLayoutEntry
        {
            Binding = 0,
            Visibility = ShaderStage.Fragment,
            Buffer = new BufferBindingLayout { Type = BufferBindingType.Uniform, MinBindingSize = (ulong)sizeof(ViewUniforms) },
        };
        entries[1] = new BindGroupLayoutEntry
        {
            Binding = 1,
            Visibility = ShaderStage.Fragment,
            Texture = new TextureBindingLayout { SampleType = TextureSampleType.Float, ViewDimension = TextureViewDimension.Dimension2D },
        };
        entries[2] = new BindGroupLayoutEntry
        {
            Binding = 2,
            Visibility = ShaderStage.Fragment,
            Sampler = new SamplerBindingLayout { Type = SamplerBindingType.Filtering },
        };
        var layoutDescriptor = new BindGroupLayoutDescriptor { EntryCount = 3, Entries = entries };
        _bindGroupLayout = WebGpu.DeviceCreateBindGroupLayout(device, &layoutDescriptor);

        var bindGroupLayout = _bindGroupLayout;
        var pipelineLayoutDescriptor = new PipelineLayoutDescriptor { BindGroupLayoutCount = 1, BindGroupLayouts = &bindGroupLayout };
        _pipelineLayout = WebGpu.DeviceCreatePipelineLayout(device, &pipelineLayoutDescriptor);

        fixed (byte* vsName = "vs_main"u8)
        fixed (byte* fsName = "fs_main"u8)
        {
            var colorTarget = new ColorTargetState { Format = TargetFormat, WriteMask = ColorWriteMask.All };
            var fragment = new FragmentState
            {
                Module = _module,
                EntryPoint = WgpuString.FromPinned(fsName, 7),
                TargetCount = 1,
                Targets = &colorTarget,
            };
            var descriptor = new RenderPipelineDescriptor
            {
                Layout = _pipelineLayout,
                Vertex = new VertexState { Module = _module, EntryPoint = WgpuString.FromPinned(vsName, 7) },
                Primitive = new PrimitiveState { Topology = PrimitiveTopology.TriangleList, FrontFace = FrontFace.CCW, CullMode = CullMode.None },
                Multisample = new MultisampleState { Count = 1, Mask = 0xFFFFFFFF },
                Fragment = &fragment,
            };
            _pipeline = WebGpu.DeviceCreateRenderPipeline(device, &descriptor);
        }

        var bufferDescriptor = new BufferDescriptor { Usage = BufferUsage.Uniform | BufferUsage.CopyDst, Size = (ulong)sizeof(ViewUniforms) };
        _viewBuffer = WebGpu.DeviceCreateBuffer(device, &bufferDescriptor);

        // Bilinear, with u wrapping around the panorama's seam and v clamped at the poles - the same
        // addressing EquirectPanoramaRenderer.SampleDirection does by hand.
        var samplerDescriptor = new SamplerDescriptor
        {
            AddressModeU = AddressMode.Repeat,
            AddressModeV = AddressMode.ClampToEdge,
            AddressModeW = AddressMode.ClampToEdge,
            MagFilter = FilterMode.Linear,
            MinFilter = FilterMode.Linear,
            MipmapFilter = MipmapFilterMode.Nearest,
            LodMinClamp = 0f,
            LodMaxClamp = 32f,
            MaxAnisotropy = 1,
        };
        _sampler = WebGpu.DeviceCreateSampler(device, &samplerDescriptor);

        if (_module.IsNull || _pipeline.IsNull || _viewBuffer.IsNull || _sampler.IsNull)
        {
            throw new InvalidOperationException("WebGPU panorama pipeline creation failed.");
        }
    }

    // Uploads the linear-light RGB panorama as an RGBA half-float texture: filterable on every
    // WebGPU device (32-bit float filtering is an optional feature), and wide enough for HDR -
    // values beyond the half-float maximum are clamped to it, which tone-maps to white anyway.
    private void UploadPanorama(FloatImage panorama)
    {
        var width = (uint)panorama.Width;
        var height = (uint)panorama.Height;
        _panoramaTexture = WebGpuSupport.CreateTexture(_gpu.Device, width, height, PanoramaFormat,
            TextureUsage.TextureBinding | TextureUsage.CopyDst);
        if (_panoramaTexture.IsNull)
        {
            throw new InvalidOperationException("WebGPU panorama texture creation failed.");
        }
        _panoramaView = WebGpu.TextureCreateView(_panoramaTexture, null);

        var source = panorama.Pixels;
        var halves = new ushort[panorama.Width * panorama.Height * 4];
        var one = BitConverter.HalfToUInt16Bits((Half)1f);
        Parallel.For(0, panorama.Height, y =>
        {
            var src = y * panorama.Width * 3;
            var dst = y * panorama.Width * 4;
            for (var x = 0; x < panorama.Width; x++)
            {
                halves[dst] = ToHalfBits(source[src]);
                halves[dst + 1] = ToHalfBits(source[src + 1]);
                halves[dst + 2] = ToHalfBits(source[src + 2]);
                halves[dst + 3] = one;
                src += 3;
                dst += 4;
            }
        });

        fixed (ushort* data = halves)
        {
            WebGpuSupport.WriteTexture(_gpu.Queue, _panoramaTexture, data, (nuint)(halves.Length * sizeof(ushort)), width, height, 8);
        }
    }

    private static ushort ToHalfBits(float value)
    {
        // NaN and negatives become black (as the CPU tone mapper treats them); overly bright
        // values clamp to the largest finite half instead of overflowing to infinity.
        var clamped = float.IsNaN(value) ? 0f : Math.Clamp(value, 0f, (float)Half.MaxValue);
        return BitConverter.HalfToUInt16Bits((Half)clamped);
    }

    private void CreateBindGroup()
    {
        var entries = stackalloc BindGroupEntry[3];
        entries[0] = new BindGroupEntry { Binding = 0, Buffer = _viewBuffer, Size = (ulong)sizeof(ViewUniforms) };
        entries[1] = new BindGroupEntry { Binding = 1, TextureView = _panoramaView };
        entries[2] = new BindGroupEntry { Binding = 2, Sampler = _sampler };
        var descriptor = new BindGroupDescriptor { Layout = _bindGroupLayout, EntryCount = 3, Entries = entries };
        _bindGroup = WebGpu.DeviceCreateBindGroup(_gpu.Device, &descriptor);
    }

    // Poly Haven ships HDRIs up to 16k wide, past the 8192 texel limit many GPUs have: halve the
    // panorama (2x2 box filter) until it fits the device. The sample app downloads 1k HDRIs, which
    // always fit, so this only matters to code that reuses the renderer.
    internal static FloatImage FitToDevice(FloatImage panorama, uint maxDimension)
    {
        while (maxDimension > 0 && (panorama.Width > maxDimension || panorama.Height > maxDimension))
        {
            var width = Math.Max(1, panorama.Width / 2);
            var height = Math.Max(1, panorama.Height / 2);
            var pixels = new float[width * height * 3];
            var source = panorama;
            Parallel.For(0, height, y =>
            {
                var y0 = Math.Min(y * 2, source.Height - 1);
                var y1 = Math.Min(y * 2 + 1, source.Height - 1);
                for (var x = 0; x < width; x++)
                {
                    var x0 = Math.Min(x * 2, source.Width - 1);
                    var x1 = Math.Min(x * 2 + 1, source.Width - 1);
                    for (var c = 0; c < 3; c++)
                    {
                        pixels[(y * width + x) * 3 + c] = 0.25f * (
                            source.Pixels[(y0 * source.Width + x0) * 3 + c] +
                            source.Pixels[(y0 * source.Width + x1) * 3 + c] +
                            source.Pixels[(y1 * source.Width + x0) * 3 + c] +
                            source.Pixels[(y1 * source.Width + x1) * 3 + c]);
                    }
                }
            });
            panorama = new FloatImage(width, height, pixels);
        }
        return panorama;
    }

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;

        // Targets before their device; every handle we created, newest first.
        _target?.Dispose();
        _target = null;

        if (!_bindGroup.IsNull) { WebGpu.BindGroupRelease(_bindGroup); }
        if (!_sampler.IsNull) { WebGpu.SamplerRelease(_sampler); }
        if (!_panoramaView.IsNull) { WebGpu.TextureViewRelease(_panoramaView); }
        if (!_panoramaTexture.IsNull) { WebGpu.TextureRelease(_panoramaTexture); }
        if (!_viewBuffer.IsNull) { WebGpu.BufferRelease(_viewBuffer); }
        if (!_pipeline.IsNull) { WebGpu.RenderPipelineRelease(_pipeline); }
        if (!_pipelineLayout.IsNull) { WebGpu.PipelineLayoutRelease(_pipelineLayout); }
        if (!_bindGroupLayout.IsNull) { WebGpu.BindGroupLayoutRelease(_bindGroupLayout); }
        if (!_module.IsNull) { WebGpu.ShaderModuleRelease(_module); }

        _gpu.Dispose();
    }
}
