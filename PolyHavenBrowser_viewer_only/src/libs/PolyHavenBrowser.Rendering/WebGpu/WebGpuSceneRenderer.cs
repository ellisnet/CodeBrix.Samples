using System.Numerics;
using CodeBrix.WebGpu.Native;
using CodeBrix.WebGpu.Skia;
using GpuBuffer = CodeBrix.WebGpu.Native.Buffer;

namespace PolyHavenBrowser.Rendering;

/// <summary>
/// A self-contained <b>offscreen WebGPU renderer</b> for previewing one <see cref="LoadedModel"/>
/// with an orbit camera - the WebGPU counterpart of <see cref="GlModelSceneRenderer"/>,
/// <c>VulkanSceneRenderer</c> and <see cref="MetalSceneRenderer"/>, drawing the same scene with the
/// same shading (the WGSL in <see cref="WebGpuShaders.Model"/> mirrors the GL/SPIR-V/MSL shaders).
/// It is built on CodeBrix.WebGpu: <c>WebGpuSkiaDevice</c> owns the device (Direct3D 12 on Windows,
/// Vulkan on Linux and Android, Metal on macOS) and a headless <c>WebGpuSkiaTarget</c> is the colour
/// target each frame renders into and is read back from; pipelines, buffers and draws are the raw
/// CodeBrix.WebGpu.Native binding.
/// <para>
/// Like the Vulkan and Metal renderers, WebGPU has no ambient thread context - so this class owns
/// its whole stack and cannot disturb the host head's own renderer. Everything is created lazily on
/// the first <see cref="RenderFrame"/> call; <see cref="SetModel"/> may be called from any thread
/// and the GPU upload happens on the next render.
/// </para>
/// <para>
/// Coordinate conventions: the camera's System.Numerics matrices are used <b>unmodified</b> (WGSL
/// reads <c>mat4x4</c> column-major, applying the implicit transpose the other backends rely on), and
/// the projection's [0, 1] depth range is WebGPU's own. WebGPU's clip-space Y points up while its
/// framebuffer origin is top-left - the same pairing as Metal - so the readback comes out
/// <b>top-down</b>. Culling is disabled, matching the other renderers, because Poly Haven models
/// frequently rely on double-sided geometry.
/// </para>
/// </summary>
public sealed unsafe class WebGpuSceneRenderer : IDisposable
{
    private const TextureFormat ColorFormat = TextureFormat.RGBA8Unorm;
    private const TextureFormat DepthFormat = TextureFormat.Depth32Float;

    // The 112-byte per-draw block; layout matches the WGSL `Uniforms` struct (mvp, baseColorFactor,
    // lightDirection, flags). The Matrix4x4 is written in .NET's row-major order and WGSL reads a
    // mat4x4 column-major - the same implicit transpose the other backends rely on.
    private struct Uniforms
    {
        public Matrix4x4 Mvp;
        public Vector4 BaseColorFactor;
        public Vector4 LightDirection;
        public int HasTexture;
        public int DoubleSided;
        public int Pad0;
        public int Pad1;
    }

    // One primitive's GPU buffers plus its own uniform buffer and bind group: every uniform write
    // of a frame lands before that frame's single submit, so each draw needs a buffer of its own.
    private sealed record GpuPrimitive(
        GpuBuffer Positions, GpuBuffer Normals, GpuBuffer TexCoords, GpuBuffer Indices,
        uint IndexCount, int MaterialIndex, GpuBuffer UniformBuffer, BindGroup UniformGroup);

    // A sampleable texture with the bind group (texture + sampler) a draw binds it through.
    private readonly record struct GpuTexture(Texture Texture, TextureView View, BindGroup Group);

    private readonly object _pendingLock = new();
    private LoadedModel? _pendingModel;
    private bool _pendingFrameCamera;
    private bool _hasPendingModel;

    private LoadedModel? _currentModel;
    private readonly List<GpuPrimitive> _gpuPrimitives = [];
    private readonly Dictionary<int, GpuTexture> _materialTextures = [];

