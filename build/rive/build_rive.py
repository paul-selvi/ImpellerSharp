#!/usr/bin/env python3
"""
Build the Rive native runtime/FFI library for a given platform and architecture.

The script mirrors `build/native/build_impeller.py` but targets the Rive C++
runtime checked out under `extern/rive`. It configures a CMake + Ninja build,
copies the resulting artifacts into `artifacts/rive/<rid>/native`, and stages
them under the managed runtime folders so packaging scripts can assemble NuGet
payloads.

Example:
    python3 build/rive/build_rive.py --platform macos --arch arm64 --configuration Release
"""

from __future__ import annotations

import argparse
import json
import os
import platform
import shutil
import subprocess
import sys
from pathlib import Path
from typing import Iterable


def run(cmd: list[str], *, cwd: Path | None = None, env: dict[str, str] | None = None) -> None:
    """Run a command and raise if it fails."""
    display = " ".join(cmd)
    print(f"[build_rive] $ {display}")
    completed = subprocess.run(cmd, cwd=cwd, env=env)
    if completed.returncode != 0:
        raise RuntimeError(f"Command failed with exit code {completed.returncode}: {display}")


def normalize_arch(value: str | None) -> str:
    """Normalize architecture names to canonical identifiers."""
    if value is None or value == "":
        value = platform.machine()
    value = value.lower()
    if value in {"x86_64", "amd64"}:
        return "x64"
    if value in {"aarch64", "arm64"}:
        return "arm64"
    raise ValueError(f"Unsupported architecture '{value}'.")


def rid_for(platform_name: str, arch: str) -> str:
    """Return the .NET runtime identifier for the given OS/arch."""
    if platform_name == "macos":
        return f"osx-{arch}"
    if platform_name == "linux":
        return f"linux-{arch}"
    if platform_name == "windows":
        return f"win-{arch}"
    raise ValueError(f"Unsupported platform '{platform_name}'.")


def default_rive_root(repo_root: Path) -> Path:
    """Return the default Rive submodule root."""
    return repo_root / "extern" / "rive"


def ensure_rive_submodule(repo_root: Path) -> None:
    """Make sure the Rive submodule is initialized."""
    rive_root = default_rive_root(repo_root)
    if rive_root.exists():
        return
    run(
        ["git", "submodule", "update", "--init", "--recursive", "extern/rive"],
        cwd=repo_root,
    )
    if not rive_root.exists():
        raise FileNotFoundError(
            "Unable to locate extern/rive after submodule initialization."
        )


def cmake_generator(platform_name: str) -> str:
    """Return the preferred CMake generator."""
    # Ninja offers a consistent experience across platforms.
    if shutil.which("ninja") is None:
        raise RuntimeError("Ninja build tool not found in PATH.")
    return "Ninja"


def configure_cmake(
    rive_root: Path,
    build_dir: Path,
    platform_name: str,
    arch: str,
    configuration: str,
    extra_cmake_args: Iterable[str],
) -> None:
    """Invoke CMake to generate build files."""
    build_dir.mkdir(parents=True, exist_ok=True)
    generator = cmake_generator(platform_name)

    cmake_args = [
        "cmake",
        "-S",
        str(rive_root),
        "-B",
        str(build_dir),
        "-G",
        generator,
        f"-DCMAKE_BUILD_TYPE={configuration.capitalize()}",
        "-DRIVE_BUILD_TESTS=OFF",
        "-DRIVE_BUILD_EXAMPLES=OFF",
        "-DRIVE_BUILD_TOOLS=OFF",
        "-DRIVE_BUILD_SHARED=ON",
    ]

    if platform_name == "windows":
        if arch == "arm64":
            cmake_args.append("-DCMAKE_SYSTEM_PROCESSOR=ARM64")
        else:
            cmake_args.append("-DCMAKE_SYSTEM_PROCESSOR=x86_64")
    else:
        cmake_args.append(f"-DCMAKE_SYSTEM_PROCESSOR={arch}")

    cmake_args.extend(extra_cmake_args)
    run(cmake_args)


def build_with_cmake(build_dir: Path, configuration: str) -> None:
    """Run cmake --build for the selected configuration."""
    run(
        [
            "cmake",
            "--build",
            str(build_dir),
            "--config",
            configuration.capitalize(),
        ]
    )


def locate_artifacts(build_dir: Path, platform_name: str) -> list[Path]:
    """Return a list of native artifacts produced by the build."""
    candidates: list[str]
    if platform_name == "macos":
        candidates = ["librive_ffi*.dylib", "librive*.dylib"]
    elif platform_name == "linux":
        candidates = ["librive_ffi*.so", "librive*.so"]
    elif platform_name == "windows":
        candidates = ["rive_ffi*.dll", "rive*.dll"]
    else:
        raise ValueError(f"Unsupported platform '{platform_name}'.")

    artifacts: list[Path] = []
    for pattern in candidates:
        artifacts.extend(build_dir.glob(pattern))

    # Fallback to static libraries if shared was not produced.
    if not artifacts:
        fallback = {
            "macos": ["librive*.a"],
            "linux": ["librive*.a"],
            "windows": ["rive*.lib"],
        }[platform_name]
        for pattern in fallback:
            artifacts.extend(build_dir.glob(pattern))

    if not artifacts:
        patterns = ", ".join(candidates)
        raise FileNotFoundError(
            f"No Rive artifacts found in {build_dir}. "
            f"Expected patterns: {patterns}."
        )
    return artifacts


