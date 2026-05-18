# 快速开始

本页用于把本地开发环境跑起来。命令默认在仓库根目录执行。

## 后端

```bash
cd General.Admin
dotnet run --project src/General.Admin.DbMigrator
dotnet run --project src/General.Admin.HttpApi.Host
```

默认 SQLite 数据文件由后端配置决定。开发时优先确认 `ConnectionStrings__Default` 和 `Database__Provider` 是否符合预期。

配置文件说明见 [配置文件](./configuration.md)。后端运行配置和迁移器配置是两套目录，数据库连接、JWT、文件存储、限流等不要混在一起改。

## 前端

```bash
cd General.Web
pnpm install
pnpm dev:general
```

如果只启动平台中心：

```bash
cd General.Web
pnpm dev:general-platform
```

## 文档站

```bash
cd General.Web
pnpm dev:general-docs
```

当前文档站目录是 `General.Web/apps/web-general-docs`。不要把内容写到 `General.Web/docs`，那个目录保留为模板参考更合适。

## 默认账号

演示账号由后端种子数据创建。常用账号包括：

- `host.admin`：Host 管理员。
- `tenant.admin`：默认租户管理员。
- `pmo.demo`：PMO 负责人。
- `pm.demo`：项目经理。
- `member.demo`：项目成员。
- `viewer.demo`：经营查看。

默认密码以当前种子代码为准。部署到共享环境后应立即修改。
