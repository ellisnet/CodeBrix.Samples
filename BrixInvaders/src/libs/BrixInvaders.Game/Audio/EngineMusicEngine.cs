using System;
using BrixInvaders.Music;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Audio;
using CodeBrix.Platform.GameEngine.GeneratedMusic;

namespace BrixInvaders.Game.Audio;

/// <summary>
/// The real <see cref="IMusicEngine"/>: <see cref="MusicSetup.RegisterEverything"/>, <c>Engine.UseGeneratedMusic</c>,
/// the engine mixer's music bus, <see cref="MusicManager"/> ducking and the engine dispatcher. It reads
/// <see cref="Engine.Instance"/> when called, so it can be created before the engine starts.
/// </summary>
public sealed class EngineMusicEngine : IMusicEngine
{
    /// <inheritdoc />
    public void RegisterEverything() => MusicSetup.RegisterEverything();

    /// <inheritdoc />
    public IMusicStream Start(GeneratedMusicOptions options) => new ProviderMusicStream(Engine.Instance.UseGeneratedMusic(options));

    /// <inheritdoc />
    public void SetMusicVolume(double volume) => AudioMixer.MusicVolume = (float)Math.Clamp(volume, 0.0, 1.0);

    /// <inheritdoc />
    public IDisposable PushDuck(float depth, TimeSpan attack, TimeSpan release) => MusicManager.Instance.PushDuck(depth, attack, release);

    /// <inheritdoc />
    public void Duck(float depth, TimeSpan attack, TimeSpan hold, TimeSpan release) => MusicManager.Instance.Duck(depth, attack, hold, release);

    /// <inheritdoc />
    public void Post(Action action) => Engine.Instance.EngineDispatcher.Post(action);
}