    private WebGpuSkiaDevice? _gpu;
    private ShaderModule _module;
    private BindGroupLayout _uniformLayout;   // group 0: the per-draw uniform block
    private BindGroupLayout _textureLayout;   // group 1: base-colour texture + sampler
    private PipelineLayout _pipelineLayout;
    private RenderPipeline _opaquePipeline;   // blending disabled, depth writes on
    private RenderPipeline _blendPipeline;    // straight-alpha "over", depth writes off
    private Sampler _sampler;
    private GpuTexture _whiteTexture;         // 1x1 white fallback bound for untextured materials

    private WebGpuSkiaTarget? _target;
    private Texture _depthTexture;
    private TextureView _depthView;

    private bool _initialized;
    private bool _disposed;

    /// <summary>The orbit camera driven by the host's pointer/scroll input.</summary>
    public OrbitCamera Camera { get; } = new();

    /// <summary>
    /// A fixed world-space light direction (pointing toward the light), or <see langword="null"/>
    /// (the default) for a camera headlight with double-sided lighting - the same semantics as
    /// <see cref="GlModelSceneRenderer.FixedLightDirection"/>.
    /// </summary>
    public Vector3? FixedLightDirection { get; set; }

    /// <summary>
    /// Whether a usable WebGPU device can be created on this machine. Intended for tests; the app
    /// itself pre-warms the engine and falls back when it fails.
    /// </summary>
    public static bool IsRuntimeAvailable() => WebGpuSupport.IsRuntimeAvailable();

    /// <summary>
    /// Sets the model to display (or <see langword="null"/> to clear). Takes effect on the next
    /// <see cref="RenderFrame"/>; when <paramref name="frameCamera"/> is true the camera is
    /// re-framed to the model's bounds at that time. Safe to call from any thread.
    /// </summary>
    public void SetModel(LoadedModel? model, bool frameCamera = true)
    {
        lock (_pendingLock)
        {
            _pendingModel = model;
            _pendingFrameCamera = frameCamera;
            _hasPendingModel = true;
        }
    }

    /// <summary>
    /// Renders the current model at the given pixel size over the given background colour and
    /// returns the frame as tightly packed RGBA bytes. As explained in the class remarks the image
    /// is top-down; call on the render thread.
    /// </summary>
    public byte[] RenderFrame(int width, int height, (float R, float G, float B, float A) background)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        EnsureInitialized();
        ApplyPendingModel();
        var target = EnsureRenderTargets(width, height);
        var queue = _gpu!.Queue;

        var drawModel = _currentModel is not null && _gpuPrimitives.Count > 0;
        if (drawModel)
        {
            // Each primitive's block goes in through a queue write; all of them run at the start of
            // the submit EndWebGpuPass makes, before the pass below reads them.
            var uniforms = BuildFrameUniforms(width, height);
            foreach (var primitive in _gpuPrimitives)
            {
                var block = BuildPrimitiveUniforms(uniforms, primitive);
                WebGpu.QueueWriteBuffer(queue, primitive.UniformBuffer, 0, &block, (nuint)sizeof(Uniforms));
            }
        }

        // Clear the colour target to the background and the depth target to the far plane; keep
        // only the colour result (depth is transient).
        var encoder = target.BeginWebGpuPass();
        var color = new RenderPassColorAttachment
        {
            View = target.TextureView,
            DepthSlice = Constants.DepthSliceUndefined,
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            ClearValue = new Color { R = background.R, G = background.G, B = background.B, A = background.A },
        };
        var depth = new RenderPassDepthStencilAttachment
        {
            View = _depthView,
            DepthLoadOp = LoadOp.Clear,
            DepthStoreOp = StoreOp.Discard,
            DepthClearValue = 1f,
        };
        var passDescriptor = new RenderPassDescriptor
        {
            ColorAttachmentCount = 1,
            ColorAttachments = &color,
            DepthStencilAttachment = &depth,
        };
        var pass = WebGpu.CommandEncoderBeginRenderPass(encoder, &passDescriptor);

