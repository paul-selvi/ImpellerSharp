[![CI](https://github.com/yourname/ImpellerSharp/actions/workflows/build.yml/badge.svg)](https://github.com/yourname/ImpellerSharp/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/ImpellerSharp.Interop.svg)](https://www.nuget.org/packages/ImpellerSharp.Interop/)


# ImpellerSharp

Cross-platform .NET bindings for Flutter’s Impeller renderer with sample host applications and utilities for exploring high-performance, modern GPU rendering from managed code.

> **Project status:** Active R&D. APIs may change while we converge on a stable interop layer and hosting story.

---

## Features

- **SafeHandle-first interop** wrapping Impeller contexts, surfaces, display lists, paints, and typography handles with predictable lifetimes.
- **Backend flexibility** covering Metal, OpenGL(ES), and Vulkan contexts plus Skia lease integrations for Avalonia.
- **Broad FFI surface area** including fragment programs, color/mask/image filters, display-list transforms, and advanced typography metrics.
- **Avalonia integration** via dedicated NuGet packages for macOS (Metal), Windows, and Linux.
- **Sample showcase** ranging from minimal Metal bootstraps to Avalonia UI demos and performance-oriented workloads.
- **Automated build pipeline** that compiles native Impeller artifacts on macOS/Linux/Windows, exercises managed tests, and packages NuGet outputs.

---

## Compatibility Matrix

| Runtime | Native backend | Status | Notes |
| --- | --- | --- | --- |
| `osx-arm64` / `osx-x64` | Metal | ✅ | Primary development platform; BasicShapes sample targets Metal. |
| `win-x64` | Skia lease (Avalonia) / WIP swapchain | ⚠️ | Managed pieces ship today; direct Impeller swapchain hosting in progress. |
| `linux-x64` | Skia lease (Avalonia) / Vulkan | ⚠️ | Vulkan interop bindings implemented; host scaffolding underway. |

> Native binaries are produced per RID using `build/native/build_impeller.py`. Run it once per target OS/architecture.

---

## Quick Start

```bash
git clone https://github.com/yourname/ImpellerSharp.git
cd ImpellerSharp
git submodule update --init --recursive

# Build native Impeller binaries for your platform(s).
python3 build/native/build_impeller.py --platform macos --arch arm64 --configuration Release

# Package managed libraries (validates native assets by default).
python3 build/managed/package_all.py --configuration Release

# Run the BasicShapes sample (macOS).
dotnet run -c Release --project samples/BasicShapes/BasicShapes.csproj
```

Skip the `--platform` lines for operating systems you do not target. Wrapper scripts (`build/native/build_impeller.sh|ps1`, `build/managed/package.sh|ps1`) honor the same arguments via environment variables.

---

## Build Instructions

### Native artifacts

Impeller native components must be staged before running managed projects or packing NuGet packages.

```bash
# Impeller renderer
python3 build/native/build_impeller.py --platform macos --arch arm64 --configuration Release
python3 build/native/build_impeller.py --platform linux --arch x64 --configuration Release
python build/native/build_impeller.py --platform windows --arch x64 --configuration Release

```

Each script:
- Initializes required submodules (`extern/flutter`) if needed.
- Invokes GN/Ninja to build the Impeller host binaries.
- Copies binaries to `artifacts/native/<rid>/native` and stages them under `src/*/bin/<Config>/net8.0/runtimes/<rid>/native`.
- Emits `manifest.json` snapshots describing copied files.

Typical dependencies include Ninja, Python 3, and platform-specific toolchains (Xcode CLI, clang, MSVC).

### Managed packaging

`build/managed/package_all.py` drives managed builds and NuGet packaging:

```bash
python3 build/managed/package_all.py \
  --configuration Release \
  --output artifacts/nuget \
  --rid osx-arm64 --rid linux-x64 --rid win-x64 \
  --prerelease-suffix ci.123
```

The script:
- Verifies native Impeller artifacts exist unless `--skip-native-check` is supplied.
- Runs `dotnet restore` once for the solution.
- Packs `ImpellerSharp.Native`, `ImpellerSharp.Interop`, and Avalonia host packages.
- Writes `artifacts/nuget/manifest.json` summarising packages, native payloads, and git metadata.

Wrapper scripts (`build/managed/package.sh`, `build/managed/package.ps1`) expose the same options via environment variables (`CONFIGURATION`, `OUTPUT`, `RIDS`, `PRERELEASE_SUFFIX`, `SKIP_NATIVE_CHECK`).

### Staging in CI

CI jobs use `build/managed/stage_native_assets.py` to copy downloaded artifacts into managed runtime folders prior to building or packaging. Invoke it locally whenever you grab artifacts from continuous integration:

```bash
python3 build/managed/stage_native_assets.py --configuration Release --artifacts artifacts
```

---

## Samples

| Sample | Location | Notes |
| --- | --- | --- |
| BasicShapes | `samples/BasicShapes` | Minimal Metal bootstrap that encodes a display list. macOS only (for now). |
| AvaloniaImpellerApp | `samples/AvaloniaImpellerApp` | Cross-platform Avalonia UI sample demonstrating Skia lease + Impeller integration. |
| MotionMark / MotionMarkOriginal | `samples/` | Renders MotionMark-inspired scenes; useful for perf validation and stress testing. |

Each sample uses the staged native binaries under `src/ImpellerSharp.Native/bin/<Configuration>/net8.0/runtimes/<rid>/native`. Run them via `dotnet run -c Release --project <path>.csproj`.

---

## CI & Release Automation

| Workflow | Path | Purpose |
| --- | --- | --- |
| Build & Package | `.github/workflows/build.yml` | On pushes/PRs, builds native Impeller matrices, stages assets, runs managed tests, and packages NuGets (uploads manifest + packages for non-PR events). |
| Release | `.github/workflows/release.yml` | Triggered by `v*` tags or manual dispatch; rebuilds native artifacts, reuses packaging pipeline, smoke-tests NuGet outputs, uploads artifacts, and (optionally) publishes to NuGet.org. |

Artifacts:
- `native-*` – per-OS native binaries.
- `nuget-packages` / `nuget-manifest` – managed packages + manifest from CI.
- `release-nuget-packages` + `native-impeller.tar.gz` – release-ready bundles attached to GitHub releases.

---

## Roadmap & References

- `docs/api-summary.md` – managed vs. native coverage map.
- `docs/impeller-interop-plan.md` – long-term interop roadmap.

Near-term goals include:
- Polishing Windows/Linux swapchain hosting.
- Expanding automated sample tests (golden images, CI validation).
- Completing README modernization follow-ups (architecture diagrams).

---

## Troubleshooting

- **Missing native binaries:** Re-run the native build scripts or pull artifacts from CI; ensure `build/managed/stage_native_assets.py` ran before launching samples.
- **`gclient` errors:** Confirm `extern/depot_tools` is synced and added to `PATH`. On macOS/Linux, run `gclient sync` inside `extern/flutter`.
- **NuGet pack fails validation:** Set `SKIP_NATIVE_CHECK=1` temporarily when iterating locally, but restore the default behavior before publishing builds.
- **macOS signing warnings:** Binaries are unsigned development builds; add ad-hoc codesigning (`codesign --force --sign - <dylib>`) if Gatekeeper blocks execution.

---

## Support & Contribution

- File issues or questions in this repository; include platform details and repro steps for build or runtime problems.
- PRs are welcome—sync submodules and stage native assets before running managed tests (`dotnet test ImpellerSharp.sln -c Release --no-build`).
- Join discussions in the issue tracker for roadmap alignment or ideas around sample coverage.

---

## License

This project is licensed under the BSD 3-Clause License. See [`LICENSE`](LICENSE) for details.
