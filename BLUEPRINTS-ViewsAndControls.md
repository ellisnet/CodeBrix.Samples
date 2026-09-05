# CodeBrix.Samples Blueprints: Views, XAML and custom controls

These recipes cover the view layer: how a page declares the CodeBrix.Platform
XAML namespaces and binds to a view model, how theme brush keys are re-keyed so
dialogs, pickers and list rows follow your own palette, how value converters
turn model state into text, visibility, opacity or a style, and how a layout
wraps and reflows as the window changes shape. They cover binding the stock
controls well - a checkbox tree, a password box, a scrubber wired straight to
the media element, encoded bytes shown in an image element - and the controls
you end up writing yourself when no stock control fits: image-backed buttons,
drawn widgets on a Skia canvas, splitter bars, floating option panels and
panels generated from a descriptor or by reflection. A last group assembles
the shell of an editor from a command model rather than from markup - menus,
toolbars, keyboard shortcuts, a tabbed document area with a toolbox and side
pads - together with the wiring that forwards pointer, wheel and keyboard
input from the view into a model that references no UI types. Reach for this
file whenever you are writing markup or page code-behind, and when you need
to know which small amount of work legitimately belongs in the view rather
than in a SimpleViewModel.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Declare a Skia page and bind with the platform Binding markup extension](#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension)
- [Re-key theme brushes so controls dialogs and picker chrome follow your palette](#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette)
- [Switch between several color schemes by mutating keyed brushes in place](#switch-between-several-color-schemes-by-mutating-keyed-brushes-in-place)
- [Follow the operating system light and dark preference with a System default entry](#follow-the-operating-system-light-and-dark-preference-with-a-system-default-entry)
- [Build a grouped list from group and row view models](#build-a-grouped-list-from-group-and-row-view-models)
- [Dim a list row for an item the application cannot act on](#dim-a-list-row-for-an-item-the-application-cannot-act-on)
- [Show a relative date with the exact one in a ToolTip](#show-a-relative-date-with-the-exact-one-in-a-tooltip)
- [Format a value for display with an IValueConverter](#format-a-value-for-display-with-an-ivalueconverter)
- [Highlight the selected button with a value converter](#highlight-the-selected-button-with-a-value-converter)
- [Bind a scrubber and volume slider straight to the media element](#bind-a-scrubber-and-volume-slider-straight-to-the-media-element)
- [Switch a page between two modes with one bool and a converter](#switch-a-page-between-two-modes-with-one-bool-and-a-converter)
- [Show a panel only when the last operation left something to say](#show-a-panel-only-when-the-last-operation-left-something-to-say)
- [Load an SVG or bitmap from an embedded resource with a custom URI scheme](#load-an-svg-or-bitmap-from-an-embedded-resource-with-a-custom-uri-scheme)
- [Build a button that combines an embedded image with text](#build-a-button-that-combines-an-embedded-image-with-text)
- [Wrap and reflow a layout with the FlexPanel add-in](#wrap-and-reflow-a-layout-with-the-flexpanel-add-in)
- [Bind a TreeView to a view model tree with checkboxes](#bind-a-treeview-to-a-view-model-tree-with-checkboxes)
- [Take a secret token in a PasswordBox and keep it out of storage](#take-a-secret-token-in-a-passwordbox-and-keep-it-out-of-storage)
- [Forward pointer input from a canvas into a model](#forward-pointer-input-from-a-canvas-into-a-model)
- [Translate platform pointer and key events into a headless input model](#translate-platform-pointer-and-key-events-into-a-headless-input-model)
- [Select a canvas base class per head with conditional compilation](#select-a-canvas-base-class-per-head-with-conditional-compilation)
- [Show live video on an SKXamlCanvas subclass](#show-live-video-on-an-skxamlcanvas-subclass)
- [Turn image bytes into a bound BitmapImage](#turn-image-bytes-into-a-bound-bitmapimage)
- [Let the page do the layout arithmetic only it can do](#let-the-page-do-the-layout-arithmetic-only-it-can-do)
- [Build menus and toolbars from a command model instead of XAML](#build-menus-and-toolbars-from-a-command-model-instead-of-xaml)
- [Dispatch keyboard shortcuts from one page KeyDown handler](#dispatch-keyboard-shortcuts-from-one-page-keydown-handler)
- [Bind a page level CheckBox two way](#bind-a-page-level-checkbox-two-way)
- [Run a command when the user presses Enter in a text box](#run-a-command-when-the-user-presses-enter-in-a-text-box)
- [Render a tool options toolbar from a descriptor model](#render-a-tool-options-toolbar-from-a-descriptor-model)
- [Build a drawn widget as an SKXamlCanvas subclass with hit testing](#build-a-drawn-widget-as-an-skxamlcanvas-subclass-with-hit-testing)
- [Supply a splitter bar where the platform has none](#supply-a-splitter-bar-where-the-platform-has-none)
- [Show a modeless floating options panel so a live preview stays visible](#show-a-modeless-floating-options-panel-so-a-live-preview-stays-visible)
- [Generate an options panel from object properties by reflection](#generate-an-options-panel-from-object-properties-by-reflection)
- [Show a cancellable progress dialog from synchronous code](#show-a-cancellable-progress-dialog-from-synchronous-code)
- [Lay out a document editor shell with tabs a toolbox and pads](#lay-out-a-document-editor-shell-with-tabs-a-toolbox-and-pads)
- [Split a page code-behind into named partial files](#split-a-page-code-behind-into-named-partial-files)
- [Use FontIcon glyphs so icons survive on a device with no system fonts](#use-fonticon-glyphs-so-icons-survive-on-a-device-with-no-system-fonts)

## Related blueprints

- [BLUEPRINTS-MVVM.md](BLUEPRINTS-MVVM.md) - the properties, SimpleCommand definitions and change notification that the markup here binds against
- [BLUEPRINTS-ThemingAndStyling.md](BLUEPRINTS-ThemingAndStyling.md) - the decisions behind the brush keys these recipes declare: the palette as data, the repaint mechanism, the visual language, the glyph table and the capability probe
- [BLUEPRINTS-GraphicsAndRendering.md](BLUEPRINTS-GraphicsAndRendering.md) - what actually gets painted inside the canvas elements these pages host
- [BLUEPRINTS-AppStructureAndStartup.md](BLUEPRINTS-AppStructureAndStartup.md) - application-level resources, fonts and the per-head startup these pages assume
- [BLUEPRINTS-TextEditing.md](BLUEPRINTS-TextEditing.md) - shaping, caret and selection geometry for a control that draws its own text

---

## Views, XAML and custom controls

### Declare a Skia page and bind with the platform Binding markup extension

**When you want this.** You are writing XAML that compiles into a Skia head and
want to know exactly which namespaces to declare, how the view model gets there,
and why plain `{Binding}` silently does nothing.

**The MVVM shape.** The page declares the platform's control and data namespaces
with `clr-namespace:...;assembly=...` URIs and binds with `{d:Binding ...}`, where
`d` is the platform's data namespace. A region can be scoped to a child view model
by re-pointing `DataContext` on its container, so every binding inside is relative
to that child.

**Code.**

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml -->
<Page
    x:Class="PdfSideBySide.Views.MainPage"
    xmlns="clr-namespace:Microsoft.UI.Xaml.Controls;assembly=CodeBrix.Platform.UI"
    xmlns:d="clr-namespace:Microsoft.UI.Xaml.Data;assembly=CodeBrix.Platform.UI"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="clr-namespace:PdfSideBySide.ViewModels;assembly=PdfSideBySide.Core"
    xmlns:local="using:PdfSideBySide.Views"
    FontFamily="{StaticResource RobotoFont}"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Page.DataContext>
        <vm:MainViewModel />
    </Page.DataContext>
    <!-- ... -->
        <!-- Bottom row: page labels and the comparison note -->
        <TextBlock Grid.Row="1" Grid.Column="0" Text="{d:Binding LeftPane.PageLabel}" HorizontalAlignment="Center" />
        <TextBlock Grid.Row="1" Grid.Column="1" Text="{d:Binding StatusText}" HorizontalAlignment="Center"
                   TextTrimming="CharacterEllipsis" TextWrapping="NoWrap" />
        <TextBlock Grid.Row="1" Grid.Column="2" Text="{d:Binding RightPane.PageLabel}" HorizontalAlignment="Center" />
```

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml -->
        <Grid Grid.Column="0" DataContext="{d:Binding LeftPane}" RowSpacing="6">
            <!-- ... -->
                <Button Content="{d:Binding BrowseLabel}" Command="{d:Binding BrowseCommand}" FontWeight="SemiBold"
                        Height="24" MinHeight="0" Padding="8,0" />
```

A command that has to say which of several things it acts on takes a plain string
parameter, parsed defensively so a typo disables the button rather than throwing:

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml -->
                <Button Grid.Row="0" Grid.Column="1" Width="24" Height="24" MinWidth="0" MinHeight="0" Padding="0"
                        Command="{d:Binding PanCommand}" CommandParameter="Left:Up"
                        ToolTipService.ToolTip="Document 1 - pan up">
                    <FontIcon Glyph="&#xE70E;" FontSize="12" />
                </Button>
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    public SimpleCommand PanCommand => field ??=
        new SimpleCommand(parameter => CanPan(parameter), parameter => DoPan(parameter));

    private static bool TryParsePan(object parameter, out DocumentSide side, out PanDirection direction)
    {
        side = default;
        direction = default;
        if (parameter is not string text) { return false; }
        var parts = text.Split(':');
        return parts.Length == 2
            && Enum.TryParse(parts[0], ignoreCase: true, out side)
            && Enum.TryParse(parts[1], ignoreCase: true, out direction);
    }
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml`

**Also shown by.**
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml`,
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml`,
and every other application's `Views/MainPage.xaml`;
`JustBetweenUs/JustBetweenUs.WinUI/Views/MainPage.xaml` and
`JustBetweenUs/JustBetweenUs.Wpf/Views/MainWindow.xaml` show the native heads
binding the same view model with plain `{Binding ...}`

**Sharp edges.**
- Bindings in Skia XAML are written `{d:Binding ...}`. The native WinUI, WPF and
  MAUI pages use plain `{Binding ...}` against the same view model. That is the
  one place four UI stacks' markup genuinely differs, which is why pages are
  per-stack files while the view model is one file.
- The default XML namespace maps to the platform's controls assembly, so plain
  element names resolve there. Types from your own libraries need an explicit
  `clr-namespace:...;assembly=...` prefix, and the assembly name is usually not
  the same as the namespace - see the RootNamespace rule in the project-layout
  area.
- `Mode=TwoWay, UpdateSourceTrigger=PropertyChanged` on a text box is what makes
  `[AffectsCommands]` refresh buttons while the user types.
- Instantiating the view model in `<Page.DataContext>` means no constructor
  injection is possible. Resolving it from `SimpleServiceResolver` in the page's
  constructor is the more flexible shape.
- `[Microsoft.UI.Xaml.Data.Bindable]` on a view-model class is what makes it
  usable as a binding source.

### Re-key theme brushes so controls dialogs and picker chrome follow your palette

**When you want this.** Stock theme colors clash with your design and you would
rather not restyle every control, or the theme's own selection highlight washes
out the text in your rows.

**The MVVM shape.** Presentation only, no view-model involvement. Override the
theme's own brush resource keys - in `Page.Resources` for the page, and in
`Application.Resources` for anything in the popup layer - then base a lightweight
style on the theme style for shaping only.

**Code.**

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml -->
<!-- Re-key the theme's accent-button brushes to the app's coral accent, so
     {ThemeResource AccentButtonStyle} buttons follow the app palette -->
<m:SolidColorBrush x:Key="AccentButtonBackground" Color="#F96854" />
<m:SolidColorBrush x:Key="AccentButtonBackgroundPointerOver" Color="#FF7F6C" />
<m:SolidColorBrush x:Key="AccentButtonBackgroundPressed" Color="#D65344" />
<m:SolidColorBrush x:Key="AccentButtonBackgroundDisabled" Color="#3A3F49" />
<m:SolidColorBrush x:Key="AccentButtonForeground" Color="#FFFFFF" />
<!-- ... -->

<!-- The primary (accent) button: the theme's accent style plus app shaping -->
<ui:Style x:Key="PrimaryButtonStyle" TargetType="c:Button" BasedOn="{StaticResource AccentButtonStyle}">
  <ui:Setter Property="CornerRadius" Value="8" />
  <ui:Setter Property="Padding" Value="16,7" />
  <ui:Setter Property="FontWeight" Value="SemiBold" />
</ui:Style>
```

Anything that opens in the popup layer follows the application's theme rather than
the page's, so its keys belong at application level:

```xml
<!-- From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/App.xaml -->
<Application x:Class="NotionDocumentCreator.App"
     xmlns="clr-namespace:Microsoft.UI.Xaml;assembly=CodeBrix.Platform.UI"
     xmlns:m="clr-namespace:Microsoft.UI.Xaml.Media;assembly=CodeBrix.Platform.UI"
     xmlns:c="clr-namespace:Microsoft.UI.Xaml.Controls;assembly=CodeBrix.Platform.UI.FluentTheme"
     xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
     RequestedTheme="Dark">

  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <!-- Load WinUI resources -->
        <c:XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
      </ResourceDictionary.MergedDictionaries>
      <!-- Roboto font - reference the .ttf file directly (the Fonts.xaml
           merge does not work on Skia targets) -->
      <m:FontFamily x:Key="RobotoFont">ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/Roboto.ttf</m:FontFamily>

      <!-- Dialogs open in the popup layer, which follows the app default theme (the
           RequestedTheme="Dark" above) rather than RootGrid's - these ContentDialog
           keys then refine them to the app palette. On the FrameBuffer heads the
           built-in picker/software-keyboard chrome resolves the same keys, so it
           restyles identically -->
      <m:SolidColorBrush x:Key="ContentDialogBackground" Color="#1F232B" />
      <m:SolidColorBrush x:Key="ContentDialogForeground" Color="#F2F4F8" />
      <m:SolidColorBrush x:Key="ContentDialogBorderBrush" Color="#2A2F39" />
      <m:SolidColorBrush x:Key="ContentDialogLightDismissOverlayBackground" Color="#99000000" />
      <!-- Resolved by the FrameBuffer/Emulated picker + software-keyboard chrome -->
      <m:SolidColorBrush x:Key="ContentDialogTopOverlay" Color="#1F232B" />
      <m:SolidColorBrush x:Key="ContentDialogSeparatorBorderBrush" Color="#2A2F39" />
      <m:SolidColorBrush x:Key="ContentDialogSmokeFill" Color="#4D000000" />
    </ResourceDictionary>
  </Application.Resources>

</Application>
```

A list's own selection brushes are worth the same treatment:

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml -->
<!-- The theme's own selection brushes are a light accent, which the light text in the file
     rows disappears into. These are the same accent taken down to something the rows read on. -->
<m:SolidColorBrush x:Key="ListViewItemBackgroundSelected" Color="#FF25344D" />
<m:SolidColorBrush x:Key="ListViewItemBackgroundSelectedPointerOver" Color="#FF2C3E5C" />
<m:SolidColorBrush x:Key="ListViewItemBackgroundSelectedPressed" Color="#FF1F2C42" />
<m:SolidColorBrush x:Key="ListViewItemBackgroundPointerOver" Color="#FF262B34" />
<m:SolidColorBrush x:Key="ListViewItemBackgroundPressed" Color="#FF20242B" />
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml` and `App.xaml`
`NotionDocumentCreator/src/NotionDocumentCreator.UI/App.xaml` and
`Views/MainPage.xaml`
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml` (re-keyed
slider brushes so the audio scrubber follows the palette)

**Sharp edges.**
- Page-level keys cover the page. Dialogs, pickers and the software keyboard open
  in the popup layer, follow the application's `RequestedTheme`, and need the same
  keys defined at application level instead.
- Each control family needs its full set of state keys - normal, pointer-over,
  pressed and disabled - not just the base one. A gated command's button spends
  real time disabled, and the theme default will not match your palette.
- The overriding brushes must be declared after the merged control-resources
  dictionary in the same resource dictionary.
- `XamlControlsResources` has to be in the merged dictionaries at all, or the
  built-in control styles are missing.
- `RequestedTheme` is set on the `Application` element. CodeBrixVideoTool's
  palette comment records the design reason for its dark theme: a video tool is
  looked at for a long time beside a moving picture, so the panels sit back and
  the picture is the only bright thing on screen.

### Switch between several color schemes by mutating keyed brushes in place

**When you want this.** The application offers the user more than the platform's two
themes - several named palettes, light and dark - and switching between them has to
repaint everything at once without losing a scroll position, a typed value or a search
that is still running.

**The MVVM shape.** The palettes are plain data in the shared library: one enum of
choices, one record of ARGB numbers per scheme, and no drawing type anywhere. The page
paints, through a small bridge interface declared beside the view model. Every color the
application draws is a keyed `SolidColorBrush` declared once at application level, and
applying a scheme assigns a new `Color` to the brush that is already in the dictionary.
Mutating a brush repaints every `{StaticResource}` consumer in the same frame, including
the stock control chrome and the list rows already realized, with nothing rebuilt.

**Code.**

The scheme table is numbers, with no UI type in the file:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorSchemes.cs
/// <summary>The light scheme.</summary>
public static ColorSchemePalette Light { get; } = new ColorSchemePalette
{
    BaseIsDark = false,
    Canvas = 0xFFFFFFFF,
    CanvasSubtle = 0xFFF6F8FA,
    CanvasInset = 0xFFF6F8FA,
    Hairline = 0xFFD0D7DE,
    // ... one value per role, twenty-four of them ...
};

// ... three more palettes in the same shape ...
```

A second table says which role each keyed brush carries, including the stock control keys
the application re-keys:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/SchemeBrushMap.cs
/// <summary>
/// Which color role each keyed brush in App.xaml carries. Applying a scheme is walking this
/// table, looking the key up in the resource dictionaries and assigning the role's color to the
/// brush that is already there, which repaints every consumer without touching the markup.
/// Keys whose color is the same in every scheme - the fully transparent faces and the dialog
/// scrim - are deliberately absent, because nothing needs to be done to them.
/// </summary>
public static class SchemeBrushMap
{
    public static IReadOnlyDictionary<string, ColorRole> Entries { get; } =
        new Dictionary<string, ColorRole>(StringComparer.Ordinal)
        {
            //The application's own role brushes.
            { "CanvasBrush", ColorRole.Canvas },
            { "CanvasSubtleBrush", ColorRole.CanvasSubtle },
            // ...

            //Button, which is both the secondary button and every clickable row.
            { "ButtonBackground", ColorRole.ButtonFace },
            { "ButtonBackgroundPointerOver", ColorRole.ButtonFaceHover },
            { "ButtonBackgroundPressed", ColorRole.ButtonFacePressed },
            { "ButtonBackgroundDisabled", ColorRole.ButtonFace },
            // ...
        };
}
```

The view model owns the choice and calls the page through the bridge; the page does the
painting:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
public interface IColorSchemeApplier
{
    void Apply(ColorSchemePalette palette, bool baseIsDark, bool followSystem);
}
```

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs
/// <summary>
/// Paints a color scheme: the element theme decides the chrome this application does not
/// re-key, and every keyed brush the scheme drives is re-pointed in place, which repaints
/// every consumer without a binding being raised.
/// </summary>
void IColorSchemeApplier.Apply(ColorSchemePalette palette, bool baseIsDark, bool followSystem)
{
    if (palette == null) { return; }

    RootGrid.RequestedTheme = followSystem
        ? ElementTheme.Default
        : (baseIsDark ? ElementTheme.Dark : ElementTheme.Light);

    Repoint(Application.Current?.Resources, palette);
    Repoint(Resources, palette);
}

private static void Repoint(ResourceDictionary dictionary, ColorSchemePalette palette)
{
    if (dictionary == null) { return; }

    foreach (var entry in SchemeBrushMap.Entries)
    {
        if (dictionary.TryGetValue(entry.Key, out var value) && value is SolidColorBrush brush)
        {
            PaletteBrushes.Repoint(brush, palette[entry.Value]);
        }
    }
}
```

Re-pointing is one assignment, and it is what makes every consumer repaint:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/PaletteBrushes.cs
/// <summary>
/// Re-points an existing brush at another color, which repaints everything drawn with it.
/// </summary>
public static void Repoint(SolidColorBrush brush, uint argb)
{
    if (brush == null) { return; }
    brush.Color = ToColor(argb);
}
```

Some colors cannot be shared resources because they follow application state rather than
the scheme - a status line that turns amber while something waits, a state glyph per row.
Those brushes live on the view models and are re-pointed the same way, and the owner walks
its children on a scheme change:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
private void ApplyCurrentScheme()
{
    var choice = _selectedScheme?.Scheme ?? ColorScheme.SystemDefault;
    var palette = ColorSchemes.Get(ColorSchemes.Resolve(choice, _osPrefersDark));
    CurrentPalette = palette;

    _schemeApplier?.Apply(palette, palette.BaseIsDark, choice == ColorScheme.SystemDefault);

    RepaintOwnBrushes();
    foreach (var group in Groups)
    {
        group.ApplyPalette(palette);
    }
}
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/SchemeBrushMap.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorSchemes.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/PaletteBrushes.cs`

**Related.**
[Re-key theme brushes so controls dialogs and picker chrome follow your palette](#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette)
is the static half of this: which keys to declare and where. This recipe is what to do
when the values have to change while the application runs.

**Sharp edges.**
- Declare the brushes at application level, after the merged control resources and in the
  same dictionary. That is what makes them win over the theme's own values, and it is the
  only place the popup layer can see them.
- Mutate the brush; do not replace it. Putting a new `SolidColorBrush` under the same key
  leaves every consumer holding the old object.
- Each control family needs every state key - normal, pointer-over, pressed, disabled - in
  the map, or a disabled button keeps the stock theme's color while everything around it
  changes.
- Set the element theme as well as the values. It governs the residue the application does
  not re-key: focus visuals, the caret and selection highlight, tooltips and the popup
  layer.
- A `TextBox` placeholder reaches its color through a binding whose fallback is a theme
  resource, and that fallback does not survive an element-theme change at run time - the
  placeholder text disappears for good. Set `PlaceholderForeground` explicitly to a brush
  you own.
- `Application.RequestedTheme` cannot change after startup, so the popup layer keeps the
  family it launched with. Re-key the dialog brushes anyway and the surfaces still follow
  the scheme.

### Follow the operating system light and dark preference with a System default entry

**When you want this.** Your theme picker should open on "System default", follow the
desktop's own light or dark preference while it is selected, and stop following it
completely the moment the user picks something explicit.

**The MVVM shape.** The choice is an enum value like any other, and a `Resolve` helper
turns it into a real scheme using a single boolean the page supplies. The page owns the
platform side: it reads the preference from `UISettings`, keeps that instance alive, and
tells the view model when it changes. The one rule that decides everything else is that
setting `Application.RequestedTheme` at all is what makes the platform stop following the
operating system - so for the system choice it is never set.

**Code.**

The choice resolves to a scheme; nothing else in the application has to know about the
operating system:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorSchemes.cs
public static ColorScheme Resolve(ColorScheme choice, bool osPrefersDark) =>
    choice == ColorScheme.SystemDefault
        ? (osPrefersDark ? ColorScheme.Dark : ColorScheme.Light)
        : choice;

public static string DisplayName(ColorScheme choice, bool osPrefersDark) => choice switch
{
    ColorScheme.SystemDefault => osPrefersDark ? "System default (Dark)" : "System default (Light)",
    ColorScheme.Light => "Light",
    ColorScheme.LightHighContrast => "Light High Contrast",
    ColorScheme.Dark => "Dark",
    ColorScheme.DarkDimmed => "Dark Dimmed",
    _ => choice.ToString(),
};
```

The `App` constructor sets the application theme only for an explicit choice:

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

The page watches the operating system and hands the answer to the view model. The
`UISettings` instance has to be a field:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs
//Kept in a field on purpose: the platform holds only a weak reference to a UISettings, so a
//local one would be collected and the operating system's theme changes would stop arriving.
private readonly UISettings _systemColors = new UISettings();

public MainPage()
{
    DataContextChanged += (_, _) =>
    {
        //Give the view model's dialog helpers a XamlRoot to attach to, and hand it the page
        //as the thing that can paint a color scheme.
        (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);
        (DataContext as MainViewModel)?.AttachSchemeApplier(this, SystemPrefersDark());
    };

    _systemColors.ColorValuesChanged += (_, _) => DispatcherQueue.TryEnqueue(() =>
        (DataContext as MainViewModel)?.OnSystemThemeChanged(SystemPrefersDark()));

    this.InitializeComponent(); //Leave this line last
}

//The operating system reports its preference as the color it would paint a window with.
private bool SystemPrefersDark()
{
    var background = _systemColors.GetColorValue(UIColorType.Background);
    var brightness = (background.R * 0.299d) + (background.G * 0.587d) + (background.B * 0.114d);
    return brightness < 128d;
}
```

The view model repaints only when the system choice is the one selected, and replaces the
picker entry rather than renaming it:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
public void OnSystemThemeChanged(bool osPrefersDark)
{
    if (_osPrefersDark == osPrefersDark) { return; }

    _osPrefersDark = osPrefersDark;
    RefreshSchemeNames();

    if (_selectedScheme != null && _selectedScheme.Scheme == ColorScheme.SystemDefault)
    {
        ApplyCurrentScheme();
    }
}

private void RefreshSchemeNames()
{
    for (var index = 0; index < SchemeOptions.Count; index++)
    {
        var option = SchemeOptions[index];
        var wanted = ColorSchemes.DisplayName(option.Scheme, _osPrefersDark);
        if (string.Equals(option.DisplayName, wanted, StringComparison.Ordinal)) { continue; }

        //The entry is replaced rather than renamed, because the picker's closed face reads
        //its item once and would otherwise keep showing the old text.
        var replacement = new ColorSchemeOptionViewModel(option.Scheme, _osPrefersDark);
        var wasSelected = ReferenceEquals(_selectedScheme, option);
        SchemeOptions[index] = replacement;

        if (wasSelected)
        {
            _selectedScheme = replacement;
            NotifyPropertyChanged(nameof(SelectedScheme));
        }
    }
}
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.UI/App.xaml.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/ColorSchemes.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Keep the `UISettings` in a field. The platform holds a weak reference to it, so a local
  one is collected and the notifications quietly stop.
- `ColorValuesChanged` does not arrive on the UI thread. Enqueue on the dispatcher before
  touching anything bound.
- Set `Application.RequestedTheme` in the `App` constructor or not at all. Leaving it unset
  is the mechanism that keeps the platform following the desktop, and setting it is the
  mechanism that stops it - there is no third state, and it cannot be changed later.
- Replace the entry that names the resolved theme, do not rename it in place. A picker's
  closed face reads its item once and does not listen for a property change on it.
- On Linux the preference comes from the desktop portal's appearance setting, and which
  desktop component serves that setting varies. On a Cinnamon session it is
  `org.x.apps.portal color-scheme` that the portal reports, not
  `org.gnome.desktop.interface color-scheme`; changing the latter has no effect on what the
  application sees. If the portal is missing entirely the platform assumes light.

### Build a grouped list from group and row view models

**When you want this.** Results belong under headings - files under a folder, issues
under a repository, messages under a day - and the headings and the rows are both
clickable.

**The MVVM shape.** Two item view models. The group carries its heading, its count and
an `ObservableCollection` of rows; the row carries everything its own line draws, worked
out once in its constructor. The page is an outer `ListView` bound to the groups whose
item template holds the heading and an inner `ItemsControl` bound to that group's rows.
Selection is off: nothing here is selected, only clicked, so each heading and each row is
a stretched transparent `Button` whose `Command` is the item's own.

**Code.**

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/RepositoryGroupViewModel.cs
[Microsoft.UI.Xaml.Data.Bindable]
public class RepositoryGroupViewModel : SimpleViewModel
{
    public RepositoryGroupViewModel(string fullName, string htmlUrl, Func<string, Task> openUrlAsync)
    {
        FullName = fullName ?? string.Empty;
        Url = htmlUrl ?? string.Empty;
        _openUrlAsync = openUrlAsync;
        Rows = new ObservableCollection<IssueRowViewModel>();
        CountText = "0";
    }

    public string FullName { get; }

    public ObservableCollection<IssueRowViewModel> Rows { get; }

    public string CountText
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public SimpleCommand OpenCommand => _openCommand ??=
        new SimpleCommand((Func<object, Task>)(_ => OpenAsync()));

    public void Add(IssueRowViewModel row)
    {
        if (row == null) { return; }

        Rows.Add(row);
        CountText = Rows.Count.ToString("N0", CultureInfo.InvariantCulture);
    }
}
```

The row does its formatting once, so the template binds to plain strings and
visibilities and a list of thousands of rows stays cheap:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/IssueRowViewModel.cs
public IssueRowViewModel(IssueItem item, ColorSchemePalette palette, bool showAssignees,
    DateTimeOffset now, Func<string, Task> openUrlAsync)
{
    if (item == null) { throw new ArgumentNullException(nameof(item)); }

    _openUrlAsync = openUrlAsync;

    Url = item.HtmlUrl ?? string.Empty;
    Title = item.Title ?? string.Empty;
    IsPullRequest = item.Kind == IssueKind.PullRequest;
    PullRequestChipVisibility = GetVisibility(IsPullRequest);

    CommentCountText = item.CommentCount.ToString("N0", CultureInfo.InvariantCulture);
    CommentVisibility = GetVisibility(item.CommentCount > 0);

    MetaText = BuildMeta(item, showAssignees, now);
    MetaToolTip = BuildToolTip(item);

    (StateGlyph, _stateRole) = DescribeState(item);
    // ... build the label pills ...
}
```

The markup is one template inside another. The outer list turns selection off and
flattens its containers so the group draws edge to edge:

```xml
<!-- From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml -->
<ListView Visibility="{d:Binding ResultsVisibility}"
          SelectionMode="None"
          ItemsSource="{d:Binding Groups}"
          Padding="0"
          HorizontalContentAlignment="Stretch">
  <ListView.ItemContainerStyle>
    <ui:Style TargetType="c:ListViewItem"
              BasedOn="{StaticResource DefaultListViewItemStyle}">
      <ui:Setter Property="Padding" Value="0" />
      <ui:Setter Property="MinHeight" Value="0" />
      <ui:Setter Property="HorizontalContentAlignment" Value="Stretch" />
    </ui:Style>
  </ListView.ItemContainerStyle>
  <ListView.ItemTemplate>
    <ui:DataTemplate>
      <StackPanel>
        <!-- The group header is itself a button, so hover and
             press come from the re-keyed Button brushes. -->
        <Button HorizontalAlignment="Stretch"
                HorizontalContentAlignment="Stretch"
                Background="{StaticResource CanvasSubtleBrush}"
                BorderBrush="{StaticResource HairlineMutedBrush}"
                BorderThickness="0,0,0,1"
                CornerRadius="0" Padding="16,8"
                Command="{d:Binding OpenCommand}">
          <!-- ... the repository glyph, its name, the count pill ... -->
        </Button>

        <ItemsControl ItemsSource="{d:Binding Rows}">
          <ItemsControl.ItemTemplate>
            <ui:DataTemplate>
              <Button HorizontalAlignment="Stretch"
                      HorizontalContentAlignment="Stretch"
                      Background="{StaticResource TransparentBrush}"
                      BorderBrush="{StaticResource HairlineMutedBrush}"
                      BorderThickness="0,0,0,1"
                      CornerRadius="0" Padding="16,10"
                      ToolTipService.ToolTip="{d:Binding MetaToolTip}"
                      Command="{d:Binding OpenCommand}">
                <!-- ... the state glyph, the title and its label pills, the meta line ... -->
              </Button>
            </ui:DataTemplate>
          </ItemsControl.ItemTemplate>
        </ItemsControl>
      </StackPanel>
    </ui:DataTemplate>
  </ListView.ItemTemplate>
</ListView>
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/RepositoryGroupViewModel.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/IssueRowViewModel.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Fill a new group before it goes into the bound collection. A group inserted empty and
  filled a moment later can be measured while it is still empty, and draws as a bare
  heading until something else forces a fresh layout.
- Both item view models need `[Microsoft.UI.Xaml.Data.Bindable]`, not just the page's
  view model.
- A template binds to its own item, so put the command on the item. A group's command and
  a row's command can both be called `OpenCommand` and each template gets the right one.
- Flatten the `ListViewItem` container - zero padding, zero minimum height, stretched
  content - or the theme's own row metrics show through as gaps between your groups.
- From the second time the list is filled onwards, the platform reports one binding
  resolution per property of the inner templates against the outer item type - the row
  template resolved once against a group, the label template once against a row - as it
  recycles containers between items. It is logged at error level in a Debug build, once
  per name, and never grows with the number of rows; every value on screen is correct.
  Bind each template only to members of its own item type and let it be.

### Dim a list row for an item the application cannot act on

**When you want this.** Some rows in a list are still selectable and still useful,
but one thing cannot be done with them, and you want that visible without hiding
them.

**The MVVM shape.** A bool on the item model, the platform toolkit's
`BoolToObjectConverter` declared in `Page.Resources` with two real `Double`
values, and one `Opacity` binding on the row's outermost element so the whole row
dims together.

**Code.**

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<!-- A file this application cannot play is still listed, still selectable and still a
     conversion source - it is only shown dimmed, so the rows that can be played stand out.
     The two stops are real doubles rather than strings so the row's Opacity takes them
     without a conversion of its own. -->
<cv:BoolToObjectConverter x:Key="PlayableOpacity">
    <cv:BoolToObjectConverter.TrueValue>
        <x:Double>1.0</x:Double>
    </cv:BoolToObjectConverter.TrueValue>
    <cv:BoolToObjectConverter.FalseValue>
        <x:Double>0.45</x:Double>
    </cv:BoolToObjectConverter.FalseValue>
</cv:BoolToObjectConverter>

<ui:DataTemplate x:Key="LibraryItemTemplate">
    <!-- One Opacity on the row dims the badge, the name and the summary together. The name is
         how the scripted run finds a row and reads the opacity it is really shown at. -->
    <Grid x:Name="LibraryRow"
          Padding="2,6"
          Opacity="{d:Binding IsPlayable, Converter={StaticResource PlayableOpacity}}">
        <!-- ... a format badge in a Border, then the file name and summary ... -->
    </Grid>
</ui:DataTemplate>
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Probing/SourceMediaInfo.cs`

**Sharp edges.**
- The two stops are declared as `<x:Double>` elements, not strings, so the row's
  `Opacity` takes them without a conversion of its own.
- The converter comes from the platform toolkit's converters namespace; its XAML
  prefix is separate from the one for your own converters.
- Naming the row element lets a scripted run walk the visual tree and read the
  opacity actually applied, rather than trusting the converter.

### Show a relative date with the exact one in a ToolTip

**When you want this.** A list reads better with "3 days ago" than with a
timestamp, and the exact moment still has to be reachable without leaving the
row.

**The MVVM shape.** The row view model builds both strings once, in its
constructor: the relative phrase for the line it draws, and a multi-line exact
form for the tooltip. The phrasing itself is a plain static helper in the
UI-free library that takes the current moment as a parameter, so it is testable
and so every row folded from one page of results measures from the same instant.
The markup binds the tooltip to the row's outermost element.

**Code.**

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Helpers/RelativeTime.cs
/// <summary>
/// Turns a moment into the short phrase GitHub shows beside an issue, for example
/// "3 days ago". Plain text work with the clock passed in, so it is straightforward to test.
/// </summary>
public static class RelativeTime
{
    public static string Describe(DateTimeOffset when, DateTimeOffset now)
    {
        var elapsed = now - when;
        if (elapsed < TimeSpan.Zero) { return "just now"; }
        if (elapsed.TotalSeconds < 60d) { return "just now"; }

        var minutes = (int)elapsed.TotalMinutes;
        if (minutes < 60) { return Phrase(minutes, "minute"); }

        // ... hours, then "yesterday", then days, weeks, months and years ...
    }
}
```

The row builds the visible line and the tooltip beside each other, so the two
cannot describe different fields:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/IssueRowViewModel.cs
private static string BuildToolTip(IssueItem item)
{
    var text = new StringBuilder();
    text.Append("Opened ").Append(item.CreatedAt.ToLocalTime().ToString("f", CultureInfo.CurrentCulture));
    text.Append("\nUpdated ").Append(item.UpdatedAt.ToLocalTime().ToString("f", CultureInfo.CurrentCulture));

    if (item.ClosedAt.HasValue)
    {
        text.Append("\nClosed ")
            .Append(item.ClosedAt.Value.ToLocalTime().ToString("f", CultureInfo.CurrentCulture));
    }

    return text.ToString();
}
```

The tooltip goes on the row itself, which here is the button the whole row is
made of:

```xml
<!-- From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml -->
<Button HorizontalAlignment="Stretch"
        HorizontalContentAlignment="Stretch"
        Background="{StaticResource TransparentBrush}"
        BorderBrush="{StaticResource HairlineMutedBrush}"
        BorderThickness="0,0,0,1"
        CornerRadius="0" Padding="16,10"
        ToolTipService.ToolTip="{d:Binding MetaToolTip}"
        Command="{d:Binding OpenCommand}">
```

**Where to look.**
`GitHubIssueFinder/src/libs/GitHubIssueFinder.GitHub/Helpers/RelativeTime.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/IssueRowViewModel.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml`
`GitHubIssueFinder/tests/libs/GitHubIssueFinder.GitHub.Tests/RelativeTimeTests.cs`

**Sharp edges.**
- Pass the clock in rather than reading it inside the helper. That is what makes
  the phrasing testable without waiting, and it is what stops rows folded from one
  page disagreeing about what "now" was.
- Build both strings once in the constructor. A converter would run again on every
  realization of a recycled row, for a value that cannot change.
- Put the tooltip on the row's outermost element so it appears wherever in the row
  the pointer rests, not only over the line of text it describes.
- Show the exact form in local time and in the running culture, and keep the
  relative form in words that need no culture at all.
- The relative phrase is an approximation by design - months of thirty days, years
  of three hundred and sixty-five - and saying so in the helper is honest, because
  the exact value is one hover away.

### Format a value for display with an IValueConverter

**When you want this.** A `TimeSpan`, or any other value, has to appear in a
particular textual form.

**The MVVM shape.** An `IValueConverter` in the library that carries the
application's view types, declared once in `Page.Resources` and used by key.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Converters/TimecodeConverter.cs
public sealed class TimecodeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not TimeSpan time || time < TimeSpan.Zero)
        {
            return "0:00";
        }

        return time.TotalHours >= 1
            ? string.Create(CultureInfo.InvariantCulture, $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}")
            : string.Create(CultureInfo.InvariantCulture, $"{(int)time.TotalMinutes}:{time.Seconds:00}");
    }

    /// <summary>Not supported: a timecode is never typed back into the player.</summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException("A timecode is shown, never entered.");
}
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<Page.Resources>
    <conv:TimecodeConverter x:Key="Timecode" />
</Page.Resources>
```

The same idea with a different precision, chosen for what the data actually looks
like:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
/// <summary>
/// Formats an AudioPlayer position/duration <see cref="TimeSpan"/> for the audio scrubber's
/// two timecode labels. The tenth of a second is deliberate: most of what an asset pack ships
/// is a sound effect well under a second long, and a plain m:ss would show "0:00 / 0:00" for
/// the whole clip.
/// </summary>
public sealed class TimecodeConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is TimeSpan time ? $"{(int)time.TotalMinutes}:{time.Seconds:00}.{time.Milliseconds / 100}" : "0:00.0";

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Converters/TimecodeConverter.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Converters/NullToVisibilityConverter.cs`
(shows an element only while a bound value is null, with any converter parameter
inverting it)

**Sharp edges.**
- `IValueConverter` here comes from the platform's data namespace, and its
  `language` parameter is a `string`.
- Return a safe default rather than throwing when the value is the wrong type or
  out of range, so a binding that is briefly wrong does not break the page.
- Use an invariant culture for anything with fixed separators.
- A one-way formatter that throws from `ConvertBack` is correct for a label but
  would break if the same converter were ever attached to a two-way binding.

### Highlight the selected button with a value converter

**When you want this.** A row of buttons behaves like a radio group and the
selected one should carry the accent style.

**The MVVM shape.** The view model exposes one bool per option. A converter maps
`true` to the application's accent style resource and everything else to `null`,
which is the default style; the buttons bind `Style` through it.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Converters/BoolToAccentStyleConverter.cs
public sealed class BoolToAccentStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool selected && selected
            && Application.Current is { } app
            && app.Resources.TryGetValue("AccentButtonStyle", out var resource)
            && resource is Style style)
        {
            return style;
        }

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
```

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml -->
<Page.Resources>
    <conv:BoolToAccentStyleConverter x:Key="SelectedButtonStyle" />
</Page.Resources>
<!-- ... -->
<Button Content="Sample Texture" Command="{d:Binding SelectTextureCommand}" MinWidth="140"
        Style="{d:Binding IsTextureSelected, Converter={StaticResource SelectedButtonStyle}}" />
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Converters/BoolToAccentStyleConverter.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml`

**Sharp edges.**
- The converter looks the style up defensively and returns `null` rather than
  throwing when the resource is missing.
- The converter lives in the application library, so the XAML reaches it with a
  `clr-namespace:...;assembly=...` declaration.
- The selection booleans are not auto-notifying; the view model raises all of them
  together from one helper, which also raises anything else that follows the
  selection.

### Bind a scrubber and volume slider straight to the media element

**When you want this.** A value ticks many times a second and routing it through
the view model would buy nothing.

**The MVVM shape.** This is the documented exception to "everything through the
view model". Position, duration, volume and mute are dependency properties on the
element, so the transport binds to them by `ElementName` and the view model owns
only the decisions. The interface the view model drives the element through
deliberately omits them.

**Code.**

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<TextBlock Grid.Column="0"
           Text="{d:Binding Position, ElementName=Player, Converter={StaticResource Timecode}}"
           Width="58"
           FontSize="12"
           VerticalAlignment="Center"
           Foreground="{StaticResource AppTextBrush}" />

<Slider Grid.Column="1"
        Maximum="{d:Binding DurationSeconds, ElementName=Player}"
        Value="{d:Binding PositionSeconds, ElementName=Player, Mode=TwoWay}"
        StepFrequency="0.1"
        VerticalAlignment="Center"
        Margin="6,0" />

<!-- ... -->

<CheckBox Content="Mute"
          IsChecked="{d:Binding IsMuted, ElementName=Player, Mode=TwoWay}"
          VerticalAlignment="Center" />
<Slider Width="130"
        Minimum="0"
        Maximum="1"
        StepFrequency="0.01"
        Value="{d:Binding Volume, ElementName=Player, Mode=TwoWay}"
        VerticalAlignment="Center" />
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/IVideoPlayerSurface.cs
/// <remarks>
/// The element itself is a XAML control and can only live in the view layer, so the page implements
/// this and hands it to the view model. Position, duration and volume are deliberately absent: those
/// are dependency properties on the element, and the scrubber and the volume slider bind straight to
/// them, which is both simpler and smoother than routing every tick through a view model. What the
/// view model owns is everything that is a decision rather than a value.
/// </remarks>
```

The same shape for an audio transport:

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
<!-- Position tracker: elapsed · scrubber · duration, bound
     straight to the AudioPlayer element rather than through the
     view model. The Slider follows playback via the two-way
     PositionSeconds binding, and dragging it seeks the clip
     (the add-in debounces to one seek on thumb release). -->
<StackPanel Orientation="Horizontal" Spacing="10"
            HorizontalAlignment="Center">
    <TextBlock Width="52" VerticalAlignment="Center"
               FontSize="12" TextAlignment="Right"
               Foreground="{StaticResource TextSecondaryBrush}"
               Text="{d:Binding Position, ElementName=AudioElement, Converter={StaticResource TimecodeConverter}}" />
    <Slider Width="300" VerticalAlignment="Center"
            StepFrequency="0.01"
            Maximum="{d:Binding DurationSeconds, ElementName=AudioElement}"
            Value="{d:Binding PositionSeconds, ElementName=AudioElement, Mode=TwoWay}" />
    <TextBlock Width="52" VerticalAlignment="Center"
               FontSize="12"
               Foreground="{StaticResource TextTertiaryBrush}"
               Text="{d:Binding Duration, ElementName=AudioElement, Converter={StaticResource TimecodeConverter}}" />
</StackPanel>
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/IVideoPlayerSurface.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml`

**Sharp edges.**
- Both add-in elements expose the same value twice: as a `TimeSpan` for the
  labels, through a converter, and as a `double` in seconds for the slider, so
  nothing has to convert both ways.
- Dragging the thumb seeks. The add-ins debounce a drag down to one seek on
  release, so a two-way binding does not flood the decoder.
- The transport bar's own visibility still comes from the view model, so the rule
  about when a transport exists stays testable even though the values inside it do
  not go through it.

### Switch a page between two modes with one bool and a converter

**When you want this.** A page has two mutually exclusive states, each with its
own main visual and its own buttons, and you do not want a second page or a
navigation stack.

**The MVVM shape.** One bound bool on the view model, with a computed inverse and
`[AffectsCommands]` naming every command it gates. The page declares the same
converter twice - once plain, once with `Invert="True"` - and binds both halves of
the UI to the same property. No code-behind at all.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
[AffectsCommands(nameof(TakePhotoCommand), nameof(BackCommand), nameof(ClearCommand),
    nameof(SaveCommand), nameof(SelectColorCommand))]
public bool IsCaptureMode
{
    get;
    private set
    {
        SetProperty(ref field, value);
        NotifyPropertyChanged(nameof(IsPaintMode));
    }
} = true;

/// <summary>Paint Mode is simply not-Capture Mode.</summary>
public bool IsPaintMode => !IsCaptureMode;
```

```xml
<!-- From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml -->
xmlns:c="clr-namespace:CodeBrix.Platform.UI.Converters;assembly=CodeBrix.Platform.UI.Toolkit"
...
<Page.Resources>
    <c:BoolToVisibilityConverter x:Key="VisibleWhenTrue" />
    <c:BoolToVisibilityConverter x:Key="VisibleWhenFalse" Invert="True" />
</Page.Resources>

<!-- The main viewer: the mirrored live preview in Camera Mode; the palm-reactive
     shader visual in Visualize Mode -->
<Border Grid.Row="1" BorderBrush="Gray" BorderThickness="1" Background="Black">
    <Grid>
        <camera:CameraCanvas x:Name="PreviewCanvas"
                             Visibility="{d:Binding IsCameraMode, Converter={StaticResource VisibleWhenTrue}}" />
        <game:GameSurfaceCanvas x:Name="VisualizerCanvas"
                                Visibility="{d:Binding IsCameraMode, Converter={StaticResource VisibleWhenFalse}}" />
    </Grid>
</Border>

<Grid Grid.Row="2" Margin="0,8,0,0">
    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Spacing="8"
                Visibility="{d:Binding IsCameraMode, Converter={StaticResource VisibleWhenTrue}}">
        <Button Content="Visualize!" Command="{d:Binding VisualizeCommand}"
                MinWidth="120" Style="{ThemeResource AccentButtonStyle}" />
    </StackPanel>

    <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Spacing="8"
                Visibility="{d:Binding IsCameraMode, Converter={StaticResource VisibleWhenFalse}}">
        <Button Content="Back" Command="{d:Binding BackCommand}" MinWidth="100" />
    </StackPanel>
</Grid>

<TextBlock Grid.Row="3" Text="{d:Binding StatusText}" Margin="0,8,0,0" TextWrapping="Wrap" />
```

**Where to look.**
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml`
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs` and
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml`

**Sharp edges.**
- Registering one converter twice with different keys, the second with
  `Invert="True"`, is the convention across the platform's converters; it avoids a
  second converter type and a negated view-model property.
- A computed inverse property needs an explicit `NotifyPropertyChanged` from the
  setter it derives from, because `SetProperty` only raises for its own name.
- Both visuals live in the same grid cell and are stacked, differing only in
  visibility. That is what keeps a long-lived canvas alive - and its engine merely
  paused - across mode switches.
- A control that should stay put is disabled rather than hidden
  (`IsEnabled="{d:Binding IsCameraMode}"`), so the layout does not shift.
- Where the panes are more than two, computed `Visibility` properties on the view
  model are the tidier form; see the view-model area.

### Show a panel only when the last operation left something to say

**When you want this.** An output area that must take no room at all until there
is something in it, and must be emptied when the next operation starts.

**The MVVM shape.** An `ObservableCollection<string>` on the view model plus a
derived `Visibility`, refilled by a private setter that notifies the visibility.
The XAML binds an `ItemsControl` inside a `Border` whose `Visibility` is bound.
The line-building rule is a public static method, so it is testable without a view
model.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs
public ObservableCollection<string> LastRunNotes { get; } = new();

public Visibility LastRunNotesVisibility => GetVisibility(LastRunNotes.Count > 0);

public static IReadOnlyList<string> DescribeOutcome(ConversionOutcome outcome, MediaFormatKind destination)
{
    if (outcome is null)
    {
        return [];
    }

    var lines = new List<string>();

    if (!string.IsNullOrWhiteSpace(outcome.ProfileVerdict))
    {
        //A standard MKV is written with its cues at the end and is EXPECTED to fail; it is checked
        //and reported on all the same, and the failure is not an error.
        var expected = destination == MediaFormatKind.Matroska
            ? " (expected for a standard MKV)"
            : string.Empty;

        lines.Add(outcome.PassesProfile
            ? "Streamable profile: PASS"
            : $"Streamable profile: FAIL - {outcome.ProfileVerdict}{expected}");
    }

    lines.AddRange(outcome.Notes);
    return lines;
}

private void SetLastRunNotes(IReadOnlyList<string> lines)
{
    if (LastRunNotes.Count == 0 && lines.Count == 0)
    {
        return;
    }

    LastRunNotes.Clear();
    foreach (var line in lines)
    {
        LastRunNotes.Add(line);
    }

    NotifyPropertyChanged(nameof(LastRunNotesVisibility));
}
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<!-- One line of what the last conversion had to say. The bound item IS the line, so this
     template binds the string itself rather than a property of it. -->
<ui:DataTemplate x:Key="RunNoteTemplate">
    <TextBlock Text="{d:Binding}" FontSize="11" TextWrapping="Wrap"
               Margin="0,2,0,0" Foreground="{StaticResource AppMutedTextBrush}" />
</ui:DataTemplate>

<!-- ... -->

<Border Grid.Row="4"
        Background="{StaticResource AppRaisedPanelBrush}"
        BorderThickness="0,1,0,0"
        Padding="20,6,20,10"
        Visibility="{d:Binding Conversion.LastRunNotesVisibility}">
    <ItemsControl x:Name="LastRunNotesList"
                  ItemsSource="{d:Binding Conversion.LastRunNotes}"
                  ItemTemplate="{StaticResource RunNoteTemplate}" />
</Border>
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml`

**Sharp edges.**
- The bound item is the line itself, so the template binds `{d:Binding}` with no
  path.
- A `Visibility` derived from a collection has to be notified by hand whenever the
  collection changes, because a collection change is not a property change.
- Empty the collection the moment the next operation starts, so what is on screen
  always belongs to the operation named in the status bar.
- Making the line-building rule static is what makes it testable: a
  `SimpleViewModel` cannot be constructed in a test process, but a static method
  on one can be called.

### Load an SVG or bitmap from an embedded resource with a custom URI scheme

**When you want this.** Vector icons that ship inside the assembly, referenced
from XAML by name, with no file paths and no per-head asset pipeline.

**The MVVM shape.** A `FrameworkElement` subclass with a string dependency
property. The control does all the loading; the page just names the resource, and
the view model knows nothing about images.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs
public sealed class EmbeddedImage : Image
{
    public static readonly DependencyProperty UriSourceProperty =
        DependencyProperty.Register(
            nameof(UriSource), typeof(string), typeof(EmbeddedImage),
            new PropertyMetadata(null, OnUriSourceChanged));

    public string UriSource
    {
        get => (string)GetValue(UriSourceProperty);
        set => SetValue(UriSourceProperty, value);
    }

    private static void OnUriSourceChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e)
        => _ = LoadImageAsync((EmbeddedImage)d, e.NewValue as string);

    private static async Task LoadImageAsync(EmbeddedImage image, string uri)
    {
        // ...
        if (uri.StartsWith("embedded://", StringComparison.OrdinalIgnoreCase))
        {
            // Parse: embedded://AssemblyName/Fully.Qualified.Resource.Name
            var path = uri["embedded://".Length..];
            var separatorIndex = path.IndexOf('/');
            // ...
            var assemblyName = path[..separatorIndex];
            var resourceName = path[(separatorIndex + 1)..];

            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName)
                ?? throw new InvalidOperationException(
                    $"Assembly '{assemblyName}' is not loaded.");

            await using var resourceStream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"Resource '{resourceName}' not found in '{assemblyName}'.");

            // Copy embedded resource into an IRandomAccessStream.
            // Note: ras and writeStream are intentionally not disposed here.
            var ras = new InMemoryRandomAccessStream();
            var writeStream = ras.AsStreamForWrite();
            await resourceStream.CopyToAsync(writeStream);
            await writeStream.FlushAsync();
            ras.Seek(0);

            // Use SvgImageSource for .svg files, BitmapImage for everything else
            if (resourceName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                var svgSource = new SvgImageSource();
                await svgSource.SetSourceAsync(ras);
                image.Source = svgSource;
            }
            else
            {
                var bitmapSource = new BitmapImage();
                await bitmapSource.SetSourceAsync(ras);
                image.Source = bitmapSource;
            }
        }
        // ... otherwise fall back to SvgImageSource or BitmapImage with a plain UriSource ...
    }
}
```

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml -->
<controls:EmbeddedImage Margin="20,0,0,0" Width="60" Height="60"
    VerticalAlignment="Center"
    UriSource="embedded://JustBetweenUs.Core/JustBetweenUs.Assets.padlock-icon.svg" />
```

**Where to look.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml`

**Sharp edges.**
- The stream-ownership comment is a real ordering rule. Disposing the write stream
  closes the underlying random-access stream, and disposing the random-access
  stream is unsafe because the source may keep a reference to it rather than
  copying. Both are left to the garbage collector, which is safe because the
  in-memory stream holds no file or unmanaged handles.
- The assembly is found by scanning already-loaded assemblies. If nothing has
  touched the assembly holding the resource it will not be loaded and the lookup
  throws; referencing a type from that assembly keeps it loaded.
- Load failures are caught and written to the debug output, so a wrong resource
  name shows an empty image with no visible error. Watch the debug output when an
  icon does not appear.
- The custom URI scheme is not understood by XAML designers; the sample keeps a
  comment in the page saying the tooling flags it but it works at run time.

### Build a button that combines an embedded image with text

**When you want this.** Toolbar-style buttons with an icon above, below, left or
right of a caption, driven by a command.

**The MVVM shape.** A `Button` subclass with dependency properties for the image
URI, the text, the image position, spacing and image size. It rebuilds its own
`Content` whenever any of them changes, and the page binds `Command` to the view
model as usual.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImageButton.cs
public sealed class EmbeddedImageButton : Button
{
    public EmbeddedImageButton()
    {
        DefaultStyleKey = typeof(Button);
        CornerRadius = new CornerRadius(4);
    }

    // ... ImageUriSource, Text, ImagePosition, Spacing, ImageWidth, ImageHeight,
    //     TextVerticalAlignment and TextHorizontalAlignment dependency properties,
    //     all registered with OnLayoutPropertyChanged ...

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        if (!_isUpdatingContent && newContent is string text)
        {
            text = text.Trim();
            if (text.Length > 0)
            {
                Text = text;
            }
        }
    }

    private void UpdateContent()
    {
        _isUpdatingContent = true;
        try
        {
            var hasImage = !string.IsNullOrWhiteSpace(ImageUriSource);
            var hasText = !string.IsNullOrWhiteSpace(Text);

            if (!hasImage && !hasText) { Content = null; return; }

            if (hasImage && hasText)
            {
                var isHorizontal = ImagePosition is ImagePosition.Left or ImagePosition.Right;
                var imageFirst = ImagePosition is ImagePosition.Left or ImagePosition.Top;

                var panel = new StackPanel
                {
                    Orientation = isHorizontal ? Orientation.Horizontal : Orientation.Vertical,
                    Spacing = Spacing
                };

                panel.Children.Add(imageFirst ? CreateImage() : CreateTextBlock());
                panel.Children.Add(imageFirst ? CreateTextBlock() : CreateImage());

                Content = panel;
            }
            else if (hasImage) { Content = CreateImage(); }
            else { Content = CreateTextBlock(); }
        }
        finally
        {
            _isUpdatingContent = false;
        }
    }
}
```

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml -->
<controls:EmbeddedImageButton Margin="0,0,20,0" Width="140" Height="90"
    VerticalAlignment="Center" HorizontalAlignment="Right"
    Background="#FFB85555"
    Command="{d:Binding EncryptCommand}"
    ImageUriSource="embedded://JustBetweenUs.Core/JustBetweenUs.Assets.padlock-icon.svg"
    Text="Encrypt" ImageWidth="40" ImageHeight="40" Spacing="6" ImagePosition="Top" />

<controls:EmbeddedImageButton Grid.Row="4" Grid.Column="0" Grid.ColumnSpan="2" Width="220" Height="50"
    VerticalAlignment="Center" HorizontalAlignment="Center"
    Background="#FFB85555"
    Command="{d:Binding CopyToClipboardCommand}"
    ImageUriSource="embedded://JustBetweenUs.Core/JustBetweenUs.Assets.clipboard.svg">
    Copy to Clipboard
</controls:EmbeddedImageButton>
```

**Where to look.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImageButton.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/ImagePosition.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml`

**Sharp edges.**
- `OnContentChanged` is overridden so XAML element content - text written between
  the opening and closing tags - is treated as the `Text` property instead of
  replacing the composed panel. A guard flag stops the override fighting the
  rebuild.
- `DefaultStyleKey = typeof(Button)` makes the subclass pick up the standard
  button template rather than needing its own.
- The native WinUI 3 head does not use this control: an equivalent with the same
  property names and the same URI scheme ships in the platform's WinUI Skia
  add-in, so the same markup works there with a different XML namespace.

### Wrap and reflow a layout with the FlexPanel add-in

**When you want this.** A toolbar or header whose groups should stay on one line
while the window is wide and fold onto a second line when it is not, or a two-pane
layout that should be side by side on a wide window and stacked on a tall one -
without a breakpoint or a converter.

**The MVVM shape.** Pure layout. Each group is one child of the panel;
`FlexPanel.Grow` decides who absorbs the slack and `FlexPanel.Basis` makes the
wrap point deterministic. Flipping the main axis on `SizeChanged` is one line of
layout plumbing rather than application logic; if the orientation matters to
anything else, put an `IsPortrait` property on the view model and set it from the
same handler.

**Code.**

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
xmlns:flex="clr-namespace:CodeBrix.Platform.UI.FlexPanel;assembly=CodeBrix.Platform.UI.FlexPanel"
```

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
<!-- Header: identity on the left; search, category filter and the assets folder
     on the right. A wrapping FlexPanel keeps everything on one row while the
     window is wide enough. -->
<flex:FlexPanel Direction="Row" Wrap="Wrap" AlignItems="Center">

    <!-- Grow=1: the identity block soaks up the free main-axis space, keeping
         the other groups pinned right while they still share its row -->
    <StackPanel Spacing="2" Margin="0,6,16,6" flex:FlexPanel.Grow="1">
        <!-- ... title and strapline ... -->
    </StackPanel>

    <!-- Search and the category filter travel as one unit when the panel wraps -->
    <StackPanel Orientation="Horizontal" Spacing="12" Margin="0,6,16,6">
        <TextBox Width="240" VerticalAlignment="Center"
                 PlaceholderText="Search assets…"
                 Text="{d:Binding SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                 CornerRadius="8" />
        <ComboBox Width="190" VerticalAlignment="Center"
                  CornerRadius="8"
                  ItemsSource="{d:Binding Categories}"
                  SelectedItem="{d:Binding SelectedCategory, Mode=TwoWay}" />
    </StackPanel>

    <Button CornerRadius="8" Padding="14,8" Margin="0,6,0,6" BorderThickness="1"
            MaxWidth="300"
            Command="{d:Binding PickFolderCommand}">
        <!-- ... folder glyph and AssetsFolderLabel ... -->
    </Button>
</flex:FlexPanel>
```

Where the wrap point must be predictable rather than content-dependent, set a
basis:

```xml
<!-- From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml -->
<flex:FlexPanel Direction="Row" Wrap="Wrap" AlignItems="Center">

    <!-- Save-target group; Grow=1 so the path box stretches into whatever
         width its row has, Basis so the wrap point is deterministic -->
    <Grid Margin="0,4,16,4" ColumnSpacing="10"
          flex:FlexPanel.Grow="1" flex:FlexPanel.Basis="420">
        <!-- ... label, path TextBox, Select button ... -->
    </Grid>

    <!-- Page-size + Create! group -->
    <StackPanel Orientation="Horizontal" Spacing="10" Margin="0,4,0,4">
        <!-- ... label, ComboBox, primary button ... -->
    </StackPanel>
</flex:FlexPanel>
```

Flipping the main axis turns a side-by-side split into a stack:

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml -->
<flex:FlexPanel x:Name="ModelContentFlex" Grid.Row="1" Padding="24,20,24,8"
                Direction="Row">
  <!-- Explicit Width (not FlexPanel.Basis) in landscape: the pane's content is
       measured against it, so the text inside wraps at the pane width -->
  <ScrollViewer x:Name="ModelInfoPane" VerticalScrollBarVisibility="Auto"
                Margin="0,0,20,0" Width="420"> <!-- ... --> </ScrollViewer>

  <!-- Grow=1: the viewer takes whatever main-axis space the info pane leaves -->
  <Grid RowSpacing="8" flex:FlexPanel.Grow="1"> <!-- ... --> </Grid>
</flex:FlexPanel>
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
//The Model View's content panes: side-by-side while the window is landscape. In
//portrait the FlexPanel's main axis flips so the 3D viewer drops below the info
//panes, and the info panes trade their fixed-width column (an explicit Width, so
//their content measures - and wraps - against it) for half the height as a flex
//basis, still scrolling internally.
SizeChanged += (_, args) =>
{
    var portrait = args.NewSize.Width < args.NewSize.Height;
    ModelContentFlex.Direction = portrait ? FlexDirection.Column : FlexDirection.Row;
    ModelInfoPane.Width = portrait ? double.NaN : 420;
    FlexPanel.SetBasis(ModelInfoPane,
        portrait ? new FlexBasis(0.5f, isRelative: true) : FlexBasis.Auto);
    ModelInfoPane.Margin = portrait ? new Thickness(0, 0, 0, 20) : new Thickness(0, 0, 20, 0);
};
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml` and
`Views/MainPage.xaml.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml` and
`Views/MainPage.xaml.cs`

**Also shown by.**
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml`
(the bottom bar; the native WinUI 3 and WPF heads lay the same bar out with a
six-column `Grid`, so the reflow is a Skia-head behavior)

**Sharp edges.**
- Group the controls that must wrap together into one child. The panel wraps
  children, not their contents.
- `Grow` and `Basis` are attached properties on the child, not on the panel.
- An explicit `Width` and a `FlexPanel.Basis` are not interchangeable. Content is
  measured against a `Width`, so text wraps to the pane; a basis sizes the box
  without giving the content that constraint. The samples use `Width` in landscape
  and a relative basis in portrait, swapping them on the same element.
- The margin has to move with the axis: a right margin in landscape, a bottom
  margin in portrait.
- A `MaxWidth` on a control whose content can be arbitrarily long (a chosen path,
  for instance) keeps it from consuming the row before the panel can wrap.
- The add-in is referenced once in the library that carries the application's
  packages, and it has its own assembly and XAML namespace.

### Bind a TreeView to a view model tree with checkboxes

**When you want this.** A hierarchy where the user checks arbitrary nodes and taps
a row to see details, without the tree owning the selection semantics.

**The MVVM shape.** `TreeView.ItemsSource` binds to the root collection and an
`ItemTemplate` produces a `TreeViewItem` per node bound to the node view model.
`IsExpanded` is two-way bound so the view model learns about expansion, the
checkbox is two-way bound, and the row's tap target is a transparent `Button`
bound to the node's own command, so the tree's own selection mode can be turned
off entirely.

**Code.**

```xml
<!-- From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml -->
<!-- One row of the page tree: explicit checkbox (independent selection — no
     parent/child propagation), the page icon in a rounded well, and the title.
     Tapping the title area (not the checkbox) previews the page. -->
<ui:DataTemplate x:Key="PageNodeTemplate">
    <TreeViewItem ItemsSource="{d:Binding Children}"
                  IsExpanded="{d:Binding IsExpanded, Mode=TwoWay}">
        <StackPanel Orientation="Horizontal" Spacing="6">
            <CheckBox IsChecked="{d:Binding IsChecked, Mode=TwoWay}"
                      MinWidth="0"
                      Visibility="{d:Binding CheckBoxVisibility}" />
            <Button Background="Transparent" BorderThickness="0" Padding="6,3"
                    CornerRadius="6"
                    Command="{d:Binding SelectCommand}">
                <StackPanel Orientation="Horizontal" Spacing="9">
                    <Border Width="26" Height="26" CornerRadius="6"
                            Background="{StaticResource CardWellBrush}"
                            VerticalAlignment="Center">
                        <Grid>
                            <FontIcon Glyph="{d:Binding KindGlyph}" FontSize="12"
                                      Foreground="{StaticResource AccentDimBrush}"
                                      HorizontalAlignment="Center" VerticalAlignment="Center"
                                      Visibility="{d:Binding IconGlyphVisibility}" />
                            <Image Source="{d:Binding IconImageSource}" Stretch="UniformToFill"
                                   Visibility="{d:Binding IconImageVisibility}" />
                        </Grid>
                    </Border>
                    <TextBlock Text="{d:Binding Title}" FontSize="14"
                               Foreground="{StaticResource TextPrimaryBrush}"
                               VerticalAlignment="Center"
                               TextTrimming="CharacterEllipsis" MaxLines="1" />
                </StackPanel>
            </Button>
        </StackPanel>
    </TreeViewItem>
</ui:DataTemplate>
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/NotionPageNodeViewModel.cs
/// <summary>Fluent glyph for the row: a document for pages, a stack for databases.</summary>
public string KindGlyph => Node?.Kind == NotionSourceKind.Database ? "\uE8B7" : "\uE8A5";

/// <summary>Tapping the row (not its checkbox) previews the page.</summary>
public SimpleCommand SelectCommand => field ??= new SimpleCommand(() => _owner?.ShowPreview(this));
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml`
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/NotionPageNodeViewModel.cs`

**Sharp edges.**
- `SelectionMode="None"` on the `TreeView`: the row button, not tree selection,
  drives the preview, which keeps checkbox state and "current row" independent.
- An image icon and a glyph icon are stacked in one `Grid`, each with its own
  visibility, so a node without an image icon still shows a mark.
- Both the parent and the node view models carry
  `[Microsoft.UI.Xaml.Data.Bindable]`.
- Lazy child loading hangs off the two-way `IsExpanded` binding; see the
  view-model area.

### Take a secret token in a PasswordBox and keep it out of storage

**When you want this.** The user supplies their own API credential and you do not
want it echoed on screen or written anywhere.

**The MVVM shape.** A `PasswordBox` two-way bound to a plain string property on
the view model, trimmed and handed to the service's connect call. Nothing stores
it: no settings file, no environment variable, no cache.

**Code.**

```xml
<!-- From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml -->
<PasswordBox Width="250" VerticalAlignment="Center" CornerRadius="8"
             PlaceholderText="Notion integration token"
             Password="{d:Binding IntegrationToken, Mode=TwoWay}" />
<TextBox Width="230" VerticalAlignment="Center" CornerRadius="8"
         PlaceholderText="Page or database ID"
         Text="{d:Binding PageOrDatabaseId, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
<Button Style="{StaticResource PrimaryButtonStyle}"
        VerticalAlignment="Center"
        Content="Connect"
        Command="{d:Binding ConnectCommand}" />
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
private async Task DoConnect()
{
    if (!CanConnect()) { return; }

    try
    {
        IsBusy = true;
        StatusText = "Connecting to Notion…";
        var botName = await _documentSvc.ConnectAsync(IntegrationToken.Trim());

        StatusText = "Loading the root page…";
        var roots = await _documentSvc.LoadRootsAsync(PageOrDatabaseId.Trim());

        RootNodes.Clear();
        SelectedNode = null;
        ResetPreview();
        foreach (var root in roots)
        {
            RootNodes.Add(new NotionPageNodeViewModel(root, this));
        }

        IsConnected = true;
        ConnectionStatus = $"Connected as {botName}";
        OnNodeCheckedChanged();
        StatusText = "Check the pages to include — the first checked page becomes the cover.";

        if (RootNodes.Count == 1)
        {
            RootNodes[0].IsExpanded = true; //Auto-expand the root; children load lazily
        }
    }
    catch (Exception e)
    {
        IsConnected = false;
        ConnectionStatus = "Not connected";
        StatusText = "Connection failed.";
        await ShowError($"Could not connect: {e.Message}");
    }
    finally
    {
        IsBusy = false;
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml`
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The `PasswordBox` binds `Password`, not `Text`.
- Connecting successfully sets a friendly identity line from the value the service
  returns, which is a cheap way to prove the credential belongs to the account the
  user expected.
- On the framebuffer head the software keyboard has to be enabled for a long token
  to be typeable at all; see the framebuffer blueprint in the startup area.

### Forward pointer input from a canvas into a model

**When you want this.** You want strokes, orbit or pan to follow the pointer, work
with a pen or a finger, and not break when the window loses focus mid-gesture.

**The MVVM shape.** The page (or the canvas element itself) forwards four pointer
events straight into the model in a few lines each, and captures the pointer while
a gesture is in progress. The model decides whether a press starts anything and
tracks whether a gesture is active, so the page holds no state of its own and the
view model is not on the per-point path at all.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs
DrawCanvas.PointerPressed += (_, e) =>
{
    var session = ViewModel?.Session;
    if (session == null) { return; }

    var pointerPoint = e.GetCurrentPoint(DrawCanvas);
    if (!pointerPoint.Properties.IsLeftButtonPressed) { return; }

    if (session.PointerPressed(DrawCanvasHelper.GetPointFromPosition(pointerPoint.Position), DrawCanvas.GetViewSize()))
    {
        DrawCanvas.CapturePointer(e.Pointer);
        e.Handled = true;
    }
};

DrawCanvas.PointerMoved += (_, e) =>
{
    var session = ViewModel?.Session;
    if (session is not { IsPointerActive: true }) { return; }

    session.PointerMoved(DrawCanvasHelper.GetPointFromPosition(e.GetCurrentPoint(DrawCanvas).Position), DrawCanvas.GetViewSize());
    e.Handled = true;
};

DrawCanvas.PointerReleased += (_, e) =>
{
    var session = ViewModel?.Session;
    if (session is not { IsPointerActive: true }) { return; }

    session.PointerReleased();
    DrawCanvas.ReleasePointerCapture(e.Pointer);
    e.Handled = true;
};

//If capture is lost mid-stroke (e.g. the window deactivates), discard the stroke
DrawCanvas.PointerCaptureLost += (_, _) => ViewModel?.Session?.PointerCanceled();
```

An element that owns its own camera does the same thing inside itself:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs
private void OnPointerMoved(object sender, PointerRoutedEventArgs e)
{
    if (!_dragging) { return; }

    var position = e.GetCurrentPoint(this).Position;
    var deltaYaw = (float)(position.X - _lastX) * OrbitDegreesPerPixel;
    var deltaPitch = (float)(position.Y - _lastY) * OrbitDegreesPerPixel;
    _lastX = position.X;
    _lastY = position.Y;

    // Grab-and-drag feel: dragging right rolls the model's near face to the right, and
    // dragging up rolls its top toward you. Invalidate coalesces to one paint per frame.
    _renderer.Camera.Orbit(-deltaYaw, deltaPitch);
    Invalidate();
    e.Handled = true;
}

private void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e) => _dragging = false;

private void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
{
    var delta = e.GetCurrentPoint(this).Properties.MouseWheelDelta;
    _renderer.Camera.Zoom(delta > 0 ? 0.9f : 1.1f);
    Invalidate();
    e.Handled = true;
}
```

Where the canvas renders in pixels and the pointer reports device-independent
units, convert before forwarding:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
DisplayCanvas.PointerPressed += (_, e) =>
{
    var painter = ViewModel?.CurrentPainter;
    if (painter == null) { return; }

    var point = e.GetCurrentPoint(DisplayCanvas);
    if (!point.Properties.IsLeftButtonPressed) { return; }

    var (x, y) = ToCanvasPixels(point.Position);
    painter.PointerDown(x, y);
    _gestureStartTimestamp = point.Timestamp;
    _gestureClock.Restart();
    DisplayCanvas.CapturePointer(e.Pointer);
    RequestRender();
    e.Handled = true;
};

// ...

// Maps a pointer position (in view/DIP units) to the canvas's pixel space, so pointer
// input stays aligned with the rendered pixels at any DPI and after any window resize
private (double X, double Y) ToCanvasPixels(Point position)
{
    var canvasSize = DisplayCanvas.CanvasSize;
    var scaleX = DisplayCanvas.ActualWidth > 0 && canvasSize.Width > 0
        ? canvasSize.Width / DisplayCanvas.ActualWidth : 1.0;
    var scaleY = DisplayCanvas.ActualHeight > 0 && canvasSize.Height > 0
        ? canvasSize.Height / DisplayCanvas.ActualHeight : 1.0;
    return (position.X * scaleX, position.Y * scaleY);
}
```

**Where to look.**
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`

**Also shown by.**
`PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs` (the same shape with the
WPF event names: mouse down, move, up and lost-capture, with `CaptureMouse()`)

**Sharp edges.**
- Set `e.Handled = true` on pointer moves. An unhandled move bubbles to the window
  manager, which then drags or manipulates the window instead of driving your
  scene; the code comment in PolyHavenBrowser_viewer_only says exactly that.
- Handle capture-lost as well as release, or a gesture that loses capture leaves
  the element stuck mid-drag - or a stroke stays half open when the window
  deactivates.
- Pass the current view size with every point where the model works in its own
  logical space, so a resize does not shift the geometry.
- Pointer positions arrive in device-independent units while a canvas may render
  in pixels; scale by canvas size over actual size or the input drifts from the
  image at non-100% display scaling.
- `SizeChanged` also has to request a render.

### Translate platform pointer and key events into a headless input model

**When you want this.** Your model wants mouse and key events but must not
reference any UI type, so it can be unit-tested headless.

**The MVVM shape.** The canvas translates platform event arguments into the
model's own event-argument types through a static mapper, captures the pointer,
and calls the model. The model's tools never see a platform event type.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs
private ToolMouseEventArgs BuildMouseArgs (PointerRoutedEventArgs e)
{
    var point = e.GetCurrentPoint (this);
    PointD viewPoint = new (point.Position.X, point.Position.Y);
    PointD canvasPoint = document?.Workspace.ViewPointToCanvas (viewPoint) ?? viewPoint;

    MouseButton button = MouseButton.None;
    var props = point.Properties;
    if (props.IsLeftButtonPressed)
        button = MouseButton.Left;
    else if (props.IsRightButtonPressed)
        button = MouseButton.Right;
    else if (props.IsMiddleButtonPressed)
        button = MouseButton.Middle;

    return new ToolMouseEventArgs {
        State = InputMapper.ToModifierType (e.KeyModifiers, props),
        MouseButton = button,
        PointDouble = canvasPoint,
        WindowPoint = viewPoint,
        RootPoint = viewPoint,
    };
}

private void OnCanvasPointerReleased (object sender, PointerRoutedEventArgs e)
{
    if (document is null)
        return;
    // The pressed-button flags are cleared by release time; recover the
    // released button from the update kind.
    ToolMouseEventArgs args = BuildMouseArgs (e);
    var kind = e.GetCurrentPoint (this).Properties.PointerUpdateKind;
    MouseButton released = kind switch {
        PointerUpdateKind.LeftButtonReleased => MouseButton.Left,
        PointerUpdateKind.RightButtonReleased => MouseButton.Right,
        PointerUpdateKind.MiddleButtonReleased => MouseButton.Middle,
        _ => args.MouseButton,
    };
    // ...
    ReleasePointerCapture (e.Pointer);
    PintaCore.Tools.DoMouseUp (document, args);
    e.Handled = true;
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/InputMapper.cs`

**Sharp edges.**
- On pointer release the pressed-button flags are already cleared; the released
  button has to be recovered from the update kind or every release reports
  `None`.
- Capture on press and release on release are required for a drag that leaves the
  element to keep delivering moves.
- The input mapper's comment records that the platform key-state API returns
  nothing on the Skia heads, so modifier state must be tracked from the modifier
  keys' own down and up events instead.
- Ctrl-plus-wheel zoom is handled on the canvas; an unmodified wheel is left alone
  so the scroll viewer still pans.

### Select a canvas base class per head with conditional compilation

**When you want this.** The same XAML element name must work on heads whose Skia
canvas control comes from different assemblies with different base types.

**The MVVM shape.** One linked source file declares an empty subclass chosen by
preprocessor symbols, plus extension helpers that hide the per-stack point type.
The XAML in every UI then uses the same element unchanged, and the code-behind
wires the same handlers to it.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/Drawing/DrawingCanvas.cs
namespace CodeBrix.Imaging.Drawing;

#if (HAS_CODEBRIXPLATFORM || HAS_WINUI)
public class DrawingCanvas : SkiaSharp.Views.Windows.SKXamlCanvas { }
#else
public class DrawingCanvas : SkiaSharp.Views.WPF.SKElement { }
#endif

public static class DrawCanvasHelper
{
    public static SkiaSharp.SKSize GetViewSize(this DrawingCanvas canvas) =>
        (canvas == null)
        ? default
        : new SkiaSharp.SKSize((float)canvas.ActualWidth, (float)canvas.ActualHeight);

#if (HAS_CODEBRIXPLATFORM || HAS_WINUI)
    public static SkiaSharp.SKPoint GetPointFromPosition(Windows.Foundation.Point point) =>
        new ((float)point.X, (float)point.Y);
#else
    public static SkiaSharp.SKPoint GetPointFromPosition(System.Windows.Point point) =>
        new ((float)point.X, (float)point.Y);
#endif
}
```

**Where to look.**
`PainDiagram/Shared/Drawing/DrawingCanvas.cs`
`PainDiagram/CodeBrixPlatform/PainDiagram.Core/PainDiagram.Core.csproj`
`PainDiagram/PainDiagram.WinUI/PainDiagram.WinUI.csproj`

**Sharp edges.**
- The subclass carries no behavior on purpose; the hosting page's code-behind
  still wires paint and pointer events.
- The file declares the type in the library's namespace even though it compiles
  into the application assembly. That lets the XAML use one namespace for the
  control, but the XAML must still name the assembly it is compiled into, which
  differs between the platform heads and the native WPF head.
- The native WPF head defines neither symbol, which is the `#else` path. If you
  add a head, decide which symbol it defines before anything else.

### Show live video on an SKXamlCanvas subclass

**When you want this.** You want live video inside a XAML layout, aspect-fit,
mirrored like a selfie camera, with no per-frame allocation.

**The MVVM shape.** The library declares a one-line `SKXamlCanvas` subclass purely
so the XAML can name the element, plus a separate renderer class that takes a
surface, its image info and the capture service. The page owns one renderer per
canvas and wires the paint event to it in a single line; the view model exposes
the capture service and never touches Skia.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Webcam/CameraCanvas.cs
public class CameraCanvas : SkiaSharp.Views.Windows.SKXamlCanvas { }

public sealed class WebcamFrameRenderer
{
    private byte[] _frameBuffer;
    private SKBitmap _bitmap;

    public void Render(SKSurface surface, SKImageInfo info, WebcamCaptureService service, bool mirror)
    {
        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);

        if (service == null
            || !service.TryCopyLatestFrame(ref _frameBuffer, out int width, out int height)
            || width <= 0 || height <= 0)
        {
            return;
        }

        if (_bitmap == null || _bitmap.Width != width || _bitmap.Height != height)
        {
            _bitmap?.Dispose();
            _bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        }
        Marshal.Copy(_frameBuffer, 0, _bitmap.GetPixels(), width * height * 4);

        float scale = Math.Min((float)info.Width / width, (float)info.Height / height);
        float destWidth = width * scale;
        float destHeight = height * scale;
        float destX = (info.Width - destWidth) / 2f;
        float destY = (info.Height - destHeight) / 2f;

        int restoreTo = canvas.Save();
        if (mirror)
        {
            canvas.Scale(-1, 1, destX + (destWidth / 2f), 0);
        }
        canvas.DrawBitmap(_bitmap, new SKRect(destX, destY, destX + destWidth, destY + destHeight),
            new SKSamplingOptions(SKFilterMode.Linear));
        canvas.RestoreToCount(restoreTo);
    }
}
```

```xml
<!-- From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml -->
<Page xmlns:webcam="clr-namespace:WebcamPainter.Webcam;assembly=WebcamPainter.Webcam" ...>
  <Border BorderBrush="Gray" BorderThickness="1" Background="Black" Height="150">
    <webcam:CameraCanvas x:Name="SelfViewCanvas" />
  </Border>
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs
//One frame renderer per canvas that shows live video (each caches its own buffers)
private readonly WebcamFrameRenderer _mainRenderer = new WebcamFrameRenderer();
private readonly WebcamFrameRenderer _selfViewRenderer = new WebcamFrameRenderer();
// ...
SelfViewCanvas.PaintSurface += (_, e) =>
    _selfViewRenderer.Render(e.Surface, e.Info, ViewModel?.CaptureService, mirror: true);

SelfViewCanvas.SizeChanged += (_, _) => SelfViewCanvas.Invalidate();
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Webcam/CameraCanvas.cs`
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml` and `Views/MainPage.xaml.cs`

**Also shown by.**
`PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraCanvas.cs` and
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Create one renderer per canvas. The cached framebuffer and bitmap are reused
  across paints and are only touched on the UI thread, so sharing one renderer
  between two canvases would race.
- The mirror is a canvas transform around the destination rectangle's horizontal
  center, applied inside a save and restore, not a pixel flip. That is why tracked
  positions have to be mirrored separately before they reach anything that draws
  in the same space.
- Clear the surface first, so "no frame yet" renders as a black panel rather than
  garbage.
- `SizeChanged` has to invalidate the canvas, or the frame keeps its old letterbox
  after a resize.
- The bitmap is recreated only when the frame dimensions change, and pixels are
  pushed straight into its buffer.
- The empty subclass exists purely so XAML can name the type from the library's
  namespace; that is the cheapest way to place a Skia canvas in a shared UI
  project.

### Turn image bytes into a bound BitmapImage

**When you want this.** Your service returns encoded image bytes and your XAML has
an `Image` whose `Source` is bound.

**The MVVM shape.** The view model exposes an image property plus its pixel size,
and one `internal` method that decodes bytes into it. The XAML binds `Source` and
nothing else.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/DocumentPaneViewModel.cs
    /// <summary>Decodes page's PNG into the pane's image. Must be called on the UI thread.</summary>
    internal async Task ShowPageAsync(RenderedPage page)
    {
        var image = new BitmapImage();
        using (var stream = new MemoryStream(page.PngBytes))
        {
            await image.SetSourceAsync(stream.AsRandomAccessStream());
        }
        PagePixelWidth = page.PixelWidth;
        PagePixelHeight = page.PixelHeight;
        PageImage = image; //Last, so a listener sees the size when the image changes
    }
```

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml -->
                    <ScrollViewer x:Name="LeftScroller"
                                  HorizontalScrollMode="Enabled" VerticalScrollMode="Enabled"
                                  HorizontalScrollBarVisibility="Hidden" VerticalScrollBarVisibility="Hidden"
                                  ZoomMode="Disabled">
                        <Image x:Name="LeftImage" Source="{d:Binding PageImage}" Stretch="Fill"
                               HorizontalAlignment="Center" VerticalAlignment="Center" />
                    </ScrollViewer>
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/DocumentPaneViewModel.cs`
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml`

**Sharp edges.**
- The order of the three assignments is load-bearing and commented: width and
  height first, the image last, so anything reacting to the image already sees the
  matching size.
- `stream.AsRandomAccessStream()` is the bridge from a `MemoryStream` to what
  `SetSourceAsync` wants.
- The method must be called on the UI thread; it is awaited from the view model's
  render path, after the `Task.Run` has completed.
- `Stretch="Fill"` on an explicitly sized `Image` is what makes a zoom exact,
  rather than letting the control choose a fit.

### Let the page do the layout arithmetic only it can do

**When you want this.** The view model owns a zoom factor and a pan fraction, but
only the page knows how large the viewport actually is, so somebody has to combine
them.

**The MVVM shape (adapted).** The sample computes the fit-to-viewport scale, sizes
the image and scrolls the viewer inside the page's code-behind, reading the view
model's state directly. The shape to prefer keeps the arithmetic in the view
model: the page reports its viewport size through a bridge method whenever it
changes, and binds the image size and scroll offsets to computed view-model
properties. The adapted block shows the page side reduced to two forwarding calls;
the formula itself is unchanged.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs
    /// <summary>
    /// Sizes side's image to zoom x fit-the-page (so 100% shows the whole page, centred, and
    /// every level above it overflows the viewer) and scrolls the viewer to the pane's pan position.
    /// </summary>
    private void ApplyView(DocumentSide side)
    {
        // ...
        var fit = Math.Min(viewportWidth / pane.PagePixelWidth, viewportHeight / pane.PagePixelHeight);
        var factor = viewModel.View.Zoom.Factor;
        image.Width = Math.Floor(pane.PagePixelWidth * fit * factor);
        image.Height = Math.Floor(pane.PagePixelHeight * fit * factor);

        //Let the viewer measure the new extent before positioning it
        scroller.UpdateLayout();
        var pan = viewModel.View.PanOf(side);
        scroller.ChangeView(
            pan.Horizontal * Math.Max(0, scroller.ScrollableWidth),
            pan.Vertical * Math.Max(0, scroller.ScrollableHeight),
            null, disableAnimation: true);
    }
```

```csharp
// Adapted from CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs
// The page forwards viewport size and applies computed values; the view model owns the maths.
public MainPage()
{
    // ...
    LeftScroller.SizeChanged += (_, _) =>
        ViewModel?.SetViewportSize(DocumentSide.Left, LeftScroller.ActualWidth, LeftScroller.ActualHeight);
    RightScroller.SizeChanged += (_, _) =>
        ViewModel?.SetViewportSize(DocumentSide.Right, RightScroller.ActualWidth, RightScroller.ActualHeight);
}
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/ComparisonView.cs
    /// <summary>How much of the visible area one pan step moves: a quarter of it.</summary>
    public const double PanStepOfViewport = 0.25;

    /// <summary>
    /// One pan step as a fraction of the scrollable range. At zoom factor f the page is
    /// f viewports wide, so the scrollable range is f - 1 viewports and a quarter
    /// of a viewport is 0.25 / (f - 1) of it. Zero at 100%, where nothing scrolls.
    /// </summary>
    public double PanStepFraction => Zoom.IsZoomedIn ? PanStepOfViewport / (Zoom.Factor - 1) : 0;
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs`
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/ComparisonView.cs`
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/PanPosition.cs`

**Sharp edges.**
- Call `UpdateLayout()` before changing the view: the scrollable extents are stale
  until the viewer has measured the newly sized content, so scrolling first lands
  in the wrong place.
- Store pan as a fraction of the scrollable range, not as pixels, which is exactly
  what makes it survive a zoom change.
- Guard the fully-zoomed-out case where nothing scrolls at all.
- Disable the scroll viewer's own zoom when the application has its own zoom
  ladder; two zooms fight.
- Re-apply the size and the pan on size changes, on the view-version change, and
  on each pane's image change; missing any one leaves the image the wrong size.

### Build menus and toolbars from a command model instead of XAML

**When you want this.** You have more than a handful of commands and want a
command's label, icon, enabled state and shortcut declared once.

**The MVVM shape.** The commands are declared in a headless library as plain
objects with a label, an icon name, shortcuts, an enabled flag and an activation
event; a builder turns each into a menu item and keeps the enabled state in sync.
With `SimpleCommand` the same builder would bind `CanExecute` instead, and the
XAML would still declare no commands.

**Code.**

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml -->
<!-- Menu. Built from the Pinta.Brix.Engine action model at runtime; see
     MainPage.Menus.cs. Nothing is declared here, so a command declared
     once in Actions/*.cs gets its label, icon, enabled state and
     shortcut without a second edit. -->
<MenuBar x:Name="MainMenuBar" Grid.Row="0" />
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Menus/CommandMenuBuilder.cs
public static MenuFlyoutItemBase Create (Command command, bool showIcon = true)
{
	ArgumentNullException.ThrowIfNull (command);

	if (command is ToggleCommand toggle)
		return CreateToggle (toggle);

	MenuFlyoutItem item = new () {
		Text = command.Label,
		IsEnabled = command.Sensitive,
	};

	ApplyIcon (item, command, showIcon);
	ApplyAcceleratorText (item, command);

	item.Click += (_, _) => command.Activate ();
	command.SensitiveChanged += (_, _) => item.IsEnabled = command.Sensitive;

	return item;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Menus.cs
private static MenuBarItem BuildMenu(string title, params Command[] commands)
{
    MenuBarItem menu = new() { Title = title };

    foreach (Command command in commands)
    {
        //A null entry is a separator - it keeps the call sites readable
        //next to upstream's menu-model code.
        menu.Items.Add(command is null
            ? CommandMenuBuilder.CreateSeparator()
            : CommandMenuBuilder.Create(command));
    }

    return menu;
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Menus/CommandMenuBuilder.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Menus.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Actions/Command.cs`

**Sharp edges.**
- A toggle command raises its toggled event for both interactive and programmatic
  changes, so the builder guards against the echo with a local flag rather than
  unhooking and rehooking.
- A missing icon must not take the menu down: the icon factory can return null and
  the builder simply omits it.
- Only shortcuts the dispatcher can actually parse are advertised on the item.

### Dispatch keyboard shortcuts from one page KeyDown handler

**When you want this.** You want working keyboard shortcuts on the Skia heads.

**The MVVM shape.** A table maps parsed accelerators to commands; the page adds
one handled-events-too key handler and asks the table to invoke. The commands live
in the model, so the table is testable headless - and this application does test
it.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Input/CommandAcceleratorTable.cs
// Pinta.Brix note: XAML KeyboardAccelerators are declared on the menu items
// (so the shortcut is visible where the user looks for it) but they do NOT
// fire on the Skia heads - verified on X11 by driving the running
// application: typing reaches a TextBox normally, while Ctrl+Z, Ctrl+Y and
// Ctrl+H registered on a Page or on a MenuFlyoutItem never invoke.
//
// So the shortcuts are dispatched here instead, from a single KeyDown handler
// on the page.
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Input/CommandAcceleratorTable.cs
public bool TryInvoke (VirtualKey key)
{
	if (!map.TryGetValue ((key, CurrentModifiers), out Engine.Command? command))
		return false;

	// A disabled command must swallow nothing: the key should behave as if
	// the shortcut were not bound at all.
	if (!command.Sensitive)
		return false;

	command.Activate ();
	return true;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Menus.cs
acceleratorTable = new CommandAcceleratorTable();

foreach (Command command in actions.AllCommands())
{
    acceleratorTable.Register(command);
}

//Handled keys have to be seen too: the canvas marks most key events
//handled, and a shortcut must still work while it has focus.
AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(OnGlobalKeyDown), handledEventsToo: true);
AddHandler(UIElement.KeyUpEvent, new KeyEventHandler(OnGlobalKeyUp), handledEventsToo: true);
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Input/CommandAcceleratorTable.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Input/AcceleratorParser.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Menus.cs`
`Pinta.Brix/tests/libs/Pinta.Brix.Controls.Tests/CommandAcceleratorTableTests.cs`

**Sharp edges.**
- XAML keyboard-accelerator objects do not invoke on the Skia heads. The menu
  items still show the shortcut text through the text-override property, which
  does work, and the builder deliberately does not attach a real accelerator so
  there is never a second dispatch path.
- `handledEventsToo: true` is required, because a canvas marks most key events
  handled.
- Modifier state is tracked from the modifier keys' own transitions, not probed,
  and a reset exists for focus loss so a modifier released elsewhere does not stay
  stuck down.
- A disabled command must not swallow the key: the shortcut should behave as if it
  were not bound.
- Duplicate accelerators resolve first-registration-wins, deliberately.

### Bind a page level CheckBox two way

**When you want this.** A checkbox on the page is one of the inputs to a command, and its
value has to be readable by the view model, settable from code, and remembered between
runs.

**The MVVM shape.** `IsChecked` binds `TwoWay` to a plain `bool` property with
`{d:Binding}`, exactly like a `TextBox`. The property does whatever else has to happen -
persisting the value, refreshing computed text - in its setter, and
`[AffectsProperties]` refreshes anything derived from it. The tick box follows the
application palette because the checkbox brush keys are re-keyed like every other control
family.

**Code.**

```xml
<!-- From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml -->
<CheckBox Content="Include closed issues" FontSize="13" TabIndex="3"
          Margin="0,3,14,3" VerticalAlignment="Center"
          IsChecked="{d:Binding IncludeClosed, Mode=TwoWay}" />
```

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
/// <summary>Whether closed issues and pull requests are searched as well as open ones.</summary>
[AffectsProperties(nameof(HelperText))]
public bool IncludeClosed
{
    get => _includeClosed;
    set
    {
        if (_includeClosed == value) { return; }

        _includeClosed = value;
        NotifyPropertyChanged(nameof(IncludeClosed));
        SettingsService.Set(SettingKeys.IncludeClosed, value);
    }
}
```

The value is read back in the constructor into the field rather than through the property,
so opening the application does not write the stored value straight back:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs
_includeClosed = SettingsService.Get(SettingKeys.IncludeClosed, false);
```

Its tick follows the scheme because the checkbox keys are in the brush map with the rest:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/SchemeBrushMap.cs
//CheckBox: the tick box, its stroke and the tick itself.
{ "CheckBoxForegroundUnchecked", ColorRole.TextPrimary },
// ...
{ "CheckBoxCheckBackgroundFillChecked", ColorRole.Accent },
{ "CheckBoxCheckBackgroundFillCheckedPointerOver", ColorRole.Accent },
{ "CheckBoxCheckBackgroundFillCheckedPressed", ColorRole.Accent },
{ "CheckBoxCheckBackgroundFillCheckedDisabled", ColorRole.Hairline },
{ "CheckBoxCheckBackgroundStrokeUnchecked", ColorRole.Hairline },
// ...
```

**Where to look.**
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/ViewModels/MainViewModel.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.Core/Theming/SchemeBrushMap.cs`

**Sharp edges.**
- `Mode=TwoWay` is not the default for `IsChecked`. Without it the box moves on screen and
  the view model never hears about it.
- Read the stored value into the backing field in the constructor, not through the
  property. Going through the setter writes the value back to the store on every launch
  and raises a change notification before anything is bound.
- A checkbox is not a text box: pressing Enter on it does not run the page's default
  action, because the Enter handler is on the text boxes. Say so, or handle the key on the
  page.
- Re-key the whole `CheckBox*` family, not just the checked fill, or the box wears the
  stock accent while the rest of the page follows your palette.

### Run a command when the user presses Enter in a text box

**When you want this.** Enter in a search box should do what the Search button
does.

**The MVVM shape.** Prefer the declarative form: an input binding in XAML pointing
at the command, with no code-behind at all. Where a key handler is unavoidable, it
stays a one-line forward to the command and checks `CanExecute` first.

**Code.**

```xml
<!-- From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.Wpf/Views/MainWindow.xaml -->
<TextBox Grid.Column="1" Height="30" VerticalContentAlignment="Center"
         Text="{Binding SearchTerms, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}">
    <!-- Pressing Enter in the search box runs Search, just like clicking the button. -->
    <TextBox.InputBindings>
        <KeyBinding Key="Return" Command="{Binding SearchCommand}" />
    </TextBox.InputBindings>
</TextBox>
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs
//Pressing Enter in the search box runs Search, just like clicking the button.
private void SearchBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
{
    if (e.Key == Windows.System.VirtualKey.Enter
        && DataContext is MainViewModel { SearchCommand: var search }
        && search.CanExecute(null))
    {
        search.Execute(null);
        e.Handled = true;
    }
}
```

A page often wants a second key that means the same thing wherever the focus is.
That one belongs on the root element rather than on each control. Here Enter runs
the search from either text box, and Escape cancels a running search from anywhere
on the page:

```csharp
// From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs
//Pressing Enter in either box runs Search, exactly as clicking the button does. The
//CanExecute check matters: a key handler would otherwise walk past the disabled state that a
//button honours.
private void OnSearchBoxKeyDown(object sender, KeyRoutedEventArgs e)
{
    if (e.Key != Windows.System.VirtualKey.Enter) { return; }

    if (DataContext is MainViewModel { SearchCommand: var search }
        && search != null
        && search.CanExecute(null))
    {
        search.Execute(null);
        e.Handled = true;
    }
}

//Escape stops a running search from anywhere on the page.
private void OnRootKeyDown(object sender, KeyRoutedEventArgs e)
{
    if (e.Key != Windows.System.VirtualKey.Escape) { return; }

    if (DataContext is MainViewModel { CancelCommand: var cancel }
        && cancel != null
        && cancel.CanExecute(null))
    {
        cancel.Execute(null);
        e.Handled = true;
    }
}
```

```xml
<!-- From CodeBrix.Samples/GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml -->
<Grid x:Name="RootGrid" Background="{StaticResource CanvasBrush}" KeyDown="OnRootKeyDown">
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.Wpf/Views/MainWindow.xaml`
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml`

**Sharp edges.**
- Check `CanExecute` before invoking; a key handler bypasses the disabled state a
  button would have honored.
- Enter means "do the default thing for this box", so it stays on the boxes. A key
  that means the same thing everywhere - Escape for cancel - goes on the root
  element's `KeyDown` instead, and then works whatever has focus.
- Match the command out of the data context and check it for null as well. A page
  can raise a key event while its data context is still being set.
- Say in the interface which keys do what where. Enter on a checkbox does not run
  the page's default action, because the Enter handler is on the text boxes.

### Render a tool options toolbar from a descriptor model

**When you want this.** Parts of your UI are described by a library that must not
reference the UI framework - a plugin's options, a tool's settings row.

**The MVVM shape.** The library appends framework-free descriptors to a model
list; a renderer materializes each into a real control and binds both ways.
Rebuilding is event-driven from the model.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/ToolBarRenderer.cs
public sealed class ToolBarRenderer : IDisposable
{
	private readonly EngineToolBar model;
	private readonly StackPanel panel;

	//Container descriptors outlive any single Rebuild (they belong to the
	//tool), so their event subscriptions must be detached when the toolbar
	//is rebuilt or the old handlers keep rebuilding orphaned panels.
	private readonly List<Action> detachers = [];

	public ToolBarRenderer (EngineToolBar model, StackPanel panel)
	{
		this.model = model;
		this.panel = panel;
		model.ItemsChanged += OnItemsChanged;
		Rebuild ();
	}

	private UIElement? CreateElement (ToolBarItem item)
	{
		UIElement? element = item switch {
			ToolBarLabel label => new TextBlock { Text = label.Text, /* ... */ },
			ToolBarSeparator => new Border { Width = 1, /* ... */ },
			ToolBarImage image => CreateImage (image),
			ToolBarToggleButton toggle => CreateToggle (toggle),
			ToolBarDropDownButton dropDown => CreateDropDown (dropDown),
			ToolBarComboBox combo => CreateCombo (combo),
			ToolBarSpinButton spin => CreateSpin (spin),
			ToolBarScale scale => CreateScale (scale),
			ToolBarContainer container => CreateContainer (container),
			_ => null,
		};
		// ... tooltip, then a Visible->Visibility binding with a detacher
		return element;
	}
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Tools/Tools/ToolOptionWidgetService.cs
if (toolOption is IntegerOption integerOption) {
    ToolBarSpinButton spin_button = new (integerOption.Minimum, integerOption.Maximum, 1, integerOption.Value);
    spin_button.ValueChanged += (_, _) => integerOption.Value = spin_button.GetValueAsInt ();
    integerOption.OnValueChanged += newValue => spin_button.Value = newValue;

    box.Append (new ToolBarLabel ($" {integerOption.LabelText}: "));
    box.Append (spin_button);
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/ToolBarRenderer.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/ToolBar/ToolBarItem.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Tools/Tools/ToolOptionWidgetService.cs`

**Sharp edges.**
- The descriptors outlive any single rebuild because they belong to the tool, so
  every subscription made during a rebuild needs an explicit detacher, or old
  handlers keep rebuilding panels that are no longer in the tree.
- Descriptor visibility maps to `Visibility`, so a tool can hide an option without
  the renderer rebuilding.

### Build a drawn widget as an SKXamlCanvas subclass with hit testing

**When you want this.** A small control whose geometry is fixed and pixel-exact -
a color swatch strip, a gauge, a mini timeline - where composing it from XAML
elements would be more work and less faithful than drawing it.

**The MVVM shape.** The control draws from model state and raises a semantic event
(not a click) when the user asks for something the view cannot decide. The page or
view model handles that event and shows a dialog or mutates the model.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Palette/PaletteWidget.cs
public sealed class PaletteWidget : SKXamlCanvas
{
	private const int WidgetHeight = 42;
	private static readonly SKRect PrimaryRect = SKRect.Create (4, 3, SwatchSize, SwatchSize);
	private static readonly SKRect SecondaryRect = SKRect.Create (17, 16, SwatchSize, SwatchSize);
	private static readonly SKRect SwapRect = SKRect.Create (27, 2, 15, 15);
	private static readonly SKRect ResetRect = SKRect.Create (2, 27, 15, 15);

	/// <summary>
	/// Raised when the user asks to edit a colour - a click on either swatch,
	/// or a modifier-click on a palette entry.
	/// </summary>
	public event EventHandler<PaletteColorEditEventArgs>? ColorEditRequested;

	public PaletteWidget ()
	{
		Height = WidgetHeight;
		MinWidth = 300;

		PaintSurface += OnPaintSurface;
		PointerPressed += OnPointerPressedHandler;

		PintaCore.Palette.PrimaryColorChanged += OnPaletteChanged;
		PintaCore.Palette.SecondaryColorChanged += OnPaletteChanged;
		PintaCore.Palette.RecentColorsChanged += OnPaletteChanged;
		PintaCore.Palette.CurrentPalette.PaletteChanged += OnPaletteChanged;
	}

	private void OnPaletteChanged (object? sender, EventArgs e) => Invalidate ();

	private void OnPointerPressedHandler (object sender, PointerRoutedEventArgs e)
	{
		PointerPoint point = e.GetCurrentPoint (this);
		SKPoint position = new ((float) point.Position.X, (float) point.Position.Y);
		// ...
		// The primary swatch is drawn on top, so it is tested first.
		if (PrimaryRect.Contains (position)) {
			ColorEditRequested?.Invoke (this, new PaletteColorEditEventArgs (PaletteColorTarget.Primary, -1));
			return;
		}
		// ...
	}
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Palette.cs
private void BuildPaletteWidget()
{
    paletteWidget = new PaletteWidget();
    paletteWidget.ColorEditRequested += async (_, args) => await EditColorAsync(args);
    PaletteWidgetHost.Content = paletteWidget;
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Palette/PaletteWidget.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Palette.cs`

**Sharp edges.**
- Hit testing must run in the same order as drawing, or overlapping regions
  resolve to the wrong one.
- The control subscribes to four model events in its constructor and never
  unsubscribes; it lives for the life of the window, which is what makes that
  acceptable here.
- Every drawn rectangle is a constant in device-independent pixels, and the
  widget's header comment lays out the whole geometry, so the drawing and the hit
  test cannot drift apart.

### Supply a splitter bar where the platform has none

**When you want this.** A resizable pane divider, and the platform ships no
splitter control.

**The MVVM shape.** A tiny `Border` subclass that captures the pointer and reports
drag deltas; the owner decides what the delta means and persists the result. The
control has no resize policy of its own.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/ThumbSplitter.cs
public sealed class ThumbSplitter : Border
{
	/// <summary>
	/// Raised while dragging with the movement since the last report, in the
	/// axis the splitter resizes: X for a vertical bar, Y for a horizontal one.
	/// </summary>
	public event EventHandler<double>? DragDelta;

	public ThumbSplitter (Orientation orientation)
	{
		Orientation = orientation;
		Background = new SolidColorBrush (Windows.UI.Color.FromArgb (0x30, 0x80, 0x80, 0x80));

		if (orientation == Orientation.Vertical)
			Width = 6;
		else
			Height = 6;

		ProtectedCursor = InputSystemCursor.Create (
			orientation == Orientation.Vertical
			? InputSystemCursorShape.SizeWestEast
			: InputSystemCursorShape.SizeNorthSouth);

		PointerPressed += OnPointerPressedHandler;
		PointerMoved += OnPointerMovedHandler;
		PointerReleased += OnPointerReleasedHandler;
	}
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
ThumbSplitter columnSplitter = new(Orientation.Vertical);
Grid.SetColumn(columnSplitter, 2);
ContentGrid.Children.Add(columnSplitter);
columnSplitter.DragDelta += (_, delta) =>
{
    double width = Math.Clamp(PadsColumn.ActualWidth - delta, 200, 800);
    PadsColumn.Width = width;
    PintaCore.Settings.PutSetting("pads-width", (int)width);
};
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/ThumbSplitter.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- The XAML reserves an empty column and row for the splitters and they are added
  in code on load; the XAML comment says so explicitly.
- The delta is relative to the previous report, so the owner clamps against
  minimums itself. Mind the sign: a pane grows as the splitter moves the other
  way.
- Writing the new size to settings on every delta is only cheap because the store
  skips unchanged values.

### Show a modeless floating options panel so a live preview stays visible

**When you want this.** The user is adjusting parameters and needs to see the
document change as they do. A modal dialog that dims the window defeats that.

**The MVVM shape.** A popup-based host with its own title bar, confirm and cancel
buttons and Escape handling, returning a `Task<bool>` so the calling code awaits
it like a dialog. The content is supplied by the caller.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/FloatingDialogHost.cs
// A modeless floating panel with a title bar, OK/Cancel buttons and a
// draggable header, shown in a non-dimming Popup. Upstream's effect and
// adjustment dialogs are small utility WINDOWS floating over the canvas, so
// the live preview stays fully visible and interactive; ContentDialog dims
// and blocks the whole window, which defeats the preview.

public static async Task<bool> ShowAsync (string title, UIElement content, XamlRoot xamlRoot, double maxWidth = 460)
{
    TaskCompletionSource<bool> completion = new (TaskCreationOptions.RunContinuationsAsynchronously);

    //The panel is deliberately OPAQUE: translucent surfaces over a white
    //canvas wash out to unreadable (the menu flyouts demonstrated it).
    Border root = new () {
        Background = new SolidColorBrush (Windows.UI.Color.FromArgb (0xFF, 0x2B, 0x2B, 0x2B)),
        // ...
        RequestedTheme = ElementTheme.Dark,
    };

    Popup popup = new () {
        XamlRoot = xamlRoot,
        IsLightDismissEnabled = false,
        Child = root,
    };
    // ... title, content, OK/Cancel, Escape -> Complete(false)

    //Centre horizontally below the toolbars; the canvas stays visible
    //beneath and beside the panel.
    root.Measure (new Windows.Foundation.Size (double.PositiveInfinity, double.PositiveInfinity));
    popup.HorizontalOffset = Math.Max (0, (xamlRoot.Size.Width - root.DesiredSize.Width) / 2);
    popup.VerticalOffset = 110;
    popup.IsOpen = true;

    return await completion.Task;
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/FloatingDialogHost.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/EffectOptionsDialog.cs`

**Sharp edges.**
- The panel is opaque on purpose: translucent surfaces over a light document
  become unreadable.
- Measure with infinite constraints before reading the desired size to center the
  popup, because a popup child is not in the normal layout pass.
- A `TaskCompletionSource` with `RunContinuationsAsynchronously` is what turns a
  modeless popup into an awaitable, dialog-shaped call.
- Dragging the title block moves the panel by adjusting the popup's offsets from
  pointer deltas, with pointer capture on the title block.

### Generate an options panel from object properties by reflection

**When you want this.** You have many small parameter objects - effect settings,
export options, plugin configuration - and do not want a hand-built panel for
each.

**The MVVM shape.** A static builder walks the data object's public writable
members, skips the ones marked with a skip attribute and the base-class ones,
reads a caption attribute for the label, and builds a row per supported type.
Values are written back through the member, and the object raises its own change
notification so a live preview re-renders.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/EffectOptionsDialog.cs
private static IEnumerable<MemberInfo> GetDialogMembers (EffectData data)
{
	Type type = data.GetType ();
	foreach (MemberInfo member in type.GetMembers (BindingFlags.Public | BindingFlags.Instance)) {
		if (member is not PropertyInfo and not FieldInfo)
			continue;
		if (member is PropertyInfo { CanWrite: false })
			continue;
		if (member.DeclaringType == typeof (EffectData) || member.DeclaringType == typeof (ObservableObject))
			continue;
		if (member.GetCustomAttribute<SkipAttribute> () is not null)
			continue;
		yield return member;
	}
}

private static string GetCaption (MemberInfo member)
	=> member.GetCustomAttribute<CaptionAttribute> ()?.Caption
		?? AddSpaces (member.Name);

private static string AddSpaces (string name)
	=> string.Concat (name.Select ((c, i) => i > 0 && char.IsUpper (c) ? " " + c : c.ToString ()));
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Effects/Adjustments/PosterizeEffect.cs
public sealed class PosterizeData : EffectData
{
	public int Red { get; set; } = 16;
	public int Green { get; set; } = 16;
	public int Blue { get; set; } = 16;
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/EffectOptionsDialog.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/DialogAttributes.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Effects/Adjustments/PosterizeEffect.cs`

**Sharp edges.**
- Base-class members must be excluded explicitly, or every object gets the base
  type's plumbing rendered as editable rows.
- Unsupported member types degrade to a read-only note rather than being silently
  dropped, so a missing editor is visible during development.
- A reflection dialog is only as good as its type coverage; the file's header
  comment lists which member types were added and which items were configurable in
  name only before they existed.

### Show a cancellable progress dialog from synchronous code

**When you want this.** A long operation driven by a synchronous loop needs to
show progress and offer cancel.

**The MVVM shape.** A small class implementing the model's progress-dialog
interface holds the progress bar and text, shows a dialog without awaiting it, and
raises a cancellation event from the close button. The model sets the progress
value from its own tick.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/ContentProgressDialog.cs
public void Show ()
{
	if (showing)
		return;

	XamlRoot? root = xaml_root_getter ();

	if (root is null)
		return; // No visual tree yet - degrade to no feedback rather than throwing.

	StackPanel panel = new () { Spacing = 12 };
	panel.Children.Add (text_block);
	panel.Children.Add (progress_bar);

	dialog = new ContentDialog {
		Title = Title,
		Content = panel,
		CloseButtonText = "Cancel",
		XamlRoot = root,
	};

	dialog.CloseButtonClick += (_, _) => Canceled?.Invoke (this, EventArgs.Empty);

	showing = true;

	// Deliberately not awaited: the caller is a synchronous engine loop that
	// keeps running while this is on screen, and it calls Hide when done.
	_ = dialog.ShowAsync ();
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/ContentProgressDialog.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ChromeManager.cs`

**Sharp edges.**
- The show call is deliberately not awaited and its result discarded; awaiting it
  would block the caller that is producing the progress.
- A null root degrades to no feedback rather than throwing.
- The interface's progress runs 0 to 1 while the control runs 0 to 100, so the
  adapter does the scaling and clamping in one place.

### Lay out a document editor shell with tabs a toolbox and pads

**When you want this.** The overall window shape of an editor: menus, toolbars, a
tool palette, a tabbed document area, dockable side panes, a status bar.

**The MVVM shape.** The XAML declares the grid and the named hosts; everything
inside the hosts is built at load time from model state. In a view-model shape the
lists bind to observable collections instead of being refilled by hand, but the
container layout is identical.

**Code.**

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml -->
<Grid.RowDefinitions>
    <RowDefinition Height="Auto" />   <!-- menu bar -->
    <RowDefinition Height="Auto" />   <!-- icon toolbar -->
    <RowDefinition Height="Auto" />   <!-- tool options -->
    <RowDefinition Height="*" />      <!-- toolbox | tabs | splitter | pads -->
    <RowDefinition Height="Auto" />   <!-- status bar -->
</Grid.RowDefinitions>

<!-- In-app icon toolbar row. Deliberately NOT an OS header bar: the
     Frame Buffer head has no window chrome at all, so anything parked
     there would be unreachable. -->
<Border x:Name="MainToolbarBorder" Grid.Row="1" BorderThickness="0,0,0,1"
        BorderBrush="{ThemeResource SystemControlForegroundBaseLowBrush}">
    <ScrollViewer HorizontalScrollBarVisibility="Auto" VerticalScrollBarVisibility="Disabled">
        <StackPanel x:Name="MainToolbarPanel" Orientation="Horizontal" Padding="6,3" Spacing="2" />
    </ScrollViewer>
</Border>

<TabView x:Name="DocumentTabs"
         Grid.Column="1"
         IsAddTabButtonVisible="False"
         TabCloseRequested="DocumentTabs_TabCloseRequested"
         SelectionChanged="DocumentTabs_SelectionChanged" />
```

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml -->
<!-- MaxLines keeps the bar one line tall: the shape tools carry
     many-line StatusBarText and would otherwise grow the bar. -->
<TextBlock x:Name="StatusText" Grid.Column="1" VerticalAlignment="Center" TextTrimming="CharacterEllipsis" MaxLines="1" />
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Do not put commands in an operating-system header bar if you ship the
  LinuxFrameBuffer head: it has no window chrome at all, so anything there is
  unreachable. The XAML comment states this as the reason for an in-application
  toolbar row.
- Status text sourced from a model can be multi-line; `MaxLines="1"` plus trimming
  keeps the bar from growing.
- A toolbox that re-flows into more or fewer columns as the window height changes
  is rebuilt from a size-changed handler with a small threshold, to avoid
  thrashing.

### Split a page code-behind into named partial files

**When you want this.** A page that genuinely has a lot of wiring, and you want it
navigable rather than one long file.

**The MVVM shape.** The right answer is a view model; where that is not possible,
partial files grouped by concern with a header comment each keep the wiring
findable. The shared project's item list must name every partial.

**Code.**

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Pinta.Brix.UI.projitems -->
<Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.xaml.cs">
  <DependentUpon>MainPage.xaml</DependentUpon>
</Compile>
<Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.Menus.cs">
  <DependentUpon>MainPage.xaml</DependentUpon>
</Compile>
<Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.Actions.cs">
  <DependentUpon>MainPage.xaml</DependentUpon>
</Compile>
<Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.Dialogs.cs">
  <DependentUpon>MainPage.xaml</DependentUpon>
</Compile>
<Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.Palette.cs">
  <DependentUpon>MainPage.xaml</DependentUpon>
</Compile>
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Pinta.Brix.UI.projitems`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Menus.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Actions.cs`

**Sharp edges.**
- A shared items project does not glob; every partial must be listed by hand or it
  silently is not compiled.
- `DependentUpon` on the XAML is what nests the partials under the page in an
  IDE's solution view.
- Each partial's header comment states what it holds and, where relevant, why it
  is not somewhere else. That is what keeps the split navigable.

### Use FontIcon glyphs so icons survive on a device with no system fonts

**When you want this.** Your application must render identically on a desktop and
on an embedded device that has no installed fonts at all.

**The MVVM shape.** Pure XAML. Never put a literal symbol character in a text
element for an icon.

**Code.**

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml -->
<!-- FontIcon resolves through the Fluent symbols font that every
     CodeBrix.Platform application ships, so it renders on a device
     that has no system fonts at all. A literal symbol character
     here would depend on the host's fonts and come out as a
     missing-glyph box on an embedded frame-buffer device. -->
<FontIcon Glyph="&#xE82C;" FontSize="30"
          Foreground="#262B34"
          HorizontalAlignment="Center" VerticalAlignment="Center"
          Visibility="{d:Binding Thumbnail, Converter={StaticResource VisibleWhenNull}}" />
<Image Source="{d:Binding Thumbnail}" Stretch="UniformToFill" />
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/App.xaml`

**Also shown by.**
`NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml`,
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml`

**Sharp edges.**
- The same reasoning applies to the application font: set the default text font
  and the script fallbacks from a bundled font package rather than trusting the
  host. See the font blueprint in the startup area.

