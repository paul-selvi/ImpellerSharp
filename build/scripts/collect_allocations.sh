#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

CONFIGURATION="${CONFIGURATION:-Release}"
FILTER="${FILTER:-TextureUploadBenchmark.*}"
OUTPUT="${OUTPUT:-${REPO_ROOT}/artifacts/traces/texture-upload.nettrace}"
PROVIDERS="${PROVIDERS:-Microsoft-Windows-DotNETRuntime:0x4c14fccbd:4}"

mkdir -p "$(dirname "${OUTPUT}")"

echo "[trace] collecting allocations -> ${OUTPUT}"

if ! command -v dotnet-trace >/dev/null 2>&1; then
  echo "[trace] installing dotnet-trace global tool"
  dotnet tool install --global dotnet-trace >/dev/null 2>&1 || {
    echo "[trace] failed to install dotnet-trace; skipping trace collection"
    exit 0
  }
  export PATH="$HOME/.dotnet/tools:$PATH"
fi

if ! dotnet-trace collect \
  --format nettrace \
  --providers "${PROVIDERS}" \
  --output "${OUTPUT}" \
  -- \
  dotnet run --project "${REPO_ROOT}/benchmarks/ImpellerSharp.Benchmarks/ImpellerSharp.Benchmarks.csproj" \
    -c "${CONFIGURATION}" -- \
    --filter "${FILTER}"; then
  echo "[trace] collection failed; removing partial trace"
  rm -f "${OUTPUT}"
  exit 0
fi

echo "[trace] trace captured at ${OUTPUT}"

REPORT_JSON="${OUTPUT%.nettrace}-gc.json"
if dotnet-trace report --format json --report GcStats --output "${REPORT_JSON}" --input "${OUTPUT}" >/dev/null 2>&1; then
  python3 "${REPO_ROOT}/build/scripts/summarize_gc_report.py" "${REPORT_JSON}" || true
else
  echo "[trace] failed to generate GC report"
fi
