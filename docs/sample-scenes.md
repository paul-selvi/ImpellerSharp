# Sample Scenes & Demos (M6 Task 6.2)

| Scene | Path | Description | Key APIs | Output |
| --- | --- | --- | --- | --- |
| BasicShapes | `samples/BasicShapes` | Draws static rectangles/circles with gradients (requires Metal) | `ImpellerDisplayListBuilder`, `ImpellerPaint` | PNG screenshot & frame-timing log |
| Avalonia Impeller Surface | `samples/AvaloniaImpellerApp` | Avalonia app that leases SkiaSharp context via `ISkiaSharpApiLeaseFeature` and exposes Metal/Vulkan handles | `ImpellerSkiaView`, `ISkiaSharpApiLease`, Avalonia `Window` | Desktop window |
| TextureGallery | `samples/TextureGallery` (TBD) | Streams textures from disk, animates quads | `TextureFactory`, `Surface.DrawDisplayList` | GIF/Video capture |
| TextLayout | `samples/TextLayout` (TBD) | Renders paragraphs with different fonts/shaders | `Typography` (future), `ImpellerPaintSetColor` | PDF snapshot |

All samples should provide:
* Managed harness (`dotnet run`), optional CLI options for backend selection.
* Batching to stress `ExposureFactory`.
* Scripts to run during CI (headless) with golden image comparisons.
