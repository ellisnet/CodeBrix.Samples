using System.Collections.Generic;
using BrixInvaders.Music;
using CodeBrix.Platform.GameEngine.GeneratedMusic;

namespace BrixInvaders.Game.Credits;

/// <summary>
/// The music card on the credits screen: which model writes the music and which instrument library plays it (from
/// what the provider reports is REALLY playing), and the two package families that do it, named in words.
/// </summary>
public static class MusicCreditsCard
{
    /// <summary>The line naming the family that composes the music.</summary>
    public const string ComposedByLine = "Composed while you play by the CodeBrix.Audio.MusicGeneration packages";

    /// <summary>The line naming the family that plays it in the game.</summary>
    public const string PlayedThroughLine = "Played through the CodeBrix.Platform.GameEngine packages";

    /// <summary>The card's first line for a model that is playing.</summary>
    /// <param name="generatorName">The model.</param>
    /// <param name="instrumentLibraryName">The instrument library.</param>
    /// <returns>"Music written live by &lt;model&gt; through &lt;instrument library&gt;".</returns>
    public static string WrittenLiveLine(string generatorName, string instrumentLibraryName) =>
        $"Music written live by {generatorName} through {instrumentLibraryName}";

    /// <summary>Builds the card.</summary>
    /// <param name="source">What is really playing; null while the music is still starting.</param>
    /// <param name="chosen">The player's choices (used while nothing plays yet); null for the defaults.</param>
    /// <returns>The card's lines.</returns>
    public static IReadOnlyList<CreditsLine> Lines(GeneratedMusicSourceInfo source, MusicSettings chosen)
    {
        string first;
        if (source == null)
        {
            var settings = chosen ?? new MusicSettings();
            first = WrittenLiveLine(MusicChoices.ResolveGenerator(settings.GeneratorName),
                MusicChoices.ResolveInstrumentLibrary(settings.InstrumentLibraryName)) + " (the model is warming up)";
        }
        else if (source.IsReplay)
        {
            first = $"Music: a recorded piece (no music model was found) through {source.InstrumentLibraryName}";
        }
        else
        {
            first = WrittenLiveLine(source.GeneratorName, source.InstrumentLibraryName);
        }

        return new List<CreditsLine>
        {
            CreditsLine.Body(first),
            CreditsLine.Body(ComposedByLine),
            CreditsLine.Body(PlayedThroughLine),
        };
    }
}
