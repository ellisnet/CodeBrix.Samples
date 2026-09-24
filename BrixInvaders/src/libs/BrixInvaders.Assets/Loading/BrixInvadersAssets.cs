using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.Assets.Providers;
using CodeBrix.Platform.GameEngine.Audio;
using CodeBrix.Platform.GameEngine.Drawing;
using CodeBrix.Platform.GameEngine.Drawing.Tilesheets;
using CodeBrix.Platform.GameEngine.KenneyAssets;
using CodeBrix.Platform.GameEngine.Rendering.Text;
using Microsoft.Extensions.Logging;

namespace BrixInvaders.Assets;

/// <summary>
/// The one-call Kenney registration and the typed loaders the game uses. Everything goes through the engine's own
/// asset provider registry (<c>Engine.Managers.AssetProviders</c>) and hands back engine types.
/// </summary>
/// <remarks>
/// Call <see cref="Register(Engine)"/> once, after the engine is initialized and before anything is loaded (a game
/// host's <c>LoadAssets</c> override). Loading is load-time work: call the loaders while a screen is being built,
/// never per frame. Every loader is idempotent - the engine's registries keep the first materialization of a key.
/// </remarks>
public static class BrixInvadersAssets
{
    /// <summary>The prefix of every log line this library writes.</summary>
    public const string LogPrefix = "[BrixInvaders] assets:";

    /// <summary>The tilesheet registry key the promo image is registered under.</summary>
    public const string PromoCardKey = "promo/Kenney_asset_bundle";

    /// <summary>
    /// Registers the five Kenney zips beside the executable (<see cref="KenneyPacks.DefaultFolder"/>) with the engine
    /// in ONE <c>UseKenneyAssets</c> call, and logs one line per pack plus the provider's warnings.
    /// </summary>
    /// <param name="engine">The initialized engine.</param>
    /// <returns>The Kenney asset provider now serving the <c>kenney:</c> keys.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the folder is missing or no pack could be read from it.</exception>
    public static KenneyGameAssetProvider Register(Engine engine) => Register(engine, KenneyPacks.DefaultFolder);

    /// <summary>
    /// Registers the five Kenney zips found in <paramref name="folder"/> with the engine in ONE
    /// <c>UseKenneyAssets</c> call, and logs one line per pack plus the provider's warnings.
    /// </summary>
    /// <param name="engine">The initialized engine.</param>
    /// <param name="folder">The folder holding the zips.</param>
    /// <param name="log">
    /// An extra receiver for the log lines (for example a test's capture); every line also goes to
    /// <see cref="Debug.WriteLine(string)"/> and the engine logger.
    /// </param>
    /// <returns>The Kenney asset provider now serving the <c>kenney:</c> keys.</returns>
    /// <remarks>
    /// Registering again is harmless: zips the provider already holds are not added a second time (which would
    /// otherwise give them <c>-2</c> slugs and a second set of keys).
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> or <paramref name="folder"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the folder is missing, holds none of the five zips, or no pack could be read from it. A folder
    /// without any of the zips is refused before the provider is touched.
    /// </exception>
    public static KenneyGameAssetProvider Register(Engine engine, string folder, Action<string> log = null)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(folder);

        string root = Path.GetFullPath(folder);
        if (!Directory.Exists(root))
        {
            throw new InvalidOperationException(NoPacksMessage(root, "the folder does not exist"));
        }

        IReadOnlyList<string> zipPaths = KenneyPacks.ZipPaths(root);
        if (!zipPaths.Any(File.Exists))
        {
            throw new InvalidOperationException(NoPacksMessage(root, "none of the zips is there"));
        }

        KenneyGameAssetProvider existing = FindProvider(engine);
        int warningsBefore = existing?.Warnings.Count ?? 0;
        string[] toAdd = zipPaths
            .Where(path => existing is null || !existing.Packs.Any(pack => SamePath(pack.SourcePath, path)))
            .ToArray();

        KenneyGameAssetProvider provider = toAdd.Length > 0 || existing is null
            ? engine.UseKenneyAssets(toAdd)
            : existing;

        List<KenneyPackSummary> packs = provider.Packs
            .Where(pack => zipPaths.Any(path => SamePath(pack.SourcePath, path)))
            .ToList();

        if (packs.Count == 0)
        {
            throw new InvalidOperationException(NoPacksMessage(root, "none of the zips could be read"));
        }

        foreach (KenneyPackSummary pack in packs)
        {
            Write(
                $"{LogPrefix} pack {pack.Slug} ({pack.DisplayName}) - {pack.AssetCount} asset(s), " +
                $"{pack.MaterializableAssetCount} materializable, from {Path.GetFileName(pack.SourcePath)}",
                log);
        }

        foreach (string missing in zipPaths.Where(path => !packs.Any(pack => SamePath(pack.SourcePath, path))))
        {
            Write($"{LogPrefix} WARNING: {Path.GetFileName(missing)} could not be read; its assets are missing", log);
        }

