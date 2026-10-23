using System;
using Microsoft.UI.Xaml.Controls;
using PalmVisualizer.Rendering;
using Xunit;

namespace PalmVisualizer.Rendering.Tests;

public class VisualizerSessionFactoryTests
{
    private sealed class PanellessHost : IVisualizerCanvasHost
    {
        public Panel Container => null;
    }

    [Theory]
    [InlineData(VisualizerKind.EtherealWebGpu)]
    [InlineData(VisualizerKind.EtherealOpenGL)]
    [InlineData(VisualizerKind.RippleWebGpu)]
    [InlineData(VisualizerKind.HolyInannaWebGpu)]
    [InlineData(VisualizerKind.CrystalsWebGpu)]
    public void CreateSession_throws_when_no_host_is_given(VisualizerKind kind)
    {
        //Arrange
        IVisualizerSessionFactory factory = new VisualizerSessionFactory();

        //Act / Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreateSession(kind, null));
    }

    [Theory]
    [InlineData(VisualizerKind.EtherealWebGpu)]
    [InlineData(VisualizerKind.EtherealOpenGL)]
    [InlineData(VisualizerKind.RippleWebGpu)]
    [InlineData(VisualizerKind.HolyInannaWebGpu)]
    [InlineData(VisualizerKind.CrystalsWebGpu)]
    public void CreateSession_throws_when_the_host_has_no_panel(VisualizerKind kind)
    {
        //Arrange - a page whose panel has not been created yet
        IVisualizerSessionFactory factory = new VisualizerSessionFactory();

        //Act / Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreateSession(kind, new PanellessHost()));
    }

    [Fact]
    public void CreateSession_throws_for_an_unknown_visualizer()
    {
        //Arrange
        IVisualizerSessionFactory factory = new VisualizerSessionFactory();

        //Act / Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => factory.CreateSession((VisualizerKind)99, new PanellessHost()));
    }

    [Fact]
    public void The_options_list_every_visualizer_with_ethereal_webgpu_first()
    {
        //Assert - the first option is the default the view model starts with
        Assert.Equal(5, VisualizerOption.All.Count);
        Assert.Equal(VisualizerKind.EtherealWebGpu, VisualizerOption.All[0].Kind);
        Assert.Equal("Ethereal - WebGPU", VisualizerOption.All[0].ToString());
        Assert.Equal(VisualizerKind.EtherealOpenGL, VisualizerOption.All[1].Kind);
        Assert.Equal("Ethereal - OpenGL", VisualizerOption.All[1].ToString());
        Assert.Equal(VisualizerKind.RippleWebGpu, VisualizerOption.All[2].Kind);
        Assert.Equal("Ripple - WebGPU", VisualizerOption.All[2].ToString());
        Assert.Equal(VisualizerKind.HolyInannaWebGpu, VisualizerOption.All[3].Kind);
        Assert.Equal("Holy Inanna - WebGPU", VisualizerOption.All[3].ToString());
        Assert.Equal(VisualizerKind.CrystalsWebGpu, VisualizerOption.All[4].Kind);
        Assert.Equal("Crystals - WebGPU", VisualizerOption.All[4].ToString());
    }

    [Theory]
    [InlineData(VisualizerKind.EtherealWebGpu)]
    [InlineData(VisualizerKind.EtherealOpenGL)]
    [InlineData(VisualizerKind.RippleWebGpu)]
    [InlineData(VisualizerKind.HolyInannaWebGpu)]
    [InlineData(VisualizerKind.CrystalsWebGpu)]
    public void Every_visualizer_has_an_option(VisualizerKind kind)
    {
        Assert.Equal(kind, VisualizerOption.For(kind).Kind);
    }

    [Fact]
    public void An_unknown_visualizer_has_no_option()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VisualizerOption.For((VisualizerKind)99));
    }
}
