using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BrixInvaders.Game.Audio;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Audio;
using CodeBrix.Platform.GameEngine.GeneratedMusic;

namespace BrixInvaders.Game.Tests.Support;

/// <summary>
/// A fake of the engine's music seams (starter, music manager, dispatcher) that records what the music director asks;
/// every started session is a <see cref="FakeMusicStream"/>, so no model loads and no audio device opens. Posted work
/// runs at once unless <see cref="HoldPosts"/> is set.
/// </summary>
internal sealed class FakeMusicEngine : IGeneratedMusicStarter, IMusicManager, IEngineDispatcher
{
    public int RegisterCount { get; private set; }

    public List<GeneratedMusicOptions> Started { get; } = new List<GeneratedMusicOptions>();

    public List<FakeMusicStream> Streams { get; } = new List<FakeMusicStream>();

    public List<float> Volumes { get; } = new List<float>();

    public List<FakeDuck> PushedDucks { get; } = new List<FakeDuck>();

    public List<float> TimedDucks { get; } = new List<float>();

    public List<string> Stingers { get; } = new List<string>();

    public List<Action> HeldPosts { get; } = new List<Action>();

    public bool HoldPosts { get; set; }

    public Exception StartFailure { get; set; }

    public FakeMusicStream Current => Streams.Count == 0 ? null : Streams[Streams.Count - 1];

    public float MusicVolume
    {
        get => Volumes.Count == 0 ? 1f : Volumes[Volumes.Count - 1];
        set => Volumes.Add(value);
    }

    public float DuckMultiplier => 1f;

    public bool IsOnEngineThread => true;

    /// <summary>A director over this fake, counting registrations.</summary>
    public GeneratedMusicDirector CreateDirector() => new GeneratedMusicDirector(this, this, this, () => RegisterCount++);

    public IGeneratedMusicSession Start(GeneratedMusicOptions options)
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

    public IDisposable PushDuck(float depth, TimeSpan attack = default, TimeSpan release = default)
    {
        var duck = new FakeDuck(depth);
        PushedDucks.Add(duck);
        return duck;
    }

    public void Duck(float depth, TimeSpan attack, TimeSpan hold, TimeSpan release) => TimedDucks.Add(depth);

    public void ClearDucks(TimeSpan release = default) => PushedDucks.ForEach(duck => duck.Dispose());

    public bool PlayStinger(string resourceKey, float volume = 1f, bool duckMusic = false, float duckDepth = 0.3f) =>
        PlayStingerOnBus(resourceKey, AudioBus.Music, volume, duckMusic, duckDepth);

    public bool PlayStingerOnBus(string resourceKey, AudioBus bus, float volume = 1f, bool duckMusic = false, float duckDepth = 0.3f)
    {
        Stingers.Add($"{resourceKey} on {bus}");
        return true;
    }

    public IDisposable PlayStingerWithHeldDuck(string resourceKey, float duckDepth, TimeSpan attack = default,
        TimeSpan release = default, AudioBus bus = AudioBus.Sfx, float volume = 1f)
    {
        PlayStingerOnBus(resourceKey, bus, volume);
        return PushDuck(duckDepth, attack, release);
    }

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

    public Task PostAsync(Func<Task> action) => action();

    public void Drain() => RunHeldPosts();

    public void BindToCurrentThread()
    {
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
