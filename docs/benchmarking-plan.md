# Managed Impeller Benchmark & Profiling Plan

## Benchmark Scenarios (Task 5.1)

| Scenario | Description | Metrics | Notes |
| --- | --- | --- | --- |
| BL-DrawRect-HotLoop | Build a display list with 1k rects and issue repeated `SurfaceDrawDisplayList` | avg frame submission (ms), stddev, CPU % | Compare managed pipeline vs native `impeller/toolkit` example |
| TextureUpload-ManagedVsNative | Upload 256x256 RGBA textures via managed `TextureFactory` vs native ABI | upload latency (ms), allocations, GC count | Backed by `TextureUploadBenchmark` (BenchmarkDotNet) |
| TextureUpload-256 | Upload 256x256 RGBA texture via `TextureFactory.CreateTexture` | upload latency (ms), GC allocations | Use deterministic data, measure map/unmap |
| SurfacePresent-Metal | Present a Metal surface after display list draw | present latency, success rate | Requires macOS w/Metal backend |

Benchmark harness should use `BenchmarkDotNet` with custom job pinning to avoid interference. Each scenario should have control (native C++ path using `impeller` CLI example) for comparison.

## Allocation Profiling (Task 5.2)

* Run `dotnet-trace collect --providers Microsoft-Windows-DotNETRuntime:0x4c14fccbd` targeting managed sample app to capture allocation metrics while running Scenario 1.
* Shortcut: `build/scripts/collect_allocations.sh` wraps the BenchmarkDotNet harness for `TextureUploadBenchmark` to emit `.nettrace` captures.
* Use `dotnet-counters monitor System.Runtime` to watch `GC Heap Size`, `Allocation Rate` in real-time, ensuring < 1KB/frame in hot loop.
* For finer detail, integrate `EventPipeEventSource` to emit per-frame summaries into logs (scripted).

## Stress Tests (Task 5.3)

1. **Texture streaming**: Upload 100 textures (1024x1024) sequentially, then randomly re-upload different mip levels; watch for failures and time per upload.
2. **Uniform buffer pressure**: Generate display lists with increasingly large uniform data (gradients, color matrices) to ensure bindings stay valid.
3. **Shader warm-up**: Preload multiple fragment programs via `ImpellerFragmentProgramNew` and time first draw vs subsequent draws.
4. **Concurrency**: Issue texture uploads on background tasks while main thread renders to verify `ImpellerContext` thread-safe expectations.

## Guardrails (Task 5.4)

* SafeHandle wrappers guard against disposed usage (already throws `ObjectDisposedException`).
* Add debug assertions (conditional `DEBUG`) to ensure library version negotiation succeeded.
* Provide optional `ImpellerInteropOptions` enabling strict mode: checks for null returns, logs warnings, and ensures callbacks invoked on thread pool.