        if (drawModel)
        {
            // Two passes, exactly as the other renderers: opaque (and mask) first with depth writes
            // on, then translucent (BLEND) primitives with straight-alpha "over" blending and depth
            // writes off, so glass surfaces show what's behind them.
            WebGpu.RenderPassEncoderSetPipeline(pass, _opaquePipeline);
            DrawPrimitives(pass, blendPass: false);
            WebGpu.RenderPassEncoderSetPipeline(pass, _blendPipeline);
            DrawPrimitives(pass, blendPass: true);
        }

        WebGpu.RenderPassEncoderEnd(pass);
        WebGpu.RenderPassEncoderRelease(pass);
        target.EndWebGpuPass();

        var pixels = new byte[width * height * 4];
        if (!target.ReadPixels(pixels))
        {
            throw new InvalidOperationException("WebGPU readback failed: " + target.LastError);
        }
        return pixels;
    }

    // The per-frame values shared by every primitive: the camera MVP, the light direction, and the
    // double-sided flag. BaseColorFactor and HasTexture are set per primitive.
    private Uniforms BuildFrameUniforms(int width, int height)
    {
        // MVP = view * projection (node transforms baked at load time), uploaded WITHOUT an extra
        // transpose - WGSL's column-major read applies the transpose the other backends rely on.
        var mvp = Camera.GetViewMatrix() * Camera.GetProjectionMatrix(width / (float)height);

        var lightDirection = FixedLightDirection is { } fixedLight && fixedLight != Vector3.Zero
            ? Vector3.Normalize(fixedLight)
            : Vector3.Normalize(Camera.GetEyePosition() - Camera.Target);

        return new Uniforms
        {
            Mvp = mvp,
            LightDirection = new Vector4(lightDirection, 0f),
            DoubleSided = FixedLightDirection is null ? 1 : 0,
        };
    }

    // BLEND materials get a fixed preview opacity so glass surfaces let the geometry behind them
    // show through. `uniforms` is a by-value copy completed for this primitive.
    private Uniforms BuildPrimitiveUniforms(Uniforms uniforms, GpuPrimitive primitive)
    {
        var material = MaterialOf(primitive);
        var isBlend = material?.AlphaMode == ModelAlphaMode.Blend;
        var baseColor = material?.BaseColorFactor ?? Vector4.One;
        var alpha = isBlend ? ModelMaterial.BlendPreviewOpacity * baseColor.W : baseColor.W;

        uniforms.BaseColorFactor = new Vector4(baseColor.X, baseColor.Y, baseColor.Z, alpha);
        uniforms.HasTexture = _materialTextures.ContainsKey(primitive.MaterialIndex) ? 1 : 0;
        return uniforms;
    }

    // Draws either the opaque/mask primitives (blendPass = false) or the translucent BLEND
    // primitives (blendPass = true).
    private void DrawPrimitives(RenderPassEncoder pass, bool blendPass)
    {
        foreach (var primitive in _gpuPrimitives)
        {
            var isBlend = MaterialOf(primitive)?.AlphaMode == ModelAlphaMode.Blend;
            if (isBlend != blendPass)
            {
                continue;
            }

            // The shader's hasTexture flag ignores the sample when false, but a texture must still
            // be bound - the 1x1 white fallback serves untextured materials.
            var texture = _materialTextures.TryGetValue(primitive.MaterialIndex, out var materialTexture)
                ? materialTexture
                : _whiteTexture;

            WebGpu.RenderPassEncoderSetBindGroup(pass, 0, primitive.UniformGroup, 0, null);
            WebGpu.RenderPassEncoderSetBindGroup(pass, 1, texture.Group, 0, null);
            WebGpu.RenderPassEncoderSetVertexBuffer(pass, 0, primitive.Positions, 0, Constants.WholeSize);
            WebGpu.RenderPassEncoderSetVertexBuffer(pass, 1, primitive.Normals, 0, Constants.WholeSize);
            WebGpu.RenderPassEncoderSetVertexBuffer(pass, 2, primitive.TexCoords, 0, Constants.WholeSize);
            WebGpu.RenderPassEncoderSetIndexBuffer(pass, primitive.Indices, IndexFormat.Uint32, 0, Constants.WholeSize);
            WebGpu.RenderPassEncoderDrawIndexed(pass, primitive.IndexCount, 1, 0, 0, 0);
        }
    }

    private ModelMaterial? MaterialOf(GpuPrimitive primitive) =>
        primitive.MaterialIndex >= 0 && primitive.MaterialIndex < _currentModel!.Materials.Count
            ? _currentModel.Materials[primitive.MaterialIndex]
            : null;

    #region | One-time initialization: device, shaders, pipelines |

    private void EnsureInitialized()
    {
        if (_initialized) { return; }
        if (_gpu is not null)
        {
            // An earlier attempt got a device but failed to build the pipelines; don't stack up
            // devices by retrying every frame.
            throw new InvalidOperationException("WebGPU initialization failed earlier on this renderer.");
        }

        _gpu = WebGpuSupport.CreateDevice();
        var device = _gpu.Device;

        _module = WebGpuSupport.CreateShaderModule(device, WebGpuShaders.Model);
        CreateLayouts(device);
        _opaquePipeline = BuildPipeline(device, blend: false);
        _blendPipeline = BuildPipeline(device, blend: true);

        var samplerDescriptor = new SamplerDescriptor
        {
            AddressModeU = AddressMode.Repeat,
            AddressModeV = AddressMode.Repeat,
            AddressModeW = AddressMode.Repeat,
            MagFilter = FilterMode.Linear,
            MinFilter = FilterMode.Linear,
            MipmapFilter = MipmapFilterMode.Nearest,
            LodMinClamp = 0f,
            LodMaxClamp = 32f,
            MaxAnisotropy = 1,
        };
        _sampler = WebGpu.DeviceCreateSampler(device, &samplerDescriptor);

        byte* white = stackalloc byte[] { 255, 255, 255, 255 };
        _whiteTexture = UploadTexture(white, 4, 1, 1);

        if (_module.IsNull || _opaquePipeline.IsNull || _blendPipeline.IsNull || _sampler.IsNull)
        {
            throw new InvalidOperationException("WebGPU pipeline creation failed.");
        }
        WebGpuSupport.ThrowIfErrors(_gpu, "pipeline setup");

        _initialized = true;
    }

    private void CreateLayouts(Device device)
    {
        var uniformEntry = new BindGroupLayoutEntry
        {
            Binding = 0,
            Visibility = ShaderStage.Vertex | ShaderStage.Fragment,
            Buffer = new BufferBindingLayout { Type = BufferBindingType.Uniform, MinBindingSize = (ulong)sizeof(Uniforms) },
        };
        var uniformDescriptor = new BindGroupLayoutDescriptor { EntryCount = 1, Entries = &uniformEntry };
        _uniformLayout = WebGpu.DeviceCreateBindGroupLayout(device, &uniformDescriptor);

        var textureEntries = stackalloc BindGroupLayoutEntry[2];
        textureEntries[0] = new BindGroupLayoutEntry
        {
            Binding = 0,
            Visibility = ShaderStage.Fragment,
            Texture = new TextureBindingLayout { SampleType = TextureSampleType.Float, ViewDimension = TextureViewDimension.Dimension2D },
        };
        textureEntries[1] = new BindGroupLayoutEntry
        {
            Binding = 1,
            Visibility = ShaderStage.Fragment,
            Sampler = new SamplerBindingLayout { Type = SamplerBindingType.Filtering },
        };
        var textureDescriptor = new BindGroupLayoutDescriptor { EntryCount = 2, Entries = textureEntries };
        _textureLayout = WebGpu.DeviceCreateBindGroupLayout(device, &textureDescriptor);

        // Explicit layouts (not "auto"), so one set of bind groups serves both pipelines.
        var layouts = stackalloc BindGroupLayout[2] { _uniformLayout, _textureLayout };
        var pipelineLayoutDescriptor = new PipelineLayoutDescriptor { BindGroupLayoutCount = 2, BindGroupLayouts = layouts };
        _pipelineLayout = WebGpu.DeviceCreatePipelineLayout(device, &pipelineLayoutDescriptor);
    }

    // Three separate vertex buffers (position/normal/texcoord) at slots 0/1/2, matching the WGSL
    // locations.
    private RenderPipeline BuildPipeline(Device device, bool blend)
    {
        var attributes = stackalloc VertexAttribute[3];
        attributes[0] = new VertexAttribute { Format = VertexFormat.Float32x3, ShaderLocation = 0 };
        attributes[1] = new VertexAttribute { Format = VertexFormat.Float32x3, ShaderLocation = 1 };
        attributes[2] = new VertexAttribute { Format = VertexFormat.Float32x2, ShaderLocation = 2 };
        var buffers = stackalloc VertexBufferLayout[3];
        buffers[0] = new VertexBufferLayout { StepMode = VertexStepMode.Vertex, ArrayStride = 12, AttributeCount = 1, Attributes = &attributes[0] };
        buffers[1] = new VertexBufferLayout { StepMode = VertexStepMode.Vertex, ArrayStride = 12, AttributeCount = 1, Attributes = &attributes[1] };
        buffers[2] = new VertexBufferLayout { StepMode = VertexStepMode.Vertex, ArrayStride = 8, AttributeCount = 1, Attributes = &attributes[2] };

        // Straight-alpha "over": colour weighted by source alpha, alpha channel accumulates coverage
        // (One, OneMinusSrcAlpha) so a region already opaque behind the glass stays opaque for the
        // Skia composite - the GL BlendFuncSeparate, mirrored.
        var blendState = new BlendState
        {
            Color = new BlendComponent { Operation = BlendOperation.Add, SrcFactor = BlendFactor.SrcAlpha, DstFactor = BlendFactor.OneMinusSrcAlpha },
            Alpha = new BlendComponent { Operation = BlendOperation.Add, SrcFactor = BlendFactor.One, DstFactor = BlendFactor.OneMinusSrcAlpha },
        };
        var colorTarget = new ColorTargetState
        {
            Format = ColorFormat,
            Blend = blend ? &blendState : null,
            WriteMask = ColorWriteMask.All,
        };

        var stencilFace = new StencilFaceState
        {
            Compare = CompareFunction.Always,
            FailOp = StencilOperation.Keep,
            DepthFailOp = StencilOperation.Keep,
            PassOp = StencilOperation.Keep,
        };
        var depthStencil = new DepthStencilState
        {
            Format = DepthFormat,
            DepthWriteEnabled = blend ? OptionalBool.False : OptionalBool.True,
            DepthCompare = CompareFunction.Less,
            StencilFront = stencilFace,
            StencilBack = stencilFace,
            StencilReadMask = 0xFFFFFFFF,
            StencilWriteMask = 0xFFFFFFFF,
        };

        fixed (byte* vsName = "vs_main"u8)
        fixed (byte* fsName = "fs_main"u8)
        {
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
                Vertex = new VertexState
                {
                    Module = _module,
                    EntryPoint = WgpuString.FromPinned(vsName, 7),
                    BufferCount = 3,
                    Buffers = buffers,
                },
                Primitive = new PrimitiveState { Topology = PrimitiveTopology.TriangleList, FrontFace = FrontFace.CCW, CullMode = CullMode.None },
                DepthStencil = &depthStencil,
                Multisample = new MultisampleState { Count = 1, Mask = 0xFFFFFFFF },
                Fragment = &fragment,
            };
            return WebGpu.DeviceCreateRenderPipeline(device, &descriptor);
        }
    }

    #endregion

    #region | Render targets, model upload, texture upload |

    // Creates (or re-creates on a size change) the headless colour target - which also does the
    // readback - and the matching depth texture.
    private WebGpuSkiaTarget EnsureRenderTargets(int width, int height)
    {
        if (_target is not null && _target.Width == width && _target.Height == height)
        {
            return _target;
        }

        DestroyRenderTargets();

        _target = WebGpuSkiaTarget.CreateHeadless(_gpu!, width, height, new WebGpuSkiaTargetOptions { Format = ColorFormat });
        if (!_target.IsAvailable)
        {
            throw new InvalidOperationException("WebGPU render target unavailable: " + _target.TierReason);
        }
        _depthTexture = WebGpuSupport.CreateTexture(_gpu!.Device, (uint)width, (uint)height, DepthFormat, TextureUsage.RenderAttachment);
        _depthView = WebGpu.TextureCreateView(_depthTexture, null);
        return _target;
    }

    private void ApplyPendingModel()
    {
        LoadedModel? model;
        bool frameCamera;
        lock (_pendingLock)
        {
            if (!_hasPendingModel)
            {
                return;
            }
            model = _pendingModel;
            frameCamera = _pendingFrameCamera;
            _pendingModel = null;
            _hasPendingModel = false;
        }

        ReleaseModelResources();
        _currentModel = model;
        if (model is null)
        {
            return;
        }

        var device = _gpu!.Device;
        foreach (var primitive in model.Primitives)
        {
            if (primitive.Indices.Length == 0)
            {
                continue;
            }

            var uniformBuffer = CreateBuffer(device, BufferUsage.Uniform | BufferUsage.CopyDst, (ulong)sizeof(Uniforms));
            var uniformEntry = new BindGroupEntry { Binding = 0, Buffer = uniformBuffer, Size = (ulong)sizeof(Uniforms) };
            var groupDescriptor = new BindGroupDescriptor { Layout = _uniformLayout, EntryCount = 1, Entries = &uniformEntry };

            _gpuPrimitives.Add(new GpuPrimitive(
                UploadBuffer(device, BufferUsage.Vertex, primitive.Positions),
                UploadBuffer(device, BufferUsage.Vertex, primitive.Normals),
                UploadBuffer(device, BufferUsage.Vertex, primitive.TexCoords),
                UploadBuffer(device, BufferUsage.Index, primitive.Indices),
                (uint)primitive.Indices.Length,
                primitive.MaterialIndex,
                uniformBuffer,
                WebGpu.DeviceCreateBindGroup(device, &groupDescriptor)));
        }

        for (var materialIndex = 0; materialIndex < model.Materials.Count; materialIndex++)
        {
            var material = model.Materials[materialIndex];
            if (material.BaseColorTextureRgba is { Length: > 0 } rgba)
            {
                fixed (byte* data = rgba)
                {
                    _materialTextures[materialIndex] = UploadTexture(
                        data, (nuint)rgba.Length, (uint)material.BaseColorTextureWidth, (uint)material.BaseColorTextureHeight);
                }
            }
        }

        WebGpuSupport.ThrowIfErrors(_gpu, "model upload");

        if (frameCamera)
        {
            Camera.FitToModel(model);
        }
    }

    private static GpuBuffer CreateBuffer(Device device, BufferUsage usage, ulong size)
    {
        var descriptor = new BufferDescriptor { Usage = usage, Size = size };
        return WebGpu.DeviceCreateBuffer(device, &descriptor);
    }

    // A GPU buffer filled with the array's contents through a queue write (it lands at the next
    // submit, before the first draw that reads it). Float and uint elements keep every size a
    // multiple of 4 bytes, as WebGPU requires.
    private GpuBuffer UploadBuffer<T>(Device device, BufferUsage usage, T[] data) where T : unmanaged
    {
        var size = (ulong)(Math.Max(data.Length, 1) * sizeof(T));
        var buffer = CreateBuffer(device, usage | BufferUsage.CopyDst, size);
        if (data.Length > 0)
        {
            fixed (T* p = data)
            {
                WebGpu.QueueWriteBuffer(_gpu!.Queue, buffer, 0, p, (nuint)(data.Length * sizeof(T)));
            }
        }
        return buffer;
    }

    // Uploads an RGBA image to a sampleable 2D texture and builds its texture + sampler bind group.
    private GpuTexture UploadTexture(byte* rgba, nuint size, uint width, uint height)
    {
        var device = _gpu!.Device;
        var texture = WebGpuSupport.CreateTexture(device, width, height, ColorFormat, TextureUsage.TextureBinding | TextureUsage.CopyDst);
        WebGpuSupport.WriteTexture(_gpu.Queue, texture, rgba, size, width, height, 4);
        var view = WebGpu.TextureCreateView(texture, null);

        var entries = stackalloc BindGroupEntry[2];
        entries[0] = new BindGroupEntry { Binding = 0, TextureView = view };
        entries[1] = new BindGroupEntry { Binding = 1, Sampler = _sampler };
        var descriptor = new BindGroupDescriptor { Layout = _textureLayout, EntryCount = 2, Entries = entries };
        return new GpuTexture(texture, view, WebGpu.DeviceCreateBindGroup(device, &descriptor));
    }

    #endregion

    #region | Teardown |

    private void ReleaseModelResources()
    {
        foreach (var primitive in _gpuPrimitives)
        {
            WebGpu.BindGroupRelease(primitive.UniformGroup);
            WebGpu.BufferRelease(primitive.UniformBuffer);
            WebGpu.BufferRelease(primitive.Positions);
            WebGpu.BufferRelease(primitive.Normals);
            WebGpu.BufferRelease(primitive.TexCoords);
            WebGpu.BufferRelease(primitive.Indices);
        }
        _gpuPrimitives.Clear();

        foreach (var texture in _materialTextures.Values)
        {
            ReleaseTexture(texture);
        }
        _materialTextures.Clear();

        _currentModel = null;
    }

    private static void ReleaseTexture(GpuTexture texture)
    {
        if (!texture.Group.IsNull) { WebGpu.BindGroupRelease(texture.Group); }
        if (!texture.View.IsNull) { WebGpu.TextureViewRelease(texture.View); }
        if (!texture.Texture.IsNull) { WebGpu.TextureRelease(texture.Texture); }
    }

    private void DestroyRenderTargets()
    {
        _target?.Dispose();
        _target = null;
        if (!_depthView.IsNull) { WebGpu.TextureViewRelease(_depthView); _depthView = default; }
        if (!_depthTexture.IsNull) { WebGpu.TextureRelease(_depthTexture); _depthTexture = default; }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;

        if (_gpu is null)
        {
            return;
        }

        // Targets and every handle we created before the device that owns them.
        ReleaseModelResources();
        DestroyRenderTargets();
        ReleaseTexture(_whiteTexture);

        if (!_sampler.IsNull) { WebGpu.SamplerRelease(_sampler); }
        if (!_blendPipeline.IsNull) { WebGpu.RenderPipelineRelease(_blendPipeline); }
        if (!_opaquePipeline.IsNull) { WebGpu.RenderPipelineRelease(_opaquePipeline); }
        if (!_pipelineLayout.IsNull) { WebGpu.PipelineLayoutRelease(_pipelineLayout); }
        if (!_textureLayout.IsNull) { WebGpu.BindGroupLayoutRelease(_textureLayout); }
        if (!_uniformLayout.IsNull) { WebGpu.BindGroupLayoutRelease(_uniformLayout); }
        if (!_module.IsNull) { WebGpu.ShaderModuleRelease(_module); }

        _gpu.Dispose();
        _gpu = null;
    }

    #endregion
}
