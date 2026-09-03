# CodeBrix.Samples Blueprints: Project layout, packaging and native assets

These recipes cover how a multi-head application is laid out on
disk and how its packages are apportioned, so that adding a head
or a dependency does not mean editing every project file. They show
the Core-library-plus-one-runtime-package rule, the shared project that
compiles App.xaml and the views into each head, the root-namespace settings
that keep libraries referencing CodeBrix.Platform from colliding, and the
src/libs plus tests/libs shape that lets non-UI work be tested without a
window. They also cover native payloads: fanning per-platform native packages
out across the heads, embedding assets with explicit logical names, letting a
Windows-targeting head restore on Linux and macOS, keeping separate solutions
where some heads cannot build everywhere, and recording bundled content
in a notices file. Reach for this file when a build error is coming from
project configuration rather than from code, or when you are setting up a
new application's projects and want the conventions the samples already follow.

This file is one of the CodeBrix.Samples blueprints. The [index](BLUEPRINTS-Index.md)
lists every recipe across all of the blueprint files and explains the
conventions the code blocks follow.

## Recipes in this file

- [Carry every package in one Core library and give each head exactly one runtime package](#carry-every-package-in-one-core-library-and-give-each-head-exactly-one-runtime-package)
- [Share App xaml and the views across heads with a shared project](#share-app-xaml-and-the-views-across-heads-with-a-shared-project)
- [Set the Core library root namespace to the application namespace](#set-the-core-library-root-namespace-to-the-application-namespace)
- [Give a library that references CodeBrix Platform its own root namespace](#give-a-library-that-references-codebrix-platform-its-own-root-namespace)
- [Fan native packages out across the heads](#fan-native-packages-out-across-the-heads)
- [Embed an asset with an explicit logical name and load it by reflection](#embed-an-asset-with-an-explicit-logical-name-and-load-it-by-reflection)
- [Let a Windows-targeting head build inside a cross-platform solution](#let-a-windows-targeting-head-build-inside-a-cross-platform-solution)
- [Restrict the solution platforms to what a WinUI head declares](#restrict-the-solution-platforms-to-what-a-winui-head-declares)
- [Ship a separate solution where some heads cannot build everywhere](#ship-a-separate-solution-where-some-heads-cannot-build-everywhere)
- [Organize an application as src libs plus tests libs around a shared UI project](#organize-an-application-as-src-libs-plus-tests-libs-around-a-shared-ui-project)
- [Code to the higher-level graphics package and let the binding arrive transitively](#code-to-the-higher-level-graphics-package-and-let-the-binding-arrive-transitively)
- [Know what a transitive package brings and name what you depend on](#know-what-a-transitive-package-brings-and-name-what-you-depend-on)
- [Record bundled third-party content in a notices file](#record-bundled-third-party-content-in-a-notices-file)

## Related blueprints

- [BLUEPRINTS-AppStructureAndStartup.md](BLUEPRINTS-AppStructureAndStartup.md) - the head entry points, App.xaml and service registration that these project files compile and reference
- [BLUEPRINTS-Testing.md](BLUEPRINTS-Testing.md) - how the mirrored test projects under tests/libs are written, including the native assets a headless test project must reference itself
- [BLUEPRINTS-DocumentsAndData.md](BLUEPRINTS-DocumentsAndData.md) - the UI-free service libraries and embedded resources these project shapes are built around
- [BLUEPRINTS-MediaAndVision.md](BLUEPRINTS-MediaAndVision.md) - the camera, playback and model-inference libraries whose native packages these recipes fan out across the heads

---

## Project layout, packaging and native assets

### Carry every package in one Core library and give each head exactly one runtime package

**When you want this.** Any multi-head application. You want to add a head, or a
package, without editing six project files.

**The MVVM shape.** Not a view-model concern. A plain class library named
`<App>.Core` holds the view models and every package the application uses -
CodeBrix.Platform itself, every add-in, the font package, the generic host and the
third-party libraries. Each head project-references it and adds exactly one
runtime package. Every head repeats the same comment, which is what keeps the rule
true.

**Code.**

```xml
<!-- Adapted from CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/CodeBrixVideoTool.Core.csproj -->
<!-- Package IDs and versions elided; see the project's csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>

    <!-- Match the namespace used by the app code -->
    <RootNamespace>CodeBrixVideoTool</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <!-- ... CodeBrix.Platform, the Roboto font package, the generic host and console logging ... -->

    <!-- The VideoPlayer add-in - the VideoPlayer element the main page hosts. Referenced ONCE here:
         every head inherits it transitively, and it is live on all four heads because the
         containers, the demultiplexer and the clock are all managed code. The two codec packages it
         plays through are the application's own and live in CodeBrixVideoTool.Playback. -->
    <!-- ... the VideoPlayer add-in package ... -->
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\libs\CodeBrixVideoTool.Processing\CodeBrixVideoTool.Processing.csproj" />
    <ProjectReference Include="..\libs\CodeBrixVideoTool.Playback\CodeBrixVideoTool.Playback.csproj" />
  </ItemGroup>
</Project>
```

```xml
<!-- Adapted from CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.LinuxX11/PalmVisualizer.LinuxX11.csproj
     (package ids and versions elided - see the project's csproj) -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <OutputType>Exe</OutputType>
</PropertyGroup>

<!-- Tell MSBuild to treat .xaml files as CodeBrix.Platform XAML pages -->
<ItemGroup>
  <Page Include="**\*.xaml" Exclude="bin\**\*.xaml;obj\**\*.xaml" />
  <None Remove="**\*.xaml" />
</ItemGroup>

<!-- Shared UI files (App.xaml + Views) -->
<Import Project="..\PalmVisualizer.UI\PalmVisualizer.UI.projitems" Label="Shared" />
<ItemGroup>
  <ProjectReference Include="..\PalmVisualizer.Core\PalmVisualizer.Core.csproj" />
</ItemGroup>

<!-- EXACTLY ONE platform head package; all other packages come from PalmVisualizer.Core -->
<ItemGroup>
  <PackageReference Include="(the X11 platform runtime package)" />
</ItemGroup>
```

| Head | Runtime package family |
| --- | --- |
| LinuxX11 | CodeBrix.Platform Skia X11 runtime |
| LinuxWayland | CodeBrix.Platform Skia Wayland runtime |
| LinuxFrameBuffer | CodeBrix.Platform Skia framebuffer runtime |
| MacOS | CodeBrix.Platform Skia macOS runtime |
| Win32Skia | CodeBrix.Platform Skia Win32 runtime |
| WinWpfSkia | CodeBrix.Platform Skia WPF runtime |

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/CodeBrixVideoTool.Core.csproj`
`PalmVisualizer/src/` (all six head project files)
`MediaPlayerDemo/src/` and `PdfSideBySide/src/` (the same six-head shape)

**Also shown by.**
`JustBetweenUs`, `KenneyAssetBrowser`, `NotionDocumentCreator`, `WebcamPainter`,
`WikipediaPublisher`, `PolyHavenBrowser` - every application in the repository,
each with the rule written into every head as a comment.

**Sharp edges.**
- A second runtime package on one head is a build the tooling will not warn you
  about and a run that will not work.
- An add-in goes on Core, once. Say in the comment why it works where it does -
  "live on all four heads because the containers, the demultiplexer and the clock
  are all managed code" is the kind of note that saves the next reader a
  test run.
- The page glob and the matching `None` removal are required in every head, or the
  shared XAML arrives as content and is never compiled.
- Where an application defines symbols for the platform's own conditional
  compilation, define them in Core and in every head that compiles shared source;
  only some of them are meant for application code.
- The documented exceptions to "exactly one platform package" are native payloads;
  see the native-assets blueprint below.

### Share App xaml and the views across heads with a shared project

**When you want this.** One `App.xaml` and one set of pages, compiled into every
head assembly rather than into a library.

**The MVVM shape.** A shared project (`.shproj` plus `.projitems`) holds only XAML
and its code-behind. Each head imports the `.projitems` with the shared label, so
the pages compile into the head itself and can see the head's own types.

**Code.**

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/PdfSideBySide.UI.projitems -->
  <PropertyGroup Label="Configuration">
    <Import_RootNamespace>PdfSideBySide.UI</Import_RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <Page Include="$(MSBuildThisFileDirectory)App.xaml">
      <SubType>Designer</SubType>
      <Generator>MSBuild:Compile</Generator>
    </Page>
    <Page Include="$(MSBuildThisFileDirectory)Views\MainPage.xaml">
      <SubType>Designer</SubType>
      <Generator>MSBuild:Compile</Generator>
    </Page>
  </ItemGroup>
  <ItemGroup>
    <Compile Include="$(MSBuildThisFileDirectory)App.xaml.cs">
      <DependentUpon>App.xaml</DependentUpon>
    </Compile>
    <Compile Include="$(MSBuildThisFileDirectory)Views\MainPage.xaml.cs">
      <DependentUpon>MainPage.xaml</DependentUpon>
    </Compile>
  </ItemGroup>
```

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.LinuxX11/PdfSideBySide.LinuxX11.csproj -->
  <!-- Tell MSBuild to treat .xaml files as CodeBrix.Platform XAML pages -->
  <ItemGroup>
    <Page Include="**\*.xaml" Exclude="bin\**\*.xaml;obj\**\*.xaml" />
    <None Remove="**\*.xaml" />
  </ItemGroup>

  <!-- Shared UI files (App.xaml + Views) -->
  <Import Project="..\PdfSideBySide.UI\PdfSideBySide.UI.projitems" Label="Shared" />
  <ItemGroup>
    <ProjectReference Include="..\PdfSideBySide.Core\PdfSideBySide.Core.csproj" />
  </ItemGroup>
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.UI/` (the `.shproj` and `.projitems` pair)
`MediaPlayerDemo/src/MediaPlayerDemo.UI/`
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/`
`WebcamPainter/src/WebcamPainter.UI/`

**Also shown by.**
`JustBetweenUs`, `KenneyAssetBrowser`, `NotionDocumentCreator`, `PalmVisualizer`,
`PolyHavenBrowser`, `WikipediaPublisher`, `CodeBrixVideoTool`.

**Sharp edges.**
- The shared project's identifier and the item list's shared identifier are the
  same value; that pairing is what makes the shared project work.
- There is no globbing in the shared project: a new page and its code-behind must
  be added by hand, as a page item with the compile generator and as a compile
  item that depends upon its XAML.
- The shared project's import root namespace is deliberately not the namespace the
  files declare. The C# namespace and the XAML class attribute win; the head's own
  root namespace is what has to agree with them.
- The XAML compiles into the head, not into Core, which is why a page can reference
  Core types with an assembly-qualified XML namespace but Core cannot reference the
  page.
- The shared project produces no assembly, but list it in the solution anyway so it
  appears in the tree.

### Set the Core library root namespace to the application namespace

**When you want this.** The library carrying your view models is named
`<App>.Core`, but you want its types in the `<App>` namespace so shared XAML and
head code see them without extra qualification.

**The MVVM shape.** One property on the Core project. View models then live in
`<App>.ViewModels`, helpers in `<App>.Helpers`, and the shared XAML reaches them
with an assembly-qualified namespace.

**Code.**

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/MediaPlayerDemo.Core.csproj -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>

  <!-- Match the namespace used by the app code -->
  <RootNamespace>MediaPlayerDemo</RootNamespace>
</PropertyGroup>
```

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml -->
<Page
    x:Class="MediaPlayerDemo.Views.MainPage"
    xmlns="clr-namespace:Microsoft.UI.Xaml.Controls;assembly=CodeBrix.Platform.UI"
    xmlns:d="clr-namespace:Microsoft.UI.Xaml.Data;assembly=CodeBrix.Platform.UI"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="clr-namespace:MediaPlayerDemo.ViewModels;assembly=MediaPlayerDemo.Core"
    ...>
```

A head that compiles linked shared source has the same problem and solves it the
same way, either by rewriting its own root namespace or by keeping its own and
letting the linked files declare theirs:

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Wpf/JustBetweenUs.Wpf.csproj -->
<RootNamespace>$(MSBuildProjectName.Replace(" ", "_").Replace(".Wpf", ""))</RootNamespace>
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/MediaPlayerDemo.Core.csproj`
`WebcamPainter/src/WebcamPainter.Core/WebcamPainter.Core.csproj`
`JustBetweenUs/JustBetweenUs.Wpf/JustBetweenUs.Wpf.csproj` and
`JustBetweenUs/JustBetweenUs.WinUI/JustBetweenUs.WinUI.csproj`

**Also shown by.**
`PdfSideBySide`, `PolyHavenBrowser`, `PolyHavenBrowser_viewer_only`,
`PainDiagram`, `WikipediaPublisher`, `NotionDocumentCreator`, `PalmVisualizer`,
`KenneyAssetBrowser`, `CodeBrixVideoTool`.

**Sharp edges.**
- The namespace and the assembly name are deliberately different things: the XAML
  still says `assembly=<App>.Core` while the namespace says `<App>.ViewModels`.
- Core's root namespace also decides the manifest resource names of everything it
  embeds. Change it and every embedded-resource URI in the XAML has to change too.
- Either choice works for a head with linked source - rewrite the head's root
  namespace to match the files, or keep the head's own and let each file declare
  its namespace - as long as it is deliberate.
- Files whose folder and namespace no longer agree carry a one-line analyzer
  suppression saying so, rather than being moved.

### Give a library that references CodeBrix Platform its own root namespace

**When you want this.** You put XAML-facing code - a view model, a custom element -
in a library under `src/libs`, so the library references CodeBrix.Platform, and the
build starts reporting a duplicate type in the head.

**The MVVM shape.** Project configuration only, but it decides whether the build
succeeds. The Core project claims the application namespace; every library that
also sees CodeBrix.Platform must claim a different one.

**Code.**

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/CodeBrixVideoTool.Playback.csproj -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>

  <!-- This library hosts a SimpleViewModel-derived view model, so it references CodeBrix.Platform.
       Keep this library's OWN RootNamespace (not the app's "CodeBrixVideoTool") so the per-head
       generated GlobalStaticResources class does not collide across assemblies (CS0433). -->
  <RootNamespace>CodeBrixVideoTool.Playback</RootNamespace>
</PropertyGroup>
```

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Pinta.Brix.Controls.csproj -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
  <!-- Library referencing CodeBrix.Platform keeps its OWN RootNamespace
       (heads fail CS0433 on GlobalStaticResources otherwise) -->
  <RootNamespace>Pinta.Brix.Controls</RootNamespace>
</PropertyGroup>
```

**Where to look.**
`CodeBrixVideoTool/src/libs/*/`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Pinta.Brix.Controls.csproj`
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmVisualizer.Rendering.csproj`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/PolyHavenBrowser.Rendering.csproj`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/KenneyAssetBrowser.Rendering.csproj`

**Sharp edges.**
- The symptom is a duplicate-type error on the generated per-head resources class,
  reported in the head rather than in the library, so it is easy to misdiagnose.
- The rule is conditional and the samples say so: a library that hosts no
  XAML-facing type keeps its default root namespace, which is already its assembly
  name and therefore already distinct.
- Naming a library `<App>.<Something>` already gives it a distinct default. Setting
  the property anyway documents the rule and survives a project rename.
- One library goes the other way on purpose: it must not override the property,
  because its embedded fonts are looked up by a name derived from it. Decide which
  rule a library is under before you touch the property.
- A library that hosts a custom element usually needs a few more properties with
  it - documentation generation, and unsafe blocks where the element uploads
  matrices or binds vertex attributes.

### Fan native packages out across the heads

**When you want this.** A library you use has a native component, and each head has
to carry the native binaries for the platforms it can run on.

**The MVVM shape.** The library that calls the native API references only the
managed binding, so it stays runtime-independent. The native packages are
referenced by the head projects.

**Code.**

```xml
<!-- From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmVisualizer.Vision.csproj -->
<ItemGroup>
  <!-- OpenCV 5 (managed binding): TFLite model inference via the DNN module.
       The native OpenCV library comes from the per-platform
       CodeBrix.VideoProcessing.OpenCV5.{Platform} packages referenced by each head. -->
  <PackageReference Include="..." />
</ItemGroup>
```

```xml
<!-- Adapted from CodeBrix.Samples/WebcamPainter/src/WebcamPainter.MacOS/WebcamPainter.MacOS.csproj
     (package IDs and versions removed - see the project's csproj for those) -->

<!-- EXACTLY ONE platform head package; all other packages come from WebcamPainter.Core -->
<ItemGroup>
  <PackageReference Include="(CodeBrix.Platform runtime for this head)" Version="(see csproj)" />
</ItemGroup>

<!-- Native OpenCV library for the hand-tracking (Paint Mode) pipeline -->
<ItemGroup>
  <PackageReference Include="(OpenCV native for macOS arm64)" Version="(see csproj)" />
  <PackageReference Include="(OpenCV native for macOS x64)"   Version="(see csproj)" />
</ItemGroup>
```

| Head | Native packages referenced |
| --- | --- |
| LinuxX11, LinuxWayland, LinuxFrameBuffer | Linux x64 and Linux arm64 |
| MacOS | macOS arm64 and macOS x64 |
| Win32Skia, WinWpfSkia | Windows x64 and Windows arm64 |

An add-in with a platform-specific native backend is the same rule with a shorter
list - the add-in on Core, the native only where it is needed:

```xml
<!-- Adapted from CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Win32Skia/MediaPlayerDemo.Win32Skia.csproj -->
<ItemGroup>
  <!-- EXACTLY ONE platform head package; all other packages come from MediaPlayerDemo.Core -->
  <PackageReference Include="(the CodeBrix.Platform Skia Win32 runtime package)" Version="..." />
  <!--The following package is required on Window heads for the CodeBrix.Platform.MediaPlayer add-in-->
  <PackageReference Include="(the VideoLAN libVLC for Windows package)" Version="..." />
</ItemGroup>
```

**Where to look.**
`PalmVisualizer/src/` (the six head project files) and
`PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmVisualizer.Vision.csproj`
`WebcamPainter/src/` (the six head project files)
`MediaPlayerDemo/src/MediaPlayerDemo.Win32Skia/` and `MediaPlayerDemo.WinWpfSkia/`

**Sharp edges.**
- Put the native packages in the head, never in the library: a library that names a
  runtime identifier stops being reusable across heads.
- Heads reference both architectures of their platform unconditionally, so a head
  publishes for either without editing the project. Only a test project conditions
  on the build machine, because a test run needs one machine's binary.
- A native dependency an add-in needs on some heads is the documented exception to
  "exactly one platform package". Leaving it off builds cleanly and fails at run
  time, so put the reason in a comment beside it.
- Where an application says nothing about a platform's native requirement, that is
  not the same as saying none is needed; check the add-in's own documentation
  before shipping there.
- Some packages carry their own natives for every runtime identifier, with a
  license file beside each. Those need no fan-out at all and no system library
  installed - worth stating in a comment so nobody adds one.
- Where a package's identifier carries a license suffix, that suffix is how the
  family encodes the license. Read it before taking the dependency.

### Embed an asset with an explicit logical name and load it by reflection

**When you want this.** A model, an image or a font has to travel inside an
assembly rather than as loose content a deployment could forget - and the same
source file may be compiled into several assemblies.

**The MVVM shape.** The project embeds the file with an explicit logical name; the
code loads it from its own assembly by that exact name and fails with a message
that names the resource.

**Code.**

```xml
<!-- From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.Core/PainDiagram.Core.csproj -->
<!-- The body-map image the view model loads; the logical name must match
     MainViewModel.BodyMapResourceName -->
<ItemGroup>
  <EmbeddedResource Include="..\..\Shared\Assets\body_map_master.png" Link="Assets\body_map_master.png">
    <LogicalName>PainDiagram.Assets.body_map_master.png</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
//The body-map image is embedded with this logical name by every head that compiles
//  this file (PainDiagram.Core, PainDiagram.WinUI, and PainDiagram.Wpf)
private const string BodyMapResourceName = "PainDiagram.Assets.body_map_master.png";

private void LoadBodyMapBackground()
{
    //The view model is compiled into a different assembly on each head, and each of those
    //  assemblies embeds the body-map image under the same logical resource name
    using Stream resourceStream = typeof(MainViewModel).Assembly.GetManifestResourceStream(BodyMapResourceName);
    if (resourceStream == null)
    {
        Debug.WriteLine($"Embedded body-map image not found: {BodyMapResourceName}");
        return;
    }

    using var buffer = new MemoryStream();
    resourceStream.CopyTo(buffer);
    _session.SetBackgroundImage(buffer.ToArray());
}
```

A library that owns a large binary asset does the same, with the file linked in
from outside the project directory:

```xml
<!-- From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmVisualizer.Vision.csproj -->
<ItemGroup>
  <EmbeddedResource Include="..\..\..\models\gesture_recognizer_2026-07-13\hand_landmarker\hand_detector.tflite"
                    Link="Models\hand_detector.tflite">
    <LogicalName>PalmVisualizer.Vision.Models.hand_detector.tflite</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs
internal static byte[] LoadEmbeddedModel(string resourceName)
{
    using Stream stream = typeof(PalmTracker).Assembly.GetManifestResourceStream(resourceName);
    if (stream == null)
    {
        throw new InvalidOperationException($"Embedded model not found: {resourceName}");
    }
    using var buffer = new MemoryStream();
    stream.CopyTo(buffer);
    return buffer.ToArray();
}
```

**Where to look.**
`PainDiagram/Shared/ViewModels/MainViewModel.cs` and the embedded-resource items in
`PainDiagram.Core.csproj`, `PainDiagram.WinUI.csproj` and `PainDiagram.Wpf.csproj`
`PalmVisualizer/src/libs/PalmVisualizer.Vision/` and
`WebcamPainter/src/libs/WebcamPainter.Vision/`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/JustBetweenUs.Core.csproj` and
`JustBetweenUs/JustBetweenUs.WinUI/JustBetweenUs.WinUI.csproj`

**Sharp edges.**
- The explicit logical name is the reliable form. Without it the name is derived
  from the root namespace and the link path, so it changes when the file moves or
  the project is renamed - and shared source compiled into several assemblies would
  get a different name in each of them.
- The link attribute only decides where the file appears in the IDE; the real file
  can stay at the application root where a notices file can point at it.
- Decide per asset whether a failure is fatal. A missing background logs and
  returns; a missing model throws with the resource name in the message.
- The same file can be embedded in one head and shipped as content in another, and
  a head that does not use it embeds nothing. Assets are a per-head decision.
- Where names are derived rather than stated - embedded fonts resolved by root
  namespace plus folder - the removal item must precede the embed item, or the
  files are included twice.
- Embedding only part of a downloaded bundle deserves a comment saying why the rest
  was left out; that comment is what stops someone re-adding it.

### Let a Windows-targeting head build inside a cross-platform solution

**When you want this.** One head needs Windows desktop APIs and the rest do not,
and you want the whole solution to restore and build on Linux and macOS.

**The MVVM shape.** Packaging only. The WPF-hosted Skia head targets the Windows
framework moniker and turns on Windows targeting so a non-Windows machine can still
evaluate and restore it. It must not turn on the WPF build support.

**Code.**

```xml
<!-- From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.WinWpfSkia/PainDiagram.WinWpfSkia.csproj -->
<PropertyGroup>
  <!--
    The WPF-hosted head must target net10.0-windows (the runtime package flows a
    Microsoft.WindowsDesktop.App.WPF FrameworkReference). Do NOT set <UseWPF> here -
    that would make the WPF build targets grab the CodeBrix.Platform XAML
    Page items. EnableWindowsTargeting lets this head compile inside the cross-platform
    solution on Linux and macOS build hosts.
  -->
  <TargetFramework>net10.0-windows</TargetFramework>
  <OutputType>Exe</OutputType>
  <EnableWindowsTargeting>true</EnableWindowsTargeting>
</PropertyGroup>
```

```xml
<!-- From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/PainDiagram.Wpf.csproj -->
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <!-- SkiaSharp.Views.WPF ships net10.0-windows10.0.19041 assets, so the TFM must
       carry (at least) that Windows platform version -->
  <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
  <UseWPF>true</UseWPF>
  <RootNamespace>$(MSBuildProjectName.Replace(" ", "_").Replace(".Wpf", ""))</RootNamespace>
  <!-- Lets the project compile (not run) on Linux/macOS build hosts -->
  <EnableWindowsTargeting>true</EnableWindowsTargeting>
</PropertyGroup>
```

**Where to look.**
`PainDiagram/CodeBrixPlatform/PainDiagram.WinWpfSkia/PainDiagram.WinWpfSkia.csproj`
and `PainDiagram/PainDiagram.Wpf/PainDiagram.Wpf.csproj`
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.WinWpfSkia/WikipediaPublisher.WinWpfSkia.csproj`
`MediaPlayerDemo/src/MediaPlayerDemo.WinWpfSkia/MediaPlayerDemo.WinWpfSkia.csproj`

**Also shown by.**
`NotionDocumentCreator`, `PdfSideBySide`, `WebcamPainter`, `PalmVisualizer`,
`KenneyAssetBrowser` (whose native WPF head sets the same property).

**Sharp edges.**
- The WPF-support switch is the one to remember: the Skia head hosted in WPF
  targets the Windows moniker but must leave it off, or the WPF build targets claim
  the platform's XAML page items. A genuinely native WPF head does set it.
- The other Windows head does not need the Windows moniker at all; it targets plain
  `net10.0` and needs no Windows targeting property.
- It compiles, it does not run. Heads that are Windows-only in a stronger sense -
  a native WinUI 3 head, a native WPF head - are usually kept out of the
  cross-platform solution entirely rather than given this property.
- A native WPF head's moniker may need a Windows platform version, because the
  graphics views package for WPF only ships assets for that platform.

### Restrict the solution platforms to what a WinUI head declares

**When you want this.** You add a native WinUI 3 head to a solution whose other
projects build as Any CPU.

**The MVVM shape.** Head configuration plus solution mapping. The head declares
the architectures it supports, its runtime identifiers, its publish profile pattern
and its packaging tooling; the solution declares the same platform list and maps
each one onto the head.

**Code.**

```xml
<!-- From CodeBrix.Samples/PainDiagram/PainDiagram.Windows.slnx -->
<!-- PainDiagram.WinUI only declares Platforms x86/x64/ARM64 (no Any CPU),
     so the solution platforms are restricted to match - otherwise VS offers
     "Any CPU" and fails to map it to the WinUI project. -->
<Configurations>
  <Platform Name="x86" />
  <Platform Name="x64" />
  <Platform Name="ARM64" />
</Configurations>
<!-- ... -->
<Project Path="PainDiagram.WinUI/PainDiagram.WinUI.csproj">
  <Platform Solution="*|x86" Project="x86" />
  <Platform Solution="*|x64" Project="x64" />
  <Platform Solution="*|ARM64" Project="ARM64" />
  <Deploy Solution="Debug|x64" />
</Project>
```

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.WinUI/JustBetweenUs.WinUI.csproj -->
<OutputType>WinExe</OutputType>
<TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
<TargetPlatformMinVersion>10.0.17763.0</TargetPlatformMinVersion>
<RootNamespace>JustBetweenUs.WinUI</RootNamespace>
<ApplicationManifest>app.manifest</ApplicationManifest>
<Platforms>x86;x64;ARM64</Platforms>
<RuntimeIdentifiers Condition="$([MSBuild]::GetTargetFrameworkVersion('$(TargetFramework)')) &gt;= 8">win-x86;win-x64;win-arm64</RuntimeIdentifiers>
<PublishProfile>win-$(Platform).pubxml</PublishProfile>
<UseWinUI>true</UseWinUI>
<EnableMsixTooling>true</EnableMsixTooling>
<DefineConstants>$(DefineConstants);HAS_WINUI</DefineConstants>
```

**Where to look.**
`PainDiagram/PainDiagram.Windows.slnx` and `PainDiagram/PainDiagram.WinUI/`
`WikipediaPublisher/WikipediaPublisher.Windows.slnx`
`JustBetweenUs/JustBetweenUs.WinUI/JustBetweenUs.WinUI.csproj` and
`JustBetweenUs/JustBetweenUs.Windows.sln`

**Sharp edges.**
- Without the platform mapping the solution will not build with Any CPU selected,
  because the head declares no such platform.
- The WinUI head is usually the only project in the solution with deploy entries,
  and the only one whose Any CPU configuration is redirected to a concrete
  architecture.
- The packaging capability blocks in the head are guarded so the tooling menus
  appear even before the Windows App SDK package has been restored.
- Two launch profiles are worth keeping, packaged and unpackaged: you do not have
  to package the application to run it.
- The cross-platform solution simply does not include this head, which is why it
  keeps the default configuration.

### Ship a separate solution where some heads cannot build everywhere

**When you want this.** Some heads only build on one operating system, and you want
a solution that opens cleanly and builds everything it contains.

**The MVVM shape.** Not a code pattern; a repository shape. One solution per
operating system - or one cross-platform solution plus a Windows superset - all
sharing the same project files.

**Code.**

```text
JustBetweenUs.Windows.sln   all six Skia heads + WinUI + WPF + Mobile + Encryption + tests
JustBetweenUs.Linux.sln     Skia heads except WinWpfSkia + Encryption + tests
JustBetweenUs.MacOS.sln     Skia heads except WinWpfSkia + Mobile + Encryption + tests
```

**Where to look.**
`JustBetweenUs/JustBetweenUs.Windows.sln`, `JustBetweenUs.Linux.sln`,
`JustBetweenUs.MacOS.sln`
`PainDiagram/PainDiagram.slnx` and `PainDiagram/PainDiagram.Windows.slnx`
`WikipediaPublisher/WikipediaPublisher.slnx` and
`WikipediaPublisher/WikipediaPublisher.Windows.slnx`

**Sharp edges.**
- Two solution files is the usual shape: one cross-platform, one Windows-only that
  is a superset, both at the application root with a comment at the top saying
  which is which.
- Exclude a head only when it genuinely cannot restore. A Win32 Skia head targets
  plain `net10.0` and so restores and builds anywhere even though it only runs on
  Windows; the WPF-hosted head targets the Windows moniker and cannot.
- A mobile head belongs only in the solutions whose workloads can build it.
- Where a solution declares several platform names, every project except the WinUI
  head maps all of them to Any CPU.

### Organize an application as src libs plus tests libs around a shared UI project

**When you want this.** Your application has more than a page and a view model, and
you want the non-UI work in libraries that can be unit tested without a window.

**The MVVM shape.** The shared project holds only XAML and its code-behind. Core
holds view models and helpers and carries the platform packages. Each self-contained
concern becomes a library under `src/libs` with a mirrored test project under
`tests/libs`. The view model is the only place the libraries meet.

**Code.**

```text
src/PalmVisualizer.UI/            .shproj + .projitems: App.xaml(.cs), Views/MainPage.xaml(.cs)
src/PalmVisualizer.Core/          view models + helpers; owns the platform and font packages
src/libs/PalmVisualizer.Camera/   capture + preview canvas       -> tests/libs/PalmVisualizer.Camera.Tests
src/libs/PalmVisualizer.Vision/   palm tracking + models         -> tests/libs/PalmVisualizer.Vision.Tests
src/libs/PalmVisualizer.Rendering/ engine session + shader scene -> tests/libs/PalmVisualizer.Rendering.Tests
src/PalmVisualizer.<Head>/        one per head; imports the .projitems, references Core
```

```xml
<!-- From CodeBrix.Samples/PalmVisualizer/PalmVisualizer.slnx -->
<Folder Name="/Libraries/">
  <Project Path="src/libs/PalmVisualizer.Camera/PalmVisualizer.Camera.csproj" />
  <Project Path="src/libs/PalmVisualizer.Rendering/PalmVisualizer.Rendering.csproj" />
  <Project Path="src/libs/PalmVisualizer.Vision/PalmVisualizer.Vision.csproj" />
</Folder>
<Folder Name="/Tests/">
  <Project Path="tests/libs/PalmVisualizer.Camera.Tests/PalmVisualizer.Camera.Tests.csproj" />
  <Project Path="tests/libs/PalmVisualizer.Rendering.Tests/PalmVisualizer.Rendering.Tests.csproj" />
  <Project Path="tests/libs/PalmVisualizer.Vision.Tests/PalmVisualizer.Vision.Tests.csproj" />
</Folder>
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/InternalsVisibleTo.cs
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PdfSideBySide.PdfRender.Tests")]
```

**Where to look.**
`PalmVisualizer/PalmVisualizer.slnx` and the projects under `src/libs` and
`tests/libs`
`PolyHavenBrowser/PolyHavenBrowser.slnx`
`PdfSideBySide/PdfSideBySide.slnx`
`WebcamPainter/WebcamPainter.slnx`

**Also shown by.**
`PolyHavenBrowser_viewer_only`, `NotionDocumentCreator`, `KenneyAssetBrowser`,
`CodeBrixVideoTool`, `Pinta.Brix`.

**Sharp edges.**
- Each library owns the packages only it needs, and one of them usually states the
  ownership rule outright - the application's Core project depends on the library
  rather than referencing what the library wraps.
- Libraries do not reference each other. All composition happens in the view model,
  which is what keeps each library's seam a plain type.
- Every library carries an internals-visible file naming only its own test
  assembly, at the library root, holding nothing else.
- The solution folders are declarations; the folder names on disk are `src/libs`
  and `tests/libs`.
- Libraries commonly enable documentation generation, nullable annotations and
  implicit usings while the head projects and Core do not; a library doing pixel or
  interop work also needs unsafe blocks.
- A library with no platform reference is what keeps its test project free of UI
  packages - and a test project has no head, so it must reference the native assets
  it needs itself.

### Code to the higher-level graphics package and let the binding arrive transitively

**When you want this.** You want hardware 3D with a clean dependency graph, and you
are about to add a direct package reference to the low-level binding. Don't.

**The MVVM shape.** A packaging rule, recorded as a comment in every project that
touches the graphics API. Neither project declares the binding package; it arrives
through the element library.

**Code.**

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/PolyHavenBrowser.Core.csproj -->
<!-- The 3D preview control (ModelSceneGlCanvas) lives in PolyHavenBrowser.Rendering and is
     built on Graphics3DGL's GLCanvasElement. The app codes to Graphics3DGL — never to
     CodeBrix.Platform.OpenGL directly — so the OpenGL binding is only ever a transitive
     dependency (Graphics3DGL -> CodeBrix.Platform.OpenGL). -->
```

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/PolyHavenBrowser.Rendering.csproj -->
<!-- CodeBrix.Platform (base) supplies the FrameworkElement / DependencyProperty surface the
     GLCanvasElement subclass is built on. Graphics3DGL supplies GLCanvasElement itself and,
     transitively, the CodeBrix.Platform.OpenGL `GL` type the shader renderer draws with.
     The app codes to Graphics3DGL and never references CodeBrix.Platform.OpenGL directly. -->
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/PolyHavenBrowser.Core.csproj`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/PolyHavenBrowser.Rendering.csproj`
`PolyHavenBrowser_viewer_only/src/` (the same two comments, one of them naming the
off-screen context)
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/KenneyAssetBrowser.Rendering.csproj`

**Sharp edges.**
- The rule is about the package reference, not the using directive: the code does
  name the binding's namespace, because that is where the graphics type lives. No
  project declares a package reference to it.
- The pay-off is that the off-screen context resolves the head's own native
  graphics wrapper, so the application carries no platform loader of its own and
  works on every head.
- The element subclass needs both packages named: the base platform for the element
  and property surface it derives from, and the element library for the canvas
  itself.

### Know what a transitive package brings and name what you depend on

**When you want this.** You are wondering whether to add a package reference for a
type you can already see, and whether a rasterizer needs a system library
installed.

**The MVVM shape.** Not a view-model concern, but a real packaging fact. The
library's project file names one package; the code uses types from three.

**Code.**

```xml
<!-- Adapted from CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/PdfSideBySide.PdfRender.csproj
     (the package reference itself is elided - see the project's csproj) -->
  <ItemGroup>
    <!-- PDFium-backed page rasterizer (page counts + page-to-PNG); bundles its own natives -->
  </ItemGroup>
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs
using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats.Png;
using CodeBrix.PdfRasterizer;
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/Helpers/TestPdfs.cs
using CodeBrix.PdfDocuments.Drawing;
using CodeBrix.PdfDocuments.Pdf;
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/PdfSideBySide.PdfRender.csproj`
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/Helpers/TestPdfs.cs`

**Sharp edges.**
- The rasterizer brings the imaging library and the PDF authoring library with it,
  and the authoring library brings compression. That is why the renderer can encode
  images and the test helper can write PDFs without either project naming those
  libraries.
- Convenient, but an upgrade of the top package moves the others too. If you depend
  on one of them directly, say so directly.
- "Bundles its own natives" is worth stating literally when it is true: the package
  carries the native library for each supported runtime identifier, each with its
  own license beside it, so there is no per-head fan-out to arrange and no system
  library to install.

### Record bundled third-party content in a notices file

**When you want this.** Every application. Anything you bundle, download at run
time, or ship inside an assembly has a license, and the place to say so is one file
at the application root.

**The MVVM shape.** Not applicable. One `THIRD-PARTY-NOTICES.txt` per application
folder, listing bundled content by path, with its origin, copyright and license -
and saying what it deliberately does not cover.

**Code.**

```text
// From CodeBrix.Samples/PalmVisualizer/THIRD-PARTY-NOTICES.txt
Third-party CODE dependencies are consumed as NuGet packages. Each package
carries its own license and third-party notices in its own repository/package
(the CodeBrix.* packages ship their own THIRD-PARTY-NOTICES.txt), so those are
not reproduced here.

------------------------------------------------------------------------
MediaPipe models (bundled: models/**/*.tflite)
------------------------------------------------------------------------
```

```text
// From CodeBrix.Samples/PolyHavenBrowser/THIRD-PARTY-NOTICES.txt
------------------------------------------------------------------------
Poly Haven assets (downloaded at run time)
------------------------------------------------------------------------
...
None of these assets are redistributed as part of this repository; they are
fetched on demand and cached locally.
```

**Where to look.**
`PalmVisualizer/THIRD-PARTY-NOTICES.txt`
`PolyHavenBrowser/THIRD-PARTY-NOTICES.txt` (bundled fonts as well as downloaded
assets)
Every other application folder in the repository carries the same file.

**Sharp edges.**
- Name the path each entry covers, so a reader can match a file on disk to its
  license.
- Say what the file does not cover: package dependencies carry their own notices,
  and content that is downloaded rather than redistributed is a different statement
  from content that ships in the repository.
- Bundled fonts count. A font embedded in a library needs its license text beside
  it and an entry here.
- Adding a bundled asset means editing this file in the same change, not later.

