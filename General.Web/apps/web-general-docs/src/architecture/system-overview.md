# 系统架构

General Admin 是前后端分离的企业管理平台。

## 后端

后端位于 `General.Admin`，基于 .NET ABP。

- `Application.Contracts`：DTO、权限码、接口契约。
- `Application`：应用服务和业务查询。
- `Domain`：领域实体、种子数据、领域规则。
- `EntityFrameworkCore`：DbContext、迁移、数据库适配。
- `HttpApi.Host`：控制器、认证、请求中间件、后台任务。
- `DbMigrator`：数据库迁移和种子执行入口。

## 前端

前端位于 `General.Web`，基于 Vue 3 和 Vben Admin。

- `apps/web-general`：General Admin 主应用。
- `apps/web-general-docs`：General Admin 文档站。
- `packages`：共享能力和基础组件。
- `docs`：上游模板文档副本，不作为当前项目主文档维护。

## 部署

发布脚本位于 `Script`。

- `package-all.sh`：构建后端、前端和迁移器。
- `deploy-to-linux.py`：同步到 Linux 服务器、执行迁移、重启服务。
- `migrate-db.sh`：服务器上执行数据库迁移和种子数据。
