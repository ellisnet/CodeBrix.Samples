// FontDescription.cs
//
// Pinta.Brix replacement for the text-layout library's font description the
// upstream text engine stored. Carries the same information (family, size in
// points, weight/style flags); the UI layer's text renderer maps it onto its
// own font stack.

namespace Pinta.Brix.Engine;

public sealed class FontDescription
{
	public string Family { get; set; } = "Sans";

	/// <summary>Font size in points.</summary>
	public double Size { get; set; } = 12;

	public bool Bold { get; set; }

	public bool Italic { get; set; }

	public static FontDescription New ()
		=> new ();

	public FontDescription Copy ()
		=> new () {
			Family = Family,
			Size = Size,
			Bold = Bold,
			Italic = Italic,
		};
}
