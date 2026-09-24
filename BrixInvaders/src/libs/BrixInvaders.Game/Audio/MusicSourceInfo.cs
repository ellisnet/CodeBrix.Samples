namespace BrixInvaders.Game.Audio;

/// <summary>What the generated music is really playing: the generator and the instrument library it plays through.</summary>
/// <param name="GeneratorName">The generator (a model such as SkyTNT or MuPT, or the embedded replay).</param>
/// <param name="InstrumentLibraryName">The instrument library (ModestSynthGm or FluidR3Gm).</param>
/// <param name="IsReplay">True when no model is playing: the music package's embedded replay is.</param>
public sealed record MusicSourceInfo(string GeneratorName, string InstrumentLibraryName, bool IsReplay);
