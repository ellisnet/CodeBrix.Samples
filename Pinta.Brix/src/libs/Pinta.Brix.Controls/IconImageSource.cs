// IconImageSource.cs
//
// Converts engine icon surfaces (premul BGRA32) into XAML image sources by
// copying pixels into a WriteableBitmap, using the platform's raw-buffer
// copy idiom (no per-icon stream wrapper).

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pinta.Brix.Engine;
using Pinta.Brix.Engine.Drawing;

namespace Pinta.Brix.Controls;

public static class IconImageSource
{
	private static readonly Dictionary<(string, int), ImageSource> cache = [];

	public static ImageSource? Create (string iconName, int size)
	{
		if (cache.TryGetValue ((iconName, size), out ImageSource? cached))
			return cached;

		// An unknown name must come back null so callers can fall back to a
		// label rather than rendering a blank square.
		if (!PintaCore.Resources.HasIcon (iconName))
			return null;

		ImageSurface surface = PintaCore.Resources.GetIcon (iconName, size);
		byte[] pixels = surface.GetData ().ToArray ();

		WriteableBitmap bitmap = new (surface.Width, surface.Height);
		pixels.CopyTo (bitmap.PixelBuffer);
		bitmap.Invalidate ();

		cache[(iconName, size)] = bitmap;
		return bitmap;
	}
}
