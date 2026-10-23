using System;
using System.Text;
using CodeBrix.WebGpu.Native;
using GpuBuffer = CodeBrix.WebGpu.Native.Buffer;
using Wgpu = CodeBrix.WebGpu.Native.WebGpu;

namespace PalmVisualizer.Rendering;

/// <summary>
/// The raw CodeBrix.WebGpu.Native calls the Ripple and Holy Inanna renderers share: shader
/// modules from WGSL, render and compute pipelines whose layouts are derived from the shader,
/// buffers, and bind groups of buffers. Every object made here belongs to the caller, who
/// releases it (in the view's DeviceLost); the device and queue stay the view's. No function
/// here sets a label - the engine ends the process on every *SetLabel call.
/// </summary>
internal static unsafe class WgslGpu
{
    /// <summary>Compiles WGSL into a shader module.</summary>
    public static ShaderModule CreateShaderModule(Device device, string wgsl)
    {
        var code = Encoding.UTF8.GetBytes(wgsl);
        fixed (byte* text = code)
        {
            var source = new ShaderSourceWGSL { Code = WgpuString.FromPinned(text, code.Length) };
            source.Chain.SType = ShaderSourceWGSL.StructType;
            var descriptor = new ShaderModuleDescriptor { NextInChain = &source.Chain };
            return Wgpu.DeviceCreateShaderModule(device, &descriptor);
        }
    }

    /// <summary>
    /// A render pipeline drawing a triangle list with the module's "vs" and "fs" entry points
    /// into one colour target: replacing what is there, or - with
    /// <paramref name="premultipliedBlend"/> - blending premultiplied colour over it (so an
    /// output with zero alpha adds light).
    /// </summary>
    public static RenderPipeline CreateRenderPipeline(Device device, ShaderModule module, TextureFormat format, bool premultipliedBlend = false)
    {
        fixed (byte* vs = "vs"u8)
        fixed (byte* fs = "fs"u8)
        {
            //Premultiplied source-over: out = src + dst * (1 - src.a)
            var over = new BlendComponent { Operation = BlendOperation.Add, SrcFactor = BlendFactor.One, DstFactor = BlendFactor.OneMinusSrcAlpha };
            var blend = new BlendState { Color = over, Alpha = over };
            var target = new ColorTargetState { Format = format, WriteMask = ColorWriteMask.All, Blend = premultipliedBlend ? &blend : null };
            var fragment = new FragmentState
            {
                Module = module,
                EntryPoint = WgpuString.FromPinned(fs, 2),
                TargetCount = 1,
                Targets = &target,
            };
            var descriptor = new RenderPipelineDescriptor
            {
                Vertex = new VertexState { Module = module, EntryPoint = WgpuString.FromPinned(vs, 2) },
                Primitive = new PrimitiveState { Topology = PrimitiveTopology.TriangleList, FrontFace = FrontFace.CCW, CullMode = CullMode.None },
                Multisample = new MultisampleState { Count = 1, Mask = 0xFFFFFFFF },
                Fragment = &fragment,
            };
            return Wgpu.DeviceCreateRenderPipeline(device, &descriptor);
        }
    }

    /// <summary>A compute pipeline running the module's "step" entry point.</summary>
    public static ComputePipeline CreateComputePipeline(Device device, ShaderModule module)
    {
        fixed (byte* step = "step"u8)
        {
            var descriptor = new ComputePipelineDescriptor
            {
                Compute = new ComputeState { Module = module, EntryPoint = WgpuString.FromPinned(step, 4) },
            };
            return Wgpu.DeviceCreateComputePipeline(device, &descriptor);
        }
    }

    /// <summary>A buffer of <paramref name="bytes"/> bytes, zero-filled (as WebGPU creates every buffer).</summary>
    public static GpuBuffer CreateBuffer(Device device, BufferUsage usage, int bytes)
    {
        var descriptor = new BufferDescriptor { Usage = usage, Size = (ulong)bytes };
        return Wgpu.DeviceCreateBuffer(device, &descriptor);
    }

    /// <summary>
    /// A bind group for group 0 of a pipeline's derived layout: buffer i bound whole at binding i.
    /// </summary>
    public static BindGroup CreateBindGroup(Device device, BindGroupLayout layout, ReadOnlySpan<(GpuBuffer Buffer, int Bytes)> buffers)
    {
        Span<BindGroupEntry> entries = stackalloc BindGroupEntry[buffers.Length];
        for (int i = 0; i < buffers.Length; i++)
        {
            entries[i] = new BindGroupEntry { Binding = (uint)i, Buffer = buffers[i].Buffer, Offset = 0, Size = (ulong)buffers[i].Bytes };
        }
        fixed (BindGroupEntry* first = entries)
        {
            var descriptor = new BindGroupDescriptor { Layout = layout, EntryCount = (nuint)buffers.Length, Entries = first };
            return Wgpu.DeviceCreateBindGroup(device, &descriptor);
        }
    }

    /// <summary>Uploads floats into a buffer; the write runs at the start of the frame's submit.</summary>
    public static void WriteBuffer(Queue queue, GpuBuffer buffer, ReadOnlySpan<float> data)
    {
        fixed (float* values = data)
        {
            Wgpu.QueueWriteBuffer(queue, buffer, 0, values, (nuint)(data.Length * sizeof(float)));
        }
    }
}
