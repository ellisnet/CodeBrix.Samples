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
- [Copy a guest program tree beside the binaries and read it from there](#copy-a-guest-program-tree-beside-the-binaries-and-read-it-from-there)
- [Generate an application's whole asset set from arithmetic on first run](#generate-an-applications-whole-asset-set-from-arithmetic-on-first-run)
- [Bind a native driver by its bare name and resolve it yourself at run time](#bind-a-native-driver-by-its-bare-name-and-resolve-it-yourself-at-run-time)
- [Keep a third-party API inside one library with PrivateAssets and a module initializer](#keep-a-third-party-api-inside-one-library-with-privateassets-and-a-module-initializer)
- [Ship a data corpus as content items and find it under the application base directory](#ship-a-data-corpus-as-content-items-and-find-it-under-the-application-base-directory)
- [Depend on a native runtime the user installs instead of shipping a package](#depend-on-a-native-runtime-the-user-installs-instead-of-shipping-a-package)

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

### Copy a guest program tree beside the binaries and read it from there

**When you want this.** The thing your application runs is a folder of files
rather than a package - a script program, a template set, a data tree - and it has
to be beside the binaries at run time on every head, with one lookup that does not
care where the build put the output.

**The MVVM shape.** Not a view-model concern, but the split matters. The asset tree
lives in the Core project, which is the project every head references; the library
that reads it takes the directory as a parameter and is therefore usable against a
tree anywhere. Exactly two project-file items do the copying, and the run-time side
of the contract is one lookup under the base directory.

**Code.**

```xml
<!-- From CodeBrix.Samples/DRAKON.Brix/src/DRAKON.Brix.Core/DRAKON.Brix.Core.csproj -->
  <!-- The vendored DRAKON Editor Tcl tree + the bootstrap glue script,
       copied beside the binaries so the runtime can source them. -->
  <ItemGroup>
    <None Include="Assets\bootstrap.tcl" CopyToOutputDirectory="PreserveNewest" />
    <None Include="Assets\drakon\**\*" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>
```

```csharp
// From CodeBrix.Samples/DRAKON.Brix/src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs
    WindowTree tree = host.Tree;
    string assets = Path.Combine(AppContext.BaseDirectory, "Assets");
    // ...
    string bootstrap = Path.Combine(assetsDirectory, "bootstrap.tcl");
    string editor = Path.Combine(assetsDirectory, "drakon", "drakon_editor.tcl");
```

The hosted start is the only place the base directory appears. Everything below it
works from the directory it was handed, which is what lets a headless caller point
the same boot sequence at the tree in the source folder instead of the output one.

**Where to look.**
`DRAKON.Brix/src/DRAKON.Brix.Core/DRAKON.Brix.Core.csproj`
`DRAKON.Brix/src/DRAKON.Brix.Core/Assets/` and
`src/libs/DRAKON.Brix.TclBridge/DrakonRuntime.cs`
`DRAKON.Brix/THIRD-PARTY-NOTICES.txt`

**Sharp edges.**
- Both halves are required: the copy items in the project file and the
  base-directory lookup in the code. Either one alone fails at run time, and it
  fails on whichever head you did not test.
- Resolve from the base directory, never from a path relative to the project. A
  project-relative path works from the IDE and stops working the moment the
  application is published.
- Glob only what the guest actually reads at run time. Here the documentation,
  artwork and example-document folders are deliberately not copied: they exist to
  be opened by a person, and by the test suite, which copies them out one at a
  time. Copying them would cost build time and output size for nothing.
- Keep the tree in the project every head references, and keep the reader library
  free of it. A directory parameter costs one argument and buys the headless test
  path.
- A bundled third-party tree belongs in the notices file, with what it is and what
  license it arrives under.

### Generate an application's whole asset set from arithmetic on first run

**When you want this.** The application needs binary inputs to do anything at all -
audio, instruments, a folder-shaped export - and you do not want them in the
repository, downloaded, or asked of the user before the first button works. This is
[Build the binary inputs your tests need instead of committing them](BLUEPRINTS-Testing.md#build-the-binary-inputs-your-tests-need-instead-of-committing-them)
turned up to application scope: the same argument, but the writer ships inside the
product, runs on first launch beside the executable, and has to produce the layouts
real files come in rather than the smallest thing a reader will accept.

**The MVVM shape.** Not a view-model concern. A static factory in the same library
as the code that consumes the assets exposes a path per asset and one
`EnsureAssets()` the start-up path calls before anything is loaded.

**Code.**

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/MusicAssetFactory.cs
/// <para>
/// WHY GENERATE RATHER THAN SHIP FILES: this application ships no binary music assets, and a sample
/// that needs some cannot be run by anyone who does not have them. Everything here is written from
/// arithmetic, so the sample is self-contained on every platform and every checkout, and the
/// generated files are ordinary <c>.wav</c> / <c>.mid</c> / <c>.sfz</c> that can be opened in any
/// editor to see what the engine was given.
/// </para>
// ...
/// <para>
/// TWO OF THE ASSETS ARE FOLDERS, NOT FILES. A <c>.dspreset</c> points at sample files beside it,
/// and a stems export is a set of files that belong together and are named for each other. Both are
/// written as the real thing is laid out, because that is the part a game gets wrong.
/// </para>
```

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/MusicAssetFactory.cs
/// <summary>Where the generated assets live.</summary>
public static string AssetDirectory { get; } =
    Path.Combine(AppContext.BaseDirectory, "GeneratedMusic");

// ...

public static void EnsureAssets()
{
    Directory.CreateDirectory(AssetDirectory);

    // A chord that agrees with itself: C major, one layer per role.
    WriteIfMissing(StemPaths[0], () => Pad(new[] { 261.63, 329.63, 392.00 }));
    WriteIfMissing(StemPaths[1], () => Pulse(65.41, notesPerBar: 4, duty: 0.45));
    WriteIfMissing(StemPaths[2], () => Arpeggio(new[] { 523.25, 659.25, 783.99, 659.25 }));

    // ...

    EnsureInstrument();
    EnsureMidi();
    EnsureDecentSamplerInstrument();
    EnsureTempoChangeMidi();
    EnsureStemsExport();
}

private static void WriteIfMissing(string path, Func<float[]> render)
{
    if (File.Exists(path))
    {
        return;
    }

    WaveFileWriter.CreateWaveFile16(path, new BufferSampleProvider(render(), SampleRate, 2));
}
```

The two folder-shaped assets are where the recipe earns its keep. An instrument
preset is written into its own folder with the samples it points at beside it, and
the export is written under the file-naming convention a real download uses:

```csharp
// From CodeBrix.Samples/GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/MusicAssetFactory.cs
// A .dspreset is not one file: it POINTS AT sample files beside it, so the folder is as much
// part of the instrument as the XML is. Written here the same way as everything else, so the
// sample still ships no binary assets.
private static void EnsureDecentSamplerInstrument()
{
    var instrumentFolder = Path.GetDirectoryName(DecentSamplerPresetPath);
    var samplesFolder = Path.Combine(instrumentFolder, "Samples");
    Directory.CreateDirectory(samplesFolder);

    var samplePath = Path.Combine(samplesFolder, "bell.wav");
    // ...
}

private static void EnsureStemsExport()
{
    Directory.CreateDirectory(StemsExportFolder);

    var beats = StemsBeatTimes();

    WriteStemIfMissing("Vocals", () => StemsPad(beats));
    WriteStemIfMissing("Drums", () => StemsPulse(beats, frequency: 180, gain: 0.30, decay: 26));
    WriteStemIfMissing("Bass", () => StemsPulse(beats, frequency: 65.41, gain: 0.34, decay: 6));

    EnsureStemsMidi();
}

private static void WriteStemIfMissing(string stemName, Func<float[]> render)
{
    var path = Path.Combine(StemsExportFolder, $"Fake Song ({stemName}).wav");

    if (!File.Exists(path))
    {
        WaveFileWriter.CreateWaveFile16(path,
            new BufferSampleProvider(render(), StemsExportSampleRate, 2));
    }
}
```

**Where to look.**
`GameEngineMusicDemo/src/libs/GameEngineMusicDemo.Game/MusicAssetFactory.cs`
`GameEngineMusicDemo/tests/libs/GameEngineMusicDemo.Game.Tests/MusicAssetFactoryTests.cs`

**Sharp edges.**
- Write beside the executable through `AppContext.BaseDirectory`, never the working
  directory, and skip anything already there so only the first run pays. Deleting
  the folder is then the way to have it written again.
- Generate the layouts, not just the bytes: a preset with its samples folder, an
  export whose files are named for each other with its grid file beside them. The
  layout is the part an application gets wrong, and a flat folder of correct files
  proves nothing.
- Make the generated inputs disagree with the application on purpose where the
  application claims to cope - a different sample rate, a tempo that changes
  partway through - so the coping path runs on every machine rather than only on
  the one that had the real files.
- A format's own rules still apply: a note is two events, and the export step here
  sorts and closes a track without releasing anything, so a note written without
  its off event is still sounding at the end of the file and the reader reports it.
  Looping audio gets a very short fade at each end so the seam does not click.

### Bind a native driver by its bare name and resolve it yourself at run time

**When you want this.** The application talks to a native library that is
installed on the machine rather than shipped with the application - a device
driver, a vendor runtime - and it is not reliably anywhere the default probe
looks. Unlike
[Fan native packages out across the heads](BLUEPRINTS-ProjectLayoutAndPackaging.md#fan-native-packages-out-across-the-heads)
and
[Add the native assets a head would have supplied](BLUEPRINTS-Testing.md#add-the-native-assets-a-head-would-have-supplied),
there is no package to reference and nothing to copy to the output: the library
must be found where the user's own installation put it.

**The MVVM shape.** Not a view-model concern. One interop library holds the
declarations and one static loader holds the search. Every entry point calls the
loader's registration method first, so no caller has to remember to.

**Code.**

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/Interop/Ps2000Api.cs
private const string Driver = Ps2000DriverLoader.LibraryName;

/// <summary>
/// Prepares the driver for use. Call before the first P/Invoke.
/// </summary>
public static void Initialise() => Ps2000DriverLoader.EnsureRegistered();

// ...

[DllImport(Driver, EntryPoint = "ps2000_open_unit")]
public static extern short ps2000_open_unit();
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/Interop/Ps2000DriverLoader.cs
public static void EnsureRegistered()
{
    if (_registered) { return; }

    lock (SyncRoot)
    {
        if (_registered) { return; }

        NativeLibrary.SetDllImportResolver(typeof(Ps2000DriverLoader).Assembly, Resolve);
        _registered = true;
    }
}
```

```csharp
// From CodeBrix.Samples/PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/Interop/Ps2000DriverLoader.cs
private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
{
    if (!string.Equals(libraryName, LibraryName, StringComparison.OrdinalIgnoreCase))
    {
        return IntPtr.Zero;
    }

    string fileName = DriverFileName;
    foreach (string directory in GetSearchPaths())
    {
        string candidate = Path.Combine(directory, fileName);
        if (!File.Exists(candidate)) { continue; }

        //Loading by absolute path also lets the OS resolve the driver's own
        //  dependencies (picoipp.dll on Windows) from beside it, which a
        //  plain name-based load would not do.
        if (NativeLibrary.TryLoad(candidate, out IntPtr handle))
        {
            LoadedFrom = candidate;
            return handle;
        }
    }

    //Fall through to the default probe, which covers the case where the
    //  driver has been copied next to the executable, put on PATH, or -- on
    //  Linux -- registered with the system loader by its package.
    return IntPtr.Zero;
}
```

Naming the library by its bare name is what lets one assembly serve every
operating system, which is why the project targets the plain framework rather
than the Windows-flavored one:

```xml
<!-- From CodeBrix.Samples/PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/PicoScope.Brix.ScopeData.Ps2000.csproj -->
<!--
  Plain net10.0, not net10.0-windows: nothing in here is Windows-specific. The
  P/Invokes bind to the same ps2000 entry points on every operating system
  (ps2000.dll on Windows, libps2000.so on Linux), and the only per-OS code is
  where Ps2000DriverLoader looks for the driver.
-->
```

**Where to look.**
`PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/Interop/Ps2000DriverLoader.cs`
`PicoScope.Brix/src/libs/PicoScope.Brix.ScopeData.Ps2000/Interop/Ps2000Api.cs` and
`PicoScope.Brix.ScopeData.Ps2000.csproj`

**Sharp edges.**
- The resolver is registered per assembly, and it must be registered before the
  first call into that assembly's declarations. Calling it from every entry
  point, behind a flag and a lock, is cheaper than documenting the rule.
- Load by absolute path. A name-based load leaves the library's own dependencies
  to the default search, and a vendor library that sits beside its dependency in
  an unusual folder will not find it.
- Returning zero from the resolver means "I have nothing", not "fail": the
  runtime then runs its own probe, which is the normal path on a system whose
  package manager registered the library properly.
- Keep, and report, the path that was actually loaded. When the answer is wrong,
  the loaded path is the first thing anyone needs.
- A 32-bit host against a 64-bit library fails with a bad-image error rather
  than a missing-library error, and the two send you looking in different
  places.
- Do not commit the driver or bundle it. Record in the notices file that it is
  loaded at run time and never redistributed.

### Keep a third-party API inside one library with PrivateAssets and a module initializer

**When you want this.** One library is meant to be the only place a third-party
API is ever named - a device SDK, a daemon client, a vendor object model - and
you want that boundary enforced by the compiler rather than by review. A
downstream project that reaches past the library should fail to build, not merely
be frowned upon in a pull request.

**The MVVM shape.** Not a view-model concern. The library takes the package
reference with `PrivateAssets=all`, exposes its own data transfer types at the
seam and maps into them, so no third-party type crosses the boundary either.
Everything above it - the Core library, the heads, the other libraries and the
test projects - sees only the library's own surface.

**Code.**

```xml
<!-- Adapted from CodeBrix.Samples/RedisSetupTool/src/libs/RedisSetupTool.DockerManagement/RedisSetupTool.DockerManagement.csproj
     (package ids and versions elided - see the project's csproj) -->
<!-- The CodeBrix Docker library - the whole reason this library exists. PrivateAssets=all keeps
     the reference from flowing to RedisSetupTool.Core and the heads, so the boundary is
     enforced by the compiler rather than by discipline: a downstream project that names a
     CodeBrix.Docker type fails with CS0234. -->
<ItemGroup>
  <PackageReference Include="(the CodeBrix Docker package)">
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>

<!-- PrivateAssets=all also stops the RUNTIME asset flowing, which would leave every consumer
     with a missing CodeBrix.Docker.dll at load time. Re-publish just the assembly as a
     copy-to-output item so consumers get the file without getting the compile-time reference.
     The assembly still does not appear in the consumer's deps.json, which is why
     DockerAssemblyResolver hooks AssemblyLoadContext to find it at load time. -->
<Target Name="FlowDockerAssemblyToConsumers" BeforeTargets="GetCopyToOutputDirectoryItems" DependsOnTargets="ResolveReferences">
  <ItemGroup>
    <AllItemsFullPathWithTargetPath Include="@(ReferencePath-&gt;WithMetadataValue('Filename', 'CodeBrix.Docker'))">
      <TargetPath>CodeBrix.Docker.dll</TargetPath>
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </AllItemsFullPathWithTargetPath>
  </ItemGroup>
</Target>
```

Copying the file out is only half of it. The consumer's dependency manifest still
does not mention the assembly, so the default load context will not find it; the
library closes that gap itself, once, in the one place that knows where its
private copy sits.

```csharp
// From CodeBrix.Samples/RedisSetupTool/src/libs/RedisSetupTool.DockerManagement/DockerAssemblyResolver.cs
//A module initializer is the trigger, not a static constructor on DockerManager: the runtime has to
//  resolve CodeBrix.Docker while it prepares that constructor's body, which is earlier than the
//  constructor's first statement. The module initializer runs before any code in this assembly does,
//  so the hook is always in place by then.
internal static class DockerAssemblyResolver
{
    private const string DockerAssemblyName = "CodeBrix.Docker";

    private static int _registered;

    //CA2255 warns that module initializers belong in application code. This is the one case the
    //  rule cannot cover: the hook has to be installed before any code in THIS assembly runs, and
    //  only this assembly knows where its private copy of CodeBrix.Docker.dll sits.
#pragma warning disable CA2255
    [ModuleInitializer]
    internal static void EnsureRegistered()
    {
        if (Interlocked.Exchange(ref _registered, 1) == 0)
        {
            AssemblyLoadContext.Default.Resolving += ResolveDockerAssembly;
        }
    }
#pragma warning restore CA2255

    private static Assembly ResolveDockerAssembly(AssemblyLoadContext context, AssemblyName name)
    {
        if (!string.Equals(name?.Name, DockerAssemblyName, StringComparison.Ordinal))
        {
            return null;
        }

        var directory = Path.GetDirectoryName(typeof(DockerAssemblyResolver).Assembly.Location);
        if (string.IsNullOrEmpty(directory))
        {
            return null;
        }

        var candidate = Path.Combine(directory, DockerAssemblyName + ".dll");
        return File.Exists(candidate) ? context.LoadFromAssemblyPath(candidate) : null;
    }
}
```

**Where to look.**
`RedisSetupTool/src/libs/RedisSetupTool.DockerManagement/RedisSetupTool.DockerManagement.csproj`
`RedisSetupTool/src/libs/RedisSetupTool.DockerManagement/DockerAssemblyResolver.cs` and
`Models/`, `Mapping/` (the data transfer types and the mappers that keep third-party
types off the seam)

**Sharp edges.**
- The private reference stops the runtime asset flowing as well as the
  compile-time one. Without the re-publish target the application builds cleanly
  and fails at load time, which is the worst place to find out.
- The timing is the whole trick. The runtime resolves the referenced assembly
  while it prepares the body of the first method that mentions one of its types,
  which is earlier than that method's first statement - so a static constructor on
  the facade is too late and a module initializer is not.
- Register the resolving handler once and guard it, because a module initializer
  can run more than once in a process that loads the assembly into more than one
  context.
- `PrivateAssets=all` alone does not hold the boundary: if the library returns a
  third-party type from a public method, every consumer needs the reference back.
  Publish your own types at the seam and map into them.

### Ship a data corpus as content items and find it under the application base directory

**When you want this.** Your sample or tool has to have something real to open the
moment it starts - clips, documents, tables, fixtures - and you want it to work
the same way beside every head's executable and beside the test binary, with no
configuration file, no installer step and no search of the file system. This is the
copied-to-output counterpart of
[Embed an asset with an explicit logical name and load it by reflection](BLUEPRINTS-ProjectLayoutAndPackaging.md#embed-an-asset-with-an-explicit-logical-name-and-load-it-by-reflection):
embed what the code reads as a stream, copy what a person browses, a library opens
by path, or a test compares against on disk.

**The MVVM shape.** Not a view-model concern in itself. The data lives under the
Core library that every head references, as ordinary content copied to the output
folder; a static class in the library that owns the feature resolves it under
`AppContext.BaseDirectory` and answers "is it there" with a path or null. The view
model asks that class once at startup and turns a null into a sentence a person can
act on.

**Code.**

```xml
<!-- From CodeBrix.Samples/SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.Core/SimpleCbxVideoPlayer.Core.csproj -->
<!--
  The video corpus and the colour lookup tables the application plays and grades with. They are DATA:
  nothing here is compiled, and every head that references this project gets its own copy beside its
  executable, which is where SampleAssets looks for them.
-->
<ItemGroup>
  <None Include="Assets\**\*" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/SimpleCbxVideoPlayer/src/libs/SimpleCbxVideoPlayer.SkiaVideo/Assets/SampleAssets.cs
/// <remarks>
/// The corpus is ordinary data that the Core project copies to the output folder, so it sits beside the
/// running executable under <c>Assets/</c> - no configuration file, no search of the file system, and the
/// same layout on every head and in the test run. A copy of the application that has lost its
/// <c>Assets</c> folder finds nothing, which is why the user interface says so rather than failing.
/// </remarks>
public static class SampleAssets
{
    /// <summary>The video corpus, relative to the folder the application runs from.</summary>
    public const string AuthoringRelativePath = "Assets/authoring";

    /// <summary>Finds the corpus beside the running application.</summary>
    /// <returns>The folder the corpus sits in, or null when the application carries no corpus.</returns>
    public static string FindAssetsRoot() => FindAssetsRoot(AppContext.BaseDirectory);

    /// <summary>Finds the corpus beside a folder of your choosing.</summary>
    public static string FindAssetsRoot(string applicationFolder)
    {
        if (string.IsNullOrWhiteSpace(applicationFolder)) { return null; }

        return Directory.Exists(GetAuthoringFolder(applicationFolder)) ? applicationFolder : null;
    }
    // ...
}
```

```csharp
// From CodeBrix.Samples/SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.Core/ViewModels/MainViewModel.cs
private void LoadCorpus()
{
    var assetsRoot = SampleAssets.FindAssetsRoot();

    if (assetsRoot == null)
    {
        CorpusText = "The sample corpus was not found. The application plays the files under "
            + $"{SampleAssets.AuthoringRelativePath}, which is copied to the folder it runs from, so a "
            + "copy that has lost that folder has nothing to open.";
        return;
    }
    // ...
}
```

The test project links the same folder rather than keeping a second copy, so the
tests read the files the application reads, at the same relative paths:

```xml
<!-- From CodeBrix.Samples/SimpleCbxVideoPlayer/tests/libs/SimpleCbxVideoPlayer.SkiaVideo.Tests/SimpleCbxVideoPlayer.SkiaVideo.Tests.csproj -->
<!--
  The corpus the application ships, laid out beside the test binary exactly as it is laid out beside a
  head's executable, so the catalogue tests read the same files at the same relative paths the running
  application does. It is linked rather than copied: there is one corpus in this application.
-->
<ItemGroup>
  <None Include="..\..\..\src\SimpleCbxVideoPlayer.Core\Assets\**\*"
        Link="Assets\%(RecursiveDir)%(Filename)%(Extension)"
        CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

**Where to look.**
`SimpleCbxVideoPlayer/src/SimpleCbxVideoPlayer.Core/SimpleCbxVideoPlayer.Core.csproj`
`SimpleCbxVideoPlayer/src/libs/SimpleCbxVideoPlayer.SkiaVideo/Assets/SampleAssets.cs` and
`tests/libs/SimpleCbxVideoPlayer.SkiaVideo.Tests/SimpleCbxVideoPlayer.SkiaVideo.Tests.csproj`
`SimpleCbxVideoPlayer/tests/libs/SimpleCbxVideoPlayer.SkiaVideo.Tests/SampleAssetsTests.cs`

**Sharp edges.**
- Resolve from `AppContext.BaseDirectory`, never from the current directory: a
  shortcut, a debugger or a test runner can start a process anywhere.
- Put the content on the library every head references and let the copy fan out;
  one glob item then feeds six output folders, and a new head needs no build edit.
- Keep the relative path as a constant and build every other path from it, so the
  message shown when nothing is found names the same path the code looked in.
- Make "the folder is missing" a normal answer - a null and a sentence - rather
  than an exception. A copy that lost its content should still start and say what
  is wrong.
- Link the folder into the test project with the same relative layout rather than
  copying it; two corpora drift, and the drift shows up as a test that passes
  against files the application never sees.
- Preserve-newest keeps incremental builds cheap; a corpus of any size copied on
  every build is felt immediately.

### Depend on a native runtime the user installs instead of shipping a package

**When you want this.** A library you reference needs a native runtime on some
operating systems and not on others, and redistributing it is the wrong answer -
because of its license, its size, or because the machine usually has it already.
This is the opposite arrangement from
[Fan native packages out across the heads](BLUEPRINTS-ProjectLayoutAndPackaging.md#fan-native-packages-out-across-the-heads):
nothing in the build declares the dependency at all.

**The MVVM shape.** Not a view-model concern on the packaging side. The half that
is: a missing runtime has to arrive as a state the view model reports, not an
exception at startup, because the application must still open, still list what it
can, and still say what is wrong on a machine that does not have it.

**Code.**

```xml
<!-- Adapted from CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/WebcamViewer.Core.csproj
     (package ids and versions elided - see the project's csproj) -->
<ItemGroup>
  <!-- ... CodeBrix.Platform, the Open Sans font package, the generic host and console logging ... -->

  <!-- SKXamlCanvas - the SkiaSharp video surface hosted in CodeBrix.Platform XAML -->
  <!-- ... the CodeBrix.Platform SkiaSharp Views package ... -->

  <!-- Webcam capture: device enumeration, live BGRA frames, in-memory photos -->
  <!-- ... the CodeBrix.Webcam package. This is the whole declaration: no native payload
       package here or on any head, because on Linux and macOS the capture session opens
       through a native media runtime the user installs. -->

  <!-- PNG encoding for the frame-photo feature -->
  <!-- ... the CodeBrix.Imaging package ... -->
</ItemGroup>
```

Because the build says nothing about it, the notices file and the README are the
only places the dependency is written down, and the notices entry has to be
explicit that it is not redistributed:

```text
// From CodeBrix.Samples/WebcamViewer/THIRD-PARTY-NOTICES.txt
------------------------------------------------------------------------
Native media runtime (not bundled; installed by the user)
------------------------------------------------------------------------
On Linux and macOS the webcam library opens a capture session through the
native libvlc runtime, which is published separately by VideoLAN under the
GNU Lesser General Public License, version 2.1 or later. It is NOT bundled
with, or redistributed by, this repository: on Linux it is installed with the
system package manager, and on macOS it is supplied by an installed VLC media
player application. On Windows no native runtime is used at all - capture
goes through the operating system's own media engine. "VideoLAN", "VLC" and
"LibVLC" are trademarks of VideoLAN; this sample is not affiliated with or
endorsed by VideoLAN.
```

The application-side half is one catch, placed where the runtime is first needed:

```csharp
// From CodeBrix.Samples/WebcamViewer/src/WebcamViewer.Core/ViewModels/MainViewModel.cs
        _session = new WebcamSession(camera.Device);
        // ...
    }
    catch (Exception e)
    {
        StatusText = $"Could not start '{camera?.Device.FriendlyName}': {e.Message}";
    }
```

**Where to look.**
`WebcamViewer/src/WebcamViewer.Core/WebcamViewer.Core.csproj`
`WebcamViewer/THIRD-PARTY-NOTICES.txt` and
`WebcamViewer/README.md` (the prerequisites list, per operating system)

**Sharp edges.**
- Separate what works without the runtime from what does not, and say so. Here
  device enumeration works everywhere, so the dropdown fills on a machine with no
  runtime and the failure only appears when a session is started - which is
  exactly where the catch has to be.
- The notices entry says who publishes the runtime, under what license, that it is
  not redistributed, how it is obtained on each operating system, and that the
  names are trademarks the sample is not affiliated with. Leaving any of those out
  is what makes the file useless later.
- The README carries the install instruction per operating system, including the
  one where there is nothing to install. A reader on the platform that needs
  nothing should be told that explicitly rather than left to infer it.
- Operating-system consent is the same kind of undeclared dependency: a head run
  from the command line inherits the terminal's camera permission, while the same
  head packaged as a bundle has to declare its own usage descriptions or be
  refused. Neither is visible in any project file.