def ensure_stage_dirs(repo_root: Path, rid: str, configuration: str) -> list[Path]:
    """Return managed runtime staging directories (creating them if needed)."""
    managed_projects = [
        repo_root / "src" / "ImpellerSharp.Native",
        repo_root / "src" / "ImpellerSharp.Interop",
    ]
    stage_dirs: list[Path] = []
    config_name = configuration.capitalize()
    for project in managed_projects:
        stage = (
            project
            / "bin"
            / config_name
            / "net8.0"
            / "runtimes"
            / rid
            / "native"
        )
        stage.mkdir(parents=True, exist_ok=True)
        stage_dirs.append(stage)
    return stage_dirs


def copy_artifacts(
    artifacts: list[Path],
    output_dir: Path,
    stage_dirs: list[Path],
) -> list[str]:
    """Copy artifacts into output + staging directories."""
    output_dir.mkdir(parents=True, exist_ok=True)
    copied_files: list[str] = []
    for source in artifacts:
        destinations = [output_dir, *stage_dirs]
        for dest in destinations:
            dest.mkdir(parents=True, exist_ok=True)
            target = dest / source.name
            shutil.copy2(source, target)
            print(f"[build_rive] Copied {source.name} -> {target}")
        copied_files.append(source.name)
    return copied_files


def write_manifest(
    output_dir: Path,
    rid: str,
    configuration: str,
    artifacts: list[str],
    rive_root: Path,
) -> None:
    """Emit a manifest.json describing copied artifacts."""
    sha = "unknown"
    git_dir = rive_root / ".git"
    if git_dir.exists():
        try:
            sha = (
                subprocess.check_output(
                    ["git", "rev-parse", "HEAD"],
                    cwd=rive_root,
                )
                .decode("utf-8")
                .strip()
            )
        except subprocess.CalledProcessError:
            pass

    manifest = {
        "rid": rid,
        "configuration": configuration,
        "artifacts": artifacts,
        "rive_commit": sha,
    }
    manifest_path = output_dir / "manifest.json"
    output_dir.mkdir(parents=True, exist_ok=True)
    manifest_path.write_text(json.dumps(manifest, indent=2))
    print(f"[build_rive] Wrote manifest -> {manifest_path}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build Rive native binaries for ImpellerSharp.")
    parser.add_argument("--platform", choices=["macos", "linux", "windows"], required=True, help="Target platform.")
    parser.add_argument(
        "--arch",
        choices=["x64", "arm64"],
        help="Target architecture (defaults to host architecture).",
    )
    parser.add_argument(
        "--configuration",
        choices=["Debug", "Release", "debug", "release"],
        default="Release",
        help="Build configuration.",
    )
    parser.add_argument(
        "--rive-root",
        type=Path,
        help="Path to the Rive checkout (defaults to extern/rive).",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("artifacts/rive"),
        help="Destination root for packaged artifacts.",
    )
    parser.add_argument(
        "--skip-sync",
        action="store_true",
        help="Skip ensuring the Rive submodule is initialized.",
    )
    parser.add_argument(
        "--cmake-arg",
        action="append",
        default=[],
        help="Additional arguments to pass through to CMake configuration (repeatable).",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    repo_root = Path(__file__).resolve().parents[2]
    rive_root = args.rive_root or default_rive_root(repo_root)

    if not args.skip_sync:
        ensure_rive_submodule(repo_root)

    if not rive_root.exists():
        raise FileNotFoundError(
            f"Rive checkout not found at {rive_root}. "
            "Run 'git submodule update --init --recursive extern/rive'."
        )

    arch = normalize_arch(args.arch)
    platform_name = args.platform
    rid = rid_for(platform_name, arch)
    configuration = "release" if args.configuration.lower() == "release" else "debug"

    print(
        f"[build_rive] Building Rive for platform={platform_name} arch={arch} configuration={configuration}"
    )

    build_dir = (
        rive_root
        / "out"
        / f"{platform_name}-{arch}-{configuration}"
    )

    configure_cmake(
        rive_root,
        build_dir,
        platform_name,
        arch,
        configuration,
        args.cmake_arg,
    )
    build_with_cmake(build_dir, configuration)

    artifacts = locate_artifacts(build_dir, platform_name)
    output_dir = (args.output / rid / "native").resolve()
    stage_dirs = ensure_stage_dirs(repo_root, rid, configuration)
    copied = copy_artifacts(artifacts, output_dir, stage_dirs)
    write_manifest(args.output / rid, rid, configuration, copied, rive_root)

    print(f"[build_rive] Completed build. Artifacts available under {output_dir}")


if __name__ == "__main__":
    main()
