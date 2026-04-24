#!/usr/bin/env bash
set -euo pipefail

# 统一输出到根目录 build，下游产物不进 Git。
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BUILD_DIR="$ROOT_DIR/build"
WEB_DIST_DIR="$ROOT_DIR/General.Web/apps/web-general/dist"
BACKEND_PROJECT="$ROOT_DIR/General.Admin/src/General.Admin.HttpApi.Host/General.Admin.HttpApi.Host.csproj"
DBMIGRATOR_PROJECT="$ROOT_DIR/General.Admin/src/General.Admin.DbMigrator/General.Admin.DbMigrator.csproj"
DESKTOP_PROJECT="$ROOT_DIR/General.Desktop/General.Desktop.csproj"
BACKEND_BUILD_DIR="$BUILD_DIR/backend"
BACKEND_PUBLISH_DIR="$BACKEND_BUILD_DIR/app"
DBMIGRATOR_PUBLISH_DIR="$BACKEND_BUILD_DIR/dbmigrator"
BACKEND_ZIP_PATH="$BUILD_DIR/backend-linux.zip"
DESKTOP_BUILD_DIR="$BUILD_DIR/desktop"
DESKTOP_PUBLISH_DIR="$DESKTOP_BUILD_DIR/publish"

PACKAGE_DESKTOP="${PACKAGE_DESKTOP:-1}"

cleanup_macos_metadata() {
  local target_dir="$1"

  [[ -d "$target_dir" ]] || return 0

  find "$target_dir" \
    \( \
      -name '__MACOSX' -o \
      -name '.DS_Store' -o \
      -name '._*' -o \
      -name '.Spotlight-V100' -o \
      -name '.Trashes' \
    \) \
    -exec rm -rf {} + 2>/dev/null || true

  if command -v xattr >/dev/null 2>&1; then
    xattr -cr "$target_dir" 2>/dev/null || true
  fi
}

detect_current_runtime_identifier() {
  local os_name
  local arch_name

  os_name="$(uname -s)"
  arch_name="$(uname -m)"

  case "$os_name" in
    Darwin)
      case "$arch_name" in
        arm64|aarch64) echo "osx-arm64" ;;
        x86_64) echo "osx-x64" ;;
        *) return 1 ;;
      esac
      ;;
    Linux)
      case "$arch_name" in
        x86_64) echo "linux-x64" ;;
        arm64|aarch64) echo "linux-arm64" ;;
        *) return 1 ;;
      esac
      ;;
    *)
      return 1
      ;;
  esac
}

DESKTOP_RUNTIME_IDENTIFIER="${DESKTOP_RUNTIME_IDENTIFIER:-$(detect_current_runtime_identifier || true)}"

