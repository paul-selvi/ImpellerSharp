#!/usr/bin/env python3
"""
Build the Impeller native library for a given platform/architecture combination.

The script wraps Flutter's GN/Ninja build process and copies the resulting
artifacts into the artifacts/native/<rid>/native folder so they can be packed
into the ImpellerSharp.Native NuGet package.

Example:
    python3 build/native/build_impeller.py --platform macos --arch arm64 --configuration Release
"""

from __future__ import annotations

import argparse
import os
import platform
import shutil
import subprocess
import sys
from pathlib import Path


def run(cmd: list[str], *, cwd: Path | None = None, env: dict[str, str] | None = None) -> None:
    """Run a command and raise if it fails."""
    display = " ".join(cmd)
    print(f"[build_impeller] $ {display}")
    completed = subprocess.run(cmd, cwd=cwd, env=env)
    if completed.returncode != 0:
        raise RuntimeError(f"Command failed with exit code {completed.returncode}: {display}")


def normalize_arch(value: str | None) -> str:
    """Normalize architecture names to values expected by GN."""
    if value is None or value == "":
        value = platform.machine()
    value = value.lower()
    if value in {"x86_64", "amd64", "x64"}:
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


def default_flutter_root(repo_root: Path) -> Path:
    """Return the default Flutter engine checkout path."""
    return repo_root / "extern" / "flutter"


def gclient_env(repo_root: Path) -> dict[str, str]:
    """Augment PATH so depot_tools is picked up when running gclient."""
    env = os.environ.copy()
    depot_tools = repo_root / "extern" / "depot_tools"
    path = env.get("PATH", "")
    env["PATH"] = f"{depot_tools}{os.pathsep}{path}"
    return env


def ensure_gclient_config(flutter_root: Path) -> None:
    """Ensure a .gclient config exists so gclient sync succeeds."""
    config_path = flutter_root / ".gclient"
    if config_path.exists():
        return
    try:
        remote_url = (
            subprocess.check_output(
                ["git", "remote", "get-url", "origin"],
                cwd=flutter_root,
            )
            .decode("utf-8")
            .strip()
        )
    except subprocess.CalledProcessError:
        remote_url = "https://github.com/flutter/flutter.git"
    if not remote_url:
        remote_url = "https://github.com/flutter/flutter.git"
    config_contents = f"""solutions = [
  {{
    "custom_deps": {{}},
    "deps_file": "DEPS",
    "managed": False,
    "name": ".",
    "safesync_url": "",
    "url": "{remote_url}",
  }},
]
"""
    config_path.write_text(config_contents)


def run_gclient_sync(flutter_root: Path, repo_root: Path) -> None:
    """Ensure the Flutter checkout is hydrated."""
    env = gclient_env(repo_root)
    if shutil.which("gclient", path=env["PATH"]) is None:
        raise RuntimeError("gclient not found. Ensure depot_tools submodule is present.")
    ensure_gclient_config(flutter_root)
    run(["gclient", "sync"], cwd=flutter_root, env=env)


def run_gn(
    flutter_root: Path,
    platform_name: str,
    arch: str,
    configuration: str,
    env: dict[str, str],
) -> Path:
    """Invoke flutter/tools/gn with the right switches."""
    engine_src = flutter_root / "engine" / "src"
    gn_script = engine_src / "flutter" / "tools" / "gn"
    if not gn_script.exists():
        raise FileNotFoundError(f"GN script not found at {gn_script}. Did you sync the engine?")

    configuration = configuration.lower()
    runtime_mode = "release" if configuration == "release" else "debug"
    target_dir_name = f"impeller_host_{runtime_mode}"

    args = [
        sys.executable,
        str(gn_script),
        "--runtime-mode",
        runtime_mode,
        "--target-dir",
        target_dir_name,
    ]

    if configuration == "debug":
        args.append("--unoptimized")

    if platform_name == "macos":
        args.extend(["--mac", "--mac-cpu", arch])
    elif platform_name == "linux":
        args.extend(["--linux", "--linux-cpu", arch])
    elif platform_name == "windows":
        args.extend(["--windows", "--windows-cpu", arch])
    else:
        raise ValueError(f"Unsupported platform '{platform_name}'.")

    run(args, cwd=engine_src, env=env)
    return engine_src / "out" / target_dir_name


