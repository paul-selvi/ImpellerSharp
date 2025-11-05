#!/usr/bin/env python3
"""Copy downloaded native artifacts into managed runtime staging folders."""

from __future__ import annotations

import argparse
import shutil
from pathlib import Path

MANAGED_PROJECTS = [
    Path('src/ImpellerSharp.Native'),
    Path('src/ImpellerSharp.Interop'),
]


def copy_tree(src: Path, dest: Path) -> list[str]:
    copied: list[str] = []
    if not src.exists():
        return copied
    dest.mkdir(parents=True, exist_ok=True)
    for item in src.iterdir():
        if item.is_file():
            target = dest / item.name
            shutil.copy2(item, target)
            copied.append(str(target))
    return copied


def stage_artifacts(artifacts_root: Path, configuration: str) -> None:
    native_root = artifacts_root / 'native'

    if not native_root.exists():
        raise RuntimeError(f'Impeller artifacts not found at {native_root}.')

    staged = 0

    for rid_dir in native_root.iterdir():
        if not rid_dir.is_dir():
            continue
        rid = rid_dir.name
        native_dir = rid_dir / 'native'
        if not native_dir.exists():
            continue

        for project in MANAGED_PROJECTS:
            dest = project / 'bin' / configuration / 'net8.0' / 'runtimes' / rid / 'native'
            copied = copy_tree(native_dir, dest)
            staged += len(copied)

    if staged == 0:
        raise RuntimeError('No artifacts were staged. Ensure artifacts/native is populated.')


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description='Stage native artifacts for managed builds.')
    parser.add_argument(
        '--configuration',
        choices=['Debug', 'Release', 'debug', 'release'],
        default='Release',
        help='Build configuration used for staging (default Release).',
    )
    parser.add_argument(
        '--artifacts',
        type=Path,
        default=Path('artifacts'),
        help='Root directory containing native artifacts.',
    )
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    configuration = 'Release' if args.configuration.lower() == 'release' else 'Debug'
    repo_root = Path(__file__).resolve().parents[2]
    artifacts_root = (repo_root / args.artifacts).resolve()
    stage_artifacts(artifacts_root, configuration)
    print(f'[stage_native_assets] Staged artifacts for configuration {configuration}.')


if __name__ == '__main__':
    main()
