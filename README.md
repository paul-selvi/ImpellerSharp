# ImpellerSharp

Cross-platform .NET bindings for Flutter’s Impeller renderer with sample host applications and utilities for exploring high-performance, modern GPU rendering from managed code.

> **Project status:** Active R&D. APIs may change while we converge on a stable interop layer and hosting story.

---

## Highlights

- **SafeHandle-based interop** wrapping Impeller contexts, surfaces, paths, display lists, and paints for predictable lifetime management.
- **Host-agnostic rendering** that supports Metal, OpenGL(ES), and Vulkan contexts alongside Skia lease integrations.
- **Avalonia hosting packages** for macOS (Metal), Windows, and Linux, exposing reusable controls that surface Impeller via Skia leases or native swapchains.
- **Background display-list generation** in the Avalonia sample so heavy geometry preparation never blocks the UI thread.
- **Ready-to-run samples** demonstrating Impeller inside Avalonia UI and a Metal bootstrap for lower-level experiments.
- **Flutter engine integration** pulled in as a submodule, keeping the native renderer in sync with upstream builds.

---

## Repository Layout

```
extern/flutter                # Flutter engine checkout used to build libimpeller.dylib and headers
src/ImpellerSharp.Interop     # Core .NET interop assembly (P/Invoke bindings, SafeHandle wrappers, helpers)
src/ImpellerSharp.Native      # NuGet packaging project that harvests native binaries per RID
src/ImpellerSharp.Avalonia    # Shared Avalonia controls and Skia lease helpers
src/ImpellerSharp.Avalonia.Mac     # macOS Metal host (CAMetalLayer + dispatcher loop)
src/ImpellerSharp.Avalonia.Windows # Windows-friendly view (Win32 + Skia lease)
src/ImpellerSharp.Avalonia.Linux   # Linux view (Wayland/X11 + Skia lease)
samples/BasicShapes           # Minimal Metal host that brings up Impeller directly
samples/AvaloniaImpellerApp   # Avalonia UI sample showcasing Skia lease + Impeller rendering
docs/                         # Roadmaps, notes, benchmarking and design documents
tmp/                          # Local build scratch space (ignored)
```

---

## System Requirements

- macOS 13 or newer (native Impeller host + Avalonia Metal integration)
- Windows 10/11 (Avalonia Skia lease view; native Impeller swapchains are forthcoming)
- Ubuntu 22.04 or a recent Wayland/X11 distro with Vulkan drivers (Avalonia Skia lease view)
- Xcode command line tools (for macOS Metal builds)
- .NET 8 SDK (preview SDK supported, see `dotnet --info`)
- Python 3 and `depot_tools` for building the Flutter engine artifacts
- CMake/Ninja (installed via Homebrew/apt/choco as appropriate)

---

## Getting Started

### 1. Clone with Submodules

```bash
git clone https://github.com/yourname/ImpellerSharp.git
cd ImpellerSharp
git submodule update --init --recursive
```

### 2. Prepare the Flutter Engine Checkout

The Impeller renderer is built from the Flutter engine sources vendored in `extern/flutter`.

```bash
cd extern/flutter
gclient sync
```

The sync step populates the necessary toolchains and third-party dependencies required by Impeller.

### 3. Build `libimpeller.dylib`

Generate the build files and compile the host Impeller target. The commands below create a debug build for Apple Silicon; adjust flags (`--mac-cpu x64`, `--runtime-mode release`) as needed.

```bash
./engine/src/flutter/tools/gn \
  --unoptimized \
  --runtime-mode debug \
  --mac \
  --mac-cpu arm64 \
  --target-dir impeller_host_debug

ninja -C engine/src/out/impeller_host_debug flutter/impeller:impeller
```

Copy (or symlink) the produced `libimpeller.dylib` next to the managed binaries. The Avalonia sample’s project file already references the debug build output and copies it into the application bundle:

```
extern/flutter/engine/src/out/impeller_host_debug/libimpeller.dylib
```

### 4. Build the .NET Interop Layer

