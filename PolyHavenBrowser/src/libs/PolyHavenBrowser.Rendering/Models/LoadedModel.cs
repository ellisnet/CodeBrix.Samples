using System.Numerics;

namespace PolyHavenBrowser.Rendering;

/// <summary>
/// A 3D model loaded into renderer-ready form: triangle primitives in scene space,
/// their materials, and the model's bounding box (for camera framing).
/// </summary>
public sealed class LoadedModel
{
    /// <summary>The model name, when the source file provides one.</summary>
    public string? Name { get; init; }

    /// <summary>The renderable triangle batches.</summary>
    public required IReadOnlyList<ModelPrimitive> Primitives { get; init; }

    /// <summary>The materials referenced by <see cref="ModelPrimitive.MaterialIndex"/>.</summary>
    public required IReadOnlyList<ModelMaterial> Materials { get; init; }

    /// <summary>The minimum corner of the axis-aligned bounding box, in scene space.</summary>
    public required Vector3 BoundsMin { get; init; }

    /// <summary>The maximum corner of the axis-aligned bounding box, in scene space.</summary>
    public required Vector3 BoundsMax { get; init; }

    /// <summary>The center of the bounding box.</summary>
    public Vector3 BoundsCenter => (BoundsMin + BoundsMax) * 0.5f;

    /// <summary>The radius of the bounding sphere enclosing the bounding box.</summary>
    public float BoundsRadius => (BoundsMax - BoundsMin).Length() * 0.5f;

    /// <summary>The total number of triangles across all primitives.</summary>
    public int TriangleCount
    {
        get
        {
            var count = 0;
            foreach (var primitive in Primitives)
            {
                count += primitive.TriangleCount;
            }
            return count;
        }
    }
}
