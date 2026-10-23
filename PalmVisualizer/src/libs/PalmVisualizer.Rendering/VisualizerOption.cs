using System;
using System.Collections.Generic;

namespace PalmVisualizer.Rendering;

/// <summary>The visualizers the application can show in Visualize Mode.</summary>
public enum VisualizerKind
{
    /// <summary>
    /// The Ethereal visual drawn with WebGPU through the platform's <c>WebGpuRenderView</c> - the
    /// default on every head.
    /// </summary>
    EtherealWebGpu,

    /// <summary>
    /// The same Ethereal visual drawn with GPU-accelerated Skia through the platform's
    /// <c>SkiaGpuContext</c>: OpenGL (or OpenGL ES) on every head except macOS, where it is Metal.
    /// </summary>
    EtherealOpenGL,

    /// <summary>
    /// An iridescent pool of water drawn with WebGPU: a wave simulation in a compute shader that
    /// the palms stir.
    /// </summary>
    RippleWebGpu,

    /// <summary>
    /// The Star of Inanna (Ishtar) in 3D over palm-swirled flames, drawn with WebGPU.
    /// </summary>
    HolyInannaWebGpu,

    /// <summary>
    /// Glinting, translucent quartz crystals growing over a dark backdrop, drawn with WebGPU;
    /// the palms excite the crystals near them.
    /// </summary>
    CrystalsWebGpu,
}

/// <summary>
/// One entry of the visualizer dropdown: the visualizer and the name shown for it.
/// </summary>
public sealed class VisualizerOption
{
    /// <summary>
    /// The options the dropdown lists, in display order; the first is the default. WebGPU leads
    /// because it is the faster path, most of all on the WPF head: its Readback overlaps each
    /// frame's copy off the GPU with the next frame, where the OpenGL path waits for it.
    /// </summary>
    public static IReadOnlyList<VisualizerOption> All { get; } = new[]
    {
        new VisualizerOption(VisualizerKind.EtherealWebGpu, "Ethereal - WebGPU"),
        new VisualizerOption(VisualizerKind.EtherealOpenGL, "Ethereal - OpenGL"),
        new VisualizerOption(VisualizerKind.RippleWebGpu, "Ripple - WebGPU"),
        new VisualizerOption(VisualizerKind.HolyInannaWebGpu, "Holy Inanna - WebGPU"),
        new VisualizerOption(VisualizerKind.CrystalsWebGpu, "Crystals - WebGPU"),
    };

    /// <summary>The option for a visualizer.</summary>
    /// <param name="kind">The visualizer.</param>
    /// <returns>Its option, from <see cref="All"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="kind"/> is not
    /// a known visualizer.</exception>
    public static VisualizerOption For(VisualizerKind kind)
    {
        foreach (var option in All)
        {
            if (option.Kind == kind) { return option; }
        }
        throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown visualizer.");
    }

    private VisualizerOption(VisualizerKind kind, string displayName)
    {
        Kind = kind;
        DisplayName = displayName;
    }

    /// <summary>The visualizer this option selects.</summary>
    public VisualizerKind Kind { get; }

    /// <summary>The name shown in the dropdown and the status line.</summary>
    public string DisplayName { get; }

    /// <inheritdoc />
    public override string ToString() => DisplayName;
}
