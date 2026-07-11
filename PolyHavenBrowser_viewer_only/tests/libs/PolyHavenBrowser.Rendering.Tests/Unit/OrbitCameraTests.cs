using System.Numerics;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

public class OrbitCameraTests
{
    [Fact]
    public void eye_sits_behind_the_target_at_zero_yaw_and_pitch()
    {
        //Arrange
        var camera = new OrbitCamera
        {
            YawDegrees = 0f,
            PitchDegrees = 0f,
            Distance = 5f,
            Target = new Vector3(1f, 2f, 3f),
        };

        //Act
        var eye = camera.GetEyePosition();

        //Assert
        eye.X.Should().BeApproximately(1f, 1e-5f);
        eye.Y.Should().BeApproximately(2f, 1e-5f);
        eye.Z.Should().BeApproximately(8f, 1e-5f); // +Z of the target, looking back at it
    }

    [Fact]
    public void pitch_is_clamped()
    {
        //Arrange
        var camera = new OrbitCamera();

        //Act
        camera.Orbit(0f, -720f);

        //Assert
        camera.PitchDegrees.Should().Be(-89f);
    }

    [Fact]
    public void zoom_scales_the_distance_and_respects_the_minimum()
    {
        //Arrange
        var camera = new OrbitCamera { Distance = 10f, MinDistance = 1f };

        //Act
        camera.Zoom(0.5f);
        var closer = camera.Distance;
        camera.Zoom(0.0001f);
        var floored = camera.Distance;

        //Assert
        closer.Should().Be(5f);
        floored.Should().Be(1f);
    }

    [Fact]
    public void zoom_rejects_non_positive_factors()
    {
        //Arrange
        var camera = new OrbitCamera();

        //Act
        var act = () => camera.Zoom(0f);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void pan_slides_the_target_in_the_view_plane()
    {
        //Arrange - looking along -Z; view-plane up is +Y, right is... derived from the basis
        var camera = new OrbitCamera { YawDegrees = 0f, PitchDegrees = 0f, Distance = 2f, Target = Vector3.Zero };

        //Act
        camera.Pan(0f, 0.5f);

        //Assert - vertical pan moves the target along world +Y when the camera is level
        camera.Target.X.Should().BeApproximately(0f, 1e-5f);
        camera.Target.Y.Should().BeGreaterThan(0f);
        camera.Target.Z.Should().BeApproximately(0f, 1e-5f);
    }

    [Fact]
    public void view_matrix_looks_at_the_target()
    {
        //Arrange
        var camera = new OrbitCamera { Target = new Vector3(0f, 1f, 0f), Distance = 4f };

        //Act - transforming the target into view space must land on the -Z axis
        var view = camera.GetViewMatrix();
        var targetInView = Vector3.Transform(camera.Target, view);

        //Assert
        targetInView.X.Should().BeApproximately(0f, 1e-4f);
        targetInView.Y.Should().BeApproximately(0f, 1e-4f);
        targetInView.Z.Should().BeApproximately(-4f, 1e-4f);
    }

    [Fact]
    public void projection_rejects_a_non_positive_aspect_ratio()
    {
        //Arrange
        var camera = new OrbitCamera();

        //Act
        var act = () => camera.GetProjectionMatrix(0f);

        //Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void fit_to_bounds_targets_the_center_and_backs_off_far_enough()
    {
        //Arrange
        var camera = new OrbitCamera { FovDegrees = 45f };
        var min = new Vector3(-1f, -1f, -1f);
        var max = new Vector3(1f, 1f, 1f);

        //Act
        camera.FitToBounds(min, max);

        //Assert
        camera.Target.Should().Be(Vector3.Zero);
        var radius = (max - min).Length() * 0.5f;
        camera.Distance.Should().BeGreaterThan(radius); // outside the bounding sphere
    }

    [Fact]
    public void fit_to_model_uses_the_model_bounds()
    {
        //Arrange
        var camera = new OrbitCamera();
        var model = TestAssets.BuildTriangleModel();

        //Act
        camera.FitToModel(model);

        //Assert
        camera.Target.X.Should().BeApproximately(0.5f, 1e-5f);
        camera.Target.Y.Should().BeApproximately(0.5f, 1e-5f);
        camera.Distance.Should().BeGreaterThan(0f);
    }
}
