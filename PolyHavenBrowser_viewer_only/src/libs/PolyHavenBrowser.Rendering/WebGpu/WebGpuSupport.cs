using System.Text;
using CodeBrix.WebGpu.Native;
using CodeBrix.WebGpu.Skia;

namespace PolyHavenBrowser.Rendering;

/// <summary>
/// The pieces the two WebGPU renderers (<see cref="WebGpuSceneRenderer"/> and
/// <see cref="WebGpuPanoramaRenderer"/>) share: creating the device, compiling WGSL, and turning
/// WebGPU's validation errors into exceptions.
/// <para>
/// Unlike the Vulkan and Metal backends there is <b>no platform allow-list</b>: CodeBrix.WebGpu ships
/// a native WebGPU library for every platform the app runs on (Windows x64/ARM64 on Direct3D 12,
/// Linux x64/ARM64/RISC-V 64 and Android on Vulkan, macOS Intel and Apple Silicon on Metal), so
/// WebGPU is offered on every head. A machine without a usable GPU driver is caught at runtime
/// instead - device creation reports why - and the app falls back (OpenGL for models, the CPU
/// renderer for HDRIs).
/// </para>
/// </summary>
public static unsafe class WebGpuSupport
{
    /// <summary>
    /// Whether a WebGPU device can actually be created on this machine. Intended for tests; the
    /// app itself just tries, and falls back when the attempt fails.
    /// </summary>
    public static bool IsRuntimeAvailable()
    {
        try
        {
            using var device = WebGpuSkiaDevice.Create();
            return device.IsAvailable;
        }
        catch
        {
            // The native library could not be loaded for this runtime identifier.
            return false;
        }
    }

    /// <summary>
    /// Creates a WebGPU device on the platform's own GPU API (Direct3D 12 on Windows, Vulkan on Linux
    /// and Android, Metal on macOS), throwing with the library's reason when none is available.
    /// </summary>
    internal static WebGpuSkiaDevice CreateDevice()
    {
        var device = WebGpuSkiaDevice.Create();
        if (!device.IsAvailable)
        {
            var reason = device.Reason;
            device.Dispose();
            throw new InvalidOperationException("No WebGPU device is available: " + reason);
        }
        return device;
    }

    /// <summary>Compiles a WGSL source into a shader module.</summary>
    internal static ShaderModule CreateShaderModule(Device device, string wgsl)
    {
        var code = Encoding.UTF8.GetBytes(wgsl);
        fixed (byte* text = code)
        {
            var source = new ShaderSourceWGSL { Code = WgpuString.FromPinned(text, code.Length) };
            source.Chain.SType = ShaderSourceWGSL.StructType;
            var descriptor = new ShaderModuleDescriptor { NextInChain = &source.Chain };
            return WebGpu.DeviceCreateShaderModule(device, &descriptor);
        }
    }

    /// <summary>
    /// Throws when the device has reported validation errors (a shader that did not compile, a
    /// pipeline or resource WebGPU rejected) since the last check. The owned device collects these
    /// instead of ending the process, so checking after setup turns a broken driver or shader into
    /// an ordinary exception the app can fall back from.
    /// </summary>
    internal static void ThrowIfErrors(WebGpuSkiaDevice device, string stage)
    {
        var errors = device.TakeUncapturedErrors();
        if (errors.Length > 0)
        {
            throw new InvalidOperationException($"WebGPU {stage} failed: {string.Join(" | ", errors)}");
        }
    }

    /// <summary>Creates a 2D texture of the given format and usage.</summary>
    internal static Texture CreateTexture(Device device, uint width, uint height, TextureFormat format, TextureUsage usage)
    {
        var descriptor = new TextureDescriptor
        {
            Usage = usage,
            Dimension = TextureDimension.Dimension2D,
            Size = new Extent3D { Width = width, Height = height, DepthOrArrayLayers = 1 },
            Format = format,
            MipLevelCount = 1,
            SampleCount = 1,
        };
        return WebGpu.DeviceCreateTexture(device, &descriptor);
    }

    /// <summary>Queues a whole-texture upload of tightly packed rows (it runs at the next submit).</summary>
    internal static void WriteTexture(Queue queue, Texture texture, void* data, nuint dataSize, uint width, uint height, uint bytesPerPixel)
    {
        var destination = new TexelCopyTextureInfo { Texture = texture, Aspect = TextureAspect.All };
        var layout = new TexelCopyBufferLayout { BytesPerRow = width * bytesPerPixel, RowsPerImage = height };
        var size = new Extent3D { Width = width, Height = height, DepthOrArrayLayers = 1 };
        WebGpu.QueueWriteTexture(queue, &destination, data, dataSize, &layout, &size);
    }
}
