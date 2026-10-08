using System.IO;
using GoddessTempleDiscovery.Game.Settings;

namespace GoddessTempleDiscovery.Game.Hosting;

/// <summary>What the app does before its first window: open the settings store.</summary>
public static class GameStartup
{
    /// <summary>The folder (under the system temp folder) an autoplay run keeps its settings in.</summary>
    public const string AutoPlaySettingsFolderName = "GoddessTempleDiscovery.AutoPlay";

    /// <summary>
    /// Opens the settings store (once; later calls do nothing): the player's own per-user store, or - when autoplay is
    /// on - a scratch store under the system temp folder, so robot games never reach the player's preferences or
    /// last setup.
    /// </summary>
    /// <returns>The folder the store is in.</returns>
    public static string OpenSettingsStore()
    {
        if (!SettingsService.IsInitialized)
        {
            if (AutoPlay.IsRequested())
            {
                SettingsService.Initialize(ScratchStoreFolder());
            }
            else
            {
                SettingsService.Initialize();
            }
        }

        return SettingsService.DirectoryPath ?? string.Empty;
    }

    /// <summary>The scratch store folder autoplay and tests use (created when missing).</summary>
    /// <returns>The folder.</returns>
    public static string ScratchStoreFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), AutoPlaySettingsFolderName);
        Directory.CreateDirectory(folder);
        return folder;
    }
}
