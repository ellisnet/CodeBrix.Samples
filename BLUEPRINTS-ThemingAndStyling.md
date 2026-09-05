# CodeBrix.Samples Blueprints: Theming and styling

These recipes cover the look of an application rather than its layout: how a
palette is written down as plain data in a library that owns no drawing type, how
a whole application repaints itself when the user picks a different scheme, how
the stock control families are re-keyed so nothing is left wearing the theme's own
colors, and how the brushes that belong to an item rather than to the application
are computed and re-tinted. They also cover the parts of a house style that are
decisions rather than code: a type and radius scale, depth done with a surface and
a hairline instead of a shadow, icons taken only from the shipped symbols font, and
the habit of proving a capability on a real head with a throwaway page before a
design leans on it. Reach for this file when you are choosing colors, offering the
user more than one scheme, following the desktop's light and dark preference, or
deciding what an application should look like before you write the markup.

The markup side of the same subject lives in
[BLUEPRINTS-ViewsAndControls.md](BLUEPRINTS-ViewsAndControls.md): the keys to
declare and where to declare them, the run-time switch as working code, the
operating-system preference wired into a picker, and `FontIcon` for a device with
no installed fonts. Those recipes stay there and are linked from the entries here
that continue them; this file is where the decisions behind them are recorded.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Model a color scheme as plain data in a UI free library](#model-a-color-scheme-as-plain-data-in-a-ui-free-library)
- [Follow or override the desktop appearance and check it from a shell](#follow-or-override-the-desktop-appearance-and-check-it-from-a-shell)
- [Choose a repaint mechanism that can carry more than two schemes](#choose-a-repaint-mechanism-that-can-carry-more-than-two-schemes)
- [Re-key every control brush family the platform ships](#re-key-every-control-brush-family-the-platform-ships)
- [Give each item its own brushes and re-tint them on a scheme change](#give-each-item-its-own-brushes-and-re-tint-them-on-a-scheme-change)
- [Build a familiar visual language from borders pills and hairlines](#build-a-familiar-visual-language-from-borders-pills-and-hairlines)
- [Draw every icon from the shipped symbols font](#draw-every-icon-from-the-shipped-symbols-font)
- [Remember the chosen scheme and read it back before the first page](#remember-the-chosen-scheme-and-read-it-back-before-the-first-page)
- [Prove a platform capability with a throwaway page before designing around it](#prove-a-platform-capability-with-a-throwaway-page-before-designing-around-it)
- [Drive a status line color and glyph from a small enum](#drive-a-status-line-color-and-glyph-from-a-small-enum)

## Related blueprints

- [BLUEPRINTS-ViewsAndControls.md](BLUEPRINTS-ViewsAndControls.md) - the markup half: which brush keys to declare and where, the run-time scheme switch as code, the system-default picker entry, and FontIcon on a device with no fonts
- [BLUEPRINTS-AppStructureAndStartup.md](BLUEPRINTS-AppStructureAndStartup.md) - the App constructor ordering these recipes slot into, and the bundled font packages a palette is drawn in
- [BLUEPRINTS-SettingsAndPersistence.md](BLUEPRINTS-SettingsAndPersistence.md) - the settings facade a chosen scheme is remembered through
- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - the bound properties and change notification the view-model-owned brushes sit beside
- [BLUEPRINTS-Testing.md](BLUEPRINTS-Testing.md) - proving the rest of an application, once a capability probe has answered its question

---

## Theming and styling

### Model a color scheme as plain data in a UI free library

**When you want this.** The application offers the user more than the platform's
light and dark, and you want the palettes themselves to be readable, testable and
free of any drawing type, so a test can assert on a color and a reader can see the
whole scheme on one screen.

**The MVVM shape.** Four small files in the shared library and no view involvement
at all. One enum names the choices the picker offers. A second enum names the jobs
a color does. A record holds one number per job. A static table holds the schemes
and the rules for choosing between them. The view layer is the only thing that
turns those numbers into brushes.

**Code.**

The roles are the vocabulary of the whole design. Naming a color by its job rather
than by its value is what lets one table drive four palettes and, later, a fifth:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorRole.cs
/// <summary>
/// The jobs a colour does in this application. A scheme is nothing more than one colour for each
/// of these roles, which is what lets the whole application be repainted by walking a table.
/// </summary>
public enum ColorRole
{
    /// <summary>The page ground and the face of a result row.</summary>
    Canvas,

    /// <summary>The header bar, box header strips, repository group rows and row hover.</summary>
    CanvasSubtle,

    // ... twenty-four roles in all, ending with ...

    /// <summary>Text drawn on an emphasis face.</summary>
    OnEmphasis,
}
```

One scheme is one record: a number per role, plus the single fact the rest of the
application needs to know about it. The indexer is what makes a scheme walkable, so
a repaint can be a loop rather than twenty-four assignments:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorSchemePalette.cs
/// <summary>
/// One complete colour scheme: a colour for every <see cref="ColorRole"/>, written as an opaque
/// ARGB value, plus whether the scheme sits on a dark ground. It carries no drawing type, so the
/// table of schemes is plain data that a test can read.
/// </summary>
public sealed record ColorSchemePalette
{
    /// <summary>True when this scheme sits on a dark ground.</summary>
    public bool BaseIsDark { get; init; }

    /// <summary>The page ground and the face of a result row.</summary>
    public uint Canvas { get; init; }

    // ... one property per role ...

    public uint this[ColorRole role] => role switch
    {
        ColorRole.Canvas => Canvas,
        ColorRole.CanvasSubtle => CanvasSubtle,
        // ...
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown colour role."),
    };
}
```

The schemes themselves are a wall of numbers, which is exactly what they should be.
Each one carries its own base, because that is what the element theme and the
readability arithmetic later ask for:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorSchemes.cs
/// <summary>The dark scheme.</summary>
public static ColorSchemePalette Dark { get; } = new ColorSchemePalette
{
    BaseIsDark = true,
    Canvas = 0xFF0D1117,
    CanvasSubtle = 0xFF161B22,
    CanvasInset = 0xFF010409,
    Hairline = 0xFF30363D,
    // ... one value per role ...
};

// ... three more palettes in the same shape ...
```

"System default" is a choice rather than a scheme, so the table resolves it against
one boolean the view supplies, and refuses to hand out colors for it:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorSchemes.cs
public static ColorScheme Resolve(ColorScheme choice, bool osPrefersDark) =>
    choice == ColorScheme.SystemDefault
        ? (osPrefersDark ? ColorScheme.Dark : ColorScheme.Light)
        : choice;

public static ColorSchemePalette Get(ColorScheme resolved) => resolved switch
{
    ColorScheme.Light => Light,
    ColorScheme.LightHighContrast => LightHighContrast,
    ColorScheme.Dark => Dark,
    ColorScheme.DarkDimmed => DarkDimmed,
    _ => throw new ArgumentOutOfRangeException(nameof(resolved), resolved,
        "Resolve the choice before asking for its colours."),
};

public static ColorScheme Parse(string name) =>
    Enum.TryParse(name, ignoreCase: false, out ColorScheme parsed) && Enum.IsDefined(parsed)
        ? parsed
        : ColorScheme.SystemDefault;
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorRole.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorScheme.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorSchemePalette.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorSchemes.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/PaletteBrushes.cs`

**Related.**
[Choose a repaint mechanism that can carry more than two schemes](#choose-a-repaint-mechanism-that-can-carry-more-than-two-schemes)
is what this table is walked by, and
[Switch between several color schemes by mutating keyed brushes in place](BLUEPRINTS-ViewsAndControls.md#switch-between-several-color-schemes-by-mutating-keyed-brushes-in-place)
is the working code on the view side.

**Sharp edges.**
- A choice is not a scheme. `Get` throws when it is handed the system entry, so the
  resolve step cannot be skipped by accident anywhere in the application.
- Write the colors as opaque `uint` ARGB values, not as strings and not as a drawing
  type. The library then has no framework dependency, a test can compare two schemes
  numerically, and one small converter class is the whole bridge to the view.
- Colors that are genuinely transparent - a fully clear row face, a dialog scrim -
  are the same in every scheme. Declare them in markup and leave them out of the
  table, so there is no doubt that everything in the table really does move.
- Keep the base light or dark flag on the palette rather than deriving it. It decides
  the element theme, and it decides which way readability arithmetic has to push.
- Parse a stored scheme name defensively and fall back to the default. A store
  written by an older build must not be able to stop the application starting.
- One enum ordering doubles as the order the picker offers, so the list of choices
  lives beside the schemes rather than in the view model.

### Follow or override the desktop appearance and check it from a shell

**When you want this.** You want the application to follow the desktop's light or
dark preference out of the box, to stop following it the moment the user picks a
named scheme, and to be able to prove both halves without logging out.

**The MVVM shape.** Two platform switches, chosen once, and one live report. The
application theme is the switch that decides whether the platform follows the desktop
at all, and it may be set only in the `App` constructor. The element theme on the root
element is the run-time switch for everything the application has not re-keyed. The
live report comes from `UISettings`, which the page owns and forwards to the view
model.

**Code.**

Leaving the application theme unset is the mechanism that keeps the platform
following the desktop. Setting it is the mechanism that stops it. There is no third
state and it cannot be changed later, so the whole decision is these five lines:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs
//Application.RequestedTheme may be set only here, before initialization completes, and
//setting it at all is what makes the platform stop following the operating system. So it
//is left alone for the "System default" choice and set for every explicit one.
var scheme = ColorSchemes.Parse(
    SettingsService.Get(SettingKeys.ColorScheme, nameof(ColorScheme.SystemDefault)));
if (scheme != ColorScheme.SystemDefault)
{
    this.RequestedTheme = ColorSchemes.Get(scheme).BaseIsDark
        ? ApplicationTheme.Dark
        : ApplicationTheme.Light;
}
```

The desktop reports its preference as the color it would paint a window with, and the
page turns that into the one boolean the palette table wants:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs
//The operating system reports its preference as the colour it would paint a window with.
private bool SystemPrefersDark()
{
    var background = _systemColors.GetColorValue(UIColorType.Background);
    var brightness = (background.R * 0.299d) + (background.G * 0.587d) + (background.B * 0.114d);
    return brightness < 128d;
}
```

On Linux the preference arrives through the desktop portal's appearance setting, and
which component of the desktop serves that setting is not the same everywhere. Two
shell commands answer both questions: the first asks the portal what it is reporting
right now, and the second flips the preference while the application is running so the
live path can be watched:

```text
gdbus call --session --dest org.freedesktop.portal.Desktop \
  --object-path /org/freedesktop/portal/desktop \
  --method org.freedesktop.portal.Settings.ReadOne "org.freedesktop.appearance" "color-scheme"
# answers (<uint32 1>,) for dark and (<uint32 2>,) for light

gsettings set org.x.apps.portal color-scheme prefer-dark
gsettings set org.x.apps.portal color-scheme prefer-light
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorSchemes.cs`

**Related.**
[Follow the operating system light and dark preference with a System default entry](BLUEPRINTS-ViewsAndControls.md#follow-the-operating-system-light-and-dark-preference-with-a-system-default-entry)
is the same subject as working view-model code: the picker entry that names what it
resolves to, and what to do when the preference changes underneath it.

**Sharp edges.**
- The application theme is settable only in the `App` constructor and only before
  initialization completes. Afterwards its setter throws. Everything that has to
  change at run time changes through the element theme on the root element instead.
- Keep the `UISettings` instance in a field. The platform holds only a weak reference
  to it, so a local one is collected and the notifications quietly stop arriving.
- Its change event does not arrive on the UI thread. Enqueue on the dispatcher before
  touching anything bound.
- The event also fires once shortly after startup, when the portal's first answer
  arrives asynchronously. Treat that as an ordinary change rather than as a surprise.
- On a Cinnamon session the key the portal actually reports is
  `org.x.apps.portal color-scheme`, not `org.gnome.desktop.interface color-scheme`.
  Setting the latter changes nothing that the application can see, and the time lost
  to that is the reason it is written down here. If the portal is missing entirely,
  the platform logs an error and assumes light.
- Put the desktop preference back the way you found it when you are finished testing.
- Deciding to follow the desktop is a decision about one boolean. Keep every other
  part of the design working from the resolved scheme, so nothing else in the
  application has to know that a desktop preference exists.

### Choose a repaint mechanism that can carry more than two schemes

**When you want this.** The application offers more palettes than the platform's two
themes, and switching has to repaint everything at once without losing a scroll
position, a typed value or a search that is still running.

**The MVVM shape.** There are four candidate mechanisms and only one of them carries
five choices. Theme dictionaries have exactly three recognized buckets, so there is
nowhere to hang a second dark scheme. Swapping merged dictionaries needs the tree
rebuilt, which loses scroll position and in-flight results. Binding every surface to
brush properties on a view model cannot reach a resource key that a control template
resolves, so the stock chrome stays on the theme. What is left, and what works, is to
declare every color once as a keyed brush and assign a new color to the brush object
that is already in the dictionary. The element theme is then set alongside it, for the
chrome the application does not own.

**Code.**

The declaration is the mechanism. Where the brushes are declared, and in what order,
is what makes the whole thing work:

```xml
<!-- From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml -->
<!-- ============================================================================
     THE COLOUR SCHEME.  Every colour the application draws comes from one of the
     role brushes below, and every stock control brush underneath them is pointed
     at the same values.  The Colors written here are the Light scheme; the page
     re-points them all when the user picks another scheme, which is why they are
     declared once, at application level, where the popup layer can see them too.
     They are declared AFTER the merged dictionary, in the same dictionary, which
     is what makes them win over the theme's own values.
     ============================================================================ -->
```

Applying a scheme is then two things, not one, and both are needed:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs
void IColorSchemeApplier.Apply(ColorSchemePalette palette, bool baseIsDark, bool followSystem)
{
    if (palette == null) { return; }

    RootGrid.RequestedTheme = followSystem
        ? ElementTheme.Default
        : (baseIsDark ? ElementTheme.Dark : ElementTheme.Light);

    Repoint(Application.Current?.Resources, palette);
    Repoint(Resources, palette);
}
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/SchemeBrushMap.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/PaletteBrushes.cs`

**Related.**
[Switch between several color schemes by mutating keyed brushes in place](BLUEPRINTS-ViewsAndControls.md#switch-between-several-color-schemes-by-mutating-keyed-brushes-in-place)
carries the full walk, the brush map and the view-model side.
[Re-key every control brush family the platform ships](#re-key-every-control-brush-family-the-platform-ships)
is what makes the stock controls part of the repaint at all.

**Sharp edges.**
- A palette consumed with `{StaticResource}` resolves once when the tree is built and
  never re-resolves. That is not a problem, it is the point: mutating the brush object
  the tree is already holding repaints every consumer without anything being resolved
  again. Replace the brush under the same key instead and every consumer keeps the old
  object, and nothing changes.
- The element theme and the brush values are complementary, not alternatives. The
  values own everything the application declared; the element theme owns the residue -
  focus visuals, the caret and selection highlight, tooltips and the popup layer.
  Setting one and not the other leaves half the window behind.
- The popup layer follows the application theme, which cannot change after startup, so
  a dialog opened after a run-time light to dark switch keeps the family it launched
  with. Re-key its brushes anyway and its surfaces still follow the scheme; the residue
  is cosmetic and worth writing into the application's known limits.
- Walk both dictionaries. Application resources are where the popup layer looks; page
  resources are where a page-only brush lives. The same walk handles both.
- Keys whose color is the same in every scheme are deliberately absent from the map.
  Anything left in the dictionary that the map does not name is a color that will not
  move, which is the first thing to check when one control stays the wrong color.

### Re-key every control brush family the platform ships

**When you want this.** The stock controls have to belong to your palette rather than
to the theme's, and you would rather not write a control template for any of them.

**The MVVM shape.** Presentation only. Every family the application actually uses gets
its full set of keys declared after the merged control resources, at application level,
and a table in the shared library says which role each key carries so a scheme change
can find them all.

**Code.**

The families this application re-keys are `Button`, `AccentButton`, the text controls,
`CheckBox`, `ComboBox`, `ListViewItem`, `ProgressBar`, `ScrollBar` and `ContentDialog`.
The map is the readable form of all of them, one comment per family:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/SchemeBrushMap.cs
//TextBox and every other text control.
{ "TextControlBackground", ColorRole.Canvas },
{ "TextControlBackgroundPointerOver", ColorRole.Canvas },
{ "TextControlBackgroundFocused", ColorRole.Canvas },
{ "TextControlBackgroundDisabled", ColorRole.CanvasSubtle },
// ...
{ "TextControlBorderBrushFocused", ColorRole.Accent },
{ "TextControlPlaceholderForeground", ColorRole.TextTertiary },
{ "TextControlPlaceholderForegroundPointerOver", ColorRole.TextTertiary },
{ "TextControlPlaceholderForegroundFocused", ColorRole.TextTertiary },
{ "TextControlPlaceholderForegroundDisabled", ColorRole.Hairline },
// ...

//ContentDialog, which the popup layer reads from application resources.
{ "ContentDialogBackground", ColorRole.Canvas },
{ "ContentDialogForeground", ColorRole.TextPrimary },
{ "ContentDialogBorderBrush", ColorRole.Hairline },
{ "ContentDialogTopOverlay", ColorRole.CanvasSubtle },
{ "ContentDialogSeparatorBorderBrush", ColorRole.Hairline },
```

Re-keying a text control's placeholder is not enough on its own. The template reaches
its placeholder color through a binding whose fallback is a theme resource, and that
fallback does not survive an element theme change at run time, so the placeholder text
disappears and does not come back. Setting the property explicitly to a brush the
application owns is the fix, and the comment is worth carrying:

```xml
<!-- From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml -->
<!-- PlaceholderForeground is set explicitly on purpose: the
     template otherwise reaches its placeholder colour through a
     binding whose fallback is a theme resource, and that fallback
     is lost when the element theme is switched at run time, which
     takes the placeholder text with it. -->
<TextBox Width="260" CornerRadius="6" TabIndex="1"
         Padding="30,6,8,6" FontSize="13"
         PlaceholderForeground="{StaticResource TextTertiaryBrush}"
         PlaceholderText="GitHub user or organization"
         Text="{d:Binding Owner, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
         KeyDown="OnSearchBoxKeyDown" />
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/SchemeBrushMap.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml`,
`NotionDocumentCreator/src/NotionDocumentCreator.UI/App.xaml` and
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml`, each re-keying the families it
uses against a single fixed palette

**Related.**
[Re-key theme brushes so controls dialogs and picker chrome follow your palette](BLUEPRINTS-ViewsAndControls.md#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette)
is the markup for one palette; this recipe is the full family sweep and the traps found
doing it.

**Sharp edges.**
- Find the key names in the platform's own theme resource dictionaries rather than by
  guessing. The naming is regular - family, part, state - so once one family is right
  the rest read straight off. The second method is empirical and just as good: apply a
  scheme, then look for the control that did not change, and the key you are missing is
  the one that paints it.
- Every family needs every state key, not only the base one. A gated command's button
  spends real time disabled, and the theme's disabled color will not be yours.
- Dialogs, pickers and an on-screen keyboard open in the popup layer, which reads
  application resources and never the page's. Those keys belong at application level
  even in an application that raises no dialogs today, because a tooltip is enough to
  show the mismatch.
- The overriding brushes must come after the merged control-resources dictionary and in
  the same dictionary. That ordering is what makes them win.
- Re-keying `ListViewItem` deserves a decision rather than a copy. A list that never
  selects wants every one of its state faces transparent, so the row's own button
  chrome is the only thing that draws.
- Set the text control placeholder color explicitly as well as re-keying it, or a
  light to dark switch at run time takes the placeholder text away permanently.

### Give each item its own brushes and re-tint them on a scheme change

**When you want this.** Some colors cannot be shared resources because they follow the
data or the application's state rather than the scheme: a state glyph per row, a pill
wearing a color the server sent, a status line that turns amber while something waits.

**The MVVM shape.** The item view model owns a `SolidColorBrush` created once in its
constructor and exposed as a get-only property, and the template binds to it. Nothing
raises a change notification, because the object never changes: only its color does. A
scheme change walks the owner's children and each one re-tints itself.

**Code.**

A row's state glyph is a role, chosen once from the data, and re-read from whatever
palette is current:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/IssueRowViewModel.cs
public void ApplyPalette(ColorSchemePalette palette)
{
    if (palette == null) { return; }

    PaletteBrushes.Repoint(StateBrush as SolidColorBrush, palette[_stateRole]);
    foreach (var label in Labels)
    {
        label.ApplyPalette(palette);
    }
}
```

A pill that wears a color from the data is arithmetic, not a lookup. The label keeps
its own hue; what changes with the scheme is the ground it is blended over and how far
its text has to be pushed to stay readable:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/IssueLabelViewModel.cs
public void ApplyPalette(ColorSchemePalette palette)
{
    if (palette == null) { return; }

    var source = _hasOwnColor ? _labelArgb : palette.Neutral;
    var (background, border, text) = LabelColorMath.PillColors(source, palette.Canvas, palette.BaseIsDark);

    PaletteBrushes.Repoint(Background as SolidColorBrush, background);
    PaletteBrushes.Repoint(BorderBrush as SolidColorBrush, border);
    PaletteBrushes.Repoint(Foreground as SolidColorBrush, text);
}
```

The arithmetic itself lives in the UI-free library, in plain numbers, so it is
straightforward to test. Three rules: blend the color faintly over the ground for the
fill and more strongly for the border, clamp the text lightness for the ground it sits
on, and push a border that lands too close to the ground away from it, or a label the
color of the page draws no pill at all:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Helpers/LabelColorMath.cs
public static (uint Background, uint Border, uint Text) PillColors(uint labelArgb, uint canvasArgb,
    bool darkBase)
{
    var background = Blend(labelArgb, canvasArgb, BackgroundBlend);
    var border = SeparateFromGround(Blend(labelArgb, canvasArgb, BorderBlend), canvasArgb);
    var text = ClampTextLightness(labelArgb, darkBase);
    return (background, border, text);
}

//Pushes a border away from the ground it is drawn on until the two are far enough apart on
//the lightness axis to be told apart. A label colour close to the page ground would otherwise
//blend into it and the pill would lose its outline; the push is along lightness only, so the
//label keeps its hue.
private static uint SeparateFromGround(uint borderArgb, uint canvasArgb)
{
    ToHsl(canvasArgb, out _, out _, out var groundLightness);
    ToHsl(borderArgb, out var hue, out var saturation, out var lightness);

    if (Math.Abs(lightness - groundLightness) >= MinimumBorderSeparation) { return borderArgb; }

    //A dark ground is pushed away from by going lighter, a light ground by going darker,
    //which is the direction that always has room.
    var wanted = groundLightness < 0.5d
        ? groundLightness + MinimumBorderSeparation
        : groundLightness - MinimumBorderSeparation;

    return FromHsl(hue, saturation, Math.Clamp(wanted, 0d, 1d));
}
```

The cascade is one method per level, so an owner never reaches past its own children:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/RepositoryGroupViewModel.cs
public void ApplyPalette(ColorSchemePalette palette)
{
    if (palette == null) { return; }

    foreach (var row in Rows)
    {
        row.ApplyPalette(palette);
    }
}
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/IssueLabelViewModel.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/IssueRowViewModel.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/RepositoryGroupViewModel.cs`
`GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Helpers/LabelColorMath.cs`
`GitHubIssueFinder/tests/libs/GitHubIssueFinder.GitHub.Tests/LabelColorMathTests.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/BundleCellViewModel.cs` and
`AtlasRegionCellViewModel.cs`, which put a `Brush` on an item view model for selection
state against a single fixed palette

**Related.**
[Build a grouped list from group and row view models](BLUEPRINTS-ViewsAndControls.md#build-a-grouped-list-from-group-and-row-view-models)
is the markup these items are drawn by, and
[Choose a repaint mechanism that can carry more than two schemes](#choose-a-repaint-mechanism-that-can-carry-more-than-two-schemes)
is what calls the cascade.

**Sharp edges.**
- Create the brush once and re-point it. A get-only `Brush` property with no change
  notification is correct precisely because the object never changes, and it is
  cheaper than raising a notification per row on every scheme change.
- Build an item against the palette that is showing when it is built. Pass the palette
  into the constructor rather than reading it from a static, so a row that arrives
  during a search is born the right color.
- Keep the arithmetic in plain numbers in a library with no drawing types. It is then
  testable without a head, and the tests are the only practical way to be sure about a
  readability rule.
- A blend and a clamp are not enough on their own. A color close to the page ground
  produces a border that vanishes into it, and the pill reads as bare text; the
  separation rule is what keeps a pill a pill.
- Push along the lightness axis only. Moving hue or saturation to gain contrast makes
  the label stop looking like itself.
- Fall back to a neutral role for an item whose color is missing or unreadable, rather
  than skipping the pill.

### Build a familiar visual language from borders pills and hairlines

**When you want this.** You want an application that people can use without being
taught, usually because it is the desktop companion to something they already know, and
you want the look written down as a small set of numbers rather than rediscovered on
every screen.

**The MVVM shape.** No view model at all. This is a set of decisions: a header bar that
wraps, bordered boxes with header strips, whole rows that are buttons, pills and chips
for counts and tags, a type scale, a radius scale, and one rule about depth.

**Code.**

The header bar is the house shape: identity on the left taking the free space, controls
on the right, and a flex panel so the right-hand group wraps under the identity on a
narrow window rather than being clipped:

```xml
<!-- From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml -->
<Border Grid.Row="0"
        Background="{StaticResource CanvasSubtleBrush}"
        BorderBrush="{StaticResource HairlineBrush}"
        BorderThickness="0,0,0,1"
        Padding="24,10">
    <flex:FlexPanel Direction="Row" Wrap="Wrap" AlignItems="Center">
        <!-- Grow=1: the identity block takes the free space, so the picker stays right -->
        <StackPanel Spacing="1" Margin="0,4,16,4" flex:FlexPanel.Grow="1">
            <!-- ... the application mark, its name and a one-line subtitle ... -->
        </StackPanel>
        <!-- ... the scheme picker ... -->
    </flex:FlexPanel>
</Border>
```

A box is a border on the page ground with a hairline and a header strip on the subtle
surface. The strip's label is all caps, small, semibold and letter-spaced, which is the
house's section label:

```xml
<!-- From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml -->
<Border Grid.Row="0"
        Background="{StaticResource CanvasBrush}"
        BorderBrush="{StaticResource HairlineBrush}"
        BorderThickness="1"
        CornerRadius="6">
    <StackPanel>
        <Border Background="{StaticResource CanvasSubtleBrush}"
                BorderBrush="{StaticResource HairlineBrush}"
                BorderThickness="0,0,0,1"
                CornerRadius="6,6,0,0"
                Padding="16,9">
            <TextBlock Text="FIND ISSUES" FontSize="11" FontWeight="SemiBold"
                       CharacterSpacing="80"
                       Foreground="{StaticResource TextSecondaryBrush}" />
        </Border>
        <!-- ... the box body ... -->
    </StackPanel>
</Border>
```

A count pill and a chip are the same object at two sizes: an inset face, a hairline
border, a generous radius and small semibold text.

```xml
<!-- From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml -->
<Border Grid.Column="2" Margin="8,0,0,0"
        Background="{StaticResource CanvasInsetBrush}"
        BorderBrush="{StaticResource HairlineBrush}"
        BorderThickness="1" CornerRadius="10"
        Padding="7,0" VerticalAlignment="Center">
    <TextBlock Text="{d:Binding CountText}" FontSize="11"
               FontWeight="SemiBold"
               Foreground="{StaticResource TextSecondaryBrush}" />
</Border>
```

The scale this application settled on, written down where the design lives rather than
inferred from the markup:

```text
Type    19 header title, 15 empty-state title, 14 row title, 13 group header and controls,
        12.5 helper and body, 12 meta and status, 11.5 subtitle, 11 pills and section
        labels, 10.5 the smallest chip.  Weight is SemiBold for titles and all-caps
        labels, Normal everywhere else.
Radius  6 on boxes, buttons, text boxes and the picker; 10 on pills, which is fully
        round at the height they are drawn.
Border  1 hairline around a box, 1 muted hairline between rows, 0,0,0,1 under a header
        strip and 0,1,0,0 above a status bar.
Depth   no shadows anywhere: a slightly different surface plus a hairline.
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml` (the header bar this one
is shaped after), `KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml`
(the whole-row-is-a-button treatment) and
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml` (the all-caps section
label as a reusable style)

**Related.**
[Wrap and reflow a layout with the FlexPanel add-in](BLUEPRINTS-ViewsAndControls.md#wrap-and-reflow-a-layout-with-the-flexpanel-add-in)
is the add-in the header bar and the label strip both lean on.

**Sharp edges.**
- Make the whole row a stretched button and hover and pressed arrive for free from the
  re-keyed button brushes. No sample here writes a visual state block for a page, and
  none needs to.
- The house radii are 8 to 12 on cards and 8 on controls. This application deliberately
  uses 6 on its boxes and controls, because it is imitating something the user already
  knows and that is the radius that product uses. A departure like that is fine and
  should be recorded in the design, not left for a reader to notice as an inconsistency.
- Depth is a surface and a hairline. An elevated view with a drop shadow exists in the
  toolkit and no sample here uses it.
- A control that spends time disabled should keep its place rather than disappear, or
  the row shifts under the pointer at exactly the moment the user is aiming at it.
- Give the content column a maximum width and center it. A results list stretched
  across a very wide window is harder to read, not easier.
- Set an explicit tab order when the markup order and the reading order differ. A
  picker declared first in the header would otherwise be the first stop.

### Draw every icon from the shipped symbols font

**When you want this.** Every icon in the application, on every head, including a device
with no installed fonts at all.

**The MVVM shape.** Fixed chrome writes the codepoint in the markup as an escape. Glyphs
a view model chooses at run time come from a constants class in the shared library, so
the codepoints are named, documented and in one place. Nothing is a literal symbol
character and nothing is an emoji.

**Code.**

The constants class is small and its comment is the important part:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/Glyphs.cs
/// <summary>
/// The symbol-font codepoints the application draws. Every one of them was checked on a running
/// head before it was written down here, because the shipped symbols font is dense but not
/// contiguous and a codepoint it does not cover draws a visible box.
/// The page's own fixed chrome writes the same codepoints as XAML escapes; these constants are
/// for the glyphs a view model chooses at run time.
/// </summary>
public static class Glyphs
{
    /// <summary>An open issue, an open pull request and the application mark: an open ring.</summary>
    public const string OpenIssue = "\uF138";

    /// <summary>A closed issue: a check.</summary>
    public const string ClosedIssue = "\uF13E";

    // ... one constant per state, ending with ...

    /// <summary>A failure: a cross in a circle.</summary>
    public const string Error = "\uEA39";
}
```

A run-time glyph and its color are both bound, so a row's state is one binding pair:

```xml
<!-- From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml -->
<FontIcon Grid.Column="0"
          Glyph="{d:Binding StateGlyph}"
          FontSize="16"
          Foreground="{d:Binding StateBrush}"
          VerticalAlignment="Top"
          Margin="0,2,10,0" />
```

The render check that has to happen before any of this is designed around is a page that
draws every candidate codepoint at a readable size with its hexadecimal value underneath,
run on the head, screenshotted and looked at. It costs a few minutes and it is the only
way to know what a codepoint actually draws:

```text
purpose                  codepoint  renders as                       decision
open issue / app mark    F138       thin ring, open circle           keep
closed issue             F13E       check mark                       keep
not planned              F140       circle with a diagonal slash     keep
closed pull request      F13D       small multiplication sign        keep
draft pull request       F137       solid filled circle              keep
search                   E721       magnifier                        keep
cancel                   E711       bold multiplication sign         keep
open in browser          E8A7       box with an arrow leaving it     keep
repository               E8B7       folder                           keep
comments                 E8BD       speech bubble with rule lines    keep
scheme picker            E790       artist's palette                 keep
waiting                  E823       clock face                       keep, see below
error                    EA39       multiplication sign in a circle  keep
person                   E77B       a person                         keep
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/Glyphs.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/IssueRowViewModel.cs`

**Related.**
[Use FontIcon glyphs so icons survive on a device with no system fonts](BLUEPRINTS-ViewsAndControls.md#use-fonticon-glyphs-so-icons-survive-on-a-device-with-no-system-fonts)
is the reason to use the symbols font at all.
[Prove a platform capability with a throwaway page before designing around it](#prove-a-platform-capability-with-a-throwaway-page-before-designing-around-it)
is the method the render check is one instance of.

**Sharp edges.**
- The symbols font's coverage is dense but not contiguous. Never compute a glyph by
  adding an offset to one you know, and never assume a codepoint exists because its
  neighbours do.
- A codepoint the font does not cover draws a visible box rather than nothing, because
  the application's text font is bundled and has a real missing-glyph shape. That is
  good news: the failure is loud, and one screenshot finds all of them at once.
- The glyph you get is not always the glyph the design named. Here the design asked for
  an hourglass for a timed wait and the shipped codepoint draws a clock, which reads
  correctly, so the design changed rather than the codepoint. Decide that deliberately
  and write it down.
- Pick a fallback codepoint for every primary while you are designing, then check both.
  Knowing the fallback renders costs nothing and removes the question later.
- Never put a literal symbol character in a text element for an icon, and do not design
  around emoji or a third-party icon set: what is not in the shipped fonts does not draw.
- The legacy symbol enum remaps codepoints, so a named symbol may not draw the codepoint
  its name suggests. Naming the codepoint is unambiguous.

### Remember the chosen scheme and read it back before the first page

**When you want this.** The application should open wearing whatever the user last chose,
and it has to be wearing it before anything is drawn rather than flickering into it.

**The MVVM shape.** The scheme is a setting like any other: written through the
application's own settings facade from the property setter that changes it, read back in
two places that must agree. The `App` constructor reads it to decide the application
theme; the first page's view model reads it into its backing field to decide the picker's
selection. The ordering is the whole recipe.

**Code.**

Keys are constants, so a misspelling is a compile error:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.Settings/SettingKeys.cs
/// <summary>The colour scheme the user picked, stored by its enum name.</summary>
public const string ColorScheme = "GitHubIssueFinder.Settings.ColorScheme";
```

The constructor order is the part that is easy to get wrong. The store has to be open
before the scheme is read, and the scheme has to be read before initialization builds the
first page:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs
//The first page's view model is built during InitializeComponent() and reads its
//remembered values in its own constructor, so the store has to be open before that.
SettingsService.Initialize();

//Application.RequestedTheme may be set only here, before initialization completes, and
//setting it at all is what makes the platform stop following the operating system. So it
//is left alone for the "System default" choice and set for every explicit one.
var scheme = ColorSchemes.Parse(
    SettingsService.Get(SettingKeys.ColorScheme, nameof(ColorScheme.SystemDefault)));
if (scheme != ColorScheme.SystemDefault)
{
    this.RequestedTheme = ColorSchemes.Get(scheme).BaseIsDark
        ? ApplicationTheme.Dark
        : ApplicationTheme.Light;
}

InitializeComponent();
```

The view model writes the value from the setter that changes it, and paints in the same
breath, so choosing and remembering cannot drift apart:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
public ColorSchemeOptionViewModel SelectedScheme
{
    get => _selectedScheme;
    set
    {
        if (value == null || ReferenceEquals(_selectedScheme, value)) { return; }

        _selectedScheme = value;
        NotifyPropertyChanged(nameof(SelectedScheme));
        SettingsService.Set(SettingKeys.ColorScheme, value.Scheme);
        ApplyCurrentScheme();
    }
}
```

Reading back happens into the field, not through the property, so opening the application
does not write the stored value straight back:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
var stored = ColorSchemes.Parse(
    SettingsService.Get(SettingKeys.ColorScheme, nameof(ColorScheme.SystemDefault)));

SchemeOptions = new ObservableCollection<ColorSchemeOptionViewModel>();
foreach (var choice in ColorSchemes.Choices)
{
    SchemeOptions.Add(new ColorSchemeOptionViewModel(choice, _osPrefersDark));
}

_selectedScheme = FindOption(stored);
CurrentPalette = ColorSchemes.Get(ColorSchemes.Resolve(stored, _osPrefersDark));
```

**Where to look.**
`GitHubIssueFinder/src/libs/GitHubIssueFinder.Settings/SettingKeys.cs`
`GitHubIssueFinder/src/libs/GitHubIssueFinder.Settings/SettingsService.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs`

**Related.**
[Wrap the AppSettings add-in in one application named facade](BLUEPRINTS-SettingsAndPersistence.md#wrap-the-appsettings-add-in-in-one-application-named-facade)
and
[Open the settings store before any other startup work](BLUEPRINTS-SettingsAndPersistence.md#open-the-settings-store-before-any-other-startup-work)
are the store this recipe writes through and the ordering rule it is a case of.

**Sharp edges.**
- Two places read the same key and they must agree: the constructor that sets the
  application theme, and the view model that sets the picker. Route both through the
  same parse helper so a stored value can only be interpreted one way.
- Read into the backing field in a constructor, never through the property. Going
  through the setter writes the value back on every launch and raises a change
  notification before anything is bound.
- Store an enum by name, not by number. A name survives a value being inserted into the
  enum later; a number does not.
- Open the store before initialization, not after. The failure is quiet and
  order-dependent: everything reads its default instead of the user's value.
- The user's other choices belong in the same store and cost nothing extra. Remembering
  the last two text-box values and a checkbox alongside the scheme is what makes the
  application feel as though it was left as it was found.

### Prove a platform capability with a throwaway page before designing around it

**When you want this.** The design depends on something no application in the repository
has done before, and you would rather find out now than after the page is written.

**The MVVM shape.** None. This is a probe: the smallest page that can answer the question,
run on the head you will ship on, screenshotted, looked at with your own eyes, and the
answer written into a file that outlives the session. Then the probe is deleted.

**Code.**

The recipe, and the numbered list of questions this application asked before its design
was final, so a reader can see the shape of a good question:

```text
V1  Does assigning a new Color to a keyed brush repaint every consumer?
V2  Does every glyph the design names actually render?
V3  Does a checkbox two-way binding work in both directions?
V4  Does the launcher open the host's default browser?
V5  Does an element theme flip re-theme control chrome and rows already realized?
V6  Do background results ever reach a bound property off the UI thread?
V7  What does the desktop report, and does a live flip reach the application?

The loop for each one:
  1  add the smallest page or block that shows the answer on screen
  2  run the head with a log file, launched by a PID you keep
  3  find the window, capture it, and open the PNG and look at it
  4  write the outcome, the evidence file name and the consequence into a notes file
  5  stop the application by the PID you launched, and delete the probe
```

Finding and capturing the window is two commands. The window this head opens does not
carry the process id, so it is found by an anchored title match and confirmed before
anything is clicked:

```text
xdotool search --name "^MyApplication$"
xwininfo -id <id>            # gives the absolute upper-left of the CLIENT area
import -window <id> shot.png # then open shot.png and look at it
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs` (what survived: the
scheme applier, the preference watch and two key handlers, with no probe left in it)

**Related.**
[Drive a scripted end-to-end run of the whole application](BLUEPRINTS-Testing.md#drive-a-scripted-end-to-end-run-of-the-whole-application)
is the other half of the same instinct: a probe answers a question once, a scripted run
keeps answering it.

**Sharp edges.**
- Run the probe on the head you will ship on. A capability that works on one head is
  evidence about that head.
- A negative answer is worth as much as a positive one and often more. One of these
  probes proved that an element theme flip correctly does not disturb the brushes the
  application had re-keyed, which is what turned two mechanisms that looked like
  alternatives into two halves that are both needed.
- Decide in advance what a "no" would mean. Each question above had a fallback design
  written down beside it, so a bad answer would have cost a redesign rather than a
  rethink.
- Delete the probe. Verification code left in a shipping page is the thing a reader
  copies by accident.
- Stop the application by the process id you launched. Never search for a process by a
  name pattern that can match your own shell, and never kill something found by a
  window title.
- Write the outcomes into a file with the screenshot names beside them. The next person
  cannot re-run your session, and "it worked when I tried it" is not evidence.
- Do not take screenshots at all on a session where the capture tools cannot see the
  window. Read the application's own log instead, and say that is what you did.

### Drive a status line color and glyph from a small enum

**When you want this.** One line at the bottom of the window says what just happened, and
it has to be able to look calm, busy, patient, finished or wrong without becoming five
separate controls.

**The MVVM shape.** A small enum names the kinds. One private method sets the text, the
kind and the glyph together, so they cannot disagree. The color is a brush the view model
owns, because it depends on the kind and on the scheme at the same time, and it is
re-pointed by the same call that repaints everything else.

**Code.**

The kinds are the design, written down:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/SearchStatusKind.cs
/// <summary>
/// What the status line is currently saying, which decides its colour and whether a glyph sits in
/// front of it.
/// </summary>
public enum SearchStatusKind
{
    /// <summary>Nothing is happening.</summary>
    Idle,

    /// <summary>A search is running.</summary>
    Working,

    /// <summary>A search is holding until a rate-limit window resets.</summary>
    Waiting,

    // ... Done, Cancelled, Failed ...
}
```

One method sets all of it, and the kind is the only thing a caller has to think about:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
private void SetStatus(string text, SearchStatusKind kind)
{
    StatusText = text ?? string.Empty;
    StatusKind = kind;
    StatusGlyph = kind switch
    {
        SearchStatusKind.Waiting => Glyphs.Waiting,
        SearchStatusKind.Failed => Glyphs.Error,
        _ => string.Empty,
    };

    NotifyPropertyChanged(nameof(StatusGlyphVisibility));
    RepaintOwnBrushes();
}

private ColorRole StatusRole() => StatusKind switch
{
    SearchStatusKind.Waiting => ColorRole.Attention,
    SearchStatusKind.Failed => ColorRole.Danger,
    _ => ColorRole.TextSecondary,
};
```

The same repaint serves both a change of kind and a change of scheme, so there is one
place where state and palette meet:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
//The brushes the view model owns rather than the resource dictionary: the status line, whose
//colour depends on what it is saying, and the search quota pill, which warms up while the
//throttle is holding.
private void RepaintOwnBrushes()
{
    var palette = CurrentPalette;
    if (palette == null) { return; }

    PaletteBrushes.Repoint(StatusBrush as SolidColorBrush, palette[StatusRole()]);

    var waiting = StatusKind == SearchStatusKind.Waiting;
    PaletteBrushes.Repoint(SearchQuotaBackground as SolidColorBrush,
        waiting ? palette.AttentionSubtle : palette.CanvasInset);
    PaletteBrushes.Repoint(SearchQuotaBorderBrush as SolidColorBrush,
        waiting ? palette.Attention : palette.Hairline);
    PaletteBrushes.Repoint(SearchQuotaForeground as SolidColorBrush,
        waiting ? palette.Attention : palette.TextSecondary);
}
```

The markup is a glyph and a line of text sharing one brush, on a bar that never grows:

```xml
<!-- From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml -->
<StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="6"
            VerticalAlignment="Center">
    <FontIcon Glyph="{d:Binding StatusGlyph}" FontSize="13"
              Foreground="{d:Binding StatusBrush}"
              Visibility="{d:Binding StatusGlyphVisibility}"
              VerticalAlignment="Center" />
    <TextBlock Text="{d:Binding StatusText}" FontSize="12"
               Foreground="{d:Binding StatusBrush}"
               TextTrimming="CharacterEllipsis" MaxLines="1"
               VerticalAlignment="Center" />
</StackPanel>
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/SearchStatusKind.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml`

**Related.**
[Report a failure as status text instead of throwing](BLUEPRINTS-MVVM.md#report-a-failure-as-status-text-instead-of-throwing)
is the rule that gives this line most of its work.
[Give each item its own brushes and re-tint them on a scheme change](#give-each-item-its-own-brushes-and-re-tint-them-on-a-scheme-change)
is the same brush-owning technique applied to list items.

**Sharp edges.**
- One method sets text, kind and glyph. Three separate setters is how a red message ends
  up with no glyph, or an amber glyph ends up beside a finished sentence.
- The status brush cannot be a shared resource. Its color is a function of the kind and
  the scheme together, so the view model owns it and re-points it.
- A visibility computed from a string needs its notification raised by hand, because
  nothing is watching the string it is derived from.
- Keep the bar one line high and trim with an ellipsis. A status bar that grows moves
  everything above it at the worst possible moment.
- Give the empty state a real sentence rather than an empty string, so the line never
  looks broken.
