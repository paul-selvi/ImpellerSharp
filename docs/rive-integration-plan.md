# Rive Native Integration & Release Automation Plan

_Draft owner: Codex agent • Last updated: 2025-11-05_

This plan tracks the work required to bring the Rive native runtime & FFI layer
into the ImpellerSharp toolchain, automate packaging across platforms, and
polish developer-facing collateral (README, CI, release process). Each
workstream below calls out prerequisites, concrete tasks, and exit criteria so
the effort can be parallelized without losing coverage.

## Goals

- Provide reproducible build scripts that compile the Rive native runtime +
  FFI bridge on macOS, Windows, and Linux, then stage artifacts into the
  managed package layout (`src/*/bin/.../runtimes/<RID>/native`).
- Add managed-side packaging scripts to assemble NuGet artifacts using the new
  native outputs.
- Expand CI to build all native components (Impeller + Rive), run managed tests,
  and publish signed artifacts for validation branches.
- Add a release pipeline that mimics CI packaging and pushes artifacts to NuGet
  and GitHub Releases.
- Refresh the README to present a professional overview, quickstarts, and
  automation guidance.

## Workstream A – Native Rive Build Tooling

### A1. Source onboarding & layout
- [x] Decide on source of truth for Rive native runtime (e.g. submodule under
  `extern/rive` or vendor tarball). Document revision pinning and update policy.
  _Decision_: Track Rive via git submodule at `extern/rive` (see `.gitmodules`);
  pin to tagged `rive-cpp` releases and update the submodule pointer alongside
  changelog entries. Contributors run `git submodule update --init --recursive extern/rive`
  after cloning.
- [x] Mirror Impeller's `build/native` conventions by introducing a
  `build/rive` directory with supporting scripts/data files. _Added_
  `build/rive/README.md` as scaffolding for upcoming scripts.
- [x] Capture build prerequisites per OS (CMake, Ninja, LLVM/Clang, MSVC, SDL if
  required) in a `docs/rive-build-prereqs.md` helper. See
  `docs/rive-build-prereqs.md`.

### A2. Cross-platform build scripts
- [x] Implement `build/rive/build_rive.py` mirroring the structure of
  `build/native/build_impeller.py`. The script now accepts `--platform`,
  `--arch`, `--configuration`, `--output`, `--skip-sync`, and arbitrary
  `--cmake-arg` pass-through flags; it ensures the `extern/rive` submodule is
  initialized, configures CMake + Ninja builds under
  `extern/rive/out/<platform>-<arch>-<config>`, and invokes `cmake --build`.
- [x] When the Rive repo differs per platform (macOS/Linux/Windows), the script
  maps the runtime identifier (`rid`) and locates platform-specific artifacts
  (`*.dylib`, `*.so`, `*.dll`), falling back to static libs if shared builds are
  unavailable.
- [x] Copy successful build outputs into:
  - `artifacts/rive/<rid>/native`
  - `src/ImpellerSharp.Native/bin/<Config>/net8.0/runtimes/<rid>/native`
  - `src/ImpellerSharp.Interop/bin/<Config>/net8.0/runtimes/<rid>/native`
- [x] Emit a manifest JSON alongside artifacts listing configuration, git SHA,
  and copied files (`artifacts/rive/<rid>/manifest.json`) to support signing and
  downstream validation.

### A3. Validation & developer ergonomics
- [x] Add `build/rive/README.md` documenting command examples for each platform
  and referencing the prerequisite checklist (`docs/rive-build-prereqs.md`).
- [x] Provide convenience shell/PowerShell wrappers
  (`build/rive/build_rive.sh`, `build/rive/build_rive.ps1`) that forward to the
  Python entrypoint with sane defaults derived from environment variables.
- [x] Integrate a smoke test script (`build/rive/smoke_test.py`) that loads the
  produced library via `ctypes` and asserts key exports are present, ensuring
  immediate validation post-build.

## Workstream B – Managed Packaging Automation

### B1. Consolidated packaging script
- [x] Create `build/managed/package_all.py` that:
  - Calls `dotnet restore` once for the solution.
  - Verifies that expected native artifacts exist for each RID (Impeller +
    Rive) before packing, with an opt-out `--skip-native-check` switch for
    local iterations.
  - Executes `dotnet pack` for `ImpellerSharp.Native`, `ImpellerSharp.Interop`,
    and the Avalonia platform split packages, emitting to `artifacts/nuget`.
  - Generates an `artifacts/nuget/manifest.json` report capturing package
    metadata, git commit, and native artifacts discovered.
- [x] Support optional parameters for configuration (`--configuration`), output
  directory overrides (`--output`), and prerelease suffix propagation via
  `--prerelease-suffix`.

