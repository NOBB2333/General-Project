# Script

这里统一放项目级脚本，方便从仓库根目录直接启动和打包。

当前脚本：

- `start-backend.sh`：启动 `General.Admin` 后端
- `start-web-general.sh`：启动 `General.Web` 下的 `web-general`
- `start-desktop.sh`：启动 `General.Desktop` 客户端
  - `sh Script/start-desktop.sh dev`：桌面端走前端 dev 地址
  - `sh Script/start-desktop.sh watch`：桌面端走前端 dev 地址，并启用 `dotnet watch`
  - `sh Script/start-desktop.sh dist`：桌面端直接加载本地 `dist`
- `start-all.sh`：同时启动前后端
- `package-all.sh`：统一构建前端，并发布后端和桌面端
- `deploy-to-linux.py`：后端部署脚本（Mac → Linux SSH，差量上传 + PG 迁移）

打包后产物：

- `build/backend/app`：后端主服务发布目录
- `build/backend/dbmigrator`：ABP 数据库迁移器发布目录
- `build/backend-linux.zip`：可直接上传到 Linux 的后端压缩包（内含 `backend/` 目录，解压即用）
- `build/backend/migrate-db.sh`：Linux 上执行数据库迁移（支持 PG / SQLite，见下方说明）
- `build/backend/start-backend.sh`：Linux 启动后端，是否 `nohup` 由你自己控制
- `build/backend/DEPLOY-LINUX.md`：Linux 完整部署说明

## Linux 部署（PostgreSQL）

### 首次手动部署

```bash
# 1. 服务器上解压
unzip backend-linux.zip && cd backend

# 2. 建库（只需一次）
psql -U postgres -c "CREATE DATABASE general_admin;"

# 3. 初始化迁移
GENERAL_ADMIN_CONNECTION_STRING="Host=localhost;Port=5432;Database=general_admin;Username=postgres;Password=你的密码" \
  bash ./migrate-db.sh

# 4. 后台启动
GENERAL_ADMIN_CONNECTION_STRING="Host=localhost;Port=5432;Database=general_admin;Username=postgres;Password=你的密码" \
  nohup bash ./start-backend.sh > logs/backend.log 2>&1 &

# 查看日志
tail -f logs/backend.log
```

### 用脚本从 Mac 自动部署

```bash
# 安装依赖（首次）
pip install fabric

# 首次部署（建库 + 迁移 + 启动）
python Script/deploy-to-linux.py --init

# 后续差量上传 + 重启（文件大小比对）
python Script/deploy-to-linux.py

# 仅运行迁移（有新版本时）
python Script/deploy-to-linux.py --migrate

# 公网模式
python Script/deploy-to-linux.py wan
```

> 使用前请修改 `Script/deploy-to-linux.py` 顶部配置区的服务器地址、密码和 `PG_CONNECTION_STRING`。

## 桌面端跨平台打包

默认打当前机器对应平台。如需指定其他平台：

```bash
DESKTOP_RUNTIME_IDENTIFIER=linux-x64 sh Script/package-all.sh
DESKTOP_RUNTIME_IDENTIFIER=osx-arm64 sh Script/package-all.sh
DESKTOP_RUNTIME_IDENTIFIER=osx-x64   sh Script/package-all.sh
DESKTOP_RUNTIME_IDENTIFIER=linux-arm64 sh Script/package-all.sh
# 仅打后端，跳过桌面端
PACKAGE_DESKTOP=0 sh Script/package-all.sh
```

## 日常开发

- 开发联调：`sh Script/start-web-general.sh` + `sh Script/start-desktop.sh dev`
- 桌面壳代码改动少重启：`sh Script/start-desktop.sh watch`
- 本地 dist 加载：`sh Script/start-desktop.sh dist`
