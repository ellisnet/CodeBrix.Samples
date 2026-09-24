using System;
using CodeBrix.Platform.GameEngine.GeneratedMusic;

namespace BrixInvaders.Game.Audio;

/// <summary>
/// The engine side of the music wiring, as <see cref="GeneratedMusicDirector"/> uses it: registration, starting a
/// generated-music session, the music bus level, ducking and the engine dispatcher. <see cref="EngineMusicEngine"/> is
/// the real one; tests pass a recording fake so no model ever loads.
/// </summary>
public interface IMusicEngine
{
    /// <summary>Registers the instrument libraries and the model generators (once; later calls do nothing).</summary>
    void RegisterEverything();

    /// <summary>
    /// Starts a FRESH generated-music session with these options, replacing (and disposing) any earlier one.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <returns>The new session.</returns>
    IMusicStream Start(GeneratedMusicOptions options);

    /// <summary>Sets the music bus level (the player's music slider).</summary>
    /// <param name="volume">The level, 0..1.</param>
    void SetMusicVolume(double volume);

    /// <summary>Ducks the music until the returned handle is disposed.</summary>
    /// <param name="depth">The level to duck to, 0 (silent) to 1 (no duck).</param>
    /// <param name="attack">How long to fade down over.</param>
    /// <param name="release">How long to fade back up over when released.</param>
    /// <returns>The handle; dispose it to release the duck.</returns>
    IDisposable PushDuck(float depth, TimeSpan attack, TimeSpan release);

    /// <summary>Ducks the music for a fixed time, then restores it.</summary>
    /// <param name="depth">The level to duck to, 0 (silent) to 1 (no duck).</param>
    /// <param name="attack">How long to fade down over.</param>
    /// <param name="hold">How long to stay ducked.</param>
    /// <param name="release">How long to fade back up over.</param>
    void Duck(float depth, TimeSpan attack, TimeSpan hold, TimeSpan release);

    /// <summary>Runs an action on the engine thread (at once when already on it).</summary>
    /// <param name="action">The action.</param>
    void Post(Action action);
}
