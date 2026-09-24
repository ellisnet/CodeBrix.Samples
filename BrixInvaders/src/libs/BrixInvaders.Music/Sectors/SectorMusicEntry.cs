using System;
using System.Linq;
using CodeBrix.Audio.MusicGeneration.Presets;

namespace BrixInvaders.Music;

/// <summary>
/// The music of one sector design (or of the title screen): a preset per generator, a tempo, and the boss
/// follow-up that takes over when the boss arrives.
/// </summary>
public sealed class SectorMusicEntry
{
    internal SectorMusicEntry(
        int design,
        string name,
        string mood,
        string skyTntPreset,
        string muptPreset,
        double beatsPerMinute,
        string bossSkyTntPreset,
        string bossMuptPreset,
        double bossBeatsPerMinute)
    {
        if (design < 0) { throw new ArgumentOutOfRangeException(nameof(design), design, "A design is 0 (the title) or more."); }
        if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("An entry needs a name.", nameof(name)); }
        if (string.IsNullOrWhiteSpace(mood)) { throw new ArgumentException("An entry needs a mood note.", nameof(mood)); }
        CheckTempo(beatsPerMinute, nameof(beatsPerMinute));
        CheckTempo(bossBeatsPerMinute, nameof(bossBeatsPerMinute));

        Design = design;
        Name = name;
        Mood = mood;
        SkyTNTPreset = CheckPreset(MusicChoices.SkyTNT, skyTntPreset, nameof(skyTntPreset));
        MuPTPreset = CheckPreset(MusicChoices.MuPT, muptPreset, nameof(muptPreset));
        BeatsPerMinute = beatsPerMinute;
        BossSkyTNTPreset = CheckPreset(MusicChoices.SkyTNT, bossSkyTntPreset, nameof(bossSkyTntPreset));
        BossMuPTPreset = CheckPreset(MusicChoices.MuPT, bossMuptPreset, nameof(bossMuptPreset));
        BossBeatsPerMinute = bossBeatsPerMinute;
    }

    /// <summary>The sector design, 1..5; 0 for the title screen.</summary>
    public int Design { get; }

    /// <summary>The sector's name ("Outer Picket") or "Title".</summary>
    public string Name { get; }

    /// <summary>What the music is meant to feel like here, in a few words.</summary>
    public string Mood { get; }

    /// <summary>The SkyTNT preset played in this sector.</summary>
    public string SkyTNTPreset { get; }

    /// <summary>The MuPT preset played in this sector.</summary>
    public string MuPTPreset { get; }

    /// <summary>The session tempo when the music session STARTS in this sector.</summary>
    public double BeatsPerMinute { get; }

    /// <summary>The SkyTNT preset the boss follow-up switches to.</summary>
    public string BossSkyTNTPreset { get; }

    /// <summary>The MuPT preset the boss follow-up switches to.</summary>
    public string BossMuPTPreset { get; }

    /// <summary>The session tempo when the music session STARTS during this sector's boss fight (faster).</summary>
    public double BossBeatsPerMinute { get; }

    /// <summary>The preset for a generator, for the sector itself or for its boss.</summary>
    /// <param name="generatorName">SkyTNT or MuPT (without regard to case; null or blank = the default generator).</param>
    /// <param name="boss">True for the boss follow-up.</param>
    /// <returns>A preset name of the generator's family.</returns>
    /// <exception cref="ArgumentException">The generator is no generator the game offers; the message lists the valid names.</exception>
    public string PresetFor(string generatorName, bool boss = false)
    {
        var generator = MusicChoices.ResolveGenerator(generatorName);
        if (generator == MusicChoices.SkyTNT) { return boss ? BossSkyTNTPreset : SkyTNTPreset; }
        return boss ? BossMuPTPreset : MuPTPreset;
    }

    /// <summary>The tempo for the sector itself or for its boss.</summary>
    /// <param name="boss">True for the boss.</param>
    /// <returns>Beats per minute.</returns>
    public double BeatsPerMinuteFor(bool boss = false) => boss ? BossBeatsPerMinute : BeatsPerMinute;

    /// <inheritdoc />
    public override string ToString() =>
        $"{Design} {Name}: SkyTNT {SkyTNTPreset} / MuPT {MuPTPreset} at {BeatsPerMinute} bpm " +
        $"(boss SkyTNT {BossSkyTNTPreset} / MuPT {BossMuPTPreset} at {BossBeatsPerMinute} bpm)";

    /// <summary>
    /// Checks that a preset belongs to the generator's family and returns its exact name.
    /// </summary>
    /// <param name="generatorName">SkyTNT or MuPT.</param>
    /// <param name="presetName">The preset name, matched without regard to case.</param>
    /// <param name="parameterName">The parameter name for the exception.</param>
    /// <returns>The preset's exact name.</returns>
    /// <exception cref="ArgumentException">The preset is not one of that generator's presets; the message lists them.</exception>
    internal static string CheckPreset(string generatorName, string presetName, string parameterName)
    {
        var generator = MusicChoices.ResolveGenerator(generatorName);
        var presets = generator == MusicChoices.SkyTNT ? SkyTNTPresets.All : MuPTPresets.All;
        var preset = string.IsNullOrWhiteSpace(presetName)
            ? null
            : presets.FirstOrDefault(p => string.Equals(p.Name, presetName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (preset == null)
        {
            throw new ArgumentException(
                $"'{presetName}' is not a {generator} preset. Valid presets: {string.Join(", ", presets.Select(p => p.Name))}.",
                parameterName);
        }

        return preset.Name;
    }

    private static void CheckTempo(double beatsPerMinute, string parameterName)
    {
        if (!(beatsPerMinute >= SectorMusic.MinimumBeatsPerMinute && beatsPerMinute <= SectorMusic.MaximumBeatsPerMinute))
        {
            throw new ArgumentOutOfRangeException(parameterName, beatsPerMinute,
                $"A tempo must be {SectorMusic.MinimumBeatsPerMinute} to {SectorMusic.MaximumBeatsPerMinute} beats per minute.");
        }
    }
}
