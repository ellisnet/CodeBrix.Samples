using System;
using System.Text;
using CodeBrix.WebGpu.Native;
using GpuBuffer = CodeBrix.WebGpu.Native.Buffer;
using Wgpu = CodeBrix.WebGpu.Native.WebGpu;

namespace PalmVisualizer.Rendering;

/// <summary>
/// One WGSL draw, with nothing but the raw CodeBrix.WebGpu.Native binding: a shader module
/// with a "vs" and an "fs" entry point, a render pipeline for the view's colour format (its
/// layout derived from the shader), and one uniform buffer bound at group 0, binding 0. Created
/// in the view's DeviceReady and released at DeviceLost; the device and queue stay the view's.
/// </summary>
internal sealed unsafe class WgslPass : IDisposable
{
    private readonly Queue _queue;
    private ShaderModule _module;
    private RenderPipeline _pipeline;
    private GpuBuffer _uniforms;
    private BindGroup _bindGroup;

    /// <summary>Builds the module, the pipeline and the uniform buffer.</summary>
    /// <param name="device">The view's device.</param>
    /// <param name="queue">The view's queue.</param>
    /// <param name="wgsl">The shader source (entry points "vs" and "fs"; one uniform block at group 0, binding 0).</param>
    /// <param name="format">The colour-target format: the view's PreferredFormat.</param>
    /// <param name="uniformBytes">The uniform block's size in bytes (a multiple of 16).</param>
    /// <param name="premultipliedBlend"><see langword="true"/> to blend the output (premultiplied
    /// colour) over what the pass already holds; <see langword="false"/> to replace it.</param>
    public WgslPass(Device device, Queue queue, string wgsl, TextureFormat format, int uniformBytes, bool premultipliedBlend)
    {
        _queue = queue;
        UniformBytes = uniformBytes;

        var code = Encoding.UTF8.GetBytes(wgsl);
        fixed (byte* text = code)
        {
            var source = new ShaderSourceWGSL { Code = WgpuString.FromPinned(text, code.Length) };
            source.Chain.SType = ShaderSourceWGSL.StructType;
            var descriptor = new ShaderModuleDescriptor { NextInChain = &source.Chain };
            _module = Wgpu.DeviceCreateShaderModule(device, &descriptor);
        }

        fixed (byte* vs = "vs"u8)
        fixed (byte* fs = "fs"u8)
        {
            //Premultiplied source-over: out = src + dst * (1 - src.a)
            var over = new BlendComponent { Operation = BlendOperation.Add, SrcFactor = BlendFactor.One, DstFactor = BlendFactor.OneMinusSrcAlpha };
            var blend = new BlendState { Color = over, Alpha = over };
            var target = new ColorTargetState
            {
                Format = format,
                WriteMask = ColorWriteMask.All,
                Blend = premultipliedBlend ? &blend : null,
            };
            var fragment = new FragmentState
            {
                Module = _module,
                EntryPoint = WgpuString.FromPinned(fs, 2),
                TargetCount = 1,
                Targets = &target,
            };
            var descriptor = new RenderPipelineDescriptor
            {
                Vertex = new VertexState { Module = _module, EntryPoint = WgpuString.FromPinned(vs, 2) },
                Primitive = new PrimitiveState { Topology = PrimitiveTopology.TriangleList, FrontFace = FrontFace.CCW, CullMode = CullMode.None },
                Multisample = new MultisampleState { Count = 1, Mask = 0xFFFFFFFF },
                Fragment = &fragment,
            };
            _pipeline = Wgpu.DeviceCreateRenderPipeline(device, &descriptor);
        }

        var bufferDescriptor = new BufferDescriptor { Usage = BufferUsage.Uniform | BufferUsage.CopyDst, Size = (ulong)uniformBytes };
        _uniforms = Wgpu.DeviceCreateBuffer(device, &bufferDescriptor);

        var layout = Wgpu.RenderPipelineGetBindGroupLayout(_pipeline, 0);
        var entry = new BindGroupEntry { Binding = 0, Buffer = _uniforms, Offset = 0, Size = (ulong)uniformBytes };
        var groupDescriptor = new BindGroupDescriptor { Layout = layout, EntryCount = 1, Entries = &entry };
        _bindGroup = Wgpu.DeviceCreateBindGroup(device, &groupDescriptor);
        Wgpu.BindGroupLayoutRelease(layout);
    }

    /// <summary>The uniform block's size in bytes.</summary>
    public int UniformBytes { get; }

    /// <summary>Uploads the uniform block; the write runs at the start of the frame's submit.</summary>
    /// <param name="data">The block (at least <see cref="UniformBytes"/> bytes).</param>
    public void WriteUniforms(ReadOnlySpan<float> data)
    {
        fixed (float* values = data)
        {
            Wgpu.QueueWriteBuffer(_queue, _uniforms, 0, values, (nuint)UniformBytes);
        }
    }

    /// <summary>Records this pass's draw into an open render pass.</summary>
    /// <param name="pass">The open render pass.</param>
    /// <param name="vertexCount">Vertices per instance.</param>
    /// <param name="instanceCount">Instances.</param>
    public void Record(RenderPassEncoder pass, uint vertexCount, uint instanceCount)
    {
        Wgpu.RenderPassEncoderSetPipeline(pass, _pipeline);
        Wgpu.RenderPassEncoderSetBindGroup(pass, 0, _bindGroup, 0, null);
        Wgpu.RenderPassEncoderDraw(pass, vertexCount, instanceCount, 0, 0);
    }

    /// <summary>Releases everything this object created (never the device or the queue: the view owns those).</summary>
    public void Dispose()
    {
        if (!_bindGroup.IsNull) { Wgpu.BindGroupRelease(_bindGroup); _bindGroup = default; }
        if (!_uniforms.IsNull) { Wgpu.BufferRelease(_uniforms); _uniforms = default; }
        if (!_pipeline.IsNull) { Wgpu.RenderPipelineRelease(_pipeline); _pipeline = default; }
        if (!_module.IsNull) { Wgpu.ShaderModuleRelease(_module); _module = default; }
    }
}
