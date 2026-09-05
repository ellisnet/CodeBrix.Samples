using GitHubIssueFinder.Settings;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace GitHubIssueFinder.Settings.Tests;

//The settings facade is process-global state, so it is pointed at a throwaway folder
//beside the test binary once per test assembly, before any test runs. The user's real
//settings store is never read or written by these tests.
internal static class TestSettingsStore
{
    //The name of the folder inside the test binary's own output that holds every run's store.
    internal const string RootFolderName = "test-settings";

    //The throwaway folder the store lives in for this run.
    internal static string DirectoryPath { get; private set; }

    [ModuleInitializer]
    internal static void Initialize()
    {
        var root = Path.Combine(AppContext.BaseDirectory, RootFolderName);

        //Clear what earlier runs left behind, so the build output does not grow a folder a run.
        try
        {
            if (Directory.Exists(root)) { Directory.Delete(root, recursive: true); }
        }
        catch (IOException)
        {
            //A file still held open by something else is no reason to fail the run.
        }
        catch (UnauthorizedAccessException)
        {
            //Same again.
        }

        DirectoryPath = Path.Combine(root, Guid.NewGuid().ToString("N"));

        if (SettingsService.IsInitialized) { return; }

        SettingsService.Initialize(DirectoryPath);
    }
}
