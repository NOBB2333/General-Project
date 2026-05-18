# 后端工程

General Admin 后端基于 ABP 分层结构。

## 分层

- Domain：实体、领域服务、种子数据。
- Application.Contracts：DTO、权限常量、输入输出模型。
- Application：应用服务、查询聚合、权限边界。
- EntityFrameworkCore：数据库映射、迁移、底层修复服务。
- HttpApi.Host：HTTP API、认证、审计中间件、后台服务。

## 权限

权限码集中定义在 `AdminPermissions`。控制器负责声明接口权限，前端菜单和按钮也应绑定同一套权限码。

页面查看和管理操作应分开建模。例如租户页面可有查看权限，新增、编辑、删除由管理权限控制。

## 种子数据

种子数据由 `PlatformDataSeedContributor` 维护。演示账号、组织、角色、菜单、项目、经营数据都在这里补齐。

种子逻辑必须支持已有库修复，不能只在空表时工作。否则全量部署但保留 SQLite 时，演示数据可能不会更新。
