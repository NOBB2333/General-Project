#!/usr/bin/env bash
set -euo pipefail

# 统一定位到仓库根目录，避免从任意位置执行时路径错乱。
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PROJECT_FILE="$ROOT_DIR/General.Admin/src/General.Admin.HttpApi.Host/General.Admin.HttpApi.Host.csproj"

echo "启动 General.Admin 后端..."

if [[ "${GENERAL_ADMIN_ISOLATED_HOST:-0}" == "1" ]]; then
  if [[ -z "${GENERAL_ADMIN_URLS:-}" || -z "${GENERAL_ADMIN_CONNECTION_STRING:-}" || -z "${GENERAL_ADMIN_DATA_PROTECTION_KEYRING:-}" ]]; then
    echo "隔离Host启动需提供 GENERAL_ADMIN_URLS、GENERAL_ADMIN_CONNECTION_STRING 和 GENERAL_ADMIN_DATA_PROTECTION_KEYRING。" >&2
    exit 2
  fi

  export ASPNETCORE_URLS="$GENERAL_ADMIN_URLS"
  export Database__Provider="${GENERAL_ADMIN_DB_PROVIDER:-Sqlite}"
  export ConnectionStrings__Default="$GENERAL_ADMIN_CONNECTION_STRING"
  export DataProtection__KeyRingPath="$GENERAL_ADMIN_DATA_PROTECTION_KEYRING"
  exec dotnet run --no-launch-profile --project "$PROJECT_FILE"
fi

export ASPNETCORE_URLS="${GENERAL_ADMIN_URLS:-http://0.0.0.0:5010}"
exec dotnet run --no-launch-profile --project "$PROJECT_FILE"
