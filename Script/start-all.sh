#!/usr/bin/env bash
set -euo pipefail

# 统一定位到仓库根目录，并在退出时一起回收子进程。
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BACKEND_PID=""
FRONTEND_PID=""

cleanup() {
  if [[ -n "$BACKEND_PID" ]] && kill -0 "$BACKEND_PID" 2>/dev/null; then
    kill "$BACKEND_PID" 2>/dev/null || true
  fi

  if [[ -n "$FRONTEND_PID" ]] && kill -0 "$FRONTEND_PID" 2>/dev/null; then
    kill "$FRONTEND_PID" 2>/dev/null || true
  fi
}

trap cleanup EXIT INT TERM

"$ROOT_DIR/Script/start-backend.sh" &
BACKEND_PID=$!

"$ROOT_DIR/Script/start-web-general.sh" &
FRONTEND_PID=$!

echo "前后端已启动，按 Ctrl+C 一起结束。"
wait "$BACKEND_PID" "$FRONTEND_PID"
