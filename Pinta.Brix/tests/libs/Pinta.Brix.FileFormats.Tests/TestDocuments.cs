// TestDocuments.cs
//
// Shared helpers for Pinta.Brix.FileFormats tests: headless document
// construction (PintaCore's managers are engine-pure), pixel fills, and
// temporary file management.

using System;
using System.IO;
using Pinta.Brix.Engine;

namespace Pinta.Brix.FileFormats.Tests;

internal static class TestDocuments
{
	/// <summary>Creates a headless document of the given size.</summary>
	public static Document Create (int width, int height)
		=> new (PintaCore.Tools, PintaCore.Workspace, new Size (width, height));

	/// <summary>
	/// A 2x2 opaque test image: red, green (top row), blue, white (bottom row).
	/// Opaque pixels are identical in premultiplied and straight alpha form, so
	/// they survive every codec's alpha conversion exactly.
	/// </summary>
	public static ColorBgra[] Quad2x2 () => [
		ColorBgra.FromBgra (b: 0, g: 0, r: 255, a: 255),
		ColorBgra.FromBgra (b: 0, g: 255, r: 0, a: 255),
		ColorBgra.FromBgra (b: 255, g: 0, r: 0, a: 255),
		ColorBgra.FromBgra (b: 255, g: 255, r: 255, a: 255),
	];

	/// <summary>Writes the given pixels into the layer's surface.</summary>
	public static void SetPixels (Layer layer, ReadOnlySpan<ColorBgra> pixels)
	{
		pixels.CopyTo (layer.Surface.GetPixelData ());
		layer.Surface.MarkDirty ();
	}

	/// <summary>Reads a copy of the layer's surface pixels.</summary>
	public static ColorBgra[] GetPixels (Layer layer)
		=> layer.Surface.GetReadOnlyPixelData ().ToArray ();

	/// <summary>Returns a unique temp-file path with the given extension.</summary>
	public static string TempFile (string extension)
		=> Path.Combine (Path.GetTempPath (), $"pinta-brix-ff-{Guid.NewGuid ():N}.{extension}");

	/// <summary>Deletes a temp file, ignoring failures.</summary>
	public static void DeleteQuietly (string file)
	{
		try {
			File.Delete (file);
		} catch {
			// Best effort - temp files are cleaned by the OS eventually.
		}
	}
}
