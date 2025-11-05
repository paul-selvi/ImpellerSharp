#!/usr/bin/env python3

"""Summarize dotnet-trace GC stats report into a compact JSON file."""

from __future__ import annotations

import json
import pathlib
import sys


def summarize(report_path: pathlib.Path) -> None:
    data = json.loads(report_path.read_text())
    tables = data.get("Tables", [])
    summary = {}

    for table in tables:
        name = table.get("Name")
        rows = table.get("Rows", [])
        if name == "GC Stats" and rows:
            row = rows[0]
            summary["TotalAllocatedMB"] = row.get("Total Allocated (MB)")
            summary["Gen0Collections"] = row.get("# Gen 0 Collections")
            summary["Gen1Collections"] = row.get("# Gen 1 Collections")
            summary["Gen2Collections"] = row.get("# Gen 2 Collections")
            summary["PauseTimeMS"] = row.get("Pause Time (%)")

    output_path = report_path.with_suffix(".summary.json")
    output_path.write_text(json.dumps(summary, indent=2))
    print(f"[trace] GC summary written to {output_path}")


def main() -> None:
    if len(sys.argv) != 2:
        print("Usage: summarize_gc_report.py <report.json>")
        sys.exit(1)

    summarize(pathlib.Path(sys.argv[1]))


if __name__ == "__main__":
    main()
