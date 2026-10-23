using System;
using CodeBrix.Platform.UI.WebGpu;
using CodeBrix.WebGpu.Native;

namespace PalmVisualizer.Rendering;

/// <summary>
/// "Holy Inanna - WebGPU": the Star of Inanna in 3D over palm-swirled flames, drawn by
/// <see cref="HolyInannaWebGpuRenderer"/> - see <see cref="WebGpuVisualizerSession"/>.
/// </summary>
public sealed class HolyInannaWebGpuSession : WebGpuVisualizerSession
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HolyInannaWebGpuSession"/> class.
    /// </summary>
    /// <param name="host">The page whose panel the visual is shown in.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="host"/> is null,
    /// or when the host has no panel yet.</exception>
    public HolyInannaWebGpuSession(IVisualizerCanvasHost host) : base(VisualizerKind.HolyInannaWebGpu, host) { }

    /// <inheritdoc />
    protected override IWebGpuRenderer CreateRenderer(Func<float> clock, Func<double> pixelScale, TextureFormat format)
        => new HolyInannaWebGpuRenderer(new PalmMotion(AttractorField), clock, format);
}
