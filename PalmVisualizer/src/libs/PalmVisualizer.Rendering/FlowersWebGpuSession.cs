using System;
using CodeBrix.Platform.UI.WebGpu;
using CodeBrix.WebGpu.Native;

namespace PalmVisualizer.Rendering;

/// <summary>
/// "Flowers - WebGPU": eight-petalled flowers growing out of dark pool-table green, lush and
/// vivid under the palms' light, drawn by <see cref="FlowersWebGpuRenderer"/> - see
/// <see cref="WebGpuVisualizerSession"/>.
/// </summary>
public sealed class FlowersWebGpuSession : WebGpuVisualizerSession
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FlowersWebGpuSession"/> class.
    /// </summary>
    /// <param name="host">The page whose panel the visual is shown in.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null,
    /// or when the host has no panel yet.</exception>
    public FlowersWebGpuSession(IVisualizerCanvasHost host) : base(VisualizerKind.FlowersWebGpu, host) { }

    /// <inheritdoc />
    protected override IWebGpuRenderer CreateRenderer(Func<float> clock, Func<double> pixelScale, TextureFormat format)
        => new FlowersWebGpuRenderer(new PalmMotion(AttractorField), clock, format);
}
