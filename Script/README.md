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
- `deploy-to-linux.py`：Linux 部署总入口（`rsync` 增量同步 + 数据库迁移 + 服务重启）

打包后产物：

- `build/backend/app`：后端主服务发布目录
- `build/backend/dbmigrator`：ABP 数据库迁移器发布目录
- `build/backend-linux.zip`：可直接上传到 Linux 的后端压缩包（内含 `backend/` 目录，解压即用）
- `build/backend/migrate-db.sh`：Linux 上执行数据库迁移（支持 SQLite / PostgreSQL / MySQL）
- `build/backend/start-backend.sh`：Linux 启动后端，是否 `nohup` 由你自己控制
- `build/backend/DEPLOY-LINUX.md`：Linux 完整部署说明

## Linux 部署

### 改哪里

部署配置直接写在 `Script/deploy-to-linux.py` 顶部：

- 局域网 / 公网服务器：`Script/deploy-to-linux.py:31`
- 本地与远端同步目录：`Script/deploy-to-linux.py:47`
- 同步排除规则：`Script/deploy-to-linux.py:53`

### 用脚本从 Mac 自动部署

```bash
# 安装依赖（首次）
pip install fabric

# 首次部署（全量同步后端 + 前端 + 迁移 + 启动）
python Script/deploy-to-linux.py --init

# 后续增量发布后端 + 重启（优先使用 rsync）
python Script/deploy-to-linux.py

# 增量发布后端 + 执行迁移 + 重启
python Script/deploy-to-linux.py --migrate

# 仅发布前端
python Script/deploy-to-linux.py --web-only

# 发布后端 + 前端 + 重启
python Script/deploy-to-linux.py --web

# 仅运行迁移
python Script/deploy-to-linux.py --migrate-only

# 仅重启后端
python Script/deploy-to-linux.py --restart

# 公网模式
python Script/deploy-to-linux.py wan
```

### 什么不在部署脚本里管

应用运行时配置继续由项目自身决定，不放在部署脚本里：

- 数据库配置：`General.Admin/src/General.Admin.HttpApi.Host/appsettings/40-connection-strings.jsonc`
- Migrator 数据库配置：`General.Admin/src/General.Admin.DbMigrator/appsettings/20-connection-strings.jsonc`
- 后端 `App` 配置：`General.Admin/src/General.Admin.HttpApi.Host/appsettings/10-app.jsonc`
- 前端 `_app.config.js` / Nginx：继续按你现有方案处理

## 同步策略

- 主路径：`rsync` 增量同步，只传变更文件，并删除远端已不存在的旧文件
- 兜底路径：若本机或服务器缺少 `rsync`，自动回退到 Fabric 差量上传
- 默认排除：`logs/`、`data/`、临时文件，不覆盖服务器上的持久化数据
- 如果服务器上的数据库连接配置和仓库内不同，直接把对应 `appsettings/...jsonc` 加进脚本顶部排除规则即可

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
