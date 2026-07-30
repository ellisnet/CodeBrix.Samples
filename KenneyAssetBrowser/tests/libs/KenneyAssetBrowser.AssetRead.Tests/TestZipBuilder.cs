using CodeBrix.Compression.Zip;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KenneyAssetBrowser.AssetRead.Tests;

/// <summary>
/// Builds small zip archives on disk for the reader tests.
/// </summary>
internal static class TestZipBuilder
{
    /// <summary>
    /// Writes a zip file containing the given entries.
    /// </summary>
    /// <param name="zipPath">The full path of the zip file to create.</param>
    /// <param name="entries">Entry path → entry bytes.</param>
    public static void Build(string zipPath, IReadOnlyDictionary<string, byte[]> entries)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);
        using var fileStream = File.Create(zipPath);
        using var zipStream = new ZipOutputStream(fileStream);
        foreach (var (entryPath, bytes) in entries)
        {
            zipStream.PutNextEntry(new ZipEntry(entryPath) { Size = bytes.Length });
            zipStream.Write(bytes, 0, bytes.Length);
            zipStream.CloseEntry();
        }

        zipStream.Finish();
    }

    /// <summary>Encodes text as UTF-8 bytes for an entry.</summary>
    public static byte[] Text(string text) => Encoding.UTF8.GetBytes(text);
}
