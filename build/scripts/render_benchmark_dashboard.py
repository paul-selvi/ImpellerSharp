#!/usr/bin/env python3

"""Aggregate BenchmarkDotNet JSON reports into a concise Markdown dashboard."""

from __future__ import annotations

import json
import math
import pathlib
import statistics
from dataclasses import dataclass
from typing import Iterable, List


ROOT = pathlib.Path(__file__).resolve().parents[2]
DEFAULT_RESULTS = ROOT / "benchmarks" / "ImpellerSharp.Benchmarks" / "bin" / "Release" / "net8.0" / "BenchmarkArtifacts" / "results"
OUTPUT_PATH = ROOT / "artifacts" / "benchmarks" / "dashboard.md"


@dataclass
class BenchmarkRow:
    title: str
    job: str
    mean_ns: float | None
    error_ns: float | None
    status: str

    @property
    def mean_display(self) -> str:
        if self.mean_ns is None or math.isnan(self.mean_ns):
            return "N/A"
        return f"{self.mean_ns/1_000_000:.3f} ms"

    @property
    def error_display(self) -> str:
        if self.error_ns is None or math.isnan(self.error_ns):
            return "N/A"
        return f"±{self.error_ns/1_000_000:.3f} ms"


def load_rows(results_dir: pathlib.Path) -> List[BenchmarkRow]:
    rows: List[BenchmarkRow] = []
    for json_path in sorted(results_dir.glob("*-report-full-compressed.json")):
        data = json.loads(json_path.read_text())
        for bench in data.get("Benchmarks", []):
            stats = bench.get("Statistics")
            mean = stats.get("Mean") if stats else None
            error = stats.get("Error") if stats else None
            status = "OK"
            if stats is None:
                status = "Failed"
            elif math.isnan(stats.get("Mean", float("nan"))):
                status = "Invalid"

            job = bench.get("DisplayInfo", "?")
            rows.append(
                BenchmarkRow(
                    title=f"{bench.get('Type')}.{bench.get('MethodTitle', bench.get('Method'))}",
                    job=job,
                    mean_ns=mean,
                    error_ns=error,
                    status=status,
                )
            )

    return rows


def write_dashboard(rows: Iterable[BenchmarkRow], output_path: pathlib.Path) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)

    lines = ["# ImpellerSharp Benchmark Dashboard", "", "| Benchmark | Mean | Error | Status |", "| --- | --- | --- | --- |"]

    for row in rows:
        lines.append(
            f"| {row.title} | {row.mean_display} | {row.error_display} | {row.status} |"
        )

    output_path.write_text("\n".join(lines) + "\n")


def main() -> None:
    rows = load_rows(DEFAULT_RESULTS)
    write_dashboard(rows, OUTPUT_PATH)
    print(f"[dashboard] Wrote {len(rows)} rows to {OUTPUT_PATH}")


if __name__ == "__main__":
    main()
