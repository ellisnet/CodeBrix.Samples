namespace PolyHavenBrowser.Display;

/// <summary>
/// Creates an <see cref="IModelRenderEngine"/> for one specific graphics backend. The app no
/// longer registers a single fixed factory in the container - the runtime choice between
/// backends is made through <see cref="IModelRenderEngineSelector"/>, which uses these
/// per-backend factories internally.
/// </summary>
public interface IModelRenderEngineFactory
{
    /// <summary>Creates a new, unused rendering engine. The caller owns it and disposes it.</summary>
    IModelRenderEngine Create();
}

/// <summary>
/// Creates the OpenGL ES (EGL) engine, <see cref="OpenGlModelRenderEngine"/> - the default
/// backend, available on every head. Its Vulkan sibling is
/// <see cref="VulkanModelRenderEngineFactory"/>.
/// </summary>
public sealed class OpenGlModelRenderEngineFactory : IModelRenderEngineFactory
{
    /// <inheritdoc />
    public IModelRenderEngine Create() => new OpenGlModelRenderEngine();
}
