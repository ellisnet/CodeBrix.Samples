# 3D rendering inside a CodeBrix.Platform view

This folder is a self-contained reference for **rendering a 3D model into an ordinary
CodeBrix.Platform UI view** — the model is drawn off-screen with **WebGPU** (the default),
OpenGL, **Vulkan, or Metal** (a runtime dropdown choice), the pixels are read back and composited onto a **Skia canvas**
(`SKXamlCanvas`) that lives in the app's normal XAML, and the user can orbit/zoom it. Copy
this folder (plus the `PolyHavenBrowser.Rendering` library it depends on) as a starting point.

## The pipeline, end to end

```
 glTF/.glb file                          a Poly Haven texture image
      │                                          │
      ▼  GltfModelLoader (Rendering lib)         ▼  CubeMeshBuilder (this folder)
   LoadedModel  ◄───────────────────────────────┘   (mesh + materials + bounds + pivot)
      │
      ▼  IModelRenderEngine.SetModel + RenderFrame(width, height, background)
   ┌─────────────────────────────────────────────────────────────────┐
   │  ONE OF (chosen at runtime via IModelRenderEngineSelector):      │
   │                                                                  │
   │  OpenGl…Engine        Vulkan…Engine          Metal…Engine        │
   │    OffscreenGLContext    VulkanSceneRenderer    MetalSceneRenderer│
   │      → native GL ctx       (Rendering lib) →      (Rendering lib)→│
   │    GlModelSceneRenderer    own instance/device,   own device/     │
   │      → shaders+VAOs→FBO    pipeline → offscreen    queue/pipeline →│
   │    glReadPixels →          image → copy-to-buffer  offscreen tex → │
   │      RGBA bytes            → RGBA bytes            blit-to-buffer →│
   │                                                   RGBA bytes      │
   │                                                                  │
   │  WebGpu…Engine (the default, every head incl. Android)           │
   │    WebGpuSceneRenderer (Rendering lib) → CodeBrix.WebGpu device  │
   │      (D3D12 / Vulkan / Metal) → WGSL pipelines → headless        │
   │      WebGpuSkiaTarget → ReadPixels → RGBA bytes                  │
   └─────────────────────────────────────────────────────────────────┘
      │  RenderedFrame (RGBA pixels; IsBottomUp true for GL/Vulkan, false for Metal/WebGPU - see below)
      ▼  ModelScenePainter  (API-agnostic)
   draw darkened background texture → SKImage.FromPixelCopy → flip if bottom-up → DrawImage
      │
      ▼  SKXamlCanvas.PaintSurface  (MainPage.xaml.cs)
   on screen, inside the normal app UI
```

The HDRI panorama takes a simpler path that does not go through the engine dropdown (which is
why the dropdown is hidden in HDRI mode): `PanoramaScenePainter` renders the view into an
`SKBitmap` with **WebGPU** (`WebGpuPanoramaRenderer` — one full-screen triangle whose WGSL
fragment shader does the equirect lookup + tone mapping, read back from a headless target) and
falls back to the **CPU** ray-tracer (`EquirectPanoramaRenderer`) when WebGPU cannot start, or
if a GPU frame fails later. Both produce the same image (a test compares them pixel for pixel).
Both painters implement `IScenePainter` so the page treats them uniformly.

## The seam that makes the graphics API swappable

Everything **above** `IModelRenderEngine` is graphics-API-agnostic and is reused unchanged for
any backend:

- `LoadedModel`, `GltfModelLoader`, `CubeMeshBuilder`, `OrbitCamera` — model data + camera math.
  (`GltfModelLoader` also classifies each material's **alpha mode** — opaque, or translucent for
  `alphaMode: BLEND` *and* `KHR_materials_transmission` glass — which both renderers act on below.)
- `ModelScenePainter` — input handling + Skia compositing.
- `IScenePainter`, the view model, the XAML/code-behind.

Everything **below** it is API-specific, one class-cluster per backend:

- WebGPU (the default): `WebGpuModelRenderEngine`, a thin adapter over the Rendering lib's
  self-contained `WebGpuSceneRenderer` (CodeBrix.WebGpu: `WebGpuSkiaDevice` creates the device on
  Direct3D 12 / Vulkan / Metal, a headless `WebGpuSkiaTarget` is the colour target and does the
  readback, and the raw `WebGpu` binding builds the pipelines; WGSL compiled at runtime from
  `WebGpuShaders`). Works on every head, Android included.
- OpenGL: `OpenGlModelRenderEngine` (+ Graphics3DGL's `OffscreenGLContext`, + `GlModelSceneRenderer`'s
  GL shaders in the Rendering lib).
- Vulkan: `VulkanModelRenderEngine`, a thin adapter over the Rendering lib's self-contained
  `VulkanSceneRenderer` (Silk.NET.Vulkan; instance → device → offscreen images → pipeline →
  readback all in one class, SPIR-V pre-compiled into `VulkanShaders`).
