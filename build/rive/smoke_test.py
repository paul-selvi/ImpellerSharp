#!/usr/bin/env python3
"""Basic smoke test to verify Rive artifacts load via ctypes."""

from __future__ import annotations

import argparse
import ctypes
import sys
from pathlib import Path


COMMON_SYMBOLS = [
    "rive_init",
    "rive_shutdown",
    "rive_factory",
]


class SmokeTestError(RuntimeError):
    pass


def find_artifact(root: Path, rid: str) -> Path:
    if not root.exists():
        raise SmokeTestError(f"Artifact root {root} does not exist.")

    patterns = {
        "dll": ["*.dll"],
        "dylib": ["*.dylib"],
        "so": ["*.so"],
    }

    for suffix, globs in patterns.items():
        for pattern in globs:
            matches = list(root.glob(pattern))
            if matches:
                return matches[0]
    raise SmokeTestError(f"No native libraries found under {root}.")


def check_symbols(handle: ctypes.CDLL) -> None:
    missing = []
    for symbol in COMMON_SYMBOLS:
        try:
            getattr(handle, symbol)
        except AttributeError:
            missing.append(symbol)
    if missing:
        raise SmokeTestError(f"Missing symbols: {', '.join(missing)}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Validate a built Rive artifact.")
    parser.add_argument("artifact_root", type=Path, help="Path to <rid>/native directory.")
    parser.add_argument("--rid", required=True, help="Runtime identifier (e.g. osx-arm64).")
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    artifact_dir = args.artifact_root
    try:
        library_path = find_artifact(artifact_dir, args.rid)
        handle = ctypes.CDLL(str(library_path))
        check_symbols(handle)
    except SmokeTestError as error:
        print(f"[rive-smoke] FAIL: {error}")
        sys.exit(1)
    except OSError as error:
        print(f"[rive-smoke] FAIL: Unable to load {library_path}: {error}")
        sys.exit(1)

    print(f"[rive-smoke] PASS: {library_path.name} loaded successfully with expected symbols.")


if __name__ == "__main__":
    main()
