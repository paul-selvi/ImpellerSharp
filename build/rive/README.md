# Rive Native Build Scripts

This directory mirrors `build/native/` but targets the Rive runtime and FFI
artifacts. The primary entry point is `build_rive.py`, which:

- Accepts the same CLI contract as `build/native/build_impeller.py`
  (`--platform`, `--arch`, `--configuration`, `--output`, `--skip-sync`) with an
  extra `--cmake-arg` pass-through for custom flags.
- Ensures the Rive source tree (`extern/rive`) is initialized as a git
  submodule.
- Configures a CMake + Ninja build under
  `extern/rive/out/<platform>-<arch>-<config>` and invokes `cmake --build`.
- Copies the resulting binaries into `artifacts/rive/<rid>/native` and stages
  them into the managed `src/*/bin/.../runtimes/<rid>/native` folders alongside
  a `manifest.json` describing the copied files.

Refer to `docs/rive-build-prereqs.md` for required toolchains, then run:

```bash
python3 build/rive/build_rive.py --platform macos --arch arm64 --configuration Release
```

Shortcut wrappers are available:

```bash
./build/rive/build_rive.sh
```

```powershell
pwsh build/rive/build_rive.ps1
```

Both wrappers honor environment variables (`PLATFORM`, `ARCH`, `CONFIGURATION`)
and forward any additional arguments to the Python script.
