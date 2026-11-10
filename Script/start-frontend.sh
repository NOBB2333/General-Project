#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if ! command -v node >/dev/null 2>&1; then
  for candidate in "$HOME"/.local/share/mise/installs/node/*/bin; do
    if [[ -x "$candidate/node" ]]; then
      export PATH="$candidate:$PATH"
      break
    fi
  done
fi

export HOST="${GENERAL_FRONTEND_HOST:-0.0.0.0}"
export PORT="${GENERAL_FRONTEND_PORT:-5011}"
export FRONTEND_DIR="${GENERAL_FRONTEND_DIR:-$ROOT_DIR/web-general}"
export API_TARGET="${GENERAL_API_TARGET:-http://127.0.0.1:5010}"

exec node "$ROOT_DIR/frontend-server.mjs"
