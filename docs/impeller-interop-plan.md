# Impeller Standalone Engine & .NET Interop Plan

## Overview
This plan distills the Impeller architecture and tooling documented in `extern/flutter/docs/engine/impeller` and the source layout under `extern/flutter/engine/src/flutter/impeller`. The goal is to host Impeller as a standalone rendering engine and surface a high-performance managed API via C ABI bindings tailored for modern .NET interop (`LibraryImport`, function pointers, `Span<T>`, `Unsafe`, source generators). The steps below emphasize predictability, low memory overhead, and minimizing crossings between managed and native code.

## Architectural Highlights
- **Core hierarchy**: `impeller/renderer` provides backend-agnostic primitives; `impeller/entity`, `impeller/runtime_stage`, and `impeller/display_list` sit above it to implement Flutter semantics. Backends live under `impeller/renderer/backend` and are swapped at build time.
- **Offline pipeline**: `impellerc` in `impeller/compiler` handles shader transpilation, reflection metadata generation, and pipeline bindings; standalone usage must replicate the build orchestration described in `BUILD.gn`.
- **Subsystem utilities**: `impeller/geometry` is a shared math library usable in host/device code; `impeller/toolkit`, `impeller/playground`, and `entity` offer testing hooks that can guide validation.
- **Runtime integration points**: Flutter depends on FML for threading, task runners, and timekeeping. Impeller also interfaces with `display_list` dispatchers to consume Flutter drawing commands—key considerations when exposing a C ABI.
- **Platform specifics**: Metal and Vulkan backends currently receive first-class support, with MoltenVK guidance in `docs/engine/impeller/Setting-up-MoltenVK-on-macOS-for-Impeller.md`. Standalone hosting must decide the minimal backend set per target OS.

## Milestones & Tasks
1. [x] Milestone 1 – Establish standalone Impeller build baseline  
   - [x] Task 1.1 – Inventory default GN targets in `engine/src/flutter/impeller/BUILD.gn` and identify minimal deps (FML, third-party libs).  
   - [x] Task 1.2 – Prototype GN/Ninja invocation that builds `impeller/renderer` and a single backend (Metal or Vulkan) without Flutter shell.  
   - [x] Task 1.3 – Document external toolchain prerequisites (Clang, MoltenVK, SPIR-V tools) and align with .NET build environments.

2. [x] Milestone 2 – Define standalone runtime surface  
   - [x] Task 2.1 – Map runtime entry points needed for resource lifecycle, command submission, and frame scheduling derived from `impeller/runtime_stage` and `impeller/entity`.  
   - [x] Task 2.2 – Specify threading model reusing `fml::TaskRunner` or replacement abstractions, with explicit ownership semantics.  
   - [x] Task 2.3 – Decide asset ingestion format (e.g., display lists, custom scene graph) and required compatibility adapters.

3. [x] Milestone 3 – Design C ABI layer  
   - [x] Task 3.1 – Enumerate types crossing the boundary (textures, buffers, pipelines) and draft POD-friendly structs mirroring Impeller types in `impeller/renderer`.  
   - [x] Task 3.2 – Specify lifetime management APIs (create/destroy/retain) with deterministic disposal and explicit arena ownership.  
   - [x] Task 3.3 – Define callback hooks for diagnostics, logging, and asynchronous resource loading using pure C function pointers.  
   - [x] Task 3.4 – Validate ABI stability across compilers (MSVC, Clang, GCC) with unit tests using `impeller/playground` fixtures.

