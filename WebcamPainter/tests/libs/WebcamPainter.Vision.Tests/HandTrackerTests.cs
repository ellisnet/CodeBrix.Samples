using CodeBrix.VideoProcessing.OpenCV5;
using SilverAssertions;
using System;
using System.IO;
using System.Threading;
using Xunit;
using WebcamPainter.Vision;
using WebcamPainter.Vision.Internal;

namespace WebcamPainter.Vision.Tests;

public class HandTrackerTests
{
    private static string TestImagePath =>
        Path.Combine(AppContext.BaseDirectory, "_data", "open_palm_hands.jpg");

    [Fact]
    public void LoadEmbeddedModel_finds_both_models()
    {
        //Act
        byte[] detector = HandTracker.LoadEmbeddedModel("WebcamPainter.Vision.Models.hand_detector.tflite");
        byte[] landmarker = HandTracker.LoadEmbeddedModel("WebcamPainter.Vision.Models.hand_landmarks_detector.tflite");

        //Assert - the palm detector is ~2.2 MB, the landmarker ~5.2 MB
        (detector.Length > 1_000_000).Should().Be(true);
        (landmarker.Length > 1_000_000).Should().Be(true);
    }

    [Fact]
    public void LoadEmbeddedModel_throws_for_unknown_resource()
        => Assert.Throws<InvalidOperationException>(
            () => HandTracker.LoadEmbeddedModel("WebcamPainter.Vision.Models.no_such_model.tflite"));

    [Fact]
    public void Start_and_stop_are_idempotent()
    {
        //Arrange
        using var tracker = new HandTracker();

        //Act / Assert
        tracker.IsRunning.Should().Be(false);
        tracker.Start();
        tracker.Start();
        tracker.IsRunning.Should().Be(true);
        tracker.Stop();
        tracker.Stop();
        tracker.IsRunning.Should().Be(false);
    }

    [Fact]
    public void Pipeline_finds_an_open_palm_in_the_test_photo()
    {
        //Arrange - the full palm-detection + landmark pipeline against a real photograph
        using var detector = new PalmDetector(
            HandTracker.LoadEmbeddedModel("WebcamPainter.Vision.Models.hand_detector.tflite"));
        using var landmarker = new HandLandmarker(
            HandTracker.LoadEmbeddedModel("WebcamPainter.Vision.Models.hand_landmarks_detector.tflite"));
        using Mat image = Cv2.ImRead(TestImagePath);

        //Act
        PalmDetection palm = detector.Detect(image);

        //Assert - a palm is found with solid confidence...
        (palm != null).Should().Be(true);
        (palm.Score > 0.7f).Should().Be(true);

        //...its landmarks show a present, open hand...
        LandmarkInference inference = landmarker.Infer(image, palm);
        (inference.PresenceScore > 0.5f).Should().Be(true);
        OpenPalmClassifier.IsOpenPalm(inference.ImageLandmarks).Should().Be(true);

        //...and the palm center lands inside the frame
        Point2f center = OpenPalmClassifier.GetPalmCenter(inference.ImageLandmarks);
        (center.X > 0 && center.X < image.Width).Should().Be(true);
        (center.Y > 0 && center.Y < image.Height).Should().Be(true);
    }

    [Fact]
    public void Tracker_reports_an_open_palm_from_a_submitted_frame()
    {
        //Arrange - feed the tracker the test photo as if it were a webcam frame
        using Mat image = Cv2.ImRead(TestImagePath);
        using var bgra = new Mat();
        Cv2.CvtColor(image, bgra, ColorConversionCodes.BGR2BGRA);
        var pixels = new byte[image.Width * image.Height * 4];
        System.Runtime.InteropServices.Marshal.Copy(bgra.Data, pixels, 0, pixels.Length);

        using var tracker = new HandTracker();
        HandTrackingResult result = null;
        using var resultArrived = new ManualResetEventSlim(false);
        tracker.TrackingUpdated += (_, e) =>
        {
            result = e.Result;
            resultArrived.Set();
        };

        //Act
        tracker.Start();
        tracker.SubmitFrame(pixels, image.Width, image.Height);
        bool signaled = resultArrived.Wait(TimeSpan.FromSeconds(30), CancellationToken.None);
        tracker.Stop();

        //Assert
        signaled.Should().Be(true);
        result.HandDetected.Should().Be(true);
        result.IsOpenPalm.Should().Be(true);
        (result.PalmCenterX > 0f && result.PalmCenterX < 1f).Should().Be(true);
        (result.PalmCenterY > 0f && result.PalmCenterY < 1f).Should().Be(true);
    }

    [Fact]
    public void Tracker_reports_no_hand_for_a_blank_frame()
    {
        //Arrange
        var pixels = new byte[320 * 240 * 4];   //all black

        using var tracker = new HandTracker();
        HandTrackingResult result = null;
        using var resultArrived = new ManualResetEventSlim(false);
        tracker.TrackingUpdated += (_, e) =>
        {
            result = e.Result;
            resultArrived.Set();
        };

        //Act
        tracker.Start();
        tracker.SubmitFrame(pixels, 320, 240);
        bool signaled = resultArrived.Wait(TimeSpan.FromSeconds(30), CancellationToken.None);
        tracker.Stop();

        //Assert
        signaled.Should().Be(true);
        result.HandDetected.Should().Be(false);
        result.IsOpenPalm.Should().Be(false);
    }
}
