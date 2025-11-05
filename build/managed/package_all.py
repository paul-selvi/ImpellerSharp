#!/usr/bin/env python3
"""Pack all managed ImpellerSharp NuGet packages with native validation."""

from __future__ import annotations

import argparse
import json
import subprocess
from pathlib import Path
from typing import Iterable

DEFAULT_RIDS = [
    "osx-arm64",
    "linux-x64",
    "win-x64",
]

PROJECTS = [
    "src/ImpellerSharp.Native/ImpellerSharp.Native.csproj",
    "src/ImpellerSharp.Interop/ImpellerSharp.Interop.csproj",
    "src/ImpellerSharp.Avalonia/ImpellerSharp.Avalonia.csproj",
    "src/ImpellerSharp.Avalonia.Mac/ImpellerSharp.Avalonia.Mac.csproj",
    "src/ImpellerSharp.Avalonia.Windows/ImpellerSharp.Avalonia.Windows.csproj",
    "src/ImpellerSharp.Avalonia.Linux/ImpellerSharp.Avalonia.Linux.csproj",
]


def run(cmd: list[str], *, cwd: Path | None = None) -> None:
    display = " ".join(cmd)
    print(f"[package_all] $ {display}")
    completed = subprocess.run(cmd, cwd=cwd)
    if completed.returncode != 0:
        raise RuntimeError(f"Command failed with exit code {completed.returncode}: {display}")


def normalize_configuration(config: str) -> str:
    return "Release" if config.lower() == "release" else "Debug"


def ensure_native_artifacts(
    repo_root: Path,
    rids: Iterable[str],
) -> dict[str, dict[str, list[str]]]:
    """Ensure Impeller artifacts exist for each RID."""
    artifacts_root = repo_root / "artifacts"
    impeller_root = artifacts_root / "native"
    summary: dict[str, dict[str, list[str]]] = {}

    missing: list[str] = []
    for rid in rids:
        summary[rid] = {}
        imp_path = impeller_root / rid / "native"

        imp_files = _file_listing(imp_path)

        if not imp_files:
            missing.append(f"Impeller artifacts missing for {rid} under {imp_path}")
        else:
            summary[rid]["impeller"] = imp_files

    if missing:
        raise RuntimeError("\n".join(missing))

    return summary


def _file_listing(path: Path) -> list[str]:
    if not path.exists():
        return []
    files = [p.name for p in path.iterdir() if p.is_file()]
    return sorted(files)


def git_commit(repo_root: Path) -> str:
    try:
        return (
            subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=repo_root)
            .decode("utf-8")
            .strip()
        )
    except subprocess.CalledProcessError:
        return "unknown"


def dotnet_restore(repo_root: Path) -> None:
    run([
        "dotnet",
        "restore",
        "ImpellerSharp.sln",
    ], cwd=repo_root)


def dotnet_pack(
    repo_root: Path,
    project: str,
    configuration: str,
    output: Path,
    version_suffix: str | None,
) -> None:
    cmd = [
        "dotnet",
        "pack",
        project,
        "-c",
        configuration,
        "-o",
        str(output),
        "--no-restore",
    ]
    if version_suffix:
        cmd.extend(["--version-suffix", version_suffix])
    run(cmd, cwd=repo_root)


def clean_output(output: Path) -> None:
    if output.exists():
        for pkg in output.glob("*.nupkg"):
            pkg.unlink()
    output.mkdir(parents=True, exist_ok=True)


def collect_packages(output: Path) -> list[dict[str, int]]:
    packages = []
    for pkg in sorted(output.glob("*.nupkg")):
        packages.append({
            "file": pkg.name,
            "size_bytes": pkg.stat().st_size,
        })
    return packages


def write_manifest(
    manifest_path: Path,
    configuration: str,
    version_suffix: str | None,
    rids: list[str],
    native_summary: dict[str, dict[str, list[str]]],
    packages: list[dict[str, int]],
    repo_root: Path,
) -> None:
    manifest = {
        "configuration": configuration,
        "version_suffix": version_suffix or "",
        "rids": rids,
        "native_artifacts": native_summary,
        "packages": packages,
        "git_commit": git_commit(repo_root),
    }
    manifest_path.write_text(json.dumps(manifest, indent=2))
    print(f"[package_all] Wrote manifest -> {manifest_path}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Pack ImpellerSharp managed projects.")
    parser.add_argument(
        "--configuration",
        choices=["Debug", "Release", "debug", "release"],
        default="Release",
        help="Build configuration (default Release).",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=Path("artifacts/nuget"),
        help="Output directory for NuGet packages.",
    )
    parser.add_argument(
        "--rid",
        action="append",
        help="Runtime identifier to validate. Repeat for multiple RIDs. Defaults to macOS, Linux, and Windows.",
    )
    parser.add_argument(
        "--prerelease-suffix",
        help="Optional prerelease suffix passed to dotnet pack (--version-suffix).",
    )
    parser.add_argument(
        "--skip-native-check",
        action="store_true",
        help="Skip validation that native artifacts exist before packing.",
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    repo_root = Path(__file__).resolve().parents[2]
    configuration = normalize_configuration(args.configuration)
    output = args.output.resolve()
    rids = args.rid if args.rid else DEFAULT_RIDS

    native_summary: dict[str, dict[str, list[str]]] = {}
    if not args.skip_native_check:
        native_summary = ensure_native_artifacts(repo_root, rids)

    clean_output(output)
    dotnet_restore(repo_root)
    for project in PROJECTS:
        dotnet_pack(repo_root, project, configuration, output, args.prerelease_suffix)

    packages = collect_packages(output)
    manifest_path = output / "manifest.json"
    write_manifest(
        manifest_path,
        configuration,
        args.prerelease_suffix,
        list(rids),
        native_summary,
        packages,
        repo_root,
    )
    print(f"[package_all] Completed packaging. Packages available under {output}")


if __name__ == "__main__":
    main()
