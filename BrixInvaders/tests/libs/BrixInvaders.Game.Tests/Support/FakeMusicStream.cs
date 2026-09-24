using System;
using System.Collections.Generic;
using BrixInvaders.Game.Audio;
using CodeBrix.Platform.GameEngine.Audio;
using CodeBrix.Platform.GameEngine.GeneratedMusic;

namespace BrixInvaders.Game.Tests.Support;

/// <summary>A scripted generated-music session: the test moves its state and it records the follow-ups asked of it.</summary>
internal sealed class FakeMusicStream : IMusicStream
{
    public FakeMusicStream(GeneratedMusicOptions options)
    {
        Options = options;
        State = StreamingMusicState.Starting;
    }

    public event EventHandler StateChanged;

    public GeneratedMusicOptions Options { get; }

    public List<string> FollowUps { get; } = new List<string>();

    public Exception FollowUpFailure { get; set; }

    public StreamingMusicState State { get; private set; }

    public string Summary { get; set; } = string.Empty;

    public MusicSourceInfo Source { get; set; }

    public Exception Fault { get; set; }

    public int StarvationGapCount { get; set; }

    public string DiagnosticsSummary { get; set; } = string.Empty;

    public int SubscriberCount => StateChanged?.GetInvocationList().Length ?? 0;

    public void FollowUp(string preset)
    {
        if (FollowUpFailure != null)
        {
            throw FollowUpFailure;
        }

        FollowUps.Add(preset);
    }

    public void MoveTo(StreamingMusicState state)
    {
        State = state;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void PlayModel(string generator, string library)
    {
        Source = new MusicSourceInfo(generator, library, false);
        Summary = $"{generator} ({generator}) through {library}, voiced by test";
        MoveTo(StreamingMusicState.Playing);
    }
}
