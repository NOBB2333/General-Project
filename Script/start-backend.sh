#!/usr/bin/env bash
set -euo pipefail

# 统一定位到仓库根目录，避免从任意位置执行时路径错乱。
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_FILE="$ROOT_DIR/General.Admin/src/General.Admin.HttpApi.Host/General.Admin.HttpApi.Host.csproj"

echo "启动 General.Admin 后端..."
dotnet run --project "$PROJECT_FILE"