4. [x] Milestone 4 – Implement .NET interop layer  
   - [x] Task 4.1 – Generate P/Invoke signatures using `LibraryImport` source generators targeting the C ABI, with `delegate* unmanaged` trampolines for hot paths.  
   - [x] Task 4.2 – Wrap GPU resource handles in safe abstractions employing `SafeHandle` only where necessary; prefer `ref struct` spans for transient data.  
   - [x] Task 4.3 – Provide span-friendly upload APIs (`ReadOnlySpan<byte>`, `Span<float>`) that pin or stack-allocate buffers to minimize GC pressure.  
   - [x] Task 4.4 – Expose command encoder APIs via function-pointer tables (`delegate*` caches) to avoid repeated lookup overhead.  
   - [x] Task 4.5 – Integrate diagnostics via `EventSource` or `ActivitySource` with opt-in native tracing interop.
   - [x] Task 4.6 – Mirror advanced Impeller toolkit APIs (fragment programs, color/mask/image filters, typography metrics, display-list transforms & clipping) in managed bindings.

5. [ ] Milestone 5 – Memory, performance, and safety validation  
   - [ ] Task 5.1 – Benchmark baseline frame submission latency comparing direct native usage vs. managed wrapper using representative workloads.  
     - Status: Benchmark harness now includes a native ABI baseline via `NativeDisplayListBenchmark` plus the new `TextureUploadBenchmark` (managed vs. C ABI texture streaming), emits GitHub-friendly Markdown/JSON exports, and the Linux CI leg uploads `BenchmarkArtifacts` for review.  
     - Next: Expand scenarios to cover surface submission and end-to-end frame presentation, then correlate managed/native timings in dashboard form.  
   - [ ] Task 5.2 – Profile allocations with .NET tracing tools (`dotnet-trace`, `EventPipe`) ensuring zero allocations per frame in hot paths.  
     - Status: Guarded texture uploads in `TextureFactory.CreateTexture` minimize managed churn; `build/scripts/collect_allocations.sh` wraps `dotnet-trace` over the BenchmarkDotNet harness to produce `.nettrace` captures for texture-upload scenarios.  
     - Next: Add Speedscope/summary post-processing and integrate results into CI dashboards.  
   - [ ] Task 5.3 – Implement stress tests that exercise texture streaming, large uniform buffers, and shader warm-up; monitor GPU/CPU synchronization.  
     - Status: Stress scenarios enumerated in `docs/benchmarking-plan.md`; sample harnesses not yet implemented.  
     - Next: Extend `samples/BasicShapes` with configurable texture streaming loops and uniform buffer builders guarded by CLI options.  
   - [ ] Task 5.4 – Add guardrails for misuse (debug asserts, optional contracts) without impacting release builds.  
     - Status: `ImpellerInteropOptions` enables strict mode by configuration/AppContext switches, enforcing length checks for texture uploads and surfacing draw failures immediately in `ImpellerSurfaceHandle.DrawDisplayList`.  
     - Next: Expand strict mode to validate thread affinity and SafeHandle disposal, and expose guard-rail telemetry (e.g., via EventSource counters).  

6. [ ] Milestone 6 – Tooling, packaging, and documentation  
   - [x] Task 6.1 – Automate GN/Ninja + .NET builds via CI scripts (GitHub Actions/Azure Pipelines) producing native binaries and NuGet packages.  
     - Status: `impeller-ci.yml` stages downloaded native artifacts before packing, persists benchmark outputs, and conditionally publishes NuGet packages when `NUGET_SOURCE`/`NUGET_API_KEY` are configured.  
   - [ ] Task 6.2 – Provide sample scenes demonstrating standalone Impeller usage from .NET, including MoltenVK setup guidance.  
     - Status: `samples/BasicShapes` now supports backend selection (`--backend`), headless golden exports (`--headless` + `--output`), texture streaming stressors, typography rendering, and a macOS `screencapture` pipeline; `build/scripts/export_basicshapes_golden.sh` drives scripted comparisons. MoltenVK guidance and cross-platform presentation hosts remain open.  
     - Next: Validate Vulkan headless mode on CI (MoltenVK/Linux), document setup guidance, and spin out TextureGallery/TextLayout as standalone samples.  
   - [x] Task 6.3 – Write developer guides covering threading, memory ownership, shader pipeline updates, and troubleshooting.  
     - Output: `docs/developer-guide.md` and `docs/developer-docs-outline.md` ship comprehensive guidance across the required topics.  
   - [x] Task 6.4 – Establish compatibility matrix by platform, backend, and .NET runtime (CoreCLR, NativeAOT, Mono/Unity).  
     - Output: `docs/compatibility-matrix.md` tracks target platforms/backends, runtime support goals, and stretch environments.