```bash
dotnet build src/ImpellerSharp.Interop
```

This compiles the managed P/Invoke bindings, SafeHandles, and helper utilities.

### 5. Run a Sample Application

#### Avalonia + Impeller Hybrid

```bash
dotnet run --project samples/AvaloniaImpellerApp
```

Features:
- Demonstrates driving Impeller through Avalonia’s Skia lease interface on macOS.
- A `DispatcherTimer` fetches CAMetalDrawables and hands them to Impeller for direct Metal presentation.
- `MotionMarkSimulation` runs on a dedicated background worker, continuously preparing Impeller display lists so UI responsiveness is maintained even at high complexities.
- Adjust the “Complexity” slider to scale the number of segments; the renderer swaps in prebuilt display lists when available.

#### Basic Metal Host

```bash
dotnet run --project samples/BasicShapes
```

This sample is a lower-level harness that hosts Impeller directly on top of Metal for quick experiments with new Impeller APIs. It assumes the Metal layer is available and configured by the sample bootstrap.

---

## Using the Interop Layer

The interop assembly models Impeller concepts with SafeHandles. Typical usage:

```csharp
using var context = ImpellerContextHandle.CreateMetal();
using var surface = ImpellerSurfaceHandle.WrapMetalDrawable(context, drawable);

var cullRect = new ImpellerRect(0f, 0f, width, height);
using var builder = ImpellerDisplayListBuilderHandle.Create(cullRect);

using var paint = ImpellerPaintHandle.Create();
paint.SetColor(new ImpellerColor(0f, 0.5f, 1f, 1f));

builder.DrawPaint(paint);

using var displayList = builder.Build();
surface.DrawDisplayList(displayList);
surface.Present();
```

Key points:
- All handles validate usage and throw `ImpellerInteropException` when native calls return failure codes.
- Handles are safe across threads as long as you respect Impeller’s native threading rules. The Avalonia sample builds display lists on a worker thread and presents them on the UI thread using atomic swaps.
- The interop API mirrors Impeller’s native naming for easier cross-referencing with C++ documentation.

### Creating Contexts

The managed bindings expose all Impeller context types:

```csharp
// Metal – macOS
using var metalContext = ImpellerContextHandle.CreateMetal();

// OpenGL / OpenGL ES – supply a proc-address callback (e.g., eglGetProcAddress)
IntPtr GlLoader(string name, IntPtr userData) => eglGetProcAddress(name);
using var glContext = ImpellerContextHandle.CreateOpenGLES(GlLoader, IntPtr.Zero);

// Vulkan – hook your proc loader and (optionally) enable validation
var vkSettings = new ImpellerContextVulkanSettings
{
    ProcAddressCallback = (instance, name, userData) => vkGetInstanceProcAddr(instance, name),
    UserData = IntPtr.Zero,
    EnableValidation = true,
};
using var vkContext = ImpellerContextHandle.CreateVulkan(vkSettings);

// Inspect the queued Vulkan objects managed by Impeller
if (vkContext.TryGetVulkanInfo(out var vkInfo))
{
    Console.WriteLine($"Device: 0x{vkInfo.PhysicalDevice:x}");
}

// Wrap a platform framebuffer for OpenGL/ANGLE scenarios (e.g., Windows)
var size = new ImpellerISize(width, height);
using var fboSurface = ImpellerSurfaceHandle.WrapFramebuffer(glContext, fbo, ImpellerPixelFormat.Bgra8888, size);
```

When Impeller adds swapchain builders for additional backends the bindings are ready—only native interop updates will be required.

### Textures & Color Sources

`ImpellerTextureHandle.CreateWithContents` uploads raw RGBA/RGBAF32 data directly to the GPU, wiring in a release callback so Impeller can free the staging buffer once the transfer finishes. You can also adopt an existing OpenGL texture via `CreateWithOpenGLHandle` and hand the resulting handle to:

