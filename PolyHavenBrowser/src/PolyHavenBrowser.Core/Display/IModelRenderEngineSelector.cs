using System;
using System.Collections.Generic;
using PolyHavenBrowser.Rendering;

namespace PolyHavenBrowser.Display;

/// <summary>The 3D rendering backends the app can offer.</summary>
public enum RenderEngineKind
{
    /// <summary>OpenGL ES via EGL (<see cref="OpenGlModelRenderEngine"/>) - the default, available on every head.</summary>
    OpenGL,

    /// <summary>Vulkan via Silk.NET (<see cref="VulkanModelRenderEngine"/>) - only on platforms <see cref="VulkanPlatformSupport"/> okays.</summary>
    Vulkan,
}

/// <summary>
/// Lets the view model choose between the available <see cref="IModelRenderEngine"/> backends
/// at runtime (the rendering-engine dropdown). This replaces registering one fixed
/// <see cref="IModelRenderEngineFactory"/> in the container: the selector owns the full list
/// of backends, knows which are supported on the running platform, and creates engines on
/// demand. The UI shows every kind and alerts (rather than hiding the option) when an
/// unsupported one is picked.
/// </summary>
public interface IModelRenderEngineSelector
{
    /// <summary>Every engine kind the app offers, in display order (OpenGL first - the default).</summary>
    IReadOnlyList<RenderEngineKind> AvailableKinds { get; }

    /// <summary>Whether the given engine kind may be used on the platform the app is running on.</summary>
    bool IsSupported(RenderEngineKind kind);

    /// <summary>
    /// Creates a new, unused engine of the given kind. The caller owns it and disposes it.
    /// Throws <see cref="NotSupportedException"/> when <see cref="IsSupported"/> is false for it.
    /// </summary>
    IModelRenderEngine Create(RenderEngineKind kind);
}

/// <summary>
/// The default selector: OpenGL everywhere, Vulkan only on the platforms the Rendering
/// library's hardcoded <see cref="VulkanPlatformSupport"/> allow-list okays.
/// </summary>
public sealed class ModelRenderEngineSelector : IModelRenderEngineSelector
{
    private static readonly RenderEngineKind[] Kinds = [RenderEngineKind.OpenGL, RenderEngineKind.Vulkan];

    /// <inheritdoc />
    public IReadOnlyList<RenderEngineKind> AvailableKinds => Kinds;

    /// <inheritdoc />
    public bool IsSupported(RenderEngineKind kind) => kind switch
    {
        RenderEngineKind.OpenGL => true,
        RenderEngineKind.Vulkan => VulkanPlatformSupport.IsCurrentPlatformSupported,
        _ => false,
    };

    /// <inheritdoc />
    public IModelRenderEngine Create(RenderEngineKind kind)
    {
        if (!IsSupported(kind))
        {
            throw new NotSupportedException($"The {kind} rendering engine is not supported on this platform.");
        }

        return kind switch
        {
            RenderEngineKind.OpenGL => new OpenGlModelRenderEngineFactory().Create(),
            RenderEngineKind.Vulkan => new VulkanModelRenderEngineFactory().Create(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
