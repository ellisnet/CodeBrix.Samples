using System.Numerics;

namespace PolyHavenBrowser.Rendering;

/// <summary>
/// The display-relevant subset of a glTF material: base color (factor and optional
/// texture) plus the flags the preview renderer needs. Metallic/roughness factors are
/// carried for future PBR shading but are not used by the current preview shading.
/// </summary>
public sealed class ModelMaterial
{
    /// <summary>The material name, when the source file provides one.</summary>
    public string? Name { get; init; }

    /// <summary>The RGBA base color factor. Defaults to opaque white.</summary>
    public Vector4 BaseColorFactor { get; init; } = Vector4.One;

    /// <summary>The decoded base color texture as RGBA bytes (4 per pixel), or <see langword="null"/> when untextured.</summary>
    public byte[]? BaseColorTextureRgba { get; init; }

    /// <summary>The base color texture width in pixels (0 when untextured).</summary>
    public int BaseColorTextureWidth { get; init; }

    /// <summary>The base color texture height in pixels (0 when untextured).</summary>
    public int BaseColorTextureHeight { get; init; }

    /// <summary>The glTF metallic factor (carried for future use).</summary>
    public float MetallicFactor { get; init; } = 1f;

    /// <summary>The glTF roughness factor (carried for future use).</summary>
    public float RoughnessFactor { get; init; } = 1f;

    /// <summary>Whether the material renders both triangle faces.</summary>
    public bool DoubleSided { get; init; }
}
