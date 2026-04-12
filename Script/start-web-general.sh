#!/usr/bin/env bash
set -euo pipefail

# 统一定位到仓库根目录，避免从任意位置执行时路径错乱。
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "启动 web-general 前端..."
cd "$ROOT_DIR/General.Web"
pnpm run dev:general
