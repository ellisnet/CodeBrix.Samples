using System;
using System.Collections.Generic;
using System.IO;
using CodeBrix.Platform.GameEngine;
using CodeBrix.Platform.GameEngine.KenneyAssets;

namespace BrixInvaders.Assets.Tests;

/// <summary>The game's Kenney zips copied beside the test executable, and the shared registration of them.</summary>
internal static class TestAssets
{
    private static readonly object _gate = new();

    /// <summary>Gets the folder the zips and the promo image are copied to (<c>assets/kenney</c> beside the test executable).</summary>
    public static string Folder => Path.Combine(AppContext.BaseDirectory, "assets", "kenney");

    /// <summary>Gets the engine every test uses.</summary>
    public static Engine Engine => Engine.Instance;

    /// <summary>Registers the zips once for the process (registering again is harmless) and returns the provider.</summary>
    /// <param name="log">An optional receiver for the log lines of this call.</param>
    /// <returns>The Kenney asset provider.</returns>
    public static KenneyGameAssetProvider Register(Action<string> log = null)
    {
        lock (_gate)
        {
            return BrixInvadersAssets.Register(Engine, Folder, log);
        }
    }

    /// <summary>Creates an empty scratch folder under the test output directory.</summary>
    /// <returns>The full path of the new folder.</returns>
    public static string CreateScratchFolder()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "test-temp", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    /// <summary>Deletes a scratch folder, ignoring one that is already gone.</summary>
    /// <param name="path">The folder.</param>
    public static void DeleteScratchFolder(string path)
    {
        if (Directory.Exists(path)) { Directory.Delete(path, recursive: true); }
    }

    /// <summary>Gets a list that collects log lines.</summary>
    /// <param name="lines">The list the lines land in.</param>
    /// <returns>The receiver.</returns>
    public static Action<string> Capture(List<string> lines) => line => lines.Add(line);
}
