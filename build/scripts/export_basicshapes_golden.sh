#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

OUTPUT_DIR="${1:-${REPO_ROOT}/artifacts/golden/basicshapes}"
BACKEND="${BACKEND:-auto}"
CONFIGURATION="${CONFIGURATION:-Release}"

SCENES=(rects stream typography)

mkdir -p "${OUTPUT_DIR}"

for scene in "${SCENES[@]}"; do
  OUTPUT_PATH="${OUTPUT_DIR}/${scene}-${BACKEND}.json"
  echo "[golden] scene=${scene} backend=${BACKEND} output=${OUTPUT_PATH}"
  if ! dotnet run \
    --project "${REPO_ROOT}/samples/BasicShapes/BasicShapes.csproj" \
    -c "${CONFIGURATION}" -- \
    --headless \
    --backend "${BACKEND}" \
    --scene "${scene}" \
    --frames 1 \
    --output "${OUTPUT_PATH}"; then
    echo "[golden] warning: scene=${scene} backend=${BACKEND} failed (likely missing backend)." >&2
    rm -f "${OUTPUT_PATH}"
  fi
done

echo "[golden] artifacts written to ${OUTPUT_DIR}"
