using System;

namespace PalmVisualizer.Rendering;

/// <summary>
/// The <see cref="IVisualizerSessionFactory"/> the application registers: hands back the
/// session for the chosen visualizer, bound to the host page's panel.
/// </summary>
public sealed class VisualizerSessionFactory : IVisualizerSessionFactory
{
    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null,
    /// or when the host has no panel yet.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="kind"/> is not
    /// a known visualizer.</exception>
    public IVisualizerSession CreateSession(VisualizerKind kind, IVisualizerCanvasHost host)
    {
        if (host == null) { throw new ArgumentNullException(nameof(host)); }

        return kind switch
        {
            VisualizerKind.EtherealWebGpu => new EtherealWebGpuSession(host),
            VisualizerKind.EtherealOpenGL => new EtherealOpenGLSession(host),
            VisualizerKind.RippleWebGpu => new RippleWebGpuSession(host),
            VisualizerKind.HolyInannaWebGpu => new HolyInannaWebGpuSession(host),
            VisualizerKind.CrystalsWebGpu => new CrystalsWebGpuSession(host),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown visualizer."),
        };
    }
}