- `ImpellerPaintHandle.SetColorSource` with an `ImpellerColorSourceHandle.CreateImage` lookup.
- `ImpellerDisplayListBuilder.DrawTexture` / `DrawTextureRect` for immediate sampling.
- Gradient helpers (`CreateLinearGradient`, `CreateRadialGradient`, etc.) that consume `Span<ImpellerColor>`/`Span<float>` stops and optional transform matrices.

### Typography & Paragraphs

The new typography bindings mirror the native C API:

```csharp
using var typography = ImpellerTypographyContextHandle.Create();

// Register a font family (bytes may come from an asset stream)
typography.RegisterFont(File.ReadAllBytes("Roboto-Regular.ttf"), familyAlias: "Roboto");

using var style = ImpellerParagraphStyleHandle.Create();
style.SetFontFamily("Roboto");
style.SetFontSize(24f);
style.SetTextAlignment(ImpellerTextAlignment.Center);

using var builder = ImpellerParagraphBuilderHandle.Create(typography);
builder.PushStyle(style);
builder.AddText("Hello Impeller Typography!");
using var paragraph = builder.Build(width: 400f);
```

Paragraphs can be added to display lists with `DrawParagraph`, and metrics such as `GetHeight()`/`GetMaxWidth()` are exposed for layout decisions. Paragraph styles accept full paint customization (foreground/background), font weight/style, locale, text direction, and decoration masks.

### C API Coverage

The managed bindings cover the major surfaces of the Impeller toolkit C API:

- **Contexts & Surfaces** – create Metal, OpenGL(ES), or Vulkan contexts; wrap CAMetalDrawables, Metal textures, or OpenGL framebuffers; draw/present display lists.
- **Textures & Sampling** – upload raw pixel data or adopt OpenGL textures and pass them to paints, color sources, or display-list texture draws with configurable sampling/tile modes.
- **Color Sources & Paint** – configure solid colors, gradients (linear/radial/conical/sweep), image sources, or fragment programs and attach them to `ImpellerPaintHandle` instances.
- **Paths & Display Lists** – build paths, issue draw commands (rects, shapes, textures, paragraphs), and manage save/restore and transforms.
- **Typography** – register fonts, create paragraph styles, build paragraphs, read layout metrics, and draw paragraphs into display lists.

Impeller’s standalone **compute** API does not yet expose public C entry points; once upstream support lands the same pattern of SafeHandle bindings will be added.

---

## Avalonia Hosting Packages

Reusable Avalonia controls ship as separate NuGet packages:

- `ImpellerSharp.Avalonia` – base Skia-lease control (`ImpellerSkiaView`) and render event args that expose Metal/Vulkan surfaces when available.
- `ImpellerSharp.Avalonia.Mac` – native `MetalNativeHost` wrapping `CAMetalLayer` and dispatching CAMetalDrawables to Impeller.
- `ImpellerSharp.Avalonia.Windows` – `Win32ImpellerView`, a ready-made Skia lease surface for Win32/ANGLE backends.
- `ImpellerSharp.Avalonia.Linux` – `LinuxImpellerView`, the matching Skia lease control for Wayland/X11.

### Example

```csharp
using ImpellerSharp.Avalonia.Controls;
using ImpellerSharp.Avalonia.Mac.Controls;
using ImpellerSharp.Avalonia.Windows.Controls;

Control ChooseHost()
{
    if (OperatingSystem.IsMacOS())
    {
        var metalHost = new MetalNativeHost();
        metalHost.RenderMetal += (sender, args) =>
        {
            if (_renderer.TryRenderMetal(args))
            {
                args.MarkRendered();
            }
        };
        return metalHost;
    }

    var view = OperatingSystem.IsWindows()
        ? new Win32ImpellerView()
        : new ImpellerSkiaView();

    view.RenderSkia += (sender, args) =>
    {
        _renderer.TryRenderSkia(args);
        args.ResetPlatformLease();
    };

    return view;
}
```

Each view/control can be consumed directly from XAML or code-behind once the corresponding package reference is added.

