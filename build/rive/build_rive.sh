#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

CONFIGURATION="${CONFIGURATION:-Release}"
PLATFORM="${PLATFORM:-macos}"
ARCH="${ARCH:-$(uname -m)}"

python3 "${SCRIPT_DIR}/build_rive.py" \
  --platform "${PLATFORM}" \
  --arch "${ARCH}" \
  --configuration "${CONFIGURATION}" \
  "$@"