- Metal: `MetalModelRenderEngine`, a thin adapter over the Rendering lib's self-contained
  `MetalSceneRenderer` (**direct to Metal via the raw Objective-C runtime** — `MetalInterop`'s
  `objc_msgSend` P/Invoke, no MoltenVK/Silk.NET/managed-Apple-bindings and **no NuGet packages**;
  device → queue → offscreen targets → pipeline → readback all in one class, MSL compiled at
  runtime from `MetalShaders`). Works on Apple Silicon **and** Intel Macs — see the arch note below.

### How the backend is chosen (the dropdown)

`IModelRenderEngineSelector` (registered in `RegisterServices`) owns the list of backends and
the platform gate:

- `AvailableKinds` → `[WebGpu, OpenGL, Vulkan, Metal]`, in dropdown order; the app always starts
  on WebGPU, pre-warmed off the UI thread, and falls back to OpenGL (with a status-line note) when
  WebGPU cannot start on the machine.
- `IsSupported(kind)` → WebGPU and OpenGL everywhere (CodeBrix.WebGpu ships a native library for
  every platform, so WebGPU needs no allow-list — a missing driver is caught at runtime by the
  pre-warm instead); Vulkan and Metal each only where the Rendering
  library's **hardcoded** allow-list okays them — and the two are **mirror images**:
  - `VulkanPlatformSupport`: the Linux **X11** and **Wayland** heads and the **Win32Skia**/
    **WinWpfSkia** heads. **macOS** (no system Vulkan loader) and the Linux **FrameBuffer** head
    are excluded.
  - `MetalPlatformSupport`: **macOS only** (Metal is an Apple API), on a supported process
    architecture (arm64 or x64); every other head is excluded.

  Both are policy lists, not driver probes, so an API is never even attempted on a platform that
  has not been okayed. The head is detected once by scanning the loaded assemblies for a
  `CodeBrix.Platform.UI.Runtime.Skia.*` assembly, which is not referenced by any project here
  (the detection is shared).
- `Create(kind, getXamlRoot)` → a fresh engine (the caller owns and disposes it). The
  `getXamlRoot` accessor is used only by the OpenGL engine, to create its offscreen native GL
  context from the hosting page's `XamlRoot`; the WebGPU, Vulkan and Metal engines ignore it.

Picking an unsupported engine (Vulkan on macOS, or Metal off macOS) shows a `SimpleDialog` alert
("{engine} rendering is not available on this platform.") and snaps the dropdown back to the
engine that is running. On a supported platform the view model pre-warms the new own-stack engine
(WebGPU, Vulkan **or** Metal)
with a 1×1 off-thread frame so a missing/broken driver fails fast into a status message (never
inside the paint callback), then swaps painters and re-displays the current sample from the local
cache.

## Four things that are easy to get wrong

- **Threading / context (OpenGL only).** A GL context must be created and used on the thread
  that renders — the UI thread, inside `SKXamlCanvas.PaintSurface`. `OpenGlModelRenderEngine`
  therefore creates its `OffscreenGLContext` **lazily** on the first `RenderFrame` (which is
  also why the engine is handed a `Func<XamlRoot>` rather than a `XamlRoot`: the accessor is
  invoked on the render thread, by which point the page has one). `OffscreenGLContext.MakeCurrent`
  returns a scope that **saves and restores** whatever context the host head had current, so
  this engine never disturbs the head's own renderer even though they share the thread. The
  context itself is cross-platform — Graphics3DGL resolves the head's native GL wrapper (WGL on
  Windows, GLX on X11, EGL on Wayland/FrameBuffer, OpenGL ES through ANGLE on macOS) — so the app P/Invokes no
  platform GL loader of its own. Vulkan and Metal have no ambient context at all —
  `VulkanSceneRenderer`/`MetalSceneRenderer` each own their whole stack (own device/queue) and
  cannot collide with the head — but both still initialize lazily on first use.
- **Pixel orientation & the MVP.** OpenGL's first pixel row is the image bottom, so its
  `RenderedFrame.IsBottomUp` is true and `ModelScenePainter` flips vertically for Skia.
  The Vulkan engine uses the **same unmodified camera matrices** — and because Vulkan's
  clip-space Y points down (and its depth range is [0, 1], which
  `Matrix4x4.CreatePerspectiveFieldOfView` already produces), its readback comes out
  vertically inverted too, so it **also** reports `IsBottomUp: true` and the same flip serves
  both. **Metal is the exception**: its clip-space Y points *up* (like GL) while its framebuffer
  origin is top-left (like Vulkan), so with the same matrices the readback comes out **top-down**
  and `MetalModelRenderEngine` reports `IsBottomUp: false` (no flip). **WebGPU** has the same
  pairing as Metal, so `WebGpuModelRenderEngine` reports `IsBottomUp: false` too (pinned by the
  `readback_is_top_down` test). Separately, the model-view-projection matrix is uploaded
  **without** an extra `Matrix4x4.Transpose` on all four APIs: System.Numerics is row-major, and
  GL/SPIR-V/MSL/WGSL reading it as column-major already applies the transpose needed —
  transposing again silently flattens the depth axis for rotated cameras (see the regression test
  `nearer_geometry_occludes_farther_geometry_regardless_of_draw_order`, which exists for all
  four renderers).
