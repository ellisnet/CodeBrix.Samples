using System;
using System.Collections.Generic;
using System.Linq;
using BrixInvaders.Assets;

namespace BrixInvaders.Game.Credits;

/// <summary>
/// The default <see cref="ICreditsContent"/>: the game, every Kenney pack it reads (credit line per pack), the Kenney
/// links, and a music line supplied by the caller.
/// </summary>
public sealed class KenneyCreditsContent : ICreditsContent
{
    /// <summary>Kenney's site.</summary>
    public const string KenneySiteUrl = "https://kenney.nl";

    /// <summary>Kenney's Patreon page.</summary>
    public const string PatreonUrl = "https://www.patreon.com/kenney/";

    private readonly Func<IEnumerable<string>> _packCredits;
    private readonly Func<IEnumerable<CreditsLine>> _musicLines;

    /// <summary>Creates the content.</summary>
    /// <param name="packCredits">The credit line of every pack read (see <see cref="KenneyPacks.PackCreditLine"/>).</param>
    /// <param name="musicLines">The music card lines; null for none.</param>
    public KenneyCreditsContent(Func<IEnumerable<string>> packCredits, Func<IEnumerable<CreditsLine>> musicLines = null)
    {
        _packCredits = packCredits ?? throw new ArgumentNullException(nameof(packCredits));
        _musicLines = musicLines;
    }

    /// <inheritdoc />
    public IReadOnlyList<CreditsLine> GetLines()
    {
        var lines = new List<CreditsLine>
        {
            CreditsLine.Heading("BRIXINVADERS"),
            CreditsLine.Body("A CodeBrix.Platform game engine showcase"),
            CreditsLine.Spacer,
            CreditsLine.Heading("ART AND SOUND"),
            CreditsLine.Body(KenneyPacks.CreditLine),
        };

        lines.AddRange(_packCredits().Select(CreditsLine.Body));
        lines.Add(CreditsLine.Link("Kenney - kenney.nl", KenneySiteUrl));
        lines.Add(CreditsLine.Link("Support Kenney on Patreon", PatreonUrl));
        lines.Add(CreditsLine.Link("Get the bundle: kenney.itch.io/kenney-game-assets", KenneyPacks.BundleUrl));

        var music = _musicLines?.Invoke()?.ToList();
        if (music is { Count: > 0 })
        {
            lines.Add(CreditsLine.Spacer);
            lines.Add(CreditsLine.Heading("MUSIC"));
            lines.AddRange(music);
        }

        return lines;
    }
}
