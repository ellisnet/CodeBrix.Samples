namespace GoddessTempleDiscovery.Rules.Cards;

/// <summary>An ancient word the cards use.</summary>
/// <param name="Word">The word as the cards print it.</param>
/// <param name="Language">Sumerian, Akkadian, Arabic, German or Greek.</param>
/// <param name="Meaning">What it means.</param>
/// <param name="Pronunciation">How to say it.</param>
/// <param name="Cuneiform">The cuneiform spelling, or empty.</param>
/// <param name="Sources">The sources.</param>
public sealed record GlossaryEntry(string Word, string Language, string Meaning, string Pronunciation, string Cuneiform, string Sources);
