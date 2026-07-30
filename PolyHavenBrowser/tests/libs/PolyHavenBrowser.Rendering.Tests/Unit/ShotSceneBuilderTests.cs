using System.Numerics;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

public class ShotSceneBuilderTests
{
    [Fact]
    public void composite_prepends_floor_and_cove_before_the_model_primitives()
    {
        //Arrange
        var model = TestAssets.BuildTriangleModel();

        //Act
        var scene = ShotSceneBuilder.Build(model, ShotStage.Light());

        //Assert
        scene.Composite.Primitives.Count.Should().Be(model.Primitives.Count + 2);
        scene.Composite.Materials.Count.Should().Be(model.Materials.Count + 2);
        scene.StagePrimitives.Count.Should().Be(2);
    }

    [Fact]
    public void model_material_indices_stay_valid_and_stage_indices_point_at_the_appended_materials()
    {
        //Arrange
        var model = TestAssets.BuildTriangleModel();

        //Act
        var scene = ShotSceneBuilder.Build(model, ShotStage.Dark());

        //Assert - the model's primitive still points at material 0 (its red material)
        var modelPrimitive = scene.Composite.Primitives[2];
        modelPrimitive.MaterialIndex.Should().Be(0);
        scene.Composite.Materials[0].Name.Should().Be("red");

        //Assert - the stage primitives point at the two appended stage materials
        scene.StagePrimitives[0].MaterialIndex.Should().Be(1);
        scene.StagePrimitives[1].MaterialIndex.Should().Be(2);
        scene.Composite.Materials[1].Name.Should().Be("shot-stage-floor");
        scene.Composite.Materials[2].Name.Should().Be("shot-stage-cove");
    }

    [Fact]
    public void composite_keeps_the_models_bounds_so_cameras_frame_the_product()
    {
        //Arrange
        var model = TestAssets.BuildTriangleModel();

        //Act
        var scene = ShotSceneBuilder.Build(model, ShotStage.Light());

        //Assert - the stage is far bigger than the model, but the bounds must not grow
        scene.Composite.BoundsMin.Should().Be(model.BoundsMin);
        scene.Composite.BoundsMax.Should().Be(model.BoundsMax);
    }

    [Fact]
    public void the_floor_sits_at_the_models_lowest_point()
    {
        //Arrange
        var model = TestAssets.BuildTriangleModel();

        //Act
        var scene = ShotSceneBuilder.Build(model, ShotStage.Light());

        //Assert - every floor vertex has y == BoundsMin.Y
        var floor = scene.StagePrimitives[0];
        for (var i = 1; i < floor.Positions.Length; i += 3)
        {
            floor.Positions[i].Should().Be(model.BoundsMin.Y);
        }
    }

    [Fact]
    public void the_contact_shadow_is_baked_darker_under_the_model_than_at_the_floors_edge()
    {
        //Arrange
        var model = TestAssets.BuildTriangleModel();
        var stage = ShotStage.Light();

        //Act
        var scene = ShotSceneBuilder.Build(model, stage);

        //Assert - compare the floor texture's center pixel (under the model) with a corner
        var floorMaterial = scene.Composite.Materials[1];
        var size = floorMaterial.BaseColorTextureWidth;
        var rgba = floorMaterial.BaseColorTextureRgba!;

        var centerOffset = ((size / 2) * size + size / 2) * 4;
        var cornerOffset = (2 * size + 2) * 4;
        ((int)rgba[centerOffset]).Should().BeLessThan(rgba[cornerOffset]);
    }

    [Fact]
    public void a_floor_texture_is_resampled_across_the_bake_instead_of_left_repeating()
    {
        //Arrange - a 2x2 red/green/blue/white source texture
        byte[] source =
        [
            255, 0, 0, 255, /**/ 0, 255, 0, 255,
            0, 0, 255, 255, /**/ 255, 255, 255, 255,
        ];
        var model = TestAssets.BuildTriangleModel();
        var stage = ShotStage.Tabletop(source, 2, 2);

        //Act
        var scene = ShotSceneBuilder.Build(model, stage);

        //Assert - the baked floor texture is the big non-repeating bake, not the source
        var floorMaterial = scene.Composite.Materials[1];
        floorMaterial.BaseColorTextureWidth.Should().Be(2048);
        floorMaterial.BaseColorTextureHeight.Should().Be(2048);
        floorMaterial.BaseColorTextureRgba!.Length.Should().Be(2048 * 2048 * 4);
    }

    [Fact]
    public void the_cove_gradient_runs_from_the_bottom_color_to_the_top_color()
    {
        //Arrange
        var model = TestAssets.BuildTriangleModel();
        var stage = ShotStage.Dark();

        //Act
        var scene = ShotSceneBuilder.Build(model, stage);

        //Assert - row 0 of the gradient texture (v=0, floor level) is the bottom color,
        //the last row (v=1, top of the cove) is the top color
        var cove = scene.Composite.Materials[2];
        var rgba = cove.BaseColorTextureRgba!;
        var width = cove.BaseColorTextureWidth;
        var height = cove.BaseColorTextureHeight;

        rgba[0].Should().Be(stage.BackdropBottom.R);
        rgba[1].Should().Be(stage.BackdropBottom.G);
        rgba[2].Should().Be(stage.BackdropBottom.B);

        var lastRow = (height - 1) * width * 4;
        rgba[lastRow].Should().Be(stage.BackdropTop.R);
        rgba[lastRow + 1].Should().Be(stage.BackdropTop.G);
        rgba[lastRow + 2].Should().Be(stage.BackdropTop.B);
    }

    [Fact]
    public void aiming_the_stage_at_the_light_rewrites_every_stage_normal()
    {
        //Arrange
        var model = TestAssets.BuildTriangleModel();
        var scene = ShotSceneBuilder.Build(model, ShotStage.Light());
        var light = Vector3.Normalize(new Vector3(0.3f, 0.8f, 0.5f));

        //Act
        scene.AimStageAtLight(light);

        //Assert - stage normals all equal the light direction; model normals are untouched
        foreach (var primitive in scene.StagePrimitives)
        {
            for (var i = 0; i < primitive.Normals.Length; i += 3)
            {
                primitive.Normals[i].Should().Be(light.X);
                primitive.Normals[i + 1].Should().Be(light.Y);
                primitive.Normals[i + 2].Should().Be(light.Z);
            }
        }
        scene.Composite.Primitives[2].Normals[2].Should().Be(1f);
    }

    [Fact]
    public void shot_angle_key_lights_hang_above_and_beside_each_shots_camera()
    {
        //Act - the key light follows the camera yaw, so back shots are lit too
        var frontLight = ShotAngle.Front.KeyLightDirection();
        var backLight = ShotAngle.Back.KeyLightDirection();

        //Assert - both lights point upward, and they differ (each follows its own camera)
        frontLight.Y.Should().BeGreaterThan(0.5f);
        backLight.Y.Should().BeGreaterThan(0.5f);
        (frontLight - backLight).Length().Should().BeGreaterThan(0.5f);

        frontLight.Length().Should().BeInRange(0.999f, 1.001f);
    }
}
