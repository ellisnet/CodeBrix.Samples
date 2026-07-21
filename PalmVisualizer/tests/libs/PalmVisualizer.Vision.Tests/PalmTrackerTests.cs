using CodeBrix.VideoProcessing.OpenCV5;
using SilverAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;
using PalmVisualizer.Vision;
using PalmVisualizer.Vision.Internal;

namespace PalmVisualizer.Vision.Tests;

public class PalmTrackerTests
{
    private static string TestImagePath =>
        Path.Combine(AppContext.BaseDirectory, "_data", "open_palm_hands.jpg");

    [Fact]
    public void LoadEmbeddedModel_finds_both_models()
    {
        //Act
        byte[] detector = PalmTracker.LoadEmbeddedModel("PalmVisualizer.Vision.Models.hand_detector.tflite");
        byte[] landmarker = PalmTracker.LoadEmbeddedModel("PalmVisualizer.Vision.Models.hand_landmarks_detector.tflite");

        //Assert - the palm detector is ~2.2 MB, the landmarker ~5.2 MB
        (detector.Length > 1_000_000).Should().Be(true);
        (landmarker.Length > 1_000_000).Should().Be(true);
    }

    [Fact]
    public void LoadEmbeddedModel_throws_for_unknown_resource()
        => Assert.Throws<InvalidOperationException>(
            () => PalmTracker.LoadEmbeddedModel("PalmVisualizer.Vision.Models.no_such_model.tflite"));

    [Fact]
    public void Start_and_stop_are_idempotent()
    {
        //Arrange
        using var tracker = new PalmTracker();

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
        //Arrange - the full multi-palm detection + landmark pipeline against a real photograph
        using var detector = new PalmDetector(
            PalmTracker.LoadEmbeddedModel("PalmVisualizer.Vision.Models.hand_detector.tflite"));
        using var landmarker = new HandLandmarker(
            PalmTracker.LoadEmbeddedModel("PalmVisualizer.Vision.Models.hand_landmarks_detector.tflite"));
        using Mat image = Cv2.ImRead(TestImagePath);

        //Act
        IReadOnlyList<PalmDetection> palms = detector.DetectAll(image, PalmTracker.MaxPalms);

        //Assert - at least one palm is found, strongest first, with solid confidence...
        (palms.Count >= 1).Should().Be(true);
        (palms.Count <= PalmTracker.MaxPalms).Should().Be(true);
        (palms[0].Score > 0.7f).Should().Be(true);

        //...its landmarks show a present, open hand...
        LandmarkInference inference = landmarker.Infer(image, palms[0]);
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
        byte[] pixels = LoadTestImageAsBgra(out int width, out int height);

        using var tracker = new PalmTracker();
        PalmTrackingResult result = null;
        using var resultArrived = new ManualResetEventSlim(false);
        tracker.TrackingUpdated += (_, e) =>
        {
            result = e.Result;
            resultArrived.Set();
        };

        //Act
        tracker.Start();
        tracker.SubmitFrame(pixels, width, height);
        bool signaled = resultArrived.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        tracker.Stop();

        //Assert - at least one palm, with a stable id and normalized position, showing open
        signaled.Should().Be(true);
        (result.Palms.Count >= 1).Should().Be(true);
        result.Palms.Any(palm => palm.IsOpenPalm).Should().Be(true);
        foreach (TrackedPalm palm in result.Palms)
        {
            (palm.TrackId > 0).Should().Be(true);
            (palm.PalmCenterX > 0f && palm.PalmCenterX < 1f).Should().Be(true);
            (palm.PalmCenterY > 0f && palm.PalmCenterY < 1f).Should().Be(true);
        }
    }

    [Fact]
    public void Tracker_keeps_track_ids_stable_across_consecutive_frames()
    {
        //Arrange - the same photo twice: the "hands" have not moved, so each palm must
        //  keep the id it was assigned on the first frame
        byte[] pixels = LoadTestImageAsBgra(out int width, out int height);

        using var tracker = new PalmTracker();
        var results = new List<PalmTrackingResult>();
        using var resultArrived = new AutoResetEvent(false);
        tracker.TrackingUpdated += (_, e) =>
        {
            lock (results) { results.Add(e.Result); }
            resultArrived.Set();
        };

        //Act - submit the second frame only after the first result lands, so neither is
        //  dropped by the tracker's latest-frame-wins hand-off
        tracker.Start();
        tracker.SubmitFrame(pixels, width, height);
        bool firstSignaled = resultArrived.WaitOne(TimeSpan.FromSeconds(30));
        tracker.SubmitFrame(pixels, width, height);
        bool secondSignaled = resultArrived.WaitOne(TimeSpan.FromSeconds(30));
        tracker.Stop();

        //Assert
        firstSignaled.Should().Be(true);
        secondSignaled.Should().Be(true);
        results.Count.Should().Be(2);
        (results[0].Palms.Count >= 1).Should().Be(true);
        results[1].Palms.Count.Should().Be(results[0].Palms.Count);
        for (int i = 0; i < results[0].Palms.Count; i++)
        {
            results[1].Palms[i].TrackId.Should().Be(results[0].Palms[i].TrackId);
        }
    }

    [Fact]
    public void Tracker_reports_no_palms_for_a_blank_frame()
    {
        //Arrange
        var pixels = new byte[320 * 240 * 4];   //all black

        using var tracker = new PalmTracker();
        PalmTrackingResult result = null;
        using var resultArrived = new ManualResetEventSlim(false);
        tracker.TrackingUpdated += (_, e) =>
        {
            result = e.Result;
            resultArrived.Set();
        };

        //Act
        tracker.Start();
        tracker.SubmitFrame(pixels, 320, 240);
        bool signaled = resultArrived.Wait(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        tracker.Stop();

        //Assert
        signaled.Should().Be(true);
        result.Palms.Count.Should().Be(0);
    }

    private static byte[] LoadTestImageAsBgra(out int width, out int height)
    {
        using Mat image = Cv2.ImRead(TestImagePath);
        using var bgra = new Mat();
        Cv2.CvtColor(image, bgra, ColorConversionCodes.BGR2BGRA);
        var pixels = new byte[image.Width * image.Height * 4];
        System.Runtime.InteropServices.Marshal.Copy(bgra.Data, pixels, 0, pixels.Length);
        width = image.Width;
        height = image.Height;
        return pixels;
    }
}
