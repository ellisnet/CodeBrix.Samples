using System;
using CodeBrix.Platform.UI.WebGpu;
using CodeBrix.WebGpu.Native;

namespace PalmVisualizer.Rendering;

/// <summary>
/// "Ethereal - WebGPU": the Ethereal scene drawn by <see cref="EtherealWebGpuRenderer"/> in a
/// <see cref="WebGpuRenderView"/> - see <see cref="WebGpuVisualizerSession"/>.
/// </summary>
public sealed class EtherealWebGpuSession : WebGpuVisualizerSession
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EtherealWebGpuSession"/> class.
    /// </summary>
    /// <param name="host">The page whose panel the visual is shown in.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null,
    /// or when the host has no panel yet.</exception>
    public EtherealWebGpuSession(IVisualizerCanvasHost host) : base(VisualizerKind.EtherealWebGpu, host) { }

    /// <inheritdoc />
    protected override IWebGpuRenderer CreateRenderer(Func<float> clock, Func<double> pixelScale, TextureFormat format)
        => new EtherealWebGpuRenderer(new EtherealScene(AttractorField), clock, pixelScale, format);
}
