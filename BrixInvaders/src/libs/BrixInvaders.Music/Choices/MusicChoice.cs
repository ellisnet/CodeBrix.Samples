using System;

namespace BrixInvaders.Music;

/// <summary>
/// One option the settings screen offers: the exact registry name the music packages know it by, and the words the
/// player sees.
/// </summary>
public sealed class MusicChoice
{
    /// <summary>Creates a choice.</summary>
    /// <param name="name">The exact registry name (a generator name or an instrument library name).</param>
    /// <param name="label">The short label the settings screen shows.</param>
    /// <param name="description">One sentence saying what the player will hear.</param>
    /// <exception cref="ArgumentException">A value is null or blank.</exception>
    public MusicChoice(string name, string label, string description)
    {
        if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("A music choice needs a name.", nameof(name)); }
        if (string.IsNullOrWhiteSpace(label)) { throw new ArgumentException("A music choice needs a label.", nameof(label)); }
        if (string.IsNullOrWhiteSpace(description)) { throw new ArgumentException("A music choice needs a description.", nameof(description)); }

        Name = name;
        Label = label;
        Description = description;
    }

    /// <summary>The exact registry name, e.g. "SkyTNT" or "FluidR3Gm" - the value stored in the settings.</summary>
    public string Name { get; }

    /// <summary>The short label the settings screen shows, e.g. "SkyTNT - electronica".</summary>
    public string Label { get; }

    /// <summary>One sentence saying what the player will hear.</summary>
    public string Description { get; }

    /// <inheritdoc />
    public override string ToString() => Label;
}
