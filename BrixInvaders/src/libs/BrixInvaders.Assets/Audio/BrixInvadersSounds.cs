using System;
using System.Collections.Generic;
using CodeBrix.Platform.GameEngine.Audio;

namespace BrixInvaders.Assets;

/// <summary>
/// Every sound effect, materialized into the engine's audio registry. Returned by
/// <see cref="BrixInvadersAssets.LoadSounds"/>.
/// </summary>
public sealed class BrixInvadersSounds
{
    private readonly IReadOnlyDictionary<SoundEffect, AudioResource> _resources;

    /// <summary>Creates the set from resources already registered with the engine.</summary>
    /// <param name="resources">One resource per sound effect.</param>
    internal BrixInvadersSounds(IReadOnlyDictionary<SoundEffect, AudioResource> resources)
    {
        _resources = resources;
    }

    /// <summary>Gets every sound effect with its engine audio resource.</summary>
    public IReadOnlyDictionary<SoundEffect, AudioResource> Resources => _resources;

    /// <summary>Gets the engine audio resource of a sound effect.</summary>
    /// <param name="effect">The sound effect.</param>
    /// <returns>The resource, registered in the engine under <see cref="SoundEffects.KeyOf"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for a value that is not a defined sound effect.</exception>
    public AudioResource this[SoundEffect effect] =>
        _resources.TryGetValue(effect, out AudioResource resource)
            ? resource
            : throw new ArgumentOutOfRangeException(nameof(effect), effect, "Not a defined sound effect.");

    /// <summary>Gets the engine audio key of a sound effect (the same as <see cref="SoundEffects.KeyOf"/>).</summary>
    /// <param name="effect">The sound effect.</param>
    /// <returns>The key the resource is registered under.</returns>
    public static string KeyOf(SoundEffect effect) => SoundEffects.KeyOf(effect);
}