## Milestone 1 Execution Notes
- **Task 1.1**: `//flutter/impeller:impeller` reduces to `base`, `geometry`, and `tessellator` when rendering is disabled, and pulls in `display_list`, `entity`, `renderer`, `renderer/backend`, and `typographer/backends/skia` when `impeller_supports_rendering` is true. Core dependencies flow through `//flutter/fml`, while `tessellator` adds `//third_party/libtess2` and backend targets add platform frameworks (`Metal.framework`, `MetalPerformanceShaders.framework`) or Vulkan/GL loaders.
- **Task 1.2**: Prototype flow uses GN from `engine/src/flutter/tools/gn` (requires `vpython3` via depot_tools). Example host Metal build:  
  ```bash
  # gclient sync after installing depot_tools to fetch gn, ninja, toolchains
  ./engine/src/flutter/tools/gn --unoptimized --runtime-mode debug --target-dir impeller_host_debug --mac --mac-cpu arm64 --no-lto
  gn gen out/impeller_host_debug --root=engine/src --args='impeller_enable_metal=true impeller_enable_opengles=false impeller_enable_vulkan=false impeller_supports_rendering=true'
  ninja -C engine/src/out/impeller_host_debug flutter/impeller:impeller
  ```
  Swap `impeller_enable_metal` for Vulkan or OpenGLES to target other backends, and specify `flutter/impeller/renderer/backend/metal:metal` if compiling a single backend.
- **Task 1.3**: Toolchain prerequisites include depot_tools (`gclient`, `vpython3`, `gn`, `ninja`), Xcode command-line tools (Clang, MetalKit), MoltenVK or the LunarG Vulkan SDK for Vulkan builds, and Python 3. Align .NET work by ensuring .NET 8+ SDK (for `LibraryImport`, NativeAOT) and CMake/Ninja integration on CI hosts.
- **Task 1.4 (ad-hoc)**: Install Apple's Metal SDK with `xcodebuild -downloadComponent MetalToolchain` before invoking Ninja; Impeller metallib generation now completes and `ninja -C engine/src/out/impeller_host_debug flutter/impeller:impeller` finishes without work remaining on subsequent invocations.

## Milestone 2 Execution Notes
- **Task 2.1 – Runtime entry points & lifecycle**  
  - `Context` is the canonical device handle, exposing allocator, shader, pipeline, and sampler factories while remaining thread-safe for creation and use (`CreateCommandBuffer`, `GetCommandQueue`, `Shutdown`) [extern/flutter/engine/src/flutter/impeller/renderer/context.h].  
  - Backend contexts (e.g. `ContextMTL::Create`, `ContextVK::Create`) stitch together device queues, shader libraries, and capability discovery before marking themselves valid, making them the construction target for a standalone loader [extern/flutter/engine/src/flutter/impeller/renderer/backend/metal/context_mtl.mm; extern/flutter/engine/src/flutter/impeller/renderer/backend/vulkan/context_vk.cc].  
  - Per-frame work allocates `CommandBuffer`, `RenderPass`, and `Blit/Compute` passes, submits through `CommandQueue::Submit`, and optionally surfaces via `Surface::Present` or `Context::SubmitOnscreen` (`command_buffer.h`, `command_queue.h`, `render_pass.h`, `surface.h`).  
  - Resource targets are abstracted via `RenderTarget` and allocator helpers, defining attachments, MSAA resolve behavior, and lifecycle for onscreen/offscreen textures [extern/flutter/engine/src/flutter/impeller/renderer/render_target.h].  
  - Shader/runtime customization enters through `RuntimeStage::DecodeRuntimeStages`, yielding backend-specific entrypoints, descriptor layouts, and UBO bindings that can be cached in managed wrappers [extern/flutter/engine/src/flutter/impeller/runtime_stage/runtime_stage.h].