write_linux_backend_scripts() {
  cat > "$BACKEND_BUILD_DIR/migrate-db.sh" <<'SCRIPT'
#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DBMIGRATOR_DIR="$ROOT_DIR/dbmigrator"
DATA_DIR="${GENERAL_ADMIN_DATA_DIR:-$ROOT_DIR/data}"
DB_FILE="${GENERAL_ADMIN_DB_FILE:-$DATA_DIR/general-admin.db}"

# 优先使用外部传入的连接串（PostgreSQL 或自定义），否则退回 SQLite
if [[ -n "${GENERAL_ADMIN_CONNECTION_STRING:-}" ]]; then
  export ConnectionStrings__Default="$GENERAL_ADMIN_CONNECTION_STRING"
else
  mkdir -p "$DATA_DIR"
  export ConnectionStrings__Default="Data Source=$DB_FILE"
fi

cd "$DBMIGRATOR_DIR"
exec dotnet General.Admin.DbMigrator.dll
SCRIPT

  cat > "$BACKEND_BUILD_DIR/start-backend.sh" <<'SCRIPT'
#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
APP_DIR="$ROOT_DIR/app"
DATA_DIR="${GENERAL_ADMIN_DATA_DIR:-$ROOT_DIR/data}"
LOG_DIR="${GENERAL_ADMIN_LOG_DIR:-$ROOT_DIR/logs}"
DB_FILE="${GENERAL_ADMIN_DB_FILE:-$DATA_DIR/general-admin.db}"

mkdir -p "$LOG_DIR"

export ASPNETCORE_URLS="${GENERAL_ADMIN_URLS:-http://0.0.0.0:8095}"

# 优先使用外部传入的连接串（PostgreSQL 或自定义），否则退回 SQLite
if [[ -n "${GENERAL_ADMIN_CONNECTION_STRING:-}" ]]; then
  export ConnectionStrings__Default="$GENERAL_ADMIN_CONNECTION_STRING"
else
  mkdir -p "$DATA_DIR"
  export ConnectionStrings__Default="Data Source=$DB_FILE"
fi

if [[ -n "${GENERAL_ADMIN_SELF_URL:-}" ]]; then
  export App__SelfUrl="$GENERAL_ADMIN_SELF_URL"
fi

if [[ -n "${GENERAL_ADMIN_CLIENT_URL:-}" ]]; then
  export App__ClientUrl="$GENERAL_ADMIN_CLIENT_URL"
fi

if [[ -n "${GENERAL_ADMIN_CORS_ORIGINS:-}" ]]; then
  export App__CorsOrigins="$GENERAL_ADMIN_CORS_ORIGINS"
fi

if [[ -n "${GENERAL_ADMIN_AUTHORITY:-}" ]]; then
  export AuthServer__Authority="$GENERAL_ADMIN_AUTHORITY"
fi

cd "$APP_DIR"
exec dotnet General.Admin.HttpApi.Host.dll
SCRIPT

  cat > "$BACKEND_BUILD_DIR/DEPLOY-LINUX.md" <<'SCRIPT'
# Linux 部署说明

## 一、解压

```bash
unzip backend-linux.zip
cd backend
```

## 二、选择数据库

### 方案 A：PostgreSQL（推荐生产）

先在 PostgreSQL 中建库（只需第一次）：

```bash
psql -U postgres -c "CREATE DATABASE general_admin;"
```

然后设置环境变量（在后续所有命令中带上）：

```bash
export GENERAL_ADMIN_CONNECTION_STRING="Host=localhost;Port=5432;Database=general_admin;Username=postgres;Password=你的密码"
```

### 方案 B：SQLite（本地轻量）

不设置 `GENERAL_ADMIN_CONNECTION_STRING`，默认使用 `backend/data/general-admin.db`。

## 三、初始化数据库（首次 / 有新迁移时）

```bash
# PostgreSQL
GENERAL_ADMIN_CONNECTION_STRING="Host=localhost;Port=5432;Database=general_admin;Username=postgres;Password=你的密码" \
  bash ./migrate-db.sh

# SQLite（不带连接串，自动用默认路径）
bash ./migrate-db.sh
```

## 四、启动后端

```bash
# PostgreSQL + 后台运行
GENERAL_ADMIN_CONNECTION_STRING="Host=localhost;Port=5432;Database=general_admin;Username=postgres;Password=你的密码" \
  nohup bash ./start-backend.sh > logs/backend.log 2>&1 &

# SQLite + 后台运行
nohup bash ./start-backend.sh > logs/backend.log 2>&1 &
```

查看日志：

```bash
tail -f logs/backend.log
```

停止服务：

```bash
pkill -f General.Admin.HttpApi.Host.dll
```

## 五、可选环境变量

| 变量 | 说明 | 默认值 |
|------|------|--------|
| `GENERAL_ADMIN_CONNECTION_STRING` | 完整连接串，PG 或自定义，不设则用 SQLite | - |
| `GENERAL_ADMIN_DB_FILE` | SQLite 文件路径（仅 SQLite 模式） | `data/general-admin.db` |
| `GENERAL_ADMIN_DATA_DIR` | SQLite 数据目录（仅 SQLite 模式） | `backend/data` |
| `GENERAL_ADMIN_URLS` | 监听地址 | `http://0.0.0.0:8095` |
| `GENERAL_ADMIN_SELF_URL` | 后端对外公网地址 | - |
| `GENERAL_ADMIN_CLIENT_URL` | 前端地址 | - |
| `GENERAL_ADMIN_CORS_ORIGINS` | 允许跨域来源（逗号分隔） | - |
| `GENERAL_ADMIN_AUTHORITY` | AuthServer 地址 | - |
SCRIPT

  chmod +x \
    "$BACKEND_BUILD_DIR/migrate-db.sh" \
    "$BACKEND_BUILD_DIR/start-backend.sh"
}

