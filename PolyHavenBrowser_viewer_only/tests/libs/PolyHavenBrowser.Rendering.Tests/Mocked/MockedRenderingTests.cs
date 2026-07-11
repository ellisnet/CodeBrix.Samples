using CodeBrix.TestMocks.Mocking;
using SilverAssertions;
using Xunit;

namespace PolyHavenBrowser.Rendering.Tests;

/// <summary>
/// Tests demonstrating how app code that consumes <see cref="IModelLoader"/> and
/// <see cref="IModelSceneRenderer"/> can be tested offline with CodeBrix.TestMocks.
/// </summary>
public class MockedRenderingTests
{
    [Fact]
    public void mocked_loader_returns_a_canned_model()
    {
        //Arrange
        var model = TestAssets.BuildTriangleModel();
        var mock = new Mock<IModelLoader>();
        mock.Setup(l => l.LoadFile("/downloads/potted_plant_01.glb")).Returns(model);

        //Act
        var loaded = mock.Object.LoadFile("/downloads/potted_plant_01.glb");

        //Assert
        loaded.Should().BeSameAs(model);
        mock.Verify(l => l.LoadFile(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void mocked_loader_can_simulate_a_corrupt_file()
    {
        //Arrange
        var mock = new Mock<IModelLoader>();
        mock.Setup(l => l.Load(It.IsAny<Stream>()))
            .Throws(new InvalidDataException("corrupt"));

        //Act
        var act = () => mock.Object.Load(new MemoryStream([1, 2, 3]));

        //Assert
        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void viewer_flow_loads_then_hands_the_model_to_the_renderer()
    {
        //Arrange - the typical app flow: load a model, give it to the scene renderer
        var model = TestAssets.BuildTriangleModel();
        var loaderMock = new Mock<IModelLoader>(MockBehavior.Strict);
        loaderMock.Setup(l => l.LoadFile("model.glb")).Returns(model);

        var rendererMock = new Mock<IModelSceneRenderer>(MockBehavior.Strict);
        rendererMock.Setup(r => r.SetModel(model, true));

        //Act
        var loaded = loaderMock.Object.LoadFile("model.glb");
        rendererMock.Object.SetModel(loaded, frameCamera: true);

        //Assert
        loaderMock.VerifyAll();
        rendererMock.VerifyAll();
        rendererMock.VerifyNoOtherCalls();
    }

    [Fact]
    public void mocked_renderer_exposes_a_real_camera_for_input_wiring()
    {
        //Arrange - a mock renderer can still hand out a real OrbitCamera for pointer input
        var camera = new OrbitCamera();
        var rendererMock = new Mock<IModelSceneRenderer>();
        rendererMock.Setup(r => r.Camera).Returns(camera);

        //Act - simulate a drag and a scroll
        rendererMock.Object.Camera.Orbit(15f, -5f);
        rendererMock.Object.Camera.Zoom(0.9f);

        //Assert
        camera.YawDegrees.Should().Be(45f);  // default 30 + 15
        camera.PitchDegrees.Should().Be(10f); // default 15 - 5
    }
}
