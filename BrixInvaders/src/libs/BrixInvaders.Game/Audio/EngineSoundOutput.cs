using CodeBrix.Platform.GameEngine.Audio;

namespace BrixInvaders.Game.Audio;

/// <summary>
/// The engine side of <see cref="ISoundOutput"/>: triggers go through the shared SFX voice pool (<c>TryPlaySfx</c>,
/// priorities honoured) and the levels go straight to the mixer's master and effects buses.
/// </summary>
public sealed class EngineSoundOutput : ISoundOutput
{
    /// <inheritdoc />
    public void Play(SoundCue cue) => AudioResourceManager.Instance.TryPlaySfx(cue.Key, cue.Volume, 0f, cue.Priority);

    /// <inheritdoc />
    public void SetLevels(double master, double effects)
    {
        AudioMixer.MasterVolume = (float)master;
        AudioMixer.SfxVolume = (float)effects;
    }
}
