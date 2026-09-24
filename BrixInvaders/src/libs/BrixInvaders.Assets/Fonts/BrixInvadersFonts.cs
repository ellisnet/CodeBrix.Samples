namespace BrixInvaders.Assets;

/// <summary>
/// The two Kenney text fonts, registered in the engine's font manager. Returned by
/// <see cref="BrixInvadersAssets.LoadFonts"/>.
/// </summary>
/// <remarks>
/// This library hands out KEYS, not typefaces: the game fetches the typeface with
/// <c>FontManager.Instance.Get(fonts.TextKey)</c> where it builds its text elements. Identify a font by its key or by
/// the family name recorded here (read from the font file by the engine), never by the platform's own family name.
/// </remarks>
/// <param name="TextKey">The font manager key of Kenvector Future (<see cref="AssetKeys.Fonts.Future"/>).</param>
/// <param name="ThinKey">The font manager key of Kenvector Future Thin (<see cref="AssetKeys.Fonts.FutureThin"/>).</param>
/// <param name="TextFamilyName">The family name of Kenvector Future, as the font file states it.</param>
/// <param name="ThinFamilyName">The family name of Kenvector Future Thin, as the font file states it.</param>
public sealed record BrixInvadersFonts(string TextKey, string ThinKey, string TextFamilyName, string ThinFamilyName);
