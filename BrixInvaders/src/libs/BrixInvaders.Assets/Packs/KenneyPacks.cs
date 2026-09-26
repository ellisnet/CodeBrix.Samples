using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeBrix.Platform.GameEngine.KenneyAssets;

namespace BrixInvaders.Assets;

/// <summary>
/// The five Kenney asset zips the game ships, where they live beside the executable, and the pack slugs the Kenney
/// asset provider gives them.
/// </summary>
/// <remarks>
/// The game ships the zips exactly as Kenney publishes them: nothing is extracted, renamed or repacked. The Core
/// project copies <c>assets/**</c> beside the executable, so the zips are found under
/// <c>&lt;AppContext.BaseDirectory&gt;/assets/kenney</c>. A pack slug is made by the provider from the pack's licence
/// title line (or from the zip's file name when the licence has no usable title); the constants below were read
/// from the provider at run time and are fenced by a test.
/// </remarks>
public static class KenneyPacks
{
    /// <summary>The ships, enemies, lasers, effects, meteors, power-ups, HUD pictures, backgrounds, fonts and bonus sounds.</summary>
    public const string SpaceShooterRemasteredFile = "kenney_space-shooter-remastered.zip";

    /// <summary>The boss ships, station pieces, parts, missiles and smoke effects.</summary>
    public const string SpaceShooterExtensionFile = "kenney_space-shooter-extension.zip";

    /// <summary>The ten 1280 px planets.</summary>
    public const string PlanetsFile = "kenney_planets.zip";

    /// <summary>The sci-fi sound effects (lasers, explosions, impacts, force fields, engines).</summary>
    public const string SciFiSoundsFile = "kenney_sci-fi-sounds.zip";

    /// <summary>The digital sound effects (menu tones, power-ups, phasers, zaps).</summary>
    public const string DigitalAudioFile = "kenney_digital-audio.zip";

    /// <summary>
    /// Kenney's marketing image for the Kenney game assets bundle. It sits beside the zips but is NOT a Kenney pack:
    /// it is loaded as a plain picture by <see cref="BrixInvadersAssets.LoadPromoCard(CodeBrix.Platform.GameEngine.Engine)"/>.
    /// </summary>
    public const string PromoCardFile = "Kenney_asset_bundle.png";

    /// <summary>The slug of the remastered pack (its licence title line is too long, so the file name is used).</summary>
    public const string SpaceShooterRemasteredSlug = "space-shooter-remastered";

    /// <summary>The slug of the extension pack.</summary>
    public const string SpaceShooterExtensionSlug = "space-shooter-extension";

    /// <summary>The slug of the planets pack.</summary>
    public const string PlanetsSlug = "planets";

    /// <summary>The slug of the sci-fi sounds pack.</summary>
    public const string SciFiSoundsSlug = "sci-fi-sounds";

    /// <summary>The slug of the digital audio pack.</summary>
    public const string DigitalAudioSlug = "digital-audio";

    /// <summary>The provider identifier, which is the <c>kenney:</c> prefix of every key.</summary>
    public const string ProviderId = KenneyGameAssetProvider.DefaultProviderId;

    /// <summary>The house-style credit line shown wherever Kenney content is on screen.</summary>
    public const string CreditLine = "Ships, sounds and planets by Kenney - kenney.nl - CC0";

    /// <summary>Where Kenney's full game assets bundle can be had (the Kenney card's link).</summary>
    public const string BundleUrl = "https://kenney.itch.io/kenney-game-assets";

    /// <summary>The folder, relative to the executable, that holds the zips and the promo image.</summary>
    public static readonly string RelativeFolder = Path.Combine("assets", "kenney");

    /// <summary>The five zip file names, in registration order.</summary>
    public static readonly IReadOnlyList<string> FileNames =
    [
        SpaceShooterRemasteredFile,
        SpaceShooterExtensionFile,
        PlanetsFile,
        SciFiSoundsFile,
        DigitalAudioFile,
    ];

    /// <summary>The five pack slugs, in the same order as <see cref="FileNames"/>.</summary>
    public static readonly IReadOnlyList<string> Slugs =
    [
        SpaceShooterRemasteredSlug,
        SpaceShooterExtensionSlug,
        PlanetsSlug,
        SciFiSoundsSlug,
        DigitalAudioSlug,
    ];

    /// <summary>Gets the folder beside the executable that holds the zips: <c>&lt;base directory&gt;/assets/kenney</c>.</summary>
    public static string DefaultFolder => Path.Combine(AppContext.BaseDirectory, RelativeFolder);

    /// <summary>Gets the full paths of the five zips in a folder.</summary>
    /// <param name="folder">The folder holding the zips; <see langword="null"/> means <see cref="DefaultFolder"/>.</param>
    /// <returns>The five full paths, in <see cref="FileNames"/> order (whether the files exist or not).</returns>
    public static IReadOnlyList<string> ZipPaths(string folder = null)
    {
        string root = Path.GetFullPath(folder ?? DefaultFolder);

        return FileNames.Select(fileName => Path.Combine(root, fileName)).ToList();
    }

    /// <summary>Gets the full path of the promo image in a folder.</summary>
    /// <param name="folder">The folder holding the image; <see langword="null"/> means <see cref="DefaultFolder"/>.</param>
    /// <returns>The full path of <see cref="PromoCardFile"/>.</returns>
    public static string PromoCardPath(string folder = null) =>
        Path.Combine(Path.GetFullPath(folder ?? DefaultFolder), PromoCardFile);
}
