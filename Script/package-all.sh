#!/usr/bin/env bash
set -euo pipefail

# 统一输出到根目录 build，下游产物不进 Git。
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BUILD_DIR="$ROOT_DIR/build"
WEB_DIST_DIR="$ROOT_DIR/General.Web/apps/web-general/dist"
BACKEND_PROJECT="$ROOT_DIR/General.Admin/src/General.Admin.HttpApi.Host/General.Admin.HttpApi.Host.csproj"
DESKTOP_PROJECT="$ROOT_DIR/General.Desktop/General.Desktop.csproj"
DESKTOP_BUILD_DIR="$BUILD_DIR/desktop"
DESKTOP_PUBLISH_DIR="$DESKTOP_BUILD_DIR/publish"

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

mkdir -p "$BUILD_DIR/web-general" "$BUILD_DIR/backend" "$DESKTOP_BUILD_DIR"

echo "构建 web-general 前端..."
cd "$ROOT_DIR/General.Web"
pnpm -F @general/web-general run build:desktop

if [[ -d "$WEB_DIST_DIR" ]]; then
  cp -R "$WEB_DIST_DIR"/. "$BUILD_DIR/web-general/"
fi

echo "发布 General.Admin 后端..."
AVALONIA_TELEMETRY_OPTOUT=1 dotnet publish "$BACKEND_PROJECT" -c Release -o "$BUILD_DIR/backend"

if [[ -f "$DESKTOP_PROJECT" ]]; then
  echo "发布 General.Desktop 桌面端..."
  rm -rf "$DESKTOP_PUBLISH_DIR"
  mkdir -p "$DESKTOP_PUBLISH_DIR"
  AVALONIA_TELEMETRY_OPTOUT=1 dotnet publish "$DESKTOP_PROJECT" -c Release -o "$DESKTOP_PUBLISH_DIR"

  # 将前端构建产物复制到桌面端输出目录，供客户端发布态直接加载。
  mkdir -p "$DESKTOP_PUBLISH_DIR/wwwroot"
  if [[ -d "$WEB_DIST_DIR" ]]; then
    cp -R "$WEB_DIST_DIR"/. "$DESKTOP_PUBLISH_DIR/wwwroot/"
  fi

  if [[ "$(uname -s)" == "Darwin" ]]; then
    create_macos_desktop_app "$ROOT_DIR/General.Desktop" "$DESKTOP_PUBLISH_DIR"
    echo "macOS 应用包已生成: $DESKTOP_PUBLISH_DIR/General Desktop.app"
  fi
fi

echo "打包完成，产物目录：$BUILD_DIR"
