# CodeBrix.Samples

This repository holds complete, runnable reference applications for the CodeBrix
family of .NET libraries. Each one is a real application rather than a snippet:
it opens files, talks to networks, draws, renders, encodes or publishes, and it
was built to show how the libraries are meant to be consumed from a
CodeBrix.Platform application. If you are building your own application and want
to see how something is actually done, open the application that does it and
read the code beside its README.

Every application follows the same house style. One shared view model and one
shared XAML UI drive every head; each head is a thin project that supplies only
its platform plumbing and references one runtime package. The six
CodeBrix.Platform heads are present in project form - LinuxX11, LinuxWayland,
LinuxFrameBuffer, MacOS, Win32Skia and WinWpfSkia - with two exceptions worth
knowing about: three applications (JustBetweenUs, PainDiagram and
WikipediaPublisher) additionally carry native WinUI 3 and WPF heads that reuse
the same view model without the CodeBrix.Platform UI stack, and JustBetweenUs
adds a .NET MAUI head, the only mobile head in the repository, while
CodeBrixVideoTool, GitHubIssueFinder and InannaRosette each build four of the
six. Libraries are consumed as packages, never as source references, so each
application folder is self-contained and can be opened and built on its own.

Everything in this repository is licensed under the Apache License, Version 2.0.
This is the repository for the reference applications whose libraries are
permissively licensed. Two sibling repositories hold the others:
CodeBrix.Samples.Gpl2 (classic games) and CodeBrix.Samples.Gpl3 (applications
whose libraries land on GPL-3.0).

The documentation is organized in one predictable way. Every application folder
has a `README.md`, the detailed guide to that application - what it does, how it
is laid out, what it uses, and what is worth studying in it - and a
`THIRD-PARTY-NOTICES.txt`, its attribution record. At the root,
[BLUEPRINTS-Index.md](BLUEPRINTS-Index.md) indexes the blueprint files, which
collect how-tos mined from all of them.

## The applications

