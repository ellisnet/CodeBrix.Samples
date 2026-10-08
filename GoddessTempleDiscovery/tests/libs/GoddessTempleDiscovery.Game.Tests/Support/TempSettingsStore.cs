using System;
using System.IO;
using GoddessTempleDiscovery.Game.Settings;

namespace GoddessTempleDiscovery.Game.Tests.Support;

/// <summary>Opens the settings store in a fresh temporary folder; disposing shuts it and deletes the folder.</summary>
internal sealed class TempSettingsStore : IDisposable
{
    public TempSettingsStore()
    {
        SettingsService.Shutdown();
        Folder = Path.Combine(Path.GetTempPath(), "GoddessTempleDiscovery.Game.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Folder);
        SettingsService.Initialize(Folder);
    }

    public string Folder { get; }

    /// <summary>Closes and re-opens the store on the same folder (proves values were written to disk).</summary>
    public void Reopen()
    {
        SettingsService.Shutdown();
        SettingsService.Initialize(Folder);
    }

    public void Dispose()
    {
        SettingsService.Shutdown();
        if (Directory.Exists(Folder))
        {
            Directory.Delete(Folder, recursive: true);
        }
    }
}