- **Shaders without a toolchain.** Vulkan consumes SPIR-V, not GLSL, so its shaders are compiled
  once at development time and the words embedded as static source in `VulkanShaders.cs` (GLSL
  alongside in comments) — the same pre-captured-output approach as the CodeBrix.Platform.OpenGL
  bindings. Metal is simpler still: it compiles **MSL from source at runtime**
  (`newLibraryWithSource:`), so `MetalShaders.cs` just holds the MSL string and no shader
  artifact is pre-baked. Either way, building and running the app needs no shader compiler.
- **Metal on both Mac architectures.** `MetalInterop` deliberately avoids any Objective-C method
  that *returns* a struct by value — the only place the `objc_msgSend` calling convention differs
  between Apple Silicon (arm64) and Intel/Rosetta (x86-64, which would otherwise need
  `objc_msgSend_stret`). Every message returns an `id`/pointer or `void`, or takes a struct only
  as an *argument* (which the P/Invoke marshaller lays out per-architecture automatically), so one
  `objc_msgSend` serves both. Every GPU↔CPU transfer also goes through a shared staging *buffer*
  blitted to/from a private texture (256-byte-aligned rows) — the one path supported identically
  on Apple Silicon, Intel, and Rosetta, unlike shared/managed *textures* whose availability splits
  by GPU family.
- **Transparent (glass) materials.** glTF marks glass two ways: `alphaMode: BLEND`, and — more
  subtly — a `KHR_materials_transmission` extension on an otherwise `alphaMode: OPAQUE` material
  (a camera lens, a clock face). `GltfModelLoader` treats **both** as translucent. This preview
  doesn't implement real transmission/refraction, so a translucent material is instead given a
  fixed preview opacity (`ModelMaterial.BlendPreviewOpacity`, currently 15%) — otherwise glass
  renders as an opaque disc that hides everything behind it. Each renderer then draws in **two
  passes**: opaque/mask primitives first with depth writes **on**, then the translucent ones with
  depth writes **off** and straight-alpha "over" blending. GL uses
  `BlendFuncSeparate(SrcAlpha, OneMinusSrcAlpha, One, OneMinusSrcAlpha)` (the alpha channel
  accumulates coverage so a region already opaque behind the glass stays opaque for the Skia
  composite); Vulkan uses a **second pipeline** (`_blendPipeline`: blend enabled,
  `DepthWriteEnable = false`) drawn after the opaque pass. The preview opacity rides in the
  existing `BaseColorFactor.W` (a GL uniform / a Vulkan push-constant field), so no shader or
  SPIR-V change was needed. BLEND primitives are **not** depth-sorted — fine for the small amount
  of transparent geometry these preview models carry.

## File map

| File | Role | API-specific? |
| --- | --- | --- |
| `IScenePainter.cs` | Common contract: paint + pointer input | no |
| `IModelRenderEngine.cs` | The swappable 3D engine seam (+ `RenderedFrame`) | no |
| `IModelRenderEngineFactory.cs` | Per-backend engine factories | no |
| `IModelRenderEngineSelector.cs` | Runtime backend choice + platform gate (the dropdown) | no |
| `ModelScenePainter.cs` | Input + Skia compositing over any engine | no |
| `CubeMeshBuilder.cs` | Texture → a cube `LoadedModel` | no |
| `PanoramaScenePainter.cs` | HDRI panorama (WebGPU, or the CPU as fallback → `SKBitmap`) | no |
| `WebGpuModelRenderEngine.cs` | WebGPU engine adapter (+ its factory) — the default | **yes** |
| `OpenGlModelRenderEngine.cs` | OpenGL FBO + readback over Graphics3DGL's `OffscreenGLContext` | **yes** |
| `VulkanModelRenderEngine.cs` | Vulkan engine adapter (+ its factory) | **yes** |
| `MetalModelRenderEngine.cs` | Metal engine adapter (+ its factory) | **yes** |

(The off-screen GL context type, `CodeBrix.Platform.WinUI.Graphics3DGL.OffscreenGLContext`, arrives through
the cross-platform CodeBrix.Platform.Graphics3DGL library,
so there is no app-owned EGL/WGL/GLX class here.)

(The heavy lifting lives in the Rendering library:
`GL/GlModelSceneRenderer.cs` for OpenGL shaders/VAOs/drawing;
`Vulkan/VulkanSceneRenderer.cs` + `Vulkan/VulkanShaders.cs` + `Vulkan/VulkanPlatformSupport.cs`
for the whole Vulkan stack and its allow-list; and
`Metal/MetalSceneRenderer.cs` + `Metal/MetalShaders.cs` + `Metal/MetalInterop.cs` +
`Metal/MetalPlatformSupport.cs` for the whole direct-to-Metal stack, its raw Objective-C interop,
and its macOS-only allow-list; and `WebGpu/WebGpuSceneRenderer.cs` + `WebGpu/WebGpuPanoramaRenderer.cs`
+ `WebGpu/WebGpuShaders.cs` + `WebGpu/WebGpuSupport.cs` for the WebGPU model and HDRI renderers,
their WGSL, and the shared device/shader helpers.)