- `ImpellerSkiaRenderEventArgs` keeps the Skia lease alive and exposes:
  - `Backend` – identify whether the surface is backed by Metal, Vulkan, OpenGL/Direct3D, or software.
  - `MetalDevice`, `VulkanContext`, and the raw `PlatformContext` so you can negotiate native handles directly with Avalonia when they exist.
  - `TryGetMetalRenderTarget` – returns the underlying `CAMetalDrawable` so macOS hosts can hand the drawable to Impeller today.
  These hints let you detect the GPU backend across macOS, Windows, and Linux, retain native surface handles already available through Avalonia, and prepare to plug them into future Impeller APIs as soon as additional upstream entry points land.

---

## Sample: MotionMark-Inspired Simulation

`samples/AvaloniaImpellerApp/Rendering/MotionMarkSimulation.cs` emulates the heavy vector workloads of the MotionMark benchmark:

- Randomized segments (lines, quads, cubics) traversing an 80×40 logical grid.
- Thousands of strokes rendered with varying widths and colors per frame.
- Background worker rebuilds display lists whenever the viewport size or complexity changes, preventing frame stalls on the UI thread.

Use the slider to explore Impeller’s geometry throughput. The renderer drops stale work if the worker falls behind, keeping frame pacing predictable.

### Sample: Native MotionMark Host (Metal)

The `samples/MotionMark` project is a lightweight macOS console host that drives Impeller directly through the managed bindings:

- Creates a GLFW window, wraps the underlying `CAMetalLayer`, and hands each `CAMetalDrawable` to Impeller via `ImpellerSurfaceHandle`.
- Renders a MotionMark-inspired “particle storm” animation entirely on the GPU using display lists, blend modes, and per-frame transforms.
- Demonstrates how to keep the Impeller context alive via `SafeHandle` retain/release calls and how to marshal Objective‑C selectors from .NET.

To run it (after ensuring `ImpellerSharp.Native` binaries are on your `PATH`/`DYLD_LIBRARY_PATH`):

```bash
dotnet run --project samples/MotionMark/MotionMark.csproj
```

First stage the native assets for the Release configuration (copy your built `libimpeller` binaries into `src/ImpellerSharp.Native/bin/Release/net8.0/runtimes/...`) so the sample can load them. The helper `python3 build/native/build_impeller.py --platform macos --arch arm64 --configuration Release` will build Impeller via GN/Ninja and place the results in both that location and `artifacts/native`.

The sample depends on the bundled Silk.NET GLFW runtime and currently supports macOS + Metal targets.

### Sample: MotionMark Original (Metal)

`samples/MotionMarkOriginal` recreates MotionMark’s classic “Multiply” scene using the same Metal host:

- Builds the same spiral/side-panel grid of rectangles and progressively activates them as complexity increases.
- Matches the benchmark’s per-frame styling by rotating each tile, modulating colour with the HSL controller, and tapering alpha by the benchmark’s distance factor.
- Shares the reusable `MetalGlfwAppHost` infrastructure, illustrating how multiple native samples can be layered on the common hosting code.

Run it with:

```bash
dotnet run --project samples/MotionMarkOriginal/MotionMarkOriginal.csproj
```

Make sure the native binaries are prepared for Release builds as noted above before launching the sample.

---

## NuGet Packaging

ImpellerSharp publishes the following NuGet packages:

- `ImpellerSharp.Native` – platform-specific runtime assets (`runtimes/<rid>/native/*`) containing `libimpeller`, `impeller.dll`, and related native files.
- `ImpellerSharp.Interop` – managed bindings (depends on `ImpellerSharp.Native`).
- `ImpellerSharp.Avalonia` – cross-platform Avalonia controls and render infrastructure.
- `ImpellerSharp.Avalonia.Mac` – macOS Metal host control.
- `ImpellerSharp.Avalonia.Windows` – Windows Skia lease view.
- `ImpellerSharp.Avalonia.Linux` – Linux Skia lease view.

### Build Native Artifacts

Run the native build helper on each supported platform. The script invokes `gclient sync`, GN, and Ninja, then copies the outputs to `artifacts/native/<rid>/native`.