- **Task 2.2 – Threading and ownership model**  
  - `Context` guarantees thread-safe usage but enforces single-threaded encoding per `CommandBuffer`; multi-threaded workloads spawn additional buffers and submit them in order (`context.h`, `command_buffer.h`).  
  - Backends such as Vulkan provision dedicated worker loops via `fml::ConcurrentMessageLoop` and cap worker counts with `ContextVK::ChooseThreadCountForWorkers`, signalling the need for host-provided executors or bridges to existing runtime schedulers [extern/flutter/engine/src/flutter/impeller/renderer/backend/vulkan/context_vk.cc].  
  - Submission order and GPU completion are surfaced through `CommandQueue::Submit` callbacks (Metal/Vulkan provide status) and blocking helpers `WaitUntilScheduled/Completed`, informing how managed code should map to tasks/Futures without risking deadlocks (`command_queue.h`, `command_buffer.h`).  
  - `ImpellerContextFuture` allows deferred context resolution on background threads, suggesting a pattern for NativeAOT or async factories prior to exposing handles to managed consumers (`context.h`).
- **Task 2.3 – Asset ingestion & adapters**  
  - High-level scene emission can reuse Flutter display lists through `AiksContext`, wrapping an Impeller `ContentContext` and optional `RenderTargetAllocator` for cached surfaces [extern/flutter/engine/src/flutter/impeller/display_list/aiks_context.h; extern/flutter/engine/src/flutter/impeller/entity/contents/content_context.h].  
  - Custom pipelines can emit direct `Entity` streams or build display-list equivalents; `ContentContextOptions` enumerates pipeline variants (blend, stencil, sample count) to mirror when constructing ABI structs (`content_context.h`).  
  - Render target creation (`RenderTarget::SetupDepthStencilAttachments`, `RenderTargetAllocator::CreateOffscreen`) and texture staging (via `HostBuffer`/`TextureDescriptor` in the same module) define the structure for managed upload APIs [extern/flutter/engine/src/flutter/impeller/renderer/render_target.h; extern/flutter/engine/src/flutter/impeller/entity/contents/content_context.h].  
- Runtime shader blobs delivered over the C ABI can be fed into `RuntimeStage` maps, providing compiled code mappings and uniform layouts for direct binding from managed spans (`runtime_stage.h`).

## Milestone 3 Execution Notes
- **Task 3.1 – ABI surface & POD types**  
  - `impeller/toolkit/interop/impeller.h` already publishes opaque handles for core objects (`ImpellerContext`, `ImpellerSurface`, `ImpellerTexture`, `ImpellerFragmentProgram`) via `IMPELLER_DEFINE_HANDLE`, alongside non-opaque structs (`ImpellerRect`, `ImpellerMatrix`, `ImpellerColor`, etc.) suitable for direct marshaling [extern/flutter/engine/src/flutter/impeller/toolkit/interop/impeller.h:120-436].  
  - Enums cover render semantics (`ImpellerBlendMode`, `ImpellerDrawStyle`, `ImpellerPixelFormat`, `ImpellerTileMode`, typography enums) ensuring managed bindings can mirror native pipeline flags without reinterpret_casts (`impeller.h:400-520`).  
  - For Vulkan WSI, `ImpellerVulkanSwapchain` and `ImpellerSurfaceCreateWrapped*` provide explicit interop entrypoints to wrap platform objects (EGL FBOs, CAMetalDrawable, VkSurfaceKHR) to keep the C ABI solely data-oriented (`impeller.h:820-880`).
