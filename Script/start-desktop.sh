#!/usr/bin/env sh
set -eu

# 统一定位到仓库根目录，避免从任意位置执行时路径错乱。
SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
ROOT_DIR=$(CDPATH= cd -- "$SCRIPT_DIR/.." && pwd)
PROJECT_FILE="$ROOT_DIR/General.Desktop/General.Desktop.csproj"
MODE="${1:-dev}"
START_URL="${GENERAL_DESKTOP_START_URL:-http://localhost:5677/workspace}"
DIST_DIR="${GENERAL_DESKTOP_DIST_DIR:-$ROOT_DIR/General.Web/apps/web-general/dist}"

echo "启动 General.Desktop 客户端..."

case "$MODE" in
  dist)
    echo "当前模式: dist"
    echo "当前前端目录: $DIST_DIR"
    GENERAL_DESKTOP_DIST_DIR="$DIST_DIR" dotnet run --project "$PROJECT_FILE"
    ;;
  watch)
    echo "当前模式: watch"
    echo "当前开发地址: $START_URL"
    GENERAL_DESKTOP_START_URL="$START_URL" dotnet watch run --project "$PROJECT_FILE"
    ;;
  *)
    echo "当前模式: dev"
    echo "当前开发地址: $START_URL"
    GENERAL_DESKTOP_START_URL="$START_URL" dotnet run --project "$PROJECT_FILE"
    ;;
esac

