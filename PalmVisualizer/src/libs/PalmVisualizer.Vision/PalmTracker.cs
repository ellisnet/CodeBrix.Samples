using CodeBrix.VideoProcessing.OpenCV5;
using PalmVisualizer.Vision.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace PalmVisualizer.Vision;

/// <summary>
/// The multi-palm tracking pipeline: feed it webcam frames (BGRA, any size) with
/// <see cref="SubmitFrame"/> and it raises <see cref="TrackingUpdated"/> with every palm's
/// position and open-palm state. Each palm keeps a stable <see cref="TrackedPalm.TrackId"/>
/// across frames (nearest-neighbor matching against the previous frame), so a consumer can
/// follow individual hands as they move. Inference runs on the tracker's own worker thread
/// with latest-frame-wins semantics - submitting faster than the models can process simply
/// drops stale frames, so the capture pipeline is never blocked.
/// </summary>
public sealed class PalmTracker : IDisposable
{
    private const string DetectorResourceName = "PalmVisualizer.Vision.Models.hand_detector.tflite";
    private const string LandmarkerResourceName = "PalmVisualizer.Vision.Models.hand_landmarks_detector.tflite";

    /// <summary>The most palms tracked at once (the palm detector examines the whole frame each time).</summary>
    public const int MaxPalms = 4;

    /// <summary>The minimum landmark-model presence confidence for a hand to count as present.</summary>
    public const float PresenceThreshold = 0.5f;

    /// <summary>
    /// The exponential-moving-average factor for each palm's position (1 = no smoothing,
    /// smaller = smoother but laggier tracking).
    /// </summary>
    public const float SmoothingAlpha = 0.5f;

    /// <summary>
    /// How far (normalized, relative to the frame) a palm may move between consecutive
    /// frames and still be recognized as the same hand.
    /// </summary>
    public const float TrackMatchMaxDistance = 0.25f;

    private sealed class PalmTrack
    {
        internal int Id;
        internal float SmoothedX;
        internal float SmoothedY;
    }

    private readonly object _pendingLock = new object();
    private byte[] _pendingFrame;
    private int _pendingWidth;
    private int _pendingHeight;
    private bool _hasPending;

    private byte[] _workingFrame;
    private Mat _bgraMat;
    private Mat _bgrMat;

    private Thread _worker;
    private AutoResetEvent _frameSignal;
    private volatile bool _running;

    //Worker-thread state: the palms being followed and the next free track id
    private readonly List<PalmTrack> _tracks = new List<PalmTrack>();
    private int _nextTrackId = 1;

    /// <summary>Indicates whether the tracker's worker is running.</summary>
    public bool IsRunning => _running;

    /// <summary>
    /// Raised after each processed frame - including all-hands-lost frames, so subscribers
    /// can release their palm-driven state. Raised on the tracker's WORKER thread: handlers
    /// must marshal any UI work themselves.
    /// </summary>
    public event EventHandler<PalmTrackingEventArgs> TrackingUpdated;

    /// <summary>
    /// Starts the tracker: loads the embedded models and spins up the inference worker.
    /// Safe to call when already started.
    /// </summary>
    public void Start()
    {
        if (_running) { return; }

        _frameSignal = new AutoResetEvent(false);
        _running = true;
        _worker = new Thread(WorkerLoop)
        {
            Name = "PalmVisualizer.PalmTracker",
            IsBackground = true,
        };
        _worker.Start();
    }

    /// <summary>Stops the inference worker. Safe to call when already stopped.</summary>
    public void Stop()
    {
        if (!_running) { return; }

        _running = false;
        _frameSignal.Set();
        _worker.Join();
        _worker = null;

        _frameSignal.Dispose();
        _frameSignal = null;

        lock (_pendingLock) { _hasPending = false; }
        _tracks.Clear();
    }

    /// <summary>
    /// Offers a frame to the tracker. The pixels are copied before returning, so the caller
    /// may reuse its buffer immediately. When the worker is still busy with an earlier
    /// frame, the previous pending frame is silently replaced (latest wins).
    /// </summary>
    /// <param name="bgraPixels">The frame's tightly packed 32-bit BGRA pixels.</param>
    /// <param name="width">The frame's width in pixels.</param>
    /// <param name="height">The frame's height in pixels.</param>
    public void SubmitFrame(byte[] bgraPixels, int width, int height)
    {
        if (!_running || bgraPixels == null || width < 1 || height < 1) { return; }

        int needed = width * height * 4;
        if (bgraPixels.Length < needed) { return; }

        lock (_pendingLock)
        {
            if (_pendingFrame == null || _pendingFrame.Length != needed)
            {
                _pendingFrame = new byte[needed];
            }
            Array.Copy(bgraPixels, _pendingFrame, needed);
            _pendingWidth = width;
            _pendingHeight = height;
            _hasPending = true;
        }
        _frameSignal.Set();
    }