- **Task 3.2 – Lifetime & ownership**  
  - Consistent `New`/`Retain`/`Release` trio per handle enforces reference-counted ownership (documented in README “Reference Management”). Functions like `ImpellerContextCreateMetalNew`, `ImpellerSurfaceRetain/Release`, `ImpellerTextureNewWithContents` set the lifecycle contract and can be surfaced to managed code via safe handles [extern/.../impeller.h:620-900, README.md:102-118].  
  - Nullability annotations (`IMPELLER_NONNULL`/`_NULLABLE`) and `IMPELLER_NODISCARD` macros allow generated bindings to enforce safety checks; managed wrappers should mirror the same semantics for disposal patterns (`impeller.h:60-115`).  
  - Explicit destruction for GPU resources (textures, swapchains) prevents leaking driver allocations; long-lived objects (context, typography) are expensive, so managed caches can map one-to-one with native handles.
- **Task 3.3 – Callback hooks**  
  - ABI defines `ImpellerCallback`, `ImpellerProcAddressCallback`, and `ImpellerVulkanProcAddressCallback` for diagnostics, shader loading, and function discovery, with user data batons captured as `void*` [extern/.../impeller.h:470-500].  
  - These callbacks are invoked on backend-defined threads (e.g., Vulkan worker loop), so managed bindings must pin delegates and ensure thread-safe marshalling (e.g., `delegate* unmanaged` trampolines with `GCHandle` batons).  
  - Logging/diagnostics can leverage existing callback slots or introduce additional externs that forward to managed event sinks, mirroring Impeller’s existing observer pattern for GPU tracing.
- **Task 3.4 – Cross-compiler validation**  
  - Impeller’s README notes current lack of long-term ABI guarantees; propose nightly CI runs that compile `impeller.h` against MSVC, Clang, GCC with generated bindings to assert binary compatibility (function signatures, struct packing).  
  - Unit tests can reuse `impeller/toolkit/interop` examples plus `impeller/playground` harnesses to validate that ABI-wrapped contexts render identically across toolchains.  
  - Versioning scheme (`IMPELLER_VERSION` and `ImpellerGetVersion`) already in place to gate managed loader—CI should verify version negotiation and fallback strategies when ABI changes.

## Milestone 4 Planning Notes
- **Task 4.1 – LibraryImport signatures**  
  - Organize DllImports as `partial` methods inside `static partial class ImpellerNative` per ABI family (core, rendering, typography). Each entry uses `LibraryImport("impeller", EntryPoint = "...", StringMarshalling = StringMarshalling.Utf8)` with `SetLastError = false`. Example:  
    ```csharp
    [LibraryImport(ImpellerLibrary, EntryPoint = "ImpellerContextCreateMetalNew")]
    internal static partial nint ImpellerContextCreateMetalNew(uint version);
    ```  
    Handles are expressed as `nint` to avoid marshaling overhead; pointer arguments (e.g., `ImpellerTextureDescriptor`) map to `ref`/`in` `struct` parameters backed by blittable layouts mirroring `extern/flutter/engine/src/flutter/impeller/toolkit/interop/impeller.h:624-670`.  
  - Tag hot-path imports (draw ops, uploads) with `[SuppressGCTransition]` once profiling confirms benefit, and expose optional overloads that accept `ReadOnlySpan<T>` while forwarding to unsafe partials.
