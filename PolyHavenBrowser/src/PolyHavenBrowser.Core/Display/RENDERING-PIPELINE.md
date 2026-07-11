# 3D rendering inside a CodeBrix.Platform view

This folder is a self-contained reference for **rendering a 3D model into an ordinary
CodeBrix.Platform UI view** — the model is drawn off-screen with OpenGL/EGL **or Vulkan**
(a runtime dropdown choice), the pixels are read back and composited onto a **Skia canvas**
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
   │  OpenGlModelRenderEngine            VulkanModelRenderEngine      │
   │    EglOffscreenGlContext              VulkanSceneRenderer        │
   │      → off-screen GLES context          (Rendering lib) → its    │
   │    GlModelSceneRenderer                 own instance/device,     │
   │      → shaders + VAOs → FBO             pipeline → off-screen    │
   │    glReadPixels → RGBA bytes            image → copy-to-buffer   │
   │                                         → RGBA bytes             │
   └─────────────────────────────────────────────────────────────────┘
      │  RenderedFrame (RGBA pixels, IsBottomUp - true for BOTH engines, see below)
      ▼  ModelScenePainter  (API-agnostic)
   draw darkened background texture → SKImage.FromPixelCopy → flip if bottom-up → DrawImage
      │
      ▼  SKXamlCanvas.PaintSurface  (MainPage.xaml.cs)
   on screen, inside the normal app UI
```

The HDRI panorama takes a simpler path: `PanoramaScenePainter` is a **CPU** ray-tracer
(`EquirectPanoramaRenderer` in the Rendering lib) that produces an `SKBitmap` directly — no
GPU engine involved, which is why the engine dropdown is hidden in HDRI mode. Both painters
implement `IScenePainter` so the page treats them uniformly.

## The seam that makes the graphics API swappable

Everything **above** `IModelRenderEngine` is graphics-API-agnostic and is reused unchanged for
any backend:

- `LoadedModel`, `GltfModelLoader`, `CubeMeshBuilder`, `OrbitCamera` — model data + camera math.
- `ModelScenePainter` — input handling + Skia compositing.
- `IScenePainter`, the view model, the XAML/code-behind.

Everything **below** it is API-specific, one class-cluster per backend:

- OpenGL: `OpenGlModelRenderEngine` (+ `EglOffscreenGlContext`, + `GlModelSceneRenderer`'s
  GL shaders in the Rendering lib).
- Vulkan: `VulkanModelRenderEngine`, a thin adapter over the Rendering lib's self-contained
  `VulkanSceneRenderer` (Silk.NET.Vulkan; instance → device → offscreen images → pipeline →
  readback all in one class, SPIR-V pre-compiled into `VulkanShaders`).

### How the backend is chosen (the dropdown)

`IModelRenderEngineSelector` (registered in `RegisterServices`, replacing the old single
`IModelRenderEngineFactory` registration) owns the list of backends and the platform gate:

- `AvailableKinds` → `[OpenGL, Vulkan]`, in dropdown order; the app always starts on OpenGL.
- `IsSupported(kind)` → OpenGL everywhere; Vulkan only where the Rendering library's
  **hardcoded** `VulkanPlatformSupport` allow-list okays it: the Linux **X11** and **Wayland**
  heads and the **Win32Skia**/**WinWpfSkia** heads. **macOS** (no system Vulkan loader) and the
  Linux **FrameBuffer** head are deliberately excluded — this is a policy list, not a driver
  probe, so Vulkan is never even attempted on a platform that has not been okayed. The head is
  detected by scanning for the loaded `CodeBrix.Platform.UI.Runtime.Skia.*` runtime assembly.
- `Create(kind)` → a fresh engine (the caller owns and disposes it).

Picking Vulkan on an unsupported platform shows a `SimpleDialog` alert ("Vulkan rendering is
not available on this platform.") and snaps the dropdown back to OpenGL. On a supported
platform the view model pre-warms the new Vulkan engine with a 1×1 off-thread frame so a
missing/broken driver fails fast into a status message (never inside the paint callback),
then swaps painters and re-displays the current sample from the local cache.

## Three things that are easy to get wrong

- **Threading / context (OpenGL only).** A GL context must be created and used on the thread
  that renders — the UI thread, inside `SKXamlCanvas.PaintSurface`. `OpenGlModelRenderEngine`
  therefore creates its EGL context **lazily** on the first `RenderFrame`.
  `EglOffscreenGlContext.MakeCurrent` also **saves and restores** whatever context the host
  head had current, so this engine never disturbs the head's own renderer even though they
  share the thread. Vulkan has no ambient context at all — `VulkanSceneRenderer` owns its
  whole stack and cannot collide with the head — but it still initializes lazily on first use.
- **Pixel orientation & the MVP.** OpenGL's first pixel row is the image bottom, so its
  `RenderedFrame.IsBottomUp` is true and `ModelScenePainter` flips vertically for Skia.
  The Vulkan engine uses the **same unmodified camera matrices** — and because Vulkan's
  clip-space Y points down (and its depth range is [0, 1], which
  `Matrix4x4.CreatePerspectiveFieldOfView` already produces), its readback comes out
  vertically inverted too, so it **also** reports `IsBottomUp: true` and the same flip serves
  both engines. Separately, the model-view-projection matrix is uploaded **without** an extra
  `Matrix4x4.Transpose` on both APIs: System.Numerics is row-major, and GL/SPIR-V reading it
  as column-major already applies the transpose needed — transposing again silently flattens
  the depth axis for rotated cameras (see the regression test
  `nearer_geometry_occludes_farther_geometry_regardless_of_draw_order`, which exists for both
  renderers).
- **SPIR-V without a toolchain.** Vulkan consumes SPIR-V, not GLSL. The shaders are compiled
  once at development time and the words embedded as static source in `VulkanShaders.cs`
  (with the GLSL alongside in comments), so building and running the app needs no shader
  compiler — the same pre-captured-output approach as the CodeBrix.Platform.OpenGL bindings.

## File map

| File | Role | API-specific? |
| --- | --- | --- |
| `IScenePainter.cs` | Common contract: paint + pointer input | no |
| `IModelRenderEngine.cs` | The swappable 3D engine seam (+ `RenderedFrame`) | no |
| `IModelRenderEngineFactory.cs` | Per-backend engine factories | no |
| `IModelRenderEngineSelector.cs` | Runtime backend choice + platform gate (the dropdown) | no |
| `ModelScenePainter.cs` | Input + Skia compositing over any engine | no |
| `CubeMeshBuilder.cs` | Texture → a cube `LoadedModel` | no |
| `PanoramaScenePainter.cs` | HDRI panorama (CPU → `SKBitmap`) | no |
| `OpenGlModelRenderEngine.cs` | OpenGL context + FBO + readback | **yes** |
| `EglOffscreenGlContext.cs` | Creates the off-screen EGL/GLES context | **yes** |
| `VulkanModelRenderEngine.cs` | Vulkan engine adapter (+ its factory) | **yes** |

(The heavy lifting lives in the Rendering library:
`GL/GlModelSceneRenderer.cs` for OpenGL shaders/VAOs/drawing, and
`Vulkan/VulkanSceneRenderer.cs` + `Vulkan/VulkanShaders.cs` + `Vulkan/VulkanPlatformSupport.cs`
for the whole Vulkan stack and the hardcoded platform allow-list.)
