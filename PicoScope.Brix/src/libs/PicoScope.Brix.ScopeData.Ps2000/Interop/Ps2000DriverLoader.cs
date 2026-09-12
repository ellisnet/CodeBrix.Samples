using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PicoScope.Brix.ScopeData.Ps2000.Interop;

/// <summary>
/// Finds and loads the ps2000 driver library at run time, on whichever
/// operating system the process is running.
/// </summary>
/// <remarks>
/// <para>
/// The P/Invokes in <see cref="Ps2000Api"/> name the library by its bare name,
/// <see cref="LibraryName"/>, so the runtime probes for <c>ps2000.dll</c> on
/// Windows and <c>libps2000.so</c> on Linux. Where that default probe finds the
/// driver, this class has nothing to do. Where it does not, the resolver
/// registered here looks in the places the driver is actually installed.
/// </para>
/// <para>
/// <b>Windows.</b> The driver is usually <b>not</b> anywhere the default probe
/// looks. Installing PicoSDK does not reliably put <c>ps2000.dll</c> in
/// <c>Program Files\Pico Technology\SDK\lib</c> -- on a machine with a current
/// PicoScope 7 install, that folder may contain only <c>psospa.dll</c>, while
/// the legacy drivers live inside the PicoScope application's own folder, which
/// is not on <c>PATH</c>. A bare P/Invoke then fails with
/// <see cref="DllNotFoundException"/> even though the driver is installed and
/// the desktop application is using it happily. The resolver searches the SDK
/// folders and every <c>PicoScope*</c> application folder, and loads the driver
/// by absolute path so that its own dependency (<c>picoipp.dll</c>) resolves
/// from beside it.
/// </para>
/// <para>
/// <b>Linux.</b> The <c>libps2000</c> package installs the driver under
/// <c>/opt/picoscope/lib</c> and adds that folder to the system loader
/// configuration, so the default probe normally succeeds on its own. The folder
/// is still searched explicitly, for a machine where the loader configuration
/// was not applied. The device itself needs no special permission: the
/// package's udev rule opens the USB device to every user.
/// </para>
/// <para>
/// <see cref="NativeLibrary.SetDllImportResolver"/> is available on modern .NET
/// but not on .NET Framework -- so the supposedly "unsupported on .NET Core"
/// legacy driver is in fact easier to load there.
/// </para>
/// <para>
/// <b>Bitness.</b> The driver is 64-bit on every platform Pico ships it for,
/// so the host process must be 64-bit too. An x86 build fails with
/// <see cref="BadImageFormatException"/>.
/// </para>
/// </remarks>
public static class Ps2000DriverLoader
{
    /// <summary>
    /// The library name the P/Invokes declare: bare, with no prefix or
    /// extension, so the runtime probes for the platform's own file name.
    /// </summary>
    public const string LibraryName = "ps2000";

    private static readonly object SyncRoot = new object();
    private static bool _registered;

    /// <summary>
    /// The driver's file name on the current operating system:
    /// <c>ps2000.dll</c> on Windows, <c>libps2000.so</c> on Linux,
    /// <c>libps2000.dylib</c> on macOS.
    /// </summary>
    public static string DriverFileName =>
        OperatingSystem.IsWindows() ? "ps2000.dll"
        : OperatingSystem.IsMacOS() ? "libps2000.dylib"
        : "libps2000.so";

    /// <summary>
    /// Additional directories to search before the built-in ones. Add to this
    /// before the first driver call if the driver lives somewhere unusual.
    /// </summary>
    public static IList<string> AdditionalSearchPaths { get; } = new List<string>();

    /// <summary>
    /// The full path of the driver that the resolver loaded, or an empty string
    /// when the resolver has not loaded it -- either because it has not been
    /// needed yet, or because the runtime's default probe found the driver
    /// without help (the usual case on Linux).
    /// </summary>
    public static string LoadedFrom { get; private set; } = string.Empty;

    /// <summary>
    /// Registers the resolver. Safe to call repeatedly; only the first call does
    /// anything.
    /// </summary>
    /// <remarks>
    /// Every entry point in <see cref="Ps2000Api"/> calls this first, so callers do
    /// not normally need to.
    /// </remarks>
    public static void EnsureRegistered()
    {
        if (_registered) { return; }

        lock (SyncRoot)
        {
            if (_registered) { return; }

            NativeLibrary.SetDllImportResolver(typeof(Ps2000DriverLoader).Assembly, Resolve);
            _registered = true;
        }
    }

    /// <summary>
    /// Returns every directory that will be searched for the driver, in order.
    /// </summary>
    /// <returns>The candidate directories.</returns>
    /// <remarks>
    /// Useful in a diagnostic message when the driver cannot be found, so a user
    /// can see where it was looked for. The runtime's own default probe (the
    /// application folder, <c>PATH</c>, the system loader path) runs after
    /// these and is not listed.
    /// </remarks>
    public static IReadOnlyList<string> GetSearchPaths()
    {
        var paths = new List<string>(AdditionalSearchPaths);

        if (OperatingSystem.IsWindows())
        {
            AddWindowsSearchPaths(paths);
        }
        else if (OperatingSystem.IsLinux())
        {
            //Where the libps2000 package installs the driver.
            paths.Add("/opt/picoscope/lib");
        }

        return paths;
    }

    private static void AddWindowsSearchPaths(List<string> paths)
    {
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        //The documented SDK location comes first, because that is where a
        //  developer who installed PicoSDK will expect it to be.
        paths.Add(Path.Combine(programFiles, "Pico Technology", "SDK", "lib"));
        paths.Add(Path.Combine(programFilesX86, "Pico Technology", "SDK", "lib"));

        //Then the PicoScope application folders, which is where the legacy
        //  drivers actually live on a current install. The channel suffix
        //  varies ("Stable", "Beta", "Early Access"), so enumerate.
        foreach (string root in new[] { programFiles, programFilesX86 })
        {
            string picoRoot = Path.Combine(root, "Pico Technology");
            if (!Directory.Exists(picoRoot)) { continue; }

            try
            {
                foreach (string dir in Directory.EnumerateDirectories(picoRoot, "PicoScope*"))
                {
                    paths.Add(dir);
                }
            }
            catch (IOException)
            {
                //An unreadable directory is not worth failing over.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, LibraryName, StringComparison.OrdinalIgnoreCase))
        {
            return IntPtr.Zero;
        }

        string fileName = DriverFileName;
        foreach (string directory in GetSearchPaths())
        {
            string candidate = Path.Combine(directory, fileName);
            if (!File.Exists(candidate)) { continue; }

            //Loading by absolute path also lets the OS resolve the driver's own
            //  dependencies (picoipp.dll on Windows) from beside it, which a
            //  plain name-based load would not do.
            if (NativeLibrary.TryLoad(candidate, out IntPtr handle))
            {
                LoadedFrom = candidate;
                return handle;
            }
        }

        //Fall through to the default probe, which covers the case where the
        //  driver has been copied next to the executable, put on PATH, or -- on
        //  Linux -- registered with the system loader by its package.
        return IntPtr.Zero;
    }
}
