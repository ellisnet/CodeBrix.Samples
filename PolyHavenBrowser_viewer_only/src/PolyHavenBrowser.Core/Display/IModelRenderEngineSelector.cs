using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using PolyHavenBrowser.Rendering;

// ReSharper disable InconsistentNaming

namespace PolyHavenBrowser.Display;

/// <summary>The 3D rendering backends the app can offer.</summary>
public enum RenderEngineKind
{
    /// <summary>WebGPU via CodeBrix.WebGpu (<see cref="WebGpuModelRenderEngine"/>) - the default, offered on every head (Android included).</summary>
    WebGpu,

    /// <summary>OpenGL via the head's native GL context (<see cref="OpenGlModelRenderEngine"/>) - available on every head, and the fallback when WebGPU cannot start.</summary>
    OpenGL,

    /// <summary>Vulkan via Silk.NET (<c>VulkanModelRenderEngine</c>) - only on platforms <see cref="VulkanPlatformSupport"/> okays.</summary>
    Vulkan,

    /// <summary>Metal via the raw Objective-C runtime (<see cref="MetalModelRenderEngine"/>) - only on platforms <see cref="MetalPlatformSupport"/> okays (macOS).</summary>
    Metal,
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
    /// <summary>Every engine kind the app offers, in display order (WebGPU first - the default).</summary>
    IReadOnlyList<RenderEngineKind> AvailableKinds { get; }

    /// <summary>Whether the given engine kind may be used on the platform the app is running on.</summary>
    bool IsSupported(RenderEngineKind kind);

    /// <summary>
    /// Creates a new, unused engine of the given kind. The caller owns it and disposes it.
    /// Throws <see cref="NotSupportedException"/> when <see cref="IsSupported"/> is false for it.
    /// </summary>
    /// <param name="kind">The backend to create.</param>
    /// <param name="getXamlRoot">
    /// Returns the hosting page's <see cref="XamlRoot"/>; used by the OpenGL engine to create its
    /// offscreen native GL context (the WebGPU, Vulkan and Metal engines own their own stack and ignore it).
    /// </param>
    IModelRenderEngine Create(RenderEngineKind kind, Func<XamlRoot> getXamlRoot);
}

/// <summary>
/// The default selector: WebGPU and OpenGL everywhere, Vulkan only on the platforms the Rendering
/// library's hardcoded <see cref="VulkanPlatformSupport"/> allow-list okays, and Metal only on
/// the platforms <see cref="MetalPlatformSupport"/> okays (macOS). The two GPU-API gates are
/// mirror images - Vulkan excludes macOS, Metal is macOS-only. WebGPU needs no gate: CodeBrix.WebGpu
/// ships its native library for every platform the app runs on (see <see cref="WebGpuSupport"/>).
/// </summary>
public sealed class ModelRenderEngineSelector : IModelRenderEngineSelector
{
    private static readonly RenderEngineKind[] Kinds =
        [RenderEngineKind.WebGpu, RenderEngineKind.OpenGL, RenderEngineKind.Vulkan, RenderEngineKind.Metal];

    /// <inheritdoc />
    public IReadOnlyList<RenderEngineKind> AvailableKinds => Kinds;

    /// <inheritdoc />
    public bool IsSupported(RenderEngineKind kind) => kind switch
    {
        RenderEngineKind.WebGpu => true,
        RenderEngineKind.OpenGL => true,
        RenderEngineKind.Vulkan => VulkanPlatformSupport.IsCurrentPlatformSupported,
        RenderEngineKind.Metal => MetalPlatformSupport.IsCurrentPlatformSupported,
        _ => false,
    };

    /// <inheritdoc />
    public IModelRenderEngine Create(RenderEngineKind kind, Func<XamlRoot> getXamlRoot)
    {
        if (!IsSupported(kind))
        {
            throw new NotSupportedException($"The {kind} rendering engine is not supported on this platform.");
        }

        return kind switch
        {
            RenderEngineKind.WebGpu => new WebGpuModelRenderEngineFactory().Create(),
            RenderEngineKind.OpenGL => new OpenGlModelRenderEngineFactory(getXamlRoot).Create(),
#if HAS_CODEBRIX_ANDROID
            //The Vulkan engine is not built for Android; IsSupported is false there, so this is unreachable.
#else
            RenderEngineKind.Vulkan => new VulkanModelRenderEngineFactory().Create(),
#endif
            RenderEngineKind.Metal => new MetalModelRenderEngineFactory().Create(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