def run_ninja(out_dir: Path, env: dict[str, str]) -> None:
    """Build the Impeller target via ninja."""
    targets = [
        "flutter/impeller/toolkit/interop:library",
    ]
    run(["ninja", "-C", str(out_dir), *targets], cwd=out_dir, env=env)


def copy_artifacts(
    out_dir: Path,
    platform_name: str,
    rid: str,
    output_root: Path,
    repo_root: Path,
    configuration: str,
) -> None:
    """Copy the produced binaries into artifacts/native/<rid>/native."""
    native_dir = output_root / rid / "native"
    stage_dir = (
        repo_root
        / "src"
        / "ImpellerSharp.Native"
        / "bin"
        / configuration.capitalize()
        / "net8.0"
        / "runtimes"
        / rid
        / "native"
    )
    native_dir.mkdir(parents=True, exist_ok=True)
    stage_dir.mkdir(parents=True, exist_ok=True)

    candidates: list[str]
    if platform_name == "macos":
        candidates = ["libimpeller.dylib"]
    elif platform_name == "linux":
        candidates = ["libimpeller.so"]
    elif platform_name == "windows":
        candidates = ["impeller.dll", "impeller.lib", "impeller.pdb"]
    else:
        raise ValueError(f"Unsupported platform '{platform_name}'.")

    copied = False
    for pattern in candidates:
        for file in out_dir.glob(pattern):
            for destination in (native_dir, stage_dir):
                shutil.copy2(file, destination / file.name)
                print(f"[build_impeller] Copied {file.name} -> {destination / file.name}")
            copied = True

    if not copied:
        raise FileNotFoundError(f"No native artifacts matching {candidates} were found in {out_dir}.")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build Impeller native binaries for ImpellerSharp.")
    parser.add_argument("--platform", choices=["macos", "linux", "windows"], required=True, help="Target platform.")
    parser.add_argument(
        "--arch",
        choices=["x64", "arm64"],
        help="Target architecture (defaults to the host machine architecture).",
    )
    parser.add_argument(
        "--configuration",
        choices=["Debug", "Release", "debug", "release"],
        default="Release",
        help="Build configuration (defaults to Release).",
    )
    parser.add_argument(
        "--flutter-root",
        type=Path,
        help="Path to the Flutter engine checkout (defaults to extern/flutter).",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("artifacts/native"),
        help="Destination root for packaged artifacts.",
    )
    parser.add_argument(
        "--skip-sync",
        action="store_true",
        help="Skip running gclient sync before building.",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    repo_root = Path(__file__).resolve().parents[2]
    flutter_root = args.flutter_root or default_flutter_root(repo_root)

    if not flutter_root.exists():
        raise FileNotFoundError(f"Flutter checkout not found at {flutter_root}.")

    arch = normalize_arch(args.arch)
    platform_name = args.platform
    rid = rid_for(platform_name, arch)
    configuration = "release" if args.configuration.lower() == "release" else "debug"

    print(f"[build_impeller] Building Impeller for platform={platform_name} arch={arch} configuration={configuration}")

    env = gclient_env(repo_root)

    if not args.skip_sync:
        run_gclient_sync(flutter_root, repo_root)

    out_dir = run_gn(flutter_root, platform_name, arch, configuration, env)
    run_ninja(out_dir, env)
    copy_artifacts(out_dir, platform_name, rid, args.output.resolve(), repo_root, configuration)

    print(f"[build_impeller] Completed build. Artifacts available under {args.output.resolve() / rid}")


if __name__ == "__main__":
    main()
