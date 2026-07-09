using System.Numerics;
using System.Text;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SkiaSharp;

namespace PolyHavenBrowser.Rendering.Tests;

/// <summary>Builders for tiny in-memory test assets (no files on disk, no network).</summary>
internal static class TestAssets
{
    /// <summary>
    /// Encodes a Radiance .hdr file with flat (uncompressed) RGBE scanlines from RGBE
    /// quads given per pixel, row-major.
    /// </summary>
    public static byte[] BuildFlatHdr(int width, int height, params (byte R, byte G, byte B, byte E)[] pixels)
    {
        if (pixels.Length != width * height)
        {
            throw new ArgumentException("Pixel count must match width*height.", nameof(pixels));
        }

        using var stream = new MemoryStream();
        var header = Encoding.ASCII.GetBytes($"#?RADIANCE\nFORMAT=32-bit_rle_rgbe\n\n-Y {height} +X {width}\n");
        stream.Write(header);
        foreach (var (r, g, b, e) in pixels)
        {
            stream.WriteByte(r);
            stream.WriteByte(g);
            stream.WriteByte(b);
            stream.WriteByte(e);
        }

        return stream.ToArray();
    }

    /// <summary>Encodes an SKBitmap-drawn solid-color PNG.</summary>
    public static byte[] BuildPng(int width, int height, SKColor color)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        bitmap.Erase(color);
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return encoded.ToArray();
    }

    /// <summary>
    /// Builds a single-triangle .glb via SharpGLTF.Toolkit: vertices (0,0,0), (1,0,0),
    /// (0,1,0) with a red, double-sided material, optionally translated.
    /// </summary>
    public static byte[] BuildTriangleGlb(Vector3? translation = null)
    {
        var material = new MaterialBuilder("red")
            .WithDoubleSide(true)
            .WithBaseColor(new Vector4(1f, 0f, 0f, 1f));

        var mesh = new MeshBuilder<VertexPosition>("triangle");
        var primitive = mesh.UsePrimitive(material);
        primitive.AddTriangle(
            new VertexPosition(0f, 0f, 0f),
            new VertexPosition(1f, 0f, 0f),
            new VertexPosition(0f, 1f, 0f));

        var scene = new SceneBuilder();
        scene.AddRigidMesh(mesh, Matrix4x4.CreateTranslation(translation ?? Vector3.Zero));
        var model = scene.ToGltf2();

        using var stream = new MemoryStream();
        model.WriteGLB(stream);
        return stream.ToArray();
    }

    /// <summary>Builds a renderer-ready one-triangle <see cref="LoadedModel"/> directly (no glTF).</summary>
    public static LoadedModel BuildTriangleModel()
    {
        float[] positions = [0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f, 0f];
        uint[] indices = [0, 1, 2];
        return new LoadedModel
        {
            Name = "triangle",
            Primitives =
            [
                new ModelPrimitive
                {
                    Positions = positions,
                    Normals = [0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f],
                    TexCoords = new float[6],
                    Indices = indices,
                    MaterialIndex = 0,
                },
            ],
            Materials = [new ModelMaterial { Name = "red", BaseColorFactor = new Vector4(1f, 0f, 0f, 1f) }],
            BoundsMin = Vector3.Zero,
            BoundsMax = new Vector3(1f, 1f, 0f),
        };
    }
}
