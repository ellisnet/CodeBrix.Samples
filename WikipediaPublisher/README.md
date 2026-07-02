# WikipediaPublisher

**WikipediaPublisher** turns any Wikipedia article into a **book-designed, print-ready
PDF** — a cover page with the article's hero image, a real table of contents with page
numbers and dot leaders, classic book typography (justified EB Garamond with first-line
indents and a raised initial), framed and numbered figures, booktabs-style tables, pull
quotes, running heads, folios, PDF outline bookmarks, and a colophon with proper
CC BY-SA attribution.

It is also a **desktop-only companion reference application for the CodeBrix.Platform
NuGet packages**, structured like
[JustBetweenUs](https://github.com/ellisnet/JustBetweenUs): one shared view model and
one shared Skia XAML UI drive every desktop head.

The heavy lifting is done by three CodeBrix libraries:

| Library | Role here |
| --- | --- |
| `CodeBrix.MarkupParse.MitLicenseForever` | Parses the article HTML into a DOM; the article parser walks it into structured content |
| `CodeBrix.PdfDocCreate.MitLicenseForever` | The high-level document object model the book is composed with (styles, sections, tables, TOC fields, outlines) |
| `CodeBrix.Imaging.ApacheLicenseForever` | Print-resolution image processing (resize, format normalization) |

---

## What the application does

1. **Search** — type search terms and click *Search*.
   * On heads with an embedded **WebView** (Win32, Skia-on-WPF, macOS, WinUI, WPF),
     the real Wikipedia search page loads in the browser pane; browse to the article
     you want.
   * On the Linux heads (no WebView yet), the results of a MediaWiki API search are
     shown in a native list; select an article (or paste any article URL directly).
2. **Choose** the output folder and a page (trim) size next to the *Publish* button —
   8″ × 10″ coffee table (default), 6″ × 9″ trade book, US Letter, or A4.
3. **Publish** — the article is fetched, parsed, its images downloaded at print
   resolution (politely rate-limited), and laid out as a book. Progress is reported
   live; the finished PDF lands in the chosen folder.

### The book design

The `WikipediaPublisher.RenderArticle` library (the analog of JustBetweenUs's
`.Encryption` library) does the full pipeline: fetch → parse → image pipeline →
book composition → PDF render. Design details worth knowing about:

* **Embedded OFL fonts** — EB Garamond (body/display serif) and Source Sans 3
  (captions, labels, tables), shipped as embedded resources and registered through
  the CodeBrix.PdfDocuments `EmbeddedFontResolver`, so the PDF looks identical on
  every OS. The OFL license texts ship alongside the fonts.
* **Front matter** — a cover page (kicker, title, short-description subtitle, hero
  image from the article's lead image, imprint), and a table of contents built with
  real MigraDoc bookmarks, `PageRefField`s and dot-leader tab stops — entries are
  clickable and page numbers are real.
* **Body typography** — justified, exact leading, no inter-paragraph gaps,
  first-line indents on continuation paragraphs, a raised accent initial opening the
  lead, centered small-cap section openers with accent rules, `KeepWithNext`
  everywhere it matters.
* **Figures & tables** — images sized by aspect ratio (with page-height caps),
  hairline keylines on photographs, numbered captions (`FIG. 7`), and
  booktabs-styled tables (horizontal rules only, repeating header rows).
* **Fidelity with judgment** — citation markers, edit links, navboxes, hatnotes and
  other web chrome are stripped; galleries, definition lists, poems/quoteboxes and
  simple wikitables are rendered; characters outside the embedded fonts' coverage
  (e.g. inline Cuneiform glyphs) are removed rather than printed as tofu boxes, and
  every skip is reported in the render warnings.
* **Finish** — PDF outline bookmarks at correct pages (two levels), document
  metadata, and a colophon page with the source URL, retrieval date, CC BY-SA
  attribution and font credits.

---

## Solutions — what to open where

| Solution | Use on | Contains |
| --- | --- | --- |
| `WikipediaPublisher.slnx` | Linux, macOS, Windows | RenderArticle + tests + all six CodeBrixPlatform (Skia) heads |
| `WikipediaPublisher.Windows.slnx` | Windows | Everything above **plus** the native WinUI 3 and WPF heads |

Build with the .NET 10 SDK. The Windows-targeting Skia heads (`WpfSkia`) compile on
Linux/macOS via `EnableWindowsTargeting` (they only *run* on Windows).

```
dotnet build WikipediaPublisher.slnx
dotnet test  WikipediaPublisher.slnx
dotnet run --project CodeBrixPlatform/WikipediaPublisher.LinuxX11
```

> The test suite includes offline tests (parser + book composition against an embedded
> HTML fixture) and live tests that fetch the
> [Cuneiform](https://en.wikipedia.org/wiki/Cuneiform) article from Wikipedia —
> the same article used by the foundational `CreateTestPdfFromOnlineArticle` test in
> CodeBrix.PdfDocuments.

## The application heads

**CodeBrixPlatform (Skia-based) heads** — one shared XAML UI (`WikipediaPublisher.UI`)
runs on every platform via the `CodeBrix.Platform.*` framework:

| Project | Platform / windowing | WebView |
| --- | --- | --- |
| `WikipediaPublisher.Windows` | Windows, native Win32 window | ✔ (Edge WebView2, built into the runtime) |
| `WikipediaPublisher.WpfSkia` | Windows, Skia hosted in WPF | ✔ (Edge WebView2, built into the runtime) |
| `WikipediaPublisher.LinuxX11` | Linux desktop, X11 | ✖ (native search list fallback) |
| `WikipediaPublisher.LinuxWayland` | Linux desktop, native Wayland | ✖ (native search list fallback) |
| `WikipediaPublisher.LinuxFrameBuffer` | Linux framebuffer (kiosk/embedded) | ✖ (native search list fallback) |
| `WikipediaPublisher.MacOs` | macOS | ✔ (WKWebView, built into the runtime) |

**Native (non-Skia) heads** — same shared `MainViewModel`, native UI stacks, WebView2
in XAML:

| Project | UI stack | CodeBrix packages used |
| --- | --- | --- |
| `WikipediaPublisher.WinUi` | WinUI 3 (Windows App SDK) | `CodeBrix.Platform.WinUI.ApacheLicenseForever` |
| `WikipediaPublisher.Wpf` | WPF + `Microsoft.Web.WebView2` | `CodeBrix.Platform.WPF.ApacheLicenseForever` |

Each head sets `AppCapabilities.HasWebView` at startup; the shared page creates the
`WebView2` control in code only when the head supports it, and shows the native
search-results pane otherwise. All heads share the **CodeBrix "Simple" MVVM toolkit**
(`SimpleViewModel`, `SimpleCommand`, `SimpleServiceResolver`, …): one `MainViewModel`
drives all eight heads.

## Repository layout

```
WikipediaPublisher.slnx              Cross-platform solution (open anywhere)
WikipediaPublisher.Windows.slnx      Windows-only solution (adds WinUI + WPF heads)
│
├─ CodeBrixPlatform/                 The Skia-based (CodeBrix.Platform UI) applications
│   ├─ WikipediaPublisher.UI/        Shared XAML UI (shared project)
│   ├─ WikipediaPublisher.Core/      Shared app library + CodeBrix.Platform packages
│   └─ WikipediaPublisher.…/         The six platform heads (see above)
│
├─ WikipediaPublisher.WinUi/         Native WinUI 3 head (Windows solution only)
├─ WikipediaPublisher.Wpf/           Native WPF head (Windows solution only)
│
├─ Shared/                           File-linked view models, helpers, test fixture
├─ WikipediaPublisher.RenderArticle/ The article→book PDF pipeline library
│   └─ Fonts/                        Embedded OFL fonts (EB Garamond, Source Sans 3)
└─ Tests/
    └─ WikipediaPublisher.RenderArticle.Tests/   xUnit v3 + SilverAssertions
```

## License

Licensed under the **Apache License, Version 2.0** — see [LICENSE](LICENSE).

Wikipedia content rendered by this application is available under the
[Creative Commons Attribution-ShareAlike license](https://creativecommons.org/licenses/by-sa/4.0/);
generated PDFs carry the attribution required by that license. The embedded EB Garamond
and Source Sans 3 fonts are used under the SIL Open Font License 1.1.
