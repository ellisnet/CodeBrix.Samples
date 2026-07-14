using SilverAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using WebcamPainter.Webcam;

namespace WebcamPainter.Webcam.Tests;

public class WebcamCaptureServiceTests
{
    [Fact]
    public async Task GetCamerasAsync_enumerates_without_error()
    {
        //Act - device enumeration talks straight to the OS and needs no camera consent,
        //  no libvlc, and no running session; machines without a camera return an empty list
        IReadOnlyList<CameraDevice> cameras = await WebcamCaptureService.GetCamerasAsync();

        //Assert
        (cameras != null).Should().Be(true);
        foreach (CameraDevice camera in cameras)
        {
            String.IsNullOrWhiteSpace(camera.FriendlyName).Should().Be(false);
            camera.ToString().Should().Be(camera.FriendlyName);
        }
    }

    [Fact]
    public void New_service_reports_no_session_and_no_frame()
    {
        //Arrange
        using var service = new WebcamCaptureService();

        //Assert
        service.IsRunning.Should().Be(false);
        service.HasFrame.Should().Be(false);
    }

    [Fact]
    public void TryCopyLatestFrame_returns_false_before_any_frame()
    {
        //Arrange
        using var service = new WebcamCaptureService();
        byte[] buffer = null;

        //Act
        bool copied = service.TryCopyLatestFrame(ref buffer, out int width, out int height);

        //Assert
        copied.Should().Be(false);
        width.Should().Be(0);
        height.Should().Be(0);
    }

    [Fact]
    public void CapturePhoto_throws_when_no_session_is_running()
    {
        //Arrange
        using var service = new WebcamCaptureService();

        //Act / Assert
        Assert.Throws<InvalidOperationException>(() => service.CapturePhoto());
    }

    [Fact]
    public void Start_throws_on_null_camera()
    {
        //Arrange
        using var service = new WebcamCaptureService();

        //Act / Assert
        Assert.Throws<ArgumentNullException>(() => service.Start(null));
    }

    [Fact]
    public void Stop_without_start_is_harmless()
    {
        //Arrange
        using var service = new WebcamCaptureService();

        //Act
        service.Stop();

        //Assert
        service.IsRunning.Should().Be(false);
    }
}
