using CodeBrix.Audio.Opus;
using CodeBrix.VideoPlayback.Dav1d;

namespace CodeBrixVideoTool.Playback.Services;

/// <summary>
/// Turns on the two codecs this application's player needs, once, at start-up.
/// </summary>
/// <remarks>
/// <para>
/// These are the APPLICATION's dependencies, never the player add-in's: AV1 decoding is BSD-2-Clause
/// and Opus is BSD-3-Clause while the add-in is Apache-2.0, so each ships as its own package and an
/// application that wants them references them and calls Register once. The add-in resolves codecs
/// through the playback session's registries, so it plays them with no change and no reference of
/// its own.
/// </para>
/// <para>
/// Dav1d is not optional here: every one of the five supported formats carries AV1, so nothing plays
/// without it - and a Mode 3 master's alpha plane is a monochrome AV1 stream decoded by the same
/// package. Opus is needed for WebM, Matroska and Mode 1; Mode 2 carries Vorbis and Mode 3 carries
/// FLAC, both of which the playback core decodes itself. The same registrations serve the authoring
/// library's exporter, which decodes a master with whatever the application registered. There is
/// deliberately no module initializer doing this - that would work in a debug build and silently not
/// run in a trimmed publish.
/// </para>
/// </remarks>
public static class PlaybackCodecs
{
    private static readonly object Gate = new();

    /// <summary>True once both codecs have been turned on.</summary>
    public static bool IsRegistered { get; private set; }

    /// <summary>
    /// Turns on AV1 video and Opus audio. Safe to call more than once; only the first call does
    /// anything.
    /// </summary>
    public static void RegisterOnce()
    {
        lock (Gate)
        {
            if (IsRegistered)
            {
                return;
            }

            CodeBrixVideoPlaybackDav1d.Register();
            CodeBrixAudioOpus.Register();
            IsRegistered = true;
        }
    }
}
