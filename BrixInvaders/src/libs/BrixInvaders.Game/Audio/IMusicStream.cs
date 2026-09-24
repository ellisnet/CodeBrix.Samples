using System;
using CodeBrix.Platform.GameEngine.Audio;

namespace BrixInvaders.Game.Audio;

/// <summary>
/// One running generated-music session as <see cref="GeneratedMusicDirector"/> sees it: the engine's generated-music
/// provider in the game (<see cref="ProviderMusicStream"/>), a scripted fake in tests.
/// </summary>
public interface IMusicStream
{
    /// <summary>Raised after every state change, on ANY thread (the audio fill thread included).</summary>
    event EventHandler StateChanged;

    /// <summary>The stream's state (Starting while the model loads, Playing once music is heard, ...).</summary>
    StreamingMusicState State { get; }

    /// <summary>What is playing, as one line (empty before the music has started).</summary>
    string Summary { get; }

    /// <summary>What is playing, or null before the music has started.</summary>
    MusicSourceInfo Source { get; }

    /// <summary>Why the stream faulted, or null.</summary>
    Exception Fault { get; }

    /// <summary>How many times the music has waited for the generator (0 when not measured yet).</summary>
    int StarvationGapCount { get; }

    /// <summary>The session's diagnostics as one line (empty when there are none yet).</summary>
    string DiagnosticsSummary { get; }

    /// <summary>
    /// Moves the music on to a preset of the playing generator; the new music takes over at a bar line and the music
    /// never stops meanwhile.
    /// </summary>
    /// <param name="preset">The preset name.</param>
    void FollowUp(string preset);
}