    private void WorkerLoop()
    {
        PalmDetector detector = null;
        HandLandmarker landmarker = null;
        try
        {
            detector = new PalmDetector(LoadEmbeddedModel(DetectorResourceName));
            landmarker = new HandLandmarker(LoadEmbeddedModel(LandmarkerResourceName));

            while (_running)
            {
                _frameSignal.WaitOne();
                if (!_running) { break; }

                int width;
                int height;
                lock (_pendingLock)
                {
                    if (!_hasPending) { continue; }

                    //Swap the pending buffer out under the lock; copy-free hand-off
                    (_workingFrame, _pendingFrame) = (_pendingFrame, _workingFrame);
                    width = _pendingWidth;
                    height = _pendingHeight;
                    _hasPending = false;
                }

                try
                {
                    PalmTrackingResult result = ProcessFrame(detector, landmarker, _workingFrame, width, height);
                    TrackingUpdated?.Invoke(this, new PalmTrackingEventArgs(result));
                }
                catch (Exception ex) when (!_running)
                {
                    //Shutting down: a frame was in flight when the tracker - or the native
                    //  OpenCV runtime at process exit - began tearing down (e.g. "terminated
                    //  TLS container"). The app is going away; exit the loop quietly rather
                    //  than surfacing this as a fatal unhandled exception on the worker thread.
                    Debug.WriteLine($"PalmTracker worker stopping during shutdown: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    //A single frame failed to process - drop it and keep tracking rather than
                    //  taking down the whole application over one bad frame.
                    Debug.WriteLine($"PalmTracker skipped a frame: {ex.Message}");
                }
            }
        }
        finally
        {
            detector?.Dispose();
            landmarker?.Dispose();
            _bgraMat?.Dispose();
            _bgraMat = null;
            _bgrMat?.Dispose();
            _bgrMat = null;
        }
    }

    private PalmTrackingResult ProcessFrame(PalmDetector detector, HandLandmarker landmarker,
        byte[] bgraPixels, int width, int height)
    {
        if (_bgraMat == null || _bgraMat.Width != width || _bgraMat.Height != height)
        {
            _bgraMat?.Dispose();
            _bgraMat = new Mat(height, width, MatType.CV_8UC4);
            _bgrMat?.Dispose();
            _bgrMat = new Mat();
        }
        Marshal.Copy(bgraPixels, 0, _bgraMat.Data, width * height * 4);
        Cv2.CvtColor(_bgraMat, _bgrMat, ColorConversionCodes.BGRA2BGR);

        //Run the landmark model over every detected palm, keeping the hands the model
        //  confirms are present
        IReadOnlyList<PalmDetection> detections = detector.DetectAll(_bgrMat, MaxPalms);
        var candidates = new List<(float X, float Y, bool IsOpen, float DetectionScore, float PresenceScore)>();
        foreach (PalmDetection palm in detections)
        {
            LandmarkInference inference = landmarker.Infer(_bgrMat, palm);
            if (inference.PresenceScore < PresenceThreshold) { continue; }

            Point2f palmCenter = OpenPalmClassifier.GetPalmCenter(inference.ImageLandmarks);
            candidates.Add((
                Math.Clamp(palmCenter.X / width, 0f, 1f),
                Math.Clamp(palmCenter.Y / height, 0f, 1f),
                OpenPalmClassifier.IsOpenPalm(inference.ImageLandmarks),
                palm.Score,
                inference.PresenceScore));
        }

        if (candidates.Count == 0)
        {
            _tracks.Clear();
            return PalmTrackingResult.Empty;
        }

        //Match this frame's palms to the tracks from the previous frame: greedy
        //  nearest-neighbor, closest pairs first, so each physical hand keeps its id
        int[] trackForCandidate = MatchCandidatesToTracks(candidates);

        var survivingTracks = new List<PalmTrack>(candidates.Count);
        var palms = new List<TrackedPalm>(candidates.Count);
        for (int c = 0; c < candidates.Count; c++)
        {
            var candidate = candidates[c];
            PalmTrack track;
            if (trackForCandidate[c] >= 0)
            {
                track = _tracks[trackForCandidate[c]];
                track.SmoothedX += (candidate.X - track.SmoothedX) * SmoothingAlpha;
                track.SmoothedY += (candidate.Y - track.SmoothedY) * SmoothingAlpha;
            }
            else
            {
                track = new PalmTrack { Id = _nextTrackId++, SmoothedX = candidate.X, SmoothedY = candidate.Y };
            }
            survivingTracks.Add(track);
            palms.Add(new TrackedPalm(track.Id, candidate.IsOpen,
                track.SmoothedX, track.SmoothedY, candidate.DetectionScore, candidate.PresenceScore));
        }

        //Tracks that matched nothing this frame are dropped (their hands left the view)
        _tracks.Clear();
        _tracks.AddRange(survivingTracks);

        //Report in stable track order so consumers see a consistent sequence
        palms.Sort((a, b) => a.TrackId.CompareTo(b.TrackId));
        return new PalmTrackingResult(palms);
    }

    private int[] MatchCandidatesToTracks(
        List<(float X, float Y, bool IsOpen, float DetectionScore, float PresenceScore)> candidates)
    {
        var trackForCandidate = new int[candidates.Count];
        for (int c = 0; c < trackForCandidate.Length; c++) { trackForCandidate[c] = -1; }

        if (_tracks.Count == 0) { return trackForCandidate; }

        var pairs = new List<(float Distance, int Candidate, int Track)>();
        for (int c = 0; c < candidates.Count; c++)
        {
            for (int t = 0; t < _tracks.Count; t++)
            {
                float dx = candidates[c].X - _tracks[t].SmoothedX;
                float dy = candidates[c].Y - _tracks[t].SmoothedY;
                var distance = (float)Math.Sqrt((dx * dx) + (dy * dy));
                if (distance <= TrackMatchMaxDistance)
                {
                    pairs.Add((distance, c, t));
                }
            }
        }
        pairs.Sort((a, b) => a.Distance.CompareTo(b.Distance));

        var trackTaken = new bool[_tracks.Count];
        foreach ((float _, int candidate, int track) in pairs)
        {
            if (trackForCandidate[candidate] >= 0 || trackTaken[track]) { continue; }
            trackForCandidate[candidate] = track;
            trackTaken[track] = true;
        }
        return trackForCandidate;
    }

    internal static byte[] LoadEmbeddedModel(string resourceName)
    {
        using Stream stream = typeof(PalmTracker).Assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Embedded model not found: {resourceName}");
        }
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        TrackingUpdated = null;
        Stop();
    }
}
