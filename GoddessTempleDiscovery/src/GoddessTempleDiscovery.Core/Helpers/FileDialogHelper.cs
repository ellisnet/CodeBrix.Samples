using System;
using System.IO;

namespace GoddessTempleDiscovery.Helpers;

/// <summary>Small helpers for the native "Save as" dialog (the same rules as InannaRosette's).</summary>
public static class FileDialogHelper
{
    /// <summary>
    /// Turns the path a picker hands back into a real file-system path: the Linux heads hand back a percent-encoded
    /// path (or a whole <c>file://</c> URI), which is decoded here; plain paths pass through.
    /// </summary>
    /// <param name="path">The picker's path.</param>
    /// <returns>The file-system path.</returns>
    public static string ToFileSystemPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase) && Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsFile)
        {
            return uri.LocalPath;
        }

        for (var i = 0; i + 2 < path.Length; i++)
        {
            if (path[i] == '%' && Uri.IsHexDigit(path[i + 1]) && Uri.IsHexDigit(path[i + 2]))
            {
                return Uri.UnescapeDataString(path);
            }
        }

        return path;
    }

    /// <summary>Removes the empty placeholder file some save pickers create, never a file with content.</summary>
    /// <param name="path">The chosen path.</param>
    public static void RemoveEmptyPlaceholder(string path)
    {
        try
        {
            var info = new FileInfo(path);
            if (info.Exists && info.Length == 0)
            {
                info.Delete();
            }
        }
        catch (Exception)
        {
            //Leave it; writing the journal replaces it
        }
    }
}
