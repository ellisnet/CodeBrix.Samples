namespace BrixInvaders.Game.Audio;

/// <summary>Where the game session sends sounds and effect/master levels (the engine in the game, a recorder in tests).</summary>
public interface ISoundOutput
{
    /// <summary>Triggers a sound effect.</summary>
    /// <param name="cue">The cue.</param>
    void Play(SoundCue cue);

    /// <summary>Sets the master and effects levels.</summary>
    /// <param name="master">Master volume, 0..1.</param>
    /// <param name="effects">Effects volume, 0..1.</param>
    void SetLevels(double master, double effects);
}
