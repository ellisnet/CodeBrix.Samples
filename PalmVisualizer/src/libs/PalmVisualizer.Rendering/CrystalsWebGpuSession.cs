using System;
using CodeBrix.Platform.UI.WebGpu;
using CodeBrix.WebGpu.Native;

namespace PalmVisualizer.Rendering;

/// <summary>
/// "Crystals - WebGPU": translucent quartz crystals growing over a dark backdrop, excited by the
/// palms, drawn by <see cref="CrystalsWebGpuRenderer"/> - see <see cref="WebGpuVisualizerSession"/>.
/// </summary>
public sealed class CrystalsWebGpuSession : WebGpuVisualizerSession
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CrystalsWebGpuSession"/> class.
    /// </summary>
    /// <param name="host">The page whose panel the visual is shown in.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null,
    /// or when the host has no panel yet.</exception>
    public CrystalsWebGpuSession(IVisualizerCanvasHost host) : base(VisualizerKind.CrystalsWebGpu, host) { }

    /// <inheritdoc />
    protected override IWebGpuRenderer CreateRenderer(Func<float> clock, Func<double> pixelScale, TextureFormat format)
        => new CrystalsWebGpuRenderer(new PalmMotion(AttractorField), clock, format);
}
