using System;
using CodeBrix.Platform.GameEngine.Host.Rendering;
using PalmVisualizer.Rendering;
using Xunit;

namespace PalmVisualizer.Rendering.Tests;

public class VisualizerSessionFactoryTests
{
    private sealed class CanvaslessHost : IGameCanvasHost
    {
        public GameSurfaceCanvas Canvas => null;
    }

    [Fact]
    public void CreateSession_throws_when_no_host_is_given()
    {
        //Arrange
        IVisualizerSessionFactory factory = new VisualizerSessionFactory();

        //Act / Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreateSession(null));
    }

    [Fact]
    public void CreateSession_throws_when_the_host_has_no_canvas()
    {
        //Arrange - a page whose canvas has not been created yet
        IVisualizerSessionFactory factory = new VisualizerSessionFactory();

        //Act / Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreateSession(new CanvaslessHost()));
    }
}
