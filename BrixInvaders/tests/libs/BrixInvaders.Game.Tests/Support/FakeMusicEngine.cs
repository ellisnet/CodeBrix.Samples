using System;
using System.Collections.Generic;
using BrixInvaders.Game.Audio;
using CodeBrix.Platform.GameEngine.GeneratedMusic;

namespace BrixInvaders.Game.Tests.Support;

/// <summary>
/// Records what the music director asks of the engine; every started session is a <see cref="FakeMusicStream"/>, so
/// no model loads and no audio device opens. Posted work runs at once unless <see cref="HoldPosts"/> is set.
/// </summary>
internal sealed class FakeMusicEngine : IMusicEngine
{
    public int RegisterCount { get; private set; }

    public List<GeneratedMusicOptions> Started { get; } = new List<GeneratedMusicOptions>();

    public List<FakeMusicStream> Streams { get; } = new List<FakeMusicStream>();

    public List<double> Volumes { get; } = new List<double>();

    public List<FakeDuck> PushedDucks { get; } = new List<FakeDuck>();

    public List<float> TimedDucks { get; } = new List<float>();

    public List<Action> HeldPosts { get; } = new List<Action>();

    public bool HoldPosts { get; set; }

    public Exception StartFailure { get; set; }

    public FakeMusicStream Current => Streams.Count == 0 ? null : Streams[Streams.Count - 1];

    public void RegisterEverything() => RegisterCount++;

    public IMusicStream Start(GeneratedMusicOptions options)
    {
        if (StartFailure != null)
        {
            throw StartFailure;
        }

        Started.Add(options);
        var stream = new FakeMusicStream(options);
        Streams.Add(stream);
        return stream;
    }

    public void SetMusicVolume(double volume) => Volumes.Add(volume);

    public IDisposable PushDuck(float depth, TimeSpan attack, TimeSpan release)
    {
        var duck = new FakeDuck(depth);
        PushedDucks.Add(duck);
        return duck;
    }

    public void Duck(float depth, TimeSpan attack, TimeSpan hold, TimeSpan release) => TimedDucks.Add(depth);

    public void Post(Action action)
    {
        if (HoldPosts)
        {
            HeldPosts.Add(action);
        }
        else
        {
            action();
        }
    }

    public void RunHeldPosts()
    {
        var posts = HeldPosts.ToArray();
        HeldPosts.Clear();
        foreach (var post in posts)
        {
            post();
        }
    }

    internal sealed class FakeDuck : IDisposable
    {
        public FakeDuck(float depth) => Depth = depth;

        public float Depth { get; }

        public bool IsReleased { get; private set; }

        public void Dispose() => IsReleased = true;
    }
}
