# ImpellerSharp Developer Guide

This guide explains how to integrate the ImpellerSharp interop layer into your .NET applications, manage threading and memory, work with shaders, and troubleshoot common issues. It assumes you have already built the native Impeller SDK and the managed `ImpellerSharp.Interop` library.

## 1. Getting Started

### Prerequisites
- Native Impeller SDK build (see `docs/ci-plan.md` for GN/Ninja steps).
- .NET SDK 8.0 or later.
- Platform SDKs:
  - macOS: Xcode command-line tools & Metal toolchain.
  - Linux: Vulkan SDK (LunarG) or SwiftShader for software fallback.
  - Windows: Visual Studio build tools + Vulkan SDK (future support).

### Project Setup
1. Add a project reference to `src/ImpellerSharp.Interop/ImpellerSharp.Interop.csproj`.
2. Ensure native binaries (`libimpeller.*`, `impeller.h`) are available at runtime (copy to output or configure `LD_LIBRARY_PATH`/`DYLD_LIBRARY_PATH`).
3. (Optional) Reference `samples/BasicShapes` for basic wiring.

### Minimal Code Sample
```csharp
using ImpellerSharp.Interop;

using var context = ImpellerContextHandle.CreateMetal(); // macOS example
var descriptor = new ImpellerTextureDescriptor(
    ImpellerPixelFormat.Rgba8888,
    new ImpellerISize(256, 256));

var pixels = new byte[descriptor.Size.Width * descriptor.Size.Height * 4];
using var texture = context.CreateTexture(descriptor, pixels);

// TODO: wrap swapchain/surface and draw display list
```

## 2. Threading Model
- `ImpellerContextHandle` is thread-safe, but `CommandBuffer` encoding must happen on a single thread. Use multiple command buffers to parallelize work.
- Background texture uploads: `TextureFactory.CreateTexture` may pin buffers and invoke release callbacks on worker threads. Use thread-safe data structures for baton state.
- For UI frameworks, execute main rendering on the UI thread while offloading uploads to background tasks.

## 3. Memory Management
- Handles derive from `SafeHandle`. Always dispose them or use `using`.
- Native `Retain`/`Release` semantics are encapsulated in methods like `Retain()`, `DangerousGetHandle()`.
- When uploading texture data, prefer span-based APIs that copy into pooled pinned buffers. Provide release callbacks for long transfers.
- Consider adding a strict mode (see `docs/benchmarking-plan.md`) to detect unexpected allocations or double releases during development.

## 4. Shader & Pipeline Updates
- Use `ImpellerFragmentProgramNew` to load precompiled shader blobs generated via `impellerc`. Cache `ImpellerFragmentProgram` handles.
- For runtime shader effects, integrate with `RuntimeStage::DecodeRuntimeStages` by exposing managed struct mirrors (future work).
- Warm up pipelines by rendering off-screen once; record timings via `ImpellerDiagnostics.ActivitySource`.

## 5. Diagnostics & Profiling
- `ImpellerDiagnostics` forwards context creation, texture uploads, and draw submissions to `ImpellerEventSource` and an `ActivitySource`. Subscribe via `EventListener` or `DiagnosticSource`.
- Enable strict guardrails via `AppContext.SetSwitch("ImpellerSharp.Interop.StrictMode", true)` or `IMPELLER_INTEROP_STRICT=1`. The strict configuration (see `ImpellerInteropOptions`) validates texture payload sizes and throws immediately on surface draw failures—ideal for catching misuse during development.
- For managed allocation profiling, follow the steps in `docs/benchmarking-plan.md` (dotnet-trace, dotnet-counters).
- Integrate BenchmarkDotNet scenarios (`BL-DrawRect`, `TextureUpload`) to detect regressions.

## 6. Troubleshooting
- **Version mismatch**: Ensure `ImpellerContextHandle.Create*` passes `ImpellerNative.ImpellerGetVersion()`.
- **Null returns**: Check that native binaries are discoverable and that required backends (Metal/Vulkan) are present.
- **Threading violations**: Avoid using surfaces/command buffers after disposal; check logs for ActivitySource events with failure status.
- **Strict mode exceptions**: `ImpellerInteropOptions` may throw if resources are misused (e.g., undersized texture data). Disable strict mode in production or update callsites to satisfy the stricter contracts.
- **Dynamic library load failures**: On macOS, configure `DYLD_LIBRARY_PATH` or embed `libimpeller.dylib` via `rpath`. On Linux/Windows (future), use RID assets in NuGet packaging.
