# 3D rendering inside a CodeBrix.Platform view

This folder is a self-contained reference for **rendering a 3D model into an ordinary
CodeBrix.Platform UI view** — the model is drawn with OpenGL/EGL off-screen, the pixels are
read back and composited onto a **Skia canvas** (`SKXamlCanvas`) that lives in the app's normal
XAML, and the user can orbit/zoom it. Copy this folder (plus the `PolyHavenBrowser.Rendering`
library it depends on) as a starting point.

## The pipeline, end to end

```
 glTF/.glb file                          a Poly Haven texture image
      │                                          │
      ▼  GltfModelLoader (Rendering lib)         ▼  CubeMeshBuilder (this folder)
   LoadedModel  ◄───────────────────────────────┘   (mesh + materials + bounds + pivot)
      │
      ▼  IModelRenderEngine.SetModel + RenderFrame(width, height, background)
   ┌───────────────────────────────────────────────────────────┐
   │  OpenGlModelRenderEngine  (the swappable, API-specific bit) │
   │    EglOffscreenGlContext  → make an off-screen GLES context │
   │    GlModelSceneRenderer   → shaders + VAOs + draw into a FBO │
   │    glReadPixels           → RGBA bytes back to the CPU       │
   └───────────────────────────────────────────────────────────┘
      │  RenderedFrame (RGBA pixels, IsBottomUp)
      ▼  ModelScenePainter  (API-agnostic)
   draw darkened background texture → SKImage.FromPixelCopy → flip if bottom-up → DrawImage
      │
      ▼  SKXamlCanvas.PaintSurface  (MainPage.xaml.cs)
   on screen, inside the normal app UI
```

The HDRI panorama takes a simpler path: `PanoramaScenePainter` is a **CPU** ray-tracer
(`EquirectPanoramaRenderer` in the Rendering lib) that produces an `SKBitmap` directly — no GL
engine involved. Both painters implement `IScenePainter` so the page treats them uniformly.

## The seam that makes the graphics API swappable

Everything **above** `IModelRenderEngine` is graphics-API-agnostic and is reused unchanged for
any backend:

- `LoadedModel`, `GltfModelLoader`, `CubeMeshBuilder`, `OrbitCamera` — model data + camera math.
- `ModelScenePainter` — input handling + Skia compositing.
- `IScenePainter`, the view model, the XAML/code-behind.

Everything **below** it is API-specific and lives in exactly one class:

- `OpenGlModelRenderEngine` (+ `EglOffscreenGlContext`, + `GlModelSceneRenderer`'s GL shaders).

### To add a Vulkan backend (the planned future upgrade)

1. Write `VulkanModelRenderEngine : IModelRenderEngine` — create a Vulkan device + an off-screen
   image, record a pipeline that draws the `LoadedModel`, and copy the rendered image back to a
   CPU `byte[]`. Return it as a `RenderedFrame` (set `IsBottomUp` to match Vulkan's convention).
   (You'll also want a `VulkanModelSceneRenderer` equivalent of `GlModelSceneRenderer`.)
2. Write `VulkanModelRenderEngineFactory : IModelRenderEngineFactory`.
3. Register it in `RegisterServices` instead of `OpenGlModelRenderEngineFactory` — or register a
   selector factory that reads a user setting and returns OpenGL or Vulkan.

Nothing else changes: the painter, camera, loaders, view models, and UI are untouched. (Per the
plan, macOS stays OpenGL/EGL-only; the other heads can offer the choice.)

## Two things that are easy to get wrong

- **Threading / context.** A GL context must be created and used on the thread that renders — the
  UI thread, inside `SKXamlCanvas.PaintSurface`. `OpenGlModelRenderEngine` therefore creates its
  EGL context **lazily** on the first `RenderFrame`. `EglOffscreenGlContext.MakeCurrent` also
  **saves and restores** whatever context the host head had current, so this engine never
  disturbs the head's own renderer even though they share the thread.
- **Pixel orientation & the MVP.** OpenGL's first pixel row is the image bottom, so
  `RenderedFrame.IsBottomUp` is true and `ModelScenePainter` flips vertically for Skia. Separately,
  the model-view-projection matrix is uploaded to GL **without** an extra `Matrix4x4.Transpose`:
  System.Numerics is row-major, and GL reading that as column-major already applies the transpose
  it needs — transposing again silently flattens the depth axis for rotated cameras (see the
  regression test `nearer_geometry_occludes_farther_geometry_regardless_of_draw_order`).

## File map

| File | Role | API-specific? |
| --- | --- | --- |
| `IScenePainter.cs` | Common contract: paint + pointer input | no |
| `IModelRenderEngine.cs` | The swappable 3D engine seam (+ `RenderedFrame`) | no |
| `IModelRenderEngineFactory.cs` | Selects the backend (via DI) | no |
| `ModelScenePainter.cs` | Input + Skia compositing over any engine | no |
| `CubeMeshBuilder.cs` | Texture → a cube `LoadedModel` | no |
| `PanoramaScenePainter.cs` | HDRI panorama (CPU → `SKBitmap`) | no |
| `OpenGlModelRenderEngine.cs` | OpenGL context + FBO + readback | **yes** |
| `EglOffscreenGlContext.cs` | Creates the off-screen EGL/GLES context | **yes** |

(The heavy lifting of shaders, VAO upload, and drawing lives in
`PolyHavenBrowser.Rendering/GL/GlModelSceneRenderer.cs`.)
