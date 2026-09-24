namespace BrixInvaders.Music;

/// <summary>
/// The player's music settings, filled by the Game library from its settings facade and handed to
/// <see cref="MusicSetup.OptionsFor(MusicSettings, int, bool)"/>.
/// </summary>
public sealed class MusicSettings
{
    /// <summary>
    /// The generator (SkyTNT or MuPT, see <see cref="MusicChoices.GeneratorNames"/>); null or blank means
    /// <see cref="MusicChoices.DefaultGenerator"/>.
    /// </summary>
    public string GeneratorName { get; set; } = MusicChoices.DefaultGenerator;

    /// <summary>
    /// The instrument library (ModestSynthGm or FluidR3Gm, see <see cref="MusicChoices.InstrumentLibraryNames"/>);
    /// null or blank means <see cref="MusicChoices.DefaultInstrumentLibrary"/>.
    /// </summary>
    public string InstrumentLibraryName { get; set; } = MusicChoices.DefaultInstrumentLibrary;

    /// <summary>
    /// The music's own level, 0..1, passed on as the generated music's MasterVolume. It sits UNDER the engine's
    /// music bus: when the game already applies the player's music volume through the engine's mixer, leave this
    /// at 1 so the slider is not applied twice.
    /// </summary>
    public double MusicVolume { get; set; } = 1.0;

    /// <summary>A copy of these settings.</summary>
    /// <returns>An independent copy.</returns>
    public MusicSettings Clone() => new()
    {
        GeneratorName = GeneratorName,
        InstrumentLibraryName = InstrumentLibraryName,
        MusicVolume = MusicVolume,
    };
}
