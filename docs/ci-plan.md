# CI Automation & Packaging Plan

## 6.1 Native + Managed Build Automation

### Pipeline Layout

1. **Native Build Stage (macOS & Linux)**
   - Checkout repo, run `gclient sync`.
   - Execute GN/Ninja build for Impeller targets (`engine/src/out/impeller_host_debug`).
   - Archive generated Impeller SDK artifacts (`impeller.h`, metallibs, dylibs).
   - Upload build outputs as pipeline artifacts.

2. **Managed Build/Test Stage (Windows/macOS/Linux)**
   - Restore .NET SDK (>= 8.0), run `dotnet build` for `ImpellerSharp.Interop`.
   - Execute unit tests/benchmarks gating using `dotnet test`/`BenchmarkDotNet`.
   - Consume native artifacts from Stage 1 (download and place on `PATH`).

3. **Packaging Stage**
   - Produce NuGet package with managed binaries + native assets (RID-specific).
   - Publish to internal feed (GitHub Packages/Azure Artifacts) using token.

### Recommended GitHub Actions Outline
```yaml
name: Impeller CI

on:
  push:
    branches: [ main ]
  pull_request:

jobs:
  native-build:
    runs-on: macos-14
    steps:
      - uses: actions/checkout@v4
      - name: Install dependencies
        run: brew install ninja
      - name: Sync dependencies
        run: gclient sync
      - name: Build Impeller
        run: ./engine/src/flutter/tools/gn --unoptimized --runtime-mode debug --mac --mac-cpu arm64 --target-dir impeller_host_debug && ninja -C engine/src/out/impeller_host_debug flutter/impeller:impeller
      - name: Upload native artifacts
        uses: actions/upload-artifact@v4
        with:
          name: impeller-native-macos-arm64
          path: |
            engine/src/out/impeller_host_debug/libimpeller.dylib
            engine/src/out/impeller_host_debug/impeller.h

  managed-build:
    runs-on: ubuntu-22.04
    needs: native-build
    steps:
      - uses: actions/checkout@v4
      - uses: actions/download-artifact@v4
        with:
          name: impeller-native-macos-arm64
          path: native
      - name: Install .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Build managed library
        run: dotnet build src/ImpellerSharp.Interop -c Release
      - name: Run tests
        run: dotnet test tests/ImpellerSharp.Tests -c Release

  package:
    runs-on: ubuntu-22.04
    needs: managed-build
    steps:
      - uses: actions/checkout@v4
      - name: Pack NuGet
        run: dotnet pack src/ImpellerSharp.Interop -c Release -o artifacts
      - name: Publish package
        if: github.event_name == 'push'
        run: dotnet nuget push artifacts/*.nupkg --source ${{ secrets.NUGET_FEED }} --api-key ${{ secrets.NUGET_API_KEY }}
```

### Considerations

- Add matrix builds for Windows/Linux RIDs once native builds exist for those platforms.
- Integrate BenchmarkDotNet job to gather perf numbers post-build.
- Cache `gclient` downloads using Actions cache to reduce sync times.