### B2. Local developer workflows
- [x] Add `build/managed/package.ps1` (Windows) and
  `build/managed/package.sh` (Unix) thin wrappers that forward environment
  variables and CLI arguments to `package_all.py`.
- [x] Update the docs/README to describe how native Impeller and Rive artifacts
  must be staged before running the packaging scripts, highlighting the new
  helpers and manifest output.
- [x] Introduce a `dotnet tool restore` optional step (documented in README) so
  future CLI helpers can be hydrated before packaging.

## Workstream C – Continuous Integration Expansion

### C1. Native build matrix
- [x] Extend `.github/workflows/build.yml` with a `native_rive` job matrix covering
  macOS arm64, Linux x64, and Windows x64 using `build/rive/build_rive.py`.
- [x] Upload Rive artifacts separately (`native-rive-<os>`) while retaining the
  existing Impeller artifacts; pack stage downloads both into `artifacts/native`
  and `artifacts/rive` before packaging.
- [x] Add an Actions cache for `extern/rive/out` keyed by platform/arch to speed
  up repeated CI runs.

### C2. Managed build/test consolidation
- [x] Add a `managed_tests` job that downloads both Impeller and Rive artifacts,
  stages them via `build/managed/stage_native_assets.py`, and runs `dotnet build`
  / `dotnet test` on the solution.
- [x] Upload TRX results (stored under `artifacts/test-results`) as workflow
  artifacts for traceability.
- [x] Ensure the packaging job also stages native assets before invoking
  `build/managed/package_all.py`, keeping NuGet outputs consistent.

### C3. Artifact publication
- [x] Append a packaging stage that invokes `build/managed/package_all.py`,
  uploads NuGet packages, and surfaces the manifest file (`nuget-manifest`) for
  downstream jobs.
- [x] Gate publishing steps so they run only for push/tag events (skipped on
  pull requests).

## Workstream D – Release Pipeline

### D1. Release workflow structure
- [x] Reworked `.github/workflows/release.yml` to build Impeller and Rive native
  artifacts (matrix jobs), stage them via `stage_native_assets.py`, and reuse
  `build/managed/package_all.py` for consistent packaging.
- [x] Publish phase uploads NuGet packages + manifest, archives native bundles,
  pushes to NuGet when `NUGET_API_KEY` is supplied, and attaches assets to the
  GitHub release.
- [x] Added smoke validation prior to publishing: Linux Rive binaries are
  loaded via `smoke_test.py`, and a temporary console app consumes the local
  `ImpellerSharp.Interop` package to ensure restore/build succeed.

### D2. Release collateral
- [x] Enable auto-generated release notes via `softprops/action-gh-release` with
  `generate_release_notes: true`.
- [x] Bundle available symbol files into `artifacts/symbols.zip`, upload them as
  build artifacts, and attach to GitHub releases.

## Workstream F – Tracking & Governance

- [x] Add a checklist issue template (`.github/ISSUE_TEMPLATE/rive-integration.yml`)
  summarizing milestones/tasks from this plan.
- [x] Embed plan status into `docs/README.md` (links to `docs/rive-status.md`) so
  contributors can see progress snapshots.
- [x] Document the need for periodic plan reviews in `docs/rive-status.md` (add
  follow-up action checklist).

## Milestones & Target Sequencing

| Milestone | Target | Key Deliverables |
| --- | --- | --- |
| **M1 – Native Foundations** | Week 1 | Rive repo onboarding, cross-platform build scripts, smoke tests. |
| **M2 – Managed Packaging** | Week 2 | `package_all.py` + wrappers, documentation for local devs. |
| **M3 – CI Coverage** | Week 3 | GitHub Actions matrix covering Impeller + Rive builds, managed tests, artifact uploads. |
| **M4 – Release Automation** | Week 4 | Tag-driven workflow publishing NuGet + GitHub release assets. |
| **M5 – README & Docs** | Week 5 | Professional README refresh, supplemental docs updated, plan review. |

> Dates assume one dedicated engineer; adjust for team size and parallelization.

## Risks & Open Questions

- **Rive licensing & source availability** – confirm redistribution terms and
  whether mirrored binaries can ship in public NuGet packages.
- **Toolchain divergence** – ensure Rive's build system supports the same
  compiler/linker versions as Impeller across all OS targets.
- **Artifact size** – monitor combined native payload to avoid exceeding NuGet
  size constraints; consider splitting packages if necessary.
- **Codesigning** – macOS binaries may require adhoc or developer ID signing
  before distribution; detail process in follow-up docs.

## Next Steps

1. Approve this plan and create tracking issues per workstream/milestone.
2. Land initial scaffolding: `build/rive` directory, stub scripts, documentation.
3. Execute Milestone 1 tasks; update this document with status and lessons
   learned.
4. Iterate through subsequent milestones, ensuring README/CI references stay in
   sync with implementation.
