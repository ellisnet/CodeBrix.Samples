namespace PolyHavenBrowser.Display;

/// <summary>
/// Creates the <see cref="IModelRenderEngine"/> the app uses for 3D previews. This is the
/// single point where the graphics backend is chosen: registering a different factory in
/// <c>RegisterServices</c> swaps the whole 3D engine - e.g. a future Vulkan engine - without
/// touching the painter, view models, or UI.
/// <para>
/// To make the choice a runtime user setting (OpenGL vs Vulkan), have a factory implementation
/// read that setting in <see cref="Create"/> and return the corresponding engine.
/// </para>
/// </summary>
public interface IModelRenderEngineFactory
{
    /// <summary>Creates a new, unused rendering engine. The caller owns it and disposes it.</summary>
    IModelRenderEngine Create();
}

/// <summary>
/// The default factory: creates the OpenGL ES (EGL) engine, <see cref="OpenGlModelRenderEngine"/>.
/// A Vulkan build would add a sibling factory and register it instead (or a selector factory
/// that chooses between them).
/// </summary>
public sealed class OpenGlModelRenderEngineFactory : IModelRenderEngineFactory
{
    /// <inheritdoc />
    public IModelRenderEngine Create() => new OpenGlModelRenderEngine();
}
