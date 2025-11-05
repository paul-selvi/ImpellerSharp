# Rive Native Build Prerequisites

_Last updated: 2025-11-05_

The Rive runtime build mirrors our Impeller workflow but has a few additional
toolchain requirements. Install the following before running
`build/rive/build_rive.py` (or the convenience wrappers `build_rive.sh` / `build_rive.ps1`).

## Common Requirements

- Git 2.40+ (for submodule sync)
- Python 3.9+ (used by helper scripts)
- CMake 3.22+
- Ninja 1.11+
- Protobuf compiler (if the runtime build expects generated assets)
- pkg-config (Linux/macOS) for dependency discovery

> Manage the Rive sources via the `extern/rive` git submodule. After cloning
> ImpellerSharp run `git submodule update --init --recursive extern/rive`.

## macOS (arm64/x64)

- Xcode 15+ with Command Line Tools
- Homebrew packages:
  - `brew install cmake ninja python` (if the system Python lacks required
    modules)
  - `brew install protobuf` (if not already present)
- Optional: `brew install sdl2` if sample viewers are built alongside the
  runtime.

## Linux (x64/arm64)

- GCC 12+ or Clang 15+ toolchain
- `sudo apt-get install build-essential ninja-build cmake python3 python3-pip pkg-config`
- `sudo apt-get install libgl1-mesa-dev libx11-dev libxcursor-dev libxi-dev`
  (only when building sample apps)
- Install `protoc` via your package manager or download from
  https://github.com/protocolbuffers/protobuf/releases.

## Windows (x64)

- Visual Studio 2022 with the following workloads:
  - Desktop development with C++
  - C++ CMake tools for Windows
- Python 3.11 (from python.org or Microsoft Store)
- CMake + Ninja (bundled with VS or install via `choco install cmake ninja`)
- Optional: `choco install protobuf` to obtain `protoc`

## Verification Steps

1. Clone ImpellerSharp with submodules:
   ```bash
   git clone https://github.com/<org>/ImpellerSharp.git
   cd ImpellerSharp
   git submodule update --init --recursive extern/rive
   ```
2. Check that the expected tools are available:
   ```bash
   cmake --version
   ninja --version
   python3 --version
   ```
3. For Windows PowerShell:
   ```powershell
   cmake --version
   ninja --version
   python --version
   ```

Re-run the prerequisite checks whenever the Rive runtime updates its minimum
tool versions. Track changes in this document to keep local environments in
sync with CI.
