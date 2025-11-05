# Sample Scenes & Demos (M6 Task 6.2)

| Scene | Path | Description | Key APIs | Output |
| --- | --- | --- | --- | --- |
| BasicShapes | `samples/BasicShapes` | CLI-selectable scenes: rect grid, streaming textures, or typography. Supports Metal window capture via `--capture` | `ImpellerDisplayListBuilder`, `TextureFactory`, `ImpellerTypographyContext` | PNG capture (macOS) + console timings |
| Avalonia Impeller Surface | `samples/AvaloniaImpellerApp` | Avalonia app that leases SkiaSharp context via `ISkiaSharpApiLeaseFeature` and exposes Metal/Vulkan handles | `ImpellerSkiaView`, `ISkiaSharpApiLease`, Avalonia `Window` | Desktop window |
| TextureGallery | `samples/TextureGallery` (TBD) | Streams textures from disk, animates quads | `TextureFactory`, `Surface.DrawDisplayList` | GIF/Video capture |
| TextLayout | `samples/TextLayout` (TBD) | Renders paragraphs with different fonts/shaders | `Typography` (future), `ImpellerPaintSetColor` | PDF snapshot |

All samples should provide:
* Managed harness (`dotnet run`), optional CLI options for backend selection.
* Batching or streaming modes to stress resource upload paths.
* Scripts or flags to run during CI (headless) with golden comparisons (see `build/scripts/export_basicshapes_golden.sh`).
