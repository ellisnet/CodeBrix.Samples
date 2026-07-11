using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;

namespace PolyHavenBrowser.Rendering;

/// <summary>
/// A self-contained <b>offscreen Vulkan renderer</b> (via Silk.NET.Vulkan) for previewing one
/// <see cref="LoadedModel"/> with an orbit camera - the Vulkan counterpart of
/// <see cref="GlModelSceneRenderer"/>, drawing the same scene with the same shading (the
/// SPIR-V in <see cref="VulkanShaders"/> mirrors the GL shaders).
/// <para>
/// Unlike the GL renderer, which draws into whatever context/framebuffer the host has bound,
/// Vulkan has no ambient context - so this class owns the whole stack: instance, device,
/// offscreen color + depth images, pipeline, per-model buffers/textures, and the CPU readback.
/// Everything is created lazily on the first <see cref="RenderFrame"/> call (on the render
/// thread) and it never touches any window system, so it cannot disturb the host head's own
/// renderer. <see cref="SetModel"/> may be called from any thread; the GPU upload happens on
/// the next render.
/// </para>
/// <para>
/// Coordinate conventions: the camera's System.Numerics matrices are used <b>unmodified</b>.
/// The projection maps depth to [0, 1], which is Vulkan's native range (GL merely wastes half
/// its [-1, 1] range on it). Vulkan's clip-space Y points down, so with these matrices the
/// scene lands vertically inverted in the framebuffer; reading the top-down rows back
/// therefore yields a <b>bottom-up image</b>, exactly like GL's readback - the caller flags
/// the frame IsBottomUp and the existing Skia flip handles both engines identically.
/// </para>
/// </summary>
public sealed unsafe class VulkanSceneRenderer : IDisposable
{
    // One 112-byte push-constant block shared by both shader stages; the layout matches the
    // PushConstants block in VulkanShaders. The Matrix4x4 is written in .NET's row-major
    // memory order and SPIR-V reads mat4 column-major - the same implicit transpose the GL
    // renderer relies on (see the depth-ordering regression test before "fixing" this).
    [StructLayout(LayoutKind.Sequential)]
    private struct PushConstants
    {
        public Matrix4x4 Mvp;
        public Vector4 BaseColorFactor;
        public Vector4 LightDirection;
        public int HasTexture;
        public int DoubleSided;
        public int Pad0;
        public int Pad1;
    }

    private sealed record GpuBuffer(Buffer Buffer, DeviceMemory Memory);

    private sealed record GpuPrimitive(
        GpuBuffer Positions, GpuBuffer Normals, GpuBuffer TexCoords, GpuBuffer Indices,
        uint IndexCount, int MaterialIndex);

    private sealed record GpuTexture(
        Image Image, DeviceMemory Memory, ImageView View, DescriptorSet DescriptorSet);

    private readonly object _pendingLock = new();
    private LoadedModel? _pendingModel;
    private bool _pendingFrameCamera;
    private bool _hasPendingModel;

    private LoadedModel? _currentModel;
    private readonly List<GpuPrimitive> _gpuPrimitives = [];
    private readonly Dictionary<int, GpuTexture> _materialTextures = [];

    private Vk? _vk;
    private Instance _instance;
    private PhysicalDevice _physicalDevice;
    private PhysicalDeviceMemoryProperties _memoryProperties;
    private Device _device;
    private Queue _queue;
    private uint _queueFamilyIndex;
    private CommandPool _commandPool;
    private CommandBuffer _commandBuffer;
    private Fence _fence;

    private DescriptorSetLayout _descriptorSetLayout;
    private PipelineLayout _pipelineLayout;
    private RenderPass _renderPass;
    private Pipeline _pipeline;
    private Sampler _sampler;

    // The fallback bound for untextured materials (the shader's hasTexture flag skips the
    // sample, but Vulkan still requires a valid combined image sampler in the set).
    private GpuTexture? _whiteTexture;
    private DescriptorPool _staticDescriptorPool;

    // Per-model descriptor pool, recreated on each model upload sized to its material count.
    private DescriptorPool _modelDescriptorPool;

    // The offscreen render targets plus CPU-readback buffer, recreated on a size change.
    private Image _colorImage;
    private DeviceMemory _colorMemory;
    private ImageView _colorView;
    private Image _depthImage;
    private DeviceMemory _depthMemory;
    private ImageView _depthView;
    private Framebuffer _framebuffer;
    private Buffer _readbackBuffer;
    private DeviceMemory _readbackMemory;
    private void* _readbackMapped;
    private uint _targetWidth;
    private uint _targetHeight;

    private bool _initialized;
    private bool _disposed;

    private const Format ColorFormat = Format.R8G8B8A8Unorm;
    private const Format DepthFormat = Format.D16Unorm;

    /// <summary>The orbit camera driven by the host's pointer/scroll input.</summary>
    public OrbitCamera Camera { get; } = new();

    /// <summary>
    /// A fixed world-space light direction (pointing toward the light), or <see langword="null"/>
    /// (the default) for a camera headlight with double-sided lighting - the same semantics as
    /// <see cref="GlModelSceneRenderer.FixedLightDirection"/>.
    /// </summary>
    public Vector3? FixedLightDirection { get; set; }

