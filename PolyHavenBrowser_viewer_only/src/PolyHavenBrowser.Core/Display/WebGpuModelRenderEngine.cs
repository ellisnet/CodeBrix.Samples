using System.Numerics;
using PolyHavenBrowser.Rendering;

namespace PolyHavenBrowser.Display;

/// <summary>
/// The WebGPU implementation of <see cref="IModelRenderEngine"/> - a thin adapter over the
/// Rendering library's self-contained <see cref="WebGpuSceneRenderer"/>, built on CodeBrix.WebGpu
/// (Direct3D 12 on Windows, Vulkan on Linux and Android, Metal on macOS). Like the Vulkan and Metal
/// engines there is no context to make current or restore: WebGPU has no ambient thread state, so
/// the renderer owns its whole stack (device, pipelines, offscreen target, readback) and cannot
/// disturb the host head's own renderer. All WebGPU resources are created lazily on the first
/// <see cref="RenderFrame"/> and freed on <see cref="Dispose"/>.
/// <para>
/// This is the default engine and is offered on <b>every</b> head - there is no allow-list; a
/// machine without a working GPU driver fails the view model's off-thread pre-warm instead, and the
/// app falls back to OpenGL.
/// </para>
/// </summary>
public sealed class WebGpuModelRenderEngine : IModelRenderEngine
{
    private readonly WebGpuSceneRenderer _renderer = new();

    /// <inheritdoc />
    public OrbitCamera Camera => _renderer.Camera;

    /// <inheritdoc />
    public Vector3? FixedLightDirection
    {
        get => _renderer.FixedLightDirection;
        set => _renderer.FixedLightDirection = value;
    }

    /// <inheritdoc />
    public void SetModel(LoadedModel model) => _renderer.SetModel(model);

    /// <inheritdoc />
    public RenderedFrame RenderFrame(int width, int height, (float R, float G, float B, float A) background)
    {
        var pixels = _renderer.RenderFrame(width, height, background);

        // Like Metal, WebGPU's clip-space Y points up while its framebuffer origin is top-left, so
        // the readback is already top-down - no Skia flip needed (see the WebGpuSceneRenderer class
        // remarks).
        return new RenderedFrame(pixels, width, height, isBottomUp: false);
    }

    /// <inheritdoc />
    public void Dispose() => _renderer.Dispose();
}

/// <summary>Creates <see cref="WebGpuModelRenderEngine"/> instances.</summary>
public sealed class WebGpuModelRenderEngineFactory : IModelRenderEngineFactory
{
    /// <inheritdoc />
    public IModelRenderEngine Create() => new WebGpuModelRenderEngine();
}
