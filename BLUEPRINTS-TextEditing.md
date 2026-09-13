# CodeBrix.Samples Blueprints: Text editing

This file holds two recipes about text that no XAML text control is drawing.
The first lays text out through the TextLayout add-in from a headless library,
so that shaping, measurement, caret and selection geometry and an outline path
are available with no text control involved. The second keeps the escape
sequences a terminal control understands inside the library that owns the
terminal, so that a page feeding an error line into one never spells a sequence
out. Reach for this file when you are building an editor or a drawing tool that
needs real text geometry, or when your application has a line of its own to say
inside somebody else's text surface, and you want the UI layer to stay out of
both.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Lay out and draw text through the CodeBrix Platform TextLayout add-in](#lay-out-and-draw-text-through-the-codebrix-platform-textlayout-add-in)
- [Keep terminal escape sequences in the terminal library](#keep-terminal-escape-sequences-in-the-terminal-library)

## Related blueprints

- [BLUEPRINTS-GraphicsAndRendering.md](BLUEPRINTS-GraphicsAndRendering.md) - painting the resulting glyph runs and outline paths onto a canvas
- [BLUEPRINTS-ViewsAndControls.md](BLUEPRINTS-ViewsAndControls.md) - forwarding keyboard and pointer input from a view into a model that owns the text, and hosting a text surface that cannot be bound to
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

### Keep terminal escape sequences in the terminal library

**When you want this.** Your application writes a line of its own into a terminal
control - an error where the shell's output would be - and you would rather not have
escape sequences spelled out in a page.

**The MVVM shape.** The terminal library owns every byte a terminal understands. One
static helper turns a message into the line a terminal will color, and the page feeds
the result to the control it owns. The view model supplies the message as ordinary
bound text and never learns that a terminal is involved.

**Code.**

The whole helper, with the sequences as private constants so nothing outside can use
one by accident:

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/libs/RedisSetupTool.TerminalView/TerminalText.cs
/// <summary>
/// The few lines the application writes into a terminal itself, rather than forwarding from a
/// shell. The escape sequences live here, beside the pump that feeds them, so that no page has
/// to know how a terminal is told to color a line.
/// </summary>
public static class TerminalText
{
    private const string Red = "\x1b[31m";
    private const string Reset = "\x1b[0m";
    private const string NewLine = "\r\n";

    /// <summary>Wraps a message as a red line of its own, ready to feed into a terminal.</summary>
    /// <param name="message">The message to show.</param>
    /// <returns>
    /// The message with the line breaks and escape sequences a terminal needs, or an empty
    /// string when there is no message to show.
    /// </returns>
    public static string Error(string message) =>
        string.IsNullOrEmpty(message)
            ? string.Empty
            : NewLine + Red + message + Reset + NewLine;
}
```

The page has the control, so the page does the feeding; the message itself came from the
view model, which reported the failure as text:

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.Consoles.cs
        //Finding a shell and opening the exec is the view model's work; what is left here is the
        //  part that needs the control: joining the session to it and showing a failure in it.
        var session = await _consoles.StartSessionAsync(host.Model, host.Terminal.Columns,
            host.Terminal.Rows);
        if (session is null)
        {
            host.Terminal.Feed(TerminalText.Error(host.Model.FailureMessage));
            return;
        }
```

Because the composing is in the library, the exact bytes are assertable, which is the
only way to be sure about an escape sequence:

```csharp
// From CodeBrix.Samples/RedisSetupTool/tests/libs/RedisSetupTool.TerminalView.Tests/TerminalTextTests.cs
    /// <summary>An error is a line of its own, in red, and is reset afterwards.</summary>
    [Fact]
    public void Error_WhenThereIsAMessage_WrapsItInItsOwnRedLine()
    {
        //Act
        var line = TerminalText.Error("No usable shell in this image.");

        //Assert
        line.Should().Be("\r\n\x1b[31mNo usable shell in this image.\x1b[0m\r\n");
    }
```

**Where to look.**
`RedisSetupTool/src/libs/RedisSetupTool.TerminalView/TerminalText.cs`
`RedisSetupTool/src/libs/RedisSetupTool.TerminalView/TerminalPalette.cs`
`RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.Consoles.cs`
`RedisSetupTool/tests/libs/RedisSetupTool.TerminalView.Tests/TerminalTextTests.cs`

**Related.**
[Host a control with no dependency properties by mirroring a collection from code-behind](BLUEPRINTS-ViewsAndControls.md#host-a-control-with-no-dependency-properties-by-mirroring-a-collection-from-code-behind)
is how the control these lines are fed into is reached at all.

**Sharp edges.**
- An empty message must produce no bytes at all. Returning the wrapper around an empty
  string leaves a blank colored line in the scrollback, and the caller cannot tell.
- Reset the color at the end of the run. Everything the shell writes afterwards inherits
  whatever was left set, and the terminal has no idea your application is finished
  talking.
- A terminal wants a carriage return as well as a line feed, at both ends. A bare newline
  starts the next line under the end of this one.
- Keep the sequences private and hand out finished strings. A public constant is an
  invitation for a page to compose its own line, which is how the second copy of the
  rule appears.
- The same library is the right home for the terminal's colors. Its palette type sits
  beside this helper for the same reason: both are things only the terminal understands.