    /// <summary>
    /// Whether a usable Vulkan implementation (loader + device) is present at runtime. Intended
    /// for tests; the app itself gates Vulkan on <see cref="VulkanPlatformSupport"/> instead.
    /// </summary>
    public static bool IsRuntimeAvailable()
    {
        try
        {
            using var probe = new VulkanSceneRenderer();
            probe.EnsureInitialized();
            return true;
        }
        catch
        {
            return false;
        }
    }

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
    /// returns the frame as tightly packed RGBA bytes. As explained in the class remarks the
    /// image is bottom-up (matching GL readback); call on the render thread.
    /// </summary>
    public byte[] RenderFrame(int width, int height, (float R, float G, float B, float A) background)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);

        EnsureInitialized();
        ApplyPendingModel();
        EnsureRenderTargets((uint)width, (uint)height);

        var vk = _vk!;
        vk.ResetCommandBuffer(_commandBuffer, CommandBufferResetFlags.None);
        var beginInfo = new CommandBufferBeginInfo
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
        };
        Check(vk.BeginCommandBuffer(_commandBuffer, in beginInfo), "BeginCommandBuffer");

        RecordRenderPass(width, height, background);
        RecordReadback((uint)width, (uint)height);

        Check(vk.EndCommandBuffer(_commandBuffer), "EndCommandBuffer");
        SubmitAndWait();

        // The readback buffer is persistently mapped host-coherent memory; after the fence
        // the copied pixels are visible to the CPU.
        var pixels = new byte[width * height * 4];
        Marshal.Copy((nint)_readbackMapped, pixels, 0, pixels.Length);
        return pixels;
    }

    #region | One-time initialization: instance, device, pipeline |

    private void EnsureInitialized()
    {
        if (_initialized) { return; }

        _vk = Vk.GetApi();
        CreateInstanceAndDevice();
        CreateCommandInfrastructure();
        CreateSamplerAndLayouts();
        CreateRenderPass();
        CreatePipeline();
        CreateWhiteTexture();
        _initialized = true;
    }

    private void CreateInstanceAndDevice()
    {
        var vk = _vk!;

        // A minimal Vulkan 1.0 instance: offscreen rendering needs no extensions (no surface,
        // no swapchain) and no layers.
        var appInfo = new ApplicationInfo
        {
            SType = StructureType.ApplicationInfo,
            ApiVersion = Vk.Version10,
        };
        var instanceInfo = new InstanceCreateInfo
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,
        };
        Check(vk.CreateInstance(in instanceInfo, null, out _instance), "CreateInstance");

        // Pick the most capable physical device that has a graphics queue: discrete first,
        // then integrated, then anything else (including CPU implementations like lavapipe,
        // which keeps tests runnable on GPU-less machines).
        uint deviceCount = 0;
        Check(vk.EnumeratePhysicalDevices(_instance, ref deviceCount, null), "EnumeratePhysicalDevices");
        if (deviceCount == 0)
        {
            throw new InvalidOperationException("No Vulkan physical devices are available.");
        }
        var devices = new PhysicalDevice[deviceCount];
        fixed (PhysicalDevice* devicesPtr = devices)
        {
            Check(vk.EnumeratePhysicalDevices(_instance, ref deviceCount, devicesPtr), "EnumeratePhysicalDevices");
        }

        var bestScore = -1;
        foreach (var candidate in devices)
        {
            if (!TryGetGraphicsQueueFamily(candidate, out var family)) { continue; }

            vk.GetPhysicalDeviceProperties(candidate, out var properties);
            var score = properties.DeviceType switch
            {
                PhysicalDeviceType.DiscreteGpu => 3,
                PhysicalDeviceType.IntegratedGpu => 2,
                PhysicalDeviceType.VirtualGpu => 1,
                _ => 0,
            };
            if (score > bestScore)
            {
                bestScore = score;
                _physicalDevice = candidate;
                _queueFamilyIndex = family;
            }
        }
        if (bestScore < 0)
        {
            throw new InvalidOperationException("No Vulkan device with a graphics queue is available.");
        }

        vk.GetPhysicalDeviceMemoryProperties(_physicalDevice, out _memoryProperties);

        // One graphics queue, no device extensions, default features.
        var priority = 1f;
        var queueInfo = new DeviceQueueCreateInfo
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = _queueFamilyIndex,
            QueueCount = 1,
            PQueuePriorities = &priority,
        };
        var deviceInfo = new DeviceCreateInfo
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = 1,
            PQueueCreateInfos = &queueInfo,
        };
        Check(vk.CreateDevice(_physicalDevice, in deviceInfo, null, out _device), "CreateDevice");
        vk.GetDeviceQueue(_device, _queueFamilyIndex, 0, out _queue);
    }

    private bool TryGetGraphicsQueueFamily(PhysicalDevice device, out uint familyIndex)
    {
        var vk = _vk!;
        uint count = 0;
        vk.GetPhysicalDeviceQueueFamilyProperties(device, ref count, null);
        var families = new QueueFamilyProperties[count];
        fixed (QueueFamilyProperties* familiesPtr = families)
        {
            vk.GetPhysicalDeviceQueueFamilyProperties(device, ref count, familiesPtr);
        }

        for (uint i = 0; i < count; i++)
        {
            if ((families[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
            {
                familyIndex = i;
                return true;
            }
        }
        familyIndex = 0;
        return false;
    }

    private void CreateCommandInfrastructure()
    {
        var vk = _vk!;
        var poolInfo = new CommandPoolCreateInfo
        {
            SType = StructureType.CommandPoolCreateInfo,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
            QueueFamilyIndex = _queueFamilyIndex,
        };
        Check(vk.CreateCommandPool(_device, in poolInfo, null, out _commandPool), "CreateCommandPool");

        var allocInfo = new CommandBufferAllocateInfo
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1,
        };
        fixed (CommandBuffer* commandBufferPtr = &_commandBuffer)
        {
            Check(vk.AllocateCommandBuffers(_device, in allocInfo, commandBufferPtr), "AllocateCommandBuffers");
        }

        var fenceInfo = new FenceCreateInfo { SType = StructureType.FenceCreateInfo };
        Check(vk.CreateFence(_device, in fenceInfo, null, out _fence), "CreateFence");
    }

    private void CreateSamplerAndLayouts()
    {
        var vk = _vk!;

        // Linear min/mag with trilinear mips and repeat wrapping - the GL renderer's
        // LinearMipmapLinear / Repeat texture parameters.
        var samplerInfo = new SamplerCreateInfo
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = Filter.Linear,
            MinFilter = Filter.Linear,
            MipmapMode = SamplerMipmapMode.Linear,
            AddressModeU = SamplerAddressMode.Repeat,
            AddressModeV = SamplerAddressMode.Repeat,
            AddressModeW = SamplerAddressMode.Repeat,
            MaxLod = Vk.LodClampNone,
        };
        Check(vk.CreateSampler(_device, in samplerInfo, null, out _sampler), "CreateSampler");

        // Set 0, binding 0: the material's base-color texture (combined image sampler).
        var binding = new DescriptorSetLayoutBinding
        {
            Binding = 0,
            DescriptorType = DescriptorType.CombinedImageSampler,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.FragmentBit,
        };
        var layoutInfo = new DescriptorSetLayoutCreateInfo
        {
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = 1,
            PBindings = &binding,
        };
        Check(
            vk.CreateDescriptorSetLayout(_device, in layoutInfo, null, out _descriptorSetLayout),
            "CreateDescriptorSetLayout");

        // All per-draw values travel in one push-constant block visible to both stages.
        var pushRange = new PushConstantRange
        {
            StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
            Offset = 0,
            Size = (uint)sizeof(PushConstants),
        };
        var descriptorSetLayout = _descriptorSetLayout;
        var pipelineLayoutInfo = new PipelineLayoutCreateInfo
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 1,
            PSetLayouts = &descriptorSetLayout,
            PushConstantRangeCount = 1,
            PPushConstantRanges = &pushRange,
        };
        Check(
            vk.CreatePipelineLayout(_device, in pipelineLayoutInfo, null, out _pipelineLayout),
            "CreatePipelineLayout");
    }

    private void CreateRenderPass()
    {
        var vk = _vk!;

        // Color: clear, keep, and end in TRANSFER_SRC layout ready for the CPU readback copy.
        // Depth: clear, discard after the pass (only needed while drawing).
        var attachments = stackalloc AttachmentDescription[2];
        attachments[0] = new AttachmentDescription
        {
            Format = ColorFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.TransferSrcOptimal,
        };
        attachments[1] = new AttachmentDescription
        {
            Format = DepthFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.DontCare,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
        };

        var colorRef = new AttachmentReference { Attachment = 0, Layout = ImageLayout.ColorAttachmentOptimal };
        var depthRef = new AttachmentReference { Attachment = 1, Layout = ImageLayout.DepthStencilAttachmentOptimal };
        var subpass = new SubpassDescription
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorRef,
            PDepthStencilAttachment = &depthRef,
        };

        // Make attachment writes visible to the readback copy that follows the pass.
        var dependency = new SubpassDependency
        {
            SrcSubpass = 0,
            DstSubpass = Vk.SubpassExternal,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            SrcAccessMask = AccessFlags.ColorAttachmentWriteBit,
            DstStageMask = PipelineStageFlags.TransferBit,
            DstAccessMask = AccessFlags.TransferReadBit,
        };

        var renderPassInfo = new RenderPassCreateInfo
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 2,
            PAttachments = attachments,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &dependency,
        };
        Check(vk.CreateRenderPass(_device, in renderPassInfo, null, out _renderPass), "CreateRenderPass");
    }

    private void CreatePipeline()
    {
        var vk = _vk!;
        var vertexModule = CreateShaderModule(VulkanShaders.VertexWords);
        var fragmentModule = CreateShaderModule(VulkanShaders.FragmentWords);
        var entryPoint = Marshal.StringToHGlobalAnsi("main");
        try
        {
            var stages = stackalloc PipelineShaderStageCreateInfo[2];
            stages[0] = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.VertexBit,
                Module = vertexModule,
                PName = (byte*)entryPoint,
            };
            stages[1] = new PipelineShaderStageCreateInfo
            {
                SType = StructureType.PipelineShaderStageCreateInfo,
                Stage = ShaderStageFlags.FragmentBit,
                Module = fragmentModule,
                PName = (byte*)entryPoint,
            };

            // Three tightly packed vertex buffers at the shader's locations 0/1/2 - the same
            // separate position/normal/texcoord streams the GL renderer uploads.
            var bindings = stackalloc VertexInputBindingDescription[3];
            bindings[0] = new VertexInputBindingDescription
            {
                Binding = 0, Stride = 3 * sizeof(float), InputRate = VertexInputRate.Vertex,
            };
            bindings[1] = new VertexInputBindingDescription
            {
                Binding = 1, Stride = 3 * sizeof(float), InputRate = VertexInputRate.Vertex,
            };
            bindings[2] = new VertexInputBindingDescription
            {
                Binding = 2, Stride = 2 * sizeof(float), InputRate = VertexInputRate.Vertex,
            };
            var attributes = stackalloc VertexInputAttributeDescription[3];
            attributes[0] = new VertexInputAttributeDescription
            {
                Location = 0, Binding = 0, Format = Format.R32G32B32Sfloat, Offset = 0,
            };
            attributes[1] = new VertexInputAttributeDescription
            {
                Location = 1, Binding = 1, Format = Format.R32G32B32Sfloat, Offset = 0,
            };
            attributes[2] = new VertexInputAttributeDescription
            {
                Location = 2, Binding = 2, Format = Format.R32G32Sfloat, Offset = 0,
            };
            var vertexInput = new PipelineVertexInputStateCreateInfo
            {
                SType = StructureType.PipelineVertexInputStateCreateInfo,
                VertexBindingDescriptionCount = 3,
                PVertexBindingDescriptions = bindings,
                VertexAttributeDescriptionCount = 3,
                PVertexAttributeDescriptions = attributes,
            };

            var inputAssembly = new PipelineInputAssemblyStateCreateInfo
            {
                SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                Topology = PrimitiveTopology.TriangleList,
            };

            // Viewport and scissor are dynamic so a canvas resize doesn't rebuild the pipeline.
            var viewportState = new PipelineViewportStateCreateInfo
            {
                SType = StructureType.PipelineViewportStateCreateInfo,
                ViewportCount = 1,
                ScissorCount = 1,
            };
            var dynamicStates = stackalloc DynamicState[2] { DynamicState.Viewport, DynamicState.Scissor };
            var dynamicState = new PipelineDynamicStateCreateInfo
            {
                SType = StructureType.PipelineDynamicStateCreateInfo,
                DynamicStateCount = 2,
                PDynamicStates = dynamicStates,
            };

            // Culling stays off - Poly Haven models frequently rely on double-sided rendering
            // (and it also makes the vertical-flip winding question moot).
            var rasterization = new PipelineRasterizationStateCreateInfo
            {
                SType = StructureType.PipelineRasterizationStateCreateInfo,
                PolygonMode = PolygonMode.Fill,
                CullMode = CullModeFlags.None,
                FrontFace = FrontFace.CounterClockwise,
                LineWidth = 1f,
            };
            var multisample = new PipelineMultisampleStateCreateInfo
            {
                SType = StructureType.PipelineMultisampleStateCreateInfo,
                RasterizationSamples = SampleCountFlags.Count1Bit,
            };
            var depthStencil = new PipelineDepthStencilStateCreateInfo
            {
                SType = StructureType.PipelineDepthStencilStateCreateInfo,
                DepthTestEnable = true,
                DepthWriteEnable = true,
                DepthCompareOp = CompareOp.LessOrEqual,
            };

            // No blending, like the GL path: fragments overwrite, alpha lands in the target
            // and compositing happens later on the Skia canvas.
            var blendAttachment = new PipelineColorBlendAttachmentState
            {
                ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit
                    | ColorComponentFlags.BBit | ColorComponentFlags.ABit,
            };
            var colorBlend = new PipelineColorBlendStateCreateInfo
            {
                SType = StructureType.PipelineColorBlendStateCreateInfo,
                AttachmentCount = 1,
                PAttachments = &blendAttachment,
            };

            var pipelineInfo = new GraphicsPipelineCreateInfo
            {
                SType = StructureType.GraphicsPipelineCreateInfo,
                StageCount = 2,
                PStages = stages,
                PVertexInputState = &vertexInput,
                PInputAssemblyState = &inputAssembly,
                PViewportState = &viewportState,
                PRasterizationState = &rasterization,
                PMultisampleState = &multisample,
                PDepthStencilState = &depthStencil,
                PColorBlendState = &colorBlend,
                PDynamicState = &dynamicState,
                Layout = _pipelineLayout,
                RenderPass = _renderPass,
                Subpass = 0,
            };
            Check(
                vk.CreateGraphicsPipelines(_device, default, 1, in pipelineInfo, null, out _pipeline),
                "CreateGraphicsPipelines");
        }
        finally
        {
            Marshal.FreeHGlobal(entryPoint);
            vk.DestroyShaderModule(_device, vertexModule, null);
            vk.DestroyShaderModule(_device, fragmentModule, null);
        }
    }

    private ShaderModule CreateShaderModule(uint[] words)
    {
        var vk = _vk!;
        fixed (uint* wordsPtr = words)
        {
            var moduleInfo = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)(words.Length * sizeof(uint)),
                PCode = wordsPtr,
            };
            Check(vk.CreateShaderModule(_device, in moduleInfo, null, out var module), "CreateShaderModule");
            return module;
        }
    }

    private void CreateWhiteTexture()
    {
        var vk = _vk!;

        // A dedicated one-set pool for the white fallback texture, which lives for the
        // renderer's whole lifetime (per-model sets use their own recreated pool).
        var poolSize = new DescriptorPoolSize { Type = DescriptorType.CombinedImageSampler, DescriptorCount = 1 };
        var poolInfo = new DescriptorPoolCreateInfo
        {
            SType = StructureType.DescriptorPoolCreateInfo,
            MaxSets = 1,
            PoolSizeCount = 1,
            PPoolSizes = &poolSize,
        };
        Check(vk.CreateDescriptorPool(_device, in poolInfo, null, out _staticDescriptorPool), "CreateDescriptorPool");

        _whiteTexture = UploadTexture([255, 255, 255, 255], 1, 1, _staticDescriptorPool);
    }

    #endregion

    #region | Offscreen render targets + readback buffer |

    private void EnsureRenderTargets(uint width, uint height)
    {
        if (_framebuffer.Handle != 0 && _targetWidth == width && _targetHeight == height)
        {
            return;
        }

        var vk = _vk!;
        vk.DeviceWaitIdle(_device);
        DestroyRenderTargets();

        (_colorImage, _colorMemory) = CreateImage(
            width, height, 1, ColorFormat,
            ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.TransferSrcBit);
        _colorView = CreateImageView(_colorImage, ColorFormat, ImageAspectFlags.ColorBit, 1);

        (_depthImage, _depthMemory) = CreateImage(
            width, height, 1, DepthFormat, ImageUsageFlags.DepthStencilAttachmentBit);
        _depthView = CreateImageView(_depthImage, DepthFormat, ImageAspectFlags.DepthBit, 1);

        var attachments = stackalloc ImageView[2] { _colorView, _depthView };
        var framebufferInfo = new FramebufferCreateInfo
        {
            SType = StructureType.FramebufferCreateInfo,
            RenderPass = _renderPass,
            AttachmentCount = 2,
            PAttachments = attachments,
            Width = width,
            Height = height,
            Layers = 1,
        };
        Check(vk.CreateFramebuffer(_device, in framebufferInfo, null, out _framebuffer), "CreateFramebuffer");

        // Host-visible, host-coherent readback buffer, mapped once and left mapped.
        var byteCount = (ulong)width * height * 4;
        (_readbackBuffer, _readbackMemory) = CreateBuffer(
            byteCount, BufferUsageFlags.TransferDstBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        fixed (void** mappedPtr = &_readbackMapped)
        {
            Check(vk.MapMemory(_device, _readbackMemory, 0, byteCount, 0, mappedPtr), "MapMemory");
        }

        _targetWidth = width;
        _targetHeight = height;
    }

    private void DestroyRenderTargets()
    {
        var vk = _vk!;
        if (_readbackMapped != null) { vk.UnmapMemory(_device, _readbackMemory); _readbackMapped = null; }
        if (_readbackBuffer.Handle != 0) { vk.DestroyBuffer(_device, _readbackBuffer, null); _readbackBuffer = default; }
        if (_readbackMemory.Handle != 0) { vk.FreeMemory(_device, _readbackMemory, null); _readbackMemory = default; }
        if (_framebuffer.Handle != 0) { vk.DestroyFramebuffer(_device, _framebuffer, null); _framebuffer = default; }
        if (_colorView.Handle != 0) { vk.DestroyImageView(_device, _colorView, null); _colorView = default; }
        if (_colorImage.Handle != 0) { vk.DestroyImage(_device, _colorImage, null); _colorImage = default; }
        if (_colorMemory.Handle != 0) { vk.FreeMemory(_device, _colorMemory, null); _colorMemory = default; }
        if (_depthView.Handle != 0) { vk.DestroyImageView(_device, _depthView, null); _depthView = default; }
        if (_depthImage.Handle != 0) { vk.DestroyImage(_device, _depthImage, null); _depthImage = default; }
        if (_depthMemory.Handle != 0) { vk.FreeMemory(_device, _depthMemory, null); _depthMemory = default; }
        _targetWidth = 0;
        _targetHeight = 0;
    }

    #endregion

    #region | Per-frame recording |

    private void RecordRenderPass(int width, int height, (float R, float G, float B, float A) background)
    {
        var vk = _vk!;
        var clearValues = stackalloc ClearValue[2];
        clearValues[0] = new ClearValue(new ClearColorValue(background.R, background.G, background.B, background.A));
        clearValues[1] = new ClearValue(depthStencil: new ClearDepthStencilValue(1f, 0));

        var renderPassBegin = new RenderPassBeginInfo
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = _renderPass,
            Framebuffer = _framebuffer,
            RenderArea = new Rect2D(new Offset2D(0, 0), new Extent2D((uint)width, (uint)height)),
            ClearValueCount = 2,
            PClearValues = clearValues,
        };
        vk.CmdBeginRenderPass(_commandBuffer, in renderPassBegin, SubpassContents.Inline);

        if (_currentModel is not null && _gpuPrimitives.Count > 0)
        {
            vk.CmdBindPipeline(_commandBuffer, PipelineBindPoint.Graphics, _pipeline);

            var viewport = new Viewport(0, 0, width, height, 0f, 1f);
            var scissor = new Rect2D(new Offset2D(0, 0), new Extent2D((uint)width, (uint)height));
            vk.CmdSetViewport(_commandBuffer, 0, 1, in viewport);
            vk.CmdSetScissor(_commandBuffer, 0, 1, in scissor);

            // The same camera/light math as the GL renderer, pushed per draw. See the class
            // remarks (and the GL renderer) for why the MVP is NOT transposed here.
            var mvp = Camera.GetViewMatrix() * Camera.GetProjectionMatrix(width / (float)height);
            var lightDirection = FixedLightDirection is { } fixedLight && fixedLight != Vector3.Zero
                ? Vector3.Normalize(fixedLight)
                : Vector3.Normalize(Camera.GetEyePosition() - Camera.Target);
            var doubleSided = FixedLightDirection is null ? 1 : 0;

            var vertexBuffers = stackalloc Buffer[3];
            var offsets = stackalloc ulong[3] { 0, 0, 0 };
            foreach (var primitive in _gpuPrimitives)
            {
                var material = primitive.MaterialIndex >= 0 && primitive.MaterialIndex < _currentModel.Materials.Count
                    ? _currentModel.Materials[primitive.MaterialIndex]
                    : null;
                var baseColor = material?.BaseColorFactor ?? Vector4.One;
                var hasTexture = _materialTextures.TryGetValue(primitive.MaterialIndex, out var texture);

                var pushConstants = new PushConstants
                {
                    Mvp = mvp,
                    BaseColorFactor = baseColor,
                    LightDirection = new Vector4(lightDirection, 0f),
                    HasTexture = hasTexture ? 1 : 0,
                    DoubleSided = doubleSided,
                };
                vk.CmdPushConstants(
                    _commandBuffer, _pipelineLayout,
                    ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit,
                    0, (uint)sizeof(PushConstants), &pushConstants);

                var descriptorSet = hasTexture ? texture!.DescriptorSet : _whiteTexture!.DescriptorSet;
                vk.CmdBindDescriptorSets(
                    _commandBuffer, PipelineBindPoint.Graphics, _pipelineLayout,
                    0, 1, in descriptorSet, 0, null);

                vertexBuffers[0] = primitive.Positions.Buffer;
                vertexBuffers[1] = primitive.Normals.Buffer;
                vertexBuffers[2] = primitive.TexCoords.Buffer;
                vk.CmdBindVertexBuffers(_commandBuffer, 0, 3, vertexBuffers, offsets);
                vk.CmdBindIndexBuffer(_commandBuffer, primitive.Indices.Buffer, 0, IndexType.Uint32);
                vk.CmdDrawIndexed(_commandBuffer, primitive.IndexCount, 1, 0, 0, 0);
            }
        }

        vk.CmdEndRenderPass(_commandBuffer);
    }

    private void RecordReadback(uint width, uint height)
    {
        var vk = _vk!;

        // The render pass left the color image in TRANSFER_SRC layout; copy it to the
        // host-visible buffer, then make the transfer visible to host reads.
        var region = new BufferImageCopy
        {
            BufferOffset = 0,
            BufferRowLength = 0,   // tightly packed
            BufferImageHeight = 0,
            ImageSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
            ImageOffset = new Offset3D(0, 0, 0),
            ImageExtent = new Extent3D(width, height, 1),
        };
        vk.CmdCopyImageToBuffer(
            _commandBuffer, _colorImage, ImageLayout.TransferSrcOptimal, _readbackBuffer, 1, in region);

        var barrier = new BufferMemoryBarrier
        {
            SType = StructureType.BufferMemoryBarrier,
            SrcAccessMask = AccessFlags.TransferWriteBit,
            DstAccessMask = AccessFlags.HostReadBit,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Buffer = _readbackBuffer,
            Offset = 0,
            Size = Vk.WholeSize,
        };
        vk.CmdPipelineBarrier(
            _commandBuffer, PipelineStageFlags.TransferBit, PipelineStageFlags.HostBit, 0,
            0, null, 1, &barrier, 0, null);
    }

    private void SubmitAndWait()
    {
        var vk = _vk!;
        var commandBuffer = _commandBuffer;
        var submitInfo = new SubmitInfo
        {
            SType = StructureType.SubmitInfo,
            CommandBufferCount = 1,
            PCommandBuffers = &commandBuffer,
        };
        Check(vk.QueueSubmit(_queue, 1, in submitInfo, _fence), "QueueSubmit");
        Check(vk.WaitForFences(_device, 1, in _fence, true, ulong.MaxValue), "WaitForFences");
        Check(vk.ResetFences(_device, 1, in _fence), "ResetFences");
    }

    #endregion

    #region | Model upload |

    private void ApplyPendingModel()
    {
        LoadedModel? model;
        bool frameCamera;
        lock (_pendingLock)
        {
            if (!_hasPendingModel) { return; }
            model = _pendingModel;
            frameCamera = _pendingFrameCamera;
            _pendingModel = null;
            _hasPendingModel = false;
        }

        var vk = _vk!;
        vk.DeviceWaitIdle(_device);
        ReleaseModelResources();
        _currentModel = model;
        if (model is null) { return; }

        foreach (var primitive in model.Primitives)
        {
            _gpuPrimitives.Add(new GpuPrimitive(
                UploadArray(primitive.Positions, BufferUsageFlags.VertexBufferBit),
                UploadArray(primitive.Normals, BufferUsageFlags.VertexBufferBit),
                UploadArray(primitive.TexCoords, BufferUsageFlags.VertexBufferBit),
                UploadArray(primitive.Indices, BufferUsageFlags.IndexBufferBit),
                (uint)primitive.Indices.Length,
                primitive.MaterialIndex));
        }

        // One descriptor set per textured material, from a pool recreated to fit this model.
        var texturedMaterials = 0;
        for (var i = 0; i < model.Materials.Count; i++)
        {
            if (model.Materials[i].BaseColorTextureRgba is { Length: > 0 }) { texturedMaterials++; }
        }
        if (texturedMaterials > 0)
        {
            var poolSize = new DescriptorPoolSize
            {
                Type = DescriptorType.CombinedImageSampler,
                DescriptorCount = (uint)texturedMaterials,
            };
            var poolInfo = new DescriptorPoolCreateInfo
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                MaxSets = (uint)texturedMaterials,
                PoolSizeCount = 1,
                PPoolSizes = &poolSize,
            };
            Check(vk.CreateDescriptorPool(_device, in poolInfo, null, out _modelDescriptorPool), "CreateDescriptorPool");

            for (var materialIndex = 0; materialIndex < model.Materials.Count; materialIndex++)
            {
                var material = model.Materials[materialIndex];
                if (material.BaseColorTextureRgba is { Length: > 0 } rgba)
                {
                    _materialTextures[materialIndex] = UploadTexture(
                        rgba, (uint)material.BaseColorTextureWidth, (uint)material.BaseColorTextureHeight,
                        _modelDescriptorPool);
                }
            }
        }

        if (frameCamera)
        {
            Camera.FitToModel(model);
        }
    }

    private void ReleaseModelResources()
    {
        var vk = _vk!;
        foreach (var primitive in _gpuPrimitives)
        {
            DestroyBuffer(primitive.Positions);
            DestroyBuffer(primitive.Normals);
            DestroyBuffer(primitive.TexCoords);
            DestroyBuffer(primitive.Indices);
        }
        _gpuPrimitives.Clear();

        foreach (var texture in _materialTextures.Values)
        {
            DestroyTexture(texture);
        }
        _materialTextures.Clear();

        if (_modelDescriptorPool.Handle != 0)
        {
            vk.DestroyDescriptorPool(_device, _modelDescriptorPool, null);
            _modelDescriptorPool = default;
        }

        _currentModel = null;
    }

    // Uploads a float[] or uint[] to a host-visible vertex/index buffer. Host-visible memory
    // (no staging/device-local copy) is plenty for this preview's model sizes.
    private GpuBuffer UploadArray<T>(T[] data, BufferUsageFlags usage) where T : unmanaged
    {
        var vk = _vk!;
        var byteCount = (ulong)(data.Length * sizeof(T));
        var (buffer, memory) = CreateBuffer(
            byteCount, usage, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);

        void* mapped;
        Check(vk.MapMemory(_device, memory, 0, byteCount, 0, &mapped), "MapMemory");
        fixed (T* dataPtr = data)
        {
            System.Buffer.MemoryCopy(dataPtr, mapped, byteCount, byteCount);
        }
        vk.UnmapMemory(_device, memory);
        return new GpuBuffer(buffer, memory);
    }

    // Uploads an RGBA image to a device-local, mip-mapped texture: staging buffer →
    // vkCmdCopyBufferToImage → a vkCmdBlitImage chain that fills the mip levels (the Vulkan
    // equivalent of the GL renderer's glGenerateMipmap), then allocates + writes its
    // descriptor set from the given pool.
    private GpuTexture UploadTexture(byte[] rgba, uint width, uint height, DescriptorPool pool)
    {
        var vk = _vk!;
        var mipLevels = 1 + (uint)Math.Floor(Math.Log2(Math.Max(width, height)));

        var byteCount = (ulong)rgba.Length;
        var (staging, stagingMemory) = CreateBuffer(
            byteCount, BufferUsageFlags.TransferSrcBit,
            MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit);
        try
        {
            void* mapped;
            Check(vk.MapMemory(_device, stagingMemory, 0, byteCount, 0, &mapped), "MapMemory");
            Marshal.Copy(rgba, 0, (nint)mapped, rgba.Length);
            vk.UnmapMemory(_device, stagingMemory);

            var (image, memory) = CreateImage(
                width, height, mipLevels, ColorFormat,
                ImageUsageFlags.SampledBit | ImageUsageFlags.TransferDstBit | ImageUsageFlags.TransferSrcBit);

            // Record the whole upload (copy + mip chain + final layout) as one submission.
            var beginInfo = new CommandBufferBeginInfo
            {
                SType = StructureType.CommandBufferBeginInfo,
                Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
            };
            vk.ResetCommandBuffer(_commandBuffer, CommandBufferResetFlags.None);
            Check(vk.BeginCommandBuffer(_commandBuffer, in beginInfo), "BeginCommandBuffer");

            TransitionMipLevel(image, 0, mipLevels,
                ImageLayout.Undefined, ImageLayout.TransferDstOptimal,
                0, AccessFlags.TransferWriteBit,
                PipelineStageFlags.TopOfPipeBit, PipelineStageFlags.TransferBit);

            var copyRegion = new BufferImageCopy
            {
                ImageSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, 0, 0, 1),
                ImageExtent = new Extent3D(width, height, 1),
            };
            vk.CmdCopyBufferToImage(_commandBuffer, staging, image, ImageLayout.TransferDstOptimal, 1, in copyRegion);

            // Blit each level from the one above it, transitioning levels as they are consumed.
            var mipWidth = (int)width;
            var mipHeight = (int)height;
            for (uint level = 1; level < mipLevels; level++)
            {
                TransitionMipLevel(image, level - 1, 1,
                    ImageLayout.TransferDstOptimal, ImageLayout.TransferSrcOptimal,
                    AccessFlags.TransferWriteBit, AccessFlags.TransferReadBit,
                    PipelineStageFlags.TransferBit, PipelineStageFlags.TransferBit);

                var nextWidth = Math.Max(mipWidth / 2, 1);
                var nextHeight = Math.Max(mipHeight / 2, 1);
                var blit = new ImageBlit
                {
                    SrcSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, level - 1, 0, 1),
                    DstSubresource = new ImageSubresourceLayers(ImageAspectFlags.ColorBit, level, 0, 1),
                };
                blit.SrcOffsets[0] = new Offset3D(0, 0, 0);
                blit.SrcOffsets[1] = new Offset3D(mipWidth, mipHeight, 1);
                blit.DstOffsets[0] = new Offset3D(0, 0, 0);
                blit.DstOffsets[1] = new Offset3D(nextWidth, nextHeight, 1);
                vk.CmdBlitImage(
                    _commandBuffer,
                    image, ImageLayout.TransferSrcOptimal,
                    image, ImageLayout.TransferDstOptimal,
                    1, in blit, Filter.Linear);

                TransitionMipLevel(image, level - 1, 1,
                    ImageLayout.TransferSrcOptimal, ImageLayout.ShaderReadOnlyOptimal,
                    AccessFlags.TransferReadBit, AccessFlags.ShaderReadBit,
                    PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit);

                mipWidth = nextWidth;
                mipHeight = nextHeight;
            }

            // The last level was only ever a blit destination; bring it to shader-read too.
            TransitionMipLevel(image, mipLevels - 1, 1,
                ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal,
                AccessFlags.TransferWriteBit, AccessFlags.ShaderReadBit,
                PipelineStageFlags.TransferBit, PipelineStageFlags.FragmentShaderBit);

            Check(vk.EndCommandBuffer(_commandBuffer), "EndCommandBuffer");
            SubmitAndWait();

            var view = CreateImageView(image, ColorFormat, ImageAspectFlags.ColorBit, mipLevels);
            var descriptorSet = AllocateTextureDescriptorSet(view, pool);
            return new GpuTexture(image, memory, view, descriptorSet);
        }
        finally
        {
            vk.DestroyBuffer(_device, staging, null);
            vk.FreeMemory(_device, stagingMemory, null);
        }
    }

    private void TransitionMipLevel(
        Image image, uint baseLevel, uint levelCount,
        ImageLayout oldLayout, ImageLayout newLayout,
        AccessFlags srcAccess, AccessFlags dstAccess,
        PipelineStageFlags srcStage, PipelineStageFlags dstStage)
    {
        var barrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            SrcAccessMask = srcAccess,
            DstAccessMask = dstAccess,
            OldLayout = oldLayout,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = image,
            SubresourceRange = new ImageSubresourceRange(ImageAspectFlags.ColorBit, baseLevel, levelCount, 0, 1),
        };
        _vk!.CmdPipelineBarrier(
            _commandBuffer, srcStage, dstStage, 0, 0, null, 0, null, 1, &barrier);
    }

    private DescriptorSet AllocateTextureDescriptorSet(ImageView view, DescriptorPool pool)
    {
        var vk = _vk!;
        var setLayout = _descriptorSetLayout;
        var allocInfo = new DescriptorSetAllocateInfo
        {
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = pool,
            DescriptorSetCount = 1,
            PSetLayouts = &setLayout,
        };
        Check(vk.AllocateDescriptorSets(_device, in allocInfo, out var descriptorSet), "AllocateDescriptorSets");

        var imageInfo = new DescriptorImageInfo
        {
            Sampler = _sampler,
            ImageView = view,
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal,
        };
        var write = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = descriptorSet,
            DstBinding = 0,
            DescriptorCount = 1,
            DescriptorType = DescriptorType.CombinedImageSampler,
            PImageInfo = &imageInfo,
        };
        vk.UpdateDescriptorSets(_device, 1, in write, 0, null);
        return descriptorSet;
    }

    private void DestroyTexture(GpuTexture texture)
    {
        var vk = _vk!;
        vk.DestroyImageView(_device, texture.View, null);
        vk.DestroyImage(_device, texture.Image, null);
        vk.FreeMemory(_device, texture.Memory, null);
        // The descriptor set is freed with its pool.
    }

    #endregion

    #region | Resource helpers |

    private (Buffer Buffer, DeviceMemory Memory) CreateBuffer(
        ulong byteCount, BufferUsageFlags usage, MemoryPropertyFlags memoryProperties)
    {
        var vk = _vk!;
        var bufferInfo = new BufferCreateInfo
        {
            SType = StructureType.BufferCreateInfo,
            Size = byteCount,
            Usage = usage,
            SharingMode = SharingMode.Exclusive,
        };
        Check(vk.CreateBuffer(_device, in bufferInfo, null, out var buffer), "CreateBuffer");

        vk.GetBufferMemoryRequirements(_device, buffer, out var requirements);
        var allocInfo = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = FindMemoryType(requirements.MemoryTypeBits, memoryProperties),
        };
        Check(vk.AllocateMemory(_device, in allocInfo, null, out var memory), "AllocateMemory");
        Check(vk.BindBufferMemory(_device, buffer, memory, 0), "BindBufferMemory");
        return (buffer, memory);
    }

    private void DestroyBuffer(GpuBuffer buffer)
    {
        var vk = _vk!;
        vk.DestroyBuffer(_device, buffer.Buffer, null);
        vk.FreeMemory(_device, buffer.Memory, null);
    }

    private (Image Image, DeviceMemory Memory) CreateImage(
        uint width, uint height, uint mipLevels, Format format, ImageUsageFlags usage)
    {
        var vk = _vk!;
        var imageInfo = new ImageCreateInfo
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Format = format,
            Extent = new Extent3D(width, height, 1),
            MipLevels = mipLevels,
            ArrayLayers = 1,
            Samples = SampleCountFlags.Count1Bit,
            Tiling = ImageTiling.Optimal,
            Usage = usage,
            SharingMode = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Undefined,
        };
        Check(vk.CreateImage(_device, in imageInfo, null, out var image), "CreateImage");

        vk.GetImageMemoryRequirements(_device, image, out var requirements);
        var allocInfo = new MemoryAllocateInfo
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = FindMemoryType(requirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit),
        };
        Check(vk.AllocateMemory(_device, in allocInfo, null, out var memory), "AllocateMemory");
        Check(vk.BindImageMemory(_device, image, memory, 0), "BindImageMemory");
        return (image, memory);
    }

    private ImageView CreateImageView(Image image, Format format, ImageAspectFlags aspect, uint mipLevels)
    {
        var viewInfo = new ImageViewCreateInfo
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = image,
            ViewType = ImageViewType.Type2D,
            Format = format,
            SubresourceRange = new ImageSubresourceRange(aspect, 0, mipLevels, 0, 1),
        };
        Check(_vk!.CreateImageView(_device, in viewInfo, null, out var view), "CreateImageView");
        return view;
    }

    private uint FindMemoryType(uint typeBits, MemoryPropertyFlags properties)
    {
        for (uint i = 0; i < _memoryProperties.MemoryTypeCount; i++)
        {
            if ((typeBits & (1u << (int)i)) != 0
                && (_memoryProperties.MemoryTypes[(int)i].PropertyFlags & properties) == properties)
            {
                return i;
            }
        }
        throw new InvalidOperationException($"No Vulkan memory type satisfies {properties}.");
    }

    private static void Check(Result result, string operation)
    {
        if (result != Result.Success)
        {
            throw new InvalidOperationException($"Vulkan {operation} failed: {result}.");
        }
    }

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) { return; }
        _disposed = true;

        var vk = _vk;
        if (vk is null) { return; }

        if (_device.Handle != 0)
        {
            vk.DeviceWaitIdle(_device);
            ReleaseModelResources();
            DestroyRenderTargets();

            if (_whiteTexture is not null) { DestroyTexture(_whiteTexture); _whiteTexture = null; }
            if (_staticDescriptorPool.Handle != 0) { vk.DestroyDescriptorPool(_device, _staticDescriptorPool, null); }
            if (_pipeline.Handle != 0) { vk.DestroyPipeline(_device, _pipeline, null); }
            if (_renderPass.Handle != 0) { vk.DestroyRenderPass(_device, _renderPass, null); }
            if (_pipelineLayout.Handle != 0) { vk.DestroyPipelineLayout(_device, _pipelineLayout, null); }
            if (_descriptorSetLayout.Handle != 0) { vk.DestroyDescriptorSetLayout(_device, _descriptorSetLayout, null); }
            if (_sampler.Handle != 0) { vk.DestroySampler(_device, _sampler, null); }
            if (_fence.Handle != 0) { vk.DestroyFence(_device, _fence, null); }
            if (_commandPool.Handle != 0) { vk.DestroyCommandPool(_device, _commandPool, null); }
            vk.DestroyDevice(_device, null);
        }

        if (_instance.Handle != 0)
        {
            vk.DestroyInstance(_instance, null);
        }

        vk.Dispose();
        _vk = null;
    }
}