create_macos_desktop_app() {
  local project_dir="$1"
  local publish_dir="$2"
  local icon_icns="$project_dir/Assets/app-icon.icns"
  local icon_png="$project_dir/Assets/app-icon.png"
  local icon_ico="$project_dir/Assets/app-icon.ico"
  local app_bundle="$publish_dir/General Desktop.app"
  local contents_dir="$app_bundle/Contents"
  local macos_dir="$contents_dir/MacOS"
  local resources_dir="$contents_dir/Resources"
  local icon_target="$resources_dir/AppIcon.icns"
  local launcher_path="$macos_dir/GeneralDesktop"

  rm -rf "$app_bundle"
  mkdir -p "$macos_dir" "$resources_dir"

  if [[ -f "$icon_icns" ]]; then
    cp "$icon_icns" "$icon_target"
  else
    if ! command -v sips >/dev/null 2>&1; then
      echo "sips is required to prepare macOS icons." >&2
      exit 1
    fi

    if ! command -v python3 >/dev/null 2>&1; then
      echo "python3 is required to build a macOS .icns icon." >&2
      exit 1
    fi

    local icon_work_dir
    icon_work_dir="$(mktemp -d "${TMPDIR:-/tmp}/general-desktop-icon.XXXXXX")"

    cleanup_icon_dir() {
      rm -rf "$icon_work_dir"
    }
    trap cleanup_icon_dir RETURN

    local source_png="$icon_work_dir/source.png"

    if [[ -f "$icon_png" ]]; then
      cp "$icon_png" "$source_png"
    elif [[ -f "$icon_ico" ]]; then
      sips -s format png "$icon_ico" --out "$source_png" >/dev/null
    else
      echo "No icon source found. Expected one of: $icon_icns, $icon_png, $icon_ico" >&2
      exit 1
    fi

    for size in 16 32 64 128 256 512 1024; do
      sips -z "$size" "$size" "$source_png" --out "$icon_work_dir/$size.png" >/dev/null
    done

    python3 - "$icon_work_dir" "$icon_target" <<'PY'
import os
import struct
import sys

work_dir = sys.argv[1]
target = sys.argv[2]
chunks = []

for chunk_type, filename in (
    ("icp4", "16.png"),
    ("icp5", "32.png"),
    ("icp6", "64.png"),
    ("ic07", "128.png"),
    ("ic08", "256.png"),
    ("ic09", "512.png"),
    ("ic10", "1024.png"),
):
    path = os.path.join(work_dir, filename)
    with open(path, "rb") as file:
        data = file.read()
    chunks.append(chunk_type.encode("ascii") + struct.pack(">I", len(data) + 8) + data)

payload = b"".join(chunks)
with open(target, "wb") as file:
    file.write(b"icns" + struct.pack(">I", len(payload) + 8) + payload)
PY
  fi

  cat > "$contents_dir/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleDevelopmentRegion</key>
  <string>zh_CN</string>
  <key>CFBundleDisplayName</key>
  <string>General Desktop</string>
  <key>CFBundleExecutable</key>
  <string>GeneralDesktop</string>
  <key>CFBundleIconFile</key>
  <string>AppIcon</string>
  <key>CFBundleIdentifier</key>
  <string>com.general.desktop</string>
  <key>CFBundleInfoDictionaryVersion</key>
  <string>6.0</string>
  <key>CFBundleName</key>
  <string>General Desktop</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>CFBundleShortVersionString</key>
  <string>1.0.0</string>
  <key>CFBundleVersion</key>
  <string>1</string>
  <key>LSMinimumSystemVersion</key>
  <string>12.0</string>
  <key>NSHighResolutionCapable</key>
  <true/>
</dict>
</plist>
PLIST

  cat > "$launcher_path" <<'LAUNCHER'
#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")/../../.." && pwd)"
export AVALONIA_TELEMETRY_OPTOUT=1

if [[ -x "$ROOT_DIR/General.Desktop" ]]; then
  exec "$ROOT_DIR/General.Desktop"
fi

if [[ -f "$ROOT_DIR/General.Desktop.dll" ]]; then
  exec dotnet "$ROOT_DIR/General.Desktop.dll"
fi

echo "Missing published desktop payload in $ROOT_DIR" >&2
exit 1
LAUNCHER

  chmod +x "$launcher_path"
}

