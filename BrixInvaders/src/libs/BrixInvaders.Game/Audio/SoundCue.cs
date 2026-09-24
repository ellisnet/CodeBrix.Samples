using BrixInvaders.Assets;

namespace BrixInvaders.Game.Audio;

/// <summary>One sound to trigger: which effect, how loud, and how important it is when the voice pool is full.</summary>
public readonly struct SoundCue
{
    /// <summary>Creates a cue.</summary>
    /// <param name="effect">The effect.</param>
    /// <param name="volume">The trigger volume, 0..1 (the effects bus and master volume apply on top).</param>
    /// <param name="priority">The pool priority; higher wins when the pool must cull.</param>
    public SoundCue(SoundEffect effect, float volume = 1f, int priority = 0)
    {
        Effect = effect;
        Volume = volume;
        Priority = priority;
    }

    /// <summary>The effect.</summary>
    public SoundEffect Effect { get; }

    /// <summary>The trigger volume, 0..1.</summary>
    public float Volume { get; }

    /// <summary>The pool priority; higher wins when the pool must cull.</summary>
    public int Priority { get; }

    /// <summary>The engine audio key of the effect (a <see cref="AssetKeys.Sfx"/> constant).</summary>
    public string Key => SoundEffects.KeyOf(Effect);
}