| Application | What it is | Headline CodeBrix libraries |
| --- | --- | --- |
| [CodeBrixVideoTool](CodeBrixVideoTool/README.md) | Desktop video converter and player for AV1 media, with chapter and caption drop-downs, a resolution and quality ladder, and long conversions run with live progress and cancellation | CodeBrix.Platform.VideoPlayer add-in, CodeBrix.VideoPlayback, CodeBrix.VideoPlayback.Authoring, CodeBrix.VideoProcessing |
| [DRAKON.Brix](DRAKON.Brix/README.md) | The DRAKON Editor, a visual algorithm-language editor whose entire Tcl/Tk program runs unmodified on a managed Tcl interpreter and Tk toolkit inside one XAML page, generating source code in many target languages and exporting diagrams to PDF | CodeBrix.Platform.TclTk, CodeBrix.Platform.TclTk.Extras, CodeBrix.Platform.TkCanvas |
| [GameEngineMusicDemo](GameEngineMusicDemo/README.md) | Control surface for the game engine's music system: buses, fades and crossfades, bar-quantized transitions, adaptive stems, MIDI through SFZ and Decent Sampler instruments, ducking, stingers and a global pause, with every asset generated on first run | CodeBrix.Platform.GameEngine |
| [GitHubIssueFinder](GitHubIssueFinder/README.md) | Finds the open issues and pull requests nobody has picked up across a GitHub user's or organization's public repositories, grouped by repository, paced to the anonymous API allowance with every wait shown on screen and five switchable color schemes | the CodeBrix.Platform FlexPanel and AppSettings add-ins |
| [InannaRosette](InannaRosette/README.md) | Forty-card oracle deck of Sumer and Akkad laid on a nine-station rosette spread by dragging cards onto its petals, read as written prose and composed into a print-quality PDF whose card art, ornaments and embedded fonts are all vector | CodeBrix.PdfDocuments, CodeBrix.Platform |
| [JustBetweenUs](JustBetweenUs/README.md) | Text-encryption utility (AES, Triple DES, Twofish) and the repository's "one view model, many heads" reference, the only application that also runs on mobile | CodeBrix.Cryptography, CodeBrix.SkiaSvg, CodeBrix.Platform with its WinUI, WPF and Mobile support |
| [KenneyAssetBrowser](KenneyAssetBrowser/README.md) | Browser for downloaded kenney.nl game-asset packs: reads each zip without extracting it and previews images, SVG art, font specimens, Tiled maps, 3D models and audio | CodeBrix.Compression, CodeBrix.Imaging, CodeBrix.SkiaSvg; the CodeBrix.Platform Graphics3DGL, AudioPlayer, FlexPanel and AppSettings add-ins |
| [MediaPlayerDemo](MediaPlayerDemo/README.md) | One-page media player - an address box, a stretch picker and the element's own transport controls - and the smallest six-head skeleton here | CodeBrix.Platform.MediaPlayer add-in |
| [NotionDocumentCreator](NotionDocumentCreator/README.md) | Turns selected pages from a Notion workspace into a single print-ready, book-designed PDF | CodeBrix.NotionApi, CodeBrix.PdfDocCreate, CodeBrix.Imaging, CodeBrix.VideoProcessing |
| [PainDiagram](PainDiagram/README.md) | Interactive pain- and symptom-mapping over a medical body map, drawn on three translucent highlighter layers and exported as a PNG | CodeBrix.Imaging.Drawing |
| [PalmVisualizer](PalmVisualizer/README.md) | Webcam toy whose shader-driven plasma and starfield visual chases the open palms a hand-tracking pipeline finds in the live camera feed | CodeBrix.Platform.GameEngine, CodeBrix.Webcam, CodeBrix.VideoProcessing.OpenCV5 |
| [PdfSideBySide](PdfSideBySide/README.md) | Opens two PDF documents side by side and steps, zooms and nudges them together or independently, so two editions can be compared page by page | CodeBrix.PdfRasterizer, CodeBrix.Imaging |
| [PicoScope.Brix](PicoScope.Brix/README.md) | One-page oscilloscope front end that streams live traces from a PicoScope 2204A into a chart, falls back to a simulated device that enforces the real instrument's limits, and drives the ps2000 driver from one interop library on Linux and Windows | CodeBrix.Plotter through the CodeBrix.Platform PlotterView add-in |
| [Pinta.Brix](Pinta.Brix/README.md) | Layered raster painting and image editor - tools, selections, adjustments and effects with live preview, and a scrubbable history; a port of Pinta | CodeBrix.Imaging, CodeBrix.SkiaSvg, CodeBrix.PolygonTools; the CodeBrix.Platform AppSettings and TextLayout add-ins |
| [PolyHavenBrowser](PolyHavenBrowser/README.md) | Catalog browser for Poly Haven's CC0 3D model library, with a lazily filled card grid, real-progress downloads, a live on-screen 3D preview and a generated one-page PDF | CodeBrix.Platform.Graphics3DGL add-in, CodeBrix.Imaging, CodeBrix.PdfDocuments; the FlexPanel add-in |
| [PolyHavenBrowser_viewer_only](PolyHavenBrowser_viewer_only/README.md) | Three curated Poly Haven samples - a PBR texture, an HDRI panorama and a glTF model - rendered off screen through a graphics backend the user can swap while the application runs, and composited onto one Skia canvas | CodeBrix.Platform.Graphics3DGL add-in, CodeBrix.Imaging |
| [RedisSetupTool](RedisSetupTool/README.md) | Desktop control panel for Redis on a local Docker daemon: thirteen topologies stood up, verified and torn down, every container on the daemon managed, images, networks and volumes handled, and a real interactive shell opened inside a container | CodeBrix.Docker, CodeBrix.Redis; the CodeBrix.Platform TerminalView add-in |
| [SimpleCbxVideoPlayer](SimpleCbxVideoPlayer/README.md) | Plays its bundled AV1 clips in Matroska, WebM and both CodeBrix Video container flavors straight through the playback library and grades the picture with a chain of color lookup tables it can bake back out as one file | CodeBrix.VideoPlayback, CodeBrix.VideoPlayback.Skia, CodeBrix.VideoPlayback.Dav1d, CodeBrix.Audio.Opus; the CodeBrix.Platform Graphics3DGL add-in |
| [WebcamPainter](WebcamPainter/README.md) | Hand-gesture painting: grab a still from the webcam, then spread highlighter ink across it by moving an open palm in front of the camera | CodeBrix.Webcam, CodeBrix.Imaging.Drawing, CodeBrix.VideoProcessing.OpenCV5 |
| [WebcamViewer](WebcamViewer/README.md) | Live webcam viewer on a Skia surface with a camera picker, an audio-monitor switch and a Photo button that writes the current frame as a PNG, and the smallest webcam skeleton here | CodeBrix.Webcam, CodeBrix.Imaging |
| [WikipediaPublisher](WikipediaPublisher/README.md) | Turns a Wikipedia article, chosen in an embedded WebView, into a book-designed print-ready PDF | CodeBrix.MarkupParse, CodeBrix.Imaging, CodeBrix.PdfDocCreate; the CodeBrix.Platform.WebView add-in |

