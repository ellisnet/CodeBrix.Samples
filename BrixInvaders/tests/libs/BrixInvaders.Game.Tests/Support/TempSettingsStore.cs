using System;
using System.IO;
using BrixInvaders.Game.Settings;

namespace BrixInvaders.Game.Tests.Support;

/// <summary>Opens the settings store in a fresh temporary folder; disposing shuts it and deletes the folder.</summary>
internal sealed class TempSettingsStore : IDisposable
{
    public TempSettingsStore()
    {
        Folder = Path.Combine(Path.GetTempPath(), "BrixInvaders.Game.Tests", Guid.NewGuid().ToString("N"));
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