- **Task 4.2 – SafeHandle and wrappers**  
  - Derive `abstract class ImpellerSafeHandle : SafeHandleZeroOrMinusOneIsInvalid` that stores a `delegate* unmanaged<nint, void>` releaser; subclass per handle (`ContextHandle`, `SurfaceHandle`, `TextureHandle`) to call `Impeller...Release` (`impeller.h:820-900`).  
  - For transient handles (display list builders, paragraph builders) prefer `ref struct` wrappers holding `nint` and scoped disposal (C# 11 `ref struct` + `Dispose`). This minimizes allocations while keeping ownership explicit.  
  - Provide factory methods returning `SafeHandle` so callers can wrap once and reuse; expose `DangerousAddRef`-style helpers when interop needs manual retention via `Impeller...Retain`.
- **Task 4.3 – Span-based uploads**  
  - Implement helper `struct ImpellerMappingRef` that pins `ReadOnlySpan<byte>` via `fixed`/`MemoryMarshal.GetReference` and fills `ImpellerMapping` before invoking `ImpellerTextureCreateWithContentsNew` (`impeller.h:1298-1342`).  
  - Use `delegate* unmanaged` release callbacks to unpin spans when the native side signals completion; fallback to eager copy for small payloads using pooled buffers (`ArrayPool<byte>`).  
  - Expose managed APIs `TextureHandle Texture.Create(ReadOnlySpan<byte> pixels, in TextureDescriptor desc, Action? release)` that execute on dedicated upload schedulers to avoid blocking UI threads.
- **Task 4.4 – Command encoder bridging**  
  - Cache unmanaged function pointers for frequently invoked draw operations (e.g., `ImpellerDisplayListBuilderDrawPath`, `ImpellerSurfaceDrawDisplayList`) in static readonly fields of type `delegate* unmanaged<...>` obtained via `LibraryImport` partials to bypass repeated marshalling.  
  - Layer managed command encoder classes that batch state changes, deferring to the function pointers within `unsafe` contexts. Where the ABI expects callbacks (`ImpellerProcAddressCallback`), forward to managed `Dictionary<string, nint>` to keep lookups O(1).
- **Task 4.5 – Diagnostics integration**  
  - Wrap `ImpellerCallback` and `ImpellerProcAddressCallback` (`impeller.h:470-500`) with marshaled delegates that enqueue events into `ChannelWriter<ImpellerLogEvent>` processed by an `EventSource`/`ActivitySource` publisher.  
  - Surface tracing toggles via `ActivitySource.StartActivity` around submission points; when Vulkan validation is enabled, capture swapchain messages and emit `EventWritten` payloads for tooling.  
  - Ensure callbacks execute on thread-pool threads to avoid deadlocks with Impeller worker loops; use `GCHandle.Alloc` to pin state until `on_release` fires.

### Milestone 4 Implementation Shapshot
- `src/ImpellerSharp.Interop/ImpellerNative.*` introduces the first `LibraryImport` partials for contexts, textures, surfaces, display lists, and paints, matching `impeller/toolkit/interop/impeller.h`.
- Safe handle wrappers (`ImpellerContextHandle`, `ImpellerSurfaceHandle`, `ImpellerTextureHandle`, `ImpellerDisplayListHandle`, `ImpellerDisplayListBuilderHandle`, `ImpellerPaintHandle`) centralise retain/release semantics and expose managed helpers (`DrawDisplayList`, `Build`, `SetColor`, etc.).
- `TextureFactory.CreateTexture` demonstrates span-friendly uploads with pinned buffers, GCHandle batons, and unmanaged release callbacks.
- `ImpellerCommandPointers` caches the hot-path `ImpellerSurfaceDrawDisplayList` export as a `delegate* unmanaged[Cdecl]` to minimise dispatch overhead during frame submission, and `ImpellerDiagnostics` + `ImpellerEventSource` bridge ActivitySource/EventSource telemetry for context creation, texture uploads, and draw submissions.

## Milestone 5 Planning Notes
- Add typed wrappers for any remaining toolkit handles still accessed via raw `nint` (e.g., placeholder spans, shadows) to keep API parity with upstream interop headers.
- **Task 5.1 – Benchmark strategy**  
  - Establish BenchmarkDotNet harness targeting macOS Metal to compare native Impeller SDK vs. managed wrappers. Scenarios: (a) repeated `SurfaceDrawDisplayList` with 1k rects, (b) texture upload loop, (c) surface present call. Collect average/percentile latency and CPU usage. Use native CLI sample for baseline.  
  - Complement managed benchmarks with existing `//flutter/benchmarking` utilities for correlation; ensure consistent compilation flags and warm-up phases.
- **Task 5.2 – Allocation profiling workflow**  
  - Use `dotnet-trace` and `dotnet-counters` to capture GC/alloc metrics while running benchmarks; automate via scripts that emit JSON summaries.  
  - For per-frame insights, integrate `EventPipeEventSource` to log allocations triggered by texture uploads and command encoding; enforce target < 1KB/frame in hot loops.
- **Task 5.3 – Stress tests**  
  - Texture streaming: repeatedly upload 1024x1024 textures (varying mip counts) and randomly replace contents to probe allocator pressure.  
  - Uniform pressure: build display lists with large gradient/typography data to stress uniform buffers and descriptor bindings.  
  - Shader warm-up: load many fragment programs via `ImpellerFragmentProgramNew`; measure first vs. subsequent draws.  
  - Concurrency check: perform background texture uploads while main thread renders to validate thread-safety and identify potential contention.
- **Task 5.4 – Guardrails**  
  - SafeHandle wrappers already throw on disposed handles; extend with optional strict mode toggles (`ImpellerInteropOptions`) that enable debug assertions, version compatibility checks, and thread affinity validation.  
  - Add diagnostic events for suspicious API usage (e.g., null texture uploads, repeated retention without release); surface via EventSource for CI monitoring.

## Milestone 6 Planning Notes
- **Task 6.1 – CI automation (docs/ci-plan.md)**  
  - Three-stage GitHub Actions pipeline: native build (GN/Ninja, artifact upload), managed build/test (dotnet build/test using downloaded native artifacts), packaging (dotnet pack + publish).  
  - Includes sample YAML snippet and considerations for matrix expansion, caching, and benchmark integration.
- **Task 6.2 – Sample scenes (docs/sample-scenes.md)**  
  - Proposed demos: BasicShapes (`samples/BasicShapes`), TextureGallery, TextLayout with expected APIs, outputs, and CI integration notes (golden images).  
  - Each sample should offer CLI options and headless execution support.
- **Task 6.3 – Developer docs (docs/developer-docs-outline.md & docs/developer-guide.md)**  
  - Outline plus fleshed-out guide covering setup, threading, memory management, shader updates, diagnostics, and troubleshooting, referencing SafeHandle usage and telemetry tooling.
- **Task 6.4 – Compatibility matrix (docs/compatibility-matrix.md)**  
  - Table tracks platform/backend combos, native binaries, target runtimes (CoreCLR, NativeAOT), and support status (Planned/Stretch).  
  - Highlights priority environments (macOS Metal) and stretch goals (Unity).

## Deliverables & Exit Criteria
- README modernization landed; follow-up items: diagrams showing Impeller/Rive layering and CI badges once artwork is ready.

- Reproducible build artifacts for Impeller standalone binaries aligned with selected backends.
- Stable C ABI documented with versioning policy and conformance tests.
- High-performance .NET bindings with benchmarks demonstrating target frame pacing and memory goals.
- End-to-end sample applications and operational documentation ready for early adopters.

## Risks & Mitigations
- **ABI churn**: Track upstream Impeller changes (especially `renderer` and `entity`). Mitigate via generated headers and CI sync checks.  
- **Shader pipeline drift**: Automate `impellerc` usage so shader bundles remain in sync; consider mirroring Flutter’s bundle generation.  
- **Threading mismatches**: Align managed scheduler with Impeller expectations; leverage custom `SynchronizationContext` where needed.  
- **Platform coverage gaps**: Prioritize Metal and Vulkan initially; abstract backend selection to plug in OpenGL ES (if revived) without rewriting interop.