        //The provider's Warnings only ever grow, so only what this registration added is reported here
        List<string> newWarnings = provider.Warnings.Skip(warningsBefore).ToList();
        if (newWarnings.Count == 0)
        {
            Write($"{LogPrefix} provider warnings: none", log);
        }

        foreach (string warning in newWarnings)
        {
            Write($"{LogPrefix} WARNING: {warning}", log);
        }

        return provider;
    }

    /// <summary>Loads the remastered pack's atlas: ships, enemies, lasers, effects, shields, meteors, power-ups, HUD.</summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <returns>The atlas tilesheet; address frames by name with <see cref="GetFrame"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    public static Tilesheet LoadMainAtlas(Engine engine) => LoadImage(engine, AssetKeys.Atlases.Main);

    /// <summary>
    /// Loads the tilesheet the player's ships, damage overlays, shields, lasers and life icons are cut from (the main
    /// atlas, <see cref="AssetKeys.Ships.Atlas"/>).
    /// </summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <returns>The atlas tilesheet.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    public static Tilesheet LoadShipSheet(Engine engine) => LoadImage(engine, AssetKeys.Ships.Atlas);

    /// <summary>
    /// Loads the tilesheet the formation enemies and UFOs are cut from (the same main atlas as the ships,
    /// <see cref="AssetKeys.Enemies.Atlas"/>).
    /// </summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <returns>The atlas tilesheet.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    public static Tilesheet LoadEnemySheet(Engine engine) => LoadImage(engine, AssetKeys.Enemies.Atlas);

    /// <summary>Loads the extension pack's atlas: boss cores, cannons, missile bays, missiles and smoke puffs.</summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <returns>The atlas tilesheet (<see cref="AssetKeys.Bosses.Atlas"/>).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    public static Tilesheet LoadBossSheet(Engine engine) => LoadImage(engine, AssetKeys.Bosses.Atlas);

    /// <summary>Gets one named frame of an atlas tilesheet.</summary>
    /// <param name="atlas">An atlas tilesheet from one of the loaders.</param>
    /// <param name="frameName">A frame name from <see cref="AssetKeys"/>, for example <c>AssetKeys.Enemies.Enemy(3, 2)</c>.</param>
    /// <returns>The frame (<c>atlas[frameName, 0, 0]</c>).</returns>
    /// <exception cref="ArgumentNullException">Thrown when an argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the atlas has no frame of that name.</exception>
    public static Frame GetFrame(Tilesheet atlas, string frameName)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(frameName);

        if (atlas.GetRegion(frameName) is null)
        {
            throw new ArgumentException($"The atlas '{atlas.Name}' has no frame named '{frameName}'.", nameof(frameName));
        }

        return atlas[frameName, 0, 0];
    }

    /// <summary>Gets the pixel size of one named frame of an atlas tilesheet.</summary>
    /// <param name="atlas">An atlas tilesheet from one of the loaders.</param>
    /// <param name="frameName">A frame name from <see cref="AssetKeys"/>.</param>
    /// <returns>The frame's width and height in pixels.</returns>
    /// <exception cref="ArgumentNullException">Thrown when an argument is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the atlas has no frame of that name.</exception>
    public static System.Drawing.Size GetFrameSize(Tilesheet atlas, string frameName)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(frameName);

        TilesheetRegion region = atlas.GetRegion(frameName)
            ?? throw new ArgumentException($"The atlas '{atlas.Name}' has no frame named '{frameName}'.", nameof(frameName));

        return region.TileSize;
    }

    /// <summary>
    /// Registers every <see cref="SoundEffect"/> with the engine's audio registry through <c>LoadAudio</c>, under the
    /// key <see cref="SoundEffects.KeyOf"/> gives (which is the Kenney asset key).
    /// </summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <returns>Every sound effect with its engine audio resource.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    public static BrixInvadersSounds LoadSounds(Engine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);

        Dictionary<SoundEffect, AudioResource> resources = [];
        foreach (KeyValuePair<SoundEffect, string> pair in SoundEffects.Keys)
        {
            resources[pair.Key] = engine.Managers.AssetProviders.LoadAudio(pair.Value);
        }

        return new BrixInvadersSounds(resources);
    }

    /// <summary>Registers the two Kenney text fonts with the engine's font manager.</summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <returns>The font keys and family names.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    public static BrixInvadersFonts LoadFonts(Engine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);

        engine.Managers.AssetProviders.LoadFont(AssetKeys.Fonts.Future);
        engine.Managers.AssetProviders.LoadFont(AssetKeys.Fonts.FutureThin);
        FontManager fonts = engine.Managers.Fonts;

        return new BrixInvadersFonts(
            AssetKeys.Fonts.Future,
            AssetKeys.Fonts.FutureThin,
            fonts.GetFamilyName(AssetKeys.Fonts.Future),
            fonts.GetFamilyName(AssetKeys.Fonts.FutureThin));
    }

    /// <summary>Loads one of the ten planets (a loose image: draw <c>sheet[0, 0]</c>).</summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <param name="index">The planet, 0..9.</param>
    /// <returns>The planet tilesheet.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    public static Tilesheet LoadPlanet(Engine engine, int index) =>
        LoadImage(engine, AssetKeys.Planets.All[AssetKeyChecks.Index(index, AssetKeys.Planets.All.Length, nameof(index))]);

    /// <summary>Loads one of the four tiling space backgrounds (a loose 256 x 256 image: draw <c>sheet[0, 0]</c>).</summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <param name="background">The background.</param>
    /// <returns>The background tilesheet.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for a value that is not a defined background.</exception>
    public static Tilesheet LoadBackground(Engine engine, SpaceBackground background) =>
        LoadImage(engine, AssetKeys.Backgrounds.All[AssetKeyChecks.Index((int)background, AssetKeys.Backgrounds.All.Length, nameof(background))]);

    /// <summary>Loads any image, atlas or vector by its <see cref="AssetKeys"/> key (for example a pack preview).</summary>
    /// <param name="engine">The engine the packs are registered with.</param>
    /// <param name="key">The asset key.</param>
    /// <returns>The tilesheet, registered in the engine under the key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when an argument is null.</exception>
    public static Tilesheet LoadImage(Engine engine, string key)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(key);

        return engine.Managers.AssetProviders.LoadTilesheet(key);
    }

    /// <summary>
    /// Loads <see cref="KenneyPacks.PromoCardFile"/> from beside the executable as a plain tilesheet (draw
    /// <c>sheet[0, 0]</c>), registered as <see cref="PromoCardKey"/>.
    /// </summary>
    /// <param name="engine">The engine.</param>
    /// <returns>The promo image tilesheet.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the image is not there.</exception>
    public static Tilesheet LoadPromoCard(Engine engine) => LoadPromoCard(engine, KenneyPacks.DefaultFolder);

    /// <summary>
    /// Loads <see cref="KenneyPacks.PromoCardFile"/> from <paramref name="folder"/> as a plain tilesheet (draw
    /// <c>sheet[0, 0]</c>), registered as <see cref="PromoCardKey"/>.
    /// </summary>
    /// <param name="engine">The engine.</param>
    /// <param name="folder">The folder holding the image (the zips' folder).</param>
    /// <returns>The promo image tilesheet.</returns>
    /// <exception cref="ArgumentNullException">Thrown when an argument is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the image is not there.</exception>
    public static Tilesheet LoadPromoCard(Engine engine, string folder)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(folder);

        //Kenney_asset_bundle.png is NOT a Kenney pack - it is Kenney's own marketing image for the whole bundle,
        //  with no licence file beside it - so it does not go through the Kenney asset provider. It is loaded as a
        //  plain picture straight from the file that sits beside the zips.
        if (TilesheetRegistry.Instance.TryGet(PromoCardKey, out Tilesheet existing) && existing is not null)
        {
            return existing;
        }

        string path = KenneyPacks.PromoCardPath(folder);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"The Kenney promo image was not found at '{path}'.", path);
        }

        Tilesheet sheet = TilesheetRegistry.Instance.LoadFromImageFile(PromoCardKey, path);

        //A sheet loaded straight from a file gets a default region with no tile size; make the whole picture its
        //  one tile, the way the Kenney provider does for a loose image, so sheet[0, 0] is the picture.
        TilesheetRegion whole = sheet.DefaultRegion;
        whole.TileSize = whole.Area.Size;

        return sheet;
    }

    /// <summary>Writes one log line to the debug output, the engine logger and the optional extra receiver.</summary>
    /// <param name="line">The line, prefix included.</param>
    /// <param name="log">The extra receiver, or <see langword="null"/>.</param>
    internal static void Write(string line, Action<string> log)
    {
        Debug.WriteLine(line);
        Engine.Logger.LogInformation("{Line}", line);
        log?.Invoke(line);
    }

    private static KenneyGameAssetProvider FindProvider(Engine engine) =>
        engine.Managers.AssetProviders.TryFind($"{KenneyPacks.ProviderId}:probe", out IGameAssetProvider found)
            ? found as KenneyGameAssetProvider
            : null;

    private static bool SamePath(string left, string right) =>
        string.Equals(Path.GetFullPath(left), Path.GetFullPath(right), StringComparison.Ordinal);

    private static string NoPacksMessage(string folder, string reason) =>
        $"No Kenney pack could be read from '{folder}' ({reason}). The game ships the five Kenney zips " +
        $"({string.Join(", ", KenneyPacks.FileNames)}) in '{KenneyPacks.RelativeFolder}' beside the executable; " +
        "check that they are there.";
}
