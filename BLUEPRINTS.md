# CodeBrix.Samples Blueprints

This file is a set of how-tos for building CodeBrix.Platform applications,
mined from the applications in this repository. Each blueprint says when you
want it, gives the code, and names the application and the files it came from,
so you can open the real thing and read the rest of it. Nothing here is
invented: every code block comes from a file in this repository.

The blueprints are written in the shape the applications use. A view model
derived from `SimpleViewModel` owns the screen's state and behavior and
exposes it as bound properties and `SimpleCommand` commands; code-behind stays
thin and forwards what only a view can do; anything the view model needs from
the platform - a file dialog, a canvas to invalidate, the clipboard - arrives
through a small bridge interface the page implements or a delegate the page
sets; services live behind interfaces and are resolved through
`SimpleServiceResolver`; and work that takes time happens off the UI thread and
marshals back. A code block introduced by a `// From` comment is verbatim from
the file it names. A block introduced by `// Adapted from` was recast - trimmed,
or rewritten into that shape - and the file it names is the original.

Packages are referred to by library or add-in name rather than by package
identifier or version. The application's own project file is the source of
truth for the exact package it references.

## Contents

- [Application structure and startup](#application-structure-and-startup)
  - [Start each head from a Program Main and pick the platform backend](#start-each-head-from-a-program-main-and-pick-the-platform-backend)
  - [Bootstrap the application in the App constructor](#bootstrap-the-application-in-the-app-constructor)
  - [Create the main window and navigate to the first page](#create-the-main-window-and-navigate-to-the-first-page)
  - [Supply a generic host builder to SimpleServiceResolver](#supply-a-generic-host-builder-to-simpleserviceresolver)
  - [Register library services with one AddXxx extension method](#register-library-services-with-one-addxxx-extension-method)
  - [Turn on console logging only in Debug builds](#turn-on-console-logging-only-in-debug-builds)
  - [Set a bundled font as the default text font and register script fallbacks](#set-a-bundled-font-as-the-default-text-font-and-register-script-fallbacks)
  - [Enable a picker and the software keyboard on the Linux framebuffer head](#enable-a-picker-and-the-software-keyboard-on-the-linux-framebuffer-head)
  - [Force the software render surface on the WinWpfSkia head](#force-the-software-render-surface-on-the-winwpfskia-head)
  - [Keep Main synchronous and STA so an embedded WebView can start](#keep-main-synchronous-and-sta-so-an-embedded-webview-can-start)
  - [Turn on extra media codecs once at startup](#turn-on-extra-media-codecs-once-at-startup)
  - [Run one view model on Skia heads and on native WinUI 3 WPF and MAUI heads](#run-one-view-model-on-skia-heads-and-on-native-winui-3-wpf-and-maui-heads)
  - [Detect which platform head is running without referencing it](#detect-which-platform-head-is-running-without-referencing-it)
- [View models, commands and threading](#view-models-commands-and-threading)
  - [Write bound properties and commands the family way](#write-bound-properties-and-commands-the-family-way)
  - [Refresh CanExecute when the gating state is not a bound property](#refresh-canexecute-when-the-gating-state-is-not-a-bound-property)
  - [Refresh command enablement in one pass from a headless command model](#refresh-command-enablement-in-one-pass-from-a-headless-command-model)
  - [Give each grid cell its own command and lazily loaded thumbnail](#give-each-grid-cell-its-own-command-and-lazily-loaded-thumbnail)
  - [Guard a view model constructor for the XAML designer](#guard-a-view-model-constructor-for-the-xaml-designer)
  - [Kick off async startup loading from the view model constructor](#kick-off-async-startup-loading-from-the-view-model-constructor)
  - [Load documents named on the command line during startup](#load-documents-named-on-the-command-line-during-startup)
  - [Set bound properties from a background thread with InvokeOnMainThread](#set-bound-properties-from-a-background-thread-with-invokeonmainthread)
  - [Hand results from a capture thread through a worker to the UI thread](#hand-results-from-a-capture-thread-through-a-worker-to-the-ui-thread)
  - [Run a long job from a command with progress cancellation and a busy flag](#run-a-long-job-from-a-command-with-progress-cancellation-and-a-busy-flag)
  - [Report progress across stages when only some of them know a percentage](#report-progress-across-stages-when-only-some-of-them-know-a-percentage)
  - [Snapshot view model state before a long running command](#snapshot-view-model-state-before-a-long-running-command)
  - [Dispose a view model its commands and its bridge delegates](#dispose-a-view-model-its-commands-and-its-bridge-delegates)
  - [Run one render per pane with latest request wins cancellation](#run-one-render-per-pane-with-latest-request-wins-cancellation)
  - [Ignore a stale async result when the selection moved on](#ignore-a-stale-async-result-when-the-selection-moved-on)
  - [Debounce a search box before rebuilding a filtered list](#debounce-a-search-box-before-rebuilding-a-filtered-list)
  - [Fill a grid lazily as it scrolls](#fill-a-grid-lazily-as-it-scrolls)
  - [Show and hide panes with computed Visibility properties](#show-and-hide-panes-with-computed-visibility-properties)
  - [Load a tree lazily as the user expands it](#load-a-tree-lazily-as-the-user-expands-it)
  - [Confirm and inform from the view model with SimpleViewModel dialogs](#confirm-and-inform-from-the-view-model-with-simpleviewmodel-dialogs)
  - [Prompt before discarding unsaved work](#prompt-before-discarding-unsaved-work)
  - [Gate an action behind a chosen folder and explain the gate with a dialog](#gate-an-action-behind-a-chosen-folder-and-explain-the-gate-with-a-dialog)
  - [Report a failure as status text instead of throwing](#report-a-failure-as-status-text-instead-of-throwing)
  - [Report a domain rule violation as a typed exception the view model can catch](#report-a-domain-rule-violation-as-a-typed-exception-the-view-model-can-catch)
  - [Compose a page from a parent view model and child view models](#compose-a-page-from-a-parent-view-model-and-child-view-models)
  - [Notify a value typed bindable property by hand](#notify-a-value-typed-bindable-property-by-hand)
  - [Bind a picker to enum values with or without friendly labels](#bind-a-picker-to-enum-values-with-or-without-friendly-labels)
  - [Stop a two way bound selection from commanding the control back](#stop-a-two-way-bound-selection-from-commanding-the-control-back)
  - [Alert and revert when the user picks an unsupported option](#alert-and-revert-when-the-user-picks-an-unsupported-option)
  - [Offer only the choices that make sense for the current selection](#offer-only-the-choices-that-make-sense-for-the-current-selection)
  - [Settle an operation in a plan before running any of it](#settle-an-operation-in-a-plan-before-running-any-of-it)
  - [Report the host operating system from the view model](#report-the-host-operating-system-from-the-view-model)
  - [Cache rendered results with a bounded most recently used cache](#cache-rendered-results-with-a-bounded-most-recently-used-cache)
  - [Signal a non property model change to the view with a version counter](#signal-a-non-property-model-change-to-the-view-with-a-version-counter)
  - [Do blocking work in a service behind Task Run](#do-blocking-work-in-a-service-behind-task-run)
  - [Load an asset off the UI thread and resolve its side files from the same container](#load-an-asset-off-the-ui-thread-and-resolve-its-side-files-from-the-same-container)
  - [Pre warm a rendering backend off the UI thread](#pre-warm-a-rendering-backend-off-the-ui-thread)
  - [Coalesce repaints and drop backlogged pointer frames](#coalesce-repaints-and-drop-backlogged-pointer-frames)
  - [Run a sensor pipeline on a worker thread with latest frame wins](#run-a-sensor-pipeline-on-a-worker-thread-with-latest-frame-wins)
  - [Survive a native runtime tearing down while a frame is in flight](#survive-a-native-runtime-tearing-down-while-a-frame-is-in-flight)
  - [Publish a small immutable result type from a background pipeline](#publish-a-small-immutable-result-type-from-a-background-pipeline)
  - [Capture a still and start a second pipeline from a command](#capture-a-still-and-start-a-second-pipeline-from-a-command)
  - [Run an effect on worker threads with a live preview](#run-an-effect-on-worker-threads-with-a-live-preview)
  - [Drive an undo history from a list and travel to a clicked point](#drive-an-undo-history-from-a-list-and-travel-to-a-clicked-point)
  - [Bind a tab per open document and keep both directions in sync](#bind-a-tab-per-open-document-and-keep-both-directions-in-sync)
  - [Show selection state in button captions from computed properties](#show-selection-state-in-button-captions-from-computed-properties)
- [Bridging platform services into the view model](#bridging-platform-services-into-the-view-model)
  - [Give the view model a XamlRoot so its dialogs can show](#give-the-view-model-a-xamlroot-so-its-dialogs-can-show)
  - [Save a file through a native dialog from the view model](#save-a-file-through-a-native-dialog-from-the-view-model)
  - [Pick a file to open through a native dialog from the view model](#pick-a-file-to-open-through-a-native-dialog-from-the-view-model)
  - [Clean up the path a file picker returns](#clean-up-the-path-a-file-picker-returns)
  - [Suppress a native save dialog overwrite prompt so the view model owns confirmation](#suppress-a-native-save-dialog-overwrite-prompt-so-the-view-model-owns-confirmation)
  - [Let the page invalidate a canvas through a bridge interface](#let-the-page-invalidate-a-canvas-through-a-bridge-interface)
  - [Copy text to the clipboard from a command through a bridge interface](#copy-text-to-the-clipboard-from-a-command-through-a-bridge-interface)
  - [Put a platform service behind an interface with a no-op default](#put-a-platform-service-behind-an-interface-with-a-no-op-default)
  - [Install UI dialogs into a headless model through handler delegates](#install-ui-dialogs-into-a-headless-model-through-handler-delegates)
  - [Marshal a repeating timer into a headless model](#marshal-a-repeating-timer-into-a-headless-model)
  - [Set the mouse cursor from a model owned interface](#set-the-mouse-cursor-from-a-model-owned-interface)
  - [Veto a window close until unsaved work is handled](#veto-a-window-close-until-unsaved-work-is-handled)
  - [Tell the user when graphics initialization failed](#tell-the-user-when-graphics-initialization-failed)
  - [Show a WebView on every head and drive it from a command](#show-a-webview-on-every-head-and-drive-it-from-a-command)
  - [Replay a finished audio clip with one button press](#replay-a-finished-audio-clip-with-one-button-press)
- [Views, XAML and custom controls](#views-xaml-and-custom-controls)
  - [Declare a Skia page and bind with the platform Binding markup extension](#declare-a-skia-page-and-bind-with-the-platform-binding-markup-extension)
  - [Re-key theme brushes so controls dialogs and picker chrome follow your palette](#re-key-theme-brushes-so-controls-dialogs-and-picker-chrome-follow-your-palette)
  - [Dim a list row for an item the application cannot act on](#dim-a-list-row-for-an-item-the-application-cannot-act-on)
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
- [Graphics and rendering](#graphics-and-rendering)
  - [Host an OpenGL scene in XAML with a GLCanvasElement subclass](#host-an-opengl-scene-in-xaml-with-a-glcanvaselement-subclass)
  - [Keep the GL renderer framework-free behind an interface](#keep-the-gl-renderer-framework-free-behind-an-interface)
  - [Pick the shader version header for desktop GL or GLES at runtime](#pick-the-shader-version-header-for-desktop-gl-or-gles-at-runtime)
  - [Share one camera and one matrix convention across graphics APIs](#share-one-camera-and-one-matrix-convention-across-graphics-apis)
  - [Frame the camera automatically on each newly bound model](#frame-the-camera-automatically-on-each-newly-bound-model)
  - [Draw translucent surfaces in a second pass with depth writes off](#draw-translucent-surfaces-in-a-second-pass-with-depth-writes-off)
  - [Render off screen product shots on the head own GL context](#render-off-screen-product-shots-on-the-head-own-gl-context)
  - [Generate scene set dressing as ordinary geometry](#generate-scene-set-dressing-as-ordinary-geometry)
  - [Swap the 3D graphics backend at run time from a dropdown](#swap-the-3d-graphics-backend-at-run-time-from-a-dropdown)
  - [Gate an optional graphics backend to specific heads with an allow list](#gate-an-optional-graphics-backend-to-specific-heads-with-an-allow-list)
  - [Render an OpenGL scene off screen and composite it onto an SKXamlCanvas](#render-an-opengl-scene-off-screen-and-composite-it-onto-an-skxamlcanvas)
  - [Add a self contained Vulkan renderer that needs no shader toolchain](#add-a-self-contained-vulkan-renderer-that-needs-no-shader-toolchain)
  - [Add a direct to Metal renderer with no NuGet package or Apple bindings](#add-a-direct-to-metal-renderer-with-no-nuget-package-or-apple-bindings)
  - [Composite engine pixels onto Skia with the right vertical orientation](#composite-engine-pixels-onto-skia-with-the-right-vertical-orientation)
  - [Paint a CPU ray traced panorama into an SKBitmap](#paint-a-cpu-ray-traced-panorama-into-an-skbitmap)
  - [Decode HDR images and tone map them for display](#decode-hdr-images-and-tone-map-them-for-display)
  - [Build a textured cube mesh from a bitmap for previewing a flat material](#build-a-textured-cube-mesh-from-a-bitmap-for-previewing-a-flat-material)
  - [Paint a zoomable image on an SKXamlCanvas from the view model](#paint-a-zoomable-image-on-an-skxamlcanvas-from-the-view-model)
  - [Spotlight one region of an image on the canvas](#spotlight-one-region-of-an-image-on-the-canvas)
  - [Play a baked animation clip in a preview canvas](#play-a-baked-animation-clip-in-a-preview-canvas)
  - [Rasterize SVG art with the CodeBrix SkiaSvg library](#rasterize-svg-art-with-the-codebrix-skiasvg-library)
  - [Decode raster images with the CodeBrix Imaging library into a Skia bitmap](#decode-raster-images-with-the-codebrix-imaging-library-into-a-skia-bitmap)
  - [Normalize a downloaded image before embedding it in a document](#normalize-a-downloaded-image-before-embedding-it-in-a-document)
  - [Create a drawing session with named color layers](#create-a-drawing-session-with-named-color-layers)
  - [Export a drawing at a chosen pixel size](#export-a-drawing-at-a-chosen-pixel-size)
  - [Drive strokes in normalized image coordinates from a sensor](#drive-strokes-in-normalized-image-coordinates-from-a-sensor)
  - [Keep a mirrored preview and a mirrored drawing consistent](#keep-a-mirrored-preview-and-a-mirrored-drawing-consistent)
  - [Draw a brush sized cursor over a rendered drawing session](#draw-a-brush-sized-cursor-over-a-rendered-drawing-session)
  - [Draw an animated SkSL shader as a game engine direct drawing](#draw-an-animated-sksl-shader-as-a-game-engine-direct-drawing)
  - [Smooth worker rate data into frame rate animation](#smooth-worker-rate-data-into-frame-rate-animation)
  - [Keep a pipeline and a renderer decoupled by a normalized seam](#keep-a-pipeline-and-a-renderer-decoupled-by-a-normalized-seam)
  - [Offer a CPU fallback for a GPU rendering path behind one switch](#offer-a-cpu-fallback-for-a-gpu-rendering-path-behind-one-switch)
  - [Choose the render resolution from the zoom level](#choose-the-render-resolution-from-the-zoom-level)
  - [Draw a zoomable document canvas on an SKXamlCanvas subclass](#draw-a-zoomable-document-canvas-on-an-skxamlcanvas-subclass)
  - [Repaint only the dirty rectangle of a cached composite](#repaint-only-the-dirty-rectangle-of-a-cached-composite)
  - [Animate an overlay with a timer that stops when unloaded](#animate-an-overlay-with-a-timer-that-stops-when-unloaded)
  - [Host a canvas in a scroll viewer and drive zoom and scroll from an interface](#host-a-canvas-in-a-scroll-viewer-and-drive-zoom-and-scroll-from-an-interface)
  - [Scale a Skia drawn control from surface pixels to logical units](#scale-a-skia-drawn-control-from-surface-pixels-to-logical-units)
  - [Turn raw pixel surfaces into XAML image sources](#turn-raw-pixel-surfaces-into-xaml-image-sources)
  - [Honor EXIF orientation when decoding with SkiaSharp codecs](#honor-exif-orientation-when-decoding-with-skiasharp-codecs)
  - [Combine selection polygons with the CodeBrix PolygonTools library](#combine-selection-polygons-with-the-codebrix-polygontools-library)
  - [Give a headless library a drawing facade over SkiaSharp](#give-a-headless-library-a-drawing-facade-over-skiasharp)
  - [Play a Lottie animation on a Skia head and on native WinUI](#play-a-lottie-animation-on-a-skia-head-and-on-native-winui)
- [Media, camera and vision](#media-camera-and-vision)
  - [Host the VideoPlayer add-in in a page and drive it from the view model](#host-the-videoplayer-add-in-in-a-page-and-drive-it-from-the-view-model)
  - [Play a video from a URL with the MediaPlayer add-in](#play-a-video-from-a-url-with-the-mediaplayer-add-in)
  - [Play an audio clip straight from bytes with the AudioPlayer add-in](#play-an-audio-clip-straight-from-bytes-with-the-audioplayer-add-in)
  - [Probe a media file behind an interface the view model resolves](#probe-a-media-file-behind-an-interface-the-view-model-resolves)
  - [Detect a container from its first bytes](#detect-a-container-from-its-first-bytes)
  - [Author a cbv file in either container mode from a settled plan](#author-a-cbv-file-in-either-container-mode-from-a-settled-plan)
  - [Export an mp4 with FFmpeg through the CodeBrix VideoProcessing library](#export-an-mp4-with-ffmpeg-through-the-codebrix-videoprocessing-library)
  - [Demultiplex a bespoke container and remux it so an external tool can read it](#demultiplex-a-bespoke-container-and-remux-it-so-an-external-tool-can-read-it)
  - [Lift chapters and captions out of a source into sidecar files](#lift-chapters-and-captions-out-of-a-source-into-sidecar-files)
  - [Build a resolution ladder keyed on the short side with even dimensions](#build-a-resolution-ladder-keyed-on-the-short-side-with-even-dimensions)
  - [Move one encoder knob and pin everything else](#move-one-encoder-knob-and-pin-everything-else)
  - [Download run scoped media into a self cleaning temp cache](#download-run-scoped-media-into-a-self-cleaning-temp-cache)
  - [Extract a video poster frame and degrade when the external tool is missing](#extract-a-video-poster-frame-and-degrade-when-the-external-tool-is-missing)
  - [Enumerate cameras and start a live capture session](#enumerate-cameras-and-start-a-live-capture-session)
  - [Wrap a device library type so the view model never sees it](#wrap-a-device-library-type-so-the-view-model-never-sees-it)
  - [Run a TFLite model through the OpenCV DNN module](#run-a-tflite-model-through-the-opencv-dnn-module)
  - [Warp a rotated region of interest into a model input](#warp-a-rotated-region-of-interest-into-a-model-input)
  - [Recognize a gesture from landmark geometry instead of a model](#recognize-a-gesture-from-landmark-geometry-instead-of-a-model)
  - [Track multiple detections across frames with stable ids](#track-multiple-detections-across-frames-with-stable-ids)
  - [Smooth a noisy sensor position before it drives the UI](#smooth-a-noisy-sensor-position-before-it-drives-the-ui)
- [Documents, data and web APIs](#documents-data-and-web-apis)
  - [Put the real work in a UI free library behind a service interface](#put-the-real-work-in-a-ui-free-library-behind-a-service-interface)
  - [Encrypt text with the CodeBrix Cryptography library](#encrypt-text-with-the-codebrix-cryptography-library)
  - [Read an embedded default value at run time](#read-an-embedded-default-value-at-run-time)
  - [Guard Base64 input against invisible clipboard characters](#guard-base64-input-against-invisible-clipboard-characters)
  - [Read a zip archive without extracting it with the CodeBrix Compression library](#read-a-zip-archive-without-extracting-it-with-the-codebrix-compression-library)
  - [Resolve a file that another archive entry references by relative path](#resolve-a-file-that-another-archive-entry-references-by-relative-path)
  - [Classify and group the contents of a container for browsing](#classify-and-group-the-contents-of-a-container-for-browsing)
  - [Build a typed REST client with source generated JSON and its own exceptions](#build-a-typed-rest-client-with-source-generated-json-and-its-own-exceptions)
  - [Call a REST API behind a service interface the view model resolves](#call-a-rest-api-behind-a-service-interface-the-view-model-resolves)
  - [Pace outbound API calls with a rate gate](#pace-outbound-api-calls-with-a-rate-gate)
  - [Be a polite HTTP client to a public API](#be-a-polite-http-client-to-a-public-api)
  - [Normalize a user entered ID or URL before calling an API](#normalize-a-user-entered-id-or-url-before-calling-an-api)
  - [Resolve an ID that may be one of several object kinds](#resolve-an-id-that-may-be-one-of-several-object-kinds)
  - [Read a nested tree from an API with a cycle guard](#read-a-nested-tree-from-an-api-with-a-cycle-guard)
  - [Batch a metadata API and treat the result as best effort](#batch-a-metadata-api-and-treat-the-result-as-best-effort)
  - [Fetch a whole remote catalog once and cache images behind a concurrency gate](#fetch-a-whole-remote-catalog-once-and-cache-images-behind-a-concurrency-gate)
  - [Report true byte progress across a multi file download with side car files](#report-true-byte-progress-across-a-multi-file-download-with-side-car-files)
  - [Cache downloaded assets with a key you can invalidate](#cache-downloaded-assets-with-a-key-you-can-invalidate)
  - [Parse messy HTML into structured blocks with the CodeBrix MarkupParse library](#parse-messy-html-into-structured-blocks-with-the-codebrix-markupparse-library)
  - [Strip web only chrome while walking the DOM](#strip-web-only-chrome-while-walking-the-dom)
  - [Upgrade thumbnail URLs to print resolution](#upgrade-thumbnail-urls-to-print-resolution)
  - [Run a multi stage pipeline behind one service method](#run-a-multi-stage-pipeline-behind-one-service-method)
  - [Register embedded OFL fonts with the PDF font system](#register-embedded-ofl-fonts-with-the-pdf-font-system)
  - [Drop characters your embedded fonts cannot render](#drop-characters-your-embedded-fonts-cannot-render)
  - [Derive a whole document theme from one page size choice](#derive-a-whole-document-theme-from-one-page-size-choice)
  - [Compose a book with sections styles running heads and folios](#compose-a-book-with-sections-styles-running-heads-and-folios)
  - [Build a table of contents with real page numbers and dot leaders](#build-a-table-of-contents-with-real-page-numbers-and-dot-leaders)
  - [Place numbered framed figures with credit lines](#place-numbered-framed-figures-with-credit-lines)
  - [Pair a figure with the credit paragraph that follows it](#pair-a-figure-with-the-credit-paragraph-that-follows-it)
  - [Render booktabs style tables from parsed rows](#render-booktabs-style-tables-from-parsed-rows)
  - [Open a document with a raised initial](#open-a-document-with-a-raised-initial)
  - [Write rich text runs into a paragraph or a hyperlink](#write-rich-text-runs-into-a-paragraph-or-a-hyperlink)
  - [Render into either a section or a table cell](#render-into-either-a-section-or-a-table-cell)
  - [Keep unsupported content visible instead of failing the document](#keep-unsupported-content-visible-instead-of-failing-the-document)
  - [Compose a fixed layout poster with the CodeBrix PdfDocuments library](#compose-a-fixed-layout-poster-with-the-codebrix-pdfdocuments-library)
  - [Open a PDF and read its page count with the CodeBrix PdfRasterizer library](#open-a-pdf-and-read-its-page-count-with-the-codebrix-pdfrasterizer-library)
  - [Rasterize a PDF page to PNG off the UI thread](#rasterize-a-pdf-page-to-png-off-the-ui-thread)
  - [Keep two documents in step while letting the user offset one](#keep-two-documents-in-step-while-letting-the-user-offset-one)
  - [Treat two spellings of one path as the same file](#treat-two-spellings-of-one-path-as-the-same-file)
  - [Register import and export formats at startup through one entry point](#register-import-and-export-formats-at-startup-through-one-entry-point)
  - [Add codec coverage beyond SkiaSharp with the CodeBrix Imaging library](#add-codec-coverage-beyond-skiasharp-with-the-codebrix-imaging-library)
  - [Save a document through a native picker with format filters](#save-a-document-through-a-native-picker-with-format-filters)
  - [Raise a UI hook from a codec through a static event](#raise-a-ui-hook-from-a-codec-through-a-static-event)
- [Settings and persistence](#settings-and-persistence)
  - [Wrap the AppSettings add-in in one application named facade](#wrap-the-appsettings-add-in-in-one-application-named-facade)
  - [Open the settings store before any other startup work](#open-the-settings-store-before-any-other-startup-work)
  - [Choose a folder with the picker and remember it across runs](#choose-a-folder-with-the-picker-and-remember-it-across-runs)
  - [Restore a remembered window size before any window exists](#restore-a-remembered-window-size-before-any-window-exists)
  - [Persist small pieces of application state through the same store](#persist-small-pieces-of-application-state-through-the-same-store)
  - [Flush deferred settings at natural points instead of at quit](#flush-deferred-settings-at-natural-points-instead-of-at-quit)
- [Text editing](#text-editing)
  - [Lay out and draw text through the CodeBrix Platform TextLayout add-in](#lay-out-and-draw-text-through-the-codebrix-platform-textlayout-add-in)
- [Hosting a game engine](#hosting-a-game-engine)
  - [Hand the view model a game canvas at its first real layout size](#hand-the-view-model-a-game-canvas-at-its-first-real-layout-size)
  - [Run and pause a game engine session inside a page](#run-and-pause-a-game-engine-session-inside-a-page)
- [Testing](#testing)
  - [Keep view model rules in a plain class so they can be tested](#keep-view-model-rules-in-a-plain-class-so-they-can-be-tested)
  - [Set up an xUnit v3 test project for a CodeBrix library](#set-up-an-xunit-v3-test-project-for-a-codebrix-library)
  - [Build a test project against real CodeBrix Platform assemblies](#build-a-test-project-against-real-codebrix-platform-assemblies)
  - [Add the native assets a head would have supplied](#add-the-native-assets-a-head-would-have-supplied)
  - [Expose library internals to its test project](#expose-library-internals-to-its-test-project)
  - [Test a service the way the container builds it](#test-a-service-the-way-the-container-builds-it)
  - [Route logging from the code under test into test output](#route-logging-from-the-code-under-test-into-test-output)
  - [Share one expensive fixture across every test class that needs it](#share-one-expensive-fixture-across-every-test-class-that-needs-it)
  - [Build the binary inputs your tests need instead of committing them](#build-the-binary-inputs-your-tests-need-instead-of-committing-them)
  - [Generate real media clips from a synthetic source](#generate-real-media-clips-from-a-synthetic-source)
  - [Read a committed fixture from beside the test binary](#read-a-committed-fixture-from-beside-the-test-binary)
  - [Test a document renderer against the object model it produces](#test-a-document-renderer-against-the-object-model-it-produces)
  - [Assert on a generated document without a golden file](#assert-on-a-generated-document-without-a-golden-file)
  - [Make live tests opt in and keep them out of the default run](#make-live-tests-opt-in-and-keep-them-out-of-the-default-run)
  - [Test an HTTP client offline with a stub handler](#test-an-http-client-offline-with-a-stub-handler)
  - [Mock a rendering or API seam with CodeBrix TestMocks](#mock-a-rendering-or-api-seam-with-codebrix-testmocks)
  - [Test GL code headlessly with a surfaceless EGL context](#test-gl-code-headlessly-with-a-surfaceless-egl-context)
  - [Prove every graphics backend with the same mirrored suite](#prove-every-graphics-backend-with-the-same-mirrored-suite)
  - [Pin a fixed bug with a regression test that says why it is shaped that way](#pin-a-fixed-bug-with-a-regression-test-that-says-why-it-is-shaped-that-way)
  - [Compare rendered images pixel by pixel](#compare-rendered-images-pixel-by-pixel)
  - [Point a process-global store at a throwaway folder in tests](#point-a-process-global-store-at-a-throwaway-folder-in-tests)
  - [Drive a scripted end-to-end run of the whole application](#drive-a-scripted-end-to-end-run-of-the-whole-application)
- [Project layout, packaging and native assets](#project-layout-packaging-and-native-assets)
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
  - [Not yet covered by a sample](#not-yet-covered-by-a-sample)

## Application structure and startup

### Start each head from a Program Main and pick the platform backend

**When you want this.** You are writing the entry point of a head project and want
to know the minimum it has to contain, and what a head is allowed to differ on.

**The MVVM shape.** `Program.Main` owns nothing but hosting. It initializes
logging, builds a host with `CodeBrixPlatformHostBuilder`, hands it a factory for
the shared `App` class, selects exactly one backend, and runs. No application
logic lives in a head; services, fonts, settings and the first page all belong to
`App`, and everything the user interacts with belongs to a view model.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.LinuxX11/Program.cs
using CodeBrix.Platform.UI.Hosting;
using System;

namespace MediaPlayerDemo;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxX11()
            .UseDirectSkiaCanvasMode() //Experimental - should be safe to leave enabled
            .Build();

        host.Run();
    }
}
```

The backend call is the only line that changes between heads:

| Head | Call |
| --- | --- |
| LinuxX11 | `.UseLinuxX11()` |
| LinuxWayland | `.UseLinuxWayland()` |
| LinuxFrameBuffer | `.UseLinuxFrameBuffer()` |
| MacOS | `.UseMacOS()` |
| Win32Skia | `.UseWindowsWin32()` |
| WinWpfSkia | `.UseWindowsWpf()` |

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.LinuxX11/Program.cs` and the five sibling
head projects under `MediaPlayerDemo/src/`
`KenneyAssetBrowser/src/KenneyAssetBrowser.LinuxX11/Program.cs`

**Also shown by.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Win32Skia/Program.cs` (the one head
in the repository that uses `async Task Main` with `await host.RunAsync()`),
`NotionDocumentCreator/src/NotionDocumentCreator.LinuxX11/Program.cs`,
`PainDiagram/CodeBrixPlatform/PainDiagram.LinuxX11/Program.cs`,
`PalmVisualizer/src/PalmVisualizer.LinuxX11/Program.cs`,
`PdfSideBySide/src/PdfSideBySide.LinuxX11/Program.cs`,
`Pinta.Brix/src/Pinta.Brix.LinuxX11/Program.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.LinuxX11/Program.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.LinuxX11/Program.cs`,
`WebcamPainter/src/WebcamPainter.LinuxX11/Program.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.LinuxX11/Program.cs`,
`CodeBrixVideoTool/src/CodeBrixVideoTool.LinuxX11/Program.cs`

**Sharp edges.**
- `App.InitializeLogging()` is called before the host is built, never after. The
  method carries the comment "Called from each head's Program.Main BEFORE
  building the host" in every application that has it; logging wired after
  `Build()` misses the platform's own startup messages.
- `[STAThread]` is on `Main` in every head, including the Linux and macOS ones.
- `.App(() => new App())` takes a factory, not an instance. The host decides when
  the application object is constructed.
- `.UseDirectSkiaCanvasMode()` is marked experimental in the generated comment
  ("should be safe to leave enabled") and most applications keep it on every head.
  WebcamPainter calls it on the LinuxX11 head only, and PolyHavenBrowser only on
  its two Windows heads, so do not assume every head in an application has it.
- The heads all declare the same namespace as the shared UI project, which is
  what lets `new App()` resolve in `Program.cs` with no using directive. Some
  heads carry a `// ReSharper disable CheckNamespace` comment because of it.
- Heads are not literally interchangeable: copy one to a new platform and check
  what it adds after `Build()` (see the WinWpfSkia and framebuffer blueprints).

### Bootstrap the application in the App constructor

**When you want this.** Every application. This is the ordering contract for the
`App` constructor: font configuration, dependency-injection container, design
mode off, then `InitializeComponent()`.

**The MVVM shape.** `App` is the composition root and does nothing else. It sets
the platform's default text font, creates the `SimpleServiceResolver` from an
`IHostBuilderProvider` and registers the application's services through one
extension method per library, calls `SimpleViewModel.SetIsDesignMode(false)` so
view models built by the XAML parser run their real constructor path, and only
then initializes the XAML. View models resolve what they need with the inherited
`GetService<T>()`; nothing is passed down from `App`.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/App.xaml.cs
public App()
{
    //Set Roboto as the default font for all text in the application
    global::CodeBrix.Platform.UI.FeatureConfiguration.Font.DefaultTextFontFamily =
        "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/Roboto.ttf";

    //Fonts consulted for characters the default font has no glyph for
    global::CodeBrix.Platform.UI.FeatureConfiguration.Font.FallbackFontFamilies =
    [
        "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansArmenian.ttf",
        "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansGeorgian.ttf",
    ];

    SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
    {
        //Register the app's services here
        services.AddCreateDocument();
    });
    SimpleViewModel.SetIsDesignMode(false);

    InitializeComponent();
}
```

The matching half is in the view model: the first line of every view-model
constructor in the family is the design-mode guard, and it only works because
`App` turned design mode off first.

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
public MainViewModel()
{
    if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

    _documentSvc = GetService<INotionDocumentService>();
    // ...
}
```

An application with a settings store opens it in the same constructor, before
`InitializeComponent()`, because the page's view model reads a setting in its own
constructor:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/App.xaml.cs
    SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
    {
        //Register the app's services here
        services.AddKenneyAssetBrowser();
    });
    SimpleViewModel.SetIsDesignMode(false);

    //Open (or silently create) the single portable settings.sqlite store —
    //  including its startup auto-backup and pruning — before any UI renders.
    SettingsService.Initialize();

    InitializeComponent();
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.UI/App.xaml.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/App.xaml.cs`
`PdfSideBySide/src/PdfSideBySide.UI/App.xaml.cs`

**Also shown by.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs`,
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs`,
`MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml.cs`,
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/App.xaml.cs`,
`PalmVisualizer/src/PalmVisualizer.UI/App.xaml.cs`,
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.UI/App.xaml.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/App.xaml.cs`,
`WebcamPainter/src/WebcamPainter.UI/App.xaml.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/App.xaml.cs`

**Sharp edges.**
- Forgetting `SetIsDesignMode(false)` is silent. Nothing throws; every view model
  built by XAML takes its design-time early-out and the application starts and
  does nothing. It has to run before the first view model is constructed, which
  in practice means before `InitializeComponent()`.
- `SimpleServiceResolver.CreateInstance()` must be called even when there is
  nothing to register. MediaPlayerDemo, PainDiagram, PalmVisualizer, PdfSideBySide,
  Pinta.Brix and WebcamPainter all keep an empty, commented registration callback
  rather than dropping the call.
- Font configuration is set before `InitializeComponent()` so the first measured
  text already uses the right family.
- The MAUI head in JustBetweenUs is the one place the order differs: it calls
  `InitializeComponent()` first, then the resolver, then `SetIsDesignMode(false)`.
  Both orders work there, but keeping the resolver before any view is constructed
  is the safer habit, because a page whose XAML instantiates a view model resolves
  services during `InitializeComponent()`.
- Some applications write the guard as `if (!IsDesignMode(true)) { ... }` wrapping
  the whole body instead of an early return; the two forms are equivalent.

### Create the main window and navigate to the first page

**When you want this.** You are writing the `OnLaunched` override for a head and
want the smallest correct window-and-frame bootstrap.

**The MVVM shape.** `App` owns the window and the navigation frame and nothing
else. The page it navigates to sets its own `DataContext` and does its own bridge
wiring; `App` never touches a view model.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml.cs
protected Window MainWindow { get; private set; }

protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    MainWindow = new Window { Title = "MediaPlayerDemo" };

    if (MainWindow.Content is not Frame rootFrame)
    {
        rootFrame = new Frame();
        MainWindow.Content = rootFrame;
        rootFrame.NavigationFailed += OnNavigationFailed;
    }

    if (rootFrame.Content == null)
    {
        rootFrame.Navigate(typeof(Views.MainPage), args.Arguments);
    }

    MainWindow.Activate();
}

void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
{
    throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
}
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs`

**Also shown by.**
`PdfSideBySide/src/PdfSideBySide.UI/App.xaml.cs`,
`JustBetweenUs/JustBetweenUs.WinUI/App.xaml.cs` (the native WinUI 3 head keeps
the same override almost verbatim, with the stock template code it replaces left
in the file as a comment)

**Sharp edges.**
- `NavigationFailed` throws rather than logging, so a typo in the page type
  surfaces immediately instead of showing an empty window.
- A native WPF head has no frame at all: `PainDiagram.Wpf` uses
  `StartupUri="Views/MainWindow.xaml"` in `App.xaml` and has no `OnLaunched`
  override, while a native WinUI 3 head keeps the frame-and-`Navigate()` shape.

### Supply a generic host builder to SimpleServiceResolver

**When you want this.** `SimpleServiceResolver.CreateInstance()` needs an
`IHostBuilderProvider`, and you want that in one shared place rather than
duplicated in every head.

**The MVVM shape.** A small static helper in the library the heads reference
wraps `Host.CreateDefaultBuilder()` in an `IHostBuilderProvider` and hands back a
single shared instance. `App` passes `HostHelper.GetHost()` and its registration
callback. View models then resolve services instead of constructing them.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/Helpers/HostHelper.cs
using CodeBrix.Platform.Simple;
using Microsoft.Extensions.Hosting;

namespace MediaPlayerDemo.Helpers;

/// <summary>
/// Supplies the generic-host builder that <see cref="SimpleServiceResolver"/> uses to build
/// the application's dependency-injection container at startup.
/// </summary>
public static class HostHelper
{
    private sealed class HostBuilderProvider : IHostBuilderProvider
    {
        public IHostBuilder CreateDefaultBuilder() => Host.CreateDefaultBuilder();
        public IHostBuilder CreateDefaultBuilder(string[] args) => Host.CreateDefaultBuilder(args);
    }

    private static readonly HostBuilderProvider Provider = new();

    /// <summary>Gets the shared host-builder provider.</summary>
    public static IHostBuilderProvider GetHost() => Provider;
}
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/Helpers/HostHelper.cs`

**Also shown by.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Helpers/HostHelper.cs`,
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/Helpers/HostHelper.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.Core/Helpers/HostHelper.cs`,
`PalmVisualizer/src/PalmVisualizer.Core/Helpers/HostHelper.cs`,
`PdfSideBySide/src/PdfSideBySide.Core/Helpers/HostHelper.cs`,
`Pinta.Brix/src/Pinta.Brix.Core/Helpers/HostHelper.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Helpers/HostHelper.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Helpers/HostHelper.cs`,
`WebcamPainter/src/WebcamPainter.Core/Helpers/HostHelper.cs`,
`JustBetweenUs/Shared/Helpers/HostHelper.cs` and
`PainDiagram/Shared/Helpers/HostHelper.cs` and
`WikipediaPublisher/Shared/Helpers/HostHelper.cs` (these three live in a
`Shared/` folder and are file-linked into the Skia library and into each native
head, so all of an application's heads get an identical container)

**Sharp edges.**
- The provider is a private nested class exposed only through the interface, with
  one cached instance, so there is nothing to construct twice by accident.
- The hosting package is referenced by the library that carries the application's
  packages, not by the heads. Keeping it there is what lets every head share one
  helper.

### Register library services with one AddXxx extension method

**When you want this.** Your real work lives in a library and you want the
application to register it in one line, without the application ever naming the
implementation type.

**The MVVM shape.** The library exports an interface and one
`IServiceCollection` extension method. The application calls it inside the
`SimpleServiceResolver.CreateInstance()` callback. The view model resolves the
interface with `GetService<T>()` and never sees the concrete class.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/RegisterServices.cs
public static class RegisterServices
{
    /// <summary>
    /// Registers the WikipediaPublisher article-rendering services with the DI container.
    /// </summary>
    public static IServiceCollection AddRenderArticle(this IServiceCollection services)
    {
        if (services == null) { throw new ArgumentNullException(nameof(services)); }
        services.AddSingleton<IArticleRenderService, ArticleRenderService>();
        return services;
    }
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
public MainViewModel()
{
    if (!IsDesignMode(true))
    {
        Debug.WriteLine("Main view model startup.");

        _renderSvc = GetService<IArticleRenderService>();
        // ...
        StatusText = "Search for an article, browse to it, choose where to save the PDF, then click Publish.";
    }
}
```

**Variant: one application-level extension that calls the library's own.** When
an application has several services and one of them is a library with its own
registration method, the application keeps a single `RegisterServices.cs` that
chains them, so `App` still calls one method:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/RegisterServices.cs
public static class RegisterServices
{
    /// <summary>Registers the Poly Haven API client, the catalog service and the download service.</summary>
    public static IServiceCollection AddPolyHavenBrowser(this IServiceCollection services)
    {
        if (services == null) { throw new ArgumentNullException(nameof(services)); }

        services.AddPolyHavenApiClient(options =>
        {
            //Poly Haven asks API consumers to identify themselves.
            options.UserAgent = "PolyHavenBrowser/1.0 (CodeBrix.Platform sample; +https://polyhaven.com)";
        });

        services.AddSingleton<ModelCatalogService>();
        services.AddSingleton<ModelDownloadService>();
        services.AddSingleton<DocumentBackdropService>();

        return services;
    }
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/RegisterServices.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.Core/RegisterServices.cs`

**Also shown by.**
`JustBetweenUs/JustBetweenUs.Encryption/RegisterServices.cs` (`AddEncryption()`),
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/RegisterServices.cs`
(`AddKenneyAssetBrowser()`),
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/RegisterServices.cs`
(`AddCreateDocument()`),
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/RegisterServices.cs`,
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs` (two `AddSingleton`
calls straight in the callback, which is the smaller form when there is no
library boundary to respect)

**Sharp edges.**
- Every one of these extensions starts with a null check on `services` and
  returns the collection so calls chain.
- Registrations that own state are singletons: the Notion service holds the
  connected client and the discovered tree metadata between calls, and the
  article renderer owns an `HttpClient`.
- Library services take an optional `ILogger<T>` and fall back to a null logger,
  so the library still works in tests with no container at all.
- A view model that can also run without a container falls back to a concrete
  instance: `runner = GetService<IConversionRunner>() ?? new ConversionRunner();`
  in `CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs`.

### Turn on console logging only in Debug builds

**When you want this.** You want platform and application diagnostics on a
console while developing, and a silent Release build, on every head.

**The MVVM shape.** Not a view-model concern. One `public static void
InitializeLogging()` on `App`, whole body inside `#if DEBUG`, called from every
head's `Main` as its first statement.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml.cs
// Called from each head's Program.Main BEFORE building the host.
public static void InitializeLogging()
{
#if DEBUG
    var factory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
        builder.SetMinimumLevel(LogLevel.Information);
        builder.AddFilter("CodeBrix.Platform", LogLevel.Warning);
        builder.AddFilter("Windows", LogLevel.Warning);
        builder.AddFilter("Microsoft", LogLevel.Warning);
    });

    global::CodeBrix.Platform.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;
    global::CodeBrix.Platform.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
}
```

**Variant: let one component through the filter.** CodeBrixVideoTool raises one
category back to Information because the player add-in logs the graphics backend
it chose exactly once and that line is worth seeing:

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs
        builder.AddFilter("CodeBrix.Platform", LogLevel.Warning);
        //The player add-in logs the graphics backend it chose exactly once, at Information.
        builder.AddFilter("CodeBrix.Platform.UI.VideoPlayer", LogLevel.Information);
```

**Variant: guard the adapter call when the same file is linked into a native
head.** Applications whose `App.xaml.cs` or view model source is compiled into a
non-Skia head wrap the adapter call in the `HAS_CODEBRIX` symbol that only the
Skia projects define:

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs
    global::CodeBrix.Platform.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_CODEBRIX
    global::CodeBrix.Platform.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/App.xaml.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/App.xaml.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.UI/App.xaml.cs`,
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/App.xaml.cs`,
`PalmVisualizer/src/PalmVisualizer.UI/App.xaml.cs`,
`PdfSideBySide/src/PdfSideBySide.UI/App.xaml.cs`,
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.UI/App.xaml.cs`,
`WebcamPainter/src/WebcamPainter.UI/App.xaml.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/App.xaml.cs`

**Sharp edges.**
- Both statements are needed. Assigning `AmbientLoggerFactory` alone is not
  enough; `LoggingAdapter.Initialize()` is what connects the platform's own
  logging to your factory, and it comes second.
- The minimum level is Information while the platform, `Windows` and `Microsoft`
  categories are filtered to Warning, so your own messages are visible without
  the framework drowning them.
- Because the whole body is inside `#if DEBUG`, the method compiles to nothing in
  Release and every call site stays valid.

### Set a bundled font as the default text font and register script fallbacks

**When you want this.** You want one typeface everywhere without setting
`FontFamily` on every control, including on heads with no system font stack to
fall back to, such as the Linux framebuffer head.

**The MVVM shape.** Pure startup and view configuration, in two places: `App`'s
constructor sets the platform's default text font family (and, optionally, the
faces consulted for characters it has no glyph for) before
`InitializeComponent()`, and `App.xaml` publishes the same face under a resource
key so a page can name it.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/App.xaml.cs
        //Set Roboto as the default font for all text in the application
        global::CodeBrix.Platform.UI.FeatureConfiguration.Font.DefaultTextFontFamily =
            "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/Roboto.ttf";

        //Fonts consulted for characters the default font has no glyph for
        global::CodeBrix.Platform.UI.FeatureConfiguration.Font.FallbackFontFamilies =
        [
            "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansArmenian.ttf",
            "ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/NotoSansGeorgian.ttf",
        ];
```

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/App.xaml -->
  <Application.Resources>
    <ResourceDictionary>
      <ResourceDictionary.MergedDictionaries>
        <!-- Load WinUI resources -->
        <c:XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
      </ResourceDictionary.MergedDictionaries>
      <!-- Roboto font - reference the .ttf file directly (the Fonts.xaml
           merge does not work on Skia targets) -->
      <m:FontFamily x:Key="RobotoFont">ms-appx:///CodeBrix.Platform.Fonts.Roboto/Fonts/Roboto.ttf</m:FontFamily>
    </ResourceDictionary>
  </Application.Resources>
```

```xml
<!-- From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml -->
<Page
    x:Class="WebcamPainter.Views.MainPage"
    FontFamily="{StaticResource RobotoFont}"
    Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.UI/App.xaml` and `App.xaml.cs`
`WebcamPainter/src/WebcamPainter.UI/App.xaml` and `Views/MainPage.xaml`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/App.xaml.cs` (a serif family with
matching Noto Serif fallbacks),
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs` (a plain Noto Sans face
in the fallback list as well as the two script faces),
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/App.xaml`,
`MediaPlayerDemo/src/MediaPlayerDemo.UI/App.xaml`,
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/App.xaml`,
`PalmVisualizer/src/PalmVisualizer.UI/App.xaml`,
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml`,
`PolyHavenBrowser/src/PolyHavenBrowser.UI/App.xaml`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/App.xaml`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/App.xaml`

**Sharp edges.**
- The comment in `App.xaml` records the rule the whole repository follows:
  merging a font package's `Fonts.xaml` resource dictionary does not work on Skia
  targets. Reference the `.ttf` directly through an `ms-appx:///` URI whose first
  segment is the font assembly name. Several applications keep the commented-out
  merge line in the file as a marker.
- Two forms of the URI appear. Some applications add a `#FamilyName` suffix
  (`.../Roboto.ttf#Roboto`) and some do not; where the suffix is used, both
  halves are required.
- `DefaultTextFontFamily` and the `FontFamily` resource are different mechanisms
  and both are worth setting: the first covers text the application never styles,
  the second is what `FontFamily="{StaticResource ...}"` binds to.
- Fallback entries name the plain, weight-less face files. A font package also
  ships per-weight files whose names will not resolve here.
- The font package is referenced by the library that carries the application's
  packages, so all six heads get it transitively; the heads never reference it.
- A native head has its own `App.xaml`, so nothing set in the shared one reaches
  it. The MAUI head in JustBetweenUs registers its own copies of the font files
  through `ConfigureFonts` in `MauiProgram.cs` instead.

### Enable a picker and the software keyboard on the Linux framebuffer head

**When you want this.** Your application asks the user for a file or folder, or
takes typed input, and you want it to work on the LinuxFrameBuffer head, which
has no desktop chrome to borrow a picker or a keyboard from.

**The MVVM shape.** Head configuration only. The view model does not change: it
still calls a picker through its bridge and still binds a `TextBox`. The head is
what decides whether a picker window and an on-screen keyboard exist to serve
those calls, and the view model already has a graceful path for a head that
supplies neither.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.LinuxFrameBuffer/Program.cs
var host = CodeBrixPlatformHostBuilder.Create()
    .App(() => new App())
    .UseLinuxFrameBuffer(fb => fb
        .Orientation(DisplayOrientations.Landscape, isPreferredOrientation: true)
        .AutoRotationEnabled(true)
        .EnableFolderPicker(new FolderPickerOptions {
           AllowNewFolderCreate = true,
           StartFolder = "/home/jeremy/Temp",
           RestrictToFolder = "/home/jeremy",
        })
        //The FrameBuffer head has no OS chrome, so the "Save PDF as…" picker the
        //  Document button pops is opt-in
        .EnableFileSavePicker(new FilePickerOptions {
           AllowNewFolderCreate = true,
           StartFolder = "/home/jeremy/Temp",
           RestrictToFolder = "/home/jeremy",
           RequiredExtension = ".pdf",
        })
        .EnableSoftwareKeyboard(new SoftwareKeyboardOptions{
            ShowDismissKey = true,  //default behavior = true
            KeyHeight = SoftwareKeyHeight.FullHeight,  //default behavior = FullHeight
        })
    )
    .UseDirectSkiaCanvasMode()
    .Build();

host.Run();
```

The application's resource dictionary restyles that built-in chrome, because the
picker and keyboard resolve the same `ContentDialog` keys the application already
themes:

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/App.xaml -->
<!-- Dialogs open in the popup layer, which follows the app default theme (the
     RequestedTheme="Dark" above) rather than RootGrid's - these ContentDialog
     keys then refine them to the app palette. On the FrameBuffer heads the
     built-in picker/software-keyboard chrome resolves the same keys, so it
     restyles identically -->
<m:SolidColorBrush x:Key="ContentDialogBackground" Color="#1F232B" />
<m:SolidColorBrush x:Key="ContentDialogForeground" Color="#F2F4F8" />
<!-- Resolved by the FrameBuffer/Emulated picker + software-keyboard chrome -->
<m:SolidColorBrush x:Key="ContentDialogTopOverlay" Color="#1F232B" />
<m:SolidColorBrush x:Key="ContentDialogSeparatorBorderBrush" Color="#2A2F39" />
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.LinuxFrameBuffer/Program.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/App.xaml`

**Also shown by.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.LinuxFrameBuffer/Program.cs`
(software keyboard only, at `SoftwareKeyHeight.HalfHeight` so the keyboard leaves
more of the page visible),
`KenneyAssetBrowser/src/KenneyAssetBrowser.LinuxFrameBuffer/Program.cs` (folder
picker with `AllowNewFolderCreate = false`),
`NotionDocumentCreator/src/NotionDocumentCreator.LinuxFrameBuffer/Program.cs`
(save picker plus keyboard, because the user types a long API token),
`PdfSideBySide/src/PdfSideBySide.LinuxFrameBuffer/Program.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.LinuxFrameBuffer/Program.cs`

**Sharp edges.**
- Both features are off unless you opt in, one builder call each, and only on
  this head. Code that assumes a picker exists gets a `NotSupportedException`
  instead of a dialog.
- `StartFolder` and `RestrictToFolder` in the samples are the author's own machine
  paths. Treat them as placeholders and compute them from the environment in your
  own application; `RestrictToFolder` fences the picker so the user cannot
  navigate above it.
- `RequiredExtension` on the picker and the application's own expectation about
  the file it will write have to agree.
- `ShowDismissKey` defaults to true and `KeyHeight` defaults to `FullHeight`; the
  samples record both defaults in comments next to the overrides.
- Dialogs open in the popup layer, which follows the application's
  `RequestedTheme` rather than the theme of the grid they were raised from, so a
  dark application has to key the `ContentDialog` brushes at the
  `Application.Resources` level.

### Force the software render surface on the WinWpfSkia head

**When you want this.** Your WPF-hosted head opens a window that stays blank,
black or white while every other head renders correctly.

**The MVVM shape.** Head-level plumbing in `Program.cs`, between `Build()` and
`Run()`. The built host is type-tested and its render surface type changed;
nothing about the application changes.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.WinWpfSkia/Program.cs
using CodeBrix.Platform.UI.Hosting;
using CodeBrix.Platform.UI.Runtime.Skia.Wpf;
using System;

namespace JustBetweenUs;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWindowsWpf()
            .Build();

        // ...
        if (host is WpfHost wpfHost)
        {
            wpfHost.RenderSurfaceType = RenderSurfaceType.Software;
        }

        host.Run();
    }
}
```

**Where to look.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.WinWpfSkia/Program.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.WinWpfSkia/Program.cs`,
`MediaPlayerDemo/src/MediaPlayerDemo.WinWpfSkia/Program.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.WinWpfSkia/Program.cs`,
`PainDiagram/CodeBrixPlatform/PainDiagram.WinWpfSkia/Program.cs`,
`PalmVisualizer/src/PalmVisualizer.WinWpfSkia/Program.cs`,
`PdfSideBySide/src/PdfSideBySide.WinWpfSkia/Program.cs`,
`Pinta.Brix/src/Pinta.Brix.WinWpfSkia/Program.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.WinWpfSkia/Program.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.WinWpfSkia/Program.cs`,
`WebcamPainter/src/WebcamPainter.WinWpfSkia/Program.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.WinWpfSkia/Program.cs`

**Sharp edges.**
- The comment trimmed out of the block above explains why it is needed: the WPF
  host's default OpenGL renderer draws through raw `opengl32` onto WPF's own
  DirectX-composited window handle. That is an airspace conflict on many systems,
  so the window appears but the content never composites. Software rendering
  blits the Skia frame into WPF and composites correctly.
- The cast is guarded with `is`, so the file stays valid if the host type ever
  changes.
- This head needs `using CodeBrix.Platform.UI.Runtime.Skia.Wpf;`, which the other
  heads do not have; the type comes from that head's runtime package.
- In most of these applications this is the only per-head behavioral difference
  in the whole solution.

### Keep Main synchronous and STA so an embedded WebView can start

**When you want this.** Your application hosts a WebView on Windows and you are
tempted to write `async Task Main`.

**The MVVM shape.** Head plumbing only, but it decides whether the WebView bridge
works at all.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.Win32Skia/Program.cs
// Must be a synchronous STA Main: WebView2 (CoreWebView2Environment.CreateAsync) requires the
// UI thread to be an STA. With 'async Task Main' the [STAThread] attribute is ignored and the
// thread runs as MTA, so WebView2 creation throws RPC_E_CHANGED_MODE ("Cannot change thread mode
// after it is set."). host.Run() pumps the Win32 message loop synchronously on this STA thread.
[STAThread]
public static void Main(string[] args)
{
    App.InitializeLogging();

    var host = CodeBrixPlatformHostBuilder.Create()
        .App(() => new App())
        .UseWindowsWin32()
        .Build();

    host.Run();
}
```

**Where to look.**
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.Win32Skia/Program.cs`

**Sharp edges.**
- `[STAThread]` is silently ignored on an `async Task Main`; the failure shows up
  much later as an RPC error when the WebView is created.

### Turn on extra media codecs once at startup

**When you want this.** You are playing media through an add-in and you need
decoders that the add-in does not, and by design cannot, reference itself.

**The MVVM shape.** A small static helper in the library that owns playback,
called from `App`'s constructor before anything else. It is idempotent behind a
lock and exposes `IsRegistered` so a test can assert it.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/PlaybackCodecs.cs
public static class PlaybackCodecs
{
    private static readonly object Gate = new();

    /// <summary>True once both codecs have been turned on.</summary>
    public static bool IsRegistered { get; private set; }

    public static void RegisterOnce()
    {
        lock (Gate)
        {
            if (IsRegistered)
            {
                return;
            }

            CodeBrixVideoPlaybackDav1d.Register();
            CodeBrixAudioOpus.Register();
            IsRegistered = true;
        }
    }
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs
//Turn on AV1 video and Opus audio, once. Every one of the four formats this application
//writes carries AV1, so nothing plays at all without the first of these.
PlaybackCodecs.RegisterOnce();
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/PlaybackCodecs.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/App.xaml.cs`
`CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Playback.Tests/PlaybackCodecsTests.cs`

**Sharp edges.**
- The class documentation is explicit that these decoders are the application's
  dependencies and never the add-in's. Their licenses differ from the add-in's,
  which is exactly why each ships as its own package and an application that
  wants them references them and calls `Register()` once. The add-in resolves
  codecs through the playback session's registries, so it plays them with no
  change and no reference of its own.
- The source says outright: "There is deliberately no module initializer doing
  this - that would work in a debug build and silently not run in a trimmed
  publish."
- Register from `App`'s constructor, ahead of the container and the XAML, so
  nothing can open a media file first.

### Run one view model on Skia heads and on native WinUI 3 WPF and MAUI heads

**When you want this.** You must ship a native Windows or mobile build alongside
the Skia heads and do not want a second implementation of your logic.

**The MVVM shape.** The view model is a plain class deriving from
`SimpleViewModel` that references only the Simple toolkit and your own service
interfaces. It is not shipped as a library: every head, Skia or native, pulls it
in as a linked `<Compile>` item and compiles its own copy. Each head then supplies
platform plumbing through the bridge interfaces the view model declares, and each
head's `App` does the same two Simple-toolkit calls at startup. The only
conditional compilation inside the view model is a single attribute.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
#if HAS_CODEBRIX
[Microsoft.UI.Xaml.Data.Bindable]
#endif
public class MainViewModel : SimpleViewModel, IFileSaveBridge, ICanvasInvalidator
{
    // ...
}
```

```xml
<!-- From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/PainDiagram.Wpf.csproj -->
<ItemGroup>
  <Compile Include="..\Shared\Drawing\DrawingCanvas.cs" Link="Drawing\DrawingCanvas.cs" />
  <Compile Include="..\Shared\Helpers\HostHelper.cs" Link="Helpers\HostHelper.cs" />
  <Compile Include="..\Shared\ViewModels\MainViewModel.cs" Link="ViewModels\MainViewModel.cs" />
</ItemGroup>
```

The native head brings its own `App.xaml` and its own window or page, and still
performs the same bootstrap:

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/App.xaml.cs
public partial class App : Application
{
    public App()
    {
        SimpleServiceResolver.CreateInstance(HostHelper.GetHost(), services =>
        {
            //No custom services needed - the drawing session lives in the view model
        });
        SimpleViewModel.SetIsDesignMode(false);
    }
}
```

```xml
<!-- From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml -->
<Window x:Class="PainDiagram.Views.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:PainDiagram.ViewModels"
        xmlns:drawing="clr-namespace:CodeBrix.Imaging.Drawing"
        Title="Pain Diagram" Height="720" Width="640">

    <Window.DataContext>
        <vm:MainViewModel />
    </Window.DataContext>
    <!-- ... -->
</Window>
```

**Where to look.**
`PainDiagram/Shared/ViewModels/MainViewModel.cs`
`PainDiagram/PainDiagram.Wpf/` and `PainDiagram/PainDiagram.WinUI/`
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`
`JustBetweenUs/JustBetweenUs.WinUI/JustBetweenUs.WinUI.csproj`,
`JustBetweenUs/JustBetweenUs.Wpf/JustBetweenUs.Wpf.csproj`,
`JustBetweenUs/Mobile/JustBetweenUs.Mobile.csproj`

**Also shown by.**
`WikipediaPublisher/WikipediaPublisher.WinUI/` and
`WikipediaPublisher/WikipediaPublisher.Wpf/` (eight heads share one
`Shared/ViewModels/MainViewModel.cs`; the WinUI head links the view model, the
host helper and the file-dialog helper, the WPF head links only the first two,
because a WPF `SaveFileDialog` leaves no placeholder file to clean up)

**Sharp edges.**
- `HAS_CODEBRIX` is defined by the library that carries the platform packages and
  by every Skia head csproj, but not by the native projects, so the `[Bindable]`
  attribute is applied only in the platform assemblies. If you link view-model
  source into a native head, check which symbols that project defines.
- Keep such symbols to a minimum. JustBetweenUs also defines `HAS_WINUI` for one
  startup timing difference, and every symbol is a place where a head can drift.
- File-linked source means every consuming assembly must also supply anything the
  source expects at run time. PainDiagram embeds its body-map image three times,
  once per assembly that compiles the shared view model, under one logical
  resource name.
- The native heads have their own `App.xaml`, so anything in the shared one - a
  font resource, the default font family - does not reach them.
- Because the file is compiled into each head, the head's root namespace must
  agree with the namespace declared in the file.

### Detect which platform head is running without referencing it

**When you want this.** A library needs to know which of the six heads is hosting
it, and must not take a dependency on any of them.

**The MVVM shape.** A static, lazily computed detection inside the headless
library; everything above it consumes a plain enum.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanPlatformSupport.cs
// Each head's Program.cs loads exactly one head runtime assembly (via
// CodeBrixPlatformHostBuilder.Use*), so by the time any UI runs, scanning the loaded
// assemblies identifies the head without this library referencing any of them.
private static PlatformHead DetectCurrentHead()
{
    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
        var head = ClassifyAssemblyName(assembly.GetName().Name);
        if (head != PlatformHead.Unknown)
        {
            return head;
        }
    }

    return PlatformHead.Unknown;
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanPlatformSupport.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Metal/MetalPlatformSupport.cs`

**Sharp edges.**
- The detection is head-generic, not backend-specific: `MetalPlatformSupport`
  forwards to the same scan rather than duplicating it.
- It relies on the head's runtime assembly already being loaded, which is true by
  the time any UI runs but not necessarily earlier. Do not call it from a static
  initializer that runs before the host is built.
- A `Lazy<PlatformHead>` caches the result so the assembly scan happens once.
- For a view model that only needs the operating system rather than the head,
  `SimpleOsInfo` is the simpler answer; see the view-model area.

## View models, commands and threading

### Write bound properties and commands the family way

**When you want this.** You are writing your first `SimpleViewModel` and want the
exact shape the whole repository uses: bound properties, lazily created commands,
and buttons that enable themselves.

**The MVVM shape.** State is `field`-keyword auto-properties whose setters call
`SetProperty(ref field, value)`. Behavior is a `SimpleCommand` per action, created
lazily from a `CanXxx()` predicate and a `DoXxx()` handler. Anything a predicate
reads carries `[AffectsCommands(...)]` naming the commands it gates, so
`CanExecute` refreshes itself with no `RaiseCanExecuteChanged()` anywhere;
`[AffectsProperties(...)]` does the same for computed properties, and
`[AffectsAllCommands]` covers a flag that gates everything.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
[AffectsCommands(nameof(TakePhotoCommand))]
public bool HasFrame
{
    get;
    private set => SetProperty(ref field, value);
}

public CameraDevice SelectedCamera
{
    get;
    set
    {
        if (field != value)
        {
            SetProperty(ref field, value);
            SwitchCamera(value);
        }
    }
}

public string StatusText
{
    get;
    set => SetProperty(ref field, value ?? string.Empty);
} = string.Empty;
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
private SimpleCommand _takePhotoCommand;
public SimpleCommand TakePhotoCommand =>
    (_takePhotoCommand ??= new SimpleCommand(CanTakePhoto, DoTakePhoto));

private bool CanTakePhoto() => (!IsBusy) && IsCaptureMode && HasFrame;

private async Task DoTakePhoto()
{
    if (!CanTakePhoto()) { return; }
    // ...
}

private SimpleCommand _selectColorCommand;
public SimpleCommand SelectColorCommand =>
    (_selectColorCommand ??= new SimpleCommand(CanSelectColor, (Action<object>)DoSelectColor));

private void DoSelectColor(object parameter)
{
    var session = _paintSession;
    if (session != null && parameter is string colorName && session.SelectColor(colorName))
    {
        ActiveColorText = $"Painting with: {session.ActiveColorName}";
    }
}
```

A property can gate a command and a computed property at once:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
[AffectsCommands(nameof(CreateCommand), nameof(LoadWholeTreeCommand))]
[AffectsProperties(nameof(TreePlaceholderVisibility), nameof(TreeVisibility))]
public bool IsConnected
{
    get;
    private set => SetProperty(ref field, value);
}

// ...

private SimpleCommand _createCommand;
public SimpleCommand CreateCommand =>
    (_createCommand ??= new SimpleCommand(CanCreate, DoCreate));

private bool CanCreate() =>
    (!IsBusy)
    && IsConnected
    && (!string.IsNullOrWhiteSpace(OutputFilePath))
    && CheckedCount > 0;
```

The page's side of the contract is a plain binding, with
`UpdateSourceTrigger=PropertyChanged` where a button should follow typing:

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml -->
<TextBox Grid.Column="0" Height="40"
         VerticalAlignment="Center" VerticalContentAlignment="Center"
         Text="{d:Binding MediaAddress, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
<Button Grid.Column="1" Margin="8,0,0,0" Height="40"
        VerticalAlignment="Center" Content="Load"
        Command="{d:Binding LoadCommand}" />
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`,
`PainDiagram/Shared/ViewModels/MainViewModel.cs`,
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`,
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`,
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs`

**Sharp edges.**
- An asynchronous command body needs an explicit cast so the right
  `SimpleCommand` overload is chosen: `(Func<object, Task>)(_ => RunAsync())` or
  `(Func<Task>)(() => StepAsync(...))`. Without it a `Task`-returning lambda binds
  to the synchronous `Action` overload and the command completes immediately while
  the work runs unobserved. A parameterized synchronous command needs
  `(Action<object>)` for the same reason.
- Every `DoXxx()` re-checks its own `CanXxx()` on the first line. `CanExecute` is a
  UI hint, not a guarantee, because a command can also be invoked
  programmatically or while the UI has not refreshed yet.
- `[AffectsCommands]` takes command property names, so renaming a command without
  updating the attribute silently stops refreshing the button. `nameof` keeps that
  honest.
- Commands are kept in explicit backing fields precisely so `Dispose()` can reach
  them. `field ??=` on an expression-bodied command property works too and creates
  the command once; a plain `=> new SimpleCommand(...)` would hand a fresh
  instance to every binding, and `RaiseCanExecuteChanged()` would then update a
  command nothing is bound to.
- A computed companion property (`IsVisualizeMode => !IsCameraMode`) needs an
  explicit `NotifyPropertyChanged` from the setter it depends on, unless the
  source property lists it in `[AffectsProperties]`.
- Setters normalize `null` to `string.Empty`, so predicates never have to
  null-check separately.
- One `CanExecute` in JustBetweenUs deliberately leaves out a validity check: the
  commented-out `IsBase64Text(EnteredText)` in `CanDecrypt` records that including
  it made the Decrypt button flash on and off as the user typed. The check moved
  into the command body, which shows an informational message instead.

### Refresh CanExecute when the gating state is not a bound property

**When you want this.** Your buttons are enabled by facts that live in a model
object, not by properties on the view model, so `[AffectsCommands]` has nothing to
hang on.

**The MVVM shape.** Use `[AffectsAllCommands]` for the one real bound property
that gates everything, and call `RaiseCanExecuteChanged()` explicitly from the
single method that already runs whenever the model moved. Both the predicate and
the body read the model directly, so the view model never mirrors model state into
properties of its own.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    /// <summary>Whether a file picker or document open is in progress (blocks the navigation buttons).</summary>
    [AffectsAllCommands]
    public bool IsBusy
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Main Down: both documents to their next page.</summary>
    public SimpleCommand NextPageCommand => field ??=
        new SimpleCommand(() => !IsBusy && _comparison.CanMoveBothNext,
            (Func<Task>)(() => StepAsync(_comparison.MoveBothNext, renderLeft: true)));

    //Tell the page the view (zoom/pan/page) moved and refresh every button that depends on it
    private void ViewChanged()
    {
        ViewVersion++;
        NotifyPropertyChanged(nameof(ZoomLabel));
        RaiseNavigationCanExecute();
    }

    private void RaiseNavigationCanExecute()
    {
        PreviousPageCommand.RaiseCanExecuteChanged();
        NextPageCommand.RaiseCanExecuteChanged();
        AdjustPreviousCommand.RaiseCanExecuteChanged();
        AdjustNextCommand.RaiseCanExecuteChanged();
        ZoomInCommand.RaiseCanExecuteChanged();
        ZoomOutCommand.RaiseCanExecuteChanged();
        ZoomResetCommand.RaiseCanExecuteChanged();
        PanCommand.RaiseCanExecuteChanged();
    }
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs` (a
private `_isCreatingDocument` field with `DocumentCommand.RaiseCanExecuteChanged()`
at both ends of the run)

**Sharp edges.**
- Funnel every model change through one method. Adding a new kind of change then
  means calling that one method rather than remembering three separate things.
- `[AffectsAllCommands]` handles the busy flag; everything else still needs the
  explicit raise.

### Refresh command enablement in one pass from a headless command model

**When you want this.** Dozens of commands whose enabled state depends on the same
few facts, declared in a headless library rather than as `SimpleCommand`
properties on a view model.

**The MVVM shape.** One method recomputes every command's enabled state from
current state, called from a single "something about the document changed" funnel
that every model event routes through. This is the manual version of what
`[AffectsCommands]` automates.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Actions.cs
/// <summary>
/// Enables and disables commands to match the current document, selection
/// and history state. Upstream drove this from a scattering of event
/// handlers; doing it in one pass makes the rules visible in one place.
/// </summary>
private void UpdateActionSensitivity()
{
    ActionManager actions = PintaCore.Actions;

    bool hasDocument = PintaCore.Workspace.HasOpenDocuments;
    // ...
    foreach (Command command in actions.View.Commands())
    {
        //The visibility toggles stay usable with no document open; only the
        //zoom commands need one.
        if (command is not ToggleCommand)
            command.Sensitive = hasDocument;
    }
    // ...
    Document document = PintaCore.Workspace.ActiveDocument;
    DocumentHistory history = document.History;

    actions.Edit.Undo.Sensitive = history.CanUndo;
    actions.Edit.Redo.Sensitive = history.CanRedo;

    bool hasSelection = document.Selection.Visible;
    actions.Edit.Deselect.Sensitive = hasSelection;
    actions.Image.CropToSelection.Sensitive = hasSelection;
    actions.View.ZoomToSelection.Sensitive = hasSelection;

    int layerCount = document.Layers.Count();
    int currentIndex = document.Layers.CurrentUserLayerIndex;

    actions.Layers.DeleteLayer.Sensitive = layerCount > 1;
    actions.Layers.MergeLayerDown.Sensitive = currentIndex > 0;
    actions.Layers.MoveLayerUp.Sensitive = currentIndex < layerCount - 1;
    actions.Image.Flatten.Sensitive = layerCount > 1;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
/// <summary>
/// One place for "something about the document changed" - the pads and the
/// command enablement both follow from it.
/// </summary>
private void OnDocumentStateChanged()
{
    RefreshLayersPad();
    RefreshHistoryPad();
    UpdateActionSensitivity();
    UpdateSelectionSizeText();
}
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Actions.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- The single funnel is what keeps six different model events from each having to
  know which commands they affect.
- The no-document branch resets the state-dependent commands explicitly, so a
  stale enabled state never survives a document close.
- Prefer `SimpleCommand` with `[AffectsCommands]` when the commands can live on a
  view model; reach for this shape only when the command model is owned by a
  headless library.

### Give each grid cell its own command and lazily loaded thumbnail

**When you want this.** A data-templated list or grid whose template should bind
to its own item, where each item lazily fetches an image and its button may also
depend on application-wide state.

**The MVVM shape.** A cell view model per item, holding display text plus
delegates the owner supplies: what opening the cell does, how its thumbnail bytes
are fetched, and (where needed) whether the action is currently allowed. The
template then binds a plain `{Binding OpenCommand}` and `{Binding Thumbnail}` with
no `ElementName` or ancestor lookups, and the cell type stays independently
testable because it holds delegates rather than a reference to its owner.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AssetCellViewModel.cs
/// <summary>
/// Creates a cell for one asset. The owning view model supplies what opening the asset
/// does (openAsync) and how the thumbnail's bytes are fetched (thumbnailBytesAsync,
/// <c>null</c> for kinds with no thumbnail).
/// </summary>
public AssetCellViewModel(string title, AssetCellKind kind, string kindLabel, string glyph,
    string subtitle, string detailText, object payload,
    Func<AssetCellViewModel, Task> openAsync, Func<Task<byte[]>> thumbnailBytesAsync)
{ /* ... */ }

/// <summary>
/// Opens this cell's asset in the viewer. Living on the cell itself keeps the cell
/// template's binding a plain <c>{Binding OpenCommand}</c> - a template binds to its own item.
/// </summary>
public SimpleCommand OpenCommand => field ??=
    new SimpleCommand((Func<object, Task>)(_ => _openAsync(this)));

/// <summary>The placeholder glyph's visibility (shown until a thumbnail arrives, or always for kinds without one).</summary>
public Visibility PlaceholderVisibility => _thumbnail == null ? Visibility.Visible : Visibility.Collapsed;

public async Task LoadThumbnailAsync()
{
    if (_thumbnail != null || _thumbnailFailed || _thumbnailBytesAsync == null) { return; }

    try
    {
        var bytes = await _thumbnailBytesAsync();
        if (bytes == null) { _thumbnailFailed = true; return; }

        //Back on the UI thread here (the awaiter restores the dispatcher context), which
        //is where BitmapImage wants to be touched.
        var image = new BitmapImage();
        using (var stream = new MemoryStream(bytes))
        {
            await image.SetSourceAsync(stream.AsRandomAccessStream());
        }
        Thumbnail = image;
    }
    catch (Exception)
    {
        //A missing thumbnail is cosmetic; the cell simply keeps its placeholder.
        _thumbnailFailed = true;
    }
}
```

The owner wires the delegates when it builds the list:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
var thumbnailLoader = kind switch
{
    AssetCellKind.Image => (Func<Task<byte[]>>)(() => ReadArchiveBytesAsync(entry.EntryPath)),
    AssetCellKind.Vector => () => ReadSvgThumbnailAsync(entry.EntryPath),
    _ => null,
};

return new AssetCellViewModel(
    entry.Name, kind, kindLabel, glyph, subtitle, sizeText, entry, OpenAssetAsync, thumbnailLoader);
```

**Variant: an application-wide gate on a per-cell command.** PolyHavenBrowser
injects the gate as a delegate too, and pokes every materialized cell when it
changes:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/ModelCellViewModel.cs
public SimpleCommand DownloadCommand => _downloadCommand ??=
    new SimpleCommand(() => _canDownload(), _ => _downloadAsync(this));

/// <summary>
/// Lets the owning view model tell this cell's Download button to re-query its enabled
/// state (called on every cell when a download starts or finishes).
/// </summary>
public void NotifyCanDownloadChanged() => _downloadCommand?.RaiseCanExecuteChanged();
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
public bool IsDownloading
{
    get;
    private set
    {
        SetProperty(ref field, value);
        NotifyPropertyChanged(nameof(DownloadBarVisibility));

        //The download gate lives on each cell's own command; tell every materialized
        //cell to re-query it. (Cells materialized later evaluate the gate fresh anyway.)
        if (Cells is { } cells)
        {
            foreach (var cell in cells) { cell.NotifyCanDownloadChanged(); }
        }
    }
}
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AssetCellViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/BundleCellViewModel.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/ModelCellViewModel.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Converters/NullToVisibilityConverter.cs`
(the same "placeholder until the image arrives" idea done with a converter rather
than a computed `Visibility` property)

**Sharp edges.**
- `BitmapImage` wants to be created and filled on the UI thread. Awaiting the fetch
  restores the dispatcher context, so the construction after the `await` is
  already in the right place; the code says so in a comment.
- Guard on both "already have one" and "already failed", because a lazily filling
  collection may ask a cell to load more than once, and a failed fetch should
  never be retried on every rescroll.
- Cells whose kind has no thumbnail get a `null` loader and return immediately.
- Making the whole card a `Button` bound to the cell's command gives keyboard and
  hover behavior for free.
- The lazy command creation plus `?.` on the refresh means a cell whose button was
  never realized costs nothing.

### Guard a view model constructor for the XAML designer

**When you want this.** The page declares its view model in XAML, so the designer
constructs it too, and the constructor does real work: opening cameras, starting
threads, hitting the network.

**The MVVM shape.** The first line of the constructor is
`if (IsDesignMode(true)) { return; }`. At run time `SetIsDesignMode(false)` has
already been called during application startup, so the guard falls through. In the
designer it returns immediately and only the property initializers run, which is
where design-time values come from.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs
[Microsoft.UI.Xaml.Data.Bindable]
public class MainViewModel : SimpleViewModel
{
    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Debug.WriteLine("Main view model startup.");

        //Load (and, because the player has AutoPlay enabled, start) the default media on startup
        LoadMedia();
    }
    // ...
}
```

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml -->
<Page.DataContext>
    <vm:MainViewModel />
</Page.DataContext>
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs`,
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`,
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs` and
`.../ViewModels/DocumentPaneViewModel.cs`,
`Pinta.Brix/src/Pinta.Brix.Core/ViewModels/MainViewModel.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`,
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs` and
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs` (both wrap the whole body
in `if (!IsDesignMode(true)) { ... }` instead of returning early)

**Sharp edges.**
- The comment is part of the pattern: the guard must be the first line, before any
  field is assigned or any service resolved.
- The pairing is easy to get half right. Without `SetIsDesignMode(false)` in `App`,
  the run-time constructor also returns early and the application silently does
  nothing at all.
- A child view model needs the guard too, and because it returns early in design
  mode its constructor-assigned members stay null then.
- `[Microsoft.UI.Xaml.Data.Bindable]` on the class is what makes the type usable
  as a binding source. Applications that also compile the view model into a native
  head put it behind `#if HAS_CODEBRIX`.

### Kick off async startup loading from the view model constructor

**When you want this.** The page must show something immediately while its data
arrives, and must show a readable message when the load fails.

**The MVVM shape.** The constructor sets up synchronous state and starts one async
method without awaiting it. That method sets bound state, flips a loading flag,
and turns a failure into text on screen rather than an exception.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
public bool IsCatalogLoading
{
    get;
    private set
    {
        SetProperty(ref field, value);
        NotifyPropertyChanged(nameof(CatalogLoadingVisibility));
    }
} = true;

public Visibility CatalogLoadingVisibility => IsCatalogLoading ? Visibility.Visible : Visibility.Collapsed;

public string CatalogStatusText
{
    get;
    private set => SetProperty(ref field, value);
} = "Loading the Poly Haven model catalog…";

private async Task LoadCatalogAsync()
{
    try
    {
        _allModels = await _catalog.GetModelsAsync(CancellationToken.None);
        IsCatalogLoading = false;
        RebuildCells();
    }
    catch (Exception ex)
    {
        CatalogStatusText = $"Could not load the Poly Haven catalog: {ex.Message}";
    }
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
public MainViewModel()
{
    if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

    _catalogService = GetService<AssetCatalogService>();

    _assetsFolder = SettingsService.Get<string>(AssetsFolderKey);
    if (HasAssetsFolder)
    {
        _ = ReloadCatalogAsync();
    }
}
```

**Variant: name the task so a page or a test can await it.**

```csharp
// Adapted from CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
// The sample starts a fire-and-forget Task in the constructor and pads it with a
// fixed Task.Delay before showing its first dialog; this version keeps the same
// steps but names the initialization so a page or a test can await it.
public MainViewModel()
{
    if (!IsDesignMode(true))
    {
        _encryptSvc = GetService<IEncryptionService>();
        // ... fill the picker list and select the first entry ...
        Initialization = InitializeAsync();
    }
}

public Task Initialization { get; private set; } = Task.CompletedTask;

private async Task InitializeAsync()
{
    var defaultKey = await _encryptSvc.GetDefaultKey();
    InvokeOnMainThread(() => EncryptionKey = defaultKey);
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`

**Also shown by.**
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`
(`_ = InitializeAsync();` after setting a "Discovering cameras…" status),
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The discard (`_ = ...`) is deliberate, and every exception is caught inside, so
  nothing is left unobserved. A constructor that awaits would block page
  construction.
- On failure PolyHavenBrowser deliberately leaves the loading indicator visible
  with the error text under it, rather than leaving the user staring at an empty
  grid.
- Work started this early can complete before the page has handed the view model a
  `XamlRoot`, and a dialog raised then has nowhere to attach. JustBetweenUs pads
  its first dialog with a fixed delay and its own comment records that the real
  fix is awaiting a page-readiness signal; prefer a readiness signal in your own
  code.

### Load documents named on the command line during startup

**When you want this.** Repeating the same task, or launching from a script,
without clicking through file pickers first.

**The MVVM shape.** A fire-and-forget async method started from the view-model
constructor, guarded by the busy flag, with every failure funnelled into the
standard error dialog. The head's `Main` does nothing special; the view model
reads the process arguments itself.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    /// <summary>
    /// Convenience for repeated comparisons: launching a head as
    /// PdfSideBySide.LinuxX11 left.pdf right.pdf pre-loads the two documents, so the
    /// user need not browse for them. Anything that goes wrong is reported in the status line.
    /// </summary>
    private async Task OpenStartupDocumentsAsync()
    {
        var arguments = Environment.GetCommandLineArgs();
        if (arguments.Length < 3) { return; }

        IsBusy = true;
        try
        {
            LeftPane.ShowDocument(await _comparison.OpenAsync(DocumentSide.Left, arguments[1]));
            RightPane.ShowDocument(await _comparison.OpenAsync(DocumentSide.Right, arguments[2]));
            UpdateStatus();
            ViewChanged();
            await Task.WhenAll(RenderSideAsync(DocumentSide.Left), RenderSideAsync(DocumentSide.Right));
        }
        catch (Exception e)
        {
            await ShowError(e, "Could not open the documents given on the command line.");
        }
        finally
        {
            IsBusy = false;
        }
    }
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- `Environment.GetCommandLineArgs()` includes the executable at index 0, so the
  two paths are at indices 1 and 2 and the guard is `arguments.Length < 3`. The
  `string[] args` passed to a head's `Main` is never forwarded anywhere.
- Starting it from the constructor means it can finish before the page has handed
  the view model a `XamlRoot`, so an error dialog raised this early has nowhere to
  attach. Deferring the load until the page signals it is ready is safer.

### Set bound properties from a background thread with InvokeOnMainThread

**When you want this.** Work finished off the UI thread and you need to push the
result into a bound property, or call a head-supplied delegate.

**The MVVM shape.** The view model owns the marshalling. Wrap the assignment in
`InvokeOnMainThread`, an inherited `SimpleViewModel` member, so the same code is
correct on every head. Everything that does not touch bound state stays on the
raising thread.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
var defaultKey = await _encryptSvc.GetDefaultKey();
//We can't set a value to EncryptionKey except on the main (UI) thread, because this causes problems on Linux and macOS
InvokeOnMainThread(() => EncryptionKey = defaultKey);
```

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
_session.DrawingChanged += (_, _) => InvokeOnMainThread(() => HasDrawing = _session.HasStrokes);
```

**Where to look.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`
`PainDiagram/Shared/ViewModels/MainViewModel.cs`

**Also shown by.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`,
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`,
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`,
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The comment in JustBetweenUs is the rule worth remembering: assigning a bound
  property off the UI thread appears to work on Windows and fails on Linux and
  macOS. Test the marshalling on the strictest head, not the most forgiving one.
- The same wrapper is used for calling head-supplied bridge delegates, not only
  for property assignment, because a clipboard or canvas API is usually
  main-thread only as well.
- An assignment that also drives `[AffectsCommands]` must be marshalled for the
  same reason: refreshing a command's `CanExecute` touches the UI.
- A `Progress<T>` constructed on the UI thread already marshals its callbacks; one
  handed to a service from a worker thread does not. Check which case you are in
  before adding a second layer of marshalling.

### Hand results from a capture thread through a worker to the UI thread

**When you want this.** Three threads are involved - a sensor callback, a
processing worker, and the UI - and only the view model should decide what the UI
sees.

**The MVVM shape.** The capture-thread handler does the minimum and forwards
pixels to the worker. The worker-thread handler feeds anything thread-safe
straight in, and wraps only what touches bound state in `InvokeOnMainThread` -
and only when it actually changed.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
private void OnFrameArrived(object sender, EventArgs e)
{
    //Capture-thread context: get out fast
    if (!HasFrame)
    {
        InvokeOnMainThread(() => HasFrame = _captureService.HasFrame);
    }

    if (IsCaptureMode)
    {
        InvalidateMainCanvas?.Invoke();
    }
    else
    {
        //Paint Mode: the live feed drives the hand tracker and the little self-view
        var tracker = _tracker;
        if (tracker is { IsRunning: true }
            && _captureService.TryCopyLatestFrame(ref _visionFrame, out var width, out var height))
        {
            tracker.SubmitFrame(_visionFrame, width, height);
        }
        InvalidateSelfView?.Invoke();
    }
}

private void OnTrackingUpdated(object sender, HandTrackingEventArgs e)
{
    //Worker-thread context: marshal all painting decisions onto the UI thread
    var result = e.Result;
    InvokeOnMainThread(() =>
    {
        var session = _paintSession;
        if (IsCaptureMode || session == null) { return; }
        // ... update crosshair, begin/continue/end the stroke ...
        InvalidateMainCanvas?.Invoke();
    });
}
```

PalmVisualizer shows the other half of the trade: when the consumer is itself
thread-safe, only the status line needs the dispatcher, and only when it changes.

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
private void OnTrackingUpdated(object sender, PalmTrackingEventArgs e)
{
    //Worker-thread context: the visualizer's attractor field is thread-safe, so the
    //  palms feed straight in - only the status line needs the UI thread
    var session = _visualizerSession;
    if (IsCameraMode || session == null) { return; }

    var attractors = new List<PalmAttractor>(e.Result.Palms.Count);
    foreach (var palm in e.Result.Palms)
    {
        //Only OPEN palms attract the colors - and the user watched a mirrored
        //  preview, so mirror the palm positions to match
        if (palm.IsOpenPalm)
        {
            attractors.Add(new PalmAttractor(palm.TrackId, 1f - palm.PalmCenterX, palm.PalmCenterY));
        }
    }
    session.UpdatePalms(attractors);

    var openCount = attractors.Count;
    if (openCount != _reportedOpenPalmCount)
    {
        _reportedOpenPalmCount = openCount;
        InvokeOnMainThread(() => StatusText = openCount switch
        {
            0 => "Show the camera your open palm - the colors will gather toward it.",
            1 => "The colors are chasing your open palm - close your hand to set them free.",
            _ => $"The colors are chasing {openCount} open palms - close your hands to set them free.",
        });
    }
}
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Read a field another thread can null into a local first
  (`var tracker = _tracker;`, `var session = _paintSession;`) and then use the
  local, so a concurrent `Dispose()` cannot turn the null check into a race.
- Dispatch only when something changed. Without the `if (openCount != ...)` and
  `if (!HasFrame)` guards, the UI thread takes a dispatch on every processed
  frame.
- Re-check the mode inside the marshalled callback: by the time it runs the user
  may already have pressed Back.
- The frame handler is the one place that decides where a frame goes - repaint in
  one mode, inference in the other - so a single camera feed serves two consumers
  with no duplicated capture.
- Coordinate conventions are reconciled in exactly one place. The tracker reports
  positions in unmirrored camera space and the preview is mirrored by a canvas
  transform, so the view model applies `1f - x` once, where both conventions meet.

### Run a long job from a command with progress cancellation and a busy flag

**When you want this.** The canonical long-running-operation shape: a Run command,
a Cancel command that stays live, a progress bar, a status line, and everything
else disabled.

**The MVVM shape.** `IsRunning` (or `IsBusy`) and `IsCancelling` are
`[AffectsCommands]` properties, so pressing Run disables Run and enables Cancel
with no manual refresh. The service takes an `IProgress<T>` and a
`CancellationToken` and knows nothing about the UI. The `CancellationTokenSource`
is a field, disposed and nulled in a `finally` that also clears the flags.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs
public SimpleCommand RunCommand => field ??= new SimpleCommand(
    () => !IsRunning && Source is not null && SelectedDestination is not null,
    (Func<object, Task>)(_ => RunAsync()));

public SimpleCommand CancelCommand => field ??= new SimpleCommand(
    () => IsRunning && !IsCancelling, _ => DoCancel());

private async Task RunAsync()
{
    // ... choose the output path, build the plan ...

    //The notes on screen belong to the run named in the status bar, so they go the moment a new
    //run takes that line over.
    SetLastRunNotes([]);

    IsRunning = true;
    IsCancelling = false;
    ProgressPercent = 0d;
    IsProgressIndeterminate = true;
    ProgressText = "Starting...";
    StatusText = plan.ToString();

    cancellation = new CancellationTokenSource();
    var progress = new Progress<ConversionProgress>(report =>
    {
        ProgressPercent = report.OverallPercent;
        IsProgressIndeterminate = report.IsIndeterminate;
        ProgressText = report.ToString();
    });

    ConversionOutcome outcome;
    try
    {
        outcome = await runner.RunAsync(plan, progress, cancellation.Token);
    }
    finally
    {
        cancellation.Dispose();
        cancellation = null;
        IsRunning = false;
        IsCancelling = false;
    }

    ProgressPercent = outcome.Succeeded ? 100d : 0d;
    IsProgressIndeterminate = false;
    ProgressText = string.Empty;
    StatusText = outcome.ToString();
    SetLastRunNotes(DescribeOutcome(outcome, destination));

    ConversionFinished?.Invoke(this, outcome);
}

private void DoCancel()
{
    if (cancellation is null)
    {
        return;
    }

    IsCancelling = true;
    ProgressText = "Stopping...";
    cancellation.Cancel();
}
```

Where the service reports from an arbitrary thread, the progress callback does the
marshalling itself:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
private async Task DoCreate()
{
    if (!CanCreate()) { return; }
    // ...
    try
    {
        IsBusy = true;
        ProgressValue = 0;
        // ...
        var progress = new Progress<CreateProgress>(p => InvokeOnMainThread(() =>
        {
            StatusText = p.Message;
            ProgressValue = p.PercentComplete;
        }));

        var result = await _documentSvc.CreateDocumentAsync(request, progress);

        StatusText = $"Saved: {result.OutputFilePath}";
        await ShowInfo(BuildResultMessage(result));
    }
    catch (Exception e)
    {
        StatusText = "Creation failed.";
        await ShowError($"Error while creating the document: {e.Message}");
    }
    finally
    {
        ProgressValue = 0;
        IsBusy = false;
    }
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Models/RenderModels.cs
/// <summary>
/// The stages a render moves through, in order (useful for progress display).
/// </summary>
public enum RenderStage
{
    FetchingArticle = 0,
    ParsingArticle,
    DownloadingImages,
    ComposingBook,
    SavingPdf,
    Done
}

/// <summary>
/// A progress report raised while rendering.
/// </summary>
public sealed record RenderProgress(RenderStage Stage, string Message, int PercentComplete);
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`

**Also shown by.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
(one `IsBusy` naming three commands, an indeterminate bar bound to
`BusyVisibility`, and `IProgress<string>` straight into `StatusText`),
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- A `Progress<T>` created on the UI thread already posts its callbacks back there;
  one whose callback runs wherever the service happens to be needs an explicit
  `InvokeOnMainThread`. Both forms appear in this repository, and each is correct
  for its own service.
- Reset the progress value in `finally`, so a failed run does not leave the bar
  part-filled, and clear the busy flag there too.
- Cancellation should not travel as an exception out of the command. The video
  tool's service catches `OperationCanceledException` itself and returns a
  cancelled outcome, so the view model has one exit path; it also deletes the
  part-written output on cancel and on failure.
- Ask for confirmation before setting the busy flag, so a cancelled overwrite
  prompt never leaves the UI in a busy state.
- A progress record that carries a stage enum as well as a message and a
  percentage lets a UI render a stage list rather than only a bar, and computing
  the percentage as a band per stage stops the bar going backwards between stages.
- Make `IProgress<T>` optional on the service; the offline tests rely on passing
  `null`.

### Report progress across stages when only some of them know a percentage

**When you want this.** An operation has a preparation stage whose length cannot
be known and a working stage that can report a real percentage, and you want one
honest bar.

**The MVVM shape.** A small immutable report type carrying the stage name, its
number, the stage count and a nullable percentage, with `IsIndeterminate` and
`OverallPercent` derived on it. The view model copies three values out of each
report; the progress bar binds `Value` and `IsIndeterminate`.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionProgress.cs
/// <remarks>
/// Every stage that runs FFmpeg reports a real percentage, because FFmpeg says where in the media it
/// has reached and the source's duration is known. A stage that does not run FFmpeg - reading a
/// bespoke container, muxing an intermediate - reports no percentage at all, and the progress bar
/// shows that it is working rather than inventing a number.
/// </remarks>
public sealed class ConversionProgress
{
    // ...
    public bool IsIndeterminate => StagePercent is null;

    /// <remarks>
    /// A stage with no percentage of its own counts as half-done, so the bar still moves forward
    /// when one finishes rather than sitting still until the last stage starts.
    /// </remarks>
    public double OverallPercent
    {
        get
        {
            var within = Math.Clamp(StagePercent ?? 50d, 0d, 100d);
            var completed = Math.Max(0, StageNumber - 1);
            return Math.Clamp(((completed * 100d) + within) / StageCount, 0d, 100d);
        }
    }

    public override string ToString() => StagePercent is null
        ? $"{Stage} ({StageNumber} of {StageCount})"
        : $"{Stage} ({StageNumber} of {StageCount}) - {StagePercent:F0}%";
}
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<ProgressBar Grid.Column="0"
             Height="6"
             Minimum="0"
             Maximum="100"
             Value="{d:Binding Conversion.ProgressPercent}"
             IsIndeterminate="{d:Binding Conversion.IsProgressIndeterminate}"
             VerticalAlignment="Center"
             Margin="0,0,14,0" />
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionProgress.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs`

**Sharp edges.**
- The stage count is fixed for every run, so the bar never rescales mid-operation.
- Where an underlying library reports per-pass, the runner folds pass number and
  pass count into one within-stage percentage before reporting.
- A stage with no percentage counts as half-done, so the bar advances when it ends
  instead of sitting at zero.

### Snapshot view model state before a long running command

**When you want this.** A command takes many seconds and the user is free to
navigate away or change the selection while it runs.

**The MVVM shape.** Copy everything the run needs into locals at the top, so the
run depends on nothing that can change underneath it. Guard re-entry with a flag,
refresh the command at both ends, and announce completion after the `finally` so
the button is live again by the time the user dismisses the dialog.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
private bool CanCreateDocument() => IsModelViewActive && !_isCreatingDocument;

private async Task CreateDocumentAsync()
{
    if (!CanCreateDocument()) { return; }

    //Snapshot everything the document needs: the user can navigate Back (or open a
    //  different model) while it builds, and the run continues from this snapshot.
    var asset = _currentAsset;
    var stats = _currentStats;
    var model = _currentModel;
    if (asset == null || stats == null || model == null) { return; }

    var title = ModelTitle;
    // ... authorLine, description, facts, downloadFolder ...

    // ... pick the output path ...

    _isCreatingDocument = true;
    DocumentCommand.RaiseCanExecuteChanged();
    var saved = false;
    try
    {
        // ... stages 1-4, then: ...
        await Task.Run(() => new MarketingSheetCreator().CreateToFile(request, outputPath));
        DocumentStatusText = $"Saved: {outputPath}";
        saved = true;
    }
    catch (Exception e)
    {
        DocumentStatusText = string.Empty;
        await ShowError(e, $"Could not create the marketing one-sheet for “{title}”.");
    }
    finally
    {
        _isCreatingDocument = false;
        DocumentCommand.RaiseCanExecuteChanged();
    }

    if (saved)
    {
        //Say so plainly: creating the sheet takes a while, and the footer status line is
        //  easy to miss. Announced after the finally block so the Document button is live
        //  again by the time the user dismisses this.
        using var alert = CreateDialog(
            $"The marketing one-sheet for “{title}” has been created.\n\n" +
            $"It was saved to:\n{outputPath}",
            "Document Created");
        _ = await alert.ShowAsync();
    }
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Setting the status line per stage is what makes a multi-second command
  tolerable.
- Announcing success after the `finally` block, rather than inside the `try`, is
  what leaves the button live while the dialog is up.

### Dispose a view model its commands and its bridge delegates

**When you want this.** A view model that holds commands, service references,
delegates the page handed it, and possibly threads and native handles.

**The MVVM shape.** Override `Dispose()`. Dispose and null each command, null
every bridge delegate (each one captures the page and would keep it alive),
unsubscribe every event before disposing its source, release service references
without disposing container singletons, and call `base.Dispose()` last.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
public override void Dispose()
{
    _takePhotoCommand?.Dispose();
    _takePhotoCommand = null;
    // ... the other four commands ...

    PickSaveJpegPathAsync = null;
    InvalidateMainCanvas = null;
    InvalidateSelfView = null;

    if (_tracker != null)
    {
        _tracker.TrackingUpdated -= OnTrackingUpdated;
        _tracker.Dispose();
        _tracker = null;
    }

    var session = _paintSession;
    _paintSession = null;
    session?.Dispose();

    if (_captureService != null)
    {
        _captureService.FrameArrived -= OnFrameArrived;
        _captureService.Dispose();
        _captureService = null;
    }

    base.Dispose();
}
```

The minimal version, for a view model that owns one command and nothing else:

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs
#region | IDisposable implementation |

public override void Dispose()
{
    _loadCommand?.Dispose();
    _loadCommand = null;
    base.Dispose();
}

#endregion
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`,
`PainDiagram/Shared/ViewModels/MainViewModel.cs`,
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Null the field before disposing the object
  (`var session = _paintSession; _paintSession = null; session?.Dispose();`), so a
  callback arriving mid-teardown sees null instead of a disposed object. The same
  applies to an engine session: null it, then `Stop()` it.
- Unsubscribe in both directions. The view model unsubscribes from each source,
  and the library classes null their own event before stopping
  (`TrackingUpdated = null; Stop();`), which guarantees no handler runs during
  teardown.
- Nulling the bridge delegates is what actually releases the page; a delegate
  captured in the page's constructor holds the page alive through the view model
  until it is cleared.
- A container singleton is released, not disposed: the view model drops its
  reference and leaves the lifetime to the container.
- Disposable library objects the view model created itself do get disposed, and a
  field rather than a get-only property is what makes that possible - the public
  property stays `=> _field`, so every consumer's null-conditional access keeps
  working after disposal.
- A command created through the `field` keyword cannot be reached from `Dispose()`,
  so use an explicit field for any command that owns resources.

### Run one render per pane with latest request wins cancellation

**When you want this.** The user is clicking faster than results render, and you
want the newest request to win without older ones painting stale output.

**The MVVM shape.** One `CancellationTokenSource` per independent region. Starting
work cancels the previous one for that region, sets its busy flag, awaits the
service, and only pushes the result if its own token was not cancelled.
`OperationCanceledException` is swallowed silently: it is the expected outcome,
not a fault.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    //One in-flight render per side; a newer page request cancels the older one
    private CancellationTokenSource _leftRender;
    private CancellationTokenSource _rightRender;
    // ...
    private async Task RenderSideAsync(DocumentSide side)
    {
        var document = _comparison.GetDocument(side);
        if (document == null) { return; }

        //Supersede whatever render was in flight for this side
        var previous = side == DocumentSide.Left ? _leftRender : _rightRender;
        previous?.Cancel();
        var cts = new CancellationTokenSource();
        if (side == DocumentSide.Left) { _leftRender = cts; } else { _rightRender = cts; }

        var pane = PaneFor(side);
        pane.SetRendering(true);
        try
        {
            var dpi = View.Zoom.GetRenderDpi(_renderer.Dpi);
            var page = await _renderer.RenderCurrentPageAsync(document, dpi, cts.Token);
            if (!cts.IsCancellationRequested)
            {
                await pane.ShowPageAsync(page);
            }
        }
        catch (OperationCanceledException)
        {
            //A newer page request won; nothing to show for this one
        }
        catch (Exception e)
        {
            await ShowError(e, $"Could not render page {document.CurrentPage} of “{document.FileName}”.");
        }
        finally
        {
            if (!cts.IsCancellationRequested) { pane.SetRendering(false); }
            previous?.Dispose();
        }
    }
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
        //The right document moves on every step; the left only on a "both" step
        var renders = renderLeft
            ? Task.WhenAll(RenderSideAsync(DocumentSide.Left), RenderSideAsync(DocumentSide.Right))
            : RenderSideAsync(DocumentSide.Right);
        await renders;
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Clear the busy flag in `finally` only when this render was not superseded. A
  cancelled render must not turn off a busy indicator that the newer render turned
  on.
- Check the token a second time before showing the result: a service can return a
  cached answer without ever observing cancellation.
- `previous?.Dispose()` disposes the older source, not this one, so the current
  source stays usable. The trade is that the last source per region is never
  disposed; a view model that lived and died repeatedly would want an
  `IDisposable` implementation that cancels and disposes both.
- Running two regions concurrently is safe only because the service locks its
  cache and does its heavy work inside `Task.Run`.

### Ignore a stale async result when the selection moved on

**When you want this.** A selection change starts a fetch, and the user can change
the selection again before it returns.

**The MVVM shape.** Capture the item the request was for; on completion, compare
it against the current selection inside the marshalled callback and drop the
result if it no longer matches. This is the comparison-based counterpart to
cancellation, and it needs no token.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
private async Task LoadPreviewForNodeAsync(NotionPageNodeViewModel node)
{
    try
    {
        var preview = await _documentSvc.LoadPreviewAsync(node.Id);
        InvokeOnMainThread(() =>
        {
            if (SelectedNode != node) { return; } //A newer selection superseded this preview

            PreviewTitle = preview.Title;
            PreviewMeta = string.Join("  ·  ",
                preview.ChildPageCount == 1 ? "1 child page" : $"{preview.ChildPageCount} child pages",
                $"edited {preview.LastEditedTime.ToLocalTime():yyyy-MM-dd}");
            PreviewSnippets = string.Join("\n\n", preview.TextSnippets);

            PreviewCoverSource = null;
            var imageUrl = preview.CoverUrl.Length > 0 ? preview.CoverUrl : preview.IconUrl;
            if (imageUrl.Length > 0)
            {
                try { PreviewCoverSource = new BitmapImage(new Uri(imageUrl)); }
                catch (Exception) { } //A malformed URL just leaves the pane imageless
            }
        });
    }
    catch (Exception e)
    {
        InvokeOnMainThread(() => StatusText = $"Preview failed: {e.Message}");
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- `SelectedNode` is set synchronously before the async work starts, which is what
  makes the comparison meaningful.
- A malformed image URL is swallowed on purpose so the pane still shows its text.

### Debounce a search box before rebuilding a filtered list

**When you want this.** A search field bound with
`UpdateSourceTrigger=PropertyChanged` where rebuilding on every keystroke would
make typing feel heavy.

**The MVVM shape.** The property setter starts a cancellable delay; the next
keystroke cancels the previous one. All of it lives on the view model, and the
page's `TextBox` stays a plain two-way binding.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>The search text; matching cells re-populate shortly after each keystroke.</summary>
public string SearchText
{
    get;
    set
    {
        var newValue = value ?? string.Empty;
        if (newValue == field) { return; }

        SetProperty(ref field, newValue);
        DebounceRebuild();
    }
} = string.Empty;

//Waits a beat after the last keystroke before rebuilding, so typing stays smooth.
private async void DebounceRebuild()
{
    _searchDebounce?.Cancel();
    var debounce = new CancellationTokenSource();
    _searchDebounce = debounce;
    try
    {
        await Task.Delay(300, debounce.Token);
        RebuildCells();
    }
    catch (OperationCanceledException)
    {
        //Superseded by more typing.
    }
}
```

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml -->
<TextBox Width="300" VerticalAlignment="Center"
         PlaceholderText="Search models…"
         Text="{d:Binding SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
         CornerRadius="8" />
```

A neighboring filter uses a suppression flag instead, so repopulating its list for
a new selection does not trigger a rebuild:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
_suppressCategoryRebuild = true;
Categories = categories;
_selectedCategory = AllCategories;
NotifyPropertyChanged(nameof(SelectedCategory));
_suppressCategoryRebuild = false;
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- `async void` is correct here - it is a fire-and-forget UI reaction - and the
  cancellation is caught rather than allowed to escape.
- The setter compares before assigning, so re-setting the same text does not
  restart the timer.
- A discrete choice such as a sort selector rebuilds immediately; only free text
  needs debouncing.
- The suppression flag is needed because assigning the list and resetting the
  selection each raise a change notification that would otherwise rebuild twice.

### Fill a grid lazily as it scrolls

**When you want this.** A collection large enough that materializing every item,
and its thumbnail, up front would stall the window.

**The MVVM shape.** A collection type that owns the full filtered list but adds
only a batch at a time, exposing `HasMoreItems` and a `RequestMore` method. The
page watches the `ScrollViewer` and calls `RequestMore` as the bottom approaches.
Each item starts its own thumbnail fetch when it appears.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AssetCellCollection.cs
[Microsoft.UI.Xaml.Data.Bindable]
public class AssetCellCollection : ObservableCollection<AssetCellViewModel>
{
    //Enough cells to overfill the first screen even on a wide monitor.
    private const int InitialBatch = 36;

    private readonly IReadOnlyList<AssetCellViewModel> _source;

    public AssetCellCollection(IReadOnlyList<AssetCellViewModel> source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));

        RequestMore(InitialBatch);
    }

    public int TotalCount => _source.Count;
    public bool HasMoreItems => Count < _source.Count;

    public void RequestMore(int count)
    {
        var toLoad = Math.Min(count, _source.Count - Count);

        for (var i = 0; i < toLoad; i++)
        {
            var cell = _source[Count];
            Add(cell);

            //Fire-and-forget: the cell fetches its thumbnail in the background and raises
            //a property change when the image arrives.
            _ = cell.LoadThumbnailAsync();
        }
    }
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
//Lazy grid loading: as the grid scrolls within two screens of its bottom edge,
//ask the cell collection to materialize the next batch.
CatalogScroll.ViewChanged += (_, _) =>
{
    var cells = ViewModel?.Cells;
    if (cells == null || !cells.HasMoreItems) { return; }

    var remaining = CatalogScroll.ExtentHeight - CatalogScroll.VerticalOffset - CatalogScroll.ViewportHeight;
    if (remaining < CatalogScroll.ViewportHeight * 2)
    {
        cells.RequestMore(24);
    }
};

//A new cell collection means the user switched bundle, searched or
//re-filtered: jump back to the top.
if (args.PropertyName == nameof(MainViewModel.Cells))
{
    CatalogScroll.ChangeView(null, 0, null, disableAnimation: true);
}
```

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
<!-- Fixed-size cells; the per-row count follows the window width. Cells
     materialize lazily: the page asks for more as this view scrolls
     toward its bottom edge. -->
<ScrollViewer x:Name="CatalogScroll" Grid.Row="1"
              Padding="24,4,24,8"
              VerticalScrollBarVisibility="Auto">
    <StackPanel HorizontalAlignment="Center">
        <ItemsRepeater ItemsSource="{d:Binding Cells}"
                       ItemTemplate="{StaticResource AssetCellTemplate}">
            <ItemsRepeater.Layout>
                <UniformGridLayout Orientation="Horizontal"
                                   MinItemWidth="230" MinItemHeight="248"
                                   MinColumnSpacing="14" MinRowSpacing="14"
                                   ItemsStretch="None" />
            </ItemsRepeater.Layout>
        </ItemsRepeater>
    </StackPanel>
</ScrollViewer>
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AssetCellCollection.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml` and
`Views/MainPage.xaml.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/ModelCellCollection.cs`
and `PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml` (the same
batching collection behind a card grid, built through a factory delegate so the
collection does not know how a cell is constructed)

**Sharp edges.**
- Filtering swaps in a whole new collection instance rather than mutating the
  existing one, which is what makes "scroll back to the top" a single property
  change to watch.
- A threshold of two viewports means a batch is already in place before the user
  reaches the end.
- `RequestMore` is safe to call repeatedly and no-ops once everything is
  materialized.
- The collection type is marked `[Microsoft.UI.Xaml.Data.Bindable]`, as is every
  other bound type in these applications, including plain record types.

### Show and hide panes with computed Visibility properties

**When you want this.** Placeholder text before data arrives and real content
afterwards, or one region of a page showing different content depending on what is
selected, with no value converters in the XAML.

**The MVVM shape.** The view model exposes `Visibility` properties computed from
its own state - `SimpleViewModel` supplies a `GetVisibility(bool)` helper - and
the source property either lists them in `[AffectsProperties]` or notifies them
from its setter. The XAML stacks the panes in the same grid cell and binds each
one's `Visibility`.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
public Visibility PreviewContentVisibility => GetVisibility(SelectedNode is not null);
public Visibility PreviewPlaceholderVisibility => GetVisibility(SelectedNode is null);
public Visibility PreviewCoverVisibility => GetVisibility(PreviewCoverSource is not null);
public Visibility TreePlaceholderVisibility => GetVisibility(!IsConnected);
public Visibility TreeVisibility => GetVisibility(IsConnected);
```

```xml
<!-- From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml -->
<!-- Before Connect: a quiet hint instead of a blank panel -->
<StackPanel Grid.Row="1" HorizontalAlignment="Center" VerticalAlignment="Center"
            Spacing="14" MaxWidth="420" Margin="20"
            Visibility="{d:Binding TreePlaceholderVisibility}">
    <FontIcon Glyph="&#xE8F1;" FontSize="40"
              Foreground="{StaticResource AccentDimBrush}"
              HorizontalAlignment="Center" />
    <TextBlock Text="Connect to see your pages"
               FontSize="15.5" FontWeight="SemiBold" TextAlignment="Center"
               Foreground="{StaticResource TextPrimaryBrush}" />
</StackPanel>

<TreeView Grid.Row="1" Padding="10,0,10,12"
          SelectionMode="None"
          Visibility="{d:Binding TreeVisibility}"
          ItemsSource="{d:Binding RootNodes}"
          ItemTemplate="{StaticResource PageNodeTemplate}" />
```

Where there are several exclusive panes, a private mode enum and one method that
sets it keeps every notification in one place:

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
private enum ViewerMode { None, Image, Model, Text, Audio }

public Visibility ImageViewerVisibility => _viewerMode == ViewerMode.Image ? Visibility.Visible : Visibility.Collapsed;
public Visibility ModelViewerVisibility => _viewerMode == ViewerMode.Model ? Visibility.Visible : Visibility.Collapsed;
public Visibility TextViewerVisibility => _viewerMode == ViewerMode.Text ? Visibility.Visible : Visibility.Collapsed;
public Visibility NoPreviewVisibility => _viewerMode == ViewerMode.None ? Visibility.Visible : Visibility.Collapsed;
public Visibility AudioViewerVisibility => _viewerMode == ViewerMode.Audio ? Visibility.Visible : Visibility.Collapsed;
public Visibility ZoomBarVisibility => ImageViewerVisibility;

private void SetViewerMode(ViewerMode mode, string hint, bool activateViewer = true)
{
    _viewerMode = mode;
    ViewerHint = hint;
    if (mode == ViewerMode.Image)
    {
        ImagePainter.ZoomFactor = 1f;
        ImagePainter.HighlightRegion = null;
        NotifyPropertyChanged(nameof(ZoomText));
    }

    NotifyPropertyChanged(nameof(ImageViewerVisibility));
    NotifyPropertyChanged(nameof(ModelViewerVisibility));
    NotifyPropertyChanged(nameof(TextViewerVisibility));
    NotifyPropertyChanged(nameof(NoPreviewVisibility));
    NotifyPropertyChanged(nameof(AudioViewerVisibility));
    NotifyPropertyChanged(nameof(ZoomBarVisibility));
    NotifyPropertyChanged(nameof(RegionListVisibility));
    NotifyPropertyChanged(nameof(AnimationBarVisibility));

    if (activateViewer)
    {
        IsViewerActive = true;
        InvalidateImageCanvas?.Invoke();
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/NotionPageNodeViewModel.cs`

**Also shown by.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/DocumentPaneViewModel.cs`,
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The placeholder and the real content are siblings in the same grid cell, each
  with its own visibility, rather than one element being swapped in and out.
- Route every path that changes mode through one method, so the notifications live
  in one place instead of being scattered through eight of them.
- KenneyAssetBrowser's two top-level views are also just two grids in the same
  cell with bound visibility, so there is no navigation and no page state to
  restore.
- Unsupported items still open, into a "nothing to preview" mode with an
  explanatory caption, so nothing in the grid is a dead card.

### Load a tree lazily as the user expands it

**When you want this.** A hierarchy that is expensive to enumerate - one API call
per level - and should be fetched only where the user looks.

**The MVVM shape.** Each row is its own small `SimpleViewModel`. A synthetic
placeholder child keeps the expand chevron visible before the real children exist.
Setting `IsExpanded` triggers a one-shot load; the parent view model does the call
and marshals the result back.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/NotionPageNodeViewModel.cs
public NotionPageNodeViewModel(NotionPageNode node, MainViewModel owner)
{
    Node = node;
    _owner = owner;

    if (node?.HasChildren == true)
    {
        //A placeholder child keeps the expand chevron visible until the real
        //  children arrive on first expand
        Children.Add(new NotionPageNodeViewModel());
    }
    // ...
}

private NotionPageNodeViewModel()
{
    IsPlaceholder = true;
}

public bool IsExpanded
{
    get;
    set
    {
        SetProperty(ref field, value);
        if (value) { _ = EnsureChildrenLoadedAsync(); }
    }
}

/// <summary>Loads the real children on first expand (no-op afterwards).</summary>
internal async System.Threading.Tasks.Task EnsureChildrenLoadedAsync()
{
    if (IsPlaceholder || _loadRequested || Node?.HasChildren != true || _owner is null) { return; }
    _loadRequested = true;
    await _owner.LoadChildrenForNodeAsync(this);
}

/// <summary>Replaces the placeholder with the loaded children.</summary>
internal void SetChildren(IEnumerable<NotionPageNodeViewModel> children)
{
    Children.Clear();
    foreach (var child in children) { Children.Add(child); }
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
/// <summary>Loads a node's children from Notion (called on first expand).</summary>
internal async Task LoadChildrenForNodeAsync(NotionPageNodeViewModel node)
{
    try
    {
        var children = await _documentSvc.LoadChildrenAsync(node.Id);
        InvokeOnMainThread(() =>
            node.SetChildren(children.Select(c => new NotionPageNodeViewModel(c, this))));
    }
    catch (Exception e)
    {
        InvokeOnMainThread(() => StatusText = $"Could not load child pages: {e.Message}");
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/NotionPageNodeViewModel.cs`
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- `_loadRequested` is a separate flag from the child count, so a page that turns
  out to have no children is not re-fetched on every expand.
- A "load everything" command walks the same `EnsureChildrenLoadedAsync()` path
  recursively, so there is one loading code path rather than two.
- A failed child load writes to the status line and leaves the row usable; it
  never throws into the expand gesture.

### Confirm and inform from the view model with SimpleViewModel dialogs

**When you want this.** A command needs a yes/no answer, or has something to tell
the user, and you do not want a dialog type in your view model.

**The MVVM shape.** `SimpleViewModel` supplies awaitable `ConfirmDialog`,
`ShowInfo` and `ShowError` helpers, so the command asks and reacts inline. The
page's only contribution is handing the view model a way to reach the XAML root
(see the bridge area). Confirmation is conditional: trivial cases are not
interrupted.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
private async Task DoClear()
{
    if (!CanClear()) { return; }

    var doClear = true;
    if (_paintSession.StrokeCount > 2)
    {
        doClear = await ConfirmDialog(
            "Are you sure you want to clear your painting and start over?",
            "Confirm");
    }

    if (doClear)
    {
        _paintSession.Clear();
        StatusText = "Cleared - paint something new.";
    }
}

private async Task DoGoBack()
{
    if (!CanGoBack()) { return; }

    if (HasDrawing)
    {
        var discard = await ConfirmDialog(
            "Going back to the camera will discard your painting. Are you sure?",
            "Discard painting?");
        if (!discard) { return; }
    }

    LeavePaintMode();
    // ...
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
var outputPath = OutputFilePath.Trim();

//Confirm before clobbering an existing file (requirement: prompt via SimpleDialog)
if (File.Exists(outputPath))
{
    var replace = await ConfirmDialog(
        $"A file already exists at:\n{outputPath}\n\nDo you want to replace it?",
        "Replace existing file?");
    if (!replace)
    {
        StatusText = "Publishing cancelled - the existing file was kept.";
        return;
    }
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
private async Task DoDecrypt()
{
    if (CanDecrypt())
    {
        if (!_encryptSvc.IsBase64Text(EnteredText))
        {
            await ShowInfo("The specified text does not look like it is encrypted.");
        }
        else
        {
            try
            {
                // ... call the service, assign ProcessedText ...
            }
            catch (Exception e)
            {
                await ShowError($"Error while decrypting: {e.Message}");
            }
        }
    }
}
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`PainDiagram/Shared/ViewModels/MainViewModel.cs`
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`

**Also shown by.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
(a result dialog that caps how many warnings it lists and says how many more there
were, so a page full of unsupported content cannot produce an unreadable dialog),
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
(`ShowError(ex, message)` as the single error surface for a whole open path)

**Sharp edges.**
- `ShowError` has two shapes: `ShowError(string)` for a message that is already
  user-ready, and `ShowError(Exception, string)` for "here is what went wrong plus
  context".
- The confirmation happens before the busy flag is set, so a cancelled overwrite
  never leaves the UI busy.
- Confirm at the moment of writing, not at the moment of picking, so a path the
  user typed by hand is covered too. The heads' own pickers have their overwrite
  prompts suppressed precisely so this is the single confirmation the user sees.
- A threshold rather than a blanket prompt keeps a destructive-action confirmation
  from becoming noise: PainDiagram and WebcamPainter both skip it for two strokes
  or fewer.
- A repeated informational message can be shown only the first time, behind a
  private flag, so an action the user repeats does not nag.
- Long multi-line dialog bodies are not portable; JustBetweenUs records that on
  one mobile platform the text is truncated to a maximum number of lines.

### Prompt before discarding unsaved work

**When you want this.** An application with dirty documents and more than one way
to close one.

**The MVVM shape.** One async method returning a three-way result (save, discard,
cancel), one close method that consumes it, and one close-all loop over the first.
Every close path in the application funnels through the same method.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Dialogs.cs
private enum SaveConfirmation
{
    Save,
    Discard,
    Cancel,
}

/// <summary>
/// Closes a document, prompting first when it has unsaved changes.
/// </summary>
/// <returns>False when the user cancelled.</returns>
private async Task<bool> CloseDocumentAsync(Document document)
{
    if (document is null) { return true; }

    if (document.IsDirty)
    {
        switch (await ConfirmDiscardAsync(document))
        {
            case SaveConfirmation.Cancel:
                return false;

            case SaveConfirmation.Save:
                //A failed or cancelled save must not lose the document.
                if (!await document.Save(saveAs: false)) { return false; }
                break;
        }
    }

    PintaCore.Workspace.CloseDocument(document);
    return true;
}

private async Task<bool> CloseAllAsync()
{
    foreach (Document document in PintaCore.Workspace.OpenDocuments.ToList())
    {
        if (!await CloseDocumentAsync(document)) { return false; }
    }

    return true;
}
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Dialogs.cs`
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`

**Sharp edges.**
- The three-button dialog maps its primary, secondary and close results to save,
  discard and cancel; the dismiss case must fall into cancel, not discard.
- The close-all loop iterates a snapshot, because closing mutates the collection.
- Both the tab close button and the window close funnel here, so there is one
  place the behavior can be wrong.

### Gate an action behind a chosen folder and explain the gate with a dialog

**When you want this.** An action cannot run until the user has supplied
something, and you want them told why rather than shown a dead button.

**The MVVM shape.** The view model owns the gate, the picker command and the
explanation. The gated command still executes; it just explains itself and
returns.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
public bool HasDownloadFolder => !string.IsNullOrWhiteSpace(_downloadFolder);

/// <summary>The folder-picker button's caption: an invitation, or the chosen path.</summary>
public string DownloadFolderLabel => HasDownloadFolder ? _downloadFolder : "Choose download folder…";

public SimpleCommand PickFolderCommand => field ??=
    new SimpleCommand((Func<object, Task>)(_ => PickFolderAsync()));

private async Task PickFolderAsync()
{
    var picker = new FolderPicker
    {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
    };
    picker.FileTypeFilter.Add("*");

    var folder = await picker.PickSingleFolderAsync();
    if (folder == null) { return; }

    //Same encoding trap as the save picker: a folder called "My Models" would otherwise
    //  come back as "My%20Models" and every download would go to the wrong place.
    _downloadFolder = FileDialogHelper.ToFileSystemPath(folder.Path);
    NotifyPropertyChanged(nameof(HasDownloadFolder));
    NotifyPropertyChanged(nameof(DownloadFolderLabel));
}

private async Task DownloadAsync(ModelCellViewModel cell)
{
    if (cell == null || IsDownloading) { return; }

    if (!HasDownloadFolder)
    {
        using (var alert = CreateDialog(
            "Downloading is disabled until you choose a download folder.\n\n" +
            "Use the folder button at the top of the window to pick where models should be saved.",
            "Choose a Download Folder"))
        {
            _ = await alert.ShowAsync();
        }
        return;
    }
    // ... download, then open the Model View ...
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Helpers/FileDialogHelper.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
(the same folder gate, with the chosen folder remembered between runs)

**Sharp edges.**
- Dispose the dialog after showing it; `using` is enough.
- `FileTypeFilter.Add("*")` is required on the folder picker even though it
  filters nothing.
- The picker's returned path needs decoding before anything touches the disk; see
  the bridge area.
- The button's caption doubles as the state display: an invitation before, the
  chosen path after.

### Report a failure as status text instead of throwing

**When you want this.** A user-entered value can be invalid and you want the
application to say so rather than crash or open a dialog.

**The MVVM shape.** The operation is wrapped in try/catch inside the view model.
On success it sets both the result property and a status string; on failure it
sets only the status string, leaving the previous good state in place. A
`TextBlock` bound to the status property is the whole UI for it.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs
private void LoadMedia()
{
    try
    {
        var uri = new Uri(MediaAddress);
        PlayerSource = MediaSource.CreateFromUri(uri);
        StatusText = $"Loaded: {uri}";
    }
    catch (Exception ex)
    {
        StatusText = $"Cannot load '{MediaAddress}': {ex.Message}";
    }
}

public string StatusText
{
    get;
    private set => SetProperty(ref field, value ?? string.Empty);
} = "Ready";
```

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml -->
<TextBlock Grid.Row="2" Text="{d:Binding StatusText}" />
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`

**Also shown by.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
(`StatusText = $"Could not load the ... sample: {ex.Message}"`)

**Sharp edges.**
- The status property has a public getter and a private setter, so only the view
  model writes it.
- On failure the previous good state stays. Whether that is what you want is an
  application decision; if not, clear it in the catch.
- Be honest about what the status covers. MediaPlayerDemo's covers only URI
  construction and source creation, and says nothing about whether the media
  actually plays.

### Report a domain rule violation as a typed exception the view model can catch

**When you want this.** A model-level rule needs a user-facing message, and the
view model needs to tell that case apart from a real failure.

**The MVVM shape.** The library declares its own exception type and throws it
wherever the application can say something better than the underlying library
can, with the message already phrased for a human. The view model catches that
type first and shows the message; anything else falls into a generic handler with
its own context sentence.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/DuplicateDocumentException.cs
public sealed class DuplicateDocumentException : InvalidOperationException
{
    public DuplicateDocumentException(string filePath, DocumentSide alreadyOpenSide)
        : base($"“{Path.GetFileName(filePath)}” is already selected as " +
               $"{DescribeSide(alreadyOpenSide)}; choose a different PDF for " +
               $"{DescribeSide(alreadyOpenSide == DocumentSide.Left ? DocumentSide.Right : DocumentSide.Left)}.")
    {
        FilePath = filePath;
        AlreadyOpenSide = alreadyOpenSide;
    }
    // ...
}
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
        catch (DuplicateDocumentException e)
        {
            //The same file cannot be compared with itself; the pane keeps what it had
            await ShowError(e.Message);
        }
        catch (Exception e)
        {
            await ShowError(e, "Could not open the PDF document.");
        }
```

One exception type can serve a whole service layer:

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/VideoToolProcessingException.cs
/// <summary>
/// Thrown when a file cannot be probed, a conversion cannot be planned, or a conversion fails in a
/// way this application can explain in a sentence.
/// </summary>
public class VideoToolProcessingException : Exception
{
    public VideoToolProcessingException(string message) : base(message) { }

    public VideoToolProcessingException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs
catch (OperationCanceledException)
{
    DeletePartialOutput(plan.OutputPath);
    return ConversionOutcome.Cancelled(stopwatch.Elapsed, notes);
}
catch (VideoToolProcessingException exception)
{
    DeletePartialOutput(plan.OutputPath);
    return ConversionOutcome.Failed(exception.Message, stopwatch.Elapsed, notes);
}
catch (Exception exception)
{
    DeletePartialOutput(plan.OutputPath);
    return ConversionOutcome.Failed(exception.Message, stopwatch.Elapsed, notes);
}
finally
{
    DeleteFolder(workingFolder);
}
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/DuplicateDocumentException.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/VideoToolProcessingException.cs`

**Sharp edges.**
- Carry the facts as properties, not only as a message, so a different UI could
  phrase the same failure differently.
- Throw before the side effect, so the failed operation leaves the previous state
  untouched - the pane keeps whatever it had.
- `OperationCanceledException` is always caught before the general handlers, so a
  cancel is never reported as a failure.
- A service can also refuse to let any exception out at all, turning each case
  into an outcome value so its caller has a single exit path.
- Every message names the thing that failed and says what to do about it, not
  only what went wrong.

### Compose a page from a parent view model and child view models

**When you want this.** A window has two or more regions that each own real state,
and you want them separate without giving up one data context.

**The MVVM shape.** The parent exposes each child as a get-only property, creates
them in its constructor, and owns the one thing they share. The children hold
bindable state and the commands that belong to them; the parent passes a
command's body in as a delegate and pushes state through `internal` methods.
Children talk upward through an event rather than a back-reference. XAML binds
through the parent with dotted paths, or scopes a region with its own
`DataContext`.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs
public MainViewModel()
{
    if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

    probe = GetService<IMediaProbe>() ?? new MediaProbe();

    Playback = new PlaybackViewModel();
    Conversion = new ConversionViewModel();
    Conversion.ConversionFinished += OnConversionFinished;
}

/// <summary>The player half: what is open, the transport, the chapters and the captions.</summary>
public PlaybackViewModel Playback { get; }

/// <summary>The conversion half: the destination, the size, the action and the progress.</summary>
public ConversionViewModel Conversion { get; }

/// <summary>The file the player is showing and the conversion panel is set up for.</summary>
[AffectsCommands(nameof(RemoveCommand))]
public SourceMediaInfo SelectedItem
{
    get;
    set
    {
        SetProperty(ref field, value);
        Conversion.Source = value;
        Playback.Open(value);
        NotifyPropertyChanged(nameof(EmptyLibraryVisibility));
    }
}
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<Page.DataContext>
    <vm:MainViewModel />
</Page.DataContext>
<!-- ... -->
<Button Content="Play"
        Style="{StaticResource TransportButton}"
        Command="{d:Binding Playback.PlayCommand}" />
<!-- ... -->
<ComboBox HorizontalAlignment="Stretch"
          PlaceholderText="Choose a format"
          ItemsSource="{d:Binding Conversion.Destinations}"
          SelectedItem="{d:Binding Conversion.SelectedDestination, Mode=TwoWay}"
          ItemTemplate="{StaticResource LabelTemplate}" />
```

Two identical regions are the same idea with a scoped `DataContext`:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    public MainViewModel()
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Debug.WriteLine("Main view model startup.");

        LeftPane = new DocumentPaneViewModel("Document 1", () => BrowseAsync(DocumentSide.Left));
        RightPane = new DocumentPaneViewModel("Document 2", () => BrowseAsync(DocumentSide.Right));
        _ = OpenStartupDocumentsAsync();
    }

    /// <summary>The left pane - Document 1.</summary>
    public DocumentPaneViewModel LeftPane { get; }

    /// <summary>The right pane - Document 2.</summary>
    public DocumentPaneViewModel RightPane { get; }
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/DocumentPaneViewModel.cs
    public DocumentPaneViewModel(string title, Func<Task> browse)
    {
        if (IsDesignMode(true)) { return; } //Leave as the first line of constructor

        Title = title;
        BrowseCommand = new SimpleCommand(browse);
    }
    // ...
    /// <summary>Shows document (or clears the pane when it is null).</summary>
    internal void ShowDocument(PdfPageDocument document)
    {
        FilePath = document?.FilePath;
        PagePixelWidth = 0;
        PagePixelHeight = 0;
        PageImage = null;
        UpdatePageLabel(document);
    }
```

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml -->
        <Grid Grid.Column="0" DataContext="{d:Binding LeftPane}" RowSpacing="6">
            <!-- ... -->
                <Button Content="{d:Binding BrowseLabel}" Command="{d:Binding BrowseCommand}" FontWeight="SemiBold"
                        Height="24" MinHeight="0" Padding="8,0" />
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs` and
`.../ViewModels/DocumentPaneViewModel.cs`

**Sharp edges.**
- The design-mode guard belongs in the child constructor too. Because the child
  returns early in design mode, members its constructor would assign stay null
  then.
- Child state-changing methods are `internal`, not `public`, so only the parent
  and the test assembly can push into them; bindings only read.
- The children in CodeBrixVideoTool live in different assemblies from the parent
  and from each other, which is what makes them testable in isolation.
- A child talks upward by raising an event the parent subscribes to, never by
  holding a reference to the parent.
- Get-only child properties that are never reassigned keep the XAML's scoped
  `DataContext` bindings valid for the life of the page.

### Notify a value typed bindable property by hand

**When you want this.** A bindable property is a `double`, an `enum` or another
value type and `SetProperty` will not take it.

**The MVVM shape.** Compare, assign, notify - in the setter, with a comment saying
why. Everything else about the property stays the same.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs
public QualityLevel SelectedQuality
{
    get;
    set
    {
        //SetProperty takes reference types only; compare-and-notify by hand, as ProgressPercent does.
        if (field == value) { return; }
        field = value;
        NotifyPropertyChanged(nameof(SelectedQuality));
    }
} = QualityLevel.Good;

public double ProgressPercent
{
    get;
    private set
    {
        //No SetProperty overload takes a double; compare-and-notify by hand.
        if (field.Equals(value)) { return; }
        field = value;
        NotifyPropertyChanged(nameof(ProgressPercent));
    }
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs`

**Sharp edges.**
- `bool` properties in the same file do use `SetProperty(ref field, value)`, so
  the restriction is not simply "value types". Check for an overload before
  assuming.
- An enum-valued property has a dedicated helper, `SetEnumProperty()`; see the
  picker blueprint.
- The `field` keyword is used throughout, with the property's initializer after
  the closing brace.

### Bind a picker to enum values with or without friendly labels

**When you want this.** A pick-one-of-several control whose choices are the
members of an enum.

**The MVVM shape.** When the member names are already the text you want, expose a
read-only list of the offered values and a two-way selected-value property set
through `SetEnumProperty()`; the page binds `ItemsSource` and `SelectedItem` with
no template, label list or converter. When you need friendlier text, derive a
small class from `SimpleEnumInfo<TEnum>` that ties each member to a description.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs
//The stretch modes offered by the ComboBox. The Stretch enum's member names ("Uniform",
//  "UniformToFill", "Fill", "None") are exactly the text we want shown, so the ComboBox can
//  bind straight to the enum values with no separate label list.
public IReadOnlyList<Stretch> StretchOptions { get; } =
[
    Stretch.Uniform,
    Stretch.UniformToFill,
    Stretch.Fill,
    Stretch.None
];

//The player's stretch mode, two-way bound to the ComboBox's SelectedItem.
public Stretch SelectedStretch
{
    get;
    set => SetEnumProperty(ref field, value);
} = Stretch.Uniform;
```

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml -->
<ComboBox Grid.Column="2" Margin="8,0,0,0" Height="40"
          VerticalAlignment="Center"
          ItemsSource="{d:Binding StretchOptions}"
          SelectedItem="{d:Binding SelectedStretch, Mode=TwoWay}" />
```

**Variant: labeled members with SimpleEnumInfo.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/EncryptionMode.cs
public class EncryptionMode : SimpleEnumInfo<EncryptionMode.CryptAlgorithm>
{
    public enum CryptAlgorithm
    {
        [SimpleEnum<EncryptionMode>(nameof(EncryptionMode.Aes))]
        Aes = 0,

        [SimpleEnum<EncryptionMode>(nameof(EncryptionMode.TripleDes))]
        TripleDes,

        [SimpleEnum<EncryptionMode>(nameof(EncryptionMode.Twofish))]
        Twofish,
    }

    public static EncryptionMode Aes => new(CryptAlgorithm.Aes,
        "AES Standard Encryption (Secure)");

    public static EncryptionMode TripleDes => new(CryptAlgorithm.TripleDes,
        "Triple DES (Obsolete, insecure)");

    public static EncryptionMode Twofish => new(CryptAlgorithm.Twofish,
        "Twofish Encryption (Very secure)");

    public EncryptionMode(CryptAlgorithm algorithm, string description)
        : base(algorithm) =>
        Description = description?.Trim();

    public static Dictionary<CryptAlgorithm, EncryptionMode> GetDictionary() =>
        GetDictionary<EncryptionMode>();
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
private readonly Dictionary<EncryptionMode.CryptAlgorithm, EncryptionMode> _encryptionModeDictionary =
    EncryptionMode.GetDictionary();

public List<string> EncryptionModes { get; } = new();

public string SelectedEncryptionModeText
{
    get => _selectedEncryptionModeText;
    set
    {
        SetProperty(ref _selectedEncryptionModeText, value);
        _selectedEncryptionMode = _encryptionModeDictionary
            .Single(s => s.Value.Description == value)
            .Key;
    }
}
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`
`JustBetweenUs/Shared/ViewModels/EncryptionMode.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml`

**Also shown by.**
`JustBetweenUs/Mobile/Views/MainPage.xaml` (a MAUI `Picker` binds identically,
with no view-model change),
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs` (a typed option list
projected to display names, so no application type leaks into XAML)

**Sharp edges.**
- Enum-valued bound properties use `SetEnumProperty()`, not `SetProperty()`.
- A curated list written out by hand keeps unwanted members out of the picker and
  fixes the display order; `Enum.GetValues()` gives you neither.
- Binding the description string rather than the object means the setter has to
  map text back to the enum with a `Single()` lookup, which throws if two members
  ever share a description. Binding the object and using `DisplayMemberPath`
  avoids that.

### Stop a two way bound selection from commanding the control back

**When you want this.** A drop-down both drives a control and follows it, so
setting the selection from the control's own event must not turn around and
command the control.

**The MVVM shape.** One suppression field on the view model. The selection setter
acts on the surface only when the flag is false; every place the view model sets
the selection itself - following a change, refreshing the list, clearing on close -
sets the flag inside a `try`/`finally`.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/ViewModels/PlaybackViewModel.cs
public ChapterEntry SelectedChapter
{
    get;
    set
    {
        SetProperty(ref field, value);
        if (!suppressSelectionChanges && value is not null)
        {
            surface?.SeekToChapter(value.Index);
        }
    }
}

private void OnChapterChanged(object sender, EventArgs e)
{
    var index = surface?.CurrentChapterIndex ?? -1;
    if (index < 0 || index >= Chapters.Count)
    {
        return;
    }

    //The drop-down follows playback; setting it here must not seek back to where it already is.
    suppressSelectionChanges = true;
    try
    {
        SelectedChapter = Chapters[index];
    }
    finally
    {
        suppressSelectionChanges = false;
    }
}
```

The same problem in a tabbed shell is solved by comparing before pushing, so the
model event and the control event cannot ping-pong:

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
private void DocumentTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (DocumentTabs.SelectedItem is not TabViewItem tab) { return; }
    Document document = documentTabs.FirstOrDefault(kv => kv.Value == tab).Key;
    if (document is null) { return; }
    int index = PintaCore.Workspace.OpenDocuments.IndexOf(document);
    if (index >= 0 && index != PintaCore.Workspace.ActiveDocumentIndex)
    {
        PintaCore.Workspace.SetActiveDocument(index);
    }
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/ViewModels/PlaybackViewModel.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
(a `_suppressCategoryRebuild` flag around repopulating a filter list),
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs` (a guard flag around
programmatic history-list selection changes)

**Sharp edges.**
- Every write path needs the flag, including teardown: clearing a collection and
  then nulling the selection would otherwise command the control on the way down.
- `try`/`finally` around each block, so an exception cannot leave the flag set.

### Alert and revert when the user picks an unsupported option

**When you want this.** A picker offers something the running platform cannot do,
and you want the user to learn why it is unavailable rather than silently not see
the option.

**The MVVM shape.** The dropdown lists every choice. The bound setter is
optimistic: it shows the new selection at once and raises the change, then an
async method validates. On failure it shows a dialog and writes the previous value
back through the backing field plus a manual notification, which snaps the control
back.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
public string SelectedRenderEngineName
{
    get => _selectedRenderEngineName;
    set
    {
        if (string.IsNullOrEmpty(value) || value == _selectedRenderEngineName) { return; }

        //Optimistic: show the new selection at once; SwitchEngineAsync reverts it if the
        //engine is unsupported or fails to initialize.
        _selectedRenderEngineName = value;
        NotifyPropertyChanged(nameof(SelectedRenderEngineName));
        _ = SwitchEngineAsync(value);
    }
}

private async Task SwitchEngineAsync(string engineName)
{
    if (!Enum.TryParse<RenderEngineKind>(engineName, out var kind) || kind == _currentEngineKind) { return; }

    if (IsBusy)
    {
        //The dropdown is disabled while busy; this is just a belt-and-braces revert.
        RevertEngineSelection();
        return;
    }

    if (!_engineSelector.IsSupported(kind))
    {
        //The unsupported engine differs by platform: Vulkan is excluded on macOS, Metal is
        //excluded everywhere except macOS - so name whichever one was picked.
        using (var alert = CreateDialog(
            $"{kind} rendering is not available on this platform.", $"{kind} Rendering"))
        {
            _ = await alert.ShowAsync();
        }
        RevertEngineSelection();
        return;
    }
    // ...
}

private void RevertEngineSelection()
{
    _selectedRenderEngineName = _currentEngineKind.ToString();
    NotifyPropertyChanged(nameof(SelectedRenderEngineName));
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The revert writes the backing field directly and raises the notification by
  hand; going through the public setter would re-enter the validation.
- The revert target is the currently active choice, not a hard-coded default, so a
  second failed switch returns to whatever is really running.
- The control is also disabled while busy, and the method still re-checks it.

### Offer only the choices that make sense for the current selection

**When you want this.** Two drop-downs whose contents depend on what is selected,
rebuilt whenever the selection changes.

**The MVVM shape.** One private refresh method, called from the source property's
setter. It clears and refills both collections from static rules that live in
plain classes, selects the first row of each, and notifies the derived text
properties. The rules are static methods, so tests can prove them without a view
model.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs
private void RefreshForSource()
{
    Destinations.Clear();
    Resolutions.Clear();

    if (Source is null)
    {
        SelectedDestination = null;
        SelectedResolution = null;
        NotifyPropertyChanged(nameof(PanelVisibility));
        NotifyPropertyChanged(nameof(RouteText));
        return;
    }

    foreach (var destination in MediaFormats.DestinationsFor(Source.Format))
    {
        Destinations.Add(new DestinationOption(destination));
    }

    foreach (var rung in ResolutionLadder.Build(Source.Width, Source.Height))
    {
        Resolutions.Add(rung);
    }

    SelectedDestination = Destinations.Count > 0 ? Destinations[0] : null;
    SelectedResolution = Resolutions.Count > 0 ? Resolutions[0] : null;

    NotifyPropertyChanged(nameof(PanelVisibility));
    NotifyPropertyChanged(nameof(RouteText));
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs
public static IReadOnlyList<MediaFormatKind> DestinationsFor(MediaFormatKind source)
{
    if (source == MediaFormatKind.Unknown)
    {
        return [];
    }

    var destinations = new List<MediaFormatKind>();
    foreach (var candidate in SupportedFormats)
    {
        if (candidate != source)
        {
            destinations.Add(candidate);
        }
    }

    if (IsSupportedFormat(source))
    {
        destinations.Add(MediaFormatKind.Mp4);
    }

    return destinations;
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/DestinationOption.cs`

**Sharp edges.**
- Each drop-down row type carries a `Label` and overrides `ToString()` to return
  it, so a `ComboBox` shows something sensible with or without an item template.
- The action button's caption is derived rather than stored: it asks the rules
  what the operation is called and falls back to a neutral word when the pair is
  one the application does not offer.

### Settle an operation in a plan before running any of it

**When you want this.** You want the "can this be done, and what exactly will
happen" question answered in one testable place, separately from the doing.

**The MVVM shape.** A static `Create()` that validates and returns an immutable
plan carrying every derived answer, plus a human-readable list of steps. The view
model catches one exception type from it and puts the message in the status bar;
the runner reads the plan and branches on nothing else.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Planning/ConversionPlanner.cs
public static ConversionPlan Create(
    SourceMediaInfo source,
    MediaFormatKind destination,
    string outputPath,
    ResolutionOption resolution,
    QualityLevel quality = QualityLevel.Good)
{
    ArgumentNullException.ThrowIfNull(source);

    if (string.IsNullOrWhiteSpace(outputPath))
    {
        throw new VideoToolProcessingException("A conversion needs somewhere to put its result.");
    }

    if (source.Format == destination)
    {
        throw new VideoToolProcessingException(
            $"'{source.FileName}' is already {MediaFormats.DisplayName(destination)}, so there is nothing to convert.");
    }

    ConversionOperationKind operation;
    try
    {
        operation = MediaFormats.OperationFor(source.Format, destination);
    }
    catch (ArgumentException exception)
    {
        throw new VideoToolProcessingException(exception.Message, exception);
    }

    if (PathsMatch(source.Path, outputPath))
    {
        throw new VideoToolProcessingException("A conversion cannot write over the file it is reading.");
    }

    var chosen = resolution ?? ResolutionOption.Original(
        ResolutionLadder.MakeEven(source.Width), ResolutionLadder.MakeEven(source.Height));

    return new ConversionPlan(source, destination, outputPath, chosen, quality, operation,
        DescribeSteps(source, destination, operation, chosen, quality));
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Planning/ConversionPlan.cs
public TargetAudioCodec AudioCodec => MediaFormats.AudioCodecFor(Destination);

public int AudioChannels => MediaFormats.AudioChannelsFor(Destination, Source.AudioChannels);

public bool DownmixesAudio => Source.HasAudio && AudioChannels < Source.AudioChannels;

public TargetVideoCodec VideoCodec => MediaFormats.VideoCodecFor(Destination);

/// <summary>
/// True when the source is a Mode 2 file, which FFmpeg cannot open and which therefore has to be
/// demultiplexed and re-wrapped before anything else can happen.
/// </summary>
public bool RequiresMode2Extraction => Source.Format == MediaFormatKind.CodeBrixMode2;

public bool IsResized => Resolution is { IsOriginal: false };
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Planning/ConversionPlanner.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Planning/ConversionPlan.cs`

**Sharp edges.**
- Everything the runner branches on is a property of the plan, so the runner reads
  as a straight line and the branching is testable without doing any work.
- The step descriptions the plan carries are the same sentences the status line
  and the run notes show, so the explanation and the behavior come from one place.
- Policy limits belong to the destination rather than to the underlying codec, so
  that adding a second destination using the same codec does not inherit the first
  one's limit by accident.

### Report the host operating system from the view model

**When you want this.** A diagnostics or About screen that proves which operating
system and runtime the user is on.

**The MVVM shape.** `SimpleOsInfo.GatherInfo()` is awaited once, cached in a
field, and formatted into a string the view model shows through its own dialog
helper. No head-specific code at all.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
public SimpleCommand ShowOsInfoCommand =>
    (field ??= new SimpleCommand(DoShowOsInfo));

private async Task DoShowOsInfo()
{
    _osInfo ??= await SimpleOsInfo.GatherInfo(withConsoleOutput: false);
    var sb = new StringBuilder();
    sb.AppendLine($"Currently running on: {_osInfo.PlatformOsName}");
    sb.AppendLine($"Operating system description: {_osInfo.OsDescription}");
    sb.AppendLine($"Operating system version: {_osInfo.OsVersion}");
    sb.AppendLine($"Product name: {_osInfo.ProductName}");
    sb.AppendLine($"Product name (for display): {_osInfo.ProductNameDisplay}");

    sb.AppendLine($"Running as user: {_osInfo.RunningAsUser}{((_osInfo.IsAdminUser is true) ? " (local admin)" : "")}");
    sb.AppendLine($"DotNet version: {_osInfo.DotNetVersion}");
    sb.AppendLine($"Platform architecture: {_osInfo.PlatformArchitecture}");

    await ShowInfo(sb.ToString());
}
```

**Where to look.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`

**Sharp edges.**
- `GatherInfo` takes a `withConsoleOutput` flag; pass false unless you want the
  same report on the console.
- This command uses the `field` keyword for its lazy backing store because it has
  nothing to dispose; the commands beside it use explicit fields so `Dispose()`
  can reach them.
- To learn which head is running rather than which operating system, see the
  head-detection blueprint in the startup area.

### Cache rendered results with a bounded most recently used cache

**When you want this.** Stepping back and forth between neighboring items should
not re-render anything, but you do not want an unbounded pile of decoded bitmaps
either.

**The MVVM shape.** The cache is a private detail of the service, not of the view
model. It is keyed by everything that affects the output, guarded by a lock
because work runs on worker threads, and exposes only a count and a `ClearCache()`
for tests and for the resolution setter.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs
    private readonly Dictionary<string, RenderedPage> _cache = new();
    private readonly LinkedList<string> _cacheOrder = new(); //Most recently used at the front
    private readonly Lock _cacheLock = new();
    // ...
    private static string CacheKey(PdfPageDocument document, int pageNumber, int dpi) =>
        $"{document.FilePath}|{pageNumber}|{dpi}";

    private bool TryGetCached(string key, out RenderedPage rendered)
    {
        lock (_cacheLock)
        {
            if (!_cache.TryGetValue(key, out rendered)) { return false; }
            _cacheOrder.Remove(key);
            _cacheOrder.AddFirst(key);
            return true;
        }
    }

    private void AddToCache(string key, RenderedPage rendered)
    {
        if (CacheCapacity < 1) { return; }
        lock (_cacheLock)
        {
            if (_cache.ContainsKey(key)) { _cacheOrder.Remove(key); }
            _cache[key] = rendered;
            _cacheOrder.AddFirst(key);
            while (_cache.Count > CacheCapacity)
            {
                var oldest = _cacheOrder.Last.Value;
                _cacheOrder.RemoveLast();
                _cache.Remove(oldest);
            }
        }
    }
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs
    public int Dpi
    {
        get;
        set
        {
            var dpi = value < 1 ? DefaultDpi : value;
            if (field == dpi) { return; }
            field = dpi;
            ClearCache();
        }
    } = DefaultDpi;
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/PageRendererTests.cs`

**Sharp edges.**
- The resolution is part of the key and changing the default also clears the
  cache. Both are needed: the key stops a low-resolution result being served for a
  high-resolution request, the clear stops stale entries accumulating.
- A capacity below one disables caching entirely rather than throwing; the
  constructor clamps.
- `System.Threading.Lock` is used rather than locking on an arbitrary object.
- A cache hit returns the same instance, so a returned record must never be
  mutated.

### Signal a non property model change to the view with a version counter

**When you want this.** The thing that changed is an object graph, and you do not
want the page subscribing to a dozen properties.

**The MVVM shape.** The view model exposes one `int` that it increments whenever
anything about the view moved. The page watches that single property name and
re-applies everything.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    /// <summary>The shared zoom level and the two pan positions; the page lays the images out from it.</summary>
    public ComparisonView View => _comparison.View;

    /// <summary>
    /// Bumped whenever the zoom, a pan position, or a page changes, so the page can re-apply
    /// the view to its image controls (one property to watch instead of many).
    /// </summary>
    public int ViewVersion
    {
        get;
        private set => SetProperty(ref field, value);
    }
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs
                viewModel.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.ViewVersion)) { ApplyViews(); }
                };
```

**Where to look.**
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Every call site that changes the model goes through one method, which bumps the
  counter, re-notifies the derived labels and refreshes the commands.
- `nameof(MainViewModel.ViewVersion)` keeps the page's filter refactor-safe.
- A counter, not a `bool` or an event: any increment is a change, and it survives
  being read late.

### Do blocking work in a service behind Task Run

**When you want this.** Startup or a command has to read a directory, parse a
file, or decode an image, and the window must stay responsive with a visible
loading state.

**The MVVM shape.** A registered service exposes only `Task`-returning methods and
does the blocking work inside `Task.Run`. The view model awaits them, owns the
loading flag and the visibility that follows it, and disposes whatever it opened.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/Services/AssetCatalogService.cs
public class AssetCatalogService
{
    public Task<AssetFolderCatalog> LoadCatalogAsync(string folderPath) =>
        Task.Run(() => AssetFolderCatalog.LoadFrom(folderPath));

    public Task<BundleArchive> OpenArchiveAsync(AssetBundle bundle) =>
        Task.Run(() => new BundleArchive(bundle.ZipPath));

    public Task<byte[]> ReadEntryBytesAsync(AssetBundle bundle, string entryPath) =>
        Task.Run(() =>
        {
            using var archive = new BundleArchive(bundle.ZipPath);
            return archive.ReadEntryBytes(entryPath);
        });
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
private async Task ReloadCatalogAsync()
{
    IsCatalogLoading = true;
    CloseViewer();
    DisposeArchive();
    _selectedBundle = null;
    BundleCells.Clear();
    Cells = new AssetCellCollection([]);
    ResultCountText = string.Empty;

    _catalog = await _catalogService.LoadCatalogAsync(_assetsFolder);
    // ... build the sidebar cards ...
    IsCatalogLoading = false;

    //Restore the bundle the user browsed last time, or start with the first one
    if (BundleCells.Count > 0)
    {
        var lastBundleFile = SettingsService.Get<string>(LastBundleKey);
        var restored = BundleCells.FirstOrDefault(c =>
            c.Bundle.FileName.Equals(lastBundleFile ?? string.Empty, StringComparison.OrdinalIgnoreCase));
        await SelectBundleAsync(restored ?? BundleCells[0]);
    }
}
```

**Variant: await the network, then build off the UI thread.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
private async Task SelectAsync(SampleAssetKind kind)
{
    if (IsBusy) { return; }

    _selectedKind = kind;
    RaiseSelectionChanged();
    IsBusy = true;

    try
    {
        var progress = new Progress<string>(message => StatusText = message);
        var asset = await _assets.EnsureSampleAsync(kind, progress, CancellationToken.None);

        //Decode/build off the UI thread; the painters upload to GL lazily during Paint.
        var painter = await Task.Run(() => BuildPainter(kind, asset));
        _currentPainter = painter;
        StatusText = $"{Label(kind)}: {asset.Name}    ·    {Hint(kind)}";
    }
    catch (Exception ex)
    {
        StatusText = $"Could not load the {kind.ToString().ToLowerInvariant()} sample: {ex.Message}";
    }
    finally
    {
        IsBusy = false;
        InvalidateCanvas?.Invoke();
    }
}
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/Services/AssetCatalogService.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- One-off reads open and dispose their own handle rather than sharing a long-lived
  one, so a background fetch cannot outlive the selection that started it.
- Swap a long-lived handle under a helper that nulls the field before disposing,
  so a read racing the swap sees null rather than a disposed object.
- Guard re-entry with the busy flag at the top of the method, and always clear the
  flag - and invalidate whatever needs repainting - in the `finally`.
- Keep GPU work off the worker thread. The renderers here take a lock, stash the
  new data as pending, and upload it on the next render, on the render thread.
- Dispose the previous result only after the new one is built and assigned, so a
  failed build leaves the previous view intact.

### Load an asset off the UI thread and resolve its side files from the same container

**When you want this.** Opening a document or model means a parse that must not
block the window, and the file references sibling files that live in the same
archive rather than on disk.

**The MVVM shape.** The parse runs in `Task.Run` behind a loader interface; the
awaited result is assigned and published with a change notification, which the
bound control picks up. External references are resolved by a closure over the
open container, so the loader stays ignorant of where the bytes come from.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
//Parse the GLB off the UI thread; the GPU upload happens lazily at first paint.
//Kenney GLBs reference their colormap texture beside themselves rather than embedding
//it, so external references resolve back into the bundle archive.
var archive = _archive;
var animated = await Task.Run(() =>
{
    using var stream = new MemoryStream(bytes, writable: false);
    return new GltfModelLoader().LoadAnimated(stream,
        name => archive?.ReadDependencyBytes(variant.EntryPath, name));
});
var loaded = animated.Model;

_animatedModel = animated;
_currentModel = loaded;
NotifyPropertyChanged(nameof(CurrentModel));
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Models/GltfModelLoader.cs
private static ModelRoot ReadRoot(Stream stream, Func<string, byte[]?>? resolveDependency)
{
    try
    {
        if (resolveDependency == null)
        {
            return ModelRoot.ReadGLB(stream);
        }

        var context = ReadContext.Create(assetName =>
        {
            var bytes = resolveDependency(Uri.UnescapeDataString(assetName));
            return bytes == null
                ? throw new FileNotFoundException($"The model references '{assetName}', which was not found.")
                : new ArraySegment<byte>(bytes);
        });
        return context.ReadBinarySchema2(stream);
    }
    catch (Exception ex) when (ex is not InvalidDataException)
    {
        throw new InvalidDataException("The stream does not contain a loadable glTF binary (.glb) model.", ex);
    }
}
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Models/GltfModelLoader.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Models/IModelLoader.cs`

**Sharp edges.**
- A "self-contained" binary file may still reference a sibling. Passing a resolver
  that reads back into the same container is what makes such files load; passing
  `null` refuses all external references, which is the safe default for untrusted
  input.
- Referenced names arrive URI-escaped; unescape before looking them up.
- Capture the container field into a local before the `Task.Run`, so a selection
  change during the parse cannot null it out mid-flight.
- The loader interface exists so the loading technology can be swapped or mocked
  without touching the renderer, which takes the loaded model type and never a
  format-specific one.

### Pre warm a rendering backend off the UI thread

**When you want this.** You are about to hand a new GPU backend to a paint
callback, and a supported platform can still have a missing or broken driver. You
want a status message, not an exception inside the paint handler.

**The MVVM shape.** The view model creates the backend, renders one throwaway tiny
frame on a worker thread, and only then swaps painters. A failure is caught,
written to bound status text, and the selection reverted.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
IsBusy = true;
try
{
    var engine = _engineSelector.Create(kind, GetXamlRoot);
    if (kind is RenderEngineKind.Vulkan or RenderEngineKind.Metal)
    {
        //Fail fast off the UI thread (a supported platform can still lack a working
        //driver) so a failure never surfaces inside the Skia paint callback. Safe for the
        //own-stack engines (Vulkan, Metal): they have no thread-affinity, unlike the
        //OpenGL engine's native GL context, which must be created on the render thread at
        //first paint.
        await Task.Run(() => engine.RenderFrame(1, 1, (0f, 0f, 0f, 1f)));
    }

    var oldPainter = _modelPainter;
    _modelPainter = new ModelScenePainter(engine);
    _currentEngineKind = kind;
    if (ReferenceEquals(_currentPainter, oldPainter))
    {
        _currentPainter = null;
    }
    oldPainter?.Dispose();
}
catch (Exception ex)
{
    StatusText = $"Could not switch to {engineName} rendering: {ex.Message}";
    RevertEngineSelection();
    return;
}
finally
{
    IsBusy = false;
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The pre-warm is only safe for backends with no thread affinity. The OpenGL
  engine is deliberately excluded, because its native context must be created on
  the render thread at first paint.
- The current painter is cleared before the old one is disposed, so the page's
  paint handler cannot call into a disposed painter between the two statements.
- After a successful switch the current asset is re-displayed from the local
  cache, so switching backends never touches the network.

### Coalesce repaints and drop backlogged pointer frames

**When you want this.** Each repaint is expensive, and a fast mouse can queue more
pointer events than you can draw.

**The MVVM shape.** Two independent mechanisms. Paint coalescing keeps at most one
pending invalidate. Backlog detection compares the pointer event's own timestamp
against a stopwatch and, when the input stream has fallen behind, advances the
painter's drag anchor without rendering, so the camera stays in sync with the
cursor while frames are skipped.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
//A pointer frame that is running more than this far behind real time is a backlog
//frame: keep the cursor anchor in sync but skip rendering it, catching up to the latest.
private const double StaleFrameMicroseconds = 1_000_000; // 1 second

//Coalescing: never queue more than one paint. While one is pending, pointer moves only
//update the camera; the next paint draws the latest state.
private bool _renderPending;

private void RequestRender()
{
    if (_renderPending) { return; }
    _renderPending = true;
    DisplayCanvas?.Invalidate();
}

private bool IsBacklogFrame(ulong timestamp)
{
    if (!_gestureClock.IsRunning) { return false; }
    var inputElapsed = timestamp - _gestureStartTimestamp;
    var lag = _gestureClock.Elapsed.TotalMicroseconds - inputElapsed;
    return lag > StaleFrameMicroseconds;
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IScenePainter.cs
/// <summary>
/// Advances the drag anchor to the given position without moving the camera, used to
/// discard a stale (backlogged) pointer frame while staying in sync with the cursor.
/// </summary>
void PointerSkip(double x, double y);
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IScenePainter.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/ModelScenePainter.cs`

**Sharp edges.**
- The pending flag is cleared at the top of the paint handler, so a request made
  during paint still queues the next frame.
- `PointerSkip` exists precisely so that dropping a frame does not make the scene
  jump: it moves the anchor without applying the delta to the camera.
- On pointer release the page requests one more render at full, non-drag
  resolution, which is what makes a two-tier resolution scheme work.

### Run a sensor pipeline on a worker thread with latest frame wins

**When you want this.** A sensor produces frames faster than your processing can
consume them and you must never block the producer.

**The MVVM shape.** The whole thing lives in a library class with a
`SubmitFrame()` method and an event; the view model owns the instance, subscribes,
and does nothing else. The class documents that its event is raised on the worker
thread, so consumers know they must marshal.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs
public void SubmitFrame(byte[] bgraPixels, int width, int height)
{
    if (!_running || bgraPixels == null || width < 1 || height < 1) { return; }

    int needed = width * height * 4;
    if (bgraPixels.Length < needed) { return; }

    lock (_pendingLock)
    {
        if (_pendingFrame == null || _pendingFrame.Length != needed)
        {
            _pendingFrame = new byte[needed];
        }
        Array.Copy(bgraPixels, _pendingFrame, needed);
        _pendingWidth = width;
        _pendingHeight = height;
        _hasPending = true;
    }
    _frameSignal.Set();
}

private void WorkerLoop()
{
    PalmDetector detector = null;
    HandLandmarker landmarker = null;
    try
    {
        detector = new PalmDetector(LoadEmbeddedModel(DetectorResourceName));
        landmarker = new HandLandmarker(LoadEmbeddedModel(LandmarkerResourceName));

        while (_running)
        {
            _frameSignal.WaitOne();
            if (!_running) { break; }

            int width;
            int height;
            lock (_pendingLock)
            {
                if (!_hasPending) { continue; }

                //Swap the pending buffer out under the lock; copy-free hand-off
                (_workingFrame, _pendingFrame) = (_pendingFrame, _workingFrame);
                width = _pendingWidth;
                height = _pendingHeight;
                _hasPending = false;
            }
            // ... process _workingFrame and raise TrackingUpdated ...
        }
    }
    finally
    {
        detector?.Dispose();
        landmarker?.Dispose();
        // ... dispose the cached Mats ...
    }
}
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs`

**Also shown by.**
`PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs` (the same worker
shape, extended with multi-hand tracking across frames)

**Sharp edges.**
- Submitting faster than the worker can process silently replaces the pending
  frame. That is the point: stale frames are dropped and the producer never waits.
- `SubmitFrame` copies before returning so the caller may reuse its buffer
  immediately; the worker then swaps the two buffers under the lock, so steady
  state costs one copy per processed frame and no allocations.
- Expensive resources are created inside the worker, so constructing the tracker
  is cheap and the loading cost lands on the background thread.
- The thread is named and marked background; `Stop()` clears the flag, signals the
  wait handle, and joins, which makes disposal genuinely synchronous.
- `Start()` and `Stop()` are idempotent, and there is a test for that.

### Survive a native runtime tearing down while a frame is in flight

**When you want this.** A worker thread calls into a native library that may be
unloaded at process exit, and you do not want that to become a fatal unhandled
exception.

**The MVVM shape.** Two `catch` clauses on the per-frame work: an exception filter
that recognizes shutdown and exits the loop quietly, and a general one that drops
a single bad frame and keeps going.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs
try
{
    HandTrackingResult result = ProcessFrame(detector, landmarker, _workingFrame, width, height);
    TrackingUpdated?.Invoke(this, new HandTrackingEventArgs(result));
}
catch (Exception ex) when (!_running)
{
    //Shutting down: a frame was in flight when the tracker - or the native
    //  OpenCV runtime at process exit - began tearing down (e.g. "terminated
    //  TLS container"). The app is going away; exit the loop quietly rather
    //  than surfacing this as a fatal unhandled exception on the worker thread.
    Debug.WriteLine($"HandTracker worker stopping during shutdown: {ex.Message}");
    break;
}
catch (Exception ex)
{
    //A single frame failed to process - drop it and keep tracking rather than
    //  taking down the whole application over one bad frame.
    Debug.WriteLine($"HandTracker skipped a frame: {ex.Message}");
}
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs`

**Sharp edges.**
- The `when (!_running)` filter is what separates "we are shutting down" from "a
  frame was bad". Without it the shutdown race is indistinguishable from a real
  failure.
- The running flag is `volatile` precisely so the filter sees it the moment
  `Stop()` clears it.
- The `finally` block disposes the native handles on the worker thread that
  created them.

### Publish a small immutable result type from a background pipeline

**When you want this.** A worker raises events at frame rate and you want no risk
of a consumer mutating shared state.

**The MVVM shape.** An immutable result class with an `internal` constructor, a
cached "nothing found" singleton, and an `EventArgs` wrapper. The view model reads
the result once into a local and closes over it.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/HandTrackingResult.cs
internal static HandTrackingResult NoHand { get; } =
    new HandTrackingResult(false, false, 0f, 0f, 0f, 0f);

/// <summary>Indicates whether a hand was found in the frame.</summary>
public bool HandDetected { get; }

/// <summary>
/// Indicates whether the hand is showing the open-palm ("spatula") gesture - the
/// gesture that paints.
/// </summary>
public bool IsOpenPalm { get; }

/// <summary>
/// The palm center's horizontal position, normalized 0..1 across the UNMIRRORED camera
/// frame (smoothed across recent frames).
/// </summary>
public float PalmCenterX { get; }
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Vision/HandTrackingResult.cs`
`WebcamPainter/src/libs/WebcamPainter.Webcam/CapturedPhoto.cs`

**Sharp edges.**
- The event fires on "nothing found" frames too, and the documentation says why:
  subscribers need it to end an in-progress gesture.
- `internal` constructors mean only the library can create results; consumers can
  only read them.
- The XML documentation carries the coordinate contract - unmirrored, normalized,
  smoothed - which is where a consumer learns it must mirror.

### Capture a still and start a second pipeline from a command

**When you want this.** One command has to grab data, build a heavier model off
the UI thread, subscribe to it, and flip the whole UI into another mode.

**The MVVM shape.** An async command that captures, offloads construction with
`Task.Run`, wires the new object's events (marshalling the ones that touch bound
state), stores it, lazily creates the long-lived worker, and flips the mode flag
last.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
IsBusy = true;
try
{
    var photo = _captureService.CapturePhoto();

    //The preview the user was watching is mirrored, so mirror the still to match
    var session = await Task.Run(() =>
        PaintingSession.Create(photo.PixelsBgra32, photo.Width, photo.Height, mirrorHorizontally: true));

    session.Session.RedrawRequested += (_, _) => InvalidateMainCanvas?.Invoke();
    session.Session.DrawingChanged += (_, _) =>
        InvokeOnMainThread(() => HasDrawing = _paintSession?.HasStrokes ?? false);

    _paintSession = session;
    HasDrawing = false;
    ActiveColorText = $"Painting with: {session.ActiveColorName}";

    if (_tracker == null)
    {
        _tracker = new HandTracker();
        _tracker.TrackingUpdated += OnTrackingUpdated;
    }
    _tracker.Start();

    IsCaptureMode = false;
    NotifyPropertyChanged(nameof(PaintSession));
    InvalidateMainCanvas?.Invoke();
    StatusText = "Show the camera your open palm to spread paint on the photo - " +
                 "close your hand (or hide it) to stop painting.";
}
catch (Exception e)
{
    StatusText = $"Photo failed: {e.Message}";
}
finally
{
    IsBusy = false;
}
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- A plain expression-bodied property over a field needs an explicit
  `NotifyPropertyChanged` when the field is replaced; only `SetProperty`
  properties notify themselves.
- The worker is created once and reused across mode changes; only `Start()` and
  `Stop()` cycle, and its event is subscribed exactly once.
- Events that fire off the UI thread marshal in their handler; a handler that only
  calls a bridge delegate can rely on the delegate to marshal itself.
- The mode flag flips only after everything is in place, so a frame arriving
  mid-setup does not find a half-built mode.

### Run an effect on worker threads with a live preview

**When you want this.** An expensive transform must render off the UI thread, show
partial results as it goes, stay cancellable, and end up in the undo history.

**The MVVM shape.** A manager owns the preview surface and the render handle; the
renderer is a static that splits the region across threads. The UI thread only
polls for finished tiles through a timer service, and the configuration dialog is
awaited concurrently with the render.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/LivePreviewManager.cs
const uint UPDATE_MILLISECONDS = 100;

AsyncEffectRenderer.Settings settings = new (
    threadCount: system.RenderThreads,
    renderBounds: RenderBounds,
    effectIsTileable: effect.IsTileable);
// ...
renderHandle = AsyncEffectRenderer.Start (
    settings,
    effect,
    layer.Surface,
    LivePreviewSurface);

using IDisposable _ = timer.Start (
    UPDATE_MILLISECONDS,
    () => {
        if (!renderAlive) return false;
        PollForUpdate (renderHandle);
        return true; // Keep ticking as long as the effect is active.
    }
);

bool userConfirmed = !effect.IsConfigurable || await effect.LaunchConfiguration ();

chrome.MainWindowBusy = true;

if (!userConfirmed) {
    renderHandle.Cancel ();
    await renderHandle.Task;
    return;
}

dialog.Show ();

var result = await renderHandle.Task;

// Final poll after the renderer finishes to ensure the last-rendered tiles are displayed.
PollForUpdate (renderHandle);
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Effects/BaseEffect.cs
/// <summary>
/// Specifies whether Render() can be called separately (and possibly in parallel) for different sub-regions of the image.
/// If false, Render () will be called once with the entire region the effect is applied to.
/// This is required for effects which cannot be applied independently to each pixel, e.g. if the effect accumulates information from previously processed pixels.
/// </summary>
public abstract bool IsTileable { get; }
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/LivePreviewManager.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/AsyncEffectRenderer.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Effects/BaseEffect.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/CanvasRenderer.cs`

**Sharp edges.**
- The tileable flag is the correctness gate: an effect that accumulates state
  across pixels must declare itself untileable or parallel tiles produce wrong
  output.
- One final poll after the render task completes, or the last tiles never reach
  the screen.
- The renderer's own comment says its methods are to be called from a single
  thread, the UI thread, only.
- Thread count comes from a system service, which the tests replace with a mock.
- The canvas renderer substitutes the preview surface for the active layer while
  the preview is enabled, so no extra compositing path is needed.

### Drive an undo history from a list and travel to a clicked point

**When you want this.** Undo and redo, a visible history, and the ability to jump
several steps at once.

**The MVVM shape.** The document owns a history of items with a pointer; the view
binds a list to the items, dims the ones past the pointer, and travels one step at
a time so each item's own undo or redo runs. Command enablement follows the
history's own `CanUndo` and `CanRedo`.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (updatingHistorySelection || !PintaCore.Workspace.HasOpenDocuments) { return; }

    DocumentHistory history = PintaCore.Workspace.ActiveWorkspace.History;
    int target = HistoryList.SelectedIndex;

    if (target < 0 || target == history.Pointer) { return; }

    //Travel to the clicked point, one step at a time so every history item's
    //own Undo/Redo runs.
    while (history.Pointer > target && history.CanUndo) { history.Undo(); }
    while (history.Pointer < target && history.CanRedo) { history.Redo(); }
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Pads/HistoryRowFactory.cs
StackPanel row = new () {
    Orientation = Orientation.Horizontal,
    Spacing = 6,
    // Dimming is what tells a user the entry is "ahead" of where the
    // document currently is.
    Opacity = undone ? 0.45 : 1.0,
};
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Tools/Tools/PencilTool.cs
protected override void OnMouseUp (Document document, ToolMouseEventArgs e)
{
	if (undo_surface != null && surface_modified)
		document.History.PushNewItem (new SimpleHistoryItem (Icon, Name, undo_surface, document.Layers.CurrentUserLayerIndex));

	surface_modified = false;
	undo_surface = null;
	mouse_button = MouseButton.None;
}
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/DocumentHistory.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Pads/HistoryRowFactory.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Tools/Tools/PencilTool.cs`

**Sharp edges.**
- A guard flag around programmatic selection changes is mandatory, or the refresh
  that follows an undo triggers another travel.
- Travel one step at a time; moving the pointer directly would skip each item's
  own undo work.
- The undo snapshot is taken on the gesture's start and pushed only if the surface
  was actually modified.

### Bind a tab per open document and keep both directions in sync

**When you want this.** A tabbed multi-document interface where the model, not the
tab control, owns which document is active.

**The MVVM shape.** A dictionary maps documents to tab items; model events add and
remove tabs, and the tab's own selection change pushes the choice back into the
model. Comparison before pushing stops the echo.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
private void AddDocumentTab(Document document)
{
    PintaCanvasView view = new() { Document = document };
    TabViewItem tab = new()
    {
        Header = document.DisplayName,
        Content = view,
    };
    documentTabs[document] = tab;
    DocumentTabs.TabItems.Add(tab);
    DocumentTabs.SelectedItem = tab;

    document.Renamed += (_, _) => { tab.Header = document.DisplayName; RebuildWindowMenu(); };
    document.IsDirtyChanged += (_, _) =>
    {
        tab.Header = document.IsDirty ? $"{document.DisplayName}*" : document.DisplayName;
        RebuildWindowMenu();
    };

    //History changes drive Undo/Redo enablement and the history pad.
    document.History.HistoryItemAdded += (_, _) => OnDocumentStateChanged();
    document.History.ActionUndone += (_, _) => OnDocumentStateChanged();
    document.History.ActionRedone += (_, _) => OnDocumentStateChanged();
    // ...
}
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml`

**Sharp edges.**
- The index check before pushing the active document is what stops the model event
  and the tab event from ping-ponging.
- A tab close request must run the save prompt rather than closing the tab
  directly; the tab close button is the most likely way to lose a document.
- Subscriptions that are re-established on every activation are removed before
  being added, so switching tabs repeatedly does not stack handlers.

### Show selection state in button captions from computed properties

**When you want this.** The UI must show which of several modes is active, without
a converter or code-behind.

**The MVVM shape.** One private-set property holds the active name; computed
properties derive the button captions from it; the setter raises change
notifications for all of them. The XAML binds `Content` to the computed
properties.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
public string ActiveLayerName
{
    get;
    private set
    {
        SetProperty(ref field, value);
        NotifyPropertyChanged(nameof(PainButtonText));
        NotifyPropertyChanged(nameof(NumbnessButtonText));
        NotifyPropertyChanged(nameof(TinglingButtonText));
    }
} = PainLayerName;

public string PainButtonText => ActiveLayerName == PainLayerName ? "✓ Pain" : "Pain";
public string NumbnessButtonText => ActiveLayerName == NumbnessLayerName ? "✓ Numbness" : "Numbness";
public string TinglingButtonText => ActiveLayerName == TinglingLayerName ? "✓ Tingling" : "Tingling";
```

```xml
<!-- From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml -->
<StackPanel Grid.Row="1" Orientation="Horizontal" Spacing="8" Margin="0,0,0,8">
    <Button Content="{d:Binding PainButtonText}" Command="{d:Binding SelectPainCommand}" MinWidth="110"
            Background="#66FF1EE6" />
    <Button Content="{d:Binding NumbnessButtonText}" Command="{d:Binding SelectNumbnessCommand}" MinWidth="110"
            Background="#661E80CC" />
    <Button Content="{d:Binding TinglingButtonText}" Command="{d:Binding SelectTinglingCommand}" MinWidth="110"
            Background="#66CCAA0A" />
</StackPanel>
```

**Where to look.**
`PainDiagram/Shared/ViewModels/MainViewModel.cs`
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml`

**Sharp edges.**
- The property initializer sets the initial caption without running the setter
  body, so the computed captions are correct before the first notification.
- Commands with no meaningful `CanExecute` - here the three selection commands -
  are constructed from the handler alone, and a synchronous handler in an
  async-shaped signature ends with `return Task.CompletedTask;`.

## Bridging platform services into the view model

### Give the view model a XamlRoot so its dialogs can show

**When you want this.** Your view model calls `ConfirmDialog`, `ShowInfo`,
`ShowError` or `CreateDialog`, and those need a `XamlRoot` that only the page has.

**The MVVM shape.** `SimpleViewModel` implements `IXamlRootGetter`. The page's one
job is to hand it a getter - not the value, a getter - as soon as the
`DataContext` is set. This is the smallest bridge in the family and every
application that shows a dialog needs it.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml.cs
using CodeBrix.Platform.Simple;
using Microsoft.UI.Xaml.Controls;

namespace MediaPlayerDemo.Views;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        DataContextChanged += (_, _) =>
        {
            //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
            (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);
        };

        this.InitializeComponent(); //Leave this line last
    }
}
```

The same getter also serves platform services that need a root of their own:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
if (OffscreenGLContext.TryCreate(GetXamlRoot(), out var glContext))
{
    // ... render the product shots ...
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
_modelPainter = new ModelScenePainter(_engineSelector.Create(RenderEngineKind.OpenGL, GetXamlRoot));
```

A native head satisfies the same interface with whatever its own dialog API
anchors to:

```csharp
// From CodeBrix.Samples/JustBetweenUs/Mobile/Views/MainPage.xaml.cs
(BindingContext as IXamlRootGetter)?.SetXamlRootGetter(() => this);
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`
`JustBetweenUs/Mobile/Views/MainPage.xaml.cs`

**Also shown by.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs`,
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`,
`NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml.cs`,
`PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs` and
`PainDiagram/PainDiagram.WinUI/Views/MainPage.xaml.cs`,
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs`,
`PdfSideBySide/src/PdfSideBySide.UI/Views/MainPage.xaml.cs`,
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`,
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs`,
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- A lambda, not the value. The page's `XamlRoot` is null until the page is in the
  visual tree, so the view model has to re-read it at the moment it needs it.
- The wiring goes in `DataContextChanged`, subscribed before
  `InitializeComponent()`, because the XAML is what sets the `DataContext`. Most
  of these files carry the comment "Leave this line last" on
  `InitializeComponent()` for exactly that reason.
- The `as` cast plus `?.` is the graceful-degradation path: a page whose data
  context is something else, or a design-time data context, simply does nothing.
- A native WPF head skips this entirely - WPF has no `XamlRoot` - and its dialogs
  still work, so shared view-model code must not assume the getter was supplied.
- Wire it even in an application that has no dialogs yet. It costs one line, and
  CodeBrixVideoTool and MediaPlayerDemo both do it before they need it.

### Save a file through a native dialog from the view model

**When you want this.** A command needs a destination path from a "save as"
dialog, and the application must still work on a head that has none.

**The MVVM shape.** The view model declares a small interface holding one delegate
the page fills in, and implements it itself. The command supplies a suggested file
name, treats a null or blank result as a cancel, and handles two separate "no
dialog" signals: a null delegate (the head never wired one) and a
`NotSupportedException` (the head wired one but the platform refuses). The page
implements the picker in a few lines inside its `DataContextChanged` handler.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Lets the hosting page give the view model a native "Save PDF as…" file dialog. Each head
/// wires this up with the file dialog appropriate to its UI stack (the CodeBrix.Platform
/// <c>FileSavePicker</c> on the Skia heads).
/// </summary>
public interface IFileSaveBridge
{
    /// <summary>
    /// Shows a "save PDF" dialog seeded with suggestedFileName and returns the
    /// full path the user chose, or <c>null</c> if they cancelled. The head leaves this null when
    /// it has no file dialog, in which case the user types the path directly into the box.
    /// Signature: <c>Func&lt;suggestedFileName, Task&lt;chosenPathOrNull&gt;&gt;</c>.
    /// </summary>
    Func<string, Task<string>> PickSavePdfPathAsync { get; set; }
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
private async Task DoSelectOutputFile()
{
    if (!CanSelectOutputFile()) { return; }

    if (PickSavePdfPathAsync == null)
    {
        //No native file dialog on this head — the user types the destination
        //  path directly into the box instead.
        await ShowInfo(
            "This head has no file dialog. Type the full path (including the .pdf file name) " +
            "for the PDF into the “Save PDF to” box.");
        return;
    }

    try
    {
        var chosenPath = await PickSavePdfPathAsync(GetSuggestedFileName());
        if (!string.IsNullOrWhiteSpace(chosenPath))
        {
            OutputFilePath = chosenPath.Trim();
            StatusText = $"Will save to: {OutputFilePath}";
        }
    }
    catch (NotSupportedException)
    {
        //Some heads register no picker — there is no window to host a dialog
        await ShowInfo(
            "File dialogs are not supported on this head. Type the full path (including the " +
            ".pdf file name) for the PDF into the “Save PDF to” box.");
    }
    catch (Exception e)
    {
        await ShowError($"Could not open the file dialog: {e.Message}");
    }
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml.cs
public MainPage()
{
    //Doing this before InitializeComponent() - in case InitializeComponent()
    //  is the thing that sets the data context.
    DataContextChanged += (_, _) =>
    {
        //Give the view model's SimpleDialog helpers a XamlRoot to attach dialogs to
        (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

        //Give the view model a native "Save PDF as…" file dialog (CodeBrix.Platform's
        //  FileSavePicker). Heads with no windowing system throw NotSupportedException
        //  from the picker; the view model handles that.
        if (DataContext is IFileSaveBridge fileSave)
        {
            fileSave.PickSavePdfPathAsync = PickSavePdfPathAsync;
        }
    };

    this.InitializeComponent(); //Leave this line last
}

private static async Task<string> PickSavePdfPathAsync(string suggestedFileName)
{
    var picker = new FileSavePicker
    {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        SuggestedFileName = suggestedFileName,
        DefaultFileExtension = ".pdf"
    };
    picker.FileTypeChoices.Add("PDF document", new List<string> { ".pdf" });

    var file = await picker.PickSaveFileAsync();
    if (file == null) { return null; }

    //Some heads percent-encode the path they return, which would save "My Book.pdf" as
    //  "My%20Book.pdf"; decode it before anything touches the disk.
    var path = FileDialogHelper.ToFileSystemPath(file.Path);

    FileDialogHelper.RemoveEmptyPlaceholder(path);
    return path;
}
```

The view model computes the suggested name from its own state and sanitizes it:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs
/// <summary>A sensible default PDF file name: the first checked page's title.</summary>
private string GetSuggestedFileName()
{
    var name = Flatten().FirstOrDefault(n => !n.IsPlaceholder && n.IsChecked)?.Title;
    if (string.IsNullOrWhiteSpace(name)) { name = "NotionBook"; }

    var invalid = Path.GetInvalidFileNameChars();
    var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
    return (cleaned.Length == 0 ? "NotionBook" : cleaned) + ".pdf";
}
```

**Variant: write somewhere sensible when there is no dialog at all.** Where an
application would rather write a file than refuse, the null-delegate branch picks
a path itself:

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
string outputPath;

if (PickSaveJpegPathAsync == null)
{
    //No native file dialog on this head (e.g. the Linux framebuffer head) -
    //  save to a default location instead
    outputPath = GetDefaultSavePath();
}
else
{
    outputPath = await PickSaveJpegPathAsync(GetSuggestedFileName());
    if (String.IsNullOrWhiteSpace(outputPath))
    {
        return; //the user cancelled the dialog
    }
    outputPath = outputPath.Trim();

    //Confirm before clobbering an existing file (the head's own overwrite
    //  prompt is suppressed so this is the single confirmation)
    if (File.Exists(outputPath))
    {
        var replace = await ConfirmDialog(
            $"A file already exists at:\n{outputPath}\n\nDo you want to replace it?",
            "Replace existing file?");
        if (!replace)
        {
            StatusText = "Save cancelled - the existing file was kept.";
            return;
        }
    }
}

IsBusy = true;

var jpeg = _paintSession.ExportJpeg();
await File.WriteAllBytesAsync(outputPath, jpeg);
```

**Variant: a bridge that also carries the extension, and writes beside the
source.** CodeBrixVideoTool's bridge takes both a suggested name and an
extension, and when it is absent the view model writes next to the input file:

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/IOutputPathBridge.cs
public interface IOutputPathBridge
{
    /// <summary>
    /// Shows a "save as" dialog seeded with a suggested file name and returns the full path the
    /// person chose, or null if they cancelled. The head leaves this null when it has no file
    /// dialog, in which case the result is written beside the source instead.
    /// </summary>
    Func<string, string, Task<string>> PickOutputPathAsync { get; set; }
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/ViewModels/ConversionViewModel.cs
var destination = SelectedDestination.Kind;
var suggested = ConversionPlanner.SuggestOutputFileName(Source, destination);
var extension = MediaFormats.Extension(destination);

string outputPath;
if (PickOutputPathAsync is null)
{
    outputPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Source.Path)) ?? ".", suggested);
}
else
{
    outputPath = await PickOutputPathAsync(suggested, extension);
    if (string.IsNullOrWhiteSpace(outputPath))
    {
        StatusText = "Cancelled - no destination was chosen.";
        return;
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/ViewModels/MainViewModel.cs`
and `NotionDocumentCreator/src/NotionDocumentCreator.UI/Views/MainPage.xaml.cs`
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/IOutputPathBridge.cs`

**Also shown by.**
`PainDiagram/Shared/ViewModels/MainViewModel.cs` and the three page code-behinds
that satisfy it (Skia, WinUI 3, WPF),
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs` and its four heads,
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs` (the
picker call kept in the view model behind a private static method, with
`NotSupportedException` caught specifically)

**Sharp edges.**
- The page code-behind needs `using System;` for the awaiter extension that makes
  the picker awaitable. Several of these files carry a comment saying so, because
  the using looks unused and is easy to remove by mistake.
- Sanitize the suggested file name against `Path.GetInvalidFileNameChars()` before
  handing it to the picker.
- Set the busy flag only after the dialog closes, so the busy state does not
  disable the UI while a modal picker is open.
- Null the delegate in `Dispose()`, or the page stays alive through the view
  model.
- A delegate bridge is also trivially substitutable in a scripted run: the video
  tool's smoke path assigns `(_, _) => Task.FromResult(outputPath)`.
- Where two formats share an extension, put the difference in the suggested name
  so the two are distinguishable on disk.

### Pick a file to open through a native dialog from the view model

**When you want this.** A command has to ask a person which file to work with, and
only a head knows how to show a dialog.

**The MVVM shape.** The same bridge shape as saving: a one-member interface whose
member is a delegate the page fills in. The command checks for null first and says
so in the status line when a head cannot supply one, so an application that runs
where there is no windowing system still starts and still explains itself.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Services/IMediaFileBridge.cs
/// <summary>
/// The one thing the main view model cannot do for itself: ask a person which file to open. Only a
/// head knows how to show a file dialog, so the page fills this in.
/// </summary>
public interface IMediaFileBridge
{
    Func<Task<string>> PickMediaFileAsync { get; set; }
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs
public SimpleCommand OpenCommand => field ??= new SimpleCommand(
    () => !IsBusy, (Func<object, Task>)(_ => DoOpenAsync()));

private async Task DoOpenAsync()
{
    if (PickMediaFileAsync is null)
    {
        StatusText = "This head has no file dialog, so a file cannot be chosen by hand.";
        return;
    }

    var path = await PickMediaFileAsync();
    if (string.IsNullOrWhiteSpace(path))
    {
        return;
    }

    await AddAsync(path, CancellationToken.None);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
private static async Task<string> PickMediaFileAsync()
{
    try
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.VideosLibrary
        };

        foreach (var extension in MediaFormats.ImportExtensions)
        {
            picker.FileTypeFilter.Add(extension);
        }

        picker.FileTypeFilter.Add(".mkv");
        picker.FileTypeFilter.Add(".webm");
        picker.FileTypeFilter.Add(".cbv");

        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }
    catch (NotSupportedException)
    {
        //A head with no windowing system registers no picker extensions.
        return null;
    }
}
```

**Adapted: put the picker behind an interface rather than calling it inline.**
PdfSideBySide calls the picker directly from its view model; the shape to prefer
keeps the picker configuration verbatim but moves the call behind a bridge, so a
head with no picker is a case the view model handles rather than an exception it
catches:

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
    private static async Task<string> PickPdfPathAsync()
    {
        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };
        picker.FileTypeFilter.Add(".pdf");

        var file = await picker.PickSingleFileAsync();
        return file?.Path;
    }
```

```csharp
// Adapted from CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
// The picker call is moved behind an interface the page implements, so a head that cannot
// show one is a case the view model handles instead of an exception it catches.
public interface IPdfFileBridge
{
    Task<string> PickPdfPathAsync();
}

// In the view model:
private IPdfFileBridge _fileBridge;

public void SetFileBridge(IPdfFileBridge bridge) => _fileBridge = bridge;

private async Task BrowseAsync(DocumentSide side)
{
    if (IsBusy) { return; }
    if (_fileBridge == null)
    {
        await ShowError("This head cannot browse for files; pass the two PDF paths on the command line.");
        return;
    }

    IsBusy = true;
    try
    {
        var path = await _fileBridge.PickPdfPathAsync();
        if (path == null) { return; }
        // ... unchanged from the sample
    }
    finally { IsBusy = false; }
}
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/Services/IMediaFileBridge.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`
`PdfSideBySide/src/PdfSideBySide.LinuxFrameBuffer/Program.cs`

**Sharp edges.**
- Two degradation points, not one: the delegate may be null (no head wired it) and
  the delegate may return null (no dialog, or the person cancelled). Treat them
  differently - the first deserves an explanation, the second is silent.
- A head with no windowing system registers no picker and
  `PickSingleFileAsync()` throws `NotSupportedException`; catch it in the page and
  return null rather than letting it reach the view model.
- `FileTypeFilter` takes extensions with the leading dot, and a filter list is
  only a first pass - candidates should still be validated after they are chosen.
- The pickers live in `Windows.Storage.Pickers`, which the library that carries
  CodeBrix.Platform already provides; no extra package is needed.
- The LinuxFrameBuffer head has to opt into an open picker on its host builder
  (`EnableFileOpenPicker(...)`); see the framebuffer blueprint in the startup
  area.

### Clean up the path a file picker returns

**When you want this.** A picker on one head hands back a percent-encoded
URI-shaped path, or creates an empty placeholder file at the chosen location, and
your application then behaves differently per head.

**The MVVM shape.** Two small static helpers in the shared library, called by
whichever head-side picker code needs them, so every head hands the view model the
same kind of plain file-system path and the same truthful answer to
`File.Exists()`.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/NotionDocumentCreator.Core/Helpers/FileDialogHelper.cs
/// <summary>
/// Turns the path a picker hands back into a real file-system path. The Linux Skia heads
/// build theirs out of the desktop portal's <c>file://</c> URI and leave it
/// percent-encoded, so a name with a space in it arrives as <c>My%20Book.pdf</c> and
/// would be written to disk under that literal name; accented names fare worse still
/// (<c>Ölberg</c> arrives as <c>%C3%96lberg</c>). Nothing is decoded unless the text
/// really does carry escapes, so paths from heads that already return a plain one — the
/// Win32 and WPF save dialogs — pass through untouched.
/// </summary>
public static string ToFileSystemPath(string path)
{
    if (string.IsNullOrWhiteSpace(path)) { return path; }

    //A head that hands back the whole URI rather than just its path.
    if (path.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
        && Uri.TryCreate(path, UriKind.Absolute, out var uri)
        && uri.IsFile)
    {
        return uri.LocalPath;
    }

    return HasPercentEscape(path) ? Uri.UnescapeDataString(path) : path;
}

//True when the text holds at least one "%" followed by two hex digits. A literal percent
//  sign that is not the start of an escape (say "100% done.pdf") leaves the path alone.
private static bool HasPercentEscape(string text)
{
    for (var i = 0; i + 2 < text.Length; i++)
    {
        if (text[i] == '%' && Uri.IsHexDigit(text[i + 1]) && Uri.IsHexDigit(text[i + 2]))
        {
            return true;
        }
    }

    return false;
}
```

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/Helpers/FileDialogHelper.cs
/// <summary>
/// The WinRT <c>FileSavePicker</c> (Skia heads and native WinUI) creates an empty
/// placeholder file at the chosen path for a brand-new name. Remove it - but only when it
/// is genuinely empty - so a chosen path behaves like a pure destination and the app's own
/// "replace existing file?" prompt fires only for a real, non-empty file. A file that has
/// content is never deleted, so no user data is lost before the save-time confirmation.
/// </summary>
public static void RemoveEmptyPlaceholder(string path)
{
    if (string.IsNullOrWhiteSpace(path)) { return; }

    try
    {
        var info = new FileInfo(path);
        if (info.Exists && info.Length == 0)
        {
            info.Delete();
        }
    }
    catch
    {
        //Leave the file in place if it cannot be removed; the save-time overwrite
        //  prompt will simply ask about it.
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/NotionDocumentCreator.Core/Helpers/FileDialogHelper.cs`
`PainDiagram/Shared/Helpers/FileDialogHelper.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Helpers/FileDialogHelper.cs` (the
folder picker needs the same decoding: a folder called "My Models" would otherwise
send every download to a literally named `My%20Models`),
`WikipediaPublisher/Shared/Helpers/FileDialogHelper.cs` (linked into the shared
library and the WinUI head only - the WPF head does not link it, because a WPF
`SaveFileDialog` already returns a plain path),
`WebcamPainter/src/WebcamPainter.Core/Helpers/FileDialogHelper.cs`

**Sharp edges.**
- Decoding unconditionally would corrupt a legitimate name containing a percent
  sign, which is why the helper looks for a real `%XX` escape first.
- The placeholder is deleted only when its length is zero, so a real file is never
  lost before the application's own overwrite confirmation.
- Failure to delete is deliberately swallowed: the worst case is one extra
  confirmation prompt, never lost data.
- Call both helpers in the page, before the path reaches the view model, so the
  view model only ever sees real paths.

### Suppress a native save dialog overwrite prompt so the view model owns confirmation

**When you want this.** The user is asked twice whether to replace a file - once
by the save dialog and once by your application.

**The MVVM shape.** The bridge delegate is the seam. Each head configures its own
dialog to stay silent, and the single point of confirmation is a `SimpleDialog`
call in the view model's command, so the behavior is identical on every head. The
view model is unchanged.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs
var dialog = new Microsoft.Win32.SaveFileDialog
{
    Title = "Save PNG as",
    Filter = "PNG image (*.png)|*.png|All files (*.*)|*.*",
    DefaultExt = ".png",
    AddExtension = true,
    FileName = suggestedFileName,
    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
    OverwritePrompt = false   //The app does its own replace prompt via SimpleDialog
};
```

The WinRT picker cannot be told to stay quiet, so the WinUI 3 head drops to the
Win32 common item dialog through COM interop and clears the option itself:

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.WinUI/Views/MainPage.xaml.cs
if (DataContext is IFileSaveBridge fileSave)
{
    fileSave.PickSavePngPathAsync = (fileName) =>
    {
        //The Win32 dialog (rather than the WinRT FileSavePicker) so the un-suppressible
        //  WinRT overwrite prompt does not double up with the app's own confirmation
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.CurrentWindow);
        var path = Win32SaveFileDialog.PickSavePath(hwnd, fileName, "Save PNG as");
        return Task.FromResult(path);
    };
}
```

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.WinUI/Views/Win32SaveFileDialog.cs
public static string PickSavePath(IntPtr ownerHwnd, string suggestedFileName, string title)
{
    var dialog = (IFileDialog)new FileSaveDialog();
    try
    {
        //Start from the file system's real paths, and don't nag about overwriting.
        dialog.GetOptions(out var options);
        options |= FOS.FORCEFILESYSTEM;
        options &= ~FOS.OVERWRITEPROMPT;
        dialog.SetOptions(options);
        // ... filters, title, suggested file name, default folder

        const int cancelledHr = unchecked((int)0x800704C7); //HRESULT_FROM_WIN32(ERROR_CANCELLED)
        var hr = dialog.Show(ownerHwnd);
        if (hr == cancelledHr) { return null; }
        if (hr < 0) { Marshal.ThrowExceptionForHR(hr); }

        dialog.GetResult(out var item);
        try
        {
            item.GetDisplayName(SIGDN.FILESYSPATH, out var pathPtr);
            try { return Marshal.PtrToStringUni(pathPtr); }
            finally { Marshal.FreeCoTaskMem(pathPtr); }
        }
        finally
        {
            Marshal.ReleaseComObject(item);
        }
    }
    finally
    {
        Marshal.ReleaseComObject(dialog);
    }
}
```

**Where to look.**
`PainDiagram/PainDiagram.WinUI/Views/Win32SaveFileDialog.cs`
`PainDiagram/PainDiagram.WinUI/Views/MainPage.xaml.cs`
`PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs`

**Also shown by.**
`WikipediaPublisher/WikipediaPublisher.Wpf/Views/MainWindow.xaml.cs` and
`WikipediaPublisher/WikipediaPublisher.WinUI/Views/Win32SaveFileDialog.cs`

**Sharp edges.**
- The class documentation records both reasons for dropping to the Win32 dialog:
  the WinRT picker always shows its own replace confirmation with no way to turn
  it off, and it also creates an empty placeholder file. The Win32 dialog does
  neither.
- The dialog needs a window handle, so `App` exposes its main window as a static
  property purely so the page can ask for it.
- COM objects are released in `finally` blocks and the display-name pointer is
  freed explicitly.
- If you keep the WinRT picker, as the Skia heads do, pair it with the
  empty-placeholder cleanup from the previous blueprint instead.

### Let the page invalidate a canvas through a bridge interface

**When you want this.** Background work changes what should be drawn, and the view
model has to trigger a repaint without owning a control reference.

**The MVVM shape.** The view model declares a one-property interface holding an
`Action` (or one per canvas) and implements it. The page assigns a closure over
its own canvas when the `DataContext` arrives, and is responsible for marshalling
to the UI thread. The view model calls `?.Invoke()` from whichever thread it is
on, which is also the graceful-degradation path when no page has wired one.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Lets the hosting page hand the view model the invalidate (repaint) delegates for the two
/// Skia canvases. Frames and tracking results arrive on capture/worker threads; the page's
/// delegates are responsible for marshalling their invalidates onto the UI thread.
/// </summary>
public interface ICanvasBridge
{
    Action InvalidateMainCanvas { get; set; }
    Action InvalidateSelfView { get; set; }
}
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs
DataContextChanged += (_, _) =>
{
    (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

    if (DataContext is IFileSaveBridge fileSave)
    {
        fileSave.PickSaveJpegPathAsync = PickSaveJpegPathAsync;
    }

    if (DataContext is ICanvasBridge canvasBridge)
    {
        //Frames and tracking results arrive on capture/worker threads - marshal
        //  the repaints onto the UI thread
        canvasBridge.InvalidateMainCanvas = () => DispatcherQueue?.TryEnqueue(() => MainCanvas?.Invalidate());
        canvasBridge.InvalidateSelfView = () => DispatcherQueue?.TryEnqueue(() => SelfViewCanvas?.Invalidate());
    }
};

InitializeComponent();
```

Where a library raises its own "I changed, repaint me" event, the view model
subscribes once and forwards, with no timer and no per-frame polling anywhere:

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
public interface ICanvasInvalidator
{
    /// <summary>Invalidates the hosting page's drawing canvas (null before the page wires it up).</summary>
    Action InvalidateCanvas { get; set; }
}

// ... in the constructor:
_session.RedrawRequested += (_, _) => InvalidateCanvas?.Invoke();
_session.DrawingChanged += (_, _) => InvokeOnMainThread(() => HasDrawing = _session.HasStrokes);
```

A native WPF head has to marshal differently, which is exactly why the bridge is a
delegate rather than a method the view model calls:

```csharp
// From CodeBrix.Samples/PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs
private void InvalidateDrawCanvas()
{
    if (DrawCanvas.Dispatcher.CheckAccess())
    {
        DrawCanvas.InvalidateVisual();
    }
    else
    {
        DrawCanvas.Dispatcher.BeginInvoke(DrawCanvas.InvalidateVisual);
    }
}
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs` and
`WebcamPainter/src/WebcamPainter.UI/Views/MainPage.xaml.cs`
`PainDiagram/Shared/ViewModels/MainViewModel.cs` and
`PainDiagram/PainDiagram.Wpf/Views/MainWindow.xaml.cs`

**Also shown by.**
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs` and
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs`,
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
and `PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`
(where the page assigns its own coalescing `RequestRender` method rather than a
raw invalidate)

**Sharp edges.**
- The null-conditional chain inside the delegate
  (`DispatcherQueue?.TryEnqueue(() => Canvas?.Invalidate())`) matters: these
  delegates can fire while the page is being torn down.
- Two kinds of event deserve two treatments. A cheap repaint request can invoke
  the delegate directly and let the delegate marshal; an event that writes a bound
  property goes through `InvokeOnMainThread` in the view model.
- The view model nulls every delegate in `Dispose()`, which is what breaks the
  page-to-view-model reference cycle.
- Call it from the `finally` of a load path too, so a failure still repaints.

### Copy text to the clipboard from a command through a bridge interface

**When you want this.** A capability only the head can provide, needed by a
command, on heads that do not all support it.

**The MVVM shape.** The view model declares a tiny interface with a settable
delegate and implements it. The command checks whether the delegate was supplied:
if it was, it invokes it on the main thread; if it was not, it tells the user the
feature is not available on this platform. Each head's page assigns the delegate
in one place, using its own clipboard API.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
public interface ICopyToClipboard { Action<string> CopyTextToClipboard { get; set; }}

// ...

public class MainViewModel : SimpleViewModel, ICopyToClipboard
{
    // ...
    private async Task DoCopyToClipboard()
    {
        if (CanCopyToClipboard())
        {
            if (CopyTextToClipboard != null)
            {
                InvokeOnMainThread(() => CopyTextToClipboard(ProcessedText));
                if (!_copyMessageShown)
                {
                    _copyMessageShown = true;
                    await ShowInfo("The processed text has been copied to the system clipboard.");
                }
            }
            else
            {
                await ShowError(
                    "This platform implementation does not have the Copy-to-clipboard functionality enabled.");
            }
        }
    }

    public Action<string> CopyTextToClipboard { get; set; }
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml.cs
public MainPage()
{
    //Doing this before InitializeComponent() - in case InitializeComponent()
    //  is the thing that sets the data context.
    DataContextChanged += (sender, args) =>
    {
        (DataContext as IXamlRootGetter)?.SetXamlRootGetter(() => XamlRoot);

        if (DataContext is ICopyToClipboard copy)
        {
            copy.CopyTextToClipboard = (text) =>
            {
                if (!string.IsNullOrEmpty(text))
                {
                    var clipData = new DataPackage();
                    clipData.SetText(text);
                    Clipboard.SetContent(clipData);
                }
            };
        }
    };

    InitializeComponent();
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Wpf/Views/MainWindow.xaml.cs
DataContextChanged += (sender, args) =>
{
    if (DataContext is ICopyToClipboard copy)
    {
        copy.CopyTextToClipboard = Clipboard.SetText;
    }
};
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Mobile/Views/MainPage.xaml.cs
BindingContextChanged += (sender, args) =>
{
    (BindingContext as IXamlRootGetter)?.SetXamlRootGetter(() => this);

    if (BindingContext is ICopyToClipboard copy)
    {
        copy.CopyTextToClipboard = (text) =>
        {
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.Default.SetTextAsync(text); //Not necessary to await this
            }
        };
    }
};
```

**Where to look.**
`JustBetweenUs/Shared/ViewModels/MainViewModel.cs`
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml.cs`
`JustBetweenUs/JustBetweenUs.WinUI/Views/MainPage.xaml.cs`
`JustBetweenUs/JustBetweenUs.Wpf/Views/MainWindow.xaml.cs`
`JustBetweenUs/Mobile/Views/MainPage.xaml.cs`

**Sharp edges.**
- The interface is declared in the view model's own file, not in a head assembly.
  That is what lets four unrelated UI stacks satisfy it.
- The wiring is done in the data-context-changed handler and subscribed before
  `InitializeComponent()`, because on some heads `InitializeComponent()` is what
  sets the data context.
- The graceful-degradation branch is the whole point of the null check: a head
  that supplies nothing still runs and tells the user why the button did nothing.
  Nothing throws.
- Three implementations use three different clipboard APIs, which is why the
  bridge is a delegate rather than a method the view model could call directly.

### Put a platform service behind an interface with a no-op default

**When you want this.** A headless model wants to cut, copy and paste - or use any
other platform capability - but must run in tests and must not break on a head
where the capability is partial.

**The MVVM shape.** The model declares the interface and holds a null-object
implementation from the start, so every call site can be unconditional. The UI
layer installs the real one at startup.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/IClipboardService.cs
public interface IClipboardService
{
	void SetText (string text);

	Task<string?> GetTextAsync ();

	void SetImage (ImageSurface surface);

	Task<ImageSurface?> GetImageAsync ();
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/PintaCore.cs
/// <summary>
/// Installs the UI-layer clipboard implementation. Call once at startup.
/// </summary>
/// <remarks>
/// Until this is called the clipboard is a no-op that reports nothing
/// available, so engine code can call it unconditionally.
/// </remarks>
public static void InitializeClipboard (IClipboardService clipboard)
{
	Clipboard = clipboard;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PlatformServices.cs
public void SetImage (ImageSurface surface)
{
    // Encode as PNG and hand the platform a stream reference.
    // (Image WRITE is not yet supported by the X11 clipboard backend;
    // this degrades gracefully there.)
    using SKImage image = SKImage.FromBitmap (surface.Bitmap);
    using SKData data = image.Encode (SKEncodedImageFormat.Png, 100);

    InMemoryRandomAccessStream stream = new ();
    using (Stream outStream = stream.AsStreamForWrite ()) {
        data.SaveTo (outStream);
        outStream.Flush ();
    }
    stream.Seek (0);

    DataPackage package = new ();
    package.SetBitmap (RandomAccessStreamReference.CreateFromStream (stream));
    Clipboard.SetContent (package);
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/IClipboardService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/NullClipboardService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PlatformServices.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Clipboard image writing is not supported by every backend; the code notes it and
  degrades rather than throwing.
- Image transfer goes through an in-memory random-access stream holding the
  encoded bytes, seeked back to zero before the package is set.
- The reads are asynchronous and the writes are not, which is why the interface is
  asymmetric; keep the asymmetry rather than forcing a shape the platform does not
  have.

### Install UI dialogs into a headless model through handler delegates

**When you want this.** A library that must stay UI-free still needs to ask the
user something: an error, a confirmation, a configuration panel.

**The MVVM shape.** The model exposes `Initialize*` methods taking delegates; the
page - or, in a cleaner shape, the view model - installs them once it has a
`XamlRoot`. The model calls them without knowing what a dialog is.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ChromeManager.cs
public delegate Task<ErrorDialogResponse> ErrorDialogHandler (string message, string body, string details);
public delegate Task MessageDialogHandler (string message, string body);
public delegate Task<bool> SimpleEffectDialogHandler (BaseEffect effect, IWorkspaceService workspace);

public interface IProgressDialog
{
	void Show ();
	void Hide ();
	string Title { get; set; }
	string Text { get; set; }
	double Progress { get; set; }
	event EventHandler Canceled;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
//Chrome wiring: dialogs need a XamlRoot, so this happens on Loaded
PintaCore.Chrome.InitializeErrorDialogHandler(ShowErrorDialogAsync);
PintaCore.Chrome.InitializeMessageDialog(ShowMessageDialogAsync);
PintaCore.Chrome.InitializeProgessDialog(new ContentProgressDialog(() => XamlRoot));
//Custom effect dialogs route by effect type; everything else gets the
//reflection-generated dialog. Upstream's effects each opened their own
//Gtk dialog directly; here the Effects library stays UI-free, so the
//routing lives at this seam instead.
PintaCore.Chrome.InitializeSimpleEffectDialog(
    (effect, _) => effect switch
    {
        Effects.AlignObjectEffect align => Dialogs.AlignmentDialog.ShowAsync(align, XamlRoot),
        Effects.CurvesEffect curves => Dialogs.CurvesDialog.ShowAsync(curves, XamlRoot),
        Effects.LevelsEffect levels => Dialogs.LevelsDialog.ShowAsync(levels, XamlRoot),
        Effects.PosterizeEffect posterize => Dialogs.PosterizeDialog.ShowAsync(posterize, XamlRoot),
        _ => EffectOptionsDialog.ShowAsync(effect, XamlRoot),
    });
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Effects/Adjustments/PosterizeEffect.cs
public override Task<bool> LaunchConfiguration ()
{
	// Pinta.Brix note: upstream constructed the custom PosterizeDialog
	// directly; this library stays UI-free, so the dialog request goes
	// through the chrome seam and the UI layer routes it to the ported
	// PosterizeDialog by effect type.
	return chrome.LaunchSimpleEffectDialog (this, workspace);
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ChromeManager.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Effects/Adjustments/PosterizeEffect.cs`

**Sharp edges.**
- The wiring must happen on `Loaded`, not in the constructor: dialogs need a
  `XamlRoot` and there is none before then.
- The progress dialog takes a `Func<XamlRoot?>` rather than a `XamlRoot`, because
  it is constructed before the page has a root.
- The type-switch router is the one place that knows which items have bespoke
  dialogs; adding another is a one-line change there.

### Marshal a repeating timer into a headless model

**When you want this.** A library needs a periodic tick on the UI thread - a poll,
a progress update - but must not reference the dispatcher.

**The MVVM shape.** The model declares a one-method interface returning an
`IDisposable` handle; the UI layer implements it over the dispatcher queue's
timer. Until it is installed, a proxy forwards to nothing.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ITimerService.cs
public interface ITimerService
{
	/// <summary>
	/// Starts a repeating timer on the UI thread. The callback returns true
	/// to keep ticking or false to stop; disposing the returned handle also
	/// stops the timer.
	/// </summary>
	IDisposable Start (uint intervalMilliseconds, Func<bool> callback);
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PlatformServices.cs
public IDisposable Start (uint intervalMilliseconds, Func<bool> callback)
{
    Handle handle = new ();
    DispatcherQueueTimer timer = dispatcher.CreateTimer ();
    handle.Timer = timer;
    timer.Interval = TimeSpan.FromMilliseconds (intervalMilliseconds);
    timer.Tick += (_, _) => {
        if (!callback ())
            handle.Dispose ();
    };
    timer.Start ();
    return handle;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/NullServices.cs
/// <summary>
/// Forwards to the timer service the UI layer installs; before that, started
/// timers never tick.
/// </summary>
public sealed class TimerServiceProxy : ITimerService
{
	public ITimerService? Inner { get; set; }

	public IDisposable Start (uint intervalMilliseconds, Func<bool> callback)
		=> Inner?.Start (intervalMilliseconds, callback) ?? new NullHandle ();
}
```

The application installs the real one with the window's dispatcher queue:
`PintaCore.InitializeTimer(new DispatcherTimerService(MainWindow.DispatcherQueue))`.

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ITimerService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/NullServices.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PlatformServices.cs`
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`

**Sharp edges.**
- A proxy, not a null object, for the timer: the real implementation arrives after
  the model has already handed the proxy to other services, so those references
  must stay valid.
- The callback's `bool` return is the stop signal, and disposing the handle stops
  it too. Both paths matter, because callers use `using`.

### Set the mouse cursor from a model owned interface

**When you want this.** Your model decides which cursor is right - a tool, a hover
state, a drag - and the view must not hold that decision.

**The MVVM shape.** The model exposes a framework-free cursor descriptor; the view
maps it to the platform cursor in one switch. Unsupported descriptors degrade to
the closest available shape rather than failing.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs
public ToolCursor? Cursor {
    get => tool_cursor;
    set {
        tool_cursor = value;
        ProtectedCursor = InputSystemCursor.Create (MapCursor (value));
    }
}

private static InputSystemCursorShape MapCursor (ToolCursor? cursor)
{
    if (cursor is null)
        return InputSystemCursorShape.Arrow;

    // Icon/image cursors are approximated with a crosshair until custom
    // bitmap cursors are supported platform-side; tools also draw brush
    // outlines as canvas overlays, which carries most of the meaning.
    if (cursor.IconName is not null || cursor.Image is not null)
        return InputSystemCursorShape.Cross;

    return cursor.Shape switch {
        StandardCursor.Crosshair => InputSystemCursorShape.Cross,
        StandardCursor.Hand => InputSystemCursorShape.Hand,
        StandardCursor.Move => InputSystemCursorShape.SizeAll,
        StandardCursor.IBeam => InputSystemCursorShape.IBeam,
        StandardCursor.NotAllowed => InputSystemCursorShape.UniversalNo,
        StandardCursor.SizeNWSE => InputSystemCursorShape.SizeNorthwestSoutheast,
        // ...
        _ => InputSystemCursorShape.Arrow,
    };
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Input/ToolCursor.cs`

**Sharp edges.**
- `ProtectedCursor` is the seam on a `UIElement`; it is protected, so this only
  works from a subclass.
- Custom bitmap cursors are not available, so image-based cursors degrade to a
  crosshair. Plan for the degradation rather than assuming a bitmap cursor.

### Veto a window close until unsaved work is handled

**When you want this.** Your application holds unsaved documents and the window's
own close button is a way out.

**The MVVM shape.** The window's `Closed` event is the platform seam. The handler
vetoes the close, runs the async save-prompt loop, and re-issues the close when
the answer comes back.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Window-close save prompt. Closed is the platform's cancellable-close
//event: setting Handled vetoes the close, and the X11 head reports
//SupportsClosingCancellation. The save-prompt loop is async, so when
//dirty documents exist the close is vetoed first and re-issued once
//the user has decided.
MainWindow.Closed += async (_, e) =>
{
    if (windowCloseConfirmed) { return; }

    if (!Pinta.Brix.Engine.PintaCore.Workspace.OpenDocuments.Any(d => d.IsDirty)) { return; }

    e.Handled = true;

    try
    {
        if (Views.MainPage.Current is { } page && await page.ConfirmCloseApplicationAsync())
        {
            windowCloseConfirmed = true;
            MainWindow.Close();
        }
    }
    catch (Exception)
    {
        //A failed prompt must never take the window down with unsaved
        //work - the veto above stands and the application stays open.
    }
};
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.Dialogs.cs`

**Sharp edges.**
- A re-entrancy guard flag is mandatory: the confirmed `Close()` re-raises
  `Closed`, and without the flag the prompt loops forever.
- The prompt is asynchronous while the event is not, hence the veto-then-reissue
  shape rather than awaiting inside the veto decision.
- Wrap the whole body so a prompt failure leaves the veto standing rather than
  losing the user's work.
- Not every head has window chrome. An application whose only exit is the window
  button has no exit path at all on the framebuffer head.

### Tell the user when graphics initialization failed

**When you want this.** A GL-backed pane can be empty on a machine with no usable
driver, and an empty pane looks like a bug. This is the graceful-degradation path
for a hardware capability.

**The MVVM shape.** The page asks the canvas for its initialization state - a view
concern - and hands the state object to a view-model method that owns the message
and the dialog. Platform detection inside the message comes from `SimpleOsInfo`,
not from a compile-time switch.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Shows a dialog explaining why the 3D preview cannot render. Called from the view when
/// the Model View is active and the preview's GLCanvasElement reports that its OpenGL
/// initialization failed (e.g. on systems without OpenGL 3.0+ support, where the preview
/// would otherwise just be an empty pane).
/// </summary>
public async Task ShowRenderingUnavailableAsync(GLInitializationState state)
{
    var message =
        "The interactive 3D model preview is not available on this system, so the preview " +
        "pane will stay empty.\n\n";

    //On Windows, the usual cause is a missing OpenGL driver; Microsoft's free "OpenCL and
    //OpenGL Compatibility Pack" adds one. Only show this hint when actually on Windows.
    var osInfo = await SimpleOsInfo.GatherInfo(withConsoleOutput: false);
    if (osInfo.IsWindows)
    {
        message += "On Windows, you may be able to fix this by installing the free Microsoft " +
            "\"OpenCL and OpenGL Compatibility Pack\"...\n\n";
    }

    message += $"Details:\nStatus: {state.Status}\n{state.FailedReason ?? "(none reported)"}";

    using var dialog = CreateDialog(message, "3D Preview Unavailable");
    _ = await dialog.ShowAsync();
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs
//The canvas may only attempt its OpenGL initialization when it loads into the visual
//tree, which can happen after IsModelViewActive is set - so check at both moments.
ModelCanvas.Loaded += (_, _) => _ = MaybeReportRenderingUnavailableAsync();

//When the Model View is active and the preview canvas reports failed OpenGL initialization,
//surface the failure (status + reason) in a dialog instead of leaving a silently empty pane.
private async Task MaybeReportRenderingUnavailableAsync()
{
    if (_renderingUnavailableReported || ViewModel is not { IsModelViewActive: true } viewModel)
    {
        return;
    }

    var state = ModelCanvas.GetGLInitializationState();
    if (state.Status == GLInitializationStatus.InitializationFailed)
    {
        _renderingUnavailableReported = true;
        await viewModel.ShowRenderingUnavailableAsync(state);
    }
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs` and
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Check at two moments - the canvas's `Loaded` and the view's activation - because
  a collapsed canvas may not attempt initialization until it enters the visual
  tree.
- A page-level flag reports the failure once per run; without it the dialog
  reappears on every item the user opens.
- Decide the operating-system-specific hint with `SimpleOsInfo` rather than
  compiling it in, so the same message code runs on every head.

### Show a WebView on every head and drive it from a command

**When you want this.** Your application needs an embedded browser the user
navigates freely, and a command that sends it somewhere.

**The MVVM shape.** The view model declares a bridge with an `Action<string>` the
page sets, plus a method the page calls whenever the browser lands on a new URL.
The command builds the URL and marshals the navigation onto the UI thread; the
page does nothing but forward. The view model checks the delegate for null before
using it and never names a WebView type.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
public interface IWebViewBridge
{
    /// <summary>Navigates the embedded browser to the given URL (null when no WebView).</summary>
    Action<string> NavigateToUrl { get; set; }

    /// <summary>Called by the page whenever the embedded browser lands on a new URL.</summary>
    void SetCurrentBrowserUrl(string url);
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Shared/ViewModels/MainViewModel.cs
private Task DoSearch()
{
    //Every head has an embedded WebView: browse the real Wikipedia search page; the user
    //  picks an article by navigating to it, and Publish uses whatever page is displayed.
    if (CanSearch() && NavigateToUrl != null)
    {
        var searchUrl =
            $"https://{WikiHost}/w/index.php?search={Uri.EscapeDataString(SearchTerms.Trim())}";
        InvokeOnMainThread(() => NavigateToUrl(searchUrl));
        StatusText = "Browse to the article you want, then click Publish.";
    }

    return Task.CompletedTask;
}

public void SetCurrentBrowserUrl(string url)
{
    if (string.IsNullOrWhiteSpace(url)) { return; }

    InvokeOnMainThread(() =>
    {
        ArticleUrl = url;
        StatusText = IsPublishableArticleUrl(url)
            ? "Ready to publish this article."
            : "Browse to an article page to enable publishing.";
    });
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs
private void InitializeBrowser()
{
    if (_browserInitialized || DataContext is not MainViewModel viewModel) { return; }
    _browserInitialized = true;

    //Use CoreWebView2.Source (the authoritative current URL after redirects / user
    //  navigation); the XAML Browser.Source property does not reliably reflect those.
    Browser.NavigationCompleted += (_, _) =>
        viewModel.SetCurrentBrowserUrl(Browser.CoreWebView2?.Source ?? Browser.Source?.AbsoluteUri);

    viewModel.NavigateToUrl = url =>
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            Browser.Source = new Uri(url);
        }
    };

    Browser.Source = new Uri(MainViewModel.HomeUrl);
}
```

```xml
<!-- From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml -->
<!-- Center: embedded browser. Every Skia head now has a WebView2 - the Windows,
     Skia-on-WPF and macOS runtimes have it built in, and the Linux heads get it
     from the CodeBrix.Platform.WebView add-in (WPE WebKit). -->
<WebView2 Grid.Row="1" x:Name="Browser" />
```

**Using the CodeBrix.Platform WebView add-in.** The Linux Skia heads have no
built-in browser; the add-in supplies one, and it is referenced once in the
library that carries the application's packages so every head inherits it:

```xml
<!-- From CodeBrix.Samples/WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.Core/WikipediaPublisher.Core.csproj -->
<!-- WebView add-in: gives the Linux Skia heads an embedded WebView2 (WPE WebKit,
     offscreen). Referenced once here in Core; every Skia head inherits it transitively.
     The Windows, Skia-on-WPF and macOS runtimes already have WebView2 built in, so the
     add-in is inert there. The Linux heads need the system WPE WebKit engine at run time:
     sudo apt install libwpewebkit-2.0-1 libwpebackend-fdo-1.0-1 libwpe-1.0-1 -->
```

**Where to look.**
`WikipediaPublisher/Shared/ViewModels/MainViewModel.cs`
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs`
and `Views/MainPage.xaml`
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.Core/WikipediaPublisher.Core.csproj`

**Also shown by.**
`WikipediaPublisher/WikipediaPublisher.WinUI/Views/MainPage.xaml.cs`,
`WikipediaPublisher/WikipediaPublisher.Wpf/Views/MainWindow.xaml.cs` (a different
WebView control satisfying the same interface)

**Sharp edges.**
- Read the current URL from the core browser object, not from the XAML `Source`
  property; all three head implementations carry the same comment saying the XAML
  property does not reliably reflect redirects or user navigation.
- The Skia head wires the browser in a `Loaded` handler behind a guard flag,
  because `Loaded` can fire more than once.
- The system WPE WebKit engine is a run-time dependency, not a build one: the
  build succeeds on a machine that cannot run the WebView.
- Referencing the add-in once, in the shared library, is deliberate. It is inert
  where a WebView already exists, so one reference covers every head.
- On Windows the browser also constrains the head's entry point: see the
  synchronous-STA blueprint in the startup area.

### Replay a finished audio clip with one button press

**When you want this.** Your transport has a single Play button and the clip is
short. Without this, a clip that has run to its end does nothing when Play is
pressed again.

**The MVVM shape.** The page's bridge implementation is the natural home for the
element's own transport quirk, but the policy - Play means replay when the clip
has finished, resume when the user has scrubbed - is application behavior and
belongs on the view model, with the bridge exposing read-only transport facts and
a seek. The block below is adapted to that shape; the sample keeps the same logic
in the page.

**Code.**

```csharp
// Adapted from CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
// The sample implements this in the page; the logic is unchanged, but here the state and
// the decision live on the view model, and the bridge grows read-only transport facts.
public interface IAudioPlayerBridge
{
    // ... LoadAudioSource, PlayAudio, PauseAudio, StopAudio, SetAudioLooping ...

    /// <summary>Whether the player is currently advancing.</summary>
    Func<bool> IsAudioPlaying { get; set; }

    /// <summary>The player's position and the clip's duration.</summary>
    Func<TimeSpan> AudioPosition { get; set; }
    Func<TimeSpan> AudioDuration { get; set; }

    /// <summary>Moves the player to a position.</summary>
    Action<TimeSpan> SeekAudio { get; set; }
}

//How close to the duration still counts as "parked at the end". The player refreshes its
//position on an interval, so the last value it reports before ending can sit just short
//of the duration.
private static readonly TimeSpan AudioEndTolerance = TimeSpan.FromMilliseconds(250);
private bool _audioPlaybackEnded;

public SimpleCommand PlayAudioCommand => field ??= new SimpleCommand(() =>
{
    //A clip that has played through to its end leaves the transport parked at the end,
    //where Play alone has nothing left to play - so rewind first and let one click replay
    //the clip. Two things deliberately do NOT rewind: a player that is still going (a
    //looping clip raises PlaybackEnded on every pass), and a clip the user has scrubbed
    //away from the end since it finished - there, the thumb is the intent.
    if (_audioPlaybackEnded
        && IsAudioPlaying?.Invoke() == false
        && AudioDuration?.Invoke() > TimeSpan.Zero
        && AudioPosition?.Invoke() >= AudioDuration.Invoke() - AudioEndTolerance)
    {
        SeekAudio?.Invoke(TimeSpan.Zero);
    }

    _audioPlaybackEnded = false;
    PlayAudio?.Invoke();
});
```

```csharp
// Adapted from CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
AudioElement.PlaybackEnded += (_, _) => ViewModel?.NotifyAudioPlaybackEnded();
viewModel.IsAudioPlaying = () => AudioElement?.IsPlaying ?? false;
viewModel.AudioPosition = () => AudioElement?.Position ?? TimeSpan.Zero;
viewModel.AudioDuration = () => AudioElement?.Duration ?? TimeSpan.Zero;
viewModel.SeekAudio = position => AudioElement?.Seek(position);
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- A looping clip raises its playback-ended event on every pass while still
  playing, so the flag alone is not enough; the "is it playing" check is what
  stops a loop being rewound mid-play.
- The player refreshes its reported position on an interval, so the last position
  before the end can sit slightly short of the duration. A tolerance window is
  what makes the end-of-clip test reliable.
- Loading a new source and stopping both clear the flag.

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

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.Wpf/Views/MainWindow.xaml`
`WikipediaPublisher/CodeBrixPlatform/WikipediaPublisher.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- Check `CanExecute` before invoking; a key handler bypasses the disabled state a
  button would have honored.

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

## Graphics and rendering

### Host an OpenGL scene in XAML with a GLCanvasElement subclass

**When you want this.** You want hardware-accelerated 3D, or any OpenGL drawing,
inside an ordinary page, on every head that can give you a GL context, without
writing native GL, EGL, WGL or GLX code of your own.

**The MVVM shape.** The control is self-contained and lives in a library: it
exposes dependency properties, drives a renderer through the base class's
GL-thread lifecycle, and turns pointer input into camera motion itself. The page
places it and binds. The view model holds the loaded scene object and the play
state and knows nothing about GL.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/ModelSceneGlCanvas.cs
public sealed class ModelSceneGlCanvas : GLCanvasElement
{
    private readonly IModelSceneRenderer _renderer = new GlModelSceneRenderer();

    /// <summary>Creates the preview control and wires its pointer (rotate/zoom) input.</summary>
    // getWindowFunc is only used on WinUI; on CodeBrix.Platform heads it is null.
    public ModelSceneGlCanvas() : base(null)
    {
        _renderer.BackgroundColor = SolidBackground;

        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerReleased += OnPointerReleased;
        PointerCaptureLost += OnPointerCaptureLost;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    public static readonly DependencyProperty ModelProperty =
        DependencyProperty.Register(
            nameof(Model),
            typeof(LoadedModel),
            typeof(ModelSceneGlCanvas),
            new PropertyMetadata(null, OnModelChanged));

    public LoadedModel? Model
    {
        get => (LoadedModel?)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var canvas = (ModelSceneGlCanvas)d;

        // Re-frame each new model from the same default angle/margins the app has always used,
        // even if the user had orbited/zoomed the previous one.
        ApplyDefaultFraming(canvas._renderer.Camera);
        canvas._renderer.SetModel(e.NewValue as LoadedModel, frameCamera: true);
        canvas.Invalidate();
    }

    /// <inheritdoc />
    protected override void Init(GL gl) => EnsureInitialized(gl);

    // Compiles the renderer's GL resources exactly once. Called from both Init and RenderOverride
    // so it does not matter which the host invokes first: GLCanvasElement does not guarantee Init
    // runs before the first RenderOverride on every head (e.g. a canvas that starts collapsed).
    private void EnsureInitialized(GL gl)
    {
        if (_rendererInitialized) { return; }
        _renderer.Initialize(gl);
        _rendererInitialized = true;
    }

    /// <inheritdoc />
    protected override void RenderOverride(GL gl)
    {
        EnsureInitialized(gl);

        // Both this preview and the head's own Skia renderer share the GL context, so save the
        // state we touch and restore it afterwards (the GLCanvasElement contract). The base has
        // already bound the off-screen framebuffer and set the viewport before calling us.
        var depthWasEnabled = gl.IsEnabled(EnableCap.DepthTest);
        var cullWasEnabled = gl.IsEnabled(EnableCap.CullFace);
        try
        {
            _renderer.Render(gl, (uint)RenderSize.Width, (uint)RenderSize.Height);
        }
        finally
        {
            if (depthWasEnabled) { gl.Enable(EnableCap.DepthTest); } else { gl.Disable(EnableCap.DepthTest); }
            if (cullWasEnabled) { gl.Enable(EnableCap.CullFace); } else { gl.Disable(EnableCap.CullFace); }
            gl.BindVertexArray(0);
            gl.UseProgram(0);
        }
    }

    /// <inheritdoc />
    protected override void OnDestroy(GL gl)
    {
        _renderer.Uninitialize(gl);
        _rendererInitialized = false;
    }
}
```

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
<!-- 3D models: a self-contained GL canvas that draws the bound model
     and handles rotate/zoom itself; animated models additionally
     bind their baked clip and play state -->
<render:ModelSceneGlCanvas x:Name="ModelCanvas"
                           Model="{d:Binding CurrentModel}"
                           AnimationClip="{d:Binding CurrentAnimationClip}"
                           IsAnimationPlaying="{d:Binding IsAnimationPlaying}"
                           Visibility="{d:Binding ModelViewerVisibility}" />
```

The view-model side is a plain property plus a change notification, with the parse
done off the UI thread:

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>The model shown in the 3D preview (null while browsing); the preview control binds to this.</summary>
public LoadedModel CurrentModel => _currentModel;

private async Task OpenModelViewAsync(PolyHavenAsset asset, DownloadedModel downloaded)
{
    //Parse the glTF and gather its stats off the UI thread; GPU upload happens lazily
    //at first paint.
    var (model, stats) = await Task.Run(() =>
    {
        var loaded = new GltfModelLoader().LoadFile(downloaded.GltfPath);
        return (loaded, ModelFileStats.FromLoadedModel(loaded, downloaded.ModelFolder));
    });

    // Hand the model to the preview control via its bound CurrentModel; the control frames
    // the camera and repaints itself. The GPU upload happens lazily at its first render.
    _currentModel = model;
    // ...
    NotifyPropertyChanged(nameof(CurrentModel));
    IsModelViewActive = true;
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/ModelSceneGlCanvas.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs`
`PolyHavenBrowser/src/PolyHavenBrowser.UI/Views/MainPage.xaml` and
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Initialization must be idempotent and called from both `Init` and
  `RenderOverride`, because the base does not guarantee `Init` runs first on every
  head. A canvas that starts collapsed is the case the code calls out.
- The head's own Skia renderer shares the GL context, so every state the render
  touches - depth test, face culling, the bound vertex array, the program - is
  saved and restored in a `finally`. That is the element's contract, and the
  `finally` matters so a renderer exception cannot leave the head's context in a
  state it did not choose.
- The base binds the off-screen framebuffer and sets the viewport before calling
  the render override; do not do it again. `RenderSize` is the size to render at.
- The base constructor takes a window accessor that only matters on WinUI; pass
  null on the platform heads.
- `Invalidate()` coalesces to one paint per frame, so calling it from every
  pointer move is fine.
- When GL initialization fails outright, tell the user rather than showing an
  empty pane; see the bridge area.

### Keep the GL renderer framework-free behind an interface

**When you want this.** You want the drawing code unit-testable, swappable and
readable without a XAML type in sight, and the control to be nothing but lifecycle
and input.

**The MVVM shape.** The renderer interface mirrors the canvas lifecycle exactly
and says which thread each member is called on. The control owns the interface;
the concrete shader renderer knows nothing about the element, the framebuffer or
pointer input, which is what also lets an off-screen pipeline drive the same
renderer.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/IModelSceneRenderer.cs
/// <summary>
/// A framework-free OpenGL renderer for previewing one <see cref="LoadedModel"/> with an
/// orbit camera. The lifecycle mirrors the Graphics3DGL <c>GLCanvasElement</c> contract:
/// the host element calls <see cref="Initialize"/> from its <c>Init(GL)</c>,
/// <see cref="Render"/> from <c>RenderOverride(GL)</c>, and <see cref="Uninitialize"/>
/// from <c>OnDestroy(GL)</c> — all on the GL thread. <see cref="SetModel"/> may be called
/// from any thread; the model is uploaded on the next render.
/// </summary>
public interface IModelSceneRenderer
{
    OrbitCamera Camera { get; }
    (float R, float G, float B, float A) BackgroundColor { get; set; }

    void SetModel(LoadedModel? model, bool frameCamera = true);
    void SetFrameVertices(IReadOnlyList<ModelFramePrimitive>? frame);

    /// <summary>Compiles shaders and creates GL resources. Call once, on the GL thread.</summary>
    void Initialize(GL gl);

    /// <summary>Renders the scene into the currently bound framebuffer at the given pixel size.</summary>
    void Render(GL gl, uint width, uint height);

    /// <summary>Deletes all GL resources. Call once when the canvas is destroyed, on the GL thread.</summary>
    void Uninitialize(GL gl);
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/GlModelSceneRenderer.cs
/// <inheritdoc />
public void SetModel(LoadedModel? model, bool frameCamera = true)
{
    lock (_pendingLock)
    {
        _pendingModel = model;
        _pendingFrameCamera = frameCamera;
        _hasPendingModel = true;
    }
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/IModelSceneRenderer.cs`
and `GL/GlModelSceneRenderer.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/IModelSceneRenderer.cs`

**Also shown by.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ModelShotRenderer.cs`
(the same renderer driven into an off-screen framebuffer for a document)

**Sharp edges.**
- The setter that can be called from any thread never touches GL: it stashes the
  new data behind a lock, and all buffer creation and deletion happens inside the
  render call on the GL thread. That is what makes the "any thread" promise safe.
- The previous scene's buffers and textures are released before the new upload, in
  the same GL-thread call.
- Uniform locations are cached once at link time rather than looked up per frame.
- The camera math has no GL dependency at all, which is why it has a full unit-test
  file.

### Pick the shader version header for desktop GL or GLES at runtime

**When you want this.** One set of shaders that must run on every head, where some
give you desktop OpenGL and others give you OpenGL ES.

**The MVVM shape.** Renderer-internal. Keep the shader bodies version-agnostic and
prepend the header after probing the live context.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs
public void Initialize(GL gl)
{
    ArgumentNullException.ThrowIfNull(gl);

    // The same shader source runs on desktop OpenGL and OpenGL ES; only the #version header
    // differs, so we detect the context type and prepend the right one.
    var isGles = (gl.GetStringS(StringName.Version) ?? string.Empty)
        .Contains("OpenGL ES", StringComparison.OrdinalIgnoreCase);
    var header = isGles ? "#version 300 es\n" : "#version 330 core\n";

    var vertexShader = CompileShader(gl, ShaderType.VertexShader, header + VertexShaderBody);
    var fragmentShader = CompileShader(gl, ShaderType.FragmentShader, header + FragmentShaderBody);
    // ... link, check LinkStatus, then cache every uniform location once ...
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs`

**Also shown by.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/GlModelSceneRenderer.cs`
(whose class summary names the mapping: the Win32 and WPF hosts and X11 give
desktop GL, while ANGLE, Wayland EGL and the framebuffer wrapper give GLES)

**Sharp edges.**
- Open the fragment shader body with a precision qualifier, which desktop GL
  accepts and GLES requires, so the same body works under both headers.
- Throw with the driver's info log when compilation or linking fails. A silent
  black canvas is much harder to diagnose.

### Share one camera and one matrix convention across graphics APIs

**When you want this.** Your scene looks right head-on but goes flat, or depth
stops working, as soon as the camera is rotated off an axis - or you have more
than one backend and want the camera math written once.

**The MVVM shape.** The camera lives in the headless library, is owned by
whichever renderer is active, and is exposed up the chain so pointer input drives
it identically regardless of backend. The matrices go to the GPU untransposed.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs
// MVP = view * projection (node transforms are baked at load time). System.Numerics
// stores matrices row-major; uploading with transpose=false makes GL read that
// row-major data as its own column-major layout, which is exactly the transpose GL
// needs. Calling Matrix4x4.Transpose here as well would double-transpose and flatten
// the depth axis for any non-axis-aligned camera.
var mvp = Camera.GetViewMatrix() * Camera.GetProjectionMatrix(width / (float)height);
gl.UniformMatrix4(_mvpLocation, 1, false, (float*)&mvp);
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Cameras/OrbitCamera.cs
/// <summary>Frames the camera on a bounding box, keeping the current yaw/pitch.</summary>
public void FitToBounds(Vector3 boundsMin, Vector3 boundsMax)
{
    Target = (boundsMin + boundsMax) * 0.5f;
    var radius = Math.Max((boundsMax - boundsMin).Length() * 0.5f, 0.001f);
    // Distance so the bounding sphere fits the vertical fov, with FitMargin of headroom.
    Distance = radius / MathF.Sin(FovDegrees * MathF.PI / 360f) * FitMargin;
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Cameras/OrbitCamera.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md`

**Sharp edges.**
- Do not add a transpose. GLSL, SPIR-V and MSL all read a four-by-four matrix
  column-major, which already applies the transpose that .NET's row-major storage
  needs. Transposing again silently flattens the depth axis, and only for rotated
  cameras, so an axis-aligned test view hides the bug entirely.
- The regression test that pins it uses a rotated camera on purpose and tries both
  draw orders; it exists once per renderer. See the testing area.
- There is no per-node model matrix in these samples: the loader bakes node world
  transforms into the vertex data, so one shared matrix draws everything.
- The projection maps depth to the zero-to-one range, which is Vulkan's and
  Metal's native range; desktop GL merely wastes half of its own range on it.

### Frame the camera automatically on each newly bound model

**When you want this.** Every model you load should appear well composed, at a
consistent angle, whatever the user did to the previous one.

**The MVVM shape.** The element resets framing in its dependency-property
callback; the camera exposes the framing policy as properties so a document
pipeline can choose different ones.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs
// The starting camera framing for every model: a gentle three-quarter angle that sits the
// model a little low in the frame. Applied before framing so FitToModel uses these margins.
private static void ApplyDefaultFraming(OrbitCamera camera)
{
    camera.FovDegrees = 45f;
    camera.YawDegrees = 30f;
    camera.PitchDegrees = 15f;
    camera.FitMargin = 0.73f;
    camera.VerticalFramingBias = 0.22f;
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Cameras/OrbitCamera.cs
/// <summary>Frames the camera on a model's bounding box, keeping the current yaw/pitch.</summary>
public void FitToModel(LoadedModel model)
{
    ArgumentNullException.ThrowIfNull(model);
    FitToBounds(model.BoundsMin, model.BoundsMax);
    //Orbit around the centroid when the model provides one, so a model with a sparse
    //extremity rotates in place instead of swinging around the bounding-box center. Raise
    //the look-at point by VerticalFramingBias so the model can sit lower in the view.
    var radius = MathF.Max((model.BoundsMax - model.BoundsMin).Length() * 0.5f, 0.001f);
    Target = (model.Pivot ?? model.BoundsCenter) + new Vector3(0f, radius * VerticalFramingBias, 0f);
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Cameras/OrbitCamera.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/ModelSceneGlCanvas.cs`

**Sharp edges.**
- Framing on the vertex centroid rather than the bounding-box center keeps a model
  with one sparse extremity rotating in place.
- Derive the near and far planes from the current distance, so a model at any
  scale keeps usable depth precision.
- Clamp pitch, distance and field of view inside the camera, so the element does
  not have to guard any of it.
- Set framing before handing over the model: framing is applied when the pending
  model is taken up at render time.

### Draw translucent surfaces in a second pass with depth writes off

**When you want this.** Glass in your scene is hiding what is behind it instead of
showing it.

**The MVVM shape.** Renderer-internal. Classify materials at load time, then draw
twice: opaque first with depth writes on, then the translucent primitives with
blending and depth writes off.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs
// Two passes over the primitives: opaque (and mask) first with depth writes on, then the
// translucent (BLEND) primitives over them with blending and depth writes off, so
// glass-like surfaces show what's behind them instead of occluding it. BLEND primitives
// are not depth-sorted - fine for the small amount of transparent geometry these preview
// models carry.
DrawPrimitives(gl, blendPass: false);

gl.Enable(EnableCap.Blend);
// Straight-alpha "over": colour is weighted by source alpha, while the alpha channel
// accumulates coverage (One, OneMinusSrcAlpha) so a region already opaque behind the
// glass stays opaque.
gl.BlendFuncSeparate(
    BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha,
    BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
gl.DepthMask(false);
DrawPrimitives(gl, blendPass: true);
gl.DepthMask(true);
gl.Disable(EnableCap.Blend);
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Models/GltfModelLoader.cs
// ...plus KHR_materials_transmission glass (e.g. a camera's lens/flash/viewfinder),
// which is alphaMode OPAQUE yet see-through. This preview doesn't implement real
// transmission/refraction, so treat any transmissive material as translucent and render it
// with the same fixed preview opacity as BLEND surfaces, rather than as an opaque solid.
// FindChannel("Transmission") returns a channel only when the extension is present (glTF
// exporters write it only for actual glass), so its presence is a reliable glass signal.
if (alphaMode == ModelAlphaMode.Opaque && material.FindChannel("Transmission") is not null)
{
    alphaMode = ModelAlphaMode.Blend;
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Models/GltfModelLoader.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Models/ModelMaterial.cs`

**Sharp edges.**
- glTF marks glass two ways and the second is easy to miss: a blend alpha mode,
  and a transmission extension on an otherwise opaque material.
- Exporters commonly mark glass as blended while leaving the base-color alpha
  fully opaque, so honoring the file alone still gives you a solid pane. A fixed
  preview opacity multiplied onto whatever real alpha the material carries fixes
  it, and riding it in the existing base-color alpha means no shader change.
- The alpha channel must accumulate coverage, or a region already opaque behind
  the glass loses its opacity when the frame is composited onto Skia.
- Blended primitives are deliberately not depth-sorted; that is acceptable for the
  small amount of transparent geometry preview models carry.
- Face culling is disabled outright in these renderers, because low-poly models
  frequently rely on double-sided rendering.
- Vulkan expresses the same thing as a second pipeline with blending enabled and
  depth writes disabled, drawn after the opaque pass.

### Render off screen product shots on the head own GL context

**When you want this.** You need high-resolution stills of your 3D scene for a
document or an export, at a size the on-screen canvas never has.

**The MVVM shape.** The view model orchestrates in ordered stages with a bound
status line; GL work stays on the UI thread inside a `using` over the context,
while the CPU-only work around it runs on `Task.Run`. When no context can be
created, the pipeline still completes with a thumbnail-led result.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
//Stage 2: build the photography sets (pure CPU — off the UI thread).
var scenes = await Task.Run(() => (
    Tabletop: ShotSceneBuilder.Build(model, stages.Tabletop),
    Light: ShotSceneBuilder.Build(model, stages.Light),
    Dark: ShotSceneBuilder.Build(model, stages.Dark)));

//Stage 3: the product shots, on this head's off-screen GL context. GL work must
//  stay on the UI thread; MakeCurrent saves/restores the head's own context.
//  With no GL available the sheet still composes, led by the catalog thumbnail.
DocumentStatusText = "Rendering product shots…";
byte[] heroShot = null;
var galleryShots = new List<MarketingSheetShot>();
if (OffscreenGLContext.TryCreate(GetXamlRoot(), out var glContext))
{
    using (glContext)
    using (glContext.MakeCurrent())
    using (var shotRenderer = new ModelShotRenderer(glContext.Gl))
    {
        heroShot = shotRenderer.RenderPng(
            scenes.Tabletop, stages.Tabletop, ShotAngle.Hero, HeroShotWidth, HeroShotHeight);
        galleryShots.Add(new MarketingSheetShot("Front", shotRenderer.RenderPng(
            scenes.Light, stages.Light, ShotAngle.Front, GalleryShotWidth, GalleryShotHeight)));
        // ... Side, Back, Top ...
    }
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ModelShotRenderer.cs
//Rendered pixels per output pixel, per axis. 2 gives 4 samples per output pixel.
private const uint Supersample = 2;

//A conservative ceiling for the supersampled framebuffer, kept below every desktop
//  GL/GLES 3.0 implementation's minimum guarantees.
private const uint MaxFramebufferSide = 4096;

//Flips the GL-oriented (bottom-up) pixels the right way up, downscales the supersampled
//  frame to the requested output size, and encodes a PNG.
private static unsafe byte[] EncodePng(byte[] pixels, int frameWidth, int frameHeight, int width, int height)
{
    // ... row-reverse copy into an SKBitmap, ScalePixels with Mitchell resampling, then Encode ...
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ModelShotRenderer.cs`

**Sharp edges.**
- GL work must stay on the UI thread; the context's `MakeCurrent()` returns a
  disposable that saves and restores the head's own context, so the head is
  untouched afterwards.
- Read-back pixels are bottom-up. Flip them before encoding.
- A compatibility-first GL context may offer no multisampling, so supersample and
  downscale instead, with a conservative framebuffer-size ceiling.
- Every member of the shot renderer, disposal included, must be called on the
  thread where the context is current, which is why a `using` block is the only
  correct shape.

### Generate scene set dressing as ordinary geometry

**When you want this.** You want a studio floor, backdrop and contact shadow
behind a model, without adding a second rendering path to maintain.

**The MVVM shape.** A builder that returns a composite of the same primitive and
material types the loader produces, so one renderer pass draws model and set
together with correct depth and blending, and the model itself is never modified.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ShotSceneBuilder.cs
//The stage materials are appended after the model's, so the model primitives'
//  material indices stay valid untouched.
var floorMaterialIndex = model.Materials.Count;
var coveMaterialIndex = model.Materials.Count + 1;

// ... build the floor quad and the cove cylinder ...

var composite = new LoadedModel
{
    Name = model.Name,
    Primitives = primitives,
    Materials = materials,
    //The model's own bounds and pivot, so cameras frame the product, not the set.
    BoundsMin = model.BoundsMin,
    BoundsMax = model.BoundsMax,
    Pivot = model.Pivot,
};

return new ShotScene(composite, [floorPrimitive, covePrimitive], model.Primitives);
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ShotSceneBuilder.cs
/// <summary>
/// Points every stage normal straight at the light, so the stage renders fully,
/// evenly lit (its look comes from its baked textures, not from shading) while the
/// model keeps real form from the same light. Call before each shot, then re-set the
/// scene on the renderer so the changed normals re-upload.
/// </summary>
public void AimStageAtLight(Vector3 lightDirection)
{
    var direction = lightDirection == Vector3.Zero ? Vector3.UnitY : Vector3.Normalize(lightDirection);
    foreach (var primitive in StagePrimitives)
    {
        var normals = primitive.Normals;
        for (var i = 0; i < normals.Length; i += 3)
        {
            normals[i] = direction.X;
            normals[i + 1] = direction.Y;
            normals[i + 2] = direction.Z;
        }
    }
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ShotSceneBuilder.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Shots/ShotStage.cs`

**Sharp edges.**
- Append the set's materials after the model's so every existing material index
  stays valid, and keep the composite's bounds and pivot as the model's, or the
  camera frames the furniture instead of the product.
- Mutating stage normals per shot means the scene must be re-set on the renderer
  before each shot so the changed data re-uploads.
- Baking the contact shadow into the floor texture rather than computing it is
  what keeps this a single-pass, shader-free addition.

### Swap the 3D graphics backend at run time from a dropdown

**When you want this.** You render with a GPU API and want the user, or a support
engineer, to pick between backends while the application is running, without
restarting and without every layer above the GPU knowing which one is active.

**The MVVM shape.** One interface is the seam. A selector registered as a
singleton owns the list of kinds, the per-platform gate and engine creation. The
view model exposes the kind names as a bound list and the selection as a bound
string property; its setter starts the switch, and everything above the seam -
painter, camera, model loading, XAML - is unchanged by the choice.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IModelRenderEngine.cs
public interface IModelRenderEngine : IDisposable
{
    OrbitCamera Camera { get; }
    Vector3? FixedLightDirection { get; set; }
    void SetModel(LoadedModel model);
    RenderedFrame RenderFrame(int width, int height, (float R, float G, float B, float A) background);
}

public readonly struct RenderedFrame
{
    public RenderedFrame(byte[] rgba, int width, int height, bool isBottomUp) { /* ... */ }
    public byte[] Rgba { get; }
    public int Width { get; }
    public int Height { get; }
    public bool IsBottomUp { get; }
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IModelRenderEngineSelector.cs
public sealed class ModelRenderEngineSelector : IModelRenderEngineSelector
{
    private static readonly RenderEngineKind[] Kinds =
        [RenderEngineKind.OpenGL, RenderEngineKind.Vulkan, RenderEngineKind.Metal];

    public IReadOnlyList<RenderEngineKind> AvailableKinds => Kinds;

    public bool IsSupported(RenderEngineKind kind) => kind switch
    {
        RenderEngineKind.OpenGL => true,
        RenderEngineKind.Vulkan => VulkanPlatformSupport.IsCurrentPlatformSupported,
        RenderEngineKind.Metal => MetalPlatformSupport.IsCurrentPlatformSupported,
        _ => false,
    };

    public IModelRenderEngine Create(RenderEngineKind kind, Func<XamlRoot> getXamlRoot)
    {
        if (!IsSupported(kind))
        {
            throw new NotSupportedException($"The {kind} rendering engine is not supported on this platform.");
        }

        return kind switch
        {
            RenderEngineKind.OpenGL => new OpenGlModelRenderEngineFactory(getXamlRoot).Create(),
            RenderEngineKind.Vulkan => new VulkanModelRenderEngineFactory().Create(),
            RenderEngineKind.Metal => new MetalModelRenderEngineFactory().Create(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
```

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.UI/Views/MainPage.xaml -->
<ComboBox Width="130" Height="36" VerticalAlignment="Center"
          ItemsSource="{d:Binding RenderEngineNames}"
          SelectedItem="{d:Binding SelectedRenderEngineName, Mode=TwoWay}"
          IsEnabled="{d:Binding IsNotBusy}"
          Visibility="{d:Binding EngineSelectorVisibility}" />
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IModelRenderEngine.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IModelRenderEngineSelector.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md`

**Sharp edges.**
- The selector replaces the older shape of registering one fixed engine factory in
  the container; the per-backend factories still exist but are used inside the
  selector.
- Creation hands back a fresh engine and the caller owns and disposes it. Dispose
  the old painter, which disposes its engine, only after the new one is built.
- The list is deliberately not filtered to supported kinds, so the user learns why
  an option is unavailable; see the alert-and-revert blueprint in the view-model
  area, and pre-warm the new backend off the UI thread before swapping.

### Gate an optional graphics backend to specific heads with an allow list

**When you want this.** A capability works only on some of the six heads and you
want a predictable, testable policy rather than a driver probe that might
half-succeed on a head you never validated.

**The MVVM shape.** The policy lives in the headless library as a static class
with a pure support function plus a cached detection of the running head. The view
model never sniffs the platform itself; it asks the selector, which asks the
policy.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanPlatformSupport.cs
public static class VulkanPlatformSupport
{
    private const string HeadAssemblyPrefix = "CodeBrix.Platform.UI.Runtime.Skia.";

    private static readonly Lazy<PlatformHead> DetectedHead = new(DetectCurrentHead);

    public static bool IsSupported(PlatformHead head) => head switch
    {
        PlatformHead.LinuxX11 => true,
        PlatformHead.LinuxWayland => true,
        PlatformHead.Win32Skia => true,
        PlatformHead.WinWpfSkia => true,
        _ => false,
    };

    public static bool IsCurrentPlatformSupported => IsSupported(CurrentHead);

    public static PlatformHead CurrentHead => DetectedHead.Value;

    public static PlatformHead ClassifyAssemblyName(string? assemblyName)
    {
        if (assemblyName is null || !assemblyName.StartsWith(HeadAssemblyPrefix, StringComparison.Ordinal))
        {
            return PlatformHead.Unknown;
        }

        var head = assemblyName[HeadAssemblyPrefix.Length..];
        if (head == "X11") { return PlatformHead.LinuxX11; }
        if (head == "Wayland") { return PlatformHead.LinuxWayland; }
        if (head == "Linux.FrameBuffer") { return PlatformHead.LinuxFrameBuffer; }
        if (head == "MacOS") { return PlatformHead.MacOS; }
        if (head == "Wpf") { return PlatformHead.WinWpfSkia; }
        if (head == "Win32" || head.StartsWith("Win32.", StringComparison.Ordinal))
        {
            return PlatformHead.Win32Skia;
        }

        return PlatformHead.Unknown;
    }
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanPlatformSupport.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Metal/MetalPlatformSupport.cs`

**Sharp edges.**
- The class comments call this out as a deliberate policy list, not a driver
  probe: an API is never even attempted on a platform that has not been okayed.
- Detection is a one-time scan of loaded assemblies for a head runtime assembly
  name, so the library needs no reference to any head. Prefix matching is used so
  satellite assemblies still classify to their head.
- An unrecognized host classifies as unknown and is conservatively unsupported,
  which is also what a unit-test host is; there is a test asserting exactly that.
- The Metal gate adds a process-architecture condition, keyed off the process
  rather than the machine so a translated process is classified correctly.

### Render an OpenGL scene off screen and composite it onto an SKXamlCanvas

**When you want this.** You want real GPU 3D inside an ordinary XAML page on every
head, without writing any platform GL loader code and without fighting the head's
own renderer for the context.

**The MVVM shape.** The engine is a plain class behind the engine interface; it is
created by the view model and driven from a painter, which the page calls from its
paint handler. The only thing the page supplies is a XAML root, and it supplies it
as an accessor.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/OpenGlModelRenderEngine.cs
public RenderedFrame RenderFrame(int width, int height, (float R, float G, float B, float A) background)
{
    // Create the off-screen context on first use, from the hosting page's XamlRoot. This runs
    // on the render thread (the Skia paint callback), so the XamlRoot accessor is valid here.
    if (_context == null)
    {
        var xamlRoot = _getXamlRoot()
            ?? throw new InvalidOperationException(
                "A XamlRoot is required to create the offscreen OpenGL context, but none was available.");
        if (!OffscreenGLContext.TryCreate(xamlRoot, out _context))
        {
            throw new InvalidOperationException(
                "The running head does not provide a native OpenGL context for offscreen rendering.");
        }
    }

    // MakeCurrent() saves whatever context the host head had current and restores it when the
    // returned scope is disposed, so this engine never disturbs the head's own renderer even
    // though they share a thread.
    using (_context.MakeCurrent())
    {
        var gl = _context.Gl;

        if (!_rendererInitialized)
        {
            _renderer.Initialize(gl);
            _rendererInitialized = true;
        }

        EnsureFramebuffer(gl, (uint)width, (uint)height);

        _renderer.BackgroundColor = background;
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
        _renderer.Render(gl, (uint)width, (uint)height);

        var pixels = new byte[width * height * 4];
        gl.ReadPixels(0, 0, (uint)width, (uint)height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        return new RenderedFrame(pixels, width, height, isBottomUp: true);
    }
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/OpenGlModelRenderEngine.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/RENDERING-PIPELINE.md`

**Sharp edges.**
- A GL context must be created and used on the thread that renders. That is why
  the engine takes an accessor and not a value, and why the context is created
  lazily inside the first render rather than in the constructor. Constructing the
  engine stays cheap and thread-agnostic.
- The framebuffer, color renderbuffer and depth renderbuffer are recreated on
  every size change and the status checked; without the depth attachment 3D
  geometry does not occlude correctly.
- Disposal also has to happen with the context current, so `Dispose()` re-enters
  the make-current scope before deleting anything.
- The read-back is the main per-frame cost, which is why this application drops
  frames under load rather than lowering resolution.

### Add a self contained Vulkan renderer that needs no shader toolchain

**When you want this.** You want a second GPU backend that cannot possibly collide
with the head's renderer, and you do not want a shader compiler to become a build
prerequisite.

**The MVVM shape.** The whole Vulkan stack lives in the headless rendering
library. The display layer contributes only a thin adapter that implements the
engine interface and reports the frame's orientation.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/VulkanModelRenderEngine.cs
public sealed class VulkanModelRenderEngine : IModelRenderEngine
{
    private readonly VulkanSceneRenderer _renderer = new();

    public OrbitCamera Camera => _renderer.Camera;

    public Vector3? FixedLightDirection
    {
        get => _renderer.FixedLightDirection;
        set => _renderer.FixedLightDirection = value;
    }

    public void SetModel(LoadedModel model) => _renderer.SetModel(model);

    public RenderedFrame RenderFrame(int width, int height, (float R, float G, float B, float A) background)
    {
        var pixels = _renderer.RenderFrame(width, height, background);

        // The renderer keeps the camera's GL-convention matrices unmodified, and Vulkan's
        // clip-space Y points the other way - so its readback is a bottom-up image just like
        // GL's, and the same Skia flip applies (see the VulkanSceneRenderer class remarks).
        return new RenderedFrame(pixels, width, height, isBottomUp: true);
    }

    public void Dispose() => _renderer.Dispose();
}

public sealed class VulkanModelRenderEngineFactory : IModelRenderEngineFactory
{
    public IModelRenderEngine Create() => new VulkanModelRenderEngine();
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/VulkanModelRenderEngine.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanSceneRenderer.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Vulkan/VulkanShaders.cs`

**Sharp edges.**
- Vulkan has no ambient thread state, so the renderer owns instance, devices,
  queue, command pool, off-screen color and depth images, render pass, pipelines,
  sampler, per-model buffers and textures, and the read-back. It never touches a
  window system, so it cannot collide with the head.
- Shaders are pre-compiled SPIR-V embedded as static word arrays with the source
  alongside in comments, so no shader compiler is needed at build or run time; the
  file's documentation records how to regenerate them.
- Per-draw values ride in one push-constant block shared by both stages, and a
  white fallback texture is bound for untextured materials because Vulkan still
  requires a valid sampler in the set.
- A runtime-availability probe exists for tests; the application itself uses the
  allow list plus a pre-warm render.

### Add a direct to Metal renderer with no NuGet package or Apple bindings

**When you want this.** You want a macOS GPU backend and would rather call the
Objective-C runtime yourself than take on a managed Apple binding dependency.

**The MVVM shape.** Identical to the Vulkan case: the whole stack is in the
headless library, and the display layer contributes a thin adapter plus its
factory.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Metal/MetalInterop.cs
internal static unsafe class MetalInterop
{
    private const string Objc = "/usr/lib/libobjc.A.dylib";
    private const string Metal = "/System/Library/Frameworks/Metal.framework/Metal";

    [DllImport(Objc, EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport(Objc, EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [DllImport(Metal, EntryPoint = "MTLCreateSystemDefaultDevice")]
    internal static extern IntPtr MTLCreateSystemDefaultDevice();

    // ---- objc_msgSend, one concrete signature per call shape (never a struct return) ------

    [DllImport(Objc, EntryPoint = "objc_msgSend")]
    internal static extern IntPtr Send(IntPtr receiver, IntPtr selector);

    [DllImport(Objc, EntryPoint = "objc_msgSend")]
    internal static extern void SendV(IntPtr receiver, IntPtr selector);

    // ...
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/MetalModelRenderEngine.cs
public RenderedFrame RenderFrame(int width, int height, (float R, float G, float B, float A) background)
{
    var pixels = _renderer.RenderFrame(width, height, background);

    // Unlike GL and Vulkan, Metal's clip-space Y points up while its framebuffer origin is
    // top-left, so the readback is already top-down - no Skia flip needed (see the
    // MetalSceneRenderer class remarks).
    return new RenderedFrame(pixels, width, height, isBottomUp: false);
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Metal/MetalInterop.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Metal/MetalSceneRenderer.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/MetalModelRenderEngine.cs`

**Sharp edges.**
- Never call an Objective-C method that returns a struct by value. That is the one
  place the message-send calling convention differs between Apple Silicon and
  Intel or translated processes. Every message here returns a pointer or void, or
  takes a struct only as an argument, so one entry point serves both
  architectures.
- Every transfer between GPU and CPU goes through a shared-storage staging buffer
  blitted to and from a private texture, which behaves identically on Apple
  Silicon, Intel and under translation, unlike shared or managed textures whose
  availability splits by GPU family. Buffer-to-texture blits require row
  alignment, so staging rows are padded and de-padded.
- Metal compiles its shading language from source at run time, so the shader file
  is just a string and no artifact is pre-baked - simpler than the Vulkan case.
- Metal enum values are declared as plain integer constants in the renderer, so no
  headers are needed.

### Composite engine pixels onto Skia with the right vertical orientation

**When you want this.** You read pixels back from a GPU API and draw them on a
Skia surface, and you need one compositing path that is correct for backends with
different framebuffer origins.

**The MVVM shape.** The painter is API-agnostic. It asks the engine for a frame,
reads the frame's own orientation flag and flips only when the flag says so. Each
engine declares its orientation; nothing else in the application knows about it.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/ModelScenePainter.cs
public void Paint(SKSurface surface, SKImageInfo info)
{
    if (_disposed || info.Width <= 0 || info.Height <= 0) { return; }

    // When a background texture is present, render the model over a transparent clear so it
    // composites onto the texture we draw behind it; otherwise use the solid dark colour.
    var hasBackground = _backgroundBitmap != null;
    var background = hasBackground ? (0f, 0f, 0f, 0f) : SolidBackground;

    // The engine does all the API-specific work and hands back RGBA pixels.
    var frame = _engine.RenderFrame(info.Width, info.Height, background);

    DrawFrame(surface, info, frame);
}

private void DrawFrame(SKSurface surface, SKImageInfo info, RenderedFrame frame)
{
    var canvas = surface.Canvas;
    var sampling = new SKSamplingOptions(SKFilterMode.Linear);

    DrawBackground(canvas, info, sampling);

    // Straight (unpremultiplied) alpha so a transparent clear lets the background show through.
    var imageInfo = new SKImageInfo(frame.Width, frame.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
    using var image = SKImage.FromPixelCopy(imageInfo, frame.Rgba);

    canvas.Save();
    if (frame.IsBottomUp)
    {
        // The engine's first pixel row is the bottom of the image; flip vertically to match
        // Skia's top-down surface.
        canvas.Scale(1f, -1f);
        canvas.Translate(0f, -info.Height);
    }
    canvas.DrawImage(image, new SKRect(0, 0, info.Width, info.Height), sampling);
    canvas.Restore();
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/ModelScenePainter.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/IModelRenderEngine.cs`

**Sharp edges.**
- OpenGL's first pixel row is the image bottom. Vulkan's clip-space Y points down,
  which with the same matrices makes its read-back come out inverted too, so it
  shares the flip. Metal's clip-space Y points up while its framebuffer origin is
  top-left, so it is the exception.
- Use unpremultiplied alpha, or the transparent clear used behind a background
  texture will not composite correctly.
- This painter always renders at full canvas resolution; smoothness under load
  comes from dropping frames, not from lowering resolution.

### Paint a CPU ray traced panorama into an SKBitmap

**When you want this.** You need an interactive image-based view that must work
even on a head with no GPU, or a fallback path that is entirely CPU-side.

**The MVVM shape.** A second painter implementation behind the same interface. The
page and the view model treat it exactly like the GPU painter; only the view model
knows which one is current, and it hides the backend dropdown while this one is
active.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/PanoramaScenePainter.cs
//Cap the CPU ray-traced resolution so a maximized window stays interactive; the result
//is scaled up to fill the canvas. A lower cap while dragging keeps the look-around smooth;
//the crisp full cap is used once the drag stops.
private const int MaxRenderDimension = 1440;
private const int DragRenderDimension = 960;

public void Paint(SKSurface surface, SKImageInfo info)
{
    if (_disposed || info.Width <= 0 || info.Height <= 0) { return; }

    var (renderWidth, renderHeight) = CapResolution(info.Width, info.Height);
    if (_buffer == null || _bufferWidth != renderWidth || _bufferHeight != renderHeight)
    {
        _buffer?.Dispose();
        _buffer = new SKBitmap(new SKImageInfo(renderWidth, renderHeight, SKColorType.Rgba8888, SKAlphaType.Opaque));
        _bufferWidth = renderWidth;
        _bufferHeight = renderHeight;
    }

    _renderer.RenderTo(_buffer);
    surface.Canvas.DrawBitmap(_buffer, new SKRect(0, 0, info.Width, info.Height), new SKSamplingOptions(SKFilterMode.Linear));
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/PanoramaScenePainter.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Panorama/EquirectPanoramaRenderer.cs`

**Sharp edges.**
- The bitmap is reused across frames and only reallocated on a size change; the
  renderer writes into it in place, in parallel over the rows.
- Two resolution caps - a lower one while dragging, the full one once the drag
  stops - make this a dynamic-quality strategy, the opposite of the GPU painter's
  drop-frames strategy.
- The renderer rejects a bitmap whose color type is not the one it writes.
- The panorama camera clamps pitch to avoid the poles and clamps the field of
  view.

### Decode HDR images and tone map them for display

**When you want this.** Your application shows high-dynamic-range content, or any
linear-light data, and you need one place that turns a file into either a display
bitmap or a float image.

**The MVVM shape.** A static loader in the headless library dispatches on file
extension; the view model calls it from a `Task.Run` and never sees a decoder.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Textures/TextureImageLoader.cs
public static SKBitmap LoadForDisplay(byte[] data, string fileExtension)
{
    ArgumentNullException.ThrowIfNull(data);
    ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);

    switch (NormalizeExtension(fileExtension))
    {
        case "exr":
            return NormalizeToBitmap(ExrDecoder.Decode(data));
        case "hdr":
            return ToneMapper.ToBitmap(RadianceHdrDecoder.Decode(data));
        default:
            return LdrImageDecoder.Decode(data);
    }
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/ToneMapping/ToneMapper.cs
internal static float ApplyOperator(float value, ToneMapOperator toneMapOperator)
{
    if (value <= 0f) { return 0f; }

    return toneMapOperator switch
    {
        // Krzysztof Narkowicz's ACES filmic approximation.
        ToneMapOperator.AcesFilmic =>
            Math.Clamp(value * (2.51f * value + 0.03f) / (value * (2.43f * value + 0.59f) + 0.14f), 0f, 1f),
        ToneMapOperator.Reinhard => value / (1f + value),
        ToneMapOperator.Clamp => Math.Min(value, 1f),
        _ => throw new ArgumentOutOfRangeException(nameof(toneMapOperator), toneMapOperator, "Unknown tone-map operator."),
    };
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Textures/TextureImageLoader.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/ToneMapping/ToneMapper.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Images/RadianceHdrDecoder.cs`

**Sharp edges.**
- The two HDR extensions are treated differently on purpose: one usually carries
  non-photographic data maps and gets min-max normalization so the full value
  range shows, while panoramas get real tone mapping.
- The float-image entry point deliberately refuses low-dynamic-range extensions
  rather than silently promoting them.
- The decoder narrows the imaging library's format exceptions to one exception
  type, so callers have one thing to catch; the model loader uses that to degrade
  an undecodable texture to the material's base color instead of failing the whole
  load.

### Build a textured cube mesh from a bitmap for previewing a flat material

**When you want this.** You have a flat texture and a swatch rectangle would not
show it as a material. A cube at a corner angle does.

**The MVVM shape.** A static builder that returns the same loaded-model type the
glTF loader produces, so the renderer path is identical for both sources.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/CubeMeshBuilder.cs
// Each face: four corners (CCW seen from outside) + its outward normal. UVs are the
// same (0,0)-(1,0)-(1,1)-(0,1) per face so the whole texture shows on every face.
private static readonly (Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 Normal)[] Faces = [ /* ... */ ];

public static LoadedModel Build(SKBitmap texture, string name)
{
    ArgumentNullException.ThrowIfNull(texture);
    var (rgba, width, height) = ToRgba(texture);
    // ... fill positions / normals / texCoords / indices per face ...

    return new LoadedModel
    {
        Name = name,
        Primitives = [primitive],
        Materials = [material],
        BoundsMin = new Vector3(-0.5f, -0.5f, -0.5f),
        BoundsMax = new Vector3(0.5f, 0.5f, 0.5f),
    };
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
//A fixed key light (from upper-front-left) shades the faces distinctly so the
//  cube reads as solid, and a 3/4 angle with perspective shows three faces.
//  Extra framing margin keeps the whole cube (and its rotating silhouette)
//  in view against the backdrop, which reads as clearly 3D.
_modelPainter.FixedLightDirection = new Vector3(-0.4f, 1f, 0.7f);
_modelPainter.Camera.FovDegrees = 40f;
_modelPainter.Camera.YawDegrees = 35f;
_modelPainter.Camera.PitchDegrees = 28f;
_modelPainter.Camera.FitMargin = 1.2f;
_modelPainter.Camera.VerticalFramingBias = 0f;
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Display/CubeMeshBuilder.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Set a fixed light for a solid symmetric shape. The default here is a camera
  headlight, which double-sides the lighting - good for flat or foliage models,
  but it makes a cube read as ambiguous because every face gets the same
  brightness.
- Camera framing must be set before handing over the model, because framing is
  applied when the pending model is taken up at render time.
- The model's texture is a plain byte array, so nothing GPU-specific leaks into
  the mesh data.

### Paint a zoomable image on an SKXamlCanvas from the view model

**When you want this.** You need a drawing surface whose content and zoom come
from the view model, repainted on demand from view-model code that has no access
to the control.

**The MVVM shape.** The painting logic is a plain Skia class, with no UI types,
that the view model owns as a property; the page's paint handler forwards the
canvas and size to it in one line. Repaint requests travel the other way through a
bridge whose single delegate the page fills in and marshals onto the UI thread.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/IImageCanvasBridge.cs
/// <summary>
/// The head-capability bridge for the 2D image viewer: the page fills in how the
/// SkiaSharp canvas is repainted (marshalled to the UI thread). The view model must
/// behave sensibly when the delegate is <c>null</c>.
/// </summary>
public interface IImageCanvasBridge { Action InvalidateImageCanvas { get; set; } }
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>The 2D canvas painter the page's SkiaSharp canvas paints with.</summary>
public ImageCanvasPainter ImagePainter { get; } = new();

/// <inheritdoc />
public Action InvalidateImageCanvas { get; set; }

public string ZoomText => $"{ImagePainter.ZoomFactor * 100:0}%";

public SimpleCommand ZoomInCommand => field ??= new SimpleCommand(() => AdjustZoom(1.25f));
public SimpleCommand ZoomOutCommand => field ??= new SimpleCommand(() => AdjustZoom(0.8f));

/// <summary>Applies one wheel notch of zoom from the page's pointer-wheel handler.</summary>
public void AdjustZoomFromWheel(int wheelDelta) => AdjustZoom(wheelDelta > 0 ? 1.25f : 0.8f);

private void AdjustZoom(float factor)
{
    ImagePainter.ZoomFactor = Math.Clamp(ImagePainter.ZoomFactor * factor, 0.25f, 16f);
    NotifyPropertyChanged(nameof(ZoomText));
    InvalidateImageCanvas?.Invoke();
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
//Marshal 2D-canvas invalidations from the view model onto the UI thread
viewModel.InvalidateImageCanvas = () => DispatcherQueue?.TryEnqueue(() => ImageCanvas?.Invalidate());

//The 2D viewer: the view model's painter draws images and spritesheets (checkerboard,
//zoom, sprite spotlight) onto this SkiaSharp surface.
ImageCanvas.PaintSurface += (_, e) =>
    ViewModel?.ImagePainter.Paint(e.Surface.Canvas, e.Info.Width, e.Info.Height);
ImageCanvas.SizeChanged += (_, _) => ImageCanvas.Invalidate();
ImageCanvas.PointerWheelChanged += (_, e) =>
{
    var delta = e.GetCurrentPoint(ImageCanvas).Properties.MouseWheelDelta;
    ViewModel?.AdjustZoomFromWheel(delta);
    e.Handled = true;
};
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/IImageCanvasBridge.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/ImageCanvasPainter.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- The invalidation delegate wraps its call in the dispatcher, because the view
  model raises it from continuations that may not be on the UI thread.
- A resize alone does not repaint, so the size-changed handler must invalidate.
- The painter's layout helpers are public so the mapping between canvas and image
  space is unit-testable without a canvas, and the test project exercises exactly
  that.
- Above a threshold the painter switches to nearest-neighbor sampling, which keeps
  low-resolution pixel art crisp instead of smearing it - the right default for
  game assets.
- The view model disposes the previous bitmap when swapping in a new one, and
  clears both the painter's bitmap and its highlight when the viewer closes.

### Spotlight one region of an image on the canvas

**When you want this.** You are showing an atlas, a scanned page or any image with
named sub-rectangles, and selecting one from a list should highlight it in place.

**The MVVM shape.** A row view model per region with its own command and selected
state; the owning view model handles selection, sets the painter's highlight
rectangle and asks for a repaint through the canvas bridge. The painter does the
dimming and the outline.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
private void SelectRegion(AtlasRegionCellViewModel row)
{
    //Re-selecting the spotlighted region clears the spotlight
    var selecting = !row.IsSelected;
    foreach (var region in AtlasRegions)
    {
        region.IsSelected = selecting && region == row;
    }

    ImagePainter.HighlightRegion = selecting
        ? new SKRectI(row.Region.X, row.Region.Y,
            row.Region.X + row.Region.Width, row.Region.Y + row.Region.Height)
        : null;
    InvalidateImageCanvas?.Invoke();
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/ImageCanvasPainter.cs
private void PaintHighlight(SKCanvas canvas, SKRect imageRect, SKRectI region, SKBitmap bitmap)
{
    var scaleX = imageRect.Width / bitmap.Width;
    var scaleY = imageRect.Height / bitmap.Height;
    var regionRect = new SKRect(
        imageRect.Left + (region.Left * scaleX),
        imageRect.Top + (region.Top * scaleY),
        imageRect.Left + (region.Right * scaleX),
        imageRect.Top + (region.Bottom * scaleY));

    //Dim everything except the spotlighted region, then outline it
    canvas.Save();
    canvas.ClipRect(regionRect, SKClipOperation.Difference);
    using var dimPaint = new SKPaint { Color = HighlightDim };
    canvas.DrawRect(imageRect, dimPaint);
    canvas.Restore();

    using var strokePaint = new SKPaint
    {
        Color = HighlightStroke,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2f,
        IsAntialias = true,
    };
    canvas.DrawRect(regionRect, strokePaint);
}
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/AtlasRegionCellViewModel.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/ImageCanvasPainter.cs`

**Sharp edges.**
- The dim pass uses a difference clip so the region stays at full brightness
  without a second draw of the image.
- Highlight coordinates are in image pixels; the painter converts them with the
  same scale it used to place the image, so the spotlight tracks zoom for free.
- Selecting the already-selected row clears the spotlight, which is what makes the
  row list behave like a toggle.

### Play a baked animation clip in a preview canvas

**When you want this.** A model with animations should play them, without teaching
the renderer about skinning or node hierarchies.

**The MVVM shape.** The clip is baked off the UI thread by the view model - a
stale bake is discarded on arrival - exposed as a bound property, and played by
the control with a UI-thread timer at the clip's own frame rate. The view model
owns the clip list, the selection and the play state; the control owns the timer.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
//Bakes the chosen clip off the UI thread and hands it to the canvas when still current
private async Task BakeSelectedClipAsync(string animationName)
{
    var animated = _animatedModel;
    if (animated == null || !animated.HasAnimations) { return; }

    try
    {
        var clip = await Task.Run(() => animated.BakeClip(animationName));
        if (_selectedAnimation == animationName && _animatedModel == animated)
        {
            CurrentAnimationClip = clip;
        }
    }
    catch (Exception)
    {
        //A clip that fails to bake leaves the model in its rest pose.
        CurrentAnimationClip = null;
    }
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/ModelSceneGlCanvas.cs
//Playback is a plain UI-thread timer at the clip's bake rate: each tick pushes the next
//baked frame's vertices to the renderer and invalidates. Stops itself when nothing plays.
private void UpdateAnimationTimer()
{
    var clip = AnimationClip;
    if (clip == null || !IsAnimationPlaying)
    {
        _animationTimer?.Stop();
        return;
    }

    _animationTimer ??= CreateAnimationTimer();
    _animationTimer.Interval = TimeSpan.FromSeconds(1d / clip.FrameRate);
    _animationTimer.Start();
}

private void PushAnimationFrame()
{
    var clip = AnimationClip;
    if (clip == null || clip.Frames.Count == 0) { return; }

    _renderer.SetFrameVertices(clip.Frames[_animationFrameIndex % clip.Frames.Count].Primitives);
    Invalidate();
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Models/AnimatedModel.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/GL/ModelSceneGlCanvas.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Baking trades memory for simplicity: playback is just swapping vertex buffers,
  so the renderer never needs skinning support.
- Frames are sampled over a half-open interval so a looping clip does not hold a
  duplicated end pose, and the frame count is clamped at both ends.
- The bake asserts that each frame's vertex layout matches the rest pose and
  throws rather than drawing garbage.
- The result is published only if the selection and the model have not changed
  since the bake started.
- Animated models are built from an evaluated rest pose rather than a static node
  walk, so the primitive layout matches the per-frame evaluations exactly.

### Rasterize SVG art with the CodeBrix SkiaSvg library

**When you want this.** You have vector art to display in a raster surface, at a
display size and again at a thumbnail size, or you ship an icon set and want it
available at any size.

**The MVVM shape.** A static decoder or a resource service in a library with no UI
types, returning a bitmap or encoded bytes. The view model calls it inside
`Task.Run`; callers ask for a pixel surface and never learn whether a raster or a
vector answered.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/SvgImageDecoder.cs
public static SKBitmap Render(byte[] svgBytes, int maxDimension = 1024)
{
    ArgumentNullException.ThrowIfNull(svgBytes);

    using var stream = new MemoryStream(svgBytes, writable: false);
    SKSvg svg;
    try
    {
        svg = SKSvg.CreateFromStream(stream);
    }
    catch (Exception ex)
    {
        throw new InvalidDataException("The data is not a renderable SVG.", ex);
    }

    using var _ = svg;
    var picture = svg.Picture;
    var rect = picture?.CullRect ?? SKRect.Empty;
    if (picture == null || rect.Width <= 0 || rect.Height <= 0)
    {
        throw new InvalidDataException("The data is not a renderable SVG.");
    }

    var scale = Math.Min(maxDimension / rect.Width, maxDimension / rect.Height);
    var width = Math.Max(1, (int)MathF.Round(rect.Width * scale));
    var height = Math.Max(1, (int)MathF.Round(rect.Height * scale));

    var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
    using var canvas = new SKCanvas(bitmap);
    canvas.Clear(SKColors.Transparent);

    //Drawn by hand rather than through SKPictureExtensions.ToBitmap, which does not
    //translate by CullRect's origin — an SVG whose content does not start at (0,0)
    //would come out clipped
    canvas.Scale(scale);
    canvas.Translate(-rect.Left, -rect.Top);
    canvas.DrawPicture(picture);
    return bitmap;
}
```

An icon service resolves by embedded-resource name, preferring an exact-size
raster and falling back to the vector:

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Pinta.Brix.Controls.csproj -->
<!-- The application icon set carried over from upstream (see
     THIRD-PARTY-NOTICES.txt at the repo root for attribution) -->
<ItemGroup>
  <EmbeddedResource Include="Assets\icons\**\*.png" />
  <EmbeddedResource Include="Assets\icons\**\*.svg" />
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/SkiaResourceService.cs
private ImageSurface? LoadIcon (string name, int size)
{
	// Embedded resource names look like:
	//   Pinta.Brix.Controls.Assets.icons.hicolor.16x16.actions.<name>.png
	//   Pinta.Brix.Controls.Assets.icons.hicolor.scalable.actions.<name>.svg
	string pngSuffix = $".{name}.png";
	string svgSuffix = $".{name}.svg";

	// Prefer an exact-size PNG, then the nearest larger size, then SVG.
	string? exact = resource_names.FirstOrDefault (r => r.Contains ($".{size}x{size}.") && r.EndsWith (pngSuffix, StringComparison.Ordinal));
	if (exact is not null)
		return DecodePng (exact, size);

	string? svg = resource_names.FirstOrDefault (r => r.Contains (".scalable.") && r.EndsWith (svgSuffix, StringComparison.Ordinal));
	if (svg is not null)
		return RenderSvg (svg, size);
	// ...
}

private ImageSurface? RenderSvg (string resourceName, int size)
{
	using Stream? stream = assembly.GetManifestResourceStream (resourceName);
	if (stream is null)
		return null;

	using SKSvg svg = new ();
	SKPicture? picture = svg.Load (stream);
	if (picture is null)
		return null;

	ImageSurface surface = new (Format.Argb32, size, size);
	using SKCanvas canvas = new (surface.Bitmap);
	SKRect bounds = picture.CullRect;
	if (bounds.Width > 0 && bounds.Height > 0) {
		float scale = Math.Min (size / bounds.Width, size / bounds.Height);
		canvas.Scale (scale);
		canvas.Translate (-bounds.Left, -bounds.Top);
	}
	canvas.DrawPicture (picture);
	canvas.Flush ();
	surface.MarkDirty ();
	return surface;
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/SvgImageDecoder.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/SkiaResourceService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/IResourceService.cs`

**Also shown by.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.Core/Controls/EmbeddedImage.cs`
(the same library behind an image source, reached from XAML by a custom URI
scheme; see the views area)

**Sharp edges.**
- The picture's own bounds may have a non-zero origin, so translate as well as
  scale. Convenience helpers that skip the translate clip any drawing whose
  content does not start at the origin.
- Computing the scale so the longer side lands on the requested dimension scales
  small icons up as well as large art down, which is the point of a vector source.
- Embedded resource names replace path separators with dots, so lookups are suffix
  and substring matches, not path matches.
- A lookup that never fails needs a companion "do you have this" query, so a
  caller that wants a text-label fallback can ask first.
- Cache by name and size: icons are requested repeatedly by menus, toolbars and
  pads.
- Throw a single, specific exception for unusable data and let the view model show
  it through its error dialog rather than crashing.

### Decode raster images with the CodeBrix Imaging library into a Skia bitmap

**When you want this.** You have image bytes in an unknown supported format and
need either a displayable bitmap or raw RGBA for a GPU upload.

**The MVVM shape.** A static decoder with two entry points in a library with no UI
types, called from `Task.Run` by the view model or from a renderer's texture
upload.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/LdrImageDecoder.cs
/// <summary>Decodes an image from a stream into an RGBA <see cref="SKBitmap"/>.</summary>
/// <exception cref="InvalidDataException">The data is not a decodable image.</exception>
public static unsafe SKBitmap Decode(Stream stream)
{
    ArgumentNullException.ThrowIfNull(stream);

    Image<Rgba32> image;
    try
    {
        image = Image.Load<Rgba32>(stream);
    }
    catch (Exception ex) when (ex is UnknownImageFormatException or InvalidImageContentException)
    {
        throw new InvalidDataException("The data is not a decodable image.", ex);
    }

    using (image)
    {
        var bitmap = new SKBitmap(new SKImageInfo(image.Width, image.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        var destination = new Span<byte>((void*)bitmap.GetPixels(), bitmap.ByteCount);
        image.CopyPixelDataTo(destination);
        return bitmap;
    }
}

/// <summary>
/// Decodes an image from a byte buffer into a raw RGBA byte array (4 bytes per pixel,
/// row-major, top-left origin) — the form GPU texture uploads want.
/// </summary>
public static (byte[] Rgba, int Width, int Height) DecodeToRgbaBytes(byte[] data) { /* ... */ }
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Rendering/Images/LdrImageDecoder.cs`

**Also shown by.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.Rendering/Images/LdrImageDecoder.cs`,
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.Rendering/Images/LdrImageDecoder.cs`

**Sharp edges.**
- Turning on unsafe blocks in the library lets pixels be copied straight into the
  Skia bitmap's buffer without an intermediate array.
- The alpha type must match what the decoder produces; getting it wrong shows up
  as dark fringing on transparent edges.
- Only the two documented decode failures are translated to a domain exception;
  anything else propagates, so a genuine bug is not swallowed as "not an image".

### Normalize a downloaded image before embedding it in a document

**When you want this.** Images arrive in whatever format and resolution the source
holds, and the document embedder accepts only some of them.

**The MVVM shape.** A static pipeline in the library that decodes, optionally
resizes, and re-encodes only when it must; the caller catches the throw and turns
it into a warning plus a placeholder card rather than a failed document.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/ImagePipeline.cs
using CodeBrix.Imaging;
using CodeBrix.Imaging.Formats;
using CodeBrix.Imaging.Formats.Gif;
using CodeBrix.Imaging.Formats.Jpeg;
using CodeBrix.Imaging.Formats.Png;
using CodeBrix.Imaging.Formats.Webp;
using CodeBrix.Imaging.Processing;

/// <summary>
/// Normalises a downloaded image for PDF embedding: capped pixel width, JPEG for
/// photographs, PNG for graphics with transparency (which also converts formats
/// the PDF embedder cannot take, such as WebP and GIF).
/// </summary>
internal static class ImagePipeline
{
    private const int MaxPixelWidth = 1800;
    private const int JpegQuality = 87;

    /// <summary>
    /// Decodes and normalises image bytes. Throws on undecodable data — callers
    /// turn that into a warning plus a media card, never a failed document.
    /// </summary>
    public static ProcessedImage ProcessForPrint(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        using var image = Image.Load(bytes, out IImageFormat format);

        var keepsTransparency = format is PngFormat or WebpFormat or GifFormat;
        var needsResize = image.Width > MaxPixelWidth;

        //Untouched JPEG/PNG bytes embed best — only re-encode when we must
        if (!needsResize && (format is JpegFormat || format is PngFormat))
        {
            return new ProcessedImage { Bytes = bytes, Width = image.Width, Height = image.Height };
        }

        if (needsResize)
        {
            image.Mutate(x => x.Resize(MaxPixelWidth, 0));
        }

        using var output = new MemoryStream();
        if (keepsTransparency)
        {
            image.Save(output, new PngEncoder());
        }
        else
        {
            image.Save(output, new JpegEncoder { Quality = JpegQuality });
        }

        return new ProcessedImage { Bytes = output.ToArray(), Width = image.Width, Height = image.Height };
    }
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
var bytes = processed.Bytes;
var image = figure.AddImage(ImageSource.FromBinary(
    $"img-{_figureNumber}-{Guid.NewGuid():N}", () => bytes, quality: 90));
image.LockAspectRatio = true;
image.Width = Unit.FromPoint(width);
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/ImagePipeline.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`

**Also shown by.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ImagePipeline.cs`

**Sharp edges.**
- Already-suitable bytes are passed through untouched; re-encoding is a last
  resort, not a default.
- Transparency decides the output format, and that same decision is what converts
  formats the embedder cannot take.
- The image source takes a name plus a byte-producing lambda; capture the bytes in
  a local first and give each image a unique name.
- The document layer's imaging back-end has to be set before any image is placed;
  see the font-registration blueprint in the documents area.

### Create a drawing session with named color layers

**When you want this.** Freehand annotation in several translucent colors, where
overlapping passes of one color must not compound and the user picks which kind of
mark they are making.

**The MVVM shape.** The view model - or a small library class it owns - creates a
`DrawingSession`, adds one layer per color by name, and exposes only what the
application needs: the active color, stroke counts, clear and export. Selecting a
color is a command that looks the layer up by name. The session is exposed as a
read-only property so the page can render it and feed it pointer events; nothing
else about it leaks into the UI.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
public const string PainLayerName = "Pain";
public const string NumbnessLayerName = "Numbness";
public const string TinglingLayerName = "Tingling";

public MainViewModel()
{
    if (!IsDesignMode(true))
    {
        _session = new DrawingSession(new DrawingSessionOptions
        {
            BackgroundFillColor = Color.White,
            SurfaceClearColor = Color.White,
        });

        //The same highlighter colors the original NuraPad application used
        _session.AddLayer(PainLayerName, Color.FromRgb(255, 30, 230));
        _session.AddLayer(NumbnessLayerName, Color.FromRgb(30, 128, 204));
        _session.AddLayer(TinglingLayerName, Color.FromRgb(204, 170, 10));

        LoadBodyMapBackground();
        // ...
    }
}

/// <summary>
/// The interactive drawing session; the hosting page renders it in its paint handler and
/// forwards pointer events to it.
/// </summary>
public DrawingSession Session => _session;

private void SetActiveLayer(string layerName)
{
    DrawingLayer layer = _session?.GetLayer(layerName);
    if (layer != null)
    {
        _session.ActiveLayer = layer;
        ActiveLayerName = layerName;
    }
}
```

The page's paint handler is one line:

```csharp
// From CodeBrix.Samples/PainDiagram/CodeBrixPlatform/PainDiagram.UI/Views/MainPage.xaml.cs
DrawCanvas.PaintSurface += (_, e) => ViewModel?.Session?.Render(e.Surface, e.Info);

// ...

DrawCanvas.SizeChanged += (_, _) => DrawCanvas.Invalidate();
```

A session built over a captured photograph looks the same, with the background
supplied as raw pixels:

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs
public static PaintingSession Create(byte[] bgraPixels, int width, int height, bool mirrorHorizontally)
{
    if (bgraPixels == null) { throw new ArgumentNullException(nameof(bgraPixels)); }
    if (width < 1) { throw new ArgumentOutOfRangeException(nameof(width)); }
    if (height < 1) { throw new ArgumentOutOfRangeException(nameof(height)); }

    //The raw-BGRA factory copies the pixels, optionally mirrors, and owns the bitmap -
    //  no SkiaSharp decode/mirror round-trip lives here anymore.
    DrawingSession session = DrawingSession.CreateForImage(
        bgraPixels, width, height,
        CalibrationSizing.DeriveFromBackgroundImage,
        new DrawingSessionOptions
        {
            BackgroundFillColor = Color.White,   //JPEG has no alpha - keep the fill opaque
            SurfaceClearColor = Color.Black,     //letterbox bars around the still
            StrokeWidth = BrushRadius * 2f,
        },
        mirrorHorizontally);
    return new PaintingSession(session);
}

private PaintingSession(DrawingSession session)
{
    _session = session;

    foreach (HighlighterColor color in HighlighterPalette.Colors)
    {
        _session.AddLayer(color.Name, color.Color);
    }
    ActiveColorName = HighlighterPalette.Colors[0].Name;
}

public bool SelectColor(string colorName)
{
    DrawingLayer layer = _session.GetLayer(colorName);
    if (layer == null) { return false; }

    _session.ActiveLayer = layer;
    ActiveColorName = layer.Name;
    return true;
}
```

**Where to look.**
`PainDiagram/Shared/ViewModels/MainViewModel.cs`
`WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs`
`WebcamPainter/src/libs/WebcamPainter.Painting/HighlighterPalette.cs`

**Sharp edges.**
- One layer per color is what makes the highlighter effect work: repeated passes
  of one color over the same area do not darken where they cross.
- Keep the layer names as constants, or as the palette's own names, and use them
  both as the session key and as the value of the bound "active" property, so the
  two can never drift apart. Only change the bound name when the lookup actually
  returned a layer.
- The layer colors are passed as opaque values; the ink is drawn translucent by
  the library. Tinting a button with the same hue at partial alpha is what matches
  the on-canvas ink.
- The background fill is opaque where the source format has no alpha, and the
  surface clear color is what shows in the letterbox bars around the image.
- The session can be null in the designer, because the view model skips
  construction in design mode, so every handler uses null-conditional access.

### Export a drawing at a chosen pixel size

**When you want this.** You want the finished artwork at a fixed resolution,
independent of how large the on-screen canvas happens to be.

**The MVVM shape.** One call on the session inside the save command, returning
bytes that are written asynchronously. Nothing about the window, the canvas size
or the display scale is involved, so the same call would work with no UI at all.

**Code.**

```csharp
// From CodeBrix.Samples/PainDiagram/Shared/ViewModels/MainViewModel.cs
private const int ExportPixelSize = 1000;

// ...

byte[] png = _session.ExportPng(new Size(ExportPixelSize, ExportPixelSize));
await File.WriteAllBytesAsync(outputPath, png);

StatusText = $"Saved: {outputPath}";
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
var jpeg = _paintSession.ExportJpeg();
await File.WriteAllBytesAsync(outputPath, jpeg);
```

**Where to look.**
`PainDiagram/Shared/ViewModels/MainViewModel.cs`
`WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs`

**Sharp edges.**
- The export includes the background image and every layer; there is no separate
  compositing step in either application.
- Set the busy flag around the export and clear it in a `finally`, with the
  affected commands marked so the buttons disable themselves while it runs.
- WebcamPainter exports at the photograph's native resolution rather than a fixed
  size, which is the other reasonable choice.

### Drive strokes in normalized image coordinates from a sensor

**When you want this.** Your stroke input comes from something other than a
pointer - a tracker, a controller, a network feed - and you do not want view-size,
display-scale or letterbox math anywhere near it.

**The MVVM shape.** The library exposes begin, continue and end in zero-to-one
image coordinates; the view model converts the sensor's normalized position into
stroke calls directly. Nothing in the input path knows the canvas size.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs
public bool BeginStroke(float normX, float normY)
    => _session.PointerPressedNormalized(normX, normY);

public bool ContinueStroke(float normX, float normY)
    => _session.PointerMovedNormalized(normX, normY);

public bool EndStroke() => _session.PointerReleased();

public void CancelStroke() => _session.PointerCanceled();
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
var paintNow = result.HandDetected && result.IsOpenPalm;
IsBrushPainting = paintNow;

//Strokes are driven in normalized still-image coordinates, so no canvas size is
//  needed - the drawing space is calibrated from the captured photo.
if (paintNow && CrosshairNormX != null && CrosshairNormY != null)
{
    if (session.IsStrokeActive)
    {
        session.ContinueStroke(CrosshairNormX.Value, CrosshairNormY.Value);
    }
    else
    {
        session.BeginStroke(CrosshairNormX.Value, CrosshairNormY.Value);
    }
}
else if (session.IsStrokeActive)
{
    session.EndStroke();
}
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs`
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Normalized input works before the first render, because the drawing space is
  calibrated from the background image rather than from a view size. There is a
  test named for exactly that.
- The stroke state machine is driven purely by whether a stroke is active: no
  hand, or a closed hand, ends the stroke; the next open palm begins a new one.
- A position outside the normalized range is ignored by the drawing session rather
  than clamped.

### Keep a mirrored preview and a mirrored drawing consistent

**When you want this.** A selfie-style preview is mirrored, so everything
downstream of it has to agree about which way is left.

**The MVVM shape.** The renderer mirrors at draw time, the still is mirrored at
capture time, and the view model mirrors the tracker's horizontal coordinate. Each
of the three is a single line, each documented where it happens.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
//The preview the user was watching is mirrored, so mirror the still to match
var session = await Task.Run(() =>
    PaintingSession.Create(photo.PixelsBgra32, photo.Width, photo.Height, mirrorHorizontally: true));
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs
if (result.HandDetected)
{
    //The preview and the captured still are mirrored, so mirror the hand too
    CrosshairNormX = 1f - result.PalmCenterX;
    CrosshairNormY = result.PalmCenterY;
}
else
{
    CrosshairNormX = null;
    CrosshairNormY = null;
}
```

**Where to look.**
`WebcamPainter/src/WebcamPainter.Core/ViewModels/MainViewModel.cs`
`WebcamPainter/src/libs/WebcamPainter.Webcam/CameraCanvas.cs`

**Also shown by.**
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The result type documents its coordinates as normalized across the unmirrored
  camera frame, so the mirror is the consumer's job, and only the horizontal axis
  flips.
- The mirroring test uses an asymmetric fixture so the flip is actually observable
  in the exported image.

### Draw a brush sized cursor over a rendered drawing session

**When you want this.** The user is painting with something that is not a mouse
and needs to see where the brush is and how big it is.

**The MVVM shape.** A static render helper in the drawing library takes the
session plus three primitive values from the view model: normalized position and
whether ink is flowing. The page's paint handler passes them through; the helper
never touches the view model.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Painting/PaintCanvas.cs
public static void Render(SKSurface surface, SKImageInfo info, PaintingSession session,
    float? crosshairNormX, float? crosshairNormY, bool isPainting)
{
    if (surface == null || session == null) { return; }

    session.Session.Render(surface, info);

    if (!crosshairNormX.HasValue || !crosshairNormY.HasValue) { return; }

    CodeBrix.Imaging.PointF center = session.NormalizedToView(
        crosshairNormX.Value, crosshairNormY.Value, info.Width, info.Height);
    float radius = session.GetBrushRadiusInView(info.Width, info.Height);
    if (radius <= 0) { return; }

    SKCanvas canvas = surface.Canvas;
    SKColor ringColor = isPainting ? new SKColor(80, 255, 80) : SKColors.White;

    //A dark halo behind the ring keeps the cursor visible over any photo content
    using (var halo = new SKPaint
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 4f,
        Color = new SKColor(0, 0, 0, 140), IsAntialias = true,
    })
    {
        canvas.DrawCircle(center.X, center.Y, radius, halo);
    }

    using (var ring = new SKPaint
    {
        Style = SKPaintStyle.Stroke, StrokeWidth = 2f,
        Color = ringColor, IsAntialias = true,
    })
    {
        canvas.DrawCircle(center.X, center.Y, radius, ring);

        float arm = Math.Max(8f, radius * 0.35f);
        canvas.DrawLine(center.X - arm, center.Y, center.X + arm, center.Y, ring);
        canvas.DrawLine(center.X, center.Y - arm, center.X, center.Y + arm, ring);
    }
}
```

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs
public PointF NormalizedToView(float normX, float normY, float viewWidth, float viewHeight)
{
    RectangleF fit = GetImageRectInView(viewWidth, viewHeight);
    return new PointF(fit.X + (normX * fit.Width), fit.Y + (normY * fit.Height));
}

public RectangleF GetImageRectInView(float viewWidth, float viewHeight)
    => _session.GetDrawingRect(new SizeF(viewWidth, viewHeight));

public float GetBrushRadiusInView(float viewWidth, float viewHeight)
    => _session.ScaleToView(BrushRadius, new SizeF(viewWidth, viewHeight));
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Painting/PaintCanvas.cs`
`WebcamPainter/src/libs/WebcamPainter.Painting/PaintingSession.cs`

**Sharp edges.**
- The dark halo drawn under the ring is what keeps the cursor readable over both
  dark and bright content.
- The cursor radius comes from the drawing session's own view scaling, so the ring
  always matches what a stroke will actually cover.
- Convert through the session's own drawing rectangle - the same letterbox
  rectangle the renderer uses - rather than recomputing aspect-fit math.
- Give the crosshair arms a minimum length so they stay visible when the brush is
  small on screen.

### Draw an animated SkSL shader as a game engine direct drawing

**When you want this.** A full-surface procedural visual - a plasma, a gradient
field, a starfield - that runs on the GPU when one is available and on the CPU
when it is not, with per-frame parameters coming from application state.

**The MVVM shape.** A `DirectDrawingBase` subclass owns the shader source,
compiles it once in its constructor, and reads its per-frame inputs from a
thread-safe state object handed in at construction. It never talks to the view
model.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealBackdrop.cs
public sealed class EtherealBackdrop : DirectDrawingBase
{
    private const string EtherealSksl = @"
uniform float iTime;
uniform float2 iResolution;
uniform float3 iPalm0;
// ... iPalm1..iPalm3

half4 main(float2 fragCoord) {
    float2 uv = fragCoord / iResolution;
    float2 p = uv * 2.0 - 1.0;
    p.x *= iResolution.x / iResolution.y;
    // ... three interfering waves, a radial swirl, and the palm warp/ripple/glow terms
    return half4(half3(col), 1.0);
}";

    public EtherealBackdrop(RenderSurfaceHostBase renderSurfaceHost, View view, Rectangle screenBounds,
        PalmAttractorField attractorField)
        : base(renderSurfaceHost, DirectDrawingMode.View, null, view, screenBounds, null, "ethereal-backdrop")
    {
        _attractorField = attractorField ?? throw new ArgumentNullException(nameof(attractorField));
        _effect = CreateEffect();
        _uniforms = new SKRuntimeEffectUniforms(_effect);
        // ... allocate the reused per-frame buffers and the fixed star seeds
    }

    /// <summary>
    /// Compiles the backdrop's SkSL shader (also exercised directly by the unit tests).
    /// </summary>
    internal static SKRuntimeEffect CreateEffect()
        => SKRuntimeEffect.CreateShader(EtherealSksl, out var errors)
            ?? throw new InvalidOperationException($"Ethereal shader failed to compile: {errors}");

    public override void Update(long tick)
    {
        base.Update(tick);
        ForceRefresh();
    }

    protected override void OnDraw(BackbufferBase backbuffer, RectangleF destRectScreen)
    {
        var canvas = backbuffer.Canvas;
        // ...
        float time = (float)Engine.Instance.TotalSecondsEngineRunning;

        //Advance the attractor smoothing by the frame's real delta (0 on the first frame;
        //  clamped so a stall cannot make the field lurch)
        float delta = _hasLastStepTime ? Math.Clamp(time - _lastStepTime, 0f, 0.1f) : 0f;
        _lastStepTime = time;
        _hasLastStepTime = true;
        _attractorField.Step(delta);
        _attractorField.CopyState(_palmState);

        _uniforms["iTime"] = time;
        _resolutionUniform[0] = w;
        _resolutionUniform[1] = h;
        _uniforms["iResolution"] = _resolutionUniform;
        for (int k = 0; k < PalmAttractorField.MaxAttractors; k++)
        {
            _palmUniforms[k][0] = ((_palmState[k * 3] * 2f) - 1f) * aspect;
            _palmUniforms[k][1] = (_palmState[(k * 3) + 1] * 2f) - 1f;
            _palmUniforms[k][2] = _palmState[(k * 3) + 2];
            _uniforms[PalmUniformNames[k]] = _palmUniforms[k];
        }
        using (var shader = _effect.ToShader(_uniforms))
        {
            _plasmaPaint.Shader = shader;
            canvas.DrawRect(rect, _plasmaPaint);
            _plasmaPaint.Shader = null;
        }
        // ... then the starfield, drawn with ordinary canvas calls over the shaded rect
    }
}
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/EtherealBackdrop.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/EtherealBackdropTests.cs`

**Sharp edges.**
- The update override forces a refresh every frame. The CPU render path uses dirty
  rectangles and would otherwise stop animating; the GPU path re-renders the whole
  surface anyway.
- Because it is an ordinary direct drawing, on the GPU path the draw runs on the
  GL thread with the graphics context current, so the shader executes on the GPU;
  on the CPU path Skia's raster backend evaluates the same shader.
- The draw allocates nothing: the uniform arrays, the state buffer, both paints
  and the seed arrays are fields created once. Only the shader produced per frame
  is transient, and it is disposed by its `using`.
- Seed any randomness at construction, so an undisturbed frame is a pure function
  of engine time and is reproducible.
- Design the shader so a zeroed parameter set reduces it exactly to the
  undisturbed visual. Every parameterized term is multiplied by its own strength,
  so at zero the scene is the plain background - that is what makes the visual
  melt back instead of snapping.
- Take time from the engine's own running total, not from wall-clock time, so a
  pause does not make the animation jump.
- Setting an unknown uniform name throws, which the test suite turns into a
  guarantee: one test sets every uniform the drawing sets, so a renamed uniform
  fails the test run rather than rendering black.

### Smooth worker rate data into frame rate animation

**When you want this.** Values arrive from a background producer at an irregular
rate and must drive a smooth animation that runs at the render rate, without
snapping when values appear, change or disappear.

**The MVVM shape.** A small thread-safe field object sits between the two. The
producer sets targets from its own thread; the renderer steps the field once per
frame and copies the state into a reusable buffer. Slots are keyed by a stable id
so a moving item keeps its animation state.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmAttractorField.cs
public const int MaxAttractors = 4;
public const float PositionLerpPerSecond = 12f;
public const float StrengthAttackPerSecond = 4f;
public const float StrengthReleasePerSecond = 2.5f;

public void SetTargets(IReadOnlyList<PalmAttractor> palms)
{
    lock (_lock)
    {
        Span<bool> seen = stackalloc bool[MaxAttractors];

        if (palms != null)
        {
            foreach (PalmAttractor palm in palms)
            {
                int slotIndex = FindSlot(palm.Id);
                if (slotIndex < 0) { continue; }

                Slot slot = _slots[slotIndex];
                if (slot.Id != palm.Id)
                {
                    //A freshly claimed slot starts AT the palm with no strength, so the
                    //  influence swells in place instead of sweeping over from stale state
                    slot.Id = palm.Id;
                    slot.X = palm.X;
                    slot.Y = palm.Y;
                    slot.Strength = 0f;
                }
                slot.TargetX = palm.X;
                slot.TargetY = palm.Y;
                slot.TargetStrength = 1f;
                seen[slotIndex] = true;
            }
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            if (!seen[i] && _slots[i].Id != 0)
            {
                _slots[i].TargetStrength = 0f;
            }
        }
    }
}

public void Step(float deltaSeconds)
{
    if (deltaSeconds <= 0f) { return; }

    //Exponential approach: framerate-independent, and never overshoots
    float positionBlend = 1f - (float)Math.Exp(-PositionLerpPerSecond * deltaSeconds);
    float attackBlend = 1f - (float)Math.Exp(-StrengthAttackPerSecond * deltaSeconds);
    float releaseBlend = 1f - (float)Math.Exp(-StrengthReleasePerSecond * deltaSeconds);

    lock (_lock)
    {
        foreach (Slot slot in _slots)
        {
            if (slot.Id == 0) { continue; }

            slot.X += (slot.TargetX - slot.X) * positionBlend;
            slot.Y += (slot.TargetY - slot.Y) * positionBlend;

            float strengthBlend = slot.TargetStrength > slot.Strength ? attackBlend : releaseBlend;
            slot.Strength += (slot.TargetStrength - slot.Strength) * strengthBlend;

            if (slot.TargetStrength <= 0f && slot.Strength < FreeSlotStrength)
            {
                slot.Id = 0;
                slot.Strength = 0f;
            }
        }
    }
}

//Returns the index of the slot already owned by this id, else a free slot, else -1.
//  A slot mid-fade keeps its id, so a returning palm re-attaches instead of popping.
private int FindSlot(int id)
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmAttractorField.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/PalmAttractorFieldTests.cs`

**Sharp edges.**
- An exponential approach rather than a linear step is framerate-independent and
  never overshoots, so a long frame cannot make the animation lurch past its
  target.
- Attack and release use different rates, so appearing feels different from
  disappearing.
- A newly claimed slot is placed at the incoming position with zero strength, so
  the influence swells in place rather than sweeping in from wherever the slot was
  last used.
- A slot that is fading out keeps its id until its strength falls below a small
  epsilon, so an item that reappears re-attaches to its own fade instead of
  restarting. There is a test named for exactly that.
- Capacity is fixed and extras are dropped rather than queued.
- The copy-out method writes into a caller-owned buffer and validates its length,
  which is what lets the renderer stay allocation-free.
- The class documents that all members are thread-safe, and that claim is what
  lets the view model feed it straight from a worker thread without marshalling.

### Keep a pipeline and a renderer decoupled by a normalized seam

**When you want this.** Two libraries have to cooperate every frame, and you do
not want either one to depend on the other's concepts.

**The MVVM shape.** Define the narrowest possible value type that describes what
the renderer needs, put it in the rendering library, and let the view model
translate.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmAttractor.cs
/// <summary>
/// One open palm the visualization should be drawn toward, in normalized screen
/// coordinates. This is the whole seam between the vision pipeline and the rendering:
/// the consumer maps whatever it tracks into these points (mirroring the X coordinate
/// when the user watches a mirror-style view) and hands them to
/// <see cref="VisualizerSession.UpdatePalms"/>.
/// </summary>
public readonly struct PalmAttractor
{
    public PalmAttractor(int id, float x, float y)
    {
        Id = id;
        X = x;
        Y = y;
    }

    /// <summary>
    /// A stable identifier for the palm. The same physical hand should keep the same id
    /// from update to update so its glow follows it instead of re-fading in.
    /// </summary>
    public int Id { get; }

    /// <summary>The palm's horizontal position, 0 (left) .. 1 (right) across the visual.</summary>
    public float X { get; }

    /// <summary>The palm's vertical position, 0 (top) .. 1 (bottom) down the visual.</summary>
    public float Y { get; }
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs
public void UpdatePalms(IReadOnlyList<PalmAttractor> palms) => _attractorField.SetTargets(palms);
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/PalmAttractor.cs`
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs`

**Sharp edges.**
- Normalized coordinates, not pixels, so a window resize needs no re-mapping
  anywhere.
- The stable id travels across the seam. That single field is what lets the
  renderer's easing follow a moving item instead of restarting.
- The seam type carries no confidence scores, no state flag and no landmarks: the
  view model filters before translating, so "what counts" stays an application
  policy rather than a rendering one.
- The documentation names the caller's responsibility - mirroring - explicitly,
  which is how a coordinate-convention pitfall stays documented at the boundary
  where it matters.

### Offer a CPU fallback for a GPU rendering path behind one switch

**When you want this.** Your visual runs on the GPU by default but must be
runnable on the CPU path for a head, a machine or a test that cannot use one, and
you want the two to be the same scene rather than two code paths.

**The MVVM shape.** One property on the canvas, set once before the surface is
touched, chosen from an environment variable so no rebuild is needed. The drawing
code is identical either way.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs
/// Set the environment variable <c>PALMVISUALIZER_USE_CPU=1</c> to run the identical scene
/// on the CpuRendering (CPU) render path.

// ...

//GpuRendering-OpenGL (GPU) by default; must be chosen before the first access to Host. The
//  render resolution tracks the window (no SetRenderResolution) - the shader
//  scene is resolution-independent.
_canvas.UseGpuRendering = Environment.GetEnvironmentVariable("PALMVISUALIZER_USE_CPU") != "1";
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/EtherealBackdropTests.cs`

**Sharp edges.**
- The choice must be made before the canvas's host is read for the first time.
- The default is the negative test, so an unset or garbage value keeps the GPU
  path.
- The two paths differ in one respect the drawing has to handle: the CPU path uses
  dirty rectangles, which is why the update override forces a refresh
  unconditionally.
- Because the fallback is Skia's raster backend, the same shader can be tested
  headlessly on a raster surface with no engine and no window, which is exactly
  what the shader tests do.

### Choose the render resolution from the zoom level

**When you want this.** Zooming in should sharpen content, not just scale up a
blurry bitmap, but you do not want to render a poster-sized image at the top of
the ladder.

**The MVVM shape.** A small value type in the library owns the zoom ladder and the
resolution rule. The view model asks it for a resolution and passes that to the
renderer; nothing about resolution lives in the page.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/ViewZoom.cs
    /// <summary>The fit-the-page level; also the minimum.</summary>
    public const int MinimumPercent = 100;

    /// <summary>The highest resolution a page is ever rendered at, whatever the zoom.</summary>
    public const int MaximumRenderDpi = 600;

    /// <summary>The zoom levels, in order, that ZoomIn and ZoomOut step through.</summary>
    public static IReadOnlyList<int> Levels { get; } = [100, 125, 150, 200, 300, 400, 500, 700, 1000];

    /// <summary>The current level as a multiplier of the fit-the-page size (1.0 at 100%).</summary>
    public double Factor => Percent / 100.0;

    /// <summary>
    /// The resolution to render a page at for the current level: baseDpi scaled by the
    /// zoom factor so text stays sharp, capped at MaximumRenderDpi (past the cap
    /// the image is scaled up a little on screen instead of rendering an enormous bitmap).
    /// </summary>
    public int GetRenderDpi(int baseDpi)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(baseDpi, 1);
        return (int)Math.Min(Math.Round(baseDpi * Factor), MaximumRenderDpi);
    }
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs
            var dpi = View.Zoom.GetRenderDpi(_renderer.Dpi);
            var page = await _renderer.RenderCurrentPageAsync(document, dpi, cts.Token);
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Viewing/ViewZoom.cs`
`PdfSideBySide/src/PdfSideBySide.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Here 100 percent means "the whole page fits the pane", not "actual size", and it
  is the minimum. The user can never zoom out past fit, so there is no empty
  border state to design for.
- Pass the resolution per call, so the renderer's own default keeps acting as the
  multiplier's base. Using the property setter instead would clear the cache on
  every zoom step.
- The resolution cap means the on-screen image at the top of the ladder is scaled
  up. That is a deliberate trade, and the comment says so.

### Draw a zoomable document canvas on an SKXamlCanvas subclass

**When you want this.** Any document editor - a drawing surface, a map, a score, a
diagram - that has to composite content and repaint fast.

**The MVVM shape.** The canvas control is a view. It implements an interface the
document model owns, so the model can ask for a repaint or set a cursor without
referencing any UI type, and it subscribes to the model's invalidation events.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs
public sealed class PintaCanvas : SKXamlCanvas, ICanvasView
{
    private Document? document;
    private CanvasRenderer? renderer;
    private Drawing.ImageSurface? canvas_surface;
    private RectangleI? pending_dirty; // union of invalidated canvas rects; null = everything
    private bool surface_stale = true;

    public PintaCanvas ()
    {
        PaintSurface += OnPaintSurface;
        IsTabStop = true;

        PointerPressed += OnCanvasPointerPressed;
        PointerMoved += OnCanvasPointerMoved;
        PointerReleased += OnCanvasPointerReleased;
        PointerWheelChanged += OnCanvasPointerWheelChanged;
        KeyDown += OnCanvasKeyDown;
        KeyUp += OnCanvasKeyUp;
        // ...
    }

    private void OnPaintSurface (object? sender, SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear (SKColors.Transparent);

        if (document is null || renderer is null)
            return;

        Size imageSize = document.ImageSize;
        Size viewSize = document.Workspace.ViewSize;

        // 1. Refresh the unscaled composite of all layers.
        canvas_surface ??= new Drawing.ImageSurface (Drawing.Format.Argb32, imageSize.Width, imageSize.Height);
        // ...
        // 3. The composite, scaled for zoom: nearest-neighbor when zoomed in,
        //    linear when zoomed out (matching upstream).
        double scale = document.Workspace.Scale;
        SKSamplingOptions sampling = scale >= 1
            ? new SKSamplingOptions (SKFilterMode.Nearest)
            : new SKSamplingOptions (SKFilterMode.Linear);
        canvas.DrawBitmap (
            canvas_surface.Bitmap,
            new SKRect (0, 0, viewSize.Width, viewSize.Height),
            sampling);
        // ...
    }
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ICanvasView.cs`

**Sharp edges.**
- Zoom is applied by resizing the element and scaling at draw time, not by a
  transform on the scroll viewer; the scroll viewer only ever pans.
- Nearest-neighbor above one-to-one and linear below is what keeps a zoomed-in
  pixel editor from looking blurred.
- `IsTabStop = true` is needed or the canvas never receives key events.

### Repaint only the dirty rectangle of a cached composite

**When you want this.** Your document is expensive to composite and most edits
touch a small region.

**The MVVM shape.** The document model raises an invalidation event carrying
either a rectangle or an "entire surface" flag; the view accumulates the union
until the next paint and re-composites only that.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs
private void OnCanvasInvalidated (object? sender, CanvasInvalidatedEventArgs e)
{
    if (e.EntireSurface || pending_dirty is null && surface_stale) {
        surface_stale = true;
        pending_dirty = null;
    } else if (pending_dirty is { } dirty) {
        pending_dirty = dirty.Union (e.Rectangle);
    } else {
        pending_dirty = e.Rectangle;
    }

    Invalidate ();
}

/// <summary>
/// A selection change alters only the overlay pass, not the layer
/// composite, so the cached surface is left intact and just the overlay
/// is repainted. Without this the marching ants and the selection tools'
/// handles never appear, because changing a selection dirties no pixels
/// and therefore never raises CanvasInvalidated.
/// </summary>
private void OnSelectionChanged (object? sender, EventArgs e)
    => Invalidate ();
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/EventArgs/CanvasInvalidatedEventArgs.cs`

**Sharp edges.**
- Overlay-only changes dirty no pixels, so they need their own invalidate path or
  they never appear. The comment in the sample records that exact bug.
- Null is used to mean "everything", so the union logic has to check for it before
  unioning.

### Animate an overlay with a timer that stops when unloaded

**When you want this.** A continuously animated overlay - a marching-ants
selection, a caret, a spinner - that must not keep running after its view is gone.

**The MVVM shape.** The timer belongs to the control that draws the animation, and
is started and stopped from the loaded and unloaded events so a closed document's
canvas stops ticking. Animation state that no one else reads stays private to the
view.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs
//Marching ants: the dash offset ticks backwards while a selection is
//visible (upstream ticks -1 per timer fire), wrapping at the dash
//pattern's period. The redraw is display-only.
private readonly Microsoft.UI.Xaml.DispatcherTimer ants_timer = new () {
    Interval = TimeSpan.FromMilliseconds (100),
};
private float ants_offset;

// ... in the constructor:
ants_timer.Tick += (_, _) => {
    if (document is null || !document.Selection.Visible || document.Selection.SelectionPolygons.Count == 0)
        return;
    ants_offset -= 1;
    if (ants_offset < 0)
        ants_offset += 8; // dash pattern period: 4 on + 4 off
    Invalidate ();
};

//The timer only runs while the canvas is in the tree, so closed
//documents do not keep ticking.
Loaded += (_, _) => ants_timer.Start ();
Unloaded += (_, _) => ants_timer.Stop ();
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvas.cs`

**Sharp edges.**
- Without the unloaded stop, every document ever opened keeps a timer running for
  the life of the process.
- The tick returns early when there is nothing to animate, so the timer costs
  nothing while the overlay is not visible.

### Host a canvas in a scroll viewer and drive zoom and scroll from an interface

**When you want this.** Your document is larger than its viewport and the model,
not the view, decides where the viewport should be after a zoom.

**The MVVM shape.** The scrollable host implements an interface the model owns:
viewport size, scroll offset, a layout pump and focus. The model computes the new
offset; the view just applies it, and never sees a scroll viewer.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ICanvasView.cs
/// <summary>
/// The scrollable view hosting a document's canvas: viewport metrics and the
/// scroll position, in view (zoomed) coordinates.
/// </summary>
public interface ICanvasScrollView
{
	Size ViewportSize { get; }

	PointD ScrollOffset { get; set; }

	/// <summary>
	/// Ensures scroll extents reflect the current view size before scroll
	/// offsets are adjusted (replaces the upstream main-loop pump).
	/// </summary>
	void UpdateLayout ();

	/// <summary>Move keyboard focus to the canvas view. Returns whether focus was gained.</summary>
	bool GrabFocus ();
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvasView.cs
public Size ViewportSize =>
    new (
        (int) Math.Max (scroller.ViewportWidth > 0 ? scroller.ViewportWidth : scroller.ActualWidth, 0),
        (int) Math.Max (scroller.ViewportHeight > 0 ? scroller.ViewportHeight : scroller.ActualHeight, 0));

public PointD ScrollOffset {
    get => new (scroller.HorizontalOffset, scroller.VerticalOffset);
    set => scroller.ChangeView (value.X, value.Y, null, disableAnimation: true);
}

public new void UpdateLayout ()
{
    Canvas.UpdateLayout ();
    scroller.UpdateLayout ();
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/DocumentWorkspace.cs
private void ZoomToScaleAroundViewPoint (double newScale, PointD center_point)
{
    PointD offset = CanvasWindow?.ScrollOffset ?? new PointD (0, 0);

    double scroll_offset_x = center_point.X - offset.X;
    double scroll_offset_y = center_point.Y - offset.Y;

    PointD canvas_point = ViewPointToCanvas (center_point);

    Scale = Math.Min (newScale, 36.0);

    // Make sure the scroll extents match the new view size before
    // recentering. (Upstream pumped the GTK main loop here.)
    CanvasWindow?.UpdateLayout ();

    // Scroll so that the canvas position under 'center_point' is still the same after zooming.
    PointD new_center_point = CanvasPointToView (canvas_point);
    if (CanvasWindow is { } view) {
        view.ScrollOffset = new PointD (
            new_center_point.X - scroll_offset_x,
            new_center_point.Y - scroll_offset_y);
    }
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/PintaCanvasView.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Services/ICanvasView.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/DocumentWorkspace.cs`

**Sharp edges.**
- The layout pump on the interface exists purely as an ordering hook: the scroll
  extents must reflect the new element size before a new offset is set, or the
  offset gets clamped against stale extents.
- Viewport width can be zero before the first layout pass, hence the fallback to
  the actual width.
- The host's layout method is declared `new` because the base class already has
  one; the interface implementation shadows it deliberately.
- The scroll viewer's own zoom is disabled: all zoom is the model's.

### Scale a Skia drawn control from surface pixels to logical units

**When you want this.** Any Skia canvas whose drawing code is written in the
element's own coordinates and must look right on a scaled display.

**The MVVM shape.** One line in the paint handler. It is a view concern entirely.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/Widgets/HistogramWidget.cs
private void OnPaintSurface (object? sender, SKPaintSurfaceEventArgs e)
{
    SKCanvas canvas = e.Surface.Canvas;
    canvas.Clear (SKColors.Transparent);

    if (ActualWidth <= 0 || ActualHeight <= 0)
        return;

    //The surface is physical pixels; draw in the element's logical space.
    canvas.Scale (e.Info.Width / (float) ActualWidth, e.Info.Height / (float) ActualHeight);

    SKRect rect = SKRect.Create (0, 0, (float) ActualWidth, (float) ActualHeight);
    // ... draw in logical units from here on
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Widgets/HistogramWidget.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Widgets/ColorGradientWidget.cs`

**Sharp edges.**
- Guard the actual size against zero before dividing; a paint can arrive before
  the first layout pass.
- The gradient widget takes the opposite approach and computes everything from the
  actual size directly, so the two show both options side by side.

### Turn raw pixel surfaces into XAML image sources

**When you want this.** You have raw premultiplied pixels - from a decoder, a
renderer, a thumbnail - and need them in an `Image` element.

**The MVVM shape.** A static factory returning an image source, caching by key.
Returning null for an unknown key lets callers fall back to a text label rather
than showing an empty square.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Controls/IconImageSource.cs
public static ImageSource? Create (string iconName, int size)
{
	if (cache.TryGetValue ((iconName, size), out ImageSource? cached))
		return cached;

	// An unknown name must come back null so callers can fall back to a
	// label rather than rendering a blank square.
	if (!PintaCore.Resources.HasIcon (iconName))
		return null;

	ImageSurface surface = PintaCore.Resources.GetIcon (iconName, size);
	byte[] pixels = surface.GetData ().ToArray ();

	WriteableBitmap bitmap = new (surface.Width, surface.Height);
	pixels.CopyTo (bitmap.PixelBuffer);
	bitmap.Invalidate ();

	cache[(iconName, size)] = bitmap;
	return bitmap;
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Controls/IconImageSource.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Controls/Pads/LayerRowFactory.cs`

**Sharp edges.**
- Copying into the bitmap's pixel buffer and then invalidating is the platform's
  raw-buffer idiom; the file's header comment says it exists to avoid a per-icon
  stream wrapper.
- The pixel layout must already be premultiplied in the order the bitmap expects;
  there is no conversion step here.
- The same idiom serves live thumbnails, letterboxed rather than stretched so a
  tall or wide source keeps its shape.

### Honor EXIF orientation when decoding with SkiaSharp codecs

**When you want this.** You decode photographs and want them upright, the way
every other image viewer shows them.

**The MVVM shape.** Decoding is a service concern. The importer reads the encoded
origin from the codec, swaps the output dimensions when the origin is transposed,
and draws through the matching matrix.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.FileFormats/SkiaCodecFormat.cs
SKEncodedOrigin origin = codec.EncodedOrigin;
// ...
// EXIF origins 5-8 are transposed: the upright image swaps width/height.
bool swapsDimensions = origin is
	SKEncodedOrigin.LeftTop or
	SKEncodedOrigin.RightTop or
	SKEncodedOrigin.RightBottom or
	SKEncodedOrigin.LeftBottom;

Size imageSize =
	swapsDimensions
	? new (decoded.Height, decoded.Width)
	: new (decoded.Width, decoded.Height);
// ...
using (SKCanvas canvas = new (layer.Surface.Bitmap)) {
	canvas.SetMatrix (GetOriginMatrix (origin, imageSize.Width, imageSize.Height));
	canvas.DrawBitmap (decoded, 0, 0, SKSamplingOptions.Default, paint: null);
}

/// <summary>
/// Returns the transform that maps decoded (encoded-orientation) pixels to
/// the upright image, mirroring Skia's SkEncodedOriginToMatrix. The width
/// and height are the dimensions of the upright output image.
/// </summary>
private static SKMatrix GetOriginMatrix (SKEncodedOrigin origin, int w, int h)
	=> origin switch {
		SKEncodedOrigin.TopRight => new (-1, 0, w, 0, 1, 0, 0, 0, 1),
		SKEncodedOrigin.BottomRight => new (-1, 0, w, 0, -1, h, 0, 0, 1),
		SKEncodedOrigin.BottomLeft => new (1, 0, 0, 0, -1, h, 0, 0, 1),
		SKEncodedOrigin.LeftTop => new (0, 1, 0, 1, 0, 0, 0, 0, 1),
		SKEncodedOrigin.RightTop => new (0, -1, w, 1, 0, 0, 0, 0, 1),
		SKEncodedOrigin.RightBottom => new (0, -1, w, -1, 0, h, 0, 0, 1),
		SKEncodedOrigin.LeftBottom => new (0, 1, 0, -1, 0, h, 0, 0, 1),
		_ => SKMatrix.Identity, // TopLeft (default)
	};
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.FileFormats/SkiaCodecFormat.cs`
`Pinta.Brix/tests/libs/Pinta.Brix.FileFormats.Tests/SkiaCodecFormatTests.cs`

**Sharp edges.**
- Four of the eight origins transpose the image, so the destination surface has to
  be allocated with swapped dimensions before drawing.
- The matrix arguments are the upright output dimensions, not the decoded ones.
- Decoding directly into the target color and alpha types avoids a format
  conversion after the fact.

### Combine selection polygons with the CodeBrix PolygonTools library

**When you want this.** Boolean geometry - union, difference, intersection,
exclusion - on user-drawn regions.

**The MVVM shape.** The document's selection object owns a clipper instance and
exposes combine operations; the tools call those. No UI type is involved and the
whole thing is unit-tested headless.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/DocumentSelection.cs
using CodeBrix.PolygonTools;
using CodeBrix.PolygonTools.Enumerations;
using CodeBrix.PolygonTools.Models;
// ...
public PolyClip SelectionClipper { get; } = new ();
// ...
SelectionClipper.AddPath (documentPolygon, PolyType.ptSubject, true);
SelectionClipper.AddPaths (SelectionPolygons, PolyType.ptClip, true);
SelectionClipper.Execute (ClipType.ctDifference, resultingPolygons);
SelectionClipper.Clear ();
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/SelectionModeHandler.cs
//Specify the Clipper Subject (the previous Polygons) and the Clipper Clip (the new Polygons).
//Note: for Union, ignore the Clipper Library instructions - the new polygon(s) should be Clips, not Subjects!
doc.Selection.SelectionClipper.AddPaths (doc.Selection.SelectionPolygons, PolyType.ptSubject, true);
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/DocumentSelection.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Classes/SelectionModeHandler.cs`
`Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/SelectionModeHandlerTests.cs`

**Sharp edges.**
- The subject and clip assignment for a union is the opposite of what the
  library's own documentation suggests; the code comment says so and the tests pin
  it.
- Clear after every execute, or the next operation inherits the previous paths.
- Coordinates are integer paths, so the engine converts its own point type in both
  directions around every call.

### Give a headless library a drawing facade over SkiaSharp

**When you want this.** A large body of drawing code, ported or original, that you
want to keep independent of any one graphics API, or that expects an
immediate-mode vector API.

**The MVVM shape.** A small drawing namespace in the model library - context,
surface, path, pattern, matrix, region. Everything above draws through it; only
the facade knows SkiaSharp.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Drawing/ImageSurface.cs
// Pinta.Brix drawing-layer bitmap surface: a premultiplied-BGRA32 pixel
// buffer with span access, mirroring the surface API the upstream Pinta code
// used. Backed by an SKBitmap so drawing Contexts and SkiaSharp interop share
// the same pixel memory with no copies.

public ImageSurface (Format format, int width, int height)
{
	if (width < 0 || height < 0)
		throw new ArgumentOutOfRangeException (nameof (width), "Surface dimensions must be non-negative");

	Format = format;

	// The engine always draws in premultiplied BGRA32; an A8 surface is
	// stored in the same layout for simplicity (alpha in every channel).
	bitmap = new SKBitmap (new SKImageInfo (
		Math.Max (width, 1),
		Math.Max (height, 1),
		SKColorType.Bgra8888,
		SKAlphaType.Premul));
	bitmap.Erase (SKColors.Transparent);

	Width = width;
	Height = height;
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Drawing/Context.cs
// Fidelity notes:
// - Path coordinates are transformed by the current matrix at verb time and
//   stored in device space, matching the original API's semantics.
// - Arcs are flattened to cubic splines in user space before transforming,
//   so they remain correct under any current matrix.
// - Stroke width and dashes are scaled by the current matrix's mean scale
//   factor; non-uniform-scale stroking (elliptical pens) is approximated.
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Drawing/ImageSurface.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Drawing/Context.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Drawing/Path.cs`

**Sharp edges.**
- Sharing the bitmap's pixel memory means every path that writes pixels directly
  must mark the bitmap dirty afterwards, and every path that reads after drawing
  must flush first.
- A zero-size surface is coerced to one pixel internally while the reported size
  stays as requested, so callers do not have to special-case empty documents.
- Immediate-mode semantics differ from a retained scene graph in ways worth
  documenting in the file itself: device-space path storage, arc flattening, and
  the scale factor used for stroke widths.

### Play a Lottie animation on a Skia head and on native WinUI

**When you want this.** A small looping vector animation, for example on a button,
that looks the same on the Skia heads and in a native WinUI 3 application.

**The MVVM shape.** Pure view concern. The animated player wraps a Lottie source
inside whatever container you want; if the animation sits on a button, the
button's `Command` still binds to the view model.

**Code.**

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml -->
<Page ...
      xmlns:lottie="clr-namespace:CommunityToolkit.WinUI.Lottie;assembly=CodeBrix.Platform.UI.Lottie">
  <!-- ... -->
  <Button Grid.Row="4" Grid.Column="1" Width="80" Height="50"
          VerticalAlignment="Center" HorizontalAlignment="Left"
          Command="{d:Binding ShowOsInfoCommand}">
      <StackPanel Orientation="Horizontal">
          <AnimatedVisualPlayer AutoPlay="True"
                                HorizontalAlignment="Center"
                                VerticalAlignment="Center"
                                Height="40" Width="50">
              <!--NOTE: Visual Studio doesn't recognize 'embedded:' and the path below, but they are correct and will work fine-->
              <lottie:LottieVisualSource UriSource="embedded://JustBetweenUs.Core/JustBetweenUs.Assets.star_icon.json" />
          </AnimatedVisualPlayer>
      </StackPanel>
  </Button>
```

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.WinUI/Views/MainPage.xaml -->
<Page ...
      xmlns:lottie="using:CodeBrix.Platform.WinUI.Lottie">
  <!-- ... -->
  <lottie:AnimatedVisualPlayer AutoPlay="True"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center"
                               Height="40" Width="50">
      <lottie:LottieVisualSource UriSource="ms-appx:///Assets/star_icon.json" />
  </lottie:AnimatedVisualPlayer>
```

**Where to look.**
`JustBetweenUs/CodeBrixPlatform/JustBetweenUs.UI/Views/MainPage.xaml`
`JustBetweenUs/JustBetweenUs.WinUI/Views/MainPage.xaml`
`JustBetweenUs/Shared/Assets/star_icon.json`

**Sharp edges.**
- On the Skia heads the player comes from the default XML namespace and only the
  source needs a prefix; on native WinUI 3 both are prefixed, and the namespace
  declarations differ in form as well.
- The same animation file is loaded by the custom embedded-resource URI on the
  Skia heads and by a packaged content URI on WinUI, because the WinUI project
  ships it as content.
- The Lottie player on the Skia heads needs the Lottie add-in and its animation
  library, both referenced from the library that carries the application's
  packages.

## Media, camera and vision

### Host the VideoPlayer add-in in a page and drive it from the view model

**When you want this.** You are putting the CodeBrix.Platform VideoPlayer add-in
on a page and you want the transport, the chapter list and the caption list to be
view-model state rather than code-behind state.

**The MVVM shape.** The view model owns every decision - what is open, whether it
can be played at all, what the transport may do, which chapter and which caption
track are showing - and reaches the element only through an interface the library
declares and the page implements over the real control. The page constructs the
implementation once when its data context arrives, hands it to the view model, and
forwards the element's events in one line each. Position, duration and volume are
deliberately not on the interface: those are dependency properties the scrubber,
timecodes and volume slider bind straight to.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/IVideoPlayerSurface.cs
public interface IVideoPlayerSurface
{
    void Open(string path);
    void Close();
    void Play();
    void Pause();
    void Stop();
    void SeekToChapter(int index);
    void SelectCaptionTrack(CaptionTrack track);

    TimeSpan Duration { get; }
    bool IsPlaying { get; }
    IReadOnlyList<Chapter> Chapters { get; }
    IReadOnlyList<CaptionTrack> CaptionTracks { get; }
    int CurrentChapterIndex { get; }

    event EventHandler MediaOpened;
    event EventHandler PlaybackEnded;
    event EventHandler<string> MediaFailed;
    event EventHandler PlayStateChanged;
    event EventHandler ChapterChanged;
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
private sealed class VideoPlayerSurface : IVideoPlayerSurface
{
    private readonly VideoPlayer player;

    internal VideoPlayerSurface(VideoPlayer player)
    {
        this.player = player;
        player.RegisterPropertyChangedCallback(
            VideoPlayer.IsPlayingProperty, (_, _) => PlayStateChanged?.Invoke(this, EventArgs.Empty));
        player.ChapterChanged += (_, _) => ChapterChanged?.Invoke(this, EventArgs.Empty);
    }

    // ...

    public void Open(string path)
    {
        //The source has to be unloaded before anything read at open time is changed, and the
        //real path comes last.
        player.Source = "";
        player.AutoPlay = false;
        player.Source = path;
    }

    public void Close() => player.Source = "";

    public void Play() => player.Play();

    public void SeekToChapter(int index) => player.SeekToChapter(index);

    public void SelectCaptionTrack(CaptionTrack track) => player.SelectedCaptionTrack = track;

    internal void RaiseMediaOpened() => MediaOpened?.Invoke(this, EventArgs.Empty);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
private void WireViewModel()
{
    if (ViewModel is not { } viewModel)
    {
        return;
    }

    surface ??= new VideoPlayerSurface(Player);
    viewModel.Playback.AttachSurface(surface);
    viewModel.PickMediaFileAsync = PickMediaFileAsync;
    viewModel.Conversion.PickOutputPathAsync = PickOutputPathAsync;
}

private void Player_MediaOpened(object sender, EventArgs e) => surface?.RaiseMediaOpened();

private void Player_PlaybackEnded(object sender, EventArgs e) => surface?.RaisePlaybackEnded();

private void Player_MediaFailed(object sender, VideoPlayerFailedEventArgs e) => surface?.RaiseMediaFailed(e.Message);
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<Page
    xmlns:video="clr-namespace:CodeBrix.Platform.UI.VideoPlayer.Skia;assembly=CodeBrix.Platform.UI.VideoPlayer.Skia">
  <!-- The stage. The player letterboxes whatever it is given inside it. -->
  <Grid Grid.Row="0" Background="{StaticResource AppStageBrush}">
      <video:VideoPlayer x:Name="Player"
                         Stretch="Uniform"
                         MediaOpened="Player_MediaOpened"
                         PlaybackEnded="Player_PlaybackEnded"
                         MediaFailed="Player_MediaFailed" />
  </Grid>
</Page>
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/IVideoPlayerSurface.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/ViewModels/PlaybackViewModel.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs` and
`Views/MainPage.xaml`

**Sharp edges.**
- Opening has an order: unload the source, change anything read at open time, then
  assign the real path last.
- A dependency property with no event needs a registered property-changed callback
  so the surface can raise its own event.
- XAML-declared handlers arrive on the page, not on the surface, so the page
  forwards each in one line to an internal raise method.
- The wiring runs from the data-context-changed handler, not from the constructor,
  because the data context is created by the XAML.
- Codecs the add-in does not carry itself must be registered by the application at
  startup; see the startup area.

### Play a video from a URL with the MediaPlayer add-in

**When you want this.** Video or audio playback inside a page with the source
chosen by application logic rather than hard-coded in XAML, and standard play,
pause and seek behavior without writing your own commands.

**The MVVM shape.** The view model owns the address string and the resulting
playback source; it builds the source and exposes it as a bound property with a
private setter. The page declares the element with its source one-way bound and
turns on the built-in transport controls. No bridge interface is needed for
playback itself, because the add-in's element is a normal XAML control.

**Code.**

```csharp
// From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs
using CodeBrix.Platform.Simple;
using System;
using Windows.Media.Core;
using Windows.Media.Playback;
// ...
private void LoadMedia()
{
    try
    {
        var uri = new Uri(MediaAddress);
        PlayerSource = MediaSource.CreateFromUri(uri);
        StatusText = $"Loaded: {uri}";
    }
    catch (Exception ex)
    {
        StatusText = $"Cannot load '{MediaAddress}': {ex.Message}";
    }
}

public IMediaPlaybackSource PlayerSource
{
    get;
    private set => SetProperty(ref field, value);
}
```

```xml
<!-- From CodeBrix.Samples/MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml -->
<MediaPlayerElement Grid.Row="1" Margin="0,10,0,10"
                    AutoPlay="True"
                    AreTransportControlsEnabled="True"
                    Source="{d:Binding PlayerSource, Mode=OneWay}"
                    Stretch="{d:Binding SelectedStretch, Mode=OneWay}" />
```

**Where to look.**
`MediaPlayerDemo/src/MediaPlayerDemo.Core/ViewModels/MainViewModel.cs`
`MediaPlayerDemo/src/MediaPlayerDemo.UI/Views/MainPage.xaml`

**Sharp edges.**
- The property type on the view model is the playback-source interface, not the
  concrete type the factory returns; the element's `Source` takes the interface.
- The media types arrive with the MediaPlayer add-in, not with the base platform.
  Without the add-in reference the element and the source type do not resolve.
- Creating a source from a URI succeeds for any well-formed URI. Constructing the
  URI is the only validation here; an unreachable or unplayable address fails
  silently at the element.
- Setting the source is what starts playback, because auto-play is on. If you do
  not want playback on launch, turn auto-play off rather than withholding the
  source.
- With the built-in transport doing the work there is no view-model notion of
  playing, paused or ended. If your application needs to react to playback state,
  reach the underlying player behind an interface the view model consumes, as the
  video-player blueprint above does.
- Assigning a new source replaces the old one; this sample never disposes the
  previous source.

### Play an audio clip straight from bytes with the AudioPlayer add-in

**When you want this.** You have audio in memory - read out of an archive,
downloaded or generated - and want it played without writing a temporary file, on
whichever heads can.

**The MVVM shape.** The view model owns the transport commands and the loop state,
and reaches the element through a bridge of settable delegates that it implements
itself. The page fills the delegates in from its data-context-changed handler.
Every call site is null-guarded, so a head with no player degrades to a viewer
that says so.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/IAudioPlayerBridge.cs
public interface IAudioPlayerBridge
{
    /// <summary>
    /// Hands the player a seekable stream of an audio file it can decode (Ogg Vorbis, WAV, MP3
    /// or FLAC); the player takes ownership of it.
    /// </summary>
    Action<Stream> LoadAudioSource { get; set; }

    Action PlayAudio { get; set; }
    Action PauseAudio { get; set; }
    Action StopAudio { get; set; }
    Action<bool> SetAudioLooping { get; set; }
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
private async Task OpenAudioAsync(AssetEntry entry)
{
    var bytes = await ReadArchiveBytesAsync(entry.EntryPath)
        ?? throw new InvalidDataException($"The bundle has no entry “{entry.EntryPath}”.");

    //Kenney audio is Ogg Vorbis, which the AudioPlayer add-in decodes itself (as it does
    //WAV, MP3 and FLAC) — the bytes go straight to the player, whatever the format.
    var audioStream = new MemoryStream(bytes, writable: false);

    // ... header and facts ...

    IsAudioLooping = false;
    SetAudioLooping?.Invoke(false);
    LoadAudioSource?.Invoke(audioStream);
    SetViewerMode(ViewerMode.Audio,
        LoadAudioSource == null ? "audio playback is not available on this head" : string.Empty);
}

public SimpleCommand PlayAudioCommand => field ??= new SimpleCommand(() => PlayAudio?.Invoke());
public SimpleCommand PauseAudioCommand => field ??= new SimpleCommand(() => PauseAudio?.Invoke());
public SimpleCommand StopAudioCommand => field ??= new SimpleCommand(() => StopAudio?.Invoke());

public SimpleCommand ToggleAudioLoopCommand => field ??= new SimpleCommand(() =>
{
    IsAudioLooping = !IsAudioLooping;
    SetAudioLooping?.Invoke(IsAudioLooping);
});
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs
//Audio bridge: the view model hands over the clip's raw stream and transport
//calls; the AudioPlayer element does the decoding and playing (it takes
//stream ownership)
viewModel.LoadAudioSource = stream =>
{
    _audioPlaybackEnded = false;
    AudioElement?.SetSourceStream(stream);
};
viewModel.PlayAudio = PlayAudio;
viewModel.PauseAudio = () => AudioElement?.Pause();
viewModel.StopAudio = () =>
{
    _audioPlaybackEnded = false;
    AudioElement?.Stop();
};
viewModel.SetAudioLooping = looping =>
{
    if (AudioElement != null) { AudioElement.IsLooping = looping; }
};
```

```xml
<!-- From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml -->
<audio:AudioPlayer x:Name="AudioElement" />
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/IAudioPlayerBridge.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml.cs`

**Sharp edges.**
- The element takes ownership of the stream handed to it; do not dispose it
  yourself, and do not hand it the same stream twice.
- The element decodes several common formats itself, so the application never
  needs a format check before playing.
- Opening a different asset stops whatever was playing first, as does leaving the
  viewer.
- The interface documents the contract: the view model must behave sensibly when a
  delegate is null, and the pane's hint text is what the user sees on a head where
  the bridge was never filled in.
- The scrubber binds straight to the element; see the views area. Replaying a
  finished clip needs one extra rule; see the bridge area.

### Probe a media file behind an interface the view model resolves

**When you want this.** You need to know what is inside a media file - size,
duration, codecs, chapter and caption counts - before you offer anything to do
with it.

**The MVVM shape.** The probe is registered as a singleton at startup and resolved
in the view model's constructor. The view model calls it inside a try/catch, sets
the busy flag around the call so the commands disable themselves, and turns any
failure into one sentence in the status bar.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Probing/IMediaProbe.cs
public interface IMediaProbe
{
    /// <summary>
    /// Probes one file. A <c>.cbv</c> file is read by the playback core's own container readers; every
    /// other file is probed with ffprobe through CodeBrix.VideoProcessing.
    /// </summary>
    Task<SourceMediaInfo> ProbeAsync(string path, CancellationToken cancellationToken);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Probing/MediaProbe.cs
public Task<SourceMediaInfo> ProbeAsync(string path, CancellationToken cancellationToken)
{
    // ... null and File.Exists guards, each throwing VideoToolProcessingException ...

    var format = MediaFormats.Detect(path);
    if (format == MediaFormatKind.Unknown)
    {
        throw new VideoToolProcessingException(
            $"'{Path.GetFileName(path)}' is not a container this application recognises.");
    }

    return MediaFormats.IsCodeBrixContainer(format)
        ? Task.FromResult(ProbeCodeBrixContainer(path, format))
        : ProbeWithFfProbeAsync(path, format, cancellationToken);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs
public async Task<SourceMediaInfo> AddAsync(string path, CancellationToken cancellationToken)
{
    IsBusy = true;
    try
    {
        var existing = Library.FirstOrDefault(i =>
            string.Equals(i.Path, path, StringComparison.Ordinal));
        if (existing is not null)
        {
            SelectedItem = existing;
            StatusText = $"{existing.FileName} is already in the list.";
            return existing;
        }

        var info = await probe.ProbeAsync(path, cancellationToken);
        Library.Add(info);
        NotifyPropertyChanged(nameof(EmptyLibraryVisibility));
        SelectedItem = info;
        StatusText = $"Opened {info.FileName} - {info}";
        return info;
    }
    catch (VideoToolProcessingException exception)
    {
        StatusText = exception.Message;
        return null;
    }
    catch (OperationCanceledException)
    {
        StatusText = "Cancelled.";
        return null;
    }
    finally
    {
        IsBusy = false;
    }
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Probing/IMediaProbe.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Probing/MediaProbe.cs`
`CodeBrixVideoTool/src/CodeBrixVideoTool.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Two probing routes, and the class comment says why: an external prober cannot
  read a bespoke container at all and would see a constrained standard container
  as an ordinary one. Your own formats go to your own readers; everything else
  goes to the external tool.
- The external call is wrapped in a filtered catch for the library's own exception
  types and for I/O failures, with cancellation rethrown before it so a cancel is
  not reported as a probe failure. That filtered catch is also the whole of this
  application's behavior when the external tools are absent: there is no
  availability check anywhere. A missing tool surfaces as one of those exceptions,
  becomes the application's own exception, and lands in the status bar, while
  files read by the in-process readers keep opening.
- The probe refuses a file with no video track, and refuses one that states no
  duration, because progress could not then be reported - the progress design
  reaching back into the intake rules.
- The probe result doubles as the list item model, with badge, summary and
  playability as bindable derived properties.

### Detect a container from its first bytes

**When you want this.** Two different formats share a file extension and you have
to tell them apart the way the reader will.

**The MVVM shape.** A static method in the formats class, with no I/O in the view
model. It falls back to the extension when the file cannot be read.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs
/// <remarks>
/// A <c>.cbv</c> file is Mode 2 when it starts with the ASCII bytes "CBVF" and Mode 1 when it
/// starts with the EBML magic. Nothing else about either file is consulted, which is exactly how
/// the playback core picks its reader.
/// </remarks>
public static MediaFormatKind Detect(string path)
{
    // ...
    var extension = Path.GetExtension(path).ToLowerInvariant();
    var sniffed = SniffSignature(path);

    if (extension == ".cbv")
    {
        return sniffed == MediaFormatKind.Unknown ? MediaFormatKind.Unknown : sniffed;
    }
    // ... .mkv, .webm, then ImportExtensions -> Mp4, else Unknown ...
}

private static MediaFormatKind SniffSignature(string path)
{
    try
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        Span<byte> first = stackalloc byte[4];
        if (stream.Read(first) < 4)
        {
            return MediaFormatKind.Unknown;
        }

        if (CbvReader.IsCbv(first))
        {
            return MediaFormatKind.CodeBrixMode2;
        }

        return first.SequenceEqual(CbvFormat.EbmlMagic) ? MediaFormatKind.CodeBrixMode1 : MediaFormatKind.Unknown;
    }
    catch (IOException) { return MediaFormatKind.Unknown; }
    catch (UnauthorizedAccessException) { return MediaFormatKind.Unknown; }
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs`

**Sharp edges.**
- The playback library publishes both the test and the magic constant, so the
  application does not hard-code signature bytes.
- A file whose signature matches neither expectation is unknown and is refused,
  rather than being trusted because of its extension.

### Author a cbv file in either container mode from a settled plan

**When you want this.** You are writing CodeBrix video with the authoring library
and want to know which knobs correspond to which output.

**The MVVM shape.** All of it is in a service behind an interface; the view model
supplies a plan and a progress sink and never touches the authoring API. The plan
carries the destination; the runner turns the destination into a flavour, a
container, a cue policy and an audio codec.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs
var request = new VideoAuthoringRequest
{
    SourcePath = sourcePath,
    OutputPath = plan.OutputPath,
    SourceDuration = plan.Source.Duration,
    TemporaryFolder = workingFolder,
    ChaptersPath = sidecars.ChaptersPath,
    CancellationToken = cancellationToken,

    //The bespoke CBVF container is written by the muxer in the playback core; the other
    //three are written by FFmpeg's own WebM and Matroska muxers.
    Flavour = plan.Destination == MediaFormatKind.CodeBrixMode2
        ? VideoAuthoringFlavour.Bespoke
        : VideoAuthoringFlavour.WebMProfile,

    Container = plan.Destination == MediaFormatKind.Matroska
        ? AuthoringContainerFormat.Matroska
        : AuthoringContainerFormat.WebM,

    //Only the two .cbv flavours are meant to satisfy the streamable profile. A standard MKV
    //is checked and reported on, but its failures are not this application's business.
    CuesToFront = plan.Destination != MediaFormatKind.Matroska,
    ValidateProfile = true,
    FailWhenProfileFails = MediaFormats.IsCodeBrixContainer(plan.Destination),
};

request.Video.FrameSize = plan.IsResized
    ? AuthoringFrameSize.Exact(plan.Resolution.Width, plan.Resolution.Height)
    : AuthoringFrameSize.Source;
request.Video.SpeedPreset = Av1SpeedPreset;
request.Video.ConstantRateFactor = Av1RateFactor(plan.Quality);

request.Audio.Include = plan.Source.HasAudio;
request.Audio.Codec = plan.AudioCodec == TargetAudioCodec.Vorbis
    ? AuthoringAudioCodec.LibVorbis
    : AuthoringAudioCodec.LibOpus;

foreach (var caption in sidecars.Captions)
{
    request.Captions.Add(new AuthoringCaptionInput(
        caption.Path, caption.Language, caption.Name, caption.Flags));
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs
public static TargetAudioCodec AudioCodecFor(MediaFormatKind kind) => kind switch
{
    MediaFormatKind.Mp4 => TargetAudioCodec.Aac,
    MediaFormatKind.Matroska => TargetAudioCodec.Opus,
    MediaFormatKind.WebM => TargetAudioCodec.Opus,
    MediaFormatKind.CodeBrixMode1 => TargetAudioCodec.Opus,

    //The hard invariant: a bespoke CBVF file this application writes carries Vorbis, never Opus.
    MediaFormatKind.CodeBrixMode2 => TargetAudioCodec.Vorbis,

    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "There is no audio codec for an unrecognised format."),
};
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormats.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/MediaFormatKind.cs`

**Sharp edges.**
- The two container modes, as the code names them. Mode 1 writes a `.cbv` that is
  a WebM constrained to the streamable profile - AV1 video, Opus audio, cues in
  front of the first cluster. Mode 2 writes a `.cbv` in the bespoke container -
  AV1 video, Vorbis audio, every index entry and every caption cue ahead of the
  media data. The codec table calls the Vorbis choice "the hard invariant".
- Audio sample rate is set per codec, and the reason is recorded in the code: one
  encoder's bit-rate mode opens only inside a band that depends on both the sample
  rate and the channel count, so the application uses its quality path instead;
  the other is always resampled to its own internal rate.
- The cue policy is on for everything except the plain standard container, and
  failing the profile check is fatal only for the two application formats - the
  standard container is checked and reported on but is expected to fail, and that
  failure is not an error.
- The authoring library is synchronous, so the pass runs on a worker thread with
  `Task.Run(() => CbvAuthor.Write(request), CancellationToken.None)` - note the
  `None`: cancellation reaches the library through the request's own token, not
  through `Task.Run`.
- The library takes captions and chapters only as files, which is why the sidecar
  step exists at all.

### Export an mp4 with FFmpeg through the CodeBrix VideoProcessing library

**When you want this.** You want the FFmpeg argument-builder style - inputs,
stream selection, codecs, filters, progress and cancellation - from a service a
view model drives.

**The MVVM shape.** The same service and the same interface, with a different
private method chosen by the plan's destination. The command line that was run is
returned in the outcome for the record.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs
var arguments = FFMpegArguments.FromFileInput(sourcePath);
foreach (var caption in sidecars.Captions)
{
    arguments = arguments.AddFileInput(caption.Path, false);
}

if (sidecars.HasChapters)
{
    arguments = arguments.AddFileInput(sidecars.ChaptersPath, false)
        .MapMetaData(sidecars.Captions.Count + 1);
}

var errors = new List<string>();
var processor = arguments
    .OutputToFile(plan.OutputPath, true, options =>
    {
        options.SelectStream(0, 0, Channel.Video);
        if (plan.Source.HasAudio)
        {
            options.SelectStream(0, 0, Channel.Audio);
        }

        for (var index = 0; index < sidecars.Captions.Count; index++)
        {
            options.SelectStream(0, index + 1, Channel.Subtitle);
            options.WithStreamMetadata(Channel.Subtitle, index, "language", sidecars.Captions[index].Language);
        }

        options
            .WithVideoCodec("libx264")
            .WithConstantRateFactor(H264RateFactor(plan.Quality))
            .WithSpeedPreset(Speed.Medium)
            .ForcePixelFormat("yuv420p");

        if (plan.IsResized)
        {
            options.WithVideoFilters(filters => filters.Scale(plan.Resolution.Width, plan.Resolution.Height));
        }

        // ... audio codec, bitrate, and the channel argument ...

        if (sidecars.Captions.Count > 0)
        {
            //MP4's own timed-text track. Nothing else in the MP4 family carries WebVTT.
            options.WithSubtitleCodec("mov_text");
        }

        options.WithFastStart().ForceFormat("mp4");
    })
    .NotifyOnProgress(
        percent => progress?.Report(new ConversionProgress(gerund, 2, 2, percent)),
        plan.Source.Duration)
    .NotifyOnError(errors.Add)
    .CancellableThrough(cancellationToken);

var commands = new[] { "ffmpeg " + processor.Arguments };
var succeeded = await processor.ProcessAsynchronously(false).ConfigureAwait(false);
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs`

**Sharp edges.**
- Progress needs the source duration to turn a position into a percentage, which
  is why the probe refuses a file that states none.
- `ProcessAsynchronously(false)` is used throughout: false means "return a bool
  rather than throw", so the code checks the result and the output file itself and
  reports the last few error lines it collected.
- Cancellation is checked twice: the exception from the cancellable wrapper, and
  the token after the call returns. Both delete the part-written file.
- Where a destination must reduce channels it has to say so explicitly, because
  left to itself the encoder can refuse the layout outright and the export fails.
- Captions in this container need the container's own timed-text codec; nothing
  else in that family carries the caption format the sidecars are written in.
- Order matters in the option chain: the streaming-friendly flag before the forced
  format.

### Demultiplex a bespoke container and remux it so an external tool can read it

**When you want this.** Your own container holds perfectly ordinary elementary
streams, and you want to hand them to a tool that cannot open the container.

**The MVVM shape.** A service class the runner uses as its first stage; the view
model never knows it happened beyond one note in the run notes and one sentence in
the route line.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/Mode2Extractor.cs
using (var reader = MediaContainers.Open(source.Path))
{
    if (reader is not CbvReader) { /* ... refuse ... */ }

    var video = reader.Tracks.FirstOrDefault(t => t.Kind == MediaTrackKind.Video)
        ?? throw new VideoToolProcessingException($"'{source.FileName}' carries no video track.");

    if (!string.Equals(video.CodecId, VideoCodecIds.Av1, StringComparison.OrdinalIgnoreCase))
    {
        throw new VideoToolProcessingException(
            $"'{source.FileName}' carries '{video.CodecId}' video; only AV1 can be re-wrapped into IVF.");
    }

    var audio = reader.Tracks.FirstOrDefault(t => t.Kind == MediaTrackKind.Audio);

    using var ivf = IvfWriter.CreateAv1(ivfPath, video.Width, video.Height);
    var ogg = audio is null ? null : CreateAudioWriter(audio, oggPath, source.FileName);

    try
    {
        Demultiplex(reader, video.Id, audio?.Id ?? -1, ivf, ogg, cancellationToken,
            out videoFrames, out audioPackets);

        ivf.Complete();
        ogg?.Complete();
    }
    finally
    {
        ogg?.Dispose();
    }

    // ...
    sidecars = SidecarExtractor.ExtractFromReader(reader, workingFolder);
}

await RemuxAsync(ivfPath, hasAudio ? oggPath : null, intermediatePath, cancellationToken)
    .ConfigureAwait(false);
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/Mode2Extractor.cs
//One packet of lookahead on the audio side: an Ogg granule position says where a packet
//ENDS, and the next packet's timestamp is the most reliable statement of that.
byte[] pendingAudio = null;
// ...
while (reader.TryReadPacket(out var packet))
{
    cancellationToken.ThrowIfCancellationRequested();

    if (packet.TrackId == videoTrackId)
    {
        ivf.WriteFrame(packet.Data.Span, packet.Timestamp);
        videoFrames++;
        continue;
    }
    // ...
    //MediaPacket.Data is borrowed from the reader and is gone on the next read.
    pendingAudio = packet.Data.ToArray();
    pendingTimestamp = packet.Timestamp;
    pendingDuration = packet.Duration;
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/Mode2Extractor.cs
var succeeded = await arguments
    .OutputToFile(outputPath, true, options =>
    {
        options.SelectStream(0, 0, Channel.Video);
        if (oggPath is not null)
        {
            options.SelectStream(0, 1, Channel.Audio);
        }

        options.WithCopyCodec().ForceFormat("matroska");
    })
    .NotifyOnError(errors.Add)
    .CancellableThrough(cancellationToken)
    .ProcessAsynchronously(false)
    .ConfigureAwait(false);
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/Mode2Extractor.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/Mode2Extraction.cs`

**Sharp edges.**
- A packet's data is borrowed from the reader and is gone on the next read, so a
  packet held for lookahead must be copied.
- The lookahead exists because a granule position states where a packet ends, and
  the next packet's timestamp is the most reliable statement of that. The final
  packet falls back to its own duration, or to the reader's.
- The re-wrapping uses the playback library's own elementary-stream writers - the
  same two containers the authoring library writes when it builds a bespoke file,
  used here in the opposite direction.
- The remux copies codecs: nothing is decoded and nothing is re-encoded, so from
  the intermediate onwards the conversion is an ordinary one.
- The writers throw when the codec-private data is not what they expect; the
  extractor catches that and restates it as its own message.
- The reader is disposed only after the sidecars are taken out of it; both uses
  share the one open reader.

### Lift chapters and captions out of a source into sidecar files

**When you want this.** Your encoder takes captions and chapters only as separate
input files, and your sources carry them embedded.

**The MVVM shape.** A service with two routes - the container reader for the
formats you own, the external tool for the rest - producing one value the runner
hands on. Anything that could not be carried across becomes a sentence in the
notes, which reaches the operation panel.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/SidecarExtractor.cs
public async Task<MediaSidecars> ExtractAsync(
    SourceMediaInfo source, string workingFolder, CancellationToken cancellationToken)
{
    ArgumentNullException.ThrowIfNull(source);
    Directory.CreateDirectory(workingFolder);

    return MediaFormats.IsSupportedFormat(source.Format)
        ? ExtractFromContainerReader(source, workingFolder)
        : await ExtractWithFfmpegAsync(source, workingFolder, cancellationToken).ConfigureAwait(false);
}

private static MediaSidecars ExtractFromContainerReader(SourceMediaInfo source, string workingFolder)
{
    try
    {
        using var reader = MediaContainers.Open(source.Path);

        //A Matroska or WebM file interleaves its subtitle cues with the video, so the cues are
        //complete only once the file has been read through. The bespoke container keeps every
        //cue in its header, so its tracks are complete the instant it is open.
        if (reader.CaptionTracks.Count > 0 && reader.CaptionTracks.Any(t => !t.AreCuesComplete))
        {
            while (reader.TryReadPacket(out _))
            {
                //Draining for the cues; nothing is decoded and no packet is kept.
            }
        }

        return ExtractFromReader(reader, workingFolder);
    }
    catch (Exception exception) when (exception is not VideoToolProcessingException)
    {
        throw new VideoToolProcessingException(
            $"The chapters and captions in '{source.FileName}' could not be read: {exception.Message}", exception);
    }
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/SidecarExtractor.cs
private static readonly string[] TextCaptionCodecs =
[
    "subrip", "srt", "webvtt", "ass", "ssa", "mov_text", "text", "eia_608", "subviewer",
];

// ... in the ffmpeg route, per subtitle stream:
if (!TextCaptionCodecs.Contains(codec, StringComparer.OrdinalIgnoreCase))
{
    notes.Add($"Caption track {index} is '{codec}', which has no text form, so it was not carried across.");
    continue;
}

var succeeded = await FFMpegArguments
    .FromFileInput(source.Path)
    .OutputToFile(path, true, options => options
        .SelectStream(index, 0, Channel.Subtitle)
        .WithSubtitleCodec("webvtt")
        .ForceFormat("webvtt"))
    .CancellableThrough(cancellationToken)
    .ProcessAsynchronously(false)
    .ConfigureAwait(false);
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/SidecarExtractor.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/MediaSidecars.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Containers/WebVttFile.cs`

**Sharp edges.**
- A container that interleaves its subtitle cues with the video is complete only
  after the whole file has been read through; one that keeps every cue in its
  header is complete the instant it is open. Drain the reader only when a track
  says its cues are incomplete.
- An image-based caption track has no text form at all, so report it in a note
  rather than losing it silently.
- Where the application supports one title language, chapters are collapsed to one
  untagged title each, and the note counts distinct languages dropped across the
  whole file rather than chapters.
- An external tool sometimes reports a placeholder string as a chapter title;
  treat that like an empty title and substitute a generated one.
- The playback library reads the caption format and formats its timestamps but
  publishes no writer, so this application brings a small one, preserving cue
  identifiers and settings because both destinations that can carry them do.

### Build a resolution ladder keyed on the short side with even dimensions

**When you want this.** You are offering downscale choices and want them to read
correctly for portrait video as well as landscape.

**The MVVM shape.** A static builder returning rows the view model just copies
into an observable collection.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Resolution/ResolutionLadder.cs
public static IReadOnlyList<int> StandardShortSides { get; } = [1440, 1080, 720, 480];

public static IReadOnlyList<ResolutionOption> Build(int sourceWidth, int sourceHeight)
{
    // ... positive-dimension guards ...

    var rungs = new List<ResolutionOption>
    {
        ResolutionOption.Original(MakeEven(sourceWidth), MakeEven(sourceHeight)),
    };

    //The rung names the SHORT side, so a portrait source is measured across its width and a
    //landscape one across its height - which is what height keying did for every landscape source,
    //and is why landscape ladders are unchanged.
    var sourceShortSide = Math.Min(sourceWidth, sourceHeight);
    var sourceLongSide = Math.Max(sourceWidth, sourceHeight);
    var isPortrait = sourceWidth < sourceHeight;

    foreach (var shortSide in StandardShortSides)
    {
        //Strictly below: a source whose short side is already 1080 is not offered "1080p".
        if (shortSide >= sourceShortSide)
        {
            continue;
        }

        var keyed = MakeEven(shortSide);
        var other = ProportionalOtherSide(sourceShortSide, sourceLongSide, shortSide);

        rungs.Add(ResolutionOption.Reduced(
            shortSide + "p",
            isPortrait ? keyed : other,
            isPortrait ? other : keyed));
    }

    return rungs;
}

/// <summary>Rounds a dimension to the nearest even number of pixels, never below 2.</summary>
public static int MakeEven(int value)
{
    if (value <= 2)
    {
        return 2;
    }

    return (value % 2 == 0) ? value : value + 1;
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Resolution/ResolutionLadder.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Resolution/ResolutionOption.cs`

**Sharp edges.**
- A rung's number names the short side, which is the industry convention; the
  comment gives the case it fixes, where a portrait phone clip would otherwise be
  offered a rung far narrower than intended.
- Every dimension is even, because the chroma planes of the pixel format in use
  are half-size in each direction and an odd dimension has no representation in
  it; the evening is applied to the source's own size too, so even the "original"
  rung is safe.
- Strictly below: a source already at a standard short side is not offered that
  rung.

### Move one encoder knob and pin everything else

**When you want this.** You are offering a quality choice and want it to mean one
thing, comparably, across two different encoders.

**The MVVM shape.** An enum of stops offered from a static list, a single
view-model property with a default, and two private mapping functions in the
service that turn a stop into a rate factor.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs
//Faster than the authoring library's own default of 6, which matters a great deal for an
//application a person is sitting in front of, and costs very little at these bit rates. It is
//PINNED: the quality knob moves the rate factor only, so an encode takes about as long whichever
//stop is chosen.
private const int Av1SpeedPreset = 8;

//THE QUALITY KNOB, IN ITS ENTIRETY. A quality stop moves the encoder's constant rate factor and
//nothing else: the speed presets above stay pinned, and sound is settled by the destination alone.
// ... a calibration table, elided ...
private static int Av1RateFactor(QualityLevel quality) => quality switch
{
    QualityLevel.Fair => 42,
    QualityLevel.Better => 24,
    QualityLevel.Best => 18,
    _ => 30,
};

private static int H264RateFactor(QualityLevel quality) => quality switch
{
    QualityLevel.Fair => 27,
    QualityLevel.Better => 17,
    QualityLevel.Best => 14,
    _ => 20,
};
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml -->
<!-- The quality stop. Its rows are the QualityLevel values themselves, whose own
     names are the words to show, so this drop-down carries no item template. -->
<StackPanel Grid.Column="4">
    <TextBlock Text="Quality" Style="{StaticResource FieldLabel}" />
    <ComboBox x:Name="QualityBox"
              HorizontalAlignment="Stretch"
              PlaceholderText="Choose a quality"
              ItemsSource="{d:Binding Conversion.QualityLevels}"
              SelectedItem="{d:Binding Conversion.SelectedQuality, Mode=TwoWay}" />
</StackPanel>
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Operations/ConversionRunner.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Formats/QualityLevel.cs`

**Sharp edges.**
- Both speed presets are pinned, so an encode takes about as long whichever stop
  is chosen, and the knob means one thing.
- The two rate-factor sets were chosen to match each other stop for stop rather
  than to look tidy on either encoder's own scale, so picking a stop gives the
  same picture whichever destination is chosen. One encoder's scale moves about
  half as far as the other's in this band, which is why its steps are smaller.
- The comment records how the numbers were arrived at: lossless masters of
  synthetic sources, encoded at every candidate rate factor with the presets
  pinned, compared against their masters through the tool's own quality filters,
  with the inputs re-timestamped by frame index first because a one-frame slip
  swamps everything a rate factor does. Nothing was installed to measure it.
- Sound is never touched by the quality knob; it is settled by the destination
  alone.
- The drop-down carries no item template, because the enum's own names are the
  words to show.

### Download run scoped media into a self cleaning temp cache

**When you want this.** You must fetch many remote files for one operation, the
URLs are short-lived, and nothing should be left on disk afterwards.

**The MVVM shape.** A disposable cache object created inside the service method
with `using`, so its temp folder disappears when the operation ends. Failures
return a result object with a reason instead of throwing, so one bad file cannot
fail the whole job; only a user cancellation is rethrown.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/MediaCache.cs
/// <summary>
/// Downloads every referenced media file once per run into a private temp folder,
/// and deletes the folder at the end. Notion's uploaded-file URLs are pre-signed
/// and expire in about an hour, so downloads always happen in the same run that
/// fetched the block tree — cached URLs are never persisted or reused later.
/// </summary>
internal sealed class MediaCache : IDisposable
{
    /// <summary>Media larger than this is not downloaded (a card is rendered instead).</summary>
    public const long DefaultMaxDownloadBytes = 100L * 1024 * 1024;

    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromMinutes(3) };
    private readonly Dictionary<string, CachedMedia> _byUrl = new(StringComparer.Ordinal);
    // ...

    /// <summary>The temp folder holding this run's downloads (deleted on dispose).</summary>
    public string CacheDirectory { get; } =
        Path.Combine(Path.GetTempPath(), "NotionDocumentCreator", Guid.NewGuid().ToString("N"));

    public async Task<CachedMedia> FetchAsync(
        string url, long maxBytes = DefaultMaxDownloadBytes, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (string.IsNullOrWhiteSpace(url))
        {
            return CachedMedia.Failed("No URL was supplied for the media file.");
        }
        if (_byUrl.TryGetValue(url, out var cached)) { return cached; }

        var result = await DownloadAsync(url, maxBytes, cancellationToken).ConfigureAwait(false);
        _byUrl[url] = result;
        return result;
    }
    // ...
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/MediaCache.cs
while ((read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
{
    total += read;
    if (total > maxBytes)
    {
        //The server did not declare a length — enforce the cap while streaming
        target.Close();
        File.Delete(filePath);
        return CachedMedia.Failed("File exceeded the download cap.");
    }
    await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
}
// ...
catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
{
    throw; //A user cancel should cancel the run, not become a warning
}
catch (Exception ex)
{
    return CachedMedia.Failed($"Download failed: {ex.Message}");
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/MediaCache.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/MediaPreparer.cs`

**Sharp edges.**
- The size cap is enforced twice: against the declared content length, and again
  while streaming for servers that declare none. Reading only the response headers
  first is what lets the cap be checked before the body is buffered.
- Disposal swallows a failure to delete the folder: a locked temp file must not
  crash disposal, and the operating system's temp cleaner will get it.
- Results are cached per URL, so the same picture used on several pages is fetched
  once; the test asserts the two calls return the same instance.
- The download pass runs before rendering precisely so the renderer can be
  synchronous and look results up by key.

### Extract a video poster frame and degrade when the external tool is missing

**When you want this.** You want a still image from a video, or a media duration,
using tools that may not be installed on the user's machine.

**The MVVM shape.** A static helper in the library wraps every call so a missing
tool, an unreadable codec or a timeout yields null; the caller turns null into a
rendered card plus one warning, never a failure.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/VideoPosterExtractor.cs
using CodeBrix.VideoProcessing;

/// <summary>
/// Extracts a poster frame from a downloaded video via ffmpeg, and probes media
/// durations via ffprobe. Every path is wrapped so a missing ffmpeg, an
/// unreadable codec or a timeout produces a null result (the caller renders a
/// media card plus one warning) — never a failed document.
/// </summary>
internal static class VideoPosterExtractor
{
    /// <summary>Probes the duration of a media file; null when ffprobe cannot say.</summary>
    public static TimeSpan? TryProbeDuration(string mediaFilePath)
    {
        try
        {
            var analysis = FFProbe.Analyse(mediaFilePath);
            var duration = analysis?.Duration ?? TimeSpan.Zero;
            return duration > TimeSpan.Zero ? duration : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Grabs a poster frame — at 10% of the duration, or one second in for very
    /// short clips — and returns the PNG bytes, or null when extraction fails.
    /// </summary>
    public static byte[] TryExtractPoster(string videoFilePath, string workDirectory, out TimeSpan? duration)
    {
        duration = TryProbeDuration(videoFilePath);
        try
        {
            var captureAt = duration is { } known && known > TimeSpan.FromSeconds(10)
                ? TimeSpan.FromTicks(known.Ticks / 10)
                : TimeSpan.FromSeconds(1);
            if (duration is { } total && captureAt >= total)
            {
                captureAt = TimeSpan.FromTicks(total.Ticks / 2);
            }

            Directory.CreateDirectory(workDirectory);
            var posterPath = Path.Combine(workDirectory, $"poster-{Guid.NewGuid():N}.png");
            try
            {
                if (!FFMpeg.Snapshot(videoFilePath, posterPath, size: null, captureTime: captureAt))
                {
                    return null;
                }
                return File.Exists(posterPath) ? File.ReadAllBytes(posterPath) : null;
            }
            finally
            {
                if (File.Exists(posterPath)) { File.Delete(posterPath); }
            }
        }
        catch (Exception)
        {
            return null;
        }
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/VideoPosterExtractor.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/MediaPreparer.cs`
`NotionDocumentCreator/THIRD-PARTY-NOTICES.txt`

**Sharp edges.**
- The external tools are the host's, not bundled; the notices file says so and the
  whole class exists to make their absence a non-event.
- Even when the poster fails, the probed duration is kept and printed on the media
  card.
- The temporary poster file is deleted in a `finally`, inside the cache folder
  that is itself deleted at the end of the run.
- Audio blocks never attempt a poster; they only probe a duration.

### Enumerate cameras and start a live capture session

**When you want this.** A camera dropdown that populates itself at startup, starts
the first camera automatically, and switches cleanly when the user picks another.

**The MVVM shape.** A capture service class wraps the webcam library and exposes a
small surface: a static discovery method, start, stop, a "has a frame" flag, a
copy-latest-frame method and a frame-arrived event. The view model owns the
service, holds the devices in an observable collection, and switches cameras from
the selected-item setter. Discovery is async and its results are marshalled onto
the UI thread.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Webcam/WebcamCaptureService.cs
public static async Task<IReadOnlyList<CameraDevice>> GetCamerasAsync()
{
    IReadOnlyList<IImagingMediaDevice> devices = await WebcamDevices.GetImagingMediaDeviceListAsync();
    var cameras = new List<CameraDevice>();
    foreach (IImagingMediaDevice device in devices)
    {
        cameras.Add(new CameraDevice(device));
    }
    return cameras;
}

public void Start(CameraDevice camera)
{
    if (camera == null) { throw new ArgumentNullException(nameof(camera)); }

    Stop();

    _session = new WebcamSession(camera.Device);
    _session.FrameReceived += OnFrameReceived;
    _session.Start();
}

private void OnFrameReceived(object sender, WebcamFrameEventArgs frame)
{
    //Capture-thread context: the session caches the pixels itself (see TryCopyLatestFrame);
    //  we only note that a frame exists and get out fast.
    _hasFrame = true;
    FrameArrived?.Invoke(this, EventArgs.Empty);
}

public bool TryCopyLatestFrame(ref byte[] buffer, out int width, out int height)
{
    WebcamSession session = _session;
    if (session == null)
    {
        width = 0;
        height = 0;
        return false;
    }
    return session.TryCopyLatestFrame(ref buffer, out width, out height);
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
private async Task InitializeAsync()
{
    try
    {
        var cameras = await WebcamCaptureService.GetCamerasAsync();
        InvokeOnMainThread(() =>
        {
            Cameras.Clear();
            foreach (var camera in cameras)
            {
                Cameras.Add(camera);
            }
            if (Cameras.Count == 0)
            {
                StatusText = "No cameras were found on this machine.";
            }
            else
            {
                StatusText = $"Found {Cameras.Count} camera(s).";
                SelectedCamera = Cameras[0]; //auto-start on the first camera
            }
        });
    }
    catch (Exception e)
    {
        InvokeOnMainThread(() => StatusText = $"Camera discovery failed: {e.Message}");
    }
}

// ...

private void SwitchCamera(CameraDevice camera)
{
    try
    {
        HasFrame = false;
        if (camera == null)
        {
            _captureService.Stop();
            InvalidatePreviewCanvas?.Invoke();
            return;
        }

        _captureService.Start(camera);
        StatusText = $"Live: {camera.FriendlyName}";
    }
    catch (Exception e)
    {
        StatusText = $"Could not start '{camera?.FriendlyName}': {e.Message}";
    }
}
```

```xml
<!-- From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml -->
<ComboBox MinWidth="280" VerticalAlignment="Center"
          ItemsSource="{d:Binding Cameras}"
          SelectedItem="{d:Binding SelectedCamera, Mode=TwoWay}"
          IsEnabled="{d:Binding IsCameraMode}" />
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Webcam/WebcamCaptureService.cs`
`PalmVisualizer/src/libs/PalmVisualizer.Camera/WebcamCaptureService.cs`
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The frame event fires on the capture thread; the documentation says so, and
  every handler is written to get out fast and marshal its own UI work.
- Start calls stop first, so switching cameras never leaves two sessions running,
  and stop unsubscribes before disposing and clears the "has a frame" flag so a
  stale frame from the previous camera cannot be drawn.
- That flag is `volatile`, because it is written on the capture thread and read
  from the UI thread.
- The service does not cache pixels itself; the underlying session does, and the
  copy method forwards to it with a caller-owned buffer reallocated only when the
  size changes.
- Enumeration is a static method and works with no session running and no camera
  present, so it is safe to call at startup, and an empty device list is a normal
  state rather than an error.
- Discovery is kicked off from the constructor as a discarded task after setting a
  "discovering" status, and every failure path writes to the same status line
  rather than throwing into the constructor.

### Wrap a device library type so the view model never sees it

**When you want this.** You want the dropdown to bind to a plain object with a
display name, and to be free to change the capture library later without touching
the view model or the XAML.

**The MVVM shape.** A small sealed wrapper with an `internal` constructor and an
`internal` property holding the real device. Everything the UI needs is public;
everything the library needs is internal.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraDevice.cs
/// <summary>
/// One connected camera, as shown in the camera-selection dropdown. Wraps the discovered
/// device so consumers of this library never handle CodeBrix.Webcam types directly.
/// </summary>
public sealed class CameraDevice
{
    internal CameraDevice(IImagingMediaDevice device)
    {
        Device = device;
    }

    internal IImagingMediaDevice Device { get; }

    /// <summary>The camera's unique hardware identifier.</summary>
    public string Id => Device.Id;

    /// <summary>The camera's human-readable name.</summary>
    public string FriendlyName => Device.FriendlyName;

    /// <summary>The dropdown display text.</summary>
    public override string ToString() => Device.FriendlyName;
}
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Camera/CameraDevice.cs`
`WebcamPainter/src/libs/WebcamPainter.Webcam/CameraDevice.cs`

**Sharp edges.**
- `ToString()` is what a `ComboBox` displays, so no item template and no
  display-member binding are needed. There is a test asserting the identity, to
  keep it that way.
- The internal constructor means only the library can mint one, so a device
  instance in the view model always came from real enumeration.
- The same shape serves tracking results: public read-only properties, internal
  constructors.

### Run a TFLite model through the OpenCV DNN module

**When you want this.** You have a model file and want to run it from a
CodeBrix.Platform application without an extra inference runtime, using the OpenCV
managed binding the application already carries.

**The MVVM shape.** An `internal` class per model, constructed from the model
bytes, holding the network and its reusable buffers, exposing one method that
takes a frame and returns a plain result object. The pipeline owns them; nothing
above the library sees OpenCV types.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/PalmDetector.cs
internal PalmDetector(byte[] modelBytes)
{
    _net = Cv2.Dnn.ReadNetFromTFLite(modelBytes);
}

// ...

//Letterbox the frame into the model's square input
float scale = (float)InputSize / Math.Max(bgrFrame.Width, bgrFrame.Height);
// ...
_letterboxed.SetTo(Scalar.All(0));
Cv2.Resize(bgrFrame, _resized, new Size(scaledW, scaledH));
using (var window = new Mat(_letterboxed, new Rect(padX, padY, scaledW, scaledH)))
{
    _resized.CopyTo(window);
}

using Mat blob = Cv2.Dnn.BlobFromImage(_letterboxed, 1.0 / 255,
    new Size(InputSize, InputSize), new Scalar(0, 0, 0), swapRB: true, crop: false);
_net.SetInput(blob);

//Identity_1 = per-anchor score logits; Identity = per-anchor box+keypoint offsets.
//Read them with separate single-name forwards (not ForwardAll) so the no-hand case
//  below can early-out before ever reading the far larger box tensor.
float[] rawScores;
using (Mat scores = _net.Forward("Identity_1"))
{
    rawScores = scores.ToArray<float>();
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/HandLandmarker.cs
//Identity = 21 x (x, y, z) in crop pixels; Identity_1 = presence probability. Both
//  outputs are always needed, so read them in one pass with ForwardAll (the second
//  read reuses the first forward's results).
float[] rawLandmarks;
float presence;
Mat[] outputs = _net.ForwardAll("Identity", "Identity_1");
try
{
    rawLandmarks = outputs[0].ToArray<float>();
    presence = outputs[1].ToArray<float>()[0];
}
finally
{
    foreach (Mat output in outputs) { output.Dispose(); }
}
```

The anchor grid the model's outputs are relative to is not in the file and has to
be regenerated exactly:

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/Internal/PalmDetector.cs
static PalmDetector()
{
    //Anchor grids: stride 8 -> 24x24 cells x 2 anchors; stride 16 -> 12x12 cells x 6
    var anchorsX = new float[2016];
    var anchorsY = new float[2016];
    var index = 0;
    foreach (var (gridSize, perCell) in new[] { (24, 2), (12, 6) })
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                for (int n = 0; n < perCell; n++)
                {
                    anchorsX[index] = (x + 0.5f) / gridSize;
                    anchorsY[index] = (y + 0.5f) / gridSize;
                    index++;
                }
            }
        }
    }
    AnchorsX = anchorsX;
    AnchorsY = anchorsY;
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs
if (_bgraMat == null || _bgraMat.Width != width || _bgraMat.Height != height)
{
    _bgraMat?.Dispose();
    _bgraMat = new Mat(height, width, MatType.CV_8UC4);
    _bgrMat?.Dispose();
    _bgrMat = new Mat();
}
Marshal.Copy(bgraPixels, 0, _bgraMat.Data, width * height * 4);
Cv2.CvtColor(_bgraMat, _bgrMat, ColorConversionCodes.BGRA2BGR);
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/PalmDetector.cs` and
`Internal/HandLandmarker.cs`
`WebcamPainter/src/libs/WebcamPainter.Vision/Internal/PalmDetector.cs`

**Sharp edges.**
- Reading the network from a byte array is what makes the embedded-resource
  approach work with no temp file.
- Output tensors are addressed by name, and the code records a deliberate choice
  between the two read styles: separate single-output calls when an early-out can
  avoid reading a large tensor at all, and the read-all call when every output is
  needed, because the second read reuses the first forward's results.
- Every result matrix is disposed - `using` for the single reads, a `finally` loop
  for the read-all.
- Frames arrive in one channel order from the camera and the models want another:
  convert once per frame with cached matrices rather than allocating.
- Preprocessing has to match the model exactly: letterbox into the square input,
  fill with zeros, center the scaled frame, then build the blob with the model's
  own scaling and channel-swap settings.
- Decoding the raw output is the application's job, not the binding's: regenerate
  the fixed anchor grid, apply a sigmoid to score logits (but not to an output
  that is already a probability), run your own suppression on overlapping boxes,
  and convert survivors back out of letterboxed space into original frame pixels.
  Doing that arithmetic in small internal static methods is what makes it
  unit-testable without a model.
- Run all of this on a worker thread with a latest-frame-wins buffer; see the
  view-model area.

### Warp a rotated region of interest into a model input

**When you want this.** A detector gave you a rotated box and the second-stage
model wants an upright square crop.

**The MVVM shape.** A second internal class taking model bytes, with one inference
method that returns landmarks already projected back into original frame pixels,
so callers never see crop-space coordinates.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/Internal/HandLandmarker.cs
float cos = (float)Math.Cos(roi.RotationRadians);
float sin = (float)Math.Sin(roi.RotationRadians);
float half = roi.RoiSize / 2f;

Point2f Corner(float offsetX, float offsetY) => new Point2f(
    roi.RoiCenterX + ((offsetX * cos) - (offsetY * sin)),
    roi.RoiCenterY + ((offsetX * sin) + (offsetY * cos)));

Point2f[] source = { Corner(-half, -half), Corner(half, -half), Corner(-half, half) };
Point2f[] destination =
{
    new Point2f(0, 0),
    new Point2f(InputSize, 0),
    new Point2f(0, InputSize),
};

using (Mat affine = Cv2.GetAffineTransform(source, destination))
{
    Cv2.WarpAffine(bgrFrame, _crop, affine, new Size(InputSize, InputSize));
}

using Mat blob = Cv2.Dnn.BlobFromImage(_crop, 1.0 / 255,
    new Size(InputSize, InputSize), new Scalar(0, 0, 0), swapRB: true, crop: false);
_net.SetInput(blob);

//Identity = 21 x (x, y, z) in crop pixels; Identity_1 = presence probability. Both
//  outputs are always needed, so read them in one pass with ForwardAll (the second
//  read reuses the first forward's results).
Mat[] outputs = _net.ForwardAll("Identity", "Identity_1");
try
{
    rawLandmarks = outputs[0].ToArray<float>();
    presence = outputs[1].ToArray<float>()[0];
}
finally
{
    foreach (Mat output in outputs) { output.Dispose(); }
}

//Project crop-space landmarks back into frame pixels through the same rotation
var imageLandmarks = new Point2f[21];
for (int i = 0; i < 21; i++)
{
    float normX = (rawLandmarks[i * 3] / InputSize) - 0.5f;
    float normY = (rawLandmarks[(i * 3) + 1] / InputSize) - 0.5f;
    imageLandmarks[i] = new Point2f(
        roi.RoiCenterX + (((normX * cos) - (normY * sin)) * roi.RoiSize),
        roi.RoiCenterY + (((normX * sin) + (normY * cos)) * roi.RoiSize));
}
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Vision/Internal/HandLandmarker.cs`
`WebcamPainter/src/libs/WebcamPainter.Vision/Internal/PalmDetector.cs`

**Sharp edges.**
- The presence output is already a probability; the field's documentation says
  "do not sigmoid". Applying one a second time is a real trap when the sibling
  model's scores do need one.
- The affine transform needs exactly three corners, and the same rotation must be
  reused in reverse to project results back - do not recompute it from the matrix.
- Landmarks come back as triples in crop pixels; only two of the three are used,
  hence the stride.
- The array returned by the read-all call is owned by the caller: dispose every
  element in a `finally`.
- The detector's own rectangle transformation - shift half a box along the rotated
  axis, expand by a fixed factor, then undo the letterbox padding and scale -
  belongs with the detector, so this class receives a region already in frame
  pixels.

### Recognize a gesture from landmark geometry instead of a model

**When you want this.** The classification model in your bundle will not import,
or you want a fast, explainable, testable rule instead of a black box.

**The MVVM shape.** A pure `internal static` class over an array of points. No
network, no state, no allocation, trivially unit tested.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/OpenPalmClassifier.cs
/// <summary>
/// Decides geometrically whether 21 hand landmarks show an open palm - the gesture that
/// draws the visualization toward the hand. The bundled MediaPipe gesture-classifier models
/// cannot run through OpenCV's TFLite importer, but they are not needed: an open palm is
/// simply a hand whose four fingers are extended, and extension falls straight out of the
/// landmark geometry (each fingertip is farther from the wrist than that finger's middle
/// joint). A curled finger folds back toward the wrist, so its tip/joint ratio drops below 1.
/// </summary>
internal static class OpenPalmClassifier
{
    /// <summary>
    /// How much farther from the wrist a fingertip must be than its PIP joint (as a ratio)
    /// to count as extended. Raise toward 1.3 to demand flatter hands; lower toward 1.0 to
    /// accept slightly cupped hands.
    /// </summary>
    internal const float ExtendedRatio = 1.1f;

    //MediaPipe hand-landmark topology: 0 = wrist; each finger runs MCP -> PIP -> DIP -> TIP
    //  (index 5-8, middle 9-12, ring 13-16, pinky 17-20; thumb 1-4)
    private static readonly (int Tip, int Pip)[] Fingers = { (8, 6), (12, 10), (16, 14), (20, 18) };

    internal static bool IsOpenPalm(Point2f[] landmarks)
    {
        if (landmarks == null || landmarks.Length < 21) { return false; }

        Point2f wrist = landmarks[0];
        foreach ((int tip, int pip) in Fingers)
        {
            if (Distance(landmarks[tip], wrist) <= Distance(landmarks[pip], wrist) * ExtendedRatio)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// The palm's center: the mean of the wrist and the four finger MCP knuckles.
    /// </summary>
    internal static Point2f GetPalmCenter(Point2f[] landmarks)
    {
        var sumX = 0f;
        var sumY = 0f;
        foreach (int i in new[] { 0, 5, 9, 13, 17 })
        {
            sumX += landmarks[i].X;
            sumY += landmarks[i].Y;
        }
        return new Point2f(sumX / 5f, sumY / 5f);
    }
}
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/OpenPalmClassifier.cs`
`WebcamPainter/src/libs/WebcamPainter.Vision/Internal/OpenPalmClassifier.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Vision.Tests/OpenPalmClassifierTests.cs`

**Sharp edges.**
- The reason is recorded in two places, the class comment and the project file's
  own comment: the classifier stages of the upstream bundle use an operator the
  importer does not support, so those stages are deliberately not embedded.
- The thumb is excluded from the four-finger test; its geometry does not follow
  the same rule.
- The tuning constant is documented with the direction to move it and what that
  trades away.
- The rule is scale- and rotation-free because it compares two distances from the
  same point, so it works in any consistent coordinate space - which is exactly
  what the tests exploit with synthetic inputs.

### Track multiple detections across frames with stable ids

**When you want this.** A per-frame detector gives you unordered results, and
downstream animation needs to know that this frame's item is the same physical
thing as last frame's.

**The MVVM shape.** Keep the track list as worker-thread-only state inside the
pipeline class. Match this frame's candidates against last frame's tracks by
nearest neighbor, closest pairs first, with a maximum distance; smooth each
track's position; report in a stable order.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs
/// <summary>The most palms tracked at once (the palm detector examines the whole frame each time).</summary>
public const int MaxPalms = 4;

/// <summary>The minimum landmark-model presence confidence for a hand to count as present.</summary>
public const float PresenceThreshold = 0.5f;

/// <summary>
/// The exponential-moving-average factor for each palm's position (1 = no smoothing,
/// smaller = smoother but laggier tracking).
/// </summary>
public const float SmoothingAlpha = 0.5f;

/// <summary>
/// How far (normalized, relative to the frame) a palm may move between consecutive
/// frames and still be recognized as the same hand.
/// </summary>
public const float TrackMatchMaxDistance = 0.25f;
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs
//Match this frame's palms to the tracks from the previous frame: greedy
//  nearest-neighbor, closest pairs first, so each physical hand keeps its id
int[] trackForCandidate = MatchCandidatesToTracks(candidates);

var survivingTracks = new List<PalmTrack>(candidates.Count);
var palms = new List<TrackedPalm>(candidates.Count);
for (int c = 0; c < candidates.Count; c++)
{
    var candidate = candidates[c];
    PalmTrack track;
    if (trackForCandidate[c] >= 0)
    {
        track = _tracks[trackForCandidate[c]];
        track.SmoothedX += (candidate.X - track.SmoothedX) * SmoothingAlpha;
        track.SmoothedY += (candidate.Y - track.SmoothedY) * SmoothingAlpha;
    }
    else
    {
        track = new PalmTrack { Id = _nextTrackId++, SmoothedX = candidate.X, SmoothedY = candidate.Y };
    }
    survivingTracks.Add(track);
    palms.Add(new TrackedPalm(track.Id, candidate.IsOpen,
        track.SmoothedX, track.SmoothedY, candidate.DetectionScore, candidate.PresenceScore));
}

//Tracks that matched nothing this frame are dropped (their hands left the view)
_tracks.Clear();
_tracks.AddRange(survivingTracks);

//Report in stable track order so consumers see a consistent sequence
palms.Sort((a, b) => a.TrackId.CompareTo(b.TrackId));
return new PalmTrackingResult(palms);
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Vision/PalmTracker.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Vision.Tests/PalmTrackerTests.cs`

**Sharp edges.**
- The matcher builds every candidate-and-track pair within the distance limit,
  sorts by distance, then assigns greedily, skipping pairs whose candidate or
  track is already taken. That is a few lines and avoids the mis-assignment a
  naive first-match loop produces when two items cross.
- A track that matches nothing this frame is dropped, and an item that leaves and
  returns gets a new id. The result type documents that, and the renderer's slot
  logic is designed around it.
- Results are sorted by id before being reported, so consumers can rely on a
  consistent order.
- Every tuning constant is a documented public constant on the pipeline class
  rather than a literal buried in the loop, with the direction to move it.
- The frame-level early-out matters: when nothing survives the threshold, the
  track list is cleared and a shared empty result is returned, so the
  "everything gone" event still fires and subscribers release their state.

### Smooth a noisy sensor position before it drives the UI

**When you want this.** Raw per-frame positions jitter and the jitter is visible
in whatever they drive.

**The MVVM shape.** The smoothing lives with the producer, not the consumer, so
every consumer gets the same smoothed value and the smoothing state resets
whenever tracking is lost.

**Code.**

```csharp
// From CodeBrix.Samples/WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs
/// <summary>
/// The exponential-moving-average factor for the palm position (1 = no smoothing,
/// smaller = smoother but laggier brush).
/// </summary>
public const float SmoothingAlpha = 0.5f;
// ...
Point2f palmCenter = OpenPalmClassifier.GetPalmCenter(inference.ImageLandmarks);
float normX = Math.Clamp(palmCenter.X / width, 0f, 1f);
float normY = Math.Clamp(palmCenter.Y / height, 0f, 1f);

if (_hasSmoothed)
{
    _smoothedX += (normX - _smoothedX) * SmoothingAlpha;
    _smoothedY += (normY - _smoothedY) * SmoothingAlpha;
}
else
{
    _smoothedX = normX;
    _smoothedY = normY;
    _hasSmoothed = true;
}
```

**Where to look.**
`WebcamPainter/src/libs/WebcamPainter.Vision/HandTracker.cs`

**Sharp edges.**
- The "have we smoothed yet" flag is cleared on every empty result and on stop, so
  the next detection snaps to the true position instead of gliding in from the
  last one.
- Normalization and clamping happen before smoothing, so the smoothed value is
  always a valid normalized coordinate.
- Two thresholds gate the result before smoothing runs: the detector's own score
  threshold and the second-stage model's presence threshold.

## Documents, data and web APIs

### Put the real work in a UI free library behind a service interface

**When you want this.** The application's actual functionality should be testable,
reusable and independent of any of the heads.

**The MVVM shape.** The library targets plain `net10.0`, references no UI
framework, exposes one interface, takes its dependencies through its constructor,
and registers itself through its own `IServiceCollection` extension. The view
model resolves the interface and calls it; nothing else in the application knows
the implementation exists.

**Code.**

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/JustBetweenUs.Encryption.csproj -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <Compile Include="..\Shared\Helpers\EmbeddedResourceHelper.cs" Link="Helpers\EmbeddedResourceHelper.cs" />
</ItemGroup>
<ItemGroup>
  <EmbeddedResource Include="Embedded\DefaultKey.txt" />
</ItemGroup>
<ItemGroup>
  <!-- ... CodeBrix.Cryptography plus Microsoft.Extensions hosting and logging abstractions;
       no CodeBrix.Platform package at all. See the project's csproj. ... -->
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/Services/IEncryptionService.cs
public interface IEncryptionService
{
    bool IsBase64Text(string text);
    Task<string> GetDefaultKey();
    Task<string> OriginalTripleDES_EncryptToBase64(string key, string toEncrypt);
    Task<string> OriginalTripleDES_DecryptFromBase64(string key, string toDecrypt);
    Task<string> AES_EncryptToBase64(string key, string toEncrypt);
    Task<string> AES_DecryptFromBase64(string key, string toDecrypt);
    Task<string> Twofish_EncryptToBase64(string key, string toEncrypt);
    Task<string> Twofish_DecryptFromBase64(string key, string toDecrypt);
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/ViewModels/MainViewModel.cs
_encryptSvc = GetService<IEncryptionService>();
// ...
ProcessedText = await _encryptSvc.AES_EncryptToBase64(EncryptionKey.Trim(), EnteredText);
```

**Where to look.**
`JustBetweenUs/JustBetweenUs.Encryption/JustBetweenUs.Encryption.csproj`
`JustBetweenUs/JustBetweenUs.Encryption/Services/IEncryptionService.cs`
`JustBetweenUs/JustBetweenUs.Encryption/RegisterServices.cs`

**Also shown by.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/`,
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/`,
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/`,
`PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/`

**Sharp edges.**
- The library is referenced by the shared platform library and, separately, by each
  native head. Nothing in it can reference back up.
- Every method returns a task even where the work is synchronous, and the
  implementation wraps the CPU-bound calls in `Task.Run` so the work does not run
  on the UI thread. That is the shape that keeps the view model's commands
  genuinely asynchronous.
- An `[Obsolete]` attribute on a concrete method is invisible to a caller going
  through the interface. If you want callers warned, put the attribute on the
  interface member too.

### Encrypt text with the CodeBrix Cryptography library

**When you want this.** Symmetric encryption inside a service, using algorithms
the base class library does not provide, with the randomness carried alongside the
ciphertext so a single encoded string is all the user has to copy.

**The MVVM shape.** All of it is inside the service implementation. The view model
selects an algorithm from its picker and calls the matching method; it never sees
a cipher, a key derivation or a byte array.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs
using CodeBrix.Cryptography.Crypto;
using CodeBrix.Cryptography.Crypto.Digests;
using CodeBrix.Cryptography.Crypto.Engines;
using CodeBrix.Cryptography.Crypto.Generators;
using CodeBrix.Cryptography.Crypto.Paddings;
using CodeBrix.Cryptography.Crypto.Parameters;

// ...

private const int TwofishSaltLength = 16;

private IBufferedCipher GetTwofishCipher(bool forEncryption, byte[] keyBytes, byte[] saltBytes)
{
    var paramGen = new Pkcs5S2ParametersGenerator(new Sha3Digest());
    paramGen.Init(keyBytes, saltBytes, 1000);

    var engine = new TwofishEngine();
    var cipher = new PaddedBufferedBlockCipher(engine, new Pkcs7Padding());
    cipher.Init(forEncryption,
        parameters: (KeyParameter)paramGen.GenerateDerivedParameters(engine.AlgorithmName, 256));

    return cipher;
}

public async Task<string> Twofish_EncryptToBase64(string key, string toEncrypt)
{
    // ... argument checks that log and return an empty string ...
    var keyBytes = await GetKeyBytes(key);
    var encryptBytes = await Task.Run(() => Encoding.UTF8.GetBytes(toEncrypt));

    var saltBytes = RandomNumberGenerator.GetBytes(TwofishSaltLength);
    var cipher = GetTwofishCipher(true, keyBytes, saltBytes);
    var encrypted = await Task.Run(() => cipher.DoFinal(encryptBytes));

    //Attach our salt bytes to the end of our encrypted byte array - see notes in AES_EncryptToBase64
    encrypted = encrypted.Concat(saltBytes).ToArray();

    result = await Task.Run(() => Convert.ToBase64String(encrypted));
    // ...
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs
//IMPORTANT INFORMATION ABOUT THE INITIALIZATION VECTOR (IV):
//  The IV bytes must be RANDOM, however they don't need to be SECRET;
//  BUT the same bytes also must be used as the IV on the decryption end.
//  So, we generate a random set of bytes; use it as our IV; and
//  then tack it onto the end of our encrypted message; so we can
//  retrieve it when it is time to decrypt the message.

var ivBytes = RandomNumberGenerator.GetBytes(aes.IV.Length);

aes.Key = keyBytes;
aes.IV = ivBytes;

using var encryptor = aes.CreateEncryptor();
var encrypted = await Task.Run(() =>
    encryptor.TransformFinalBlock(encryptBytes, 0, encryptBytes.Length));

encrypted = encrypted.Concat(ivBytes).ToArray();
```

**Where to look.**
`JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs`

**Sharp edges.**
- Both random-material schemes append their bytes to the end of the ciphertext
  before encoding, and both decryption paths first check that the incoming array
  is longer than that material before splitting it. Skipping that check turns a
  malformed paste into an unhelpful exception.
- The Triple DES path has no randomness at all, which is why its output is
  reproducible and why the tests can assert an exact string for it. It is in the
  sample as a deliberately obsolete example.
- The text key is turned into key bytes with a plain hash in the two older paths.
  That is a sample-grade key derivation, not a recommendation; the Twofish path
  shows the better shape with a salted derivation over a modern digest.

### Read an embedded default value at run time

**When you want this.** A default that must ship inside the assembly rather than
as a file on disk, read once and cached.

**The MVVM shape.** The service owns the reading and the caching; the view model
just awaits a task. A helper resolves the manifest resource name from a folder
path and a root namespace so callers do not have to spell the name out.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs
public async Task<string> GetDefaultKey()
{
    var result = _defaultKey;

    if (string.IsNullOrWhiteSpace(result))
    {
        try
        {
            var filename = $"Embedded/{DefaultKeyFilename}";
            _logger.LogInformation("Attempting to read the file: {FilePath}", filename);
            var key = (await EmbeddedResourceHelper.GetResourceAsString(filename,
                typeof(RegisterServices).Namespace)).Trim();

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new DataException($"The embedded file {DefaultKeyFilename} appears to be empty.");
            }

            result = _defaultKey = key.Trim();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error while trying to {nameof(GetDefaultKey)}.");
        }
    }

    return result;
}
```

**Where to look.**
`JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs`
`JustBetweenUs/Shared/Helpers/EmbeddedResourceHelper.cs`
`JustBetweenUs/JustBetweenUs.Encryption/JustBetweenUs.Encryption.csproj`

**Sharp edges.**
- The root namespace is derived from a type in the assembly rather than
  hard-coded, so renaming the project's default namespace does not break the
  lookup.
- The helper converts both slash characters in the path into dots, so the same
  call works whichever separator you write.
- The project file needs both a `None Remove` and an `EmbeddedResource Include`
  for the file; without the removal it is also treated as content.
- The helper offers a line-ending option so a text resource can be normalized,
  which matters for anything compared against a literal in a test.

### Guard Base64 input against invisible clipboard characters

**When you want this.** Users paste your encoded output back into your application
and decoding sometimes fails for no visible reason.

**The MVVM shape.** The guard belongs in the service, applied at every point where
text becomes bytes, so no caller can forget it. The view model asks the service
whether the text is valid and gets a truthful answer.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs
//Strips any character that is not part of the standard Base64 alphabet
//  (A-Z, a-z, 0-9, '+', '/', '='). This guards against invisible junk that
//  can ride along on a copy/paste round-trip - e.g. a stray U+0001 control
//  character that was observed being prepended on the clipboard->TextBox path
//  on Intel/x64 macOS. Such characters are never part of valid Base64, so it
//  is always safe to remove them before decoding.
private static string CleanBase64(string text) =>
    string.IsNullOrEmpty(text)
        ? text ?? string.Empty
        : new string(text.Where(c =>
            (c >= 'A' && c <= 'Z') ||
            (c >= 'a' && c <= 'z') ||
            (c >= '0' && c <= '9') ||
            c == '+' || c == '/' || c == '=').ToArray());

public bool IsBase64Text(string text)
{
    var result = false;

    if (!string.IsNullOrWhiteSpace(text))
    {
        try
        {
            var converted = Convert.FromBase64String(CleanBase64(text));
            result = converted is { Length: > 0 };
        }
        catch (Exception)
        {
            result = false;
        }
    }

    return result;
}
```

**Where to look.**
`JustBetweenUs/JustBetweenUs.Encryption/Services/EncryptionService.cs`
`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs`

**Sharp edges.**
- The cleaner is applied in all three decrypt paths as well as in the validity
  check, so the check and the decode always agree.
- The bug it guards against was head-specific and invisible: a control character
  prepended on the clipboard-to-text-box path on one platform. Anything that
  survives a system clipboard round trip in a multi-head application deserves this
  kind of guard, and a regression test.

### Read a zip archive without extracting it with the CodeBrix Compression library

**When you want this.** Your application browses a container file and should read
individual members on demand rather than unpacking everything.

**The MVVM shape.** A plain class in a UI-free library owns the open archive and
serves byte reads. A service wraps every call in `Task.Run` so the view model
never blocks the UI thread, and the view model owns the archive's lifetime.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/BundleArchive.cs
public class BundleArchive : IDisposable
{
    private readonly ZipFile _zipFile;
    private readonly Dictionary<string, long> _indexesByEntryPath;

    //ZipFile entry streams share the underlying FileStream, so reads are serialized
    private readonly object _gate = new();

    public BundleArchive(string zipPath)
    {
        ZipPath = zipPath ?? throw new ArgumentNullException(nameof(zipPath));
        _zipFile = new ZipFile(zipPath);

        //ZipFile.GetEntry is an O(n) scan per lookup; build a name index once instead
        _indexesByEntryPath = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        var entries = new List<AssetEntry>();
        foreach (ZipEntry entry in _zipFile)
        {
            if (entry.IsDirectory) { continue; }
            _indexesByEntryPath[entry.Name] = entry.ZipFileIndex;
            entries.Add(new AssetEntry(entry.Name, Math.Max(0, entry.Size), AssetClassifier.Classify(entry.Name)));
        }

        Entries = entries.OrderBy(e => e.EntryPath, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public byte[] ReadEntryBytes(string entryPath)
    {
        if (entryPath == null || !_indexesByEntryPath.TryGetValue(entryPath, out var index))
        {
            return null;
        }

        lock (_gate)
        {
            using var input = _zipFile.GetInputStream(index);
            using var buffer = new MemoryStream();
            input.CopyTo(buffer);
            return buffer.ToArray();
        }
    }

    /// <summary>Closes the underlying zip file.</summary>
    public void Dispose() => _zipFile.Close();
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/BundleArchive.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/AssetFolderCatalog.cs`

**Sharp edges.**
- Looking an entry up by name is a linear scan; build a case-insensitive
  name-to-index dictionary once in the constructor and read by index afterwards.
- Entry streams share the archive's underlying file stream, so every read is taken
  under a lock. Reads can still be issued from worker threads; they simply
  serialize.
- Skip directory entries, and clamp a reported size, because an archive may report
  an unknown one.
- The whole-folder load catches per-file exceptions and turns them into warnings,
  so one corrupt archive does not fail the catalog.

### Resolve a file that another archive entry references by relative path

**When you want this.** A file inside your container refers to a sibling - a model
to its texture, a map to its tileset, a tileset to its image - and you have to
turn that reference into a real member of the archive.

**The MVVM shape.** A pure lookup method on the archive class. Because it returns
a resolved path as well as bytes, a resolved path can anchor the next reference in
a chain.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/BundleArchive.cs
public string ResolveDependencyPath(string baseEntryPath, string relativeUri)
{
    if (baseEntryPath == null || string.IsNullOrWhiteSpace(relativeUri)) { return null; }

    var relative = relativeUri.Replace('\\', '/').TrimStart('/');
    var lastSlash = baseEntryPath.Replace('\\', '/').LastIndexOf('/');
    var folder = lastSlash < 0 ? string.Empty : baseEntryPath.Substring(0, lastSlash);

    //Try the reference against the entry's own folder, then each parent up to the root
    while (true)
    {
        var candidate = NormalizePath(folder.Length == 0 ? relative : folder + "/" + relative);
        if (HasEntry(candidate)) { return candidate; }
        if (folder.Length == 0) { break; }

        var parentSlash = folder.LastIndexOf('/');
        folder = parentSlash < 0 ? string.Empty : folder.Substring(0, parentSlash);
    }

    //Last resort: match the bare file name anywhere in the archive
    var fileNameSlash = relative.LastIndexOf('/');
    var fileName = fileNameSlash < 0 ? relative : relative.Substring(fileNameSlash + 1);
    var match = Entries.FirstOrDefault(e =>
        e.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
    return match?.EntryPath;
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
foreach (var tilesetRef in map.Tilesets)
{
    var info = tilesetRef.Inline;
    var basePath = entry.EntryPath;
    if (tilesetRef.Source != null)
    {
        basePath = archive?.ResolveDependencyPath(entry.EntryPath, tilesetRef.Source);
        if (basePath == null) { continue; }
        TiledMapParser.TryParseTileset(archive.ReadEntryText(basePath), out info);
    }
    if (info == null) { continue; }

    var imagePath = archive?.ResolveDependencyPath(basePath, info.ImagePath);
    var imageBytes = imagePath == null ? null : archive.ReadEntryBytes(imagePath);
    if (imageBytes == null) { continue; }

    resolved.Add((tilesetRef.FirstGid, info, LdrImageDecoder.Decode(imageBytes)));
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/BundleArchive.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Normalize separators and collapse relative segments before any lookup, because
  references inside asset files are written by many different tools.
- The bare-file-name fallback is deliberate and last; without it, packs whose
  references are written relative to an export folder that does not exist in the
  archive would fail.
- Dispose intermediate decoded images in a `finally` after compositing; only the
  composited result survives.

### Classify and group the contents of a container for browsing

**When you want this.** A container holds many files that should be presented as
fewer, more meaningful items - one entry per logical asset rather than one per
file.

**The MVVM shape.** The classification and grouping live in the UI-free library
and produce plain model objects. The view model turns them into cells and decides
which raw entries to hide because a grouped card already represents them.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/KenneyBundleReader.cs
//Groups 3D model entries that share a file-name stem into one ModelAsset per model
private static List<ModelAsset> GroupModelAssets(IReadOnlyList<AssetEntry> entries)
{
    var modelEntries = entries.Where(e => e.Kind == AssetKind.Model3D).ToList();
    if (modelEntries.Count == 0) { return []; }

    var previewsByName = entries
        .Where(e => e.Kind == AssetKind.Image &&
            e.EntryPath.StartsWith("Previews/", StringComparison.OrdinalIgnoreCase))
        .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(g => g.Key, g => g.First().EntryPath, StringComparer.OrdinalIgnoreCase);

    // ... materialsByName the same way ...

    return modelEntries
        .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
        .Select(group => new ModelAsset(
            group.Key,
            group.OrderBy(e => FormatRank(e.Extension)).ToList(),
            previewsByName.TryGetValue(group.Key, out var preview) ? preview : null,
            materialsByName.TryGetValue(group.Key, out var material) ? material : null))
        .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
//Sheet images and their XML files are represented by the Spritesheet cards
var atlasPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
foreach (var atlas in bundle.Atlases)
{
    atlasPaths.Add(atlas.ImageEntryPath);
    atlasPaths.Add(atlas.XmlEntryPath);
}

foreach (var entry in bundle.Entries)
{
    //Model files (and their materials) are represented by the grouped Model cards
    if (entry.Kind is AssetKind.Model3D or AssetKind.Material) { continue; }
    // ... hide atlas members and per-model preview renders when browsing "All categories" ...
}
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/KenneyBundleReader.cs`
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.AssetRead/Parsing/AssetClassifier.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- Classification is a single extension-to-kind dictionary with an unknown
  fallback; adding a format is a one-line change and the test is a theory over the
  mapping.
- The grouped-card rule must be applied twice - once when building cells, once
  when building the category filter - or a folder of hidden files shows up as an
  empty category.
- Identity comes from the pack's own license file first, falling back to a
  prettified file name; the title parser rejects a first line that is too long or
  contains a URL, because those are license prose rather than a title.
- Some packs declare a stale image path; the reader falls back to the same-stem
  sibling beside it rather than dropping the asset.

### Build a typed REST client with source generated JSON and its own exceptions

**When you want this.** You want an API library with no UI dependency, trimmable
JSON, a sensible timeout policy, error types callers can catch meaningfully, and
DI-friendly registration with correct client lifetime.

**The MVVM shape.** A standalone library project consumed only through its
interface, with an interface for the client, an interface for a factory, an
options class, and one `IServiceCollection` extension. It has no platform
reference at all, which is what lets its whole test suite run offline. View models
never construct an HTTP client.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.PolyHavenApiClient/PolyHavenServiceCollectionExtensions.cs
public static IServiceCollection AddPolyHavenApiClient(
    this IServiceCollection services,
    Action<PolyHavenClientOptions>? configureOptions = null)
{
    ArgumentNullException.ThrowIfNull(services);

    var options = new PolyHavenClientOptions();
    configureOptions?.Invoke(options);

    services.AddHttpClient(DefaultPolyHavenClientFactory.HttpClientName)
        .ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            AutomaticDecompression = DecompressionMethods.All,
        });

    services.TryAddSingleton<IPolyHavenApiClientFactory>(serviceProvider =>
        new DefaultPolyHavenClientFactory(
            serviceProvider.GetRequiredService<IHttpClientFactory>(), options));

    return services;
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/Serialization/PolyHavenJsonContext.cs
/// <summary>
/// Source-generated JSON serialization context for Poly Haven API response types.
/// The API uses snake_case property names (e.g. <c>date_published</c>).
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(Dictionary<string, PolyHavenAsset>))]
[JsonSerializable(typeof(PolyHavenAsset))]
[JsonSerializable(typeof(PolyHavenAuthor))]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(Dictionary<string, int>))]
internal sealed partial class PolyHavenJsonContext : JsonSerializerContext;
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/DefaultPolyHavenClientFactory.cs
// Metadata calls enforce their own timeout; downloads are governed by the caller's
// CancellationToken, so the HttpClient-level timeout must not cut long downloads short.
httpClient.Timeout = Timeout.InfiniteTimeSpan;
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/RestPolyHavenApiClient.cs
private async Task<string> GetStringAsync(
    string relativeUrl, string resourceDescription, CancellationToken cancellationToken)
{
    ThrowIfDisposed();

    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    if (_options.MetadataRequestTimeout > TimeSpan.Zero)
    {
        timeoutCts.CancelAfter(_options.MetadataRequestTimeout);
    }

    using var response = await _httpClient.GetAsync(
        new Uri(_baseUri, relativeUrl), HttpCompletionOption.ResponseContentRead, timeoutCts.Token)
        .ConfigureAwait(false);
    await EnsureSuccessAsync(response, resourceDescription, timeoutCts.Token).ConfigureAwait(false);
    return await response.Content.ReadAsStringAsync(timeoutCts.Token).ConfigureAwait(false);
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/IPolyHavenApiClient.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.PolyHavenApiClient/RestPolyHavenApiClient.cs`
`PolyHavenBrowser_viewer_only/src/libs/PolyHavenBrowser.PolyHavenApiClient/PolyHavenServiceCollectionExtensions.cs`

**Sharp edges.**
- A single client-wide timeout cannot serve both metadata calls and large
  downloads. Disable it and apply a per-call linked-token timeout to metadata
  requests only.
- The registration is a factory singleton, not a client singleton. Clients are
  cheap and short-lived, and disposing one never tears down the shared connection
  pool. The factory also works standalone with its own pooled handler - which is
  how the tests use it - and in that case it constructs the client without
  disposing the handler.
- Downloads stream with a headers-only completion option and report progress per
  buffer; checksum verification folds into the same read loop.
- A not-found response maps to a distinct exception type so callers can tell
  "missing" from "failed"; unparseable JSON becomes an API exception naming the
  resource; a failed download deletes the partially written file.
- The interface's documentation names the endpoint each method calls, which makes
  the library readable without a separate document.

### Call a REST API behind a service interface the view model resolves

**When you want this.** Your application talks to a REST API with the user's own
credential, and you want the whole conversation behind one interface the view
model can hold.

**The MVVM shape.** The library owns the interface; the implementation creates and
holds the client, and every method the view model needs is one call returning
plain data objects. Reconnecting disposes the previous client. Every method takes
an optional cancellation token.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/INotionDocumentService.cs
public interface INotionDocumentService
{
    /// <summary>Validates the token and returns the bot user's name, or throws.</summary>
    Task<string> ConnectAsync(string integrationToken, CancellationToken cancellationToken = default);

    /// <summary>Loads the root node(s) for a page ID or a database/data-source ID.</summary>
    Task<IList<NotionPageNode>> LoadRootsAsync(string pageOrDatabaseId,
        CancellationToken cancellationToken = default);

    /// <summary>Loads the immediate child pages of a node (called on expand).</summary>
    Task<IList<NotionPageNode>> LoadChildrenAsync(string pageId,
        CancellationToken cancellationToken = default);

    /// <summary>A short, non-scrolling preview for the right-hand pane.</summary>
    Task<NotionPagePreview> LoadPreviewAsync(string pageId,
        CancellationToken cancellationToken = default);

    /// <summary>Renders the selected pages, in the given order, into one book PDF.</summary>
    Task<CreatedDocument> CreateDocumentAsync(CreateRequest request,
        IProgress<CreateProgress> progress = null, CancellationToken cancellationToken = default);
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/NotionDocumentService.cs
public async Task<string> ConnectAsync(
    string integrationToken, CancellationToken cancellationToken = default)
{
    CheckIsDisposed();
    if (string.IsNullOrWhiteSpace(integrationToken))
    {
        throw new ArgumentException("An integration token is required.", nameof(integrationToken));
    }

    //Reconnecting replaces any previous client (the user may paste a new token)
    _client?.Dispose();
    _client = null;
    _treeReader = null;
    _pageReader = null;

    var client = NotionClientFactory.Instance.Create(new ClientOptions
    {
        AuthToken = integrationToken.Trim(),
        //Retries transient 429/5xx responses; the NotionRateGate does the proactive pacing
        RetryPolicy = new DefaultRetryPolicy()
    });

    try
    {
        var user = await _gate.RunAsync(
            () => client.Users.MeAsync(cancellationToken), cancellationToken);

        _client = client;
        _treeReader = new NotionTreeReader(client, _gate);
        _pageReader = new NotionPageReader(client, _gate);

        var botName = string.IsNullOrWhiteSpace(user.Name) ? "Notion integration" : user.Name;
        _logger.LogInformation("Connected to Notion as bot \"{Bot}\".", botName);
        return botName;
    }
    catch
    {
        client.Dispose();
        throw;
    }
}

private NotionTreeReader CheckConnected()
{
    if (_client is null || _treeReader is null || _pageReader is null)
    {
        throw new InvalidOperationException(
            "Not connected to Notion — call ConnectAsync with an integration token first.");
    }
    return _treeReader;
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/INotionDocumentService.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/NotionDocumentService.cs`

**Sharp edges.**
- The connect call is a real round trip, so a bad credential fails immediately
  with a message the view model can show, instead of failing later inside a long
  build.
- If the validation call throws, the half-built client is disposed before the
  exception propagates, and the service stays in its previous state.
- The service is disposable and disposes both the client and its rate gate;
  `ObjectDisposedException.ThrowIf` guards every public method.
- Retries for transient failures come from the client's own retry policy; the
  application layers proactive pacing on top rather than replacing it.

### Pace outbound API calls with a rate gate

**When you want this.** An API publishes a request-rate limit and your application
can easily exceed it while walking a tree.

**The MVVM shape.** A tiny internal gate object owned by the service; every call
site wraps its API call in it. The view model knows nothing about it.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionRateGate.cs
/// <summary>
/// Serialises Notion API calls and enforces a minimum delay between them, keeping
/// the app inside Notion's published rate limit of roughly three requests per
/// second average. The NotionApi client's resilience layer already retries
/// transient 429s; this gate keeps us from provoking them in the first place.
/// </summary>
internal sealed class NotionRateGate : IDisposable
{
    private static readonly TimeSpan MinimumInterval = TimeSpan.FromMilliseconds(350);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private DateTimeOffset _lastCallCompleted = DateTimeOffset.MinValue;
    private bool _isDisposed;

    public async Task<T> RunAsync<T>(Func<Task<T>> apiCall, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(apiCall);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var sinceLast = DateTimeOffset.UtcNow - _lastCallCompleted;
            if (sinceLast < MinimumInterval)
            {
                await Task.Delay(MinimumInterval - sinceLast, cancellationToken).ConfigureAwait(false);
            }

            return await apiCall().ConfigureAwait(false);
        }
        finally
        {
            _lastCallCompleted = DateTimeOffset.UtcNow;
            _gate.Release();
        }
    }
    // ...
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionRateGate.cs`

**Sharp edges.**
- The semaphore serializes calls as well as spacing them, so concurrent
  expansions cannot burst.
- `ConfigureAwait(false)` throughout: this is library code and must not capture
  the UI context.
- The interval is measured from call completion, not call start.

### Be a polite HTTP client to a public API

**When you want this.** You are downloading many files from someone else's servers
and do not want to be blocked.

**The MVVM shape.** One disposable internal client owning one HTTP client, with an
identifying user agent, a timeout, and a semaphore-guarded minimum gap between
downloads.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/WikipediaClient.cs
//Wikimedia's user-agent policy asks for an identifying UA string with contact info
private const string UserAgent =
    "WikipediaPublisher/1.0 (https://github.com/ellisnet; jeremy@ellisnet.com) CodeBrix.MarkupParse";

private const int MediaDownloadDelayMs = 250;

private readonly HttpClient _httpClient;
private DateTime _lastMediaDownloadUtc = DateTime.MinValue;
private readonly SemaphoreSlim _mediaThrottle = new(1, 1);

public WikipediaClient()
{
    _httpClient = new HttpClient();
    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
    _httpClient.Timeout = TimeSpan.FromSeconds(60);
}

/// <summary>
/// Downloads a media file (rate-limited to be polite to Wikimedia servers).
/// Returns null when the download fails - callers treat missing images as non-fatal.
/// </summary>
public async Task<byte[]> TryDownloadMediaAsync(string url, CancellationToken cancellationToken = default)
{
    // ...
    await _mediaThrottle.WaitAsync(cancellationToken);
    try
    {
        var sinceLast = DateTime.UtcNow - _lastMediaDownloadUtc;
        var wait = TimeSpan.FromMilliseconds(MediaDownloadDelayMs) - sinceLast;
        if (wait > TimeSpan.Zero)
        {
            await Task.Delay(wait, cancellationToken);
        }

        _lastMediaDownloadUtc = DateTime.UtcNow;
        return await _httpClient.GetByteArrayAsync(url, cancellationToken);
    }
    catch (HttpRequestException)
    {
        return null;
    }
    catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        //Request timeout (not user cancellation)
        return null;
    }
    finally
    {
        _mediaThrottle.Release();
    }
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/WikipediaClient.cs`

**Also shown by.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/RegisterServices.cs` (an identifying
user agent set once through the client library's options, because the service asks
consumers to identify themselves)

**Sharp edges.**
- The exception filter is what separates a request timeout from a real user
  cancellation; without it, cancelling would look like a failed download.
- The throttle is enforced with a semaphore plus a timestamp, so concurrent
  callers cannot bypass it.
- The owning service disposes the client, and the client disposes both the HTTP
  client and the semaphore.

### Normalize a user entered ID or URL before calling an API

**When you want this.** Users paste whatever they have: a bare identifier, a
formatted one, or a full URL with query parameters that contain other identifiers.

**The MVVM shape.** A pure static function in the library, unit-tested without a
network, called at the boundary of every service method.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionConvert.cs
/// <summary>
/// Normalises user input into a canonical hyphenated Notion ID. Accepts a bare
/// 32-hex ID, a hyphenated ID, or a full Notion URL (the query string is dropped
/// first, because a URL's ?v=…/?p=… parameters carry other object IDs). Returns
/// the trimmed input unchanged when no ID can be found, letting the API reject it.
/// </summary>
public static string NormalizeId(string input)
{
    if (string.IsNullOrWhiteSpace(input)) { return ""; }

    var value = input.Trim();
    var queryStart = value.IndexOf('?');
    if (queryStart >= 0) { value = value[..queryStart]; }

    var compact = value.Replace("-", "");
    var matches = Regex.Matches(compact, "[0-9a-fA-F]{32}");
    if (matches.Count == 0) { return input.Trim(); }

    var hex = matches[^1].Value.ToLowerInvariant();
    return $"{hex[..8]}-{hex[8..12]}-{hex[12..16]}-{hex[16..20]}-{hex[20..]}";
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionConvert.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/NotionConvertTests.cs`

**Sharp edges.**
- The query string is stripped first, or a view parameter's identifier would win.
- The last match in the path is taken, because a slug precedes the identifier.
- Unrecognizable input is passed through so the API produces the error message,
  rather than the application inventing one.

### Resolve an ID that may be one of several object kinds

**When you want this.** One input box has to accept several kinds of object, and
the API answers "not found" for a retrieve of the wrong kind.

**The MVVM shape.** The reader tries each shape in turn, filtering the exception
by its API error code, and remembers the shape per identifier so that later
"load children" calls use the right retrieval. The final failure is rethrown as a
message the user can act on.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionTreeReader.cs
public async Task<IList<NotionPageNode>> LoadRootsAsync(
    string pageOrDatabaseId, CancellationToken cancellationToken = default)
{
    var id = NotionConvert.NormalizeId(pageOrDatabaseId);

    try
    {
        var page = await _gate.RunAsync(() => _client.Pages.RetrieveAsync(id, cancellationToken), cancellationToken);
        return [BuildPageNode(page, depth: 0, parentId: null)];
    }
    catch (NotionApiException ex) when (IsWrongKind(ex)) { }

    try
    {
        var database = await _gate.RunAsync(() => _client.Databases.RetrieveAsync(id, cancellationToken), cancellationToken);
        return [BuildDatabaseNode(database, depth: 0, parentId: null)];
    }
    catch (NotionApiException ex) when (IsWrongKind(ex)) { }

    try
    {
        var dataSource = await _gate.RunAsync(() => _client.DataSources.RetrieveAsync(
            new RetrieveDataSourceRequest { DataSourceId = id }, cancellationToken), cancellationToken);
        return [BuildDataSourceNode(dataSource, depth: 0, parentId: null)];
    }
    catch (NotionApiException ex) when (IsWrongKind(ex))
    {
        throw new InvalidOperationException(
            "No page or database with that ID is visible to this integration. " +
            "Check the ID, and make sure the page is shared with the integration in Notion.", ex);
    }
}

//Notion answers a retrieve of the wrong kind (or of an unshared/absent object)
//  with object_not_found, and a malformed ID with validation_error
private static bool IsWrongKind(NotionApiException ex) =>
    ex.NotionAPIErrorCode == NotionAPIErrorCode.ObjectNotFound
    || ex.NotionAPIErrorCode == NotionAPIErrorCode.ValidationError;
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionTreeReader.cs
//A database's children come from querying its data sources rather than from
//  block children, so the reader tracks which retrieval shape each known ID needs.
private enum SourceShape { Page, Database, DataSource }

private sealed record NodeMeta(SourceShape Shape, int Depth, string Title, string ParentId);

public async Task<IList<NotionPageNode>> LoadChildrenAsync(
    string id, CancellationToken cancellationToken = default)
{
    var normalized = NotionConvert.NormalizeId(id);
    var meta = _metaById.TryGetValue(normalized, out var known)
        ? known
        : new NodeMeta(SourceShape.Page, 0, "", null);
    var childDepth = meta.Depth + 1;

    return meta.Shape switch
    {
        SourceShape.Database => await LoadDatabaseChildrenAsync(normalized, childDepth, cancellationToken),
        SourceShape.DataSource => await LoadDataSourceChildrenAsync(normalized, childDepth, cancellationToken),
        _ => await LoadPageChildrenAsync(normalized, childDepth, cancellationToken)
    };
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionTreeReader.cs`

**Sharp edges.**
- The exception filter is on the API's error code, not on the message.
- Failing to retrieve a child's full record is not fatal: the reader falls back to
  the title carried on the reference itself and loses only decoration.
- Paging uses the API's own has-more and cursor fields in a do-while, and the
  metadata map is a concurrent dictionary with a case-insensitive comparer.

### Read a nested tree from an API with a cycle guard

**When you want this.** An API returns children one level per request, some node
types must not be recursed into, and some can point back at each other.

**The MVVM shape.** A recursive reader in the library that returns a plain node
tree; a set of visited identifiers guards against loops.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionPageReader.cs
private async Task<IReadOnlyList<NotionBlockNode>> ReadChildrenAsync(
    string blockId, HashSet<string> visited, CancellationToken cancellationToken)
{
    if (!visited.Add(blockId)) { return []; } //Cycle guard (synced blocks can loop)

    var blocks = await _gate.RunAsync(
        () => _client.RetrieveAllChildrenAsync(blockId, cancellationToken), cancellationToken);

    var nodes = new List<NotionBlockNode>(blocks.Count);
    foreach (var block in blocks)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<NotionBlockNode> children = [];
        if (ShouldRecurse(block))
        {
            children = await ReadChildrenAsync(block.Id, visited, cancellationToken);
        }
        else if (block is SyncedBlockBlock synced
            && !string.IsNullOrEmpty(synced.SyncedBlock?.SyncedFrom?.BlockId))
        {
            //A duplicate synced block mirrors its source — fetch the source's
            //  children (the visited set guards against reference cycles)
            children = await ReadChildrenAsync(
                synced.SyncedBlock.SyncedFrom.BlockId, visited, cancellationToken);
        }
        nodes.Add(new NotionBlockNode { Block = block, Children = children });
    }
    return nodes;
}

private static bool ShouldRecurse(IBlock block) =>
    block.HasChildren && block is not ChildPageBlock && block is not ChildDatabaseBlock;
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/NotionPageReader.cs`

**Sharp edges.**
- Node kinds that are separate documents in their own right are never recursed
  into; they are references, not inline content.
- A preview that must stay cheap reads one batch of children only, and its comment
  records the consequence: for a longer page the child count reads as "at least".

### Batch a metadata API and treat the result as best effort

**When you want this.** You need per-item metadata for many items and one failure
must not fail the job.

**The MVVM shape.** An internal resolver asks only for items that will actually be
used, de-duplicates, batches, and writes results back onto the model. Failures
leave the field empty.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/WikipediaClient.cs
/// <summary>
/// Looks up Wikimedia "extmetadata" (author, credit, license, ...) for the given "File:" page
/// titles via the MediaWiki imageinfo API, batching up to 50 titles per request. The local
/// wiki's API transparently resolves files hosted on Wikimedia Commons. Titles that cannot be
/// resolved are simply absent from the result; a failed request yields an empty dictionary
/// (attribution is best-effort and never fails the render).
/// </summary>
public async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>>
    GetImageMetadataAsync(
        IReadOnlyCollection<string> fileTitles,
        string wikiHost = DefaultWikiHost,
        CancellationToken cancellationToken = default)
{
    // ...
    for (var offset = 0; offset < uniqueTitles.Count; offset += MaxTitlesPerMetadataQuery)
    {
        // ... build apiUrl with the batch's titles ...
        try
        {
            var json = await _httpClient.GetStringAsync(apiUrl, cancellationToken);
            ParseImageMetadata(json, result);
        }
        catch (HttpRequestException) { /* best-effort; skip this batch */ }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested) { }
    }

    return result;
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/AttributionResolver.cs
//Only fetch for images that will actually appear (downloaded) and that we can identify
var placed = images
    .Where(i => i.ProcessedBytes is { Length: > 0 }
                && !string.IsNullOrWhiteSpace(i.MediaPageTitle))
    .ToList();
if (placed.Count == 0) { return; }
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/WikipediaClient.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/AttributionResolver.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/AttributionFormatter.cs`

**Sharp edges.**
- The result dictionary is keyed case-insensitively, because the API normalizes
  titles.
- The formatter has to defend against real-world metadata: markup inside field
  values, entity encoding, placeholder names, a phrase duplicated by
  machine-plus-human templates, and codes that mean nothing to a reader. Each of
  those is a named private method with a comment.

### Fetch a whole remote catalog once and cache images behind a concurrency gate

**When you want this.** One endpoint returns everything, images are per item, and
you want to be polite to the server without starving the scroll.

**The MVVM shape.** A service registered in the container and resolved by the view
model. Filtering and sorting are a static, pure method on the service, which makes
them trivially testable and keeps the view model free of query expressions.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/Services/ModelCatalogService.cs
public sealed class ModelCatalogService
{
    //Cell thumbnails are requested at exactly the size the catalog cell displays them.
    private const int ThumbnailWidth = 512;
    private const int ThumbnailHeight = 288;

    private readonly SemaphoreSlim _catalogGate = new(1, 1);

    //At most a handful of thumbnail requests in flight at once - polite to the CDN, and
    //plenty to keep up with scrolling.
    private readonly SemaphoreSlim _thumbnailGate = new(4, 4);
    private readonly ConcurrentDictionary<string, byte[]> _thumbnailCache = new();

    public async Task<IReadOnlyList<PolyHavenAsset>> GetModelsAsync(CancellationToken cancellationToken)
    {
        if (_models != null) { return _models; }

        await _catalogGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_models != null) { return _models; }

            using var client = _factory.GetClient();
            var assets = await client.GetAssetsAsync(PolyHavenAssetType.Model, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            _models = assets.Values.ToList();
            return _models;
        }
        finally
        {
            _catalogGate.Release();
        }
    }

    public static IReadOnlyList<PolyHavenAsset> SortAndFilter(
        IReadOnlyList<PolyHavenAsset> models, CatalogSortOrder sortOrder, string searchText)
    { /* ... Where(Matches) then an OrderBy per CatalogSortOrder ... */ }
}
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Services/ModelCatalogService.cs`

**Sharp edges.**
- Double-checked locking around the one-shot fetch: check before waiting on the
  semaphore and again after acquiring it, so a burst of startup callers makes one
  request.
- Every service await uses `ConfigureAwait(false)`; only the view-model layer
  needs the dispatcher context back.
- Thumbnails are requested at exactly the cell's display size, so the server does
  the downscale.

### Report true byte progress across a multi file download with side car files

**When you want this.** One logical download is several files - a main file and
the companions it references by relative path - and a per-file bar would jump back
to zero repeatedly.

**The MVVM shape.** The service takes an `IProgress<double>` and computes the
fraction across all files from sizes the API advertises up front. The view model
converts the fraction into a bound percentage inside a `Progress<double>`
callback, which marshals back to the UI context for you.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/Services/ModelDownloadService.cs
//Every file advertises its size, so the bar can show true byte progress across the
//main .gltf and all its sidecars (buffer .bin + texture images).
var totalBytes = Math.Max(1L, gltf.Size + (gltf.Include?.Values.Sum(f => f.Size) ?? 0L));
var completedBytes = 0L;

Directory.CreateDirectory(modelFolder);

var gltfPath = Path.Combine(modelFolder, FileNameFromUrl(gltf.Url));
await DownloadOneAsync(client, gltf, gltfPath, totalBytes, completedBytes, progress, cancellationToken)
    .ConfigureAwait(false);
completedBytes += gltf.Size;

//A glTF references its sidecar files by relative path; the Include dictionary is
//keyed by exactly those relative paths, so the files land where the glTF expects them.
if (gltf.Include != null)
{
    foreach (var (relativePath, sidecar) in gltf.Include)
    {
        var sidecarPath = Path.Combine(modelFolder, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(sidecarPath));
        await DownloadOneAsync(client, sidecar, sidecarPath, totalBytes, completedBytes, progress, cancellationToken)
            .ConfigureAwait(false);
        completedBytes += sidecar.Size;
    }
}

private static async Task DownloadOneAsync(
    IPolyHavenApiClient client, PolyHavenFileRef file, string destinationPath,
    long totalBytes, long completedBytes, IProgress<double> progress, CancellationToken cancellationToken)
{
    var fileProgress = progress == null
        ? null
        : new Progress<PolyHavenDownloadProgress>(p =>
            progress.Report(Math.Min(1d, (completedBytes + p.BytesReceived) / (double)totalBytes)));

    await client.DownloadFileAsync(file, destinationPath, fileProgress, cancellationToken: cancellationToken)
        .ConfigureAwait(false);
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/PolyHavenBrowser.Core/ViewModels/MainViewModel.cs
var progress = new Progress<double>(fraction => DownloadProgress = fraction * 100d);
var downloaded = await _downloads.EnsureDownloadedAsync(
    cell.Asset, _downloadFolder, progress, CancellationToken.None);
```

**Where to look.**
`PolyHavenBrowser/src/PolyHavenBrowser.Core/Services/ModelDownloadService.cs`
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Services/SampleAssetService.cs`

**Sharp edges.**
- Side-car keys are relative paths using forward slashes; translate them to the
  platform separator and create the intermediate directory, or the file lands
  somewhere the main file cannot find it.
- The service checks for an existing download first and reports completion
  immediately, so re-opening an asset costs no network traffic and the bar does
  not flicker.
- The file selection uses layered fallbacks, so an asset with an unexpected tree
  still resolves or fails with a clear message.
- A `double` bound property may need hand-written compare-and-notify; see the
  view-model area.

### Cache downloaded assets with a key you can invalidate

**When you want this.** Your application fetches large files it should download
once, and you want a cache that invalidates correctly when you change which file
you fetch.

**The MVVM shape.** A singleton service registered in the container, taking the
API client factory in its constructor and exposing one async method that reports
progress and honors a cancellation token. The view model resolves it and awaits
it.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Services/SampleAssetService.cs
public SampleAssetService(IPolyHavenApiClientFactory factory)
{
    _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    _cacheRoot = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PolyHavenBrowser", "cache");
}

public async Task<SampleAsset> EnsureSampleAsync(
    SampleAssetKind kind, IProgress<string> status, CancellationToken cancellationToken)
{
    await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
        var kindDir = Path.Combine(_cacheRoot, kind.ToString().ToLowerInvariant());
        var markerPath = Path.Combine(kindDir, "sample.json");
        var slug = SlugFor(kind);

        //Reuse the cached asset only when it is the SAME curated slug. The marker is keyed by
        //kind, so changing the curated slug (SlugFor) must invalidate a stale marker and
        //re-download rather than keep serving the previously-cached asset.
        var cached = TryReadMarker(markerPath);
        if (cached != null && string.Equals(cached.Slug, slug, StringComparison.Ordinal)
            && File.Exists(cached.PrimaryFilePath))
        {
            status?.Report($"{Describe(kind)}: {cached.Name} (cached)");
            return cached;
        }

        using var client = _factory.GetClient();
        // ... download, then:
        var result = new SampleAsset { Slug = slug, Name = name, PrimaryFilePath = primaryPath };
        WriteMarker(markerPath, result);
        status?.Report($"{Describe(kind)}: {name}");
        return result;
    }
    finally
    {
        _gate.Release();
    }
}
```

**Where to look.**
`PolyHavenBrowser_viewer_only/src/PolyHavenBrowser.Core/Services/SampleAssetService.cs`

**Sharp edges.**
- The cache marker records which asset it holds, and that is compared before
  reuse. The marker file is keyed by kind, so without the comparison, changing
  which asset the application curates would keep serving the old one forever.
- The marker also verifies the file still exists before trusting it, and reading a
  marker swallows any exception and returns null, so a corrupt marker just
  re-downloads.
- A semaphore serializes the whole method, so two rapid button presses cannot race
  on the same folder.
- Every await uses `ConfigureAwait(false)` - this is a service, not view-model
  code.

### Parse messy HTML into structured blocks with the CodeBrix MarkupParse library

**When you want this.** You have real-world HTML and need typed content, not a
string.

**The MVVM shape.** Parsing lives in an internal class in the pipeline library,
reached only through the service interface. The result is a plain model with an
ordered block list and no UI or document types in it.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs
public ParsedArticle Parse(string html)
{
    if (string.IsNullOrWhiteSpace(html))
    {
        throw new ArgumentException("Value cannot be null or blank.", nameof(html));
    }

    var parser = new HtmlParser();
    var document = parser.ParseDocument(html);

    _strippedGlyphCount = 0;
    var article = new ParsedArticle { SourceUrl = SourceUrl };

    article.Title = GetTitle(document);
    article.ShortDescription = document.QuerySelector("div.shortdescription")?.TextContent?.Trim() ?? "";
    article.LeadImage = GetLeadImage(document);

    //Some articles contain more than one .mw-parser-output element - e.g. a small wrapper
    //  emitted by a transcluded template (coordinates, short description, hatnotes) plus the
    //  real article body. QuerySelector would return whichever comes first in document order,
    //  which can be the near-empty wrapper. Pick the container that actually holds the prose:
    //  the one with the most paragraph descendants.
    var contentRoots = document.QuerySelectorAll(".mw-parser-output").ToList();
    var contentRoot = contentRoots
        .OrderByDescending(e => e.QuerySelectorAll("p").Count())
        .ThenByDescending(e => e.Children.Count())
        .FirstOrDefault();
    // ...
    var state = new WalkState();
    WalkChildren(contentRoot, article, state);
    // ...
    ReportSkips(article, state);
    return article;
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Models/ArticleContent.cs
public enum ArticleBlockType
{
    Heading,
    Paragraph,
    BulletList,
    NumberedList,
    BlockQuote,
    Image,
    Table,
    DefinitionList
}

/// <summary>
/// A single run of text with uniform character formatting.
/// </summary>
public sealed record TextRun(
    string Text,
    bool Bold = false,
    bool Italic = false,
    bool Superscript = false,
    bool Subscript = false);
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Models/ArticleContent.cs`

**Sharp edges.**
- Picking "the first matching container" is a trap on real pages; this parser
  ranks candidates by paragraph count and records a warning when there was more
  than one. There is a regression test for exactly this.
- Warnings are collected on the model rather than thrown, and surface in the
  render result.
- The service treats an empty block list as a hard error with a message the user
  can act on.

### Strip web only chrome while walking the DOM

**When you want this.** Printed output must not carry citation markers, edit
links, navigation boxes or reference sections.

**The MVVM shape.** A recursive walk with a small state object, a class deny-list
for block elements, an inline skip predicate, and an in-place removal helper.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs
private static readonly HashSet<string> StopSections =
    new(StringComparer.OrdinalIgnoreCase)
    {
        "References", "See also", "External links", "Notes", "Further reading",
        "Bibliography", "Sources", "Citations", "Footnotes", "Works cited",
        "Explanatory notes", "General references"
    };

//Divs with any of these classes are web chrome and never book content
private static readonly string[] SkippedDivClasses =
[
    "hatnote", "navbox", "vertical-navbox", "toc", "reflist", "refbegin",
    "sistersitebox", "side-box", "ambox", "mbox-small", "asbox", "metadata",
    "printfooter", "mw-empty-elt", "noprint", "mw-authority-control", "portalbox",
    "portal-bar", "navigation-not-searchable", "spoken-wikipedia", "sister-bar"
];
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs
/// <summary>Removes reference markers, edit links and other non-content from an element (in place).</summary>
private static void RemoveNonContent(IElement element)
{
    foreach (var node in element
                 .QuerySelectorAll("sup.reference, sup.plainlinks, .mw-editsection, style, script, .noprint, sup[typeof*='mw:Extension/ref']")
                 .ToList())
    {
        node.Remove();
    }
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs`

**Sharp edges.**
- Materialize the query result before removing: you cannot mutate the document
  while enumerating a live query.
- Markup evolves. The walker handles both a bare heading element and a newer
  wrapped form, and recurses through section wrappers.
- Skipped things are counted by kind and reported as warnings rather than silently
  dropped, which makes the parser debuggable against a live site.

### Upgrade thumbnail URLs to print resolution

**When you want this.** The images on a web page are screen-sized and you need
print resolution without upscaling anything.

**The MVVM shape.** A pure internal static method with a clear contract,
unit-tested with a theory and no I/O.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs
/// <summary>
/// Derives a print-resolution rendition URL from a Wikimedia thumbnail URL.
/// Thumbnail URLs look like .../thumb/6/66/Name.jpg/250px-Name.jpg - the pixel
/// prefix of the final segment selects the rendition size.
/// </summary>
internal static string DerivePrintUrl(string src, int fileWidth, string urlPath)
{
    if (!src.Contains("/thumb/")) { return src; }
    // ...
    var currentWidth = int.Parse(pxMatch.Groups[1].Value);

    //SVG renditions (....svg/NNNpx-....svg.png) can be rasterized at any size;
    //  raster files cannot be upscaled beyond their true file width.
    var isSvgRendition = urlPath.EndsWith(".svg.png", StringComparison.OrdinalIgnoreCase);
    var target = isSvgRendition
        ? TargetImagePixelWidth
        : (fileWidth > 0 ? Math.Min(TargetImagePixelWidth, fileWidth) : TargetImagePixelWidth);

    if (target <= currentWidth) { return src; }

    var newSegment = $"{target}px-{lastSegment[(pxMatch.Length)..]}";
    return src[..(lastSlash + 1)] + newSegment;
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ImagePipeline.cs
private async Task<bool> TryPrepareOneAsync(ArticleImage image, CancellationToken cancellationToken)
{
    var bytes = await _client.TryDownloadMediaAsync(image.PrintUrl, cancellationToken);

    //High-resolution rendition may 404 (e.g. odd file types) - fall back to the page thumbnail
    if (bytes is null && (!image.PrintUrl.Equals(image.ThumbUrl, StringComparison.Ordinal)))
    {
        bytes = await _client.TryDownloadMediaAsync(image.ThumbUrl, cancellationToken);
    }
    // ...
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ImagePipeline.cs`

**Sharp edges.**
- Vector sources rasterize at any size; raster sources must be clamped to their
  true file width, read from the markup's own attribute.
- Non-thumbnail URLs are returned unchanged, so the caller must not assume a
  rewrite happened, and the higher-resolution rendition may not exist - fall back
  to the original.
- Icons are filtered out earlier by a minimum pixel width, and unsupported file
  extensions are rejected before a download is attempted.
- Normalizing the downloaded bytes for print is a separate step; see the
  graphics area.

### Run a multi stage pipeline behind one service method

**When you want this.** You want the view model to call one method and get a
result object, while the stages stay separate and testable.

**The MVVM shape.** The service validates the request, runs the stages in order,
reports progress between them, honors a cancellation token, wraps the CPU-bound
stages in `Task.Run` because the caller is the UI thread, writes the file, and
returns a result record with everything the UI wants to display.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Services/ArticleRenderService.cs
// 1. Fetch
progress?.Report(new RenderProgress(RenderStage.FetchingArticle, "Fetching the article...", 5));
var html = await _client.GetArticleHtmlAsync(request.ArticleUrl, cancellationToken);

// 2. Parse
progress?.Report(new RenderProgress(RenderStage.ParsingArticle, "Reading the article...", 12));
var parser = new ArticleParser(request.ArticleUrl);
var article = parser.Parse(html);

if (article.Blocks.Count == 0)
{
    throw new InvalidOperationException(
        "No readable article content was found at the given URL. " +
        "Make sure the URL points at a Wikipedia article page.");
}

// 3. Images
// ... ImagePipeline.PrepareImagesAsync, then AttributionResolver.ResolveAsync ...

// 4. Compose
cancellationToken.ThrowIfCancellationRequested();
progress?.Report(new RenderProgress(RenderStage.ComposingBook, "Laying out the book...", 74));
var theme = BookTheme.For(request.PageSize);
var composer = new BookComposer(article, theme, DateTime.Now);
var document = composer.Compose();

// 5. Render + save
progress?.Report(new RenderProgress(RenderStage.SavingPdf, "Rendering the PDF...", 82));
var renderer = new PdfDocumentRenderer(unicode: true) { Document = document };
renderer.RenderDocument();

// ... resolve outputPath, create the folder ...
renderer.PdfDocument.Save(outputPath);
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/NotionDocumentService.cs
// 3 + 4. Compose and render off the caller's thread (both are CPU-bound,
//    and the caller is typically the UI thread)
progress?.Report(new CreateProgress(CreateStage.ComposingBook, "Laying out the book…", 72));
var composer = new BookComposer(chapters, context);
var outputPath = request.OutputFilePath.Trim();

var pageCount = await Task.Run(() =>
{
    var document = composer.Compose();

    progress?.Report(new CreateProgress(CreateStage.SavingPdf, "Rendering the PDF…", 82));
    var renderer = new PdfDocumentRenderer(unicode: true) { Document = document };
    renderer.RenderDocument();

    var folder = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(folder)) { Directory.CreateDirectory(folder); }
    renderer.PdfDocument.Save(outputPath);
    return renderer.PdfDocument.PageCount;
}, cancellationToken);
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Models/DocumentModels.cs
public sealed class CreateRequest
{
    public IReadOnlyList<string> PageIds { get; init; } = [];
    public string OutputFilePath { get; init; } = "";
    public PageSizeOption PageSize { get; init; } = PageSizeOption.EightByTen;

    /// <summary>When false, images are skipped entirely (text-only rendering).</summary>
    public bool IncludeImages { get; init; } = true;

    /// <summary>
    /// When false, non-image media (video poster frames, audio/file cards' downloads)
    /// are not fetched; those blocks render as cards from metadata alone.
    /// </summary>
    public bool IncludeMedia { get; init; } = true;
}

public sealed class CreatedDocument
{
    public string OutputFilePath { get; init; } = "";
    public string Title { get; init; } = "";
    public int PageCount { get; init; }
    public int ChapterCount { get; init; }
    public int ImageCount { get; init; }
    public TimeSpan Elapsed { get; init; }

    /// <summary>Non-fatal notes collected during the creation.</summary>
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Services/ArticleRenderService.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Services/NotionDocumentService.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Models/DocumentModels.cs`

**Sharp edges.**
- Validate the request up front, so a failure surfaces before any network traffic.
- The document renderer is created with its Unicode option on, which is what
  embedded fonts need for non-ASCII text.
- Where two ways of naming the output exist - a full path, or a directory plus an
  optional name - the request record documents the precedence.
- The result carries a warning list; collect non-fatal problems as you go and show
  them all at the end.
- Progress percentages are fixed per stage, with a long stage interpolating across
  its own range, so the bar never goes backwards.
- `ObjectDisposedException.ThrowIf` guards every public method; the service owns
  its HTTP client and is disposable.

### Register embedded OFL fonts with the PDF font system

**When you want this.** Your generated documents must look identical everywhere,
regardless of what fonts the host has installed.

**The MVVM shape.** A static, lock-guarded, idempotent registration in the
library, called at the top of composition. The fonts are embedded resources of the
same assembly, with their license texts embedded beside them.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookFonts.cs
using CodeBrix.Imaging.PixelFormats;
using CodeBrix.PdfDocuments.Drawing;
using CodeBrix.PdfDocuments.Fonts;
using CodeBrix.PdfDocuments.Utils;

internal static class BookFonts
{
    public const string SerifFamily = "EB Garamond";
    public const string SansFamily = "Source Sans 3";
    public const string MonoFamily = "Source Code Pro";
    public const string EmojiFamily = "Noto Emoji";

    private const string ResourcePrefix = "NotionDocumentCreator.CreateDocument.Fonts.";

    private static readonly object _locker = new();
    private static bool _registered;

    public static void EnsureRegistered()
    {
        lock (_locker)
        {
            if (_registered) { return; }

            //The PDF image pipeline needs an imaging implementation before any image can be placed
            ImageSource.ImageSourceImpl ??= new ImagingImageSource<Rgba32>();

            var assembly = typeof(BookFonts).Assembly;

            var serifFaces = new[]
            {
                "EBGaramond-Regular", "EBGaramond-Italic", "EBGaramond-Bold", "EBGaramond-BoldItalic"
            };
            var serifResolver = new EmbeddedFontResolver(
                fontFamilyName: SerifFamily,
                fontFaceResources:
                [
                    new EmbeddedResourceFontFace(FaceName: "EBGaramond-Regular", EmbeddedResourceName: $"{ResourcePrefix}EBGaramond-Regular.ttf"),
                    // ... one EmbeddedResourceFontFace per face ...
                ],
                fontEmbeddedResourceAssembly: assembly);

            // ... sans, mono and emoji resolvers built the same way ...

            //MetaFontResolver routes family-name lookups (ResolveTypeface) via any registered
            //  resolver whose DefaultFontName matches, but face-name lookups (GetFont) require
            //  a registration per face name.
            foreach (var face in serifFaces)
            {
                MetaFontResolver.Instance.RegisterFontResolver(face, serifResolver);
            }
            // ... same loop for the other three families ...

            _registered = true;
        }
    }
}
```

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/PolyHavenBrowser.CreateDocument.csproj -->
<ItemGroup>
  <None Remove="Fonts\*.ttf" />
  <None Remove="Fonts\OFL-*.txt" />
</ItemGroup>

<ItemGroup>
  <EmbeddedResource Include="Fonts\*.ttf" />
  <EmbeddedResource Include="Fonts\OFL-*.txt" />
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Internal/SheetFonts.cs
        //The face is deliberately named "Roboto-Heavy" (not "-ExtraBold"): the resolver
        //  matches faces by looking for "bold"/"italic" in their names, so a non-bold
        //  request for this single-face family must not see "bold" in the face name.
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookFonts.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Internal/SheetFonts.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookFonts.cs`

**Sharp edges.**
- The same method sets the PDF layer's imaging back-end. Forget it and image
  placement fails, not the font lookup.
- One registration call per face name is required, not one per family; the comment
  in the code explains the difference between family-name and face-name lookups.
- The resolver matches faces by looking for weight and style words in their names,
  so a heavy single-face family must be named without "bold" in it, while a
  semibold face intended to serve bold requests should contain it. Weights beyond
  regular and bold need their own family name.
- The resource prefix is the library's default root namespace, which is why these
  libraries deliberately do not override it.
- Registration is idempotent under a lock and is called from more than one entry
  point, so a test that exercises only the renderer still gets fonts, and parallel
  renders in one process are safe.
- Embed the license text alongside the font files so it travels with the binary.

### Drop characters your embedded fonts cannot render

**When you want this.** Text from an arbitrary source may contain characters your
embedded fonts have no glyph for, and empty boxes would ruin a printed page.

**The MVVM shape.** A filter applied where text enters the document, counting
removals into a warning the result carries. Two workable forms appear in this
repository: a static range filter, and a coverage cache that parses each embedded
font's character map once and asks it per codepoint.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/GlyphFilter.cs
/// <summary>
/// Filters article text down to the character ranges covered by the embedded book
/// fonts (EB Garamond and Source Sans 3: Latin, Latin Extended, Greek, Cyrillic and
/// common punctuation). Characters outside those ranges - for example Cuneiform,
/// CJK or Arabic glyphs quoted inline in an article - would otherwise render as
/// "tofu" boxes in the PDF, which ruins a printed page.
/// </summary>
internal static class GlyphFilter
{
    public static string Sanitize(string text, out int removedCount)
    {
        // ... walks the string, keeping supported characters ...
        //Tidy the holes left behind: empty bracket pairs, doubled spaces,
        //  stray space before closing punctuation
        cleaned = Regex.Replace(cleaned, @"[\(\[«“‘]\s*[\)\]»”’]", "");
        cleaned = Regex.Replace(cleaned, @"\s+([,;.:!?)\]])", "$1");
        cleaned = Regex.Replace(cleaned, @"[ \t]{2,}", " ");

        return cleaned;
    }

    private static bool IsSupported(char c) => (int)c switch
    {
        0x0009 or 0x000A or 0x000D => true,   //Tab, newline
        >= 0x0020 and <= 0x007E => true,      //ASCII
        >= 0x00A0 and <= 0x024F => true,      //Latin-1, Latin Extended A/B
        // ... Greek, Cyrillic, Latin Extended Additional, punctuation, currency, number forms ...
        _ => false
    };
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/FontCoverage.cs
/// <summary>Whether the given embedded font file has a glyph for the codepoint.</summary>
public static bool Covers(string fontFileName, int codepoint)
{
    var coverage = _coverageByFile.GetOrAdd(fontFileName, LoadCoverage);
    return coverage.Contains(codepoint);
}

/// <summary>
/// Whether an emoji codepoint can actually be PRINTED: it must be in the
/// emoji face's cmap AND inside the Basic Multilingual Plane — the PDF text
/// engine addresses glyphs per UTF-16 code unit, so astral-plane emoji
/// (U+1F300 and friends) would print as tofu even though the font has them.
/// </summary>
public static bool EmojiPrintable(int codepoint) =>
    codepoint <= 0xFFFF && Covers(EmojiRegular, codepoint);
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs
//Variation selectors and the zero-width joiner shape emoji sequences the
//  monochrome face cannot compose anyway — drop them without counting
if (codepoint is 0xFE0E or 0xFE0F or 0x200D)
{
    i += charCount - 1;
    continue;
}

bool isEmoji;
if (codepoint is '\n' or '\r' or '\t' || FontCoverage.Covers(FontCoverage.SerifRegular, codepoint))
{
    isEmoji = false;
    if (codepoint == '\r') { i += charCount - 1; continue; } //Normalise CRLF to the \n that follows
}
else if (FontCoverage.EmojiPrintable(codepoint))
{
    isEmoji = true;
}
else
{
    DroppedCharacterCount++;
    i += charCount - 1;
    continue;
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/FontCoverage.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/GlyphFilter.cs`

**Sharp edges.**
- Astral-plane characters are dropped even when the font has the glyph, because
  the text engine addresses glyphs per code unit.
- Skip surrogate pairs in both halves, or you leave an orphaned half behind.
- Removing characters leaves empty brackets and doubled spaces; the cleanup passes
  matter as much as the filter.
- An unparseable font should report empty coverage rather than throwing, so
  callers fall back safely.
- Where a glyph is needed and no family has it, print a text stand-in rather than
  nothing.
- The count of removed characters becomes a warning in the result, so the user is
  told rather than silently shortchanged.

### Derive a whole document theme from one page size choice

**When you want this.** Your document must be laid out proportionately at several
page sizes rather than tuned at one.

**The MVVM shape.** An immutable theme object built by a factory from the chosen
trim; every style in the document derives its metrics from the theme, so the
picker in the view model changes one enum value and the whole document
re-proportions.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookTheme.cs
public static BookTheme For(PageSizeOption option)
{
    var page = PageSizeInfo.For(option);

    //Every metric derives from the trim size, so each of the four trims (and any
    //  added later) gets a proportionate book layout instead of one tuned at a
    //  single size: margins as fractions of the page, the inner (binding) margin
    //  a little larger than the outer, and type scaled to the resulting measure
    var innerMargin = page.WidthPoints * 0.125;
    var outerMargin = page.WidthPoints * 0.106;
    var topMargin = page.HeightPoints * 0.09;
    var bottomMargin = page.HeightPoints * 0.113;
    var textWidth = page.WidthPoints - innerMargin - outerMargin;

    //Cap the measure at the classic book line length (~65-75 characters) — on
    //  wide trims (US Letter, A4) the excess goes into the side margins instead
    const double maxMeasure = 435;
    if (textWidth > maxMeasure)
    {
        var extra = (textWidth - maxMeasure) / 2;
        innerMargin += extra;
        outerMargin += extra;
        textWidth = maxMeasure;
    }

    var bodySize = System.Math.Clamp(8.0 + textWidth * 0.0058, 9.0, 11.5);
    // ... construct the theme ...
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookTheme.cs
// Palette (warm ink on paper with an oxblood accent)
public static readonly Color Ink = new(31, 30, 28);
public static readonly Color Accent = new(122, 44, 38);
public static readonly Color Muted = new(112, 108, 102);
public static readonly Color Hairline = new(203, 197, 189);

/// <summary>The width of the text block (page width minus side margins).</summary>
public double TextWidth => PageWidth - InnerMargin - OuterMargin;

/// <summary>Body text size in points; every other size in the scale derives from this.</summary>
public double BodySize { get; private init; }

/// <summary>Body leading (line spacing) in points.</summary>
public double Leading => BodySize * 1.47;

public double H1Size => BodySize * 1.62;
public double H2Size => BodySize * 1.27;
public double CaptionSize => BodySize * 0.81;
public double RaisedCapSize => BodySize * 2.35;

public static BookTheme For(PageSizeOption option) => /* one record per trim size */;
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookTheme.cs`
and `Internal/BookStyles.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookTheme.cs`

**Sharp edges.**
- Every dimension is in typographic points, matching the document model's own unit
  type.
- The theme is the right home for a letterspacing helper, and its comment records
  a layout rule worth knowing: the layout engine collapses runs of ordinary
  spaces, so the helper uses non-breaking spaces; a second variant keeps word gaps
  breakable so a long display line can still wrap.
- A picker bound to display-name strings needs the view model to map the name back
  to the enum with a fallback.

### Compose a book with sections styles running heads and folios

**When you want this.** You need real book structure - a cover section, a body
section with mirrored margins, running heads and page numbers - not a flat report.

**The MVVM shape.** A composer class takes the parsed model plus the theme and
returns a document; the service renders and saves it. No UI type appears anywhere.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
public Document Compose()
{
    BookFonts.EnsureRegistered();

    var document = new Document();
    document.Info.Title = _article.Title;
    document.Info.Subject = string.IsNullOrWhiteSpace(_article.ShortDescription)
        ? $"Wikipedia article: {_article.Title}"
        : _article.ShortDescription;
    document.Info.Author = "Wikipedia contributors";

    DefineStyles(document);

    ComposeFrontMatter(document);
    ComposeContent(document);
    ComposeColophon();

    return document;
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
_content = document.AddSection();
_content.PageSetup.PageWidth = Unit.FromPoint(t.PageWidth);
_content.PageSetup.PageHeight = Unit.FromPoint(t.PageHeight);
_content.PageSetup.TopMargin = Unit.FromPoint(t.TopMargin);
_content.PageSetup.BottomMargin = Unit.FromPoint(t.BottomMargin);
_content.PageSetup.LeftMargin = Unit.FromPoint(t.InnerMargin);
_content.PageSetup.RightMargin = Unit.FromPoint(t.OuterMargin);
_content.PageSetup.MirrorMargins = true;
_content.PageSetup.DifferentFirstPageHeaderFooter = true;
_content.PageSetup.StartingNumber = 1;
_content.PageSetup.HeaderDistance = Unit.FromPoint(t.TopMargin * 0.48);
_content.PageSetup.FooterDistance = Unit.FromPoint(t.BottomMargin * 0.42);

var header = _content.Headers.Primary.AddParagraph(BookTheme.Letterspace(_article.Title));
header.Style = "RunningHead";
header.Format.SpaceAfter = 0;

var folio = _content.Footers.Primary.AddParagraph();
folio.Style = "Folio";
folio.AddPageField();

var firstFolio = _content.Footers.FirstPage.AddParagraph();
firstFolio.Style = "Folio";
firstFolio.AddPageField();
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
var normal = document.Styles["Normal"];
normal.Font.Name = BookFonts.SerifFamily;
normal.Font.Size = t.BodySize;
normal.Font.Color = BookTheme.Ink;
normal.ParagraphFormat.Alignment = ParagraphAlignment.Justify;
normal.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
normal.ParagraphFormat.LineSpacing = t.Leading;
normal.ParagraphFormat.SpaceAfter = 0;
normal.ParagraphFormat.WidowControl = true;

//Body paragraph that opens a section (no indent), and continuation paragraphs
//  (classic book first-line indent, no inter-paragraph space)
var bodyOpen = document.AddStyle("BodyOpen", "Normal");
bodyOpen.ParagraphFormat.FirstLineIndent = 0;

var bodyIndented = document.AddStyle("BodyIndented", "Normal");
bodyIndented.ParagraphFormat.FirstLineIndent = Unit.FromPoint(t.BodySize * 1.55);
```

**Numbering across several sections.** Where each chapter is its own section, set
the starting number on exactly one of them:

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookComposer.cs
//The unnumbered cover is "page 0": printed numbering starts at 1 on the
//  first content chapter, and later sections must NOT set StartingNumber
//  again — that would restart every chapter at 1
if (isFirstContentChapter)
{
    setup.StartingNumber = 1;
}

//Running heads — chapter title on recto, book title on verso, none on
//  the chapter opener page
var recto = section.Headers.Primary.AddParagraph(BookTheme.Letterspace(chapter.Title));
recto.Style = "RunningHead";
var verso = section.Headers.EvenPage.AddParagraph(BookTheme.Letterspace(_chapters[0].Title));
verso.Style = "RunningHead";

AddFolio(section.Footers.Primary);
AddFolio(section.Footers.EvenPage);
AddFolio(section.Footers.FirstPage);
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
if (pageId is not null
    && _context.PagesInBook.TryGetValue(NotionConvert.NormalizeId(pageId), out var bookPage))
{
    //The target is in the book: give the reader the printed folio
    var paragraph = target.AddParagraph();
    paragraph.Style = "RefLine";
    paragraph.AddText($"See {bookPage.Title}, page ");
    paragraph.AddPageRefField(bookPage.BookmarkName);
    paragraph.AddText(".");
    return;
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BookComposer.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/PageNumberingTests.cs`

**Sharp edges.**
- A different first-page header and footer means the first page needs its own
  folio paragraph, or page one silently loses its number.
- Setting the starting number on more than one section restarts numbering; there
  is a test asserting later sections keep the default.
- Styles inherit by name, so define the base styles first.
- Heading styles set an outline level, which is what produces the document's
  outline bookmarks; a style that should not appear there resets it.
- Bookmarks are added where a title is written, and page-reference fields resolve
  them at render time, so forward references work.

### Build a table of contents with real page numbers and dot leaders

**When you want this.** A generated document needs a contents page whose numbers
are correct after layout.

**The MVVM shape.** Headings get bookmarks as they are composed; entries are a
hyperlink to the bookmark, a tab, and a page-reference field; the leader dots come
from a right-aligned tab stop on the entry style.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
var tocEntry = document.AddStyle("TocEntry", "Normal");
tocEntry.ParagraphFormat.Alignment = ParagraphAlignment.Left;
tocEntry.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
tocEntry.ParagraphFormat.LineSpacing = Unit.FromPoint(t.Leading * 1.15);
tocEntry.ParagraphFormat.AddTabStop(Unit.FromPoint(t.TextWidth), TabAlignment.Right, TabLeader.Dots);
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
private void ComposeToc(Section frontMatter)
{
    var headings = _article.Blocks
        .Where(b => b.Type == ArticleBlockType.Heading && b.HeadingLevel <= 2)
        .ToList();
    var level1Count = headings.Count(h => h.HeadingLevel == 1);
    if (level1Count < 2) { return; }

    //Only include sub-headings when the contents stay comfortably on one page
    var includeLevel2 = headings.Count <= 24;

    frontMatter.AddPageBreak();
    // ... title and rule ...

    foreach (var heading in headings)
    {
        if (heading.HeadingLevel == 1) { sectionIndex++; }
        else if (!includeLevel2) { continue; }

        var bookmark = BookmarkNameFor(heading);
        var entry = frontMatter.AddParagraph();
        entry.Style = heading.HeadingLevel == 1 ? "TocEntry" : "TocEntry2";

        var link = entry.AddHyperlink(bookmark, HyperlinkType.Bookmark);
        link.AddText(heading.Text);
        entry.AddTab();
        entry.AddPageRefField(bookmark);
    }
}

private string BookmarkNameFor(ArticleBlock heading)
{
    //Stable, unique bookmark names derived from block identity
    var index = _article.Blocks.IndexOf(heading);
    return $"sec.{index}";
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs`

**Sharp edges.**
- Bookmark names must be unique and stable; deriving them from the block's index
  in the parsed model means the contents and the body agree without a shared
  counter.
- The contents are composed into the front-matter section before the body exists;
  the page-reference fields resolve at render time.
- No contents page is emitted for a short document, and sub-headings are dropped
  when the list would overflow.

### Place numbered framed figures with credit lines

**When you want this.** Images in a document need consistent sizing, a frame where
it flatters them, a credit and a numbered caption.

**The MVVM shape.** The composer chooses a width from the aspect ratio, clamps the
height against the text block, and only ever places images that actually
downloaded.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
private void ComposeFigure(ArticleImage articleImage)
{
    if (articleImage?.ProcessedBytes is null
        || articleImage.ProcessedWidth <= 0 || articleImage.ProcessedHeight <= 0)
    {
        return;
    }

    var t = _theme;
    _figureNumber++;

    var aspect = (double)articleImage.ProcessedWidth / articleImage.ProcessedHeight;
    var width = aspect switch
    {
        >= 1.25 => t.TextWidth,
        >= 0.85 => t.TextWidth * 0.70,
        _ => t.TextWidth * 0.52
    };

    var maxHeight = t.TextHeight * 0.58;
    if (width / aspect > maxHeight)
    {
        width = maxHeight * aspect;
    }

    var paragraph = _content.AddParagraph("", "Figure");
    var image = paragraph.AddImage(CreateImageSource(articleImage));
    image.LockAspectRatio = true;
    image.Width = Unit.FromPoint(width);
    ApplyKeyline(image, articleImage);
    PlacedImageCount++;

    //A small credit line for the photographer/illustrator and licence, directly under the
    //  image and above the caption, when Wikimedia supplied attribution for the file
    if (!string.IsNullOrWhiteSpace(articleImage.Attribution))
    {
        _content.AddParagraph(articleImage.Attribution, "Credit");
    }

    var caption = _content.AddParagraph();
    caption.Style = "Caption";
    var label = caption.AddFormattedText($"FIG. {_figureNumber}");
    label.Font.Bold = true;
    label.Font.Size = t.LabelSize;
    label.Font.Color = BookTheme.Accent;
    // ... caption text ...
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
private static ImageSource.IImageSource CreateImageSource(ArticleImage articleImage)
{
    var bytes = articleImage.ProcessedBytes;
    return ImageSource.FromBinary(
        $"img-{articleImage.FileName}-{Guid.NewGuid():N}",
        () => bytes,
        quality: 90);
}

private static void ApplyKeyline(CodeBrix.PdfDocCreate.DocumentObjectModel.Shapes.Image image, ArticleImage articleImage)
{
    //A hairline keyline flatters photographs; diagrams and transparent
    //  graphics (PNG) read better without a frame
    if (IsJpeg(articleImage.ProcessedBytes))
    {
        image.LineFormat.Width = 0.4;
        image.LineFormat.Color = BookTheme.Hairline;
    }
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`

**Sharp edges.**
- The image source takes a name that acts as a cache key; add a unique suffix so
  two renders in one process never collide on a stale entry.
- Lock the aspect ratio and give only the width; setting both dimensions distorts.
- A credit style should use exact line spacing, because an at-least rule reserves
  a full text line above a tiny credit.
- Increment the figure counter before the placement, and report the placed count
  back in the result.

### Pair a figure with the credit paragraph that follows it

**When you want this.** The source format has no notion of an image credit, but
authors write one in the paragraph immediately after the picture, and it should be
typeset with the figure instead of as body text.

**The MVVM shape.** A pure static look-ahead in the library that the block loop
calls; it returns the index the loop should continue from, so a consumed paragraph
is never rendered twice. It is unit-tested with no network and no document.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
/// <summary>
/// The credit look-ahead: when the sibling immediately after an image/video is a
/// paragraph shaped like a rights line, it is consumed and rendered with the
/// figure. Returns that sibling's index, or the block's own index when the next
/// paragraph is ordinary body text.
/// </summary>
internal static int FindCreditParagraph(IReadOnlyList<NotionBlockNode> blocks, int index)
{
    if (index + 1 >= blocks.Count) { return index; }
    if (blocks[index + 1].Block is not ParagraphBlock paragraph) { return index; }

    var text = NotionConvert.PlainText(paragraph.Paragraph?.RichText).TrimStart();
    string[] creditShapes = ["Credit", "Image credit", "Photo", "Source", "©", "Public domain", "CC "];
    return creditShapes.Any(shape => text.StartsWith(shape, StringComparison.Ordinal))
        ? index + 1
        : index;
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
private int RenderImage(IBlockTarget target, IReadOnlyList<NotionBlockNode> blocks, int index,
    ImageBlock image)
{
    if (!_context.IncludeImages) { return index; }

    var creditIndex = FindCreditParagraph(blocks, index);
    var creditRuns = creditIndex > index
        ? ((ParagraphBlock)blocks[creditIndex].Block).Paragraph?.RichText
        : null;

    if (_context.MediaByBlockId.TryGetValue(image.Id, out var media) && media.HasImage)
    {
        PlaceFigure(target, media.Image, "FIG.", image.Image?.Caption, creditRuns, urlLine: null);
    }
    else
    {
        _context.Warnings.Add(BuildMediaWarning("Image", image.Image, media));
        RenderMediaFileCard(target, "IMAGE", image.Image, image.Id);
        RenderConsumedCredit(target, creditRuns);
    }
    return creditIndex;
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/CreditPairingTests.cs`

**Sharp edges.**
- The loop variable is assigned from the return value, which is how the consumed
  paragraph is skipped; returning the block's own index means "nothing consumed".
- Both the success path and the fallback card path render the credit, so a failed
  download never silently drops the rights line.
- The figure placement chooses its width from the aspect ratio, caps its height
  against the text block, and adds a keyline only for photographic formats.

### Render booktabs style tables from parsed rows

**When you want this.** Tables in a typeset document should have horizontal rules
only.

**The MVVM shape.** The parser produces a rectangular table model and refuses
anything it cannot lay out; the composer applies the rules.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
var table = _content.AddTable();
table.Borders.Visible = false;
table.TopPadding = Unit.FromPoint(3);
table.BottomPadding = Unit.FromPoint(3);
table.LeftPadding = Unit.FromPoint(2);
table.RightPadding = Unit.FromPoint(4);

var columnWidth = t.TextWidth / articleTable.ColumnCount;
for (var c = 0; c < articleTable.ColumnCount; c++)
{
    table.AddColumn(Unit.FromPoint(columnWidth));
}

for (var r = 0; r < articleTable.Rows.Count; r++)
{
    var sourceRow = articleTable.Rows[r];
    var row = table.AddRow();

    //Booktabs styling: strong top and bottom rules, a light rule under the header,
    //  and no vertical rules at all
    if (r == 0)
    {
        row.Borders.Top.Width = 1.0;
        row.Borders.Top.Color = BookTheme.Ink;
        if (articleTable.HasHeaderRow)
        {
            row.Borders.Bottom.Width = 0.5;
            row.Borders.Bottom.Color = BookTheme.Ink;
            row.HeadingFormat = true;
        }
    }
    if (r == articleTable.Rows.Count - 1)
    {
        row.Borders.Bottom.Width = 1.0;
        row.Borders.Bottom.Color = BookTheme.Ink;
    }
    // ... cells, with MergeRight for column spans ...
}
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs
foreach (var tr in table.QuerySelectorAll("tr"))
{
    //Only rows belonging to this table (not nested tables)
    if (tr.Closest("table") != table) { continue; }
    // ...
    if (int.TryParse(cell.GetAttribute("rowspan"), out var rowSpan) && rowSpan > 1)
    {
        skipReason = "uses row spans";
        return null;
    }
    // ...
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/ArticleParser.cs`

**Sharp edges.**
- Marking the first row as a heading is what repeats it on a page break.
- Nested tables would be picked up by a plain descendant query; filter by nearest
  ancestor.
- The parser refuses tables it cannot represent - row spans, too few rows, too
  many rows or columns - and records the reason as a warning rather than producing
  a broken layout.
- A zero-height spacer paragraph after the table supplies the space below it,
  because a table object carries no space-after.

### Open a document with a raised initial

**When you want this.** The first paragraph of a book should start with a large
colored capital.

**The MVVM shape.** A one-shot flag on the composer; the first body paragraph
whose text starts with a letter gets its first character split into its own
formatted run.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs
private void ComposeParagraph(ArticleBlock block)
{
    var paragraph = _content.AddParagraph();
    paragraph.Style = _previousWasBodyParagraph ? "BodyIndented" : "BodyOpen";

    var runs = block.Runs;

    //A raised initial marks the opening of the book (the first lead paragraph)
    if (!_raisedCapPlaced && runs.Count > 0 && runs[0].Text.Length > 0
        && char.IsLetter(runs[0].Text[0]))
    {
        _raisedCapPlaced = true;
        paragraph.Format.SpaceBefore = Unit.FromPoint(_theme.BodySize * 0.6);

        var first = runs[0];
        var initial = paragraph.AddFormattedText(first.Text[..1]);
        initial.Font.Size = _theme.RaisedCapSize;
        initial.Font.Color = BookTheme.Accent;

        AppendRuns(paragraph, [first with { Text = first.Text[1..] }, .. runs.Skip(1)]);
        return;
    }

    AppendRuns(paragraph, runs);
}
```

**Where to look.**
`WikipediaPublisher/WikipediaPublisher.RenderArticle/Internal/BookComposer.cs`

**Sharp edges.**
- The style choice is driven by a "previous was a body paragraph" flag the block
  loop maintains, which is how classic first-line indents are applied only to
  continuation paragraphs.
- The run type is a record, so a `with` expression keeps the formatting of the
  remaining text.

### Write rich text runs into a paragraph or a hyperlink

**When you want this.** Source text arrives as annotated runs - bold, italic,
code, links - and the document model's paragraph and hyperlink types share no base
type for adding runs.

**The MVVM shape.** A writer class plus a tiny private adapter interface with two
implementations, so annotation handling is written once.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs
//Paragraph and Hyperlink share no add-text base type, so a tiny adapter pair
//  lets annotated runs target either
private interface IRunTarget
{
    FormattedText AddFormattedText(string text);
    void AddLineBreak();
}

private sealed class ParagraphTarget : IRunTarget
{
    private readonly Paragraph _paragraph;
    public ParagraphTarget(Paragraph paragraph) { _paragraph = paragraph; }
    public FormattedText AddFormattedText(string text) => _paragraph.AddFormattedText(text);
    public void AddLineBreak() => _paragraph.AddLineBreak();
}

private sealed class HyperlinkTarget : IRunTarget
{
    private readonly Hyperlink _hyperlink;
    public HyperlinkTarget(Hyperlink hyperlink) { _hyperlink = hyperlink; }
    public FormattedText AddFormattedText(string text) => _hyperlink.AddFormattedText(text);

    //Hyperlinks cannot hold line breaks — a newline inside link text becomes a space
    public void AddLineBreak() => _hyperlink.AddText(" ");
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs
private void ApplyAnnotations(FormattedText formatted, Annotations annotations, bool isLink, bool isEmoji)
{
    if (isEmoji) { formatted.Font.Name = BookFonts.EmojiFamily; }
    if (isLink) { formatted.Font.Color = BookTheme.Accent; }
    if (annotations is null) { return; }

    if (annotations.IsBold) { formatted.Font.Bold = true; }
    if (annotations.IsItalic) { formatted.Font.Italic = true; }
    if (annotations.IsUnderline) { formatted.Font.Underline = Underline.Single; }
    if (annotations.IsStrikeThrough) { formatted.Font.Strikethrough = Strikethrough.Single; }
    if (annotations.IsCode && !isEmoji)
    {
        formatted.Font.Name = BookFonts.MonoFamily;
        formatted.Font.Size = _theme.BodySize * 0.88;
    }
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RichTextWriter.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/RichTextWriterTests.cs`

**Sharp edges.**
- A link URL can arrive either on the run itself or inside the text run's own link
  object; check both.
- Newlines inside a run become explicit line breaks, except inside a hyperlink,
  where they become a space.
- Content the document cannot typeset - an inline equation, for instance - is
  rendered as its source text and adds a warning.

### Render into either a section or a table cell

**When you want this.** The same content renderer must work at top level and
inside a table cell, where the document model forbids nested tables.

**The MVVM shape.** One private interface with a capability flag, two adapters,
and every render method taking the interface. Callers ask the flag before choosing
a boxed layout or a flat one.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
//Section and Cell share no content-adding base type, so the renderer targets
//  either through a tiny adapter. Tables cannot nest inside cells in MigraDoc,
//  which is what SupportsTables guards.
private interface IBlockTarget
{
    bool SupportsTables { get; }
    Paragraph AddParagraph();
    Paragraph AddParagraph(string text, string style);
    Table AddTable();
}

private sealed class SectionTarget : IBlockTarget
{
    private readonly Section _section;
    public SectionTarget(Section section) { _section = section; }
    public bool SupportsTables => true;
    public Paragraph AddParagraph() => _section.AddParagraph();
    public Paragraph AddParagraph(string text, string style) => _section.AddParagraph(text, style);
    public Table AddTable() => _section.AddTable();
}

private sealed class CellTarget : IBlockTarget
{
    private readonly Cell _cell;
    public CellTarget(Cell cell) { _cell = cell; }
    public bool SupportsTables => false;
    public Paragraph AddParagraph() => _cell.AddParagraph();
    public Paragraph AddParagraph(string text, string style)
    {
        var paragraph = _cell.AddParagraph(text);
        paragraph.Style = style;
        return paragraph;
    }
    public Table AddTable() =>
        throw new InvalidOperationException("MigraDoc cannot nest a table inside a table cell.");
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
if (target.SupportsTables)
{
    var table = target.AddTable();
    table.Borders.Visible = false;
    table.Borders.Left.Width = 1.4;
    table.Borders.Left.Color = BookTheme.Accent;
    table.Shading.Color = BookTheme.PanelTint;
    // ...
    var cell = table.AddRow().Cells[0];
    FillCallout(new CellTarget(cell), node, titleRuns, bodyRuns, iconGlyph);
    AddPanelSpacer(target);
}
else
{
    //Inside a table cell (a column): tinted paragraphs, no nested table
    FillCallout(target, node, titleRuns, bodyRuns, iconGlyph, tintParagraphs: true);
}
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`

**Sharp edges.**
- Content that cannot be represented in the constrained target degrades to a
  simpler form plus a warning, rather than throwing.
- The cell adapter's styled overload has to set the style itself, because the cell
  version does not take one.
- A hair-high spacer paragraph after each panel keeps consecutive panels from
  touching.

### Keep unsupported content visible instead of failing the document

**When you want this.** You are mapping an open-ended source format and new input
kinds will appear after you ship.

**The MVVM shape.** The renderer's switch has a default arm that prints a visible
marker and records a warning; the service collects warnings and notes on a context
object and returns the warnings in the result, while notes are only logged.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs
private void RenderUnsupported(IBlockTarget target, IBlock block)
{
    target.AddParagraph("[unsupported Notion block]", "UnsupportedMarker");
    var typeName = (block as UnsupportedBlock)?.Unsupported?.BlockType;
    _context.Warnings.Add(string.IsNullOrWhiteSpace(typeName)
        ? "A block of an unsupported type could not be rendered; a marker was printed instead."
        : $"A block of unsupported type \"{typeName}\" could not be rendered; a marker was printed instead.");
}
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RenderContext.cs
/// <summary>Non-fatal problems worth showing the user (surfaced in the result dialog).</summary>
public IList<string> Warnings { get; init; } = new List<string>();

/// <summary>Informational notes (logged, not shown as warnings).</summary>
public IList<string> Notes { get; init; } = new List<string>();
```

**Where to look.**
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/BlockRenderer.cs`
`NotionDocumentCreator/src/libs/NotionDocumentCreator.CreateDocument/Internal/RenderContext.cs`

**Sharp edges.**
- The class comment states the contract: unknown types render a visible marker and
  a warning; nothing silently vanishes and nothing ever throws mid-document.
- Two sinks, deliberately: warnings reach the user, notes only the log.

### Compose a fixed layout poster with the CodeBrix PdfDocuments library

**When you want this.** You want a designed, fixed-layout page - a one-sheet, a
certificate, a label - rather than a flowing document.

**The MVVM shape.** A separate library with a plain request object as its whole
input, no UI and no graphics-backend dependency, so the composition is exercised
headlessly. The view model gathers a snapshot into the request and calls the
creator on a worker thread.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Services/MarketingSheetCreator.cs
private static PdfDocument Compose(MarketingSheetRequest request)
{
    SheetFonts.EnsureRegistered();

    var document = new PdfDocument();
    document.Info.Title = $"{request.ModelName} — Poly Haven model one-sheet";
    document.Info.Subject = "A marketing one-sheet generated by PolyHavenBrowser";

    var page = document.AddPage();
    page.Width = SheetTheme.PageWidth;
    page.Height = SheetTheme.PageHeight;

    var accent = AccentColorSampler.Sample(request.CatalogThumbnailBytes);
    var theme = new SheetTheme(accent);

    using var gfx = XGraphics.FromPdfPage(page);
    new SheetComposer(request, theme).Compose(gfx);

    return document;
}
```

```xml
<!-- From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/PolyHavenBrowser.CreateDocument.csproj -->
<!-- The marketing one-sheet is a poster (absolute placement of shots, rules and type),
     so it draws directly with CodeBrix.PdfDocuments' XGraphics rather than composing a
     flowing document through the CodeBrix.PdfDocCreate document object model the way
     NotionDocumentCreator's book pipeline does. -->
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Internal/SheetComposer.cs
public void Compose(XGraphics gfx)
{
    ArgumentNullException.ThrowIfNull(gfx);

    DrawHeader(gfx);
    DrawHeroRow(gfx);
    DrawCopyColumns(gfx);
    DrawTags(gfx);
    DrawGallery(gfx);
    DrawSpecs(gfx);
    DrawFooter(gfx);
}
```

**Where to look.**
`PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Services/MarketingSheetCreator.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Internal/SheetComposer.cs`
`PolyHavenBrowser/src/libs/PolyHavenBrowser.CreateDocument/Models/MarketingSheetRequest.cs`

**Sharp edges.**
- Choose the API to match the document: absolute placement wants the graphics
  object directly; a flowing, paginated document wants a document object model
  instead. The project file says so where a future reader will find it.
- The graphics object has no character-spacing property, so a letterspaced label
  is made by interleaving thin spaces into the string itself.
- Every band of the poster clamps or truncates its own content - the title shrinks
  in one-point steps until it measures within the content width - which is how the
  sheet stays exactly one page.
- Offering both a save-to-file and a return-bytes entry point is what lets the
  tests parse the result.

### Open a PDF and read its page count with the CodeBrix PdfRasterizer library

**When you want this.** You need to know how many pages a user-chosen PDF has, and
you want a clear error when the file is missing or is not a PDF.

**The MVVM shape.** A plain document type in the library owns the file: it reads
the bytes once, asks the rasterizer for the page count, and exposes a one-based
cursor that cannot leave the range. The view model holds the document; it never
touches the rasterizer.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/PdfPageDocument.cs
    /// <summary>The raw PDF bytes, handed to the rasterizer so the file is never re-read.</summary>
    internal byte[] PdfBytes { get; }

    public static async Task<PdfPageDocument> OpenAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = DocumentPath.Normalize(filePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The PDF file was not found.", fullPath);
        }

        var pdfBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);

        int pageCount;
        try
        {
            using var rasterizer = new PageRasterizer();
            pageCount = await rasterizer.GetPageCount(pdfBytes, cancellationToken: cancellationToken);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            throw new InvalidDataException(
                $"“{Path.GetFileName(fullPath)}” could not be read as a PDF document.", e);
        }

        if (pageCount < 1)
        {
            throw new InvalidDataException($"“{Path.GetFileName(fullPath)}” has no pages.");
        }

        return new PdfPageDocument(fullPath, pdfBytes, pageCount);
    }
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/PdfPageDocument.cs`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/PdfPageDocumentTests.cs`

**Sharp edges.**
- The bytes are read once and kept, and every later render is given the same
  array. The file is never re-opened, so the user can move or delete it
  mid-session.
- The rasterizer is disposable. Here it is created and disposed just for the page
  count; the long-lived one lives in the renderer.
- The exception filter is what keeps a cancellation from being reported as "not a
  PDF".
- The page count is checked, not trusted: a result below one becomes a typed
  exception with a message naming the file.

### Rasterize a PDF page to PNG off the UI thread

**When you want this.** You want a page image to hand to a bound image element,
and the rasterizer is synchronous underneath.

**The MVVM shape.** A renderer service in the library does the work and returns a
small immutable record. The view model awaits it and never sees a bitmap API; the
pane view model turns the bytes into an image source.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs
        //PDFium renders synchronously, so keep it off the caller's (UI) thread
        var pngBytes = await Task.Run(async () =>
        {
            using var image = await _rasterizer.RasterizeToImage(
                document.PdfBytes, pageNumber, dpi, cancellationToken: cancellationToken);
            using var stream = new MemoryStream();
            await image.SaveAsync(stream, PngFormat.Instance, cancellationToken);
            return (Width: image.Width, Height: image.Height, Bytes: stream.ToArray());
        }, cancellationToken);

        var rendered = new RenderedPage(document.FilePath, pageNumber, pngBytes.Width, pngBytes.Height, pngBytes.Bytes);
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/RenderedPage.cs
public sealed record RenderedPage(string FilePath, int PageNumber, int PixelWidth, int PixelHeight, byte[] PngBytes);
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/PageRenderer.cs`
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Rendering/RenderedPage.cs`

**Sharp edges.**
- The comment states the reason for `Task.Run`: the engine renders synchronously,
  so awaiting alone would still block the calling thread.
- The rendered image is disposed inside the `Task.Run` body; only the encoded
  bytes and the pixel size escape, so nothing that needs disposal crosses back to
  the UI thread.
- The pixel size is read from the image, not computed; the tests rely on it.
- One rasterizer instance is created per renderer and disposed with it, rather
  than one per render.

### Keep two documents in step while letting the user offset one

**When you want this.** Two editions of a document rarely paginate identically,
and the user needs to line them up once and then page through both together.

**The MVVM shape.** A plain model class holds both documents and both cursors and
exposes the moves as boolean-returning methods with matching "can" properties, so
the view model can wire them straight to command predicates. No UI type appears in
it.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/PdfComparison.cs
    /// <summary>Whether MoveBothNext would move at least one cursor.</summary>
    public bool CanMoveBothNext => IsReady && (Left.CanMoveNext || Right.CanMoveNext);

    /// <summary>
    /// Steps both documents to their next page; a document already on its last page stays
    /// there. Returns whether any cursor moved.
    /// </summary>
    public bool MoveBothNext()
    {
        if (!IsReady) { return false; }
        var movedLeft = Left.MoveNext();
        var movedRight = Right.MoveNext();
        return ResetViewIf(movedLeft || movedRight);
    }

    /// <summary>Steps only the right document to its next page. Returns whether it moved.</summary>
    public bool AdjustRightNext() => ResetViewIf(IsReady && Right.MoveNext());

    //A page change always comes back at fit-the-page, both panes centred
    private bool ResetViewIf(bool moved)
    {
        if (moved) { View.Reset(); }
        return moved;
    }
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/PdfComparison.cs`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/PdfComparisonTests.cs`

**Sharp edges.**
- Both moves are performed unconditionally and their results combined afterwards.
  Short-circuiting would skip the second document, so assigning to two locals
  first is deliberate.
- Each cursor clamps at its own last page, so the offset the user set is preserved
  until one document runs out.
- Every move that actually changes a page resets the zoom and pan; a move that
  goes nowhere leaves the view alone, decided by the return value.
- The model owns the view state too, so the "reset on page change" rule cannot be
  forgotten by a caller.

### Treat two spellings of one path as the same file

**When you want this.** You need to reject "the user picked the same file twice",
and the two paths may be spelled differently and may be on a case-insensitive file
system.

**The MVVM shape.** A tiny internal helper in the library, used by the model and
covered by tests through the internals-visible attribute.

**Code.**

```csharp
// From CodeBrix.Samples/PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/DocumentPath.cs
internal static class DocumentPath
{
    private static readonly StringComparison Comparison =
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    /// <summary>Returns the absolute, separator-trimmed form of path.</summary>
    public static string Normalize(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }

    /// <summary>Whether pathA and pathB refer to the same file.</summary>
    public static bool AreSame(string pathA, string pathB) =>
        string.Equals(Normalize(pathA), Normalize(pathB), Comparison);
}
```

**Where to look.**
`PdfSideBySide/src/libs/PdfSideBySide.PdfRender/Documents/DocumentPath.cs`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/DocumentPathTests.cs`

**Sharp edges.**
- The comparison is chosen by operating system, not hard-coded.
- Getting the full path collapses relative segments and makes the path absolute;
  the trailing separator has to be trimmed separately or two spellings of a
  directory differ.
- The normalized form is what the document stores, so everything downstream - the
  cache key, the duplicate check, the displayed path - agrees.

### Register import and export formats at startup through one entry point

**When you want this.** You want your codec set to be data, not scattered
registration calls, and you want a library to own it.

**The MVVM shape.** The model's format manager starts empty; a library exposes one
static registration method the application calls at startup. Each format is a
descriptor pairing an importer and an exporter - either may be absent - with
display name, extensions, media types and a capability flag.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.FileFormats/Registration/FileFormats.cs
public static void RegisterAll (ImageConverterManager formats)
{
	// --- SkiaSharp-codec formats (Skia decodes and encodes these)

	SkiaCodecFormat pngHandler = new ("png", SKEncodedImageFormat.Png);
	formats.RegisterFormat (new FormatDescriptor (
		displayPrefix: "PNG",
		extensions: ["png", "PNG"],
		mimes: ["image/png"],
		importer: pngHandler,
		exporter: pngHandler));
	// ...
	// --- Formats Skia decodes but cannot encode (CodeBrix.Imaging exports)

	formats.RegisterFormat (new FormatDescriptor (
		displayPrefix: "BMP",
		extensions: ["bmp", "BMP"],
		mimes: ["image/bmp"],
		importer: new SkiaCodecFormat ("bmp"),
		exporter: CodeBrixImagingFormat.CreateBmp ()));
	// ...
	// --- This library's own format implementations

	OraFormat oraHandler = new ();
	formats.RegisterFormat (new FormatDescriptor (
		displayPrefix: "OpenRaster",
		extensions: ["ora", "ORA"],
		mimes: ["image/openraster"],
		importer: oraHandler,
		exporter: oraHandler,
		supportsLayers: true));

	// Export-only, matching upstream
	formats.RegisterFormat (new FormatDescriptor (
		displayPrefix: "TGA",
		extensions: ["tga", "TGA"],
		mimes: ["image/x-tga"],
		importer: null,
		exporter: new TgaExporter ()));
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Engine bootstrap: install the UI-layer services and register the
//file formats and core effects/adjustments with the engine
Pinta.Brix.Engine.PintaCore.InitializeResources(new Pinta.Brix.Controls.SkiaResourceService());
Pinta.Brix.Engine.PintaCore.InitializeTimer(
    new Pinta.Brix.Controls.DispatcherTimerService(MainWindow.DispatcherQueue));
Pinta.Brix.FileFormats.FileFormatsRegistration.RegisterAll(Pinta.Brix.Engine.PintaCore.ImageFormats);
Pinta.Brix.Effects.CoreEffects.Register(Pinta.Brix.Engine.PintaCore.Services);
Pinta.Brix.Tools.CoreTools.Register(Pinta.Brix.Engine.PintaCore.Services);
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.FileFormats/Registration/FileFormats.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Effects/Registration/CoreEffects.cs`
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`

**Sharp edges.**
- The effects and tools registrations take a service provider and resolve what
  each item needs, so the registration list is the only place that knows the
  catalog.
- Extensions are listed in both cases because file matching is ordinal; the save
  picker filters to the lowercase ones only.
- Registration happens after the window exists, because one of the installed
  services needs the window's dispatcher queue.

### Add codec coverage beyond SkiaSharp with the CodeBrix Imaging library

**When you want this.** Your users expect formats the base graphics library cannot
encode, or cannot handle at all.

**The MVVM shape.** One importer and exporter class wrapping the imaging library,
with factory methods per format, sitting behind the same interfaces as every other
codec. Nothing above the format registry knows which library answered.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.FileFormats/CodeBrixImagingFormat.cs
/// <summary>Windows bitmap, written as uncompressed 32-bit BGRA.</summary>
public static CodeBrixImagingFormat CreateBmp ()
	=> new (
		"bmp",
		BmpFormat.Instance,
		new BmpEncoder {
			BitsPerPixel = BmpBitsPerPixel.Pixel32,
			SupportTransparency = true,
		});

/// <summary>GIF, quantized to a 256-color palette by the encoder.</summary>
public static CodeBrixImagingFormat CreateGif ()
	=> new ("gif", GifFormat.Instance, new GifEncoder ());

/// <summary>TIFF, written with the encoder's default settings.</summary>
public static CodeBrixImagingFormat CreateTiff ()
	=> new ("tiff", TiffFormat.Instance, new TiffEncoder ());

/// <inheritdoc/>
public void Export (Document document, string file)
{
	using ImageSurface flattenedImage = document.GetFlattenedImage ();

	ReadOnlySpan<ColorBgra> source = flattenedImage.GetReadOnlyPixelData ();
	Bgra32[] destination = new Bgra32[flattenedImage.Width * flattenedImage.Height];

	// Premultiplied engine pixels back to straight alpha for the encoder.
	for (int i = 0; i < destination.Length; i++) {
		ColorBgra p = source[i].ToStraightAlpha ();
		destination[i] = new Bgra32 (r: p.R, g: p.G, b: p.B, a: p.A);
	}

	using CodeBrix.Imaging.Image<Bgra32> image = ImagingImage.LoadPixelData<Bgra32> (
		destination,
		flattenedImage.Width,
		flattenedImage.Height,
		image_format);

	using FileStream stream = File.Create (file);
	encoder.Encode (image, stream);
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.FileFormats/CodeBrixImagingFormat.cs`
`Pinta.Brix/tests/libs/Pinta.Brix.FileFormats.Tests/CodeBrixImagingFormatTests.cs`

**Sharp edges.**
- The alpha convention differs on each side: the imaging library's pixels are
  straight alpha, the engine's surfaces are premultiplied. Both directions convert
  explicitly, and this is the bug that would otherwise show up as dark halos on
  transparent edges.
- Import and export can come from different libraries for the same format.
- The project file's comment states the reason for the dependency in one line:
  codec coverage beyond the other library's encoders.

### Save a document through a native picker with format filters

**When you want this.** A save-as that offers exactly the formats you can write
and warns before a lossy conversion.

**The MVVM shape.** The model exposes a handler delegate; the UI layer installs an
async implementation that shows the picker, resolves an exporter, warns when the
chosen format cannot hold everything, and marks the document clean. In a
view-model shape the handler is a bridge interface the page implements and the
view model consumes.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/WorkspaceManager.cs
/// <summary>
/// Handler the application installs to perform document saving (shows
/// pickers, runs exporters). Returns true when the save completed.
/// (Upstream routed this through the app-level action manager.)
/// </summary>
public Func<Document, bool, System.Threading.Tasks.Task<bool>>? SaveDocumentHandler { get; set; }
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs
var picker = new Windows.Storage.Pickers.FileSavePicker
{
    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
    SuggestedFileName = document.DisplayName,
};
foreach (var format in PintaCore.ImageFormats.Formats.Where(f => f.IsExportAvailable()))
{
    var extensions = format.Extensions
        .Where(x => x.All(char.IsLower))
        .Select(x => $".{x}")
        .ToList();
    if (extensions.Count > 0)
    {
        picker.FileTypeChoices.Add(format.FilterName, extensions);
    }
}

StorageFile file = await picker.PickSaveFileAsync();
if (file is null) { return false; }
// ...
//Saving to a format that cannot hold layers flattens the image; upstream
//asks first, because the layers are gone from the FILE either way.
if (document.Layers.Count() > 1 && !descriptor.SupportsLayers)
{
    ContentDialog flattenDialog = new()
    {
        Title = "Flatten Image?",
        Content = $"The {descriptor.FilterName} format does not support layers. "
            + "The saved file will contain a flattened copy of the image.",
        PrimaryButtonText = "Flatten",
        CloseButtonText = "Cancel",
        DefaultButton = ContentDialogButton.Primary,
        XamlRoot = XamlRoot,
    };

    if (await flattenDialog.ShowAsync() != ContentDialogResult.Primary) { return false; }
}

descriptor.Exporter.Export(document, path);
document.File = path;
document.FileType = fileType;
document.Workspace.History.SetClean();
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/Views/MainPage.xaml.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/WorkspaceManager.cs`

**Sharp edges.**
- The picker's filter list is built from the registry's export-capable formats
  only, and from lowercase extensions only, because the registry lists both cases
  for matching.
- The picker returns null on cancel and the whole save must return false, or a
  cancelled save marks the document clean.
- Clearing the history's dirty flag after a successful export is what clears the
  marker on the tab title.
- A failed or cancelled save inside a close prompt must abort the close; the close
  path checks the return value.

### Raise a UI hook from a codec through a static event

**When you want this.** A library needs one optional interaction - a quality
slider, an overwrite confirmation - without taking a UI dependency.

**The MVVM shape.** The codec exposes a static event carrying mutable event
arguments; if nothing subscribes, the stored value is used unchanged. The UI layer
subscribes if and when it wants to show the dialog.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.FileFormats/JpegFormat.cs
/// <summary>
/// Raised before a document's first JPEG save of the session so a UI layer
/// can let the user adjust the compression quality. Handlers may update
/// <see cref="ModifyCompressionEventArgs.Quality"/> or set
/// <see cref="ModifyCompressionEventArgs.Cancel"/> to abort the save.
/// With no handler installed the pending quality value is used as-is.
/// </summary>
public static event EventHandler<ModifyCompressionEventArgs>? ModifyCompression;

protected override void DoSave (ImageSurface flattenedImage, Document document, string file, SKEncodedImageFormat format)
{
	//Load the JPG compression quality, but use the default value if there is no saved value.
	int level = PintaCore.Settings.GetSetting (JpgQualitySetting, DefaultQuality);

	//Check to see if the Document has been saved before.
	if (!document.HasBeenSavedInSession) {
		level = RaiseModifyCompression (level);

		if (level == -1)
			throw new OperationCanceledException ();
	}

	//Store the "previous" JPG compression quality value (before saving with it).
	PintaCore.Settings.PutSetting (JpgQualitySetting, level);

	SaveBitmap (flattenedImage, file, format, level);
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.FileFormats/JpegFormat.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/EventArgs/ModifyCompressionEventArgs.cs`

**Sharp edges.**
- The setting key is duplicated as a private constant with a comment saying it
  matches the engine's internal name, because the engine's constants class is
  internal. Duplicating a key across an assembly boundary needs that comment or
  the two silently drift.
- The event is static, so a subscriber must unsubscribe or it lives for the
  process.
- Cancel is signalled by a sentinel value converted into a cancellation exception,
  so a cancel is distinguishable from a failure.

## Settings and persistence

### Wrap the AppSettings add-in in one application named facade

**When you want this.** Any application with settings. The facade gives you one
application-named type to call, one place to change the backend, and a store that
survives corruption.

**The MVVM shape.** A static facade in its own small library forwards every call
to the add-in. View models call the facade by key; nothing else in the application
talks to the add-in. Keys are constants on the type that owns them.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Settings/SettingsService.cs
public static class SettingsService
{
    /// <summary>The application name the settings store is registered under.</summary>
    public const string AppName = "KenneyAssetBrowser";

    public static bool IsInitialized => AppSettingsService.IsInitialized;
    public static AppSettingsStore Store => AppSettingsService.Store;
    public static string DefaultDirectory => AppSettingsService.GetDefaultDirectory(AppName);

    /// <summary>
    /// Opens the settings store in the default folder, running the startup
    /// auto-backup and pruning sequence. Call once, before any UI renders.
    /// </summary>
    public static void Initialize() => AppSettingsService.Initialize(AppName);

    public static void Initialize(string directoryPath) =>
        AppSettingsService.Initialize(AppName, directoryPath);

    /// <summary>Closes the store and permits a later Initialize() (test hosts).</summary>
    public static void Shutdown() => AppSettingsService.Shutdown();

    public static AppSettingProperty<T> Wrap<T>(string property, T defaultValue) =>
        AppSettingsService.Wrap(property, defaultValue);

    public static T Get<T>(string property) => AppSettingsService.Get<T>(property);
    public static void Set(string key, object val) => AppSettingsService.Set(key, val);

    public static void AddPropertyHandler(string propertyName, EventHandler<AppSettingChangedEventArgs> handler) =>
        AppSettingsService.AddSettingHandler(propertyName, handler);
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
/// <summary>The settings.sqlite key holding the user's chosen assets folder.</summary>
public const string AssetsFolderKey = "KenneyAssetBrowser.Settings.AssetsFolder";

/// <summary>The settings.sqlite key holding the file name of the last-browsed bundle.</summary>
public const string LastBundleKey = "KenneyAssetBrowser.Settings.LastBundleFile";
```

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Settings/Pinta.Brix.Settings.csproj -->
<!-- The settings machinery (store, typed properties, change events, backup/
     import/export) is provided by the CodeBrix.Platform.AppSettings add-in;
     this library is the thin Pinta.Brix-named facade over it. -->
```

**Where to look.**
`KenneyAssetBrowser/src/libs/KenneyAssetBrowser.Settings/SettingsService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Settings/SettingsService.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Settings/Pinta.Brix.Settings.csproj`

**Sharp edges.**
- The add-in supplies the whole store - typed properties, change events, startup
  auto-backup and pruning, corruption recovery, import and export. Do not
  re-implement any of it; the facade exists only to name it after your
  application.
- Initialization runs the startup backup and prune, so it belongs before any UI
  renders and before anything reads a setting.
- The store is process-global, so a test host needs the shutdown call, or a
  throwaway directory, to re-initialize between cases.
- A companion logging facade forwards to the add-in's logging service, so the
  settings backend's diagnostics reach the same sinks as the rest of the
  application.
- Keep the layering rule in a project-file comment: every persisted value goes
  through the settings library, and it is the only project that takes the storage
  dependency.

### Open the settings store before any other startup work

**When you want this.** A static type in one of your libraries reads a setting
from its own static constructor, so ordering is not optional.

**The MVVM shape.** The `App` constructor opens the store as its first real step,
before `InitializeComponent()`; the ordering comment travels with the call.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Open (or silently create) the single portable settings.sqlite store -
//including its startup auto-backup and pruning - before anything reads
//a setting. PintaCore's static constructor builds the palette manager,
//which reads settings, so this must come first.
Pinta.Brix.Settings.SettingsService.Initialize();
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`

**Also shown by.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/App.xaml.cs` (opened after the
container and before `InitializeComponent()`, because the page's view model reads
a setting in its own constructor)

**Sharp edges.**
- The failure is quiet and order-dependent: a static constructor that runs before
  the store exists gets defaults instead of the user's values.
- Store creation is silent on first run: no dialog, no error.

### Choose a folder with the picker and remember it across runs

**When you want this.** The application needs a user-chosen location, and the
choice should be the last thing the user ever has to do about it.

**The MVVM shape.** An async command on the view model opens the picker, writes
the result through the settings facade, and raises change notifications for every
derived property - including the visibility properties that swap a first-launch
prompt for the real content.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs
public bool HasAssetsFolder => !string.IsNullOrWhiteSpace(_assetsFolder);

public string AssetsFolderLabel => HasAssetsFolder ? _assetsFolder : "Choose assets folder…";

public Visibility FolderPromptVisibility => HasAssetsFolder ? Visibility.Collapsed : Visibility.Visible;

public Visibility CatalogAreaVisibility => HasAssetsFolder ? Visibility.Visible : Visibility.Collapsed;

public SimpleCommand PickFolderCommand => field ??=
    new SimpleCommand((Func<object, Task>)(_ => PickFolderAsync()));

private async Task PickFolderAsync()
{
    var picker = new FolderPicker
    {
        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
    };
    picker.FileTypeFilter.Add("*");

    var folder = await picker.PickSingleFolderAsync();
    if (folder == null) { return; }

    _assetsFolder = folder.Path;
    SettingsService.Set(AssetsFolderKey, _assetsFolder);
    NotifyPropertyChanged(nameof(HasAssetsFolder));
    NotifyPropertyChanged(nameof(AssetsFolderLabel));
    NotifyPropertyChanged(nameof(FolderPromptVisibility));
    NotifyPropertyChanged(nameof(CatalogAreaVisibility));

    await ReloadCatalogAsync();
}
```

**Where to look.**
`KenneyAssetBrowser/src/KenneyAssetBrowser.Core/ViewModels/MainViewModel.cs`
`KenneyAssetBrowser/src/KenneyAssetBrowser.UI/Views/MainPage.xaml`

**Sharp edges.**
- The filter call is required even for a folder picker.
- A cancelled picker returns null; the command returns without touching state.
- Bind the same command from both the first-launch prompt and the header button,
  so there is one code path either way.
- On the LinuxFrameBuffer head the picker exists only because that head opted into
  it; see the startup area.
- A path a picker returns may need decoding before it is stored; see the bridge
  area.

### Restore a remembered window size before any window exists

**When you want this.** You want the application to reopen at the size the user
left it, and the head creates the native window before your page loads.

**The MVVM shape.** A settings read in the `App` constructor feeding the
platform's preferred launch size, plus a write-through handler on the window's
size-changed event. The scale conversion is the part that is easy to get wrong.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Restore the persisted window size BEFORE any window exists - the
//Skia heads consult ApplicationView.PreferredLaunchViewSize when they
//create the native window, and that is the only public seam for the
//initial size. Setting names and the 1100x750 defaults match
//upstream. The maximized flag is not restored: the platform exposes
//no public presenter state on the Skia heads.
int windowWidth = Pinta.Brix.Settings.SettingsService.Get("window-size-width", 1100);
int windowHeight = Pinta.Brix.Settings.SettingsService.Get("window-size-height", 750);
Windows.UI.ViewManagement.ApplicationView.PreferredLaunchViewSize =
    new Windows.Foundation.Size(windowWidth, windowHeight);
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs
//Write-through persistence of the window size; the store ignores
//writes when the value is unchanged. args.Size is in logical units
//but the X11 head consumes PreferredLaunchViewSize as NATIVE pixels,
//so the stored value must be native pixels or every restart would
//rescale the window by the display-scale factor.
MainWindow.SizeChanged += (_, args) =>
{
    if (MainWindow.Content?.XamlRoot is not { } root) { return; }

    double scale = root.RasterizationScale;
    Pinta.Brix.Settings.SettingsService.Set("window-size-width", (int)Math.Round(args.Size.Width * scale));
    Pinta.Brix.Settings.SettingsService.Set("window-size-height", (int)Math.Round(args.Size.Height * scale));
};
```

**Where to look.**
`Pinta.Brix/src/Pinta.Brix.UI/App.xaml.cs`

**Sharp edges.**
- The size-changed event reports logical units while the preferred launch size is
  consumed as native pixels on the X11 head. Multiply by the root's rasterization
  scale on the way in, or the window shrinks or grows at every restart on a scaled
  display.
- There is no public presenter state on the Skia heads, so a maximized flag cannot
  be restored - only the size.
- Write-through on every resize is cheap because the store skips unchanged values.

### Persist small pieces of application state through the same store

**When you want this.** A palette, a recent list, a last-used value - state that
should survive a restart without inventing a file format.

**The MVVM shape.** The owning manager reads its state from the settings service
on construction and writes it back on change; the values are serialized through
the same store as everything else, and the keys live in one constants class.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/PaletteManager.cs
// Pinta.Brix note: upstream kept the working palette in a palette.txt file
// beside settings.xml. Everything persisted now lives in settings.sqlite, so
// the palette is a setting like any other - stored as its list of colours.
// (Edit > Palette > Save As still writes a real file, but only where the
// user asks for one: that is an export, not application state.)
private const string CURRENT_PALETTE_KEY = "current-palette";
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/PaletteManager.cs
private void SaveColors ()
{
	// Primary / Secondary colors
	settings.PutSetting (SettingNames.PRIMARY_COLOR, PrimaryColor.ToHex ());
	settings.PutSetting (SettingNames.SECONDARY_COLOR, SecondaryColor.ToHex ());

	// Recently used palette
	settings.PutSetting (SettingNames.RECENT_COLORS, recently_used.Select (c => c.ToHex ()).ToArray ());
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/SettingNames.cs
internal static class SettingNames
{
	internal const string DEFAULT_IMAGE_TYPE = "default-image-type";
	internal const string JPG_QUALITY = "jpg-quality";
	// ...
	internal static string ToolAntialias (BaseTool tool)
		=> $"{tool.GetType ().Name.ToLowerInvariant ()}-antialias";
}
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/PaletteManager.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/SettingNames.cs`

**Sharp edges.**
- Setting keys live in one constants class, including a convention for per-item
  keys derived from a type name, so a key is never spelled twice.
- The store serializes values as JSON, so an array round trips directly and no
  packing convention is needed.
- Reads use a default that is also the application's default, so a missing key and
  a fresh install behave identically.

### Flush deferred settings at natural points instead of at quit

**When you want this.** Components push their state on a "save before quit" event,
in an application that has no quit path.

**The MVVM shape.** Keep the event, but raise it at points where the state has
naturally settled - a tool change, a document close - rather than only at exit.
Every write goes straight through to the store.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/SettingsManager.cs
// Pinta.Brix note: upstream kept its settings in an in-memory dictionary that
// was serialised to settings.xml ONCE, on quit. This port stores everything in
// the single portable settings.sqlite instead (see Pinta.Brix.Settings), and
// every PutSetting WRITES THROUGH IMMEDIATELY - so nothing is lost when the
// application is closed from the window's own chrome, which is the only way it
// can be closed here (there is deliberately no File > Quit).
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/SettingsManager.cs
/// <remarks>
/// Safe and cheap to call often: each PutSetting is a single upsert, and the
/// store does nothing at all when the value has not changed.
/// </remarks>
public void DoSaveSettingsBeforeQuit ()
{
	try {
		SaveSettingsBeforeQuit?.Invoke (this, EventArgs.Empty);
	} catch (Exception ex) {
		// Flushing settings must never take the application down.
		LoggingService.LogError ("Settings could not be saved", ex);
	}
}
```

```csharp
// From CodeBrix.Samples/Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ToolManager.cs
// Pinta.Brix note: the ported tools push their option values from
// inside SaveSettingsBeforeQuit rather than as they change, and this
// application has no quit path - the window's own chrome closes it.
// Flushing on every tool change means a tool's options reach
// settings.sqlite while the user is still working.
PintaCore.Settings.DoSaveSettingsBeforeQuit ();
```

**Where to look.**
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/SettingsManager.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/Managers/ToolManager.cs`

**Sharp edges.**
- A quit-only flush loses everything on a head with no quit command; find the
  natural settle points instead.
- The flush is wrapped so a failing subscriber cannot take the application down.
- Frequent flushing is only cheap because the store skips unchanged values.

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

## Hosting a game engine

### Hand the view model a game canvas at its first real layout size

**When you want this.** You want the CodeBrix.Platform GameEngine loop rendering
inside an ordinary page, and the engine can only be started against a surface that
already has a non-zero size - which, for a canvas that starts hidden, is the first
time it is shown.

**The MVVM shape.** The view model declares an interface with one method taking
the canvas. The page forwards the canvas's first-started event to it in a single
line. The view model builds its scene object and starts it; no engine code lives
in the code-behind.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
/// <summary>
/// Lets the hosting page tell the view model when the visualizer's game canvas has its
/// first real layout size - the engine can only start against a non-zero surface, which
/// happens the first time Visualize Mode is shown.
/// </summary>
public interface IManageGameCanvas
{
    /// <summary>Called once, on the UI thread, at the canvas's FirstStarted event.</summary>
    /// <param name="canvas">The game canvas the visualizer renders into.</param>
    void CanvasFirstStart(GameSurfaceCanvas canvas);
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs
//Fires once, at the canvas's first non-zero layout size - i.e. the first time
//  Visualize Mode is shown - which is when the engine can start
VisualizerCanvas.FirstStarted += (_, _) => _gameCanvasManager?.CanvasFirstStart(VisualizerCanvas);
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
public void CanvasFirstStart(GameSurfaceCanvas canvas)
{
    //UI thread, the first time Visualize Mode is shown with a real size: build the
    //  shader scene and start the engine. Later mode switches pause and resume it.
    _visualizerSession = new VisualizerSession(canvas);
    _visualizerSession.Start();
}
```

```xml
<!-- From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml -->
xmlns:game="clr-namespace:CodeBrix.Platform.GameEngine.Host.Rendering;assembly=CodeBrix.Platform.GameEngine.Host"
...
<game:GameSurfaceCanvas x:Name="VisualizerCanvas"
                        Visibility="{d:Binding IsCameraMode, Converter={StaticResource VisibleWhenFalse}}" />
```

**Where to look.**
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`
`PalmVisualizer/src/PalmVisualizer.UI/Views/MainPage.xaml.cs` and
`Views/MainPage.xaml`

**Sharp edges.**
- The first-started event fires once. Starting the engine from the page's loaded
  event, or from the command that switches modes, would run against a zero-sized
  surface.
- The order inside the command matters: making the canvas visible - which is what
  raises the event the first time - comes before resuming the session, and the
  resume is null-safe because on the first pass the session does not exist yet.
- Passing the canvas type through a view-model interface keeps the sample short
  but does put a view type in the view model's signature; a bridge that hands over
  only what the session needs is the cleaner shape.
- The page captures the interface in its data-context-changed handler and calls it
  null-safely.

### Run and pause a game engine session inside a page

**When you want this.** The engine loop should run while one part of the UI is on
screen and cost nothing while the user is elsewhere, without tearing the scene
down and rebuilding it.

**The MVVM shape.** A session class owns the engine lifecycle and exposes start,
pause, resume, stop and a thread-safe data-in method. The view model calls those
from its commands and from `Dispose()`. Nothing else touches the engine instance.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs
public void Start()
{
    if (IsStarted) { return; }

    //GpuRendering-OpenGL (GPU) by default; must be chosen before the first access to Host. The
    //  render resolution tracks the window (no SetRenderResolution) - the shader
    //  scene is resolution-independent.
    _canvas.UseGpuRendering = Environment.GetEnvironmentVariable("PALMVISUALIZER_USE_CPU") != "1";

    _renderSurface = _canvas.Host;
    _renderSurface.ViewManager.ConfigureSingleFullView();

    Engine.Instance.Start(SynchronizationContext.Current);
    Engine.Instance.Configuration.TargetFPS = 60;

    var adapter = _renderSurface.RenderSurfaceAdapter;
    var view = _renderSurface.ViewManager.Views[0];

    _backdrop = new EtherealBackdrop(_renderSurface, view,
        new Rectangle(0, 0, adapter.Width, adapter.Height), _attractorField);
    _backdrop.ZOrder = 0;

    //The render resolution tracks the window, so follow adapter resizes
    adapter.Resized += OnAdapterResized;

    IsStarted = true;
}

public void Pause()
{
    if (!IsStarted || Engine.Instance.IsPaused) { return; }

    _attractorField.Reset();
    Engine.Instance.Pause();
}

public void Resume()
{
    if (!IsStarted || !Engine.Instance.IsPaused) { return; }

    Engine.Instance.Resume();
}

public void Stop()
{
    if (!IsStarted) { return; }

    _renderSurface.RenderSurfaceAdapter.Resized -= OnAdapterResized;
    Engine.Instance.Stop();
    IsStarted = false;
}

private void OnAdapterResized(RenderSurfaceAdapterResizedEventArgs args)
{
    if (_backdrop != null)
        _backdrop.ScreenBounds = new Rectangle(0, 0, args.NewWidth, args.NewHeight);
}
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs
private Task DoVisualize()
{
    if (!CanVisualize()) { return Task.CompletedTask; }

    if (_tracker == null)
    {
        _tracker = new PalmTracker();
        _tracker.TrackingUpdated += OnTrackingUpdated;
    }
    _tracker.Start();
    _reportedOpenPalmCount = 0;

    //Showing the game canvas gives it its first real layout size, which raises its
    //  FirstStarted -> CanvasFirstStart the first time through; on later entries the
    //  engine is merely paused from Camera Mode, so wake it back up
    IsCameraMode = false;
    _visualizerSession?.Resume();

    StatusText = "Show the camera your open palm - the colors will gather toward it.";
    return Task.CompletedTask;
}

private Task DoGoBack()
{
    if (!CanGoBack()) { return Task.CompletedTask; }

    _tracker?.Stop();
    _visualizerSession?.Pause();

    IsCameraMode = true;
    InvalidatePreviewCanvas?.Invoke();
    StatusText = SelectedCamera != null
        ? $"Live: {SelectedCamera.FriendlyName}"
        : "Select a camera.";
    return Task.CompletedTask;
}
```

**Where to look.**
`PalmVisualizer/src/libs/PalmVisualizer.Rendering/VisualizerSession.cs`
`PalmVisualizer/src/PalmVisualizer.Core/ViewModels/MainViewModel.cs`

**Sharp edges.**
- The GPU-or-CPU choice must be made before the first access to the canvas's host;
  reading the host first locks the choice in.
- The engine is started with the current synchronization context, so it must be
  called on the UI thread - which is guaranteed here because it runs from the
  canvas's first-started event.
- Starting is once per process. Use pause and resume to leave and re-enter the
  mode; every one of the four methods is guarded by the started flag and by the
  engine's own paused flag, so double calls are harmless.
- The pause is invisible to engine time, so a resumed scene picks up mid-motion.
  Resetting the scene's input state on pause is what makes it resume undisturbed
  rather than with stale input still acting on it.
- Configuring a single full view plus a zero draw order is the whole scene graph
  here; the backdrop fills the one view.
- Subscribe to the render adapter's resize event and update the drawing's screen
  bounds; the render resolution tracks the window because the resolution is
  deliberately not pinned.
- Stopping unsubscribes the resize handler before stopping the engine.

## Testing

### Keep view model rules in a plain class so they can be tested

**When you want this.** You want your rules covered, and a `SimpleViewModel`
cannot be constructed without a running application host.

**The MVVM shape.** Every decision lives in a static class of plain methods over
plain values; the view model is a thin observable wrapper that calls them and
raises change notifications. The view model keeps the wiring, the collections and
the commands; the class keeps the answers.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/PlaybackSelection.cs
/// <remarks>
/// The rules live here rather than inside the view model because a view model derived from the
/// platform's SimpleViewModel cannot be constructed without a running application host, and rules
/// that cannot be tested are rules that quietly stop being true. The view model is a thin observable
/// wrapper over this.
/// </remarks>
public static class PlaybackSelection
{
    public static bool CanOpen(SourceMediaInfo item) =>
        item is not null && MediaFormats.IsPlayable(item.Format);

    public static string DescribeUnplayable(SourceMediaInfo item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return $"{MediaFormats.DisplayName(item.Format)} is not played in this application - " +
               "import it to one of the four CodeBrix formats first.";
    }

    // ... BuildChapterRows, BuildCaptionRows, ShouldShowChapters, ShouldShowCaptions, DescribeOpened ...
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/ViewModels/PlaybackViewModel.cs
public void Open(SourceMediaInfo item)
{
    Close();

    if (item is null)
    {
        return;
    }

    CurrentItem = item;

    if (!PlaybackSelection.CanOpen(item))
    {
        IsUnplayableFormat = true;
        StatusText = PlaybackSelection.DescribeUnplayable(item);
        return;
    }

    if (surface is null)
    {
        StatusText = "The player is not ready yet.";
        return;
    }

    StatusText = $"Opening {item.FileName}...";
    surface.Open(item.Path);
}
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/Services/PlaybackSelection.cs`
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Playback/ViewModels/PlaybackViewModel.cs`
`CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Playback.Tests/PlaybackSelectionTests.cs`

**Sharp edges.**
- The constraint is stated in three places in that application - the two class
  remarks and the test project file comment - which is a good sign it is the real
  organizing principle.
- The parts that cannot be reduced this way (the panel itself, the player element)
  are covered by a scripted run on a real head instead; see the last blueprint in
  this area.

### Set up an xUnit v3 test project for a CodeBrix library

**When you want this.** You are adding the first test project to an application
and want it to match the family conventions and actually be discovered.

**The MVVM shape.** Not applicable; project setup. A `global.json` at the
application root selects the runner for every project below it.

**Code.**

```xml
<!-- Adapted from CodeBrix.Samples/PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/PalmVisualizer.Rendering.Tests.csproj
     (package ids and versions elided - see the project's csproj) -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <!-- xUnit.net v3 test projects are self-executing binaries and
       must build as Exe; run via Microsoft.Testing.Platform,
       matching the CodeBrix family test convention. -->
  <OutputType>Exe</OutputType>
  <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
  <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="..\..\..\src\libs\PalmVisualizer.Rendering\PalmVisualizer.Rendering.csproj" />
</ItemGroup>

<ItemGroup>
  <PackageReference Include="(xunit.v3, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk)" />
  <PackageReference Include="(SilverAssertions)" />
</ItemGroup>
```

```text
// From CodeBrix.Samples/PalmVisualizer/global.json
{
    "test": {
        "runner": "Microsoft.Testing.Platform"
    }
}
```

Test bodies follow the family style - `<Class>Tests.cs`, snake_case method names,
`//Arrange` / `//Act` / `//Assert` comments, and the assertions library's
`Should()`:

```csharp
// From CodeBrix.Samples/PalmVisualizer/tests/libs/PalmVisualizer.Camera.Tests/WebcamCaptureServiceTests.cs
[Fact]
public void TryCopyLatestFrame_returns_false_before_any_frame()
{
    //Arrange
    using var service = new WebcamCaptureService();
    byte[] buffer = null;

    //Act
    bool copied = service.TryCopyLatestFrame(ref buffer, out int width, out int height);

    //Assert
    copied.Should().Be(false);
    width.Should().Be(0);
    height.Should().Be(0);
}
```

**Where to look.**
`PalmVisualizer/global.json` and the three project files under
`PalmVisualizer/tests/libs/`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/PdfSideBySide.PdfRender.Tests.csproj`
`KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Rendering.Tests/KenneyAssetBrowser.Rendering.Tests.csproj`

**Also shown by.**
`NotionDocumentCreator`, `WebcamPainter`, `Pinta.Brix`, `WikipediaPublisher`,
`CodeBrixVideoTool` - these test projects carry the same two properties and
the same comment. (`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests` sets
neither property explicitly.)

**Sharp edges.**
- The output type must be `Exe`. The comment appears in every one of these
  projects: xUnit v3 test projects are self-executing binaries, and a library
  test project will not run.
- Because the runner is the Microsoft Testing Platform, `dotnet test` can report
  that it discovered no tests on some SDK builds; running the built test
  executable directly always works.
- Two applications (`PdfSideBySide` and `CodeBrixVideoTool`) have no `global.json`
  at all, so the runner is selected by the project properties alone. Adding the
  `global.json` shown above matches the rest of the repository.
- Every async test passes `TestContext.Current.CancellationToken` to the method
  under test; it satisfies the analyzer that flags a missing token and makes the
  test cancellable. A test that waits on a background thread passes it to the
  wait as well.
- Enabling nullable annotations in a test project is worth a comment when the
  library under test is annotated, or a ported test file raises a wave of
  warnings about redundant annotations.

### Build a test project against real CodeBrix Platform assemblies

**When you want this.** Your test project references a library that references
CodeBrix.Platform, and calls into platform types.

**The MVVM shape.** Not a view-model concern. One project property.

**Code.**

```xml
<!-- From CodeBrix.Samples/Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/Pinta.Brix.Engine.Tests.csproj -->
<!-- The published CodeBrix.Platform nuget ships REFERENCE assemblies in
     lib/; every method body throws NotSupportedException("Ref assembly").
     Application heads get the real implementations swapped in
     automatically, plain test projects do NOT. This is the lever that
     swaps them in, and without it every text-layout call would compile
     cleanly and then throw on first use. -->
<CodeBrixRuntimeIdentifier>skia</CodeBrixRuntimeIdentifier>
```

```xml
<!-- From CodeBrix.Samples/CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Playback.Tests/CodeBrixVideoTool.Playback.Tests.csproj -->
<!-- ... Note that even
     with the real assemblies present a SimpleViewModel cannot be constructed here, because its
     dispatcher needs a running application host; the view models are exercised by the
     application's own scripted run instead, and the rules under them live in plain classes
     these tests can reach. -->
<CodeBrixRuntimeIdentifier>skia</CodeBrixRuntimeIdentifier>
```

**Where to look.**
`Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/Pinta.Brix.Engine.Tests.csproj`
`Pinta.Brix/tests/libs/Pinta.Brix.Effects.Tests/Pinta.Brix.Effects.Tests.csproj`
`CodeBrixVideoTool/tests/libs/` (both project files)

**Sharp edges.**
- Without the property, platform calls compile cleanly and then throw at run time.
  The failure looks like a test bug rather than a build-configuration one.
- The property does not lift the view-model construction limit: a `SimpleViewModel`
  still needs a running application host. Put the rules in plain classes (first
  blueprint in this area) and drive the view models from a scripted run (last
  blueprint in this area).

### Add the native assets a head would have supplied

**When you want this.** Your library binds to a native runtime - Skia, text
shaping, computer vision - and the tests exercise it for real.

**The MVVM shape.** Not a view-model concern. The test project references the
native package for the current operating system, with an MSBuild platform
condition.

**Code.**

```xml
<!-- Adapted from CodeBrix.Samples/WebcamPainter/tests/libs/WebcamPainter.Vision.Tests/WebcamPainter.Vision.Tests.csproj
     (package IDs and versions removed - see the project's csproj) -->
<ItemGroup>
  <!-- The tests run real TFLite inference - the native OpenCV library must be present -->
  <PackageReference Include="(OpenCV native, Linux x64)"   Condition="$([MSBuild]::IsOSPlatform('Linux'))" />
  <PackageReference Include="(OpenCV native, Windows x64)" Condition="$([MSBuild]::IsOSPlatform('Windows'))" />
  <PackageReference Include="(OpenCV native, macOS arm64)" Condition="$([MSBuild]::IsOSPlatform('OSX'))" />
  <PackageReference Include="(OpenCV native, macOS x64)"   Condition="$([MSBuild]::IsOSPlatform('OSX'))" />
</ItemGroup>
```

```xml
<!-- Adapted from CodeBrix.Samples/Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/Pinta.Brix.Engine.Tests.csproj -->
<!-- The engine's SkiaSharp reference is managed-only; on Linux the native
     libSkiaSharp must be pulled in explicitly for the tests to run. -->
<!-- Text layout shapes with HarfBuzz, so its native library is needed here
     for the same reason libSkiaSharp is: an application head gets these
     from its runtime package, a bare test project does not. -->
```

**Where to look.**
`WebcamPainter/tests/libs/WebcamPainter.Vision.Tests/` and
`WebcamPainter.Painting.Tests/` project files
`Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/Pinta.Brix.Engine.Tests.csproj`
`KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Rendering.Tests/KenneyAssetBrowser.Rendering.Tests.csproj`
`PalmVisualizer/tests/libs/PalmVisualizer.Rendering.Tests/PalmVisualizer.Rendering.Tests.csproj`

**Sharp edges.**
- In a running application the head's runtime package supplies these; a bare test
  host does not. The list is exactly what your library touches: the graphics
  native for anything that rasterizes, the shaping native for anything that lays
  out text, the vision native for anything that infers.
- Conditioning on the operating system only brings in the host architecture's
  package; a build machine on another architecture needs its own reference added.
- Shader tests are worth this on their own: compiling and evaluating real shader
  source on raster surfaces needs the native library present, with no engine, no
  window and no GPU.

### Expose library internals to its test project

**When you want this.** A library keeps its implementation types internal and you
want to unit test them without widening the public surface.

**The MVVM shape.** One file per library, named after what it does, with one
attribute naming that library's test project.

**Code.**

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/InternalsVisibleTo.cs
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PalmVisualizer.Vision.Tests")]
```

```csharp
// From CodeBrix.Samples/PalmVisualizer/src/libs/PalmVisualizer.Vision/Internal/PalmDetector.cs
/// <summary>Exposed for unit tests: the anchor grid's X centers.</summary>
internal static float[] TestAnchorsX => AnchorsX;

/// <summary>Exposed for unit tests: the anchor grid's Y centers.</summary>
internal static float[] TestAnchorsY => AnchorsY;
```

**Where to look.**
`PalmVisualizer/src/libs/*/InternalsVisibleTo.cs`
`WebcamPainter/src/libs/*/InternalsVisibleTo.cs`
`Pinta.Brix/src/libs/Pinta.Brix.Engine/InternalsVisibleTo.cs`
`WikipediaPublisher/WikipediaPublisher.RenderArticle/InternalsVisibleTo.cs`

**Also shown by.**
`NotionDocumentCreator`, `KenneyAssetBrowser`, `CodeBrixVideoTool`,
`PolyHavenBrowser` (where it is also what lets a test reach the client factory's
internal constructor).

**Sharp edges.**
- Every library that has tests carries the file, even one whose tests only touch
  public members; the convention is applied uniformly.
- When a test needs a value that is otherwise private, add a documented internal
  test accessor rather than making the field itself visible.
- Factoring one step of an expensive operation into an internal static method -
  compiling a shader, loading an embedded model - is what lets the test call it
  with nothing else running.

### Test a service the way the container builds it

**When you want this.** Unit tests that resolve the service under test the same
way the application does, with the same constructor dependencies.

**The MVVM shape.** A reusable test fixture base builds a small service
collection, exposes a typed resolve method, and offers one virtual registration
hook. The test project subclasses it once and registers what its tests need; test
classes take the subclass as a class fixture.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/EncryptionTestingFixture.cs
public class EncryptionTestingFixture : SimpleTestFixture
{
    protected override void RegisterCustomServices(
        IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration config,
        Func<IServiceProvider> serviceResolver)
    {
        //Register my custom testing services here
        services.AddSingleton<IEncryptionService>(_ =>
            new EncryptionService(serviceResolver().GetService<ILogger<EncryptionService>>()));
    }
}
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs
public class EncryptionServiceTests : IClassFixture<EncryptionTestingFixture>
{
    private readonly EncryptionTestingFixture _fixture;
    private readonly ITestOutputHelper _output;

    private IEncryptionService GetService() => _fixture.GetService<IEncryptionService>() as EncryptionService;

    public EncryptionServiceTests(EncryptionTestingFixture fixture,
        ITestOutputHelper output)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _output = new SimpleTestOutputHelper(output);

        fixture.CreateAndRegisterLogger<EncryptionService>(_output);
    }

    [Fact]
    public void can_get_service() => GetService().Should().NotBeNull();

    [Fact]
    public async Task GetDefaultKey_retrieves_key() =>
        (await GetService().GetDefaultKey()).Should().NotBeNullOrEmpty();
}
```

```xml
<!-- From CodeBrix.Samples/JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/JustBetweenUs.Encryption.Tests.csproj -->
<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Debug|AnyCPU'">
  <DefineConstants>$(DefineConstants);SIMPLE_OUTPUT_LOGGING</DefineConstants>
</PropertyGroup>
<ItemGroup>
  <Compile Include="..\..\Shared\Testing\SimpleTestFixture.cs" Link="SimpleTestFixture.cs" />
</ItemGroup>
```

**Where to look.**
`JustBetweenUs/Shared/Testing/SimpleTestFixture.cs`
`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/EncryptionTestingFixture.cs`
`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs`

**Also shown by.**
`WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/` (the same
fixture file, linked in).

**Sharp edges.**
- The fixture is a single file linked into the test project, not a package, and it
  is feature-gated by two compilation constants - one for the test-output logging,
  one for an HTTP client factory stub. Define them, or build in the configuration
  that defines them, because the test classes use the gated types unconditionally.
- The fixture also scans its own assembly for registration classes and calls their
  registration methods after checking that each names this fixture, which gives a
  second hook for tests that want their setup in a separate file.
- It reads optional settings files from the working directory and honors the
  environment name variable, defaulting to Development.
- Resolving a type that was never registered throws rather than returning null, so
  a missing registration fails the test with a readable message.

### Route logging from the code under test into test output

**When you want this.** The service under test logs through a logger abstraction
and you want those lines in the test report.

**The MVVM shape.** The fixture holds a logger factory that wraps the test
framework's output helper; the test class registers a logger for the type it is
testing in its constructor, and the fixture hands that logger out whenever the
container is asked for a logger.

**Code.**

```csharp
// From CodeBrix.Samples/JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs
_output = new SimpleTestOutputHelper(output);
fixture.CreateAndRegisterLogger<EncryptionService>(_output);
```

```csharp
// From CodeBrix.Samples/JustBetweenUs/Shared/Testing/SimpleTestFixture.cs
private void WriteText(string text, bool withEndOfLine = false)
{
    if (text != null)
    {
        // ...
        if (AlwaysWriteToConsole
            || (_wrappedOutput == null)
            || (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))) //Need to write test output to console on Linux
        {
            if (withEndOfLine) { Console.WriteLine(text); }
            else { Console.Write(text); }
        }
        else
        {
            try
            {
                //Note: writing to ITestOutputHelper can fail if the test has already completed
                if (withEndOfLine) { _wrappedOutput.WriteLine(text); }
                else { _wrappedOutput.Write(text); }
            }
            catch (Exception)
            {
                if (withEndOfLine) { Console.WriteLine(text); }
                else { Console.Write(text); }
            }
        }
    }
}
```

**Where to look.**
`JustBetweenUs/Shared/Testing/SimpleTestFixture.cs`
`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs`

**Sharp edges.**
- Two platform notes are baked into the wrapper: the test output helper does not
  reliably reach the console on Linux, so output goes to the console there
  instead; and writing to it after a test has completed throws, so every write is
  wrapped and falls back to the console.
- The logger registration refuses an open generic type, because the logger key is
  built from the type's full name.
- Diagnostic output is also how a probe-style test earns its place: writing
  environment strings through the output helper lets an environment-specific
  failure be pinned before anyone edits platform code.

### Share one expensive fixture across every test class that needs it

**When you want this.** Setup that takes real work - generating media, running
imports, probing the results - and must not run once per test class.

**The MVVM shape.** Not a view-model concern. An async-lifetime fixture plus a
collection definition; each test class takes the fixture in its constructor.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Processing.Tests/SampleMediaFixture.cs
public sealed class SampleMediaFixture : IAsyncLifetime
{
    public string Root { get; } = Path.Combine(
        Path.GetTempPath(), "CodeBrixVideoTool.Tests", Guid.NewGuid().ToString("N"));

    // ... Mp4Path, CaptionsPath, ChaptersPath, RichMp4Path, Mode2Path, Mode1Path ...

    public async ValueTask InitializeAsync()
    {
        Directory.CreateDirectory(Root);

        await SampleClipFactory.WriteMp4Async(Mp4Path, Width, Height, Duration).ConfigureAwait(false);
        SampleClipFactory.WriteWebVtt(CaptionsPath, Duration);
        SampleClipFactory.WriteChapterMetadata(ChaptersPath, Duration);

        // ... mux the three into RichMp4Path ...

        var probe = new MediaProbe();
        var runner = new ConversionRunner();

        RichMp4Info = await probe.ProbeAsync(RichMp4Path, CancellationToken.None).ConfigureAwait(false);

        await ImportAsync(probe, runner, MediaFormatKind.CodeBrixMode2, Mode2Path).ConfigureAwait(false);
        await ImportAsync(probe, runner, MediaFormatKind.CodeBrixMode1, Mode1Path).ConfigureAwait(false);

        Mode2Info = await probe.ProbeAsync(Mode2Path, CancellationToken.None).ConfigureAwait(false);
        Mode1Info = await probe.ProbeAsync(Mode1Path, CancellationToken.None).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            if (Directory.Exists(Root)) { Directory.Delete(Root, true); }
        }
        catch (IOException)
        {
            //A temporary folder that will not delete is not worth failing a test run over.
        }

        return ValueTask.CompletedTask;
    }
}

/// <summary>Shares one <see cref="SampleMediaFixture" /> across every test class that needs media.</summary>
[CollectionDefinition(Name)]
public sealed class SampleMediaCollection : ICollectionFixture<SampleMediaFixture>
{
    public const string Name = "sample media";
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Processing.Tests/ConversionRunnerTests.cs
[Collection(SampleMediaCollection.Name)]
public class ConversionRunnerTests
{
    private readonly SampleMediaFixture media;

    public ConversionRunnerTests(SampleMediaFixture media) => this.media = media;

    [Theory]
    [InlineData(MediaFormatKind.Matroska)]
    [InlineData(MediaFormatKind.WebM)]
    [InlineData(MediaFormatKind.CodeBrixMode1)]
    [InlineData(MediaFormatKind.CodeBrixMode2)]
    public async Task an_import_writes_every_one_of_the_four_formats(MediaFormatKind destination)
    {
        //Arrange
        var output = Path.Combine(media.Root, "import-" + destination + MediaFormats.Extension(destination));
        var plan = ConversionPlanner.Create(media.RichMp4Info, destination, output, null);

        //Act
        var outcome = await new ConversionRunner()
            .RunAsync(plan, null, TestContext.Current.CancellationToken);

        //Assert
        outcome.Succeeded.Should().BeTrue(outcome.Failure ?? "");
        File.Exists(output).Should().BeTrue();
        outcome.SizeInBytes.Should().BeGreaterThan(0);
    }
}
```

**Where to look.**
`CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Processing.Tests/SampleMediaFixture.cs`
`CodeBrixVideoTool/tests/libs/CodeBrixVideoTool.Processing.Tests/ConversionRunnerTests.cs`

**Sharp edges.**
- Keep the collection name as a constant on the definition class, so the attribute
  on each test class cannot be misspelled.
- Let the fixture's own setup use the production code path, so a break in the
  pipeline fails setup loudly rather than one test obscurely.
- The fixture writes everything under one uniquely named temporary folder and
  deletes it on disposal, swallowing the delete failure.

### Build the binary inputs your tests need instead of committing them

**When you want this.** You are testing a reader, a decoder or a renderer, and you
do not want binary fixtures in the repository.

**The MVVM shape.** Not applicable. A small internal builder in the test project
writes exactly the input each test needs, in memory or into a throwaway folder.

**Code.**

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Rendering.Tests/TestData/TestAssets.cs
/// <summary>Builders for tiny in-memory test assets (no files on disk, no network).</summary>
internal static class TestAssets
{
    /// <summary>Encodes an SKBitmap-drawn solid-color PNG.</summary>
    public static byte[] BuildPng(int width, int height, SKColor color)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        bitmap.Erase(color);
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return encoded.ToArray();
    }

    /// <summary>
    /// Builds a single-triangle .glb via SharpGLTF.Toolkit: vertices (0,0,0), (1,0,0),
    /// (0,1,0) with a red, double-sided material, optionally translated.
    /// </summary>
    public static byte[] BuildTriangleGlb(Vector3? translation = null) { /* ... */ }
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.AssetRead.Tests/TestZipBuilder.cs
internal static class TestZipBuilder
{
    public static void Build(string zipPath, IReadOnlyDictionary<string, byte[]> entries)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);
        using var fileStream = File.Create(zipPath);
        using var zipStream = new ZipOutputStream(fileStream);
        foreach (var (entryPath, bytes) in entries)
        {
            zipStream.PutNextEntry(new ZipEntry(entryPath) { Size = bytes.Length });
            zipStream.Write(bytes, 0, bytes.Length);
            zipStream.CloseEntry();
        }

        zipStream.Finish();
    }

    /// <summary>Encodes text as UTF-8 bytes for an entry.</summary>
    public static byte[] Text(string text) => Encoding.UTF8.GetBytes(text);
}
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/Helpers/TestPdfs.cs
/// <summary>A fresh, empty temp folder for one test's files.</summary>
public static string CreateTempFolder()
{
    var folder = Path.Combine(Path.GetTempPath(), "PdfSideBySide.PdfRender.Tests", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(folder);
    return folder;
}

/// <summary>
/// Writes a PDF with pageCount pages to folder as fileName; every page carries a
/// filled rectangle placed by page number so the pages are not blank.
/// </summary>
public static string WriteSamplePdf(string folder, string fileName, int pageCount)
{
    using var document = new PdfDocument();
    for (var i = 0; i < pageCount; i++)
    {
        var page = document.AddPage();
        using var graphics = XGraphics.FromPdfPage(page);
        graphics.DrawRectangle(XBrushes.Black, new XRect(50, 50 + i * 20, 200, 30));
    }

    var path = Path.Combine(folder, fileName);
    document.Save(path);
    return path;
}
```

**Where to look.**
`KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Rendering.Tests/TestData/TestAssets.cs`
`KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.AssetRead.Tests/TestZipBuilder.cs`
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/Helpers/TestPdfs.cs`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/TestData/TestAssets.cs`

**Also shown by.**
`PolyHavenBrowser_viewer_only` (its test assets hand-encode high-dynamic-range
image bytes as well as building a model file).

**Sharp edges.**
- A synthetic document has to differ page by page, or "different pages render to
  different images" is not testable; draw something placed by the page index.
- A fixture with whole-inch page dimensions lets the renderer tests assert exact
  pixel sizes rather than a tolerance.
- Give every test that writes files its own uniquely named folder, so tests using
  the same file name cannot collide and can run in parallel.
- Writing a deliberately corrupt input is how a warning path gets tested; the
  point of a warning list is that one bad file does not fail the whole load.
- A test project that builds an archive needs the compression library's writing
  side referenced explicitly, even when the library under test only reads.
- A fake image must be a real decodable image when the imaging back-end decodes
  eagerly; a placeholder byte array throws.
- Test classes that write files implement disposal and delete their temporary
  folder on a best-effort basis.
- A synthetic-document writer may reach its document library transitively; if that
  ever stops, the test project needs its own reference.

### Generate real media clips from a synthetic source

**When you want this.** You need real media to test against, and you do not want
binary files in the repository.

**The MVVM shape.** A factory in the production library, not the test project, so
the scripted run can use it too, writing clips from the media tool's own synthetic
sources into a folder the caller names.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Samples/SampleClipFactory.cs
var filterGraph = string.Create(CultureInfo.InvariantCulture,
    $"testsrc2=size={width}x{height}:rate={frameRate}[out0]; sine=frequency=440:sample_rate=48000[out1]");

Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));

var errors = new List<string>();
var succeeded = await FFMpegArguments
    .FromFileInput(filterGraph, false, input => input.ForceFormat("lavfi"))
    .OutputToFile(path, true, options => options
        .WithDuration(length)
        .WithVideoCodec("libx264")
        .WithConstantRateFactor(28)
        .WithSpeedPreset(Speed.UltraFast)
        .ForcePixelFormat("yuv420p")
        .WithAudioCodec("aac")
        .WithAudioBitrate(96)
        .ForceFormat("mp4"))
    .NotifyOnError(errors.Add)
    .CancellableThrough(cancellationToken)
    .ProcessAsynchronously(false)
    .ConfigureAwait(false);
```

**Where to look.**
`CodeBrixVideoTool/src/libs/CodeBrixVideoTool.Processing/Samples/SampleClipFactory.cs`
(`WriteMp4Async`, `WriteRichMp4Async`, `WriteWebVtt`, `WriteChapterMetadata`)

**Sharp edges.**
- The synthetic input is passed as a file input with the filter-graph format
  forced and existence checking turned off - a filter graph is not a file.
- The interesting case is built in two passes: a plain clip, then a mux that adds
  a caption track and a chapter metadata file, copying the media rather than
  re-encoding it, with the metadata mapping pointing at the right input.
- The class documentation states the discipline: nothing is copied from anywhere
  and nothing is left behind - every clip is written where the caller asks.

### Read a committed fixture from beside the test binary

**When you want this.** One input really has to be a file a real tool produced -
a document, a photograph, a page of markup - and the tests need to find it.

**The MVVM shape.** Not applicable. Either copy it to the output folder and locate
it from the base directory, or embed it in the test assembly and read it through a
shared helper.

**Code.**

```xml
<!-- From CodeBrix.Samples/PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/PdfSideBySide.PdfRender.Tests.csproj -->
<!-- Real-world PDF the tests open and render (a WikipediaPublisher sample) -->
<ItemGroup>
  <None Include="assets\**" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/Helpers/TestPdfs.cs
/// <summary>Full path of the assets/Inanna.pdf sample copied beside the test binary.</summary>
public static string InannaPath => Path.Combine(AppContext.BaseDirectory, "assets", "Inanna.pdf");
```

```xml
<!-- From CodeBrix.Samples/WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/WikipediaPublisher.RenderArticle.Tests.csproj -->
<ItemGroup>
  <Compile Include="..\..\Shared\Helpers\EmbeddedResourceHelper.cs" Link="Helpers\EmbeddedResourceHelper.cs" />
</ItemGroup>

<ItemGroup>
  <None Remove="Fixtures\cuneiform.html" />
  <EmbeddedResource Include="Fixtures\cuneiform.html" />
</ItemGroup>
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/Internal/ArticleParserTests.cs
private const string FixtureResource = "WikipediaPublisher.RenderArticle.Tests.Fixtures.cuneiform.html";

private static string _fixtureHtml;

private static async Task<ParsedArticle> ParseFixture()
{
    _fixtureHtml ??= await EmbeddedResourceHelper.GetResourceAsString(
        FixtureResource, typeof(ArticleParserTests).Assembly);
    return new ArticleParser(FixtureUrl).Parse(_fixtureHtml);
}
```

**Where to look.**
`PdfSideBySide/tests/libs/PdfSideBySide.PdfRender.Tests/Helpers/TestPdfs.cs`
`WikipediaPublisher/Shared/Helpers/EmbeddedResourceHelper.cs`
`WebcamPainter/tests/libs/WebcamPainter.Vision.Tests/HandTrackerTests.cs`
`PalmVisualizer/tests/libs/PalmVisualizer.Vision.Tests/`

**Sharp edges.**
- A copied fixture needs both halves: the copy item in the project file and the
  base-directory lookup in the test. Either one alone fails at run time.
- An embedded resource is named by the default namespace with folder separators
  replaced by dots; the shared helper also offers a path-based overload and a
  name-lookup method for when the exact name is uncertain.
- Parse an embedded fixture once into a static field and reuse it across the tests
  in the class.
- The same helper family is what an embedded font resolver mirrors, so fixtures,
  licenses and fonts are all reached the same way.

### Test a document renderer against the object model it produces

**When you want this.** Your library builds a document rather than returning a
value, and you want tests that are fast, offline and specific.

**The MVVM shape.** A test helper builds a themed document, a section and a
renderer over a fresh context, then walks the produced object model (including
into table cells) so each test asserts on styles and text rather than on a
rendered file.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/TestDom.cs
/// <summary>Creates a styled document, one section, and a renderer over a fresh context.</summary>
public static (Document Document, Section Section, RenderContext Context, BlockRenderer Renderer)
    CreateRenderer(Action<RenderContext> configure = null)
{
    var theme = BookTheme.For(PageSizeOption.EightByTen);
    var document = new Document();
    BookStyles.Define(document, theme);
    var section = document.AddSection();
    var context = new RenderContext { Theme = theme };
    configure?.Invoke(context);
    return (document, section, context, new BlockRenderer(context));
}

/// <summary>Every paragraph in the section, including paragraphs inside table cells.</summary>
public static List<Paragraph> AllParagraphs(Section section) =>
    CollectParagraphs(section.Elements).ToList();

/// <summary>The concatenated plain text of one paragraph (line breaks become \n).</summary>
public static string TextOf(Paragraph paragraph) => TextOfElements(paragraph.Elements);
```

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/BlockRendererTests.cs
private static (Section Section, RenderContext Context) Render(
    Action<RenderContext> configure, params NotionBlockNode[] nodes)
{
    var (_, section, context, renderer) = TestDom.CreateRenderer(configure);
    renderer.RenderPage(section, nodes);
    return (section, context);
}

[Fact]
public void consecutive_paragraphs_get_the_classic_book_indent()
{
    //Act
    var (section, _) = Render(Para("First."), Para("Second."));

    //Assert
    var paragraphs = TestDom.AllParagraphs(section);
    paragraphs[0].Style.Should().Be("BodyOpen");
    paragraphs[1].Style.Should().Be("BodyIndented");
}
```

**Where to look.**
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/TestDom.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/BlockRendererTests.cs`
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/PageNumberingTests.cs`

**Sharp edges.**
- Walking the produced object model - including into table cells - is what makes
  the assertions specific: a style name and a string, not a rendered page.
- One helper that builds document, section, context and renderer together keeps
  every test's arrange step to a single line.
- Tests reach the internal renderer types only because the library exposes its
  internals to the test assembly.

### Assert on a generated document without a golden file

**When you want this.** You want the parse-compose-render path covered offline and
you have no golden output to compare against.

**The MVVM shape.** The test drives the internal classes directly, writes the
document to a folder under the test binary, and asserts on the file signature and
a lower bound on its size in pages.

**Code.**

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/Services/ArticleRenderServiceTests.cs
[Fact]
public async Task Compose_and_render_fixture_offline_produces_multipage_pdf()
{
    //Arrange - parse the embedded article fixture (no network, no images)
    var html = await EmbeddedResourceHelper.GetResourceAsString(
        FixtureResource, typeof(ArticleRenderServiceTests).Assembly);
    var article = new ArticleParser(CuneiformUrl).Parse(html);
    article.Blocks.Should().NotBeEmpty();

    //Act - compose the book and render it to a PDF
    var composer = new BookComposer(article, BookTheme.For(PageSizeOption.EightByTen), DateTime.Now);
    var document = composer.Compose();
    var renderer = new PdfDocumentRenderer(unicode: true) { Document = document };
    renderer.RenderDocument();

    var outPath = Path.Combine(GetOutDirectory(), "cuneiform-offline.pdf");
    renderer.PdfDocument.Save(outPath);

    //Assert
    File.Exists(outPath).Should().BeTrue();
    VerifyPdfSignature(outPath);
    renderer.PdfDocument.PageCount.Should().BeGreaterThan(5);
    _output.WriteLine($"Rendered {renderer.PdfDocument.PageCount} pages to {outPath}");
}
```

**Where to look.**
`WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/Services/ArticleRenderServiceTests.cs`

**Sharp edges.**
- Asserting on the format signature plus a page count is a cheap, stable way to
  verify a generated document with no golden file to maintain.
- Write the output to a folder under the test binary's base directory, created by
  the test, so a failure leaves something to look at.
- Assertions about content that can change are written as lower bounds rather than
  equalities.

### Make live tests opt in and keep them out of the default run

**When you want this.** A few tests genuinely need the network or a real account,
and they must not fail the suite for anyone who does not have one.

**The MVVM shape.** Either the credentials come from environment variables and the
class skips itself when they are absent, or the live tests carry a category trait
and share one fixture so a filter can exclude the whole set.

**Code.**

```csharp
// From CodeBrix.Samples/NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/NotionDocumentServiceTests.cs
/// <summary>
/// Integration tests against the live Notion API. Opt-in: they skip unless both
/// NOTION_AUTH_TOKEN and NOTION_TEST_PAGE_ID environment variables are set ...
/// </summary>
public class NotionDocumentServiceTests : IDisposable
{
    public NotionDocumentServiceTests()
    {
        _authToken = Environment.GetEnvironmentVariable("NOTION_AUTH_TOKEN");
        _testPageId = Environment.GetEnvironmentVariable("NOTION_TEST_PAGE_ID");

        Assert.SkipWhen(_authToken == null,
            "NOTION_AUTH_TOKEN environment variable is not set; skipping Notion integration tests.");
        Assert.SkipWhen(_testPageId == null,
            "NOTION_TEST_PAGE_ID environment variable is not set; skipping Notion integration tests.");

        _service = new NotionDocumentService();
    }

    public void Dispose() => _service?.Dispose();
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/Live/LiveApiFixture.cs
/// <summary>
/// Shares one factory and client across all live-API test classes so the whole live suite
/// reuses a single HTTP connection pool. Live tests carry
/// <c>[Trait("Category", "LiveApi")]</c> and can be excluded with
/// <c>dotnet test --filter Category!=LiveApi</c>.
/// </summary>
public sealed class LiveApiFixture : IDisposable
{
    public LiveApiFixture()
    {
        Factory = new DefaultPolyHavenClientFactory(new PolyHavenClientOptions
        {
            UserAgent = "PolyHavenBrowser.PolyHavenApiClient.Tests/1.0",
        });
        Client = Factory.GetClient();
    }
    // ...
}

[CollectionDefinition("LiveApi")]
public sealed class LiveApiCollection : ICollectionFixture<LiveApiFixture>;
```

```csharp
// From CodeBrix.Samples/WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/Services/ArticleRenderServiceTests.cs
//Regression for the "No readable article content" failure: the Uruk article carries more
//  than one .mw-parser-output container (a near-empty template wrapper plus the real body),
//  which used to make the parser walk the empty one and produce zero blocks. Fetches live
//  HTML and parses it WITHOUT downloading images, so it is fast.
[Theory]
[InlineData(UrukUrl, "Uruk")]
[InlineData(CuneiformUrl, "Cuneiform")]
public async Task Fetch_and_parse_finds_readable_content(string url, string expectedTitle)
```

**Where to look.**
`NotionDocumentCreator/tests/libs/NotionDocumentCreator.CreateDocument.Tests/NotionDocumentServiceTests.cs`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/Live/`
`WikipediaPublisher/Tests/WikipediaPublisher.RenderArticle.Tests/Services/ArticleRenderServiceTests.cs`

**Also shown by.**
`PolyHavenBrowser_viewer_only` (the same fixture and trait, with the analyzer rule
about cancellation tokens suppressed in the project file and the reason written
down).

**Sharp edges.**
- Skipping in the constructor makes the whole class inert, so nobody has to
  remember an attribute per test.
- A live class takes the shared fixture and carries both the collection attribute
  and the category trait; the trait is what a filter can exclude.
- Split the fast live test (fetch and parse, no downloads) from the slow
  end-to-end one, so a regression can be caught without paying for the rest.
- Assertions against live content are deliberately loose, because the content
  changes; assertions written against one specific account's data are a smoke test
  for that account rather than a portable suite.
- Keep an offline counterpart for the same code path - a stub handler, or a client
  pointed at a closed local port - so a normal run is entirely offline.
- The service under test is disposed by the test class, because the test class
  constructed it.

### Test an HTTP client offline with a stub handler

**When you want this.** Your API client should be almost entirely testable with no
network, including the exact URLs it builds.

**The MVVM shape.** A stub message handler that routes canned responses and
records every request, plus a tiny factory helper that wires it into the real
client.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/TestDoubles/TestClient.cs
internal static class TestClient
{
    public static (IPolyHavenApiClient Client, StubHttpMessageHandler Stub) Create(
        PolyHavenClientOptions options = null)
    {
        var stub = new StubHttpMessageHandler();
        var factory = new DefaultPolyHavenClientFactory(stub, options);
        return (factory.GetClient(), stub);
    }
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/TestDoubles/StubHttpMessageHandler.cs
/// <summary>Serves <paramref name="json"/> for requests whose path-and-query matches exactly.</summary>
public void OnPath(string pathAndQuery, string json, HttpStatusCode statusCode = HttpStatusCode.OK) =>
    _routes.Add((
        request => request.RequestUri!.PathAndQuery == pathAndQuery,
        _ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        }));
```

**Where to look.**
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/TestDoubles/`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/Unit/`

**Also shown by.**
`PolyHavenBrowser_viewer_only/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/TestDoubles/`
(with the canned JSON in its own file).

**Sharp edges.**
- Design the library so a test can hand it a handler: an internal constructor
  taking a message handler, reachable through the internals attribute, that never
  disposes what it was given.
- Recording the request URLs on the stub is what makes "did it build the right
  query string?" a one-line assertion.
- Return a not-found response naming the URL for anything unrouted, so a missing
  route reads as a missing route rather than as a client bug.

### Mock a rendering or API seam with CodeBrix TestMocks

**When you want this.** You want the code around an expensive or platform-bound
service covered without touching it.

**The MVVM shape.** This is the payoff for putting interfaces in front of the
concrete loader and renderer: the flow test needs neither a GPU nor a file on
disk.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Mocked/MockedRenderingTests.cs
[Fact]
public void viewer_flow_loads_then_hands_the_model_to_the_renderer()
{
    //Arrange - the typical app flow: load a model, give it to the scene renderer
    var model = TestAssets.BuildTriangleModel();
    var loaderMock = new Mock<IModelLoader>(MockBehavior.Strict);
    loaderMock.Setup(l => l.LoadFile("model.glb")).Returns(model);

    var rendererMock = new Mock<IModelSceneRenderer>(MockBehavior.Strict);
    rendererMock.Setup(r => r.SetModel(model, true));

    //Act
    var loaded = loaderMock.Object.LoadFile("model.glb");
    rendererMock.Object.SetModel(loaded, frameCamera: true);

    //Assert
    loaderMock.VerifyAll();
    rendererMock.VerifyAll();
    rendererMock.VerifyNoOtherCalls();
}
```

**Where to look.**
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Mocked/MockedRenderingTests.cs`
`PolyHavenBrowser_viewer_only/tests/libs/PolyHavenBrowser.PolyHavenApiClient.Tests/Mocked/`

**Sharp edges.**
- The mocks come from the CodeBrix TestMocks library, not a third-party mocking
  package.
- A mocked renderer can still hand out a real camera object, which is how pointer
  input wiring gets covered with no GPU at all.
- The loader interface exists in the production library specifically so the
  loading technology can be swapped or mocked; its own documentation comment says
  so.

### Test GL code headlessly with a surfaceless EGL context

**When you want this.** You want your renderer covered by real tests on a machine
or build agent with no window system.

**The MVVM shape.** A test double that creates the context and hands out the GL
object, plus a helper that skips cleanly when the machine cannot provide one.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/TestDoubles/EglTestContext.cs
private const string LibEgl = "libEGL.so.1";
private const int EGL_PLATFORM_SURFACELESS_MESA = 0x31DD;
// ...

// The core EGL 1.5 entry point: unlike eglGetPlatformDisplayEXT, this is a real
// exported symbol even under GLVND's dispatcher libEGL.
[DllImport(LibEgl)] private static extern IntPtr eglGetPlatformDisplay(int platform, IntPtr nativeDisplay, IntPtr attribs);
// ...

/// <summary>Tries to create a current GL context; returns <see langword="null"/> when the machine can't.</summary>
public static EglTestContext TryCreate()
{
    if (!OperatingSystem.IsLinux()) { return null; }

    try
    {
        var display = eglGetPlatformDisplay(EGL_PLATFORM_SURFACELESS_MESA, IntPtr.Zero, IntPtr.Zero);
        if (display == IntPtr.Zero || !eglInitialize(display, out _, out _)) { return null; }
        // ... eglChooseConfig, eglCreateContext (client version 3), eglCreatePbufferSurface, eglMakeCurrent ...

        var gl = GL.GetApi(name => eglGetProcAddress(name));
        return new EglTestContext(display, context, surface, gl);
    }
    catch (DllNotFoundException) { return null; }
    catch (EntryPointNotFoundException) { return null; }
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Gl/GlModelSceneRendererTests.cs
[Trait("Category", "RequiresGL")]
public class GlModelSceneRendererTests
{
    private static EglTestContext RequireGl()
    {
        var context = EglTestContext.TryCreate();
        Assert.SkipWhen(context is null, "No EGL/OpenGL stack available on this machine (install Mesa llvmpipe).");
        return context!;
    }

    [Fact]
    public void renderer_initializes_and_renders_a_triangle_onto_the_background()
    {
        //Arrange
        using var egl = RequireGl();
        var gl = egl.Gl;
        const uint size = 64;
        var (fbo, colorRb, depthRb) = CreateFramebuffer(gl, size, size);
        var renderer = new GlModelSceneRenderer { BackgroundColor = (0f, 0f, 1f, 1f) };
        try
        {
            //Act
            renderer.Initialize(gl);
            renderer.SetModel(TestAssets.BuildTriangleModel());
            renderer.Render(gl, size, size);

            var pixels = new byte[size * size * 4];
            gl.ReadPixels(0, 0, size, size, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.AsSpan());

            //Assert - some pixels show the red triangle, some the blue background
            // ...
        }
        finally
        {
            renderer.Uninitialize(gl);
            gl.DeleteRenderbuffer(colorRb);
            gl.DeleteRenderbuffer(depthRb);
            gl.DeleteFramebuffer(fbo);
        }
    }
}
```

**Where to look.**
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/TestDoubles/EglTestContext.cs`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/TestDoubles/GlDesktopTestContext.cs`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Gl/`

**Also shown by.**
`PolyHavenBrowser_viewer_only/tests/libs/PolyHavenBrowser.Rendering.Tests/TestDoubles/EglTestContext.cs`

**Sharp edges.**
- Bind the core EGL 1.5 platform-display entry point, not the extension one: only
  the former is a real exported symbol under the vendor-neutral dispatcher.
- Two contexts are worth having. The surfaceless one gives OpenGL ES; a second
  binds the desktop GL API and asks for a core profile, which is what the X11,
  Win32, WPF and macOS heads actually hand you. A bug that only appears on desktop
  GL is invisible in an ES-only suite.
- Catch the two native-loading exceptions and return null, so a machine with no
  software GL stack skips instead of failing.
- Trait the class so the whole GPU suite can be excluded by filter, and delete
  every renderbuffer and framebuffer in a finally block.

### Prove every graphics backend with the same mirrored suite

**When you want this.** You ship more than one graphics backend and want the same
behaviors proven for each of them.

**The MVVM shape.** Not applicable; test infrastructure. Each backend gets a
requirement helper that skips with an actionable message, and each suite is
trait-tagged.

**Code.**

```csharp
// From CodeBrix.Samples/PolyHavenBrowser_viewer_only/tests/libs/PolyHavenBrowser.Rendering.Tests/Vulkan/VulkanSceneRendererTests.cs
[Trait("Category", "RequiresVulkan")]
public class VulkanSceneRendererTests
{
    private static void RequireVulkan() =>
        Assert.SkipWhen(
            !VulkanSceneRenderer.IsRuntimeAvailable(),
            "No Vulkan stack available on this machine (install a Vulkan driver or Mesa lavapipe).");
```

**Where to look.**
`PolyHavenBrowser_viewer_only/tests/libs/PolyHavenBrowser.Rendering.Tests/Gl/`,
`.../Vulkan/`, `.../Metal/`, `.../TestData/TestAssets.cs`

**Sharp edges.**
- The three suites deliberately mirror each other test for test - draws a triangle
  onto the background, clearing the model renders only the background, resizing
  between frames renders at the new size, a textured material shows its texture
  color, the full path from model file to pixels, and the depth-ordering
  regression - so every backend proves the same behaviors.
- The backend that may hand back its pixels the other way up needs
  orientation-agnostic checks: scan the whole buffer, or assert on a vertically
  symmetric pixel.
- Each requirement helper names what to install, so a skip is actionable rather
  than mysterious.

### Pin a fixed bug with a regression test that says why it is shaped that way

**When you want this.** You fixed something subtle, and you want the test to
survive a later tidy-up that would make it useless.

**The MVVM shape.** Reproduce the cause in the test rather than the environment,
and put the reason in the arrange comment.

**Code.**

```csharp
// Adapted from CodeBrix.Samples/JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs
// (the source file prepends the control character as a literal inside the string;
//  here it stands in as a named constant so it stays visible)
[Theory]
[InlineData("27544076", "This is a test.")]
public async Task AES_decrypt_tolerates_stray_control_chars_from_clipboard(string key, string message)
{
    //Arrange - reproduce the Intel/x64 macOS clipboard glitch where an invisible
    //  U+0001 control character was being prepended to the pasted Base64 text,
    //  which made IsBase64Text() return false and blocked decryption.
    var crypt = GetService();
    var encrypted = await crypt.AES_EncryptToBase64(key, message);
    var corrupted = StrayControlChar + encrypted; //stray SOH char at index 0, as seen in the diagnostic output

    //Act + Assert - the corrupted text must still be recognized as encrypted...
    crypt.IsBase64Text(corrupted).Should().BeTrue();

    //...and must still decrypt back to the original message.
    var decrypted = await crypt.AES_DecryptFromBase64(key, corrupted);
    decrypted.Should().Be(message);
}
```

```csharp
// From CodeBrix.Samples/PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Gl/GlModelSceneRendererTests.cs
[Fact]
public void nearer_geometry_occludes_farther_geometry_regardless_of_draw_order()
{
    //Arrange - two large overlapping triangles centered on the origin: a near red one
    //(z=+0.5) and a far blue one (z=-0.5). Viewed from a ROTATED (non-axis-aligned)
    //camera, the near red triangle must win the center pixel no matter which is drawn
    //first. A rotated view is essential: a bad model-view-projection transpose collapses
    //the depth axis only for non-axis-aligned cameras (an axis-aligned view hides it).
    using var egl = RequireGl();
    var gl = egl.Gl;
    const uint size = 32;
    var center = (((int)size / 2) * (int)size + ((int)size / 2)) * 4;

    foreach (var nearDrawnFirst in new[] { false, true })
    {
        // ... render, read pixels ...

        //Assert - the near (red) triangle occludes the far (blue) one at the center
        pixels[center].Should().BeGreaterThan((byte)128);
        pixels[center + 2].Should().BeLessThan((byte)128);
    }
}
```

**Where to look.**
`JustBetweenUs/tests/JustBetweenUs.Encryption.Tests/Services/EncryptionServiceTests.cs`
`PolyHavenBrowser/tests/libs/PolyHavenBrowser.Rendering.Tests/Gl/GlModelSceneRendererTests.cs`

**Sharp edges.**
- Reproducing the corrupted input inside the test is what frees the test from the
  head the bug appeared on.
- Name both symptoms the fix protects, so a partial regression still fails.
- Assert on a single known pixel rather than an aggregate, and cover both draw
  orders; an aggregate passes with the depth axis flattened, and one order can
  happen to look right.
- Tests that assert an exact string produced from a source literal should
  normalize line endings on both sides, because the literal has whatever endings
  the checkout gave it. Anything with randomness in it is tested by round trip
  instead.

### Compare rendered images pixel by pixel

**When you want this.** Golden-image tests for rendering code - effects, charts,
report layout - that must be exact but tolerate one-bit rounding.

**The MVVM shape.** A test helper that loads the expected image, renders the
actual, compares with a tolerance, and reports the first few differences with
their values. A save hook makes accepting a new golden a one-line change.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/tests/libs/Pinta.Brix.Effects.Tests/Utilities.cs
public static void CompareImages (
	ImageSurface result,
	ImageSurface expected,
	int tolerance = 1)
{
	Assert.Equal (expected.GetSize (), result.GetSize ());

	ReadOnlySpan<ColorBgra> result_pixels = result.GetReadOnlyPixelData ();
	ReadOnlySpan<ColorBgra> expected_pixels = expected.GetReadOnlyPixelData ();

	int diffs = 0;
	StringBuilder details = new ();
	for (int i = 0; i < result_pixels.Length; ++i) {

		if (ColorBgra.ColorsWithinTolerance (result_pixels[i], expected_pixels[i], tolerance))
			continue;

		++diffs;

		// Display info about the first few failures.
		if (diffs <= 10)
			details.AppendLine ($"Difference at pixel {i}, got {result_pixels[i]} vs {expected_pixels[i]}, diff. of {ColorBgra.ColorDifference (result_pixels[i], expected_pixels[i])}");
	}

	if (diffs != 0)
		Assert.Fail ($"{diffs} pixel(s) differ beyond tolerance {tolerance}:{Environment.NewLine}{details}");
}

public static void TestEffect (
	BaseEffect effect,
	string result_image_name,
	string? save_image_name = null,
	string source_image_name = "input.png")
{
	using ImageSurface source = Utilities.LoadImage (source_image_name);
	using ImageSurface result = CairoExtensions.CreateImageSurface (Format.Argb32, source.Width, source.Height);
	using ImageSurface expected = LoadImage (result_image_name);

	effect.Render (source, result, [source.GetBounds ()]);

	// For debugging, optionally save out the result to a file.
	if (save_image_name != null)
		SaveImage (result, save_image_name);

	CompareImages (result, expected);
}
```

**Where to look.**
`Pinta.Brix/tests/libs/Pinta.Brix.Effects.Tests/Utilities.cs`
`Pinta.Brix/tests/libs/Pinta.Brix.Effects.Tests/Mocks/`
`Pinta.Brix/tests/libs/Pinta.Brix.Effects.Tests/EffectsTest.cs`

**Sharp edges.**
- Decode straight into the surface's own pixel format, or the comparison fails on
  conversion rounding rather than on the code under test.
- Report the first few differing pixels with both values and the delta; a bare
  count is not debuggable.
- The effects under test resolve their dependencies from a mock service provider
  built in the same helper, so no real chrome, workspace or palette is needed.
- The optional save hook is what turns "accept the new golden" into changing one
  argument.

### Point a process-global store at a throwaway folder in tests

**When you want this.** Your production code initializes a singleton store on
startup, and your tests must never touch the user's real data.

**The MVVM shape.** A module initializer in the test assembly opens the store at a
temporary path before any test runs, guarded by the store's own initialized flag.
Tests of the store itself take the opposite approach: a fresh directory per test.

**Code.**

```csharp
// From CodeBrix.Samples/Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/TestSettingsStore.cs
// PintaCore's static constructor builds the palette manager, which reads
// settings, so touching PintaCore at all requires an open settings store.
// SettingsService is a process-global singleton, so it is pointed at a
// throwaway folder once per test assembly - never at the user's real
// ~/.config/Pinta.Brix/settings, which tests must never read or write.

internal static class TestSettingsStore
{
	[ModuleInitializer]
	internal static void Initialize ()
	{
		if (SettingsService.IsInitialized)
			return;

		SettingsService.Initialize (Path.Combine (
			Path.GetTempPath (),
			"PintaBrix.Engine.Tests_" + Guid.NewGuid ().ToString ("N")));
	}
}
```

```csharp
// From CodeBrix.Samples/KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Settings.Tests/SettingsStoreTests.cs
// These tests exercise the CodeBrix.Platform.AppSettings store that the
// KenneyAssetBrowser.Settings facade wraps. The add-in's store has no public test
// clock, so assertions about timestamped file names match on the naming
// pattern rather than exact names.
public class SettingsStoreTests : IDisposable
{
    public SettingsStoreTests()
    {
        root = Path.Combine(Path.GetTempPath(), "kenney-asset-browser-tests", Path.GetRandomFileName());
        directory = Path.Combine(root, "settings");
        // ...
    }

    public void Dispose()
    {
        try { Directory.Delete(root, recursive: true); } catch { /* best effort */ }
    }

    AppSettingsStore CreateStore() => new AppSettingsStore(SettingsService.AppName, directory);

    // The auto-backup files whose names carry a parseable timestamp,
    // alphabetical (= chronological, the naming scheme's guarantee).
    string[] AutoBackupFiles() =>
        Directory.EnumerateFiles(directory, $"{AppSettingsStore.AutoBackupFilePrefix}*.sqlite")
            .Select(Path.GetFileName)
            .Where(HasParseableTimestamp)
            .OrderBy(name => name)
            .ToArray();
}
```

**Where to look.**
`Pinta.Brix/tests/libs/Pinta.Brix.Engine.Tests/TestSettingsStore.cs` and the
matching file under `Pinta.Brix.FileFormats.Tests/`
`KenneyAssetBrowser/tests/libs/KenneyAssetBrowser.Settings.Tests/SettingsStoreTests.cs`

**Sharp edges.**
- A module initializer is what guarantees the store is open before a static
  constructor in the library under test runs; a fixture would be too late.
- Each test assembly needs its own copy - two of them in one process would
  otherwise race on the guard.
- The store's own constants for file names, backup prefixes and timestamp formats
  are public, so tests assert against the real naming scheme rather than a copy of
  it, and against the guarantee that timestamped names sort chronologically.
- The corruption tests write junk over the store's file and assert that it is
  quarantined and restored from the newest backup; that path exists only because
  the add-in provides it.

### Drive a scripted end-to-end run of the whole application

**When you want this.** The parts a unit test cannot reach - a real head, a real
player element, a real visual tree - still need proving.

**The MVVM shape.** The page reads options from the environment in its constructor
and, when they are present, hooks its loaded event to run a script that drives the
view model's own commands and properties, then prints machine-readable lines and
exits with a status. Nothing about the run changes what the application does when
the variables are not set.

**Code.**

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
//Optional scripted run: import, play and report without anyone touching the window.
if (SmokeOptions.FromEnvironment() is { } smoke)
{
    Loaded += (_, _) => RunSmoke(smoke);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
private static void Fact(string name, object value) =>
    Console.WriteLine($"CBVT-SMOKE: {name}={value?.ToString() ?? "(null)"}");

private static void Finish(int failures)
{
    Console.WriteLine($"CBVT-SMOKE: RESULT {(failures == 0 ? "PASS" : $"FAIL ({failures})")}");
    Console.Out.Flush();
    Environment.Exit(failures == 0 ? 0 : 1);
}
```

```csharp
// From CodeBrix.Samples/CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs
var outputPath = Path.Combine(
    options.WorkFolder, "smoke" + MediaFormats.Extension(options.Destination));
viewModel.Conversion.PickOutputPathAsync = (_, _) => Task.FromResult(outputPath);

var finished = new TaskCompletionSource<Processing.Operations.ConversionOutcome>();
void OnFinished(object _, Processing.Operations.ConversionOutcome result) => finished.TrySetResult(result);
viewModel.Conversion.ConversionFinished += OnFinished;
viewModel.Conversion.RunCommand.Execute(null);
var outcome = await finished.Task;
viewModel.Conversion.ConversionFinished -= OnFinished;
```

**Where to look.**
`CodeBrixVideoTool/src/CodeBrixVideoTool.UI/Views/MainPage.xaml.cs` (the smoke-mode
region: `SmokeOptions`, `RunSmoke`, `RunMp4ExportAsync`, `CheckLastRunNotes`,
`ShownRowOpacity`, `FindLibraryRow`)

**Sharp edges.**
- The bridge delegates are what make the script possible: replacing the save-path
  delegate with one that returns a fixed path removes the only dialog in the way.
- An event plus a completion source is how the script awaits a fire-and-forget
  command.
- Anything that happens off an event rather than in the command needs a bounded
  retry loop before the script asserts on it, rather than an assumption that it
  has landed.
- To prove a visual rule is real rather than only configured, the script forces a
  layout pass, gets the item's container, walks the visual tree for the named
  element and compares it against a control case.
- Where a case is expected to fail a profile check, assert the expectation rather
  than success.

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

## Not yet covered by a sample

- Navigating between several pages with a back stack, rather than one page per
  application.
- Opening a second top-level window from a running application.
- Localized user-interface strings, and formatting for a culture other than the
  running machine's.
- Printing a document or a page from a Skia head.
- Drag and drop between the application and the desktop.
- A database-backed data layer, beyond the settings store the AppSettings
  add-in provides.
- Publishing and installers for the Skia heads; only the native WinUI head's
  packaging configuration appears here.
- Accessibility: naming elements for a screen reader, or a keyboard-only path
  through a page.
