# CodeBrix.Samples Blueprints: Text editing

This file holds a single recipe: laying out and drawing text through the
TextLayout add-in from a headless library, so that shaping, measurement,
caret and selection geometry and an outline path are available with no XAML
text control involved. Reach for it when you are building an editor or a
drawing tool that needs real text geometry and wants the UI layer to stay
out of the add-in.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Lay out and draw text through the CodeBrix Platform TextLayout add-in](#lay-out-and-draw-text-through-the-codebrix-platform-textlayout-add-in)

## Related blueprints

- [BLUEPRINTS-GraphicsAndRendering.md](BLUEPRINTS-GraphicsAndRendering.md) - painting the resulting glyph runs and outline paths onto a canvas
- [BLUEPRINTS-ViewsAndControls.md](BLUEPRINTS-ViewsAndControls.md) - forwarding keyboard and pointer input from a view into a model that owns the text
- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - the headless-library and change-notification shape the wrapper class follows

---

## Text editing

### Lay out and draw text through the CodeBrix Platform TextLayout add-in

**When you want this.** You need real text shaping, measurement, caret and
selection geometry and an outline path, with no XAML text control involved.

**The MVVM shape.** A wrapper class in a headless library holds the add-in's
layout result, rebuilds it when the text model reports a change, and exposes the
geometry the editor needs. The UI layer never touches the add-in.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/Re-editable/Text/TextLayout.cs
private TextLayoutResult BuildResult ()
{
	string text = engine.ToString ();
	is_empty = text.Length == 0;

	FontDescription font = engine.Font;

	// G8: clamp the weight onto the add-in's 100..900 scale.
	TextFontWeight weight = (TextFontWeight) Math.Clamp (font.Weight / 100 * 100, 100, 900);

	TextRunDescriptor run = new (
		is_empty ? " " : text,
		font.Family,
		(float) Math.Max (1.0, font.Size),
		weight,
		font.Italic ? TextFontStyle.Italic : TextFontStyle.Normal);

	// G1: alignment has no effect without a width, and Pinta aligns
	// without wrapping - so measure the natural width first, then lay out
	// again at that width with the wanted alignment.
	TextAlign alignment = engine.Alignment switch {
		TextAlignment.Center => TextAlign.Center,
		TextAlignment.Right => TextAlign.Right,
		_ => TextAlign.Left,
	};

	TextLayoutResult first = TextLayoutEngine.Layout ([run], null);

	if (alignment == TextAlign.Left || is_empty)
		return first;

	float width = first.Size.Width;
	first.Dispose ();

	return TextLayoutEngine.Layout ([run], new TextLayoutOptions {
		MaxWidth = width,
		Alignment = alignment,
	});
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/Re-editable/Text/TextLayout.cs
/// <summary>
/// The text outline as a path in canvas coordinates (already offset by
/// the engine's origin). Fill it for the text body, stroke it for the
/// outline style; an empty path when there is no text.
/// </summary>
public Drawing.Path GetOutline ()
{
	SKPathBuilder builder = new ();

	if (!is_empty) {
		using SKPath outline = Result.GetOutlinePath ();
		builder.AddPath (outline, SKMatrix.CreateTranslation (engine.Origin.X, engine.Origin.Y));
	}

	return new Drawing.Path (builder.Snapshot ());
}
```

Font family enumeration is answered by the graphics library directly, not by the
add-in:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/Re-editable/Text/TextLayout.cs
public static IReadOnlyList<string> Families {
	get {
		if (families is null) {
			families = SKFontManager.Default.GetFontFamilies ();
			Array.Sort (families, StringComparer.OrdinalIgnoreCase);
		}
		return families;
	}
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/Re-editable/Text/TextLayout.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Tools/Tools/TextTool.cs`
`Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/TextLayoutTests.cs`

**Sharp edges.**
- Alignment does nothing without a width, so a non-left alignment needs a measure
  pass followed by a second layout at the measured width. Dispose the first
  result.
- Empty text is laid out as a single space so caret and line metrics stay
  meaningful, with a private flag remembering the truth.
- Indices are .NET character indices, so a surrogate pair is two of them; the
  tests cover exactly that round trip.
- Font weight is clamped onto the add-in's own scale.
- The add-in has no text-decoration concept, so underline rules are derived from
  per-line selection rectangles.
- Layout results are disposable and cached; drop the cache whenever the text model
  reports a change.
- The project file states a version rule worth copying: the add-in must stay
  lock-stepped with the platform version the heads reference, because the family
  ships at one version.

