#!/usr/bin/env python3
"""
Generate a coverage report that compares Impeller's C ABI to the managed
P/Invoke bindings and writes the results to docs/api-summary.md.
"""

from __future__ import annotations

import argparse
import datetime as _dt
import sys
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable, Set
import re


@dataclass(frozen=True)
class CoverageReport:
    native_exports: Set[str]
    managed_exports: Set[str]

    @property
    def shared_exports(self) -> Set[str]:
        return self.native_exports & self.managed_exports

    @property
    def missing_managed(self) -> Set[str]:
        return self.native_exports - self.managed_exports

    @property
    def extra_managed(self) -> Set[str]:
        return self.managed_exports - self.native_exports

    @property
    def coverage_percentage(self) -> float:
        if not self.native_exports:
            return 100.0
        return (len(self.shared_exports) / len(self.native_exports)) * 100.0


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Generate Impeller interop API coverage report."
    )
    root = Path(__file__).resolve().parents[2]
    default_header = root / "extern" / "flutter" / "engine" / "src" / "flutter" / "impeller" / "toolkit" / "interop" / "impeller.h"
    default_managed = root / "src" / "ImpellerSharp.Interop"
    default_output = root / "docs" / "api-summary.md"

    parser.add_argument(
        "--header",
        type=Path,
        default=default_header,
        help=f"Path to impeller.h (default: {default_header})",
    )
    parser.add_argument(
        "--interop-dir",
        type=Path,
        default=default_managed,
        help=f"Directory containing managed interop bindings (default: {default_managed})",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=default_output,
        help=f"Output markdown file (default: {default_output})",
    )
    parser.add_argument(
        "--check",
        action="store_true",
        help="Check if the output file is up-to-date without writing changes.",
    )
    args = parser.parse_args()
    setattr(args, "repo_root", root)
    return args


def load_native_exports(header_path: Path) -> Set[str]:
    text = header_path.read_text()
    pattern = re.compile(
        r"IMPELLER_EXPORT(?:\s+IMPELLER_NODISCARD)?\s+(?:[A-Za-z0-9_\s\*]+?)\s+(Impeller[A-Za-z0-9_]+)\s*\(",
        re.MULTILINE,
    )
    exports = {
        match.group(1)
        for match in pattern.finditer(text)
        if not match.group(1).startswith("__")
    }
    return exports


def load_managed_exports(interop_dir: Path) -> Set[str]:
    pattern = re.compile(
        r"(?:LibraryImport|DllImport)\([^)]*?EntryPoint\s*=\s*\"([^\"]+)\"",
        re.DOTALL,
    )
    exports: Set[str] = set()
    for path in interop_dir.rglob("*.cs"):
        text = path.read_text()
        for entry in pattern.findall(text):
            if entry.startswith("Impeller"):
                exports.add(entry)
    return exports


def build_report(args: argparse.Namespace) -> CoverageReport:
    native = load_native_exports(args.header)
    managed = load_managed_exports(args.interop_dir)
    return CoverageReport(native_exports=native, managed_exports=managed)


def format_section(title: str, items: Iterable[str]) -> str:
    sorted_items = sorted(items)
    if not sorted_items:
        return f"## {title}\n\n_None_\n"
    joined = "\n".join(f"- `{item}`" for item in sorted_items)
    return f"## {title}\n\n{joined}\n"


def _path_for_display(path: Path, repo_root: Path) -> str:
    try:
        return path.resolve().relative_to(repo_root.resolve()).as_posix()
    except ValueError:
        return path.resolve().as_posix()


def render_markdown(
    report: CoverageReport, header_path: Path, interop_dir: Path, repo_root: Path
) -> str:
    timestamp = _dt.datetime.utcnow().replace(microsecond=0).isoformat() + "Z"
    rows = [
        ("Native exports in header", str(len(report.native_exports))),
        ("Managed P/Invoke entry points", str(len(report.managed_exports))),
        ("Shared exports (bound)", str(len(report.shared_exports))),
        ("Missing managed bindings", str(len(report.missing_managed))),
        ("Extra managed bindings", str(len(report.extra_managed))),
        (
            "Coverage",
            f"{report.coverage_percentage:.2f}%",
        ),
    ]
    header_rel = _path_for_display(header_path, repo_root)
    interop_rel = _path_for_display(interop_dir, repo_root)
    table_lines = ["| Metric | Count |", "| --- | --- |"] + [
        f"| {name} | {value} |" for name, value in rows
    ]
    body = "\n".join(table_lines)

    sections = [
        format_section("Missing Managed Bindings", report.missing_managed),
        format_section("Extra Managed Bindings (no native export)", report.extra_managed),
    ]

    return (
        "# Impeller Interop API Coverage\n\n"
        f"_Generated on {timestamp} by `build/scripts/generate_api_summary.py`._\n\n"
        f"- Header inspected: `{header_rel}`\n"
        f"- Managed bindings: `{interop_rel}`\n\n"
        f"{body}\n\n"
        + "\n".join(sections)
    )


def write_output(path: Path, content: str, check_only: bool) -> None:
    existing = path.read_text() if path.exists() else None

    if check_only:
        if existing != content:
            print(
                f"{path} is out of date. Run generate_api_summary.py to refresh.",
                file=sys.stderr,
            )
            sys.exit(1)
        print(f"{path} is up to date.")
        return

    if existing == content:
        print(f"{path} is already up to date. No changes written.")
        return

    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content)
    print(f"Wrote coverage report to {path}")


def main() -> None:
    args = parse_args()
    if not args.header.exists():
        raise FileNotFoundError(f"Header not found: {args.header}")
    if not args.interop_dir.exists():
        raise FileNotFoundError(f"Interop directory not found: {args.interop_dir}")

    report = build_report(args)
    markdown = render_markdown(report, args.header, args.interop_dir, args.repo_root)
    write_output(args.output, markdown, args.check)


if __name__ == "__main__":
    main()
