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
panels generated from a descriptor or by reflection. Another group assembles
the shell of an editor from a command model rather than from markup - menus,
toolbars, keyboard shortcuts, a tabbed document area with a toolbox and side
pads - together with the wiring that forwards pointer, wheel and keyboard
input from the view into a model that references no UI types. A last group is
about a page that draws a scene instead of arranging controls: a whole pointer
gesture from press through drag to a snap onto the nearest target, a press told
apart from a drag and a double click timed in the page, one method that rebuilds
every size in the scene when the window changes, a control whose face is drawn
in code inside a fixed design box, a fallback chain that gets run-time path
geometry past a parser that rejects arcs, animations that end in the right value
even where animation support is thin, and letter spacing faked with thin spaces.
Running through all of it is one question about the page's own code: how little
of it there can be. So the file also covers the wiring itself - subscribing to
a view model once and unsubscribing when the page unloads, passing a getter
rather than capturing an object that is not there yet, applying layout values the
view model worked out, and routing a container control's own chrome back to
the item's command. Reach for this file whenever you are writing markup or
page code-behind, and when you need to know which small amount of work
legitimately belongs in the view rather than in a SimpleViewModel.

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
- [Set the PasswordBox mask character back to WinUI's black circle](#set-the-passwordbox-mask-character-back-to-winuis-black-circle)
- [Host an unmodified guest program in one page element](#host-an-unmodified-guest-program-in-one-page-element)
- [Host a live chart with the PlotterView add-in and bind the model the view model owns](#host-a-live-chart-with-the-plotterview-add-in-and-bind-the-model-the-view-model-owns)
- [Host a control with no dependency properties by mirroring a collection from code-behind](#host-a-control-with-no-dependency-properties-by-mirroring-a-collection-from-code-behind)
- [Generate a form from a parameter list with one template and per-editor Visibility](#generate-a-form-from-a-parameter-list-with-one-template-and-per-editor-visibility)
- [Show mask and copy a secret in a one-line row](#show-mask-and-copy-a-secret-in-a-one-line-row)
- [Wire a control before the DataContext arrives by passing a getter](#wire-a-control-before-the-datacontext-arrives-by-passing-a-getter)
- [Subscribe to a view model once and unsubscribe when the page unloads](#subscribe-to-a-view-model-once-and-unsubscribe-when-the-page-unloads)
- [Decide portrait or landscape on the view model and apply it from the page](#decide-portrait-or-landscape-on-the-view-model-and-apply-it-from-the-page)
- [Build a row of buttons from a palette with one item template](#build-a-row-of-buttons-from-a-palette-with-one-item-template)
- [Route a container's chrome button to the item's own command](#route-a-containers-chrome-button-to-the-items-own-command)
- [Report a control's load failure with an event and a log line](#report-a-controls-load-failure-with-an-event-and-a-log-line)
- [Drag a card across the scene and snap it to the nearest station](#drag-a-card-across-the-scene-and-snap-it-to-the-nearest-station)
- [Tell a press from a drag and hand roll a double click](#tell-a-press-from-a-drag-and-hand-roll-a-double-click)
- [Rebuild the whole scene geometry in one Relayout method](#rebuild-the-whole-scene-geometry-in-one-relayout-method)
- [Draw a control face procedurally inside a fixed design box](#draw-a-control-face-procedurally-inside-a-fixed-design-box)
- [Parse path data through a fallback chain that flattens arcs](#parse-path-data-through-a-fallback-chain-that-flattens-arcs)
- [Begin every Storyboard inside a try that sets the final value](#begin-every-storyboard-inside-a-try-that-sets-the-final-value)
- [Fake letter spacing with thin spaces](#fake-letter-spacing-with-thin-spaces)

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
binding the same view model with plain `{Binding ...}`,
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml`
(the view model declared in `<Page.DataContext>`, `xmlns:d` aliasing the
platform's data namespace for every `{d:Binding}`, and the page's own control
namespace taking the `using:` form because that control compiles into the head
beside the page while the view model comes from another assembly)

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
slider brushes so the audio scrubber follows the palette),
`InannaRosette/src/InannaRosette.UI/App.xaml`
(the whole `TextControl…` family and the whole `ContentDialog…` family redeclared
from the same sixteen-color palette that is two sections above them in the same
dictionary, so the header's text boxes and the application's own dialogs follow
it with no retemplating at all)

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
/// Which colour role each keyed brush in App.xaml carries. Applying a scheme is walking this
/// table, looking the key up in the resource dictionaries and assigning the role's colour to the
/// brush that is already there, which repaints every consumer without touching the markup.
/// Keys whose colour is the same in every scheme - the fully transparent faces and the dialog
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
/// Paints a colour scheme: the element theme decides the chrome this application does not
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
/// Re-points an existing brush at another colour, which repaints everything drawn with it.
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
tells the view model when it changes - through `IManageColorScheme`, the matching half of
the bridge the view model implements, rather than through the view model's own type.
Reading the preference is the page's job; deciding what the number means is not, so the
perceived-brightness rule sits beside the palettes in the shared library. The one rule
that decides everything else is that setting `Application.RequestedTheme` at all is what
makes the platform stop following the operating system - so for the system choice it is
never set.

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

// ...

/// <summary>
/// Reads the operating system's light or dark preference out of the color it says it would
/// paint a window with, which is the form every head reports that preference in. The weights
/// are the usual perceived-brightness ones, and a ground below the mid point is a dark one.
/// </summary>
public static bool PrefersDark(byte red, byte green, byte blue)
{
    var brightness = (red * 0.299d) + (green * 0.587d) + (blue * 0.114d);
    return brightness < 128d;
}
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

The page watches the operating system, asks the scheme table what the reading means, and
hands the answer to the view model. The `UISettings` instance has to be a field:

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
        //as the thing that can paint a colour scheme.
        (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);
        (DataContext as IManageColorScheme)?.AttachSchemeApplier(this, SystemPrefersDark());
    };

    _systemColors.ColorValuesChanged += (_, _) => DispatcherQueue.TryEnqueue(() =>
        (DataContext as IManageColorScheme)?.OnSystemThemeChanged(SystemPrefersDark()));

    this.InitializeComponent(); //Leave this line last
}

//The operating system reports its preference as the colour it would paint a window with.
//Reading it is the page's job; deciding what it means belongs with the scheme table.
private bool SystemPrefersDark()
{
    var background = _systemColors.GetColorValue(UIColorType.Background);
    return ColorSchemes.PrefersDark(background.R, background.G, background.B);
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
- Keep the brightness arithmetic out of the page. The page reads a color from the
  platform; the rule that turns that color into "this desktop is dark" is a decision, it
  belongs beside the palettes, and once it is there a test can pin it without a window.
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
like. It sits in the shared library too, so the page reaches it through an assembly
qualified namespace:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/Converters/TimecodeConverter.cs
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
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/Converters/TimecodeConverter.cs` and
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml` (the `xmlns:conv`
declaration and the `Page.Resources` entry)

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
- A converter that lives in the shared library rather than beside the page needs the
  `clr-namespace:<Namespace>;assembly=<Assembly>` form in the page's `xmlns`. Without
  the assembly part the markup compiler looks only in the head project and the key
  silently fails to resolve.

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
    <TextBlock Text="{d:Binding}"
               FontSize="11"
               TextWrapping="Wrap"
               Margin="0,2,0,0"
               Foreground="{StaticResource AppMutedTextBrush}" />
</ui:DataTemplate>

<!-- ... -->

<Border Grid.Row="4"
        Background="{StaticResource AppRaisedPanelBrush}"
        BorderBrush="{StaticResource AppDividerBrush}"
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

A load that fails cannot throw out of a property-changed callback, so the control
reports it twice instead - to the application log, and as a `LoadFailed` event.
The page reaches the control through the image button in the next recipe, and keeps the
bare form beside it as a commented example of the standalone control:

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml -->
<controls:EmbeddedImageButton Margin="0,0,20,0" Width="140" Height="90"
    VerticalAlignment="Center" HorizontalAlignment="Right"
    Background="#FFB85555"
    Command="{d:Binding EncryptCommand}"
    ImageUriSource="embedded://JustBetweenUs.Core/JustBetweenUs.Assets.padlock-icon.svg"
    Text="Encrypt" ImageWidth="40" ImageHeight="40" Spacing="6" ImagePosition="Top" />

<!--Example of using EmbeddedImage standalone (not inside a button)-->
<!--<controls:EmbeddedImage Margin="20,0,0,0" Width="60" Height="60"
    VerticalAlignment="Center"
    UriSource="embedded://JustBetweenUs.Core/JustBetweenUs.Assets.padlock-icon.svg" />-->
```

**Where to look.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImageFailedEventArgs.cs`
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
- A load failure leaves the image empty rather than throwing, so the control logs it
  through the ambient logger factory and raises `LoadFailed` for anything that wants
  to say so on screen. See
  [Report a control's load failure with an event and a log line](#report-a-controls-load-failure-with-an-event-and-a-log-line).
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
    // ... TextVerticalAlignment and TextHorizontalAlignment dependency properties,
    // ... every one of them registered with OnLayoutPropertyChanged ...

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

            if (!hasImage && !hasText)
            {
                Content = null;
                return;
            }

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
            else if (hasImage)
            {
                Content = CreateImage();
            }
            else
            {
                Content = CreateTextBlock();
            }
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
    ImageUriSource="embedded://JustBetweenUs.Core/JustBetweenUs.Assets.clipboard.svg"><!--ImagePosition="Right"-->
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

**The MVVM shape.** Mostly pure layout. Each group is one child of the panel;
`FlexPanel.Grow` decides who absorbs the slack and `FlexPanel.Basis` makes the
wrap point deterministic, and a header that only wraps needs no code at all. Where
the axis itself flips, the page still owns the assignment to the panel - but which
way round the window is, and what the pane's width, basis and margin should then
be, are answers the view model can give, which is how PolyHavenBrowser does it.

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
                  Margin="0,0,20,0" Width="420">
        <!-- ... the fact cards ... -->
    </ScrollViewer>

    <!-- ...
         Grow=1: the viewer takes whatever main-axis space the info pane leaves -->
    <Grid RowSpacing="8" flex:FlexPanel.Grow="1">
        <!-- ... the GL canvas and its hint line ... -->
    </Grid>
</flex:FlexPanel>
```

The page applies the axis, the width, the basis and the margin; it does not work
any of them out:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
SizeChanged += (_, args) =>
{
    var viewModel = ViewModel;
    if (viewModel == null) { return; }

    viewModel.NotifyWindowSizeChanged(args.NewSize.Width, args.NewSize.Height);

    var stacked = viewModel.IsModelViewStacked;
    ModelContentFlex.Direction = stacked ? FlexDirection.Column : FlexDirection.Row;
    ModelInfoPane.Width = viewModel.ModelInfoPaneWidth;
    FlexPanel.SetBasis(ModelInfoPane, stacked
        ? new FlexBasis(viewModel.ModelInfoPaneStackedHeightBasis, isRelative: true)
        : FlexBasis.Auto);
    ModelInfoPane.Margin = viewModel.ModelInfoPaneMargin;
};
```

The properties it reads back are in
[Decide portrait or landscape on the view model and apply it from the page](#decide-portrait-or-landscape-on-the-view-model-and-apply-it-from-the-page).

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml` and
`Views/MainPage.xaml.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml` and
`Views/MainPage.xaml.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs` (the pane
width, basis and margin the handler reads)

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

**The MVVM shape.** The forwarding is four handlers of a few lines each, plus a
pointer capture while a gesture is in progress. The model decides whether a press
starts anything and tracks whether a gesture is active, so the view holds no state
of its own and the view model is not on the per-point path at all. Where an
application has more than one head, the four handlers belong in one shared helper
that every head calls rather than in each code-behind.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/Drawing/DrawingCanvasBinder.cs
public static void BindToSession(this DrawingCanvas canvas, Func<DrawingSession> sessionGetter)
{
    if (canvas == null || sessionGetter == null) { return; }

    canvas.PaintSurface += (_, e) => sessionGetter()?.Render(e.Surface, e.Info);

// ...
    canvas.PointerPressed += (_, e) =>
    {
        var session = sessionGetter();
        if (session == null) { return; }

        var pointerPoint = e.GetCurrentPoint(canvas);
        if (!pointerPoint.Properties.IsLeftButtonPressed) { return; }

        if (session.PointerPressed(DrawCanvasHelper.GetPointFromPosition(pointerPoint.Position), canvas.GetViewSize()))
        {
            canvas.CapturePointer(e.Pointer);
            e.Handled = true;
        }
    };

    canvas.PointerMoved += (_, e) =>
    {
        var session = sessionGetter();
        if (session is not { IsPointerActive: true }) { return; }

        session.PointerMoved(DrawCanvasHelper.GetPointFromPosition(e.GetCurrentPoint(canvas).Position), canvas.GetViewSize());
        e.Handled = true;
    };

    canvas.PointerReleased += (_, e) =>
    {
        var session = sessionGetter();
        if (session is not { IsPointerActive: true }) { return; }

        session.PointerReleased();
        canvas.ReleasePointerCapture(e.Pointer);
        e.Handled = true;
    };

    //If capture is lost mid-stroke (e.g. the window deactivates), discard the stroke
    canvas.PointerCaptureLost += (_, _) => sessionGetter()?.PointerCanceled();

    //SKXamlCanvas does not repaint itself when it is resized
    canvas.SizeChanged += (_, _) => canvas.Invalidate();
// ... the native WPF head's mouse-event branch does the same five things ...
}
```

Every head's code-behind is then one line:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs
//Paint, press, move, release and capture-lost all go straight to the drawing session
DrawCanvas.BindToSession(() => ViewModel?.Session);
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
units, the page converts before forwarding - and here the view model itself owns
the gesture, so the handler passes the point and the timestamp on and does nothing
with them:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
DisplayCanvas.PointerPressed += (_, e) =>
{
    var point = e.GetCurrentPoint(DisplayCanvas);
    if (!point.Properties.IsLeftButtonPressed) { return; }

    var (x, y) = ToCanvasPixels(point.Position);
    if (ViewModel?.PointerPressed(x, y, point.Timestamp) != true) { return; }

    DisplayCanvas.CapturePointer(e.Pointer);
    e.Handled = true;
};

// ...

// Maps a pointer position (in view/DIP units) to the canvas's pixel space, so pointer
// input stays aligned with the rendered pixels at any DPI and after any window resize -
// the coordinate robustness the PainDiagram sample demonstrates.
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
`PainDiagram/Shared/Drawing/DrawingCanvasBinder.cs`
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`

**Also shown by.**
`PainDiagram/Shared/Drawing/DrawingCanvasBinder.cs` again, in its `#else` branch:
the same five subscriptions with the WPF event names - mouse down, move, up and
lost-capture, with `CaptureMouse()` - so the native WPF window's code-behind is
the same single `BindToSession` call as every other head's.

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
- A shared binder must take the session as a getter rather than as a value: the
  wiring runs before the `DataContext` arrives. See
  [Wire a control before the DataContext arrives by passing a getter](#wire-a-control-before-the-datacontext-arrives-by-passing-a-getter).

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
surface, its image info and the frame source. There is one renderer per canvas,
and it is owned by whoever decides what that canvas shows: the page owns the
self-view's renderer, because a mirrored preview is all that canvas ever shows,
while the main canvas can show either the live preview or the painting, so its
renderer belongs to the view model and the page's paint handler is one forward
through a bridge.

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
xmlns:webcam="clr-namespace:WebcamPainter.Webcam;assembly=WebcamPainter.Webcam"

<!-- ... -->

<Border BorderBrush="Gray" BorderThickness="1" Background="Black" Height="150">
    <webcam:CameraCanvas x:Name="SelfViewCanvas" />
</Border>
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs
//One frame renderer per canvas that shows live video (each caches its own buffers); the
//  main canvas's renderer is the view model's, because the view model is what decides
//  whether that canvas is showing video or the painting
private readonly WebcamFrameRenderer _selfViewRenderer = new WebcamFrameRenderer();
// ...
//Which of the two things the main canvas shows is application state, so the handler
//  forwards the surface and lets the view model draw (see ICanvasBridge)
MainCanvas.PaintSurface += (_, e) =>
    (DataContext as ICanvasBridge)?.RenderMainCanvas(e.Surface, e.Info);

SelfViewCanvas.PaintSurface += (_, e) =>
    _selfViewRenderer.Render(e.Surface, e.Info, ViewModel?.CaptureService, mirror: true);

MainCanvas.SizeChanged += (_, _) => MainCanvas.Invalidate();
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Webcam/CameraCanvas.cs`
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml` and `Views/MainPage.xaml.cs`

**Also shown by.**
`PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraCanvas.cs` and
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs` - the same renderer
shape, except that it takes an `IWebcamFrameSource` rather than the capture
service, so the view model can implement the one method the renderer needs and
keep the device to itself.

**Sharp edges.**
- Create one renderer per canvas. The cached framebuffer and bitmap are reused
  across paints and are only touched on the UI thread, so sharing one renderer
  between two canvases would race.
- Put the renderer where the decision is. A canvas that always shows the same
  thing can keep its renderer in the page; a canvas whose content depends on
  application state should be painted by the view model through a bridge, or the
  page ends up reading mode flags in a paint handler.
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

**The MVVM shape.** Only one number in the whole calculation belongs to the page:
how big the viewer is. So the page measures that, hands it over, and applies the
four values it gets back - an image width and height, and a horizontal and
vertical scroll offset. Everything between those two ends is a static method on a
record in the library, which is why it can be checked without a window.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/PaneLayout.cs
public static PaneLayout Create(double pageWidth, double pageHeight, double viewportWidth,
    double viewportHeight, double zoomFactor, PanPosition pan)
{
    if (pan == null || zoomFactor <= 0 || pageWidth <= 0 || pageHeight <= 0
        || viewportWidth <= 0 || viewportHeight <= 0)
    {
        return None;
    }

    var fit = Math.Min(viewportWidth / pageWidth, viewportHeight / pageHeight);
    var imageWidth = Math.Floor(pageWidth * fit * zoomFactor);
    var imageHeight = Math.Floor(pageHeight * fit * zoomFactor);

    //Whatever the image overflows its viewer by is the scrollable range the fractions apply to
    return new PaneLayout(
        imageWidth,
        imageHeight,
        pan.Horizontal * Math.Max(0, imageWidth - viewportWidth),
        pan.Vertical * Math.Max(0, imageHeight - viewportHeight));
}
```

The view supplies the two values that are its own - the shared zoom and this
pane's pan - and the same file keeps the other piece of view arithmetic, how far
one pan step moves:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/ComparisonView.cs
public PaneLayout LayoutOf(DocumentSide side, double pageWidth, double pageHeight,
    double viewportWidth, double viewportHeight) =>
    PaneLayout.Create(pageWidth, pageHeight, viewportWidth, viewportHeight, Zoom.Factor, PanOf(side));

/// <summary>
/// One pan step as a fraction of the scrollable range. At zoom factor <c>f</c> the page is
/// <c>f</c> viewports wide, so the scrollable range is <c>f - 1</c> viewports and a quarter
/// of a viewport is <c>0.25 / (f - 1)</c> of it. Zero at 100%, where nothing scrolls.
/// </summary>
public double PanStepFraction => Zoom.IsZoomedIn ? PanStepOfViewport / (Zoom.Factor - 1) : 0;
```

The view model takes the viewport size and stores the answer on the pane, whose
four bindable values notify as a set:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
public void SetViewportSize(DocumentSide side, double viewportWidth, double viewportHeight)
{
    var pane = PaneFor(side);
    if (pane == null) { return; }

    pane.SetLayout(View.LayoutOf(side, pane.PagePixelWidth, pane.PagePixelHeight,
        viewportWidth, viewportHeight));
}
```

And the page measures and applies, with no arithmetic left in it:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs
private void ApplyView(DocumentSide side)
{
    var viewModel = ViewModel;
    if (viewModel == null) { return; }

    var pane = side == DocumentSide.Left ? viewModel.LeftPane : viewModel.RightPane;
    var scroller = side == DocumentSide.Left ? LeftScroller : RightScroller;
    var image = side == DocumentSide.Left ? LeftImage : RightImage;

    //Only the page knows how big the viewer is; the arithmetic belongs to the view model
    viewModel.SetViewportSize(side, scroller.ActualWidth, scroller.ActualHeight);

    image.Width = pane.ImageWidth;
    image.Height = pane.ImageHeight;
    if (double.IsNaN(pane.ImageWidth)) { return; }

    //Let the viewer measure the new extent before positioning it
    scroller.UpdateLayout();
    scroller.ChangeView(pane.ScrollOffsetX, pane.ScrollOffsetY, null, disableAnimation: true);
}
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/PaneLayout.cs`
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/ComparisonView.cs`
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/PanPosition.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/DocumentPaneViewModel.cs`
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Call `UpdateLayout()` before changing the view: the scrollable extents are stale
  until the viewer has measured the newly sized content, so scrolling first lands
  in the wrong place.
- Store pan as a fraction of the scrollable range, not as pixels, which is exactly
  what makes it survive a zoom change.
- Guard the fully-zoomed-out case where nothing scrolls at all.
- Disable the scroll viewer's own zoom when the application has its own zoom
  ladder; two zooms fight.
- Re-apply the size and the pan on size changes and on the view-version change;
  missing either one leaves the image the wrong size. The pane folds its own
  changes into that one version counter, so the page has one thing to watch.
- Return the offsets rather than the fractions. The fraction is the durable form
  to store, but the page would have to ask the viewer for its scrollable range to
  use one, and that range is stale until the new content has been measured.
- Make the layout a value type and compare before notifying, or every resize
  raises four property changes whether anything moved or not.

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
// ...
// application: typing reaches a TextBox normally, while Ctrl+Z, Ctrl+Y and
// Ctrl+H registered on a Page or on a MenuFlyoutItem never invoke.
//
// So the shortcuts are dispatched here instead, from a single KeyDown handler
// ...
```

The elided line records how that was established: by driving the running
application on an X11 head and watching which keys arrived.

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
forwards the gesture to a view-model method that decides what it means and
returns whether it did anything - which is exactly what the handler needs to
report back as "handled". The `CanExecute` check goes with the decision, in the
view model, not in the page.

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
    if (e.Key == Windows.System.VirtualKey.Enter && DataContext is MainViewModel viewModel)
    {
        e.Handled = viewModel.SubmitSearch();
    }
}
```

The page recognizes the key; the view model decides whether the gesture means
anything right now. Eight heads share that method, and a test can call it:

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
/// <summary>
/// Runs the search exactly as the Search button does, for a page that forwards a key
/// gesture (Enter in the search box) instead of raising the command itself. Returns true
/// when the search ran, which is what such a handler reports as "handled".
/// </summary>
public bool SubmitSearch()
{
    if (!SearchCommand.CanExecute(null)) { return false; }

    SearchCommand.Execute(null);
    return true;
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
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml.cs`
`GitHubIssueFinder/src/GitHubIssueFinder.UI/Views/MainPage.xaml`

**Sharp edges.**
- Check `CanExecute` before invoking; a key handler bypasses the disabled state a
  button would have honored. Put that check in the view-model method the handler
  forwards to, so every head gets it, and let the method's return value be what
  the handler assigns to `e.Handled`.
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
// and blocks the whole window, which defeats the preview. Every effect
// configuration dialog in the port goes through this host.

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

**The MVVM shape.** The XAML declares the grid and the named hosts; the panels
inside the hosts are built at load time from the engine's action model. What is
plain text or a plain command, though, is bound: the status bar's three readouts
and the zoom controls are view-model properties and commands, not named elements
the code-behind writes to. In a fuller view-model shape the toolbox and pad lists
would bind to observable collections as well, but the container layout would be
identical.

**Code.**

Five rows: the menu bar, the icon toolbar, the tool options bar, the row that
carries the toolbox, the tabs, the splitter and the pads, and the status bar.

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml -->
<Grid.RowDefinitions>
    <RowDefinition Height="Auto" />
    <RowDefinition Height="Auto" />
    <RowDefinition Height="Auto" />
    <RowDefinition Height="*" />
    <RowDefinition Height="Auto" />
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

The status bar is bound rather than named and written to: the view model keeps
the three texts, fed from the engine's own events.

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml -->
<!-- MaxLines keeps the bar one line tall: the shape tools carry
     many-line StatusBarText and would otherwise grow the bar. -->
<TextBlock Grid.Column="1" VerticalAlignment="Center" TextTrimming="CharacterEllipsis" MaxLines="1"
           Text="{d:Binding StatusText}" />
<TextBlock Grid.Column="2" VerticalAlignment="Center" MinWidth="80"
           Text="{d:Binding CursorPositionText}" />
<TextBlock Grid.Column="3" VerticalAlignment="Center" MinWidth="80"
           Text="{d:Binding SelectionSizeText}" />
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
- A named `TextBlock` the code-behind assigns to is the shape to grow out of
  first. Binding it costs one property on the view model and removes a page field,
  a null check and an ordering rule.
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

### Set the PasswordBox mask character back to WinUI's black circle

**When you want this.** The application's password boxes should be masked the way
they are on Windows, with the black circle U+25CF, rather than with the platform's
default bullet U+2022 - on one box, or on every box in the application.

**The MVVM shape.** None. `PasswordChar` is a property of the control, set in
markup; the view model never knows what the mask looks like.

**Code.**

Why there is anything to do: the platform's default mask is the bullet, on every
head, which is an intentional divergence from WinUI. A CodeBrix.Platform
application draws its text from the fonts it ships, and the black circle is missing
from every face of the Open Sans and Roboto Mono packages (Roboto and Merriweather
carry it), while the bullet is in every face of every application-font package. So
the default mask is always drawn from the application's own font, and a
`PasswordBox` is exactly as tall as a `TextBox` of the same font and size, empty or
full. Asking for the circle is a choice you make knowing which font you ship.

One box:

```xml
<!-- The black circle on this box only. The character can also be typed literally
     in place of the entity. -->
<PasswordBox Width="250" PlaceholderText="Password" PasswordChar="&#x25CF;" />
```

Every box in the application, from `App.xaml`: an implicit style based on the
platform's own default style, carrying just the one setter.

```xml
<!-- In App.xaml, inside Application.Resources, after the XamlControlsResources merge. -->
<Style TargetType="PasswordBox" BasedOn="{StaticResource DefaultPasswordBoxStyle}">
    <Setter Property="PasswordChar" Value="&#x25CF;" />
</Style>
```

A box that sets its own `PasswordChar` still wins over the style, so the two forms
combine: the application's boxes show the circle, and one box can show something
else.

Whether the circle can be drawn at all depends on the font the box uses. A glyph
the application's font lacks is drawn from a host font on the desktop heads, if
the host has one with that glyph, and comes out in that font's shape and weight
rather than the application's; on a device where the application cannot reach
system fonts, or has none to reach, it renders as the font's missing-glyph shape,
which is blank in Open Sans and a box in Roboto. The box's height does not change
either way, because a line is never laid out shorter than its own font's line
height, but its look does. If the circle matters, choose Roboto or Merriweather as
the application font, or declare a companion face that carries it the way
[Set a bundled font as the default text font and register script fallbacks](BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks)
shows; otherwise stay with the default bullet.

**Where to look.**
No application in this repository does this. Both forms were proven with a
throwaway change to the CodeBrix.Platform repository's EvaluateUIElementsDemo
sample on the Linux X11 head: with the application-wide style every default box
switched to circles, a box with its own `PasswordChar` kept it, and no box changed
height.

**Related.**
[Take a secret token in a PasswordBox and keep it out of storage](#take-a-secret-token-in-a-passwordbox-and-keep-it-out-of-storage)
is the box this recipe changes the look of.
[Keep a text box's own colors while it is hovered or focused](BLUEPRINTS-ThemingAndStyling.md#keep-a-text-boxs-own-colors-while-it-is-hovered-or-focused)
is the other thing an application tends to want from the same control.
[Set a bundled font as the default text font and register script fallbacks](BLUEPRINTS-AppStructureAndStartup.md#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks)
is where the application decides which fonts a glyph can come from.

**Sharp edges.**
- The application-wide style must be `BasedOn` `DefaultPasswordBoxStyle`. An
  implicit style without it replaces the platform's default style, and the box
  loses its template along with it.
- `PasswordChar` is exactly one character; the platform throws on an empty or a
  longer string.
- The circle is a font question before it is a style question: Open Sans and
  Roboto Mono do not carry it, Roboto and Merriweather do, and a host font is not
  something an application can count on.
- The bullet, the asterisk and the middle dot are in every face of every
  application-font package; anything else needs checking against the font you ship.
- The symbols font and the music font are not text fonts and never mask a password.

### Host an unmodified guest program in one page element

**When you want this.** The application you are shipping is somebody else's
program - a script program, an interpreted editor, a toolkit UI - and you want it
running inside a CodeBrix.Platform window without rewriting it as XAML. The whole
guest user interface, menus and dialogs included, has to draw into one element on
one page, and the application must contribute no controls of its own.

**The MVVM shape.** The page owns the hosting element and its lifecycle, because
only the page has the element and only the page knows when it has been loaded. It
holds one small facade type - here a `RuntimeHost` with a start method and a
dispose - so the code-behind never sees the interpreter, the toolkit bridge or the
boot sequence. The view model is a `SimpleViewModel` that stays empty on purpose:
the guest owns every pixel, so there is nothing to bind yet, and the design-mode
guard and the empty regions are kept so application state has somewhere to go the
day the application grows a control of its own.

**Code.**

```xml
<!-- From CodeBrix.Samples/DRAKON.Brix/src/DRAKON.Brix.UI/Views/MainPage.xaml -->
<Page
    x:Class="DRAKON.Brix.Views.MainPage"
    xmlns="clr-namespace:Microsoft.UI.Xaml.Controls;assembly=CodeBrix.Platform.UI"
    xmlns:d="clr-namespace:Microsoft.UI.Xaml.Data;assembly=CodeBrix.Platform.UI"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="clr-namespace:DRAKON.Brix.ViewModels;assembly=DRAKON.Brix.Core"
    xmlns:local="using:DRAKON.Brix.Views"
    xmlns:tkhost="clr-namespace:CodeBrix.Platform.TkCanvas.Hosting;assembly=CodeBrix.Platform.TkCanvas"
    FontFamily="{StaticResource OpenSansFont}"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">

    <Page.DataContext>
        <vm:MainViewModel />
    </Page.DataContext>

    <!-- The entire DRAKON Editor UI is built by its own (unmodified) Tcl
         through the TkCanvas command bridge; this page only hosts the
         single Tk surface. -->
    <Grid>
        <tkhost:TkHostView x:Name="TkHost" />
    </Grid>
</Page>
```

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/DRAKON.Brix.UI/Views/MainPage.xaml.cs
// ...
public sealed partial class MainPage : Page
{
    private readonly RuntimeHost _runtimeHost = new RuntimeHost();

    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);
        };

        //The DRAKON Editor Tcl builds the whole UI; this page just starts the
        //  Tcl runtime once the Tk host (and its dispatcher) is live, and disposes
        //  it on unload. RuntimeHost keeps this page decoupled from DrakonRuntime.
        Loaded += (s, e) => _runtimeHost.Start(TkHost);
        Unloaded += (s, e) => _runtimeHost.Dispose();

        this.InitializeComponent(); //Leave this line last
    }
}
```

The facade is the whole of what the page is allowed to know, and it is guarded so
that a double start or a double dispose costs nothing:

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/RuntimeHost.cs
public sealed class RuntimeHost : IDisposable
{
    private DrakonRuntime _runtime;

    /// <summary>
    /// Creates and starts the DRAKON runtime inside the given host view. Call
    /// once, from the UI thread, after the host has loaded (its tree and
    /// dispatcher exist). Subsequent calls are ignored.
    /// </summary>
    /// <param name="host">The loaded Tk host view.</param>
    public void Start(TkHostView host)
    {
        if (host == null) { throw new ArgumentNullException(nameof(host)); }
        if (_runtime != null) { return; }

        _runtime = new DrakonRuntime();
        _runtime.Start(host);
    }

    /// <summary>
    /// Stops the Tcl thread and disposes the runtime. Safe to call more than
    /// once, and safe to call when <see cref="Start"/> was never called.
    /// </summary>
    public void Dispose()
    {
        DrakonRuntime runtime = _runtime;
        _runtime = null;
        if (runtime != null) { runtime.Dispose(); }
    }
}
```

**Where to look.**
`DRAKON.Brix/src/DRAKON.Brix.UI/Views/MainPage.xaml`
`DRAKON.Brix/src/DRAKON.Brix.UI/Views/MainPage.xaml.cs` and
`src/libs/DRAKON.Brix.TclBridge/RuntimeHost.cs`
`DRAKON.Brix/src/DRAKON.Brix.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The loaded event, not the constructor, is the moment to start. The hosting
  element's window tree and its dispatcher only exist once the page has been
  loaded, and the guest needs both.
- The hosting namespace is declared in the same assembly-qualified
  `clr-namespace:...;assembly=...` form as the platform's own namespaces; the
  `using:` form is only for the application's own types.
- Dispose reads the field into a local, nulls the field and only then disposes, so
  a second unload or a concurrent teardown cannot double-dispose. That guard is
  the facade's job and it matters: the runtime one level down guards its start
  with a started flag but not its dispose, and teardown here can be begun by the
  guest, by the window closing or by a page unload.
- Keep the empty view model rather than deleting it. It costs nothing, it keeps
  the data context shape every other page in a repository uses, and it is where
  the first real bound property will go.

### Host a live chart with the PlotterView add-in and bind the model the view model owns

**When you want this.** A page needs a real chart - axes, a legend, pan, zoom and
a tracker - fed by data the application produces, on every head. This is the
opposite shape to
[Host the VideoPlayer add-in in a page and drive it from the view model](BLUEPRINTS-MediaAndVision.md#host-the-videoplayer-add-in-in-a-page-and-drive-it-from-the-view-model):
that add-in owns its own state, so the page has to implement a bridge interface
over it. The chart's whole state is a plain model object, so the view model owns
the model and one binding is the entire seam.

**The MVVM shape.** A UI-free class in the Core library builds the plot model,
holds it as a get-only property, and exposes the application's vocabulary -
show a captured block, append a streaming batch, show or hide a channel. The
view model owns one instance of that class and exposes it. The page binds the
control's `Model` property through it and has no chart code at all.

**Code.**

```xml
<!-- From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.UI/Views/MainPage.xaml -->
xmlns:plot="clr-namespace:CodeBrix.Platform.UI.PlotterView;assembly=CodeBrix.Platform.UI.PlotterView"
...
<!-- The chart. A bounded star cell, so the control has a real size to render into. -->
<Border Grid.Row="4" BorderBrush="#3A3A46" BorderThickness="1">
    <plot:PlotterControl x:Name="Plotter" Model="{d:Binding Plot.Model}" />
</Border>
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/ViewModels/MainViewModel.cs
/// <summary>
/// The chart. Its <see cref="ScopePlot.Model"/> is what the view renders.
/// </summary>
public ScopePlot Plot { get; } = new ScopePlot();
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/Charting/ScopePlot.cs
/// <summary>
/// Creates a plot, building the axes and legend.
/// </summary>
public ScopePlot()
{
    Model = BuildModel();
}

/// <summary>
/// The plot model to render. Hand this to a plot view; do not mutate it
/// directly.
/// </summary>
public PlotModel Model { get; }
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/PicoScope.Brix.Core/Charting/ScopePlot.cs
var model = new PlotModel
{
    Title = "PicoScope.Brix",
    //Set explicitly, and not only for looks. The text and gridline
    //  colours below are chosen for a dark background; without a
    //  Background on the model an exported PNG comes out white and the
    //  title, legend and grid are all but invisible. The plot view
    //  clears to the model's background, so screen and export match.
    Background = PlotterColor.FromRgb(24, 24, 30),
    PlotAreaBorderColor = PlotterColor.FromRgb(90, 90, 100),
    TextColor = PlotterColor.FromRgb(220, 220, 225),
    TitleColor = PlotterColor.FromRgb(240, 240, 245),
    SubtitleColor = PlotterColor.FromRgb(170, 170, 180)
};
```

**Where to look.**
`PicoScope.Brix/src/PicoScope.Brix.UI/Views/MainPage.xaml`
`PicoScope.Brix/src/PicoScope.Brix.Core/Charting/ScopePlot.cs` and
`ViewModels/MainViewModel.cs`
`PicoScope.Brix/src/PicoScope.Brix.Core/PicoScope.Brix.Core.csproj`

**Sharp edges.**
- The control needs a bounded cell. A star row inside a `Border` gives it a real
  size; put it in an auto-sized row, or inside a stack panel, and it has nothing
  to render into.
- Set the model's background explicitly. On screen the control clears to it, so
  a model with no background of its own looks correct until something exports,
  and then the light text and gridlines land on white.
- The add-in brings the plotting engine with it, so the axis, series and color
  types come from a package the application never names. That is fine, and it is
  worth a comment in the project file saying so - see
  [Know what a transitive package brings and name what you depend on](BLUEPRINTS-ProjectLayoutAndPackaging.md#know-what-a-transitive-package-brings-and-name-what-you-depend-on).
- The chart class hands out the model and asks callers not to mutate it, because
  every mutation it makes is done under a lock. A page that reached past the
  binding and edited series directly would break that.
- Time axes read better with a plain decimal format than with the default:
  a scope's axis spans microseconds to seconds, and exponents are hard to read
  at a glance.

### Host a control with no dependency properties by mirroring a collection from code-behind

**When you want this.** A control you need declares no dependency properties, so
it cannot be bound, styled or placed inside a data template - and you want one of
them per item in a bound collection, created and destroyed as items come and go.
An items control cannot help you here, so the page has to do it by hand without
that turning into the page owning the state as well.

**The MVVM shape.** The view model still owns the list and each item's status as
ordinary bound properties, and the commands that add and remove items. The page
subscribes to the collection's change notification and mirrors it: one control per
item, created in code, torn down on removal. Everything that crossing takes is one
interface the view model implements, so the page never names the view-model type:
the collection to mirror, the call that opens the work behind a new item, and a
delegate the page fills in for what only it can do. What the page learns while the
item lives - the grid size, the state, a failure - it pushes back through a few
`Apply` methods on the item view model, so the status strip beside the control is
still plain bound XAML.

**Code.**

```xml
<!-- From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.xaml -->
<!-- The tab strip only. Each tab's body lives in the star row below, so
     the TerminalControl sits in a real bounded cell: a tab's own content
     presenter measures against an unbounded height, which leaves a star
     row inside it collapsed and the terminal a few rows tall. Bodies are
     built in code-behind because TerminalControl declares no dependency
     properties and so cannot live in a DataTemplate. -->
<TabView x:Name="ConsoleTabs" Grid.Row="0" VerticalAlignment="Top"
         IsAddTabButtonVisible="False"
         TabCloseRequested="ConsoleTabs_TabCloseRequested"
         SelectionChanged="ConsoleTabs_SelectionChanged" />
<Grid x:Name="ConsoleBodyHost" Grid.Row="1" />
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/Bridges/IConsoleTabsBridge.cs
public interface IConsoleTabsBridge
{
    /// <summary>The open console tabs, which the page mirrors into its tab strip.</summary>
    ObservableCollection<ConsoleTabViewModel> Tabs { get; }

    // ...
    Action<ConsoleTabViewModel, string> SendInput { get; set; }

    // ...
    Task<IExecSession> StartSessionAsync(ConsoleTabViewModel tab, int columns, int rows);
}
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.Consoles.cs
private void AttachConsoles(IConsoleTabsBridge consoles)
{
    if (consoles is null || _consoles is not null) { return; }

    _consoles = consoles;
    _consoles.Tabs.CollectionChanged += ConsoleTabs_ModelCollectionChanged;
    _consoles.SendInput = (model, text) =>
    {
        if (model is not null && _consoleHosts.TryGetValue(model, out var host))
        {
            host.Pump?.OnInput(text);
        }
    };
}

private void ConsoleTabs_ModelCollectionChanged(object sender,
    NotifyCollectionChangedEventArgs args)
{
    if (args.NewItems is not null)
    {
        foreach (var added in args.NewItems)
        {
            if (added is ConsoleTabViewModel model) { AddConsoleTab(model); }
        }
    }

    if (args.OldItems is not null)
    {
        foreach (var removed in args.OldItems)
        {
            if (removed is ConsoleTabViewModel model) { RemoveConsoleTab(model); }
        }
    }
}
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.Consoles.cs
private void AddConsoleTab(ConsoleTabViewModel model)
{
    // ... build the options, then the control, through the factory ...

    var status = new ContentControl
    {
        Content = model,
        ContentTemplate = (DataTemplate)Resources["ConsoleStatusTemplate"],
        HorizontalContentAlignment = HorizontalAlignment.Stretch,
        HorizontalAlignment = HorizontalAlignment.Stretch,
    };

    //TerminalControl's grid follows the control's pixel size, so it must land in a bounded
    //  cell; the body stretches into ConsoleBodyHost's star row and gives it one.
    var body = new Grid
    {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
    };
    // ... an auto row for the status strip and a star row for the control ...

    //The tab carries no content: a tab's content presenter measures its child against an
    //  unbounded height, which leaves a star row inside it collapsed and the terminal a few
    //  rows tall. The body goes into ConsoleBodyHost instead, a real star cell in the page's
    //  own grid, and the tab strip only selects which body is showing.
    var tab = new TabViewItem { Header = model.ContainerName };

    var host = new ConsoleHost { Tab = tab, Terminal = terminal, Model = model, Body = body };
    _consoleHosts[model] = host;

    ConsoleBodyHost.Children.Add(body);

    //The control drops anything fed to it before Loaded, so the exec is opened and the pump
    //  started there - which is also what makes a tab created while this section is hidden
    //  work: Loaded arrives when the section becomes visible.
    terminal.Loaded += (_, _) => _ = StartConsoleAsync(host, options);

    ConsoleTabs.TabItems.Add(tab);
    ConsoleTabs.SelectedItem = tab;
    ShowConsoleBody(host);
}

//Exactly one console body is visible at a time; the others stay in the tree, collapsed, so
//  their terminals remain loaded and keep receiving their sessions' output.
private void ShowConsoleBody(ConsoleHost selected)
{
    foreach (var pair in _consoleHosts)
    {
        pair.Value.Body.Visibility = ReferenceEquals(pair.Value, selected)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/ViewModels/ConsolesViewModel.cs
/// <summary>The open console tabs. The page mirrors this into its tab strip.</summary>
public ObservableCollection<ConsoleTabViewModel> Tabs { get; } = [];

// ...

/// <summary>
/// Opens a console tab on a container. The page notices the new tab through the collection
/// and does the rest: it builds a terminal, asks for the tab's session and starts the pump.
/// </summary>
public ConsoleTabViewModel OpenConsole(string containerId, string containerName)
{
    if (string.IsNullOrEmpty(containerId)) { return null; }

    IsPickerOpen = false;
    var tab = new ConsoleTabViewModel(containerId, containerName, CloseTab, Reopen);
    Tabs.Add(tab);
    NotifyPropertyChanged(nameof(EmptyVisibility));
    NotifyPropertyChanged(nameof(TabsVisibility));
    // ...
    return tab;
}
```

**Where to look.**
`RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.Consoles.cs`
`RedisSetupTool/src/RedisSetupTool.Core/ViewModels/ConsolesViewModel.cs` and
`src/RedisSetupTool.UI/Views/MainPage.xaml` (the tab strip, the body host and the
status strip's data template)

**Sharp edges.**
- A tab's own content presenter measures its child against an unbounded height,
  which flattens any star row inside it. Put the body in a real star cell of the
  page's grid and let the tab strip only choose which body is visible.
- Collapse the inactive bodies rather than removing them. A control removed from
  the tree stops receiving whatever is feeding it, and in this application that
  means losing shell output while the user is looking somewhere else.
- Anything fed to a control before its loaded event can be silently dropped, so
  open the session from that handler. It is also what makes an item created while
  the whole section is collapsed work, because the loaded event arrives when the
  section becomes visible.
- Removal is the page's job and it is easy to half-do: take the control out of
  both the strip and the host grid, unhook the handlers, and dispose the session.
  Key the page's bookkeeping on the item's view model rather than on the control,
  because that is what every one of those lookups starts from.
- Draw the line at "needs the control". Finding the shell and opening the session
  is the view model's work even though the page is what asks for it; what stays in
  the page is the grid size to open at, joining the session to the control, and
  writing a failure into it.

### Generate a form from a parameter list with one template and per-editor Visibility

**When you want this.** The fields on a form are not known at design time: they
come from whichever item the user selected, each with a kind, a label, a help
line, a default and its own validity rule. Unlike
[Generate an options panel from object properties by reflection](#generate-an-options-panel-from-object-properties-by-reflection),
which walks a settings object's members and builds controls in code, here the
fields are declared as plain data in a catalog and the editors are real XAML in
one data template - so the form is styled, themed and testable like any other
markup.

**The MVVM shape.** Selecting an item clears and refills a bound collection of
field view models, one per declared parameter. Each field view model carries the
value, the typed value its editor writes through, and a `Visibility` property per
editor kind, with exactly one ever visible. The form's own view model builds the
request out of the fields, publishes what is still wrong with it as a bound list,
and gates the command on that list being empty.

**Code.**

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/ViewModels/ParameterFieldViewModel.cs
/// <summary>
/// One generated field of the create form. The family has no <c>DataTemplateSelector</c>, so a
/// single template carries all six editors and each one has its own <see cref="Visibility"/>
/// property here; exactly one is ever visible.
/// </summary>
[Microsoft.UI.Xaml.Data.Bindable]
public class ParameterFieldViewModel : SimpleViewModel
{
    // ... the constructor copies the label, kind, bounds and choices off the parameter ...

    /// <summary>Whether the plain-text editor is showing.</summary>
    public Visibility TextVisibility => GetVisibility(Kind == TopologyParameterKind.Text);

    /// <summary>Whether the password editor is showing.</summary>
    public Visibility PasswordVisibility => GetVisibility(Kind == TopologyParameterKind.Password);

    /// <summary>Whether the number editor is showing.</summary>
    public Visibility IntegerVisibility => GetVisibility(Kind == TopologyParameterKind.Integer);

    /// <summary>Whether the choice editor is showing.</summary>
    public Visibility ChoiceVisibility => GetVisibility(Kind == TopologyParameterKind.Choice);

    /// <summary>Whether the switch is showing.</summary>
    public Visibility BooleanVisibility => GetVisibility(Kind == TopologyParameterKind.Boolean);

    /// <summary>Whether the multi-line editor is showing.</summary>
    public Visibility MultiLineVisibility =>
        GetVisibility(Kind == TopologyParameterKind.MultiLineText);

    /// <summary>
    /// The value of an integer field. <c>NumberBox</c> reports an emptied box as
    /// <see cref="double.NaN"/>, which is left in place rather than written through, so the
    /// request keeps the last good number and validation still sees it.
    /// </summary>
    public double NumberValue
    {
        get => _numberValue;
        set
        {
            //There is no double overload of SetProperty, so compare and notify by hand.
            if (double.IsNaN(value)) { return; }
            if (Math.Abs(_numberValue - value) < 0.0001d) { return; }
            _numberValue = value;
            NotifyPropertyChanged(nameof(NumberValue));
            Value = ((long)Math.Round(value)).ToString(CultureInfo.InvariantCulture);
        }
    }
}
```

```xml
<!-- From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.xaml -->
<!-- One generated field of the create form. The family has no DataTemplateSelector, so
     all six editors live here and exactly one of them is visible. -->
<ui:DataTemplate x:Key="ParameterFieldTemplate">
    <StackPanel Spacing="4" Margin="0,0,0,14">
        <TextBlock Text="{d:Binding Label}" FontSize="11.5" FontWeight="SemiBold"
                   Foreground="{StaticResource TextSecondaryBrush}" />
        <Grid ColumnSpacing="8">
            <!-- ... a star column for the editor and an auto column for the button ... -->

            <TextBox CornerRadius="8" Visibility="{d:Binding TextVisibility}"
                     Text="{d:Binding Value, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
            <PasswordBox CornerRadius="8" Visibility="{d:Binding PasswordVisibility}"
                         Password="{d:Binding Value, Mode=TwoWay}" />
            <NumberBox CornerRadius="8" Visibility="{d:Binding IntegerVisibility}"
                       Minimum="{d:Binding Minimum}" Maximum="{d:Binding Maximum}"
                       SpinButtonPlacementMode="Inline"
                       Value="{d:Binding NumberValue, Mode=TwoWay}" />
            <ComboBox HorizontalAlignment="Stretch" CornerRadius="8"
                      Visibility="{d:Binding ChoiceVisibility}"
                      ItemsSource="{d:Binding Choices}"
                      SelectedItem="{d:Binding Value, Mode=TwoWay}" />
            <ToggleSwitch Header="" Visibility="{d:Binding BooleanVisibility}"
                          IsOn="{d:Binding BoolValue, Mode=TwoWay}" />
            <!-- ... the multi-line editor, same shape ... -->

            <Button Grid.Column="1" Content="Generate" VerticalAlignment="Top"
                    Style="{StaticResource SmallButtonStyle}"
                    Visibility="{d:Binding GenerateVisibility}"
                    Command="{d:Binding GenerateCommand}" />
        </Grid>
        <TextBlock Text="{d:Binding HelpText}" FontSize="10.5" TextWrapping="Wrap"
                   Foreground="{StaticResource TextTertiaryBrush}"
                   Visibility="{d:Binding HelpVisibility}" />
    </StackPanel>
</ui:DataTemplate>
```

```xml
<!-- From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.xaml -->
<ItemsControl ItemsSource="{d:Binding Fields}"
              ItemTemplate="{StaticResource ParameterFieldTemplate}"
              Visibility="{d:Binding FieldsVisibility}" />
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/ViewModels/CreateInstanceViewModel.cs
private void Select(TopologyChoiceViewModel choice)
{
    if (choice is null || IsCreating) { return; }

    // ... mark the chosen row and copy the descriptor's captions into bound properties ...

    _isBuildingForm = true;
    Fields.Clear();
    foreach (var parameter in descriptor.Parameters)
    {
        Fields.Add(new ParameterFieldViewModel(parameter, Revalidate));
    }
    _isBuildingForm = false;
    NotifyPropertyChanged(nameof(FieldsVisibility));

    // ...
    Revalidate();
}

private void Revalidate()
{
    if (_isBuildingForm) { return; }

    var descriptor = SelectedTopology?.Descriptor;
    ValidationMessages.Clear();

    // ... the one rule the form owns: a resource name the daemon will accept ...

    foreach (var problem in _topologies.Validate(BuildRequest(descriptor)))
    {
        ValidationMessages.Add(problem);
    }

    CanCreate = ValidationMessages.Count == 0;
    NotifyPropertyChanged(nameof(ValidationVisibility));
}
```

**Where to look.**
`RedisSetupTool/src/RedisSetupTool.Core/ViewModels/ParameterFieldViewModel.cs`
`RedisSetupTool/src/RedisSetupTool.Core/ViewModels/CreateInstanceViewModel.cs` and
`src/RedisSetupTool.UI/Views/MainPage.xaml`,
`src/libs/RedisSetupTool.DockerManagement/Topologies/TopologyCatalog.cs` (where the
parameters are declared)

**Sharp edges.**
- Every editor in the template is really there, collapsed. Get one of the
  visibility predicates wrong and the user sees two editors for one field, which
  looks like a layout bug rather than a logic one.
- Seed the field's value through the backing field, not the property, and raise a
  flag while the collection is being refilled - otherwise every field added
  revalidates a form that is still being built.
- A number editor reports an emptied box as not-a-number. Refuse to write that
  through, or the request loses a value the user never meant to clear.
- Publish the validation messages as a bound list and derive the command's
  enablement from that list being empty. Two separate rules - one in the command
  predicate and one in the message the user reads - drift apart on the first
  change. The field's public value stays a string either way, so the request the
  builder receives is the same shape whichever editor produced it.

### Show mask and copy a secret in a one-line row

**When you want this.** A details panel lists connection facts - an address, a
user name, a password, a whole command line - and every one of them should be one
click away from the clipboard, with the secret ones hidden until asked for. This
is the display side of a secret, not the entry side:
[Take a secret token in a PasswordBox and keep it out of storage](#take-a-secret-token-in-a-passwordbox-and-keep-it-out-of-storage)
covers taking one from the user, and
[Copy text to the clipboard from a command through a bridge interface](BLUEPRINTS-PlatformServices.md#copy-text-to-the-clipboard-from-a-command-through-a-bridge-interface)
covers reaching the clipboard at all; this recipe is the row itself.

**The MVVM shape.** A small view model per row: the label, the value as it should
be shown, a flag saying whether it is a secret, the reveal caption and a
visibility for the transient confirmation. Two commands, copy and toggle. Nothing
platform-specific: the row is handed a copy delegate by whoever built it, and that
delegate goes through the shell's clipboard bridge, so a head with no clipboard
simply copies nothing. One data template renders all of them.

**Code.**

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/ViewModels/EndpointRowViewModel.cs
private EndpointRowViewModel(string label, string secret, Action<string> copy, bool isSecret)
{
    Label = label;
    _secret = secret ?? string.Empty;
    _copy = copy;
    IsSecret = isSecret;
    Value = new string('•', Math.Min(12, Math.Max(6, _secret.Length)));
}

/// <summary>
/// Creates a masked row whose value is hidden behind bullets until it is revealed. The
/// password is also readable through <c>docker inspect</c>, so this is convenience rather
/// than secrecy — the card says so.
/// </summary>
public static EndpointRowViewModel Secret(string label, string secret, Action<string> copy) =>
    new(label, secret, copy, true);

// ...

/// <summary>The value shown on the row: bullets while a secret is hidden.</summary>
public string Value { get; private set; }

/// <summary>Whether the reveal button is offered.</summary>
public Visibility RevealVisibility => GetVisibility(IsSecret);

/// <summary>The reveal button's caption.</summary>
public string RevealText => _isRevealed ? "hide" : "show";

/// <summary>Whether the transient "Copied" confirmation is showing.</summary>
public Visibility CopiedVisibility => GetVisibility(_isCopied);

// ...

private async Task CopyAsync()
{
    var text = IsSecret ? _secret : Value;
    if (string.IsNullOrEmpty(text)) { return; }

    _copy?.Invoke(text);
    _isCopied = true;
    NotifyPropertyChanged(nameof(CopiedVisibility));

    //The confirmation is a nicety, not state: it fades on its own after a moment.
    await Task.Delay(1500).ConfigureAwait(true);
    _isCopied = false;
    NotifyPropertyChanged(nameof(CopiedVisibility));
}

private void ToggleReveal()
{
    if (!IsSecret) { return; }

    _isRevealed = !_isRevealed;
    Value = _isRevealed
        ? _secret
        : new string('•', Math.Min(12, Math.Max(6, _secret.Length)));
    NotifyPropertyChanged(nameof(Value));
    NotifyPropertyChanged(nameof(RevealText));
}
```

```xml
<!-- From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.xaml -->
<!-- One copyable connection row of an instance card -->
<ui:DataTemplate x:Key="EndpointRowTemplate">
    <Grid ColumnSpacing="8" Margin="0,2">
        <!-- ... a fixed label column, a star value column and three auto columns ... -->
        <TextBlock Text="{d:Binding Label}" FontSize="11"
                   Foreground="{StaticResource TextTertiaryBrush}" VerticalAlignment="Center" />
        <TextBlock Grid.Column="1" Text="{d:Binding Value}"
                   FontFamily="{StaticResource RobotoMonoFont}" FontSize="11.5"
                   TextTrimming="CharacterEllipsis" TextWrapping="NoWrap"
                   Foreground="{StaticResource TextPrimaryBrush}" VerticalAlignment="Center" />
        <TextBlock Grid.Column="2" Text="Copied" FontSize="10.5" Margin="0,0,4,0"
                   Foreground="{StaticResource AccentBrush}" VerticalAlignment="Center"
                   Visibility="{d:Binding CopiedVisibility}" />
        <Button Grid.Column="3" Command="{d:Binding ToggleRevealCommand}"
                Content="{d:Binding RevealText}" FontSize="10.5"
                Height="24" MinWidth="0" MinHeight="0" Padding="7,0" CornerRadius="5"
                BorderThickness="0" Background="{StaticResource CardWellBrush}"
                Visibility="{d:Binding RevealVisibility}" />
        <Button Grid.Column="4" Command="{d:Binding CopyCommand}"
                Width="26" Height="24" MinWidth="0" MinHeight="0" Padding="0"
                CornerRadius="5" Background="Transparent" BorderThickness="0"
                ToolTipService.ToolTip="Copy to clipboard">
            <FontIcon Glyph="&#xE8C8;" FontSize="12" />
        </Button>
    </Grid>
</ui:DataTemplate>
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/ViewModels/InstanceCardViewModel.cs
private void RebuildConnectionRows(TopologyInstance instance)
{
    ConnectionRows.Clear();
    Notes.Clear();

    var connection = instance.Connection;
    // ...

    void Copy(string text) => _shell?.CopyToClipboard(text);

    if (!string.IsNullOrEmpty(connection.ServiceName))
    {
        ConnectionRows.Add(new EndpointRowViewModel("service", connection.ServiceName, Copy));
    }

    // ... the endpoints, and the sentinels where a topology has them ...

    if (!string.IsNullOrEmpty(connection.Password))
    {
        ConnectionRows.Add(EndpointRowViewModel.Secret("password", connection.Password, Copy));
    }
    foreach (var user in connection.AdditionalUsers)
    {
        ConnectionRows.Add(EndpointRowViewModel.Secret("user " + user.Username,
            user.Password, Copy));
    }
    if (!string.IsNullOrEmpty(connection.ConnectionString))
    {
        ConnectionRows.Add(new EndpointRowViewModel("string",
            connection.ConnectionString, Copy));
    }
    // ...
}
```

The card says plainly, next to the rows, that the masked value is readable
elsewhere anyway. Where masking is convenience rather than secrecy, saying so
beside it is part of the design.

**Where to look.**
`RedisSetupTool/src/RedisSetupTool.Core/ViewModels/EndpointRowViewModel.cs`
`RedisSetupTool/src/RedisSetupTool.Core/ViewModels/InstanceCardViewModel.cs` and
`src/RedisSetupTool.UI/Views/MainPage.xaml`,
`src/RedisSetupTool.Core/Bridges/ICopyToClipboard.cs`

**Sharp edges.**
- The shown value and the real value are different fields. Copy the real one and
  show the masked one, or revealing becomes the only way to copy. Two
  constructors, one public and one private behind a named factory, keep a plain
  row and a masked row from being confused at the call site - a masked row built
  by the plain constructor shows the secret.
- Clamp the bullet count instead of matching the secret's length, so the mask does
  not publish how long the value is.
- The confirmation is transient display state, not something to persist or to
  bind two-way. Raise its change notification by hand around the wait, and let the
  wait resume on the UI thread.
- The copy delegate can be null on a head with no clipboard. Invoke it
  conditionally and let the row do nothing rather than reporting a failure the
  user cannot act on.

### Wire a control before the DataContext arrives by passing a getter

**When you want this.** A page wires a control to something the view model owns -
a drawing session, a document, a scene - in its constructor, but the
`DataContext` is set afterwards, and it can be replaced or disposed while the
page is still alive.

**The MVVM shape.** Do not hand the wiring the object; hand it a function that
returns the object. Every event then asks for the current one, so the order the
page and its data context are built in stops mattering, nothing is captured that
has to be released, and a helper shared by several heads needs no knowledge of
which view-model type any of them uses.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/Drawing/DrawingCanvasBinder.cs
/// <summary>
/// Subscribes the canvas's paint and pointer (or mouse) events to the drawing session the
/// getter returns. The getter is called on every event rather than captured once, so the
/// canvas can be wired before the page's <c>DataContext</c> has arrived and keeps working
/// after the view model is disposed.
/// </summary>
public static void BindToSession(this DrawingCanvas canvas, Func<DrawingSession> sessionGetter)
{
    if (canvas == null || sessionGetter == null) { return; }

    canvas.PaintSurface += (_, e) => sessionGetter()?.Render(e.Surface, e.Info);

// ...
    canvas.PointerMoved += (_, e) =>
    {
        var session = sessionGetter();
        if (session is not { IsPointerActive: true }) { return; }
// ...
    };
}
```

The page's constructor then wires the canvas without having to know whether the
data context has arrived - here `InitializeComponent` may be the thing that sets
it - because the getter is not called until an event arrives:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs
//I tend to like to declare/define private methods above the constructor, in C# classes
private MainViewModel ViewModel => DataContext as MainViewModel;

public MainPage()
{
    //Doing this before InitializeComponent() - in case InitializeComponent()
    //  is the thing that sets the data context.
    DataContextChanged += (_, _) =>
    {
        // ...
    };

    InitializeComponent();

    //Paint, press, move, release and capture-lost all go straight to the drawing session
    DrawCanvas.BindToSession(() => ViewModel?.Session);
}
```

**Where to look.**
`PainDiagram/Shared/Drawing/DrawingCanvasBinder.cs`
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs`

**Also shown by.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs` and
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs` (the same idea without
a delegate: each handler starts from a `ViewModel` property, or from a
`DataContext as <interface>` cast, so it reads the current data context every
time rather than one captured in the constructor)

**Sharp edges.**
- A captured reference is a lifetime decision. The getter form makes the wiring
  outlive a replaced or disposed view model without the page having to unhook
  anything.
- Null-check inside the handler, not once at wiring time. `sessionGetter()` can
  legitimately return null before the first document exists.
- The getter is also what keeps a shared helper free of the application's view
  model type: it takes a function returning a model type the helper already knows.
- This is for wiring a control to a model. It is not a substitute for a bridge
  interface where the traffic goes the other way, from the view model to the page.

### Subscribe to a view model once and unsubscribe when the page unloads

**When you want this.** The page has to watch the view model for something a
binding cannot express - a version counter, a "the view moved" signal - and the
data context can arrive, change, or be missing when the page is built.

**The MVVM shape.** Give the page one private field holding the view model it is
wired to, and two methods: one that wires if it is not already wired to that
instance, one that unwires. Call the first from both `DataContextChanged` and
`Loaded`, because either can come first; call the second from `Unloaded`. One
handler, one subscription, and nothing left behind when the page goes away.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs
private MainViewModel _wiredViewModel;

public MainPage()
{
    DataContextChanged += (_, _) =>
    {
        // ...
        WireViewModel();
    };

    //Anything the view model opens can fail, and an error dialog needs the XamlRoot that only
    //  a loaded page has, so the startup documents are opened from here
    Loaded += (_, _) =>
    {
        WireViewModel();
        ViewModel?.OnPageReady();
    };

    Unloaded += (_, _) => UnwireViewModel();

    this.InitializeComponent(); //Leave this line last
    // ...
}
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs
//One property to watch: the view model folds everything that moves the view - a zoom, a pan, a
//  page change, a newly rendered image - into ViewVersion
private void WireViewModel()
{
    var viewModel = ViewModel;
    if (ReferenceEquals(viewModel, _wiredViewModel)) { return; }

    UnwireViewModel();
    if (viewModel == null) { return; }

    _wiredViewModel = viewModel;
    _wiredViewModel.PropertyChanged += OnViewModelPropertyChanged;
}

private void UnwireViewModel()
{
    if (_wiredViewModel == null) { return; }

    _wiredViewModel.PropertyChanged -= OnViewModelPropertyChanged;
    _wiredViewModel = null;
}

private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs args)
{
    if (args.PropertyName == nameof(MainViewModel.ViewVersion)) { ApplyViews(); }
}
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs`
(`WireViewModel()` called from both `DataContextChanged` and `Loaded` and
returning immediately when the data context is the one already wired, so
whichever arrives first wins and the other is a no-op; the single subscription
exists only for the drawn rail chrome, which cannot be bound)

**Related.**
[Signal a non property model change to the view with a version counter](BLUEPRINTS-MVVM.md#signal-a-non-property-model-change-to-the-view-with-a-version-counter)
is what this subscription is watching for, and why one property is enough.

**Sharp edges.**
- Wiring from `DataContextChanged` alone is not enough, and neither is wiring from
  `Loaded` alone: which arrives first depends on how the page got its data
  context. Call the same idempotent method from both.
- Compare against the instance you are already wired to before subscribing. A data
  context that is set twice with the same object would otherwise leave two
  subscriptions and run every handler twice.
- Unwire on `Unloaded`, not only in a disposer. The page can outlive its view
  model or be taken out of the tree and put back.
- A lambda subscription cannot be removed. Use a named method so the minus-equals
  can find it.

### Decide portrait or landscape on the view model and apply it from the page

**When you want this.** A two-pane view should sit side by side on a wide window
and stack on a tall one, and more than the panel's axis depends on which way
round it is: a pane's width, its flex basis and its margin all change with it.

**The MVVM shape.** The page measures - it is the only thing that can - and
reports the new size in one call. The view model decides what that size means and
publishes the answers as ordinary bindable properties, one per value the page has
to apply. The orientation itself is a bindable property too, so anything else that
cares about it (a visibility, a caption) follows without the page being involved.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
//The Model View's info pane is a fixed column beside the viewer while the window is
//  landscape, and half the height above it when the window is portrait.
private const double InfoPaneLandscapeWidth = 420d;
private const float InfoPaneStackedHeightBasis = 0.5f;

/// <summary>
/// Whether the window is taller than it is wide, so the Model View stacks its info pane
/// above the 3D viewer instead of placing the two side by side.
/// </summary>
[AffectsProperties(nameof(ModelInfoPaneWidth), nameof(ModelInfoPaneMargin))]
public bool IsModelViewStacked
{
    get;
    private set => SetProperty(ref field, value);
}

/// <summary>
/// The Model View info pane's width: an explicit column while the panes sit side by side
/// (so the text inside measures - and wraps - against it), and automatic when they stack,
/// where the pane is sized by <see cref="ModelInfoPaneStackedHeightBasis"/> instead.
/// </summary>
public double ModelInfoPaneWidth => IsModelViewStacked ? double.NaN : InfoPaneLandscapeWidth;

/// <summary>The share of the Model View's height the info pane takes when the panes stack.</summary>
public float ModelInfoPaneStackedHeightBasis => InfoPaneStackedHeightBasis;

/// <summary>The gap the info pane leaves for the 3D viewer: below it when stacked, beside it otherwise.</summary>
public Thickness ModelInfoPaneMargin => IsModelViewStacked
    ? new Thickness(0, 0, 0, 20)
    : new Thickness(0, 0, 20, 0);

/// <summary>
/// Tells the view model the window's new size, so it can decide which way round the
/// Model View's panes belong. The page reads the pane properties back and applies them
/// to the layout panel.
/// </summary>
/// <param name="width">The window's new width.</param>
/// <param name="height">The window's new height.</param>
public void NotifyWindowSizeChanged(double width, double height) =>
    IsModelViewStacked = width < height;
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
SizeChanged += (_, args) =>
{
    var viewModel = ViewModel;
    if (viewModel == null) { return; }

    viewModel.NotifyWindowSizeChanged(args.NewSize.Width, args.NewSize.Height);

    var stacked = viewModel.IsModelViewStacked;
    // ... assign the axis, the width, the basis and the margin to the panel ...
};
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`

**Related.**
[Wrap and reflow a layout with the FlexPanel add-in](#wrap-and-reflow-a-layout-with-the-flexpanel-add-in)
is the panel these values are applied to, and the cases that need no code at all.

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs` keeps the
same decision in its size-changed handler instead, which is defensible where
nothing but the panel cares - and is the shape to move first when something else
starts to.

**Sharp edges.**
- A property whose value follows the orientation must be notified when the
  orientation changes. An attribute that names the dependent properties is the
  cheap way to get that right; a hand-written setter has to raise them all.
- Report the size, do not report "portrait". The threshold is a decision, and
  decisions belong with the state they affect - which is also what makes it
  testable without a window.
- Some of these values are platform types (a thickness, a flex basis number). That
  is not a layering violation in a view model that already references the UI
  types the bindings need, but keep the arithmetic behind named constants so the
  numbers are readable.
- Size-changed handlers fire during layout. Keep the work in them to assignments,
  and never start anything that can recurse into a new layout pass.

### Build a row of buttons from a palette with one item template

**When you want this.** A panel holds one button per entry in a small fixed set -
a color palette, a tool list, a set of presets - and hand-writing them means the
set is written down twice: once in the model and once in the markup.

**The MVVM shape.** The view model turns the palette into a read-only list of item
view models, one per entry, each carrying what the button shows and the owner's
own command with its own parameter. The markup becomes one `ItemsControl` and one
`DataTemplate` that binds only to an item. Add a color to the palette and a button
appears; nothing in the page changes.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/HighlighterColorViewModel.cs
/// <summary>
/// One highlighter button in the Paint Mode side panel: a palette entry's name, its ink color
/// and the caption color that reads on it, plus the command the button invokes. Each item
/// carries the owning view model's own <c>SelectColorCommand</c> and its name as the command
/// parameter, so the template binds to its item only, the palette decides how many buttons
/// there are, and one <c>[AffectsCommands]</c> refresh still reaches every button at once.
/// </summary>
#if HAS_CODEBRIX
[Microsoft.UI.Xaml.Data.Bindable]
#endif
public sealed class HighlighterColorViewModel
{
    // ... Name, Color, TextColor and SelectColorCommand, all set in the constructor ...
}
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
//One button item per palette entry, each carrying this view model's own select command
private IReadOnlyList<HighlighterColorViewModel> BuildHighlighterColors()
{
    var colors = new List<HighlighterColorViewModel>(HighlighterPalette.Colors.Count);
    foreach (var color in HighlighterPalette.Colors)
    {
        colors.Add(new HighlighterColorViewModel(color, SelectColorCommand));
    }

    return colors;
}
```

```xml
<!-- From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml -->
<!-- One highlighter button: the palette entry supplies the caption, the ink color
     and the caption color, and hands back the view model's own select command -->
<ui:DataTemplate x:Key="HighlighterColorTemplate">
    <Button Content="{d:Binding Name}" HorizontalAlignment="Stretch" Margin="0,0,0,8"
            Background="{d:Binding Color, Converter={StaticResource ColorToBrush}}"
            Foreground="{d:Binding TextColor, Converter={StaticResource ColorToBrush}}"
            Command="{d:Binding SelectColorCommand}"
            CommandParameter="{d:Binding Name}" />
</ui:DataTemplate>

<!-- ... -->

<!-- Highlighter colors: one button per palette entry, so the palette is the
     only place the colors are written down -->
<ItemsControl ItemsSource="{d:Binding HighlighterColors}"
              ItemTemplate="{StaticResource HighlighterColorTemplate}" />
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/HighlighterColorViewModel.cs`
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`WebcamPainter/src/WebcamPainter.Core/Converters/ColorToBrushConverter.cs`
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml`

**Sharp edges.**
- The item carries the owner's command, not a command of its own, with the item's
  identity as the parameter. One `CanExecute` refresh on the owner then reaches
  every button at once.
- The template binds to its item and nothing else. Reaching back to the page's
  data context from inside a template is what makes generated items stop working
  when they are recycled.
- An item view model that is built once and never changes needs no change
  notification, but it still has to be visible to the binding engine - the family
  marks such a type bindable behind the platform's own compilation symbol.
- A color is not a brush. Convert in a value converter rather than putting a
  platform brush type on the item, and the palette stays a plain list of numbers.

### Route a container's chrome button to the item's own command

**When you want this.** A container control draws chrome you cannot bind - a tab
strip's close button, a header's menu - and it reports the gesture as an event on
the container rather than as a command on the item.

**The MVVM shape.** The event handler's whole job is to find the item the chrome
belongs to and execute that item's command. It must not close, remove or dispose
anything itself, because the item also carries a real bound button that does the
same thing, and two code paths for one user action drift apart.

**Code.**

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.Consoles.cs
private void ConsoleTabs_TabCloseRequested(TabView sender,
    TabViewTabCloseRequestedEventArgs args)
{
    foreach (var pair in _consoleHosts)
    {
        if (ReferenceEquals(pair.Value.Tab, args.Tab))
        {
            //The strip's close button takes the same command the tab's own Close button
            //  does, so there is one way to close a console rather than two.
            pair.Key.CloseCommand.Execute(null);
            return;
        }
    }
}
```

The command it reaches is the same one the status strip binds to in markup:

```xml
<!-- From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.xaml -->
<Button Grid.Column="5" Content="Close"
        Style="{StaticResource SmallButtonStyle}"
        Command="{d:Binding CloseCommand}" />
```

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/RedisSetupTool.Core/ViewModels/ConsolesViewModel.cs
/// <summary>Closes this console.</summary>
public SimpleCommand CloseCommand => field ??= new SimpleCommand(() => _close?.Invoke(this));
```

**Where to look.**
`RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.Consoles.cs`
`RedisSetupTool/src/RedisSetupTool.Core/ViewModels/ConsolesViewModel.cs`
`RedisSetupTool/src/RedisSetupTool.UI/Views/MainPage.xaml`

**Related.**
[Host a control with no dependency properties by mirroring a collection from code-behind](#host-a-control-with-no-dependency-properties-by-mirroring-a-collection-from-code-behind)
is why this page has a dictionary from item to chrome to look the item up in.

**Sharp edges.**
- Look the item up from the chrome, not the other way round. The event gives you
  the container's own object, so the page needs a map from that object back to the
  item it was built for.
- Do the work in the command, never in the handler. The tear-down that follows -
  removing the item, disposing what it owns - belongs to whoever owns the
  collection, and the chrome handler should not know about any of it.
- The container may act on the gesture itself. A tab strip that removes its own
  tab on a close request would fight the collection mirror; take the item out in
  one place only.

### Report a control's load failure with an event and a log line

**When you want this.** A custom control loads something asynchronously - an
image, a document, a font - and it cannot throw, because the load was started
from a dependency-property callback that nothing is awaiting. A silent empty
control is the usual result, and it is the worst of the options.

**The MVVM shape.** The control reports the failure twice: once to the
application's log through the ambient logger factory, which needs no
collaborator and is always there, and once as an event carrying what was asked
for and what went wrong, which an application can handle to say something in its
own voice. Neither reaches the view model directly, because the control is a view
type; a page that subscribes decides what, if anything, the user is told.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs
/// <summary>
/// Raised when the image named by <see cref="UriSource"/> could not be loaded - a misspelled
/// resource name, an assembly that is not loaded, or an image the decoder rejected. Without a
/// handler the control is left empty, so anything that cares about a missing image should
/// subscribe; the failure is written to the application log either way.
/// </summary>
public event EventHandler<EmbeddedImageFailedEventArgs> LoadFailed;

// ...

catch (Exception ex)
{
    // ...
    LogExtensionPoint.AmbientLoggerFactory
        .CreateLogger<EmbeddedImage>()
        .LogError(ex, "EmbeddedImage failed to load from '{UriSource}'.", uri);

    image.LoadFailed?.Invoke(image, new EmbeddedImageFailedEventArgs(uri, ex));
}
```

The arguments carry both halves of the answer - what was asked for and why it
failed - so a handler never has to guess which of several images went missing:

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImageFailedEventArgs.cs
/// <summary>
/// Describes an image that an <see cref="EmbeddedImage"/> could not load. A page (or a control that
/// composes an <see cref="EmbeddedImage"/>) handles <see cref="EmbeddedImage.LoadFailed"/> to show
/// the failure instead of leaving an empty image behind.
/// </summary>
public sealed class EmbeddedImageFailedEventArgs : EventArgs
{
    // ...

    /// <summary>The URI the control was asked to load.</summary>
    public string UriSource { get; }

    /// <summary>The exception that stopped the load.</summary>
    public Exception Error { get; }
}
```

The sample itself subscribes to neither: its icons are embedded in the same
assembly and cannot go missing between builds. The log line is what it relies on,
and the event is what an application with a fallback icon would use.

**Where to look.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImageFailedEventArgs.cs`

**Related.**
[Load an SVG or bitmap from an embedded resource with a custom URI scheme](#load-an-svg-or-bitmap-from-an-embedded-resource-with-a-custom-uri-scheme)
is the control this failure path belongs to.

**Sharp edges.**
- Catch everything on that path. An exception from an asynchronous
  property-changed callback has nowhere to go and can take the process with it.
- Log through the ambient logger factory rather than a logger the control is
  given. A control cannot be constructed with dependencies by the markup that
  creates it, and a null logger is the harmless default when nothing configured
  one.
- Do not raise the event on a background thread and then touch the tree in a
  handler. This one is raised from the load path, which is already on the UI
  thread; if yours is not, marshal first.
- Debug output is not a report. It is invisible in a published build, which is
  exactly where a missing resource shows up.

### Drag a card across the scene and snap it to the nearest station

**When you want this.** Your page draws a scene rather than a form, and the user has
to pick one of its objects up, move it about and let it go somewhere meaningful.
You want the drop to land on the nearest target if it is close enough, and to be a
real "dropped on nothing" answer if it is not.

**The MVVM shape.** The gesture is the page's, and only the page's: pointer capture,
the grab offset inside the object, the Z order, the drop shadow, the highlight on
the target under the pointer. None of that is state anybody else can act on. What
the page does not decide is what the drop *means* - it computes which station the
object landed on and calls the view model, which owns the table and answers back
through its bridge delegates. The page's last line of the gesture is a view model
call and nothing else.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
private void OnCardPressed(object sender, PointerRoutedEventArgs e)
{
    var item = ItemOf(sender);
    if (item is null) { return; }

    var pp = e.GetCurrentPoint(ContentCanvas);
    if (!pp.Properties.IsLeftButtonPressed) { return; }
    e.Handled = true;
    // ...
    CloseDetail();

    _dragItem = item;
    _dragMoved = false;
    _grab = new Point(pp.Position.X - Canvas.GetLeft(item.View), pp.Position.Y - Canvas.GetTop(item.View));
    Canvas.SetZIndex(item.View, 900);
    _dragging = item.View.CapturePointer(e.Pointer);
}
```

The grab offset is taken once, at the press, against the same canvas every later
coordinate is read in, so the object does not jump under the pointer. Capturing
returns a bool, and that bool - not the press itself - is what says a drag is
running.

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
private int NearestStation(double x, double y)
{
    var best = -1;
    var bestDistance = double.MaxValue;
    var snap = _cardH * 0.8;
    for (int i = 0; i < 9; i++)
    {
        var c = StationCentre(i);
        var d = Math.Sqrt((c.X - x) * (c.X - x) + (c.Y - y) * (c.Y - y));
        if (d < bestDistance) { bestDistance = d; best = i; }
    }
    return bestDistance <= snap ? best : -1;
}

private void OnCardReleased(object sender, PointerRoutedEventArgs e)
{
    var item = ItemOf(sender);
    e.Handled = true;
    try { ((UIElement)sender).ReleasePointerCapture(e.Pointer); }
    catch (Exception) { /* capture may already be gone */ }

    if (item is null || !_dragging) { EndDrag(); return; }

    if (!_dragMoved)
    {
        EndDrag();
        ShowDetail(item);
        return;
    }

    var x = Canvas.GetLeft(item.View);
    var y = Canvas.GetTop(item.View);
    var target = NearestStation(x + _cardW / 2, y + _cardH / 2);
    EndDrag();

    // the view model decides what a drop on that station means; this page only reports it
    if (target >= 0)
    {
        ViewModel?.PlaceOnStation(item.Model, target);
    }
    else
    {
        ViewModel?.ReturnToTray(item.Model, announce: false);
    }
}
```

`NearestStation` is the whole snap rule: the closest of the nine station centers,
accepted only when it is inside an accept radius scaled to the card, and `-1`
otherwise. Returning `-1` rather than the nearest station regardless is what makes
"the user dropped this on the table, not on a petal" something the page can report
honestly.

A drag can also end without a release, and a move that nothing claimed can be
taken for a window drag:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
private void OnCardCaptureLost(object sender, PointerRoutedEventArgs e)
{
    if (_dragging && _dragMoved && _dragItem is not null)
    {
        var item = _dragItem;
        EndDrag();
        if (item.Model.InTray) { LayoutTray(); } else { RepositionAllCards(); }
    }
    else
    {
        EndDrag();
    }
}
// ...
/// <summary>
/// An unhandled pointer move bubbles out to the window manager, which on some heads then
/// drags the window instead of leaving the scene alone. Card drags mark their own moves
/// handled; this catches every other move over the app's own chrome.
/// </summary>
private void OnRootPointerMoved(object sender, PointerRoutedEventArgs e) => e.Handled = true;
```

**Where to look.**
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs`
(`OnCardPressed`, `OnCardMoved`, `BeginDragVisuals`, `NearestStation`,
`OnCardReleased`, `OnCardCaptureLost`, `EndDrag`)
`InannaRosette/src/InannaRosette.Core/ViewModels/MainViewModel.cs`
(`PlaceOnStation`, `ReturnToTray`) and
`src/InannaRosette.Core/Services/IReadingTableBridge.cs`

**Sharp edges.**
- `ReleasePointerCapture` is wrapped in a `try`, because by the time the release
  arrives the capture may already have been taken away, and the exception would
  abandon the rest of the handler - including the view model call.
- Without `OnCardCaptureLost` a window that loses focus mid-drag leaves the object
  stranded wherever the pointer abandoned it. Ending the drag is not enough on its
  own: whichever region the object belongs to has to be laid out again, because the
  object's position is now a drag position and nothing else will correct it.
- `OnRootPointerMoved` sets `e.Handled = true` on every pointer move over the
  application's own chrome. An unhandled move bubbles out to the window manager,
  which on some heads reads it as a window drag. Card moves mark themselves handled
  already; this catches the rest.
- `EndDrag` runs before the view model call, not after, so the visual state is
  settled by the time the bridge delegate comes back asking the page to re-place
  the card.
- The page never asks the view model where a card is. It reports the drop and is
  told, through the bridge, what to draw.

### Tell a press from a drag and hand roll a double click

**When you want this.** One object on your scene has to answer three different
gestures from the same pointer button: a click that opens something, a double click
that changes the object, and a drag that moves it. Nothing in the markup can tell
them apart, because which one is happening is not known until after the press.

**The MVVM shape.** All three are resolved in the page, because all three are
questions about pixels and clocks. Each one ends somewhere different: the click
opens a panel the page builds, the double click calls the view model, and the drag
ends in a view model call too. The view model is never told about presses, moves or
milliseconds.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
// double click toggles the orientation
var now = DateTime.UtcNow;
if (ReferenceEquals(_lastClickItem, item) && (now - _lastClickAt).TotalMilliseconds < 420)
{
    _lastClickItem = null;
    _lastClickAt = DateTime.MinValue;
    CloseDetail();
    ViewModel?.ToggleReversed(item.Model);
    return;
}
_lastClickItem = item;
_lastClickAt = now;
```

The double click is decided first, at the top of the press handler, before any drag
bookkeeping happens. The state it needs is two fields - which object was last
pressed and when - and both are cleared on a hit so that three presses in quick
succession are one double click and one fresh press, not two overlapping ones.

The press becomes a drag only once the pointer has moved far enough:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
private void OnCardMoved(object sender, PointerRoutedEventArgs e)
{
    if (!_dragging || _dragItem is null) { return; }
    e.Handled = true;

    var p = e.GetCurrentPoint(ContentCanvas).Position;
    var x = p.X - _grab.X;
    var y = p.Y - _grab.Y;

    if (!_dragMoved)
    {
        var dx = x - Canvas.GetLeft(_dragItem.View);
        var dy = y - Canvas.GetTop(_dragItem.View);
        if (Math.Abs(dx) + Math.Abs(dy) < 4) { return; }
        BeginDragVisuals(_dragItem);
    }
    // ...
}
```

The threshold is a Manhattan distance against the object's current position rather
than against the press point, which is what makes it survive a single coarse move
event. Until it is crossed, `_dragMoved` stays false and the release takes the
other road:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
private void OnCardReleased(object sender, PointerRoutedEventArgs e)
{
    var item = ItemOf(sender);
    e.Handled = true;
    try { ((UIElement)sender).ReleasePointerCapture(e.Pointer); }
    catch (Exception) { /* capture may already be gone */ }

    if (item is null || !_dragging) { EndDrag(); return; }

    if (!_dragMoved)
    {
        EndDrag();
        ShowDetail(item);
        return;
    }
    // ...
```

And the double click ends in a complete view model transaction:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Turns a card the other way up, which the page calls on a double click. A reversed card
/// reads its reversed meaning, so the interpretation goes stale.
/// </summary>
/// <param name="card">The card to turn.</param>
public void ToggleReversed(ReadingCard? card)
{
    if (card == null || !_cards.Contains(card)) { return; }

    card.IsReversed = !card.IsReversed;
    CardFlipped?.Invoke(card);
    InvalidateInterpretation(hidePanel: false);
    SetStatus($"{card.Card.Name} is now {(card.IsReversed ? "reversed" : "upright")}.");
    RefreshCounts(refreshGuidance: false);
}
```

**Where to look.**
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs`
(`OnCardPressed`, `OnCardMoved`, `OnCardReleased`, `ShowDetail`)
`InannaRosette/src/InannaRosette.Core/ViewModels/MainViewModel.cs`
(`ToggleReversed`)

**Sharp edges.**
- The double-click interval is a constant in the page, timed off `DateTime.UtcNow`.
  It does not follow the desktop's own double-click setting, so a user who has
  changed that setting gets this application's interval instead.
- `DateTime.UtcNow`, not `DateTime.Now`: a local-time reading can jump backwards
  at a daylight-saving transition and make the interval test nonsense.
- The press handler returns early unless the left button is down, so a right-click
  or a stylus barrel press never starts a drag and never counts towards a double
  click.
- The threshold has to be crossed before any visual lift happens, or a click that
  wobbles by a pixel produces a shadow and a scale that then have to be undone.
- The first press of a double click still opens nothing, because the release that
  follows it sees `_dragMoved` false and shows the detail panel; the second press
  closes that panel before calling the view model.

### Rebuild the whole scene geometry in one Relayout method

**When you want this.** Your page is a drawn scene on a `Canvas`, not a panel of
controls, so nothing reflows itself. Every size in it - the panels, the objects,
the radius of an arrangement, the position of each label - is arithmetic somebody
has to redo whenever the window changes size. This differs from
[Let the page do the layout arithmetic only it can do](BLUEPRINTS-ViewsAndControls.md#let-the-page-do-the-layout-arithmetic-only-it-can-do),
where the arithmetic is split with the view model because the view model owns half
the inputs: here every input is a pixel and the whole calculation stays in the page.

**The MVVM shape.** One private method owns all of it, and two events call that one
method: `SizeChanged` on the canvas and the page's `Loaded`. Nothing else
recalculates geometry, and the view model contributes exactly one thing to it - a
bound flag saying which panel is on show, so the method can skip drawing chrome
nobody can see.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
private void OnContentSizeChanged(object sender, SizeChangedEventArgs e) => Relayout();

private void Relayout()
{
    if (_viewModelDisposed) { return; }

    var w = ContentCanvas.ActualWidth;
    var h = ContentCanvas.ActualHeight;
    if (w < 200 || h < 200) { return; }

    //Everything below is about to be recomputed, and a blessing in flight was drawn for the
    //  geometry that is going away; it ends here rather than finishing somewhere odd
    EndCelebration();

    _altarX = Margin_;
    _altarY = Gap;
    _altarH = h - Gap * 2;
    _railX = w - Margin_ - RailWidth;
    _altarW = Math.Max(320, _railX - Gap - _altarX);
    _railY = Gap;
    _railH = _altarH;

    Place(AltarBorder, _altarX, _altarY, _altarW, _altarH);
    Place(RailBorder, _railX, _railY, RailWidth, _railH);
    Place(InterpretationBorder, _railX, _railY, RailWidth, _railH);
    // the ScrollViewer on this head measures its child with an unbounded width, so the
    // text column is pinned explicitly or long paragraphs run off the panel edge
    InterpStack.Width = RailWidth - 50;

    // card size: the rosette must fit in the altar with room for the station labels
    var hFromHeight = (_altarH / 2 - 28) / 2.05;
    var hFromWidth = (_altarW / 2 - 64) / 1.8625;
    _cardH = Math.Clamp(Math.Min(hFromHeight, hFromWidth), 88, 176);
    _cardW = Math.Round(_cardH * 0.625);
    _cardH = Math.Round(_cardH);
    _radius = Math.Round(_cardH * 1.55);

    _centreX = _altarX + _altarW / 2;
    _centreY = _altarY + _altarH / 2;

    BuildOrnament();
    BuildSlots();

    // ...
    RepositionAllCards();
}
```

The order inside it is the point. Panels first, because the object size is derived
from what the panels left; then the object size, clamped so the scene is neither
unreadable nor absurd; then the radius as a multiple of that size; then everything
that depends on all three. A guard at the top refuses to run against a canvas that
has not been given a real size yet, which is what makes it safe to call from
`Loaded` as well as from `SizeChanged`.

The one piece of arithmetic worth reading on its own is how far out each label sits:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
/// <summary>
/// How far out a station label must sit so its box clears the card box on that petal.
/// The two rectangles are separated as soon as they clear on EITHER axis, so the smaller
/// of the two required distances is enough — which keeps the diagonal labels tucked in.
/// </summary>
private double LabelRadius(double labelWidth, double angleDegrees)
{
    var radians = angleDegrees * Math.PI / 180.0;
    var sin = Math.Abs(Math.Sin(radians));
    var cos = Math.Abs(Math.Cos(radians));
    var needX = sin > 0.02 ? (labelWidth / 2 + _cardW / 2 + 9) / sin : double.MaxValue;
    var needY = cos > 0.02 ? (9 + _cardH / 2 + 11) / cos : double.MaxValue;
    return _radius + Math.Min(needX, needY);
}
```

**Where to look.**
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs`
(`Relayout`, `Place`, `StationCentre`, `BuildOrnament`, `BuildSlots`,
`LabelRadius`, `ComputeRailGeometry`, `RepositionAllCards`)
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml` (the `SizeChanged`
handler on the scene canvas) and
`src/InannaRosette.UI/Controls/Ornament.cs` (`Polar`)

**Sharp edges.**
- `Loaded` and `SizeChanged` both have to be wired. On some heads the first real
  size arrives before `Loaded`, on others after, and a scene drawn only from one of
  them is empty on the other.
- The minimum-size guard is what keeps a transient zero or near-zero canvas from
  producing negative widths that then have to be clamped everywhere downstream.
- Recomputing everything means anything in flight was drawn for geometry that is
  about to vanish. The method ends any running animation on its first lines rather
  than letting it finish against coordinates that no longer exist.
- Work for a layer that is currently collapsed is skipped and a staleness flag is
  set instead, so the chrome is redrawn when that layer is shown again. Forget the
  flag and a window resized while the other panel was up comes back drawn for the
  old size.
- A `ScrollViewer` on these heads measures its child with an unbounded width, so
  the text column inside it is pinned to an explicit width here; without that,
  long paragraphs run off the panel edge instead of wrapping.

### Draw a control face procedurally inside a fixed design box

**When you want this.** A control whose appearance is data - forty variants of the
same design, each with its own text, colors and emblem - and which has to look
right at any size from a thumbnail to a full card. Authoring forty XAML files is
not the answer, and neither is a bitmap per variant.

**The MVVM shape.** Not a view-model concern at all. The control is a `UserControl`
with dependency properties for what varies, and everything it draws it builds in
code into two `Canvas` layers held at one fixed design size inside a `Viewbox`. The
control's own `Rebuild()` is the single entry point, called from the property
changed callbacks and from `Loaded`, so there is one code path whatever changed.

**Code.**

```xml
<!-- From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml -->
<Grid x:Name="Shell" Width="104" Height="166" RenderTransformOrigin="0.5,0.5">
    <Grid.RenderTransform>
        <CompositeTransform x:Name="LiftTransform" TranslateX="0" TranslateY="0" ScaleX="1" ScaleY="1" />
    </Grid.RenderTransform>

    <Grid x:Name="FaceHost" RenderTransformOrigin="0.5,0.5">
        <Grid.RenderTransform>
            <CompositeTransform x:Name="FaceTransform" Rotation="0" ScaleX="1" ScaleY="1" />
        </Grid.RenderTransform>
        <Viewbox Stretch="Fill">
            <Grid Width="250" Height="400">
                <Canvas x:Name="BackCanvas" Width="250" Height="400" />
                <Canvas x:Name="FaceCanvas" Width="250" Height="400" />
            </Grid>
        </Viewbox>
    </Grid>
    <!-- ... -->
    <Canvas x:Name="Overlay" IsHitTestVisible="True" />
</Grid>
```

The art is authored once at a single design size and the `Viewbox` does every
scale. That is why nothing inside the drawing code ever consults the control's real
width: the face at a thumbnail size and the face at full size are the same objects
under a different transform. The two canvases are siblings so that turning the card
over is a visibility swap, not a rebuild.

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs
private void BuildFace(Card card)
{
    var gold = Ornament.Brush(Ornament.Gold);
    var goldDeep = Ornament.Brush(Ornament.GoldDeep);
    var goldRule = Ornament.Brush(Ornament.GoldDeep, 0.55);
    var goldFaint = Ornament.Brush(Ornament.GoldDeep, 0.30);
    var secondary = Ornament.ToColor(card.SecondaryColor);
    // ...
    AddCornerFlourishes();

    // numeral / suit band
    FaceCanvas.Children.Add(Label(Ornament.Track(card.NumeralLabel), 24, 30, 96, 10, goldDeep));
    FaceCanvas.Children.Add(Label(Ornament.TrackLight(card.SuitName), 96, 31, 130, 8.6, goldDeep, TextAlignment.Right));
    FaceCanvas.Children.Add(Rect(24, 52, DW - 48, 1.4, 0, goldRule));
    FaceCanvas.Children.Add(Rect(24, 56.5, DW - 48, 0.8, 0, goldFaint));

    // medallion
    const double mx = DW / 2, my = 166, mr = 74;
    FaceCanvas.Children.Add(Circle(mx, my, mr, new RadialGradientBrush
    {
        Center = new Windows.Foundation.Point(0.5, 0.5),
        GradientOrigin = new Windows.Foundation.Point(0.5, 0.42),
        RadiusX = 0.5,
        RadiusY = 0.5,
        GradientStops =
        {
            new GradientStop { Color = Ornament.Lighten(secondary, 0.22), Offset = 0 },
            new GradientStop { Color = secondary, Offset = 0.55 },
            new GradientStop { Color = Ornament.ToColor(Ornament.Night), Offset = 1 },
        },
    }, gold, 1.8));
    FaceCanvas.Children.Add(Circle(mx, my, mr - 7, null, goldFaint, 0.9));

    AddEmblem(card, mx, my, mr * 1.46);
    // ...
```

Every constant in there is in design-box units. A radial gradient whose origin sits
above center gives the medallion a light source; mixing the card's own secondary
color towards white for the inner stop and down to the background color for the
outer one means one data field drives the whole medallion.

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs
private void ApplySize()
{
    var w = Math.Max(20, CardWidth);
    var h = Math.Max(32, CardHeight);
    Shell.Width = w;
    Shell.Height = h;
    Width = w;
    Height = h;
    GlowRect.Width = w + 6;
    GlowRect.Height = h + 6;
    HighlightRect.Width = w + 10;
    HighlightRect.Height = h + 10;
    FaceTransform.CenterX = w / 2;
    FaceTransform.CenterY = h / 2;
    LiftTransform.CenterX = w / 2;
    LiftTransform.CenterY = h / 2;
    BuildOverlay();
}
// ...
/// <summary>Rebuild all the vector art. Safe to call repeatedly.</summary>
public void Rebuild()
{
    FaceCanvas.Children.Clear();
    BackCanvas.Children.Clear();
    BuildBack();
    if (Card is not null) { BuildFace(Card); }
    ApplySize();
    ApplyFacing();
    ApplyOrientation(animate: false);
}
```

`ApplySize()` is the only place in the control that touches real pixels, and what
it resizes is the shell and the transform centers, never the art. `Rebuild()`
clears both canvases before it draws, which is what makes it safe to call from a
property changed callback that may fire several times before the control is ever
shown.

**Where to look.**
`InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml`
`InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs`
(`Rebuild`, `BuildFace`, `BuildBack`, `ApplySize`, `Rect`, `Circle`, `Label`,
`PathBox`, `BuildOverlay`) and
`src/InannaRosette.UI/Controls/Ornament.cs`

**Sharp edges.**
- The transform centers are set from the real size in `ApplySize()`, not from the
  design box. Leave them at their defaults and a flip or a rotation pivots around
  the wrong point at every size but one.
- The overlay layer sits outside the transformed host, so the badges on a card that
  has been turned upside down are still the right way up. Anything that must not
  rotate with the art has to live outside the rotating element.
- A face built from text that varies in length needs a size rule, not a fixed size:
  the name here steps down through three sizes by character count so the block
  always clears the band below it whether it sets on one line or two.
- Font families are named explicitly on every hand-built text element. Styles in
  the application resource dictionary do not reach elements created in code.
- The emblem lookup is wrapped in a `try` that returns quietly, so a variant whose
  art is missing draws a card with no emblem rather than throwing inside a property
  changed callback.

### Parse path data through a fallback chain that flattens arcs

**When you want this.** You are building `Path` geometry from path mini-language
strings at run time, and the parser behind that conversion is not the same on every
head. One head accepts everything; another rejects elliptical arcs with "the arc
must be based on a circle". You want one call that returns a usable geometry on all
of them, and a log line that tells you which road it took without a line per shape.

**The MVVM shape.** Not a view-model concern. The parse lives in one internal
static method on the control that draws, so every caller in the application - card
faces, backs, ornaments, overlay marks - goes through the same fallback chain and
the same logger.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs
/// <summary>Parse path mini-language into a Geometry (the XAML parser route, kept in one place).</summary>
internal static Geometry ParseGeometry(string data)
{
    if (string.IsNullOrWhiteSpace(data)) { return new PathGeometry(); }

    var parsed = TryParseGeometry(data);
    if (parsed is not null) { return parsed; }

    // This head's parser only accepts circular arcs; flatten every arc to beziers and retry.
    var flattened = Ornament.ArcsToBeziers(data);
    if (!ReferenceEquals(flattened, data))
    {
        parsed = TryParseGeometry(flattened);
        if (parsed is not null)
        {
            if (!_geometryFallbackLogged)
            {
                _geometryFallbackLogged = true;
                Log.LogWarning("Arcs flattened to beziers for this head's path parser.");
            }
            return parsed;
        }
    }

    Log.LogError("Geometry parse failed for \"{PathData}\".",
        data.Length <= 60 ? data : data[..60] + "\u2026");
    return new PathGeometry();
}
```

The chain is three steps, each cheaper than the one before it is general. The
binding helper's converter first, then the XAML reader, then the same string with
every arc rewritten as cubics and both parsers tried again:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs
private static Geometry? TryParseGeometry(string data)
{
    try { return (Geometry)Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Geometry), data); }
    catch (Exception) { /* try the reader */ }

    try
    {
        const string ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        var xaml = $"<Path xmlns=\"{ns}\" Data=\"{System.Security.SecurityElement.Escape(data)}\" />";
        if (Microsoft.UI.Xaml.Markup.XamlReader.Load(xaml) is Microsoft.UI.Xaml.Shapes.Path p && p.Data is not null)
        {
            return p.Data;
        }
    }
    catch (Exception) { /* caller reports */ }

    return null;
}
```

The rewrite itself refuses to guess. It returns the original string untouched when
there is no arc to rewrite, and also when the path uses relative commands, because
tracking a current point through relative commands is a different job from the one
it was written for:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Controls/Ornament.cs
/// <summary>
/// Rewrite every absolute elliptical-arc command (A) in a path as cubic beziers.
/// This head's path parser only accepts circular arcs ("the arc must be based on a circle,
/// not an ellipse"), and the deck art contains real ellipses, so arcs are flattened before
/// they reach it. Returns the original string when there is nothing to do, or when the path
/// uses relative commands (which are never produced by the deck art or by this class).
/// </summary>
public static string ArcsToBeziers(string data)
{
    if (string.IsNullOrWhiteSpace(data) || data.IndexOf('A') < 0) { return data; }
    foreach (var relative in "mlhvcsqtaz")
    {
        if (data.IndexOf(relative) >= 0) { return data; }
    }

    var tokens = System.Text.RegularExpressions.Regex.Matches(
        data, @"([MLHVCSQTAZ])([^MLHVCSQTAZ]*)");
    if (tokens.Count == 0) { return data; }

    // ...
```

**Where to look.**
`InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs`
(`ParseGeometry`, `TryParseGeometry`, `_geometryFallbackLogged`)
`InannaRosette/src/InannaRosette.UI/Controls/Ornament.cs`
(`ArcsToBeziers`, `ArcSegment`, `ParseNumbers`, `CirclePath`)

**Sharp edges.**
- The fallback is logged once per process, behind a static bool. Logging it per
  shape produces a line for every layer of every card and buries everything else.
- The failure log truncates the path data before printing it. Path strings run to
  hundreds of characters and an untruncated one makes the log unreadable.
- The final fallback returns an empty `PathGeometry`, not null and not an
  exception, so a shape that cannot be parsed is a missing ornament rather than a
  page that does not draw.
- The string handed to the XAML reader is escaped, because path data is put into an
  attribute value and an unescaped one is malformed markup.
- The rewrite is not free: the safer route for art you author yourself is to avoid
  arcs entirely. `Ornament.CirclePath` writes circles as four cubics for exactly
  that reason, so the shapes this application generates never need the fallback at
  all - only the authored art does.

### Begin every Storyboard inside a try that sets the final value

**When you want this.** You are animating with `Storyboard` on an application that
ships to several heads, and animation support is not equally deep on all of them.
You want the motion where it works, and the correct end state everywhere else -
without the page filling up with capability checks.

**The MVVM shape.** Not a view-model concern. Animation is a page and control
matter end to end. The rule is local and mechanical: every `Begin()` call in the
application sits inside a `try`, and every `catch` sets by hand the value the
animation would have arrived at.

**Code.**

```xml
<!-- From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml -->
<UserControl.Resources>
    <Storyboard x:Name="FlipOutStory">
        <DoubleAnimation Storyboard.TargetName="FaceTransform" Storyboard.TargetProperty="ScaleX"
                         From="1" To="0" Duration="0:0:0.13" />
    </Storyboard>
    <Storyboard x:Name="FlipInStory">
        <DoubleAnimation Storyboard.TargetName="FaceTransform" Storyboard.TargetProperty="ScaleX"
                         From="0" To="1" Duration="0:0:0.16" />
    </Storyboard>
    <Storyboard x:Name="RotateStory">
        <DoubleAnimation x:Name="RotateStep" Storyboard.TargetName="FaceTransform"
                         Storyboard.TargetProperty="Rotation"
                         From="0" To="180" Duration="0:0:0.26" />
    </Storyboard>
    <!-- ... -->
</UserControl.Resources>
```

The storyboards are declared in markup, one per motion, and started from code. A
flip is two of them with a face swap in between, and each stage carries its own
fallback:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs
/// <summary>Flip a face-down card up: ScaleX 1 to 0, swap the face, then 0 to 1.</summary>
public void FlipToFaceUp()
{
    if (IsFaceUp) { return; }
    try
    {
        void OnDone(object? s, object e)
        {
            FlipOutStory.Completed -= OnDone;
            IsFaceUp = true;
            try { FlipInStory.Begin(); } catch (Exception) { FaceTransform.ScaleX = 1; }
        }
        FlipOutStory.Completed += OnDone;
        FlipOutStory.Begin();
    }
    catch (Exception)
    {
        IsFaceUp = true;
        FaceTransform.ScaleX = 1;
    }
}

/// <summary>Fade/slide the card in (used by auto-lay and by drawing into the tray).</summary>
public void PlayAppear()
{
    try { AppearStory.Begin(); }
    catch (Exception) { Shell.Opacity = 1; }
}
```

The same rule applies to a storyboard built in code, where the fallback is also the
line that guarantees the value even when the animation did run:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
private static void FadeIn(UIElement element, bool visible)
{
    try
    {
        var story = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
        var animation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
        {
            From = visible ? 0 : 1,
            To = visible ? 1 : 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(220)),
        };
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animation, element);
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animation, "Opacity");
        story.Children.Add(animation);
        story.Begin();
        element.Opacity = visible ? 1 : 0;
    }
    catch (Exception)
    {
        element.Opacity = visible ? 1 : 0;
    }
}
```

Notice that `element.Opacity` is set inside the `try` as well, right after
`Begin()`. The animation and the assignment agree about the destination, so the
element is in the right state whether the storyboard ran, ran and was interrupted,
or never started.

**Where to look.**
`InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml` and
`Controls/CardView.xaml.cs` (`FlipToFaceUp`, `PlayAppear`, `ToggleReversed`,
`ApplyOrientation`, the hover handlers)
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs` (`FadeIn`, `ShowDetail`,
`Ease`)

**Sharp edges.**
- A `Completed` handler unsubscribes itself as its first statement. A storyboard is
  a resource that outlives the run, so a handler left attached fires again on the
  next one.
- The `catch` has to set the value the animation targeted, not the value it started
  from. A fallback that sets the wrong end leaves the control visibly wrong instead
  of merely unanimated.
- Where the motion is a multi-step move of several elements rather than a property
  ramp, this application drives it from an awaited per-frame loop instead, timed
  off a stopwatch rather than off a count of frames, because a delay is only ever
  at least as long as it was asked for.
- A generation counter guards those loops: the page increments it whenever the
  scene is rebuilt or the page unloads, and each frame checks that its generation
  is still current before touching anything.

### Fake letter spacing with thin spaces

**When you want this.** Your design calls for tracked small capitals - a title, a
label, a badge - and the text element you are drawing into ignores character
spacing on the heads you ship to.

**The MVVM shape.** Not a view-model concern: it is a typography helper, and it
belongs beside the palette tokens in the static class the page and the controls
share. It returns a string, so it composes with anything that takes text - a bound
property, a hand-built text element, a control's own caption.

**Code.**

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Controls/Ornament.cs
// ---------------------------------------------------------------- typography
private const char ThinSpace = ' ';
private const char HairSpace = ' ';

/// <summary>
/// TextBlock.CharacterSpacing is ignored on this head, so "tracked caps" are faked by
/// inserting thin spaces between letters. Existing spaces become a wider gap.
/// </summary>
public static string Track(string? text, bool upper = true)
{
    if (string.IsNullOrEmpty(text)) { return string.Empty; }
    var source = upper ? text.ToUpperInvariant() : text;
    var sb = new StringBuilder(source.Length * 2);
    for (int i = 0; i < source.Length; i++)
    {
        var ch = source[i];
        if (i > 0)
        {
            sb.Append(char.IsWhiteSpace(ch) || char.IsWhiteSpace(source[i - 1]) ? HairSpace : ThinSpace);
        }
        sb.Append(ch == ' ' ? ' ' : ch);
    }
    return sb.ToString();
}

/// <summary>Light tracking (hair spaces only) for longer strings that must still fit.</summary>
public static string TrackLight(string? text, bool upper = true)
{
    if (string.IsNullOrEmpty(text)) { return string.Empty; }
    var source = upper ? text.ToUpperInvariant() : text;
    var sb = new StringBuilder(source.Length * 2);
    for (int i = 0; i < source.Length; i++)
    {
        if (i > 0) { sb.Append(HairSpace); }
        sb.Append(source[i]);
    }
    return sb.ToString();
}
```

Two widths of space, not one. A thin space between letters and a wider hair space
where the source already had a word break, so the words stay legible as words
instead of dissolving into one run of letters. Callers ask for the treatment by
name at the point the string is made:

```csharp
// From CodeBrix.Samples/InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs
private static string StationLabel(int index)
{
    var positions = RosetteSpread.Positions;
    if (index < 0 || index >= positions.Count)
    {
        return Ornament.Track(index == 0 ? "The Heart" : $"{Card.ToRoman(index)}");
    }

    var pos = positions[index];
    return index == 0
        ? Ornament.Track(pos.Title)
        : Ornament.Track($"{Card.ToRoman(index)} · {pos.Title}");
}
```

The report side of this application does the same job properly, drawing one glyph
at a time and advancing the pen by the measured width plus a tracking amount,
because a graphics surface has a pen position and a text element does not. The
screen version is the compromise that a text element forces.

**Where to look.**
`InannaRosette/src/InannaRosette.UI/Controls/Ornament.cs` (`Track`, `TrackLight`)
`InannaRosette/src/InannaRosette.UI/Views/MainPage.xaml.cs` (`StationLabel`,
`BuildChips`, the constructor's title and badge text)
`InannaRosette/src/InannaRosette.UI/Controls/CardView.xaml.cs` (`BuildFace`,
`BuildBack`) and
`src/libs/InannaRosette.Reading/Services/Pdf/PdfText.cs` (`DrawTracked`)

**Sharp edges.**
- A tracked string is display text only. It is roughly twice as long as the
  original, so never round-trip it, search it, compare it or save it: track at the
  moment the string reaches the text element and nowhere earlier.
- The inserted characters are real spaces from the space family, which means a text
  element may break a line inside a tracked word. Tracked text wants
  `TextWrapping="NoWrap"` and a box wide enough for it.
- Measurement gets harder, not easier. `BuildChips` lays out its keyword pills by
  estimating each one's width from the untracked character count, because there is
  no wrapping panel here and the tracked string's real width is not something the
  page can ask for before the element exists. The estimate is deliberately generous
  so a row breaks early rather than overflowing.
- An embedded font that has no thin space glyph draws a missing-glyph box between
  every pair of letters. Prove the treatment with the font you actually ship before
  designing around it.
