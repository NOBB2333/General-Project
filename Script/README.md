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



- 开发联调：
    - sh Script/start-web-general.sh
    - sh Script/start-desktop.sh dev
- 桌面壳代码改动少重启：
    - sh Script/start-desktop.sh watch
- 本地 dist 加载：
    - sh Script/start-desktop.sh dist