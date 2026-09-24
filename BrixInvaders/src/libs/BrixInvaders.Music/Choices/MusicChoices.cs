using System;
using System.Collections.Generic;
using System.Linq;
using CodeBrix.Audio.ModestSynth;
using CodeBrix.Audio.MusicGeneration.MuPT;
using CodeBrix.Audio.MusicGeneration.SkyTNT;
using CodeBrix.Audio.Samples.FluidR3Gm;

namespace BrixInvaders.Music;

/// <summary>
/// The two music choices the player makes on the settings screen - which model writes the music and which
/// instrument library plays it - as the exact names the music packages register them under.
/// </summary>
public static class MusicChoices
{
    /// <summary>The SkyTNT model's registry name (electronica, with drums).</summary>
    public const string SkyTNT = SkyTNTModel.GeneratorName;

    /// <summary>The MuPT model's registry name (folk and classical tunes in parts, no drums).</summary>
    public const string MuPT = MuPTModel.GeneratorName;

    /// <summary>The synthesized General MIDI instrument library's registry name.</summary>
    public const string ModestSynthGm = GeneralMidiInstrumentLibrary.LibraryName;

    /// <summary>The recorded (SoundFont) General MIDI instrument library's registry name.</summary>
    public const string FluidR3Gm = FluidR3GmInstrumentLibrary.LibraryName;

    /// <summary>The generator a fresh install plays: <see cref="SkyTNT"/>.</summary>
    public const string DefaultGenerator = SkyTNT;

    /// <summary>The instrument library a fresh install plays through: <see cref="ModestSynthGm"/>.</summary>
    public const string DefaultInstrumentLibrary = ModestSynthGm;

    private static readonly MusicChoice[] _generators =
    [
        new MusicChoice(SkyTNT, "SkyTNT - electronica",
            "Electronic music with drums, synth bass, leads and pads."),
        new MusicChoice(MuPT, "MuPT - folk and classical",
            "Reels, jigs, waltzes and airs written in parts, with no drums."),
    ];

    private static readonly MusicChoice[] _instrumentLibraries =
    [
        new MusicChoice(ModestSynthGm, "ModestSynth - synthesized",
            "Synthesized instruments: small, quick to start, with a retro edge."),
        new MusicChoice(FluidR3Gm, "FluidR3 GM - recorded",
            "Recorded instruments from a General MIDI SoundFont: fuller and more natural."),
    ];

    /// <summary>The generators the player can choose between, in settings-screen order.</summary>
    public static IReadOnlyList<MusicChoice> Generators { get; } = Array.AsReadOnly(_generators);

    /// <summary>The instrument libraries the player can choose between, in settings-screen order.</summary>
    public static IReadOnlyList<MusicChoice> InstrumentLibraries { get; } = Array.AsReadOnly(_instrumentLibraries);

    /// <summary>The generator registry names, in settings-screen order: SkyTNT, MuPT.</summary>
    public static IReadOnlyList<string> GeneratorNames { get; } = Array.AsReadOnly(_generators.Select(c => c.Name).ToArray());

    /// <summary>The instrument library registry names, in settings-screen order: ModestSynthGm, FluidR3Gm.</summary>
    public static IReadOnlyList<string> InstrumentLibraryNames { get; } =
        Array.AsReadOnly(_instrumentLibraries.Select(c => c.Name).ToArray());

    /// <summary>
    /// Returns the exact registry name of a generator. Names match without regard to case; null or blank means
    /// <see cref="DefaultGenerator"/> (a setting that was never saved).
    /// </summary>
    /// <param name="name">The stored or typed name.</param>
    /// <returns>The canonical generator name.</returns>
    /// <exception cref="ArgumentException">The name is no generator the game offers; the message lists the valid names.</exception>
    public static string ResolveGenerator(string name) => Resolve(_generators, name, DefaultGenerator, "music generator");

    /// <summary>
    /// Returns the exact registry name of an instrument library. Names match without regard to case; null or blank
    /// means <see cref="DefaultInstrumentLibrary"/> (a setting that was never saved).
    /// </summary>
    /// <param name="name">The stored or typed name.</param>
    /// <returns>The canonical instrument library name.</returns>
    /// <exception cref="ArgumentException">The name is no instrument library the game offers; the message lists the valid names.</exception>
    public static string ResolveInstrumentLibrary(string name) =>
        Resolve(_instrumentLibraries, name, DefaultInstrumentLibrary, "instrument library");

    /// <summary>Whether a name is a generator the game offers (without regard to case).</summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True for SkyTNT and MuPT.</returns>
    public static bool IsGenerator(string name) => Find(_generators, name) != null;

    /// <summary>Whether a name is an instrument library the game offers (without regard to case).</summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True for ModestSynthGm and FluidR3Gm.</returns>
    public static bool IsInstrumentLibrary(string name) => Find(_instrumentLibraries, name) != null;

    /// <summary>The choice for a generator name (null or blank = the default).</summary>
    /// <param name="name">The generator name.</param>
    /// <returns>The matching choice, with its label and description.</returns>
    /// <exception cref="ArgumentException">The name is no generator the game offers.</exception>
    public static MusicChoice GeneratorChoice(string name) => Find(_generators, ResolveGenerator(name));

    /// <summary>The choice for an instrument library name (null or blank = the default).</summary>
    /// <param name="name">The instrument library name.</param>
    /// <returns>The matching choice, with its label and description.</returns>
    /// <exception cref="ArgumentException">The name is no instrument library the game offers.</exception>
    public static MusicChoice InstrumentLibraryChoice(string name) => Find(_instrumentLibraries, ResolveInstrumentLibrary(name));

    /// <summary>
    /// The generator the settings screen moves to on Left (-1) or Right (+1), wrapping at both ends.
    /// </summary>
    /// <param name="current">The current generator name (null or blank = the default).</param>
    /// <param name="direction">How many places to move; negative moves left.</param>
    /// <returns>The canonical name of the generator moved to.</returns>
    /// <exception cref="ArgumentException">The current name is no generator the game offers.</exception>
    public static string NextGenerator(string current, int direction) =>
        Cycle(_generators, ResolveGenerator(current), direction);

    /// <summary>
    /// The instrument library the settings screen moves to on Left (-1) or Right (+1), wrapping at both ends.
    /// </summary>
    /// <param name="current">The current instrument library name (null or blank = the default).</param>
    /// <param name="direction">How many places to move; negative moves left.</param>
    /// <returns>The canonical name of the instrument library moved to.</returns>
    /// <exception cref="ArgumentException">The current name is no instrument library the game offers.</exception>
    public static string NextInstrumentLibrary(string current, int direction) =>
        Cycle(_instrumentLibraries, ResolveInstrumentLibrary(current), direction);

    private static string Resolve(MusicChoice[] choices, string name, string fallback, string what)
    {
        if (string.IsNullOrWhiteSpace(name)) { return fallback; }

        var choice = Find(choices, name);
        if (choice == null)
        {
            throw new ArgumentException(
                $"'{name}' is not a {what} BrixInvaders offers. Valid names: {string.Join(", ", choices.Select(c => c.Name))}.",
                nameof(name));
        }

        return choice.Name;
    }

    private static MusicChoice Find(MusicChoice[] choices, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) { return null; }

        return choices.FirstOrDefault(c => string.Equals(c.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static string Cycle(MusicChoice[] choices, string canonicalName, int direction)
    {
        var index = Array.FindIndex(choices, c => c.Name == canonicalName);
        var next = ((index + direction) % choices.Length + choices.Length) % choices.Length;
        return choices[next].Name;
    }
}