python3 - <<PY
import shutil
from pathlib import Path

for path in (
    Path(r"$BUILD_DIR/web-general"),
    Path(r"$BACKEND_BUILD_DIR"),
):
    shutil.rmtree(path, ignore_errors=True)
PY

mkdir -p "$BUILD_DIR/web-general" "$BACKEND_BUILD_DIR" "$DESKTOP_BUILD_DIR"

cleanup_macos_metadata "$BUILD_DIR"

echo "构建 web-general 前端..."
cd "$ROOT_DIR/General.Web"
pnpm -F @general/web-general run build

if [[ -d "$WEB_DIST_DIR" ]]; then
  cp -R "$WEB_DIST_DIR"/. "$BUILD_DIR/web-general/"
fi
cleanup_macos_metadata "$BUILD_DIR/web-general"

echo "发布 General.Admin 后端..."
AVALONIA_TELEMETRY_OPTOUT=1 dotnet publish "$BACKEND_PROJECT" -c Release /p:UseAppHost=false -o "$BACKEND_PUBLISH_DIR"

echo "发布 General.Admin.DbMigrator..."
AVALONIA_TELEMETRY_OPTOUT=1 dotnet publish "$DBMIGRATOR_PROJECT" -c Release /p:UseAppHost=false -o "$DBMIGRATOR_PUBLISH_DIR"

write_linux_backend_scripts

cleanup_macos_metadata "$BACKEND_BUILD_DIR"

echo "压缩 Linux 后端发布包..."
rm -f "$BACKEND_ZIP_PATH"
(cd "$BUILD_DIR" && zip -Xqr "$BACKEND_ZIP_PATH" backend -x '*/.DS_Store' '*/._*' '*/__MACOSX/*' '__MACOSX/*')

if [[ "$PACKAGE_DESKTOP" == "1" && -f "$DESKTOP_PROJECT" ]]; then
  echo "发布 General.Desktop 桌面端..."
  rm -rf "$DESKTOP_PUBLISH_DIR"
  mkdir -p "$DESKTOP_PUBLISH_DIR"
  desktop_publish_args=(
    "$DESKTOP_PROJECT"
    -c Release
    --self-contained false
    -o "$DESKTOP_PUBLISH_DIR"
  )

  if [[ -n "$DESKTOP_RUNTIME_IDENTIFIER" ]]; then
    desktop_publish_args+=(-r "$DESKTOP_RUNTIME_IDENTIFIER")
  fi

  AVALONIA_TELEMETRY_OPTOUT=1 dotnet publish "${desktop_publish_args[@]}"

  # 将前端构建产物复制到桌面端输出目录，供客户端发布态直接加载。
  mkdir -p "$DESKTOP_PUBLISH_DIR/wwwroot"
  if [[ -d "$WEB_DIST_DIR" ]]; then
    cp -R "$WEB_DIST_DIR"/. "$DESKTOP_PUBLISH_DIR/wwwroot/"
  fi
  cleanup_macos_metadata "$DESKTOP_PUBLISH_DIR"

  if [[ "$(uname -s)" == "Darwin" ]]; then
    create_macos_desktop_app "$ROOT_DIR/General.Desktop" "$DESKTOP_PUBLISH_DIR"
    cleanup_macos_metadata "$DESKTOP_PUBLISH_DIR"
    echo "macOS 应用包已生成: $DESKTOP_PUBLISH_DIR/General Desktop.app"
  fi
fi

echo "打包完成，产物目录：$BUILD_DIR"
