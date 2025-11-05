#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

CONFIGURATION="${CONFIGURATION:-Release}"
OUTPUT="${OUTPUT:-artifacts/nuget}"
PRERELEASE_SUFFIX="${PRERELEASE_SUFFIX:-}"
SKIP_NATIVE_CHECK="${SKIP_NATIVE_CHECK:-}"  # set to 1 to skip
RID_LIST="${RIDS:-}"

args=(
  "--configuration" "${CONFIGURATION}"
  "--output" "${OUTPUT}"
)

if [[ -n "${PRERELEASE_SUFFIX}" ]]; then
  args+=("--prerelease-suffix" "${PRERELEASE_SUFFIX}")
fi

if [[ "${SKIP_NATIVE_CHECK}" == "1" ]]; then
  args+=("--skip-native-check")
fi

if [[ -n "${RID_LIST}" ]]; then
  RID_LIST="${RID_LIST//,/ }"
  for rid in ${RID_LIST}; do
    args+=("--rid" "${rid}")
  done
fi

python3 "${SCRIPT_DIR}/package_all.py" "${args[@]}" "$@"