```bash
python3 build/native/build_impeller.py --platform macos --arch arm64 --configuration Release
python3 build/native/build_impeller.py --platform linux --arch x64 --configuration Release
python build/native/build_impeller.py --platform windows --arch x64 --configuration Release
```

The resulting layout:

```
artifacts/native/
  osx-arm64/native/libimpeller.dylib
  linux-x64/native/libimpeller.so
  win-x64/native/impeller.dll
  win-x64/native/impeller.lib
  win-x64/native/impeller.pdb
```

### Pack NuGet Artifacts

After all native runtimes are available, pack the managed projects:

```bash
dotnet pack src/ImpellerSharp.Native/ImpellerSharp.Native.csproj -c Release -o artifacts/nuget
dotnet pack src/ImpellerSharp.Interop/ImpellerSharp.Interop.csproj -c Release -o artifacts/nuget
dotnet pack src/ImpellerSharp.Avalonia/ImpellerSharp.Avalonia.csproj -c Release -o artifacts/nuget
dotnet pack src/ImpellerSharp.Avalonia.Mac/ImpellerSharp.Avalonia.Mac.csproj -c Release -o artifacts/nuget
dotnet pack src/ImpellerSharp.Avalonia.Windows/ImpellerSharp.Avalonia.Windows.csproj -c Release -o artifacts/nuget
dotnet pack src/ImpellerSharp.Avalonia.Linux/ImpellerSharp.Avalonia.Linux.csproj -c Release -o artifacts/nuget
```

Versioning is coordinated by `Directory.Build.props`, keeping all packages in lock-step. Publish the resulting `.nupkg` files to your chosen feed (NuGet.org, GitHub Packages, etc.).

---

## Development Workflow

1. Make native changes in the Flutter engine if new Impeller features are required.
2. Rebuild `libimpeller.dylib` and re-run the managed samples to validate interop changes.
3. For .NET changes, rely on `dotnet build`/`dotnet test` (where available). Most code is library/sample focused, so tests live downstream for now.
4. Keep SafeHandle lifetime boundaries tight—wrap all native pointers immediately after allocation and dispose aggressively to avoid GPU resource leaks.

---

## Troubleshooting

- **Application hangs under load:** Confirm the background worker is delivering display lists (watch console logs). Extremely high complexities can still overwhelm Impeller; reduce the slider or enable profiling.
- **`ImpellerInteropException`:** Indicates a native call failed (usually a null return). Confirm `libimpeller.dylib` is in the application directory and matches the target architecture.
- **Metal validation errors:** Run with `MTL_DEBUG_LAYER=1` to surface GPU-side issues during experimentation.

---

## Continuous Integration

Two GitHub Actions workflows keep the repo continuously validated:

- `.github/workflows/build.yml` runs for PRs and pushes. It compiles native Impeller binaries on macOS/Linux/Windows, builds the managed solution, and emits all NuGet packages (`ImpellerSharp.Native`, `ImpellerSharp.Interop`, `ImpellerSharp.Avalonia*`) as artifacts.
- `.github/workflows/release.yml` triggers on `v*` tags (or manually). It repeats the native + managed build, publishes packages to NuGet when `NUGET_API_KEY` is supplied, and attaches the generated `.nupkg` files to a GitHub release.

Together these pipelines guarantee every change is exercised against fresh native bits and that shipping a release is a single-tag operation.

---

## Roadmap

- Maintain the expanded text/image bindings and add compute pipelines once the standalone C API exposes the necessary entry points.
- Support Vulkan/Direct3D hosting once the native Impeller backends stabilize.
- Add automated performance benchmarks mirroring MotionMark and Design Studio workloads.
- Harden cross-platform build pipelines (GitHub Actions runners with prebuilt Impeller artifacts).

See `docs/impeller-interop-plan.md` for detailed milestones and design notes.

---

## License

This project is released under the MIT license. Flutter and Impeller remain governed by their respective licenses—review the Flutter engine repository for third-party notices.