## Blueprints

[BLUEPRINTS-Index.md](BLUEPRINTS-Index.md) is the entry point to a set of
how-tos for building CodeBrix.Platform applications, mined from the
applications in this repository and split into one file per topic.
Each blueprint says when you want it, gives the code, and names the application
and the files it came from, so you can open the real thing and read the rest of
it. The blueprints teach the shape the applications use: a view model derived
from `SimpleViewModel` owns the screen's state and behavior and exposes it as
bound properties and `SimpleCommand` commands; code-behind stays thin and
forwards only what a view alone can do; anything the view model needs from the
platform - a file dialog, a canvas to invalidate, the clipboard - arrives
through a small bridge interface the page implements; services live behind
interfaces and are resolved through `SimpleServiceResolver`; and work that takes
time happens off the UI thread and marshals its results back.

## Building and testing

The .NET 10 SDK is the only universal prerequisite. Open the solution in the
application's folder: most applications have a single `.slnx` there. The
exceptions are JustBetweenUs, which has three OS-specific `.sln` files
(`JustBetweenUs.Windows.sln`, `JustBetweenUs.Linux.sln`,
`JustBetweenUs.MacOS.sln`) instead of a `.slnx`, and PainDiagram and
WikipediaPublisher, which each have a cross-platform `.slnx` plus a
`.Windows.slnx` that adds the native heads.

Windows-targeting heads compile elsewhere but run only on Windows: they target
`net10.0-windows` and set `EnableWindowsTargeting`, so a solution containing
them still restores and builds on Linux and macOS.

Some applications need more than the SDK, and their own READMEs say exactly
what. In summary: the .NET MAUI workloads (JustBetweenUs); the WPE WebKit system
packages for the Linux WebView (WikipediaPublisher); a webcam (WebcamPainter,
PalmVisualizer, WebcamViewer); network access for the applications that download content; a
Notion integration token (NotionDocumentCreator); ffmpeg and ffprobe on the
host for probing and conversion (CodeBrixVideoTool, and optionally
NotionDocumentCreator); a reachable Docker daemon (RedisSetupTool); and Pico's
ps2000 driver with a PicoScope 2204A attached for live traces, though
PicoScope.Brix runs on its simulated device without either.

Test projects use the Microsoft.Testing.Platform runner, selected by
`global.json` where an application has one and by properties in the test csproj
otherwise. Test assemblies are self-executing binaries, and in practice a plain
`dotnet test` can report that zero tests ran. When it does, build the test
project and run the executable it produces directly:

```text
dotnet build tests/libs/<Project>.Tests/<Project>.Tests.csproj -c Release
./tests/libs/<Project>.Tests/bin/Release/net10.0/<Project>.Tests
```

Each application's README gives the working form for that application, along
with what its individual test projects need.

## Third-party notices

Each application folder carries its own `THIRD-PARTY-NOTICES.txt`, and that file
is the authoritative record for that application: the bundled fonts, models and
sample assets it ships, the code it ported or adapted, and the content it
fetches at run time, together with their licenses and upstream locations. The
root [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) is a pointer to them.
Third-party code arrives as packages, and each package carries its own notices.

## License

Everything in this repository is licensed under the Apache License, Version 2.0.
See [LICENSE](LICENSE) for the full text.

Copyright (c) 2026 Jeremy Ellis and contributors
