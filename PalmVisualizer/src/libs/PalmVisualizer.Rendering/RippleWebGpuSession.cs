using System;
using CodeBrix.Platform.UI.WebGpu;
using CodeBrix.WebGpu.Native;

namespace PalmVisualizer.Rendering;

/// <summary>
/// "Ripple - WebGPU": an iridescent pool whose waves the palms stir, drawn by
/// <see cref="RippleWebGpuRenderer"/> - see <see cref="WebGpuVisualizerSession"/>.
/// </summary>
public sealed class RippleWebGpuSession : WebGpuVisualizerSession
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RippleWebGpuSession"/> class.
    /// </summary>
    /// <param name="host">The page whose panel the visual is shown in.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null,
    /// or when the host has no panel yet.</exception>
    public RippleWebGpuSession(IVisualizerCanvasHost host) : base(VisualizerKind.RippleWebGpu, host) { }

    /// <inheritdoc />
    protected override IWebGpuRenderer CreateRenderer(Func<float> clock, Func<double> pixelScale, TextureFormat format)
        => new RippleWebGpuRenderer(new PalmMotion(AttractorField), clock, format);
}
