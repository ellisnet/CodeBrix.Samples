using System.Numerics;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

public class GltfModelLoaderTests
{
    private readonly GltfModelLoader _loader = new();

    [Fact]
    public void triangle_glb_loads_with_geometry_material_and_bounds()
    {
        //Arrange
        var glb = TestAssets.BuildTriangleGlb();

        //Act
        var model = _loader.Load(new MemoryStream(glb));

        //Assert
        model.Primitives.Should().ContainSingle();
        var primitive = model.Primitives[0];
        primitive.VertexCount.Should().Be(3);
        primitive.TriangleCount.Should().Be(1);
        primitive.Indices.Should().HaveCount(3);
        primitive.Normals.Should().HaveCount(9);
        primitive.TexCoords.Should().HaveCount(6);

        model.BoundsMin.Should().Be(new Vector3(0f, 0f, 0f));
        model.BoundsMax.Should().Be(new Vector3(1f, 1f, 0f));
        model.TriangleCount.Should().Be(1);

        var material = model.Materials[primitive.MaterialIndex];
        material.BaseColorFactor.X.Should().BeApproximately(1f, 1e-5f);
        material.BaseColorFactor.Y.Should().BeApproximately(0f, 1e-5f);
        material.DoubleSided.Should().BeTrue();
    }

    [Fact]
    public void node_transforms_are_baked_into_the_vertices()
    {
        //Arrange
        var glb = TestAssets.BuildTriangleGlb(translation: new Vector3(10f, 0f, 0f));

        //Act
        var model = _loader.Load(new MemoryStream(glb));

        //Assert
        model.BoundsMin.X.Should().BeApproximately(10f, 1e-4f);
        model.BoundsMax.X.Should().BeApproximately(11f, 1e-4f);
    }

    [Fact]
    public void missing_normals_are_generated_facing_the_triangle()
    {
        //Arrange - triangle in the XY plane, wound counter-clockwise → +Z normal
        var glb = TestAssets.BuildTriangleGlb();

        //Act
        var model = _loader.Load(new MemoryStream(glb));

        //Assert
        var normals = model.Primitives[0].Normals;
        for (var i = 0; i < normals.Length; i += 3)
        {
            normals[i + 2].Should().BeApproximately(1f, 1e-4f);
        }
    }

    [Fact]
    public void generate_smooth_normals_averages_adjacent_faces()
    {
        //Arrange - two triangles sharing an edge, both in the XY plane
        float[] positions = [0f, 0f, 0f, 1f, 0f, 0f, 0f, 1f, 0f, 1f, 1f, 0f];
        uint[] indices = [0, 1, 2, 2, 1, 3];

        //Act
        var normals = GltfModelLoader.GenerateSmoothNormals(positions, indices);

        //Assert - every vertex normal is +Z
        for (var i = 0; i < normals.Length; i += 3)
        {
            normals[i].Should().BeApproximately(0f, 1e-5f);
            normals[i + 1].Should().BeApproximately(0f, 1e-5f);
            normals[i + 2].Should().BeApproximately(1f, 1e-5f);
        }
    }

    [Fact]
    public void non_gltf_data_is_rejected()
    {
        //Act
        var act = () => _loader.Load(new MemoryStream([9, 9, 9, 9, 9, 9, 9, 9]));

        //Assert
        act.Should().Throw<InvalidDataException>();
    }
}
