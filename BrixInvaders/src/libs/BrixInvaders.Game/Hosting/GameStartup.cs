using System;
using System.IO;
using BrixInvaders.Game.Session;
using BrixInvaders.Game.Settings;

namespace BrixInvaders.Game.Hosting;

/// <summary>What the app does before its first window: open the settings store.</summary>
public static class GameStartup
{
    /// <summary>The folder (under the system temp folder) an autopilot run keeps its settings in.</summary>
    public const string AutoPilotSettingsFolderName = "BrixInvaders.AutoPilot";

    /// <summary>
    /// Opens the settings store (once; later calls do nothing): the player's own per-user store, or - when the
    /// autopilot is on - a scratch store under the system temp folder, so robot games never reach the player's high
    /// scores or preferences.
    /// </summary>
    /// <returns>The folder the store is in.</returns>
    public static string OpenSettingsStore()
    {
        if (!SettingsService.IsInitialized)
        {
            if (AutoPilot.IsRequested())
            {
                var folder = Path.Combine(Path.GetTempPath(), AutoPilotSettingsFolderName);
                Directory.CreateDirectory(folder);
                SettingsService.Initialize(folder);
            }
            else
            {
                SettingsService.Initialize();
            }
        }

        return SettingsService.DirectoryPath ?? string.Empty;
    }
}
